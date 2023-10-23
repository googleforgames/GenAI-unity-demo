package coordination

import (
	"context"
	"gss/gameprompt/common"
	"log"
	"strings"
	"sync/atomic"
	"time"

	"github.com/redis/go-redis/v9"
)

// Constants ==================================================================

const (
	WORKER_DB               = 0
	OPS_METADATA_DB         = 1
	CHARACTER_MEDATADATA_DB = 2
	host_list               = "gameprompt_worker_list"
	question_count_key      = "question_count"
	question_count_age_key  = "question_count:since"
)

// Functions ==================================================================

// NewRedisClient is a simple helper function to tie clients to DBs.
func NewRedisClient(ctx context.Context, db int) *redis.Client {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	client := redis.NewClient(&redis.Options{
		Addr:     gpc.RedisHost + ":6379",
		Password: "",
		DB:       db,
	})

	return client
}

// Host Coordination Functions ================================================

// Heartbeat periodically sends a UUID along with the pod's IP to Redis every heartbeat_interval
// with an expiration of heartbeat_expiration.
func Heartbeat(ctx context.Context, connection_count *int32) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)
	var err error

	// Initial delay to wait for gRPC socket binding and instantiation:
	time.Sleep(time.Duration(time.Second * 5))

	for {
		// If we are processing a question, we are not ready:
		conns := atomic.LoadInt32(connection_count)

		if conns > 0 {
			log.Printf("[Coordination.ConnTrack] We are busy at [%d] connection(s), not sending heartbeat.", conns)

		} else {
			// Record PodUID:PodIP mapping with key expiration in main Redis DB:
			err = gpc.WorkerClient.Set(ctx, gpc.PodUID, gpc.PodIP, gpc.HeartbeatTimeout).Err()
			if err != nil {
				log.Fatalf("[Coordination] Could not set key:value [%s:%s] in Redis: %s", gpc.PodUID, gpc.PodIP, err)
			}

			// Add PodUID to Set in secondary Redis DB:
			err = gpc.OpsClient.SAdd(ctx, host_list, gpc.PodUID).Err()
			if err != nil {
				log.Fatalf("[Coordination] Could not add member [%s] to key [%s] in Redis: %s", gpc.PodUID, host_list, err)
			}

			log.Printf("[Coordination] Heartbeat sent for pod UID-IP: [%s]-[%s] for [%s].", gpc.PodUID, gpc.PodIP, gpc.HeartbeatTimeout)
		}

		time.Sleep(gpc.HeartbeatInterval)
	}

}

// GetReadyHost retrieves the IP address of a Pod who has been marked as ready,
// and removes the entry from Redis (i.e. removes worker from pool).
func GetReadyHost(ctx context.Context) (string, error) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	var ip string
	var uid string
	var err error

	for {

		// Retrieve a random Pod UID from Redis:
		uid, err = gpc.OpsClient.SPop(ctx, host_list).Result()
		if err != nil && err != redis.Nil {
			log.Printf("[Coordination] Removed pod with UID [%s] from worker list.", uid)
		}

		// We've got a key, now check if the server is alive:
		if err == nil {

			ip, err = gpc.WorkerClient.GetDel(ctx, uid).Result()
			if err == nil {
				// We got the key, and got the IP, we're done:
				log.Printf("[Coordination] Expired pod UID-IP [%s]-[%s] from worker list.", uid, ip)
				break

			} else if err == redis.Nil {
				// Got a server, but it wasn't available (SPOP worked, but GETDEL returned redis.Nil), so try again:
				log.Printf("[Coordination] Removed stale host from set: [%s]", uid)
				continue

			} else if err != nil {
				log.Fatalln("[Coordination] Miscellaneous Redis error: ", err)
			}

		} else if err == redis.Nil {
			// There are no servers available (no members in set):
			err = redis.Nil
			break

		} else if err != nil {
			// Misc Redis error:
			log.Fatalln("[Coordination] Miscellaneous Redis error: ", err)
		}
	}

	return ip, err
}

// Host Metadata Functions ====================================================

// IncrementQuestionCount is used for storing the total number of questions
// *attempted* to be asked by all clients. It is used to make scaling decisions,
// as clients who fail to obtain a ready worker still increment the count.
// It is reset periodically by a singular instance of Scaler.
func IncrementQuestionCount(ctx context.Context) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	err := gpc.OpsClient.Incr(ctx, question_count_key).Err()
	if err != nil {
		log.Fatalln("[Coordination] Could not increment question count", err)
	}
}

