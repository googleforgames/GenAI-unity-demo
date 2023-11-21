using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(LobbyManager))]
    public class LobbyManagerEditor : Editor
    {
        public LobbyManager script;

        private ReorderableList charactersList;
        private ReorderableList levelsList;
        // private ReorderableList allWeaponsList;
        // private ReorderableList gameModesList;
        private ReorderableList avatarsList;

        private GUIStyle style;

        private int currentMode;

        public void Awake()
        {
            script = (LobbyManager) target;
        }

        public void OnEnable()
        {
            charactersList = new ReorderableList(serializedObject, serializedObject.FindProperty("characters"), true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Character"); },

                onAddCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.characters.Add(null);
                    }
                },

                onRemoveCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.characters.Remove(script.characters[items.index]);
                    }
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var backgroundColor = GUI.backgroundColor;
                    
                    GUI.backgroundColor = script.characters[index] != null && script.characters[index].CharacterSync ? new Color(0,1,0,0.3f): new Color(1, 0,0, 0.3f);
                    
                    script.characters[index] = (Controller) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.characters[index], typeof(Controller), false);
                    
                    GUI.backgroundColor = backgroundColor;
                }
            };

            levelsList = new ReorderableList(serializedObject, serializedObject.FindProperty("allMaps"), true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Scene");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 + 10, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), "Image");
                },

                onAddCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.allMaps.Add(new MultiplayerHelper.MultiplayerLevel());
                        script.currentMapsInEditor.Add(null);
                    }
                },

                onRemoveCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        if (script.allMaps.Count > 1)
                        {
                            script.allMaps.Remove(script.allMaps[items.index]);
                            script.currentMapsInEditor.Remove(script.currentMapsInEditor[items.index]);
                        }
                    }
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.currentMapsInEditor[index] = (SceneAsset) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), script.currentMapsInEditor[index], typeof(SceneAsset), false);
                    script.allMaps[index].image = (Texture) EditorGUI.ObjectField(new Rect(rect.x + rect.width / 2 + 10, rect.y, rect.width / 2 - 10, EditorGUIUtility.singleLineHeight), script.allMaps[index].image, typeof(Texture), true);
                }
            };
            
            avatarsList = new ReorderableList(serializedObject, serializedObject.FindProperty("defaultAvatars"), true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Avatar"); },

                onAddCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.defaultAvatars.Add(null);
                    }
                },

                onRemoveCallback = items =>
                {
                    if (!Application.isPlaying)
                    {
                        script.defaultAvatars.Remove(script.defaultAvatars[items.index]);
                    }
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.defaultAvatars[index] = (Texture) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.defaultAvatars[index], typeof(Texture), false);
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
            if (!Application.isPlaying && script)
            {

                if (script.currentInspectorTab != script.lastInspectorTab)
                {
                    UpdateScenesInBuildSettings();
                    script.lastInspectorTab = script.currentInspectorTab;
                }

                if (script.allMaps.Count != script.currentMapsInEditor.Count)
                {
                    script.currentMapsInEditor.Clear();

                    for (var i = 0; i < script.allMaps.Count; i++)
                    {
                        script.currentMapsInEditor.Add(null);
                    }
                }
                
                if (script.currentInspectorTab == 1)
                {
                    for (var index = 0; index < script.currentMapsInEditor.Count; index++)
                    {
                        var level = script.currentMapsInEditor[index];
                        if (!level) continue;

                        if (!string.Equals(level.name, script.allMaps[index].name, StringComparison.Ordinal))
                        {
                            script.allMaps[index].name = level.name;
                        }
                    }

                    CheckScenesInBuildSettings(script.oldMapsInEditor, script.currentMapsInEditor);
                }
                
                if (!script.characterAnimatorController)
                {
                    script.characterAnimatorController = Resources.Load("Controller for Lobby", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/_Animator Controllers/Controller for Lobby.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;

                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }

                // if (!script.projectSettings)
                // {
                //     script.projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
                // }
                
                // if (script.adjustCameraPosition)// && script.projectSettings)
                // {
                //     switch (script.currentCameraMode)
                //     {
                //         case 0:
                //             script.mainMenuCameraPosition.position = script.defaultCamera.transform.position;
                //             script.mainMenuCameraPosition.rotation = script.defaultCamera.transform.rotation;
                //             break;
                //         case 1:
                //             script.characterSelectorCameraPosition.position = script.defaultCamera.transform.position;
                //             script.characterSelectorCameraPosition.rotation = script.defaultCamera.transform.rotation;
                //             break;
                //     }
                //     
                //     if (script.lastCameraMode != script.currentCameraMode)
                //     {
                //         switch (script.currentCameraMode)
                //         {
                //             case 0:
                //                 script.defaultCamera.transform.position = script.mainMenuCameraPosition.position;
                //                 script.defaultCamera.transform.rotation = script.mainMenuCameraPosition.rotation;
                //                 break;
                //
                //             case 1:
                //                 script.defaultCamera.transform.position = script.characterSelectorCameraPosition.position;
                //                 script.defaultCamera.transform.rotation = script.characterSelectorCameraPosition.rotation;
                //                 break;
                //         }
                //
                //         script.lastCameraMode = script.currentCameraMode;
                //     }
                // }
                
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 11};

            EditorGUILayout.Space();
#if !USK_MULTIPLAYER
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.HelpBox("To use this multiplayer mode, import PUN2 from the Asset Store, then open [Window -> USK -> Integrations Manager] add enable multiplayer modules." + "\n\n" + 
                                    "(If Photon is already in the project and you still see this message, restart Unity)", MessageType.Info);
            if (GUILayout.Button("Open Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
#endif


            script.currentInspectorTab = GUILayout.Toolbar(script.currentInspectorTab, new[] {"Characters", "Maps", "Other"});
            
            EditorGUILayout.Space();

            switch (script.currentInspectorTab)
            {
                case 0:
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSpawnPoint"), new GUIContent("Spawn Point"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("<b><color=green>Green</color></b> - the character is ready for multiplayer." + "\n" +
                                               "<b><color=red>Red</color></b> - the character is not ready for multiplayer, <b>add scripts</b> by clicking on the corresponding button in the <b>Controller</b> script.", style);
                    EditorGUILayout.Space();
                    charactersList.DoLayoutList();
                    EditorGUILayout.Space();
                    break;

                case 1:
                    EditorGUILayout.Space();
                    levelsList.DoLayoutList();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("All scenes will be automatically added to the Build Settings.", style);
                    break;

                case 2:

                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPlayers"), new GUIContent("Max Players"));
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matchTimeLimit"), new GUIContent("Time Limit"));
                    
                    if(script.matchTimeLimit)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("matchTime"), new GUIContent("Duration (sec)"));
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canAttackTeammates"), new GUIContent("Can Attack Each Other"));

                    EditorGUILayout.EndVertical();
                   
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("helpbox");
                    script.checkInternetConnection = EditorGUILayout.Toggle(new GUIContent("Check Internet Connection°","Checking if the game has an internet connection" + "\n" + "Disable this if you are going to build a web game"), script.checkInternetConnection);

                    if (script.checkInternetConnection)
                    {
                        // EditorGUILayout.Space();
                        // EditorGUILayout.LabelField(, style);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("checkConnectionServer"), new GUIContent("Server°", "This server is needed to check the internet connection." + "\n" +
                                                                                                                                       "It should be like 'https://[name].[domain]'"));
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Cameras", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("helpbox");
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultCamera"), new GUIContent("Main Menu"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSelectionCamera"), new GUIContent("Character Selection"));

                    // if (script.defaultCamera)
                    // {
                        // EditorGUILayout.Space();
                        
                        // script.adjustCameraPosition = GUILayout.Toggle(script.adjustCameraPosition, "Adjust Position", "Button");
                      
                        // if (script.adjustCameraPosition)
                        // {
                            // EditorGUILayout.Space();
                            
                            // script.currentCameraMode = GUILayout.Toolbar(script.currentCameraMode, new[] {"Main Menu", "Characters Menu"});
                        // }
                    // }

                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Default Avatars", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("helpbox");
                    avatarsList.DoLayoutList();
                    EditorGUILayout.EndVertical();
                    break;
            }

           

#if !USK_MULTIPLAYER
            EditorGUI.EndDisabledGroup();
#endif

            serializedObject.ApplyModifiedProperties();
            
            // DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script.gameObject);

                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        void UpdateScenesInBuildSettings()
        {
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene> {new EditorBuildSettingsScene(SceneManager.GetActiveScene().path, true)};
            
            foreach (var sceneAsset in script.currentMapsInEditor)
            {
                if (!sceneAsset) continue;

                var scenePath = AssetDatabase.GetAssetPath(sceneAsset);

                if (!string.IsNullOrEmpty(scenePath))
                    editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
//            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }

        void CheckScenesInBuildSettings(List<SceneAsset> oldScenes, List<SceneAsset> currentScenes)
        {
            foreach (var map in currentScenes)
            {
                if (map && !oldScenes.Contains(map))
                {
                    oldScenes.Add(map);
                    UpdateScenesInBuildSettings();
                    break;
                }
            }
                    
            foreach (var map in oldScenes)
            {
                if (!currentScenes.Exists(level => level == map))
                {
                    oldScenes.Remove(map);
                    UpdateScenesInBuildSettings();
                    break;
                }
            }
        }
    }
}


