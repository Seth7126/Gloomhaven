using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

internal struct TextHandle : ITextHandle
{
	internal ITextHandle textHandle;

	public Vector2 MeasuredSizes
	{
		get
		{
			return textHandle.MeasuredSizes;
		}
		set
		{
			textHandle.MeasuredSizes = value;
		}
	}

	public Vector2 RoundedSizes
	{
		get
		{
			return textHandle.RoundedSizes;
		}
		set
		{
			textHandle.RoundedSizes = value;
		}
	}

	public Vector2 GetCursorPosition(CursorPositionStylePainterParameters parms, float scaling)
	{
		return textHandle.GetCursorPosition(parms, scaling);
	}

	public float GetLineHeight(int characterIndex, MeshGenerationContextUtils.TextParams textParams, float textScaling, float pixelPerPoint)
	{
		return textHandle.GetLineHeight(characterIndex, textParams, textScaling, pixelPerPoint);
	}

	public float ComputeTextWidth(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		return textHandle.ComputeTextWidth(parms, scaling);
	}

	public float ComputeTextHeight(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		return textHandle.ComputeTextHeight(parms, scaling);
	}

	public TextInfo Update(MeshGenerationContextUtils.TextParams parms, float pixelsPerPoint)
	{
		return textHandle.Update(parms, pixelsPerPoint);
	}

	public int VerticesCount(MeshGenerationContextUtils.TextParams parms, float pixelPerPoint)
	{
		return textHandle.VerticesCount(parms, pixelPerPoint);
	}

	public ITextHandle New()
	{
		return textHandle.New();
	}

	public bool IsLegacy()
	{
		return textHandle.IsLegacy();
	}

	public void SetDirty()
	{
		textHandle.SetDirty();
	}

	public bool IsElided()
	{
		return textHandle.IsElided();
	}
}
