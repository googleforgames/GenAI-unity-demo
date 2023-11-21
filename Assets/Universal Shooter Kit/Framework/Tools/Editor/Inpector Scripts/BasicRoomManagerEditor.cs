using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{

    [CustomEditor(typeof(RoomManager))]
    public class BasicRoomManagerEditor : Editor
    {
        public RoomManager script;
        
        private ReorderableList spectateCamerasList;
        private ReorderableList spawnList;
        private ReorderableList redSpawnList;
        private ReorderableList blueSpawnList;
        private ReorderableList hardPointsList;
        
        private GUIStyle grayBackground;

        public void Awake()
        {
            script = (RoomManager) target;
        }

        public void OnEnable()
        {
            spawnList = new ReorderableList(serializedObject, serializedObject.FindProperty("PlayersSpawnAreas"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "For all players"); },

                onAddCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.PlayersSpawnAreas.Add(null);
                    }
                },

                onRemoveCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.PlayersSpawnAreas.RemoveAt(items.index);
                        
                        
                    }
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.PlayersSpawnAreas[index] = (SpawnZone) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.PlayersSpawnAreas[index], typeof(SpawnZone), true);
                }
            };

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
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

            if (Application.isPlaying) return;

            if (script)
            {
                // if (!script.UiManager)
                // {
                //     script.UiManager = AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/UI Manager.prefab", typeof(UIManager)) as UIManager;
                //     EditorUtility.SetDirty(script.gameObject);
                //     EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
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
                //     script.projectSettings = AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
                //     EditorUtility.SetDirty(script.gameObject);
                //     EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                // }
            }
        }

        public override void OnInspectorGUI()
        {
            Helper.InitStyles(ref grayBackground, new Color32(160,160, 160, 200));
            
            serializedObject.Update();
            
            EditorGUILayout.Space();
            
#if !USK_MULTIPLAYER
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.HelpBox("To use the multiplayer mode, import PUN2 from Asset Store" + "\n" + 
                                    "(If Photon is already in the project and you still see this message, restart Unity)", MessageType.Info);           
            if (GUILayout.Button("Open Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
#endif
            
            script.currentInspectorTab = GUILayout.Toolbar(script.currentInspectorTab, new[] {"Parameters","Mini-map"});

            EditorGUILayout.Space();
            switch (script.currentInspectorTab)
            {
                case 0:
                    
                    EditorGUILayout.BeginVertical(grayBackground);
                    EditorGUILayout.LabelField(new GUIContent("Spawn Zones°", "Add Spawn Zones if you are going to use this scene with game modes in which the [Use Teams] parameter is not active" + "\n\n" +
                                                                              "During the game all players will be respawned at a random zone"), EditorStyles.boldLabel);

                    spawnList.DoLayoutList();
                    EditorGUILayout.EndVertical();
            
                    EditorGUILayout.Space();
                    break;
                
               
                
                case 1:
                    
                    EditorGUILayout.BeginVertical("helpbox");

                    script.minimapParameters.useMinimap = EditorGUILayout.ToggleLeft("Use", script.minimapParameters.useMinimap);
                    EditorGUILayout.EndVertical();

                    if (script.minimapParameters.useMinimap)
					{
						EditorGUILayout.Space();
     
						EditorGUILayout.BeginVertical(grayBackground);
                        EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.mapTexture"), new GUIContent("Texture"));
                        EditorGUILayout.EndVertical();
                        
						EditorGUILayout.Space();
     
                        EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.mapScale"), new GUIContent("Map Scale"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("minimapParameters.blipsScale"), new GUIContent("Blips Scale"));
                        EditorGUILayout.EndVertical();
                        
						EditorGUILayout.Space();
                        
                        EditorGUILayout.BeginVertical("helpbox");
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

#if !USK_MULTIPLAYER
            EditorGUI.EndDisabledGroup();
#endif
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                
                if(!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

}

