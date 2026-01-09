#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UIWidgets.Attributes;
	using UnityEditor;
	using UnityEngine;
#if UIWIDGETS_TMPRO_SUPPORT && UIWIDGETS_TMPRO_4_0_OR_NEWER && !UNITY_2023_2_OR_NEWER
	using FontAsset = UnityEngine.TextCore.Text.FontAsset;
#elif UIWIDGETS_TMPRO_SUPPORT
	using FontAsset = TMPro.TMP_FontAsset;
#else
	using FontAsset = UnityEngine.ScriptableObject;
#endif

	/// <summary>
	/// Project settings.
	/// </summary>
	public static class ProjectSettings
	{
		class Init : AssetPostprocessor
		{
			#if UNITY_2022_3_OR_NEWER
			public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
			#else
			public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
			#endif
			{
				#if UNITY_2022_3_OR_NEWER
				if (!didDomainReload)
				{
					return;
				}
				#endif

				if (ProjectOptions.InitDone)
				{
					return;
				}

				ProjectOptions.InitDone = true;

				// Enable() with delay because AssetDatabase.FindAssets() does not find asset otherwise
				Run();
			}

			static async void Run()
			{
				await Task.Delay(10000);
				Enable();
			}
		}

		static void Enable()
		{
			var references = WidgetsReferences.Instance;
			var assemblies = ScriptingDefineSymbols.GetState("UIWIDGETS_ASMDEF");
			if (assemblies.Any)
			{
				references.AssemblyDefinitions = true;
				assemblies.Disable();
			}

			var instantiate = ScriptingDefineSymbols.GetState("UIWIDGETS_INSTANTIATE_PREFABS");
			if (instantiate.Any)
			{
				instantiate.Disable();
				references.InstantiateWidgets = true;
			}

			ScriptingDefineSymbols.Rename("I2_LOCALIZATION_SUPPORT", "UIWIDGETS_I2LOCALIZATION_SUPPORT");

			if (AssemblyDefinitionGenerator.IsCreated)
			{
				references.AssemblyDefinitions = true;
			}
			else if (references.AssemblyDefinitions)
			{
				AssemblyDefinitionGenerator.Create();
			}

			var installed = ScriptingDefineSymbols.GetState("UIWIDGETS_INSTALLED");
			if (!installed.All)
			{
				installed.Enable();
			}

			if (ProjectOptions.R3.Available && !ProjectOptions.R3.State.All)
			{
				ProjectOptions.R3.EnableForAll();
			}

			if (ProjectOptions.TMPro.Available && !ProjectOptions.TMPro.State.All)
			{
				ProjectOptions.TMPro.EnableForAll();
			}

			if (ProjectOptions.UnityLocalization.Available && !ProjectOptions.UnityLocalization.State.All)
			{
				ProjectOptions.UnityLocalization.EnableForAll();
			}

			if (ProjectOptions.I2Localization.Available && !ProjectOptions.I2Localization.State.All)
			{
				ProjectOptions.I2Localization.EnableForAll();
			}
		}

		class Labels
		{
			public readonly GUIContent AssemblyDefinitions = new GUIContent("Assembly Definitions");

			public readonly GUIContent InstantiateWidgets = new GUIContent("Instantiate Widgets");

			public readonly GUIContent R3 = new GUIContent("Widgets Generator: R3 Support");

			#if UITHEMES_INSTALLED
			public readonly GUIContent StylesLabel = new GUIContent("Styles or Themes");

			public readonly GUIContent AttachThemeLabel = new GUIContent("Attach Default Theme", "Attach default Theme to the widgets created from the menu.");
			#endif

			public readonly GUIContent UseWhiteSprite = new GUIContent("Use White Sprite");

			public readonly GUIContent TMPro = new GUIContent("TextMeshPro Support");

			public readonly GUIContent DataBind = new GUIContent("Data Bind for Unity Support");

			public readonly GUIContent UnityLocalization = new GUIContent("Unity Localization Support");

			public readonly GUIContent I2Localization = new GUIContent("I2 Localization Support");

			public readonly GUIContent GeneratorSettings = new GUIContent("Widgets Generator Settings");

			public readonly GUIContent GeneratorNamespace = new GUIContent("Namespace:");

			public readonly GUIContent GeneratorEditorNamespace = new GUIContent("Editor Namespace:");

			public readonly GUIContent GeneratorPathScene = new GUIContent("Scene Path:");

			public readonly GUIContent GeneratorPathScripts = new GUIContent("Scripts Path:");

			public readonly GUIContent GeneratorPathPrefabs = new GUIContent("Prefabs Path:");

			public readonly GUIContent GeneratorPathEditor = new GUIContent("Editor Path:");

			#if UITHEMES_INSTALLED
			public readonly GUIContent UIThemesSettings = new GUIContent("UI Themes Settings");

			public readonly GUIContent Addressable = new GUIContent("UI Themes: Addressables Support");

			public readonly GUIContent UIThemesAttachUIOnly = new GUIContent("Attach to UI only:", "Attach Theme only to game objects with RectTransform component.");

			public readonly GUIContent UIThemesSelectable = new GUIContent("Attach Default Selectable Colors:");

			public readonly GUIContent UIThemesWrappersFolder = new GUIContent("Wrappers Folder:");

			public readonly GUIContent UIThemesWrappersNamespace = new GUIContent("Wrappers Namespace:");

			public readonly GUIContent UIThemesWrappersGenerate = new GUIContent("Generate Wrappers:");
			#endif
		}

		class Block
		{
			readonly ISetting symbol;

			readonly GUIContent label;

			readonly Action<ISetting> info;

			readonly string buttonEnable;

			readonly string buttonDisable;

			public Block(ISetting symbol, GUIContent label, Action<ISetting> info = null, string buttonEnable = "Enable", string buttonDisable = "Disable")
			{
				this.symbol = symbol;
				this.label = label;
				this.info = info;

				this.buttonEnable = buttonEnable;
				this.buttonDisable = buttonDisable;
			}

			void EnableForAll()
			{
				if (symbol.Available && symbol.Enabled && !symbol.IsFullSupport)
				{
					EditorGUILayout.BeginVertical();
					ShowHelpBox("Feature is not enabled for all BuildTargets.", MessageType.Info);

					if (GUILayout.Button("Enable for All", GUILayout.ExpandWidth(true)))
					{
						symbol.EnableForAll();
					}

					EditorGUILayout.EndVertical();
				}
			}

			public void Show(GUIOptions options)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(label, options.Name);

				if (symbol.IsFullSupport)
				{
					var status = new GUIContent(symbol.Status);
					EditorGUILayout.LabelField(status, options.Status);
				}
				else
				{
					var color = EditorStyles.label.normal.textColor;
					EditorStyles.label.normal.textColor = Color.red;

					var status = new GUIContent(symbol.Status, "Support is not enabled for all BuildTargets.");
					EditorGUILayout.LabelField(status, options.Status);

					EditorStyles.label.normal.textColor = color;
				}

				if (symbol.Available)
				{
					if (symbol.Enabled)
					{
						if (GUILayout.Button(buttonDisable))
						{
							symbol.Enabled = false;
						}
					}
					else
					{
						if (GUILayout.Button(buttonEnable))
						{
							symbol.Enabled = true;
						}
					}
				}

				EditorGUILayout.EndHorizontal();

				if (symbol.Available)
				{
					EnableForAll();

					info?.Invoke(symbol);
				}
			}
		}

		class Options
		{
			/// <summary>
			/// R3 warning.
			/// </summary>
			public const string R3Warning = "In case of error CS0234 you must add reference to R3.Unity in the UIWidgets.Editor.asmdef.\n" +
				"Or you can disable assembly definitions.";

			/// <summary>
			/// DataBind warning.
			/// </summary>
			public const string DataBindWarning = "Data Bind for Unity does not have assembly definitions by default.\n" +
				"You must create them and add references to UIWidgets.asmdef," +
				" UIWidgets.Editor.asmdef, and UIWidgets.Samples.asmdef.\n" +
				"Or you can disable assembly definitions.";

			/// <summary>
			/// I2Localization warning.
			/// </summary>
			public const string I2LocalizationWarning = "I2 Localization does not have assembly definitions by default.\n" +
				"You must create them and add references to UIWidgets.asmdef," +
				" UIWidgets.Editor.asmdef, and UIWidgets.Samples.asmdef.\n" +
				"Or you can disable assembly definitions.";

			/// <summary>
			/// Use white sprite description.
			/// </summary>
			public const string UseWhiteSpriteDescription = "Sets white sprite for the Image components without sprite.\n" +
				"Prevents rare bugs when such Images are displayed as black.";

			/// <summary>
			/// Labels.
			/// </summary>
			public readonly Labels Labels = new Labels();

			/// <summary>
			/// Enable/disable assembly definitions.
			/// </summary>
			public readonly ScriptableSetting AssemblyDefinitions = new ScriptableSetting(
				() => WidgetsReferences.Instance.AssemblyDefinitions && AssemblyDefinitionGenerator.IsCreated,
				state =>
				{
					WidgetsReferences.Instance.AssemblyDefinitions = state;
					if (state)
					{
						AssemblyDefinitionGenerator.Create();
					}
					else
					{
						AssemblyDefinitionGenerator.Delete();
					}
				});

			/// <summary>
			/// Toggle widgets instance type.
			/// </summary>
			public readonly ScriptableSetting InstantiateWidgets = new ScriptableSetting(
				() => WidgetsReferences.Instance.InstantiateWidgets,
				x => WidgetsReferences.Instance.InstantiateWidgets = x,
				enabledText: "Prefabs",
				disabledText: "Copies");

			/// <summary>
			/// Widgets generator R3 support.
			/// </summary>
			public readonly ScriptingDefineSymbol R3 = new ScriptingDefineSymbol(
				"UIWIDGETS_R3_SUPPORT",
				() => UtilitiesEditor.GetType("R3.R3LoopRunners") != null,
				package: "R3.Unity");

			#if UITHEMES_INSTALLED
			/// <summary>
			/// Enable/disable legacy styles.
			/// </summary>
			public readonly ScriptingDefineSymbol LegacyStyles = new ScriptingDefineSymbol(
				"UIWIDGETS_LEGACY_STYLE",
				() => true,
				enabledText: "Styles (obsolete)",
				disabledText: "UI Themes");
			#endif

			/// <summary>
			/// Enable/disable TextMeshPro support.
			/// </summary>
			public readonly ScriptingDefineSymbol TMPro = new ScriptingDefineSymbol(
				"UIWIDGETS_TMPRO_SUPPORT",
				() => UtilitiesEditor.GetType("TMPro.TextMeshProUGUI") != null,
				package: "TextMeshPro");

			/// <summary>
			/// Enable/disable DataBind support.
			/// </summary>
			public readonly ScriptingDefineSymbol DataBind = new ScriptingDefineSymbol(
				"UIWIDGETS_DATABIND_SUPPORT",
				() => UtilitiesEditor.GetType("Slash.Unity.DataBind.Core.Presentation.DataProvider") != null,
				package: "DataBind");

			/// <summary>
			/// Enable/disable Unity Localization support.
			/// </summary>
			public readonly ScriptingDefineSymbol UnityLocalization = new ScriptingDefineSymbol(
				"UIWIDGETS_UNITY_LOCALIZATION_SUPPORT",
				() => UtilitiesEditor.GetType("UnityEngine.Localization.LocalizedString") != null,
				package: "Unity Localization");

			/// <summary>
			/// Enable/disable I2Localization support.
			/// </summary>
			public readonly ScriptingDefineSymbol I2Localization = new ScriptingDefineSymbol(
				"UIWIDGETS_I2LOCALIZATION_SUPPORT",
				() => UtilitiesEditor.GetType("I2.Loc.LocalizationManager") != null,
				package: "I2 Localization");

			#if UITHEMES_INSTALLED
			/// <summary>
			/// Enable/disable Addressable support.
			/// </summary>
			public readonly ScriptingDefineSymbol Addressable = new ScriptingDefineSymbol(
				"UITHEMES_ADDRESSABLE_SUPPORT",
				() => UtilitiesEditor.GetType("UnityEngine.AddressableAssets.Addressables") != null,
				package: "Addressables");

			/// <summary>
			/// Attach default theme to the widgets created from menu.
			/// </summary>
			public readonly ScriptableSetting AttachTheme = new ScriptableSetting(
				() => WidgetsReferences.Instance.AttachTheme,
				x => WidgetsReferences.Instance.AttachTheme = x);
			#endif

			/// <summary>
			/// Use white sprite.
			/// </summary>
			public readonly ScriptableSetting UseWhiteSprite = new ScriptableSetting(
				() => WidgetsReferences.Instance.UseWhiteSprite,
				x => WidgetsReferences.Instance.UseWhiteSprite = x);

			/// <summary>
			/// Init done.
			/// </summary>
			public bool InitDone;
		}

		class UI
		{
			readonly Options projectOptions;

			readonly Action<ISetting> TMProInfo;

			readonly Action<ISetting> R3Info;

			readonly Action<ISetting> DataBindInfo;

			readonly Action<ISetting> I2LocalizationInfo;

			readonly Action<ISetting> UseWhiteSpriteInfo;

			public readonly List<Block> Blocks;

			WidgetsReferences widgetsSettings;

#if UITHEMES_INSTALLED
			UIThemes.ThemesReferences themesSettings;
#endif

			GUIOptions guiOptions;

			GUIStyle boldLabel;

			public UI(Options settingsUI, GUIOptions options)
			{
				projectOptions = settingsUI;
				widgetsSettings = WidgetsReferences.Instance;
#if UITHEMES_INSTALLED
				themesSettings = UIThemes.ThemesReferences.Instance;
#endif
				guiOptions = options;

				boldLabel = new GUIStyle(EditorStyles.label);
				boldLabel.fontStyle = FontStyle.Bold;

				TMProInfo = (ISetting setting) =>
				{
					if (setting.Available && setting.Enabled)
					{
						WidgetsReferences.Instance.DefaultFont = EditorGUILayout.ObjectField("Default Font", WidgetsReferences.Instance.DefaultFont, typeof(FontAsset), false) as FontAsset;

						ShowHelpBox(
							"You can replace all Unity text with TMPro text by using" +
							" the context menu \"UI / New UI Widgets / Replace Unity Text with TextMeshPro\"" +
							" or by using the menu \"Window / New UI Widgets / Replace Unity Text with TextMeshPro\".",
							MessageType.Info);
					}
				};

				R3Info = (ISetting setting) =>
				{
					if (!setting.Available)
					{
						return;
					}

					if (!projectOptions.AssemblyDefinitions.Enabled)
					{
						return;
					}

					ShowHelpBox(Options.R3Warning, MessageType.Warning);
				};

				DataBindInfo = (ISetting setting) =>
				{
					if (!setting.Available)
					{
						return;
					}

					if (!projectOptions.AssemblyDefinitions.Enabled)
					{
						return;
					}

					ShowHelpBox(Options.DataBindWarning, MessageType.Warning);
				};

				I2LocalizationInfo = (ISetting setting) =>
				{
					if (!setting.Available)
					{
						return;
					}

					if (!projectOptions.AssemblyDefinitions.Enabled)
					{
						return;
					}

					ShowHelpBox(Options.I2LocalizationWarning, MessageType.Warning);
				};

				UseWhiteSpriteInfo = (ISetting setting) =>
				{
					ShowHelpBox(Options.UseWhiteSpriteDescription, MessageType.Info);
				};

				Blocks = new List<Block>()
				{
#if !UIWIDGETS_ASMDEF_DISABLED
					new Block(projectOptions.AssemblyDefinitions, projectOptions.Labels.AssemblyDefinitions),
#endif
					new Block(projectOptions.InstantiateWidgets, projectOptions.Labels.InstantiateWidgets, buttonDisable: "Create Copies", buttonEnable: "Create Prefabs"),
					new Block(projectOptions.R3, projectOptions.Labels.R3, R3Info),
					#if UITHEMES_INSTALLED
					new Block(projectOptions.LegacyStyles, projectOptions.Labels.StylesLabel, buttonDisable: "Use UI Themes", buttonEnable: "Use Obsolete Styles"),
					new Block(projectOptions.Addressable, projectOptions.Labels.Addressable),
					new Block(projectOptions.AttachTheme, projectOptions.Labels.AttachThemeLabel),
					#endif
					
					new Block(projectOptions.UseWhiteSprite, projectOptions.Labels.UseWhiteSprite, UseWhiteSpriteInfo),

					// ScriptsRecompile.SetStatus(ReferenceGUID.TMProStatus, ScriptsRecompile.StatusSymbolsAdded);
					new Block(projectOptions.TMPro, projectOptions.Labels.TMPro, TMProInfo),

					new Block(projectOptions.UnityLocalization, projectOptions.Labels.UnityLocalization),

					// ScriptsRecompile.SetStatus(ReferenceGUID.I2LocalizationStatus, ScriptsRecompile.StatusSymbolsAdded);
					new Block(projectOptions.I2Localization, projectOptions.Labels.I2Localization, I2LocalizationInfo),

					// ScriptsRecompile.SetStatus(ReferenceGUID.DataBindStatus, ScriptsRecompile.StatusSymbolsAdded);
					new Block(projectOptions.DataBind, projectOptions.Labels.DataBind, DataBindInfo),
				};
			}

			public void Show()
			{
				foreach (var block in Blocks)
				{
					block.Show(guiOptions);
					EditorGUILayout.Space(6);
				}

				GeneratorSettings();

#if UITHEMES_INSTALLED
				UIThemesSettings();
#endif
			}

#if UITHEMES_INSTALLED
			void UIThemesSettings()
			{
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesSettings, boldLabel, guiOptions.Name);

				EditorGUI.indentLevel++;
				AttachUIOnlySettings();
				SelectableSettings();
				WrappersSettings();
				EditorGUI.indentLevel--;
			}

			void AttachUIOnlySettings()
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesAttachUIOnly, guiOptions.Name);
				themesSettings.AttachUIOnly = EditorGUILayout.Toggle(themesSettings.AttachUIOnly);
				EditorGUILayout.EndHorizontal();
			}

			void SelectableSettings()
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesSelectable, guiOptions.Name);
				themesSettings.AttachDefaultSelectable = EditorGUILayout.Toggle(themesSettings.AttachDefaultSelectable);
				EditorGUILayout.EndHorizontal();
			}

			void WrappersSettings()
			{
				var valid = true;

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesWrappersFolder, guiOptions.Name);
				themesSettings.WrappersFolder = EditorGUILayout.TextField(themesSettings.WrappersFolder);
				if (GUILayout.Button("...", GUILayout.Width(30f)))
				{
					var folder = UtilitiesEditor.SelectAssetsPath(themesSettings.WrappersFolder, "Select a directory for wrappers scripts");
					if (string.IsNullOrEmpty(folder))
					{
						valid = false;
					}
					else
					{
						themesSettings.WrappersFolder = folder;
					}
				}

				EditorGUILayout.EndHorizontal();

				if (!System.IO.Directory.Exists(themesSettings.WrappersFolder))
				{
					EditorGUI.indentLevel += 1;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.HelpBox("Specified directory is not exists.", MessageType.Error);
					if (GUILayout.Button("Create Directory"))
					{
						System.IO.Directory.CreateDirectory(themesSettings.WrappersFolder);
						AssetDatabase.Refresh();
					}

					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel -= 1;
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesWrappersNamespace, guiOptions.Name);
				themesSettings.WrappersNamespace = EditorGUILayout.TextField(themesSettings.WrappersNamespace);
				if (string.IsNullOrEmpty(themesSettings.WrappersNamespace))
				{
					valid = false;
					ShowHelpBox("Namespace is not specified.", MessageType.Error);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(!valid);
				EditorGUILayout.LabelField(projectOptions.Labels.UIThemesWrappersGenerate, guiOptions.Name);
				themesSettings.GenerateWrappers = EditorGUILayout.Toggle(themesSettings.GenerateWrappers);

				var style = GUI.skin.GetStyle("HelpBox");
				style.richText = true;
				EditorGUILayout.LabelField(string.Empty, "Automatically generate wrapper scripts for properties which available only via reflection after using the <i>Theme Attach</i> command.", style);
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
			}
