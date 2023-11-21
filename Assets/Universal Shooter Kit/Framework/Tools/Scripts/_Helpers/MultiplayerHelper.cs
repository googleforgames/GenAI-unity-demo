using System;
using System.Collections.Generic;
using System.Linq;
#if USK_MULTIPLAYER
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
	public static class MultiplayerHelper
	{
		public enum PhotonEventCodes
		{
			ChangeHealth = 0,
			ChangeWeapon = 1,
			PickUp = 2,
			Grenade = 3,
			DropWeapon = 4,
			Bullets = 5,
			Fire = 6,
			Rocket = 7,
			BulletHit = 8,
			ChangeCameraType = 9,
			DamagePlayer = 10,
			PlayerDeath = 11,
			Reload = 12,
			Aim = 13, 
			ChangeAttack = 14, 
			Crouch = 15,
			MeleeAttack = 16,
			UpdatePlayerList = 17,
			SetKillAssistants = 18,
			UpdateKillAssistants = 19,
			ChangeMovementType = 20, 
			CreateHitMark = 21, 
			ResetMinimap = 22,
			
			ChangeEnemyAnimation = 23,
			SyncWayPoints = 24,
			SyncAllPointsToCheck = 25,
			SyncPlayers = 26,
			SyncEnemiesTargets = 27,
			SyncGlobalAttackState = 28, 
			SyncEnemyHealth = 29, 
			SyncInstantiatedEnemy = 30,
			SyncEnemiesHealthWhenNewPlayerJoins = 31,
			SyncBotsParametersWhenNewPlayerJoins = 32,
			EnemyDeath = 33,
			UpdateBotsList = 34
			
			// UpdateWeaponsFromScript = 24
		}

		public enum CanKillOthers
		{
			OnlyOpponents,
			Everyone,
			NoOne
		}

		public enum MatchTarget
		{
			Score,
			Kills,
			Domination,
			Survive, 
			WithoutTarget
		}

		public enum ContentType
		{
			Player,
			Match,
			Weapon,
			GameMode,
			Map,
			Avatar, 
			Room
		}
		
		public enum Teams
		{
			Null,
			FirstTeam,
			SecondTeam
		}

		public static List<string> PhotonRegions = new List<string>()
		{
			"Asia",
			"Australia",
			"Canada, East",
			"Europe",
			"India",
			"Japan",
			"Russia",
			"Russia, East",
			"South America",
			"South Korea",
			"USA, East",
			"USA, West"
		};

		public static string EmptyLine = "                                                                        ";
		
		[Serializable]
		public class MultiplayerLevel
		{
			public string name;
			public Texture image;
			public int requiredRankLevel;
		}

		[Serializable]
		public class CameraPosition
		{
			public Vector3 position;
			public Quaternion rotation;
		}

		[Serializable]
		public class EditorScenes
		{
#if UNITY_EDITOR
			public List<SceneAsset> currentMapsInEditor = new List<SceneAsset>{null};
			public ReorderableList weaponsList;
			public ReorderableList mapsList;
			public List<SceneAsset> oldScenes = new List<SceneAsset>();
#endif
		}

		public static bool CanDamageInMultiplayer(AIController aiController1, AIController aiController2)
		{
			if (!aiController1.multiplayerBot || !aiController2.multiplayerBot)
				return false;
			
#if USK_ADVANCED_MULTIPLAYER
			if (PhotonNetwork.InRoom && !(bool) PhotonNetwork.CurrentRoom.CustomProperties["gs"])
				return false;
#endif
			
			return aiController1.canKillOthers == CanKillOthers.Everyone || aiController1.canKillOthers == CanKillOthers.OnlyOpponents && (aiController1.multiplayerTeam != aiController2.multiplayerTeam || aiController1.multiplayerTeam == aiController2.multiplayerTeam && aiController1.multiplayerTeam == Teams.Null);
		}

		public static bool CanDamageInMultiplayer(Controller controller1, Controller controller2)
		{
#if USK_ADVANCED_MULTIPLAYER
			if (PhotonNetwork.InRoom && !(bool) PhotonNetwork.CurrentRoom.CustomProperties["gs"])
				return false;
#endif
			
			return controller1.canKillOthers == CanKillOthers.Everyone || controller1.canKillOthers == CanKillOthers.OnlyOpponents && (controller2.multiplayerTeam != controller1.multiplayerTeam || controller2.multiplayerTeam == controller1.multiplayerTeam && controller1.multiplayerTeam == Teams.Null);
		}

		public static bool CanDamageInMultiplayer(Controller controller, AIController aiController)
		{
#if USK_ADVANCED_MULTIPLAYER
			if (PhotonNetwork.InRoom && !(bool) PhotonNetwork.CurrentRoom.CustomProperties["gs"])
			{
				return false;
			}
#endif
			return !aiController.multiplayerBot || aiController.multiplayerBot && (controller.canKillOthers == CanKillOthers.Everyone || controller.canKillOthers == CanKillOthers.OnlyOpponents && (aiController.multiplayerTeam != controller.multiplayerTeam || aiController.multiplayerTeam == controller.multiplayerTeam && controller.multiplayerTeam == Teams.Null));
		}

#if USK_MULTIPLAYER
		public static string FormatTime(double time)
		{
			int minutes = (int) time / 60;
			int seconds = (int) time - 60 * minutes;

			return $"{minutes:00}:{seconds:00}";
		}
		
		public static bool GetStartTime(out int startTimestamp, string value)
		{
			startTimestamp = - 1;

			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(value, out var startTimeFromProps))
			{
				startTimestamp = (int)startTimeFromProps;
				return true;
			}

			return false;
		}

		public static void SetNetworkTimerOnMC(string value)
		{
			var time = PhotonNetwork.ServerTimestamp;
			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {{value, time}});
		}

		public static Vector3 CalculateSpawnPosition(List<SpawnZone> spawnZones, List<GameObject> allOpponentsInScene, out int zoneIndex)
		{
			// var suitablePoints = new List<Vector3>();
			// var spawnPosition = Vector3.zero;
			// var indexes = new List<int>();
			var generateSpawnPointFunctionCount = 0;
			
			
			// spawn zones with the least number of opponents near them
			var suitableSpawnZones = GetSpawnZonesWithLeastNumberOfOpponents(spawnZones, allOpponentsInScene);
			
			zoneIndex = Random.Range(0, suitableSpawnZones.Count);
			var suitableSpawnZone = suitableSpawnZones[zoneIndex];
			
			var point = CalculateSpawnPoint(suitableSpawnZone, ref generateSpawnPointFunctionCount, allOpponentsInScene);
			
			// foreach (var spawnZone in suitableSpawnZones)
			// {
			// 	var foundSpawnPoint = false;
			// 	var generateSpawnPointFunctionCount = 0;
			// 	
			// 	var point = CalculateSpawnPoint(spawnZone, ref generateSpawnPointFunctionCount, ref foundSpawnPoint, allOpponentsInScene);
			//
			// 	if (foundSpawnPoint)
			// 	{
			// 		suitablePoints.Add(point);
			// 		// indexes.Add(i);
			// 	}
			// }

			// if (suitablePoints.Count > 0)
			// {
			// 	var index = Random.Range(0, suitablePoints.Count);
			// 	spawnPosition = suitablePoints[index];
			// 	zoneIndex = index;
			// }
			// else
			// {
				// spawnPosition = CalculateSpawnPointWithLeastOpponents(spawnZones, allOpponentsInScene, ref zoneIndex);
			// }
			
			// if (Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out var hit))
			// {
			// 	spawnPosition = new Vector3(spawnPosition.x, hit.point.y, spawnPosition.z);
			// }

			return point;
		}

		static List<SpawnZone> GetSpawnZonesWithLeastNumberOfOpponents(List<SpawnZone> spawnZones, List<GameObject> allOpponents)
		{
			var countOfEnemiesInZone = new int[spawnZones.Count];

			for (var i = 0; i < spawnZones.Count; i++)
			{
				var spawnZone = spawnZones[i];
				countOfEnemiesInZone[i] = 0;
				
				foreach (var opponent in allOpponents)
				{
					if(!opponent) continue;
					
					var zoneMinPoint = new Vector3(spawnZone.transform.position.x - spawnZone.transform.localScale.x / 2, 0, spawnZone.transform.position.z - spawnZone.transform.localScale.z / 2);
					var zoneMaxPoint = new Vector3(spawnZone.transform.position.x + spawnZone.transform.localScale.x / 2, 0, spawnZone.transform.position.z + spawnZone.transform.localScale.z / 2);
					var opponentPosition = opponent.transform.position;

					if (opponentPosition.x > zoneMinPoint.x && opponentPosition.z > zoneMinPoint.z && opponentPosition.x < zoneMaxPoint.x && opponentPosition.z < zoneMaxPoint.z)
					{
						countOfEnemiesInZone[i]++;
					}
				}
			}

			var bestCountOfEnemiesInZone = int.MaxValue;

			for (var i = 0; i < spawnZones.Count; i++)
			{
				if (countOfEnemiesInZone[i] < bestCountOfEnemiesInZone)
				{
					bestCountOfEnemiesInZone = countOfEnemiesInZone[i];
				}
			}

			var bestIndexes = new List<int>();

			for (var i = 0; i < countOfEnemiesInZone.Length; i++)
			{
				if (countOfEnemiesInZone[i] == bestCountOfEnemiesInZone)
				{
					bestIndexes.Add(i);
				}
			}

			var bestSpawnZones = bestIndexes.Select(index => spawnZones[index]).ToList();

			return bestSpawnZones;
		}

		// get a position on the spawn zone away from other players
		private static Vector3 CalculateSpawnPoint(SpawnZone spawnZone, ref int functionCount, List<GameObject> allPlayers)
		{
			functionCount++;

			var spawnPoint = CharacterHelper.GetRandomPointInRectangleZone(spawnZone.transform);

			var areTherePlayersNearby = false;

			foreach (var player in allPlayers.Where(player => player && Vector3.Distance(player.transform.position, spawnPoint) < 3))
			{
				areTherePlayersNearby = true;
			}

			if (!areTherePlayersNearby)
			{
				// foundPoint = true;
				return spawnPoint;
			}

			if (functionCount < 10)
				spawnPoint = CalculateSpawnPoint(spawnZone, ref functionCount, allPlayers);

			return spawnPoint;
		}
		
		public static void UpdateBotsList(AIArea aiArea, AIController aiController)
		{
			if (aiArea && aiArea.multiplayerMatch)
			{
				if (aiArea.allBotsInMatch.Exists(bot => bot == aiController))
					aiArea.allBotsInMatch.Remove(aiController);
			}
			
			aiArea.eventsManager.UpdateBotsList();
		}

		public static string ConvertRegionToCode(int value)
		{
			switch (value)
			{
				case 0:
					return "asia";

				case 1:
					return "au";
               
				case 2:
					return "cae";
                
				case 3:
					return "eu";
                
				case 4:
					return "in";
                
				case 5:
					return "jp";
                
				case 6:
					return "ru";
                
				case 7:
					return "rue";
                
				case 8:
					return "sa";
                
				case 9:
					return "kr";
                
				case 10:
					return "us";
                
				case 11:
					return "usw";
			}

			return "";
		}
		
		public static int ConvertCodeToRegion(string value)
		{
			if (value.Contains("/*"))
			{
				var replace = value.Replace("/*", "");
				value = replace;
			}
			
			switch (value)
			{
				case "asia":
					return 0;

				case "au":
					return 1;
               
				case "cae":
					return 2;
                
				case "eu":
					return 3;
                
				case "in":
					return 4;
                
				case "jp":
					return 5;
                
				case "ru":
					return 6;
                
				case "rue":
					return 7;
                
				case "sa":
					return 8;
                
				case "kr":
					return 9;
                
				case "us":
					return 10;
                
				case "usw":
					return 11;
			}

			return 0;
		}
#endif
	}
}
