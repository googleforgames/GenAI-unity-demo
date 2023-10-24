using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public static class Helper
    {
#if UNITY_EDITOR
        [InitializeOnLoad]
        public static class SelectObjectWhenLoadScene
        {
            static SelectObjectWhenLoadScene()
            {
                EditorSceneManager.sceneOpened += SceneOpenedCallback;
            }

            static void SceneOpenedCallback(Scene _scene, OpenSceneMode _mode)
            {
                if (_scene.name == "Adjustment Scene")
                {
                    Selection.activeObject = Object.FindObjectOfType<Adjustment>();
                }
            }
        }
        
#endif
        
//        [MenuItem ("Export/MyExport")]
//        static void export()
//        {
//            AssetDatabase.ExportPackage (AssetDatabase.GetAllAssetPaths(),PlayerSettings.productName + ".unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
//        }
        
        public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
        {
            public AnimationClipOverrides(int capacity) : base(capacity)
            {
            }

            public AnimationClip this[string name]
            {
                get { return Find(x => x.Key.name.Equals(name)).Value; }
                set
                {
                    int index = FindIndex(x => x.Key.name.Equals(name));
                    if (index != -1)
                        this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
                }
            }
        }
        
        public static int LayerMask()
        {
            var layerMask = ~ (UnityEngine.LayerMask.GetMask("Character") | UnityEngine.LayerMask.GetMask("Grass") | UnityEngine.LayerMask.GetMask("Head") | UnityEngine.LayerMask.GetMask("Noise Collider") | UnityEngine.LayerMask.GetMask("Smoke"));
            return layerMask;
        }

        public static int MultiplayerLayerMask(bool isMultiplayerWeapon)
        {
            if (isMultiplayerWeapon)
                return ~ (UnityEngine.LayerMask.GetMask("Character") | UnityEngine.LayerMask.GetMask("MultiplayerCharacter") | UnityEngine.LayerMask.GetMask("Grass") | UnityEngine.LayerMask.GetMask("Head") | UnityEngine.LayerMask.GetMask("Noise Collider") | UnityEngine.LayerMask.GetMask("Smoke"));
            
            return LayerMask();
        }
        
        [Serializable]
        public class MinimapParameters
        {
            public Texture mapTexture;
            
            public RawImage mapExample;

            [Range(0.1f, 10)] public float mapScale = 1;
            [Range(0.1f, 10)] public float blipsScale = 1;
            public float blipsVisibleDistance = 70;
            
            public bool rotateMinimap = true;
            public bool blipsAreAlwaysVisible;
            public bool rotateBlips = true;
            public bool adjustMapScale;
            public bool useMinimap;

            [Tooltip("If the character moves away from an object, then icon of the object on the mini-map will decrease (until it completely disappears)")]
            public bool scaleBlipsByDistance;

        }

        public enum NextPointAction
        {
            NextPoint, RandomPoint, ClosestPoint, Stop
        }

        public enum RotationAxes
        {
            X, Y, Z
        }

        [Serializable]
        public class Attacker
        {
            public GameObject attackerGO;
            public GameObject collider;
            public List<BodyPartCollider> collidersThatTakingDamage = new List<BodyPartCollider>();
        }
        
        [Serializable]
        public class ActorID
        {
            public int actorID;
            public string type;
        }

        [Serializable]
        public class EnemyToSpawn
        {
            public AIController aiPrefab;
            public int count;
            public float spawnTimeout;
            public float currentTime;
            public int currentSpawnMethodIndex;
            public int spawnedEnemiesCount;
            public SpawnZone spawnZone;
            public MovementBehavior movementBehavior;
            public bool spawnConstantly;
        }

        [Serializable]
        public class CharacterInGameManager
        {
            public GameObject characterPrefab;
            public SpawnZone spawnZone;
        }

        public enum AxisButtonValue
        {
            Plus, Minus, Both
        }

        public enum CubeSolid
        {
            Solid, Wire 
        }

        public static bool[] ButtonsStatus(int size)
        {
            var array = new List<bool>();
            
            for (var i = 0; i < size; i++)
            {
                array.Add(true);
            }

            return array.ToArray();
        }
        

        public static Color32[] colors =
        {
            new Color32(255, 190, 0, 255),
            new Color32(188, 140, 0, 255),
            new Color32(0, 67, 255, 255)
        };


        [Serializable]
        public class EditorColors
        {
            public Color32 linkColor;
            public Color32 textColor;
            public Color32 statusColor;
        }

        public struct ClipPlanePoints
        {
            public Vector3 UpperRight;
            public Vector3 UpperLeft;
            public Vector3 LowerRight;
            public Vector3 LowerLeft;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            do
            {
                if (angle < -360)
                    angle += 360;

                if (angle > 360)
                    angle -= 360;
            } while (angle < -360 || angle > 360);

            return Mathf.Clamp(angle, min, max);
        }

        public static ClipPlanePoints NearPoints(Vector3 pos, Camera camera)
        {
            var clipPlanePoints = new ClipPlanePoints();

            var transform = camera.transform;
            var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
            var aspect = camera.aspect;
            var distance = camera.nearClipPlane;
            var height = distance * Mathf.Tan(halfFOV);
            var width = height * aspect;

            clipPlanePoints.LowerRight = pos + transform.right * width;
            clipPlanePoints.LowerRight -= transform.up * height;
            clipPlanePoints.LowerRight += transform.forward * distance;

            clipPlanePoints.LowerLeft = pos - transform.right * width;
            clipPlanePoints.LowerLeft -= transform.up * height;
            clipPlanePoints.LowerLeft += transform.forward * distance;

            clipPlanePoints.UpperRight = pos + transform.right * width;
            clipPlanePoints.UpperRight += transform.up * height;
            clipPlanePoints.UpperRight += transform.forward * distance;

            clipPlanePoints.UpperLeft = pos - transform.right * width;
            clipPlanePoints.UpperLeft += transform.up * height;
            clipPlanePoints.UpperLeft += transform.forward * distance;

            return clipPlanePoints;
        }
        
        public static class WaitFor
        {
            public static IEnumerator Frames(int frameCount)
            {
                while (frameCount > 0)
                {
                    frameCount--;
                    yield return null;
                }
            }
        }

        public static bool HasParameter(string paramName, Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }

            return false;
        }

        public static Vector3 MoveObjInNewPosition(Vector3 position,Vector3 newPosition, float Speed)
        {
            var x = Mathf.Lerp(position.x, newPosition.x, Speed);
            var y = Mathf.Lerp(position.y, newPosition.y, Speed);
            var z = Mathf.Lerp(position.z, newPosition.z, Speed);
            
            var newPos = new Vector3(x, y, z);

            return newPos;
        }

        public static void CopyTransformsRecurse(Transform src, Transform dst)
        {
            dst.position = src.position;
            dst.rotation = src.rotation;

            foreach (Transform child in dst)
            {
                var curSrc = src.Find(child.name);
                if (curSrc)
                    CopyTransformsRecurse(curSrc, child);
            }
        }

        public static int GetRandomIndex(ref int index, int count)
        {
            var tempIndex = Random.Range(0, count);
            
            if (tempIndex == index)
            {
                tempIndex++;

                if (tempIndex > count - 1)
                    tempIndex = 0;
            }

            index = tempIndex;

            return tempIndex;
        }

        public static void ChangeLayersRecursively(Transform trans, string name)
        {
            if(name == "Character" && trans.gameObject.layer == UnityEngine.LayerMask.NameToLayer("Head"))
                return;

            trans.gameObject.layer = UnityEngine.LayerMask.NameToLayer(name);
            
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, name);
                
