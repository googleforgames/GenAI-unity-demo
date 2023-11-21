using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
#if USK_MULTIPLAYER
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif

namespace GercStudio.USK.Scripts
{
    public class LobbyManager : 
#if USK_MULTIPLAYER
        MonoBehaviourPunCallbacks, ILobbyCallbacks
#else
    MonoBehaviour
#endif
    {
        public List<MultiplayerHelper.MultiplayerLevel> allMaps = new List<MultiplayerHelper.MultiplayerLevel>{new MultiplayerHelper.MultiplayerLevel()};
        
#if UNITY_EDITOR
        public List<SceneAsset> currentMapsInEditor = new List<SceneAsset>{null};
        public List<SceneAsset> oldMapsInEditor = new List<SceneAsset>();
#endif

        public UIManager currentUIManager;
        
        public List<Controller> characters = new List<Controller>();
        public List<Texture> defaultAvatars = new List<Texture>();
        
        public List<UIPreset> allAvatarsPlaceholders = new List<UIPreset>();
        public List<UIPreset> allMapsPlaceholders = new List<UIPreset>();

        public ProjectSettings projectSettings;

        public RuntimeAnimatorController characterAnimatorController;

        public Camera defaultCamera;
        public Camera characterSelectionCamera;
        
        private GameObject currentCharacter;

        public Transform characterSpawnPoint;

        public string checkConnectionServer = "https://google.com";

        // public bool adjustCameraPosition;
        public bool checkInternetConnection = true;
        public bool matchTimeLimit;
        public bool canAttackTeammates;

        private MultiplayerHelper.CameraPosition currentCameraPositions;
        
        public MultiplayerHelper.CameraPosition mainMenuCameraPosition;
        public MultiplayerHelper.CameraPosition characterSelectorCameraPosition;

        #region InspectorVariables
        
        public int currentInspectorTab;
        public int lastInspectorTab;
        public int currentCameraMode;
        public int lastCameraMode;

        #endregion
        
        public int currentMapIndex;
        public int currentCharacterIndex;
        public int currentAvatarIndex;


        public int maxPlayers;
        [Range(1, 1000)]public float matchTime;

#if USK_MULTIPLAYER
        private List<RoomInfo> allRoomsPlaceholders = new List<RoomInfo>();

        private bool isConnected;
        private bool firstTake = true;
        private string selectedRoomName;
#endif

#if USK_MULTIPLAYER
        private void Awake()
        {
#if USK_MULTIPLAYER  
           
            currentUIManager = !FindObjectOfType<UIManager>() ? Instantiate(Resources.Load("UI Manager", typeof(UIManager)) as UIManager) : FindObjectOfType<UIManager>();

            currentUIManager.HideAllSinglePlayerMenus();
            currentUIManager.HideAllMultiplayerRoomUI();
            currentUIManager.HideAllMultiplayerLobbyUI();
            currentUIManager.CharacterUI.DisableAll();

            currentUIManager.basicMultiplayerGameLobby.MainMenu.ActivateAll(true);
            
            projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
            
#else
#if UNITY_EDITOR
            Debug.LogWarning("To use the multiplayer mode, import PUN2 from the Asset Store.");
            UnityEditor.EditorApplication.isPlaying = false;
#endif
#endif
        }
        
#if USK_MULTIPLAYER
        void Start()
        {
            var defaultName = String.Empty;
            
            var charactersToRemove = new List<Controller>();
            
            foreach (var character in characters)
            {
                if (!character)
                {
                    charactersToRemove.Add(character);
                }
                else
                {
                    
                }
            }

            foreach (var tempCharacter in charactersToRemove)
            {
                characters.Remove(tempCharacter);
            }

            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.nicknameInputField)
            {
                if (PlayerPrefs.HasKey("PlayerName"))
                {
                    defaultName = PlayerPrefs.GetString("PlayerName");
                    currentUIManager.basicMultiplayerGameLobby.MainMenu.nicknameInputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;

            if(PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            UIHelper.InitializeLobbyUI(this);
            
            UpdateMapsPlaceholders();
                
            if(!PlayerPrefs.HasKey("BLM_CurrentCharacter"))
                PlayerPrefs.SetInt("BLM_CurrentCharacter", 0);

            if(!PlayerPrefs.HasKey("BLM_MapIndex"))
                PlayerPrefs.SetInt("BLM_MapIndex", 0);

            if (!PlayerPrefs.HasKey("BLM_AvatarIndex"))
                PlayerPrefs.SetInt("BLM_AvatarIndex", 0);

            SetPlayer(PlayerPrefs.GetInt("BLM_CurrentCharacter"));
            SetMap(PlayerPrefs.GetInt("BLM_MapIndex"));
            SetAvatar(PlayerPrefs.GetInt("BLM_AvatarIndex"));

            if (defaultCamera)
            {
                defaultCamera.gameObject.SetActive(true);
                mainMenuCameraPosition = new MultiplayerHelper.CameraPosition{position = defaultCamera.transform.position, rotation = defaultCamera.transform.rotation};
            }

            if (characterSelectionCamera)
            {
                characterSelectionCamera.gameObject.SetActive(false);
                characterSelectorCameraPosition = new MultiplayerHelper.CameraPosition{position = characterSelectionCamera.transform.position, rotation = characterSelectionCamera.transform.rotation};
            }

            currentCameraPositions = mainMenuCameraPosition;
            
            UpdateMainMenu();
            
            isConnected = false;

            if (checkInternetConnection)
            {
                if (Helper.GetHtmlFromUrl(checkConnectionServer) == "")
                {
                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "No Internet Connection";

                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.gameObject.SetActive(false);

                    StartCoroutine(CheckInternetConnection());
                }
                else
                {
                    if (!PhotonNetwork.IsConnected)
                        PhotonNetwork.ConnectUsingSettings();
                }
            }
            else
            {
                if (!PhotonNetwork.IsConnected)
                    PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void Update()
        {
            if (isConnected && currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "Connected | Ping - " + PhotonNetwork.GetPing() + " ms";

            if (defaultCamera)
            {
                var cameraTransform = defaultCamera.transform;
                cameraTransform.position = Vector3.Slerp(cameraTransform.position, currentCameraPositions.position, 0.6f);
                cameraTransform.transform.rotation = Quaternion.Lerp(cameraTransform.rotation, currentCameraPositions.rotation, 0.3f);
            }
        }

        void SetPlayer(int index)
        {
            if (!characters[index]) return;
            
            currentCharacterIndex = index;
                
            PlayerPrefs.SetString("BLM_CurrentCharacterName", characters[currentCharacterIndex].name);
            PlayerPrefs.SetInt("BLM_CurrentCharacter", currentCharacterIndex);

            if (characterSpawnPoint)
            {
                if (currentCharacter)
                    Destroy(currentCharacter);

                if (characters[currentCharacterIndex])
                {
                    currentCharacter = Instantiate(characters[currentCharacterIndex].gameObject, characterSpawnPoint.transform.position, characterSpawnPoint.transform.rotation);
                    currentCharacter.GetComponent<Animator>().runtimeAnimatorController = characterAnimatorController;

                    if (currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponsScrollRect && currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder)
                    {
                        foreach (Transform child in currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponsScrollRect.content)
                        {
                            if(child.gameObject.GetInstanceID() != currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder.gameObject.GetInstanceID())
                                Destroy(child.gameObject);
                        }
                        
                        var allCharacterWeapons = new List<WeaponController>();
                        
                        

                        var inventoryManager = currentCharacter.GetComponent<InventoryManager>();

                        for (var i = 0; i < 8; i++)
                        {
                            foreach (var slot in inventoryManager.slots[i].weaponSlotInInspector)
                            {
                                if (!slot.fistAttack && slot.weapon && slot.weapon.GetComponent<WeaponController>())
                                {
                                    allCharacterWeapons.Add(slot.weapon.GetComponent<WeaponController>());
                                }
                            }
                        }
                        
                        foreach (var weapon in allCharacterWeapons)
                        {
                            var placeholder = Instantiate(currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder, currentUIManager.basicMultiplayerGameLobby.CharactersMenu.weaponsScrollRect.content);
                            placeholder.gameObject.SetActive(true);
                            
                            if(weapon.weaponImage)
                                placeholder.texture = weapon.weaponImage;
                        }
                    }
                }
            }
        }

        public void ChangeCharacter(string type)
        {
            var index = currentCharacterIndex;

            if (type == "+")
            {
                index++;

                if (index > characters.Count - 1)
                    index = 0;
            }
            else
            {
                index--;

                if (index < 0)
                    index = characters.Count - 1;
            }
            
            SetPlayer(index);
        }


        private void CreateRoom(string password, string roomName)
        {
            
            var customValues = new Hashtable {{"psw", password}, {"gs", true}, {"gn", roomName}, {"map", allMaps.Count > 0 ? currentMapIndex : 0}, {"MatchTime", matchTimeLimit ? matchTime : -1}, {"km", canAttackTeammates ? MultiplayerHelper.CanKillOthers.Everyone : MultiplayerHelper.CanKillOthers.NoOne}};
            
            var roomOpt = new RoomOptions 
            {
                MaxPlayers = (byte) maxPlayers, 
                IsOpen = true, IsVisible = true, 
                CustomRoomProperties = customValues
            };

            var value = new string[3];
            value[0] = "map";
            value[1] = "psw";
            value[2] = "gn";
            roomOpt.CustomRoomPropertiesForLobby = value;
            
            
            PhotonNetwork.CreateRoom(Helper.GenerateRandomString(10), roomOpt);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("Player name is empty");
                return;
            }

            PhotonNetwork.NickName = name;
            PlayerPrefs.SetString("PlayerName", name);
        }

        static Predicate<RoomInfo> ByName(string name)
        {
            return room => room.Name == name;
        }

        #region UIManaged

        void SetMap(int index)
        {
            currentMapIndex = index;

            if (currentMapIndex > allMaps.Count - 1)
            {
                SetMap(0);
                return;
            }
            
            PlayerPrefs.SetInt("BLM_MapIndex", currentMapIndex);
            SetMapIndicator(index);
        }

        void SetMapIndicator(int index)
        {
            foreach (var placeholder in allMapsPlaceholders.Where(placeholder => placeholder.SelectionIndicator))
            {
                placeholder.SelectionIndicator.gameObject.SetActive(false);
            }
            
            if(allMapsPlaceholders.Count > 0 && allMapsPlaceholders[index].SelectionIndicator)
                allMapsPlaceholders[index].SelectionIndicator.gameObject.SetActive(true);
            
            
            if(currentUIManager.basicMultiplayerGameLobby.MainMenu.CurrentModeAndMap)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.CurrentModeAndMap.text = "Map - " + allMaps[index].name;
        }

        void UpdateMapsPlaceholders()
        {
            foreach (var child in allMapsPlaceholders)
            {
                Destroy(child.gameObject);
            }
            
            allMapsPlaceholders.Clear();
            
            for (var i = 0; i < allMaps.Count; i++)
            {
                var level = allMaps[i];
        
                if (currentUIManager.basicMultiplayerGameLobby.mapPlaceholder && currentUIManager.basicMultiplayerGameLobby.MapsMenu.scrollRect)
                {
                    var placeholder = Instantiate(currentUIManager.basicMultiplayerGameLobby.mapPlaceholder, currentUIManager.basicMultiplayerGameLobby.MapsMenu.scrollRect.content).GetComponent<UIPreset>();
                   
                    allMapsPlaceholders.Add(placeholder);
        
                    placeholder.name = level.name;
        
                    if (placeholder.Name)
                        placeholder.Name.text = level.name;
        
                    if (placeholder.ImagePlaceholder && level.image)
                        placeholder.ImagePlaceholder.texture = level.image;
                   
                    if(placeholder.SelectionIndicator)
                        placeholder.SelectionIndicator.gameObject.SetActive(false);
                   
                    placeholder.gameObject.SetActive(true);
                   
                    if (level.image)
                        placeholder.ImagePlaceholder.texture = level.image;
                    
                    var i1 = i;
                    
                    if(placeholder.Button)
                        placeholder.Button.onClick.AddListener(delegate { SetMap(i1); });
                }
            }
        }

        public void SetAvatar(int index)
        {
            currentAvatarIndex = index;

            PlayerPrefs.SetInt("BLM_AvatarIndex", currentAvatarIndex);
            
            foreach (var placeholder in allAvatarsPlaceholders.Where(placeholder => placeholder.SelectionIndicator))
            {
                placeholder.SelectionIndicator.gameObject.SetActive(false);
            }
            
            if(allAvatarsPlaceholders.Count > 0 && allAvatarsPlaceholders[currentAvatarIndex].SelectionIndicator)
                allAvatarsPlaceholders[currentAvatarIndex].SelectionIndicator.gameObject.SetActive(true);
        }

        void UpdateMainMenu()
        {
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.Avatar)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.Avatar.texture = defaultAvatars[currentAvatarIndex];
        }

        public void UpdateMaxPlayers(float value)
        {
            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayersText)
                currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayersText.text = "Max Players: " + (int) value + " ";

            currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers.value = value;
        }

        public void UpdateMatchDuration(float value)
        {
            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDurationText)
                currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDurationText.text = "Game Duration: " + (int) value + " min";
            
            currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDuration.value = value;

        }

        public void RandomRoomClick()
        {
            var foundRoom = false;

            if (allRoomsPlaceholders.Count > 0)
            {
                foreach (var room in allRoomsPlaceholders)
                {
                    if (currentMapIndex == (int) room.CustomProperties["map"] && room.IsOpen && room.IsVisible &&  (string) room.CustomProperties["psw"] == "")
                    {
                        foundRoom = true;
                        
                        if(PhotonNetwork.InLobby)
                            PhotonNetwork.JoinRoom(room.Name);
                        
                        break;
                    }
                }
                
                if(!foundRoom)
                    CreateRoom("", "");
            }
            else
            {
                CreateRoom("", "");
            }
        }

        public void CreateRoomClick()
        {
            var password = "";
            var gameName = "";

            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.Password)
                password = currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.Password.text;

            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.GameName)
            {
                gameName = currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.GameName.text;

                if (gameName == "")
                    gameName = "Game " + Helper.GenerateRandomString(4);
            }

            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers)
                maxPlayers = (int)currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers.value;

            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDuration)
            {
                matchTimeLimit = true;
                matchTime = (int)currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDuration.value * 60;
            }

            if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.canKillEachOther)
                canAttackTeammates = currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.canKillEachOther.isOn;


            CreateRoom(password, gameName);
        }

        void JoinSpecificRoom(string name, string password)
        {
            var room = allRoomsPlaceholders.Find(info => info.Name == name);
            
            if (room.IsOpen)
            {
                if (password != "")
                {
                    if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password && currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton)
                    {
                        Helper.EnableAllParents(currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.gameObject);
                        Helper.EnableAllParents(currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton.gameObject);

                        currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton.onClick.AddListener(delegate { CheckPassword(name, password); });
                    }

                    selectedRoomName = name;
                }
                else
                {
                    if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password && currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton)
                    {
                        currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.gameObject.SetActive(false);
                        currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton.gameObject.SetActive(false);
                    }
                    
                    if(!PhotonNetwork.InRoom)
                        PhotonNetwork.JoinRoom(name);
                }
            }
        }

        void CheckPassword(string name, string password)
        {
            if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password)
            {
                if (password == currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.text)
                {
                    var room = allRoomsPlaceholders.Find(info => info.Name == name);

                    if (room != null && room.IsOpen)
                    {
                        if(!PhotonNetwork.InRoom)
                            PhotonNetwork.JoinRoom(name);
                    }
                }
                else
                { 
                    currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.text = "";
                    currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.placeholder.GetComponent<Text>().text = "Wrong Password!";
                }
            }
        }

        public void OpenMenu(string type)
        {
            currentUIManager.HideAllMultiplayerLobbyUI();
            currentUIManager.SwitchMenu(type, "basic");
            
            switch (type)
            {
                case "maps":
                    currentUIManager.basicMultiplayerGameLobby.MapsMenu.ActivateAll();
                    break;

                case "settings":
                    currentUIManager.gameOptions.ActivateAll();
                    break;
                
                case "avatars":
                    currentUIManager.basicMultiplayerGameLobby.AvatarsMenu.ActivateAll();
                    break;
                
                case "characters":
                    currentCameraPositions = characterSelectorCameraPosition; //projectSettings.basicMultCameraCharacterPositions;
                    currentUIManager.basicMultiplayerGameLobby.CharactersMenu.ActivateAll();
                    break;
                
                case "allRooms":
                    currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.ActivateAll();
                    break;
                
                case "createGame":
                    currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.ActivateAll();

                    if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.currentMap)
                        currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.currentMap.text = "Map - " + allMaps[currentMapIndex].name;
                    break;
                
                case "mainMenu":
                    
                    UpdateMainMenu();

                    currentCameraPositions = mainMenuCameraPosition; //projectSettings.basicMultCameraMainMenuPositions;
                    currentUIManager.basicMultiplayerGameLobby.MainMenu.ActivateAll(isConnected);
                    break;
            }
        }

        public void CloseApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        void ChangeRegion(int value)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectToRegion(MultiplayerHelper.ConvertRegionToCode(value));
        }

        #endregion

        #region PhotonCallBacks

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var room in roomList)
            {
                if (allRoomsPlaceholders.Count > 0)
                {
                    if (!allRoomsPlaceholders.Exists(_room => _room.Name == room.Name))
                    {
                        if (room.PlayerCount > 0)
                            allRoomsPlaceholders.Add(room);
                    }
                    else
                    {
                        allRoomsPlaceholders.Remove(room);

                        if (room.PlayerCount <= 0)
                        {
                            if (room.Name == selectedRoomName)
                            {
                                if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password && currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton)
                                {
                                    currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.Password.gameObject.SetActive(false);
                                    currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.JoinButton.gameObject.SetActive(false);
                                }

                                selectedRoomName = "";
                            }
                        }
                        else
                        {
                            allRoomsPlaceholders.Add(room);
                        }
                    }
                }
                else
                {
                    if (room.PlayerCount > 0)
                        allRoomsPlaceholders.Add(room);
                }
            }

            UpdateRooms();
        }

        void UpdateRooms()
        {
            if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.scrollRect)
            {
                foreach (Transform child in currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.scrollRect.content)
                {
                    if(child.gameObject.GetInstanceID() != currentUIManager.basicMultiplayerGameLobby.roomInfoPlaceholder.gameObject.GetInstanceID())
                        Destroy(child.gameObject);
                }
            }

            foreach (var room in allRoomsPlaceholders)
            {
                if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.scrollRect && currentUIManager.basicMultiplayerGameLobby.roomInfoPlaceholder)
                {
                    var roomInfo = Instantiate(currentUIManager.basicMultiplayerGameLobby.roomInfoPlaceholder.gameObject, currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.scrollRect.content);
                    roomInfo.SetActive(true);

                    var script = roomInfo.GetComponent<UIPreset>();

                    if ((string) room.CustomProperties["gn"] != "")
                        script.Name.text = (string) room.CustomProperties["gn"];
                    else script.Name.text = "Auto Generated Room";

                    script.Map.text = allMaps[(int) room.CustomProperties["map"]].name;
                    script.Count.text = room.PlayerCount + " / " + maxPlayers;

                    if (script.ImagePlaceholder)
                        script.ImagePlaceholder.gameObject.SetActive((string) room.CustomProperties["psw"] != "");

                    script.Button.onClick.AddListener(delegate { JoinSpecificRoom(room.Name, (string) room.CustomProperties["psw"]); });
                }
            }
        }

        private void PlayerManager()
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"k", 0}, {"d", 0}, {"avatar", defaultAvatars[currentAvatarIndex].name}, {"pt", MultiplayerHelper.Teams.Null}});
            
            // if (projectSettings)
            // {
            //     projectSettings.weaponSlots.Clear();
            //     projectSettings.weaponsIndices.Clear();
            //     projectSettings.useAllWeapons = true;
            // }
        }

         public override void OnConnectedToMaster()
         {
             print("on joined master");
             
             PhotonNetwork.AutomaticallySyncScene = false;
             
             if (!PhotonNetwork.InLobby)
             {
                 PhotonNetwork.JoinLobby(TypedLobby.Default);
             }
         }

         public override void OnJoinedLobby()
        {
            print("on joined lobby");
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "Connected | Ping - " + PhotonNetwork.GetPing() + " ms"; //PhotonNetwork.CloudRegion.Substring(0, PhotonNetwork.CloudRegion.Length - 2).ToUpper();

            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown)
            {
                currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.gameObject.SetActive(true);

                if (firstTake)
                {
                    currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.value = MultiplayerHelper.ConvertCodeToRegion(PhotonNetwork.CloudRegion);
                    currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.onValueChanged.AddListener(ChangeRegion);
                    firstTake = false;
                }
            }

            isConnected = true;

