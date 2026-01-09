#if UNITY_EDITOR
namespace UIWidgets
{
	using System.IO;
	using UnityEditor;

	/// <summary>
	/// Editor references.
	/// </summary>
	public class EditorReferences
	{
		DataBindTemplates dataBindTemplates;

		/// <summary>
		/// DataBindTemplates.
		/// </summary>
		public DataBindTemplates DataBindTemplates
		{
			get
			{
				if (dataBindTemplates == null)
				{
					dataBindTemplates = UtilitiesEditor.LoadAssetWithGUID<DataBindTemplates>(ReferenceGUID.DataBindTemplates);
				}

				return dataBindTemplates;
			}
		}

		PrefabsMenu prefabsMenu;

		/// <summary>
		/// PrefabsMenu.
		/// </summary>
		public PrefabsMenu PrefabsMenu
		{
			get
			{
				if (prefabsMenu == null)
				{
					prefabsMenu = UtilitiesEditor.LoadAssetWithGUID<PrefabsMenu>(ReferenceGUID.PrefabsMenu);
				}

				return prefabsMenu;
			}
		}

		PrefabsTemplates prefabsTemplates;

		/// <summary>
		/// PrefabsTemplates.
		/// </summary>
		public PrefabsTemplates PrefabsTemplates
		{
			get
			{
				if (prefabsTemplates == null)
				{
					prefabsTemplates = UtilitiesEditor.LoadAssetWithGUID<PrefabsTemplates>(ReferenceGUID.PrefabsTemplates);
				}

				return prefabsTemplates;
			}
		}

		ScriptsTemplates scriptsTemplates;

		/// <summary>
		/// ScriptsTemplates.
		/// </summary>
		public ScriptsTemplates ScriptsTemplates
		{
			get
			{
				if (scriptsTemplates == null)
				{
					scriptsTemplates = UtilitiesEditor.LoadAssetWithGUID<ScriptsTemplates>(ReferenceGUID.ScriptsTemplates);
				}

				return scriptsTemplates;
			}
		}

		WidgetsReferences widgetsReferences;

		/// <summary>
		/// Widgets references.
		/// </summary>
		public WidgetsReferences WidgetsReferences
		{
			get
			{
				if (widgetsReferences == null)
				{
					widgetsReferences = FindWidgetsReferences();
				}

				return widgetsReferences;
			}
		}

		string WidgetsReferencesDefaultPath => Path.Combine(ReferenceGUID.Instance.EditorFolder, "Widgets References.asset");

		WidgetsReferences FindWidgetsReferences()
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(WidgetsReferences).FullName);
			if (guids.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				return AssetDatabase.LoadAssetAtPath<WidgetsReferences>(path);
			}

			if (File.Exists(WidgetsReferencesDefaultPath))
			{
				return AssetDatabase.LoadAssetAtPath<WidgetsReferences>(WidgetsReferencesDefaultPath);
			}

			return CreateWidgetsReferences();
		}

		/// <summary>
		/// Create references.
		/// </summary>
		/// <returns>Created instance.</returns>
		WidgetsReferences CreateWidgetsReferences()
		{
			var folder = ReferenceGUID.Instance.EditorFolder;
			if (string.IsNullOrEmpty(folder))
			{
				return null;
			}

			return UtilitiesEditor.CreateScriptableObjectAsset<WidgetsReferences>(WidgetsReferencesDefaultPath);
		}
	}
}
#endif