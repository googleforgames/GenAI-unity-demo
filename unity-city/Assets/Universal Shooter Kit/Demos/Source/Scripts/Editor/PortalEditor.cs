using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(Portal))]
    public class PortalEditor : Editor
    {
        private Portal script;
        
        private void Awake()
        {
            script = (Portal) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portalType"), new GUIContent("Type"));

            if (script.portalType == Portal.PortalType.Point)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("destination"), new GUIContent("Destination Point"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneName"), new GUIContent("Scene Name"));
                
                EditorGUILayout.HelpBox("Make sure all scenes are added to the Build Settings!", MessageType.Info);

            }
            
            serializedObject.ApplyModifiedProperties();

            // DrawDefaultInspector();
            
            if (GUI.changed)
                EditorUtility.SetDirty(script.gameObject);

        }
    }
}
