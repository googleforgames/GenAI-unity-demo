using System.Linq;
#if USK_MULTIPLAYER
using Photon.Pun;
#endif
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AIController))]
	public class AIControllerEditor : Editor
	{
		private AIController script;

		private ReorderableList damageAnimations;
		private ReorderableList findAnimations;
		private ReorderableList attackAnimations;
		private ReorderableList attackPoints;
		private ReorderableList damageColliders;
		private ReorderableList bloodHoles;
		private ReorderableList additionalEffects;
		private ReorderableList spawnedItems;
		private ReorderableList genericColliders;
		private ReorderableList damageSounds;

		private GUIStyle style;
		// private GUIStyle grayBackground;

		private bool deleteMultiplayerScripts;
		private bool isSceneObject;
		private bool isSceneObjectForText;

		private void Awake()
		{
			script = (AIController) target;
		}

		private void OnEnable()
		{
			if (!script) return;

			var allObjectsOnScene = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
			isSceneObject = script.gameObject.scene.name == script.gameObject.name || allObjectsOnScene.Contains(script.gameObject);
			isSceneObjectForText = allObjectsOnScene.Contains(script.gameObject);

			findAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("FindAnimations"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Look Around", EditorStyles.boldLabel); },

				onAddCallback = items => { script.FindAnimations.Add(null); },

				onRemoveCallback = items =>
				{
					script.FindAnimations.Remove(script.FindAnimations[items.index]);
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.FindAnimations[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						script.FindAnimations[index], typeof(AnimationClip), false);
				}
			};
			damageAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("DamageAnimations"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Damage Reactions", EditorStyles.boldLabel); },

				onAddCallback = items => { script.DamageAnimations.Add(null); },

				onRemoveCallback = items =>
				{
					script.DamageAnimations.Remove(script.DamageAnimations[items.index]);
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.DamageAnimations[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						script.DamageAnimations[index], typeof(AnimationClip), false);
				}
			};
			attackAnimations = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(0).FindPropertyRelative("MeleeAttackAnimations"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Animations"); },

				onAddCallback = items => { script.Attacks[0].MeleeAttackAnimations.Add(null); },

				onRemoveCallback = items =>
				{
					if (script.Attacks[0].MeleeAttackAnimations.Count == 1)
						return;

					script.Attacks[0].MeleeAttackAnimations.Remove(script.Attacks[0].MeleeAttackAnimations[items.index]);
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.Attacks[0].MeleeAttackAnimations[index] = (AnimationClip) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						script.Attacks[0].MeleeAttackAnimations[index], typeof(AnimationClip), false);
				}
			};

			bloodHoles = new ReorderableList(serializedObject, serializedObject.FindProperty("BloodHoles"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Blood Holes", EditorStyles.boldLabel); },

				onAddCallback = items => { script.BloodHoles.Add(null); },

				onRemoveCallback = items =>
				{
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
			
			spawnedItems = new ReorderableList(serializedObject, serializedObject.FindProperty("itemsAppearingAfterDeath"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Items Appearing After Death°", "• If you add scene objects, they will become active after the enemy death." + "\n" +
				                                                                                                                                                                            "• If you add prefabs, one of them will be instantiated after death."), EditorStyles.boldLabel); },

				onAddCallback = items => { script.itemsAppearingAfterDeath.Add(null); },

				onRemoveCallback = items =>
				{
					script.itemsAppearingAfterDeath.Remove(script.itemsAppearingAfterDeath[items.index]);
				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.itemsAppearingAfterDeath[index] = (GameObject) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.itemsAppearingAfterDeath[index], typeof(GameObject), true);
				}
			};

			genericColliders = new ReorderableList(serializedObject, serializedObject.FindProperty("genericColliders"), false, true, true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), "Body Colliders");
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight), "Multipliers");
				},

				onAddCallback = items => { script.genericColliders.Add(new AIHelper.GenericCollider()); },

				onRemoveCallback = items => { script.genericColliders.Remove(script.genericColliders[items.index]); },

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.genericColliders[index].collider = (Collider) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 1.5f, EditorGUIUtility.singleLineHeight), script.genericColliders[index].collider, typeof(Collider), true);
					script.genericColliders[index].damageMultiplier = EditorGUI.FloatField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - rect.width / 1.5f - 10, EditorGUIUtility.singleLineHeight), script.genericColliders[index].damageMultiplier);
				}
			};

			attackPoints = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(0).FindPropertyRelative("AttackSpawnPoints"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Attack Points"); },

				onAddCallback = items =>
				{
					if (!script.gameObject.activeInHierarchy)
					{
						var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

						var attackPoint = new GameObject("Attack Point");

						attackPoint.transform.parent = tempEnemy.transform;
						attackPoint.transform.localPosition = Vector3.zero;
						tempEnemy.GetComponent<AIController>().Attacks[0].AttackSpawnPoints.Add(attackPoint.transform);

#if !UNITY_2018_3_OR_NEWER
						PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
						PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

						DestroyImmediate(tempEnemy);
					}
					else
					{
						var attackPoint = new GameObject("Attack Point");
						attackPoint.transform.parent = script.transform;
						attackPoint.transform.localPosition = Vector3.zero;
						script.Attacks[0].AttackSpawnPoints.Add(attackPoint.transform);
					}
				},

				onRemoveCallback = items =>
				{
					if (script.Attacks[0].AttackSpawnPoints.Count == 1)
						return;

					if (!script.gameObject.activeInHierarchy)
					{
						var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

						if (tempEnemy.GetComponent<AIController>().Attacks[0].AttackSpawnPoints[items.index])
							DestroyImmediate(tempEnemy.GetComponent<AIController>().Attacks[0].AttackSpawnPoints[items.index].gameObject);

						tempEnemy.GetComponent<AIController>().Attacks[0].AttackSpawnPoints.Remove(tempEnemy.GetComponent<AIController>().Attacks[0].AttackSpawnPoints[items.index]);

#if !UNITY_2018_3_OR_NEWER
						PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
						PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

						DestroyImmediate(tempEnemy);
					}
					else
					{
						if (script.Attacks[0].AttackSpawnPoints[items.index])
							DestroyImmediate(script.Attacks[0].AttackSpawnPoints[items.index].gameObject);
						script.Attacks[0].AttackSpawnPoints.Remove(script.Attacks[0].AttackSpawnPoints[items.index]);
					}

				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.Attacks[0].AttackSpawnPoints[index] = (Transform) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						script.Attacks[0].AttackSpawnPoints[index], typeof(Transform), true);
				}
			};

			damageColliders = new ReorderableList(serializedObject, serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(0).FindPropertyRelative("DamageColliders"), false, true, true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Damage Colliders"); },

				onAddCallback = items =>
				{
					if (!script.gameObject.activeInHierarchy)
					{
						var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

						var collider = new GameObject("Damage Collider");
						collider.transform.parent = tempEnemy.transform;
						collider.transform.localPosition = Vector3.zero;
						collider.tag = script.Attacks[0].AttackType == AIHelper.AttackTypes.Fire ? "Fire" : "Melee Collider";
						tempEnemy.GetComponent<AIController>().Attacks[0].DamageColliders.Add(collider.AddComponent<BoxCollider>());

#if !UNITY_2018_3_OR_NEWER
						PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
						PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

						DestroyImmediate(tempEnemy);
					}
					else
					{
						var collider = new GameObject("Damage Collider");
						collider.transform.parent = script.transform;
						collider.transform.localPosition = Vector3.zero;
						collider.tag = script.Attacks[0].AttackType == AIHelper.AttackTypes.Fire ? "Fire" : "Melee Collider";
						script.Attacks[0].DamageColliders.Add(collider.AddComponent<BoxCollider>());
					}
				},

				onRemoveCallback = items =>
				{
					if (script.Attacks[0].DamageColliders.Count == 1)
						return;

					if (!script.gameObject.activeInHierarchy)
					{
						var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

						if (tempEnemy.GetComponent<AIController>().Attacks[0].DamageColliders[items.index])
							DestroyImmediate(tempEnemy.GetComponent<AIController>().Attacks[0].DamageColliders[items.index].gameObject);
						tempEnemy.GetComponent<AIController>().Attacks[0].DamageColliders.Remove(tempEnemy.GetComponent<AIController>().Attacks[0].DamageColliders[items.index]);

#if !UNITY_2018_3_OR_NEWER
						PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
						PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

						DestroyImmediate(tempEnemy);
					}
					else
					{
						if (script.Attacks[0].DamageColliders[items.index])
							DestroyImmediate(script.Attacks[0].DamageColliders[items.index].gameObject);

						script.Attacks[0].DamageColliders.Remove(script.Attacks[0].DamageColliders[items.index]);
					}

				},

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.Attacks[0].DamageColliders[index] = (BoxCollider) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
						script.Attacks[0].DamageColliders[index], typeof(BoxCollider), true);
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
			if (Application.isPlaying || !script) return;

			if(string.IsNullOrEmpty(script.enemyID))
				script.enemyID = Helper.GenerateRandomString(20);
			
			if (deleteMultiplayerScripts)
			{
				deleteMultiplayerScripts = false;

#if USK_MULTIPLAYER
				AIHelper.RemoveMultiplayerScripts(script.gameObject);
#endif
			}

			if (script.centralHorizontalAngle > script.peripheralHorizontalAngle)
				script.centralHorizontalAngle = script.peripheralHorizontalAngle - 1;

