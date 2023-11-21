using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GercStudio.USK.Scripts
{

    [CustomEditor(typeof(PickupItem)), CanEditMultipleObjects]
    public class PickupItemEditor : Editor
    {
        public PickupItem script;

        private GUIStyle grayBackground;

        private void Awake()
        {
            script = (PickupItem) target;

            if (script.gameObject.GetComponent<WeaponController>())
                script.type = PickupItem.TypeOfPickUp.Weapon;
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

            if(string.IsNullOrEmpty(script.pickUpId))
                script.pickUpId = Helper.GenerateRandomString(20);
        }

        public override void OnInspectorGUI()
        {
            Helper.InitStyles(ref grayBackground, new Color32(160, 160, 160, 200));

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"), new GUIContent("Type"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"), new GUIContent("Rotation Speed°", "Leave the value at 0 if you don't need to rotate this object."));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("method"), new GUIContent("Method°", "• Collider - enter a collider to pick up the item" + "\n" +
                                                                                                             "• Raycast - aim the camera at an object and press the button (adjusted in the Input menu) to pick it up" + "\n" +
                                                                                                             "• Both - both of these methods"));

            var backgroundColor = GUI.backgroundColor;
            switch (script.method)
            {
                case PickupItem.PickUpMethod.Raycast:

                    GUI.backgroundColor = new Color(1, 0.7f, 0, 0.5f);
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("distance"), new GUIContent("Max Raycast Distance"));
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = backgroundColor;

                    break;
                case PickupItem.PickUpMethod.Collider:
                    GUI.backgroundColor = new Color(0, 0.3f, 0.7f, 0.5f);
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("colliderSize"), new GUIContent("Collider Size"));
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = backgroundColor;

                    break;
                case PickupItem.PickUpMethod.Both:

                    GUI.backgroundColor = new Color(1, 0.7f, 0, 0.5f);
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("distance"), new GUIContent("Max Raycast Distance"));
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = backgroundColor;

                    GUI.backgroundColor = new Color(0, 0.3f, 0.7f, 0.5f);
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("colliderSize"), new GUIContent("Collider Size"));
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = backgroundColor;

                    break;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical("helpbox");

            if (script.type == PickupItem.TypeOfPickUp.Ammo || script.type == PickupItem.TypeOfPickUp.Health)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoApply"), new GUIContent("Auto Apply°", "If this value is not active, this kit will be added to the inventory and you can use it there"));
            else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoApply"), new GUIContent("Auto Apply°", "If this value is not active, this weapon will be added to the inventory and you can take it from there"));

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (script.type != PickupItem.TypeOfPickUp.Weapon)
            {
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pickUpAudio"), new GUIContent("Audio"));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }

            switch (script.type)
            {
                case PickupItem.TypeOfPickUp.Health:
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("healthAmount"), new GUIContent("Amount of Health"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryTexture"), new GUIContent("Inventory Image°"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipTexture"), new GUIContent("Blip Image°"));
                    EditorGUILayout.EndVertical();

                    break;

                case PickupItem.TypeOfPickUp.Ammo:
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoAmount"), new GUIContent("Amount of Ammo"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoName"), new GUIContent("Ammo Name°"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryTexture"), new GUIContent("Inventory Image°"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipTexture"), new GUIContent("Blip Image°"));
                    EditorGUILayout.EndVertical();

                    break;

                case PickupItem.TypeOfPickUp.Weapon:
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventorySlot"), new GUIContent("Inventory Slot"));
                    EditorGUILayout.EndVertical();
                    break;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Additionally, you can add the [Rigidbody] component so that the object lies on the ground.", MessageType.Info);

            if (script.type != PickupItem.TypeOfPickUp.Weapon)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("helpbox");

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pickUpId"), new GUIContent("Item ID°", "The id is used for internal processes. Each item must have an unique id to make the Save System work correctly."));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("If you copy/paste items, you must reset the ID!", MessageType.Info);

                if (GUILayout.Button("Reset ID"))
                {
                    script.pickUpId = Helper.GenerateRandomString(20);
                }

                EditorGUILayout.EndVertical();
            }


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
}


