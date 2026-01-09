namespace UIWidgets.Examples
{
	using System;

	/// <summary>
	/// GroupedListItem interface.
	/// </summary>
	public interface IGroupedListItem : IEquatable<IGroupedListItem>
	{
		/// <summary>
		/// Name.
		/// </summary>
		string Name
		{
			get;
			set;
		}
	}
}