using System;
using System.Collections.Generic;
using System.Linq;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{

    public class Explosion : MonoBehaviour
    {
        [HideInInspector] public float radius = 2;
        [HideInInspector] public float force = 100;
        [HideInInspector] public float time = 1;

        [HideInInspector] public float damage;
        [HideInInspector] public int instanceId;
        
        [HideInInspector] public string ownerName;
        [HideInInspector] public bool applyForce;
        [HideInInspector] public Controller characterOwner;
        [HideInInspector] public AIController aiOwner;
        [HideInInspector] public Vector3 startPosition;

        [HideInInspector] public Texture weaponImage;

        private List<int> charactersIds = new List<int>();
        private List<int> enemiesIds = new List<int>();

        private bool anyDamage;
        private AIArea aiManager;
        
#if USK_DESTROYIT_INTEGRATION
        // private Dictionary<DestroyIt.ChipAwayDebris, float> affectedChipAwayDebris = new Dictionary<DestroyIt.ChipAwayDebris, float>();
        // private Dictionary<DestroyIt.Destructible, DestroyIt.ExplosiveDamage> affectedDestructibles = new Dictionary<DestroyIt.Destructible, DestroyIt.ExplosiveDamage>();
        
        public List<int> destroyedObjects = new List<int>();
#endif
        private void Awake()
        {
            aiManager = FindObjectOfType<AIArea>();
        }

        void Start()
        {
            ExplosionProcess();
        }

        void ExplosionProcess()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, radius);

            foreach (var collider in hitColliders)
            {
                //if an ai received damage
                if (collider.transform.root.GetComponent<AIController>())
                {
                    var root = collider.transform.root;
                    var damagedAIController = root.gameObject.GetComponent<AIController>();

                    if (enemiesIds.All(id => id != root.gameObject.GetInstanceID()))
                    {
                        enemiesIds.Add(root.gameObject.GetInstanceID());

                        if (characterOwner)
                        {
                            if (MultiplayerHelper.CanDamageInMultiplayer(characterOwner, damagedAIController))
                            {
                                damagedAIController.Damage(damage, "explosion",
#if USK_MULTIPLAYER
                                    PhotonNetwork.InRoom
                                        ? new Helper.ActorID {actorID = characterOwner.CharacterSync.photonView.OwnerActorNr, type = "player"}
                                        :
#endif
                                        new Helper.ActorID {actorID = characterOwner.gameObject.GetInstanceID(), type = "instanceID"}
                                );

                                if (!damagedAIController.multiplayerBot)
                                {
                                    damagedAIController.PlayDamageAnimation();
                                }
                            }
                        }
#if USK_MULTIPLAYER
                        //if ai attacks another ai
                        else if (aiOwner && aiOwner.photonView)
                        {
                            if (damagedAIController.multiplayerBot)
                            {
                                if (MultiplayerHelper.CanDamageInMultiplayer(aiOwner, damagedAIController))
                                {
                                    damagedAIController.Damage(damage, "explosion",

                                        PhotonNetwork.InRoom ? new Helper.ActorID {actorID = aiOwner.photonView.ViewID, type = "ai"} : null
                                        // new Helper.ActorID{actorID = characterOwner.gameObject.GetInstanceID(), type = "instanceID"}
                                    );
                                }
                            }
                        }
#endif
                    }
                }

#if USK_EMERALDAI_INTEGRATION
                if (collider.transform.root.GetComponent<EmeraldAI.EmeraldAISystem>())
                {
                    AIHelper.DamageEmeraldAI((int)damage, collider.transform.root.gameObject, characterOwner.transform);

                    // var enemy = collider.transform.root.GetComponent<EmeraldAI.EmeraldAISystem>();
                    // enemy.Damage(, EmeraldAI.EmeraldAISystem.TargetType.Player, characterOwner.transform);
                    // EmeraldAI.CombatTextSystem.Instance.CreateCombatText((int)damage, enemy.HitPointTransform.position, false, false, false);
                    break;
                }
#endif

                if (collider.GetComponent<Rigidbody>() && applyForce && !collider.transform.root.GetComponent<Controller>())
                    collider.GetComponent<Rigidbody>().AddExplosionForce(force * 50, transform.position, radius, 0.0f);

                //if a character received damage
                if (collider.transform.root.gameObject.GetComponent<Controller>() && collider.transform.name != "Noise Collider")
                {
                    var root = collider.transform.root;
                    var damagedCharacterController = root.gameObject.GetComponent<Controller>();

                    if (charactersIds.All(id => id != root.gameObject.GetInstanceID()))
                    {
                        charactersIds.Add(damagedCharacterController.gameObject.GetInstanceID());

                        if (characterOwner)
                        {
                            if (MultiplayerHelper.CanDamageInMultiplayer(damagedCharacterController, characterOwner))
                            {
                                if (damagedCharacterController.health > 0 && damagedCharacterController.health - damage <= 0 && characterOwner.CharacterSync && damagedCharacterController != characterOwner)
                                {
                                    // characterOwner.CharacterSync.AddScore(PlayerPrefs.GetInt("ExplosionKill"), "explosion");
                                }

                                damagedCharacterController.Damage(damage, "explosion",
#if USK_MULTIPLAYER
                                    PhotonNetwork.InRoom
                                        ? new Helper.ActorID {actorID = characterOwner.CharacterSync.photonView.OwnerActorNr, type = "player"}
                                        :
#endif
                                        null
                                );

                                CreateHitMarker(damagedCharacterController);
                            }
                        }
                        else if (aiOwner)
                        {
                            if (MultiplayerHelper.CanDamageInMultiplayer(damagedCharacterController, aiOwner))
                            {
                                damagedCharacterController.Damage(damage, "explosion",
#if USK_MULTIPLAYER
                                    PhotonNetwork.InRoom
                                        ? new Helper.ActorID {actorID = aiOwner.photonView.ViewID, type = "ai"}
                                        :
#endif
                                        null
                                );

                                var direction = startPosition - damagedCharacterController.transform.position;
                                var targetPosition = startPosition + direction * 1000;
                                CharacterHelper.CreateHitMarker(damagedCharacterController, null, targetPosition);
                            }
                        }
                    }
                }

                if (collider.GetComponent<FlyingProjectile>() && collider.gameObject.GetInstanceID() != instanceId)
                {
                    collider.GetComponent<FlyingProjectile>().Explosion();
                }

#if USK_DESTROYIT_INTEGRATION
                DestroyIt.ExplosiveDamage pointBlankExplosiveDamage = new DestroyIt.ExplosiveDamage()
                {
                    Position = transform.position,
                    DamageAmount = damage * 2,
                    BlastForce = force,
                    Radius = radius,
                    UpwardModifier = 1
                };

                var applyDamage = false;
                
                if (collider.GetComponentInParent<DestroyIt.Destructible>())
                {
                    var destObj = collider.GetComponentInParent<DestroyIt.Destructible>();
                    if (!destObj.isActiveAndEnabled && !destObj.isTerrainTree || destroyedObjects.Exists(id => id == collider.GetComponentInParent<DestroyIt.Destructible>().gameObject.GetInstanceID())) continue;
                    destObj.ApplyDamage(pointBlankExplosiveDamage);
                    
                    destroyedObjects.Add(destObj.gameObject.GetInstanceID());
                    
                    DestroyIt.ChipAwayDebris chipAwayDebris = collider.gameObject.GetComponent<DestroyIt.ChipAwayDebris>();
                    if (chipAwayDebris != null)
                        chipAwayDebris.BreakOff(force, radius, 10);

                    DestroyIt.HitEffects hitEffects = collider.gameObject.GetComponentInParent<DestroyIt.HitEffects>();
                    if (hitEffects != null && hitEffects.effects.Count > 0)
                        hitEffects.PlayEffect(DestroyIt.HitBy.Cannonball, transform.position, Vector3.up);
                }
#endif
            }

            if (characterOwner)
            {
                if (aiManager)
                {
                    aiManager.CheckExplosion(transform.position);
                }
            }
            
            if(gameObject.GetComponent<ParticleSystem>() && gameObject.GetComponent<ParticleSystem>().main.stopAction != ParticleSystemStopAction.Destroy || !gameObject.GetComponent<ParticleSystem>())
                Destroy(gameObject, 5);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        void CreateHitMarker(Controller opponentController)
        {
            if (opponentController != characterOwner)
            {
#if USK_MULTIPLAYER
                if (opponentController.CharacterSync)
                    opponentController.CharacterSync.CreateHitMark(startPosition);
#endif
            }
            else
            {
                var direction = transform.position - opponentController.transform.position;
                var targetPosition = transform.position + direction * 1000;
                CharacterHelper.CreateHitMarker(opponentController, null, targetPosition);
            }
        }
    }

}



