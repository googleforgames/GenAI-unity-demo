using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(WeaponController))]
    public class WeaponControllerEditor : Editor
    {
        private WeaponController script;

        private InventoryManager manager;

        private ReorderableList tagsList;
        private ReorderableList fpAnimations;
        private ReorderableList tdAnimations;
        private ReorderableList fullBodyAnimations;
        private ReorderableList attackEffects;
        private ReorderableList fullBodyCrouchAnimations;
        
        private GUIStyle grayBackground;
        private GUIStyle style;
        
        private bool isSceneObject;

        private void Awake()
        {
            script = (WeaponController) target;
        }

        private void OnEnable()
        {
            
            var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            isSceneObject = allObjectsOnScene.Contains(script.gameObject);

            tagsList = new ReorderableList(serializedObject, serializedObject.FindProperty("IkSlots"), false, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 4, EditorGUIUtility.singleLineHeight), "Tag");

                    EditorGUI.LabelField(new Rect(rect.x + rect.width / 4 + 15, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), "FP View");

                    EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 + 10, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), "TP View");

                    EditorGUI.LabelField(new Rect(rect.x + 3 * rect.width / 4 + 7, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), "TD View");
                },

                onAddCallback = items => { script.IkSlots.Add(new WeaponsHelper.IKSlot()); },

                onRemoveCallback = items =>
                {
                    if (script.IkSlots.Count == 1)
                        return;

                    script.IkSlots.Remove(script.IkSlots[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.IkSlots[index].currentTag = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width / 4, EditorGUIUtility.singleLineHeight),
                        script.IkSlots[index].currentTag, script.projectSettings.CharacterTags.ToArray());

                    script.IkSlots[index].fpsSettingsSlot = EditorGUI.Popup(new Rect(rect.x + rect.width / 4 + 7, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), script.IkSlots[index].fpsSettingsSlot, script.enumNames.ToArray());

                    script.IkSlots[index].tpsSettingsSlot = EditorGUI.Popup(new Rect(rect.x + rect.width / 2 + 7, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), script.IkSlots[index].tpsSettingsSlot, script.enumNames.ToArray());

                    script.IkSlots[index].tdsSettingsSlot = EditorGUI.Popup(new Rect(rect.x + 3 * rect.width / 4 + 7, rect.y, rect.width / 4 - 7,
                        EditorGUIUtility.singleLineHeight), script.IkSlots[index].tdsSettingsSlot, script.enumNames.ToArray());
                }
            };

            fpAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("fpAttacks"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Attacks"); },

                onAddCallback = items => { script.Attacks[script.currentAttack].fpAttacks.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.Attacks[script.currentAttack].fpAttacks.Count == 1)
                        return;

                    script.Attacks[script.currentAttack].fpAttacks.Remove(script.Attacks[script.currentAttack].fpAttacks[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.Attacks[script.currentAttack].fpAttacks[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.Attacks[script.currentAttack].fpAttacks[index], typeof(AnimationClip), false);
                }
            };
            
            tdAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("tdAttacks"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Attacks"); },

                onAddCallback = items => { script.Attacks[script.currentAttack].tdAttacks.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.Attacks[script.currentAttack].tdAttacks.Count == 1)
                        return;

                    script.Attacks[script.currentAttack].tdAttacks.Remove(script.Attacks[script.currentAttack].tdAttacks[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.Attacks[script.currentAttack].tdAttacks[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.Attacks[script.currentAttack].tdAttacks[index], typeof(AnimationClip), false);
                }
            };

            fullBodyAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("tpAttacks"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Attacks"); },

                onAddCallback = items => { script.Attacks[script.currentAttack].tpAttacks.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.Attacks[script.currentAttack].tpAttacks.Count == 1)
                        return;

                    script.Attacks[script.currentAttack].tpAttacks.Remove(script.Attacks[script.currentAttack].tpAttacks[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.Attacks[script.currentAttack].tpAttacks[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.Attacks[script.currentAttack].tpAttacks[index], typeof(AnimationClip), false);
                }
            };

            fullBodyCrouchAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("tpCrouchAttacks"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Attacks (Crouch State)"); },

                onAddCallback = items => { script.Attacks[script.currentAttack].tpCrouchAttacks.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.Attacks[script.currentAttack].tpCrouchAttacks.Count == 1)
                        return;

                    script.Attacks[script.currentAttack].tpCrouchAttacks.Remove(script.Attacks[script.currentAttack].tpCrouchAttacks[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.Attacks[script.currentAttack].tpCrouchAttacks[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.Attacks[script.currentAttack].tpCrouchAttacks[index], typeof(AnimationClip), false);
                }
            };

            attackEffects = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("attackEffects"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Effects"); },

                onAddCallback = items => { script.Attacks[script.currentAttack].attackEffects.Add(null); },

                onRemoveCallback = items => { script.Attacks[script.currentAttack].attackEffects.Remove(script.Attacks[script.currentAttack].attackEffects[items.index]); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.Attacks[script.currentAttack].attackEffects[index] = (ParticleSystem) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.Attacks[script.currentAttack].attackEffects[index], typeof(ParticleSystem), false);
                }
            };

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (!script.projectSettings)
                {
                    script.projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }

                if (string.IsNullOrEmpty(script.weaponID))
                {
                    script.weaponID = Helper.GenerateRandomString(20);
                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                
                // script.Attacks[0].fpAttacks = new List<AnimationClip>(){null, null};
                // script.Attacks[0].tpAttacks = new List<AnimationClip>(){null, null};
                // script.Attacks[0].tdAttacks = new List<AnimationClip>(){null, null};

                if (ActiveEditorTracker.sharedTracker.isLocked)
                    ActiveEditorTracker.sharedTracker.isLocked = false;

                if (script && !script.inspectorCanvas && !isSceneObject)//!script.gameObject.activeInHierarchy && script.gameObject.activeSelf)
                {
                    var tempWeapon = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

                    tempWeapon.GetComponent<WeaponController>().inspectorCanvas = Helper.NewCanvas("Canvas", new Vector2(1920, 1080), tempWeapon.transform);

                    var parts = CharacterHelper.CreateCrosshair(tempWeapon.GetComponent<WeaponController>().inspectorCanvas.transform);

                    tempWeapon.GetComponent<WeaponController>().upPart = parts[1].GetComponent<Image>();
                    tempWeapon.GetComponent<WeaponController>().downPart = parts[2].GetComponent<Image>();
                    tempWeapon.GetComponent<WeaponController>().rightPart = parts[3].GetComponent<Image>();
                    tempWeapon.GetComponent<WeaponController>().leftPart = parts[4].GetComponent<Image>();
                    tempWeapon.GetComponent<WeaponController>().middlePart = parts[5].GetComponent<Image>();

#if !UNITY_2018_3_OR_NEWER
                    PrefabUtility.ReplacePrefab(tempWeapon, PrefabUtility.GetPrefabParent(tempWeapon), ReplacePrefabOptions.ConnectToPrefab);
#else
                    PrefabUtility.SaveAsPrefabAssetAndConnect(tempWeapon, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempWeapon), InteractionMode.AutomatedAction);
#endif

                    DestroyImmediate(tempWeapon);
                }
                else
                {
                    if (script.inspectorTabTop == 0 && script.Attacks[script.currentAttack].inspectorTab == 2 && script.inspectorCanvas)
                    {
                        script.inspectorCanvas.gameObject.SetActive(true);

                        script.upPart.GetComponent<RectTransform>().sizeDelta = new Vector2(script.Attacks[script.currentAttack].CrosshairSize, script.Attacks[script.currentAttack].CrosshairSize);
                        script.downPart.GetComponent<RectTransform>().sizeDelta = new Vector2(script.Attacks[script.currentAttack].CrosshairSize, script.Attacks[script.currentAttack].CrosshairSize);
                        script.leftPart.GetComponent<RectTransform>().sizeDelta = new Vector2(script.Attacks[script.currentAttack].CrosshairSize, script.Attacks[script.currentAttack].CrosshairSize);
                        script.rightPart.GetComponent<RectTransform>().sizeDelta = new Vector2(script.Attacks[script.currentAttack].CrosshairSize, script.Attacks[script.currentAttack].CrosshairSize);
                        script.middlePart.GetComponent<RectTransform>().sizeDelta = new Vector2(script.Attacks[script.currentAttack].CrosshairSize, script.Attacks[script.currentAttack].CrosshairSize);

                        script.upPart.GetComponent<RectTransform>().anchoredPosition = script.Attacks[script.currentAttack].crosshairPartsPositions[1];
                        script.downPart.GetComponent<RectTransform>().anchoredPosition = script.Attacks[script.currentAttack].crosshairPartsPositions[2];
                        script.rightPart.GetComponent<RectTransform>().anchoredPosition = script.Attacks[script.currentAttack].crosshairPartsPositions[3];
                        script.leftPart.GetComponent<RectTransform>().anchoredPosition = script.Attacks[script.currentAttack].crosshairPartsPositions[4];

                        script.upPart.sprite = script.Attacks[script.currentAttack].UpPart ? script.Attacks[script.currentAttack].UpPart : null;

                        script.downPart.sprite = script.Attacks[script.currentAttack].DownPart ? script.Attacks[script.currentAttack].DownPart : null;

                        script.leftPart.sprite = script.Attacks[script.currentAttack].LeftPart ? script.Attacks[script.currentAttack].LeftPart : null;

                        script.rightPart.sprite = script.Attacks[script.currentAttack].RightPart ? script.Attacks[script.currentAttack].RightPart : null;

                        script.middlePart.sprite = script.Attacks[script.currentAttack].MiddlePart ? script.Attacks[script.currentAttack].MiddlePart : null;

                        switch (script.Attacks[script.currentAttack].sightType)
                        {
                            case WeaponsHelper.CrosshairType.OnePart:
                                script.middlePart.gameObject.SetActive(true);
                                script.rightPart.gameObject.SetActive(false);
                                script.leftPart.gameObject.SetActive(false);
                                script.upPart.gameObject.SetActive(false);
                                script.downPart.gameObject.SetActive(false);

                                break;
                            case WeaponsHelper.CrosshairType.TwoParts:

                                script.middlePart.gameObject.SetActive(script.middlePart.sprite);

                                script.rightPart.gameObject.SetActive(true);
                                script.leftPart.gameObject.SetActive(true);

                                script.upPart.gameObject.SetActive(false);
                                script.downPart.gameObject.SetActive(false);

                                break;
                            case WeaponsHelper.CrosshairType.FourParts:

                                script.middlePart.gameObject.SetActive(script.middlePart.sprite);

                                script.rightPart.gameObject.SetActive(true);
                                script.leftPart.gameObject.SetActive(true);

                                script.upPart.gameObject.SetActive(true);
                                script.downPart.gameObject.SetActive(true);

                                break;
                        }
                    }
                    else if (script.inspectorCanvas && (script.inspectorTabTop != 0 || script.Attacks[script.currentAttack].inspectorTab != 2))
                    {
                        script.inspectorCanvas.gameObject.SetActive(false);
                    }
                }


                if (!script) return;

//                if (script.Attacks[script.currentAttack].AttackEffects.Length > 0)
//                {
//                    foreach (var effect in script.Attacks[script.currentAttack].AttackEffects)
//                    {
//                        if (effect && effect.emission.enabled)
//                        {
//                            var effectEmission = effect.emission;
//                            effectEmission.enabled = false;
//                        }
//                    }
//                }

                // if (script.gameObject.GetComponent<Rigidbody>())
                // {
                    // script.gameObject.GetComponent<Rigidbody>().hideFlags = HideFlags.None;
                    // script.gameObject.AddComponent<Rigidbody>();
                // }

                if (script.Attacks.All(attack => attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade))
                {
                    if (!script.gameObject.GetComponent<BoxCollider>())
                        script.gameObject.AddComponent<BoxCollider>();
                    else script.gameObject.GetComponent<BoxCollider>().enabled = true;

                    if (script.gameObject.GetComponent<CapsuleCollider>())
                        script.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                }
                else
                {
                    if (!script.gameObject.GetComponent<CapsuleCollider>())
                        script.gameObject.AddComponent<CapsuleCollider>();
                    else script.gameObject.GetComponent<CapsuleCollider>().enabled = true;

                    if (script.gameObject.GetComponent<BoxCollider>())
                        script.gameObject.GetComponent<BoxCollider>().enabled = false;
                }


                if (script.gameObject.GetComponent<PickupItem>() && script.enabled)
                {
                    script.enabled = false;
                }
                else if(!script.gameObject.GetComponent<PickupItem>() && !script.enabled)
                {
                    script.enabled = true;
                }
                // script.PickUpWeapon = isSceneObject && SceneManager.GetActiveScene().name != script.gameObject.name;

                // if (!script.PickUpWeapon)
                // {
//                     if (!script.PickUpWeapon && script.gameObject.GetComponent<PickupItem>())
//                     {
//                         if (!isSceneObject)
//                         {
//                             var tempWeapon = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
//                             DestroyImmediate(tempWeapon.GetComponent<PickupItem>());
// #if !UNITY_2018_3_OR_NEWER
//                             PrefabUtility.ReplacePrefab(tempWeapon, PrefabUtility.GetPrefabParent(tempWeapon), ReplacePrefabOptions.ConnectToPrefab);
// #else
//                             PrefabUtility.SaveAsPrefabAssetAndConnect(tempWeapon, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempWeapon), InteractionMode.AutomatedAction);
// #endif
//                             DestroyImmediate(tempWeapon);
//                         }
//                         else
//                         {
//                             DestroyImmediate(script.gameObject.GetComponent<PickupItem>());
//                         }
//                     }
                    
                    // script.enabled = true;
                // }
                // else if (script.PickUpWeapon && !script.gameObject.GetComponent<PickupItem>())
                // {
                //     if (script.Attacks.All(attack => attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade))
                //     {
                //         if (!script.gameObject.GetComponent<PickupItem>())
                //             script.gameObject.AddComponent<PickupItem>();
                //
                //         script.gameObject.GetComponent<PickupItem>().type = PickupItem.TypeOfPickUp.Weapon;
                //         if (script.Attacks.Any(attack => attack.AttackCollider))
                //         {
                //             var _attacks = script.Attacks.FindAll(attack => attack.AttackCollider);
                //             foreach (var _attack in _attacks)
                //             {
                //                 if (_attack.AttackCollider.enabled)
                //                     _attack.AttackCollider.enabled = false;
                //             }
                //         }
                //     }
                //
                //     // script.enabled = false;
                // }
            }
        }

        public override void OnInspectorGUI()
        {
            Helper.InitStyles(ref grayBackground, new Color32(160,160, 160, 200));
            
            serializedObject.Update();
            EditorGUILayout.Space();
            
            style = new GUIStyle{richText = true, fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};


            if (script.gameObject.GetComponent<PickupItem>())
            {
                var backgroundColor = GUI.backgroundColor;
                
                GUI.backgroundColor = new Color(1, 0.5f, 0, 0.5f);

                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Pick-up Item", style);
                EditorGUILayout.EndVertical();
                
                GUI.backgroundColor = backgroundColor;

                EditorGUILayout.Space();
            }

            // EditorGUILayout.HelpBox(script.PickUpWeapon
            //     ? "This weapon is a pick-up item. Set parameters in the [PickUpItem] script."
            //     : "To use this weapon as a pick-up item, just place it in a scene.", MessageType.Info);

            EditorGUILayout.Space();

            script.inspectorTabTop = GUILayout.Toolbar(script.inspectorTabTop, new[] {"Attacks", "Weapon Settings", "Aim Settings"});


            switch (script.inspectorTabTop)
            {
                case 0:
                    script.inspectorTabBottom = 3;
                    script.currentTab = "Attacks";
                    break;
                case 1:
                    script.inspectorTabBottom = 3;
                    script.currentTab = "Weapon Settings";
                    break;
                case 2:
                    script.inspectorTabBottom = 3;
                    script.currentTab = "Aim Settings";
                    break;
            }

            script.inspectorTabBottom = GUILayout.Toolbar(script.inspectorTabBottom,
                new[] {"Animations", "Sounds"});

            switch (script.inspectorTabBottom)
            {
                case 0:
                    script.inspectorTabTop = 3;
                    script.currentTab = "Animations";
                    break;
                case 1:
                    script.inspectorTabTop = 3;
                    script.currentTab = "Sounds";
                    break;
            }


            switch (script.currentTab)
            {
                case "Attacks":

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical(grayBackground);
                    if (script.Attacks.Count > 0)
                    {
                        script.currentAttack = EditorGUILayout.Popup("Attacks", script.currentAttack, script.attacksNames.ToArray());
//                        EditorGUILayout.BeginHorizontal();
                        
//                        if (!script.rename)
//                        {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginDisabledGroup(script.rename);
                            if (GUILayout.Button("Rename"))
                            {
                                script.rename = true;
                                script.curName = "";
                            }
//                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(script.Attacks.Count <= 1 || script.delete);
//                        if (!script.delete)
//                        {
                            if (GUILayout.Button("Delete"))
                            {
                                script.delete = true;
                            }
//                        }
                        EditorGUI.EndDisabledGroup();
                    }


                    EditorGUI.BeginDisabledGroup(script.Attacks.Any(attack => attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade));

                    if (GUILayout.Button("Add a new one"))
                    {
                        script.Attacks.Add(new WeaponsHelper.Attack());
                        script.Attacks[script.Attacks.Count - 1].BulletsSettings.Add(new WeaponsHelper.BulletsSettings());
                        script.Attacks[script.Attacks.Count - 1].BulletsSettings.Add(new WeaponsHelper.BulletsSettings());

                        if (!script.attacksNames.Contains("Attack " + script.Attacks.Count))
                            script.attacksNames.Add("Attack " + script.Attacks.Count);
                        else script.attacksNames.Add("Attack " + Random.Range(10, 100));

                        script.currentAttack = script.Attacks.Count - 1;

                        break;
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                  

                    if (script.rename)
                    {
                        EditorGUILayout.BeginVertical("helpbox");
                        script.curName = EditorGUILayout.TextField("New name", script.curName);

                        EditorGUILayout.BeginHorizontal();


                        if (GUILayout.Button("Cancel"))
                        {
                            script.rename = false;
                            script.curName = "";
                            script.renameError = false;
                        }

                        if (GUILayout.Button("Save"))
                        {
                            if (!script.attacksNames.Contains(script.curName))
                            {
                                script.rename = false;
                                script.attacksNames[script.currentAttack] = script.curName;
                                script.curName = "";
                                script.renameError = false;
                            }
                            else
                            {
                                script.renameError = true;
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        if (script.renameError)
                            EditorGUILayout.HelpBox("This name already exist.", MessageType.Warning);

                        EditorGUILayout.EndVertical();
                    }
                    else if (script.delete)
                    {
                        EditorGUILayout.BeginVertical("helpbox");
                        EditorGUILayout.LabelField("Are you sure?");
                        EditorGUILayout.BeginHorizontal();


                        if (GUILayout.Button("No"))
                        {
                            script.delete = false;
                        }

                        if (GUILayout.Button("Yes"))
                        {
                            script.attacksNames.Remove(script.attacksNames[script.currentAttack]);
                            script.Attacks.Remove(script.Attacks[script.currentAttack]);
                            script.currentAttack = script.Attacks.Count - 1;
                            script.delete = false;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();    
                    }
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (script.Attacks.Count > 0)
                    {
                        var _attack = script.Attacks[script.currentAttack];
                        var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack);

                        GUILayout.BeginVertical("Attack: " + script.attacksNames[script.currentAttack], "window");
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(script.currentAttack).FindPropertyRelative("AttackType"), new GUIContent("Type"));

                        EditorGUILayout.Space();

                        script.Attacks[script.currentAttack].inspectorTab = GUILayout.Toolbar(script.Attacks[script.currentAttack].inspectorTab, new[] {"Parameters", "Effects", "Crosshair"});

                        EditorGUILayout.Space();
                        
//                        EditorGUILayout.BeginVertical(grayBackground);

                        switch (script.Attacks[script.currentAttack].inspectorTab)
                        {
                            case 1:
                                switch (_attack.AttackType)
                                {
                                    case WeaponsHelper.TypeOfAttack.Rockets:

                                        EditorGUILayout.Space();
//                                        if (script.Attacks[script.currentAttack].magazine)
//                                        {
                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Explosion"), new GUIContent("Explosion"));
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                        attackEffects.DoLayoutList();
//                                        }

                                        break;
                                    case WeaponsHelper.TypeOfAttack.Flame:
                                        attackEffects.DoLayoutList();
                                        break;
                                    case WeaponsHelper.TypeOfAttack.Melee:
                                    {
                                        EditorGUILayout.HelpBox("There aren't any effects for the Melee attacks.", MessageType.Info);
                                        break;
                                    }
                                    case WeaponsHelper.TypeOfAttack.Bullets:
                                    case WeaponsHelper.TypeOfAttack.Minigun:

                                        if (_attack.shootingMethod == WeaponsHelper.ShootingMethod.Raycast)
                                        {
                                            EditorGUILayout.BeginVertical("HelpBox");
                                            EditorGUILayout.HelpBox("The bullet trail prefab must have a [Trail Renderer] or [Line Renderer] component.", MessageType.Info);
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletTrail"), new GUIContent("Bullet Trail"));
                                            EditorGUILayout.EndVertical();
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("MuzzleFlash"), new GUIContent("Muzzle Flash (prefab)"));
                                        EditorGUILayout.EndVertical();

                                        EditorGUILayout.Space();

                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Shell"), new GUIContent("Shell (prefab)"));
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("ShellPoint"), new GUIContent("Shells spawn point"));
                                        CheckPoint(_attack, "shell");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("spawnShellsImmediately"), new GUIContent("Spawn Shells Immediately°"));
//                                        EditorGUILayout.HelpBox("If this checkbox is active, the shells appear immediately when a character shoots." + "\n" +
//                                                               "If you need a delay in the appearance of shells (for example, for a shotgun or rifle), add an event called [SpawnShell] to the shot animation.", MessageType.Info);
                                        EditorGUILayout.EndVertical();

                                        break;
                                    case WeaponsHelper.TypeOfAttack.Grenade:

                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Explosion"), new GUIContent("Explosion"));
                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical("HelpBox");
//                                        EditorGUILayout.HelpBox("If active, the explosion will push objects with the [Rigidbody] component.", MessageType.Info);
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("applyForce"), new GUIContent("Apply Force°"));

                                        EditorGUILayout.EndVertical();

                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical("HelpBox");
//                                        EditorGUILayout.HelpBox("Use it, if you need a Flash Grenade effect.", MessageType.Info);
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flashExplosion"), new GUIContent("Flash Effect°"));

                                        EditorGUILayout.EndVertical();

                                        break;
                                    case WeaponsHelper.TypeOfAttack.GrenadeLauncher:


                                        EditorGUILayout.BeginVertical("HelpBox");
                                        script.Attacks[script.currentAttack].showTrajectory = EditorGUILayout.ToggleLeft(new GUIContent("Show Grenade Trajectory°", "Whether the trajectory will be displayed when aiming"), script.Attacks[script.currentAttack].showTrajectory);
                                        script.Attacks[script.currentAttack].ExplodeWhenTouchGround = EditorGUILayout.ToggleLeft("Explode when touch an object", script.Attacks[script.currentAttack].ExplodeWhenTouchGround);

                                        if (!script.Attacks[script.currentAttack].ExplodeWhenTouchGround)
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("GrenadeExplosionTime"), new GUIContent("Explosion Time"));
                                        EditorGUILayout.EndVertical();

                                        EditorGUILayout.Space();

                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Explosion"), new GUIContent("Explosion"));
                                        EditorGUILayout.EndVertical();

                                        break;

                                }

                                break;

                            case 0:

                                if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && _attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackSpawnPoint"), new GUIContent("Attack Point"));
                                    CheckPoint(_attack, "attack");
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Flame)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackCollider"), new GUIContent("Fire collider"));
                                    CheckCollider(_attack, "fire");
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Melee)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackCollider"), new GUIContent("Attack collider"));
                                    CheckCollider(_attack, "knife");
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Rockets)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("magazine"), new GUIContent("Rocket"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("magazine"), new GUIContent("Grenade"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
//                                    EditorGUILayout.HelpBox("Minigun barrel and rotation axis.", MessageType.Info);
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Barrel"), new GUIContent("Barrel"));
                                    script.Attacks[script.currentAttack].barrelRotationAxes = (Helper.RotationAxes) EditorGUILayout.EnumPopup(new GUIContent("Rotation Axis°", "Minigun barrel rotation axis"), script.Attacks[script.currentAttack].barrelRotationAxes);
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                EditorGUILayout.BeginVertical("HelpBox");
//                                EditorGUILayout.HelpBox("◆ TP and FP views:" + "\n" +
//                                                        "When you aim the crosshair at the enemy and the character is closer than the set distance the weapon will attack." + "\n\n" +
//                                                        "◆ TD view (mobile):" + "\n" +
//                                                        "While you're using the camera joystick, the character is shooting."
//                                                        , MessageType.Info);
                                script.Attacks[script.currentAttack].autoAttack = EditorGUILayout.ToggleLeft(new GUIContent("Auto Attack°", //"• TP and FP views:" + "\n" +
                                                                                                                                            "The weapon attacks if you aim the crosshair at an enemy and the character is closer than the [Distance] value." ),//+ "\n\n" +
                                                                                                                                            //"• TD view (mobile):" + "\n" +
                                                                                                                                            //"While you're using the camera joystick, the character is shooting."),
                                                                                                                                            script.Attacks[script.currentAttack].autoAttack);


                                if (script.Attacks[script.currentAttack].autoAttack)
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("attackDistance"), new GUIContent("Distance"));

                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                                {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    script.Attacks[script.currentAttack].ExplodeWhenTouchGround = EditorGUILayout.ToggleLeft("Explode when touch an object", script.Attacks[script.currentAttack].ExplodeWhenTouchGround);
                                    script.Attacks[script.currentAttack].showTrajectory = EditorGUILayout.ToggleLeft(new GUIContent("Show Grenade Trajectory°", "Whether the trajectory will be displayed when aiming"), script.Attacks[script.currentAttack].showTrajectory);
                                    script.Attacks[script.currentAttack].applyGravity = EditorGUILayout.ToggleLeft(new GUIContent("Apply Gravity°", "Use this parameter to apply gravity to the flying projectile"), script.Attacks[script.currentAttack].applyGravity);
                                    script.Attacks[script.currentAttack].useTakeAnimation = EditorGUILayout.ToggleLeft(new GUIContent("Use 'Take' animation°", "After a character has thrown this projectile, he must take a new one - this parameter determines whether animations will be used for this." + "\n\n" +
                                                                                                                                                               "[On] - good for grenades" + "\n" +
                                                                                                                                                               "[Off] - may be useful for magic projectiles"), script.Attacks[script.currentAttack].useTakeAnimation);
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }
                                
                                EditorGUILayout.BeginVertical("HelpBox");

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Bullets)
                                {
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("shootingMethod"), new GUIContent("Shooting Method"));

                                    if (_attack.shootingMethod == WeaponsHelper.ShootingMethod.InstantiateBullet)
                                    {
                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletPrefab"), new GUIContent("Bullet Prefab"));
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
                                        EditorGUILayout.EndVertical();
                                    }
                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.Space();
                                    
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    script.bulletTypeInspectorTab = EditorGUILayout.Popup(new GUIContent("Shooting type°", "You can use single, automatic or both shooting types." + "\n" +
                                                                                                                           "In the game you can switch between them by pressing the [" + script.projectSettings.keyboardButtonsInProjectSettings[19] + "] button."), script.bulletTypeInspectorTab, new[] {new GUIContent("Single"), new GUIContent("Auto")});
                                    
                                    switch (script.bulletTypeInspectorTab)
                                    {
                                        case 0:

                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("Active"), new GUIContent("Active"));
                                            
                                            if (script.Attacks[script.currentAttack].BulletsSettings[0].Active)
                                            {
                                                EditorGUILayout.BeginVertical("HelpBox");                                                
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("weapon_damage"), new GUIContent("Damage°", "Negative damage restores health."));
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("RateOfShoot"), new GUIContent("Rate of Shoot"));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("bulletsScatterX"), new GUIContent("Bullets Scatter X"));
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("bulletsScatterY"), new GUIContent("Bullets Scatter Y"));

                                                EditorGUILayout.Space();
                                                
                                                script.isShotgun = EditorGUILayout.ToggleLeft("Is Shotgun", script.isShotgun);
                                                
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(0).FindPropertyRelative("attackImage"), new GUIContent("Attack Type Icon°"));
                                                EditorGUILayout.EndVertical();
                                            }

                                            break;

                                        case 1:

                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("Active"), new GUIContent("Active"));
                                            
                                            if (script.Attacks[script.currentAttack].BulletsSettings[1].Active)
                                            {
                                                EditorGUILayout.BeginVertical("HelpBox");                                                
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("weapon_damage"), new GUIContent("Damage"));
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("RateOfShoot"), new GUIContent("Rate of Shoot"));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("bulletsScatterX"), new GUIContent("Bullets Scatter X"));
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("bulletsScatterY"), new GUIContent("Bullets Scatter Y"));
                                                EditorGUILayout.Space();
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("attackImage"), new GUIContent("Attack Type Icon°"));
                                                EditorGUILayout.EndVertical();
                                            }

                                            break;

                                    }
                                }
                                else if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                                {
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("shootingMethod"), new GUIContent("Shooting Method"));

                                    if (_attack.shootingMethod == WeaponsHelper.ShootingMethod.InstantiateBullet)
                                    {
                                        EditorGUILayout.BeginVertical("HelpBox");
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletPrefab"), new GUIContent("Bullet Prefab"));
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
                                        EditorGUILayout.EndVertical();
                                    }
                                    
                                    EditorGUILayout.EndVertical();
                                    
                                    EditorGUILayout.Space();
                                    
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("weapon_damage"), new GUIContent("Damage"));
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("RateOfShoot"), new GUIContent("Rate of Shoot"));
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("bulletsScatterX"), new GUIContent("Bullets Scatter X"));
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("BulletsSettings").GetArrayElementAtIndex(1).FindPropertyRelative("bulletsScatterY"), new GUIContent("Bullets Scatter Y"));
                                    
                                }
                                else if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade)
                                {
                                    // script.Attacks[script.currentAttack].StickToObject = EditorGUILayout.ToggleLeft("Stick to an object", script.Attacks[script.currentAttack].StickToObject);

                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
                                    
                                    if(!_attack.ExplodeWhenTouchGround)
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("GrenadeExplosionTime"), new GUIContent("Time before explosion"));
                                    
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("weapon_damage"), new GUIContent("Damage"));
                                }
                                else
                                {

                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("weapon_damage"),
                                        _attack.AttackType == WeaponsHelper.TypeOfAttack.Flame ? new GUIContent("Damage (per 1 sec)") : new GUIContent("Damage"));

                                    if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Flame)
                                    {
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RateOfAttack"), new GUIContent("Rate of Attack"));

                                        if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && _attack.AttackType != WeaponsHelper.TypeOfAttack.GrenadeLauncher)
                                        {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletsScatterX"), _attack.AttackType != WeaponsHelper.TypeOfAttack.Rockets ? new GUIContent("Bullets Scatter X") : new GUIContent("Rockets Scatter X"));
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletsScatterY"), _attack.AttackType != WeaponsHelper.TypeOfAttack.Rockets ? new GUIContent("Bullets Scatter Y") : new GUIContent("Rockets Scatter Y"));
                                        }

                                        if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Rockets || _attack.AttackType == WeaponsHelper.TypeOfAttack.GrenadeLauncher)
                                        {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
                                        }
                                    }
                                }
                                EditorGUILayout.EndVertical();

                                if (_attack.AttackType == WeaponsHelper.TypeOfAttack.Bullets || _attack.AttackType == WeaponsHelper.TypeOfAttack.Minigun)
                                {
                                    EditorGUILayout.Space();

                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("penetrationMultiplier"), new GUIContent("Penetration Multiplier°", "This parameter will be multiplied by the base penetration values that are set in each surface material."));
                                    EditorGUILayout.EndVertical();
                                }

                                EditorGUILayout.Space();

                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("attackNoiseRadiusMultiplier"), new GUIContent("Noise Multiplier°"));
                                EditorGUILayout.EndVertical();

                                if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("maxAmmo"), new GUIContent("Count of ammo in magazine"));


                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("inventoryAmmo"),
                                        _attack.AttackType == WeaponsHelper.TypeOfAttack.Grenade ? new GUIContent("Count in inventory") : new GUIContent("Count of ammo in inventory"));

                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();

                                    EditorGUILayout.BeginVertical("HelpBox");
