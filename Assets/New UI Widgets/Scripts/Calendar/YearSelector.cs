namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Year selector.
	/// </summary>
	public class YearSelector : DateSelectorBase
	{
		/// <summary>
		/// Current year.
		/// </summary>
		protected int CurrentYear;

		/// <summary>
		/// Years per page.
		/// </summary>
		[SerializeField]
		protected int YearsPerPage = 20;

		/// <summary>
		/// Text to display years range.
		/// </summary>
		[SerializeField]
		protected TextAdapter YearsRange;

		/// <summary>
		/// Years format.
		/// </summary>
		[SerializeField]
		protected string YearFormat = "yyyy";

		/// <summary>
		/// Years range.
		/// </summary>
		protected int Range => Mathf.Min(YearsPerPage, DateMax.Year - DateMin.Year);

		/// <summary>
		/// Set current year.
		/// </summary>
		/// <param name="year">Year.</param>
		/// <returns>true if year was changed; otherwise false.</returns>
		protected bool SetCurrentYear(int year)
		{
			var old = CurrentYear;

			CurrentYear = year;
			if (CurrentYear > (DateMax.Year - YearsPerPage))
			{
				CurrentYear = DateMax.Year - YearsPerPage;

				if (CurrentYear < DateMin.Year)
				{
					CurrentYear = DateMin.Year;
				}
			}
			else if (CurrentYear < DateMin.Year)
			{
				CurrentYear = DateMin.Year;
			}

			var changed = old != CurrentYear;
			if (changed)
			{
				UpdateRangeButtons();
			}

			return changed;
		}

		/// <inheritdoc/>
		protected override void ClickPrev()
		{
			if (!IsInteractable())
			{
				return;
			}

			SetCurrentYear(CurrentYear - YearsPerPage);
		}

		/// <inheritdoc/>
		protected override void ClickNext()
		{
			if (!IsInteractable())
			{
				return;
			}

			SetCurrentYear(CurrentYear + YearsPerPage);
		}

		/// <inheritdoc/>
		public override void UpdateCalendar() => SetCurrentYear(Date.Year);

		/// <inheritdoc/>
		protected override void UpdateRangeButtons()
		{
			var delta = CurrentYear - Date.Year;

			var range = Range;
			RangeButtons.Require(range);
			for (var i = 0; i < RangeButtons.Count; i++)
			{
				var btn = RangeButtons[i];
				btn.SetDate(Date.AddYears(i + delta));
				btn.transform.SetAsLastSibling();
			}

			var start = Date.AddYears(delta);
			var end = Date.AddYears(delta + range);
			YearsRange.text = string.Format("{0} - {1}", start.ToString(YearFormat, Culture), end.ToString(YearFormat, Culture));

			Prev.gameObject.SetActive(CurrentYear > DateMin.Year);
			Next.gameObject.SetActive(CurrentYear < (DateMax.Year - YearsPerPage));
		}
	}
}