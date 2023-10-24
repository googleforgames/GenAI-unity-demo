using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if USK_MULTIPLAYER
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(EventsManager))]
    public class RoomManager :
#if USK_MULTIPLAYER
        MonoBehaviourPunCallbacks
#else
    MonoBehaviour
#endif
    {
        public List<SpawnZone> PlayersSpawnAreas = new List<SpawnZone>();
        
        public UIManager currentUIManager;

        public ProjectSettings projectSettings;

        public GameObject Player;

        public Camera DefaultCamera;
        
        public int currentInspectorTab;
        private int startTime;

        private float matchTime = -1;

        private bool hasStartTime;

        public Helper.MinimapParameters minimapParameters;
        
        public Controller controller;
        
        public bool isPause;

#if USK_MULTIPLAYER
        private List<Player> currentPlayersInRoom = new List<Player>();
#endif

#if USK_MULTIPLAYER
        void Awake()
        {
#if !USK_MULTIPLAYER
        Debug.LogWarning("To use the multiplayer mode, import PUN2 from Asset Store.");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif

#else
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning("You aren't in the Photon.RoomManager - Connect to this scene in the Lobby.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                currentUIManager = !FindObjectOfType<UIManager>() ? Instantiate(Resources.Load("UI Manager", typeof(UIManager)) as UIManager) : FindObjectOfType<UIManager>();
                currentUIManager.useMinimap = minimapParameters.useMinimap;

                projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
                
                currentUIManager.HideAllMultiplayerRoomUI();
                currentUIManager.HideAllMultiplayerLobbyUI();
                currentUIManager.HideAllSinglePlayerMenus();
                currentUIManager.CharacterUI.Inventory.ActivateAll();
                currentUIManager.CharacterUI.DisableAll();

                Time.timeScale = 1;

//              PhotonNetwork.IsMessageQueueRunning = true;

                if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.currentGameAndPassword)
                {
                    var password = (string) PhotonNetwork.CurrentRoom.CustomProperties["psw"];
                    var name = (string) PhotonNetwork.CurrentRoom.CustomProperties["gn"];

                    if (name != "")
                        currentUIManager.basicMultiplayerGameRoom.PauseMenu.currentGameAndPassword.text = "Game name: " + name;

                    if (password != "")
                        currentUIManager.basicMultiplayerGameRoom.PauseMenu.currentGameAndPassword.text += " | Password: " + password;
                }

                if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.exitButton)
                    currentUIManager.basicMultiplayerGameRoom.PauseMenu.exitButton.onClick.AddListener(LeaveMatch);
                
                if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.optionsButton)
                    currentUIManager.basicMultiplayerGameRoom.PauseMenu.optionsButton.onClick.AddListener(delegate { OpenOptions(true); });
                
                if (currentUIManager.gameOptions.back)
                    currentUIManager.gameOptions.back.onClick.AddListener(delegate { OpenOptions(false); });

                if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton)
                    currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton.onClick.AddListener(delegate { Pause(true); });
                
                if (currentUIManager.basicMultiplayerGameRoom.GameOverMenu.exitButton)
                    currentUIManager.basicMultiplayerGameRoom.GameOverMenu.exitButton.onClick.AddListener(LeaveMatch);
                
                if (currentUIManager.basicMultiplayerGameRoom.GameOverMenu.matchStatsButton)
                    currentUIManager.basicMultiplayerGameRoom.GameOverMenu.matchStatsButton.onClick.AddListener(OpenMatchStats);
                
                
                // if (currentUIManager.basicMultiplayerGameRoom.GameOverMenu.BackButton)
                //     currentUIManager.basicMultiplayerGameRoom.GameOverMenu.BackButton.onClick.AddListener(OpenGameOverMenu);

                if (DefaultCamera)
                    DefaultCamera.gameObject.SetActive(true);

                InitializeGameTimer();
                
                LaunchGame();
            }
#endif
        }
