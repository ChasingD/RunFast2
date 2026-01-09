namespace UIWidgets.Timeline
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Track data.
	/// </summary>
	[Serializable]
	public class TimelineData : ObservableDataWithPropertyChanged, ITrackData<TimeSpan>
	{
		[SerializeField]
		TimeSpan startPoint;

		/// <summary>
		/// Start point.
		/// </summary>
		public TimeSpan StartPoint
		{
			get => startPoint;

			set => Change(ref startPoint, value);
		}

		[SerializeField]
		TimeSpan endPoint;

		/// <summary>
		/// End point.
		/// </summary>
		public TimeSpan EndPoint
		{
			get => endPoint;

			set => Change(ref endPoint, value);
		}

		[SerializeField]
		[HideInInspector]
		int order;

		/// <summary>
		/// Order.
		/// Is not observable property!
		/// </summary>
		public int Order
		{
			get => order;

			set
			{
				if (order != value)
				{
					order = value;
					fixedOrder = false;
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		bool fixedOrder;

		/// <summary>
		/// Is order fixed?
		/// </summary>
		public bool FixedOrder
		{
			get => fixedOrder;

			set => fixedOrder = value;
		}

		[SerializeField]
		string name;

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get => name;

			set => Change(ref name, value);
		}

		object data;

		/// <summary>
		/// Data.
		/// </summary>
		public object Data
		{
			get => data;

			set => Change(ref data, value);
		}

		/// <summary>
		/// Is item dragged?
		/// </summary>
		public bool IsDragged
		{
			get;
			set;
		}

		/// <summary>
		/// Get new EndPoint by specified StartPoint to maintain same length.
		/// </summary>
		/// <param name="newStart">New start point.</param>
		/// <returns>New EndPoint value.</returns>
		public TimeSpan EndPointByStartPoint(TimeSpan newStart) => newStart + (EndPoint - StartPoint);

		/// <summary>
		/// Set StartPoint and EndPoint to maintain same length.
		/// </summary>
		/// <param name="newStart">New start point.</param>
		/// <param name="newEnd">New end point.</param>
		public void SetPoints(TimeSpan newStart, TimeSpan newEnd)
		{
			StartPoint = newStart;
			EndPoint = endPoint;
		}

		/// <summary>
		/// Change StartPoint and EndPoint.
		/// </summary>
		/// <param name="newStart">New StartPoint.</param>
		/// <param name="newEnd">New EndPoint.</param>
		public void ChangePoints(TimeSpan newStart, TimeSpan newEnd)
		{
			var changed = (startPoint != newStart) || (endPoint != newEnd);
			if (changed)
			{
				startPoint = newStart;
				endPoint = newEnd;
				Changed(nameof(StartPoint));
			}
		}

		/// <summary>
		/// Copy data to target.
		/// </summary>
		/// <param name="target">Target.</param>
		public void CopyTo(TimelineData target)
		{
			target.StartPoint = StartPoint;
			target.EndPoint = EndPoint;
			target.Name = Name;
			target.Data = Data;
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String representation.</returns>
		public override string ToString() => Name;
	}
}