#if USK_MULTIPLAYER
			if (!script.photonAnimatorView && script.gameObject.GetComponent<PhotonAnimatorView>())
				script.photonAnimatorView = script.gameObject.GetComponent<PhotonAnimatorView>();
#endif
			if (!script.bloodProjector)
				script.bloodProjector = Resources.Load("Blood Projector", typeof(Projector)) as Projector;

			if (!script.projectSettings)
			{
				script.projectSettings = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
				EditorUtility.SetDirty(script.gameObject);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			if (!script.directionObject && !isSceneObject)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

				tempEnemy.GetComponent<AIController>().directionObject = new GameObject("Direction").transform;
				tempEnemy.GetComponent<AIController>().directionObject.parent = tempEnemy.transform;
				tempEnemy.GetComponent<AIController>().directionObject.localPosition = Vector3.zero;

#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

				DestroyImmediate(tempEnemy);
			}
			else if (!script.directionObject && isSceneObject) //script.gameObject.activeInHierarchy && script.gameObject.activeSelf)
			{
				script.directionObject = new GameObject("Direction").transform;
				script.directionObject.parent = script.transform;
				script.directionObject.localPosition = Vector3.zero;
			}


			if (!script.FeetAudioSource && !isSceneObject) //script.gameObject.activeSelf && !script.gameObject.activeInHierarchy)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

				var controller = tempEnemy.GetComponent<AIController>();

				controller.FeetAudioSource = new GameObject("FeetAudio").AddComponent<AudioSource>();
				controller.FeetAudioSource.transform.parent = tempEnemy.transform;
				controller.FeetAudioSource.spatialBlend = 1;
				controller.FeetAudioSource.maxDistance = 100;
				controller.FeetAudioSource.minDistance = 1;
				controller.FeetAudioSource.transform.localPosition = Vector3.zero;

#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

				DestroyImmediate(tempEnemy);
			}
			else if (!script.FeetAudioSource && isSceneObject) //script.gameObject.activeSelf && script.gameObject.activeInHierarchy)
			{
				script.FeetAudioSource = new GameObject("FeetAudio").AddComponent<AudioSource>();
				script.FeetAudioSource.transform.parent = script.transform;
				script.FeetAudioSource.transform.localPosition = Vector3.zero;
				script.FeetAudioSource.spatialBlend = 1;
				script.FeetAudioSource.maxDistance = 100;
				script.FeetAudioSource.minDistance = 1;
			}

			if (!script.statsCanvas && !isSceneObject)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
				var enemyScript = tempEnemy.GetComponent<AIController>();

				AIHelper.CreateStatsCanvas(enemyScript);

#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif
				DestroyImmediate(tempEnemy);
			}
			else if (!script.statsCanvas && isSceneObject)
			{
				AIHelper.CreateStatsCanvas(script);
			}

			if (!script.attentionStatusMainObject && script.UseStates && !isSceneObject)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);

				var enemyScript = tempEnemy.GetComponent<AIController>();

				AIHelper.CreateAttentionIndicator(enemyScript);


#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif
				DestroyImmediate(tempEnemy);
			}
			else if (!script.attentionStatusMainObject && script.UseStates && isSceneObject) //script.gameObject.activeSelf && script.gameObject.activeInHierarchy)
			{
				AIHelper.CreateAttentionIndicator(script);
			}

			if (!script.healthBarMainObject && !isSceneObject) //script.gameObject.activeSelf && !script.gameObject.activeInHierarchy)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
				var enemyScript = tempEnemy.GetComponent<AIController>();
				AIHelper.CreateNewHealthBar(enemyScript);


