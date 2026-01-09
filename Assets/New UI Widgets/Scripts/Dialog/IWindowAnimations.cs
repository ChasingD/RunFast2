namespace UIWidgets
{
	using System.Collections;

	/// <summary>
	/// Interface for window animations.
	/// </summary>
	public interface IWindowAnimations
	{
		/// <summary>
		/// Enable animations.
		/// </summary>
		bool Enabled
		{
			get;
		}

		/// <summary>
		/// Open animation.
		/// </summary>
		/// <returns>Coroutine.</returns>
		IEnumerator Open();

		/// <summary>
		/// Close animation.
		/// </summary>
		/// <returns>Coroutine.</returns>
		IEnumerator Close();
	}
}