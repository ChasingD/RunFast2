namespace UIThemes
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Registry of properties created with reflection.
	/// </summary>
	public static class ReflectionWrappersRegistry
	{
		/// <summary>
		/// Action on registry data changed.
		/// </summary>
		public static event Action OnChanged
		{
			add => StaticFields.Instance.ReflectionWrappersRegistryOnChanged += value;

			remove => StaticFields.Instance.ReflectionWrappersRegistryOnChanged -= value;
		}

		/// <summary>
		/// Add.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="property">Property or field name.</param>
		public static void Add(Type type, string property) => StaticFields.Instance.ReflectionWrappersRegistryAdd(type, property);

		/// <summary>
		/// Get all registered properties.
		/// </summary>
		/// <returns>Registered properties.</returns>
		public static IReadOnlyDictionary<Type, IReadOnlyCollection<string>> All() => StaticFields.Instance.ReflectionWrappersRegistryAll();
	}
}