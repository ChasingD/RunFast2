namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;

	/// <summary>
	/// ListViewIconsItemDescription extended
	/// </summary>
	[Serializable]
	public class ListViewIconsItemDescriptionExt : ListViewIconsItemDescription
	{
		bool visible = true;

		/// <summary>
		/// Is visible?
		/// </summary>
		public bool Visible
		{
			get => visible;

			set => Change(ref visible, value);
		}

		bool interactable = true;

		/// <summary>
		/// Is interactable?
		/// </summary>
		public bool Interactable
		{
			get => interactable;

			set => Change(ref interactable, value);
		}
	}
}