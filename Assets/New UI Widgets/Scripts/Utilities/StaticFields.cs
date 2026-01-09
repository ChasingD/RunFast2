namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using UIWidgets.Attributes;
	using UIWidgets.l10n;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	#if ENABLE_INPUT_SYSTEM
	using UnityEngine.InputSystem;
	#endif
	using UnityEngine.UI;
	using static UIWidgets.CompatibilityInput;
	using static UIWidgets.SnapGridBase;

	/// <summary>
	/// Static fields.
	/// Class to reduce amount of static fields, because of static fields have negative impact on domain reload time.
	/// </summary>
	public class StaticFields
	{
		#region ModalHelper

		/// <summary>
		/// ModalHelper: instances.
		/// </summary>
		public readonly Dictionary<InstanceID, ModalHelper> ModalHelperInstances = new Dictionary<InstanceID, ModalHelper>();

		/// <summary>
		/// ModalHelper: templates.
		/// </summary>
		public readonly Templates<ModalHelper> ModalHelperTemplates = new Templates<ModalHelper>();

		/// <summary>
		/// ModalHelper: key.
		/// </summary>
		public readonly string ModalHelperKey = "ModalTemplate";

		#endregion

		#region DragSupport

		/// <summary>
		/// DragSupport: points.
		/// </summary>
		public readonly Dictionary<InstanceID, RectTransform> DragSupportPoints = new Dictionary<InstanceID, RectTransform>();

		#endregion

		#region ListViewBaseScrollData

		/// <summary>
		/// ListViewBaseScrollData: Get ScrollRect ContentStartPosition.
		/// </summary>
		public readonly Func<ScrollRect, Vector2> ListViewBaseScrollDataGetContentStartPosition;

		/// <summary>
		/// ListViewBaseScrollData: Set ScrollRect ContentStartPosition.
		/// </summary>
		public readonly Action<ScrollRect, Vector2> ListViewBaseScrollDataSetContentStartPosition;

		#endregion

		#region ListView

		/// <summary>
		/// ListView: Items under pointer.
		/// </summary>
		public readonly ListViewBase.ItemsUnderPointerFinder ListViewItemsUnderPointer = new ListViewBase.ItemsUnderPointerFinder();

		/// <summary>
		/// ListView: items comparison.
		/// </summary>
		public readonly Comparison<ListViewItem> ListViewItemsIndexComparison = (x, y) =>
		{
			if (x.Index == y.Index)
			{
				return 0;
			}

			if (x.Index < 0)
			{
				return 1;
			}

			if (y.Index < 0)
			{
				return -1;
			}

			return x.Index.CompareTo(y.Index);
		};

		/// <summary>
		/// Check if instance is null.
		/// </summary>
		public readonly Predicate<ListViewItem> ListViewItemIsNull = x => x == null;

		/// <summary>
		/// Items comparison by localized names.
		/// </summary>
		public readonly Comparison<ListViewIconsItemDescription> ListViewLocalizedItemsComparison = (x, y) => UtilitiesCompare.Compare(GetLocalizedItemName(x), GetLocalizedItemName(y));

		/// <summary>
		/// Items comparison.
		/// </summary>
		public readonly Comparison<ListViewIconsItemDescription> ListViewItemsComparison = (x, y) => UtilitiesCompare.Compare(GetItemName(x), GetItemName(y));

		static string GetLocalizedItemName(ListViewIconsItemDescription item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			return item.LocalizedName ?? Localization.GetTranslation(item.Name);
		}

		static string GetItemName(ListViewIconsItemDescription item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			return item.Name;
		}

		/// <summary>
		/// Default function to check if index can be selected or deselected.
		/// </summary>
		public readonly Func<int, bool> ListViewAlwaysAllow = index => true;

		#endregion

		#region ContextMenu

		/// <summary>
		/// ContextMenu: Active menu.
		/// </summary>
		public readonly List<UIWidgets.Menu.ContextMenu> ContextMenuActive = new List<UIWidgets.Menu.ContextMenu>();

		/// <summary>
		/// ContextMenu: Menu comparison.
		/// </summary>
		public readonly Comparison<UIWidgets.Menu.ContextMenu> ContextMenuComparison = (x, y) =>
		{
			var x_depth = Utilities.GetDepth(x.transform);
			var y_depth = Utilities.GetDepth(y.transform);

			return -x_depth.CompareTo(y_depth);
		};

		/// <summary>
		/// ContextMenu: Is menu open.
		/// </summary>
		public readonly Predicate<UIWidgets.Menu.ContextMenu> ContextMenuIsOpen = menu => menu.IsActiveMenu() && menu.OpenOnContextMenuKey;

		/// <summary>
		/// ContextMenu: Is default menu.
		/// </summary>
		public readonly Predicate<UIWidgets.Menu.ContextMenu> ContextMenuIsDefault = menu => menu.OpenOnContextMenuKey && menu.gameObject.activeInHierarchy && menu.IsDefault;

		/// <summary>
		/// ContextMenu: Last frame when toggle was called.
		/// </summary>
		public int ContextMenuLastToggledByKey = -2;

		/// <summary>
		/// ContextMenu: Current menu.
		/// </summary>
		public UIWidgets.Menu.ContextMenu ContextMenuCurrent;

#if ENABLE_INPUT_SYSTEM
		/// <summary>
		/// ContextMenu: Input action.
		/// </summary>
		public InputAction ContextMenuToggleAction;
#endif

		#endregion

		#region Cursors
		bool cursorsWarningDisplayed;

		Cursors cursors;

		/// <summary>
		/// Cursor: Cursors.
		/// </summary>
		public Cursors Cursors
		{
			get
			{
				if ((cursors == null) && !cursorsWarningDisplayed)
				{
					Debug.LogWarning("Cursors are not specified. Please specify cursors using the unique CursorsDPISelector component or with a field at component.");
					cursorsWarningDisplayed = true;
				}

				return cursors;
			}

			set => cursors = value;
		}

		/// <summary>
		/// Cursor: Current cursor owner.
		/// </summary>
		public Component CursorOwner;

		/// <summary>
		/// Cursor: Current cursor.
		/// </summary>
		public Cursors.Cursor CursorCurrent;

		/// <summary>
		/// Cursor: Cursor mode.
		/// </summary>
		public CursorMode CursorMode =
#if UNITY_WEBGL
			CursorMode.ForceSoftware;
#else
			CursorMode.Auto;
#endif

		/// <summary>
		/// Cursor: Can the specified component set cursor?
		/// </summary>
		public Func<Component, bool> CursorCanSet = UICursor.DefaultCanSet;

		/// <summary>
		/// Cursor: Set cursor.
		/// </summary>
		public Action<Component, Cursors.Cursor> CursorSet = UICursor.DefaultSet;

		/// <summary>
		/// Cursor: Reset cursor.
		/// </summary>
		public Action<Component> CursorReset = UICursor.DefaultReset;

		#endregion

		#region Notification

		NotifySequenceManager notificationManager;

		/// <summary>
		/// Notification: manager.
		/// </summary>
		public NotifySequenceManager NotificationManager
		{
			get
			{
				if (notificationManager == null)
				{
					var go = new GameObject("NotificationSequenceManager");
					notificationManager = go.AddComponent<NotifySequenceManager>();
				}

				return notificationManager;
			}
		}

		/// <summary>
		/// Notification: Replacements cache.
		/// </summary>
		public readonly Stack<RectTransform> NotificationReplacements = new Stack<RectTransform>();

		/// <summary>
		/// Notification: sequence.
		/// </summary>
		public readonly List<NotificationBase> NotificationSequence = new List<NotificationBase>();

		/// <summary>
		/// Notification: current.
		/// </summary>
		public NotificationBase NotificationCurrent;

		#endregion

		#region Compare

		/// <summary>
		/// Compare: Culture.
		/// </summary>
		public CultureInfo CompareCulture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Compare: Case sensitive compare options.
		/// </summary>
		public CompareOptions CompareOptionsCaseSensitive = CompareOptions.None;

		/// <summary>
		/// Compare: Case ignore compare options.
		/// </summary>
		public CompareOptions CompareOptionsCaseIgnore = CompareOptions.IgnoreCase;

		#endregion

		#region Localization

		/// <summary>
		/// Locale changed event.
		/// </summary>
		public event LocaleChangedEventHandler LocalizationOnChanged = () => { };

		/// <summary>
		/// Translate the specified string.
		/// </summary>
		public Func<string, string> LocalizationTranslate = Localization.NoTranslation;

		/// <summary>
		/// Get country code.
		/// Used to instantiate System.Globalization.CultureInfo.
		/// </summary>
		/// <returns>Country code.</returns>
		public Func<string> LocalizationCountryCode = Localization.DefaultCountryCode;

		/// <summary>
		/// Current culture.
		/// </summary>
		public CultureInfo LocalizationCurrentCulture;

		/// <summary>
		/// Culture.
		/// </summary>
		public Func<string, CultureInfo> LocalizationCulture = Localization.DefaultGetCulture;

		/// <summary>
		/// Localization changed.
		/// </summary>
		public void LocalizationChanged()
		{
			LocalizationCurrentCulture = null;
			LocalizationOnChanged?.Invoke();
		}

		#endregion

		#region Shader

		/// <summary>
		/// Line color ID.
		/// </summary>
		public readonly int ShaderBorderColor = Shader.PropertyToID("_BorderColor");

		/// <summary>
		/// Borders ID.
		/// </summary>
		public readonly int ShaderBorders = Shader.PropertyToID("_Borders");

		/// <summary>
		/// Transparent ID.
		/// </summary>
		public readonly int ShaderTransparent = Shader.PropertyToID("_Transparent");

		/// <summary>
		/// Left color ID.
		/// </summary>
		public readonly int ShaderColorLeft = Shader.PropertyToID("_ColorLeft");

		/// <summary>
		/// Right color ID.
		/// </summary>
		public readonly int ShaderColorRight = Shader.PropertyToID("_ColorRight");

		/// <summary>
		/// Top color ID.
		/// </summary>
		public readonly int ShaderColorTop = Shader.PropertyToID("_ColorTop");

		/// <summary>
		/// Bottom color ID.
		/// </summary>
		public readonly int ShaderColorBottom = Shader.PropertyToID("_ColorBottom");

		/// <summary>
		/// Quality ID.
		/// </summary>
		public readonly int ShaderQuality = Shader.PropertyToID("_Quality");

		/// <summary>
		/// Value ID.
		/// </summary>
		public readonly int ShaderValue = Shader.PropertyToID("_Value");

		/// <summary>
		/// Color.
		/// </summary>
		public readonly int ShaderFlareColor = Shader.PropertyToID("_FlareColor");

		/// <summary>
		/// Size.
		/// </summary>
		public readonly int ShaderFlareSize = Shader.PropertyToID("_FlareSize");

		/// <summary>
		/// Speed.
		/// </summary>
		public readonly int ShaderFlareSpeed = Shader.PropertyToID("_FlareSpeed");

		/// <summary>
		/// Delay.
		/// </summary>
		public readonly int ShaderFlareDelay = Shader.PropertyToID("_FlareDelay");

		/// <summary>
		/// Height delay.
		/// </summary>
		public readonly int ShaderFlareHeightDelay = Shader.PropertyToID("_FlareHeightDelay");

		/// <summary>
		/// Line color ID.
		/// </summary>
		public readonly int ShaderLinesDrawerColor = Shader.PropertyToID("_LineColor");

		/// <summary>
		/// Line thickness ID.
		/// </summary>
		public readonly int ShaderLinesDrawerThickness = Shader.PropertyToID("_LineThickness");

		/// <summary>
		/// Resolution X ID.
		/// </summary>
		public readonly int ShaderLinesDrawerResolutionX = Shader.PropertyToID("_ResolutionX");

		/// <summary>
		/// Resolution Y ID.
		/// </summary>
		public readonly int ShaderLinesDrawerResolutionY = Shader.PropertyToID("_ResolutionY");

		/// <summary>
		/// Horizontal lines count ID.
		/// </summary>
		public readonly int ShaderLinesDrawerHorizontalLinesCount = Shader.PropertyToID("_HLinesCount");

		/// <summary>
		/// Horizontal lines ID.
		/// </summary>
		public readonly int ShaderLinesDrawerHorizontalLines = Shader.PropertyToID("_HLines");

		/// <summary>
		/// Vertical lines count ID.
		/// </summary>
		public readonly int ShaderLinesDrawerVerticalLinesCount = Shader.PropertyToID("_VLinesCount");

		/// <summary>
		/// Vertical lines ID.
		/// </summary>
		public readonly int ShaderLinesDrawerVerticalLines = Shader.PropertyToID("_VLines");

		/// <summary>
		/// Transparent ID.
		/// </summary>
		public readonly int ShaderLinesDrawerTransparent = Shader.PropertyToID("_Transparent");

		/// <summary>
		/// Ring color ID.
		/// </summary>
		public readonly int ShaderRingColor = Shader.PropertyToID("_RingColor");

		/// <summary>
		/// Thickness ID.
		/// </summary>
		public readonly int ShaderRingThickness = Shader.PropertyToID("_Thickness");

		/// <summary>
		/// Padding ID.
		/// </summary>
		public readonly int ShaderRingPadding = Shader.PropertyToID("_Padding");

		/// <summary>
		/// Resolution ID.
		/// </summary>
		public readonly int ShaderRingResolution = Shader.PropertyToID("_Resolution");

		/// <summary>
		/// Fill ID.
		/// </summary>
		public readonly int ShaderRingFill = Shader.PropertyToID("_Fill");

		/// <summary>
		/// Transparent ID.
		/// </summary>
		public readonly int ShaderRingTransparent = Shader.PropertyToID("_Transparent");

		/// <summary>
		/// Start color ID.
		/// </summary>
		public readonly int ShaderRippleStartColor = Shader.PropertyToID("_RippleStartColor");

		/// <summary>
		/// End color ID.
		/// </summary>
		public readonly int ShaderRippleEndColor = Shader.PropertyToID("_RippleEndColor");

		/// <summary>
		/// Speed ID.
		/// </summary>
		public readonly int ShaderRippleSpeed = Shader.PropertyToID("_RippleSpeed");

		/// <summary>
		/// Max size ID.
		/// </summary>
		public readonly int ShaderRippleMaxSize = Shader.PropertyToID("_RippleMaxSize");

		/// <summary>
		/// Count ID.
		/// </summary>
		public readonly int ShaderRippleCount = Shader.PropertyToID("_RippleCount");

		/// <summary>
		/// Ripple ID.
		/// </summary>
		public readonly int ShaderRipple = Shader.PropertyToID("_Ripple");

		/// <summary>
		/// Size ID.
		/// </summary>
		public readonly int ShaderCornersSize = Shader.PropertyToID("_Size");

		/// <summary>
		/// Border radius ID.
		/// </summary>
		public readonly int ShaderCornersBorderRadius = Shader.PropertyToID("_BorderRadius");

		/// <summary>
		/// Border width ID.
		/// </summary>
		public readonly int ShaderCornersBorderWidth = Shader.PropertyToID("_BorderWidth");

		/// <summary>
		/// Border color ID.
		/// </summary>
		public readonly int ShaderCornersBorderColor = Shader.PropertyToID("_BorderColor");

		/// <summary>
		/// Border radius ID.
		/// </summary>
		public readonly int ShaderCornersX4BorderRadius = Shader.PropertyToID("_BorderRadius");

		/// <summary>
		/// Border width ID.
		/// </summary>
		public readonly int ShaderCornersX4BorderWidth = Shader.PropertyToID("_BorderWidth");

		/// <summary>
		/// Border color ID.
		/// </summary>
		public readonly int ShaderCornersX4BorderColor = Shader.PropertyToID("_BorderColor");

		/// <summary>
		/// Half size (xy) + origin (zw).
		/// </summary>
		public readonly int ShaderCornersX4HalfSizeAndOrigin = Shader.PropertyToID("_HalfSizeAndOrigin");

		/// <summary>
		/// Internal half size (xy) + origin (zw).
		/// Internal is minus border width.
		/// </summary>
		public readonly int ShaderCornersX4InternalHalfSizeAndOrigin = Shader.PropertyToID("_InternalHalfSizeAndOrigin");

		/// <summary>
		/// Width normal.
		/// </summary>
		public readonly Vector2 ShaderCornersX4WidthNormal = new Vector2(Mathf.Sqrt(0.5f), -Mathf.Sqrt(0.5f));

		/// <summary>
		/// Height normal.
		/// </summary>
		public readonly Vector2 ShaderCornersX4HeightNormal = new Vector2(Mathf.Sqrt(0.5f), Mathf.Sqrt(0.5f));

		/// <summary>
		/// Rate ID.
		/// </summary>
		public readonly int ShaderGrayscaleRate = Shader.PropertyToID("_Rates");

		/// <summary>
		/// Enabled ID.
		/// </summary>
		public readonly int ShaderGrayscaleEnabled = Shader.PropertyToID("_Enabled");
		#endregion

		#region ColorPicker

		/// <summary>
		/// RGB palette mode.
		/// </summary>
		public readonly ColorPickerPaletteMode[] RGBPaletteModes = new ColorPickerPaletteMode[]
		{
			ColorPickerPaletteMode.Red,
			ColorPickerPaletteMode.Green,
			ColorPickerPaletteMode.Blue,
		};

		/// <summary>
		/// HSV palette mode.
		/// </summary>
		public readonly ColorPickerPaletteMode[] HSVPaletteModes = new ColorPickerPaletteMode[]
		{
			ColorPickerPaletteMode.Hue,
			ColorPickerPaletteMode.Saturation,
			ColorPickerPaletteMode.Value,
			ColorPickerPaletteMode.HSVCircle,
		};

		/// <summary>
		/// Image palette mode.
		/// </summary>
		public readonly ColorPickerPaletteMode[] ImagePaletteModes = new ColorPickerPaletteMode[]
		{
			ColorPickerPaletteMode.Image,
		};

		#endregion

		#region ScrollBlock

		/// <summary>
		/// Default function to get value.
		/// </summary>
		public readonly Func<int, string> ScrollBlockDefaultValue = step => string.Format("Index: {0}", step.ToString());

		/// <summary>
		/// Default function to check is interactable.
		/// </summary>
		public readonly Func<bool> ScrollBlockDefaultInteractable = () => true;

		/// <summary>
		/// Default function to check is action allowed.
		/// </summary>
		public readonly Func<bool> ScrollBlockDefaultAllow = () => true;

		#endregion

		#region Types

		/// <summary>
		/// Types to friendly name.
		/// </summary>
		public readonly Dictionary<Type, string> Types2FriendlyName = new Dictionary<Type, string>();

		/// <summary>
		/// Types cache.
		/// </summary>
		public readonly Dictionary<string, Type> TypesCache = new Dictionary<string, Type>();

		#endregion

		#region Style

		/// <summary>
		/// Style: function to process TMPro game object.
		/// </summary>
		public readonly Func<StyleText, GameObject, bool> StyleTMProSupport =
#if UIWIDGETS_TMPRO_SUPPORT
			UIWidgets.TMProSupport.StyleTMPro.ApplyTo;
#else
			(style, go) => false;
#endif

		/// <summary>
		/// Style: function to process TMPro game object.
		/// </summary>
		public readonly Func<StyleText, GameObject, bool> StyleTMProSupportGetFrom =
#if UITHEMES_INSTALLED && UIWIDGETS_TMPRO_SUPPORT
			UIWidgets.TMProSupport.StyleTMPro.GetFrom;
#else
			(style, go) => false;
#endif

		#endregion

		#region LinesDrawer

		/// <summary>
		/// Create LineX instance.
		/// </summary>
		public readonly Func<float, bool, bool, LineX> LineXCreate = (x, left, right) => new LineX(x, left, right);

		/// <summary>
		/// Create LineY instance.
		/// </summary>
		public readonly Func<float, bool, bool, LineY> LineYCreate = (y, bottom, top) => new LineY(y, bottom, top);

		#endregion

		#region FilesListView

		/// <summary>
		/// FilesListView: Name comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> NameComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.DisplayName, y.DisplayName)
				: x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: CreationTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> CreationTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.CreationTime, y.CreationTime)
				: x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: LastWriteTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> LastWriteTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.LastWriteTime, y.LastWriteTime)
				: x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: LastAccessTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> LastAccessTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.LastAccessTime, y.LastAccessTime)
				: x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: Size comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> SizeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? UtilitiesCompare.Compare(x.Size, y.Size)
				: x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: Name comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> ReverseNameComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? -UtilitiesCompare.Compare(x.DisplayName, y.DisplayName)
				: -x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: CreationTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> ReverseCreationTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? -UtilitiesCompare.Compare(x.CreationTime, y.CreationTime)
				: -x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: LastWriteTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> ReverseLastWriteTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? -UtilitiesCompare.Compare(x.LastWriteTime, y.LastWriteTime)
				: -x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: LastAccessTime comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> ReverseLastAccessTimeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? -UtilitiesCompare.Compare(x.LastAccessTime, y.LastAccessTime)
				: -x.IsFile.CompareTo(y.IsFile);
		};

		/// <summary>
		/// FilesListView: Size comparer.
		/// </summary>
		public readonly Comparison<FileSystemEntry> ReverseSizeComparer = (x, y) =>
		{
			return x.IsFile == y.IsFile
				? -UtilitiesCompare.Compare(x.Size, y.Size)
				: -x.IsFile.CompareTo(y.IsFile);
		};

		#endregion

		/// <summary>
		/// Determines whether the specified pageComponent is null.
		/// </summary>
		public readonly Predicate<ScrollRectPage> PaginatorIsNullComponent = pageComponent => pageComponent == null;

		/// <summary>
		/// Notification animations.
		/// </summary>
		public readonly NotificationAnimationsList NotificationAnimations = new NotificationAnimationsList();

		/// <summary>
		/// Cache for WaitForSecondsCustom.
		/// </summary>
		public readonly List<WaitForSecondsCustom> WaitForSecondsCustom = new List<WaitForSecondsCustom>();

		/// <summary>
		/// Cache for TooltipListener.Info.
		/// </summary>
		public readonly Stack<TooltipListener.Info> TooltipInfoCache = new Stack<TooltipListener.Info>();