//            if(currentUIManager.MultiplayerGameLobby.MainMenu.ConnectButton)
//                currentUIManager.MultiplayerGameLobby.MainMenu.ConnectButton.gameObject.SetActive(false);
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton.interactable = true;
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton.interactable = true;
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton.interactable = true;
        }

        
        IEnumerator CheckInternetConnection()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);

                if (checkInternetConnection && Helper.GetHtmlFromUrl(checkConnectionServer) != "" || !checkInternetConnection)
                {
                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "Disconnected from Server";

                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.gameObject.SetActive(true);

                    PhotonNetwork.ConnectUsingSettings();
                    
                    StopCoroutine(CheckInternetConnection());
                    break;
                }
            }
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            print("Failed create room: " + returnCode + "\n" + message);
        }

        public override void OnCreatedRoom()
        {
            //print("RoomManager is created");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            print("Game didn't found, but a new one has been created");
            CreateRoom("", "");
        }

        public override void OnJoinedRoom()
        {
            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                if (player.NickName == PhotonNetwork.NickName)
                {
                    PhotonNetwork.NickName = PhotonNetwork.NickName + " (" + Random.Range(100, 10000) + ")";
                }
            }

            PlayerManager();

            PhotonNetwork.LoadLevel(allMaps[(int) PhotonNetwork.CurrentRoom.CustomProperties["map"]].name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "Disconnected from Server";
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton.interactable = false;
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton.interactable = false;
            
            if (currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton)
                currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton.interactable = false;
            
            isConnected = false;

            if (checkInternetConnection)
            {
                if (Helper.GetHtmlFromUrl(checkConnectionServer) == "")
                {
                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "No Internet Connection";

                    if (currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown)
                        currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.gameObject.SetActive(false);

                    StartCoroutine(CheckInternetConnection());
                }
            }
            else
            {
                StartCoroutine(CheckInternetConnection());
            }
        }

        #endregion

#endif
#endif
    }
}




