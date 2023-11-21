using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if USK_MULTIPLAYER
using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif


namespace GercStudio.USK.Scripts
{
    public class CharacterSync :
#if USK_MULTIPLAYER
        MonoBehaviourPun, IPunObservable
#else
    MonoBehaviour
#endif
    {
#if USK_MULTIPLAYER
        [HideInInspector] public EventsManager eventsManager;

       [HideInInspector] public List<Helper.ActorID> opponentsWhoAttackedPlayer = new List<Helper.ActorID>();

       [HideInInspector] public Controller controller;
        private InventoryManager weaponManager;
        private WeaponController weaponController;

#if USK_MULTIPLAYER
        private RoomManager roomManager;
        private LobbyManager lobbyManager;
#endif

#if USK_ADVANCED_MULTIPLAYER
        [HideInInspector] public AdvancedRoomManager advancedRoomManager;
        private AdvancedLobbyManager advancedLobbyManager;
#endif

        private GameObject grenade;

        private float camera_Distance;
        private float camera_Angle;

        private Vector3 camera_Direction;
        private Vector3 camera_NetworkPosition;
        private Vector3 camera_StoredPosition;

        private Quaternion camera_NetworkRotation;

        private Vector3 CameraPosition;
        private Vector3 CameraRotation;

        private float destroyTimeOut;

        private bool hasTimerStarted;
        private bool fireAttackEventWasSent;
        private bool firstTake;
        private bool opponent;
        private bool canUseMinimap;


        #region StartMethods

        private void OnEnable()
        {

           if(lobbyManager)
               return;
           
            firstTake = true;
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }

        private void Awake()
        {
            controller = GetComponent<Controller>();
            controller.enabled = true;
            
            if (FindObjectOfType<LobbyManager>())
            {
                lobbyManager = FindObjectOfType<LobbyManager>();
                
                if (controller.multiplayerStatsBackground)
                    controller.multiplayerStatsBackground.SetActive(false);

                if (controller.healthBarImage)
                    controller.healthBarImage.gameObject.SetActive(false);
                
                if(controller.nickNameText)
                    controller.nickNameText.gameObject.SetActive(false);

                return;
            }

#if USK_ADVANCED_MULTIPLAYER
            if (FindObjectOfType<AdvancedLobbyManager>())
            {
                advancedLobbyManager = FindObjectOfType<AdvancedLobbyManager>();
                
                if (controller.multiplayerStatsBackground)
                    controller.multiplayerStatsBackground.SetActive(false);

                if (controller.healthBarImage)
                    controller.healthBarImage.gameObject.SetActive(false);

                if (controller.nickNameText)
                    controller.nickNameText.gameObject.SetActive(false);

                return;
            }

            advancedRoomManager = FindObjectOfType<AdvancedRoomManager>();
#endif

            roomManager = FindObjectOfType<RoomManager>();
            
            controller.CharacterSync = this;
            eventsManager = FindObjectOfType<EventsManager>();
            weaponManager = GetComponent<InventoryManager>();
            weaponManager.enabled = true;

            controller.CameraController.BodyLookAt = new GameObject("BodyLookAt").transform;
            controller.CameraController.BodyLookAt.hideFlags = HideFlags.HideInHierarchy;

            if (!photonView.IsMine && PhotonNetwork.IsConnected)
            {
                controller.isRemoteCharacter = true;
            }
            else if (photonView.IsMine && PhotonNetwork.IsConnected)
            {

            }
        }

        void Start()
        {
            if (lobbyManager)
                return;

#if USK_ADVANCED_MULTIPLAYER
                if (advancedLobbyManager)
                    return;

                if (advancedRoomManager)
                {
                    controller.oneShotOneKill = (bool) PhotonNetwork.CurrentRoom.CustomProperties["oneShot"];
                    controller.multiplayerTeam = (MultiplayerHelper.Teams) photonView.Controller.CustomProperties["pt"];
                    controller.canKillOthers = (MultiplayerHelper.CanKillOthers) PhotonNetwork.CurrentRoom.CustomProperties["km"];
                } 
                else
#endif
            if (roomManager)
            {
                controller.canKillOthers = (MultiplayerHelper.CanKillOthers) PhotonNetwork.CurrentRoom.CustomProperties["km"];
                controller.multiplayerTeam = MultiplayerHelper.Teams.Null;
            }


            if (PhotonNetwork.InRoom)
            {
                controller.multiplayerNickname = photonView.Owner.NickName;
                CharacterHelper.SetAnimatorViewComponents(GetComponent<PhotonAnimatorView>());
            }

            if (!photonView.IsMine && PhotonNetwork.IsConnected)
            {
                controller.thisCamera.SetActive(false);

                // if (photonView.Owner.CustomProperties.ContainsKey("plh"))
                // {
                //     controller.health = (float) photonView.Owner.CustomProperties["plh"];
                // }

                if (weaponManager.LeftHandCollider)
                    weaponManager.LeftHandCollider.enabled = false;

                if (weaponManager.RightHandCollider)
                    weaponManager.RightHandCollider.enabled = false;

                Helper.ChangeLayersRecursively(gameObject.transform, "Default");

                if (!controller.CameraController.layerCamera)
                {
                    controller.CameraController.layerCamera = Helper.NewCamera("LayerCamera", transform, "Sync");
                    controller.CameraController.layerCamera.gameObject.SetActive(false);
                }

                StartCoroutine(EnableHealthBarTimeout());

            }
            else
            {
                if (Application.isMobilePlatform || controller.projectSettings.mobileDebug)
                {
                    if (roomManager)
                        controller.UIManager.uiButtons[9].onClick.AddListener(delegate { roomManager.Pause(true); });
#if USK_ADVANCED_MULTIPLAYER
                        else if (advancedRoomManager)
                            controller.UIManager.uiButtons[9].onClick.AddListener(delegate { advancedRoomManager.Pause(true); });
#endif
                }

                if (controller.healthBarImage && controller.multiplayerStatsBackground)
                {
                    controller.healthBarImage.gameObject.SetActive(false);
                    controller.multiplayerStatsBackground.SetActive(false);
                }

                if(controller.nickNameText)
                    controller.nickNameText.gameObject.SetActive(false);
                
                // PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"plh", controller.health}, {"id", photonView.ViewID}});

                controller.thisCamera.SetActive(true);
            }

            // currentHealth = controller.health;
        }

        IEnumerator EnableHealthBarTimeout()
        {
            yield return new WaitForSeconds(1);

            if (controller.nickNameText)
            {
                controller.nickNameText.gameObject.SetActive(true);
                controller.nickNameText.text = controller.multiplayerNickname;
            }
            
            
            if (controller.healthBarImage && controller.multiplayerStatsBackground)
            {
                if (roomManager 
#if USK_ADVANCED_MULTIPLAYER 
                    || advancedRoomManager
#endif
                    
                    ) 
                {
                    controller.healthBarImage.gameObject.SetActive(true);
                    controller.multiplayerStatsBackground.SetActive(true);

                    if (roomManager)
                    {
                        controller.healthBarImage.color = controller.canKillOthers == MultiplayerHelper.CanKillOthers.Everyone ? controller.opponentColor : controller.teammateColor;
                    }
#if USK_ADVANCED_MULTIPLAYER
                    else if (advancedRoomManager)
                    {
                        if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.WithoutTarget)
                            controller.healthBarImage.color = controller.teammateColor;
                        else
                        {
                            if ((MultiplayerHelper.Teams) PhotonNetwork.LocalPlayer.CustomProperties["pt"] != controller.multiplayerTeam || (MultiplayerHelper.Teams) PhotonNetwork.LocalPlayer.CustomProperties["pt"] == controller.multiplayerTeam && controller.multiplayerTeam == MultiplayerHelper.Teams.Null)
                            {
                                controller.healthBarImage.color = controller.opponentColor;
                            }
                            else
                            {
                                controller.healthBarImage.color = controller.teammateColor;
                            }
                        }
                    }
#endif
                }
                else
                {
                    // controller.healthBarImage.gameObject.SetActive(false);
                    // controller.multiplayerStatsBackground.SetActive(false);
                }
            }

            if (controller.blipRawImage && controller.blipRawImage.texture)
            {
                if ((MultiplayerHelper.Teams) PhotonNetwork.LocalPlayer.CustomProperties["pt"] != controller.multiplayerTeam || (MultiplayerHelper.Teams) PhotonNetwork.LocalPlayer.CustomProperties["pt"] == controller.multiplayerTeam && controller.multiplayerTeam == MultiplayerHelper.Teams.Null && (MultiplayerHelper.CanKillOthers) PhotonNetwork.CurrentRoom.CustomProperties["km"] != MultiplayerHelper.CanKillOthers.NoOne)
                {
                    controller.opponentColor.a = 0;
                    controller.blipRawImage.color = controller.opponentColor;
                    opponent = true;
                }
                else
                {
                    controller.opponentColor.a = 1;
                    controller.blipRawImage.color = controller.teammateColor;
                }

                canUseMinimap = true;
            }

            StopCoroutine(EnableHealthBarTimeout());
        }

