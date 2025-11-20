using UnityEngine.TextCore.Text;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal static class TextUtilities
{
	public static float ComputeTextScaling(Matrix4x4 worldMatrix, float pixelsPerPoint)
	{
		Vector3 vector = new Vector3(worldMatrix.m00, worldMatrix.m10, worldMatrix.m20);
		Vector3 vector2 = new Vector3(worldMatrix.m01, worldMatrix.m11, worldMatrix.m21);
		float num = (vector.magnitude + vector2.magnitude) / 2f;
		return num * pixelsPerPoint;
	}

	internal static Vector2 MeasureVisualElementTextSize(VisualElement ve, string textToMeasure, float width, VisualElement.MeasureMode widthMode, float height, VisualElement.MeasureMode heightMode, ITextHandle textHandle)
	{
		float x = float.NaN;
		float y = float.NaN;
		if (textToMeasure == null || !IsFontAssigned(ve))
		{
			return new Vector2(x, y);
		}
		Vector3 vector = ve.ComputeGlobalScale();
		if (vector.x + vector.y <= 0f || ve.scaledPixelsPerPoint <= 0f)
		{
			return Vector2.zero;
		}
		float scaledPixelsPerPoint = ve.scaledPixelsPerPoint;
		if (widthMode == VisualElement.MeasureMode.Exactly)
		{
			x = width;
		}
		else
		{
			MeshGenerationContextUtils.TextParams parms = MeshGenerationContextUtils.TextParams.MakeStyleBased(ve, textToMeasure);
			parms.wordWrap = false;
			parms.rect = new Rect(parms.rect.x, parms.rect.y, width, height);
			x = textHandle.ComputeTextWidth(parms, scaledPixelsPerPoint);
			if (widthMode == VisualElement.MeasureMode.AtMost)
			{
				x = Mathf.Min(x, width);
			}
		}
		if (heightMode == VisualElement.MeasureMode.Exactly)
		{
			y = height;
		}
		else
		{
			MeshGenerationContextUtils.TextParams parms2 = MeshGenerationContextUtils.TextParams.MakeStyleBased(ve, textToMeasure);
			parms2.wordWrapWidth = x;
			parms2.rect = new Rect(parms2.rect.x, parms2.rect.y, width, height);
			y = textHandle.ComputeTextHeight(parms2, scaledPixelsPerPoint);
			if (heightMode == VisualElement.MeasureMode.AtMost)
			{
				y = Mathf.Min(y, height);
			}
		}
		float x2 = AlignmentUtils.CeilToPixelGrid(x, scaledPixelsPerPoint);
		float y2 = AlignmentUtils.CeilToPixelGrid(y, scaledPixelsPerPoint);
		Vector2 vector2 = new Vector2(x2, y2);
		textHandle.MeasuredSizes = new Vector2(x, y);
		textHandle.RoundedSizes = vector2;
		return vector2;
	}

	internal static FontAsset GetFontAsset(MeshGenerationContextUtils.TextParams textParam)
	{
		PanelTextSettings textSettingsFrom = GetTextSettingsFrom(textParam);
		if (textParam.fontDefinition.fontAsset != null)
		{
			return textParam.fontDefinition.fontAsset;
		}
		if (textParam.fontDefinition.font != null)
		{
			return textSettingsFrom.GetCachedFontAsset(textParam.fontDefinition.font);
		}
		return textSettingsFrom.GetCachedFontAsset(textParam.font);
	}

	internal static FontAsset GetFontAsset(VisualElement ve)
	{
		if (ve.computedStyle.unityFontDefinition.fontAsset != null)
		{
			return ve.computedStyle.unityFontDefinition.fontAsset;
		}
		PanelTextSettings textSettingsFrom = GetTextSettingsFrom(ve);
		if (ve.computedStyle.unityFontDefinition.font != null)
		{
			return textSettingsFrom.GetCachedFontAsset(ve.computedStyle.unityFontDefinition.font);
		}
		return textSettingsFrom.GetCachedFontAsset(ve.computedStyle.unityFont);
	}

	internal static Font GetFont(MeshGenerationContextUtils.TextParams textParam)
	{
		if (textParam.fontDefinition.font != null)
		{
			return textParam.fontDefinition.font;
		}
		if (textParam.font != null)
		{
			return textParam.font;
		}
		return textParam.fontDefinition.fontAsset?.sourceFontFile;
	}

	internal static Font GetFont(VisualElement ve)
	{
		ComputedStyle computedStyle = ve.computedStyle;
		if (computedStyle.unityFontDefinition.font != null)
		{
			return computedStyle.unityFontDefinition.font;
		}
		if (computedStyle.unityFont != null)
		{
			return computedStyle.unityFont;
		}
		return computedStyle.unityFontDefinition.fontAsset?.sourceFontFile;
	}

	internal static bool IsFontAssigned(VisualElement ve)
	{
		return ve.computedStyle.unityFont != null || !ve.computedStyle.unityFontDefinition.IsEmpty();
	}

	internal static bool IsFontAssigned(MeshGenerationContextUtils.TextParams textParams)
	{
		return textParams.font != null || !textParams.fontDefinition.IsEmpty();
	}

	internal static PanelTextSettings GetTextSettingsFrom(VisualElement ve)
	{
		if (ve.panel is RuntimePanel runtimePanel)
		{
			return runtimePanel.panelSettings.textSettings ?? PanelTextSettings.defaultPanelTextSettings;
		}
		return PanelTextSettings.defaultPanelTextSettings;
	}

	internal static PanelTextSettings GetTextSettingsFrom(MeshGenerationContextUtils.TextParams textParam)
	{
		if (textParam.panel is RuntimePanel runtimePanel)
		{
			return runtimePanel.panelSettings.textSettings ?? PanelTextSettings.defaultPanelTextSettings;
		}
		return PanelTextSettings.defaultPanelTextSettings;
	}

	internal static TextCoreSettings GetTextCoreSettingsForElement(VisualElement ve)
	{
		FontAsset fontAsset = GetFontAsset(ve);
		if (fontAsset == null)
		{
			return default(TextCoreSettings);
		}
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		ComputedStyle computedStyle = ve.computedStyle;
		float num = 1f / (float)fontAsset.atlasPadding;
		float num2 = (float)fontAsset.faceInfo.pointSize / ve.computedStyle.fontSize.value;
		float num3 = num * num2;
		float num4 = Mathf.Max(0f, resolvedStyle.unityTextOutlineWidth * num3);
		float underlaySoftness = Mathf.Max(0f, computedStyle.textShadow.blurRadius * num3);
		Vector2 underlayOffset = computedStyle.textShadow.offset * num3;
		Color color = resolvedStyle.color;
		Color unityTextOutlineColor = resolvedStyle.unityTextOutlineColor;
		if (num4 < 1E-30f)
		{
			unityTextOutlineColor.a = 0f;
		}
		return new TextCoreSettings
		{
			faceColor = color,
			outlineColor = unityTextOutlineColor,
			outlineWidth = num4,
			underlayColor = computedStyle.textShadow.color,
			underlayOffset = underlayOffset,
			underlaySoftness = underlaySoftness
		};
	}
}
