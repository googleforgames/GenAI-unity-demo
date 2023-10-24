using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GercStudio.USK.Scripts
{
	public class UIManager : MonoBehaviour
	{
		[Serializable]
		public class AdvancedMultiplayerGameLobby
		{
			public UIPreset weaponPlaceholder;
			public UIPreset gameModePlaceholder;
			public UIPreset mapPlaceholder;
			public UIPreset avatarPlaceholder;
			public UIPreset roomInfoPlaceholder;

			public UIHelper.LobbyMainMenu MainMenu;
			public UIHelper.ProfileMenu ProfileMenu;
			public UIHelper.LobbyGameModesUI GameModesMenu;
			public UIHelper.LobbyMapsUI MapsMenu;
			public UIHelper.LobbyLoadoutMenu LoadoutMenu;
			public UIHelper.AvatarsMenu AvatarsMenu;
			public UIHelper.LobbyCharactersMenu CharactersMenu;
			public UIHelper.AllRoomsMenu AllRoomsMenu;
			public UIHelper.CreateRoomMenu CreateRoomMenu;
		}

		[Serializable]
		public class AdvancedMultiplayerGameRoom
		{
			public UIPreset playerInfoPlaceholder;
			public UIPreset matchStatsPlaceholder;
			public UIHelper.GameOverMenu GameOverMenu;
			public UIHelper.SpectateMenu SpectateMenu;
			public UIHelper.MatchStats MatchStats;
			public UIHelper.PauseMenu PauseMenu;
			public UIHelper.StartMenu StartMenu;
			public UIHelper.TimerBeforeMatch TimerBeforeMatch;
			public UIHelper.TimerAfterDeath TimerAfterDeath;
			public UIHelper.PreMatchMenu PreMatchMenu;
		}

		[Serializable]
		public class BasicMultiplayerGameLobby
		{
			public UIPreset mapPlaceholder;
			public UIPreset roomInfoPlaceholder;
			public UIPreset avatarPlaceholder;

			public UIHelper.LobbyMainMenu MainMenu;
			public UIHelper.LobbyMapsUI MapsMenu;
			public UIHelper.AvatarsMenu AvatarsMenu;
			public UIHelper.LobbyCharactersMenu CharactersMenu;
			public UIHelper.AllRoomsMenu AllRoomsMenu;
			public UIHelper.CreateRoomMenu CreateRoomMenu;
		}

		[Serializable]
		public class BasicMultiplayerGameRoom
		{
			public UIHelper.PauseMenu PauseMenu;
			public UIHelper.GameOverMenu GameOverMenu;
			public UIHelper.MatchStats MatchStats;
			public UIPreset playerInfoPlaceholder;
		}

		[Serializable]
		public class singlePlayerGame
		{
			public UIHelper.SinglePlayerGamePause SinglePlayerGamePause;
			public UIHelper.SinglePlayerGameOver SinglePlayerGameGameOver;
		}

		public UIHelper.GameOptions gameOptions;
		public AdvancedMultiplayerGameLobby advancedMultiplayerGameLobby;
		public AdvancedMultiplayerGameRoom advancedMultiplayerGameRoom;
		public BasicMultiplayerGameLobby basicMultiplayerGameLobby;
		public BasicMultiplayerGameRoom basicMultiplayerGameRoom;
		public singlePlayerGame SinglePlayerGame;

		public UIHelper.CharacterUI CharacterUI;

		public Button[] uiButtons = new Button[17];

		[Range(100, 1000)] public int moveStickRange = 200;
		public bool moveStickAlwaysVisible;
		public bool fixedPosition;
		public bool useMinimap;

		public GameObject UIButtonsMainObject;
		public GameObject moveStick;
		public GameObject moveStickOutline;
		public GameObject cameraStick;
		public GameObject cameraStickOutline;

		public RawImage gamepadCursor;

		public Texture gridTexture;
		public Texture previousGridTexture;

		public bool useGamepad;
		private bool canSwitchButton;

		public string currentMenuType;
		public string lastMenuType;

		public List<Button> allButtonsInCurrentMenu = new List<Button>();
		public Button currentButton;

		private Button currentRoom;

		private int roomsCount;
		private int roomIndex;

		private Vector3 mapCenterPosition;
		private Vector3 mapCornerPosition;
		private Vector3 mapMiddleCenterPosition;

		public Vector3 targetPosition;

		private RawImage mapPlaceholder;

		private float deltaAngle;
		private float mapDistanceScale;
		private float characterHeight;
		private GameManager gameManager;

		private ProjectSettings projectSettings;

#if USK_MULTIPLAYER
		private LobbyManager lobbyManager;
		private RoomManager roomManager;
#endif
#if USK_ADVANCED_MULTIPLAYER
		private AdvancedLobbyManager advancedLobbyManager;
		private AdvancedRoomManager advancedRoomManager;
#endif

		public Transform convertDirectionObject;

		public List<UIHelper.MinimapImage> allMinimapImages = new List<UIHelper.MinimapImage>();

		public List<GameObject> hideObjects;
		public bool hide;
		public bool hideUIManager;
		
		public RawImage grid;
		[Range(0, 255)] public int gridOpacity;
		public int lastGridOpacity;

		private int frameCount;
		private float deltaTime;
		public float currentFrameRate;
		
		public RectTransform leftScopeTextureFill;
		public RectTransform rightScopeTextureFill;
		public RectTransform fillParentRect;

		public Image leftFillImageComponent;
		public Image rightFillImageComponent;

#if UNITY_EDITOR
		#region InspectorVariables

		public int currentMenuInGame;
		public int roomMatchStatsTab;
		public int roomMatchStatsLastTab;
		public int curWeaponSlot;
		public int teamsIndex;
		public int lastTeamsIndex;
		public int preMatchMenuIndex;
		public int preMatchMenuLastIndex;
		public int deathScreensIndex;
		public int deathScreensLastIndex;
		

		public List<UIHelper.MenuPages> currentMenusInGame;
		public UIHelper.MenuPages currentMenuPage;
		public UIHelper.MenuPages previousMenuPage;

		public GameObject[] hierarchy;

		#endregion
#endif
		private void Awake()
		{
			if (!FindObjectOfType<EventSystem>())
			{
				Helper.CreateEventManager(transform);
			}

			var parent = CharacterUI.aimPlaceholder.transform.parent;
			
			fillParentRect = parent.GetComponent<RectTransform>();
			
			UIHelper.CreateScopeFill("Left Scope Fill", parent, ref leftScopeTextureFill, ref leftFillImageComponent);
			UIHelper.CreateScopeFill("Right Scope Fill", parent, ref rightScopeTextureFill, ref rightFillImageComponent);
			
			leftFillImageComponent.gameObject.SetActive(false);
			rightFillImageComponent.gameObject.SetActive(false);
			
			if(CharacterUI.aimPlaceholder)
				CharacterUI.aimPlaceholder.gameObject.SetActive(false);

			gameObject.name = Helper.CorrectName(gameObject.name);

			projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;

#if UNITY_EDITOR
			HideAllHierarchy();
#endif

			if (UIButtonsMainObject)
				UIButtonsMainObject.SetActive(false);

#if USK_MULTIPLAYER
			if (basicMultiplayerGameLobby.mapPlaceholder)
				basicMultiplayerGameLobby.mapPlaceholder.gameObject.SetActive(false);
			
			if (basicMultiplayerGameLobby.roomInfoPlaceholder)
				basicMultiplayerGameLobby.roomInfoPlaceholder.gameObject.SetActive(false);
			
			if (basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder)
				basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder.gameObject.SetActive(false);

			if (basicMultiplayerGameRoom.playerInfoPlaceholder)
				basicMultiplayerGameRoom.playerInfoPlaceholder.gameObject.SetActive(false);
			
#if USK_ADVANCED_MULTIPLAYER
			if (advancedMultiplayerGameLobby.weaponPlaceholder)
				advancedMultiplayerGameLobby.weaponPlaceholder.gameObject.SetActive(false);

			if (advancedMultiplayerGameLobby.mapPlaceholder)
				advancedMultiplayerGameLobby.mapPlaceholder.gameObject.SetActive(false);

			if (advancedMultiplayerGameLobby.gameModePlaceholder)
				advancedMultiplayerGameLobby.gameModePlaceholder.gameObject.SetActive(false);

			if (advancedMultiplayerGameLobby.avatarPlaceholder)
				advancedMultiplayerGameLobby.avatarPlaceholder.gameObject.SetActive(false);
			
			if (advancedMultiplayerGameLobby.roomInfoPlaceholder)
				advancedMultiplayerGameLobby.roomInfoPlaceholder.gameObject.SetActive(false);

			if (advancedMultiplayerGameRoom.matchStatsPlaceholder)
				advancedMultiplayerGameRoom.matchStatsPlaceholder.gameObject.SetActive(false);

			if (advancedMultiplayerGameRoom.playerInfoPlaceholder)
				advancedMultiplayerGameRoom.playerInfoPlaceholder.gameObject.SetActive(false);
#endif
#endif
			if (grid)
				grid.gameObject.SetActive(false);

			foreach (var marker in CharacterUI.hitMarkers)
			{
				if (marker)
					marker.gameObject.SetActive(false);
			}

#if USK_MULTIPLAYER
			roomManager = FindObjectOfType<RoomManager>();
			lobbyManager = FindObjectOfType<LobbyManager>();
#endif

#if USK_ADVANCED_MULTIPLAYER
			advancedLobbyManager = FindObjectOfType<AdvancedLobbyManager>();
			advancedRoomManager = FindObjectOfType<AdvancedRoomManager>();
#endif
			
			gameManager = FindObjectOfType<GameManager>();
			
			InstantiateResolutionButtons();
			UIHelper.ManageSettingsButtons(this);
			
			SetSettingsParameter(gameOptions.graphicsButtons, -1, "CurrentQualityButton", "quality");

			if (!Application.isMobilePlatform)
			{
				SetSettingsParameter(gameOptions.resolutionButtons, -1, "CurrentResolutionButton", "resolution");
				SetSettingsParameter(gameOptions.frameRateButtons, -1, "CurrentFrameRateButton", "framerate");
				SetWindowMode("CurrentWindowModeButton", -1);
			}

			if (!CharacterUI.mapMask
#if USK_MULTIPLAYER
			    || lobbyManager
#if USK_ADVANCED_MULTIPLAYER
			    || advancedLobbyManager
#endif
#endif
			) return;

			var corners = new Vector3[4];
			
			
			CharacterUI.mapMask.color = new Color32(255, 255, 255, 1);

			CharacterUI.mapPlaceholder = Helper.NewCanvas("Map Placeholder", CharacterUI.mapMask.transform).gameObject.AddComponent<RawImage>();
			CharacterUI.mapPlaceholder.rectTransform.sizeDelta = new Vector2(512, 512);
			CharacterUI.mapPlaceholder.rectTransform.localScale = new Vector3(1,1,1);
			
			if (gameManager)
			{
				if (gameManager.minimapParameters.mapTexture)
					CharacterUI.mapPlaceholder.texture = gameManager.minimapParameters.mapTexture;

				if (gameManager.minimapParameters.mapExample)
				{
					mapCenterPosition = gameManager.minimapParameters.mapExample.transform.position;

					gameManager.minimapParameters.mapExample.GetComponent<RectTransform>().GetWorldCorners(corners);
				}
			}
#if USK_MULTIPLAYER
			else if (roomManager)
			{
				if (roomManager.minimapParameters.mapTexture)
					CharacterUI.mapPlaceholder.texture = roomManager.minimapParameters.mapTexture;

				if (roomManager.minimapParameters.mapExample)
				{
					mapCenterPosition = roomManager.minimapParameters.mapExample.transform.position;

					roomManager.minimapParameters.mapExample.GetComponent<RectTransform>().GetWorldCorners(corners);
				}
			}
#if USK_ADVANCED_MULTIPLAYER
			else if (advancedRoomManager)
			{
				if (advancedRoomManager.mapTexture)
					CharacterUI.mapPlaceholder.texture = advancedRoomManager.mapTexture;

				if (advancedRoomManager.mapExample)
				{
					mapCenterPosition = advancedRoomManager.mapExample.transform.position;

					advancedRoomManager.mapExample.GetComponent<RectTransform>().GetWorldCorners(corners);
				}
			}
#endif
#endif
			else
			{
				CharacterUI.mapMask.gameObject.SetActive(false);
				return;
			}
			
			mapCenterPosition.y = 0;

			for (var i = 0; i < 4; i++)
			{
				corners[i].y = 0;
			}
			
			var direction = corners[0] - mapCenterPosition;
			var distance = Vector3.Distance(corners[0], mapCenterPosition);

			mapCornerPosition = mapCenterPosition + direction.normalized * distance;

			if (gameManager && gameManager.minimapParameters.mapExample)
				mapMiddleCenterPosition = corners[1] + (corners[2] - corners[1]).normalized * Vector3.Distance(corners[2], corners[1]) / 2;
#if USK_MULTIPLAYER
			else if (roomManager && roomManager.minimapParameters.mapExample)
				mapMiddleCenterPosition = corners[1] + (corners[2] - corners[1]).normalized * Vector3.Distance(corners[2], corners[1]) / 2;
#if USK_ADVANCED_MULTIPLAYER
			else if (advancedRoomManager && advancedRoomManager.mapExample)
				mapMiddleCenterPosition = corners[1] + (corners[2] - corners[1]).normalized * Vector3.Distance(corners[2], corners[1]) / 2;
#endif
#endif
			
			convertDirectionObject = new GameObject("correct direction").transform;
			convertDirectionObject.position = mapCenterPosition;
			
			convertDirectionObject.gameObject.hideFlags = HideFlags.HideInHierarchy;

			var convertDirectionObjectRotation = mapMiddleCenterPosition - mapCenterPosition;
			
			if(convertDirectionObjectRotation != Vector3.zero)
				convertDirectionObject.transform.rotation = Quaternion.LookRotation(convertDirectionObjectRotation, Vector3.up);
		}

		private void InstantiateResolutionButtons()
		{
			if(!gameOptions.resolutionButtonPlaceholder.button) return;
			
			var resolutions = UIHelper.GetResolutions(out var stringResolutions);

			foreach (var resolutionButton in gameOptions.resolutionButtons)
			{
				if(resolutionButton.button && resolutionButton.button.gameObject.GetInstanceID() != gameOptions.resolutionButtonPlaceholder.button.gameObject.GetInstanceID())
					Destroy(resolutionButton.button.gameObject);
			}
			
			gameOptions.resolutionButtons.Clear();
			
			gameOptions.resolutionButtonPlaceholder.button.gameObject.SetActive(false);

			for (var i = 0; i < resolutions.Count; i++)
			{
				var resolution = resolutions[i];
				var instantiatedButton = Instantiate(gameOptions.resolutionButtonPlaceholder.button.gameObject, gameOptions.resolutionsScrollRect.content).GetComponent<Button>();
				var buttonItem = new UIHelper.GameOptions.SettingsButton {button = instantiatedButton, resolution = new Resolution {height = resolution.height, width = resolution.width}, textPlaceholder = instantiatedButton.transform.GetChild(0).GetComponent<Text>()};
				buttonItem.textPlaceholder.text = stringResolutions[i];
				gameOptions.resolutionButtons.Add(buttonItem);
				instantiatedButton.gameObject.SetActive(true);
			}
		}

		public void SetWindowMode(string value, int index)
		{
			if(gameOptions.resolutionButtons.Count == 0) return;
			
			if (!PlayerPrefs.HasKey(value))
			{
				PlayerPrefs.SetInt(value, 0);
				index = 0;
			}
			else
			{
				if(index == - 1)
					index = PlayerPrefs.GetInt(value);
					
				PlayerPrefs.SetInt(value, index);
			}

			UIHelper.SetWindowMode(index, gameOptions.resolutionButtons[PlayerPrefs.GetInt("CurrentResolutionButton")].resolution);
			UIHelper.ResetSettingsButtons(new List<UIHelper.GameOptions.SettingsButton>{gameOptions.fullscreenMode, gameOptions.windowedMode}, index);
		}

		public void SetSettingsParameter(List<UIHelper.GameOptions.SettingsButton> settingsButtons, int index, string value, string type)
		{
			if(settingsButtons.Count == 0) return;
			
			if (!PlayerPrefs.HasKey(value))
			{
				var newValue = 0;

				if (!Application.isMobilePlatform)
					newValue = settingsButtons.Count - 1;
				
				PlayerPrefs.SetInt(value, newValue);
				index = newValue;
			}
			else
			{
				if (index == -1)
				{
					index = PlayerPrefs.GetInt(value);
				}
			}
			
			if (index > settingsButtons.Count - 1)
				index = settingsButtons.Count - 1;
			
			if(index != -1)
				PlayerPrefs.SetInt(value, index);

			if (type == "resolution")
			{
				var windowMode = PlayerPrefs.HasKey("CurrentWindowModeButton") ? (PlayerPrefs.GetInt("CurrentWindowModeButton") == 0 ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed) : FullScreenMode.ExclusiveFullScreen;
				UIHelper.SetResolution(settingsButtons[index].resolution, windowMode, QualitySettings.vSyncCount);
			}
			else if(type == "quality") UIHelper.SetQuality(settingsButtons[index].qualitySettings);
			else if (type == "framerate") UIHelper.SetFrameRate(settingsButtons[index].frameRate);

			UIHelper.ResetSettingsButtons(settingsButtons, index);
		}
		

		public void HideAllMultiplayerLobbyUI()
		{
			basicMultiplayerGameLobby.MainMenu.DisableAll();
			basicMultiplayerGameLobby.MapsMenu.DisableAll();
			basicMultiplayerGameLobby.CharactersMenu.DisableAll();
			basicMultiplayerGameLobby.AllRoomsMenu.DisableAll();
			basicMultiplayerGameLobby.CreateRoomMenu.DisableAll();
			basicMultiplayerGameLobby.AvatarsMenu.DisableAll();

			advancedMultiplayerGameLobby.MainMenu.DisableAll();
			advancedMultiplayerGameLobby.LoadoutMenu.DisableAll();
			advancedMultiplayerGameLobby.MapsMenu.DisableAll();
			advancedMultiplayerGameLobby.GameModesMenu.DisableAll();
			advancedMultiplayerGameLobby.AvatarsMenu.DisableAll();
			advancedMultiplayerGameLobby.CharactersMenu.DisableAll();
			advancedMultiplayerGameLobby.AllRoomsMenu.DisableAll();
			advancedMultiplayerGameLobby.CreateRoomMenu.DisableAll();
			advancedMultiplayerGameLobby.ProfileMenu.DisableAll();
			
			gameOptions.DisableAll();
		}

		public void HideAllMultiplayerRoomUI()
		{
			advancedMultiplayerGameRoom.SpectateMenu.DisableAll();
			advancedMultiplayerGameRoom.MatchStats.DisableAll();
			advancedMultiplayerGameRoom.GameOverMenu.DisableAll();
			advancedMultiplayerGameRoom.PauseMenu.DisableAll();
			advancedMultiplayerGameRoom.StartMenu.DisableAll();
			advancedMultiplayerGameRoom.TimerBeforeMatch.DisableAll();
			advancedMultiplayerGameRoom.TimerAfterDeath.DisableAll();
			advancedMultiplayerGameRoom.PreMatchMenu.DisableAll();
			
#if UNITY_EDITOR
			hierarchy[42].SetActive(false);
			hierarchy[29].SetActive(false);
			hierarchy[30].SetActive(false);
			hierarchy[39].SetActive(false);
			hierarchy[33].SetActive(false);
			hierarchy[36].SetActive(false);
#endif
           
			if(advancedMultiplayerGameRoom.MatchStats.KillDeathStatsScrollRect)
				advancedMultiplayerGameRoom.MatchStats.KillDeathStatsScrollRect.gameObject.SetActive(false);
			
			basicMultiplayerGameRoom.PauseMenu.DisableAll();
			basicMultiplayerGameRoom.MatchStats.DisableAll();
			basicMultiplayerGameRoom.GameOverMenu.DisableAll();
			
		}

		public void HideAllSinglePlayerMenus()
		{
			SinglePlayerGame.SinglePlayerGamePause.DisableAll();
			SinglePlayerGame.SinglePlayerGameGameOver.DisableAll();
			gameOptions.DisableAll();
		}
		
#if UNITY_EDITOR
		public void HideAllHierarchy()
		{
			foreach (var obj in hierarchy)
			{
				if (obj)
				{
					obj.hideFlags = HideFlags.HideInHierarchy;
					
					if (!Application.isPlaying || Application.isPlaying && gameObject.scene.name == "UI Manager")
						obj.SetActive(false);
				}
			}
		}
#endif

		private void Update()
		{
			
#if UNITY_EDITOR
			foreach (var item in hierarchy)
			{
				if (!item) continue;

				if (!item.activeInHierarchy && item.hideFlags != HideFlags.HideInHierarchy)
					item.hideFlags = HideFlags.HideInHierarchy;
			}


			foreach (var obj in hideObjects.Where(obj => obj && obj.hideFlags != HideFlags.HideInHierarchy))
			{
				obj.hideFlags = HideFlags.HideInHierarchy;
			}

			UIHelper.OpenPagesInPlayMode(this);
#endif

#if USK_MULTIPLAYER
			if (!lobbyManager
#if USK_ADVANCED_MULTIPLAYER
			    && !advancedLobbyManager
#endif
			)
#endif
			{
				if (gameManager && gameManager.controllers.Count > 0)
				{
					if (gameManager.minimapParameters.useMinimap)
						PlaceAndRotateMinimap(gameManager.controllers[gameManager.CurrentCharacter].CameraController.MainCamera.transform, gameManager.controllers[gameManager.CurrentCharacter].transform, gameManager.minimapParameters.mapScale, gameManager.minimapParameters.rotateMinimap);
					else
					{
						if (CharacterUI.mapMask && CharacterUI.mapMask.gameObject.activeSelf)
							CharacterUI.mapMask.gameObject.SetActive(false);
					}
				}
#if USK_MULTIPLAYER
				else if (roomManager && roomManager.Player && roomManager.controller && roomManager.controller.health > 0)
				{
					if (roomManager.minimapParameters.useMinimap)
						PlaceAndRotateMinimap(roomManager.controller.CameraController.MainCamera.transform, roomManager.controller.transform, roomManager.minimapParameters.mapScale, roomManager.minimapParameters.rotateMinimap);
					else if (CharacterUI.mapMask && CharacterUI.mapMask.gameObject.activeSelf)
						CharacterUI.mapMask.gameObject.SetActive(false);
				}
#endif
#if USK_ADVANCED_MULTIPLAYER
				else if (advancedRoomManager && advancedRoomManager.Player && !advancedRoomManager.matchIsOver && advancedRoomManager.controller && advancedRoomManager.controller.health > 0)
				{
					if (advancedRoomManager.useMinimap)
						PlaceAndRotateMinimap(advancedRoomManager.controller.CameraController.MainCamera.transform, advancedRoomManager.controller.transform, advancedRoomManager.mapScale, advancedRoomManager.rotateMinimap);
					else if (CharacterUI.mapMask && CharacterUI.mapMask.gameObject.activeSelf)
						CharacterUI.mapMask.gameObject.SetActive(false);
				}
#endif
			}

			InputHelper.CheckGamepad(ref useGamepad, projectSettings);

			GetCurrentMenuType();

			var scopeTextureCurrentRect = CharacterUI.aimPlaceholder.rectTransform.rect;
			var curRect = fillParentRect.rect;

			// if(curRect.height > curRect.width)
				CharacterUI.aimPlaceholder.rectTransform.sizeDelta = new Vector2(curRect.height, CharacterUI.aimPlaceholder.rectTransform.sizeDelta.y);
			
			if (leftScopeTextureFill)
			{
				leftScopeTextureFill.offsetMin = curRect.min;
				leftScopeTextureFill.offsetMax = new Vector2(-scopeTextureCurrentRect.height / 2, curRect.yMax);
			}
			
			if (rightScopeTextureFill)
			{
				rightScopeTextureFill.offsetMin = new Vector2(scopeTextureCurrentRect.height / 2, curRect.yMin);
				rightScopeTextureFill.offsetMax = curRect.max;
			}
			
			frameCount++;
			deltaTime += Time.unscaledDeltaTime;

			if (deltaTime > 1)
			{
				currentFrameRate = frameCount / deltaTime;
				frameCount = 0;
				deltaTime -= 1;
			}

			if (gameOptions.frameRate)
			{
				if (currentFrameRate > Application.targetFrameRate - 3) currentFrameRate = Application.targetFrameRate - 3;

				if (currentFrameRate < 0 && gameOptions.frameRate.gameObject.activeSelf)
					gameOptions.frameRate.gameObject.SetActive(false);
				else if (currentFrameRate > 0)
					gameOptions.frameRate.text = string.Format("{0} fps", (int) (currentFrameRate));
			}


			if (gamepadCursor)
			{
				gamepadCursor.gameObject.SetActive(useGamepad && currentMenuType != "null" && currentMenuType != "characters" && currentMenuType != "createRoom" && currentButton
#if USK_ADVANCED_MULTIPLAYER && USK_MULTIPLAYER
				                                   && !advancedRoomManager
#endif
				);
			}

			if (currentMenuType != "mainMenu")
			{
				if (Gamepad.current != null && InputHelper.WasGamepadButtonPressed(Gamepad.current.buttonEast)) //Input.GetKeyDown(KeyCode.Joystick1Button2))
				{
#if USK_MULTIPLAYER
					if (lobbyManager)
						lobbyManager.OpenMenu("mainMenu");
#endif
#if USK_ADVANCED_MULTIPLAYER
					if (advancedLobbyManager)
						advancedLobbyManager.OpenMenu("mainMenu");
#endif
				}
			}

			// Cursor.visible = !useGamepad;
			// }

// #endif

			ManageButtonsInCurrentMenu();

			if (!useGamepad)
				return;

			GamepadNavigation();
		}

		private Vector3 GetUIPosition(Transform objectTransform)
		{
			var correctedObjectPos = new Vector3(objectTransform.position.x, 0, objectTransform.position.z);
			var correctedMapPosition = new Vector3(mapCenterPosition.x, 0, mapCenterPosition.z);
			
			var direction = correctedObjectPos - correctedMapPosition;
			var distance = Vector3.Distance(correctedObjectPos, correctedMapPosition);
			
			direction = convertDirectionObject.transform.InverseTransformDirection(direction);
			
			var screenDirection = new Vector3(direction.x, direction.z, 0);
			
			screenDirection = CharacterUI.mapPlaceholder.transform.rotation * screenDirection;
			
			var position = CharacterUI.mapPlaceholder.transform.position + screenDirection.normalized * (distance * mapDistanceScale);

			return position;
		}

		public void SetBlipRotation(Transform mainTransform, RawImage blipImage)
		{
			blipImage.transform.eulerAngles = new Vector3(0, 0, -mainTransform.eulerAngles.y - deltaAngle + convertDirectionObject.eulerAngles.y);
		}

		public void SetBlipPosition(Transform transform, RawImage blipImage, Vector3 position, bool blipsAreAlwaysVisible)
		{
			if (blipsAreAlwaysVisible)
			{
				var corners = new Vector3[4];

				CharacterUI.mapMask.rectTransform.GetWorldCorners(corners);
				
				if (CharacterUI.currentMiniMapType == UIHelper.MiniMapType.Circle)
				{
					var direction = position - CharacterUI.mapMask.transform.position;
					var rectTransform = CharacterUI.mapMask.rectTransform;
					
					var size = Vector3.Distance(corners[0], corners[1]);

					var position1 = rectTransform.position;
					var border = position1 + direction.normalized * size / 2;
					
					var distance = Vector3.Distance(position, position1);
					var distance2 = Vector3.Distance(border, position1);

					if (Mathf.Abs(distance2) < Mathf.Abs(distance))
						position = border;
				}
				else
				{
					position.x = Mathf.Clamp(position.x, corners[0].x, corners[2].x);
					position.y = Mathf.Clamp(position.y, corners[0].y, corners[2].y);
				}

				if (blipImage.transform.parent != CharacterUI.mapMask.transform.parent)
					blipImage.transform.SetParent(CharacterUI.mapMask.transform.parent);

				blipImage.transform.position = position;
			}
			else
			{
				blipImage.transform.position = position;

				if (blipImage.transform.parent != CharacterUI.mapMask.transform)
					blipImage.transform.SetParent(CharacterUI.mapMask.transform);
			}
		}

		public void SetBlip(Transform mainTransform, string type, UIHelper.MinimapImage minimapImage = null, RawImage rawImage = null)
		{
			if(!convertDirectionObject) return;
			
			var blipImage = minimapImage != null ? minimapImage.image : rawImage;
			
			if (gameManager && gameManager.minimapParameters.useMinimap 
			    
#if  USK_MULTIPLAYER
			    || roomManager && roomManager.minimapParameters.useMinimap
#if USK_ADVANCED_MULTIPLAYER 
			    || advancedRoomManager && advancedRoomManager.useMinimap
#endif
#endif
			)
			{
				var blipsScale = 1f;

				if (gameManager)
					blipsScale = gameManager.minimapParameters.blipsScale;
#if USK_ADVANCED_MULTIPLAYER
				else if(advancedRoomManager)
					blipsScale = advancedRoomManager.blipsScale;

#endif

				// var screenDelta =  Screen.width / 1920;
				blipImage.rectTransform.localScale = new Vector3(1, 1, 1);
				
				if (minimapImage != null && minimapImage.upArrow && minimapImage.downArrow)
				{
					if (Mathf.Abs(mainTransform.position.y - characterHeight) > 1)
					{
						if (mainTransform.position.y > characterHeight)
						{
							minimapImage.upArrow.gameObject.SetActive(true);
							minimapImage.downArrow.gameObject.SetActive(false);
						}
						else
						{
							minimapImage.upArrow.gameObject.SetActive(false);
							minimapImage.downArrow.gameObject.SetActive(true);
						}
					}
					else
					{
						minimapImage.upArrow.gameObject.SetActive(false);
						minimapImage.downArrow.gameObject.SetActive(false);
					}
				}
				
				if (type != "positionOnly")
				{
					if (type != "rotationOnly")
					{
						if (gameManager && gameManager.minimapParameters.rotateBlips 
#if USK_MULTIPLAYER
						    || roomManager && roomManager.minimapParameters.rotateBlips
#if USK_ADVANCED_MULTIPLAYER
						    || advancedRoomManager && advancedRoomManager.rotateBlips
#endif
#endif
						)
						{
							SetBlipRotation(mainTransform, blipImage);
						}
						
						else blipImage.transform.eulerAngles = Vector3.zero;
					}
				}
				else
				{
					blipImage.transform.eulerAngles = Vector3.zero;
				}

				if (type != "rotationOnly")
				{
					var position = GetUIPosition(mainTransform);

					if (gameManager && gameManager.minimapParameters.blipsAreAlwaysVisible 
#if	USK_MULTIPLAYER
					    || roomManager && roomManager.minimapParameters.blipsAreAlwaysVisible
#if USK_ADVANCED_MULTIPLAYER
					    || advancedRoomManager && advancedRoomManager.blipsAreAlwaysVisible
#endif
#endif
					    )
					{
						if (gameManager && gameManager.minimapParameters.scaleBlipsByDistance 
#if USK_MULTIPLAYER
						    || roomManager && roomManager.minimapParameters.scaleBlipsByDistance
#if USK_ADVANCED_MULTIPLAYER
						    || advancedRoomManager && advancedRoomManager.scaleBlipsByDistance
#endif
#endif
						    )
						{
							var distance = Vector3.Distance(CharacterUI.mapMask.transform.position, position);
							
							// var distance = Vector3.Distance(mapCenterPosition, new Vector3(mainTransform.position.x, 0, mainTransform.position.z));
							var targetDistance = 1f;
							
							if (gameManager)
								targetDistance = gameManager.minimapParameters.blipsVisibleDistance * mapDistanceScale;
#if USK_MULTIPLAYER
							else if(roomManager)
								targetDistance = roomManager.minimapParameters.blipsVisibleDistance * mapDistanceScale;
#if USK_ADVANCED_MULTIPLAYER
							else if(advancedRoomManager)
								targetDistance = advancedRoomManager.blipsVisibleDistance * mapDistanceScale;
#endif
#endif

							if (distance > targetDistance)
							{
								var difference = distance - targetDistance;

								blipImage.rectTransform.sizeDelta = new Vector2(30, 30) * blipsScale - new Vector2(difference, difference) * blipsScale / 2;

								if (blipImage.rectTransform.sizeDelta.x < 0)
									blipImage.rectTransform.sizeDelta = Vector2.zero;
							}
							else
							{
								blipImage.rectTransform.sizeDelta = new Vector2(30, 30) * blipsScale;
							}
						}
						else
						{
							blipImage.rectTransform.sizeDelta = new Vector2(30, 30) * blipsScale;
						}
					}
					else
					{
						blipImage.rectTransform.sizeDelta = new Vector2(30, 30) * blipsScale;
					}

					if (minimapImage != null && minimapImage.upArrow)
					{
						var sizeDelta = blipImage.rectTransform.sizeDelta / 2;
						minimapImage.upArrow.rectTransform.sizeDelta = sizeDelta;
						minimapImage.downArrow.rectTransform.sizeDelta = sizeDelta;
					}

					if(gameManager)
						SetBlipPosition(mainTransform, blipImage, position, gameManager.minimapParameters.blipsAreAlwaysVisible);
#if USK_MULTIPLAYER
					else if (roomManager)
						SetBlipPosition(mainTransform, blipImage, position, roomManager.minimapParameters.blipsAreAlwaysVisible);
#if USK_ADVANCED_MULTIPLAYER
					else if (advancedRoomManager)
						SetBlipPosition(mainTransform, blipImage, position, advancedRoomManager.blipsAreAlwaysVisible);
#endif
#endif
				}
				else
				{
					blipImage.transform.localPosition = Vector3.zero;
					blipImage.rectTransform.sizeDelta = new Vector2(30, 30) * blipsScale;
					
					if(gameManager && gameManager.minimapParameters.rotateMinimap 
#if USK_MULTIPLAYER
					   || roomManager && roomManager.minimapParameters.rotateMinimap
#if USK_ADVANCED_MULTIPLAYER
					   || advancedRoomManager && advancedRoomManager.rotateMinimap
#endif
#endif
					   )
						blipImage.transform.eulerAngles = Vector3.zero;
					else
						SetBlipRotation(mainTransform, blipImage);
				}
			}
			else
			{
				if (blipImage.gameObject.activeSelf)
					blipImage.gameObject.SetActive(false);
			}
		}

		void PlaceAndRotateMinimap(Transform cameraTransform, Transform controllerTransform, float scale, bool rotate)
		{
			if (CharacterUI.mapPlaceholder && CharacterUI.mapPlaceholder.texture && CharacterUI.mapMask)
			{
				if (rotate)
				{
					deltaAngle = Mathf.DeltaAngle(cameraTransform.eulerAngles.y, convertDirectionObject.transform.eulerAngles.y);
					CharacterUI.mapPlaceholder.transform.eulerAngles = new Vector3(0, 0, -deltaAngle);
				}

				CharacterUI.mapPlaceholder.rectTransform.sizeDelta = new Vector2(512, 512) * scale;
					
				var correctedMapPos = new Vector3(mapCenterPosition.x, 0, mapCenterPosition.z);

				characterHeight = controllerTransform.position.y;
				
				var correctedPlayerPos = new Vector3(controllerTransform.position.x, 0, controllerTransform.position.z);

				var corners = new Vector3[4];
				CharacterUI.mapPlaceholder.rectTransform.GetWorldCorners(corners);

				mapCornerPosition.y = 0;
				
				var worldScale = Vector3.Distance(correctedMapPos, mapCornerPosition);
				var screenScale = Vector3.Distance(CharacterUI.mapPlaceholder.transform.position, corners[0]);
				
				mapDistanceScale = screenScale / worldScale;

				var distance = Vector3.Distance(correctedMapPos, correctedPlayerPos);
				var direction =  (correctedMapPos - correctedPlayerPos).normalized;
				
				direction = convertDirectionObject.transform.InverseTransformDirection(direction);
					
				var screenDirection = new Vector3(direction.x, direction.z, 0);
				var transform1 = CharacterUI.mapPlaceholder.transform;
				
				screenDirection = transform1.rotation * screenDirection;
				
				var position = CharacterUI.mapMask.transform.position + screenDirection.normalized * (distance * mapDistanceScale);
				
				transform1.position = position;
			}
		}
		
		void ManageButtonsInCurrentMenu()
		{
			switch (currentMenuType)
			{
#if USK_ADVANCED_MULTIPLAYER
				case "mainMenu":
				
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(currentButton && currentButton.interactable)
							currentButton.onClick.Invoke();
					}
					
					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);


					break;
				
				case "loadout":
				
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(advancedMultiplayerGameLobby.LoadoutMenu.equipButton)
							advancedMultiplayerGameLobby.LoadoutMenu.equipButton.onClick.Invoke();
					}
					
					if(advancedMultiplayerGameLobby.LoadoutMenu.scrollRect)
						ScrollRectFitPosition(advancedMultiplayerGameLobby.LoadoutMenu.scrollRect, ref advancedMultiplayerGameLobby.LoadoutMenu.firstTimeMenuOpened);
					
					break;
				
				case "gameModes":
				{
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameLobby.GameModesMenu.MapsButton)
							advancedMultiplayerGameLobby.GameModesMenu.MapsButton.onClick.Invoke();
					}
					
					if(advancedMultiplayerGameLobby.GameModesMenu.scrollRect)
						ScrollRectFitPosition(advancedMultiplayerGameLobby.GameModesMenu.scrollRect, ref advancedMultiplayerGameLobby.GameModesMenu.firstTimeMenuOpened);

					break;
				}
				case "maps":
				
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameLobby.MapsMenu.gameModesButton)
							advancedMultiplayerGameLobby.MapsMenu.gameModesButton.onClick.Invoke();
					}
					
					if(advancedMultiplayerGameLobby.MapsMenu.scrollRect)
						ScrollRectFitPosition(advancedMultiplayerGameLobby.MapsMenu.scrollRect, ref advancedMultiplayerGameLobby.MapsMenu.firstTimeMenuOpened);
					
					break;
				
				case "avatars":
					
					if(advancedMultiplayerGameLobby.AvatarsMenu.scrollRect)
						ScrollRectFitPosition(advancedMultiplayerGameLobby.AvatarsMenu.scrollRect, ref advancedMultiplayerGameLobby.AvatarsMenu.firstTimeMenuOpened);
					
					break;
				
				case "allRooms":
					
					bool empty = false;
					
					if(advancedMultiplayerGameLobby.AllRoomsMenu.scrollRect)
						ScrollRectFitPosition(advancedMultiplayerGameLobby.AllRoomsMenu.scrollRect, ref empty);
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(currentButton)
							currentButton.onClick.Invoke();
					}
					
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameLobby.AllRoomsMenu.JoinButton && advancedMultiplayerGameLobby.AllRoomsMenu.JoinButton.gameObject.activeSelf)
							advancedMultiplayerGameLobby.AllRoomsMenu.JoinButton.onClick.Invoke();
					}

					if (roomsCount != advancedMultiplayerGameLobby.AllRoomsMenu.scrollRect.content.childCount)
					{
						roomsCount = advancedMultiplayerGameLobby.AllRoomsMenu.scrollRect.content.childCount;
						ResetRoomsButtons(advancedLobbyManager ? "advanced" : "basic");
					}
					
					break;
				
				case "createRoom":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(advancedMultiplayerGameLobby.CreateRoomMenu.CreateButton)
							advancedMultiplayerGameLobby.CreateRoomMenu.CreateButton.onClick.Invoke();
					}
					
					break;
				
				case "multiplayerTeamsPause":
				case "multiplayerNotTeamsPause":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameRoom.PauseMenu.exitButton)
							advancedMultiplayerGameRoom.PauseMenu.exitButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button2))
					{
						if(advancedMultiplayerGameRoom.PauseMenu.resumeButton && advancedMultiplayerGameRoom.PauseMenu.resumeButton.gameObject.activeSelf)
							advancedMultiplayerGameRoom.PauseMenu.resumeButton.onClick.Invoke();
						
						else if(advancedMultiplayerGameRoom.GameOverMenu.backButton && advancedMultiplayerGameRoom.GameOverMenu.backButton.gameObject.activeSelf)
							advancedMultiplayerGameRoom.GameOverMenu.backButton.onClick.Invoke();
						
						else if(advancedMultiplayerGameRoom.SpectateMenu.BackButton && advancedMultiplayerGameRoom.SpectateMenu.BackButton.gameObject.activeSelf)
							advancedMultiplayerGameRoom.SpectateMenu.BackButton.onClick.Invoke();
					}
					
					
