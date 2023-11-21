using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(Cover))]
    public class CoverEditor : Editor
    {

        private Cover script;
        
        private ReorderableList coverPoints;


        private void Awake()
        {
            script = (Cover) target;
        }

        private void OnEnable()
        {
            coverPoints = new ReorderableList(serializedObject, serializedObject.FindProperty("points"), false, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), "Cover Points");
                },

                onAddCallback = items => { script.points.Add(null); },

                onRemoveCallback = items => { script.points.Remove(script.points[items.index]); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.points[index].pointTransform = (Transform) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.points[index].pointTransform, typeof(Transform), true);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // EditorGUILayout.BeginVertical("helpbox");
            // EditorGUILayout.Space();
            
            coverPoints.DoLayoutList();

            // DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();


            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                if(!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
