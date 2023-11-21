using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GercStudio.USK.Scripts;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public class AIArea : NavMeshSurface
    {
        public List<SpawnZone> spawnZones;
        public List<Helper.EnemyToSpawn> enemiesToSpawn = new List<Helper.EnemyToSpawn>();

        public EventsManager eventsManager;

        public AIHelper.CommunicationBetweenAIs communicationBetweenAIs;

        // sync ->
        public List<AIController> allBotsInMatch = new List<AIController>();
        public List<Controller> allPlayersInMatch = new List<Controller>();
        //
        
        public List<AIController> allEnemiesInZone = new List<AIController>();
        public List<Cover> allCoversInZone = new List<Cover>();
        public List<AIHelper.CoverPoint> suitableCoverPoints = new List<AIHelper.CoverPoint>();

        public List<AIHelper.Opponent> allPlayersInScene = new List<AIHelper.Opponent>();
        public List<AIHelper.Opponent> allKnowPlayersInZone = new List<AIHelper.Opponent>();
        
        public List<Transform> pointsToCheck = new List<Transform>();
        public List<Transform> assignedPointsToCheck = new List<Transform>();
        
        private float findPlayersTimer;
        private float getCoversTimeout;
        
        public bool hasAnyPlayerInZone;
        public bool globalAttackState;
        public bool multiplayerMatch;
        public int navMeshArea;
        public float disableAttackStateTime = 15;

        private RaycastHit[] coversRaycasts = new RaycastHit[2];

        #region InspectorValues

        public int inspectorTab;
        
        #endregion

        private float endAttackTimer;
        private float setWaypointsStateTimeout;
        
        IEnumerator SpawnEnemies()
        {
            while (true)
            {
#if USK_MULTIPLAYER
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
#endif
                {
                    for (var i = 0; i < enemiesToSpawn.Count; i++)
                    {
                        var enemy = enemiesToSpawn[i];
                        enemy.currentTime += Time.deltaTime;

                        if (enemy.currentTime >= enemy.spawnTimeout && (enemy.spawnConstantly && allEnemiesInZone.Count < enemy.count || !enemy.spawnConstantly && enemy.spawnedEnemiesCount < enemy.count) && enemy.aiPrefab)
                        {
                            enemy.currentTime = 0;

                            var spawnZone = enemy.currentSpawnMethodIndex == 0 ? spawnZones[Random.Range(0, spawnZones.Count)] : enemy.spawnZone;

                            if (spawnZone)
                            {
                                var position = CharacterHelper.GetRandomPointInRectangleZone(spawnZone.transform);
                                var rotation = Quaternion.Euler(0, spawnZone.transform.eulerAngles.y, 0);

                                var name = enemy.aiPrefab.name;

                                GameObject instantiatedEnemy;

#if USK_MULTIPLAYER
                                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                                {
                                    instantiatedEnemy = PhotonNetwork.InstantiateRoomObject(enemy.aiPrefab.gameObject.name, position, rotation);
                                }
                                else
#endif
                                {
                                    instantiatedEnemy = Instantiate(enemy.aiPrefab.gameObject, position, rotation);
                                }

                                var controller = instantiatedEnemy.GetComponent<AIController>(); 
                                controller.aiArea = this;
                                // controller.indexInArea = i;

                                instantiatedEnemy.name = name;

                                enemy.spawnedEnemiesCount++;

                                ClearEmptyEnemies();
                                allEnemiesInZone.Add(controller);

                                foreach (var player in allPlayersInScene)
                                {
                                    controller.allPlayersInScene.Add(new AIHelper.Opponent {player = player.player, controller = player.controller});
                                }

                                if (enemy.movementBehavior)
                                    controller.MovementBehaviour = enemy.movementBehavior;

#if USK_MULTIPLAYER
                                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                                {
                                    eventsManager.SyncInstantiatedEnemy(controller, this, i);
                                }
#endif
                            }
                        }
                    }
                }

                yield return 0;
            }
        }

        // public void RemoveEnemyFromList(int index)
        // {
        //     if(index == -1) return;
        //     
        //     if(enemiesToSpawn[index].spawnConstantly)
        //         enemiesToSpawn[index].spawnedEnemiesCount--;
        // }

        private void Awake()
        {
            eventsManager = FindObjectOfType<EventsManager>();
        }

        void Start()
        {
            if(!Application.isPlaying) return;
            
            navMeshArea = 1 << defaultArea;

            GetAllEnemies();

            GetAllCovers();
            
            // var allCovers =  FindObjectsOfType<Surface>().ToList().FindAll(obj => obj.Cover);
            //
            // foreach (var obj in allCovers)
            // {
            //     var go = obj.gameObject;
            //     var collider = go.GetComponent<Collider>();
            //
            //     var newCover = new AIHelper.CoverObject()
            //     {
            //         script = obj,
            //         cover = go,
            //         collider = collider,
            //         bounds = collider.bounds
            //     };
            //     
            //     allCoversInZone.Add(newCover);
            // }
            
            if(!multiplayerMatch)
                StartCoroutine(SpawnEnemies());
        }

        private new void OnEnable()
        {
            base.OnEnable();
        }

        private new void OnDisable()
        {
            base.OnDisable();
        }


        private AIHelper.CoverPoint GetSuitableCoverPoint(AIController aiController)
        {
            AIHelper.CoverPoint suitablePoint = null;
            
            if (suitableCoverPoints.Count > 0)
            {
                var hasCurrentCoverPoint = aiController.currentCoverPoint != null && aiController.currentCoverPoint.pointTransform;

                var aiControllerPosition = aiController.transform.position;

                var sortPoints = suitableCoverPoints.OrderBy(point => point.parentCover.countOfEnemiesBehindThisCover).ThenBy(point => Vector3.Distance(point.pointTransform.position, aiControllerPosition));

                // AIHelper.CoverPoint nearestPoint = null;
                // var nearestDist = float.MaxValue;

                foreach (var point in sortPoints)
                {
                    var pointPosition = point.pointTransform.position;

                    if (hasCurrentCoverPoint && Vector3.Distance(point.pointTransform.position, aiControllerPosition) >= Vector3.Distance(aiController.currentCoverPoint.pointTransform.position, aiControllerPosition))
                        continue;
                    
                    if (point.aIController || !aiController.globalTarget || Vector3.Distance(pointPosition, aiController.globalTarget.transform.position) > aiController.distanceToSee * aiController.attackDistancePercent / 100) 
                        continue;
                    
                    suitablePoint = point;
                    
                    break;
                }
            }

            return suitablePoint;
        }

        public void SetSuitableCoverPoint(AIController aiController)
        {
            // if (point.parentCover.countOfEnemiesBehindThisCover == 0)
            // {
            // ApplyCoverPoint(aiController, point);
            
            var suitableCoverPoint = GetSuitableCoverPoint(aiController);

            if (suitableCoverPoint != null)
            {
                if (aiController.currentCoverPoint != null)
                {
                    aiController.currentCoverPoint.aIController = null;
                    aiController.currentCoverPoint = null;
                }

                if (aiController.currentPointToMove && aiController.currentPointToMove.gameObject.activeInHierarchy)
                {
                    aiController.currentPointToMove.gameObject.SetActive(false);

                    // Destroy(aiController.currentPointToMove.gameObject);
                }
                
                suitableCoverPoint.aIController = aiController;
                aiController.currentCoverPoint = suitableCoverPoint;
                
                suitableCoverPoint.parentCover.countOfEnemiesBehindThisCover++;
                
                if (aiController.agent.isOnNavMesh)
                    aiController.agent.SetDestination(aiController.currentCoverPoint.pointTransform.position);

                if (aiController.agentIsStopped)
                    aiController.StartMovement(aiController.anim.GetBool("Covers State"));
            }
            else
            {
                if (aiController.currentCoverPoint == null || !aiController.currentCoverPoint.pointTransform)
                    aiController.RandomMovementOnMap();
            }

            // }
            // else
            // {
            //         var dist = Vector3.Distance(pointPosition, aiController.transform.position);
            //         if (dist < nearestDist)
            //         {
            //             nearestDist = dist;
            //             nearestPoint = point;
            //         }
            // }



            // if (!foundCoverPoint && nearestPoint != null)
            // {
            //     ApplyCoverPoint(aiController, nearestPoint);
            // }
            // else 
            // if (!foundCoverPoint)
            // {
            //     aiController.ChoosePointOnMap();
            // }

        }

        private void GetSuitableCovers()
        {
            var layerMask = ~ (LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke") | LayerMask.GetMask("Character") | LayerMask.GetMask("MultiplayerCharacter"));
            
            suitableCoverPoints.Clear();
            
            foreach (var cover in allCoversInZone)
            {
                cover.countOfEnemiesBehindThisCover = cover.points.Count(point => point.aIController);
                
                foreach (var point in cover.points)
                {
                    if (!point.pointTransform) continue;
                    
                    var coverPosition = point.pointTransform.position;
                    
                    foreach (var player in allKnowPlayersInZone)
                    {
                        if(!player.player) continue;
                        
                        var playerPosition = player.player.transform.position;
                        
                        if (Vector3.Distance(playerPosition, coverPosition) <= 10)
                        {
                            point.isSuitablePoint = false;
                            break;
                        }

                        // if (Physics.Linecast(coverPosition, playerPosition, out var hitInfo, layerMask))
                        // {
                        //     if (hitGO.CompareTag("Cover") && cover.instanceIDs.Contains(hitGO.GetInstanceID()))
                        //     {
                        
                        if (Physics.RaycastNonAlloc(playerPosition, coverPosition - playerPosition, coversRaycasts, Vector3.Distance(playerPosition, coverPosition), layerMask) > 0)
                        {
                            if (coversRaycasts.Length == 2)
                            {
                                var firstHit = coversRaycasts[0].transform.gameObject;

                                point.isSuitablePoint = firstHit.CompareTag("Cover") && cover.instanceIDs.Contains(firstHit.GetInstanceID());

                                if (!cover.instanceIDs.Contains(firstHit.GetInstanceID()))
                                {
                                    var secondHit = coversRaycasts[1].transform.gameObject;
                                    point.isSuitablePoint = firstHit.CompareTag("Cover") && cover.instanceIDs.Contains(secondHit.GetInstanceID());
                                }
                               
                            }
                            else if(coversRaycasts.Length == 1)
                            {
                                var hitGO = coversRaycasts[0].transform.gameObject;

                                if (hitGO.CompareTag("Cover") && cover.instanceIDs.Contains(hitGO.GetInstanceID()))
                                {
                                    point.isSuitablePoint = true;
                                }
                                else
                                {
                                    point.isSuitablePoint = false;
                                }
                            }
                        }
                        
                        // }
                        // }
                        
                        else
                        {
                            point.isSuitablePoint = false;
                            break;
                        }
                    }

                    if (point.isSuitablePoint)
                    {
                        suitableCoverPoints.Add(point);
                    }
                    else if (point.aIController && !point.isSuitablePoint || 
                             point.aIController && point.aIController.globalTarget && Vector3.Distance(point.pointTransform.position, point.aIController.globalTarget.transform.position) > point.aIController.distanceToSee * point.aIController.attackDistancePercent / 100)
                    {
                        point.parentCover.countOfEnemiesBehindThisCover--;
                        point.aIController.currentCoverPoint = null;
                        point.aIController = null;
                    }
                }
            }
            // return suitableCovers;
        }

        private void GetAllCovers()
        {
            allCoversInZone.Clear();

            var allCovers = FindObjectsOfType<Cover>().ToList();
            
            foreach (var cover in allCovers)
            {
                if (NavMesh.SamplePosition(new Vector3(cover.transform.position.x, cover.transform.position.y, cover.transform.position.z), out var hit, 1, navMeshArea))
                {
                    allCoversInZone.Add(cover);
                }
            }
        }

        public void GetAllEnemies()
        {
            allEnemiesInZone.Clear();
            
            var allEnemies = FindObjectsOfType<AIController>().ToList();

            foreach (var enemy in allEnemies)
            {
                if (NavMesh.SamplePosition(new Vector3(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z), out var hit, 1, navMeshArea))
                {
                    if(enemy.health > 0)
                    {
                        ClearEmptyEnemies();
                        allEnemiesInZone.Add(enemy);
                        enemy.aiArea = this;
                    }
                }
            }
        }

        public void ClearEmptyPlayer(AIHelper.Opponent opponent, bool removeFromGlobalList)
        {
            var playerController = opponent.controller ? opponent.controller : null;
            
            if (removeFromGlobalList)
            {
                if (allPlayersInScene.Exists(player1 => player1.controller == playerController))
                {
                    allPlayersInScene.Remove(allPlayersInScene.Find(player => player.controller == playerController));
                }
            }
            
            if (allKnowPlayersInZone.Exists(player1 => player1.controller == playerController))
            {
                allKnowPlayersInZone.Remove(allKnowPlayersInZone.Find(player => player.controller == playerController));
            }

            foreach (var enemy in allEnemiesInZone)
            {
                if (removeFromGlobalList)
                {
                    
                    if (enemy.allPlayersInScene.Exists(player1 => player1.controller == playerController))
                    {
                        enemy.playersToRemove.Add(enemy.allPlayersInScene.Find(player => player.controller == playerController));
                    }
                }
                
                if (enemy.globalTarget == opponent.controller)
                    enemy.globalTarget = null;
                
                if (enemy.closestTarget == opponent.controller)
                    enemy.closestTarget = null;

                if (enemy.allPlayersSeenByEnemy.Exists(player1 => player1.controller == playerController))
                {
                    enemy.allPlayersSeenByEnemy.Remove(enemy.allPlayersSeenByEnemy.Find(player => player.controller == playerController));
                }
                
                if (enemy.allPlayersNearEnemy.Exists(player1 => player1.controller == playerController))
                {
                    enemy.allPlayersNearEnemy.Remove(enemy.allPlayersNearEnemy.Find(player => player.controller == playerController));
                }
                
                if (enemy.allPlayersHeardByEnemy.Exists(player1 => player1.controller == playerController))
                {
                    enemy.allPlayersHeardByEnemy.Remove(enemy.allPlayersHeardByEnemy.Find(player => player.controller == playerController));
                }

                if (enemy.lastOpponentWhoHitThisEnemy != null && enemy.lastOpponentWhoHitThisEnemy.controller == playerController)
                    enemy.lastOpponentWhoHitThisEnemy = null;
            }
        }

        public void ClearEmptyPlayers()
        {
            var playersToRemove = new List<AIHelper.Opponent>();

            foreach (var player in allPlayersInScene.Where(player => player.player == null))
            {
                playersToRemove.Add(player);
                //ClearEmptyPlayer(player, true);
            }

            foreach (var player in allKnowPlayersInZone.Where(player => player.player == null))
            {
                playersToRemove.Add(player);
                //ClearEmptyPlayer(player, true);
            }
            
            foreach (var enemy in allEnemiesInZone)
            {
                foreach (var player in enemy.allPlayersInScene.Where(player => player.player == null))
                {
                    playersToRemove.Add(player);
                    //ClearEmptyPlayer(player, true);
                }
                
                foreach (var player in enemy.allPlayersHeardByEnemy.Where(player => player.player == null))
                {
                    playersToRemove.Add(player);
                }
                
                foreach (var player in enemy.allPlayersSeenByEnemy.Where(player => player.player == null))
                {
                    playersToRemove.Add(player);
                }
                
                foreach (var player in enemy.allPlayersNearEnemy.Where(player => player.player == null))
                {
                    playersToRemove.Add(player);
                }
            }

            foreach (var player in playersToRemove)
            {
                ClearEmptyPlayer(player, true);
            }

            // if (PhotonNetwork.IsMasterClient)
            // {
            //     eventsManager.SyncKnownPlayers(allKnowPlayers);
            // }
        }

        public void ClearEmptyEnemies()
        {
            allEnemiesInZone.RemoveAll(enemy => enemy == null);
        }

        public void AddPlayerToGlobalList(Controller controller)
        {
            if(!allPlayersInScene.Exists(player => player.controller == controller))
                allPlayersInScene.Add(new AIHelper.Opponent {player = controller.gameObject, controller = controller});
            
            foreach (var enemy in allEnemiesInZone)
            {
                if(!enemy.allPlayersInScene.Exists(player => player.controller == controller))
                    enemy.allPlayersInScene.Add(new AIHelper.Opponent {player = controller.gameObject, controller = controller});
            }
        }

        public void FindPlayers()
        {
            allPlayersInScene.Clear();
            
            foreach (var enemyController in allEnemiesInZone)
            {
                enemyController.allPlayersInScene.Clear();
            }
            
            ClearEmptyPlayers();
                
            var foundPlayers = FindObjectsOfType<Controller>();
            // var foundAllies = FindObjectsOfType<EnemyController>().Where(enemy => enemy.isAlly).ToList();

            foreach (var player in foundPlayers)
            {
                if (player.health <= 0) continue;
                
                allPlayersInScene.Add(new AIHelper.Opponent {player = player.gameObject, controller = player});

                foreach (var enemy in allEnemiesInZone)
                {
                    enemy.allPlayersInScene.Add(new AIHelper.Opponent {player = player.gameObject, controller = player});
                }
            }

            // foreach (var ally in foundAllies)
            // {
            //     if (ally.EnemyHealth <= 0) continue;
            //     
            //     allPlayersInScene.Add(new AIHelper.Opponent {player = ally.gameObject, enemyController = ally});
            //     
            //     foreach (var enemy in allEnemiesInScene)
            //     {
            //         enemy.allPlayersInScene.Add(new AIHelper.Opponent {player = ally.gameObject, enemyController = ally});
            //     }
            // }
        }

#if USK_MULTIPLAYER
        public void SyncAllEnemies(int newPlayerId)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                if (!multiplayerMatch)
                {
                    eventsManager.SyncHealthOnAllEnemies(this, newPlayerId);
                    eventsManager.SyncGlobalAttackState(this, globalAttackState, newPlayerId);
                    eventsManager.SyncEnemiesTargets(allEnemiesInZone, newPlayerId, this);
                    eventsManager.SyncAllPoints(allEnemiesInZone, newPlayerId, this);
                    eventsManager.SyncKnownPlayersInArea(allKnowPlayersInZone, newPlayerId, this);

                    foreach (var enemy in allEnemiesInZone)
                    {
                        if (!enemy) continue;

                        eventsManager.SyncWayPoints(enemy, newPlayerId);
                    }
                }
                else
                {
                    eventsManager.SyncMultiplayerBots(this, newPlayerId);
                }
            }
        }
