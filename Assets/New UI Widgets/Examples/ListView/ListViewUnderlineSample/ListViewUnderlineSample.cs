namespace UIWidgets.Examples
{
	using UIWidgets;

	/// <summary>
	/// ListViewUnderline sample.
	/// </summary>
	public class ListViewUnderlineSample : ListViewCustom<ListViewUnderlineSampleComponent, ListViewUnderlineSampleItemDescription>
	{
		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();
			DataSource.Comparison = (x, y) => UtilitiesCompare.Compare(x.Name, y.Name);
		}
	}
}