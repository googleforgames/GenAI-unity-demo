using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class AIController :
        
#if USK_MULTIPLAYER
        MonoBehaviourPun, IPunObservable
#else
    MonoBehaviour
#endif
    
    {
        public AIArea aiArea;
        public AIAttack aiAttack;

        public UIManager currentUIManager;

        public Controller globalTarget;
        public Controller closestTarget;
        
#if USK_MULTIPLAYER
        public PhotonAnimatorView photonAnimatorView;
        private RoomManager roomManager;
#endif
#if USK_ADVANCED_MULTIPLAYER
        public AdvancedRoomManager advancedRoomManager;
#endif

        public AIHelper.Opponent lastOpponentWhoHitThisEnemy;

        public AIHelper.OpponentsDetectionType opponentsDetectionType = AIHelper.OpponentsDetectionType.All;

        public List<AIHelper.Opponent> allPlayersInScene = new List<AIHelper.Opponent>();
        public List<AIHelper.Opponent> playersToRemove = new List<AIHelper.Opponent>();
        public List<AIHelper.Opponent> allPlayersSeenByEnemy = new List<AIHelper.Opponent>();
        public List<AIHelper.Opponent> allPlayersHeardByEnemy = new List<AIHelper.Opponent>();
        public List<AIHelper.Opponent> allPlayersNearEnemy = new List<AIHelper.Opponent>();

        public AIHelper.NavMeshAgentParameters navMeshAgentParameters = new AIHelper.NavMeshAgentParameters();

        public Projector bloodProjector;

        public float visionDetectionTime = 5;
        public float hearingDetectionTime = 5;
        public float rangeDetectionTime = 5;
        
        public MultiplayerHelper.Teams multiplayerTeam; 
        public MultiplayerHelper.CanKillOthers canKillOthers;

        public bool multiplayerBot;
        public bool oneShotOneKill;
        public int deaths;
        public int kills;
        public int score;
        public int place;
        public string avatarName;
        public string nickname = "Enemy";
        public string enemyID;
        public List<Helper.ActorID> opponentsWhoAttackedThisAI = new List<Helper.ActorID>();

        public bool captureHardPoint;
        public bool captureAPoint;
        public bool captureBPoint;
        public bool captureCPoint;

        [Range(1, 100)] public float distanceToSee = 30;
        [Tooltip("• This value regulates how many percent is the attack zone relative to the See Area." + "\n" +
                 "• During the game the enemy runs to the Attack Area and then attacks.")]
        [Range(1, 100)] public float attackDistancePercent = 50;
        [Range(1, 180)] public float peripheralHorizontalAngle = 60;
        [Range(1, 180)] public float centralHorizontalAngle = 30;
        [Range(1, 180)] public float heightToSee = 20;
        [Range(1, 1000)] public float health = 100;
        public float currentSpeed;
        public float walkForwardSpeed = 2f;
        public float runForwardSpeed = 3f;
        public float runBackwardSpeed = 1f;
        public float runLateralSpeed = 1f;
        [Range(0.1f, 2)] public float SpeedOffset = 1;
        public float headMultiplier = 1;
        public float bodyMultiplier = 1;
        public float handsMultiplier = 1;
        public float legsMultiplier = 1;
        public float damageAnimationTimeout;
        public float destroyRagdollTime = 10;
        public float attentionValue;
        public float increaseAttentionValueTimer;
        public float turnSpeed = 3;
        public float grenadeEffectTimeout;
        public float bodyRotationSpeed = 1;
        public float endAttackTimer;
        public float rateOfAttack;
        public float monitoringTimer;
        
        [Range(1, 100)] public float detectionDistance = 5;
        [Range(1, 100)] public float attackDistance = 15;
        
        [Range(0, 100)] public int playDamageSoundsDelay;
        public int enemyType;
        public int currentWayPointIndex = -1;
        public int previousWayPointIndex = -1;
        
        public bool targetOutOfSight = true;
        public bool notSeeTarget;
        public bool inAttackZone;
        public bool rootMotionMovement = true;
        public bool isHuman;
        public bool useCovers;
        public bool UseStates = true;
        public bool grenadeEffect;
        public bool observer;
        public bool allSidesMovement;
        public bool runWhileAttacking = true;
        public bool deleteWeaponAfterDeath;
        public bool setNavMeshHeightMoreAccurately;
        public bool rotateBlipWithEnemy = true;

        public List<Transform> pointsToCheckForThisEnemy = new List<Transform>();
        public List<Transform> BodyParts = new List<Transform> {null, null, null, null, null, null, null, null, null, null, null};

        public AIHelper.CoverPoint currentCoverPoint;
        
        public Transform currentPointToMove;
        public Transform directionObject;
        public Transform bodyDirectionObject; 
        public Transform genericAvatarBody; 
        
        [Tooltip("This variable is needed to separate the weapon from AI on death.")]
        public GameObject weapon;
        public GameObject ragdoll;
        public GameObject currentMultiplayerTarget;
        public List<GameObject> itemsAppearingAfterDeath = new List<GameObject>();
        
        public List<AIHelper.GenericCollider> genericColliders = new List<AIHelper.GenericCollider>();
        
        public AudioSource FeetAudioSource;
        public AudioSource audioSource;

        public ProjectSettings projectSettings;

        public List<AIHelper.EnemyAttack> Attacks = new List<AIHelper.EnemyAttack>{new AIHelper.EnemyAttack()};

        public AnimationClip LeftRotationAnimation;
        public AnimationClip RightRotationAnimation;
        public AnimationClip SpotRun;
        public AnimationClip SpotWalk;
        public AnimationClip IdleAnimation;
        public AnimationClip AttackIdleAnimation;
        public AnimationClip CrouchIdleAnimation;
        public AnimationClip WalkAnimation;
        public AnimationClip RunAnimation;
        public AnimationClip GrenadeReaction;
        public AnimationClip DeathAnimation;
        public List<AnimationClip> FindAnimations;
        public List<AnimationClip> DamageAnimations;
        public AnimationClip[] AllSidesMovementAnimations = new AnimationClip[8];

        public MovementBehavior MovementBehaviour;
        
        public AIHelper.EnemyStates currentState = AIHelper.EnemyStates.Waypoints;
        
        public GameObject attentionStatusMainObject;
        public GameObject healthBarMainObject;
        
        public Image yellowImg;
        public Image redImg;
        public Image healthBarValue;
        
        public Text nameText;
        
        public RawImage blipRawImage;
        public Texture blipMainTexture;
        public Texture blipDeathTexture;
        
        public List<Helper.Attacker> attackers = new List<Helper.Attacker>();
        
        public Color32 opponentBlipColor = new Color32(255, 0, 0, 255);
        public Color32 teammateBlipColor = new Color32(0, 255, 0, 255);
        public Color32 opponentHealthBarColor = new Color32(255, 0, 0, 255);
        public Color32 teammateHealthBarColor = new Color32(0, 255, 0, 255);

        public List<Texture> BloodHoles = new List<Texture>{null};
        public List<GameObject> additionalHitEffects = new List<GameObject>();
        public List<AudioClip> damageSounds = new List<AudioClip>();
        
        public Canvas statsCanvas;
        public List<GameObject> bloodMarkersOnBody = new List<GameObject>();
        
        public Animator anim;

        public RuntimeAnimatorController AnimatorController;

        public Helper.AnimationClipOverrides ClipOverrides; 
        public AnimatorOverrideController newController;

        public RaycastHit[] visionHits = new RaycastHit[20];

        #region InspectorParameters

        public int topInspectorTab;
        public int bottomInspectorTab;
        public int currentInspectorTab;
        public int currentBehaviourInspectorTab;

        public bool delete;
        public bool rename;
        public bool renameError;
        
        public string curName;

        #endregion

        private int currentDamageAnimationIndex;
        private float currentDamageAnimationTime;
        private float addPointToCheckFunctionCount;
        private int addMovePointFunctionCount;
        
        private float randomMovementTimeout;

        public float defaultHealth;
        private float timerBehindCover;
        private float timeBehindCover = 5;
        private float statsCanvasHeight;
        private float currentDamageSoundsDelay;

        private string[] allSidesMovementOverrideAnimations = 
        {
            "_EnemyMoveForward", "_EnemyMoveForwardLeft", "_EnemyMoveForwardRight", "_EnemyMoveLeft",
            "_EnemyMoveRight", "_EnemyMoveBackwardLeft", "_EnemyMoveBackwardRight", "_EnemyMoveBackward"
        };
        
        private bool isMovementPause;
        private bool isNextAction;
        public bool damageTakenFromPlayer;
        public bool canReduceAttentionValue;
        public bool agentIsStopped;
        public bool canAttack;
        private bool adjustAgentPosition;
        
        public Vector3 currentMonitoringDirection;
        private Vector3 agentDestination;
        public Vector2 bodyRotationLimits = new Vector2(30,30);
        public Vector2 currentBodyRotationLimits;
        private Vector2 desiredBodyRotationAngle;
        
        private float desiredRotationAngle;
        private Quaternion desiredRotation;

        public NavMeshAgent agent;

        public bool opponentForLocalPlayer;
        private bool parametersAreInitialised;
        public bool inBattleZone;
        private bool playStartLinkAnimation;
        private bool playEndLinkAnimation = true;

        private int currentBattleZone = -1;

        public List<BodyPartCollider> allBodyColliders = new List<BodyPartCollider>();
        
        private Coroutine reloadRoutine;
        
        public AnimationCurve transitionCurve = new AnimationCurve();

        void Awake()
        {
            agent = !gameObject.GetComponent<NavMeshAgent>() ? gameObject.AddComponent<NavMeshAgent>() : gameObject.GetComponent<NavMeshAgent>();
            agent.radius = navMeshAgentParameters.radius;
            agent.height = navMeshAgentParameters.height;
            agent.agentTypeID = navMeshAgentParameters.agentType;
            agent.autoBraking = false;
            agent.stoppingDistance = 0;

            defaultHealth = health;
            
            aiAttack = gameObject.AddComponent<AIAttack>();
            aiAttack.aiController = this;
            
            anim = gameObject.GetComponent<Animator>();
            
            anim.runtimeAnimatorController = AnimatorController;
            
            newController = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = newController;

            ClipOverrides = new Helper.AnimationClipOverrides(newController.overridesCount);
            
#if USK_MULTIPLAYER
            roomManager = FindObjectOfType<RoomManager>();
#endif
#if USK_ADVANCED_MULTIPLAYER
            advancedRoomManager = FindObjectOfType<AdvancedRoomManager>();
#endif

            var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            
            foreach (var item in itemsAppearingAfterDeath.Where(item => allObjectsOnScene.Contains(item)))
            {
                item.SetActive(false);
            }
        }

        void Start()
        {
            InitializeParameters();
            parametersAreInitialised = true;
        }

        void InitializeParameters()
        {
            isHuman = anim.avatar.isHuman;

#if USK_ADVANCED_MULTIPLAYER
            if (advancedRoomManager && advancedRoomManager.aiArea && advancedRoomManager.aiArea.multiplayerMatch)
            {
                aiArea = advancedRoomManager.aiArea;
                UseStates = false;
                multiplayerBot = true;
                
                if(advancedRoomManager.matchStarted)
                    StartMovement(false);

                var botsCountInEachArea = new int [advancedRoomManager.battleZones.Count];
                
                foreach (var bot in aiArea.allBotsInMatch)
                {
                    if(bot.currentBattleZone != -1)
                        botsCountInEachArea[bot.currentBattleZone]++;
                }
                
                var minVal = botsCountInEachArea.ToList().Min();
                currentBattleZone = botsCountInEachArea.ToList().IndexOf(minVal);
                
                aiArea.allBotsInMatch.Add(this);

                oneShotOneKill = (bool)PhotonNetwork.CurrentRoom.CustomProperties["oneShot"];

                if(PhotonNetwork.InRoom)
                    canKillOthers = (MultiplayerHelper.CanKillOthers) PhotonNetwork.CurrentRoom.CustomProperties["km"];
            }
#endif

            agent.autoTraverseOffMeshLink = false;
            agent.angularSpeed = 120;

            gameObject.name = Helper.CorrectName(gameObject.name);

            audioSource = GetComponent<AudioSource>();

            currentDamageSoundsDelay = playDamageSoundsDelay;

            newController.GetOverrides(ClipOverrides);

            if (WalkAnimation)
                ClipOverrides["_EnemyWalk"] = WalkAnimation;

            if (IdleAnimation)
                ClipOverrides["_EnemyIdle"] = IdleAnimation;

            if (SpotRun)
                ClipOverrides["_SpotRun"] = SpotRun;

            if (SpotWalk)
                ClipOverrides["_SpotWalk"] = SpotWalk;

            if (AttackIdleAnimation) ClipOverrides["_EnemyAttackIdle"] = AttackIdleAnimation;
            else ClipOverrides["_EnemyAttackIdle"] = IdleAnimation;

            if (LeftRotationAnimation)
                ClipOverrides["_LeftRotation"] = LeftRotationAnimation;

            if (RightRotationAnimation)
                ClipOverrides["_RightRotation"] = RightRotationAnimation;

            if (CrouchIdleAnimation)
                ClipOverrides["_EnemyCrouchIdle"] = CrouchIdleAnimation;

            if (RunAnimation)
                ClipOverrides["_EnemyRun"] = RunAnimation;
            
            if (DeathAnimation)
                ClipOverrides["_EnemyDeath"] = DeathAnimation;
            
            if (GrenadeReaction)
                ClipOverrides["_EnemyGrenadeReaction"] = GrenadeReaction;

            if (Attacks[0].HandsIdleAnimation)
                ClipOverrides["_EnemyHandsIdle"] = Attacks[0].HandsIdleAnimation;

            if (Attacks[0].HandsAttackAnimation)
                ClipOverrides["_EnemyHandsAttack"] = Attacks[0].HandsAttackAnimation;

            if (Attacks[0].HandsReloadAnimation)
                ClipOverrides["_EnemyHandsReload"] = Attacks[0].HandsReloadAnimation;

            ClipOverrides["_EnemyAttack"] = Attacks[0].MeleeAttackAnimations.Find(clip => clip != null);

            currentDamageAnimationIndex = 0;

            for (var i = 0; i < AllSidesMovementAnimations.Length; i++)
            {
                if (AllSidesMovementAnimations[i])
                {
                    ClipOverrides[allSidesMovementOverrideAnimations[i]] = AllSidesMovementAnimations[i];
                }
            }

            newController.ApplyOverrides(ClipOverrides);

            if (anim.avatar && anim.avatar.isHuman && Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
            {
                anim.SetLayerWeight(1, 1);
            }
            else if (anim.avatar && !anim.avatar.isHuman || Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
            {
                anim.SetLayerWeight(1, 0);
            }

            if (weapon && !weapon.GetComponent<AudioSource>())
                weapon.AddComponent<AudioSource>();

            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
            {
                Attacks[0].UseReload = false;
                anim.SetBool("Melee", true);

            }

            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee || Attacks[0].AttackType == AIHelper.AttackTypes.Fire)
            {
                useCovers = false;
            }

            if (opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing || opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange)
            {
                attackDistancePercent = 100;
                heightToSee = 20;
            }

            Attacks[0].CurrentAmmo = Attacks[0].InventoryAmmo;
            rateOfAttack = Attacks[0].RateOfAttack;
            
            if (statsCanvas)
            {
                if (isHuman)
                    statsCanvasHeight = Vector3.Distance(statsCanvas.transform.position, anim.GetBoneTransform(HumanBodyBones.Head).position);
            }

            if (attentionStatusMainObject)
            {
                attentionStatusMainObject.SetActive(UseStates);
            }

            if (healthBarMainObject)
            {
                healthBarMainObject.SetActive(true);
            }

            if (nameText)
                nameText.gameObject.SetActive(true);

            Helper.ChangeLayersRecursively(transform, "Enemy");

            if (isHuman) Helper.ManageBodyColliders(BodyParts, this);
            else Helper.ManageBodyColliders(genericColliders, this);

            if (!bodyDirectionObject)
            {
                // if (isHuman)
                // {
                    bodyDirectionObject = new GameObject("body direction object").transform;
                    bodyDirectionObject.parent = isHuman ? anim.GetBoneTransform(HumanBodyBones.Hips) : genericAvatarBody; //directionObject.parent;
                    bodyDirectionObject.rotation = directionObject.rotation;
                    bodyDirectionObject.position = isHuman ? anim.GetBoneTransform(HumanBodyBones.Hips).position : genericAvatarBody.position;
                    // bodyDirectionObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
                // }
                // else
                // {
                //     bodyDirectionObject = genericAvatarBody ? genericAvatarBody : null;
                // }
            }

            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
                allSidesMovement = false;

            if (FeetAudioSource)
            {
                FeetAudioSource.hideFlags = HideFlags.HideInHierarchy;
            }

            if (FindObjectOfType<UIManager>())
            {
                currentUIManager = FindObjectOfType<UIManager>();
            }

            if (currentUIManager)
            {
                if (!multiplayerBot)
                {
                    opponentForLocalPlayer = true;

                    if (healthBarValue)
                        healthBarValue.color = opponentHealthBarColor;

                    currentUIManager.allMinimapImages.Add(UIHelper.CreateNewBlip(currentUIManager, ref blipRawImage, blipMainTexture, opponentBlipColor, "AI Blip", false));
                }
                else
                {
                    StartCoroutine(EnableHealthBarTimeout());
                }
            }

            if (observer)
            {
                useCovers = false;
                currentMonitoringDirection = directionObject.forward;
                StopMovement();
            }

            GetFindAnimation();

            StopMovement();
            
            currentWayPointIndex = 0;
            previousWayPointIndex = 0;

            if (aiArea && aiArea.globalAttackState && aiArea.communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther)
            {
                currentState = AIHelper.EnemyStates.Attack;
                attentionValue = 2f;

                if (!observer)
                    StartMovement(false);

                // only on master client
                aiArea.ManagePlayersBetweenOpponents();
            }
            else if(aiArea && !aiArea.globalAttackState || aiArea.communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents)
            {
                if (MovementBehaviour && !observer)
                {
                    StartMovement(false);
                    SetDest(MovementBehaviour.points[currentWayPointIndex].point.transform.position);
                }
            }
        }

        private void OnAnimatorMove()
        {
            if (!agent) return;
            
            var transformComponent = transform;
            
#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            {
                if(agent.updatePosition)
                    agent.updatePosition = false;
                
                if(agent.updateRotation)
                    agent.updateRotation = false;
                
                agent.nextPosition = transformComponent.position;
                
                return;
            }
#endif
            
            if (agent.isOnOffMeshLink)
            {
                if (!playStartLinkAnimation)
                {
                    anim.CrossFade("Start Jump", 0, 0);
                    StartCoroutine(NavMeshLinkMovement());
                    playStartLinkAnimation = true;
                }
            }

            //state when animations control all movement
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Damage Reaction") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump End") || anim.GetCurrentAnimatorStateInfo(0).IsName("Grenades Reaction") || anim.GetCurrentAnimatorStateInfo(0).IsName("Find") || Attacks[0].AttackType == AIHelper.AttackTypes.Melee && (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")))
            {
                agent.updatePosition = false;
                agent.updateRotation = false;
            
                var position = anim.rootPosition;
                position.y = agent.nextPosition.y;
                
                transformComponent.rotation = anim.rootRotation;
                transformComponent.position = position;
                
                agent.nextPosition = transformComponent.position;
            
                adjustAgentPosition = false;
            }
            else
            {
                if (!adjustAgentPosition)
                {
                    agent.updatePosition = true;
                    agent.updateRotation = true;
                    
                    adjustAgentPosition = true;
                }
            }
            
            anim.SetFloat("AttentionValue", attentionValue);

            if (!agent.isOnOffMeshLink && setNavMeshHeightMoreAccurately)
            {
                //more accurate positioning of AI on NavMesh
                var ray = new Ray(transformComponent.position + Vector3.up * 2, Vector3.down);
                var layerMask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Character") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Head") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
                
                if (Physics.Raycast(ray, out var hitInfo, 5, layerMask))
                {
                    var position = transformComponent.position;
                    position = new Vector3(position.x, hitInfo.point.y, position.z);
                    transform.position = position; // new Vector3(transform.position.x, 12.607f, transform.position.z);//Vector3.Lerp(transform.position, position, 0.5f * Time.deltaTime);
                }
            }

            if (!rootMotionMovement)
            {
                AIHelper.GetCurrentSpeed(this);
                agent.speed = currentSpeed;
                anim.SetFloat("SpeedOffset", 1);
                
                return;
            }
           
            
            anim.SetFloat("SpeedOffset", SpeedOffset);

            if (Time.deltaTime > 0)
            {
                var currentRotation = transform.rotation;

                //more accurate positioning of AI on NavMesh
                // var ray = new Ray(transformComponent.position + Vector3.up * 2, Vector3.down);
                // var layerMask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Character") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Head") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));

                if (!agent.isOnOffMeshLink)
                {
                    // if (Physics.Raycast(ray, out var hitInfo, 5, layerMask))
                    // {
                    //     var position = transformComponent.position;
                    //     position = new Vector3(position.x, hitInfo.point.y, position.z);
                    //     transform.position = Vector3.Lerp(transform.position, position, 0.5f * Time.deltaTime);
                    // }

                    var angle = Mathf.Abs(anim.GetFloat("AngleToTarget"));

                    if (anim.GetBool("Move"))
                    {
                        if (angle > 45)
                        {
                            if (!anim.GetBool("Run"))
                            {
                                agent.speed = (anim.deltaPosition / Time.deltaTime).magnitude * SpeedOffset;
                                agent.angularSpeed = 0;

                                var lookRot = agent.steeringTarget - transformComponent.position;

                                var lookRotation = Quaternion.LookRotation(lookRot);

                                lookRotation.x = currentRotation.x;
                                lookRotation.z = currentRotation.z;

                                currentRotation = Quaternion.Lerp(currentRotation, lookRotation, Time.deltaTime * turnSpeed);

                                transform.rotation = currentRotation;
                            }
                            else
                            {
                                agent.angularSpeed = 120;
                                agent.speed = 2;
                            }
                        }
                        else
                        {
                            agent.angularSpeed = 120;
                            agent.speed = (anim.deltaPosition / Time.deltaTime).magnitude * SpeedOffset;
                        }
                    }
                }
            }

            // GetFindAnimation();
        }

        IEnumerator NavMeshLinkMovement()
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
            float normalizedTime = 0.0f;
            
            while (true)
            {
                if (normalizedTime < 1)
                {
                    float yOffset = transitionCurve.Evaluate(normalizedTime);
                    agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
                    normalizedTime += Time.deltaTime / 1;
                }
                else
                {
                    if (agent.isOnNavMesh)
                    {
                        agent.CompleteOffMeshLink();
                        anim.CrossFade("Jump End", 0.01f, 0);
                        playStartLinkAnimation = false;
                    }

                    break;
                }

                yield return null;
            }
            
            
        }

        void Update()
        {
#if USK_MULTIPLAYER
            if (multiplayerBot && blipRawImage && health > 0)
            {
                var color = blipRawImage.color;

                if (opponentForLocalPlayer)
                {
                    if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
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

                // Debug.LogError(color.a + " | " + gameObject.name);

                blipRawImage.color = color;
            }

            if (roomManager && roomManager.controller)
            {
                AIHelper.UILookAtCharacter(this, roomManager.controller);
            }else 

#if USK_ADVANCED_MULTIPLAYER
            if (advancedRoomManager && advancedRoomManager.controller)
            {
                AIHelper.UILookAtCharacter(this, advancedRoomManager.controller);
                
            } else
#endif
#endif
            {
                if (aiArea.allPlayersInScene.Count > 0 && aiArea.allPlayersInScene[0].player)
                {
                    AIHelper.UILookAtCharacter(this, aiArea.allPlayersInScene[0].controller);
                }
            }
            
#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            {
                if (healthBarValue)
                {
                    healthBarValue.fillAmount = health / defaultHealth;
                }

                if (!multiplayerBot)
                {
                    if (yellowImg)
                        yellowImg.fillAmount = attentionValue;

                    if (redImg)
                        redImg.fillAmount = attentionValue - 1;

                    switch (currentState)
                    {
                        case AIHelper.EnemyStates.Attack:
                            AttackState();
                            break;
                    }
                }
                else
                {
                    if (currentMultiplayerTarget)
                    {
                        // currentState = AIHelper.EnemyStates.Attack;
#if USK_ADVANCED_MULTIPLAYER
                        if (advancedRoomManager && advancedRoomManager.matchStarted)
#endif
                            AttackState();
                    }
                }

                return;
            }
#endif

            if (agentIsStopped)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
            }
            else
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Covers Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run") || anim.GetCurrentAnimatorStateInfo(0).IsName("All-Sides Movement"))
                {
                    if(agent.isOnNavMesh)
                        agent.isStopped = false;
                }
            }

            //movement rotation
            var destination = agent.steeringTarget;
            
            var dirTransform = directionObject.transform;
            var direction = destination - dirTransform.position;
            var angle = Helper.AngleBetween(dirTransform.forward, direction);
            
            var distToTarget = Vector3.Distance(destination, transform.position);
            
            anim.SetFloat("AngleToTarget", Mathf.Lerp(anim.GetFloat("AngleToTarget"), angle, distToTarget > 1 ? 2 : 0.7f * Time.deltaTime));

            if (currentState == AIHelper.EnemyStates.Waypoints || currentState == AIHelper.EnemyStates.Warning && pointsToCheckForThisEnemy.Count == 0)
            {
                if (anim.GetFloat("DistanceToTarget") > 0)
                    anim.SetFloat("DistanceToTarget", Mathf.Lerp(anim.GetFloat("DistanceToTarget"), 0, 1 * Time.deltaTime));
            }
            else
            {
                anim.SetFloat("DistanceToTarget", Vector3.Distance(transform.position, agent.destination));
            }

            monitoringTimer += Time.deltaTime;
            
            increaseAttentionValueTimer += Time.deltaTime;

            if (increaseAttentionValueTimer > 3)
            {
                canReduceAttentionValue = true;
            }

            if (attentionValue > 0 && currentState == AIHelper.EnemyStates.Waypoints && allPlayersSeenByEnemy.Count == 0 && allPlayersHeardByEnemy.Count == 0 && allPlayersNearEnemy.Count == 0)
            {
                if (canReduceAttentionValue)
                {
                    attentionValue = Mathf.Lerp(attentionValue, 0, 1 * Time.deltaTime);
                }
            }
            else if (attentionValue > 1 && currentState == AIHelper.EnemyStates.Warning && (!aiArea.hasAnyPlayerInZone || !aiArea.globalAttackState))
            {
                if (canReduceAttentionValue)
                {
                    attentionValue = Mathf.Lerp(attentionValue, 1, 1 * Time.deltaTime);
                }
            }

            if (currentState != AIHelper.EnemyStates.Attack && anim.GetBool("Aim"))
            {
                anim.SetBool("Aim", false);
            }

            currentDamageAnimationTime += Time.deltaTime;

            if(yellowImg)
                yellowImg.fillAmount = attentionValue;
                
            if (redImg)
                redImg.fillAmount = attentionValue - 1;
            
            if (healthBarValue)
                healthBarValue.fillAmount = health / defaultHealth;

            if (statsCanvas)
            {
                if (isHuman)
                    statsCanvas.transform.position = anim.GetBoneTransform(HumanBodyBones.Head).position + new Vector3(0, statsCanvasHeight, 0);
            }

            grenadeEffectTimeout += Time.deltaTime;

            if (grenadeEffectTimeout > 1 && grenadeEffect && aiAttack.flashGrenadeEffectTimeout == null)
            {
                EndGrenadeEffect();
            }
            else if (grenadeEffect)
            {
                if(isHuman)
                    anim.SetLayerWeight(1, 0);
                
                anim.SetBool("Covers State", false);
            }
            
            anim.SetBool("Grenade Reaction", grenadeEffect);

            if (!multiplayerBot)
            {
                ScanningProcess();
                
                ClearEmptyPlayers();
                ManageValuesWhenGetDamage(lastOpponentWhoHitThisEnemy);
                
                switch (currentState)
                {
                    case AIHelper.EnemyStates.Waypoints:
                        WayPointsMovement();
                        break;
                    case AIHelper.EnemyStates.Warning:
                        WarningState();
                        break;
                    case AIHelper.EnemyStates.Attack:
                        AttackState();
                        break;
                }

                if (aiArea.communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && (currentState == AIHelper.EnemyStates.Attack || currentState == AIHelper.EnemyStates.Warning))
                {
                    if (aiArea.hasAnyPlayerInZone)
                    {
                        var seeOrHearAnyPlayer = allPlayersInScene.Exists(opponent => opponent.controller.currentNavMeshArea == aiArea.navMeshArea && (opponent.seePlayerHead || opponent.seePlayerHips || opponent.hearPlayer || opponent.inAttentionZone));
                        
                        if (seeOrHearAnyPlayer)
                        {
                            endAttackTimer = 0;
                        }
                        else
                        {
                            endAttackTimer += Time.deltaTime;

                            if (endAttackTimer > aiArea.disableAttackStateTime)
                            {
                                endAttackTimer = 0;
                                EndAttackIndependentOpponent();
                            }
                        }
                    }
                }
            }
            else
            {
#if USK_ADVANCED_MULTIPLAYER     
                MultiplayerBotScanningProcess();

                if (currentMultiplayerTarget)
                {
                    // currentState = AIHelper.EnemyStates.Attack;

                    if (advancedRoomManager && advancedRoomManager.matchStarted)
                        AttackState();
                }
                
                if (advancedRoomManager && advancedRoomManager.matchStarted)
                    MultiplayerBotMovement();

                if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Domination)
                {
                    var curPosition = transform.position;
                    captureAPoint = health > 0 && Vector3.Distance(advancedRoomManager.aPoint.transform.position, curPosition) < advancedRoomManager.aPoint.radius;
                    captureBPoint = health > 0 && Vector3.Distance(advancedRoomManager.bPoint.transform.position, curPosition) < advancedRoomManager.bPoint.radius;
                    captureCPoint = health > 0 && Vector3.Distance(advancedRoomManager.cPoint.transform.position, curPosition) < advancedRoomManager.cPoint.radius;
                    
                    if (advancedRoomManager.currentHardPoint)
                    {
                        if (Mathf.Abs((transform.position - advancedRoomManager.currentHardPoint.transform.position).x) < advancedRoomManager.currentHardPoint.size.x / 2 &&
                            Mathf.Abs((transform.position - advancedRoomManager.currentHardPoint.transform.position).z) < advancedRoomManager.currentHardPoint.size.y / 2)
                        {
                            captureHardPoint = true;
                        }
                        else
                        {
                            captureHardPoint = false;
                        }
                    }
                }
