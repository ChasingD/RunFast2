namespace UIWidgets
{
	using System;
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// StackView animations.
	/// </summary>
	public class StackViewAnimations : StackViewAnimationsBase
	{
		/// <summary>
		/// Axis.
		/// </summary>
		[Serializable]
		public enum Axis
		{
			/// <summary>
			/// Horizontal.
			/// </summary>
			Horizontal = 0,

			/// <summary>
			/// Vertical.
			/// </summary>
			Vertical = 1,
		}

		/// <summary>
		/// Settings.
		/// </summary>
		[Serializable]
		public struct Settings
		{
			/// <summary>
			/// Curve.
			/// </summary>
			[SerializeField]
			public AnimationCurve Curve;

			/// <summary>
			/// Axis.
			/// </summary>
			[SerializeField]
			public Axis Axis;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="curve">Curve.</param>
			/// <param name="axis">Axis.</param>
			public Settings(AnimationCurve curve, Axis axis = Axis.Horizontal)
			{
				Curve = curve;
				Axis = axis;
			}
		}

		/// <summary>
		/// PushNew settings: move from right to left.
		/// </summary>
		[SerializeField]
		public Settings PushNewSettings = new Settings(AnimationCurve.EaseInOut(0f, 1f, 0.3f, 0f));

		/// <summary>
		/// PushCurrent settings: move from right to left on short distance (20%).
		/// </summary>
		[SerializeField]
		public Settings PushCurrentSettings = new Settings(AnimationCurve.EaseInOut(0f, 0f, 0.3f, -0.2f));

		/// <summary>
		/// PopCurrent settings: move down.
		/// </summary>
		[SerializeField]
		public Settings PopCurrentSettings = new Settings(AnimationCurve.EaseInOut(0f, 0f, 0.3f, -1f), Axis.Vertical);

		/// <summary>
		/// PopPrevious settings: move left to right.
		/// </summary>
		[SerializeField]
		public Settings PopPreviousSettings = new Settings(AnimationCurve.EaseInOut(0f, -0.2f, 0.3f, 0f), Axis.Horizontal);

		/// <summary>
		/// ReplaceNew settings: move from right to left.
		/// </summary>
		[SerializeField]
		public Settings ReplaceNewSettings = new Settings(AnimationCurve.EaseInOut(0f, 1f, 0.3f, 0f));

		/// <summary>
		/// ReplaceCurrent settings: move down.
		/// </summary>
		[SerializeField]
		public Settings ReplaceCurrentSettings = new Settings(AnimationCurve.EaseInOut(0f, 0f, 0.3f, -1f), Axis.Vertical);

		/// <summary>
		/// Animate with unscaled time.
		/// </summary>
		[SerializeField]
		public bool UnscaledTime = true;

		/// <inheritdoc/>
		public override IEnumerator PushNew(RectTransform view) => Move(view, PushNewSettings);

		/// <inheritdoc/>
		public override IEnumerator PushCurrent(RectTransform view) => Move(view, PushCurrentSettings);

		/// <inheritdoc/>
		public override IEnumerator PopCurrent(RectTransform view) => Move(view, PopCurrentSettings);

		/// <inheritdoc/>
		public override IEnumerator PopPrevious(RectTransform view) => Move(view, PopPreviousSettings);

		/// <inheritdoc/>
		public override IEnumerator ReplaceNew(RectTransform view) => Move(view, ReplaceNewSettings);

		/// <inheritdoc/>
		public override IEnumerator ReplaceCurrent(RectTransform view) => Move(view, ReplaceCurrentSettings);

		/// <summary>
		/// Movement animation.
		/// </summary>
		/// <param name="rectTransform">Target.</param>
		/// <param name="settings">Settings.</param>
		/// <returns>Animation coroutine.</returns>
		protected virtual IEnumerator Move(RectTransform rectTransform, Settings settings)
		{
			var is_horizontal = settings.Axis == Axis.Horizontal;
			var base_size = is_horizontal ? rectTransform.rect.width : rectTransform.rect.height;
			var base_pos = rectTransform.anchoredPosition;
			var time = 0f;
			var duration = settings.Curve[settings.Curve.length - 1].time;

			do
			{
				var pos = base_size * settings.Curve.Evaluate(time);
				rectTransform.anchoredPosition = is_horizontal
					? new Vector2(base_pos.x + pos, base_pos.y)
					: new Vector2(base_pos.x, base_pos.y + pos);

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(UnscaledTime);
			}
			while (time < duration);
		}
	}
}