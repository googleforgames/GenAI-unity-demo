using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GercStudio.USK.Scripts
{
	[CustomEditor(typeof(UIManager))]
	public class UIManagerEditor : Editor
	{
		public UIManager script;
		private ReorderableList graphicsButtons;
		private ReorderableList frameRateButtons;
		
		private ReorderableList bloodHitMarks;

		private Texture2D backButton;
		private Texture2D settingsButton;

		private string path;

		private GUIStyle buttonStyle;
		private GUIStyle labelStyle;
		private GUIStyle windowStyle;
		private GUIStyle toolbarStyle;
		private GUIStyle toolbarSmallStyle;
		private GUIStyle descriptionStyle;
		private GUIStyle infoStyle;

		private GUISkin customSkin;

		private GUIStyle helpboxStyle;

		private UIHelper.MenuPages targetPage;
		private bool switchMenu;
		private bool moreThanOneMenuOpenedInGame;
		
		private string highlightColor;

		public void Awake()
		{
			script = (UIManager) target;

			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
				UIHelper.OpenItem(script, script.currentMenuPage, ref script.previousMenuPage);
		}

		private void OnEnable()
		{
			if (!script)
				return;

			if(Application.isPlaying)
				script.currentMenuPage = UIHelper.MenuPages.Settings;

			customSkin = (GUISkin) Resources.Load("EditorSkin");

			buttonStyle = customSkin.GetStyle("Button");
			labelStyle = customSkin.GetStyle("Label");
			toolbarStyle = customSkin.GetStyle("TabSmall");
			toolbarSmallStyle = customSkin.GetStyle("TabSmallest");
			descriptionStyle = customSkin.GetStyle("Description");
			infoStyle = customSkin.GetStyle("Info");

			highlightColor = !EditorGUIUtility.isProSkin ? "color=blue" : "color=orange";

			backButton = (Texture2D) Resources.Load("left arrow");
			settingsButton = (Texture2D) Resources.Load("settings");

			descriptionStyle.normal.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;
			infoStyle.normal.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;

			labelStyle.normal.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;
			labelStyle.hover.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;
			labelStyle.active.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;

			if (EditorGUIUtility.isProSkin)
			{
				customSkin.customStyles[1].onNormal.background = Resources.Load("TabSelected") as Texture2D;
				customSkin.customStyles[1].onNormal.textColor = Color.white;

				customSkin.customStyles[2].onNormal.background = Resources.Load("TabSelected") as Texture2D;
				customSkin.customStyles[2].onNormal.textColor = Color.white;
			}
			else
			{
				customSkin.customStyles[1].onNormal.background = Resources.Load("TabSelected2") as Texture2D;
				customSkin.customStyles[1].onNormal.textColor = Color.black;

				customSkin.customStyles[2].onNormal.background = Resources.Load("TabSelected2") as Texture2D;
				customSkin.customStyles[2].onNormal.textColor = Color.black;
			}

			buttonStyle.normal.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;
			buttonStyle.hover.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;
			buttonStyle.active.textColor = !EditorGUIUtility.isProSkin ? Color.black : Color.white;

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

			graphicsButtons = new ReorderableList(serializedObject, serializedObject.FindProperty("gameOptions.graphicsButtons"), true, true,
				true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Graphic Buttons");
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), "Quality Layers");
				},

				onAddCallback = items => { script.gameOptions.graphicsButtons.Add(new UIHelper.GameOptions.SettingsButton()); },

				onRemoveCallback = items => { script.gameOptions.graphicsButtons.Remove(script.gameOptions.graphicsButtons[items.index]); },

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.gameOptions.graphicsButtons[index].button = (Button) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.gameOptions.graphicsButtons[index].button, typeof(Button), true);

					script.gameOptions.graphicsButtons[index].qualitySettings = EditorGUI.Popup(
						new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.gameOptions.graphicsButtons[index].qualitySettings, QualitySettings.names);
					
				}
			};

			frameRateButtons = new ReorderableList(serializedObject, serializedObject.FindProperty("gameOptions.frameRateButtons"), true, true,
				true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Frame Rate Buttons");
					EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), "Value");
				},

				onAddCallback = items => { script.gameOptions.frameRateButtons.Add(new UIHelper.GameOptions.SettingsButton()); },

				onRemoveCallback = items => { script.gameOptions.frameRateButtons.Remove(script.gameOptions.frameRateButtons[items.index]); },

				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					script.gameOptions.frameRateButtons[index].button = (Button) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.gameOptions.frameRateButtons[index].button, typeof(Button), true);
					script.gameOptions.frameRateButtons[index].frameRate = EditorGUI.IntField(new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight), script.gameOptions.frameRateButtons[index].frameRate);
				}
			};

			bloodHitMarks = new ReorderableList(serializedObject, serializedObject.FindProperty("CharacterUI.hitMarkers"), false, true,
				true, true)
			{
				drawHeaderCallback = rect => { EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Damage Markers"); },

				onAddCallback = items => { script.CharacterUI.hitMarkers.Add(null); },

				onRemoveCallback = items => { script.CharacterUI.hitMarkers.Remove(script.CharacterUI.hitMarkers[items.index]); },

				drawElementCallback = (rect, index, isActive, isFocused) => { script.CharacterUI.hitMarkers[index] = (RawImage) EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), script.CharacterUI.hitMarkers[index], typeof(RawImage), true); }
			};

			EditorApplication.update += Update;
		}

		private void OnDisable()
		{
			EditorApplication.update -= Update;
		}

		void Update()
		{
			if (!script)
				return;
			
			
			if (script.CharacterUI.currentMiniMapType != script.CharacterUI.lastMiniMapType && script.CharacterUI.mapMask)
			{
				if (script.CharacterUI.currentMiniMapType == UIHelper.MiniMapType.Circle)
				{
					var texture = Resources.Load<Texture>("circle");
					if (texture != null)
					{
//						var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
						script.CharacterUI.mapMask.texture = texture;
//						script.CharacterUI.mapMask.rectTransform.sizeDelta = new Vector2(300, 300);

						EditorUtility.SetDirty(script.gameObject);
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
				else
				{
					script.CharacterUI.mapMask.texture = null;
				}

				script.CharacterUI.lastMiniMapType = script.CharacterUI.currentMiniMapType;
			}
			
			if (Application.isPlaying)
			{
				if (script.currentMenusInGame.Count > 1)
				{
					var overrideMenu = false;

					foreach (var menuPage in script.currentMenusInGame.Where(menuPage => menuPage == UIHelper.MenuPages.CharacterInventory || menuPage == UIHelper.MenuPages.SinglePause || menuPage == UIHelper.MenuPages.OptionsMenu
					                                                                     || menuPage == UIHelper.MenuPages.AdvancedMultiplayerRoomPause || menuPage == UIHelper.MenuPages.BasicMultiplayerRoomPause))
					{
						script.currentMenuPage = menuPage;
						overrideMenu = true;
						break;
					}

					moreThanOneMenuOpenedInGame = !overrideMenu;
				}
				else
				{
					moreThanOneMenuOpenedInGame = false;
					script.currentMenuPage = script.currentMenusInGame[0];
				}
				
				return;
			}
			
			if (script.currentMenuPage != script.previousMenuPage || script.teamsIndex != script.lastTeamsIndex || script.preMatchMenuIndex != script.preMatchMenuLastIndex
			    || script.deathScreensIndex != script.deathScreensLastIndex || script.roomMatchStatsLastTab != script.roomMatchStatsTab)
			{
				UIHelper.OpenItem(script, script.currentMenuPage, ref script.previousMenuPage);
			}
			
			// foreach (var obj in script.hierarchy.Where(obj => obj && obj.hideFlags != HideFlags.None))
			// {
			// 	obj.hideFlags = HideFlags.None;
			// }
			
			if (script.hide)
			{
				foreach (var obj in script.hideObjects.Where(obj => obj && obj.hideFlags != HideFlags.HideInHierarchy))
				{
					obj.hideFlags = HideFlags.HideInHierarchy;
				}
			}
			else
			{
				foreach (var obj in script.hideObjects.Where(obj => obj && obj.hideFlags != HideFlags.None))
				{
					obj.hideFlags = HideFlags.None;
				}
			}

#if !USK_ADVANCED_MULTIPLAYER
			switch (script.currentMenuPage)
			{
				case UIHelper.MenuPages.AdvancedMultiplayer:
				case UIHelper.MenuPages.AdvancedMultiplayerLobby:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:
				case UIHelper.MenuPages.AdvancedMultiplayerRoom:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:
					script.currentMenuPage = UIHelper.MenuPages.MainMenu;
					script.HideAllHierarchy();
					break;
			}
#endif

			AddEmptyMainObjects();

			if (!script.gameObject.activeSelf)
				script.gameObject.SetActive(true);

			if (script.grid)
			{
				if (!script.grid.gameObject.activeSelf)
					script.grid.gameObject.SetActive(true);

				if (script.lastGridOpacity != script.gridOpacity)
				{
					script.lastGridOpacity = script.gridOpacity;
					script.grid.color = new Color32(255, 255, 255, (byte) script.gridOpacity);
				}

				if (script.gridTexture)
				{
					if (script.gridTexture != script.previousGridTexture)
					{
						script.previousGridTexture = script.gridTexture;
						script.grid.texture = script.gridTexture;
					}
				}
			}

			// if (script.hideUIManager && script.gameObject.hideFlags != HideFlags.HideInHierarchy)
			// 	script.gameObject.hideFlags = HideFlags.HideInHierarchy;
			// else if(!script.hideUIManager && script.gameObject.hideFlags != HideFlags.None)
			// 	script.gameObject.hideFlags = HideFlags.None;


			if (script.CharacterUI.flashPlaceholder && script.CharacterUI.flashPlaceholder.gameObject.hideFlags != HideFlags.HideInHierarchy)
			{
				script.CharacterUI.flashPlaceholder.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}

			if (script.CharacterUI.crosshairMainObject && script.CharacterUI.crosshairMainObject.gameObject.hideFlags != HideFlags.HideInHierarchy)
			{
				script.CharacterUI.crosshairMainObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}

			if (script.CharacterUI.aimPlaceholder )//&& script.CharacterUI.aimPlaceholder.gameObject.hideFlags != HideFlags.HideInHierarchy)
			{
				script.CharacterUI.aimPlaceholder.gameObject.hideFlags = HideFlags.None;
			}
		}

		void DrawBackButton(Rect lastRect, UIHelper.MenuPages menuPage)
		{
			var rect = new Rect(new Vector2(lastRect.x, lastRect.y - 2), new Vector2(30, 30));

			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			EditorGUI.LabelField(rect, new GUIContent(backButton));

			if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
			{
				switchMenu = true;
				targetPage = menuPage;
			}
		}

		void DrawSettingsButton(Rect lastRect)
		{
			var rect = new Rect(new Vector2(lastRect.x, lastRect.y - 1), new Vector2(26, 26));

			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			EditorGUI.LabelField(rect, new GUIContent(settingsButton));

			if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
			{
				switchMenu = true;
				targetPage = UIHelper.MenuPages.Settings;
			}
		}

		void DrawPath(Rect lastRect, UIHelper.MenuPages[] items, string[] names)
		{
			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
			{
				GUILayout.Space(20);

				// var lastItemLenght = labelStyle.CalcSize(new GUIContent(" / " + names[0] + " / ")).x;
				var lastItemPosition = lastRect.x + 40;

				EditorGUI.LabelField(new Rect(new Vector2(lastItemPosition, lastRect.y + 2), new Vector2(10, labelStyle.lineHeight * 2)), "/ ", labelStyle);

				lastItemPosition += 11;

				for (var i = 1; i <= items.Length; i++)
				{
					var lastItemLenght = labelStyle.CalcSize(new GUIContent(names[i - 1] + " / ")).x;

					var rect = new Rect(new Vector2(lastItemPosition, lastRect.y + 10), new Vector2(lastItemLenght, labelStyle.lineHeight * 2));
					DrawPathItem(rect, items[i - 1], names[i - 1], true, i != items.Length);

					lastItemPosition += lastItemLenght + 2;
				}
			}
			else
			{
				labelStyle.fontSize = 16;
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.fontStyle = FontStyle.Bold;
				
				if(!moreThanOneMenuOpenedInGame)
					EditorGUILayout.LabelField("- " + names[names.Length - 1] + " -", labelStyle);
				else
				{
					var menus = new List<string>();

					foreach (var menu in script.currentMenusInGame)
					{
						switch (menu)
						{
							case UIHelper.MenuPages.CharacterInGame:
								menus.Add("Character UI"); 
								break;
							case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
							case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
								menus.Add("Match"); 
								break;
							case UIHelper.MenuPages.MobileInput:
								menus.Add("Mobile Input"); 
								break;
						}
					}
					
					script.currentMenuInGame = GUILayout.Toolbar(script.currentMenuInGame, menus.ToArray(), toolbarStyle);

					if (script.currentMenuInGame >= script.currentMenusInGame.Count)
						script.currentMenuPage = 0;
					
					script.currentMenuPage = script.currentMenusInGame[script.currentMenuInGame];
				}
			}
		}

		void DrawPathItem(Rect rect, UIHelper.MenuPages item, string name, bool drawBrake, bool isLink)
		{
			if (Helper.LinkLabel(new GUIContent(name), rect, labelStyle.CalcSize(new GUIContent(name)).x, labelStyle, false, isLink, isLink, true))
			{
				script.currentMenuPage = item;
			}

			if (drawBrake)
			{
				var labelLenght = labelStyle.CalcSize(new GUIContent(name)).x + 2;
				EditorGUI.LabelField(new Rect(new Vector2(rect.x + labelLenght, rect.y - 8), new Vector2(labelStyle.CalcSize(new GUIContent(" / ")).x, labelStyle.lineHeight * 2)), " / ", labelStyle);
			}
		}

		void DrawButton(string text, UIHelper.MenuPages menuPage, string textureName)
		{
			if (GUILayout.Button("   " + text, buttonStyle))
				script.currentMenuPage = menuPage;

			var lastRect = GUILayoutUtility.GetLastRect();
			var tex = (Texture2D) Resources.Load(textureName);
			var lenght = buttonStyle.CalcSize(new GUIContent(text)).x;

			EditorGUI.LabelField(new Rect(new Vector2(lastRect.x + lastRect.width / 2 - lenght / 2 - 20, lastRect.y + (lastRect.height - 25) / 2), new Vector2(25, 25)), new GUIContent(tex, ""));
		}

		void DrawDescription(string text)
		{
			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
			{
				EditorGUILayout.Space();
				EditorGUILayout.Space();

				EditorGUILayout.LabelField(text, descriptionStyle);
				EditorGUILayout.Space();
			}
		}

		void AddEmptyMainObject(int hierarchyIndex, GameObject mainObject)
		{
			if (!script.hierarchy[hierarchyIndex] && mainObject)
				script.hierarchy[hierarchyIndex] = mainObject;
		}

		public override void OnInspectorGUI()
		{
			if (!script)
				return;

			if (!script.gameObject.activeInHierarchy)
			{
				EditorGUILayout.HelpBox("Open this prefab or add it on a scene to adjust the UI.", MessageType.Info);

				return;
			}

			serializedObject.Update();

			if (switchMenu)
			{
				if (Event.current.type == EventType.Layout)
				{
					script.currentMenuPage = targetPage;
					switchMenu = false;
				}
			}

			if ((script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayerRoomMatch || script.currentMenuPage == UIHelper.MenuPages.AdvancedMultiplayerRoomMatch && script.roomMatchStatsTab == 1) && script.teamsIndex > 1)
				script.teamsIndex = 0;

			// style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};

			// EditorGUILayout.Space();

// #if !UNITY_2018_3_OR_NEWER
// 			EditorGUILayout.LabelField("Place this prefab in a scene, adjust UI elements, then <b><color=green>apply changes</color></b>.", style);
// #else
// 			EditorGUILayout.LabelField("Open this prefab, to adjust UI elements.", style);
// #endif

			helpboxStyle = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 12, fontStyle = FontStyle.Normal};

			EditorGUILayout.Space();

			// EditorGUILayout.BeginVertical("box");
// #if USK_ADVANCED_MULTIPLAYER || USK_MULTIPLAYER
			// script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Character UI", "Single-player", "Multiplayer"}, tabStyle);
// #elif USK_ADVANCED_MULTIPLAYER && !USK_MULTIPLAYER
// 			script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Single-player", "Character UI", "ADV Multiplayer"}, tabStyle);
// 			
// 			if (script.inspectorTab > 2)
// 				script.inspectorTab = 2;
// #elif !USK_ADVANCED_MULTIPLAYER && USK_MULTIPLAYER
// 			script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Single-player Game", "Character UI", "Multiplayer"}, tabStyle);
//
// 			if (script.inspectorTab > 2)
// 				script.inspectorTab = 2;
// #else
			// script.inspectorTab = GUILayout.Toolbar(script.inspectorTab, new[] {"Single-player Game", "Character UI"});
			//
			// if (script.inspectorTab > 1)
			// 	script.inspectorTab = 1;
// #endif
			// EditorGUILayout.EndVertical();



			if (script.currentMenuPage == UIHelper.MenuPages.MainMenu)
			{
				labelStyle.fontSize = 17;
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.fontStyle = FontStyle.Bold;

				buttonStyle.fontSize = 17;
				buttonStyle.fontStyle = FontStyle.Bold;
				buttonStyle.margin.top = 7;
				buttonStyle.margin.bottom = 7;
			}
			else
			{
				labelStyle.fontSize = 15;
				labelStyle.alignment = TextAnchor.MiddleLeft;
				labelStyle.fontStyle = FontStyle.Bold;

				buttonStyle.fontStyle = FontStyle.Bold;
				buttonStyle.fontSize = 15;
				buttonStyle.margin.top = 4;
				buttonStyle.margin.bottom = 4;
			}

			if (script.currentMenuPage == UIHelper.MenuPages.MainMenu)
			{
				labelStyle.fontSize = 15;
				GUILayout.Label("Use this component to manage all in-game UI", labelStyle);
			}

			EditorGUILayout.Space();

			var lastRect = GUILayoutUtility.GetLastRect();

			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
			{

				if (script.currentMenuPage == UIHelper.MenuPages.MainMenu)
				{
					DrawSettingsButton(new Rect(new Vector2(EditorGUIUtility.currentViewWidth - 50, lastRect.y - 30), lastRect.size));

					EditorGUILayout.Space();

					// var backgroundColor = GUI.backgroundColor;
					// GUI.backgroundColor = new Color(0, 1, 0, 0.5f);
					// EditorGUILayout.BeginVertical("helpbox");
					// EditorGUILayout.LabelField("<b>Please note:</b> " + "\n" +
					//                            "Don't delete or move the path items that are written in caps (e.g. SINGLE-PLAYER/PAUSE/...), " +
					//                            "but you can safely move and delete everything else.", infoStyle);
					// EditorGUILayout.EndVertical();
					// GUI.backgroundColor = backgroundColor;
					//
					// EditorGUILayout.Space();
					EditorGUILayout.Space();
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(35);
				EditorGUILayout.BeginVertical();
			}

			// path
			switch (script.currentMenuPage)
			{
				case UIHelper.MenuPages.MainMenu:
					break;
				case UIHelper.MenuPages.Settings:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.Settings}, new[] {"Main Menu", "Settings"});
					break;
				case UIHelper.MenuPages.CharacterUI:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.CharacterUI}, new[] {"Main Menu", "Character UI"});
					break;
				case UIHelper.MenuPages.CharacterInGame:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.CharacterUI, UIHelper.MenuPages.CharacterInGame}, new[] {"Main Menu", "Character UI", "Game UI"});
					break;
				case UIHelper.MenuPages.CharacterInventory:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.CharacterUI, UIHelper.MenuPages.CharacterInventory}, new[] {"Main Menu", "Character UI", "Inventory"});
					break;
				case UIHelper.MenuPages.MobileInput:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.MobileInput}, new[] {"Main Menu", "Mobile Input"});
					break;
				case UIHelper.MenuPages.OptionsMenu:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.OptionsMenu}, new[] {"Main Menu", "Game Options"});
					break;
				case UIHelper.MenuPages.SinglePlayer:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.SinglePlayer}, new[] {"Main Menu", "Single-Player"});
					break;
				case UIHelper.MenuPages.SinglePause:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.SinglePlayer, UIHelper.MenuPages.SinglePause}, new[] {"Main Menu", "Single-Player", "Pause"});
					break;
				case UIHelper.MenuPages.SingleGameOver:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.SinglePlayer, UIHelper.MenuPages.SingleGameOver}, new[] {"Main Menu", "Single-Player", "Game Over"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayer:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer}, new[] {"Main Menu", "Advanced Multiplayer"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobby:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Main Menu"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Game Modes"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Profile"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Maps"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Loadout"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Avatars"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Characters"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "All Rooms"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerLobby, UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom}, new[] {"Main Menu", "Advanced Multiplayer", "Lobby", "Create Room"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoom:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom}, new[] {"Main Menu", "Advanced Multiplayer", "Room"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomStart}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Finding Opponents"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomPause}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Pause"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Game Over"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Pre Match"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomMatch}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Match"});
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.AdvancedMultiplayerRoom, UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens}, new[] {"Main Menu", "Advanced Multiplayer", "Room", "Death Screens"});
					break;
				// case UIHelper.MenuPages.UIPlaceholders:
				// 	DrawPath(lastRect, new [] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.AdvancedMultiplayer, UIHelper.MenuPages.UIPlaceholders},new []{"Main Menu", "Advanced Multiplayer", "UI Placeholders"});
				// 	break;
				case UIHelper.MenuPages.BasicMultiplayer:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer}, new[] {"Main Menu", "Multiplayer"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobby:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby}, new[] {"Main Menu", "Multiplayer", "Lobby"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu}, new[] {"Main Menu", "Multiplayer", "Lobby", "Main Menu"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyAvatars:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyAvatars}, new[] {"Main Menu", "Multiplayer", "Lobby", "Avatars"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyCharacters:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyCharacters}, new[] {"Main Menu", "Multiplayer", "Lobby", "Characters"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyMaps:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyMaps}, new[] {"Main Menu", "Multiplayer", "Lobby", "Maps"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms}, new[] {"Main Menu", "Multiplayer", "Lobby", "All Rooms"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerLobby, UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom}, new[] {"Main Menu", "Multiplayer", "Lobby", "Create Room"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoom:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerRoom}, new[] {"Main Menu", "Multiplayer", "Room"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomPause:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerRoom, UIHelper.MenuPages.BasicMultiplayerRoomPause}, new[] {"Main Menu", "Multiplayer", "Room", "Pause"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerRoom, UIHelper.MenuPages.BasicMultiplayerRoomMatch}, new[] {"Main Menu", "Multiplayer", "Room", "Match"});
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomGameOver:
					DrawPath(lastRect, new[] {UIHelper.MenuPages.MainMenu, UIHelper.MenuPages.BasicMultiplayer, UIHelper.MenuPages.BasicMultiplayerRoom, UIHelper.MenuPages.BasicMultiplayerRoomGameOver}, new[] {"Main Menu", "Multiplayer", "Room", "Game Over"});
					break;
			}

			if (!Application.isPlaying || Application.isPlaying && script.gameObject.scene.name == "UI Manager")
			{

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				lastRect = GUILayoutUtility.GetLastRect();

				// back buttons
				switch (script.currentMenuPage)
				{
					case UIHelper.MenuPages.CharacterUI:
					case UIHelper.MenuPages.SinglePlayer:
					case UIHelper.MenuPages.AdvancedMultiplayer:
					case UIHelper.MenuPages.BasicMultiplayer:
					case UIHelper.MenuPages.MobileInput:
					case UIHelper.MenuPages.Settings:
					case UIHelper.MenuPages.OptionsMenu:
						DrawBackButton(lastRect, UIHelper.MenuPages.MainMenu);

						break;
					case UIHelper.MenuPages.CharacterInGame:
					case UIHelper.MenuPages.CharacterInventory:


						DrawBackButton(lastRect, UIHelper.MenuPages.CharacterUI);

						break;
					case UIHelper.MenuPages.SinglePause:
					case UIHelper.MenuPages.SingleGameOver:
						DrawBackButton(lastRect, UIHelper.MenuPages.SinglePlayer);
						break;
					case UIHelper.MenuPages.AdvancedMultiplayerLobby:
					case UIHelper.MenuPages.AdvancedMultiplayerRoom:
						// case UIHelper.MenuPages.UIPlaceholders:
						DrawBackButton(lastRect, UIHelper.MenuPages.AdvancedMultiplayer);
						break;

					case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:
					case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:


						DrawBackButton(lastRect, UIHelper.MenuPages.AdvancedMultiplayerLobby);
						break;
					case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:
					case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
					case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
					case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:
					case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
					case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:
						DrawBackButton(lastRect, UIHelper.MenuPages.AdvancedMultiplayerRoom);
						break;

					case UIHelper.MenuPages.BasicMultiplayerLobby:
					case UIHelper.MenuPages.BasicMultiplayerRoom:
						// case UIHelper.MenuPages.BasicMultiplayerPlaceholders:

						DrawBackButton(lastRect, UIHelper.MenuPages.BasicMultiplayer);

						break;

					case UIHelper.MenuPages.BasicMultiplayerLobbyCharacters:
					case UIHelper.MenuPages.BasicMultiplayerLobbyMaps:
					case UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu:
					case UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms:
					case UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom:
					case UIHelper.MenuPages.BasicMultiplayerLobbyAvatars:

						DrawBackButton(lastRect, UIHelper.MenuPages.BasicMultiplayerLobby);

						break;

					case UIHelper.MenuPages.BasicMultiplayerRoomPause:
					case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
					case UIHelper.MenuPages.BasicMultiplayerRoomGameOver:

						DrawBackButton(lastRect, UIHelper.MenuPages.BasicMultiplayerRoom);

						break;
				}

				if (script.currentMenuPage != UIHelper.MenuPages.MainMenu)
				{
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
				}
				// lastRect = GUILayoutUtility.GetLastRect();

				//main buttons
				switch (script.currentMenuPage)
				{
					case UIHelper.MenuPages.MainMenu:

						// EditorGUILayout.BeginVertical("helpbox");

						DrawButton("Character UI", UIHelper.MenuPages.CharacterUI, "character ui");

						EditorGUILayout.Space();

						DrawButton("Single-Player", UIHelper.MenuPages.SinglePlayer, "single-player");

						DrawButton("Multiplayer", UIHelper.MenuPages.BasicMultiplayer, "basic multiplayer");
						
						EditorGUILayout.Space();


#if USK_ADVANCED_MULTIPLAYER

						DrawButton("Advanced Multiplayer", UIHelper.MenuPages.AdvancedMultiplayer, "advanced multiplayer");
						
						EditorGUILayout.Space();

#endif
						DrawButton("Mobile Input", UIHelper.MenuPages.MobileInput, "mobile input");

						DrawButton("Game Options", UIHelper.MenuPages.OptionsMenu, "game options");

						// EditorGUILayout.EndVertical();

						break;
					case UIHelper.MenuPages.SinglePlayer:

						// EditorGUILayout.Space();
						// backgroundColor = GUI.backgroundColor;
						// GUI.backgroundColor = Color.gray;
						// EditorGUILayout.BeginVertical("helpbox");
						// GUI.backgroundColor = backgroundColor;

						if (GUILayout.Button("Pause", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.SinglePause;

						// EditorGUILayout.Space();

						// if (GUILayout.Button("Options", buttonStyle))
						// 	script.currentMenuPage = UIHelper.MenuPages.SingleOptions;

						// EditorGUILayout.Space();

						if (GUILayout.Button("Game Over", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.SingleGameOver;

						// EditorGUILayout.EndVertical();

						// EditorGUILayout.Space();
						break;

					case UIHelper.MenuPages.CharacterUI:

						// EditorGUILayout.Space();

						// var backgroundColor2 = GUI.backgroundColor;
						// GUI.backgroundColor = Color.gray;
						// EditorGUILayout.BeginVertical("helpbox");
						// GUI.backgroundColor = backgroundColor2;

						if (GUILayout.Button("Game UI", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.CharacterInGame;

						// EditorGUILayout.Space();

						if (GUILayout.Button("Inventory", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.CharacterInventory;

						// EditorGUILayout.Space();

						// if (GUILayout.Button("Mobile Input", buttonStyle))
						// 	script.currentMenuPage = UIHelper.MenuPages.MobileInput;

						// EditorGUILayout.EndVertical();

						break;

					case UIHelper.MenuPages.AdvancedMultiplayer:

						// var backgroundColor3 = GUI.backgroundColor;
						// GUI.backgroundColor = Color.gray;
						// EditorGUILayout.BeginVertical("helpbox");
						// GUI.backgroundColor = backgroundColor3;

						if (GUILayout.Button("Lobby", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobby;

						// EditorGUILayout.Space();

						if (GUILayout.Button("Room", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoom;

						// EditorGUILayout.Space();

						// if (GUILayout.Button("UI Prefabs", buttonStyle))
						// 	script.currentMenuPage = UIHelper.MenuPages.UIPlaceholders;

						// EditorGUILayout.EndVertical();

						// EditorGUILayout.Space();

						break;

					case UIHelper.MenuPages.AdvancedMultiplayerLobby:

						// var backgroundColor4 = GUI.backgroundColor;
						// GUI.backgroundColor = Color.gray;
						// EditorGUILayout.BeginVertical("helpbox");
						// GUI.backgroundColor = backgroundColor4;

						if (GUILayout.Button("Main Menu", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu;

						EditorGUILayout.Space();

						if (GUILayout.Button("Game Modes", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes;

						// EditorGUILayout.Space();

						if (GUILayout.Button("Maps", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps;

						// EditorGUILayout.Space();

						DrawButton("Loadout", UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout, "");

						// EditorGUILayout.Space();

						// EditorGUILayout.Space();

						if (GUILayout.Button("Characters", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters;

						// EditorGUILayout.Space();

						if (GUILayout.Button("All Rooms", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms;

						// EditorGUILayout.Space();

						if (GUILayout.Button("Create Room", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom;
						
						EditorGUILayout.Space();
						
						if (GUILayout.Button("Profile", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile;
						
						if (GUILayout.Button("Avatars", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars;

						// EditorGUILayout.EndVertical();

						break;

					case UIHelper.MenuPages.AdvancedMultiplayerRoom:

						// var backgroundColor5 = GUI.backgroundColor;
						// GUI.backgroundColor = Color.gray;
						// EditorGUILayout.BeginVertical("helpbox");
						// GUI.backgroundColor = backgroundColor5;

						if (GUILayout.Button("Finding Opponents", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomStart;
						
						if (GUILayout.Button("Pre-match", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch;
						
						if (GUILayout.Button("Match", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomMatch;
						
						if (GUILayout.Button("Pause", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomPause;
						
						if (GUILayout.Button("Death Screens", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens;
						
						if (GUILayout.Button("Game Over", buttonStyle))
							script.currentMenuPage = UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver;
						

						// EditorGUILayout.EndVertical();

						// EditorGUILayout.Space();

						break;

					case UIHelper.MenuPages.BasicMultiplayer:

						DrawButton("Lobby", UIHelper.MenuPages.BasicMultiplayerLobby, "");
						DrawButton("Room", UIHelper.MenuPages.BasicMultiplayerRoom, "");
						// DrawButton("UI Placeholders", UIHelper.MenuPages.BasicMultiplayerPlaceholders, "");

						break;

					case UIHelper.MenuPages.BasicMultiplayerLobby:

						DrawButton("Main Menu", UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu, "");
						DrawButton("Characters", UIHelper.MenuPages.BasicMultiplayerLobbyCharacters, "");
						DrawButton("Avatars", UIHelper.MenuPages.BasicMultiplayerLobbyAvatars, "");
						DrawButton("Maps", UIHelper.MenuPages.BasicMultiplayerLobbyMaps, "");
						DrawButton("All Rooms", UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms, "");
						DrawButton("Create Room", UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom, "");

						break;

					case UIHelper.MenuPages.BasicMultiplayerRoom:

						DrawButton("Match", UIHelper.MenuPages.BasicMultiplayerRoomMatch, "");
						DrawButton("Pause", UIHelper.MenuPages.BasicMultiplayerRoomPause, "");
						DrawButton("Game Over", UIHelper.MenuPages.BasicMultiplayerRoomGameOver, "");

						break;
				}
			}

			// description
			switch (script.currentMenuPage)
			{
				case UIHelper.MenuPages.CharacterInGame:
				case UIHelper.MenuPages.CharacterInventory:
				case UIHelper.MenuPages.MobileInput:
				case UIHelper.MenuPages.SinglePause:
				case UIHelper.MenuPages.SingleGameOver:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:
				case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:
				case UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu:
				case UIHelper.MenuPages.BasicMultiplayerLobbyCharacters:
				case UIHelper.MenuPages.BasicMultiplayerLobbyMaps:
				case UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms:
				case UIHelper.MenuPages.BasicMultiplayerLobbyAvatars:
				case UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom:
				case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
				case UIHelper.MenuPages.BasicMultiplayerRoomGameOver:
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					break; 
				case UIHelper.MenuPages.OptionsMenu:
					DrawDescription("- These are the game settings. This window is the same for all modes - Single-player, Multiplayer and Advanced Multipler -");
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:
					DrawDescription("- This is the menu for finding opponents. It is the same for all game modes. -");
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
					DrawDescription("- This is the menu that displays all player statistics (kills/deaths, score, status). It is also used as a pause menu. -");
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
					DrawDescription("- This menu is displayed after the end of a round or the entire match and shows who won. -");
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					break;
				case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
					DrawDescription("- All this is the UI that is displayed during the multiplayer match. -");
					break;
				case UIHelper.MenuPages.BasicMultiplayerRoomPause:
					DrawDescription("- This is a pause menu and also it displays all players. -");

					break;
				
			}

			if (script.currentMenuPage != UIHelper.MenuPages.MainMenu && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayer && script.currentMenuPage != UIHelper.MenuPages.SinglePlayer && script.currentMenuPage != UIHelper.MenuPages.CharacterUI && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayerLobby && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayerRoom
			    && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayer && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayerLobby && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayerRoom)
			{
				// EditorGUILayout.BeginVertical(new GUIStyle("Window"){padding = {top = 0, bottom = 6}});
				EditorGUILayout.BeginVertical("helpbox");
			}

			// content
			switch (script.currentMenuPage)
			{
				case UIHelper.MenuPages.Settings:

					// EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gridTexture"), new GUIContent("Grid Image"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gridOpacity"), new GUIContent("Grid Opacity"));
					// EditorGUILayout.EndVertical();

					// EditorGUILayout.Space();
					// EditorGUILayout.Space();

					// if (script.uiManagerLayout)
					// {
					// 	EditorGUILayout.LabelField("Editor Layout", EditorStyles.boldLabel);
					//
					// 	if (GUILayout.Button("Save Current Layout"))
					// 	{
					// 		var layoutPath = AssetDatabase.GetAssetPath(script.uiManagerLayout);
					// 		Helper.LayoutUtility.SaveLayout(layoutPath);
					// 	}
					// }

					break;
				
				case UIHelper.MenuPages.OptionsMenu:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.back"), new GUIContent("Back"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.LabelField("Screen Resolution", EditorStyles.boldLabel);
					EditorGUILayout.HelpBox("At the game start, all the screen resolutions will appear in the Scroll Rect", MessageType.Info);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.resolutionButtonPlaceholder.button"), new GUIContent("Button Placeholder"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.resolutionsScrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.fullscreenMode.button"), new GUIContent("Full Screen Mode"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.windowedMode.button"), new GUIContent("Windowed Mode"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					graphicsButtons.DoLayoutList();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.frameRate"), new GUIContent("Current Frame Rate"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					frameRateButtons.DoLayoutList();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOptions.resetGameData"), new GUIContent("Reset Game Data°", "This button is only used in the Advanced Multiplayer add-on."));
					EditorGUILayout.EndVertical();


					break;

				case UIHelper.MenuPages.SinglePause:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGamePause.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGamePause.Resume"), new GUIContent("Resume"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGamePause.Options"), new GUIContent("Options"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGamePause.Exit"), new GUIContent("Exit"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.SingleGameOver:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGameGameOver.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGameGameOver.Restart"), new GUIContent("Restart"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("SinglePlayerGame.SinglePlayerGameGameOver.Exit"), new GUIContent("Exit"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.CharacterInGame:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.WeaponAmmo"), new GUIContent("Ammo"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.WeaponAmmoImagePlaceholder"), new GUIContent("Image Placeholder"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.attackImage"), new GUIContent("Attack Type Placeholder"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Health"), new GUIContent("Health"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.HealthBar"), new GUIContent("Health bar"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.aimPlaceholder"), new GUIContent("Sniper Scope Texture Placeholder"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.PickupImage"), new GUIContent("'Pick up' Image"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.infoTooltip"), new GUIContent("Info Tooltip"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gamepadCursor"), new GUIContent("Gamepad Cursor°", "This cursor appears when the player interacts with any in-game menu using a gamepad."));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Damage Indicators", EditorStyles.boldLabel);

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.hitIndicatorsBorder"), new GUIContent("Border"));

					if (script.CharacterUI.hitIndicatorsBorder == UIHelper.HitIndicatorsBorder.Circle)
						EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.circleRadius"), new GUIContent("Radius"));

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					bloodHitMarks.DoLayoutList();
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.bloodSplatter"), new GUIContent("Blood Splatter Screen"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Mini-map", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.currentMiniMapType"), new GUIContent("Form"));
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.mapMask"), new GUIContent("Placeholder°", "You can adjust the size, position, and rotation of this placeholder."));
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.CharacterInventory:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Weapons Slots", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");

					script.curWeaponSlot = EditorGUILayout.Popup("Slot №", script.curWeaponSlot, new[] {"1", "2", "3", "4", "5", "6", "7", "8"});
					// EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.WeaponsButtons").GetArrayElementAtIndex(script.curWeaponSlot), new GUIContent("Main Button"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.WeaponImagePlaceholder").GetArrayElementAtIndex(script.curWeaponSlot), new GUIContent("Image Placeholder"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.WeaponAmmoText").GetArrayElementAtIndex(script.curWeaponSlot), new GUIContent("Ammo Text"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.UpWeaponButton"), new GUIContent("<- Weapon"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.DownWeaponButton"), new GUIContent("-> Weapon"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.WeaponsCount"), new GUIContent("Weapons Count"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Health Slot", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");

					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.UpHealthButton"), new GUIContent("<- Health"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.DownHealthButton"), new GUIContent("-> Health"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.HealthButton"), new GUIContent("Main Button"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.HealthImage"), new GUIContent("Image Placeholder"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.HealthKitsCount"), new GUIContent("Kits Count"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.CurrentHealthValue"), new GUIContent("Kit Value"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Ammo Slot", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");

					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.UpAmmoButton"), new GUIContent("<- Ammo"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.DownAmmoButton"), new GUIContent("-> Ammo"));
					EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.AmmoButton"), new GUIContent("Main Button"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.AmmoImage"), new GUIContent("Image Placeholder"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.AmmoKitsCount"), new GUIContent("Kits Count"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterUI.Inventory.CurrentAmmoValue"), new GUIContent("Kit Value"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.MobileInput:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("UIButtonsMainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Characters", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					script.moveStick = (GameObject) EditorGUILayout.ObjectField("Move Stick", script.moveStick, typeof(GameObject), true);
					script.moveStickOutline = (GameObject) EditorGUILayout.ObjectField("Move Stick Outline", script.moveStickOutline, typeof(GameObject), true);
					EditorGUILayout.Space();
					script.cameraStick = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Camera Stick°", "Used only in the Top Down mode."), script.cameraStick, typeof(GameObject), true);
					script.cameraStickOutline = (GameObject) EditorGUILayout.ObjectField("Camera Stick Outline", script.cameraStickOutline, typeof(GameObject), true);
					EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("moveStickRange"), new GUIContent("Range (in px)"));
					EditorGUILayout.Space();
					
					

					EditorGUILayout.PropertyField(serializedObject.FindProperty("moveStickAlwaysVisible"), new GUIContent("Always Visible°", "If this bool is not active, the joystick will appear when you tap on the screen."));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedPosition"), new GUIContent("Fixed Position°", "If this bool is active, the joystick will always be in one place." + "\n\n" +
					                                                                                                                "If not, the joystick will be where you pressed on the screen."));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					script.uiButtons[2] = (Button) EditorGUILayout.ObjectField("Change Camera Type", script.uiButtons[2], typeof(Button), true);
					// script.uiButtons[12] = (Button) EditorGUILayout.ObjectField("Change Character", script.uiButtons[12], typeof(Button), true);
					script.uiButtons[16] = (Button) EditorGUILayout.ObjectField("Change TP Movement Type / TD Mode", script.uiButtons[16], typeof(Button), true);
					script.uiButtons[6] = (Button) EditorGUILayout.ObjectField("Sprint", script.uiButtons[6], typeof(Button), true);
					script.uiButtons[7] = (Button) EditorGUILayout.ObjectField("Crouch", script.uiButtons[7], typeof(Button), true);
					script.uiButtons[8] = (Button) EditorGUILayout.ObjectField("Jump", script.uiButtons[8], typeof(Button), true);
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Weapons", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					script.uiButtons[5] = (Button) EditorGUILayout.ObjectField("Attack", script.uiButtons[5], typeof(Button), true);
					script.uiButtons[1] = (Button) EditorGUILayout.ObjectField("Reload", script.uiButtons[1], typeof(Button), true);
					script.uiButtons[0] = (Button) EditorGUILayout.ObjectField("Aim", script.uiButtons[0], typeof(Button), true);
					script.uiButtons[3] = (Button) EditorGUILayout.ObjectField("Change Attack Type", script.uiButtons[3], typeof(Button), true);
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Inventory", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					script.uiButtons[10] = (Button) EditorGUILayout.ObjectField("Open/Close Inventory", script.uiButtons[10], typeof(Button), true);
					script.uiButtons[11] = (Button) EditorGUILayout.ObjectField("Pick up Object", script.uiButtons[11], typeof(Button), true);
					script.uiButtons[4] = (Button) EditorGUILayout.ObjectField("Drop weapon", script.uiButtons[4], typeof(Button), true);
					script.uiButtons[14] = (Button) EditorGUILayout.ObjectField("Change Weapon (up)", script.uiButtons[14], typeof(Button), true);
					script.uiButtons[13] = (Button) EditorGUILayout.ObjectField("Change Weapon (down)", script.uiButtons[13], typeof(Button), true);
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					script.uiButtons[9] = (Button) EditorGUILayout.ObjectField("Pause", script.uiButtons[9], typeof(Button), true);
#if USK_RCC_INTEGRATION || USK_EVPH_INTEGRATION || USK_NWHVPH_INTEGRATION
					script.uiButtons[15] = (Button) EditorGUILayout.ObjectField("Interaction with Cars", script.uiButtons[15], typeof(Button), true);
#endif
					EditorGUILayout.EndVertical();

					break;


				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMainMenu:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.nickname"), new GUIContent("Nickname"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.currentMoney"), new GUIContent("Money"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.currentLevel"), new GUIContent("Level"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.Avatar"), new GUIContent("Avatar"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.ConnectionStatus"), new GUIContent("Connection Status"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.RegionsDropdown"), new GUIContent("Regions Dropdown"));

					EditorGUILayout.EndVertical();


					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.CurrentModeAndMap"), new GUIContent("Current Mode & Map"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.ChooseGameModeButton"), new GUIContent("Choose Mode & Map"));
					
					EditorGUILayout.Space();
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.ChangeCharacter"), new GUIContent("Change Character"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.PlayButton"), new GUIContent("Play"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.LoadoutButton"), new GUIContent("Open Loadout"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.AllRoomsButton"), new GUIContent("All Rooms"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.CreateRoomButton"), new GUIContent("Create Room"));
					
					EditorGUILayout.Space();
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.openProfileMenuButton"), new GUIContent("Profile Menu"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.settingsButton"), new GUIContent("Settings"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MainMenu.exitButton"), new GUIContent("Exit"));

					EditorGUILayout.EndVertical();
					
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyGameModes:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.gameModePlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.Info"), new GUIContent("Mode Info"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.MapsButton"), new GUIContent("Choose Map"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.MapButtonText"), new GUIContent("Button Text"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.GameModesMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();
					break;
				
				case UIHelper.MenuPages.AdvancedMultiplayerLobbyProfile:
					
					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.mainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.nickNameInputField"), new GUIContent("NickName"));
					
					EditorGUILayout.Space();
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.avatar"), new GUIContent("Avatar Placeholder"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.changeAvatarButton"), new GUIContent("Change Avatar"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.money"), new GUIContent("Money Value"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.score"), new GUIContent("Score Value"));
					
					EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.level"), new GUIContent("Current Level (label 1)"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.currentLevel"), new GUIContent("Current Level (label 2)"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.nextLevel"), new GUIContent("Next Level"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.progress"), new GUIContent("Progress Text"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.progressBarFill"), new GUIContent("Progress Bar Fill"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.ProfileMenu.backButton"), new GUIContent("Back"));
					EditorGUILayout.EndVertical();
					
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyMaps:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MapsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MapsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.mapPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MapsMenu.gameModesButton"), new GUIContent("Choose Game Mode"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MapsMenu.GameModesButtonText"), new GUIContent("Button Text"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.MapsMenu.backButton"), new GUIContent("Close Menu"));

					EditorGUILayout.EndVertical();
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyLoadout:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.mainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.weaponPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.weaponInfo"), new GUIContent("Weapon Info"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.purchaseWarning"), new GUIContent("Purchase Warning"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.equipButton"), new GUIContent("Interaction Button"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.equipButtonText"), new GUIContent("Button Text"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.LoadoutMenu.backButton"), new GUIContent("Back"));

					EditorGUILayout.EndVertical();
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAvatars:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AvatarsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AvatarsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.avatarPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AvatarsMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCharacters:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.mainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.requiredStatsAndStatus"), new GUIContent("Status"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.interactionButton"), new GUIContent("Interaction Button"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.interactionButtonText"), new GUIContent("Button Text"));
					EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.upButton"), new GUIContent("->"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.downButton"), new GUIContent("<-"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CharactersMenu.backButton"), new GUIContent("Back"));
					EditorGUILayout.EndVertical();
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerLobbyAllRooms:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AllRoomsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AllRoomsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.roomInfoPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AllRoomsMenu.Password"), new GUIContent("Password"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AllRoomsMenu.JoinButton"), new GUIContent("Join"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.AllRoomsMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();

					break;


				case UIHelper.MenuPages.AdvancedMultiplayerLobbyCreateRoom:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.GameName"), new GUIContent("Game Name"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.Password"), new GUIContent("Password"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.currentMode"), new GUIContent("Selected Game Mode"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.currentMap"), new GUIContent("Selected Map"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.CreateButton"), new GUIContent("Create"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameLobby.CreateRoomMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();
					break;


				case UIHelper.MenuPages.AdvancedMultiplayerRoomStart:

					// EditorGUILayout.LabelField("• This is the menu for finding opponents. It is the same for all modes.", helpboxStyle);
					// EditorGUILayout.Space();
					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.StartMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");

					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.StartMenu.FindPlayersTimer"), new GUIContent("Timer"));


					// EditorGUILayout.Space();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.StartMenu.FindPlayersStatsText"), new GUIContent("Status"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.StartMenu.PlayersContent"), new GUIContent("Players Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.playerInfoPlaceholder"), new GUIContent("Player Placeholder"));

					EditorGUILayout.Space();				
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.StartMenu.ExitButton"), new GUIContent("Exit Button"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.AdvancedMultiplayerRoomPause:
					// EditorGUILayout.LabelField("• ", helpboxStyle);

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.currentGameAndPassword"), new GUIContent("Game Name & Password°", "If a player creates his own room, this text will display the name and password of that room."));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.exitButton"), new GUIContent("Exit"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.resumeButton"), new GUIContent("Resume"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					script.teamsIndex = GUILayout.Toolbar(script.teamsIndex, new[]
					{
						new GUIContent("Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS ACTIVE. Players will be divided into teams and will be displayed in 2 lists."),
						new GUIContent("Not Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS NOT ACTIVE. All players will be displayed in 1 list.")
					}, toolbarStyle);

					EditorGUILayout.Space();

					// EditorGUILayout.LabelField(new GUIContent("Teams UI°", "These elements are used for modes in which the [Use Teams] parameter is active. Players will be divided into teams and will be displayed in two lists."), EditorStyles.boldLabel);

					if (script.teamsIndex == 0)
					{
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.teamsPauseMenuMain"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.playerInfoPlaceholder"), new GUIContent("Player Placeholder"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("1st Team", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.firstTeamName"), new GUIContent("Name"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.firstTeamScore"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.firstTeamTotalWins"), new GUIContent("Rounds Won"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.firstTeamScrollRect"), new GUIContent("Players Scroll Rect"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("2nd Team", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.secondTeamName"), new GUIContent("Name"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.secondTeamScore"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.secondTeamTotalWins"), new GUIContent("Rounds Won"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.secondTeamScrollRect"), new GUIContent("Player Scroll Rect"));
						EditorGUILayout.EndVertical();
					}

					// EditorGUILayout.Space();
					// EditorGUILayout.Space();

					else if (script.teamsIndex == 1)
					{
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PauseMenu.notTeamsScrollRect"), new GUIContent("Players Scroll Rect"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.playerInfoPlaceholder"), new GUIContent("Player Placeholder"));

						EditorGUILayout.EndVertical();
					}

					break;

				case UIHelper.MenuPages.AdvancedMultiplayerRoomGameOver:
					// EditorGUILayout.LabelField("• This menu is displayed after the end of a round or the entire match and shows who won.", helpboxStyle);

					// EditorGUILayout.Space();
					// EditorGUILayout.Space();

					EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.playAgainButton"), new GUIContent("Play Again"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.matchStatsButton"), new GUIContent("Show Stats"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.exitButton"), new GUIContent("Exit"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.backButton"), new GUIContent("Back"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.moneyProfit"), new GUIContent("Money Profit"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.newLevel"), new GUIContent("New Level"));

					EditorGUILayout.EndVertical();


					EditorGUILayout.Space();
					EditorGUILayout.Space();

					script.teamsIndex = GUILayout.Toolbar(script.teamsIndex, new[]
					{
						new GUIContent("Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS ACTIVE. Players will be divided into teams and will be displayed in 2 lists."),
						new GUIContent("Not Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS NOT ACTIVE. All players will be displayed in 1 list.")
					}, toolbarStyle);

					EditorGUILayout.Space();

					if (script.teamsIndex == 1)
					{
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.notTeamsMainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.notTeamsStatus"), new GUIContent("Place Status"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("1st Player", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(0).FindPropertyRelative("mainObject"), new GUIContent("Main Object"));
						EditorGUILayout.Space();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(0).FindPropertyRelative("nickname"), new GUIContent("Nickname"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(0).FindPropertyRelative("score"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(0).FindPropertyRelative("avatar"), new GUIContent("Avatar"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.LabelField("2nd Player", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(1).FindPropertyRelative("mainObject"), new GUIContent("Main Object"));
						EditorGUILayout.Space();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(1).FindPropertyRelative("nickname"), new GUIContent("Nickname"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(1).FindPropertyRelative("score"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(1).FindPropertyRelative("avatar"), new GUIContent("Avatar"));

						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.LabelField("3rd Player", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(2).FindPropertyRelative("mainObject"), new GUIContent("Main Object"));
						EditorGUILayout.Space();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(2).FindPropertyRelative("nickname"), new GUIContent("Nickname"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(2).FindPropertyRelative("score"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.podiumPlaceholders").GetArrayElementAtIndex(2).FindPropertyRelative("avatar"), new GUIContent("Avatar"));
						EditorGUILayout.EndVertical();

						// EditorGUILayout.EndVertical();
					}

					// EditorGUILayout.LabelField("Teams UI", EditorStyles.boldLabel);

					else if (script.teamsIndex == 0)
					{
						// EditorGUILayout.BeginVertical("helpbox");

						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.teamsMainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.teamsStatus"), new GUIContent("Victory Status"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.roundStatusText"), new GUIContent("Round Status"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.victoryImage"), new GUIContent("Victory Image"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.defeatImage"), new GUIContent("Defeat Image"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.drawImage"), new GUIContent("Draw Image"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("1st Team", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.firstTeamBackground"), new GUIContent("Background"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.firstTeamName"), new GUIContent("Name"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.firstTeamScore"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.firstTeamScoreMeaning"), new GUIContent("Score Meaning"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.firstTeamLogoPlaceholder"), new GUIContent("Logo Placeholder"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.LabelField("2nd Team", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.secondTeamBackground"), new GUIContent("Background"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.secondTeamName"), new GUIContent("Name"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.secondTeamScore"), new GUIContent("Score"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.secondTeamScoreMeaning"), new GUIContent("Score Meaning"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.GameOverMenu.secondTeamLogoPlaceholder"), new GUIContent("Logo Placeholder"));
						EditorGUILayout.EndVertical();
						// EditorGUILayout.EndVertical();
					}

					// EditorGUILayout.Space();
					break;

				case UIHelper.MenuPages.AdvancedMultiplayerRoomMatch:
					// EditorGUILayout.LabelField("• All this is the UI that is displayed during the multiplayer match.", helpboxStyle);

					// EditorGUILayout.Space();
					// EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.KillDeathStatsScrollRect"), new GUIContent("Kill/Death Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.matchStatsPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.MatchTimer"), new GUIContent("Match Timer"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.TargetText"), new GUIContent("Match Target"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.AddScorePopup"), new GUIContent("Add Score Popup"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					// EditorGUILayout.HelpBox("" + "\n" + "", MessageType.Info);

					// EditorGUILayout.BeginVertical("helpbox");
					script.roomMatchStatsTab = GUILayout.Toolbar(script.roomMatchStatsTab, new[]
					{
						new GUIContent("Normal Mode°", "If the [USE RESPAWNS] parameter IS ACTIVE it is the 'NORMAL' mode."),
						new GUIContent("Survival Mode°", "If the [USE RESPAWNS] parameter IS NOT ACTIVE, the player won't be respawned and this mode calls 'SURVIVAL'."),
					}, toolbarStyle);
					// EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					// EditorGUILayout.Space();

					// EditorGUILayout.Space();


					switch (script.roomMatchStatsTab)
					{
						case 0:

							EditorGUILayout.BeginVertical("helpbox");
							script.teamsIndex = GUILayout.Toolbar(script.teamsIndex, new[]
							{
								new GUIContent("Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS ACTIVE. Players will be divided into teams and will be displayed in 2 lists."),
								new GUIContent("Not Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS NOT ACTIVE. All players will be displayed in 1 list."),
								new GUIContent("Domination UI°", "If the [MATCH TARGET] is the POINT RETENTION and [POINTS COUNT] = 3."),
								new GUIContent("Hard Point UI°", "If the [MATCH TARGET] is the POINT RETENTION and [POINTS COUNT] = 1."),
							}, toolbarSmallStyle);

							EditorGUILayout.Space();

							// EditorGUILayout.BeginVertical("helpbox");

							switch (script.teamsIndex)
							{
								case 0:
								{
									// EditorGUILayout.LabelField("Teams UI", EditorStyles.boldLabel);

									BeginGreenHelpBox();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.TeamsMatchUIMain"), new GUIContent("Main Object"));
									EditorGUILayout.EndVertical();
									
									EditorGUILayout.Space();
									
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.TeamImagePlaceholder"), new GUIContent("Logo Placeholder"));
									EditorGUILayout.EndVertical();
									
									EditorGUILayout.Space();
									
									EditorGUILayout.LabelField("1st Team", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.firstTeamMatchStats"), new GUIContent("Score"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.firstTeamMatchStatsBackground"), new GUIContent("Background"));
									EditorGUILayout.EndVertical();
									
									EditorGUILayout.Space();
									
									EditorGUILayout.LabelField("2nd Team", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.secondTeamMatchStats"), new GUIContent("Score"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.secondTeamMatchStatsBackground"), new GUIContent("Background"));
									EditorGUILayout.EndVertical();
									
									// EditorGUILayout.Space();
									
									// EditorGUILayout.Space();
									// EditorGUILayout.Space();

									// script.dominationIndex = GUILayout.Toolbar(script.dominationIndex, new[]
									// {
									// 	new GUIContent("Domination Mode°", "If the [MATCH TARGET] is the POINT RETENTION and [POINTS COUNT] = 3."),
									// 	new GUIContent("Hard Point Mode°", "If the [MATCH TARGET] is the POINT RETENTION and [POINTS COUNT] = 1.")
									// }, toolbarSmallStyle);

									// EditorGUILayout.Space();

									// if (script.dominationIndex == 0)
									// {
									// 	// EditorGUILayout.LabelField("UI for Domination", EditorStyles.boldLabel);
									// 	// EditorGUILayout.HelpBox("", MessageType.Info);
									// 	
									// }
									// else if (script.dominationIndex == 1)
									// {
									// 	// EditorGUILayout.Space();
									// 	// EditorGUILayout.LabelField("UI for Hard Point", EditorStyles.boldLabel);
									// 	// EditorGUILayout.HelpBox("If the [Match Target] is Point Retention and [Points Count] = 1, these elements will be displayed.", MessageType.Info);
									// 	
									//
									// }
									// EditorGUILayout.BeginVertical("helpbox");
									// EditorGUILayout.Space();
									break;
								}
								case 1:
									// EditorGUILayout.LabelField("Not Teams UI", EditorStyles.boldLabel);

									BeginGreenHelpBox();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.NotTeamsMatchUIMain"), new GUIContent("Main Object"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();

									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.CurrentPlaceText"), new GUIContent("Player Place"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();
									EditorGUILayout.Space();

									EditorGUILayout.LabelField("Player Score", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.PlayerStats"), new GUIContent("Score"));
									// EditorGUILayout.Space();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.PlayerStatsBackground"), new GUIContent("Background"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.PlayerStatsHighlight"), new GUIContent("Background Highlight"));
									EditorGUILayout.EndVertical();
									EditorGUILayout.Space();
									EditorGUILayout.LabelField("1st Place Score", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.FirstPlaceStats"), new GUIContent("Score"));
									// EditorGUILayout.Space();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.FirstPlaceStatsBackground"), new GUIContent("Background"));
									EditorGUILayout.EndVertical();
									// EditorGUILayout.EndVertical();
									break;

								case 2:
									// EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.HelpBox("This mode depends on the [USE TEAMS] value, so you should also set the parameters in the [TEAMS UI] section.", MessageType.Info);
									EditorGUILayout.Space();

									BeginGreenHelpBox();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.DominationMain"), new GUIContent("Main Object"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();
									EditorGUILayout.Space();

									EditorGUILayout.LabelField("A Point", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.A_CurrentFill"), new GUIContent("Current Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.A_CapturedFill"), new GUIContent("Captured Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.A_ScreenTargetTexture"), new GUIContent("Screen Target Texture"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();

									EditorGUILayout.LabelField("B Point", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.B_CurrentFill"), new GUIContent("Current Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.B_CapturedFill"), new GUIContent("Captured Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.B_ScreenTargetTexture"), new GUIContent("Screen Target Texture"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();

									EditorGUILayout.LabelField("C Point", EditorStyles.boldLabel);
									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.C_CurrentFill"), new GUIContent("Current Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.C_CapturedFill"), new GUIContent("Captured Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.C_ScreenTargetTexture"), new GUIContent("Screen Target Texture"));
									EditorGUILayout.EndVertical();
									// EditorGUILayout.EndVertical();
									break;


								case 3:
									// EditorGUILayout.BeginVertical("helpbox");
									// if (!script.MultiplayerGameRoom.MatchStats.TeamsMatchUIMain || !script.MultiplayerGameRoom.MatchStats.RedTeamMatchStats || !script.MultiplayerGameRoom.MatchStats.BlueTeamMatchStats || !script.MultiplayerGameRoom.MatchStats.TeamImagePlaceholder)
									// {
									// 	EditorGUILayout.HelpBox("Also, fill all [Teams UI] values.", MessageType.Warning);
									// 	EditorGUILayout.Space();
									// }
									// else
									// {
									EditorGUILayout.HelpBox("This mode depends on the [USE TEAMS] value, so you should also set the parameters in the [TEAMS UI] section.", MessageType.Info);
									EditorGUILayout.Space();
									// }

									BeginGreenHelpBox();
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.HardPointMain"), new GUIContent("Main Object"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();

									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.HardPoint_CurrentFill"), new GUIContent("Current Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.HardPoint_CapturedFill"), new GUIContent("Captured Fill"));
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.HardPoint_ScreenTargetTexture"), new GUIContent("Screen Target Texture"));
									EditorGUILayout.EndVertical();

									EditorGUILayout.Space();

									EditorGUILayout.BeginVertical("helpbox");
									EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.HardPointTimer"), new GUIContent("Timer"));
									EditorGUILayout.EndVertical();
									// EditorGUILayout.EndVertical();
									break;
							}

							// EditorGUILayout.EndVertical();

							break;

						case 1:

							EditorGUILayout.BeginVertical("helpbox");
							script.teamsIndex = GUILayout.Toolbar(script.teamsIndex, new[]
							{
								new GUIContent("Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS ACTIVE. Players will be divided into teams and will be displayed in 2 lists."),
								new GUIContent("Not Teams UI°", "These elements are used for modes in which the [USE TEAMS] parameter IS NOT ACTIVE. All players will be displayed in 1 list."),
							}, toolbarSmallStyle);

							EditorGUILayout.Space();

							// EditorGUILayout.BeginVertical("helpbox");
							if (script.teamsIndex == 0)
							{
								// EditorGUILayout.LabelField("Teams UI", EditorStyles.boldLabel);
								// EditorGUILayout.BeginVertical("helpbox");

								BeginGreenHelpBox();
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.TeamsSurvivalMain"), new GUIContent("Main Object"));
								EditorGUILayout.EndVertical();

								EditorGUILayout.Space();
								EditorGUILayout.LabelField("1st Team", EditorStyles.boldLabel);
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.firstTeamLogoPlaceholder"), new GUIContent("Logo Placeholder"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.firstTeamPlayersList"), new GUIContent("Players list"));
								EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
								EditorGUILayout.LabelField("2nd Team", EditorStyles.boldLabel);
								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.secondTeamLogoPlaceholder"), new GUIContent("Logo Placeholder"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.secondTeamPlayersList"), new GUIContent("Players list"));
								// EditorGUILayout.EndVertical();
								EditorGUILayout.EndVertical();
							}
							else if (script.teamsIndex == 1)
							{
								BeginGreenHelpBox();
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.NotTeamsSurvivalMain"), new GUIContent("Main Object"));
								EditorGUILayout.EndVertical();

								EditorGUILayout.Space();

								EditorGUILayout.BeginVertical("helpbox");
								EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.MatchStats.playersList"), new GUIContent("Players List"));
								EditorGUILayout.EndVertical();
							}
							// EditorGUILayout.EndVertical();

							break;


					}

					// EditorGUILayout.EndVertical();
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.AdvancedMultiplayerRoomDeathScreens:

					script.deathScreensIndex = GUILayout.Toolbar(script.deathScreensIndex, new[]
					{
						new GUIContent("Reborn Timer°", "If the [USE RESPAWNS] parameter IS ACTIVE, this timer will be displayed after the player's death."),
						new GUIContent("Spectate Screen°", "If the [USE RESPAWNS] parameter IS NOT ACTIVE, the player won't be respawned. " + "\n" +
						                                   "But you can see other players after a character's death; these screen is needed for that."),
					}, toolbarSmallStyle);

					EditorGUILayout.Space();
					// EditorGUILayout.LabelField(new GUIContent("", ""), EditorStyles.boldLabel);
					// EditorGUILayout.BeginVertical("helpbox");

					if (script.deathScreensIndex == 0)
					{
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerAfterDeath.MainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerAfterDeath.LaunchButton"), new GUIContent("Launch Match Button"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerAfterDeath.RestartTimer"), new GUIContent("Timer"));
						EditorGUILayout.EndVertical();
					}
					else
					{
						// EditorGUILayout.LabelField(new GUIContent("", ""), EditorStyles.boldLabel);
						// EditorGUILayout.BeginVertical("helpbox");
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.MainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.PlayerStats"), new GUIContent("Status Text"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.ChangeCameraButton"), new GUIContent("Change Camera"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.MatchStatsButton"), new GUIContent("Show Stats"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.BackButton"), new GUIContent("Back"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.SpectateMenu.ExitButton"), new GUIContent("Exit"));
						EditorGUILayout.EndVertical();
						// EditorGUILayout.Space();
						// EditorGUILayout.BeginVertical("helpbox");
						// EditorGUILayout.EndVertical();
					}

					// EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.AdvancedMultiplayerRoomPreMatch:

					script.preMatchMenuIndex = GUILayout.Toolbar(script.preMatchMenuIndex, new[]
					{
						new GUIContent("Pre-match Game°", "This menu is displayed in a pre-match game."),
						new GUIContent("Start Match Timer°", "This is the timer before match."),
					}, toolbarSmallStyle);

					// EditorGUILayout.LabelField(new GUIContent("", "This menu is displayed in a pre-match game."), EditorStyles.boldLabel);
					EditorGUILayout.Space();

					// EditorGUILayout.BeginVertical("helpbox");

					if (script.preMatchMenuIndex == 0)
					{
						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PreMatchMenu.MainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.PreMatchMenu.Status"), new GUIContent("Status Text"));
						EditorGUILayout.EndVertical();
						// EditorGUILayout.EndVertical();
						// EditorGUILayout.EndVertical();
					}
					else
					{
						// EditorGUILayout.LabelField(new GUIContent("Start Match Timer°", ""), EditorStyles.boldLabel);

						BeginGreenHelpBox();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerBeforeMatch.MainObject"), new GUIContent("Main Object"));
						EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical("helpbox");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerBeforeMatch.StartMatchTimer"), new GUIContent("Timer"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedMultiplayerGameRoom.TimerBeforeMatch.Background"), new GUIContent("Background"));
						EditorGUILayout.EndVertical();
					}

					// EditorGUILayout.EndVertical();
					break;

				case UIHelper.MenuPages.BasicMultiplayerLobbyMainMenu:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.nicknameInputField"), new GUIContent("Nickname"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.Avatar"), new GUIContent("Avatar"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.ChangeAvatarButton"), new GUIContent("Change Avatar"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.ConnectionStatus"), new GUIContent("Connection Status"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.RegionsDropdown"), new GUIContent("Regions Dropdown"));
					EditorGUILayout.EndVertical();


					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.CurrentModeAndMap"), new GUIContent("Current Map"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.mapsButton"), new GUIContent("Select Map"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.ChangeCharacter"), new GUIContent("Change Character"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.PlayButton"), new GUIContent("Play"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.AllRoomsButton"), new GUIContent("All Rooms"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.CreateRoomButton"), new GUIContent("Create Room"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.settingsButton"), new GUIContent("Settings"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MainMenu.exitButton"), new GUIContent("Exit"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.BasicMultiplayerLobbyCharacters:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.mainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.weaponsScrollRect"), new GUIContent("Weapons Scroll Rect°", "Images of all guns that the character has will be shown here."));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.weaponPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.upButton"), new GUIContent("->"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.downButton"), new GUIContent("<-"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CharactersMenu.backButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();
					
					break;
				case UIHelper.MenuPages.BasicMultiplayerLobbyAvatars:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AvatarsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AvatarsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.avatarPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AvatarsMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();
					
					break;

				case UIHelper.MenuPages.BasicMultiplayerLobbyMaps:

					BeginGreenHelpBox();

					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MapsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MapsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.mapPlaceholder"), new GUIContent("Placeholder"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.MapsMenu.backButton"), new GUIContent("Close Menu"));

					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.BasicMultiplayerLobbyAllRooms:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AllRoomsMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AllRoomsMenu.scrollRect"), new GUIContent("Scroll Rect"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.roomInfoPlaceholder"), new GUIContent("Placeholder"));

					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AllRoomsMenu.Password"), new GUIContent("Password"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AllRoomsMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.AllRoomsMenu.JoinButton"), new GUIContent("Join"));
					EditorGUILayout.EndVertical();


					break;

				case UIHelper.MenuPages.BasicMultiplayerLobbyCreateRoom:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.MainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.GameName"), new GUIContent("Game Name"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.Password"), new GUIContent("Password"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.currentMap"), new GUIContent("Selected Map"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.maxPlayers"), new GUIContent("Max Players"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.maxPlayersText"), new GUIContent("Text"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.gameDuration"), new GUIContent("Match Duration"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.gameDurationText"), new GUIContent("Text"));
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.canKillEachOther"), new GUIContent("Kill Mode"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.CreateButton"), new GUIContent("Create"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameLobby.CreateRoomMenu.BackButton"), new GUIContent("Close Menu"));
					EditorGUILayout.EndVertical();

					break;

				case UIHelper.MenuPages.BasicMultiplayerRoomPause:

					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.notTeamsScrollRect"), new GUIContent("Players List"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.playerInfoPlaceholder"), new GUIContent("Player Placeholder"));

					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.currentGameAndPassword"), new GUIContent("Game Name & Password°", "If a player creates his own room, this text will display the name and password of that room."));
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.exitButton"), new GUIContent("Exit"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.resumeButton"), new GUIContent("Resume"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.PauseMenu.optionsButton"), new GUIContent("Options"));
					EditorGUILayout.EndVertical();

					break;
				
				case UIHelper.MenuPages.BasicMultiplayerRoomMatch:
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.MatchStats.MatchTimer"), new GUIContent("Match Timer"));
					
					break;
				
				case UIHelper.MenuPages.BasicMultiplayerRoomGameOver:
					
					BeginGreenHelpBox();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.GameOverMenu.notTeamsMainObject"), new GUIContent("Main Object"));
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.Space();
					
					EditorGUILayout.BeginVertical("helpbox");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.GameOverMenu.matchStatsButton"), new GUIContent("Show Stats"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("basicMultiplayerGameRoom.GameOverMenu.exitButton"), new GUIContent("Exit"));
					EditorGUILayout.EndVertical();
					break;
			}

			if (script.currentMenuPage != UIHelper.MenuPages.MainMenu && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayer && script.currentMenuPage != UIHelper.MenuPages.SinglePlayer && script.currentMenuPage != UIHelper.MenuPages.CharacterUI && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayerLobby && script.currentMenuPage != UIHelper.MenuPages.AdvancedMultiplayerRoom
			    && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayer && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayerLobby && script.currentMenuPage != UIHelper.MenuPages.BasicMultiplayerRoom)
				EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			Repaint();

			serializedObject.ApplyModifiedProperties();

			// DrawDefaultInspector();

			if (GUI.changed)
				EditorUtility.SetDirty(script.gameObject);
		}

		void BeginGreenHelpBox()
		{
			var backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;
			EditorGUILayout.BeginVertical("helpbox");
			GUI.backgroundColor = backgroundColor;
		}

		void AddEmptyMainObjects()
		{
			AddEmptyMainObject(9, script.gameOptions.MainObject);
			AddEmptyMainObject(8, script.SinglePlayerGame.SinglePlayerGamePause.MainObject);
			AddEmptyMainObject(10, script.SinglePlayerGame.SinglePlayerGameGameOver.MainObject);
			AddEmptyMainObject(13, script.basicMultiplayerGameLobby.MainMenu.MainObject);
			AddEmptyMainObject(14, script.basicMultiplayerGameLobby.CharactersMenu.mainObject);
			AddEmptyMainObject(15, script.basicMultiplayerGameLobby.MapsMenu.MainObject);
			AddEmptyMainObject(16, script.basicMultiplayerGameLobby.AllRoomsMenu.MainObject);
			AddEmptyMainObject(17, script.basicMultiplayerGameLobby.CreateRoomMenu.MainObject);
			AddEmptyMainObject(18, script.basicMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain);
			AddEmptyMainObject(21, script.advancedMultiplayerGameLobby.MainMenu.MainObject);
			AddEmptyMainObject(22, script.advancedMultiplayerGameLobby.AllRoomsMenu.MainObject);
			AddEmptyMainObject(23, script.advancedMultiplayerGameLobby.CreateRoomMenu.MainObject);
			AddEmptyMainObject(24, script.advancedMultiplayerGameLobby.GameModesMenu.MainObject);
			AddEmptyMainObject(25, script.advancedMultiplayerGameLobby.MapsMenu.MainObject);
			AddEmptyMainObject(26, script.advancedMultiplayerGameLobby.AvatarsMenu.MainObject);
			AddEmptyMainObject(27, script.advancedMultiplayerGameLobby.LoadoutMenu.mainObject);
			AddEmptyMainObject(28, script.advancedMultiplayerGameLobby.CharactersMenu.mainObject);
			AddEmptyMainObject(31, script.advancedMultiplayerGameRoom.PauseMenu.teamsPauseMenuMain);
			AddEmptyMainObject(32, script.advancedMultiplayerGameRoom.PauseMenu.notTeamsPauseMenuMain);
			AddEmptyMainObject(34, script.advancedMultiplayerGameRoom.GameOverMenu.teamsMainObject);
			AddEmptyMainObject(35, script.advancedMultiplayerGameRoom.GameOverMenu.notTeamsMainObject);
			AddEmptyMainObject(37, script.advancedMultiplayerGameRoom.PreMatchMenu.MainObject);
			AddEmptyMainObject(38, script.advancedMultiplayerGameRoom.TimerBeforeMatch.MainObject);
			AddEmptyMainObject(40, script.advancedMultiplayerGameRoom.TimerAfterDeath.MainObject);
			AddEmptyMainObject(41, script.advancedMultiplayerGameRoom.SpectateMenu.MainObject);
			AddEmptyMainObject(43, script.advancedMultiplayerGameRoom.MatchStats.TeamsMatchUIMain);
			AddEmptyMainObject(44, script.advancedMultiplayerGameRoom.MatchStats.NotTeamsMatchUIMain);
			AddEmptyMainObject(45, script.advancedMultiplayerGameRoom.MatchStats.DominationMain);
			AddEmptyMainObject(46, script.advancedMultiplayerGameRoom.MatchStats.HardPointMain);
			AddEmptyMainObject(47, script.advancedMultiplayerGameRoom.MatchStats.TeamsSurvivalMain);
			AddEmptyMainObject(48, script.advancedMultiplayerGameRoom.MatchStats.NotTeamsSurvivalMain);
			AddEmptyMainObject(49, script.basicMultiplayerGameRoom.GameOverMenu.notTeamsMainObject);
			AddEmptyMainObject(50, script.basicMultiplayerGameLobby.AvatarsMenu.MainObject);
		}
	}
}