#endif
            }
        }

        public void EndGrenadeEffect()
        {
            if(isHuman)
                anim.SetLayerWeight(1, 1);

            if(currentState == AIHelper.EnemyStates.Waypoints) aiArea.CheckExplosion(transform.position);
            else StartMovement(false);
                
            grenadeEffect = false;
        }

        void EndAttackIndependentOpponent()
        {
            foreach (var player in aiArea.allPlayersInScene)
            {
                aiArea.ClearEmptyPlayer(player, false);
            }
            
            if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
            {
                currentPointToMove.gameObject.SetActive(false);
            }

            aiArea.globalAttackState = false;
            currentState = AIHelper.EnemyStates.Waypoints;
            
            increaseAttentionValueTimer = 0;
            
            if (MovementBehaviour && MovementBehaviour.points[currentWayPointIndex].point)
            {
                agent.SetDestination(MovementBehaviour.points[currentWayPointIndex].point.transform.position);
                StartMovement(false);
            }
            else
            {
                StopMovement();
            }
        }

        void LateUpdate()
        {
            // if (isHuman)
            // {
                var body = isHuman ? anim.GetBoneTransform(HumanBodyBones.Spine) : genericAvatarBody;
                if (body)
                {
                    var target = Vector3.zero;
                    var direction = Vector3.zero;
                    var anglesBetweenTargetAndBody = Vector2.zero;

                    if (currentMultiplayerTarget || aiArea.allPlayersInScene.Count > 0 && globalTarget && currentState == AIHelper.EnemyStates.Attack && canAttack && Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
                    {
                        if (!currentMultiplayerTarget)
                        {
                            var targetScript = closestTarget && Attacks[0].AttackType != AIHelper.AttackTypes.Fire && Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? closestTarget : globalTarget;

                            target = Attacks[0].AttackType != AIHelper.AttackTypes.Fire ? targetScript.BodyObjects.TopBody.position : targetScript.transform.position;
                        }
                        else
                        {
                            target = currentMultiplayerTarget.transform.position + Vector3.up;
                        }

                        direction = target - bodyDirectionObject.position;

                        anglesBetweenTargetAndBody = Helper.AngleBetween(direction, bodyDirectionObject);
                        
                        desiredBodyRotationAngle.x = anglesBetweenTargetAndBody.x;
                        desiredBodyRotationAngle.y = anglesBetweenTargetAndBody.y;

                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Covers Idle State") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Covers Left Rotate") &&
                            !anim.GetCurrentAnimatorStateInfo(0).IsName("Covers Movement") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")
                            && !anim.GetCurrentAnimatorStateInfo(0).IsName("Covers Right Rotate") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")
                            && !anim.GetCurrentAnimatorStateInfo(0).IsName("Find")  && !anim.GetCurrentAnimatorStateInfo(0).IsName("Damage Reaction") &&
                            !anim.GetCurrentAnimatorStateInfo(0).IsName("Grenades Reaction"))
                        {
                            if (Mathf.Abs(desiredBodyRotationAngle.x) > bodyRotationLimits.x || Mathf.Abs(desiredBodyRotationAngle.y) > bodyRotationLimits.y)
                            {
                                targetOutOfSight = true;
                                
                                currentBodyRotationLimits.x = Mathf.Lerp(currentBodyRotationLimits.x, 0, 1 * Time.deltaTime);
                                currentBodyRotationLimits.y =  Mathf.Lerp(currentBodyRotationLimits.y, 0, 1 * Time.deltaTime);
                            }
                            else
                            {
                                targetOutOfSight = false;
                                
                                currentBodyRotationLimits.x = Mathf.Lerp(currentBodyRotationLimits.x, bodyRotationLimits.x, 2 * Time.deltaTime);
                                currentBodyRotationLimits.y = Mathf.Lerp(currentBodyRotationLimits.y,bodyRotationLimits.y,  2 * Time.deltaTime);
                            }
                        }
                        else
                        {
                            currentBodyRotationLimits.x = Mathf.Lerp(currentBodyRotationLimits.x, 0, 1 * Time.deltaTime);
                            currentBodyRotationLimits.y =  Mathf.Lerp(currentBodyRotationLimits.y, 0, 1 * Time.deltaTime);
                        }
                    }
                    else
                    {
                        direction = directionObject.forward; 
                        
                        if (!currentMultiplayerTarget && globalTarget && Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
                        {
                            var targetScript = closestTarget && Attacks[0].AttackType != AIHelper.AttackTypes.Fire && Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? closestTarget : globalTarget;
                            target = Attacks[0].AttackType != AIHelper.AttackTypes.Fire ? targetScript.BodyObjects.TopBody.position : targetScript.transform.position;
                            direction = target - bodyDirectionObject.position;
                        }

                        anglesBetweenTargetAndBody = Helper.AngleBetween(direction, bodyDirectionObject);

                        desiredBodyRotationAngle.x = anglesBetweenTargetAndBody.x;
                        desiredBodyRotationAngle.y = anglesBetweenTargetAndBody.y;
                        
                        currentBodyRotationLimits.x = Mathf.Lerp(currentBodyRotationLimits.x, 0, 1 * Time.deltaTime);
                        currentBodyRotationLimits.y =  Mathf.Lerp(currentBodyRotationLimits.y, 0, 1 * Time.deltaTime);
                    }
                    
                    if (desiredBodyRotationAngle.x > currentBodyRotationLimits.x) desiredBodyRotationAngle.x = currentBodyRotationLimits.x;
                    else if (desiredBodyRotationAngle.x < -currentBodyRotationLimits.x) desiredBodyRotationAngle.x = -currentBodyRotationLimits.x;

                    if (desiredBodyRotationAngle.y > currentBodyRotationLimits.y) desiredBodyRotationAngle.y = currentBodyRotationLimits.y;
                    else if (desiredBodyRotationAngle.y < -currentBodyRotationLimits.y) desiredBodyRotationAngle.y = -currentBodyRotationLimits.y;
                    
                    
                    if (!multiplayerBot || currentMultiplayerTarget && inBattleZone)
                    {
                        if (isHuman)
                        {
                            body.RotateAround(bodyDirectionObject.position, Vector3.up, -desiredBodyRotationAngle.y);
                            body.RotateAround(bodyDirectionObject.position, bodyDirectionObject.TransformDirection(Vector3.right), -desiredBodyRotationAngle.x);
                        }
                        else
                        {
                            body.RotateAround(bodyDirectionObject.position, Vector3.up, -desiredBodyRotationAngle.y * bodyRotationSpeed * Time.deltaTime);
                            body.RotateAround(bodyDirectionObject.position, bodyDirectionObject.TransformDirection(Vector3.right), -desiredBodyRotationAngle.x * bodyRotationSpeed * Time.deltaTime);
                        }
                    }
                }
                // }
            
            if (currentUIManager && blipRawImage)
            {
                currentUIManager.SetBlip(transform, rotateBlipWithEnemy ? "positionAndRotation" : "positionOnly", null, blipRawImage);
            }

#if USK_MULTIPLAYER
            if(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;
#endif

            if (!multiplayerBot)
            {
                CalculateAttentionValueBasedOnVisiblePlayers();
                CalculateAttentionValueBasedOnAudiblePlayers();
                CalculateAttentionValueBasedOnNearPlayers();
            }
        }

        void ClearEmptyPlayers()
        {
            if (playersToRemove.Count > 0)
            {
                foreach (var player in playersToRemove)
                {
                    if (allPlayersInScene.Exists(player1 => player1.controller == player.controller))
                    {
                        allPlayersInScene.Remove(player);
                    }
                }
                
                playersToRemove.Clear();
            }
        }

        public void RestartEnemyMovement()
        {
            if(!agent) return;

            if (!agentIsStopped)
            {
                SetDest(agentDestination);
                StartMovement(false);
            }
        }

        void CalculateAttentionValueBasedOnVisiblePlayers()
        {
            if(opponentsDetectionType != AIHelper.OpponentsDetectionType.Vision && opponentsDetectionType != AIHelper.OpponentsDetectionType.All) return;
            
            foreach (var player in allPlayersSeenByEnemy)
            {
                if(!player.player) continue;
                
                var distance = Vector3.Distance(transform.position, player.player.transform.position);
                
                if (distance > 5)
                {
                    if (!player.seeDirectly)
                    {
                        if (player.seePlayerHips)
                        {
                            CharacterDetection(visionDetectionTime, allPlayersSeenByEnemy, "see");
                        }

                        if (player.seePlayerHead)
                        {
                            CharacterDetection(visionDetectionTime, allPlayersSeenByEnemy, "see");
                        }
                    }
                    else
                    {
                        if (player.seePlayerHips || player.seePlayerHead)
                        {
                            CharacterDetection(visionDetectionTime * 1.5f, allPlayersSeenByEnemy, "see");
                        }
                    }

                    //!!!
                    // if (!player.seePlayersHips && !player.seePlayersHead || InSmoke)
                    //     if(allPlayersSeenByEnemy.Contains(player))
                    //         allPlayersSeenByEnemy
                    // Players[0].seePlayer = false;
                }
                else
                {
                    if (player.seePlayerHips || player.seePlayerHead)
                    {
                        CharacterDetection(visionDetectionTime, allPlayersSeenByEnemy, "see");
                    }
                }
            }
        }

        void CalculateAttentionValueBasedOnAudiblePlayers()
        {
            if(opponentsDetectionType != AIHelper.OpponentsDetectionType.Hearing && opponentsDetectionType != AIHelper.OpponentsDetectionType.All) return;

            foreach (var player in allPlayersHeardByEnemy)
            {
                CharacterDetection(hearingDetectionTime, allPlayersHeardByEnemy, "hear");
            }
        }
        
        void CalculateAttentionValueBasedOnNearPlayers()
        {
            if(opponentsDetectionType != AIHelper.OpponentsDetectionType.CloseRange && opponentsDetectionType != AIHelper.OpponentsDetectionType.All) return;

            foreach (var player in allPlayersNearEnemy)
            { 
                CharacterDetection(rangeDetectionTime, allPlayersNearEnemy, "near");
            }
        }
        
        public void PlayDamageAnimation()
        {
            if (currentDamageAnimationTime > damageAnimationTimeout)
            {
                currentDamageAnimationTime = 0;
                
                if (DamageAnimations.Count > 0)
                {
                    currentDamageAnimationIndex = Random.Range(0, DamageAnimations.Count - 1);
                    
                    if (DamageAnimations.Contains(DamageAnimations[currentDamageAnimationIndex]))
                    {
                        ClipOverrides["_EnemyDamage"] = DamageAnimations[currentDamageAnimationIndex];
                        newController.ApplyOverrides(ClipOverrides);
                        
                        anim.SetTrigger("Taking Damage");
                        
                        // anim.CrossFade("Damage Reaction", 0.3f, 0);
                    }
                }
            }
        }

        void MultiplayerBotScanningProcess()
        {
            GameObject closestTarget = null;
            var closestDist = float.MaxValue;
            
            var detectionRange = distanceToSee;

            if (Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
                detectionRange = distanceToSee * attackDistancePercent / 100;
            
            foreach (var bot in aiArea.allBotsInMatch)
            {
                if (bot == null || bot == this || bot.health <= 0 || bot.multiplayerTeam != MultiplayerHelper.Teams.Null && bot.multiplayerTeam == multiplayerTeam) continue;
                
                var distance = Vector3.Distance(transform.position, bot.gameObject.transform.position);

                if (distance < detectionRange && distance < closestDist)
                {
                    closestDist = distance;
                    closestTarget = bot.gameObject;
                }
            }

            foreach (var player in aiArea.allPlayersInMatch)
            {
                if(!player) continue;
                
                if (player.health <= 0 || player.multiplayerTeam != MultiplayerHelper.Teams.Null && player.multiplayerTeam == multiplayerTeam) continue;
                
                var distance = Vector3.Distance(transform.position, player.gameObject.transform.position);
                
                if (distance < detectionRange && distance < closestDist)
                {
                    closestDist = distance;
                    closestTarget = player.gameObject;
                }
            }

            if (closestTarget)
            {
                notSeeTarget = AIHelper.IsObstacle(closestTarget.transform.position + Vector3.up, directionObject, visionHits);
                currentMultiplayerTarget = !notSeeTarget ? closestTarget : null;
            }
            else
            {
                currentMultiplayerTarget = null;
            }
        }

        void ScanningProcess()
        {
            if (aiArea.hasAnyPlayerInZone)
            {
                allPlayersSeenByEnemy.Clear();
                allPlayersNearEnemy.Clear();

                Controller closestPlayer = null;
                var closestDist = float.MaxValue;
                
                foreach (var player in allPlayersInScene)
                {
                    if (!player.player)
                    {
                        aiArea.ClearEmptyPlayer(player, true);
                        continue;
                    }

                    if (player.controller.health <= 0 || player.controller.currentNavMeshArea != aiArea.navMeshArea)
                    {
                        player.seePlayerHead = false;
                        player.seePlayerHips = false;
                        player.inAttentionZone = false;
                    }
                    else
                    {
                        var distance = Vector3.Distance(transform.position, player.player.transform.position);

                        if (opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
                        {
                            if (Vector3.Distance(player.player.transform.position, transform.position) <= detectionDistance)
                            {
                                if (player.controller.health > 0)
                                {
                                    canReduceAttentionValue = false;
                                    increaseAttentionValueTimer = 0;
                                    
                                    player.inAttentionZone = true;
                                    allPlayersNearEnemy.Add(player);
                                }
                            }
                            else
                            {
                                player.inAttentionZone = false;
                            }
                        }

                        var currentPoint = bodyDirectionObject ? bodyDirectionObject : directionObject;

                        if (opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
                        {
                            if (distance < distanceToSee)
                            {
                                player.seePlayerHips = !grenadeEffect && AIHelper.CheckRaycast(player.controller.BodyObjects.Hips, currentPoint, peripheralHorizontalAngle, centralHorizontalAngle, heightToSee, distanceToSee, currentState == AIHelper.EnemyStates.Attack, ref player.seeDirectly, ref player.isObstacle, visionHits, ref player.inSight);
                                player.seePlayerHead = !grenadeEffect && AIHelper.CheckRaycast(player.controller.BodyObjects.Head, currentPoint, peripheralHorizontalAngle, centralHorizontalAngle, heightToSee, distanceToSee, currentState == AIHelper.EnemyStates.Attack, ref player.seeDirectly, ref player.isObstacle, visionHits, ref player.inSight);
                                
                                if ((player.seePlayerHead || player.seePlayerHips) && player.controller.health > 0 && (currentState != AIHelper.EnemyStates.Attack && (!player.controller.inGrass || player.controller.isCrouch && distance < 2) || currentState == AIHelper.EnemyStates.Attack))
                                {
                                    canReduceAttentionValue = false;
                                    increaseAttentionValueTimer = 0;

                                    allPlayersSeenByEnemy.Add(player);
                                }

                                // vision debug
                                // var position = directionObject.position;
                                // Debug.DrawLine(position, player.controller.BodyObjects.Hips.position, player.seePlayersHips ? Color.red : Color.green);
                                // Debug.DrawLine(position, player.controller.BodyObjects.Head.position, player.seePlayersHead ? Color.red : Color.green);
                            }
                            else
                            {
                                player.seePlayerHips = false;
                                player.seePlayerHead = false;
                            }
                        }
                        else if (currentState== AIHelper.EnemyStates.Attack && (opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing || opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange))
                        {
                            if (distance < distanceToSee)
                            {
                                player.seePlayerHips = AIHelper.CheckRaycast(player.controller.BodyObjects.Hips, currentPoint, peripheralHorizontalAngle, centralHorizontalAngle, heightToSee, distanceToSee, currentState == AIHelper.EnemyStates.Attack, ref player.seeDirectly, ref player.isObstacle, visionHits, ref player.inSight);
                                
                                if (player.seePlayerHips && player.controller.health > 0)
                                {
                                    allPlayersSeenByEnemy.Add(player);
                                }
                            }
                            else
                            {
                                player.seePlayerHips = false;
                            }
                        }

                        if (currentState == AIHelper.EnemyStates.Attack && distance < distanceToSee)
                        {
                            if (distance < closestDist)
                            {
                                closestDist = distance;
                                closestPlayer = player.controller;
                            }
                        }
                    }

                    var _player = aiArea.allPlayersInScene.Find(opponent => opponent.controller == player.controller);

                    if (_player != null)
                    {
                        _player.seePlayerHips = player.seePlayerHips;
                        _player.seePlayerHead = player.seePlayerHead;
                        _player.hearPlayer = player.hearPlayer;
                        _player.inAttentionZone = player.inAttentionZone;
                    }
                }
                
                if (globalTarget)
                {
                    var dist = Vector3.Distance(transform.position, globalTarget.transform.position);

                    if (closestDist < 10 && dist > closestDist && closestPlayer != null)
                    {
                        closestTarget = closestPlayer;
                    }
                    else
                    {
                        closestTarget = null;
                    }
                }
            }
        }

        private void SetDest(Vector3 position)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(position);
            }
        }

        void CharacterDetection(float time, List<AIHelper.Opponent> activePlayers, string type)
        {
            IncreaseAttentionValue(currentState == AIHelper.EnemyStates.Warning ? time / 1.2f : time, activePlayers, type);
        }

        private void IncreaseAttentionValue(float time, List<AIHelper.Opponent> activePlayers, string type)
        {
            if(agent.isOnOffMeshLink) return;
            
            bool unknownPlayer;
            switch (currentState)
            {
                case AIHelper.EnemyStates.Waypoints:
                {
                    if (UseStates)
                    {
                        if (time > 0)
                        {
                            var additionalValue = 1 / time * Time.deltaTime;
                            
                            attentionValue += additionalValue;

                            canReduceAttentionValue = false;
                            increaseAttentionValueTimer = 0;

                            if (attentionValue > 1)
                            {
                                if (!aiArea.globalAttackState)
                                {
                                    attentionValue = 1;
                                    aiArea.GenerateCheckPoints(activePlayers, this);

                                    if (type == "see")
                                    {
                                        //sound: I saw something!
                                    }
                                    else if (type == "hear")
                                    {
                                        //sound: I heard something!
                                    }
                                    else if (type == "near")
                                    {
                                        //sound: He's beside me!
                                    }
                                }
                                else
                                {
                                    attentionValue = 2;
                                    foreach (var player in activePlayers.Where(player => !aiArea.allKnowPlayersInZone.Exists(_player => _player.controller == player.controller)))
                                    {
                                        aiArea.allKnowPlayersInZone.Add(player);
                                    }

                                    aiArea.GenerateAttackPoints(this);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (time > 0)
                        {
                            attentionValue = 2;
                            aiArea.allKnowPlayersInZone.AddRange(activePlayers);
                            aiArea.GenerateAttackPoints(this);
                        }
                        // sound: attack!!!
                    }

                    break;
                }
                case AIHelper.EnemyStates.Warning:
                {
                    var additionalValue = 1 / time * Time.deltaTime;
                    
                    attentionValue += additionalValue;
                    
                    if (attentionValue >= 2)
                    {
                        attentionValue = 2;
                        aiArea.GenerateAttackPoints(this);
                        
                        //sound: here he is, on the attack!

                    }
                    else if (attentionValue >= 1)
                    {
                        unknownPlayer = false;
                    
                        foreach (var player in activePlayers.Where(player => !aiArea.allKnowPlayersInZone.Exists(player1 => player1.controller == player.controller)))
                        {
                            unknownPlayer = true;
                        }

                        if (unknownPlayer)
                        {
                            aiArea.GenerateCheckPoints(activePlayers, this);

                            if (type == "see")
                            {
                                //sound: I saw something!
                            }
                            else if (type == "hear")
                            {
                                //sound: I heard something!
                            }
                        }
                    }

                    break;
                }
                case AIHelper.EnemyStates.Attack:

                    unknownPlayer = false;
                    
                    foreach (var player in activePlayers)
                    {
                        if (!aiArea.allKnowPlayersInZone.Exists(player1 => player1.controller == player.controller))
                        {
                            if (player.controller.health > 0)
                            {
                                unknownPlayer = true;
                                aiArea.allKnowPlayersInZone.Add(player);
                            }
                        }
                    }

                    if (unknownPlayer)
                    {
                        aiArea.ManagePlayersBetweenOpponents();
                    }

                    break;
            }
        }

        public void CalculateAttentionValueBasedOnDamageTaken(Controller player)
        {
            lastOpponentWhoHitThisEnemy = new AIHelper.Opponent {controller = player, player = player.gameObject};
            damageTakenFromPlayer = true;
        }

        void ManageValuesWhenGetDamage(AIHelper.Opponent opponent)
        {
            if (opponent == null || opponent.controller && opponent.controller.currentNavMeshArea != aiArea.navMeshArea)
            {
                lastOpponentWhoHitThisEnemy = null;
                damageTakenFromPlayer = false;
                return;
            }

            if (damageTakenFromPlayer)
            {
                increaseAttentionValueTimer = 0;
                canReduceAttentionValue = false;

                if (UseStates)
                {
                    attentionValue = Mathf.Lerp(attentionValue, currentState == AIHelper.EnemyStates.Waypoints ? 1.5f : 2.5f, 6 * Time.deltaTime);
                }

                switch (currentState)
                {
                    case AIHelper.EnemyStates.Waypoints:
                    {
                        if (UseStates)
                        {
                            if (attentionValue >= 1)
                            {
                                if (!aiArea.globalAttackState)
                                {
                                    damageTakenFromPlayer = false;
                                    aiArea.GenerateCheckPoints(opponent, this);
                                }
                                else
                                {
                                    damageTakenFromPlayer = false;
                                    attentionValue = 2;
                                    
                                    if(!aiArea.allKnowPlayersInZone.Exists(_player => _player.controller == opponent.controller))
                                        aiArea.allKnowPlayersInZone.Add(opponent);
                                    
                                    aiArea.GenerateAttackPoints(this);
                                }

                                // sound: They shot at me! I'm under attack!
                            }
                        }
                        else
                        {
                            attentionValue = 2;
                            aiArea.allKnowPlayersInZone.Add(opponent);
                            aiArea.GenerateAttackPoints(this);
                            
                            // sound: attack!!!
                        }
                        break;
                    }

                    case AIHelper.EnemyStates.Warning:
                    {
                        if (attentionValue >= 2)
                        {
                            aiArea.GenerateAttackPoints(this);
                            damageTakenFromPlayer = false;
                            // sound: Here he is! Attack! I need help
                            
                        }
                        else if (attentionValue >= 1)
                        {
                            var unknownPlayer = !aiArea.allKnowPlayersInZone.Exists(player1 => player1.controller == opponent.controller);
                    
                            if (unknownPlayer)
                            {
                                aiArea.GenerateCheckPoints(opponent, this);
                    
                                // sound: Somebody shot at me! I'm under attack! Help!
                            }
                        }
                        break;
                    }
                    case AIHelper.EnemyStates.Attack:

                        if (!aiArea.allKnowPlayersInZone.Exists(player1 => player1.controller == opponent.controller ))
                        {
                            if (opponent.controller.health > 0)
                            {
                                aiArea.allKnowPlayersInZone.Add(opponent);
                                aiArea.ManagePlayersBetweenOpponents();
                            }
                        }
                        break;
                }
            }
        }

        public void Damage(float damage, string attackType, Helper.ActorID attackerActorNumber = null)
        {
            TakingDamage(damage, attackType, true, attackerActorNumber);
        }

        public void TakingDamage(float damage, string attackType, bool syncInMultiplayer, Helper.ActorID attackerActorNumber = null)
        {
            if (health <= 0) return;

            if (oneShotOneKill)
                damage = (int)health + 50;
            
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

            if (attackerActorNumber != null && attackerActorNumber.type != "instanceID")
            {
                if (multiplayerBot)
                {
                    if (!opponentsWhoAttackedThisAI.Exists(actor => actor.type == attackerActorNumber.type && actor.actorID == attackerActorNumber.actorID))
                        opponentsWhoAttackedThisAI.Add(attackerActorNumber);
                }

#if USK_MULTIPLAYER
                if (syncInMultiplayer && !PhotonNetwork.IsMasterClient)
                    aiArea.eventsManager.SyncDamage(damage, this, aiArea, attackerActorNumber, attackType);
#endif
            }

#if USK_MULTIPLAYER
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;
#endif
            health -= damage;
            ControlHealth(attackType, attackerActorNumber);
            
            if (!multiplayerBot && attackerActorNumber != null)
            {
                Controller attackerController = null;

#if USK_MULTIPLAYER
                if (attackerActorNumber.type != "instanceID")
                {
                    var attacker = aiArea.allPlayersInScene.Find(opponent => opponent.controller.CharacterSync.photonView.OwnerActorNr == attackerActorNumber.actorID);
                    if (attacker != null)
                        attackerController = attacker.controller;
                }
                else
#endif
                {
                    var attacker = aiArea.allPlayersInScene.Find(opponent => opponent.controller.gameObject.GetInstanceID() == attackerActorNumber.actorID);
                    
                    if (attacker != null)
                        attackerController = attacker.controller;
                }
                
                CalculateAttentionValueBasedOnDamageTaken(attackerController);
            }
            
        }

        public void TakingDamageFromBodyColliders(GameObject attacker, string attackType)
        {
            if (attacker.GetComponent<Controller>()) //&& aiArea.hasAnyPlayerInZone)
            {
                var attackerController = attacker.GetComponent<Controller>();

                if (MultiplayerHelper.CanDamageInMultiplayer(attackerController, this))
                {
                    var inventoryManager = attackerController.inventoryManager;
                    var weaponController = attackerController.inventoryManager.WeaponController;

                    if (weaponController && (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee || weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame))
                    {
                        Damage(weaponController.Attacks[weaponController.currentAttack].weapon_damage * (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame ? Time.deltaTime : 1), attackType,
#if USK_MULTIPLAYER
                            PhotonNetwork.InRoom ? new Helper.ActorID {actorID = weaponController.Controller.CharacterSync.photonView.OwnerActorNr, type = "player"} :
#endif                            
                                new Helper.ActorID{actorID = weaponController.Controller.gameObject.GetInstanceID(), type = "instanceID"}

                            );
                        
                        // CalculateAttentionValueBasedOnDamageTaken(attackerController);
                    }
                    else if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].fistAttack)
                    {
                        Damage(inventoryManager.FistDamage, attackType,
                            
#if USK_MULTIPLAYER
                            PhotonNetwork.InRoom ? new Helper.ActorID{actorID = inventoryManager.Controller.CharacterSync.photonView.OwnerActorNr, type = "player"} :
#endif
                                new Helper.ActorID{actorID = inventoryManager.Controller.gameObject.GetInstanceID(), type = "instanceID"}

                            );
                        
                        // CalculateAttentionValueBasedOnDamageTaken(attackerController);
                    }

                    if(!multiplayerBot)
                        PlayDamageAnimation();
                }
            }
            else if (attacker.GetComponent<AIController>())
            {
                if(attacker.gameObject.GetInstanceID() == gameObject.GetInstanceID()) return;
                
                var attackerController = attacker.GetComponent<AIController>();
                
                if (MultiplayerHelper.CanDamageInMultiplayer(attackerController, this))
                {
                    Damage(attackerController.Attacks[0].Damage * (attackerController.Attacks[0].AttackType == AIHelper.AttackTypes.Fire ? Time.deltaTime : 1), "melee",
#if USK_MULTIPLAYER
                         PhotonNetwork.InRoom ? new Helper.ActorID{actorID =  attackerController.photonView.ViewID, type = "ai"} :
#endif
                        null
                        );
                }
            }
        }

        void CoverBehaviour()
        {
            if (!anim.GetBool("Covers State"))
            {
                canAttack = false;
                anim.SetBool("Covers State", true);
            }
            
            timerBehindCover += Time.deltaTime;
            
            if (timerBehindCover > timeBehindCover && !anim.GetCurrentAnimatorStateInfo(1).IsName("Reload"))
            {
                timerBehindCover = 0;

                if (!canAttack)
                {
                    timeBehindCover = Random.Range(7, 10);
                    canAttack = true;
                }
                else
                {
                    timeBehindCover = Random.Range(3, 5);
                    canAttack = false;

                    if (Attacks[0].UseReload && Attacks[0].CurrentAmmo <= Attacks[0].InventoryAmmo / 2)
                    {
                        Reload();
                    }
                }
            }
            
            if(canAttack)
                RotateToTarget();
        }

        // this function is used in animation events
        public void PlayStepSound(float volume)
        {
            var hit = new RaycastHit();

            var layerMask = ~ (LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Character") | LayerMask.GetMask("Head") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
            
            if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 100, layerMask))
            {
                var surface = hit.collider.GetComponent<Surface>();
				
                if (FeetAudioSource && surface && surface.EnemyFootstepsSounds.Length > 0)
                    CharacterHelper.PlayStepSound(surface, FeetAudioSource, enemyType, volume, "enemy");
            }
        }

        void AttackState()
        {
            if (!grenadeEffect)
            {
                AttackBehaviour();

#if USK_MULTIPLAYER
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                    return;
#endif

                if (!multiplayerBot)
                    AttackMovement();
            }
            else
            {
                if(!agentIsStopped)
                    StopMovement();
            }
        }

        void AttackBehaviour()
        {
#if USK_MULTIPLAYER
            var visualiseOnly = PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
#else
            var visualiseOnly = false;
#endif
            
            if (canAttack || currentMultiplayerTarget && (Attacks[0].AttackType != AIHelper.AttackTypes.Melee || inAttackZone))// if the target is in the enemy's Attack Zone
            {
                if (!visualiseOnly)
                {
                    if (!anim.GetBool("Aim"))
                        anim.SetBool("Aim", true);
                }

                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    rateOfAttack += Time.deltaTime;
                
                if (rateOfAttack >= Attacks[0].RateOfAttack)
                {
                    if (Attacks[0].AttackType != AIHelper.AttackTypes.Melee && targetOutOfSight || !visualiseOnly && notSeeTarget) return;

                    rateOfAttack = 0;

                    switch (Attacks[0].AttackType)
                    {
                        case AIHelper.AttackTypes.Melee:
                        {
                            if (Attacks[0].MeleeAttackAnimations.Count > 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetBool("Attack"))
                            {
                                AnimationClip animationClip;

                                var index = Random.Range(0, Attacks[0].MeleeAttackAnimations.Count);
                                animationClip = Attacks[0].MeleeAttackAnimations.Contains(Attacks[0].MeleeAttackAnimations[index]) ? Attacks[0].MeleeAttackAnimations[index] : Attacks[0].MeleeAttackAnimations.Find(clip => clip != null);

                                if (!WeaponsHelper.HasAnimationCollidersEvent(animationClip)) // if there are no MeleeColliders event on the Attack animation, activate those colliders manually
                                    aiAttack.MeleeAttack(visualiseOnly);
                                
                                ClipOverrides["_EnemyAttack"] = animationClip;
                                newController.ApplyOverrides(ClipOverrides);
                                anim.SetBool("Attack", true);
                            }

                            break;
                        }
                        case AIHelper.AttackTypes.Bullets:
                        case AIHelper.AttackTypes.Rockets:
                        case AIHelper.AttackTypes.Fire:

                            if (Attacks[0].UseReload && Attacks[0].CurrentAmmo > 0 && !anim.GetCurrentAnimatorStateInfo(1).IsName("Reload") && !anim.GetBool("Reload") || !Attacks[0].UseReload)
                            {
                                aiAttack.Attack(Attacks[0]);

                                if (Attacks[0].AttackType != AIHelper.AttackTypes.Fire)
                                {
                                    if (isHuman) //&& !Attacks[0].HandsAttackAnimation.isLooping)
                                        anim.Play("Attack", 1, 0);
                                }
                                else
                                {
                                    anim.SetBool("Attack", true);
                                }
                            }
                            
                            if (!visualiseOnly && Attacks[0].UseReload && Attacks[0].CurrentAmmo <= 0)
                            {
                                if (anim.GetBool("Covers State"))
                                {
                                    canAttack = false;
                                }

                                if(reloadRoutine == null)
                                    reloadRoutine = StartCoroutine(ReloadTimeout());
                            }
                            
                            break;
                    }
                }
                else
                {
                    if(anim.GetBool("Attack"))
                        anim.SetBool("Attack", false);

                    if (!visualiseOnly)
                    {
                        if (Attacks[0].AttackType == AIHelper.AttackTypes.Fire)
                            aiAttack.DisableFireColliders();
                    }
                }
            }
            else
            {
                if(anim.GetBool("Attack"))
                    anim.SetBool("Attack", false);

                if (!visualiseOnly)
                {
                    if (anim.GetBool("Aim"))
                        anim.SetBool("Aim", false);

                    if (Attacks[0].AttackType == AIHelper.AttackTypes.Fire)
                        aiAttack.DisableFireColliders();
                }
            }
        }

        IEnumerator ReloadTimeout()
        {
            yield return new WaitForSeconds(1);
            
            Reload();
        }

        void Reload()
        {
            reloadRoutine = null;

            anim.SetBool("Reload", true);
            anim.SetBool("Attack", false);
            Attacks[0].CurrentAmmo = Attacks[0].InventoryAmmo;
            aiAttack.StartCoroutine(aiAttack.ReloadTimeout());
        }

        void AttackMovement()
        {
            if(!globalTarget) return;
            
            if(runWhileAttacking && !anim.GetBool("Run"))
                anim.SetBool("Run", true);

            inAttackZone = Vector3.Distance(transform.position, globalTarget.transform.position) < distanceToSee * attackDistancePercent / 100;
            var targetScript = closestTarget && Attacks[0].AttackType != AIHelper.AttackTypes.Fire && Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? closestTarget : globalTarget;
            var targetPlayer = allPlayersInScene.Find(player => player.controller == targetScript);

            if (targetPlayer != null)
            {
                notSeeTarget = !targetPlayer.seePlayerHead && !targetPlayer.seePlayerHips && targetPlayer.isObstacle; //&& !targetPlayer.inAttentionZone;
            }

            if (useCovers)
            {
                if (currentCoverPoint == null || !currentCoverPoint.pointTransform)
                {
                    aiArea.SetSuitableCoverPoint(this);
                }
                else
                {
                    if (Vector3.Distance(transform.position, globalTarget.transform.position) > distanceToSee * attackDistancePercent / 100)
                    {
                        currentCoverPoint.aIController = null;
                        currentCoverPoint = null;
                        return;
                    }
                    
                    if (agent.isOnNavMesh)
                        agent.SetDestination(currentCoverPoint.pointTransform.position);

                    var dist = Vector3.Distance(transform.position, currentCoverPoint.pointTransform.position);

                    if (dist > 10)
                    {
                        aiArea.SetSuitableCoverPoint(this);
                    }

                    if (dist <= 1)
                    {
                        if (!agentIsStopped) StopMovement();

                        // if(canAttack && (notSeeTarget || !inAttackZone)) RandomMovementOnMap();
                        // else 
                        CoverBehaviour();
                    }
                    else
                    {
                        var targetTransform = closestTarget ? closestTarget.transform : globalTarget.transform;

                        anim.SetFloat("DistanceToTarget", 10);

                        if (Vector3.Distance(transform.position, targetTransform.position) < distanceToSee * attackDistancePercent / 200)
                        {
                            if (dist > 7)
                            {
                                anim.SetBool("Covers State", false);
                            }

                            if (!anim.GetBool("Covers State"))
                            {
                                if (!canAttack) canAttack = true;
                                
                                AllSidesMovement(true);
                                RotateToTarget();
                            }
                            else
                            {
                                if (canAttack) canAttack = false;
                            }
                        }
                        else
                        {
                            if (canAttack) canAttack = false;
                            anim.SetBool("Covers State", dist <= 7 && useCovers);
                            AllSidesMovement(false);
                        }
                    }
                }
            }
            else
            {
                RandomMovementOnMap();
            }
        }

#if USK_ADVANCED_MULTIPLAYER
        void MultiplayerBotMovement()
        {
            if(!advancedRoomManager || advancedRoomManager.battleZones.Count == 0) return;

            if (advancedRoomManager.matchIsOver)
            {
                StopMovement();
                return;
            }
            
            var battleZonePositions = advancedRoomManager.battleZones[currentBattleZone].transform.position;
            var battleZoneScale = advancedRoomManager.battleZones[currentBattleZone].transform.localScale;
            
            var zoneMinPoint = new Vector3(battleZonePositions.x - battleZoneScale.x / 2, 0, battleZonePositions.z - battleZoneScale.z / 2);
            var zoneMaxPoint = new Vector3(battleZonePositions.x + battleZoneScale.x / 2, 0, battleZonePositions.z + battleZoneScale.z / 2);
            var position = transform.position;

            if (position.x > zoneMinPoint.x && position.z > zoneMinPoint.z && position.x < zoneMaxPoint.x && position.z < zoneMaxPoint.z)
            {
                inBattleZone = true;
            }
            else if (position.x < (zoneMinPoint.x - battleZoneScale.x / 3) || position.z < (zoneMinPoint.z - battleZoneScale.z / 3) || position.x > zoneMaxPoint.x + (battleZoneScale.x / 3) || position.z > zoneMaxPoint.z + (battleZoneScale.z / 3))
            {
                inBattleZone = false;
            }
            
            anim.SetFloat("DistanceToTarget", 10);
            
            if(!anim.GetBool("Run"))
                anim.SetBool("Run", true);

            if (currentMultiplayerTarget)
            {
                inAttackZone = Vector3.Distance(transform.position, currentMultiplayerTarget.transform.position) < distanceToSee * attackDistancePercent / 100;
                
                if (Vector3.Distance(transform.position, currentMultiplayerTarget.transform.position) < distanceToSee * attackDistancePercent / 200 || inBattleZone)
                {
                    AllSidesMovement(true);
                    RotateToTarget();
                }
                else
                {
                    AllSidesMovement(false);
                }
            }
            else
            {
                inAttackZone = false;
                
                if (inBattleZone)
                {
                    AllSidesMovement(true);

                    if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
                        RotateToDirection(currentPointToMove.forward);
                }
                else
                {
                    AllSidesMovement(false);
                }
            }

            if (!inBattleZone)
            {
                if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
                {
                    currentPointToMove.gameObject.SetActive(false);
                    // Destroy(currentPointToMove.gameObject);
                }
                
                if(agentIsStopped)
                    StartMovement(false);
            
                if (agent.isOnNavMesh)
                    agent.SetDestination(battleZonePositions);
            }
            else
            {
                if (!currentPointToMove || !currentPointToMove.gameObject.activeInHierarchy)
                {
                    SetRandomPoint();
                }
                else
                {
                    if (Vector3.Distance(transform.position, currentPointToMove.position) <= 1)
                    {
                        currentPointToMove.gameObject.SetActive(false);
                    }
                }
            }
        }
#endif

        public void RandomMovementOnMap()
        {
            if (!globalTarget)
                return;

            if (!observer && (!inAttackZone || notSeeTarget))
            {
                if (inAttackZone && notSeeTarget)
                {
                    //RoteToTarget();
                    
                    MoveToPlayer();
                }
                else if(!inAttackZone)
                {
                     MoveToPlayer();
                }
                
                if (allSidesMovement)
                {
                    if (inAttackZone && !notSeeTarget)
                    {
                        AllSidesMovement(true);
                        RotateToTarget();
                    }
                    else
                    {
                        AllSidesMovement(false);
                    }
                }
                else
                {
                    AllSidesMovement(false);
                }

                if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
                {
                    currentPointToMove.gameObject.SetActive(false);
                }

                if (canAttack)
                    canAttack = false;
            }
            else
            {
                canAttack = true;
                
                RotateToTarget();
                
                if (agent.updateRotation)
                    agent.updateRotation = false;

                if (observer) return;

                if (!allSidesMovement)
                {
                    if (!agentIsStopped)
                        StopMovement();
                }
                else
                {
                    if (!currentPointToMove || !currentPointToMove.gameObject.activeInHierarchy)
                    {
                        SetRandomPoint();
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, currentPointToMove.position) <= 1)
                        {
                            currentPointToMove.gameObject.SetActive(false);
                            // Destroy(currentPointToMove.gameObject);
                        }
                        else
                        {
                            AllSidesMovement(true);
                        }
                    }
                }
            }
        }


        void MoveToPlayer()
        {
            if(anim.GetBool("Covers State"))
                anim.SetBool("Covers State", false);
            
            if(agentIsStopped)
                StartMovement(false);
            
            if (agent.isOnNavMesh)
                agent.SetDestination(globalTarget.transform.position);
        }

        void RotateToDirection(Vector3 direction)
        {
            desiredRotation = Quaternion.LookRotation(direction);
            desiredRotationAngle = Helper.AngleBetween(directionObject.forward, direction);
            
            anim.SetFloat("Angle", Mathf.Lerp(anim.GetFloat("Angle"), desiredRotationAngle, 3 * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * 2);
        }

        void RotateToTarget()
        {
            if(Attacks[0].AttackType == AIHelper.AttackTypes.Melee && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) return;
            
            Transform target;
            if (!multiplayerBot) target = closestTarget ? closestTarget.transform : globalTarget.transform;
            else target = currentMultiplayerTarget.transform;
            
            var lookPos = target.position - directionObject.position;
            lookPos.y = 0;
            desiredRotation = Quaternion.LookRotation(lookPos);
            desiredRotationAngle = Helper.AngleBetween(directionObject.forward, lookPos);
                
            anim.SetFloat("Angle", Mathf.Lerp(anim.GetFloat("Angle"), desiredRotationAngle, 3 * Time.deltaTime));

            //!!! var speed = Mathf.Abs(desiredRotationAngle) > 5 ? turnSpeed : 10;
            
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * turnSpeed);
        }

        void SetRandomPoint()
        {
            // var addMovePointFunctionCount = 0;
             // var direction = Vector3.zero;
             // var distance = 3f;
             
             // if ((Attacks[0].AttackType == AIHelper.AttackTypes.Fire || Attacks[0].AttackType == AIHelper.AttackTypes.Melee) && !multiplayerBot)
             // {
             //     var angle = Random.Range(-90, 90);
             //     distance = distanceToSee * attackDistancePercent / 150;
             //     direction = (Quaternion.AngleAxis(angle, Vector3.up) * globalTarget.DirectionObject.forward).normalized;
             // }
             // else
             // {
             //     direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
             // }

             var point = Vector3.zero;
            
             // if (!multiplayerBot) point = Attacks[0].AttackType != AIHelper.AttackTypes.Fire && Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? transform.position : globalTarget.transform.position;
             // else point = transform.position;

             Transform target = null;

            if (multiplayerBot)
            {
                if(currentMultiplayerTarget) target = currentMultiplayerTarget.transform;
            }
            else
            {
                if(globalTarget) target = globalTarget.transform;
            }

            if (target != null)
            {
                currentPointToMove = AIHelper.RandomNavmeshPoint(target,10, "attack", aiArea.navMeshArea, currentPointToMove, transform);
            }
            else
            {
                if (!currentPointToMove)
                    currentPointToMove = new GameObject("Point to check").transform;

// #if USK_ADVANCED_MULTIPLAYER
// //                 currentPointToMove.position = CharacterHelper.GetRandomPointInRectangleZone(advancedRoomManager.battleZones[currentBattleZone].transform);
//                  currentPointToMove.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
// #endif
            }
            if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
            {
                if (agent.isOnNavMesh)
                    agent.SetDestination(currentPointToMove.position);
            }
            else
            {
                anim.SetBool("AllSidesMovement", false);
                StopMovement();
            }
            
        }

        void WayPointsMovement()
        {
            if(anim.GetBool("Run"))
                anim.SetBool("Run", false);
            
            anim.SetFloat("Angle", 0);
            
            if (!observer)
            {
                if (MovementBehaviour && MovementBehaviour.points.Count > 0)
                {
                    if (Vector3.Distance(MovementBehaviour.points[currentWayPointIndex].point.transform.position, transform.position) <= 2)
                    {
                        if (!isNextAction)
                        {
                            ChoiceNextAction();
                            isNextAction = true;
                        }
                    }
                    else
                    {
                        isNextAction = false;
                    }
                }
            }
            else
            {
                ObserverBehavior();
            }
        }

        void ObserverBehavior()// warning state
        {
            if (monitoringTimer > 10)
            {
                monitoringTimer = 0;
                currentMonitoringDirection = Quaternion.Euler(0, -90, 0) * currentMonitoringDirection;
            }

            desiredRotation = Quaternion.LookRotation(currentMonitoringDirection);
            desiredRotationAngle = Helper.AngleBetween(transform.forward, currentMonitoringDirection);
            anim.SetFloat("Angle", desiredRotationAngle);
            var speed = Mathf.Abs(desiredRotationAngle) > 5 ? 3 : 7;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * speed);
        }

        void ChoiceNextAction()
        {
            switch (MovementBehaviour.points[currentWayPointIndex].action)
            {
                case Helper.NextPointAction.NextPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.NextPoint);
                    break;
                case Helper.NextPointAction.RandomPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.RandomPoint);
                    break;
                case Helper.NextPointAction.ClosestPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.ClosestPoint);
                    break;
                case Helper.NextPointAction.Stop:
                    StopMovement();
                    break;
            }
            

            if (MovementBehaviour.points[currentWayPointIndex].waitTime > 0 && MovementBehaviour.points[currentWayPointIndex].action != Helper.NextPointAction.Stop)
            {
                isMovementPause = true;
                StopMovement();
            }
            else
            {
                SetDest(MovementBehaviour.points[currentWayPointIndex].point.transform.position);
                // if(agent.isOnNavMesh)
                //     agent.SetDestination(currentWaypointPosition);
            }

