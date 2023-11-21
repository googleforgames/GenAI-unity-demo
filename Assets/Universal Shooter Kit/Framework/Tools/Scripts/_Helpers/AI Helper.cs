using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
#if UNITY_EDITOR
// using TMPro;
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
	public static class AIHelper
	{

		public enum EnemyStates
		{
			Waypoints,
			Warning,
			Attack,
			FindAfterAttack
		}

		public enum AttackTypes
		{
			Bullets,
			Rockets,
			Fire,
			Melee
		}
		

		public enum CommunicationBetweenAIs
		{
			CommunicateWithEachOther, 
			IndependentOpponents
		}
		
		[Serializable]
		public class CheckPoint
		{
			public GameObject point;
			// public GameObject nextPoint;
			public float waitTime;
			// public bool isLookAround;
			public Helper.NextPointAction action;
			public Helper.NextPointAction curAction;
		}

		public enum OpponentsDetectionType
		{
			Vision,
			Hearing,
			CloseRange, 
			All
		}
		

		[Serializable]
		public class CoverPoint
		{
			public bool isSuitablePoint;
			public AIController aIController;
			public Cover parentCover;
			public Transform pointTransform;
		}
		
		[Serializable]
		public class EnemyAttack
		{
			public AttackTypes AttackType;
			[Range(0, 100)] public int Damage = 5;
			[Range(0.1f, 2)] public float Scatter = 1;
			[Range(0, 10)] public float RateOfAttack = 0.5f;
			[Range(1, 500)] public float flightSpeed = 20;
			
			public float InventoryAmmo = 20;
			public float CurrentAmmo = 20;
			
			public WeaponsHelper.ShootingMethod shootingMethod;
			
			public GameObject rocket;
			public GameObject fire;
			public GameObject muzzleFlash;
			public GameObject explosion;
			public GameObject bulletTrail;
			public GameObject bullet;
			
			public List<Transform> AttackSpawnPoints;

			public AudioClip AttackAudio;
			
			public List<AnimationClip> MeleeAttackAnimations;
			public AnimationClip HandsAttackAnimation;
			public AnimationClip HandsIdleAnimation;
			public AnimationClip HandsReloadAnimation;

			public List<BoxCollider> DamageColliders;
			
			public bool UseReload;
		}
		
		[Serializable]
		public class NavMeshAgentParameters
		{
			public float radius = 0.5f;
			public float height = 2f;
			public int agentType;
		}

		[Serializable]
		public class Opponent
		{
			public GameObject player;
			public Controller controller;

			public List<AIController> enemiesWhoAttackPlayer = new List<AIController>();
			
			public bool seePlayerHead;
			public bool seePlayerHips;
			public bool seeDirectly;

			public bool inAttentionZone;

			public bool inSight;
			
			public bool hearPlayer;
			
			public bool isObstacle;
		}

		[Serializable]
		public class GenericCollider
		{
			public string name;
			public Collider collider;
			public float damageMultiplier;
		}

		// [Serializable]
		// public class CoverObject
		// {
		// 	public GameObject cover;
		// 	public Surface script;
		// 	public Collider collider;
		// 	public Bounds bounds;
		// }

		#if USK_EMERALDAI_INTEGRATION
		
		public static void DamageEmeraldAI(int damageAmount, GameObject enemyGO, Transform character)
		{
			var enemy = enemyGO.GetComponent<EmeraldAI.EmeraldAISystem>();
			enemy.EmeraldDetectionComponent.DetectTargetType(character);
			enemy.Damage(damageAmount, EmeraldAI.EmeraldAISystem.TargetType.Player, character);
			EmeraldAI.CombatTextSystem.Instance.CreateCombatText(damageAmount, enemy.HitPointTransform.position, false, false, false);
		}
#endif

		public static bool CheckRaycast(Transform targetPoint, Transform currentPoint, float peripheralAngleToSee, float directAngleToSee, float heightToSee, float distanceToSee, RaycastHit[] hits)
		{
			var direction = targetPoint.position - currentPoint.position;
			var look = Quaternion.LookRotation(direction);

			var horizontalAngle = look.eulerAngles.y;
			if (horizontalAngle > 180)
				horizontalAngle -= 360;

			var spineAngleY = currentPoint.eulerAngles.y;
			if (spineAngleY > 180)
				spineAngleY -= 360;

			var middleAngleY = Mathf.DeltaAngle(horizontalAngle, spineAngleY);

			var verticalAngle = look.eulerAngles.x;
			if (verticalAngle > 180)
				verticalAngle -= 360;

			var spineAngleX = currentPoint.eulerAngles.x;
			if (spineAngleX > 180)
				spineAngleX -= 360;

			var middleAngleX = Mathf.DeltaAngle(verticalAngle, spineAngleX);

			if (Mathf.Abs(middleAngleY) > peripheralAngleToSee / 2 || Mathf.Abs(middleAngleX) > Mathf.Abs(Mathf.Asin(heightToSee / 2 / distanceToSee) * 180 / Mathf.PI))
			{
				return false;
			}

			var isObstacle = IsObstacle(targetPoint.position, currentPoint, false, hits);

			return !isObstacle;
		}
		
		public static bool CheckRaycast(Transform targetPoint, Transform currentPoint, float peripheralAngleToSee, float directAngleToSee, float heightToSee, float distanceToSee, bool attack, ref bool directVision, ref bool isObstacle, RaycastHit[] hits, ref bool inSight)
		{
			var direction = targetPoint.position - currentPoint.position;
			var look = Quaternion.LookRotation(direction);

			var horizontalAngle = look.eulerAngles.y;
			if (horizontalAngle > 180)
				horizontalAngle -= 360;

			var spineAngleY = currentPoint.eulerAngles.y;
			if (spineAngleY > 180)
				spineAngleY -= 360;

			var middleAngleY = Mathf.DeltaAngle(horizontalAngle, spineAngleY);

			var verticalAngle = look.eulerAngles.x;
			if (verticalAngle > 180)
				verticalAngle -= 360;

			var spineAngleX = currentPoint.eulerAngles.x;
			if (spineAngleX > 180)
				spineAngleX -= 360;

			var middleAngleX = Mathf.DeltaAngle(verticalAngle, spineAngleX);

			if (Mathf.Abs(middleAngleY) > peripheralAngleToSee / 2 || Mathf.Abs(middleAngleX) > Mathf.Abs(Mathf.Asin(heightToSee / 2 / distanceToSee) * 180 / Mathf.PI))
			{
				inSight = false;
				return false;
			}
			else
			{
				inSight = true;
			}
			
			isObstacle = IsObstacle(targetPoint.position, currentPoint, !attack, hits);
			
			var isPlayerVisible = !isObstacle;

			if (directAngleToSee > 0)
			{
				if (Mathf.Abs(middleAngleY) < directAngleToSee / 2 && isPlayerVisible)
				{
					directVision = true;
				}
			}

			return isPlayerVisible;
		}

		public static bool IsObstacle(Vector3 targetPoint, Transform currentPoint, RaycastHit[] visionHits)
		{
			return IsObstacle(targetPoint, currentPoint, false, visionHits);
		}

		public static bool IsObstacle(Vector3 targetPoint, Transform currentPoint, bool considerGrass, RaycastHit[] visionHits)
		{
			var layerMask = ~ LayerMask.GetMask("Enemy");
			
			// var targetPosition = targetPoint;
			var currentPosition = currentPoint.position;

			var dir = targetPoint - currentPosition;
			var dist = Vector3.Distance(targetPoint, currentPosition);
			
			var size = Physics.RaycastNonAlloc(currentPosition, dir, visionHits , dist, layerMask);
			
			var obstacle = false;
			// isObstacle = false;
			
			if (visionHits.Length > 0)
			{
				for (var i = 0; i < size; i++)
				{
					var hit = visionHits[i];
					
					if (!hit.transform.root.gameObject.GetComponent<Controller>() && !hit.transform.root.GetComponent<AIController>() && !hit.transform.root.gameObject.GetComponent<FlyingProjectile>()) //|| info.transform.root.gameObject.GetComponent<Controller>() && info.transform.name == "Noise Collider") &&
					{
						obstacle = true;
					}
				}
			}

			return obstacle;
		}

		public static Transform GetNearestPoint(List<Transform> points, Vector3 myPosition)
		{
			var bestPointIndex = 0;
			var closestDistanceSqr = Mathf.Infinity;
			
			for (var i = 0; i < points.Count; i++)
			{
				var directionToObject = points[i].transform.position - myPosition;
				var dSqrToTarget = directionToObject.sqrMagnitude;

				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					bestPointIndex = i;
				}
			}

			return points[bestPointIndex];
		}

		public static int GetNearestPoint(List<CheckPoint> points, Vector3 myPosition, int curIndex, int previousIndex)
		{
			var bestPointIndex = 0;
			var closestDistanceSqr = Mathf.Infinity;
			for (var i = 0; i < points.Count; i++)
			{
				Vector3 directionToObject = points[i].point.transform.position - myPosition;
				var dSqrToTarget = directionToObject.sqrMagnitude;

				if (dSqrToTarget < closestDistanceSqr && i != curIndex && i != previousIndex)
				{
					closestDistanceSqr = dSqrToTarget;
					bestPointIndex = i;
				}
			}

			return bestPointIndex;
		}

