package main

import (
	"bufio"
	"context"
	"flag"
	"fmt"
	"io"
	"log"
	"net"
	"os"
	"os/exec"
	"os/signal"
	"runtime"
	"strconv"
	"strings"
	"sync/atomic"
	"syscall"
	"time"

	pb "gss/gameprompt/api"
	"gss/gameprompt/common"
	"gss/gameprompt/coordination"
	"gss/gameprompt/invocation"
	"gss/gameprompt/metrics"

	"google.golang.org/grpc/health"
	healthpb "google.golang.org/grpc/health/grpc_health_v1"

	"github.com/dustin/go-humanize"
	"google.golang.org/grpc"
)

/*
  ____                      ____                            _
 / ___| __ _ _ __ ___   ___|  _ \ _ __ ___  _ __ ___  _ __ | |_
| |  _ / _' | '_ ' _ \ / _ \ |_) | '__/ _ \| '_ ' _ \| '_ \| __|
| |_| | (_| | | | | | |  __/  __/| | | (_) | | | | | | |_) | |_
 \____|\__,_|_| |_| |_|\___|_|   |_|  \___/|_| |_| |_| .__/ \__|
             Your way to lifelike NPCs through LLMs! |_|

Program:	GamePrompt Server v1.1
Author:		Sebastian Weigand <tdg@google.com>
Copyright:	2023, Google LLC
License:	Apache License Version 2.0, January 2004

*/

// Types ======================================================================

type grpc_server struct {
	pb.UnimplementedConverseServer
}

// Globals ====================================================================

// General version information for GamePrompt:
var gameprompt_info = "gameprompt-server_v1.1:" + runtime.GOOS + ":llama-cpp_primary-ec2a24f"
var sigc = make(chan os.Signal, 1)                           // For interrups and signals
var gpc_ctx context.Context                                  // With GamePrompt-centric params
var gpc *common.GamePromptConfig                             // Primary parameter list
var max_character_cache_age = time.Duration(time.Minute * 5) // Maximum age to wait to refresh character information from Redis

// Stats:
var connection_times = make(map[string]time.Time)
var total_question_count uint32 = 0
var connection_count int32 = 0
var startup_time float64 = 0.0

// Server-Specific Flags ======================================================

var port = flag.Int("port", 50005, "The port for GamePrompt server to listen on.")
var warmup = flag.Bool("warmup", true, "Prewarm model into virtual memory?")
var use_redis = flag.Bool("use_redis", true, "Register with Redis?")
var use_bigquery = flag.Bool("use_bigquery", true, "Send metrics to BigQuery?")
var use_vertexai = flag.Bool("use_vertexai", false, "Use Vertex AI's Bison model?")
var use_mock = flag.Bool("use_mock", false, "Respond with fake answers? (skips model invocation)")
var thread_count = flag.Int("threads", runtime.NumCPU(), "Number of threads to use per invocation (default is CPU_COUNT).")
var gpu_layers = flag.Int("gpu_layers", 0, "Number of layers to offload to the GPU.")
var max_connections = flag.Int("max_connections", 2, "Maximum number of simultaneous connections to accept.")

// Local Functions ============================================================

// cache_file_into_memory uses `vmtouch` to read a file into virtual memory, which allows for faster invocation once warmed,
// and does not require 2x the memory of a ramdisk (which would still require reading from the ramdisk into memory):
func cache_file_into_memory() {
	fi, err := os.Stat(gpc.ModelPath)
	if err != nil {
		log.Fatalf("[Warmup] failed to read model file: %v", err)
	}
	size := humanize.Bytes(uint64(fi.Size()))
	log.Printf("GamePrompt Initialization: Copying %s to virtual memory...", size)

	start := time.Now()
	cmd := exec.Command(gpc.VmtouchPath, []string{"-t", gpc.ModelPath}...)
	result, err := cmd.CombinedOutput()
	if err != nil {
		log.Print(result)
		log.Fatalf("[Warmup]failed to vmtouch: %v", err)
	}
	duration := time.Since(start)

	startup_time = duration.Seconds()

	log.Printf("[Warmup] Finished: Copied %s in %v.", size, duration)
}