#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                aiArea.eventsManager.SyncWayPoints(this, -1);
            }
#endif
        }

        private void CalculationNextPointIndex(Helper.NextPointAction currentAction)
        {
            if (MovementBehaviour.points.Count < 2)
            {
                StopMovement();
                return;
            }

            var curIndex = currentWayPointIndex;//MovementBehaviour.points.FindIndex(behavior => behavior.point == MovementBehaviour.points[currentWayPointIndex].point);
            
            curIndex++;
                    
            if (curIndex >= MovementBehaviour.points.Count)
                curIndex = 0;
            
            switch (currentAction)
            {
                case Helper.NextPointAction.NextPoint:
                    previousWayPointIndex = currentWayPointIndex;
                    currentWayPointIndex = curIndex;
                    // currentWayPoint = MovementBehaviour.points[curIndex];
                    
                    break;
                case Helper.NextPointAction.RandomPoint:

                    var allBehavioursExceptCurrent = MovementBehaviour.points.FindAll(point => point.point != MovementBehaviour.points[currentWayPointIndex].point && point.point != MovementBehaviour.points[previousWayPointIndex].point);
                    var newIndex = Random.Range(0, allBehavioursExceptCurrent.Count);
                    previousWayPointIndex = currentWayPointIndex;
                    // previousWayPoint = currentWayPoint;
                    
                    var curPoint = allBehavioursExceptCurrent.Count > 0 ? allBehavioursExceptCurrent[newIndex] : MovementBehaviour.points[curIndex];

                    currentWayPointIndex = MovementBehaviour.points.IndexOf(curPoint);
                    
                    break;
                case Helper.NextPointAction.ClosestPoint:

                    var _curIndex = currentWayPointIndex;//MovementBehaviour.points.FindIndex(behavior => behavior.point == MovementBehaviour.points[currentWayPointIndex].point);
                    var _previousIndex = previousWayPointIndex;//MovementBehaviour.points.FindIndex(behavior => behavior.point == MovementBehaviour.points[previousWayPointIndex].point);
                    var _newIndex = AIHelper.GetNearestPoint(MovementBehaviour.points, transform.position, _curIndex, _previousIndex);
                    previousWayPointIndex = currentWayPointIndex;
                    
                    // previousWayPoint = currentWayPoint;
                    
                    curPoint = MovementBehaviour.points.Count > 2 ? MovementBehaviour.points[_newIndex] : MovementBehaviour.points[curIndex];
                    currentWayPointIndex = MovementBehaviour.points.IndexOf(curPoint);
                    
                    break;
            }

            // currentWaypointPosition = currentWayPoint.point.transform.position;
        }

        public void StopMovement()
        {
            agentIsStopped = true;
            agent.updateRotation = false;

            anim.SetBool("Move", false);
            anim.SetBool("Run", false);

            if (isMovementPause) //&& State == AIHelper.EnemyStates.Waypoints) 
                StartCoroutine(MovePause());
        }

        private void AllSidesMovement(bool enable)
        {
            if (enable)
            {
                AIHelper.AllSidesMovement(Helper.AngleBetween(directionObject.forward, agent.velocity), anim);
                
                if (agentIsStopped)
                    StartMovement(false);

                if (agent.updateRotation)
                    agent.updateRotation = false;
                
                if (!anim.GetBool("AllSidesMovement"))
                    anim.SetBool("AllSidesMovement", true);
            }
            else
            {
                if(!agent.updateRotation)
                    agent.updateRotation = true;
                    
                if (anim.GetBool("AllSidesMovement"))
                    anim.SetBool("AllSidesMovement", false);
            }
            
        }
        
        public void StartMovement(bool coversMovement)
        {
            anim.SetBool("Attack", false);
            anim.SetBool("Covers State", coversMovement);
            anim.SetBool("AllSidesMovement", false);
            anim.SetBool("Move", true);
            anim.SetBool("Find", false);
            agent.updateRotation = true;
            agentIsStopped = false;
        }

        public IEnumerator MovePause()
        {
            yield return new WaitForSeconds(MovementBehaviour.points[currentWayPointIndex].waitTime + 2);
            
            // if(State == AIHelper.EnemyStates.Warning)
            //     StopCoroutine(MovePause());
                
            isMovementPause = false;

            agent.updateRotation = true;
            agentIsStopped = false;
            
            if (agent.isOnNavMesh && (!currentPointToMove || !currentPointToMove.gameObject.activeInHierarchy))
            {
                SetDest(MovementBehaviour.points[currentWayPointIndex].point.transform.position);
                // agent.SetDestination(currentWaypointPosition);
            }

            anim.SetBool("Move", true);

            StopCoroutine("MovePause");
        }

        public void SetNewPointToCheck()
        { 
            if(currentPointToMove)
                Destroy(currentPointToMove.gameObject);
            
            currentPointToMove = AIHelper.GetNearestPoint(pointsToCheckForThisEnemy, transform.position);
                 
            SetDest(currentPointToMove.position);
        }

        IEnumerator PlayFindAnimationTimeout()
        {
            var lenght = PlayFindAnimation();
            yield return new WaitForSeconds(lenght);
            
            CalculateNextWarningAction();

            anim.SetBool("Find", false);
            agent.updateRotation = true;
        }

        float PlayFindAnimation()
        {
            var animationClip = GetFindAnimation();

            agent.updateRotation = false;
            anim.SetBool("Find", true);
            
            if (animationClip != null) return animationClip.length;

            return 5;
        }

        IEnumerator EnableHealthBarTimeout()
        {
            yield return new WaitForSeconds(1);
            
            opponentForLocalPlayer = true;
            
#if USK_ADVANCED_MULTIPLAYER
            opponentForLocalPlayer = (MultiplayerHelper.Teams)PhotonNetwork.LocalPlayer.CustomProperties["pt"] != multiplayerTeam || multiplayerTeam == MultiplayerHelper.Teams.Null;

#endif
            var blipColor = opponentForLocalPlayer ? opponentBlipColor : teammateBlipColor;
            var healthBarColor = opponentForLocalPlayer ? opponentHealthBarColor : teammateHealthBarColor;
           
            if (healthBarValue)
                healthBarValue.color = healthBarColor;
            
            currentUIManager.allMinimapImages.Add(UIHelper.CreateNewBlip(currentUIManager, ref blipRawImage, blipMainTexture, blipColor, "AI Blip", false));

#if USK_ADVANCED_MULTIPLAYER
            if (advancedRoomManager && currentUIManager.advancedMultiplayerGameRoom.TimerBeforeMatch.MainObject.activeInHierarchy)
            {
                blipRawImage.gameObject.SetActive(false);
            }
#endif
        }

        private AnimationClip GetFindAnimation()
        {
            AnimationClip animationClip = null;

            if (FindAnimations.Count > 0)
            {
                var index = Random.Range(0, FindAnimations.Count - 1);

                SetFindAnimation(index);
            }

            return animationClip;
        }

        public void SetFindAnimation(int index)
        {
            if (FindAnimations.Contains(FindAnimations[index]))
            {
                var animationClip = FindAnimations[index];

                if (animationClip == null) return;
                
                ClipOverrides["_EnemyFind"] = animationClip;
                newController.ApplyOverrides(ClipOverrides);
                
#if USK_MULTIPLAYER
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    var em = aiArea.eventsManager;
                    em.ChangeEnemyFindAnimation(photonView.ViewID, index, aiArea);
                }
#endif
            }
        }

        void WarningState()
        {
            anim.SetFloat("Angle", 0);
            
            if (pointsToCheckForThisEnemy.Count > 0)
            {
                if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
                {
                    var dist = Vector3.Distance(currentPointToMove.position, transform.position);
                    
                    if (dist <= 3)
                    {
                        // checkedPoint = true;

                        if (!agentIsStopped)
                        {
                            StopMovement();
                            StartCoroutine(PlayFindAnimationTimeout());
                        }
                    }
                }
            }
            else
            {
                if (observer)
                {
                    ObserverBehavior();
                }
                else
                {
                    WayPointsMovement();
                }
            }

            // }
        }

        void CalculateNextWarningAction()
        {
            pointsToCheckForThisEnemy.Remove(currentPointToMove);
            aiArea.pointsToCheck.Remove(currentPointToMove);

            if (currentPointToMove && currentPointToMove.gameObject.activeInHierarchy)
            {
                // Destroy(currentPointToMove.gameObject);
                currentPointToMove.gameObject.SetActive(false);

                if (aiArea.pointsToCheck.Count == 0)
                {
                    // sound: Everything is clean ... in places! Don't relax!
                }
            }

            if (pointsToCheckForThisEnemy.Count > 0)
            {
                SetNewPointToCheck();
                StartMovement(false);
            }
            else
            {
                if (currentState == AIHelper.EnemyStates.Warning)
                {
                    if (aiArea.pointsToCheck.Count == 0)
                    {
                        currentState = AIHelper.EnemyStates.Waypoints;
                    }
                    
                    if (agent.isOnNavMesh)
                    {
                        if (MovementBehaviour && currentWayPointIndex != -1)
                        {
                            SetDest(MovementBehaviour.points[currentWayPointIndex].point.transform.position);
                            StartMovement(false);
                        }
                        else StopMovement();
                    }
                }
            }
            
#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                if (aiArea.eventsManager)
                {
                    aiArea.eventsManager.SyncAllPoints(aiArea.allEnemiesInZone, -1, aiArea);
                }
            }
