namespace UIWidgets.Examples.TableListDemo
{
	using System.Collections.Generic;
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test TableListCombobox.
	/// </summary>
	public class TableListComboboxTest : MonoBehaviour
	{
		/// <summary>
		/// Table.
		/// </summary>
		[SerializeField]
		public TableListCombobox Table;

		/// <summary>
		/// Header template.
		/// </summary>
		[SerializeField]
		public TableListComboboxCell HeaderTemplate;

		/// <summary>
		/// Value template.
		/// </summary>
		[SerializeField]
		public TableListComboboxCell ValueTemplate;

		int columns;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected void Start()
		{
			Table.DataSource = Generate(100, 10);
			columns = Table.DefaultItem.ValueCells.Count;
		}

		/// <summary>
		/// Add column.
		/// </summary>
		/// <param name="columnName">Column name.</param>
		public void AddColumn(string columnName)
		{
			foreach (var row in Table.GetComponentsEnumerator(PoolEnumeratorMode.All))
			{
				row.AddValueCell(ValueTemplate);
			}

			// add header
			var cell = Compatibility.Instantiate(HeaderTemplate);
			columns += 1;
			cell.Text.text = string.Format("{0} {1}", columnName, columns.ToString());
			Table.Header.AddCell(cell.gameObject);

			Table.ComponentsColoring();
		}

		/// <summary>
		/// Remove column.
		/// </summary>
		/// <param name="columnIndex">Column index.</param>
		public void RemoveColumn(int columnIndex)
		{
			if (columnIndex >= Table.DefaultItem.ValueCells.Count)
			{
				return;
			}

			foreach (var row in Table.GetComponentsEnumerator(PoolEnumeratorMode.All))
			{
				row.RemoveValueCell(columnIndex);
			}

			var cell = Table.Header.transform.GetChild(columnIndex + 1);
			Table.Header.RemoveCell(cell.gameObject);
		}

		ObservableList<TableListComboboxItem> Generate(int rows, int columns)
		{
			var data = new ObservableList<TableListComboboxItem>(rows);
			for (int row = 0; row < rows; row++)
			{
				var item = new TableListComboboxItem()
				{
					Name = "Item " + row,
					Values = CreateValue(row, columns),
				};
				data.Add(item);
			}

			return data;
		}

		List<string> CreateValue(int row, int columns)
		{
			var values = new List<string>(columns);
			for (int j = 0; j < columns; j++)
			{
				var v = (columns * row) + j;
				values.Add(v.ToString());
			}

			return values;
		}
	}
}