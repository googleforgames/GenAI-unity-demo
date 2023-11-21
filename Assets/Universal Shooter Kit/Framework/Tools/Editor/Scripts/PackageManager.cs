using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class PackageManager : EditorWindow
    {
        
        private static Texture2D punIcon;
        private static Texture2D advaneMultIcon;
        private static Texture2D RCCIcon;
        private static Texture2D EVPHIcon;
        private static Texture2D NWHIcon;
        private static Texture2D emAIIcon;
        private static Texture2D destroyIcon;
        private static Texture2D easySaveIcon;
        
        private static bool isInited;
        
        GUIStyle linkStyle;
        GUIStyle labelStyle;
        // GUIStyle indicatorStyle;
        GUIStyle statusStyle;
        GUIStyle descriptionStyle;
        GUIStyle generalLabel;
        GUIStyle generaText;
        GUIStyle tabStyle;

        private Helper.EditorColors editorColorsLightTheme = new Helper.EditorColors();
        private Helper.EditorColors editorColorsDarkTheme = new Helper.EditorColors();

        private string lastButtonClickedName;
        private string photonPath;
        private string advancedMultiplayerPath;
        private string rccPath;
        private string rccAdditionalPath;
        private string EvphPath;
        private string NWHPath;
        private string emAIPath;
        private string destroyPath;
        private string easySavePath;

        private GUISkin customSkin;

        private int inspectorTab;
        private int lastInspectorTab;

        [MenuItem("Tools/Universal Shooter Kit/Integrations Manager #m", false, 100)]
        public static void ShowWindow()
        {
            GetWindowWithRect(typeof(PackageManager), new Rect(Vector2.zero, new Vector2(800, 600)), true, "").ShowUtility();
        }
        
        void OnEnable()
        {
            Init();
            EditorApplication.update -= Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }
        
        private void Update()
        {
            if (lastInspectorTab != inspectorTab)
            {
                lastInspectorTab = inspectorTab;
                PlayerPrefs.SetInt("PM_Tab", inspectorTab);
            }
        }

        void Init()
        {
            if (PlayerPrefs.HasKey("PM_Tab"))
                inspectorTab = PlayerPrefs.GetInt("PM_Tab");
            
            customSkin = (GUISkin) Resources.Load("EditorSkin");
            tabStyle = customSkin.GetStyle("Tab");

            if (EditorGUIUtility.isProSkin)
            {
                customSkin.customStyles[0].onNormal.background = Resources.Load("TabSelected") as Texture2D;
                customSkin.customStyles[0].onNormal.textColor = Color.white;
            }
            else
            {
                customSkin.customStyles[0].onNormal.background = Resources.Load("TabSelected2") as Texture2D;
                customSkin.customStyles[0].onNormal.textColor = Color.black;
            }
            
            punIcon = Resources.Load("PUN Logo") as Texture2D;
            advaneMultIcon = Resources.Load("Advanced Multiplayer Logo") as Texture2D;
            RCCIcon = Resources.Load("RCC Logo") as Texture2D;
            EVPHIcon = Resources.Load("EVPH Logo") as Texture2D;
            NWHIcon = Resources.Load("NWH VP Logo") as Texture2D;
            emAIIcon = Resources.Load("EmeraldAI Logo") as Texture2D;
            destroyIcon = Resources.Load("DestroyIt Logo") as Texture2D;
            easySaveIcon = Resources.Load("EasySave Logo") as Texture2D;

            editorColorsLightTheme.linkColor = Color.black;//new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            editorColorsDarkTheme.linkColor = Color.white;//new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);

            editorColorsLightTheme.textColor = Color.black;
            editorColorsDarkTheme.textColor = Color.white;

            editorColorsLightTheme.statusColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            editorColorsDarkTheme.statusColor = Color.yellow;
            
            labelStyle = new GUIStyle
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontStyle = FontStyle.Bold, fontSize = 14, alignment  = TextAnchor.MiddleLeft
            };
            
            // indicatorStyle = new GUIStyle
            // {
            //     normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter
            // };

            linkStyle = new GUIStyle(labelStyle)
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.linkColor : editorColorsDarkTheme.linkColor}
            };

            statusStyle = new GUIStyle
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.statusColor : editorColorsDarkTheme.statusColor}, fontStyle = FontStyle.Bold, fontSize = 11
            };

            descriptionStyle = new GUIStyle
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontSize = 13
            };
            
            generalLabel = new GUIStyle
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontStyle = FontStyle.Bold, fontSize = 17, alignment  = TextAnchor.MiddleCenter
            };
            
            generaText = new GUIStyle
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontSize = 14, alignment  = TextAnchor.MiddleCenter
            };
            
            // initialize components
            
            var results = AssetDatabase.FindAssets("PhotonServerSettings t:ScriptableObject");
            var asset = "";
            
            if (results.Length > 0)
            {
                asset = results[0];
                photonPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = photonPath.Replace("PhotonUnityNetworking/Resources/PhotonServerSettings.asset", "");
                photonPath = replace;
            }
           
            results = AssetDatabase.FindAssets("AdvancedLobbyManager t:MonoScript");
            if (results.Length > 0)
            {
                foreach (var result in results)
                {
                    if (result != "AdvancedLobbyManagerEditor")
                        asset = result;
                }
                
                advancedMultiplayerPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = advancedMultiplayerPath.Replace("Tools/Main Scripts/AdvancedLobbyManager.cs", "");
                advancedMultiplayerPath = replace;
            }
            
            results = AssetDatabase.FindAssets("RCC_CarControllerV3 t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                rccPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = rccPath.Replace("Scripts/RCC_CarControllerV3.cs", "");
                rccPath = replace;
            }
            
            results = AssetDatabase.FindAssets("BCG_EnterExitCharacterUICanvas t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                rccAdditionalPath = AssetDatabase.GUIDToAssetPath(asset);
            }

            results = AssetDatabase.FindAssets("VehicleViewConfig t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                EvphPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = EvphPath.Replace("Scripts/VehicleViewConfig.cs", "");
                EvphPath = replace;
            }
            
            results = AssetDatabase.FindAssets("VehicleControllerEditor t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                NWHPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = NWHPath.Replace("Scripts/VehicleController/Editor/VehicleControllerEditor.cs", "");
                NWHPath = replace;
            }
            
            results = AssetDatabase.FindAssets("EmeraldAIHideMouse t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                emAIPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = emAIPath.Replace("Scripts/Character Controller/EmeraldAIHideMouse.cs", "");
                emAIPath = replace;
            }
            
            results = AssetDatabase.FindAssets("PoolAfter t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                destroyPath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = destroyPath.Replace("Scripts/Behaviors/PoolAfter.cs", "");
                destroyPath = replace;
            }
            
            results = AssetDatabase.FindAssets("ES3JSONReader t:MonoScript");
            if (results.Length > 0)
            {
                asset = results[0];
                easySavePath = AssetDatabase.GUIDToAssetPath(asset);
                var replace = easySavePath.Replace("Scripts/Readers/ES3JSONReader.cs", "");
                easySavePath = replace;
            }
        }
        

        private void OnGUI()
        {
            
            GUILayout.Space(15f);

            GUILayout.Label("Integrations Manager", generalLabel);
            GUILayout.Space(8);

            generaText.fontStyle = FontStyle.Normal;
            GUILayout.Label("Use this manager to activate and deactivate different game modules and integrations.", generaText);
            // GUILayout.Space(8);
            
            // generaText.fontSize = 11;
            // generaText.fontStyle = FontStyle.Bold;
            
            // var backgroundColor = GUI.backgroundColor;
            // GUI.backgroundColor = new Color(0,1,0,0.5f);
            
            // GUILayout.BeginVertical("helpbox");
            // GUILayout.Space(2);
            // GUILayout.Label("Please note:", text);
            // GUILayout.Space(3);
            // generaText.fontStyle = FontStyle.Normal;
            // GUILayout.Label("- Do not activate an add-on if you have not downloaded files for it -", text);
            // GUILayout.Space(2);
            // GUILayout.Label( "(After pressing [Activate] | [Deactivate] buttons, wait a few seconds until all scripts are recompiled)", generaText);
            
            // GUILayout.Space(5);
            // GUILayout.EndVertical();
            
            // GUI.backgroundColor = backgroundColor;
            GUILayout.Space(15);

            inspectorTab = GUILayout.Toolbar(inspectorTab, new[] {"Multiplayer", "Vehicles", "AI", "Game Features", "Tools"}, tabStyle);
            
            GUILayout.Space(10);

            // backgroundColor = GUI.backgroundColor;
            // GUI.backgroundColor = new Color32(255,255,255,255);
            // GUILayout.BeginVertical("helpbox");
            // GUI.backgroundColor = backgroundColor;
            switch (inspectorTab)
            {
                case 0:
                    
                    
                    
#if USK_MULTIPLAYER
            DrawLine(punIcon, "PUN 2", ("A basic framework that is required for all multiplayer functions to work."), "Enabled", "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922", photonPath);
                    
#if USK_ADVANCED_MULTIPLAYER
                    DrawLine(advaneMultIcon, "Advanced Multiplayer", ("A module that greatly expands the basic multiplayer features."), "Enabled", "https://u3d.as/2H8Q", advancedMultiplayerPath);
#else
                    DrawLine(advaneMultIcon, "Advanced Multiplayer", ("A module that greatly expands the basic multiplayer features."), "Disabled", "https://u3d.as/2H8Q", advancedMultiplayerPath);
#endif
                    
#else
                    DrawLine(punIcon, "PUN 2", ("A basic framework that is required for all multiplayer functions to work."), "Disabled", "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922", photonPath);

                    DrawLine(advaneMultIcon, "Advanced Multiplayer", !string.IsNullOrWhiteSpace(advancedMultiplayerPath) ? "To work with this add-on you need to activate <b>PUN2</b> first." : "A module that greatly expands the basic multiplayer features.", "Disabled", "https://u3d.as/2H8Q", advancedMultiplayerPath);

#endif
                    break;
                
                case 1:
                    
                    // text.fontSize = 13;
                    // GUILayout.Label("Integrations with this systems allows you to add cars and other vehicles to your game.", text);
                    // GUILayout.Space(5);

                   
#if USK_RCC_INTEGRATION
                    DrawLine(RCCIcon, "Realistic Car Controller", "", "Enabled", "https://assetstore.unity.com/packages/tools/physics/realistic-car-controller-16296", rccPath);
#else
                    if(!string.IsNullOrWhiteSpace(rccAdditionalPath))
                        DrawLine(RCCIcon, "Realistic Car Controller", "", "Disabled", "https://assetstore.unity.com/packages/tools/physics/realistic-car-controller-16296", rccPath);
                    else
                        DrawLine(RCCIcon, "Realistic Car Controller", (!string.IsNullOrEmpty(rccPath) ? "To work with this kit you also need to import the <b>BCG Shared Assets</b>." : ""), "Disabled", "https://assetstore.unity.com/packages/tools/physics/realistic-car-controller-16296", rccPath);

#endif
            
#if USK_EVPH_INTEGRATION
                    DrawLine(EVPHIcon, "Edy's Vehicle Physics", "", "Enabled", "https://assetstore.unity.com/packages/tools/physics/edy-s-vehicle-physics-403", EvphPath);
#else
                    DrawLine(EVPHIcon, "Edy's Vehicle Physics", "", "Disabled", "https://assetstore.unity.com/packages/tools/physics/edy-s-vehicle-physics-403", EvphPath);
#endif
                    
#if USK_NWHVPH_INTEGRATION
                    DrawLine(NWHIcon, "NWH Vehicle Physics 2", "", "Enabled", "https://assetstore.unity.com/packages/tools/physics/nwh-vehicle-physics-2-166252", NWHPath);
#else
                    DrawLine(NWHIcon, "NWH Vehicle Physics 2", "", "Disabled", "https://assetstore.unity.com/packages/tools/physics/nwh-vehicle-physics-2-166252", NWHPath);
#endif
                    break;
                
                case 2:
                    
#if USK_EMERALDAI_INTEGRATION
                    DrawLine(emAIIcon, "Emerald AI 3.0", "", "Enabled", "https://assetstore.unity.com/packages/tools/ai/emerald-ai-3-0-203904", emAIPath);
#else
                    DrawLine(emAIIcon, "Emerald AI 3.0", "", "Disabled", "https://assetstore.unity.com/packages/tools/ai/emerald-ai-3-0-203904", emAIPath);
#endif 
                    
                    break;
                
                case 3:
                    
#if USK_DESTROYIT_INTEGRATION
                    DrawLine(destroyIcon, "Destroy It", "", "Enabled", "https://assetstore.unity.com/packages/tools/physics/destroyit-destruction-system-18811", destroyPath);
#else
                    DrawLine(destroyIcon, "Destroy It", "", "Disabled", "https://assetstore.unity.com/packages/tools/physics/destroyit-destruction-system-18811", destroyPath);
#endif 
                    
                    break;
                
                
                case 4:
                    
#if USK_EASYSAVE_INTEGRATION
                    DrawLine(easySaveIcon, "Easy Save", "It will give you more saving options than the standard saves to json files.", "Enabled", "https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-data-serialization-system-768", easySavePath);
#else
                    DrawLine(easySaveIcon, "Easy Save", "It will give you more saving options than the standard saves to json files.", "Disabled", "https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-data-serialization-system-768", easySavePath);
#endif 
                    
                    break;
            }
            
            
            
            GUILayout.Space(170);
            // GUILayout.EndVertical();

            // generaText.fontSize = 11;
            // GUI.backgroundColor = new Color32(0,255,0,(byte)(!EditorGUIUtility.isProSkin ? 100 : 255));
            // GUILayout.BeginVertical("helpbox");
            // GUILayout.Space(2);
            // generaText.fontStyle = FontStyle.Normal;
            // GUILayout.Label( "After pressing [Activate] | [Deactivate] buttons, wait a few seconds until all scripts are recompiled !", generaText);
            // GUILayout.Space(2);
            // GUILayout.EndVertical();
            
            // GUI.backgroundColor = backgroundColor;
        }

        string CorrectDescriptionLenght(string description)
        {
            if (description.Length < 73)
            {
                var lenght = description.Length;
                var additionalText = "";
                var additionalLenght = 74 - lenght;
               
                for (var i = 0; i <= additionalLenght; i++)
                {
                    additionalText += " ";
                }

                description += additionalText;
            }

            return description;
        }


        void DrawLine(Texture2D icon, string title = "", string description = "", string status = "", string link = "", string path = "")
        {
            var _backgroundColor = GUI.backgroundColor;

            // if(!EditorGUIUtility.isProSkin)
            //     GUI.backgroundColor = new Color32(0,0,0,70);

            GUILayout.BeginHorizontal("helpbox");
            GUI.backgroundColor = _backgroundColor;

            // if (!string.IsNullOrEmpty(description))
            //     description = CorrectDescriptionLenght(description);

            GUILayout.Space(15);
            
            GUILayout.BeginVertical();
            GUILayout.Space(string.IsNullOrEmpty(description) ? 2 : 13);
            GUILayout.Box(icon, GUIStyle.none, GUILayout.MaxWidth(70), GUILayout.MaxHeight(70));
            var iconRect = GUILayoutUtility.GetLastRect();
            
            
            GUILayout.Space(string.IsNullOrEmpty(description) ? 2 : 13);
            
            GUILayout.EndVertical();
            
            // GUILayout.Space(15);

            // GUILayout.BeginVertical();
            // GUILayout.Space(5f);

            if (link != "")
            {
                var label = "" + title;

                // EditorGUILayout.BeginHorizontal();
                if (Helper.LinkLabel(new GUIContent(label), new Rect(new Vector2(iconRect.x + 100, iconRect.y - (!string.IsNullOrEmpty(description) ? 6 : - 5)), new Vector2(100,100)),  linkStyle.CalcSize(new GUIContent(label)).x, linkStyle, true, true, true,  false))
                {
                    Application.OpenURL(link);
                }
                // EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUI.Label(new Rect(new Vector2(iconRect.x + 100, iconRect.y - (!string.IsNullOrEmpty(description) ? 6 : - 5)), new Vector2(100,100)),"" + title, labelStyle);
            }
            
            var lastRect = GUILayoutUtility.GetLastRect();

            if (!string.IsNullOrEmpty(description))
            {
                GUI.Label(new Rect(new Vector2(iconRect.x + 100, lastRect.y + 33), new Vector2(100,100)), description, descriptionStyle);
                // GUILayout.Space(5);
            }


            var tex = (Texture2D) Resources.Load("manual 3");
            EditorGUI.LabelField(new Rect(new Vector2(iconRect.x + 100, lastRect.y + (!string.IsNullOrEmpty(description) ? 52 : 32)), new Vector2(18,18)), new GUIContent(tex, ""));
            
            if (Helper.LinkLabel(new GUIContent("Manual for use"), new Rect(new Vector2(iconRect.x + 122, lastRect.y + (!string.IsNullOrEmpty(description) ? 53 : 33)), new Vector2(100,100)), descriptionStyle.CalcSize(new GUIContent("Manual for use")).x, descriptionStyle, false, true, true, false))
            {
                var _link = "";
                
                switch (title)
                {
                    case "PUN 2":
                        _link = "https://docs.gercstudio.com/multiplayer/overview";
                        break;
                    case "Advanced Multiplayer":
                        _link = "https://gerc-studio.gitbook.io/advanced-multiplayer/";
                        break;
                    case "Realistic Car Controller":
                        _link = "https://docs.gercstudio.com/integrations-manager/realistic-car-controller";
                        break;
                    case "Edy's Vehicle Physics":
                        _link = "https://docs.gercstudio.com/integrations-manager/edys-vehicle-physics";
                        break; 
                    case "NWH Vehicle Physics 2":
                        _link = "https://docs.gercstudio.com/integrations-manager/nwh-vehicle-physics-2";
                        break;
                    case "Emerald AI 3.0":
                        _link = "https://docs.gercstudio.com/integrations-manager/emerald-ai";
                        break;
                    case "Destroy It":
                        _link = "https://docs.gercstudio.com/integrations-manager/destroy-it";
                        break;
                    case "Easy Save":
                        _link = "";
                        break;
                }
                
                Application.OpenURL(_link);
            }
            
            // EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(path));

           
                tex = (Texture2D) Resources.Load("demo");
                EditorGUI.LabelField(new Rect(new Vector2(iconRect.x + 230, lastRect.y + (!string.IsNullOrEmpty(description) ? 52 : 32)), new Vector2(19, 19)), new GUIContent(tex, ""));

                if (Helper.LinkLabel(new GUIContent("Demo Scene"), new Rect(new Vector2(iconRect.x + 252, lastRect.y + (!string.IsNullOrEmpty(description) ? 53 : 33)), new Vector2(100, 100)), descriptionStyle.CalcSize(new GUIContent("Demo Scene")).x, descriptionStyle, false, !string.IsNullOrEmpty(path), true, false))
                {
                    var scene = "";

                    switch (title)
                    {
                        case "PUN 2":
                            scene = "Lobby";
                            break;
                        case "Advanced Multiplayer":
                            scene = "Advanced Lobby";
                            break;
                        case "Realistic Car Controller":
                            scene = "USK & Realistic Car Controller";
                            break;
                        case "Edy's Vehicle Physics":
                            scene = "USK & Edy's Vehicle Physics";
                            break;
                        case "NWH Vehicle Physics 2":
                            scene = "USK & NWH Vehicle Phycics 2";
                            break;
                        case "Emerald AI 3.0":
                            scene = "USK & Emerald AI";
                            break;
                        case "Destroy It":
                            scene = "USK & Destroy It";
                            break;
                        case "Easy Save":
                            scene = "Save System (Scene 1)";
                            break;
                    }

                    EditorGUIUtility.PingObject(Resources.Load(scene) as SceneAsset);
                }
            
            // EditorGUI.EndDisabledGroup();


            // GUILayout.Space(10);
            
            if (!string.IsNullOrEmpty(path))
            {
                var text = "";

                if (title == "Advanced Multiplayer")
                {
                    var txt = (TextAsset)Resources.Load("AM_Version", typeof(TextAsset));

                    if (txt)
                    {
                        text = "Downloaded (" + path + ") <color=green>Version: " + txt + "</color>";
                    }
                    else
                    {
                        text = "Downloaded (" + path + ")";
                    }
                }
                else
                {
                    text = "Downloaded (" + path + ")";
                }

                GUI.Label(new Rect(new Vector2(iconRect.x + 100, lastRect.y + (!string.IsNullOrEmpty(description) ? 76 : 57)), new Vector2(100,100)), text, statusStyle);
            }
            else
            {
                GUI.Label(new Rect(new Vector2(iconRect.x + 100, lastRect.y + (!string.IsNullOrEmpty(description) ? 76 : 57)), new Vector2(100,100)),"Not downloaded", statusStyle);
            }

            // GUILayout.Space(5);

            // GUILayout.EndVertical();

            GUILayout.BeginVertical();
            
            if (string.IsNullOrEmpty(description)) GUILayout.Space(13.5f);
            else GUILayout.Space(25);
            
            var newStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = {textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor}, fontStyle = FontStyle.Bold, fontSize = 11, alignment = TextAnchor.MiddleCenter
            };

            if (status == "Disabled")
            {
                // indicatorStyle.normal.textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor;

                var backgroundColor = GUI.backgroundColor;

                if (EditorApplication.isCompiling && lastButtonClickedName == title)
                {
                    GUI.backgroundColor = Color.yellow;
                    status = "Compiling Scripts...";
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }

                // GUILayout.BeginHorizontal()

                GUI.TextArea(new Rect(new Vector2(610, iconRect.y + 10), new Vector2(180,20)), status, newStyle);
                // GUILayout.EndVertical();

                GUI.backgroundColor = backgroundColor;

                // GUILayout.Space(3);

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(path) || title == "Realistic Car Controller" && string.IsNullOrWhiteSpace(rccAdditionalPath) || EditorApplication.isCompiling);

