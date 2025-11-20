#define UNITY_ASSERTIONS
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

internal struct TextNativeHandle : ITextHandle
{
	internal NativeArray<TextVertex> textVertices;

	private int m_PreviousTextParamsHash;

	public Vector2 MeasuredSizes { get; set; }

	public Vector2 RoundedSizes { get; set; }

	public static ITextHandle New()
	{
		return new TextNativeHandle
		{
			textVertices = default(NativeArray<TextVertex>)
		};
	}

	public bool IsLegacy()
	{
		return true;
	}

	public void SetDirty()
	{
	}

	ITextHandle ITextHandle.New()
	{
		return New();
	}

	public float GetLineHeight(int characterIndex, MeshGenerationContextUtils.TextParams textParams, float textScaling, float pixelPerPoint)
	{
		textParams.wordWrapWidth = 0f;
		textParams.wordWrap = false;
		return ComputeTextHeight(textParams, textScaling);
	}

	public TextInfo Update(MeshGenerationContextUtils.TextParams parms, float pixelsPerPoint)
	{
		Debug.Log("TextNative Update should not be called");
		return null;
	}

	public int VerticesCount(MeshGenerationContextUtils.TextParams parms, float pixelPerPoint)
	{
		return GetVertices(parms, pixelPerPoint).Length;
	}

	public NativeArray<TextVertex> GetVertices(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		Vector2 vector = parms.rect.size;
		if (Mathf.Abs(parms.rect.size.x - RoundedSizes.x) < 0.01f && Mathf.Abs(parms.rect.size.y - RoundedSizes.y) < 0.01f)
		{
			vector = MeasuredSizes;
			parms.wordWrapWidth = vector.x;
		}
		else
		{
			RoundedSizes = vector;
			MeasuredSizes = vector;
		}
		parms.rect = new Rect(Vector2.zero, vector);
		int hashCode = parms.GetHashCode();
		if (m_PreviousTextParamsHash == hashCode)
		{
			return textVertices;
		}
		m_PreviousTextParamsHash = hashCode;
		TextNativeSettings textNativeSettings = MeshGenerationContextUtils.TextParams.GetTextNativeSettings(parms, scaling);
		Assert.IsNotNull(textNativeSettings.font);
		textVertices = TextNative.GetVertices(textNativeSettings);
		return textVertices;
	}

	public Vector2 GetCursorPosition(CursorPositionStylePainterParameters parms, float scaling)
	{
		return TextNative.GetCursorPosition(parms.GetTextNativeSettings(scaling), parms.rect, parms.cursorIndex);
	}

	public float ComputeTextWidth(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		float num = TextNative.ComputeTextWidth(MeshGenerationContextUtils.TextParams.GetTextNativeSettings(parms, scaling));
		if (scaling != 1f && num != 0f)
		{
			return num + 0.0001f;
		}
		return num;
	}

	public float ComputeTextHeight(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		return TextNative.ComputeTextHeight(MeshGenerationContextUtils.TextParams.GetTextNativeSettings(parms, scaling));
	}

	public bool IsElided()
	{
		return false;
	}
}