// GetQPS determines the current QPS from the existing question count over
// the period of time since the timestamp from GetQuestionAge().
func GetQPS(ctx context.Context) float64 {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	question_count, err := gpc.OpsClient.Get(ctx, question_count_key).Int()
	if err != nil {
		log.Fatalln("[Coordination] Could not get question count", err)
	}

	question_count_age, err := gpc.OpsClient.Get(ctx, question_count_age_key).Time()
	if err != nil {
		log.Fatalln("[Coordination] Could not get question count duration", err)
	}

	return float64(question_count) / time.Since(question_count_age).Seconds()
}

// ResetQuestionCountAndAge resets both the total question attempt count and
// the timestamp back to zero and now, respectively. This should be used
// carefully, as resetting too frequently or not frequently enough may result
// in different QPS calculations.
func ResetQuestionCountAndAge(ctx context.Context) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	err := gpc.OpsClient.Set(ctx, question_count_key, 0, 0).Err()
	if err != nil {
		log.Fatalln("[Coordination] Could not reset question count", err)
	}

	err = gpc.OpsClient.Set(ctx, question_count_age_key, time.Now().UTC(), 0).Err()
	if err != nil {
		log.Fatalln("[Coordination] Could not reset question count timestamp", err)
	}

}

// Character Metadata Operations ==============================================

// GetCharacter retrieves a GamePrompt Character from Redis based upon a given
// character_key (a lowercase unique name):
func GetCharacter(ctx context.Context, character_key string) (*common.Character, error) {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	// Keys used to retrieve information from Redis:
	character_version_key := character_key + ":version"
	last_updated_key := character_key + ":last-updated"
	prompt_prefix_key := character_key + ":prefix"
	prompt_suffix_key := character_key + ":suffix"
	loading_phrases_key := character_key + ":loading-phrases"
	busy_phrases_key := character_key + ":busy-phrases"

	var err error

	// First, check if this character exists in Redis. To accomplish this, we first check if the version key has been set,
	// as all characters _should_ have the following keys: version, last updated time, prefix, suffix, loading phrases, and busy phrases.
	version, err := gpc.CharacterClient.Get(ctx, character_version_key).Result()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", character_version_key, err)
		return nil, err
	}

	// Second, obtain the rest of the character information:
	last_updated, err := gpc.CharacterClient.Get(ctx, last_updated_key).Time()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", last_updated_key, err)
		return nil, err
	}

	prompt_prefix, err := gpc.CharacterClient.Get(ctx, prompt_prefix_key).Result()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", prompt_prefix_key, err)
		return nil, err
	}

	prompt_suffix, err := gpc.CharacterClient.Get(ctx, prompt_suffix_key).Result()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", prompt_suffix_key, err)
		return nil, err
	}

	loading_phrases, err := gpc.CharacterClient.SMembers(ctx, loading_phrases_key).Result()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", loading_phrases_key, err)
		return nil, err
	}

	busy_phrases, err := gpc.CharacterClient.SMembers(ctx, busy_phrases_key).Result()
	if err != nil {
		log.Printf("[Character] Could not get key: [%s]: %v", busy_phrases_key, err)
		return nil, err
	}

	return common.NewCharacter(character_key, version, prompt_prefix, prompt_suffix, loading_phrases, busy_phrases, last_updated), nil
}