#if UNITY_EDITOR
		/// <summary>
		/// Refences.
		/// </summary>
		public readonly EditorReferences References = new EditorReferences();

		/// <summary>
		/// GUID references.
		/// </summary>
		public readonly ReferenceGUID ReferenceGUID = new ReferenceGUID();

		readonly Dictionary<string, PrefabsMenuGenerated> prefabsMenu = new Dictionary<string, PrefabsMenuGenerated>();

		/// <summary>
		/// Get PrefabsMenuGenerated asset.
		/// </summary>
		/// <param name="guid">GUID.</param>
		/// <returns>PrefabsMenuGenerated asset.</returns>
		public PrefabsMenuGenerated GetPrefabsMenuGenerated(string guid)
		{
			if (!prefabsMenu.TryGetValue(guid, out var menu))
			{
				menu = UtilitiesEditor.LoadAssetWithGUID<PrefabsMenuGenerated>(guid);
				prefabsMenu[guid] = menu;
			}

			return menu;
		}

#endif

#if ENABLE_INPUT_SYSTEM

		/// <summary>
		/// HotKeys.
		/// </summary>
		public readonly InputSystemKeysGroup[] HotKey2InputSystem = new[]
		{
			new InputSystemKeysGroup(Key.None),

			new InputSystemKeysGroup(Key.Backspace),
			new InputSystemKeysGroup(Key.Tab),
			new InputSystemKeysGroup(Key.Enter, Key.NumpadEnter),
			new InputSystemKeysGroup(Key.Pause),
			new InputSystemKeysGroup(Key.Escape),
			new InputSystemKeysGroup(Key.Space),

			new InputSystemKeysGroup(Key.Quote),
			new InputSystemKeysGroup(Key.Backquote),

			new InputSystemKeysGroup(Key.Period, Key.NumpadPeriod),
			new InputSystemKeysGroup(Key.Comma),
			new InputSystemKeysGroup(Key.Semicolon),

			new InputSystemKeysGroup(Key.Minus, Key.NumpadMinus),
			new InputSystemKeysGroup(Key.NumpadPlus),
			new InputSystemKeysGroup(Key.NumpadMultiply),
			new InputSystemKeysGroup(Key.Slash, Key.NumpadDivide),
			new InputSystemKeysGroup(Key.Equals, Key.NumpadEquals),

			new InputSystemKeysGroup(Key.Backslash),
			new InputSystemKeysGroup(Key.LeftBracket),
			new InputSystemKeysGroup(Key.RightBracket),

			new InputSystemKeysGroup(Key.Digit0),
			new InputSystemKeysGroup(Key.Digit1),
			new InputSystemKeysGroup(Key.Digit2),
			new InputSystemKeysGroup(Key.Digit3),
			new InputSystemKeysGroup(Key.Digit4),
			new InputSystemKeysGroup(Key.Digit5),
			new InputSystemKeysGroup(Key.Digit6),
			new InputSystemKeysGroup(Key.Digit7),
			new InputSystemKeysGroup(Key.Digit8),
			new InputSystemKeysGroup(Key.Digit9),

			new InputSystemKeysGroup(Key.A),
			new InputSystemKeysGroup(Key.B),
			new InputSystemKeysGroup(Key.C),
			new InputSystemKeysGroup(Key.D),
			new InputSystemKeysGroup(Key.E),
			new InputSystemKeysGroup(Key.F),
			new InputSystemKeysGroup(Key.G),
			new InputSystemKeysGroup(Key.H),
			new InputSystemKeysGroup(Key.I),
			new InputSystemKeysGroup(Key.J),
			new InputSystemKeysGroup(Key.K),
			new InputSystemKeysGroup(Key.L),
			new InputSystemKeysGroup(Key.M),
			new InputSystemKeysGroup(Key.N),
			new InputSystemKeysGroup(Key.O),
			new InputSystemKeysGroup(Key.P),
			new InputSystemKeysGroup(Key.Q),
			new InputSystemKeysGroup(Key.R),
			new InputSystemKeysGroup(Key.S),
			new InputSystemKeysGroup(Key.T),
			new InputSystemKeysGroup(Key.U),
			new InputSystemKeysGroup(Key.V),
			new InputSystemKeysGroup(Key.W),
			new InputSystemKeysGroup(Key.X),
			new InputSystemKeysGroup(Key.Y),
			new InputSystemKeysGroup(Key.Z),

			new InputSystemKeysGroup(Key.Numpad0),
			new InputSystemKeysGroup(Key.Numpad1),
			new InputSystemKeysGroup(Key.Numpad2),
			new InputSystemKeysGroup(Key.Numpad3),
			new InputSystemKeysGroup(Key.Numpad4),
			new InputSystemKeysGroup(Key.Numpad5),
			new InputSystemKeysGroup(Key.Numpad6),
			new InputSystemKeysGroup(Key.Numpad7),
			new InputSystemKeysGroup(Key.Numpad8),
			new InputSystemKeysGroup(Key.Numpad9),

			new InputSystemKeysGroup(Key.UpArrow),
			new InputSystemKeysGroup(Key.DownArrow),
			new InputSystemKeysGroup(Key.RightArrow),
			new InputSystemKeysGroup(Key.LeftArrow),

			new InputSystemKeysGroup(Key.Insert),
			new InputSystemKeysGroup(Key.Delete),
			new InputSystemKeysGroup(Key.Home),
			new InputSystemKeysGroup(Key.End),
			new InputSystemKeysGroup(Key.PageUp),
			new InputSystemKeysGroup(Key.PageDown),

			new InputSystemKeysGroup(Key.F1),
			new InputSystemKeysGroup(Key.F2),
			new InputSystemKeysGroup(Key.F3),
			new InputSystemKeysGroup(Key.F4),
			new InputSystemKeysGroup(Key.F5),
			new InputSystemKeysGroup(Key.F6),
			new InputSystemKeysGroup(Key.F7),
			new InputSystemKeysGroup(Key.F8),
			new InputSystemKeysGroup(Key.F9),
			new InputSystemKeysGroup(Key.F10),
			new InputSystemKeysGroup(Key.F11),
			new InputSystemKeysGroup(Key.F12),

			new InputSystemKeysGroup(Key.PrintScreen),

			new InputSystemKeysGroup(Key.NumLock),
			new InputSystemKeysGroup(Key.CapsLock),
			new InputSystemKeysGroup(Key.ScrollLock),
		};