// 		public static Transform GetCoverPoint(EnemyController script)
// 		{
// 			Transform coverPoint = null;
// 			// var collidersNearEnemy = Physics.OverlapSphere(script.transform.position, script.DistanceToSee * script.attackDistancePercent / 100);
// 			// var collidersNearPlayer = Physics.OverlapSphere(script.Players[0].player.transform.position, script.DistanceToSee * script.attackDistancePercent / 100);
//
// 			// var coversNearEnemy = new List<GameObject>();
// 			// var coversNearPlayer = new List<GameObject>();
//
// 			var coversNearEnemy = FindClosestCovers(script.aiArea.allCoversInZone, script.transform, script.DistanceToSee * script.attackDistancePercent / 100);
// 			var coversNearPlayer = FindClosestCovers(script.aiArea.allCoversInZone, script.globalTarget.transform, script.DistanceToSee * script.attackDistancePercent / 100);
//
// 			var allEnemies = script.aiArea.allEnemiesInZone;//new List<EnemyController>(GameObject.FindObjectsOfType<EnemyController>());
// 			allEnemies.Remove(allEnemies.Find(enemy => enemy == script));
// 			allEnemies.RemoveAll(move => !move.currentCover.cover);
//
// 			// foreach (var collider in coversNearPlayer)
// 			// {
// 			// 	if (collider.gameObject.GetComponent<Surface>() && collider.gameObject.GetComponent<Surface>().Cover)
// 			// 	{
// 			// 		coversNearPlayer.Add(collider.gameObject);
// 			// 	}
// 			// }
// 			
// 			var suitableCoversNearEnemy = new List<CoverObject>();
//
// 			foreach (var collider in coversNearEnemy)
// 			{
// 				// if (collider.gameObject.GetComponent<Surface>() && collider.gameObject.GetComponent<Surface>().Cover)
// 				// {
// 					if (coversNearPlayer.Any(col => col.cover.gameObject.GetInstanceID() == collider.cover.gameObject.GetInstanceID()))
// 					{
// 						if (allEnemies.Count > 0)
// 						{
// 							if (allEnemies.All(enemyController => enemyController.currentCover.cover.gameObject.GetInstanceID() != collider.cover.gameObject.GetInstanceID()))
// 								suitableCoversNearEnemy.Add(collider);
// 						}
// 						else
// 						{
// 							suitableCoversNearEnemy.Add(collider);
// 						}
// 					}
// 				// }
// 			}
//
// 			var success = false;
// 			
// 			script.currentCover = FindClosestCover(suitableCoversNearEnemy, script, ref success);
//
// 			if (!success)
// 				script.currentCover = FindClosestCover(coversNearPlayer, script, ref success);
// 			
// 			var newPoint = Vector3.zero;
//
// 			if (script.currentCover.cover != null)
// 			{
// 				var position = script.currentCover.cover.transform.position;
// 				script.currentCoverDirection = position - script.globalTarget.transform.position;
// 				script.currentCoverDirection.Normalize();
//
// 				newPoint = position;
//
// 				var i = 0f;
// 				while (script.currentCover.bounds.Contains(newPoint - script.currentCoverDirection))
// 				{
// 					i += 0.5f;
// 					newPoint = script.currentCover.cover.transform.position + script.currentCoverDirection * i;
// 				}
//
// 				if (Vector3.Distance(script.globalTarget.transform.position, newPoint) < script.DistanceToSee * script.attackDistancePercent / 100)
// 				{
// 					coverPoint = new GameObject("Cover Point").transform;
// 					coverPoint.transform.position = newPoint;
// #if UNITY_EDITOR
// 					Helper.AddObjectIcon(coverPoint.gameObject, "Cover Point");
// #endif
// 				}
// 				else
// 				{
// 					script.currentCover = new CoverObject();
// 				}
// 			}
//
// 			if (coverPoint != null)
// 			{
// 				coverPoint.tag = "CoverPoint";
// 				// coverPoint.gameObject.hideFlags = HideFlags.HideInHierarchy;
// 			}
//
// 			return coverPoint;
// 		}

		public static void UILookAtCharacter(AIController aiController, Controller controller)
		{
			if(!controller.thisCamera) return;
			
			if (aiController.statsCanvas)
				aiController.statsCanvas.transform.LookAt(controller.thisCamera.transform);
                        
			// if(aiController.nicknameText)
			// 	aiController.nicknameText.transform.LookAt(controller.thisCamera.transform);
			//
			// if(aiController.StateCanvas)
			// 	aiController.StateCanvas.transform.LookAt(controller.thisCamera.transform);
		}
		
