using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace GercStudio.USK.Scripts
{
	[CustomEditor(typeof(Adjustment))]
	public class AdjustmentEditor : Editor
	{
		private Adjustment script;

		private ReorderableList weaponsList;
		private ReorderableList charactersList;
		private ReorderableList copyToList;

		private string curName;
		
		Texture2D image1;
		Texture2D image2;
		
		private int stateIndex;
		
		private GUIStyle style;
		private GUIStyle style2;
		private GUIStyle grayBackground;

		private Helper.RotationAxes axises;

		private void Awake()
		{
			image1 = Resources.Load("example_1", typeof(Texture2D)) as Texture2D;
			image2 = Resources.Load("example_2", typeof(Texture2D)) as Texture2D;
			

			script = (Adjustment) target;
		}

		private void OnEnable()
		{
			copyToList = new ReorderableList(serializedObject, serializedObject.FindProperty("CopyToList"), false, true,
				false, false)
			{
				drawHeaderCallback = rect =>
				{
					if (script.ikInspectorTab == 0)
					{
						EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 3 - 7, EditorGUIUtility.singleLineHeight), "Weapon:");
						EditorGUI.LabelField(new Rect(rect.x + rect.width / 3, rect.y, rect.width / 3 - 7, EditorGUIUtility.singleLineHeight), "Settings Slot:");
						EditorGUI.LabelField(new Rect(rect.x + 2 * rect.width / 3, rect.y, rect.width / 3 - 7, EditorGUIUtility.singleLineHeight), "IK Mode:");
					}
					else
					{
						EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), "Weapon:");
						EditorGUI.LabelField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), "Settings Slot:");
					}

				},
				
				drawElementBackgroundCallback = (rect, index, active, focused) => { },
				
				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					if (script.ikInspectorTab == 0)
					{
						script.copyFromWeaponSlot = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width / 3 - 7, EditorGUIUtility.singleLineHeight), script.copyFromWeaponSlot,
							script.WeaponsNames.ToArray());
						script.copyFromSlot = EditorGUI.Popup(new Rect(rect.x + rect.width / 3, rect.y, rect.width / 3 - 7, EditorGUIUtility.singleLineHeight), script.copyFromSlot,
							script.Weapons[script.copyFromWeaponSlot].enumNames.ToArray());
						script.copyFromIKState = EditorGUI.Popup(new Rect(rect.x + 2 * rect.width / 3, rect.y, rect.width / 3, EditorGUIUtility.singleLineHeight), script.copyFromIKState,
							script.IKStateNames);
					}
					else
					{
						script.copyFromWeaponSlot = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), script.copyFromWeaponSlot,
							script.WeaponsNames.ToArray());
						script.copyFromSlot = EditorGUI.Popup(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), script.copyFromSlot,
							script.Weapons[script.copyFromWeaponSlot].enumNames.ToArray());
					}
				}
			};
			
			charactersList = new ReorderableList(serializedObject, serializedObject.FindProperty("Characters"), true, true,
				true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, Application.isPlaying ? rect.width / 1.5f : rect.width, EditorGUIUtility.singleLineHeight),
						Application.isPlaying ? "Select a character to adjust it" : "");

					if(Application.isPlaying)
						EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Status");
				},

				onAddCallback = items =>
				{
					if (!Application.isPlaying) script.Characters.Add(null);
				},

				onRemoveCallback = items =>
				{
					if (!Application.isPlaying) script.Characters.Remove(script.Characters[items.index]);
				},

				onSelectCallback = items =>
				{
					if (!script.Characters[items.index])
						return;

					if (Application.isPlaying && script.characterIndex != items.index)
					{
						script.ActivateCharacter(items.index, false);

						script.characterIndex = items.index;
					}
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					
					if (Application.isPlaying && index == script.characterIndex && script.Characters[index] && script.isPause)
					{
						var options = new GUIStyle {normal = {textColor = Color.green}};
						EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight), "Adjustment", EditorStyles.boldLabel);
					}
					
					EditorGUI.BeginDisabledGroup(Application.isPlaying);
					if(Application.isPlaying)
						script.Characters[index] = (Controller) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), script.Characters[index], typeof(Controller), false);
					else 
						script.Characters[index] = (Controller) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.Characters[index], typeof(Controller), false);

					EditorGUI.EndDisabledGroup();
				}
			};

			weaponsList = new ReorderableList(serializedObject, serializedObject.FindProperty("Weapons"), false, true,
				true, true)
			{
				drawHeaderCallback = rect =>
				{
					if (Application.isPlaying)
					{
						EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), "Select a weapon to adjust it");


						EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Status");
					}
					else
					{
						EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "");
					}
				},

				onAddCallback = items =>
				{
					if (!Application.isPlaying)
					{
						script.Weapons.Add(null);
						script.WeaponsPrefabs.Add(null);
					}
				},

				onRemoveCallback = items =>
				{
					if (!Application.isPlaying)
					{
						script.Weapons.Remove(script.Weapons[items.index]);
						script.WeaponsPrefabs.Remove(script.WeaponsPrefabs[items.index]);
					}
				},

				onSelectCallback = items =>
				{
					if (!script.Weapons[items.index] || script.weaponIndex == items.index)
						return;

					if (Application.isPlaying)
					{
						script.ResetWeapons();

						if (script.Weapons[items.index].settingsSlotIndexInAdjustment > script.Weapons[items.index].WeaponInfos.Count - 1)
							script.Weapons[items.index].settingsSlotIndexInAdjustment = 0;
						
						script.Weapons[items.index].settingsSlotIndex = script.Weapons[items.index].settingsSlotIndexInAdjustment;

						var weaponManager = script.Characters[script.characterIndex].gameObject.GetComponent<InventoryManager>();
						WeaponsHelper.SetWeaponController(script.Weapons[items.index].gameObject, script.WeaponsPrefabs[items.index], 0, weaponManager, script.Characters[script.characterIndex].GetComponent<Controller>(), script.Characters[script.characterIndex].transform);

						weaponManager.hasAnyWeapon = true;
						weaponManager.slots[0].weaponSlotInGame.Add(new CharacterHelper.Weapon {weapon = script.Weapons[items.index].gameObject});
						weaponManager.slots[0].currentWeaponInSlot = 0;
						weaponManager.ActivateWeapon(0, false, false);

						if (script.CurrentWeaponController)
							Helper.HideAllObjects(script.CurrentWeaponController.IkObjects);

						script.SerializedWeaponController = new SerializedObject(script.Weapons[items.index]);
					}

					script.weaponIndex = items.index;
					script.CurrentWeaponController = script.Weapons[script.weaponIndex];
					script.CheckIKObjects();
					script.oldDebugModeIndex = IKHelper.IkDebugMode.Aim;
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					if (Application.isPlaying && index == script.weaponIndex && script.Weapons[index])
					{
						var options = new GUIStyle {normal = {textColor = Color.green}};
						EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight), "Adjustment", options);
					}

					EditorGUI.BeginDisabledGroup(Application.isPlaying);
					if(Application.isPlaying)
						script.Weapons[index] = (WeaponController) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), script.Weapons[index], typeof(WeaponController), false);
					else 
						script.Weapons[index] = (WeaponController) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.Weapons[index], typeof(WeaponController), false);

					EditorGUI.EndDisabledGroup();
				}
			};


			EditorApplication.update += Update;
		}

		private void OnDisable()
		{
			EditorApplication.update -= Update;
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				if (!script.UIManager)
				{
					script.UIManager = Resources.Load("UI Manager", typeof(UIManager)) as UIManager;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/UI Manager.prefab", typeof(UIManager)) as UIManager;
					EditorUtility.SetDirty(script.gameObject);
                 	EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
			else
			{
				// if(script.currentController)
					// if (Input.GetKeyDown(KeyCode.C))
						// script.currentController.ChangeCameraType();
			}

			if (script.hide)
			{
				foreach (var obj in script.hideObjects.Where(obj => obj && obj.hideFlags != HideFlags.HideInHierarchy))
				{
					obj.hideFlags = HideFlags.HideInHierarchy;
				}
			}
			else
			{
				foreach (var obj in script.hideObjects.Where(obj => obj && obj.hideFlags != HideFlags.None))
				{
					obj.hideFlags = HideFlags.None;
				}
			}
			
			if (Application.isPlaying && script.currentController &&
			    script.oldCameraType != script.Characters[script.characterIndex].TypeOfCamera)
			{
				script.oldCameraType = script.Characters[script.characterIndex].TypeOfCamera;
				Repaint();
			}
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			script.SerializedWeaponController?.Update();
			
			style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};
			style2 = new GUIStyle(EditorStyles.label) {richText = true, fontSize = 10};
			
			Helper.InitStyles(ref grayBackground, new Color32(160, 160, 160, 200));


			EditorGUILayout.Space();

			script.generalInspectorTab = GUILayout.Toolbar(script.generalInspectorTab, new[] {"Editor", "Settings"});

			switch (script.generalInspectorTab)
			{
				case 1:

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					script.settings.CubesSize = EditorGUILayout.Slider("IK Handle Scale", script.settings.CubesSize, 1, 30);
					script.settings.CubeSolid = (Helper.CubeSolid) EditorGUILayout.EnumPopup("IK Handle Layer", script.settings.CubeSolid);
					EditorGUILayout.EndVertical();
					break;


				case 0:

					EditorGUILayout.Space();

					if (!Application.isPlaying)
					{
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("<b>Place here your <color=green>prefabs</color>" + ", then go to the <color=green>[Play Mode]</color> to start adjustment.</b>", style);
						EditorGUILayout.Space();
					}
					else
					{
						// EditorGUILayout.LabelField("<b>Camera Mode: <color=green>" + script.currentController.TypeOfCamera+"</color> | Press [C] button to change it</b>", style);
						// EditorGUILayout.Space();
					}

					EditorGUI.BeginDisabledGroup(Application.isPlaying && !script.isPause);

					script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Characters", "Weapons"});

					switch (script.inspectorTab)
					{
						case 0:

							#region CharacterAdjustment

							EditorGUILayout.Space();
							charactersList.DoLayoutList();
							EditorGUILayout.Space();
							EditorGUILayout.Space();
							script.Type = Adjustment.AdjustmentType.Character;

							if (Application.isPlaying && script.isPause && script.currentController && script.currentController.DebugMode)
							{
								var curCharInfo = script.currentController.CharacterOffset;
								// var curCameraMode = "";

								// switch (script.currentController.TypeOfCamera)
								// {
								// 	case CharacterHelper.CameraType.ThirdPerson:
								// 		curCameraMode = "TP view";
								// 		break;
								// 	case CharacterHelper.CameraType.FirstPerson:
								// 		curCameraMode = "FP view";
								// 		break;
								// 	case CharacterHelper.CameraType.TopDown:
								// 		curCameraMode = "TD view";
								// 		break;
								// }
								// GUILayout.BeginVertical(script.currentController.gameObject.name + "'s Settings | " + curCameraMode, "window");
								GUILayout.BeginVertical(grayBackground);
								// EditorGUILayout.Space(); 
								//
								EditorGUILayout.LabelField("Body Rotation Offset", EditorStyles.boldLabel);

								if (script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
								{
									EditorGUILayout.LabelField("Adjust the rotation of the character's body so that it looks in the right direction", style);
								}
								else
								{
									EditorGUILayout.BeginVertical("HelpBox");
									EditorGUILayout.LabelField("The <color=blue>blue arrow</color> should look <b>forward</b> and the <color=red>red arrow</color> should look <b>right</b>.", style2);
									var lastRect = GUILayoutUtility.GetLastRect();
									script.showExample1 = EditorGUI.Foldout(new Rect(new Vector2(lastRect.x + 10, lastRect.y + EditorGUIUtility.singleLineHeight), new Vector2(200, EditorGUIUtility.singleLineHeight)), script.showExample1, "Example", true);
									GUILayout.Space(EditorGUIUtility.singleLineHeight);
									lastRect = GUILayoutUtility.GetLastRect();
									if (script.showExample1)
									{
										EditorGUI.DrawPreviewTexture(new Rect(new Vector2(lastRect.x, lastRect.y + EditorGUIUtility.singleLineHeight + 5), new Vector2(300, 300)), image1);
										GUILayout.Space(307);
									}

									EditorGUILayout.EndVertical();
								}

								EditorGUILayout.BeginVertical("HelpBox");

								if (script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
								{
									curCharInfo.xRotationOffset = EditorGUILayout.Slider("X axis", curCharInfo.xRotationOffset, -90, 90);
									curCharInfo.yRotationOffset = EditorGUILayout.Slider("Y axis", curCharInfo.yRotationOffset, -90, 90);
									curCharInfo.zRotationOffset = EditorGUILayout.Slider("Z axis", curCharInfo.zRotationOffset, -90, 90);
								}
								else
								{
									EditorGUILayout.PropertyField(serializedObject.FindProperty("dirObjRotX"), new GUIContent("X axis"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("dirObjRotY"), new GUIContent("Y axis"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("dirObjRotZ"), new GUIContent("Z axis"));
								}
								
								EditorGUILayout.EndVertical();

								if (script.currentController.handDirectionObject)
								{
									EditorGUILayout.Space();
									EditorGUILayout.LabelField("Hands Rotation Offset", EditorStyles.boldLabel);
									
									EditorGUILayout.BeginVertical("HelpBox");
									EditorGUILayout.LabelField("The <color=green>green arrow</color> should look <b>along the fingers</b> and the <color=yellow>yellow arrow</color> should look <b>back</b>.", style2);
									var lastRect = GUILayoutUtility.GetLastRect();
									script.showExample2 = EditorGUI.Foldout(new Rect(new Vector2(lastRect.x + 10, lastRect.y + EditorGUIUtility.singleLineHeight), new Vector2(200, EditorGUIUtility.singleLineHeight)), script.showExample2, "Example", true);
									GUILayout.Space(EditorGUIUtility.singleLineHeight);
									lastRect = GUILayoutUtility.GetLastRect();
									if (script.showExample2)
									{
										EditorGUI.DrawPreviewTexture(new Rect(new Vector2(lastRect.x, lastRect.y + EditorGUIUtility.singleLineHeight + 5), new Vector2(300, 300)), image2);
										GUILayout.Space(307);
									}
									EditorGUILayout.EndVertical();
									
									EditorGUILayout.BeginVertical("HelpBox");

									EditorGUILayout.PropertyField(serializedObject.FindProperty("handsDirObjRotX"), new GUIContent("X axis"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("handsDirObjRotY"), new GUIContent("Y axis"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("handsDirObjRotZ"), new GUIContent("Z axis"));
									EditorGUILayout.EndVertical();

								}
								
								EditorGUILayout.Space();
								
								var curCamera = script.currentController.CameraController;
								var curCamInfo = curCamera.CameraOffset;

								EditorGUILayout.LabelField("Camera Parameters", EditorStyles.boldLabel);
								// switch (script.currentController.TypeOfCamera)
								// {
								// 	case CharacterHelper.CameraType.ThirdPerson:
								// 		EditorGUILayout.LabelField("These parameters are responsible for the camera position. " + "\n" +
								// 		                        "Press the [C] button to switch the camera type.", style);
								// 		break;
								// 	case CharacterHelper.CameraType.FirstPerson:
								// 		EditorGUILayout.LabelField("Use this object to adjust camera position and rotation in FP view." + "\n" +
								// 		                           "Press the [C] button to switch the camera type.", style);
								// 		break;
								// 	case CharacterHelper.CameraType.TopDown:
								// 		EditorGUILayout.LabelField("These parameters are responsible for the camera position." + "\n" +
								// 		                           "Press the [C] button to switch the camera type.", style);
								// 		break;
								// }
								

								switch (script.currentController.TypeOfCamera)
								{
									case CharacterHelper.CameraType.FirstPerson:

										EditorGUILayout.LabelField("Move and rotate the object below to adjust the camera.", style);
										EditorGUILayout.BeginVertical("HelpBox");
										EditorGUI.BeginDisabledGroup(true);
										curCamera.CameraPosition = (Transform) EditorGUILayout.ObjectField("Camera Object", curCamera.CameraPosition, typeof(GameObject), true);
										EditorGUI.EndDisabledGroup();
										break;

									case CharacterHelper.CameraType.ThirdPerson:

										script.cameraInspectorTab = GUILayout.Toolbar(script.cameraInspectorTab, new[] {"Norm", "Aim"});
										EditorGUILayout.BeginVertical("HelpBox");
										if (!curCamera.isCameraAimEnabled)
										{
											curCamInfo.normDistance = EditorGUILayout.Slider("Distance", curCamInfo.normDistance, -20, 20);
//											EditorGUILayout.Space();
											curCamInfo.normCameraOffsetX = EditorGUILayout.Slider("Offset - X axis", curCamInfo.normCameraOffsetX, -20, 20);
											curCamInfo.normCameraOffsetY = EditorGUILayout.Slider("Offset - Y axis", curCamInfo.normCameraOffsetY, -20, 20);
//											EditorGUILayout.Space();
//											curCamInfo.cameraNormRotationOffset = EditorGUILayout.Vector3Field("Camera Rotation Offset", curCamInfo.cameraNormRotationOffset);
										}
										else
										{
											curCamInfo.aimDistance = EditorGUILayout.Slider("Distance", curCamInfo.aimDistance, -20, 20);
//											EditorGUILayout.Space();
											curCamInfo.aimCameraOffsetX = EditorGUILayout.Slider("Offset - X axis", curCamInfo.aimCameraOffsetX, -20, 20);
											curCamInfo.aimCameraOffsetY = EditorGUILayout.Slider("Offset - Y axis", curCamInfo.aimCameraOffsetY, -20, 20);
//											EditorGUILayout.Space();
//											curCamInfo.cameraAimRotationOffset = EditorGUILayout.Vector3Field("Camera Rotation Offset", curCamInfo.cameraAimRotationOffset);
										}

										break;
									case CharacterHelper.CameraType.TopDown:

										script.tdModeInspectorTab = GUILayout.Toolbar(script.tdModeInspectorTab, new[] {"Norm Mode", "Lock Camera"}); 
										
										EditorGUILayout.BeginVertical("HelpBox");
										// curCamera.Controller.CameraParameters.lockCamera = EditorGUILayout.Toggle("Lock Camera", curCamera.Controller.CameraParameters.lockCamera);
										if (!curCamera.Controller.CameraParameters.lockCamera)
										{
											curCamInfo.TD_Distance = EditorGUILayout.Slider("Distance", curCamInfo.TD_Distance, -20, 20);
											curCamInfo.TopDownAngle = EditorGUILayout.Slider("Angle", curCamInfo.TopDownAngle, 60, 90);
											curCamInfo.tdCameraOffsetX = EditorGUILayout.Slider("Offset - X axis", curCamInfo.tdCameraOffsetX, -20, 20);
											curCamInfo.tdCameraOffsetY = EditorGUILayout.Slider("Offset - Y axis", curCamInfo.tdCameraOffsetY, -20, 20);
										}
										else
										{
											curCamInfo.TDLockCameraDistance = EditorGUILayout.Slider("Distance", curCamInfo.TDLockCameraDistance, -20, 20);
											curCamInfo.tdLockCameraAngle = EditorGUILayout.Slider("Angle", curCamInfo.tdLockCameraAngle, 60, 90);
											curCamInfo.tdLockCameraOffsetX = EditorGUILayout.Slider("Offset - X axis", curCamInfo.tdLockCameraOffsetX, -20, 20);
											curCamInfo.tdLockCameraOffsetY = EditorGUILayout.Slider("Offset - Y axis", curCamInfo.tdLockCameraOffsetY, -20, 20);
										}

										break;
								}

								EditorGUILayout.EndVertical();
								EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
								EditorGUILayout.Space();


								if (!script.currentController.CharacterOffset.HasTime)
									EditorGUILayout.LabelField("Not any save", style);
								else
								{
									var time = script.currentController.CharacterOffset.SaveTime;
									var date = script.currentController.CharacterOffset.SaveDate;
									EditorGUILayout.LabelField("Last Save: " + date.x + "/" + date.y + "/" + date.z + " " +
									                           time.x + ":" + time.y + ":" + time.z, style);
								}

								if (GUILayout.Button("Save"))
								{
									script.currentController.CharacterOffset.SaveDate =
										new Vector3(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
									script.currentController.CharacterOffset.SaveTime =
										new Vector3(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

									script.currentController.CharacterOffset.HasTime = true;

									script.currentController.CharacterOffset.directionObjRotation = new Vector3(script.dirObjRotX, script.dirObjRotY, script.dirObjRotZ);
									script.currentController.CharacterOffset.handsDirectionObjRotation = new Vector3(script.handsDirObjRotX, script.handsDirObjRotY, script.handsDirObjRotZ);

									script.CurrentCharacterOffsets[script.characterIndex].Clone(script.currentController.CharacterOffset);
									script.currentController.OriginalScript.CharacterOffset.Clone(script.currentController.CharacterOffset);

									script.currentController.CameraController.CameraOffset.cameraObjPos = script.currentController.CameraController.CameraPosition.localPosition;
									script.currentController.CameraController.CameraOffset.cameraObjRot = script.currentController.CameraController.CameraPosition.localEulerAngles;

									script.CurrentCameraOffsets[script.characterIndex].Clone(script.currentController.CameraController.CameraOffset);
									script.currentController.CameraController.OriginalScript.CameraOffset.Clone(script.currentController.CameraController.CameraOffset);
									
									if (script.currentController.OriginalScript)
										EditorUtility.SetDirty(script.currentController.OriginalScript);
									
									
								}

								EditorGUI.BeginDisabledGroup(!script.currentController.CharacterOffset.HasTime);
								if (GUILayout.Button("Return values from the Last Save"))
								{
									script.currentController.CharacterOffset.Clone(script.CurrentCharacterOffsets[script.characterIndex]);
									script.currentController.CameraController.CameraOffset.Clone(script.CurrentCameraOffsets[script.characterIndex]);

									curCamera.CameraPosition.localPosition = curCamera.CameraOffset.cameraObjPos;
									curCamera.CameraPosition.localEulerAngles = curCamera.CameraOffset.cameraObjRot;

									script.currentController.DirectionObject.localEulerAngles = script.currentController.CharacterOffset.directionObjRotation;
								}

								EditorGUI.EndDisabledGroup();

								if (GUILayout.Button("Set default positions"))
								{
									script.currentController.CharacterOffset.xRotationOffset = 0;
									script.currentController.CharacterOffset.yRotationOffset = 0;
									script.currentController.CharacterOffset.zRotationOffset = 0;
									script.currentController.CharacterOffset.CharacterHeight = -1.1f;

									script.currentController.CharacterOffset.directionObjRotation = Vector3.zero;
									script.currentController.CharacterOffset.handsDirectionObjRotation = Vector3.zero;
									script.currentController.DirectionObject.eulerAngles = Vector3.zero;

									script.dirObjRotX = 0;
									script.dirObjRotY = 0;
									script.dirObjRotZ = 0;

									curCamera.CameraOffset.normDistance = 0;
									curCamera.CameraOffset.normCameraOffsetX = 0;
									curCamera.CameraOffset.normCameraOffsetY = 0;
									
//									curCamera.CameraOffset.cameraNormRotationOffset = Vector3.zero;
//									curCamera.CameraOffset.cameraAimRotationOffset = Vector3.zero;

									curCamera.CameraOffset.aimDistance = 0;
									curCamera.CameraOffset.aimCameraOffsetX = 0;
									curCamera.CameraOffset.aimCameraOffsetY = 0;

									curCamera.CameraOffset.cameraObjPos = Vector3.zero;
									curCamera.CameraOffset.cameraObjRot = Vector3.zero;

									curCamera.CameraPosition.localPosition = Vector3.zero;
									curCamera.CameraPosition.localEulerAngles = Vector3.zero;

									curCamera.CameraOffset.tdLockCameraAngle = 90;
									curCamera.CameraOffset.tdLockCameraOffsetX = 0;
									curCamera.CameraOffset.tdLockCameraOffsetY = 0;

									curCamera.CameraOffset.TopDownAngle = 80;
									curCamera.CameraOffset.tdCameraOffsetX = 0;
									curCamera.CameraOffset.tdCameraOffsetY = 0;

									curCamera.CameraOffset.TD_Distance = 0;

								}
							}

							#endregion

							break;
						
						case 1:

							#region WeaponAdjustment

							if (Application.isPlaying && script.isPause && !script.currentController)
							{
								EditorGUILayout.Space();
								EditorGUILayout.LabelField("First of all select any character.", style);
							}

							EditorGUI.BeginDisabledGroup(Application.isPlaying && !script.currentController);
							EditorGUILayout.Space();
							weaponsList.DoLayoutList();
							EditorGUILayout.Space();
							EditorGUILayout.Space();
							EditorGUI.EndDisabledGroup();


							if (Application.isPlaying && script.isPause && script.CurrentWeaponController && script.currentController && script.CurrentWeaponController.ActiveDebug)
							{

								#region attack_menu

								if (script.CurrentWeaponController.Attacks.Any(attack => attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade) && script.Type != Adjustment.AdjustmentType.Enemy ||
								    script.CurrentWeaponController.Attacks.Any(attack => attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade) &&
								    script.Type == Adjustment.AdjustmentType.Character ||
								    script.CurrentWeaponController.Attacks.All(attack => attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade))

								{

									var curInfo = script.CurrentWeaponController.CurrentWeaponInfo[script.CurrentWeaponController.settingsSlotIndex];
									
									EditorGUILayout.BeginVertical("HelpBox");
									EditorGUILayout.BeginVertical(grayBackground);
                    
									script.CurrentWeaponController.settingsSlotIndex = EditorGUILayout.Popup("Settings Slot", script.CurrentWeaponController.settingsSlotIndex, script.CurrentWeaponController.enumNames.ToArray());

									EditorGUILayout.BeginHorizontal();
									EditorGUI.BeginDisabledGroup(script.rename);
									if (GUILayout.Button("Rename"))
									{
										script.rename = true;
										script.curName = "";
									}
									EditorGUI.EndDisabledGroup();
									
									EditorGUI.BeginDisabledGroup(script.CurrentWeaponController.WeaponInfos.Count <= 1 || script.delete);
									if (GUILayout.Button("Delete"))
									{
										script.delete = true;
									}
									EditorGUI.EndDisabledGroup();


									if (GUILayout.Button("Create a new one"))
									{
										script.CurrentWeaponController.WeaponInfos.Add(new WeaponsHelper.WeaponInfo());
										script.CurrentWeaponController.OriginalScript.WeaponInfos.Add(new WeaponsHelper.WeaponInfo());
										
										script.CurrentWeaponController.enumNames.Add("Slot " + (script.CurrentWeaponController.enumNames.Count + 1));
										script.CurrentWeaponController.OriginalScript.enumNames.Add("Slot " + (script.CurrentWeaponController.enumNames.Count + 1));
										

										script.CurrentWeaponController.CurrentWeaponInfo.Add(new WeaponsHelper.WeaponInfo());

										script.CurrentWeaponController.settingsSlotIndex = script.CurrentWeaponController.enumNames.Count - 1;
										
										if (script.CurrentWeaponController.OriginalScript)
											EditorUtility.SetDirty(script.CurrentWeaponController.OriginalScript);
									}
									EditorGUILayout.EndHorizontal();
									
									

									if (script.delete)
									{
										EditorGUILayout.BeginVertical("helpbox");
										EditorGUILayout.LabelField("Are you sure?");
										EditorGUILayout.BeginHorizontal();


										if (GUILayout.Button("No"))
										{
											script.delete = false;
										}

										if (GUILayout.Button("Yes"))
										{
											Helper.HideAllObjects(script.CurrentWeaponController.IkObjects);
											Selection.activeObject = script.gameObject;

											script.CurrentWeaponController.WeaponInfos.Remove(script.Weapons[script.weaponIndex]
												.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex]);

											script.CurrentWeaponController.CurrentWeaponInfo.Remove(
												script.CurrentWeaponController.CurrentWeaponInfo[script.CurrentWeaponController.settingsSlotIndex]);

											script.CurrentWeaponController.enumNames.Remove(script.Weapons[script.weaponIndex]
												.enumNames[script.CurrentWeaponController.settingsSlotIndex]);

											script.CurrentWeaponController.OriginalScript.WeaponInfos.Remove(
												script.CurrentWeaponController.OriginalScript.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex]);

											script.CurrentWeaponController.OriginalScript.enumNames.Remove(
												script.CurrentWeaponController.OriginalScript.enumNames[script.CurrentWeaponController.settingsSlotIndex]);

											var newInfoIndex = script.CurrentWeaponController.settingsSlotIndex;

											newInfoIndex++;
											if (newInfoIndex > script.CurrentWeaponController.WeaponInfos.Count - 1)
											{
												newInfoIndex = 0;
											}

											script.CurrentWeaponController.settingsSlotIndex = newInfoIndex;
											script.delete = false;
										}

										EditorGUILayout.EndHorizontal();
										EditorGUILayout.EndVertical();
									}
									else if (script.rename)
									{
										EditorGUILayout.BeginVertical("helpbox");
										script.curName = EditorGUILayout.TextField("New name", script.curName);

										EditorGUILayout.BeginHorizontal();

										if (GUILayout.Button("Cancel"))
										{
											script.rename = false;
											script.curName = "";
											script.renameError = false;
										}

										if (GUILayout.Button("Save"))
										{
											if (script.curName == "")
												script.curName = "...empty name...";
											
											if (!script.CurrentWeaponController.enumNames.Contains(curName))
											{
												script.rename = false;
												script.CurrentWeaponController.enumNames[script.CurrentWeaponController.settingsSlotIndex] = script.curName;
												script.CurrentWeaponController.OriginalScript.enumNames[script.CurrentWeaponController.settingsSlotIndex] = script.curName;
												script.curName = "";
												script.renameError = false;
											}
											else
											{
												script.renameError = true;
											}
										}

										EditorGUILayout.EndHorizontal();

										if (script.renameError)
											EditorGUILayout.HelpBox("This name already exist.", MessageType.Warning);

										EditorGUILayout.EndVertical();
									}
									EditorGUILayout.EndVertical();
									EditorGUILayout.EndVertical();
									// EditorGUILayout.EndVertical();


									#endregion

//									if (script.CurrentWeaponController.Attacks.All(attack => attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade))
//									{
									EditorGUILayout.Space();
									// GUILayout.BeginVertical(script.CurrentWeaponController.gameObject.name + " Settings | "+ "Slot: " + script.CurrentWeaponController.enumNames[script.CurrentWeaponController.settingsSlotIndex], "window");
									script.ikInspectorTab = GUILayout.Toolbar(script.ikInspectorTab, new[] {"Hands", "Elbows", "Fingers"});
									
									// EditorGUILayout.LabelField(script.CurrentWeaponController.gameObject.name + " Settings | "+ "Slot: " + script.CurrentWeaponController.enumNames[script.CurrentWeaponController.settingsSlotIndex], EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical(grayBackground);

									switch (script.ikInspectorTab)
									{
										case 0:
											EditorGUILayout.BeginVertical("HelpBox");
											EditorGUILayout.PropertyField(script.SerializedWeaponController.FindProperty("DebugMode"), new GUIContent("IK Mode"));
											EditorGUILayout.EndVertical();
											
											EditorGUILayout.Space();

											if(script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Crouch && script.currentController && script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson)
												EditorGUILayout.HelpBox("You don't need to adjust hands in crouch state for the FP view.", MessageType.Info);
											
											EditorGUILayout.Space();

											if (script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Norm)
											{
												EditorGUILayout.BeginVertical("HelpBox");
												curInfo.disableIkInNormalState = EditorGUILayout.ToggleLeft("Disable IK in this state", curInfo.disableIkInNormalState);
												EditorGUILayout.EndVertical();
												EditorGUILayout.Space();
											}
											else if (script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Crouch)
											{
												EditorGUI.BeginDisabledGroup(script.currentController && script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson);
												EditorGUILayout.BeginVertical("HelpBox");
												curInfo.disableIkInCrouchState = EditorGUILayout.ToggleLeft("Disable IK in this state", curInfo.disableIkInCrouchState);
												EditorGUILayout.EndVertical();
												EditorGUI.EndDisabledGroup();
												EditorGUILayout.Space();
											}

											if (script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Norm && !curInfo.disableIkInNormalState ||
											    script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Crouch && !curInfo.disableIkInCrouchState
											    || script.CurrentWeaponController.DebugMode != IKHelper.IkDebugMode.Norm
											    || script.CurrentWeaponController.DebugMode != IKHelper.IkDebugMode.Crouch)
											{
												switch (script.CurrentWeaponController.DebugMode)
												{
													case IKHelper.IkDebugMode.Norm:
														if (!curInfo.disableIkInNormalState)
														{
															
															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Right Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															script.CurrentWeaponController.IkObjects.RightObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.RightObject, typeof(Transform), true);
															EditorGUI.EndDisabledGroup();
															
															EditorGUILayout.Space();

															script.foldoutRightHandPositions = EditorGUILayout.Foldout(script.foldoutRightHandPositions, "Position & Rotation");

															if (script.foldoutRightHandPositions)
															{
																EditorGUILayout.BeginVertical("HelpBox");
																script.CurrentWeaponController.IkObjects.RightObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.RightObject.localPosition);
																script.CurrentWeaponController.IkObjects.RightObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.RightObject.localEulerAngles);
																EditorGUILayout.EndVertical();
															}

															EditorGUILayout.EndVertical();
															
															EditorGUILayout.Space();
															
															
															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Left Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.CurrentWeaponController.IkObjects.LeftObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.LeftObject, typeof(Transform), true);
																EditorGUILayout.Space();
															}				
															EditorGUI.EndDisabledGroup();

															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.foldoutLeftHandPositions = EditorGUILayout.Foldout(script.foldoutLeftHandPositions, "Position & Rotation");
																if (script.foldoutLeftHandPositions)
																{
																	EditorGUILayout.BeginVertical("HelpBox");
																	script.CurrentWeaponController.IkObjects.LeftObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.LeftObject.localPosition);
																	script.CurrentWeaponController.IkObjects.LeftObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.LeftObject.localEulerAngles);
																	EditorGUILayout.EndVertical();
																}
																EditorGUILayout.Space();
															}
															
															script.CurrentWeaponController.pinLeftObject = EditorGUILayout.ToggleLeft("Pin Hand", script.CurrentWeaponController.pinLeftObject);
															EditorGUILayout.LabelField("If this checkbox is active, the left hand depends on the right.", style);

															EditorGUILayout.EndVertical();

															EditorGUILayout.Space();
														}

														break;
													case IKHelper.IkDebugMode.Aim:

														EditorGUILayout.BeginVertical("HelpBox");
														EditorGUILayout.LabelField("Right Hand:", EditorStyles.boldLabel);
														EditorGUI.BeginDisabledGroup(true);
														script.CurrentWeaponController.IkObjects.RightAimObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.RightAimObject, typeof(Transform), true);
														EditorGUI.EndDisabledGroup();
														
														EditorGUILayout.Space();

														script.foldoutRightHandPositions = EditorGUILayout.Foldout(script.foldoutRightHandPositions, "Position & Rotation");

														if (script.foldoutRightHandPositions)
														{
															EditorGUILayout.BeginVertical("HelpBox");
															script.CurrentWeaponController.IkObjects.RightAimObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.RightAimObject.localPosition);
															script.CurrentWeaponController.IkObjects.RightAimObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.RightAimObject.localEulerAngles);
															EditorGUILayout.EndVertical();
														}
														
														EditorGUILayout.EndVertical();
														EditorGUILayout.Space();
														
														EditorGUILayout.BeginVertical("HelpBox");
														EditorGUILayout.LabelField("Left Hand:", EditorStyles.boldLabel);
														EditorGUI.BeginDisabledGroup(true);
														if (!script.CurrentWeaponController.pinLeftObject)
														{
															script.CurrentWeaponController.IkObjects.LeftAimObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.LeftAimObject, typeof(Transform), true);
															EditorGUILayout.Space();
														}
														EditorGUI.EndDisabledGroup();
														
														if (!script.CurrentWeaponController.pinLeftObject)
														{
															script.foldoutLeftHandPositions = EditorGUILayout.Foldout(script.foldoutLeftHandPositions, "Position & Rotation");
															if (script.foldoutLeftHandPositions)
															{
																EditorGUILayout.BeginVertical("HelpBox");
																script.CurrentWeaponController.IkObjects.LeftAimObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.LeftAimObject.localPosition);
																script.CurrentWeaponController.IkObjects.LeftAimObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.LeftAimObject.localEulerAngles);
																EditorGUILayout.EndVertical();
															}
															EditorGUILayout.Space();
														}
														
														script.CurrentWeaponController.pinLeftObject = EditorGUILayout.ToggleLeft("Pin Hand", script.CurrentWeaponController.pinLeftObject);
														EditorGUILayout.LabelField("If this checkbox is active, the left hand depends on the right.", style);
														EditorGUILayout.EndVertical();
														
														EditorGUILayout.Space();

														break;
													case IKHelper.IkDebugMode.ObjectDetection:
														
															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Right Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															script.CurrentWeaponController.IkObjects.RightWallObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.RightWallObject, typeof(Transform), true);
															EditorGUI.EndDisabledGroup();
															
															EditorGUILayout.Space();

															script.foldoutRightHandPositions = EditorGUILayout.Foldout(script.foldoutRightHandPositions, "Position & Rotation");

															if (script.foldoutRightHandPositions)
															{
																EditorGUILayout.BeginVertical("HelpBox");
																script.CurrentWeaponController.IkObjects.RightWallObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.RightWallObject.localPosition);
																script.CurrentWeaponController.IkObjects.RightWallObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.RightWallObject.localEulerAngles);
																EditorGUILayout.EndVertical();
															}
															
															EditorGUILayout.EndVertical();
															EditorGUILayout.Space();

															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Left Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.CurrentWeaponController.IkObjects.LeftWallObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.LeftWallObject, typeof(Transform), true);
																EditorGUILayout.Space();
															}														
															EditorGUI.EndDisabledGroup();
															
															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.foldoutLeftHandPositions = EditorGUILayout.Foldout(script.foldoutLeftHandPositions, "Position & Rotation");
																if (script.foldoutLeftHandPositions)
																{
																	EditorGUILayout.BeginVertical("HelpBox");
																	script.CurrentWeaponController.IkObjects.LeftWallObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.LeftWallObject.localPosition);
																	script.CurrentWeaponController.IkObjects.LeftWallObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.LeftWallObject.localEulerAngles);
																	EditorGUILayout.EndVertical();
																}
																EditorGUILayout.Space();
															}
															
															script.CurrentWeaponController.pinLeftObject = EditorGUILayout.ToggleLeft("Pin Hand", script.CurrentWeaponController.pinLeftObject);
															EditorGUILayout.LabelField("If this checkbox is active, the left hand depends on the right.", style);
															EditorGUILayout.EndVertical();

															EditorGUILayout.Space();

															break;
													case IKHelper.IkDebugMode.Crouch:

														EditorGUI.BeginDisabledGroup(script.currentController && script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson);

														if (!curInfo.disableIkInCrouchState)
														{
															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Right Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															script.CurrentWeaponController.IkObjects.RightCrouchObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.RightCrouchObject, typeof(Transform), true);
															EditorGUI.EndDisabledGroup();
															
															EditorGUILayout.Space();

															script.foldoutRightHandPositions = EditorGUILayout.Foldout(script.foldoutRightHandPositions, "Position & Rotation");

															if (script.foldoutRightHandPositions)
															{
																EditorGUILayout.BeginVertical("HelpBox");
																script.CurrentWeaponController.IkObjects.RightCrouchObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.RightCrouchObject.localPosition);
																script.CurrentWeaponController.IkObjects.RightCrouchObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.RightCrouchObject.localEulerAngles);
																EditorGUILayout.EndVertical();
															}
															
															EditorGUILayout.EndVertical();
															EditorGUILayout.Space();
															
															EditorGUILayout.BeginVertical("HelpBox");
															EditorGUILayout.LabelField("Left Hand:", EditorStyles.boldLabel);
															EditorGUI.BeginDisabledGroup(true);
															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.CurrentWeaponController.IkObjects.LeftCrouchObject = (Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.LeftCrouchObject, typeof(Transform), true);
																EditorGUILayout.Space();
															}															
															EditorGUI.EndDisabledGroup();
															
															if (!script.CurrentWeaponController.pinLeftObject)
															{
																script.foldoutLeftHandPositions = EditorGUILayout.Foldout(script.foldoutLeftHandPositions, "Position & Rotation");
																if (script.foldoutLeftHandPositions)
																{
																	EditorGUILayout.BeginVertical("HelpBox");
																	script.CurrentWeaponController.IkObjects.LeftCrouchObject.localPosition = EditorGUILayout.Vector3Field("Local Position", script.CurrentWeaponController.IkObjects.LeftCrouchObject.localPosition);
																	script.CurrentWeaponController.IkObjects.LeftCrouchObject.localEulerAngles = EditorGUILayout.Vector3Field("Local Rotation", script.CurrentWeaponController.IkObjects.LeftCrouchObject.localEulerAngles);
																	EditorGUILayout.EndVertical();
																}
																EditorGUILayout.Space();
															}
															
															script.CurrentWeaponController.pinLeftObject = EditorGUILayout.ToggleLeft("Pin Hand", script.CurrentWeaponController.pinLeftObject);
															EditorGUILayout.LabelField("If this checkbox is active, the left hand depends on the right.", style);
															EditorGUILayout.EndVertical();
															
															EditorGUILayout.Space();
														}
															
														EditorGUI.EndDisabledGroup();
														break;
												}
											}

											EditorGUILayout.Space();
											EditorGUILayout.Space();
											
											EditorGUI.BeginDisabledGroup(script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Crouch && script.currentController && script.currentController.TypeOfCamera == CharacterHelper.CameraType.FirstPerson);

											EditorGUILayout.LabelField("Copy values from:", EditorStyles.boldLabel);
											EditorGUILayout.BeginVertical("HelpBox");
											copyToList.DoLayoutList();

											if (GUILayout.Button("Copy"))
											{
												if (script.CurrentWeaponController.pinLeftObject)
												{
													script.CurrentWeaponController.pinLeftObject = false;
													script.StartCoroutine("CopyTimeout");
												}
												else
												{
													script.CopyWeaponData();
												}
											}
											
											EditorGUILayout.EndVertical();
											
											EditorGUI.EndDisabledGroup();
											
											break;

										case 1:

											EditorGUILayout.BeginVertical("HelpBox");
											curInfo.disableElbowIK = EditorGUILayout.ToggleLeft("Disable Elbow IK", curInfo.disableElbowIK);
											EditorGUILayout.EndVertical();
											EditorGUILayout.Space();

											if ((script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Norm && !curInfo.disableIkInNormalState ||
											     script.CurrentWeaponController.DebugMode == IKHelper.IkDebugMode.Crouch && !curInfo.disableIkInCrouchState
											     || script.CurrentWeaponController.DebugMode != IKHelper.IkDebugMode.Norm
											     || script.CurrentWeaponController.DebugMode != IKHelper.IkDebugMode.Crouch) && !curInfo.disableElbowIK)
											{

												EditorGUILayout.BeginVertical("HelpBox");
												EditorGUILayout.LabelField("Right Elbow:", EditorStyles.boldLabel);
												EditorGUI.BeginDisabledGroup(true);
												script.CurrentWeaponController.IkObjects.RightElbowObject =
													(Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.RightElbowObject, typeof(Transform), true);
												EditorGUI.EndDisabledGroup();
												EditorGUILayout.EndVertical();
												
												EditorGUILayout.Space();
												
												EditorGUILayout.BeginVertical("HelpBox");
												EditorGUILayout.LabelField("Left Elbow:", EditorStyles.boldLabel);
												EditorGUI.BeginDisabledGroup(true);
												script.CurrentWeaponController.IkObjects.LeftElbowObject =
													(Transform) EditorGUILayout.ObjectField(script.CurrentWeaponController.IkObjects.LeftElbowObject, typeof(Transform), true);
												EditorGUI.EndDisabledGroup();
												EditorGUILayout.EndVertical();
											}

											EditorGUILayout.Space();
											EditorGUILayout.Space();

											
											EditorGUILayout.LabelField("Copy values from:", EditorStyles.boldLabel);
											EditorGUILayout.BeginVertical("HelpBox");
											copyToList.DoLayoutList();

											if (GUILayout.Button("Copy"))
											{
												script.CurrentWeaponController.CurrentWeaponInfo[script.CurrentWeaponController.settingsSlotIndex]
													.ElbowsClone(script.Weapons[script.copyFromWeaponSlot].WeaponInfos[script.copyFromSlot]);
											}
											EditorGUILayout.EndVertical();

											break;


										case 2:
											
											EditorGUILayout.BeginVertical("HelpBox");
											axises = (Helper.RotationAxes) EditorGUILayout.EnumPopup("Fingers rotation axis", axises);
											EditorGUILayout.EndVertical();
											
											EditorGUILayout.Space();
											
											EditorGUILayout.BeginVertical("HelpBox");
											switch (axises)
											{
												case Helper.RotationAxes.X:

													curInfo.FingersRightX = EditorGUILayout.Slider("Right Fingers", curInfo.FingersRightX, -25, 25);

													curInfo.ThumbRightX = EditorGUILayout.Slider("Right Thumb", curInfo.ThumbRightX, -25, 25);

													EditorGUILayout.Space();

													curInfo.FingersLeftX = EditorGUILayout.Slider("Left Fingers", curInfo.FingersLeftX, -25, 25);

													curInfo.ThumbLeftX = EditorGUILayout.Slider("Left Thumb", curInfo.ThumbLeftX, -25, 25);

													break;
												case Helper.RotationAxes.Y:

													curInfo.FingersRightY = EditorGUILayout.Slider("Right Fingers", curInfo.FingersRightY, -25, 25);

													curInfo.ThumbRightY = EditorGUILayout.Slider("Right Thumb", curInfo.ThumbRightY, -25, 25);

													EditorGUILayout.Space();

													curInfo.FingersLeftY = EditorGUILayout.Slider("Left Fingers", curInfo.FingersLeftY, -25, 25);

													curInfo.ThumbLeftY = EditorGUILayout.Slider("Left Thumb", curInfo.ThumbLeftY, -25, 25);

													break;
												case Helper.RotationAxes.Z:

													curInfo.FingersRightZ = EditorGUILayout.Slider("Right Fingers", curInfo.FingersRightZ, -25, 25);

													curInfo.ThumbRightZ = EditorGUILayout.Slider("Right Thumb", curInfo.ThumbRightZ, -25, 25);

													EditorGUILayout.Space();

													curInfo.FingersLeftZ = EditorGUILayout.Slider("Left Fingers", curInfo.FingersLeftZ, -25, 25);

													curInfo.ThumbLeftZ = EditorGUILayout.Slider("Left Thumb", curInfo.ThumbLeftZ, -25, 25);

													break;
											}
											EditorGUILayout.EndVertical();

											EditorGUILayout.Space();
											EditorGUILayout.Space();

											
											EditorGUILayout.LabelField("Copy values from:", EditorStyles.boldLabel);
											EditorGUILayout.BeginVertical("HelpBox");
											copyToList.DoLayoutList();

											if (GUILayout.Button("Copy"))
											{
												script.CurrentWeaponController.CurrentWeaponInfo[script.CurrentWeaponController.settingsSlotIndex].FingersClone(script.Weapons[script.copyFromWeaponSlot].WeaponInfos[script.copyFromSlot]);
											}

											EditorGUILayout.EndVertical();
											break;
									}

									EditorGUILayout.EndVertical();
									EditorGUILayout.Space();
									EditorGUILayout.Space();

									if (script.CurrentWeaponController.WeaponInfos.Count > 0 && !script.CurrentWeaponController.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex].HasTime)
										EditorGUILayout.LabelField("Not any save", style);
									else
									{
										var time = script.CurrentWeaponController.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex].SaveTime;
										var date = script.CurrentWeaponController.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex].SaveDate;
										EditorGUILayout.LabelField("Last Save: " + date.x + "/" + date.y + "/" + date.z + " " + time.x + ":" + time.y + ":" + time.z, style);
									}

									if (GUILayout.Button("Save"))
									{

										curInfo.SaveDate = new Vector3(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
										curInfo.SaveTime = new Vector3(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

										curInfo.HasTime = true;

										if (script.CurrentWeaponController.pinLeftObject)
										{
											script.CurrentWeaponController.pinLeftObject = false;
											script.StartCoroutine("SaveTimeout");
										}
										else
										{
											script.CurrentWeaponController.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex].Clone(curInfo);

											script.SaveData();

											IKHelper.CheckIK(ref script.CurrentWeaponController.CanUseElbowIK,
												ref script.CurrentWeaponController.CanUseIK, ref script.CurrentWeaponController.CanUseAimIK,
												ref script.CurrentWeaponController.CanUseWallIK, ref script.CurrentWeaponController.CanUseCrouchIK, curInfo);

											if (!script.CurrentWeaponController.CanUseIK)
												script.CurrentWeaponController.CanUseIK = true;
										}
									}

									EditorGUI.BeginDisabledGroup(!curInfo.HasTime);
									if (GUILayout.Button("Return values from the Last Save"))
									{
										curInfo.Clone(script.CurrentWeaponController.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex]);

										if (curInfo.WeaponSize != Vector3.zero) script.CurrentWeaponController.transform.localScale = curInfo.WeaponSize;
										else script.CurrentWeaponController.transform.localScale = script.currentScales[script.weaponIndex];

										script.CurrentWeaponController.transform.localPosition = curInfo.WeaponPosition;
										script.CurrentWeaponController.transform.localEulerAngles = curInfo.WeaponRotation;
										
										PlaceHand(script.CurrentWeaponController.IkObjects.RightObject, curInfo.RightHandPosition, curInfo.RightHandRotation, "right");
										PlaceHand(script.CurrentWeaponController.IkObjects.LeftObject, curInfo.LeftHandPosition, curInfo.LeftHandRotation, "left");
										
										PlaceHand(script.CurrentWeaponController.IkObjects.RightCrouchObject, curInfo.RightCrouchHandPosition, curInfo.RightCrouchHandRotation, "right");
										PlaceHand(script.CurrentWeaponController.IkObjects.LeftCrouchObject, curInfo.LeftCrouchHandPosition, curInfo.LeftCrouchHandRotation, "left");

										PlaceHand(script.CurrentWeaponController.IkObjects.RightAimObject, curInfo.RightAimPosition, curInfo.RightAimRotation, "right");
										PlaceHand(script.CurrentWeaponController.IkObjects.LeftAimObject, curInfo.LeftAimPosition, curInfo.LeftAimRotation, "left");
										
										PlaceHand(script.CurrentWeaponController.IkObjects.RightWallObject, curInfo.RightHandWallPosition, curInfo.RightHandWallRotation, "right");
										PlaceHand(script.CurrentWeaponController.IkObjects.LeftWallObject, curInfo.LeftHandWallPosition, curInfo.LeftHandWallRotation, "left");

										if (curInfo.LeftElbowPosition != Vector3.zero)
										{
											script.CurrentWeaponController.IkObjects.LeftElbowObject.localPosition = script.CurrentWeaponController
												.WeaponInfos[script.CurrentWeaponController.settingsSlotIndex].LeftElbowPosition;
										}
										else
										{
											script.CurrentWeaponController.IkObjects.LeftElbowObject.localPosition =
												script.currentController.DirectionObject.position - script.currentController.DirectionObject.right * 2;
										}

										if (curInfo.RightElbowPosition != Vector3.zero)
										{
											script.CurrentWeaponController.IkObjects.RightElbowObject.localPosition =
												curInfo.RightElbowPosition;
										}
										else
										{
											script.CurrentWeaponController.IkObjects.RightElbowObject.localPosition =
												script.currentController.DirectionObject.position + script.currentController.DirectionObject.right * 2;
										}
									}

									EditorGUI.EndDisabledGroup();

									if (GUILayout.Button("Set default values"))
									{

//										script.CurrentWeaponController.IkObjects.RightObject.parent = script.CurrentWeaponController.BodyObjects.RightHand;
//										script.CurrentWeaponController.IkObjects.RightObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.RightObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.LeftObject.parent = script.CurrentWeaponController.BodyObjects.LeftHand;
//										script.CurrentWeaponController.IkObjects.LeftObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.LeftObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.RightCrouchObject.parent = script.CurrentWeaponController.BodyObjects.RightHand;
//										script.CurrentWeaponController.IkObjects.RightCrouchObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.RightCrouchObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.LeftCrouchObject.parent = script.CurrentWeaponController.BodyObjects.LeftHand;
//										script.CurrentWeaponController.IkObjects.LeftCrouchObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.LeftCrouchObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.RightAimObject.parent = script.CurrentWeaponController.BodyObjects.RightHand;
//										script.CurrentWeaponController.IkObjects.RightAimObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.RightAimObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.LeftAimObject.parent = script.CurrentWeaponController.BodyObjects.LeftHand;
//										script.CurrentWeaponController.IkObjects.LeftAimObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.LeftAimObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.RightWallObject.parent = script.CurrentWeaponController.BodyObjects.RightHand;
//										script.CurrentWeaponController.IkObjects.RightWallObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.RightWallObject.localRotation = Quaternion.Euler(-90, 0, 0);
//
//										script.CurrentWeaponController.IkObjects.LeftWallObject.parent = script.CurrentWeaponController.BodyObjects.LeftHand;
//										script.CurrentWeaponController.IkObjects.LeftWallObject.localPosition = Vector3.zero;
//										script.CurrentWeaponController.IkObjects.LeftWallObject.localRotation = Quaternion.Euler(-90, 0, 0);

										script.currentController.inventoryManager.debugIKValue = 0;
										script.StartCoroutine(script.SetDefault());

//										script.CurrentWeaponController.IkObjects.LeftElbowObject.localPosition =
//											script.currentController.DirectionObject.position - script.currentController.DirectionObject.right * 2;
//										script.CurrentWeaponController.IkObjects.RightElbowObject.localPosition =
//											script.currentController.DirectionObject.position + script.currentController.DirectionObject.right * 2;


										curInfo = new WeaponsHelper.WeaponInfo();


										script.CurrentWeaponController.transform.localPosition = curInfo.WeaponPosition;
										script.CurrentWeaponController.transform.localEulerAngles = curInfo.WeaponRotation;

										if (curInfo.WeaponSize != Vector3.zero) script.CurrentWeaponController.transform.localScale = curInfo.WeaponSize;
										else script.CurrentWeaponController.transform.localScale = script.currentScales[script.weaponIndex];

									}
								}

							}

							#endregion

							break;
					}

					break;
			}

			var inputs = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
			
			if (!Application.isPlaying && inputs.oldScenePath != "" && script.generalInspectorTab == 0)
			{
				EditorGUILayout.Space();
				if (GUILayout.Button("Back to the [" + inputs.oldSceneName + "] scene"))
				{
					if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] {SceneManager.GetActiveScene()}))
						EditorSceneManager.OpenScene(inputs.oldScenePath, OpenSceneMode.Single);

					return;
				}
			}

			serializedObject.ApplyModifiedProperties();

			if (script.SerializedWeaponController != null)
				script.SerializedWeaponController.ApplyModifiedProperties();

			// DrawDefaultInspector();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);

				if (script.CurrentWeaponController)
					EditorUtility.SetDirty(script.CurrentWeaponController);

				if (!Application.isPlaying)
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		void PlaceHand(Transform hand, Vector3 position, Vector3 rotation, string side)
		{
			if (position != Vector3.zero && rotation != Vector3.zero)
			{
				hand.localPosition = position;
				hand.localEulerAngles = rotation;
			}
			else
			{
				IKHelper.PlaceIKObject(script.currentController, hand, Vector3.zero, Vector3.zero, script.currentController.BodyObjects.TopBody, side == "right" ? script.CurrentWeaponController.BodyObjects.RightHand : script.CurrentWeaponController.BodyObjects.LeftHand, side);
			}
		}
	}
}
