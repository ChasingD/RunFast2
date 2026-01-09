namespace UIWidgets
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for the lines drawer.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(Graphic))]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/effects/lines-drawer.html")]
	public abstract class LinesDrawerBase : UVEffect
	{
		[SerializeField]
		Color lineColor = Color.black;

		/// <summary>
		/// Line color.
		/// </summary>
		public Color LineColor
		{
			get
			{
				return lineColor;
			}

			set
			{
				if (lineColor != value)
				{
					lineColor = value;
					UpdateMaterial();
				}
			}
		}

		[SerializeField]
		float lineThickness = 1f;

		/// <summary>
		/// Line thickness.
		/// </summary>
		public float LineThickness
		{
			get
			{
				return lineThickness;
			}

			set
			{
				if (lineThickness != value)
				{
					lineThickness = value;
					UpdateMaterial();
				}
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
			get
			{
				return transparentBackground;
			}

			set
			{
				if (transparentBackground != value)
				{
					transparentBackground = value;
					UpdateMaterial();
				}
			}
		}

		/// <summary>
		/// Horizontal lines.
		/// </summary>
		protected List<float> HorizontalLines = new List<float>();

		/// <summary>
		/// Vertical lines.
		/// </summary>
		protected List<float> VerticalLines = new List<float>();

		/// <summary>
		/// Shader horizontal lines.
		/// </summary>
		protected List<float> ShaderHorizontalLines = new List<float>(200);

		/// <summary>
		/// Shader vertical lines.
		/// </summary>
		protected List<float> ShaderVerticalLines = new List<float>(200);

		/// <summary>
		/// Max lines count. Should match with shader setting.
		/// </summary>
		protected int MaxLinesCount = 200;

		/// <inheritdoc/>
		protected override void OnEnable()
		{
			base.OnEnable();

			Mode = UVMode.One;

			UpdateLines();
		}

		/// <summary>
		/// Update lines.
		/// </summary>
		protected virtual void UpdateLines()
		{
			UpdateMaterial();
		}

		/// <summary>
		/// Set material properties.
		/// </summary>
		protected override void SetMaterialProperties()
		{
			if (EffectMaterial != null)
			{
				var size = RectTransform.rect.size;

				UpdateShaderLines(HorizontalLines, ShaderHorizontalLines);
				UpdateShaderLines(VerticalLines, ShaderVerticalLines);

				EffectMaterial.SetColor(StaticFields.Instance.ShaderLinesDrawerColor, lineColor);
				EffectMaterial.SetFloat(StaticFields.Instance.ShaderLinesDrawerThickness, lineThickness);
				EffectMaterial.SetFloat(StaticFields.Instance.ShaderLinesDrawerResolutionX, size.x);
				EffectMaterial.SetFloat(StaticFields.Instance.ShaderLinesDrawerResolutionY, size.y);
				EffectMaterial.SetFloat(StaticFields.Instance.ShaderLinesDrawerTransparent, transparentBackground ? 1 : 0);

				EffectMaterial.SetInt(StaticFields.Instance.ShaderLinesDrawerHorizontalLinesCount, HorizontalLines.Count);
				EffectMaterial.SetFloatArray(StaticFields.Instance.ShaderLinesDrawerHorizontalLines, ShaderHorizontalLines);

				EffectMaterial.SetInt(StaticFields.Instance.ShaderLinesDrawerVerticalLinesCount, VerticalLines.Count);
				EffectMaterial.SetFloatArray(StaticFields.Instance.ShaderLinesDrawerVerticalLines, ShaderVerticalLines);
			}
		}

		/// <summary>
		/// Update shader lines.
		/// </summary>
		/// <param name="lines">Lines.</param>
		/// <param name="shaderLines">Shader lines.</param>
		protected void UpdateShaderLines(List<float> lines, List<float> shaderLines)
		{
			shaderLines.Clear();
			shaderLines.AddRange(lines);

			for (int i = shaderLines.Count; i < MaxLinesCount; i++)
			{
				shaderLines.Add(0);
			}
		}
	}
}