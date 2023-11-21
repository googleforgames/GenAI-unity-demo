using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GercStudio.USK.Scripts
{

	public static class RagdollHelper
	{

#if UNITY_EDITOR
		[Serializable]
		public class BoneInfo
		{
			public string name;

			public Transform anchor;
			public CharacterJoint joint;
			public BoneInfo parent;

			public float minLimit;
			public float maxLimit;
			public float swingLimit;

			public Vector3 axis;
			public Vector3 normalAxis;

			public float radiusScale;
			public Type colliderType;

			public ArrayList children = new ArrayList();
			public float density;
			public float summedMass;
		}

		[Serializable]
		public class BodyParameters
		{
			public Transform pelvis;

			public Transform leftHips = null;
			public Transform leftKnee = null;
			public Transform leftFoot = null;

			public Transform rightHips = null;
			public Transform rightKnee = null;
			public Transform rightFoot = null;

			public Transform leftArm = null;
			public Transform leftElbow = null;

			public Transform rightArm = null;
			public Transform rightElbow = null;

			public Transform middleSpine = null;
			public Transform head = null;

			public Vector3 right = Vector3.right;
			public Vector3 up = Vector3.up;
			public Vector3 forward = Vector3.forward;

			public Vector3 worldUp = Vector3.up;

			public ArrayList bones;
			public BoneInfo rootBone;
		}

		public static void CreateColliders(BodyParameters bodyParameters)
		{
			PrepareBones(ref bodyParameters.rootBone, ref bodyParameters.bones, bodyParameters.pelvis, bodyParameters.middleSpine, bodyParameters.leftHips, bodyParameters.rightHips, bodyParameters.leftKnee, bodyParameters.rightKnee, bodyParameters.leftArm, bodyParameters.rightArm, bodyParameters.leftElbow, bodyParameters.rightElbow, bodyParameters.head, bodyParameters.up, bodyParameters.right, bodyParameters.forward, ref bodyParameters.worldUp);
			CalculateAxes(bodyParameters.head, bodyParameters.pelvis, bodyParameters.rightElbow, ref bodyParameters.up, ref bodyParameters.right, ref bodyParameters.forward);

			Cleanup(ref bodyParameters.bones);

			BuildCapsules(ref bodyParameters.bones);

			if (bodyParameters.middleSpine && bodyParameters.pelvis)
				AddBreastColliders(bodyParameters.pelvis, bodyParameters.middleSpine, bodyParameters.leftHips, bodyParameters.rightHips, bodyParameters.leftArm, bodyParameters.rightArm, bodyParameters.worldUp);
			else AddBreastColliders(bodyParameters.pelvis, bodyParameters.leftHips, bodyParameters.rightHips, bodyParameters.leftArm, bodyParameters.rightArm);

			AddHeadCollider(bodyParameters.head, bodyParameters.pelvis, bodyParameters.leftArm, bodyParameters.rightArm);

			BuildBodies(ref bodyParameters.bones);
			BuildJoints(ref bodyParameters.bones);
			CalculateMass(ref bodyParameters.bones, bodyParameters.rootBone);
		}

		public static void GetBones(BodyParameters bodyParameters, Animator animator)
		{
			if (animator.GetBoneTransform(HumanBodyBones.Hips))
				bodyParameters.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);

			if (animator.GetBoneTransform(HumanBodyBones.Spine))
				bodyParameters.middleSpine = animator.GetBoneTransform(HumanBodyBones.Spine);

			if (animator.GetBoneTransform(HumanBodyBones.Head))
				bodyParameters.head = animator.GetBoneTransform(HumanBodyBones.Head);

			if (animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg))
				bodyParameters.leftHips = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);

			if (animator.GetBoneTransform(HumanBodyBones.RightUpperLeg))
				bodyParameters.rightHips = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);

			if (animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))
				bodyParameters.leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

			if (animator.GetBoneTransform(HumanBodyBones.RightLowerLeg))
				bodyParameters.rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

			if (animator.GetBoneTransform(HumanBodyBones.LeftFoot))
				bodyParameters.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

			if (animator.GetBoneTransform(HumanBodyBones.RightFoot))
				bodyParameters.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

			if (animator.GetBoneTransform(HumanBodyBones.LeftUpperArm))
				bodyParameters.leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

			if (animator.GetBoneTransform(HumanBodyBones.RightUpperArm))
				bodyParameters.rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

			if (animator.GetBoneTransform(HumanBodyBones.LeftLowerArm))
				bodyParameters.leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

			if (animator.GetBoneTransform(HumanBodyBones.RightLowerArm))
				bodyParameters.rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

		}

		public static void SetBodyParts(BodyParameters bodyParameters, Controller controller)
		{
			SetBodyParts(bodyParameters, controller.BodyParts);
		}

		public static void SetBodyParts(BodyParameters bodyParameters, AIController aiController)
		{
			SetBodyParts(bodyParameters, aiController.BodyParts);
		}

		static void SetBodyParts(BodyParameters bodyParameters, List<Transform> BodyParts)
		{
			if (bodyParameters.pelvis)
				BodyParts[0] = bodyParameters.pelvis;

			if (bodyParameters.middleSpine)
				BodyParts[1] = bodyParameters.middleSpine;

			if (bodyParameters.head)
				BodyParts[2] = bodyParameters.head;

			if (bodyParameters.leftHips)
				BodyParts[3] = bodyParameters.leftHips;

			if (bodyParameters.leftKnee)
				BodyParts[4] = bodyParameters.leftKnee;

			if (bodyParameters.rightHips)
				BodyParts[5] = bodyParameters.rightHips;

			if (bodyParameters.rightKnee)
				BodyParts[6] = bodyParameters.rightKnee;

			if (bodyParameters.leftArm)
				BodyParts[7] = bodyParameters.leftArm;

			if (bodyParameters.leftElbow)
				BodyParts[8] = bodyParameters.leftElbow;

			if (bodyParameters.rightArm)
				BodyParts[9] = bodyParameters.rightArm;

			if (bodyParameters.rightElbow)
				BodyParts[10] = bodyParameters.rightElbow;
		}

		static BoneInfo FindBone(ArrayList bones, string name)
		{
			foreach (BoneInfo bone in bones)
			{
				if (bone.name == name)
					return bone;
			}

			return null;
		}

		public static void BuildCapsules(ref ArrayList bones)
		{
			foreach (BoneInfo bone in bones)
			{
				if (bone.colliderType != typeof(CapsuleCollider))
					continue;

				int direction;
				float distance;
				if (bone.children.Count == 1)
				{
					BoneInfo childBone = (BoneInfo) bone.children[0];
					Vector3 endPoint = childBone.anchor.position;
					CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);
				}
				else
				{
					Vector3 endPoint = (bone.anchor.position - bone.parent.anchor.position) + bone.anchor.position;
					CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);

					if (bone.anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
					{
						Bounds bounds = new Bounds();
						foreach (Transform child in bone.anchor.GetComponentsInChildren(typeof(Transform)))
						{
							bounds.Encapsulate(bone.anchor.InverseTransformPoint(child.position));
						}

						if (distance > 0)
							distance = bounds.max[direction];
						else
							distance = bounds.min[direction];
					}
				}

				CapsuleCollider collider = Undo.AddComponent<CapsuleCollider>(bone.anchor.gameObject);
				collider.direction = direction;

				Vector3 center = Vector3.zero;
				center[direction] = distance * 0.5F;
				collider.center = center;
				collider.height = Mathf.Abs(distance);
				collider.radius = Mathf.Abs(distance * bone.radiusScale);
			}
		}

		public static void AddBreastColliders(Transform pelvis, Transform middleSpine, Transform leftHips, Transform rightHips, Transform leftArm, Transform rightArm, Vector3 worldUp)
		{
			Bounds bounds;
			BoxCollider box;

			// Middle spine bounds
			bounds = Clip(GetBreastBounds(pelvis, leftHips, rightHips, leftArm, rightArm), pelvis, middleSpine, false, worldUp);
			box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
			box.center = bounds.center;
			box.size = bounds.size;

			bounds = Clip(GetBreastBounds(middleSpine, leftHips, rightHips, leftArm, rightArm), middleSpine, middleSpine, true, worldUp);
			box = Undo.AddComponent<BoxCollider>(middleSpine.gameObject);
			box.center = bounds.center;
			box.size = bounds.size;
		}

		static Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below, Vector3 worldUp)
		{
			int axis = LargestComponent(bounds.size);

			if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
			{
				Vector3 min = bounds.min;
				min[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
				bounds.min = min;
			}
			else
			{
				Vector3 max = bounds.max;
				max[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
				bounds.max = max;
			}

			return bounds;
		}

		static Bounds GetBreastBounds(Transform relativeTo, Transform leftHips, Transform rightHips, Transform leftArm, Transform rightArm)
		{
			// Pelvis bounds
			Bounds bounds = new Bounds();
			bounds.Encapsulate(relativeTo.InverseTransformPoint(leftHips.position));
			bounds.Encapsulate(relativeTo.InverseTransformPoint(rightHips.position));
			bounds.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
			bounds.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
			Vector3 size = bounds.size;
			size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;
			bounds.size = size;
			return bounds;
		}


		public static void AddBreastColliders(Transform pelvis, Transform leftHips, Transform rightHips, Transform leftArm, Transform rightArm)
		{
			Bounds bounds = new Bounds();
			bounds.Encapsulate(pelvis.InverseTransformPoint(leftHips.position));
			bounds.Encapsulate(pelvis.InverseTransformPoint(rightHips.position));
			bounds.Encapsulate(pelvis.InverseTransformPoint(leftArm.position));
			bounds.Encapsulate(pelvis.InverseTransformPoint(rightArm.position));

			Vector3 size = bounds.size;
			size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;

			BoxCollider box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
			box.center = bounds.center;
			box.size = size;
		}

		public static void AddHeadCollider(Transform head, Transform pelvis, Transform leftArm, Transform rightArm)
		{
			if (head.GetComponent<Collider>())
				UnityEngine.Object.Destroy(head.GetComponent<Collider>());

			float radius = Vector3.Distance(leftArm.transform.position, rightArm.transform.position);
			radius /= 4;

			SphereCollider sphere = Undo.AddComponent<SphereCollider>(head.gameObject);
			sphere.radius = radius;
			Vector3 center = Vector3.zero;

			int direction;
			float distance;
			CalculateDirection(head.InverseTransformPoint(pelvis.position), out direction, out distance);
			if (distance > 0)
				center[direction] = -radius;
			else
				center[direction] = radius;
			sphere.center = center;
		}

		public static void BuildBodies(ref ArrayList bones)
		{
			foreach (BoneInfo bone in bones)
			{
				Undo.AddComponent<Rigidbody>(bone.anchor.gameObject);
				bone.anchor.GetComponent<Rigidbody>().mass = bone.density;
			}
		}

		public static void BuildJoints(ref ArrayList bones)
		{
			foreach (BoneInfo bone in bones)
			{
				if (bone.parent == null)
					continue;

				CharacterJoint joint = Undo.AddComponent<CharacterJoint>(bone.anchor.gameObject);
				bone.joint = joint;

				// Setup connection and axis
				joint.axis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.axis));
				joint.swingAxis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.normalAxis));
				joint.anchor = Vector3.zero;
				joint.connectedBody = bone.parent.anchor.GetComponent<Rigidbody>();
				joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

				// Setup limits
				SoftJointLimit limit = new SoftJointLimit();
				limit.contactDistance = 0; // default to zero, which automatically sets contact distance.

				limit.limit = bone.minLimit;
				joint.lowTwistLimit = limit;

				limit.limit = bone.maxLimit;
				joint.highTwistLimit = limit;

				limit.limit = bone.swingLimit;
				joint.swing1Limit = limit;

				limit.limit = 0;
				joint.swing2Limit = limit;
			}
		}

		public static void PrepareBones(ref BoneInfo rootBone, ref ArrayList bones, Transform pelvis, Transform middleSpine, Transform leftHips, Transform rightHips, Transform leftKnee, Transform rightKnee,
			Transform leftArm, Transform rightArm, Transform leftElbow, Transform rightElbow, Transform head, Vector3 up, Vector3 right, Vector3 forward, ref Vector3 worldUp)
		{
			Vector3 worldRight = Vector3.right;
			Vector3 worldForward = Vector3.forward;

			if (pelvis)
			{
				worldRight = pelvis.TransformDirection(right);
				worldUp = pelvis.TransformDirection(up);
				worldForward = pelvis.TransformDirection(forward);
			}

			bones = new ArrayList();

			rootBone = new BoneInfo();
			rootBone.name = "Pelvis";
			rootBone.anchor = pelvis;
			rootBone.parent = null;
			rootBone.density = 2.5F;
			bones.Add(rootBone);

			AddMirroredJoint(ref bones, "Hips", leftHips, rightHips, "Pelvis", worldRight, worldForward, -20, 70, 30, typeof(CapsuleCollider), 0.3F, 1.5F);
			AddMirroredJoint(ref bones, "Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -80, 0, 0, typeof(CapsuleCollider), 0.25F, 1.5F);

			AddJoint(ref bones, "Middle Spine", middleSpine, "Pelvis", worldRight, worldForward, -20, 20, 10, null, 1, 2.5F);

			AddMirroredJoint(ref bones, "Arm", leftArm, rightArm, "Middle Spine", worldUp, worldForward, -70, 10, 50, typeof(CapsuleCollider), 0.25F, 1.0F);
			AddMirroredJoint(ref bones, "Elbow", leftElbow, rightElbow, "Arm", worldForward, worldUp, -90, 0, 0, typeof(CapsuleCollider), 0.20F, 1.0F);

			AddJoint(ref bones, "Head", head, "Middle Spine", worldRight, worldForward, -40, 25, 25, null, 1, 1.0F);
		}

		public static void CalculateAxes(Transform head, Transform pelvis, Transform rightElbow, ref Vector3 up, ref Vector3 right, ref Vector3 forward)
		{
			if (head != null && pelvis != null)
				up = CalculateDirectionAxis(pelvis.InverseTransformPoint(head.position));
			if (rightElbow != null && pelvis != null)
			{
				Vector3 removed, temp;
				DecomposeVector(out temp, out removed, pelvis.InverseTransformPoint(rightElbow.position), up);
				right = CalculateDirectionAxis(removed);
			}

			forward = Vector3.Cross(right, up);

//		if (flipForward)
//			forward = -forward;
		}

		public static void Cleanup(ref ArrayList bones)
		{
			foreach (BoneInfo bone in bones)
			{
				if (!bone.anchor)
					continue;

				Component[] joints = bone.anchor.GetComponentsInChildren(typeof(Joint));
				foreach (Joint joint in joints)
					Undo.DestroyObjectImmediate(joint);

				Component[] bodies = bone.anchor.GetComponentsInChildren(typeof(Rigidbody));
				foreach (Rigidbody body in bodies)
					Undo.DestroyObjectImmediate(body);

				Component[] colliders = bone.anchor.GetComponentsInChildren(typeof(Collider));
				foreach (Collider collider in colliders)
					Undo.DestroyObjectImmediate(collider);
			}
		}

		static void DecomposeVector(out Vector3 normalCompo, out Vector3 tangentCompo, Vector3 outwardDir, Vector3 outwardNormal)
		{
			outwardNormal = outwardNormal.normalized;
			normalCompo = outwardNormal * Vector3.Dot(outwardDir, outwardNormal);
			tangentCompo = outwardDir - normalCompo;
		}

		static void AddMirroredJoint(ref ArrayList bones, string name, Transform leftAnchor, Transform rightAnchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
		{
			AddJoint(ref bones, "Left " + name, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
			AddJoint(ref bones, "Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
		}

		static void AddJoint(ref ArrayList bones, string name, Transform anchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
		{
			BoneInfo bone = new BoneInfo();
			bone.name = name;
			bone.anchor = anchor;
			bone.axis = worldTwistAxis;
			bone.normalAxis = worldSwingAxis;
			bone.minLimit = minLimit;
			bone.maxLimit = maxLimit;
			bone.swingLimit = swingLimit;
			bone.density = density;
			bone.colliderType = colliderType;
			bone.radiusScale = radiusScale;

			if (FindBone(bones, parent) != null)
				bone.parent = FindBone(bones, parent);
			else if (name.StartsWith("Left"))
				bone.parent = FindBone(bones, "Left " + parent);
			else if (name.StartsWith("Right"))
				bone.parent = FindBone(bones, "Right " + parent);


			bone.parent.children.Add(bone);
			bones.Add(bone);
		}

		static void CalculateMassRecurse(BoneInfo bone)
		{
			float mass = bone.anchor.GetComponent<Rigidbody>().mass;
			foreach (BoneInfo child in bone.children)
			{
				CalculateMassRecurse(child);
				mass += child.summedMass;
			}

			bone.summedMass = mass;
		}

		public static void CalculateMass(ref ArrayList bones, BoneInfo rootBone)
		{
			// Calculate allChildMass by summing all bodies
			CalculateMassRecurse(rootBone);

			// Rescale the mass so that the whole character weights totalMass
			float massScale = 20 / rootBone.summedMass;
			foreach (BoneInfo bone in bones)
				bone.anchor.GetComponent<Rigidbody>().mass *= massScale;

			// Recalculate allChildMass by summing all bodies
			CalculateMassRecurse(rootBone);
		}

		static void CalculateDirection(Vector3 point, out int direction, out float distance)
		{
			// Calculate longest axis
			direction = 0;
			if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
				direction = 1;
			if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
				direction = 2;

			distance = point[direction];
		}

		static Vector3 CalculateDirectionAxis(Vector3 point)
		{
			int direction = 0;
			float distance;
			CalculateDirection(point, out direction, out distance);
			Vector3 axis = Vector3.zero;
			if (distance > 0)
				axis[direction] = 1.0F;
			else
				axis[direction] = -1.0F;
			return axis;
		}

		static int LargestComponent(Vector3 point)
		{
			int direction = 0;
			if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
				direction = 1;
			if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
				direction = 2;
			return direction;
		}

		static int SmallestComponent(Vector3 point)
		{
			int direction = 0;
			if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
				direction = 1;
			if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
				direction = 2;
			return direction;
		}
#endif
	}
}
