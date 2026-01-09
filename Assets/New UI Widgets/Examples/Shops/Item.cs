namespace UIWidgets.Examples.Shops
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Item.
	/// </summary>
	[Serializable]
	public class Item : ObservableDataWithPropertyChanged, IEquatable<Item>
	{
		[SerializeField]
		[FormerlySerializedAs("Name")]
		string name;

		/// <summary>
		/// The name.
		/// </summary>
		public string Name
		{
			get => name;

			set => Change(ref name, value);
		}

		[SerializeField]
		[FormerlySerializedAs("count")]
		int quantity;

		/// <summary>
		/// Gets or sets the quantity. -1 for infinity quantity.
		/// </summary>
		/// <value>The quantity.</value>
		public int Quantity
		{
			get => quantity;

			set
			{
				if (quantity == -1)
				{
					Changed(nameof(Quantity));
					return;
				}

				Change(ref quantity, value);
			}
		}

		/// <summary>
		/// Gets or sets the quantity. -1 for infinity quantity.
		/// </summary>
		/// <value>The quantity.</value>
		[Obsolete("Renamed to Quantity.")]
		public int Count
		{
			get => Quantity;

			set => Quantity = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.Examples.Shops.Item"/> class.
		/// </summary>
		/// <param name="itemName">Name.</param>
		/// <param name="itemQuantity">Quantity.</param>
		public Item(string itemName, int itemQuantity)
		{
			name = itemName;
			quantity = itemQuantity;
		}

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

			return name == other.name && quantity == other.quantity;
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
		public override int GetHashCode() => HashCode.Combine(Name, Quantity);
	}
}