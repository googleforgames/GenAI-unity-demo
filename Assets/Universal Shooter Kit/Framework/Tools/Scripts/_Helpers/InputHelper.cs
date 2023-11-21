using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TouchPhase = UnityEngine.TouchPhase;

namespace GercStudio.USK.Scripts
{
	public static class InputHelper
	{
		public enum GamepadSticks
		{
			LeftStick,
			RightStick
		}

		public enum GamepadButtons
		{
			NotUse,
			NorthButton, 
			SouthButton, 
			WestButton,
			EastButton,
			LeftShoulder, 
			RightShoulder, 
			LeftTrigger,
			RightTrigger,
			DpadUpButton,
			DpadDownButton,
			DpadLeftButton,
			DpadRightButton,
			LeftStickButton, 
			RightStickButton,
			StartButton,
			SelectButton
		}
		
		public enum KeyboardCodes
		{
			LeftMouseButton,
			RightMouseButton,
			MiddleMouseButton, 
			Q, W, E, R, T, Y, U, I, O, P, A, S, D, F, G, H, J, K, L, Z, X, C, V, B, N, M, _1, _2, _3, _4, _5, _6, _7, _8, _9, _0,
			Space,
			Backspace,
			LeftShift,
			RightShift,
			LeftCtrl,
			RightCtrl,
			LeftAlt,
			RightAlt,
			Tab,
			Escape, 
			Enter,
			UpArrow, DownArrow, LeftArrow, RightArrow
		}
		
		public static void CheckMobileJoystick(GameObject Stick, GameObject Outline, ref int touchId, UIManager uiManager, ref Vector2 MobileTouchjPointA, ref Vector2 MobileTouchjPointB, ref Vector2 MobileMoveStickDirection, Controller controller)
		{
			var empty = false;
			MobileJoystick(Stick, Outline, ref touchId, uiManager, ref MobileTouchjPointA, ref MobileTouchjPointB, ref MobileMoveStickDirection, controller, "controller", ref empty);
		}
		
		public static void CheckMobileJoystick(GameObject Stick, GameObject Outline, ref int touchId, UIManager uiManager, ref Vector2 MobileTouchjPointA, ref Vector2 MobileTouchjPointB, ref Vector2 MobileMoveStickDirection, ref bool useJoystick, Controller controller)
		{
			// Controller empty = null;
			MobileJoystick(Stick, Outline, ref touchId, uiManager, ref MobileTouchjPointA, ref MobileTouchjPointB, ref MobileMoveStickDirection, controller, "camera", ref useJoystick);
		}

