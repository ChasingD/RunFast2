namespace UIWidgets
{
	using System;
	using UIWidgets.l10n;
	using UnityEngine;

	/// <summary>
	/// ListViewIcons.
	/// </summary>
	public class ListViewIcons : ListViewCustom<ListViewIconsItemComponent, ListViewIconsItemDescription>, ILocalizationSupport
	{
		[SerializeField]
		[Tooltip("If enabled translates items names using Localization.GetTranslation().")]
		bool localizationSupport = true;

		/// <summary>
		/// Localization support.
		/// </summary>
		public bool LocalizationSupport
		{
			get => localizationSupport;

			set => localizationSupport = value;
		}

		/// <summary>
		/// Sort items.
		/// Deprecated. Replaced with DataSource.Comparison.
		/// </summary>
		[Obsolete("Replaced with DataSource.Comparison.")]
		public override bool Sort
		{
			get => DataSource.Comparison == ItemsComparison;

			set => DataSource.Comparison = value ? (LocalizationSupport ? LocalizedItemsComparison : ItemsComparison) : null;
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected override void InitOnce()
		{
			base.InitOnce();

#pragma warning disable 0618
			if (base.Sort)
			{
				DataSource.Comparison = LocalizationSupport
					? StaticFields.Instance.ListViewLocalizedItemsComparison
					: StaticFields.Instance.ListViewItemsComparison;
			}
#pragma warning restore 0618
		}

		/// <summary>
		/// Process locale changes.
		/// </summary>
		public override void LocaleChanged()
		{
			base.LocaleChanged();

			if (DataSource.Comparison != null)
			{
				DataSource.CollectionChanged();
			}
		}

		/// <summary>
		/// Items comparison by localized names.
		/// </summary>
		public static Comparison<ListViewIconsItemDescription> LocalizedItemsComparison => StaticFields.Instance.ListViewLocalizedItemsComparison;

		/// <summary>
		/// Items comparison.
		/// </summary>
		public static Comparison<ListViewIconsItemDescription> ItemsComparison => StaticFields.Instance.ListViewItemsComparison;
	}
}