        #endregion

        #region UpdateSynchElements

        void FixedUpdate()
        {
            if (lobbyManager)
                return;

#if USK_ADVANCED_MULTIPLAYER
            if (advancedLobbyManager)
                return;
#endif

            weaponController = weaponManager.WeaponController;

            // if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            // {
                // if (Mathf.Abs(currentHealth - controller.health) > 0.1f)
                // {
                //     var options = new RaiseEventOptions
                //     {
                //         CachingOption = EventCaching.DoNotCache,
                //         Receivers = ReceiverGroup.Others
                //     };
                //     object[] content =
                //     {
                //         photonView.ViewID, controller.health, controller.KillerName
                //     };
                //     PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ChangeHealth, content, options, SendOptions.SendReliable);
                //     currentHealth = controller.health;
                // }

//                controller.thisCamera.transform.position = CameraPosition;
//                controller.thisCamera.transform.eulerAngles = CameraRotation;
            // }
            // else 
            if (photonView.IsMine && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
#if USK_ADVANCED_MULTIPLAYER
                if (advancedRoomManager)
                {
                    if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Domination)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(Vector3.Distance(advancedRoomManager.aPoint.transform.position, transform.position) < advancedRoomManager.aPoint.radius ? new Hashtable {{"ac", true}} : new Hashtable {{"ac", false}});
                        PhotonNetwork.LocalPlayer.SetCustomProperties(Vector3.Distance(advancedRoomManager.bPoint.transform.position, transform.position) < advancedRoomManager.bPoint.radius ? new Hashtable {{"bc", true}} : new Hashtable {{"bc", false}});
                        PhotonNetwork.LocalPlayer.SetCustomProperties(Vector3.Distance(advancedRoomManager.cPoint.transform.position, transform.position) < advancedRoomManager.cPoint.radius ? new Hashtable {{"cc", true}} : new Hashtable {{"cc", false}});
                        
                        if (advancedRoomManager.currentHardPoint)
                        {
                            if (Mathf.Abs((transform.position - advancedRoomManager.currentHardPoint.transform.position).x) < advancedRoomManager.currentHardPoint.size.x / 2 &&
                                // Mathf.Abs((transform.position - advancedRoomManager.CurrentHardPoint.transform.position).y) < advancedRoomManager.CurrentHardPoint.size.y / 2 &&
                                Mathf.Abs((transform.position - advancedRoomManager.currentHardPoint.transform.position).z) < advancedRoomManager.currentHardPoint.size.y / 2)
                            {
                                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"hpc", true}});
                            }
                            else
                            {
                                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"hpc", false}});
                            }
                        }
                    }
                }
