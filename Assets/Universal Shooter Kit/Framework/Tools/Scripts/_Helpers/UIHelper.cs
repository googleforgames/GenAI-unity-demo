using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GercStudio.USK.Scripts
{
	public static class UIHelper
	{
		[Serializable]
		public class SinglePlayerGamePause
		{
			public GameObject MainObject;

			public Button Exit;
			public Button Resume;
			public Button Options;

			public Button currentSelectedButton;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button> {Resume, Options, Exit};

				return buttons;
			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);
				
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// }
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
				
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
			}
		}

		[Serializable]
		public class GameOptions
		{
			public GameObject MainObject;

			public Button back;
			public Button resetGameData;
			public SettingsButton fullscreenMode;
			public SettingsButton windowedMode;

			public Text frameRate;

			public Button currentSelectedSettingsButton;

			public SettingsButton resolutionButtonPlaceholder;
			public ScrollRect resolutionsScrollRect;

			public List<SettingsButton> graphicsButtons = new List<SettingsButton>();
			public List<SettingsButton> resolutionButtons = new List<SettingsButton>();
			public List<SettingsButton> frameRateButtons = new List<SettingsButton>();

			public bool firstTimeMenuOpened;

			[Serializable]
			public class SettingsButton
			{
				public Button button;
				public int indexNumber;

				public Resolution resolution;
				public int qualitySettings;
				public int frameRate;

				public Text textPlaceholder;

				public Color normColor;
				public Sprite normSprite;
			}

			public List<Button> GetAllOptionButtons()
			{

				var buttons = (from button in graphicsButtons where button.button select button.button).ToList();
				buttons.AddRange(from button in resolutionButtons where button.button select button.button);
				buttons.AddRange(from button in frameRateButtons where button.button select button.button);

				if (fullscreenMode.button)
					buttons.Add(fullscreenMode.button);

				if (windowedMode.button)
					buttons.Add(windowedMode.button);

				return buttons;
			}

			public void ActivateAll()
			{
				foreach (var button in graphicsButtons.Where(button => button.button))
				{
					Helper.EnableAllParents(button.button.gameObject);
				}

				foreach (var button in resolutionButtons.Where(button => button.button))
				{
					Helper.EnableAllParents(button.button.gameObject);
				}

				foreach (var button in frameRateButtons.Where(button => button.button))
				{
					Helper.EnableAllParents(button.button.gameObject);
				}

				if (back)
					Helper.EnableAllParents(back.gameObject);

				if (MainObject)
					Helper.EnableAllParents(MainObject);

				if (fullscreenMode.button)
					Helper.EnableAllParents(fullscreenMode.button.gameObject);

				if (windowedMode.button)
					Helper.EnableAllParents(windowedMode.button.gameObject);

				if (frameRate)
					Helper.EnableAllParents(frameRate.gameObject);
			}

			public void DisableAll()
			{
				foreach (var button in graphicsButtons.Where(button => button.button))
				{
					button.button.gameObject.SetActive(false);
				}

				foreach (var button in resolutionButtons.Where(button => button.button))
				{
					button.button.gameObject.SetActive(false);
				}

				foreach (var button in frameRateButtons.Where(button => button.button))
				{
					button.button.gameObject.SetActive(false);
				}

				if (back)
					back.gameObject.SetActive(false);

				if (fullscreenMode.button)
					fullscreenMode.button.gameObject.SetActive(false);

				if (windowedMode.button)
					windowedMode.button.gameObject.SetActive(false);

				if (frameRate)
					frameRate.gameObject.SetActive(false);

				if (MainObject)
					MainObject.SetActive(false);
				
				if(resetGameData)
					resetGameData.gameObject.SetActive(false);
			}
		}

		public static List<Resolution> GetResolutions(out List<string> stringResolutions)
		{
			var resolutions = Screen.resolutions;
			var isWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
			var isFullScreenWindow = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
			var systemWidth = Display.main.systemWidth;
			var systemHeight = Display.main.systemHeight;

			stringResolutions = new List<string>();

			var finalResolutions = new List<Resolution>();

			foreach (var res in resolutions)
			{
				var resParts = res.ToString().Split(new char[] {'x', '@', 'H'});
				var width = int.Parse(resParts[0].Trim());
				var height = int.Parse(resParts[1].Trim());

				// skip resolutions that won't fit in windowed modes
				if (isWindowed && (width >= systemWidth || height >= systemHeight))
					continue;
				if (isFullScreenWindow && (width > systemWidth || height > systemHeight))
					continue;

				var resString = GetResolutionString(width, height);

				finalResolutions.Add(res);
				stringResolutions.Add(resString);
			}

			return finalResolutions;
		}

		static string GetResolutionString(int w, int h)
		{
			return string.Format("{0}x{1}", w, h);
		}

		public static void SetResolution(Resolution resolution, FullScreenMode mode, int hz)
		{
			Screen.SetResolution(resolution.width, resolution.height, mode, hz);
		}

		public static void SetFrameRate(int frameRate)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = frameRate > 0 ? (frameRate + 3) : 100;
		}

		public static void SetQuality(int value)
		{
			QualitySettings.SetQualityLevel(value, !Application.isMobilePlatform);

			if (!Application.isMobilePlatform)
				QualitySettings.vSyncCount = 0;
		}

		public static void SetWindowMode(int index, Resolution currentResolution)
		{
			if (index == 0) // full screen mode
			{
				Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
			}
			else if (index == 1) // windowed mode
			{
				Screen.fullScreenMode = FullScreenMode.Windowed;

				var screenWidth = Display.main.systemWidth;
				var screenHeight = Display.main.systemHeight;

				if (currentResolution.width >= screenWidth || currentResolution.height >= screenHeight)
				{
					var closestWidth = screenWidth;
					var closestHeight = screenHeight;
					foreach (var res in Screen.resolutions)
					{
						if (res.width < screenWidth && res.height < screenHeight)
						{
							closestWidth = res.width;
							closestHeight = res.height;
						}
					}

					SetResolution(new Resolution {width = closestWidth, height = closestHeight}, FullScreenMode.Windowed, QualitySettings.vSyncCount);
				}
				else
				{
					SetResolution(currentResolution, FullScreenMode.Windowed, QualitySettings.vSyncCount);
				}
			}
		}

		public static void ResetSettingsButtons(List<GameOptions.SettingsButton> buttons, int currentButtonIndex)
		{
			Button currentButton = null;

			for (var i = 0; i < buttons.Count; i++)
			{
				var button = buttons[i];
				if (!button.button) continue;

				Helper.ChangeButtonColor(button.button, button.normColor, button.normSprite);

				if (i == currentButtonIndex)
				{
					currentButton = button.button;
				}
			}

			if (buttons.Count > 0 && currentButton != null)
			{
				var color = currentButton.colors.selectedColor;
				var sprite = currentButton.spriteState.selectedSprite;

				Helper.ChangeButtonColor(currentButton, color, sprite);
			}
		}

		public static void ManageSettingsButtons(UIManager uiManager)
		{
			var allButtons = new List<GameOptions.SettingsButton>();
			allButtons.AddRange(uiManager.gameOptions.graphicsButtons);
			allButtons.AddRange(uiManager.gameOptions.resolutionButtons);
			allButtons.AddRange(uiManager.gameOptions.frameRateButtons);
			allButtons.Add(uiManager.gameOptions.fullscreenMode);
			allButtons.Add(uiManager.gameOptions.windowedMode);

			foreach (var button in allButtons)
			{
				if (button.button)
				{
					switch (button.button.transition)
					{
						case Selectable.Transition.ColorTint:
							button.normColor = button.button.colors.normalColor;
							break;
						case Selectable.Transition.SpriteSwap:
							button.normSprite = button.button.GetComponent<Image>().sprite;
							break;
					}
				}
			}

			for (var i = 0; i < uiManager.gameOptions.graphicsButtons.Count; i++)
			{
				var button = uiManager.gameOptions.graphicsButtons[i];

				if (button.button)
				{
					button.indexNumber = i;
					// button.button.onClick.AddListener(delegate { SetQuality(0); });

					button.button.onClick.AddListener(delegate { uiManager.SetSettingsParameter(uiManager.gameOptions.graphicsButtons, button.indexNumber, "CurrentQualityButton", "quality"); });
				}
			}

			if (!Application.isMobilePlatform)
			{
				for (var i = 0; i < uiManager.gameOptions.resolutionButtons.Count; i++)
				{
					var button = uiManager.gameOptions.resolutionButtons[i];
					if (button.button)
					{
						button.indexNumber = i;
						button.button.onClick.AddListener(delegate { uiManager.SetSettingsParameter(uiManager.gameOptions.resolutionButtons, button.indexNumber, "CurrentResolutionButton", "resolution"); });
					}
				}

				for (var i = 0; i < uiManager.gameOptions.frameRateButtons.Count; i++)
				{
					var button = uiManager.gameOptions.frameRateButtons[i];
					if (button.button)
					{
						button.indexNumber = i;
						button.button.onClick.AddListener(delegate { uiManager.SetSettingsParameter(uiManager.gameOptions.frameRateButtons, button.indexNumber, "CurrentFrameRateButton", "framerate"); });
					}
				}

				uiManager.gameOptions.fullscreenMode.button.onClick.AddListener(delegate { uiManager.SetWindowMode("CurrentWindowModeButton", 0); });
				uiManager.gameOptions.windowedMode.button.onClick.AddListener(delegate { uiManager.SetWindowMode("CurrentWindowModeButton", 1); });
			}
			else
			{
				foreach (var button in uiManager.gameOptions.resolutionButtons)
				{
					button.button.interactable = false;
				}

				foreach (var button in uiManager.gameOptions.frameRateButtons)
				{
					button.button.interactable = false;
				}

				uiManager.gameOptions.fullscreenMode.button.interactable = false;
				uiManager.gameOptions.windowedMode.button.interactable = false;
			}
		}

		[Serializable]
		public class SinglePlayerGameOver
		{
			public GameObject MainObject;

			public Button Restart;
			public Button Exit;

			public List<Button> GetAllOptionButtons()
			{
				var buttons = new List<Button>();

				if (Restart)
					buttons.Add(Restart);

				if (Exit)
					buttons.Add(Exit);

				return buttons;
			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// if (Restart)
				// 	Restart.gameObject.SetActive(false);
				//
				// if (Exit)
				// 	Exit.gameObject.SetActive(false);
				//
				// if (MainObject)
				// 	MainObject.SetActive(false);
			}
		}

		public enum MiniMapType
		{
			Circle,
			Rectangle
		}

		public enum HitIndicatorsBorder
		{
			Screen,
			Circle
		}

		[Serializable]
		public class CharacterUI
		{
			public GameObject MainObject;

			public Text WeaponAmmo;
			public Text Health;
			public Text infoTooltip;

			public List<RawImage> hitMarkers;
			public RawImage WeaponAmmoImagePlaceholder;
			public RawImage PickupImage;
			public RawImage bloodSplatter;
			public RawImage mapPlaceholder;
			public RawImage aimPlaceholder;

			public RectTransform crosshairMainObject;

			public Image HealthBar;
			public RawImage mapMask;
			public Image flashPlaceholder;
			public Image topCrosshairPart;
			public Image bottomCrosshairPart;
			public Image leftCrosshairPart;
			public Image rightCrosshairPart;
			public Image middleCrosshairPart;

			public RawImage attackImage;

			public MiniMapType currentMiniMapType;
			public MiniMapType lastMiniMapType;

			public Inventory Inventory;

			public int circleRadius = 300;
			public HitIndicatorsBorder hitIndicatorsBorder;

			public void DisableAllCrosshairParts()
			{
				if (rightCrosshairPart)
					rightCrosshairPart.gameObject.SetActive(false);

				if (leftCrosshairPart)
					leftCrosshairPart.gameObject.SetActive(false);

				if (topCrosshairPart)
					topCrosshairPart.gameObject.SetActive(false);

				if (bottomCrosshairPart)
					bottomCrosshairPart.gameObject.SetActive(false);
			}

			public void ManageCrosshairParts(WeaponsHelper.CrosshairType type)
			{
				switch (type)
				{
					case WeaponsHelper.CrosshairType.OnePart:

						if (middleCrosshairPart)
							middleCrosshairPart.gameObject.SetActive(middleCrosshairPart.sprite);

						if (rightCrosshairPart)
							rightCrosshairPart.gameObject.SetActive(false);

						if (leftCrosshairPart)
							leftCrosshairPart.gameObject.SetActive(false);

						if (topCrosshairPart)
							topCrosshairPart.gameObject.SetActive(false);

						if (bottomCrosshairPart)
							bottomCrosshairPart.gameObject.SetActive(false);

						break;
					case WeaponsHelper.CrosshairType.TwoParts:

						if (middleCrosshairPart)
							middleCrosshairPart.gameObject.SetActive(middleCrosshairPart.sprite);

						if (rightCrosshairPart)
							rightCrosshairPart.gameObject.SetActive(true);

						if (leftCrosshairPart)
							leftCrosshairPart.gameObject.SetActive(true);

						if (topCrosshairPart)
							topCrosshairPart.gameObject.SetActive(false);

						if (bottomCrosshairPart)
							bottomCrosshairPart.gameObject.SetActive(false);

						break;
					case WeaponsHelper.CrosshairType.FourParts:

						if (middleCrosshairPart)
							middleCrosshairPart.gameObject.SetActive(middleCrosshairPart.sprite);

						if (rightCrosshairPart)
							rightCrosshairPart.gameObject.SetActive(true);

						if (leftCrosshairPart)
							leftCrosshairPart.gameObject.SetActive(true);

						if (topCrosshairPart)
							topCrosshairPart.gameObject.SetActive(true);

						if (bottomCrosshairPart)
							bottomCrosshairPart.gameObject.SetActive(true);

						break;

				}
			}

			public void ActivateAll(bool isMapUsed)
			{
				if (MainObject)
					Helper.EnableAllParents(MainObject);

				if (WeaponAmmo)
					Helper.EnableAllParents(WeaponAmmo.gameObject);

				if (attackImage)
					Helper.EnableAllParents(attackImage.gameObject);

				if (WeaponAmmoImagePlaceholder)
					Helper.EnableAllParents(WeaponAmmoImagePlaceholder.gameObject);

				if (Health)
					Helper.EnableAllParents(Health.gameObject);

				if (HealthBar)
					Helper.EnableAllParents(HealthBar.gameObject);

				if (mapMask && isMapUsed)
					Helper.EnableAllParents(mapMask.gameObject);

				if (mapPlaceholder && isMapUsed)
					Helper.EnableAllParents(mapPlaceholder.gameObject);
			}

			public void DisableAll()
			{
				// UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				if (MainObject)
					MainObject.SetActive(false);
				
				if (WeaponAmmo)
					WeaponAmmo.gameObject.SetActive(false);
				
				if (attackImage)
					attackImage.gameObject.SetActive(false);
				
				if (WeaponAmmoImagePlaceholder)
					WeaponAmmoImagePlaceholder.gameObject.SetActive(false);
				
				if (infoTooltip)
					infoTooltip.gameObject.SetActive(false);
				
				if (Health)
					Health.gameObject.SetActive(false);
				
				if (HealthBar)
					HealthBar.gameObject.SetActive(false);
				
				if (PickupImage)
					PickupImage.gameObject.SetActive(false);
				
				if (mapMask)
					mapMask.gameObject.SetActive(false);

				Inventory.MainObject.SetActive(false);

				if (bloodSplatter)
					bloodSplatter.gameObject.SetActive(false);
			}

			public void ShowImage(string type, InventoryManager inventoryManager)
			{
				switch (type)
				{
					case "weapon":
					{
						for (var i = 0; i < 8; i++)
						{
							if (inventoryManager.slots[i].weaponSlotInGame.Count <= 0)
							{
								var slotButton = Inventory.WeaponsButtons[i];

								if (!slotButton)
									continue;

								slotButton.interactable = false;

								Helper.ChangeButtonColor(inventoryManager.Controller.UIManager, i, "norm");

								if (Inventory.WeaponImagePlaceholder[i])
								{
									var img = Inventory.WeaponImagePlaceholder[i];
									img.color = new Color(1, 1, 1, 0);
								}

								if (Inventory.WeaponAmmoText[i])
									Inventory.WeaponAmmoText[i].gameObject.SetActive(false);

								continue;
							}

							if (Inventory.WeaponsButtons[i])
								Inventory.WeaponsButtons[i].interactable = true;

							if (!inventoryManager.slots[i].weaponSlotInGame[inventoryManager.slots[i].currentWeaponInSlot].fistAttack)
							{
								var weaponController = inventoryManager.slots[i].weaponSlotInGame[inventoryManager.slots[i].currentWeaponInSlot].weapon.GetComponent<WeaponController>();

								if (!weaponController.weaponImage || !Inventory.WeaponsButtons[i])
									continue;

								var image = Inventory.WeaponImagePlaceholder[i];

								image.texture = weaponController.weaponImage;

								image.color = new Color(1, 1, 1, 1);

								if (Inventory.WeaponAmmoText[i])
								{
									Inventory.WeaponAmmoText[i].gameObject.SetActive(true);
									if (weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee)
									{
										if (weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
										{
											Inventory.WeaponAmmoText[i].text = weaponController.Attacks[weaponController.currentAttack].curAmmo.ToString("F0") + "/" +
											                                   weaponController.Attacks[weaponController.currentAttack].inventoryAmmo;
										}
										else
										{
											Inventory.WeaponAmmoText[i].text = weaponController.Attacks[weaponController.currentAttack].curAmmo.ToString("F0");
										}
									}
									else
									{
										Inventory.WeaponAmmoText[i].text = inventoryManager.slots[i].weaponSlotInGame[inventoryManager.slots[i].currentWeaponInSlot].weapon.name;
									}
								}
							}
							else
							{
								if (!Inventory.WeaponImagePlaceholder[i] || !inventoryManager.FistIcon)
									continue;

								var image = Inventory.WeaponImagePlaceholder[i];

								image.texture = inventoryManager.FistIcon;

								image.color = new Color(1, 1, 1, 1);

								if (Inventory.WeaponAmmoText[i])
									Inventory.WeaponAmmoText[i].text = " ";
							}
						}

						break;
					}
					case "health":

						if (Inventory.HealthButton)
							Inventory.HealthButton.interactable = inventoryManager.HealthKits.Count > 0;

						if (Inventory.UpHealthButton)
							Inventory.HealthButton.interactable = inventoryManager.HealthKits.Count > 0;

						if (Inventory.DownHealthButton)
							Inventory.DownHealthButton.interactable = inventoryManager.HealthKits.Count > 0;

						if (Inventory.HealthKitsCount)
							Inventory.HealthKitsCount.text = inventoryManager.HealthKits.Count > 0 ? inventoryManager.currentHealthKit + 1 + "/" + inventoryManager.HealthKits.Count : "0";

						if (Inventory.CurrentHealthValue)
						{
							Inventory.CurrentHealthValue.gameObject.SetActive(inventoryManager.HealthKits.Count > 0);

							if (inventoryManager.HealthKits.Count > 0)
								Inventory.CurrentHealthValue.text = "+ " + inventoryManager.HealthKits[inventoryManager.currentHealthKit].AddedValue;
							else Inventory.CurrentHealthValue.text = " ";
						}

						if (Inventory.HealthImage)
							Inventory.HealthImage.color = new Color(1, 1, 1, inventoryManager.HealthKits.Count > 0 ? 1 : 0);

						foreach (var kit in inventoryManager.HealthKits)
						{
							if (inventoryManager.HealthKits.IndexOf(kit) == inventoryManager.currentHealthKit)
								if (Inventory.HealthImage)
									Inventory.HealthImage.texture = kit.Image;
						}

						break;

					case "ammo":

						if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame.Count > 0)
						{
							if (!inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].fistAttack)
							{
								var weaponController = inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].weapon.GetComponent<WeaponController>();

								if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count > 0 &&
								    weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee)
								{

									if (Inventory.AmmoButton)
										Inventory.AmmoButton.interactable = true;

									if (Inventory.UpAmmoButton)
										Inventory.UpAmmoButton.interactable = true;

									if (Inventory.DownAmmoButton)
										Inventory.DownAmmoButton.interactable = true;

									if (Inventory.AmmoKitsCount)
										Inventory.AmmoKitsCount.text = inventoryManager.currentAmmoKit + 1 + "/" + inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].WeaponAmmoKits.Count;

									if (Inventory.CurrentAmmoValue)
									{
										Inventory.CurrentAmmoValue.gameObject.SetActive(true);
										Inventory.CurrentAmmoValue.text = "+ " + inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].WeaponAmmoKits[inventoryManager.currentAmmoKit].AddedValue;
									}


									if (Inventory.AmmoImage)
										Inventory.AmmoImage.color = new Color(1, 1, 1, 1);
									Inventory.AmmoImage.texture = inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].WeaponAmmoKits[inventoryManager.currentAmmoKit].Image;

								}
								else NotActiveAmmoKits();

							}
							else NotActiveAmmoKits();

						}
						else NotActiveAmmoKits();

						break;
				}
			}

			private void NotActiveAmmoKits()
			{
				if (Inventory.AmmoButton)
					Inventory.AmmoButton.interactable = false;

				if (Inventory.UpAmmoButton)
					Inventory.UpAmmoButton.interactable = false;

				if (Inventory.DownAmmoButton)
					Inventory.DownAmmoButton.interactable = false;

				if (Inventory.AmmoKitsCount)
					Inventory.AmmoKitsCount.text = "0";

				if (Inventory.CurrentAmmoValue)
					Inventory.CurrentAmmoValue.gameObject.SetActive(false);

				if (Inventory.AmmoImage)
					Inventory.AmmoImage.color = new Color(1, 1, 1, 0);
			}
		}

		[Serializable]
		public class Inventory
		{
			public GameObject MainObject;

			public Button[] WeaponsButtons = new Button[8];
			public Text[] WeaponAmmoText = new Text[8];
			public RawImage[] WeaponImagePlaceholder = new RawImage[8];

			public Button UpWeaponButton;
			public Button DownWeaponButton;
			public Button UpHealthButton;
			public Button DownHealthButton;
			public Button UpAmmoButton;
			public Button DownAmmoButton;
			public Button AmmoButton;
			public Button HealthButton;

			public Text WeaponsCount;
			public Text AmmoKitsCount;
			public Text CurrentAmmoValue;
			public Text HealthKitsCount;
			public Text CurrentHealthValue;

			public RawImage HealthImage;
			public RawImage AmmoImage;

			public Color[] normButtonsColors = new Color[10];
			public Sprite[] normButtonsSprites = new Sprite[10];

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);
				
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(RawImage))
				// 	{
				// 		var go = (RawImage) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// }
			}
		}

		[Serializable]
		public class AllRoomsMenu
		{
			public GameObject MainObject;

			public ScrollRect scrollRect;

			public InputField Password;

			public Button BackButton;
			public Button JoinButton;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button> {BackButton, JoinButton};

				return buttons;
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (scrollRect)
				// 	scrollRect.gameObject.SetActive(false);
				//
				// if (BackButton)
				// 	BackButton.gameObject.SetActive(false);
				//
				// if (JoinButton)
				// 	JoinButton.gameObject.SetActive(false);
				//
				// if (Password)
				// 	Password.gameObject.SetActive(false);
			}

			public void ActivateAll()
			{
				if (MainObject)
					Helper.EnableAllParents(MainObject);

				if (scrollRect)
					Helper.EnableAllParents(scrollRect.gameObject);

				if (BackButton)
					Helper.EnableAllParents(BackButton.gameObject);
			}
		}

		[Serializable]
		public class MinimapImage
		{
			public Controller controller;
			public RawImage image;
			public RawImage upArrow;
			public RawImage downArrow;
			public bool isBlipComponent;
		}

		[Serializable]
		public class CreateRoomMenu
		{
			public GameObject MainObject;

			public Button BackButton;
			public Button CreateButton;

			public InputField GameName;
			public InputField Password;

			public Text currentMode;
			public Text currentMap;


			public Text maxPlayersText;
			public Slider maxPlayers;

			public Text gameDurationText;
			public Slider gameDuration;

			public Toggle canKillEachOther;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button> {BackButton, CreateButton};

				return buttons;
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);
			}
		}

		[Serializable]
		public class LobbyMainMenu
		{
			public GameObject MainObject;

			public Text ConnectionStatus;
			public Text CurrentModeAndMap;
			public Text currentMoney;
			public Text currentLevel;
			public Text nickname;

			public RawImage Avatar;

			public Dropdown RegionsDropdown;

			public InputField nicknameInputField;

			public Button ChooseGameModeButton;
			public Button PlayButton;
			public Button ChangeAvatarButton;
			public Button openProfileMenuButton;
			public Button ChangeCharacter;
			public Button LoadoutButton;
			public Button AllRoomsButton;
			public Button CreateRoomButton;
			public Button mapsButton;
			public Button settingsButton;
			public Button exitButton;

			public Button currentSelectedButton;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button>();

				foreach (var field in GetType().GetFields())
				{
					if (field.FieldType == typeof(Button))
					{
						var go = (Button) field.GetValue(this);
						if (go && go.name != "currentSelectedButton") buttons.Add(go);
					}
				}

				return buttons;
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
				
				if (RegionsDropdown)
					RegionsDropdown.gameObject.SetActive(false);
			}

			public void ActivateAll(bool activateRegionsDropdown)
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);

				if (RegionsDropdown && activateRegionsDropdown)
					Helper.EnableAllParents(RegionsDropdown.gameObject);
				
				if (openProfileMenuButton)
					openProfileMenuButton.interactable = true;
			}

			public void ActivatePlayerStats()
			{
				if (openProfileMenuButton)
					openProfileMenuButton.interactable = false;
				
				if (Avatar)
					Helper.EnableAllParents(Avatar.gameObject);
				
				if (nickname)
					Helper.EnableAllParents(nickname.gameObject);
				
				if (currentLevel)
					Helper.EnableAllParents(currentLevel.gameObject);
				
				if (currentMoney)
					Helper.EnableAllParents(currentMoney.gameObject);
			}
		}

		[Serializable]
		public class LobbyMapsUI
		{
			public GameObject MainObject;

			public ScrollRect scrollRect;

			public Button gameModesButton;
			public Button backButton;

			public Button currentSelectedButton;

			public Text GameModesButtonText;

			public bool firstTimeMenuOpened;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button> {gameModesButton, backButton};
				return buttons;
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) go.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(ScrollRect))
				// 	{
				// 		var go = (ScrollRect) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
				//
				// if (GameModesButtonText)
				// 	GameModesButtonText.gameObject.SetActive(false);

			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(), this);

				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(ScrollRect))
				// 	{
				// 		var go = (ScrollRect) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// }
				//
				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (GameModesButtonText)
				// 	Helper.EnableAllParents(GameModesButtonText.gameObject);
			}
		}

		[Serializable]
		public class LobbyCharactersMenu
		{
			public GameObject mainObject;

			public Button upButton;
			public Button downButton;
			public Button backButton;
			public Button interactionButton;

			public Text interactionButtonText;
			public Text requiredStatsAndStatus;

			public ScrollRect weaponsScrollRect;
			public RawImage weaponPlaceholder;

			public void ActivateAll()
			{

				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);
				
				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (UpButton)
				// 	Helper.EnableAllParents(UpButton.gameObject);
				//
				// if (DownButton)
				// 	Helper.EnableAllParents(DownButton.gameObject);
				//
				// if (BackButton)
				// 	Helper.EnableAllParents(BackButton.gameObject);
				//
				// if (weaponsScrollRect)
				// 	Helper.EnableAllParents(weaponsScrollRect.gameObject);
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (UpButton)
				// 	UpButton.gameObject.SetActive(false);
				//
				// if (DownButton)
				// 	DownButton.gameObject.SetActive(false);
				//
				// if (BackButton)
				// 	BackButton.gameObject.SetActive(false);
				//
				// if (weaponsScrollRect)
				// 	weaponsScrollRect.gameObject.SetActive(false);
			}
		}
		
		[Serializable]
		public class ProfileMenu
		{
			public GameObject mainObject;

			public Text money;
			public Text score;
			public Text level;

			public Text currentLevel;
			public Text nextLevel;
			public Text progress;

			public InputField nickNameInputField;

			public Image progressBarFill;
			public RawImage avatar;

			public Button changeAvatarButton;
			public Button backButton;
			
			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);
			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
			}
		}

		[Serializable]
		public class AvatarsMenu
		{
			public GameObject MainObject;

			public ScrollRect scrollRect;
			
			public Button BackButton;
			public Button currentSelectedButton;

			public bool firstTimeMenuOpened;

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
				
				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (scrollRect)
				// 	scrollRect.gameObject.SetActive(false);
				//
				// if (BackButton)
				// 	BackButton.gameObject.SetActive(false);
			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);
			}
		}

		[Serializable]
		public class MatchStats
		{
			public GameObject TeamsSurvivalMain;
			public GameObject TeamsMatchUIMain;
			public GameObject NotTeamsMatchUIMain;
			public GameObject NotTeamsSurvivalMain;
			public GameObject DominationMain;
			public GameObject HardPointMain;

			public ScrollRect KillDeathStatsScrollRect;

			public Transform firstTeamPlayersList;
			public Transform secondTeamPlayersList;
			public Transform playersList;

			public Text firstTeamMatchStats;
			public Text secondTeamMatchStats;
			public Text TargetText;
			public Text CurrentPlaceText;
			public Text FirstPlaceStats;
			public Text PlayerStats;
			public Text MatchTimer;
			public Text HardPointTimer;
			public Text AddScorePopup;

			public Image A_CurrentFill;
			public Image A_CapturedFill;
			public Image B_CurrentFill;
			public Image B_CapturedFill;
			public Image C_CurrentFill;
			public Image C_CapturedFill;
			public Image HardPoint_CurrentFill;
			public Image HardPoint_CapturedFill;


			public Texture A_ScreenTargetTexture;
			public Texture B_ScreenTargetTexture;
			public Texture C_ScreenTargetTexture;
			public Texture HardPoint_ScreenTargetTexture;

			public RawImage A_ScreenTarget;
			public RawImage B_ScreenTarget;
			public RawImage C_ScreenTarget;
			public RawImage FirstPlaceStatsBackground;
			public RawImage PlayerStatsBackground;
			public RawImage TeamImagePlaceholder;
			public RawImage firstTeamLogoPlaceholder;
			public RawImage secondTeamLogoPlaceholder;

			public RawImage firstTeamMatchStatsBackground;
			public RawImage secondTeamMatchStatsBackground;


			public Color32 PlayerStatsHighlight;


			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(RawImage))
				// 	{
				// 		var go = (RawImage) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Image))
				// 	{
				// 		var go = (Image) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
				//
				// if (KillDeathStatsScrollRect)
				// {
				// 	Helper.EnableAllParents(KillDeathStatsScrollRect.gameObject);
				// 	KillDeathStatsScrollRect.gameObject.SetActive(false);
				// }
			}

			public void ActivateDominationScreen()
			{
				CreateTargetUI(ref A_ScreenTarget, A_ScreenTargetTexture, DominationMain.transform);
				CreateTargetUI(ref B_ScreenTarget, B_ScreenTargetTexture, DominationMain.transform);
				CreateTargetUI(ref C_ScreenTarget, C_ScreenTargetTexture, DominationMain.transform);

				if (DominationMain)
					Helper.EnableAllParents(DominationMain);

				if (A_ScreenTarget)
					Helper.EnableAllParents(A_ScreenTarget.gameObject);

				if (B_ScreenTarget)
					Helper.EnableAllParents(B_ScreenTarget.gameObject);

				if (C_ScreenTarget)
					Helper.EnableAllParents(C_ScreenTarget.gameObject);

				if (A_CurrentFill)
					Helper.EnableAllParents(A_CurrentFill.gameObject);

				if (B_CurrentFill)
					Helper.EnableAllParents(B_CurrentFill.gameObject);

				if (C_CurrentFill)
					Helper.EnableAllParents(C_CurrentFill.gameObject);

				if (A_CapturedFill)
					Helper.EnableAllParents(A_CapturedFill.gameObject);

				if (B_CapturedFill)
					Helper.EnableAllParents(B_CapturedFill.gameObject);

				if (C_CapturedFill)
					Helper.EnableAllParents(C_CapturedFill.gameObject);
			}


			public void ActivateHardPointScreen()
			{
				CreateTargetUI(ref A_ScreenTarget, HardPoint_ScreenTargetTexture, HardPointMain.transform);

				if (HardPointMain)
					Helper.EnableAllParents(HardPointMain);

				if (HardPoint_CapturedFill)
					Helper.EnableAllParents(HardPoint_CapturedFill.gameObject);

				if (HardPointTimer)
					Helper.EnableAllParents(HardPointTimer.gameObject);
			}

			void CreateTargetUI(ref RawImage point, Texture texture, Transform parent)
			{
				if (!point && texture)
				{
					point = Helper.NewUIElement(texture.name, parent, Vector2.zero, new Vector2(70, 70), Vector3.one).AddComponent<RawImage>();
					point.raycastTarget = false;
					point.texture = texture;
				}
			}


			public void ActivateTeamScreen(string type)
			{
				if (MatchTimer)
					Helper.EnableAllParents(MatchTimer.gameObject);

				if (TargetText)
					Helper.EnableAllParents(TargetText.gameObject);

				if (type != "survival")
				{
					if (TeamsMatchUIMain)
						Helper.EnableAllParents(TeamsMatchUIMain);

					if (firstTeamMatchStats)
						Helper.EnableAllParents(firstTeamMatchStats.gameObject);

					if (secondTeamMatchStats)
						Helper.EnableAllParents(secondTeamMatchStats.gameObject);

					if (TeamImagePlaceholder)
						Helper.EnableAllParents(TeamImagePlaceholder.gameObject);
				}
				else
				{
					if (TeamsSurvivalMain)
						Helper.EnableAllParents(TeamsSurvivalMain);

					if (firstTeamPlayersList)
						Helper.EnableAllParents(firstTeamPlayersList.gameObject);

					if (secondTeamPlayersList)
						Helper.EnableAllParents(secondTeamPlayersList.gameObject);

					if (firstTeamLogoPlaceholder)
						Helper.EnableAllParents(firstTeamLogoPlaceholder.gameObject);

					if (secondTeamLogoPlaceholder)
						Helper.EnableAllParents(secondTeamLogoPlaceholder.gameObject);
				}
			}

			public void ActivateNotTeamScreen(string type)
			{
				if (MatchTimer)
					Helper.EnableAllParents(MatchTimer.gameObject);

				if (TargetText)
					Helper.EnableAllParents(TargetText.gameObject);

				if (type != "survival")
				{
					if (NotTeamsMatchUIMain)
						Helper.EnableAllParents(NotTeamsMatchUIMain);

					if (CurrentPlaceText)
						Helper.EnableAllParents(CurrentPlaceText.gameObject);

					if (FirstPlaceStats)
						Helper.EnableAllParents(FirstPlaceStats.gameObject);

					if (PlayerStats)
						Helper.EnableAllParents(PlayerStats.gameObject);

					if (FirstPlaceStatsBackground)
						Helper.EnableAllParents(FirstPlaceStatsBackground.gameObject);

					if (PlayerStatsBackground)
						Helper.EnableAllParents(PlayerStatsBackground.gameObject);
				}
				else
				{
					if (NotTeamsSurvivalMain)
						Helper.EnableAllParents(NotTeamsSurvivalMain);

					if (playersList)
						Helper.EnableAllParents(playersList.gameObject);
				}
			}

		}

		[Serializable]
		public class GameOverMenu
		{
			public GameObject teamsMainObject;
			public GameObject notTeamsMainObject;

			public Text teamsStatus;
			public Text notTeamsStatus;

			public Text firstTeamScore;
			public Text firstTeamScoreMeaning;
			public Text secondTeamScore;
			public Text secondTeamScoreMeaning;
			public Text firstTeamName;
			public Text secondTeamName;
			public Text roundStatusText;

			public Text moneyProfit;
			public Text newLevel;

			public RawImage firstTeamBackground;
			public RawImage secondTeamBackground;

			public RawImage victoryImage;
			public RawImage defeatImage;
			public RawImage drawImage;
			public RawImage firstTeamLogoPlaceholder;
			public RawImage secondTeamLogoPlaceholder;

			public Button playAgainButton;
			public Button exitButton;
			public Button matchStatsButton;
			public Button backButton;

			public GameOverPlayerInfo[] podiumPlaceholders = {new GameOverPlayerInfo(), new GameOverPlayerInfo(), new GameOverPlayerInfo()};


			public void ActivateNotTeamsScreen()
			{
				ActivateButtons();

				if (notTeamsMainObject)
					Helper.EnableAllParents(notTeamsMainObject);

				if (notTeamsStatus)
					Helper.EnableAllParents(notTeamsStatus.gameObject);

				podiumPlaceholders[0].ActivateAll();
				podiumPlaceholders[1].ActivateAll();
				podiumPlaceholders[2].ActivateAll();
			}

			public void ActivateTeamsScreen(MultiplayerHelper.Teams winnerTeam, MultiplayerHelper.Teams playerTeam, bool activeButtons)
			{
				if (activeButtons)
					ActivateButtons();

				if (teamsStatus)
					Helper.EnableAllParents(teamsStatus.gameObject);

				if (roundStatusText)
					Helper.EnableAllParents(roundStatusText.gameObject);

				if (teamsMainObject)
					Helper.EnableAllParents(teamsMainObject);

				if (firstTeamScore)
					Helper.EnableAllParents(firstTeamScore.gameObject);

				if (secondTeamScore)
					Helper.EnableAllParents(secondTeamScore.gameObject);

				if (firstTeamScoreMeaning)
					Helper.EnableAllParents(firstTeamScoreMeaning.gameObject);

				if (secondTeamScoreMeaning)
					Helper.EnableAllParents(secondTeamScoreMeaning.gameObject);

				if (firstTeamBackground)
					Helper.EnableAllParents(firstTeamBackground.gameObject);

				if (secondTeamBackground)
					Helper.EnableAllParents(secondTeamBackground.gameObject);

				if (secondTeamName)
					Helper.EnableAllParents(secondTeamName.gameObject);

				if (firstTeamName)
					Helper.EnableAllParents(firstTeamName.gameObject);

				if (firstTeamLogoPlaceholder)
					Helper.EnableAllParents(firstTeamLogoPlaceholder.gameObject);

				if (secondTeamLogoPlaceholder)
					Helper.EnableAllParents(secondTeamLogoPlaceholder.gameObject);

				if (winnerTeam != MultiplayerHelper.Teams.Null)
				{
					if (winnerTeam == playerTeam)
					{
						if (victoryImage)
							Helper.EnableAllParents(victoryImage.gameObject);
					}
					else
					{
						if (defeatImage)
							Helper.EnableAllParents(defeatImage.gameObject);
					}
				}
				else
				{
					if (drawImage)
						Helper.EnableAllParents(drawImage.gameObject);
				}

			}

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				podiumPlaceholders[0].DisableAll();
				podiumPlaceholders[1].DisableAll();
				podiumPlaceholders[2].DisableAll();
			}

			void ActivateButtons()
			{
				if (playAgainButton)
					Helper.EnableAllParents(playAgainButton.gameObject);

				if (exitButton)
					Helper.EnableAllParents(exitButton.gameObject);

				if (matchStatsButton)
					Helper.EnableAllParents(matchStatsButton.gameObject);
				
				if(moneyProfit)
					Helper.EnableAllParents(moneyProfit.gameObject);
				
				if(newLevel)
					Helper.EnableAllParents(newLevel.gameObject);
			}
		}

		[Serializable]
		public class PauseMenu
		{
			public GameObject teamsPauseMenuMain;
			public GameObject notTeamsPauseMenuMain;

			public ScrollRect secondTeamScrollRect;
			public ScrollRect firstTeamScrollRect;
			public ScrollRect notTeamsScrollRect;

			public Button exitButton;
			public Button resumeButton;
			public Button optionsButton;

			public Text firstTeamName;
			public Text secondTeamName;
			public Text firstTeamScore;
			public Text secondTeamScore;
			public Text firstTeamTotalWins;
			public Text secondTeamTotalWins;
			public Text currentGameAndPassword;

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
			}

			public void ActivateTeamsMenu(bool activeButtons, bool showMatchNameAndPassword)
			{
				if (activeButtons)
					ActivateButtons();

				if (showMatchNameAndPassword)
					if (currentGameAndPassword)
						Helper.EnableAllParents(currentGameAndPassword.gameObject);

				if (teamsPauseMenuMain)
					Helper.EnableAllParents(teamsPauseMenuMain);

				if (secondTeamScrollRect)
					Helper.EnableAllParents(secondTeamScrollRect.gameObject);

				if (firstTeamScrollRect)
					Helper.EnableAllParents(firstTeamScrollRect.gameObject);

				if (firstTeamName)
					Helper.EnableAllParents(firstTeamName.gameObject);

				if (secondTeamName)
					Helper.EnableAllParents(secondTeamName.gameObject);

				if (firstTeamScore)
					Helper.EnableAllParents(firstTeamScore.gameObject);

				if (secondTeamScore)
					Helper.EnableAllParents(secondTeamScore.gameObject);

				if (firstTeamTotalWins)
					Helper.EnableAllParents(firstTeamTotalWins.gameObject);

				if (secondTeamTotalWins)
					Helper.EnableAllParents(secondTeamTotalWins.gameObject);
			}

			public void ActivateNotTeamsMenu(bool activeButtons, bool showMatchNameAndPassword)
			{
				if (showMatchNameAndPassword)
					if (currentGameAndPassword)
						Helper.EnableAllParents(currentGameAndPassword.gameObject);

				if (activeButtons)
					ActivateButtons();

				if (notTeamsPauseMenuMain)
					Helper.EnableAllParents(notTeamsPauseMenuMain);

				if (notTeamsScrollRect)
					Helper.EnableAllParents(notTeamsScrollRect.gameObject);
			}

			void ActivateButtons()
			{
				if (exitButton)
					Helper.EnableAllParents(exitButton.gameObject);

				if (resumeButton)
					Helper.EnableAllParents(resumeButton.gameObject);

				if (optionsButton)
					Helper.EnableAllParents(optionsButton.gameObject);
			}
		}

		#region Advanced Multiplayer UI

		[Serializable]
		public class LobbyLoadoutMenu
		{
			public GameObject mainObject;

			public ScrollRect scrollRect;

			public Button equipButton;
			public Button backButton;

			public Button currentSelectedButton;

			public Text weaponInfo;
			public Text purchaseWarning;
			public Text equipButtonText;

			public bool firstTimeMenuOpened;

			public void DisableAll()
			{
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) go.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(ScrollRect))
				// 	{
				// 		var go = (ScrollRect) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
			}

			public void ActivateAll()
			{
			
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);

				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(ScrollRect))
				// 	{
				// 		var go = (ScrollRect) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// }
				//
				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
			}
		}

		[Serializable]
		public class LobbyGameModesUI
		{
			public GameObject MainObject;

			public ScrollRect scrollRect;

			public Text Info;
			public Text MapButtonText;

			public Button MapsButton;
			public Button BackButton;

			public Button currentSelectedButton;

			public bool firstTimeMenuOpened;

			public List<Button> GetAllButtons()
			{
				var buttons = new List<Button> {MapsButton, BackButton};

				return buttons;
			}

			public void DisableAll()
			{
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(GameObject))
				// 	{
				// 		var go = (GameObject) field.GetValue(this);
				// 		if (go) go.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
				//
				// if (scrollRect)
				// 	scrollRect.gameObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

			}

			public void ActivateAll()
			{
				
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);
				
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go && go.name != "currentSelectedButton") Helper.EnableAllParents(go.gameObject);
				// 	}
				// 	else if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) Helper.EnableAllParents(go.gameObject);
				// 	}
				// }
				//
				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (scrollRect)
				// 	Helper.EnableAllParents(scrollRect.gameObject);
			}
		}



		[Serializable]
		public class SpectateMenu
		{
			public GameObject MainObject;

			public Button MatchStatsButton;
			public Button ExitButton;
			public Button ChangeCameraButton;
			public Button BackButton;

			public Text PlayerStats;
//			public Text MatchTimer;

			public void DisableAll()
			{
				// foreach (var field in GetType().GetFields())
				// {
				// 	if (field.FieldType == typeof(Text))
				// 	{
				// 		var go = (Text) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// 	else if (field.FieldType == typeof(Button))
				// 	{
				// 		var go = (Button) field.GetValue(this);
				// 		if (go) go.gameObject.SetActive(false);
				// 	}
				// }
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

			}

			public void ActivateTeamsScreen()
			{
				ActivateAllButtons();

				if (PlayerStats)
					Helper.EnableAllParents(PlayerStats.gameObject);
				
			}

			public void ActivateNotTeamsScreen()
			{
				ActivateAllButtons();

				if (PlayerStats)
					Helper.EnableAllParents(PlayerStats.gameObject);
			}

			private void ActivateAllButtons()
			{
				if (MatchStatsButton)
					Helper.EnableAllParents(MatchStatsButton.gameObject);

				if (ExitButton)
					Helper.EnableAllParents(ExitButton.gameObject);

				if (ChangeCameraButton)
					Helper.EnableAllParents(ChangeCameraButton.gameObject);
			}
		}



		[Serializable]
		public class StartMenu
		{
			public GameObject MainObject;

			public ScrollRect PlayersContent;

			public Text FindPlayersStatsText;
			public Text FindPlayersTimer;

			public Button ExitButton;

			public void DisableAll()
			{
				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (PlayersContent)
				// 	PlayersContent.gameObject.SetActive(false);
				//
				// if (FindPlayersTimer)
				// 	FindPlayersTimer.gameObject.SetActive(false);
				//
				// if (FindPlayersStatsText)
				// 	FindPlayersStatsText.gameObject.SetActive(false);
				//
				// if (ExitButton)
				// 	ExitButton.gameObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
			}

			public void ActivateScreen()
			{
				if (MainObject)
					Helper.EnableAllParents(MainObject);

				if (PlayersContent)
					Helper.EnableAllParents(PlayersContent.gameObject);

				if (FindPlayersTimer)
					Helper.EnableAllParents(FindPlayersTimer.gameObject);

				if (FindPlayersStatsText)
					Helper.EnableAllParents(FindPlayersStatsText.gameObject);

				if (ExitButton)
					Helper.EnableAllParents(ExitButton.gameObject);
			}
		}

		[Serializable]
		public class TimerBeforeMatch
		{
			public GameObject MainObject;

			public RawImage Background;

			public Text StartMatchTimer;

			public void DisableAll()
			{
				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (Background)
				// 	Background.gameObject.SetActive(false);
				//
				// if (StartMatchTimer)
				// 	StartMatchTimer.gameObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);

				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (Background)
				// 	Helper.EnableAllParents(Background.gameObject);
				//
				// if (StartMatchTimer)
				// 	Helper.EnableAllParents(StartMatchTimer.gameObject);
			}
		}

		[Serializable]
		public class TimerAfterDeath
		{
			public GameObject MainObject;

			public Button LaunchButton;
			public Text RestartTimer;

			public void DisableAll()
			{
				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (LaunchButton)
				// 	LaunchButton.gameObject.SetActive(false);
				//
				// if (RestartTimer)
				// 	RestartTimer.gameObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);
				
				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (LaunchButton)
				// 	Helper.EnableAllParents(LaunchButton.gameObject);
				//
				// if (RestartTimer)
				// 	Helper.EnableAllParents(RestartTimer.gameObject);
			}
		}

		[Serializable]
		public class GameOverPlayerInfo
		{
			public GameObject mainObject;

			public Text nickname;
			public Text score;
			public RawImage avatar;

			public void DisableAll()
			{
				// if (nickname)
				// 	nickname.gameObject.SetActive(false);
				//
				// if (score)
				// 	score.gameObject.SetActive(false);
				//
				// if (mainObject)
				// 	mainObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);

			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);

				// if (nickname)
				// 	Helper.EnableAllParents(nickname.gameObject);
				//
				// if (score)
				// 	Helper.EnableAllParents(score.gameObject);
				//
				// if (mainObject)
				// 	Helper.EnableAllParents(mainObject);
			}
		}

		[Serializable]
		public class PreMatchMenu
		{
			public GameObject MainObject;

			public Text Status;

			public void DisableAll()
			{
				// if (MainObject)
				// 	MainObject.SetActive(false);
				//
				// if (Status)
				// 	Status.gameObject.SetActive(false);
				
				UIHelper.DisableAll(GetType().GetFields().ToList(), this);
			}

			public void ActivateAll()
			{
				UIHelper.ActivateAll(GetType().GetFields().ToList(),this);

				// if (MainObject)
				// 	Helper.EnableAllParents(MainObject);
				//
				// if (Status)
				// 	Helper.EnableAllParents(Status.gameObject);
			}
		}

		#endregion
		
		
		private static void ActivateAll(List<FieldInfo> fields, object target)
		{
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(GameObject))
				{
					var go = (GameObject) field.GetValue(target);
					if (go) Helper.EnableAllParents(go);
				}
				if (field.FieldType == typeof(Text))
				{
					var go = (Text) field.GetValue(target);
					if (go) Helper.EnableAllParents(go.gameObject);
				}
				else if (field.FieldType == typeof(Button))
				{
					var go = (Button) field.GetValue(target);
					if (go && go.name != "currentSelectedButton") Helper.EnableAllParents(go.gameObject);
				}
				else if (field.FieldType == typeof(RawImage))
				{
					var go = (RawImage) field.GetValue(target);
					if (go) Helper.EnableAllParents(go.gameObject);
				}
				else if (field.FieldType == typeof(ScrollRect))
				{
					var go = (ScrollRect) field.GetValue(target);
					if (go) Helper.EnableAllParents(go.gameObject);
				}
				else if (field.FieldType == typeof(InputField))
				{
					var go = (InputField) field.GetValue(target);
					if (go) Helper.EnableAllParents(go.gameObject);
				}
			}
		}
		
		private static void DisableAll(List<FieldInfo> fields, object target)
		{
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(GameObject))
				{
					var go = (GameObject) field.GetValue(target);
					if (go) go.SetActive(false);
				}
				if (field.FieldType == typeof(Text))
				{
					var go = (Text) field.GetValue(target);
					if (go) go.gameObject.SetActive(false);
				}
				else if (field.FieldType == typeof(Button))
				{
					var go = (Button) field.GetValue(target);
					if (go) go.gameObject.SetActive(false);
				}
				else if (field.FieldType == typeof(RawImage))
				{
					var go = (RawImage) field.GetValue(target);
					if (go) go.gameObject.SetActive(false);
				}
				else if (field.FieldType == typeof(ScrollRect))
				{
					var go = (ScrollRect) field.GetValue(target);
					if (go) go.gameObject.SetActive(false);
				}
				else if (field.FieldType == typeof(InputField))
				{
					var go = (InputField) field.GetValue(target);
					if (go) go.gameObject.SetActive(false);
				}
			}
		}

		public static void CalculateCrosshairPartsPositions(Controller controller, float crosshairMultiplier, ref Vector2[] currentCrosshairPositions)
		{
			if (Math.Abs(crosshairMultiplier - 1) > 0.1f)
			{
				currentCrosshairPositions[3] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[3].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[3].x * crosshairMultiplier, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[3].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[3].y * crosshairMultiplier, 5 * Time.deltaTime));

				currentCrosshairPositions[4] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[4].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[4].x * crosshairMultiplier, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[4].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[4].y * crosshairMultiplier, 5 * Time.deltaTime));

				currentCrosshairPositions[1] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[1].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[1].x * crosshairMultiplier, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[1].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[1].y * crosshairMultiplier, 5 * Time.deltaTime));

				currentCrosshairPositions[2] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[2].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[2].x * crosshairMultiplier, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[2].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[2].y * crosshairMultiplier, 5 * Time.deltaTime));

			}
			else
			{
				currentCrosshairPositions[3] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[3].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[3].x, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[3].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[3].y, 5 * Time.deltaTime));

				currentCrosshairPositions[4] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[4].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[4].x, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[4].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[4].y, 5 * Time.deltaTime));

				currentCrosshairPositions[1] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[1].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[1].x, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[1].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[1].y, 5 * Time.deltaTime));

				currentCrosshairPositions[2] = new Vector2(
					Mathf.Lerp(currentCrosshairPositions[2].x, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[2].x, 5 * Time.deltaTime),
					Mathf.Lerp(currentCrosshairPositions[2].y, controller.inventoryManager.WeaponController.Attacks[controller.inventoryManager.WeaponController.currentAttack].crosshairPartsPositions[2].y, 5 * Time.deltaTime));
			}
		}

		public static void GetScreenTarget(ref Vector3 screenPosition, ref float angle, Vector3 screenCentre, Vector3 screenBounds)
		{
			screenPosition -= screenCentre;

			if (screenPosition.z < 0)
			{
				screenPosition *= -1;
			}

			angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
			float slope = Mathf.Tan(angle);

			if (screenPosition.x > 0)
			{
				screenPosition = new Vector3(screenBounds.x, screenBounds.x * slope, 0);
			}
			else
			{
				screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * slope, 0);
			}

			if (screenPosition.y > screenBounds.y)
			{
				screenPosition = new Vector3(screenBounds.y / slope, screenBounds.y, 0);
			}
			else if (screenPosition.y < -screenBounds.y)
			{
				screenPosition = new Vector3(-screenBounds.y / slope, -screenBounds.y, 0);
			}

			screenPosition += screenCentre;
		}

		public static void SetMobileButtons(Controller controller, GameManager gameManager)
		{
			Helper.AddButtonsEvents(controller.UIManager.uiButtons, controller.inventoryManager, controller);

			if (gameManager)
			{
				if (controller.UIManager.uiButtons[9])
				{
					controller.UIManager.uiButtons[9].onClick.AddListener(delegate { gameManager.Pause(true); });
				}

				// if (controller.UIManager.uiButtons[12])
				// {
				// 	controller.UIManager.uiButtons[12].onClick.AddListener(gameManager.SwitchCharacter);
				// }
			}

			ManageUIButtons(controller, controller.inventoryManager, controller.UIManager, controller.CharacterSync);
		}

		public static void ManageUIButtons(Controller controller, InventoryManager manager, UIManager uiManager, bool isMultiplayer)
		{
			if (!controller.UIManager) return;

			var weaponController = manager.WeaponController;
			var mainConditions = IsMobile(controller.projectSettings) && !manager.inventoryIsOpened && !controller.isPause && !controller.inCar;
			bool activityConditions;


			if (uiManager.moveStick && uiManager.moveStickOutline)
			{
				uiManager.moveStick.gameObject.SetActive(mainConditions);
				uiManager.moveStickOutline.gameObject.SetActive(mainConditions);
			}

			//aim
			if (uiManager.uiButtons[0])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[5] && weaponController && weaponController.activeAimMode && controller.TypeOfCamera != CharacterHelper.CameraType.TopDown;

				uiManager.uiButtons[0].gameObject.SetActive(activityConditions);
			}

			//reload
			if (uiManager.uiButtons[1])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[4] && manager.allWeaponsCount > 0 &&
				                     !manager.slots[manager.currentSlot].weaponSlotInGame[manager.slots[manager.currentSlot].currentWeaponInSlot].fistAttack &&
				                     weaponController && weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade &&
				                     weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee;

				uiManager.uiButtons[1].gameObject.SetActive(activityConditions);
			}

			//change camera type
			if (uiManager.uiButtons[2])
			{
				int countOfCameras;

				if (controller.CameraParameters.activeFP && (controller.CameraParameters.activeTP || controller.CameraParameters.activeTD))
					countOfCameras = 2;
				else if (controller.CameraParameters.activeTP && (controller.CameraParameters.activeFP || controller.CameraParameters.activeTD))
					countOfCameras = 2;
				else if (controller.CameraParameters.activeTD && (controller.CameraParameters.activeFP || controller.CameraParameters.activeTP))
					countOfCameras = 2;
				else countOfCameras = 1;

				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[11] && countOfCameras == 2;

				uiManager.uiButtons[2].gameObject.SetActive(activityConditions);
			}

			//change attack type
			if (uiManager.uiButtons[3])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[19] && weaponController && (weaponController.Attacks.Count > 1 ||
				                                                                                                                      weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Bullets &&
				                                                                                                                      weaponController.Attacks[weaponController.currentAttack].BulletsSettings[0].Active && weaponController.Attacks[weaponController.currentAttack].BulletsSettings[1].Active);

				uiManager.uiButtons[3].gameObject.SetActive(activityConditions);
			}

			//drop weapon
			if (uiManager.uiButtons[4])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[9] && manager.slots[manager.currentSlot].weaponSlotInGame.Count > 0 && !manager.slots[manager.currentSlot].weaponSlotInGame[manager.slots[manager.currentSlot].currentWeaponInSlot].fistAttack
				                     && weaponController && weaponController.Attacks[weaponController.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade;

				uiManager.uiButtons[4].gameObject.SetActive(activityConditions);
			}

			//attack
			if (uiManager.uiButtons[5])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[3] && (manager.allWeaponsCount > 0 || manager.HasFistAttack) && (weaponController && !weaponController.Attacks[weaponController.currentAttack].autoAttack || !weaponController);

				uiManager.uiButtons[5].gameObject.SetActive(activityConditions);
			}

			//sprint
			if (uiManager.uiButtons[6])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[0] && !controller.projectSettings.runWithJoystick;

				uiManager.uiButtons[6].gameObject.SetActive(activityConditions);
			}

			//crouch
			if (uiManager.uiButtons[7])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[1] && controller.TypeOfCamera != CharacterHelper.CameraType.TopDown;

				uiManager.uiButtons[7].gameObject.SetActive(activityConditions);
			}

			//jump
			if (uiManager.uiButtons[8])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[2];

				uiManager.uiButtons[8].gameObject.SetActive(activityConditions);
			}

			//pause
			if (uiManager.uiButtons[9])
			{
				activityConditions = IsMobile(controller.projectSettings) && !controller.isPause && !controller.inCar && !manager.inventoryIsOpened && controller.projectSettings.ButtonsActivityStatuses[10];

				uiManager.uiButtons[9].gameObject.SetActive(activityConditions);
			}

			//open/close inventory
			if (uiManager.uiButtons[10])
			{
				activityConditions = IsMobile(controller.projectSettings) && !controller.inCar && !controller.isPause && controller.projectSettings.ButtonsActivityStatuses[7];

				uiManager.uiButtons[10].gameObject.SetActive(activityConditions);
			}

			//switch character
			if (uiManager.uiButtons[12])
			{
				activityConditions = mainConditions && !isMultiplayer && controller.projectSettings.ButtonsActivityStatuses[18] && Object.FindObjectOfType<GameManager>() && Object.FindObjectOfType<GameManager>().controllers.Count > 1;

				uiManager.uiButtons[12].gameObject.SetActive(false); //activityConditions);
			}

			//Change TP Movement Type / TD Mode
			if (uiManager.uiButtons[16])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[20] && controller.TypeOfCamera != CharacterHelper.CameraType.FirstPerson;

				uiManager.uiButtons[16].gameObject.SetActive(activityConditions);
			}

			//weapon up
			if (uiManager.uiButtons[13])
			{

				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[16] && manager.allWeaponsCount > 1;

				uiManager.uiButtons[13].gameObject.SetActive(activityConditions);
			}

			//weapon down
			if (uiManager.uiButtons[14])
			{
				activityConditions = mainConditions && controller.projectSettings.ButtonsActivityStatuses[16] && manager.allWeaponsCount > 1;

				uiManager.uiButtons[14].gameObject.SetActive(activityConditions);
			}
		}

		static bool IsMobile(ProjectSettings projectSettings)
		{
			return Application.isMobilePlatform || projectSettings.mobileDebug;
		}

		public static float GetScale(int width, int height, Vector2 scalerReferenceResolution, float scalerMatchWidthOrHeight)
		{
			return Mathf.Pow(width / scalerReferenceResolution.x, 1f - scalerMatchWidthOrHeight) *
			       Mathf.Pow(height / scalerReferenceResolution.y, scalerMatchWidthOrHeight);
		}

		public static MinimapImage CreateNewBlip(UIManager uiManager, ref RawImage blipRawImage, Texture blipTexture, Color color, string name, bool createHeightDetection)
		{
			if (blipTexture && uiManager.CharacterUI.mapMask)
			{
				blipRawImage = Helper.NewUIElement(name, uiManager.CharacterUI.mapMask.transform, Vector2.zero, Vector2.zero, Vector3.one).gameObject.AddComponent<RawImage>();
				blipRawImage.rectTransform.sizeDelta = new Vector2(30, 30);

				blipRawImage.texture = blipTexture;

				var _color = color;
				_color.a = 1;
				blipRawImage.color = _color;
			}


			if (createHeightDetection)
			{
				var upArrowImg = Resources.Load("UpBlipArrow", typeof(Texture)) as Texture;
				var downArrowImg = Resources.Load("DownBlipArrow", typeof(Texture)) as Texture;
				var upArrow = CreateAdditionalArrows(upArrowImg, blipRawImage.rectTransform, "upArrow");
				var downArrow = CreateAdditionalArrows(downArrowImg, blipRawImage.rectTransform, "downArrow");

				return new MinimapImage {image = blipRawImage, isBlipComponent = true, downArrow = downArrow, upArrow = upArrow};
			}

			return new MinimapImage {image = blipRawImage, isBlipComponent = true};
		}

		private static RawImage CreateAdditionalArrows(Texture arrowTexture, RectTransform parent, string name)
		{
			var image = Helper.NewUIElement(name, parent, new Vector2(15, 0), new Vector2(10, 10), Vector3.one).gameObject.AddComponent<RawImage>();
			image.texture = arrowTexture;

			return image;
		}

