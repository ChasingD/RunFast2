namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Track data.
	/// </summary>
	[Serializable]
	public class TrackData : ObservableDataWithPropertyChanged, ITrackData<DateTime>
	{
		[SerializeField]
		DateTime startPoint;

		/// <summary>
		/// Start point.
		/// </summary>
		public DateTime StartPoint
		{
			get => startPoint;

			set => Change(ref startPoint, value);
		}

		[SerializeField]
		DateTime endPoint;

		/// <summary>
		/// End point.
		/// </summary>
		public DateTime EndPoint
		{
			get => endPoint;

			set => Change(ref endPoint, value);
		}

		[SerializeField]
		[HideInInspector]
		int order;

		/// <summary>
		/// Order.
		/// </summary>
		public int Order
		{
			get => order;

			set
			{
				if (Change(ref order, value))
				{
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

		[SerializeField]
		string description;

		/// <summary>
		/// Description.
		/// </summary>
		public string Description
		{
			get => description;

			set => Change(ref description, value);
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
		public DateTime EndPointByStartPoint(DateTime newStart)
		{
			return newStart + (EndPoint - StartPoint);
		}

		/// <summary>
		/// Set StartPoint and EndPoint to maintain same length.
		/// </summary>
		/// <param name="newStart">New start point.</param>
		/// <param name="newEnd">New end point.</param>
		public void SetPoints(DateTime newStart, DateTime newEnd)
		{
			StartPoint = newStart;
			EndPoint = endPoint;
		}

		/// <summary>
		/// Change StartPoint and EndPoint.
		/// </summary>
		/// <param name="newStart">New StartPoint.</param>
		/// <param name="newEnd">New EndPoint.</param>
		public void ChangePoints(DateTime newStart, DateTime newEnd)
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
		public void CopyTo(TrackData target)
		{
			target.StartPoint = StartPoint;
			target.EndPoint = EndPoint;
			target.Name = Name;
			target.Description = Description;
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String representation.</returns>
		public override string ToString() => Name;
	}
}