namespace UIWidgets
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Border effect.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(Graphic))]
	[AddComponentMenu("UI/New UI Widgets/Effects/Border Effect")]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/effects/border.html")]
	public class BorderEffect : UVEffect
	{
		[SerializeField]
		Color borderColor = Color.white;

		/// <summary>
		/// Border color.
		/// </summary>
		public Color BorderColor
		{
			get => borderColor;

			set
			{
				borderColor = value;
				UpdateMaterial();
			}
		}

		[SerializeField]
		[Tooltip("Make the background transparent.")]
		bool transparentBackground = false;

		/// <summary>
		/// Make the background transparent.
		/// </summary>
		public bool TransparentBackground
		{
			get => transparentBackground;

			set
			{
				transparentBackground = value;
				UpdateMaterial();
			}
		}

		[SerializeField]
		Vector2 horizontalBorders = new Vector2(1f, 1f);

		/// <summary>
		/// Horizontal borders.
		/// </summary>
		public Vector2 HorizontalBorders
		{
			get => horizontalBorders;

			set
			{
				horizontalBorders = value;
				UpdateMaterial();
			}
		}

		[SerializeField]
		Vector2 verticalBorders = new Vector2(1f, 1f);

		/// <summary>
		/// Thickness.
		/// </summary>
		public Vector2 VerticalBorders
		{
			get => verticalBorders;

			set
			{
				verticalBorders = value;
				UpdateMaterial();
			}
		}

		/// <summary>
		/// Borders.
		/// </summary>
		protected Vector4 Borders
		{
			get
			{
				var size = RectTransform.rect.size;
				return new Vector4(horizontalBorders.x / size.x, horizontalBorders.y / size.x, verticalBorders.x / size.y, verticalBorders.y / size.y);
			}
		}

		/// <inheritdoc/>
		protected override void OnEnable()
		{
			base.OnEnable();

			Mode = UVMode.One;
		}

		/// <summary>
		/// Set material properties.
		/// </summary>
		protected override void SetMaterialProperties()
		{
			if (EffectMaterial != null)
			{
				var size = RectTransform.rect.size;

				EffectMaterial.SetColor(StaticFields.Instance.ShaderBorderColor, BorderColor);
				EffectMaterial.SetVector(StaticFields.Instance.ShaderBorders, Borders);
				EffectMaterial.SetFloat(StaticFields.Instance.ShaderTransparent, transparentBackground ? 1 : 0);
			}
		}
	}
}