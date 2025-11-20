using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

internal interface ITextHandle
{
	Vector2 MeasuredSizes { get; set; }

	Vector2 RoundedSizes { get; set; }

	Vector2 GetCursorPosition(CursorPositionStylePainterParameters parms, float scaling);

	float ComputeTextWidth(MeshGenerationContextUtils.TextParams parms, float scaling);

	float ComputeTextHeight(MeshGenerationContextUtils.TextParams parms, float scaling);

	float GetLineHeight(int characterIndex, MeshGenerationContextUtils.TextParams textParams, float textScaling, float pixelPerPoint);

	TextInfo Update(MeshGenerationContextUtils.TextParams parms, float pixelsPerPoint);

	int VerticesCount(MeshGenerationContextUtils.TextParams parms, float pixelPerPoint);

	ITextHandle New();

	bool IsLegacy();

	void SetDirty();

	bool IsElided();
}
