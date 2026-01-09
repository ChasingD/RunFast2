namespace UIWidgets.WidgetGeneration
{
	using System;

	/// <summary>
	/// Paths attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class GeneratorNamespaceAttribute : Attribute
	{
		/// <summary>
		/// Widgets namespace.
		/// </summary>
		public readonly string Widgets;

		/// <summary>
		/// Editor namespace.
		/// </summary>
		public readonly string Editor;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorNamespaceAttribute"/> class.
		/// </summary>
		/// <param name="widgets">Widgets.</param>
		/// <param name="editor">Editor.</param>
		public GeneratorNamespaceAttribute(string widgets, string editor = null)
		{
			Widgets = widgets;
			Editor = editor ?? (widgets + ".Editor");
		}
	}
}