namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Calendar with Spinner for year.
	/// </summary>
	public class CalendarYearSpinner : CalendarBase
	{
		[SerializeField]
		Spinner yearSpinner;

		/// <summary>
		/// Year spinner.
		/// </summary>
		public Spinner YearSpinner
		{
			get => yearSpinner;

			set
			{
				if (yearSpinner == value)
				{
					return;
				}

				DisableSpinner();

				yearSpinner = value;

				EnableSpinner();

				UpdateDate();
			}
		}

		/// <inheritdoc/>
		protected override void InitNestedWidgets()
		{
			base.InitNestedWidgets();

			EnableSpinner();
		}

		/// <summary>
		/// Enable spinner.
		/// </summary>
		protected virtual void EnableSpinner()
		{
			if (yearSpinner != null)
			{
				yearSpinner.onValueChangeInt.AddListener(YearSpinnerChanged);
				yearSpinner.Min = DateMin.Year;
				yearSpinner.Max = DateMax.Year;
			}
		}

		/// <summary>
		/// Disable spinner.
		/// </summary>
		protected virtual void DisableSpinner()
		{
			if (yearSpinner != null)
			{
				yearSpinner.onValueChangeInt.RemoveListener(YearSpinnerChanged);
			}
		}

		/// <summary>
		/// Process Yeas Spinner changed.
		/// </summary>
		/// <param name="year">Year.</param>
		protected virtual void YearSpinnerChanged(int year)
		{
			if (year != Date.Year)
			{
				DateDisplay = DateDisplay.AddYears(year - DateDisplay.Year);
			}
		}

		/// <inheritdoc/>
		protected override void UpdateDate()
		{
			if (YearSpinner != null)
			{
				YearSpinner.Min = DateMin.Year;
				YearSpinner.Max = DateMax.Year;
				YearSpinner.Value = DateDisplay.Year;
			}

			base.UpdateDate();
		}
	}
}