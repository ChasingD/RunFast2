namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// ListViewVariableHeight item description.
	/// </summary>
	[Serializable]
	public class ListViewVariableHeightItemDescription : IEquatable<ListViewVariableHeightItemDescription>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public string Name;

		/// <summary>
		/// Text.
		/// </summary>
		[SerializeField]
		public string Text;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(ListViewVariableHeightItemDescription other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && Text == other.Text;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is ListViewVariableHeightItemDescription item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(ListViewVariableHeightItemDescription a, ListViewVariableHeightItemDescription b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(ListViewVariableHeightItemDescription a, ListViewVariableHeightItemDescription b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Text);
	}
}