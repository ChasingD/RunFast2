#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System;
	using System.IO;

	/// <summary>
	/// Data paths.
	/// </summary>
	[Serializable]
	public class DataPath
	{
		/// <summary>
		/// Path to save created files.
		/// </summary>
		public string Base;

		/// <summary>
		/// Path to save created editor scripts.
		/// </summary>
		public string Editor;

		/// <summary>
		/// Path to save created widgets scripts.
		/// </summary>
		public string Scripts;

		/// <summary>
		/// Path to save created prefab.
		/// </summary>
		public string Prefabs;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataPath"/> class.
		/// </summary>
		/// <param name="settings">Settings.</param>
		/// <param name="typeName">Type name.</param>
		/// <param name="path">Path.</param>
		public DataPath(WidgetsReferences settings, string typeName, string path)
		{
			Base = settings.WidgetsPathBase.Replace("{DataTypePath}", path).Replace("{DataTypeName}", typeName);
			Scripts = settings.WidgetsPathScripts.Replace("{DataTypePath}", path).Replace("{DataTypeName}", typeName);
			Editor = settings.WidgetsPathEditor.Replace("{DataTypePath}", path).Replace("{DataTypeName}", typeName);
			Prefabs = settings.WidgetsPathPrefabs.Replace("{DataTypePath}", path).Replace("{DataTypeName}", typeName);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataPath"/> class.
		/// </summary>
		/// <param name="typeName">Type name.</param>
		/// <param name="path">Path.</param>
		public DataPath(string typeName, string path)
		{
			Base = path + Path.DirectorySeparatorChar + "Widgets" + typeName;
			Scripts = Base + Path.DirectorySeparatorChar + "Scripts";
			Editor = Base + Path.DirectorySeparatorChar + "Editor";
			Prefabs = Base + Path.DirectorySeparatorChar + "Prefabs";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataPath"/> class.
		/// </summary>
		/// <param name="generatorPaths">Paths.</param>
		public DataPath(GeneratorPathsAttribute generatorPaths)
		{
			Base = generatorPaths.Scene;
			Scripts = generatorPaths.Scripts;
			Editor = generatorPaths.Editor;
			Prefabs = generatorPaths.Prefabs;
		}

		/// <summary>
		/// Is valid editor folder?
		/// </summary>
		/// <returns>true if valid editor folder; otherwise false.</returns>
		public bool IsValidEditor()
		{
			var path = new DirectoryInfo(Editor);
			while (path != null)
			{
				if (path.Name == "Editor")
				{
					return true;
				}

				path = path.Parent;
			}

			return false;
		}

		bool Create(string path, out string error)
		{
			error = string.Empty;

			if (File.Exists(path))
			{
				error = path;
				return false;
			}

			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch
				{
					error = path;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Create paths.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <returns>true if paths were successfully created; otherwise false.</returns>
		public bool Create(out string error) => Create(Base, out error) && Create(Editor, out error) && Create(Scripts, out error) && Create(Prefabs, out error);
	}
}
#endif