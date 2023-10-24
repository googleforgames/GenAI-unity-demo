using System;
using System.Linq;
using UnityEngine;
#if USK_MULTIPLAYER
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
#endif


namespace GercStudio.USK.Scripts
{
    public class EventsManager :
#if USK_MULTIPLAYER
        MonoBehaviourPun
#else
    MonoBehaviour
#endif
    {
#if USK_MULTIPLAYER

        [HideInInspector]
        public List<AIArea> allAreasInScene;
        
#if USK_ADVANCED_MULTIPLAYER
        private AdvancedRoomManager advancedRoomManager; 
#endif
        
#if USK_MULTIPLAYER
        private RoomManager roomManager;
#endif
        
        private void Awake()
        {
            allAreasInScene = FindObjectsOfType<AIArea>().ToList();
            
#if USK_MULTIPLAYER
            if(GetComponent<RoomManager>())
                roomManager = GetComponent<RoomManager>();
#if USK_ADVANCED_MULTIPLAYER
            if(GetComponent<AdvancedRoomManager>())
                advancedRoomManager = GetComponent<AdvancedRoomManager>();
#endif
#endif
        }

        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
        
        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }

        public void ResetMinimap()
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ResetMinimap, null, options, SendOptions.SendReliable);

        }
        
        public void PickUp(int playerID, string pickUpId)
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.AddToRoomCacheGlobal,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                playerID,
                pickUpId
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.PickUp, content, options, SendOptions.SendReliable);
        }
        
