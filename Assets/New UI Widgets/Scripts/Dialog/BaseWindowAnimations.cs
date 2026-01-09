namespace UIWidgets
{
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// Base class for window animations.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public abstract class BaseWindowAnimations : MonoBehaviourConditional, IWindowAnimations
	{
		/// <summary>
		/// Animate with unscaled time.
		/// </summary>
		[SerializeField]
		public bool UnscaledTime = true;

		/// <summary>
		/// RectTransform.
		/// </summary>
		protected RectTransform RectTransform;

		/// <summary>
		/// CanvasGroup.
		/// </summary>
		protected CanvasGroup CanvasGroup;

		/// <summary>
		/// Enable animations.
		/// </summary>
		public virtual bool Enabled => enabled;

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();

			RectTransform = transform as RectTransform;
			TryGetComponent(out CanvasGroup);
		}

		/// <summary>
		/// Open.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator Open();

		/// <summary>
		/// Close.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator Close();
	}
}