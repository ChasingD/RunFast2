namespace UIWidgets.l10n
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Locale changed event handler.
	/// </summary>
	public delegate void LocaleChangedEventHandler();

	/// <summary>
	/// Localization class.
	/// </summary>
	public static class Localization
	{
		/// <summary>
		/// Locale changed event.
		/// </summary>
		public static event LocaleChangedEventHandler OnLocaleChanged
		{
			add => StaticFields.Instance.LocalizationOnChanged += value;

			remove => StaticFields.Instance.LocalizationOnChanged -= value;
		}

		/// <summary>
		/// Translate the specified string.
		/// </summary>
		public static ref Func<string, string> GetTranslation => ref StaticFields.Instance.LocalizationTranslate;

		/// <summary>
		/// Get country code.
		/// Used to instantiate System.Globalization.CultureInfo.
		/// </summary>
		/// <returns>Country code.</returns>
		public static ref Func<string> GetCountryCode => ref StaticFields.Instance.LocalizationCountryCode;

		static ref CultureInfo CurrentCulture => ref StaticFields.Instance.LocalizationCurrentCulture;

		/// <summary>
		/// Get culture info (based on country code).
		/// </summary>
		public static CultureInfo Culture
		{
			get
			{
				if (CurrentCulture == null)
				{
					var code = GetCountryCode();
					CurrentCulture = GetCulture(code);
				}

				return CurrentCulture;
			}
		}

		/// <summary>
		/// Get culture.
		/// </summary>
		public static ref Func<string, CultureInfo> GetCulture => ref StaticFields.Instance.LocalizationCulture;

		/// <summary>
		/// Default implementation of get culture.
		/// </summary>
		/// <param name="code">Country code.</param>
		/// <returns>Culture.</returns>
		public static CultureInfo DefaultGetCulture(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return CultureInfo.InvariantCulture;
			}

			var new_code = string.Format("{0}-{1}", code, code.ToUpperInvariant());
			try
			{
				var culture = new CultureInfo(code);
				if (culture.IsNeutralCulture)
				{
					return new CultureInfo(new_code);
				}

				return culture;
			}
			catch (CultureNotFoundException)
			{
				try
				{
					return new CultureInfo(new_code);
				}
				catch (CultureNotFoundException)
				{
				}
			}
			catch (NotSupportedException)
			{
				try
				{
					return new CultureInfo(new_code);
				}
				catch (NotSupportedException)
				{
				}
			}

			return CultureInfo.InvariantCulture;
		}

#if UIWIDGETS_UNITY_LOCALIZATION_SUPPORT
		/// <summary>
		/// Translate the input string using Unity Localization.
		/// </summary>
		/// <param name="input">String to translate.</param>
		/// <returns>Translated string.</returns>
		public static string UnityTranslation(string input)
		{
			var db = UnityEngine.Localization.Settings.LocalizationSettings.Instance.GetStringDatabase();
			return db.GetLocalizedString(db.DefaultTable, input);
		}

		/// <summary>
		/// Get country code of the current locale using Unity Localization.
		/// </summary>
		/// <returns>Country code.</returns>
		public static string UnityCountryCode()
		{
			var locale = UnityEngine.Localization.Settings.LocalizationSettings.Instance.GetSelectedLocale();
			return locale.Identifier.Code;
		}

		public static void UnityLocaleChanged(UnityEngine.Localization.Locale locale) => LocaleChanged();

#elif UIWIDGETS_I2LOCALIZATION_SUPPORT
		/// <summary>
		/// Translate the input string using I2 Localization.
		/// </summary>
		/// <param name="input">String to translate.</param>
		/// <returns>Translated string.</returns>
		public static string I2Translation(string input)
		{
			var result = I2.Loc.LocalizationManager.GetTranslation(input);
			return (result != null) ? result : input;
		}

		/// <summary>
		/// Get country code of the current locale using I2 Localization.
		/// </summary>
		/// <returns>Country code.</returns>
		public static string I2CountryCode()
		{
			var code = I2.Loc.LocalizationManager.CurrentLanguageCode;
			if (code == "ja")
			{
				return "ja-JP";
			}

			return code;
		}
#endif

		static Localization()
		{
			#if UIWIDGETS_UNITY_LOCALIZATION_SUPPORT
			GetTranslation = UnityTranslation;
			GetCountryCode = UnityCountryCode;
			UnityEngine.Localization.Settings.LocalizationSettings.Instance.OnSelectedLocaleChanged += UnityLocaleChanged;
			#elif UIWIDGETS_I2LOCALIZATION_SUPPORT
			GetTranslation = I2Translation;
			GetCountryCode = I2CountryCode;
			I2.Loc.LocalizationManager.OnLocalizeEvent += LocaleChanged;
			#endif
		}

		/// <summary>
		/// Invoke locale changed event.
		/// </summary>
		public static void LocaleChanged() => StaticFields.Instance.LocalizationChanged();

		/// <summary>
		/// Get default country code.
		/// Used to instantiate System.Globalization.CultureInfo.
		/// </summary>
		/// <returns>Country code.</returns>
		public static string DefaultCountryCode() => null;

		/// <summary>
		/// Return the input string without translation.
		/// </summary>
		/// <param name="input">String to translate.</param>
		/// <returns>Translated string.</returns>
		public static string NoTranslation(string input) => input;
	}
}