using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public List<Helper.CharacterInGameManager> Characters = new List<Helper.CharacterInGameManager>();

        // public UIManager UIManager;
        public UIManager currentUIManager;
        
        public int inspectorTab;
        public int CurrentCharacter;
        public int LastCharacter;

        public List<Controller> controllers;
        public List<InventoryManager> inventoryManager;
        public List<CameraController> cameraController;
        
        public bool gameStarted;

        public bool isPause;
        
        private bool isOptions;
        private bool cameraFlyingStep1;
        private bool cameraFlyingStep2;
        private bool switchingCamera;

        private int _quantity;
        private int currentMenuItem;
        private int currentOptionsMenuItem;

        public ProjectSettings projectSettings;

        public Helper.MinimapParameters minimapParameters;

        private Camera cameraForSwitching;
        public Camera defaultCamera;

        private float _timeout;


        public GameObject secondCharacter;
        public GameObject secondCharGO;

        void Awake()
        {
            if (!enabled) return;
            
            // if (UIManager)
            // {
            
            currentUIManager = !FindObjectOfType<UIManager>() ? Instantiate(Resources.Load("UI Manager", typeof(UIManager)) as UIManager) : FindObjectOfType<UIManager>();
            currentUIManager.useMinimap = minimapParameters.useMinimap;
            
            projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
            
            // }
            // else
            // {
            //     Debug.LogError("UI Manager was not be loaded.");
            // }

            currentUIManager.HideAllMultiplayerRoomUI();
            currentUIManager.HideAllMultiplayerLobbyUI();
            currentUIManager.HideAllSinglePlayerMenus();
            currentUIManager.CharacterUI.Inventory.ActivateAll();
            currentUIManager.CharacterUI.DisableAll();
            currentUIManager.CharacterUI.ActivateAll(minimapParameters.useMinimap);

            gameStarted = true;
            
            cameraForSwitching = Helper.NewCamera("Camera for switching", transform, "GameManager");
            Destroy(cameraForSwitching.GetComponent<AudioListener>());

            if(defaultCamera)
                defaultCamera.gameObject.SetActive(false);
            
            cameraForSwitching.gameObject.SetActive(false);

// #if UNITY_EDITOR
//             foreach (var character in Characters.Where(character => character.characterPrefab))
//             {
//                 // if(character.characterPrefab.GetComponent<CharacterSync>())
//                 //     CharacterHelper.RemoveMultiplayerScripts(character.characterPrefab);
//             }
// #endif

            // if (EnemiesSpawnZones.Count == 0) Debug.LogWarning("(Game Manager) There are not any spawn zone for enemies in the scene.");

            // StartCoroutine("SpawnEnemies");

            if (currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.Exit)
                currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.Exit.onClick.AddListener(ExitGame);

            if (currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.Restart)
                currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.Restart.onClick.AddListener(RestartScene);

            if (currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Resume)
                currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Resume.onClick.AddListener(delegate { Pause(true); });
            
            if (currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Exit)
                currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Exit.onClick.AddListener(ExitGame);

            if (currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Options)
                currentUIManager.SinglePlayerGame.SinglePlayerGamePause.Options.onClick.AddListener(OptionsMenu);

            if (currentUIManager.gameOptions.back)
                currentUIManager.gameOptions.back.onClick.AddListener(OptionsMenu);
        }

        private void Start()
        {
            if (Characters.Count > 0)
            {
                for (var i = 0; i < Characters.Count; i++)
                {
                    var character = Characters[i];
                    if (!character.characterPrefab) continue;

                    var position = Vector3.zero;
                    var rotation = Quaternion.identity;

                    if (character.spawnZone)
                    {
                        position = CharacterHelper.GetRandomPointInRectangleZone(character.spawnZone.transform);
                        rotation = Quaternion.Euler(0, character.spawnZone.transform.eulerAngles.y, 0);
                    }

                    var name = character.characterPrefab.name;
                    var instantiateChar = Instantiate(character.characterPrefab, position, rotation);

                    instantiateChar.name = name;

                    var controller = instantiateChar.GetComponent<Controller>();
                    controller.enabled = true;
                    controller.inventoryManager.enabled = true;
                    
                    controllers.Add(controller);
                    inventoryManager.Add(instantiateChar.GetComponent<InventoryManager>());
                    cameraController.Add(instantiateChar.GetComponent<Controller>().CameraController);
                    
                    StartCoroutine(rotationTimeout());

                    var controllerScript = instantiateChar.GetComponent<Controller>();
                    var inventoryManagerScript = instantiateChar.GetComponent<InventoryManager>();
                   
                    controllerScript.enabled = true;
                    inventoryManagerScript.enabled = true;

                    controllerScript.ActiveCharacter = i == 0;
                    
                    if((Application.isMobilePlatform || projectSettings.mobileDebug) && currentUIManager.UIButtonsMainObject)
                        currentUIManager.UIButtonsMainObject.SetActive(true);
                    
                    if (controllerScript.thisCamera.GetComponent<AudioListener>())
                        controllerScript.thisCamera.GetComponent<AudioListener>().enabled = i == 0;
                }
            }
            else
            {
                Debug.LogError("Game Manager hasn't any character.");
                Debug.Break();
            }

            // if (!hasCharacter)
            // {
            //     Debug.LogError("Game Manager hasn't any character.");
            //     Debug.Break();
            // }

            if (minimapParameters.mapExample)
            {
                if (minimapParameters.mapExample.gameObject.hideFlags == HideFlags.None)
                {
                    minimapParameters.mapExample.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    minimapParameters.mapExample.gameObject.SetActive(false);
                }
            }
            
            CurrentCharacter = 0;
        }

        public void SwitchCharacter()
        {
            
            // secondCharGO = Instantiate(secondCharacter,  controllers[CurrentCharacter].transform.position, controllers[CurrentCharacter].transform.rotation);
            //
            // controllers[CurrentCharacter].gameObject.SetActive(false);
            // controllers[CurrentCharacter].CameraController.gameObject.SetActive(false);
            // secondCharGO.GetComponent<Controller>().ActiveCharacter = true;

            // if(switchingCamera || controllers.Count < 2)
            //     return;
            //
            // var newCharacterIndex = CurrentCharacter;
            //
            // newCharacterIndex++;
            // if (newCharacterIndex > Characters.Count - 1)
            //     newCharacterIndex = 0;
            //
            // if (CurrentCharacter != newCharacterIndex)
            // {
            //     switchingCamera = true;
            //     controllers[CurrentCharacter].ActiveCharacter = false;
            //     
            //     if(Application.isMobilePlatform || projectSettings.mobileDebug)
            //         controllers[CurrentCharacter].UIManager.UIButtonsMainObject.SetActive(false);
            //
            //     controllers[CurrentCharacter].anim.SetBool("Attack", false);
            //     controllers[CurrentCharacter].anim.SetBool("Move", false);
            //     controllers[CurrentCharacter].anim.SetFloat("Horizontal", 0);
            //     controllers[CurrentCharacter].anim.SetFloat("Vertical", 0);
            //     
            //     if(controllers[CurrentCharacter].thisCamera.GetComponent<AudioListener>())
            //         controllers[CurrentCharacter].thisCamera.GetComponent<AudioListener>().enabled = false;
            //     
            //     if(controllers[CurrentCharacter].inventoryManager.WeaponController && controllers[CurrentCharacter].inventoryManager.WeaponController.isAimEnabled)
            //         controllers[CurrentCharacter].inventoryManager.WeaponController.Aim(true, false, false);
            //
            //     cameraForSwitching.gameObject.SetActive(true);
            //     cameraForSwitching.transform.SetPositionAndRotation(controllers[CurrentCharacter].thisCamera.transform.position,
            //         controllers[CurrentCharacter].thisCamera.transform.rotation);
            //     
            //     LastCharacter = CurrentCharacter;
            //     CurrentCharacter = newCharacterIndex;
            //     StartCoroutine(FlyCamera());
            // }

        }

        // IEnumerator FlyCamera()
        // {
        //     while (true)
        //     {
        //         var dist = Vector3.Distance(controllers[LastCharacter].transform.position, controllers[CurrentCharacter].transform.position);
        //
        //         var currentHeight = controllers[CurrentCharacter].transform.position + Vector3.up * dist / 3;
        //         var lastHeight = controllers[CurrentCharacter].transform.position + Vector3.up * dist / 3;
        //
        //         var checkTopDown = controllers[LastCharacter].thisCamera.transform.position.y < currentHeight.y &&
        //                            controllers[CurrentCharacter].thisCamera.transform.position.y < lastHeight.y;
        //         
        //         if (dist > 25 && checkTopDown)
        //         {
        //             var currentControllerScript = controllers[CurrentCharacter].GetComponent<Controller>();
        //
        //             if (!cameraFlyingStep1 && !cameraFlyingStep2)
        //             {
        //                 cameraForSwitching.transform.position = Helper.MoveObjInNewPosition(cameraForSwitching.transform.position,
        //                     controllers[LastCharacter].transform.position + Vector3.up * dist / 3, 5 * Time.deltaTime);
        //
        //                 var rot = controllers[LastCharacter].thisCamera.transform.eulerAngles;
        //                 
        //                 cameraForSwitching.transform.rotation = Quaternion.Slerp(cameraForSwitching.transform.rotation, Quaternion.Euler(90, rot.y, rot.z), 0.5f);
        //                 
        //                 
        //                 if(currentControllerScript.UIManager.CharacterUI.crosshairMainObject)
        //                     currentControllerScript.UIManager.CharacterUI.crosshairMainObject.gameObject.SetActive(false);
        //                 
        //
        //                 if(currentUIManager.CharacterUI.PickupImage)
        //                     currentUIManager.CharacterUI.PickupImage.gameObject.SetActive(false);
        //
        //                 cameraFlyingStep1 = Helper.ReachedPositionAndRotation(cameraForSwitching.transform.position, controllers[LastCharacter].transform.position + Vector3.up * dist / 3);
        //             }
        //
        //             if (cameraFlyingStep1 && !cameraFlyingStep2)
        //             {
        //
        //                 cameraForSwitching.transform.position = Helper.MoveObjInNewPosition(cameraForSwitching.transform.position,
        //                     controllers[CurrentCharacter].transform.position + Vector3.up * dist / 3, 3 * Time.deltaTime);
        //
        //
        //                 cameraFlyingStep2 = Helper.ReachedPositionAndRotation(cameraForSwitching.transform.position, controllers[CurrentCharacter].transform.position + Vector3.up * dist / 3);
        //
        //             }
        //
        //             if (cameraFlyingStep2)
        //             {
        //                 cameraForSwitching.transform.position = Helper.MoveObjInNewPosition(cameraForSwitching.transform.position,
        //                     controllers[CurrentCharacter].thisCamera.transform.position, 5 * Time.deltaTime);
        //
        //                 var speed = controllers[CurrentCharacter].TypeOfCamera == CharacterHelper.CameraType.FirstPerson ? 5f : 2.5f;
        //                 
        //                 cameraForSwitching.transform.rotation = Quaternion.Slerp(cameraForSwitching.transform.rotation,
        //                     controllers[CurrentCharacter].thisCamera.transform.rotation, speed * Time.deltaTime);
        //
        //                 if (currentControllerScript.UIManager.CharacterUI.crosshairMainObject && controllers[CurrentCharacter].TypeOfCamera != CharacterHelper.CameraType.ThirdPerson)
        //                     currentControllerScript.UIManager.CharacterUI.crosshairMainObject.gameObject.SetActive(true);
        //                 
        //                 if (Helper.ReachedPositionAndRotation(cameraForSwitching.transform.position, controllers[CurrentCharacter].thisCamera.transform.position,
        //                     cameraForSwitching.transform.eulerAngles, controllers[CurrentCharacter].thisCamera.transform.eulerAngles))
        //                 {
        //                     controllers[CurrentCharacter].ActiveCharacter = true;
        //                     
        //                     if(Application.isMobilePlatform || projectSettings.mobileDebug)
        //                         controllers[CurrentCharacter].UIManager.UIButtonsMainObject.SetActive(true);
        //
        //                     controllers[CurrentCharacter].thisCamera.GetComponent<Camera>().enabled = true;
        //
        //                     cameraFlyingStep1 = false;
        //                     cameraFlyingStep2 = false;
        //                     switchingCamera = false;
        //                     
        //                     if(controllers[CurrentCharacter].thisCamera.GetComponent<AudioListener>())
        //                         controllers[CurrentCharacter].thisCamera.GetComponent<AudioListener>().enabled = true;
        //                     
        //                     StopCoroutine(FlyCamera());
        //                     break;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             cameraForSwitching.transform.position = Helper.MoveObjInNewPosition(cameraForSwitching.transform.position,
        //                 controllers[CurrentCharacter].thisCamera.transform.position, 5 * Time.deltaTime);
        //
        //             cameraForSwitching.transform.rotation = Quaternion.Slerp(cameraForSwitching.transform.rotation,
        //                 controllers[CurrentCharacter].thisCamera.transform.rotation, 2.5f * Time.deltaTime);
        //
        //             if (Helper.ReachedPositionAndRotation(cameraForSwitching.transform.position, controllers[CurrentCharacter].thisCamera.transform.position,
        //                 cameraForSwitching.transform.eulerAngles, controllers[CurrentCharacter].thisCamera.transform.eulerAngles))
        //             {
        //                 controllers[CurrentCharacter].ActiveCharacter = true;
        //                 controllers[CurrentCharacter].thisCamera.GetComponent<Camera>().enabled = true;
        //                 
        //                 if(Application.isMobilePlatform || projectSettings.mobileDebug)
        //                     controllers[CurrentCharacter].UIManager.UIButtonsMainObject.SetActive(true);
        //
        //                 cameraFlyingStep1 = false;
        //                 cameraFlyingStep2 = false;
        //                 switchingCamera = false;
        //
        //                 StopCoroutine(FlyCamera());
        //                 break;
        //             }
        //         }
        //
        //         yield return 0;
        //     }
        // }

        void Update()
        {
            if(!gameStarted || Characters.Count == 0 || controllers.Count <= 0 || !controllers[CurrentCharacter] || inventoryManager.Count <= 0 || !inventoryManager[CurrentCharacter])
                return;
            
            if (controllers[CurrentCharacter].projectSettings.ButtonsActivityStatuses[18] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[18])
                || InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[16], controllers[CurrentCharacter])))
                
                // (Input.GetKeyDown(controllers[CurrentCharacter]._gamepadCodes[16]) || Input.GetKeyDown(controllers[CurrentCharacter]._keyboardCodes[18]) ||
                //     Helper.CheckGamepadAxisButton(16, controllers[CurrentCharacter]._gamepadButtonsAxes, controllers[CurrentCharacter].hasAxisButtonPressed, 
                //         "GetKeyDown", controllers[CurrentCharacter].projectSettings.AxisButtonValues[16])))
            {
                SwitchCharacter();
            }

            if (controllers[CurrentCharacter].health <= 0)
            {
                if (currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.MainObject)
                {
                    if (!currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.MainObject.activeSelf)
                        SwitchMenu("gameOver");
                }
                else StartCoroutine("FastRestart");
            }
            else
            {
                if (controllers[CurrentCharacter].projectSettings.ButtonsActivityStatuses[10] && (InputHelper.WasKeyboardOrMouseButtonPressed(projectSettings.keyboardButtonsInUnityInputSystem[10])
                                                                                                                               || InputHelper.WasGamepadButtonPressed(projectSettings.gamepadButtonsInUnityInputSystem[10], controllers[CurrentCharacter])))
                {
                    if (inventoryManager[CurrentCharacter].Controller.UIManager.CharacterUI.Inventory.MainObject)
                        if (inventoryManager[CurrentCharacter].Controller.UIManager.CharacterUI.Inventory.MainObject.activeSelf)
                            return;

                    Pause(true);
                }
            }
        }

        public void OptionsMenu()
        {
            isOptions = !isOptions;
            
           // UIHelper.ResetSettingsButtons(currentUIManager.gameOptions.graphicsButtons,  PlayerPrefs.GetInt("CurrentQuality"));
            
            SwitchMenu(isOptions ? "options" : "pause");
        }

        // public void SetQuality(int index, bool resetButtons)
        // {
        //     QualitySettings.SetQualityLevel(index);
        //     
        //     PlayerPrefs.SetInt("GraphicIndex", index);
        //     
        //     CurrentQuality = index;
        //     
        //     if(resetButtons)
        //         currentUIManager.ResetSettingsButtons(index);
        // }

        void SwitchMenu(string type)
        {
            currentUIManager.HideAllSinglePlayerMenus();
            
            switch (type)
            {
                case "pause":
                    currentUIManager.SinglePlayerGame.SinglePlayerGamePause.ActivateAll();
                    break;
                
                case "options":
                    currentUIManager.gameOptions.ActivateAll();
                    break;
                
                case "gameOver":
                    currentUIManager.SinglePlayerGame.SinglePlayerGameGameOver.ActivateAll();
                    break;
            }
        }

        public void Pause(bool showUI)
        {
            isPause = !isPause;
            
            SwitchMenu("null");

            if (!isPause)
                isOptions = false;

            controllers[CurrentCharacter].CameraController.canUseCursorInPause = true;

            if (isPause)
            {
                AudioListener.pause = true;
                controllers[CurrentCharacter].isPause = true;
                UIHelper.ManageUIButtons(controllers[CurrentCharacter], controllers[CurrentCharacter].inventoryManager, currentUIManager, controllers[CurrentCharacter].CharacterSync);

            }
            else
            {
                AudioListener.pause = false;
                StartCoroutine(ControllerPauseDelay());
            }

            if (isPause && showUI)
                SwitchMenu("pause");
            else SwitchMenu("null");
            

            Time.timeScale = isPause ? 0 : 1;
        }

        private IEnumerator ControllerPauseDelay()
        {
            yield return new WaitForSeconds(0.1f);
            controllers[CurrentCharacter].isPause = false;
            
            UIHelper.ManageUIButtons(controllers[CurrentCharacter], controllers[CurrentCharacter].inventoryManager, currentUIManager, controllers[CurrentCharacter].CharacterSync);

            StopCoroutine(ControllerPauseDelay());
        }

        void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        IEnumerator rotationTimeout()
        {
            yield return new WaitForSeconds(0.1f);
                
            controllers[0].transform.rotation = Quaternion.Euler(0, Characters[0].spawnZone.transform.eulerAngles.y, 0);
            cameraController[0]._mouseAbsolute = new Vector2(Characters[0].spawnZone.transform.eulerAngles.y, 0);
        }

        IEnumerator FastRestart()
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            StopCoroutine("FastRestart");
        }

    }
}