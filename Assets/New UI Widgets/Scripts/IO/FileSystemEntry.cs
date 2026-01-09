namespace UIWidgets
{
	using System;
	using System.IO;

	/// <summary>
	/// FileSystem entry.
	/// </summary>
	public class FileSystemEntry : IEquatable<FileSystemEntry>
	{
		/// <summary>
		/// Full name.
		/// </summary>
		public string FullName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Name to display.
		/// </summary>
		public string DisplayName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Is entry is directory?
		/// </summary>
		public bool IsDirectory => !IsFile;

		/// <summary>
		/// is entry is file?
		/// </summary>
		public bool IsFile
		{
			get;
			protected set;
		}

		/// <summary>
		/// Creation time.
		/// </summary>
		public DateTime CreationTime
		{
			get;
			protected set;
		}

		/// <summary>
		/// Last write time.
		/// </summary>
		public DateTime LastWriteTime
		{
			get;
			protected set;
		}

		/// <summary>
		/// Last access time.
		/// </summary>
		public DateTime LastAccessTime
		{
			get;
			protected set;
		}

		/// <summary>
		/// Size.
		/// </summary>
		public long Size
		{
			get;
			protected set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemEntry"/> class.
		/// </summary>
		/// <param name="fullName">Full name.</param>
		/// <param name="displayName">Name to display.</param>
		/// <param name="isFile">Entry is file.</param>
		public FileSystemEntry(string fullName, string displayName, bool isFile)
		{
			FullName = fullName;
			DisplayName = displayName;
			IsFile = isFile;

			CreationTime = File.GetCreationTime(FullName);
			LastWriteTime = File.GetLastWriteTime(FullName);
			LastAccessTime = File.GetLastAccessTime(FullName);
			Size = isFile ? new FileInfo(FullName).Length : 0;
		}

		/// <summary>
		/// Get readable size.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <returns>Readable size.</returns>
		public string ReadableSize(string format)
		{
			if (IsDirectory)
			{
				return string.Empty;
			}

			var tb = Math.Pow(1024, 4);
			if (Size >= tb)
			{
				return string.Format(format, Size / tb, "Tb");
			}

			var gb = Math.Pow(1024, 3);
			if (Size >= gb)
			{
				return string.Format(format, Size / gb, "Gb");
			}

			var mb = Math.Pow(1024, 2);
			if (Size >= mb)
			{
				return string.Format(format, Size / mb, "Mb");
			}

			var kb = Math.Pow(1024, 1);
			if (Size >= kb)
			{
				return string.Format(format, Size / kb, "Kb");
			}

			return string.Format(format, Size / kb, "b");
		}

		/// <summary>
		/// Convert instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString() => FullName;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(FileSystemEntry other)
		{
			if (other == null)
			{
				return false;
			}

			return FullName == other.FullName
				&& DisplayName == other.DisplayName
				&& IsFile == other.IsFile
				&& CreationTime == other.CreationTime
				&& LastWriteTime == other.LastWriteTime
				&& LastAccessTime == other.LastAccessTime
				&& Size == other.Size;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is FileSystemEntry item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(FileSystemEntry a, FileSystemEntry b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(FileSystemEntry a, FileSystemEntry b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(FullName, DisplayName, IsFile, CreationTime, LastWriteTime, LastAccessTime, Size);
	}
}