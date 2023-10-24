using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	[CustomEditor(typeof(ProjectSettings))]
	public class ProjectSettingsEditor : Editor
	{
		private ProjectSettings script;

		private GUIStyle tabStyle;
		private GUISkin customSkin;
		
		List<string> allKeyboardButtons = new List<string>();

		public void Awake()
		{
			script = (ProjectSettings) target;
		}

		public void OnEnable()
		{
			customSkin = (GUISkin) Resources.Load("EditorSkin");
			tabStyle = customSkin.GetStyle("TabSmall");

			if (EditorGUIUtility.isProSkin)
			{
				customSkin.customStyles[0].onNormal.background = Resources.Load("TabSelected") as Texture2D;
				customSkin.customStyles[0].onNormal.textColor = Color.white;
			}
			else
			{
				customSkin.customStyles[0].onNormal.background = Resources.Load("TabSelected2") as Texture2D;
				customSkin.customStyles[0].onNormal.textColor = Color.black;
			}
			// var keyControls = new List<UnityEngine.InputSystem.Controls.ButtonControl>();
			// allKeyboardButtons = new List<string> {"-"};
			// allKeyboardButtons.AddRange(keyControls.Select(button => button.name));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			// EditorGUILayout.BeginVertical("helpbox");
			// EditorGUILayout.LabelField("To adjust the mobile buttons go to [UI Manager -> Mobile Input].");
			//
			// if (GUILayout.Button("Open UI Manager"))
			// {
			// 	var ui = Resources.Load("UI Manager", typeof(UIManager)) as UIManager;
			// 	AssetDatabase.OpenAsset(ui);
			// 	// EditorGUIUtility.PingObject(ui);
			// }
			//
			// EditorGUILayout.EndVertical();
			
			// EditorGUILayout.Space();
			// EditorGUILayout.Space();
			
			// EditorGUILayout.BeginVertical("box");
			script.tab = GUILayout.Toolbar(script.tab, new string[] {"Character", "Weapons", "Inventory", "Other"}, tabStyle);
			EditorGUILayout.Space();
			// EditorGUILayout.EndVertical();
			switch (script.tab)
			{
				case 0:
					
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					
					EditorGUILayout.LabelField("Gamepad", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.gamepadSticksInProjectSettings[0] = (InputHelper.GamepadSticks) EditorGUILayout.EnumPopup("Stick", script.gamepadSticksInProjectSettings[0]);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("invertAxes").GetArrayElementAtIndex(0), new GUIContent("Invert X value"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("invertAxes").GetArrayElementAtIndex(1), new GUIContent("Invert Y value"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Keyboard", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.keyboardButtonsInProjectSettings[12] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Forward", script.keyboardButtonsInProjectSettings[12]);
					script.keyboardButtonsInProjectSettings[13] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Backward", script.keyboardButtonsInProjectSettings[13]);
					script.keyboardButtonsInProjectSettings[15] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Left", script.keyboardButtonsInProjectSettings[15]);
					script.keyboardButtonsInProjectSettings[14] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Right", script.keyboardButtonsInProjectSettings[14]);
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Rotate Camera", EditorStyles.boldLabel);
					
					// EditorGUILayout.BeginVertical("HelpBox");
					
					// EditorGUILayout.LabelField("Gamepad", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("invertAxes").GetArrayElementAtIndex(2), new GUIContent("Invert X value"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("invertAxes").GetArrayElementAtIndex(3), new GUIContent("Invert Y value"));
					
					EditorGUILayout.Space();
					
					script.gamepadSticksInProjectSettings[1] = (InputHelper.GamepadSticks) EditorGUILayout.EnumPopup("Gamepad Stick", script.gamepadSticksInProjectSettings[1]);

					EditorGUILayout.EndVertical();
		
					// EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					
					DrawInputField("Switch Camera Type", 11,11);

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Change TP Movement Type / TD Mode", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					script.ButtonsActivityStatuses[20] = GUILayout.Toggle(script.ButtonsActivityStatuses[20], script.ButtonsActivityStatuses[20] ? "Disable" : "Enable", "Button");
			
					EditorGUILayout.Space();
					EditorGUI.BeginDisabledGroup(!script.ButtonsActivityStatuses[20]);
					
					script.keyboardButtonsInProjectSettings[21] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[21]);
					script.gamepadButtonsInProjectSettings[19] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[19]);
			
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndVertical();
					
					// DrawInputField("Change TP Movement Type / TD Mode", 21,19);
					
					// DrawInputField("Change Character", 18,16);
					
					EditorGUILayout.Space();
					
					DrawInputField("Sprint", 0,0, "holdSprintButton", new GUIContent("Hold Button°", "• If active - Hold the button to run." + "\n" +
					                                                                                 "• If not - Click the button to run. Press again to stop running."));

					
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("runWithJoystick"), new GUIContent("Run using joystick°" ,"Character start running after you move the joystick to a certain value (works with gamepads and mobile input)."));

					if (script.runWithJoystick)
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("runJoystickRange"), new GUIContent("Joystick Range"));
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					DrawInputField("Crouch", 1,1, "holdCrouchButton", new GUIContent("Hold Button°", "• If active - Hold the button to crouch." + "\n" +
					                                                                                 "• If not - Click the button to crouch. Press again to stand up."));

					EditorGUILayout.Space();
					
					DrawInputField("Jump", 2,2);
					break;
				
				case 1:

					EditorGUILayout.Space();
					
					DrawInputField("Attack", 3,3);
					
					EditorGUILayout.Space();
					
					DrawInputField("Reload", 4,4);
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Aim", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					script.ButtonsActivityStatuses[5] = GUILayout.Toggle(script.ButtonsActivityStatuses[5], script.ButtonsActivityStatuses[5] ? "Disable" : "Enable", "Button");
			
					EditorGUILayout.Space();
					EditorGUI.BeginDisabledGroup(!script.ButtonsActivityStatuses[5]);
					
					script.keyboardButtonsInProjectSettings[5] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[5]);
					script.gamepadButtonsInProjectSettings[5] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[5]);
			
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.Space();

					script.aimButtonInteraction = (ProjectSettings.AimButtonInteraction) EditorGUILayout.EnumPopup(new GUIContent("Interaction°","• Click - click the button to aim, click again to stop aiming." + "\n" +
					                                                                                                                             "• Hold - hold the button to aim." + "\n" +
					                                                                                                                             "• Hold on Gamepads - hold the button to aim with gamepads. Click the button to aim with keyboard/mouse."), script.aimButtonInteraction);
					EditorGUILayout.EndVertical();
					
					// DrawInputField("Aim", 5,5, "holdAimButton", new GUIContent("Hold Button°", "• If active - Hold the button to aim." + " \n" + 
					//                                                                            "• If not - Click the button to aim, click again to stop aiming."));

					EditorGUILayout.Space();
					
					DrawInputField("Change Attack Type", 19,17);
					
					break;
				
				case 2:
					EditorGUILayout.Space();
					
					DrawInputField("Open/Close Inventory",7,7, "holdInventoryButton", new GUIContent("Hold Button°", "• If active - Hold the button to keep inventory open." + " \n" + 
					                                                                            "• If not - Click the button to open inventory, click again to close."));
					
					EditorGUILayout.Space();

					DrawInputField("Pick up Object", 8, 8);
					
					EditorGUILayout.Space();
					
					DrawInputField("Drop Weapon", 9, 9);
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Change weapon", EditorStyles.boldLabel);
				
					EditorGUILayout.BeginVertical("HelpBox");
					script.ButtonsActivityStatuses[16] = GUILayout.Toggle(script.ButtonsActivityStatuses[16], script.ButtonsActivityStatuses[16] ? "Disable" : "Enable", "Button");
				
					EditorGUILayout.Space();
					
					EditorGUI.BeginDisabledGroup(!script.ButtonsActivityStatuses[16]);

					EditorGUILayout.LabelField("<-", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.keyboardButtonsInProjectSettings[17] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[17]);
					script.gamepadButtonsInProjectSettings[15] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[15]);
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.LabelField("->", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.keyboardButtonsInProjectSettings[16] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[16]);
					script.gamepadButtonsInProjectSettings[14] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[14]);
					EditorGUILayout.EndVertical();
					
					EditorGUI.EndDisabledGroup();
					
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					
					EditorGUILayout.LabelField("Interacting with Inventory (Gamepad)", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.gamepadButtonsInProjectSettings[12] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Use Health Kit", script.gamepadButtonsInProjectSettings[12]);
					script.gamepadButtonsInProjectSettings[13] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Use Ammo Kit", script.gamepadButtonsInProjectSettings[13]);
					script.gamepadSticksInProjectSettings[2] = (InputHelper.GamepadSticks) EditorGUILayout.EnumPopup("Select weapons", script.gamepadSticksInProjectSettings[2]);
					EditorGUILayout.EndVertical();
					
					break;
				case 3:
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					// DrawInputField("Pause", 10,10);
					EditorGUILayout.LabelField("Pause", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.keyboardButtonsInProjectSettings[10] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[10]);
					script.gamepadButtonsInProjectSettings[10] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[10]);
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Interaction with UI", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					script.gamepadSticksInProjectSettings[3] = (InputHelper.GamepadSticks) EditorGUILayout.EnumPopup("Gamepad Stick", script.gamepadSticksInProjectSettings[3]);
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					

					
#if USK_RCC_INTEGRATION || USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
					
					EditorGUILayout.LabelField("Interaction with vehicles", EditorStyles.boldLabel);
					
					EditorGUILayout.BeginVertical("HelpBox");
					script.keyboardButtonsInProjectSettings[20] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[20]);
					script.gamepadButtonsInProjectSettings[18] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[18]);
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();

