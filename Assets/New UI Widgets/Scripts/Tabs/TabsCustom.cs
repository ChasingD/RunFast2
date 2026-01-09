namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UIWidgets.Attributes;
	using UIWidgets.l10n;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for creating custom tabs.
	/// </summary>
	/// <typeparam name="TTab">Type of tab data.</typeparam>
	/// <typeparam name="TButton">Type of tab button.</typeparam>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/widgets/containers/tabs.html")]
	public class TabsCustom<TTab, TButton> : TabsBase
		#if UITHEMES_INSTALLED
		, IStylable<StyleTabs>
		#endif
		where TTab : Tab
		where TButton : TabButton<TTab>
	{
		/// <summary>
		/// Information of the Tab button.
		/// </summary>
		[Serializable]
		public class TabButtonInfo
		{
			[SerializeField]
			[FormerlySerializedAs("Owner")]
			TabsCustom<TTab, TButton> owner;

			[SerializeField]
			[FormerlySerializedAs("DefaultButton")]
			TButton defaultButton;

			/// <summary>
			/// Default button.
			/// </summary>
			public TButton DefaultButton => defaultButton;

			[SerializeField]
			[FormerlySerializedAs("ActiveButton")]
			TButton activeButton;

			/// <summary>
			/// Active button.
			/// </summary>
			public TButton ActiveButton => activeButton;

			[SerializeField]
			[FormerlySerializedAs("Tab")]
			TTab tab;

			/// <summary>
			/// Initializes a new instance of the <see cref="TabButtonInfo"/> class.
			/// </summary>
			/// <param name="owner">Owner.</param>
			/// <param name="defaultButton">Default button.</param>
			/// <param name="activeButton">Active button.</param>
			public TabButtonInfo(TabsCustom<TTab, TButton> owner, TButton defaultButton, TButton activeButton)
			{
				this.owner = owner;

				this.defaultButton = defaultButton;
				this.defaultButton.transform.SetParent(this.owner.Container, false);

				this.activeButton = activeButton;
				this.activeButton.transform.SetParent(this.owner.Container, false);

				this.defaultButton.onClick.AddListener(Click);

				this.defaultButton.OnSelectEvent.AddListener(Select);
			}

			/// <summary>
			/// Process the select event.
			/// </summary>
			protected void Select()
			{
				if (owner.ImmediateSelect)
				{
					owner.ProcessButtonClick(tab);
				}
			}

			/// <summary>
			/// Process the click event.
			/// </summary>
			protected void Click() => owner.ProcessButtonClick(tab);

			/// <summary>
			/// Set the tab.
			/// </summary>
			/// <param name="tab">Tab.</param>
			public void SetTab(TTab tab)
			{
				this.tab = tab;

				SetData();
			}

			/// <summary>
			/// Set buttons data.
			/// </summary>
			public void SetData()
			{
				defaultButton.SetData(tab);
				activeButton.SetData(tab);
			}

			/// <summary>
			/// Remove buttons callback.
			/// </summary>
			protected void RemoveCallback()
			{
				if (!Utilities.IsNull(defaultButton))
				{
					defaultButton.onClick.RemoveListener(Click);
				}
			}

			/// <summary>
			/// Enable buttons interactions.
			/// </summary>
			public void EnableInteractable()
			{
				defaultButton.interactable = true;
				activeButton.interactable = true;
			}

			/// <summary>
			/// Disable buttons interactions
			/// </summary>
			public void DisableInteractable()
			{
				defaultButton.interactable = false;
				activeButton.interactable = false;
			}

			/// <summary>
			/// Toggle to the default state.
			/// </summary>
			public void Default()
			{
				defaultButton.gameObject.SetActive(true);
				activeButton.gameObject.SetActive(false);
			}

			/// <summary>
			/// Toggle to the active state.
			/// </summary>
			public void Active()
			{
				defaultButton.gameObject.SetActive(false);
				activeButton.gameObject.SetActive(true);

				if (owner.EventSystemSelectActiveHeader && activeButton.gameObject.activeInHierarchy)
				{
					if (EventSystem.current.alreadySelecting)
					{
						Updater.RunOnce(activeButton, SetSelected);
					}
					else
					{
						SetSelected();
					}
				}
			}

			void SetSelected()
			{
				EventSystem.current.SetSelectedGameObject(activeButton.gameObject);
			}

			#if UITHEMES_INSTALLED
			/// <summary>
			/// Set the style.
			/// </summary>
			/// <param name="style">Style.</param>
			public void SetStyle(StyleTabs style)
			{
				style.DefaultButton.ApplyTo(defaultButton.gameObject);
				style.ActiveButton.ApplyTo(activeButton.gameObject);
			}
			#endif

			/// <summary>
			/// Destroy buttons.
			/// </summary>
			public void Destroy()
			{
				Updater.RemoveRunOnce(SetSelected);

				RemoveCallback();

				if (defaultButton != null)
				{
					UnityEngine.Object.Destroy(defaultButton.gameObject);
				}

				if (activeButton != null)
				{
					UnityEngine.Object.Destroy(activeButton.gameObject);
				}

				owner = null;
			}
		}

		/// <summary>
		/// Animation data.
		/// </summary>
		public readonly struct AnimationData
		{
			/// <summary>
			/// Previous tab.
			/// </summary>
			public readonly TTab PreviousTab;

			/// <summary>
			/// Current tab.
			/// </summary>
			public readonly TTab CurrentTab;

			/// <summary>
			/// Duration.
			/// </summary>
			public readonly float Duration;

			/// <summary>
			/// Unscaled time.
			/// </summary>
			public readonly bool UnscaledTime;

			/// <summary>
			/// Has previous tab.
			/// </summary>
			public readonly bool HasPreviousTab => PreviousTab?.TabObject != null;

			/// <summary>
			/// Initializes a new instance of the <see cref="AnimationData"/> struct.
			/// </summary>
			/// <param name="previousTab">Previous tab.</param>
			/// <param name="currentTab">Current tab.</param>
			/// <param name="duration">Duration.</param>
			/// <param name="unscaledTime">Unscaled time.</param>
			public AnimationData(TTab previousTab, TTab currentTab, float duration, bool unscaledTime)
			{
				PreviousTab = previousTab;
				CurrentTab = currentTab;
				Duration = duration;
				UnscaledTime = unscaledTime;
			}
		}

		/// <summary>
		/// The container for tab toggle buttons.
		/// </summary>
		[SerializeField]
		public Transform Container;

		/// <summary>
		/// The default tab button.
		/// </summary>
		[SerializeField]
		public TButton DefaultTabButton;

		/// <summary>
		/// The active tab button.
		/// </summary>
		[SerializeField]
		public TButton ActiveTabButton;

		[SerializeField]
		TTab[] tabObjects = Compatibility.EmptyArray<TTab>();

		/// <summary>
		/// Gets or sets the tab objects.
		/// </summary>
		/// <value>The tab objects.</value>
		public TTab[] TabObjects
		{
			get => tabObjects;

			set
			{
				tabObjects = value;
				UpdateButtons();
			}
		}

		/// <summary>
		/// The name of the default tab.
		/// </summary>
		[SerializeField]
		[Tooltip("Tab name which will be active by default, if not specified will be opened first Tab.")]
		public string DefaultTabName = string.Empty;

		/// <summary>
		/// If true does not deactivate hidden tabs.
		/// </summary>
		[SerializeField]
		[Tooltip("If true does not deactivate hidden tabs.")]
		public bool KeepTabsActive = false;

		/// <summary>
		/// Toggle tab on EventSystem select event.
		/// </summary>
		[SerializeField]
		[Tooltip("Toggle tab on EventSystem select event.")]
		public bool ImmediateSelect = false;

		/// <summary>
		/// Make active header selected by EventsSystem.
		/// </summary>
		[SerializeField]
		[Tooltip("Select active header by EventsSystem.")]
		public bool EventSystemSelectActiveHeader = true;

		/// <summary>
		/// Animate tabs change.
		/// </summary>
		[SerializeField]
		public bool Animation = true;

		/// <summary>
		/// Animation duration.
		/// </summary>
		[SerializeField]
		[EditorConditionBool(nameof(Animation))]
		public float AnimationDuration = 0.2f;

		/// <summary>
		/// Animation with unscaled time.
		/// </summary>
		[SerializeField]
		[EditorConditionBool(nameof(Animation))]
		public bool UnscaledTime = true;

		/// <summary>
		/// Current animation.
		/// </summary>
		protected IEnumerator CurrentAnimation;

		/// <summary>
		/// Custom animation.
		/// </summary>
		public Func<AnimationData, IEnumerator> CustomAnimation;

		/// <summary>
		/// OnTabSelect event.
		/// </summary>
		[SerializeField]
		public TabSelectEvent OnTabSelect = new TabSelectEvent();

		/// <summary>
		/// Gets or sets the selected tab.
		/// </summary>
		/// <value>The selected tab.</value>
		public TTab SelectedTab
		{
			get;
			protected set;
		}

		/// <inheritdoc/>
		public override int SelectedTabIndex => Array.IndexOf(TabObjects, SelectedTab);

		/// <inheritdoc/>
		protected override int TabsCount => TabObjects.Length;

		/// <summary>
		/// Buttons.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TabButtonInfo> Buttons = new List<TabButtonInfo>();

		/// <summary>
		/// Check is tab can be selected.
		/// </summary>
		public Func<TTab, bool> CanSelectTab = x => true;

		/// <summary>
		/// Default function for the CanSelectTab.
		/// </summary>
		public static Func<TTab, bool> AllowSelect => x => true;

		/// <inheritdoc/>
		protected override void InitOnce()
		{
			base.InitOnce();

			if (Container == null)
			{
				throw new InvalidOperationException("Container is null. Set object of type GameObject to Container.");
			}

			if (DefaultTabButton == null)
			{
				throw new InvalidOperationException("DefaultTabButton is null. Set object of type GameObject to DefaultTabButton.");
			}

			if (ActiveTabButton == null)
			{
				throw new InvalidOperationException("ActiveTabButton is null. Set object of type GameObject to ActiveTabButton.");
			}

			DefaultTabButton.gameObject.SetActive(false);
			ActiveTabButton.gameObject.SetActive(false);

			UpdateButtons();
		}

		bool localeSubscription;

		/// <summary>
		/// Process the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (!localeSubscription)
			{
				Init();

				localeSubscription = true;
				Localization.OnLocaleChanged += LocaleChanged;
				LocaleChanged();
			}
		}

		/// <summary>
		/// Process destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			Localization.OnLocaleChanged -= LocaleChanged;

			for (int i = 0; i < Buttons.Count; i++)
			{
				Buttons[i].Destroy();
			}

			Buttons.Clear();
		}

		/// <inheritdoc/>
		protected override bool CanSelectTabByIndex(int index) => CanSelectTab(TabObjects[index]);

		/// <summary>
		/// Process locale changes.
		/// </summary>
		protected virtual void LocaleChanged() => UpdateButtonsData();

		/// <summary>
		/// Update buttons data.
		/// </summary>
		public virtual void UpdateButtonsData()
		{
			if (!IsInited)
			{
				return;
			}

			foreach (var btn in Buttons)
			{
				btn.SetData();
			}
		}

		/// <summary>
		/// Update button data.
		/// </summary>
		/// <param name="index">Index.</param>
		public virtual void UpdateButtonData(int index)
		{
			if (!IsInited)
			{
				return;
			}

			Buttons[index].SetData();
		}

		/// <summary>
		/// Process tab button click.
		/// </summary>
		/// <param name="tab">Tab.</param>
		protected virtual void ProcessButtonClick(TTab tab)
		{
			if (CanSelectTab(tab))
			{
				SelectTab(tab);
			}
		}

		/// <summary>
		/// Updates the buttons.
		/// </summary>
		protected virtual void UpdateButtons()
		{
			CreateButtons();

			if (tabObjects.Length == 0)
			{
				return;
			}

			if (!string.IsNullOrEmpty(DefaultTabName))
			{
				var index = Name2Index(DefaultTabName);
				if (index != -1)
				{
					SelectTab(index);
				}
				else
				{
					Debug.LogWarning(string.Format("Tab with specified DefaultTabName \"{0}\" not found. Opened the first Tab.", DefaultTabName), this);
					SelectTab(0);
				}
			}
			else
			{
				SelectTab(0);
			}
		}

		/// <summary>
		/// Creates the buttons.
		/// </summary>
		protected virtual void CreateButtons()
		{
			foreach (var button in Buttons)
			{
				EnableInteractable(button);
			}

			if (tabObjects.Length > Buttons.Count)
			{
				for (var i = Buttons.Count; i < tabObjects.Length; i++)
				{
					var defaultButton = Compatibility.Instantiate(DefaultTabButton);

					var activeButton = Compatibility.Instantiate(ActiveTabButton);

					Buttons.Add(new TabButtonInfo(this, defaultButton, activeButton));
				}
			}

			// delete existing buttons if necessary
			if (tabObjects.Length < Buttons.Count)
			{
				for (var i = Buttons.Count - 1; i > tabObjects.Length - 1; i--)
				{
					Buttons[i].Destroy();

					Buttons.RemoveAt(i);
				}
			}

			for (int i = 0; i < Buttons.Count; i++)
			{
				SetButtonName(Buttons[i], i);
			}
		}

		/// <summary>
		/// Activate button.
		/// </summary>
		/// <param name="button">Button.</param>
		protected virtual void DefaultState(TabButtonInfo button) => button.Default();

		/// <summary>
		/// Enable button interactions.
		/// </summary>
		/// <param name="button">Button.</param>
		protected virtual void EnableInteractable(TabButtonInfo button) => button.EnableInteractable();

		/// <summary>
		/// Sets the name of the button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="index">Index.</param>
		protected virtual void SetButtonName(TabButtonInfo button, int index) => button.SetTab(TabObjects[index]);

		/// <summary>
		/// Get tab by name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <returns>Tab.</returns>
		public TTab GetTabByName(string name)
		{
			var index = Name2Index(name);

			return (index == -1) ? null : tabObjects[index];
		}

		/// <summary>
		/// Get index for the specified Tab name.
		/// </summary>
		/// <param name="name">Tab name.</param>
		/// <returns>Index.</returns>
		protected int Name2Index(string name)
		{
			for (int i = 0; i < tabObjects.Length; i++)
			{
				if (tabObjects[i].Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Selects the tab.
		/// </summary>
		/// <param name="tabName">Tab name.</param>
		public virtual void SelectTab(string tabName)
		{
			var index = Name2Index(tabName);
			if (index == -1)
			{
				throw new ArgumentException(string.Format("Tab with specified name \"{0}\" not found.", tabName));
			}

			SelectTab(index);
		}

		/// <summary>
		/// Selects the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void SelectTab(TTab tab)
		{
			var index = Array.IndexOf(tabObjects, tab);
			if (index == -1)
			{
				throw new ArgumentException(string.Format("Tab with name \"{0}\" not found.", tab.Name));
			}

			SelectTab(index);
		}

		/// <summary>
		/// Selects the tab.
		/// </summary>
		/// <param name="index">Tab index.</param>
		public override void SelectTab(int index)
		{
			if ((index < 0) || (index >= tabObjects.Length))
			{
				throw new ArgumentException(string.Format("Invalid tab index \"{0}\"; should be in range 0..{1}", index.ToString(), (tabObjects.Length - 1).ToString()));
			}

			var previous = SelectedTab;
			SelectedTab = tabObjects[index];

			if (KeepTabsActive)
			{
				tabObjects[index].TabObject.transform.SetAsLastSibling();
			}
			else
			{
				foreach (var t in tabObjects)
				{
					DeactivateTab(t);
				}

				tabObjects[index].TabObject.SetActive(true);
			}

			foreach (var button in Buttons)
			{
				DefaultState(button);
			}

			Buttons[index].Active();
			OnTabSelect.Invoke(index);

			if (Animation)
			{
				if (CurrentAnimation != null)
				{
					StopCoroutine(CurrentAnimation);
				}

				var data = new AnimationData(previous, SelectedTab, AnimationDuration, UnscaledTime);
				CurrentAnimation = CustomAnimation != null ? CustomAnimation(data) : AnimateToggle(data);
				StartCoroutine(CurrentAnimation);
			}
		}

		/// <summary>
		/// Animate tabs toggle.
		/// </summary>
		/// <param name="data">Animation data.</param>
		/// <returns>Animation.</returns>
		protected virtual IEnumerator AnimateToggle(AnimationData data)
		{
			if (data.HasPreviousTab)
			{
				var previous_go = data.PreviousTab.TabObject;
				previous_go.SetActive(true);
				previous_go.transform.SetAsLastSibling();
				var previous_bg = previous_go.GetComponent<Graphic>();
				previous_bg.canvasRenderer.SetColor(Color.white);
				previous_bg.CrossFadeAlpha(0f, data.Duration, data.UnscaledTime);

				var current_go = data.CurrentTab.TabObject;
				current_go.transform.SetAsLastSibling();
				var current_bg = current_go.GetComponent<Graphic>();
				current_bg.canvasRenderer.SetColor(new Color(1f, 1f, 1f, 0f));
				current_bg.CrossFadeAlpha(1f, data.Duration, data.UnscaledTime);

				yield return WaitForSecondsCustom.Instance(data.Duration, data.UnscaledTime);

				if (!KeepTabsActive)
				{
					previous_go.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Deactivate tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		protected virtual void DeactivateTab(TTab tab) => tab.TabObject.SetActive(false);

		/// <summary>
		/// Sets the name of the button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="index">Index.</param>
		[Obsolete("No more used since button.SetData() is available in the base class.")]
		protected virtual void SetButtonData(TButton button, int index) => button.SetData(TabObjects[index]);

		/// <summary>
		/// Disable the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void DisableTab(TTab tab)
		{
			var i = Array.IndexOf(TabObjects, tab);
			if (i != -1)
			{
				Buttons[i].DisableInteractable();
			}
		}

		/// <summary>
		/// Enable the tab.
		/// </summary>
		/// <param name="tab">Tab.</param>
		public virtual void EnableTab(TTab tab)
		{
			var i = Array.IndexOf(TabObjects, tab);
			if (i != -1)
			{
				Buttons[i].EnableInteractable();
			}
		}

		#if UITHEMES_INSTALLED
		#region IStylable implementation

		/// <inheritdoc/>
		public virtual bool SetStyle(StyleTabs styleTyped, Style style)
		{
			if (DefaultTabButton != null)
			{
				styleTyped.DefaultButton.ApplyTo(DefaultTabButton.gameObject);
			}

			if (ActiveTabButton != null)
			{
				styleTyped.ActiveButton.ApplyTo(ActiveTabButton.gameObject);
			}

			for (int i = 0; i < Buttons.Count; i++)
			{
				Buttons[i].SetStyle(styleTyped);
			}

			for (int i = 0; i < tabObjects.Length; i++)
			{
				var tab = tabObjects[i];
				if (tab.TabObject != null)
				{
					styleTyped.ContentBackground.ApplyTo(tab.TabObject.GetComponent<Image>());
					style.ApplyForChildren(tab.TabObject);
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public virtual bool GetStyle(StyleTabs styleTyped, Style style)
		{
			if (DefaultTabButton != null)
			{
				styleTyped.DefaultButton.GetFrom(DefaultTabButton.gameObject);
			}

			if (ActiveTabButton != null)
			{
				styleTyped.ActiveButton.GetFrom(ActiveTabButton.gameObject);
			}

			for (int i = 0; i < tabObjects.Length; i++)
			{
				var tab = tabObjects[i];
				if (tab.TabObject != null)
				{
					styleTyped.ContentBackground.GetFrom(tab.TabObject.GetComponent<Image>());
					style.GetFromChildren(tab.TabObject);
				}
			}

			return true;
		}
		#endregion
		#endif
	}
}