#endif

        void Update()
        {
            if (!Application.isPlaying) return;
            
            getCoversTimeout += Time.deltaTime;

#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient || multiplayerMatch) return;
#endif

            findPlayersTimer += Time.deltaTime;

            if (findPlayersTimer > 2 && allPlayersInScene.Count == 0)
            {
                findPlayersTimer = 0;
                FindPlayers();
            }

            hasAnyPlayerInZone = allPlayersInScene.Any(player => player.controller && player.controller.onNavMesh && player.controller.currentNavMeshArea == navMeshArea); // && player.controller.PlayerHealth > 0);

            if (!hasAnyPlayerInZone && globalAttackState && pointsToCheck.Count == 0)
            {
                EndAttack();

                // sound: they seem to be gone, let's check everything again
            }

            if (hasAnyPlayerInZone && globalAttackState && communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther)
            {
                var seeOrHearAnyPlayer = allPlayersInScene.Exists(opponent => opponent.controller.currentNavMeshArea == navMeshArea && (opponent.seePlayerHead || opponent.seePlayerHips || opponent.hearPlayer || opponent.inAttentionZone));

                if (seeOrHearAnyPlayer)
                {
                    endAttackTimer = 0;
                }
                else
                {
                    endAttackTimer += Time.deltaTime;

                    if (endAttackTimer > disableAttackStateTime)
                    {
                        EndAttack();

                        // sound: they leave, let's check everything again
                    }
                }
                
                if (getCoversTimeout > 1)
                {
                    GetSuitableCovers();
                    getCoversTimeout = 0;
                }
            }

            if (allEnemiesInZone.Count == 0 && (globalAttackState || pointsToCheck.Count > 0))
            {
                globalAttackState = false;
                
#if USK_MULTIPLAYER
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    eventsManager.SyncGlobalAttackState(this, globalAttackState, -1);
                }
#endif
                ClearPoints();
            }
            
            // if there are no any points to check on the scene, reset Warning state on all enemies
            if (pointsToCheck.Count == 0 && !globalAttackState && communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther && allEnemiesInZone.Exists(enemy => !enemy.observer))
            {
                if (allKnowPlayersInZone.Count > 0)
                {
                    allKnowPlayersInZone.Clear();
#if USK_MULTIPLAYER
                    if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                        eventsManager.SyncKnownPlayersInArea(allKnowPlayersInZone, -1, this);
#endif
                }

                if (assignedPointsToCheck.Count > 0)
                {
                    assignedPointsToCheck.Clear();
                }

                if (allEnemiesInZone.Exists(enemy => enemy.currentState != AIHelper.EnemyStates.Waypoints))
                {
                    var aiController = allEnemiesInZone.Find(enemy => enemy.currentState != AIHelper.EnemyStates.Waypoints);

                    if (aiController)
                    {
                        aiController.currentState = AIHelper.EnemyStates.Waypoints;
                    }
                }
            }
        }

        void EndAttack()
        {
            foreach (var player in allPlayersInScene)
            {
                ClearEmptyPlayer(player, false);
            }
            
            SetWarningStateToAllEnemies();
            GenerateRandomCheckPoints();
        }

        void GenerateRandomCheckPoints()
        {
            ClearPoints();
            
            foreach (var enemy in allEnemiesInZone)
            {
                if(!enemy || enemy.observer || communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemy.currentState == AIHelper.EnemyStates.Waypoints) continue;
                
                var addPointToCheckFunctionCount = 0;
                
                var point = AIHelper.RandomNavmeshPoint(enemy.transform,10, "warning", navMeshArea);
                
                // var point = AIHelper.GeneratePointOnNavMesh(enemy.transform.position, new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)), Random.Range(5, 15), ref addPointToCheckFunctionCount, true, "warning", null);//AIHelper.CreatePointToCheck(new Vector3(35, -4, 148), "warning");
                if(point)
                    pointsToCheck.Add(point);
            }

            if (pointsToCheck.Count > 0)
            {
                var firstEnemyWithWarningState = allEnemiesInZone.FirstOrDefault(enemy => enemy.currentState == AIHelper.EnemyStates.Warning);

                AssignPointsToEnemies(firstEnemyWithWarningState, pointsToCheck[0]);
            }
        }

        public void CheckExplosion(Vector3 explosionPosition)
        {
            if(multiplayerMatch) return;

            var point = AIHelper.CreatePointToCheck(explosionPosition + Vector3.up, "warning");
            pointsToCheck.Add(point);

            AIController closestAI = null;
            var closestDist = float.MaxValue;

            var emptyEnemies = allEnemiesInZone.FindAll(controller => controller.currentState == AIHelper.EnemyStates.Waypoints && controller.pointsToCheckForThisEnemy.Count < 3 && !controller.observer && Vector3.Distance(controller.transform.position, point.transform.position) < controller.distanceToSee);
            var observers = allEnemiesInZone.FindAll(controller => controller.observer && Vector3.Distance(controller.transform.position, point.transform.position) < controller.distanceToSee);

            foreach (var enemy in observers.Where(enemy => enemy.currentState == AIHelper.EnemyStates.Waypoints))
            {
                if(enemy.UseStates)
                    SetWarningState(enemy, point);
            }

            if (communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther)
            {
                foreach (var enemy in emptyEnemies)
                {
                    var enemyPos = enemy.transform.position;
                    var distance = Vector3.Distance(enemyPos, point.position);

                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        closestAI = enemy;
                    }
                }

                if (closestAI != null)
                {
                    SetPointToEnemy(closestAI, point);

                    if (closestAI.pointsToCheckForThisEnemy.Count > 0 && (!closestAI.currentPointToMove || !closestAI.currentPointToMove.gameObject.activeInHierarchy))
                    {
                        closestAI.StopAllCoroutines();
                        closestAI.SetNewPointToCheck();
                        closestAI.StartMovement(false);

                        // sound: I hear explosion! I'll check that!
                    }
#if USK_MULTIPLAYER
                    if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                    {
                        eventsManager.SyncAllPoints(allEnemiesInZone, -1, this);
                    }
#endif
                }
                else
                {
                    pointsToCheck.Remove(point);
                    Destroy(point.gameObject);
                }
            }
            else
            {
                pointsToCheck.Remove(point);
                Destroy(point.gameObject);
            }
        }

        public void GenerateAttackPoints(AIController originalAI)//(List<AIHelper.Player> visiblePlayers, EnemyController originalEnemy)
        {
            globalAttackState = true;
            
#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                eventsManager.SyncGlobalAttackState(this, globalAttackState, -1);
            }
