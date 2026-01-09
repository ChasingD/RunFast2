namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// ListViewEnum.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/collections/listview-enum.html")]
	public class ListViewEnum : ListViewCustom<ListViewEnumComponent, ListViewEnum.Item>
	{
		/// <summary>
		/// Item.
		/// </summary>
		public readonly struct Item : IEquatable<Item>
		{
			/// <summary>
			/// Value.
			/// </summary>
			public readonly long Value;

			/// <summary>
			/// Name.
			/// </summary>
			public readonly string Name;

			/// <summary>
			/// Is value obsolete?
			/// </summary>
			public readonly bool IsObsolete;

			/// <summary>
			/// Initializes a new instance of the <see cref="Item"/> struct.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <param name="name">Name.</param>
			/// <param name="isObsolete">Is value obsolete?</param>
			public Item(long value, string name, bool isObsolete)
			{
				Value = value;
				Name = name;
				IsObsolete = isObsolete;
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="other">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public bool Equals(Item other) => Value == other.Value && Name == other.Name && IsObsolete == other.IsObsolete;

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public override bool Equals(object obj) => (obj is Item item) && Equals(item);

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="a">First object.</param>
			/// <param name="b">Second object.</param>
			/// <returns>true if the objects are equal; otherwise, false.</returns>
			public static bool operator ==(Item a, Item b) => a.Equals(b);

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="a">First object.</param>
			/// <param name="b">Second object.</param>
			/// <returns>true if the objects not equal; otherwise, false.</returns>
			public static bool operator !=(Item a, Item b) => !(a == b);

			/// <summary>
			/// Hash function.
			/// </summary>
			/// <returns>A hash code for the current object.</returns>
			public override int GetHashCode() => HashCode.Combine(Name, Value, IsObsolete);
		}

		/// <summary>
		/// Create wrapper of the specified enum type.
		/// </summary>
		/// <typeparam name="TEnum">Enum type.</typeparam>
		/// <param name="showObsolete">Show obsolete values?</param>
		/// <param name="converter">Value converter from long to TEnum.</param>
		/// <param name="enum2long">Enum to long converter.</param>
		/// <returns>Wrapper.</returns>
		public ListViewEnum<TEnum> UseEnum<TEnum>(bool showObsolete = false, Func<long, TEnum> converter = null, Func<TEnum, long> enum2long = null)
#if CSHARP_7_3_OR_NEWER
		where TEnum : struct, Enum
#else
		where TEnum : struct
#endif
		{
			return new ListViewEnum<TEnum>(this, showObsolete, converter, enum2long);
		}

		/// <summary>
		/// Create wrapper of the specified enum type.
		/// </summary>
		/// <typeparam name="TEnum">Enum type.</typeparam>
		/// <param name="allowedValues">List of displayed enums.</param>
		/// <param name="long2enum">Long to enum converter.</param>
		/// <param name="enum2long">Enum to long converter.</param>
		/// <returns>Wrapper.</returns>
		public ListViewEnum<TEnum> UseEnum<TEnum>(IList<TEnum> allowedValues, Func<long, TEnum> long2enum = null, Func<TEnum, long> enum2long = null)
#if CSHARP_7_3_OR_NEWER
		where TEnum : struct, Enum
#else
		where TEnum : struct
#endif
		{
			return new ListViewEnum<TEnum>(this, allowedValues, long2enum, enum2long);
		}
	}
}