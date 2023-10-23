package common

import (
	"fmt"
	"log"
	"math"
	"os"
	"runtime"
	"strings"
	"sync/atomic"
	"time"

	"cloud.google.com/go/bigquery"
	"github.com/dustin/go-humanize"
	"github.com/redis/go-redis/v9"
	"github.com/shirou/gopsutil/cpu"
)

// Globals ====================================================================

var GPCCtxKey GamePromptGPCContextKey = "gpc-ctx"

// Types ======================================================================

// GamePromptGPCContextKey is a simple context key used to retrieve the primary
// GPC type from the context which is passed to subsequent functions:
type GamePromptGPCContextKey string

// Character stores all the information GamePrompt needs to know about
// an NPC:
type Character struct {
	Name           string    `json:"Name"`
	Version        string    `json:"Version"`
	LastUpdated    time.Time `json:"LastUpdated"`
	LoadingPhrases []string  `json:"LoadingPhrases"`
	BusyPhrases    []string  `json:"BusyPhrases"`
	PromptPrefix   string    `json:"PromptPrefix"`
	PromptSuffix   string    `json:"PromptSuffix"`
}

func (c *Character) String() string {
	var character_string strings.Builder

	// Name and version:
	character_string.WriteString(fmt.Sprintf("[%s] v[%s] as of [%s]:\n", c.Name, c.Version, c.LastUpdated))

	// Prefix and suffix:
	character_string.WriteString(fmt.Sprintf("Prompt Prefix: [%s]\nPrompt Suffix: [%s]\n", c.PromptPrefix, c.PromptSuffix))

	// Phrases:
	for i, phrase := range c.LoadingPhrases {
		character_string.WriteString(fmt.Sprintf("Loading phrase %d/%d: [%s]\n", i+1, len(c.LoadingPhrases), phrase))
	}

	for i, phrase := range c.BusyPhrases {
		character_string.WriteString(fmt.Sprintf("   Busy phrase %d/%d: [%s]\n", i+1, len(c.BusyPhrases), phrase))
	}

	return character_string.String()

}

// NewCharacter is a simple Character constructor.
func NewCharacter(name, version, prompt_prefix, prompt_suffix string, loading_phrases, busy_phrases []string, last_updated time.Time) *Character {
	return &Character{
		Name:           strings.ToLower(name), // Must be lowercase
		Version:        version,               // Should not include a leading "v" (e.g. "1.20.345" is OK, but "v1.20.345" is not)
		LastUpdated:    last_updated,
		PromptPrefix:   prompt_prefix,
		PromptSuffix:   prompt_suffix,
		LoadingPhrases: loading_phrases,
		BusyPhrases:    busy_phrases,
	}
}

// GamePromptConfig stores all necessary variables required for GamePrompt to operate, including
// all subsystems (coordiation, metrics). It is passed, when needed, via Context.
type GamePromptConfig struct {

	// GamePrompt parameters:
	VmtouchPath string
	PodIP       string
	PodUID      string

	// Redis coordination parameters:
	RedisHost         string
	HeartbeatInterval time.Duration
	HeartbeatTimeout  time.Duration

	// Redis clients:
	WorkerClient    *redis.Client
	OpsClient       *redis.Client
	CharacterClient *redis.Client

	// BigQuery metrics parameters:
	Project string
	Dataset string
	Table   string
	CPUInfo string

	// BigQuery Client
	BigQueryClient *bigquery.Client

	// Character store:
	CharacterMap    map[string]*Character
	CharacterChache map[string]time.Time

	// Invocation parameters:
	UseMock               bool
	ModelPath             string
	ThreadCount           int
	PromptPrefix          string
	PromptSuffix          string
	LlamaPath             string
	LlamaSeed             int
	LlamaContextSize      int
	LlamaRepeatPenalty    float64
	LlamaGPUOffloadLayers int
	ModelTemperature      float64 // Shared by Llama & Vertex AI
	ModelMaxOutputTokens  int     // Shared by Llama & Vertex AI
	VertexAITopK          int
	VertexAITopP          float64
}

// Functions ==================================================================

