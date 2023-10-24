using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using UnityEditorInternal;

namespace GercStudio.USK.Scripts
{
	[CustomEditor(typeof(Welcome))]
	[InitializeOnLoad]
	public class WelcomeScriptEditor : Editor
	{
		static string kShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";

		static float kSpace = 16f;

		private Welcome script;

		private static bool firstStart;

		private ReorderableList items;

		private void Awake()
		{
			script = (Welcome) target;
		}


		private void OnEnable()
		{
			items = new ReorderableList(serializedObject, serializedObject.FindProperty("sections"), true, false, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Find Animations"); },

				onAddCallback = items => { script.sections.Add(null); },

				onRemoveCallback = items =>
				{
					script.sections.Remove(script.sections[items.index]);
				}
			};
		}

		static WelcomeScriptEditor()
		{
			EditorApplication.update += SelectReadmeAutomatically;
		}

		static void SelectReadmeAutomatically()
		{
			if (!SessionState.GetBool(kShowedReadmeSessionStateName, false) && !PlayerPrefs.HasKey("USK1.7"))
			{
				var readme = SelectReadme();
				SessionState.SetBool(kShowedReadmeSessionStateName, true);

				PlayerPrefs.SetInt("USK1.7", 1);
				PlayerPrefs.Save();

				if (readme && !readme.loadedLayout)
				{
					LoadLayout();
					readme.loadedLayout = true;
				}
			}
		}

		static void LoadLayout()
		{
			var inspectorWindow = EditorWindow.GetWindow( typeof( Editor ).Assembly.GetType( "UnityEditor.InspectorWindow" ) );
			// Vector2 size = new Vector2( inspectorWindow.position.width, inspectorWindow.position.height );
			// inspectorWindow = Instantiate( inspectorWindow );
			// inspectorWindow.minSize = size;
			// inspectorWindow.Show();
			inspectorWindow.Focus();
			
			// var assembly = typeof(EditorApplication).Assembly;
			// var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
			// var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
			// method.Invoke(null, new object[] {Path.Combine(Application.dataPath, "Universal Shooter Kit/Framework/Editor/Layout.wlt"), false});
		}

		// [MenuItem("Tutorial/Show Tutorial Instructions")]
		static Welcome SelectReadme()
		{
			var ids = AssetDatabase.FindAssets("!Welcome t:Welcome");
			if (ids.Length == 1)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
				
				Selection.objects = new[] {readmeObject};

				return (Welcome) readmeObject;
			}
			else
			{
				// Debug.Log("Couldn't find a readme");
				return null;
			}
		}

		protected override void OnHeaderGUI()
		{
			var readme = (Welcome) target;

			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			var readme = (Welcome) target;
			Init();

			foreach (var section in readme.sections)
			{
				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label("<b>" + section.heading + "</b>", HeadingStyle);
				}

				if (!string.IsNullOrEmpty(section.text))
				{
					GUILayout.Label(section.text, BodyStyle);
				}

				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}

				GUILayout.Space(kSpace);
			}

			// items.DoLayoutList();

			// DrawDefaultInspector();
		}


		bool m_Initialized;

		GUIStyle LinkStyle
		{
			get { return m_LinkStyle; }
		}

		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle
		{
			get { return m_TitleStyle; }
		}

		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle
		{
			get { return m_HeadingStyle; }
		}

		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle
		{
			get { return m_BodyStyle; }
		}

		[SerializeField] GUIStyle m_BodyStyle;

		void Init()
		{
			if (m_Initialized)
				return;
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;
			m_HeadingStyle.richText = true;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}
}

