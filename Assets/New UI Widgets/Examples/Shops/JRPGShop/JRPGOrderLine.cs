namespace UIWidgets.Examples.Shops
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// JRPG order line.
	/// </summary>
	[Serializable]
	public class JRPGOrderLine : IOrderLine, IEquatable<JRPGOrderLine>
	{
		[SerializeField]
		Item item;

		/// <summary>
		/// Gets or sets the item.
		/// </summary>
		/// <value>The item.</value>
		public Item Item
		{
			get => item;

			set => item = value;
		}

		/// <summary>
		/// The price.
		/// </summary>
		[SerializeField]
		public int Price;

		[SerializeField]
		[FormerlySerializedAs("count")]
		int quantity;

		/// <summary>
		/// Gets or sets the quantity.
		/// </summary>
		/// <value>The quantity.</value>
		public int Quantity
		{
			get => quantity;

			set => quantity = value;
		}

		/// <summary>
		/// Gets or sets the quantity.
		/// </summary>
		/// <value>The quantity.</value>
		[Obsolete("Renamed to Quantity.")]
		public int Count
		{
			get => quantity;

			set => quantity = value;
		}

		/// <summary>
		/// Is it playlist? Otherwise it's song.
		/// </summary>
		public bool IsPlaylist;

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.Examples.Shops.JRPGOrderLine"/> class.
		/// </summary>
		/// <param name="newItem">New item.</param>
		/// <param name="newPrice">New price.</param>
		public JRPGOrderLine(Item newItem, int newPrice)
		{
			item = newItem;
			Price = newPrice;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(JRPGOrderLine other)
		{
			if (other == null)
			{
				return false;
			}

			return item.Equals(other) && Price == other.Price && quantity == other.quantity && IsPlaylist == other.IsPlaylist;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is JRPGOrderLine item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(JRPGOrderLine a, JRPGOrderLine b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(JRPGOrderLine a, JRPGOrderLine b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(item, Price, quantity, IsPlaylist);
	}
}