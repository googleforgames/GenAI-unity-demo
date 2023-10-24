using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	public class DemoControl : MonoBehaviour
	{
		public GameObject tdInfo;
		public GameObject tpInfo;
		public GameObject commonInfo;
		public GameObject inputMap;
		public GameObject weaponAimingModeInfo;
		public Text movementModeInfo;

		public bool isSaveSceneExample;
		public GameObject saveSceneInfo;
		
		[HideInInspector] public Controller controller;

		private bool mobileVersion;
		private bool showMenu;
		private bool canShowMenu;
		private bool canShowMultiplayerMenu;
		private bool showInputMenu;

#if USK_MULTIPLAYER
		private RoomManager roomManager;
#endif

		private void Awake()
		{
			// canShowMenu = CheckPlayerPrefs("ShowMenu");
			// canShowMultiplayerMenu = CheckPlayerPrefs("ShowMultiplayerMenu");
		}

		void Start()
		{
			if (!isSaveSceneExample)
			{
				controller = FindObjectOfType<Controller>();

#if USK_MULTIPLAYER
			roomManager = FindObjectOfType<RoomManager>();
#endif

				if (commonInfo && !mobileVersion
#if USK_MULTIPLAYER
			              && !roomManager
#endif
				)
					commonInfo.SetActive(true);
				else commonInfo.SetActive(false);

				if (tpInfo) tpInfo.SetActive(false);
				if (tdInfo) tdInfo.SetActive(false);

				if (weaponAimingModeInfo) weaponAimingModeInfo.SetActive(false);

				if (movementModeInfo) movementModeInfo.gameObject.SetActive(false);
			}
			else
			{
				if(saveSceneInfo) saveSceneInfo.gameObject.SetActive(true);
			}
		}

		bool CheckPlayerPrefs(string value)
		{
			if (PlayerPrefs.HasKey(value))
			{
				if (PlayerPrefs.GetInt(value) == 0)
				{
					PlayerPrefs.SetInt(value, 1);
					return true;
				}

				return false;
			}

			PlayerPrefs.SetInt(value, 1);
			return true;
		}
		
		void Update()
		{
			if(saveSceneInfo) return;
			
			if (controller)
			{
				if(controller.projectSettings)
					mobileVersion = Application.isMobilePlatform || controller.projectSettings.mobileDebug;
				
				if (!inputMap.activeInHierarchy && !controller.isPause)
				{
					if(commonInfo && !mobileVersion) commonInfo.SetActive(true);
					
					if (controller.TypeOfCamera == CharacterHelper.CameraType.TopDown || controller.emulateTDModeLikeTP)
					{
						if (tdInfo && !tdInfo.activeInHierarchy && !mobileVersion) tdInfo.SetActive(true);
						if (tpInfo && tpInfo.activeInHierarchy) tpInfo.SetActive(false);
						
						if(weaponAimingModeInfo) weaponAimingModeInfo.SetActive(false);
						
						if (movementModeInfo)
						{
							movementModeInfo.gameObject.SetActive(true);

							if (controller.CameraParameters.lockCamera)
							{
								movementModeInfo.text = controller.CameraParameters.lookAtCursor ? "TD Mode - Lock Camera " + "\n" + "The character aims where the cursor is pointing" : "TD Mode - Lock Camera " + "\n" + "The character aims directly";
							}
							else if(controller.emulateTDModeLikeTP)
							{
								movementModeInfo.text = "TD Mode - [Free Camera] " + "\n" + "The character behaves the same way as in the TP mode";
							}
							else
							{
								movementModeInfo.text = "TD Mode - [Free Camera] " + "\n" + "The character is always aimed";
							}
						}
					}
					else if (controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && !controller.emulateTDModeLikeTP)
					{
						if (weaponAimingModeInfo)
						{
							weaponAimingModeInfo.SetActive(!controller.isAlwaysTpAimEnabled && controller.inventoryManager.WeaponController && controller.inventoryManager.WeaponController.switchToFpCamera);
						}

						if (movementModeInfo)
						{
							movementModeInfo.gameObject.SetActive(true);

							if (controller.movementType == CharacterHelper.MovementType.Standard)
							{
								movementModeInfo.text = "TP Mode - [Classic]";
							}
							else
							{
								movementModeInfo.text = controller.isAlwaysTpAimEnabled ? "TP Mode - [All Directions Movement] + [Always Aim]" : "TP Mode - [All Directions Movement]";
							}
						}

						if (tpInfo && !tpInfo.activeInHierarchy && !mobileVersion) tpInfo.SetActive(true);
						if (tdInfo && tdInfo.activeInHierarchy) tdInfo.SetActive(false);
					}
					else
					{
						if (tpInfo && tpInfo.activeInHierarchy) tpInfo.SetActive(false);
						if (tdInfo && tdInfo.activeInHierarchy) tdInfo.SetActive(false);
						if(movementModeInfo) movementModeInfo.gameObject.SetActive(false);
						if(weaponAimingModeInfo) weaponAimingModeInfo.SetActive(false);

					}
				}
				else
				{
					if(tpInfo) tpInfo.SetActive(false);
					if(tdInfo) tdInfo.SetActive(false);
					if(commonInfo) commonInfo.SetActive(false);
					if(weaponAimingModeInfo) weaponAimingModeInfo.SetActive(false);
					if(movementModeInfo) movementModeInfo.gameObject.SetActive(false);
				}
			}
			else
			{
#if USK_MULTIPLAYER
				if (roomManager && roomManager.controller)
					controller = roomManager.controller;
#endif
			}

			if (Input.GetKeyDown(KeyCode.F1))
			{
				if (inputMap && !mobileVersion) inputMap.SetActive(true);
			}
			else if (Input.GetKeyUp(KeyCode.F1))
			{
				if (inputMap && !mobileVersion) inputMap.SetActive(false);
			}
		}
	}
}
