using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(SpawnZone))]
    public class SpawnZoneEditor : Editor
    {
        private SpawnZone script;
        
        private void Awake()
        {
            script = (SpawnZone) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Rotate, move and resize this object to adjust the area. The arrow indicates the direction in which opponents and players will appear.", MessageType.Info);
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("color"), new GUIContent("Debug Color"));
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
				
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

        }
    }
}
