namespace UIWidgets.Examples
{
	using System;
	using UnityEngine;

	/// <summary>
	/// LVInputFields item.
	/// </summary>
	[Serializable]
	public class LVInputFieldsItem : ObservableDataWithPropertyChanged
	{
		[SerializeField]
		string text1;

		/// <summary>
		/// Text1.
		/// </summary>
		public string Text1
		{
			get => text1;

			set => Change(ref text1, value);
		}

		[SerializeField]
		string text2;

		/// <summary>
		/// Text2.
		/// </summary>
		public string Text2
		{
			get => text2;

			set => Change(ref text2, value);
		}

		[SerializeField]
		bool isOn;

		/// <summary>
		/// IsOn.
		/// </summary>
		public bool IsOn
		{
			get => isOn;

			set => Change(ref isOn, value);
		}
	}
}