// GercStudio
// © 2018-2020

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = System.Random;

namespace GercStudio.USK.Scripts
{
	public class CameraController : MonoBehaviour
	{
		public CameraController OriginalScript;

		public CharacterHelper.CameraOffset CameraOffset = new CharacterHelper.CameraOffset();

		public Controller Controller;

		private Transform targetLookAt;
		public Transform CameraPosition;
//		public Transform Crosshair;
		public Camera layerCamera;

		public float CrosshairOffsetX;
		public float CrosshairOffsetY = 300;

		public float maxMouseAbsolute;
		public float minMouseAbsolute;
		public float cameraMovementDistance = 5;
		public float cameraDistanse;

		public bool isCameraAimEnabled;
		public bool isDeepAimEnabled;
		public bool Occlusion;
		public bool cameraDebug;
		public bool setCameraType;
		public bool canViewTarget;
		public bool useCameraJoystick;
		public bool canUseCursorInPause;
		public bool cameraPause;
		public bool cameraOcclusion;

		public Camera Camera;
		public Camera AimCamera;
		
		public Vector2 targetDirection;
		public Vector2 _mouseAbsolute;
		public Vector2 _smoothMouse;

//		public Image upPart;
//		public Image leftPart;
//		public Image rightPart;
//		public Image downPart;
//		public Image middlePart;

public Vector3 desiredCameraPosition = Vector3.zero;
		private Vector3 desiredBodyLookAtPosition = Vector3.zero;
		private Vector3 _position = Vector3.zero;
		private Vector3 bodyLookAtPosition = Vector3.zero;
		public Vector3 LastMousePosition;

		public Vector2 mouseDelta;
		public Vector2 TPmouseDelta;

		private Vector2[] currentCrosshairPositions = new Vector2[5];

		private Vector2 GamePadAxis;
		private Vector2 LastGamepadAxis;
		
		private Quaternion desiredRotation;

		private List<GameObject> disabledObjects = new List<GameObject>();
		private Transform preOcclededCamera;
		public Transform MainCamera;
		public Transform BodyLookAt;
		private Transform body;

		public float currentSensitivityX;
		public float currentSensitivityY;
		public float mouseX;
		public float mouseY;
		private float velX;
		private float velY;
		private float velZ;
		private float normDepth;
		public float currentDistance;
		public float currentOffsetX;
		public float currentOffsetY;
		private float desiredDistance;
		private float floorHeight;
		private float desiredOffsetX;
		private float bobbingTime;
		private float lastBobbingTime;
		private float bobbingValue;
		private float curveTransitionValue;

		public float enableAimTextureTimeout;
		private float enableDeepAimTimeout;

//		private float desiredOffsetY;
		private float textuteAlpha;
		private float crosshairMultiplier = 1;
		private Vector3 cameraMovementDirection;

		private Vector2 mobileCameraStickDirection;
		private Vector2 MobileTouchjPointA, MobileTouchjPointB;

		private Vector3 lastControllerPosition;

		private bool canChangeCurve;
		private bool cursorImageHasChanged;
		public bool useGamepad;
		private bool canUseNewBobbingCurve;

		private AnimationCurve currentCurve;
		private AnimationCurve newCurve;

		private AnimationCurve fpIdleCurve;
		private AnimationCurve fpWalkCurve;
		private AnimationCurve fpRunCurve;
		private AnimationCurve fpCrouchCurve;
		
		public CharacterHelper.BobbingValues currentBobbingValues;

		private int touchId = -1;

		private Collider[] _occlusionCollisers = new Collider[2];

		private Coroutine deepAimDelayCoroutine;

		private void Awake()
		{
#if USK_MULTIPLAYER
			if (FindObjectOfType<LobbyManager>())
			{
				Destroy(gameObject);
				return;
			}
			
#if USK_ADVANCED_MULTIPLAYER 
			if (FindObjectOfType<AdvancedLobbyManager>())
			{
				Destroy(gameObject);
				return;
			}
#endif
#endif
			
			Camera = GetComponent<Camera>();

			if (!MainCamera)
			{
				MainCamera = new GameObject("Camera <-> " + Controller.name).transform;
				MainCamera.name = Helper.CorrectName(MainCamera.name);
				
				// MainCamera.gameObject.AddComponent<Camera>().enabled = false;

				if (Controller.AdjustmentScene)
					MainCamera.hideFlags = HideFlags.HideInHierarchy;

				if(Controller.isRemoteCharacter)
					MainCamera.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}

			if (!Controller)
			{
				Debug.Log("Disconnect between camera and controller");
				Debug.Break();
			}
		}

		public void InitializeParameters()
		{
			if (!layerCamera)
			{
				layerCamera = Helper.NewCamera("LayerCamera", transform, "CameraController");
				layerCamera.gameObject.SetActive(false);
				layerCamera.hideFlags = HideFlags.HideInHierarchy;
			}

			normDepth = Camera.fieldOfView;

			if (Controller.UIManager)
			{
				if(Controller.UIManager.CharacterUI.aimPlaceholder)
					Controller.UIManager.CharacterUI.aimPlaceholder.gameObject.SetActive(false);
				
				if(Controller.UIManager.leftScopeTextureFill)
					Controller.UIManager.leftScopeTextureFill.gameObject.SetActive(false);
				
				if(Controller.UIManager.rightScopeTextureFill)
					Controller.UIManager.rightScopeTextureFill.gameObject.SetActive(false);
			}

			preOcclededCamera = new GameObject("preOclCamera").transform;
			preOcclededCamera.parent = MainCamera;
			preOcclededCamera.localPosition = Vector3.zero;
			preOcclededCamera.hideFlags = HideFlags.HideInHierarchy;

			transform.parent = MainCamera;
			transform.position = new Vector3(0, 0, 0);
			transform.rotation = Quaternion.Euler(0, 0, 0);

			body = Controller.BodyObjects.TopBody;
			BodyLookAt = new GameObject("BodyLookAt").transform;
			BodyLookAt.hideFlags = HideFlags.HideInHierarchy;
			
			LastGamepadAxis = new Vector2(Controller.transform.forward.x, Controller.transform.forward.z);
			targetDirection = MainCamera.localEulerAngles;

			if (Controller.CameraParameters.alwaysTPAimMode)
			{
				CameraOffset.normCameraOffsetX = CameraOffset.aimCameraOffsetX;
				CameraOffset.normCameraOffsetY = CameraOffset.aimCameraOffsetY;
				CameraOffset.Distance = CameraOffset.aimDistance;
			}

			CameraOffset.tpCameraOffsetX = CameraOffset.normCameraOffsetX;
			CameraOffset.tpCameraOffsetY = CameraOffset.normCameraOffsetY;
			CameraOffset.Distance = CameraOffset.normDistance;

			_position = Controller.BodyObjects.Head.transform.position;

			if (CameraPosition)
			{
				CameraPosition.parent = Controller.BodyObjects.Head;
				CameraPosition.localPosition = CameraOffset.cameraObjPos;
				CameraPosition.localEulerAngles = CameraOffset.cameraObjRot;
			}
			else
			{
				Debug.LogError("<color=red>Missing component</color>: [Camera position]", gameObject);
				Debug.Break();
			}

			if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
			{
				Helper.CameraExtensions.LayerCullingHide(Camera, "Head");
				Helper.CameraExtensions.LayerCullingHide(AimCamera, "Head");
				Helper.CameraExtensions.LayerCullingHide(layerCamera, "Head");
			}
			
			Helper.CameraExtensions.LayerCullingHide(AimCamera, "Character");
			
			AimCamera.gameObject.SetActive(false);

			SetAnimVariables();

			setCameraType = true;
			
			if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
			{
				mouseX = Controller.transform.localEulerAngles.y;
				mouseY = Controller.transform.localEulerAngles.x;
				
				if(Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown)
					BodyLookAt.position = Controller.transform.position;
			}
			else
			{
				mouseX = 0;
				mouseY = 0;
			}
			
			if(Controller.UIManager.CharacterUI.aimPlaceholder)
				Controller.UIManager.CharacterUI.aimPlaceholder.color = new Color(1,1,1,0);

			if (Controller.UIManager.leftFillImageComponent)
			{
				var color = Controller.UIManager.leftFillImageComponent.color;
				color.a = 0;
			}

			if (Controller.UIManager.rightFillImageComponent)
			{
				var color = Controller.UIManager.rightFillImageComponent.color;
				color.a = 0;			
			}
			
			Reset();
		}

