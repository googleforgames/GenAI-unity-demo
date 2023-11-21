using System;
using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(AIArea))]
    public class 
        AIAreaEditor : Editor
    {
        public AIArea script;

        private GUIStyle grayBackground;
        
        private ReorderableList spawnZonesList;
        private ReorderableList enemiesList;
        
        BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        public void Awake()
        {
            script = (AIArea) target;
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            NavMeshVisualizationSettings.showNavigation++;
            
            enemiesList = new ReorderableList(serializedObject, serializedObject.FindProperty("enemiesToSpawn"), false, true,
				true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 5, EditorGUIUtility.singleLineHeight), "Prefab");

					EditorGUI.LabelField(new Rect(rect.x + rect.width / 5 + 5, rect.y, rect.width / 5, EditorGUIUtility.singleLineHeight), new GUIContent("Behaviour°", "Movement behaviour in the current scene"));
					
					EditorGUI.LabelField(new Rect(rect.x + 2 * (rect.width / 5 + 5), rect.y, rect.width / 4 + 5, EditorGUIUtility.singleLineHeight), new GUIContent("Spawn Method°", "• Random - one random point from the Spawn Zones (below) will be chosen" + "\n\n" +
					                                                                                                                                                                           "• Specific point - set a spawn point for the enemy as you need"));
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 30, rect.y, rect.width / 30 + 10, EditorGUIUtility.singleLineHeight), new GUIContent("∞°", "Spawn enemies constantly"));
                    
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 25 + rect.width / 30 + 15, rect.y, rect.width / 12 + 10, EditorGUIUtility.singleLineHeight),new GUIContent("Count°", "• If [∞] is not active, this number means how many enemies will spawn during the game" + "\n\n" +
					                                                                                                                                                                          "• If [∞] is active, this number means the limit of enemies in the scene"));

					EditorGUI.LabelField(new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 20 + rect.width / 12 + rect.width / 12 + 10, rect.y,
						rect.width - (rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + rect.width / 12 + rect.width / 12 + 10)
						, EditorGUIUtility.singleLineHeight), new GUIContent("Time°", "A break between the appearance of enemies (in seconds)"));
				},

				onAddCallback = items => { script.enemiesToSpawn.Add(null); },

				onRemoveCallback = items => { script.enemiesToSpawn.Remove(script.enemiesToSpawn[items.index]); },


				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.enemiesToSpawn[index].aiPrefab = (AIController) EditorGUI.ObjectField(
						new Rect(rect.x, rect.y, rect.width / 5, EditorGUIUtility.singleLineHeight),
						script.enemiesToSpawn[index].aiPrefab, typeof(AIController), false);
					
					script.enemiesToSpawn[index].movementBehavior = (MovementBehavior) EditorGUI.ObjectField(
						new Rect(rect.x + rect.width / 5 + 5, rect.y, rect.width / 5, EditorGUIUtility.singleLineHeight),
						script.enemiesToSpawn[index].movementBehavior, typeof(MovementBehavior), true);


					if (script.enemiesToSpawn[index].currentSpawnMethodIndex == 0)
					{

						script.enemiesToSpawn[index].currentSpawnMethodIndex = EditorGUI.Popup(
							new Rect(rect.x + rect.width / 5 + 5 + rect.width / 5 + 5, rect.y, rect.width / 4 + 15,
								EditorGUIUtility.singleLineHeight), script.enemiesToSpawn[index].currentSpawnMethodIndex,
							new[] {"Random", "Specific Point"});
					}
					else
					{
						script.enemiesToSpawn[index].currentSpawnMethodIndex = EditorGUI.Popup(
							new Rect(rect.x + rect.width / 5 + 5 + rect.width / 5 + 5, rect.y, rect.width / 8 + 10,
								EditorGUIUtility.singleLineHeight), script.enemiesToSpawn[index].currentSpawnMethodIndex, new[] {"Random", "Specific Area"});

						script.enemiesToSpawn[index].spawnZone = (SpawnZone) EditorGUI.ObjectField(
							new Rect(rect.x + rect.width / 5 + 5 + rect.width / 5 + 5 + rect.width / 8 + 10 + 3, rect.y, rect.width / 8 + 5,
								EditorGUIUtility.singleLineHeight),
							script.enemiesToSpawn[index].spawnZone, typeof(SpawnZone), true);
					}
					
					script.enemiesToSpawn[index].spawnConstantly = EditorGUI.Toggle(
						new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 30, rect.y, rect.width / 30,
							EditorGUIUtility.singleLineHeight), script.enemiesToSpawn[index].spawnConstantly);
					
					script.enemiesToSpawn[index].count = EditorGUI.IntField(
						new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 30 + rect.width / 30 + 10, rect.y, rect.width / 12,
							EditorGUIUtility.singleLineHeight), script.enemiesToSpawn[index].count);

					script.enemiesToSpawn[index].spawnTimeout = EditorGUI.FloatField(
						new Rect(rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + 20 + rect.width / 12 + rect.width / 12 + 10, rect.y,
							rect.width - (rect.x + rect.width / 5 + rect.width / 5 + 5 + rect.width / 4 + rect.width / 12 + rect.width / 12 + 10)
							, EditorGUIUtility.singleLineHeight), script.enemiesToSpawn[index].spawnTimeout);
				},
			};
            
            spawnZonesList = new ReorderableList(serializedObject, serializedObject.FindProperty("spawnZones"), false, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Spawn Zones");
                },
				
                onAddCallback = items =>
                {
                    script.spawnZones.Add(null);
                },
			
                onRemoveCallback = items =>
                {
                    script.spawnZones.Remove(script.spawnZones[items.index]);
                },
				
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    
                    var backgroundColor = GUI.backgroundColor;

                    if (script.spawnZones[index])
                        GUI.backgroundColor = script.spawnZones[index].color;
						
                    script.spawnZones[index] = (SpawnZone) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.spawnZones[index], typeof(SpawnZone), true);

                    GUI.backgroundColor = backgroundColor;
                }
            };
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
            NavMeshVisualizationSettings.showNavigation--;
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                if (script.collectObjects != CollectObjects.Children)
                    script.collectObjects = CollectObjects.Children;
            }
        }
        
        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        public override void OnInspectorGUI()
        {

            Helper.InitStyles(ref grayBackground, new Color32(160, 160, 160, 200));

            serializedObject.Update();

            EditorGUILayout.Space();

            script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Area Parameters", "Spawn Enemies"});
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            switch (script.inspectorTab)
            {
                case 0:
                    
                    EditorGUILayout.LabelField("Nav Mesh Surface", EditorStyles.boldLabel);

                    EditorGUILayout.BeginVertical("helpbox");
                    EditorGUILayout.BeginVertical("HelpBox");

                    var bs = NavMesh.GetSettingsByID(serializedObject.FindProperty("m_AgentTypeID").intValue);
                    
                    // var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
                    // var layerProps = tagManager.FindProperty("m_Settings");
                    // var propCount = layerProps.arraySize;
                    // Debug.Log(layerProps.);

                    if (bs.agentTypeID != -1)
                    {
                        // Draw image
                        const float diagramHeight = 80.0f;
                        Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                        NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
                    }
                    
                    NavMeshComponentsGUIUtility.AgentTypePopup("Baked Agent Type", serializedObject.FindProperty("m_AgentTypeID"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginVertical("HelpBox");
                    NavMeshComponentsGUIUtility.AreaPopup( "Area Type°", serializedObject.FindProperty("m_DefaultArea"));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();


                    var backgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = script.navMeshData ? Helper.GetAreaColor(script.defaultArea) : backgroundColor;
                    

                    EditorGUILayout.BeginVertical("HelpBox");
                    
                    var nmdRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

                    EditorGUI.BeginProperty(nmdRect, GUIContent.none, serializedObject.FindProperty("m_NavMeshData"));
                    
                    var rectLabel = EditorGUI.PrefixLabel(nmdRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(serializedObject.FindProperty("m_NavMeshData").displayName));
                    EditorGUI.EndProperty();

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.BeginProperty(nmdRect, GUIContent.none, serializedObject.FindProperty("m_NavMeshData"));
                        EditorGUI.ObjectField(rectLabel, serializedObject.FindProperty("m_NavMeshData"), GUIContent.none);
                        EditorGUI.EndProperty();
                    }

                    // EditorGUILayout.Space();
                    
                    GUI.backgroundColor = backgroundColor;
                    
                    var hadError = false;
                    var multipleTargets = targets.Length > 1;
                    foreach (NavMeshSurface navSurface in targets)
                    {
                        var settings = navSurface.GetBuildSettings();
                        // Calculating bounds is potentially expensive when unbounded - so here we just use the center/size.
                        // It means the validation is not checking vertical voxel limit correctly when the surface is set to something else than "in volume".
                        var bounds = new Bounds(Vector3.zero, Vector3.zero);
                        if (navSurface.collectObjects == CollectObjects.Volume)
                        {
                            bounds = new Bounds(navSurface.center, navSurface.size);
                        }

                        var errors = settings.ValidationReport(bounds);
                        if (errors.Length > 0)
                        {
                            if (multipleTargets)
                                EditorGUILayout.LabelField(navSurface.name);
                            foreach (var err in errors)
                            {
                                EditorGUILayout.HelpBox(err, MessageType.Warning);
                            }

                            GUILayout.BeginHorizontal();
                            GUILayout.Space(EditorGUIUtility.labelWidth);
                            if (GUILayout.Button("Open Agent Settings...", EditorStyles.miniButton))
                                NavMeshEditorHelpers.OpenAgentSettings(navSurface.agentTypeID);
                            GUILayout.EndHorizontal();
                            hadError = true;
                        }
                    }

                    if (hadError)
                        EditorGUILayout.Space();

                    using (new EditorGUI.DisabledScope(Application.isPlaying || serializedObject.FindProperty("m_AgentTypeID").intValue == -1))
                    {
                        GUILayout.BeginHorizontal();
                        // GUILayout.Space(EditorGUIUtility.labelWidth);
                        if (GUILayout.Button("Clear"))
                        {
                            NavMeshAssetManager.instance.ClearSurfaces(targets);
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Bake"))
                        {
                            NavMeshAssetManager.instance.StartBakingSurfaces(targets);
                        }

                        GUILayout.EndHorizontal();
                    }

                    // Show progress for the selected targets
                    var bakeOperations = NavMeshAssetManager.instance.GetBakeOperations();
                    for (int i = bakeOperations.Count - 1; i >= 0; --i)
                    {
                        if (!targets.Contains(bakeOperations[i].surface))
                            continue;

                        var oper = bakeOperations[i].bakeOperation;
                        if (oper == null)
                            continue;

                        var p = oper.progress;
                        if (oper.isDone)
                        {
                            SceneView.RepaintAll();
                            continue;
                        }

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                        {
                            var bakeData = bakeOperations[i].bakeData;
                            UnityEngine.AI.NavMeshBuilder.Cancel(bakeData);
                            bakeOperations.RemoveAt(i);
                        }

                        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p, "Baking: " + (int) (100 * p) + "%");
                        if (p <= 1)
                            Repaint();

                        GUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("AI Settings", EditorStyles.boldLabel);

                    EditorGUILayout.BeginVertical("helpbox");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("communicationBetweenAIs"), 
                        new GUIContent("Communication°", "• [Communicate With Each Other] - if an enemy notice the player, he will notify all AIs in the area and they will find and attack together." + "\n\n" +
                                                                                                                                            "• [Independent Opponents] - only the enemy who noticed the player will attack him, if other AIs notice the player later, they will join the attack."));
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("disableAttackStateTime"), new GUIContent("Disable Attack State Time°", "How quickly will opponents stop attacking players if they don't see or hear them."));

                    
                    EditorGUILayout.EndVertical();

                    // EditorGUILayout.EndVertical();

                    break;
                case 1:
                    
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("You can place AI opponents in the scene, or point them here so that they appear during the game.", MessageType.Info);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    enemiesList.DoLayoutList();
                    EditorGUILayout.Space();
                    spawnZonesList.DoLayoutList();
                    EditorGUILayout.Space();
                    break;
            }
            
            
            EditorGUILayout.Space();
            
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();
            // EditorGUILayout.Space();

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