#endif

            // if ()
            // {
                foreach (var enemyController in allEnemiesInZone)
                {
                    if(communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemyController.currentState == AIHelper.EnemyStates.Waypoints && enemyController != originalAI) continue;
                    
                    SetAttackState(enemyController);
                }
            // }
            // else
            // {
            //     SetAttackState(originalAI);
            // }

            foreach (var point in pointsToCheck)
            {
                if(!point) continue;
                
                Destroy(point.gameObject);
            }
            
            pointsToCheck.Clear();
            assignedPointsToCheck.Clear();

            ManagePlayersBetweenOpponents();
        }

        void SetAttackState(AIController aiController)
        {
            aiController.currentState = AIHelper.EnemyStates.Attack;
            aiController.attentionValue = 2.2f;

            if (aiController.currentPointToMove)
            {
                aiController.currentPointToMove.gameObject.SetActive(false);
            }

            aiController.pointsToCheckForThisEnemy.Clear();
            aiController.StopAllCoroutines();

            if (!aiController.observer)
                aiController.StartMovement(false);
        }

        public void SetWarningStateToAllEnemies()
        {
            foreach (var enemy in allEnemiesInZone)
            {
                if(!enemy || communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemy.currentState != AIHelper.EnemyStates.Attack) continue;

                if (enemy.currentPointToMove && enemy.currentPointToMove.gameObject.activeInHierarchy)
                {
                    enemy.currentPointToMove.gameObject.SetActive(false);
                }

                globalAttackState = false;
                
                if (communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther && allEnemiesInZone.Exists(ai => !ai.observer))
                {
                    enemy.currentState = AIHelper.EnemyStates.Warning;
                    enemy.attentionValue = 1.1f;
                }
                else
                {
                    enemy.currentState = AIHelper.EnemyStates.Waypoints;
                }

#if USK_MULTIPLAYER
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    eventsManager.SyncGlobalAttackState(this, globalAttackState, -1);
                }
#endif
                
                enemy.increaseAttentionValueTimer = 0;

                if (!enemy.observer)
                {
                    if (enemy.MovementBehaviour && enemy.MovementBehaviour.points[enemy.currentWayPointIndex].point)
                    {
                        enemy.agent.SetDestination(enemy.MovementBehaviour.points[enemy.currentWayPointIndex].point.transform.position);
                        enemy.StartMovement(false);
                    }
                    else
                    {
                        enemy.StopMovement();
                    }
                }
                else
                {
                    enemy.monitoringTimer = 0;
                    enemy.currentMonitoringDirection = enemy.directionObject.forward;
                }
            }
        }

        public void ManagePlayersBetweenOpponents()
        {
#if USK_MULTIPLAYER
            if(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                return;
            
            if(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                eventsManager.SyncKnownPlayersInArea(allKnowPlayersInZone, -1, this);
#endif
            
            foreach (var player in allKnowPlayersInZone)
            {
                player.enemiesWhoAttackPlayer.Clear();
            }

            if (allKnowPlayersInZone.Count == 0)
            {
                //sound: seems he was the last! (only for one enemy), but let's check everything again

                SetWarningStateToAllEnemies();
                GenerateRandomCheckPoints();

                return;
            }

            foreach (var enemy in allEnemiesInZone)
            {
                if(!enemy || communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemy.currentState != AIHelper.EnemyStates.Attack) continue;
                
                enemy.globalTarget = null;
                AIHelper.Opponent closestOpponent = null;
                var closestDist = float.MaxValue;
                var emptyPlayers = allKnowPlayersInZone.FindAll(controller => controller.enemiesWhoAttackPlayer.Count == 0);

                if (emptyPlayers.Count > 0)
                {
                    foreach (var player in emptyPlayers)
                    {
                        if(!player.player) continue;
                        
                        var enemyPos = enemy.transform.position;
                        var playerPos = player.player.transform.position;
                        var distance = Vector3.Distance(enemyPos, playerPos);

                        if (distance < closestDist)
                        {
                            closestDist = distance;
                            closestOpponent = player;
                        }
                    }

                    if (closestOpponent != null)
                    {
                        enemy.globalTarget = closestOpponent.controller;
                        closestOpponent.enemiesWhoAttackPlayer.Add(enemy);
                    }
                }
                else
                {
                    AIHelper.Opponent minAttentionOpponent = null;
                    var minAttacks = int.MaxValue;
                    
                    foreach (var player in allKnowPlayersInZone.Where(player => player.enemiesWhoAttackPlayer.Count < minAttacks))
                    {
                        if(!player.player) continue;
                        
                        minAttacks = player.enemiesWhoAttackPlayer.Count;
                        minAttentionOpponent = player;
                    }

                    if (minAttentionOpponent != null)
                    {
                        enemy.globalTarget = minAttentionOpponent.controller;
                        minAttentionOpponent.enemiesWhoAttackPlayer.Add(enemy);
                    }
                }
            }

#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                eventsManager.SyncEnemiesTargets(allEnemiesInZone, -1,this);
            }
#endif
            
        }

        public void GenerateCheckPoints(AIHelper.Opponent opponent, AIController originalAI)
        {
            ResetCheckPoints(opponent);
            
            // var firstPoint = AIHelper.RandomNavmeshPoint(opponent.player.transform, navMeshArea);
            var firstPoint = AIHelper.CreatePointToCheck(opponent.player.transform.position + Vector3.up, "warning");
            
            if(firstPoint)
                pointsToCheck.Add(firstPoint);
            
            GenerateAdditionalPoints(firstPoint);
            AssignPointsToEnemies(originalAI, firstPoint);
        }

        // void GenerateCheckPoints()
        // {
        //     ClearPoints();
        //     
        //     // for (int i = 0; i < 3; i++)
        //     // {
        //         var point = AIHelper.CreatePointToCheck(new Vector3(transform.position.x, allEnemiesInZone[0].transform.position.y, transform.position.z), "warning");
        //         if(point)
        //             pointsToCheck.Add(point);
        //     // }
        // }
        
        public Vector3 GetRandomLocation()
        {
            var navMeshData = NavMesh.CalculateTriangulation();
        
            // Pick the first indice of a random triangle in the nav mesh
            var t = Random.Range(0, navMeshData.indices.Length-3);
         
            var point = Vector3.zero;
            // Select a random point on it
            do
            {
                point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
                Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);
            } 
            while (NavMesh.SamplePosition(point, out var hit, 1, navMeshArea));
                
        
            return point;
        }
        
        public void GenerateCheckPoints(List<AIHelper.Opponent> visiblePlayers, AIController originalAI)
        {
            foreach (var player in visiblePlayers)
            {
                ResetCheckPoints(player);
            }
            
            var newPointsToCheck = new List<Transform>();
            
            foreach (var player in visiblePlayers)
            {
                var pointToCheck= AIHelper.CreatePointToCheck(player.player.transform.position + Vector3.up, "warning");
                newPointsToCheck.Add(pointToCheck);
            }

            pointsToCheck.AddRange(newPointsToCheck);

            if (newPointsToCheck.Count > 1)
            {
                foreach (var pointToCheck in newPointsToCheck)
                {
                    var addPointToCheckFunctionCount = 0;
                    
                    var point = AIHelper.RandomNavmeshPoint(pointToCheck,10, "warning", navMeshArea);
                    // var point = AIHelper.GeneratePointOnNavMesh(pointToCheck.position, transform.right * (Random.Range(0,2) * 2-1) + transform.forward * Random.Range(-1,2), Random.Range(7, 12), ref addPointToCheckFunctionCount, false, "warning", null);
                   
                    if (point)
                        pointsToCheck.Add(point);
                }
            }
            else
            {
                GenerateAdditionalPoints(newPointsToCheck[0]);
            }

            AssignPointsToEnemies(originalAI, newPointsToCheck[0]);
        }

        void AssignPointsToEnemies(AIController originalAI, Transform firstPoint)
        {
            ManagePointsToCheck(originalAI);

            var enemiesObservers = allEnemiesInZone.FindAll(controller => controller.observer);

            foreach (var enemyObserver in enemiesObservers)
            {
                if(communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemyObserver.currentState == AIHelper.EnemyStates.Waypoints && enemyObserver != originalAI) continue;
                
                SetWarningState(enemyObserver, firstPoint);
            }
        }

        void GenerateAdditionalPoints(Transform firstPoint)
        {
            // var addPointToCheckFunctionCount = 0;
            // var point = AIHelper.GeneratePointOnNavMesh(firstPoint.position, transform.forward, Random.Range(7, 12), ref addPointToCheckFunctionCount, false, "warning", null);
            var point = AIHelper.RandomNavmeshPoint(firstPoint,10, "warning", navMeshArea);
            if (point)
                pointsToCheck.Add(point);


            // addPointToCheckFunctionCount = 0;
            point = AIHelper.RandomNavmeshPoint(firstPoint,10, "warning", navMeshArea);
            // point = AIHelper.GeneratePointOnNavMesh(firstPoint.position, transform.right, Random.Range(7, 12), ref addPointToCheckFunctionCount, false, "warning", null);
            if (point)
                pointsToCheck.Add(point);


            // addPointToCheckFunctionCount = 0;
            point = AIHelper.RandomNavmeshPoint(firstPoint, 10, "warning",navMeshArea);
            // point = AIHelper.GeneratePointOnNavMesh(firstPoint.position, -transform.forward, Random.Range(7, 12), ref addPointToCheckFunctionCount, false, "warning", null);
            if (point)
                pointsToCheck.Add(point);


            // addPointToCheckFunctionCount = 0;
            point = AIHelper.RandomNavmeshPoint(firstPoint, 10, "warning", navMeshArea);
            // point = AIHelper.GeneratePointOnNavMesh(firstPoint.position, -transform.right, Random.Range(7, 12), ref addPointToCheckFunctionCount, false, "warning", null);
            if (point)
                pointsToCheck.Add(point);
            
        }

        void ResetCheckPoints(AIHelper.Opponent opponent)
        {
            if (!allKnowPlayersInZone.Exists(player1 => player1.controller == opponent.controller))
            {
                if (opponent.controller && opponent.controller.health > 0)
                {
                    allKnowPlayersInZone.Add(opponent);
#if USK_MULTIPLAYER
                    if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                    {
                        eventsManager.SyncKnownPlayersInArea(allKnowPlayersInZone, -1, this);
                    }
#endif
                }
            }
            else
            {
                ClearPoints();

                // sound: (original enemy) I saw him again, let's take a look there
            }
        }

        void ClearPoints()
        {
            var pointsToRemove = new List<Transform>();
                
            foreach (var enemy in allEnemiesInZone)
            {
                // var dist = Vector3.Distance(enemy.gameObject.transform.position, originalEnemy.gameObject.transform.position);

                // if (dist < distanceToCallForHelp)
                // {
                pointsToRemove.AddRange(enemy.pointsToCheckForThisEnemy);
                enemy.pointsToCheckForThisEnemy.Clear();

                if (enemy.currentPointToMove)
                {
                    enemy.currentPointToMove.gameObject.SetActive(false);
                }

                // enemy.currentPointToMove = null;
                // }
            }
                
            foreach (var _point in pointsToRemove)
            {
                if (_point == null) continue;
                
                pointsToCheck.Remove(_point);
                assignedPointsToCheck.Remove(_point);
                Destroy(_point.gameObject);
            }
        }

        void ManagePointsToCheck(AIController originalAI)
        {
            var pointsToRemove = new List<Transform>();
            var changedEnemies = new List<AIController>();

            foreach (var point in pointsToCheck)
            {
                if (!assignedPointsToCheck.Contains(point))
                {
                    var emptyEnemies = allEnemiesInZone.FindAll(controller => communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.CommunicateWithEachOther && controller.currentState == AIHelper.EnemyStates.Waypoints /*|| areaType == AIHelper.AreaType.Simplified && controller.currentState != AIHelper.EnemyStates.Waypoints && controller != originalAI)*/
                                                                                                                                                                   && controller.pointsToCheckForThisEnemy.Count < 3 && !controller.observer && controller.UseStates);

                    if (emptyEnemies.Count > 0)
                    {
                        AIController closestAI = null;
                        var closestDist = float.MaxValue;
                        
                        foreach (var enemy in emptyEnemies)
                        {
                            // if (areaType == AIHelper.AreaType.Simplified && enemy.currentState == AIHelper.EnemyStates.Waypoints && enemy != originalAI) continue;

                            var enemyPos = enemy.transform.position;
                            var distance = Vector3.Distance(enemyPos, point.position);

                            if (distance < closestDist)
                            {
                                closestDist = distance;
                                closestAI = enemy;
                            }
                        }

                        if (closestAI != null && (closestAI != originalAI || closestAI == originalAI && originalAI.pointsToCheckForThisEnemy.Count < 3))
                        {
                            if (closestAI != originalAI)
                            {
                                // sound:  I'll take a look too! I'll help you!
                            }

                            SetPointToEnemy(closestAI, point);

                            if (!changedEnemies.Contains(closestAI))
                                changedEnemies.Add(closestAI);
                            // Debug.LogError(closestEnemy.gameObject.name);
                        }
                        else
                        {
                            pointsToRemove.Add(point);
                        }
                    }
                    else
                    {
                        AIController suitableAI = null;
                        var pointsToCheck = int.MaxValue;

                        foreach (var enemy in allEnemiesInZone.Where(enemy => enemy && enemy.pointsToCheckForThisEnemy.Count < pointsToCheck && enemy.UseStates))
                        {
                            if (communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemy.currentState == AIHelper.EnemyStates.Waypoints && enemy != originalAI) continue;

                            var distanceBetweenEnemies = Vector3.Distance(originalAI.gameObject.transform.position, enemy.transform.position);

                            if (enemy.pointsToCheckForThisEnemy.Count < 3 && !enemy.observer) //distanceBetweenEnemies <= distanceToCallForHelp && 
                            {
                                pointsToCheck = enemy.pointsToCheckForThisEnemy.Count;
                                suitableAI = enemy;
                            }
                        }

                        if (suitableAI != null && (suitableAI != originalAI || suitableAI == originalAI && originalAI.pointsToCheckForThisEnemy.Count < 3))
                        {
                            if (suitableAI != originalAI)
                            {
                                // sound: I will finish my search and help you! | I will come to you soon and help you!
                            }
                            
                            SetPointToEnemy(suitableAI, point);

                            if (!changedEnemies.Contains(suitableAI))
                                changedEnemies.Add(suitableAI);
                            // Debug.LogError(suitableEnemy.gameObject.name);
                        }
                        else
                        {
                            pointsToRemove.Add(point);
                        }
                    }
                }

                // }
                // else
                // {
                //     if (originalAI.pointsToCheckForThisEnemy.Count < 2)
                //     {
                //         SetPointToEnemy(originalAI, point);
                //
                //         if (!changedEnemies.Contains(originalAI))
                //             changedEnemies.Add(originalAI);
                //     }
                //     else
                //     {
                //         pointsToRemove.Add(point);
                //     }
                // }
            }

            foreach (var enemy in allEnemiesInZone)
            {
                if(communicationBetweenAIs == AIHelper.CommunicationBetweenAIs.IndependentOpponents && enemy.currentState == AIHelper.EnemyStates.Waypoints && enemy != originalAI) continue;

                if (enemy.pointsToCheckForThisEnemy.Count > 0 && (!enemy.currentPointToMove || !enemy.currentPointToMove.gameObject.activeInHierarchy))
                {
                    enemy.StopAllCoroutines();
                    enemy.SetNewPointToCheck();
                    enemy.StartMovement(false);
                }
            }

            foreach (var point in pointsToRemove)
            {
                pointsToCheck.Remove(point);
                Destroy(point.gameObject);
            }
            
#if USK_MULTIPLAYER
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                eventsManager.SyncAllPoints(allEnemiesInZone, -1, this);
            }
#endif
        }

        void SetPointToEnemy(AIController aiController, Transform point)
        {
            SetWarningState(aiController, null);
            aiController.pointsToCheckForThisEnemy.Add(point);
            assignedPointsToCheck.Add(point);
        }

        void SetWarningState(AIController aiController, Transform firstPoint)
        {
            if(aiController.currentState == AIHelper.EnemyStates.Attack || aiController.currentState == AIHelper.EnemyStates.Warning) return;
            
            aiController.currentState = AIHelper.EnemyStates.Warning;
            aiController.attentionValue = 1;

            if (aiController.observer && firstPoint != null)
            {
                // sound: I will cover you!
                aiController.monitoringTimer = 0;
                var newDirection = firstPoint.transform.position - aiController.gameObject.transform.position;
                newDirection.y = 0;
                aiController.currentMonitoringDirection = newDirection;
            }
        }
    }
}
