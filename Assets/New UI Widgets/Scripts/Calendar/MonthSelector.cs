namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Month selector.
	/// </summary>
	public class MonthSelector : DateSelectorBase
	{
		/// <summary>
		/// Text to display current year.
		/// </summary>
		[SerializeField]
		protected TextAdapter CurrentYear;

		/// <summary>
		/// Year format.
		/// </summary>
		[SerializeField]
		protected string CurrentYearFormat = "yyyy";

		/// <summary>
		/// Current months range.
		/// </summary>
		protected (int Start, int End) Range
		{
			get
			{
				if (DateMin.Year == DateMax.Year)
				{
					return (DateMin.Month, DateMax.Month);
				}

				if (DateMin.Year == Date.Year)
				{
					return (DateMin.Month, 12);
				}

				if (DateMax.Year == Date.Year)
				{
					return (1, DateMin.Month);
				}

				return (1, 12);
			}
		}

		/// <inheritdoc/>
		public override void UpdateCalendar()
		{
			UpdateRangeButtons();
		}

		/// <inheritdoc/>
		protected override void UpdateRangeButtons()
		{
			var (start, end) = Range;
			RangeButtons.Require(end - start + 1);

			var delta = start - Date.Month;
			for (var i = 0; i < RangeButtons.Count; i++)
			{
				var btn = RangeButtons[i];
				btn.SetDate(Date.AddMonths(i + delta));
				btn.transform.SetAsLastSibling();
			}

			CurrentYear.text = Date.ToString(CurrentYearFormat, Culture);

			Prev.gameObject.SetActive(Date.Year > DateMin.Year);
			Next.gameObject.SetActive(Date.Year < DateMax.Year);
		}

		/// <inheritdoc/>
		protected override void ClickNext()
		{
			if (!IsInteractable())
			{
				return;
			}

			Date = Date.AddYears(1);
		}

		/// <inheritdoc/>
		protected override void ClickPrev()
		{
			if (!IsInteractable())
			{
				return;
			}

			Date = Date.AddYears(-1);
		}
	}
}