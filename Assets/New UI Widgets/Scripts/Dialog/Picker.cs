namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for Pickers.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/dialogs/picker.html")]
	public abstract class Picker : MonoBehaviourConditional, ITemplatable, IStylable, INotifyCompletion
	{
		/// <summary>
		/// Animation event.
		/// </summary>
		public class AnimationEvent : UnityEvent<bool>
		{
		}

		bool isTemplate = true;

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
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name of the template.</value>
		public string TemplateName
		{
			get;
			set;
		}

		/// <summary>
		/// Set focus to the last Selectable object in dialog.
		/// </summary>
		[SerializeField]
		[Tooltip("Set focus to the last Selectable object in dialog.")]
		public bool AutoFocus;

		/// <summary>
		/// Close button.
		/// </summary>
		[SerializeField]
		Button closeButton;

		/// <summary>
		/// Close button.
		/// </summary>
		public Button CloseButton
		{
			get => closeButton;

			set
			{
				if (IsInited && (closeButton != null))
				{
					closeButton.onClick.RemoveListener(Cancel);
				}

				closeButton = value;

				if (IsInited && (closeButton != null))
				{
					closeButton.onClick.AddListener(Cancel);
				}
			}
		}

		/// <summary>
		/// Hide on modal click.
		/// </summary>
		[SerializeField]
		public bool HideOnModalClick = false;

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
		/// Opened base pickers.
		/// </summary>
		public static IReadOnlyList<Picker> OpenedBasePickers => WindowInstances<Picker>.Instance.Opened;

		/// <summary>
		/// Event on any instance opened.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnBaseInstanceOpen
		{
			add => WindowInstances<Picker>.Instance.OnOpen += value;

			remove => WindowInstances<Picker>.Instance.OnOpen -= value;
		}

		/// <summary>
		/// Event on any instance closed.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnBaseInstanceClose
		{
			add => WindowInstances<Picker>.Instance.OnClose += value;

			remove => WindowInstances<Picker>.Instance.OnClose -= value;
		}

		/// <summary>
		/// The modal ID.
		/// </summary>
		protected InstanceID? ModalKey;

		/// <summary>
		/// Hierarchy position.
		/// </summary>
		protected HierarchyPosition Position;

		/// <summary>
		/// Canvas resize subscription.
		/// </summary>
		protected Subscription CanvasResize;

		/// <summary>
		/// Callback when picker closed without any value selected.
		/// </summary>
		protected Action OnCancel;

		/// <summary>
		/// Callback when picker closed without any value selected.
		/// </summary>
		public Action OnClose;

		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		protected virtual void Awake()
		{
			if (IsTemplate)
			{
				gameObject.SetActive(false);
			}
		}

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();

			if (closeButton != null)
			{
				closeButton.onClick.AddListener(Cancel);
			}

			AddListeners();
		}

		/// <summary>
		/// Add listeners.
		/// </summary>
		protected virtual void AddListeners()
		{
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected virtual void RemoveListeners()
		{
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
			Position.ParentDestroyed();
			RemoveListeners();
		}

		/// <summary>
		/// Close picker without specified value.
		/// </summary>
		public virtual void Cancel()
		{
			OnCancel?.Invoke();

			Complete();
			Close();
		}

		/// <summary>
		/// Set modal mode.
		/// Warning: modal block is created at the current root canvas.
		/// </summary>
		/// <param name="modalSprite">Modal sprite.</param>
		/// <param name="modalColor">Modal color.</param>
		/// <param name="parentCanvas">Parent canvas.</param>
		public virtual void SetModal(Sprite modalSprite = null, Color? modalColor = null, RectTransform parentCanvas = null)
		{
			ModalHelper.Close(ref ModalKey);
			ModalKey = ModalHelper.Open(this, modalSprite, modalColor, ProcessModalClick, parentCanvas);

			if (Position.Changed)
			{
				transform.SetAsLastSibling();
			}
		}

		/// <summary>
		/// Process modal click.
		/// </summary>
		protected void ProcessModalClick()
		{
			if (HideOnModalClick)
			{
				Cancel();
			}
		}

		/// <summary>
		/// Set canvas.
		/// </summary>
		/// <param name="canvas">Canvas.</param>
		/// <returns>Canvas RectTransform.</returns>
		public virtual RectTransform SetCanvas(Canvas canvas)
		{
			return SetCanvas(canvas != null ? canvas.transform as RectTransform : null);
		}

		/// <summary>
		/// Set canvas.
		/// </summary>
		/// <param name="parent">Parent.</param>
		/// <param name="worldPositionStays">World position stays.</param>
		/// <returns>Canvas RectTransform.</returns>
		public virtual RectTransform SetCanvas(RectTransform parent, bool worldPositionStays = true)
		{
			if (parent == null)
			{
				parent = UtilitiesUI.FindTopmostCanvas(gameObject.transform);
			}

			Position.Restore();
			Position = HierarchyPosition.SetParent(transform, parent, worldPositionStays);

			var resize = Utilities.RequireComponent<ResizeListener>(parent);
			CanvasResize.Clear();
			CanvasResize = new Subscription(resize.OnResize, RefreshPosition);

			return parent;
		}

		/// <summary>
		/// Refresh position.
		/// </summary>
		protected virtual void RefreshPosition() => Position.Refresh();

		/// <summary>
		/// Prepare picker to close.
		/// </summary>
		public virtual void BeforeClose()
		{
		}

		/// <summary>
		/// Close picker.
		/// </summary>
		protected virtual void Close()
		{
			BeforeClose();

			if (!TryRunAnimation(open: false))
			{
				CloseAnimationEnded();
			}
		}

		/// <summary>
		/// Return this instance to cache.
		/// </summary>
		protected abstract void Return();

		/// <inheritdoc/>
		public abstract bool GetStyle(Style style);

		/// <inheritdoc/>
		public abstract bool SetStyle(Style style);

		/// <summary>
		/// Instance opened.
		/// </summary>
		protected virtual void InstanceOpened()
		{
			WindowInstances<Picker>.Instance.Add(this);

			TryRunAnimation(open: true);
		}

		/// <summary>
		/// Instance closed.
		/// </summary>
		protected virtual void InstanceClosed() => WindowInstances<Picker>.Instance.Remove(this);

		#region Animation

		/// <summary>
		/// Current animation.
		/// </summary>
		protected IEnumerator CurrentAnimation;

		/// <summary>
		/// Try to run animation.
		/// </summary>
		/// <param name="open">Run open or close animation or </param>
		/// <returns>true if animation started; otherwise false.</returns>
		protected virtual bool TryRunAnimation(bool open)
		{
			if (!gameObject.activeInHierarchy)
			{
				return false;
			}

			if (CurrentAnimation != null)
			{
				return false;
			}

			if (!TryGetComponent<IWindowAnimations>(out var animation) || !animation.Enabled)
			{
				return false;
			}

			OnAnimationStart.Invoke(open);
			CurrentAnimation = AnimationStart(animation, open);
			StartCoroutine(CurrentAnimation);

			return true;
		}

		/// <summary>
		/// Start animation.
		/// </summary>
		/// <param name="animation">Animation.</param>
		/// <param name="open">true for the open animation; false for the close animation.</param>
		/// <returns>Animation coroutine.</returns>
		protected virtual IEnumerator AnimationStart(IWindowAnimations animation, bool open)
		{
			yield return open ? animation.Open() : animation.Close();

			if (open)
			{
				OpenAnimationEnded();
			}
			else
			{
				CloseAnimationEnded();
			}
		}

		/// <summary>
		/// open animation ended.
		/// </summary>
		protected virtual void OpenAnimationEnded()
		{
			CurrentAnimation = null;
		}

		/// <summary>
		/// Close animation ended.
		/// </summary>
		protected virtual void CloseAnimationEnded()
		{
			OnClose?.Invoke();

			CurrentAnimation = null;

			ModalHelper.Close(ref ModalKey);
			Position.Restore();
			CanvasResize.Clear();

			Return();
		}

		#endregion

		#region async

		/// <summary>
		/// The action to perform when the wait operation completes.
		/// </summary>
		protected Action Continuation;

		/// <summary>
		/// Gets a value that indicates whether the asynchronous task has completed.
		/// </summary>
		public virtual bool IsCompleted
		{
			get;
			protected set;
		}

		/// <summary>
		/// Sets the action to perform when the this object stops waiting for the asynchronous task to complete.
		/// </summary>
		/// <param name="continuation">The action to perform when the wait operation completes.</param>
		public virtual void OnCompleted(Action continuation)
		{
			Continuation = continuation;
		}

		/// <summary>
		/// Complete asynchronous task.
		/// </summary>
		protected void Complete()
		{
			if (Continuation != null)
			{
				IsCompleted = true;

				var c = Continuation;
				Continuation = null;
				c?.Invoke();
			}
		}

		#endregion
	}
}