#endif
            }
        }

        void Update()
        {
            if (lobbyManager)
                return;

#if USK_ADVANCED_MULTIPLAYER
            if (advancedLobbyManager)
                return;
#endif
            if (!photonView.IsMine)
            {
                controller.thisCamera.transform.position = Vector3.Lerp(controller.thisCamera.transform.position, CameraPosition, 5);
                controller.thisCamera.transform.eulerAngles = Vector3.Lerp(controller.thisCamera.transform.eulerAngles, CameraRotation, 5);

                if (controller.blipRawImage && canUseMinimap && controller.health > 0)
                {
                    var color = controller.blipRawImage.color;

                    if (opponent)
                    {
                        if (!controller.anim.GetBool("Attack"))
                        {
                            color.a -= Time.deltaTime * 2;
                            if (color.a < 0) color.a = 0;
                        }
                        else
                        {
                            color.a += Time.deltaTime * 8;
                            if (color.a > 1) color.a = 1;
                        }
                    }
                    else
                    {
                        if (color.a < 1) color.a = 1;
                    }

                    controller.blipRawImage.color = color;
                }
            }
        }

        void LateUpdate()
        {
            if (lobbyManager)
                return;

#if USK_ADVANCED_MULTIPLAYER
            if (advancedLobbyManager)
                return;
#endif
            if (!photonView.IsMine && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
                {
                    if (controller.CameraController.BodyLookAt)
                        controller.BodyLookAt(controller.CameraController.BodyLookAt);
                }
                else
                {
                    controller.BodyObjects.TopBody.localEulerAngles = controller.BodyLocalEulerAngles;
                }

                if (roomManager && roomManager.controller && roomManager.controller.thisCamera)
                {
                    if (controller.multiplayerStatsBackground)
                            controller.multiplayerStatsBackground.transform.LookAt(roomManager.controller.thisCamera.transform);
                        
                    if(controller.nickNameText)
                            controller.nickNameText.transform.LookAt(roomManager.controller.thisCamera.transform);
                }

#if USK_ADVANCED_MULTIPLAYER
                else if (advancedRoomManager && advancedRoomManager.controller && advancedRoomManager.controller.thisCamera)
                {
                        if (controller.multiplayerStatsBackground)
                            controller.multiplayerStatsBackground.transform.LookAt(advancedRoomManager.controller.thisCamera.transform);
                        
                        if(controller.nickNameText)
                            controller.nickNameText.transform.LookAt(advancedRoomManager.controller.thisCamera.transform);
                }
#endif

                if (weaponController)
                {
                    if (!weaponController.isMultiplayerWeapon)
                        weaponController.isMultiplayerWeapon = true;

                    if (weaponController.gameObject.layer == LayerMask.NameToLayer("Character"))
                        Helper.ChangeLayersRecursively(weaponController.transform, "MultiplayerCharacter");
                }

                if (weaponController && weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame && weaponController.attackAudioPlay)
                {
                    foreach (var _effect in from effect in weaponController.Attacks[weaponController.currentAttack].attackEffects where effect select Instantiate(effect, weaponController.Attacks[weaponController.currentAttack].AttackSpawnPoint.position, weaponController.Attacks[weaponController.currentAttack].AttackSpawnPoint.rotation))
                    {
                        _effect.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
            }
            else if (photonView.IsMine & PhotonNetwork.IsConnected & PhotonNetwork.InRoom)
            {
                if (weaponController)
                {
                    if (weaponController.isMultiplayerWeapon)
                        weaponController.isMultiplayerWeapon = false;
                }
            }
            
            if (controller.healthBarImage)
                controller.healthBarImage.fillAmount = controller.health / controller.healthPercent;
        }

        #endregion

        public void ChangeCameraType(CharacterHelper.CameraType cameraType)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            RaiseEventSender(cameraType, MultiplayerHelper.PhotonEventCodes.ChangeCameraType, options);
        }
        
        public void CrouchState()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            RaiseEventSender(null, MultiplayerHelper.PhotonEventCodes.Crouch, options);
        }

        public void FireAttack(bool isEnabled)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            
            if (isEnabled && !fireAttackEventWasSent)
            {
                RaiseEventSender(true, MultiplayerHelper.PhotonEventCodes.Fire, options);
                fireAttackEventWasSent = true;
            }
            else if(!isEnabled && fireAttackEventWasSent)
            {
                RaiseEventSender(false, MultiplayerHelper.PhotonEventCodes.Fire, options);
                fireAttackEventWasSent = false;
            }
        }

        public void BulletsShooting()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                controller.thisCamera.transform.position,
                controller.thisCamera.transform.rotation
            };
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.Bullets, content, options, SendOptions.SendReliable);
        }
        
        public void ChangeWeaponAttack()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            RaiseEventSender(null, MultiplayerHelper.PhotonEventCodes.ChangeAttack, options);
        }

        public void PickUp()
        {
            var room = FindObjectOfType<EventsManager>();
            if (room) room.PickUp(photonView.ViewID, weaponManager.currentPickUpId);
            else Debug.LogError("You must add the [PickUp Manger] script to the scene.");
        }

        public void ChangeMovementType(string type)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            
            RaiseEventSender(type, MultiplayerHelper.PhotonEventCodes.ChangeMovementType, options);
        }

        public void ReloadWeapon(bool enableReload)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            RaiseEventSender(enableReload, MultiplayerHelper.PhotonEventCodes.Reload, options);
        }

        public void LaunchRocket()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                weaponController.hit.point,
                controller.thisCamera.transform.position,
                controller.thisCamera.transform.rotation
            };
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.Rocket, content, options, SendOptions.SendReliable);
        }

        public void DropWeapon(bool getNewWeapon)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };

            object[] content =
            {
                photonView.ViewID,
                weaponManager.DropIdMultiplayer,
                weaponManager.DropDirection,
                getNewWeapon
            };
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.DropWeapon, content, options, SendOptions.SendReliable);
        }

        public void ThrowGrenade(bool fullBody)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                controller.thisCamera.transform.position,
                controller.thisCamera.transform.rotation,
                fullBody
            };
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.Grenade, content, options, SendOptions.SendReliable);
        }

        public void ChangeWeapon(bool hasWeapon)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                weaponManager.currentSlot,
                weaponManager.slots[weaponManager.currentSlot].currentWeaponInSlot,
                hasWeapon
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ChangeWeapon, content, options, SendOptions.SendReliable);
        }

        public void MeleeAttack(bool value, int animationIndex, int crouchAnimationIndex)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                value,
                animationIndex,
                crouchAnimationIndex
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.MeleeAttack, content, options, SendOptions.SendReliable);
        }

        public void MeleeAttack(bool value, int animationIndex)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                photonView.ViewID,
                value,
                animationIndex
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.MeleeAttack, content, options, SendOptions.SendReliable);
        }

        public void Aim()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            RaiseEventSender(null, MultiplayerHelper.PhotonEventCodes.Aim, options);
        }

        public void UseHealthKit()
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };

            object[] content = {photonView.ViewID, controller.health};

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ChangeHealth, content, options, SendOptions.SendReliable);
        }

        public void TakingDamage(float damage, Helper.ActorID attackerID, string attackType)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            
            object[] content =
            {
                photonView.ViewID,
                damage,
                attackerID.actorID,
                attackerID.type,
                attackType,
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.DamagePlayer, content, options, SendOptions.SendReliable);
        }

        public void SendCurrentHealth(int targetID)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                TargetActors = new [] {targetID}
            };

            object[] content =
            {
                photonView.ViewID,
                controller.health
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ChangeHealth, content, options, SendOptions.SendReliable);

        }

        public void CreateHitMark(int id)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            RaiseEventSender(id, MultiplayerHelper.PhotonEventCodes.CreateHitMark, options);
        }

        public void CreateHitMark(Vector3 startPosition)
        {
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            RaiseEventSender(startPosition, MultiplayerHelper.PhotonEventCodes.CreateHitMark, options);
        }

        // public void UpdateWeaponsManually(string type)
        // {
        //     var options = new RaiseEventOptions
        //     {
        //         CachingOption = EventCaching.AddToRoomCacheGlobal,
        //         Receivers = ReceiverGroup.Others
        //     };
        //     
        //     if (type == "clear")
        //     {
        //         object[] content =
        //         {
        //             photonView.ViewID,
        //         };
        //         
        //         PhotonNetwork.RaiseEvent((byte) PUNHelper.PhotonEventCodes.UpdateWeaponsFromScript, content, options, SendOptions.SendReliable);
        //
        //     }
        //     else if (type == "add")
        //     {
        //         object[] content =
        //         {
        //             photonView.ViewID,
        //             weaponManager.currentSlot,
        //             weaponManager.slots[weaponManager.currentSlot].currentWeaponInSlot
        //         };
        //         
        //         PhotonNetwork.RaiseEvent((byte) PUNHelper.PhotonEventCodes.UpdateWeaponsFromScript, content, options, SendOptions.SendReliable);
        //     }
        // }


        //set different parameters when player dies, then send event to other clients
        public void Destroy(Helper.ActorID killerActorID, string attackType)
        {
            // var playersList = PhotonNetwork.PlayerList;
            var customValues = new Hashtable();

#if USK_ADVANCED_MULTIPLAYER
            if (advancedRoomManager)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"ac", false}});
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"bc", false}});
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"cc", false}});
            }
