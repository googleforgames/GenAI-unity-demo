using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    // [RequireComponent(typeof(Rigidbody))]
    public class PickupItem : MonoBehaviour
    {
        public int healthAmount = 20;
        public int ammoAmount = 20;
        [Range(1, 100)] public int distance = 10;
        [Range(1, 8)] public int inventorySlot = 1;

        public float rotationSpeed;

        public bool autoApply;

        public UIHelper.MinimapImage minimapImage;

        [Tooltip("This ammo will be used only for weapons with the same type of ammunition " +
        "- write the same name in a [WeaponController] script" + "\n\n" +
        "If name is empty, this ammo will be used for all weapons")]
        public string ammoName;
        
        public string pickUpId;

        [Tooltip("This image will be displayed in the inventory")]
        public Texture inventoryTexture;
        
        [Tooltip("This image will be displayed on the mini-map (leave blank if you don't need it)")]
        public Texture blipTexture;

        public enum TypeOfPickUp
        {
            Health,
            Ammo,
            Weapon
        }

        public enum PickUpMethod
        {
            Collider,
            Raycast,
            Both
        }

        public PickUpMethod method = PickUpMethod.Both;
        public TypeOfPickUp type;
        
        public AudioClip pickUpAudio;

        public Vector3 colliderSize = Vector3.one;

        public BoxCollider pickUpArea;

        private UIManager uiManager;

        private GameObject target;

        private WeaponController weaponController;
        

        private void OnEnable()
        {
            var rigidbodyComponent = GetComponent<Rigidbody>();
            var colliderComponent = GetComponent<BoxCollider>();

            if (rigidbodyComponent)
            {
                rigidbodyComponent.isKinematic = false;
                rigidbodyComponent.useGravity = true;
                colliderComponent.isTrigger = false;
            }

            if ((method == PickUpMethod.Collider || method == PickUpMethod.Both) && !pickUpArea)
            {
                pickUpArea = gameObject.AddComponent<BoxCollider>();
                pickUpArea.size = colliderSize;
                pickUpArea.isTrigger = true;
            }
            
            if (type == TypeOfPickUp.Weapon && GetComponent<WeaponController>())
            {
                weaponController = GetComponent<WeaponController>();
                
                if(weaponController.blipImage)
                    blipTexture = weaponController.blipImage;
            }
        }

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            
            if (blipTexture && uiManager && uiManager.CharacterUI.mapMask && minimapImage != null && !minimapImage.image)
            {
                minimapImage = UIHelper.CreateNewBlip(uiManager, ref minimapImage.image, blipTexture, Color.white, "Pick-up Item Blip", true);
                uiManager.allMinimapImages.Add(minimapImage);
            }
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void LateUpdate()
        {
            if (minimapImage != null && minimapImage.image && uiManager && uiManager.CharacterUI.mapMask)
            {
                uiManager.SetBlip(transform,"positionOnly", minimapImage);
            }
        }

        public void PickUpObject(GameObject character)
        {
            if (!character.GetComponent<Controller>()) return;

            if (SaveManager.Instance)
            {
                if(!SaveManager.Instance.sceneSaveData.objectsToDeleteFromScene.Exists(itemId => itemId == pickUpId))
                    SaveManager.Instance.sceneSaveData.objectsToDeleteFromScene.Add(pickUpId);
            }

            Destroy(pickUpArea);
            
            if(minimapImage != null && minimapImage.image)
                Destroy(minimapImage.image.gameObject);

            target = character;
            switch (type)
            {
                case TypeOfPickUp.Health:
                {
                    if (!autoApply)
                    {
                        var healthKit = new CharacterHelper.Kit
                        {
                            AddedValue = healthAmount,
                            Image = inventoryTexture
                        };

                        if (pickUpAudio & character.GetComponent<AudioSource>())
                        {
                            character.GetComponent<AudioSource>().PlayOneShot(pickUpAudio);
                        }

                        target.GetComponent<InventoryManager>().HealthKits.Add(healthKit);
                    }
                    else
                    {
                        var controller = target.GetComponent<Controller>();
                        
                        controller.health += healthAmount;

                        controller.ControlHealth("");
                        
#if USK_MULTIPLAYER
                        if(!controller.isRemoteCharacter && controller.CharacterSync)
                            controller.CharacterSync.UseHealthKit();
#endif
                    }
                    
                    Destroy(gameObject);
                    
                    break;
                }
                case TypeOfPickUp.Ammo:
                {
                    var hasWeapon = false;
                    var weaponManager = target.GetComponent<InventoryManager>();
                    
                    for (var i = 0; i < 8; i++)
                    {
                        foreach (var weapon in weaponManager.slots[i].weaponSlotInGame)
                        {
                            if (!weapon.weapon) continue;

                            var weaponController = weapon.weapon.GetComponent<WeaponController>();
                                
                            if (ammoName == "")
                            {
                                if (!autoApply)
                                {
                                    weapon.WeaponAmmoKits.Add(new CharacterHelper.Kit {AddedValue = ammoAmount, Image = inventoryTexture, ammoType = ammoName, PickUpId = pickUpId});
                                    hasWeapon = true;
                                }
                                else
                                {
                                    weaponController.Attacks[weaponController.currentAttack].inventoryAmmo += ammoAmount;

                                    if (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                                        weaponController.Attacks[weaponController.currentAttack].curAmmo = weaponController.Attacks[weaponController.currentAttack].inventoryAmmo;

                                    i = 9;
                                    break;
                                }
                            }
                            else
                            {
                                foreach (var attack in weaponController.Attacks)
                                {
                                    if (attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && attack.ammoName == ammoName)
                                    {
                                        if (!autoApply)
                                        {
                                            weapon.WeaponAmmoKits.Add(new CharacterHelper.Kit {AddedValue = ammoAmount, Image = inventoryTexture, ammoType = ammoName, PickUpId = pickUpId});
                                            hasWeapon = true;
                                        }
                                        else
                                        {
                                            attack.inventoryAmmo += ammoAmount;

                                            if (attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                                                attack.curAmmo = attack.inventoryAmmo;
                                            
                                            i = 9;
                                        }
                                        
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (pickUpAudio & character.GetComponent<AudioSource>())
                    {
                        character.GetComponent<AudioSource>().PlayOneShot(pickUpAudio);
                    }

                    if (!hasWeapon)
                        weaponManager.ReserveAmmo.Add(new CharacterHelper.Kit{AddedValue = ammoAmount, Image = inventoryTexture, ammoType = ammoName, PickUpId = pickUpId});
                    
                    Destroy(gameObject);
                    
                    break;
                }
                case TypeOfPickUp.Weapon:
                {
                    var weaponController = GetComponent<WeaponController>();
                    var weaponManager = target.GetComponent<InventoryManager>();
                    var controller = target.GetComponent<Controller>();
                    enabled = false;
                    
                    if (SaveManager.Instance)
                    {
                        if (SaveManager.Instance.sceneSaveData.droppedWeapons.Exists(itemId => itemId.weaponSaveData.id == weaponController.weaponID))
                            SaveManager.Instance.sceneSaveData.droppedWeapons.Remove(SaveManager.Instance.sceneSaveData.droppedWeapons.Find(itemId => itemId.weaponSaveData.id == weaponController.weaponID));
                    }
                    
                    transform.parent = controller.BodyObjects.RightHand;
                    weaponController.Controller = controller;
                    weaponController.WeaponManager = weaponManager;
                    
                    weaponController.enabled = true;
                    weaponController.PickUpWeapon = false;

                    if (weaponController.Attacks[0].AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                    {
                        var rigidBody = gameObject.AddComponent<Rigidbody>();
                        rigidBody.useGravity = false;
                        rigidBody.isKinematic = true;
                    }

                    switch (inventorySlot)
                    {
                        case 1:
                            PlaceWeaponInInventory(0, weaponManager, controller);
                            break;
                        case 2:
                            PlaceWeaponInInventory(1, weaponManager, controller);
                            break;
                        case 3:
                            PlaceWeaponInInventory(2, weaponManager, controller);
                            break;
                        case 4:
                            PlaceWeaponInInventory(3, weaponManager, controller);
                            break;
                        case 5:
                            PlaceWeaponInInventory(4, weaponManager, controller);
                            break;
                        case 6:
                            PlaceWeaponInInventory(5, weaponManager, controller);
                            break;
                        case 7:
                            PlaceWeaponInInventory(6, weaponManager, controller);
                            break;
                        case 8:
                            PlaceWeaponInInventory(7, weaponManager, controller);
                            break;
                    }

                    if (weaponController.PickUpWeaponAudio & character.GetComponent<AudioSource>())
                    {
                        character.GetComponent<AudioSource>().PlayOneShot(weaponController.PickUpWeaponAudio);
                    }

                    break;
                }
            }
        }

        void PlaceWeaponInInventory(int slotNumber, InventoryManager weaponManager, Controller controller)
        {
            var weapon = new CharacterHelper.Weapon {weapon = gameObject, WeaponAmmoKits = new List<CharacterHelper.Kit>()};

            var removeWeapons = new List<CharacterHelper.Kit>();

            foreach (var kit in weaponManager.ReserveAmmo)
            {
                var weaponController = weapon.weapon.GetComponent<WeaponController>();
                
                if (kit.ammoType == weaponController.Attacks[weaponController.currentAttack].ammoName)
                {
                    weapon.WeaponAmmoKits.Add(kit);
                    removeWeapons.Add(kit);
                }
            }

            if (removeWeapons.Count > 0)
            {
                foreach (var removeWeapon in removeWeapons)
                {
                    if (weaponManager.ReserveAmmo.Contains(removeWeapon))
                        weaponManager.ReserveAmmo.Remove(removeWeapon);
                }
                removeWeapons.Clear();
            }

            weaponManager.slots[slotNumber].weaponSlotInGame.Add(weapon);

            weaponManager.slots[slotNumber].currentWeaponInSlot =
                weaponManager.slots[slotNumber].weaponSlotInGame.Count - 1;

            if (weaponManager.hasAnyWeapon)
            {
                if (autoApply)
                    weaponManager.ActivateWeapon(slotNumber, false, false);
                else
                {
                    gameObject.GetComponent<WeaponController>().Controller = controller;
                    gameObject.SetActive(false);
                }
            }
            else
            {
                weaponManager.currentSlot = slotNumber;
                weaponManager.ActivateWeapon(slotNumber, false, false);
            }
            
            if(autoApply)
                weaponManager.SelectWeaponInInventory(slotNumber);

            switch (controller.TypeOfCamera)
            {
                case CharacterHelper.CameraType.ThirdPerson:
                case CharacterHelper.CameraType.TopDown:
                    controller.currentAnimatorLayer = 2;
                    break;
                case CharacterHelper.CameraType.FirstPerson:
                    controller.currentAnimatorLayer = 1;
                    break;
            }
        }
        
#if UNITY_EDITOR
        
        void OnDrawGizmosSelected()
        {
            if(!enabled)
                return;
            
            Handles.zTest = CompareFunction.Greater;

            switch (method)
            {
                case PickUpMethod.Collider:
                    Handles.matrix = transform.localToWorldMatrix;
                    Handles.color = new Color32(0, 100, 200, 50);
                    Handles.DrawWireCube(Vector3.zero, colliderSize);
                    break;
                
                case PickUpMethod.Raycast:
                    Handles.color = new Color32(255, 153, 0, 10);
                    Handles.DrawSolidDisc(transform.position, Vector3.up, distance);
                    break;
                
                case PickUpMethod.Both:
                    
                    Handles.color = new Color32(0, 100, 200, 50);
                    Handles.matrix = transform.localToWorldMatrix;
                    Handles.DrawWireCube(Vector3.zero, colliderSize);
                    
                    Handles.color = new Color32(255, 153, 0, 10);
                    Handles.matrix = Matrix4x4.identity;
                    Handles.DrawSolidDisc(transform.position, Vector3.up, distance);
                    break;
            }

            Handles.zTest = CompareFunction.Less;

            switch (method)
            {
                case PickUpMethod.Collider:
                    Handles.matrix = transform.localToWorldMatrix;
                    Handles.color = new Color32(0, 100, 200, 255);
                    Handles.DrawWireCube(Vector3.zero, colliderSize);
                    break;
                
                case PickUpMethod.Raycast:
                    Handles.color = new Color32(255, 153, 0, 50);
                    Handles.DrawSolidDisc(transform.position, Vector3.up, distance);
                    break;
                
                case PickUpMethod.Both:
                    Handles.color = new Color32(0, 100, 200, 255);
                    Handles.matrix = transform.localToWorldMatrix;
                    Handles.DrawWireCube(Vector3.zero, colliderSize);
                    
                    Handles.color = new Color32(255, 153, 0, 50);
                    Handles.matrix = Matrix4x4.identity;
                    Handles.DrawSolidDisc(transform.position, Vector3.up, distance);
                    break;
            }
        }
#endif
    }
}




