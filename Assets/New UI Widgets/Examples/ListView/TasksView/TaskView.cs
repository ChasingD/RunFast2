namespace UIWidgets.Examples.Tasks
{
	using UIWidgets;

	/// <summary>
	/// TaskView.
	/// </summary>
	public class TaskView : ListViewCustom<TaskComponent, Task>
	{
		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();

			DataSource.Comparison = (x, y) => UtilitiesCompare.Compare(x.Name, y.Name);
		}
	}
}