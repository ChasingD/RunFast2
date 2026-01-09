namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Calendar with years list.
	/// </summary>
	public class CalendarYearList : CalendarBase
	{
		/// <summary>
		/// Button to close all selectors.
		/// </summary>
		[SerializeField]
		protected Button DateButton;

		/// <summary>
		/// Button to show month selector.
		/// </summary>
		[SerializeField]
		protected Button MonthButton;

		/// <summary>
		/// Month selector.
		/// </summary>
		[SerializeField]
		protected MonthSelector MonthSelector;

		/// <summary>
		/// Button to show year selector.
		/// </summary>
		[SerializeField]
		protected Button YearButton;

		/// <summary>
		/// Year selector.
		/// </summary>
		[SerializeField]
		protected YearSelector YearSelector;

		/// <inheritdoc/>
		protected override void InitNestedWidgets()
		{
			base.InitNestedWidgets();

			MonthSelector.gameObject.SetActive(false);
			YearSelector.gameObject.SetActive(false);

			DateButton.onClick.AddListener(CloseSelectors);

			MonthButton.onClick.AddListener(OpenMonthSelector);
			YearButton.onClick.AddListener(OpenYearSelector);
			MonthSelector.OnSelectDate.AddListener(MonthSelected);
			YearSelector.OnSelectDate.AddListener(YearSelected);
		}

		/// <inheritdoc/>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (DateButton != null)
			{
				DateButton.onClick.RemoveListener(CloseSelectors);
			}

			if (MonthButton != null)
			{
				MonthButton.onClick.RemoveListener(OpenMonthSelector);
			}

			if (YearButton != null)
			{
				YearButton.onClick.RemoveListener(OpenYearSelector);
			}

			if (MonthSelector != null)
			{
				MonthSelector.OnSelectDate.RemoveListener(MonthSelected);
			}

			if (YearSelector != null)
			{
				YearSelector.OnSelectDate.RemoveListener(YearSelected);
			}
		}

		/// <summary>
		/// Close selectors.
		/// </summary>
		protected virtual void CloseSelectors()
		{
			MonthSelector.gameObject.SetActive(false);
			YearSelector.gameObject.SetActive(false);
		}

		/// <summary>
		/// Process the month selected event.
		/// </summary>
		/// <param name="date">Date.</param>
		protected virtual void MonthSelected(DateTime date)
		{
			if (MonthSelector.gameObject.activeSelf)
			{
				DateDisplay = DateDisplay.AddMonths(date.Month - DateDisplay.Month);
				MonthSelector.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Process the year selected event.
		/// </summary>
		/// <param name="date">Date.</param>
		protected virtual void YearSelected(DateTime date)
		{
			if (YearSelector.gameObject.activeSelf)
			{
				DateDisplay = DateDisplay.AddYears(date.Year - DateDisplay.Year);
				YearSelector.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Open month selector.
		/// </summary>
		protected virtual void OpenMonthSelector()
		{
			MonthSelector.gameObject.SetActive(true);
		}

		/// <summary>
		/// Open year selector.
		/// </summary>
		protected virtual void OpenYearSelector()
		{
			YearSelector.gameObject.SetActive(true);
		}

		/// <inheritdoc/>
		public override void UpdateCalendar()
		{
			base.UpdateCalendar();

			MonthSelector.Culture = Culture;
			MonthSelector.Date = DateDisplay;
			MonthSelector.DateMin = DateMin;
			MonthSelector.DateMax = DateMax;

			YearSelector.Culture = Culture;
			YearSelector.Date = DateDisplay;
			YearSelector.DateMin = DateMin;
			YearSelector.DateMax = DateMax;
		}
	}
}