		static void MobileJoystick(GameObject Stick, GameObject Outline,ref int touchId, UIManager uiManager, ref Vector2 MobileTouchjPointA, ref Vector2 MobileTouchjPointB, ref Vector2 MobileMoveStickDirection, Controller controller, string Type, ref bool useJoystick)
		{
			if(!Stick || !Outline)
				return;

			if (uiManager.moveStickAlwaysVisible && !controller.isPause)
			{
				Stick.gameObject.SetActive(true);
				Outline.gameObject.SetActive(true);
			}

			MouseTouchMovementControl(Stick, Outline,ref  touchId, uiManager, ref MobileTouchjPointA, ref MobileTouchjPointB, ref MobileMoveStickDirection, controller, "controller", ref useJoystick);

			if (Input.touches.Length > 0)
			{
				for (var i = 0; i < Input.touches.Length; i++)
				{
					var touch = Input.GetTouch(i);
			
					if (touchId == -1 && touch.phase == TouchPhase.Began && (Type == "controller" ? touch.position.x < Screen.width / 2 : touch.position.x > Screen.width / 2) && touch.position.y < Screen.height / 2)
					{
						var eventSystem = controller.eventSystem;
						
						if (!eventSystem.currentInputModule.IsPointerOverGameObject(touch.fingerId))
						{
							touchId = touch.fingerId;
			
							if(!uiManager.fixedPosition) MobileTouchjPointA = touch.position;
							else MobileTouchjPointA = Outline.transform.position;
			
							Stick.gameObject.SetActive(true);
							Outline.gameObject.SetActive(true);
			
							if (!uiManager.fixedPosition) Stick.transform.position = MobileTouchjPointA;
							else Stick.transform.position = touch.position;
							
							Outline.transform.position = MobileTouchjPointA;
						}
					}
			
					if (touch.fingerId == touchId)
					{
						MobileTouchjPointB = new Vector2(touch.position.x, touch.position.y);
			
							var offset = MobileTouchjPointB - MobileTouchjPointA;
			
							if (Type == "controller")
							{
								if (offset.x > 0.1f || offset.y > 0.1f || offset.x < -0.1 || offset.y < -0.1f)
								{
									controller.anim.SetBool("Move", true);
									controller.anim.SetBool("MoveButtonHasPressed", true);
									
									controller.hasMoveButtonPressed = true;
								}
							}
							else
							{
								if (offset.x > 0.1f || offset.y > 0.1f || offset.x < -0.1 || offset.y < -0.1f)
								{
									useJoystick = true;
								}
								else
								{
									useJoystick = false;
								}
							}
							
							var screenDelta =  Screen.width / 1920;
							MobileMoveStickDirection = Vector2.ClampMagnitude(offset, uiManager.moveStickRange * screenDelta);
							
							Stick.transform.position = new Vector2(MobileTouchjPointA.x + MobileMoveStickDirection.x, MobileTouchjPointA.y + MobileMoveStickDirection.y);
			
							MobileMoveStickDirection /= uiManager.moveStickRange * screenDelta;

							if (controller.projectSettings.runWithJoystick)
							{
								var isSprint = MobileMoveStickDirection.magnitude > controller.projectSettings.runJoystickRange;

								if (!controller.isCrouch)
								{
									if (isSprint)
									{
										controller.changeSprintStateTimeout = 0;
										controller.isSprint = isSprint;
										controller.anim.SetBool("Sprint", isSprint);
									}
									else
									{
										if (controller.changeSprintStateTimeout > 0.5f)
										{
											controller.changeSprintStateTimeout = 0;
											controller.isSprint = isSprint;
											controller.anim.SetBool("Sprint", isSprint);
										}
									}
								}
							}

						if (touch.phase == TouchPhase.Ended)
						{
							touchId = -1;
							MobileMoveStickDirection = Vector2.zero;
			
							if (!uiManager.moveStickAlwaysVisible)
							{
								Stick.gameObject.SetActive(false);
								Outline.gameObject.SetActive(false);
							}
							else
							{
								Stick.transform.position = Outline.transform.position;
							}
							
							useJoystick = false;
						}
					}
				}
			}
			else
			{
				if (!uiManager.moveStickAlwaysVisible)
				{
					if (Mouse.current != null && !Mouse.current.leftButton.isPressed || Mouse.current == null)
					{
						Stick.gameObject.SetActive(false);
						Outline.gameObject.SetActive(false);
					}
				}
			}
		}