//                                    EditorGUILayout.HelpBox("Write the same type in a PickUp script.", MessageType.Info);
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("ammoName"), new GUIContent("Ammo Name°"));
                                    EditorGUILayout.EndVertical();

                                }

                                if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && _attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade && _attack.AttackType != WeaponsHelper.TypeOfAttack.Rockets && _attack.AttackType != WeaponsHelper.TypeOfAttack.GrenadeLauncher)
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("HelpBox");
//                                    EditorGUILayout.HelpBox("Model for synchronization with animation." + "\n" +
//                                                            "Read more about that in the Documentation (section 'Events' -> 'Weapons' -> 'Magazines')"), MessageType.Info);
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("magazine"), new GUIContent("Magazine (optional)°"));
                                    EditorGUILayout.EndVertical();
                                }

                                if (_attack.AttackType != WeaponsHelper.TypeOfAttack.Bullets)
                                {
                                    EditorGUILayout.Space();
                                    
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("attackImage"), new GUIContent("Attack Type Icon°"));
                                    EditorGUILayout.EndVertical();
                                }
                                
                                // EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("mobileAttackButton"), new GUIContent("Attack Mobile Button°"));


                                break;

                            case 2:

                                EditorGUILayout.BeginVertical("HelpBox");
                                _attack.sightType = (WeaponsHelper.CrosshairType) EditorGUILayout.EnumPopup("Type", _attack.sightType);
                                EditorGUILayout.EndVertical();
                                
                                EditorGUILayout.Space();
                                
                                EditorGUILayout.BeginVertical("HelpBox");
                                switch (_attack.sightType)
                                {
                                    case WeaponsHelper.CrosshairType.OnePart:
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("MiddlePart"), new GUIContent("Middle Part"));
//                                        EditorGUILayout.Space();
//                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("CrosshairSize"), new GUIContent("Scale"));
                                        break;
                                    case WeaponsHelper.CrosshairType.TwoParts:

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("LeftPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(4), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RightPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(3), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("MiddlePart"), new GUIContent("Middle Part (optional)"));
//                                        EditorGUILayout.Space();
//                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("CrosshairSize"), new GUIContent("Scale"));

                                        break;
                                    case WeaponsHelper.CrosshairType.FourParts:

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("UpPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(1), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("DownPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(2), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("LeftPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(4), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RightPart"));
                                        if (script.showCrosshairPositions)
                                        {
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("crosshairPartsPositions").GetArrayElementAtIndex(3), new GUIContent("Position"));
                                            EditorGUILayout.Space();
                                        }

                                        EditorGUILayout.Space();
                                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("MiddlePart"), new GUIContent("Middle Part (optional)"));

                                        break;
                                }

                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();
                                
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("CrosshairSize"), new GUIContent("Scale"));
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();
                                
                                script.showCrosshairPositions = GUILayout.Toggle(script.showCrosshairPositions, "Adjust Positions", "Button");

                                break;

                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }


                    break;

                case "Weapon Settings":
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(grayBackground);
                    
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponWeight"), new GUIContent("Weapon Weight°"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponImage"), new GUIContent("Weapon Image°"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipImage"), new GUIContent("Blip Image°"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    if (script.Attacks[script.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Melee && script.Attacks[script.currentAttack].AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                    {
                        EditorGUILayout.BeginVertical("HelpBox");
                        script.autoReload = EditorGUILayout.ToggleLeft(new GUIContent("Auto Reload", "When the ammo runs out the character reloads the weapon."), script.autoReload);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.BeginVertical("HelpBox");
                    script.enableObjectDetectionMode = EditorGUILayout.ToggleLeft(new GUIContent("Object Detection°", "When a character comes close to an object, he removes his hands " +
                                                                                                                      "(a pose is adjusted using IK [Tools -> USK -> Adjust])."), script.enableObjectDetectionMode);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("HelpBox");

                    if (script.numberOfUsedHands == 2)
                    {
                        EditorGUILayout.HelpBox("In this mode, the left hand depends on the right. " + "\n\n" +
                                                "Use this if the character holds this weapon in two hands. In this case, the IK system will be more accurate.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("In this mode, the left and right hands are independent of each other." + "\n\n" +
                                                "Use this if the character holds this weapon in one hand (grenades, knives, etc.)", MessageType.Info);
                    }

                    var index = script.numberOfUsedHands - 1;
                    index = EditorGUILayout.Popup("Number of Used Hands", index, new[] {"One-Handed", "Two-Handed"});
                    script.numberOfUsedHands = index + 1;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.HelpBox("For different characters you can use different IK settings, " +
                                            "just set the necessary settings for each tag." + "\n\n" +
                                            "You can change the character's tag in any [Controller] script." + "\n\n" +
                                            "Also for each type of camera, you can set an own slot.", MessageType.Info);
                    tagsList.DoLayoutList();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("FP Head Bobbing", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingValues.useCommonParameters"), new GUIContent("Use Common Parameters°", "If active, the parameters that you set in Controller scripts will be used for head bobbing."));
                    // EditorGUILayout.PropertyField(serializedObject.FindProperty("resetBobbing"), new GUIContent("Reset Bobbing"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    
                    EditorGUI.BeginDisabledGroup(script.bobbingValues.useCommonParameters);
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingValues.bobbingRotationAxis"), new GUIContent("Axis"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingValues.bobbingAmplitude"), new GUIContent("Amplitude"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingValues.bobbingDuration"), new GUIContent("Duration"));
                    EditorGUILayout.EndVertical();
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.EndVertical();

                    break;

                case "Aim Settings":
                    EditorGUILayout.Space();
                   
                    EditorGUILayout.BeginVertical("HelpBox");
                    script.activeAimMode = EditorGUILayout.ToggleLeft("Active Aim Mode", script.activeAimMode);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    
                    script.aimInspectorTabIndex = GUILayout.Toolbar(script.aimInspectorTabIndex, new[] {"First Person", "Third Person", "Top Down"});
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical(grayBackground);
                    switch (script.aimInspectorTabIndex)
                    {
                        case 0:
                            EditorGUI.BeginDisabledGroup(!script.activeAimMode);

                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("The speed at which the character will aim.", MessageType.Info);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingSpeed"), new GUIContent("Aim Speed°"));
                            EditorGUILayout.EndVertical();


                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Scope", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("Use this to add a scope model to the weapon.", MessageType.Info);
//                            EditorGUILayout.Space();
                            script.useScope = EditorGUILayout.ToggleLeft(new GUIContent("Use°","Use this to add a scope model to the weapon."), script.useScope);

                            if (script.useScope)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ScopeScreen"), new GUIContent("Scope Screen"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("scopeDepth"), new GUIContent("Depth"));
                            }

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("If this option is active, the weapon will use the texture as a sight.", MessageType.Info);

                            script.useAimTexture = EditorGUILayout.ToggleLeft(new GUIContent("Use°", "If this option is active, the weapon will use the texture as a sight."), script.useAimTexture);

                            if (script.useAimTexture)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimCrosshairTexture"), new GUIContent("Texture"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("fillColor"), new GUIContent("Fill Color°", "A solid fill will be generated from the edges of the screen to the texture. Adjust its color to match the color of the texture."));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTextureDepth"), new GUIContent("Depth"));
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUI.EndDisabledGroup();

                            break;

                        case 1:
//                            EditorGUILayout.BeginVertical("HelpBox");
//                            script.activeAimTP = EditorGUILayout.ToggleLeft("Active Aim mode", script.activeAimTP, EditorStyles.boldLabel);
//                            EditorGUILayout.EndVertical();
//                            EditorGUILayout.Space();
//                            EditorGUILayout.Space();
                            EditorGUI.BeginDisabledGroup(!script.activeAimMode);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("The speed at which the character will aim.", MessageType.Info);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingSpeed"), new GUIContent("Aim Speed°"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("The camera will switch to first-person view when aiming.", MessageType.Info);

                            script.switchToFpCamera = EditorGUILayout.ToggleLeft(new GUIContent("Switch to FP View°", "The camera will switch to first-person view when aiming."), script.switchToFpCamera);
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("When you press the [Attack] button the character will first aim and then attack.", MessageType.Info);
                            script.aimForAttack = EditorGUILayout.ToggleLeft(new GUIContent("Aim before Attack°", "When you press the [Attack] button the character will first aim and then attack."), script.aimForAttack);
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Scope", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("Use this to add a scope model to the weapon.", MessageType.Info);

                            script.useScope = EditorGUILayout.ToggleLeft(new GUIContent("Use°","Use this to add a scope model to the weapon."), script.useScope);

                            if (script.useScope)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ScopeScreen"), new GUIContent("Scope Screen"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("scopeDepth"), new GUIContent("Depth"));
                            }

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Texture Aiming", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("If this option is active, the weapon will use the texture as a sight.", MessageType.Info);

                            script.useAimTexture = EditorGUILayout.ToggleLeft(new GUIContent("Use°", "If this option is active, the weapon will use the texture as a sight."), script.useAimTexture);

                            if (script.useAimTexture)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimCrosshairTexture"), new GUIContent("Texture"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("fillColor"), new GUIContent("Fill Color°", "A solid fill will be generated from the edges of the screen to the texture. Adjust its color to match the color of the texture."));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTextureDepth"), new GUIContent("Depth"));
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUI.EndDisabledGroup();

                            break;

                        case 2:
                            EditorGUI.BeginDisabledGroup(!script.activeAimMode);
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("The speed at which the character will aim.", MessageType.Info);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingSpeed"), new GUIContent("Aim Speed°"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
//                            EditorGUILayout.HelpBox("When you press the [Attack] button the character will first aim and then attack.", MessageType.Info);
                            script.aimForAttack = EditorGUILayout.ToggleLeft(new GUIContent("Aim before Attack°", "When you press the [Attack] button the character will first aim and then attack."), script.aimForAttack);
                            EditorGUILayout.EndVertical();
                            EditorGUI.EndDisabledGroup();
                            break;
                    }
                    EditorGUILayout.EndVertical();
                    if (script.activeAimMode)
                    {
                        EditorGUILayout.Space();
                    }
