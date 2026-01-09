namespace UIWidgets
{
	using System;
	using System.Collections;
	using UIWidgets.Attributes;
	using UnityEngine;

	/// <summary>
	/// Notification animations.
	/// </summary>
	[DisallowMultipleComponent]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/components/windows/notifications-animations.html")]
	public class NotificationAnimations : BaseWindowAnimations
	{
		/// <summary>
		/// Show animations.
		/// </summary>
		public enum ShowMode
		{
			/// <summary>
			/// Rotate.
			/// </summary>
			Rotate = 0,

			/// <summary>
			/// Explode.
			/// </summary>
			Explode = 1,

			/// <summary>
			/// FadeIn.
			/// </summary>
			FadeIn = 2,

			/// <summary>
			/// Slide right.
			/// </summary>
			SlideRight = 20,

			/// <summary>
			/// Slide left.
			/// </summary>
			SlideLeft = 21,

			/// <summary>
			/// Slide up.
			/// </summary>
			SlideUp = 22,

			/// <summary>
			/// Slide down.
			/// </summary>
			SlideDown = 23,
		}

		/// <summary>
		/// Hide animations.
		/// </summary>
		public enum HideMode
		{
			/// <summary>
			/// Rotate.
			/// </summary>
			Rotate = 0,

			/// <summary>
			/// Collapse.
			/// </summary>
			Collapse = 1,

			/// <summary>
			/// Fade out.
			/// </summary>
			FadeOut = 2,

			/// <summary>
			/// Slide right.
			/// </summary>
			SlideRight = 20,

			/// <summary>
			/// Slide left.
			/// </summary>
			SlideLeft = 21,

			/// <summary>
			/// Slide up.
			/// </summary>
			SlideUp = 22,

			/// <summary>
			/// Slide down.
			/// </summary>
			SlideDown = 23,
		}

		/// <summary>
		/// Disable interactable.
		/// </summary>
		[SerializeField]
		public bool DisableInteractable = true;

		/// <summary>
		/// Show animation.
		/// </summary>
		[SerializeField]
		public ShowMode Show = ShowMode.Rotate;

		/// <summary>
		/// Show: animation duration.
		/// </summary>
		[Obsolete("Replaced with ShowAnimationCurve.")]
		public float ShowDuration
		{
			get => ShowAnimationCurve[ShowAnimationCurve.length - 1].time;

			set => ShowAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, value, 1f);
		}

		/// <summary>
		/// Show: animate in horizontal direction.
		/// </summary>
		[SerializeField]
		[EditorConditionEnum(nameof(Show), (int)ShowMode.Rotate, (int)ShowMode.Explode)]
		public bool ShowHorizontal = true;

		/// <summary>
		/// Snow: animation curve.
		/// </summary>
		[SerializeField]
		public AnimationCurve ShowAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 0.5f, 1f);

		/// <summary>
		/// Hide animation.
		/// </summary>
		[SerializeField]
		public HideMode Hide = HideMode.Rotate;

		/// <summary>
		/// Hide: animation duration.
		/// </summary>
		[Obsolete("Replaced with HideAnimationCurve.")]
		public float HideDuration
		{
			get => HideAnimationCurve[HideAnimationCurve.length - 1].time;

			set => HideAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, value, 1f);
		}

		/// <summary>
		/// Hide: animate in horizontal direction.
		/// </summary>
		[SerializeField]
		[EditorConditionEnum(nameof(Hide), (int)HideMode.Rotate, (int)HideMode.Collapse)]
		public bool HideHorizontal = true;

		/// <summary>
		/// Hide: animation curve.
		/// </summary>
		[SerializeField]
		public AnimationCurve HideAnimationCurve = AnimationCurve.EaseInOut(0f, 1f, 0.5f, 0f);

		/// <summary>
		/// Animations list.
		/// </summary>
		protected static NotificationAnimationsList Animations => StaticFields.Instance.NotificationAnimations;

		/// <inheritdoc/>
		public override IEnumerator Open()
		{
			Init();

			if (DisableInteractable)
			{
				CanvasGroup.interactable = false;
			}

			yield return Show switch
			{
				ShowMode.Rotate => Animations.ShowRotate(RectTransform, UnscaledTime, ShowHorizontal, ShowAnimationCurve),
				ShowMode.Explode => Animations.ShowExplode(RectTransform, UnscaledTime, ShowHorizontal, ShowAnimationCurve),
				ShowMode.FadeIn => Animations.Fade(RectTransform, UnscaledTime, ShowAnimationCurve),

				ShowMode.SlideRight => Animations.ShowSlide(RectTransform, UnscaledTime, true, +1f, ShowAnimationCurve, false),
				ShowMode.SlideLeft => Animations.ShowSlide(RectTransform, UnscaledTime, true, -1f, ShowAnimationCurve, false),
				ShowMode.SlideUp => Animations.ShowSlide(RectTransform, UnscaledTime, false, +1f, ShowAnimationCurve, false),
				ShowMode.SlideDown => Animations.ShowSlide(RectTransform, UnscaledTime, false, -1f, ShowAnimationCurve, false),
				_ => throw new NotImplementedException("Unknown animation: " + EnumHelper<ShowMode>.Instance.ToString(Show)),
			};

			if (DisableInteractable)
			{
				CanvasGroup.interactable = true;
			}
		}

		/// <inheritdoc/>
		public override IEnumerator Close()
		{
			Init();

			if (DisableInteractable)
			{
				CanvasGroup.interactable = false;
			}

			yield return Hide switch
			{
				HideMode.Rotate => Animations.HideRotate(RectTransform, UnscaledTime, HideHorizontal, HideAnimationCurve),
				HideMode.Collapse => Animations.HideCollapse(RectTransform, UnscaledTime, HideHorizontal, HideAnimationCurve),
				HideMode.FadeOut => Animations.Fade(RectTransform, UnscaledTime, HideAnimationCurve),

				HideMode.SlideRight => Animations.HideSlide(RectTransform, UnscaledTime, true, +1f, HideAnimationCurve, false),
				HideMode.SlideLeft => Animations.HideSlide(RectTransform, UnscaledTime, true, -1f, HideAnimationCurve, false),
				HideMode.SlideUp => Animations.HideSlide(RectTransform, UnscaledTime, false, +1f, HideAnimationCurve, false),
				HideMode.SlideDown => Animations.HideSlide(RectTransform, UnscaledTime, false, -1f, HideAnimationCurve, false),
				_ => throw new NotImplementedException("Unknown animation: " + EnumHelper<HideMode>.Instance.ToString(Hide)),
			};

			if (DisableInteractable)
			{
				CanvasGroup.interactable = true;
			}
		}
	}
}