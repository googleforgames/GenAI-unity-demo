using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#if USK_MULTIPLAYER
using Photon.Pun;
#endif

namespace GercStudio.USK.Scripts
{

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] public CharacterHelper.InventorySlot[] slots = new CharacterHelper.InventorySlot[8];

        [SerializeField] public List<CharacterHelper.Kit> HealthKits;
        [SerializeField] public List<CharacterHelper.Kit> ReserveAmmo;
        
        public GameObject currentWeapon;

        public int currentSlot;
        public int previousSlot;
        public int previousWeaponInSlot;
        public int currentAmmoKit;
        public int currentHealthKit;
        public int allWeaponsCount;

        [Range(0, 10)]public float RateOfAttack = 0.7f;
        public float FistDamage = 20;
        public float debugIKValue = 1;
        public float currentIKWeight;
        public float currentHeadIKWeight;
        public float flashTimeout;

        public int inventoryTabUp;
        public int inventoryTabMiddle;
        public int currentInventorySlot;

        public string DropIdMultiplayer;
        public string currentPickUpId;
        
        public bool hasWeaponTaken;
        public bool hasAnyWeapon;
        public bool enablePickUpTooltip;
        public bool pickUpUiButton;
        public bool hideAllWeapons;
        public bool HasFistAttack;
        public bool inventoryIsOpened;
        
        public Texture FistIcon;
        
        public LineRenderer lineRenderer;
        public Projector bloodProjector;

        public AnimationClip HandsIdle;
        public AnimationClip HandsWalk;
        public AnimationClip HandsRun;
        
        public List<AnimationClip> fistAttackHandsAnimations;
        public List<AnimationClip> fistAttackFullBodyAnimations;
       
        [Tooltip("Add the [PlayAttackSound] event on an attack animation to set the exact playing time of a punch sound")]
        public AudioClip fistAttackAudio;

        public BoxCollider LeftHandCollider;
        public BoxCollider RightHandCollider;
        
        public Material trailMaterial;

        public RenderTexture ScopeScreenTexture;

        public Vector3 DropDirection;

        public Controller Controller;
        public WeaponController WeaponController;

        private int weaponId;
        public int animationIndex;
        public int lastAttackAnimationIndex = -1;
        
        private float _rateOfAttack;
        
        private bool activateMeleeTimer;
        private bool canChangeWeaponInSlot;
        private bool tempIsAim;
        private bool closeInventory;
        public bool hasWeaponChanged;
        private bool pressedUIInventoryButton;
        private bool canDropWeapon = true;
        private bool gamepadInfo;
        private bool setWeaponLayer = true;
        private bool firstLayerSet;
        private bool uiButtonAttack;
        private bool gamepadInput;
        private bool fistInstance;
        private bool inMultiplayerLobby;
        public bool activeAimByGamepadButton;
        
        private GameObject currentDropWeapon;

        private RaycastHit wallHitInfoRH;

        private void OnEnable()
        {

#if USK_MULTIPLAYER
            if (FindObjectOfType<LobbyManager>())
            {
                inMultiplayerLobby = true;
                return;
            }
			
#if USK_ADVANCED_MULTIPLAYER 
            if (FindObjectOfType<AdvancedLobbyManager>())
            {
                inMultiplayerLobby = true;
                return;
            }
#endif
#endif
            // if (Controller && !Controller.firstSceneStart)
            // {
            //     StopAllCoroutines();
            //     InitializeParameters(false);
            // }
        }

        public void InitializeParameters(bool resetWeapons)
        {
            if(Controller.AdjustmentScene) return;

            currentIKWeight = 0;
            hasWeaponTaken = false;
            
            if (!fistInstance)
            {
                CharacterHelper.SetInventoryUI(Controller, this);
                fistInstance = true;
            }

            if (resetWeapons)
            {
                InstantiateWeaponsAtStart();
            }
            else
            {
                if(slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack) ActivateHandsOnlyMode(true);
                else ActivateWeapon(currentSlot, false, true);
            }

            if (!Controller.AdjustmentScene && Controller.UIManager && Controller.UIManager.CharacterUI.WeaponAmmo)
                Controller.UIManager.CharacterUI.WeaponAmmo.gameObject.SetActive(true);

            if (!lineRenderer)
            {
                Helper.SetLineRenderer(ref lineRenderer, gameObject, trailMaterial);
            }

            if(!gameObject.GetComponent<AudioSource>())
                gameObject.AddComponent<AudioSource>();

            if (LeftHandCollider)
                CharacterHelper.SetMeleeCollider(ref LeftHandCollider, Controller, FistDamage, USKIneractionWithEmeraldAI.ColliderType.Melee);

            if (RightHandCollider)
                CharacterHelper.SetMeleeCollider(ref RightHandCollider, Controller, FistDamage, USKIneractionWithEmeraldAI.ColliderType.Melee);
            

            if (!Controller.AdjustmentScene 
#if USK_MULTIPLAYER
                && (Controller.GetComponent<PhotonView>() && Controller.GetComponent<PhotonView>().IsMine || !Controller.GetComponent<PhotonView>())
#endif                
            )
                    
                DeactivateInventory();
        }

        public void ActivateFirstWeapon()
        {
            currentSlot = 7;
            slots[0].currentWeaponInSlot = 0;
            FindWeapon(true, true);
        }

        void Update()
        {
            if (inMultiplayerLobby)
            {
                return;
            }
            
            if (Controller.isRemoteCharacter || !Controller.ActiveCharacter || Controller.AdjustmentScene)
                return;

            if (WeaponController && WeaponController.gameObject.activeSelf)
            {
                var startPoint = WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade ? WeaponController.transform : WeaponController.Attacks[WeaponController.currentAttack].AttackSpawnPoint;
                var enable = ((WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade || WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher) && WeaponController.Attacks[WeaponController.currentAttack].showTrajectory)
                             && (WeaponController.isAimEnabled || Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown) && WeaponController.canDrawGrenadesPath && !WeaponController.isReloadEnabled && hasWeaponTaken;

                WeaponsHelper.ShowGrenadeTrajectory(enable, startPoint, lineRenderer, Controller, WeaponController, transform);
            }

            if (WeaponController)
            {
                if (WeaponController.resetBobbing)
                {
                    Controller.CameraController.InitializeHeadBobbingValues(WeaponController);
                    WeaponController.resetBobbing = false;
                }
            }
            
            if (Controller.UIManager.CharacterUI.infoTooltip && (Controller.interactionWithCars && !Controller.interactionWithCars.showUITooltip || !Controller.interactionWithCars))
            {
                if (enablePickUpTooltip && !Controller.UIManager.CharacterUI.infoTooltip.gameObject.activeSelf)
                {
                    Helper.EnableAllParents(Controller.UIManager.CharacterUI.infoTooltip.gameObject);
                    Controller.UIManager.CharacterUI.infoTooltip.text = "Press ''" + (!Controller.CameraController.useGamepad ? Controller.projectSettings.keyboardButtonsInProjectSettings[8].ToString() : Controller.projectSettings.gamepadButtonsInProjectSettings[8].ToString()) + "'' button to pick up item";
                }
                else if(!enablePickUpTooltip && Controller.UIManager.CharacterUI.infoTooltip.gameObject.activeSelf)
                {
                    Controller.UIManager.CharacterUI.infoTooltip.gameObject.SetActive(false);
                }
            }

            FlashEffect();

            CheckPickUp();

            if (Controller.projectSettings.ButtonsActivityStatuses[9] && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[9])
                || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[9], Controller)))
                DropWeapon(true);

            if (Controller.projectSettings.ButtonsActivityStatuses[16] && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[16])
                || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[14], Controller)))
                WeaponUp();

            if (Controller.projectSettings.ButtonsActivityStatuses[16] && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[17])
                || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[15], Controller)))
                WeaponDown();

            if (slots[currentSlot].weaponSlotInGame.Count > 0 && slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
            {
                if (Controller.projectSettings.ButtonsActivityStatuses[3] && (!Controller.projectSettings.mobileDebug && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[3])
                    || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[3], Controller)) || uiButtonAttack))
                    
                {
                    Punch();
                }

                if (!Controller.projectSettings.mobileDebug)
                {
                    if (Controller.projectSettings.ButtonsActivityStatuses[5] && InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[5]))
                        //Input.GetKeyDown(Controller._keyboardCodes[5]))
                    {
                        Aim();
                        activeAimByGamepadButton = false;
                    }

                    if (Controller.projectSettings.ButtonsActivityStatuses[5] && InputHelper.IsGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[5], Controller))//(Input.GetKey(Controller._gamepadCodes[5]) || Helper.CheckGamepadAxisButton(5, Controller._gamepadButtonsAxes, Controller.hasAxisButtonPressed, "GetKey", Controller.projectSettings.AxisButtonValues[5])))
                    {
                        if (!Controller.CameraController.isCameraAimEnabled)
                        {
                            Aim();
                            activeAimByGamepadButton = true;
                        }
                    }
                    else
                    {
                        if (Controller.CameraController.isCameraAimEnabled && activeAimByGamepadButton)
                        {
                            Aim();
                            activeAimByGamepadButton = false;
                        }
                    }
                }
            }

            // if (Input.GetKeyDown(KeyCode.M))
            // {
            //     ClearAllWeapons();
            // }
            //
            // if (Input.GetKeyDown(KeyCode.H))
            // {
            //     SwitchWeaponInSlot();
            // }

            MeleeAttackTimeout();

            if (Controller.UIManager.CharacterUI.Inventory.MainObject)
            {
                if (!Application.isMobilePlatform && !Controller.projectSettings.mobileDebug)
                {
                    if (Controller.projectSettings.holdInventoryButton)
                    {
                        if (Controller.projectSettings.ButtonsActivityStatuses[7] && (InputHelper.IsKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[7])
                            || InputHelper.IsGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[7], Controller)))
                        {
                            ActivateInventory();
                        }
                        else
                        {
                            if (!closeInventory)
                            {
                                DeactivateInventory();
                                closeInventory = true;
                            }
                        }
                    }
                    else
                    {
                        if (Controller.projectSettings.ButtonsActivityStatuses[7] && (InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[7]) 
                            || InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[7], Controller)))
                        {
                            if (!inventoryIsOpened)
                            {
                                ActivateInventory();
                            }
                            else
                            {
                                if (!closeInventory)
                                {
                                    DeactivateInventory();
                                    closeInventory = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Controller.projectSettings.holdInventoryButton)
                    {
                        if (pressedUIInventoryButton)
                        {
                            ActivateInventory();
                        }
                        else
                        {
                            if (!closeInventory)
                            {
                                DeactivateInventory();
                                closeInventory = true;
                            }
                        }
                    }
                }

                if (!Controller.AdjustmentScene && inventoryIsOpened)
                    CheckInventoryButtons();
            }

            if (Controller.UIManager.CharacterUI.WeaponAmmo && !inventoryIsOpened)
            {
                if (WeaponController && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
                {
                    Controller.UIManager.CharacterUI.WeaponAmmo.color = WeaponController.Attacks[WeaponController.currentAttack].curAmmo > 0 ? Color.white : Color.red;

                    if (WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee)
                    {
                        if (WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                            Controller.UIManager.CharacterUI.WeaponAmmo.text = WeaponController.Attacks[WeaponController.currentAttack].curAmmo.ToString("F0") + "/" +
                                                                               WeaponController.Attacks[WeaponController.currentAttack].inventoryAmmo.ToString("F0");
                        else Controller.UIManager.CharacterUI.WeaponAmmo.text = WeaponController.Attacks[WeaponController.currentAttack].curAmmo.ToString("F0");
                    }
                    else Controller.UIManager.CharacterUI.WeaponAmmo.text = WeaponController.gameObject.name;
                }
                else Controller.UIManager.CharacterUI.WeaponAmmo.text = " ";
            }

            if (Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder && !inventoryIsOpened)
            {
                if (slots[currentSlot].weaponSlotInGame.Count > 0 && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack && WeaponController)
                {
                    if (WeaponController.weaponImage && Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder.texture != WeaponController.weaponImage)
                    {
                        Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder.texture = WeaponController.weaponImage;
                    }
                    
                    if (Controller.UIManager.CharacterUI.attackImage)
                    {
                        if (WeaponController.Attacks[WeaponController.currentAttack].attackImage)
                        {
                            if(!Controller.UIManager.CharacterUI.attackImage.gameObject.activeSelf)
                                Controller.UIManager.CharacterUI.attackImage.gameObject.SetActive(true);

                            if(Controller.UIManager.CharacterUI.attackImage.texture != WeaponController.Attacks[WeaponController.currentAttack].attackImage)
                                Controller.UIManager.CharacterUI.attackImage.texture = WeaponController.Attacks[WeaponController.currentAttack].attackImage;
                        }    
                        else if(!WeaponController.Attacks[WeaponController.currentAttack].attackImage && Controller.UIManager.CharacterUI.attackImage.gameObject.activeSelf)
                        {
                            Controller.UIManager.CharacterUI.attackImage.gameObject.SetActive(false);
                        }
                    }
                }
                else if (slots[currentSlot].weaponSlotInGame.Count > 0 && slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack &&  Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder.texture != FistIcon)
                {
                    if (FistIcon)
                        Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder.texture = FistIcon;
                }
            }
        }

        void MeleeAttackTimeout()
        {
            _rateOfAttack += Time.deltaTime;
            
            float rateOfAttackLimit;

            if (fistAttackHandsAnimations.Count > 0 && fistAttackHandsAnimations[animationIndex])
            {
                if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                    rateOfAttackLimit = RateOfAttack / 2;
                else rateOfAttackLimit = fistAttackHandsAnimations[animationIndex].length;

                if (_rateOfAttack > rateOfAttackLimit && activateMeleeTimer)
                {
                    DisablePunchAttack();
                }

                if (_rateOfAttack > RateOfAttack && activateMeleeTimer)
                {
                    MeleeColliders("off");
                    activateMeleeTimer = false;
                }
            }
        }
        
        public void MeleeColliders(string status)
        {
            if (WeaponController && WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee)
            {
                var curAttack = WeaponController.Attacks[WeaponController.currentAttack]; 
                if (curAttack.AttackCollider)
                    curAttack.AttackCollider.enabled = status == "on";
                
#if USK_DESTROYIT_INTEGRATION
                if (status == "on")
                {
                    if (curAttack.weapon_damage >= 0)
                        WeaponsHelper.DestroyItMeleeDamage(transform, 2, curAttack.weapon_damage);
                    else
                        WeaponsHelper.DestroyItOnRepair(transform.position, 2, curAttack.weapon_damage * -1);
                }
#endif
            }
            else
            {
                if (LeftHandCollider)
                    LeftHandCollider.enabled = status == "on";

                if (RightHandCollider)
                    RightHandCollider.enabled = status == "on";
                
#if USK_DESTROYIT_INTEGRATION
                if (status == "on")
                {
                    if (FistDamage >= 0)
                        WeaponsHelper.DestroyItMeleeDamage(transform, 2, FistDamage);
                    else
                        WeaponsHelper.DestroyItOnRepair(RightHandCollider != null ? RightHandCollider.transform.position : transform.position, 2, FistDamage * -1);
                }
#endif
            }
        }

        public void DisablePunchAttack()
        {
            Controller.anim.SetBool("Attack", false);
            Controller.anim.SetBool("Pause", false);
            // Controller.anim.SetBool("MeleeAttack", false);
            
#if USK_MULTIPLAYER
            if(!Controller.isRemoteCharacter && Controller.CharacterSync)
                Controller.CharacterSync.MeleeAttack(false, 0);
#endif
        }

        void Aim()
        {
            if (!WeaponsHelper.CanAim(false, Controller)) return;

            if (slots[currentSlot].weaponSlotInGame.Count > 0 && slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack || slots[currentSlot].weaponSlotInGame.Count == 0)
            {
                Controller.CameraController.Aim();
            }
        }
        
        public void Punch()
        {
            if (!Controller.isRemoteCharacter && ((slots[currentSlot].weaponSlotInGame.Count > 0 && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack) || slots[currentSlot].weaponSlotInGame.Count == 0 || Controller.isPause || Controller.CameraController.cameraPause || _rateOfAttack < RateOfAttack)) return;
           
            DisablePunchAttack();

            if (!Controller.isRemoteCharacter)
            {
                animationIndex = Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson ? WeaponsHelper.GetRandomIndex(fistAttackFullBodyAnimations, ref lastAttackAnimationIndex) : WeaponsHelper.GetRandomIndex(fistAttackHandsAnimations, ref lastAttackAnimationIndex);
            }

            AnimationClip animationClip = null;
                
            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
            {
                if (Controller.isCrouch || Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch->Idle") || Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Idle"))
                    return;
                
                animationClip = fistAttackFullBodyAnimations[animationIndex];

                if (fistAttackFullBodyAnimations[animationIndex])
                    Controller.ClipOverrides["_FullbodyMeleeAttack"] = fistAttackFullBodyAnimations[animationIndex];
            }
            else
            {
                animationClip = fistAttackHandsAnimations[animationIndex];
                
                if (fistAttackHandsAnimations[animationIndex])
                    Controller.ClipOverrides["_WeaponAttack"] = fistAttackHandsAnimations[animationIndex];
            }

            Controller.newController.ApplyOverrides(Controller.ClipOverrides);

            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.anim.GetLayerWeight(2) < 0.1f)
            {
                var duration = 0.3f;
                    
                if (Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Main Idle") || Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Idle"))
                    duration = 0.02f;
                else if (!Controller.anim.GetBool("Move"))
                    duration = 0;
                
                Controller.anim.CrossFade("Melee", duration, 0);
            }

            if (!Controller.isRemoteCharacter)
            {
#if USK_MULTIPLAYER
                if(Controller.CharacterSync)
                    Controller.CharacterSync.MeleeAttack(true, animationIndex);
#endif

                

                if (!WeaponsHelper.HasAnimationCollidersEvent(animationClip))
                {
                    MeleeColliders("on");
                }

                _rateOfAttack = 0;
                    
                activateMeleeTimer = true;
            }

            Controller.anim.SetBool("Attack", true);
            Controller.anim.CrossFade("Attack", 0, 1);
        }

        public void WeaponUp()
        {
            if (Controller.AdjustmentScene) return;

            if (WeaponController && ((WeaponController.isReloadEnabled || !hasWeaponTaken) || (WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade && Controller.anim.GetBool("Attack"))) || Controller.isPause || inventoryIsOpened) return;

            FindWeapon(true, true);
            SelectWeaponInInventory(currentSlot);

            Controller.UIManager.CharacterUI.ShowImage("weapon", this);
        }

        public void WeaponDown()
        {
            if (Controller.AdjustmentScene) return;

            if (WeaponController && (WeaponController.isReloadEnabled || !hasWeaponTaken) || Controller.isPause || Controller.anim.GetBool("Attack") || inventoryIsOpened) return;

            FindWeapon(false, true);
            SelectWeaponInInventory(currentSlot);
            
            Controller.UIManager.CharacterUI.ShowImage("weapon", this);
        }

        public void FindWeapon(bool plus, bool sendToNetwork)
        {
            if (slots[currentSlot].weaponSlotInGame.Count < 2 || CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot + (plus ? 1 : -1), plus) == -1)
            {
                Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "norm");

                hasAnyWeapon = false;
                slots[currentSlot].currentWeaponInSlot = 0;
                
                for (var i = 0; i < 8; i++)
                {
                    if (plus)
                    {
                        currentSlot++;

                        if (currentSlot > 7)
                            currentSlot = 0;
                        
                        slots[currentSlot].currentWeaponInSlot = 0;
                    }
                    else
                    {
                        currentSlot--;
                        
                        if (currentSlot < 0)
                            currentSlot = 7;
                        
                        slots[currentSlot].currentWeaponInSlot = slots[currentSlot].weaponSlotInGame.Count - 1;
                    }

                    if (slots[currentSlot].weaponSlotInGame.Count > 0 && CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot, plus) != -1)
                    {
                        slots[currentSlot].currentWeaponInSlot = CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot, plus);
                        
                        if (slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
                        {
                            hideAllWeapons = true;

                            GetNewWeapon(sendToNetwork);

                            hasAnyWeapon = false;
                            
                            break;
                        }

                        if (!WeaponController || WeaponController && WeaponController.gameObject.GetInstanceID() != slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].weapon.GetInstanceID())
                        {
                            hasWeaponChanged = true;

                            GetNewWeapon(sendToNetwork);
                            
                            hasAnyWeapon = true;
                            break;
                        }
                    }
                }
                
                if (!hasAnyWeapon)
                {
                    hideAllWeapons = true;
                    GetNewWeapon(sendToNetwork);
                }
            }
            else
            {
                slots[currentSlot].currentWeaponInSlot = CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot + (plus ? 1 : -1), plus);

                if (slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
                {
                    hideAllWeapons = true;
                }
                else
                {
                    hasWeaponChanged = true;
                }

                GetNewWeapon(sendToNetwork);
            }
        }

        void GetNewWeapon(bool sendToNetwork)
        {
            if (WeaponController)
            {
                if (WeaponController.isAimEnabled)
                {
                    WeaponController.Aim(true, false, false);
                    StartCoroutine(SwitchWeaponTimeOut());
                }
                else
                {
                    ChooseWeapon(sendToNetwork);
                }
            }
            else
            {
                ChooseWeapon(sendToNetwork);
            }
        }

        void InventoryGamepadInputs()
        {
            if(Gamepad.current == null) return;
            
            var vector = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[2].ReadValue();

            gamepadInput = vector.magnitude > 0;
            
            // gamepadInput = Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[2].x.ReadValue()) > 0.1f ||
            //                Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[2].y.ReadValue()) > 0.1f;
            

            // vector.y *= -1;
            
            
            vector.Normalize();

            if (Math.Abs(vector.x) < 0.4f & Math.Abs(vector.y - 1) < 0.4f)
            {
                if (slots[1].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(1);
                    DeselectAllSlots(1);
                    Helper.ChangeButtonColor(Controller.UIManager, 1, "high");
                }
            }
            else if (Math.Abs(vector.x - 0.707f) < 0.4f & Math.Abs(vector.y - 0.707f) < 0.4f)
            {
                if (slots[2].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(2);
                    DeselectAllSlots(2);
                    Helper.ChangeButtonColor(Controller.UIManager, 2, "high");
                }
            }
            else if (Math.Abs(vector.x - 1) < 0.4f & Math.Abs(vector.y) < 0.4f)
            {
                if (slots[3].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(3);
                    DeselectAllSlots(3);
                    Helper.ChangeButtonColor(Controller.UIManager, 3, "high");
                }
            }
            else if (Math.Abs(vector.x - 0.707f) < 0.4f & Math.Abs(vector.y + 0.707f) < 0.4f)
            {
                if (slots[4].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(4);
                    DeselectAllSlots(4);
                    Helper.ChangeButtonColor(Controller.UIManager, 4, "high");
                }
            }
            else if (Math.Abs(vector.x ) < 0.4f & Math.Abs(vector.y + 1) < 0.4f)
            {
                if (slots[5].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(5);
                    DeselectAllSlots(5);
                    Helper.ChangeButtonColor(Controller.UIManager, 5, "high");
                }
            }
            else if (Math.Abs(vector.x + 0.707f) < 0.4f & Math.Abs(vector.y + 0.707f) < 0.4f)
            {
                if (slots[6].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(6);
                    DeselectAllSlots(6);
                    Helper.ChangeButtonColor(Controller.UIManager, 6, "high");
                }
            }
            else if (Math.Abs(vector.x + 1) < 0.4f & Math.Abs(vector.y) < 0.4f)
            {
                if (slots[7].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(7);
                    DeselectAllSlots(7);
                    Helper.ChangeButtonColor(Controller.UIManager, 7, "high");
                }
            }
            else if (Math.Abs(vector.x + 0.707f) < 0.4f & Math.Abs(vector.y - 0.707f) < 0.4f)
            {
                if (slots[0].weaponSlotInGame.Count > 0)
                {
                    SelectWeaponInInventory(0);
                    DeselectAllSlots(0);
                    Helper.ChangeButtonColor(Controller.UIManager, 0, "high");
                }
            }

            var axis = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[0].y.ReadValue();//Input.GetAxis(Controller._gamepadAxes[4]);

            if (Math.Abs(axis + 1) < 0.1f)
            {
                if (canChangeWeaponInSlot)
                {
                    DownInventoryValue("weapon");
                    canChangeWeaponInSlot = false;
                }
            }
            else if (Math.Abs(axis - 1) < 0.1f)
            {
                if (canChangeWeaponInSlot)
                {
                    UpInventoryValue("weapon");
                    canChangeWeaponInSlot = false;
                }
            }
            else if (Math.Abs(axis) < 0.1f)
            {
                if(!canChangeWeaponInSlot)
                    canChangeWeaponInSlot = true;
            }


            if (InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[12], Controller))
                UseKit("health");
            
            
            if (InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[13], Controller))
                if(slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count > 0)
                    UseKit("ammo");
        }

        void DeselectAllSlots(int curSlot)
        {
            for (var i = 0; i < 8; i++)
            {
                if (i != curSlot)
                    Helper.ChangeButtonColor(Controller.UIManager, i, "norm");
            }
            
            if(Controller.UIManager.CharacterUI.Inventory.HealthButton)
                Helper.ChangeButtonColor(Controller.UIManager.CharacterUI.Inventory.HealthButton, Controller.UIManager.CharacterUI.Inventory.normButtonsColors[8], Controller.UIManager.CharacterUI.Inventory.normButtonsSprites[8]);
            
            if(Controller.UIManager.CharacterUI.Inventory.AmmoButton)
                Helper.ChangeButtonColor(Controller.UIManager.CharacterUI.Inventory.AmmoButton, Controller.UIManager.CharacterUI.Inventory.normButtonsColors[9], Controller.UIManager.CharacterUI.Inventory.normButtonsSprites[9]);
        }

        void CheckInventoryButtons()
        {
            InventoryGamepadInputs();
            
            if (Controller.UIManager.CharacterUI.Inventory.UpHealthButton)
                Controller.UIManager.CharacterUI.Inventory.UpHealthButton.gameObject.SetActive(HealthKits.Count > 1 && !gamepadInput);

            if (Controller.UIManager.CharacterUI.Inventory.DownHealthButton)
                Controller.UIManager.CharacterUI.Inventory.DownHealthButton.gameObject.SetActive(HealthKits.Count > 1 && !gamepadInput);

            if (slots[currentSlot].weaponSlotInGame.Count > 0 && !gamepadInput)
            {
                if (Controller.UIManager.CharacterUI.Inventory.UpAmmoButton)
                    Controller.UIManager.CharacterUI.Inventory.UpAmmoButton.gameObject.SetActive(slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count > 1);
                
                if (Controller.UIManager.CharacterUI.Inventory.DownAmmoButton)
                    Controller.UIManager.CharacterUI.Inventory.DownAmmoButton.gameObject.SetActive(slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count > 1);

            }
            else
            {
                if (Controller.UIManager.CharacterUI.Inventory.UpAmmoButton)
                    Controller.UIManager.CharacterUI.Inventory.UpAmmoButton.gameObject.SetActive(false);
                
                if (Controller.UIManager.CharacterUI.Inventory.DownAmmoButton)
                    Controller.UIManager.CharacterUI.Inventory.DownAmmoButton.gameObject.SetActive(false);
            }
            
            if (Controller.UIManager.CharacterUI.Inventory.UpWeaponButton)
                Controller.UIManager.CharacterUI.Inventory.UpWeaponButton.gameObject.SetActive(slots[currentSlot].weaponSlotInGame.Count > 1);

            if (Controller.UIManager.CharacterUI.Inventory.DownWeaponButton)
                Controller.UIManager.CharacterUI.Inventory.DownWeaponButton.gameObject.SetActive(slots[currentSlot].weaponSlotInGame.Count > 1);

            if (Controller.UIManager.CharacterUI.Inventory.WeaponsCount)
            {
                Controller.UIManager.CharacterUI.Inventory.WeaponsCount.gameObject.SetActive(slots[currentSlot].weaponSlotInGame.Count > 1);

                Controller.UIManager.CharacterUI.Inventory.WeaponsCount.text = slots[currentSlot].currentWeaponInSlot + 1 + "/" + slots[currentSlot].weaponSlotInGame.Count;
            }
        }
        
        public void ClearAllWeapons()
        {
            WeaponController = null;
            currentWeapon = null;
            
            for (var i = 0; i < 8; i++)
            {
                foreach (var slot in slots[i].weaponSlotInGame.Where(slot => slot != null && slot.weapon))
                {
                    foreach (var kit in slot.WeaponAmmoKits)
                    {
                        ReserveAmmo.Add(kit);
                    }
        
                    Destroy(slot.weapon);
                }
        
                slots[i].weaponSlotInGame.Clear();
                slots[i].currentWeaponInSlot = 0;
            }
        }

        public WeaponController AddNewWeapon(GameObject weapon, int inventorySlot, List<CharacterHelper.Kit> ammoKits = null)
        {
            return WeaponsHelper.InstantiateWeapon(weapon, inventorySlot, this, Controller, ammoKits).GetComponent<WeaponController>();
        }

        public void DropWeapon(bool getNewWeapon)
        {
            if ((!WeaponController || Controller.isPause || Controller.CameraController.cameraPause || Controller.AdjustmentScene || !canDropWeapon || !hasAnyWeapon || slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack || slots[currentSlot].weaponSlotInGame.Count <= 0 ||
                 ((WeaponController.isAimEnabled || WeaponController.isReloadEnabled) && getNewWeapon) || !hasWeaponTaken || 
#if USK_ADVANCED_MULTIPLAYER
                 Controller.CharacterSync && Controller.CharacterSync.advancedRoomManager && !Controller.CharacterSync.advancedRoomManager.matchStarted ||
#endif
                 WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade) && !Controller.isRemoteCharacter)
                return;

            if (SaveManager.Instance)
            {
                var weaponAttackParameters = WeaponController.Attacks.Select(attack => new WeaponsHelper.Attack {curAmmo = attack.curAmmo, inventoryAmmo = attack.inventoryAmmo}).ToList();
                SaveManager.Instance.sceneSaveData.droppedWeapons.Add(new SaveManager.DroppedWeapons{position = gameObject.transform.position + Vector3.up, weaponSaveData = new SaveManager.WeaponSaveData{id = WeaponController.weaponID, currentAttack = WeaponController.currentAttack, inventorySlot = currentSlot + 1, name = WeaponController.gameObject.name, weaponAttackParameters = weaponAttackParameters}});    
            }
            
            if (WeaponController.DropWeaponAudio)
                GetComponent<AudioSource>().PlayOneShot(WeaponController.DropWeaponAudio);

            Helper.ChangeLayersRecursively(WeaponController.transform, "Default");

            WeaponController = null;
            currentWeapon = null;

            var curIndex = slots[currentSlot].currentWeaponInSlot;
            var curWeapon = slots[currentSlot].weaponSlotInGame[curIndex];
            var weaponController = curWeapon.weapon.GetComponent<WeaponController>();
            
            weaponController.enabled = false;

            WeaponsHelper.AddPickupItemScript(curWeapon.weapon, this, currentSlot + 1);

            if (!Controller.isRemoteCharacter)
            {
                switch (Controller.TypeOfCamera)
                {
                    case CharacterHelper.CameraType.ThirdPerson:
                        DropDirection = Controller.DirectionObject.forward * 5;
                        break;
                    case CharacterHelper.CameraType.FirstPerson:
                        DropDirection = Controller.thisCamera.transform.forward * 5 ;
                        break;
                    case CharacterHelper.CameraType.TopDown:
                        DropDirection = Controller.DirectionObject.forward * 5;
                        break;
                }
            }

            curWeapon.weapon.GetComponent<BoxCollider>().isTrigger = false;
            curWeapon.weapon.transform.parent = null;

            var rigidbodyComponent = curWeapon.weapon.GetComponent<Rigidbody>() ? curWeapon.weapon.GetComponent<Rigidbody>() : curWeapon.weapon.AddComponent<Rigidbody>();

            rigidbodyComponent.velocity = DropDirection;
            rigidbodyComponent.isKinematic = false;
            rigidbodyComponent.useGravity = true;

            foreach (var kit in slots[currentSlot].weaponSlotInGame[curIndex].WeaponAmmoKits)
            {
                ReserveAmmo.Add(kit);
            }

            slots[currentSlot].weaponSlotInGame.Remove(curWeapon);

            if (getNewWeapon)
            {
                if (slots[currentSlot].weaponSlotInGame.Count > 0)
                {
                    if (CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot, true) != -1)
                    {
                        slots[currentSlot].currentWeaponInSlot = CharacterHelper.FindWeapon(slots, currentSlot, slots[currentSlot].currentWeaponInSlot, true);

                        if (slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack) ActivateHandsOnlyMode(false);
                        else ActivateWeapon(currentSlot, false, false);
                    }
                    else
                    {
                        FindWeapon(true, false);
                    }
                }
                else
                {
                    FindWeapon(true, false);
                }
            }
            else
            {
                ActivateHandsOnlyMode(false);
            }

            if (!Controller.isRemoteCharacter)
            {
#if USK_MULTIPLAYER
                if (Controller.CharacterSync)
                    Controller.CharacterSync.DropWeapon(getNewWeapon);
#endif
                
                canDropWeapon = false;
                StartCoroutine(DropTimeOut(curWeapon));
            }
        }

        void ActivateInventory()
        {
            if (WeaponController)
                if (WeaponController.isAimEnabled && WeaponController.useAimTexture || WeaponController.isReloadEnabled || !hasWeaponTaken)
                    return;

            if (Controller.isPause || inventoryIsOpened || Controller.AdjustmentScene)
                return;
            
#if USK_ADVANCED_MULTIPLAYER && USK_MULTIPLAYER
            if (Controller.CharacterSync && Controller.CharacterSync.advancedRoomManager && !Controller.CharacterSync.advancedRoomManager.canPause)
                return;
#endif
            
            previousSlot = currentSlot;
            previousWeaponInSlot = slots[currentSlot].currentWeaponInSlot;
            
            inventoryIsOpened = true;

            UIHelper.ManageUIButtons(Controller, this, Controller.UIManager, Controller.CharacterSync);

            CheckInventoryButtons();

            DeselectAllSlots(currentSlot);
            
            if (slots[currentSlot].weaponSlotInGame.Count > 0 && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
            {
                var _weaponController = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();

                if (_weaponController.Attacks[_weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade
                    && _weaponController.Attacks[_weaponController.currentAttack].curAmmo > 0
                    || _weaponController.Attacks[_weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                    Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "high");
            }
            else
            {
                Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "high");
            }

            Controller.UIManager.CharacterUI.ShowImage("weapon", this);
            Controller.UIManager.CharacterUI.ShowImage("health", this);
            Controller.UIManager.CharacterUI.ShowImage("ammo", this);

            Controller.UIManager.CharacterUI.Inventory.MainObject.SetActive(true);

            Controller.hasMoveButtonPressed = false;

            Controller.CameraController.cameraPause = true;

            closeInventory = false;
        }

        void DeactivateInventory()
        {
            if (!Controller.UIManager || Controller.UIManager && !Controller.UIManager.CharacterUI.Inventory.MainObject)
                return;
            
            Controller.UIManager.CharacterUI.Inventory.MainObject.SetActive(false);
            inventoryIsOpened = false;
            
            Controller.CameraController.cameraPause = false;

            if (slots[currentSlot].weaponSlotInGame.Count > 0 && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
            {
                var _controller = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();

                if (_controller.Attacks[_controller.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade
                    && _controller.Attacks[_controller.currentAttack].curAmmo > 0
                    || _controller.Attacks[_controller.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                {
                    Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "high");
                }
                else
                {
                    Helper.ChangeButtonColor(Controller.UIManager, previousSlot, "high");
                    Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "norm");

                    currentSlot = previousSlot;
                    slots[currentSlot].currentWeaponInSlot = previousWeaponInSlot;
                }
            }
            else
            {
                Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "high");
            }

            if(Controller.UIManager.CharacterUI.Inventory.AmmoButton)
                Helper.ChangeButtonColor(Controller.UIManager.CharacterUI.Inventory.AmmoButton, Controller.UIManager.CharacterUI.Inventory.normButtonsColors[9], Controller.UIManager.CharacterUI.Inventory.normButtonsSprites[9]);
            
            if(Controller.UIManager.CharacterUI.Inventory.HealthButton)
                Helper.ChangeButtonColor(Controller.UIManager.CharacterUI.Inventory.HealthButton, Controller.UIManager.CharacterUI.Inventory.normButtonsColors[8], Controller.UIManager.CharacterUI.Inventory.normButtonsSprites[8]);

            UIHelper.ManageUIButtons(Controller, this, Controller.UIManager, Controller.CharacterSync);

            if (hasWeaponChanged || hideAllWeapons)
            {
                if (WeaponController)
                {
                    if (WeaponController.isAimEnabled)
                    {
                        WeaponController.Aim(true, false, false);
                        StartCoroutine(SwitchWeaponTimeOut());
                    }
                    else
                    {
                        ChooseWeapon(true);
                    }
                }
                else
                {
                    ChooseWeapon(true);
                }
            }
        }

        public void ChooseWeapon(bool sendToNetwork)
        {
            if (hasWeaponChanged)
            {
                ActivateWeapon(currentSlot, false, sendToNetwork);
                hasWeaponChanged = false;
            }
            else if(hideAllWeapons)
            {
               ActivateHandsOnlyMode(sendToNetwork);
            }
        }

        void ActivateHandsOnlyMode(bool sendToNetwork)
        {
#if USK_MULTIPLAYER
            if (Controller && !Controller.isRemoteCharacter && Controller.CharacterSync && sendToNetwork)
                Controller.CharacterSync.ChangeWeapon(false);
#endif

//            if (Controller.UIManager.CharacterUI.WeaponAmmo)
//                Controller.UIManager.CharacterUI.WeaponAmmo.gameObject.SetActive(false);
                    
            NullWeapons();
            
            Controller.anim.SetBool("NoWeapons", true);
                
            StopCoroutine("TakeWeapon");
            
            Controller.speedDivider = 1;
                
            Controller.anim.Play("Take Weapon", 1);
            Controller.anim.Play("Take Weapon", 2);
            
            currentIKWeight = 0;
                
            SetHandsAnimations();
            
            Controller.CameraController.InitializeHeadBobbingValues(null);
            
            UIHelper.ManageUIButtons(Controller, this, Controller.UIManager, Controller.CharacterSync);

            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                StartCoroutine(ChangeAnimatorLayers(0));
            
            if (FistIcon)
                Controller.UIManager.CharacterUI.WeaponAmmoImagePlaceholder.texture = FistIcon;
                
            hideAllWeapons = true; //false;  // Commented out by dz on 20230707 in order to remove weapons.
            hasAnyWeapon = false;

            WeaponController = null;
        }

        void SetHandsAnimations()
        {
            if (HandsIdle)
                Controller.ClipOverrides["_WeaponIdle"] = HandsIdle;
            
            if (HandsWalk)
                Controller.ClipOverrides["_WeaponWalk"] = HandsWalk;

            if (HandsRun)
                Controller.ClipOverrides["_WeaponRun"] = HandsRun;
            
            Controller.newController.ApplyOverrides(Controller.ClipOverrides);
        }
        
        IEnumerator ChangeAnimatorLayers(int value)
        {
            while (true)
            {
                Controller.anim.SetLayerWeight(2, Mathf.Lerp(Controller.anim.GetLayerWeight(2), value, 10 * Time.deltaTime));

                if (Math.Abs(Controller.anim.GetLayerWeight(2) - value) < 0.1f)
                {
                    Controller.anim.SetLayerWeight(2, value);
                    break;
                }

                yield return 0;
            }
        }

        IEnumerator SwitchWeaponTimeOut()
        {
            while (true)
            {
                if (WeaponController.setHandsPositionsAim)
                {
                    yield return new WaitForSeconds(0.1f);

                    ChooseWeapon(true);
                    break;
                }
                
                yield return 0;
            }
        }


        public void SelectWeaponInInventory(int slot)
        {
            return;
            if (slots[slot].weaponSlotInGame.Count <= 0 && !slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].fistAttack)
                return;

            if (!slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].fistAttack)
            {
                weaponId = slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon.GetInstanceID();

                var _controller = slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();
                
                if (_controller.Attacks[_controller.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade && _controller.Attacks[_controller.currentAttack].curAmmo > 0
                    || _controller.Attacks[_controller.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                {
                    hideAllWeapons = true; //false;  // Commented out by dz on 20230707 in order to remove weapons.

                    if (currentWeapon)
                    {
                        if (hasAnyWeapon)
                        {
                            hasWeaponChanged = currentWeapon.GetInstanceID() != weaponId && slots[slot].weaponSlotInGame.Count > 0 && slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon;
                        }
                        else hasWeaponChanged = true;
                    }
                    else
                    {
                        hasWeaponChanged = true;
                    }
                }
                else
                {
                    hasWeaponChanged = false;
                }
            }
            else
            {
                hideAllWeapons = true;
                hasWeaponChanged = false;
            }

            if (!gamepadInput)
            {
                if (currentSlot != slot)
                {
                    Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "norm");
                }
            }
            
            currentSlot = slot;
            
            Controller.UIManager.CharacterUI.ShowImage("ammo", this);

            Helper.ChangeButtonColor(Controller.UIManager, currentSlot, "high");
        }

        public void UpInventoryValue(string type)
        {
            switch (type)
            {
                case "weapon":
                {
                    var curWeapon = slots[currentSlot].currentWeaponInSlot;
                    curWeapon++;

                    if (curWeapon > slots[currentSlot].weaponSlotInGame.Count - 1)
                        curWeapon = 0;

                    slots[currentSlot].currentWeaponInSlot = curWeapon;

                    Controller.UIManager.CharacterUI.ShowImage("weapon", this);
                    SelectWeaponInInventory(currentSlot);
                    break;
                }
                case "health":

                    var curKit = currentHealthKit;
                    curKit++;
                    if (curKit > HealthKits.Count - 1)
                        curKit = 0;
                    currentHealthKit = curKit;

                    Controller.UIManager.CharacterUI.ShowImage("health", this);
                    break;

                case "ammo":

                    curKit = currentAmmoKit;
                    curKit++;
                    
                    if (curKit > slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count - 1)
                        curKit = 0;
                    
                    currentAmmoKit = curKit;

                    Controller.UIManager.CharacterUI.ShowImage("ammo", this);
                    break;
            }
        }

        public void DownInventoryValue(string type)
        {
            switch (type)
            {
                case "weapon":
                
                    var curWeapon = slots[currentSlot].currentWeaponInSlot;
                    curWeapon--;

                    if (curWeapon < 0)
                        curWeapon = slots[currentSlot].weaponSlotInGame.Count - 1;

                    slots[currentSlot].currentWeaponInSlot = curWeapon;

                    Controller.UIManager.CharacterUI.ShowImage("weapon", this);
                    SelectWeaponInInventory(currentSlot);
                    break;
                
                case "health":

                    var curKit = currentHealthKit;
                    curKit--;
                    if (curKit < 0)
                        curKit = HealthKits.Count - 1;
                    currentHealthKit = curKit;

                    Controller.UIManager.CharacterUI.ShowImage("health", this);
                    break;
                
                case "ammo":
                    curKit = currentAmmoKit;
                    curKit--;
                    
                    if (curKit < 0)
                        curKit = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count - 1;
                    
                    currentAmmoKit = curKit;

                    Controller.UIManager.CharacterUI.ShowImage("ammo", this);
                    break;
            }
        }

        public void UseKit(string type)
        {
            switch (type)
            {
                case "health":
                    if (HealthKits.Count <= 0)
                        return;

                    Controller.health += HealthKits[currentHealthKit].AddedValue;
                    HealthKits.Remove(HealthKits[currentHealthKit]);
                    var curIndex = currentHealthKit;
                    curIndex++;
                    if (curIndex > HealthKits.Count - 1)
                        curIndex = 0;
                    currentHealthKit = curIndex;
                    Controller.UIManager.CharacterUI.ShowImage("health", this);
                    
#if USK_MULTIPLAYER
                    if(!Controller.isRemoteCharacter && Controller.CharacterSync)
                        Controller.CharacterSync.UseHealthKit();
#endif
                    
                    break;
                case "ammo":
                    
                    if (slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count <= 0)
                        return;

                    var ammoKit = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits[currentAmmoKit];
                    var weaponController = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();

                    foreach (var attack in weaponController.Attacks)
                    {
                        if (attack.ammoName == ammoKit.ammoType || ammoKit.ammoType == "")
                        {
                            weaponController.Attacks[weaponController.currentAttack].inventoryAmmo += ammoKit.AddedValue;

                            if (attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                            {
                                weaponController.Attacks[weaponController.currentAttack].curAmmo = weaponController.Attacks[weaponController.currentAttack].inventoryAmmo;
                            }

                            break;
                        }
                    }
                    
                    for (var i = 0; i < 8; i++)
                    {
                        foreach (var weapon in slots[i].weaponSlotInGame)
                        {
                            if (weapon.WeaponAmmoKits.Exists(x => x.PickUpId == ammoKit.PickUpId))
                            {
                                var kit = weapon.WeaponAmmoKits.Find(x => x.PickUpId == ammoKit.PickUpId);
                                weapon.WeaponAmmoKits.Remove(kit);
                            }
                        }
                    }

                    curIndex = currentAmmoKit;
                    curIndex++;
                    
                    if (curIndex > slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count - 1)
                        curIndex = 0;
                    
                    currentAmmoKit = curIndex;

                    Controller.UIManager.CharacterUI.ShowImage("ammo", this);
                    Controller.UIManager.CharacterUI.ShowImage("weapon", this);
                    
                    break;
            }
        }

        void CheckPickUp()
        {
            if (Controller.isPause || Controller.CameraController.cameraPause || Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown)
                return;

            if (WeaponController)
                if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson && WeaponController.isAimEnabled || WeaponController.isReloadEnabled || !hasWeaponTaken)
                {
                    enablePickUpTooltip = false;
                    return;
                }

            var Hit = new RaycastHit();

            if (Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown && !Controller.emulateTDModeLikeTP)
            {
                var direction = Controller.thisCamera.transform.TransformDirection(Vector3.forward);
                if (!Physics.Raycast(Controller.thisCamera.transform.position, direction, out Hit, 100, Helper.LayerMask())) return;

            }
            else
            {
                if (!Physics.Raycast(Controller.BodyObjects.Head.position + transform.forward * 2, Vector3.down * 3, out Hit, 100, Helper.LayerMask())) return;
            }

            {
                if (Hit.collider.GetComponent<PickupItem>())
                {
                    if (!Hit.collider.GetComponent<PickupItem>().isActiveAndEnabled) return;

                    var pickUp = Hit.collider.GetComponent<PickupItem>();

                    if (pickUp.method == PickupItem.PickUpMethod.Raycast || pickUp.method == PickupItem.PickUpMethod.Both)
                    {
                        var correctedDistance = Hit.distance;

                        if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !Controller.emulateTDModeLikeTP)
                        {
                            correctedDistance = Vector3.Distance(Controller.BodyObjects.Head.position, Hit.point);
                        }
                        
                        if (correctedDistance <= pickUp.distance)
                        {
                            enablePickUpTooltip = true;

                            if (pickUpUiButton || InputHelper.WasKeyboardOrMouseButtonPressed(Controller.projectSettings.keyboardButtonsInUnityInputSystem[8]) || 
                                InputHelper.WasGamepadButtonPressed(Controller.projectSettings.gamepadButtonsInUnityInputSystem[8]))
                            {
                                pickUp.PickUpObject(gameObject);
                                currentPickUpId = pickUp.pickUpId;

#if USK_MULTIPLAYER
                                if (Controller.CharacterSync)
                                    Controller.CharacterSync.PickUp();
#endif

                                pickUpUiButton = false;
                            }
                        }
                        else
                        {
                            enablePickUpTooltip = false;
                        }
                    }
                }
                else
                {
                    enablePickUpTooltip = false;
                }
            }

        }
        
        void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<PickupItem>())
            {
                var pickUp = other.GetComponent<PickupItem>();
                
                if ((pickUp.method == PickupItem.PickUpMethod.Collider || pickUp.method == PickupItem.PickUpMethod.Both) && pickUp.enabled)
                {
                    pickUp.PickUpObject(gameObject);
                    currentPickUpId = pickUp.pickUpId;
                    
#if USK_MULTIPLAYER
                    if(Controller.CharacterSync)
                        Controller.CharacterSync.PickUp(); 
#endif
                }
            }
        }

        public void NullWeapons()
        {
            for (var i = 0; i < 8; i++)
            {
                foreach (var weapon in slots[i].weaponSlotInGame)
                {
                    if(weapon.weapon)
                        weapon.weapon.SetActive(false);
                }
            }
        }

        public void ActivateWeapon(int slot, bool isGrenade, bool sendToNetwork)
        {
            if(slots[slot].weaponSlotInGame.Count <= slots[slot].currentWeaponInSlot) return;
            
            StopCoroutine("TakeWeapon");

            NullWeapons();
            
            slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon.SetActive(true);
                
            WeaponController = slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();
            WeaponController.canAttack = false;
            WeaponController.enabled = true;
            
            WeaponController.Controller = Controller;
            WeaponController.CurrentWeaponInfo.Clear();

            foreach (var weaponInfo in WeaponController.WeaponInfos)
            {
                var info = new WeaponsHelper.WeaponInfo();
                info.Clone(weaponInfo);
                WeaponController.CurrentWeaponInfo.Add(info);
            }

            if (!isGrenade)
            {
                ResetAnimatorParameters();
                SetWeaponAnimations(false, Controller.TypeOfCamera);

                Controller.CameraController.InitializeHeadBobbingValues(WeaponController);

                if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson && !Controller.isRemoteCharacter
                                                                                      && (!Controller.isCrouch && !WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState ||
                                                                                          Controller.isCrouch && !WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState))
                    StartCoroutine(ChangeAnimatorLayers(1));

                currentWeapon = slots[slot].weaponSlotInGame[slots[slot].currentWeaponInSlot].weapon;

                Helper.ChangeLayersRecursively(currentWeapon.transform, "Character");
                
                Controller.anim.SetBool("HasWeaponTaken", false);
                currentIKWeight = 0;

                if (WeaponController.useScope && WeaponController.ScopeScreen)
                {
                    if(!ScopeScreenTexture)			
                        ScopeScreenTexture = new RenderTexture(1024, 1024, 24);

                    WeaponController.ScopeScreen.GetComponent<MeshRenderer>().material.mainTexture = ScopeScreenTexture;
                }

                if (WeaponController.useAimTexture && Controller.UIManager.CharacterUI.aimPlaceholder)
                {
                    Controller.UIManager.CharacterUI.aimPlaceholder.texture = WeaponController.aimCrosshairTexture;

                    if (Controller.UIManager.leftScopeTextureFill && Controller.UIManager.rightScopeTextureFill)
                    {
                        Controller.UIManager.leftScopeTextureFill.GetComponent<Image>().color = WeaponController.fillColor;
                        Controller.UIManager.rightScopeTextureFill.GetComponent<Image>().color = WeaponController.fillColor;
                    }
                }
            }

            UIHelper.ManageUIButtons(Controller, this, Controller.UIManager, Controller.CharacterSync);
            SetCrosshair();

            firstLayerSet = false;
            hasWeaponTaken = false;

            if (!Controller.AdjustmentScene)
            {
                if (!isGrenade)
                {
                    Controller.anim.Play("Take Weapon", 1);
                    Controller.anim.Play("Take Weapon", 2);
                }
                else
                {
                    if (!WeaponController.Attacks[WeaponController.currentAttack].useTakeAnimation)
                    {
                        Controller.anim.CrossFade("Idle", 0.5f, 1);
                        Controller.anim.CrossFade("Idle", 0.5f, 2);
                    }
                    else
                    {
                        Controller.anim.Play("Take Weapon", 1);
                        Controller.anim.Play("Take Weapon", 2);
                    }
                }
                
                StartCoroutine(TakeWeapon(isGrenade && !WeaponController.Attacks[WeaponController.currentAttack].useTakeAnimation));
            }
            else
            {
                Controller.anim.Play("Idle", 1);
                Controller.anim.Play("Idle", 2);
            }

            Controller.anim.SetBool("CanWalkWithWeapon", true);

            Controller.speedDivider = 1 + 0.03f * WeaponController.weaponWeight;

            hasAnyWeapon = true;
            
            if (Controller.isRemoteCharacter)
                return;
            
