namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// FileListViewComponentBase.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/collections/filelistview.html")]
	public class FileListViewComponentBase : ListViewItem, IViewData<FileSystemEntry>
	{
		/// <summary>
		/// Name.
		/// </summary>
		[SerializeField]
		protected TextAdapter NameAdapter;

		/// <summary>
		/// Icon.
		/// </summary>
		[SerializeField]
		protected Image Icon;

		/// <summary>
		/// Directory icon.
		/// </summary>
		[SerializeField]
		protected Sprite DirectoryIcon;

		/// <summary>
		/// Creation time.
		/// </summary>
		[SerializeField]
		protected TextAdapter CreationTime;

		/// <summary>
		/// Last write time.
		/// </summary>
		[SerializeField]
		protected TextAdapter LastWriteTime;

		/// <summary>
		/// Last access time.
		/// </summary>
		[SerializeField]
		protected TextAdapter LastAccessTime;

		/// <summary>
		/// DateTime format.
		/// </summary>
		[SerializeField]
		protected string DateTimeFormat = "g";

		/// <summary>
		/// Size.
		/// </summary>
		[SerializeField]
		protected TextAdapter Size;

		/// <summary>
		/// Size format.
		/// </summary>
		[SerializeField]
		protected string SizeFormat = "{0:N} {1}";

		/// <summary>
		/// Init graphics foreground.
		/// </summary>
		protected override void GraphicsForegroundInit()
		{
			if (GraphicsForegroundVersion == 0)
			{
				#pragma warning disable 0618
				Foreground = new Graphic[] { UtilitiesUI.GetGraphic(NameAdapter), };
				#pragma warning restore
				GraphicsForegroundVersion = 1;
			}

			base.GraphicsForegroundInit();
		}

		/// <summary>
		/// Current item.
		/// </summary>
		protected FileSystemEntry Item;

		/// <summary>
		/// Set data.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void SetData(FileSystemEntry item)
		{
			Item = item;

			Icon.sprite = GetIcon(item);
			Icon.enabled = Icon.sprite != null;

			NameAdapter.text = item.DisplayName;

			if (CreationTime != null)
			{
				CreationTime.text = item.CreationTime.ToString(DateTimeFormat);
			}

			if (LastWriteTime != null)
			{
				LastWriteTime.text = item.LastWriteTime.ToString(DateTimeFormat);
			}

			if (LastAccessTime != null)
			{
				LastAccessTime.text = item.LastAccessTime.ToString(DateTimeFormat);
			}

			if (Size != null)
			{
				Size.text = item.ReadableSize(SizeFormat);
			}
		}

		/// <inheritdoc/>
		public override void SetThemeImagesPropertiesOwner(Component owner)
		{
			base.SetThemeImagesPropertiesOwner(owner);

#if UITHEMES_INSTALLED
			UIThemes.Utilities.SetTargetOwner(typeof(Sprite), Icon, nameof(Icon.sprite), owner);
			UIThemes.Utilities.SetTargetOwner(typeof(Color), Icon, nameof(Icon.color), owner);
#endif
		}

		/// <summary>
		/// Get icon for specified FileSystemEntry.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Icon for specified FileSystemEntry.</returns>
		public virtual Sprite GetIcon(FileSystemEntry item) => item.IsDirectory ? DirectoryIcon : null;
	}
}