// prune_connections ensures that, in the very unlikely event that a client
// disconnects after calling AreYouBusy(), but before calling AskQuestion(),
// we clean up the connection count to ensure this instance is marked as
// available for other clients:
func prune_connections() {
	for {
		// This should be set to roughly > average deadlock time:
		time.Sleep(time.Minute * 2)

		live_connection_count := int(atomic.LoadInt32(&connection_count))
		live_connection_times := len(connection_times)

		// This is a bonkers possibility:
		if live_connection_count < 0 {
			log.Printf("[Coordination.ConnTrack] WARNING: Connection count was somehow negative [%d], resetting to 0...", live_connection_count)
			atomic.StoreInt32(&connection_count, 0)
		}

		// We've noticed a potential problem, but are not taking action just yet:
		if live_connection_count != live_connection_times {
			log.Printf("[Coordination.ConnTrack] WARNING: Mismatched connection count [%d] and connection timestamp entry count [%d].", live_connection_count, live_connection_times)
		}

		if len(connection_times) > 0 {
			for id, timestamp := range connection_times {
				// We are taking action, as something unexpected happened:
				if time.Since(timestamp) > time.Duration(time.Second*90) {
					log.Printf("[Coordination.ConnTrack] WARNING: Pruned [%s] as it was created at [%v], which is too long ago.", id, timestamp)
					delete(connection_times, id)
				}
			}
		}

		log.Println("[Coordination.ConnTrack] Check run.")
	}
}

// gRPC Functions =============================================================

// AreYouBusy determines if the server should accept a new connection, or if it
// has already reached maximum capacity. The client calls this function first,
// and if the server is not busy, then calls AskQuestion().
func (s grpc_server) AreYouBusy(ctx context.Context, id *pb.AreYouBusyQuestion) (*pb.AreYouBusyAnswer, error) {
	// Increment our connection count as we anticipate a forthcoming question:
	atomic.AddInt32(&connection_count, 1)

	// Check total connection counts:
	busy := false
	if atomic.LoadInt32(&connection_count) > int32(*max_connections) {
		busy = true
	}
	status := &pb.AreYouBusyAnswer{Busy: busy}

	// If we're busy, decrement the count as we will not respond to a question
	// from that client:
	if busy {
		atomic.AddInt32(&connection_count, -1)
	} else {
		// Log connection UUID:
		connection_times[id.GetConnId()] = time.Now().UTC()
	}

	return status, nil
}

