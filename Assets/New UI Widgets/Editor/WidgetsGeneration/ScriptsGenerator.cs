#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using UIWidgets.Extensions;
	using UIWidgets.Pool;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Widget scripts generator.
	/// </summary>
	public class ScriptsGenerator : IFormattable
	{
		/// <summary>
		/// Script data.
		/// </summary>
		public class ScriptData
		{
			/// <summary>
			/// Type.
			/// </summary>
			public readonly string Type;

			/// <summary>
			/// Class name.
			/// </summary>
			public readonly string ClassName;

			/// <summary>
			/// Template.
			/// </summary>
			public readonly string Template;

			/// <summary>
			/// Code.
			/// </summary>
			public string Content
			{
				get;
				private set;
			}

			/// <summary>
			/// Can create.
			/// </summary>
			public readonly bool CanCreate;

			/// <summary>
			/// Path.
			/// </summary>
			public readonly string Path;

			/// <summary>
			/// File exists.
			/// </summary>
			public readonly bool Exists;

			/// <summary>
			/// Allow to overwrite file.
			/// </summary>
			public bool Overwrite;

			/// <summary>
			/// Need save file.
			/// </summary>
			public bool NeedSave => Changed && (!Exists || Overwrite);

			/// <summary>
			/// File data changed or file not exists.
			/// </summary>
			public bool Changed
			{
				get;
				private set;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ScriptData"/> class.
			/// </summary>
			/// <param name="type">Type.</param>
			/// <param name="className">Class name.</param>
			/// <param name="template">Template.</param>
			/// <param name="path">Path.</param>
			/// <param name="canCreate">Can script be created?</param>
			public ScriptData(string type, string className, string template, string path, bool canCreate)
			{
				Type = type;
				ClassName = className;
				Template = template;
				Path = path;
				CanCreate = canCreate;

				Exists = File.Exists(path);
				Overwrite = false;
			}

			/// <summary>
			/// Set content.
			/// </summary>
			/// <param name="content">Content.</param>
			public void SetContent(string content)
			{
				Content = content;
				Changed = !Exists || (File.ReadAllText(Path) != Content);
			}
		}

		/// <summary>
		/// Class info.
		/// </summary>
		public ClassInfo Info;

		/// <summary>
		/// Paths to save created files.
		/// </summary>
		protected DataPath Paths;

		/// <summary>
		/// Scripts templates.
		/// </summary>
		public readonly Dictionary<string, ScriptData> Scripts = new Dictionary<string, ScriptData>();

		/// <summary>
		/// Script templates values.
		/// </summary>
		protected Dictionary<string, string> TemplateValues;

		/// <summary>
		/// Script editor templates.
		/// </summary>
		protected HashSet<string> EditorTemplates = new HashSet<string>()
		{
			"MenuOptions",
			"PrefabGenerator",
			"PrefabGeneratorAutocomplete",
			"PrefabGeneratorTable",
			"PrefabGeneratorScene",
		};

		/// <summary>
		/// Prefabs.
		/// </summary>
		protected List<string> Prefabs = new List<string>()
		{
			"ListView",
			"DragInfo",
			"Combobox",
			"ComboboxMultiselect",
			"Table",
			"TileView",
			"TreeView",
			"TreeGraph",
			"PickerListView",
			"PickerTreeView",
			"Autocomplete",
			"AutoCombobox",
			"Tooltip",
		};

		/// <summary>
		/// Allow autocomplete script and widget.
		/// </summary>
		protected bool AllowAutocomplete;

		/// <summary>
		/// Allow table widget.
		/// </summary>
		protected bool AllowTable;

		/// <summary>
		/// Allow test item generation.
		/// </summary>
		protected bool AllowItem;

		/// <summary>
		/// String builder.
		/// </summary>
		protected StringBuilder StrBuilder = new StringBuilder();

		/// <summary>
		/// Add script data.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="template">Template.</param>
		/// <param name="canCreate">Can script be created?</param>
		protected void AddScriptData(string type, TextAsset template, bool canCreate = true)
		{
			var class_name = type + Info.ShortTypeName;
			var dir = EditorTemplates.Contains(type) ? Paths.Editor : Paths.Scripts;
			var path = dir + Path.DirectorySeparatorChar + class_name + ".cs";

			Scripts[type] = new ScriptData(type, class_name, template.text, path, canCreate);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptsGenerator"/> class.
		/// </summary>
		/// <param name="info">Class info.</param>
		/// <param name="paths">Paths to save created files.</param>
		public ScriptsGenerator(ClassInfo info, DataPath paths)
		{
			Info = info;
			Paths = paths;

			var templates = ScriptsTemplates.Instance;

			AllowAutocomplete = Info.AutocompleteField != null;
			AllowTable = Info.TextFields.Count > 0;
			AllowItem = Info.ParameterlessConstructor;

			// collections
			AddScriptData("Autocomplete", templates.Autocomplete, AllowAutocomplete);
			AddScriptData("AutoCombobox", templates.AutoCombobox, AllowAutocomplete);
			AddScriptData("Combobox", templates.Combobox);
			AddScriptData("ListView", templates.ListView);
			AddScriptData("ListViewComponent", templates.ListViewComponent);
			AddScriptData("TreeGraph", templates.TreeGraph);
			AddScriptData("TreeGraphComponent", templates.TreeGraphComponent);
			AddScriptData("TreeView", templates.TreeView);
			AddScriptData("TreeViewComponent", templates.TreeViewComponent);

			// collections support
			AddScriptData("Comparers", templates.Comparers);
			AddScriptData("ListViewDragSupport", templates.ListViewDragSupport);
			AddScriptData("ListViewDropSupport", templates.ListViewDropSupport);
			AddScriptData("TreeViewDropSupport", templates.TreeViewDropSupport);
			AddScriptData("TreeViewNodeDragSupport", templates.TreeViewNodeDragSupport);
			AddScriptData("TreeViewNodeDropSupport", templates.TreeViewNodeDropSupport);
			AddScriptData("Tooltip", templates.Tooltip);
			AddScriptData("TooltipViewer", templates.TooltipViewer);

			// dialogs
			AddScriptData("PickerListView", templates.PickerListView);
			AddScriptData("PickerTreeView", templates.PickerTreeView);

			// test
			AddScriptData("Test", templates.Test);
			AddScriptData("TestItem", AllowItem ? templates.TestItem : templates.TestItemOff);

			// menu
			AddScriptData("MenuOptions", templates.MenuOptions);

			// generators
			AddScriptData("PrefabGenerator", templates.PrefabGenerator);
			AddScriptData("PrefabGeneratorAutocomplete", AllowAutocomplete ? templates.PrefabGeneratorAutocomplete : templates.PrefabGeneratorAutocompleteOff);
			AddScriptData("PrefabGeneratorTable", AllowTable ? templates.PrefabGeneratorTable : templates.PrefabGeneratorTableOff);
			AddScriptData("PrefabGeneratorScene", templates.PrefabGeneratorScene);
		}

		void SetAvailableScripts()
		{
			foreach (var template in Scripts)
			{
				if (template.Value.CanCreate)
				{
					Info.Scripts[template.Key] = !File.Exists(template.Value.Path);
				}
			}
		}

		void SetAvailablePrefabs()
		{
			foreach (var prefab in Prefabs)
			{
				if (CanCreateWidget(prefab))
				{
					Info.Prefabs[prefab] = !File.Exists(Prefab2Filename(prefab));
				}
			}
		}

		void SetAvailableScenes()
		{
			Info.Scenes["TestScene"] = !File.Exists(Scene2Filename(Info.ShortTypeName));
		}

		void SetTemplateValues()
		{
			var label_listview = Info.ParameterlessConstructor
				? "The left ListView and TileView display the same list.\\r\\nYou can Drag-and-Drop items between ListView, TileView and TreeView."
				: "Test data is not available because of a data type\\r\\ndoes not have a parameterless constructor.";
			TemplateValues = new Dictionary<string, string>()
			{
				{ "WidgetsNamespace", Info.WidgetsNamespace },
				{ "WidgetsEditorNamespace", Info.WidgetsEditorNamespace },
				{ "SourceClassShortName", Info.ShortTypeName },
				{ "SourceClass", Info.FullTypeName },
				{ "AutocompleteField", Info.AutocompleteField?.FieldName },
				{ "AutocompleteFieldWrapperField", Info.AutocompleteField?.WrapperField },
				{ "ComparersEnum", "ComparersFields" + Info.ShortTypeName },
				{ "Info", UtilitiesEditor.Serialize(Info) },
				{ "Paths", UtilitiesEditor.Serialize(Paths) },
				{ "TextType", UtilitiesEditor.GetFriendlyTypeName(Info.TextFieldType) },
				{ "AutocompleteInput", UtilitiesEditor.GetFriendlyTypeName(Info.InputField) },
				{ "AutocompleteText", UtilitiesEditor.GetFriendlyTypeName(Info.InputText) },
				{ "PrefabsMenuGUID", Info.PrefabsMenuGUID },
				{ "LabelListView", label_listview },
			};
		}

		/// <summary>
		/// Generate files.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0603:Delegate allocation from a method group", Justification = "Required.")]
		public void Generate()
		{
			SetAvailableScripts();
			SetAvailablePrefabs();
			SetAvailableScenes();

			OverwriteRequestWindow.Open(this, GenerateScripts);
		}

		void GenerateScriptCode()
		{
			var menu = ScriptableObject.CreateInstance<PrefabsMenuGenerated>();

			StrBuilder.Append(Paths.Prefabs);
			StrBuilder.Append(Path.DirectorySeparatorChar);
			StrBuilder.Append("PrefabsMenu");
			StrBuilder.Append(Info.ShortTypeName);
			StrBuilder.Append(".asset");
			var menu_path = StrBuilder.ToString();
#if CSHARP_7_3_OR_NEWER
			StrBuilder.Clear();
#else
			StrBuilder = new StringBuilder();
#endif

			AssetDatabase.CreateAsset(menu, menu_path);
			Info.PrefabsMenuGUID = AssetDatabase.AssetPathToGUID(menu_path);

			SetTemplateValues();

			foreach (var script in Scripts.Values)
			{
				var code = string.Format(script.Template, this);
				script.SetContent(code);
			}
		}

		void GenerateScripts()
		{
			if (!Paths.IsValidEditor())
			{
				Debug.LogError(string.Format("There is no Editor folder in the Editor path:\r\n{0}\r\nEditor scripts should be in the Editor folder or in one of nested folder.", Paths.Editor));
				return;
			}

			if (!Paths.Create(out var error))
			{
				Debug.LogError(string.Format("Cannot create specified path:\r\n{0}", error));
				return;
			}

			if (Info.Scenes["TestScene"])
			{
				if (!Compatibility.SceneSave())
				{
					EditorUtility.DisplayDialog("Widget Generation", "Please save scene to continue.", "OK");
					return;
				}

				Compatibility.SceneNew();
			}

			GenerateScriptCode();

			try
			{
				var progress = 0;
				ProgressUpdate(progress);

				foreach (var template in Scripts.Values)
				{
					if (Info.Scripts.TryGetValue(template.Type, out var can_create) && can_create)
					{
						File.WriteAllText(template.Path, template.Content);
					}

					progress++;
					ProgressUpdate(progress);
				}
			}
			catch (Exception)
			{
				EditorUtility.ClearProgressBar();
				throw;
			}

			AssetDatabase.Refresh();
		}

		void ProgressUpdate(int progress)
		{
			if (progress < Scripts.Count)
			{
				EditorUtility.DisplayProgressBar("Widget Generation", "Step 1. Creating scripts.", progress / (float)Scripts.Count);
			}
			else
			{
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>
		/// Is widget can be created?
		/// </summary>
		/// <param name="name">Widget name.</param>
		/// <returns>True if widget can be created; otherwise false.</returns>
		protected virtual bool CanCreateWidget(string name)
		{
			return name switch
			{
				"Autocomplete" => AllowAutocomplete,
				"AutoCombobox" => AllowAutocomplete,
				"Table" => AllowTable,
				_ => true,
			};
		}

		/// <summary>
		/// Get prefab filename by widget name.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>Filename.</returns>
		public string Prefab2Filename(string type)
		{
			StrBuilder.Append(Paths.Prefabs);
			StrBuilder.Append(Path.DirectorySeparatorChar);
			StrBuilder.Append(type);
			StrBuilder.Append(Info.ShortTypeName);
			StrBuilder.Append(".prefab");

			var filename = StrBuilder.ToString();
#if CSHARP_7_3_OR_NEWER
			StrBuilder.Clear();
#else
			StrBuilder = new StringBuilder();
#endif

			return filename;
		}

		/// <summary>
		/// Get prefab filename by widget name.
		/// </summary>
		/// <param name="scene">Scene.</param>
		/// <returns>Filename.</returns>
		public string Scene2Filename(string scene)
		{
			return Paths.Base + Path.DirectorySeparatorChar + scene + ".unity";
		}

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (TemplateValues.ContainsKey(format))
			{
				return TemplateValues[format];
			}

			var cls = "Class";
			if (format.EndsWith(cls))
			{
				var key = format.Substring(0, format.Length - cls.Length);
				if (Scripts.ContainsKey(key))
				{
					return Scripts[key].ClassName;
				}
			}

			var pos = format.IndexOf("@");
			if (pos != -1)
			{
				return ToStringList(format, formatProvider);
			}

			throw new ArgumentOutOfRangeException("Unsupported format: " + format);
		}

		readonly List<Type> TypesInt = new List<Type>()
		{
			typeof(decimal),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
		};

		readonly List<Type> TypesFloat = new List<Type>()
		{
			typeof(float),
			typeof(double),
		};

		readonly List<Type> TypesColor = new List<Type>()
		{
			typeof(Color),
			typeof(Color32),
		};

		/// <summary>
		/// Format fields list to list.
		/// </summary>
		/// <param name="predicate">Predicate to check if field should be included.</param>
		/// <param name="format">Format.</param>
		/// <param name="formatProvider">Format provider.</param>
		/// <param name="onlyFirst">Include only first field that match predicate.</param>
		/// <returns>The fields in the specified format.</returns>
		protected string FormatFields(Predicate<ClassField> predicate, string format, IFormatProvider formatProvider, bool onlyFirst = false)
		{
			using var _ = ListPool<ClassField>.Get(out var fields);

			foreach (var field in Info.Fields)
			{
				if (predicate(field))
				{
					fields.Add(field);
					if (onlyFirst)
					{
						break;
					}
				}
			}

			return fields.ToString(format, this, formatProvider);
		}

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed.")]
		protected string ToStringList(string format, IFormatProvider formatProvider)
		{
			var template = format.Split(new[] { "@" }, 2, StringSplitOptions.None);
			template[1] = template[1].Replace("[", "{").Replace("]", "}");

			return template[0] switch
			{
				"IfTMProText" => Info.IsTMProText ? string.Format(template[1], this) : string.Empty,
				"!IfTMProText" => !Info.IsTMProText ? string.Format(template[1], this) : string.Empty,
				"IfTMProInputField" => Info.IsTMProInputField ? string.Format(template[1], this) : string.Empty,
				"!IfTMProInputField" => !Info.IsTMProInputField ? string.Format(template[1], this) : string.Empty,
				"IfAutocomplete" => AllowAutocomplete ? string.Format(template[1], this) : string.Empty,
				"!IfAutocomplete" => !AllowAutocomplete ? string.Format(template[1], this) : string.Empty,
				"IfTable" => AllowTable ? string.Format(template[1], this) : string.Empty,
				"!IfTable" => !AllowTable ? string.Format(template[1], this) : string.Empty,
				"Fields" => Info.Fields.ToString(template[1], this, formatProvider),
				"TextFields" => Info.TextFields.ToString(template[1], this, formatProvider),
				"TextFieldsComparableGeneric" => Info.TextFieldsComparableGeneric.ToString(template[1], this, formatProvider),
				"TextFieldsComparableNonGeneric" => Info.TextFieldsComparableNonGeneric.ToString(template[1], this, formatProvider),
				"TextFieldFirst" => Info.TextFieldFirst.ToString(template[1], this, formatProvider),
				"ImageFields" => FormatFields(x => x.IsImage, template[1], formatProvider),
				"ImageFieldsNullable" => FormatFields(x => x.IsImage && x.IsNullable, template[1], formatProvider),
				"TreeViewFields" => FormatFields(x => x.WidgetFieldName != "TextAdapter" && x.WidgetFieldName != "Icon", template[1], formatProvider),
				"FieldsString" => FormatFields(x => x.CanWrite && x.ActualFieldType == typeof(string), template[1], formatProvider),
				"FieldsStringFirst" => FormatFields(x => x.CanWrite && x.ActualFieldType == typeof(string), template[1], formatProvider, true),
				"FieldsInt" => FormatFields(x => x.CanWrite && TypesInt.Contains(x.ActualFieldType), template[1], formatProvider),
				"FieldsFloat" => FormatFields(x => x.CanWrite && TypesFloat.Contains(x.ActualFieldType), template[1], formatProvider),
				"FieldsSprite" => FormatFields(x => x.ActualFieldType == typeof(Sprite), template[1], formatProvider),
				"FieldsSpriteWritable" => FormatFields(x => x.CanWrite && x.ActualFieldType == typeof(Sprite), template[1], formatProvider),
				"FieldsTexture2D" => FormatFields(x => x.CanWrite && x.ActualFieldType == typeof(Texture2D), template[1], formatProvider),
				"FieldsColor" => FormatFields(x => x.CanWrite && TypesColor.Contains(x.ActualFieldType), template[1], formatProvider),
				_ => throw new ArgumentOutOfRangeException("Unsupported format: " + format),
			};
		}
	}
}
#endif