		static void MouseTouchMovementControl(GameObject Stick, GameObject Outline,ref int touchId, UIManager uiManager, ref Vector2 MobileTouchjPointA, ref Vector2 MobileTouchjPointB, ref Vector2 MobileMoveStickDirection, Controller controller, string Type, ref bool useJoystick)
		{
			if(Mouse.current == null) return;

			if (Mouse.current.leftButton.wasPressedThisFrame && (Type == "controller" ? Mouse.current.position.ReadValue().x < Screen.width / 2 : Mouse.current.position.ReadValue().x > Screen.width / 2) && Mouse.current.position.ReadValue().y < Screen.height / 2)
			{
				touchId = 1;
				if(!uiManager.fixedPosition) MobileTouchjPointA = Mouse.current.position.ReadValue();
				else MobileTouchjPointA = Outline.transform.position;
				
				Stick.gameObject.SetActive(true);
				Outline.gameObject.SetActive(true);
				
				if (!uiManager.fixedPosition) Stick.transform.position = MobileTouchjPointA;
				else Stick.transform.position = Mouse.current.position.ReadValue();
							
				Outline.transform.position = MobileTouchjPointA;
			}
			
			if(Mouse.current.leftButton.isPressed && touchId == 1)
			{
				MobileTouchjPointB = Mouse.current.position.ReadValue();

				var offset = MobileTouchjPointB - MobileTouchjPointA;

				if (Type == "controller")
				{
					if (offset.x > 0.1f || offset.y > 0.1f || offset.x < -0.1 || offset.y < -0.1f)
					{
							controller.anim.SetBool("Move", true);
							controller.anim.SetBool("MoveButtonHasPressed", true);
									
						controller.hasMoveButtonPressed = true;
					}
				}
				else
				{
					if (offset.x > 0.1f || offset.y > 0.1f || offset.x < -0.1 || offset.y < -0.1f)
					{
						useJoystick = true;
					}
					else
					{
						useJoystick = false;
					}
				}

				var screenDelta =  Screen.width / 1920;
				MobileMoveStickDirection = Vector2.ClampMagnitude(offset, uiManager.moveStickRange * screenDelta);
							
				Stick.transform.position = new Vector2(MobileTouchjPointA.x + MobileMoveStickDirection.x, MobileTouchjPointA.y + MobileMoveStickDirection.y);
			
				MobileMoveStickDirection /= uiManager.moveStickRange * screenDelta;

				if (controller.projectSettings.runWithJoystick)
				{
					var isSprint = MobileMoveStickDirection.magnitude > controller.projectSettings.runJoystickRange;

					if (!controller.isCrouch)
					{
						if (isSprint)
						{
							controller.changeSprintStateTimeout = 0;
							controller.isSprint = isSprint;
							controller.anim.SetBool("Sprint", isSprint);
						}
						else
						{
							if (controller.changeSprintStateTimeout > 0.5f)
							{
								controller.changeSprintStateTimeout = 0;
								controller.isSprint = isSprint;
								controller.anim.SetBool("Sprint", isSprint);
							}
						}
					}
				}
			}

			if(Mouse.current.leftButton.wasReleasedThisFrame)
			{
				touchId = 0;
				MobileMoveStickDirection = Vector2.zero;

				if (!uiManager.moveStickAlwaysVisible)
				{
					Stick.gameObject.SetActive(false);
					Outline.gameObject.SetActive(false);
				}
				else
				{
					Stick.transform.position = Outline.transform.position;
				}
				
				useJoystick = false;
			}
		}
		
		public static void CheckTouchCamera(ref int touchId, ref Vector2 mouseDelta, Controller controller)
		{
			MouseTouchCameraControl(controller, ref mouseDelta);
			
			if(Input.touchCount > 0)
			{
				for (var i = 0; i < Input.touches.Length; i++)
				{
					var touch = Input.GetTouch(i);

					if (touch.position.x > Screen.width / 2 && touchId == -1 && touch.phase == TouchPhase.Began)
					{
						touchId = touch.fingerId;
					}

					if (touch.fingerId == touchId)
					{
						if (touch.position.x > Screen.width / 2)
							mouseDelta = touch.deltaPosition / 75;
						else
						{
							mouseDelta = Vector2.zero;
						}

						if (touch.phase == TouchPhase.Ended)
						{
							touchId = -1;
							mouseDelta = Vector2.zero;
						}
					}
				}
			}
			
			if(controller.UIManager.cameraStick)
				controller.UIManager.cameraStick.gameObject.SetActive(false);
					
			if(controller.UIManager.cameraStickOutline)
				controller.UIManager.cameraStickOutline.gameObject.SetActive(false);
		}