#if USK_ADVANCED_MULTIPLAYER
        //this happens on the dead player client
        public void AllocateScoreAfterSomeoneDead(Helper.ActorID deadActorID, Helper.ActorID killerActorID, string killType, List<Helper.ActorID> opponentsWhoAttackedPlayer)
        {
            if (!advancedRoomManager) return;

            Player deadPlayer = null;
            AIController deadAI = null;
            
            if (deadActorID.type == "player")
            {
                deadPlayer = PhotonNetwork.PlayerList.ToList().Find(player => player.ActorNumber == deadActorID.actorID);
            }
            else if(deadActorID.type == "ai")
            {
                if(advancedRoomManager.aiArea)
                    deadAI = advancedRoomManager.aiArea.allBotsInMatch.Find(ai =>  ai.photonView.ViewID == deadActorID.actorID);

            }

            var benefits = AMHelper.CalculateScoreForKill(advancedRoomManager.gameData, killType, out var text);
      
            var benefitsForAssist = advancedRoomManager.gameData.killAssist;
           
            var content = new List<object>();
            var killerTeam = MultiplayerHelper.Teams.Null;

            // add score for kill
            if (killerActorID.type == "player")
            {
                var playerKiller = PhotonNetwork.PlayerList.ToList().Find(player => player.ActorNumber == killerActorID.actorID);

                killerTeam = (MultiplayerHelper.Teams) playerKiller.CustomProperties["pt"];

                if (deadPlayer != null && (MultiplayerHelper.Teams) deadPlayer.CustomProperties["pt"] == killerTeam && killerTeam != MultiplayerHelper.Teams.Null) return;
                if (deadAI != null && deadAI.multiplayerTeam == killerTeam && killerTeam != MultiplayerHelper.Teams.Null) return;
                
                var playerScore = (int) playerKiller.CustomProperties["scr"];
                playerScore += benefits.score;
                
                var playerMoney = (int) playerKiller.CustomProperties["money"];
                playerMoney += benefits.money;
                
                var killsCount = (int) playerKiller.CustomProperties["k"];
                killsCount++;

                playerKiller.SetCustomProperties(new Hashtable {{"scr", playerScore}, {"k", killsCount}, {"money", playerMoney}});
                
                content.Add(playerKiller.ActorNumber);
                content.Add(killerActorID.type);
                content.Add(killType);
            }
            else if(killerActorID.type == "ai")
            {
                if (advancedRoomManager.aiArea)
                {
                    var aiKiller = advancedRoomManager.aiArea.allBotsInMatch.Find(ai =>  ai.photonView.ViewID == killerActorID.actorID);

                    if (aiKiller)
                    {
                        killerTeam = aiKiller.multiplayerTeam;

                        if (deadPlayer != null && (MultiplayerHelper.Teams) deadPlayer.CustomProperties["pt"] == killerTeam && killerTeam != MultiplayerHelper.Teams.Null) return;
                        if (deadAI != null && deadAI.multiplayerTeam == killerTeam && killerTeam != MultiplayerHelper.Teams.Null) return;
                        
                        aiKiller.kills++;
                        
                        aiKiller.score += benefits.score;
                    }
                }

                content.Add(-1);
                content.Add(" ");
                content.Add(" ");
            }
            //

            // add score and kills count to the killer team
            if (advancedRoomManager.useTeams)
            {
                var property = killerTeam == MultiplayerHelper.Teams.FirstTeam ? "fts" : "sts";
                var currentScore = (int) PhotonNetwork.CurrentRoom.CustomProperties[property];

                if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Score)
                {
                    currentScore += benefits.score;
                }
                else if (advancedRoomManager.matchTarget == MultiplayerHelper.MatchTarget.Kills)
                {
                    currentScore++;
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {{property, currentScore}});

                //

                //add score for kill assist
                foreach (var opponentID in opponentsWhoAttackedPlayer)
                {
                    if(opponentID.type == killerActorID.type && opponentID.actorID == killerActorID.actorID) continue;

                    MultiplayerHelper.Teams curAssistTeam = MultiplayerHelper.Teams.Null;
                    
                    if (opponentID.type == "player")
                    {
                        var playerAssist = PhotonNetwork.PlayerList.ToList().Find(player => player.ActorNumber == opponentID.actorID);
                        if(playerAssist == null) continue;
                        
                        curAssistTeam = (MultiplayerHelper.Teams) playerAssist.CustomProperties["pt"];
                    }
                    else if(opponentID.type == "ai")
                    {
                        var aiAssist = advancedRoomManager.aiArea.allBotsInMatch.Find(ai =>  ai.photonView.ViewID == opponentID.actorID);
                        if(!aiAssist) continue;
                        
                        curAssistTeam = aiAssist.multiplayerTeam;
                    }

                    if (deadPlayer != null && (MultiplayerHelper.Teams) deadPlayer.CustomProperties["pt"] == curAssistTeam && curAssistTeam != MultiplayerHelper.Teams.Null) continue;
                    if (deadAI != null && deadAI.multiplayerTeam == curAssistTeam && curAssistTeam != MultiplayerHelper.Teams.Null) continue;
                    
                    if (opponentID.type == "player")
                    {
                        foreach (var player in PhotonNetwork.PlayerList.ToList().Where(player => player.ActorNumber == opponentID.actorID))
                        {
                            currentScore = (int) player.CustomProperties["scr"];
                            currentScore += benefitsForAssist.score;
                            
                            var playerMoney = (int) player.CustomProperties["money"];
                            playerMoney += benefitsForAssist.money;
                            
                            player.SetCustomProperties(new Hashtable {{"scr", currentScore}, {"money", playerMoney}});
                            content.Add(player.ActorNumber);
                            content.Add(opponentID.type);
                        }
                    }
                    else if(opponentID.type == "ai")
                    {
                        if (advancedRoomManager.aiArea)
                        {
                            foreach (var aiBot in advancedRoomManager.aiArea.allBotsInMatch.Where(bot => bot != null && bot.photonView.ViewID == opponentID.actorID))
                            {
                                aiBot.score += benefitsForAssist.score;
                                content.Add(-1);
                                content.Add(opponentID.type);
                            }
                        }
                    }
                }
            }
            //

            // send an event to other clients to show the pop-up message
            var options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SetKillAssistants, content, options, SendOptions.SendReliable);
            //
        }

