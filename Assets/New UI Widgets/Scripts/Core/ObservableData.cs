namespace UIWidgets
{
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Base class for the IObservable types.
	/// </summary>
	public class ObservableData : IObservable
	{
		/// <summary>
		/// The pause observation.
		/// </summary>
		public bool PauseObservation
		{
			get;
			set;
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event OnChange OnChange;

		/// <summary>
		/// Change value.
		/// </summary>
		/// <typeparam name="T">Type of field.</typeparam>
		/// <param name="field">Field value.</param>
		/// <param name="value">New value.</param>
		/// <param name="propertyName">Property name.</param>
		/// <returns>true if field was changed; otherwise false.</returns>
		protected bool Change<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			Changed(propertyName);

			return true;
		}

		/// <summary>
		/// Raise PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected virtual void Changed(string propertyName)
		{
			if (PauseObservation)
			{
				return;
			}

			OnChange?.Invoke();
		}
	}
}