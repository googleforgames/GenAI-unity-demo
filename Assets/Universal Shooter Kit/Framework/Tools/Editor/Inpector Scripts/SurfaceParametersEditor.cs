using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace GercStudio.USK.Scripts
{

	[CustomEditor(typeof(SurfaceParameters))]
	[CanEditMultipleObjects]
	public class SurfaceParametersEditor : Editor
	{
		
		private SurfaceParameters script;
		private ReorderableList shellsList;
		public ReorderableList[] charactersStepsList = new ReorderableList[0];
		public ReorderableList[] enemiesStepsList = new ReorderableList[0];


		private void Awake()
		{
			script = (SurfaceParameters) target;
		}
		
		void OnEnable()
		{
			EditorApplication.update += Update;
			
			if(!script.projectSettings)
				return;
			
			Array.Resize(ref charactersStepsList, script.projectSettings.CharacterTags.Count);
			Array.Resize(ref enemiesStepsList, script.projectSettings.EnemiesTags.Count);
			
			Array.Resize(ref script.CharacterFootstepsSounds, script.projectSettings.CharacterTags.Count);
			Array.Resize(ref script.EnemyFootstepsSounds, script.projectSettings.EnemiesTags.Count);
			

			for (var i = 0; i < charactersStepsList.Length; i++)
			{
				var i1 = i;
				charactersStepsList[i] = new ReorderableList(serializedObject, serializedObject.FindProperty("CharacterFootstepsSounds")
					.GetArrayElementAtIndex(i).FindPropertyRelative("FootstepsAudios"), false, true, true, true)
				{
					drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Sounds"); },

					onAddCallback = items => { script.CharacterFootstepsSounds[i1].FootstepsAudios.Add(null); },

					onRemoveCallback = items =>
					{
						script.CharacterFootstepsSounds[i1].FootstepsAudios.Remove(script.CharacterFootstepsSounds[i1].FootstepsAudios[items.index]);
					},

					drawElementCallback = (rect, index, isActive, isFocused) =>
					{
						script.CharacterFootstepsSounds[i1].FootstepsAudios[index] =
							(AudioClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.CharacterFootstepsSounds[i1].FootstepsAudios[index], typeof(AudioClip), false);
					}
				};
			}
			
			for (var i = 0; i < enemiesStepsList.Length; i++)
			{
				var i1 = i;
				enemiesStepsList[i] = new ReorderableList(serializedObject, serializedObject.FindProperty("EnemyFootstepsSounds")
					.GetArrayElementAtIndex(i).FindPropertyRelative("FootstepsAudios"), false, true, true, true)
				{
					drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Sounds"); },

					onAddCallback = items => { script.EnemyFootstepsSounds[i1].FootstepsAudios.Add(null); },

					onRemoveCallback = items =>
					{
						script.EnemyFootstepsSounds[i1].FootstepsAudios.Remove(script.EnemyFootstepsSounds[i1].FootstepsAudios[items.index]);
					},

					drawElementCallback = (rect, index, isActive, isFocused) =>
					{
						script.EnemyFootstepsSounds[i1].FootstepsAudios[index] =
							(AudioClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.EnemyFootstepsSounds[i1].FootstepsAudios[index], typeof(AudioClip), false);
					}
				};
			}
			
			shellsList = new ReorderableList(serializedObject, serializedObject.FindProperty("ShellDropSounds"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Sounds"); },

				onAddCallback = items => { script.ShellDropSounds.Add(null); },

				onRemoveCallback = items =>
				{
					script.ShellDropSounds.Remove(script.ShellDropSounds[items.index]);
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.ShellDropSounds[index] =
						(AudioClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.ShellDropSounds[index], typeof(AudioClip), false);
				}
			};
		}

		void OnDisable()
		{
			EditorApplication.update -= Update;
		}

		void Update()
		{
			if (!script.projectSettings)
			{
				script.projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
			}

			// script.inspectorTab = 2;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			
			script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Settings", "Effects", "Step sounds", "Shell sounds"});

			switch (script.inspectorTab)
			{
				case 0:
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("penetrationWidth"), new GUIContent("Penetration Width°", "This value indicates the object width that weapons can shoot through. This parameter will be multiplied by the [Penetration Multiplier] value that is on each weapon."));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ricochetChance"), new GUIContent("Ricochet Chance"));
					EditorGUILayout.EndVertical();
					
					break;

				case 1:
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("Sparks"), new GUIContent("Sparks"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("Hit"), new GUIContent("Hit"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("HitAudio"), new GUIContent("Hit audio"));
					EditorGUILayout.EndVertical();
					break;


				case 2:

					EditorGUILayout.Space();
					
					EditorGUILayout.HelpBox("Add the [PlayStepSound] event with the float parameter [Volume] on the characters' and enemies' movement animations to set the exact playing time of the foot sounds.", MessageType.Info);

					EditorGUILayout.Space();
					script.stepsTab = GUILayout.Toolbar(script.stepsTab, new[] {"Characters", "Enemies"});
					
					if(!script.projectSettings)
						return;
					
					switch (script.stepsTab)
					{
						case 0:
							EditorGUILayout.Space();
							
							EditorGUILayout.BeginVertical("HelpBox");

							script.currentCharacterTag = EditorGUILayout.Popup(new GUIContent("Character ID°:", "This ID is created and set in the [Controller] script."), script.currentCharacterTag, script.projectSettings.CharacterTags.ToArray());
					
							EditorGUILayout.Space();
							for (var i = 0; i < charactersStepsList.Length; i++)
							{
								if(i == script.currentCharacterTag)
									charactersStepsList[i].DoLayoutList();
							}
							
							break;
						
						
						case 1:
							EditorGUILayout.Space();
							
							EditorGUILayout.BeginVertical("HelpBox");
							script.currentEnemyTag = EditorGUILayout.Popup(new GUIContent("Enemy ID°:", "This ID is created and set in the [AIController] script."), script.currentEnemyTag, script.projectSettings.EnemiesTags.ToArray());
					
							EditorGUILayout.Space();
							for (var i = 0; i < enemiesStepsList.Length; i++)
							{
								if(i == script.currentEnemyTag)
									enemiesStepsList[i].DoLayoutList();
							}
							
							break;
					}
					EditorGUILayout.EndVertical();
					

					break;


				case 3:
					EditorGUILayout.Space();
					shellsList.DoLayoutList();

					break;
			}
			
			serializedObject.ApplyModifiedProperties();

			// DrawDefaultInspector();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);
				
				if(!Application.isPlaying)
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

	}
}