// NewGamePromptConfig returns a new primary GPC, but without BigQuery or Redis
// clients having been instantiated (add these from your caller):
func NewGamePromptConfig() *GamePromptConfig {
	return &GamePromptConfig{
		// GamePrompt parameters:
		VmtouchPath: fmt.Sprintf("/vmtouch-%s-%s-static-upx", runtime.GOOS, runtime.GOARCH), //vmtouch-linux-amd64-static-upx
		PodIP:       "localhost",
		PodUID:      "no_pod_uid",

		// Redis coordination parameters:
		RedisHost:         "localhost",
		HeartbeatInterval: time.Second * 5,
		HeartbeatTimeout:  time.Second * 6,

		// BigQuery metrics parameters:
		Project: "gss-arena",
		Dataset: "gameprompt_metrics",
		Table:   "lotr_metrics",
		CPUInfo: "unknown_cpu",

		// Character store:
		CharacterMap:    make(map[string]*Character),
		CharacterChache: make(map[string]time.Time),

		// Invocation parameters:
		UseMock:               false,
		LlamaPath:             fmt.Sprintf("/llama-%s-%s-static-upx", runtime.GOOS, runtime.GOARCH), //llama-linux-amd64-static-upx
		ModelPath:             "/model.bin",
		ThreadCount:           runtime.NumCPU(),
		LlamaSeed:             1,
		LlamaContextSize:      2048,
		LlamaRepeatPenalty:    1.1,
		LlamaGPUOffloadLayers: 0,
		ModelMaxOutputTokens:  1024,
		ModelTemperature:      0.25,
		VertexAITopK:          40,
		VertexAITopP:          0.8,
	}

}

// load_env_into_gpc overwrites sane defaults in the GPC from those in your
// environment variables, and checks for (some) sanity in those variables.
func LoadEnvIntoGPC(gpc *GamePromptConfig) {

	// GamePrompt parameters:
	var vmtouch_path = os.Getenv("GAMEPROMPT_VMTOUCH_PATH")
	var pod_ip = os.Getenv("POD_IP")
	var pod_uid = os.Getenv("POD_UID")

	// Redis coordination parameters:
	var redis_host = os.Getenv("GAMEPROMPT_REDIS_HOST")
	var heartbeat_interval = os.Getenv("GAMEPROMPT_HEARTBEAT_INTERVAL")
	var heartbeat_timeout = os.Getenv("GAMEPROMPT_HEARTBEAT_TIMEOUT")

	// BigQuery metrics parameters:
	var project = os.Getenv("GAMEPROMPT_PROJECT")
	var dataset = os.Getenv("GAMEPROMPT_DATASET")
	var table = os.Getenv("GAMEPROMPT_TABLE")

	// Invocation parameters:
	var llama_path = os.Getenv("GAMEPROMPT_LLAMA_PATH")
	var model_path = os.Getenv("GAMEPROMPT_MODEL_PATH")

	// If not using Redis, load local character metadata:
	var character_key = os.Getenv("GAMEPROMPT_CHARACTER_KEY")
	var local_character_version = os.Getenv("GAMEPROMPT_CHARACTER_VERSION")
	var local_prompt_prefix = os.Getenv("GAMEPROMPT_PROMPT_PREFIX")
	var local_prompt_suffix = os.Getenv("GAMEPROMPT_PROMPT_SUFFIX")
	var local_prompt_loading_phrases = os.Getenv("GAMEPROMPT_PROMPT_LOADING_PHRASES")
	var local_prompt_busy_phrases = os.Getenv("GAMEPROMPT_PROMPT_BUSY_PHRASES")

	var err error

	gpc.CPUInfo = GetCPUMetadata()

	if len(redis_host) > 0 {
		gpc.RedisHost = redis_host
	}

	if len(heartbeat_interval) > 0 {
		gpc.HeartbeatInterval, err = time.ParseDuration(heartbeat_interval)
		if err != nil {
			log.Fatalf("Could not parse time duration for heartbeat interval from [%s]: %s", heartbeat_interval, err)
		}
	}

	if len(heartbeat_timeout) > 0 {
		gpc.HeartbeatTimeout, err = time.ParseDuration(heartbeat_timeout)
		if err != nil {
			log.Fatalf("Could not parse time duration for heartbeat timeout from [%s]: %s", heartbeat_timeout, err)
		}
	}

	if len(pod_ip) > 0 {
		gpc.PodIP = pod_ip
	}

	if len(pod_uid) > 0 {
		gpc.PodUID = pod_uid
	}

	// Local character metadata:
	if len(character_key) > 0 &&
		len(local_character_version) > 0 &&
		len(local_prompt_prefix) > 0 &&
		len(local_prompt_suffix) > 0 &&
		len(local_prompt_loading_phrases) > 0 &&
		len(local_prompt_busy_phrases) > 0 {

		local_character := NewCharacter(
			character_key,
			local_character_version,
			local_prompt_prefix,
			local_prompt_suffix,
			strings.Split(local_prompt_loading_phrases, ";"),
			strings.Split(local_prompt_busy_phrases, ";"),
			time.Now().UTC(),
		)

		log.Println("[Character] Loaded a new character locally:")
		log.Println(local_character.String())

		gpc.CharacterMap[character_key] = local_character
	}

	if len(vmtouch_path) > 0 {
		gpc.VmtouchPath = vmtouch_path
	}

	if len(llama_path) > 0 {
		gpc.LlamaPath = llama_path
	}

	if len(model_path) > 0 {
		gpc.ModelPath = model_path
	}

	if len(project) > 0 {
		gpc.Project = project
	}

	if len(dataset) > 0 {
		gpc.Dataset = dataset
	}

	if len(table) > 0 {
		gpc.Table = table
	}
}