// CacheCharacter ensures that whatever character GamePrompt is processing has
// been cached into GamePrompt's memory, to reduce overall load on Redis, given
// characters do not change that frequently. The cache is stored in the primary
// GPC object which is passed by context to downstream functions. While the
// amount of data is relatively small that we are placing in memory, care should
// be taken to not overdo it (i.e. ~100 or so characters is fine). As an  operator,
// you should prefer to defer cache updates in favor of 'eventual consistency',
// however, this function might be updated in the future to make use of the
// *Character.LastUpdated field.
func CacheCharacter(ctx context.Context, character_key string, max_age time.Duration) error {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	var character *common.Character
	var err error = nil

	refresh_cache := false
	found_in_cache := false

	// See if requested character exists in the cache already:
	_, found_in_cache = gpc.CharacterMap[character_key]
	if found_in_cache {
		log.Printf("[Character] Found [%s] in cache.", character_key)
		age := time.Since(gpc.CharacterChache[character_key])

		if age > max_age {
			log.Printf("[Character] Cache was too old for [%s].", character_key)
			refresh_cache = true
		}
	} else {
		log.Printf("[Character] Did not find [%s] in cache.", character_key)
		refresh_cache = true
	}

	if refresh_cache {
		character, err = GetCharacter(ctx, character_key)
		if err != nil {
			return err
		} else {
			log.Printf("[Character] Added [%s] to the character cache.", character_key)
			gpc.CharacterMap[character_key] = character
			gpc.CharacterChache[character_key] = time.Now().UTC()
		}
	}

	return nil
}

// SetCharacter writes a new character to the database
func SetCharacter(ctx context.Context, character *common.Character) error {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	// Keys used to store information in Redis:
	character_version_key := character.Name + ":version"
	last_updated_key := character.Name + ":last-updated"
	prompt_prefix_key := character.Name + ":prefix"
	prompt_suffix_key := character.Name + ":suffix"
	loading_phrases_key := character.Name + ":loading-phrases"
	busy_phrases_key := character.Name + ":busy-phrases"

	var err error

	err = gpc.CharacterClient.Set(ctx, character_version_key, character.Version, 0).Err()
	if err != nil {
		log.Printf("[Character] Could not set key: [%s]: %v", character_version_key, err)
		return err
	}

	err = gpc.CharacterClient.Set(ctx, last_updated_key, time.Now().UTC(), 0).Err()
	if err != nil {
		log.Printf("[Character] Could not set key: [%s]: %v", last_updated_key, err)
		return err
	}

	err = gpc.CharacterClient.Set(ctx, prompt_prefix_key, character.PromptPrefix, 0).Err()
	if err != nil {
		log.Printf("[Character] Could not set key: [%s]: %v", prompt_prefix_key, err)
		return err
	}

	err = gpc.CharacterClient.Set(ctx, prompt_suffix_key, character.PromptSuffix, 0).Err()
	if err != nil {
		log.Printf("[Character] Could not set key: [%s]: %v", prompt_suffix_key, err)
		return err
	}

	err = gpc.CharacterClient.SAdd(ctx, loading_phrases_key, character.LoadingPhrases).Err()
	if err != nil {
		log.Printf("[Character] Could not add members to key: [%s]: %v", loading_phrases_key, err)
		return err
	}

	err = gpc.CharacterClient.SAdd(ctx, busy_phrases_key, character.BusyPhrases).Err()
	if err != nil {
		log.Printf("[Character] Could not add members to key: [%s]: %v", busy_phrases_key, err)
		return err
	}

	return nil
}

// GetCharacterNames returns all the characters present in the database by
// filtering on "*:prefix", as we purposefully do not store a list of all
// characters as another data structure. If dealing with large numbers of
// characters, it might make sense to call SMEMBERS on another Redis entry,
// however, that has potential to become out-of-sync with actual character
// entries that people wish to use.
func GetCharacterNames(ctx context.Context) []string {

	gpc := ctx.Value(common.GPCCtxKey).(*common.GamePromptConfig)

	var keys []string
	var names []string
	var err error

	keys, err = gpc.CharacterClient.Keys(ctx, "*:prefix").Result()
	if err != nil {
		log.Fatalf("[Character] Could not get keys from prefix: %v", err)
	}

	for _, key := range keys {
		name := strings.TrimSuffix(key, ":prefix")
		log.Printf("[Character] Got new character name: [%s]", name)

		names = append(names, name)
	}

	return names
}

// GetCharacters is a bloated function which returns all characters from the
// database, and should not be used or used sparingly. This literally dumps
// the entirety of the character database and returns them in a map. You
// probably want to use GetCharacterNames() or GetCharacter(), though you
// should use it in conjunction with CacheCharacter().
func GetCharacters(ctx context.Context) map[string]*common.Character {

	var names []string
	var character *common.Character
	var characters = make(map[string]*common.Character)

	names = GetCharacterNames(ctx)

	for _, name := range names {
		character, _ = GetCharacter(ctx, name)
		characters[name] = character
	}

	return characters
}