#endif
        }

        public void ControlHealth(string attackType, Helper.ActorID attackerActorNumber = null)
        {
            if (health <= 0)
            {
                Death(attackType, attackerActorNumber);

                if (blipRawImage && blipDeathTexture)
                {
                    blipRawImage.texture = blipDeathTexture;
                    
                    var color = blipRawImage.color;
                    color = new Color(color.r, color.g, color.b, 1);
                    blipRawImage.color = color;
                }
            }
        }

        public void Death(string attackType, Helper.ActorID killerActor = null)
        {
#if USK_MULTIPLAYER
            var curActor = new Helper.ActorID {actorID = photonView ? photonView.ViewID : -1, type = "ai"};
#endif
            
            if (aiArea)
            {
                // only on master client
                if (!aiArea.multiplayerMatch)
                {
#if USK_MULTIPLAYER
                    if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
#endif
                    if(aiArea.communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther)
                        StartCoroutine(SomeoneSeeCorps());

                    aiArea.allEnemiesInZone.Remove(this);

                    if (SaveManager.Instance)
                    {
                        if(SaveManager.Instance.sceneSaveData.aiSaveData.Exists(ai => ai.id == enemyID))
                        {
                            SaveManager.Instance.sceneSaveData.aiSaveData.Find(ai => ai.id == enemyID).healthValue = 0;
                        }
                        else
                        {
                            SaveManager.Instance.sceneSaveData.aiSaveData.Add(new SaveManager.AISaveData {id = enemyID, healthValue = 0});
                        }
                    }
                    
#if USK_MULTIPLAYER
                    if (PhotonNetwork.IsMasterClient && attackType != "")
                    {
                        if (aiArea.eventsManager)
                            aiArea.eventsManager.EnemyDeath(this, aiArea);
                    }
#endif
                }
                else
                {
                    StopMovement();

#if USK_ADVANCED_MULTIPLAYER
                    if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Survive)
                    {
                        AMHelper.CalculatePlaceInSurvivalMode(advancedRoomManager, this);
                        advancedRoomManager.UpdateAvatarsInSurvivalGame();
                    }

                    advancedRoomManager.UpdatePlayersUIList(false);
                    
                    if (PhotonNetwork.IsMasterClient && attackType != "")
                    {
                        deaths++;

                        if (aiArea.eventsManager)
                        {
                            aiArea.eventsManager.EnemyDeath(this, aiArea);
                            aiArea.eventsManager.AllocateScoreAfterSomeoneDead(curActor, killerActor, attackType, opponentsWhoAttackedThisAI);
                        }
                    }
#endif
                }
            }

            
