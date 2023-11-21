using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if USK_EVPH_INTEGRATION
using EVP;
#endif
#if USK_NWHVPH_INTEGRATION
using NWH;
using NWH.Common.SceneManagement;
using NWH.VehiclePhysics2.Input;
using NWH.VehiclePhysics2.VehicleGUI;
#endif
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class InteractionWithCars :
#if USK_RCC_INTEGRATION
        BCG_EnterExitPlayer
#elif USK_NWHVPH_INTEGRATION
            MonoBehaviour
         //CharacterVehicleChanger
#else
        MonoBehaviour
#endif
    {
        public bool useRayCast = true;
        public bool matchesCharacterCamera;
        
        public float distanceToInteract = 5;
        public int enterExitSpeedLimit = 20;
        
        public Controller controller;

#if USK_RCC_INTEGRATION
        public RCC_Camera.CameraMode carCameraMode;
        private RCC_UIDashboardDisplay uiManager;
        private List<BCG_EnterExitVehicle> allRCCVehiclesOnScene = new List<BCG_EnterExitVehicle>();
#elif USK_EVPH_INTEGRATION
        public EVP.VehicleCameraController.Mode carCameraMode;
        public EVP.VehicleCameraController evpVehicleCamera;
        public List<InteractionWithUSKCharcaters> allVehiclesInScene = new List<InteractionWithUSKCharcaters>();
        public InteractionWithUSKCharcaters targetVehicle;
#elif USK_NWHVPH_INTEGRATION
        public int currentCarCamera;
        public VehicleChanger vehicleChanger;
        public DashGUIController uiController;
        public MobileVehicleInputProvider mobileInputProvider;
        public List<InteractionWithUSKCharcaters> allVehiclesInScene = new List<InteractionWithUSKCharcaters>();
        public InteractionWithUSKCharcaters targetVehicle;
#endif

        public bool inCar;
        public bool showUITooltip;
        private bool enableUI;
        private bool changeInputMethod;
        private float interactionTimeout;

#if USK_RCC_INTEGRATION || USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION

        void Awake()
        {
#if USK_NWHVPH_INTEGRATION
            // Instance = this;
#endif
        }
        
        void Start()
        {
            controller = GetComponent<Controller>();
            controller.interactionWithCars = this;

#if USK_RCC_INTEGRATION

            if (FindObjectOfType<RCC_UIDashboardDisplay>())
            {
                uiManager = FindObjectOfType<RCC_UIDashboardDisplay>();
                uiManager.gameObject.SetActive(false);
            }
            
            BCG_EnterExitSettings.Instance.enterExitSpeedLimit = enterExitSpeedLimit;
            var allCars = FindObjectsOfType<RCC_CarControllerV3>().ToList();
            
            foreach (var car in allCars)
            {
                var _car = car.gameObject.AddComponent<InteractionWithUSKCharcaters>();
                allRCCVehiclesOnScene.Add(_car);
            }
#endif

#if USK_EVPH_INTEGRATION
            var allCars = FindObjectsOfType<EVP.VehicleController>().ToList();

            foreach (var car in allCars)
            {
                var _car = car.gameObject.AddComponent<InteractionWithUSKCharcaters>();
                allVehiclesInScene.Add(_car);
            }
            
            foreach (var vehicle in allVehiclesInScene)
            {
                if(vehicle.vehicleInput)
                    vehicle.vehicleInput.enabled = false;
            }
            
            if (FindObjectOfType<EVP.VehicleCameraController>())
            {
                evpVehicleCamera = FindObjectOfType<EVP.VehicleCameraController>();
                evpVehicleCamera.gameObject.SetActive(false);
            }
            
#elif USK_NWHVPH_INTEGRATION
            // Instance = this;

            var allCars = FindObjectsOfType<Vehicle>().ToList();
            
            foreach (var car in allCars)
            {
                // var obj = new GameObject("InteractionWithUSKCharacters Component");
                // obj.transform.parent = car.transform;
                //
                var _car = car.gameObject.AddComponent<InteractionWithUSKCharcaters>();
                allVehiclesInScene.Add(_car);
            }

            vehicleChanger = FindObjectOfType<VehicleChanger>();
            
            vehicleChanger.characterBased = true;
            vehicleChanger.deactivateAll  = true;

            mobileInputProvider = FindObjectOfType<MobileVehicleInputProvider>();
            
            if(mobileInputProvider)
                mobileInputProvider.gameObject.SetActive(false);
            
            var allDashBoards = FindObjectsOfType<DashGUIController>();

            foreach (var dashBoard in allDashBoards)
            {
                if (dashBoard.dataSource == DashGUIController.DataSource.VehicleChanger)
                    uiController = dashBoard;
            }
            
            if(uiController)
                uiController.gameObject.SetActive(false);
            
#endif
        }

        public void UIButtonEvent()
        {
            if(inCar) GetOutVehicle();
            else GetInVehicle();
        }


        void GetInVehicle()
        {
            if(interactionTimeout < 1) return;

            interactionTimeout = 0;
            
            
#if USK_RCC_INTEGRATION

            if (uiManager)
                uiManager.gameObject.SetActive(true);
            
            // if (Application.isMobilePlatform || controller.projectSettings.mobileDebug) RCC_Settings.Instance.controllerType = RCC_Settings.ControllerType.Mobile;
            // else
            // {
            //     if (uiManager && uiManager.controllerButtons)
            //         uiManager.controllerButtons.gameObject.SetActive(false);
            //         
            //     RCC_Settings.Instance. = RCC_Settings.ControllerType.Keyboard;
            // }

            GetIn(targetVehicle);
            targetVehicle.gameObject.GetComponent<InteractionWithUSKCharcaters>().controller = this;
            targetVehicle.gameObject.GetComponent<InteractionWithUSKCharcaters>().interactionTimeout = 0;

#elif USK_EVPH_INTEGRATION
            if (targetVehicle.vehicleController.speed > enterExitSpeedLimit) return;

            targetVehicle.controller = this;
            targetVehicle.enabled = true;
            
            targetVehicle.vehicleInput.enabled = true;
            
              if (evpVehicleCamera)
            {
                evpVehicleCamera.gameObject.SetActive(true);
                evpVehicleCamera.target = targetVehicle.transform;
            }
#elif USK_NWHVPH_INTEGRATION
            
            if (targetVehicle.vehicleController.Speed > enterExitSpeedLimit) return;

            targetVehicle.controller = this;
            
            vehicleChanger.deactivateAll = false;
            vehicleChanger.ChangeVehicle(targetVehicle.vehicleController);

            targetVehicle.enabled = true;

            if(uiController) uiController.gameObject.SetActive(true);
            
            if(mobileInputProvider)// && (Application.isMobilePlatform || controller.projectSettings.mobileDebug))
                mobileInputProvider.gameObject.SetActive(true);
#endif

            if (controller.CameraController.isCameraAimEnabled)
            {
                if (controller.inventoryManager.WeaponController) controller.inventoryManager.WeaponController.Aim(true, false, false);
                else controller.CameraController.Aim();
            }
            
            controller.gameObject.SetActive(false);
            controller.thisCamera.SetActive(false);
            controller.inCar = true;
            
            inCar = true;

            if (controller.UIManager)
            {
                controller.UIManager.CharacterUI.DisableAll();
                
                if(controller.UIManager.CharacterUI.crosshairMainObject)
                    controller.UIManager.CharacterUI.crosshairMainObject.gameObject.SetActive(false);

                if (Application.isMobilePlatform || controller.projectSettings.mobileDebug)
                {
                    UIHelper.ManageUIButtons(controller, controller.inventoryManager, controller.UIManager, controller.CharacterSync);
                }

                enableUI = false;
            }


#if USK_RCC_INTEGRATION
            var RCC_Camera = FindObjectOfType<RCC_Camera>();
#elif USK_EVPH_INTEGRATION
            var EVP_Camera = FindObjectOfType<EVP.VehicleCameraController>();
#endif

            if (matchesCharacterCamera)
            {
#if USK_RCC_INTEGRATION
                switch (controller.TypeOfCamera)
                {
                    case CharacterHelper.CameraType.ThirdPerson:
                        RCC_Camera.cameraMode = !controller.emulateTDModeLikeTP ? RCC_Camera.CameraMode.TPS : RCC_Camera.CameraMode.TOP;
                        break;
                    case CharacterHelper.CameraType.FirstPerson:
                        RCC_Camera.cameraMode = RCC_Camera.CameraMode.FPS;
                        break;
                    case CharacterHelper.CameraType.TopDown:
                        RCC_Camera.cameraMode = RCC_Camera.CameraMode.TOP;
                        break;
                }
#elif USK_EVPH_INTEGRATION
                switch (controller.TypeOfCamera)
                {
                    case CharacterHelper.CameraType.ThirdPerson:
                        EVP_Camera.mode = VehicleCameraController.Mode.MouseOrbit; //!controller.emulateTDModeLikeTP ? VehicleCameraController.Mode.MouseOrbit : VehicleCameraController.Mode.SmoothFollow;
                        break;
                    case CharacterHelper.CameraType.FirstPerson:
                        EVP_Camera.mode = VehicleCameraController.Mode.AttachTo;
                        break;
                    case CharacterHelper.CameraType.TopDown:
                        EVP_Camera.mode = VehicleCameraController.Mode.MouseOrbit;
                        break;
                }
#elif USK_NWHVPH_INTEGRATION
                switch (controller.TypeOfCamera)
                {
                    case CharacterHelper.CameraType.ThirdPerson:
                        currentCarCamera = !controller.emulateTDModeLikeTP ? targetVehicle.cameraMouseDrag : targetVehicle.cameraFollow;
                        break;
                    case CharacterHelper.CameraType.FirstPerson:
                        currentCarCamera = targetVehicle.cameraInsideVehicle;
                        break;
                    case CharacterHelper.CameraType.TopDown:
                        currentCarCamera = targetVehicle.cameraFollow;
                        break;
                }
                
                while (targetVehicle.cameraChanger.currentCameraIndex != currentCarCamera)
                    targetVehicle.cameraChanger.NextCamera();
#endif
            }
            else
            {
#if USK_RCC_INTEGRATION
                RCC_Camera.cameraMode = carCameraMode;
#elif USK_EVPH_INTEGRATION
                EVP_Camera.mode = carCameraMode;
#elif USK_NWHVPH_INTEGRATION
                while (targetVehicle.cameraChanger.currentCameraIndex != currentCarCamera)
                    targetVehicle.cameraChanger.NextCamera();
#endif
            }
        }


        public void GetOutVehicle()
        {
#if USK_RCC_INTEGRATION
            
            GetOut();
            
            inCar = false;
            controller.inCar = inCar;

            if(uiManager)
                uiManager.gameObject.SetActive(false);

#elif USK_EVPH_INTEGRATION
            if (targetVehicle.vehicleController.speed > enterExitSpeedLimit) return;
            
             targetVehicle.vehicleInput.enabled = false;
            
            if(evpVehicleCamera)
                evpVehicleCamera.gameObject.SetActive(false);
            
            targetVehicle.controller = null;
            
#elif USK_NWHVPH_INTEGRATION

            if (targetVehicle.vehicleController.Speed > enterExitSpeedLimit) return;
            
            vehicleChanger.DeactivateAllIncludingActive();
            vehicleChanger.deactivateAll = true;
            
            if(uiController)
                uiController.gameObject.SetActive(false);
            
            if(mobileInputProvider)
                mobileInputProvider.gameObject.SetActive(false);
            
            targetVehicle.controller = null;
#endif
            
#if USK_NWHVPH_INTEGRATION || USK_EVPH_INTEGRATION
            inCar = false;
            controller.inCar = false;
            controller.gameObject.transform.position = targetVehicle.getOutPosition.position;

            targetVehicle.enabled = false;
            
            controller.gameObject.SetActive(true);
            controller.thisCamera.SetActive(true);
#endif

            controller.CameraController.setCameraType = true;
        }
        

        void Update()
        {
            interactionTimeout += Time.deltaTime;
            
            if (showUITooltip)
            {
                if (Application.isMobilePlatform || controller.projectSettings.mobileDebug)
                {
                    if (controller.UIManager.CharacterUI.infoTooltip && !controller.UIManager.CharacterUI.infoTooltip.gameObject.activeSelf)
                    {
                        Helper.EnableAllParents(controller.UIManager.CharacterUI.infoTooltip.gameObject);
                        controller.UIManager.CharacterUI.infoTooltip.text = "Press ''" + "interaction" + "'' button to enter the ''" + targetVehicle.gameObject.name + "'' car" ;
                    }

                    if (controller.UIManager.uiButtons[15] && !controller.UIManager.uiButtons[15].gameObject.activeSelf)
                        controller.UIManager.uiButtons[15].gameObject.SetActive(true);
                }
                else
                {
                    if (controller.UIManager.CharacterUI.infoTooltip && !controller.UIManager.CharacterUI.infoTooltip.gameObject.activeSelf)
                    {
                        Helper.EnableAllParents(controller.UIManager.CharacterUI.infoTooltip.gameObject);
                        controller.UIManager.CharacterUI.infoTooltip.text = "Press ''" + (!controller.CameraController.useGamepad ? controller.projectSettings.keyboardButtonsInProjectSettings[20].ToString() : controller.projectSettings.gamepadButtonsInProjectSettings[18].ToString()) + "'' button to enter the ''" + targetVehicle.gameObject.name + "'' car" ;
                    }

                    if (controller.UIManager.uiButtons[15] && controller.UIManager.uiButtons[15].gameObject.activeSelf)
                        controller.UIManager.uiButtons[15].gameObject.SetActive(false);
                }
            }
            else
            {
                if (controller.UIManager.CharacterUI.infoTooltip && controller.UIManager.CharacterUI.infoTooltip.gameObject.activeSelf)
                    controller.UIManager.CharacterUI.infoTooltip.gameObject.SetActive(false);

                if (controller.UIManager.uiButtons[15] && controller.UIManager.uiButtons[15].gameObject.activeSelf)
                    controller.UIManager.uiButtons[15].gameObject.SetActive(false);
            }

            if (!inCar)
            {
                // if (controller.inCar)
                //     controller.inCar = false;

                if (controller.UIManager && !enableUI)
                {
                    controller.UIManager.CharacterUI.ActivateAll(controller.UIManager.useMinimap);

                    if (Application.isMobilePlatform || controller.projectSettings.mobileDebug)
                        UIHelper.ManageUIButtons(controller, controller.inventoryManager, controller.UIManager, controller.CharacterSync);

                    enableUI = true;
                }

                // BCG_EnterExitSettings.Instance.enterExitVehicleKB = controller.CameraController.useGamepad ? controller.projectSettings.gamepadButtonsInUnityInputSystem[18] : controller.projectSettings.keyboardButtonsInUnityInputSystem[20];

                RaycastHit hit;

                // common scripts
                if (useRayCast)
                {
                    if (controller.TypeOfCamera != CharacterHelper.CameraType.TopDown)
                    {
                        var direction = controller.thisCamera.transform.TransformDirection(Vector3.forward);

                        if (!Physics.Raycast(controller.thisCamera.transform.position, direction, out hit, 100, Helper.LayerMask()))
                        {
                            showUITooltip = false;

#if USK_RCC_INTEGRATION
                        targetVehicle = null;
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                            targetVehicle = null;
#endif
                            return;
                        }

                    }
                    else
                    {
                        if (!Physics.Raycast(controller.BodyObjects.Head.position + transform.forward * 2, Vector3.down * 3, out hit, 100, Helper.LayerMask()))
                        {
                            showUITooltip = false;

#if USK_RCC_INTEGRATION
                        targetVehicle = null;
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                            targetVehicle = null;
#endif

                            return;
                        }
                    }
                    
                    if (hit.distance < distanceToInteract

#if USK_RCC_INTEGRATION
                        && hit.transform.gameObject.GetComponentInParent<BCG_EnterExitVehicle>())
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                        && hit.transform.gameObject.GetComponentInParent<InteractionWithUSKCharcaters>())
#endif

                    {
#if USK_RCC_INTEGRATION
                    if (!targetVehicle)
                    {
                        targetVehicle = hit.transform.gameObject.GetComponentInParent<BCG_EnterExitVehicle>();
                    }
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                        if (!targetVehicle)
                        {
                            targetVehicle = hit.transform.gameObject.GetComponentInParent<InteractionWithUSKCharcaters>();
                        }
#endif
                        else
                        {
                            showUITooltip = true;

                            if (InputHelper.WasKeyboardOrMouseButtonPressed(controller.projectSettings.keyboardButtonsInUnityInputSystem[20]) ||
                                InputHelper.WasGamepadButtonPressed(controller.projectSettings.gamepadButtonsInUnityInputSystem[18]))
                            {
#if USK_RCC_INTEGRATION
#endif
                                GetInVehicle();
                            }
                        }
                    }
                    else
                    {
#if USK_RCC_INTEGRATION
                    targetVehicle = null;
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                        targetVehicle = null;
#endif
                        showUITooltip = false;
                    }
                }
                else
                {
                    
#if USK_RCC_INTEGRATION
                    BCG_EnterExitVehicle closestVehicle = null;
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                    InteractionWithUSKCharcaters closestVehicle = null;
#endif
                    var closestDist = float.MaxValue;

#if USK_RCC_INTEGRATION
                foreach (var vehicle in allRCCVehiclesOnScene)
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                    foreach (var vehicle in allVehiclesInScene)
#endif
                    {
                        var distance = Vector3.Distance(controller.transform.position, vehicle.transform.position);

                        if (distance < distanceToInteract && distance < closestDist)
                        {
                            closestDist = distance;
                            closestVehicle = vehicle;
                        }
                    }
                    
                    if (closestVehicle)
                    {
#if USK_RCC_INTEGRATION
                        if (!targetVehicle || targetVehicle != closestVehicle)
                        {
                            showUITooltip = false;
                            targetVehicle = closestVehicle;
                        }
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                        if (!targetVehicle || targetVehicle != closestVehicle)
                        {
                            showUITooltip = false;
                            targetVehicle = closestVehicle;
                        }
#endif
                        else
                        {
                            showUITooltip = true;

                            if (InputHelper.WasKeyboardOrMouseButtonPressed(controller.projectSettings.keyboardButtonsInUnityInputSystem[20]) ||
                                InputHelper.WasGamepadButtonPressed(controller.projectSettings.gamepadButtonsInUnityInputSystem[18]))
                            {
                                GetInVehicle();
                            }
                        }
                    }
                    else
                    {
                        showUITooltip = false;
#if USK_RCC_INTEGRATION
                            targetVehicle = null;
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
                        targetVehicle = null;
#endif
                    }
                }
            }
        }
#endif
        
#if USK_NWHVPH_INTEGRATION
        // public InteractionWithCars(Vehicle nearestVehicle) : base(nearestVehicle)
        // {
        // }
#endif
    }
}