#if !USK_MULTIPLAYER
                EditorGUI.BeginDisabledGroup(title == "Advanced Multiplayer");
#endif
                if (Helper.NamedGUILayout.TryGetNameOfJustClickedButton(out var clickedName))
                {
                    lastButtonClickedName = clickedName;
                }

                var buttonText = "Activate";

                if (Helper.NamedGUILayout.Button(new Rect(new Vector2(610, iconRect.y + 35), new Vector2(180,27)), buttonText, title))
                {
                    if (title == "Realistic Car Controller" || title == "Edy's Vehicle Physics" || title == "NWH Vehicle Physics 2")
                    {
                        DisableAllVehicleIntegrations();
                    }
                    
                    ChangeStatus(title, false);
                }
                
#if !USK_MULTIPLAYER
                EditorGUI.EndDisabledGroup();
#endif

                EditorGUI.EndDisabledGroup();
            }
            else if (status == "Enabled")
            {
                // indicatorStyle.normal.textColor = !EditorGUIUtility.isProSkin ? editorColorsLightTheme.textColor : editorColorsDarkTheme.textColor;


                var backgroundColor = GUI.backgroundColor;

                if (EditorApplication.isCompiling && lastButtonClickedName == title)
                {
                    GUI.backgroundColor = Color.yellow;
                    status = "Compiling Scripts...";
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                }

                GUI.TextArea(new Rect(new Vector2(610, iconRect.y + 10), new Vector2(180,20)), status, newStyle);

                // GUILayout.BeginVertical("helpbox");
                // GUILayout.Label(status, indicatorStyle);
                // GUILayout.EndVertical();

                GUI.backgroundColor = backgroundColor;

                // GUILayout.Space(3);
                EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

                var buttonText = "Deactivate";

                if (Helper.NamedGUILayout.Button(new Rect(new Vector2(610, iconRect.y + 35), new Vector2(180,27)), buttonText, title))
                {
                    ChangeStatus(title, true);
                }

                EditorGUI.EndDisabledGroup();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            // Rect rect = GUILayoutUtility.GetLastRect();
            // EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            GUILayout.Space(10f);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void DisableAllVehicleIntegrations()
        {
            ChangeStatus("Realistic Car Controller", true);
            ChangeStatus("Edy's Vehicle Physics", true);
            ChangeStatus("NWH Vehicle Physics 2", true);
        }

        void ChangeStatus(string type, bool currentStatus)
        {
            var newSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            if (!currentStatus)
            {
                switch (type)
                {
                    case "PUN 2":
                        newSymbols += ";USK_MULTIPLAYER";
                        break;
                    
                    case "Advanced Multiplayer":
                        newSymbols += ";USK_ADVANCED_MULTIPLAYER";
                        break;
                    
                    case "Realistic Car Controller":
                        newSymbols += ";USK_RCC_INTEGRATION";
                        break;
                    
                    case "Edy's Vehicle Physics":
                        newSymbols += ";USK_EVPH_INTEGRATION";
                        break; 
                    
                    case "NWH Vehicle Physics 2":
                        newSymbols += ";USK_NWHVPH_INTEGRATION";
                        break;
                    
                    case "Emerald AI 3.0":
                        newSymbols += ";USK_EMERALDAI_INTEGRATION";
                        break;
                    
                    case "Destroy It":
                        newSymbols += ";USK_DESTROYIT_INTEGRATION";
                        break;
                    
                    case "Easy Save":
                        newSymbols += ";USK_EASYSAVE_INTEGRATION";
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case "PUN 2":
                        if (newSymbols.Contains("USK_MULTIPLAYER"))
                            newSymbols = newSymbols.Replace("USK_MULTIPLAYER", "");
                        if (newSymbols.Contains("USK_ADVANCED_MULTIPLAYER"))
                            newSymbols = newSymbols.Replace("USK_ADVANCED_MULTIPLAYER", "");
                        break;
                    
                    case "Advanced Multiplayer":
                        if (newSymbols.Contains("USK_ADVANCED_MULTIPLAYER"))
                            newSymbols = newSymbols.Replace("USK_ADVANCED_MULTIPLAYER", "");
                        break;
                    
                    case "Realistic Car Controller":
                        if (newSymbols.Contains("USK_RCC_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_RCC_INTEGRATION", "");
                        break;
                    
                    case "Edy's Vehicle Physics":
                        if (newSymbols.Contains("USK_EVPH_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_EVPH_INTEGRATION", "");
                        break; 
                    
                    case "NWH Vehicle Physics 2":
                        if (newSymbols.Contains("USK_NWHVPH_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_NWHVPH_INTEGRATION", "");
                        break;
                    
                    case "Emerald AI 3.0":
                        if (newSymbols.Contains("USK_EMERALDAI_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_EMERALDAI_INTEGRATION", "");
                        break;
                    
                    case "Destroy It":
                        if (newSymbols.Contains("USK_DESTROYIT_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_DESTROYIT_INTEGRATION", "");
                        break;
                    
                    case "Easy Save":
                        if (newSymbols.Contains("USK_EASYSAVE_INTEGRATION"))
                            newSymbols = newSymbols.Replace("USK_EASYSAVE_INTEGRATION", "");
                        break;
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newSymbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, newSymbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, newSymbols);
        }
    }
}
