using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [InitializeOnLoad]
    public class CustomHierarchy : MonoBehaviour
    {
        static CustomHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            Color fontColor = Color.blue;
            Color backgroundColor = new Color(.76f, .76f, .76f);

            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj != null)
            {
                var offsetRect = new Rect(selectionRect.position + new Vector2(selectionRect.size.x - 10, 0), selectionRect.size);
                
                // var prefabType = PrefabUtility.GetPrefabType(obj);
                // if (prefabType == PrefabType.PrefabInstance)
                
                if (obj.GetComponent<SpawnZone>())
                {
                    var script = obj.GetComponent<SpawnZone>();

                    fontColor = script.color;
                    var offsetRect1 = new Rect(selectionRect.position + new Vector2(selectionRect.size.x - 27, 0), selectionRect.size);

                    EditorGUI.LabelField(offsetRect1, "●", new GUIStyle
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = FontStyle.Bold,
                            fontSize = 15
                        }
                    );
                    
                    var tex = Resources.Load("Spawn Zone Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<MovementBehavior>())
                {
                    var tex = Resources.Load("Movement Behaviour Icon 2") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<Controller>())
                {
                    var tex = Resources.Load("Controller Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<PickupItem>())
                {
                    var tex = Resources.Load("PickUp Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));

                    if (obj.GetComponent<WeaponController>())
                    {
                        var offsetRect1 = new Rect(selectionRect.position + new Vector2(selectionRect.size.x - 30, 0), selectionRect.size);
                        tex = Resources.Load("WeaponController Icon 2") as Texture2D;
                        EditorGUI.LabelField(offsetRect1, new GUIContent(tex));
                    }
                }
                else if (obj.GetComponent<AIArea>())
                {
                    var script = obj.GetComponent<AIArea>();
                    var textIcon = "○";
                    fontColor = Color.black;
                    
                    var offsetRect1 = new Rect(selectionRect.position + new Vector2(selectionRect.size.x - 27, 0), selectionRect.size);
                    if (script.navMeshData)
                    {
                        fontColor = Helper.GetAreaColor(script.defaultArea);
                        textIcon = "●";
                    }

                    EditorGUI.LabelField(offsetRect1, textIcon, new GUIStyle
                        {
                            normal = new GUIStyleState {textColor = fontColor},
                            fontStyle = FontStyle.Bold,
                            fontSize = 15
                        }
                    );
                    
                    var tex = Resources.Load("AI Area Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<WeaponController>())
                {
                    var tex = Resources.Load("Weapon Controller Icon 2") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<GameManager>())
                {
                    var tex = Resources.Load("GameManager Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<AIController>())
                {
                    var tex = Resources.Load("EnemyController Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<Cover>())
                {
                    var tex = Resources.Load("Cover Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<StealthZone>())
                {
                    var tex = Resources.Load("StealthZone Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<UIManagerItemIcon>())
                {
                    var script = obj.GetComponent<UIManagerItemIcon>();
                    // var offsetRect = new Rect(selectionRect.min, selectionRect.size);
                    
                    var tex = Resources.Load(script.itemIcon.name) as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                
#if USK_ADVANCED_MULTIPLAYER
                else if (obj.GetComponent<AdvancedRoomManager>())
                {
                    var tex = Resources.Load("AdvancedRoomManager Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<AdvancedLobbyManager>())
                {
                    var tex = Resources.Load("AdvancedLobbyManager Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<BattleZone>())
                {
                    var script = obj.GetComponent<BattleZone>();

                    fontColor = script.debugColor;
                    var offsetRect1 = new Rect(selectionRect.position + new Vector2(selectionRect.size.x - 27, 0), selectionRect.size);

                    EditorGUI.LabelField(offsetRect1, "●", new GUIStyle
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = FontStyle.Bold,
                            fontSize = 15
                        }
                    );
                    
                    var tex = Resources.Load("BattleZone Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
                else if (obj.GetComponent<CapturePoint>())
                {
                    var tex = Resources.Load("CapturePoint Icon") as Texture2D;
                    EditorGUI.LabelField(offsetRect, new GUIContent(tex));
                }
#endif
            }
        }
    }
}