		public void InitializeHeadBobbingValues(WeaponController weaponController)
		{
			if (!weaponController)
			{
				currentBobbingValues = Controller.CameraParameters.bobbingValues;
				// fpIdleCurve = new AnimationCurve(new Keyframe(0, -Controller.CameraParameters.bobbingValues.bobbingAmplitude / 3), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration, Controller.CameraParameters.bobbingValues.bobbingAmplitude / 3), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration * 2, -Controller.CameraParameters.bobbingValues.bobbingAmplitude / 3));
				// fpWalkCurve = new AnimationCurve(new Keyframe(0, -Controller.CameraParameters.bobbingValues.bobbingAmplitude), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration / 2, Controller.CameraParameters.bobbingValues.bobbingAmplitude), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration, -Controller.CameraParameters.bobbingValues.bobbingAmplitude));
				// fpRunCurve = new AnimationCurve(new Keyframe(0, -Controller.CameraParameters.bobbingValues.bobbingAmplitude * 1.5f), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration / 4, Controller.CameraParameters.bobbingValues.bobbingAmplitude * 1.5f), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration / 2, -Controller.CameraParameters.bobbingValues.bobbingAmplitude * 1.5f));
				// fpCrouchCurve = new AnimationCurve(new Keyframe(0, -Controller.CameraParameters.bobbingValues.bobbingAmplitude / 2), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration / 2, Controller.CameraParameters.bobbingValues.bobbingAmplitude / 2), new Keyframe(Controller.CameraParameters.bobbingValues.bobbingDuration, -Controller.CameraParameters.bobbingValues.bobbingAmplitude / 2));
			}
			else
			{
				currentBobbingValues = !weaponController.bobbingValues.useCommonParameters ? weaponController.bobbingValues : Controller.CameraParameters.bobbingValues;
			}
			
			SetBoobingValues(currentBobbingValues.bobbingAmplitude, currentBobbingValues.bobbingDuration);

			currentCurve = fpIdleCurve;
			newCurve = currentCurve;
		}

		void SetBoobingValues(float amplitude, float duration)
		{
			fpIdleCurve = new AnimationCurve(new Keyframe(0, -amplitude / 3), new Keyframe(duration, amplitude / 3), new Keyframe(duration * 2, -amplitude / 3));
			fpWalkCurve = new AnimationCurve(new Keyframe(0, -amplitude), new Keyframe(duration / 2, amplitude), new Keyframe(duration, -amplitude));
			fpRunCurve = new AnimationCurve(new Keyframe(0, -amplitude * 1.5f), new Keyframe(duration / 4, amplitude * 1.5f), new Keyframe(duration / 2, -amplitude * 1.5f));
			fpCrouchCurve = new AnimationCurve(new Keyframe(0, -amplitude / 2), new Keyframe(duration / 2, amplitude / 2), new Keyframe(duration, -amplitude / 2));
		}

		void Update()
		{
			if (Controller && !Controller.ActiveCharacter)
			{
				// if (GetComponent<Camera>().enabled)
				// 	GetComponent<Camera>().enabled = false;

				if (AimCamera.enabled)
					AimCamera.enabled = false;
			}

			InputHelper.CheckGamepad(ref useGamepad, Controller.projectSettings);
			ControlCrosshair();
			
			if (!isCameraAimEnabled)
			{
				if (Math.Abs(CameraOffset.tpCameraOffsetX - CameraOffset.normCameraOffsetX) > 0.01f)
					CameraOffset.tpCameraOffsetX = Mathf.Lerp(CameraOffset.tpCameraOffsetX, CameraOffset.normCameraOffsetX, 5 * Time.deltaTime);

				if (Math.Abs(CameraOffset.tpCameraOffsetY - CameraOffset.normCameraOffsetY) > 0.01f)
					CameraOffset.tpCameraOffsetY = Mathf.Lerp(CameraOffset.tpCameraOffsetY, CameraOffset.normCameraOffsetY, 5 * Time.deltaTime);

//				if (Vector3.Distance(CameraOffset.tpCameraRotationOffset, CameraOffset.cameraNormRotationOffset) > 0.1f)
//					CameraOffset.tpCameraRotationOffset = Vector3.Lerp(CameraOffset.tpCameraRotationOffset, CameraOffset.cameraNormRotationOffset, 5 * Time.deltaTime);

				if (Math.Abs(CameraOffset.Distance - CameraOffset.normDistance) > 0.1f)
				{
					CameraOffset.Distance = Mathf.Lerp(CameraOffset.Distance, CameraOffset.normDistance, 10 * Time.deltaTime);
					Reset();
				}
				
				if(Controller.AdjustmentScene)
					Reset();
				
				// if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				// {
				// 	if (CameraPosition)
				// 	{
				// 		CameraPosition.localPosition = Vector3.Lerp(CameraPosition.localPosition, CameraOffset.cameraObjPos, CameraOffset.changeCameraSpeed * Time.deltaTime);
				// 		CameraPosition.localRotation = Quaternion.Lerp(CameraPosition.localRotation, Quaternion.Euler(CameraOffset.cameraObjRot), CameraOffset.changeCameraSpeed * Time.deltaTime);
				// 	}
				// }
			}
			else
			{
				// if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				// {
				// 	if (CameraPosition)
				// 	{
				// 		CameraPosition.localPosition = Vector3.Lerp(CameraPosition.localPosition, CameraOffset.aimCameraObjPos, CameraOffset.changeCameraSpeed * Time.deltaTime);
				// 		CameraPosition.localRotation = Quaternion.Lerp(CameraPosition.localRotation, Quaternion.Euler(CameraOffset.aimCameraObjRot), CameraOffset.changeCameraSpeed * Time.deltaTime);
				// 	}
				// }
				
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !Controller.emulateTDModeLikeTP || Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson)
				{
					if (Math.Abs(CameraOffset.tpCameraOffsetX - CameraOffset.aimCameraOffsetX) > 0.1f)
						CameraOffset.tpCameraOffsetX = Mathf.Lerp(CameraOffset.tpCameraOffsetX, CameraOffset.aimCameraOffsetX, 5 * Time.deltaTime);

					if (Math.Abs(CameraOffset.tpCameraOffsetY - CameraOffset.aimCameraOffsetY) > 0.1f)
						CameraOffset.tpCameraOffsetY = Mathf.Lerp(CameraOffset.tpCameraOffsetY, CameraOffset.aimCameraOffsetY, 5 * Time.deltaTime);

//				if (Vector3.Distance(CameraOffset.tpCameraRotationOffset, CameraOffset.cameraAimRotationOffset) > 0.1f)
//					CameraOffset.tpCameraRotationOffset = Vector3.Lerp(CameraOffset.tpCameraRotationOffset, CameraOffset.cameraAimRotationOffset, 5 * Time.deltaTime);

					if (Math.Abs(CameraOffset.Distance - CameraOffset.aimDistance) > 0.1f)
					{
						CameraOffset.Distance = Mathf.Lerp(CameraOffset.Distance, CameraOffset.aimDistance, 10 * Time.deltaTime);
						Reset();
					}

					if (Controller.AdjustmentScene)
						Reset();
				}
			}
			

			layerCamera.fieldOfView = Camera.fieldOfView;
			
			if (cameraDebug)
				Reset();


			desiredOffsetX = currentOffsetX;

			if (Controller.AdjustmentScene)
			{
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, isCameraAimEnabled ? Controller.CameraParameters.tpAimDepth : normDepth, 15 * Time.deltaTime);
				}
				
