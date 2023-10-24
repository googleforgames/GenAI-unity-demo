using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
	[RequireComponent(typeof(Animator))]
	public class Controller : MonoBehaviour
	{
		public CharacterController CharacterController;
		public InventoryManager inventoryManager;
		public Controller OriginalScript;
		public CharacterSync CharacterSync;
		public UIManager UIManager;
		public InteractionWithCars interactionWithCars;
		public EventSystem eventSystem;
		public GameManager gameManager;
		public AudioSource audioSource;
		
		public MultiplayerHelper.Teams multiplayerTeam;
		public MultiplayerHelper.CanKillOthers canKillOthers;

		public CharacterHelper.CameraType TypeOfCamera;
		public CharacterHelper.CameraType PreviousTypeOfCamera;
		public CharacterHelper.CameraParameters CameraParameters;

		public CharacterHelper.MovementType movementType;
		
		public Animator anim;

		public RuntimeAnimatorController characterAnimatorController;
		
		public AudioSource FeetAudioSource;

		public Helper.AnimationClipOverrides ClipOverrides;

		public CharacterHelper.CharacterOffset CharacterOffset;

		public CharacterHelper.Speeds FPSpeed;
		public CharacterHelper.Speeds TPSpeed;
		public CharacterHelper.Speeds TDSpeed;

		[Range(0.1f, 2)]public float TPspeedOffset = 1;
		[Range(1, 1000)] public float health = 100;
		[Range(0,50)] public float CrouchIdleNoise;
		[Range(0,50)] public float CrouchMovementNoise;
		[Range(0,50)] public float SprintMovementNoise;
		[Range(0,50)] public float MovementNoise;
		[Range(0,50)] public float IdleNoise;
		[Range(0,50)] public float JumpNoise;
		public float bodyRotationUpLimit_y;
		public float bodyRotationDownLimit_y;
		public float bodyRotationUpLimit_x;
		public float bodyRotationDownLimit_x;
		public float defaultHeight = -1;
		public float pressButtonTimeout;
		public float changeCameraTypeTimeout;
		public float currentCharacterControllerCenter;
		public float speedDivider = 1;
		public float headMultiplier = 1;
		public float bodyMultiplier = 1;
		public float handsMultiplier = 1;
		public float legsMultiplier = 1;
		public float changeSprintStateTimeout;
		public float noiseRadius;
		public float CurrentSpeed;
		
		public int characterTag;
		
		[Range(0.1f, 10)] public float CrouchHeight = 0.5f;

		public float defaultCharacterCenter;
		public float middleAngleX;
		
		#region InspectorParameters
		
		public int TDSpeedInspectorTab;
		public int TPSpeedInspectorTab;
		public int moveInspectorTab;
		public int inspectorTabTop;
		public int inspectorTabDown;
		public int currentInspectorTab;
		public int otherSettingsInspectorTab;
		public int inspectorSettingsTab;
		public int cameraInspectorTab;
		public int currentNavMeshArea = -1;
		
		public string curName;
		public string characterID;

		public bool delete;
		public bool rename;
		public bool renameError;
		
		#endregion

		public bool bodyLimit;
		[Tooltip("[OFF] - The character’s hands very accurately follow the camera" + "\n\n" +
		         "[ON] - The hands move with a small delay (like in most first-person games)")]
		public bool smoothHandsMovement = true;
		public bool oneShotOneKill;
		public bool ActiveCharacter;
		public bool isRemoteCharacter;
		public bool activeJump = true;
		public bool isPause;
		public bool DebugMode;
		public bool[] hasAxisButtonPressed = new bool[18];
		public bool AdjustmentScene;
		public bool SmoothCameraWhenMoving = true;
		public bool CameraFollowCharacter = true;
		public bool hasMoveButtonPressed;
		public bool emulateTDModeLikeTP;
		public bool onNavMesh;
		public bool inGrass;
		public bool inCar;
		public bool isAlwaysTpAimEnabled;

		public CharacterHelper.BodyObjects BodyObjects = new CharacterHelper.BodyObjects();
		public IKHelper.FeetIKVariables IKVariables;

		public List<Transform> BodyParts = new List<Transform> {null, null, null, null, null, null, null, null, null, null, null};
		public Transform DirectionObject;
		public Transform handDirectionObject;
		public Transform ColliderToObjectsDetection;
		
		public List<Helper.Attacker> colliderAttackers = new List<Helper.Attacker>();

		public GameObject thisCamera;
		
		public SphereCollider noiseCollider;

		public CameraController CameraController;
		
		public List<Texture> BloodHoles = new List<Texture>{null};
		public List<GameObject> additionalHitEffects = new List<GameObject>();
		public List<AudioClip> damageSounds = new List<AudioClip>();

		public Image healthBarImage;
		public Text nickNameText;
		public GameObject multiplayerStatsBackground;

		public RawImage blipRawImage;
		public Texture blipMainTexture;
		
		[Tooltip("This icon will be used in the multiplayer mode for opponents and teammates. It will be painted in the appropriate color.")]
		public Texture blipMultiplayerTexture;
		
		public Texture blipDeathTexture;
		public Texture2D KilledWeaponImage;
		
		public Color32 opponentColor = Color.red;
		public Color32 teammateColor = Color.green;

		public enum Direction
		{
			Forward,
			Backward,
			Left,
			Right,
			Stationary,
			ForwardLeft,
			ForwardRight,
			BackwardLeft,
			BackwardRight
		}

		public Direction MoveDirection;

		public Vector3 directionVector;
		public Vector3 MoveVector;
		public Vector3 BodyLocalEulerAngles;

		public Quaternion RotationAngle;
		public Quaternion CurrentRotation;
		
		public AnimatorOverrideController newController;

		public ProjectSettings projectSettings;
		
#if USK_ADVANCED_MULTIPLAYER
		public AMGameData amGameData;
#endif

		public List<HitMarker> hitMarkers;
		
		public string KillerName;
		public string multiplayerNickname;
		
		private RaycastHit distanceInfo;
		private RaycastHit heightInfo;

		private Transform bodylooks;

		private bool isObstacle;
		private bool CanMove;
		private bool wasRunningActiveBeforeJump;
		public bool isSprint;
		public bool isJump;
		public bool isCrouch;
		
		private bool flyingUp;

		private bool crouchTimeOut = true;
		private bool setDefaultDistance;
		private bool meleeDamage;

		public bool firstSceneStart = true;

		public bool clickMoveButton;
		private float defaultDistance;
		public float SmoothIKSwitch = 1;
		private float BodyHeight;
		public float JumpPosition;
		
		private float hipsAngleX;
		private float spineAngleX;
		public float headHeight = -1;
		public float currentGravity;
		private float defaultGravity;
		private float newJumpHeight;
		public float healthPercent;
		private float checkOnNavMeshTimer;

		private float currentDamageSoundsDelay;
		
		[Range(1, 20)]
		public float smoothHandsMovementValue = 10;
		
		private float angleBetweenCharacterAndCamera;

		public int touchId = -1;
		public int currentAnimatorLayer;
		public int currentGrassID;
		[Range(0, 100)] public int playDamageSoundsDelay;

		private Vector3 CheckCollisionVector;

		private Vector2 MobileMoveStickDirection;
		private Vector2 MobileTouchjPointA, MobileTouchjPointB;

		private RaycastHit HeightInfo;
		private bool clickButton;
		public bool isCharacterInLobby;

		private Coroutine setHandsAfterJumpTimeout;
		
		private void OnAnimatorMove()
		{
			if (isCharacterInLobby) return;

			if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson) return;

			anim.SetFloat("Speed Devider", 1 * TPspeedOffset / speedDivider);

			switch (TypeOfCamera)
			{
				case CharacterHelper.CameraType.ThirdPerson:
					if (!isJump && !anim.GetBool("Aim") && (!isAlwaysTpAimEnabled || CharacterHelper.CrouchMovement(anim)))
//								CharacterController.Move(anim.velocity * Time.deltaTime);
						transform.position += anim.deltaPosition * TPspeedOffset / speedDivider;
					break;
				case CharacterHelper.CameraType.TopDown:
					break;
			}


			if (TypeOfCamera != CharacterHelper.CameraType.TopDown && !isJump && !anim.GetBool("Aim") && (!isAlwaysTpAimEnabled || CharacterHelper.CrouchMovement(anim)))
			{
				transform.rotation = anim.rootRotation;
			}
		}

		private void Awake()
		{
			if (multiplayerStatsBackground)
				multiplayerStatsBackground.SetActive(false);

			if (healthBarImage)
				healthBarImage.gameObject.SetActive(false);

			if (nickNameText)
				nickNameText.gameObject.SetActive(false);

#if USK_MULTIPLAYER
			if (FindObjectOfType<LobbyManager>())
			{
				isCharacterInLobby = true;
				return;
			}
			
#if USK_ADVANCED_MULTIPLAYER 
			if (FindObjectOfType<AdvancedLobbyManager>())
			{
				isCharacterInLobby = true;
				return;
			}
			
#endif
			
			if (!PhotonNetwork.InRoom && GetComponent<CharacterSync>())
			{
				CharacterHelper.RemoveMultiplayerScriptsInGame(gameObject);
			}
#endif
			
			Input.simulateMouseWithTouches = false;

			inventoryManager = gameObject.GetComponent<InventoryManager>();
			inventoryManager.Controller = this;

			eventSystem = FindObjectOfType<EventSystem>();
		}

		void Start()
		{
#if USK_MULTIPLAYER
			if (isCharacterInLobby)
				return;
#endif
			Helper.ManageBodyColliders(BodyParts, this);

			ColliderToObjectsDetection = new GameObject("ColliderToCheckObjects").transform;
			ColliderToObjectsDetection.parent = BodyObjects.TopBody;
			ColliderToObjectsDetection.hideFlags = HideFlags.HideInHierarchy;
			
			if (FindObjectOfType<AIArea>())
			{
				var aiManager = FindObjectOfType<AIArea>();
				aiManager.AddPlayerToGlobalList(this);
				// aiManager.FindPlayers();
			}

			if (firstSceneStart)
			{
				InitializeParameters();
				
				inventoryManager.InitializeParameters(true);
				CameraController.InitializeParameters();

				firstSceneStart = false;
			}

			if (UIManager.CharacterUI.HealthBar)
			{
				UIManager.CharacterUI.HealthBar.fillAmount = 1;
				UIManager.CharacterUI.HealthBar.color = Color.green;
			}
		}

		private void OnDestroy()
		{
			
#if USK_MULTIPLAYER

			if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
			{
				var aiArea = FindObjectOfType<AIArea>();

				if (aiArea && aiArea.eventsManager)
				{
					aiArea.eventsManager.StopAllCoroutines();
					aiArea.eventsManager.StartCoroutine(aiArea.eventsManager.ClearLeftPlayers());
				}
			}
#endif
		}

		void OnEnable()
		{
			if(isCharacterInLobby) return;
			
			if (!firstSceneStart)
			{
				StopAllCoroutines();
				InitializeParameters();
				
				inventoryManager.StopAllCoroutines();
				inventoryManager.InitializeParameters(false);
			}
		}

		public void InitializeParameters()
		{
			if (firstSceneStart)
			{
				projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
				emulateTDModeLikeTP = false;

				if (damageSounds.Count > 0)
				{
					audioSource = gameObject.GetComponent<AudioSource>() ? gameObject.GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
				}

#if USK_ADVANCED_MULTIPLAYER
				amGameData = Resources.Load("AM Game Data", typeof(AMGameData)) as AMGameData;
				var arm = FindObjectOfType<AdvancedRoomManager>();
				
				if(arm && PlayerPrefs.HasKey("CameraType"))
				{
					var cameraType = (CharacterHelper.CameraType) PlayerPrefs.GetInt("CameraType");

					if (cameraType == CharacterHelper.CameraType.TopDown)
					{
						if (CameraParameters.alwaysTDAim || CameraParameters.lockCamera)
						{
							TypeOfCamera = CharacterHelper.CameraType.TopDown;
						}
						else
						{
							TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;
							emulateTDModeLikeTP = true;
						}
					}
					else
					{
						TypeOfCamera = cameraType;
					}
				}
				else
#endif
				
				{
					if (CameraParameters.activeFP && TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
					{
						TypeOfCamera = CharacterHelper.CameraType.FirstPerson;
					}
					else if (CameraParameters.activeTP && TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
					{
						TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;
					}

					else if (CameraParameters.activeTD && TypeOfCamera == CharacterHelper.CameraType.TopDown)
					{
						if (CameraParameters.alwaysTDAim || CameraParameters.lockCamera)
							TypeOfCamera = CharacterHelper.CameraType.TopDown;
						else
						{
							TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;
							emulateTDModeLikeTP = true;
						}
					}
					else if (CameraParameters.activeFP) TypeOfCamera = CharacterHelper.CameraType.FirstPerson;
					else if (CameraParameters.activeTD)
					{
						if (CameraParameters.alwaysTDAim || CameraParameters.lockCamera)
							TypeOfCamera = CharacterHelper.CameraType.TopDown;
						else
						{
							TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;
							emulateTDModeLikeTP = true;
						}
					}
					else if (CameraParameters.activeTP) TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;
					else
					{
						Debug.LogError("Please select any active camera view.", gameObject);
						Debug.Break();
					}
				}

				healthPercent = health;

				anim = gameObject.GetComponent<Animator>();

				if (FindObjectOfType<GameManager>())
				{
					gameManager = FindObjectOfType<GameManager>();

					if (gameManager)
					{
						UIManager = gameManager.currentUIManager;
						canKillOthers = MultiplayerHelper.CanKillOthers.Everyone;

						isRemoteCharacter = false;
					}
				}
#if USK_MULTIPLAYER
				else if (FindObjectOfType<RoomManager>())
				{
					UIManager = FindObjectOfType<RoomManager>().currentUIManager;
				}
#if USK_ADVANCED_MULTIPLAYER
				else if (arm)
				{
					UIManager = arm.currentUIManager;
				}
#endif
#endif
				
#if UNITY_EDITOR
				else if (FindObjectOfType<Adjustment>())
				{
					UIManager = FindObjectOfType<Adjustment>().UIManager;
					isRemoteCharacter = false;
				}
#endif
				else
				{
					ActiveCharacter = true;
					
					UIManager = !FindObjectOfType<UIManager>() ? Instantiate(Resources.Load("UI Manager", typeof(UIManager)) as UIManager) : FindObjectOfType<UIManager>();
					UIManager.HideAllMultiplayerRoomUI();
					UIManager.HideAllMultiplayerLobbyUI();
					UIManager.HideAllSinglePlayerMenus();
					UIManager.CharacterUI.Inventory.ActivateAll();
					UIManager.CharacterUI.DisableAll();
					UIManager.CharacterUI.ActivateAll(UIManager.useMinimap);
				}

				if (!projectSettings)
				{
					Debug.LogError("<color=red>Missing component</color> [Project Settings]. Please reimport this kit.");
					Debug.Break();
				}

#if USK_NWHVPH_INTEGRATION || USK_RCC_INTEGRATION || USK_EVPH_INTEGRATION
				if (interactionWithCars)
				{
					if (Application.isMobilePlatform || projectSettings.mobileDebug)
						if (UIManager.uiButtons[15])
							UIManager.uiButtons[15].onClick.AddListener(interactionWithCars.UIButtonEvent);
				}
#endif

				if (!CharacterController)
				{
					CharacterController = gameObject.AddComponent<CharacterController>();
				}

				if (!gameObject.GetComponent<CharacterSync>() && !gameObject.GetComponent<NavMeshObstacle>())
				{
					var script = gameObject.AddComponent<NavMeshObstacle>();
					script.shape = NavMeshObstacleShape.Capsule;
					script.carving = true;
				}
				
				if (FindObjectOfType<Adjustment>())
				{
					AdjustmentScene = true;
				}
				else
				{
					AdjustmentScene = false;
					
					if ((Application.isMobilePlatform || projectSettings.mobileDebug)
#if USK_MULTIPLAYER
					    && (GetComponent<PhotonView>() && GetComponent<PhotonView>().IsMine || !GetComponent<PhotonView>())
#endif
					)
					{
						var gameManager = FindObjectOfType<GameManager>();
						UIHelper.SetMobileButtons(this, gameManager);
						
						if (UIManager.UIButtonsMainObject)
							UIManager.UIButtonsMainObject.SetActive(true);
						
						if (UIManager.uiButtons[15] && !GetComponent<InteractionWithCars>())
							UIManager.uiButtons[15].gameObject.SetActive(false);
						// }
					}

					if (!Application.isMobilePlatform && !projectSettings.mobileDebug)
					{
						if (UIManager.UIButtonsMainObject)
							UIManager.UIButtonsMainObject.SetActive(false);
					}
				}

				StartCoroutine(SetDefaultHeight());
				
				if (FeetAudioSource)
					FeetAudioSource.hideFlags = HideFlags.HideInHierarchy;

				if (DirectionObject)
					DirectionObject.localEulerAngles = CharacterOffset.directionObjRotation;

				if (handDirectionObject)
					handDirectionObject.localEulerAngles = CharacterOffset.handsDirectionObjRotation;

				if (!isRemoteCharacter)
					Helper.ChangeLayersRecursively(transform, "Character");
				
				if (!noiseCollider)
					Helper.CreateNoiseCollider(transform, this);
				
				if(isRemoteCharacter)
					noiseCollider.gameObject.SetActive(false);
			}
			
			if (UIManager.CharacterUI.Health)
				UIManager.CharacterUI.Health.text = health.ToString("F0");

			isJump = false;
			isCrouch = false;
			isSprint = false;
			
			MoveVector = Vector3.zero;

			currentDamageSoundsDelay = playDamageSoundsDelay;

			anim.Rebind();
			anim.runtimeAnimatorController = characterAnimatorController;

			newController = new AnimatorOverrideController(anim.runtimeAnimatorController);
			anim.runtimeAnimatorController = newController;
			ClipOverrides = new Helper.AnimationClipOverrides(newController.overridesCount);
			newController.GetOverrides(ClipOverrides);
			
			if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
			{
				anim.Play("Walk", 0);
			}
			
			if (!thisCamera || !CameraController)
			{
				var foundObjects = FindObjectsOfType<CameraController>();
				foreach (var camera in foundObjects)
				{
					if (camera.transform.parent == transform)
					{
						CameraController = camera.GetComponent<CameraController>();
						thisCamera = CameraController.gameObject;
					}
				}
			}
			
			defaultGravity = Physics.gravity.y;
			currentGravity = defaultGravity;

			var center = CharacterController.center;
			center = new Vector3(center.x, -CharacterOffset.CharacterHeight, center.z);
			CharacterController.center = center;
			defaultCharacterCenter = -CharacterOffset.CharacterHeight;
			CharacterController.skinWidth = 0.01f;
			CharacterController.height = 1;
			CharacterController.radius = 0.35f;

			CharacterController.slopeLimit = 90;

			CharacterHelper.ResetCameraParameters(TypeOfCamera, TypeOfCamera, this);
			
			anim.SetBool("Movement in All Directions", TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && (movementType == CharacterHelper.MovementType.AllDirections || isAlwaysTpAimEnabled));

			if (!isRemoteCharacter)
			{
				if (thisCamera)
					thisCamera.SetActive(true);

				InputHelper.InitializeKeyboardAndMouseButtons(projectSettings);
				InputHelper.InitializeGamepadButtons(projectSettings);
				
				InputSystem.onDeviceChange += (device, change) =>
				{
					switch (change)
					{
						case InputDeviceChange.Added:
							InputHelper.InitializeGamepadButtons(projectSettings);
							break;
					}
				};
				
				if (CameraController)
				{
					CameraController.maxMouseAbsolute = middleAngleX + CameraParameters.fpXLimitMax;
					CameraController.minMouseAbsolute = middleAngleX + CameraParameters.fpXLimitMin;
				}

				gameObject.tag = "Player";
			}
			else
			{
				CameraController.SetAnimVariables();
			}
		}
		

		private void LateUpdate()
		{
			if (isCharacterInLobby) return;

			if (isRemoteCharacter && blipRawImage)
			{
				UIManager.SetBlip(transform, "rotationAndPosition", null, blipRawImage);
			}
		}

		void Update()
		{
			if(isCharacterInLobby) return;

			changeCameraTypeTimeout += Time.deltaTime;

			if (AdjustmentScene)
			{
				if(Keyboard.current != null && InputHelper.WasKeyboardOrMouseButtonPressed(Keyboard.current.cKey))
					ChangeCameraType();
				
				return;
			}

			changeSprintStateTimeout += Time.deltaTime;
			
			noiseCollider.radius = noiseRadius;
			
			CreateNewBlip();
			
			if (!isRemoteCharacter && blipRawImage)
			{
				UIManager.SetBlip(TypeOfCamera != CharacterHelper.CameraType.FirstPerson ? transform : CameraController.MainCamera.transform,"rotationOnly", null, blipRawImage);
			}

			if (isRemoteCharacter || !ActiveCharacter)
				return;

			currentCharacterControllerCenter = CharacterController.center.y;
				 
			if(TypeOfCamera != CharacterHelper.CameraType.TopDown)
				anim.SetFloat("CameraAngle", Helper.AngleBetween(transform.forward, thisCamera.transform.forward));
			else
			{
				anim.SetFloat("CameraAngle", Helper.AngleBetween(transform.forward, !CameraParameters.lockCamera ? thisCamera.transform.forward :
					CameraController.BodyLookAt.position - transform.position));
			}
			
			if (!AdjustmentScene)
			{
				if (projectSettings.ButtonsActivityStatuses[11] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[11]) ||
				                                                    InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[11], this)))
					ChangeCameraType();
			}

			if (projectSettings.ButtonsActivityStatuses[20] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[21]) ||
			                                                    InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[19], this)))
			{
				ChangeMovementType();
			}

			if (projectSettings.ButtonsActivityStatuses[2] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[2]) || InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[2], this)))
			{
				Jump();
			}

			if (!Application.isMobilePlatform && !projectSettings.mobileDebug)
			{
				if (projectSettings.holdSprintButton)
				{
					if (projectSettings.ButtonsActivityStatuses[0] && (InputHelper.IsKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[0]) || InputHelper.IsGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[0], this)))
						Sprint(true, "press");
					else 
						Sprint(false, "press");
				}
				else
				{
					if (projectSettings.ButtonsActivityStatuses[0] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[0]) || InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[0], this)))
					{
						Sprint(true, "click");
					}
				}

				if (projectSettings.holdCrouchButton)
				{
					if (projectSettings.ButtonsActivityStatuses[1] && (InputHelper.IsKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[1])
						|| InputHelper.IsGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[1], this)))
					{
						if (!isCrouch)
						{
							if (isSprint)
								DeactivateSprint();

							Crouch(true, "press");
							crouchTimeOut = false;
							StartCoroutine("CrouchTimeout");
						}
					}
					else
					{
						if (isCrouch && crouchTimeOut)
						{
							Crouch(false, "press");
						}
					}
				}
				else
				{
					if (projectSettings.ButtonsActivityStatuses[1] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[1])
						|| InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[1], this)))
					{
						if (crouchTimeOut)
						{
							if (isSprint)
								DeactivateSprint();

							Crouch(true, "click");
							crouchTimeOut = false;
							StartCoroutine("CrouchTimeout");
						}
					}
				}
			}

			HeightDetection();
			GetLocomotionInput();
			SnapAlignCharacterWithCamera();
			ProcessMotion();
			JumpProcess();
			
			// if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson || MovementType == CharacterHelper.MovementType.FastAndAccurate) JumpingProcess();
			
			checkOnNavMeshTimer += Time.deltaTime;
			
			if (checkOnNavMeshTimer > 1)
			{
				checkOnNavMeshTimer = 0;
				
				NavMeshHit hit;
				if (NavMesh.SamplePosition(new Vector3(transform.position.x, transform.position.y, transform.position.z), out hit, 2, NavMesh.AllAreas))
				{
					onNavMesh = hit.distance <= 4;
					currentNavMeshArea = hit.mask;
				}
				else
				{
					currentNavMeshArea = -1;
					onNavMesh = false;
				}
			}
		}

		IEnumerator SetHandsAfterJumpTimeout()
		{
			yield return new WaitForSeconds(0.7f);
			
			inventoryManager.WeaponController.IkObjects.LeftObject.parent = inventoryManager.WeaponController.IkObjects.RightObject;
			inventoryManager.WeaponController.IkObjects.RightObject.parent = BodyObjects.TopBody;
			inventoryManager.WeaponController.IkObjects.RightObject.position = BodyObjects.RightHand.position;
			inventoryManager.WeaponController.IkObjects.RightObject.rotation = BodyObjects.RightHand.rotation * (handDirectionObject ? Quaternion.Euler(CharacterOffset.handsDirectionObjRotation) : Quaternion.Euler(Vector3.zero));
			inventoryManager.WeaponController.setHandsPositionsAim = false;
			
			anim.SetBool("CanWalkWithWeapon", false);

			inventoryManager.WeaponController.aimWhileJumping = false;
		}

		void HeightDetection()
		{
			if (defaultHeight == -1)
				return;
			
			anim.SetBool("OnFloor", CharacterHelper.CheckIfGrounded(transform, CharacterController, Helper.LayerMask()));

			if (anim.GetBool("OnFloor") && anim.GetBool("Jump") && !flyingUp)
			{
				isJump = false;
				SmoothIKSwitch = 1;
				flyingUp = false;
				anim.SetBool("Jump", false);

				if (setHandsAfterJumpTimeout != null)
					StopCoroutine(setHandsAfterJumpTimeout);
					
				if (inventoryManager.WeaponController && inventoryManager.WeaponController.aimWhileJumping)
				{
					setHandsAfterJumpTimeout = StartCoroutine(SetHandsAfterJumpTimeout());
				}
			}

			if (Physics.Raycast(BodyObjects.Hips.position + transform.forward, Vector3.down, out var hitInfo, 10, Helper.LayerMask()))
			{
				if (Mathf.Abs(defaultHeight - hitInfo.distance) > 2)
				{
					if (anim.GetBool("OnFloorForward"))
						anim.SetFloat("FallingHeight", hitInfo.distance);
					
					anim.SetBool("OnFloorForward", false);
				}
				else
				{
					anim.SetBool("OnFloorForward", true);
				}
			}
			else
			{
				anim.SetBool("OnFloorForward", false);
			}
		}

		IEnumerator SetDefaultHeight()
		{
			yield return new WaitForSeconds(0.1f);
			RaycastHit info;
			
			if (Physics.Raycast(BodyObjects.Hips.position, Vector3.down, out info, 100, Helper.LayerMask()))
				defaultHeight = info.distance;
			
			if (Physics.Raycast(BodyObjects.Head.position, Vector3.down, out info, 100, Helper.LayerMask()))
				headHeight = info.distance;
			
			StopCoroutine("SetDefaultHeight");
		}

		public void CreateNewBlip()
		{
			if(!UIManager) return;
			
			if (blipMainTexture && UIManager.CharacterUI.mapMask && !blipRawImage)
			{
				blipRawImage = Helper.NewUIElement("Character Cursor", UIManager.CharacterUI.mapMask.transform, Vector2.zero, Vector2.zero, Vector3.one ).gameObject.AddComponent<RawImage>();
				blipRawImage.rectTransform.sizeDelta = new Vector2(30, 30);
				
				blipRawImage.texture = !isRemoteCharacter ? blipMainTexture : blipMultiplayerTexture;
				
				UIManager.allMinimapImages.Add(new UIHelper.MinimapImage{controller = this, image = blipRawImage});
				
				if (isRemoteCharacter)
				{
					var color = blipRawImage.color;
					color.a = 0;
					blipRawImage.color = color;
				}
			}
		}

		void GetLocomotionInput()
		{
			directionVector = Vector3.zero;

			if (!isPause)
			{
				hasMoveButtonPressed = false;
				
				if (Application.isMobilePlatform || projectSettings.mobileDebug)
				{
					InputHelper.CheckMobileJoystick(UIManager.moveStick, UIManager.moveStickOutline, ref touchId, UIManager, ref MobileTouchjPointA, ref MobileTouchjPointB, ref MobileMoveStickDirection, this);
					directionVector = new Vector3(MobileMoveStickDirection.x, 0, MobileMoveStickDirection.y);
				}
				else
				{
					if (Gamepad.current != null && 
					    (Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[0].x.ReadValue()) > 0.1f ||
					    Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[0].y.ReadValue()) > 0.1f))
					{
						hasMoveButtonPressed = false;

						var Horizontal = projectSettings.gamepadAxisControlsInUnityInputSystem[0].x.ReadValue();
						var Vertical = projectSettings.gamepadAxisControlsInUnityInputSystem[0].y.ReadValue();

						if (projectSettings.invertAxes[0])
							Horizontal *= -1;

						if (projectSettings.invertAxes[1])
							Vertical *= -1;

						if (Mathf.Abs(Horizontal) > 0.1f || Mathf.Abs(Vertical) > 0.1f)
						{
							if(!anim.GetBool("Move"))
								anim.SetBool("MoveButtonHasPressed", true);
							
							hasMoveButtonPressed = true;
						}

						directionVector = new Vector3(Horizontal, 0, Vertical);


						if (projectSettings.runWithJoystick)
						{
							var sprint = directionVector.magnitude > projectSettings.runJoystickRange;

							if (!isCrouch)
							{
								if (isSprint)
								{
									changeSprintStateTimeout = 0;
									isSprint = sprint;
									anim.SetBool("Sprint", isSprint);
								}
								else
								{
									// if (changeSprintStateTimeout > 2)
									// {
									changeSprintStateTimeout = 0;
									isSprint = sprint;
									anim.SetBool("Sprint", isSprint);
									// }
								}
							}
						}
					}
					else
					{
						hasMoveButtonPressed = false;

						InputHelper.ProcessMoveButton(this, "forward");
						InputHelper.ProcessMoveButton(this, "backward");
						InputHelper.ProcessMoveButton(this, "left");
						InputHelper.ProcessMoveButton(this, "right");
					}
				}

				anim.SetBool("PressMoveAxis", hasMoveButtonPressed);

				// if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(2).IsName("Attack"))
				// {
				var targetNoiseValue = 0f;
				
					if (hasMoveButtonPressed)
					{
						if (isSprint)
						{
							targetNoiseValue = SprintMovementNoise / (inGrass ? 2 : 1);
							// noiseRadius = Mathf.Lerp(noiseRadius, SprintMovementNoise / (inGrass ? 2 : 1), 5 * Time.deltaTime);
						}
						else if (isCrouch)
						{
							targetNoiseValue = CrouchMovementNoise / (inGrass ? 2 : 1);
							// noiseRadius = Mathf.Lerp(noiseRadius, CrouchMovementNoise/ (inGrass ? 2 : 1), 5 * Time.deltaTime);
						}
						else
						{
							targetNoiseValue = MovementNoise / (inGrass ? 2 : 1);
							// noiseRadius = Mathf.Lerp(noiseRadius, MovementNoise/ (inGrass ? 2 : 1), 5 * Time.deltaTime);
						}
					}
					else
					{
						targetNoiseValue = !isCrouch ? IdleNoise : CrouchIdleNoise;
						// noiseRadius = Mathf.Lerp(noiseRadius, !isCrouch ? IdleNoise : CrouchIdleNoise, 5 * Time.deltaTime);
					}

					if (anim.GetBool("Attack") && inventoryManager.WeaponController && inventoryManager.WeaponController.Attacks[inventoryManager.WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade &&
					    inventoryManager.WeaponController.Attacks[inventoryManager.WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.GrenadeLauncher)
					{
						targetNoiseValue *= inventoryManager.WeaponController.Attacks[inventoryManager.WeaponController.currentAttack].attackNoiseRadiusMultiplier;
					}

					noiseRadius = Mathf.Lerp(noiseRadius, targetNoiseValue, 5 * Time.deltaTime);

				// }

//				if(directionVector.magnitude > 0)
					CheckCollisionVector = directionVector * 100;

				if (CanMove)
				{
					if (hasMoveButtonPressed)
					{
						if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.TopDown)
						{
							var newDirection = directionVector;
							
							if (TypeOfCamera == CharacterHelper.CameraType.TopDown && CameraParameters.lockCamera)
							{
								newDirection = transform.InverseTransformDirection(newDirection);
							}

							anim.SetFloat("Horizontal", newDirection.x, 0.5f, Time.deltaTime);
							anim.SetFloat("Vertical", newDirection.z, 0.5f, Time.deltaTime);

							if (TypeOfCamera == CharacterHelper.CameraType.TopDown && CameraParameters.lockCamera)
							{
								var newDir2 = transform.TransformDirection(newDirection);
								MoveVector = newDir2;
							}
							else
							{
								MoveVector = new Vector3(anim.GetFloat("Horizontal"), 0, anim.GetFloat("Vertical"));
							}
						}
						else
						{
							anim.SetFloat("Horizontal", directionVector.x, 0.1f, Time.deltaTime);
							anim.SetFloat("Vertical", directionVector.z, 0.1f, Time.deltaTime);

							MoveVector = new Vector3(anim.GetFloat("Horizontal"), 0, anim.GetFloat("Vertical"));
						}
					}
					else
					{
						anim.SetFloat("Horizontal", directionVector.x, 0.2f, Time.deltaTime);
						anim.SetFloat("Vertical", directionVector.z, 0.2f, Time.deltaTime);

						MoveVector = directionVector;
					}
				}
				else
				{
					anim.SetFloat("Horizontal", 0, 0.5f, Time.deltaTime);
					anim.SetFloat("Vertical", 0, 0.5f, Time.deltaTime);
				}
			}
			else
			{
				anim.SetFloat("Horizontal", 0, 0.3f, Time.deltaTime);
				anim.SetFloat("Vertical", 0, 0.3f, Time.deltaTime);

				Sprint(false, "press");
			}

			if (!isPause)
			{
				if (clickMoveButton && inventoryManager.WeaponController && inventoryManager.WeaponController.isAimEnabled && isCrouch && TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					inventoryManager.WeaponController.Aim(true, false, false);
				}

				angleBetweenCharacterAndCamera = Helper.AngleBetween(transform.TransformDirection(new Vector3(-directionVector.x, directionVector.y, directionVector.z)), thisCamera.transform.forward);

				anim.SetFloat("Angle", angleBetweenCharacterAndCamera);

				if (!hasMoveButtonPressed)
				{
					anim.SetBool("Move", false);
				}
				else
				{
					if (Mathf.Abs(MoveVector.x) > 0.2f || Math.Abs(MoveVector.z) > 0.2f)
					{
						anim.SetBool("MoveButtonHasPressed", false);
						anim.SetBool("Move", true);
						clickMoveButton = false;
					}
				}

				if (SmoothCameraWhenMoving)
				{
					if (hasMoveButtonPressed)
					{
						if (!isSprint && !isCrouch)
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 6, 3 * Time.deltaTime);
						else if (isSprint)
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 7, 3 * Time.deltaTime);
						else if (isCrouch)
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 6, 3 * Time.deltaTime);
					}
					else
					{
						if (!isCrouch && !isSprint)
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 5, 3 * Time.deltaTime);

						else if (isCrouch && !CameraController.isCameraAimEnabled)
						{
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 5, 3 * Time.deltaTime);
						}
						else if (isCrouch && CameraController.isCameraAimEnabled)
						{
							CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 5, 3 * Time.deltaTime);
						}
						else if (isSprint) CameraController.cameraMovementDistance = Mathf.Lerp(CameraController.cameraMovementDistance, 5.5f, 3 * Time.deltaTime);
					}
				}
				
				CurrentMoveDirection();
			}
		}

		public void TakingDamageFromColliders(GameObject attacker, string type)
		{
			if (attacker.GetComponent<AIController>())
			{
				var attackerController = attacker.GetComponent<AIController>();

				if (MultiplayerHelper.CanDamageInMultiplayer(this, attackerController))
				{
					var damage = attacker.GetComponent<AIController>().Attacks[0].Damage * (attackerController.Attacks[0].AttackType == AIHelper.AttackTypes.Fire ? Time.deltaTime : 1);
					
					Damage(damage, type,
#if USK_MULTIPLAYER
						PhotonNetwork.InRoom ? new Helper.ActorID{actorID =  attackerController.photonView.ViewID, type = "ai"} :
#endif
						null
						);


					// health -= 
					//
					// if (health <= 0)
					// {
					// 	KillerName = attackerController.nickname;
					// }
				}
			}
			else if (attacker.GetComponent<Controller>())
			{
				if (attacker.GetComponent<Controller>().gameObject.GetInstanceID() == gameObject.GetInstanceID()) return;

				var attackerController = attacker.GetComponent<Controller>();

				if (MultiplayerHelper.CanDamageInMultiplayer(this, attackerController))
				{

// #if USK_ADVANCED_MULTIPLAYER
// 					if (CharacterSync)
// 						CharacterSync.UpdateKillAssists(attackerController.multiplayerNickname);
// #endif

					var damage = 0f;
					Texture2D weaponImage = null;

					if (attackerController.inventoryManager.WeaponController)
					{
						var weaponController = attackerController.inventoryManager.WeaponController;
						damage = weaponController.Attacks[weaponController.currentAttack].weapon_damage * (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame ? Time.deltaTime : 1);
						weaponImage = (Texture2D) weaponController.weaponImage;
					}
					else
					{
						weaponImage = (Texture2D) attackerController.inventoryManager.FistIcon;
						damage = attackerController.inventoryManager.FistDamage;
					}

// #if USK_ADVANCED_MULTIPLAYER
// 					if (health > 0 && health - damage <= 0)
// 					{
// 						if (attackerController.CharacterSync)
// 							attackerController.CharacterSync.AddScore(PlayerPrefs.GetInt(type == "fire" ? "FireKill" : "MeleeKill"), type);
// 					}
// #endif

					Damage(damage, type,
#if USK_MULTIPLAYER
						 PhotonNetwork.InRoom ? new Helper.ActorID{actorID =  attackerController.CharacterSync.photonView.OwnerActorNr, type = "player"} :
#endif
						null
						);
				}
			}
		}

		public void ControlHealth(string attackType, Helper.ActorID attackerActorID = null)
		{
			UpdateHealthUI();
			
			if (UIManager.CharacterUI.bloodSplatter)
			{
				if(!UIManager.CharacterUI.bloodSplatter.gameObject.activeSelf)
					UIManager.CharacterUI.bloodSplatter.gameObject.SetActive(true);
				
				if (health < 40)
				{
					var healthPercentage = 100 - (health / 30 * 100);
					
					UIManager.CharacterUI.bloodSplatter.color = new Color(1, 1, 1, healthPercentage / 100);
				}
				else
				{
					UIManager.CharacterUI.bloodSplatter.color = new Color(1, 1, 1, 0);
				}
			}

			if (health <= 0)
			{
				Death(attackType, attackerActorID);
			}
		}

		public void UpdateHealthUI()
		{
			if (UIManager.CharacterUI.Health)
				UIManager.CharacterUI.Health.text = health < 0 ? "0" : health.ToString("F0");

			if (UIManager.CharacterUI.HealthBar)
			{
				UIManager.CharacterUI.HealthBar.fillAmount = health / healthPercent;
				
				if (health >= 75)
					UIManager.CharacterUI.HealthBar.color = Color.green;
				if (health >= 50 & health < 75)
					UIManager.CharacterUI.HealthBar.color = Color.yellow;
				if (health >= 25 & health < 50)
					UIManager.CharacterUI.HealthBar.color = new Color32(255, 140, 0, 255);
				if (health < 25)
					UIManager.CharacterUI.HealthBar.color = Color.red;

				UIManager.CharacterUI.HealthBar.fillAmount = health / healthPercent;
			}
		}

		private void Death(string attackType, Helper.ActorID killerActor = null)
		{
			if(killerActor == null) killerActor = new Helper.ActorID();
			
			if (!Application.isMobilePlatform && !projectSettings.mobileDebug)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			
			foreach (var part in BodyParts)
			{
				part.GetComponent<Rigidbody>().isKinematic = false;
			}

			if (blipRawImage && blipDeathTexture)
			{
				var color = blipRawImage.color;
				color = new Color(color.r, color.g, color.b, 1);
				blipRawImage.color = color;
				
				blipRawImage.texture = blipDeathTexture;
			}

			for (int i = 0; i < 8; i++)
			{
				switch (UIManager.CharacterUI.Inventory.WeaponsButtons[i].transition)
				{
					case Selectable.Transition.ColorTint:

						var colorBlock = UIManager.CharacterUI.Inventory.WeaponsButtons[i].colors;
						colorBlock.normalColor = UIManager.CharacterUI.Inventory.normButtonsColors[i];
						UIManager.CharacterUI.Inventory.WeaponsButtons[i].colors = colorBlock;
						break;

					case Selectable.Transition.SpriteSwap:
						UIManager.CharacterUI.Inventory.WeaponsButtons[i].GetComponent<Image>().sprite = UIManager.CharacterUI.Inventory.normButtonsSprites[i];
						break;
				}
			}

			UIManager.CharacterUI.DisableAll();

			if (Application.isMobilePlatform || projectSettings.mobileDebug)
				UIManager.UIButtonsMainObject.SetActive(false);

			anim.enabled = false;
			inventoryManager.enabled = false;
			enabled = false;

			inventoryManager.StopAllCoroutines();

			Helper.CameraExtensions.LayerCullingShow(CameraController.Camera, "Head");

			if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && emulateTDModeLikeTP)
				TypeOfCamera = CharacterHelper.CameraType.TopDown;
			
			PlayerPrefs.SetInt("CameraType", (int)TypeOfCamera);

			if (inventoryManager.WeaponController)
				Destroy(inventoryManager.WeaponController.gameObject);

