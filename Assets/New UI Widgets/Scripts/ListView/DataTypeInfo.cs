namespace UIWidgets
{
	using System;
	using UIWidgets.Attributes;

	/// <summary>
	/// Information on data type.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	public class DataTypeInfo<T>
	{
		/// <summary>
		/// Type is value type or implements IEquatable&lt;T&gt;.
		/// </summary>
		public readonly bool IsEquatable;

		/// <summary>
		/// Instance.
		/// </summary>
		[DomainReloadExclude]
		public static readonly DataTypeInfo<T> Instance = new DataTypeInfo<T>();

		DataTypeInfo()
		{
			var type = typeof(T);
			IsEquatable = type.IsValueType || typeof(IEquatable<T>).IsAssignableFrom(type);
		}
	}
}