package main

import (
	"context"
	"gss/gameprompt/common"
	"gss/gameprompt/coordination"
	"log"
	"net/http"
	"os"
	"os/signal"
	"path/filepath"
	"strings"
	"syscall"
	"text/template"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
)

var sigc = make(chan os.Signal, 1)                     // For interrups and signals
var gpc_ctx context.Context                            // With GamePrompt-centric params
var gpc *common.GamePromptConfig                       // Primary parameter list
var characters = make(map[string]*common.Character)    // Primary character store, to reduce Redis calls
var character_timestamp time.Time                      // Freshness for characters
var max_character_age = time.Duration(time.Minute * 2) // Max age we can accept

func get_cached_character(name string) (*common.Character, error) {

	var character *common.Character
	var err error = nil

	refresh_cache := false
	found_in_cache := false

	age := time.Since(character_timestamp)

	log.Printf("Age of character timestamp: %s", age.String())
	log.Printf("Character timestamp: %s", character_timestamp.String())

	// See if requested character exists in the cache already:
	character, found_in_cache = characters[name]
	if found_in_cache {
		log.Printf("[Character] Found [%s] in cache.", name)
		if age > max_character_age {
			log.Printf("[Character] Cache was too old for [%s].", name)
			refresh_cache = true
		}
	} else {
		log.Printf("[Character] Did not find [%s] in cache.", name)
		refresh_cache = true
	}

	if refresh_cache {
		character, err = coordination.GetCharacter(gpc_ctx, name)
		character_timestamp = time.Now().UTC()
		if err != nil {
			log.Printf("[Character] Warning: Character [%s] not found: %v", name, err)
		} else {
			characters[name] = character
		}
	}

	log.Printf("Character Name: %s; Character Version: %s", character.Name, character.Version)

	return character, err
}

func get_cached_characters() map[string]*common.Character {

	// If we have no characters, always try refreshing:
	if len(characters) == 0 {
		log.Println("[Character] No characters cached.")
		characters = coordination.GetCharacters(gpc_ctx)
		character_timestamp = time.Now().UTC()
		return characters
	}

	// Check age of our local "cache":
	age := time.Since(character_timestamp)
	if age > max_character_age {
		log.Println("[Character] Character cache too old, refreshing...")
		characters = coordination.GetCharacters(gpc_ctx)
		character_timestamp = time.Now().UTC()
	} else {
		log.Println("[Character] Using cache for characters.")
	}

	return characters
}

func init() {

	// Establish default logging prefix for _this_ package:
	log.SetPrefix("[GamePrompt.WebUI] ")

	// Construct primary GamePromptConfig:
	gpc = common.NewGamePromptConfig()

	// Lastly, create a context that we'll use for subsequent GamePrompt packages:
	gpc_ctx = context.WithValue(context.Background(), common.GPCCtxKey, gpc)

	// Load all environment variables into the GPC struct:
	common.LoadEnvIntoGPC(gpc)

	// Redis clients:
	gpc.WorkerClient = coordination.NewRedisClient(gpc_ctx, coordination.WORKER_DB)
	gpc.OpsClient = coordination.NewRedisClient(gpc_ctx, coordination.OPS_METADATA_DB)
	gpc.CharacterClient = coordination.NewRedisClient(gpc_ctx, coordination.CHARACTER_MEDATADATA_DB)

	// Log our variables:
	// common.LogGPCClientVariables(gpc)

	// Handle signals:
	signal.Notify(sigc, syscall.SIGINT, syscall.SIGTERM)
	go func() {
		<-sigc
		log.Println("\nReceived termination signal, exiting...")
		os.Exit(0)
	}()

}

