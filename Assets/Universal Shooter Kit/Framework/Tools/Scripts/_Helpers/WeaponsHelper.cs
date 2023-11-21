using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
#if USK_DESTROYIT_INTEGRATION
using DestroyIt;
#endif
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
	public static class WeaponsHelper 
	{
		[Serializable]
		public class WeaponInfo
		{
			public Vector3 SaveTime;
			public Vector3 SaveDate;
			
			public bool HasTime;
			public bool disableIkInNormalState;
			public bool disableIkInCrouchState;
			public bool disableElbowIK = true;

			public Vector3 WeaponSize;
            
			public Vector3 WeaponPosition;
			public Vector3 WeaponRotation;
            
			public Vector3 RightHandPosition;
			public Vector3 LeftHandPosition;
            
			public Vector3 RightHandRotation;
			public Vector3 LeftHandRotation;
			
			public Vector3 RightCrouchHandPosition;
			public Vector3 LeftCrouchHandPosition;
            
			public Vector3 RightCrouchHandRotation;
			public Vector3 LeftCrouchHandRotation;
            
			public Vector3 RightAimPosition;
			public Vector3 LeftAimPosition;
            
			public Vector3 RightAimRotation;
			public Vector3 LeftAimRotation;

			public Vector3 RightElbowPosition;
			public Vector3 LeftElbowPosition;

			public Vector3 RightHandWallPosition;
			public Vector3 LeftHandWallPosition;
            
			public Vector3 RightHandWallRotation;
			public Vector3 LeftHandWallRotation;

			public float FingersRightX;
			public float FingersLeftX;
            
			public float FingersRightY;
			public float FingersLeftY;
            
			public float FingersRightZ;
			public float FingersLeftZ;
            
			public float ThumbRightX;
			public float ThumbLeftX;
            
			public float ThumbRightY;
			public float ThumbLeftY;
            
			public float ThumbRightZ;
			public float ThumbLeftZ;

//			public Vector3 CheckWallsColliderSize = Vector3.one;
//			public Vector3 CheckWallsBoxPosition;
//			public Vector3 CheckWallsBoxRotation;
			
			public float timeInHand_FPS = 2;
			public float timeBeforeCreating_FPS = 1;
			
			public float timeInHand_TPS = 2;
			public float timeBeforeCreating_TPS = 1;

			public void CloneToAim(WeaponInfo CloneFrom, string CloneFromState)
			{
				switch (CloneFromState)
				{
					case "Aim":
						RightAimPosition = CloneFrom.RightAimPosition;
						RightAimRotation = CloneFrom.RightAimRotation;
						LeftAimPosition = CloneFrom.LeftAimPosition;
						LeftAimRotation = CloneFrom.LeftAimRotation;
						break;
					case "Norm":
						RightAimPosition = CloneFrom.RightHandPosition;
						RightAimRotation = CloneFrom.RightHandRotation;
						LeftAimPosition = CloneFrom.LeftHandPosition;
						LeftAimRotation = CloneFrom.LeftHandRotation;
						break;
					case "Wall":
						RightAimPosition = CloneFrom.RightHandWallPosition;
						RightAimRotation = CloneFrom.RightHandWallRotation;
						LeftAimPosition = CloneFrom.LeftHandWallPosition;
						LeftAimRotation = CloneFrom.LeftHandWallRotation;
						break;
					case "Crouch":
						RightAimPosition = CloneFrom.RightCrouchHandPosition;
						RightAimRotation = CloneFrom.RightCrouchHandRotation;
						LeftAimPosition = CloneFrom.LeftCrouchHandPosition;
						LeftAimRotation = CloneFrom.LeftCrouchHandRotation;
						break;
				}

				WeaponClone(CloneFrom);
			}

			public void CloneToNorm(WeaponInfo CloneFrom, string CloneFromState)
			{
				switch (CloneFromState)
				{
					case "Aim":
						RightHandPosition = CloneFrom.RightAimPosition;
						RightHandRotation = CloneFrom.RightAimRotation;
						LeftHandPosition = CloneFrom.LeftAimPosition;
						LeftHandRotation = CloneFrom.LeftAimRotation;
						break;
					case "Norm":
						disableIkInNormalState = CloneFrom.disableIkInNormalState;
						
						RightHandPosition = CloneFrom.RightHandPosition;
						RightHandRotation = CloneFrom.RightHandRotation;
						LeftHandPosition = CloneFrom.LeftHandPosition;
						LeftHandRotation = CloneFrom.LeftHandRotation;
						break;
					case "Wall":
						RightHandPosition = CloneFrom.RightHandWallPosition;
						RightHandRotation = CloneFrom.RightHandWallRotation;
						LeftHandPosition = CloneFrom.LeftHandWallPosition;
						LeftHandRotation = CloneFrom.LeftHandWallRotation;
						break;
					case "Crouch":
						RightHandPosition = CloneFrom.RightCrouchHandPosition;
						RightHandRotation = CloneFrom.RightCrouchHandRotation;
						LeftHandPosition = CloneFrom.LeftCrouchHandPosition;
						LeftHandRotation = CloneFrom.LeftCrouchHandRotation;
						break;
				}
				
				WeaponClone(CloneFrom);
			}

			public void CloneToWall(WeaponInfo CloneFrom, string CloneFromState)
			{
				
				switch (CloneFromState)
				{
					case "Aim":
						RightHandWallPosition = CloneFrom.RightAimPosition;
						RightHandWallRotation = CloneFrom.RightAimRotation;
						LeftHandWallPosition = CloneFrom.LeftAimPosition;
						LeftHandWallRotation = CloneFrom.LeftAimRotation;
						break;
					case "Norm":
						RightHandWallPosition = CloneFrom.RightHandPosition;
						RightHandWallRotation = CloneFrom.RightHandRotation;
						LeftHandWallPosition = CloneFrom.LeftHandPosition;
						LeftHandWallRotation = CloneFrom.LeftHandRotation;
						break;
					case "Wall":
						RightHandWallPosition = CloneFrom.RightHandWallPosition;
						RightHandWallRotation = CloneFrom.RightHandWallRotation;
						LeftHandWallPosition = CloneFrom.LeftHandWallPosition;
						LeftHandWallRotation = CloneFrom.LeftHandWallRotation;
						break;
					case "Crouch":
						RightHandWallPosition = CloneFrom.RightCrouchHandPosition;
						RightHandWallRotation = CloneFrom.RightCrouchHandRotation;
						LeftHandWallPosition = CloneFrom.LeftCrouchHandPosition;
						LeftHandWallRotation = CloneFrom.LeftCrouchHandRotation;
						break;
				}

//				CheckWallsColliderSize = CloneFrom.CheckWallsColliderSize;
//				CheckWallsBoxPosition = CloneFrom.CheckWallsBoxPosition;
//				CheckWallsBoxRotation = CloneFrom.CheckWallsBoxRotation;
				
				WeaponClone(CloneFrom);
			}

			public void CloneToCrouch(WeaponInfo CloneFrom, string CloneFromState)
			{
				switch (CloneFromState)
				{
					case "Aim":
						RightCrouchHandPosition = CloneFrom.RightAimPosition;
						RightCrouchHandRotation = CloneFrom.RightAimRotation;
						LeftCrouchHandPosition = CloneFrom.LeftAimPosition;
						LeftCrouchHandRotation = CloneFrom.LeftAimRotation;
						break;
					case "Norm":
						RightCrouchHandPosition = CloneFrom.RightHandPosition;
						RightCrouchHandRotation = CloneFrom.RightHandRotation;
						LeftCrouchHandPosition = CloneFrom.LeftHandPosition;
						LeftCrouchHandRotation = CloneFrom.LeftHandRotation;
						break;
					case "Wall":
						RightCrouchHandPosition = CloneFrom.RightHandWallPosition;
						RightCrouchHandRotation = CloneFrom.RightHandWallRotation;
						LeftCrouchHandPosition = CloneFrom.LeftHandWallPosition;
						LeftCrouchHandRotation = CloneFrom.LeftHandWallRotation;
						break;
					case "Crouch":
						disableIkInCrouchState = CloneFrom.disableIkInCrouchState;
						
						RightCrouchHandPosition = CloneFrom.RightCrouchHandPosition;
						RightCrouchHandRotation = CloneFrom.RightCrouchHandRotation;
						LeftCrouchHandPosition = CloneFrom.LeftCrouchHandPosition;
						LeftCrouchHandRotation = CloneFrom.LeftCrouchHandRotation;
						break;
				}

				WeaponClone(CloneFrom);
			}

			public void ElbowsClone(WeaponInfo CloneFrom)
			{
				disableElbowIK = CloneFrom.disableElbowIK;
				
				RightElbowPosition = CloneFrom.RightElbowPosition;
				LeftElbowPosition = CloneFrom.LeftElbowPosition;
			}

			public void FingersClone(WeaponInfo CloneFrom)
			{
				FingersRightX = CloneFrom.FingersRightX;
				FingersRightY = CloneFrom.FingersRightY;
				FingersRightZ = CloneFrom.FingersRightZ;

				FingersLeftX = CloneFrom.FingersLeftX;
				FingersLeftY = CloneFrom.FingersLeftY;
				FingersLeftZ = CloneFrom.FingersLeftZ;

				ThumbRightX = CloneFrom.ThumbRightX;
				ThumbRightY = CloneFrom.ThumbRightY;
				ThumbRightZ = CloneFrom.ThumbRightZ;

				ThumbLeftX = CloneFrom.ThumbLeftX;
				ThumbLeftY = CloneFrom.ThumbLeftY;
				ThumbLeftZ = CloneFrom.ThumbLeftZ;
			}

			public void WeaponClone(WeaponInfo CloneFrom)
			{
				WeaponSize = CloneFrom.WeaponSize;
				WeaponPosition = CloneFrom.WeaponPosition;
				WeaponRotation = CloneFrom.WeaponRotation;
			}

			public void Clone(WeaponInfo CloneFrom)
			{
				HasTime = CloneFrom.HasTime;
				SaveTime = CloneFrom.SaveTime;
				SaveDate = CloneFrom.SaveDate;
				
				WeaponClone(CloneFrom);

				CloneToNorm(CloneFrom, "Norm");
				
				CloneToCrouch(CloneFrom, "Crouch");
				
				CloneToAim(CloneFrom, "Aim");

				CloneToWall(CloneFrom, "Wall");
				
				ElbowsClone(CloneFrom);

				FingersClone(CloneFrom);

				timeInHand_FPS = CloneFrom.timeInHand_FPS;
				timeBeforeCreating_FPS = CloneFrom.timeBeforeCreating_FPS;

				timeInHand_TPS = CloneFrom.timeInHand_TPS;
				timeBeforeCreating_TPS = CloneFrom.timeBeforeCreating_TPS;
			}
		}
		
		[Serializable]
		public class WeaponAnimation
		{
			public AnimationClip idle;
			public AnimationClip take;
			public AnimationClip fpWalk;
			public AnimationClip fpRun;
		}
		
		public enum ShootingMethod
		{
			Raycast,
			InstantiateBullet
		}

		[Serializable]
		public class BulletsSettings
		{
			public bool Active = true;
			[Range(-100, 100)] public int weapon_damage;
			[Range(0, 10)] public float RateOfShoot = 0.5f;
			[Range(0, 2)] public float bulletsScatterX = 0.02f;
			[Range(0, 2)] public float bulletsScatterY = 0.02f;

			[Tooltip("If the weapon has multiple attacks, add an image so that players can distinguish them")]
			public Texture attackImage;
		}

		[Serializable]
		public class Attack
		{
			public TypeOfAttack AttackType = TypeOfAttack.Bullets;
			public Helper.RotationAxes barrelRotationAxes;
			
			public List<BulletsSettings> BulletsSettings = new List<BulletsSettings>{new BulletsSettings(), new BulletsSettings()};

			public ShootingMethod shootingMethod;
			public GameObject bulletPrefab;
			
			[Tooltip("If the weapon has several attacks, add images so that players can distinguish between them")]
			public Texture attackImage;
			
			[Tooltip("This icon will be displayed during the attack in the mobile mode")]
			public Texture mobileAttackButton;

			public List<AnimationClip> fpAttacks = new List<AnimationClip>{null, null};
			public List<AnimationClip> tpAttacks = new List<AnimationClip>{null, null};
			public List<AnimationClip> tpCrouchAttacks = new List<AnimationClip>{null};
			public List<AnimationClip> tdAttacks = new List<AnimationClip>{null, null};
			public AnimationClip reloadAnimation;
			
			[Range(0, 10)] public float RateOfAttack = 0.5f;
			[Range(0, 0.2f)] public float bulletsScatterX = 0.02f;
			[Range(0, 0.2f)] public float bulletsScatterY = 0.02f;
			[Range(1, 500)] public float flightSpeed = 20;
			[Range(0.1f, 30)]public float GrenadeExplosionTime = 3;
			[Range(1, 100)]public float attackDistance = 50;
			[Range(10, 200)] public float CrosshairSize = 25;
			
			[Tooltip("This parameter controls how many times a radius of noise increases during the attack" + "\n" +
			         "(if you are not using enemies, you don't need to use this variable)")]
			[Range(1, 20)] public float attackNoiseRadiusMultiplier = 2;

			public float curAmmo = 12;
			public float maxAmmo = 12;
			public float inventoryAmmo = 24;
			
			public float penetrationMultiplier = 1;
			
			[Range(-100, 100)] public int weapon_damage = 10;
			public int inspectorTab;
			public int currentBulletType;
			
			public Vector2[] crosshairPartsPositions = 
			{
				new Vector2(0, 0), new Vector2(0, 30), new Vector2(0, -30), new Vector2(30, 0), new Vector2(-30, 0)
			};

			public List<GameObject> TempMagazine = new List<GameObject>();
			
			[Tooltip("A model for synchronization with animations" + "\n" +
			         "Read more about that in the Documentation (section 'Events' -> 'Weapons' -> 'Magazines')")]
			public GameObject magazine;
			
			public GameObject Barrel;
			public GameObject MuzzleFlash;
			public GameObject Shell;

			public GameObject bulletTrail;
			
			public Transform AttackSpawnPoint;
			public Transform ShellPoint;
			public Transform Explosion;
			
			public BoxCollider AttackCollider;
			public List<ParticleSystem> attackEffects = new List<ParticleSystem>();
			
			public AudioClip AttackAudio;
			public AudioClip ReloadAudio;
			public AudioClip NoAmmoShotAudio;
			
			public Sprite UpPart;
			public Sprite DownPart;
			public Sprite LeftPart;
			public Sprite RightPart;
			public Sprite MiddlePart;

			public CrosshairType sightType;

			public bool StickToObject;
			public bool autoAttack;
			public bool ExplodeWhenTouchGround = true;
			
			public bool showTrajectory = true;
			
			[Tooltip("If this checkbox is active, shells appear immediately when a character shoots" + "\n\n" +
			         "If you need a delay for that (for example, for shotguns or rifles), add an event called [SpawnShell] to a shot animation")]
			public bool spawnShellsImmediately = true;
			
			[Tooltip("If active, the explosion will push objects with the [Rigidbody] component")]
			public bool applyForce = true;
			
			public bool applyGravity = true;
			
			public bool useTakeAnimation = true;
			
			[Tooltip("Use it, if you need a Flash Grenade effect")]
			public bool flashExplosion;

			[Tooltip("Write the same type in a PickUp script")]
			public string ammoName = "gun";
		}

		[Serializable]
		public class IKSlot
		{
			public int fpsSettingsSlot;
			public int tpsSettingsSlot;
			public int tdsSettingsSlot;
			
			public int currentTag;
		}

		[Serializable]
		public class WeaponSlotInInventory
		{
			public GameObject weapon;

			public bool fistAttack;
			
//			public int tpSlotIndex;
//			public int tdSlotIndex;
//			public int fpSlotIndex;
//			public List<string> saveSlotsNames;
		}

		[Serializable]
		public class GrenadeSlot
		{
			public GameObject Grenade;
			public int grenadeAmmo;
//			public int saveSlotIndex;
			public WeaponController GrenadeScript;
		}
		
		[Serializable]
		public class IKObjects
		{
			public Transform RightObject;
			public Transform LeftObject;
			
			public Transform RightAimObject;
			public Transform LeftAimObject;
			
			public Transform RightCrouchObject;
			public Transform LeftCrouchObject;

			public Transform RightWallObject;
			public Transform LeftWallObject;

			public Transform RightElbowObject;
			public Transform LeftElbowObject;
		}

		[Serializable]
		public class GrenadeParameters
		{
			[Range(1, 100)] public float GrenadeSpeed = 20;
			[Range(0.1f, 30)]public float GrenadeExplosionTime = 3;
			
			public bool ExplodeWhenTouchGround;
			
			public GameObject GrenadeExplosion;
			
			public AudioClip ThrowAudio;

			public AnimationClip GrenadeThrow_FPS;
			public AnimationClip GrenadeThrow_TPS_TDS;
		}
		
//		[Serializable]
//		public class GrenadeInfo
//		{
//			
//		}

		// public enum WeaponWeight
		// {
		// 	Light, Medium, Heavy
		// }
		
		public enum TypeOfAttack
		{
			Bullets,
			Rockets,
			GrenadeLauncher,
			Flame,
			Melee,
			Grenade,
			Minigun
		}

		public enum CrosshairType
		{
			OnePart, TwoParts, FourParts
		}
		

		public static void PlaceWeapon(WeaponInfo weaponInfo, Transform target)
		{
			if (weaponInfo.WeaponSize != Vector3.zero)
				target.localScale = weaponInfo.WeaponSize;

			target.localPosition = weaponInfo.WeaponPosition;
			target.localEulerAngles = weaponInfo.WeaponRotation;
		}

		public static void SetWeaponPositions(WeaponController weaponController, bool placeAll, Transform dirObj)
		{
			IKHelper.CheckIK(ref weaponController.CanUseElbowIK, ref weaponController.CanUseIK, ref weaponController.CanUseAimIK, ref weaponController.CanUseWallIK, ref weaponController.CanUseCrouchIK, weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex]);

			var slot = weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex];
			
			IKHelper.PlaceAllIKObjects(weaponController, slot, placeAll, dirObj);
			
			PlaceWeapon(weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex], weaponController.transform);
		}

		public static bool HasAnimationCollidersEvent(AnimationClip animationClip)
		{
			if (animationClip == null) return false;
			
			var hasCollidersEvent = false;

			if (animationClip.events.Length > 0)
			{
				foreach (var _event in animationClip.events)
				{
					if (_event.functionName == "MeleeColliders")
						hasCollidersEvent = true;
				}
			}

			return hasCollidersEvent;
		}

		public static void SetHandsSettingsSlot(ref int SettingsSlotIndex, int tag, WeaponController weaponController, bool enable)
		{
			if (weaponController.IkSlots.Any(slot => slot.currentTag == tag))
			{
				var _slot = weaponController.IkSlots.Find(slot => slot.currentTag == tag);

				SettingsSlotIndex = enable ? _slot.fpsSettingsSlot : _slot.tpsSettingsSlot;
			}
		}

		public static void SetHandsSettingsSlot(ref int SettingsSlotIndex, int tag, CharacterHelper.CameraType type, WeaponController weaponController)
		{
			if (weaponController.IkSlots.Any(slot => slot.currentTag == tag))
			{
				var _slot = weaponController.IkSlots.Find(slot => slot.currentTag == tag);
				
				switch (type)
				{
					case CharacterHelper.CameraType.FirstPerson:
						SettingsSlotIndex = _slot.fpsSettingsSlot;
						break;
					case CharacterHelper.CameraType.ThirdPerson:
						SettingsSlotIndex = !weaponController.Controller.emulateTDModeLikeTP ? _slot.tpsSettingsSlot : _slot.tdsSettingsSlot;
						break;
					case CharacterHelper.CameraType.TopDown:
						SettingsSlotIndex = _slot.tdsSettingsSlot;
						break;
				}
			}
			else
			{
				switch (type)
				{
					case CharacterHelper.CameraType.FirstPerson:
						SettingsSlotIndex = weaponController.IkSlots[0].fpsSettingsSlot;
						break;
					case CharacterHelper.CameraType.ThirdPerson:
						SettingsSlotIndex = weaponController.IkSlots[0].tpsSettingsSlot;
						break;
					case CharacterHelper.CameraType.TopDown:
						SettingsSlotIndex = weaponController.IkSlots[0].tdsSettingsSlot;
						break;
				}
			}
		}

		public static void MinigunBarrelRotation(WeaponController weaponController)
		{
			if (weaponController.Attacks[weaponController.currentAttack].AttackType == TypeOfAttack.Minigun && !weaponController.Controller.isPause && !weaponController.Controller.CameraController.cameraPause)
			{
				if (weaponController.Attacks[weaponController.currentAttack].Barrel)
				{
					var transformLocalEulerAngles = weaponController.Attacks[weaponController.currentAttack].Barrel.transform.localEulerAngles;
					switch (weaponController.Attacks[weaponController.currentAttack].barrelRotationAxes)
					{
						case Helper.RotationAxes.X:
							transformLocalEulerAngles.x += weaponController.BarrelRotationSpeed;
							break;
						case Helper.RotationAxes.Y:
							transformLocalEulerAngles.y += weaponController.BarrelRotationSpeed;
							break;
						case Helper.RotationAxes.Z:
							transformLocalEulerAngles.z += weaponController.BarrelRotationSpeed;
							break;
					}
					weaponController.Attacks[weaponController.currentAttack].Barrel.transform.localEulerAngles = transformLocalEulerAngles;
				}
			}
		}

		public static void SetWeaponController(GameObject instantiatedWeapon, GameObject originalWeapon, int saveSlot, InventoryManager manager, Controller controller, Transform parent)
		{
			var weaponController = instantiatedWeapon.GetComponent<WeaponController>();

			SetWeaponController(weaponController, instantiatedWeapon, originalWeapon, parent, controller.BodyObjects);

//			weaponController.tpsSettingsSlot = saveSlot;
			weaponController.WeaponManager = manager;
			weaponController.Controller = controller;
			
			weaponController.enabled = true;
		}

		public static void SetWeaponController(GameObject instantiatedWeapon, GameObject originalWeapon, InventoryManager manager, Controller controller, Transform parent)
		{
			var weaponController = instantiatedWeapon.GetComponent<WeaponController>();
			
			SetWeaponController(weaponController, instantiatedWeapon, originalWeapon, parent, controller.BodyObjects);
			
			weaponController.WeaponManager = manager;
			weaponController.Controller = controller;
			
			weaponController.enabled = true;
		}
		
		public static GameObject InstantiateWeapon(GameObject weapon, int index, InventoryManager manager, Controller controller, List<CharacterHelper.Kit> ammoKits = null)
		{
			var name = weapon.name;
			
			if(ammoKits == null) ammoKits = new List<CharacterHelper.Kit>();

			var instantiatedWeapon = Object.Instantiate(weapon);

			if (instantiatedWeapon.GetComponent<PickupItem>())
			{
				var pickUpScript = instantiatedWeapon.GetComponent<PickupItem>();
				if (pickUpScript.pickUpArea)
					Object.DestroyImmediate(pickUpScript.pickUpArea);
				
				Object.DestroyImmediate(instantiatedWeapon.GetComponent<PickupItem>());
			}
			
			instantiatedWeapon.name = name;
		
			manager.slots[index].currentWeaponInSlot = 0;
			manager.slots[index].weaponSlotInGame.Add(new CharacterHelper.Weapon {weapon = instantiatedWeapon, WeaponAmmoKits = ammoKits});
			manager.hasAnyWeapon = true;
			manager.allWeaponsCount++;

			SetWeaponController(instantiatedWeapon, weapon, manager, controller, controller.transform);

			return instantiatedWeapon;
		}

		static void SetWeaponController(WeaponController weaponController, GameObject instantiatedWeapon, GameObject originalWeapon, Transform parent, CharacterHelper.BodyObjects objects)
		{
			weaponController = instantiatedWeapon.GetComponent<WeaponController>();
			weaponController.OriginalScript = originalWeapon.GetComponent<WeaponController>();

			foreach (var attack in weaponController.Attacks)
			{
				attack.curAmmo = attack.AttackType != TypeOfAttack.Grenade ? attack.maxAmmo : attack.inventoryAmmo;
			}

//			if (weaponController.Attacks[weaponController.currentAttack].AttackType != TypeOfAttack.Grenade)
//			{
				if (objects.RightHand && instantiatedWeapon.transform.parent != objects.RightHand)
					instantiatedWeapon.transform.parent = objects.RightHand;
//			}
//			else
//			{
//				if (objects.LeftHand && instantiatedWeapon.transform.parent != objects.LeftHand)
//					instantiatedWeapon.transform.parent = objects.LeftHand;
//			}
		}

		public static int GetRandomIndex(List<AnimationClip> animations, ref int lastAttackAnimationIndex)
		{
			var animationIndex = Random.Range(0, animations.Count);
                    
			if (animationIndex == lastAttackAnimationIndex)
			{
				animationIndex++;

				if (animationIndex > animations.Count - 1)
					animationIndex = 0;
			}

			lastAttackAnimationIndex = animationIndex;

			return animationIndex;
		}

		public static void AddTrail(GameObject Tracer, Vector3 TargetPoint, Material Material, float Size)
		{
			var tracerScript = Tracer.gameObject.AddComponent<FlyingProjectile>();
			tracerScript.isTracer = true;
			tracerScript.TargetPoint = TargetPoint;
			tracerScript.Speed = 300;
		}

		private static void InstantiateBullet(GameObject bulletPrefab, Vector3 startPoint, Vector3 targetPoint, float speed, float damage, Texture weaponImage, Transform directionObject, Controller characterOwner = null, AIController aiOwner = null)
		{
			var direction = targetPoint - startPoint;
			var bullet = Object.Instantiate(bulletPrefab, startPoint, Quaternion.LookRotation(direction));
			bullet.SetActive(true);
			bullet.hideFlags = HideFlags.HideInHierarchy;
                            
			var bulletScript = bullet.AddComponent<FlyingProjectile>();
			bulletScript.startPosition = startPoint;
			// bulletScript.isMultiplayerWeapon = isMultiplayerWeapon;
			bulletScript.isBullet = true;
			if (weaponImage) bulletScript.WeaponImage = weaponImage;
			
			bulletScript.TargetPoint = targetPoint + direction * 10;
			bulletScript.directionObject = directionObject;
			// bulletScript.isRaycast = bulletScript.TargetPoint != Vector3.zero;
			bulletScript.Speed = speed;
			bulletScript.damage = damage;
			
			if(characterOwner) bulletScript.characterOwner = characterOwner;
			else if (aiOwner) bulletScript.aiOwner = aiOwner;
			
			// if (weaponController.Controller.multiplayerNickname != null) bulletScript.characterOwner = weaponController.Controller;
		}
		
		public static void InstantiateBullet(AIController aiController, Vector3 startPoint, Vector3 targetPoint)
		{
			var attack = aiController.Attacks[0];
			InstantiateBullet(attack.bullet, startPoint, targetPoint, attack.flightSpeed, attack.Damage, null, attack.AttackSpawnPoints[0],null, aiController);
		}

		public static void InstantiateBullet(WeaponController weaponController, Vector3 startPoint, Vector3 targetPoint)
		{
			var attack = weaponController.Attacks[weaponController.currentAttack];
			InstantiateBullet(attack.bulletPrefab, startPoint, targetPoint, attack.flightSpeed, attack.weapon_damage, weaponController.weaponImage, weaponController.Controller.thisCamera.transform, weaponController.Controller);
		}

		public static void CreateTrail(Vector3 startPoint, Vector3 targetPoint, GameObject trailPrefab)
		{
			var dir = targetPoint - startPoint;
			
			var tracer = Object.Instantiate(trailPrefab, startPoint,  Quaternion.LookRotation (dir));

			tracer.hideFlags = HideFlags.HideInHierarchy;
			
			if (tracer.GetComponent<TrailRenderer>())
			{
				var tracerScript = tracer.gameObject.AddComponent<FlyingProjectile>();
				tracerScript.isTracer = true;
				tracerScript.TargetPoint = targetPoint;
				tracerScript.Speed = 200;
			}
			else if (tracer.GetComponent<LineRenderer>())
			{
				var script = tracer.GetComponent<LineRenderer>();
				script.SetPositions(new []{targetPoint, startPoint});
			}

			// AddTrail(tracer, DirectionPoint, TrailMaterial, 0.3f);
		}

		private static Transform CreateSparks(Surface surface, Vector3 position, Quaternion rotation, Transform parent)
		{
			var spark = GameObject.Instantiate(surface.Sparks, position, rotation);
			spark.hideFlags = HideFlags.HideInHierarchy;
			spark.transform.parent = parent;
			
			if (surface.HitAudio)
			{
				var _audio = !parent.gameObject.GetComponent<AudioSource>() ? parent.gameObject.AddComponent<AudioSource>() : parent.gameObject.GetComponent<AudioSource>();
				_audio.clip = surface.HitAudio;
				_audio.spatialBlend = 1;
				_audio.minDistance = 10;
				_audio.maxDistance = 100;
				_audio.PlayOneShot(parent.gameObject.GetComponent<AudioSource>().clip);
			}

			return spark;
		}

		private static void CreateHitPoint(Surface surface, Vector3 position, Quaternion rotation, Transform parent)
		{
			var hitGO = GameObject.Instantiate(surface.Hit, position, rotation).transform;
			hitGO.hideFlags = HideFlags.HideInHierarchy;
			hitGO.parent = parent;
		}

		public static bool CanAim(bool isMultiplayerWeapon, Controller controller)
		{
			var canAim = false;
			
			if (!isMultiplayerWeapon)
			{
				if (controller.TypeOfCamera != CharacterHelper.CameraType.TopDown && !controller.isPause && !controller.CameraController.Occlusion)
					canAim = true;

				// if (controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && controller.CameraParameters.alwaysTPAim)
				// 	canAim = false;
			}
			else
			{
				canAim = true;
			}

			return canAim;
		}

		public static void CreateBlood(AIController aiController, Projector bloodProjector, Vector3 position, Quaternion rotation, Transform parent, List<Texture> bloodHoles)
		{
			aiController.bloodMarkersOnBody.Add(CreateBlood(bloodProjector, position, rotation, parent, bloodHoles));
		}

		public static GameObject CreateBlood(Projector bloodProjector, Vector3 position, Quaternion rotation, Transform parent, List<Texture> bloodHoles)
		{
			if (bloodHoles.Count <= 0) return null;
			
			var hole = Object.Instantiate(bloodProjector, position, rotation);
			hole.transform.parent = parent;
			hole.gameObject.hideFlags = HideFlags.HideInHierarchy;

			var index = Random.Range(0, bloodHoles.Count - 1);

			if (bloodHoles[index])
				hole.material.mainTexture = bloodHoles[index];

			return hole.gameObject;
		}

		public static void InstantiateAdditionalBloodEffects(List<GameObject> effects, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if(effects.Count == 0) return;
			
			var index = Random.Range(0, effects.Count);

			if (effects[index])
			{
				Object.Instantiate(effects[index], position, rotation, parent);
			}
		}
		

		public static bool UpdateAttackDirection(WeaponController script, ref RaycastHit raycastHit, out bool isHit)
		{
			isHit = true;

			if (!script.DetectObject && !script.Controller.CameraController.cameraOcclusion && (script.Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && (script.isAimEnabled || script.Controller.isAlwaysTpAimEnabled) || script.Controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson))
            {
                if (Mathf.Abs(script.Controller.anim.GetFloat("CameraAngle")) < 60)
                {
	                if (script.Controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || script.Controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && script.Controller.emulateTDModeLikeTP)
	                {
		                if (!script.Controller.CameraParameters.lockCamera)
		                {
			                script.tempCamera.position = script.Controller.thisCamera.transform.position;
			                script.tempCamera.rotation = Quaternion.Euler(90 - script.Controller.CameraParameters.shootingAngleCorrection, script.Controller.thisCamera.transform.eulerAngles.y, script.Controller.thisCamera.transform.eulerAngles.z);
			                script.attackDirection = script.tempCamera.TransformDirection(Vector3.up + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y)) * 2);
		                }
		                else
		                {
			                if (script.Controller.CameraParameters.lookAtCursor)
			                {
				                if (!script.Controller.bodyLimit)
				                {
					                var dir = script.Controller.CameraController.BodyLookAt.position - script.Attacks[script.currentAttack].AttackSpawnPoint.position;
					                script.attackDirection = dir + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y) * 2) * 10;
					                script.lastAttackDirection = script.Controller.transform.InverseTransformDirection(dir);
				                }
				                else
				                {
					                var dir = script.Controller.transform.TransformDirection(script.lastAttackDirection);
					                script.attackDirection = dir + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y) * 2) * 10;
				                }
			                }
			                else
			                {
				                var tempPos = script.Controller.CameraController.BodyLookAt.position;
				                tempPos = new Vector3(tempPos.x, script.Attacks[script.currentAttack].AttackSpawnPoint.position.y, tempPos.z);
				                var dir = tempPos - script.Attacks[script.currentAttack].AttackSpawnPoint.position;
				                
				                var rightVector = new Vector3(dir.z, 0, -dir.x).normalized;
				                var correctedVector = Quaternion.AngleAxis(-script.Controller.CameraParameters.shootingAngleCorrection, rightVector) * dir;
				                
				                script.attackDirection = correctedVector + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y) * 2) * 10;
			                }
		                }
		                
		                if (Physics.Raycast(script.Attacks[script.currentAttack].AttackSpawnPoint.position, script.attackDirection, out raycastHit, 10000f, Helper.MultiplayerLayerMask(script.isMultiplayerWeapon)))
		                {
			                return true;
		                }
	                }
	                else
	                {
		                script.attackDirection = script.Controller.thisCamera.transform.TransformDirection(Vector3.forward + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), 0));
		                if (Physics.Raycast(script.Controller.thisCamera.transform.position, script.attackDirection, out raycastHit, 10000f, Helper.MultiplayerLayerMask(script.isMultiplayerWeapon)))
			                return true;

		                isHit = false;
		                raycastHit = new RaycastHit {point = script.Attacks[script.currentAttack].AttackSpawnPoint.position + script.Controller.thisCamera.transform.forward * 100 + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), 0)};
		                
		                return true;
	                }
                }
                else
                {
	                script.attackDirection = script.Attacks[script.currentAttack].AttackSpawnPoint.forward;
	                if (Physics.Raycast(script.Controller.thisCamera.transform.position, script.attackDirection, out raycastHit, 10000f, Helper.MultiplayerLayerMask(script.isMultiplayerWeapon)))
		                return true;
	                
	                isHit = false;
	                raycastHit = new RaycastHit {point = script.Attacks[script.currentAttack].AttackSpawnPoint.position + script.Attacks[script.currentAttack].AttackSpawnPoint.forward * 100 + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), 0)};

	                return true;
                }
            }
            else
            {
	            
	            script.attackDirection = script.Attacks[script.currentAttack].AttackSpawnPoint.forward;
	            if (Physics.Raycast(script.Controller.thisCamera.transform.position, script.attackDirection, out raycastHit, 10000f, Helper.MultiplayerLayerMask(script.isMultiplayerWeapon)))
		            return true;
	            
	            isHit = false;
	            raycastHit = new RaycastHit {point = script.Attacks[script.currentAttack].AttackSpawnPoint.position + script.Attacks[script.currentAttack].AttackSpawnPoint.forward * 100 + new Vector3(Random.Range(-script.currentBulletsScatter.x, script.currentBulletsScatter.x), Random.Range(-script.currentBulletsScatter.y, script.currentBulletsScatter.y), 0)};

	            return true;
            }

            return false;
        }

		public static void CheckBulletRaycast(Collider collider, Vector3 point, Vector3 normal, WeaponController weaponController)
		{
			CheckBulletRaycast(collider.gameObject, point, normal, weaponController, true, true);
		}

		public static void CheckBulletRaycast(RaycastHit raycastHit, WeaponController weaponController)
		{
			CheckBulletRaycast(raycastHit.transform ? raycastHit.transform.gameObject : null, raycastHit.point, raycastHit.normal , weaponController, true, true);
		}

		private static void CheckBulletRaycast(GameObject targetGameObject, Vector3 hitPoint, Vector3 hitNormal,  WeaponController weaponController, bool isPenetration, bool isRicochet)
		{
			var bloodHoles = new List<Texture>();
			
			var hitRotation = Quaternion.FromToRotation(Vector3.forward, hitNormal);

			if (!targetGameObject) return;
			
			if (targetGameObject.GetComponent<BodyPartCollider>())
			{
				var bodyColliderScript = targetGameObject.GetComponent<BodyPartCollider>();

				if (bodyColliderScript.aiController)
				{
					if (MultiplayerHelper.CanDamageInMultiplayer(weaponController.Controller, bodyColliderScript.aiController))
					{
						var enemyScript = bodyColliderScript.aiController;
						
						bloodHoles = enemyScript.BloodHoles;

						if(!enemyScript.multiplayerBot)
							enemyScript.PlayDamageAnimation();
#if USK_MULTIPLAYER
						if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
						{
							enemyScript.Damage((int) (weaponController.Attacks[weaponController.currentAttack].weapon_damage * bodyColliderScript.damageMultiplayer), bodyColliderScript.bodyPart != BodyPartCollider.BodyPart.Head ? "bullet" : "headshot", new Helper.ActorID{actorID = weaponController.Controller.CharacterSync.photonView.OwnerActorNr, type = "player"});
						}
						else
#endif
						{
							enemyScript.Damage((int) (weaponController.Attacks[weaponController.currentAttack].weapon_damage * bodyColliderScript.damageMultiplayer), bodyColliderScript.bodyPart != BodyPartCollider.BodyPart.Head ? "bullet" : "headshot",
#if USK_MULTIPLAYER
								 PhotonNetwork.InRoom ? new Helper.ActorID{actorID = weaponController.Controller.CharacterSync.photonView.OwnerActorNr, type = "player"} :
#endif
									new Helper.ActorID{actorID = weaponController.Controller.gameObject.GetInstanceID(), type = "instanceID"}

								);
							
						}
						
						if (enemyScript.bloodProjector)
							CreateBlood(enemyScript, enemyScript.bloodProjector, hitPoint - hitNormal * 0.15f, Quaternion.LookRotation(hitPoint - hitNormal * 2), targetGameObject.transform, bloodHoles);

						InstantiateAdditionalBloodEffects(enemyScript.additionalHitEffects, hitPoint, Quaternion.LookRotation(hitNormal));
					}
				}
				else if (bodyColliderScript.controller)
				{
					var damagedOpponentController = bodyColliderScript.controller;

					if (!weaponController.isMultiplayerWeapon)
					{
						if (MultiplayerHelper.CanDamageInMultiplayer(weaponController.Controller, damagedOpponentController))
						{

#if USK_MULTIPLAYER
// #if USK_ADVANCED_MULTIPLAYER && USK_MULTIPLAYER
// 							if (damagedOpponentController.health > 0 && damagedOpponentController.health - weaponController.Attacks[weaponController.currentAttack].weapon_damage * bodyColliderScript.damageMultiplayer <= 0 && weaponController.Controller.CharacterSync)
// 							{
// 								// if (bodyColliderScript.bodyPart != BodyPartCollider.BodyPart.Head)
// 								// 	weaponController.Controller.CharacterSync.AddScore(PlayerPrefs.GetInt("NormKill"), "bullet");
// 								// else weaponController.Controller.CharacterSync.AddScore(PlayerPrefs.GetInt("Headshot"), "headshot");
// 							}
// #endif
							if (damagedOpponentController.CharacterSync && weaponController.Controller.CharacterSync)
								damagedOpponentController.CharacterSync.CreateHitMark(weaponController.Controller.CharacterSync.photonView.ViewID);
#endif
							damagedOpponentController.Damage((int) (weaponController.Attacks[weaponController.currentAttack].weapon_damage * bodyColliderScript.damageMultiplayer), bodyColliderScript.bodyPart != BodyPartCollider.BodyPart.Head ? "bullet" : "headshot"
#if USK_MULTIPLAYER
								, PhotonNetwork.InRoom ?  new Helper.ActorID{actorID = weaponController.Controller.CharacterSync.photonView.OwnerActorNr, type = "player"} : null
#endif								
								);
						}
					}
					
					bloodHoles = bodyColliderScript.controller.BloodHoles;
				}

				if (weaponController.WeaponManager.bloodProjector)
					CreateBlood(weaponController.WeaponManager.bloodProjector, hitPoint - hitNormal * 0.15f, Quaternion.LookRotation(hitPoint - hitNormal * 2), targetGameObject.transform, bloodHoles);
				
				InstantiateAdditionalBloodEffects(weaponController.Controller.additionalHitEffects, hitPoint, Quaternion.LookRotation(hitNormal));
			}

			if (targetGameObject.GetComponent<FlyingProjectile>())
			{
				if (!targetGameObject.GetComponent<FlyingProjectile>().isTracer && !targetGameObject.GetComponent<FlyingProjectile>().isBullet)
				{
					targetGameObject.GetComponent<FlyingProjectile>().Explosion();
				}
			}

			if (weaponController.Attacks[weaponController.currentAttack].weapon_damage >= 0)
			{
				if (targetGameObject.GetComponent<Rigidbody>())
				{
					targetGameObject.GetComponent<Rigidbody>().AddForceAtPosition(weaponController.attackDirection * 800, hitPoint);
				}

				if (targetGameObject.GetComponent<Surface>())
				{
					var surface = targetGameObject.GetComponent<Surface>();

					if (!surface.Material) return;

					var _point = hitPoint + hitNormal * Random.Range(0.01f, 0.04f);
					var emptyPlace = true;

					foreach (var hit in weaponController.bulletHitsDecals)
					{
						if (hit)
						{
							if (Vector3.Distance(hit.transform.position, _point) < 0.13f)
								emptyPlace = false;
						}
					}

					if (emptyPlace)
					{

						if (surface.Sparks)
						{
							weaponController.bulletHitsDecals.Add(CreateSparks(surface, _point, hitRotation, targetGameObject.transform));
						}

						if (surface.Hit)
						{
							CreateHitPoint(surface, _point, Quaternion.FromToRotation(-Vector3.forward, hitNormal), targetGameObject.transform);
						}
					}

					weaponController.bulletHitsDecals.RemoveAll(item => item == null);

					var angle = Vector3.Angle(weaponController.attackDirection, -hitNormal);

					if (angle > 70)
					{
						var ricochetChance = (float) surface.Material.ricochetChance;
						ricochetChance /= 100;

						if (Random.value > 1 - ricochetChance)
						{
							if (isRicochet)
								BulletRicochet(hitPoint, hitNormal, weaponController, surface);
						}
						else
						{
							if (isPenetration)
								BulletPenetration(surface, weaponController, hitPoint);
						}
					}
					else
					{
						if (isPenetration)
							BulletPenetration(surface, weaponController, hitPoint);
					}
				}
			}

#if USK_DESTROYIT_INTEGRATION
			if (weaponController.Attacks[weaponController.currentAttack].weapon_damage >= 0)
			{
				var destObjs = targetGameObject.GetComponentsInParent<DestroyIt.Destructible>(false);
				foreach (DestroyIt.Destructible destObj in destObjs)
				{
					if (!destObj.isActiveAndEnabled && !destObj.isTerrainTree) continue;
					DestroyIt.ImpactDamage bulletDamage = new DestroyIt.ImpactDamage {DamageAmount = weaponController.Attacks[weaponController.currentAttack].weapon_damage, AdditionalForce = 25, AdditionalForcePosition = hitPoint, AdditionalForceRadius = 0.5f};
					destObj.ApplyDamage(bulletDamage);
					break;
				}

				// Check for Chip-Away Debris
				DestroyIt.ChipAwayDebris chipAwayDebris = targetGameObject.GetComponent<DestroyIt.ChipAwayDebris>();
				if (chipAwayDebris != null)
					chipAwayDebris.BreakOff(-hitNormal * (-1.5f * 800), hitPoint);

				DestroyIt.HitEffects hitEffects = targetGameObject.GetComponentInParent<DestroyIt.HitEffects>();
				if (hitEffects != null && hitEffects.effects.Count > 0)
					hitEffects.PlayEffect(DestroyIt.HitBy.Bullet, hitPoint, hitNormal);
			}
			else
			{
				var destObjs = targetGameObject.GetComponentsInParent<DestroyIt.Destructible>(false);
				DestroyItOnRepair(destObjs.ToList(), weaponController.Attacks[weaponController.currentAttack].weapon_damage * -1);
			}
#endif
			
#if USK_EMERALDAI_INTEGRATION
			if (targetGameObject.GetComponent<EmeraldAI.EmeraldAISystem>())
			{
				AIHelper.DamageEmeraldAI(weaponController.Attacks[weaponController.currentAttack].weapon_damage, targetGameObject, weaponController.Controller.transform);
			}
#endif
			
		}

		static void BulletRicochet(Vector3 hitPoint, Vector3 hitNormal, WeaponController weaponController, Surface surface)
		{
			if(surface.Material.ricochetChance <= 0) return;
			
			var ray = new Ray(hitPoint, Vector3.Reflect(weaponController.attackDirection.normalized, hitNormal));

			var secondTargetPoint = Vector3.zero;
			
			if (Physics.Raycast(ray, out var hit, 10000, Helper.LayerMask()))
			{
				CheckBulletRaycast(hit.transform.gameObject, hit.point, hit.normal, weaponController, false, false);
				secondTargetPoint = hit.point;
			}
			else
			{
				secondTargetPoint = hitPoint + ray.direction * 1000;
			}
			
			if (weaponController.Attacks[weaponController.currentAttack].shootingMethod == ShootingMethod.Raycast)
			{
				if (weaponController.Attacks[weaponController.currentAttack].bulletTrail)
					CreateTrail(hitPoint, secondTargetPoint, weaponController.Attacks[weaponController.currentAttack].bulletTrail);
			}
			else
			{
				InstantiateBullet(weaponController, hitPoint, secondTargetPoint);
			}
		}

		static void BulletPenetration(Surface surface, WeaponController weaponController, Vector3 hitPoint)
		{
			if(surface.Material.penetrationWidth <= 0) return;
			
			var penetrationWidth = surface.Material.penetrationWidth * weaponController.Attacks[weaponController.currentAttack].penetrationMultiplier;
			var ray = new Ray(hitPoint + weaponController.attackDirection.normalized * (penetrationWidth * 1.1f), -weaponController.attackDirection.normalized);

			if (Physics.Raycast(ray, out var hit, penetrationWidth, Helper.LayerMask()))
			{
				CheckBulletRaycast(hit.transform.gameObject, hit.point, hit.normal, weaponController, false, false);

				ray = new Ray(hit.point, weaponController.attackDirection.normalized);
				
				var secondStartPoint = hit.point;
				var secondTargetPoint = Vector3.zero;
				
				if (Physics.Raycast(ray, out hit, 10000, Helper.LayerMask()))
				{
					CheckBulletRaycast(hit.transform.gameObject, hit.point, hit.normal, weaponController, false, false);
					secondTargetPoint = hit.point;
				}
				else
				{
					secondTargetPoint = secondStartPoint + weaponController.attackDirection.normalized * 1000;
				}

				if (weaponController.Attacks[weaponController.currentAttack].shootingMethod == ShootingMethod.Raycast)
				{
					if (weaponController.Attacks[weaponController.currentAttack].bulletTrail)
						CreateTrail(secondStartPoint, secondTargetPoint, weaponController.Attacks[weaponController.currentAttack].bulletTrail);
				}
				else
				{
					InstantiateBullet(weaponController, secondStartPoint, secondTargetPoint);
				}
			}
		}

		public static void ShowGrenadeTrajectory(bool enable, Transform startPoint, LineRenderer lineRenderer, Controller controller, WeaponController weaponController, Transform characterTransform)
        {
            if (enable)
            {
                if(!lineRenderer.enabled)
                    lineRenderer.enabled = true;
                
                lineRenderer.startColor = new Color(1, 1, 1, Mathf.Lerp(lineRenderer.startColor.a, 0, 5 * Time.deltaTime));
                lineRenderer.endColor = new Color(1, 1, 1, Mathf.Lerp(lineRenderer.endColor.a, 1, 5 * Time.deltaTime));
                
                var lastPoint = 0;
                var lastPos = Vector3.zero;
                for (int i = 0; i < 50; i++)
                {
                    var currentTime = 0.05f * i;
                    var gravity = weaponController.Attacks[weaponController.currentAttack].applyGravity ? Physics.gravity.y : 0;
                    var trajectoryPosition = Vector3.zero;
                    
                    if (controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && controller.emulateTDModeLikeTP)
                    {
                        if (!controller.CameraParameters.lockCamera)
                        {
                            trajectoryPosition = ProjectileHelper.ComputePositionAtTimeAhead(startPoint.position, controller.thisCamera.transform.TransformDirection(Vector3.up * weaponController.Attacks[weaponController.currentAttack].flightSpeed), gravity, currentTime);
                        }
                        else
                        {
	                        var starPoint = weaponController.Attacks[weaponController.currentAttack].AttackType != TypeOfAttack.Grenade ? weaponController.Attacks[weaponController.currentAttack].AttackSpawnPoint.position : weaponController.transform.position;
                           
	                        if (weaponController.Controller.CameraParameters.lookAtCursor)
                            {
	                            trajectoryPosition = ProjectileHelper.ComputePositionAtTimeAhead(startPoint.position, ProjectileHelper.ComputeVelocityToHitTargetAtTime(starPoint, weaponController.Controller.CameraController.BodyLookAt.position, gravity, 10 / weaponController.Attacks[weaponController.currentAttack].flightSpeed), gravity, currentTime);
                            }
                            else
                            {
	                            var tempPos = weaponController.Controller.CameraController.BodyLookAt.position;
	                            tempPos.y = weaponController.Controller.transform.position.y;

	                            RaycastHit hit;
	                            if (Physics.Raycast(weaponController.Controller.CameraController.BodyLookAt.position, Vector3.down, out hit))
		                            tempPos = hit.point;

	                            trajectoryPosition = ProjectileHelper.ComputePositionAtTimeAhead(startPoint.position, ProjectileHelper.ComputeVelocityToHitTargetAtTime(starPoint, tempPos, gravity, 10 / weaponController.Attacks[weaponController.currentAttack].flightSpeed), gravity, currentTime);
                            }
                        }
                    }
                    else
                    {
	                    trajectoryPosition = ProjectileHelper.ComputePositionAtTimeAhead(startPoint.position, controller.thisCamera.transform.TransformDirection(Vector3.forward * weaponController.Attacks[weaponController.currentAttack].flightSpeed), gravity, currentTime);
                    }

                    lineRenderer.SetPosition(i, trajectoryPosition);

                    if (Physics.OverlapSphere(trajectoryPosition, 0.1f, Helper.LayerMask()).Length > 0)
                    {
                        lastPoint = i;
                        lastPos = trajectoryPosition;
                        break;
                    }
                }

                if (lastPoint > 0)
                {
                    for (int i = lastPoint; i < 50; i++)
                    {
                        lineRenderer.SetPosition(i, lastPos);
                    }
                }
            }
            else
            {
                if (lineRenderer)
                {
                    lineRenderer.startColor = new Color(1, 1, 1, Mathf.Lerp(lineRenderer.startColor.a, 0, 7 * Time.deltaTime));
                    lineRenderer.endColor = new Color(1, 1, 1, Mathf.Lerp(lineRenderer.endColor.a, 0, 7 * Time.deltaTime));

                    if (lineRenderer.endColor == new Color(1, 1, 1, 0) && lineRenderer.enabled)
                        lineRenderer.enabled = false;
                }
            }
        }

		public static PickupItem AddPickupItemScript(GameObject weapon, InventoryManager inventoryManager, int inventorySlot)
		{
			if (!weapon.GetComponent<PickupItem>())
			{
				var pickUpScript = weapon.AddComponent<PickupItem>();
				var weaponController = weapon.GetComponent<WeaponController>();
				
				pickUpScript.type = PickupItem.TypeOfPickUp.Weapon;
				pickUpScript.distance = 10;
				pickUpScript.rotationSpeed = 0;

				if (weaponController.blipImage)
				{
					pickUpScript.blipTexture = weaponController.blipImage;
				}
                
				pickUpScript.inventorySlot = inventorySlot;
				pickUpScript.method = PickupItem.PickUpMethod.Both;

				if (weaponController.Controller)
				{
					if (!weaponController.Controller.isRemoteCharacter)
					{
						if (pickUpScript.pickUpId == null)
						{
							pickUpScript.pickUpId = Helper.GenerateRandomString(20);
							inventoryManager.DropIdMultiplayer = pickUpScript.pickUpId;
						}
					}
					else
					{
						if (pickUpScript.pickUpId == null)
						{
							pickUpScript.pickUpId = inventoryManager.DropIdMultiplayer;
						}
					}
				}

				pickUpScript.enabled = false;

				return pickUpScript;
			}

			return weapon.GetComponent<PickupItem>();
		}
		