//                child.gameObject.layer = LayerMask.NameToLayer(name);
            }
        }

        public static float ConvertAngle(float angle)
        {
            if (angle < 0)
            {
                angle += 360;

                if (angle > 360 - 50)
                {
                    angle -= 360;
                    angle = Mathf.Abs(angle);
                }
            }

            return angle;
        }

        public static float AngleBetween(Vector3 direction1, Vector3 direction2)
        {
            if (direction1 == Vector3.zero || direction2 == Vector3.zero) return 0;
            
            var dir1 = Quaternion.LookRotation(direction1);
            var dir1Angle = dir1.eulerAngles.y;
            if (dir1Angle > 180)
                dir1Angle -= 360;

            var dir2 = Quaternion.LookRotation(direction2);
            var dir2Angle = dir2.eulerAngles.y;
            if (dir2Angle > 180)
                dir2Angle -= 360;

            var middleAngle = Mathf.DeltaAngle(dir1Angle, dir2Angle);
            
            return middleAngle;
        }

        public static Vector2 AngleBetween(Vector3 direction1, Transform obj)
        {
            var look1 = Quaternion.LookRotation(direction1);

            var dir1AngleY = look1.eulerAngles.y;
            if (dir1AngleY > 180)
                dir1AngleY -= 360;
            
            var dir2AngleY = obj.eulerAngles.y;
            if (dir2AngleY > 180)
                dir2AngleY -= 360;

            var middleAngleY = Mathf.DeltaAngle(dir1AngleY, dir2AngleY);

            var dir1AngleX = look1.eulerAngles.x;
            if (dir1AngleX > 180)
                dir1AngleX -= 360;

            var dir2AngleX = obj.eulerAngles.x;
            if (dir2AngleX > 180)
                dir2AngleX -= 360;

            var middleAngleX = Mathf.DeltaAngle(dir1AngleX, dir2AngleX);

            return new Vector2(middleAngleX, middleAngleY);
        }
        
        public static float AngleBetween(Vector3 direction1, Vector3 position1, string empty)
        {
            var look1 = Quaternion.LookRotation(direction1);

            var dir1AngleY = look1.eulerAngles.y;
            if (dir1AngleY > 180)
                dir1AngleY -= 360;
            
            var dir2AngleY = position1.y;
            if (dir2AngleY > 180)
                dir2AngleY -= 360;

            var middleAngleY = Mathf.DeltaAngle(dir1AngleY, dir2AngleY);

            return middleAngleY;
        }
        
        public static bool IsBetween(double testValue, double bound1, double bound2)
        {
            if (bound1 > bound2)
                return testValue >= bound2 && testValue <= bound1;
            return testValue >= bound1 && testValue <= bound2;
        }

        public static bool ReachedPositionAndRotation(Vector3 position1, Vector3 position2, Vector3 angles1, Vector3 angles2)
        {
            return Math.Abs(position1.x - position2.x) < 0.2f && Math.Abs(position1.y - position2.y) < 0.2f && Math.Abs(position1.z - position2.z) < 0.2f &&
                   Math.Abs(angles1.x - angles2.x) < 0.2f && Math.Abs(angles1.y - angles2.y) < 0.2f && Math.Abs(angles1.z - angles2.z) < 0.2f;
        }
        
        public static bool ReachedPositionAndRotationAccurate(ref Vector3 position1, Vector3 position2, ref Vector3 angles1, Vector3 angles2, float rotationError, float positionError)
        {
            var delX = Math.Abs(angles1.x - angles2.x);
            var delY = Math.Abs(angles1.y - angles2.y);
            var delZ = Math.Abs(angles1.z - angles2.z);

            if (delX > 180)
                delX -= 360;
            
            if (delY > 180)
                delY -= 360;
            
            if (delZ > 180)
                delZ -= 360;
//
            if (Math.Abs(position1.x - position2.x) < positionError && Math.Abs(position1.y - position2.y) < positionError && Math.Abs(position1.z - position2.z) < positionError && Math.Abs(delX) < rotationError && Math.Abs(delY) < rotationError && Math.Abs(delZ) < rotationError)
            {
                position1 = position2;
                angles1 = angles2;

                return true;
            }

            return false;
        }

        public static bool ReachedPositionAndRotation(Vector3 position1, Vector3 position2, float positionDelta)
        {
            return Math.Abs(position1.x - position2.x) < positionDelta && Math.Abs(position1.y - position2.y) < positionDelta && Math.Abs(position1.z - position2.z) < positionDelta;
        }

        public static Camera NewCamera(string name, Transform parent, string type)
        {
            Camera camera = new GameObject(name).AddComponent<Camera>();

            if (type != "GameManager")
            {
                camera.cullingMask = 1 << 8;
                camera.depth = 1;
                camera.clearFlags = CameraClearFlags.Depth;
                camera.nearClipPlane = 0.01f;
            }

            camera.transform.parent = parent;
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.Euler(0, 0, 0);

            return camera;
        }

        public static void ChangeButtonColor(Button button, Color color, Sprite sprite)
        {
            switch (button.transition)
            {
                case Selectable.Transition.ColorTint:
                    var buttonColors = button.colors;
                    buttonColors.normalColor = color;
                    button.colors = buttonColors;
                    break;
                case Selectable.Transition.SpriteSwap:
                    if (sprite)
                        button.GetComponent<Image>().sprite = sprite;
                    break;
            }
        }

        public static void ChangeButtonColor (UIManager uiManager, int slot, string type)
        {
            if (type != "norm")
            {
                if(uiManager.CharacterUI.Inventory.WeaponsButtons[slot])
                    ChangeButtonColor(uiManager.CharacterUI.Inventory.WeaponsButtons[slot], uiManager.CharacterUI.Inventory.WeaponsButtons[slot].colors.highlightedColor , uiManager.CharacterUI.Inventory.WeaponsButtons[slot].spriteState.highlightedSprite);
            }
            else
            {
                if(uiManager.CharacterUI.Inventory.WeaponsButtons[slot])
                    ChangeButtonColor(uiManager.CharacterUI.Inventory.WeaponsButtons[slot], uiManager.CharacterUI.Inventory.normButtonsColors[slot], uiManager.CharacterUI.Inventory.normButtonsSprites[slot]);
            }
        }

        public static void AddButtonsEvents(Button[] buttons, InventoryManager manager, Controller controller)
        {
            if (buttons[0])
            {
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(manager.UIAim);
            }

            if (buttons[1])
            {
                buttons[1].onClick.RemoveAllListeners();
                buttons[1].onClick.AddListener(manager.UIReload);
            }

            if (buttons[2])
            {
                buttons[2].onClick.RemoveAllListeners();
                buttons[2].onClick.AddListener(controller.ChangeCameraType);
            }

            if (buttons[3])
            {
                buttons[3].onClick.RemoveAllListeners();
                buttons[3].onClick.AddListener(manager.UIChangeAttackType);
            }

            if (buttons[4])
            {
                buttons[4].onClick.RemoveAllListeners();
                buttons[4].onClick.AddListener(delegate { manager.DropWeapon(true); });
            }

            if (buttons[8])
            {
                buttons[8].onClick.RemoveAllListeners();
                buttons[8].onClick.AddListener(controller.Jump); 
            }

            if (buttons[11])
            {
                buttons[11].onClick.RemoveAllListeners();
                buttons[11].onClick.AddListener(manager.UIPickUp);
            }

            if (buttons[13])
            {
                buttons[13].onClick.RemoveAllListeners();
                buttons[13].onClick.AddListener(manager.WeaponDown);
            }

            if (buttons[14])
            {
                buttons[14].onClick.RemoveAllListeners();
                buttons[14].onClick.AddListener(manager.WeaponUp);
            }

            if (buttons[16])
            {
                buttons[16].onClick.RemoveAllListeners();
                buttons[16].onClick.AddListener(controller.ChangeMovementType);
            }
            

            if (buttons[5])
            {
                addEventTriger(buttons[5].gameObject, manager, controller, "Attack");
            }

            if (buttons[6])
            {
                if (controller.projectSettings.holdSprintButton)
                {
                    addEventTriger(buttons[6].gameObject, manager, controller, "Sprint");
                }
                else
                {
                    buttons[6].onClick.RemoveAllListeners();
                    buttons[6].onClick.AddListener(delegate { controller.Sprint(true, "click"); });
                }
            }

            if (buttons[7])
            {
                if (controller.projectSettings.holdCrouchButton)
                {
                    addEventTriger(buttons[7].gameObject, manager, controller, "Crouch");
                }
                else
                {
                    buttons[7].onClick.RemoveAllListeners();
                    buttons[7].onClick.AddListener(delegate { controller.Crouch(true, "click"); });
                }
            }

            if (buttons[10])
            {
                if (controller.projectSettings.holdInventoryButton)
                {
                    addEventTriger(buttons[10].gameObject, manager, controller, "Inventory");
                }
                else
                {
                    buttons[10].onClick.RemoveAllListeners();
                    buttons[10].onClick.AddListener(manager.UIInventory);
                }
            }
        }

        public static void addEventTriger(GameObject button, InventoryManager manager, Controller controller, string type)
        {
            var eventTrigger = !button.GetComponent<EventTrigger>() ? button.AddComponent<EventTrigger>() : button.GetComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
            
            var entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};

            switch (type)
            {
                case "Attack":
                    entry.callback.AddListener(data => { manager.UIAttack(); });
                    break;
                case "Sprint":
                    entry.callback.AddListener(data => { controller.Sprint(true, "press"); });
                    break;
                case "Crouch":
                    entry.callback.AddListener(data => { controller.Crouch(true, "press"); });
                    break;
                case "Inventory":
                    entry.callback.AddListener(data => { manager.UIActivateInventory(); });
                    break;
            }
            eventTrigger.triggers.Add(entry);
            entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerUp};

            switch (type)
            {
                case "Attack":
                    entry.callback.AddListener(data => { manager.UIEndAttack(); });
                    break;
                case "Sprint":
                    entry.callback.AddListener(data => { controller.Sprint(false, "press"); });
                    break;
                case "Crouch":
                    entry.callback.AddListener(data => { controller.Crouch(false, "press"); });
                    break;
                case "Inventory":
                    entry.callback.AddListener(data => { manager.UIDeactivateInventory(); });
                    break;
            }
            
            eventTrigger.triggers.Add(entry);
        }
        
        
        public static Transform NewObject(Transform parent, string name, PrimitiveType type, Color color, float size, CubeSolid mode)
        {
            color = new Color(color.r, color.g, color.b, 0);
            
            var sourse = GameObject.CreatePrimitive(type).transform;
            sourse.name = name;
            sourse.hideFlags = HideFlags.HideInHierarchy;
            sourse.GetComponent<MeshRenderer>().enabled = false;
            sourse.GetComponent<MeshRenderer>().material = NewMaterial(color);
            sourse.localScale = new Vector3(size / 100, size / 100, size / 100);
            sourse.parent = parent;
            sourse.localPosition = Vector3.zero;
            sourse.localRotation = Quaternion.Euler(Vector3.zero);
            ChangeLayersRecursively(sourse, "Character");

            return sourse;
        }
        
        public static Transform NewObject(Transform parent, string name)
        {
            var sourse = new GameObject(name).transform;
            sourse.hideFlags = HideFlags.HideInHierarchy;
            sourse.parent = parent;
            sourse.localPosition = Vector3.zero;
            sourse.localRotation = Quaternion.Euler(Vector3.zero);
            ChangeLayersRecursively(sourse, "Character");
            return sourse;
        }
        
        public static Material NewMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard")) {name = "Standard", color = color};
            ChangeShaderMode.ChangeRenderMode(mat, ChangeShaderMode.BlendMode.Fade);
            return mat;
        }

        public static string GenerateRandomString(int count)
        {
            const string glyphs= "abcdefghijklmnopqrstuvwxyz0123456789";

            var name = "";
            
            for (int i = 0; i < count; i++)
            {
                name += glyphs[Random.Range(0, glyphs.Length)];
            }

            return name;
        }
        
        // public static bool CheckGamepadAxisButton(int number, string[] _gamepadButtonsAxes, bool[] hasAxisButtonPressed, string type, AxisButtonValue value)
        // {
        //     if (_gamepadButtonsAxes[number] != "")
        //     {
        //         if (type == "GetKeyDown")
        //         {
        //             switch (value)
        //             {
        //                 case AxisButtonValue.Both:
        //                 {
        //                     if ((Input.GetAxis(_gamepadButtonsAxes[number]) > 0.5f || Input.GetAxis(_gamepadButtonsAxes[number]) < -0.5f) && !hasAxisButtonPressed[number])
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //                 }
        //
        //                 case AxisButtonValue.Plus:
        //                 {
        //                     if (Input.GetAxis(_gamepadButtonsAxes[number]) > 0.5f && !hasAxisButtonPressed[number])
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //                 }
        //
        //                 case AxisButtonValue.Minus:
        //                 {
        //                     if (Input.GetAxis(_gamepadButtonsAxes[number]) < -0.5f && !hasAxisButtonPressed[number])
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //                 }
        //             }
        //
        //             if (Input.GetAxis(_gamepadButtonsAxes[number]) < 0.1f && Input.GetAxis(_gamepadButtonsAxes[number]) > -0.1f && hasAxisButtonPressed[number])
        //             {
        //                 hasAxisButtonPressed[number] = false;
        //                 return false;
        //             }
        //         }
        //         else
        //         {
        //             switch (value)
        //             {
        //                 case AxisButtonValue.Both:
        //                     if (Input.GetAxis(_gamepadButtonsAxes[number]) > 0.5f || Input.GetAxis(_gamepadButtonsAxes[number]) < -0.5f)
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //
        //                 case AxisButtonValue.Plus:
        //                     if (Input.GetAxis(_gamepadButtonsAxes[number]) > 0.5f)
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //
        //                 case AxisButtonValue.Minus:
        //
        //                     if (Input.GetAxis(_gamepadButtonsAxes[number]) < -0.5f)
        //                     {
        //                         hasAxisButtonPressed[number] = true;
        //                         return true;
        //                     }
        //
        //                     break;
        //             }
        //
        //
        //             if (Math.Abs(Input.GetAxis(_gamepadButtonsAxes[number])) < 0.2f && Math.Abs(Input.GetAxis(_gamepadButtonsAxes[number])) > -0.2f)
        //             {
        //                 hasAxisButtonPressed[number] = false;
        //                 return false;
        //             }
        //         }
        //     }
        //
        //     return false;
        // }

        public static void CreateNoiseCollider(Transform parent, Controller script)
        {
            var noiseCollider = new GameObject("Noise Collider");
            noiseCollider.transform.parent = parent;
            noiseCollider.transform.localPosition= Vector3.zero;
            noiseCollider.transform.localEulerAngles = Vector3.zero;
            noiseCollider.layer = 12;

            script.noiseCollider = noiseCollider.AddComponent<SphereCollider>();
            script.noiseCollider.isTrigger = true;
            var rb = noiseCollider.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // noiseCollider.hideFlags = HideFlags.HideInHierarchy;
        }

        public static void EnableAllParents(GameObject childObject)
        {
            var t = childObject.transform;
            childObject.SetActive(true);
            
            while (t.parent != null)
            {
                t.parent.gameObject.SetActive(true);
                t = t.parent;
            }
        }
        
        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
  
        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static void SetLineRenderer (ref LineRenderer lineRenderer, GameObject parent, Material trailMaterial)
        {
            lineRenderer = parent.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.material = trailMaterial;
            lineRenderer.positionCount = 50;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            var curve = new AnimationCurve();
            curve.AddKey(0, 0.5f);
            curve.AddKey(1, 1);
            lineRenderer.widthCurve = curve;
        }

        public static void ManageBodyColliders(List<Transform> BodyParts, Controller controller)
        {
            ManageBodyCollider(BodyParts[0], controller, BodyPartCollider.BodyPart.Body, controller.bodyMultiplier);
            ManageBodyCollider(BodyParts[1], controller, BodyPartCollider.BodyPart.Body, controller.bodyMultiplier);
            ManageBodyCollider(BodyParts[2], controller, BodyPartCollider.BodyPart.Head, controller.headMultiplier);
            ManageBodyCollider(BodyParts[3], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[4], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[5], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[6], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[7], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[8], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[9], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[10], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            
            if(BodyParts[0])
                BodyParts[0].GetComponent<BodyPartCollider>().checkColliders = true;
        }
        
        public static void ManageBodyColliders(List<Transform> BodyParts, AIController controller)
        {
            ManageBodyCollider(BodyParts[0], controller, BodyPartCollider.BodyPart.Body, controller.bodyMultiplier);
            ManageBodyCollider(BodyParts[1], controller, BodyPartCollider.BodyPart.Body, controller.bodyMultiplier);
            ManageBodyCollider(BodyParts[2], controller, BodyPartCollider.BodyPart.Head, controller.headMultiplier);
            ManageBodyCollider(BodyParts[3], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[4], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[5], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[6], controller, BodyPartCollider.BodyPart.Legs, controller.legsMultiplier);
            ManageBodyCollider(BodyParts[7], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[8], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[9], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            ManageBodyCollider(BodyParts[10], controller, BodyPartCollider.BodyPart.Hands, controller.handsMultiplier);
            
            if(BodyParts[0])
                BodyParts[0].GetComponent<BodyPartCollider>().checkColliders = true;
        }

        public static void ManageBodyColliders(List<AIHelper.GenericCollider> colliders, AIController controller)
        {
            for (var i = 0; i < colliders.Count; i++)
            {
                ManageBodyCollider(colliders[i].collider.transform, controller, colliders[i].damageMultiplier, i == 0);
            }
        }
        

        private static void ManageBodyCollider(Transform bodyPart, Controller controller, BodyPartCollider.BodyPart bodyPartType, float multiplier)
        {
            if(!bodyPart)
                return;

            var script = bodyPart.gameObject.AddComponent<BodyPartCollider>();
            script.controller = controller;
            script.bodyPart = bodyPartType;
            script.damageMultiplayer = multiplier;
            bodyPart.GetComponent<Rigidbody>().isKinematic = true;
        }
        
        private static void ManageBodyCollider(Transform bodyPart, AIController controller, BodyPartCollider.BodyPart bodyPartType, float multiplier)
        {
            if(!bodyPart)
                return;
            
            var script = bodyPart.gameObject.AddComponent<BodyPartCollider>();
            controller.allBodyColliders.Add(script);
            script.aiController = controller;
            script.bodyPart = bodyPartType;
            script.damageMultiplayer = multiplier;
            bodyPart.GetComponent<Rigidbody>().isKinematic = true;
        }

        private static void ManageBodyCollider(Transform collider, AIController controller, float multiplier, bool firstCollider)
        {
            if(!collider)
                return;
            
            var script = collider.gameObject.AddComponent<BodyPartCollider>();
            if (firstCollider)
                script.checkColliders = true;
            
            controller.allBodyColliders.Add(script);
            script.aiController = controller;
            script.damageMultiplayer = multiplier;
        }
        
        public static Canvas NewCanvas(string name, Vector2 size, Transform parent)
        {
            var canvas = new GameObject(name);
            canvas.AddComponent<RectTransform>();
            var _canvas = canvas.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = size;
            canvas.AddComponent<GraphicRaycaster>();
            canvas.transform.SetParent(parent);

            return _canvas;
        }

#if UNITY_EDITOR

        public static void DrawGizmoRectangle(Vector3 position, Vector3 scale, Vector3 rotation, Color32 color)
        {
            byte alpha = 255;
            byte additionalAlpha = 100;
            byte additionalAlpha2 = 30;
            
            var rot = Quaternion.Euler(0, rotation.y, 0);

            var verts = new[]
            {
                new Vector3(position.x - scale.x, position.y, position.z - scale.z),
                new Vector3(position.x - scale.x, position.y, position.z + scale.z),
                new Vector3(position.x + scale.x, position.y, position.z + scale.z),
                new Vector3(position.x + scale.x, position.y, position.z - scale.z)
            };

            for (var i = 0; i < verts.Length; i++)
            {
                verts[i] = rot * (verts[i] - position) + position;
            }
				
            Handles.zTest = CompareFunction.Less;
            Handles.color = new Color32(color.r, color.g, color.b, alpha);
            Handles.DrawSolidRectangleWithOutline(verts, new Color32(color.r, color.g, color.b, additionalAlpha), new Color32(0, 0, 0, 255));

            Handles.zTest = CompareFunction.Greater;
            Handles.color = new Color32(color.r, color.g, color.b, additionalAlpha);
            Handles.DrawSolidRectangleWithOutline(verts, new Color32(color.r, color.g, color.b, additionalAlpha2), new Color32(0, 0, 0, 100));
        }

       public static bool LinkLabel(GUIContent label, Rect rect, float lenght, GUIStyle style, bool isLabel, bool isActive, bool isLink, bool uiManager, params GUILayoutOption[] options)
        {
            var position = rect;//GUILayoutUtility.GetRect(label, style, options);

            if (isLink)
            {
                Handles.BeginGUI();
                Handles.color = style.normal.textColor;
                Handles.DrawLine(new Vector3(position.xMin, position.yMin + style.lineHeight + (isLabel ? 1 : 0)), new Vector3(position.xMin + lenght, position.yMin + style.lineHeight + (isLabel ? 1 : 0)));
                Handles.color = Color.white;
                Handles.EndGUI();
            }

            var _rect = new Rect(position.xMin, !uiManager ? position.yMin : position.yMin - style.lineHeight / 2, lenght, !uiManager ? style.lineHeight : style.lineHeight * 2);
            
            if(isActive)
                EditorGUIUtility.AddCursorRect(_rect, MouseCursor.Link);

            return GUI.Button(_rect, label, style);
        }

       public static bool LinkLabel(GUIContent label, Rect rect, float lenght)
       {
           var position = rect;//GUILayoutUtility.GetRect(label, style, options);

           var _rect = new Rect(position.xMin, position.yMin, lenght, EditorGUIUtility.singleLineHeight);
           
           EditorGUIUtility.AddCursorRect(_rect, MouseCursor.Link);

           return GUI.Button(_rect, label);
       }

       // public static void DrawProObjectField<T>(GUIContent label, SerializedProperty value, GUIStyle style, bool allowSceneObjects, Texture objIcon = null, params GUILayoutOption[] options) where T : UnityEngine.Object
       // {
       //
       //     T tObj = value.objectReferenceValue as T;
       //
       //     if (objIcon == null)
       //     {
       //         objIcon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
       //     }
       //
       //     style.imagePosition = ImagePosition.ImageLeft;
       //
       //     int pickerID = 455454425;
       //
       //     if (tObj != null)
       //     {
       //         EditorGUILayout.LabelField(label, new GUIContent(tObj.name, objIcon), style, options);
       //     }
       //
       //     if (GUILayout.Button("Select"))
       //     {
       //         EditorGUIUtility.ShowObjectPicker<T>(tObj, allowSceneObjects, "", pickerID);
       //     }
       //
       //     if (Event.current.commandName == "ObjectSelectorUpdated")
       //     {
       //         if (EditorGUIUtility.GetObjectPickerControlID() == pickerID)
       //         {
       //             tObj = EditorGUIUtility.GetObjectPickerObject() as T;
       //             value.objectReferenceValue = tObj;
       //         }
       //     }
       //
       // }

       public static void CreateRagdoll (List<Transform> BodyParts, Animator animator)
        {
            var ragdollBuilderType = Type.GetType("UnityEditor.RagdollBuilder, UnityEditor");
            var windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);

            if (windows == null || windows.Length == 0)
            {
                EditorApplication.ExecuteMenuItem("GameObject/3D Object/Ragdoll...");
                windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            }

            if (windows != null && windows.Length > 0)
            {
                var ragdollWindow = windows[0] as ScriptableWizard;
                
                BodyParts[0] = SetFieldValue(ragdollWindow, "pelvis", animator.GetBoneTransform(HumanBodyBones.Hips));
                BodyParts[1] = SetFieldValue(ragdollWindow, "middleSpine", animator.GetBoneTransform(HumanBodyBones.Spine));
                BodyParts[2] = SetFieldValue(ragdollWindow, "head", animator.GetBoneTransform(HumanBodyBones.Head));
                BodyParts[3] = SetFieldValue(ragdollWindow, "leftHips", animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
                BodyParts[4] = SetFieldValue(ragdollWindow, "leftKnee", animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
                SetFieldValue(ragdollWindow, "leftFoot", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                BodyParts[5] = SetFieldValue(ragdollWindow, "rightHips", animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
                BodyParts[6] = SetFieldValue(ragdollWindow, "rightKnee", animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
                SetFieldValue(ragdollWindow, "rightFoot", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                BodyParts[7] = SetFieldValue(ragdollWindow, "leftArm", animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
                BodyParts[8] = SetFieldValue(ragdollWindow, "leftElbow", animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
                BodyParts[9] = SetFieldValue(ragdollWindow, "rightArm", animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
                BodyParts[10] = SetFieldValue(ragdollWindow, "rightElbow", animator.GetBoneTransform(HumanBodyBones.RightLowerArm));

                var method = ragdollWindow.GetType().GetMethod("CheckConsistency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
               
                if (method != null)
                {
                    ragdollWindow.errorString = (string) method.Invoke(ragdollWindow, null);
                    ragdollWindow.isValid = string.IsNullOrEmpty(ragdollWindow.errorString);
                }
                
            }
        }
         
         
        private static Transform SetFieldValue(ScriptableWizard obj, string name, Transform value)
        {
            if (value == null)
            {
                return null;
            }

            var field = obj.GetType().GetField(name);
            if (field != null)
            { 
                field.SetValue(obj, value);
            }

            return value;
        }
        
        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Vector3.one);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;
 
                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
 
            }
        }
        
        public static void InitStyles(ref GUIStyle style, Color32 color)
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.helpBox) {normal = {background = MakeTex(2, 2, color)}};
            }
        }
        
        
        private static Texture2D MakeTex( int width, int height, Color col )
        {
            var pix = new Color[width * height];
            for( int i = 0; i < pix.Length; ++i )
            {
                pix[ i ] = col;
            }
            var result = new Texture2D( width, height );
            result.SetPixels( pix );
            result.Apply();
            return result;
        }
        
        public static class NamedGUILayout
        {
            public static bool ButtonWasJustClicked
            {
                get;
                private set;
            }
 
            public static string LastClickedButtonName
            {
                get;
                private set;
            }
 
            public static bool Button(Rect rect, string label, string addonName)
            {
                return Button(new GUIContent(label), rect, addonName);
            }

            private static bool Button(GUIContent label, Rect rect, string controlName)
            {
                if(ButtonWasJustClicked)
                {
                    if(string.Equals(LastClickedButtonName, controlName, StringComparison.Ordinal))
                    {
                        ButtonWasJustClicked = false;
                    }
                }
 
                if(GUI.Button(rect, label))
                {
                    ButtonWasJustClicked = true;
                    LastClickedButtonName = controlName;
                    return true;
                }
                return false;
            }
 
            public static bool TryGetNameOfJustClickedButton(out string name)
            {
                if(ButtonWasJustClicked)
                {
                    name = LastClickedButtonName;
                    return true;
                }
 
                name = "";
                return false;
            }
        }
        
        [MenuItem("Tools/Universal Shooter Kit/Adjust #a")]
        public static void OpenAdjustmentScene()
        {
            if(Application.isPlaying) return;
            
            if (SceneManager.GetActiveScene().name != "Adjustment Scene")
            {
                var inputs = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
                inputs.oldScenePath = SceneManager.GetActiveScene().path;
                inputs.oldSceneName = SceneManager.GetActiveScene().name;
                
                if(EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] {SceneManager.GetActiveScene()}))
                    EditorSceneManager.OpenScene("Assets/Universal Shooter Kit/Framework/Tools/Assets/_Scenes/Adjustment Scene.unity", OpenSceneMode.Single);
            }
        }
        
        [MenuItem("Tools/Universal Shooter Kit/Project Settings/Weapons Pool", false, 4000)]
        public static void WeaponsPool()
        {
            if(Application.isPlaying) return;

            var pool = Resources.Load("Weapons Pool", typeof(WeaponsPool)) as WeaponsPool;
            Selection.activeObject = pool;
            EditorGUIUtility.PingObject(pool);
        }

        [MenuItem("Tools/Universal Shooter Kit/Project Settings/Input #i", false, -100)]
        public static void Inputs()
        {
            if(Application.isPlaying) return;
            
            var inputs = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/Input.asset", typeof(ProjectSettings)) as ProjectSettings;
            Selection.activeObject = inputs;
            EditorGUIUtility.PingObject(inputs);
        }
        
        [MenuItem("Tools/Universal Shooter Kit/Project Settings/UI Manager #u",false, -100)]
        public static void UI()
        {
            if(Application.isPlaying) return;
            
            var ui = Resources.Load("UI Manager", typeof(UIManager)) as UIManager;//AssetDatabase.LoadAssetAtPath("Assets/Universal Shooter Kit/Tools/!Settings/UI Manager.prefab", typeof(UIManager)) as UIManager;
            AssetDatabase.OpenAsset(ui);
            EditorGUIUtility.PingObject(ui);
            // Selection.activeObject = ui;
            
            

            // if (SceneManager.GetActiveScene().name != "UI Manager")
            // {
            //     // var inputs = Resources.Load("Input", typeof(ProjectSettings)) as ProjectSettings;
            //     // inputs.oldScenePath = SceneManager.GetActiveScene().path;
            //     // inputs.oldSceneName = SceneManager.GetActiveScene().name;
            //
            //     if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] {SceneManager.GetActiveScene()}))
            //     {
            //         var scene = Resources.Load("UI Manager") as SceneAsset;
            //         var path = AssetDatabase.GetAssetPath(scene); 
            //         if (scene != null) EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            //     }
            // }

        }
        
        [MenuItem("GameObject/Universal Shooter Kit/Common Components/Movement Behavior (AI)", false, -10)]
        public static void CreateWaypointBehavior()
        {
            var behavior = new GameObject("Movement Behaviour");
            behavior.AddComponent<MovementBehavior>();
            Selection.activeObject = behavior;
        }
        
        [MenuItem("GameObject/Universal Shooter Kit/Common Components/Spawn Zone", false, -10)]
        public static void CreateSpawnZone()
        {
            var zone = new GameObject("Spawn Zone");
            zone.AddComponent<SpawnZone>();
            
            if (SceneView.lastActiveSceneView)
            {
                var transform = SceneView.lastActiveSceneView.camera.transform;
                zone.transform.position = transform.position + transform.forward * 10;
            }
            
            EditorGUIUtility.PingObject(zone);
        }
        
        [MenuItem("GameObject/Universal Shooter Kit/Common Components/Stealth Area", false, -10)]
        public static void CreateStealthZone()
        {
            var zone = new GameObject("Stealth Zone");
            zone.AddComponent<StealthZone>();
            
            if (SceneView.lastActiveSceneView)
            {
                var transform = SceneView.lastActiveSceneView.camera.transform;
                zone.transform.position = transform.position + transform.forward * 10;
            }
            
            EditorGUIUtility.PingObject(zone);
        }


        [MenuItem("GameObject/Universal Shooter Kit/Single-player/Game Manager", false, 1)]
        public static void CreateGameManger()
        {
            var manager = new GameObject("GameManager");
            var script = manager.AddComponent<GameManager>();
            
            script.defaultCamera = NewCamera("Default camera", manager.transform, "GameManager");

            // CreateEventManager(manager.transform);

            Selection.activeObject = manager;
        }

#if USK_MULTIPLAYER
        [MenuItem("GameObject/Universal Shooter Kit/Multiplayer/Lobby Manager", false, 1)]
        public static void CreateLobbyManger()
        {
            var manager = new GameObject("Lobby");
            var script = manager.AddComponent<LobbyManager>();
            
            script.defaultCamera = NewCamera("Default camera", manager.transform, "GameManager");

            // CreateEventManager(manager.transform);

            Selection.activeObject = manager;
        }
        
        [MenuItem("GameObject/Universal Shooter Kit/Multiplayer/Room Manager", false, 2)]
        public static void CreateRoomManager()
        {
            var manager = new GameObject("Room Manager");
            var script = manager.AddComponent<RoomManager>();
            
            script.DefaultCamera = Helper.NewCamera("Default camera", manager.transform, "GameManager");
            
            // CreateEventManager(manager.transform);
            
            Selection.activeObject = manager;
        }
#endif
        
        public static void AddObjectIcon(GameObject obj, string icon)
        {
            var image = (Texture2D)Resources.Load(icon);
            var editorGuiUtilityType = typeof(EditorGUIUtility);
            var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            var args = new object[] { obj, image };
            editorGuiUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
        }
        
        public static GameObject CreateWayPoint(MovementBehavior behavior)
        {
            // var foundObjects = GameObject.FindGameObjectsWithTag("WayPoint");
            var curPointNumber = 0;

            foreach (var obj in behavior.points)
            {
                obj.point.name = "Waypoint " + curPointNumber;
                curPointNumber++;
            }

            var waypoint = new GameObject("Waypoint " + curPointNumber);
            // waypoint.tag = "WayPoint";
            AddObjectIcon(waypoint, "DefaultWaypoint");

            if (SceneView.lastActiveSceneView)
            {
                var transform = SceneView.lastActiveSceneView.camera.transform;
                waypoint.transform.position = transform.position + transform.forward * 10;
            }
            
            Selection.activeObject = waypoint;
            EditorGUIUtility.PingObject(waypoint);
            
            return waypoint;
        }

        public static Transform NewPoint(GameObject parent, string name)
        {
            var point = new GameObject(name).transform;
            point.parent = parent.transform;
            point.localPosition = Vector3.zero;
            point.localRotation = Quaternion.Euler(Vector3.zero);
            point.localScale = Vector3.one;
            EditorUtility.SetDirty(parent.GetComponent<WeaponController>());
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return point;
        }

        public static BoxCollider NewCollider(string name, string tag, Transform parent)
        {
            var collider = new GameObject {name = name, tag = tag};
            var boxCollider = collider.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
				
            var rigidbody = collider.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            collider.transform.parent = parent;
            collider.transform.localPosition = Vector3.zero;
            collider.transform.localScale = Vector3.one;
            return boxCollider;

        }

        static RawImage NewImagePlace(string name, Transform parent, Transform parent2, Vector2 size)
        {
            var image = NewUIElement(name, parent, Vector2.zero, size, Vector3.one);

            var raw = image.AddComponent<RawImage>();
            raw.color = new Color(1, 1, 1, 0);

            raw.raycastTarget = false;

            image.transform.SetParent(parent2);

            return raw;
        }

        public static Image NewImage(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var image = NewUIElement(name, parent, position, size, Vector3.one);

            var img = image.AddComponent<Image>();
            img.raycastTarget = false;

            return img;
        }


        static Button NewInventoryPart(string name, string type, Transform parent, Vector2 position, Vector2 size,
            Vector3 rotation,
            Sprite image)
        {
            var part = NewUIElement(name, parent, position, size, Vector3.one);

            var img = part.AddComponent<Image>();
            img.sprite = image;
            img.color = new Color32(255, 255, 255, 1);

            part.AddComponent<Mask>();
            part.AddComponent<RaycastMask>();

            var button = NewButton("Button", new Vector2(0, 0), new Vector2(200, 200), Vector3.one, image,
                part.transform);

            var colors = button.colors;

            colors.normalColor = new Color32(0, 67, 255, 170);

            if (type == "wheel")
                colors.highlightedColor = new Color32(255, 190, 0, 190);
            else
                colors.normalColor = new Color32(0, 67, 255, 170);
            
            colors.pressedColor = new Color32(223, 166, 0, 190);

            button.colors = colors;

            part.GetComponent<RectTransform>().eulerAngles = rotation;

            return button;
        }

        public static GameObject NewText(string name, Transform parent, Vector2 position, Vector2 size,
            string textContent, Font font, int textSize, TextAnchor textAlignment, Color textColor, bool needOutline)
        {
            var textObject = NewUIElement(name, parent, position, size, Vector3.one);
            
            var text = textObject.AddComponent<Text>();
            text.text = textContent;
            
            if(font)
                text.font = font;
            
            text.fontSize = textSize;
            text.alignment = textAlignment;
            text.color = textColor;

            if (!needOutline) return textObject;
            
            var outline = textObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            return textObject;
        }
        
        public static Button NewButton(string name, Vector2 position, Vector2 size, Vector3 scale, Sprite sprite, Transform parent)
        {
            var button = NewUIElement(name, parent, position, size, scale);
            var image = button.AddComponent<Image>();
            image.sprite = sprite;
            var _button = button.AddComponent<Button>();
            
            return _button;
        }
        
        public static Button NewButton(string name, Vector2 position, Vector2 size, Vector3 scale, Color32[] colors, Transform parent)
        {
            var button = NewUIElement(name, parent, position, size, scale);
           // var image = button.AddComponent<Image>();
            
            var _button = button.AddComponent<Button>();

            var buttonColors = _button.colors;
            buttonColors.normalColor = colors[0];
            buttonColors.highlightedColor = colors[1];
            buttonColors.pressedColor = colors[2];
            _button.colors = buttonColors;
            
            return _button;
        }

        public static Canvas NewCanvas(string name, Vector2 size)
        {
            var canvas = new GameObject(name);
            canvas.AddComponent<RectTransform>();
            var _canvas = canvas.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = size;
            canvas.AddComponent<GraphicRaycaster>();

            return _canvas;
        }
        
        public static void SetAndStretchToParentSize(RectTransform obj, RectTransform parent, bool fullScreen)
        {
            obj.anchoredPosition = parent.position;
            if(!fullScreen)
                obj.anchorMin = new Vector2(0, 0);
            
            else obj.anchorMin = new Vector2(-1, -1);
            obj.anchorMax = new Vector2(1, 1);
            obj.pivot = new Vector2(0.5f, 0.5f);
            obj.sizeDelta = Vector2.zero;
        }

       

        public static GameObject newCrosshairPart(string name, Vector2 positions, Vector2 size, GameObject parent)
        {
            GameObject crosshiarPart = NewUIElement(name, parent.transform, positions, size, Vector3.one);
            crosshiarPart.AddComponent<Image>().color = Color.white;
            Outline outline = crosshiarPart.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            return crosshiarPart;
        }

#endif
        
        public static EventSystem CreateEventManager(Transform parent)
        {
            var eventManager = new GameObject("EventSystem");
            var es =  eventManager.AddComponent<EventSystem>();
            eventManager.AddComponent<StandaloneInputModule>();
            eventManager.transform.parent = parent;
            eventManager.transform.localPosition = Vector3.zero;
            eventManager.transform.localEulerAngles = Vector3.zero;

            return es;
        }
        
        public static Canvas NewCanvas(string name, Transform parent)
        {
            var canvas = new GameObject(name);
            canvas.AddComponent<RectTransform>();
            var script = canvas.AddComponent<Canvas>();
            script.renderMode = RenderMode.WorldSpace;
            canvas.AddComponent<GraphicRaycaster>();
            canvas.transform.SetParent(parent);
            return script;
        }
        
        public static GameObject NewUIElement(string name, Transform parent, Vector2 position, Vector2 size, Vector3 scale)
        {
            var element = new GameObject(name);
            element.transform.SetParent(parent);
            var rectTransform = element.AddComponent<RectTransform>();
            element.AddComponent<CanvasRenderer>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            element.transform.localScale = scale;
            element.layer = 5;
            
            return element;
        }
        
        public static void HideIKObjects(bool value, HideFlags flag, Transform obj)
        {

            var renderer = obj.GetComponent<MeshRenderer>();
            renderer.enabled = !value;
            
            obj.hideFlags = flag;

            if (value)
            {
                if (obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }
            }
        }
        
        public static void HideAllObjects(WeaponsHelper.IKObjects IkObjects)
        {
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.RightObject);
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.LeftObject);

            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.RightAimObject);
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.LeftAimObject);

            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.RightWallObject);
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.LeftWallObject);

            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.RightElbowObject);
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.LeftElbowObject);
            
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.RightCrouchObject);
            HideIKObjects(true, HideFlags.HideInHierarchy, IkObjects.LeftCrouchObject);
        }

        public static void CreateObjects(WeaponsHelper.IKObjects IkObjects, Transform parent, bool adjusment, bool hide, float size, CubeSolid mode)
        {
            IkObjects.RightObject = NewObject(parent, "Right Hand Object", PrimitiveType.Cube, Color.red, size, mode);
            Object.Destroy(IkObjects.RightObject.GetComponent<BoxCollider>());
            if(!adjusment && hide)
                Object.Destroy(IkObjects.RightObject.GetComponent<MeshRenderer>());

            IkObjects.LeftObject = NewObject(parent, "Left Hand Object", PrimitiveType.Cube, Color.red, size, mode);
            Object.Destroy(IkObjects.LeftObject.GetComponent<BoxCollider>());
            if(!adjusment && hide)
                Object.Destroy(IkObjects.LeftObject.GetComponent<MeshRenderer>());

            IkObjects.RightAimObject = NewObject(parent, "Right Aim Object", PrimitiveType.Cube, Color.blue, size, mode);
            Object.Destroy(IkObjects.RightAimObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.RightAimObject.GetComponent<MeshRenderer>());

            IkObjects.LeftAimObject = NewObject(parent, "Left Aim Object", PrimitiveType.Cube, Color.blue, size, mode);
            Object.Destroy(IkObjects.LeftAimObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.LeftAimObject.GetComponent<MeshRenderer>());
            
            IkObjects.RightCrouchObject = NewObject(parent, "Right Crouch Object", PrimitiveType.Cube, Color.magenta, size, mode);
            Object.Destroy(IkObjects.RightCrouchObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.RightCrouchObject.GetComponent<MeshRenderer>());

            IkObjects.LeftCrouchObject = NewObject(parent, "Left Crouch Object", PrimitiveType.Cube, Color.magenta, size, mode);
            Object.Destroy(IkObjects.LeftCrouchObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.LeftCrouchObject.GetComponent<MeshRenderer>());


            IkObjects.RightWallObject = NewObject(parent, "Right Hand Wall Object", PrimitiveType.Cube, Color.yellow, size, mode);
            Object.Destroy(IkObjects.RightWallObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.RightWallObject.GetComponent<MeshRenderer>());


            IkObjects.LeftWallObject = NewObject(parent, "Left Hand Wall Object", PrimitiveType.Cube, Color.yellow, size, mode);
            Object.Destroy(IkObjects.LeftWallObject.GetComponent<BoxCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.LeftWallObject.GetComponent<MeshRenderer>());


            IkObjects.RightElbowObject = NewObject(parent, "Right Elbow Object", PrimitiveType.Sphere, Color.green, size, mode);
            Object.Destroy(IkObjects.RightElbowObject.GetComponent<SphereCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.RightElbowObject.GetComponent<MeshRenderer>());


            IkObjects.LeftElbowObject = NewObject(parent, "Left Elbow Object", PrimitiveType.Sphere, Color.green, size, mode);
            Object.Destroy(IkObjects.LeftElbowObject.GetComponent<SphereCollider>());
            if(!adjusment)
                Object.Destroy(IkObjects.LeftElbowObject.GetComponent<MeshRenderer>());

        }

        public static void HandsIK(Controller controller, WeaponController weaponController, InventoryManager weaponManager, Transform LeftIKObject, Transform RightIKObject, Transform leftParent, Transform rightParent, float value, bool pinObj)
        {
            var L_ikObj = LeftIKObject;
            var R_ikObj = RightIKObject;

            if (weaponController.CanUseElbowIK && !weaponController.CurrentWeaponInfo[weaponController.settingsSlotIndex].disableElbowIK)
            {
                controller.anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, value);
                controller.anim.SetIKHintPosition(AvatarIKHint.LeftElbow, weaponController.IkObjects.LeftElbowObject.position);

                controller.anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, value);
                controller.anim.SetIKHintPosition(AvatarIKHint.RightElbow, weaponController.IkObjects.RightElbowObject.position);
            }
            
            if (weaponController.setHandsPositionsAim && weaponController.setHandsPositionsObjectDetection && weaponController.setHandsPositionsCrouch && value >= 1)
            {
                if (controller.TypeOfCamera == CharacterHelper.CameraType.ThirdPerson && controller.anim.GetBool("HasWeaponTaken") || controller.TypeOfCamera != CharacterHelper.CameraType.ThirdPerson)
                {
                    R_ikObj.parent = rightParent;

                    if (!pinObj || weaponController.numberOfUsedHands == 1 || weaponController.isShotgun && controller.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
                    {
                        L_ikObj.parent = leftParent;
                    }
                    else
                    {
                        L_ikObj.parent = R_ikObj;
                    }
                }

            }
           
            if (value >= 0)
            {
                controller.anim.SetIKPositionWeight(AvatarIKGoal.RightHand, value);
                controller.anim.SetIKRotationWeight(AvatarIKGoal.RightHand, value);
            
                controller.anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, value);
                controller.anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, value);
                
                controller.anim.SetIKPosition(AvatarIKGoal.RightHand, R_ikObj.position);
                controller.anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(R_ikObj.eulerAngles));

                controller.anim.SetIKPosition(AvatarIKGoal.LeftHand, L_ikObj.position);
                controller.anim.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.Euler(L_ikObj.eulerAngles));
            }
        }

        private static void FingersRotation(Animator anim, float angle, HumanBodyBones finger, Vector3 axis)
        {
            anim.SetBoneLocalRotation(finger, anim.GetBoneTransform(finger).localRotation *= Quaternion.AngleAxis(angle, axis));
        }
        
        public static void FingersRotate(WeaponsHelper.WeaponInfo weaponInfo, Animator anim, string type)
        {
            if (type == "Weapon")
            {
                var leftAngleX = weaponInfo.FingersLeftX;
                var rightAngleX = weaponInfo.FingersRightX;

                var leftAngleY = weaponInfo.FingersLeftY;
                var rightAngleY = weaponInfo.FingersRightY;

                var leftAngleZ = weaponInfo.FingersLeftZ;
                var rightAngleZ = weaponInfo.FingersRightZ;

                var leftThumbAngleX = weaponInfo.ThumbLeftX;
                var rightThumbAngleX = weaponInfo.ThumbRightX;

                var leftThumbAngleY = weaponInfo.ThumbLeftY;
                var rightThumbAngleY = weaponInfo.ThumbRightY;

                var leftThumbAngleZ = weaponInfo.ThumbLeftZ;
                var rightThumbAngleZ = weaponInfo.ThumbRightZ;
                
                RotateFingersByAxis("X", leftAngleX, rightAngleX, leftThumbAngleX, rightThumbAngleX, anim, "Weapon");
                RotateFingersByAxis("Y", leftAngleY, rightAngleY, leftThumbAngleY, rightThumbAngleY, anim, "Weapon");
                RotateFingersByAxis("Z", leftAngleZ, rightAngleZ, leftThumbAngleZ, rightThumbAngleZ, anim, "Weapon");
            }
            else if (type == "Null")
            {
                RotateFingersByAxis("X", 0, 0, 0, 0, anim, "Reload");
                RotateFingersByAxis("Y", 0, 0, 0, 0, anim, "Reload");
                RotateFingersByAxis("Z", 0, 0, 0, 0, anim, "Reload");
            }
            else if  (type == "Grenade")
            {
                var leftAngleX = weaponInfo.FingersLeftX;
                var leftAngleY = weaponInfo.FingersLeftY;
                var leftAngleZ = weaponInfo.FingersLeftZ;
                
                var leftThumbAngleX = weaponInfo.ThumbLeftX;
                var leftThumbAngleY = weaponInfo.ThumbLeftY;
                var leftThumbAngleZ = weaponInfo.ThumbLeftY;

                RotateFingersByAxis("X", leftAngleX, 0, leftThumbAngleX, 0, anim, "Grenade");
                RotateFingersByAxis("Y", leftAngleY, 0, leftThumbAngleY, 0, anim, "Grenade");
                RotateFingersByAxis("Z", leftAngleZ, 0, leftThumbAngleZ, 0, anim, "Grenade");
            }
        }

        private static void RotateFingersByAxis(string axis, float leftAngle, float rightAngle, float leftThumbAngle,
            float rightThumbAngle, Animator anim, string type)
        {

            var axs = new Vector3();

            switch (axis)
            {
                case "X":
                    axs = Vector3.right;
                    break;
                case "Y":
                    axs = Vector3.up;
                    break;
                case "Z":
                    axs = Vector3.forward;
                    break;
            }

            if (type != "Grenade")
            {
                if (anim.GetBoneTransform(HumanBodyBones.RightIndexProximal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightIndexProximal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightIndexIntermediate, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightIndexDistal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightIndexDistal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightRingProximal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightRingProximal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightRingIntermediate, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightRingDistal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightRingDistal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightMiddleProximal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightMiddleIntermediate, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightMiddleDistal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightLittleProximal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightLittleProximal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightLittleIntermediate, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightLittleDistal))
                    FingersRotation(anim, rightAngle, HumanBodyBones.RightLittleDistal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightThumbProximal))
                    FingersRotation(anim, rightThumbAngle, HumanBodyBones.RightThumbProximal, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate))
                    FingersRotation(anim, rightThumbAngle, HumanBodyBones.RightThumbIntermediate, axs);

                if (anim.GetBoneTransform(HumanBodyBones.RightThumbDistal))
                    FingersRotation(anim, rightThumbAngle, HumanBodyBones.RightThumbDistal, axs);

            }


            //left fingers

            if (anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftIndexProximal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftIndexIntermediate, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftIndexDistal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftRingProximal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftRingProximal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftRingIntermediate, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftRingDistal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftRingDistal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftMiddleProximal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftMiddleIntermediate, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftMiddleDistal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftLittleProximal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftLittleIntermediate, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal))
                FingersRotation(anim, leftAngle, HumanBodyBones.LeftLittleDistal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal))
                FingersRotation(anim, leftThumbAngle, HumanBodyBones.LeftThumbProximal, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate))
                FingersRotation(anim, leftThumbAngle, HumanBodyBones.LeftThumbIntermediate, axs);

            if (anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal))
                FingersRotation(anim, leftThumbAngle, HumanBodyBones.LeftThumbDistal, axs);

        }

        public static class ChangeShaderMode
        {
            public enum BlendMode
            {
                Opaque,
                Cutout,
                Fade,
                Transparent
            }

            public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
            {
                switch (blendMode)
                {
                    case BlendMode.Opaque:
                        standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                        standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                        standardShaderMaterial.SetInt("_ZWrite", 1);
                        standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        standardShaderMaterial.renderQueue = -1;
                        break;
                    case BlendMode.Cutout:
                        standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                        standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                        standardShaderMaterial.SetInt("_ZWrite", 1);
                        standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        standardShaderMaterial.renderQueue = 2450;
                        break;
                    case BlendMode.Fade:
                        standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                        standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        standardShaderMaterial.SetInt("_ZWrite", 0);
                        standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                        standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        standardShaderMaterial.renderQueue = 3000;
                        break;
                    case BlendMode.Transparent:
                        standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                        standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        standardShaderMaterial.SetInt("_ZWrite", 0);
                        standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                        standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                        standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        standardShaderMaterial.renderQueue = 3000;
                        break;
                }

            }
        }

        public static string GetHtmlFromUrl(string resource)
        {

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return "";
            }
            else
            {
                return "connected";
            }

            // var html = string.Empty;
            //
            // var req = (HttpWebRequest)WebRequest.Create(resource);
            //
            // try
            // {
            //     using (var resp = (HttpWebResponse) req.GetResponse())
            //     {
            //         var isSuccess = (int) resp.StatusCode < 299 && (int) resp.StatusCode >= 200;
            //         if (isSuccess)
            //         {
            //             using (var reader = new StreamReader(resp.GetResponseStream()))
            //             {
            //                 var cs = new char[80];
            //                 reader.Read(cs, 0, cs.Length);
            //                 foreach (var ch in cs)
            //                 {
            //                     html += ch;
            //                 }
            //             }
            //         }
            //     }
            // }
            // catch
            // {
            //     return "";
            // }
            //
            // return html;
        }
        
        public static bool CheckConnection(string URL)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Timeout = 5000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
 
                if (response.StatusCode == HttpStatusCode.OK) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        public static bool canSeeObject (GameObject go, Camera camera) {

            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var point = go.transform.position;
            return planes.All(plane => !(plane.GetDistanceToPoint(point) < 0));
        }
        
        public static bool HasLayer(this LayerMask layerMask, int layer)
        {
            if (layerMask == (layerMask | (1 << layer)))
            {
                return true;
            }
 
            return false;
        }
 
        public static bool[] HasLayers(this LayerMask layerMask)
        {
            var hasLayers = new bool[32];
 
            for (int i = 0; i < 32; i++)
            {
                if (layerMask == (layerMask | (1 << i)))
                {
                    hasLayers[i] = true;
                }
            }
 
            return hasLayers;
        }

        public static string CorrectName(string currentName)
        {
            var name = currentName;
					
            if (name.Contains("(Clone)"))
            {
                var replace = name.Replace("(Clone)", "");
                name = replace;
            }

            return name;
        }
        
        
        private static int Bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }
 
        public static Color GetAreaColor(int i)
        {
            if (i == 0)
                return new Color(0, 0.75f, 1.0f, 0.5f);
            int r = (Bit(i, 4) + Bit(i, 1) * 2 + 1) * 63;
            int g = (Bit(i, 3) + Bit(i, 2) * 2 + 1) * 63;
            int b = (Bit(i, 5) + Bit(i, 0) * 2 + 1) * 63;
            return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 0.5f);
        }
        
        public static void SwitchGizmo<T>(bool value)
        {
            var typeName = typeof(T).Name;
            var annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var classId = annotation.GetField("classID");
            var scriptClass = annotation.GetField("scriptClass");
 
            var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var getAnnotations = annotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var setGizmoEnabled = annotationUtility.GetMethod("SetGizmoEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var setIconEnabled = annotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
 
            var annotations = (Array)getAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                var scriptClassValue = (string)scriptClass.GetValue(a);
                if (scriptClassValue.Equals(typeName) == false) continue;
                var classIdValue = (int)classId.GetValue(a);
 
                setGizmoEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0, false });
                setIconEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0 });
                break;
            }
        }
        
