using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public SaveManager script;

    private GUIStyle grayBackground;

    private void Awake()
    {
        script = (SaveManager) target;
    }
    
    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    private void Update()
    {
        if (Application.isPlaying || !script) return;

       if(!script.weaponsPool) script.weaponsPool = Resources.Load("Weapons Pool", typeof(WeaponsPool)) as WeaponsPool;
    }
    
     public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            
#if USK_EASYSAVE_INTEGRATION       
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0,1,0,0.5f);

            var style = new GUIStyle{richText = true, fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Easy Save Integration is Enabled", style);
            EditorGUILayout.HelpBox("All progress will be saved using the ES3 features." + "\n" +
                                    "Saved data format, encryption and other parameters can be configured in [Window -> Easy Save 3 -> Settings].", MessageType.Info);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
#endif
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoSave"), new GUIContent("Auto Save"));
            
            if(script.autoSave) EditorGUILayout.PropertyField(serializedObject.FindProperty("autoSaveTime"), new GUIContent("Time (min)"));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveCharacterHealth"), new GUIContent("Save Character Health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveCharacterPosition"), new GUIContent("Save Character Position"));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Weapons", EditorStyles.boldLabel);
            
            if (script.weaponsPool)
            {
                EditorGUILayout.HelpBox("Make sure all the weapons you use in the game (including pickup items) are in the [Weapons Pool].", MessageType.Info);
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponsPool"), new GUIContent("Weapons Pool"));
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("[Weapons Pool] did not found.", MessageType.Error);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveInventory"), new GUIContent("Save Inventory°", "Save the current player's inventory."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveWeaponsAmmoAmount"), new GUIContent("Save Ammo Amount"));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveDroppedWeapons"), new GUIContent("Save Dropped Weapons°", "Keep on the scene the weapons that the player threw."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deletePickedUpObjects"), new GUIContent("Delete Picked Up Items°", "Remove picked up items when loading a save (if disabled, items will be available every time the scene is reloaded)."));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("AI Enemies", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveAIHealth"), new GUIContent("Save AI Health"));
            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();

            // DrawDefaultInspector();


            if (GUI.changed)
            {
                EditorUtility.SetDirty(script.gameObject);
                
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
}