#endif
					
					EditorGUILayout.LabelField("Mobile Debug", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.HelpBox("Use this checkbox if you need to test a mobile game in the Editor (Unity Remote required). " + "\n\n" +
					                        "You do not need to enable this variable for the build, because all mobile buttons are automatically activated in it.", MessageType.Info);
					script.mobileDebug = EditorGUILayout.ToggleLeft("Enable", script.mobileDebug);
					EditorGUILayout.EndVertical();
					break;
			}
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			// DrawDefaultInspector();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);
			}
		}

		void DrawInputField(string label, int keyboardButton, int gamepadButton, string specialBool = null, GUIContent guiContent = null)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("HelpBox");
			script.ButtonsActivityStatuses[keyboardButton] = GUILayout.Toggle(script.ButtonsActivityStatuses[keyboardButton], script.ButtonsActivityStatuses[keyboardButton] ? "Disable" : "Enable", "Button");
			
			EditorGUILayout.Space();
			EditorGUI.BeginDisabledGroup(!script.ButtonsActivityStatuses[keyboardButton]);

			script.keyboardButtonsInProjectSettings[keyboardButton] = (InputHelper.KeyboardCodes) EditorGUILayout.EnumPopup("Keyboard", script.keyboardButtonsInProjectSettings[keyboardButton]);
			script.gamepadButtonsInProjectSettings[gamepadButton] = (InputHelper.GamepadButtons) EditorGUILayout.EnumPopup("Gamepad", script.gamepadButtonsInProjectSettings[gamepadButton]);
			
			if (specialBool != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(specialBool), guiContent, true);
			}
			
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndVertical();
		}
	}
}