#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif
				DestroyImmediate(tempEnemy);
			}
			else if (!script.healthBarMainObject && isSceneObject) //&& script.gameObject.activeInHierarchy &&  script.gameObject.activeSelf )
			{
				AIHelper.CreateNewHealthBar(script);
			}

			if (!script.nameText && !isSceneObject)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
				var enemyScript = tempEnemy.GetComponent<AIController>();
				AIHelper.CreateNicknameText(enemyScript);


#if !UNITY_2018_3_OR_NEWER
					PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif
				DestroyImmediate(tempEnemy);
			}
			else if (!script.nameText && isSceneObject)
			{
				AIHelper.CreateNicknameText(script);
			}

			if (script.attentionStatusMainObject)
			{
				script.attentionStatusMainObject.gameObject.SetActive(script.UseStates);
			}

			// if (!script.trailMaterial)
			// 	script.trailMaterial = Resources.Load("Trail Mat", typeof(Material)) as Material; //AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/Trail Mat.mat", typeof(Material)) as Material;


			if (!script.AnimatorController)
			{
				script.AnimatorController = Resources.Load("AI", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController; //AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/Assets/_Animator Controllers/AI.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
			}

			if (!script.anim)
			{
				script.anim = script.GetComponent<Animator>();
			}
			else
			{
				if (!script.anim.runtimeAnimatorController)
					script.anim.runtimeAnimatorController = script.AnimatorController;

				if (script.anim.avatar && script.anim.avatar.isHuman)
				{
					script.isHuman = true;
				}
				else
				{
					script.isHuman = false;
				}

				if (script.gameObject.activeInHierarchy)
				{
					script.newController = new AnimatorOverrideController(script.anim.runtimeAnimatorController);
					
					// if (!script.anim.runtimeAnimatorController)
						script.anim.runtimeAnimatorController = script.newController;

					if (script.topInspectorTab != 2)
					{
						script.ClipOverrides = new Helper.AnimationClipOverrides(script.newController.overridesCount);
						script.newController.GetOverrides(script.ClipOverrides);

						if (script.Attacks[0].HandsIdleAnimation)
							script.ClipOverrides["_EnemyHandsIdle"] = script.Attacks[0].HandsIdleAnimation;

						if (script.IdleAnimation)
							script.ClipOverrides["_EnemyIdle"] = script.IdleAnimation;

						script.newController.ApplyOverrides(script.ClipOverrides);

						if (script.anim.avatar && script.anim.avatar.isHuman && script.Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
						{
							script.anim.SetLayerWeight(1, 1);

							if (script.Attacks[0].HandsIdleAnimation)
								script.anim.Play("Idle", 1);
						}
						else if (script.anim.avatar && !script.anim.avatar.isHuman || script.Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
						{
							script.anim.SetLayerWeight(1, 0);
						}
						
						if (script.IdleAnimation)
							script.anim.Play("Idle", 0);

						if (script.Attacks[0].HandsIdleAnimation || script.IdleAnimation)
							script.anim.Update(Time.deltaTime);
					}
					else
					{
						script.anim.Play("T-Pose", 0);
						script.anim.SetLayerWeight(1, 0);
						script.anim.Update(Time.deltaTime);
					}
				}
			}


			foreach (var collider in script.Attacks[0].DamageColliders.Where(collider => collider))
			{
				collider.gameObject.SetActive(script.currentInspectorTab == 1);
			}
#if USK_MULTIPLAYER
			if (script.photonAnimatorView)
			{
				var value = script.photonAnimatorView.GetSynchronizedParameters().Find(parameter => parameter.Name == "SpeedOffset");
			
				if (value != null && value.SynchronizeType == PhotonAnimatorView.SynchronizeType.Disabled)
				{
					AIHelper.SetAnimatorParameters(script.photonAnimatorView);
				}
			}
#endif
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			var curAttackSerialized = serializedObject.FindProperty("Attacks").GetArrayElementAtIndex(0);

			style = new GUIStyle{richText = true, fontSize = 11, alignment = TextAnchor.MiddleCenter};

			EditorGUILayout.Space();

			
			var backgroundColor = GUI.backgroundColor;
#if USK_MULTIPLAYER
			GUI.backgroundColor = script.photonAnimatorView ? new Color(0,1,0,0.5f): new Color(1, 0,0, 0.3f);
            
			EditorGUILayout.BeginVertical("HelpBox");
			
			if(!script.photonAnimatorView)
				EditorGUILayout.Space();
			
			EditorGUILayout.LabelField(!script.photonAnimatorView ? "<b>Not ready for multiplayer</b> " + "\n" + 
			                                                        "(change it in the [Other Parameters] tab)" : "<b>Ready for multiplayer</b>", style);
            
			if(!script.photonAnimatorView)
				EditorGUILayout.Space();
			
			EditorGUILayout.EndVertical();
			GUI.backgroundColor = backgroundColor;
            
			EditorGUILayout.Space();
#endif
			
// #if !USK_MULTIPLAYER
// 			EditorGUILayout.LabelField(script.isHuman ? "<b><color=green>Humanoid Avatar</color></b>" : "<b><color=blue>Generic Avatar</color></b>", style);
// #else
// 			var avatarText = script.isHuman ? "<b><color=green>Humanoid Avatar</color></b>" : "<b><color=blue>Generic Avatar</color></b>";
// 			var multText = script.photonTransform ? "<b><color=green>Ready for multiplayer</color></b>" : "<b><color=red>Not ready for multiplayer</color></b>";
// 			EditorGUILayout.LabelField(avatarText + " <b>|</b> " + multText, style);
// #endif
			EditorGUILayout.Space();

			// if (script.gameObject.activeInHierarchy)
			// {
			// 	EditorGUILayout.HelpBox("Set a Waypoints Behaviour in the [Movement & Behaviour] -> [Behaviour] tab.", MessageType.Info);
			// 	EditorGUILayout.Space();
			// }

			if (script.isHuman && script.BodyParts.Count > 0 && (!script.BodyParts[0] || script.BodyParts[0] && !script.BodyParts[0].GetComponent<Rigidbody>()))
			{
				EditorGUILayout.HelpBox("Generate Body Colliders in the [Health] tab.", MessageType.Info);
				EditorGUILayout.Space();
			}

//			EditorGUILayout.BeginVertical(yellowBackground);

			script.topInspectorTab = GUILayout.Toolbar(script.topInspectorTab, new[] {"Behavior", "Attack", "Health"});

			switch (script.topInspectorTab)
			{
				case 0:
					script.currentInspectorTab = 0;
					script.bottomInspectorTab = 3;
					break;

				case 1:
					script.currentInspectorTab = 1;
					script.bottomInspectorTab = 3;
					break;

				case 2:
					script.currentInspectorTab = 2;
					script.bottomInspectorTab = 3;
					break;
			}

			script.bottomInspectorTab = GUILayout.Toolbar(script.bottomInspectorTab, new[] {"Movement", "Animations", "Other Parameters"});

			switch (script.bottomInspectorTab)
			{
				case 0:
					script.currentInspectorTab = 3;
					script.topInspectorTab = 3;
					break;

				case 1:
					script.currentInspectorTab = 4;
					script.topInspectorTab = 3;
					break;

				case 2:
					script.currentInspectorTab = 5;
					script.topInspectorTab = 3;
					break;
			}

//			EditorGUILayout.EndVertical();


			switch (script.currentInspectorTab)
			{
				case 0:

					EditorGUILayout.Space();

					script.currentBehaviourInspectorTab = GUILayout.Toolbar(script.currentBehaviourInspectorTab, new[] {"Detection Parameters", "In-game Behavior"});
					//
					// if (script.currentMovementInspectorTab > 1)
					// 	script.currentMovementInspectorTab = 1;

					EditorGUILayout.Space();

					// EditorGUILayout.BeginVertical(grayBackground);

					switch (script.currentBehaviourInspectorTab)
					{
						case 0:
							
							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("opponentsDetectionType"), new GUIContent("Detection Type"));
							EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
							EditorGUILayout.Space();
							
							if (script.opponentsDetectionType == AIHelper.OpponentsDetectionType.Vision || script.opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
							{
								EditorGUILayout.LabelField(new GUIContent("Vision"), EditorStyles.boldLabel);

								EditorGUILayout.BeginVertical("helpbox");
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.PropertyField(serializedObject.FindProperty("directionObject"), new GUIContent("Direction Object°", "Move and rotate this object so that it looks forward and located on the enemy's head."));
								EditorGUI.EndDisabledGroup();
								EditorGUILayout.EndVertical();

								EditorGUILayout.Space();

								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("visionDetectionTime"), new GUIContent("Detection Time"));
								EditorGUILayout.Space();

								EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceToSee"), new GUIContent("Distance"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("heightToSee"), new GUIContent("Height"));

								backgroundColor = GUI.backgroundColor;

								if (script.UseStates)
								{

									GUI.backgroundColor = new Color(0, 1, 0, 0.5f);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("centralHorizontalAngle"), new GUIContent("Central Vision Angle°", "When a character falls into this area, the enemy will immediately attack him"));
									EditorGUILayout.EndVertical();
									GUI.backgroundColor = backgroundColor;
								}

								GUI.backgroundColor = script.UseStates ? new Color(1, 0.7f, 0, 0.5f) : new Color(0, 1, 0, 0.5f);
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("peripheralHorizontalAngle"), new GUIContent(script.UseStates ? "Peripheral Vision Angle°" : "Vision Angle°", "When a character falls into this area, the enemy will not immediately notice him"));
								EditorGUILayout.EndVertical();
								GUI.backgroundColor = backgroundColor;

								EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
								EditorGUILayout.Space();
							}
							
							if (script.opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing || script.opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
							{
								EditorGUILayout.LabelField(new GUIContent("Hearing°", "All characters have a noise radius, if the enemy is in that area and there are no walls between them, then he hears the character."), EditorStyles.boldLabel);

								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("hearingDetectionTime"), new GUIContent("Detection Time"));
								EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
								EditorGUILayout.Space();

							}

							if (script.opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || script.opponentsDetectionType == AIHelper.OpponentsDetectionType.All)
							{
								EditorGUILayout.LabelField(new GUIContent("Close Range"), EditorStyles.boldLabel);
								
								
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("rangeDetectionTime"), new GUIContent("Detection Time"));
								
								backgroundColor = GUI.backgroundColor;
								GUI.backgroundColor = new Color(0, 1, 1, 0.5f);
								
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionDistance"), new GUIContent("Detection Distance"));
								EditorGUILayout.EndVertical();
								
								GUI.backgroundColor = backgroundColor;
								
								EditorGUILayout.EndVertical();
							}

						
							break;

						case 1:

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("observer"), new GUIContent("Observer°", "The enemy moves along the waypoints and watches the scene. If he sees the player, he will inform other opponents in the area and will attack the player (but he won't come close to the player)" + "\n\n" + "This behaviour is good for distant opponents (e.g. with Sniper Rifles)"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();
							
							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("UseStates"), new GUIContent("Warning State°", script.UseStates
								? "[Enabled]" + "\n\n" +
								  "• If the enemy sees (with peripheral vision) or hears a player for a while or the player has shot him a few times, " +
								  "the enemy's warning state is activated and he will look for him. " + "\n\n" +
								  "• If the enemy has found the player the attack state will be activated." + "\n\n" +
								  "• If the enemy doesn't see or hear the player, he looks for him for а while again." +
								  " And after that returns to waypoints." + "\n\n" +
								  "• If the enemy saw the player with central vision, he will attack immediately."
								: "[Disabled]" + "\n\n" +
								  "• If the enemy sees or hears the player, he immediately attacks him." + "\n\n" +
								  "• If the enemy doesn't see or hear a player, he immediately returns to waypoints."));

							if (script.UseStates)
							{
								EditorGUILayout.BeginVertical("helpbox");
								// EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.PropertyField(serializedObject.FindProperty("attentionStatusMainObject"), new GUIContent("UI Main Object"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("yellowImg"), new GUIContent("Warning State Fill"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("redImg"), new GUIContent("Attack State Fill"));
								// EditorGUI.EndDisabledGroup();
								EditorGUILayout.EndVertical();
							}

							EditorGUILayout.EndVertical();
							
							EditorGUILayout.Space();
							
							EditorGUILayout.BeginVertical("helpbox");
							EditorGUI.BeginDisabledGroup(script.Attacks[0].AttackType == AIHelper.AttackTypes.Melee || script.Attacks[0].AttackType == AIHelper.AttackTypes.Fire);
							
							if(script.Attacks[0].AttackType != AIHelper.AttackTypes.Melee && script.Attacks[0].AttackType != AIHelper.AttackTypes.Fire)
								EditorGUILayout.PropertyField(serializedObject.FindProperty("useCovers"), new GUIContent("Use Covers°", "If the enemy finds suitable cover (close enough to yourself and to the player), he will hide behind it"));
							else 							
								EditorGUILayout.PropertyField(serializedObject.FindProperty("useCovers"), new GUIContent("Use Covers°", "Covers can not be used if the attack type is [Fire] or [Melee]"));

							if (script.useCovers  && !HasAllSidesMovementAnimations())
								EditorGUILayout.HelpBox("Add all-directions movement animations in the [Animations] tab.", MessageType.Warning);

							EditorGUI.EndDisabledGroup();
							EditorGUILayout.EndVertical();
							break;
					}
					

					// EditorGUILayout.EndVertical();

					break;


				case 1:

					EditorGUILayout.Space();

					// script.attackInspectorTab = GUILayout.Toolbar(script.attackInspectorTab, new[] {"Attack Area", "Attack Parameters"});

					EditorGUILayout.Space();
					
					
					backgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = new Color(1, 1, 0, 0.5f);

					if(script.opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || script.opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing)
						EditorGUILayout.LabelField(new GUIContent("Attack Area"), EditorStyles.boldLabel);

					EditorGUILayout.BeginVertical("helpbox");
					
					if(script.opponentsDetectionType != AIHelper.OpponentsDetectionType.CloseRange && script.opponentsDetectionType != AIHelper.OpponentsDetectionType.Hearing)
						EditorGUILayout.PropertyField(serializedObject.FindProperty("attackDistancePercent"), new GUIContent("Attack Distance°"));
					else
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("peripheralHorizontalAngle"), new GUIContent("Angle"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceToSee"), new GUIContent("Distance"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("heightToSee"), new GUIContent("Height"));
						// EditorGUILayout.PropertyField(serializedObject.FindProperty("attackDistance"), new GUIContent("Attack Distance°"));
					}
					
					EditorGUILayout.EndVertical();

					GUI.backgroundColor = backgroundColor;
					
					if(script.opponentsDetectionType == AIHelper.OpponentsDetectionType.CloseRange || script.opponentsDetectionType == AIHelper.OpponentsDetectionType.Hearing)
						EditorGUILayout.Space();

					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					script.Attacks[0].AttackType = (AIHelper.AttackTypes) EditorGUILayout.EnumPopup("Attack Type", script.Attacks[0].AttackType);
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					switch (script.Attacks[0].AttackType)
					{
						case AIHelper.AttackTypes.Bullets:
							// EditorGUILayout.Space();
							// EditorGUILayout.Space();

							attackPoints.DoLayoutList();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("weapon"), new GUIContent("Weapon° (optional)"));

							if (script.weapon)
							{
								EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteWeaponAfterDeath"), new GUIContent("Delete Weapon After Death°", "If active, the weapon will be immediately deleted after the enemy dies"));
							}
							
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("muzzleFlash"), new GUIContent("Muzzle Flash"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");

							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("shootingMethod"), new GUIContent("Shooting Method"));

							if (script.Attacks[0].shootingMethod == WeaponsHelper.ShootingMethod.InstantiateBullet)
							{
								EditorGUILayout.BeginVertical("HelpBox");
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bullet"), new GUIContent("Bullet Prefab"));
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
								EditorGUILayout.EndVertical();
							}
							else
							{
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("bulletTrail"), new GUIContent("Bullet Trail"));
							}

							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Damage"), new GUIContent("Damage"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Scatter"), new GUIContent("Scatter of Bullets"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RateOfAttack"), new GUIContent("Rate of Shoot"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackAudio"), new GUIContent("Sound"));
							EditorGUILayout.EndVertical();

							if (script.Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
								EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("UseReload"), new GUIContent("Use Reload"));
							if (script.Attacks[0].UseReload)
							{
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("InventoryAmmo"), new GUIContent("Ammo In Magazine"));
								
								if(!script.Attacks[0].HandsReloadAnimation)
									EditorGUILayout.HelpBox("Add a Reload animation in the [Animations] tab.", MessageType.Warning);
							}

							EditorGUILayout.EndVertical();

							break;
						case AIHelper.AttackTypes.Rockets:
							// EditorGUILayout.Space();
							// EditorGUILayout.Space();

							attackPoints.DoLayoutList();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("weapon"), new GUIContent("Weapon° (optional)"));
							
							if (script.weapon)
							{
								EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteWeaponAfterDeath"), new GUIContent("Delete Weapon After Death°", "If active, the weapon will be immediately deleted after the enemy dies"));
							}
							
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("rocket"), new GUIContent("Rocket (prefab)"));
							if (script.Attacks[0].rocket)
							{
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("flightSpeed"), new GUIContent("Flight Speed"));
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("explosion"), new GUIContent("Explosion"));
								EditorGUILayout.EndVertical();
							}

							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Damage"), new GUIContent("Damage"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Scatter"), new GUIContent("Scatter of Rockets"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RateOfAttack"), new GUIContent("Rate of Launch"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackAudio"), new GUIContent("Audio"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("UseReload"), new GUIContent("Use Reload"));
							if (script.Attacks[0].UseReload)
							{
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("InventoryAmmo"), new GUIContent("Ammo In Magazine"));
								
								if(!script.Attacks[0].HandsReloadAnimation)
									EditorGUILayout.HelpBox("Add a Reload animation in the [Animations] tab.", MessageType.Warning);
							}

							EditorGUILayout.EndVertical();

							break;

						case AIHelper.AttackTypes.Fire:
							// EditorGUILayout.Space();
							// EditorGUILayout.Space();

							attackPoints.DoLayoutList();

							EditorGUILayout.Space();
							EditorGUILayout.Space();

							damageColliders.DoLayoutList();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("weapon"), new GUIContent("Weapon° (optional)"));
							
							if (script.weapon)
							{
								EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteWeaponAfterDeath"), new GUIContent("Delete Weapon After Death°", "If active, the weapon will be immediately deleted after the enemy dies"));
							}
							
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("fire"), new GUIContent("Fire (prefab)"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Damage"), new GUIContent("Damage"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RateOfAttack"), new GUIContent("Rate of Atack"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackAudio"), new GUIContent("Sound"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("UseReload"), new GUIContent("Use Reload"));
							if (script.Attacks[0].UseReload)
							{
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("InventoryAmmo"), new GUIContent("Ammo In Magazine"));

								if(!script.Attacks[0].HandsReloadAnimation)
									EditorGUILayout.HelpBox("Add a Reload animation in the [Animations] tab.", MessageType.Warning);
								
							}

							EditorGUILayout.EndVertical();

							break;

						case AIHelper.AttackTypes.Melee:
							// EditorGUILayout.Space();
							// EditorGUILayout.Space();

							EditorGUILayout.HelpBox("Add the [MeleeColliders] events with [on] and [off] parameters on attack animations to set the exact activating/deactivating time of damage colliders.", MessageType.Info);
							damageColliders.DoLayoutList();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("weapon"), new GUIContent("Weapon° (optional)"));
							
							if (script.weapon)
							{
								EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteWeaponAfterDeath"), new GUIContent("Delete Weapon After Death°", "If active, the weapon will be immediately deleted after the enemy dies"));
							}
							
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("Damage"), new GUIContent("Damage"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("RateOfAttack"), new GUIContent("Rate of Attack"));
							EditorGUILayout.EndVertical();

							EditorGUILayout.Space();

							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.HelpBox("Add the [PlayAttackSound] event on attack animations to set the exact playing time of the attack sound.", MessageType.Info);
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("AttackAudio"), new GUIContent("Sound"));
							EditorGUILayout.EndVertical();
							
							EditorGUILayout.Space();

							script.allSidesMovement = false;
							break;
					}

					if (script.Attacks[0].AttackType != AIHelper.AttackTypes.Melee && (!script.Attacks[0].HandsAttackAnimation || !script.Attacks[0].HandsIdleAnimation) || script.Attacks[0].AttackType == AIHelper.AttackTypes.Melee && (script.Attacks[0].MeleeAttackAnimations.Count <= 0 || !HasAttackAnimations()))
					{
						EditorGUILayout.Space();
						EditorGUILayout.HelpBox("Add attack animations in the [Animations] tab.", MessageType.Warning);
					}
					
					EditorGUILayout.EndVertical();

					break;

				case 2:
					EditorGUILayout.Space();

					// EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("health"), new GUIContent("Health"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(new GUIContent("Ragdoll"), EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.BeginVertical("helpbox");
					if (script.isHuman)
					{
						if (!script.BodyParts[0] || script.BodyParts[0] && !script.BodyParts[0].GetComponent<Rigidbody>())
							EditorGUILayout.HelpBox("For humanoid enemies a ragdoll is not needed - it will be automatically generated during the game." + "\n\n" +
							                        "(Don't forget to generate body colliders)", MessageType.Info);
						else EditorGUILayout.HelpBox("For humanoid enemies a ragdoll is not needed (it will be automatically generated during the game).", MessageType.Info);
					}

					EditorGUI.BeginDisabledGroup(script.isHuman);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ragdoll"), new GUIContent("Ragdoll"));
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyRagdollTime"), new GUIContent("Destroy Ragdoll Time"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndVertical();


					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(new GUIContent("UI Elements"), EditorStyles.boldLabel);

					EditorGUILayout.BeginVertical("helpbox");
					// EditorGUILayout.PropertyField(serializedObject.FindProperty("UseHealthBar"), new GUIContent("Use Health Bar"));

					// if (script.UseHealthBar)
					// {
						// EditorGUILayout.BeginVertical("helpbox");
						// EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("healthBarMainObject"), new GUIContent("UI Main Object"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("healthBarValue"), new GUIContent("Fill Value"));
						// EditorGUI.EndDisabledGroup();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("opponentHealthBarColor"), new GUIContent("Color"));

