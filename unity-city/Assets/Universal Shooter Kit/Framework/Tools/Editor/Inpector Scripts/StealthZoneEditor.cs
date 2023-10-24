using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(StealthZone))]
    public class StealthZoneEditor : Editor
    {
        private StealthZone script;

        private void Awake()
        {
            script = (StealthZone) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Move and resize this object to adjust the area.", MessageType.Info);
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideOnlyWhenSquatting"), new GUIContent("Hide Only In Crouch State°", "[ON] - A character is invisible only in the Crouch state" + "\n" +
                                                                                                                                               "[OFF] - The character is always invisible in this area (even while standing)"));
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