#endif
            
            //update player death count
            var deaths = (int) photonView.Owner.CustomProperties["d"] + 1;
            customValues.Add("d", deaths);
            photonView.Owner.SetCustomProperties(customValues);
            //

            if (roomManager)
            {
                roomManager.StartCoroutine(roomManager.RestartGame());

                roomManager.currentUIManager.basicMultiplayerGameRoom.PauseMenu.DisableAll();
                roomManager.currentUIManager.CharacterUI.DisableAll();
                roomManager.currentUIManager.DisableAllBlips();
            }

#if USK_ADVANCED_MULTIPLAYER
            else if (advancedRoomManager)
            {
                if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Survive)
                {
                    AMHelper.CalculatePlaceInSurvivalMode(advancedRoomManager);
                    // CalculatePlayerPlace();
                    // advancedRoomManager.deadPlayersAndBots.Add(PhotonNetwork.LocalPlayer);
                }

                AMHelper.UpdatePlayersList(advancedRoomManager, controller);

                advancedRoomManager.currentUIManager.CharacterUI.DisableAll();
                advancedRoomManager.currentUIManager.DisableAllBlips();

                // var allControllers = FindObjectsOfType<Controller>();
                // allControllers.ToList().Remove(controller);

                advancedRoomManager.StartCoroutine(advancedRoomManager.RestartGameAfterDeath());
                
                // advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.MatchStats.KillStatsContent.gameObject.SetActive(true);
                // advancedRoomManager.StartCoroutine(advancedRoomManager.StatsEnabledTimer());
                //
                // var tempScript = Instantiate(advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.matchStatsPlaceholder.gameObject, advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.MatchStats.KillStatsContent).GetComponent<UIPlaceholder>();
                // tempScript.gameObject.SetActive(true);
                // tempScript.KillerName.text = controller.KillerName;
                // tempScript.VictimName.text = photonView.Owner.NickName;
                // if (controller.KilledWeaponImage && tempScript.WeaponIcon) tempScript.WeaponIcon.texture = controller.KilledWeaponImage;
            }
