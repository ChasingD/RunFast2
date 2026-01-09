namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UIWidgets.l10n;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Base class for notifications.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/dialogs/notify.html")]
	public abstract class NotificationBase : MonoBehaviourInitiable, ITemplatable, IHideable, ILocalizationSupport
	{
		/// <summary>
		/// Animation event.
		/// </summary>
		public class AnimationEvent : UnityEvent<bool>
		{
		}

		[SerializeField]
		bool unscaledTime;

		/// <summary>
		/// Use unscaled time.
		/// </summary>
		public bool UnscaledTime
		{
			get => unscaledTime;

			protected set => unscaledTime = value;
		}

		bool isTemplate = true;

		[SerializeField]
		[Tooltip("If enabled translates buttons labels using Localization.GetTranslation().")]
		bool localizationSupport = true;

		/// <summary>
		/// Localization support.
		/// </summary>
		public bool LocalizationSupport
		{
			get => localizationSupport;

			set => localizationSupport = value;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is template.
		/// </summary>
		/// <value><c>true</c> if this instance is template; otherwise, <c>false</c>.</value>
		public bool IsTemplate
		{
			get => isTemplate;

			set => isTemplate = value;
		}

		/// <summary>
		/// Template name.
		/// </summary>
		protected string NotificationTemplateName;

		/// <summary>
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name of the template.</value>
		public virtual string TemplateName
		{
			get => NotificationTemplateName;

			set => NotificationTemplateName = value;
		}

		/// <summary>
		/// Time between previous notification was hidden and next will be showed.
		/// </summary>
		public float SequenceDelay;

		/// <summary>
		/// Gets the notification manager.
		/// </summary>
		/// <value>The notification manager.</value>
		public static NotifySequenceManager NotifyManager => StaticFields.Instance.NotificationManager;

		RectTransform rectTransform;

		/// <summary>
		/// RectTransform.
		/// </summary>
		public RectTransform RectTransform
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = transform as RectTransform;
				}

				return rectTransform;
			}
		}

		/// <summary>
		/// Is instance destroyed?
		/// </summary>
		public bool IsDestroyed
		{
			get;
			protected set;
		}

		/// <summary>
		/// Event before animation start.
		/// </summary>
		[SerializeField]
		public AnimationEvent OnAnimationStart = new AnimationEvent();

		/// <summary>
		/// Opened base notifications.
		/// </summary>
		public static IReadOnlyList<NotificationBase> OpenedBaseNotifications => WindowInstances<NotificationBase>.Instance.Opened;

		/// <summary>
		/// Event on any instance opened.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnBaseInstanceOpen
		{
			add => WindowInstances<NotificationBase>.Instance.OnOpen += value;

			remove => WindowInstances<NotificationBase>.Instance.OnOpen -= value;
		}

		/// <summary>
		/// Event on any instance closed.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnBaseInstanceClose
		{
			add => WindowInstances<NotificationBase>.Instance.OnClose += value;

			remove => WindowInstances<NotificationBase>.Instance.OnClose -= value;
		}

		/// <summary>
		/// Instance opened.
		/// </summary>
		protected virtual void InstanceOpened() => WindowInstances<NotificationBase>.Instance.Add(this);

		/// <summary>
		/// Instance closed.
		/// </summary>
		protected virtual void InstanceClosed() => WindowInstances<NotificationBase>.Instance.Remove(this);

		/// <summary>
		/// Return this instance to cache.
		/// </summary>
		public virtual void Return()
		{
			InstanceClosed();
		}

		/// <summary>
		/// Display notification.
		/// </summary>
		/// <param name="onHideCallback">On hide callback.</param>
		public abstract void Display(Action onHideCallback = null);

		/// <summary>
		/// Clear notifications sequence.
		/// </summary>
		public static void ClearSequence()
		{
			NotifyManager.Clear();
		}

		/// <summary>
		/// Set container (parent game object).
		/// </summary>
		/// <param name="container">Container.</param>
		public virtual void SetContainer(RectTransform container)
		{
			if (container != null)
			{
				transform.SetParent(container, false);
			}
		}

		/// <summary>
		/// Returns replacement slide to cache.
		/// </summary>
		/// <param name="replacement">Replacement.</param>
		[Obsolete("Replaced with NotificationAnimationsList.FreeReplacement().")]
		public static void FreeSlide(RectTransform replacement) => NotificationAnimationsList.FreeReplacement(replacement);

		/// <summary>
		/// Is asynchronous?
		/// </summary>
		public abstract bool IsAsync
		{
			get;
		}

		/// <summary>
		/// Close notification on button click.
		/// </summary>
		public bool CloseOnButtonClick
		{
			get;
			protected set;
		}

		/// <summary>
		/// Hide notification.
		/// </summary>
		public abstract void Hide();

		/// <summary>
		/// Hide notification.
		/// </summary>
		/// <param name="buttonIndex">Button index.</param>
		public abstract void Complete(int buttonIndex);

		#region HideAnimationRotate

		/// <summary>
		/// Vertical rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to AnimationRotateVertical.")]
		public static Func<NotificationBase, IEnumerator> AnimationRotate => StaticFields.Instance.NotificationAnimations.AnimationRotate;

		/// <summary>
		/// Vertical rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationRotateVertical => StaticFields.Instance.NotificationAnimations.AnimationRotateVertical;

		/// <summary>
		/// Horizontal rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationRotateHorizontal => StaticFields.Instance.NotificationAnimations.AnimationRotateHorizontal;

		/// <summary>
		/// Base rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to HideAnimationRotateBase.")]
		public static Func<NotificationBase, bool, float, IEnumerator> AnimationRotateBase => StaticFields.Instance.NotificationAnimations.AnimationRotateBase;

		/// <summary>
		/// Base rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, bool, float, IEnumerator> HideAnimationRotateBase => StaticFields.Instance.NotificationAnimations.HideAnimationRotateBase;

		/// <summary>
		/// Rotate animation to hide notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("AnimationRotate() now supports UnscaledTime.")]
		public static Func<NotificationBase, IEnumerator> AnimationRotateUnscaledTime => StaticFields.Instance.NotificationAnimations.AnimationRotateUnscaledTime;

		#endregion

		#region HideAnimationCollapse

		/// <summary>
		/// Base collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to HideAnimationCollapseBase.")]
		public static Func<NotificationBase, bool, float, IEnumerator> AnimationCollapseBase => StaticFields.Instance.NotificationAnimations.AnimationCollapseBase;

		/// <summary>
		/// Base collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, bool, float, IEnumerator> HideAnimationCollapseBase => StaticFields.Instance.NotificationAnimations.HideAnimationCollapseBase;

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("Renamed to AnimationCollapseVertical.")]
		public static Func<NotificationBase, IEnumerator> AnimationCollapse => StaticFields.Instance.NotificationAnimations.AnimationCollapse;

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationCollapseVertical => StaticFields.Instance.NotificationAnimations.AnimationCollapseVertical;

		/// <summary>
		/// Horizontal collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationCollapseHorizontal => StaticFields.Instance.NotificationAnimations.AnimationCollapseHorizontal;

		/// <summary>
		/// Vertical collapse animation.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		[Obsolete("AnimationCollapse now supports UnscaledTime.")]
		public static Func<NotificationBase, IEnumerator> AnimationCollapseUnscaledTime => StaticFields.Instance.NotificationAnimations.AnimationCollapseUnscaledTime;

		#endregion

		#region HideAnimationSlide

		/// <summary>
		/// Slide animation to hide notification to right.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationSlideRight => StaticFields.Instance.NotificationAnimations.AnimationSlideRight;

		/// <summary>
		/// Slide animation to hide notification to left.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationSlideLeft => StaticFields.Instance.NotificationAnimations.AnimationSlideLeft;

		/// <summary>
		/// Slide animation to hide notification to top.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationSlideUp => StaticFields.Instance.NotificationAnimations.AnimationSlideUp;

		/// <summary>
		/// Slide animation to hide notification to bottom.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> AnimationSlideDown => StaticFields.Instance.NotificationAnimations.AnimationSlideDown;

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
		public static IEnumerator AnimationSlideBase(NotificationBase notification, bool isHorizontal, float direction, float speed, bool animateOthers = true)
		{
			return StaticFields.Instance.NotificationAnimations.HideSlideSpeed(notification.RectTransform, notification.unscaledTime, isHorizontal, direction, speed, animateOthers);
		}

		/// <summary>
		/// Base slide animation.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public static IEnumerator HideAnimationSlideBase(NotificationBase notification, bool isHorizontal, float direction, float speed, bool animateOthers = true)
		{
			return StaticFields.Instance.NotificationAnimations.HideSlideSpeed(notification.RectTransform, notification.unscaledTime, isHorizontal, direction, speed, animateOthers);
		}

		#endregion

		#region ShowAnimationSlide

		/// <summary>
		/// Slide animation to show notification from right.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationSlideRight => StaticFields.Instance.NotificationAnimations.ShowAnimationSlideRight;

		/// <summary>
		/// Slide animation to show notification from left.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationSlideLeft => StaticFields.Instance.NotificationAnimations.ShowAnimationSlideLeft;

		/// <summary>
		/// Slide animation to show notification from top.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationSlideUp => StaticFields.Instance.NotificationAnimations.ShowAnimationSlideUp;

		/// <summary>
		/// Slide animation to show notification from bottom.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationSlideDown => StaticFields.Instance.NotificationAnimations.ShowAnimationSlideDown;

		/// <summary>
		/// Base slide animation to show notification.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="isHorizontal">Is horizontal slide?</param>
		/// <param name="direction">Slide direction.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="animateOthers">Animate other notifications.</param>
		/// <returns>Animation coroutine.</returns>
		public static IEnumerator ShowAnimationSlideBase(NotificationBase notification, bool isHorizontal, float direction, float speed, bool animateOthers = true)
		{
			return StaticFields.Instance.NotificationAnimations.ShowSlideSpeed(notification.RectTransform, notification.UnscaledTime, isHorizontal, direction, speed, animateOthers);
		}

		#endregion

		#region ShowAnimationExplode

		/// <summary>
		/// Base explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, bool, float, IEnumerator> ShowAnimationExplodeBase => StaticFields.Instance.NotificationAnimations.ShowAnimationExplodeBase;

		/// <summary>
		/// Vertical explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationExplodeVertical => StaticFields.Instance.NotificationAnimations.ShowAnimationExplodeVertical;

		/// <summary>
		/// Horizontal explode animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationExplodeHorizontal => StaticFields.Instance.NotificationAnimations.ShowAnimationExplodeHorizontal;

		#endregion

		#region ShowAnimationRotate

		/// <summary>
		/// Vertical rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationRotateVertical => StaticFields.Instance.NotificationAnimations.ShowAnimationRotateVertical;

		/// <summary>
		/// Horizontal rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, IEnumerator> ShowAnimationRotateHorizontal => StaticFields.Instance.NotificationAnimations.ShowAnimationRotateHorizontal;

		/// <summary>
		/// Base rotate animation to show notification.
		/// </summary>
		/// <returns>Animation coroutine.</returns>
		public static Func<NotificationBase, bool, float, IEnumerator> ShowAnimationRotateBase => StaticFields.Instance.NotificationAnimations.ShowAnimationRotateBase;

		#endregion

		#region FadeAnimation

		/// <summary>
		/// Fade in animation.
		/// </summary>
		public static Func<NotificationBase, IEnumerator> FadeIn => StaticFields.Instance.NotificationAnimations.FadeIn;

		/// <summary>
		/// Fade out animation.
		/// </summary>
		public static Func<NotificationBase, IEnumerator> FadeOut => StaticFields.Instance.NotificationAnimations.FadeOut;

		/// <summary>
		/// Fade animation.
		/// </summary>
		/// <param name="notification">Notification.</param>
		/// <param name="animation">Animation curve.</param>
		/// <returns>Animation coroutine.</returns>
		public static IEnumerator FadeBase(NotificationBase notification, AnimationCurve animation)
		{
			return StaticFields.Instance.NotificationAnimations.Fade(notification.RectTransform, notification.UnscaledTime, animation);
		}

		#endregion
	}
}