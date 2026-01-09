namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Item class for GroupedPhotos.
	/// </summary>
	public class Photo : IEquatable<Photo>
	{
		/// <summary>
		/// Date.
		/// </summary>
		public DateTime Created;

		/// <summary>
		/// Image.
		/// </summary>
		public Sprite Image;

		/// <summary>
		/// Is group?
		/// </summary>
		public bool IsGroup;

		/// <summary>
		/// Is empty?
		/// </summary>
		public bool IsEmpty;

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0101:Array allocation for params parameter", Justification = "Required.")]
		public override string ToString()
		{
			return string.Format("{0} at {1}; IsGroup: {2}; IsEmpty: {3}", Image, Created.ToString("yyyy-MM-dd"), IsGroup.ToString(), IsEmpty.ToString());
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(Photo other)
		{
			if (other == null)
			{
				return false;
			}

			return Created == other.Created && Image == other.Image && IsGroup == other.IsGroup && IsEmpty == other.IsEmpty;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is Photo item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(Photo a, Photo b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(Photo a, Photo b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Image, Created, IsGroup, IsEmpty);
	}
}