namespace UIWidgets.Examples.TableListDemo
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// TableList component.
	/// </summary>
	public class TableListComboboxComponent : ListViewItem, IViewData<TableListComboboxItem>
	{
		/// <summary>
		/// Name.
		/// </summary>
		public TextAdapter Name;

		/// <summary>
		/// The text adapters for values.
		/// </summary>
		[SerializeField]
		public List<TableListComboboxCell> ValueCells = new List<TableListComboboxCell>();

		/// <summary>
		/// The item.
		/// </summary>
		[SerializeField]
		protected TableListComboboxItem Item;

		protected override void InitOnce()
		{
			base.InitOnce();

			foreach (var cell in ValueCells)
			{
				cell.Owner = this;
			}
		}

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public void SetData(TableListComboboxItem item)
		{
			Item = item;
			UpdateView();
		}

		/// <summary>
		/// Update view.
		/// </summary>
		public void UpdateView()
		{
			Name.text = Item.Name;

			for (int index = 0; index < ValueCells.Count; index++)
			{
				ValueCells[index].Text.text = index < Item.Values.Count ? Item.Values[index] : "none";
			}
		}

		/// <summary>
		/// Add value cell.
		/// </summary>
		/// <param name="template">Template.</param>
		public void AddValueCell(TableListComboboxCell template)
		{
			var cell = Compatibility.Instantiate(template);
			cell.transform.SetParent(RectTransform, false);
			cell.gameObject.SetActive(true);
			cell.Owner = this;

			ValueCells.Add(cell);
			foregrounds.Add(cell.Text.Graphic);

			UpdateView();
		}

		/// <summary>
		/// Remove value cell.
		/// </summary>
		/// <param name="valueCellIndex">Value cell index.</param>
		public void RemoveValueCell(int valueCellIndex)
		{
			var go = RectTransform.GetChild(valueCellIndex + 1).gameObject;
			Destroy(go);

			ValueCells.RemoveAt(valueCellIndex);
			foregrounds.RemoveAt(valueCellIndex + 1);

			UpdateView();
		}

		public void ShowListView(TableListComboboxCell cell) => (Owner as TableListCombobox).ShowListView(Index, ValueCells.IndexOf(cell), cell.ToggleListView);
	}
}