// PrettyQuestionCount returns the combination of Ordinal and Comma'd forms:
// e.g. 123456 -> 123,456th
func PrettyQuestionCount(total_question_count *uint32) string {
	qc := atomic.LoadUint32(total_question_count)
	ordinal := humanize.Ordinal(int(qc))
	commas := humanize.Comma(int64(qc))

	return commas + ordinal[len(ordinal)-2:]
}

// log_gpc_entry is a small helper function to print GPC variables:
func log_gpc_entry(name string, value any) {
	max_length := 28
	log.Printf("%-*s: %v", max_length, name, value)
}

// LogGPCServerVariables prints our GamePrompt server configuration:
func LogGPCServerVariables(gpc *GamePromptConfig) {
	log.Println("Paths ==============================================")
	log_gpc_entry(" - Path to `vmtouch`", gpc.VmtouchPath)
	log_gpc_entry(" - Path to `llama.cpp`", gpc.LlamaPath)
	log_gpc_entry(" - Path to model", gpc.ModelPath)

	log.Println("Server variables ===================================")
	log_gpc_entry(" - Pod IP", gpc.PodIP)
	log_gpc_entry(" - Pod UID", gpc.PodUID)
	log_gpc_entry(" - CPU Info", gpc.CPUInfo)

	log.Println("Binary variables ===================================")
	log_gpc_entry(" - Redis Host", gpc.RedisHost)
	log_gpc_entry(" - Heartbeat Interval", gpc.HeartbeatInterval.String())
	log_gpc_entry(" - Server Deadline", gpc.HeartbeatTimeout.String())

	log.Println("Google Cloud Variables =============================")
	log_gpc_entry(" - Project", gpc.Project)
	log_gpc_entry(" - Dataset", gpc.Dataset)
	log_gpc_entry(" - Table", gpc.Table)

	log.Println("Invocation Parameters ==============================")
	log_gpc_entry(" - Use Mock?", gpc.UseMock)
	log_gpc_entry(" - Thread Count", fmt.Sprintf("%d/%d", gpc.ThreadCount, runtime.NumCPU()))

	log.Println("Llama.cpp Parameters ===============================")
	log_gpc_entry(" - Llama Seed", gpc.LlamaSeed)
	log_gpc_entry(" - Llama Temp", gpc.ModelTemperature)
	log_gpc_entry(" - Llama Context", gpc.LlamaContextSize)
	log_gpc_entry(" - Llama Repeat Penalty", gpc.LlamaRepeatPenalty)
	log_gpc_entry(" - Llama Num Predict Tokens", gpc.ModelMaxOutputTokens)
	log_gpc_entry(" - Llama GPU Offload Layers", gpc.LlamaGPUOffloadLayers)
}

// LogGPCClientVariables only prints those parameters that concern the client:
func LogGPCClientVariables(gpc *GamePromptConfig) {
	log_gpc_entry("Redis Host", gpc.RedisHost)
}

// GetCPUMetadata attempts to resolve the CPU information.
// As of September 2023, this does not quite work with Apple Silicon :(
func GetCPUMetadata() string {

	vendor := "unknown_vendor"
	model := "unknown_model"
	clock := "unknown_clock"
	cache := "unknown_cache"

	// This will return an entry per core. If the first core is different (BIGlittle), this should be updated:
	info, _ := cpu.Info()

	if len(info) >= 1 {
		if len(info[0].VendorID) > 0 {
			vendor = info[0].VendorID
		}
		if len(info[0].ModelName) > 0 {
			model = info[0].ModelName
		}
		if info[0].Mhz > 0 {
			clock = fmt.Sprintf("%d", int(math.Round(info[0].Mhz)))
		}
		if info[0].CacheSize > 0 {
			cache = fmt.Sprintf("%d", info[0].CacheSize)
		}
	}

	return fmt.Sprintf("%s|%s|%s MHz|%s KiB", vendor, model, clock, cache)
}
