namespace UIWidgets
{
	using System.Collections;
	using UIWidgets.Attributes;
	using UnityEngine;

	/// <summary>
	/// Window animations.
	/// </summary>
	[DisallowMultipleComponent]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/components/windows/windows-animations.html")]
	public class WindowAnimations : BaseWindowAnimations
	{
		/// <summary>
		/// Animation curve.
		/// </summary>
		[SerializeField]
		public AnimationCurve AnimationCurve = AnimationCurve.EaseInOut(0f, 0.0f, 0.15f, 1f);

		/// <summary>
		/// Animate alpha.
		/// </summary>
		[SerializeField]
		public bool AnimateAlpha = true;

		/// <summary>
		/// Animate scale.
		/// </summary>
		[SerializeField]
		public bool AnimateScale = true;

		/// <summary>
		/// Animate anchors.
		/// </summary>
		[SerializeField]
		public bool AnimateAnchors = false;

		/// <summary>
		/// Anchor X.
		/// </summary>
		[SerializeField]
		[EditorConditionBool(nameof(AnimateAnchors))]
		[Range(0f, 1f)]
		public float AnchorX = 0f;

		/// <summary>
		/// Anchor Y.
		/// </summary>
		[SerializeField]
		[EditorConditionBool(nameof(AnimateAnchors))]
		[Range(0f, 1f)]
		public float AnchorY = 0f;

		/// <summary>
		/// Disable interactable.
		/// </summary>
		[SerializeField]
		public bool DisableInteractable = true;

		/// <inheritdoc/>
		public override IEnumerator Open() => Animation();

		/// <inheritdoc/>
		public override IEnumerator Close() => Animation(reverse: true);

		IEnumerator Animation(bool reverse = false)
		{
			InitOnce();

			var scale = RectTransform.localScale;
			var anchor_min = RectTransform.anchorMin;
			var anchor_max = RectTransform.anchorMax;
			var alpha = CanvasGroup.alpha;

			if (DisableInteractable)
			{
				CanvasGroup.interactable = false;
			}

			var anchor_target = new Vector2(AnchorX, AnchorY);
			var duration = AnimationCurve[AnimationCurve.length - 1].time;
			var time = 0f;

			do
			{
				var v = AnimationCurve.Evaluate(reverse ? duration - time : time);
				if (AnimateAlpha)
				{
					CanvasGroup.alpha = alpha * v;
				}

				if (AnimateScale)
				{
					RectTransform.localScale = scale * v;
				}

				if (AnimateAnchors)
				{
					RectTransform.anchorMin = Vector2.Lerp(anchor_target, anchor_min, v);
					RectTransform.anchorMax = Vector2.Lerp(anchor_target, anchor_max, v);
				}

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(UnscaledTime);
			}
			while (time < duration);

			RectTransform.localScale = scale;
			RectTransform.anchorMin = anchor_min;
			RectTransform.anchorMax = anchor_max;
			CanvasGroup.alpha = alpha;

			if (DisableInteractable)
			{
				CanvasGroup.interactable = true;
			}
		}
	}
}