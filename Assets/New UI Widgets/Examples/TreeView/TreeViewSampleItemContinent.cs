namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TreeViewSample continent item.
	/// </summary>
	[Serializable]
	public class TreeViewSampleItemContinent : ObservableDataWithPropertyChanged, ITreeViewSampleItem
	{
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

		[SerializeField]
		int countries;

		/// <summary>
		/// Countries.
		/// </summary>
		public int Countries
		{
			get => countries;

			set => Change(ref countries, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeViewSampleItemContinent"/> class.
		/// </summary>
		/// <param name="itemName">Name.</param>
		/// <param name="itemCountries">Countries.</param>
		public TreeViewSampleItemContinent(string itemName, int itemCountries = 0)
		{
			name = itemName;
			countries = itemCountries;
		}

		/// <summary>
		/// Display item data using specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		public void Display(TreeViewSampleComponent component)
		{
			component.Icon.sprite = null;
			component.Icon.enabled = false;
			component.TextAdapter.text = string.Format("{0} (Countries: {1})", Name, Countries.ToString());
			component.name = Name;
		}
	}
}