#if USK_MULTIPLAYER
		public static void InitializeLobbyUI(LobbyManager lobbyManager)
		{
			var currentUIManager = lobbyManager.currentUIManager;

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton.onClick.AddListener(lobbyManager.RandomRoomClick);
			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("allRooms"); });
			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("createGame"); });
			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.nicknameInputField) currentUIManager.basicMultiplayerGameLobby.MainMenu.nicknameInputField.onValueChanged.AddListener(lobbyManager.SetName);
			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ChangeAvatarButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.ChangeAvatarButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("avatars"); });

			if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers)
			{
				currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers.onValueChanged.AddListener(lobbyManager.UpdateMaxPlayers);
				lobbyManager.UpdateMaxPlayers(5);
			}

			if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDuration)
			{
				currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.gameDuration.onValueChanged.AddListener(lobbyManager.UpdateMatchDuration);
				lobbyManager.UpdateMatchDuration(2);
			}

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ChangeCharacter) currentUIManager.basicMultiplayerGameLobby.MainMenu.ChangeCharacter.onClick.AddListener(delegate { lobbyManager.OpenMenu("characters"); });
			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.mapsButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.mapsButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("maps"); });
			if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.CreateButton) currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.CreateButton.onClick.AddListener(lobbyManager.CreateRoomClick);

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.exitButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.exitButton.onClick.AddListener(lobbyManager.CloseApp);

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.settingsButton) currentUIManager.basicMultiplayerGameLobby.MainMenu.settingsButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("settings"); });
			if (currentUIManager.gameOptions.back) currentUIManager.gameOptions.back.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });

			if (currentUIManager.basicMultiplayerGameLobby.MapsMenu.backButton) currentUIManager.basicMultiplayerGameLobby.MapsMenu.backButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });
			if (currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.BackButton) currentUIManager.basicMultiplayerGameLobby.AllRoomsMenu.BackButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });
			if (currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.BackButton) currentUIManager.basicMultiplayerGameLobby.CreateRoomMenu.BackButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });

			if (currentUIManager.basicMultiplayerGameLobby.CharactersMenu.backButton) currentUIManager.basicMultiplayerGameLobby.CharactersMenu.backButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });
			if (currentUIManager.basicMultiplayerGameLobby.CharactersMenu.upButton) currentUIManager.basicMultiplayerGameLobby.CharactersMenu.upButton.onClick.AddListener(delegate { lobbyManager.ChangeCharacter("+"); });
			if (currentUIManager.basicMultiplayerGameLobby.CharactersMenu.downButton) currentUIManager.basicMultiplayerGameLobby.CharactersMenu.downButton.onClick.AddListener(delegate { lobbyManager.ChangeCharacter("-"); });

			if (currentUIManager.basicMultiplayerGameLobby.AvatarsMenu.BackButton) currentUIManager.basicMultiplayerGameLobby.AvatarsMenu.BackButton.onClick.AddListener(delegate { lobbyManager.OpenMenu("mainMenu"); });

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown)
			{
				currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.ClearOptions();
				currentUIManager.basicMultiplayerGameLobby.MainMenu.RegionsDropdown.AddOptions(MultiplayerHelper.PhotonRegions);
			}

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton)
				currentUIManager.basicMultiplayerGameLobby.MainMenu.PlayButton.interactable = false;

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton)
				currentUIManager.basicMultiplayerGameLobby.MainMenu.AllRoomsButton.interactable = false;

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton)
				currentUIManager.basicMultiplayerGameLobby.MainMenu.CreateRoomButton.interactable = false;

			if (currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus)
				currentUIManager.basicMultiplayerGameLobby.MainMenu.ConnectionStatus.text = "Disconnected from Server";

			if (currentUIManager.basicMultiplayerGameLobby.avatarPlaceholder)
				currentUIManager.basicMultiplayerGameLobby.avatarPlaceholder.gameObject.SetActive(false);

			for (var i = 0; i < lobbyManager.defaultAvatars.Count; i++)
			{
				var avatar = lobbyManager.defaultAvatars[i];

				if (avatar && currentUIManager.basicMultiplayerGameLobby.avatarPlaceholder && currentUIManager.basicMultiplayerGameLobby.AvatarsMenu.scrollRect)
				{
					var placeholder = Object.Instantiate(currentUIManager.basicMultiplayerGameLobby.avatarPlaceholder.gameObject, currentUIManager.basicMultiplayerGameLobby.AvatarsMenu.scrollRect.content).GetComponent<UIPreset>();

					lobbyManager.allAvatarsPlaceholders.Add(placeholder);

					placeholder.gameObject.SetActive(true);

					if (placeholder.SelectionIndicator)
						placeholder.SelectionIndicator.gameObject.SetActive(false);

					if (placeholder.ImagePlaceholder)
						placeholder.ImagePlaceholder.texture = avatar;

					var i1 = i;

					if (placeholder.Button)
						placeholder.Button.onClick.AddListener(delegate { lobbyManager.SetAvatar(i1); });
				}
			}
		}