#if USK_EMERALDAI_INTEGRATION
				if(GetComponent<EmeraldAI.EmeraldAIPlayerDamage>())
					GetComponent<EmeraldAI.EmeraldAIPlayerDamage>().IsDead = true;
#endif
			var aiManagers = FindObjectsOfType<AIArea>();
			{
				foreach (var manager in aiManagers)
				{
					if (manager.multiplayerMatch) break;
					
					var thisCharacter = manager.allPlayersInScene.Find(controller1 => controller1.controller == this);
					manager.ClearEmptyPlayer(thisCharacter, true);
#if USK_MULTIPLAYER
					if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
					{
						if (PhotonNetwork.IsMasterClient)
							manager.ManagePlayersBetweenOpponents();
					}
					else
#endif
					{
						manager.ManagePlayersBetweenOpponents();
					}
				}
			}

#if USK_ADVANCED_MULTIPLAYER

			if (CharacterSync)
			{
				var curActorID = new Helper.ActorID {actorID = CharacterSync.photonView.OwnerActorNr, type = "player"};

				if (CharacterSync.eventsManager)
					CharacterSync.eventsManager.AllocateScoreAfterSomeoneDead(curActorID, killerActor, attackType, CharacterSync.opponentsWhoAttackedPlayer);

				AMHelper.ShowKillDeathStats(CharacterSync.advancedRoomManager, killerActor, curActorID, attackType);
			}
