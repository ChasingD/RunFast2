#if UNITY_EDITOR
namespace UIWidgets.WidgetGeneration
{
	using UnityEditor;

	/// <summary>
	/// Menu options for the widgets generation.
	/// </summary>
	public static class MenuOptions
	{
		/// <summary>
		/// Generate widget.
		/// </summary>
		[MenuItem("Assets/Create/New UI Widgets/Generate Widgets", false)]
		public static void GenerateWidgets()
		{
			var info = GetClassInfo();
			WidgetsGenerationWindow.Open(Selection.activeObject as MonoScript, info);
		}

		/// <summary>
		/// Can widget be created?
		/// </summary>
		/// <returns>True if widget can be created; otherwise false.</returns>
		[MenuItem("Assets/Create/New UI Widgets/Generate Widgets", true)]
		public static bool CanGenerateWidgets()
		{
			if (Selection.activeObject == null)
			{
				return false;
			}

			var script = Selection.activeObject as MonoScript;
			if (script == null)
			{
				return false;
			}

			return true;
		}

		static ClassInfo GetClassInfo()
		{
			if (Selection.activeObject == null)
			{
				return null;
			}

			var script = Selection.activeObject as MonoScript;
			if (script == null)
			{
				return null;
			}

			return new ClassInfo(script);
		}
	}
}
#endif