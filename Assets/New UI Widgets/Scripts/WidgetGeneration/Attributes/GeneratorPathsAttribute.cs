namespace UIWidgets.WidgetGeneration
{
	using System;
	using System.IO;

	/// <summary>
	/// Paths attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class GeneratorPathsAttribute : Attribute
	{
		/// <summary>
		/// Scene path.
		/// </summary>
		public readonly string Scene;

		/// <summary>
		/// Scripts path.
		/// </summary>
		public readonly string Scripts;

		/// <summary>
		/// Editor path.
		/// </summary>
		public readonly string Editor;

		/// <summary>
		/// Prefabs path.
		/// </summary>
		public readonly string Prefabs;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorPathsAttribute"/> class.
		/// </summary>
		/// <param name="scene">Scene path.</param>
		/// <param name="scripts">Scripts path.</param>
		/// <param name="editor">Editor path.</param>
		/// <param name="prefabs">Prefabs path.</param>
		public GeneratorPathsAttribute(string scene, string scripts, string editor, string prefabs)
		{
			Scene = scene;
			Scripts = scripts;
			Editor = editor;
			Prefabs = prefabs;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorPathsAttribute"/> class.
		/// </summary>
		/// <param name="basePath">Base path.</param>
		public GeneratorPathsAttribute(string basePath)
		{
			Scene = basePath;
			Scripts = basePath + Path.DirectorySeparatorChar + "Scripts";
			Editor = basePath + Path.DirectorySeparatorChar + "Editor";
			Prefabs = basePath + Path.DirectorySeparatorChar + "Prefabs";
		}
	}
}