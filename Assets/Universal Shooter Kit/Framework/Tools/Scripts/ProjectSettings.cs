using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace GercStudio.USK.Scripts
{
	public class ProjectSettings : ScriptableObject
	{
		public List<ButtonControl> keyboardButtonsInUnityInputSystem = new List<ButtonControl>();
		public List<ButtonControl> gamepadButtonsInUnityInputSystem = new List<ButtonControl>();
		public List<StickControl> gamepadAxisControlsInUnityInputSystem = new List<StickControl>();
		
		public InputHelper.GamepadSticks[] gamepadSticksInProjectSettings = new InputHelper.GamepadSticks[4];
		
		public InputHelper.GamepadButtons[] gamepadButtonsInProjectSettings = new InputHelper.GamepadButtons[20];
		public InputHelper.KeyboardCodes[] keyboardButtonsInProjectSettings = new InputHelper.KeyboardCodes[22];
		public bool[] ButtonsActivityStatuses = Helper.ButtonsStatus(21);
		
		public bool holdSprintButton;
		public bool holdCrouchButton;
		public bool holdInventoryButton;
		public bool runWithJoystick;
		public bool mobileDebug;

		[Range(0.1f, 1)]public float runJoystickRange = 0.7f;

		public List<string> CharacterTags = new List<string>{"Character"};
		public List<string> EnemiesTags = new List<string>{"Enemy"};

		public float CubesSize = 10;
		public Helper.CubeSolid CubeSolid = Helper.CubeSolid.Wire;

		public string oldScenePath;
		public string oldSceneName;
		
		public List<string> defaultAvatars;

		public enum AimButtonInteraction
		{
			Click,
			Hold,
			HoldOnGamepads
		}

		public AimButtonInteraction aimButtonInteraction;

		public bool[] invertAxes = new bool[5];

		public int tab;
	}
}
