namespace UIWidgets.Examples.Inventory
{
	/// <summary>
	/// Inventory view.
	/// </summary>
	public class InventoryView : ListViewCustom<ItemView, Item>
	{
#if UITHEMES_INSTALLED
		/// <summary>
		/// Add wrappers.
		/// </summary>
		[UIThemes.PropertiesRegistry]
		[UnityEngine.Scripting.Preserve]
		public static void AddWrappers()
		{
			UIThemes.PropertyWrappers<UnityEngine.Color>.Add(new InventoryTooltipHighlightedValueHigher());
			UIThemes.PropertyWrappers<UnityEngine.Color>.Add(new InventoryTooltipHighlightedValueLower());
			UIThemes.PropertyWrappers<UnityEngine.Color>.Add(new ItemViewEmptyColor());
		}
#endif
	}
}