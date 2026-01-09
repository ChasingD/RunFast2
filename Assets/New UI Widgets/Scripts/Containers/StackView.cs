namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Stack view.
	/// </summary>
	[RequireComponent(typeof(StackViewAnimationsBase))]
	public class StackView : MonoBehaviourInitiable
    {
		/// <summary>
		/// Action after animation ended.
		/// </summary>
		[Flags]
		protected enum AnimationAction
		{
			/// <summary>
			/// None.
			/// </summary>
			None = 0,

			/// <summary>
			/// Add.
			/// </summary>
			Add = 1,

			/// <summary>
			/// Remove.
			/// </summary>
			Remove = 2,

			/// <summary>
			/// Invoke event.
			/// </summary>
			InvokeEvent = 4,
		}

		/// <summary>
		/// Animation info.
		/// </summary>
		protected class AnimationInfo
		{
			/// <summary>
			/// Owner.
			/// </summary>
			public StackView Owner;

			/// <summary>
			/// Animation.
			/// </summary>
			public IEnumerator Animation
			{
				get;
				protected set;
			}

			/// <summary>
			/// Target.
			/// </summary>
			public RectTransform Target
			{
				get;
				protected set;
			}

			/// <summary>
			/// Action after animation ended.
			/// </summary>
			public AnimationAction Action
			{
				get;
				protected set;
			}

			/// <summary>
			/// Is animation running.
			/// </summary>
			public bool IsRunning => Animation != null;

			/// <summary>
			/// Set values.
			/// </summary>
			/// <param name="target">Target.</param>
			/// <param name="animation">Animation.</param>
			/// <param name="action">Action after animation ended.</param>
			public void Set(
				RectTransform target,
				Func<RectTransform, IEnumerator> animation,
				AnimationAction action = AnimationAction.None)
			{
				Stop();

				Target = target;
				Action = action;
				Animation = null;

				Run(animation);
			}

			void Run(Func<RectTransform, IEnumerator> animation)
			{
				if ((Target == null) || (animation == null))
				{
					return;
				}

				Animation = Animate(animation);
				Owner.StartCoroutine(Animation);
			}

			IEnumerator Animate(Func<RectTransform, IEnumerator> animation)
			{
				yield return animation(Target);
				Stop();
			}

			/// <summary>
			/// Stop animation.
			/// </summary>
			public void Stop()
			{
				if (!IsRunning)
				{
					return;
				}

				Owner.StopCoroutine(Animation);
				Animation = null;

				Owner.SetSizePosition(Target);

				if (EnumHelper<AnimationAction>.Instance.HasFlag(Action, AnimationAction.Add))
				{
					Owner.Views.Add(Target);
				}
				else if (EnumHelper<AnimationAction>.Instance.HasFlag(Action, AnimationAction.Remove))
				{
					Target.gameObject.SetActive(false);
					Owner.Views.Remove(Target);
				}

				if (EnumHelper<AnimationAction>.Instance.HasFlag(Action, AnimationAction.InvokeEvent))
				{
					Owner.OnAfterCurrentChanged.Invoke(Target);
				}
			}
		}

		/// <summary>
		/// View changed event.
		/// </summary>
		[Serializable]
		public class ViewChanged : UnityEvent<RectTransform>
		{
		}

		/// <summary>
		/// Views.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<RectTransform> Views = new List<RectTransform>();

		/// <summary>
		/// Event raised before current changed (before animation started).
		/// </summary>
		[SerializeField]
		public ViewChanged OnBeforeCurrentChanged = new ViewChanged();

		/// <summary>
		/// Event raised after current changed (after animation ended)
		/// </summary>
		[SerializeField]
		public ViewChanged OnAfterCurrentChanged = new ViewChanged();

		/// <summary>
		/// Current view.
		/// </summary>
		public RectTransform Current => Count > 0 ? Views[Views.Count - 1] : null;

		/// <summary>
		/// Previous view.
		/// </summary>
		protected RectTransform Previous => Count > 1 ? Views[Views.Count - 2] : null;

		/// <summary>
		/// Count.
		/// </summary>
		public int Count => Views.Count;

		/// <summary>
		/// Is animation running?
		/// </summary>
		public bool RunningAnimation => AnimationNew.IsRunning || AnimationCurrent.IsRunning;

		StackViewAnimationsBase animations;

		/// <summary>
		/// Animations.
		/// </summary>
		protected virtual StackViewAnimationsBase Animations
		{
			get
			{
				if (animations == null)
				{
					TryGetComponent(out animations);
				}

				return animations;
			}
		}

		/// <summary>
		/// Animation for the new view.
		/// </summary>
		protected AnimationInfo AnimationNew = new AnimationInfo();

		/// <summary>
		/// Animation for the current view.
		/// </summary>
		protected AnimationInfo AnimationCurrent = new AnimationInfo();

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();
			AnimationNew.Owner = this;
			AnimationCurrent.Owner = this;

			for (int i = 0; i < transform.childCount; i++)
			{
				var view = transform.GetChild(i) as RectTransform;
				var go = view.gameObject;
				if (go.activeSelf)
				{
					SetSizePosition(view);
					Views.Add(view);
				}
			}
		}

		/// <summary>
		/// Process the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			foreach (var view in Views)
			{
				SetSizePosition(view);
			}
		}

		/// <summary>
		/// Push view into stack.
		/// If newView is already in stack it will be moved to top.
		/// If newView is current (on top) then nothing will happen.
		/// </summary>
		/// <param name="newView">View.</param>
		public virtual void Push(RectTransform newView)
		{
			Init();

			if (Current == newView)
			{
				return;
			}

			if (Contains(newView))
			{
				Views.Remove(newView);
			}

			var current = Current;
			OnBeforeCurrentChanged.Invoke(newView);
			Prepare(newView);

			AnimationCurrent.Set(current, Animations.PushCurrent, AnimationAction.None);
			AnimationNew.Set(newView, Animations.PushNew, AnimationAction.Add | AnimationAction.InvokeEvent);
		}

		/// <summary>
		/// Pops one or more items off the stack.
		/// Animated only current item.
		/// </summary>
		/// <param name="total">Count of item to pop.</param>
		public virtual void Pop(int total = 1)
		{
			Init();

			total = Mathf.Min(total, Count);
			if (total <= 0)
			{
				return;
			}

			var current = Current;

			AnimationCurrent.Set(current, Animations.PopCurrent, AnimationAction.Remove);

			for (int i = 0; i < total - 1; i++)
			{
				Views[Count - 2].gameObject.SetActive(false);
				Views.RemoveAt(Count - 2);
			}

			var previous = Previous;
			AnimationNew.Set(previous, Animations.PopPrevious, AnimationAction.InvokeEvent);
			OnBeforeCurrentChanged.Invoke(previous);
		}

		/// <summary>
		/// Replace the current view with a specified one.
		/// If newView is already in stack it will be moved to top.
		/// If newView is current (on top) then nothing will happen.
		/// </summary>
		/// <param name="newView">View.</param>
		public virtual void Replace(RectTransform newView)
		{
			Init();

			if (Current == newView)
			{
				return;
			}

			if (Contains(newView))
			{
				Views.Remove(newView);
			}

			var current = Current;
			OnBeforeCurrentChanged.Invoke(newView);
			Prepare(newView);

			AnimationCurrent.Set(current, Animations.ReplaceCurrent, AnimationAction.Remove);
			AnimationNew.Set(newView, Animations.ReplaceNew, AnimationAction.Add | AnimationAction.InvokeEvent);
		}

		/// <summary>
		/// Remove all views from stack.
		/// </summary>
		public virtual void Clear() => Pop(Count);

		/// <summary>
		/// Remove all views except first from stack.
		/// </summary>
		public virtual void FirstOnly() => Pop(Count - 1);

		/// <summary>
		/// Is StackView contains a specified view?
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>true if StackView contains a specified view; otherwise false.</returns>
		public virtual bool Contains(RectTransform view) => Views.Contains(view);

		/// <summary>
		/// Remove view without any animations or event.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="raiseEvents">Raise events.</param>
		/// <returns>true if view was removed; otherwise false.</returns>
		public virtual bool Remove(RectTransform view, bool raiseEvents = true)
		{
			var current_changed = raiseEvents && Current == view;
			if (current_changed)
			{
				OnBeforeCurrentChanged.Invoke(Previous);
			}

			var result = Views.Remove(view);
			if (current_changed && (Count > 0))
			{
				OnAfterCurrentChanged.Invoke(Current);
			}

			return result;
		}

		/// <summary>
		/// Prepare a new view.
		/// </summary>
		/// <param name="newView">View.</param>
		protected virtual void Prepare(RectTransform newView)
		{
			newView.SetParent(transform, false);
			SetSizePosition(newView);
			newView.gameObject.SetActive(true);
			newView.SetAsLastSibling();
		}

		/// <summary>
		/// Set size and position.
		/// </summary>
		/// <param name="view">View.</param>
		protected virtual void SetSizePosition(RectTransform view)
		{
			view.anchorMin = Vector2.zero;
			view.anchorMax = Vector2.one;
			view.sizeDelta = Vector2.zero;
			view.anchoredPosition = Vector2.zero;
		}
	}
}