#if UNITY_EDITOR

		public static void CreateStatsCanvas(AIController aiController)
		{
			aiController.statsCanvas = Helper.NewCanvas("State Canvas", aiController.transform);
			var rect =  aiController.statsCanvas.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition3D = new Vector3(0, 2, 0);
			rect.localEulerAngles = Vector3.zero;
		}
		public static void CreateAttentionIndicator(AIController aiScript)
		{
			aiScript.attentionStatusMainObject = new GameObject("Attention Status Main Object");
			aiScript.attentionStatusMainObject.transform.SetParent(aiScript.statsCanvas.transform);
			var parent = aiScript.attentionStatusMainObject.transform;
			var rect = parent.gameObject.AddComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0.2f, 0.26f);
			rect.localEulerAngles = Vector3.zero;

			var background = Helper.NewImage("Background", parent, Vector2.one, Vector2.zero);
			background.transform.SetParent(aiScript.attentionStatusMainObject.transform);
			background.rectTransform.localEulerAngles = Vector3.zero;
			background.sprite = Resources.Load("State Background", typeof(Sprite)) as Sprite;
			background.raycastTarget = false;
			background.type = Image.Type.Filled;
			background.fillMethod = Image.FillMethod.Vertical;

			aiScript.yellowImg = Helper.NewImage("Warning State", parent, Vector2.one, Vector2.zero);
			aiScript.yellowImg.transform.SetParent(aiScript.attentionStatusMainObject.transform);
			aiScript.yellowImg.rectTransform.localEulerAngles = Vector3.zero;
			aiScript.yellowImg.sprite = Resources.Load("State Warning", typeof(Sprite)) as Sprite;
			aiScript.yellowImg.raycastTarget = false;
			aiScript.yellowImg.type = Image.Type.Filled;
			aiScript.yellowImg.fillMethod = Image.FillMethod.Vertical;

			aiScript.redImg = Helper.NewImage("Attack State", parent, Vector2.one, Vector2.zero);
			aiScript.redImg.transform.SetParent(aiScript.attentionStatusMainObject.transform);
			aiScript.redImg.rectTransform.localEulerAngles = Vector3.zero;
			aiScript.redImg.sprite = Resources.Load("State Attack", typeof(Sprite)) as Sprite;
			aiScript.redImg.raycastTarget = false;
			aiScript.redImg.type = Image.Type.Filled;
			aiScript.redImg.fillMethod = Image.FillMethod.Vertical;
			aiScript.redImg.fillAmount = 0.7f;
			
			Helper.SetAndStretchToParentSize(background.GetComponent<RectTransform>(), rect, false);
			Helper.SetAndStretchToParentSize(aiScript.yellowImg.GetComponent<RectTransform>(), rect, false);
			Helper.SetAndStretchToParentSize(aiScript.redImg.GetComponent<RectTransform>(), rect, false);

			aiScript.attentionStatusMainObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0.15f, 0);
		}

		public static void CreateNicknameText(AIController aiScript)
		{
			aiScript.nameText = Helper.NewUIElement("Name Text", aiScript.statsCanvas.transform, Vector2.zero, Vector2.one, Vector3.one).AddComponent<Text>();
			aiScript.nameText.text = "Bot";
			aiScript.nameText.fontSize = 85;
			aiScript.nameText.alignment = TextAnchor.MiddleCenter;
			aiScript.nameText.raycastTarget = false;
			aiScript.nameText.transform.SetParent(aiScript.statsCanvas.transform);

			var rect = aiScript.nameText.GetComponent<RectTransform>();
			rect.anchoredPosition3D = new Vector3(0, -0.06f, 0);
			rect.sizeDelta = new Vector2(700, 200);
			rect.localScale = new Vector3(0.001f, 0.001f, 0.001f);
			rect.localEulerAngles = new Vector3(0, 180, 0);
			
			// Helper.SetAndStretchToParentSize(aiScript.nicknameText.GetComponent<RectTransform>(), aiScript.statsCanvas.GetComponent<RectTransform>(), false);
		}

		public static void CreateNewHealthBar(AIController aiScript)
		{
			// var canvas = Helper.NewCanvas("Health Canvas", parent);
			// canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

			aiScript.healthBarMainObject = new GameObject("Health Bar Main Object");
			aiScript.healthBarMainObject.transform.SetParent(aiScript.statsCanvas.transform);

			var parent = aiScript.healthBarMainObject.transform;
			var rect = parent.gameObject.AddComponent<RectTransform>();
			rect.sizeDelta = Vector2.one;
			rect.localEulerAngles = Vector3.zero;

			var healthBarBackground =  Helper.NewImage("Background", parent, Vector2.one, Vector2.zero);
			healthBarBackground.transform.SetParent(aiScript.healthBarMainObject.transform);
			healthBarBackground.rectTransform.localEulerAngles = Vector3.zero;
			healthBarBackground.sprite = Resources.Load("Health Bar Background", typeof(Sprite)) as Sprite;
			healthBarBackground.raycastTarget = false;
			healthBarBackground.type = Image.Type.Filled;
			healthBarBackground.fillMethod = Image.FillMethod.Horizontal;
			
			aiScript.healthBarValue = Helper.NewImage("Value", healthBarBackground.transform, Vector2.one, Vector2.zero);
			aiScript.healthBarValue.transform.SetParent(healthBarBackground.transform);
			aiScript.healthBarValue.rectTransform.localEulerAngles = Vector3.zero;
			aiScript.healthBarValue.sprite = Resources.Load("Health Bar", typeof(Sprite)) as Sprite;
			aiScript.healthBarValue.raycastTarget = false;
			aiScript.healthBarValue.type = Image.Type.Filled;
			aiScript.healthBarValue.fillMethod = Image.FillMethod.Horizontal;
			aiScript.healthBarValue.fillOrigin = 1;
			
			
			Helper.SetAndStretchToParentSize(healthBarBackground.GetComponent<RectTransform>(), rect, false);
			Helper.SetAndStretchToParentSize(aiScript.healthBarValue.GetComponent<RectTransform>(), healthBarBackground.GetComponent<RectTransform>(), false);
			
			aiScript.healthBarMainObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -0.16f, 0);
		}
