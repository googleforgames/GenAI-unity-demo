using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace GercStudio.USK.Scripts
{

    [CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : Editor
    {
        private Vector3 defaultSize;

        private InventoryManager script;

        private Animator animator;

        private float startVal;
        private float progress;

        private ReorderableList[] weaponsList = new ReorderableList[8];
        private ReorderableList handsAnimations;
        private ReorderableList fullBodyAnimations;
        
        private GUIStyle grayBackground;

        private bool weaponPrefabWarning;

        private bool greandePrefabWarning;

        public void Awake()
        {
            script = (InventoryManager) target;
        }

        private void OnEnable()
        {
            for (var i = 0; i < 8; i++)
            {
                var i1 = i;
                weaponsList[i] = new ReorderableList(serializedObject, serializedObject.FindProperty("slots").GetArrayElementAtIndex(i).FindPropertyRelative("weaponSlotInInspector"), true, true, true, true)
                {
                    drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Weapons"); },
                    onAddCallback = items =>
                    {
                        if (script.slots[i1] == null)
                            script.slots[i1] = new CharacterHelper.InventorySlot();

                        script.slots[i1].weaponSlotInInspector.Add(null);

                    },

                    onRemoveCallback = items => { script.slots[i1].weaponSlotInInspector.Remove(script.slots[i1].weaponSlotInInspector[items.index]); },

                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        if (!script.slots[i1].weaponSlotInInspector[index].weapon && !script.slots[i1].weaponSlotInInspector[index].fistAttack)
                        {
                            if (!script.HasFistAttack)
                            {
                                script.slots[i1].weaponSlotInInspector[index].weapon = (GameObject) EditorGUI.ObjectField(
                                    new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight),
                                    script.slots[i1].weaponSlotInInspector[index].weapon, typeof(GameObject), false);

                                script.slots[i1].weaponSlotInInspector[index].fistAttack = EditorGUI.ToggleLeft(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight)
                                    , "Fist Attack", script.slots[i1].weaponSlotInInspector[index].fistAttack);
                            }
                            else
                            {
                                script.slots[i1].weaponSlotInInspector[index].weapon = (GameObject) EditorGUI.ObjectField(
                                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    script.slots[i1].weaponSlotInInspector[index].weapon, typeof(GameObject), false);
                            }

                        }
                        else if (script.slots[i1].weaponSlotInInspector[index].weapon)
                        {
                            script.slots[i1].weaponSlotInInspector[index].weapon = (GameObject) EditorGUI.ObjectField(
                                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                script.slots[i1].weaponSlotInInspector[index].weapon, typeof(GameObject), false);
                        }
                        else if (script.slots[i1].weaponSlotInInspector[index].fistAttack)
                        {
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - rect.width / 2, EditorGUIUtility.singleLineHeight), "Set all variables below ↓");

                            script.slots[i1].weaponSlotInInspector[index].fistAttack = EditorGUI.ToggleLeft(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight)
                                , "Fist Attack", script.slots[i1].weaponSlotInInspector[index].fistAttack);
                        }
                    }
                };
            }

            handsAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("fistAttackHandsAnimations"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Hands Animations"); },

                onAddCallback = items => { script.fistAttackHandsAnimations.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.fistAttackHandsAnimations.Count == 1)
                        return;

                    script.fistAttackHandsAnimations.Remove(script.fistAttackHandsAnimations[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.fistAttackHandsAnimations[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.fistAttackHandsAnimations[index], typeof(AnimationClip), false);
                }
            };

            fullBodyAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("fistAttackFullBodyAnimations"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Full-body animations"); },

                onAddCallback = items => { script.fistAttackFullBodyAnimations.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.fistAttackFullBodyAnimations.Count == 1)
                        return;

                    script.fistAttackFullBodyAnimations.Remove(script.fistAttackFullBodyAnimations[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.fistAttackFullBodyAnimations[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.fistAttackFullBodyAnimations[index], typeof(AnimationClip), false);
                }
            };

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        public void Update()
        {
            for (var i = 0; i < 8; i++)
            {
//                var WeaponCount = script.slots[i].weaponsCount;

                for (var j = 0; j < script.slots[i].weaponSlotInInspector.Count; j++)
                {
                    if (script.slots[i].weaponSlotInInspector[j] != null)
                    {
                        if (script.slots[i].weaponSlotInInspector[j].weapon)
                        {
                            if (!script.slots[i].weaponSlotInInspector[j].weapon.GetComponent<WeaponController>())
                            {
                                weaponPrefabWarning = true;
                                script.slots[i].weaponSlotInInspector[j].weapon = null;
                            }
                            else
                            {
                                weaponPrefabWarning = false;
                            }
                        }
                    }
                }
            }

            var fistAttack = false;

            for (var i = 0; i < 8; i++)
            {
                foreach (var slot in script.slots[i].weaponSlotInInspector)
                {
                    if (slot != null && slot.fistAttack)
                    {
                        fistAttack = true;
                        break;
                    }
                }
            }

            script.HasFistAttack = fistAttack;

            if (Application.isPlaying)
            {
                if (script && !animator)
                    animator = script.GetComponent<Animator>();
            }
            else
            {
                if (!script.trailMaterial)
                    script.trailMaterial = Resources.Load("Trail Mat", typeof(Material)) as Material;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/Trail Mat.mat", typeof(Material)) as Material;
                
                if (!script.bloodProjector)
                    script.bloodProjector = Resources.Load("Blood Projector", typeof(Projector)) as Projector;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/Blood Projector.prefab", typeof(Projector)) as Projector;
            }
        }

        public override void OnInspectorGUI()
        {
        
            Helper.InitStyles(ref grayBackground, new Color32(160,160, 160, 200));

            serializedObject.Update();

            EditorGUILayout.Space();

            script.inventoryTabUp = GUILayout.Toolbar(script.inventoryTabUp,
                new[] {"Slot 1", "Slot 2", "Slot 3", "Slot 4"});

            switch (script.inventoryTabUp)
            {
                case 0:
                    script.inventoryTabMiddle = 4;
                    script.currentInventorySlot = 0;
                    break;
                case 1:
                    script.inventoryTabMiddle = 4;
                    script.currentInventorySlot = 1;
                    break;
                case 2:
                    script.inventoryTabMiddle = 4;
                    script.currentInventorySlot = 2;
                    break;
                case 3:
                    script.inventoryTabMiddle = 4;
                    script.currentInventorySlot = 3;
                    break;
            }

            script.inventoryTabMiddle = GUILayout.Toolbar(script.inventoryTabMiddle,
                new[] {"Slot 5", "Slot 6", "Slot 7", "Slot 8"});

            switch (script.inventoryTabMiddle)
            {
                case 0:
                    script.inventoryTabUp = 4;
                    script.currentInventorySlot = 4;
                    break;
                case 1:
                    script.inventoryTabUp = 4;
                    script.currentInventorySlot = 5;
                    break;
                case 2:
                    script.inventoryTabUp = 4;
                    script.currentInventorySlot = 6;
                    break;
                case 3:
                    script.inventoryTabUp = 4;
                    script.currentInventorySlot = 7;
                    break;
            }

            for (var i = 0; i < 8; i++)
            {
                if (script.currentInventorySlot == i)
                {
                    EditorGUILayout.Space();
//                    EditorGUILayout.BeginVertical(grayBackground);
                    // if (!Application.isPlaying)
                    // {
                        weaponsList[script.currentInventorySlot].DoLayoutList();

                        if (script.slots[i].weaponSlotInInspector.Any(item => item != null && item.fistAttack))
                        {
                            EditorGUILayout.Space();

                            EditorGUILayout.LabelField("Fist Attack Parameters", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical(grayBackground);

                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("FistIcon"), new GUIContent("Fist Image"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("RateOfAttack"), new GUIContent("Rate of Attack"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("FistDamage"), new GUIContent("Damage"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("fistAttackAudio"), new GUIContent("Sound°"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            
                            
                            EditorGUILayout.LabelField("Damage Colliders", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("helpbox");
                            // EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftHandCollider"), new GUIContent("Left"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("RightHandCollider"), new GUIContent("Right"));

                            if (!script.RightHandCollider && !script.LeftHandCollider && script.gameObject.activeInHierarchy)
                            {
                                if (GUILayout.Button("Create Colliders"))
                                {
                                    script.RightHandCollider = Helper.NewCollider("Right Collider", "Melee Collider", script.gameObject.GetComponent<Controller>().BodyObjects.RightHand);
                                    script.LeftHandCollider = Helper.NewCollider("Left Collider", "Melee Collider", script.gameObject.GetComponent<Controller>().BodyObjects.LeftHand);
                                }
                            }
                            // EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();

                            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsIdle"), new GUIContent("Idle"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsWalk"), new GUIContent("Walk"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsRun"), new GUIContent("Run"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            handsAnimations.DoLayoutList();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            fullBodyAnimations.DoLayoutList();
                            EditorGUILayout.EndVertical();
                        }
                    // }
                    // else if (Application.isPlaying && (script.Controller && !script.Controller.AdjustmentScene || !script.gameObject.activeInHierarchy))
                    // {

                        // weaponsList[script.currentInventorySlot].DoLayoutList();
                        //
                        // if (script.slots[i].weaponSlotInInspector.Any(item => item.fistAttack))
                        // {
                        //     EditorGUILayout.Space();
                        //     EditorGUILayout.BeginVertical("HelpBox");
                        //
                        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("FistIcon"), new GUIContent("Fist Image"));
                        //     EditorGUILayout.Space();
                        //
                        //     EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsIdle"), new GUIContent("Idle"));
                        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsWalk"), new GUIContent("Walk"));
                        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("HandsRun"), new GUIContent("Run"));
                        //
                        //     EditorGUILayout.EndVertical();
                        // }
                    // }
                    // else if (Application.isPlaying && script.Controller && script.Controller.AdjustmentScene)
                    // {
                        // EditorGUILayout.LabelField("You are in the [Adjustment scene]. " + "\n" + "To choose a weapon use [Adjustment] script here:");
                        //
                        // EditorGUILayout.Space();
                        // EditorGUILayout.ObjectField("Adjustment object", FindObjectOfType<Adjustment>(), typeof(Adjustment), true);
                    // }

                    if (weaponPrefabWarning)
                        EditorGUILayout.HelpBox("Your weapon should have the [WeaponController] script", MessageType.Warning);


//                    EditorGUILayout.EndVertical();
                }
            }


            switch (script.currentInventorySlot)
            {
                case 9:
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.HelpBox("You can change the texture, text, color and other graphic parameters of this slot.", MessageType.Info);

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoButton"),
                        new GUIContent("Slot UI"), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndVertical();
                    break;
                case 10:
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.HelpBox(
                        "You can change the texture, text, color and other graphic parameters of this slot.",
                        MessageType.Info);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("HealthButton"), new GUIContent("Slot UI"), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndVertical();
                    break;
            }


            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            // DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}


