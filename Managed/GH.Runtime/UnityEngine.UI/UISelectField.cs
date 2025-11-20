#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Gloomhaven;
using SM.Utils;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UI/Select Field", 58)]
[RequireComponent(typeof(Image))]
public class UISelectField : Toggle
{
	public enum Direction
	{
		Auto,
		Down,
		Up
	}

	public enum VisualState
	{
		Normal,
		Highlighted,
		Pressed,
		Active,
		ActiveHighlighted,
		ActivePressed,
		Disabled,
		Selected,
		ActiveSelected
	}

	public enum ListAnimationType
	{
		None,
		Fade,
		Animation
	}

	public enum OptionTextTransitionType
	{
		None,
		CrossFade
	}

	public enum OptionTextEffectType
	{
		None,
		Shadow,
		Outline
	}

	[Serializable]
	public class ChangeEvent : UnityEvent<int, string>
	{
	}

	[HideInInspector]
	[SerializeField]
	private string m_SelectedItem;

	[SerializeField]
	private Direction m_Direction;

	private List<UISelectField_Option> m_OptionObjects = new List<UISelectField_Option>();

	private VisualState m_CurrentVisualState;

	private bool m_PointerWasUsedOnOption;

	private int scrollableIfMoreOptionsThan = 6;

	private GameObject scrollArea;

	private GameObject m_ListObject;

	private CanvasGroup m_ListCanvasGroup;

	private Vector2 m_LastListSize = Vector2.zero;

	public UISelectField_Arrow arrow;

	public UISelectField_Label label;

	public List<string> options = new List<string>();

	public HashSet<string> disabledOptions = new HashSet<string>();

	public new ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;

	public new SpriteStateExtended spriteState;

	public new AnimationTriggersExtended animationTriggers = new AnimationTriggersExtended();

	public Sprite listBackgroundSprite;

	public Image.Type listBackgroundSpriteType = Image.Type.Sliced;

	public Color listBackgroundColor = Color.white;

	public RectOffset listMargins;

	public RectOffset listPadding;

	public float listSpacing;

	public ListAnimationType listAnimationType = ListAnimationType.Fade;

	public float listAnimationDuration = 0.1f;

	public RuntimeAnimatorController listAnimatorController;

	public string listAnimationOpenTrigger = "Open";

	public string listAnimationCloseTrigger = "Close";

	public Font optionFont = FontData.defaultFontData.font;

	public int optionFontSize = FontData.defaultFontData.fontSize;

	public FontStyle optionFontStyle = FontData.defaultFontData.fontStyle;

	public Color optionColor = Color.white;

	public OptionTextTransitionType optionTextTransitionType = OptionTextTransitionType.CrossFade;

	public ColorBlockExtended optionTextTransitionColors = ColorBlockExtended.defaultColorBlock;

	public RectOffset optionPadding;

	public OptionTextEffectType optionTextEffectType;

	public Color optionTextEffectColor = new Color(0f, 0f, 0f, 128f);

	public Vector2 optionTextEffectDistance = new Vector2(1f, -1f);

	public bool optionTextEffectUseGraphicAlpha = true;

	public Sprite optionBackgroundSprite;

	public Color optionBackgroundSpriteColor = Color.white;

	public Image.Type optionBackgroundSpriteType = Image.Type.Sliced;

	public Transition optionBackgroundTransitionType;

	public ColorBlockExtended optionBackgroundTransColors = ColorBlockExtended.defaultColorBlock;

	public SpriteStateExtended optionBackgroundSpriteStates;

	public AnimationTriggersExtended optionBackgroundAnimationTriggers = new AnimationTriggersExtended();

	public RuntimeAnimatorController optionBackgroundAnimatorController;

	public Sprite listSeparatorSprite;

	public Image.Type listSeparatorType;

	public Color listSeparatorColor = Color.white;

	public float listSeparatorHeight;

	public ChangeEvent onChange = new ChangeEvent();

	[NonSerialized]
	private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

	public Direction direction
	{
		get
		{
			return m_Direction;
		}
		set
		{
			m_Direction = value;
		}
	}

	public string value
	{
		get
		{
			return m_SelectedItem;
		}
		set
		{
			SelectOption(value);
		}
	}

	public int selectedOptionIndex => GetOptionIndex(m_SelectedItem);

	public bool IsOpen => base.isOn;

