namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Time interface.
	/// </summary>
	public interface ITime
	{
		/// <summary>
		/// Frame count.
		/// </summary>
		public int FrameCount
		{
			get;
		}

		/// <summary>
		/// Get time.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled time.</param>
		/// <returns>Time.</returns>
		public float Time(bool unscaledTime);

		/// <summary>
		/// Get delta time.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled delta time.</param>
		/// <returns>Delta time.</returns>
		public float DeltaTime(bool unscaledTime);

		/// <summary>
		/// Suspends the coroutine execution for the given amount of seconds.
		/// </summary>
		/// <param name="seconds">Seconds.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <returns>Coroutine.</returns>
		public CustomYieldInstruction Wait(float seconds, bool unscaledTime);
	}
}