#else
		public readonly LegacyInputKeysGroup[] HotKey2LegacyInput = new[]
		{
			new LegacyInputKeysGroup(KeyCode.None),

			new LegacyInputKeysGroup(KeyCode.Backspace),
			new LegacyInputKeysGroup(KeyCode.Tab),
			new LegacyInputKeysGroup(KeyCode.Return, KeyCode.KeypadEnter),
			new LegacyInputKeysGroup(KeyCode.Pause),
			new LegacyInputKeysGroup(KeyCode.Escape),
			new LegacyInputKeysGroup(KeyCode.Space),

			new LegacyInputKeysGroup(KeyCode.Quote),
			new LegacyInputKeysGroup(KeyCode.BackQuote),

			new LegacyInputKeysGroup(KeyCode.Comma),
			new LegacyInputKeysGroup(KeyCode.Period, KeyCode.KeypadPeriod),
			new LegacyInputKeysGroup(KeyCode.Semicolon),

			new LegacyInputKeysGroup(KeyCode.Minus, KeyCode.KeypadMinus),
			new LegacyInputKeysGroup(KeyCode.KeypadPlus),
			new LegacyInputKeysGroup(KeyCode.KeypadMultiply),
			new LegacyInputKeysGroup(KeyCode.Slash, KeyCode.KeypadDivide),
			new LegacyInputKeysGroup(KeyCode.Equals, KeyCode.KeypadEquals),

			new LegacyInputKeysGroup(KeyCode.Backslash),
			new LegacyInputKeysGroup(KeyCode.LeftBracket),
			new LegacyInputKeysGroup(KeyCode.RightBracket),

			new LegacyInputKeysGroup(KeyCode.Alpha0),
			new LegacyInputKeysGroup(KeyCode.Alpha1),
			new LegacyInputKeysGroup(KeyCode.Alpha2),
			new LegacyInputKeysGroup(KeyCode.Alpha3),
			new LegacyInputKeysGroup(KeyCode.Alpha4),
			new LegacyInputKeysGroup(KeyCode.Alpha5),
			new LegacyInputKeysGroup(KeyCode.Alpha6),
			new LegacyInputKeysGroup(KeyCode.Alpha7),
			new LegacyInputKeysGroup(KeyCode.Alpha8),
			new LegacyInputKeysGroup(KeyCode.Alpha9),

			new LegacyInputKeysGroup(KeyCode.A),
			new LegacyInputKeysGroup(KeyCode.B),
			new LegacyInputKeysGroup(KeyCode.C),
			new LegacyInputKeysGroup(KeyCode.D),
			new LegacyInputKeysGroup(KeyCode.E),
			new LegacyInputKeysGroup(KeyCode.F),
			new LegacyInputKeysGroup(KeyCode.G),
			new LegacyInputKeysGroup(KeyCode.H),
			new LegacyInputKeysGroup(KeyCode.I),
			new LegacyInputKeysGroup(KeyCode.J),
			new LegacyInputKeysGroup(KeyCode.K),
			new LegacyInputKeysGroup(KeyCode.L),
			new LegacyInputKeysGroup(KeyCode.M),
			new LegacyInputKeysGroup(KeyCode.N),
			new LegacyInputKeysGroup(KeyCode.O),
			new LegacyInputKeysGroup(KeyCode.P),
			new LegacyInputKeysGroup(KeyCode.Q),
			new LegacyInputKeysGroup(KeyCode.R),
			new LegacyInputKeysGroup(KeyCode.S),
			new LegacyInputKeysGroup(KeyCode.T),
			new LegacyInputKeysGroup(KeyCode.U),
			new LegacyInputKeysGroup(KeyCode.V),
			new LegacyInputKeysGroup(KeyCode.W),
			new LegacyInputKeysGroup(KeyCode.X),
			new LegacyInputKeysGroup(KeyCode.Y),
			new LegacyInputKeysGroup(KeyCode.Z),

			new LegacyInputKeysGroup(KeyCode.Keypad0),
			new LegacyInputKeysGroup(KeyCode.Keypad1),
			new LegacyInputKeysGroup(KeyCode.Keypad2),
			new LegacyInputKeysGroup(KeyCode.Keypad3),
			new LegacyInputKeysGroup(KeyCode.Keypad4),
			new LegacyInputKeysGroup(KeyCode.Keypad5),
			new LegacyInputKeysGroup(KeyCode.Keypad6),
			new LegacyInputKeysGroup(KeyCode.Keypad7),
			new LegacyInputKeysGroup(KeyCode.Keypad8),
			new LegacyInputKeysGroup(KeyCode.Keypad9),

			new LegacyInputKeysGroup(KeyCode.UpArrow),
			new LegacyInputKeysGroup(KeyCode.DownArrow),
			new LegacyInputKeysGroup(KeyCode.RightArrow),
			new LegacyInputKeysGroup(KeyCode.LeftArrow),

			new LegacyInputKeysGroup(KeyCode.Insert),
			new LegacyInputKeysGroup(KeyCode.Delete),
			new LegacyInputKeysGroup(KeyCode.Home),
			new LegacyInputKeysGroup(KeyCode.End),
			new LegacyInputKeysGroup(KeyCode.PageUp),
			new LegacyInputKeysGroup(KeyCode.PageDown),

			new LegacyInputKeysGroup(KeyCode.F1),
			new LegacyInputKeysGroup(KeyCode.F2),
			new LegacyInputKeysGroup(KeyCode.F3),
			new LegacyInputKeysGroup(KeyCode.F4),
			new LegacyInputKeysGroup(KeyCode.F5),
			new LegacyInputKeysGroup(KeyCode.F6),
			new LegacyInputKeysGroup(KeyCode.F7),
			new LegacyInputKeysGroup(KeyCode.F8),
			new LegacyInputKeysGroup(KeyCode.F9),
			new LegacyInputKeysGroup(KeyCode.F10),
			new LegacyInputKeysGroup(KeyCode.F11),
			new LegacyInputKeysGroup(KeyCode.F12),

			new LegacyInputKeysGroup(KeyCode.Print),

			new LegacyInputKeysGroup(KeyCode.Numlock),
			new LegacyInputKeysGroup(KeyCode.CapsLock),
			new LegacyInputKeysGroup(KeyCode.ScrollLock),
		};
