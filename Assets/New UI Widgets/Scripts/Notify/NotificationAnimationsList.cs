namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// List of notification animations.
	/// </summary>
	public class NotificationAnimationsList
	{
		#region HideAnimationRotate

		/// <summary>
		/// Initializes a new instance of the <see cref="NotificationAnimationsList"/> class.
		/// </summary>
		public NotificationAnimationsList()
		{
#pragma warning disable 0618
			AnimationRotate = AnimationRotateMethod;
			AnimationRotateBase = AnimationRotateBaseMethod;
			AnimationRotateUnscaledTime = AnimationRotateUnscaledTimeMethod;

			AnimationCollapseBase = AnimationCollapseBaseMethod;
			AnimationCollapse = AnimationCollapseMethod;
			AnimationCollapseUnscaledTime = AnimationCollapseUnscaledTimeMethod;
#pragma warning restore

			AnimationRotateVertical = AnimationRotateVerticalMethod;
			AnimationRotateHorizontal = AnimationRotateHorizontalMethod;
			HideAnimationRotateBase = HideAnimationRotateBaseMethod;

			HideAnimationCollapseBase = HideAnimationCollapseBaseMethod;
			AnimationCollapseVertical = AnimationCollapseVerticalMethod;
			AnimationCollapseHorizontal = AnimationCollapseHorizontalMethod;

			AnimationSlideRight = AnimationSlideRightMethod;
			AnimationSlideLeft = AnimationSlideLeftMethod;
			AnimationSlideUp = AnimationSlideUpMethod;
			AnimationSlideDown = AnimationSlideDownMethod;

			ShowAnimationSlideRight = ShowAnimationSlideRightMethod;
			ShowAnimationSlideLeft = ShowAnimationSlideLeftMethod;
			ShowAnimationSlideUp = ShowAnimationSlideUpMethod;
			ShowAnimationSlideDown = ShowAnimationSlideDownMethod;

			ShowAnimationExplodeBase = ShowAnimationExplodeBaseMethod;
			ShowAnimationExplodeVertical = ShowAnimationExplodeVerticalMethod;
			ShowAnimationExplodeHorizontal = ShowAnimationExplodeHorizontalMethod;

			ShowAnimationRotateVertical = ShowAnimationRotateVerticalMethod;
			ShowAnimationRotateHorizontal = ShowAnimationRotateHorizontalMethod;
			ShowAnimationRotateBase = ShowAnimationRotateBaseMethod;

			FadeIn = FadeInMethod;
			FadeOut = FadeOutMethod;
		}

		/// <summary>
		/// Replacements cache.
		/// </summary>
		protected static Stack<RectTransform> Replacements => StaticFields.Instance.NotificationReplacements;

		/// <summary>
		/// Get replacement for this instance.
		/// </summary>
		/// <returns>Replacement.</returns>
		protected static RectTransform GetReplacement()
		{
			RectTransform rect;

			if (Replacements.Count == 0)
			{
				var obj = new GameObject("NotificationReplacement");
				obj.SetActive(false);
				rect = obj.AddComponent<RectTransform>();

				// change size don't work without graphic component
				var image = obj.AddComponent<Image>();
				image.enabled = false;
			}
			else
			{
				do
				{
					rect = (Replacements.Count > 0) ? Replacements.Pop() : GetReplacement();
				}
				while (rect == null);
			}

			return rect;
		}

		/// <summary>
		/// Get notification replacement.
		/// </summary>
		/// <param name="rectTransform">RectTransform to replace.</param>
		/// <returns>Replacement.</returns>
		public static RectTransform GetReplacement(RectTransform rectTransform)
		{
			var target = GetReplacement();

			target.localRotation = rectTransform.localRotation;
			target.localPosition = rectTransform.localPosition;
			target.localScale = rectTransform.localScale;
			target.anchorMin = rectTransform.anchorMin;
			target.anchorMax = rectTransform.anchorMax;
			target.anchoredPosition = rectTransform.anchoredPosition;
			target.sizeDelta = rectTransform.sizeDelta;
			target.pivot = rectTransform.pivot;

			target.transform.SetParent(rectTransform.parent, false);
			target.transform.SetSiblingIndex(rectTransform.GetSiblingIndex());

			target.gameObject.SetActive(true);

			return target;
		}

		/// <summary>
		/// Returns replacement slide to cache.
		/// </summary>
		/// <param name="replacement">Replacement.</param>
		public static void FreeReplacement(RectTransform replacement)
		{
			replacement.gameObject.SetActive(value: false);

			Replacements.Push(replacement);
		}

		/// <summary>
		/// Vertical rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to AnimationRotateVertical.")]
		public readonly Func<NotificationBase, IEnumerator> AnimationRotate;

		IEnumerator AnimationRotateMethod(NotificationBase notification) => AnimationRotateVertical(notification);

		/// <summary>
		/// Vertical rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationRotateVertical;

		IEnumerator AnimationRotateVerticalMethod(NotificationBase notification)
			=> HideRotate(notification.RectTransform, notification.UnscaledTime, false, 0.5f);

		/// <summary>
		/// Horizontal rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationRotateHorizontal;

		IEnumerator AnimationRotateHorizontalMethod(NotificationBase notification)
			=> HideRotate(notification.RectTransform, notification.UnscaledTime, true, 0.5f);

		/// <summary>
		/// Base rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to HideAnimationRotateBase.")]
		public readonly Func<NotificationBase, bool, float, IEnumerator> AnimationRotateBase;

		IEnumerator AnimationRotateBaseMethod(NotificationBase notification, bool isHorizontal, float timeLength)
			=> HideAnimationRotateBase(notification, isHorizontal, timeLength);

		/// <summary>
		/// Base rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, bool, float, IEnumerator> HideAnimationRotateBase;

		IEnumerator HideAnimationRotateBaseMethod(NotificationBase notification, bool isHorizontal, float duration)
			=> HideRotate(notification.RectTransform, notification.UnscaledTime, isHorizontal, duration);

		/// <summary>
		/// Hide animation: rotate.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Animate with unscaled time.</param>
		/// <param name="isHorizontal">Horizontal animation.</param>
		/// <param name="duration">Duration.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator HideRotate(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float duration)
			=> HideRotate(rectTransform, unscaledTime, isHorizontal, AnimationCurve.EaseInOut(0f, 0f, duration, 1f));

		/// <summary>
		/// Hide animation: rotate.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Animate with unscaled time.</param>
		/// <param name="isHorizontal">Horizontal animation.</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator HideRotate(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, AnimationCurve animation)
		{
			var base_rotation = rectTransform.localRotation.eulerAngles;
			var time = 0f;
			var duration = animation[animation.length - 1].time;

			do
			{
				var rotation = 90f * animation.Evaluate(duration - time);

				rectTransform.localRotation = isHorizontal
					? Quaternion.Euler(base_rotation.x, rotation, base_rotation.z)
					: Quaternion.Euler(rotation, base_rotation.y, base_rotation.z);
				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			// return rotation back for future use
			rectTransform.localRotation = Quaternion.Euler(base_rotation);
		}

		/// <summary>
		/// Rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("AnimationRotate() now supports UnscaledTime.")]
		public readonly Func<NotificationBase, IEnumerator> AnimationRotateUnscaledTime;

		IEnumerator AnimationRotateUnscaledTimeMethod(NotificationBase notification) => AnimationRotateVertical(notification);

		#endregion

		#region HideAnimationCollapse

		/// <summary>
		/// Base collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to HideAnimationCollapseBase.")]
		public readonly Func<NotificationBase, bool, float, IEnumerator> AnimationCollapseBase;

		IEnumerator AnimationCollapseBaseMethod(NotificationBase notification, bool isHorizontal, float speed)
			=> HideAnimationCollapseBase(notification, isHorizontal, speed);

		/// <summary>
		/// Base collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, bool, float, IEnumerator> HideAnimationCollapseBase;

		IEnumerator HideAnimationCollapseBaseMethod(NotificationBase notification, bool isHorizontal, float speed)
			=> HideCollapseSpeed(notification.RectTransform, notification.UnscaledTime, isHorizontal, speed);

		/// <summary>
		/// Hide animation: collapse.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Animate with unscaled time.</param>
		/// <param name="isHorizontal">Horizontal animation.</param>
		/// <param name="speed">Speed.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator HideCollapseSpeed(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float speed)
		{
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			return HideCollapse(rectTransform, unscaledTime, isHorizontal, base_size / speed);
		}

		/// <summary>
		/// Hide animation: collapse.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Animate with unscaled time.</param>
		/// <param name="isHorizontal">Horizontal animation.</param>
		/// <param name="duration">Duration.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator HideCollapse(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float duration)
			=> HideCollapse(rectTransform, unscaledTime, isHorizontal, AnimationCurve.EaseInOut(0f, 0f, duration, 1f));

		/// <summary>
		/// Hide animation: collapse.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Animate with unscaled time.</param>
		/// <param name="isHorizontal">Horizontal animation.</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator HideCollapse(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, AnimationCurve animation)
		{
			var axis = isHorizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			var time = 0f;
			var duration = animation[animation.length - 1].time;

			do
			{
				var size = base_size * animation.Evaluate(time);
				rectTransform.SetSizeWithCurrentAnchors(axis, size);

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			// return height back for future use
			rectTransform.SetSizeWithCurrentAnchors(axis, base_size);
		}

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to AnimationCollapseVertical.")]
		public readonly Func<NotificationBase, IEnumerator> AnimationCollapse;

		IEnumerator AnimationCollapseMethod(NotificationBase notification) => AnimationCollapseVertical(notification);

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationCollapseVertical;

		IEnumerator AnimationCollapseVerticalMethod(NotificationBase notification) => HideAnimationCollapseBase(notification, false, 200f);

		/// <summary>
		/// Horizontal collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationCollapseHorizontal;

		IEnumerator AnimationCollapseHorizontalMethod(NotificationBase notification) => HideAnimationCollapseBase(notification, true, 200f);

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("AnimationCollapse now supports UnscaledTime.")]
		public readonly Func<NotificationBase, IEnumerator> AnimationCollapseUnscaledTime;

		IEnumerator AnimationCollapseUnscaledTimeMethod(NotificationBase notification) => AnimationCollapseVertical(notification);

		#endregion

		#region HideAnimationSlide

		/// <summary>
		/// Slide animation to hide notification to right.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationSlideRight;

		IEnumerator AnimationSlideRightMethod(NotificationBase notification)
			=> HideSlideSpeed(notification.RectTransform, notification.UnscaledTime, true, +1f, 200f);

		/// <summary>
		/// Slide animation to hide notification to left.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationSlideLeft;

		IEnumerator AnimationSlideLeftMethod(NotificationBase notification)
			=> HideSlideSpeed(notification.RectTransform, notification.UnscaledTime, true, -1f, 200f);

		/// <summary>
		/// Slide animation to hide notification to top.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationSlideUp;

		IEnumerator AnimationSlideUpMethod(NotificationBase notification)
			=> HideSlideSpeed(notification.RectTransform, notification.UnscaledTime, false, +1f, 200f);

		/// <summary>
		/// Slide animation to hide notification to bottom.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> AnimationSlideDown;

		IEnumerator AnimationSlideDownMethod(NotificationBase notification)
			=> HideSlideSpeed(notification.RectTransform, notification.UnscaledTime, false, -1f, 200f);

		/// <summary>
		/// Base slide animation to hide notification.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to AnimationSlideBaseHide.")]
		public IEnumerator AnimationSlideBase(NotificationBase notification, bool isHorizontal, float direction, float speed, bool animateOthers = true)
			=> HideSlideSpeed(notification.RectTransform, notification.UnscaledTime, isHorizontal, direction, speed, animateOthers);

		/// <summary>
		/// Hide animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator HideSlideSpeed(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, float speed, bool animateOthers = true)
		{
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			return HideSlide(rectTransform, unscaledTime, isHorizontal, direction, base_size / speed, animateOthers);
		}

		/// <summary>
		/// Hide animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator HideSlide(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, float duration, bool animateOthers = true)
			=> HideSlide(rectTransform, unscaledTime, isHorizontal, direction, AnimationCurve.EaseInOut(0f, 0f, duration, 1f), animateOthers);

		/// <summary>
		/// Hide animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="animation">Animation curve.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator HideSlide(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, AnimationCurve animation, bool animateOthers = true)
		{
			var replacement = animateOthers ? GetReplacement(rectTransform) : null;
			var container = rectTransform.parent as RectTransform;

			var layout_element = Utilities.RequireComponent<LayoutElement>(rectTransform);
			layout_element.ignoreLayout = true;
			rectTransform.SetAsLastSibling();

			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			var base_pos = rectTransform.anchoredPosition;
			var time = 0f;
			var axis = isHorizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
			var duration = animation[animation.length - 1].time;

			do
			{
				var size = base_size * animation.Evaluate(duration - time);
				rectTransform.anchoredPosition = isHorizontal
					? new Vector2(base_pos.x + (size * direction), base_pos.y)
					: new Vector2(base_pos.x, base_pos.y + (size * direction));

				if (animateOthers)
				{
					replacement.SetSizeWithCurrentAnchors(axis, base_size - size);
					LayoutRebuilder.MarkLayoutForRebuild(container);
				}

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			layout_element.ignoreLayout = false;

			if (animateOthers)
			{
				replacement.SetSizeWithCurrentAnchors(axis, base_size);
				FreeReplacement(replacement);
			}
		}

		#endregion

		#region ShowAnimationSlide

		/// <summary>
		/// Slide animation to show notification from right.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationSlideRight;

		IEnumerator ShowAnimationSlideRightMethod(NotificationBase notification)
			=> ShowSlideSpeed(notification.RectTransform, notification.UnscaledTime, true, +1f, 200f);

		/// <summary>
		/// Slide animation to show notification from left.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationSlideLeft;

		IEnumerator ShowAnimationSlideLeftMethod(NotificationBase notification)
			=> ShowSlideSpeed(notification.RectTransform, notification.UnscaledTime, true, -1f, 200f);

		/// <summary>
		/// Slide animation to show notification from top.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationSlideUp;

		IEnumerator ShowAnimationSlideUpMethod(NotificationBase notification)
			=> ShowSlideSpeed(notification.RectTransform, notification.UnscaledTime, false, +1f, 200f);

		/// <summary>
		/// Slide animation to show notification from bottom.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationSlideDown;

		IEnumerator ShowAnimationSlideDownMethod(NotificationBase notification)
		{
			return ShowSlideSpeed(notification.RectTransform, notification.UnscaledTime, false, -1f, 200f);
		}

		/// <summary>
		/// Show animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator ShowSlideSpeed(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, float speed, bool animateOthers = true)
		{
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			return ShowSlide(rectTransform, unscaledTime, isHorizontal, direction, base_size / speed, animateOthers);
		}

		/// <summary>
		/// Show animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator ShowSlide(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, float duration, bool animateOthers = true)
			=> ShowSlide(rectTransform, unscaledTime, isHorizontal, direction, AnimationCurve.EaseInOut(0f, 0f, duration, 1f), animateOthers);

		/// <summary>
		/// Show animation: slide.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="animation">Animation curve.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator ShowSlide(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float direction, AnimationCurve animation, bool animateOthers = true)
		{
			var container = rectTransform.parent as RectTransform;
			var replacement = GetReplacement(rectTransform);

			var layout_element = Utilities.RequireComponent<LayoutElement>(rectTransform);
			layout_element.ignoreLayout = true;
			LayoutRebuilder.ForceRebuildLayoutImmediate(container);

			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			var base_pos = replacement.anchoredPosition;
			var time = 0f;
			var axis = isHorizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
			var duration = animation[animation.length - 1].time;

			do
			{
				base_pos = replacement.anchoredPosition;

				var size = base_size * animation.Evaluate(duration - time);
				rectTransform.anchoredPosition = isHorizontal
					? new Vector2(base_pos.x + (size * direction), base_pos.y)
					: new Vector2(base_pos.x, base_pos.y + (size * direction));

				if (animateOthers)
				{
					replacement.SetSizeWithCurrentAnchors(axis, base_size - size);
					LayoutRebuilder.MarkLayoutForRebuild(container);
				}

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			layout_element.ignoreLayout = false;

			replacement.SetSizeWithCurrentAnchors(axis, base_size);
			FreeReplacement(replacement);
		}

		#endregion

		#region ShowAnimationExplode

		/// <summary>
		/// Base explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, bool, float, IEnumerator> ShowAnimationExplodeBase;

		IEnumerator ShowAnimationExplodeBaseMethod(NotificationBase notification, bool isHorizontal, float speed)
			=> ShowExplodeSpeed(notification.RectTransform, notification.UnscaledTime, isHorizontal, speed);

		/// <summary>
		/// Show animation: explode.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal animation?</param>
		/// <param name="speed">Animation speed.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator ShowExplodeSpeed(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float speed)
		{
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			return ShowExplode(rectTransform, unscaledTime, isHorizontal, base_size / speed);
		}

		/// <summary>
		/// Show animation: explode.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal animation?</param>
		/// <param name="duration">Animation duration.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator ShowExplode(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float duration)
			=> ShowExplode(rectTransform, unscaledTime, isHorizontal, AnimationCurve.EaseInOut(0f, 0f, duration, 1f));

		/// <summary>
		/// Show animation: explode.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal animation?</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator ShowExplode(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, AnimationCurve animation)
		{
			var axis = isHorizontal ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
			var base_size = isHorizontal ? rectTransform.rect.width : rectTransform.rect.height;
			var time = 0f;
			var duration = animation[animation.length - 1].time;

			do
			{
				var size = base_size * animation.Evaluate(time);
				rectTransform.SetSizeWithCurrentAnchors(axis, size);
				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			// return height back for future use
			rectTransform.SetSizeWithCurrentAnchors(axis, base_size);
		}

		/// <summary>
		/// Vertical explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationExplodeVertical;

		IEnumerator ShowAnimationExplodeVerticalMethod(NotificationBase notification) => ShowAnimationExplodeBase(notification, false, 200f);

		/// <summary>
		/// Horizontal explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationExplodeHorizontal;

		IEnumerator ShowAnimationExplodeHorizontalMethod(NotificationBase notification) => ShowAnimationExplodeBase(notification, true, 200f);
		#endregion

		#region ShowAnimationRotate

		/// <summary>
		/// Vertical rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationRotateVertical;

		IEnumerator ShowAnimationRotateVerticalMethod(NotificationBase notification) => ShowAnimationRotateBase(notification, false, 0.5f);

		/// <summary>
		/// Horizontal rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, IEnumerator> ShowAnimationRotateHorizontal;

		IEnumerator ShowAnimationRotateHorizontalMethod(NotificationBase notification) => ShowAnimationRotateBase(notification, true, 0.5f);

		/// <summary>
		/// Base rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public readonly Func<NotificationBase, bool, float, IEnumerator> ShowAnimationRotateBase;

		IEnumerator ShowAnimationRotateBaseMethod(NotificationBase notification, bool isHorizontal, float duration)
			=> ShowRotate(notification.RectTransform, notification.UnscaledTime, isHorizontal, duration);

		/// <summary>
		/// Show animation: rotate.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal direction?</param>
		/// <param name="duration">Animation duration.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator ShowRotate(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, float duration)
			=> ShowRotate(rectTransform, unscaledTime, isHorizontal, AnimationCurve.EaseInOut(0f, 0f, duration, 1f));

		/// <summary>
		/// Show animation: rotate.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="isHorizontal">Is horizontal direction?</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Coroutine.</returns>
		public IEnumerator ShowRotate(RectTransform rectTransform, bool unscaledTime, bool isHorizontal, AnimationCurve animation)
		{
			var base_rotation = rectTransform.localRotation.eulerAngles;
			var time = 0f;
			var duration = animation[animation.length - 1].time;

			do
			{
				var rotation = 90f * animation.Evaluate(duration - time);

				rectTransform.localRotation = isHorizontal
					? Quaternion.Euler(base_rotation.x, rotation, base_rotation.z)
					: Quaternion.Euler(rotation, base_rotation.y, base_rotation.z);
				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			// return rotation back for future use
			rectTransform.localRotation = Quaternion.Euler(base_rotation);
		}

		#endregion

		#region FadeAnimation

		/// <summary>
		/// Fade in animation.
		/// </summary>
		public readonly Func<NotificationBase, IEnumerator> FadeIn;

		IEnumerator FadeInMethod(NotificationBase notification)
			=> Fade(notification.RectTransform, notification.UnscaledTime, AnimationCurve.EaseInOut(0f, 0f, 0.5f, 1f));

		/// <summary>
		/// Fade out animation.
		/// </summary>
		public readonly Func<NotificationBase, IEnumerator> FadeOut;

		IEnumerator FadeOutMethod(NotificationBase notification)
			=> Fade(notification.RectTransform, notification.UnscaledTime, AnimationCurve.EaseInOut(0f, 1f, 0.5f, 0f));

		/// <summary>
		/// Fade animation.
		/// </summary>
		/// <param name="rectTransform">RectTransform.</param>
		/// <param name="unscaledTime">Unscaled time.</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Animation coroutine.</returns>
		public IEnumerator Fade(RectTransform rectTransform, bool unscaledTime, AnimationCurve animation)
		{
			var group = Utilities.RequireComponent<CanvasGroup>(rectTransform);

			var time = 0f;
			var last = animation[animation.length - 1];
			var duration = last.time;

			do
			{
				group.alpha = animation.Evaluate(time);

				yield return null;

				time += WidgetsTime.Instance.DeltaTime(unscaledTime);
			}
			while (time < duration);

			group.alpha = 1f;
		}

		#endregion
	}
}