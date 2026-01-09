namespace UIWidgets
{
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// Base class for StackView animations.
	/// </summary>
	public abstract class StackViewAnimationsBase : MonoBehaviourInitiable
	{
		/// <summary>
		/// Animation for new view when this view pushed into stack.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator PushNew(RectTransform view);

		/// <summary>
		/// Animation for current (previous) view when a new view pushed into stack.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator PushCurrent(RectTransform view);

		/// <summary>
		/// Animation for current (previous) view when this view popped from stack.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator PopCurrent(RectTransform view);

		/// <summary>
		/// Animation for a new current view when other views popped from stack.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator PopPrevious(RectTransform view);

		/// <summary>
		/// Animation for new view when this view replace current.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator ReplaceNew(RectTransform view);

		/// <summary>
		/// Animation for current view when a new view replace this view.
		/// </summary>
		/// <param name="view">View.</param>
		/// <returns>Animation coroutine.</returns>
		public abstract IEnumerator ReplaceCurrent(RectTransform view);
	}
}