namespace UIWidgets
{
	using UnityEngine.Events;

	/// <summary>
	/// interface for displaying item data.
	/// </summary>
	/// <typeparam name="T">Item type.</typeparam>
	public interface IViewData<T>
	{
		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		void SetData(T item);

		/// <summary>
		/// Item changed event.
		/// </summary>
		public UnityEvent OnItemChanged
		{
			get;
		}
	}
}