#if USK_MULTIPLAYER
        void Start()
        {
            if (!PhotonNetwork.InRoom) return;
            
            UpdatePlayersUIList();
        }

        void InitializeGameTimer()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    startTime = PhotonNetwork.ServerTimestamp;
                    var customValues = new Hashtable {{"StartTimer", startTime}};
                    PhotonNetwork.CurrentRoom.SetCustomProperties(customValues);
                    matchTime = (float) PhotonNetwork.CurrentRoom.CustomProperties["MatchTime"];
                    hasStartTime = true;
                }
                else
                {
                    Initialize();
                }
            }
        }

        private void Initialize()
        {
            hasStartTime = MultiplayerHelper.GetStartTime(out startTime, "StartTimer");
        }
        
        private float TimeRemaining()
        {
            var timer = PhotonNetwork.ServerTimestamp - startTime;
            return matchTime - timer / 1000f;
        }

        private void Update()
        {
            if (!Player) return;

            if (!controller)
                controller = Player.GetComponent<Controller>();
            
            if (controller.projectSettings.ButtonsActivityStatuses[10] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[10])
                                                                                                        || InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[10], controller)))
            {
                Pause(true);
            }

            if (hasStartTime)
            {
                if (matchTime != -1)
                {
                    var timeRemaining = TimeRemaining();
                    
                    if (currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer)
                        currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer.text = "Time left: " + MultiplayerHelper.FormatTime(timeRemaining);
                    
                    if (timeRemaining <= 0)
                        GameOver();
                }
                else
                {
                    if (currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer && currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer.gameObject.activeSelf)
                        currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer.gameObject.SetActive(false);
                }
            }
        }

        private void ClearPlayerList()
        {
            if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.notTeamsScrollRect)
            {
                foreach (Transform child in currentUIManager.basicMultiplayerGameRoom.PauseMenu.notTeamsScrollRect.content)
                {
                    if (child.gameObject.GetInstanceID() != currentUIManager.basicMultiplayerGameRoom.playerInfoPlaceholder.gameObject.GetInstanceID())
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void AddPlayersToList()
        {
            if (!PhotonNetwork.InRoom) return;

            var players = PhotonNetwork.PlayerList.ToList();
            
            if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.notTeamsScrollRect && currentUIManager.basicMultiplayerGameRoom.playerInfoPlaceholder)
            {
                for (var i = 0; i < players.Count; i++)
                {
                    var player = players[i];
                    if (player.CustomProperties.Count <= 0) continue;
                    
                    var tempPrefab = Instantiate(currentUIManager.basicMultiplayerGameRoom.playerInfoPlaceholder.gameObject, currentUIManager.basicMultiplayerGameRoom.PauseMenu.notTeamsScrollRect.content);
                    var tempScript = tempPrefab.GetComponent<UIPreset>();

                    tempPrefab.SetActive(true);

                    if (tempScript.Rank)
                        tempScript.Rank.text = (i + 1).ToString();

                    if (tempScript.Name)
                        tempScript.Name.text = player.NickName + MultiplayerHelper.EmptyLine;

                    if (tempScript.Icon)
                        tempScript.Icon.texture = Resources.Load((string) player.CustomProperties["avtr"]) as Texture;

                    if (tempScript.KD)
                        tempScript.KD.text = player.CustomProperties["k"] + " / " + player.CustomProperties["d"];

                    if (tempScript.Icon)
                        tempScript.Icon.texture = Resources.Load((string) player.CustomProperties["avatar"]) as Texture;

                    if (player.NickName == PhotonNetwork.LocalPlayer.NickName)
                        if (tempPrefab.GetComponent<Image>())
                            tempPrefab.GetComponent<Image>().color = tempScript.HighlightedColor;
                }
            }
            
            // StartCoroutine(InstantiateCharactersInfoTimeout());
        }

        IEnumerator RotationTimeout(float direction)
        {
            yield return new WaitForEndOfFrame();
                
            Player.transform.rotation = Quaternion.Euler(0, direction, 0);
            controller.CameraController._mouseAbsolute = new Vector2(direction, 0);
        }

        // IEnumerator InstantiateCharactersInfoTimeout()
        // {
        //     yield return new WaitForSeconds(1);
        //     InstantiateCharactersInfo(PhotonNetwork.PlayerList.ToList(), currentUIManager.basicMultiplayerGameRoom.PauseMenu.NotTeamsScrollRect.content);
        //     StopCoroutine(InstantiateCharactersInfoTimeout());
        // }
        
        public void UpdatePlayersUIList()
        {
            ClearPlayerList();
            AddPlayersToList();
        }
        

        void LaunchGame()
        {
            // currentUIManager.HideAllMultiplayerRoomUI();

            currentUIManager.CharacterUI.ActivateAll(currentUIManager.useMinimap);
            currentUIManager.EnableAllBlips();

            if (currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer)
                Helper.EnableAllParents(currentUIManager.basicMultiplayerGameRoom.MatchStats.MatchTimer.gameObject);
            
            InstantiateCharacter();
            
            if ((Application.isMobilePlatform || projectSettings.mobileDebug) && currentUIManager.UIButtonsMainObject)
            {
                currentUIManager.UIButtonsMainObject.SetActive(true);
                UIHelper.ManageUIButtons(controller, controller.inventoryManager, currentUIManager, controller.CharacterSync);
            }
        }

        private void InstantiateCharacter()
        {
            var spawnZone = PlayersSpawnAreas[Random.Range(0, PlayersSpawnAreas.Count)];
            var spawnPosition = Vector3.zero;
           
            if (spawnZone)
                spawnPosition = CharacterHelper.GetRandomPointInRectangleZone(spawnZone.transform);
            
            Player = PhotonNetwork.Instantiate(PlayerPrefs.GetString("BLM_CurrentCharacterName"), spawnPosition, Quaternion.Euler(0, spawnZone.transform.eulerAngles.y, 0));
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"IsPlayerLoaded", true}});

            StartCoroutine(RotationTimeout(spawnZone.transform.eulerAngles.y));
            
            // ClearPlayerList();
            // InstantiateCharactersInfo(PhotonNetwork.PlayerList.ToList(), currentUIManager.advancedMultiplayerGameRoom.StartMenu.PlayersContent.content);

            // Player.GetComponent<CharacterSync>().UpdatePlayersList();
            
            controller = Player.GetComponent<Controller>();
            controller.enabled = true;
            controller.inventoryManager.enabled = true;
            controller.ActiveCharacter = true;
        }

        public void Pause(bool showUI)
        {

            var value = !isPause;
            
            if (showUI)
            {
                if (value)
                {
                    UpdatePlayersUIList();
                }

                if (value)
                {
                    currentUIManager.basicMultiplayerGameRoom.PauseMenu.ActivateNotTeamsMenu(true, (string) PhotonNetwork.CurrentRoom.CustomProperties["gn"] != "");
                }
                else
                {
                    currentUIManager.basicMultiplayerGameRoom.PauseMenu.DisableAll();
                    currentUIManager.gameOptions.DisableAll();
                }
            }

            if(value) controller.anim.SetBool("Move", false);

            if (value)
            {
                controller.isPause = true;
                UIHelper.ManageUIButtons(controller, controller.inventoryManager, currentUIManager, controller.CharacterSync);
            }
            else
            {
                StartCoroutine(ControllerPauseDelay());
            }
            
            controller.CameraController.canUseCursorInPause = true;
            
            if ((Application.isMobilePlatform || projectSettings.mobileDebug) && currentUIManager.UIButtonsMainObject)
                currentUIManager.UIButtonsMainObject.SetActive(!value);

            isPause = value;
        }

        void OpenOptions(bool status)
        {
            if (status)
            {
                currentUIManager.basicMultiplayerGameRoom.PauseMenu.DisableAll();
                currentUIManager.gameOptions.ActivateAll();
            }
            else
            {
                currentUIManager.gameOptions.DisableAll();
                currentUIManager.basicMultiplayerGameRoom.PauseMenu.ActivateNotTeamsMenu(true, (string) PhotonNetwork.CurrentRoom.CustomProperties["gn"] != "");
            }
        }
        
        private IEnumerator ControllerPauseDelay()
        {
            yield return new WaitForEndOfFrame();
            controller.isPause = false;
            UIHelper.ManageUIButtons(controller, controller.inventoryManager, currentUIManager, controller.CharacterSync);

            StopCoroutine(ControllerPauseDelay());
        }
        
        public IEnumerator RestartGame()
        {
            yield return new WaitForSeconds(3);
            {
                if(controller.CameraController.MainCamera)
                    Destroy(controller.CameraController.MainCamera.gameObject);
                
                if (Player)
                {
                    GetComponent<EventsManager>().ResetMinimap();
                    PhotonNetwork.Destroy(Player);
                }
                
                LaunchGame();
                
                StopAllCoroutines();
            }
        }

        #region UIManaged

        void LeaveMatch()
        {
            if (Player)
            {
                Destroy(Player.GetComponent<Controller>().CameraController.MainCamera.gameObject);
                PhotonNetwork.Destroy(Player);
            }
            
            PhotonNetwork.LeaveRoom();
        }
        
        void OpenMatchStats()
        {
            UpdatePlayersUIList();
            
            currentUIManager.HideAllMultiplayerRoomUI();

            currentUIManager.basicMultiplayerGameRoom.PauseMenu.ActivateNotTeamsMenu(false, false);
            
            if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton)
                currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton.gameObject.SetActive(true);
        }

        void GameOver()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            currentPlayersInRoom = PhotonNetwork.PlayerList.ToList();

            StopAllCoroutines();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton)
            {
                currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton.onClick.RemoveAllListeners();
                currentUIManager.basicMultiplayerGameRoom.PauseMenu.resumeButton.onClick.AddListener(OpenGameOverMenu);
            }

            if ((Application.isMobilePlatform || projectSettings.mobileDebug) && currentUIManager.UIButtonsMainObject)
                currentUIManager.UIButtonsMainObject.SetActive(false);

            if (Player)
            {
                if (Player.GetComponent<Controller>().CameraController.MainCamera)
                    Destroy(Player.GetComponent<Controller>().CameraController.MainCamera.gameObject);

                PhotonNetwork.Destroy(Player);
            }

            if (DefaultCamera)
                DefaultCamera.gameObject.SetActive(true);
            
            OpenGameOverMenu();
        }

        void OpenGameOverMenu()
        {
            currentUIManager.CharacterUI.DisableAll();
            currentUIManager.HideAllMultiplayerRoomUI();
            currentUIManager.basicMultiplayerGameRoom.GameOverMenu.ActivateNotTeamsScreen();
        }

        #endregion

        #region PhotonCallBacks

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var eventsManager = GetComponent<EventsManager>();

                foreach (var area in eventsManager.allAreasInScene)
                {
                    foreach (var enemy in area.allEnemiesInZone)
                    {
                        enemy.RestartEnemyMovement();
                    }
                }
            }
