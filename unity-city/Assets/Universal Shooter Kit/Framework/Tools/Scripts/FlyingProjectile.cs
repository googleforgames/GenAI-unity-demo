// GercStudio
// © 2018-2020

using System.Collections;
using System.Linq;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class FlyingProjectile : MonoBehaviour
    {
        [HideInInspector][Range(1, 50)] public float Speed = 10;

       public Controller characterOwner;
        [HideInInspector] public AIController aiOwner;

        [HideInInspector] public Transform explosion;
        [HideInInspector] public Transform directionObject;
        [HideInInspector] private Transform permanentFlyingDirection;

        public Vector3 TargetPoint;
        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public Vector3 m_CurrentVelocity = new Vector3(1.0f, 0.0f, 0.0f);
        
        public int ownerId;
        [HideInInspector] public float damage = 10;
        [HideInInspector] public float GrenadeExplosionTime;
        private int layerMask;
        
        [HideInInspector] public string ownerName;

        [HideInInspector] public bool applyGravity;
        [HideInInspector] public bool ExplodeWhenTouchGround;
        [HideInInspector] public bool isTracer;
        [HideInInspector] public bool isRocket;
        [HideInInspector] public bool isBullet;
        [HideInInspector] public bool isGrenade;
        [HideInInspector] public bool stickOnObject;
        [HideInInspector] public bool ApplyForce;
        [HideInInspector] public bool FlashExplosion;
        [HideInInspector] public bool isMultiplayerWeapon;

        private Vector3 lastObjPosition;

        [HideInInspector] public Texture WeaponImage;
        
        // [HideInInspector] public ParticleSystem[] Particles = {null};
        

        private float timeout;
        private float lifetimeout;
        private float _scatter;
        
        private bool isTopDown;
        private bool touchGround;

        private Vector3 direction;
        
        private Rigidbody _rigidbody;

        void Start()
        {
            if (isGrenade)
                _rigidbody = GetComponent<Rigidbody>();

            if (!isRocket && !isBullet) return;
            
            lastObjPosition = transform.position;

            if (directionObject)
            {
                permanentFlyingDirection = new GameObject("OriginalCameraPosition").transform;
                permanentFlyingDirection.hideFlags = HideFlags.HideInHierarchy;
                permanentFlyingDirection.position = directionObject.position;
                var eulerAngles = directionObject.eulerAngles;
                permanentFlyingDirection.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            }
            
            if (characterOwner) ownerId = characterOwner.gameObject.GetInstanceID();
            else if (aiOwner) ownerId = aiOwner.gameObject.GetInstanceID();

            if (TargetPoint != Vector3.zero)
            {
                direction = TargetPoint - transform.position;
                direction.Normalize();
            }
            
            layerMask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
        }

        private void Update()
        {
            timeout += Time.deltaTime;
        }

        void FixedUpdate()
        {
            if (isRocket || isBullet)
            {
                lifetimeout += Time.deltaTime;

                transform.Rotate(Vector3.forward, 10, Space.Self);

                if (TargetPoint != Vector3.zero)
                {
                    transform.LookAt(TargetPoint);
                    transform.position = Vector3.MoveTowards(transform.position, TargetPoint + direction * 100, Speed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector3.forward * (Speed * Time.deltaTime), permanentFlyingDirection);
                }

                if (Physics.Linecast(lastObjPosition, transform.position, out var hit, layerMask))
                {
                    if (hit.transform.root.gameObject.GetInstanceID() != ownerId && hit.transform.root.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                    {
                        if (isRocket) Explosion();
                        else BulletDamage(hit, direction);
                    }
                }

                lastObjPosition = transform.position;

                if (lifetimeout > 10)
                {
                    if(isRocket) Explosion();
                    else Destroy(gameObject);
                }

            }
            else if(isTracer)
            {
                transform.LookAt(TargetPoint);
                transform.position = Vector3.MoveTowards(transform.position, TargetPoint, Speed * Time.deltaTime);
            }
            else if (isGrenade)
            {
                var mask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
                var objects = Physics.OverlapSphere(transform.position, 0.1f, mask);
                
                
                if (objects.Length > 0 && objects.ToList().Any(obj =>
                {
                    Transform root;
                    return (root = obj.transform.root).gameObject.GetInstanceID() != ownerId && root.gameObject.GetInstanceID() != gameObject.GetInstanceID() &&
                           (timeout > 1 && root.gameObject.GetInstanceID() == characterOwner.gameObject.GetInstanceID() || root.gameObject.GetInstanceID() != characterOwner.gameObject.GetInstanceID());
                }))
                {
                    touchGround = true;
                    
                    if (!stickOnObject && _rigidbody)
                    {
                        _rigidbody.useGravity = true;
                        _rigidbody.isKinematic = false;
                    }
                }
                else
                {
                    if (!touchGround)
                    {
                        var position = transform.position;
                        var gravity = applyGravity ? Physics.gravity.y : 0;
                        ProjectileHelper.UpdateProjectile(ref position, ref m_CurrentVelocity, gravity, Time.deltaTime);
                        transform.position = position;
                    }
                }
            }
        }

        public IEnumerator GrenadeFlying()
        {
            yield return new WaitForSeconds(GrenadeExplosionTime);
            Explosion();
            StopCoroutine("GrenadeFlying");
        }

        public void Explosion()
        {
            if (explosion)
            {
                var _explosion = Instantiate(explosion, transform.position, transform.rotation);

                if (!isMultiplayerWeapon)
                {
#if USK_EMERALDAI_INTEGRATION
                    if (_explosion.gameObject.layer == LayerMask.NameToLayer("Smoke"))
                    {
                        var interactionScript = _explosion.gameObject.AddComponent<USKIneractionWithEmeraldAI>();
                        interactionScript.colliderType = USKIneractionWithEmeraldAI.ColliderType.Smoke;
                    }
#endif
                    
                    var script = _explosion.gameObject.AddComponent<Explosion>();
                    script.startPosition = startPosition;
                    script.damage = damage;
                    
                    if(characterOwner) script.characterOwner = characterOwner;
                    else if (aiOwner) script.aiOwner = aiOwner;
                    
                    script.ownerName = ownerName;
                    script.applyForce = ApplyForce;
                    if (WeaponImage) script.weaponImage = WeaponImage;
                    script.instanceId = gameObject.GetInstanceID();
                }

                StopCoroutine(GrenadeFlying());

                if (isRocket)
                {
                    if (permanentFlyingDirection)
                        Destroy(permanentFlyingDirection.gameObject);
                }
            }
            else
            {
                if(!FlashExplosion)
                    Debug.Log("(Weapon Controller) <color=yellow>Missing component</color>: [Explosion].", gameObject);
            }

            if (FlashExplosion)
            {
                var findArea = Physics.OverlapSphere(transform.position, 20);
                
                foreach (var obj in findArea)
                {
                    if (obj.gameObject.GetComponent<InventoryManager>())
                    {
                        if (Helper.canSeeObject(gameObject, obj.gameObject.GetComponent<InventoryManager>().Controller.CameraController.Camera))
                        {
                            FlashEffect(obj);
                        }
                    }

                    if (obj.transform.root.gameObject.GetComponent<AIController>())
                    {
                        var script = obj.transform.root.gameObject.GetComponent<AIController>();
                        
                        var seeFlash = AIHelper.CheckRaycast(transform, script.directionObject, script.peripheralHorizontalAngle, script.centralHorizontalAngle, script.heightToSee, script.distanceToSee, script.visionHits);

                        if (seeFlash)
                        {
                            script.grenadeEffect = true;

                            if (script.aiAttack.flashGrenadeEffectTimeout == null)
                            {
                                script.aiAttack.flashGrenadeEffectTimeout = script.aiAttack.StartCoroutine(script.aiAttack.FlashGrenadeEffectDuration());
                            }
                        }
                    }

#if USK_EMERALDAI_INTEGRATION
                    if (obj.transform.root.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
                    {
                        var script = obj.transform.root.gameObject.GetComponent<EmeraldAI.EmeraldAISystem>();
                        
                        var layerMask = ~ (LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
                       
                        if (!Physics.Linecast(transform.position + Vector3.up, script.transform.position, out var hit, layerMask))
                        {
                            if (script.gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>())
                            {
                                var em = script.gameObject.GetComponent<EmeraldAI.EmeraldAIEventsManager>();

                                var interactionScript = !script.gameObject.GetComponent<USKIneractionWithEmeraldAI>() ? script.gameObject.AddComponent<USKIneractionWithEmeraldAI>() : script.gameObject.GetComponent<USKIneractionWithEmeraldAI>();
                                
                                if (interactionScript.colliderType == USKIneractionWithEmeraldAI.ColliderType.Null)
                                {
                                    if (em.GetPlayerRelation() != EmeraldAI.EmeraldAISystem.RelationType.Friendly)
                                    {
                                        interactionScript.relationType = em.GetPlayerRelation();
                                        em.SetPlayerRelation(EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType.Friendly);
                                        em.ClearTarget();
                                        em.StopMovement();
                                        interactionScript.colliderType = USKIneractionWithEmeraldAI.ColliderType.Enemy;
                                        interactionScript.grenadeEffectDeactivationTimeout = 5;
                                    }
                                    else
                                    {
                                        DestroyImmediate(interactionScript);
                                    }
                                }
                                else
                                {
                                    interactionScript.grenadeEffectDeactivationTimer = 0;
                                }
                            }
                        }
                    }
#endif
                }
            }
                
            Destroy(gameObject);
        }

        private void BulletDamage(RaycastHit hit, Vector3 direction)
        {
            if(characterOwner)
                WeaponsHelper.CheckBulletRaycast(hit, characterOwner.inventoryManager.WeaponController);
            else if(aiOwner)
                AIHelper.CheckBulletRaycast(aiOwner, hit, direction);

            if (permanentFlyingDirection)
                Destroy(permanentFlyingDirection.gameObject);

            Destroy(gameObject);
        }

        void FlashEffect(Collider obj)
        {
            if(obj.gameObject.GetComponent<Controller>().isRemoteCharacter)
                return;
            
            var manager = obj.gameObject.GetComponent<InventoryManager>();
            manager.flashTimeout = 0;

            if (obj.gameObject.GetComponent<Controller>().thisCamera.GetComponent<Motion>())
            {
                var motion = obj.gameObject.GetComponent<Controller>().thisCamera.GetComponent<Motion>();
                motion.shutterAngle = 360;
                motion.sampleCount = 10;
                motion.frameBlending = 1;
            }
            
            if(obj.gameObject.GetComponent<Controller>().isRemoteCharacter)
                return;
            
            var controller = obj.gameObject.GetComponent<Controller>();
           
            if(controller.UIManager.CharacterUI.flashPlaceholder)
                controller.UIManager.CharacterUI.flashPlaceholder.color = new Color(1, 1, 1, 1);
            
            controller.UIManager.CharacterUI.flashPlaceholder.gameObject.SetActive(true);

            if (!controller.thisCamera.GetComponent<Motion>())
            {
                controller.thisCamera.AddComponent<Motion>();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isTracer || isRocket || isBullet) return;
            
            if (ExplodeWhenTouchGround && (!other.transform.root.gameObject.GetComponent<Controller>() || characterOwner.gameObject.GetInstanceID() != other.transform.root.gameObject.GetInstanceID()))
            {
                Explosion();
            }
        }
    }
}