#endif

#if USK_MULTIPLAYER
			if (CharacterSync)
			{
				CharacterSync.Destroy(killerActor, attackType);
			}
			else
#endif
			{
				CameraController.enabled = false;
			}
		}

		void ProcessMotion()
		{
			if (!isPause)
			{
				if (TypeOfCamera != CharacterHelper.CameraType.TopDown)
				{
					MoveVector = !isJump ? transform.TransformDirection(MoveVector) : thisCamera.transform.TransformDirection(MoveVector);
				}
				else
				{
					if (!CameraParameters.lockCamera)
						MoveVector = transform.TransformDirection(MoveVector);
				}

				if (TypeOfCamera != CharacterHelper.CameraType.TopDown)
				{
					CheckCollisionVector = thisCamera.transform.TransformDirection(CheckCollisionVector);
				}
				else
				{
					if (!CameraParameters.lockCamera)
						CheckCollisionVector = transform.TransformDirection(CheckCollisionVector);

				}

				if (MoveVector.magnitude > 1)
					MoveVector = MoveVector.normalized;


				CheckCollisionVector = CheckCollisionVector.normalized;

				float speed = 0;

				switch (TypeOfCamera)
				{
					case CharacterHelper.CameraType.ThirdPerson:
						if(emulateTDModeLikeTP)
							speed = MoveSpeed(TDSpeed);
						else
							speed = MoveSpeed(TPSpeed);
						break;
					case CharacterHelper.CameraType.FirstPerson:
						speed = MoveSpeed(FPSpeed);
						break;
					case CharacterHelper.CameraType.TopDown:
						speed = MoveSpeed(TDSpeed);
						break;
				}

				CurrentSpeed = MoveDirection == Direction.Stationary ? Mathf.Lerp(CurrentSpeed, speed, 0.5f * Time.deltaTime) : Mathf.Lerp(CurrentSpeed, speed, 3 * Time.deltaTime);

				if (!anim.GetBool("OnFloor") || isJump)
				{
					CurrentSpeed = Mathf.Lerp(CurrentSpeed, 2, 3 * Time.deltaTime);
				}

				if (CurrentSpeed < 0)
					CurrentSpeed = 0;

				var checkCollisionPoint1 = Vector3.zero;
				var checkCollisionPoint2 = Vector3.zero;

				if (Physics.Raycast(BodyObjects.Hips.position, Vector3.down, out var hit1, 10, Helper.LayerMask()))
				{
					if (Physics.Raycast(BodyObjects.Hips.position + BodyObjects.Hips.forward, Vector3.down, out var hit2, 10, Helper.LayerMask()))
					{
						checkCollisionPoint2 = hit2.point + hit2.normal * 2;
					}
					else
					{
						checkCollisionPoint2 = hit1.point + hit1.normal * 2;
					}

					checkCollisionPoint1 = hit1.point + hit1.normal * 2;
				}

				var checkDir = checkCollisionPoint2 - checkCollisionPoint1;

				if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + headHeight, transform.position.z), new Vector3(CheckCollisionVector.x, checkDir.y, CheckCollisionVector.z), out distanceInfo, CheckCollisionVector.magnitude * 10, Helper.LayerMask()))
				{
					if (!distanceInfo.collider || distanceInfo.collider && !distanceInfo.collider.isTrigger)
					{
						if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
						{
							if (distanceInfo.distance < 1)
							{
								CurrentSpeed = 0;
								isObstacle = true;
							}
							else
							{
								isObstacle = false;
							}
						}
						else
						{
							if (distanceInfo.distance < 1)
							{
								isObstacle = true;
							}
							else if (distanceInfo.distance > 1.35)
							{
								isObstacle = false;
							}
						}
					}
				}
				else
				{
					if (gameObject.activeSelf)
					{
//					canMoveTimer += Time.deltaTime;
//					if (canMoveTimer > 0.1f) 
						isObstacle = false;

//						if(directionVector.magnitude > 0)
//							anim.SetBool("Move", true);
					}

//					StartCoroutine(MovePause());
				}


				MoveVector = new Vector3(MoveVector.x * CurrentSpeed, 0, MoveVector.z * CurrentSpeed);

				if (!isObstacle)
				{
					switch (TypeOfCamera)
					{
						case CharacterHelper.CameraType.ThirdPerson:
							if (!isCrouch && (anim.GetBool("Aim") || isAlwaysTpAimEnabled) && (!anim.GetCurrentAnimatorStateInfo(0).IsName("Melee") || anim.GetCurrentAnimatorStateInfo(0).IsName("Melee") && !anim.GetBool("Attack")) && !anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch Melee") && !anim.GetBool("Pause") || isJump)
								CharacterController.Move(new Vector3(MoveVector.x, 0, MoveVector.z) * Time.deltaTime);
							break;
						case CharacterHelper.CameraType.FirstPerson:
							CharacterController.Move(new Vector3(MoveVector.x, 0, MoveVector.z) * Time.deltaTime);
							break;
						case CharacterHelper.CameraType.TopDown:
							if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Grenade_Throw"))
								CharacterController.Move(new Vector3(MoveVector.x, 0, MoveVector.z) * Time.deltaTime);
							break;
					}

					CanMove = true;
				}
				else
				{
					anim.SetBool("Move", false);
					CanMove = false;
				}

				// if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && alwaysTPAim)
				// {
				// 	if (CharacterHelper.CrouchMovement(anim) && (Mathf.Abs(anim.GetFloat("Horizontal")) > 0.7f || Mathf.Abs(anim.GetFloat("Vertical")) > 0.7f) && !inventoryManager.WeaponController.ActiveCrouchHands)
				// 	{
				// 		inventoryManager.WeaponController.CrouchHands();
				// 	}
				// 	else if (!CharacterHelper.CrouchMovement(anim) && inventoryManager.WeaponController.ActiveCrouchHands)
				// 	{
				// 		inventoryManager.WeaponController.CrouchHands();
				// 	}
				// }
			}

			CharacterController.Move(new Vector3(0, currentGravity, 0) * Time.deltaTime);
		}

		IEnumerator CrouchTimeout()
		{
			yield return new WaitForSeconds(1);
			crouchTimeOut = true;
		}

		#region JumpingProcess
		
		public void Jump()
		{
			if (isCrouch || isJump || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && inventoryManager.hasAnyWeapon && !anim.GetBool("HasWeaponTaken"))
				return;
			
			if (activeJump & !isPause && anim.GetBool("OnFloor"))
			{
				CalculateJumpParameters();
			}
		}
		
		void CalculateJumpParameters()
		{
			SmoothIKSwitch = 0;
			isJump = true;

			anim.SetBool("Jump", true);
			anim.SetBool("OnFloor", false);
			
			if (CharacterHelper.CheckIfCeiling(transform, CharacterController, out var hitInfo, Helper.LayerMask()))
			{
				if (hitInfo.distance <= headHeight + 2)
				{
					newJumpHeight = hitInfo.distance - 1;
			
					if (newJumpHeight < 2)
					{
						Debug.Log("Your character has not jump because the ceiling is too low.");
						return;
					}
					JumpPosition = transform.position.y + newJumpHeight;
				}
			}
			else
			{
				switch (TypeOfCamera)
				{
					case CharacterHelper.CameraType.ThirdPerson:
						JumpPosition = transform.position.y + TPSpeed.JumpHeight;
						break;
					case CharacterHelper.CameraType.FirstPerson:
						JumpPosition = transform.position.y + FPSpeed.JumpHeight;
						break;
					case CharacterHelper.CameraType.TopDown:
						JumpPosition = transform.position.y + TDSpeed.JumpHeight;
						break;
				}
			}
				
			flyingUp = true;
		}

		void JumpProcess()
		{
			if (isJump && flyingUp)
			{
				currentGravity = 0;
				
				var jumpPos = new Vector3(transform.position.x, JumpPosition, transform.position.z);
				var speed = 0f;
				
				switch (TypeOfCamera)
				{
					case CharacterHelper.CameraType.ThirdPerson:
						speed = TPSpeed.JumpSpeed;
						break;
					case CharacterHelper.CameraType.FirstPerson:
						speed = FPSpeed.JumpSpeed;
						break;
					case CharacterHelper.CameraType.TopDown:
						speed = TDSpeed.JumpSpeed;
						break;
				}
				
				transform.position = Vector3.Lerp(transform.position, jumpPos, 0.1f * speed * Time.deltaTime);

				if (Math.Abs(transform.position.y - JumpPosition) < 0.1f)
				{
					flyingUp = false;
				}
			}

			if (!anim.GetBool("OnFloor") && !flyingUp)
			{
				currentGravity = Mathf.Lerp(currentGravity, defaultGravity, 1 * Time.deltaTime);
				
				if (Math.Abs(currentGravity - defaultGravity) < 0.5f)
				{
					currentGravity = defaultGravity;
				}
			}
		}

		#endregion

		public void Sprint(bool active, string type)
		{
			if (!isPause && !isJump) //&& !isCrouch)
			{
				if (type == "press")
				{
					if (active)
					{
						if(isCrouch)
							DeactivateCrouch();
						
						ActivateSprint();
					}
					else
					{
						if(isSprint)
							DeactivateSprint();
					}
				}
				else
				{
					if (!isSprint)
					{
						if(isCrouch)
							DeactivateCrouch();
						
						ActivateSprint();
					}
					else
					{
						DeactivateSprint();
					}
				}
			}
		}

		void DeactivateSprint()
		{
			isSprint = false;
			anim.SetBool("Sprint", false);
		}
		
		void ActivateSprint()
		{
			isSprint = true;
			anim.SetBool("Sprint", true);
		}

		public void Crouch(bool active, string type)
		{
			if (isPause || isJump || TypeOfCamera == CharacterHelper.CameraType.TopDown || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson &&  inventoryManager.hasAnyWeapon && !anim.GetBool("HasWeaponTaken")) return;
			
			if (type == "press")
			{
				if (active)
					ActivateCrouch();
				else
				{
					if (isCrouch)
						DeactivateCrouch();
				}
			}
			else
			{
				if (!isCrouch)
					ActivateCrouch();
				else
					DeactivateCrouch();
			}
		}

		public void ActivateCrouch()
		{
			if (isRemoteCharacter || isAlwaysTpAimEnabled) return;
			
			if (isSprint) Sprint(false, "press");
				
			anim.SetBool("Crouch", true);

			if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
			{
				defaultCharacterCenter += CrouchHeight;
				StartCoroutine(ChangeBodyHeight());
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !isAlwaysTpAimEnabled)
			{
				if (inventoryManager.hasAnyWeapon && !isCrouch)
					inventoryManager.WeaponController.CrouchHands();
					
#if USK_MULTIPLAYER
				if(CharacterSync)
					CharacterSync.CrouchState();
#endif
			}
			isCrouch = true;
		}

		public void DeactivateCrouch()
		{
			if (!isRemoteCharacter)
			{
				RaycastHit hit;
				RaycastHit hit2;

				if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, out hit, 100, Helper.LayerMask()))
				{
					if (Physics.Raycast(BodyObjects.Hips.transform.position, Vector3.down, out hit2, 100, Helper.LayerMask()))
					{
						if (hit.point.y - 1 - hit2.point.y < headHeight * 1.5f)
						{
							return;
						}
					}
				}

				if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				{
					if(inventoryManager.WeaponController)
						inventoryManager.WeaponController.ActiveCrouchHands = false;
					
					defaultCharacterCenter -= CrouchHeight;
					StartCoroutine(ChangeBodyHeight());
				}
				else if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					if (inventoryManager.hasAnyWeapon && isCrouch)
						inventoryManager.WeaponController.CrouchHands();
					
