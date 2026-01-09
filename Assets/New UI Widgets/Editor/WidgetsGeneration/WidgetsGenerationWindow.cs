#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System.IO;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Widgets generator window.
	/// </summary>
	public class WidgetsGenerationWindow : EditorWindow
	{
		/// <summary>
		/// Show window.
		/// </summary>
		[MenuItem("Window/New UI Widgets/Widgets Generator")]
		public static void Open()
		{
			Open(null, null);
		}

		/// <summary>
		/// Show window.
		/// </summary>
		/// <param name="script">Script.</param>
		/// <param name="info">Class info.</param>
		public static void Open(MonoScript script, ClassInfo info)
		{
			var window = GetWindow<WidgetsGenerationWindow>("Widgets Generator");
			window.minSize = new Vector2(520, 200);
			window.currentScript = script;
			window.typeInfo = info;
		}

		readonly GUIStyle styleLabel = new GUIStyle();

		readonly GUIStyle styleHeader = new GUIStyle();

		readonly GUILayoutOption[] scrollOptions = new GUILayoutOption[] { GUILayout.Height(150) };

		readonly GUILayoutOption[] errorOptions = new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.MaxHeight(150) };

		readonly GUIContent toggleLabel = new GUIContent(string.Empty, "Use in widgets");

		readonly GUILayoutOption[] toggleHeaderOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(30) };

		readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(20) };

		readonly GUILayoutOption[] fieldOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(200) };

		readonly GUILayoutOption[] autocompleteOptions = new GUILayoutOption[] { GUILayout.Width(150) };

		readonly GUILayoutOption[] autocompleteButtonOptions = new GUILayoutOption[] { GUILayout.Width(40) };

		readonly GUILayoutOption[] PathOptions = new GUILayoutOption[] { GUILayout.Width(100) };

		MonoScript previousScript;

		MonoScript currentScript;

		string previousType;

		string currentType;

		Vector2 scrollPosition;

		ClassInfo typeInfo;

		DataPath paths;

		/// <summary>
		/// Set styles.
		/// </summary>
		protected virtual void SetStyles()
		{
			styleLabel.margin = new RectOffset(4, 4, 2, 2);
			styleLabel.richText = true;

			styleHeader.margin = new RectOffset(4, 4, 0, 0);
			styleHeader.fontStyle = FontStyle.Bold;
		}

		/// <summary>
		/// Draw GUI.
		/// </summary>
		protected virtual void OnGUI()
		{
			SetStyles();

			GUILayout.Label("Widgets Generator", EditorStyles.boldLabel);
			currentScript = EditorGUILayout.ObjectField("Data Script", currentScript, typeof(MonoScript), false, new GUILayoutOption[] { }) as MonoScript;

			if ((previousScript != currentScript) || (typeInfo == null))
			{
				typeInfo = new ClassInfo(currentScript);
				UpdatePaths();

				previousScript = currentScript;

				previousType = typeInfo.FullTypeName;
				currentType = typeInfo.FullTypeName;
			}

			currentType = EditorGUILayout.TextField("Data Type", currentType);

			if (previousType != currentType)
			{
				typeInfo = new ClassInfo(currentType);
				UpdatePaths();
				previousType = currentType;
			}

			if (paths == null)
			{
				UpdatePaths();
			}

			if (!typeInfo.IsValid)
			{
				ShowErrors();
				return;
			}
			else
			{
				ShowNamespace();
				ShowPaths();
				ShowFields();
			}

			var button_label = "Generate Widgets";

			if (typeInfo.IsUnityObject)
			{
				GUILayout.Label("<b>Warning:</b>", styleLabel);
				GUILayout.Label("Class is derived from Unity.Object.\nUsing it as a data class can be a bad practice and lead to future problems.", styleLabel);
				button_label = "Continue Generation";
			}

			if (typeInfo.Fields.Count == 0)
			{
				GUILayout.Label("<b>Cannot generate widgets since all fields deselected.</b>", styleLabel);
				return;
			}

			if (GUILayout.Button(button_label))
			{
				if (!CheckPaths())
				{
					return;
				}

				var gen = new ScriptsGenerator(typeInfo, paths);
				gen.Generate();
				Close();
			}
		}

		void UpdatePaths()
		{
			var path = Utilities.IsNull(currentScript)
				? "Assets"
				: Path.GetDirectoryName(AssetDatabase.GetAssetPath(currentScript));
			paths = typeInfo?.Paths ?? new DataPath(WidgetsReferences.Instance, typeInfo.ShortTypeName, path);
		}

		bool CheckPaths()
		{
			if (!paths.IsValidEditor())
			{
				EditorUtility.DisplayDialog("Invalid Editor Path", string.Format("There is no Editor folder in the Editor path:\r\n{0}\r\nEditor scripts should be in the Editor folder or in one of nested folder.", paths.Editor), "OK");
				return false;
			}

			if (!paths.Create(out var error))
			{
				EditorUtility.DisplayDialog("Cannot Create Directory", string.Format("Cannot create specified path:\r\n{0}", error), "OK");
				return false;
			}

			return true;
		}

		void ShowNamespace()
		{
			EditorGUILayout.LabelField("Namespace", EditorStyles.boldLabel);
			typeInfo.WidgetsNamespace = ShowNamespace("Widgets:", typeInfo.WidgetsNamespace);
			typeInfo.WidgetsEditorNamespace = ShowNamespace("Editor:", typeInfo.WidgetsEditorNamespace);
		}

		string ShowNamespace(string label, string widgetsNamespace)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, PathOptions);
			widgetsNamespace = EditorGUILayout.TextField(widgetsNamespace);
			EditorGUILayout.EndHorizontal();

			return widgetsNamespace;
		}

		void ShowPaths()
		{
			if (paths == null)
			{
				return;
			}

			EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
			paths.Base = ShowPath("Scene:", paths.Base);
			paths.Scripts = ShowPath("Scripts:", paths.Scripts);
			paths.Prefabs = ShowPath("Prefabs:", paths.Prefabs);
			paths.Editor = ShowPath("Editor:", paths.Editor);
		}

		string ShowPath(string label, string path)
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(label, PathOptions);
			path = EditorGUILayout.TextField(path);
			if (GUILayout.Button("...", GUILayout.Width(30f)))
			{
				var folder = UtilitiesEditor.SelectAssetsPath(path, "Select a directory for wrappers scripts");
				if (!string.IsNullOrEmpty(folder))
				{
					path = folder;
				}
			}

			EditorGUILayout.EndHorizontal();

			return path;
		}

		void ShowFields()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, scrollOptions);

			GUILayout.Space(16f);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Use", styleHeader, toggleHeaderOptions);
			GUILayout.Label("Field", styleHeader, fieldOptions);
			GUILayout.Label("Autocomplete", styleHeader, autocompleteOptions);

			EditorGUILayout.EndHorizontal();

			foreach (var field in typeInfo.AllFields)
			{
				EditorGUILayout.BeginHorizontal();

				ShowField(field);

				EditorGUILayout.BeginVertical();
				GUILayout.Space(20f);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}

		void ShowField(ClassField field)
		{
			var allowed = typeInfo.Fields.Contains(field);

			GUILayout.Space(5f);
			if (allowed != EditorGUILayout.Toggle(toggleLabel, allowed, toggleOptions))
			{
				if (allowed)
				{
					typeInfo.Fields.Remove(field);
				}
				else
				{
					typeInfo.Fields.Add(field);
				}
			}

			GUILayout.Space(1f);
			GUILayout.Label(field.FieldName, fieldOptions);

			if (field.AllowAutocomplete)
			{
				if (field.FieldName == typeInfo.AutocompleteField.FieldName)
				{
					EditorGUILayout.LabelField("Used", autocompleteOptions);
				}
				else if (GUILayout.Button("Use", autocompleteButtonOptions))
				{
					typeInfo.AutocompleteField = field;
				}
			}
		}

		void ShowErrors()
		{
			GUILayout.Label("<b>Errors:</b>", styleLabel);
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, scrollOptions);
			foreach (var error in typeInfo.Errors)
			{
				GUILayout.Label(error, styleLabel, errorOptions);
			}

			EditorGUILayout.EndScrollView();
		}
	}
}
#endif