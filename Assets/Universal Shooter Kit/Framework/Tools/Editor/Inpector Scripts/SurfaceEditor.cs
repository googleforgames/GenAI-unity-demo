using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GercStudio.USK.Scripts
{
	
	[CustomEditor(typeof(Surface)), CanEditMultipleObjects]
	public class SurfaceEditor : Editor
	{
		private Surface script;
		
		private GUIStyle grayBackground;

		
		private void Awake()
		{
			script = (Surface) target;
			
			if(script.Shadow)
				DestroyImmediate(script.Shadow);
		}

		public override void OnInspectorGUI()
		{
			Helper.InitStyles(ref grayBackground, new Color32(160,160, 160, 200));
			
			serializedObject.Update();
			
			EditorGUILayout.BeginVertical("HelpBox");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Material"), new GUIContent("Material"));

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();

//			DrawDefaultInspector();
           
			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);
				if(!Application.isPlaying)
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

	}
}
