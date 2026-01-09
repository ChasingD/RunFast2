namespace UIWidgets
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Connector line.
	/// </summary>
	[Serializable]
	public class ConnectorLine : ObservableDataWithPropertyChanged
	{
		[SerializeField]
		RectTransform target;

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>The target.</value>
		public RectTransform Target
		{
			get => target;

			set => Change(ref target, value);
		}

		[SerializeField]
		ConnectorType type;

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		public ConnectorType Type
		{
			get => type;

			set => Change(ref type, value);
		}

		[SerializeField]
		ConnectorPosition start = ConnectorPosition.Right;

		/// <summary>
		/// Gets or sets the start.
		/// </summary>
		/// <value>The start.</value>
		public ConnectorPosition Start
		{
			get => start;

			set => Change(ref start, value);
		}

		[SerializeField]
		ConnectorPosition end = ConnectorPosition.Left;

		/// <summary>
		/// Gets or sets the end.
		/// </summary>
		/// <value>The end.</value>
		public ConnectorPosition End
		{
			get => end;

			set => Change(ref end, value);
		}

		[SerializeField]
		ConnectorArrow arrow = ConnectorArrow.None;

		/// <summary>
		/// Gets or sets the arrow.
		/// </summary>
		/// <value>The arrow.</value>
		public ConnectorArrow Arrow
		{
			get => arrow;

			set => Change(ref arrow, value);
		}

		[SerializeField]
		Vector2 arrowSize = new Vector2(20f, 10f);

		/// <summary>
		/// Gets or sets the arrow.
		/// </summary>
		/// <value>The arrow.</value>
		public Vector2 ArrowSize
		{
			get => arrowSize;

			set => Change(ref arrowSize, value);
		}

		[SerializeField]
		float thickness = 1f;

		/// <summary>
		/// Gets or sets the thickness.
		/// </summary>
		/// <value>The thickness.</value>
		public float Thickness
		{
			get => thickness;

			set => Change(ref thickness, value);
		}

		[SerializeField]
		float margin = 30f;

		/// <summary>
		/// Gets or sets the margin.
		/// </summary>
		/// <value>The margin.</value>
		public float Margin
		{
			get => margin;

			set => Change(ref margin, value);
		}
	}
}