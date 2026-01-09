namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TreeViewSampleItemCountry.
	/// Sample class to display country data.
	/// </summary>
	[Serializable]
	public class TreeViewSampleItemCountry : ObservableDataWithPropertyChanged, ITreeViewSampleItem
	{
		[SerializeField]
		Sprite icon;

		/// <summary>
		/// Icon.
		/// </summary>
		public Sprite Icon
		{
			get => icon;

			set => Change(ref icon, value);
		}

		[SerializeField]
		string name;

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get => name;

			set => Change(ref name, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeViewSampleItemCountry"/> class.
		/// </summary>
		/// <param name="itemName">Name.</param>
		/// <param name="itemIcon">Icon.</param>
		public TreeViewSampleItemCountry(string itemName, Sprite itemIcon = null)
		{
			name = itemName;
			icon = itemIcon;
		}

		/// <summary>
		/// Display data with specified component.
		/// </summary>
		/// <param name="component">Component to display item.</param>
		public void Display(TreeViewSampleComponent component)
		{
			component.Icon.sprite = Icon;
			component.TextAdapter.text = Name;
			component.name = Name;

			if (component.SetNativeSize)
			{
				component.Icon.SetNativeSize();
			}

			component.Icon.enabled = component.Icon.sprite != null;
		}
	}
}