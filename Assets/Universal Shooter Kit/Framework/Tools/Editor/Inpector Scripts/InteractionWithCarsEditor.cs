using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(InteractionWithCars))]
    public class InteractionWithCarsEditor : Editor
    {
        public InteractionWithCars script;
        
        private GUIStyle grayBackground;
        private GUIStyle style;
        
        public void Awake()
        {
            script = (InteractionWithCars) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Helper.InitStyles(ref grayBackground, new Color32(160, 160, 160, 200));
            style = new GUIStyle{richText = true, fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};
            
            EditorGUILayout.Space();

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0,1,0,0.5f);
            
            
#if USK_EVPH_INTEGRATION
EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Edy's Vehicle Physics", style);
            EditorGUILayout.EndVertical();
#elif USK_RCC_INTEGRATION
EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Realistic Car Controller", style);
            EditorGUILayout.EndVertical();
#elif USK_NWHVPH_INTEGRATION
EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("NWH Vehicle Physics 2", style);
            EditorGUILayout.EndVertical();
#endif
            
            
            GUI.backgroundColor = backgroundColor;
            EditorGUILayout.Space();
            
#if !USK_RCC_INTEGRATION && !USK_EVPH_INTEGRATION && !USK_NWHVPH_INTEGRATION
            EditorGUILayout.HelpBox("This script is used for the integrations with vehicle systems" + "\n\n" +
                                    "Import one of the supported packages to the project and activate it in [Window -> USK -> Integrations Manager]", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);

#endif
            EditorGUILayout.BeginVertical(grayBackground);
            EditorGUILayout.LabelField("Car Camera Parameters", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("matchesCharacterCamera"), new GUIContent("Matches Character Camera°", "The car camera will be the same as the camera of the character."));
            
#if USK_RCC_INTEGRATION
            if (!script.matchesCharacterCamera)
            {
                // EditorGUILayout.Space();
                // EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("carCameraMode"), new GUIContent("Fix Camera Mode°", "Set fixed view for the car camera."));
                
            }
#elif USK_EVPH_INTEGRATION
            if (!script.matchesCharacterCamera)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("carCameraMode"), new GUIContent("Fix Camera Mode°", "Set fixed view for the car camera."));
            }
#elif USK_NWHVPH_INTEGRATION
            if (!script.matchesCharacterCamera)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("currentCarCamera"), new GUIContent("Fixed Camera Index°", "Set a fixed camera index that will be selected in all NWH.CameraChanger scripts."));
            }
#endif
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Interaction Parameters", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRayCast"), new GUIContent("Use Raycast°", "• If active - point the camera at the car to interact with it." + "\n" + 
            "• If not, get close to the car to interact with it."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceToInteract"), new GUIContent("Distance to Interact"));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enterExitSpeedLimit"), new GUIContent("Enter/Exit Speed Limit°", "If the car moves at a higher speed than this, the character won't be able to enter or exit it."));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Get Out Position", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox ("Add an empty objects named 'Get Out Pos' to all car prefabs in the scene." + "\n" +
                                     "The character will appear at those points when leaving the cars.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
#if !USK_RCC_INTEGRATION && !USK_EVPH_INTEGRATION
            EditorGUI.EndDisabledGroup();
#endif
            
            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

        }
    }
}
