namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// FileView.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/collections/filelistview.html")]
	public class FileListView : TileViewCustomSize<FileListViewComponentBase, FileSystemEntry>
	{
		/// <summary>
		/// Sort fields.
		/// </summary>
		public enum SortFields
		{
			/// <summary>
			/// Name.
			/// </summary>
			Name,

			/// <summary>
			/// CreationTime.
			/// </summary>
			CreationTime,

			/// <summary>
			/// LastWriteTime.
			/// </summary>
			LastWriteTime,

			/// <summary>
			/// LastAccessTime.
			/// </summary>
			LastAccessTime,

			/// <summary>
			/// Size.
			/// </summary>
			Size,
		}

		/// <summary>
		/// Current directory.
		/// </summary>
		[SerializeField]
		protected string currentDirectory;

		/// <summary>
		/// Current directory.
		/// </summary>
		public string CurrentDirectory
		{
			get => currentDirectory;

			set => SetCurrentDirectory(value);
		}

		/// <summary>
		/// Directory patterns.
		/// </summary>
		[SerializeField]
		protected string directoryPatterns = string.Empty;

		/// <summary>
		/// Gets or sets the directory patterns, semicolon used as separator.
		/// Directory will be displayed if it's match one of the pattern.
		/// Wild-cards:
		/// * - Zero or more characters in that position.
		/// ? - Zero or one character in that position.
		/// Warning: if directory match two or more patterns it will be displayed two or more times.
		/// </summary>
		/// <value>The directory patterns.</value>
		public string DirectoryPatterns
		{
			get => directoryPatterns;

			set => directoryPatterns = value;
		}

		/// <summary>
		/// File patterns.
		/// </summary>
		[SerializeField]
		protected string filePatterns = string.Empty;

		/// <summary>
		/// Gets or sets the file patterns, semicolon used as separator between patterns.
		/// File will be displayed if it's match one of the pattern.
		/// Wild-cards:
		/// * - Zero or more characters in that position.
		/// ? - Zero or one character in that position.
		/// Warning: if file match two or more patterns it will be displayed two or more times.
		/// </summary>
		/// <value>The files patterns.</value>
		public string FilePatterns
		{
			get => filePatterns;

			set => filePatterns = value;
		}

		/// <summary>
		/// Button Up.
		/// Open parent directory.
		/// </summary>
		[SerializeField]
		protected Button ButtonUp;

		/// <summary>
		/// Button to toggle DriversList.
		/// </summary>
		[SerializeField]
		protected Button ButtonToggleDrivers;

		/// <summary>
		/// FileListViewPath.
		/// Display path.
		/// </summary>
		[SerializeField]
		protected FileListViewPath PathView;

		/// <summary>
		/// DrivesListView.
		/// </summary>
		[SerializeField]
		protected DrivesListView DrivesListView;

		/// <summary>
		/// Display IO errors.
		/// </summary>
		[SerializeField]
		public IOExceptionsView ExceptionsView;

		/// <summary>
		/// Can display file system entry?
		/// </summary>
		public Func<FileSystemEntry, bool> CanDisplayEntry = DisplayAll;

		/// <summary>
		/// Default comparison.
		/// </summary>
		/// <param name="x">First FileSystemEntry.</param>
		/// <param name="y">Seconds FileSystemEntry.</param>
		/// <returns>Result of the comparison.</returns>
		protected virtual int ComparisonDefault(FileSystemEntry x, FileSystemEntry y)
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.DisplayName, y.DisplayName)
				: x.IsFile.CompareTo(y.IsFile);
		}

		/// <summary>
		/// Sort comparers.
		/// </summary>
		protected Dictionary<int, Comparison<FileSystemEntry>> SortComparers;

		/// <summary>
		/// Sort comparers.
		/// </summary>
		protected Dictionary<int, Comparison<FileSystemEntry>> ReverseSortComparers;

		/// <summary>
		/// Current sort field.
		/// </summary>
		protected SortFields CurrentSortField = SortFields.Name;

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			SortComparers = new Dictionary<int, Comparison<FileSystemEntry>>()
			{
				{ (int)SortFields.Name, StaticFields.Instance.NameComparer },
				{ (int)SortFields.CreationTime, StaticFields.Instance.CreationTimeComparer },
				{ (int)SortFields.LastWriteTime, StaticFields.Instance.LastWriteTimeComparer },
				{ (int)SortFields.LastAccessTime, StaticFields.Instance.LastAccessTimeComparer },
				{ (int)SortFields.Size, StaticFields.Instance.SizeComparer },
			};
			ReverseSortComparers = new Dictionary<int, Comparison<FileSystemEntry>>()
			{
				{ (int)SortFields.Name, StaticFields.Instance.ReverseNameComparer },
				{ (int)SortFields.CreationTime, StaticFields.Instance.ReverseCreationTimeComparer },
				{ (int)SortFields.LastWriteTime, StaticFields.Instance.ReverseLastWriteTimeComparer },
				{ (int)SortFields.LastAccessTime, StaticFields.Instance.ReverseLastAccessTimeComparer },
				{ (int)SortFields.Size, StaticFields.Instance.ReverseSizeComparer },
			};

			base.InitOnce();

			DataSource.Comparison = SortComparers[(int)CurrentSortField];

			SetCurrentDirectory(currentDirectory);

			if (ButtonUp != null)
			{
				ButtonUp.onClick.AddListener(Up);
			}

			if (ButtonToggleDrivers != null)
			{
				ButtonToggleDrivers.onClick.AddListener(DrivesListView.Toggle);
			}

			if (DrivesListView != null)
			{
				DrivesListView.OnSelectInternal.AddListener(ChangeDrive);
				DrivesListView.FileListView = this;
				DrivesListView.gameObject.SetActive(false);
			}

			InstancesEventsInternal.DoubleClick.AddListener(ProcessDoubleClick);
		}

		/// <summary>
		/// Remove listeners.
		/// </summary>
		protected override void OnDestroy()
		{
			if (ButtonUp != null)
			{
				ButtonUp.onClick.RemoveListener(Up);
			}

			if (DrivesListView != null)
			{
				DrivesListView.OnSelectInternal.RemoveListener(ChangeDrive);
			}

			InstancesEventsInternal.DoubleClick.RemoveListener(ProcessDoubleClick);

			base.OnDestroy();
		}

		/// <summary>
		/// Toggle sort.
		/// </summary>
		/// <param name="field">Sort field.</param>
		public void ToggleSort(SortFields field)
		{
			if (field == CurrentSortField)
			{
				DataSource.Comparison = DataSource.Comparison == SortComparers[(int)field]
					? ReverseSortComparers[(int)field]
					: SortComparers[(int)field];
			}
			else if (SortComparers.ContainsKey((int)field))
			{
				CurrentSortField = field;

				DataSource.Comparison = SortComparers[(int)field];
			}
		}

		/// <summary>
		/// Callback when drive changed.
		/// </summary>
		/// <param name="index">Drive index.</param>
		protected virtual void ChangeDrive(int index)
		{
			CurrentDirectory = DrivesListView.DataSource[index].FullName;
			DrivesListView.Close();
		}

		/// <summary>
		/// Open parent directory.
		/// </summary>
		public virtual void Up()
		{
			var current = CurrentDirectory;
			var directory = Path.GetDirectoryName(current);
			if (!string.IsNullOrEmpty(directory))
			{
				CurrentDirectory = directory;
				Select(FullName2Index(current));
			}
		}

		/// <summary>
		/// Set current directory.
		/// </summary>
		/// <param name="directory">New directory.</param>
		protected virtual void SetCurrentDirectory(string directory)
		{
			currentDirectory = Path.GetFullPath(string.IsNullOrEmpty(directory) ? Application.persistentDataPath : directory);

			if (ButtonUp != null)
			{
				ButtonUp.gameObject.SetActive(!string.IsNullOrEmpty(Path.GetDirectoryName(CurrentDirectory)));
			}

			if (PathView != null)
			{
				PathView.FileView = this;
				PathView.Path = currentDirectory;
			}

			using var _ = DataSource.BeginUpdate();
			DataSource.Clear();
			ExceptionsView.Execute(GetFiles);
		}

		/// <summary>
		/// Get files.
		/// </summary>
		protected virtual void GetFiles()
		{
			if (!string.IsNullOrEmpty(directoryPatterns))
			{
				var patterns = directoryPatterns.Split(StaticFields.Instance.FileMaskSeparators);
				for (int i = 0; i < patterns.Length; i++)
				{
					var dirs = Directory.GetDirectories(currentDirectory, patterns[i]);
					foreach (var dir in dirs)
					{
						AddDirectory(dir);
					}
				}
			}
			else
			{
				var dirs = Directory.GetDirectories(currentDirectory);
				foreach (var dir in dirs)
				{
					AddDirectory(dir);
				}
			}

			if (!string.IsNullOrEmpty(filePatterns))
			{
				var patterns = filePatterns.Split(StaticFields.Instance.FileMaskSeparators);
				for (int i = 0; i < patterns.Length; i++)
				{
					var files = Directory.GetFiles(currentDirectory, patterns[i]);
					foreach (var file in files)
					{
						AddFile(file);
					}
				}
			}
			else
			{
				var files = Directory.GetFiles(currentDirectory);
				foreach (var file in files)
				{
					AddFile(file);
				}
			}
		}

		/// <summary>
		/// Add directory to DataSource.
		/// </summary>
		/// <param name="directory">Directory.</param>
		protected virtual void AddDirectory(string directory)
		{
			var item = new FileSystemEntry(directory, Path.GetFileName(directory), false);
			if (CanDisplayEntry(item))
			{
				DataSource.Add(item);
			}
		}

		/// <summary>
		/// Add files DataSource.
		/// </summary>
		/// <param name="file">File.</param>
		protected virtual void AddFile(string file)
		{
			var item = new FileSystemEntry(file, Path.GetFileName(file), true);
			if (CanDisplayEntry(item))
			{
				DataSource.Add(item);
			}
		}

		/// <summary>
		/// Get index by full name.
		/// </summary>
		/// <param name="fullName">Full name.</param>
		/// <returns>Index.</returns>
		public int FullName2Index(string fullName)
		{
			for (int index = 0; index < DataSource.Count; index++)
			{
				if (DataSource[index].FullName == fullName)
				{
					return index;
				}
			}

			return -1;
		}

		int doubleClickFrame = -1;

		/// <summary>
		/// Handle double click event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="component">Item.</param>
		/// <param name="eventData">Event data.</param>
		protected virtual void ProcessDoubleClick(int index, ListViewItem component, PointerEventData eventData)
		{
			if (doubleClickFrame == WidgetsTime.Instance.FrameCount)
			{
				return;
			}

			doubleClickFrame = WidgetsTime.Instance.FrameCount;

			var item = DataSource[index];
			if (item.IsDirectory && (CurrentDirectory != item.FullName))
			{
				CurrentDirectory = item.FullName;
			}
		}

		#region used in Button.OnClick()

		/// <summary>
		/// Sort by Name.
		/// </summary>
		public void SortByName() => ToggleSort(SortFields.Name);

		/// <summary>
		/// Sort by CreationTime.
		/// </summary>
		public void SortByCreationTime() => ToggleSort(SortFields.CreationTime);

		/// <summary>
		/// Sort by LastWriteTime.
		/// </summary>
		public void SortByLastWriteTime() => ToggleSort(SortFields.LastWriteTime);

		/// <summary>
		/// Sort by LastAccessTime.
		/// </summary>
		public void SortByLastAccessTime() => ToggleSort(SortFields.LastAccessTime);

		/// <summary>
		/// Sort by Size.
		/// </summary>
		public void SortBySize() => ToggleSort(SortFields.Size);
		#endregion

		#region Display

		/// <summary>
		/// Display all file system entry.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>true.</returns>
		public static bool DisplayAll(FileSystemEntry item)
		{
			return true;
		}

		/// <summary>
		/// Display only directories.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>true if item is directory; otherwise, false.</returns>
		public static bool DisplayOnlyDirectories(FileSystemEntry item)
		{
			return item.IsDirectory;
		}

		/// <summary>
		/// Display only files.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>true if item is file; otherwise, false.</returns>
		public static bool DisplayOnlyFiles(FileSystemEntry item)
		{
			return item.IsFile;
		}
		#endregion

		#if UITHEMES_INSTALLED
		#region IStylable implementation

		/// <inheritdoc/>
		public override bool SetStyle(Style style)
		{
			if (ExceptionsView != null)
			{
				ExceptionsView.SetStyle(style);
			}

			if ((ButtonUp != null) && ButtonUp.TryGetComponent<Image>(out var btn))
			{
				style.FileListView.ButtonUp.ApplyTo(btn);
			}

			if ((ButtonToggleDrivers != null) && ButtonToggleDrivers.TryGetComponent<Image>(out var drives))
			{
				style.FileListView.ButtonToggle.ApplyTo(drives);
			}

			if (DrivesListView != null)
			{
				DrivesListView.SetStyle(style);
			}

			if (PathView != null)
			{
				PathView.SetStyle(style);
			}

			return base.SetStyle(style);
		}

		/// <inheritdoc/>
		public override bool GetStyle(Style style)
		{
			if (ExceptionsView != null)
			{
				ExceptionsView.GetStyle(style);
			}

			if ((ButtonUp != null) && ButtonUp.TryGetComponent<Image>(out var btn))
			{
				style.FileListView.ButtonUp.GetFrom(btn);
			}

			if ((ButtonToggleDrivers != null) && ButtonToggleDrivers.TryGetComponent<Image>(out var drives))
			{
				style.FileListView.ButtonToggle.GetFrom(drives);
			}

			if (DrivesListView != null)
			{
				DrivesListView.GetStyle(style);
			}

			if (PathView != null)
			{
				PathView.GetStyle(style);
			}

			return base.GetStyle(style);
		}
		#endregion
		#endif
	}
}