#endif

			void GeneratorSettings()
			{
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorSettings, boldLabel, guiOptions.Name);

				EditorGUI.indentLevel++;

				// namespaces
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorNamespace, guiOptions.Name);
				widgetsSettings.WidgetsNamespace = EditorGUILayout.TextField(widgetsSettings.WidgetsNamespace);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorEditorNamespace, guiOptions.Name);
				widgetsSettings.WidgetsEditorNamespace = EditorGUILayout.TextField(widgetsSettings.WidgetsEditorNamespace);
				EditorGUILayout.EndHorizontal();

				// paths
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorPathScene, guiOptions.Name);
				widgetsSettings.WidgetsPathBase = EditorGUILayout.TextField(widgetsSettings.WidgetsPathBase);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorPathScripts, guiOptions.Name);
				widgetsSettings.WidgetsPathScripts = EditorGUILayout.TextField(widgetsSettings.WidgetsPathScripts);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorPathPrefabs, guiOptions.Name);
				widgetsSettings.WidgetsPathPrefabs = EditorGUILayout.TextField(widgetsSettings.WidgetsPathPrefabs);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(projectOptions.Labels.GeneratorPathEditor, guiOptions.Name);
				widgetsSettings.WidgetsPathEditor = EditorGUILayout.TextField(widgetsSettings.WidgetsPathEditor);
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// GUI Options.
		/// </summary>
		public class GUIOptions
		{
			/// <summary>
			/// Name.
			/// </summary>
			public readonly GUILayoutOption[] Name = new GUILayoutOption[] { GUILayout.Width(210) };

			/// <summary>
			/// Status.
			/// </summary>
			public readonly GUILayoutOption[] Status = new GUILayoutOption[] { GUILayout.Width(220) };
		}

		[DomainReloadExclude]
		static readonly Options ProjectOptions = new Options();

		static void ShowHelpBox(string text, MessageType type)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.indentLevel += 1;
			EditorGUILayout.HelpBox(text, type);
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Get required assemblies.
		/// </summary>
		/// <param name="output">Assemblies name list.</param>
		public static void GetAssemblies(List<string> output)
		{
			output.Add("UIThemes");
			output.Add("Unity.TextMeshPro");
			output.Add("Unity.InputSystem");
			output.Add("Unity.Localization");

			if (ProjectOptions.R3.Enabled)
			{
				Debug.LogWarning(Options.R3Warning);
			}

			if (ProjectOptions.DataBind.Enabled)
			{
				Debug.LogWarning(Options.DataBindWarning);
			}

			if (ProjectOptions.I2Localization.Enabled)
			{
				Debug.LogWarning(Options.I2LocalizationWarning);
			}
		}

		static IEnumerable<string> GetSearchKeywords<T>(T labels)
		{
			var result = new HashSet<string>();

			var fields = typeof(T).GetFields();
			foreach (var field in fields)
			{
				if (typeof(GUIContent).IsAssignableFrom(field.FieldType))
				{
					var label = ((GUIContent)field.GetValue(labels)).text.ToLower();
					result.Add(label);
				}
			}

			return result;
		}

		/// <summary>
		/// Create settings provider.
		/// </summary>
		/// <returns>Settings provider.</returns>
		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider()
		{
			var provider = new SettingsProvider("Project/New UI Widgets", SettingsScope.Project)
			{
				guiHandler = (searchContext) =>
				{
					var ui = new UI(ProjectOptions, new GUIOptions());
					ui.Show();
				},

				keywords = GetSearchKeywords(ProjectOptions.Labels),
			};

			return provider;
		}
	}
}
#endif