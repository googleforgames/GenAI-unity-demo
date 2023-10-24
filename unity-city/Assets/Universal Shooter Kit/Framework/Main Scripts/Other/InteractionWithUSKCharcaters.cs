using System;
using System.Collections;
using System.Collections.Generic;
#if USK_NWHVPH_INTEGRATION
using NWH.Common.Cameras;
using NWH.Common.SceneManagement;
#endif
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class InteractionWithUSKCharcaters :

#if !USK_RCC_INTEGRATION
    MonoBehaviour
#else
        BCG_EnterExitVehicle
#endif
    { 
        public InteractionWithCars controller;
        
#if USK_EVPH_INTEGRATION
        public EVP.VehicleStandardInput vehicleInput;
        public EVP.VehicleController vehicleController;
        public Transform getOutPosition;
#elif USK_NWHVPH_INTEGRATION
        public NWH.Vehicle vehicleController;
        public NWH.Common.Cameras.CameraChanger cameraChanger;
        
        public int cameraInsideVehicle;
        public int cameraFollow;
        public int cameraMouseDrag;
        public Transform getOutPosition;
#endif

        public float interactionTimeout;

#if USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
        void Awake()
        {

#if USK_EVPH_INTEGRATION
            vehicleController = gameObject.GetComponent<EVP.VehicleController>();
            vehicleInput = gameObject.GetComponent<EVP.VehicleStandardInput>();
            FindGetOutPosition();
            
            enabled = false;
#elif USK_NWHVPH_INTEGRATION

            vehicleController = GetComponent<NWH.Vehicle>();
            FindGetOutPosition();
            
            cameraChanger = GetComponentInChildren<NWH.Common.Cameras.CameraChanger>(true);

            if (cameraChanger)
            {
                for (var i = 0; i < cameraChanger.cameras.Count; i++)
                {
                    var camera = cameraChanger.cameras[i];

                    if (camera.GetComponent<NWH.Common.Cameras.CameraInsideVehicle>())
                        cameraInsideVehicle = i;

                    else if (camera.GetComponent<NWH.Common.Cameras.VehicleCamera>())
                        cameraFollow = i;

                    else if (camera.GetComponent<NWH.Common.Cameras.CameraMouseDrag>())
                        cameraMouseDrag = i;
                }
            }
            
            enabled = false;
#endif
        }
#endif

        private void Start()
        {
            
#if USK_RCC_INTEGRATION
            Helper.ChangeLayersRecursively(transform, "Default");
#endif
        }

        void Update()
        {
            interactionTimeout += Time.deltaTime;

#if USK_RCC_INTEGRATION
            if (driver != null && controller != null && (InputHelper.WasKeyboardOrMouseButtonPressed(controller.controller.projectSettings.keyboardButtonsInUnityInputSystem[20]) ||
                                                         InputHelper.WasGamepadButtonPressed(controller.controller.projectSettings.gamepadButtonsInUnityInputSystem[18])))
            {
                if (interactionTimeout > 1)
                {
                    interactionTimeout = 0;
                    controller.GetOutVehicle();
                }
            }
#elif USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
            if (controller != null && controller.inCar && (InputHelper.WasKeyboardOrMouseButtonPressed(controller.controller.projectSettings.keyboardButtonsInUnityInputSystem[20]) ||
                                                           InputHelper.WasGamepadButtonPressed(controller.controller.projectSettings.gamepadButtonsInUnityInputSystem[18])))
            {
                if (interactionTimeout > 1)
                {
                    interactionTimeout = 0;
                    controller.GetOutVehicle();
                }
            }
#endif
            
        }

#if USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
        void FindGetOutPosition()
        {
            if (transform.Find ("Get Out Pos")) {
			
                getOutPosition = transform.Find ("Get Out Pos");

            }
            else
            {
                var getOut = new GameObject("Get Out Pos");
                getOut.transform.SetParent(transform, false);
                getOut.transform.rotation = transform.rotation;
                getOut.transform.localPosition = new Vector3(-1.5f, 0f, 0f);
                getOutPosition = getOut.transform;
            }
        }
#endif 
    }
}