#if USK_ADVANCED_MULTIPLAYER
						EditorGUILayout.PropertyField(serializedObject.FindProperty("teammateHealthBarColor"), new GUIContent("Color (teammate)"));
#endif

						// EditorGUILayout.EndVertical();
					// }

					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					if (script.isHuman)
					{
						if (!script.BodyParts[0] || script.BodyParts[0] && !script.BodyParts[0].GetComponent<Rigidbody>())
						{
							EditorGUILayout.Space();
#if !UNITY_2018_3_OR_NEWER
							EditorGUILayout.HelpBox("Place this prefab in a scene to create the Body Colliders.", MessageType.Info);
#else
							EditorGUILayout.HelpBox("Open this prefab to create Body Colliders.", MessageType.Info);
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
							EditorGUILayout.LabelField(new GUIContent("Damage Multipliers°", "A character's weapon damage will be multiplied by these values"), EditorStyles.boldLabel);
							EditorGUILayout.BeginVertical("helpbox");
							EditorGUILayout.PropertyField(serializedObject.FindProperty("headMultiplier"), new GUIContent("Head"));
							EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyMultiplier"), new GUIContent("Body"));
							EditorGUILayout.PropertyField(serializedObject.FindProperty("handsMultiplier"), new GUIContent("Hands"));
							EditorGUILayout.PropertyField(serializedObject.FindProperty("legsMultiplier"), new GUIContent("Legs"));
							EditorGUILayout.EndVertical();
						}
					}
					else
					{
						EditorGUILayout.Space();
						EditorGUILayout.Space();
						EditorGUILayout.LabelField(new GUIContent("Damage Multipliers°", "A character's weapon damage will be multiplied by these values"), EditorStyles.boldLabel);

						genericColliders.DoLayoutList();
					}
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");

					spawnedItems.DoLayoutList();
					
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
					EditorGUILayout.PropertyField(serializedObject.FindProperty("playDamageSoundsDelay"), new GUIContent("Delay°", "After how many attacks the sounds will be played (set to 0 to play sounds every time the enemy takes damage)."));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();

					damageSounds.DoLayoutList();
					
					EditorGUILayout.EndVertical();
					break;

				case 3:
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(new GUIContent("NavMesh Agent°", "Use these options to set up the enemy to move on NavMesh Areas."), EditorStyles.boldLabel);

					EditorGUILayout.BeginVertical("helpbox");
					NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", serializedObject.FindProperty("navMeshAgentParameters.agentType"));
					
					backgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = new Color(0, 1, 1, 0.5f);
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("navMeshAgentParameters.radius"), new GUIContent("Radius"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("navMeshAgentParameters.height"), new GUIContent("Height"));
					EditorGUILayout.EndVertical();
					GUI.backgroundColor = backgroundColor;
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("setNavMeshHeightMoreAccurately"), new GUIContent("Place Accurately on the Floor°", "If the enemy is walking above the ground, enable this variable to more accurately position it on the surface."));
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(new GUIContent("Movement Parameters"), EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUI.BeginDisabledGroup(!isSceneObjectForText);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("MovementBehaviour"), new GUIContent("Waypoints Behaviour"));
					EditorGUI.EndDisabledGroup();
					
					if (!isSceneObjectForText)
						EditorGUILayout.HelpBox("This parameter will become active when you place the enemy on the scene.", MessageType.Info);
					
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					script.rootMotionMovement = EditorGUILayout.ToggleLeft(new GUIContent("Root Motion°","• ON - The movement is based on Root Motion animations. You can adjust animations speed." + "\n" +
					                                                                                    "• OFF - Set speeds manually."), script.rootMotionMovement);
					
					EditorGUILayout.Space();

					if (!script.rootMotionMovement)
					{
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("walkForwardSpeed"), new GUIContent("Walk Forward Speed"));

						EditorGUILayout.Space();

						EditorGUILayout.PropertyField(serializedObject.FindProperty("runForwardSpeed"), new GUIContent("Run Forward Speed"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("runBackwardSpeed"), new GUIContent("Run Backward Speed"));

						if (script.allSidesMovement || script.useCovers)
						{
							EditorGUILayout.PropertyField(serializedObject.FindProperty("runLateralSpeed"), new GUIContent("Lateral Speed"));
						}
						
						EditorGUILayout.Space();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("turnSpeed"), new GUIContent("Turn Speed°", "If a target is behind the enemy, he turns around at this speed."));

						EditorGUILayout.EndVertical();
					}
					else
					{
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeedOffset"), new GUIContent("Animation Speed Offset"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("turnSpeed"), new GUIContent("Turn Speed°", "If the enemy's target is behind him, he turns at this speed."));
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					script.allSidesMovement = EditorGUILayout.ToggleLeft(new GUIContent("All Directions Movement°", "• ON - the enemy will move during the attack." + "\n" + "• OFF - it will stand still."), script.allSidesMovement);

					if (script.allSidesMovement && !HasAllSidesMovementAnimations())
						EditorGUILayout.HelpBox("Add all-directions movement animations in the [Animations] tab.", MessageType.Warning);

					EditorGUILayout.EndVertical();
					
					EditorGUILayout.EndVertical();


					break;

				case 4:

					EditorGUILayout.Space();

					// script.animsAndSoundInspectorTab = GUILayout.Toolbar(script.animsAndSoundInspectorTab, new[] {"Animations"});//, "Sounds"});

					// EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");

					// switch (script.animsAndSoundInspectorTab)
					// {
					// 	case 0:
					EditorGUILayout.LabelField(new GUIContent("Attack"), EditorStyles.boldLabel);
					
					
					if (script.isHuman)
					{
						if (script.Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
						{
							EditorGUILayout.BeginVertical("HelpBox");
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("HandsIdleAnimation"), new GUIContent("Idle with weapon"));
							EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("HandsAttackAnimation"), new GUIContent("Attack with weapon"));

							if (script.Attacks[0].UseReload)
							{
								EditorGUILayout.Space();
								EditorGUILayout.PropertyField(curAttackSerialized.FindPropertyRelative("HandsReloadAnimation"), new GUIContent("Reload Animation"));
							}
							
							EditorGUILayout.EndVertical();
						}
						else
						{
							attackAnimations.DoLayoutList();
						}
					}
					else
					{
						attackAnimations.DoLayoutList();
					}

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(new GUIContent("Movement"), EditorStyles.boldLabel);

					// EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("IdleAnimation"), new GUIContent("Idle"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackIdleAnimation"), new GUIContent("Idle When Attack (optional)"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkAnimation"), new GUIContent("Walk"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SpotWalk"), new GUIContent("Spot Walk°", "This animation is needed for transitions and intermediate states."));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("runWhileAttacking"), new GUIContent("Run While Attacking°", "[ON] - the enemy runs when attacking" + "\n" +
					                                                                                                                         "[OFF] - the enemy walks when attacking"));
					EditorGUI.BeginDisabledGroup(!script.runWhileAttacking);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAnimation"), new GUIContent("Run"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SpotRun"), new GUIContent("Spot Run°", "This animation is needed for transitions and intermediate states."));
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftRotationAnimation"), new GUIContent("Left Turn"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("RightRotationAnimation"), new GUIContent("Right Turn"));
					EditorGUILayout.EndVertical();
					// EditorGUILayout.EndVertical();
					
					if (script.allSidesMovement || script.useCovers)
					{
						EditorGUILayout.Space();
						EditorGUILayout.Space();

						EditorGUILayout.LabelField(new GUIContent("All Directions Movement"), EditorStyles.boldLabel);

						EditorGUILayout.BeginVertical("HelpBox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(0), new GUIContent("Forward"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(1), new GUIContent("Forward Left"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(2), new GUIContent("Forward Right"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(3), new GUIContent("Left"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(4), new GUIContent("Right"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(5), new GUIContent("Backward Left"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(6), new GUIContent("Backward Right"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("AllSidesMovementAnimations").GetArrayElementAtIndex(7), new GUIContent("Backward"));
						EditorGUILayout.EndVertical();
					}


					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					findAnimations.DoLayoutList();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("damageAnimationTimeout"), new GUIContent("Play Timeout (sec)"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					damageAnimations.DoLayoutList();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("DeathAnimation"), new GUIContent("Death Animation°", "Set a death animation here, or leave the variable empty and then a ragdoll will be generated."));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("GrenadeReaction"), new GUIContent("Grenade Reaction°", "This animation is played when the enemy is in smoke or affected by a flash grenade."));
					EditorGUILayout.EndVertical();
					// break;

					// case 1:
					// if (script.UseStates)
					// {
					// 	EditorGUILayout.BeginVertical("HelpBox");
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase1"), new GUIContent("Phase 1°"));
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase2"), new GUIContent("Phase 2°", "He left, I'll look for him"));
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase3"), new GUIContent("Phase 3°"));
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase4"), new GUIContent("Phase 4°"));
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase5"), new GUIContent("Phase 5°"));
					// 	EditorGUILayout.EndVertical();
					// }
					// else
					// {
					// 	EditorGUILayout.BeginVertical("HelpBox");
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase2"), new GUIContent("Phase 1°", "Damn, he left..."));
					// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("phrase3"), new GUIContent("Phase 2°"));
					// 	EditorGUILayout.EndVertical();
					// }

					// break;
					// }
					EditorGUILayout.EndVertical();

					break;

				case 5:

					if (script.projectSettings)
					{

						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical("helpbox");

//						EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

//						EditorGUILayout.HelpBox("Use the tag to set footsteps sounds for this enemy. " + "\n\n" +
//						                        "Step sounds are set in the surface presets (Assets -> USK -> Presets -> Surfaces).", MessageType.Info);

						EditorGUILayout.BeginVertical("HelpBox");
						script.enemyType = EditorGUILayout.Popup("Enemy ID", script.enemyType, script.projectSettings.EnemiesTags.ToArray());

						EditorGUILayout.BeginHorizontal();
						EditorGUI.BeginDisabledGroup(script.rename);
						if (GUILayout.Button("Rename"))
						{
							script.rename = true;
							script.curName = "";
						}

						EditorGUI.EndDisabledGroup();
						EditorGUI.BeginDisabledGroup(script.projectSettings.EnemiesTags.Count <= 1 || script.delete);
						if (GUILayout.Button("Delete"))
						{
							script.delete = true;
						}

						EditorGUI.EndDisabledGroup();

						if (GUILayout.Button("Create a new one"))
						{
							if (!script.projectSettings.EnemiesTags.Contains("Enemy " + script.projectSettings.EnemiesTags.Count))
								script.projectSettings.EnemiesTags.Add("Enemy " + script.projectSettings.EnemiesTags.Count);
							else script.projectSettings.EnemiesTags.Add("Enemy " + Random.Range(10, 100));

							script.enemyType = script.projectSettings.EnemiesTags.Count - 1;

						}

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
								if (!script.projectSettings.EnemiesTags.Contains(script.curName))
								{
									script.rename = false;
									script.projectSettings.EnemiesTags[script.enemyType] = script.curName;
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
								script.projectSettings.EnemiesTags.Remove(script.projectSettings.EnemiesTags[script.enemyType]);
								script.enemyType = script.projectSettings.EnemiesTags.Count - 1;
								script.delete = false;
							}

							EditorGUILayout.EndHorizontal();
							EditorGUILayout.EndVertical();

						}

						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.Space();
						EditorGUILayout.Space();
					}
					
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("nameText"), new GUIContent("Name Text"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("HelpBox");

					if (!script.isHuman)
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("genericAvatarBody"), new GUIContent("Body"));

						if (script.genericAvatarBody)
						{
							EditorGUILayout.Space();
							script.bodyRotationLimits = EditorGUILayout.Vector3Field(new GUIContent("Body Rotation Limits"), script.bodyRotationLimits);
							EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyRotationSpeed"), new GUIContent("Body Rotation Speed"));
						}
					}
					else
					{
						script.bodyRotationLimits = EditorGUILayout.Vector3Field(new GUIContent("Body Rotation Limits"), script.bodyRotationLimits);
					}

					
					
					EditorGUILayout.EndVertical();
					
						
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Icons for Minimap", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("HelpBox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("blipMainTexture"), new GUIContent("Main Blip"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("blipDeathTexture"), new GUIContent("Death Blip"));
					
					EditorGUILayout.Space();
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("opponentBlipColor"), new GUIContent("Color"));

