namespace UIWidgets.Examples
{
	using System;

	/// <summary>
	/// Item class for GroupedList.
	/// </summary>
	public class GroupedListItem : IGroupedListItem, IEquatable<GroupedListItem>
	{
		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Displayed item height.
		/// </summary>
		public float Height
		{
			get;
			set;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(IGroupedListItem other) => Equals(other as GroupedListItem);

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(GroupedListItem other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && Height == other.Height;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is GroupedListItem item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(GroupedListItem a, GroupedListItem b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(GroupedListItem a, GroupedListItem b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Height);
	}
}