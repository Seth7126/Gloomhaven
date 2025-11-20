using System;
using System.Collections;
using TMPro;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UI/Tooltip", 58)]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(VerticalLayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class UITooltip : MonoBehaviour
{
	public enum Transition
	{
		None,
		Fade
	}

	public enum VisualState
	{
		Shown,
		Hidden
	}

	public enum Corner
	{
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight,
		Auto
	}

	public enum TextEffectType
	{
		None,
		Shadow,
		Outline
	}

	private static UITooltip mInstance;

	public const ContentSizeFitter.FitMode DefaultHorizontalFitMode = ContentSizeFitter.FitMode.Unconstrained;

	[SerializeField]
	[Tooltip("Used when no width is specified for the current tooltip display.")]
	private float m_DefaultWidth = 257f;

	[SerializeField]
	[Tooltip("Should the tooltip follow the mouse movement or anchor to the position where it was called.")]
	private bool m_followMouse;

	[SerializeField]
	[Tooltip("Tooltip offset from the pointer when not anchored to a rect.")]
	private Vector2 m_Offset = Vector2.zero;

	[SerializeField]
	[Tooltip("Tooltip offset when anchored to a rect.")]
	private Vector2 m_AnchoredOffset = Vector2.zero;

	[SerializeField]
	[Tooltip("Tooltip offset position when anchored to a rect.")]
	private Vector2 m_AchoredPositionOffset = Vector2.zero;

	[SerializeField]
	private Graphic m_AnchorGraphic;

	[SerializeField]
	private Vector2 m_AnchorGraphicOffset = Vector2.zero;

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private TweenEasing m_TransitionEasing;

	[SerializeField]
	private float m_TransitionDuration = 0.15f;

	[SerializeField]
	private bool m_screenBound;

	[SerializeField]
	private float m_screenBoundOffset = 20f;

	[SerializeField]
	private TMP_FontAsset m_TitleFont;

	[SerializeField]
	private FontStyles m_TitleFontStyle;

	[SerializeField]
	private int m_PCTitleFontSize = 16;

	[SerializeField]
	private int m_TitleFontSize = FontData.defaultFontData.fontSize;

	[SerializeField]
	private float m_TitleFontLineSpacing = FontData.defaultFontData.lineSpacing;

	[SerializeField]
	private Color m_TitleFontColor = Color.white;

	[SerializeField]
	private TextEffectType m_TitleTextEffect;

	[SerializeField]
	private Color m_TitleTextEffectColor = new Color(0f, 0f, 0f, 128f);

	[SerializeField]
	private Vector2 m_TitleTextEffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_TitleTextEffectUseGraphicAlpha = true;

	[SerializeField]
	private TMP_FontAsset m_KeywordFont;

	[SerializeField]
	private FontStyles m_KeywordFontStyle;

	[SerializeField]
	private int m_KeywordFontSize = FontData.defaultFontData.fontSize;

	[SerializeField]
	private float m_KeywordFontLineSpacing = FontData.defaultFontData.lineSpacing;

	[SerializeField]
	private Color m_KeywordFontColor = Color.white;

	[SerializeField]
	private TextEffectType m_KeywordTextEffect;

	[SerializeField]
	private Color m_KeywordTextEffectColor = new Color(0f, 0f, 0f, 128f);

	[SerializeField]
	private Vector2 m_KeywordTextEffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_KeywordTextEffectUseGraphicAlpha = true;

	[SerializeField]
	private TMP_FontAsset m_DescriptionFont;

	[SerializeField]
	private FontStyles m_DescriptionFontStyle;

	[SerializeField]
	private int m_DescriptionFontSize = FontData.defaultFontData.fontSize;

	[SerializeField]
	private float m_DescriptionFontLineSpacing = FontData.defaultFontData.lineSpacing;

	[SerializeField]
	private Color m_DescriptionFontColor = Color.white;

	[SerializeField]
	private TextEffectType m_DescriptionTextEffect;

	[SerializeField]
	private Color m_DescriptionTextEffectColor = new Color(0f, 0f, 0f, 128f);

	[SerializeField]
	private Vector2 m_DescriptionTextEffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_DescriptionTextEffectUseGraphicAlpha = true;

	[SerializeField]
	private TMP_FontAsset m_AttributeFont;

	[SerializeField]
	private FontStyles m_AttributeFontStyle;

	[SerializeField]
	private int m_AttributeFontSize = FontData.defaultFontData.fontSize;

	[SerializeField]
	private float m_AttributeFontLineSpacing = FontData.defaultFontData.lineSpacing;

	[SerializeField]
	private Color m_AttributeFontColor = Color.white;

	[SerializeField]
	private TextEffectType m_AttributeTextEffect;

	[SerializeField]
	private Color m_AttributeTextEffectColor = new Color(0f, 0f, 0f, 128f);

	[SerializeField]
	private Vector2 m_AttributeTextEffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_AttributeTextEffectUseGraphicAlpha = true;

	private Image m_image;

	private RectTransform m_Rect;

	private CanvasGroup m_CanvasGroup;

	private VerticalLayoutGroup m_LayoutGroup;

	private ContentSizeFitter m_SizeFitter;

	private Canvas m_Canvas;

	private VisualState m_VisualState;

	private RectTransform m_AnchorToTarget;

	private UITooltipLines m_LinesTemplate;

	private Corner targetCorner = Corner.Auto;

	private bool autoCorner = true;

	private Vector2 defaultAnchoredOffset;

	private bool defaultBackgroundImageState;

	private Vector2 defaultOffset;

	private Vector2 defaultAnchoredPositionOffset;

	[NonSerialized]
	private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

	public float defaultWidth
	{
		get
		{
			return m_DefaultWidth;
		}
		set
		{
			m_DefaultWidth = value;
		}
	}

	public bool followMouse
	{
		get
		{
			return m_followMouse;
		}
		set
		{
			m_followMouse = value;
		}
	}

	public Vector2 offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
		}
	}

	public Vector2 anchoredOffset
	{
		get
		{
			return m_AnchoredOffset;
		}
		set
		{
			m_AnchoredOffset = value;
		}
	}

	public Vector2 anchoredPositionOffset
	{
		get
		{
			return m_AchoredPositionOffset;
		}
		set
		{
			m_AchoredPositionOffset = value;
		}
	}

	public float alpha
	{
		get
		{
			if (!(m_CanvasGroup != null))
			{
				return 1f;
			}
			return m_CanvasGroup.alpha;
		}
	}

	public VisualState visualState => m_VisualState;

	public Camera uiCamera
	{
		get
		{
			if (m_Canvas == null)
			{
				return null;
			}
			if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (m_Canvas.renderMode == RenderMode.ScreenSpaceCamera && m_Canvas.worldCamera == null))
			{
				return null;
			}
			if (m_Canvas.worldCamera != null)
			{
				return m_Canvas.worldCamera;
			}
			return Camera.main;
		}
	}

	public Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public TweenEasing transitionEasing
	{
		get
		{
			return m_TransitionEasing;
		}
		set
		{
			m_TransitionEasing = value;
		}
	}

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
		}
	}

	protected UITooltip()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
	}

	protected virtual void Awake()
	{
		mInstance = this;
		m_image = GetComponent<Image>();
		m_Rect = GetComponent<RectTransform>();
		m_CanvasGroup = GetComponent<CanvasGroup>();
		m_LayoutGroup = GetComponent<VerticalLayoutGroup>();
		m_SizeFitter = GetComponent<ContentSizeFitter>();
		defaultAnchoredOffset = anchoredOffset;
		defaultBackgroundImageState = m_image.enabled;
		defaultOffset = offset;
		defaultAnchoredPositionOffset = anchoredPositionOffset;
	}

	protected virtual void Start()
	{
		EvaluateAndTransitionToState(state: false, 0f);
	}

	protected virtual void OnDestroy()
	{
		mInstance = null;
	}

	protected virtual void OnCanvasGroupChanged()
	{
		m_Canvas = UIUtility.FindInParents<Canvas>(base.gameObject);
	}

	public virtual bool IsActive()
	{
		if (base.enabled)
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}

	protected virtual void Update()
	{
		if (m_followMouse && base.enabled && IsActive() && alpha > 0f)
		{
			UpdatePositionAndPivot(null);
		}
	}

	public virtual void UpdatePositionAndPivot(RectTransform tooltipTransform)
	{
		if (m_AnchorToTarget == null)
		{
			targetCorner = Corner.Auto;
			autoCorner = true;
			Vector2 vector = new Vector2((m_Rect.pivot.x == 1f) ? (m_Offset.x * -1f) : m_Offset.x, (m_Rect.pivot.y == 1f) ? (m_Offset.y * -1f) : m_Offset.y);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Canvas.transform as RectTransform, InputManager.CursorPosition, m_Canvas.worldCamera, out var localPoint);
			localPoint = ((!tooltipTransform) ? ((Vector2)m_Canvas.transform.TransformPoint(localPoint)) : ((Vector2)tooltipTransform.position));
			m_Rect.position = new Vector3(vector.x + localPoint.x, vector.y + localPoint.y, m_Rect.position.z);
		}
		UpdatePivot();
		if (m_AnchorToTarget != null)
		{
			Vector3[] array = new Vector3[4];
			m_AnchorToTarget.GetWorldCorners(array);
			Corner corner = VectorPivotToCorner(m_Rect.pivot);
			if (autoCorner)
			{
				targetCorner = GetOppositeCorner(corner);
			}
			Vector3 vector2 = new Vector3((m_Rect.pivot.x == 1f) ? (m_AnchoredOffset.x * -1f) : m_AnchoredOffset.x, (m_Rect.pivot.y == 1f) ? (m_AnchoredOffset.y * -1f) : m_AnchoredOffset.y, 0f);
			base.transform.position = vector2 + array[(int)targetCorner];
			m_Rect.anchoredPosition += m_AchoredPositionOffset;
		}
		if (m_screenBound)
		{
			Vector3 vector3 = m_Rect.DeltaWorldPositionToFitTheScreen(m_Canvas.worldCamera, m_screenBoundOffset);
			base.transform.position += vector3;
			StartCoroutine(DelayedRefreshScreenBound());
		}
	}

	private IEnumerator DelayedRefreshScreenBound()
	{
		yield return null;
		if (m_screenBound && base.enabled && IsActive() && alpha > 0f)
		{
			base.transform.position += m_Rect.DeltaWorldPositionToFitTheScreen(m_Canvas.worldCamera, m_screenBoundOffset);
		}
	}

	public void UpdatePivot()
	{
		if (targetCorner == Corner.Auto)
		{
			Vector2 vector;
			if (PlatformLayer.Instance.IsConsole)
			{
				vector = InputManager.CursorPosition;
			}
			else
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Canvas.transform as RectTransform, InputManager.CursorPosition, m_Canvas.worldCamera, out var localPoint);
				vector = m_Canvas.transform.TransformPoint(localPoint);
			}
			Vector2 pivot = new Vector2((vector.x > (float)Screen.width / 2f) ? 1f : 0f, (vector.y > (float)Screen.height / 2f) ? 1f : 0f);
			targetCorner = VectorPivotToCorner(pivot);
		}
		SetPivot(targetCorner);
	}

	protected void SetPivot(Corner point)
	{
		switch (point)
		{
		case Corner.BottomLeft:
			m_Rect.pivot = new Vector2(0f, 0f);
			break;
		case Corner.BottomRight:
			m_Rect.pivot = new Vector2(1f, 0f);
			break;
		case Corner.TopLeft:
			m_Rect.pivot = new Vector2(0f, 1f);
			break;
		case Corner.TopRight:
			m_Rect.pivot = new Vector2(1f, 1f);
			break;
		}
		UpdateAnchorGraphicPosition();
	}

	protected void UpdateAnchorGraphicPosition()
	{
		if (!(m_AnchorGraphic == null))
		{
			RectTransform rectTransform = m_AnchorGraphic.transform as RectTransform;
			rectTransform.pivot = Vector2.zero;
			rectTransform.anchorMax = m_Rect.pivot;
			rectTransform.anchorMin = m_Rect.pivot;
			rectTransform.localPosition = new Vector3((m_Rect.pivot.x == 1f) ? (m_AnchorGraphicOffset.x * -1f) : m_AnchorGraphicOffset.x, (m_Rect.pivot.y == 1f) ? (m_AnchorGraphicOffset.y * -1f) : m_AnchorGraphicOffset.y, rectTransform.localPosition.z);
			rectTransform.localScale = new Vector3((m_Rect.pivot.x == 0f) ? 1f : (-1f), (m_Rect.pivot.y == 0f) ? 1f : (-1f), rectTransform.localScale.z);
		}
	}

	protected virtual void Internal_Show(float delay, RectTransform tooltipTransform)
	{
		EvaluateAndCreateTooltipLines();
		UpdatePositionAndPivot(tooltipTransform);
		UIUtility.BringToFront(base.gameObject);
		EvaluateAndTransitionToState(state: true, delay);
	}

	protected virtual void Internal_Hide(float delay)
	{
		EvaluateAndTransitionToState(state: false, delay);
	}

	protected virtual void Internal_AnchorToRect(RectTransform targetRect, Corner corner)
	{
		m_AnchorToTarget = targetRect;
		targetCorner = corner;
		autoCorner = corner == Corner.Auto;
	}

	protected void Internal_SetHorizontalFitMode(ContentSizeFitter.FitMode mode)
	{
		m_SizeFitter.horizontalFit = mode;
	}

	protected void Internal_SetVerticalControls(bool autoAdjusted, bool tempIsPrefabTooltip = false)
	{
		if (tempIsPrefabTooltip)
		{
			m_LayoutGroup.childControlHeight = !autoAdjusted;
			m_LayoutGroup.childControlWidth = true;
			m_LayoutGroup.childForceExpandHeight = autoAdjusted;
			m_LayoutGroup.childForceExpandWidth = false;
			m_SizeFitter.verticalFit = ((!autoAdjusted) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained);
		}
		else
		{
			m_LayoutGroup.childControlHeight = autoAdjusted;
			m_LayoutGroup.childControlWidth = autoAdjusted;
			m_LayoutGroup.childForceExpandHeight = autoAdjusted;
			m_LayoutGroup.childForceExpandWidth = autoAdjusted;
			m_SizeFitter.verticalFit = (autoAdjusted ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained);
		}
	}

	private void EvaluateAndTransitionToState(bool state, float delay)
	{
		Transition transition = m_Transition;
		if (transition != Transition.None && transition == Transition.Fade)
		{
			StartAlphaTween(state ? 1f : 0f, (delay < 0f) ? m_TransitionDuration : delay);
		}
		else
		{
			SetAlpha(state ? 1f : 0f);
			m_VisualState = ((!state) ? VisualState.Hidden : VisualState.Shown);
		}
		if (m_Transition == Transition.None && !state)
		{
			InternalOnHide();
		}
	}

	public void SetAlpha(float alpha)
	{
		m_CanvasGroup.alpha = alpha;
	}

	public void StartAlphaTween(float targetAlpha, float duration)
	{
		FloatTween info = new FloatTween
		{
			duration = duration,
			startFloat = m_CanvasGroup.alpha,
			targetFloat = targetAlpha
		};
		info.AddOnChangedCallback(SetAlpha);
		info.AddOnFinishCallback(OnTweenFinished);
		info.ignoreTimeScale = true;
		info.easing = m_TransitionEasing;
		m_FloatTweenRunner.StartTween(info);
	}

	protected virtual void OnTweenFinished()
	{
		if (alpha == 0f)
		{
			m_VisualState = VisualState.Hidden;
			InternalOnHide();
		}
		else
		{
			m_VisualState = VisualState.Shown;
		}
	}

	private void InternalOnHide()
	{
		CleanupLines();
		m_AnchorToTarget = null;
		m_SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		m_Rect.sizeDelta = new Vector2(m_DefaultWidth, m_Rect.sizeDelta.y);
		anchoredOffset = defaultAnchoredOffset;
		m_image.enabled = defaultBackgroundImageState;
		offset = defaultOffset;
		anchoredPositionOffset = defaultAnchoredPositionOffset;
	}

	private void EvaluateAndCreateTooltipLines()
	{
		if (m_LinesTemplate == null || m_LinesTemplate.lineList.Count == 0)
		{
			return;
		}
		foreach (UITooltipLines.Line line in m_LinesTemplate.lineList)
		{
			GameObject gameObject = CreateLine(line.padding);
			for (int i = 0; i < 2; i++)
			{
				string text = ((i == 0) ? line.left : line.right);
				if (string.IsNullOrEmpty(text))
				{
					if (line is UITooltipLines.ImageLine)
					{
						CreateLineColumnWithSprites(gameObject.transform, i == 0, ((UITooltipLines.ImageLine)line).attributes, ((UITooltipLines.ImageLine)line).images);
					}
				}
				else
				{
					CreateLineColumn(gameObject.transform, text, i == 0, line.style, line.spriteAsset);
				}
			}
		}
	}

	private GameObject CreateLine(RectOffset padding)
	{
		GameObject obj = new GameObject("Line", typeof(RectTransform));
		obj.layer = base.gameObject.layer;
		obj.transform.SetParent(base.transform);
		obj.transform.localPosition = Vector3.zero;
		RectTransform obj2 = obj.transform as RectTransform;
		obj2.pivot = new Vector2(0f, 1f);
		obj2.localScale = new Vector3(1f, 1f, 1f);
		obj.AddComponent<HorizontalLayoutGroup>().padding = padding;
		return obj;
	}

	private void CreateLineColumn(Transform parent, string content, bool isLeft, UITooltipLines.LineStyle style, TMP_SpriteAsset spriteAsset)
	{
		GameObject gameObject = new GameObject("Column", typeof(RectTransform), typeof(CanvasRenderer));
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = Vector3.zero;
		RectTransform obj = gameObject.transform as RectTransform;
		obj.pivot = new Vector2(0f, 1f);
		obj.localScale = new Vector3(1f, 1f, 1f);
		TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.text = content;
		textMeshProUGUI.richText = true;
		textMeshProUGUI.alignment = (isLeft ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.BottomRight);
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.spriteAsset = spriteAsset;
		TextEffectType textEffectType = TextEffectType.None;
		Color effectColor = Color.white;
		Vector2 effectDistance = new Vector2(1f, -1f);
		bool useGraphicAlpha = true;
		switch (style)
		{
		case UITooltipLines.LineStyle.Title:
			textMeshProUGUI.font = m_TitleFont;
			textMeshProUGUI.fontStyle = m_TitleFontStyle;
			if (InputManager.GamePadInUse)
			{
				textMeshProUGUI.fontSize = m_TitleFontSize;
			}
			else
			{
				textMeshProUGUI.fontSize = m_PCTitleFontSize;
			}
			textMeshProUGUI.lineSpacing = m_TitleFontLineSpacing;
			textMeshProUGUI.color = m_TitleFontColor;
			textEffectType = m_TitleTextEffect;
			effectColor = m_TitleTextEffectColor;
			effectDistance = m_TitleTextEffectDistance;
			useGraphicAlpha = m_TitleTextEffectUseGraphicAlpha;
			break;
		case UITooltipLines.LineStyle.Attribute:
			textMeshProUGUI.font = m_AttributeFont;
			textMeshProUGUI.fontStyle = m_AttributeFontStyle;
			textMeshProUGUI.fontSize = m_AttributeFontSize;
			textMeshProUGUI.lineSpacing = m_AttributeFontLineSpacing;
			textMeshProUGUI.color = m_AttributeFontColor;
			textEffectType = m_AttributeTextEffect;
			effectColor = m_AttributeTextEffectColor;
			effectDistance = m_AttributeTextEffectDistance;
			useGraphicAlpha = m_AttributeTextEffectUseGraphicAlpha;
			break;
		case UITooltipLines.LineStyle.Description:
			textMeshProUGUI.font = m_DescriptionFont;
			textMeshProUGUI.fontStyle = m_DescriptionFontStyle;
			textMeshProUGUI.fontSize = m_DescriptionFontSize;
			textMeshProUGUI.lineSpacing = m_DescriptionFontLineSpacing;
			textMeshProUGUI.color = m_DescriptionFontColor;
			textEffectType = m_DescriptionTextEffect;
			effectColor = m_DescriptionTextEffectColor;
			effectDistance = m_DescriptionTextEffectDistance;
			useGraphicAlpha = m_DescriptionTextEffectUseGraphicAlpha;
			break;
		case UITooltipLines.LineStyle.Keyword:
			textMeshProUGUI.font = m_KeywordFont;
			textMeshProUGUI.fontStyle = m_KeywordFontStyle;
			textMeshProUGUI.fontSize = m_KeywordFontSize;
			textMeshProUGUI.lineSpacing = m_KeywordFontLineSpacing;
			textMeshProUGUI.color = m_KeywordFontColor;
			textEffectType = m_KeywordTextEffect;
			effectColor = m_KeywordTextEffectColor;
			effectDistance = m_KeywordTextEffectDistance;
			useGraphicAlpha = m_KeywordTextEffectUseGraphicAlpha;
			break;
		}
		switch (textEffectType)
		{
		case TextEffectType.Shadow:
		{
			Shadow shadow = gameObject.AddComponent<Shadow>();
			shadow.effectColor = effectColor;
			shadow.effectDistance = effectDistance;
			shadow.useGraphicAlpha = useGraphicAlpha;
			break;
		}
		case TextEffectType.Outline:
		{
			Outline outline = gameObject.AddComponent<Outline>();
			outline.effectColor = effectColor;
			outline.effectDistance = effectDistance;
			outline.useGraphicAlpha = useGraphicAlpha;
			break;
		}
		}
	}

	private void CreateLineColumnWithSprites(Transform parent, bool isLeft, int[] attributes, Sprite[] sprites)
	{
		GameObject gameObject = new GameObject("ImageColumn", typeof(RectTransform), typeof(CanvasRenderer));
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.SetParent(parent);
		HorizontalLayoutGroup horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
		horizontalLayoutGroup.childAlignment = TextAnchor.UpperRight;
		horizontalLayoutGroup.childForceExpandWidth = false;
		(gameObject.transform as RectTransform).pivot = new Vector2(0f, 1f);
		for (int i = 0; i < attributes.Length; i++)
		{
			GameObject obj = new GameObject("AttributeText", typeof(RectTransform), typeof(CanvasRenderer));
			obj.layer = base.gameObject.layer;
			obj.transform.SetParent(gameObject.transform);
			obj.AddComponent<LayoutElement>().preferredWidth = 20f;
			TextMeshProUGUI textMeshProUGUI = obj.AddComponent<TextMeshProUGUI>();
			textMeshProUGUI.text = attributes[i].ToString();
			textMeshProUGUI.richText = true;
			textMeshProUGUI.alignment = (isLeft ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.BottomRight);
			textMeshProUGUI.font = m_AttributeFont;
			textMeshProUGUI.fontStyle = m_AttributeFontStyle;
			textMeshProUGUI.fontSize = m_AttributeFontSize;
			textMeshProUGUI.color = m_AttributeFontColor;
			GameObject obj2 = new GameObject("AttributeImage", typeof(RectTransform), typeof(CanvasRenderer));
			obj2.layer = base.gameObject.layer;
			obj2.transform.SetParent(gameObject.transform);
			LayoutElement layoutElement = obj2.AddComponent<LayoutElement>();
			float preferredWidth = (layoutElement.preferredHeight = 15f);
			layoutElement.preferredWidth = preferredWidth;
			obj2.AddComponent<Image>().sprite = sprites[i];
		}
	}

	protected virtual void CleanupLines()
	{
		foreach (Transform item in base.transform)
		{
			if (!(item.gameObject.GetComponent<LayoutElement>() != null) || !item.gameObject.GetComponent<LayoutElement>().ignoreLayout)
			{
				Object.Destroy(item.gameObject);
			}
		}
		m_LinesTemplate = null;
	}

	protected void Internal_SetLines(UITooltipLines lines)
	{
		m_LinesTemplate = lines;
	}

	protected void Internal_SetWidth(float width)
	{
		m_Rect.sizeDelta = new Vector2(width, m_Rect.sizeDelta.y);
	}

	protected void Internal_SetSize(float width, float height)
	{
		m_Rect.sizeDelta = new Vector2(width, height);
		RectTransform rect = m_Rect;
		Vector2 anchorMin = (m_Rect.anchorMax = new Vector2(0.5f, 0.5f));
		rect.anchorMin = anchorMin;
		m_Rect.pivot = new Vector2(0.5f, 0.5f);
	}

	protected void Internal_SetScreenBound(bool enable, float offset = 0f)
	{
		m_screenBound = enable;
		m_screenBoundOffset = offset;
	}

	protected void Internal_AddLine(string a, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(a, padding, spriteAsset);
	}

	protected void Internal_AddLine(string a, RectOffset padding, UITooltipLines.LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(a, padding, style, spriteAsset);
	}

	protected void Internal_AddLine(string a, string b, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(a, b, padding, spriteAsset);
	}

	protected void Internal_AddLine(string a, string b, RectOffset padding, UITooltipLines.LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(a, b, padding, style, spriteAsset);
	}

	protected void Internal_AddLineColumn(string a, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddColumn(a, spriteAsset);
	}

	protected virtual void Internal_AddTitle(string title, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(title, new RectOffset(0, 0, 0, 0), UITooltipLines.LineStyle.Title, spriteAsset);
	}

	protected virtual void Internal_AddAttributeLine(string leftContent, int[] attributes, Sprite[] images)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddAttributeLine(leftContent, attributes, images);
	}

	protected virtual void Internal_AddKeywords(string keywords)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(keywords, new RectOffset(0, 0, 4, 0), UITooltipLines.LineStyle.Keyword);
	}

	protected virtual void Internal_AddDescription(string description, TMP_SpriteAsset spriteAsset = null)
	{
		if (m_LinesTemplate == null)
		{
			m_LinesTemplate = new UITooltipLines();
		}
		m_LinesTemplate.AddLine(description, new RectOffset(0, 0, 4, 0), UITooltipLines.LineStyle.Description, spriteAsset);
	}

	protected virtual void Internal_ResetContent()
	{
		if (m_LinesTemplate != null)
		{
			m_LinesTemplate.lineList.Clear();
			CleanupLines();
		}
	}

	public static void AddTitle(string title, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddTitle(title, spriteAsset);
		}
	}

	public static void AddAttributeLine(string leftContent, int[] attributes, Sprite[] images)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddAttributeLine(leftContent, attributes, images);
		}
	}

	public static void AddKeywords(string keywords)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddKeywords(keywords);
		}
	}

	public static void AddDescription(string description, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddDescription(description, spriteAsset);
		}
	}

	public static void SetLines(UITooltipLines lines)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetLines(lines);
		}
	}

	public static void AddLine(string content, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(content, new RectOffset(), spriteAsset);
		}
	}

	public static void AddLine(string content, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(content, padding, spriteAsset);
		}
	}

	public static void AddLine(string content, RectOffset padding, UITooltipLines.LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(content, padding, style, spriteAsset);
		}
	}

	public static void AddLine(string leftContent, string rightContent, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(leftContent, rightContent, new RectOffset(), spriteAsset);
		}
	}

	public static void AddLine(string leftContent, string rightContent, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(leftContent, rightContent, padding, spriteAsset);
		}
	}

	public static void AddLine(string leftContent, string rightContent, RectOffset padding, UITooltipLines.LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLine(leftContent, rightContent, padding, style, spriteAsset);
		}
	}

	public static void AddLineColumn(string content, TMP_SpriteAsset spriteAsset = null)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AddLineColumn(content, spriteAsset);
		}
	}

	public static void ResetContent()
	{
		if (mInstance != null)
		{
			mInstance.Internal_ResetContent();
		}
	}

	public static void Show(float delay = -1f, RectTransform tooltipTransform = null)
	{
		if (mInstance != null && mInstance.IsActive())
		{
			mInstance.Internal_Show(delay, tooltipTransform);
		}
	}

	public static void Hide(float delay = -1f)
	{
		if (mInstance != null)
		{
			mInstance.Internal_Hide(delay);
		}
	}

	public static void AnchorToRect(RectTransform targetRect, Corner corner)
	{
		if (mInstance != null)
		{
			mInstance.Internal_AnchorToRect(targetRect, corner);
		}
	}

	public static void SetAnchoredOffset(Vector2 anchoredOffset)
	{
		if (mInstance != null)
		{
			mInstance.anchoredOffset = anchoredOffset;
		}
	}

	public static void SetAnchoredPositionOffset(Vector2 anchoredOffset)
	{
		if (mInstance != null)
		{
			mInstance.anchoredPositionOffset = anchoredOffset;
		}
	}

	public static void SetOffset(Vector2 offset)
	{
		if (mInstance != null)
		{
			mInstance.offset = offset;
		}
	}

	public static void ShowBackgroundImage(bool show)
	{
		if (mInstance != null)
		{
			mInstance.m_image.enabled = show;
		}
	}

	public static void SetHorizontalFitMode(ContentSizeFitter.FitMode mode)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetHorizontalFitMode(mode);
		}
	}

	public static void SetVerticalControls(bool autoAdjusted, bool tempIsPrefabTooltip = false)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetVerticalControls(autoAdjusted, tempIsPrefabTooltip);
		}
	}

	public static void SetWidth(float width)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetWidth(width);
		}
	}

	public static void SetSize(float width, float height)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetSize(width, height);
		}
	}

	public static void SetScreenBound(bool enable, float offset)
	{
		if (mInstance != null)
		{
			mInstance.Internal_SetScreenBound(enable, offset);
		}
	}

	public static Corner VectorPivotToCorner(Vector2 pivot)
	{
		if (pivot.x == 0f && pivot.y == 0f)
		{
			return Corner.BottomLeft;
		}
		if (pivot.x == 0f && pivot.y == 1f)
		{
			return Corner.TopLeft;
		}
		if (pivot.x == 1f && pivot.y == 0f)
		{
			return Corner.BottomRight;
		}
		return Corner.TopRight;
	}

	public static Corner GetOppositeCorner(Corner corner)
	{
		return corner switch
		{
			Corner.BottomLeft => Corner.TopRight, 
			Corner.BottomRight => Corner.TopLeft, 
			Corner.TopLeft => Corner.BottomRight, 
			Corner.TopRight => Corner.BottomLeft, 
			_ => Corner.BottomLeft, 
		};
	}

	public static Transform GetTransform()
	{
		if (mInstance != null)
		{
			return mInstance.transform;
		}
		return null;
	}
}
