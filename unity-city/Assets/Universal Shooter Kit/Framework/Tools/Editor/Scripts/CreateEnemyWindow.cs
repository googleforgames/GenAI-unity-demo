using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;

namespace GercStudio.USK.Scripts
{
    public class CreateEnemyWindow : EditorWindow
    {
        public GameObject Model;

        private bool enemyAdded;
        private bool hasCreated;
        private bool startCreation;
        
        private RagdollHelper.BodyParameters bodyParameters = new RagdollHelper.BodyParameters();

        private float startVal;
        private float progress;

        private Vector2 scrollPos;

        private GUIStyle labelStyle;
        private GUIStyle style;


        [MenuItem("Tools/Universal Shooter Kit/Create/Enemy")]
        public static void ShowWindow()
        {
            GetWindowWithRect(typeof(CreateEnemyWindow), new Rect(Vector2.zero, new Vector2(400, 400)), true, "").ShowUtility();
        }

        private void Awake()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle {normal = {textColor = Color.black}, fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter};
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
            if (Model)
            {
                if (!enemyAdded)
                {
                    enemyAdded = true;
                }
                else
                {
                    if (Model.GetComponent<Animator>())
                    {
                        if (!Model.GetComponent<Animator>().avatar)
                        {
                            DestroyImmediate(Model.GetComponent<Animator>());
                            Model.AddComponent<Animator>();

                        }
                    }
                    else
                    {
                        Model.AddComponent<Animator>();
                    }
                }

                if (startCreation & progress > 1.16f)
                {
                    AddScripts();
                    CreateBodyColliders();
                    SaveEnemyToPrefab();
                    hasCreated = true;
                    startVal = (float) EditorApplication.timeSinceStartup;

                    startCreation = false;
                }
            }
            else
            {
                enemyAdded = false;
            }

            if (hasCreated)
            {
                if (progress > 10)
                {
                    hasCreated = false;

                    Model = null;
                }
            }
        }

        private void OnGUI()
        {
            style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.Space();
            GUILayout.Label("Create Enemy", labelStyle);
            EditorGUILayout.Space();
            if (hasCreated)
            {
                var labelStyle = new GUIStyle {normal = {textColor = Color.green}, fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter};
                EditorGUILayout.LabelField("Enemy has been created", labelStyle);
                EditorGUILayout.LabelField("Add other parameters (animations, sounds, etc) to the <b>EnemyController</b> script.", style);
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginVertical("HelpBox");

            Model = (GameObject) EditorGUILayout.ObjectField("Enemy Model", Model, typeof(GameObject), false);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (Model)
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


            EditorGUI.BeginDisabledGroup(!Model);

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

        void SaveEnemyToPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Universal Shooter Kit/Prefabs/Enemies/"))
            {
                Directory.CreateDirectory("Assets/Universal Shooter Kit/Prefabs/Enemies/");
            }
            
            var name = Model.name;
            if (name.Contains("(Clone)"))
            {
                var replace = name.Replace("(Clone)", "");
                name = replace;
            }

            var index = 0;
            while (AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Prefabs/Enemies/" + name + " " + index + ".prefab", typeof(GameObject)) != null)
            {
                index++;
            }

#if !UNITY_2018_3_OR_NEWER
            var prefab = PrefabUtility.CreateEmptyPrefab("Assets/Universal Shooter Kit/Prefabs/Enemies/" + name + " " + (index > 0 ? "" + index : "") + ".prefab");
            PrefabUtility.ReplacePrefab(Model, prefab, ReplacePrefabOptions.ConnectToPrefab);
#else
            PrefabUtility.SaveAsPrefabAsset(Model, "Assets/Universal Shooter Kit/Prefabs/Enemies/" + name + " " + (index > 0 ? "" + index : "") + ".prefab");
#endif

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Prefabs/Enemies/" + name + " " + (index > 0 ? "" + index : "") + ".prefab", typeof(GameObject)));

            DestroyImmediate(Model);
        }

        void AddScripts()
        {
            var name = Model.name;

            Model = Instantiate(Model, Vector3.zero, Quaternion.Euler(Vector3.zero));
            Model.SetActive(true);

            Model.name = name;

            if (!Model.GetComponent<AIController>())
                Model.AddComponent<AIController>();

            var aiController = Model.GetComponent<AIController>();

            aiController.directionObject = new GameObject("Direction").transform;
            aiController.directionObject.parent = aiController.transform;
            aiController.directionObject.localPosition = Vector3.zero;
            aiController.bloodProjector = Resources.Load("Blood Projector", typeof(Projector)) as Projector;
            aiController.BodyParts = new List<Transform> {null, null, null, null, null, null, null, null, null, null, null};

            AIHelper.CreateStatsCanvas(aiController);
            AIHelper.CreateAttentionIndicator(aiController);
            AIHelper.CreateNewHealthBar(aiController);
            AIHelper.CreateNicknameText(aiController);
                
            aiController.bloodProjector = Resources.Load("Blood Projector", typeof(Projector)) as Projector;
            // aiController.trailMaterial = Resources.Load("Trail Mat", typeof(Material)) as Material;
            aiController.AnimatorController = Resources.Load("AI", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
        }

        void CreateBodyColliders()
        {
            if (Model.GetComponent<Animator>() && Model.GetComponent<Animator>().avatar && Model.GetComponent<Animator>().avatar.isHuman)
            {
                RagdollHelper.GetBones(bodyParameters, Model.GetComponent<Animator>());
                RagdollHelper.SetBodyParts(bodyParameters, Model.GetComponent<AIController>());

                RagdollHelper.CreateColliders(bodyParameters);
            }
        }
    }
}

