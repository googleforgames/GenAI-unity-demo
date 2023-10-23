package main

import (
	"bufio"
	"context"
	"flag"
	"fmt"
	"gss/gameprompt/common"
	"gss/gameprompt/coordination"
	"log"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	"github.com/dustin/go-humanize"
)

/*
  ____                      ____                            _
 / ___| __ _ _ __ ___   ___|  _ \ _ __ ___  _ __ ___  _ __ | |_
| |  _ / _' | '_ ' _ \ / _ \ |_) | '__/ _ \| '_ ' _ \| '_ \| __|
| |_| | (_| | | | | | |  __/  __/| | | (_) | | | | | | |_) | |_
 \____|\__,_|_| |_| |_|\___|_|   |_|  \___/|_| |_| |_| .__/ \__|
             Your way to lifelike NPCs through LLMs! |_|

Program:	GamePrompt Management v1.0
Author:		Sebastian Weigand <tdg@google.com>
Copyright:	2023, Google LLC
License:	Apache License Version 2.0, January 2004

*/

// Globals ====================================================================

var sigc = make(chan os.Signal, 1) // For interrups and signals
var gpc_ctx context.Context        // With GamePrompt-centric params
var reader *bufio.Reader           // Primary buffered reader
var gpc *common.GamePromptConfig   // Primary parameter list

func get_line(lower bool) string {
	line, _ := reader.ReadString('\n')
	line = strings.TrimSuffix(line, "\n")

	if lower {
		line = strings.ToLower(line)
	}
	return line
}

func init() {
	// Establish default logging prefix for _this_ package:
	log.SetPrefix("[GamePrompt.Management] ")

	// Read in environment variables and flags:
	flag.Parse()

	// Construct primary GamePromptConfig:
	gpc = common.NewGamePromptConfig()

	// Lastly, create a context that we'll use for subsequent GamePrompt packages:
	gpc_ctx = context.WithValue(context.Background(), common.GPCCtxKey, gpc)

	// Redis clients:
	gpc.WorkerClient = coordination.NewRedisClient(gpc_ctx, coordination.WORKER_DB)
	gpc.OpsClient = coordination.NewRedisClient(gpc_ctx, coordination.OPS_METADATA_DB)
	gpc.CharacterClient = coordination.NewRedisClient(gpc_ctx, coordination.CHARACTER_MEDATADATA_DB)

	// Load all environment variables into the GPC struct:
	common.LoadEnvIntoGPC(gpc)

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

func main() {

	fmt.Println("Welcome to GamePrompt!")
	fmt.Println("Enter the character you would like to create:")

	fmt.Printf("%15s", "Name: ")
	name := get_line(true)

	fmt.Printf("%15s", "Version: ")
	version := get_line(true)

	fmt.Printf("%15s", "Prompt Prefix: ")
	prompt_prefix := get_line(false)

	fmt.Printf("%15s", "Prompt Suffix: ")
	prompt_suffix := get_line(false)

	fmt.Println("================================================================================")

	loading_phrases := []string{}
	fmt.Println("Enter each loading phrase. Enter an empty loading phrase to finish:")
	count := 1
	for {

		fmt.Printf("%21s", fmt.Sprintf("%s loading phrase: ", humanize.Ordinal(count)))
		loading_phrase := get_line(false)

		if len(loading_phrase) == 0 {
			break
		} else {
			loading_phrases = append(loading_phrases, loading_phrase)
			count++
		}
	}

	fmt.Println("================================================================================")

	busy_phrases := []string{}
	fmt.Println("Enter each busy phrase. Enter an empty busy phrase to finish:")
	count = 1
	for {
		fmt.Printf("%21s", fmt.Sprintf("%s busy phrase: ", humanize.Ordinal(count)))
		busy_phrase := get_line(false)

		if len(busy_phrase) == 0 {
			break
		} else {
			busy_phrases = append(busy_phrases, busy_phrase)
			count++
		}
	}

	fmt.Println("================================================================================")

	new_character := common.NewCharacter(name, version, prompt_prefix, prompt_suffix, loading_phrases, busy_phrases, time.Now().UTC())

	fmt.Println("Here's your new character:")
	fmt.Println(new_character.String())

	fmt.Println("")
	fmt.Print("Does this look right? <Y/n>: ")
	confirmation := get_line(true)

	if confirmation == "y" || len(confirmation) == 0 {
		coordination.SetCharacter(gpc_ctx, new_character)
	} else {
		fmt.Println("Aborting.")
	}
}