#endif

		// static List<CoverObject> FindClosestCovers(List<CoverObject> objects, Transform target, float maxDistance)
		// {
		// 	var nearObjects = new List<CoverObject>();
		//
		// 	foreach (var obj in objects)
		// 	{
		// 		var distance = Vector3.Distance(target.position, obj.cover.transform.position);
		// 		if (distance < maxDistance)
		// 		{
		// 			nearObjects.Add(obj);
		// 		}
		// 	}
		//
		// 	return nearObjects;
		// }
		
		// static CoverObject FindClosestCover(List<CoverObject> objects, AIController script, ref bool success)
		// {
		// 	CoverObject closest = null;
		// 	var distance = float.MaxValue;
		//
		// 	foreach (var obj in objects)
		// 	{
		// 		var position = obj.cover.transform.position;
		// 		var enemyDistance = Vector3.Distance(position, script.transform.position);
		// 		var characterDistance = Vector3.Distance(position, script.globalTarget.transform.position);
		//
		// 		if (characterDistance > 15 && characterDistance < script.DistanceToSee * script.attackDistancePercent / 100 && enemyDistance < distance)
		// 		{
		// 			closest = obj;
		// 			distance = enemyDistance;
		// 		}
		// 	}
		//
		// 	return closest;
		// }

		static void SetAnimsAxes(Animator anim, float horizontalValue, float verticalValue)
		{
			var xValue = anim.GetFloat("Horizontal");
			xValue = Mathf.Lerp(xValue, horizontalValue, 5 * Time.deltaTime);
			anim.SetFloat("Horizontal", xValue);
				
			var yValue = anim.GetFloat("Vertical");
			yValue = Mathf.Lerp(yValue, verticalValue, 5 * Time.deltaTime);
			anim.SetFloat("Vertical", yValue);
		}

		public static void AllSidesMovement(float angle, Animator anim)
		{
			// NullDirectionAnimations(anim);

			if (angle > -23 && angle <= 23)
			{
				SetAnimsAxes(anim, 0, 1);
				// anim.SetBool("Forward", true);
			}
			else if (angle > 23 && angle <= 68)
			{
				SetAnimsAxes(anim, 0.75f, 0.75f);
				// anim.SetBool("ForwardRight", true);
			}
			else if (angle > 68 && angle <= 113)
			{
				SetAnimsAxes(anim, 1, 0);
				// anim.SetBool("Right", true);
			}
			else if (angle > 113 && angle <= 158)
			{
				SetAnimsAxes(anim, 0.75f, -0.75f);
				// anim.SetBool("BackwardRight", true);
			}
			else if (angle <= -23 && angle > -68)
			{
				SetAnimsAxes(anim, -0.75f, 0.75f);
				// anim.SetBool("ForwardLeft", true);
			}
			else if (angle <= -68 && angle > -113)
			{
				SetAnimsAxes(anim, -1, 0);
				// anim.SetBool("Left", true);
			}
			else if (angle <= -113 && angle > -158)
			{
				SetAnimsAxes(anim, -0.75f, -0.75f);
				// anim.SetBool("BackwardLeft", true);
			}
			else
			{
				SetAnimsAxes(anim, 0, -1);
				// anim.SetBool("Backward", true);
			}
		}

		public static void GetCurrentSpeed(AIController controller)
		{
			var angle = Helper.AngleBetween(controller.directionObject.forward, controller.agent.velocity);

			if (angle > -68 && angle < 68)
			{
				controller.currentSpeed = controller.currentState != EnemyStates.Attack ? controller.walkForwardSpeed : controller.runForwardSpeed;
			}
			else if (angle > -113 && angle <= -68 || angle >= 68 && angle < 113)
			{
				controller.currentSpeed = controller.runLateralSpeed;
			}
			else
			{
				controller.currentSpeed = controller.runBackwardSpeed;
			}
		}

		public static Transform CreatePointToCheck(Vector3 position, string type)
		{
			var point = new GameObject("Point to check");
			point.transform.position = position;
#if UNITY_EDITOR
			Helper.AddObjectIcon(point, type == "warning" ? "Point to check (warning)" : "Point to check (attack)");
#endif
			return point.transform;
		}

		public static void CheckBulletRaycast(AIController owner, RaycastHit hit, Vector3 direction)
		{
			
#if USK_MULTIPLAYER
			var visualiseOnly = PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient;
#else
            var visualiseOnly = false;
#endif
			
			if (!visualiseOnly)
			{
				if (hit.transform.GetComponent<BodyPartCollider>())
				{
					var bodyCollider = hit.transform.GetComponent<BodyPartCollider>();

					var damagedController = bodyCollider.controller;
					var damagedAIController = bodyCollider.aiController;

					if (damagedController)
					{
						if (MultiplayerHelper.CanDamageInMultiplayer(damagedController, owner))
						{
							BulletDamage(owner.transform, bodyCollider, hit, damagedController.BodyObjects.Hips, owner.Attacks[0].Damage
#if USK_MULTIPLAYER
								, PhotonNetwork.InRoom ? new Helper.ActorID{actorID = owner.photonView.ViewID, type = "ai"} : null
#else
								, null
#endif
								, null, damagedController);
						}
					}
					else if (damagedAIController)
					{
						if (MultiplayerHelper.CanDamageInMultiplayer(owner, damagedAIController))
						{
							BulletDamage(owner.transform, bodyCollider, hit, damagedAIController.BodyParts[0], owner.Attacks[0].Damage
#if USK_MULTIPLAYER
								, PhotonNetwork.InRoom ? new Helper.ActorID{actorID = owner.photonView.ViewID, type = "ai"} : null
#else
								, null
#endif
								, damagedAIController);
						}
					}
				}
				
				if (hit.transform.GetComponent<Rigidbody>())
				{
					hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(direction * 500, hit.point);
				}
			}

			if (hit.transform.GetComponent<Surface>())
			{
				var surface = hit.transform.GetComponent<Surface>();
				
				if (surface.Material)
				{
					if (surface.Sparks)
					{
						var sparks = Object.Instantiate(surface.Sparks, hit.point + hit.normal * 0.01f, Quaternion.FromToRotation(Vector3.forward, hit.normal));
						sparks.hideFlags = HideFlags.HideInHierarchy;
					}
					
					if (surface.Hit)
					{
						var hitGO = Object.Instantiate(surface.Hit, hit.point + hit.normal * 0.001f, Quaternion.FromToRotation(Vector3.forward, hit.normal));
						hitGO.hideFlags = HideFlags.HideInHierarchy;
						hitGO.parent = hitGO.transform;
						
						if (surface.HitAudio)
						{
							var audio = hitGO.gameObject.AddComponent<AudioSource>();
							audio.clip = surface.HitAudio;
							audio.PlayOneShot(hitGO.gameObject.GetComponent<AudioSource>().clip);
						}
					}
				}
			}
		}

		private static void BulletDamage(Transform owner, BodyPartCollider bodyScript, RaycastHit hit, Transform hipsBone, float damage, Helper.ActorID attackerActorID = null, AIController damagedAIController = null, Controller damagedController = null)
		{
			if (damagedController)
			{
				if (damagedController.inventoryManager.bloodProjector)
				{
					if (hit.transform.gameObject.GetInstanceID() != hipsBone.gameObject.GetInstanceID())
						WeaponsHelper.CreateBlood(damagedController.inventoryManager.bloodProjector, hit.point - hit.normal * 0.15f, Quaternion.LookRotation(hit.point - hit.normal * 2), hit.transform, damagedController.BloodHoles);
				}

				damagedController.Damage(damage, bodyScript.bodyPart != BodyPartCollider.BodyPart.Head ? "bullet" : "headshot", attackerActorID);

				var _direction = owner.position - damagedController.transform.position;
				var targetPosition = owner.position + _direction * 1000;
				CharacterHelper.CreateHitMarker(damagedController, owner, targetPosition);
				
				WeaponsHelper.InstantiateAdditionalBloodEffects(damagedController.additionalHitEffects, hit.point, Quaternion.LookRotation(hit.normal));

			}
			else if (damagedAIController)
			{
				if (damagedAIController.bloodProjector)
				{
					if (hit.transform.gameObject.GetInstanceID() != hipsBone.gameObject.GetInstanceID())
						WeaponsHelper.CreateBlood(damagedAIController, damagedAIController.bloodProjector, hit.point - hit.normal * 0.15f, Quaternion.LookRotation(hit.point - hit.normal * 2), hit.transform, damagedAIController.BloodHoles);
				}

				damagedAIController.Damage(damage, bodyScript.bodyPart != BodyPartCollider.BodyPart.Head ? "bullet" : "headshot", attackerActorID);
				
				WeaponsHelper.InstantiateAdditionalBloodEffects(damagedAIController.additionalHitEffects, hit.point, Quaternion.LookRotation(hit.normal));
				// damagedAIController.health -= damage;
			}
		}
		
		 public static void DisableAllComponentsAfterDeath(AIController aiController)
        {
            aiController.anim.SetLayerWeight(1,0);
            
            if (aiController.weapon)
            {
	            if (!aiController.deleteWeaponAfterDeath)
	            {
		            aiController.weapon.transform.parent = null;
		            aiController.weapon.AddComponent<Rigidbody>();
	            }
	            else
	            {
		            Object.Destroy(aiController.weapon);
	            }
            }

#if USK_MULTIPLAYER
	        if (!PhotonNetwork.InRoom)
	        {
#endif
		        var index = Random.Range(0, aiController.itemsAppearingAfterDeath.Count);
		        
		        if (aiController.itemsAppearingAfterDeath.Count > 0 && aiController.itemsAppearingAfterDeath[index])
		        {
			        var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();

			        foreach (var item in aiController.itemsAppearingAfterDeath)
			        {
				        if (allObjectsOnScene.Contains(item))
				        {
					        item.SetActive(true);
					        item.transform.position = aiController.transform.position;
				        }
				        else
				        {
					        var instantiatedItem = Object.Instantiate(aiController.itemsAppearingAfterDeath[index], aiController.transform.position, Quaternion.identity);
					        instantiatedItem.name = Helper.CorrectName(instantiatedItem.name);

					        if (instantiatedItem.GetComponent<Rigidbody>())
					        {
						        instantiatedItem.GetComponent<Rigidbody>().velocity = Vector3.up * 5;
					        }
					        
					        break;
				        }
			        }
		        }
		        
#if USK_MULTIPLAYER  
	        }
#endif

	        
	        if(aiController.statsCanvas)
                aiController.statsCanvas.gameObject.SetActive(false);

            aiController.enabled = false;
            aiController.agent.enabled = false;

            if (!aiController.DeathAnimation)
            {
	            aiController.anim.enabled = false;
	            
	            if (aiController.isHuman)
	            {
		            foreach (var part in aiController.BodyParts)
		            {
			            part.GetComponent<Rigidbody>().isKinematic = false;
		            }

		            AddRagdollScripts(aiController);
	            }
	            else
	            {
		            if (aiController.ragdoll)
		            {
			            var _ragdoll = Object.Instantiate(aiController.ragdoll, aiController.transform.position, aiController.transform.rotation);
			            AddRagdollScripts(_ragdoll, aiController);
		            }

#if USK_MULTIPLAYER
		            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
		            {
			            if (PhotonNetwork.IsMasterClient && !aiController.multiplayerBot)
				            PhotonNetwork.Destroy(aiController.gameObject);
		            }
		            else
#endif
		            {
			            Object.Destroy(aiController.gameObject);
		            }
	            }
            }
            else
            {
	            aiController.anim.CrossFade("Death Animation", 0.1f, 0);
	            AddRagdollScripts(aiController);
            }
        }

		 static void AddRagdollScripts(GameObject ragdoll, AIController aiController)
		 {
			 var destroyRagdollTime = aiController.destroyRagdollTime;

#if USK_ADVANCED_MULTIPLAYER
			 if (aiController.advancedRoomManager)
			 {
				 destroyRagdollTime = aiController.advancedRoomManager.restartGameAfterPlayerDeathTimeout;
			 }
#endif
			 var destroyScript = ragdoll.AddComponent<DestroyObject>();
			 destroyScript.destroyTime = destroyRagdollTime;

			 var blipScript = ragdoll.AddComponent<Blip>();

			 if (aiController.currentUIManager && aiController.blipRawImage)
			 {
				 blipScript.blipImage = new UIHelper.MinimapImage{image = aiController.blipRawImage};
				 blipScript.uiManager = aiController.currentUIManager;
				 blipScript.rotateWithObject = aiController.rotateBlipWithEnemy;
			 }
		 }

		 static void AddRagdollScripts(AIController aiController = null)
		 {
			 var destroyRagdollTime = aiController.destroyRagdollTime;

#if USK_ADVANCED_MULTIPLAYER
			 if (aiController.advancedRoomManager)
			 {
				 destroyRagdollTime = aiController.advancedRoomManager.restartGameAfterPlayerDeathTimeout;
			 }
#endif
			 var destroyScript = aiController.gameObject.AddComponent<DestroyObject>();
			 destroyScript.destroyTime = destroyRagdollTime;

			 var blipScript = aiController.gameObject.AddComponent<Blip>();

			 if (aiController.currentUIManager && aiController.blipRawImage)
			 {
				 blipScript.blipImage = new UIHelper.MinimapImage{image = aiController.blipRawImage};
				 blipScript.uiManager = aiController.currentUIManager;
				 blipScript.rotateWithObject = aiController.rotateBlipWithEnemy;
			 }
		 }

		 public static Transform GenerateAttackMovementPointOnNavMesh(Transform currentTarget, float distance, ref int functionCount, int curAngle)
		{
			Transform point = null;
			
			functionCount++;
			
			var finalPosition = RandomPointInCircle(currentTarget, distance, Random.Range(-50, 50));

			NavMeshHit hit;
            
			var onNavMesh = false;

			if (NavMesh.SamplePosition(finalPosition, out hit, 1000, NavMesh.AllAreas))
			{
				if (hit.distance < 3)
					onNavMesh = true;
			}
			
			if (Physics.Linecast(finalPosition, currentTarget.position + Vector3.up, Helper.LayerMask()))
			{
				onNavMesh = false;
			}


			if (onNavMesh)
			{
				point = CreatePointToCheck(finalPosition, "attack");
			}
			else
			{
				if (functionCount < 3)
				{
					point = GenerateAttackMovementPointOnNavMesh(currentTarget, distance, ref functionCount, Random.Range(-100, 100));
				}
			}
			
			if(point) point.position = new Vector3(point.position.x, currentTarget.transform.position.y, point.position.z);
			
			return point;
		}


		static Vector3 RandomPointInCircle(Transform trans, float radius, float angle)
		{
			var rad = angle * Mathf.Deg2Rad;
			var position = trans.right * Mathf.Sin(rad) - trans.forward * Mathf.Cos(rad);
			return trans.position + position * radius;
		}
		
		public static Transform RandomNavmeshPoint(Transform baseTransform, int radius, string type, int areaMask, Transform point = null, Transform aiTransform = null) {
			
			var randomDirection = type == "attack" ? GetPointOnUnitSphereCap(aiTransform.rotation) : Random.insideUnitSphere;
			randomDirection *= radius;
			randomDirection += baseTransform.position;
			
			var finalPosition = Vector3.zero;
			if (NavMesh.SamplePosition(randomDirection, out var hit, radius, areaMask)) {
				finalPosition = hit.position;            
			}

			if (point == null)
			{
				point = CreatePointToCheck(finalPosition, type);
			}
			else
			{
				point.gameObject.SetActive(true);
				point.position = new Vector3(finalPosition.x, finalPosition.y, finalPosition.z);
			}

			return point;
		}

		private static Vector3 GetPointOnUnitSphereCap(Quaternion targetDirection)
		{
			var angleInRad = Random.Range(90, 180) * Mathf.Deg2Rad;
			var PointOnCircle = Random.insideUnitCircle.normalized * (Random.Range(0.4f, 1f) * Mathf.Sin(angleInRad));
			var V = new Vector3(PointOnCircle.x, PointOnCircle.y,Mathf.Cos(angleInRad));
			return targetDirection * V;
		}

		// public static Transform GeneratePointOnNavMesh(Vector3 position, Vector3 direction, float distance, ref int functionCount, bool randomNextDirection, string type, Transform currentTarget, Transform point = null)
		// {
		// 	functionCount++;
		// 	direction.Normalize();
		//
		// 	var finalPosition = position + direction * distance;
		//
		// 	NavMeshHit hit;
  //           
		// 	var onNavMesh = false;
		//
		// 	if (NavMesh.SamplePosition(finalPosition, out hit, 50, NavMesh.AllAreas))
		// 	{
		// 		if (hit.distance < 1)
		// 			onNavMesh = true;
		// 	}
		//
		// 	if (type == "attack")
		// 	{
		// 		if (Physics.Linecast(finalPosition + Vector3.up, currentTarget.position + Vector3.up, out var hitInfo, Helper.LayerMask()))
		// 		{
		// 			if(!hitInfo.collider.transform.root.GetComponent<Controller>() && !hitInfo.collider.transform.root.GetComponent<AIController>())
		// 				onNavMesh = false;
		// 		}
		// 	}
		//
		// 	if (!point)
		// 	{
		// 		if (onNavMesh)
		// 		{
		// 			point = CreatePointToCheck(finalPosition, type);
		// 		}
		// 		else
		// 		{
		// 			if (functionCount < 7)
		// 			{
		// 				if (randomNextDirection)
		// 					direction = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
		// 				else
		// 					distance /= 2;
		//
		// 				point = GeneratePointOnNavMesh(position, direction, distance, ref functionCount, randomNextDirection, type, currentTarget);
		// 			}
		// 		}
		//
		// 		if (point)
		// 			point.position = new Vector3(point.position.x, position.y, point.position.z);
		// 	}
		// 	else
		// 	{
		// 		point.gameObject.SetActive(true);
		// 		point.position = new Vector3(finalPosition.x, finalPosition.y, finalPosition.z);
		// 	}
		// 	
		// 	
		// 	return point;
		// }
		
