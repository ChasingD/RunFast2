namespace UIWidgets
{
	using System.ComponentModel;
	using UIWidgets.Attributes;

	/// <summary>
	/// Information on maybe observable type.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	public class ObservableInfo<T>
	{
		/// <summary>
		/// Is value type?
		/// </summary>
		public readonly bool IsValueType;

		/// <summary>
		/// Type instance implements IObservable.
		/// </summary>
		public readonly bool IsObservable;

		/// <summary>
		/// Type instance implements INotifyPropertyChanged.
		/// </summary>
		public readonly bool IsNotifyPropertyChanged;

		/// <summary>
		/// Instance.
		/// </summary>
		[DomainReloadExclude]
		public static readonly ObservableInfo<T> Instance = new ObservableInfo<T>();

		ObservableInfo()
		{
			var type = typeof(T);
			IsValueType = type.IsValueType;
			IsObservable = !IsValueType && typeof(IObservable).IsAssignableFrom(type);
			IsNotifyPropertyChanged = !IsValueType && typeof(INotifyPropertyChanged).IsAssignableFrom(type);
		}
	}
}