//            if(!(bool)newMasterClient.CustomProperties["cl"])
//                newMasterClient.SetCustomProperties(new Hashtable{{"cl", true}});
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
            // SceneManager.LoadScene("Lobby");
        }

         public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // if game has been already started OR player only one in room OR offline mode with bots, then skipping the pre-match game and timer countdown
            // if ((bool) PhotonNetwork.CurrentRoom.CustomProperties["gs"]) return; //|| (int) PhotonNetwork.CurrentRoom.CustomProperties["mp"] == 1 || botsMode == MultiplayerHelper.BotsMode.BotsOnly || !changedProps.ContainsKey("IsPlayerLoaded")) return;

            if (changedProps.ContainsKey("IsPlayerLoaded"))
            {
                UpdatePlayersUIList();
            }
            else if (changedProps.ContainsKey("d"))
            {
                UpdatePlayersUIList();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            var eventsManager = GetComponent<EventsManager>();
            // aiManager.GetAllEnemies();// не надо здесь, лучше автоматически на каждом клиенете

            foreach (var area in eventsManager.allAreasInScene)
            {
                area.SyncAllEnemies(newPlayer.ActorNumber);
            }
            
            controller.CharacterSync.SendCurrentHealth(newPlayer.ActorNumber);

            StartCoroutine(UpdatePlayerListWithDelay());
//            CheckPlayersNames();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // var eventsManager = GetComponent<EventsManager>();
            
            UpdatePlayersUIList();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Initialize();
        }

        #endregion

        IEnumerator UpdatePlayerListWithDelay()
        {
            yield return new WaitForSeconds(1);
            UpdatePlayersUIList();
        }
#endif
#endif
    }
}


