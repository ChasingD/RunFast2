namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Time functions.
	/// </summary>
	public class WidgetsTime : ITime
	{
		/// <summary>
		/// Instance.
		/// </summary>
		public static ref ITime Instance => ref StaticFields.Instance.Time;

		/// <summary>
		/// Frame count.
		/// </summary>
		public int FrameCount => UnityEngine.Time.frameCount;

		/// <summary>
		/// Get time.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled time.</param>
		/// <returns>Time.</returns>
		public float Time(bool unscaledTime) => unscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;

		/// <summary>
		/// Get delta time.
		/// </summary>
		/// <param name="unscaledTime">Return unscaled delta time.</param>
		/// <returns>Delta time.</returns>
		public float DeltaTime(bool unscaledTime) => unscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;

		/// <summary>
		/// Suspends the coroutine execution for the given amount of seconds.
		/// </summary>
		/// <param name="seconds">Seconds.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		/// <returns>Coroutine.</returns>
		public CustomYieldInstruction Wait(float seconds, bool unscaledTime) => WaitForSecondsCustom.Instance(seconds, unscaledTime);
	}
}