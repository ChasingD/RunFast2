#if UITHEMES_INSTALLED
namespace UIWidgets
{
	using UIThemes.Wrappers;
	using UnityEngine;

	/// <summary>
	/// Wrapper for the TabsIcons.
	/// </summary>
	public class TabsIconsWrapper : Wrapper<Sprite, TabsIcons>
	{
		readonly bool activeIcon;

		readonly int index;

		/// <summary>
		/// Initializes a new instance of the <see cref="TabsIconsWrapper"/> class.
		/// </summary>
		/// <param name="activeIcon">Wrapper for the active or default icon.</param>
		/// <param name="index">Index of the tab in the TabsObjects properties.</param>
		public TabsIconsWrapper(bool activeIcon, int index)
		{
			this.activeIcon = activeIcon;
			this.index = index;

			var icon = activeIcon
				? nameof(TabIcons.IconActive)
				: nameof(TabIcons.IconDefault);
			Name = string.Format("Tab[{0}].{1}", index, icon);
		}

		/// <inheritdoc/>
		protected override Sprite Get(TabsIcons widget)
		{
			if (!Active(widget))
			{
				return null;
			}

			var tab = widget.TabObjects[index];
			return activeIcon ? tab.IconActive : tab.IconDefault;
		}

		/// <inheritdoc/>
		protected override void Set(TabsIcons widget, Sprite value)
		{
			if (!Active(widget))
			{
				return;
			}

			var tab = widget.TabObjects[index];
			if (activeIcon)
			{
				tab.IconActive = value;
			}
			else
			{
				tab.IconDefault = value;
			}

			widget.UpdateButtonData(index);
		}

		/// <inheritdoc/>
		protected override bool Active(TabsIcons widget) => index < widget.TabObjects.Length;

		/// <inheritdoc/>
		protected override bool ShouldAttachValue(TabsIcons widget) => Get(widget) != null;
	}
}
#endif