namespace UIWidgets
{
	using System;
	using UIWidgets.Menu;
	using UnityEngine;
#if ENABLE_INPUT_SYSTEM
	using UnityEngine.InputSystem;
#endif

	/// <summary>
	/// Input compatibility functions.
	/// </summary>
	public static class CompatibilityInput
	{
		/// <summary>
		/// Mouse position.
		/// </summary>
		public static Vector2 MousePosition
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return MousePresent ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
				return Input.mousePosition;
#endif
			}
		}

		/// <summary>
		/// Is mouse present?
		/// </summary>
		public static bool MousePresent
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return Mouse.current != null;
#else
				return Input.mousePresent;
#endif
			}
		}

		/// <summary>
		/// Is left mouse button pressed?
		/// </summary>
		public static bool MouseLeftButtonPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return MousePresent && Mouse.current.leftButton.isPressed;
#else
				return Input.GetMouseButton(0);
#endif
			}
		}

		/// <summary>
		/// Is left mouse button pressed?
		/// </summary>
		[Obsolete]
		public static bool IsMouseLeftButtonPressed => MouseLeftButtonPressed;

		/// <summary>
		/// Is right mouse button pressed?
		/// </summary>
		public static bool MouseRightButtonPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return MousePresent && Mouse.current.rightButton.isPressed;
#else
				return Input.GetMouseButton(1);
#endif
			}
		}

		/// <summary>
		/// Is right mouse button pressed?
		/// </summary>
		[Obsolete]
		public static bool IsMouseRightButtonPressed => MouseRightButtonPressed;

		/// <summary>
		/// Is middle mouse button pressed?
		/// </summary>
		public static bool MouseMiddleButtonPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return MousePresent && Mouse.current.middleButton.isPressed;
#else
				return Input.GetMouseButton(2);
#endif
			}
		}

		/// <summary>
		/// Is middle mouse button pressed?
		/// </summary>
		[Obsolete]
		public static bool IsMouseMiddleButtonPressed => MouseMiddleButtonPressed;

		/// <summary>
		/// Touches count.
		/// </summary>
		public static int TouchCount
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
				return UnityEngine.InputSystem.Enhanced​Touch.Touch.activeTouches.Count;
#else
				return Input.touchCount;
#endif
			}
		}

#if ENABLE_INPUT_SYSTEM
		static UnityEngine.TouchPhase ConvertTouchPhase(UnityEngine.InputSystem.TouchPhase phase)
		{
			return phase switch
			{
				UnityEngine.InputSystem.TouchPhase.None => UnityEngine.TouchPhase.Ended,
				UnityEngine.InputSystem.TouchPhase.Began => UnityEngine.TouchPhase.Began,
				UnityEngine.InputSystem.TouchPhase.Moved => UnityEngine.TouchPhase.Moved,
				UnityEngine.InputSystem.TouchPhase.Ended => UnityEngine.TouchPhase.Ended,
				UnityEngine.InputSystem.TouchPhase.Canceled => UnityEngine.TouchPhase.Canceled,
				UnityEngine.InputSystem.TouchPhase.Stationary => UnityEngine.TouchPhase.Stationary,
				_ => UnityEngine.TouchPhase.Ended,
			};
		}

		static UnityEngine.Touch ConvertTouch(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
		{
			return new Touch()
			{
				fingerId = touch.finger.index,
				position = touch.screenPosition,
				rawPosition = touch.screenPosition,
				deltaPosition = touch.delta,
				deltaTime = (float)(touch.time - touch.startTime),
				tapCount = touch.tapCount,
				phase = ConvertTouchPhase(touch.phase),
				pressure = touch.pressure,
				maximumPossiblePressure = 1f,
				type = TouchType.Direct,
				radius = touch.radius.magnitude,
			};
		}
#endif

		/// <summary>
		/// Get touch.
		/// </summary>
		/// <param name="index">Touch index.</param>
		/// <returns>Touch.</returns>
		public static UnityEngine.Touch GetTouch(int index)
		{
#if ENABLE_INPUT_SYSTEM
			UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
			return ConvertTouch(UnityEngine.InputSystem.Enhanced​Touch.Touch.activeTouches[index]);
#else
			return Input.GetTouch(index);
#endif
		}

		/// <summary>
		/// Is keyboard present?
		/// </summary>
		public static bool KeyboardPresent
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return Keyboard.current != null;
#else
				return true;
#endif
			}
		}

		/// <summary>
		/// Is left arrow down?
		/// </summary>
		public static bool LeftArrowDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.leftArrowKey.wasPressedThisFrame;
