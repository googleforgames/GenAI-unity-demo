using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor {
		
		private GameManager script;

		// private ReorderableList enemiesList;
		// private ReorderableList spawnZonesList;
		private ReorderableList charactersList;
		// private ReorderableList graphicsButtons;
		
		private GUIStyle grayBackground;
		
//		private GUIStyle style;
		
		private void Awake()
		{
			script = (GameManager) target;
		}

		private void OnEnable()
		{
			charactersList = new ReorderableList(serializedObject, serializedObject.FindProperty("Characters"), true, true, true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Prefabs");
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), "Spawn Zones");
				},
				
				onAddCallback = items =>
				{
					script.Characters.Add(new Helper.CharacterInGameManager());
				},

				onRemoveCallback = items =>
				{
					script.Characters.Remove(script.Characters[items.index]);
				},
				
				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.Characters[index].characterPrefab = (GameObject) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.Characters[index].characterPrefab, typeof(GameObject), false);

					var backgroundColor = GUI.backgroundColor;

					if(script.Characters[index].spawnZone)
						GUI.backgroundColor = script.Characters[index].spawnZone.color;
						
					script.Characters[index].spawnZone = (SpawnZone) EditorGUI.ObjectField(new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.Characters[index].spawnZone, typeof(SpawnZone), true);

					GUI.backgroundColor = backgroundColor;
					
				}
			};
			
			// spawnZonesList = new ReorderableList(serializedObject, serializedObject.FindProperty("EnemiesSpawnZones"), false, true, true, true)
			// {
			// 	drawHeaderCallback = rect =>
			// 	{
			// 		EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Spawn Zones");
			// 	},
			// 	
			// 	onAddCallback = items =>
			// 	{
			// 		script.EnemiesSpawnZones.Add(null);
			// 	},
			//
			// 	onRemoveCallback = items =>
			// 	{
			// 		script.EnemiesSpawnZones.Remove(script.EnemiesSpawnZones[items.index]);
			// 	},
			// 	
			// 	drawElementCallback = (rect, index, isActive, isFocused) =>
			// 	{
			// 		script.EnemiesSpawnZones[index] = (SpawnZone) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.EnemiesSpawnZones[index], typeof(SpawnZone), true);
			// 	}
			// };

			EditorApplication.update += Update;
		}

		private void OnDisable()
		{
			EditorApplication.update -= Update;
		}

		private void Update()
		{
			
			if (!script || !script.gameObject.activeInHierarchy)
				return;

			if (script.minimapParameters.mapExample)
			{
				if (script.minimapParameters.adjustMapScale && script.minimapParameters.useMinimap)
				{
					if (script.minimapParameters.mapExample.gameObject.hideFlags == HideFlags.HideInHierarchy)
					{
						script.minimapParameters.mapExample.gameObject.hideFlags = HideFlags.None;
						script.minimapParameters.mapExample.gameObject.SetActive(true);
					}
				}
				else
				{
					if (script.minimapParameters.mapExample.gameObject.hideFlags == HideFlags.None)
					{
						script.minimapParameters.mapExample.gameObject.hideFlags = HideFlags.HideInHierarchy;
						script.minimapParameters.mapExample.gameObject.SetActive(false);
					}
				}
			}

			if(Application.isPlaying) return;

			// if (!script.UIManager)
			// {
			// 	script.UIManager = AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/UI Manager.prefab", typeof(UIManager)) as UIManager;
			// 	EditorUtility.SetDirty(script.gameObject);
			// 	EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			// }

			if (!script.minimapParameters.mapExample)
			{
				script.minimapParameters.mapExample = Helper.NewCanvas("Map Example", script.transform).gameObject.AddComponent<RawImage>();
				script.minimapParameters.mapExample.transform.localEulerAngles = new Vector3(90, 0, 0);
				script.minimapParameters.mapExample.transform.localPosition = Vector3.zero;
				var color = script.minimapParameters.mapExample.color;
				color.a = 0.5f;
				script.minimapParameters.mapExample.color = color;
			}
			else
			{
				if (script.minimapParameters.mapTexture && script.minimapParameters.mapExample.texture != script.minimapParameters.mapTexture)
					script.minimapParameters.mapExample.texture = script.minimapParameters.mapTexture;
			}
			
			// if (!script.projectSettings)
			// {
			// 	script.projectSettings = AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
			// 	EditorUtility.SetDirty(script.gameObject);
			// 	EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			// }
		}

		public override void OnInspectorGUI()
		{
		
			Helper.InitStyles(ref grayBackground, new Color32(160,160, 160, 200));

			serializedObject.Update();

//			style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};
			
			EditorGUILayout.Space();
			script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Characters", "Mini-map"});

			EditorGUILayout.Space();

			switch (script.inspectorTab)
			{
				case 0:

					charactersList.DoLayoutList();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					break;
				case 6:

					// enemiesList.DoLayoutList();
					EditorGUILayout.Space();
					// spawnZonesList.DoLayoutList();
					EditorGUILayout.Space();
//					EditorGUILayout.LabelField("<b>Behavior</b> - movement behaviour in the current scene." + "\n\n" +
//					                           "<b>Spawn method</b> - " + "\n" +
//					                           "    <color=blue>Random</color> - one random point from the Spawn Zones will be chosen." + "\n" +
//					                           "    <color=blue>Specific point</color> - set a spawn point for the enemy as you need." + "\n\n" +
//					                           "<b>∞</b>- Spawn enemies constantly." + "\n\n" +
//					                           "<b>Count</b> - " + "\n" +
//					                           "    If <color=blue>∞</color> is not active, this number means how many enemies will spawn during the game." + "\n" +
//					                           "    If <color=blue>∞</color> is active, this number means the limit of enemies in the scene." + "\n\n" +
//					                           "<b>Time</b> - a break between the appearance of enemies (in seconds).", style);
					break;

				case 1:

					EditorGUILayout.BeginVertical("HelpBox");

					script.minimapParameters.useMinimap = EditorGUILayout.ToggleLeft("Use", script.minimapParameters.useMinimap);
					EditorGUILayout.EndVertical();

					if (script.minimapParameters.useMinimap)
					{
						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical(grayBackground);
						EditorGUILayout.BeginVertical("HelpBox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.mapTexture"), new GUIContent("Texture"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						EditorGUILayout.BeginVertical("HelpBox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.mapScale"), new GUIContent("Map Scale"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.blipsScale"), new GUIContent("Blips Scale"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						EditorGUILayout.BeginVertical("HelpBox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.rotateMinimap"), new GUIContent("Rotate Mini-map"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.rotateBlips"), new GUIContent("Rotate Blips"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.blipsAreAlwaysVisible"), new GUIContent("Blips are Always Visible"));
						
						if (script.minimapParameters.blipsAreAlwaysVisible)
						{
							EditorGUILayout.BeginVertical("HelpBox");

							EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.scaleBlipsByDistance"), new GUIContent("Scale by Distance°"));

							if(script.minimapParameters.scaleBlipsByDistance)
								EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.blipsVisibleDistance"), new GUIContent("Visible Distance"));
							
							EditorGUILayout.EndVertical();
						}
						
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						script.minimapParameters.adjustMapScale = GUILayout.Toggle(script.minimapParameters.adjustMapScale, "Adjust Map", "Button");

						if (script.minimapParameters.adjustMapScale)
						{
							EditorGUILayout.BeginVertical("HelpBox");
							EditorGUILayout.HelpBox("Adjust the size, position, and rotation of this plane to fit your map in the scene." + "\n\n" +
							                        "(it is needed to set the dimensions and won't be used during the game)", MessageType.Info);
							EditorGUI.BeginDisabledGroup(true);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.mapExample"), new GUIContent("Map Example"));
							EditorGUI.EndDisabledGroup();
							EditorGUILayout.EndVertical();
						}

						EditorGUILayout.EndVertical();
					}


					break;
			}

			serializedObject.ApplyModifiedProperties();

			// DrawDefaultInspector();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);
				
				if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
}

