namespace UIWidgets.Examples
{
	using UIWidgets;

	/// <summary>
	/// ListViewCustom sample.
	/// </summary>
	public class ListViewCustomSample : ListViewCustom<ListViewCustomSampleComponent, ListViewCustomSampleItemDescription>
	{
		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();
			DataSource.Comparison = (x, y) => UtilitiesCompare.Compare(x.Name, y.Name);
		}
	}
}