	protected UISelectField()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
	}

	protected override void Awake()
	{
		base.Awake();
		if (base.targetGraphic == null)
		{
			base.targetGraphic = GetComponent<Image>();
		}
	}

	protected override void Start()
	{
		base.Start();
		toggleTransition = ToggleTransition.None;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		onValueChanged.AddListener(OnToggleValueChanged);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		onValueChanged.RemoveListener(OnToggleValueChanged);
		base.isOn = false;
		ListCleanup();
		LogUtils.Log("OnDisable!");
		DoStateTransition(SelectionState.Disabled, instant: true);
	}

	public void Open()
	{
		base.isOn = true;
	}

	public void Close()
	{
		base.isOn = false;
	}

	public void CloseTimed(float duration)
	{
		LogUtils.Log("Timed closing of the dropdown list commanded.");
		if (base.isActiveAndEnabled && base.isOn)
		{
			LogUtils.Log("Closing started.");
			StartCoroutine(WaitAndClose(duration));
		}
	}

	private IEnumerator WaitAndClose(float duration)
	{
		yield return new WaitForSecondsRealtime(duration);
		LogUtils.Log("Closing dropdown list!");
		Close();
	}

	public int GetOptionIndex(string optionValue)
	{
		if (options != null && options.Count > 0 && !string.IsNullOrEmpty(optionValue))
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (optionValue.Equals(options[i], StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void SelectOptionByIndex(int index)
	{
		if (IsOpen)
		{
			UISelectField_Option uISelectField_Option = m_OptionObjects[index];
			if (uISelectField_Option != null)
			{
				uISelectField_Option.isOn = true;
			}
		}
		else
		{
			m_SelectedItem = options[index];
			TriggerChangeEvent();
		}
	}

	public void SelectOption(string optionValue)
	{
		if (!string.IsNullOrEmpty(optionValue))
		{
			int optionIndex = GetOptionIndex(optionValue);
			if (optionIndex >= 0 && optionIndex < options.Count)
			{
				SelectOptionByIndex(optionIndex);
			}
		}
	}

	public void AddOption(string optionValue)
	{
		if (options != null)
		{
			options.Add(optionValue);
		}
	}

	public void AddOptionAtIndex(string optionValue, int index)
	{
		if (options != null)
		{
			if (index >= options.Count)
			{
				options.Add(optionValue);
			}
			else
			{
				options.Insert(index, optionValue);
			}
		}
	}

	public void RemoveOption(string optionValue)
	{
		if (options != null && options.Contains(optionValue))
		{
			options.Remove(optionValue);
			ValidateSelectedOption();
		}
	}

	public void RemoveOptionAtIndex(int index)
	{
		if (options != null && index >= 0 && index < options.Count)
		{
			options.RemoveAt(index);
			ValidateSelectedOption();
		}
	}

	public void ValidateSelectedOption()
	{
		if (options != null && !options.Contains(m_SelectedItem))
		{
			SelectOptionByIndex(0);
		}
	}

	public void OnOptionSelect(string option)
	{
		if (!string.IsNullOrEmpty(option))
		{
			string selectedItem = m_SelectedItem;
			m_SelectedItem = option;
			if (!selectedItem.Equals(m_SelectedItem))
			{
				TriggerChangeEvent();
			}
			if (IsOpen && m_PointerWasUsedOnOption)
			{
				m_PointerWasUsedOnOption = false;
				Close();
				base.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
	}

	public void OnOptionPointerUp(BaseEventData eventData)
	{
		m_PointerWasUsedOnOption = true;
	}

	protected virtual void TriggerChangeEvent()
	{
		if (label != null && label.textComponent != null)
		{
			label.textComponent.text = m_SelectedItem;
		}
		if (onChange != null)
		{
			onChange.Invoke(selectedOptionIndex, m_SelectedItem);
		}
	}

	private void OnToggleValueChanged(bool state)
	{
		if (Application.isPlaying)
		{
			LogUtils.Log("OnToggleValueChanged!");
			DoStateTransition(base.currentSelectionState, instant: false);
			ToggleList(base.isOn);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (m_ListObject != null && m_ListObject.GetComponent<UISelectField_List>().IsHighlighted(eventData))
		{
			return;
		}
		foreach (UISelectField_Option optionObject in m_OptionObjects)
		{
			if (optionObject.IsHighlighted(eventData))
			{
				return;
			}
		}
		Close();
		base.OnDeselect(eventData);
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (IsOpen)
		{
			int num = selectedOptionIndex - 1;
			int num2 = selectedOptionIndex + 1;
			switch (eventData.moveDir)
			{
			case MoveDirection.Up:
				if (num >= 0)
				{
					SelectOptionByIndex(num);
				}
				break;
			case MoveDirection.Down:
				if (num2 < options.Count)
				{
					SelectOptionByIndex(num2);
				}
				break;
			}
			eventData.Use();
		}
		else
		{
			base.OnMove(eventData);
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Color color = colors.normalColor;
			Sprite newSprite = null;
			string trigger = animationTriggers.normalTrigger;
			switch (state)
			{
			case SelectionState.Disabled:
				m_CurrentVisualState = VisualState.Disabled;
				color = colors.disabledColor;
				newSprite = spriteState.disabledSprite;
				trigger = animationTriggers.disabledTrigger;
				break;
			case SelectionState.Normal:
				m_CurrentVisualState = (base.isOn ? VisualState.Active : VisualState.Normal);
				color = (base.isOn ? colors.activeColor : colors.normalColor);
				newSprite = (base.isOn ? spriteState.activeSprite : null);
				trigger = (base.isOn ? animationTriggers.activeTrigger : animationTriggers.normalTrigger);
				break;
			case SelectionState.Highlighted:
				m_CurrentVisualState = ((!base.isOn) ? VisualState.Highlighted : VisualState.ActiveHighlighted);
				color = (base.isOn ? colors.activeHighlightedColor : colors.highlightedColor);
				newSprite = (base.isOn ? spriteState.activeHighlightedSprite : spriteState.highlightedSprite);
				trigger = (base.isOn ? animationTriggers.activeHighlightedTrigger : animationTriggers.highlightedTrigger);
				break;
			case SelectionState.Pressed:
				m_CurrentVisualState = (base.isOn ? VisualState.ActivePressed : VisualState.Pressed);
				color = (base.isOn ? colors.activePressedColor : colors.pressedColor);
				newSprite = (base.isOn ? spriteState.activePressedSprite : spriteState.pressedSprite);
				trigger = (base.isOn ? animationTriggers.activePressedTrigger : animationTriggers.pressedTrigger);
				break;
			case SelectionState.Selected:
				m_CurrentVisualState = (base.isOn ? VisualState.ActivePressed : VisualState.Selected);
				color = (base.isOn ? colors.activeSelectedColor : colors.selectedColor);
				newSprite = (base.isOn ? spriteState.activeSelectedSprite : spriteState.selectedSprite);
				trigger = (base.isOn ? animationTriggers.activeSelectedTrigger : animationTriggers.selectedTrigger);
				break;
			}
			switch (base.transition)
			{
			case Transition.ColorTint:
				StartColorTween(color * colors.colorMultiplier, instant ? 0f : colors.fadeDuration);
				break;
			case Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Transition.Animation:
				TriggerAnimation(trigger);
				break;
			}
			if (arrow != null)
			{
				arrow.UpdateState(m_CurrentVisualState, instant);
			}
			if (label != null)
			{
				label.UpdateState(m_CurrentVisualState, instant);
			}
		}
	}

	private void StartColorTween(Color color, float duration)
	{
		if (!(base.targetGraphic == null))
		{
			base.targetGraphic.CrossFadeColor(color, duration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		Image image = base.targetGraphic as Image;
		if (!(image == null))
		{
			image.overrideSprite = newSprite;
		}
	}

	private void TriggerAnimation(string trigger)
	{
		if (!(base.animator == null) && base.animator.isActiveAndEnabled && !(base.animator.runtimeAnimatorController == null) && !string.IsNullOrEmpty(trigger))
		{
			base.animator.ResetTrigger(animationTriggers.normalTrigger);
			base.animator.ResetTrigger(animationTriggers.pressedTrigger);
			base.animator.ResetTrigger(animationTriggers.highlightedTrigger);
			base.animator.ResetTrigger(animationTriggers.activeTrigger);
			base.animator.ResetTrigger(animationTriggers.activeHighlightedTrigger);
			base.animator.ResetTrigger(animationTriggers.activePressedTrigger);
			base.animator.ResetTrigger(animationTriggers.disabledTrigger);
			base.animator.ResetTrigger(animationTriggers.selectedTrigger);
			base.animator.ResetTrigger(animationTriggers.activeSelectedTrigger);
			base.animator.SetTrigger(trigger);
		}
	}

	public virtual void ToggleList(bool state)
	{
		if (scrollArea == null && state)
		{
			CreateList();
		}
		if (!(scrollArea == null))
		{
			if (m_ListCanvasGroup != null)
			{
				m_ListCanvasGroup.blocksRaycasts = state;
			}
			if (state)
			{
				UIUtility.BringToFront(scrollArea);
			}
			if (listAnimationType == ListAnimationType.None || listAnimationType == ListAnimationType.Fade)
			{
				float targetAlpha = (state ? 1f : 0f);
				TweenListAlpha(targetAlpha, (listAnimationType == ListAnimationType.Fade) ? listAnimationDuration : 0f, ignoreTimeScale: true);
			}
			else if (listAnimationType == ListAnimationType.Animation)
			{
				TriggerListAnimation(state ? listAnimationOpenTrigger : listAnimationCloseTrigger);
			}
		}
	}

	protected void CreateList()
	{
		m_LastListSize = Vector2.zero;
		m_OptionObjects.Clear();
		scrollArea = new GameObject("UISelectField - Scroll Area", typeof(RectTransform));
		scrollArea.transform.SetParent(base.transform, worldPositionStays: false);
		ClickTracker clickTracker = scrollArea.AddComponent<ClickTracker>();
		clickTracker.clickType = MouseClickType.LeftClick;
		clickTracker.trackingArea = TrackingArea.OutsideThisArea;
		clickTracker.onClick = new ClickTracker.ClickButtonEvent();
		clickTracker.onClick.AddListener(delegate
		{
			CloseTimed(0.1f);
		});
		GameObject gameObject = new GameObject("Background Image", typeof(RectTransform));
		gameObject.transform.SetParent(scrollArea.transform, worldPositionStays: false);
		(gameObject.transform as RectTransform).sizeDelta = new Vector2(220f, 240f);
		m_ListObject = new GameObject("UISelectField - List", typeof(RectTransform));
		m_ListObject.transform.SetParent(scrollArea.transform, worldPositionStays: false);
		UISelectField_List uISelectField_List = m_ListObject.AddComponent<UISelectField_List>();
		m_ListCanvasGroup = scrollArea.AddComponent<CanvasGroup>();
		RectTransform rectTransform = scrollArea.transform as RectTransform;
		Vector2 anchorMin = (rectTransform.anchorMax = Vector2.zero);
		rectTransform.anchorMin = anchorMin;
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector3(listMargins.left, (float)listMargins.top * -1f, 0f);
		RectTransform obj = m_ListObject.transform as RectTransform;
		Vector2 vector = (obj.pivot = new Vector2(0.5f, 1f));
		anchorMin = (obj.anchorMax = vector);
		obj.anchorMin = anchorMin;
		obj.anchoredPosition = Vector2.zero;
		float x = base.targetGraphic.rectTransform.sizeDelta.x - (float)(listMargins.left + listMargins.right);
		rectTransform.sizeDelta = new Vector2(x, (float)scrollableIfMoreOptionsThan * 36f + 17f);
		obj.sizeDelta = new Vector2(x, 0f);
		if (options.Count > scrollableIfMoreOptionsThan)
		{
			Scrollbar component = Object.Instantiate(Singleton<ReferenceBank>.Instance.ScrollBarPrefab, scrollArea.transform, worldPositionStays: false).GetComponent<Scrollbar>();
			RectTransform obj2 = component.transform as RectTransform;
			obj2.anchorMin = new Vector2(1f, 0f);
			obj2.anchorMax = new Vector2(1f, 1f);
			obj2.pivot = new Vector2(1f, 0.5f);
			obj2.offsetMin = new Vector2(obj2.offsetMin.x, 0f);
			obj2.offsetMax = new Vector2(obj2.offsetMax.x, 0f);
			ScrollRect scrollRect = scrollArea.AddComponent<ScrollRect>();
			scrollRect.content = (RectTransform)m_ListObject.transform;
			scrollRect.verticalScrollbar = component;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.inertia = false;
			scrollRect.scrollSensitivity = 60f;
			scrollArea.AddComponent<Image>().sprite = Singleton<ReferenceBank>.Instance.MaskSprite;
			scrollArea.AddComponent<Mask>().showMaskGraphic = false;
			Image image = gameObject.AddComponent<Image>();
			if (listBackgroundSprite != null)
			{
				image.sprite = listBackgroundSprite;
				image.type = listBackgroundSpriteType;
				image.color = listBackgroundColor;
			}
		}
		else
		{
			Image image2 = m_ListObject.AddComponent<Image>();
			if (listBackgroundSprite != null)
			{
				image2.sprite = listBackgroundSprite;
			}
			image2.type = listBackgroundSpriteType;
			image2.color = listBackgroundColor;
		}
		uISelectField_List.onDimensionsChange.AddListener(ListDimensionsChanged);
		VerticalLayoutGroup verticalLayoutGroup = m_ListObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.padding = listPadding;
		verticalLayoutGroup.spacing = listSpacing;
		m_ListObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		ToggleGroup toggleGroup = m_ListObject.AddComponent<ToggleGroup>();
		for (int num = 0; num < options.Count; num++)
		{
			CreateOption(num, toggleGroup);
			if (num < options.Count - 1)
			{
				CreateSeparator(num);
			}
		}
		if (listAnimationType == ListAnimationType.None || listAnimationType == ListAnimationType.Fade)
		{
			m_ListCanvasGroup.alpha = 0f;
		}
		else if (listAnimationType == ListAnimationType.Animation)
		{
			m_ListObject.AddComponent<Animator>().runtimeAnimatorController = listAnimatorController;
			uISelectField_List.SetTriggers(listAnimationOpenTrigger, listAnimationCloseTrigger);
			uISelectField_List.onAnimationFinish.AddListener(OnListAnimationFinish);
		}
	}

	protected void CreateOption(int index, ToggleGroup toggleGroup)
	{
		if (m_ListObject == null)
		{
			return;
		}
		GameObject gameObject = new GameObject("Option " + index, typeof(RectTransform));
		gameObject.transform.SetParent(m_ListObject.transform, worldPositionStays: false);
		UISelectField_Option uISelectField_Option = gameObject.AddComponent<UISelectField_Option>();
		if (optionBackgroundSprite != null)
		{
			Image image = gameObject.AddComponent<Image>();
			image.sprite = optionBackgroundSprite;
			image.type = optionBackgroundSpriteType;
			image.color = optionBackgroundSpriteColor;
			uISelectField_Option.targetGraphic = image;
		}
		if (optionBackgroundTransitionType == Transition.Animation)
		{
			gameObject.AddComponent<Animator>().runtimeAnimatorController = optionBackgroundAnimatorController;
		}
		gameObject.AddComponent<VerticalLayoutGroup>().padding = optionPadding;
		GameObject gameObject2 = new GameObject("Label", typeof(RectTransform));
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		(gameObject2.transform as RectTransform).pivot = new Vector2(0f, 1f);
		Text text = gameObject2.AddComponent<Text>();
		text.font = optionFont;
		text.fontSize = optionFontSize;
		text.fontStyle = optionFontStyle;
		text.color = optionColor;
		if (options != null)
		{
			text.text = options[index];
		}
		if (optionTextTransitionType == OptionTextTransitionType.CrossFade)
		{
			text.canvasRenderer.SetColor(optionTextTransitionColors.normalColor);
		}
		if (optionTextEffectType != OptionTextEffectType.None)
		{
			if (optionTextEffectType == OptionTextEffectType.Shadow)
			{
				Shadow shadow = gameObject2.AddComponent<Shadow>();
				shadow.effectColor = optionTextEffectColor;
				shadow.effectDistance = optionTextEffectDistance;
				shadow.useGraphicAlpha = optionTextEffectUseGraphicAlpha;
			}
			else if (optionTextEffectType == OptionTextEffectType.Outline)
			{
				Outline outline = gameObject2.AddComponent<Outline>();
				outline.effectColor = optionTextEffectColor;
				outline.effectDistance = optionTextEffectDistance;
				outline.useGraphicAlpha = optionTextEffectUseGraphicAlpha;
			}
		}
		uISelectField_Option.Initialize(this, text);
		if (index == selectedOptionIndex)
		{
			uISelectField_Option.isOn = true;
		}
		if (toggleGroup != null)
		{
			uISelectField_Option.group = toggleGroup;
		}
		uISelectField_Option.onSelectOption.AddListener(OnOptionSelect);
		uISelectField_Option.onPointerUp.AddListener(OnOptionPointerUp);
		if (m_OptionObjects != null)
		{
			m_OptionObjects.Add(uISelectField_Option);
		}
		uISelectField_Option.gameObject.SetActive(!disabledOptions.Contains(options[index]));
	}

	protected void CreateSeparator(int index)
	{
		if (!(m_ListObject == null) && !(listSeparatorSprite == null))
		{
			GameObject obj = new GameObject("Separator " + index, typeof(RectTransform));
			obj.transform.SetParent(m_ListObject.transform, worldPositionStays: false);
			Image obj2 = obj.AddComponent<Image>();
			obj2.sprite = listSeparatorSprite;
			obj2.type = listSeparatorType;
			obj2.color = listSeparatorColor;
			obj.AddComponent<LayoutElement>().preferredHeight = ((listSeparatorHeight > 0f) ? listSeparatorHeight : listSeparatorSprite.rect.height);
		}
	}

	protected virtual void ListCleanup()
	{
		if (scrollArea != null)
		{
			Object.Destroy(scrollArea.gameObject);
		}
		m_OptionObjects.Clear();
	}

	public virtual void PositionListForDirection(Direction direction)
	{
		if (!(m_ListObject == null))
		{
			RectTransform rectTransform = base.transform as RectTransform;
			RectTransform rectTransform2 = scrollArea.transform as RectTransform;
			if (direction == Direction.Auto)
			{
				Vector3[] array = new Vector3[4];
				rectTransform2.GetWorldCorners(array);
				direction = ((!(array[0].y < 0f)) ? Direction.Down : Direction.Up);
			}
			Vector3[] array2 = new Vector3[4];
			rectTransform.GetWorldCorners(array2);
			if (direction == Direction.Down)
			{
				rectTransform2.position = new Vector3(array2[0].x + (float)listMargins.left, array2[0].y + (float)listMargins.top * -1f, 0f);
				return;
			}
			rectTransform2.position = new Vector3(array2[1].x + (float)listMargins.left, array2[1].y + (float)listMargins.bottom, 0f);
			rectTransform2.anchoredPosition += new Vector2(0f, m_ListObject.GetComponent<RectTransform>().rect.height + rectTransform.rect.height);
		}
	}

	protected virtual void ListDimensionsChanged()
	{
		if (IsActive() && !(m_ListObject == null) && !m_LastListSize.Equals((m_ListObject.transform as RectTransform).sizeDelta))
		{
			m_LastListSize = (m_ListObject.transform as RectTransform).sizeDelta;
			PositionListForDirection(m_Direction);
		}
	}

	private void TweenListAlpha(float targetAlpha, float duration, bool ignoreTimeScale)
	{
		if (!(m_ListCanvasGroup == null))
		{
			float alpha = m_ListCanvasGroup.alpha;
			if (!alpha.Equals(targetAlpha))
			{
				FloatTween info = new FloatTween
				{
					duration = duration,
					startFloat = alpha,
					targetFloat = targetAlpha
				};
				info.AddOnChangedCallback(SetListAlpha);
				info.AddOnFinishCallback(OnListTweenFinished);
				info.ignoreTimeScale = ignoreTimeScale;
				m_FloatTweenRunner.StartTween(info);
			}
		}
	}

	private void SetListAlpha(float alpha)
	{
		if (!(m_ListCanvasGroup == null))
		{
			m_ListCanvasGroup.alpha = alpha;
		}
	}

	private void TriggerListAnimation(string trigger)
	{
		if (!(m_ListObject == null) && !string.IsNullOrEmpty(trigger))
		{
			Animator component = m_ListObject.GetComponent<Animator>();
			if (!(component == null) && component.isActiveAndEnabled && !(component.runtimeAnimatorController == null))
			{
				component.ResetTrigger(listAnimationOpenTrigger);
				component.ResetTrigger(listAnimationCloseTrigger);
				component.SetTrigger(trigger);
			}
		}
	}

	protected virtual void OnListTweenFinished()
	{
		if (!IsOpen)
		{
			ListCleanup();
		}
	}

	protected virtual void OnListAnimationFinish(UISelectField_List.State state)
	{
		if (state == UISelectField_List.State.Closed && !IsOpen)
		{
			ListCleanup();
		}
	}

	public void ToggleOption(string optionValue, bool visible)
	{
		if (string.IsNullOrEmpty(optionValue))
		{
			return;
		}
		int optionIndex = GetOptionIndex(optionValue);
		if (optionIndex >= 0 && optionIndex < options.Count)
		{
			if (optionIndex < m_OptionObjects.Count)
			{
				m_OptionObjects[optionIndex].gameObject.SetActive(visible);
			}
			if (visible)
			{
				disabledOptions.Remove(optionValue);
			}
			else
			{
				disabledOptions.Add(optionValue);
			}
		}
	}
}