#if USK_MULTIPLAYER
					if(CharacterSync)
						CharacterSync.CrouchState();
#endif
				}
				
				anim.SetBool("Crouch", false);
				isCrouch = false;
			}
		}

		public void ChangeMovementType()
		{
			if (TypeOfCamera == CharacterHelper.CameraType.TopDown || emulateTDModeLikeTP)
			{
				if (!isPause && !isJump && !anim.GetBool("Pause") && (!inventoryManager.WeaponController || !inventoryManager.WeaponController.isReloadEnabled))
				{
					if (TypeOfCamera == CharacterHelper.CameraType.TopDown || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && emulateTDModeLikeTP)
					{
						CharacterHelper.ChangeTDMode(this);
						CameraController.ReloadParameters();

						if (inventoryManager.WeaponController)
							WeaponsHelper.SetWeaponPositions(inventoryManager.WeaponController, true, DirectionObject);
					}
				}
				
#if USK_MULTIPLAYER
				if (CharacterSync && !isRemoteCharacter)
					CharacterSync.ChangeMovementType("TD");
#endif
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
			{
				if (movementType == CharacterHelper.MovementType.AllDirections)
				{
					if (CameraParameters.alwaysTPAimMode)
					{
						CameraParameters.alwaysTPAimMode = false;
						movementType = CharacterHelper.MovementType.Standard;
					}
					else
					{
						CameraParameters.alwaysTPAimMode = true;
					}
				}
				else movementType = CharacterHelper.MovementType.AllDirections;
					
					
				CameraController.ReloadParameters();
				anim.SetBool("Movement in All Directions", TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && (movementType == CharacterHelper.MovementType.AllDirections || isAlwaysTpAimEnabled));

				if (inventoryManager.WeaponController)
				{
					inventoryManager.WeaponController.ActiveCrouchHands = false;
					inventoryManager.WeaponController.isAimEnabled = false;
					WeaponsHelper.SetWeaponPositions(inventoryManager.WeaponController, true, DirectionObject);
				}
				
#if USK_MULTIPLAYER
				if (CharacterSync && !isRemoteCharacter)
					CharacterSync.ChangeMovementType("TP");
#endif
			}
		}
		
		IEnumerator SetWeaponPositionsDelay()
		{
			yield return new WaitForSeconds(1);
			StartCoroutine(SetHandsAfterJumpTimeout());
		}

		public void ChangeCameraType()
		{
			if(!AdjustmentScene && isPause || isJump || anim.GetBool("Pause") || inventoryManager.WeaponController && inventoryManager.WeaponController.isReloadEnabled)
				return;
			
			if(changeCameraTypeTimeout <= 1 || inventoryManager.WeaponController && !inventoryManager.WeaponController.setHandsPositionsAim)
				return;

			if (CameraController.isDeepAimEnabled)
			{
				// CameraController.isDeepAimEnabled = false;
				// inventoryManager.WeaponController.Aim(false, false, false);
				return;
			}

			if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson && CameraParameters.activeTD)
			{
				// if (inventoryManager.WeaponController && (CameraParameters.lockCamera || CameraParameters.alwaysTDAim) && inventoryManager.WeaponController.isAimEnabled)
				// {
				// 	inventoryManager.WeaponController.Aim(true, false, false);
				// 	StartCoroutine(ChangeCameraTimeout(CharacterHelper.CameraType.TopDown));
				// }
				// else
				// {
					CharacterHelper.SwitchCamera(TypeOfCamera, CharacterHelper.CameraType.TopDown, this);
				// }
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson && CameraParameters.activeTP)
			{
				CharacterHelper.SwitchCamera(TypeOfCamera, CharacterHelper.CameraType.ThirdPerson, this);
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraParameters.activeFP)
			{
				CharacterHelper.SwitchCamera(TypeOfCamera, emulateTDModeLikeTP && CameraParameters.activeTP ? CharacterHelper.CameraType.ThirdPerson : CharacterHelper.CameraType.FirstPerson, this);
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraParameters.activeTD)
			{
				if (inventoryManager.WeaponController && (CameraParameters.lockCamera || CameraParameters.alwaysTDAim) && inventoryManager.WeaponController.isAimEnabled)
				{
					inventoryManager.WeaponController.Aim(true, false, false);
					StartCoroutine(ChangeCameraTimeout(CharacterHelper.CameraType.ThirdPerson));
				}
				else
				{
					CharacterHelper.SwitchCamera(TypeOfCamera, emulateTDModeLikeTP ? CharacterHelper.CameraType.ThirdPerson : CharacterHelper.CameraType.TopDown, this);
				}
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.TopDown && CameraParameters.activeTP)
			{
				CharacterHelper.SwitchCamera(TypeOfCamera, CharacterHelper.CameraType.ThirdPerson, this);
			}
			else if (TypeOfCamera == CharacterHelper.CameraType.TopDown && CameraParameters.activeFP)
			{
				CharacterHelper.SwitchCamera(TypeOfCamera, CharacterHelper.CameraType.FirstPerson, this);
			}
			
			changeCameraTypeTimeout = 0;
		}

		public void ChangeCameraType(CharacterHelper.CameraType type)
		{
			if(!AdjustmentScene && isPause)
				return;
			
			//!!!
			// if(changeCameraTypeTimeout <= 0.5f)// || inventoryManager.WeaponController && !inventoryManager.WeaponController.setHandsPositionsAim)
			// 	return;
			
			changeCameraTypeTimeout = 0;
			
			CharacterHelper.SwitchCamera(TypeOfCamera, type, this);
		}

		private void SnapAlignCharacterWithCamera()
		{
			// if(!CameraController.setCameraType) return;
			
			if (!anim.GetBool("Aim") && (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk_Forward") || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk_Forward_Start") || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk_Start_90_L") || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk_Start_90_R") ||
			                             anim.GetCurrentAnimatorStateInfo(0).IsName("Run_Forward") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run_Start") ||
										anim.GetCurrentAnimatorStateInfo(0).IsName("Run_Start_90_L") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run_Start_90_R")) || CharacterHelper.CrouchMovement(anim))
			{
				var angle = angleBetweenCharacterAndCamera;

				var _angle = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + angle, transform.eulerAngles.z);

				var speed = 3;
				
				if (Mathf.Abs(angle) < 135)
				{
					transform.rotation = Quaternion.Slerp(transform.rotation, _angle, speed * Time.deltaTime);
				}
			}
			else if ((Math.Abs(MoveVector.x) > 0.8f || Math.Abs(MoveVector.z) > 0.8f) && (anim.GetBool("Aim") || TypeOfCamera == CharacterHelper.CameraType.FirstPerson || TypeOfCamera == CharacterHelper.CameraType.TopDown && !CameraParameters.lockCamera || anim.GetBool("Movement in All Directions")))
			{
				var angle = Mathf.DeltaAngle(transform.eulerAngles.y, CameraController.MainCamera.eulerAngles.y);
				
				var _angle = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + angle, transform.eulerAngles.z);

				var speed = 5;
				transform.rotation = Quaternion.Slerp(transform.rotation, _angle, speed * Time.deltaTime);

			}
			else if (anim.GetBool("Aim") || anim.GetBool("Movement in All Directions") || TypeOfCamera == CharacterHelper.CameraType.TopDown || anim.GetCurrentAnimatorStateInfo(0).IsName("Falling Loop"))
			{
				var directionAngle = Helper.AngleBetween(transform.forward, CameraController.BodyLookAt.position - transform.position);
				
				var angle = Mathf.DeltaAngle(transform.eulerAngles.y, CameraController.MainCamera.eulerAngles.y);
				
				var _angle = TypeOfCamera == CharacterHelper.CameraType.TopDown
					? Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + (!CameraParameters.lockCamera ? angle : directionAngle), transform.eulerAngles.z)
					: Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + angle, transform.eulerAngles.z);

				transform.rotation = Quaternion.Slerp(transform.rotation, _angle, 5 * Time.deltaTime);

			}

			CurrentRotation = transform.rotation;
		}

		public void BodyLookAt(Transform bodyLookAt)
		{
			if (!isRemoteCharacter)
			{
				if (inventoryManager.WeaponController)
				{
					if (!inventoryManager.hasAnyWeapon || TypeOfCamera != CharacterHelper.CameraType.TopDown &&
					    (!anim.GetBool("Aim") && !isAlwaysTpAimEnabled || inventoryManager.WeaponController.Attacks[inventoryManager.WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee)
						 || isCrouch && !anim.GetBool("Aim") && (!isAlwaysTpAimEnabled || CharacterHelper.CrouchMovement(anim)) &&
					    !anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Aim_Idle") &&
					    !anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Aim_Turn_90_L") &&
					    !anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Aim_Turn_90_R"))
					{
						var speed = isCrouch ? 10 : 3;

						bodyRotationUpLimit_y = Mathf.Lerp(bodyRotationUpLimit_y, 0, speed * Time.deltaTime);
						bodyRotationDownLimit_y = Mathf.Lerp(bodyRotationDownLimit_y, 0, speed * Time.deltaTime);

						bodyRotationUpLimit_x = Mathf.Lerp(bodyRotationUpLimit_x, 0, speed * Time.deltaTime);
						bodyRotationDownLimit_x = Mathf.Lerp(bodyRotationDownLimit_x, 0, speed * Time.deltaTime);

					}
					else
					{
						var minLimit = TypeOfCamera != CharacterHelper.CameraType.TopDown ? CameraParameters.fpXLimitMin : CameraParameters.tdXLimitMin;
						var maxLimit = TypeOfCamera != CharacterHelper.CameraType.TopDown ? CameraParameters.fpXLimitMax : CameraParameters.tdXLimitMax;
						
						if (Math.Abs(anim.GetFloat("CameraAngle")) < 45)
						{
							bodyRotationUpLimit_y = Mathf.Lerp(bodyRotationUpLimit_y, 60, 3 * Time.deltaTime);
							bodyRotationDownLimit_y = Mathf.Lerp(bodyRotationDownLimit_y, -60, 3 * Time.deltaTime);

							bodyRotationUpLimit_x = Mathf.Lerp(bodyRotationUpLimit_x, maxLimit + 30, 3 * Time.deltaTime);
							bodyRotationDownLimit_x = Mathf.Lerp(bodyRotationDownLimit_x, minLimit, 3 * Time.deltaTime);
						}
						else
						{
							bodyRotationUpLimit_y = Mathf.Lerp(bodyRotationUpLimit_y, 60, 1 * Time.deltaTime);
							bodyRotationDownLimit_y = Mathf.Lerp(bodyRotationDownLimit_y, -60, 1 * Time.deltaTime);

							bodyRotationUpLimit_x = Mathf.Lerp(bodyRotationUpLimit_x, maxLimit + 30, 1 * Time.deltaTime);
							bodyRotationDownLimit_x = Mathf.Lerp(bodyRotationDownLimit_x, minLimit, 1 * Time.deltaTime);
						}
					}
				}
				else
				{
					bodyRotationUpLimit_y = Mathf.Lerp(bodyRotationUpLimit_y, 0, 3 * Time.deltaTime);
					bodyRotationDownLimit_y = Mathf.Lerp(bodyRotationDownLimit_y, 0, 3 * Time.deltaTime);

					bodyRotationUpLimit_x = Mathf.Lerp(bodyRotationUpLimit_x, 0, 3 * Time.deltaTime);
					bodyRotationDownLimit_x = Mathf.Lerp(bodyRotationDownLimit_x, 0, 3 * Time.deltaTime);
				}
			}

			var direction = bodyLookAt.position - DirectionObject.position;

			var middleAngleX = Helper.AngleBetween(direction, DirectionObject).x;
			var middleAngleY = Helper.AngleBetween(direction, DirectionObject).y;

			if (middleAngleY > bodyRotationUpLimit_y)
				middleAngleY = bodyRotationUpLimit_y;
			else if (middleAngleY < bodyRotationDownLimit_y)
				middleAngleY = bodyRotationDownLimit_y;

			if (middleAngleX > bodyRotationUpLimit_x)
			{
				middleAngleX = bodyRotationUpLimit_x;
				bodyLimit = true;
			}
			else if (middleAngleX < bodyRotationDownLimit_x)
			{
				middleAngleX = bodyRotationDownLimit_x;
				bodyLimit = true;
			}
			else
			{
				bodyLimit = false;
			}

			if (AdjustmentScene) return;
			
			if (!isCrouch)
			{
				BodyObjects.TopBody.RotateAround(DirectionObject.position, Vector3.up, -middleAngleY);
				BodyObjects.TopBody.RotateAround(DirectionObject.position, DirectionObject.TransformDirection(Vector3.right), -middleAngleX);
			}
			else
			{
				BodyObjects.TopBody.RotateAround(DirectionObject.position, Vector3.up, -middleAngleY);
				BodyObjects.TopBody.RotateAround(DirectionObject.position, DirectionObject.TransformDirection(Vector3.right), -middleAngleX);
			}

		}

		public void TopBodyOffset()
		{
			if (!AdjustmentScene)
			{
				BodyObjects.TopBody.Rotate(Vector3.right, CharacterOffset.xRotationOffset);
				BodyObjects.TopBody.Rotate(Vector3.up, CharacterOffset.yRotationOffset);
				BodyObjects.TopBody.Rotate(Vector3.forward, CharacterOffset.zRotationOffset);
			}
			else
			{
				BodyObjects.TopBody.eulerAngles = new Vector3(CharacterOffset.xRotationOffset, CharacterOffset.yRotationOffset, CharacterOffset.zRotationOffset);
			}
		}

		public void CharacterRotation()
		{
			if (DebugMode) return;

			BodyLocalEulerAngles = BodyObjects.TopBody.localEulerAngles;

			if (BodyLocalEulerAngles.x > 180)
				BodyLocalEulerAngles.x -= 360;
			if (BodyLocalEulerAngles.y > 180)
				BodyLocalEulerAngles.y -= 360;
			
			var hipsAngleY = transform.eulerAngles.y;
			var spineAngleY = BodyObjects.TopBody.eulerAngles.y - CharacterOffset.yRotationOffset;
			var middleAngleY = Mathf.DeltaAngle(hipsAngleY, spineAngleY);

			if (TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
			{
				if (middleAngleY > 50)
				{
					RotationAngle = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + (middleAngleY - 50), transform.eulerAngles.z);

					transform.rotation = Quaternion.Slerp(transform.rotation, RotationAngle, middleAngleY - 50 * Time.deltaTime);
				}

				else if (middleAngleY < -50)
				{
					RotationAngle = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - (-50 - middleAngleY), transform.eulerAngles.z);
					transform.rotation = Quaternion.Slerp(transform.rotation, RotationAngle, -middleAngleY - 50 * Time.deltaTime);
				}
			}

			hipsAngleX = transform.eulerAngles.x;
			spineAngleX = BodyObjects.TopBody.eulerAngles.x - CharacterOffset.xRotationOffset;
			middleAngleX = Mathf.DeltaAngle(hipsAngleX, spineAngleX);
		}
		

		private void CurrentMoveDirection()
		{
			var forward = false;
			var backward = false;
			var left = false;
			var right = false;
			
			NullDirectionAnimations();
			
			var newDirVector = Helper.AngleBetween(transform.forward, directionVector);
			
			if (TypeOfCamera != CharacterHelper.CameraType.TopDown || TypeOfCamera == CharacterHelper.CameraType.TopDown && !CameraParameters.lockCamera)
			{
				
				if (directionVector.z > 0)
					forward = true;
				if (directionVector.z < 0)
					backward = true;
				if (directionVector.x > 0)
					right = true;
				if (directionVector.x < 0)
					left = true;

				if (forward)
				{
					if (left)
					{
						MoveDirection = Direction.ForwardLeft;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("ForwardLeft", true);
					}
					else if (right)
					{
						MoveDirection = Direction.ForwardRight;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("ForwardRight", true);
					}
					else
					{
						MoveDirection = Direction.Forward;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("Forward", true);
					}
				}
				else if (backward)
				{
					if (left)
					{
						MoveDirection = Direction.BackwardLeft;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("BackwardLeft", true);
					}
					else if (right)
					{
						MoveDirection = Direction.BackwardRight;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("BackwardRight", true);
					}
					else
					{
						MoveDirection = Direction.Backward;
						
						// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
							anim.SetBool("Backward", true);
					}
				}
				else if (right)
				{
					MoveDirection = Direction.Right;
					
					// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim)
						anim.SetBool("Right", true);
				}
				else if (left)
				{
					MoveDirection = Direction.Left;
					
					// if(TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && CameraController.CameraAim) 
						anim.SetBool("Left", true);
				}
				else
				{
					MoveDirection = Direction.Stationary;
				}
			}
			else
			{
				if(Math.Abs(directionVector.x) < 0.01f && Math.Abs(directionVector.z) < 0.01f) 
					MoveDirection = Direction.Stationary;
				else
				{
					if (newDirVector > -23 && newDirVector <= 23)
					{
						MoveDirection = Direction.Forward;
						anim.SetBool("Forward", true);
					}
					else if (newDirVector > 23 && newDirVector <= 68)
					{
						MoveDirection = Direction.ForwardRight;
						anim.SetBool("ForwardRight", true);
					}
					else if(newDirVector > 68 && newDirVector <= 113)
					{
						MoveDirection = Direction.Right;
						anim.SetBool("Right", true);
					}
					else if(newDirVector > 113 && newDirVector <= 158)
					{
						MoveDirection = Direction.BackwardRight;
						anim.SetBool("BackwardRight", true);
					}
					else if(newDirVector <= -23 && newDirVector > -68)
					{
						MoveDirection = Direction.ForwardLeft;
						anim.SetBool("ForwardLeft", true);
					}
					else if (newDirVector <= -68 && newDirVector > -113)
					{
						MoveDirection = Direction.Left;
						anim.SetBool("Left", true);
					}
					else if (newDirVector <= -113 && newDirVector > -158)
					{
						MoveDirection = Direction.BackwardLeft;
						anim.SetBool("BackwardLeft", true);
					}
					else
					{
						MoveDirection = Direction.Backward;
						anim.SetBool("Backward", true);
					}
				}
			}
		}

		void NullDirectionAnimations()
		{
			anim.SetBool("Forward", false);
			anim.SetBool("ForwardRight", false);
			anim.SetBool("ForwardLeft", false);
			anim.SetBool("Left", false);
			anim.SetBool("Right", false);
			anim.SetBool("BackwardLeft", false);
			anim.SetBool("BackwardRight", false);
			anim.SetBool("Backward", false);
		}

		float MoveSpeed(CharacterHelper.Speeds speeds)
		{
			var moveSpeed = 0f;

			switch (MoveDirection)
			{
				case Direction.Stationary:
					moveSpeed = 0;
					break;
				case Direction.Forward:
					moveSpeed = ChoiceSpeed(speeds.NormForwardSpeed, speeds.RunForwardSpeed, speeds.CrouchForwardSpeed);
					break;
				case Direction.Backward:
					moveSpeed = ChoiceSpeed(speeds.NormBackwardSpeed, speeds.RunBackwardSpeed, speeds.CrouchBackwardSpeed);
					break;
				case Direction.Right:
					moveSpeed = ChoiceSpeed(speeds.NormLateralSpeed, speeds.RunLateralSpeed, speeds.CrouchLateralSpeed);
					break;
				case Direction.Left:
					moveSpeed = ChoiceSpeed(speeds.NormLateralSpeed, speeds.RunLateralSpeed, speeds.CrouchLateralSpeed);
					break;
				case Direction.ForwardRight:
					moveSpeed = ChoiceSpeed(speeds.NormForwardSpeed, speeds.RunForwardSpeed, speeds.CrouchForwardSpeed);
					break;
				case Direction.ForwardLeft:
					moveSpeed = ChoiceSpeed(speeds.NormForwardSpeed, speeds.RunForwardSpeed, speeds.CrouchForwardSpeed);
					break;
				case Direction.BackwardRight:
					moveSpeed = ChoiceSpeed(speeds.NormBackwardSpeed, speeds.RunBackwardSpeed, speeds.CrouchBackwardSpeed);
					break;
				case Direction.BackwardLeft:
					moveSpeed = ChoiceSpeed(speeds.NormBackwardSpeed, speeds.RunBackwardSpeed, speeds.CrouchBackwardSpeed);
					break;
			}
			
			return moveSpeed / speedDivider;
		}

		float ChoiceSpeed(float norm, float run, float crouch)
		{
			float speed;
			
			if (isSprint)
				speed = run;
			else if (isCrouch)
				speed = crouch;
			else
				speed = norm;
			
			return speed;
		}

		IEnumerator ChangeCameraTimeout(CharacterHelper.CameraType type)
		{
			while (true)
			{
				if (inventoryManager.WeaponController.setHandsPositionsAim)
				{
					if(type == CharacterHelper.CameraType.TopDown && emulateTDModeLikeTP)
						CharacterHelper.SwitchCamera(TypeOfCamera, CharacterHelper.CameraType.ThirdPerson, this);
					else CharacterHelper.SwitchCamera(TypeOfCamera, type, this);

					StopCoroutine("ChangeCameraTimeout");
					break;
				}

				yield return 0;
			}
		}

		//this function is used in animation events
		public void PlayStepSound(float volume)
		{
			if(!anim.GetBool("Move")) return;
			
			var hit = new RaycastHit();

			if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 100, Helper.LayerMask()))
			{
				var surface = hit.collider.GetComponent<Surface>();
				
				if (FeetAudioSource && surface && surface.CharacterFootstepsSounds.Length > 0)
					CharacterHelper.PlayStepSound(surface, FeetAudioSource, characterTag, volume, "character");

			}
		}

		IEnumerator ChangeBodyHeight()
		{
			while (true)
			{
				var crouchHeight = Mathf.Lerp(CharacterController.center.y, defaultCharacterCenter, 5 * Time.deltaTime);
				CharacterController.center = new Vector3(CharacterController.center.x, crouchHeight, CharacterController.center.z);

				if (Math.Abs(crouchHeight - defaultCharacterCenter) < 0.1f && isCrouch)
				{
					CharacterController.center = new Vector3(CharacterController.center.x, defaultCharacterCenter, CharacterController.center.z);
					break;
				}

				if (Math.Abs(crouchHeight - defaultCharacterCenter) < 0.1f && !isCrouch)
				{
					CharacterController.center = new Vector3(CharacterController.center.x, defaultCharacterCenter, CharacterController.center.z);
					break;
				}

				yield return 0;
			}
		}
		
		#region HealthMethods
		

		public void Damage(float damage)
		{
			TakingDamage(damage, "enemy", false);
		}
		
		public void Damage(float damage, string attackType, Helper.ActorID attackerActorNumber = null)
		{
			TakingDamage(damage, attackType, true, attackerActorNumber);
		}

		public void TakingDamage(float damage, string attackType, bool syncInMultiplayer, Helper.ActorID attackerActorID = null)
		{
			if (health <= 0) return;

			if (oneShotOneKill)
				damage = (int) health + 50;

			currentDamageSoundsDelay += 1 * (attackType == "fire" ? Time.deltaTime : 1);

			if (audioSource && damageSounds.Count > 0 && currentDamageSoundsDelay >= playDamageSoundsDelay)
			{
				currentDamageSoundsDelay = 0;
				
				var index = Random.Range(0, damageSounds.Count - 1);
				if (damageSounds[index])
				{
					audioSource.PlayOneShot(damageSounds[index]);
				}
			}

			if (attackerActorID != null)
			{
#if USK_MULTIPLAYER
				if (CharacterSync)
				{
					// update the list of opponents who attacked the player (only on the main client)
					if (CharacterSync.photonView.IsMine)
					{
						if (!CharacterSync.opponentsWhoAttackedPlayer.Exists(actor => actor.type == attackerActorID.type && actor.actorID == attackerActorID.actorID))
							CharacterSync.opponentsWhoAttackedPlayer.Add(attackerActorID);
					}
					//


					if (syncInMultiplayer)
						CharacterSync.TakingDamage(damage, attackerActorID, attackType);
				}
#endif
			}
			
			
			health -= damage;

#if USK_MULTIPLAYER
			if (CharacterSync && CharacterSync.photonView && !CharacterSync.photonView.IsMine) return;
#endif
			ControlHealth(attackType, attackerActorID);
			// }
		}

		#endregion
		
		#region FeetIK

		void OnAnimatorIK(int layerIndex)
		{
			if(isCharacterInLobby) return;
			
			IKVariables.LastPelvisPosition = anim.bodyPosition.y;
			
			if (layerIndex != 0) return;

			if (!isRemoteCharacter)
			{
				if (TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
				{
					if (isJump && (anim.GetCurrentAnimatorStateInfo(0).IsName("Jumple_Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump_Movemement_L")
					                                                                         || anim.GetCurrentAnimatorStateInfo(0).IsName("Falling Loop") ||
					                                                                         anim.GetCurrentAnimatorStateInfo(0).IsName("Jump_Land_Hard")
					                                                                         || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump_Land_Walk Loop") ||
					                                                                         anim.GetCurrentAnimatorStateInfo(0).IsName("Start Falling")))
					{
						if (SmoothIKSwitch > 0.1f)
							SmoothIKSwitch = Mathf.Lerp(SmoothIKSwitch, 0, 5 * Time.deltaTime);
						else SmoothIKSwitch = 0;
					}
					else
					{
						if (SmoothIKSwitch < 0.9f)
							SmoothIKSwitch = Mathf.Lerp(SmoothIKSwitch, 0, 5 * Time.deltaTime);
						else SmoothIKSwitch = 1;
					}
				}
				else
				{
					if (!isJump)
						SmoothIKSwitch = 1;
				}
			}

			anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, SmoothIKSwitch);
			anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat("RightFoot"));

			IKHelper.MoveFeetToIkPoint(this, "right");

			anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, SmoothIKSwitch);
			anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat("LeftFoot"));

			IKHelper.MoveFeetToIkPoint(this, "left");
		}

		void FixedUpdate()
		{
			if(isCharacterInLobby) return;
			
			if(isRemoteCharacter) return;

			IKHelper.AdjustFeetTarget(this, "right");
			IKHelper.AdjustFeetTarget(this, "left");

			IKHelper.FeetPositionSolver(this, "right");
			IKHelper.FeetPositionSolver(this, "left");
		}

		#endregion


#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if(AdjustmentScene)
				return;

			var position = BodyObjects.Hips.position;
			var up = transform.up;
			
			// if (Application.isPlaying)
			// {
			// 	Handles.zTest = CompareFunction.Less;
			// 	Handles.color = new Color32(255, 255, 255, 100);
			// 	
			// 	Handles.DrawSolidDisc(position, up, noiseRadius);
			//
			// 	Handles.zTest = CompareFunction.Greater;
			// 	Handles.color = new Color32(255, 255, 255, 20);
			// 	Handles.DrawSolidDisc(position, up, noiseRadius);
			// }
			// else
			// {
			// 	Handles.zTest = CompareFunction.Less;
			// 	Handles.color = new Color32(255, 255, 255, 100);
			// 	Handles.DrawSolidDisc(position, up, IdleNoise);
			//
			// 	Handles.zTest = CompareFunction.Greater;
			// 	Handles.color = new Color32(255, 255, 255, 20);
			// 	Handles.DrawSolidDisc(position, up, IdleNoise);
			// }
		}
#endif
	}
}


