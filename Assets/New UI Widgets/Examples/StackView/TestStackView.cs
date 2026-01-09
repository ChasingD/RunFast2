namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Test StackView.
	/// </summary>
	public class TestStackView : MonoBehaviour
	{
		/// <summary>
		/// StackView.
		/// </summary>
		[SerializeField]
		protected StackView StackView;

		/// <summary>
		/// Template.
		/// </summary>
		[SerializeField]
		protected RectTransform Template;

		int index;

		/// <summary>
		/// Push.
		/// </summary>
		public void Push() => StackView.Push(Create());

		/// <summary>
		/// Replace.
		/// </summary>
		public void Replace() => StackView.Replace(Create());

		/// <summary>
		/// Pop.
		/// </summary>
		/// <param name="count">Count.</param>
		public void Pop(int count) => StackView.Pop(count);

		/// <summary>
		/// Clear.
		/// </summary>
		public void Clear() => StackView.Clear();

		RectTransform Create()
		{
			var instance = Instantiate(Template);
			var name = "View #" + index;
			instance.name = name;
			var text = instance.GetComponentInChildren<TextAdapter>(includeInactive: true);
			if (text != null)
			{
				text.text = name;
			}

			index++;

			if (instance.TryGetComponent<Graphic>(out var graphic))
			{
				graphic.color = new Color(Random.Range(0f, 1f), 0.5f, Random.Range(0f, 1f), 1f);
			}

			return instance;
		}
	}
}