#endif
            controller.CameraController.enabled = false;
            controller.UIManager.CharacterUI.crosshairMainObject.gameObject.SetActive(false);

            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            
            object[] content =
            {
                photonView.ViewID,
                killerActorID.type,
                killerActorID.actorID,
                attackType
                
            };
            PhotonNetwork.RaiseEvent((byte)MultiplayerHelper.PhotonEventCodes.PlayerDeath, content, options, SendOptions.SendReliable);

            if (GetComponent<PhotonView>())
                GetComponent<PhotonView>().enabled = false;

            enabled = false;
        }

//         public void UpdatePlayersList()
//         {
//             if (basicRoomManager)
//             {
//                 basicRoomManager.ClearPlayerList();
//                 basicRoomManager.InstantiateCharactersInfo(PhotonNetwork.PlayerList.ToList(), basicRoomManager.currentUIManager.advancedMultiplayerGameRoom.StartMenu.PlayersContent.content);
//             }
// #if USK_ADVANCED_MULTIPLAYER
//             else if (advancedRoomManager)
//             {
//                 advancedRoomManager.UpdateAvatarsInSurvivalGame();
//                 advancedRoomManager.UpdatePlayersUIList(false);
//             }
// #endif
//
//             // var options = new RaiseEventOptions
//             // {
//             //     CachingOption = EventCaching.DoNotCache,
//             //     Receivers = ReceiverGroup.Others
//             // };
//             // RaiseEventSender(null, MultiplayerHelper.PhotonEventCodes.UpdatePlayerList, options);
//         }