				return;
			}
			
			var speed = Controller.inventoryManager.WeaponController ? Controller.inventoryManager.WeaponController.aimingSpeed : 1;
			
			enableAimTextureTimeout += Time.deltaTime;
			
			if (isCameraAimEnabled)
			{
				switch (Controller.TypeOfCamera)
				{
					case CharacterHelper.CameraType.FirstPerson:

						Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, Controller.CameraParameters.fpAimDepth, speed * 10 * Time.deltaTime);

						if (Controller.inventoryManager.WeaponController && Controller.inventoryManager.WeaponController.useAimTexture && enableAimTextureTimeout > 0.1f) //&& Controller.inventoryManager.WeaponController.canEnableAimTexture)
						{
							AimCamera.targetTexture = null;
							AimCamera.gameObject.SetActive(true);
							AimCamera.fieldOfView = Mathf.Lerp(AimCamera.fieldOfView, Controller.inventoryManager.WeaponController.aimTextureDepth, speed * 10 * Time.deltaTime);

							if (Controller.UIManager.CharacterUI.aimPlaceholder)
							{
								Controller.UIManager.CharacterUI.aimPlaceholder.gameObject.SetActive(true);
								Controller.UIManager.leftFillImageComponent.gameObject.SetActive(true);
								Controller.UIManager.rightFillImageComponent.gameObject.SetActive(true);

								var color = Controller.UIManager.CharacterUI.aimPlaceholder.color;
								color.a = Mathf.Lerp(color.a, 1, 10 * Time.deltaTime);
								Controller.UIManager.CharacterUI.aimPlaceholder.color = color;

								UIHelper.SetFillColor(Controller.UIManager.leftFillImageComponent, color.a);
								UIHelper.SetFillColor(Controller.UIManager.rightFillImageComponent, color.a);

								if (color.a > 0.5f)
								{
									Controller.UIManager.CharacterUI.DisableAll();
									Controller.UIManager.DisableAllBlips();
								}
							}
						}

						break;
					case CharacterHelper.CameraType.ThirdPerson:
						
						
						Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, !Controller.emulateTDModeLikeTP ? Controller.CameraParameters.tpAimDepth : normDepth, speed * 10 * Time.deltaTime);
						DisableAimTextures();
						
						break;
				}
			}
			else
			{
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, Controller.isAlwaysTpAimEnabled ? Controller.CameraParameters.tpAimDepth : normDepth, speed * 10 * Time.deltaTime);
				}
				else
				{
					Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, normDepth, speed * 10 * Time.deltaTime);
				}

				DisableAimTextures();
			}

		}
		
		void LateUpdate()
		{
			if(isCameraAimEnabled && Controller.inventoryManager.WeaponController && Controller.inventoryManager.WeaponController.useScope && Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				bobbingTime += 0.5f * Time.deltaTime;
			else bobbingTime += Time.deltaTime;
			
			if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
			{
				if (!CameraPosition)
					return;

				FPCameraUpdate();
			}
			else
			{
				if (setCameraType && (Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && !Controller.CameraParameters.lockCamera))
					GetMouseAxis();

				switch (Controller.TypeOfCamera)
				{
					case CharacterHelper.CameraType.ThirdPerson:

						if (!Controller.emulateTDModeLikeTP)
						{
							TPCheckIfCameraOccluded();
							ChangeCurrentPosition();
							TPCalculateDesiredPosition();
						}
						else
						{
							TDCalculateDesiredPosition();
							// CharacterHelper.CheckCameraPoints(Controller.BodyObjects.Head.position, transform.position, disabledObjects, Camera);
						}

						break;

					case CharacterHelper.CameraType.TopDown:

						TDCalculateDesiredPosition();
						// CharacterHelper.CheckCameraPoints(Controller.BodyObjects.Head.position, transform.position, disabledObjects, Camera);

						break;
				}

				if (Time.timeScale > 0)
					UpdateCameraPosition();
			}
		}

		void ChangeCurrentPosition()
		{
			currentDistance = Mathf.Lerp(currentDistance, desiredDistance, 0.6f);
			currentOffsetX = Mathf.Lerp(currentOffsetX, desiredOffsetX, 0.6f);
		}

		void TDCalculateDesiredPosition()
		{
			RaycastHit hit;
			floorHeight = Physics.Raycast(Controller.BodyObjects.Hips.position, Vector3.down, out hit, 100, Helper.LayerMask()) ? hit.point.y : Controller.transform.position.y;
			desiredCameraPosition = CharacterHelper.CalculatePosition(mouseX, 10, Controller, CameraOffset.TopDownAngle - 10, floorHeight, "camera");
		}

		void TPCalculateDesiredPosition()
		{
			RaycastHit hit;
			floorHeight = Physics.Raycast(Controller.BodyObjects.Hips.position, Vector3.down, out hit, 100, Helper.LayerMask()) ? hit.point.y : Controller.transform.position.y;
			
			desiredCameraPosition = CharacterHelper.CalculatePosition(mouseY, mouseX, cameraMovementDistance, Controller, floorHeight, Controller.isJump);
			desiredBodyLookAtPosition = CharacterHelper.CalculatePosition(mouseY, mouseX, 5, Controller, floorHeight, Controller.isJump);
		}

		void UpdateCameraPosition()
		{
			switch (Controller.TypeOfCamera)
			{
				case CharacterHelper.CameraType.ThirdPerson:
					if (!Controller.emulateTDModeLikeTP)
					{
						TPCameraUpdate();
					}
					else
					{
						TDCameraUpdate();
					}
					break;

				case CharacterHelper.CameraType.TopDown:
					TDCameraUpdate();
					break;
			}

			if (body)
				Controller.BodyLookAt(BodyLookAt);

			Controller.CharacterRotation();
		}

		public IEnumerator CameraTimeout()
		{
			yield return new WaitForSeconds(0.01f);
			canViewTarget = true;
		}

		private IEnumerator DeepAimDelay()
		{
			if (!isDeepAimEnabled)
			{
				yield return new WaitForSeconds(0);
				
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					if (!isCameraAimEnabled)
						Aim();

					Controller.ChangeCameraType(CharacterHelper.CameraType.FirstPerson);
					
					isDeepAimEnabled = true;

					SetSensitivity();
				}
			}
			else
			{
				yield return new WaitForSeconds(0);
				
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				{
					if (isCameraAimEnabled)
						Aim();
					
					Controller.ChangeCameraType(CharacterHelper.CameraType.ThirdPerson);
					isDeepAimEnabled = false;
				}
			}
			
		}

		void TPCameraUpdate()
		{

			var posX = Mathf.SmoothDamp(_position.x, desiredCameraPosition.x, ref velX, Controller.CameraParameters.tpSmoothX);
			var posY = Mathf.SmoothDamp(_position.y, desiredCameraPosition.y, ref velY, Controller.CameraParameters.tpSmoothY);
			var posZ = Mathf.SmoothDamp(_position.z, desiredCameraPosition.z, ref velZ, Controller.CameraParameters.tpSmoothX);
			_position = new Vector3(posX, posY, posZ);

			if (setCameraType)
			{
				if (Time.timeScale > 0)
					MainCamera.position = _position;

				transform.localEulerAngles = new Vector3(-1, 0, 0);
				transform.localPosition = new Vector3(currentOffsetX, currentOffsetY, currentDistance);

				if (Controller.CameraFollowCharacter)
					MainCamera.LookAt(Controller.BodyObjects.Head);
				else
				{
					if (Controller.headHeight == -1)
						MainCamera.LookAt(Controller.BodyObjects.Head);
					else
					{
						if (!Controller.isJump && !Controller.isCrouch)
							MainCamera.LookAt(new Vector3(Controller.transform.position.x, floorHeight + Controller.headHeight, Controller.transform.position.z));
						else MainCamera.LookAt(Controller.BodyObjects.Head);
					}
				}
			}
			else
			{

				if (Controller.PreviousTypeOfCamera == CharacterHelper.CameraType.TopDown)
				{
					mouseY = 0;
				}
				
				transform.parent = null;
				MainCamera.position = _position;
				
				var additionalPos = MainCamera.TransformPoint(new Vector3(currentOffsetX, currentOffsetY, currentDistance));
				
				MoveCameraToTargetPosition(additionalPos, Quaternion.identity, false);
				
				MainCamera.LookAt(Controller.BodyObjects.Head);
				transform.LookAt(Controller.BodyObjects.Head);

				if (Controller.PreviousTypeOfCamera == CharacterHelper.CameraType.TopDown)
				{
					transform.eulerAngles = new Vector3(transform.eulerAngles.x, MainCamera.eulerAngles.y, transform.eulerAngles.z) + new Vector3(-1, 0, 0);
				}
				else
				{
					transform.eulerAngles = new Vector3(MainCamera.eulerAngles.x, MainCamera.eulerAngles.y, transform.eulerAngles.z) + new Vector3(-1, 0, 0);

				}
			}
			
			bodyLookAtPosition = desiredBodyLookAtPosition;

			BodyLookAt.position = bodyLookAtPosition;
			BodyLookAt.RotateAround(Controller.BodyObjects.Head.position, Vector3.right, 180);

			var newPos = BodyLookAt.position;
			BodyLookAt.position = bodyLookAtPosition;
			BodyLookAt.RotateAround(Controller.BodyObjects.Head.position, Vector3.up, 185);

			BodyLookAt.position = new Vector3(BodyLookAt.position.x, newPos.y, BodyLookAt.position.z);
		}

		void TDCameraUpdate()
		{
			transform.localEulerAngles = Vector3.zero;

			if (Controller.CameraParameters.lockCamera)
			{
				transform.localEulerAngles = new Vector3(CameraOffset.tdLockCameraAngle, 0, 0);
				MainCamera.eulerAngles = Vector3.zero;
			}

			var posX = Mathf.SmoothDamp(_position.x, desiredCameraPosition.x, ref velX, Controller.CameraParameters.tdSmoothX);
			var posY = Mathf.SmoothDamp(_position.y, desiredCameraPosition.y, ref velY, Controller.CameraParameters.tdSmoothX);
			var posZ = Mathf.SmoothDamp(_position.z, desiredCameraPosition.z, ref velZ, Controller.CameraParameters.tdSmoothX);
			_position = new Vector3(posX, posY, posZ);

			if (setCameraType)
			{
				if (Controller.CameraParameters.lockCamera)
				{
					MainCamera.position = new Vector3(Controller.transform.position.x, floorHeight + Vector3.up.y * 10, Controller.transform.position.z);
					transform.localPosition = new Vector3(CameraOffset.tdLockCameraOffsetX, CameraOffset.TDLockCameraDistance, CameraOffset.tdLockCameraOffsetY);
				}
				else
				{
					MainCamera.position = _position;
					transform.localPosition = new Vector3(CameraOffset.tdCameraOffsetX, CameraOffset.tdCameraOffsetY, CameraOffset.TD_Distance);
					MainCamera.LookAt(new Vector3(Controller.transform.position.x, floorHeight + Controller.headHeight, Controller.transform.position.z));
				}
			}
			else
			{
				if (!Controller.CameraParameters.lockCamera)
				{
					
					transform.parent = null;
					MainCamera.position = _position;
				
					var additionalPos = MainCamera.TransformPoint(new Vector3(CameraOffset.tdCameraOffsetX, CameraOffset.tdCameraOffsetY, CameraOffset.TD_Distance));
				
					MoveCameraToTargetPosition(additionalPos, Quaternion.identity, false);
				
					MainCamera.LookAt(new Vector3(Controller.transform.position.x, floorHeight + Controller.headHeight, Controller.transform.position.z));
					transform.eulerAngles = MainCamera.eulerAngles;
				}
				else
				{
					var pos = new Vector3(Controller.transform.position.x, floorHeight + Vector3.up.y * 10, Controller.transform.position.z);
					var localPos = new Vector3(CameraOffset.tdLockCameraOffsetX, CameraOffset.TDLockCameraDistance, CameraOffset.tdLockCameraOffsetY);

					transform.parent = null;
					MainCamera.position = pos;
				
					var additionalPos = MainCamera.TransformPoint(localPos);
				
					MoveCameraToTargetPosition(additionalPos, Quaternion.identity, false);
				
					transform.eulerAngles = new Vector3(CameraOffset.tdLockCameraAngle, 0, 0);
					MainCamera.eulerAngles = Vector3.zero;
				}
			}

			if (Controller.CameraParameters.lockCamera)
			{
				var height = Controller.defaultHeight > 0 ? Controller.defaultHeight : Controller.BodyObjects.Hips.position.y - Controller.transform.position.y;

				if (!Controller.isPause && !cameraPause)
				{
					if (!useGamepad && !Application.isMobilePlatform && !Controller.projectSettings.mobileDebug)
					{
						if (Physics.Raycast(transform.position, transform.forward, out var hit, 100, Helper.LayerMask()))
						{
							cameraDistanse = hit.distance - height;
						}
						else
						{
							cameraDistanse = Vector3.Distance(Controller.BodyObjects.Hips.position, transform.position);
						}

						var mousePosition = Mouse.current.position.ReadValue();
						var cursorPosition = Controller.CameraParameters.CursorImage ? new Vector3(mousePosition.x + Controller.CameraParameters.CursorImage.texture.height / 2, mousePosition.y - Controller.CameraParameters.CursorImage.texture.width / 2, cameraDistanse) : new Vector3(mousePosition.x, mousePosition.y, cameraDistanse);

						LastMousePosition = Camera.ScreenToWorldPoint(cursorPosition);

						cursorPosition.z = 0;//Input.mousePosition.z;

						var point = LastMousePosition;

						if (Controller.CameraParameters.lookAtCursor)
						{
							if ((Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].weaponSlotInGame.Count > 0 && !Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].weaponSlotInGame[Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].currentWeaponInSlot].fistAttack
							     || Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].weaponSlotInGame.Count == 0) &&
							    (Controller.inventoryManager.WeaponController && Controller.inventoryManager.WeaponController.Attacks[Controller.inventoryManager.WeaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee || !Controller.inventoryManager.WeaponController))
							{
								cursorPosition.z = 0;//Input.mousePosition.z;
								if (Physics.Raycast(Camera.ScreenPointToRay(cursorPosition), out hit, 1000, Helper.LayerMask()))
								{
									point = hit.point;

									if (hit.transform.root.gameObject.GetComponent<Controller>() || hit.transform.root.gameObject.GetComponent<AIController>())
										point.y = hit.transform.root.position.y;
								}
							}
						}

						var position = BodyLookAt.position;
						var speed = Mathf.Abs(point.y - position.y) > 1 ? 5 : 100;
						position = Vector3.MoveTowards(position, point, Controller.CameraParameters.tdXMouseSensitivity * speed * Time.deltaTime);
						BodyLookAt.position = position;
					}
					else
					{
						GetMouseAxis();

						var dir = GamePadAxis * 100;
						dir.Normalize();

						if (dir.magnitude > 0.9)
						{
							LastGamepadAxis = dir;
						}

						BodyLookAt.position = new Vector3(Controller.transform.position.x, floorHeight + height, Controller.transform.position.z) + new Vector3(LastGamepadAxis.x, 0, LastGamepadAxis.y) * 5;
					}
				}
			}
			else
			{
				BodyLookAt.position = new Vector3(MainCamera.position.x, Controller.BodyObjects.Hips.position.y, MainCamera.position.z);

				BodyLookAt.RotateAround(Controller.BodyObjects.Head.position, Vector3.up, 180);

				bodyLookAtPosition = _position;
			}
		}

		void FPCameraUpdate()
		{
			mouseY = Controller.CameraParameters.alwaysTPAimMode ? Controller.BodyLocalEulerAngles.x : 0;
			mouseX = MainCamera.eulerAngles.y;
			
			var targetOrientation = Quaternion.Euler(targetDirection);

			if (!Controller.isPause && !cameraPause && Controller.ActiveCharacter)
			{
				if (Application.isMobilePlatform || Controller.projectSettings.mobileDebug)
				{
					InputHelper.CheckTouchCamera(ref touchId, ref mouseDelta, Controller);
					mouseDelta *= 5;
				}
				else
				{
					if (Gamepad.current != null && (Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].x.ReadValue()) > 0.1f || 
					    Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].y.ReadValue()) > 0.1f))
					{
						GamePadAxis.x = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].x.ReadValue() / (AimCamera ? 4 : 2);
						
						if (Controller.projectSettings.invertAxes[2])
							GamePadAxis.x *= -1;
						
						GamePadAxis.y = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].y.ReadValue() / (AimCamera ? 4 : 2);
						
						if (Controller.projectSettings.invertAxes[3])
							GamePadAxis.y *= -1;

						mouseDelta = new Vector2(GamePadAxis.x * 2, GamePadAxis.y * 2);
					}
					else
					{
						if(Mouse.current != null)
							mouseDelta = new Vector2(Mouse.current.delta.x.ReadValue() / 10, Mouse.current.delta.y.ReadValue() / 10);
					}
				}
				
				mouseDelta = Vector2.Scale(mouseDelta, new Vector2(currentSensitivityX / 10 * Controller.CameraParameters.fpXSmooth, currentSensitivityY / 10 * Controller.CameraParameters.fpYSmooth));
				
				if (Controller.projectSettings.invertAxes[2])
					mouseDelta.x *= -1;
					
				if (Controller.projectSettings.invertAxes[3])
					mouseDelta.y *= -1;
				
				_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1 / Controller.CameraParameters.fpXSmooth);
				_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1 / Controller.CameraParameters.fpYSmooth);
				_mouseAbsolute += _smoothMouse;
			}

			if (_mouseAbsolute.y > maxMouseAbsolute)
			{
				_mouseAbsolute.y = maxMouseAbsolute;
			}
			else if (_mouseAbsolute.y < minMouseAbsolute)
			{
				_mouseAbsolute.y = minMouseAbsolute;
			}

			var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);

			desiredRotation = yRotation * Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

			if (setCameraType)
			{
				MainCamera.rotation = desiredRotation;
				body.rotation = MainCamera.rotation;

				Controller.TopBodyOffset();
				Controller.CharacterRotation();

				if (!Controller.AdjustmentScene || Controller.inventoryManager.WeaponController)
				{
					if (Controller.gameObject.activeSelf && bobbingTime > currentCurve.keys[currentCurve.length - 2].time)
					{
						if (Controller.anim.GetFloat("Horizontal") > 0.3f || Controller.anim.GetFloat("Horizontal") < -0.3f ||
						    Controller.anim.GetFloat("Vertical") > 0.3f || Controller.anim.GetFloat("Vertical") < -0.3f)
						{
							if (Controller.isCrouch)
							{
								newCurve = fpCrouchCurve;
							}
							else if (Controller.isSprint)
							{
								newCurve = fpRunCurve;
							}
							else
							{
								newCurve = fpWalkCurve;
							}
						}
						else
						{
							newCurve = fpIdleCurve;
						}
					}


					if (newCurve.Equals(currentCurve) && canUseNewBobbingCurve)
					{
						if (bobbingTime > currentCurve.keys[currentCurve.length - 1].time)
						{
							bobbingTime = currentCurve.keys[0].time;
						}

						bobbingValue = currentCurve.Evaluate(bobbingTime);

						if (isCameraAimEnabled)
							bobbingValue /= 1.5f;

						transform.eulerAngles = new Vector3(CameraPosition.eulerAngles.x, CameraPosition.eulerAngles.y, 0) - new Vector3(currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.X ? bobbingValue : 0, currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.Y ? bobbingValue : 0, currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.Z ? bobbingValue : 0);
					}
					else
					{
						var targetValue = newCurve.Evaluate(newCurve.keys[newCurve.length / 2].time);
						bobbingValue = Mathf.Lerp(bobbingValue, targetValue, 10 * Time.deltaTime);

						var tempValue = bobbingValue;

						if (isCameraAimEnabled)
							tempValue /= 1.5f;

						transform.eulerAngles = new Vector3(CameraPosition.eulerAngles.x, CameraPosition.eulerAngles.y, 0) - new Vector3(currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.X ? tempValue : 0, currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.Y ? tempValue : 0, currentBobbingValues.bobbingRotationAxis == Helper.RotationAxes.Z ? tempValue : 0);

						canUseNewBobbingCurve = false;

						if (Math.Abs(bobbingValue - targetValue) < 0.01f)
						{
							canUseNewBobbingCurve = true;
							currentCurve = newCurve;
							bobbingTime = currentCurve.keys[newCurve.length / 2].time;
						}
					}
				}

				// }

				MainCamera.position = CameraPosition.position;
			}
			else
			{
				if (isDeepAimEnabled)
				{
					_mouseAbsolute.x = -3.5f;
					_mouseAbsolute.y = -6;
				}
				else
				{
					_mouseAbsolute.y = 0;
					_mouseAbsolute.x = 0;
				}

				if (Controller.isCrouch)
				{
					_mouseAbsolute.x = -17;
//					_mouseAbsolute.y = 55;
				}
				
				body.rotation = desiredRotation;

				Controller.TopBodyOffset();
				Controller.CharacterRotation();
				
				var targetRotation = Quaternion.Euler(CameraPosition.eulerAngles.x, CameraPosition.eulerAngles.y, 0) * desiredRotation;
				
				MoveCameraToTargetPosition(CameraPosition.position, targetRotation, true);
			}
		}

		void MoveCameraToTargetPosition(Vector3 position, Quaternion rotation, bool includeRotation)
		{
			var transformPosition = transform.position;

			var multiplier = 1f;

			if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.emulateTDModeLikeTP || Controller.PreviousTypeOfCamera == CharacterHelper.CameraType.TopDown)
			{
				multiplier = 2f;
			}
			else
			{
				if (Controller.anim.GetBool("Move"))
				{
					if (Controller.isSprint || Controller.isJump) multiplier = 4;
					else multiplier = 2;
				}
			}

			transformPosition = Vector3.MoveTowards(transformPosition, position, Controller.CameraParameters.switchCameraSpeed * multiplier * Time.deltaTime);//Helper.MoveObjInNewPosition(transform.localPosition, Vector3.zero, 5 * Time.deltaTime);

			if (includeRotation)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Controller.CameraParameters.switchCameraSpeed * Time.deltaTime);
			}
			
			if(Helper.ReachedPositionAndRotation(transformPosition, position, 0.1f))
			{
				transformPosition = position;
				transform.position = transformPosition;
				
				setCameraType = true;

				if (includeRotation)
				{
					transform.parent = null;

					MainCamera.position = position; 
					MainCamera.rotation = rotation;
				}

				transform.parent = MainCamera;

				if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				{
					Helper.CameraExtensions.LayerCullingHide(Camera, "Head");
					Helper.CameraExtensions.LayerCullingHide(AimCamera, "Head");
					Helper.CameraExtensions.LayerCullingHide(layerCamera, "Head");
				}
			}
			else
			{
				transform.position = transformPosition;
			}
		}

		void GetMouseAxis()
		{
			if (Controller.isPause || cameraPause || !Controller.ActiveCharacter) return;

			if (!Application.isMobilePlatform && !Controller.projectSettings.mobileDebug)
			{
				if (Gamepad.current != null && (Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].x.ReadValue()) > 0.1f || Mathf.Abs(Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].y.ReadValue()) > 0.1f))
				{
					GamePadAxis.x = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].x.ReadValue() * currentSensitivityX;
					if (Controller.projectSettings.invertAxes[2])
						GamePadAxis.x *= -1;

					GamePadAxis.y = Controller.projectSettings.gamepadAxisControlsInUnityInputSystem[1].y.ReadValue() * currentSensitivityY;
					
					if (Controller.projectSettings.invertAxes[3])
						GamePadAxis.y *= -1;

					mouseX += GamePadAxis.x / 2;

					if (Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
						mouseY -= GamePadAxis.y / 2;
				}
				else
				{
					
					var delta = Mouse.current.delta.ReadValue();

					if (Controller.projectSettings.invertAxes[2])
						delta.x *= -1;
					
					if (Controller.projectSettings.invertAxes[3])
						delta.y *= -1;
					
					mouseX += delta.x * currentSensitivityX / 20;

					if (Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
						mouseY -= delta.y * currentSensitivityY / 20;
				}

				if(Controller.UIManager.cameraStick)
					Controller.UIManager.cameraStick.gameObject.SetActive(false);
				
				if(Controller.UIManager.cameraStickOutline)
					Controller.UIManager.cameraStickOutline.gameObject.SetActive(false);
			}
			else
			{
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && !Controller.CameraParameters.lockCamera || Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
				{
					InputHelper.CheckTouchCamera(ref touchId, ref TPmouseDelta, Controller);
					TPmouseDelta *= 2;
					TPmouseDelta = Vector2.Scale(TPmouseDelta, new Vector2(currentSensitivityX, currentSensitivityY));
				}
				else if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && Controller.CameraParameters.lockCamera)
				{
					useCameraJoystick = false;
					InputHelper.CheckMobileJoystick(Controller.UIManager.cameraStick, Controller.UIManager.cameraStickOutline, ref touchId, Controller.UIManager, ref MobileTouchjPointA, ref MobileTouchjPointB, ref mobileCameraStickDirection, ref useCameraJoystick, Controller);
					GamePadAxis = mobileCameraStickDirection;
				}
			}

			var vector = new Vector2(mouseX, mouseY);

			vector.x += TPmouseDelta.x;
			vector.y -= TPmouseDelta.y;
			
			mouseX = vector.x;
			
			if(Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
				mouseY = vector.y;

			mouseY = Helper.ClampAngle(mouseY, Controller.CameraParameters.tpXLimitMin, Controller.CameraParameters.tpXLimitMax);
		}

		public void DeepAim()
		{
			// if (deepAimDelayCoroutine != null)
			// {
			// 	StopCoroutine(deepAimDelayCoroutine);
			// 	isDeepAimEnabled = !isDeepAimEnabled;
			// }
			
			StartCoroutine(DeepAimDelay());
		}

		public void Aim()
		{
			enableAimTextureTimeout = 0;
			
			if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !isCameraAimEnabled)
			{
				isCameraAimEnabled = true;
				Controller.anim.SetBool("Aim", true);
				// normDepth = Camera.fieldOfView;
				
				Reset();
			}
			else if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && isCameraAimEnabled)
			{
				Controller.anim.SetBool("Aim", false);
				isCameraAimEnabled = false;
				Reset();
			}
			else if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson && !isCameraAimEnabled)
			{
				isCameraAimEnabled = true;
				Controller.anim.SetBool("Aim", true);
				// normDepth = Camera.fieldOfView;
				Reset();
			}
			else if (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson && isCameraAimEnabled)
			{
				Controller.anim.SetBool("Aim", false);
				isCameraAimEnabled = false;

				if (isDeepAimEnabled)
				{
					DeepAim();
				}

				Reset();
			}
		}

		// [SuppressMessage("ReSharper", "ReplaceWithSingleAssignment.True")]
		void ControlCrosshair()
		{
			if(!Controller.UIManager) return;
			
			var characterUI = Controller.UIManager.CharacterUI;

			if (Controller.ActiveCharacter && !Controller.AdjustmentScene)
			{
				var cursorState = Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && Controller.CameraParameters.lockCamera && !useGamepad && !Application.isMobilePlatform && !Controller.projectSettings.mobileDebug ||
				                  (Controller.isPause || cameraPause) && canUseCursorInPause && !useGamepad && !Controller.UIManager.useGamepad || Controller.projectSettings.mobileDebug && (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && !Controller.CameraParameters.lockCamera || Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
				                  || (Controller.inventoryManager.inventoryIsOpened || Controller.health <= 0) && !Controller.UIManager.useGamepad;//||// Controller.projectSettings.mobileDebug;
				
				Cursor.visible = cursorState;
				
				if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && Controller.CameraParameters.lockCamera)
				{
					if (!Controller.isPause && !cameraPause && !Controller.UIManager.CharacterUI.Inventory.MainObject.activeSelf && Controller.health > 0)
					{
						if (Controller.CameraParameters.CursorImage && !cursorImageHasChanged)
						{
							Cursor.SetCursor(Controller.CameraParameters.CursorImage.texture, Vector2.zero, CursorMode.ForceSoftware);
							cursorImageHasChanged = true;
						}
					}
					else
					{
						if (cursorImageHasChanged)
						{
							Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
							cursorImageHasChanged = false;
						}
					}
				}
				else
				{
					if (cursorImageHasChanged)
					{
						Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
						cursorImageHasChanged = false;
					}
				}
				//


				// crosshair animation and adjustment
				if (characterUI.crosshairMainObject)
				{
					//if game is active
					if (!Controller.isPause && !cameraPause && !Controller.UIManager.CharacterUI.Inventory.MainObject.activeSelf && !Controller.isRemoteCharacter)
					{
						var gun = Controller.inventoryManager.WeaponController;

						//conditions under which the crosshair will be visible
						var crosshairSetActiveState = !Controller.inCar && !Controller.inventoryManager.enablePickUpTooltip && Controller.health > 0 &&
						                     (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && !Controller.CameraParameters.lockCamera
						                      || Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && Controller.CameraParameters.lockCamera &&  (useGamepad || Application.isMobilePlatform || Controller.projectSettings.mobileDebug)
						                      || Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown) &&
						                     (Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson && !isCameraAimEnabled || Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson) &&
						                     (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && (isCameraAimEnabled || Controller.isAlwaysTpAimEnabled) && !cameraOcclusion ||
						                      Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson) &&
						                     Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].weaponSlotInGame.Count > 0 &&
						                     !Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].weaponSlotInGame[Controller.inventoryManager.slots[Controller.inventoryManager.currentSlot].currentWeaponInSlot].fistAttack;


						//weapons related conditions
						var crosshairAdditionalState = !gun || gun && !gun.DetectObject && (Controller.TypeOfCamera != CharacterHelper.CameraType.TopDown && !gun.isReloadEnabled || Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown) &&
						                               (gun.Attacks[gun.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade && !gun.isAimEnabled || gun.Attacks[gun.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade) &&
						                               (gun.Attacks[gun.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher && !gun.isAimEnabled || gun.Attacks[gun.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.GrenadeLauncher);


						var multiplayerAdditionalState = true;
						
#if USK_ADVANCED_MULTIPLAYER
						if (Controller.CharacterSync && Controller.CharacterSync.advancedRoomManager && Controller.CharacterSync.advancedRoomManager.matchIsOver)
							multiplayerAdditionalState = false;
#endif
						characterUI.crosshairMainObject.gameObject.SetActive(crosshairSetActiveState && crosshairAdditionalState && multiplayerAdditionalState);

						if (Controller.anim.GetBool("Attack"))
						{
							crosshairMultiplier = 1.5f;
						}
						else if (Controller.anim.GetFloat("Horizontal") > 0.3f || Controller.anim.GetFloat("Horizontal") < -0.3f ||
						         Controller.anim.GetFloat("Vertical") > 0.3f || Controller.anim.GetFloat("Vertical") < -0.3f)
						{
							crosshairMultiplier = 2;
						}
						else
						{
							crosshairMultiplier = 1;
						}


						// crosshair parts adjustment

						if (Controller.inventoryManager.WeaponController)
						{
							UIHelper.CalculateCrosshairPartsPositions(Controller, crosshairMultiplier, ref currentCrosshairPositions);

							characterUI.topCrosshairPart.rectTransform.anchoredPosition = currentCrosshairPositions[1];
							characterUI.bottomCrosshairPart.rectTransform.anchoredPosition = currentCrosshairPositions[2];
							characterUI.rightCrosshairPart.rectTransform.anchoredPosition = currentCrosshairPositions[3];
							characterUI.leftCrosshairPart.rectTransform.anchoredPosition = currentCrosshairPositions[4];

							var direction = Controller.thisCamera.transform.TransformDirection(Vector3.forward);
							var enemyCondition = false;
							var opponentCondition = false;

							if (Physics.Raycast(Controller.thisCamera.transform.position, direction, out var hit, 100, Helper.LayerMask()))
							{
								var root = hit.collider.transform.root;

								var enemyController = root.gameObject.GetComponent<AIController>();
								var controller = root.gameObject.GetComponent<Controller>();

								var dist = Controller.inventoryManager.WeaponController.Attacks[Controller.inventoryManager.WeaponController.currentAttack].attackDistance;
								enemyCondition = hit.distance <= dist
								                 &&
								                 enemyController && (enemyController.multiplayerTeam != Controller.multiplayerTeam || enemyController.multiplayerTeam == MultiplayerHelper.Teams.Null) && enemyController.health > 0
								                 ||
								                 controller && (controller.multiplayerTeam != Controller.multiplayerTeam || controller.multiplayerTeam == MultiplayerHelper.Teams.Null) && controller.health > 0;

								opponentCondition = hit.distance <= dist && (controller && controller.health > 0 && controller.multiplayerTeam == Controller.multiplayerTeam
								                                             ||
								                                             enemyController && enemyController.health > 0 && enemyController.multiplayerTeam == Controller.multiplayerTeam);
							}

							if (characterUI.topCrosshairPart)
								characterUI.topCrosshairPart.color = enemyCondition ? Color.red : opponentCondition ? Color.green : Color.white;

							if (characterUI.rightCrosshairPart)
								characterUI.rightCrosshairPart.color = enemyCondition ? Color.red : opponentCondition ? Color.green : Color.white;

							if (characterUI.leftCrosshairPart)
								characterUI.leftCrosshairPart.color = enemyCondition ? Color.red : opponentCondition ? Color.green : Color.white;

							if (characterUI.bottomCrosshairPart)
								characterUI.bottomCrosshairPart.color = enemyCondition ? Color.red : opponentCondition ? Color.green : Color.white;

							if (characterUI.middleCrosshairPart)
								characterUI.middleCrosshairPart.color = enemyCondition ? Color.red : opponentCondition ? Color.green : Color.white;

							if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
							{
								characterUI.middleCrosshairPart.rectTransform.gameObject.SetActive(true);

								if (characterUI.middleCrosshairPart.gameObject.GetComponent<Outline>())
									characterUI.middleCrosshairPart.gameObject.GetComponent<Outline>().enabled = false;

								characterUI.DisableAllCrosshairParts();
							}
							else
							{
								characterUI.ManageCrosshairParts(Controller.inventoryManager.WeaponController.Attacks[Controller.inventoryManager.WeaponController.currentAttack].sightType);
							}
						}
						else
						{
							characterUI.middleCrosshairPart.gameObject.SetActive(false);
							characterUI.DisableAllCrosshairParts();
						}
						//


						//adjusting the crosshair position on the screen
						if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && Controller.emulateTDModeLikeTP)
						{
							if (!Controller.CameraParameters.lockCamera)
							{
								if (Cursor.lockState != CursorLockMode.Locked && !Controller.projectSettings.mobileDebug)
								{
									Cursor.lockState = CursorLockMode.Locked;
								}

								if (characterUI.crosshairMainObject.anchoredPosition != new Vector2(CrosshairOffsetX, CrosshairOffsetY))
								{
									characterUI.crosshairMainObject.anchorMin = new Vector2(0.5f, 0.5f);
									characterUI.crosshairMainObject.anchorMax = new Vector2(0.5f, 0.5f);

									characterUI.crosshairMainObject.anchoredPosition = new Vector2(CrosshairOffsetX, CrosshairOffsetY);
									characterUI.crosshairMainObject.gameObject.SetActive(false);
									characterUI.crosshairMainObject.gameObject.SetActive(true);
								}
							}
							else
							{
								if (Cursor.lockState != CursorLockMode.Confined && !Controller.projectSettings.mobileDebug)
								{
									Cursor.lockState = CursorLockMode.Confined;
								}

								if (Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown && Controller.CameraParameters.lockCamera && (useGamepad || Application.isMobilePlatform || Controller.projectSettings.mobileDebug))
								{
									if (characterUI.crosshairMainObject.anchorMin != new Vector2(0.5f, 0.5f))
									{
										characterUI.crosshairMainObject.anchorMin = new Vector2(0.5f, 0.5f);
										characterUI.crosshairMainObject.anchorMax = new Vector2(0.5f, 0.5f);

										characterUI.crosshairMainObject.gameObject.SetActive(false);
										characterUI.crosshairMainObject.gameObject.SetActive(true);
									}

									var position = Camera.WorldToScreenPoint(BodyLookAt.position);
									characterUI.crosshairMainObject.anchoredPosition = new Vector2(position.x - Screen.width / 2, position.y - Screen.height / 2);
								}
							}
						}
						else
						{
							if (Cursor.lockState != CursorLockMode.Locked && !Controller.projectSettings.mobileDebug)
							{
								Cursor.lockState = CursorLockMode.Locked;
							}

							if (characterUI.crosshairMainObject.anchoredPosition != new Vector2(0, 0))
							{
								characterUI.crosshairMainObject.anchorMin = new Vector2(0.5f, 0.5f);
								characterUI.crosshairMainObject.anchorMax = new Vector2(0.5f, 0.5f);

								characterUI.crosshairMainObject.anchoredPosition = new Vector2(0, 0);

								characterUI.crosshairMainObject.gameObject.SetActive(false);
								characterUI.crosshairMainObject.gameObject.SetActive(true);
							}
						}
						//

						//pick-up icon adjustment
						if (!Application.isMobilePlatform && !Controller.projectSettings.mobileDebug)
						{
							if (Controller.UIManager.CharacterUI.PickupImage)
								Controller.UIManager.CharacterUI.PickupImage.gameObject.SetActive(Controller.inventoryManager.enablePickUpTooltip);
							
							// if (Controller.UIManager.CharacterUI.infoTooltip)
							// {
							// 	Controller.UIManager.CharacterUI.infoTooltip.gameObject.SetActive(Controller.inventoryManager.isPickUp);
							// 	Controller.UIManager.CharacterUI.infoTooltip.text = "Press ''" + (!Controller.CameraController.useGamepad ? Controller.projectSettings.keyboardButtonsInProjectSettings[8].ToString() : Controller.projectSettings.gamepadButtonsInProjectSettings[8].ToString()) + "'' button to pick up the item";
							// }
						}
						else
						{
							if (Controller.UIManager.uiButtons[11])
								Controller.UIManager.uiButtons[11].gameObject.SetActive(Controller.inventoryManager.enablePickUpTooltip);
						}
						//
					}
					// if game isn't active, disable crosshair and pick-up icons
					else
					{
						characterUI.crosshairMainObject.gameObject.SetActive(false);

						if (Controller.UIManager.CharacterUI.PickupImage)
							Controller.UIManager.CharacterUI.PickupImage.gameObject.SetActive(false);
						
						// if (Controller.UIManager.CharacterUI.infoTooltip)
						// 	Controller.UIManager.CharacterUI.infoTooltip.gameObject.SetActive(false);

						if (Controller.UIManager.uiButtons[11])
							Controller.UIManager.uiButtons[11].gameObject.SetActive(false);

						if (Controller.isPause || cameraPause || Controller.UIManager.CharacterUI.Inventory.MainObject.activeSelf || Controller.projectSettings.mobileDebug)
						{
							if (Cursor.lockState != CursorLockMode.None)
							{
								Cursor.lockState = CursorLockMode.None;
							}
						}
					}
				}

			}
		}

		void DisableAimTextures()
		{
			if (Controller.inventoryManager.WeaponController && Controller.UIManager.CharacterUI.aimPlaceholder.gameObject.activeSelf)
			{
				if (Controller.inventoryManager.WeaponController.useAimTexture && Controller.UIManager.CharacterUI.aimPlaceholder)
				{
					Controller.inventoryManager.WeaponController.canEnableAimTexture = false;

					var color = Controller.UIManager.CharacterUI.aimPlaceholder.color;

					color.a = Mathf.Lerp(color.a, 0, 10 * Time.deltaTime);

					Controller.UIManager.CharacterUI.aimPlaceholder.color = color;
					
					UIHelper.SetFillColor(Controller.UIManager.leftFillImageComponent, color.a);
					UIHelper.SetFillColor(Controller.UIManager.rightFillImageComponent, color.a);
					
					if (color.a < 0.5f)
					{
						Controller.UIManager.CharacterUI.ActivateAll(Controller.UIManager.useMinimap);
						Controller.UIManager.EnableAllBlips();
					}

					if (color.a <= 0.05f)
					{
						Controller.UIManager.CharacterUI.aimPlaceholder.gameObject.SetActive(false);
						Controller.UIManager.leftFillImageComponent.gameObject.SetActive(false);
						Controller.UIManager.rightFillImageComponent.gameObject.SetActive(false);
					}
				}
			}

			if (Controller.inventoryManager.WeaponController)
			{
				if (Controller.inventoryManager.WeaponController.useScope)
				{
					if (!AimCamera.gameObject.activeSelf)
						AimCamera.gameObject.SetActive(true);

					AimCamera.fieldOfView = Mathf.Lerp(AimCamera.fieldOfView, Controller.inventoryManager.WeaponController.scopeDepth, 0.5f);
					AimCamera.targetTexture = Controller.inventoryManager.ScopeScreenTexture;
				}
				else
				{
					AimCamera.targetTexture = null;
					if (AimCamera.gameObject.activeSelf)
						AimCamera.gameObject.SetActive(false);
				}
			}
		}

		private void SetSensitivity()
		{
			if (Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
			{
				if (isCameraAimEnabled || Controller.isAlwaysTpAimEnabled)
				{
					currentSensitivityX = Controller.CameraParameters.tpAimXMouseSensitivity;
					currentSensitivityY = Controller.CameraParameters.tpAimYMouseSensitivity;
				}
				else
				{
					currentSensitivityX = Controller.CameraParameters.tpXMouseSensitivity;
					currentSensitivityY = Controller.CameraParameters.tpYMouseSensitivity;
				}
			}
			else
			{
				if (isCameraAimEnabled)
				{
					if (Controller.inventoryManager.WeaponController && Controller.inventoryManager.WeaponController.useAimTexture)
					{
						currentSensitivityX = Controller.CameraParameters.fpAimXMouseSensitivity / 10;
						currentSensitivityY = Controller.CameraParameters.fpAimYMouseSensitivity / 10;
					}
					else
					{
						currentSensitivityX = Controller.CameraParameters.fpAimXMouseSensitivity;
						currentSensitivityY = Controller.CameraParameters.fpAimYMouseSensitivity;
					}
				}
				else
				{
					currentSensitivityX = Controller.CameraParameters.fpXMouseSensitivity;
					currentSensitivityY = Controller.CameraParameters.fpYMouseSensitivity;
				}
			}
		}

		public void ReloadParameters()
		{
			if (isCameraAimEnabled)
			{
				Aim();
			}

			if (Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
			{
				mouseX = Controller.transform.localEulerAngles.y;
				mouseY = Controller.transform.localEulerAngles.x;
			}
			else
			{
				mouseX = 0;
				mouseY = 0;
				Controller.BodyObjects.TopBody.localEulerAngles = Vector3.zero;
			}
			
			

			CharacterHelper.ResetCameraParameters(Controller.TypeOfCamera, Controller.TypeOfCamera, Controller);
		}

		public void Reset()
		{
			if(!Controller)
				return;
			
			switch (Controller.TypeOfCamera)
			{
				case CharacterHelper.CameraType.ThirdPerson:
				{
					SetSensitivity();

					Helper.CameraExtensions.LayerCullingShow(Camera, "Head");
					Helper.CameraExtensions.LayerCullingShow(AimCamera, "Head");
					Helper.CameraExtensions.LayerCullingShow(layerCamera, "Head");

					if (Controller.emulateTDModeLikeTP)
						break;

					currentDistance = CameraOffset.Distance;
					currentOffsetX = CameraOffset.tpCameraOffsetX;
					currentOffsetY = CameraOffset.tpCameraOffsetY;

					desiredDistance = currentDistance;
					desiredOffsetX = currentOffsetX;

					if (preOcclededCamera)
						preOcclededCamera.localPosition = new Vector3(CameraOffset.tpCameraOffsetX, CameraOffset.tpCameraOffsetY, CameraOffset.Distance);

					break;
				}

				case CharacterHelper.CameraType.TopDown:
				{
					Helper.CameraExtensions.LayerCullingShow(Camera, "Head");
					Helper.CameraExtensions.LayerCullingShow(AimCamera, "Head");
					Helper.CameraExtensions.LayerCullingShow(layerCamera, "Head");
					
					currentSensitivityX = Controller.CameraParameters.tdXMouseSensitivity;
					currentSensitivityY = Controller.CameraParameters.tdXMouseSensitivity;
					break;
				}

				case CharacterHelper.CameraType.FirstPerson:
				{
					SetSensitivity();
					break;
				}
			}
		}

		public void SetAnimVariables()
		{
			switch (Controller.TypeOfCamera)
			{
				case CharacterHelper.CameraType.ThirdPerson:
				case CharacterHelper.CameraType.TopDown:

					Controller.anim.SetBool(Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson ? "TPS" : "TDS", true);

					if (Controller.inventoryManager.hasAnyWeapon)
					{
						Controller.anim.SetLayerWeight(2, 1);
						Controller.anim.SetLayerWeight(1, 0);

						Controller.currentAnimatorLayer = 2;
					}

					break;

				case CharacterHelper.CameraType.FirstPerson:

					Controller.anim.SetBool("FPS", true);
					Controller.anim.SetLayerWeight(2, 0);
					Controller.anim.SetLayerWeight(3, 0);
					Controller.anim.SetLayerWeight(1, 1);
					Controller.currentAnimatorLayer = 1;

					break;
			}
		}

		void TPCheckIfCameraOccluded()
		{
			RaycastHit Hit;
			var nearestDistance = CharacterHelper.CheckCameraPoints(Controller.BodyObjects.Head.position, transform.position, disabledObjects, transform, Camera);

			if (nearestDistance > -1)
			{
				desiredDistance += 0.2f;

				if (desiredDistance > 4.5f)
					desiredDistance = 4.5f;

				if (Physics.Raycast(transform.position - transform.right * 2, transform.right, out Hit, 5))
				{
					desiredOffsetX -= 0.1f;

					if (desiredOffsetX < 0.3f)
						desiredOffsetX = 0.3f;
				}
			}
			else
			{
				var position = preOcclededCamera.position;
				
				nearestDistance = CharacterHelper.CheckCameraPoints(Controller.BodyObjects.Head.position, position, disabledObjects, transform, Camera);

				var canChangeDist = true;

				var layerMask = ~ (LayerMask.GetMask("Grass") | LayerMask.GetMask("Character") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Head") | LayerMask.GetMask("Noise Collider") | LayerMask.GetMask("Smoke"));

				var size = Physics.OverlapSphereNonAlloc(position, 0.5f, _occlusionCollisers, layerMask);
				
				if (size > 0)
				{
					canChangeDist = false;
					cameraOcclusion = true;
				}

				if (!Physics.Raycast(transform.position - transform.right * 3, transform.right, out Hit, 6) &&
				    !Physics.Raycast(transform.position + transform.right * 3, -transform.right, out Hit, 6) && canChangeDist)
				{
					desiredOffsetX += 0.1f;

					if (desiredOffsetX > CameraOffset.tpCameraOffsetX)
						desiredOffsetX = CameraOffset.tpCameraOffsetX;
				}

				if (nearestDistance <= -1 && canChangeDist)
				{
					desiredDistance += 0.01f;
					
					if (desiredDistance > CameraOffset.Distance)
						desiredDistance = CameraOffset.Distance;
				}
				
				if(Math.Abs(desiredOffsetX - CameraOffset.tpCameraOffsetX) < 0.2f && Math.Abs(desiredDistance - CameraOffset.Distance) < 0.2f)
					cameraOcclusion = false;
			}
		}
	}
}


