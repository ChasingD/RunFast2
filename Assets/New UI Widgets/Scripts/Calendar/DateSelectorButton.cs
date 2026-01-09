namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Date selector button.
	/// </summary>
	public class DateSelectorButton : MonoBehaviourInitiable
	{
		/// <summary>
		/// Button.
		/// </summary>
		[SerializeField]
		protected Button Button;

		/// <summary>
		/// Button text.
		/// </summary>
		[SerializeField]
		protected TextAdapter Text;

		/// <summary>
		/// Date format.
		/// </summary>
		[SerializeField]
		protected string Format = "yyyy";

		/// <summary>
		/// Convert button text to upper case.
		/// </summary>
		[SerializeField]
		protected bool UpperCase = true;

		/// <summary>
		/// Current date.
		/// </summary>
		protected DateTime Date;

		/// <summary>
		/// Owner.
		/// </summary>
		protected DateSelectorBase Owner;

		/// <summary>
		/// Set owner.
		/// </summary>
		/// <param name="owner">Owner.</param>
		public virtual void SetOwner(DateSelectorBase owner) => Owner = owner;

		/// <summary>
		/// Set date.
		/// </summary>
		/// <param name="date">Date.</param>
		public virtual void SetDate(DateTime date)
		{
			Date = date;

			var t = Date.ToString(Format, Owner.Culture);
			if (UpperCase)
			{
				t = t.ToUpper();
			}

			Text.text = t;
		}

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();

			Button.onClick.AddListener(SelectDate);
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Button != null)
			{
				Button.onClick.RemoveListener(SelectDate);
			}
		}

		/// <summary>
		/// Select date.
		/// </summary>
		protected void SelectDate() => Owner.SelectDate(Date);
	}
}