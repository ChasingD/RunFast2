namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Tree view item.
	/// </summary>
	[Serializable]
	public class TreeViewItem : ObservableDataWithPropertyChanged
	{
		/// <summary>
		/// The icon.
		/// </summary>
		[SerializeField]
		Sprite icon;

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public Sprite Icon
		{
			get => icon;

			set => Change(ref icon, value);
		}

		/// <summary>
		/// The name.
		/// </summary>
		[SerializeField]
		string name;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
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
		[FormerlySerializedAs("_value")]
		int itemValue;

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public int Value
		{
			get => itemValue;

			set => Change(ref itemValue, value);
		}

		[SerializeField]
		object tag;

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public object Tag
		{
			get => tag;

			set => Change(ref tag, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UIWidgets.TreeViewItem"/> class.
		/// </summary>
		/// <param name="itemName">Item name.</param>
		/// <param name="itemIcon">Item icon.</param>
		public TreeViewItem(string itemName, Sprite itemIcon = null)
		{
			name = itemName;
			icon = itemIcon;
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString() => LocalizedName ?? Name;
	}
}