namespace UIWidgets.Examples.Shops
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Harbor order line.
	/// </summary>
	[Serializable]
	public class HarborOrderLine : IOrderLine, IEquatable<HarborOrderLine>
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
		/// The buy price.
		/// </summary>
		[SerializeField]
		public int BuyPrice;

		/// <summary>
		/// The sell price.
		/// </summary>
		[SerializeField]
		public int SellPrice;

		/// <summary>
		/// The buy quantity.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("BuyCount")]
		public int BuyQuantity;

		/// <summary>
		/// The buy quantity.
		/// </summary>
		[Obsolete("Renamed to BuyQuantity.")]
		public int BuyCount
		{
			get => BuyQuantity;

			set => BuyQuantity = value;
		}

		/// <summary>
		/// The sell quantity.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("SellCount")]
		public int SellQuantity;

		/// <summary>
		/// The buy quantity.
		/// </summary>
		[Obsolete("Renamed to SellQuantity.")]
		public int SellCount
		{
			get => SellQuantity;

			set => SellQuantity = value;
		}

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
		/// Initializes a new instance of the <see cref="UIWidgets.Examples.Shops.HarborOrderLine"/> class.
		/// </summary>
		/// <param name="newItem">New item.</param>
		/// <param name="buyPrice">Buy price.</param>
		/// <param name="sellPrice">Sell price.</param>
		/// <param name="buyQuantity">Buy quantity.</param>
		/// <param name="sellQuantity">Sell quantity.</param>
		public HarborOrderLine(Item newItem, int buyPrice, int sellPrice, int buyQuantity, int sellQuantity)
		{
			item = newItem;
			BuyPrice = buyPrice;
			SellPrice = sellPrice;
			BuyQuantity = buyQuantity;
			SellQuantity = sellQuantity;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(HarborOrderLine other)
		{
			if (other == null)
			{
				return false;
			}

			return item.Equals(other.item)
				&& BuyPrice == other.BuyPrice
				&& SellPrice == other.SellPrice
				&& BuyQuantity == other.BuyQuantity
				&& SellQuantity == other.SellQuantity
				&& Quantity == other.Quantity;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is HarborOrderLine item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(HarborOrderLine a, HarborOrderLine b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(HarborOrderLine a, HarborOrderLine b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(item, BuyPrice, SellPrice, BuyQuantity, SellQuantity, Quantity);
	}
}