//                    }

                    break;

                case "Animations":
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical(grayBackground);

                    if (script.Attacks.Count > 0)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Common Animations°", "Animations that are used in all views."), EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical("HelpBox");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponAnimations.idle"), new GUIContent("Idle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponAnimations.take"), new GUIContent("Take From Inventory"));

                        if (script.Attacks.Count > 1)
                            EditorGUILayout.Space();

                        for (var i = 0; i < script.Attacks.Count; i++)
                        {
                            var attack = script.Attacks[i];
                            var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(i);

                            if (attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                            {
                                if (script.Attacks.Count > 1)
                                {
                                    EditorGUILayout.LabelField(new GUIContent("Attack: " + script.attacksNames[i] + "°", "For each attack you can set different reload animations (for a machine gun with an underbarrel grenade launcher, for example)."), EditorStyles.boldLabel);
                                    EditorGUILayout.BeginVertical("HelpBox");
                                }

                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("reloadAnimation"), new GUIContent("Reload"));

                                if (script.Attacks.Count > 1)
                                {
                                    EditorGUILayout.EndVertical();
                                }

                            }
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();


                        script.animationsInspectorTabIndex = GUILayout.Toolbar(script.animationsInspectorTabIndex, new[] {"First Person", "Third Person", "Top Down"});

                        EditorGUILayout.Space();
                        
                        switch (script.animationsInspectorTabIndex)
                        {
                            case 0:
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponAnimations.fpWalk"), new GUIContent("Walk"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponAnimations.fpRun"), new GUIContent("Run"));
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();
                                
                                for (var i = 0; i < script.Attacks.Count; i++)
                                {
                                    var attack = script.Attacks[i];
                                    var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(i);

                                    EditorGUILayout.LabelField("Attack: " + script.attacksNames[i], EditorStyles.boldLabel);
                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.BeginVertical("HelpBox");
                                    switch (attack.AttackType)
                                    {
                                        case WeaponsHelper.TypeOfAttack.Bullets:
                                            if (attack.BulletsSettings[1].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fpAttacks").GetArrayElementAtIndex(1), new GUIContent("Auto Shoot"));

                                            if (attack.BulletsSettings[0].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fpAttacks").GetArrayElementAtIndex(0), new GUIContent("Single Shoot"));
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Grenade:
                                            EditorGUILayout.HelpBox("Add the [ThrowGrenade] event on animations to set the exact time of the grenade throw, otherwise, the grenade will be launched at the end of the animation.", MessageType.Info);
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fpAttacks").GetArrayElementAtIndex(0), new GUIContent("Throw"));


                                            break;
                                        case WeaponsHelper.TypeOfAttack.Melee:

                                            fpAnimations.DoLayoutList();
                                            EditorGUILayout.Space();
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Rockets:
                                        case WeaponsHelper.TypeOfAttack.GrenadeLauncher:
                                        case WeaponsHelper.TypeOfAttack.Flame:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fpAttacks").GetArrayElementAtIndex(0), new GUIContent("Attack"));
                                            break;
                                        case WeaponsHelper.TypeOfAttack.Minigun:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fpAttacks").GetArrayElementAtIndex(0), new GUIContent("Auto Shoot"));
                                            break;
                                    }


                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.EndVertical();
                                    // if (i < script.Attacks.Count - 1)
                                    //     EditorGUILayout.Space();
                                }

                                break;

                            case 1:

                                EditorGUILayout.HelpBox("For this mode, the walk and run animations are calculated based on the character movement animations, and you just need to set the pose you need (Tools -> USK -> Adjust).", MessageType.Info);
                               
                                EditorGUILayout.Space();

                              
                                for (var i = 0; i < script.Attacks.Count; i++)
                                {
                                    
                                    var attack = script.Attacks[i];
                                    var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(i);
                                    
                                    EditorGUILayout.LabelField("Attack: " + script.attacksNames[i], EditorStyles.boldLabel);
                                   
                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.BeginVertical("HelpBox");
                                    switch (attack.AttackType)
                                    {
                                        case WeaponsHelper.TypeOfAttack.Bullets:
                                            if (attack.BulletsSettings[1].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpAttacks").GetArrayElementAtIndex(1), new GUIContent("Auto Shoot"));

                                            if (attack.BulletsSettings[0].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpAttacks").GetArrayElementAtIndex(0), new GUIContent("Single Shoot"));
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Grenade:
                                            
                                            EditorGUILayout.HelpBox("Add the [ThrowGrenade] event on animations to set the exact time of the grenade throw, otherwise, the grenade will be launched at the end of the animation.", MessageType.Info);
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpAttacks").GetArrayElementAtIndex(0), new GUIContent("Throw"));
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpCrouchAttacks").GetArrayElementAtIndex(0), new GUIContent("Throw (Crouch State)"));
                                            break;

                                        case WeaponsHelper.TypeOfAttack.Melee:
                                            fullBodyAnimations.DoLayoutList();
                                            EditorGUILayout.Space();
                                            fullBodyCrouchAnimations.DoLayoutList();
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Rockets:
                                        case WeaponsHelper.TypeOfAttack.GrenadeLauncher:
                                        case WeaponsHelper.TypeOfAttack.Flame:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpAttacks").GetArrayElementAtIndex(0), new GUIContent("Attack"));
                                            break;
                                        case WeaponsHelper.TypeOfAttack.Minigun:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tpAttacks").GetArrayElementAtIndex(0), new GUIContent("Auto Shoot"));
                                            break;

                                    }

                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.EndVertical();

                                }

                                break;

                            case 2:

                                EditorGUILayout.HelpBox("For this mode, the walk and run animations are calculated based on the character movement animations, and you just need to set the pose you need (Tools -> USK -> Adjust).", MessageType.Info);

                                EditorGUILayout.Space();
                                
                               
                                for (var i = 0; i < script.Attacks.Count; i++)
                                {
                                    var attack = script.Attacks[i];
                                    var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(i);

                                    EditorGUILayout.LabelField("Attack: " + script.attacksNames[i], EditorStyles.boldLabel);
                                    
                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.BeginVertical("HelpBox");
                                    
                                    switch (attack.AttackType)
                                    {
                                        case WeaponsHelper.TypeOfAttack.Bullets:
                                            if (attack.BulletsSettings[1].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tdAttacks").GetArrayElementAtIndex(1), new GUIContent("Auto Shoot"));

                                            if (attack.BulletsSettings[0].Active)
                                                EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tdAttacks").GetArrayElementAtIndex(0), new GUIContent("Single Shoot"));
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Grenade:
                                            EditorGUILayout.HelpBox("Add the [ThrowGrenade] event on animations to set the exact time of the grenade throw, otherwise, the grenade will be launched at the end of the animation.", MessageType.Info);
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tdAttacks").GetArrayElementAtIndex(0), new GUIContent("Throw"));
                                            break;

                                        case WeaponsHelper.TypeOfAttack.Melee:
                                            tdAnimations.DoLayoutList();
                                            break;
                                        
                                        case WeaponsHelper.TypeOfAttack.Rockets:
                                        case WeaponsHelper.TypeOfAttack.GrenadeLauncher:
                                        case WeaponsHelper.TypeOfAttack.Flame:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tdAttacks").GetArrayElementAtIndex(0), new GUIContent("Attack"));
                                            break;
                                        case WeaponsHelper.TypeOfAttack.Minigun:
                                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("tdAttacks").GetArrayElementAtIndex(0), new GUIContent("Auto Shoot"));
                                            break;

                                    }
                                    
                                    if(attack.AttackType != WeaponsHelper.TypeOfAttack.Melee)
                                        EditorGUILayout.EndVertical();

                                }
                                break;
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    break;

                case "Sounds":
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(grayBackground);

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PickUpWeaponAudio"), new GUIContent("Pick Up"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DropWeaponAudio"), new GUIContent("Drop"));

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    for (var i = 0; i < script.Attacks.Count; i++)
                    {
                        var attack = script.Attacks[i];
                        var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(i);

                        EditorGUILayout.LabelField("Attack: " + script.attacksNames[i], EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical("HelpBox");

                        if (attack.AttackType == WeaponsHelper.TypeOfAttack.Melee)
                        {
                            EditorGUILayout.HelpBox("Add the [PlayAttackSound] event on attack animations to set the exact playing time of the attack sound.", MessageType.Info);
                        }

                        EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackAudio"), new GUIContent("Attack"));

                        if (attack.AttackType != WeaponsHelper.TypeOfAttack.Melee && attack.AttackType != WeaponsHelper.TypeOfAttack.Grenade)
                        {
                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("ReloadAudio"), new GUIContent("Reload"));
                            EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("NoAmmoShotAudio"), new GUIContent("Attack without ammo"));
                        }

                        EditorGUILayout.EndVertical();
                        if (i < script.Attacks.Count - 1)
                            EditorGUILayout.Space();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            // DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        void CheckPoint(WeaponsHelper.Attack _attack, string type)
        {
            if (type == "attack" && !_attack.AttackSpawnPoint || type != "attack" && !_attack.ShellPoint)
            {
                if (!script.gameObject.activeInHierarchy)
                {
                    if (GUILayout.Button("Create point"))
                    {
                        var tempWeapon = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
                        if (type == "attack") tempWeapon.GetComponent<WeaponController>().Attacks[script.currentAttack].AttackSpawnPoint = Helper.NewPoint(tempWeapon, "Attack Point");
                        else tempWeapon.GetComponent<WeaponController>().Attacks[script.currentAttack].ShellPoint = Helper.NewPoint(tempWeapon, "Shell Spawn Point");

#if !UNITY_2018_3_OR_NEWER
                        PrefabUtility.ReplacePrefab(tempWeapon, PrefabUtility.GetPrefabParent(tempWeapon), ReplacePrefabOptions.ConnectToPrefab);
#else
                        PrefabUtility.SaveAsPrefabAssetAndConnect(tempWeapon, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempWeapon), InteractionMode.AutomatedAction);
#endif

                        DestroyImmediate(tempWeapon);
                    }
                }
                else
                {
                    if (GUILayout.Button("Create point"))
                    {
                        if (type == "attack") script.Attacks[script.currentAttack].AttackSpawnPoint = Helper.NewPoint(script.gameObject, "Attack Point");
                        else script.Attacks[script.currentAttack].ShellPoint = Helper.NewPoint(script.gameObject, "Shell Spawn Point");
                    }
                }
            }
            else if (type == "attack" && _attack.AttackSpawnPoint || type != "attack" && _attack.ShellPoint)
            {
                if (type == "attack" && _attack.AttackSpawnPoint.localPosition == Vector3.zero)
                    EditorGUILayout.HelpBox("Adjust the position of the [Attack Point]", MessageType.Warning);

                else if (type != "attack" && _attack.ShellPoint.localPosition == Vector3.zero)
                    EditorGUILayout.HelpBox("Adjust the position of the [Shell Spawn Point]", MessageType.Warning);
            }
        }

        void CheckCollider(WeaponsHelper.Attack _attack, string type)
        {
            if (_attack.AttackCollider)
            {
                if (_attack.AttackCollider.transform.localScale == Vector3.one)
                {
                    EditorGUILayout.HelpBox("Adjust the size of the" + (type == "fire" ? " Fire Collider." : " Melee Collider.") +
                                            " It's the area that will deal damage.", MessageType.Warning);
                }
            }
            else
            {
                if (!script.gameObject.activeInHierarchy)
                {
                    if (GUILayout.Button("Create collider"))
                    {
                        var tempWeapon = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
                        tempWeapon.GetComponent<WeaponController>().Attacks[script.currentAttack].AttackCollider = type == "fire"
                            ? Helper.NewCollider("Fire Collider", "Fire", tempWeapon.transform)
                            : Helper.NewCollider("Melee Collider", "Melee Collider", tempWeapon.transform);
#if !UNITY_2018_3_OR_NEWER
                        PrefabUtility.ReplacePrefab(tempWeapon, PrefabUtility.GetPrefabParent(tempWeapon), ReplacePrefabOptions.ConnectToPrefab);
#else
                        PrefabUtility.SaveAsPrefabAssetAndConnect(tempWeapon, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempWeapon), InteractionMode.AutomatedAction);
#endif
                        DestroyImmediate(tempWeapon);
                    }
                }
                else
                {
                    if (GUILayout.Button("Create collider"))
                    {
                        script.Attacks[script.currentAttack].AttackCollider = type == "fire"
                            ? Helper.NewCollider("Fire Collider", "Fire", script.transform)
                            : Helper.NewCollider("Melee Collider", "Melee Collider", script.transform);
                    }
                }
            }
        }
    }
}


