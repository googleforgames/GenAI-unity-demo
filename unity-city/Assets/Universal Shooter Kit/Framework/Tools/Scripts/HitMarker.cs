using System;
using System.Collections;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	public class HitMarker : MonoBehaviour
	{
		[HideInInspector] public Vector3 targetPosition;
		[HideInInspector] public Controller player;
		[HideInInspector] public Transform enemy;
		private RawImage image;
		private Vector2 center;
		private float halfHeightScreen;
		private float halfWeightScreen;
		public float angleBetweenEnemyAndPlayer;
		public int circleRadius;
		private bool setPosition;

#if USK_ADVANCED_MULTIPLAYER
		private AdvancedRoomManager advancedRoomManager;
#endif
		
		void Start()
		{
			halfHeightScreen = Screen.height / 2;
			halfWeightScreen = Screen.width / 2;
			center = new Vector2(halfWeightScreen, halfHeightScreen);

			image = GetComponent<RawImage>();
			
			var vectorToObj = targetPosition - player.transform.position;
			vectorToObj.y = 0;
			
			StartCoroutine(DestroyTimeout());
			StartCoroutine(EnableColor());

#if USK_ADVANCED_MULTIPLAYER
			advancedRoomManager = FindObjectOfType<AdvancedRoomManager>();
#endif

		}

		void Update()
		{
			
#if USK_ADVANCED_MULTIPLAYER
			if (advancedRoomManager && advancedRoomManager.matchIsOver)
			{
				Destroy(gameObject);
				return;
			}
#endif
			if (!player || !player.CameraController || player && player.health <= 0)
			{
				Destroy(gameObject);
				return;
			}

			var objPos = new Vector2(transform.position.x, transform.position.y);
			var uiDir = center - objPos;
			var angle = Mathf.Atan2(uiDir.y, uiDir.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			var vectorToObj = targetPosition - player.transform.position;
			vectorToObj.y = 0;

			var camDir = Vector3.zero;
			if (player.TypeOfCamera == CharacterHelper.CameraType.TopDown && player.CameraParameters.lockCamera)
			{
				camDir = new Vector3(player.CameraController.Camera.transform.up.x, 0, player.CameraController.Camera.transform.up.z);
			}
			else
			{
				camDir = new Vector3(player.CameraController.Camera.transform.forward.x, 0, player.CameraController.Camera.transform.forward.z);
			}

			var angleBetweenCamAndAttackPoint = Helper.AngleBetween(camDir, vectorToObj);

			var dirFromAngle = Helper.DegreeToVector2(angleBetweenCamAndAttackPoint);
			var correctDir = new Vector2(dirFromAngle.y, dirFromAngle.x); // * -1;

			angleBetweenEnemyAndPlayer = angleBetweenCamAndAttackPoint;

			if (!setPosition)
			{
				foreach (var hitMarker in player.hitMarkers)
				{
					if (Mathf.Abs(Mathf.DeltaAngle(hitMarker.angleBetweenEnemyAndPlayer, angleBetweenCamAndAttackPoint)) < 50)
					{
						hitMarker.Restart();
						Destroy(gameObject);
						return;
					}
				}

				player.hitMarkers.Add(this);
				CalculateSize();

				setPosition = true;
			}

			angleBetweenCamAndAttackPoint = Mathf.Abs(angleBetweenCamAndAttackPoint);

			var dist = 0f;

			if (angleBetweenCamAndAttackPoint > 0 && angleBetweenCamAndAttackPoint <= 60)
			{
				dist = halfHeightScreen / Mathf.Cos(angleBetweenCamAndAttackPoint * Mathf.Deg2Rad);
			}
			else if (angleBetweenCamAndAttackPoint > 60 && angleBetweenCamAndAttackPoint <= 90)
			{
				dist = halfWeightScreen / Mathf.Cos((90 - angleBetweenCamAndAttackPoint) * Mathf.Deg2Rad);
			}
			else if (angleBetweenCamAndAttackPoint > 90 && angleBetweenCamAndAttackPoint <= 120)
			{
				dist = halfWeightScreen / Mathf.Cos((angleBetweenCamAndAttackPoint - 90) * Mathf.Deg2Rad);
			}
			else if (angleBetweenCamAndAttackPoint > 120 && angleBetweenCamAndAttackPoint <= 180)
			{
				dist = halfHeightScreen / Mathf.Cos((180 - angleBetweenCamAndAttackPoint) * Mathf.Deg2Rad);
			}
			
			var pos = center + correctDir.normalized * (circleRadius == -1 ? dist : (circleRadius * (Screen.currentResolution.height / 1080f)));

			pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height);

			objPos = pos;
			transform.position = objPos;
		}

		private void Restart()
		{
			StopAllCoroutines();
			StartCoroutine(DestroyTimeout());

			if (image.color.a < 0.3f)
				CalculateSize();

			image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
		}

		public void CalculateSize()
		{
			if (!enemy)
				return;

			var distance = Vector3.Distance(player.transform.position, enemy.position);

			if (distance < 10)
			{
				image.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			}

			if (distance >= 10 && distance < 30)
			{
				image.rectTransform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
			}
			else if (distance > 60)
			{
				image.rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
		}

		IEnumerator DestroyTimeout()
		{
			yield return new WaitForSeconds(2.5f);
			StartCoroutine(Destroy());
		}

		IEnumerator EnableColor()
		{
			yield return new WaitForSeconds(0.1f);
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
		}

		IEnumerator Destroy()
		{
			while (true)
			{
				var color = image.color;
				color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0, 1 * Time.deltaTime));
				image.color = color;

				if (Math.Abs(color.a) <= 0.1f)
				{
					Destroy(gameObject);
					player.hitMarkers.Remove(this);
					break;
				}

				yield return 0;
			}
		}
	}
}