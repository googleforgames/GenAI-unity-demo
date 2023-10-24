using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(Controller))]
    public class ControllerEditor : Editor
    {
        public Controller script;
        private Animator _animator;

        private ReorderableList bloodHoles;
        private ReorderableList additionalEffects;
        private ReorderableList damageSounds;

        private CameraController camera;
        
        private GUIStyle grayBackground;
        private GUIStyle style;
        
        private bool deleteMultiplayerScripts;
        
        private bool isSceneObject;

        public void Awake()
        {
            script = (Controller) target;
            _animator = script.gameObject.GetComponent<Animator>();
        }


        void OnEnable()
        {
            
            var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            isSceneObject = allObjectsOnScene.Contains(script.gameObject); 

            
            bloodHoles = new ReorderableList(serializedObject, serializedObject.FindProperty("BloodHoles"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Blood Holes", EditorStyles.boldLabel); },

                onAddCallback = items => { script.BloodHoles.Add(null); },

                onRemoveCallback = items =>
                {
                    if (script.BloodHoles.Count == 1)
                        return;

                    script.BloodHoles.Remove(script.BloodHoles[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.BloodHoles[index] = (Texture) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        script.BloodHoles[index], typeof(Texture), false);
                }
            };

            additionalEffects = new ReorderableList(serializedObject, serializedObject.FindProperty("additionalHitEffects"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Additional Hit Effects°", "A random object from here will spawn where a bullet hit (use this to add extra effects like blood)."), EditorStyles.boldLabel); },

                onAddCallback = items =>
                {
                    script.additionalHitEffects.Add(null);
                },

                onRemoveCallback = items =>
                {
                    script.additionalHitEffects.Remove(script.additionalHitEffects[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.additionalHitEffects[index] = (GameObject) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.additionalHitEffects[index], typeof(GameObject), false);
                }
            };
            
            damageSounds = new ReorderableList(serializedObject, serializedObject.FindProperty("damageSounds"), false, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Damage Sounds°", "These sounds will be played when the character takes damage."), EditorStyles.boldLabel); },

                onAddCallback = items =>
                {
                    script.damageSounds.Add(null);
                },

                onRemoveCallback = items =>
                {
                    script.damageSounds.Remove(script.damageSounds[items.index]);
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    script.damageSounds[index] = (AudioClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.damageSounds[index], typeof(AudioClip), false);
                }
            };
            
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            if (Application.isPlaying && script)
            {
                if (!camera && script.thisCamera)
                    camera = script.CameraController;
            }
            else if(!Application.isPlaying && script)
            {
                if (script && !script.inventoryManager)
                    script.inventoryManager = script.GetComponent<InventoryManager>();
                
                
                if (string.IsNullOrEmpty(script.characterID))
                {
                    script.characterID = Helper.GenerateRandomString(20);
                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                
                // script.health = script.health;
                
                if (deleteMultiplayerScripts)
                {
                    deleteMultiplayerScripts = false;
                    CharacterHelper.RemoveMultiplayerScripts(script.gameObject);
                }

                if (script.gameObject.GetComponent<CharacterSync>() && !script.CharacterSync)
                    script.CharacterSync = script.gameObject.GetComponent<CharacterSync>();

                if (!script.characterAnimatorController)
                {
                    script.characterAnimatorController = Resources.Load("Character", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/_Animator Controllers/Character.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                
#if USK_NWHVPH_INTEGRATION
                if (!script.gameObject.GetComponent<InteractionWithCars>())
                {
                    if (script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>())
                        DestroyImmediate(script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>());
                }
                else
                {
                    if (script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>() && script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>().hideFlags != HideFlags.HideInInspector)
                    {
                        script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>().enabled = false;
                        script.gameObject.GetComponent<NWH.Common.SceneManagement.VehicleChanger>().hideFlags = HideFlags.HideInInspector;
                    }
                }
#endif

                // if (script && !script.projectSettings)
                // {
                //     script.projectSettings = AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
                //     EditorUtility.SetDirty(script.gameObject);
                //     EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                // }
                
                if (!script.handDirectionObject)
                {
                    var tempCharacter = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
                    var controller = tempCharacter.GetComponent<Controller>();
                    
                    controller.handDirectionObject = new GameObject("Hands Direction object").transform;
                    controller.handDirectionObject.parent = controller.BodyObjects.RightHand;
                    controller.handDirectionObject.localPosition = Vector3.zero;
                    controller.handDirectionObject.localEulerAngles = Vector3.zero;
                    controller.handDirectionObject.hideFlags = HideFlags.HideInHierarchy;
                    
#if !UNITY_2018_3_OR_NEWER
                    PrefabUtility.ReplacePrefab(tempCharacter, PrefabUtility.GetPrefabParent(tempCharacter), ReplacePrefabOptions.ConnectToPrefab);
#else
                    PrefabUtility.SaveAsPrefabAssetAndConnect(tempCharacter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempCharacter), InteractionMode.AutomatedAction);
#endif

                    DestroyImmediate(tempCharacter);

                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }

                if (!script.FeetAudioSource)
                {
                    var tempCharacter = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

                    var controller = tempCharacter.GetComponent<Controller>();

                    controller.FeetAudioSource = new GameObject("FeetAudio").AddComponent<AudioSource>();
                    controller.FeetAudioSource.transform.parent = tempCharacter.transform;
                    controller.FeetAudioSource.transform.localPosition = Vector3.zero;

#if !UNITY_2018_3_OR_NEWER
                    PrefabUtility.ReplacePrefab(tempCharacter, PrefabUtility.GetPrefabParent(tempCharacter), ReplacePrefabOptions.ConnectToPrefab);
#else
                    PrefabUtility.SaveAsPrefabAssetAndConnect(tempCharacter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempCharacter), InteractionMode.AutomatedAction);
#endif

                    DestroyImmediate(tempCharacter);

                    EditorUtility.SetDirty(script.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }

                if (script.thisCamera && !script.thisCamera.GetComponent<CameraController>().layerCamera)
                {
                    if (!isSceneObject)
                    {
                        var tempCharacter = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

                        var cameraController = tempCharacter.GetComponent<Controller>().thisCamera.GetComponent<CameraController>();

                        var layerCamera = Helper.NewCamera("Layer Camera", cameraController.transform, "CameraController");
                        layerCamera.gameObject.SetActive(false);
                        cameraController.layerCamera = layerCamera;

#if !UNITY_2018_3_OR_NEWER
                    PrefabUtility.ReplacePrefab(tempCharacter, PrefabUtility.GetPrefabParent(tempCharacter), ReplacePrefabOptions.ConnectToPrefab);
#else
                        PrefabUtility.SaveAsPrefabAssetAndConnect(tempCharacter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempCharacter), InteractionMode.AutomatedAction);
#endif

                        DestroyImmediate(tempCharacter);

                        EditorUtility.SetDirty(script.gameObject);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }

                    else
                    {
                        var tempCharacter = script.gameObject;
                        var cameraController = tempCharacter.GetComponent<Controller>().thisCamera.GetComponent<CameraController>();

                        var layerCamera = Helper.NewCamera("Layer Camera", cameraController.transform, "CameraController");
                        layerCamera.gameObject.SetActive(false);
                        cameraController.layerCamera = layerCamera;
                    }
                }

                if (script.characterTag > script.projectSettings.CharacterTags.Count - 1)
                {
                    script.characterTag = script.projectSettings.CharacterTags.Count - 1;
                }

                if (_animator)
                {
                    if (_animator.isHuman)
                    {
                        if (!script.BodyObjects.RightHand)
                        {
                            script.BodyObjects.RightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }

                        if (!script.BodyObjects.LeftHand)
                        {
                            script.BodyObjects.LeftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }

                        if (!script.BodyObjects.Head)
                        {
                            script.BodyObjects.Head = _animator.GetBoneTransform(HumanBodyBones.Head);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }

                        if (!script.BodyObjects.TopBody)
                        {
                            script.BodyObjects.TopBody = _animator.GetBoneTransform(HumanBodyBones.Spine);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }

                        if (!script.BodyObjects.Hips)
                        {
                            script.BodyObjects.Hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }

                        if (!script.BodyObjects.Chest)
                        {
                            script.BodyObjects.Chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
                            EditorUtility.SetDirty(script);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            Helper.InitStyles(ref grayBackground, new Color32(160, 160, 160, 200));
            
            serializedObject.Update();

            style = new GUIStyle{richText = true, fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};

            
            EditorGUILayout.Space();
            // EditorGUILayout.Space();

            if (!script.BodyParts[0] || script.BodyParts[0] && !script.BodyParts[0].GetComponent<Rigidbody>())
            {
                EditorGUILayout.HelpBox("Generate Body Colliders in the [Health] tab.", MessageType.Info);
                EditorGUILayout.Space();
            }
            
            var backgroundColor = GUI.backgroundColor;
#if USK_MULTIPLAYER
            GUI.backgroundColor = script.CharacterSync ? new Color(0,1,0,0.5f): new Color(1, 0,0, 0.3f);
            
            EditorGUILayout.BeginVertical("HelpBox");
			
            if(!script.CharacterSync)
                EditorGUILayout.Space();
			
            EditorGUILayout.LabelField(!script.CharacterSync ? "<b>Not ready for multiplayer</b> " + "\n" + 
                                                               "(change it in the [Multiplayer] tab)" : "<b>Ready for multiplayer</b>", style);
            
            if(!script.CharacterSync)
                EditorGUILayout.Space();
			
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.Space();
#endif

//            EditorGUILayout.BeginVertical("Box");
            script.inspectorTabTop = GUILayout.Toolbar(script.inspectorTabTop, new[] {new GUIContent( "Camera"), new GUIContent( "Movement"), new GUIContent("Health")});

            switch (script.inspectorTabTop)
            {
                case 0:
                    script.currentInspectorTab = 0;
                    script.inspectorTabDown = 3;
                    break;


                case 1:
                    script.currentInspectorTab = 1;
                    script.inspectorTabDown = 3;
                    break;
                
                case 2:
                    script.currentInspectorTab = 2;
                    script.inspectorTabDown = 3;
                    break;
            }

            script.inspectorTabDown = GUILayout.Toolbar(script.inspectorTabDown, new[] {"Parameters", "Multiplayer"});

            switch (script.inspectorTabDown)
            {
                case 0:
                    script.currentInspectorTab = 3;
                    script.inspectorTabTop = 4;
                    break;

                case 1:
                    script.currentInspectorTab = 4;
                    script.inspectorTabTop = 4;
                    break;
            }

//            EditorGUILayout.EndVertical();

            switch (script.currentInspectorTab)
            {
                case 0:

                    EditorGUILayout.Space();                    
                    EditorGUILayout.Space();                    
                    
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.switchCameraSpeed"), new GUIContent("Switch Camera Speed"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    script.cameraInspectorTab = GUILayout.Toolbar(script.cameraInspectorTab, new[] {"Third Person", "First Person", "Top Down"});
                    switch (script.cameraInspectorTab)
                    {
                        case 0:

                            if (!Application.isPlaying)
                                script.TypeOfCamera = CharacterHelper.CameraType.ThirdPerson;

                            script.moveInspectorTab = 0;
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraParameters.activeTP = EditorGUILayout.ToggleLeft("Use", script.CameraParameters.activeTP);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical(grayBackground);
                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpXMouseSensitivity"), new GUIContent("X Sensitivity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpYMouseSensitivity"), new GUIContent("Y Sensitivity"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpAimXMouseSensitivity"), new GUIContent("(Aim) X Sensitivity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpAimYMouseSensitivity"), new GUIContent("(Aim) Y Sensitivity"));
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpAimDepth"), new GUIContent("Aim depth"));
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpXLimitMax"), new GUIContent("Vertical Min Limit"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpXLimitMin"), new GUIContent("Vertical Max Limit"));
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpSmoothX"), new GUIContent("Vertical Smooth"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tpSmoothY"), new GUIContent("Horizontal Smooth"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraParameters.alwaysTPAimMode = EditorGUILayout.ToggleLeft(new GUIContent("Always Aim"), script.CameraParameters.alwaysTPAimMode);
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            script.SmoothCameraWhenMoving = EditorGUILayout.ToggleLeft(new GUIContent("Smooth Camera°","When the character walks, the camera moves away from him"), script.SmoothCameraWhenMoving);
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraFollowCharacter = EditorGUILayout.ToggleLeft(new GUIContent("Camera Bobbing°","The camera follows the character’s head"), script.CameraFollowCharacter);
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.EndVertical();
                            break;

                        case 1:

                            if (!Application.isPlaying)
                                script.TypeOfCamera = CharacterHelper.CameraType.FirstPerson;

                            script.moveInspectorTab = 1;
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraParameters.activeFP = EditorGUILayout.ToggleLeft("Use", script.CameraParameters.activeFP);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(grayBackground);
                            EditorGUILayout.HelpBox("Set the 'Head' layer for the character's head, then the camera will not see it.", MessageType.Info);
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpXMouseSensitivity"), new GUIContent("X Sensitivity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpYMouseSensitivity"), new GUIContent("Y Sensitivity"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpAimXMouseSensitivity"), new GUIContent("(Aim) X Sensitivity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpAimYMouseSensitivity"), new GUIContent("(Aim) Y Sensitivity"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpAimDepth"), new GUIContent("Aim depth"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpXLimitMin"), new GUIContent("Vertical Min Limit"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpXLimitMax"), new GUIContent("Vertical Max Limit"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpXSmooth"), new GUIContent("Horizontal Smooth"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.fpYSmooth"), new GUIContent("Vertical Smooth"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothHandsMovement"), new GUIContent("Smooth Hands Movement°"));

                            if (script.smoothHandsMovement)
                            {
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothHandsMovementValue"), new GUIContent("Value"));

                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Head Bobbing", EditorStyles.boldLabel);
                            
                            EditorGUILayout.HelpBox("You can set head bob for each weapon in [WeaponController] scripts. In this case, these parameters will be used when the character is unarmed.", MessageType.Info);
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.bobbingValues.bobbingRotationAxis"), new GUIContent("Axis"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.bobbingValues.bobbingAmplitude"), new GUIContent("Amplitude"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.bobbingValues.bobbingDuration"), new GUIContent("Duration"));
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();

                            break;

                        case 2:

                            if (!Application.isPlaying)
                                script.TypeOfCamera = CharacterHelper.CameraType.TopDown;

                            script.moveInspectorTab = 2;
                            EditorGUILayout.Space();
                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraParameters.activeTD = EditorGUILayout.ToggleLeft("Use", script.CameraParameters.activeTD);
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical(grayBackground);
                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            script.CameraParameters.lockCamera = EditorGUILayout.ToggleLeft("Lock Camera", script.CameraParameters.lockCamera);
                            if (script.CameraParameters.lockCamera)
                            {
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.CursorImage"), new GUIContent("Cursor Image"));
                                EditorGUILayout.Space();

                                script.CameraParameters.lookAtCursor = EditorGUILayout.ToggleLeft(new GUIContent("Aim at Cursor°","• If this feature is active, the character will aim where the cursor is pointing" + "\n\n" +
                                                                                                                                 "• If not, the character will aim always forward"), script.CameraParameters.lookAtCursor);
//                                EditorGUILayout.HelpBox("If this feature is active, the character will aim where the cursor is pointing." + "\n" +
//                                                        "If not, the character will aim always forward.", MessageType.Info);

                                if (script.CameraParameters.lookAtCursor)
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tdXLimitMin"), new GUIContent("Body Rotation(x) Min"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tdXLimitMax"), new GUIContent("Body Rotation(x) Max"));
                                }

                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                script.CameraParameters.alwaysTDAim = EditorGUILayout.ToggleLeft(new GUIContent("Always Aim°","• If this feature is active, the character will always aim " + "\n\n" +
                                                                                                                              "• If not, the character will aim when the button is pressed (like in TP view)"), script.CameraParameters.alwaysTDAim);
//                                EditorGUILayout.HelpBox("If this feature is active, the character will always aim. " + "\n" +
//                                                        "If not, the character will aim when the button is pressed (like in TP view).", MessageType.Info);
                            }

                            EditorGUILayout.EndVertical();

                            if (!script.CameraParameters.lockCamera || script.CameraParameters.lockCamera && !script.CameraParameters.lookAtCursor)
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.shootingAngleCorrection"), new GUIContent("Shooting Angle Correction°", "With the selected camera modes, the character shoots straight ahead. Sometimes this can cause bullets to fly above the heads of opponents. So you can slightly adjust the angle of attack to correct this."));
                                EditorGUILayout.EndVertical();
                            }

                            EditorGUILayout.Space();

                            
                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tdXMouseSensitivity"), new GUIContent("Sensitivity"));

                            if (!script.CameraParameters.lockCamera)
                            {
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraParameters.tdSmoothX"), new GUIContent("Smooth"));
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            break;
                    }

                    break;

                case 1:
                    EditorGUILayout.Space();

                    script.moveInspectorTab = GUILayout.Toolbar(script.moveInspectorTab, new[] {"Third Person", "First Person", "Top Down"});

                    switch (script.moveInspectorTab)
                    {
                        case 0:

                            script.cameraInspectorTab = 0;

                            EditorGUILayout.Space();
                            
                            EditorGUILayout.HelpBox("Movement in the Aim state is based on the CharacterController, other states are based on the Root Motion animations." + "\n\n" +
                                                    "You can adjust movement and jump settings manually, and also speed multiplier for all animations.", MessageType.Info);

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical(grayBackground);

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementType"), new GUIContent("Movement Type"));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.LabelField(new GUIContent("Animations Speed Multiplier°", "Affects all states expect the aim mode."));
                            script.TPspeedOffset = EditorGUILayout.Slider(script.TPspeedOffset, 0.1f, 2);
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.Space();

                            
                            script.TPSpeedInspectorTab = GUILayout.Toolbar(script.TPSpeedInspectorTab, new[] {"Aim Walk", "Aim Run", "Jump"});
                            // EditorGUILayout.Space();

//                            EditorGUILayout.BeginVertical("HelpBox");
                            switch (script.TPSpeedInspectorTab)
                            {
                                case 0:
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.NormForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.NormBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.NormLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
                                    break;

                                case 1:
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.RunForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.RunBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.RunLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
                                    break;

                                case 2:
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.JumpHeight"), new GUIContent("Height"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSpeed.JumpSpeed"), new GUIContent("Speed"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();

                                    EditorGUILayout.HelpBox("You can also change the gravity in [Edit -> Project Settings -> Physics -> Gravity (Y value)].", MessageType.Info);
                                    break;
                            }

//                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndVertical();
                            break;

                        case 1:
                            EditorGUILayout.Space();

                            script.cameraInspectorTab = 1;

                            EditorGUILayout.BeginVertical(grayBackground);
                            script.inspectorSettingsTab = GUILayout.Toolbar(script.inspectorSettingsTab, new[] {"Walk", "Run", "Crouch", "Jump"});

                            switch (script.inspectorSettingsTab)
                            {
                                case 0:
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.NormForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.NormBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.NormLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
                                    break;
                                case 1:
                                    EditorGUILayout.Space();
//                            script.activeSprint = EditorGUILayout.Toggle("Enabled", script.activeSprint);
//                            EditorGUILayout.Space();
//                            if (script.activeSprint)
//                            {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.RunForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.RunBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.RunLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
//                            }

//                            EditorGUILayout.Space();
                                    break;
                                case 2:

                                    EditorGUILayout.Space();
//                            script.activeCrouch = EditorGUILayout.Toggle("Enabled", script.activeCrouch);
//                            EditorGUILayout.Space();
//                            if (script.activeCrouch)
//                            {

//                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.CrouchForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.CrouchBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.CrouchLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CrouchHeight"), new GUIContent("Crouch depth"));
                                    EditorGUILayout.EndVertical();

//                            }
//                            EditorGUILayout.Space();
                                    break;
                                case 3:
                                    EditorGUILayout.Space();
//                            EditorGUILayout.PropertyField(serializedObject.FindProperty("activeJump"),
//                                new GUIContent("Enabled"));
//                            EditorGUILayout.Space();
//                            if (script.activeJump)
//                            {
                                    EditorGUILayout.BeginVertical("HelpBox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.JumpHeight"), new GUIContent("Height"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FPSpeed.JumpSpeed"), new GUIContent("Speed"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                    EditorGUILayout.HelpBox("You can also change the gravity in [Edit -> Project Settings -> Physics -> Gravity (Y value)].", MessageType.Info);
//                            }

//                            EditorGUILayout.Space();
                                    break;
                            }
                            
                            EditorGUILayout.EndVertical();

                            break;

                        case 2:

                            script.cameraInspectorTab = 2;

                            EditorGUILayout.Space();

                            
                            if (!script.CameraParameters.lockCamera && !script.CameraParameters.alwaysTDAim)
                            {
                                EditorGUILayout.HelpBox("Movement in the Aim state is based on the CharacterController, other states are based on the Root Motion animations." + "\n\n" +
                                                        "You can adjust movement and jump settings manually, and also speed multiplier for all animations.", MessageType.Info);
                                
                                EditorGUILayout.Space();
                                
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.BeginVertical("HelpBox");
                                EditorGUILayout.LabelField(new GUIContent("Animations Speed Multiplier°", "Affects all states expect the aim mode."));
                                script.TPspeedOffset = EditorGUILayout.Slider(script.TPspeedOffset, 0.1f, 2);
                                EditorGUILayout.EndVertical();

                            }
                            else
                            {
                                EditorGUILayout.HelpBox("All movement is based on the CharacterController." + "\n" + 
                                                        "You can adjust speed and jump settings manually.", MessageType.Info);
                            }
                            
                            EditorGUILayout.Space();

                            if (script.CameraParameters.lockCamera || script.CameraParameters.alwaysTDAim)
                                EditorGUILayout.BeginVertical("helpbox");
                            
                            script.TDSpeedInspectorTab = GUILayout.Toolbar(script.TDSpeedInspectorTab, new[] {"Walk", "Run", "Jump"});
                            // EditorGUILayout.Space();

                            
                            switch (script.TDSpeedInspectorTab)
                            {
                                case 0:
//                                            EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("helpbox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.NormForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.NormBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.NormLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();

                                    break;
                                case 1:
//                                            EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("helpbox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.RunForwardSpeed"), new GUIContent("Forward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.RunBackwardSpeed"), new GUIContent("Backward speed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.RunLateralSpeed"), new GUIContent("Lateral speed"));
                                    EditorGUILayout.EndVertical();
                                    break;

                                case 2:
//                                            EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical("helpbox");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.JumpHeight"), new GUIContent("Height"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TDSpeed.JumpSpeed"), new GUIContent("Speed"));
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                    EditorGUILayout.HelpBox("You can also change the gravity in [Edit -> Project Settings -> Physics -> Gravity (Y value)].", MessageType.Info);
                                    break;
                            }

//                                    break;
//                                case CharacterHelper.MovementType.Realistic:
//                                    EditorGUILayout.HelpBox("In this mode, transitions between animations are slower, animator controller has more 'Start'/'Stop' states." + "\n\n" +
//                                                            "All movement is based on the Root Motion animations.", MessageType.Info);
//
//                                    break;
//                            }

                            
                            if (script.CameraParameters.lockCamera || script.CameraParameters.alwaysTDAim)
                                EditorGUILayout.EndVertical();
                            
                            if (!script.CameraParameters.lockCamera && !script.CameraParameters.alwaysTDAim)
                                EditorGUILayout.EndVertical();
                            
                            break;
                    }

                    break;

                case 2:
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(grayBackground);

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("health"), new GUIContent("Health Value"));
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    
                    if (!script.BodyParts[0] || script.BodyParts[0] && !script.BodyParts[0].GetComponent<Rigidbody>())
                    {
#if !UNITY_2018_3_OR_NEWER
                        EditorGUILayout.HelpBox("Place this prefab in a scene to create the Body Colliders, then apply changes.", MessageType.Info);
#else
                        EditorGUILayout.HelpBox("Open this prefab to create the Body Colliders.", MessageType.Info);
#endif
                        EditorGUI.BeginDisabledGroup(!script.gameObject.activeInHierarchy);
                        if (GUILayout.Button("Generate Body Colliders"))
                        {
                            CreateRagdoll();
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    else if (script.BodyParts[0] && script.BodyParts[0].GetComponent<Rigidbody>())
                    {
                        EditorGUILayout.LabelField(new GUIContent("Damage Multipliers°","An enemy's weapon damage will be multiplied by these values"), EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical("HelpBox");
//                        EditorGUILayout.HelpBox("An enemy's weapon damage will be multiplied by these values.", MessageType.Info);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("headMultiplier"), new GUIContent("Head"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyMultiplier"), new GUIContent("Body"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("handsMultiplier"), new GUIContent("Hands"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("legsMultiplier"), new GUIContent("Legs"));
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    bloodHoles.DoLayoutList();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    additionalEffects.DoLayoutList();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("playDamageSoundsDelay"), new GUIContent("Delay°", "After how many attacks the sounds will be played (set to 0 to play sounds every time the player takes damage)."));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    
                    damageSounds.DoLayoutList();
                    EditorGUILayout.EndVertical();

                    break;


                case 3:
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical(grayBackground);

                    EditorGUILayout.BeginVertical("HelpBox");
                    
                    script.characterTag = EditorGUILayout.Popup("Character ID", script.characterTag, script.projectSettings.CharacterTags.ToArray());

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(script.rename);
                    if (GUILayout.Button("Rename"))
                    {
                        script.rename = true;
                        script.curName = "";
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(script.projectSettings.CharacterTags.Count <= 1 || script.delete);

                    if (GUILayout.Button("Delete"))
                    {
                        script.delete = true;
                    }


                    EditorGUI.EndDisabledGroup();


                    if (GUILayout.Button("Create a new one"))
                    {
                        if (!script.projectSettings.CharacterTags.Contains("Character " + script.projectSettings.CharacterTags.Count))
                            script.projectSettings.CharacterTags.Add("Character " + script.projectSettings.CharacterTags.Count);
                        else script.projectSettings.CharacterTags.Add("Character " + Random.Range(10, 100));

                        script.characterTag = script.projectSettings.CharacterTags.Count - 1;

                        EditorUtility.SetDirty(script);
                        EditorUtility.SetDirty(script.projectSettings);
                    }

                    EditorGUILayout.EndHorizontal();
                    
                    if (script.delete)
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
                            script.projectSettings.CharacterTags.Remove(script.projectSettings.CharacterTags[script.characterTag]);
                            script.characterTag = script.projectSettings.CharacterTags.Count - 1;
                            script.delete = false;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    else if (script.rename)
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
                            if (!script.projectSettings.CharacterTags.Contains(script.curName))
                            {
                                script.rename = false;
                                script.projectSettings.CharacterTags[script.characterTag] = script.curName;
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
                    
                    EditorGUILayout.EndVertical();

//                            break;

//                        case 1:
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Noise Radius°", "These parameters show a radius of character's noise. " + "\n\n" +
                                                                               "If the character makes a lot of noise, an enemy will notice him."), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("IdleNoise"), new GUIContent("Idle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MovementNoise"), new GUIContent("Walk"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SprintMovementNoise"), new GUIContent("Run"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CrouchIdleNoise"), new GUIContent("Crouch Idle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CrouchMovementNoise"), new GUIContent("Crouch Walk"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Icons for Minimaps", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipMainTexture"), new GUIContent("Main Blip"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipDeathTexture"), new GUIContent("Death Blip"));
                    // EditorGUILayout.Space();
//                    EditorGUILayout.HelpBox("This icon will be used in the multiplayer mode for opponets and teammates.", MessageType.Info);
                   
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.EndVertical();


                    // EditorGUILayout.Space();
                    // EditorGUILayout.Space();
                  
//                            break;
//                    }
                    

                    break;
                
                case 4:
                    
                    EditorGUILayout.Space();

#if USK_MULTIPLAYER
                    if (script.CharacterSync)
                    {
                        EditorGUILayout.HelpBox(isSceneObject ? "Use the Lobby scripts to load this character in multiplayer game." : 
                            "Make sure this character is located in the [Resources] folder.", MessageType.Info);
                        
                        EditorGUILayout.Space();
                    }
#else
                    EditorGUILayout.HelpBox("To enable the multiplayer module, open the Integrations Manager [Window/Universal Shooter Kit/...].", MessageType.Info);
                    
                    EditorGUILayout.Space();
#endif
                    
                    

#if !USK_MULTIPLAYER
                    EditorGUI.BeginDisabledGroup(true);
#endif
                    
                    // EditorGUILayout.BeginVertical("HelpBox");
                    
                    if (!script.CharacterSync)
                    {
#if USK_MULTIPLAYER
                        EditorGUILayout.HelpBox("Press the button below to add necessary scripts and use this character in multiplayer. " + "\n" +
                                                "After you add them, you do not need to do anything extra, everything works automatically.", MessageType.Info);
#endif
                        EditorGUILayout.Space();
                        
                        backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0,1,0,0.5f);
                        
                        if (GUILayout.Button("Add Multiplayer Scripts"))
                        {
#if USK_MULTIPLAYER
                            CharacterHelper.AddMultiplayerScripts(script.gameObject);
#endif
                        }
                        
                        GUI.backgroundColor = backgroundColor;
                    }
                    else
                    {
                        backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1,0,0,0.5f);
                        
                        if (GUILayout.Button("Remove Multiplayer Scripts"))
                        {
                            deleteMultiplayerScripts = true;
                        }
                        
                        GUI.backgroundColor = backgroundColor;
                    }
                    // EditorGUILayout.EndVertical();
                    
                   
#if USK_MULTIPLAYER
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical("HelpBox");  

                    EditorGUI.BeginDisabledGroup(!script.CharacterSync);
                    
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blipMultiplayerTexture"), new GUIContent("Multiplayer Blip°"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("multiplayerStatsBackground"), new GUIContent("Stats Background"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("healthBarImage"), new GUIContent("Health Bar"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nickNameText"), new GUIContent("Nickname"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("opponentColor"), new GUIContent("Opponent Color"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("teammateColor"), new GUIContent("Teammate Color"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUI.EndDisabledGroup();
#endif
                    
#if !USK_MULTIPLAYER
                    EditorGUI.EndDisabledGroup();
#endif
                      
#if USK_MULTIPLAYER
                      EditorGUILayout.EndVertical();
#endif
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

        void CreateRagdoll()
        {
            if (!script.gameObject.activeInHierarchy)
            {
                var tempCharacter = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
                var controller = tempCharacter.GetComponent<Controller>();

                foreach (var part in controller.BodyParts)
                {
                    if (part)
                    {
                        foreach (var comp in part.GetComponents<Component>())
                        {
                            if (comp is CharacterJoint || comp is Rigidbody || comp is CapsuleCollider)
                            {
                                DestroyImmediate(comp);
                            }
                        }
                    }
                }

                Helper.CreateRagdoll(controller.BodyParts, tempCharacter.GetComponent<Animator>());


#if !UNITY_2018_3_OR_NEWER
                PrefabUtility.ReplacePrefab(tempCharacter, PrefabUtility.GetPrefabParent(tempCharacter), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempCharacter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempCharacter), InteractionMode.AutomatedAction);
#endif

                DestroyImmediate(tempCharacter);
            }
            else
            {
                foreach (var part in script.BodyParts)
                {
                    if (part)
                    {
                        foreach (var comp in part.GetComponents<Component>())
                        {
                            if (comp is CharacterJoint || comp is Rigidbody || comp is CapsuleCollider)
                            {
                                DestroyImmediate(comp);
                            }
                        }
                    }
                }

                Helper.CreateRagdoll(script.BodyParts, script.gameObject.GetComponent<Animator>());

            }
        }
    }
}