#endif

		/// <summary>
		/// Do nothing.
		/// </summary>
		public readonly Action DoNothing = () => { };

		/// <summary>
		/// Animations: Linear curve.
		/// </summary>
		public readonly AnimationCurve LinearCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		/// <summary>
		/// AutocompleteRemote: Line separators.
		/// </summary>
		public readonly string[] LineSeparators = new string[] { "\r\n", "\r", "\n" };

		/// <summary>
		/// Compatibility: Scripting symbols separator.
		/// </summary>
		public readonly char[] ScriptingSymbolsSeparator = new char[] { ';' };

		/// <summary>
		/// DirectoryTreeView: Path separators.
		/// </summary>
		public readonly char[] PathSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

		/// <summary>
		/// FileListView: Match pattern separators.
		/// </summary>
		public readonly char[] FileMaskSeparators = new char[] { ';' };

		/// <summary>
		/// ILayoutElement: comparison.
		/// </summary>
		public readonly Comparison<ILayoutElement> ILayoutElementComparison = (a, b) => a.layoutPriority.CompareTo(b.layoutPriority);

		/// <summary>
		/// ILayoutElement: reverse comparison.
		/// </summary>
		public readonly Comparison<ILayoutElement> ILayoutElementComparisonReverse = (a, b) => -a.layoutPriority.CompareTo(b.layoutPriority);

		/// <summary>
		/// ResizableHandles: Show Resizable handles only if target (or one of handles) is selected.
		/// </summary>
		public readonly Func<ResizableHandles, BaseEventData, bool, bool> ResizableHandlesShowOnSelect = (resizableHandles, eventData, select) =>
		{
			if (select)
			{
				return true;
			}

			var ev = eventData as PointerEventData;
			if (eventData == null)
			{
				return false;
			}

			return resizableHandles.IsControlled(ev.pointerEnter);
		};

		/// <summary>
		/// RotatableHandle: Show Rotatable handle only if target (or one of handles) is selected.
		/// </summary>
		public readonly Func<RotatableHandle, BaseEventData, bool, bool> RotatableHandleShowOnSelect = (rotatableHandle, eventData, select) =>
		{
			if (select)
			{
				return true;
			}

			var ev = eventData as PointerEventData;
			if (eventData == null)
			{
				return false;
			}

			return rotatableHandle.IsControlled(ev.pointerEnter);
		};

		/// <summary>
		/// ScaleMark: Items comparison.
		/// </summary>
		public readonly Comparison<ScaleMark> ScaleMarkComparison = (x, y) => -x.Step.CompareTo(y.Step);

		/// <summary>
		/// Spinner: Always allow input.
		/// </summary>
		public readonly InputField.OnValidateInput SpinnerAlwaysAllow = (x, y, z) => z;

		/// <summary>
		/// TableHeader: Cell comparison.
		/// </summary>
		public readonly Comparison<TableHeaderCellInfo> CellComparison = (x, y) => x.Position.CompareTo(y.Position);

		/// <summary>
		/// Tabs: Default function for the CanSelectTab.
		/// </summary>
		public readonly Func<Tab, bool> TabsAllowSelect = x => true;

		/// <summary>
		/// UtilitiesColor: hex trim chars.
		/// </summary>
		public readonly char[] ColorHexTrimChars = new char[] { '#', ';' };

		/// <summary>
		/// Time.
		/// </summary>
		public ITime Time = new WidgetsTime();

		/// <summary>
		/// UpdaterProxy instance.
		/// </summary>
		public IUpdaterProxy UpdaterProxyInstance;

		/// <summary>
		/// Proxy to run Update().
		/// </summary>
		public IUpdaterProxy UpdaterProxy
		{
			get
			{
				if (Utilities.IsNull(UpdaterProxyInstance) && !UIWidgets.UpdaterProxy.Destroyed)
				{
					UpdaterProxyInstance = UIWidgets.UpdaterProxy.Instance;
				}

				return UpdaterProxyInstance;
			}

			set => UpdaterProxyInstance = value;
		}

		static StaticFields instance;

		/// <summary>
		/// Instance.
		/// </summary>
		public static StaticFields Instance => instance ??= new StaticFields();

		private StaticFields()
		{
			var type = typeof(ScrollRect);
			var content_start = type.GetField("m_ContentStartPosition", BindingFlags.NonPublic | BindingFlags.Instance);
			ListViewBaseScrollDataGetContentStartPosition = sr => (Vector2)content_start.GetValue(sr);
			ListViewBaseScrollDataSetContentStartPosition = (sr, value) => content_start.SetValue(sr, value);
		}

		#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		[DomainReload(nameof(instance))]
		static void StaticInit()
		{
			instance = null;
		}
		#endif
	}
}