#if USK_ADVANCED_MULTIPLAYER
					EditorGUILayout.PropertyField(serializedObject.FindProperty("teammateBlipColor"), new GUIContent("Color (teammate)"));
#endif

					EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateBlipWithEnemy"), new GUIContent("Rotate Blips with AI°", "If this option is active, the blip will be rotated with the object direction."));

					
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.LabelField("Multiplayer", EditorStyles.boldLabel);

#if !USK_MULTIPLAYER
                    EditorGUILayout.HelpBox("To enable the multiplayer module, open the Integrations Manager [Window/Universal Shooter Kit/...].", MessageType.Info);
                    
                    EditorGUI.BeginDisabledGroup(true);
#endif


#if USK_MULTIPLAYER
					if (!script.photonAnimatorView)
#else
					if (true)
#endif

					{
#if USK_MULTIPLAYER
						EditorGUILayout.HelpBox("Press this button to use the character in multiplayer. After you add scripts, you do not need to do anything extra, everything works automatically.", MessageType.Info);
#endif
						backgroundColor = GUI.backgroundColor;
						GUI.backgroundColor = new Color(0, 1, 0, 0.5f);

						if (GUILayout.Button("Add Multiplayer Scripts"))
						{
#if USK_MULTIPLAYER
							AIHelper.AddMultiplayerScripts(script.gameObject);
#endif
						}

						GUI.backgroundColor = backgroundColor;
					}
					else
					{
						backgroundColor = GUI.backgroundColor;
						GUI.backgroundColor = new Color(1, 0, 0, 0.5f);

						if (GUILayout.Button("Remove Multiplayer Scripts"))
						{
							deleteMultiplayerScripts = true;
						}

						GUI.backgroundColor = backgroundColor;
					}