#endif

        public void ChangeEnemyFindAnimation(int enemyID, int animationIndex, AIArea aiArea)
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.AddToRoomCacheGlobal,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                aiArea.defaultArea,
                enemyID,
                animationIndex
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.ChangeEnemyAnimation, content, options, SendOptions.SendReliable);
        }

        public void SyncInstantiatedEnemy(AIController aiController, AIArea aiArea, int index)
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                aiArea.defaultArea,
                aiController.photonView.ViewID,
                index
            };

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncInstantiatedEnemy, content, options, SendOptions.SendReliable);
        }

        public void SyncWayPoints(AIController aiController, int target)
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                aiController.photonView.ViewID,
                aiController.currentWayPointIndex
            };

            if (target != -1)
                options.TargetActors = new int[] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncWayPoints, content, options, SendOptions.SendReliable);
        }
        
        public void SyncKnownPlayersInArea(List<AIHelper.Opponent> players, int target, AIArea aiArea)
        {
            var ids = players.Select(player =>
                {
                    if (player != null && player.player) return player.controller.CharacterSync.photonView.ViewID;
                    return -1;
                }
                ).ToList();

            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            object[] content =
            {
                aiArea.defaultArea,
                ids.ToArray()
            };
            
            if (target != -1)
                options.TargetActors = new int[] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncPlayers, content, options, SendOptions.SendReliable);
        }
        
        // public void SyncAttackMovementPoint(AIArea aiArea, EnemyController enemyController, Transform point, int target)
        // {
        //     var position = point ? point.position : Vector3.one * -1;
        //     
        //     var content = new List<object>
        //     {
        //         aiArea.defaultArea,
        //         enemyController.photonView.ViewID,
        //         position
        //     };
        //     
        //     RaiseEventOptions options = new RaiseEventOptions()
        //     {
        //         CachingOption = EventCaching.DoNotCache,
        //         Receivers = ReceiverGroup.Others
        //     };
        //
        //     if (target != -1)
        //         options.TargetActors = new int[] {target};
        //     
        //     PhotonNetwork.RaiseEvent((byte) PUNHelper.PhotonEventCodes.SyncEnemyPointToMove, content.ToArray(), options, SendOptions.SendReliable);
        // }
        
        public void SyncEnemiesTargets(List<AIController> enemyControllers, int target, AIArea aiArea)
        {
            var content = new List<object>
            {
                aiArea.defaultArea,
                enemyControllers.Count // count of enemies
            };
            
            foreach (var enemy in enemyControllers)
            {
                if (enemy)
                {
                    content.Add(enemy.photonView.ViewID);
                    
                    if(enemy.globalTarget) content.Add(enemy.globalTarget.CharacterSync.photonView.ViewID);
                    else content.Add(-1);
                }
                else
                {
                    content.Add(-1);
                    content.Add(-1);
                }
            }
            
            // content.Add(players.Count);
            //
            // foreach (var player in players)
            // {
            //     if (player == null) continue;
            //     
            //     content.Add(player.controller.CharacterSync.photonView.ViewID);
            //     content.Add(player..globalTarget);
            // }
            
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
        
            if (target != -1)
                options.TargetActors = new int[] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncEnemiesTargets, content.ToArray(), options, SendOptions.SendReliable);
        }

        public void SyncGlobalAttackState(AIArea aiArea, bool value, int target)
        {
            var content = new List<object>
            {
                aiArea.defaultArea,
                value
            };
            
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            if (target != -1)
                options.TargetActors = new int[] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncGlobalAttackState, content.ToArray(), options, SendOptions.SendReliable);
        }

        public void SyncAllPoints(List<AIController> enemyControllers, int target, AIArea aiArea)
        {
            var pointsPositions = aiArea.pointsToCheck.Select(point => point.position).ToList();

            var assignedPoints = new List<int>();
            foreach (var point in aiArea.assignedPointsToCheck)
            {
                if (point)
                {
                    var index = aiArea.pointsToCheck.IndexOf(point);
                    
                    if(index != -1)
                        assignedPoints.Add(index);
                }
            }
            
            var content = new List<object>
            {
                aiArea.defaultArea,
                pointsPositions.ToArray(), // all points in AI Manager
                assignedPoints.ToArray(),
                enemyControllers.Count // count of enemies
            };
            
            foreach (var enemy in enemyControllers)
            {
                if (enemy == null)
                {
                    content.Add(-1);
                    content.Add(-1);
                }
                else
                {
                    var points = enemy.pointsToCheckForThisEnemy;
                    var currentPoint = enemy.currentPointToMove;
                    var currentPointIndex = currentPoint ? aiArea.pointsToCheck.IndexOf(currentPoint) : -1;

                    var pointsIndexes = new List<int> {currentPointIndex};

                    foreach (var point in points)
                    {
                        if (point)
                            pointsIndexes.Add(aiArea.pointsToCheck.IndexOf(point));
                    }

                    content.Add(enemy.photonView.ViewID);
                    content.Add(pointsIndexes.ToArray());
                }
            }
            
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            if (target != -1)
                options.TargetActors = new int[] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncAllPointsToCheck, content.ToArray(), options, SendOptions.SendReliable);
        }

        // event from Master Client to others
        public void EnemyDeath(AIController ai, AIArea aiArea)
        {
            if(!aiArea || ai && !ai.photonView) return;

            var content = new List<object>
            {
                aiArea.defaultArea,
                ai.photonView.ViewID,
            };
            
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.EnemyDeath, content.ToArray(), options, SendOptions.SendReliable);
        }

        public void UpdateBotsList()
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.UpdateBotsList, null, options, SendOptions.SendReliable);
        }

        // event from all clients to Master Client
        public void SyncDamage(float damage, AIController ai, AIArea aiArea, Helper.ActorID attackerActorNumber, string attackType)
        {
            if(!aiArea) return;
            
            var content = new List<object>
            {
                aiArea.defaultArea,
                ai.photonView.ViewID, 
                damage,
                attackerActorNumber.actorID,
                attackerActorNumber.type,
                attackType,
            };

            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.MasterClient
            };
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncEnemyHealth, content.ToArray(), options, SendOptions.SendReliable);
        }
        
        // void SyncEnemyHealth(EnemyController enemy, AIArea aiArea)
        // {
        //     var content = new List<object>
        //     {
        //         aiArea.defaultArea,
        //         enemy.photonView.ViewID, 
        //         enemy.EnemyHealth
        //     };
        //     
        //     RaiseEventOptions options = new RaiseEventOptions()
        //     {
        //         CachingOption = EventCaching.DoNotCache,
        //         Receivers = ReceiverGroup.Others
        //     };
        //
        //     PhotonNetwork.RaiseEvent((byte) PUNHelper.PhotonEventCodes.SyncEnemyHealth, content.ToArray(), options, SendOptions.SendReliable);
        // }

        public void SyncMultiplayerBots(AIArea aiArea, int target)
        {
            var content = new List<object>
            {
                aiArea.allBotsInMatch.Count
            };
            
            foreach (var enemy in aiArea.allBotsInMatch)
            {
                if (enemy)
                {
                    content.Add(enemy.photonView.ViewID);//1
                    content.Add(enemy.nickname);//2
                    content.Add(enemy.avatarName);//3
                    content.Add((int)enemy.multiplayerTeam);//4
                    content.Add(enemy.opponentsWhoAttackedThisAI.Count);//5

                    foreach (var attacker in enemy.opponentsWhoAttackedThisAI)
                    {
                        content.Add(attacker.actorID);//6
                        content.Add(attacker.type);//7
                    }
                }
            }
            
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            if (target != -1)
                options.TargetActors = new [] {target};
            
            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncBotsParametersWhenNewPlayerJoins, content.ToArray(), options, SendOptions.SendReliable);
        }

        public void SyncHealthOnAllEnemies(AIArea aiArea, int target)
        {
            var count = aiArea.allEnemiesInZone.Count;
            var content = new List<object>
            {
                aiArea.defaultArea,
                count
            };

            foreach (var enemy in aiArea.allEnemiesInZone)
            {
                if (enemy)
                {
                    content.Add(enemy.photonView.ViewID);
                    content.Add(enemy.health);
                }
                else
                {
                    content.Add(-1);
                    content.Add(-1);
                }
            }

            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            if (target != -1)
                options.TargetActors = new int[] {target};

            PhotonNetwork.RaiseEvent((byte) MultiplayerHelper.PhotonEventCodes.SyncEnemiesHealthWhenNewPlayerJoins, content.ToArray(), options, SendOptions.SendReliable);
        }

        void OnEvent(EventData photonEvent)
        {
            var eventCode = (MultiplayerHelper.PhotonEventCodes) photonEvent.Code;
            
            if (photonEvent.CustomData is object[] data)
            {
#if USK_ADVANCED_MULTIPLAYER
                if (eventCode == MultiplayerHelper.PhotonEventCodes.SetKillAssistants)
                {
                    var killerID = (int) data[0];
                    var killerType = (string) data[1];
                    var killType = (string) data[2];
                    
                    var assistIDs = new List<Helper.ActorID>();

                    for (var i = 3; i < data.Length; i += 2)
                    {
                        assistIDs.Add(new Helper.ActorID {actorID = (int) data[i], type = (string) data[i + 1]});
                    }
                    
                    if (killerType == "player" || assistIDs.Exists(id => id.type == "player"))
                    {
                        if (killerID == PhotonNetwork.LocalPlayer.ActorNumber && killerType == "player")
                        {
                            AMHelper.CalculateScoreForKill(advancedRoomManager.gameData, killType, out var text);
                            AMHelper.ShowMatchPopUp(advancedRoomManager, text);
                        }
                        else
                        {
                            if (assistIDs.Exists(id => id.actorID == PhotonNetwork.LocalPlayer.ActorNumber && id.type == "player"))
                            {
                                AMHelper.ShowMatchPopUp(advancedRoomManager, "+" + advancedRoomManager.gameData.normKill.score + " | $" + advancedRoomManager.gameData.normKill.money + " ((Kill Assistant))");
                            }
                        }
                    }
                }
                else 
#endif
                if (eventCode == MultiplayerHelper.PhotonEventCodes.PickUp)
                {
                    if (data.Length == 2)
                    {
                        var foundObjects = FindObjectsOfType<PickupItem>();
                        var players = FindObjectsOfType<Controller>();
                        var picUpId = (string) data[1];
                        var playerId = (int) data[0];
                        GameObject curPlayer = null;
                        var hasPlayer = false;
                        foreach (var player in players)
                        {
                            if (!player.GetComponent<PhotonView>()) continue;

                            if (player.GetComponent<PhotonView>().ViewID == playerId)
                            {
                                curPlayer = player.gameObject;
                                hasPlayer = true;
                            }
                        }

                        foreach (var obj in foundObjects)
                        {
                            if (obj.pickUpId == picUpId)
                            {
                                if (hasPlayer)
                                    obj.PickUpObject(curPlayer);
                                else
                                {
                                    Destroy(obj.gameObject);
                                }
                            }
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.ChangeEnemyAnimation)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if (aiArea == null) return;
                    
                    // var allEnemiesOnScene = FindObjectsOfType<AIController>().ToList();
                    var enemy = aiArea.allEnemiesInZone.Find(aiController => aiController != null && aiController.photonView.ViewID == (int) data[1]);
                    
                    if(enemy)
                        enemy.SetFindAnimation((int) data[2]);
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncInstantiatedEnemy)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if (aiArea == null) return;

                    var enemy = FindObjectsOfType<AIController>().ToList().Find(aiController => aiController != null && aiController.photonView.ViewID == (int) data[1]);

                    if (enemy && enemy.health > 0)
                    {
                        enemy.aiArea = aiArea;
                        enemy.MovementBehaviour = aiArea.enemiesToSpawn[(int) data[2]].movementBehavior;
                        enemy.gameObject.name = aiArea.enemiesToSpawn[(int) data[2]].aiPrefab.gameObject.name;
                        aiArea.ClearEmptyEnemies();
                        aiArea.allEnemiesInZone.Add(enemy);
                        
                        foreach (var player in aiArea.allPlayersInScene)
                        {
                            enemy.allPlayersInScene.Add(new AIHelper.Opponent {player = player.player, controller = player.controller});
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncGlobalAttackState)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if (aiArea == null) return;

                    aiArea.globalAttackState = (bool) data[1];
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncPlayers)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if (aiArea == null) return;

                    var ids = (int[]) data[1];

                    aiArea.allKnowPlayersInZone.Clear();
                    aiArea.ClearEmptyPlayers();

                    foreach (var player in aiArea.allPlayersInScene)
                    {
                        if (ids.Contains(player.controller.CharacterSync.photonView.ViewID) && !aiArea.allKnowPlayersInZone.Exists(_player => _player.controller == player.controller))
                        {
                            aiArea.allKnowPlayersInZone.Add(player);
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncWayPoints)
                {
                    // var aiArea = GetAIAreaFromIndex((int) data[0]);
                    // if (aiArea == null) return;
                    //
                    // if (aiArea.allEnemiesInZone.Count == 0)
                    //     aiArea.GetAllEnemies();
                    
                    var allEnemiesOnScene = FindObjectsOfType<AIController>().ToList();

                    var enemy = allEnemiesOnScene.Find(controller => controller.photonView.ViewID == (int) data[0]);
                    enemy.previousWayPointIndex = enemy.currentWayPointIndex;
                    enemy.currentWayPointIndex = (int) data[1];
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncAllPointsToCheck)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if(aiArea == null) return;
                    
                    // destroy all points on the scene
                    foreach (var point in aiArea.pointsToCheck.Where(point => point))
                    {
                        Destroy(point.gameObject);
                    }

                    aiArea.pointsToCheck.Clear();
                    aiArea.assignedPointsToCheck.Clear();
                    //

                    // read all received points 
                    var positions = (Vector3[]) data[1];
                    var assignedPointsIndexes = (int[]) data[2];
                    //

                    // instantiate new points 
                    foreach (var pos in positions)
                    {
                        var point = AIHelper.CreatePointToCheck(pos, "warning");
                        aiArea.pointsToCheck.Add(point);
                    }

                    for (var i = 0; i < assignedPointsIndexes.Length; i++)
                    {
                        if (aiArea.pointsToCheck[assignedPointsIndexes[i]])
                            aiArea.assignedPointsToCheck.Add(aiArea.pointsToCheck[assignedPointsIndexes[i]]);
                    }
                    //

                    // add points to enemies (which are changed)
                    var allChangedEnemies = new List<int>();

                    if ((int) data[3] > 0)
                    {
                        for (var i = 1; i <= (int) data[3]; i++)
                        {
                            var curIndex = i * 2 + 2;
                            var id = (int) data[curIndex];
                            
                            if(id == -1)
                                return;

                            var enemyController = aiArea.allEnemiesInZone.Find(controller => controller != null && controller.photonView.ViewID == id);

                            if (!enemyController) continue;
                            
                            enemyController.pointsToCheckForThisEnemy.Clear();

                            var allIndexes = (int[]) data[curIndex + 1];

                            allChangedEnemies.Add(enemyController.photonView.ViewID);

                            if (allIndexes.Length > 0)
                            {
                                if (allIndexes[0] != -1)
                                    enemyController.currentPointToMove = aiArea.pointsToCheck[allIndexes[0]];

                                for (var j = 1; j < allIndexes.Length; j++)
                                {
                                    if (aiArea.pointsToCheck[allIndexes[j]])
                                        enemyController.pointsToCheckForThisEnemy.Add(aiArea.pointsToCheck[allIndexes[j]]);

                                    // if (allIndexes.Contains(j) && allIndexes.ToList().IndexOf(j) != 0)
                                    // enemyController.pointsToCheckForThisEnemy.Add(AIManager.pointsToCheck[j]);
                                }
                            }
                        }
                    }
                    //

                    // return points to other enemies which are not changed them
                    // for (var i = 0; i < oldPoints.Count; i++) // if points didn't change on enemies, we need to return old points to these enemies
                    // {
                    //     var id = (int) oldPoints[i * 3];
                    //     var pointsForEnemy = (Vector3[]) oldPoints[id + 1];
                    //     var currentPointIndex = id + 2;
                    //     
                    //     if(id != -1 && !allChangedEnemies.Contains(id))
                    //     {
                    //         var enemyController = AIManager.allEnemiesInScene.Find(controller => controller.photonView.ViewID == id);
                    //         var allPoints = AIManager.pointsToCheck.FindAll(transform1 => pointsForEnemy.Contains(transform1.position));
                    //         
                    //         enemyController.pointsToCheckForThisEnemy.Clear();
                    //         enemyController.pointsToCheckForThisEnemy.AddRange(allPoints);
                    //         enemyController.currentPointToCheck = enemyController.pointsToCheckForThisEnemy[currentPointIndex];
                    //     }
                    // }
                    //
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncEnemiesTargets)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if(aiArea == null) return;
                    
                    aiArea.FindPlayers();
                    
                    foreach (var point in aiArea.pointsToCheck)
                    { 
                        Destroy(point.gameObject);   
                    }
                    
                    aiArea.pointsToCheck.Clear();
                    aiArea.assignedPointsToCheck.Clear();
                    

                    foreach (var enemy in aiArea.allEnemiesInZone)
                    {
                        enemy.globalTarget = null;
                    }

                    foreach (var player in aiArea.allPlayersInScene)
                    {
                        player.enemiesWhoAttackPlayer.Clear();
                    }

                    if ((int) data[1] > 0)
                    {
                        for (var i = 1; i <= (int) data[1]; i++)
                        {
                            var curIndex = i * 2;
                            var id = (int) data[curIndex];
                            
                            if(id == -1) return;

                            var enemyController = aiArea.allEnemiesInZone.Find(controller => controller != null && controller.photonView.ViewID == id);

                            if (!enemyController) continue;
                            
                            var targetId = (int) data[3];

                            if (targetId == -1)
                            {
                                enemyController.globalTarget = null;
                            }
                            else
                            {
                                var target = aiArea.allPlayersInScene.Find(player => player.controller.CharacterSync.photonView.ViewID == targetId);

                                if (target != null)
                                {
                                    enemyController.globalTarget = target.controller;
                                    target.enemiesWhoAttackPlayer.Add(enemyController);
                                }
                            }
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncBotsParametersWhenNewPlayerJoins)
                {
                    var allBots = FindObjectsOfType<AIController>().ToList();
                    
                    if ((int) data[0] > 0)
                    {
                        var lastIndex = 0;
                        
                        for (int i = 0; i < (int)data[0]; i ++)
                        {
                            if(data.Length <= lastIndex + 1) break;
                            
                            var id = (int) data[lastIndex + 1];
                            var nickname = (string) data[lastIndex + 2];
                            var avatarName = (string) data[lastIndex + 3];
                            // var health = (float) data[lastIndex + 4];
                            // var deaths = (int) data[lastIndex + 5];
                            var team = (MultiplayerHelper.Teams) (int) data[lastIndex + 4];
                            var attackersCount = (int) data[lastIndex + 5];

                            lastIndex += 5;

                            if (id == -1) return;

                            var enemyController = allBots.Find(aiController => aiController.photonView.ViewID == id);
                            
                            enemyController.avatarName = avatarName;
                            enemyController.nickname = nickname;
                            enemyController.ControlHealth("");
                            
                            enemyController.multiplayerTeam = team;
                            enemyController.opponentsWhoAttackedThisAI.Clear();

                            if (attackersCount > 0)
                            {
                                var lastAttackerIndex = lastIndex;
                                
                                for (var j = 0; j < attackersCount; j++)
                                {
                                    enemyController.opponentsWhoAttackedThisAI.Add(new Helper.ActorID {actorID = (int) data[lastAttackerIndex + 1], type = (string) data[lastAttackerIndex + 2]});
                                    lastAttackerIndex += 2;
                                }
                                
                                lastIndex += attackersCount * 2;
                            }
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncEnemiesHealthWhenNewPlayerJoins)
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if (aiArea == null) return;

                    if ((int) data[1] > 0)
                    {
                        if (aiArea.allEnemiesInZone.Count == 0)
                            aiArea.GetAllEnemies();

                        var lastIndex = 0;

                        for (var i = 0; i < (int)data[1]; i ++)
                        {
                            lastIndex += 2;
                            
                            var id = (int) data[lastIndex];

                            if (id == -1) return;

                            var enemyController = aiArea.allEnemiesInZone.Find(controller => controller != null && controller.photonView.ViewID == id);
                            enemyController.health = (float) data[lastIndex + 1];
                            enemyController.ControlHealth("");

                            
                        }
                    }
                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.EnemyDeath) 
                {
                    var aiArea = GetAIAreaFromIndex((int) data[0]);
                    if(aiArea == null) return;
                    
                    var id = (int)data[1];

                    AIController enemyController = null;
                    
                    if (aiArea.multiplayerMatch)
                    {
                        enemyController = aiArea.allBotsInMatch.Find(aiController => aiController != null && aiController.photonView && aiController.photonView.ViewID == id);
                    }
                    else
                    {
                        enemyController = aiArea.allEnemiesInZone.Find(aiController => aiController != null && aiController.photonView && aiController.photonView.ViewID == id);
                    }
                    
                    if (enemyController)
                        enemyController.Death("");

                }
                else if (eventCode == MultiplayerHelper.PhotonEventCodes.SyncEnemyHealth) // if get damage on other clients (recipient - master client)
                {
                    if (data.Length == 6)  
                    {
                        var aiArea = GetAIAreaFromIndex((int) data[0]);
                        if(aiArea == null) return;

                        var id = (int)data[1];
                        var damage = (float) data[2];
                        var attackerActorNumber = (int) data[3];
                        var attackerType = (string) data[4];
                        var attackType = (string) data[5];

                        if (!aiArea.multiplayerMatch)
                        {
                            var enemyController = aiArea.allEnemiesInZone.Find(controller => controller != null && controller.photonView.ViewID == id);

                            if (enemyController)
                            {
                                enemyController.TakingDamage(damage, attackType, false, new Helper.ActorID {actorID = attackerActorNumber, type = attackerType});
                            }
                        }
                        else
                        {
                            var enemyController = aiArea.allBotsInMatch.Find(controller => controller != null && controller.photonView.ViewID == id);
                            
                            if (enemyController)
                            {
                                enemyController.TakingDamage(damage, attackType, false, new Helper.ActorID {actorID = attackerActorNumber, type = attackerType});
                            }
                        }
                    }
                    
                    
                    // else if (data.Length == 3) // event from master client to others
                    // {
                    //     var aiArea = GetAIAreaFromIndex((int) data[0]);
                    //     if(aiArea == null) return;
                    //
                    //     var id = (int)data[1];
                    //     
                    //     var enemyController = aiArea.allEnemiesInZone.Find(controller => controller.photonView.ViewID == id);
                    //
                    //     enemyController.EnemyHealth = (float) data[2];
                    //
                    //     if (enemyController.EnemyHealth <= 0)
                    //     {
                    //         enemyController.CheckHealth();
                    //     }
                    // }
                    // else // set health when a new player have joined
                    // {
                    //     
                    // }
                }
            }
            else
            {
                if (eventCode == MultiplayerHelper.PhotonEventCodes.ResetMinimap)
                {
                    if(roomManager) roomManager.currentUIManager.ResetMinimap();
#if USK_ADVANCED_MULTIPLAYER
                    else if(advancedRoomManager) advancedRoomManager.currentUIManager.ResetMinimap();
#endif
                }
                else if(eventCode == MultiplayerHelper.PhotonEventCodes.UpdateBotsList)
                {
#if USK_ADVANCED_MULTIPLAYER
                    StartCoroutine(ClearLeftBots());
#endif
                }
            }
        }
        
#if USK_ADVANCED_MULTIPLAYER
        private IEnumerator ClearLeftBots()
        {
            yield return Helper.WaitFor.Frames(10);
            
            if (advancedRoomManager.aiArea && advancedRoomManager.aiArea.multiplayerMatch)
            {
                if (advancedRoomManager.aiArea.allBotsInMatch.Exists(bot => bot == null))
                    advancedRoomManager.aiArea.allBotsInMatch.Remove(advancedRoomManager.aiArea.allBotsInMatch.Find(bot => bot == null));
            }
        }
#endif

        AIArea GetAIAreaFromIndex(int index)
        {
            AIArea aiArea = null;
                    
            foreach (var area in allAreasInScene.Where(area => area.defaultArea == index))
            {
                aiArea = area;
            }

            return aiArea;
        }

        public IEnumerator ClearLeftPlayers()
        {
            // yield return new WaitForSeconds(1);
            yield return Helper.WaitFor.Frames(3);
            
            if(roomManager) roomManager.currentUIManager.ResetMinimap();
            
#if USK_ADVANCED_MULTIPLAYER
            else if(advancedRoomManager) advancedRoomManager.currentUIManager.ResetMinimap();
#endif

            foreach (var area in allAreasInScene)
            {
                area.ClearEmptyPlayers();
      
                if (PhotonNetwork.IsMasterClient)
                {
                    if(area.globalAttackState)
                        area.ManagePlayersBetweenOpponents();
                }
            }

            StopCoroutine(ClearLeftPlayers());
        }
#endif
    }
}

