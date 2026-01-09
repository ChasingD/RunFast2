namespace UIWidgets.WidgetGeneration
{
	using System;

	/// <summary>
	/// Ignore attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class GeneratorIgnoreAttribute : Attribute
	{
	}
}