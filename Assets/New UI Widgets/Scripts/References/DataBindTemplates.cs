namespace UIWidgets
{
	using UnityEngine;

	/// <summary>
	/// Scripts templates.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/third-party-support/data-bind.html")]
	public class DataBindTemplates : ScriptableObject
	{
#if UNITY_EDITOR
		/// <summary>
		/// Instance.
		/// </summary>
		public static DataBindTemplates Instance => StaticFields.Instance.References.DataBindTemplates;
#endif

		/// <summary>
		/// Setter.
		/// </summary>
		[SerializeField]
		public TextAsset Setter;

		/// <summary>
		/// Provider.
		/// </summary>
		[SerializeField]
		public TextAsset Provider;

		/// <summary>
		/// Observer.
		/// </summary>
		[SerializeField]
		public TextAsset Observer;

		/// <summary>
		/// Synchronizer.
		/// </summary>
		[SerializeField]
		public TextAsset Synchronizer;
	}
}