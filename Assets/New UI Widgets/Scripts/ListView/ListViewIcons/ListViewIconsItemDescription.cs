namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// ListViewIcons item description.
	/// </summary>
	[Serializable]
	public class ListViewIconsItemDescription : ObservableDataWithPropertyChanged, IEquatable<ListViewIconsItemDescription>
	{
		[SerializeField]
		[FormerlySerializedAs("Icon")]
		Sprite icon;

		/// <summary>
		/// The icon.
		/// </summary>
		public Sprite Icon
		{
			get => icon;

			set => Change(ref icon, value);
		}

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

		[NonSerialized]
		string localizedName;

		/// <summary>
		/// The localized name.
		/// </summary>
		public string LocalizedName
		{
			get => localizedName;

			set => Change(ref localizedName, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Value")]
		int val;

		/// <summary>
		/// The value.
		/// </summary>
		public int Value
		{
			get => val;

			set => Change(ref val, value);
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString() => LocalizedName ?? Name;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(ListViewIconsItemDescription other)
		{
			if (other == null)
			{
				return false;
			}

			return name == other.name && localizedName == other.localizedName && icon == other.icon && val == other.val;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is ListViewIconsItemDescription item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(ListViewIconsItemDescription a, ListViewIconsItemDescription b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(ListViewIconsItemDescription a, ListViewIconsItemDescription b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(name, localizedName, icon, val);
	}
}