// AskQuestion parses questions received via gRPC and responds accordingly:
func (s grpc_server) AskQuestion(in *pb.Question, srv pb.Converse_AskQuestionServer) error {

	// Keep track of each question uniquely (for analytics):
	question_id := in.GetConnId()

	// Increment the number of questions we've received:
	atomic.AddUint32(&total_question_count, 1)

	// Count the number of connections this instance of the GamePrompt server is responding to:
	defer atomic.AddInt32(&connection_count, -1)
	defer delete(connection_times, question_id)

	// This bool does not need to be atomic, as we determine if we're busy per-invocation:
	busy := false
	if atomic.LoadInt32(&connection_count) > 2 {
		busy = true
	}
	log.Printf("[Coordination.ConnTrack] We are at [%d] concurrent connections.", connection_count)

	// This is _highly_ unlikely, but there is a nonzero chance we might get
	// a question from a client which does not respect the busy status. In this
	// case, we exit this function immediately without returning anything. The
	// client will effectively receive EOF and must handle that condition.
	if busy {
		log.Printf("[Coordination.ConnTrack] [!!OVERLOADED!!] Dropping question as we are overloaded: [%s] for [%s].", question_id, in.GetCharacter())
		return nil
	}

	// Ensure we have the latest character information stored in GPC:
	if *use_redis {
		err := coordination.CacheCharacter(gpc_ctx, in.GetCharacter(), max_character_cache_age)
		if err != nil {
			log.Printf("[Character] Unable to retrieve character [%s]: %v", in.GetCharacter(), err)
			return err
		}
	} else {
		// Check to see if the character we've received via gRPC is present in our GPC, despite not loading from Redis:
		_, found := gpc.CharacterMap[in.GetCharacter()]
		if found {
			log.Printf("[Character] Loaded parameters for [%s] from local environment variables.", in.GetCharacter())
		} else {
			log.Printf("[Character] [%s] parameters were not found in local environment variables.", in.GetCharacter())
			return fmt.Errorf("not enough parameters")
		}
	}

	// The input might contain extra nonsense:
	trimmed_question := strings.TrimSuffix(in.GetQuestion(), "\n")

	// Begin question-answer processing:
	log.Printf("[AskQuestion] Received %s question with ID: [%s] for character [%s]: [%s]", common.PrettyQuestionCount(&total_question_count), question_id, in.GetCharacter(), trimmed_question)

	// Capture full string output of a response for analytics:
	var sb strings.Builder

	// Construct an efficient pipe for handling I/O through various writers:
	pr, pw := io.Pipe()

	// Then buffer it:
	bufferedPipe := bufio.NewWriter(pw)

	// We might process >1 at a time:
	deadlock_time := make(map[string]float64)
	var prompt_duration_seconds float64

	// Spawn new async invocation of the model:
	go func() {
		// This is needed to send EOF and finish this, and its parent function:
		defer pw.Close()

		start := time.Now()
		if *use_vertexai {
			prompt_duration_seconds = invocation.InteractWithVertexAI(bufferedPipe, gpc_ctx, in.GetCharacter(), trimmed_question)
		} else {
			prompt_duration_seconds = invocation.InteractWithLlama(bufferedPipe, gpc_ctx, in.GetCharacter(), trimmed_question)
		}
		duration := time.Since(start)
		deadlock_time[question_id] = duration.Seconds()

	}()

	// We're going to use buffered I/O to help ensure any word received is stored and processed properly:
	scanner := bufio.NewScanner(pr)
	scanner.Split(bufio.ScanWords)

	for scanner.Scan() {
		word := scanner.Text()

		resp := pb.Answer{Answer: word}
		if err := srv.Send(&resp); err != nil {
			log.Printf("send error %v", err)
		}
		sb.WriteString(word)
		sb.WriteString(" ")
	}

	answer_metric := &metrics.AnswerMetric{
		GamePromptInfo:        gameprompt_info,
		DateTime:              time.Now().UTC(),
		CharacterKey:          in.GetCharacter(),
		CharacterVersion:      gpc.CharacterMap[in.GetCharacter()].Version,
		QuestionID:            question_id,
		PodUID:                gpc.PodUID,
		CPUInfo:               gpc.CPUInfo,
		StartupTime:           startup_time,
		QuestionText:          trimmed_question,
		AnswerText:            sb.String(),
		PromptTime:            prompt_duration_seconds,
		DeadlockTime:          deadlock_time[question_id],
		ModelName:             gpc.ModelPath,
		Threads:               gpc.ThreadCount,
		LlamaSeed:             gpc.LlamaSeed,
		LlamaContextSize:      gpc.LlamaContextSize,
		LlamaRepeatPenalty:    gpc.LlamaRepeatPenalty,
		LlamaGPUOffloadLayers: gpc.LlamaGPUOffloadLayers,
		ModelTemperature:      gpc.ModelTemperature,
		ModelMaxOutputTokens:  gpc.ModelMaxOutputTokens,
		VertexAITopK:          gpc.VertexAITopK,
		VertexAITopP:          gpc.VertexAITopP,
	}

	// Report metrics back to BigQuery:
	if gpc.UseMock || !*use_bigquery {
		log.Println("[Metrics] Did NOT send metric:")
	} else {
		metrics.SendMetric(gpc_ctx, answer_metric)
	}

	// Cleanup question time:
	delete(deadlock_time, question_id)

	return nil
}

// Initialization =============================================================

