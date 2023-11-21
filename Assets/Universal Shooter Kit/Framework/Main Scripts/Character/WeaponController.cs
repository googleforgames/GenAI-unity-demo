using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class WeaponController : MonoBehaviour
    {
        public Controller Controller;
        public InventoryManager WeaponManager;
        public WeaponController OriginalScript;

        public GameObject ScopeScreen;

        public WeaponsHelper.IKObjects IkObjects;
        public CharacterHelper.BodyObjects BodyObjects;

        public List<WeaponsHelper.Attack> Attacks = new List<WeaponsHelper.Attack>() {new WeaponsHelper.Attack()};
        
        public List<WeaponsHelper.IKSlot> IkSlots = new List<WeaponsHelper.IKSlot>{new WeaponsHelper.IKSlot()};

        public IKHelper.IkDebugMode DebugMode;

        public int currentAttack;
        
        [Range(20,1)]public float scopeDepth;
        [Range(20,1)]public float aimTextureDepth;

        public float BarrelRotationSpeed;

        [Tooltip("The heavier the weapon, the slower a character moves")]
        [Range(1, 10)] public int weaponWeight = 5;

        public AudioClip DropWeaponAudio;
        public AudioClip PickUpWeaponAudio;

        public WeaponsHelper.WeaponAnimation weaponAnimations;

        [Tooltip("This image will be displayed in the inventory, multiplayer lobby and during the game")]
        public Texture weaponImage;
        [Tooltip("If the weapon is used as a pick-up item, this image will be displayed on the mini-map (leave blank if you don't need it)")]
        public Texture blipImage;
        public Texture aimCrosshairTexture;
        public Color fillColor = Color.black;

        public List<WeaponsHelper.WeaponInfo> WeaponInfos = new List<WeaponsHelper.WeaponInfo>{new WeaponsHelper.WeaponInfo()};
        public List<WeaponsHelper.WeaponInfo> CurrentWeaponInfo = new List<WeaponsHelper.WeaponInfo>{new WeaponsHelper.WeaponInfo()};

        public Vector3 attackDirection;
        public Vector3 lastAttackDirection;
        
        public ProjectSettings projectSettings;

        public RaycastHit hit;

        #region Inspector Variables

        public string currentTab;
        public List<string> enumNames = new List<string>{"Slot 1"};
        public List<string> attacksNames = new List<string>{"Bullet attack"};

        public int inspectorTabTop;
        public int bulletTypeInspectorTab;
        public int inspectorTabBottom;
        public int settingsSlotIndex;
        public int settingsSlotIndexInAdjustment;
        public int lastSettingsSlotIndex;
        public int aimInspectorTabIndex;
        public int animationsInspectorTabIndex;
        
        public bool ActiveDebug;
        public bool PickUpWeapon;
        public bool showCrosshairPositions;

        public Canvas inspectorCanvas;
        
        public Image upPart;
        public Image leftPart;
        public Image rightPart;
        public Image downPart;
        public Image middlePart;
        
        public string curName;

        public bool delete;
        public bool rename;
        public bool renameError;
        
        #endregion
        
        public bool autoReload;
        public bool aimForAttack = true;
        public bool canUseValuesInAdjustment;
        public bool switchToFpCamera;
        public bool wasSetSwitchToFP;
        public bool useScope;
        public bool useAimTexture;
        public bool isReloadEnabled;
        public bool canceledReload;
        public bool reloadProcessHasBeenStarted;
        public bool attackAudioPlay;
        public bool isAimEnabled;
        public bool uiButtonAttack;
        public bool activeAimMode = true;
        public bool canAttack;
        public bool canDrawGrenadesPath = true;
        public bool setHandsPositionsAim = true;
        public bool canEnableAimTexture;
        public bool setHandsPositionsObjectDetection = true;
        public bool setHandsPositionsCrouch = true;
        public bool enableObjectDetectionMode = true;
        public bool isShotgun;
        public bool aimWhileJumping;
        public bool correctHandsPosition;

        //Check bools for IK
        public bool CanUseIK;
        public bool CanUseElbowIK;
        public bool CanUseWallIK;
        public bool CanUseCrouchIK;
        public bool CanUseAimIK;
        public bool hasAimIKChanged;
        public bool hasWallIKChanged;
        public bool hasCrouchIKChanged;
        public bool DetectObject;
        public bool pinLeftObject;

        public bool isMultiplayerWeapon;
        
        // temporary camera for processing the direction of shooting with top down view
        public Transform tempCamera;
        
        public List<Transform> bulletHitsDecals = new List<Transform>();

        private bool playNoAmmoSound;
        private bool hasAttackButtonPressed;
        private bool setColliderPosition;
        private bool aimWasSetBeforeAttack;
        public bool aimTimeout = true;
        public bool applyChanges = true;
        private bool startAutoReloadProcess;
        public bool ActiveCrouchHands;
        public bool aimIsActivatedByHoldingButton;
        public bool firstTake;
        public bool canUseSmoothWeaponRotation;

        public bool wasEmptyMagSoundPlayed;
        
        public float rateOfAttackTimer;
        private float disableAimAfterAttackTimer;
        public Vector2 currentBulletsScatter;
        [Tooltip("The speed at which the character will aim")]
        [Range(0.1f, 5)] public float aimingSpeed = 1;
        public float aimTimer;
        public float crouchTimer;
        public Quaternion handRotationWhenTransitionStarts;
        
        public bool setCrouchHandsFirstly = true;
        public bool resetBobbing;
        private bool activateMeleeTimer;

        private int lastTrailPoint;
        public int animationIndex;
        public int crouchAnimationIndex;
        public int lastAttackAnimationIndex = -1;
        public int lastCrouchAttackAnimationIndex = -1;
        public int numberOfUsedHands = 2;

        public string weaponID;

        private Vector3 lastTrailPosition;

        private float rateOfAttackLimit;
        private float changeAnimationSpeedTimeout;
        public float placeHandsToStartPositionTimeout;

        public IKHelper.HandsPositions RightHandPositions;
        public IKHelper.HandsPositions LeftHandPositions;

        public CharacterHelper.BobbingValues bobbingValues;

        private Coroutine reloadRoutine;
        
        private void OnEnable()
        {
            setCrouchHandsFirstly = false;
#if USK_MULTIPLAYER
            if (FindObjectOfType<LobbyManager>())
                return;

#if USK_ADVANCED_MULTIPLAYER 
            if (FindObjectOfType<AdvancedLobbyManager>())
                return;
#endif
#endif
            
            if (Controller && !Controller.AdjustmentScene)
            {
                WeaponsHelper.SetHandsSettingsSlot(ref settingsSlotIndex, Controller.characterTag, Controller.TypeOfCamera, this);
            }

            inspectorCanvas.gameObject.SetActive(false);
                
            aimTimeout = true;
            applyChanges = true;
            firstTake = false;

            lastSettingsSlotIndex = settingsSlotIndex;
            
            CurrentWeaponInfo.Clear();

            for (var i = 0; i < WeaponInfos.Count; i++)
            {
                var info = new WeaponsHelper.WeaponInfo();
                info.Clone(WeaponInfos[i]);
                CurrentWeaponInfo.Add(info);
            }

            WeaponsHelper.PlaceWeapon(CurrentWeaponInfo[settingsSlotIndex], transform);

            var rigidbodyComponent = GetComponent<Rigidbody>();

            if (rigidbodyComponent)
            {
                rigidbodyComponent.useGravity = false;
                rigidbodyComponent.isKinematic = true;
            }

            if(GetComponent<BoxCollider>()) GetComponent<BoxCollider>().isTrigger = true;
            else if (GetComponent<SphereCollider>()) GetComponent<SphereCollider>().isTrigger = true;

            if (!Controller)
                return;
            
            BodyObjects = Controller.BodyObjects;

            BarrelRotationSpeed = 0;

            foreach (var attack in Attacks)
            {
                if (attack.magazine && attack.TempMagazine.Count == 0 && attack.magazine.activeInHierarchy)
                {
                    HideAndCreateNewMagazine();
                }

                if (attack.AttackType == WeaponsHelper.TypeOfAttack.Bullets)
                {
                    if (bulletTypeInspectorTab == 0)
                    {
                        if(attack.BulletsSettings[0].Active)
                            attack.currentBulletType = 0;
                        else attack.currentBulletType = 1;

                        attack.attackImage = attack.BulletsSettings[0].attackImage;
                    }
                    else if (bulletTypeInspectorTab == 1)
                    {
                        if (attack.BulletsSettings[1].Active)
                            attack.currentBulletType = 1;
                        else attack.currentBulletType = 0;
                        
                        attack.attackImage = attack.BulletsSettings[1].attackImage;
                    }
                }
                else if(attack.AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                {
                    attack.currentBulletType = 1;
                    attack.BulletsSettings[1].Active = true;
                }
            }

            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                ChangeBulletType();

            if (!isMultiplayerWeapon)
            {
                tempCamera = new GameObject("tempCamera").transform;
                tempCamera.hideFlags = HideFlags.HideInHierarchy;

                currentBulletsScatter = new Vector2(Attacks[currentAttack].bulletsScatterX, Attacks[currentAttack].bulletsScatterY);
            }
            
            foreach (var attack in Attacks.Where(attack => attack.AttackCollider))
            {
                CharacterHelper.SetMeleeCollider(ref attack.AttackCollider, Controller, attack.weapon_damage, attack.AttackType == WeaponsHelper.TypeOfAttack.Melee ? USKIneractionWithEmeraldAI.ColliderType.Melee : USKIneractionWithEmeraldAI.ColliderType.Fire);
            }
            
            rateOfAttackTimer = Attacks[currentAttack].RateOfAttack;
            
            CanUseIK = false;

            DetectObject = false;

            if (GetComponent<AudioSource>()) GetComponent<AudioSource>().enabled = true;

            if (!Controller.AdjustmentScene) SetIK();
            else StartCoroutine(SetIKTimeout());
        }

        void SetIK()
        {
            if(!IkObjects.RightObject)
                Helper.CreateObjects(IkObjects, transform, Controller.AdjustmentScene, true, Controller.projectSettings.CubesSize, Helper.CubeSolid.Wire);
            
            IKHelper.CheckIK(ref CanUseElbowIK, ref CanUseIK, ref CanUseAimIK, ref CanUseWallIK, ref CanUseCrouchIK, CurrentWeaponInfo[settingsSlotIndex]);

            if (Controller.isCrouch && (!Controller.isAlwaysTpAimEnabled || Controller.anim.GetBool("Move")) && CanUseCrouchIK && Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                ActiveCrouchHands = true;
            
            IKHelper.PlaceAllIKObjects(this, CurrentWeaponInfo[settingsSlotIndex],true, Controller.DirectionObject);
            
            canUseValuesInAdjustment = true;
        }

        IEnumerator SetIKTimeout()
        {
            yield return new WaitForSeconds(0.5f);
            SetIK();
        }

        void Start()
        {
            if (useAimTexture)
                switchToFpCamera = true;

            if (!Controller.AdjustmentScene)
                ActiveDebug = false;


            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                Attacks[currentAttack].maxAmmo = Attacks[currentAttack].inventoryAmmo;

            if (SaveManager.Instance && SaveManager.Instance.saveInventory && SaveManager.Instance.saveWeaponsAmmoAmount && SaveManager.Instance.HasAnyData(SaveManager.CharacterDataFileName))
                return;
                
            Attacks[currentAttack].curAmmo = Attacks[currentAttack].maxAmmo;

//            if(!Controller.AdjustmentScene && !CanUseWallIK && enableObjectDetectionMode)
//                Debug.LogWarning("You haven't set the position of the hands for the Objects Detection mode. Go to the [Tools -> USK -> Adjust] to do that.", gameObject);
        }

        // private void FixedUpdate()
        // {
        //     WeaponsHelper.SmoothWeaponMovement(this);
        // }
        // private void LateUpdate()
        // {
        //     WeaponsHelper.SmoothWeaponMovement(this);
        // }

        void Update()
        {
            if (lastSettingsSlotIndex != settingsSlotIndex)
            {
                WeaponsHelper.SetWeaponPositions(this, true, Controller.DirectionObject);
                lastSettingsSlotIndex = settingsSlotIndex;
            }
            
            if (Mathf.Abs(Controller.CameraController.mouseDelta.x) > 0)
                canUseSmoothWeaponRotation = true;

            WeaponsHelper.SmoothWeaponMovement(this);
            
            if (!Controller.AdjustmentScene && Controller.ColliderToObjectsDetection && !DetectObject && setHandsPositionsObjectDetection)
            {
                if(Attacks[currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee && Attacks[currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade && Attacks[currentAttack].AttackSpawnPoint)
                    Controller.ColliderToObjectsDetection.transform.position = Attacks[currentAttack].AttackSpawnPoint.transform.position;
                else if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                    Controller.ColliderToObjectsDetection.transform.position = IkObjects.RightObject.transform.position;
            }
            
            WeaponsHelper.MinigunBarrelRotation(this);

            rateOfAttackTimer += Time.deltaTime;
            disableAimAfterAttackTimer += Time.deltaTime;

            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && isAimEnabled && disableAimAfterAttackTimer > 5 && aimWasSetBeforeAttack && !isReloadEnabled)
            {
                Aim(true, false, false);
                aimWasSetBeforeAttack = false;
            }
            
            IKHelper.ManageHandsPositions(this);

            if (!WeaponManager || Controller && Controller.AdjustmentScene)
                return;

            if (isMultiplayerWeapon || Controller && !Controller.ActiveCharacter)
                return;
            
            DetectObjects();

            if (autoReload && Attacks[currentAttack].curAmmo <= 0 && Attacks[currentAttack].inventoryAmmo > 0 && !isReloadEnabled && !startAutoReloadProcess)
            {
                startAutoReloadProcess = true;
                StartCoroutine(ReloadTimeout());
            }

            CheckButtons();
        }

        void PlayNoAmmoSound()
        {
            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee || Controller.CameraController.cameraPause || isReloadEnabled)
                return;

            if (Attacks[currentAttack].NoAmmoShotAudio)
                GetComponent<AudioSource>().PlayOneShot(Attacks[currentAttack].NoAmmoShotAudio);
        }

        private void ChangeBulletType()
        {
            Attacks[currentAttack].weapon_damage = Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].weapon_damage;
            Attacks[currentAttack].RateOfAttack = Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].RateOfShoot;
            Attacks[currentAttack].bulletsScatterX = Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].bulletsScatterX;
            Attacks[currentAttack].bulletsScatterY = Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].bulletsScatterY;
            currentBulletsScatter = new Vector2(Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].bulletsScatterX, Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].bulletsScatterY);

            Attacks[currentAttack].attackImage = Attacks[currentAttack].BulletsSettings[Attacks[currentAttack].currentBulletType].attackImage;
        }

        public void ChangeAttack()
        {
            if (!isMultiplayerWeapon)
            {
                if (isReloadEnabled /*|| WeaponManager.creategrenade*/ || WeaponManager.enablePickUpTooltip || Controller.isPause || Controller.CameraController.cameraPause)
                    return;

#if USK_MULTIPLAYER
                if(Controller.CharacterSync)
                    Controller.CharacterSync.ChangeWeaponAttack();
#endif
            }

            var newAttack = 0;
            
            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].BulletsSettings[0].Active &&  Attacks[currentAttack].BulletsSettings[1].Active)
            {
                if (Attacks[currentAttack].currentBulletType == 0)
                {
                    Attacks[currentAttack].currentBulletType++;
                    ChangeBulletType();
                    WeaponManager.SetWeaponAnimations(true, Controller.TypeOfCamera);
                    return;
                }
            }
            
            newAttack = currentAttack + 1;
            
            if (newAttack > Attacks.Count - 1) newAttack = 0;

            currentAttack = newAttack;
            
            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].BulletsSettings[0].Active && Attacks[currentAttack].BulletsSettings[1].Active)
            {
                if (Attacks[currentAttack].currentBulletType == 1)
                {
                    Attacks[currentAttack].currentBulletType--;
                    ChangeBulletType();
                }
                ChangeBulletType();
            }
            
