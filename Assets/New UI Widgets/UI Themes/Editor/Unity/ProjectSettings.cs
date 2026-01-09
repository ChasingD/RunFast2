#if UNITY_EDITOR
namespace UIThemes.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEditor;
	using UnityEngine;

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
			var references = ThemesReferences.Instance;
			var assemblies = ScriptingDefineSymbols.GetState("UIWIDGETS_ASMDEF");
			if (assemblies.Any)
			{
				references.AssemblyDefinitions = true;
				assemblies.Disable();
				AssetDatabase.Refresh();
			}

			if (ProjectOptions.ThemesOnly)
			{
				if (AssemblyDefinitionGenerator.IsCreated)
				{
					references.AssemblyDefinitions = true;
				}
				else if (references.AssemblyDefinitions)
				{
					AssemblyDefinitionGenerator.Create();
				}
			}

			var installed = ScriptingDefineSymbols.GetState("UITHEMES_INSTALLED");
			if (!installed.All)
			{
				installed.Enable();
			}

			if (ProjectOptions.TMPro.Available && !ProjectOptions.TMPro.State.All)
			{
				ProjectOptions.TMPro.EnableForAll();
			}

			if (ProjectOptions.ThemesOnly)
			{
				AssetDatabase.Refresh();
			}
		}

		class Labels
		{
			public readonly GUIContent AssemblyDefinitions = new GUIContent("Assembly Definitions");

			public readonly GUIContent TMPro = new GUIContent("TextMeshPro Support");

			public readonly GUIContent Addressable = new GUIContent("Addressables Support");

			public readonly GUIContent Selectable = new GUIContent("Attach Default Selectable Colors");

			public readonly GUIContent AttachUIOnly = new GUIContent("Attach to UI only", "Add ThemeTarget component only to game objects with RectTransform component.");

			public readonly GUIContent WrappersSettings = new GUIContent("Wrappers Settings");

			public readonly GUIContent WrappersFolder = new GUIContent("   Folder");

			public readonly GUIContent WrappersNamespace = new GUIContent("   Namespace");

			public readonly GUIContent WrappersGenerate = new GUIContent("   Generate");
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
			/// Package with UIThemes only.
			/// </summary>
			public readonly bool ThemesOnly = false;

			/// <summary>
			/// Labels.
			/// </summary>
			public readonly Labels Labels = new Labels();

			/// <summary>
			/// Enable/disable assembly definitions.
			/// </summary>
			public readonly ScriptableSetting AssemblyDefinitions = new ScriptableSetting(
				() => ThemesReferences.Instance.AssemblyDefinitions && AssemblyDefinitionGenerator.IsCreated,
				state =>
				{
					ThemesReferences.Instance.AssemblyDefinitions = state;
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
			/// Enable/disable TextMeshPro support.
			/// </summary>
			public readonly ScriptingDefineSymbol TMPro = new ScriptingDefineSymbol(
				"UIWIDGETS_TMPRO_SUPPORT",
				() => UtilitiesEditor.GetType("TMPro.TextMeshProUGUI") != null);

			/// <summary>
			/// Enable/disable Addressable support.
			/// </summary>
			public readonly ScriptingDefineSymbol Addressable = new ScriptingDefineSymbol(
				"UITHEMES_ADDRESSABLE_SUPPORT",
				() => UtilitiesEditor.GetType("UnityEngine.AddressableAssets.Addressables") != null);

			/// <summary>
			/// Init done.
			/// </summary>
			public bool InitDone;
		}

		class UI
		{
			readonly Options options;

			public readonly List<Block> Blocks;

			ThemesReferences themesSettings;

			GUIOptions guiOptions;

			public UI(Options settingsUI, GUIOptions options)
			{
				this.options = settingsUI;
				themesSettings = ThemesReferences.Instance;
				this.guiOptions = options;

				Blocks = new List<Block>()
				{
					new Block(this.options.AssemblyDefinitions, this.options.Labels.AssemblyDefinitions),
					new Block(this.options.TMPro, this.options.Labels.TMPro),
					new Block(this.options.Addressable, this.options.Labels.Addressable),
				};
			}

			public void Show()
			{
				foreach (var block in Blocks)
				{
					block.Show(guiOptions);
					EditorGUILayout.Space(6);
				}

				AttachUIOnlySettings();
				SelectableSettings();
				WrappersSettings();
			}

			void AttachUIOnlySettings()
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(options.Labels.AttachUIOnly, guiOptions.Name);
				themesSettings.AttachUIOnly = EditorGUILayout.Toggle(themesSettings.AttachUIOnly);
				EditorGUILayout.EndHorizontal();
			}

			void SelectableSettings()
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(options.Labels.Selectable, guiOptions.Name);
				themesSettings.AttachDefaultSelectable = EditorGUILayout.Toggle(themesSettings.AttachDefaultSelectable);
				EditorGUILayout.EndHorizontal();
			}

			void WrappersSettings()
			{
				var valid = true;

				EditorGUILayout.LabelField(options.Labels.WrappersSettings, guiOptions.Name);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(options.Labels.WrappersFolder, guiOptions.Name);
				themesSettings.WrappersFolder = EditorGUILayout.TextField(themesSettings.WrappersFolder);
				if (GUILayout.Button("...", GUILayout.Width(30f)))
				{
					var folder = UIThemes.Editor.Utilities.SelectAssetsPath(themesSettings.WrappersFolder, "Select a directory for wrappers scripts");
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
					ShowHelpBox("Specified directory is not exists.", MessageType.Error);
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(options.Labels.WrappersNamespace, guiOptions.Name);
				themesSettings.WrappersNamespace = EditorGUILayout.TextField(themesSettings.WrappersNamespace);
				if (string.IsNullOrEmpty(themesSettings.WrappersNamespace))
				{
					valid = false;
					ShowHelpBox("Namespace is not specified.", MessageType.Error);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(!valid);
				EditorGUILayout.LabelField(options.Labels.WrappersGenerate, guiOptions.Name);
				themesSettings.GenerateWrappers = EditorGUILayout.Toggle(themesSettings.GenerateWrappers);

				var style = GUI.skin.GetStyle("HelpBox");
				style.richText = true;
				EditorGUILayout.LabelField(string.Empty, "Automatically generate wrapper scripts for properties which available only via reflection after using the <i>Theme Attach</i> command.", style);
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// GUI options.
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
			public readonly GUILayoutOption[] Status = new GUILayoutOption[] { GUILayout.Width(170) };
		}

		[DomainReloadExclude]
		static readonly Options ProjectOptions = new Options();

		/// <summary>
		/// Get required assemblies.
		/// </summary>
		/// <param name="output">Assemblies name list.</param>
		public static void GetAssemblies(List<string> output)
		{
			output.Add("Unity.TextMeshPro");
		}

		static void ShowHelpBox(string text, MessageType type)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.indentLevel += 1;
			EditorGUILayout.HelpBox(text, type);
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.EndHorizontal();
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
#if !UIWIDGETS_INSTALLED
		[SettingsProvider]
#endif
		public static SettingsProvider CreateSettingsProvider()
		{
			var provider = new SettingsProvider("Project/UI Themes", SettingsScope.Project)
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