#if UNITY_EDITOR
        // [InitializeOnLoad]
        class SceneClosingCallbackTest
        {
            static SceneClosingCallbackTest()
            {
                UnityEditor.SceneManagement.EditorSceneManager.sceneClosing += SceneClosing;
                UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += SceneClosed;
                UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += SceneOpening;
                UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpened;
                UnityEditor.SceneManagement.EditorSceneManager.newSceneCreated += NewSceneCreated;
            }
 
            static void SceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
            {
            }
 
            static void SceneClosed(UnityEngine.SceneManagement.Scene scene)
            {
                // if (scene.name == "UI Manager")
                // {
                //     if (PlayerPrefs.HasKey("DefaultLayoutPath"))
                //     {
                //         LayoutUtility.LoadLayoutFromAsset(PlayerPrefs.GetString("DefaultLayoutPath"));
                //         
                //         Selection.activeGameObject = null;
                //     }
                // }
            }
 
            static void SceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
            {
            }
 
            static void SceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
            {
                // if (scene.name == "UI Manager")
                // {
                //     var uiManager = Object.FindObjectOfType<UIManager>();
                //     uiManager.currentMenuPage = UIHelper.MenuPages.MainMenu;
                //     
                //     var layoutPath = AssetDatabase.GetAssetPath(uiManager.defaultLayout);
                //     LayoutUtility.SaveLayout(layoutPath);
                //     
                //     PlayerPrefs.SetString("DefaultLayoutPath", layoutPath);
                //     
                //     layoutPath = AssetDatabase.GetAssetPath(uiManager.uiManagerLayout);
                //     LayoutUtility.LoadLayoutFromAsset(layoutPath);
                //     
                //     Selection.activeGameObject = null;
                // }
            }
 
            static void NewSceneCreated(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.NewSceneSetup setup, UnityEditor.SceneManagement.NewSceneMode mode)
            {
            }
        }

        public static class LayoutUtility
        {
            private static MethodInfo _miLoadWindowLayout;
            private static MethodInfo _miSaveWindowLayout;
            private static MethodInfo _miReloadWindowLayoutMenu;
            private static bool _available;
            private static string _layoutsPath;

            static LayoutUtility()
            {
                Type tyWindowLayout = Type.GetType("UnityEditor.WindowLayout,UnityEditor");
                Type tyEditorUtility = Type.GetType("UnityEditor.EditorUtility,UnityEditor");
                Type tyInternalEditorUtility = Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor");
                if (tyWindowLayout != null && tyEditorUtility != null && tyInternalEditorUtility != null)
                {
                    MethodInfo miGetLayoutsPath = tyWindowLayout.GetMethod("GetLayoutsPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    _miLoadWindowLayout = tyWindowLayout.GetMethod("LoadWindowLayout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] {typeof(string), typeof(bool)}, null);
                    _miSaveWindowLayout = tyWindowLayout.GetMethod("SaveWindowLayout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] {typeof(string)}, null);
                    _miReloadWindowLayoutMenu = tyInternalEditorUtility.GetMethod("ReloadWindowLayoutMenu", BindingFlags.Public | BindingFlags.Static);

                    if (miGetLayoutsPath == null || _miLoadWindowLayout == null || _miSaveWindowLayout == null || _miReloadWindowLayoutMenu == null)
                        return;

                    _layoutsPath = (string) miGetLayoutsPath.Invoke(null, null);
                    if (string.IsNullOrEmpty(_layoutsPath))
                        return;

                    _available = true;
                }
            }

            // Gets a value indicating whether all required Unity API functionality is available for usage.
            public static bool IsAvailable
            {
                get { return _available; }
            }

            // Gets absolute path of layouts directory. Returns `null` when not available.
            public static string LayoutsPath
            {
                get { return _layoutsPath; }
            }

            // Save current window layout to asset file. `assetPath` must be relative to project directory.
            public static void SaveLayoutToAsset(string assetPath)
            {
                SaveLayout(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
            }

            // Load window layout from asset file. `assetPath` must be relative to project directory.
            public static void LoadLayoutFromAsset(string assetPath)
            {
                if (_miLoadWindowLayout != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    _miLoadWindowLayout.Invoke(null, new object[] {path, true});
                }
            }

            // Save current window layout to file. `path` must be absolute.
            public static void SaveLayout(string path)
            {
                if (_miSaveWindowLayout != null)
                    _miSaveWindowLayout.Invoke(null, new object[] {path});
            }
        }
#endif

        // Copyright 2014 Jarrah Technology (http://www.jarrahtechnology.com). All Rights Reserved. 
        public static class CameraExtensions {

            public static void LayerCullingShow(Camera cam, int layerMask) {
                cam.cullingMask |= layerMask;
            }

            public static void LayerCullingShow(Camera cam, string layer) {
                LayerCullingShow(cam, 1 << UnityEngine.LayerMask.NameToLayer(layer));
            }

            public static void LayerCullingHide(Camera cam, int layerMask) {
                cam.cullingMask &= ~layerMask;
            }

            public static void LayerCullingHide(Camera cam, string layer) {
                LayerCullingHide(cam, 1 << UnityEngine.LayerMask.NameToLayer(layer));
            }

            public static void LayerCullingToggle(Camera cam, int layerMask) {
                cam.cullingMask ^= layerMask;
            }

            public static void LayerCullingToggle(Camera cam, string layer) {
                LayerCullingToggle(cam, 1 << UnityEngine.LayerMask.NameToLayer(layer));
            }

            public static bool LayerCullingIncludes(Camera cam, int layerMask) {
                return (cam.cullingMask & layerMask) > 0;
            }

            public static bool LayerCullingIncludes(Camera cam, string layer) {
                return LayerCullingIncludes(cam, 1 << UnityEngine.LayerMask.NameToLayer(layer));
            }

            public static void LayerCullingToggle(Camera cam, int layerMask, bool isOn) {
                var included = LayerCullingIncludes(cam, layerMask);
                if (isOn && !included) {
                    LayerCullingShow(cam, layerMask);
                } else if (!isOn && included) {
                    LayerCullingHide(cam, layerMask);
                }
            }

            public static void LayerCullingToggle(Camera cam, string layer, bool isOn) {
                LayerCullingToggle(cam, 1 << UnityEngine.LayerMask.NameToLayer(layer), isOn);
            }
        }
        //
    }
}


