package metrics

import (
	"context"
	"fmt"
	"gss/gameprompt/common"
	"log"
	"time"

	"cloud.google.com/go/bigquery"
	"google.golang.org/api/iterator"
)

// Types ======================================================================

// AnswerMetric describes all the metadata we'd like to store for analytics:
type AnswerMetric struct {
	GamePromptInfo        string    `bigquery:"gameprompt_info"`         // The version of GamePrompt we are using
	DateTime              time.Time `bigquery:"datetime"`                // When the question was asked
	CharacterKey          string    `bigquery:"character_key"`           // Which character we are talking to
	CharacterVersion      string    `bigquery:"character_version"`       // Which version of the character we are using
	QuestionID            string    `bigquery:"question_id"`             // The specific ID of the question
	PodUID                string    `bigquery:"pod_uid"`                 // Who answered the question
	CPUInfo               string    `bigquery:"cpu_info"`                // Some basic CPU information from the node on which the model ran
	StartupTime           float64   `bigquery:"startup_time"`            // The amount of time it took to read the model into memory
	QuestionText          string    `bigquery:"question_text"`           // The question iteself
	AnswerText            string    `bigquery:"answer_text"`             // The answer the model generated
	PromptTime            float64   `bigquery:"prompt_time"`             // The amount of time it took the model to process the prompt
	DeadlockTime          float64   `bigquery:"deadlock_time"`           // The total time spent processing the prompt and streaming the answer
	ModelName             string    `bigquery:"model_name"`              // The name of the model we invoked
	Threads               int       `bigquery:"llama_threads"`           // How many threads were used in answering the question
	LlamaSeed             int       `bigquery:"llama_seed"`              // Parameters/arguments used to invoke the model
	LlamaContextSize      int       `bigquery:"llama_context_size"`      // How large of a context is used with the model
	LlamaRepeatPenalty    float64   `bigquery:"llama_repeat_penalty"`    // How often words should not be repeated
	LlamaGPUOffloadLayers int       `bigquery:"llama_gpu_layers"`        // The number of layers offloaded to the GPU
	ModelTemperature      float64   `bigquery:"model_temperature"`       // How variable the responses are
	ModelMaxOutputTokens  int       `bigquery:"model_max_output_tokens"` // The number of maximum tokens to predict, where -1 is infinite
	VertexAITopK          int       `bigquery:"vertexai_top_k"`          // How the model selects tokens for output (selection size)
	VertexAITopP          float64   `bigquery:"vertexai_top_p"`          // How the model selects tokens for output (selection probability)
}

// ScalingStatistics holds the floats needed to make a scaling
// decision (see scaler.go).
type ScalingStatistics struct {
	Average           float64 `bigquery:"AVERAGE"`
	StandardDeviation float64 `bigquery:"DEVIATION"`
	Max               float64 `bigquery:"MAXIMUM"`
}

// Functions ==================================================================

// NewBigqueryClient is a helper function which returns a BQ client based upon
// the configuration present in the main GPC struct.
func NewBigqueryClient(gpc_ctx context.Context) *bigquery.Client {

	project := gpc_ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig).Project

	client, err := bigquery.NewClient(gpc_ctx, project)
	if err != nil {
		log.Fatalf("[Metrics] bigquery.NewClient: %v", err)
	}
	return client
}

// SendMetric establishes a connection to BQ and inserts 1 row of data:
func SendMetric(ctx context.Context, answer_metric *AnswerMetric) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)
	dataset := gpc.Dataset
	table := gpc.Table

	inserter := gpc.BigQueryClient.Dataset(dataset).Table(table).Inserter()
	if err := inserter.Put(ctx, answer_metric); err != nil {
		log.Fatalln("[Metrics] Could not insert data into BQ:", err)
	}

	log.Println("[Metrics] Sent metric:", answer_metric)
}

// GetDeadlockStatistics determines the appropriately-weighted average length
// of time required to process the average query. This is used for scaling
// decisions, as the average deadlock time * QPS = target replica count.
func GetDeadlockStatistics(ctx context.Context) *ScalingStatistics {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)
	project := gpc.Project
	dataset := gpc.Dataset
	table := gpc.Table

	average_deadlock_time_query := fmt.Sprintf("SELECT AVG(deadlock_time) as AVERAGE, STDDEV(deadlock_time) as DEVIATION, MAX(deadlock_time) as MAXIMUM FROM `%s.%s.%s`", project, dataset, table)

	average_query := gpc.BigQueryClient.Query(average_deadlock_time_query)

	rows, err := average_query.Read(ctx)
	if err != nil {
		log.Fatal("bigquery.Query.Read: ", err)
	}

	var stats ScalingStatistics
	for {
		err := rows.Next(&stats)
		if err == iterator.Done {
			break
		}
		if err != nil {
			log.Fatalf("bigquery.Query.Read.Next: %v\n", err)
		}
	}

	return &stats
}
