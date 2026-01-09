namespace UIWidgets
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Wrapper for the Cursor class.
	/// Required to support multiple behavior components on the same GameObject (like Resizable and Rotatable).
	/// </summary>
	public static class UICursor
	{
		/// <summary>
		/// Cursors.
		/// </summary>
		public static Cursors Cursors
		{
			get => StaticFields.Instance.Cursors;

			set => StaticFields.Instance.Cursors = value;
		}

		/// <summary>
		/// Has cursors.
		/// </summary>
		public static bool HasCursors => Cursors != null;

		/// <summary>
		/// Cursor mode.
		/// </summary>
		public static CursorMode Mode
		{
			get => StaticFields.Instance.CursorMode;
			set => StaticFields.Instance.CursorMode = value;
		}

		/// <summary>
		/// Can the specified component set cursor?
		/// </summary>
		public static Func<Component, bool> CanSet
		{
			get => StaticFields.Instance.CursorCanSet;
			set => StaticFields.Instance.CursorCanSet = value;
		}

		/// <summary>
		/// Set cursor.
		/// </summary>
		public static Action<Component, Cursors.Cursor> Set
		{
			get => StaticFields.Instance.CursorSet;
			set => StaticFields.Instance.CursorSet = value;
		}

		/// <summary>
		/// Reset cursor.
		/// </summary>
		public static Action<Component> Reset
		{
			get => StaticFields.Instance.CursorReset;
			set => StaticFields.Instance.CursorReset = value;
		}

		/// <summary>
		/// Can the specified component set cursor?
		/// </summary>
		/// <param name="owner">Component.</param>
		/// <returns>true if component can set cursor; otherwise false.</returns>
		public static bool DefaultCanSet(Component owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			if (StaticFields.Instance.CursorOwner == null)
			{
				return true;
			}

			return owner == StaticFields.Instance.CursorOwner;
		}

		/// <summary>
		/// Set cursor.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="texture">Cursor texture.</param>
		/// <param name="hotspot">Cursor hot spot.</param>
		[Obsolete("Replaced with Set(Component owner, Cursors.Cursor cursor).")]
		public static void DefaultSet(Component owner, Texture2D texture, Vector2 hotspot)
		{
			if (!CanSet(owner))
			{
				return;
			}

			StaticFields.Instance.CursorOwner = owner;
			SetCursor(new Cursors.Cursor(texture, hotspot));
		}

		/// <summary>
		/// Set cursor.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="cursor">Cursor.</param>
		public static void DefaultSet(Component owner, Cursors.Cursor cursor)
		{
			if (!CanSet(owner))
			{
				return;
			}

			StaticFields.Instance.CursorOwner = owner;
			SetCursor(cursor);
		}

		static void SetCursor(Cursors.Cursor cursor)
		{
			if (StaticFields.Instance.CursorCurrent == cursor)
			{
				return;
			}

			StaticFields.Instance.CursorCurrent = cursor;
			Cursor.SetCursor(cursor.Texture, cursor.Hotspot, Mode);
		}

		/// <summary>
		/// Reset cursor.
		/// </summary>
		/// <param name="owner">Owner.</param>
		public static void DefaultReset(Component owner)
		{
			if (!CanSet(owner))
			{
				return;
			}

			StaticFields.Instance.CursorOwner = null;
			SetCursor(Cursors != null ? Cursors.Default : default);
		}

		/// <summary>
		/// Show obsolete warning,
		/// </summary>
		public static void ObsoleteWarning()
		{
			Debug.LogWarning("Cursors texture and hot spot fields are obsolete and replaced with UICursorSettings component. Set DefaultCursorTexture to null to disable warning or reset component.");
		}
	}
}