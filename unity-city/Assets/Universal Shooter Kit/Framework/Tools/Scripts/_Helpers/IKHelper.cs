using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
	public static class IKHelper {
		
		
		[Serializable]
		public class FeetIKVariables
		{
			public Quaternion RightFootIKRotation, LeftFootIKRotation;
			public Vector3 RightFootPosition, LeftFootPosition, LeftFootIKPosition, RightFootIKPosition;
			public float LastRightFootPosition;
			public float LastLeftFootPosition;
			public float LastPelvisPosition;
			public float feetToIKPositionSpeed = 0.5f;
			public float heightFromGroundRaycast = 1.15f;
			public float raycastDownDistance = 1.5f;
			public float pelvisOffset;
			
		}

		[Serializable]
		public class HandsPositions
		{
			public Vector2 startHandPosition;
			public Vector2 startHandRotation;
        
			public Vector2 desiredHandPosition;
			public Vector2 desiredHandRotation;
			
			public Vector2 currentHandPosition;
			public Vector2 currentHandRotation;
		}
		
		public enum IkDebugMode
		{
			Norm, Aim, ObjectDetection, Crouch
		}

		public static void FeetPositionSolver(Controller controller, string type)
		{
			switch (type)
			{
				case "right":
					FeetPositionSolver(controller.IKVariables.RightFootPosition,
						ref controller.IKVariables.RightFootIKPosition, ref controller.IKVariables.RightFootIKRotation,
						controller.IKVariables.raycastDownDistance, controller.IKVariables.heightFromGroundRaycast,
						controller.IKVariables.pelvisOffset, controller.transform);
					break;
				case "left":
					FeetPositionSolver(controller.IKVariables.LeftFootPosition,
						ref controller.IKVariables.LeftFootIKPosition, ref controller.IKVariables.LeftFootIKRotation,
						controller.IKVariables.raycastDownDistance, controller.IKVariables.heightFromGroundRaycast,
						controller.IKVariables.pelvisOffset, controller.transform);
					break;
			}
		}

		static void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations,
			float raycastDownDistance, float heightFromGroundRaycast, float pelvisOffset, Transform transform)
		{
			RaycastHit feetOutHit;

			if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, Helper.LayerMask()))
			{
				if (feetOutHit.transform.root.gameObject.GetComponent<Controller>() || feetOutHit.transform.root.gameObject.GetComponent<WeaponController>()
				    || feetOutHit.transform.root.gameObject.GetComponent<AIController>()) return;
				
				feetIkPositions = fromSkyPosition;
				feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
				feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

				return;
			}

			feetIkPositions = Vector3.zero;
		}

		public static void AdjustFeetTarget(Controller controller, string type)
		{
			switch (type)
			{
				case "right":
					AdjustFeetTarget(ref controller.IKVariables.RightFootPosition, HumanBodyBones.RightFoot,
						controller.anim, controller.transform, controller.IKVariables.heightFromGroundRaycast);
					break;
				case "left":
					AdjustFeetTarget(ref controller.IKVariables.LeftFootPosition, HumanBodyBones.LeftFoot,
						controller.anim, controller.transform, controller.IKVariables.heightFromGroundRaycast);
					break;
			}
		}

		static void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones footBone, Animator anim, Transform transform, float heightFromGroundRaycast)
		{
			if (anim.GetBoneTransform(footBone))
			{
				feetPositions = anim.GetBoneTransform(footBone).position;
				feetPositions.y = transform.position.y + heightFromGroundRaycast;
			}
		}

		public static void MovePelvisHeight(Vector3 RightFootIKPosition, Vector3 LeftFootIKPosition, float LastPelvisPosition, Animator anim,
			Transform transform)
		{
			if (RightFootIKPosition == Vector3.zero || LeftFootIKPosition == Vector3.zero || LastPelvisPosition == 0)
			{
				LastPelvisPosition = anim.bodyPosition.y;
				return;
			}

			float l_offsetPosition = LeftFootIKPosition.y - transform.position.y;
			float r_offsetPosition = RightFootIKPosition.y - transform.position.y;

			float totalOffset = (l_offsetPosition < r_offsetPosition) ? l_offsetPosition : r_offsetPosition;

			Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;

			newPelvisPosition.y =
				Mathf.Lerp(LastPelvisPosition, newPelvisPosition.y, 0.5f);

			anim.bodyPosition = newPelvisPosition;

		}

		public static void MoveFeetToIkPoint(Controller controller, string type)
		{
			switch (type)
			{
				case "right":
					MoveFeetToIkPoint(AvatarIKGoal.RightFoot, controller.IKVariables.RightFootIKPosition,
						controller.IKVariables.RightFootIKRotation, ref controller.IKVariables.LastRightFootPosition, controller.anim,
						controller.transform, controller.IKVariables.feetToIKPositionSpeed);
					break;
				case "left":
					MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, controller.IKVariables.LeftFootIKPosition,
						controller.IKVariables.LeftFootIKRotation, ref controller.IKVariables.LastLeftFootPosition, controller.anim,
						controller.transform, controller.IKVariables.feetToIKPositionSpeed);
					break;
			}
		}

		static void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 curPositionIk, Quaternion curRotationIk,
			ref float lastFootPosition, Animator anim, Transform transform, float feetToIKPositionSpeed)
		{
			Vector3 targetIkPosition = anim.GetIKPosition(foot);
			
			if (curPositionIk != Vector3.zero)
			{
				targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
				curPositionIk = transform.InverseTransformPoint(curPositionIk);

				var yPos = Mathf.Lerp(lastFootPosition, curPositionIk.y, feetToIKPositionSpeed);
				targetIkPosition.y += yPos;

				lastFootPosition = yPos;

				targetIkPosition = transform.TransformPoint(targetIkPosition);

				anim.SetIKRotation(foot, curRotationIk);
			}

			anim.SetIKPosition(foot, targetIkPosition);
		}

		public static void ManageHandsPositions(WeaponController weaponController)
		{
			if(weaponController.setHandsPositionsCrouch && weaponController.setHandsPositionsObjectDetection)
				SetAimPosition(weaponController);
			
			if(weaponController.setHandsPositionsCrouch && weaponController.setHandsPositionsAim)
				SetWallPosition(weaponController);
			
			if(weaponController.setHandsPositionsObjectDetection && weaponController.setHandsPositionsAim)
				SetCrouchPosition(weaponController);
		}

		static void SetAimPosition(WeaponController weaponController)
		{
			//var leftHand = weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade || weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee;

			var leftHand = weaponController.numberOfUsedHands == 1;
			var isTpMode = weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && (!weaponController.Controller.isCrouch && weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInNormalState ||
			                                                                                                       weaponController.Controller.isCrouch && weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInCrouchState);// || weaponController.Controller.isCrouch);
			var speed = weaponController.aimingSpeed;
			var desiredAngle = 60f;
			// if (weaponController.correctHandsPosition && !weaponController.Controller.isJump)
			// {
			// 	speed = 0.1f;
			// }

			if (!weaponController.setHandsPositionsAim && weaponController.CanUseIK && weaponController.isAimEnabled)
			{
				weaponController.firstTake = false;
				
				desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation));

				MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation,  ref weaponController.setHandsPositionsAim, leftHand, ref isTpMode, speed, "aim", ref weaponController.aimTimer, desiredAngle);
			}
			else if (!weaponController.setHandsPositionsAim && weaponController.CanUseIK && !weaponController.isAimEnabled && 
			         (weaponController.Controller.isCrouch && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInCrouchState || 
			          !weaponController.Controller.isCrouch && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInNormalState ||
			         weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson))
			{
				if (!weaponController.Controller.isCrouch || weaponController.Controller.isCrouch && weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
				{
					weaponController.firstTake = false;
					
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation));

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation,  ref weaponController.setHandsPositionsAim, leftHand, ref isTpMode, speed, "aim", ref weaponController.aimTimer, desiredAngle);
				}
				else if (weaponController.Controller.isCrouch && weaponController.Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation));

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation, ref weaponController.setHandsPositionsAim, leftHand, ref isTpMode, speed, "aim", ref weaponController.aimTimer, desiredAngle);
				}
			}
			
			if (weaponController.setHandsPositionsAim && !weaponController.aimTimeout)
			{
				weaponController.aimTimeout = true;
				weaponController.correctHandsPosition = false;

				weaponController.canUseSmoothWeaponRotation = false;

				if (!weaponController.Controller.isCrouch && weaponController.ActiveCrouchHands)
					weaponController.ActiveCrouchHands = false;
				else if(weaponController.Controller.isCrouch && !weaponController.ActiveCrouchHands)
					weaponController.ActiveCrouchHands = true;

				weaponController.Controller.anim.SetBool("CanWalkWithWeapon", true);

				weaponController.aimTimer = 0;

				if (weaponController.wasSetSwitchToFP)
				{
					weaponController.switchToFpCamera = true;
					weaponController.wasSetSwitchToFP = false;
				}
			}
		}

		private static void SetWallPosition(WeaponController weaponController)
		{
			var speed = 1.6f - (float) weaponController.weaponWeight / 10;
			var timer = 0f;
			var leftHand = weaponController.numberOfUsedHands == 1;
			var isTpMode = false;
			var desiredAngle = 60f;

			if (!weaponController.setHandsPositionsObjectDetection && weaponController.CanUseIK && weaponController.DetectObject)
			{
				desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandWallRotation)); 
				
				MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandWallPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandWallRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandWallPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandWallRotation, ref weaponController.setHandsPositionsObjectDetection, leftHand, ref isTpMode, speed, "wall", ref timer, desiredAngle);
			}
			else if (!weaponController.setHandsPositionsObjectDetection && weaponController.CanUseIK && !weaponController.DetectObject) 
			{
				if (!weaponController.isAimEnabled && !weaponController.Controller.isAlwaysTpAimEnabled && (weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !weaponController.Controller.isCrouch || weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson))
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation)); 
					
					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation, ref weaponController.setHandsPositionsObjectDetection, leftHand, ref isTpMode, speed, "wall", ref timer, desiredAngle);
				}
				else if (weaponController.isAimEnabled || weaponController.Controller.isAlwaysTpAimEnabled)
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation)); 

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation, ref weaponController.setHandsPositionsObjectDetection, leftHand , ref isTpMode,speed, "wall", ref timer, desiredAngle);
				}
				else if (weaponController.Controller.isCrouch && weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson)
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation)); 

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation, ref weaponController.setHandsPositionsObjectDetection, leftHand, ref isTpMode, speed, "wall", ref timer, desiredAngle);
				}
			}

			if (weaponController.setHandsPositionsObjectDetection && !weaponController.applyChanges)
			{
				weaponController.applyChanges = true;
				weaponController.Controller.anim.SetBool("CanWalkWithWeapon", true);
				weaponController.canUseSmoothWeaponRotation = false;
				weaponController.firstTake = false;
				
				if (!weaponController.Controller.isCrouch && weaponController.ActiveCrouchHands)
					weaponController.ActiveCrouchHands = false;
				else if(weaponController.Controller.isCrouch && !weaponController.ActiveCrouchHands)
					weaponController.ActiveCrouchHands = true;
			}
		}

		static void SetCrouchPosition(WeaponController weaponController)
		{
			var speed = 1.2f - 0.02f * weaponController.weaponWeight;
			var leftHand = weaponController.setCrouchHandsFirstly && weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInNormalState && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInCrouchState;
			var desiredAngle = 60f;

			if (!weaponController.setHandsPositionsCrouch && weaponController.CanUseCrouchIK && weaponController.ActiveCrouchHands)
			{
				desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation));
				
				MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftCrouchHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandPosition,
					weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightCrouchHandRotation,  ref weaponController.setHandsPositionsCrouch, leftHand, ref weaponController.setCrouchHandsFirstly, speed, "crouch", ref weaponController.crouchTimer, desiredAngle);
			}
			else if (!weaponController.setHandsPositionsCrouch && weaponController.CanUseCrouchIK && !weaponController.ActiveCrouchHands && (weaponController.Controller.isCrouch && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInCrouchState ||
			                                                                                                                                 !weaponController.Controller.isCrouch && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableIkInNormalState))
			{
				if (!weaponController.Controller.isAlwaysTpAimEnabled)
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation));

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftHandRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightHandRotation, ref weaponController.setHandsPositionsCrouch, leftHand, ref weaponController.setCrouchHandsFirstly, speed, "crouch", ref weaponController.crouchTimer, desiredAngle);
				}
				else
				{
					desiredAngle = Quaternion.Angle(weaponController.handRotationWhenTransitionStarts, Quaternion.Euler(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation));

					MovingIKObject(weaponController.IkObjects.RightObject, weaponController.IkObjects.LeftObject, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].LeftAimRotation, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimPosition,
						weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].RightAimRotation, ref weaponController.setHandsPositionsCrouch, leftHand, ref weaponController.setCrouchHandsFirstly, speed, "crouch", ref weaponController.crouchTimer,desiredAngle);
				}
			}

		}

		public static void ConvertIKPosition(Vector3 leftPosition, Vector3 rightPosition, WeaponController weaponController)
		{
			if (weaponController.IkObjects.RightObject)
				ConvertIKObjPosition(weaponController.IkObjects.RightObject, weaponController.Controller.BodyObjects.RightHand, weaponController.BodyObjects.TopBody, rightPosition, weaponController, "right");
			
			if (weaponController.numberOfUsedHands == 1 && weaponController.IkObjects.LeftObject)
				ConvertIKObjPosition(weaponController.IkObjects.LeftObject, weaponController.Controller.BodyObjects.LeftHand, weaponController.BodyObjects.TopBody, leftPosition, weaponController, "left");
			
			weaponController.handRotationWhenTransitionStarts = weaponController.IkObjects.RightObject.localRotation;
		}
		
		private static void ConvertIKObjPosition(Transform obj, Transform hand, Transform parent, Vector3 targetPosition, WeaponController weaponController, string side)
		{
			obj.parent = parent;
			obj.localPosition = targetPosition;
			obj.rotation = hand.rotation * Quaternion.Euler((side == "right" ? 1 : -1) * weaponController.Controller.CharacterOffset.handsDirectionObjRotation);
		}

		private static void MovingIKObject(Transform RightObj, Transform LeftObj, Vector3 LeftTargetPos, Vector3 LeftTargetRot, Vector3 RightTargetPos, Vector3 RightTargetRot, ref bool endTransition, bool leftHand, ref bool tpsMode, float speed, string type, ref float timer, float desiredRotationAngle)
		{
			var multiplier = type == "aim" ? 6 : 4;
			var rightObjLocalPosition = RightObj.localPosition;
			var rightObjLocalEulerAngles = RightObj.localEulerAngles;
			
			if (tpsMode && (type == "aim" || type == "crouch"))
			{
				timer += Time.deltaTime;
				
				RightObj.localPosition = RightTargetPos;
				RightObj.localEulerAngles = RightTargetRot;

				if (leftHand)
				{
					LeftObj.localPosition = LeftTargetPos;
					LeftObj.localEulerAngles = LeftTargetRot;
				}

				if (timer >= 1)
				{
					endTransition = true;

					if (type == "crouch")
					{
						tpsMode = false;
					}
				}
			}
			else
			{

				var remainingAngle = Quaternion.Angle(RightObj.localRotation, Quaternion.Euler(RightTargetRot));
				if (remainingAngle < 1) remainingAngle = 1;
				
				var speedMultiplier = desiredRotationAngle / remainingAngle;
				
				speed *= speedMultiplier / 2;

				rightObjLocalPosition = Vector3.MoveTowards(rightObjLocalPosition, RightTargetPos, speed * Time.deltaTime);
				RightObj.localRotation = Quaternion.Slerp(RightObj.localRotation, Quaternion.Euler(RightTargetRot), speed * multiplier * Time.deltaTime);
				
				rightObjLocalEulerAngles = RightObj.localEulerAngles;

				if (Helper.ReachedPositionAndRotationAccurate(ref rightObjLocalPosition, RightTargetPos, ref rightObjLocalEulerAngles, RightTargetRot, 0.01f, 0.01f))
				{
					endTransition = true;
				}
				
				RightObj.localPosition = rightObjLocalPosition;
				RightObj.localEulerAngles = rightObjLocalEulerAngles;

				if (leftHand)
				{
					LeftObj.localPosition = Vector3.MoveTowards(LeftObj.localPosition, LeftTargetPos, speed * Time.deltaTime);
					LeftObj.localRotation = Quaternion.Slerp(LeftObj.localRotation, Quaternion.Euler(LeftTargetRot), 10 * Time.deltaTime);
				}
			}
		}
		
		public static void PlaceAllIKObjects(WeaponController script, WeaponsHelper.WeaponInfo weaponInfo, bool placeAll, Transform dirObj)
		{
			//ResetIKValues(script);

			var BodyObjects = script.Controller.BodyObjects;

			if (!script.Controller.AdjustmentScene)
			{
				if (script.CanUseAimIK && (script.isAimEnabled || script.Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown))
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightObject, weaponInfo.RightAimPosition, weaponInfo.RightAimRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftObject, weaponInfo.LeftAimPosition, weaponInfo.LeftAimRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				else if (script.CanUseWallIK && script.DetectObject)
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightObject, weaponInfo.RightHandWallPosition, weaponInfo.RightHandWallRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftObject, weaponInfo.LeftHandWallPosition, weaponInfo.LeftHandWallRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				else if (script.CanUseCrouchIK && script.ActiveCrouchHands && script.Controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson)
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightObject, weaponInfo.RightCrouchHandPosition, weaponInfo.RightCrouchHandRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftObject, weaponInfo.LeftCrouchHandPosition, weaponInfo.LeftCrouchHandRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				else
				{
					if (script.isAimEnabled)
					{
						script.Controller.CameraController.Aim();
						script.currentBulletsScatter *= 2;
						script.isAimEnabled = false;
					}

					if (!script.Controller.isAlwaysTpAimEnabled)
					{
						PlaceIKObject(script.Controller, script.IkObjects.RightObject, weaponInfo.RightHandPosition, weaponInfo.RightHandRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
						PlaceIKObject(script.Controller, script.IkObjects.LeftObject, weaponInfo.LeftHandPosition, weaponInfo.LeftHandRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
					}
					else
					{
						PlaceIKObject(script.Controller, script.IkObjects.RightObject, weaponInfo.RightAimPosition, weaponInfo.RightAimRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
						PlaceIKObject(script.Controller, script.IkObjects.LeftObject, weaponInfo.LeftAimPosition, weaponInfo.LeftAimRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
					}
				}
			}
			else
			{
				if (script.DebugMode == IkDebugMode.Norm || placeAll)
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightObject, weaponInfo.RightHandPosition, weaponInfo.RightHandRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftObject, weaponInfo.LeftHandPosition, weaponInfo.LeftHandRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				
				if (script.DebugMode == IkDebugMode.ObjectDetection || placeAll)
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightWallObject, weaponInfo.RightHandWallPosition, weaponInfo.RightHandWallRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftWallObject, weaponInfo.LeftHandWallPosition, weaponInfo.LeftHandWallRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				
				if (script.DebugMode == IkDebugMode.Aim || placeAll)
				{
					// if (!placeAll)
					// {
						// script.Controller.anim.SetFloat("AimAnimationSpeed", 0);
						// script.Controller.anim.Play("Idle", 1, 0);
					// }
					
					PlaceIKObject(script.Controller,script.IkObjects.RightAimObject, weaponInfo.RightAimPosition, weaponInfo.RightAimRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftAimObject, weaponInfo.LeftAimPosition, weaponInfo.LeftAimRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
				
				if (script.DebugMode == IkDebugMode.Crouch || placeAll)
				{
					PlaceIKObject(script.Controller,script.IkObjects.RightCrouchObject, weaponInfo.RightCrouchHandPosition, weaponInfo.RightCrouchHandRotation, BodyObjects.TopBody, BodyObjects.RightHand, "right");
					PlaceIKObject(script.Controller,script.IkObjects.LeftCrouchObject, weaponInfo.LeftCrouchHandPosition, weaponInfo.LeftCrouchHandRotation, BodyObjects.TopBody, BodyObjects.LeftHand, "left");
				}
			}
			
			PlaceIKObject(script.IkObjects.RightElbowObject, weaponInfo.RightElbowPosition, script.Controller.DirectionObject.position + script.Controller.DirectionObject.right * 2, BodyObjects.TopBody);
			PlaceIKObject(script.IkObjects.LeftElbowObject,  weaponInfo.LeftElbowPosition, script.Controller.DirectionObject.position - script.Controller.DirectionObject.right * 2, BodyObjects.TopBody);
		}
		
		public static void PlaceIKObject(Controller controller, Transform obj, Vector3 pos,  Vector3 rot, Transform parent, Transform parent2, string side)
		{
			if (pos == Vector3.zero && rot == Vector3.zero)
			{
				obj.parent = parent;
				obj.position = parent2.position;
				obj.rotation = parent2.rotation * Quaternion.Euler((side == "right" ? 1 : -1) * controller.CharacterOffset.handsDirectionObjRotation);
			}
			else
			{
				obj.parent = parent;
				obj.localPosition = pos;
				obj.localEulerAngles = rot;
			}
		}
		
		static void PlaceIKObject(Transform obj, Vector3 pos, Vector3 dir, Transform parent)
		{
			if (pos == Vector3.zero)
			{
				obj.parent = parent;
				obj.localPosition = dir * 2;
			}
			else
			{
				obj.parent = parent;
				obj.localPosition = pos;
			}
		}
		
		public static void CheckIK(ref bool CanUseElbowIK, ref bool CanUseIK, ref bool CanUseAimIK, ref bool CanUseWallIK, ref bool CanUseCrouchIK, WeaponsHelper.WeaponInfo weaponInfo)
		{
			CanUseElbowIK = CheckIKPosition(weaponInfo.LeftElbowPosition, weaponInfo.RightElbowPosition);
			
			CanUseIK = CheckIKPosition(weaponInfo.RightHandPosition, weaponInfo.LeftHandPosition, 
				weaponInfo.RightHandRotation, weaponInfo.LeftHandRotation);

			CanUseCrouchIK = CheckIKPosition(weaponInfo.RightCrouchHandPosition, weaponInfo.LeftCrouchHandPosition, weaponInfo.RightCrouchHandRotation,
				weaponInfo.LeftCrouchHandRotation);

			CanUseAimIK = CheckIKPosition(weaponInfo.RightAimPosition, weaponInfo.LeftAimPosition, 
				weaponInfo.RightAimRotation, weaponInfo.LeftAimRotation);
                
			CanUseWallIK = CheckIKPosition(weaponInfo.RightHandWallPosition, weaponInfo.LeftHandWallPosition, 
				weaponInfo.RightHandWallRotation, weaponInfo.LeftHandWallRotation);
		}

		private static bool CheckIKPosition(Vector3 position_1, Vector3 position_2, Vector3 rotation_1, Vector3 rotation_2)
		{
			return position_1 != Vector3.zero || rotation_1 != Vector3.zero || position_2 != Vector3.zero ||
			       rotation_2 != Vector3.zero;
		}

		private static bool CheckIKPosition(Vector3 position_1, Vector3 position_2)
		{
			return position_1 != Vector3.zero || position_2 != Vector3.zero;
		}
	}
}