		private static void MouseTouchCameraControl(Controller controller, ref Vector2 mouseDelta)
		{
			if(Mouse.current == null) return;
			
			if (Mouse.current.leftButton.isPressed)
			{
				if (Mouse.current.position.ReadValue().x > Screen.width / 2 && controller.touchId != 1)
					mouseDelta = Mouse.current.delta.ReadValue() / 30;
				else
				{
					mouseDelta = Vector2.zero;
				}
			}

			if (Mouse.current.leftButton.wasReleasedThisFrame)
			{
				mouseDelta = Vector2.zero;
			}

			if (controller.UIManager.cameraStick)
				controller.UIManager.cameraStick.gameObject.SetActive(false);

			if (controller.UIManager.cameraStickOutline)
				controller.UIManager.cameraStickOutline.gameObject.SetActive(false);
		}

		private static bool ResolveInputConflict(ButtonControl button, Controller controller)
		{
			if (controller.projectSettings.gamepadButtonsInUnityInputSystem[18] == button)
			{
				if (controller.interactionWithCars)
					return !controller.interactionWithCars.showUITooltip;
			}
			
			if(controller.projectSettings.gamepadButtonsInUnityInputSystem[8] == button)
				return !controller.inventoryManager.enablePickUpTooltip;

			return true;
		}
		
