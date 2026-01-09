#if UNITY_EDITOR
namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Serialization;
#if UIWIDGETS_TMPRO_SUPPORT && UIWIDGETS_TMPRO_4_0_OR_NEWER && !UNITY_2023_2_OR_NEWER
	using FontAsset = UnityEngine.TextCore.Text.FontAsset;
#elif UIWIDGETS_TMPRO_SUPPORT
	using FontAsset = TMPro.TMP_FontAsset;
#else
	using FontAsset = UnityEngine.ScriptableObject;
#endif

	/// <summary>
	/// Widgets references.
	/// </summary>
	[Serializable]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/project-settings.html")]
	public class WidgetsReferences : ScriptableObject
	{
		[SerializeField]
		bool assemblyDefinitions = true;

		/// <summary>
		/// Assembly definitions.
		/// </summary>
		public bool AssemblyDefinitions
		{
#if UIWIDGETS_ASMDEF_DISABLED
			get => false;
#else
			get => assemblyDefinitions;
#endif

			set => Change(ref assemblyDefinitions, value);
		}

		[SerializeField]
		[FormerlySerializedAs("instantiatePrefabs")]
		bool instantiateWidgets;

		/// <summary>
		/// Instantiate widgets.
		/// </summary>
		public bool InstantiateWidgets
		{
			get => instantiateWidgets;

			set => Change(ref instantiateWidgets, value);
		}

		[SerializeField]
		bool attachTheme = false;

		/// <summary>
		/// Attach default to the widgets created from the menu.
		/// </summary>
		public bool AttachTheme
		{
			get => attachTheme;

			set => Change(ref attachTheme, value);
		}

		[SerializeField]
		bool useWhiteSprite = false;

		/// <summary>
		/// Sets white sprite for the Image components without sprite.
		/// Prevents rare bugs when such Images are displayed as black.
		/// </summary>
		public bool UseWhiteSprite
		{
			get => useWhiteSprite;

			set => Change(ref useWhiteSprite, value);
		}

		[SerializeField]
		PrefabsMenu current;

		/// <summary>
		/// Current theme.
		/// </summary>
		public PrefabsMenu Current
		{
			get => current;

			set => Change(ref current, value);
		}

		[SerializeField]
		FontAsset defaultFont;

		/// <summary>
		/// Default font.
		/// </summary>
		public FontAsset DefaultFont
		{
			get => defaultFont;

			set => Change(ref defaultFont, value);
		}

		[SerializeField]
		string widgetsNamespace = "{DataTypeNamespace}.Widgets";

		/// <summary>
		/// Widgets Generator: Widgets namespace.
		/// </summary>
		public string WidgetsNamespace
		{
			get => widgetsNamespace;

			set => Change(ref widgetsNamespace, value);
		}

		[SerializeField]
		string widgetsEditorNamespace = "{DataTypeNamespace}.Widgets.Editor";

		/// <summary>
		/// Widgets Generator: Widgets.Editor namespace.
		/// </summary>
		public string WidgetsEditorNamespace
		{
			get => widgetsEditorNamespace;

			set => Change(ref widgetsEditorNamespace, value);
		}

		[SerializeField]
		string widgetsPathBase = "{DataTypePath}" + Path.DirectorySeparatorChar + "Widgets{DataTypeName}";

		/// <summary>
		/// Widgets Generator: Path to save created files.
		/// </summary>
		public string WidgetsPathBase
		{
			get => widgetsPathBase;

			set => Change(ref widgetsPathBase, value);
		}

		[SerializeField]
		string widgetsPathEditor = "{DataTypePath}" + Path.DirectorySeparatorChar + "Widgets{DataTypeName}" + Path.DirectorySeparatorChar + "Editor";

		/// <summary>
		/// Widgets Generator: Path to save created editor scripts.
		/// </summary>
		public string WidgetsPathEditor
		{
			get => widgetsPathEditor;

			set => Change(ref widgetsPathEditor, value);
		}

		[SerializeField]
		string widgetsPathScripts = "{DataTypePath}" + Path.DirectorySeparatorChar + "Widgets{DataTypeName}" + Path.DirectorySeparatorChar + "Scripts";

		/// <summary>
		/// Widgets Generator: Path to save created widgets scripts.
		/// </summary>
		public string WidgetsPathScripts
		{
			get => widgetsPathScripts;

			set => Change(ref widgetsPathScripts, value);
		}

		[SerializeField]
		string widgetsPathPrefabs = "{DataTypePath}" + Path.DirectorySeparatorChar + "Widgets{DataTypeName}" + Path.DirectorySeparatorChar + "Prefabs";

		/// <summary>
		/// Widgets Generator: Path to save created prefabs.
		/// </summary>
		public string WidgetsPathPrefabs
		{
			get => widgetsPathPrefabs;

			set => Change(ref widgetsPathPrefabs, value);
		}

		/// <summary>
		/// Change field.
		/// </summary>
		/// <typeparam name="T">Type of field.</typeparam>
		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		/// <returns>true if field is changed; otherwise false.</returns>
		protected bool Change<T>(ref T field, T value)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			EditorUtility.SetDirty(this);
			return true;
		}

		/// <summary>
		/// Get widgets namespace.
		/// </summary>
		/// <param name="dataNamespace">Data type namespace.</param>
		/// <returns>Widgets namespace.</returns>
		public string GetWidgetsNamespace(string dataNamespace) => WidgetsNamespace.Replace("{DataTypeNamespace}", dataNamespace);

		/// <summary>
		/// Get widgets editor namespace.
		/// </summary>
		/// <param name="dataNamespace">Data type namespace.</param>
		/// <returns>Widgets editor namespace.</returns>
		public string GetWidgetsEditorNamespace(string dataNamespace) => WidgetsEditorNamespace.Replace("{DataTypeNamespace}", dataNamespace);

		/// <summary>
		/// Is widgets references exists?
		/// </summary>
		public static bool InstanceExists
		{
			get
			{
				var refs = Resources.FindObjectsOfTypeAll<WidgetsReferences>();
				if (refs.Length > 0)
				{
					return true;
				}

				var guids = AssetDatabase.FindAssets("t:" + typeof(WidgetsReferences).FullName);
				if (guids.Length > 0)
				{
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Widgets references.
		/// </summary>
		public static WidgetsReferences Instance => StaticFields.Instance.References.WidgetsReferences;
	}
}
#endif