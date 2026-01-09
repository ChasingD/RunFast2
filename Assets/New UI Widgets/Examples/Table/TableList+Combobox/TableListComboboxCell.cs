namespace UIWidgets.Examples.TableListDemo
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// TableList cell.
	/// </summary>
	public class TableListComboboxCell : MonoBehaviourInitiable
	{
		/// <summary>
		/// The text.
		/// </summary>
		[SerializeField]
		public TextAdapter Text;

		[SerializeField]
		public Button ToggleListView;

		[NonSerialized]
		public TableListComboboxComponent Owner;

		protected override void InitOnce()
		{
			base.InitOnce();

			if (ToggleListView != null)
			{
				ToggleListView.onClick.AddListener(ShowListView);
			}
		}

		private void OnDestroy()
		{
			if (ToggleListView != null)
			{
				ToggleListView.onClick.RemoveListener(ShowListView);
			}
		}

		void ShowListView() => Owner.ShowListView(this);
	}
}