//					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);
					
//					ScrollRectFitPosition(MultiplayerGameRoom.PauseMenu.NotTeamsScrollRect);

					break;
				
				case "multiplayerStart":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameRoom.StartMenu.ExitButton)
							advancedMultiplayerGameRoom.StartMenu.ExitButton.onClick.Invoke();
					}
					
					break;
				
				case "multiplayerTimerAfterDeath":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(advancedMultiplayerGameRoom.TimerAfterDeath.LaunchButton)
							advancedMultiplayerGameRoom.TimerAfterDeath.LaunchButton.onClick.Invoke();
					}
					
					break;
				
				case "multiplayerSpectate":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(advancedMultiplayerGameRoom.SpectateMenu.ChangeCameraButton)
							advancedMultiplayerGameRoom.SpectateMenu.ChangeCameraButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameRoom.SpectateMenu.ExitButton)
							advancedMultiplayerGameRoom.SpectateMenu.ExitButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button3))
					{
						if(advancedMultiplayerGameRoom.SpectateMenu.MatchStatsButton)
							advancedMultiplayerGameRoom.SpectateMenu.MatchStatsButton.onClick.Invoke();
					}

					break;
				
				case "multiplayerGameOver":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(advancedMultiplayerGameRoom.GameOverMenu.playAgainButton)
							advancedMultiplayerGameRoom.GameOverMenu.playAgainButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button0))
					{
						if(advancedMultiplayerGameRoom.GameOverMenu.exitButton)
							advancedMultiplayerGameRoom.GameOverMenu.exitButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button3))
					{
						if(advancedMultiplayerGameRoom.GameOverMenu.matchStatsButton)
							advancedMultiplayerGameRoom.GameOverMenu.matchStatsButton.onClick.Invoke();
					}

					break;
