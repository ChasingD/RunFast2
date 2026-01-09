namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Base custom class for Pickers.
	/// </summary>
	/// <typeparam name="TValue">Type of value.</typeparam>
	/// <typeparam name="TPicker">Type of picker.</typeparam>
	public abstract class Picker<TValue, TPicker> : Picker
		where TPicker : Picker<TValue, TPicker>
	{
		/// <summary>
		/// Picker result.
		/// </summary>
		public readonly struct Result
		{
			/// <summary>
			/// Selected value or default value if nothing selected.
			/// </summary>
			public readonly TValue Value;

			/// <summary>
			/// True if value was selected; otherwise false.
			/// </summary>
			public readonly bool Success;

			/// <summary>
			/// Initializes a new instance of the <see cref="Result"/> struct.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <param name="success">True if value was selected; otherwise false.</param>
			public Result(TValue value, bool success = true)
			{
				Value = value;
				Success = success;
			}

			/// <summary>
			/// Convert this instance to value.
			/// </summary>
			/// <param name="result">Picker result.</param>
			public static implicit operator TValue(Result result) => result.Value;

			/// <summary>
			/// Returns a string that represents the selected value.
			/// </summary>
			/// <returns>A string that represents the selected value.</returns>
			public override string ToString() => Value.ToString();
		}

		/// <summary>
		/// Picker templates.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Reviewed")]
		public static Templates<TPicker> Templates
		{
			get => WindowInstances<TPicker>.Instance.Templates;

			set => WindowInstances<TPicker>.Instance.Templates = value;
		}

		/// <summary>
		/// Opened pickers.
		/// </summary>
		public static IReadOnlyList<TPicker> OpenedPickers => WindowInstances<TPicker>.Instance.Opened;

		/// <summary>
		/// Inactive pickers with the same template.
		/// </summary>
		public List<TPicker> InactivePickers => WindowInstances<TPicker>.Instance.Templates.CachedInstances(TemplateName);

		/// <summary>
		/// All pickers.
		/// </summary>
		public static List<TPicker> AllPickers => WindowInstances<TPicker>.Instance.All;

		/// <summary>
		/// Count of the opened pickers.
		/// </summary>
		public static int Opened => WindowInstances<TPicker>.Instance.Count;

		/// <summary>
		/// Event on custom instance opened.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnInstanceOpen
		{
			add => WindowInstances<TPicker>.Instance.OnOpen += value;

			remove => WindowInstances<TPicker>.Instance.OnOpen -= value;
		}

		/// <summary>
		/// Event on custom instance closed.
		/// The parameter is opened instances count.
		/// </summary>
		public static event Action<int> OnInstanceClose
		{
			add => WindowInstances<TPicker>.Instance.OnClose += value;

			remove => WindowInstances<TPicker>.Instance.OnClose -= value;
		}

		/// <summary>
		/// Get opened pickers.
		/// </summary>
		/// <param name="output">Output list.</param>
		public static void GetOpenedPickers(List<TPicker> output) => WindowInstances<TPicker>.Instance.GetOpened(output);

		/// <summary>
		/// Value.
		/// </summary>
		protected TValue Value;

		/// <inheritdoc/>
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (!IsTemplate)
			{
				if (gameObject.activeSelf)
				{
					IsDestroyed = true;
					Cancel();
				}

				WindowInstances<TPicker>.Instance.Templates = null;
				return;
			}

			// if FindTemplates never called than TemplateName == null
			if (TemplateName != null)
			{
				Templates.Delete(TemplateName);
			}
		}

		/// <summary>
		/// Return picker instance using current instance as template.
		/// </summary>
		/// <returns>New picker instance.</returns>
		[Obsolete("Use Clone() instead.")]
		public TPicker Template()
		{
			return Clone();
		}

		/// <summary>
		/// Return picker instance using current instance as template.
		/// </summary>
		/// <returns>New picker instance.</returns>
		public virtual TPicker Clone()
		{
			var picker = this as TPicker;
			if ((TemplateName != null) && Templates.Exists(TemplateName))
			{
				// do nothing
			}
			else if (!Templates.Exists(gameObject.name))
			{
				Templates.Add(gameObject.name, picker);
			}
			else if (Templates.Get(gameObject.name) != this)
			{
				Templates.Add(gameObject.name, picker);
			}

			var id = gameObject.GetInstanceID().ToString();
			if (!Templates.Exists(id))
			{
				Templates.Add(id, picker);
			}
			else if (Templates.Get(id) != this)
			{
				Templates.Add(id, picker);
			}

			return Templates.Instance(id);
		}

		/// <summary>
		/// Callback with selected value.
		/// </summary>
		protected Action<TValue> OnSelect;

		/// <summary>
		/// Show picker.
		/// </summary>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="onSelect">Callback with selected value.</param>
		/// <param name="onCancel">Callback when picker closed without any value selected.</param>
		/// <param name="modalSprite">Modal sprite.</param>
		/// <param name="modalColor">Modal color.</param>
		/// <param name="canvas">Canvas.</param>
		/// <param name="onClose">On close callback. Also can be changed with OnClose field.</param>
		public virtual void Show(
			TValue defaultValue = default,
			Action<TValue> onSelect = null,
			Action onCancel = null,
			Sprite modalSprite = null,
			Color? modalColor = null,
			Canvas canvas = null,
			Action onClose = null)
		{
			AsyncResult = new Result(defaultValue, false);

			if (modalColor == null)
			{
				modalColor = new Color(0, 0, 0, 0.8f);
			}

			if (IsTemplate)
			{
				Debug.LogWarning("Use the template clone, not the template itself: PickerTemplate.Clone().Show(...), not PickerTemplate.Show(...)");
			}

			OnSelect = onSelect;
			OnCancel = onCancel;
			OnClose = onClose;

			var canvas_rt = SetCanvas(canvas);
			SetModal(modalSprite, modalColor, canvas_rt);

			gameObject.SetActive(true);

			BeforeOpen(defaultValue);
			InstanceOpened();
		}

		/// <summary>
		/// Show picker.
		/// </summary>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="modalSprite">Modal sprite.</param>
		/// <param name="modalColor">Modal color.</param>
		/// <param name="canvas">Canvas.</param>
		/// <returns>Selected value and confirmation.</returns>
		public virtual Picker<TValue, TPicker> ShowAsync(
			TValue defaultValue = default,
			Sprite modalSprite = null,
			Color? modalColor = null,
			Canvas canvas = null)
		{
			Show(defaultValue, modalSprite: modalSprite, modalColor: modalColor, canvas: canvas);
			return this;
		}

		/// <inheritdoc/>
		protected override void InstanceOpened()
		{
			if (AutoFocus)
			{
				UtilitiesUI.SelectChild(this, null);
			}

			WindowInstances<TPicker>.Instance.Add(this as TPicker);

			base.InstanceOpened();
		}

		/// <inheritdoc/>
		protected override void InstanceClosed()
		{
			OnClose = null;

			WindowInstances<TPicker>.Instance.Remove(this as TPicker);

			base.InstanceClosed();
		}

		/// <summary>
		/// Close picker with specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void Selected(TValue value)
		{
			OnSelect?.Invoke(value);

			AsyncResult = new Result(value);
			Complete();
			Close();
		}

		/// <summary>
		/// Prepare picker to open.
		/// </summary>
		/// <param name="defaultValue">Default value.</param>
		public virtual void BeforeOpen(TValue defaultValue) => Value = defaultValue;

		/// <inheritdoc/>
		protected override void Return()
		{
			Templates.ToCache(this as TPicker);

			InstanceClosed();
		}

		#region async

		/// <summary>
		/// Async result.
		/// </summary>
		protected Result AsyncResult;

		/// <summary>
		/// Gets an awaiter used to await this result.
		/// </summary>
		/// <returns>Awaiter.</returns>
		public virtual Picker<TValue, TPicker> GetAwaiter()
		{
			IsCompleted = false;
			return this;
		}

		/// <summary>
		/// Ends the wait for the completion of the asynchronous task.
		/// </summary>
		/// <returns>Result.</returns>
		public virtual Result GetResult() => AsyncResult;

		#endregion

		#region IStylable implementation

		/// <inheritdoc/>
		public override bool SetStyle(Style style)
		{
			if (TryGetComponent<Image>(out var bg))
			{
				style.Dialog.Background.ApplyTo(bg);
			}

			style.Dialog.Title.ApplyTo(transform.Find("Header/Title"));

			style.Dialog.ContentBackground.ApplyTo(transform.Find("Content"));

			style.Dialog.Delimiter.ApplyTo(transform.Find("Delimiter/Delimiter"));

			if (CloseButton != null)
			{
				style.ButtonClose.ApplyTo(CloseButton);
			}
			else
			{
				style.ButtonClose.Background.ApplyTo(transform.Find("Header/CloseButton"));
				style.ButtonClose.Text.ApplyTo(transform.Find("Header/CloseButton/Text"));
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool GetStyle(Style style)
		{
			if (TryGetComponent<Image>(out var bg))
			{
				style.Dialog.Background.GetFrom(bg);
			}

			style.Dialog.Title.GetFrom(transform.Find("Header/Title"));

			style.Dialog.ContentBackground.GetFrom(transform.Find("Content"));

			style.Dialog.Delimiter.GetFrom(transform.Find("Delimiter/Delimiter"));

			if (CloseButton != null)
			{
				style.ButtonClose.GetFrom(CloseButton);
			}
			else
			{
				style.ButtonClose.Background.GetFrom(transform.Find("Header/CloseButton"));
				style.ButtonClose.Text.GetFrom(transform.Find("Header/CloseButton/Text"));
			}

			return true;
		}

		#endregion
	}
}