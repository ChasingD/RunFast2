namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// ListViewCustom sample item description.
	/// </summary>
	[Serializable]
	public class ListViewCustomSampleItemDescription : IEquatable<ListViewCustomSampleItemDescription>
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
		/// Progress.
		/// </summary>
		[SerializeField]
		public int Progress;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(ListViewCustomSampleItemDescription other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && Icon == other.Icon && Progress == other.Progress;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is ListViewCustomSampleItemDescription item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(ListViewCustomSampleItemDescription a, ListViewCustomSampleItemDescription b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(ListViewCustomSampleItemDescription a, ListViewCustomSampleItemDescription b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Icon, Progress);
	}
}