#if USK_MULTIPLAYER
            if(Controller.CharacterSync && sendToNetwork)
                Controller.CharacterSync.ChangeWeapon(true);
#endif
            if (!isGrenade)
            {
                if (Controller.CameraController.isCameraAimEnabled && (Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !Controller.isAlwaysTpAimEnabled))
                    Controller.CameraController.Aim();
            }
        }

        private void ResetAnimatorParameters()
        {
            foreach (var parameter in Controller.anim.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                {
                    if (parameter.name == "Aim" || parameter.name == "TakeWeapon" || parameter.name == "Attack" || parameter.name == "Reload"
                        || parameter.name == "Pause" || parameter.name == "HasWeaponTaken" || parameter.name == "CanWalkWithWeapon" || parameter.name == "NoWeapons")
                    {
                        Controller.anim.SetBool(parameter.name, false); //parameter.name == "NoWeapons");
                    }
                }

            }
        }

        public void SetCrosshair()
        {
            if(!Controller.UIManager) return;
            
            if(Controller.AdjustmentScene || !WeaponController
#if USK_MULTIPLAYER
              || Controller.GetComponent<PhotonView>() && !Controller.GetComponent<PhotonView>().IsMine
#endif
              )
                return;
            
            var characterUI = Controller.UIManager.CharacterUI;

            if (characterUI.topCrosshairPart)
            {
                characterUI.topCrosshairPart.rectTransform.sizeDelta = new Vector2(WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize, WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize);
                characterUI.topCrosshairPart.sprite = WeaponController.Attacks[WeaponController.currentAttack].UpPart ? WeaponController.Attacks[WeaponController.currentAttack].UpPart : null;
            }

            if (characterUI.bottomCrosshairPart)
            {
                characterUI.bottomCrosshairPart.rectTransform.sizeDelta = new Vector2(WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize, WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize);
                characterUI.bottomCrosshairPart.sprite = WeaponController.Attacks[WeaponController.currentAttack].DownPart ? WeaponController.Attacks[WeaponController.currentAttack].DownPart : null;
            }

            if (characterUI.leftCrosshairPart)
            {
                characterUI.leftCrosshairPart.rectTransform.sizeDelta = new Vector2(WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize, WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize);
                characterUI.leftCrosshairPart.sprite = WeaponController.Attacks[WeaponController.currentAttack].LeftPart ? WeaponController.Attacks[WeaponController.currentAttack].LeftPart : null;
            }

            if (characterUI.rightCrosshairPart)
            {
                characterUI.rightCrosshairPart.rectTransform.sizeDelta = new Vector2(WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize, WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize);
                characterUI.rightCrosshairPart.sprite = WeaponController.Attacks[WeaponController.currentAttack].RightPart ? WeaponController.Attacks[WeaponController.currentAttack].RightPart : null;
            }

            if (characterUI.middleCrosshairPart)
            {
                characterUI.middleCrosshairPart.rectTransform.sizeDelta = new Vector2(WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize, WeaponController.Attacks[WeaponController.currentAttack].CrosshairSize);

                if (characterUI.middleCrosshairPart.gameObject.GetComponent<Outline>())
                    characterUI.middleCrosshairPart.gameObject.GetComponent<Outline>().enabled = true;
                
                if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
                {
                    if (Controller.CameraParameters.CursorImage)
                    {
                        characterUI.middleCrosshairPart.rectTransform.sizeDelta = new Vector2(70, 70);
                        characterUI.middleCrosshairPart.sprite = Controller.CameraParameters.CursorImage;
                    }
                }
                else
                {
                    characterUI.middleCrosshairPart.sprite = WeaponController.Attacks[WeaponController.currentAttack].MiddlePart ? WeaponController.Attacks[WeaponController.currentAttack].MiddlePart : null;
                }
            }

//            Controller.CameraController.rightPart.gameObject.SetActive(false);
//            Controller.CameraController.rightPart.gameObject.SetActive(true);
        }

        public void SetWeaponAnimations(bool changeAttack, CharacterHelper.CameraType cameraType)
        {
            if (WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets)
            {
                SetAnimation(WeaponController.Attacks[WeaponController.currentAttack].currentBulletType, "_WeaponAttack", cameraType);
            }
            else
            {
                if (WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                {
                    if (WeaponController.Attacks[WeaponController.currentAttack].tpAttacks[0])
                        Controller.ClipOverrides["_GrenadeFullBody"] = WeaponController.Attacks[WeaponController.currentAttack].tpAttacks[0];
                    
                    if (WeaponController.Attacks[WeaponController.currentAttack].tpCrouchAttacks[0])
                        Controller.ClipOverrides["_GrenadeCrouchFullBody"] = WeaponController.Attacks[WeaponController.currentAttack].tpCrouchAttacks[0];
                    
                    if (WeaponController.Attacks[WeaponController.currentAttack].fpAttacks[0])
                        Controller.ClipOverrides["_WeaponAttack"] = WeaponController.Attacks[WeaponController.currentAttack].fpAttacks[0];
                }
                else
                {
                    SetAnimation(0, "_WeaponAttack", cameraType);

                }
            }

            if (WeaponController.weaponAnimations.idle)
                Controller.ClipOverrides["_WeaponIdle"] = WeaponController.weaponAnimations.idle;

            if (WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee && WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
            {
                if (WeaponController.Attacks[WeaponController.currentAttack].reloadAnimation)
                    Controller.ClipOverrides["_WeaponReload"] = WeaponController.Attacks[WeaponController.currentAttack].reloadAnimation;
            }

            if (!changeAttack)
            {
                if (WeaponController.weaponAnimations.fpWalk)
                    Controller.ClipOverrides["_WeaponWalk"] = WeaponController.weaponAnimations.fpWalk;

                if (WeaponController.weaponAnimations.fpRun)
                    Controller.ClipOverrides["_WeaponRun"] = WeaponController.weaponAnimations.fpRun;

                if (WeaponController.weaponAnimations.take)
                    Controller.ClipOverrides["_TakeWeapon"] = WeaponController.weaponAnimations.take;
            }

            Controller.newController.ApplyOverrides(Controller.ClipOverrides);

            StartCoroutine(SetAnimParameters());
        }

        void SetAnimation(int animationIndex, string animationProperty, CharacterHelper.CameraType cameraType)
        {
            switch (cameraType)
            {
                case CharacterHelper.CameraType.FirstPerson:
                    if (WeaponController.Attacks[WeaponController.currentAttack].fpAttacks[animationIndex])
                        Controller.ClipOverrides[animationProperty] = WeaponController.Attacks[WeaponController.currentAttack].fpAttacks[animationIndex];
                    break;
                case CharacterHelper.CameraType.ThirdPerson:
                    if (WeaponController.Attacks[WeaponController.currentAttack].tpAttacks[animationIndex])
                        Controller.ClipOverrides[animationProperty] = WeaponController.Attacks[WeaponController.currentAttack].tpAttacks[animationIndex];
                    break;
                case CharacterHelper.CameraType.TopDown:
                    if (WeaponController.Attacks[WeaponController.currentAttack].tdAttacks[animationIndex])
                        Controller.ClipOverrides[animationProperty] = WeaponController.Attacks[WeaponController.currentAttack].tdAttacks[animationIndex];
                    break;
            }
        }

        private void InstantiateWeaponsAtStart()
        {
            hasAnyWeapon = false;

            if (Controller.AdjustmentScene)
            {
                return;
            }

            if (!gameObject.GetComponent<CharacterSync>() 
                
#if USK_MULTIPLAYER
                || FindObjectOfType<RoomManager>()
#endif
                
                )
            {
                if (SaveManager.Instance && SaveManager.Instance.saveInventory && SaveManager.Instance.HasAnyData(SaveManager.CharacterDataFileName))
                {
                    SaveManager.Instance.characterController = Controller;
                    SaveManager.Instance.LoadWeaponsData();
                }
                
                if (!hasAnyWeapon)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        foreach (var slot in slots[i].weaponSlotInInspector)
                        {
                            var weapon = slot.weapon;

                            if (!weapon && !slot.fistAttack) continue;
                            if (weapon && !weapon.GetComponent<WeaponController>()) continue;

                            if (weapon)
                            {
                                WeaponsHelper.InstantiateWeapon(weapon, i, this, Controller);
                            }
                            else if (slot.fistAttack)
                            {
                                slots[i].weaponSlotInGame.Add(new CharacterHelper.Weapon {fistAttack = true});
                                allWeaponsCount++;
                            }
                        }
                    }
                    
                    ActivateFirstWeapon();
                }
                else
                {
                    for (var i = 0; i < 8; i++)
                    {
                        foreach (var slot in slots[i].weaponSlotInInspector.Where(slot => slot.fistAttack))
                        {
                            slots[i].weaponSlotInGame.Add(new CharacterHelper.Weapon {fistAttack = true});
                            allWeaponsCount++;
                        }
                    }
                }
            }
#if USK_ADVANCED_MULTIPLAYER
            else
            {
                if (Controller.amGameData)
                {
                    for (var i = 0; i < Controller.amGameData.weaponSlots.Count; i++)
                    {
                        var weaponSlot = Controller.amGameData.weaponSlots[i];

                        if (weaponSlot.weapon)
                        {
                            if (Controller.amGameData.useAllWeapons)
                            {
                                if (gameObject.GetComponent<PhotonView>().IsMine)
                                {
                                    var weaponIndices = AMHelper.GetSelectedWeapons(Controller.amGameData.selectedWeaponsNetworkValue);
                                    
                                    if (weaponIndices.Contains(i))
                                        WeaponsHelper.InstantiateWeapon(weaponSlot.weapon.gameObject, weaponSlot.slot, this, Controller);
                                }
                                else
                                {
                                    var weaponIndices = AMHelper.GetSelectedWeapons((string) gameObject.GetComponent<CharacterSync>().photonView.Owner.CustomProperties["weaponIndexes"]);
                                    
                                    if (weaponIndices.Contains(i))
                                        WeaponsHelper.InstantiateWeapon(weaponSlot.weapon.gameObject, weaponSlot.slot, this, Controller);
                                }
                            }
                            else
                            {
                                WeaponsHelper.InstantiateWeapon(weaponSlot.weapon.gameObject, weaponSlot.slot, this, Controller);
                            }
                        }
                    }
                }

                for (var i = 0; i < 8; i++)
                {
                    foreach (var slot in slots[i].weaponSlotInInspector.Where(slot => slot.fistAttack))
                    {
                        slots[i].weaponSlotInGame.Add(new CharacterHelper.Weapon {fistAttack = true});
                    }
                }
                
                ActivateFirstWeapon();
            }
#endif
        }

        void FlashEffect()
        {
            if(Controller.isRemoteCharacter || !Controller.UIManager.CharacterUI.flashPlaceholder)
                return;

            var flashImage = Controller.UIManager.CharacterUI.flashPlaceholder;
            
            flashTimeout += Time.deltaTime;
            
            if (flashTimeout > 2)
            {
                if(flashImage.gameObject.activeSelf)
                    flashImage.color = new Color(1, 1, 1, Mathf.Lerp(flashImage.color.a, 0, 0.5f * Time.deltaTime));

                if (flashImage.color.a <= 0.01f && Controller.thisCamera.GetComponent<Motion>())
                {
                    flashImage.color = new Color(1, 1, 1, 0);
                    flashImage.gameObject.SetActive(false);

                    var motion = Controller.thisCamera.GetComponent<Motion>();

                    motion.frameBlending = 0;
                    motion.sampleCount = 0;

                    motion.shutterAngle = Mathf.Lerp(motion.frameBlending, 0, 5 * Time.deltaTime);

                    if (motion.frameBlending <= 0.1f)
                    {
                        Destroy(Controller.thisCamera.GetComponent<Motion>());
                    }
                }
            }
        }

        IEnumerator TakeWeapon(bool isGrenade)
        {
            var time = 5f;
            
            if (Controller.isCrouch || !Controller.isCrouch && (!WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState))
                time = WeaponController.weaponAnimations.take.length;

            else if (!Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState)
                time = WeaponController.weaponAnimations.take.length / 2;
            
            if(!isGrenade)
                yield return new WaitForSeconds(time);
            
            hasWeaponTaken = true;
            currentIKWeight = 0;
            WeaponController.canDrawGrenadesPath = true;
            
            if (Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState ||
                !Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState && (Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson))
            {
                firstLayerSet = true;
            }
            else
            {
                firstLayerSet = false;
            }
            
            Controller.anim.SetBool("HasWeaponTaken", false);
            StartCoroutine(ShootingTimeout()); 
        }

        IEnumerator DropTimeOut(CharacterHelper.Weapon curWeapon)
        {
            yield return new WaitForSeconds(1);
           
            var script = curWeapon.weapon.GetComponent<PickupItem>();
            script.enabled = true;
            script.rotationSpeed = 0;
            
            canDropWeapon = true;
            StopCoroutine("DropTimeOut");
        }

        IEnumerator ShootingTimeout() 
        {
            while (true)
            {
                if (WeaponController && Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
                {
                    WeaponController.canAttack = true;
                    StopCoroutine("ShootingTimeout");
                    break;
                }

                yield return 0;
            }
        }

        IEnumerator SetAnimParameters()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            Controller.anim.SetBool("TakeWeapon", true);
        }


        #region MobileUI

        public void UIAttack()
        {
            uiButtonAttack = true;
            
            if (WeaponController)
            {
                WeaponController.uiButtonAttack = true;
            }
        }

        public void UIEndAttack()
        {
            uiButtonAttack = false;
            
            if (WeaponController)
            {
                WeaponController.wasEmptyMagSoundPlayed = false;
                WeaponController.uiButtonAttack = false;
            }
        }

        public void UIAim()
        {
            if (slots[currentSlot].weaponSlotInGame.Count > 0 && slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
            {
                Aim();
            }
            else if (WeaponController)
            {
                WeaponController.Aim(false, false, false);
            }
        }

        public void UIReload()
        {
            if(WeaponController)
                WeaponController.Reload();
        }

        public void UIChangeAttackType()
        {
            if(WeaponController)
                WeaponController.ChangeAttack();
        }

        public void UIPickUp()
        {
            pickUpUiButton = true;
        }

        public void UIInventory()
        {
            if (!Controller.UIManager.CharacterUI.Inventory.MainObject) return;

            if (inventoryIsOpened)
            {
                DeactivateInventory();
            }
            else
            {
                ActivateInventory();
            }
        }

        public void UIActivateInventory()
        {
            pressedUIInventoryButton = true;
        }

        public void UIDeactivateInventory()
        {
            pressedUIInventoryButton = false;
        }

        #endregion

        #region AnimationEvents

        public void AddBullet()
        {
            if(!WeaponController) return;

            WeaponController.Attacks[WeaponController.currentAttack].curAmmo += 1;
        }
        
        public void ChangeMagazineVisibility(string value)
        {
            if(!WeaponController.Attacks[WeaponController.currentAttack].magazine)
                return;
            
            switch (value)
            {
                case "show":
                    WeaponController.Attacks[WeaponController.currentAttack].magazine.SetActive(true);
                    break;
                case "hide":
                    WeaponController.Attacks[WeaponController.currentAttack].magazine.SetActive(false);
                    break;
                case "hideAndCreate":
                    WeaponController.HideAndCreateNewMagazine();
                    break;
            }
        }

        public void DropMagazine()
        {
            foreach (var magazine in WeaponController.Attacks[WeaponController.currentAttack].TempMagazine)
            {
                if (magazine)
                {
                    var tempMag = magazine;
                    tempMag.transform.parent = null;
                    tempMag.AddComponent<Rigidbody>();
                    tempMag.AddComponent<DestroyObject>().destroyTime = 10;
                }   
            }
            
            WeaponController.Attacks[WeaponController.currentAttack].TempMagazine.Clear();
        }

        public void SpawnShell()
        {
            if (WeaponController.Attacks[WeaponController.currentAttack].Shell && WeaponController.Attacks[WeaponController.currentAttack].ShellPoint)
            {
                var _shell = Instantiate(WeaponController.Attacks[WeaponController.currentAttack].Shell, WeaponController.Attacks[WeaponController.currentAttack].ShellPoint.position, WeaponController.Attacks[WeaponController.currentAttack].ShellPoint.localRotation);
                Helper.ChangeLayersRecursively(_shell.transform, "Character");
                _shell.hideFlags = HideFlags.HideInHierarchy;
                _shell.gameObject.AddComponent<ShellControll>().ShellPoint = WeaponController.Attacks[WeaponController.currentAttack].ShellPoint;
            }
        }

        public void PlayAttackSound()
        {
            if (slots[currentSlot].weaponSlotInGame.Count > 0 && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
            {
                if (WeaponController.Attacks[WeaponController.currentAttack].AttackAudio)
                {
                    WeaponController.GetComponent<AudioSource>().loop = false;
                    WeaponController.GetComponent<AudioSource>().PlayOneShot(WeaponController.Attacks[WeaponController.currentAttack].AttackAudio);
                }
            }
            else
            {
                if (fistAttackAudio && GetComponent<AudioSource>())
                    GetComponent<AudioSource>().PlayOneShot(fistAttackAudio);
            }
        }

        public void ChangeParent(string side)
        {
            switch (side)
            {
                case "left":
                    currentWeapon.transform.parent = Controller.BodyObjects.LeftHand;
                    break;
                case "right":
                    currentWeapon.transform.parent = Controller.BodyObjects.RightHand;
                    break;
                case "rightAndPlace":
                    currentWeapon.transform.parent = Controller.BodyObjects.RightHand;
                    // StopCoroutine("SetWeaponAnimations");
                    StartCoroutine("SetWeaponPosition");
                    break;
            }
        }
        
        IEnumerator SetWeaponPosition()
        {
            while (true)
            {
                currentWeapon.transform.localPosition =  Vector3.MoveTowards(currentWeapon.transform.localPosition, WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponPosition, (!WeaponController.canceledReload ? 0.5f : 20) * Time.deltaTime);
                currentWeapon.transform.localRotation = Quaternion.Slerp(currentWeapon.transform.localRotation, Quaternion.Euler(WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponRotation), (!WeaponController.canceledReload ? 10 : 20) * Time.deltaTime);

                if (Helper.ReachedPositionAndRotation(currentWeapon.transform.localPosition, WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponPosition,
                    currentWeapon.transform.localEulerAngles, WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponRotation))
                {
                    currentWeapon.transform.localPosition = WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponPosition;
                    currentWeapon.transform.localEulerAngles = WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].WeaponRotation;
                    // StopCoroutine("SetWeaponAnimations");
                    break;
                }
                
                yield return 0;
            }
        }

        public void LaunchGrenade()
        {
            WeaponController.StopCoroutine("FlyGrenade");
            WeaponController.LaunchGrenade();
        }

        #endregion
        
        public IEnumerator TakeGrenade(bool fullBody)
        {
            yield return new WaitForSeconds(!fullBody ? WeaponController.Attacks[WeaponController.currentAttack].fpAttacks[0].length : WeaponController.Attacks[WeaponController.currentAttack].tpAttacks[0].length);
            TakeNewGreande();
        }

        public void TakeNewGreande()
        {
            Controller.anim.SetBool("Pause", false);
            Controller.anim.SetBool("LaunchGrenade", false);

            if (!Controller.isRemoteCharacter)
            {
                if (WeaponController.Attacks[WeaponController.currentAttack].curAmmo > 0)
                {
                    ActivateWeapon(currentSlot, true, true);
                }
                else
                {
                    WeaponUp();
                }
            }
        }
        
        #region HandsIK

        private void OnAnimatorIK(int layerIndex)
        {
            if(inMultiplayerLobby) return;
            
            if (WeaponController && hasAnyWeapon)
            {
                if (WeaponController.isReloadEnabled)
                {
                    Helper.FingersRotate(null, Controller.anim, "Null");
                }
                else
                {
                    Helper.FingersRotate(WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex], Controller.anim, "Weapon");
                }

                if (!WeaponController.ActiveDebug)
                {
                    var disableIK = slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack && !inventoryIsOpened ||
                                    WeaponController.isReloadEnabled ||
                                     Controller.anim.GetBool("Pause") || 
                                    !WeaponController.isAimEnabled && (!Controller.isAlwaysTpAimEnabled || WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee) && (!Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState && (Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson || WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee) ||
                                                                                                                                                                                                               Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState);
                    
                    if(disableIK)
                    {
                        if (currentIKWeight > 0)
                        {
                            currentIKWeight -= 1 * Time.deltaTime;
                        }
                        else
                        {
                            currentIKWeight = 0;

                            Controller.anim.SetBool("HasWeaponTaken",
                                !Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState && (Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                                ||  Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState);
                        }
                    }
                    else
                    {
                        if (currentIKWeight < (!Controller.anim.GetBool("HasWeaponTaken") && Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson ? 2 : 1))
                        {
                            var cantIncrease = Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.isCrouch && !Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Idle") &&
                                               !Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Aim_Idle") && !Controller.anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch_Walk_Forward");

                            if (!cantIncrease)
                                currentIKWeight += (!WeaponController.canceledReload ? 1 : 5) * Time.deltaTime;
                        }
                        else
                        {
                            if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                            {
                                if (currentIKWeight > 1.9)
                                {
                                    Controller.anim.SetBool("HasWeaponTaken", true);
                                }
                            }
                            else 
                            {
                                Controller.anim.SetBool("HasWeaponTaken", true);
                            }

                            WeaponController.canceledReload = false;
                            firstLayerSet = true;
                            currentIKWeight = 1;
                        }
                    }
                    
                    
                    if (!Controller.isRemoteCharacter)
                    {
                        if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !WeaponController.isReloadEnabled && (WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState ||
                                                                                                                                       WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState))
                        {
                            if (setWeaponLayer && firstLayerSet)
                            {
                                Controller.anim.SetLayerWeight(2, currentIKWeight);

                                if (currentIKWeight <= 0)
                                {
                                    if (WeaponController && !WeaponController.setHandsPositionsAim && !WeaponController.isAimEnabled && (!Controller.isAlwaysTpAimEnabled || WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee))
                                    {
                                        WeaponController.setHandsPositionsAim = true;
                                    }
                                }
                            }
                        }
                        else if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown)
                        {
                            Controller.anim.SetLayerWeight(2, 1);
                        }


                        if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson &&
                            !Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState ||
                            Controller.isCrouch && WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState)
                        {
                            if (WeaponController.isReloadEnabled || !hasWeaponTaken)
                            {
                                setWeaponLayer = false;

                                Controller.anim.SetLayerWeight(2, Mathf.Lerp(Controller.anim.GetLayerWeight(2), 1, 3 * Time.deltaTime));
                            }
                            else if (!WeaponController.isReloadEnabled && !setWeaponLayer && hasWeaponTaken)
                            {
                                if (!WeaponController.isAimEnabled && (!Controller.isAlwaysTpAimEnabled || WeaponController.Attacks[WeaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee))
                                {
                                    Controller.anim.SetLayerWeight(2, Mathf.Lerp(Controller.anim.GetLayerWeight(2), 0, 3 * Time.deltaTime));

                                    if (Math.Abs(Controller.anim.GetLayerWeight(2)) < 0.05f)//!!
                                    {
                                        Controller.anim.SetLayerWeight(2, 0);
                                        setWeaponLayer = true;
                                    }
                                    
                                }
                                else
                                {
                                    if (currentIKWeight > 0.9f)
                                        setWeaponLayer = true;
                                }
                            }
                        }

                        if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson && !slots[currentSlot].weaponSlotInGame[slots[currentSlot].currentWeaponInSlot].fistAttack)
                        {
                            Controller.anim.SetLayerWeight(3, Mathf.Abs(Controller.anim.GetLayerWeight(2) - 1));
                        }
                        else
                        {
                            Controller.anim.SetLayerWeight(3, 0);
                        }
                    }
                    
                    if (layerIndex == Controller.currentAnimatorLayer)
                    {
                        if (WeaponController.IkObjects.RightObject && WeaponController.IkObjects.LeftObject)
                        {
                            if (WeaponController.CanUseIK && hasWeaponTaken)
                            {
                                Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftObject, WeaponController.IkObjects.RightObject, Controller.BodyObjects.LeftHand, Controller.BodyObjects.RightHand, currentIKWeight, true);
                                
                                if((WeaponController.isAimEnabled || Controller.isAlwaysTpAimEnabled) && WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade && WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                {
                                    if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson) //&& WeaponController.Attacks[WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                    {
                                        currentHeadIKWeight = Mathf.Lerp(currentHeadIKWeight, 1, 0.7f * Time.deltaTime);
                                        Controller.anim.SetLookAtWeight(currentHeadIKWeight);
                                    }
                                }
                                else
                                {
                                    currentHeadIKWeight = Mathf.Lerp(currentHeadIKWeight, 0, 5 * Time.deltaTime);
                                    Controller.anim.SetLookAtWeight(currentHeadIKWeight);
                                }
                                
                                Controller.anim.SetLookAtPosition(WeaponController.transform.position + Controller.DirectionObject.forward * 3);
                            }
                        }
                    }
                }
                else if (WeaponController.ActiveDebug && WeaponController.canUseValuesInAdjustment)
                {
                    if (!WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableElbowIK)
                    {
                        Controller.anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                        Controller.anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                    }
                    else
                    {
                        Controller.anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
                        Controller.anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                    }
                    
                    Controller.anim.SetIKHintPosition(AvatarIKHint.LeftElbow, WeaponController.IkObjects.LeftElbowObject.position);
                    Controller.anim.SetIKHintPosition(AvatarIKHint.RightElbow, WeaponController.IkObjects.RightElbowObject.position);

                    switch (WeaponController.DebugMode)
                    {
                        case IKHelper.IkDebugMode.Aim:
                            
                            WeaponController.hasAimIKChanged = true;
                            
                            if(Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                Controller.anim.SetLayerWeight(2, 1);
                            
                            Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftAimObject,
                                WeaponController.IkObjects.RightAimObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, debugIKValue, WeaponController.pinLeftObject);
                            
                            break;
                        case IKHelper.IkDebugMode.ObjectDetection:
                            WeaponController.hasWallIKChanged = true;
                            
                            if(Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                Controller.anim.SetLayerWeight(2, 1);
                            
                            Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftWallObject, WeaponController.IkObjects.RightWallObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, debugIKValue, WeaponController.pinLeftObject);
                            
                            break;
                        case IKHelper.IkDebugMode.Norm:
                            if (!WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInNormalState)
                            {
                                if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                {
                                    Controller.anim.SetLayerWeight(2, 1);
                                    Controller.anim.SetLayerWeight(3, 0);
                                }
                                
                                Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftObject, WeaponController.IkObjects.RightObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, debugIKValue, WeaponController.pinLeftObject);
                            }
                            else
                            {
                                Controller.anim.SetLayerWeight(2, 0);

                                if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                {
                                    Controller.anim.SetLayerWeight(3, 1);
                                }

                                Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftObject,
                                    WeaponController.IkObjects.RightObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, 0, WeaponController.pinLeftObject);
                            }
                            break;
                        case IKHelper.IkDebugMode.Crouch:
                            if (!WeaponController.CurrentWeaponInfo[WeaponController.settingsSlotIndex].disableIkInCrouchState)
                            {
                                if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                {
                                    Controller.anim.SetLayerWeight(3, 0);
                                    Controller.anim.SetLayerWeight(2, 1);
                                }
                                
                                WeaponController.hasCrouchIKChanged = true;
                                
                                Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftCrouchObject,
                                    WeaponController.IkObjects.RightCrouchObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, debugIKValue, WeaponController.pinLeftObject);
                            }
                            else
                            {
                                Controller.anim.SetLayerWeight(2, 0);

                                if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                                {
                                    Controller.anim.SetLayerWeight(3, 1);
                                }

                                Helper.HandsIK(Controller, WeaponController, this, WeaponController.IkObjects.LeftCrouchObject,
                                    WeaponController.IkObjects.RightCrouchObject, Controller.BodyObjects.TopBody, Controller.BodyObjects.TopBody, 0, WeaponController.pinLeftObject);
                            }

                            break;
                    }
                }
            }
            else
            {
                Helper.FingersRotate(null, Controller.anim, "Null");
            }
        }
        #endregion
    }
}





