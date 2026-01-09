namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Test safe area.
	/// </summary>
	public class TestSafeArea : MonoBehaviour
	{
		[SerializeField]
		SafeArea area;

		[SerializeField]
		RectTransform left;

		[SerializeField]
		RectTransform right;

		[SerializeField]
		RectTransform top;

		[SerializeField]
		RectTransform bottom;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected void Start()
		{
			area.OnScreenChange.AddListener(UpdateBorders);
			area.Init();
			UpdateBorders(area.GetBorders());
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected void OnDestroy()
		{
			if (area != null)
			{
				area.OnScreenChange.RemoveListener(UpdateBorders);
			}
		}

		void UpdateBorders(SafeArea.Borders borders)
		{
			UpdateBorder(left, borders.Left);
			UpdateBorder(right, borders.Right);
			UpdateBorder(top, borders.Top);
			UpdateBorder(bottom, borders.Bottom);
		}

		void UpdateBorder(RectTransform rt, Rect rect)
		{
			if (rt == null)
			{
				return;
			}

			rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.xMin, rect.width);
			rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, rect.yMin, rect.height);
		}
	}
}