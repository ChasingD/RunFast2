namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// SteamSpy item.
	/// </summary>
	[Serializable]
	public class SteamSpyItem : IEquatable<SteamSpyItem>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		public string Name;

		/// <summary>
		/// ScoreRank.
		/// </summary>
		[SerializeField]
		public int ScoreRank;

		/// <summary>
		/// Owners.
		/// </summary>
		[SerializeField]
		public int Owners;

		/// <summary>
		/// OwnersVariance.
		/// </summary>
		[SerializeField]
		public int OwnersVariance;

		/// <summary>
		/// Players.
		/// </summary>
		[SerializeField]
		public int Players;

		/// <summary>
		/// PlayersVariance.
		/// </summary>
		[SerializeField]
		public int PlayersVariance;

		/// <summary>
		/// PlayersIn2Week.
		/// </summary>
		[SerializeField]
		public int PlayersIn2Week;

		/// <summary>
		/// PlayersIn2WeekVariance.
		/// </summary>
		[SerializeField]
		public int PlayersIn2WeekVariance;

		/// <summary>
		/// AverageTimeIn2Weeks.
		/// </summary>
		[SerializeField]
		public int AverageTimeIn2Weeks;

		/// <summary>
		/// MedianTimeIn2Weeks.
		/// </summary>
		[SerializeField]
		public int MedianTimeIn2Weeks;

		/// <inheritdoc/>
		public override string ToString() => Name;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(SteamSpyItem other)
		{
			if (other == null)
			{
				return false;
			}

			return Name == other.Name
				&& ScoreRank == other.ScoreRank
				&& Owners == other.Owners
				&& OwnersVariance == other.OwnersVariance
				&& Players == other.Players
				&& PlayersVariance == other.PlayersVariance
				&& PlayersIn2Week == other.PlayersIn2Week
				&& PlayersIn2WeekVariance == other.PlayersIn2WeekVariance
				&& AverageTimeIn2Weeks == other.AverageTimeIn2Weeks
				&& MedianTimeIn2Weeks == other.MedianTimeIn2Weeks;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is SteamSpyItem item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(SteamSpyItem a, SteamSpyItem b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(SteamSpyItem a, SteamSpyItem b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			var hash = HashCode.Combine(Name, ScoreRank, Owners, OwnersVariance, Players, PlayersVariance, PlayersIn2Week, PlayersIn2WeekVariance);
			return HashCode.Combine(hash, AverageTimeIn2Weeks, MedianTimeIn2Weeks);
		}
	}
}