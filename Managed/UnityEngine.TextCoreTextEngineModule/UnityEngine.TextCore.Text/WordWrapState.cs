namespace UnityEngine.TextCore.Text;

internal struct WordWrapState
{
	public int previousWordBreak;

	public int totalCharacterCount;

	public int visibleCharacterCount;

	public int visibleSpriteCount;

	public int visibleLinkCount;

	public int firstCharacterIndex;

	public int firstVisibleCharacterIndex;

	public int lastCharacterIndex;

	public int lastVisibleCharIndex;

	public int lineNumber;

	public float maxCapHeight;

	public float maxAscender;

	public float maxDescender;

	public float maxLineAscender;

	public float maxLineDescender;

	public float previousLineAscender;

	public float xAdvance;

	public float preferredWidth;

	public float preferredHeight;

	public float previousLineScale;

	public int wordCount;

	public FontStyles fontStyle;

	public float fontScale;

	public float fontScaleMultiplier;

	public float currentFontSize;

	public float baselineOffset;

	public float lineOffset;

	public TextInfo textInfo;

	public LineInfo lineInfo;

	public Color32 vertexColor;

	public Color32 underlineColor;

	public Color32 strikethroughColor;

	public Color32 highlightColor;

	public FontStyleStack basicStyleStack;

	public TextProcessingStack<Color32> colorStack;

	public TextProcessingStack<Color32> underlineColorStack;

	public TextProcessingStack<Color32> strikethroughColorStack;

	public TextProcessingStack<Color32> highlightColorStack;

	public TextProcessingStack<TextColorGradient> colorGradientStack;

	public TextProcessingStack<float> sizeStack;

	public TextProcessingStack<float> indentStack;

	public TextProcessingStack<TextFontWeight> fontWeightStack;

	public TextProcessingStack<int> styleStack;

	public TextProcessingStack<float> baselineStack;

	public TextProcessingStack<int> actionStack;

	public TextProcessingStack<MaterialReference> materialReferenceStack;

	public TextProcessingStack<TextAlignment> lineJustificationStack;

	public int spriteAnimationId;

	public FontAsset currentFontAsset;

	public SpriteAsset currentSpriteAsset;

	public Material currentMaterial;

	public int currentMaterialIndex;

	public Extents meshExtents;

	public bool tagNoParsing;

	public bool isNonBreakingSpace;
}
