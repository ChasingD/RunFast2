namespace UIWidgets
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Scale mark.
	/// </summary>
	[Serializable]
	public class ScaleMark : ObservableDataWithPropertyChanged
	{
		[SerializeField]
		float step;

		/// <summary>
		/// Step.
		/// </summary>
		public float Step
		{
			get => step;

			set => Change(ref step, value);
		}

		[SerializeField]
		ScaleMarkTemplate template;

		/// <summary>
		/// Template.
		/// </summary>
		public ScaleMarkTemplate Template
		{
			get => template;

			set => Change(ref template, value);
		}
	}
}