#if USK_DESTROYIT_INTEGRATION
		public static void DestroyItOnRepair(Vector3 position, float meleeRadius, float value)
		{
			Collider[] objectsInRange = Physics.OverlapSphere(position, meleeRadius);
			List<DestroyIt.Destructible> repairedObjects = new List<DestroyIt.Destructible>(); // Keep track of what objects have been repaired so we don't repair multiple times per collider.
			bool hasPlayedRepairEffect = false;

			foreach (Collider col in objectsInRange)
			{
				if (col is TerrainCollider) continue;

				if (col.isTrigger) continue;

				if (col is CharacterController && col.tag == "Player") continue;

				Repair(col.gameObject, value, ref repairedObjects);
			}
		}

		public static void DestroyItOnRepair(List<DestroyIt.Destructible> targetGameObjects, float value)
		{
			var array = new List<DestroyIt.Destructible>();

			foreach (DestroyIt.Destructible obj in targetGameObjects)
			{
				Repair(obj.gameObject, value, ref array);
			}
		}

		static void Repair(GameObject gameObject, float value, ref List<DestroyIt.Destructible> repairedObjects)
		{
			DestroyIt.Destructible destObj = gameObject.GetComponentInParent<DestroyIt.Destructible>();
			if (destObj != null && !repairedObjects.Contains(destObj) && destObj.CurrentHitPoints < destObj.TotalHitPoints && destObj.canBeRepaired)
			{
				repairedObjects.Add(destObj);
				destObj.RepairDamage(value);
			}
		}

		public static void DestroyItMeleeDamage(Transform weapon, float meleeRadius, float value)
		{
			Collider[] objectsInRange = Physics.OverlapSphere(weapon.position, meleeRadius);
            List<Destructible> damagedObjects = new List<Destructible>(); // Keep track of what objects have been damaged so we don't do damage multiple times per collider.
            bool hasPlayedHitEffect = false;

            foreach (Collider col in objectsInRange)
            {
                if (col is TerrainCollider || col.isTrigger || col is CharacterController && col.tag == "Player") continue;

                if (!hasPlayedHitEffect)
                {
                    HitEffects hitEffects = col.gameObject.GetComponentInParent<HitEffects>();
                    if (hitEffects != null && hitEffects.effects.Count > 0)
                        hitEffects.PlayEffect(HitBy.Axe, weapon.position, weapon.forward * -1);

                    hasPlayedHitEffect = true;
                }

                Destructible[] destObjs = col.gameObject.GetComponentsInParent<Destructible>(false);
                foreach (Destructible destObj in destObjs)
                {
                    if (damagedObjects.Contains(destObj)) continue;
                    if (!destObj.isActiveAndEnabled && !destObj.isTerrainTree) continue;

                    damagedObjects.Add(destObj);
                    ImpactDamage meleeImpact = new ImpactDamage() { DamageAmount = value, AdditionalForce = 150,
                        AdditionalForcePosition = weapon.position, AdditionalForceRadius = 2 };
                    destObj.ApplyDamage(meleeImpact);
                }
            }
		}
