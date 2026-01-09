namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// ListViewUnderline sample item description.
	/// </summary>
	[Serializable]
	public class ListViewUnderlineSampleItemDescription : IEquatable<ListViewUnderlineSampleItemDescription>
	{
		/// <summary>
		/// Icon.
		/// </summary>
		[SerializeField]
		public Sprite Icon;

		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public string Name;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(ListViewUnderlineSampleItemDescription other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && Icon == other.Icon;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is ListViewUnderlineSampleItemDescription item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(ListViewUnderlineSampleItemDescription a, ListViewUnderlineSampleItemDescription b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(ListViewUnderlineSampleItemDescription a, ListViewUnderlineSampleItemDescription b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Icon);
	}
}