using System.Collections;
using UnityEngine;
using System.Linq;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public class AIAttack : MonoBehaviour
    {
        private bool attackAudioPlay;

//        private RaycastHit Hit;

        public AIController aiController;
        private AudioSource _audio;
        private AIHelper.EnemyAttack _attack;

        // public bool grenadeEffectTimeout;

        private bool disableColliders;
        private float disableCollidersTimeout;

        public Coroutine flashGrenadeEffectTimeout;

        private void Start()
        {
            _audio = GetComponent<AudioSource>() ? GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();

            _attack = aiController.Attacks[0];

            foreach (var collider in _attack.DamageColliders.Where(collider => collider))
            {
                collider.gameObject.SetActive(true);
                collider.isTrigger = true;
                collider.enabled = false;
            }
        }

        void Update()
        {
            if (_attack != null)
            {
                if (_attack.AttackType == AIHelper.AttackTypes.Fire && !aiController.anim.GetBool("Attack"))
                {

                }

//                if (_attack.AttackType == AIHelper.AttackTypes.Melee && !EnemyController.anim.GetBool("Attack"))
//                {

                disableCollidersTimeout += Time.deltaTime;

                if (disableColliders && disableCollidersTimeout > 1)
                {
                    disableColliders = false;

                    if (_attack.DamageColliders.Count > 0)
                    {
                        foreach (var damageCollider in _attack.DamageColliders.Where(collider => collider))
                        {
                            if (!damageCollider.isTrigger)
                                damageCollider.isTrigger = true;

                            if (damageCollider.enabled)
                                damageCollider.enabled = false;
                        }
                    }
                }
            }
        }

        public IEnumerator ReloadTimeout()
        {
            yield return new WaitForSeconds(0.5f);
            aiController.anim.SetBool("Reload", false);
        }

        public IEnumerator FlashGrenadeEffectDuration()
        {
            yield return new WaitForSeconds(7);
            aiController.EndGrenadeEffect();
            flashGrenadeEffectTimeout = null;
        }

        public void Attack(AIHelper.EnemyAttack Attack)
        {
            switch (Attack.AttackType)
            {
                case AIHelper.AttackTypes.Bullets:
                    BulletsAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Rockets:
                    RocketsAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Fire:
                    FireAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Melee:
                    break;
            }
        }

        void RocketsAttack(AIHelper.EnemyAttack Attack)
        {
            
#if USK_MULTIPLAYER
            var visualiseOnly = PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
#else
            var visualiseOnly = false;
#endif
            
            if (Attack.AttackAudio)
                _audio.PlayOneShot(Attack.AttackAudio);

            if (Attack.AttackSpawnPoints.Count > 0)
            {
                if (!visualiseOnly)
                {
                    if (Attack.UseReload)
                    {
                        Attack.CurrentAmmo -= 1;
                    }
                }

                foreach (var spawnPoint in Attack.AttackSpawnPoints)
                {
                    if (spawnPoint)
                    {
                        var rocket = Instantiate(Attack.rocket, spawnPoint.position, spawnPoint.rotation);
                        rocket.SetActive(true);

                        var rocketScript = rocket.AddComponent<FlyingProjectile>();
                        rocketScript.startPosition = transform.position;
                        rocketScript.isRocket = true;
                        rocketScript.ApplyForce = true;
                        rocketScript.Speed = Attack.flightSpeed;
                        // rocketScript.isEnemy = true;
                        rocketScript.ownerName = aiController.nickname;
                        rocketScript.aiOwner = aiController;
                        rocketScript.damage = !visualiseOnly ? Attack.Damage : 0;
                        // rocketScript.isRaycast = true;
                        
                        if (Attack.explosion)
                            rocketScript.explosion = Attack.explosion.transform;

                        var targetPosition = Vector3.zero;

                        if (!aiController.currentMultiplayerTarget)
                        {
                            var targetScript = aiController.closestTarget ? aiController.closestTarget : aiController.globalTarget;
                            targetPosition = targetScript.BodyObjects.TopBody.position;
                        }
                        else
                        {
                            targetPosition = aiController.currentMultiplayerTarget.transform.position + Vector3.up;
                        }


                        rocketScript.TargetPoint = targetPosition + new Vector3(Random.Range(-Attack.Scatter, Attack.Scatter), Random.Range(-Attack.Scatter, Attack.Scatter), 0);
                    }
                }
            }
        }

        void BulletsAttack(AIHelper.EnemyAttack Attack)
        {

#if USK_MULTIPLAYER
            var visualiseOnly = PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
#else
            var visualiseOnly = false;
#endif
            if (Attack.AttackAudio)
                _audio.PlayOneShot(Attack.AttackAudio);

            if (Attack.AttackSpawnPoints.Count > 0)
            {
                if (!visualiseOnly)
                {
                    if (Attack.UseReload)
                    {
                        Attack.CurrentAmmo -= 1;
                    }
                }

                foreach (var spawnPoint in Attack.AttackSpawnPoints)
                {
                    if (spawnPoint)
                    {
                        if (Attack.muzzleFlash)
                        {
                            var Flash = Instantiate(Attack.muzzleFlash, spawnPoint.position, spawnPoint.rotation);
                            Flash.transform.parent = spawnPoint.transform;
                            Flash.gameObject.AddComponent<DestroyObject>().destroyTime = 0.17f;
                        }

                        var randomRange = new Vector3(Random.Range(-Attack.Scatter, Attack.Scatter), Random.Range(-Attack.Scatter, Attack.Scatter), 0);

                        if (Attack.shootingMethod == WeaponsHelper.ShootingMethod.Raycast)
                        {
                            var hits = new RaycastHit[5];
                            var dir = Vector3.zero;
                            var dist = 0f;
                            var layerMask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));
                            var spawnPointPosition = spawnPoint.position;

                            if (!aiController.multiplayerBot)
                            {
                                var target = aiController.closestTarget ? aiController.closestTarget : aiController.globalTarget;

                                if (!target) return;

                                var topBodyPosition = target.BodyObjects.TopBody.position;

                                dir = topBodyPosition + randomRange - spawnPointPosition;
                                dist = Vector3.Distance(spawnPointPosition, topBodyPosition + randomRange);
                                
                                // hits = Physics.RaycastAll(spawnPointPosition, dir, dist, layerMask);
                                
                                Physics.RaycastNonAlloc(spawnPointPosition, dir, hits, dist, layerMask);
                            }
                            else
                            {
                                var target = aiController.currentMultiplayerTarget;
                                var targetPosition = aiController.currentMultiplayerTarget.transform.position;

                                if (!target) return;

                                if (target.GetComponent<Controller>())
                                {
                                    var script = target.GetComponent<Controller>();
                                    var topBodyPosition = script.BodyObjects.TopBody.position;

                                    dir = topBodyPosition + randomRange - spawnPoint.position;
                                    dist = Vector3.Distance(spawnPointPosition, topBodyPosition + randomRange);

                                }
                                else if (aiController.currentMultiplayerTarget.GetComponent<AIController>())
                                {
                                    var script = target.GetComponent<AIController>();

                                    if (script.BodyParts.Count > 0 && script.BodyParts[1])
                                    {
                                        dir = script.BodyParts[1].position + randomRange - spawnPoint.position;
                                        dist = Vector3.Distance(spawnPointPosition, script.BodyParts[1].position + randomRange);
                                    }
                                    else
                                    {
                                        dir = targetPosition + randomRange - spawnPoint.position;
                                        dist = Vector3.Distance(spawnPointPosition, targetPosition + randomRange);
                                    }
                                }

                                Physics.RaycastNonAlloc(spawnPointPosition, dir, hits, dist, layerMask);
                            }
                            
                            if (hits.Length > 0)
                            {
                                foreach (var hit in hits)
                                {
                                    if(!hit.transform) continue;
                                    
                                    if (hit.transform.name == "Noise Collider" || hit.transform.root.gameObject.GetInstanceID() == aiController.gameObject.GetInstanceID())
                                        continue;
                                    
                                    if(aiController.Attacks[0].bulletTrail)
                                        WeaponsHelper.CreateTrail(spawnPoint.position, hit.point, aiController.Attacks[0].bulletTrail);
                                    
                                    AIHelper.CheckBulletRaycast(aiController, hit, dir);
                                    
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var targetPosition = Vector3.zero;
                            
                            if (!aiController.multiplayerBot)
                            {
                                var target = aiController.closestTarget ? aiController.closestTarget : aiController.globalTarget;
                                
                                if(target)
                                    targetPosition = target.BodyObjects.TopBody.position + randomRange;
                            }
                            else
                            {
                                targetPosition = aiController.currentMultiplayerTarget.transform.position + Vector3.up + randomRange;
                            }
                            
                            WeaponsHelper.InstantiateBullet(aiController, spawnPoint.position, targetPosition);
                        }

                    }
                    else
                    {
                        Debug.LogError("(Enemy) <color=red>Missing components [AttackSpawnPoint]</Color>. Add it, otherwise the enemy won't shoot.", gameObject);
                    }
                }
            }
            else
            {
                Debug.LogError("(Enemy) <color=red>Missing components</color> [AttackSpawnPoint]. Add it, otherwise the enemy won't shoot.", gameObject);
            }
        }
        


        public void DisableFireColliders()
        {
            if (_audio.isPlaying)
            {
                attackAudioPlay = false;
                _audio.Stop();
            }

            if (_attack.DamageColliders.Count > 0)
            {
                foreach (var damageCollider in _attack.DamageColliders.Where(collider => collider))
                {
                    if (!damageCollider.isTrigger)
                        damageCollider.isTrigger = true;

                    if (damageCollider.enabled)
                        damageCollider.enabled = false;
                }
            }
        }

        private void FireAttack(AIHelper.EnemyAttack Attack)
        {
#if USK_MULTIPLAYER
            var visualiseOnly = PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
#else
            var visualiseOnly = false;
#endif
            if (!attackAudioPlay)
            {
                attackAudioPlay = true;
                _audio.clip = Attack.AttackAudio;
                _audio.Play();
            }

            if (Attack.AttackSpawnPoints.Count > 0)
            {
                if (!visualiseOnly)
                {
                    if (Attack.UseReload)
                    {
                        Attack.CurrentAmmo -= 1 * Time.deltaTime;
                    }
                }

                for (var i = 0; i < Attack.AttackSpawnPoints.Count; i++)
                {
                    if (Attack.fire)
                    {
                        var fire = Instantiate(Attack.fire, Attack.AttackSpawnPoints[i].position, Attack.AttackSpawnPoints[i].rotation);
                        fire.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        fire.gameObject.SetActive(true);
                    }

                    if (!visualiseOnly)
                    {
                        if (Attack.DamageColliders[i] && !Attack.DamageColliders[i].enabled)
                            Attack.DamageColliders[i].enabled = true;
                    }
                }
            }
        }

        public void MeleeAttack(bool visualiseOnly)
        {
            if (!visualiseOnly)
            {
                MeleeColliders("on");
                disableColliders = true;
                disableCollidersTimeout = 0;
            }
        }

        public void MeleeColliders(string status)
        {
            var attack = aiController.Attacks[0];

            if (attack.DamageColliders.Count > 0)
            {
                foreach (var collider in attack.DamageColliders.Where(collider => collider))
                {
                    collider.enabled = status == "on";
                }
            }
        }
    }

}





 

		