#if USK_ADVANCED_MULTIPLAYER
        // void CalculatePlayerPlace()
        // {
        //     if(!advancedRoomManager) return;
        //     
        //     if (!advancedRoomManager.useTeams)
        //     {
        //         var numberOfLivePlayers = PhotonNetwork.PlayerListOthers.Count(player => (int) player.CustomProperties["d"] == 0);
        //         numberOfLivePlayers++;
        //         PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"pl", numberOfLivePlayers}});
        //     }
        //     else
        //     {
        //         var numberOfLivePlayers = 0;
        //         
        //         foreach (var player in PhotonNetwork.PlayerListOthers)
        //         {
        //             if ((MultiplayerHelper.Teams) player.CustomProperties["pt"] == controller.multiplayerTeam)
        //             {
        //                 if ((int) player.CustomProperties["d"] == 0) numberOfLivePlayers++;
        //             }
        //         }
        //
        //         numberOfLivePlayers++;
        //         PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"pl", numberOfLivePlayers}});
        //     }
        // }
#endif


        #region ProcessingSynchElements

        void RaiseEventSender(object value, MultiplayerHelper.PhotonEventCodes code, RaiseEventOptions options)
        {
            object[] content =
            {
                photonView.ViewID, value
            };
            PhotonNetwork.RaiseEvent((byte) code, content, options, SendOptions.SendReliable);
        }

        void OnEvent(EventData photonEvent)
        {
            var eventCode = (MultiplayerHelper.PhotonEventCodes) photonEvent.Code;
            var data = photonEvent.CustomData as object[];

            if (data == null) return;

            // if (eventCode == PUNHelper.PhotonEventCodes.UpdateWeaponsFromScript)
            // {
            //     if (data.Length == 1)
            //     {
            //        weaponManager.ClearAllWeapons();
            //     }
            //     else if (data.Length == 3)
            //     {
            //         weaponManager.AddNewWeapon(weaponManager.slots[(int)data[1]].weaponSlotInInspector[(int)data[2]].weapon, 0);
            //     }
            // }

            if (eventCode == MultiplayerHelper.PhotonEventCodes.UpdatePlayerList && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    if (roomManager)
                    {
                        roomManager.UpdatePlayersUIList();
                    }
#if USK_ADVANCED_MULTIPLAYER
                    else if (advancedRoomManager)
                    {
                        advancedRoomManager.UpdatePlayersUIList(false);
                        advancedRoomManager.UpdateAvatarsInSurvivalGame();
                    }
#endif
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.DamagePlayer && (int) data[0] == photonView.ViewID)
            {
                var damage = (float) data[1];
                var attackerActorNumber = (int) data[2];
                var attackerType = (string) data[3];
                var attackType = (string) data[4];

                //apply damage on the current client
                controller.TakingDamage(damage, attackType, false, new Helper.ActorID{actorID = attackerActorNumber, type = attackerType});
                
                // else 
                // {
                //     if (PhotonNetwork.IsMasterClient && advancedRoomManager.aiArea)
                //     {
                //         foreach (var bot in advancedRoomManager.aiArea.allBotsInMatch)
                //         {
                //             if (bot.photonView.ViewID == targetID)
                //             {
                //                 if (!bot.opponentsWhoAttackedThisAI.Contains(attackerID))
                //                     bot.opponentsWhoAttackedThisAI.Add(attackerID);
                //             }
                //         }
                //     }
                // }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeHealth && (int) data[0] == photonView.ViewID)
            {
                // if (data.Length == 3)
                // {
                //     controller.health = (float) data[1];
                //     currentHealth = controller.health;
                //
                //     if (controller.health <= 0)
                //     {
                //         controller.KillerName = (string) data[2];
                //
                //         foreach (var character in FindObjectsOfType<CharacterSync>())
                //         {
                //             if (character.controller.multiplayerNickname == controller.KillerName)
                //                 if (character.weaponController && character.weaponController.weaponImage)
                //                     controller.KilledWeaponImage = (Texture2D) character.weaponController.weaponImage;
                //         }
                //     }
                // }
                // else 
                
                if (data.Length == 2)
                {
                    controller.health = (float) data[1];
                    // currentHealth = controller.health;
                }

                // if (photonView.IsMine)
                //     photonView.Owner.SetCustomProperties(new Hashtable {{"plh", controller.health}});
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.PlayerDeath && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    foreach (var aiArea in eventsManager.allAreasInScene)
                    {
                        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                        {
                            if (PhotonNetwork.IsMasterClient)
                                aiArea.ManagePlayersBetweenOpponents();
                        }
                    }

                    foreach (var part in controller.BodyParts)
                    {
                        part.GetComponent<Rigidbody>().isKinematic = false;
                    }

                    controller.anim.enabled = false;
                    controller.enabled = false;
                    weaponManager.enabled = false;

                    if (controller.CameraController.MainCamera)
                        Destroy(controller.CameraController.MainCamera.gameObject);

                    if (controller.blipRawImage && controller.blipDeathTexture)
                    {
                        var color = controller.blipRawImage.color;
                        color.a = 1;
                        controller.blipRawImage.color = color;

                        controller.blipRawImage.texture = controller.blipDeathTexture;

                        var blipScript = controller.gameObject.AddComponent<Blip>();
                        blipScript.blipImage = new UIHelper.MinimapImage{image = controller.blipRawImage};
                        blipScript.uiManager = controller.UIManager;
                    }

                    if (weaponManager.WeaponController)
                        Destroy(weaponManager.WeaponController.gameObject);

                    if (controller.healthBarImage && controller.multiplayerStatsBackground)
                    {
                        controller.healthBarImage.gameObject.SetActive(false);
                        controller.multiplayerStatsBackground.SetActive(false);
                    }

                    if (controller.nickNameText)
                        controller.nickNameText.gameObject.SetActive(false);

                    if (roomManager)
                    {
                        roomManager.UpdatePlayersUIList();
                    }
#if USK_ADVANCED_MULTIPLAYER
                    else if (advancedRoomManager)
                    {
                        advancedRoomManager.UpdatePlayersUIList(false);
                        advancedRoomManager.UpdateAvatarsInSurvivalGame();

                        AMHelper.UpdatePlayersList(advancedRoomManager, controller);
                        
                        var killerActor = new Helper.ActorID{actorID = (int)data[2], type = (string) data[1]};
                        var attackType = (string) data[3];
                        var curCharacterActor = new Helper.ActorID() {actorID = photonView.OwnerActorNr, type = "player"};
                        
                        AMHelper.ShowKillDeathStats(advancedRoomManager, killerActor, curCharacterActor, attackType);
                    }
#endif
                    if (photonView)
                        photonView.enabled = false;

                    enabled = false;
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeWeapon && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    weaponManager.currentSlot = (int) data[1];
                    weaponManager.slots[weaponManager.currentSlot].currentWeaponInSlot = (int) data[2];

                    if ((bool) data[3])
                    {
                        weaponManager.hasWeaponChanged = true;
                    }
                    else
                    {
                        weaponManager.hideAllWeapons = true;
                    }

                    if (weaponManager.Controller.firstSceneStart)
                    {
                        weaponManager.Controller.InitializeParameters();
                        weaponManager.InitializeParameters(true);
                        weaponManager.Controller.CameraController.InitializeParameters();

                        weaponManager.Controller.firstSceneStart = false;
                    }

                    weaponManager.ChooseWeapon(false);
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Aim && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    if (weaponController)
                        weaponController.Aim(false, false, false);
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeMovementType && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    if ((string) data[1] == "TD")
                    {
                        CharacterHelper.ChangeTDMode(controller);

                        CharacterHelper.ResetCameraParameters(controller.TypeOfCamera, controller.TypeOfCamera, controller);

                        if (weaponManager.WeaponController)
                            WeaponsHelper.SetWeaponPositions(weaponManager.WeaponController, true, controller.DirectionObject);
                    }
                    else
                    {
                        controller.ChangeMovementType();
                    }
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.CreateHitMark && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2 && !controller.isRemoteCharacter)
                {
                    if (data[1] is int)
                    {
                        Transform player = null;

                        foreach (var character in FindObjectsOfType<CharacterSync>())
                        {
                            if (character.photonView.ViewID == (int) data[1])
                            {
                                player = character.transform;
                            }
                        }

                        if (player)
                        {
                            var direction = player.position - controller.transform.position;
                            var targetPosition = player.position + direction * 1000;
                            CharacterHelper.CreateHitMarker(controller, player, targetPosition);
                        }
                    }
                    else
                    {
                        var direction = (Vector3) data[1] - controller.transform.position;
                        var targetPosition = (Vector3) data[1] + direction * 1000;
                        CharacterHelper.CreateHitMarker(controller, null, targetPosition);
                    }
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.MeleeAttack && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    if ((bool) data[1])
                    {
                        weaponController.animationIndex = (int) data[2];
                        weaponController.crouchAnimationIndex = (int) data[3];
                        weaponController.MeleeAttack();
                    }
                    else weaponController.MeleeAttackOff();
                }
                else if (data.Length == 3)
                {
                    if ((bool) data[1])
                    {
                        weaponManager.animationIndex = (int) data[2];
                        weaponManager.Punch();
                    }
                    else
                    {
                        weaponManager.DisablePunchAttack();
                    }
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Crouch && (int) data[0] == photonView.ViewID)
            {
                controller.isCrouch = !controller.isCrouch;

                if (controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
                {
                    if (controller.inventoryManager.hasAnyWeapon)
                        weaponManager.WeaponController.CrouchHands();
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Grenade && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    controller.thisCamera.transform.position = (Vector3) data[1];
                    controller.thisCamera.transform.rotation = (Quaternion) data[2];

                    weaponController.ThrowGrenade((bool) data[3]);
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeAttack && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    weaponController.ChangeAttack();
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Reload && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    weaponController.Reload();
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Bullets && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 3)
                {
                    controller.thisCamera.transform.position = (Vector3) data[1];
                    controller.thisCamera.transform.rotation = (Quaternion) data[2];

                    weaponController.BulletAttack();
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Fire && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    if (!(bool) data[1])
                    {
                        weaponController.GetComponent<AudioSource>().Stop();
                        weaponController.attackAudioPlay = false;
                    }
                    else
                    {
                        if (!weaponController.attackAudioPlay)
                        {
                            controller.anim.CrossFade("Attack", 0, 1, Time.deltaTime, 10);

                            if (weaponController.Attacks[weaponController.currentAttack].AttackAudio)
                            {
                                weaponController.GetComponent<AudioSource>().clip = weaponController.Attacks[weaponController.currentAttack].AttackAudio;
                                weaponController.GetComponent<AudioSource>().Play();
                                weaponController.attackAudioPlay = true;
                            }
                        }
                    }
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.Rocket && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    controller.thisCamera.transform.rotation = (Quaternion) data[3];
                    controller.thisCamera.transform.position = (Vector3) data[2];

                    weaponController.RocketAttack();
                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeCameraType && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 2)
                {
                    CharacterHelper.SwitchCamera(controller.TypeOfCamera, (CharacterHelper.CameraType) data[1], controller);

                    if (weaponManager.WeaponController)
                        WeaponsHelper.SetWeaponPositions(weaponManager.WeaponController, true, controller.DirectionObject);

                }
            }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.DropWeapon && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 4)
                {
                    weaponManager.DropIdMultiplayer = (string) data[1];
                    weaponManager.DropDirection = (Vector3) data[2];
                    weaponManager.DropWeapon((bool) data[3]);
                }
            }
//             else if (eventCode == MultiplayerHelper.PhotonEventCodes.SetKillAssistants && (int) data[0] == photonView.ViewID)
//             {
//                 for (var index = 1; index < data.Length; index++)
//                 {
//                     var name = (string) data[index];
//
//                     if (name == PhotonNetwork.LocalPlayer.NickName)
//                     {
// #if USK_ADVANCED_MULTIPLAYER
//                         if (advancedRoomManager && advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.MatchStats.AddScorePopup)
//                         {
//                             advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.MatchStats.AddScorePopup.gameObject.SetActive(true);
//                             advancedRoomManager.currentUIManager.advancedMultiplayerGameRoom.MatchStats.AddScorePopup.text = "+ " + PlayerPrefs.GetInt("KillAssist") + " (Kill Assistant)";
//                             advancedRoomManager.StartCoroutine(advancedRoomManager.AddScorePopupDisableTimeout());
//                         }
// #endif
//                     }
//                 }
//             }
            else if (eventCode == MultiplayerHelper.PhotonEventCodes.BulletHit && (int) data[0] == photonView.ViewID)
            {
                if (data.Length == 3)
                {

                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(controller.onNavMesh);
                stream.SendNext(controller.currentNavMeshArea);
                stream.SendNext(controller.noiseRadius);
                stream.SendNext(controller.BodyLocalEulerAngles);
                stream.SendNext(controller.CurrentRotation);
                stream.SendNext(controller.SmoothIKSwitch);
                stream.SendNext(controller.currentGravity);
                stream.SendNext(controller.thisCamera.transform.position);
                stream.SendNext(controller.thisCamera.transform.eulerAngles);
                stream.SendNext(controller.currentCharacterControllerCenter);

                if (controller.CameraController.BodyLookAt)
                    stream.SendNext(controller.CameraController.BodyLookAt.position);

                stream.SendNext(controller.bodyRotationDownLimit_x);
                stream.SendNext(controller.bodyRotationDownLimit_y);
                stream.SendNext(controller.bodyRotationUpLimit_x);
                stream.SendNext(controller.bodyRotationUpLimit_y);

                if (weaponManager.WeaponController)
                    stream.SendNext(weaponManager.WeaponController.BarrelRotationSpeed);

            }
            else
            {
                controller.onNavMesh = (bool) stream.ReceiveNext();
                controller.currentNavMeshArea = (int) stream.ReceiveNext();
                controller.noiseRadius = (float) stream.ReceiveNext();
                controller.BodyLocalEulerAngles = (Vector3) stream.ReceiveNext();
                controller.CurrentRotation = (Quaternion) stream.ReceiveNext();
                controller.SmoothIKSwitch = (float) stream.ReceiveNext();
                controller.currentGravity = (float) stream.ReceiveNext();
                CameraPosition = (Vector3) stream.ReceiveNext();
                CameraRotation = (Vector3) stream.ReceiveNext();

                var yValue = (float) stream.ReceiveNext();

                if (controller.CharacterController)
                    controller.CharacterController.center = new Vector3(controller.CharacterController.center.x, yValue, controller.CharacterController.center.z);

                if (controller.CameraController.BodyLookAt)
                    controller.CameraController.BodyLookAt.position = (Vector3) stream.ReceiveNext();
                
                controller.bodyRotationDownLimit_x = (float) stream.ReceiveNext();
                
                controller.bodyRotationDownLimit_y = (float) stream.ReceiveNext();
                controller.bodyRotationUpLimit_x = (float) stream.ReceiveNext();
                controller.bodyRotationUpLimit_y = (float) stream.ReceiveNext();

                if (weaponManager.WeaponController && stream.Count > 15)
                    weaponManager.WeaponController.BarrelRotationSpeed = (float) stream.ReceiveNext();

                if (firstTake)
                    firstTake = false;
            }
        }

        #endregion

#endif
    }
}


