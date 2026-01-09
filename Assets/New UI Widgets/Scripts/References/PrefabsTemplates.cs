namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Prefabs templates.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/generator.html")]
	public class PrefabsTemplates : ScriptableObject
	{
#if UNITY_EDITOR
		/// <summary>
		/// Instance.
		/// </summary>
		public static PrefabsTemplates Instance => StaticFields.Instance.References.PrefabsTemplates;
#endif

		/// <summary>
		/// Autocomplete.
		/// </summary>
		[SerializeField]
		public GameObject Autocomplete;

		/// <summary>
		/// Combobox.
		/// </summary>
		[SerializeField]
		public GameObject Combobox;

		/// <summary>
		/// Combobox with enabled multiselect.
		/// </summary>
		[SerializeField]
		public GameObject ComboboxMultiselect;

		/// <summary>
		/// DragInfo.
		/// </summary>
		[SerializeField]
		public GameObject DragInfo;

		/// <summary>
		/// ListView.
		/// </summary>
		[SerializeField]
		public GameObject ListView;

		/// <summary>
		/// PickerListView.
		/// </summary>
		[SerializeField]
		public GameObject PickerListView;

		/// <summary>
		/// PickerTreeView.
		/// </summary>
		[SerializeField]
		public GameObject PickerTreeView;

		/// <summary>
		/// Scene.
		/// </summary>
		[SerializeField]
		public GameObject Scene;

		/// <summary>
		/// Table.
		/// </summary>
		[SerializeField]
		public GameObject Table;

		/// <summary>
		/// TileView.
		/// </summary>
		[SerializeField]
		public GameObject TileView;

		/// <summary>
		/// TreeGraph.
		/// </summary>
		[SerializeField]
		public GameObject TreeGraph;

		/// <summary>
		/// TreeView.
		/// </summary>
		[SerializeField]
		public GameObject TreeView;

		/// <summary>
		/// Tooltip.
		/// </summary>
		[SerializeField]
		public GameObject Tooltip;

#if UITHEMES_INSTALLED
		/// <summary>
		/// Default style.
		/// </summary>
		[SerializeField]
		public UIWidgets.Styles.Style StyleDefault;

		/// <summary>
		/// Blue style.
		/// </summary>
		[SerializeField]
		public UIWidgets.Styles.Style StyleBlue;

		/// <summary>
		/// Theme.
		/// </summary>
		[SerializeField]
		public UIThemes.Theme Theme;
#endif
	}
}