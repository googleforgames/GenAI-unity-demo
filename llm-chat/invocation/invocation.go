package invocation

import (
	"bufio"
	"context"
	"fmt"
	"gss/gameprompt/common"
	"log"
	"os/exec"
	"strings"
	"time"

	aiplatform "cloud.google.com/go/aiplatform/apiv1"
	aiplatformpb "cloud.google.com/go/aiplatform/apiv1/aiplatformpb"
	"google.golang.org/api/option"
	"google.golang.org/protobuf/types/known/structpb"
)

// Constants ==================================================================

const VERTEX_AI_MODEL_NAME = "text-bison@001"

// Functions ==================================================================

func WriteStringSlowly(w *bufio.Writer, text string, word_delay time.Duration) {

	for _, word := range strings.Split(text, " ") {

		w.WriteString(word)
		w.WriteString(" ")
		w.Flush()

		time.Sleep(word_delay)
	}
}

// Interact is the primary function used to actually interact with the LLM. It
// returns the length of time waiting for the first token to come back from the
// LLM, in seconds.
func InteractWithLlama(w *bufio.Writer, ctx context.Context, character_key, prompt string) float64 {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	complete_prompt := fmt.Sprintf("%s\n### Question: %s\n%s\n",
		gpc.CharacterMap[character_key].PromptPrefix,
		prompt,
		gpc.CharacterMap[character_key].PromptSuffix,
	)

	var arguments = []string{
		"--model", gpc.ModelPath,
		"--multiline-input",
		"--seed", fmt.Sprintf("%d", gpc.LlamaSeed),
		"--threads", fmt.Sprintf("%d", gpc.ThreadCount),
		"--ctx-size", fmt.Sprintf("%d", gpc.LlamaContextSize),
		"--temp", fmt.Sprintf("%f", gpc.ModelTemperature),
		"--repeat_penalty", fmt.Sprintf("%f", gpc.LlamaRepeatPenalty),
		"--n-predict", fmt.Sprintf("%d", gpc.ModelMaxOutputTokens),
		"--n-gpu-layers", fmt.Sprintf("%d", gpc.LlamaGPUOffloadLayers),
		"--log-disable",
		"--prompt", complete_prompt}

	var time_at_question_receipt = time.Now().UTC()
	var time_at_first_token time.Time
	var time_waiting_for_first_token time.Duration

	// If mock, send a loading phrase and then this static phrase:
	if gpc.UseMock {
		WriteStringSlowly(w, "This is a mock answer.", 2)

	} else {

		cmd := exec.Command(gpc.LlamaPath, arguments...)

		cmd_reader, err := cmd.StdoutPipe()
		if err != nil {
			log.Fatal("[Invocation] Pipe Failure:", err)
		}

		err = cmd.Start()
		if err != nil {
			log.Fatal("[Invocation] Start Command Failure:", err)
		}

		// Read each word from the buffer, but don't start writing to the Writer until after llama has printed your prompt back to you (the verbose flag does nothing):
		scanner := bufio.NewScanner(cmd_reader)
		scanner.Split(bufio.ScanWords)

		begin := false
		marked_time := false
		var sb strings.Builder
		for scanner.Scan() {
			word := scanner.Text()

			// We're receiving what we sent to the prompt back again (it echoes our input):
			if !begin {
				sb.WriteString(word)
				sb.WriteString(" ")

				if strings.Contains(sb.String(), gpc.CharacterMap[character_key].PromptSuffix) {
					begin = true
					continue
				}
			}

			// We're receiving words in the answer:
			if begin {
				w.WriteString(word)
				w.WriteString(" ")
				w.Flush()

				if !marked_time {
					time_at_first_token = time.Now().UTC()
					time_waiting_for_first_token = time_at_first_token.Sub(time_at_question_receipt)

					// log.Printf("[Invocation.Metrics] Time at invocation  : %v", time_at_question_receipt)
					// log.Printf("[Invocation.Metrics] Time at first token : %v", time_at_first_token)
					log.Printf("[Invocation.Metrics] Time to first token : %v", time_waiting_for_first_token.Seconds())

					marked_time = true
				}
			}
		}

		// If llama.cpp's default behavior of echoing back our prompt changes:
		if !begin {
			log.Fatalf("[Critical Error] - Was not able to determine where the answer began from the model, as its response [%s] did not contain your prompt suffix: [%s]", sb.String(), gpc.PromptSuffix)
		}

		err = cmd.Wait()
		if err != nil {
			log.Fatal("[Invocation] Wait Command Failure:", err.Error())
		}
	}

	w.WriteString("\n")
	w.Flush()

	return time_waiting_for_first_token.Seconds()
}

func GetResponseFromVertexAI(ctx context.Context, character_key, prompt string) string {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	complete_prompt := fmt.Sprintf("%s\n### Question: %s\n%s\n",
		gpc.CharacterMap[character_key].PromptPrefix,
		prompt,
		gpc.CharacterMap[character_key].PromptSuffix,
	)

	// As of Jul 2023, only available in us-central1
	c, err := aiplatform.NewPredictionClient(ctx, option.WithEndpoint("us-central1-aiplatform.googleapis.com:443"))
	if err != nil {
		log.Fatalf("Error 1: %v", err)
	}
	defer c.Close()

	params, err := structpb.NewValue(map[string]interface{}{
		"temperature":     gpc.ModelTemperature,
		"maxOutputTokens": gpc.ModelMaxOutputTokens,
		"topK":            gpc.VertexAITopK,
		"topP":            gpc.VertexAITopP,
	})
	if err != nil {
		log.Fatalf("Error 2: %v", err)
	}

	// Add input to the prompt
	instance, err := structpb.NewValue(map[string]interface{}{
		"prompt": complete_prompt,
	})
	if err != nil {
		log.Fatalf("Error 3: %v", err)
	}

	req := &aiplatformpb.PredictRequest{
		Endpoint:   gpc.ModelPath,
		Instances:  []*structpb.Value{instance},
		Parameters: params,
	}
	resp, err := c.Predict(ctx, req)
	if err != nil {
		log.Fatalf("Error 4: %v", err)
	}

	respMap := resp.Predictions[0].GetStructValue().AsMap()

	return respMap["content"].(string)
}

func InteractWithVertexAI(w *bufio.Writer, ctx context.Context, character_key, prompt string) float64 {

	var time_at_question_receipt = time.Now().UTC()
	var time_at_first_token time.Time
	var time_waiting_for_first_token time.Duration

	response := GetResponseFromVertexAI(ctx, character_key, prompt)

	time_at_first_token = time.Now().UTC()
	time_waiting_for_first_token = time_at_first_token.Sub(time_at_question_receipt)

	w.WriteString(response)
	w.WriteString("\n")
	w.Flush()

	return time_waiting_for_first_token.Seconds()
}
