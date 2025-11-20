using System;
using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

public class Image : VisualElement
{
	public new class UxmlFactory : UxmlFactory<Image, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}
	}

	private ScaleMode m_ScaleMode;

	private Texture m_Image;

	private Sprite m_Sprite;

	private VectorImage m_VectorImage;

	private Rect m_UV;

	private Color m_TintColor;

	private bool m_ImageIsInline;

	private bool m_ScaleModeIsInline;

	private bool m_TintColorIsInline;

	public static readonly string ussClassName = "unity-image";

	private static CustomStyleProperty<Texture2D> s_ImageProperty = new CustomStyleProperty<Texture2D>("--unity-image");

	private static CustomStyleProperty<Sprite> s_SpriteProperty = new CustomStyleProperty<Sprite>("--unity-image");

	private static CustomStyleProperty<VectorImage> s_VectorImageProperty = new CustomStyleProperty<VectorImage>("--unity-image");

	private static CustomStyleProperty<string> s_ScaleModeProperty = new CustomStyleProperty<string>("--unity-image-size");

	private static CustomStyleProperty<Color> s_TintColorProperty = new CustomStyleProperty<Color>("--unity-image-tint-color");

	public Texture image
	{
		get
		{
			return m_Image;
		}
		set
		{
			if (value != null && (m_Sprite != null || m_VectorImage != null))
			{
				string text = ((m_Sprite != null) ? "sprite" : "vector image");
				Debug.LogWarning("Image object already has a background, removing " + text);
				m_Sprite = null;
				m_VectorImage = null;
			}
			m_ImageIsInline = value != null;
			if (m_Image != value)
			{
				m_Image = value;
				IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
				if (m_Image == null)
				{
					m_UV = new Rect(0f, 0f, 1f, 1f);
				}
			}
		}
	}

	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (value != null && (m_Image != null || m_VectorImage != null))
			{
				string text = ((m_Image != null) ? "texture" : "vector image");
				Debug.LogWarning("Image object already has a background, removing " + text);
				m_Image = null;
				m_VectorImage = null;
			}
			m_ImageIsInline = value != null;
			if (m_Sprite != value)
			{
				m_Sprite = value;
				IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			}
		}
	}

	public VectorImage vectorImage
	{
		get
		{
			return m_VectorImage;
		}
		set
		{
			if (value != null && (m_Image != null || m_Sprite != null))
			{
				string text = ((m_Image != null) ? "texture" : "sprite");
				Debug.LogWarning("Image object already has a background, removing " + text);
				m_Image = null;
				m_Sprite = null;
			}
			m_ImageIsInline = value != null;
			if (m_VectorImage != value)
			{
				m_VectorImage = value;
				IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
				if (m_VectorImage == null)
				{
					m_UV = new Rect(0f, 0f, 1f, 1f);
				}
			}
		}
	}

	public Rect sourceRect
	{
		get
		{
			return GetSourceRect();
		}
		set
		{
			if (sprite != null)
			{
				Debug.LogError("Cannot set sourceRect on a sprite image");
			}
			else
			{
				CalculateUV(value);
			}
		}
	}

	public Rect uv
	{
		get
		{
			return m_UV;
		}
		set
		{
			m_UV = value;
		}
	}

	public ScaleMode scaleMode
	{
		get
		{
			return m_ScaleMode;
		}
		set
		{
			m_ScaleModeIsInline = true;
			SetScaleMode(value);
		}
	}

	public Color tintColor
	{
		get
		{
			return m_TintColor;
		}
		set
		{
			m_TintColorIsInline = true;
			if (m_TintColor != value)
			{
				m_TintColor = value;
				IncrementVersion(VersionChangeType.Repaint);
			}
		}
	}

	public Image()
	{
		AddToClassList(ussClassName);
		m_ScaleMode = ScaleMode.ScaleToFit;
		m_TintColor = Color.white;
		m_UV = new Rect(0f, 0f, 1f, 1f);
		base.requireMeasureFunction = true;
		RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
		base.generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(base.generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
	}

	private Vector2 GetTextureDisplaySize(Texture texture)
	{
		Vector2 result = Vector2.zero;
		if (texture != null)
		{
			result = new Vector2(texture.width, texture.height);
		}
		return result;
	}

	private Vector2 GetTextureDisplaySize(Sprite sprite)
	{
		Vector2 result = Vector2.zero;
		if (sprite != null)
		{
			result = sprite.bounds.size * sprite.pixelsPerUnit;
		}
		return result;
	}

	protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		float x = float.NaN;
		float y = float.NaN;
		if (image == null && sprite == null && vectorImage == null)
		{
			return new Vector2(x, y);
		}
		Vector2 zero = Vector2.zero;
		zero = ((image != null) ? GetTextureDisplaySize(image) : ((!(sprite != null)) ? vectorImage.size : GetTextureDisplaySize(sprite)));
		Rect rect = sourceRect;
		bool flag = rect != Rect.zero;
		x = (flag ? rect.width : zero.x);
		y = (flag ? rect.height : zero.y);
		if (widthMode == MeasureMode.AtMost)
		{
			x = Mathf.Min(x, desiredWidth);
		}
		if (heightMode == MeasureMode.AtMost)
		{
			y = Mathf.Min(y, desiredHeight);
		}
		return new Vector2(x, y);
	}

	private void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		if (!(image == null) || !(sprite == null) || !(vectorImage == null))
		{
			MeshGenerationContextUtils.RectangleParams rectParams = default(MeshGenerationContextUtils.RectangleParams);
			if (image != null)
			{
				rectParams = MeshGenerationContextUtils.RectangleParams.MakeTextured(base.contentRect, uv, image, scaleMode, base.panel.contextType);
			}
			else if (sprite != null)
			{
				Vector4 slices = Vector4.zero;
				rectParams = MeshGenerationContextUtils.RectangleParams.MakeSprite(base.contentRect, sprite, scaleMode, base.panel.contextType, hasRadius: false, ref slices);
			}
			else if (vectorImage != null)
			{
				rectParams = MeshGenerationContextUtils.RectangleParams.MakeVectorTextured(base.contentRect, uv, vectorImage, scaleMode, base.panel.contextType);
			}
			rectParams.color = tintColor;
			mgc.Rectangle(rectParams);
		}
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
	{
		Texture2D value = null;
		Sprite value2 = null;
		VectorImage value3 = null;
		Color value4 = Color.white;
		ICustomStyle customStyle = e.customStyle;
		if (!m_ImageIsInline && customStyle.TryGetValue(s_ImageProperty, out value))
		{
			m_Image = value;
			m_Sprite = null;
			m_VectorImage = null;
		}
		if (!m_ImageIsInline && customStyle.TryGetValue(s_SpriteProperty, out value2))
		{
			m_Image = null;
			m_Sprite = value2;
			m_VectorImage = null;
		}
		if (!m_ImageIsInline && customStyle.TryGetValue(s_VectorImageProperty, out value3))
		{
			m_Image = null;
			m_Sprite = null;
			m_VectorImage = value3;
		}
		if (!m_ScaleModeIsInline && customStyle.TryGetValue(s_ScaleModeProperty, out var value5))
		{
			StylePropertyUtil.TryGetEnumIntValue(StyleEnumType.ScaleMode, value5, out var intValue);
			SetScaleMode((ScaleMode)intValue);
		}
		if (!m_TintColorIsInline && customStyle.TryGetValue(s_TintColorProperty, out value4))
		{
			m_TintColor = value4;
		}
	}

	private void SetScaleMode(ScaleMode mode)
	{
		if (m_ScaleMode != mode)
		{
			m_ScaleMode = mode;
			IncrementVersion(VersionChangeType.Repaint);
		}
	}

	private void CalculateUV(Rect srcRect)
	{
		m_UV = new Rect(0f, 0f, 1f, 1f);
		Vector2 vector = Vector2.zero;
		Texture texture = image;
		if (texture != null)
		{
			vector = GetTextureDisplaySize(texture);
		}
		VectorImage vectorImage = this.vectorImage;
		if (vectorImage != null)
		{
			vector = vectorImage.size;
		}
		if (vector != Vector2.zero)
		{
			m_UV.x = srcRect.x / vector.x;
			m_UV.width = srcRect.width / vector.x;
			m_UV.height = srcRect.height / vector.y;
			m_UV.y = 1f - m_UV.height - srcRect.y / vector.y;
		}
	}

	private Rect GetSourceRect()
	{
		Rect zero = Rect.zero;
		Vector2 vector = Vector2.zero;
		Texture texture = image;
		if (texture != null)
		{
			vector = GetTextureDisplaySize(texture);
		}
		VectorImage vectorImage = this.vectorImage;
		if (vectorImage != null)
		{
			vector = vectorImage.size;
		}
		if (vector != Vector2.zero)
		{
			zero.x = uv.x * vector.x;
			zero.width = uv.width * vector.x;
			zero.y = (1f - uv.y - uv.height) * vector.y;
			zero.height = uv.height * vector.y;
		}
		return zero;
	}
}
