using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GercStudio.USK.Scripts;
using UnityEngine;
#if USK_MULTIPLAYER
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif

namespace GercStudio.USK.Scripts
{

    public class BodyPartCollider : MonoBehaviour
    {
        [HideInInspector] public AIController aiController;
        [HideInInspector] public Controller controller;

        public enum BodyPart
        {
            Head,
            Hands,
            Legs,
            Body
        }

        [HideInInspector] public BodyPart bodyPart;
        [HideInInspector] public bool checkColliders;
        [HideInInspector] public float damageMultiplayer = 2;
        

        private void Update()
        {
            if(!checkColliders)
                return;

            if (controller)
            {
               RemoveAttackers(ref controller.colliderAttackers);
            }
            else if (aiController)
            {
                RemoveAttackers(ref aiController.attackers);
            }
        }

        void RemoveAttackers(ref List<Helper.Attacker> attackers)
        {
            var attackersToRemove = attackers.Where(attacker => !attacker.attackerGO || !attacker.collider.GetComponent<Collider>().enabled).ToList();
            
            foreach (var attacker in attackersToRemove)
            {
                attackers.Remove(attacker);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            
            if (!checkColliders) return;

#if USK_MULTIPLAYER
            if(aiController &&  PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;
#endif
            
            if (other.CompareTag("Fire"))// if getting damage from fire
            {
                var attacker = other.transform.root.gameObject;

                if (aiController)
                {
                    aiController.TakingDamageFromBodyColliders(attacker, "fire");
                }
                else if(controller)
                {
                    controller.TakingDamageFromColliders(attacker, "fire");
                }
                
                // gettingDamage = true;
                // attackType = "Fire";
                // attacking = other.transform.root.gameObject;
            }


            if (aiController)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Smoke"))// check if the enemy is in smoke
                {
                    aiController.grenadeEffect = true;
                    aiController.grenadeEffectTimeout = 0;
                }

                if (aiController.opponentsDetectionType != AIHelper.OpponentsDetectionType.Hearing && aiController.opponentsDetectionType != AIHelper.OpponentsDetectionType.All) return;
               
                var rootGO = other.transform.root.gameObject;
                
                if (aiController.aiArea.hasAnyPlayerInZone && rootGO.GetComponent<Controller>() && !other.CompareTag("Melee Collider") && !other.CompareTag("Fire")) // check players' noise
                {
                    if(rootGO.GetComponent<Controller>().currentNavMeshArea != aiController.aiArea.navMeshArea)
                        return;
                    
                    foreach (var player in aiController.aiArea.allPlayersInScene)
                    {
                        if (!player.player.Equals(other.transform.root.gameObject)) continue;

                        if (!player.inSight)
                            AIHelper.IsObstacle(player.controller.BodyObjects.Hips.transform.position, aiController.directionObject, aiController.currentState == AIHelper.EnemyStates.Attack, aiController.visionHits);

                        player.hearPlayer = !player.isObstacle;

                        if (player.hearPlayer)
                        {
                            aiController.canReduceAttentionValue = false;
                            aiController.increaseAttentionValueTimer = 0;
                        }

                        if (player.hearPlayer)
                        {
                            if (!aiController.allPlayersHeardByEnemy.Contains(player) && player.controller.health > 0)
                            {
                                aiController.allPlayersHeardByEnemy.Add(player);
                            }
                        }
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Melee Collider"))
            {
                var attacker = other.transform.root.gameObject;
                
                if (aiController)// character attacks enemy
                {
                    // if (!attacker.GetComponent<AIController>())
                    // {
                        if (!aiController.attackers.Exists(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID()))
                        {
                            aiController.attackers.Add(new Helper.Attacker {attackerGO = attacker, collidersThatTakingDamage = new List<BodyPartCollider> {this}, collider = other.gameObject});
                            aiController.TakingDamageFromBodyColliders(attacker, "melee");
                        }
                        else
                        {
                            var tempAttacker = aiController.attackers.Find(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID());

                            if (!tempAttacker.collidersThatTakingDamage.Contains(this))
                                tempAttacker.collidersThatTakingDamage.Add(this);
                        }
                    // }
                }
                else if (controller)// ai attacks character
                {
                    if (attacker.GetInstanceID() != controller.gameObject.GetInstanceID())
                    {
                        if (!controller.colliderAttackers.Exists(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID()))
                        {
                            controller.colliderAttackers.Add(new Helper.Attacker {attackerGO = attacker, collidersThatTakingDamage = new List<BodyPartCollider> {this}, collider = other.gameObject});
                            controller.TakingDamageFromColliders(attacker, "melee");
                        }
                        else
                        {
                            var tempAttacker = controller.colliderAttackers.Find(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID());

                            if (!tempAttacker.collidersThatTakingDamage.Contains(this))
                                tempAttacker.collidersThatTakingDamage.Add(this);
                        }
                    }
                }
                // gettingDamage = true;
                // attackType = "Melee";
                // attacking = other.transform.root.gameObject;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Melee Collider"))
            {
                var attacker = other.transform.root.gameObject;

                // if (aiController)
                // {
                //     if (aiController.attackers.Exists(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID()))
                //     {
                //         var tempAttacker = aiController.attackers.Find(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID());
                //
                //         if (tempAttacker.collidersThatTakingDamage.Contains(this))
                //         {
                //             tempAttacker.collidersThatTakingDamage.Remove(this);
                //         }
                //
                //         if (tempAttacker.collidersThatTakingDamage.Count == 0)
                //             aiController.attackers.Remove(tempAttacker);
                //     }
                //
                // }
                // else if (controller)
                // {
                //     if (controller.colliderAttackers.Exists(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID()))
                //     {
                //         var tempAttacker = controller.colliderAttackers.Find(attacker1 => attacker1.attackerGO.GetInstanceID() == attacker.GetInstanceID());
                //
                //         if (tempAttacker.collidersThatTakingDamage.Contains(this))
                //         {
                //             tempAttacker.collidersThatTakingDamage.Remove(this);
                //         }
                //
                //         if (tempAttacker.collidersThatTakingDamage.Count == 0)
                //         {
                //             controller.colliderAttackers.Remove(tempAttacker);
                //             print("remove");
                //         }
                //     }
                // }
            }

            if (!checkColliders
#if USK_MULTIPLAYER
                || PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient
#endif
                ) return;

            if (aiController)
            {
            //     if (other.gameObject.layer == LayerMask.NameToLayer("Smoke")) //other.CompareTag("Smoke"))
            //     {
            //         aiController.InSmoke = false;
            //     }

                if (aiController.opponentsDetectionType != AIHelper.OpponentsDetectionType.Hearing && aiController.opponentsDetectionType != AIHelper.OpponentsDetectionType.All) return;

                if (other.transform.root.gameObject.GetComponent<Controller>())
                {
                    aiController.allPlayersHeardByEnemy.Clear();

                    foreach (var player in aiController.aiArea.allPlayersInScene)
                    {
                        if (player.player.Equals(other.transform.root.gameObject))
                        {
                            player.hearPlayer = false;
                        }
                    }
                }
            }
        }
    }
}