#if !USK_MULTIPLAYER
                    EditorGUI.EndDisabledGroup();
#endif

					// EditorGUILayout.Space();

					// EditorGUILayout.BeginVertical("HelpBox");

// #if USK_MULTIPLAYER
// 					EditorGUI.BeginDisabledGroup(!script.gameObject.GetComponent<PhotonView>());
// #else
// 					EditorGUI.BeginDisabledGroup(true);
// #endif
// 					EditorGUI.EndDisabledGroup();

					// EditorGUILayout.EndVertical();
					EditorGUILayout.EndVertical();
					break;
			}

//			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();

			// DrawDefaultInspector();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(script);

				if (!Application.isPlaying)
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

		bool HasAllSidesMovementAnimations()
		{
			var hasAllAnimations = true;
			
			foreach (var anim in script.AllSidesMovementAnimations)
			{
				if (!anim) hasAllAnimations = false;
			}
			
			return hasAllAnimations;
		}
		
		bool HasAttackAnimations()
		{
			var hasAllAnimations = true;
			
			foreach (var anim in script.Attacks[0].MeleeAttackAnimations)
			{
				if (!anim) hasAllAnimations = false;
			}
			
			return hasAllAnimations;
		}

		void CreateRagdoll()
		{
			if (!script.gameObject.activeInHierarchy)
			{
				var tempEnemy = (GameObject) PrefabUtility.InstantiatePrefab(script.gameObject);
				var enemyScript = tempEnemy.GetComponent<AIController>();

				foreach (var part in enemyScript.BodyParts)
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

#if !UNITY_2018_3_OR_NEWER
		        PrefabUtility.ReplacePrefab(tempEnemy, PrefabUtility.GetPrefabParent(tempEnemy), ReplacePrefabOptions.ConnectToPrefab);
#else
				PrefabUtility.SaveAsPrefabAssetAndConnect(tempEnemy, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tempEnemy), InteractionMode.AutomatedAction);
#endif

				DestroyImmediate(tempEnemy);
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
			}

			Helper.CreateRagdoll(script.BodyParts, script.gameObject.GetComponent<Animator>());
		}
	}
}