#if UNITY_EDITOR && USK_MULTIPLAYER

		public static void AddMultiplayerScripts(GameObject enemy)
		{
			var photonView = !enemy.GetComponent<PhotonView>() ? enemy.AddComponent<PhotonView>() : enemy.GetComponent<PhotonView>();
			
			var photonAnimatorView = !enemy.GetComponent<PhotonAnimatorView>() ? enemy.AddComponent<PhotonAnimatorView>() : enemy.GetComponent<PhotonAnimatorView>();
			
			SetAnimatorParameters(photonAnimatorView);

			var transformView = !enemy.GetComponent<PhotonTransformViewClassic>() ? enemy.AddComponent<PhotonTransformViewClassic>() : enemy.GetComponent<PhotonTransformViewClassic>();
			transformView.m_PositionModel.SynchronizeEnabled = true;
			transformView.m_RotationModel.SynchronizeEnabled = true;
			transformView.m_RotationModel.InterpolateOption = PhotonTransformViewRotationModel.InterpolateOptions.Lerp;
			
			if (photonView && photonAnimatorView && transformView)
			{
				photonView.ObservedComponents = new List<Component> {photonAnimatorView , transformView};
				photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
			}
		}

		public static void SetAnimatorParameters(PhotonAnimatorView photonAnimatorView)
		{
			photonAnimatorView.SetLayerSynchronized(0, PhotonAnimatorView.SynchronizeType.Disabled);
			photonAnimatorView.SetLayerSynchronized(1, PhotonAnimatorView.SynchronizeType.Disabled);
			
			photonAnimatorView.SetParameterSynchronized("AttentionValue", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Disabled);
			photonAnimatorView.SetParameterSynchronized("SpeedOffset", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Angle", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
			photonAnimatorView.SetParameterSynchronized("AngleToTarget", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
			photonAnimatorView.SetParameterSynchronized("DistanceToTarget", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
			photonAnimatorView.SetParameterSynchronized("AllSidesMovement", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Melee", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Disabled);
			photonAnimatorView.SetParameterSynchronized("Reload", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Covers State", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Move", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Attack", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Disabled);
			photonAnimatorView.SetParameterSynchronized("Find", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Run", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			photonAnimatorView.SetParameterSynchronized("Horizontal", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
			photonAnimatorView.SetParameterSynchronized("Vertical", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
			photonAnimatorView.SetParameterSynchronized("Taking Damage", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Disabled);
			photonAnimatorView.SetParameterSynchronized("Aim", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
			
			EditorUtility.SetDirty(photonAnimatorView);
		}
		
		public static void RemoveMultiplayerScripts(GameObject enemy)
		{
			var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
			var isSceneObject = enemy.gameObject.scene.name == enemy.gameObject.name || allObjectsOnScene.Contains(enemy);

			var tempEnemy = isSceneObject ? enemy : (GameObject) PrefabUtility.InstantiatePrefab(enemy);
			
			if(tempEnemy.GetComponent<PhotonAnimatorView>())
				Object.DestroyImmediate(tempEnemy.GetComponent<PhotonAnimatorView>());
			
			if(tempEnemy.GetComponent<PhotonTransformViewClassic>())
				Object.DestroyImmediate(tempEnemy.GetComponent<PhotonTransformViewClassic>());
			
			if(tempEnemy.GetComponent<PhotonView>())
				Object.DestroyImmediate(tempEnemy.GetComponent<PhotonView>());

			if (!isSceneObject)
			{
#if !UNITY_2018_3_OR_NEWER
			PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

				Object.DestroyImmediate(tempEnemy);
			}
		}
#endif
	}
}

