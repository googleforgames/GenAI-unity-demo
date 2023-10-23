package main

import (
	"bufio"
	"context"
	"flag"
	"fmt"
	"io"
	"log"
	"math/rand"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	pb "gss/gameprompt/api"
	"gss/gameprompt/common"
	"gss/gameprompt/coordination"
	"gss/gameprompt/invocation"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"

	"github.com/google/uuid"
	"github.com/redis/go-redis/v9"
)

/*
  ____                      ____                            _
 / ___| __ _ _ __ ___   ___|  _ \ _ __ ___  _ __ ___  _ __ | |_
| |  _ / _' | '_ ' _ \ / _ \ |_) | '__/ _ \| '_ ' _ \| '_ \| __|
| |_| | (_| | | | | | |  __/  __/| | | (_) | | | | | | |_) | |_
 \____|\__,_|_| |_| |_|\___|_|   |_|  \___/|_| |_| |_| .__/ \__|
             Your way to lifelike NPCs through LLMs! |_|

Program:	GamePrompt Client v1.1
Author:		Sebastian Weigand <tdg@google.com>
Copyright:	2023, Google LLC
License:	Apache License Version 2.0, January 2004

*/

// Globals ====================================================================

var sigc = make(chan os.Signal, 1) // For interrups and signals
var gpc_ctx context.Context        // With GamePrompt-centric params
var reader *bufio.Reader           // Primary buffered reader
var gpc *common.GamePromptConfig   // Primary parameter list

// Client-Specific Flags =======================================================

var character = flag.String("character", "Gandalf", "The character you wish to converse with.")
var host = flag.String("host", "localhost", "The host to connect to.")
var port = flag.String("port", "50005", "The port to use for communication.")
var use_redis = flag.Bool("use_redis", true, "Pull host from Redis.")
var aggressive_retransmit = flag.Bool("aggressive_retransmit", false, "Immediately retransmit gRPC request to overloaded backend via service address.")
var service_addr = flag.String("service_addr", "localhost", "Address of the load-balanced service (used with -aggresive_retransmit).")
var max_character_cache_age = time.Duration(time.Minute * 5) // Maximum age to wait to refresh character information from Redis

// Initialization =============================================================

func init() {

	// Establish default logging prefix for _this_ package:
	log.SetPrefix("[GamePrompt.Client] ")

	// Read in environment variables and flags:
	flag.Parse()

	// Keep character keys lowercase:
	*character = strings.ToLower(*character)

	// Construct primary GamePromptConfig:
	gpc = common.NewGamePromptConfig()

	// Lastly, create a context that we'll use for subsequent GamePrompt packages:
	gpc_ctx = context.WithValue(context.Background(), common.GPCCtxKey, gpc)

	// Load all environment variables into the GPC struct:
	common.LoadEnvIntoGPC(gpc)

	// Create our Redis clients:
	if *use_redis {
		// Redis clients:
		gpc.WorkerClient = coordination.NewRedisClient(gpc_ctx, coordination.WORKER_DB)
		gpc.OpsClient = coordination.NewRedisClient(gpc_ctx, coordination.OPS_METADATA_DB)
		gpc.CharacterClient = coordination.NewRedisClient(gpc_ctx, coordination.CHARACTER_MEDATADATA_DB)

		// Pull in character information here:
		err := coordination.CacheCharacter(gpc_ctx, *character, max_character_cache_age)
		if err != nil {
			log.Fatalf("[Character] Unable to retrieve character [%s]: %v", *character, err)
		}
	} else {
		log.Println("Not using Redis for coordination.")

		_, found := gpc.CharacterMap[*character]
		if found {
			log.Printf("[Character] Loaded parameters for [%s] from local environment variables.", *character)
		} else {
			log.Fatalf("[Character] [%s] parameters were not found in local environment variables.", *character)
		}

	}

	// Log our variables:
	common.LogGPCClientVariables(gpc)

	// Primary reader:
	reader = bufio.NewReader(os.Stdin)

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

	// Main loop:
	for {

		conn_id := uuid.NewString()
		fmt.Printf("\n\n(%s)> ", *character)
		input, _ := reader.ReadString('\n')

		if *use_redis {

			// Increment counter immediately for scaling:
			coordination.IncrementQuestionCount(gpc_ctx)

			// Retrieve a free gRPC server from the pool:
			addr, err := coordination.GetReadyHost(gpc_ctx)
			if err == redis.Nil {
				fmt.Println("No GamePrompt servers available at the moment.")

				if *aggressive_retransmit {
					addr = *service_addr

					print("\n")
					print(">>> WARNING: You are using aggressive retransmit. This could lead to potentially overloaded backends. <<<")
					print("\n\n")

				} else {
					continue
				}
			}
			flag.Set("host", addr)
			log.Printf("[Coordination] Using GamePrompt server @ '%s'...", addr)
		}

		// gRPC Stuff:
		conn, err := grpc.Dial(*host+":"+*port, grpc.WithTransportCredentials(insecure.NewCredentials()))
		if err != nil {
			log.Fatalf("can not connect with server %v", err)
		}

		client := pb.NewConverseClient(conn)

		status, err := client.AreYouBusy(gpc_ctx, &pb.AreYouBusyQuestion{ConnId: conn_id})
		if err != nil {
			log.Fatalf("can not get busy status: %v", err)
		}

		if status.GetBusy() {
			selection := rand.Intn(len(gpc.CharacterMap[*character].BusyPhrases))
			busy_phrase := gpc.CharacterMap[*character].BusyPhrases[selection]

			invocation.WriteStringSlowly(bufio.NewWriter(os.Stdout), busy_phrase, 0)

		} else {
			question := &pb.Question{
				ConnId:    conn_id,
				Character: *character,
				Question:  input,
			}

			// gRPC Invocation:
			stream, err := client.AskQuestion(gpc_ctx, question)
			if err != nil {
				log.Fatalf("open stream error %v", err)
			}

			// Construct an efficient pipe for handling I/O through various writers:
			pr, pw := io.Pipe()

			// Then buffer it:
			bpw := bufio.NewWriterSize(pw, bufio.MaxScanTokenSize)

			// Stream each token from server word-for-word:
			go func() {
				defer pw.Close()
				for {
					resp, err := stream.Recv()
					if err == io.EOF {
						break
					} else if err != nil {
						log.Fatalf("Client gRPC Receive Error: %v", err)
					}

					bpw.WriteString(resp.Answer)
					bpw.WriteString(" ")
					bpw.Flush()
				}
			}()
			selection := rand.Intn(len(gpc.CharacterMap[*character].LoadingPhrases))
			loading_phrase := gpc.CharacterMap[*character].LoadingPhrases[selection]

			stdout_writer := bufio.NewWriter(os.Stdout)

			delay, _ := time.ParseDuration("0.1s")
			invocation.WriteStringSlowly(stdout_writer, loading_phrase, delay)

			bps := bufio.NewScanner(pr)
			bps.Split(bufio.ScanWords)
			for bps.Scan() {
				word := bps.Text()
				stdout_writer.WriteString(word)
				stdout_writer.WriteString(" ")
				stdout_writer.Flush()
				time.Sleep(delay)
			}
		}
	}
}