#else
				return Input.GetKeyDown(KeyCode.LeftArrow);
#endif
			}
		}

		/// <summary>
		/// Is left arrow down?
		/// </summary>
		[Obsolete]
		public static bool IsLeftArrowDown => LeftArrowDown;

		/// <summary>
		/// Is right arrow down?
		/// </summary>
		public static bool RightArrowDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.rightArrowKey.wasPressedThisFrame;
#else
				return Input.GetKeyDown(KeyCode.RightArrow);
#endif
			}
		}

		/// <summary>
		/// Is right arrow down?
		/// </summary>
		[Obsolete]
		public static bool IsRightArrowDown => RightArrowDown;

		/// <summary>
		/// Is up arrow down?
		/// </summary>
		public static bool UpArrowDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.upArrowKey.wasPressedThisFrame;
#else
				return Input.GetKeyDown(KeyCode.UpArrow);
#endif
			}
		}

		/// <summary>
		/// Is up arrow down?
		/// </summary>
		[Obsolete]
		public static bool IsUpArrowDown => UpArrowDown;

		/// <summary>
		/// Is down arrow down?
		/// </summary>
		public static bool DownArrowDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.downArrowKey.wasPressedThisFrame;
#else
				return Input.GetKeyDown(KeyCode.DownArrow);
#endif
			}
		}

		/// <summary>
		/// Is down arrow down?
		/// </summary>
		[Obsolete]
		public static bool IsDownArrowDown => DownArrowDown;

		/// <summary>
		/// Is tab key down?
		/// </summary>
		public static bool TabDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.tabKey.wasPressedThisFrame;
#else
				return Input.GetKeyDown(KeyCode.Tab);
#endif
			}
		}

		/// <summary>
		/// Is tab key down?
		/// </summary>
		[Obsolete]
		public static bool IsTabDown => TabDown;

		/// <summary>
		/// Is enter key down?
		/// </summary>
		public static bool EnterDown
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
				return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
			}
		}

		/// <summary>
		/// Is enter key down?
		/// </summary>
		[Obsolete]
		public static bool IsEnterDown => EnterDown;

		/// <summary>
		/// Is enter key pressed?
		/// </summary>
		public static bool EnterPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && (Keyboard.current.enterKey.isPressed || Keyboard.current.numpadEnterKey.isPressed);
#else
				return Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
#endif
			}
		}

		/// <summary>
		/// Is enter key pressed?
		/// </summary>
		[Obsolete]
		public static bool IsEnterPressed => EnterPressed;

		/// <summary>
		/// Is shift key pressed?
		/// </summary>
		[Obsolete]
		public static bool IsShiftPressed => ShiftPressed;

		/// <summary>
		/// Is pointer over screen?
		/// </summary>
		public static bool PointerOverScreen
		{
			get
			{
#if UNITY_EDITOR
				var screen_size = UnityEditor.Handles.GetMainGameViewSize();
#else
				var screen_size = new Vector2(Screen.width, Screen.height);
#endif
				var mouse = MousePosition;

				if ((mouse.x <= 0)
					|| (mouse.y <= 0)
					|| (mouse.x >= (screen_size.x - 1))
					|| (mouse.y >= (screen_size.y - 1)))
				{
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Is pointer over screen?
		/// </summary>
		[Obsolete]
		public static bool IsPointerOverScreen => PointerOverScreen;

		/// <summary>
		/// Is context menu key pressed?
		/// </summary>
		public static bool ContextMenuPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.contextMenuKey.isPressed;
#else
				return Input.GetKey(KeyCode.Menu);
#endif
			}
		}

		/// <summary>
		/// Is context menu key up?
		/// </summary>
		public static bool ContextMenuUp
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.contextMenuKey.wasReleasedThisFrame;
#else
				return Input.GetKeyUp(KeyCode.Menu);
#endif
			}
		}

		/// <summary>
		/// Is Ctrl key pressed?
		/// </summary>
		public static bool CtrlPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.ctrlKey.isPressed;
#else
				return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
			}
		}

		/// <summary>
		/// Is Shift key pressed?
		/// </summary>
		public static bool ShiftPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.shiftKey.isPressed;
#else
				return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
			}
		}

		/// <summary>
		/// Is Alt key pressed?
		/// </summary>
		public static bool AltPressed
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return KeyboardPresent && Keyboard.current.altKey.isPressed;
#else
				return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
#endif
			}
		}