func init() {

	// Establish default logging prefix for _this_ package:
	log.SetPrefix("[GamePrompt.Server] ")

	// Construct primary GamePromptConfig:
	gpc = common.NewGamePromptConfig()

	// Read in environment variables and flags:
	flag.Parse()

	// Update flags into GPC:
	gpc.ThreadCount = *thread_count
	gpc.UseMock = *use_mock
	gpc.LlamaGPUOffloadLayers = *gpu_layers

	// Load all environment variables into the GPC struct:
	common.LoadEnvIntoGPC(gpc)

	// If we are using Vertex AI, update the model path below:
	if *use_vertexai {
		gpc.ModelPath = fmt.Sprintf("projects/%s/locations/us-central1/publishers/google/models/%s", gpc.Project, invocation.VERTEX_AI_MODEL_NAME)
	}

	// Log our current configuration:
	common.LogGPCServerVariables(gpc)

	// There's no point reading in a model if we're not going to use it:
	if gpc.UseMock || *use_vertexai {
		*warmup = false
	}

	// Lastly, create a context that we'll use for subsequent GamePrompt packages:
	gpc_ctx = context.WithValue(context.Background(), common.GPCCtxKey, gpc)

	// Create our Redis clients:
	if *use_redis {
		gpc.WorkerClient = coordination.NewRedisClient(gpc_ctx, coordination.WORKER_DB)
		gpc.OpsClient = coordination.NewRedisClient(gpc_ctx, coordination.OPS_METADATA_DB)
		gpc.CharacterClient = coordination.NewRedisClient(gpc_ctx, coordination.CHARACTER_MEDATADATA_DB)
	} else {
		log.Println("Not using Redis for coordination.")
	}

	// Create our BigQuery client:
	if *use_bigquery {
		gpc.BigQueryClient = metrics.NewBigqueryClient(gpc_ctx)
	} else {
		log.Println("Not sending metrics to BigQuery.")
	}

	// Handle signals:
	signal.Notify(sigc, syscall.SIGINT, syscall.SIGTERM)
	go func() {
		<-sigc
		log.Println("\nReceived termination signal, exiting...")
		os.Exit(0)
	}()

}

// Main Program ===============================================================

func main() {

	print(
		`  ____                      ____                            _
 / ___| __ _ _ __ ___   ___|  _ \ _ __ ___  _ __ ___  _ __ | |_
| |  _ / _' | '_ ' _ \ / _ \ |_) | '__/ _ \| '_ ' _ \| '_ \| __|
| |_| | (_| | | | | | |  __/  __/| | | (_) | | | | | | |_) | |_
 \____|\__,_|_| |_| |_|\___|_|   |_|  \___/|_| |_| |_| .__/ \__|
                                                     |_|
`)

	log.Println("Welcome to GamePrompt!")
	log.Println(gameprompt_info)

	if *warmup {
		cache_file_into_memory()

	} else {
		log.Println("Warmup skipped.")
	}

	// Perform some simple sanity checking on connection counts and core counts:
	// Put this towards the bottom so that people see it, as it's important:
	if !*use_vertexai && *gpu_layers == 0 {
		if *max_connections**thread_count > runtime.NumCPU() {
			log.Println(" ")
			log.Println(" ==================================================================================================== ")
			log.Println("                                    !! WARNING !!")
			log.Println("You are accepting more connections than your CPU can reasonably handle:")
			log.Printf("[%d] connections * [%d] threads per connection is [%d], which is more than the [%d] available CPU cores.", *max_connections, *thread_count, (*max_connections * *thread_count), runtime.NumCPU())
			log.Println("Due to the nature of CPU-based invocation, you will see exponentially longer delays for concurrent connections.")
			log.Println("You should relaunch GamePrompt with fewer connections, or with GPU offloading, or with Vertex AI.")
			log.Println(" ==================================================================================================== ")
			log.Println(" ")
		}
	}

	// Declare gRPC port:
	lis, err := net.Listen("tcp", ":"+strconv.Itoa(*port))
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}

	// Create primary gRPC server:
	s := grpc.NewServer(grpc.MaxConcurrentStreams(uint32(*max_connections + 1)))
	healthServer := health.NewServer()

	// Register protobufs:
	pb.RegisterConverseServer(s, grpc_server{})
	healthpb.RegisterHealthServer(s, healthServer)

	// Set healthchecks:
	healthServer.SetServingStatus("", healthpb.HealthCheckResponse_SERVING)
	healthServer.SetServingStatus(pb.Converse_ServiceDesc.ServiceName, healthpb.HealthCheckResponse_SERVING)

	// Do Redis stuff:
	if *use_redis {
		// Fork off Redis heartbeat loop:
		go coordination.Heartbeat(gpc_ctx, &connection_count)
	}

	// Ensure we track connections properly:
	go prune_connections()

	log.Println("Starting gRPC server...")

	// This blocks until killed:
	if err := s.Serve(lis); err != nil {
		log.Fatalf("failed to serve: %v", err)
	}

	log.Println("GamePrompt gRPC server terminating.")

}