		public static void CheckGamepad(ref bool useGamepad, ProjectSettings projectSettings)
		{
			if (Gamepad.current != null && (Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[0].x.ReadValue()) > 0.1f ||
			    Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[0].y.ReadValue()) > 0.1f || 
			    Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[1].x.ReadValue()) > 0.1f ||
			    Mathf.Abs(projectSettings.gamepadAxisControlsInUnityInputSystem[1].y.ReadValue()) > 0.1f))
			{
				useGamepad = true;
			}
			else if (Mouse.current != null && (Mathf.Abs(Mouse.current.delta.x.ReadValue()) > 0.1f || Mathf.Abs(Mouse.current.delta.y.ReadValue()) > 0.1f))
			{
				useGamepad = false;
			}
		}
		
		// public static void CheckGamepad(ref bool useGamepad)
		// {
		// 	if (Gamepad.current == null)
		// 	{
		// 		useGamepad = false;
		// 		return;
		// 	}
		// 	
		// 	if (Mathf.Abs(Gamepad.current.) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Gamepad 4th axis")) > 0.1f)
		// 	{
		// 		useGamepad = true;
		// 	}
		// 	else if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
		// 	{
		// 		useGamepad = false;
		// 	}
		// }

		public static bool WasKeyboardOrMouseButtonPressed(ButtonControl button)
		{
			return Keyboard.current != null && Mouse.current != null && button.wasPressedThisFrame;
		}

		public static bool IsKeyboardOrMouseButtonPressed(ButtonControl button)
		{
			return Keyboard.current != null && Mouse.current != null && button.isPressed;
		}

		public static bool WasKeyboardOrMouseButtonRealised(ButtonControl button)
		{
			return Keyboard.current != null && Mouse.current != null && button.wasReleasedThisFrame;
		}
		
		public static bool WasGamepadButtonRealised(ButtonControl button)
		{
			return Gamepad.current != null && button != null && button.wasReleasedThisFrame;
		}

		public static bool WasGamepadButtonPressed(ButtonControl button)
		{
			return Gamepad.current != null && button != null && button.wasPressedThisFrame;
		}

		public static bool WasGamepadButtonPressed(ButtonControl button, Controller controller)
		{
			return WasGamepadButtonPressed(button) && ResolveInputConflict(button, controller);
		}

		public static bool IsGamepadButtonPressed(ButtonControl button, Controller controller)
		{
			return Gamepad.current != null && ResolveInputConflict(button, controller) && button.isPressed;
		}
		
		public static void ProcessMoveButton(Controller controller, string direction)
		{
			var button = new ButtonControl();
			var dir = Vector3.zero;

			switch (direction)
			{
				case "forward":
					button = controller.projectSettings.keyboardButtonsInUnityInputSystem[12];
					dir = Vector3.forward;
					break;
				
				case "backward":
					button = controller.projectSettings.keyboardButtonsInUnityInputSystem[13];
					dir = Vector3.back;
					break;
				
				case "left":
					button = controller.projectSettings.keyboardButtonsInUnityInputSystem[15];
					dir = Vector3.left;
					break;
				
				case "right":
					button = controller.projectSettings.keyboardButtonsInUnityInputSystem[14];
					dir = Vector3.right;
					break;
			}
			
			if (WasKeyboardOrMouseButtonPressed(button))
			{
				controller.anim.SetBool("MoveButtonHasPressed", false);
				controller.clickMoveButton = true;
			}
			if (IsKeyboardOrMouseButtonPressed(button))
			{
				controller.directionVector += dir;
				controller.hasMoveButtonPressed = true;
			}
			if (WasKeyboardOrMouseButtonRealised(button))
			{
				controller.clickMoveButton = false;
				controller.directionVector += dir;
							
				if(!controller.anim.GetBool("Move"))
					controller.anim.SetBool("MoveButtonHasPressed", true);
			}
		}

		public static void InitializeGamepadButtons(ProjectSettings projectSettings)
		{
			projectSettings.gamepadButtonsInUnityInputSystem.Clear();
			projectSettings.gamepadAxisControlsInUnityInputSystem.Clear();
			
			for (int i = 0; i < 4; i++)
			{
				projectSettings.gamepadAxisControlsInUnityInputSystem.Add(new StickControl());

				if (Gamepad.current == null) continue;
				
				switch (projectSettings.gamepadSticksInProjectSettings[i])
				{
					case GamepadSticks.LeftStick:
						projectSettings.gamepadAxisControlsInUnityInputSystem[i] = Gamepad.current.leftStick;
						break;
					case GamepadSticks.RightStick:
						projectSettings.gamepadAxisControlsInUnityInputSystem[i] = Gamepad.current.rightStick;
						break;
				}
			}

			for (int i = 0; i < 20; i++)
			{
				projectSettings.gamepadButtonsInUnityInputSystem.Add(new ButtonControl());

				if (Gamepad.current == null) continue;
				
				switch (projectSettings.gamepadButtonsInProjectSettings[i])
				{
					case GamepadButtons.NotUse:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = null;
						break;
					case GamepadButtons.NorthButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.buttonNorth;
						break;
					case GamepadButtons.SouthButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.buttonSouth;
						break;
					case GamepadButtons.WestButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.buttonWest;
						break;
					case GamepadButtons.EastButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.buttonEast;
						break;
					case GamepadButtons.LeftShoulder:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.leftShoulder;
						break;
					case GamepadButtons.RightShoulder:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.rightShoulder;
						break;
					case GamepadButtons.LeftTrigger:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.leftTrigger;
						break;
					case GamepadButtons.RightTrigger:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.rightTrigger;
						break;
					case GamepadButtons.DpadUpButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.dpad.up;
						break;
					case GamepadButtons.DpadDownButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.dpad.down;
						break;
					case GamepadButtons.DpadLeftButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.dpad.left;
						break;
					case GamepadButtons.DpadRightButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.dpad.right;
						break;
					case GamepadButtons.LeftStickButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.leftStickButton;
						break;
					case GamepadButtons.RightStickButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.rightStickButton;
						break;
					case GamepadButtons.StartButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.startButton;
						break;
					case GamepadButtons.SelectButton:
						projectSettings.gamepadButtonsInUnityInputSystem[i] = Gamepad.current.selectButton;
						break;
				}
			}
		}

		public static void InitializeKeyboardAndMouseButtons(ProjectSettings projectSettings)
		{
			projectSettings.keyboardButtonsInUnityInputSystem.Clear();


			for (int i = 0; i < 22; i++)
			{
				projectSettings.keyboardButtonsInUnityInputSystem.Add(new ButtonControl());

				if (Keyboard.current == null || Mouse.current == null) continue;
				
				switch (projectSettings.keyboardButtonsInProjectSettings[i])
				{
					case KeyboardCodes.RightMouseButton:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Mouse.current.rightButton;
						break;
					case KeyboardCodes.LeftMouseButton:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Mouse.current.leftButton;
						break;
					case KeyboardCodes.MiddleMouseButton:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Mouse.current.middleButton;
						break;
					case KeyboardCodes.Q:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.qKey;
						break;
					case KeyboardCodes.W:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.wKey;
						break;
					case KeyboardCodes.E:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.eKey;
						break;
					case KeyboardCodes.R:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.rKey;
						break;
					case KeyboardCodes.T:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.tKey;
						break;
					case KeyboardCodes.Y:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.yKey;
						break;
					case KeyboardCodes.U:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.uKey;
						break;
					case KeyboardCodes.I:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.iKey;
						break;
					case KeyboardCodes.O:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.oKey;
						break;
					case KeyboardCodes.P:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.pKey;
						break;
					case KeyboardCodes.A:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.aKey;
						break;
					case KeyboardCodes.S:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.sKey;
						break;
					case KeyboardCodes.D:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.dKey;
						break;
					case KeyboardCodes.F:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.fKey;
						break;
					case KeyboardCodes.G:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.gKey;
						break;
					case KeyboardCodes.H:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.hKey;
						break;
					case KeyboardCodes.J:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.jKey;
						break;
					case KeyboardCodes.K:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.kKey;
						break;
					case KeyboardCodes.L:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.lKey;
						break;
					case KeyboardCodes.Z:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.zKey;
						break;
					case KeyboardCodes.X:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.xKey;
						break;
					case KeyboardCodes.C:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.cKey;
						break;
					case KeyboardCodes.V:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.vKey;
						break;
					case KeyboardCodes.B:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.bKey;
						break;
					case KeyboardCodes.N:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.nKey;
						break;
					case KeyboardCodes.M:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.mKey;
						break;
					case KeyboardCodes._0:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit0Key;
						break;
					case KeyboardCodes._1:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit1Key;
						break;
					case KeyboardCodes._2:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit2Key;
						break;
					case KeyboardCodes._3:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit3Key;
						break;
					case KeyboardCodes._4:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit4Key;
						break;
					case KeyboardCodes._5:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit5Key;
						break;
					case KeyboardCodes._6:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit6Key;
						break;
					case KeyboardCodes._7:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit7Key;
						break;
					case KeyboardCodes._8:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit8Key;
						break;
					case KeyboardCodes._9:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.digit9Key;
						break;
					case KeyboardCodes.Space:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.spaceKey;
						break;
					case KeyboardCodes.Backspace:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.backspaceKey;
						break;
					case KeyboardCodes.LeftShift:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.leftShiftKey;
						break;
					case KeyboardCodes.RightShift:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.rightShiftKey;
						break;
					case KeyboardCodes.LeftCtrl:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.leftCtrlKey;
						break;
					case KeyboardCodes.RightCtrl:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.rightCtrlKey;
						break;
					case KeyboardCodes.LeftAlt:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.leftAltKey;
						break;
					case KeyboardCodes.RightAlt:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.rightAltKey;
						break;
					case KeyboardCodes.Tab:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.tabKey;
						break;
					case KeyboardCodes.Enter:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.enterKey;
						break;
					case KeyboardCodes.Escape:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.escapeKey;
						break;
					case KeyboardCodes.UpArrow:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.upArrowKey;
						break;
					case KeyboardCodes.DownArrow:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.downArrowKey;
						break;
					case KeyboardCodes.LeftArrow:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.leftArrowKey;
						break;
					case KeyboardCodes.RightArrow:
						projectSettings.keyboardButtonsInUnityInputSystem[i] = Keyboard.current.rightArrowKey;
						break;
				}
			}
		}
	}
}