#if ENABLE_INPUT_SYSTEM
		/// <summary>
		/// Keys group for InputSystem.
		/// </summary>
		public readonly struct InputSystemKeysGroup : IEquatable<InputSystemKeysGroup>
		{
			/// <summary>
			/// Keys.
			/// </summary>
			public readonly Key[] Keys;

			/// <summary>
			/// Initializes a new instance of the <see cref="InputSystemKeysGroup"/> struct.
			/// </summary>
			/// <param name="keys">Keys.</param>
			public InputSystemKeysGroup(params Key[] keys)
			{
				Keys = keys;
			}

			/// <summary>
			/// Is any key in group is pressed?
			/// </summary>
			public bool IsPressed
			{
				get
				{
					if (!KeyboardPresent)
					{
						return false;
					}

					for (int i = 0; i < Keys.Length; i++)
					{
						var key = Keys[i];
						if (Keyboard.current[key].isPressed)
						{
							return true;
						}
					}

					return false;
				}
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public override bool Equals(object obj) => (obj is InputSystemKeysGroup group) && Equals(group);

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="other">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public bool Equals(InputSystemKeysGroup other) => Keys == other.Keys;

			/// <summary>
			/// Hash function.
			/// </summary>
			/// <returns>A hash code for the current object.</returns>
			public override int GetHashCode() => HashCode.Combine(Keys);

			/// <summary>
			/// Compare specified instances.
			/// </summary>
			/// <param name="a">First instance.</param>
			/// <param name="b">Second instance.</param>
			/// <returns>true if the instances are equal; otherwise, false.</returns>
			public static bool operator ==(InputSystemKeysGroup a, InputSystemKeysGroup b) => a.Equals(b);

			/// <summary>
			/// Compare specified instances.
			/// </summary>
			/// <param name="a">First instance.</param>
			/// <param name="b">Second instance.</param>
			/// <returns>true if the instances not equal; otherwise, false.</returns>
			public static bool operator !=(InputSystemKeysGroup a, InputSystemKeysGroup b) => !a.Equals(b);
		}

#else
		/// <summary>
		/// Keys group for legacy input.
		/// </summary>
		public struct LegacyInputKeysGroup : IEquatable<LegacyInputKeysGroup>
		{
			/// <summary>
			/// Keys.
			/// </summary>
			public readonly KeyCode[] Keys;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="keys">Keys.</param>
			public LegacyInputKeysGroup(params KeyCode[] keys)
			{
				Keys = keys;
			}

			/// <summary>
			/// Is any key in group is pressed?
			/// </summary>
			public bool IsPressed
			{
				get
				{
					for (int i = 0; i < Keys.Length; i++)
					{
						var key = Keys[i];
						if (Input.GetKey(key))
						{
							return true;
						}
					}

					return false;
				}
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				if (obj is LegacyInputKeysGroup)
				{
					return Equals((LegacyInputKeysGroup)obj);
				}

				return false;
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public bool Equals(LegacyInputKeysGroup other)
			{
				return Keys == other.Keys;
			}

			/// <summary>
			/// Hash function.
			/// </summary>
			/// <returns>A hash code for the current object.</returns>
			public override int GetHashCode() => HashCode.Combine(Keys);

			/// <summary>
			/// Compare specified instances.
			/// </summary>
			/// <param name="color1">First instance.</param>
			/// <param name="color2">Second instance.</param>
			/// <returns>true if the instances are equal; otherwise, false.</returns>
			public static bool operator ==(LegacyInputKeysGroup a, LegacyInputKeysGroup b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Compare specified instances.
			/// </summary>
			/// <param name="color1">First instance.</param>
			/// <param name="color2">Second instance.</param>
			/// <returns>true if the instances not equal; otherwise, false.</returns>
			public static bool operator !=(LegacyInputKeysGroup a, LegacyInputKeysGroup b)
			{
				return !a.Equals(b);
			}
		}
#endif

#if ENABLE_INPUT_SYSTEM
		/// <summary>
		/// Get keys group.
		/// </summary>
		/// <param name="key">HotKey code.</param>
		/// <returns>Keys group</returns>
		public static InputSystemKeysGroup KeysGroup(HotKeyCode key)
		{
			return StaticFields.Instance.HotKey2InputSystem[(int)key];
		}
#endif

		/// <summary>
		/// Is hot key pressed?
		/// </summary>
		/// <param name="key">Key.</param>
		/// <returns>true if hot key pressed; otherwise false.</returns>
		public static bool IsPressed(HotKeyCode key)
		{
#if ENABLE_INPUT_SYSTEM
			return StaticFields.Instance.HotKey2InputSystem[(int)key].IsPressed;
#else
			return StaticFields.Instance.HotKey2LegacyInput[(int)key].IsPressed;
#endif
		}
	}
}