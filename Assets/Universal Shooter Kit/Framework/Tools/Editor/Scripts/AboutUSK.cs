using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
	public class AboutUSK : EditorWindow
	{

		private Font font;

		private GUIStyle LabelStyle;

		private Vector2 scrollPos;

		private const string Version = "1.7.3";

		[MenuItem("Tools/Universal Shooter Kit/About", false, -500)]
		public static void ShowWindow()
		{
			GetWindowWithRect(typeof(AboutUSK), new Rect(Vector2.zero, new Vector2(400, 150)), true, "About USK").ShowUtility();
		}

		
		private void Awake()
		{
			if(!font)
				font = Resources.Load("font1", typeof(Font)) as Font;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Textures & Materials/Other/Font/hiragino.otf", typeof(Font)) as Font;
			
			if (LabelStyle != null) return;

			LabelStyle = new GUIStyle
			{
				normal = {textColor = Color.black},
				fontStyle = FontStyle.Bold,
				fontSize = 14,
				alignment = TextAnchor.MiddleCenter
			};
		}
		
		private void OnGUI()
		{
			EditorGUILayout.Space();
			LabelStyle.fontStyle = FontStyle.Bold;
			LabelStyle.fontSize = 15;
			GUILayout.Label("Universal Shooter Kit", LabelStyle);
			GUILayout.Label(Version + " version", LabelStyle);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			LabelStyle.fontStyle = FontStyle.Normal;
			LabelStyle.fontSize = 12;
			GUILayout.Label("Support email: gercstudio@gmail.com", LabelStyle);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.Label("Copyright © 2018 - 2022 GercStudio " + "\n" + "All rights reserved", LabelStyle);
			
		}
	}
}
