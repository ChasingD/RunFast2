namespace UIWidgets.Examples.Inventory
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Item.
	/// </summary>
	[Serializable]
	public class Item : IEquatable<Item>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public string Name;

		/// <summary>
		/// Color.
		/// </summary>
		[SerializeField]
		public Color Color;

		/// <summary>
		/// Weight.
		/// </summary>
		[SerializeField]
		public float Weight;

		/// <summary>
		/// Price.
		/// </summary>
		[SerializeField]
		public int Price;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(Item other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && Color == other.Color && Weight == other.Weight && Price == other.Price;
		}

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
		public static bool operator ==(Item a, Item b) => (a is null) ? b is null : a.Equals(b);

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
		public override int GetHashCode() => HashCode.Combine(Name, Color, Weight, Price);
	}
}