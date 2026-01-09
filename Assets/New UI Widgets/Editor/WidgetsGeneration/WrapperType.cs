#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using System;

	/// <summary>
	/// Information about wrapper type.
	/// Used to support types like R3.SerializableReactiveProperty{T}.
	/// </summary>
	public readonly struct WrapperType
	{
		/// <summary>
		/// Field to access value in wrapper type (like Value).
		/// </summary>
		public readonly string Field;

		/// <summary>
		/// Constructor for the wrapper type. Must include namespace.
		/// </summary>
		public readonly string ConstructorFormat;

		/// <summary>
		/// Index of actual type in the GenericTypeArguments.
		/// </summary>
		public readonly int TypeIndex;

		/// <summary>
		/// Can create instance.
		/// </summary>
		public readonly bool CanCreate;

		/// <summary>
		/// Initializes a new instance of the <see cref="WrapperType"/> struct.
		/// </summary>
		/// <param name="field">Field.</param>
		/// <param name="constructorFormat">Constructor for the wrapper type. Must include namespace.</param>
		/// <param name="typeIndex">Index of actual type in the GenericTypeArguments.</param>
		public WrapperType(string field, string constructorFormat, int typeIndex = 0)
		{
			Field = field;
			ConstructorFormat = constructorFormat;
			TypeIndex = typeIndex;
			CanCreate = !string.IsNullOrEmpty(constructorFormat);
		}

		/// <summary>
		/// Get constructor string.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <returns>Constructor string if has constructor and can create instance.</returns>
		public readonly string Constructor(Type type)
		{
			return CanCreate ? string.Format(ConstructorFormat, UtilitiesEditor.GetFriendlyTypeName(type)) : string.Empty;
		}
	}
}
#endif