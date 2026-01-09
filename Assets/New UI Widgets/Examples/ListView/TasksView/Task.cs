namespace UIWidgets.Examples.Tasks
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Task.
	/// </summary>
	[Serializable]
	public class Task : IEquatable<Task>
	{
		/// <summary>
		/// Name.
		/// </summary>
		public string Name;

		[SerializeField]
		[FormerlySerializedAs("Progress")]
		int progress;

		/// <summary>
		/// Progress.
		/// </summary>
		public int Progress
		{
			get => progress;

			set
			{
				progress = value;
				OnProgressChange?.Invoke();
			}
		}

		/// <summary>
		/// Progress changed event.
		/// </summary>
		public event OnChange OnProgressChange;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(Task other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name && progress == other.progress;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is Task item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(Task a, Task b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(Task a, Task b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(Name, Progress);
	}
}