namespace UIThemes
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	/// <summary>
	/// Static fields.
	/// </summary>
	public class StaticFields
	{
		#region ReflectionWrappersRegistry

		/// <summary>
		/// Types.
		/// </summary>
		readonly Dictionary<Type, HashSet<string>> ReflectionWrappersRegistryTypes = new Dictionary<Type, HashSet<string>>();

		/// <summary>
		/// Action on registry data changed.
		/// </summary>
		public event Action ReflectionWrappersRegistryOnChanged;

		/// <summary>
		/// Add.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="property">Property or field name.</param>
		public void ReflectionWrappersRegistryAdd(Type type, string property)
		{
			if (!ReflectionWrappersRegistryTypes.TryGetValue(type, out var properties))
			{
				properties = new HashSet<string>();
				ReflectionWrappersRegistryTypes[type] = properties;
			}

			properties.Add(property);

			ReflectionWrappersRegistryOnChanged?.Invoke();
		}

		/// <summary>
		/// Get all registered properties.
		/// </summary>
		/// <returns>Registered properties.</returns>
		public IReadOnlyDictionary<Type, IReadOnlyCollection<string>> ReflectionWrappersRegistryAll()
		{
			var result = new Dictionary<Type, IReadOnlyCollection<string>>();
			foreach (var item in ReflectionWrappersRegistryTypes)
			{
				result[item.Key] = item.Value;
			}

			return result;
		}

		#endregion

		/// <summary>
		/// Color comparer instance.
		/// </summary>
		public readonly ColorComparer ColorComparerInstance = new ColorComparer();

		/// <summary>
		/// Color32 comparer instance.
		/// </summary>
		public readonly Color32Comparer Color32ComparerInstance = new Color32Comparer();

		/// <summary>
		/// ExclusionList cache.
		/// </summary>
		public readonly Stack<ExclusionList> ExclusionListCache = new Stack<ExclusionList>();

		/// <summary>
		/// Types methods.
		/// </summary>
		public readonly Utilities.TypesMethods TypesMethods = new Utilities.TypesMethods();

#if UNITY_EDITOR

		/// <summary>
		/// Asset label for sprites for exclude from ThemeTarget list.
		/// </summary>
		public readonly string SpritesLabelExclude = "ui-themes-exclude";

		/// <summary>
		/// Asset label for white sprites.
		/// </summary>
		public readonly string SpritesLabelWhite = "ui-themes-white-sprite";

		/// <summary>
		/// White sprites.
		/// </summary>
		public readonly Dictionary<int, Tuple<bool, DateTime>> SpritesWhite = new Dictionary<int, Tuple<bool, DateTime>>();

		#region ThemesReferences

		ThemesReferences themesReferencesInstance;

		/// <summary>
		/// Themes references.
		/// </summary>
		public ThemesReferences ThemesReferencesInstance
		{
			get
			{
				if (themesReferencesInstance == null)
				{
					themesReferencesInstance = ThemesReferencesFind();
				}

				return themesReferencesInstance;
			}
		}

		string DefaultPath => Path.Combine(ReferencesGUIDs.AssetsFolder, "UI Themes References.asset");

		ThemesReferences ThemesReferencesFind()
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(ThemesReferences).FullName);
			if (guids.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				return AssetDatabase.LoadAssetAtPath<ThemesReferences>(path);
			}

			if (File.Exists(DefaultPath))
			{
				return AssetDatabase.LoadAssetAtPath<ThemesReferences>(DefaultPath);
			}

			return ThemesReferencesCreate();
		}

		/// <summary>
		/// Create ThemesReferences.
		/// </summary>
		/// <returns>Created instance.</returns>
		ThemesReferences ThemesReferencesCreate()
		{
			var folder = ReferencesGUIDs.AssetsFolder;
			if (string.IsNullOrEmpty(folder))
			{
				return null;
			}

			return UtilitiesEditor.CreateScriptableObjectAsset<ThemesReferences>(DefaultPath);
		}

		#endregion

		#region Styles

		readonly List<StyleSheet> styles = new List<StyleSheet>();

		StyleSheet defaultStyle;

		/// <summary>
		/// Add stylesheet.
		/// </summary>
		/// <param name="styleSheet">Stylesheet.</param>
		public void StyleSheetsAdd(StyleSheet styleSheet)
		{
			if (styles.Contains(styleSheet))
			{
				return;
			}

			styles.Add(styleSheet);
		}

		/// <summary>
		/// Get stylesheets.
		/// </summary>
		/// <returns>Stylesheets.</returns>
		public IReadOnlyCollection<StyleSheet> StyleSheetsAll()
		{
			for (var i = styles.Count - 1; i >= 0; i--)
			{
				if (styles[i] == null)
				{
					styles.RemoveAt(i);
				}
			}

			var style = GetDefaultStyleSheet();
			if (style != null)
			{
				styles.Add(style);
			}

			return styles;
		}

		StyleSheet GetDefaultStyleSheet()
		{
			if (defaultStyle != null)
			{
				return defaultStyle;
			}

			var path = AssetDatabase.GUIDToAssetPath(ReferencesGUIDs.EditorUSSGUID);
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			defaultStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
			return defaultStyle;
		}

		#endregion

		#region Types

		/// <summary>
		/// Types to friendly name.
		/// </summary>
		public readonly Dictionary<Type, string> Types2FriendlyName = new Dictionary<Type, string>();

		/// <summary>
		/// Types cache.
		/// </summary>
		public readonly Dictionary<string, Type> TypesCache = new Dictionary<string, Type>();

		/// <summary>
		/// Scripting symbols separator.
		/// </summary>
		public readonly char[] ScriptingSymbolsSeparator = new char[] { ';' };

		#endregion

#endif

		static StaticFields instance;

		/// <summary>
		/// Instance.
		/// </summary>
		public static StaticFields Instance => instance ??= new StaticFields();

		StaticFields()
		{
		}

#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		[DomainReload(nameof(instance))]
		static void StaticInit()
		{
			instance = null;
		}
#endif
	}
}