func main() {

	log.Println("Welcome to the GamePrompt Web UI Server!")

	r := chi.NewRouter()

	// r.Use(middleware.Compress(5))

	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)
	r.Use(middleware.URLFormat)

	// Route definitions:
	r.Get("/", AllCharacters)
	r.Get("/character", CharacterCreationForm)
	r.Get("/character/{character}", GetCharacter)
	r.Get("/character/edit/{character}", EditCharacter)

	r.Get("/resetcache", ResetCache)

	// This will update an existing character:
	r.Post("/character/{character}", UpdateCharacter)

	// This will create a new character:
	r.Post("/character", UpdateCharacter)

	// Create a route along /files that will serve contents from
	// the ./data/ folder.
	workDir, _ := os.Getwd()
	filesDir := http.Dir(filepath.Join(workDir, "static"))
	FileServer(r, "/static", filesDir)

	// Run the server:
	http.ListenAndServe(":3000", r)
}

// FileServer conveniently sets up a http.FileServer handler to serve
// static files from a http.FileSystem.
func FileServer(r chi.Router, path string, root http.FileSystem) {
	if strings.ContainsAny(path, "{}*") {
		panic("FileServer does not permit any URL parameters.")
	}

	if path != "/" && path[len(path)-1] != '/' {
		r.Get(path, http.RedirectHandler(path+"/", http.StatusMovedPermanently).ServeHTTP)
		path += "/"
	}
	path += "*"

	r.Get(path, func(w http.ResponseWriter, r *http.Request) {
		rctx := chi.RouteContext(r.Context())
		pathPrefix := strings.TrimSuffix(rctx.RoutePattern(), "/*")
		fs := http.StripPrefix(pathPrefix, http.FileServer(root))
		fs.ServeHTTP(w, r)
	})
}

func AllCharacters(w http.ResponseWriter, r *http.Request) {
	characters := get_cached_characters()

	t, _ := template.ParseFiles("templates/base.gohtml", "templates/all-characters.gohtml")
	err := t.Execute(w, characters)
	if err != nil {
		log.Fatalf("error: %v", err)
	}
}

func GetCharacter(w http.ResponseWriter, r *http.Request) {
	name := chi.URLParam(r, "character")
	character, err := get_cached_character(name)

	if err != nil {
		http.Error(w, http.StatusText(http.StatusNotFound), http.StatusNotFound)
	} else {
		t, _ := template.ParseFiles("templates/base.gohtml", "templates/character.gohtml")
		err = t.Execute(w, character)
		if err != nil {
			log.Fatalf("error: %v", err)
		}
	}
}

func EditCharacter(w http.ResponseWriter, r *http.Request) {
	name := chi.URLParam(r, "character")
	character, err := get_cached_character(name)
	if err != nil {
		http.Error(w, http.StatusText(http.StatusNotFound), http.StatusNotFound)
	} else {
		t, _ := template.ParseFiles("templates/base.gohtml", "templates/character-edit.gohtml")
		err = t.Execute(w, character)
		if err != nil {
			log.Fatalf("error: %v", err)
		}
	}
}

func CharacterCreationForm(w http.ResponseWriter, r *http.Request) {
	t, _ := template.ParseFiles("templates/base.gohtml", "templates/character-create.gohtml")
	err := t.Execute(w, nil)
	if err != nil {
		log.Fatalf("error: %v", err)
	}
}

func UpdateCharacter(w http.ResponseWriter, r *http.Request) {

	r.ParseForm()

	new_character := common.NewCharacter(
		r.FormValue("Name"),
		r.FormValue("Version"),
		r.FormValue("PromptPrefix"),
		r.FormValue("PromptSuffix"),
		strings.Split(r.FormValue("LoadingPhrases"), "\r\n"),
		strings.Split(r.FormValue("BusyPhrases"), "\r\n"),
		time.Now().UTC(),
	)

	coordination.SetCharacter(gpc_ctx, new_character)

	http.Redirect(w, r, "/resetcache", http.StatusFound)
}

func ResetCache(w http.ResponseWriter, r *http.Request) {
	character_timestamp = time.Unix(0, 0)
	log.Println("Cache expired.")
	http.Redirect(w, r, "/", http.StatusFound)
}
