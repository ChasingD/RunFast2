namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Date selector.
	/// </summary>
	public abstract class DateSelectorBase : DateBase
	{
		/// <summary>
		/// Container.
		/// </summary>
		[SerializeField]
		protected RectTransform Container;

		/// <summary>
		/// Template.
		/// </summary>
		[SerializeField]
		protected DateSelectorButton Template;

		/// <summary>
		/// Button to show previous range.
		/// </summary>
		[SerializeField]
		protected Button Prev;

		/// <summary>
		/// Button to show next range.
		/// </summary>
		[SerializeField]
		protected Button Next;

		/// <summary>
		/// Cache.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<DateSelectorButton> buttonsCache = new List<DateSelectorButton>();

		/// <summary>
		/// Instances.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<DateSelectorButton> buttonsInstances = new List<DateSelectorButton>();

		[NonSerialized]
		ListComponentPool<DateSelectorButton> rangeButtons;

		/// <summary>
		/// Buttons pool.
		/// </summary>
		protected ListComponentPool<DateSelectorButton> RangeButtons
		{
			get
			{
				if ((rangeButtons == null) || (rangeButtons.Template == null))
				{
					rangeButtons = new ListComponentPool<DateSelectorButton>(Template, buttonsInstances, buttonsCache, Container);
				}

				return rangeButtons;
			}
		}

		/// <summary>
		/// Event called when date selected.
		/// </summary>
		[SerializeField]
		public CalendarDateEvent OnSelectDate = new CalendarDateEvent();

		/// <summary>
		/// Process click on button to show previous range.
		/// </summary>
		protected abstract void ClickPrev();

		/// <summary>
		/// Process click on button to show next range.
		/// </summary>
		protected abstract void ClickNext();

		/// <summary>
		/// Update buttons.
		/// </summary>
		protected abstract void UpdateRangeButtons();

		/// <summary>
		/// Selected date.
		/// </summary>
		/// <param name="date">Date.</param>
		public virtual void SelectDate(DateTime date)
		{
			OnSelectDate.Invoke(date);
		}

		/// <inheritdoc/>
		protected override void InitNestedWidgets()
		{
			Template.gameObject.SetActive(false);

			foreach (var btn in RangeButtons.GetEnumerator(PoolEnumeratorMode.All))
			{
				OnButtonCreate(btn);
			}

			RangeButtons.OnCreate = OnButtonCreate;
			RangeButtons.OnDestroy = OnButtonDestroy;

			if (Prev != null)
			{
				Prev.onClick.AddListener(ClickPrev);
			}

			if (Next != null)
			{
				Next.onClick.AddListener(ClickNext);
			}
		}

		/// <summary>
		/// Process created button.
		/// </summary>
		/// <param name="button">Button.</param>
		protected virtual void OnButtonCreate(DateSelectorButton button)
		{
			button.Init();
			button.SetOwner(this);
		}

		/// <summary>
		/// Process destroyed button.
		/// </summary>
		/// <param name="button">Button.</param>
		protected virtual void OnButtonDestroy(DateSelectorButton button)
		{
			button.SetOwner(null);
		}

		/// <inheritdoc/>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			RangeButtons.Clear();
			RangeButtons.OnCreate = null;
			RangeButtons.OnDestroy = null;
			rangeButtons = null;

			if (Prev != null)
			{
				Prev.onClick.RemoveListener(ClickPrev);
			}

			if (Next != null)
			{
				Next.onClick.RemoveListener(ClickNext);
			}
		}

		/// <inheritdoc/>
		public override bool GetStyle(Style style) => true;

		/// <inheritdoc/>
		public override bool SetStyle(Style style) => true;
	}
}