#endif
				
				case "gameOptions":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						currentButton.onClick.Invoke();
					}
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button2))
					{
						if(gameOptions.back)
							gameOptions.back.onClick.Invoke();
					}

					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);
					
					if(gameOptions.resolutionsScrollRect)
						ScrollRectFitPosition(gameOptions.resolutionsScrollRect, ref gameOptions.firstTimeMenuOpened);
					
					break;
					
				case "singlePlayerPause":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						currentButton.onClick.Invoke();
					}

					if (Input.GetKeyDown(KeyCode.Joystick1Button2))
					{
						if(SinglePlayerGame.SinglePlayerGamePause.Resume)
							SinglePlayerGame.SinglePlayerGamePause.Resume.onClick.Invoke();
					}

					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);
					
					break;
				
				case "singlePlayerGameOver":
					
					if (Input.GetKeyDown(KeyCode.Joystick1Button1))
					{
						if(currentButton)
							currentButton.onClick.Invoke();
					}
					
					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);

					break;
			}
		}


		void GamepadNavigation()
		{
			if(Gamepad.current == null) return;
			
			if (canSwitchButton)
			{
				if (currentMenuType != "characters")
				{
					if (Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[3].x.ReadValue()) > 0.3f || Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[3].y.ReadValue()) > 0.3f)
					{
						canSwitchButton = false;

						var direction = projectSettings.gamepadAxisControlsInUnityInputSystem[3].ReadValue().normalized;
						var suitableButtons = new List<Button>();
						var currentDirs = new List<Vector2>();

						foreach (var button in allButtonsInCurrentMenu)
						{
							var currentDirection = button.transform.position - currentButton.transform.position;

							if (Vector2.Dot(currentDirection.normalized, direction.normalized) > 0.95f)
							{
								suitableButtons.Add(button);
								currentDirs.Add(currentDirection);
							}
						}

						Button nextButton = null;
						var dist = float.MaxValue;

						for (var i = 0; i < suitableButtons.Count; i++)
						{
							var button = suitableButtons[i];
							var currentDist = Vector2.Distance(button.transform.position, currentButton.transform.position);

							if (currentDist < dist)
							{
								dist = currentDist;
								nextButton = button;
							}
						}

						if (nextButton != null)
						{
							currentButton = nextButton;

							if (currentMenuType == "allRooms")
							{
								currentRoom = nextButton;
								roomIndex = allButtonsInCurrentMenu.IndexOf(currentButton);
							}

							if (gamepadCursor)
							{
								targetPosition = currentButton.transform.position;

								if (currentMenuType == "loadout" || currentMenuType == "gameModes" || currentMenuType == "maps" || currentMenuType == "avatars")
								{
									currentButton.onClick.Invoke();
								}
								else if (currentMenuType == "mainMenu")
								{
									advancedMultiplayerGameLobby.MainMenu.currentSelectedButton = currentButton;
								}
								else if (currentMenuType == "singlePlayerPause")
								{
									SinglePlayerGame.SinglePlayerGamePause.currentSelectedButton = currentButton;
								}
								else if (currentMenuType == "gameOptions")
								{
									gameOptions.currentSelectedSettingsButton = currentButton;
								}
							}
						}
					}
				}
				else
				{
					if (projectSettings.gamepadAxisControlsInUnityInputSystem[3].x.ReadValue() > 0.3f)
					{
						canSwitchButton = false;

						if(advancedMultiplayerGameLobby.CharactersMenu.upButton)
							advancedMultiplayerGameLobby.CharactersMenu.upButton.onClick.Invoke();
					}
					else if (projectSettings.gamepadAxisControlsInUnityInputSystem[3].x.ReadValue() < -0.3f)
					{
						canSwitchButton = false;

						if(advancedMultiplayerGameLobby.CharactersMenu.downButton)
							advancedMultiplayerGameLobby.CharactersMenu.downButton.onClick.Invoke();
					}
				}
			}
			
			if (Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[3].x.ReadValue()) < 0.3f && Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[3].y.ReadValue()) < 0.3f)
			{
				canSwitchButton = true;
			}
		}

		void GetCurrentMenuType()
		{
#if USK_MULTIPLAYER
			if (lobbyManager)
			{

				if (basicMultiplayerGameLobby.CharactersMenu.mainObject && basicMultiplayerGameLobby.CharactersMenu.mainObject.activeSelf)
					currentMenuType = "characters";
				
				else if (basicMultiplayerGameLobby.AllRoomsMenu.MainObject && basicMultiplayerGameLobby.AllRoomsMenu.MainObject.activeSelf)
					currentMenuType = "allRooms";
				
				else if (basicMultiplayerGameLobby.CreateRoomMenu.MainObject && basicMultiplayerGameLobby.CreateRoomMenu.MainObject.activeSelf)
					currentMenuType = "createRoom";

				else if (basicMultiplayerGameLobby.MapsMenu.MainObject && basicMultiplayerGameLobby.MapsMenu.MainObject.activeSelf)
					currentMenuType = "maps";
				else 
					currentMenuType = "mainMenu";
			}
#endif
			
#if USK_ADVANCED_MULTIPLAYER
			if (advancedLobbyManager)
			{
				if (advancedMultiplayerGameLobby.AvatarsMenu.MainObject && advancedMultiplayerGameLobby.AvatarsMenu.MainObject.activeSelf)
					currentMenuType = "avatars";
				else if (advancedMultiplayerGameLobby.LoadoutMenu.mainObject && advancedMultiplayerGameLobby.LoadoutMenu.mainObject.activeSelf)
					currentMenuType = "loadout";
				else if (advancedMultiplayerGameLobby.CharactersMenu.mainObject && advancedMultiplayerGameLobby.CharactersMenu.mainObject.activeSelf)
					currentMenuType = "characters";
				else if (advancedMultiplayerGameLobby.AllRoomsMenu.MainObject && advancedMultiplayerGameLobby.AllRoomsMenu.MainObject.activeSelf)
					currentMenuType = "allRooms";
				else if (advancedMultiplayerGameLobby.CreateRoomMenu.MainObject && advancedMultiplayerGameLobby.CreateRoomMenu.MainObject.activeSelf)
					currentMenuType = "createRoom";
				else if (advancedMultiplayerGameLobby.ProfileMenu.mainObject && advancedMultiplayerGameLobby.ProfileMenu.mainObject.activeSelf)
					currentMenuType = "profile";
				else if (advancedMultiplayerGameLobby.GameModesMenu.MainObject && advancedMultiplayerGameLobby.GameModesMenu.MainObject.activeSelf)
					currentMenuType = "gameModes";
				else if (advancedMultiplayerGameLobby.MapsMenu.MainObject && advancedMultiplayerGameLobby.MapsMenu.MainObject.activeSelf)
					currentMenuType = "maps";
				else if (gameOptions.MainObject && gameOptions.MainObject.activeSelf)
					currentMenuType = "gameOptions";
				else currentMenuType = "mainMenu";
			}
			else if (advancedRoomManager)
			{
				if (advancedMultiplayerGameRoom.StartMenu.MainObject && advancedMultiplayerGameRoom.StartMenu.MainObject.activeSelf)
					currentMenuType = "multiplayerStart";
				else if (advancedMultiplayerGameRoom.PauseMenu.teamsPauseMenuMain && advancedMultiplayerGameRoom.PauseMenu.teamsPauseMenuMain.activeSelf)
					currentMenuType = "multiplayerTeamsPause";
				else if (advancedMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain && advancedMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain.activeSelf)
					currentMenuType = "multiplayerNotTeamsPause";
				else if (advancedMultiplayerGameRoom.TimerAfterDeath.MainObject && advancedMultiplayerGameRoom.TimerAfterDeath.MainObject.activeSelf)
					currentMenuType = "multiplayerTimerAfterDeath";
				else if(advancedMultiplayerGameRoom.SpectateMenu.PlayerStats && advancedMultiplayerGameRoom.SpectateMenu.PlayerStats.gameObject.activeSelf)
				        currentMenuType = "multiplayerSpectate";
				else if(advancedMultiplayerGameRoom.GameOverMenu.exitButton && advancedMultiplayerGameRoom.GameOverMenu.exitButton.gameObject.activeSelf)
				        currentMenuType = "multiplayerGameOver";
				else if (gameOptions.MainObject && gameOptions.MainObject.activeSelf)
					currentMenuType = "gameOptions";
				else currentMenuType = "null";
			} else
#endif
			if (gameManager)
			{
				if(SinglePlayerGame.SinglePlayerGamePause.MainObject && SinglePlayerGame.SinglePlayerGamePause.MainObject.activeSelf)
					currentMenuType = "singlePlayerPause";
				else if (gameOptions.MainObject && gameOptions.MainObject.activeSelf)
					currentMenuType = "gameOptions";
				else if (SinglePlayerGame.SinglePlayerGameGameOver.MainObject && SinglePlayerGame.SinglePlayerGameGameOver.MainObject.activeSelf)
					currentMenuType = "singlePlayerGameOver";
				else
					currentMenuType = "null";
			}

			if (lastMenuType != currentMenuType)
			{
				SwitchMenu(currentMenuType 
#if USK_MULTIPLAYER
					, lobbyManager ? "basic" : "advanced"
#endif
				);
				lastMenuType = currentMenuType;
			}
		}

		public void SwitchMenu(string type, string multiplayerType = null)
		{
			var buttons = new List<Button>();
			currentMenuType = type;
			
			switch (type)
			{
				
#if USK_MULTIPLAYER
#if USK_ADVANCED_MULTIPLAYER
				case "gameModes":
					
					if (advancedMultiplayerGameLobby.GameModesMenu.scrollRect)
					{
						foreach (Transform child in advancedMultiplayerGameLobby.GameModesMenu.scrollRect.content)
						{
							if (child.gameObject.GetComponent<UIPreset>() && child.gameObject.GetComponent<UIPreset>().Button)
								buttons.Add(child.gameObject.GetComponent<UIPreset>().Button);
						}

						allButtonsInCurrentMenu = buttons;
						currentButton = advancedLobbyManager.AllGameModesPlaceholders[advancedLobbyManager.selectedGameModeIndex].Button;
						
						UpdateRectPos(advancedMultiplayerGameLobby.GameModesMenu.scrollRect, ref advancedMultiplayerGameLobby.GameModesMenu.firstTimeMenuOpened, "horizontal");
					}

					break;

				case "loadout":
					
					if (advancedMultiplayerGameLobby.LoadoutMenu.scrollRect)
					{
						foreach (Transform child in advancedMultiplayerGameLobby.LoadoutMenu.scrollRect.content)
						{
							if (child.gameObject.GetComponent<UIPreset>() && child.gameObject.GetComponent<UIPreset>().Button)
								buttons.Add(child.gameObject.GetComponent<UIPreset>().Button);
						}

						allButtonsInCurrentMenu = buttons;

						if (advancedLobbyManager.AllWeaponsPlaceholders.Count > 0)
							currentButton = advancedLobbyManager.AllWeaponsPlaceholders[advancedLobbyManager.selectedWeaponIndex].Button;
						else currentButton = null;
						
						UpdateRectPos(advancedMultiplayerGameLobby.LoadoutMenu.scrollRect, ref advancedMultiplayerGameLobby.LoadoutMenu.firstTimeMenuOpened, "vertical");
					}

					break;

#endif
				
				case "avatars":
					
					var avatarsMenu = multiplayerType == "advanced" ? advancedMultiplayerGameLobby.AvatarsMenu : basicMultiplayerGameLobby.AvatarsMenu;
					
					if (avatarsMenu.scrollRect)
					{
						foreach (Transform child in avatarsMenu.scrollRect.content)
						{
							if (child.gameObject.GetComponent<UIPreset>() && child.gameObject.GetComponent<UIPreset>().Button)
								buttons.Add(child.gameObject.GetComponent<UIPreset>().Button);
						}

						allButtonsInCurrentMenu = buttons;
						currentButton =  
#if USK_ADVANCED_MULTIPLAYER
							multiplayerType == "advanced" ? advancedLobbyManager.AllAvatarsPlaceholders[advancedLobbyManager.gameData.avatarIndex].Button : 
#endif
							lobbyManager.allAvatarsPlaceholders[lobbyManager.currentAvatarIndex].Button;
						
						UpdateRectPos(avatarsMenu.scrollRect, ref avatarsMenu.firstTimeMenuOpened, "vertical");

					}

					break;
				
				case "maps":

					var mapsMenu = multiplayerType == "advanced" ? advancedMultiplayerGameLobby.MapsMenu : basicMultiplayerGameLobby.MapsMenu;

					if (mapsMenu.scrollRect)
					{
						foreach (Transform child in mapsMenu.scrollRect.content)
						{
							if (child.gameObject.GetComponent<UIPreset>() && child.gameObject.GetComponent<UIPreset>().Button)
								buttons.Add(child.gameObject.GetComponent<UIPreset>().Button);
						}

						allButtonsInCurrentMenu = buttons;


						if (
#if USK_ADVANCED_MULTIPLAYER
							multiplayerType == "advanced" && advancedLobbyManager.AllMapsPlaceholders.Count > 0 && advancedLobbyManager.gameData.selectedMapIndex != -1 || 
#endif
						    multiplayerType != "advanced" && lobbyManager.allMapsPlaceholders.Count > 0)
						{
							currentButton =
#if USK_ADVANCED_MULTIPLAYER
								multiplayerType == "advanced"
									? advancedLobbyManager.AllMapsPlaceholders[advancedLobbyManager.gameData.selectedMapIndex].Button
									:
#endif
									lobbyManager.allMapsPlaceholders[lobbyManager.currentMapIndex].Button;
						}
						else
						{
							currentButton = null;
						}

						UpdateRectPos(mapsMenu.scrollRect, ref mapsMenu.firstTimeMenuOpened, "horizontal");

					}

					break;
                
				case "allRooms":

					ResetRoomsButtons(multiplayerType);
					
					break;

				case "mainMenu":


					if (multiplayerType == "advanced")
					{
						allButtonsInCurrentMenu = advancedMultiplayerGameLobby.MainMenu.GetAllButtons();
						currentButton = advancedMultiplayerGameLobby.MainMenu.currentSelectedButton ? advancedMultiplayerGameLobby.MainMenu.currentSelectedButton : advancedMultiplayerGameLobby.MainMenu.ChangeCharacter;
					}
					else
					{
						allButtonsInCurrentMenu = basicMultiplayerGameLobby.MainMenu.GetAllButtons();
						currentButton = basicMultiplayerGameLobby.MainMenu.currentSelectedButton ? basicMultiplayerGameLobby.MainMenu.currentSelectedButton : basicMultiplayerGameLobby.MainMenu.ChangeCharacter;
					}

					break;
#endif
				
				case "singlePlayerPause":
					allButtonsInCurrentMenu = SinglePlayerGame.SinglePlayerGamePause.GetAllButtons();
					currentButton = SinglePlayerGame.SinglePlayerGamePause.currentSelectedButton ? SinglePlayerGame.SinglePlayerGamePause.currentSelectedButton : SinglePlayerGame.SinglePlayerGamePause.Resume;
					break;

				case "singlePlayerGameOver":
					allButtonsInCurrentMenu = SinglePlayerGame.SinglePlayerGameGameOver.GetAllOptionButtons();
					currentButton = SinglePlayerGame.SinglePlayerGameGameOver.Restart;
					break;
				
				case "gameOptions":
					allButtonsInCurrentMenu = gameOptions.GetAllOptionButtons();
					currentButton = gameOptions.currentSelectedSettingsButton ? gameOptions.currentSelectedSettingsButton : gameOptions.resolutionButtons[PlayerPrefs.GetInt("CurrentResolutionButton")].button;
					break;
			}
			
			if (currentButton && (currentMenuType == "loadout" || currentMenuType == "gameModes" || currentMenuType == "maps" || currentMenuType == "avatars"))
			{
				currentButton.onClick.Invoke();
			}

			StopAllCoroutines();

			if (currentButton)
			{
#if	USK_MULTIPLAYER
				if (lobbyManager) StartCoroutine(setCursorTimeout());
				else
#if USK_ADVANCED_MULTIPLAYER 
				if (advancedLobbyManager) StartCoroutine(setCursorTimeout());
				else 
#endif
#endif
				targetPosition = currentButton.transform.position;
			}
		}

		IEnumerator setCursorTimeout()
		{
			yield return new WaitForSeconds(0.05f);
			targetPosition = currentButton.transform.position;
			StopCoroutine(setCursorTimeout());
		}

		void ScrollRectFitPosition(ScrollRect scrollRect, ref bool firstTimeMenuOpened)
		{
			if(!useGamepad || currentButton.transform.parent != scrollRect.content)
				return;
			
			var buttonDirection = Vector2.zero;
			
			var type = scrollRect.horizontal ? "horizontal" : "vertical";

			if (firstTimeMenuOpened)
			{
				if (currentButton)
				{
					if (type == "horizontal")
					{
						if (currentButton.transform.position.x < scrollRect.viewport.transform.position.x + scrollRect.viewport.rect.width / 2)
							buttonDirection = new Vector2(-1, 0);
						else if (currentButton.transform.position.x > scrollRect.viewport.transform.position.x + scrollRect.viewport.rect.width / 2)
							buttonDirection = new Vector2(1, 0);


						var border = new Vector2(currentButton.transform.position.x + currentButton.GetComponent<RectTransform>().rect.width / 2 * (buttonDirection == new Vector2(1, 0) ? 1 : -1), currentButton.transform.position.y);

						if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRect.viewport, border))
						{
							scrollRect.horizontalNormalizedPosition += 0.05f * (buttonDirection == new Vector2(1, 0) ? 1 : -1);

							targetPosition = (buttonDirection == new Vector2(-1, 0) ? new Vector2(scrollRect.viewport.transform.position.x + currentButton.GetComponent<RectTransform>().rect.width / 4, currentButton.transform.position.y) : new Vector2(scrollRect.viewport.transform.position.x + scrollRect.viewport.rect.width - currentButton.GetComponent<RectTransform>().rect.width / 4, currentButton.transform.position.y));
						}
						else
						{
							targetPosition = currentButton.transform.position;
						}
					}
					else
					{
						if (currentButton.transform.position.y < scrollRect.viewport.transform.position.y - scrollRect.viewport.rect.height / 2)
							buttonDirection = new Vector2(0, -1);
						else if (currentButton.transform.position.y > scrollRect.viewport.transform.position.y - scrollRect.viewport.rect.height / 2)
							buttonDirection = new Vector2(0, 1);

						var border = new Vector2(currentButton.transform.position.x, currentButton.transform.position.y + currentButton.GetComponent<RectTransform>().rect.height / 2 * (buttonDirection == new Vector2(0, 1) ? 1 : -1));

						if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRect.viewport, border))
						{
							var speed = currentMenuType != "allRooms" ? 0.1f : 0.05f;
							scrollRect.verticalNormalizedPosition += speed * (buttonDirection == new Vector2(0, 1) ? 1 : -1);

							targetPosition = (buttonDirection == new Vector2(0, 1) ? new Vector2(currentButton.transform.position.x, scrollRect.viewport.transform.position.y - currentButton.GetComponent<RectTransform>().rect.height / 4) : new Vector2(currentButton.transform.position.x, scrollRect.viewport.transform.position.y - scrollRect.viewport.rect.height + currentButton.GetComponent<RectTransform>().rect.height / 4));

						}
						else
						{
							targetPosition = currentButton.transform.position;
						}
					}
					
					gamepadCursor.transform.position = Vector3.Lerp(gamepadCursor.transform.position, targetPosition, 10 * Time.unscaledDeltaTime);
				}
			}
			else
			{
				UpdateRectPos(scrollRect, ref firstTimeMenuOpened, type);
			}
		}

		public void UpdateRectPos(ScrollRect scrollRect, ref bool firstTimeMenuOpened, string type)
		{
			if (!firstTimeMenuOpened)
			{
				Canvas.ForceUpdateCanvases();

				var siblingIndex = allButtonsInCurrentMenu.IndexOf(currentButton) + 1;

				var pos = 0f;

				if (type == "horizontal")
				{
					pos = 0f + (float) siblingIndex / scrollRect.content.transform.childCount;
					scrollRect.horizontalNormalizedPosition = pos;
				}
				else
				{
					pos = 1f - (float) siblingIndex / scrollRect.content.transform.childCount;
					scrollRect.verticalNormalizedPosition = pos;
				}

				firstTimeMenuOpened = true;
			}
		}

		private void ResetRoomsButtons(string multiplayerType)
		{
			var buttons = new List<Button>();

			var allRoomsMenu = multiplayerType == "advanced" ? advancedMultiplayerGameLobby.AllRoomsMenu : basicMultiplayerGameLobby.AllRoomsMenu;
			
			if (allRoomsMenu.scrollRect)
			{
				foreach (Transform child in allRoomsMenu.scrollRect.content)
				{
					if (child.gameObject.GetComponent<UIPreset>() && child.gameObject.GetComponent<UIPreset>().Button)
						buttons.Add(child.gameObject.GetComponent<UIPreset>().Button);
				}

				allButtonsInCurrentMenu = buttons;

				if (allButtonsInCurrentMenu.Count > 0)
				{
					if (currentRoom)
					{
						currentButton = currentRoom;
					}
					else
					{
						if (allButtonsInCurrentMenu.Count > roomIndex)
						{
							currentButton = allButtonsInCurrentMenu[roomIndex];
							currentRoom = currentButton;
						}
						else
						{
							for (var i = roomIndex; i >= 0; i--)
							{
								if(allButtonsInCurrentMenu.Count > i)
									if (allButtonsInCurrentMenu[i])
									{
										currentButton = allButtonsInCurrentMenu[i];
										currentRoom = currentButton;
										roomIndex = i;
										break;
									}
							}
						}
					}
				}
				else
				{
					currentButton = null;
				}
			}
		}

		public void DisableAllBlips()
		{
			foreach (var image in allMinimapImages)
			{
				if(image.image)
					image.image.gameObject.SetActive(false);
			}
		}

		public void EnableAllBlips()
		{
			foreach (var image in allMinimapImages)
			{
				if(image.image)
					image.image.gameObject.SetActive(true);
			}
		}

		public void ResetMinimap()
		{
			var removeImages = new List<UIHelper.MinimapImage>();
			
			foreach (var image in allMinimapImages)
			{
				if(image.isBlipComponent) continue;
				
				if (image.controller != null && image.image)
				{
					if (image.controller.health <= 0)
					{
						Destroy(image.image.gameObject);
						removeImages.Add(image);
					}
				}
				else if (image.controller == null && image.image)
				{
					Destroy(image.image.gameObject);
					removeImages.Add(image);
				}
				else if (image.controller == null && !image.image)
				{
					removeImages.Add(image);
				}
			}

			if (removeImages.Count > 0)
			{
				foreach (var image in removeImages.Where(image => allMinimapImages.Contains(image)))
				{
					allMinimapImages.Remove(image);
				}

				removeImages.Clear();
			}
		}
	}
}