#endif

		public static void CreateScopeFill(string name, Transform parent, ref RectTransform fill, ref Image imageComponent)
		{
			
			var newObj = new GameObject(name);
			imageComponent = newObj.gameObject.AddComponent<Image>();
			newObj.gameObject.layer = 5;
			newObj.transform.SetParent(parent);
			fill = newObj.GetComponent<RectTransform>();
			newObj.transform.localScale = Vector3.one;
		}

		public static void SetFillColor(Image imageComponent, float alpha)
		{
			if (imageComponent)
			{
				
				var color1 = imageComponent.color;
				color1.a = alpha;
				imageComponent.color = color1;
			}
		}

#if UNITY_EDITOR
		public enum MenuPages
		{
			MainMenu,
			OptionsMenu,

			CharacterUI,
			CharacterInGame,
			CharacterInventory,
			MobileInput,

			SinglePlayer,
			SinglePause,
			SingleGameOver,

			AdvancedMultiplayer,
			AdvancedMultiplayerLobby,
			AdvancedMultiplayerLobbyMainMenu,
			AdvancedMultiplayerLobbyGameModes,
			AdvancedMultiplayerLobbyMaps,
			AdvancedMultiplayerLobbyLoadout,
			AdvancedMultiplayerLobbyAvatars,
			AdvancedMultiplayerLobbyCharacters,
			AdvancedMultiplayerLobbyAllRooms,
			AdvancedMultiplayerLobbyCreateRoom,
			AdvancedMultiplayerLobbyProfile,

			AdvancedMultiplayerRoom,
			AdvancedMultiplayerRoomStart,
			AdvancedMultiplayerRoomPause,
			AdvancedMultiplayerRoomGameOver,
			AdvancedMultiplayerRoomPreMatch,
			AdvancedMultiplayerRoomMatch,
			AdvancedMultiplayerRoomDeathScreens,

			Settings,

			BasicMultiplayer,
			BasicMultiplayerLobby,
			BasicMultiplayerLobbyMainMenu,
			BasicMultiplayerLobbyCharacters,
			BasicMultiplayerLobbyMaps,
			BasicMultiplayerLobbyAllRooms,
			BasicMultiplayerLobbyCreateRoom,
			BasicMultiplayerLobbyAvatars,

			BasicMultiplayerRoom,
			BasicMultiplayerRoomPause,
			BasicMultiplayerRoomMatch,
			BasicMultiplayerRoomGameOver
		}

		public static class SceneHierarchyUtility
		{
			private static List<GameObject> GetExpandedGameObjects()
			{
				object sceneHierarchy = GetSceneHierarchy();

				MethodInfo methodInfo = sceneHierarchy.GetType().GetMethod("GetExpandedGameObjects");

				object result = methodInfo.Invoke(sceneHierarchy, new object[0]);

				return (List<GameObject>) result;
			}

			public static void SetExpanded(GameObject go, bool expand)
			{
				var sceneHierarchy = GetSceneHierarchy();
				var methodInfo = sceneHierarchy.GetType().GetMethod("ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);

				methodInfo.Invoke(sceneHierarchy, new object[] {go.GetInstanceID(), expand});

				// Selection.activeGameObject = go;
			}

			public static void SetExpandedRecursive(GameObject go, bool expand)
			{
				var sceneHierarchy = GetSceneHierarchy();
				var methodInfo = sceneHierarchy.GetType().GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);

				methodInfo.Invoke(sceneHierarchy, new object[] {go.GetInstanceID(), expand});
			}

			private static object GetSceneHierarchy()
			{
				EditorWindow window = GetHierarchyWindow();

				object sceneHierarchy = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow").GetProperty("sceneHierarchy")?.GetValue(window);

				return sceneHierarchy;
			}

			private static EditorWindow GetHierarchyWindow()
			{
				EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
				return EditorWindow.focusedWindow;
			}
		}

		public static void OpenPagesInPlayMode(UIManager script)
		{
			CanMenuBeOpen(script.hierarchy[13], UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu, script);
			CanMenuBeOpen(script.hierarchy[14], UIHelper.MenuPages.BasicMultiplayerLobbyCharacters, script);
			CanMenuBeOpen(script.hierarchy[15], UIHelper.MenuPages.BasicMultiplayerLobbyMaps, script);
			CanMenuBeOpen(script.hierarchy[16], UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms, script);
			CanMenuBeOpen(script.hierarchy[17], UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom, script);

			CanMenuBeOpen(script.hierarchy[21], UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu, script);
			CanMenuBeOpen(script.hierarchy[22], UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms, script);
			CanMenuBeOpen(script.hierarchy[23], UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom, script);
			CanMenuBeOpen(script.hierarchy[24], UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes, script);
			CanMenuBeOpen(script.hierarchy[25], UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps, script);
			CanMenuBeOpen(script.hierarchy[51], UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile, script);
			CanMenuBeOpen(script.hierarchy[26], UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars, script);
			CanMenuBeOpen(script.hierarchy[27], UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout, script);
			CanMenuBeOpen(script.hierarchy[28], UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters, script);

			CanMenuBeOpen(script.CharacterUI.MainObject, UIHelper.MenuPages.CharacterInGame, script);
			CanMenuBeOpen(script.CharacterUI.Inventory.MainObject, UIHelper.MenuPages.CharacterInventory, script);

			CanMenuBeOpen(script.hierarchy[8], UIHelper.MenuPages.SinglePause, script);
			CanMenuBeOpen(script.hierarchy[10], UIHelper.MenuPages.SingleGameOver, script);

			CanMenuBeOpen(script.hierarchy[9], UIHelper.MenuPages.OptionsMenu, script);

			CanMenuBeOpen(script.hierarchy[29], UIHelper.MenuPages.AdvancedMultiplayerRoomStart, script);

			CanMenuBeOpen(script.hierarchy[36], UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch, script);
			// CanMenuBeOpen(script.hierarchy[37], UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch, script);
			// CanMenuBeOpen(script.hierarchy[38], UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch, script);

			CanMenuBeOpen(script.hierarchy[42], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);

			// CanMenuBeOpen(script.hierarchy[43], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);
			// CanMenuBeOpen(script.hierarchy[44], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);
			// CanMenuBeOpen(script.hierarchy[45], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);
			// CanMenuBeOpen(script.hierarchy[46], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);
			// CanMenuBeOpen(script.hierarchy[47], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);
			// CanMenuBeOpen(script.hierarchy[48], UIHelper.MenuPages.AdvancedMultiplayerRoomMatch, script);

			CanMenuBeOpen(script.hierarchy[39], UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens, script);
			// CanMenuBeOpen(script.hierarchy[40], UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens, script);
			// CanMenuBeOpen(script.hierarchy[41], UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens, script);

			CanMenuBeOpen(script.hierarchy[33], UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver, script);
			// CanMenuBeOpen(script.hierarchy[34], UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver, script);
			// CanMenuBeOpen(script.hierarchy[35], UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver, script);

			CanMenuBeOpen(script.hierarchy[30], UIHelper.MenuPages.AdvancedMultiplayerRoomPause, script);

			// CanMenuBeOpen(script.hierarchy[31], UIHelper.MenuPages.AdvancedMultiplayerRoomPause, script);
			// CanMenuBeOpen(script.hierarchy[32], UIHelper.MenuPages.AdvancedMultiplayerRoomPause, script);

			CanMenuBeOpen(script.hierarchy[18], UIHelper.MenuPages.BasicMultiplayerRoomPause, script);

			CanMenuBeOpen(script.hierarchy[1], UIHelper.MenuPages.MobileInput, script);

			if (script.currentMenuPage != script.previousMenuPage || script.teamsIndex != script.lastTeamsIndex || script.preMatchMenuIndex != script.preMatchMenuLastIndex
			    || script.deathScreensIndex != script.deathScreensLastIndex || script.roomMatchStatsLastTab != script.roomMatchStatsTab)
			{
				// foreach (var menu in script.currentMenusInGame)
				// {
				OpenItem(script, script.currentMenuPage, ref script.previousMenuPage);
				// }
			}
		}

		static void CanMenuBeOpen(GameObject mainObject, MenuPages menuPage, UIManager script)
		{
			if (mainObject.activeInHierarchy)
			{
				// script.currentMenuPage = menuPage;
				if (!script.currentMenusInGame.Exists(page => page == menuPage))
					script.currentMenusInGame.Add(menuPage);
			}
			else
			{
				if (script.currentMenusInGame.Exists(page => page == menuPage))
					script.currentMenusInGame.Remove(menuPage);
			}
		}

		public static void OpenItem(UIManager script, MenuPages currentMenuPage, ref MenuPages previousMenuPage)
		{
			previousMenuPage = currentMenuPage;
			script.lastTeamsIndex = script.teamsIndex;
			script.preMatchMenuLastIndex = script.preMatchMenuIndex;
			script.deathScreensLastIndex = script.deathScreensIndex;
			script.roomMatchStatsLastTab = script.roomMatchStatsTab;

			if (script.advancedMultiplayerGameRoom.playerInfoPlaceholder)
			{
				if (currentMenuPage == MenuPages.AdvancedMultiplayerRoomPause)
				{
					if (script.teamsIndex == 1)
					{
						if (script.advancedMultiplayerGameRoom.PauseMenu.notTeamsScrollRect)
						{
							script.advancedMultiplayerGameRoom.playerInfoPlaceholder.transform.SetParent(script.advancedMultiplayerGameRoom.PauseMenu.notTeamsScrollRect.content);
						}
					}
					else
					{
						if (script.advancedMultiplayerGameRoom.PauseMenu.firstTeamScrollRect)
							script.advancedMultiplayerGameRoom.playerInfoPlaceholder.transform.SetParent(script.advancedMultiplayerGameRoom.PauseMenu.firstTeamScrollRect.content);
					}
				}
				else if (currentMenuPage == MenuPages.AdvancedMultiplayerRoomStart)
				{
					if (script.advancedMultiplayerGameRoom.StartMenu.PlayersContent)
						script.advancedMultiplayerGameRoom.playerInfoPlaceholder.transform.SetParent(script.advancedMultiplayerGameRoom.StartMenu.PlayersContent.content);
				}

				script.advancedMultiplayerGameRoom.playerInfoPlaceholder.UpdateWidth();
			}

			script.HideAllHierarchy();

			switch (currentMenuPage)
			{
				case UIHelper.MenuPages.MainMenu:
					break;
				case UIHelper.MenuPages.CharacterUI:
					EnableHierarchyItem(new[] {0}, script);
					break;
				case UIHelper.MenuPages.CharacterInGame:
					EnableHierarchyItem(new[] {0, 6}, script);
					break;
				case UIHelper.MenuPages.CharacterInventory:
					EnableHierarchyItem(new[] {0, 7}, script);
					break;
				case UIHelper.MenuPages.MobileInput:
					EnableHierarchyItem(new[] {1}, script);
					break;
				case UIHelper.MenuPages.SinglePlayer:
					EnableHierarchyItem(new[] {2}, script);
					break;
				case UIHelper.MenuPages.SinglePause:
					EnableHierarchyItem(new[] {2, 8}, script);
					break;
				case UIHelper.MenuPages.OptionsMenu:
					EnableHierarchyItem(new[] {9}, script);
					break;
				case UIHelper.MenuPages.SingleGameOver:
					EnableHierarchyItem(new[] {2, 10}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayer:
					EnableHierarchyItem(new[] {4}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobby:
					EnableHierarchyItem(new[] {4, 19}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:
					EnableHierarchyItem(new[] {4, 19, 21}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:
					EnableHierarchyItem(new[] {4, 19, 24}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:
					EnableHierarchyItem(new[] {4, 19, 25}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
					EnableHierarchyItem(new[] {4, 19, 51}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:
					EnableHierarchyItem(new[] {4, 19, 27}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:
					EnableHierarchyItem(new[] {4, 19, 26}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:
					EnableHierarchyItem(new[] {4, 19, 28}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:
					EnableHierarchyItem(new[] {4, 19, 22}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:
					EnableHierarchyItem(new[] {4, 19, 23}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoom:
					EnableHierarchyItem(new[] {4, 20}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:
					EnableHierarchyItem(new[] {4, 20, 29}, script);
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
					// EnableHierarchyItem(new[] {4, 20, 30}, null);

					if (script.teamsIndex == 0)
					{
						EnableHierarchyItem(new[] {4, 20, 30, 31}, script);
					}
					else
					{
						EnableHierarchyItem(new[] {4, 20, 30, 32}, script);
					}

					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
					if (script.teamsIndex == 0)
					{
						EnableHierarchyItem(new[] {4, 20, 49, 33, 34}, script);
					}
					else
					{
						EnableHierarchyItem(new[] {4, 20, 49, 33, 35}, script);
					}

					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:

					// EnableHierarchyItem(new[] {4, 20, 36}, null);

					if (script.preMatchMenuIndex == 0)
					{
						EnableHierarchyItem(new[] {4, 20, 36, 37}, script);
					}
					else
					{
						EnableHierarchyItem(new[] {4, 20, 36, 38}, script);
					}

					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:

					if (script.roomMatchStatsTab == 0)
					{
						switch (script.teamsIndex)
						{
							case 0:
								EnableHierarchyItem(new[] {4, 20, 42, 43}, script);
								break;
							case 1:
								EnableHierarchyItem(new[] {4, 20, 42, 44}, script);
								break;
							case 2:
								EnableHierarchyItem(new[] {4, 20, 42, 43, 45}, script);
								break;

							case 3:
								EnableHierarchyItem(new[] {4, 20, 42, 43, 46}, script);
								break;
						}
					}
					else
					{
						switch (script.teamsIndex)
						{
							case 0:
								EnableHierarchyItem(new[] {4, 20, 42, 47}, script);
								break;
							case 1:
								EnableHierarchyItem(new[] {4, 20, 42, 48}, script);
								break;
						}
					}

					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:
					// EnableHierarchyItem(new[] {4, 20, 39}, null);

					if (script.deathScreensIndex == 0)
					{
						EnableHierarchyItem(new[] {4, 20, 39, 40}, script);
					}
					else
					{
						EnableHierarchyItem(new[] {4, 20, 39, 41}, script);
					}

					break;
				// case UIHelper.MenuPages.UIPlaceholders:
				// 	EnableHierarchyItem(new[] {5}, null);
				// break;
				case UIHelper.MenuPages.BasicMultiplayer:
					EnableHierarchyItem(new[] {3}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobby:
					EnableHierarchyItem(new[] {3, 11}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu:
					EnableHierarchyItem(new[] {3, 11, 13}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyCharacters:
					EnableHierarchyItem(new[] {3, 11, 14}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyMaps:
					EnableHierarchyItem(new[] {3, 11, 15}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms:
					EnableHierarchyItem(new[] {3, 11, 16}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom:
					EnableHierarchyItem(new[] {3, 11, 17}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyAvatars:
					EnableHierarchyItem(new[] {3, 11, 50}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoom:
					EnableHierarchyItem(new[] {3, 12}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomPause:
					EnableHierarchyItem(new[] {3, 12, 18}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
					EnableHierarchyItem(new[] {3, 12, 5}, script);
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomGameOver:
					EnableHierarchyItem(new[] {3, 12, 49}, script);
					break;
			}
		}

		static void EnableHierarchyItem(int[] pathItems, UIManager script)
		{
			foreach (var index in pathItems)
			{
				if (!script.hierarchy[index]) continue;

				script.hierarchy[index].hideFlags = HideFlags.None;
				script.hierarchy[index].SetActive(true);

				if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
					UIHelper.SceneHierarchyUtility.SetExpanded(script.hierarchy[index], true);
			}

			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
				UIHelper.SceneHierarchyUtility.SetExpanded(script.gameObject, true);
		}
#endif

	}
}