//             if (Application.isMobilePlatform || Controller.projectSettings.mobileDebug)
//             {
// //                Controller.UIManager.uiButtons[5].gameObject.SetActive(!Attacks[currentAttack].autoAttack);
//             }

            WeaponManager.SetCrosshair();
            WeaponManager.SetWeaponAnimations(true, Controller.TypeOfCamera);
        }

        void CheckButtons()
        {
            if (Controller.projectSettings.ButtonsActivityStatuses[5] && !Controller.projectSettings.mobileDebug)
            {
                // if (/*!switchToFpCamera || */Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
                {
                    var aimValue = false;
                    switch (projectSettings.aimButtonInteraction)
                    {
                        case ProjectSettings.AimButtonInteraction.Click:

                            if (InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[5], Controller) || InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[5]))
                            {
                                aimIsActivatedByHoldingButton = false;
                                Aim(false, false, false);
                            }

                            break;

                        case ProjectSettings.AimButtonInteraction.Hold:

                            aimValue = InputHelper.IsGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[5], Controller) || InputHelper.IsKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[5]);

                            break;

                        case ProjectSettings.AimButtonInteraction.HoldOnGamepads:

                            aimValue = InputHelper.IsGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[5], Controller);

                            if (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[5]))
                            {
                                aimIsActivatedByHoldingButton = false;
                                Aim(false, false, false);
                            }

                            break;
                    }
                    
                    if (aimValue)
                    {
                        if (!isAimEnabled)
                        {
                            aimIsActivatedByHoldingButton = true;
                            Aim(false, false, true);
                        }
                    }
                    else
                    {
                        if (isAimEnabled && aimIsActivatedByHoldingButton)
                        {
                            Aim(false, false, true);
                        }
                    }

                }
                // else
                // {
                //     if (Controller.projectSettings.ButtonsActivityStatuses[5] && (InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[5], Controller) ||
                //         InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[5])))
                //         //(Input.GetKeyDown(Controller._gamepadCodes[5]) || Helper.CheckGamepadAxisButton(5, Controller._gamepadButtonsAxes, Controller.hasAxisButtonPressed, "GetKeyDown", Controller.projectSettings.AxisButtonValues[5])))
                //     {
                //         aimIsActivatedByHoldingButton = false;
                //         Aim(false, false, false);
                //     }
                // }

                // if (Controller.projectSettings.ButtonsActivityStatuses[5] && InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[5]))//Input.GetKeyDown(Controller._keyboardCodes[5]))
                // {
                //     aimIsActivatedByHoldingButton = false;
                //     Aim(false, false, false);
                // }
            }

            if (Controller.projectSettings.ButtonsActivityStatuses[19] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[19]) ||
                InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[17], Controller)))
                ChangeAttack();

            if (Controller.projectSettings.ButtonsActivityStatuses[3] && (InputHelper.WasKeyboardOrMouseButtonRealised(Controller.projectSettings.keyboardButtonsInUnityInputSystem[3]) || InputHelper.WasGamepadButtonRealised(Controller.projectSettings.gamepadButtonsInUnityInputSystem[3])))
            {
                wasEmptyMagSoundPlayed = false;
            }

            if (!Attacks[currentAttack].autoAttack)
            {
                // if ((Application.isMobilePlatform || projectSettings.mobileDebug) && (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP) && Controller.CameraParameters.lockCamera 
                //     && Attacks[currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee && Attacks[currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                // {
                    // if (Controller.CameraController.useCameraJoystick)
                    // {
                    //     if ((Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 0 ||
                    //          Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets ||
                    //          Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher) && rateOfAttackTimer > Attacks[currentAttack].RateOfAttack)
                    //     {
                    //         Attack(true, "Single");
                    //     }
                    //     else if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 1 ||
                    //              Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                    //     {
                    //         Attack(true, "Auto");
                    //     }
                    // }
                    // else
                    // {
                    //     Attack(false, "Auto");
                    // }
                // }
                // else
                // {
                    if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets ||
                        Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 0
                        || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                    {
                        if (Controller.projectSettings.ButtonsActivityStatuses[3] && (!Controller.projectSettings.mobileDebug && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[3]) ||
                                                                                                                                  InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[3], Controller)) || uiButtonAttack))
                        {
                            // playNoAmmoSound = false;
                            Attack(true, "Single");
                        }
                        else
                        {
                            Attack(false, "Single");
                        }
                    }
                    else
                    {
                        if (Controller.projectSettings.ButtonsActivityStatuses[3] && (!Controller.projectSettings.mobileDebug && (InputHelper.IsKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[3]) ||
                                                                                                                                  InputHelper.IsGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[3], Controller)) || uiButtonAttack))

                        {
                            Attack(true, "Auto");
                        }
                        else
                        {
                            Attack(false, "Auto");
                        }
                    }
                // }
            }
            else
            {
                var startPoint = Controller.thisCamera.transform.position;
                var direction = Controller.thisCamera.transform.forward;

                if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
                {
                    startPoint = Controller.DirectionObject.position;
                    direction = Controller.DirectionObject.forward;
                }

                // if ((Application.isMobilePlatform || projectSettings.mobileDebug) && (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP) && Controller.CameraParameters.lockCamera)
                // {
                //     if (Controller.CameraController.useCameraJoystick)
                //     {
                //         if ((Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 0 ||
                //              Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee ||
                //              Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher) && rateOfAttackTimer > Attacks[currentAttack].RateOfAttack)
                //         {
                //             Attack(true, "Single");
                //         }
                //         else if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 1 ||
                //                  Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                //         {
                //             Attack(true, "Auto");
                //         }
                //     }
                // }
                // else
                // {
                    if (Physics.Raycast(startPoint, direction, out var hit, 10000f, Helper.LayerMask()))
                    {
                        var root = hit.collider.transform.root;

                        var condition = root.gameObject.GetComponent<AIController>() && MultiplayerHelper.CanDamageInMultiplayer(Controller, root.gameObject.GetComponent<AIController>()) && hit.distance <= Attacks[currentAttack].attackDistance &&
                                        (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && isAimEnabled || Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson && (!isAimEnabled && !Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Run") || isAimEnabled));

                        if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
                        {
                            condition = root.gameObject.GetComponent<AIController>() && MultiplayerHelper.CanDamageInMultiplayer(Controller, root.gameObject.GetComponent<AIController>()) && hit.distance <= Attacks[currentAttack].attackDistance &&
                                        (Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || isAimEnabled);
                        }

                        if (condition)
                        {
                            if ((Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 0 ||
                                 Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee ||
                                 Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher) && rateOfAttackTimer > Attacks[currentAttack].RateOfAttack)
                            {
                                Attack(true, "Single");
                            }
                            else if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 1 ||
                                     Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                            {
                                Attack(true, "Auto");
                            }
                        }
                        else
                        {
                            Attack(false, "Single");
                            Attack(false, "Auto");
                        }

                    }
                    else
                    {
                        Attack(false, "Single");
                        Attack(false, "Auto");
                    }
                // }
            }
            
            if (Controller.projectSettings.ButtonsActivityStatuses[4] && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[4])
                || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[4], Controller)))
                Reload();
        }

        private void SwitchAttack(string type)
        {
            switch (type)
            {
                case "Single":
                {
                    switch (Attacks[currentAttack].AttackType)
                    {
                        case WeaponsHelper.TypeOfAttack.Bullets:
                            if (Attacks[currentAttack].currentBulletType == 0)
                                BulletAttack();
                            return;
                        case WeaponsHelper.TypeOfAttack.Rockets:
                        case WeaponsHelper.TypeOfAttack.GrenadeLauncher:
                            RocketAttack();
                            return;
                        case WeaponsHelper.TypeOfAttack.Melee:
                            MeleeAttack();
                            break;
                        case WeaponsHelper.TypeOfAttack.Grenade:
                            var fullBody = Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !isAimEnabled && Controller.anim.GetLayerWeight(2) < 0.1f;
                            ThrowGrenade(fullBody);
                            break;
                    }

                    break;
                }

                case "Auto":
                {
                    switch (Attacks[currentAttack].AttackType)
                    {
                        case WeaponsHelper.TypeOfAttack.Flame:
                            FireAttack();
                            return;
                        case WeaponsHelper.TypeOfAttack.Bullets:
                            if(Attacks[currentAttack].currentBulletType == 1)
                               BulletAttack();
                            return;
                        case WeaponsHelper.TypeOfAttack.Minigun:
                            if(BarrelRotationSpeed >= 20)
                                BulletAttack();
                            break;
                    }

                    break;
                }
            }
        }

        IEnumerator SetAimBeforeAttackTimeout(string type)
        {
            // while (true)
            // {
            //     if (setHandsPositionsAim)
            //     {
                    yield return new WaitForEndOfFrame();

                    SwitchAttack(type);
                    
                    // break;
                // }

                // yield return 0;
            // }
        }

        private void Attack(bool isAttack, string type)
        {
            if (isAttack && !isReloadEnabled && !Controller.isPause && !Controller.CameraController.cameraPause && canAttack && !wasEmptyMagSoundPlayed && Attacks[currentAttack].curAmmo <= 0)
            {
                PlayNoAmmoSound();
                wasEmptyMagSoundPlayed = true;
            }

            if (isAttack && !isReloadEnabled && !Controller.isPause && !Controller.CameraController.cameraPause && canAttack && !WeaponManager.enablePickUpTooltip && Attacks[currentAttack].curAmmo > 0)
            {
                disableAimAfterAttackTimer = 0;

                if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !isAimEnabled && activeAimMode && !DetectObject && aimForAttack)
                {
                    Aim(false, false, false);
                    aimWasSetBeforeAttack = true;
                    StartCoroutine(SetAimBeforeAttackTimeout(type));
                }
                else
                {
                    if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                        BarrelRotationSpeed += 10 * Time.deltaTime;

                    if (BarrelRotationSpeed > 30)
                        BarrelRotationSpeed = 30;

                    SwitchAttack(type);
                }
            }
            else
            {
                switch (type)
                {
                    case "Single":
                    {
                        if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee)
                        {
                            // float rateOfAttackLimit;

                            // rateOfAttackLimit = Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson ? Attacks[currentAttack].[animationIndex].length : Attacks[currentAttack].WeaponAttacks[animationIndex].length;
                            
                            if (rateOfAttackTimer > rateOfAttackLimit && activateMeleeTimer)
                            {
                                MeleeAttackOff();
                            }
                        }

                        else if (Attacks[currentAttack].fpAttacks[0] && rateOfAttackTimer > Attacks[currentAttack].RateOfAttack || rateOfAttackTimer > Attacks[currentAttack].fpAttacks[0].length || isReloadEnabled)
                        {
                            Controller.anim.SetBool("Attack", false);
                        }

                        break;
                    }

                    case "Auto":
                    {
                        if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame)
                            FireAttackOff();

                        Controller.anim.SetBool("Attack", false);
                        
                        BarrelRotationSpeed -= 10 * Time.deltaTime;

                        break;
                    }
                }

                if (BarrelRotationSpeed < 0)
                    BarrelRotationSpeed = 0;

            }
        }

        public void MeleeAttackOff()
        {
            if (Attacks[currentAttack].AttackCollider)
                Attacks[currentAttack].AttackCollider.enabled = false;
            
            activateMeleeTimer = false;
            Controller.anim.SetBool("Attack", false);
            Controller.anim.SetBool("Pause", false);
            
            // Controller.anim.SetBool("MeleeAttack", false);
            
#if USK_MULTIPLAYER
            if (!isMultiplayerWeapon && Controller.CharacterSync)
                Controller.CharacterSync.MeleeAttack(false, 0, 0);
#endif
        }

        #region BulletAttack

        public void BulletAttack()
        {
            if (rateOfAttackTimer > Attacks[currentAttack].RateOfAttack || isMultiplayerWeapon)
            {
                if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets && Attacks[currentAttack].currentBulletType == 0)
                    uiButtonAttack = false;

                rateOfAttackTimer = 0;

                if (Attacks[currentAttack].AttackAudio)
                    GetComponent<AudioSource>().PlayOneShot(Attacks[currentAttack].AttackAudio);
                else Debug.LogWarning("(Weapon) <color=yellow>Missing component</color> [Attack Audio].", gameObject);

                Controller.anim.SetBool("Attack", true);
                Controller.anim.CrossFade("Attack", 0, 1);

                if (!isMultiplayerWeapon)
                {
#if USK_MULTIPLAYER
                    if(Controller.CharacterSync)
                        Controller.CharacterSync.BulletsShooting();
#endif    
                }
                
                if (Attacks[currentAttack].AttackSpawnPoint)
                {
                    Attacks[currentAttack].curAmmo -= 1;

                    if (Attacks[currentAttack].MuzzleFlash)
                    {
                        var flash = Instantiate(Attacks[currentAttack].MuzzleFlash, Attacks[currentAttack].AttackSpawnPoint.position, Attacks[currentAttack].AttackSpawnPoint.rotation);
                        flash.transform.parent = gameObject.transform;
                        flash.AddComponent<DestroyObject>().destroyTime = 1;
                        Helper.ChangeLayersRecursively(flash.transform, "Character");
                    }

                    if (Attacks[currentAttack].Shell && Attacks[currentAttack].ShellPoint && Attacks[currentAttack].spawnShellsImmediately)
                    {
                        var _shell = Instantiate(Attacks[currentAttack].Shell, Attacks[currentAttack].ShellPoint.position, Attacks[currentAttack].ShellPoint.localRotation);
                        Helper.ChangeLayersRecursively(_shell.transform, "Character");
                        _shell.hideFlags = HideFlags.HideInHierarchy;
                        _shell.gameObject.AddComponent<ShellControll>().ShellPoint = Attacks[currentAttack].ShellPoint;
                    }

                    if (WeaponsHelper.UpdateAttackDirection(this, ref hit, out _))
                    {
                        if (Attacks[currentAttack].shootingMethod == WeaponsHelper.ShootingMethod.Raycast)
                        {
                            if (Attacks[currentAttack].bulletTrail)
                            {
                                WeaponsHelper.CreateTrail(Attacks[currentAttack].AttackSpawnPoint.position, hit.point, Attacks[currentAttack].bulletTrail);
                            }

                            WeaponsHelper.CheckBulletRaycast(hit, this);
                        }
                        else
                        {
                            WeaponsHelper.InstantiateBullet(this,Attacks[currentAttack].AttackSpawnPoint.position, hit.point);
                        }
                    }

                    var shotgunPoints = new List<RaycastHit>();

                    if (isShotgun)
                    {
                        for (int i = 0; i < Random.Range(4, 7); i++)
                        {
                            var hit = new RaycastHit();

                            if (WeaponsHelper.UpdateAttackDirection(this, ref hit, out var isHit))
                            {
                                if(!isHit)
                                    hit.point += new Vector3(Random.Range(-currentBulletsScatter.x, currentBulletsScatter.x) * 100, Random.Range(-currentBulletsScatter.y, currentBulletsScatter.y) * 100);
                                
                                shotgunPoints.Add(hit);

                                if (Attacks[currentAttack].shootingMethod == WeaponsHelper.ShootingMethod.Raycast)
                                {
                                    if (Attacks[currentAttack].bulletTrail)
                                        WeaponsHelper.CreateTrail(Attacks[currentAttack].AttackSpawnPoint.position, shotgunPoints[shotgunPoints.Count - 1].point, Attacks[currentAttack].bulletTrail);

                                    WeaponsHelper.CheckBulletRaycast(hit, this);
                                }
                                else
                                {
                                    WeaponsHelper.InstantiateBullet(this,Attacks[currentAttack].AttackSpawnPoint.position, shotgunPoints[shotgunPoints.Count - 1].point);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("(Weapon) <color=red>Missing component</color> [AttackSpawnPoint]", gameObject);
                }
            }
        }

        #endregion

        #region RocketsAttack

        public void RocketAttack()
        {
            if ((rateOfAttackTimer > Attacks[currentAttack].RateOfAttack || isMultiplayerWeapon) && !isReloadEnabled)
            {
                uiButtonAttack = false;
                
                Controller.anim.SetBool("Attack", true);
                Controller.anim.CrossFade("Attack", 0, 1);

                if (Attacks[currentAttack].AttackAudio)
                    GetComponent<AudioSource>().PlayOneShot(Attacks[currentAttack].AttackAudio);
                else Debug.LogWarning("(Weapon) <color=yellow>Missing component</color> [AttackAudio]. Add it, otherwise the sound of shooting won't be played.", gameObject);

                GameObject rocket = null;

                if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets)
                {
                    if (Attacks[currentAttack].TempMagazine[0])
                    {
                        rocket = Attacks[currentAttack].TempMagazine[0];
                        rocket.transform.parent = null;
                    }
                    else if (Attacks[currentAttack].AttackSpawnPoint && Attacks[currentAttack].magazine)
                    {
                        rocket = Instantiate(Attacks[currentAttack].magazine, Attacks[currentAttack].AttackSpawnPoint.position, Attacks[currentAttack].AttackSpawnPoint.rotation);
                        rocket.SetActive(true);
                    }
                    
                    var rocketScript = rocket.AddComponent<FlyingProjectile>();
                    rocketScript.startPosition = Controller.transform.position;
                    rocketScript.isMultiplayerWeapon = isMultiplayerWeapon;
                    rocketScript.isRocket = true;
                    rocketScript.ApplyForce = true;
                    if(weaponImage) rocketScript.WeaponImage = weaponImage;
                    // rocketScript.Particles = Attacks[currentAttack].attackEffects.ToArray();

                    if (WeaponsHelper.UpdateAttackDirection(this, ref hit, out _))
                    {
#if USK_MULTIPLAYER
                        if (!isMultiplayerWeapon && Controller.CharacterSync)
                            Controller.CharacterSync.LaunchRocket();
#endif

                        var direction = hit.point - Attacks[currentAttack].AttackSpawnPoint.position;
                        rocketScript.TargetPoint = hit.point + direction * 10;
                    }

                    rocketScript.directionObject = Controller.thisCamera.transform;
                    // rocketScript.isRaycast = rocketScript.TargetPoint != Vector3.zero;
                    rocketScript.Speed = Attacks[currentAttack].flightSpeed;
                    rocketScript.explosion = Attacks[currentAttack].Explosion;
                    rocketScript.damage = Attacks[currentAttack].weapon_damage;

                    Attacks[currentAttack].TempMagazine.Clear();

                    if (Attacks[currentAttack].attackEffects.Count > 0)
                    {
                        foreach (var effect in Attacks[currentAttack].attackEffects)
                        {
                            if (effect && !effect.emission.enabled)
                            {
                                var effectEmission = effect.emission;
                                effectEmission.enabled = true;
                            }
                        }
                    }

                    if (Controller.multiplayerNickname != null)
                        rocketScript.characterOwner = Controller;

                }
                else
                {
                    if (Attacks[currentAttack].AttackSpawnPoint && Attacks[currentAttack].magazine)
                    {
                        rocket = Instantiate(Attacks[currentAttack].magazine, Attacks[currentAttack].AttackSpawnPoint.position, Attacks[currentAttack].AttackSpawnPoint.rotation);
                        rocket.SetActive(true);
                    }
                    
#if USK_MULTIPLAYER
                    if (!isMultiplayerWeapon && Controller.CharacterSync)
                        Controller.CharacterSync.LaunchRocket();
#endif

                    var grenadeScript = rocket.AddComponent<FlyingProjectile>();
                    grenadeScript.startPosition = Controller.transform.position;
                    grenadeScript.isMultiplayerWeapon = isMultiplayerWeapon;
                    grenadeScript.ApplyForce = true;
                    grenadeScript.enabled = true;
                    if(weaponImage) grenadeScript.WeaponImage = weaponImage;
                    grenadeScript.ExplodeWhenTouchGround = Attacks[currentAttack].ExplodeWhenTouchGround;
                    grenadeScript.Speed = Attacks[currentAttack].flightSpeed;
                    grenadeScript.GrenadeExplosionTime = Attacks[currentAttack].GrenadeExplosionTime;
                    grenadeScript.damage = Attacks[currentAttack].weapon_damage;
                    // grenadeScript.ownerID = Controller.gameObject.GetInstanceID();

                    if (Controller.multiplayerNickname != null)
                        grenadeScript.characterOwner = Controller;

                    if (Attacks[currentAttack].Explosion)
                        grenadeScript.explosion = Attacks[currentAttack].Explosion;

                    if (!rocket.GetComponent<BoxCollider>() && !rocket.GetComponent<SphereCollider>() && !rocket.GetComponent<MeshCollider>())
                        rocket.AddComponent<SphereCollider>();

                    grenadeScript.isGrenade = true;

                    if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
                    {
                        if (!Controller.CameraParameters.lockCamera)
                        {
                            grenadeScript.m_CurrentVelocity = Controller.thisCamera.transform.TransformDirection(Vector3.up * Attacks[currentAttack].flightSpeed);
                        }
                        else
                        {
                            if (!Controller.CameraParameters.lookAtCursor)
                            {
//                                var dir = Controller.CameraController.BodyLookAt.position - Attacks[currentAttack].AttackSpawnPoint.position;
//                                var speed = Vector3.Distance(new Vector3(Controller.CameraController.BodyLookAt.position.x, 0, Controller.CameraController.BodyLookAt.position.z), new Vector3(Attacks[currentAttack].AttackSpawnPoint.position.x, 0, Attacks[currentAttack].AttackSpawnPoint.position.z));
//                                dir.y = 0;
//                                Direction = dir.normalized;
//                                grenadeScript.m_CurrentVelocity = 1.7f * speed * Direction;

                                var tempPos = Controller.CameraController.BodyLookAt.position;
                                tempPos.y = Controller.transform.position.y;

                                RaycastHit hit;
                                if (Physics.Raycast(Controller.CameraController.BodyLookAt.position, Vector3.down, out hit))
                                {
                                    tempPos = hit.point;
                                }

                                grenadeScript.m_CurrentVelocity = ProjectileHelper.ComputeVelocityToHitTargetAtTime(Attacks[currentAttack].AttackSpawnPoint.position, tempPos, Physics.gravity.y, 10 / Attacks[currentAttack].flightSpeed);
                            }
                            else
                            {
                                grenadeScript.m_CurrentVelocity = ProjectileHelper.ComputeVelocityToHitTargetAtTime(Attacks[currentAttack].AttackSpawnPoint.position, Controller.CameraController.BodyLookAt.position, Physics.gravity.y, 10 / Attacks[currentAttack].flightSpeed);
                            }
                        }
                    }
                    else
                        grenadeScript.m_CurrentVelocity = Controller.thisCamera.transform.TransformDirection(Vector3.forward * Attacks[currentAttack].flightSpeed);

                    var rigidBody = rocket.AddComponent<Rigidbody>();
                    rigidBody.useGravity = false;
                    rigidBody.isKinematic = true;

                    if (!grenadeScript.ExplodeWhenTouchGround)
                        grenadeScript.StartCoroutine("GrenadeFlying");
                }

                Attacks[currentAttack].curAmmo -= 1;
                rateOfAttackTimer = 0;
            }
        }

        #endregion

        #region FireAttack

        public void FireAttack()
        {
            if (!isReloadEnabled)
            {
                Controller.anim.SetBool("Attack", true);
                
                if (!attackAudioPlay)
                {
                    Controller.anim.CrossFade("Attack", 0, 1, Time.deltaTime, 10);
                    
                    if (Attacks[currentAttack].AttackAudio)
                    {
                        GetComponent<AudioSource>().loop = true;
                        GetComponent<AudioSource>().clip = Attacks[currentAttack].AttackAudio;
                        GetComponent<AudioSource>().Play();
                        attackAudioPlay = true;
                    }
                    else
                    {
                        Debug.LogWarning("(Weapon) <color=yellow>Missing component</color> [AttackAudio].", gameObject);
                    }
                }

                if (Attacks[currentAttack].AttackSpawnPoint && Attacks[currentAttack].attackEffects.Count > 0)
                {
                    Attacks[currentAttack].curAmmo -= 1 * Time.deltaTime;

                    if (!isMultiplayerWeapon)
                    {
#if USK_MULTIPLAYER
                        if (Controller.CharacterSync)
                            Controller.CharacterSync.FireAttack(true);
#endif
                    }

                    foreach (var effect in Attacks[currentAttack].attackEffects)
                    {
                        if (effect)
                        {
                            var _effect = Instantiate(effect, Attacks[currentAttack].AttackSpawnPoint.position, Attacks[currentAttack].AttackSpawnPoint.rotation);
                            _effect.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }

                }
                else
                {
                    Debug.LogError("(Weapon) <color=red>Missing components</color>: [AttackSpawnPoint] and/or [Fire_prefab]. Add it, otherwise the flamethrower won't attack.", gameObject);
                }

                if (Attacks[currentAttack].AttackCollider)
                    Attacks[currentAttack].AttackCollider.enabled = true;
                else
                {
                    Debug.LogError("(Weapon) <color=red>Missing components</color>: [FireCollider]. Add it, otherwise the flamethrower won't attack.", gameObject);
                    Debug.Break();
                }
            }
        }

        public void FireAttackOff()
        {
            if (Attacks[currentAttack].AttackAudio)
                if (attackAudioPlay)
                {
                    attackAudioPlay = false;
                    GetComponent<AudioSource>().Stop();
                }

            if (Attacks[currentAttack].AttackCollider)
                Attacks[currentAttack].AttackCollider.enabled = false;
            
#if USK_MULTIPLAYER
            if (Controller.CharacterSync)
                Controller.CharacterSync.FireAttack(false);
#endif
        }

        #endregion

        public void ThrowGrenade(bool fullBody)
        {
            if (rateOfAttackTimer > Attacks[currentAttack].RateOfAttack || isMultiplayerWeapon)
            {
#if USK_MULTIPLAYER
                if(!isMultiplayerWeapon && Controller.CharacterSync)
                    Controller.CharacterSync.ThrowGrenade(fullBody);
#endif

                if (!fullBody)
                {
                    uiButtonAttack = false;
                    
                    rateOfAttackTimer = 0;
                    
                    Controller.anim.SetBool("Attack", true);
                    
                    Controller.anim.CrossFade("Attack", 0, 1);
                    Controller.anim.CrossFade("Attack", 0, 2);

                    StartCoroutine(FlyGrenade(false));
                    WeaponManager.StartCoroutine(WeaponManager.TakeGrenade(false));
                }
                else
                {
                    rateOfAttackTimer = 0;
                    
                    Controller.anim.SetBool("Attack", true);
                    Controller.anim.CrossFade("Grenade_Throw", 0, 0);

                    StartCoroutine(FlyGrenade(true));
                    WeaponManager.StartCoroutine(WeaponManager.TakeGrenade(true));
                    
                    // Controller.anim.SetBool("Pause", true);
                    // Controller.anim.SetBool("LaunchGrenade", true);
                    // StartCoroutine("FullBodyGrenadeLaunch");
                }
            }
        }

        // IEnumerator FullBodyGrenadeLaunch()
        // {
        //     while (true)
        //     {
        //         if (Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Grenade_Throw"))
        //         {
        //             StartCoroutine("FlyGrenade");
        //             WeaponManager.StartCoroutine(WeaponManager.TakeGrenade());
        //             break;
        //         }
        //         yield return 0;
        //     }
        //
        // }

        IEnumerator FlyGrenade(bool fullBody)
        {
            yield return new WaitForSeconds(!fullBody ? Attacks[currentAttack].fpAttacks[0].length : Attacks[currentAttack].tpAttacks[0].length);
            LaunchGrenade();
        }

        public void LaunchGrenade()
        {
            var tempGrenade = Instantiate(gameObject, transform.localPosition, transform.localRotation, transform.parent);

            Destroy(tempGrenade.GetComponent<WeaponController>());
            Destroy(tempGrenade.GetComponent<LineRenderer>());

            var grenadeScript = tempGrenade.AddComponent<FlyingProjectile>();
            grenadeScript.startPosition = Controller.transform.position;
            grenadeScript.isMultiplayerWeapon = isMultiplayerWeapon;
            grenadeScript.isGrenade = true;
            if (weaponImage) grenadeScript.WeaponImage = weaponImage;
            grenadeScript.FlashExplosion = Attacks[currentAttack].flashExplosion;
            grenadeScript.ApplyForce = Attacks[currentAttack].applyForce;
            grenadeScript.stickOnObject = Attacks[currentAttack].StickToObject;
            grenadeScript.applyGravity = Attacks[currentAttack].applyGravity;

            if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
            {
                if (!Controller.CameraParameters.lockCamera)
                {
                    grenadeScript.m_CurrentVelocity = Controller.thisCamera.transform.TransformDirection(Vector3.up * Attacks[currentAttack].flightSpeed);
                }
                else
                {
                    var gravity =  Attacks[currentAttack].applyGravity ? Physics.gravity.y : 0;

                    if (Controller.CameraParameters.lookAtCursor)
                    {
                        grenadeScript.m_CurrentVelocity = ProjectileHelper.ComputeVelocityToHitTargetAtTime(transform.position, Controller.CameraController.BodyLookAt.position, gravity, 10 / Attacks[currentAttack].flightSpeed);
                    }
                    else
                    {
                        var tempPos = Controller.CameraController.BodyLookAt.position;
                        tempPos.y = Controller.transform.position.y;

                        RaycastHit hit;
                        if (Physics.Raycast(Controller.CameraController.BodyLookAt.position, Vector3.down, out hit))
                            tempPos = hit.point;

                        grenadeScript.m_CurrentVelocity = ProjectileHelper.ComputeVelocityToHitTargetAtTime(transform.position, tempPos, gravity, 10 / Attacks[currentAttack].flightSpeed);
                    }
                }
            }
            else
            {
                if(Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.movementType == CharacterHelper.MovementType.Standard && !isAimEnabled)
                    grenadeScript.m_CurrentVelocity = Controller.transform.TransformDirection(Vector3.forward * Attacks[currentAttack].flightSpeed);
                else
                    grenadeScript.m_CurrentVelocity = Controller.thisCamera.transform.TransformDirection(Vector3.forward * Attacks[currentAttack].flightSpeed);
            }

            grenadeScript.enabled = true;
            grenadeScript.ExplodeWhenTouchGround = Attacks[currentAttack].ExplodeWhenTouchGround;
            grenadeScript.Speed = Attacks[currentAttack].flightSpeed;
            grenadeScript.GrenadeExplosionTime = Attacks[currentAttack].GrenadeExplosionTime;
            grenadeScript.damage = Attacks[currentAttack].weapon_damage;

            if (Controller.multiplayerNickname != null)
                grenadeScript.characterOwner = Controller;

            if (Attacks[currentAttack].Explosion)
                grenadeScript.explosion = Attacks[currentAttack].Explosion;

            tempGrenade.SetActive(true);
            tempGrenade.transform.parent = null;

            if (Attacks[currentAttack].AttackAudio)
                tempGrenade.GetComponent<AudioSource>().PlayOneShot(Attacks[currentAttack].AttackAudio);

            if (!grenadeScript.ExplodeWhenTouchGround)
                grenadeScript.StartCoroutine("GrenadeFlying");

            Attacks[currentAttack].curAmmo -= 1;

            gameObject.SetActive(false);

            canDrawGrenadesPath = false;
        }

        #region MeleeAttack

        public void MeleeAttack()
        {
            if (rateOfAttackTimer > Attacks[currentAttack].RateOfAttack || isMultiplayerWeapon)
            {

                AnimationClip animationClip = null;
                
                if (!isMultiplayerWeapon)
                {
                    if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.anim.GetLayerWeight(2) < 0.1f)
                    {
                        animationIndex = WeaponsHelper.GetRandomIndex(Attacks[currentAttack].tpAttacks, ref lastAttackAnimationIndex);
                        
                        crouchAnimationIndex = WeaponsHelper.GetRandomIndex(Attacks[currentAttack].tpCrouchAttacks, ref lastCrouchAttackAnimationIndex);
                    }
                    else
                    {
                        animationIndex = Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson ?
                            WeaponsHelper.GetRandomIndex(Attacks[currentAttack].fpAttacks, ref lastAttackAnimationIndex) : 
                            WeaponsHelper.GetRandomIndex(Attacks[currentAttack].tdAttacks, ref lastAttackAnimationIndex);
                    }
                    
#if USK_MULTIPLAYER
                    if(Controller.CharacterSync)
                        Controller.CharacterSync.MeleeAttack(true, animationIndex, crouchAnimationIndex);
#endif
                }

                if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.anim.GetLayerWeight(2) < 0.1f)
                {
                    if (Attacks[currentAttack].tpAttacks[animationIndex])
                        Controller.ClipOverrides["_FullbodyMeleeAttack"] = Attacks[currentAttack].tpAttacks[animationIndex];

                    if (Attacks[currentAttack].tpCrouchAttacks[crouchAnimationIndex])
                        Controller.ClipOverrides["_FullbodyCrouchMeleeAttack"] = Attacks[currentAttack].tpCrouchAttacks[crouchAnimationIndex];
                    
                    animationClip = Controller.isCrouch ? Attacks[currentAttack].tpCrouchAttacks[crouchAnimationIndex] : Attacks[currentAttack].tpAttacks[animationIndex];

                    rateOfAttackLimit = (Controller.isCrouch ? Attacks[currentAttack].tpCrouchAttacks[crouchAnimationIndex].length / 1.7f : Attacks[currentAttack].tpAttacks[animationIndex].length / 1.7f);
                }
                else
                {
                    if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
                    {
                        if (Attacks[currentAttack].fpAttacks[animationIndex])
                            Controller.ClipOverrides["_WeaponAttack"] = Attacks[currentAttack].fpAttacks[animationIndex];

                        animationClip = Attacks[currentAttack].fpAttacks[animationIndex];
                    }
                    else
                    {
                        if (Attacks[currentAttack].tdAttacks[animationIndex])
                            Controller.ClipOverrides["_WeaponAttack"] = Attacks[currentAttack].tdAttacks[animationIndex];

                        animationClip = Attacks[currentAttack].tdAttacks[animationIndex];
                    }

                    rateOfAttackLimit = animationClip.length;
                }

                Controller.newController.ApplyOverrides(Controller.ClipOverrides);
                
                if(Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
                    Controller.anim.CrossFade("Attack", 0, 1);

                if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.anim.GetLayerWeight(2) < 0.1f)
                {
                    var duration = 0.3f;
                    
                    if (Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Main Idle") || Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Idle"))
                        duration = 0.02f;
                    else if (!Controller.anim.GetBool("Move"))
                        duration = 0;
                    
                    Controller.anim.CrossFade(!Controller.isCrouch ? "Melee" : "Crouch Melee", duration, 0);
                }
                
                Controller.anim.SetBool("Attack", true);

                if (!isMultiplayerWeapon)
                {
                    uiButtonAttack = false;

                    if (!WeaponsHelper.HasAnimationCollidersEvent(animationClip))
                    {
                        if (Attacks[currentAttack].AttackCollider)
                            Attacks[currentAttack].AttackCollider.enabled = true;

#if USK_DESTROYIT_INTEGRATION
                        if (Attacks[currentAttack].weapon_damage >= 0)
                        {
                            WeaponsHelper.DestroyItMeleeDamage(transform, 2, Attacks[currentAttack].weapon_damage);
                        }
                        else
                        {
                            WeaponsHelper.DestroyItOnRepair(transform.position, 2, Attacks[currentAttack].weapon_damage * -1);
                        }
#endif
                    }

                    rateOfAttackTimer = 0;
                    
                    activateMeleeTimer = true;
                }
            }
        }

        #endregion

        private void CancelReload()
        {
            canceledReload = true;
                
            isReloadEnabled = false;
            reloadProcessHasBeenStarted = false;
            attackAudioPlay = false;
            startAutoReloadProcess = false;
            
            Controller.anim.SetBool("CanWalkWithWeapon", false);
            Controller.anim.SetBool("Reload", false);

            Controller.inventoryManager.ChangeParent("rightAndPlace");

            GetComponent<AudioSource>().Stop();

            StartCoroutine(WalkWithWeaponTimeout());
                
            StopCoroutine(DisableAnimation());
           
            if(reloadRoutine != null)
                StopCoroutine(reloadRoutine);
            
#if USK_MULTIPLAYER
            if(!isMultiplayerWeapon && Controller.CharacterSync)
                Controller.CharacterSync.ReloadWeapon(false);
#endif
            
            StopCoroutine(ReloadTimeout());
        }
        
        public void Reload()
        {
            if (reloadProcessHasBeenStarted && !startAutoReloadProcess)
            {
                CancelReload();
                return;
            }
            if(Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade || !isMultiplayerWeapon && (Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") || 
                                                                                                                   Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Attack")) || isReloadEnabled || reloadProcessHasBeenStarted || 
               Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.isCrouch && !Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Idle") && !Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Aim_Idle"))
                return;
            
            var pause = false;

            if (!isMultiplayerWeapon)
                pause = Controller.isPause && Controller.CameraController.cameraPause;

            if (Attacks[currentAttack].inventoryAmmo > 0 && Attacks[currentAttack].curAmmo < Attacks[currentAttack].maxAmmo && !pause && !DetectObject || isMultiplayerWeapon)
            {
                reloadProcessHasBeenStarted = true;

                if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson && isAimEnabled)
                { 
                    Aim(false, true, false);
                    StartCoroutine(ReloadTimeout());
                }
                else
                {
                    isReloadEnabled = true;

                    Controller.anim.SetBool("Reload", true);
                    
#if USK_MULTIPLAYER
                    if(!isMultiplayerWeapon && Controller.CharacterSync)
                        Controller.CharacterSync.ReloadWeapon(true);
#endif
                    
                    PlayReloadAudio();
                    
                    if(reloadRoutine != null)
                        StopCoroutine(reloadRoutine);
                    
                    reloadRoutine = StartCoroutine(ReloadProcess());
                    
                    StopCoroutine(DisableAnimation());
                    StartCoroutine(DisableAnimation());
                    
                }
            }
        }
        
        void PlayReloadAudio()
        {
            if (Attacks[currentAttack].ReloadAudio)
                GetComponent<AudioSource>().PlayOneShot(Attacks[currentAttack].ReloadAudio);
            else
            {
                Debug.LogWarning("(Weapon) <color=yellow>Missing component</color> [ReloadAudio]. Add it, otherwise the sound of reloading won't be played.", gameObject);
            }

            StopCoroutine("PlayReloadAudio");
        }

        public void CrouchHands()
        {
            if(Controller.AdjustmentScene || !setHandsPositionsAim || Controller.isAlwaysTpAimEnabled || !setHandsPositionsObjectDetection || Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
                return;

            if (/*setHandsPositionsCrouch &&*/ CanUseCrouchIK && !isAimEnabled && !DetectObject)
            {
                if (!ActiveCrouchHands)
                {
                    setHandsPositionsCrouch = false;
                    ActiveCrouchHands = true;
                    
                    IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightHandPosition, this);
                }
                else if (ActiveCrouchHands)
                {
                    if (!CurrentWeaponInfo[settingsSlotIndex].disableIkInNormalState)
                    {
                        setHandsPositionsCrouch = false;
                        
                        IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftCrouchHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightCrouchHandPosition, this);
                    }

                    ActiveCrouchHands = false;
                }
            }
        }

        public void Aim(bool instantly, bool notSendToMultiplayer, bool gamepadInput)
        {
            if (!isMultiplayerWeapon)
            {
                if (Controller.isAlwaysTpAimEnabled || !activeAimMode || Controller.CameraController.cameraOcclusion ||
                    isReloadEnabled ||  //Controller.changeCameraTypeTimeout < 0.5f ||
                    Controller.anim.GetBool("Pause") || /*!aimTimeout ||*/ !Controller.anim.GetBool("HasWeaponTaken") || 
                    Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") || Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Take Weapon") ||
                    isShotgun && (Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") || Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Attack")))
                    return;
            }

            if (isAimEnabled)
                aimIsActivatedByHoldingButton = false;

            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Attacks[currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee || Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson)
            {
                if (CanUseAimIK)
                {
                    if (!WeaponsHelper.CanAim(isMultiplayerWeapon, Controller)) return;

                    if (!Controller.anim.GetBool("OnFloor"))
                        aimWhileJumping = true;

                    if (!isAimEnabled)
                    {
                        if (!isMultiplayerWeapon)
                        {
                            Controller.CameraController.Aim();

                            currentBulletsScatter /= 3;// Attacks[currentAttack].ScatterOfBullets / 2;

#if USK_MULTIPLAYER
                            if(Controller.CharacterSync)
                                Controller.CharacterSync.Aim();
#endif
                            
                            aimTimeout = false;
                        }

                        isAimEnabled = true;
                        
                        if (switchToFpCamera && Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !Controller.emulateTDModeLikeTP && !instantly && !isMultiplayerWeapon)
                        {
                            Controller.CameraController.DeepAim();
                        }

                        WeaponsHelper.ReplaceRunAnimation(Controller, weaponAnimations.fpWalk, 1.5f, Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets,0);

                        if (!DetectObject && setHandsPositionsCrouch && setHandsPositionsObjectDetection)
                        {
                            setHandsPositionsAim = false;
                            
                            if(Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                                setCrouchHandsFirstly = false;
                            
                            IkObjects.LeftObject.parent = IkObjects.RightObject;

                            Controller.anim.SetBool("CanWalkWithWeapon", false);

                            if (!Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") && !Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Take Weapon"))
                            {
                                Controller.anim.CrossFade("Idle", 0, 1);
                                Controller.anim.CrossFade("Idle", 0, 2);
                            }

                            if (Controller.isCrouch && Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                            {
                                IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftCrouchHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightCrouchHandPosition, this);
                            }
                            else
                            {
                                IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightHandPosition, this);
                            }
                        }
                    }
                    else if (isAimEnabled) //&& setHandsPositionsAim)
                    {
                        if (!isMultiplayerWeapon)
                        {
                            Controller.CameraController.Aim();
                            currentBulletsScatter *= 3;
                            aimTimeout = false;
                            
#if USK_MULTIPLAYER
                            if (!notSendToMultiplayer && Controller.CharacterSync)
                                Controller.CharacterSync.Aim();
#endif
                        }

                        isAimEnabled = false;
                        
                        WeaponsHelper.ReplaceRunAnimation(Controller, weaponAnimations.fpRun, 1,true, 1);

                        if (!DetectObject && setHandsPositionsCrouch && setHandsPositionsObjectDetection)
                        {
                            aimWasSetBeforeAttack = false;
                            setHandsPositionsAim = false;
                            
                            if(Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                                setCrouchHandsFirstly = false;
                            
                            IkObjects.LeftObject.parent = IkObjects.RightObject;

                            Controller.anim.SetBool("CanWalkWithWeapon", false);

                            if (!Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") && !Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Take Weapon"))
                            {
                                Controller.anim.CrossFade("Idle", 0, 1);
                                Controller.anim.CrossFade("Idle", 0, 2);
                            }

                            IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftAimPosition, CurrentWeaponInfo[settingsSlotIndex].RightAimPosition, this);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("You don't set positions of character's hands for aiming. Open the Adjustment Scene [Tools -> USK -> Adjust] to do that.", gameObject);
                }
            }
            else
            {
                if (!WeaponsHelper.CanAim(isMultiplayerWeapon, Controller)) return;
                
                Controller.CameraController.Aim();
            }
        }
        
        void DetectObjects()
        {
            if (!CanUseWallIK || !Controller.anim.GetBool("HasWeaponTaken") ||
                Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") || Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Take Weapon")
                || !enableObjectDetectionMode || Controller.AdjustmentScene || 
                Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !setHandsPositionsCrouch || !setHandsPositionsAim || //!setHandsPositionsObjectDetection || 
                isShotgun && Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
                return;
            
            var mask = ~ (LayerMask.GetMask("Character") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Head") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));

            var size = Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade || Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee ? 5 : 10;
            
            var hitColliders = Physics.OverlapBox(Controller.ColliderToObjectsDetection.position, Vector3.one / size, Controller.ColliderToObjectsDetection.rotation, mask);
            
            if (DetectObject)
            {
                var nearObject = false;

                foreach (var col in hitColliders)
                {
                    if (!col) continue;
                    
                    if (!col.transform.root.GetComponent<Controller>()) //&& !col.transform.root.CompareTag("Smoke"))
                        nearObject = true;
                }

                if (nearObject) return;
                
                DetectObject = false;
                setHandsPositionsObjectDetection = false;
                applyChanges = false;
                Controller.CameraController.layerCamera.gameObject.SetActive(false);
                
                Controller.anim.SetBool("CanWalkWithWeapon", false);
                
                if (!Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") &&
                    !Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Take Weapon"))
                {
                    Controller.anim.CrossFade("Idle", 0, 1);
                    Controller.anim.CrossFade("Idle", 0, 2);
                }
                
                IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftHandWallPosition, CurrentWeaponInfo[settingsSlotIndex].RightHandWallPosition, this);
            }
            else
            {
                if (Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Run") || Controller.anim.GetCurrentAnimatorStateInfo(2).IsName("Run")) return;
                
                if (hitColliders.Any(collider => !collider.transform.root.GetComponent<Controller>())) //&& !collider.transform.root.CompareTag("Smoke")))
                {
                    if (!isMultiplayerWeapon)
                        Controller.CameraController.layerCamera.gameObject.SetActive(true);

                    DetectObject = true;
                    applyChanges = false;
                    setHandsPositionsObjectDetection = false;

                    if (isAimEnabled || Controller.isAlwaysTpAimEnabled)
                    {
                        IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftAimPosition, CurrentWeaponInfo[settingsSlotIndex].RightAimPosition, this);
                    }
                    else
                    {
                        if (!Controller.isCrouch || Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
                        {
                            IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightHandPosition, this);
                        }
                        else
                        {
                            IKHelper.ConvertIKPosition(CurrentWeaponInfo[settingsSlotIndex].LeftCrouchHandPosition, CurrentWeaponInfo[settingsSlotIndex].RightCrouchHandPosition, this);
                        }
                    }
                }
            }
        }

        IEnumerator DisableAnimation()
        {
            yield return new WaitForSeconds(Attacks[currentAttack].reloadAnimation.length);

            Controller.anim.SetBool("CanWalkWithWeapon", false);
            Controller.anim.SetBool("Reload", false);

            StartCoroutine(WalkWithWeaponTimeout());
        }

        IEnumerator ReloadTimeout()
        {
            while (true)
            {
                if (setHandsPositionsAim && Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
                {
                    // yield return new WaitForSeconds(!autoReload ? 0.1f : 0.5f);
                    
                    isReloadEnabled = true;
                    Controller.anim.SetBool("Reload", true);

#if USK_MULTIPLAYER
                    if (!isMultiplayerWeapon && Controller.CharacterSync)
                        Controller.CharacterSync.ReloadWeapon(true);
#endif

                    PlayReloadAudio();
                    
                    if(reloadRoutine != null)
                        StopCoroutine(reloadRoutine);
                    
                    reloadRoutine = StartCoroutine(ReloadProcess());
                    
                    StopCoroutine(DisableAnimation());
                    StartCoroutine(DisableAnimation());
                    break;
                }

                yield return 0;
            }
        }

        IEnumerator ReloadProcess()
        {
            yield return new WaitForSeconds(Attacks[currentAttack].reloadAnimation.length - 0.3f);
            
            isReloadEnabled = false;
            reloadProcessHasBeenStarted = false;

            if (Attacks[currentAttack].inventoryAmmo < Attacks[currentAttack].maxAmmo - Attacks[currentAttack].curAmmo)
            {
                Attacks[currentAttack].curAmmo += Attacks[currentAttack].inventoryAmmo;
                Attacks[currentAttack].inventoryAmmo = 0;
            }
            else
            {
                Attacks[currentAttack].inventoryAmmo -= Attacks[currentAttack].maxAmmo - Attacks[currentAttack].curAmmo;
                Attacks[currentAttack].curAmmo += Attacks[currentAttack].maxAmmo - Attacks[currentAttack].curAmmo;
            }

            attackAudioPlay = false;
            startAutoReloadProcess = false;
            
            if(reloadRoutine != null)
                StopCoroutine(reloadRoutine);
        }

        public IEnumerator WalkWithWeaponTimeout()
        {
            yield return new WaitForSeconds(0.5f);
            Controller.anim.SetBool("CanWalkWithWeapon", true);
        }

        public void HideAndCreateNewMagazine()
        {
            Attacks[currentAttack].magazine.SetActive(false);
            
            if (Attacks[currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Rockets)
            {
                if (Attacks[currentAttack].attackEffects.Count > 0)
                {
                    foreach (var effect in Attacks[currentAttack].attackEffects)
                    {
                        if (effect && effect.emission.enabled)
                        {
                            var effectEmission = effect.emission;
                            effectEmission.enabled = false;
                        }
                    }
                }
            }
            
            var oldMag = Attacks[currentAttack].magazine;
            var newMag = Instantiate(oldMag);
            newMag.transform.parent = oldMag.transform.parent;
            newMag.transform.localPosition = oldMag.transform.localPosition;
            newMag.transform.localEulerAngles = oldMag.transform.localEulerAngles;
            newMag.SetActive(true);
            Attacks[currentAttack].TempMagazine.Add(newMag);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Handles.zTest = CompareFunction.Less;
            // Handles.color = Color.green;
            // Handles.CubeHandleCap(0, IkObjects.RightObject.position, IkObjects.RightObject.rotation, 0.1f, EventType.Repaint);
            // Handles.color = Color.red;
            // Handles.ArrowHandleCap(0, IkObjects.RightObject.position, IkObjects.RightObject.rotation, 1, EventType.Repaint);
            //
            //
            // Handles.zTest = CompareFunction.Greater;
            // Handles.color = new Color(0, 1, 0, 0.3f);
            // Handles.CubeHandleCap(0, IkObjects.RightObject.position, IkObjects.RightObject.rotation, 0.1f, EventType.Repaint);
            // Handles.ArrowHandleCap(0, IkObjects.RightObject.position, IkObjects.RightObject.rotation, 1, EventType.Repaint);

            
            if(Application.isPlaying || Attacks.Any(attack => attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade))
                return;
            
            if (Attacks[currentAttack].ShellPoint)
            {
                Handles.zTest = CompareFunction.Less;
                Handles.color = new Color32(250, 170, 0, 255);
                Handles.SphereHandleCap(0, Attacks[currentAttack].ShellPoint.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.ArrowHandleCap(0, Attacks[currentAttack].ShellPoint.position, Quaternion.LookRotation(Attacks[currentAttack].ShellPoint.forward), 0.2f, EventType.Repaint);

                Handles.zTest = CompareFunction.Greater;
                Handles.color = new Color32(250, 170, 0, 50);
                Handles.SphereHandleCap(0, Attacks[currentAttack].ShellPoint.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.ArrowHandleCap(0, Attacks[currentAttack].ShellPoint.position, Quaternion.LookRotation(Attacks[currentAttack].ShellPoint.forward), 0.2f, EventType.Repaint);
            }

            if (Attacks[currentAttack].AttackSpawnPoint)
            {
                Handles.zTest = CompareFunction.Less;
                Handles.color = new Color32(250, 0, 0, 255);
                Handles.SphereHandleCap(0, Attacks[currentAttack].AttackSpawnPoint.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.ArrowHandleCap(0, Attacks[currentAttack].AttackSpawnPoint.position, Quaternion.LookRotation(Attacks[currentAttack].AttackSpawnPoint.forward),
                    0.2f, EventType.Repaint);

                Handles.zTest = CompareFunction.Greater;
                Handles.color = new Color32(250, 0, 0, 50);
                Handles.SphereHandleCap(0, Attacks[currentAttack].AttackSpawnPoint.position, Quaternion.identity, 0.02f, EventType.Repaint);
                Handles.ArrowHandleCap(0, Attacks[currentAttack].AttackSpawnPoint.position, Quaternion.LookRotation(Attacks[currentAttack].AttackSpawnPoint.forward),
                    0.2f, EventType.Repaint);
            }
        }
#endif
    }
}





	


				
	
	