#endif

		public static void SmoothWeaponMovement(WeaponController weaponController)
		{
			if (!weaponController.isMultiplayerWeapon && weaponController.setHandsPositionsAim && !weaponController.Controller.AdjustmentScene && !weaponController.firstTake && weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson &&
			    weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Idle") && weaponController.Controller.anim.GetBool("HasWeaponTaken") && weaponController.Controller.anim.GetBool("CanWalkWithWeapon"))
            {
	            SetHand(ref weaponController.RightHandPositions, weaponController.IkObjects.RightObject, ref weaponController.firstTake);
	            SetHand(ref weaponController.LeftHandPositions, weaponController.IkObjects.LeftObject, ref weaponController.firstTake);
            }
            else if (weaponController.canUseSmoothWeaponRotation && !weaponController.isMultiplayerWeapon && weaponController.Controller.smoothHandsMovement && !weaponController.Controller.AdjustmentScene && weaponController.firstTake && weaponController.Controller.anim.GetBool("HasWeaponTaken") && !weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Take Weapon") &&
                      weaponController.setHandsPositionsAim && !weaponController.DetectObject /*&& !weaponController.isAimEnabled*/ && weaponController.setHandsPositionsObjectDetection && weaponController.Controller.TypeOfCamera == CharacterHelper.CameraType.FirstPerson &&
                     (weaponController.isShotgun && !weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") || !weaponController.isShotgun))
			{

				var rotationLimit = 6 + 0.3f * weaponController.weaponWeight;
	            var positionLimit = 0.1f;
	            var speed = 10 - 0.4f * weaponController.weaponWeight;

	            CalculateHandPosition(ref weaponController.RightHandPositions, weaponController.IkObjects.RightObject, weaponController, rotationLimit, positionLimit, speed, ref weaponController.placeHandsToStartPositionTimeout, weaponController.Controller.smoothHandsMovementValue);

	            if (weaponController.numberOfUsedHands == 1)//weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee || weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
	            {
		            CalculateHandPosition(ref weaponController.LeftHandPositions, weaponController.IkObjects.LeftObject, weaponController, rotationLimit, positionLimit, speed / 1.5f, ref weaponController.placeHandsToStartPositionTimeout, weaponController.Controller.smoothHandsMovementValue);
	            }
            }
		}

		public static void SmoothHandMovement(IKHelper.HandsPositions handsPositions, Transform ikHandle)
		{
			ikHandle.localEulerAngles = new Vector3(handsPositions.currentHandRotation.x, handsPositions.currentHandRotation.y, ikHandle.localEulerAngles.z);
			ikHandle.localPosition = new Vector3(ikHandle.localPosition.x, handsPositions.currentHandPosition.x,  handsPositions.currentHandPosition.y);
		}

		private static void SetHand(ref IKHelper.HandsPositions handsPositions, Transform ikHandle, ref bool firstTake)
		{
			handsPositions.startHandRotation = new Vector2(ikHandle.localEulerAngles.x, ikHandle.localEulerAngles.y);
			handsPositions.startHandPosition = new Vector2(ikHandle.localPosition.y, ikHandle.localPosition.z);

			if (handsPositions.startHandRotation.x > 180)
				handsPositions.startHandRotation.x -= 360;

			if (handsPositions.startHandRotation.y > 180)
				handsPositions.startHandRotation.y -= 360;

			handsPositions.currentHandRotation = handsPositions.startHandRotation;
			handsPositions.currentHandPosition = handsPositions.startHandPosition;

			firstTake = true;
		}

		static void CalculateHandPosition(ref IKHelper.HandsPositions handsPositions, Transform ikHandle, WeaponController weaponController, float rotationLimit, float positionLimit, float speed, ref float timer, float setValue)
		{
			var mouseDelta = weaponController.Controller.CameraController.mouseDelta;//new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());
			setValue -= 1;
			
			if (!weaponController.Controller.inventoryManager.inventoryIsOpened && (weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Idle") || weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Walk") ||
			    weaponController.Controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack")))
			{
				var mouseSpeed = mouseDelta.magnitude / Time.deltaTime;
				mouseSpeed = Math.Abs(mouseSpeed);

				if (mouseDelta.magnitude > 0)
				{
					timer = 0;
					handsPositions.desiredHandRotation = handsPositions.startHandRotation + mouseDelta.normalized / (200 - setValue * 10) * mouseSpeed;
					// handsPositions.desiredHandRotation.y = handsPositions.startHandRotation.y + weaponController.Controller.CameraController.mouseDelta.y * mouseSpeed;

					handsPositions.desiredHandPosition = handsPositions.startHandPosition + mouseDelta.normalized / (20000 - setValue * 1000) * mouseSpeed;
					// handsPositions.desiredHandPosition.y = handsPositions.startHandPosition.y + weaponController.Controller.CameraController.mouseDelta.y / 500;
				}
				else
				{
					timer += Time.deltaTime;

					if (timer > Time.deltaTime * 5)
					{
						handsPositions.desiredHandPosition = handsPositions.startHandPosition;
						handsPositions.desiredHandRotation = handsPositions.startHandRotation;

					}
				}
			}
			else
			{				
				handsPositions.desiredHandRotation = handsPositions.startHandRotation;
				// handsPositions.desiredHandRotation.y = handsPositions.startHandRotation.y;

				handsPositions.desiredHandPosition = handsPositions.startHandPosition;
				// handsPositions.desiredHandPosition.y = handsPositions.startHandPosition.y;
			}

			if (handsPositions.desiredHandRotation.x > 180)
				handsPositions.desiredHandRotation.x -= 360;

			if (handsPositions.desiredHandRotation.y > 180)
				handsPositions.desiredHandRotation.y -= 360;

			
			if (handsPositions.desiredHandRotation.x > handsPositions.startHandRotation.x + rotationLimit) handsPositions.desiredHandRotation.x = handsPositions.startHandRotation.x + rotationLimit;
			else if (handsPositions.desiredHandRotation.x < handsPositions.startHandRotation.x - rotationLimit) handsPositions.desiredHandRotation.x = handsPositions.startHandRotation.x - rotationLimit;

			if (handsPositions.desiredHandRotation.y > handsPositions.startHandRotation.y + rotationLimit) handsPositions.desiredHandRotation.y = handsPositions.startHandRotation.y + rotationLimit;
			else if (handsPositions.desiredHandRotation.y < handsPositions.startHandRotation.y - rotationLimit) handsPositions.desiredHandRotation.y = handsPositions.startHandRotation.y - rotationLimit;

			handsPositions.currentHandRotation.x = Mathf.Lerp(handsPositions.currentHandRotation.x, handsPositions.desiredHandRotation.x, speed * Time.deltaTime);
			handsPositions.currentHandRotation.y = Mathf.Lerp(handsPositions.currentHandRotation.y, handsPositions.desiredHandRotation.y, speed * Time.deltaTime);

			if (handsPositions.desiredHandPosition.x > handsPositions.startHandPosition.x + positionLimit) handsPositions.desiredHandPosition.x = handsPositions.startHandPosition.x + positionLimit;
			else if (handsPositions.desiredHandPosition.x < handsPositions.startHandPosition.x - positionLimit) handsPositions.desiredHandPosition.x = handsPositions.startHandPosition.x - positionLimit;

			if (handsPositions.desiredHandPosition.y > handsPositions.startHandPosition.y + positionLimit) handsPositions.desiredHandPosition.y = handsPositions.startHandPosition.y + positionLimit;
			else if (handsPositions.desiredHandPosition.y < handsPositions.startHandPosition.y - positionLimit) handsPositions.desiredHandPosition.y = handsPositions.startHandPosition.y - positionLimit;

			handsPositions.currentHandPosition.x = Mathf.Lerp(handsPositions.currentHandPosition.x, handsPositions.desiredHandPosition.x, speed / 2 * Time.deltaTime);
			handsPositions.currentHandPosition.y = Mathf.Lerp(handsPositions.currentHandPosition.y, handsPositions.desiredHandPosition.y, speed / 2 * Time.deltaTime);
			
			if(!float.IsNaN(handsPositions.currentHandRotation.x) && !float.IsNaN(handsPositions.currentHandRotation.y) && !float.IsNaN(ikHandle.localEulerAngles.z))
				ikHandle.localEulerAngles = new Vector3(handsPositions.currentHandRotation.x, handsPositions.currentHandRotation.y, ikHandle.localEulerAngles.z);
			
			if(!float.IsNaN(handsPositions.currentHandPosition.x) && !float.IsNaN(handsPositions.currentHandPosition.y) && !float.IsNaN(ikHandle.localPosition.x))
				ikHandle.localPosition = new Vector3(ikHandle.localPosition.x, handsPositions.currentHandPosition.x, handsPositions.currentHandPosition.y);
		}

		public static void ReplaceRunAnimation(Controller controller, AnimationClip animation, float speedMultiplier, bool resetAnimationSpeed, float aimAnimationMultiplier)
		{
			controller.ClipOverrides["_WeaponRun"] = animation;
			controller.newController.ApplyOverrides(controller.ClipOverrides);
			controller.anim.SetFloat("RunAnimationMultiplier", speedMultiplier);

			if (resetAnimationSpeed)
			{
				controller.anim.SetFloat("AimAnimationSpeed", aimAnimationMultiplier);

				if (aimAnimationMultiplier == 0)
					controller.anim.Play("Idle", 1, 0);
			}

			// if(controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Run"))
			// 	controller.anim.CrossFade("Run", 0.1f, 1);
		}
	}
}

