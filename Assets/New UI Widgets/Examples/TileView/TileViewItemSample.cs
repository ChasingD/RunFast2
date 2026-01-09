namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// ListViewIcons item description.
	/// </summary>
	[Serializable]
	public class TileViewItemSample : IEquatable<TileViewItemSample>
	{
		/// <summary>
		/// The icon.
		/// </summary>
		[SerializeField]
		public Sprite Icon;

		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		public string Name;

		/// <summary>
		/// The capital.
		/// </summary>
		[SerializeField]
		public string Capital;

		/// <summary>
		/// The area.
		/// </summary>
		[SerializeField]
		public int Area;

		/// <summary>
		/// The population.
		/// </summary>
		[SerializeField]
		public int Population;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(TileViewItemSample other)
		{
			if (other == null)
			{
				return false;
			}

			return Icon == other.Icon && Name == other.Name && Capital == other.Capital && Area == other.Area && Population == other.Population;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is TileViewItemSample item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(TileViewItemSample a, TileViewItemSample b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(TileViewItemSample a, TileViewItemSample b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Icon, Capital, Area, Population);
	}
}