#if USK_ADVANCED_MULTIPLAYER
            if (advancedRoomManager)
            {
                AMHelper.ShowKillDeathStats(advancedRoomManager, killerActor, curActor, attackType);
            }
#endif

            var GO = gameObject;
            
            if (multiplayerBot)
            {
                GO = Instantiate(gameObject, transform.position, transform.rotation);
                GO.name = Helper.CorrectName(gameObject.name) + " | Ragdoll";
                
                gameObject.SetActive(false);
                
                var scripts = GO.GetComponents<MonoBehaviour>();
                
                foreach(var script in scripts)
                {
                    if(script.GetType() != typeof(AIController))
                        Destroy(script);
                }
            }

            AIHelper.DisableAllComponentsAfterDeath(GO.GetComponent<AIController>());

#if USK_ADVANCED_MULTIPLAYER
            if (multiplayerBot)
            {
                if(advancedRoomManager.matchTarget != MultiplayerHelper.MatchTarget.Survive)
                    advancedRoomManager.StartCoroutine(RebindAfterDeathTimeout());
            }
#endif
        }

        IEnumerator SomeoneSeeCorps()
        {
            while (true)
            {
                var someoneSeeCorp = false;
                var someoneNearCorp = false;
                
#if USK_MULTIPLAYER
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
#endif
                    foreach (var enemy in aiArea.allEnemiesInZone)
                    {
                        if (enemy.currentState == AIHelper.EnemyStates.Waypoints)
                        {
                            if ((enemy.opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || enemy.opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && enemy.visionDetectionTime > 0)
                            {
                                var seeCorp = AIHelper.CheckRaycast(transform, enemy.directionObject, enemy.peripheralHorizontalAngle, enemy.centralHorizontalAngle, enemy.heightToSee, enemy.distanceToSee, visionHits);

                                //Sound (play ones): We have a corpse!!!
                                if (seeCorp)
                                    someoneSeeCorp = true;
                            }

                            if ((enemy.opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || enemy.opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && enemy.rangeDetectionTime > 0)
                            {
                                if (Vector3.Distance(transform.position, enemy.transform.position) < enemy.detectionDistance)
                                {
                                    someoneNearCorp = true;
                                }
                            }
                        }
                        
                    }

                if (someoneSeeCorp || someoneNearCorp)
                {
                    aiArea.ManagePlayersBetweenOpponents();
                    break;
                }
                
                yield return 0;
            }
        }

#if USK_ADVANCED_MULTIPLAYER
        IEnumerator RebindAfterDeathTimeout()
        {
            if (!advancedRoomManager) yield return 0;
            
            yield return new WaitForSeconds(advancedRoomManager.restartGameAfterPlayerDeathTimeout);

            AMHelper.RebindAIAfterDeath(this, false);
        }
#endif

        public void PlayAttackSound()
        {
            if(weapon) weapon.GetComponent<AudioSource>().PlayOneShot(Attacks[0].AttackAudio);
            else if(audioSource) audioSource.PlayOneShot(Attacks[0].AttackAudio);
        }
        

#if USK_MULTIPLAYER
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(!parametersAreInitialised) return;

            if (stream.IsWriting)
            {
                if (!multiplayerBot)
                {
                    stream.SendNext(closestTarget ? closestTarget.CharacterSync.photonView.ViewID : -1);
                    stream.SendNext(canAttack);
                    stream.SendNext(Attacks[0].CurrentAmmo);
                    stream.SendNext(attentionValue);
                    stream.SendNext(currentSpeed);
                    stream.SendNext((int) currentState);

                    stream.SendNext(isNextAction);
                    stream.SendNext(canReduceAttentionValue);
                    stream.SendNext(agentIsStopped);
                    stream.SendNext(currentMonitoringDirection);
                    stream.SendNext(agent.destination);
                    
                    stream.SendNext(health);
                }
                else
                {
                    
#if USK_ADVANCED_MULTIPLAYER
                    stream.SendNext(currentMultiplayerTarget ? currentMultiplayerTarget.GetComponent<PhotonView>().ViewID : -1);

                    stream.SendNext(Attacks[0].CurrentAmmo);
                    stream.SendNext(currentSpeed);
                    stream.SendNext(agentIsStopped);
                    stream.SendNext(agent.destination);
                    
                    stream.SendNext(kills);
                    stream.SendNext(score);
                    
                    if (advancedRoomManager.pointsCount == 3)
                    {
                        stream.SendNext(captureAPoint);
                        stream.SendNext(captureBPoint);
                        stream.SendNext(captureCPoint);
                    }
                    else
                    {
                        stream.SendNext(captureHardPoint);
                    }

                    stream.SendNext(deaths);
                    stream.SendNext(health);
#endif
                }
            }
            else
            {
                if (!multiplayerBot)
                {
                    var id = (int) stream.ReceiveNext();

                    
                    // if (closestTargetCurId != id)
                    // {
                        if (id == -1)
                        {
                            closestTarget = null;
                            // closestTargetCurId = id;
                        }
                        else
                        {
                            var target = allPlayersInScene.Find(player => player != null && player.controller && player.controller.CharacterSync && player.controller.CharacterSync.photonView.ViewID == id);

                            closestTarget = target != null ? target.controller : null;
                        }
                    // }

                    
                    canAttack = (bool) stream.ReceiveNext();
                    Attacks[0].CurrentAmmo = (float) stream.ReceiveNext();
                    attentionValue = (float) stream.ReceiveNext();
                    currentSpeed = (float) stream.ReceiveNext();

                    var state = (int) stream.ReceiveNext();
                    currentState = (AIHelper.EnemyStates) state;

                    isNextAction = (bool) stream.ReceiveNext();
                    canReduceAttentionValue = (bool) stream.ReceiveNext();
                    agentIsStopped = (bool) stream.ReceiveNext();
                    currentMonitoringDirection = (Vector3) stream.ReceiveNext();
                    agentDestination = (Vector3) stream.ReceiveNext();
                    
                    health = (float) stream.ReceiveNext();
                }
                else
                {
                    
#if USK_ADVANCED_MULTIPLAYER
                    var id = (int) stream.ReceiveNext();

                    if (id == -1)
                    {
                        currentMultiplayerTarget = null;
                    }
                    else
                    {
                        GameObject target;

                        var playerTarget = aiArea.allPlayersInMatch.Find(player => player != null && player.CharacterSync && player.CharacterSync.photonView.ViewID == id);

                        var botTarget = aiArea.allBotsInMatch.Find(ai => ai != null && ai.photonView && ai.photonView.ViewID == id);

                        target = playerTarget ? playerTarget.gameObject : botTarget ? botTarget.gameObject : null;

                        currentMultiplayerTarget = target;
                    }

                    Attacks[0].CurrentAmmo = (float) stream.ReceiveNext();
                    currentSpeed = (float) stream.ReceiveNext();
                    agentIsStopped = (bool) stream.ReceiveNext();
                    agentDestination = (Vector3) stream.ReceiveNext();

                    kills = (int) stream.ReceiveNext();
                    score = (int) stream.ReceiveNext();

                    if (advancedRoomManager.pointsCount == 3)
                    {
                        captureAPoint = (bool) stream.ReceiveNext();
                        captureBPoint = (bool) stream.ReceiveNext();
                        captureCPoint = (bool) stream.ReceiveNext();
                    }
                    else
                    {
                        captureHardPoint = (bool) stream.ReceiveNext();
                    }

                    deaths = (int) stream.ReceiveNext();
                    health = (float) stream.ReceiveNext();
#endif
                }
            }
        }
#endif

        #region Gizmos
        
#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var position = directionObject.position;
            
            Handles.zTest = CompareFunction.Greater;
            
            if (currentInspectorTab == 0 && currentBehaviourInspectorTab == 0)
            {
                if (directionObject)
                {
                    Handles.color = new Color32(255, 150, 0, 30);
                    Handles.ArrowHandleCap(0, position, directionObject.transform.rotation, 1, EventType.Repaint);
                    Handles.SphereHandleCap(0, position, Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
            
            Handles.zTest = CompareFunction.Less;
            
            if (currentInspectorTab == 0 && currentBehaviourInspectorTab == 0)
            {
                if (directionObject)
                {
                    Handles.color = new Color32(255, 150, 0, 255);
                    Handles.ArrowHandleCap(0, position, directionObject.transform.rotation, 1, EventType.Repaint);
                    Handles.SphereHandleCap(0, position, Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            Handles.zTest = CompareFunction.Greater;
        
            Handles.color = new Color32(0, 255, 255, 30);
            
            if (currentInspectorTab == 3)
            {
                var position1 = transform.position;
                
                Handles.DrawWireDisc(position1, Vector3.up, navMeshAgentParameters.radius);  
                Handles.DrawWireDisc(position1 + navMeshAgentParameters.height * Vector3.up, Vector3.up, navMeshAgentParameters.radius);
                
                Handles.DrawLine(position1 + Vector3.left * navMeshAgentParameters.radius, position1 + Vector3.left * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.right * navMeshAgentParameters.radius, position1 + Vector3.right * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.forward * navMeshAgentParameters.radius, position1 + Vector3.forward * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.back * navMeshAgentParameters.radius, position1 + Vector3.back * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
            }
            
            Handles.zTest = CompareFunction.Less;
            Handles.color = new Color32(0, 255, 255, 255);
            
            if (currentInspectorTab == 3)
            {
                var position1 = transform.position;
                Handles.DrawWireDisc(position1, Vector3.up, navMeshAgentParameters.radius);  
                Handles.DrawWireDisc(position1 + navMeshAgentParameters.height * Vector3.up, Vector3.up, navMeshAgentParameters.radius);    
                
                Handles.DrawLine(position1 + Vector3.left * navMeshAgentParameters.radius, position1 + Vector3.left * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.right * navMeshAgentParameters.radius, position1 + Vector3.right * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.forward * navMeshAgentParameters.radius, position1 + Vector3.forward * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
                Handles.DrawLine(position1 + Vector3.back * navMeshAgentParameters.radius, position1 + Vector3.back * navMeshAgentParameters.radius + navMeshAgentParameters.height * Vector3.up);
            }

            if(!directionObject || currentInspectorTab != 0 && currentInspectorTab != 1)
                return;
            
            var dir = directionObject.forward;
        
            var xLeftDir = Quaternion.Euler(0, peripheralHorizontalAngle / 2, 0) * dir;
            var xRightDir = Quaternion.Euler(0, -peripheralHorizontalAngle / 2, 0) * dir;
            
            var addXLeftDir = Quaternion.Euler(0, centralHorizontalAngle / 2, 0) * dir;
            var addXRightDir = Quaternion.Euler(0, -centralHorizontalAngle / 2, 0) * dir;
        
            var position = directionObject.position;
            var up = transform.up;
            var curPos = transform.position;
        
            Handles.zTest = CompareFunction.Greater;
            
            Color32 color;
           
            if (topInspectorTab == 0)
            {
                color = UseStates ? new Color32(255, 200, 0, 30) : new Color32(0, 255, 0, 30);
            }
            else
            {
                color = new Color32(255, 255, 255, 30);
            }
        
            if((opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && (currentInspectorTab == 0 && currentBehaviourInspectorTab == 0 || currentInspectorTab == 1) ||
               (opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing) && currentInspectorTab == 1)
                DrawArea(position, xRightDir, xLeftDir, dir, distanceToSee, heightToSee, peripheralHorizontalAngle, color);
            
            if (UseStates && (opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && currentInspectorTab == 0 && currentBehaviourInspectorTab == 0)
            {
                DrawArea(position, addXRightDir, addXLeftDir, dir, distanceToSee, heightToSee, centralHorizontalAngle, new Color32(0, 255, 0, 50));
            }

            if (currentInspectorTab == 0 && (opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && currentBehaviourInspectorTab == 0)
            {
                Handles.color = new Color32(0, 255, 255, 30);
                Handles.DrawSolidDisc(curPos + Vector3.up, Vector3.up, detectionDistance);
            }
            
            if (currentInspectorTab == 1)
            {
                if (opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
                {
                    Handles.color = new Color32(255, 255, 0, 30);
                    Handles.DrawSolidArc(position, up, dir, peripheralHorizontalAngle / 2, distanceToSee * attackDistancePercent / 100);
                    Handles.DrawSolidArc(position, up, dir, -peripheralHorizontalAngle / 2, distanceToSee * attackDistancePercent / 100);
                }
                else
                {
                    // color = new Color32(255, 255, 0, 30);
                    
                    Handles.color = new Color32(255, 255, 0, 30);
                    Handles.DrawSolidArc(position, up, dir, peripheralHorizontalAngle / 2, distanceToSee);
                    Handles.DrawSolidArc(position, up, dir, -peripheralHorizontalAngle / 2, distanceToSee);
                    
                    // DrawArea(position, xRightDir, xLeftDir, dir, distanceToSee, heightToSee, peripheralHorizontalAngle, color);
                }
            }
        
            Handles.zTest = CompareFunction.Less;
            
            if (currentInspectorTab == 0)
            {
                color = UseStates ? new Color32(255, 200, 0, 255) : new Color32(0, 255, 0, 255);
            }
            else
            {
                color = new Color32(255, 255, 255, 255);
            }            
            
            if((opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && (currentInspectorTab == 0 && currentBehaviourInspectorTab == 0 || currentInspectorTab == 1) ||
               (opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing) && currentInspectorTab == 1)
                DrawArea(position, xRightDir, xLeftDir, dir, distanceToSee, heightToSee, peripheralHorizontalAngle, color);
            
            if (UseStates && (opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && currentInspectorTab == 0 && currentBehaviourInspectorTab == 0)
            {
                DrawArea(position, addXRightDir, addXLeftDir, dir, distanceToSee, heightToSee, centralHorizontalAngle, new Color32(0, 255, 0, 255));
            }

            if (currentInspectorTab == 0 && (opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || opponentsDetectionType == AIHelper.OpponentsDetectionType.All) && currentBehaviourInspectorTab == 0)
            {
                Handles.color = new Color32(0, 255, 255, 70);
                Handles.DrawSolidDisc(curPos + Vector3.up, Vector3.up, detectionDistance);
            }

            if (currentInspectorTab == 1)
            {
                // var newHeight = distanceToSee * attackDistancePercent / 100 * Mathf.Tan(Mathf.Abs(Helper.AngleBetween((position + xLeftDir * distanceToSee - up * heightToSee / 2) - position, directionObject).x) * Mathf.Deg2Rad);

                
                if (opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
                {
                    Handles.color = new Color32(255, 255, 0, 70);
                    Handles.DrawSolidArc(position, up, dir, peripheralHorizontalAngle / 2, distanceToSee * attackDistancePercent / 100);
                    Handles.DrawSolidArc(position, up, dir, -peripheralHorizontalAngle / 2, distanceToSee * attackDistancePercent / 100);
                }
                else
                {
                    
                    Handles.color = new Color32(255, 255, 0, 70);
                    Handles.DrawSolidArc(position, up, dir, peripheralHorizontalAngle / 2, distanceToSee);
                    Handles.DrawSolidArc(position, up, dir, -peripheralHorizontalAngle / 2, distanceToSee);
                    
                    // color = new Color32(255, 255, 0, 255);
                    // DrawArea(position, xRightDir, xLeftDir, dir, distanceToSee, heightToSee, peripheralHorizontalAngle, color);
                }
                
                // DrawArea(position, xRightDir, xLeftDir, dir, distanceToSee * attackDistancePercent / 100, newHeight * 2, peripheralHorizontalAngle, new Color32(255, 255, 0, 255));
            }
        }

        void DrawArea(Vector3 position, Vector3 xRightDir, Vector3 xLeftDir, Vector3 dir, float distance, float height, float angle, Color32 color)
        {
            var tempColor = color;
            
            tempColor.a = (byte)(color.a / 4);
            Handles.color = tempColor;

            var up = transform.up;
            
            Handles.DrawSolidArc(position, up, dir, angle / 2, distance);
            Handles.DrawSolidArc(position, up, dir, -angle / 2, distance);

            Handles.color = color;

            var thickness = 5f;
            
            DrawLine(position, position + xLeftDir * distance - up * height / 2, color, thickness);
            DrawLine(position, position + xRightDir * distance - up * height / 2, color, thickness);

            Handles.DrawWireArc(position - up * height / 2, up, dir, angle / 2, distance);
            Handles.DrawWireArc(position - up * height / 2, up, dir, -angle / 2, distance);

            DrawLine(position + xLeftDir * distance, position + xLeftDir * distance + up * height / 2, color, thickness);
            DrawLine(position + xRightDir * distance, position + xRightDir * distance + up * height / 2, color, thickness);

            DrawLine(position + xLeftDir * distance, position + xLeftDir * distance - up * height / 2, color, thickness);
            DrawLine(position + xRightDir * distance, position + xRightDir * distance - up * height / 2, color, thickness);

            DrawLine(position, position + xLeftDir * distance + up * height / 2, color, thickness);
            DrawLine(position, position + xRightDir * distance + up * height / 2, color, thickness);

            Handles.DrawWireArc(position + up * height / 2, up, dir, angle / 2, distance);
            Handles.DrawWireArc(position + up * height / 2, up, dir, -angle / 2, distance);
        }

        void DrawLine(Vector3 pos1, Vector3 pos2, Color32 color, float thickness)
        {
            Handles.DrawBezier(pos1, pos2, pos1, pos2, color, null, thickness);
        }

#endif
        
        #endregion
    }
}




