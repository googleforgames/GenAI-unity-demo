using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GercStudio.USK.Scripts
{
    public class SaveManager : MonoBehaviour
    {
        public WeaponsPool weaponsPool;
        public Controller characterController;
        public AIArea aiArea;

        public static SaveManager Instance;

        [Serializable]
        public class CharacterSaveData
        {
            public float health = -1;
            public int selectedSlot;
            public int selectedWeaponInSlot;
            public List<WeaponSaveData> weaponsInInventory = new List<WeaponSaveData>();
            public List<PickUpItem> healthKits = new List<PickUpItem>();
        }
        
        [Serializable]
        public class AISaveData
        {
            public string id;
            public float healthValue;
        }
        
        [Serializable]
        public class WeaponSaveData
        {
            public string id;
            public string name;

            public int inventorySlot;
            public int currentAttack;
            public List<WeaponsHelper.Attack> weaponAttackParameters = new List<WeaponsHelper.Attack>();
            public List<PickUpItem> ammoKits = new List<PickUpItem>();
        }

        [Serializable]
        public class PickUpItem
        {
            public int addedValue;
            public string imageName;
			
            public string itemId;
            public string ammoType;
        }

        [Serializable]
        public class DroppedWeapons
        {
            public WeaponSaveData weaponSaveData;
            public Vector3 position;
        }

        [Serializable]
        public class SceneSaveData
        {
            public Vector3 characterPosition;

            public List<DroppedWeapons> droppedWeapons = new List<DroppedWeapons>();
            public List<string> objectsToDeleteFromScene = new List<string>();
            public List<AISaveData> aiSaveData = new List<AISaveData>();
        }

        public CharacterSaveData characterSaveData;
        public SceneSaveData sceneSaveData;
        
        [Serializable]
        public class CustomSaveData
        {
            //Here you can add any values you need save.
        }
        
        public CustomSaveData customSaveData;

        public bool saveCharacterHealth = true;
        public bool saveInventory = true;
        public bool saveWeaponsAmmoAmount = true;
        public bool saveDroppedWeapons = true;
        public bool deletePickedUpObjects = true;
        public bool saveAIHealth = true;
        public bool saveCharacterPosition;
        public bool autoSave;

        public int autoSaveTime;
        private float autoSaveTimer;
        
        public const string CharacterDataFileName = "CharacterData";
        private const string AdditionalDataFileName = "SceneData";
        
        private void Start()
        {
            Instance = this;

            if(!weaponsPool) weaponsPool = Resources.Load("Weapons Pool", typeof(WeaponsPool)) as WeaponsPool;
            
            aiArea = FindObjectOfType<AIArea>();
            
            LoadDataFromFile(CharacterDataFileName, ref characterSaveData);
            LoadDataFromFile(AdditionalDataFileName + "-" + SceneManager.GetActiveScene().name, ref sceneSaveData);
       
            StartCoroutine(LoadDataDelay());
        }

        IEnumerator LoadDataDelay()
        {
            yield return new WaitForEndOfFrame();
            
            if (!characterController)
                characterController = FindObjectOfType<Controller>();

            if (!characterController) yield return null;

            LoadData();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                SaveData();
            }
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            
            if (Input.GetKeyDown(KeyCode.J))
            {
                DeleteAllSavedData();
            }

            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;

                if (autoSaveTimer > autoSaveTime * 60)
                {
                    autoSaveTimer = 0;
                    
                    SaveData();
                }
            }
        }

        public void SaveData()
        {
            var allWeapons = new List<WeaponSaveData>();

            for (var i = 0; i < 8; i++)
            {
                foreach (var weaponSlot in characterController.inventoryManager.slots[i].weaponSlotInGame)
                {
                    if(!weaponSlot.weapon) continue;
                    
                    var weaponController = weaponSlot.weapon.GetComponent<WeaponController>();
                    
                    var weaponAttackParameters = weaponController.Attacks.Select(attack => new WeaponsHelper.Attack {curAmmo = attack.curAmmo, inventoryAmmo = attack.inventoryAmmo}).ToList();
                    
                    var ammoKits = new List<PickUpItem>();
                    
                    if(weaponSlot.WeaponAmmoKits != null && weaponSlot.WeaponAmmoKits.Count > 0)
                        ammoKits = weaponSlot.WeaponAmmoKits.Select(ammoKit => new PickUpItem {addedValue = ammoKit.AddedValue, ammoType = ammoKit.ammoType, itemId = ammoKit.PickUpId, imageName = ammoKit.Image.name}).ToList();

                    var weaponSaveData = new WeaponSaveData { id = weaponController.weaponID, name = weaponController.gameObject.name, ammoKits = ammoKits, weaponAttackParameters = weaponAttackParameters, currentAttack = weaponController.currentAttack, inventorySlot = i};
                    
                    allWeapons.Add(weaponSaveData);
                }
            }
            
            var healthKits = characterController.inventoryManager.HealthKits.Select(healthKit => new PickUpItem {addedValue = healthKit.AddedValue, itemId = healthKit.PickUpId, imageName = healthKit.Image.name}).ToList();
            characterSaveData = new CharacterSaveData {health = characterController.health, healthKits = healthKits, weaponsInInventory = allWeapons, selectedSlot = characterController.inventoryManager.currentSlot, selectedWeaponInSlot = characterController.inventoryManager.slots[characterController.inventoryManager.currentSlot].currentWeaponInSlot};
            
            SaveDataToFile(CharacterDataFileName, characterSaveData);

            if (aiArea)
            {
                foreach (var aiController in aiArea.allEnemiesInZone.Where(aiController => !sceneSaveData.aiSaveData.Exists(ai => ai.id == aiController.enemyID)))
                {
                    sceneSaveData.aiSaveData.Add(new AISaveData {id = aiController.enemyID, healthValue = aiController.health});
                }
            }

            SaveDataToFile(AdditionalDataFileName + "-" + SceneManager.GetActiveScene().name, sceneSaveData);
            
            //Save custom parameters
            SaveDataToFile("CustomData", customSaveData);
        }

        public void LoadData()
        {
           LoadCharacterData();
           
           LoadPickUpObjectsData();
           LoadDroppedWeaponsData();

           LoadAIData();

           //Load and apply your custom parameters
           LoadDataFromFile("CustomData", ref customSaveData);
           ApplyCustomData();
        }

        void ApplyCustomData()
        {
            // Apply the data here
        }

        public void LoadAIData()
        {
            if(!aiArea || !saveAIHealth) return;

            var enemiesToDestroy = new List<AIController>();
            
            foreach (var aiController in aiArea.allEnemiesInZone)
            {
                if (sceneSaveData.aiSaveData.Exists(ai => ai.id == aiController.enemyID))
                {
                    var aiData = sceneSaveData.aiSaveData.Find(ai => ai.id == aiController.enemyID);

                    if (aiData.healthValue <= 0)
                    {
                        enemiesToDestroy.Add(aiController);
                    }
                    else
                    {
                        aiController.health = aiData.healthValue;
                    }
                }
            }

            foreach (var enemy in enemiesToDestroy)
            {
                aiArea.allEnemiesInZone.Remove(enemy);
                Destroy(enemy.gameObject);
            }
        }

        public void LoadDroppedWeaponsData()
        {
            if(!saveDroppedWeapons) return;
            
            foreach (var droppedWeapon in sceneSaveData.droppedWeapons)
            {
                if (!weaponsPool.weapons.Exists(controller => controller.weaponID == droppedWeapon.weaponSaveData.id))
                {
                    Debug.LogWarning("The save file has the <b><color=blue>(" + droppedWeapon.weaponSaveData.name + ")</color></b> weapon with the <b><color=blue>[" + droppedWeapon.weaponSaveData.id + "]</color></b> id, but this weapon could no be found in the <b><color=green>Weapons Pool</color></b>.");
                    continue;
                }
                var foundWeapon = weaponsPool.weapons.Find(controller => controller.weaponID == droppedWeapon.weaponSaveData.id);

                var instantiatedWeapon = Instantiate(foundWeapon.gameObject).GetComponent<WeaponController>();
                instantiatedWeapon.transform.position = droppedWeapon.position;
                
                var pickupItemScript = WeaponsHelper.AddPickupItemScript(instantiatedWeapon.gameObject, characterController.inventoryManager, droppedWeapon.weaponSaveData.inventorySlot);

                for (var i = 0; i < instantiatedWeapon.Attacks.Count; i++)
                {
                    instantiatedWeapon.Attacks[i].curAmmo = droppedWeapon.weaponSaveData.weaponAttackParameters[i].curAmmo;
                    instantiatedWeapon.Attacks[i].inventoryAmmo = droppedWeapon.weaponSaveData.weaponAttackParameters[i].inventoryAmmo;
                }

                instantiatedWeapon.currentAttack = droppedWeapon.weaponSaveData.currentAttack;

                instantiatedWeapon.enabled = false;
                pickupItemScript.enabled = true;
                pickupItemScript.rotationSpeed = 0;
            }
        }
        
        public void LoadPickUpObjectsData()
        {
            if(!deletePickedUpObjects) return;
            
            var allObjects = FindObjectsOfType<PickupItem>().ToList();

            foreach (var objId in sceneSaveData.objectsToDeleteFromScene)
            {
                if(allObjects.Exists(item => item.pickUpId == objId));
                {
                    var pickupObject = allObjects.Find(item => item.pickUpId == objId);
                    if(pickupObject) Destroy(pickupObject.gameObject);
                }
            }
        }

        public void LoadCharacterData()
        {
            if (saveCharacterPosition && sceneSaveData.characterPosition != Vector3.zero)
            {
                characterController.transform.position = sceneSaveData.characterPosition;
            }
            
            if (saveInventory)
            {
                characterController.inventoryManager.HealthKits = new List<CharacterHelper.Kit>();

                foreach (var healthKit in characterSaveData.healthKits)
                {
                    characterController.inventoryManager.HealthKits.Add(new CharacterHelper.Kit{AddedValue = healthKit.addedValue, PickUpId = healthKit.itemId, Image = Resources.Load(healthKit.imageName, typeof(Texture)) as Texture});
                }
            }

            if(!saveCharacterHealth || characterSaveData.health == -1) return;

            if (characterSaveData.health <= 0) characterSaveData.health = 1;
            characterController.health = characterSaveData.health;
            characterController.UpdateHealthUI();
        }

        public void LoadWeaponsData()
        {
            if(!saveInventory) return;
            
            var inventoryManager = characterController.inventoryManager;
            inventoryManager.ClearAllWeapons();
            
            foreach (var weapon in characterSaveData.weaponsInInventory)
            {
                if (!weaponsPool.weapons.Exists(controller => controller.weaponID == weapon.id))
                {
                    Debug.LogWarning("The save file has the <b><color=blue>(" + weapon.name + ")</color></b> weapon with the <b><color=blue>[" + weapon.id + "]</color></b> id, but this weapon could no be found in the <b><color=green>Weapons Pool</color></b>.");
                    continue;
                }
                
                var foundWeapon = weaponsPool.weapons.Find(controller => controller.weaponID == weapon.id);
                
                var ammoKits = new List<CharacterHelper.Kit>();
                
                if(weapon.ammoKits != null && weapon.ammoKits.Count > 0)
                    ammoKits = weapon.ammoKits.Select(ammoKit => new CharacterHelper.Kit {AddedValue = ammoKit.addedValue, ammoType = ammoKit.ammoType, PickUpId = ammoKit.itemId, Image = Resources.Load(ammoKit.imageName, typeof(Texture)) as Texture}).ToList();

                var instantiatedGameObject = inventoryManager.AddNewWeapon(foundWeapon.gameObject, weapon.inventorySlot, ammoKits);
                
                if (saveWeaponsAmmoAmount)
                {
                    for (var i = 0; i < instantiatedGameObject.Attacks.Count; i++)
                    {
                        instantiatedGameObject.Attacks[i].curAmmo = weapon.weaponAttackParameters[i].curAmmo;
                        instantiatedGameObject.Attacks[i].inventoryAmmo = weapon.weaponAttackParameters[i].inventoryAmmo;
                    }

                    instantiatedGameObject.currentAttack = weapon.currentAttack;
                }
            }

            characterController.inventoryManager.slots[characterSaveData.selectedSlot].currentWeaponInSlot = characterSaveData.selectedWeaponInSlot;
            characterController.inventoryManager.SelectWeaponInInventory(characterSaveData.selectedSlot);
        }

        public bool HasAnyData(string fileName)
        {
            var path = Application.persistentDataPath + "/" + fileName + ".json";
            return System.IO.File.Exists(path);
        }

        public void DeleteAllSavedData()
        {
            DeleteFile(CharacterDataFileName);

            var sceneCount = SceneManager.sceneCountInBuildSettings;     
            var scenes = new string[sceneCount];
            for(var i = 0; i < sceneCount; i++ )
            {
                scenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex( i ) );
            }

            foreach (var scene in scenes)
            {
                DeleteFile(AdditionalDataFileName + "-" + scene);
            }

            characterSaveData = new CharacterSaveData();
            sceneSaveData = new SceneSaveData();
            
            DeleteFile("CustomData");
            customSaveData = new CustomSaveData();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void DeleteFile(string fileName)
        {
#if USK_EASYSAVE_INTEGRATION
            ES3.DeleteKey(fileName);
#endif
            var path = Application.persistentDataPath + "/" + fileName + ".json";
			
            if(!System.IO.File.Exists(path)) return;
            
            System.IO.File.Delete(path);
        }
        
        private void SaveDataToFile<T>(string fileName, T dataToSave)
        {
            var path = Application.persistentDataPath + "/" + fileName + ".json";
            var json = JsonUtility.ToJson(dataToSave);
			
            System.IO.File.WriteAllText(path, json);
			
 #if USK_EASYSAVE_INTEGRATION
			ES3.Save(fileName, dataToSave);
#endif
        }

        private void LoadDataFromFile<T>(string fileName, ref T savedData)
        {
#if !USK_EASYSAVE_INTEGRATION
            var path = Application.persistentDataPath + "/" + fileName + ".json";
			
            if(!System.IO.File.Exists(path)) return;

            var fileContents = System.IO.File.ReadAllText(path);
            savedData = JsonUtility.FromJson<T>(fileContents);

#else
            savedData = ES3.Load(fileName, savedData);
#endif
        }
    }
}
