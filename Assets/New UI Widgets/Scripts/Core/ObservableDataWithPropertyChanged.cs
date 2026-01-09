namespace UIWidgets
{
	using System.ComponentModel;

	/// <summary>
	/// ObservableData with INotifyPropertyChanged interface.
	/// </summary>
	public class ObservableDataWithPropertyChanged : ObservableData, INotifyPropertyChanged
    {
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc/>
		protected sealed override void Changed(string propertyName)
		{
			if (PauseObservation)
			{
				return;
			}

			base.Changed(propertyName);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}