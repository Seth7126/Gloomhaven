using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

internal struct TextCoreHandle : ITextHandle
{
	private Vector2 m_PreferredSize;

	private int m_PreviousGenerationSettingsHash;

	private UnityEngine.TextCore.Text.TextGenerationSettings m_CurrentGenerationSettings;

	private static UnityEngine.TextCore.Text.TextGenerationSettings s_LayoutSettings = new UnityEngine.TextCore.Text.TextGenerationSettings();

	private TextInfo m_TextInfoMesh;

	private static TextInfo s_TextInfoLayout;

	private bool isDirty;

	public Vector2 MeasuredSizes { get; set; }

	public Vector2 RoundedSizes { get; set; }

	internal TextInfo textInfoMesh
	{
		get
		{
			if (m_TextInfoMesh == null)
			{
				m_TextInfoMesh = new TextInfo();
			}
			return m_TextInfoMesh;
		}
	}

	internal static TextInfo textInfoLayout
	{
		get
		{
			if (s_TextInfoLayout == null)
			{
				s_TextInfoLayout = new TextInfo();
			}
			return s_TextInfoLayout;
		}
	}

	public static ITextHandle New()
	{
		return new TextCoreHandle
		{
			m_CurrentGenerationSettings = new UnityEngine.TextCore.Text.TextGenerationSettings()
		};
	}

	internal bool IsTextInfoAllocated()
	{
		return m_TextInfoMesh != null;
	}

	public bool IsLegacy()
	{
		return false;
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public bool IsDirty(MeshGenerationContextUtils.TextParams parms)
	{
		int hashCode = parms.GetHashCode();
		if (m_PreviousGenerationSettingsHash == hashCode && !isDirty)
		{
			return false;
		}
		m_PreviousGenerationSettingsHash = hashCode;
		isDirty = false;
		return true;
	}

	public Vector2 GetCursorPosition(CursorPositionStylePainterParameters parms, float scaling)
	{
		return UnityEngine.TextCore.Text.TextGenerator.GetCursorPosition(textInfoMesh, parms.rect, parms.cursorIndex);
	}

	public float ComputeTextWidth(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		UpdatePreferredValues(parms);
		return m_PreferredSize.x;
	}

	public float ComputeTextHeight(MeshGenerationContextUtils.TextParams parms, float scaling)
	{
		UpdatePreferredValues(parms);
		return m_PreferredSize.y;
	}

	public float GetLineHeight(int characterIndex, MeshGenerationContextUtils.TextParams textParams, float textScaling, float pixelPerPoint)
	{
		if (m_TextInfoMesh == null || m_TextInfoMesh.characterCount == 0)
		{
			Update(textParams, pixelPerPoint);
		}
		return m_TextInfoMesh.lineInfo[0].lineHeight;
	}

	public int VerticesCount(MeshGenerationContextUtils.TextParams parms, float pixelPerPoint)
	{
		Update(parms, pixelPerPoint);
		int num = 0;
		MeshInfo[] meshInfo = textInfoMesh.meshInfo;
		for (int i = 0; i < meshInfo.Length; i++)
		{
			MeshInfo meshInfo2 = meshInfo[i];
			num += meshInfo2.vertexCount;
		}
		return num;
	}

	ITextHandle ITextHandle.New()
	{
		return New();
	}

	public TextInfo Update(MeshGenerationContextUtils.TextParams parms, float pixelsPerPoint)
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
		if (!IsDirty(parms))
		{
			return textInfoMesh;
		}
		UpdateGenerationSettingsCommon(parms, m_CurrentGenerationSettings);
		m_CurrentGenerationSettings.color = parms.fontColor;
		m_CurrentGenerationSettings.inverseYAxis = true;
		textInfoMesh.isDirty = true;
		UnityEngine.TextCore.Text.TextGenerator.GenerateText(m_CurrentGenerationSettings, textInfoMesh);
		return textInfoMesh;
	}

	private void UpdatePreferredValues(MeshGenerationContextUtils.TextParams parms)
	{
		Vector2 size = parms.rect.size;
		parms.rect = new Rect(Vector2.zero, size);
		UpdateGenerationSettingsCommon(parms, s_LayoutSettings);
		m_PreferredSize = UnityEngine.TextCore.Text.TextGenerator.GetPreferredValues(s_LayoutSettings, textInfoLayout);
	}

	private static TextOverflowMode GetTextOverflowMode(MeshGenerationContextUtils.TextParams textParams)
	{
		if (textParams.textOverflow == TextOverflow.Clip)
		{
			return TextOverflowMode.Masking;
		}
		if (textParams.textOverflow != TextOverflow.Ellipsis)
		{
			return TextOverflowMode.Overflow;
		}
		if (!textParams.wordWrap && textParams.overflow == OverflowInternal.Hidden)
		{
			return TextOverflowMode.Ellipsis;
		}
		return TextOverflowMode.Overflow;
	}

	private static void UpdateGenerationSettingsCommon(MeshGenerationContextUtils.TextParams painterParams, UnityEngine.TextCore.Text.TextGenerationSettings settings)
	{
		if (settings.textSettings == null)
		{
			settings.textSettings = TextUtilities.GetTextSettingsFrom(painterParams);
			if (settings.textSettings == null)
			{
				return;
			}
		}
		settings.fontAsset = TextUtilities.GetFontAsset(painterParams);
		if (!(settings.fontAsset == null))
		{
			settings.material = settings.fontAsset.material;
			settings.screenRect = painterParams.rect;
			settings.text = (string.IsNullOrEmpty(painterParams.text) ? "\u200b" : (painterParams.text + "\u200b"));
			settings.fontSize = ((painterParams.fontSize > 0) ? painterParams.fontSize : settings.fontAsset.faceInfo.pointSize);
			settings.fontStyle = TextGeneratorUtilities.LegacyStyleToNewStyle(painterParams.fontStyle);
			settings.textAlignment = TextGeneratorUtilities.LegacyAlignmentToNewAlignment(painterParams.anchor);
			settings.wordWrap = painterParams.wordWrap;
			settings.wordWrappingRatio = 0.4f;
			settings.richText = painterParams.richText;
			settings.overflowMode = GetTextOverflowMode(painterParams);
			settings.characterSpacing = painterParams.letterSpacing.value;
			settings.wordSpacing = painterParams.wordSpacing.value;
			settings.paragraphSpacing = painterParams.paragraphSpacing.value;
		}
	}

	public bool IsElided()
	{
		if (m_TextInfoMesh == null)
		{
			return false;
		}
		if (m_TextInfoMesh.characterCount == 0)
		{
			return true;
		}
		return m_TextInfoMesh.textElementInfo[m_TextInfoMesh.characterCount - 1].character == '…';
	}
}
