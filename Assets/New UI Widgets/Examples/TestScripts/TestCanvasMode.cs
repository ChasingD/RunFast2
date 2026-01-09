namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Test Canvas mode.
	/// </summary>
	public class TestCanvasMode : MonoBehaviour
	{
#if UITHEMES_INSTALLED
		/// <summary>
		/// UI Theme.
		/// </summary>
		[SerializeField]
		public UIThemes.Theme Theme;
#endif

		/// <summary>
		/// Canvas.
		/// </summary>
		[SerializeField]
		public Canvas Canvas;

		/// <summary>
		/// Toggle overlay.
		/// </summary>
		[SerializeField]
		public Toggle ToggleOverlay;

		/// <summary>
		/// Toggle camera.
		/// </summary>
		[SerializeField]
		public Toggle ToggleCamera;

		/// <summary>
		/// Toggle world space.
		/// </summary>
		[SerializeField]
		public Toggle ToggleWorldSpace;

		/// <summary>
		/// Toggle theme blue.
		/// </summary>
		[SerializeField]
		public Toggle ToggleThemeBlue;

		/// <summary>
		/// Toggle theme red.
		/// </summary>
		[SerializeField]
		public Toggle ToggleThemeRed;

		/// <summary>
		/// Toggle theme dark.
		/// </summary>
		[SerializeField]
		public Toggle ToggleThemeDark;

		/// <summary>
		/// Toggle theme legacy.
		/// </summary>
		[SerializeField]
		public Toggle ToggleThemeLegacy;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected void Start()
		{
			// Canvas
			if (ToggleOverlay != null)
			{
				ToggleOverlay.onValueChanged.AddListener(SetOverlay);
			}

			if (ToggleCamera != null)
			{
				ToggleCamera.onValueChanged.AddListener(SetCamera);
			}

			if (ToggleWorldSpace != null)
			{
				ToggleWorldSpace.onValueChanged.AddListener(SetWorldSpace);
			}

#if UITHEMES_INSTALLED
			// Theme
			if (ToggleThemeBlue != null)
			{
				ToggleThemeBlue.onValueChanged.AddListener(SetThemeBlue);
			}

			if (ToggleThemeRed != null)
			{
				ToggleThemeRed.onValueChanged.AddListener(SetThemeRed);
			}

			if (ToggleThemeDark != null)
			{
				ToggleThemeDark.onValueChanged.AddListener(SetThemeDark);
			}

			if (ToggleThemeLegacy != null)
			{
				ToggleThemeLegacy.onValueChanged.AddListener(SetThemeLegacy);
			}
#endif
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected void OnDestroy()
		{
			// Canvas
			if (ToggleOverlay != null)
			{
				ToggleOverlay.onValueChanged.RemoveListener(SetOverlay);
			}

			if (ToggleCamera != null)
			{
				ToggleCamera.onValueChanged.RemoveListener(SetCamera);
			}

			if (ToggleWorldSpace != null)
			{
				ToggleWorldSpace.onValueChanged.RemoveListener(SetWorldSpace);
			}

#if UITHEMES_INSTALLED
			// Themes
			if (ToggleThemeBlue != null)
			{
				ToggleThemeBlue.onValueChanged.RemoveListener(SetThemeBlue);
			}

			if (ToggleThemeRed != null)
			{
				ToggleThemeRed.onValueChanged.RemoveListener(SetThemeRed);
			}

			if (ToggleThemeDark != null)
			{
				ToggleThemeDark.onValueChanged.RemoveListener(SetThemeDark);
			}

			if (ToggleThemeLegacy != null)
			{
				ToggleThemeLegacy.onValueChanged.RemoveListener(SetThemeLegacy);
			}
#endif
		}

		/// <summary>
		/// Set ScreenSpaceOverlay.
		/// </summary>
		public void SetOverlay()
		{
			Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		/// <summary>
		/// Set ScreenSpaceCamera.
		/// </summary>
		public void SetCamera()
		{
			Canvas.renderMode = RenderMode.ScreenSpaceCamera;
		}

		/// <summary>
		/// Set WorldSpace.
		/// </summary>
		public void SetWorldSpace()
		{
			Canvas.renderMode = RenderMode.WorldSpace;

			var rt = Canvas.transform as RectTransform;
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280f);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 720f);
			rt.localScale = new Vector3(0.16f, 0.16f, 0.16f);
			rt.localRotation = Quaternion.Euler(45f, -5f, -5f);
			rt.localPosition = new Vector3(10f, -70f, 60f);
		}

		/// <summary>
		/// Toggle ScreenSpaceOverlay.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetOverlay(bool isOn)
		{
			if (isOn)
			{
				SetOverlay();
			}
		}

		/// <summary>
		/// Toggle ScreenSpaceCamera.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetCamera(bool isOn)
		{
			if (isOn)
			{
				SetCamera();
			}
		}

		/// <summary>
		/// Toggle WorldSpace.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetWorldSpace(bool isOn)
		{
			if (isOn)
			{
				SetWorldSpace();
			}
		}

#if UITHEMES_INSTALLED
		/// <summary>
		/// Set theme variation.
		/// </summary>
		/// <param name="variation">Variation.</param>
		public void ToggleTheme(string variation)
		{
			Theme.SetActiveVariation(variation);
		}

		/// <summary>
		/// Set blue theme variation.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetThemeBlue(bool isOn)
		{
			if (isOn)
			{
				Theme.SetActiveVariation("Blue");
			}
		}

		/// <summary>
		/// Set red theme variation.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetThemeRed(bool isOn)
		{
			if (isOn)
			{
				Theme.SetActiveVariation("Red");
			}
		}

		/// <summary>
		/// Set dark theme variation.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetThemeDark(bool isOn)
		{
			if (isOn)
			{
				Theme.SetActiveVariation("Dark");
			}
		}

		/// <summary>
		/// Set legacy theme variation.
		/// </summary>
		/// <param name="isOn">Is on.</param>
		public void SetThemeLegacy(bool isOn)
		{
			if (isOn)
			{
				Theme.SetActiveVariation("Legacy");
			}
		}
#endif
	}
}