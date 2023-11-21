// GercStudio
// © 2018-2020

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;   
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace GercStudio.USK.Scripts
{
    public class CreateCharacterWindow : EditorWindow
    {

        public GameObject CharacterModel;
        public GameObject Ragdoll;
        public Avatar avatar;

        private bool avatarError;
        private bool characterError;
        private bool characterAdded;
        private bool CameraParametersError;
        private bool hasCreated;
        private bool startCreation;
        private float startVal;
        private float progress;
        private Vector2 scrollPos;

        private RagdollHelper.BodyParameters bodyParameters = new RagdollHelper.BodyParameters();

        private GUIStyle style;
        private GUIStyle labelStyle;

        [MenuItem("Tools/Universal Shooter Kit/Create/Character")]
        public static void ShowWindow()
        {
            GetWindowWithRect(typeof(CreateCharacterWindow), new Rect(Vector2.zero, new Vector2(400, 200)), true, "").ShowUtility();
        }

        private void Awake()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.black;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.fontSize = 12;
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }
        }

        void OnEnable()
        {
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            if (CharacterModel)
            {
                if (!characterAdded)
                {
                    characterAdded = true;
                    CharacterModel = Instantiate(CharacterModel, Vector3.zero, Quaternion.Euler(Vector3.zero));
                }
                else
                {
                    if (CharacterModel.GetComponent<Animator>())
                    {
                        if (CharacterModel.GetComponent<Animator>().avatar)
                        {
                            avatar = CharacterModel.GetComponent<Animator>().avatar;
                        }
                        else
                        {
                            avatarError = true;
                        }

                        if (avatar)
                        {
                            if (!avatar.isHuman)
                            {
                                DestroyImmediate(CharacterModel);
                                CharacterModel = null;
                                characterError = true;
                                avatar = null;
                            }
                            else
                            {
                                characterError = false;
                            }
                        }
                    }
                    else
                    {
                        CharacterModel.AddComponent<Animator>();
                    }
                }

                if (startCreation & progress > 1.16f)
                {
                    AddScripts();
                    SetVariables();
                    CreateBodyColliders();
                    CreateCamera();

                    SaveCharacterToPrefab();
                    hasCreated = true;
                    startVal = (float) EditorApplication.timeSinceStartup;
                    avatarError = false;

                    startCreation = false;
                }
            }
            else
            {
                Ragdoll = null;
                characterAdded = false;
                avatarError = false;
            }

            if (hasCreated)
            {
                if (progress > 10)
                {
                    hasCreated = false;

                    CharacterModel = null;
                    Ragdoll = null;
                    avatar = null;
                }
            }
        }

        private void OnGUI()
        {
            style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.Space();
            GUILayout.Label("Create Character", labelStyle);
            EditorGUILayout.Space();
            if (hasCreated)
            {
                var labelStyle = new GUIStyle {normal = {textColor = Color.green}, fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter};
                EditorGUILayout.LabelField("Character has been created", labelStyle);
                EditorGUILayout.LabelField("To adjust the character open the <b>Adjustment scene</b> [Tools -> USK -> Adjust]", style);
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginVertical("HelpBox");
            if (characterError)
            {
                EditorGUILayout.HelpBox("Character model must be a Humanoid type.", MessageType.Warning);
            }

            CharacterModel = (GameObject) EditorGUILayout.ObjectField("Character Model", CharacterModel, typeof(GameObject), false);
            
            if (CharacterModel && avatarError)
            {
                EditorGUILayout.Space();
                if(!avatar)
                    EditorGUILayout.HelpBox("Your character model has no any Avatar, please add it manually.", MessageType.Info);
                avatar = (Avatar) EditorGUILayout.ObjectField("Avatar", avatar, typeof(Avatar), false);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (CharacterModel && avatar)
            {
                if (startCreation)
                {
                    if (progress < 0.3f)
                        EditorGUI.ProgressBar(new Rect(3, GUILayoutUtility.GetLastRect().max.y + 10, position.width - 6, 20), progress / 1, "Creation.");
                    else if (progress > 0.3f && progress < 0.6f)
                        EditorGUI.ProgressBar(new Rect(3, GUILayoutUtility.GetLastRect().max.y + 10, position.width - 6, 20), progress / 1, "Creation..");
                    else if (progress > 0.6f)
                        EditorGUI.ProgressBar(new Rect(3, GUILayoutUtility.GetLastRect().max.y + 10, position.width - 6, 20), progress / 1, "Creation...");
                }
            }


            EditorGUI.BeginDisabledGroup(!CharacterModel || !avatar);

            if (!startCreation)
                if (GUILayout.Button("Create"))
                {
                    startVal = (float) EditorApplication.timeSinceStartup;
                    startCreation = true;
                }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();

            progress = (float) (EditorApplication.timeSinceStartup - startVal);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void SetFieldValue(ScriptableWizard obj, string name, object value)
        {
            if (value == null)
            {
                return;
            }

            var field = obj.GetType().GetField(name);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        void SaveCharacterToPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Universal Shooter Kit/Prefabs/Characters/"))
            {
                Directory.CreateDirectory("Assets/Universal Shooter Kit/Prefabs/Characters/");
            }

            var name = CharacterModel.name;
            if (name.Contains("(Clone)"))
            {
                var replace = name.Replace("(Clone)", "");
                name = replace;
            }

            var index = 0;
            while(AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Prefabs/Characters/" + name + " " + index + ".prefab", typeof(GameObject)) != null)
            {
                index++;
            }

#if !UNITY_2018_3_OR_NEWER
            var prefab = PrefabUtility.CreateEmptyPrefab("Assets/Universal Shooter Kit/Prefabs/Characters/" + name + " " + (index > 0 ? "" + index : "") + ".prefab");
            PrefabUtility.ReplacePrefab(CharacterModel, prefab, ReplacePrefabOptions.ConnectToPrefab);
#else
            PrefabUtility.SaveAsPrefabAsset(CharacterModel, "Assets/Universal Shooter Kit/Prefabs/Characters/" + name + " " + (index > 0 ? "" + index : "") + ".prefab");
#endif

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Prefabs/Characters/" + name + " " + (index > 0 ? "" + index : "") + ".prefab", typeof(GameObject)));
            
            DestroyImmediate(CharacterModel);
        }

        void AddScripts()
        {
            var name = CharacterModel.name;
            
            // CharacterModel = Instantiate(CharacterModel, Vector3.zero, Quaternion.Euler(Vector3.zero));
            CharacterModel.SetActive(true);

            CharacterModel.GetComponent<Animator>().avatar = avatar;
            
            CharacterModel.name = name;
            
            if (!CharacterModel.GetComponent<Controller>())
                CharacterModel.AddComponent<Controller>();

            if (!CharacterModel.GetComponent<InventoryManager>())
                CharacterModel.AddComponent<InventoryManager>();

            var controller = CharacterModel.GetComponent<Controller>();
            var manager = CharacterModel.GetComponent<InventoryManager>();

            controller.BodyParts = new List<Transform> {null, null, null, null, null, null, null, null, null, null, null};

            controller.anim = CharacterModel.GetComponent<Animator>();

            if(!manager.bloodProjector)
                manager.bloodProjector = Resources.Load("Blood Projector", typeof(Projector)) as Projector;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/Blood Projector.prefab", typeof(Projector)) as Projector;

            if(!manager.trailMaterial)
                manager.trailMaterial = Resources.Load("Trail Mat", typeof(Material)) as Material;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/Trail Mat.mat", typeof(Material)) as Material;

            if (!controller.DirectionObject)
            {
                controller.DirectionObject = new GameObject("Direction object").transform;
                controller.DirectionObject.parent = CharacterModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Spine);
                controller.DirectionObject.localPosition = Vector3.zero;
                controller.DirectionObject.localEulerAngles = Vector3.zero;
                controller.DirectionObject.hideFlags = HideFlags.HideInHierarchy;
            }

            if (!controller.handDirectionObject)
            {
                controller.handDirectionObject = new GameObject("Hands Direction object").transform;
                controller.handDirectionObject.parent = CharacterModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
                controller.handDirectionObject.localPosition = Vector3.zero;
                controller.handDirectionObject.localEulerAngles = Vector3.zero;
                controller.handDirectionObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        void CreateCamera()
        {
            var controller = CharacterModel.GetComponent<Controller>();
            if (!controller.thisCamera)
            {
                var camera = new GameObject("MainCamera");
                var cameraComponent = camera.AddComponent<Camera>();
                camera.tag = "MainCamera";
                var cameraController = camera.AddComponent<CameraController>();

                cameraComponent.nearClipPlane = 0.01f;

                camera.transform.parent = CharacterModel.transform;
                camera.transform.localPosition = new Vector3(0, 0, 0);
                camera.AddComponent<AudioListener>();

                controller.thisCamera = camera;
                controller.CameraController = camera.GetComponent<CameraController>();
                camera.GetComponent<CameraController>().Controller = controller;

                var aimCamera = new GameObject("AimCamera") {tag = "MainCamera"};
                aimCamera.transform.parent = camera.transform;
                aimCamera.transform.localPosition = Vector3.zero;
                aimCamera.transform.localEulerAngles = Vector3.zero;
                cameraController.AimCamera = aimCamera.AddComponent<Camera>();
                cameraController.AimCamera.nearClipPlane = 0.01f;
                
                var layerCamera = Helper.NewCamera("Layer Camera", camera.transform, "CameraController");
                layerCamera.gameObject.SetActive(false);
                // layerCamera.hideFlags = HideFlags.HideInHierarchy;
                cameraController.layerCamera = layerCamera;

                cameraController.CameraPosition = Helper.NewObject(CharacterModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head), "FP Camera Position & Rotation");

                DestroyImmediate(cameraController.CameraPosition.GetComponent<BoxCollider>());
            }
        }

        void SetVariables()
        {
            var _animator = CharacterModel.GetComponent<Animator>();
            var controller = CharacterModel.GetComponent<Controller>();

            controller.characterAnimatorController = Resources.Load("Character", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;

            controller.BodyObjects.RightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            controller.BodyObjects.LeftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            controller.BodyObjects.Head = _animator.GetBoneTransform(HumanBodyBones.Head);
            controller.BodyObjects.TopBody = _animator.GetBoneTransform(HumanBodyBones.Spine);
            controller.BodyObjects.Hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            
            controller.FeetAudioSource = new GameObject("FeetAudio").AddComponent<AudioSource>();
            controller.FeetAudioSource.transform.parent = CharacterModel.transform;
            controller.FeetAudioSource.transform.localPosition = Vector3.zero;

            controller.projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
            
            controller.characterID = Helper.GenerateRandomString(20);
        }

        void CreateBodyColliders()
        {
            RagdollHelper.GetBones(bodyParameters, CharacterModel.GetComponent<Animator>());
            RagdollHelper.SetBodyParts(bodyParameters, CharacterModel.GetComponent<Controller>());

            RagdollHelper.CreateColliders(bodyParameters);
        }

       
    }
}
