#define ENABLE_LOGS
using System;
using GLOOM;
using I2.Loc;
using SM.Gamepad;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class ExtendedButton : Button, IInteractable
{
	public TextMeshProUGUI buttonText;

	public string textLanguageKey;

	public RectTransform targetRect;

	public bool scaleNonInteractable = true;

	public float highlightScaleFactor;

	public Transform[] ignoreScalingObjects;

	public bool animateScaling = true;

	public Vector3 hoverMovement = Vector3.zero;

	public float animationDuration = 0.3f;

	public TooltipUI tooltip;

	public bool autoDisplayTooltip = true;

	public UnityEvent onMouseEnter;

	public UnityEvent onMouseExit;

	public UnityEvent onSelected;

	public UnityEvent onDeselected;

	public Key alternativeInputKey;

	public bool restoreHighlightWhenReenable;

	public RectTransform overridedTargetRectScale;

	public AudioButtonProfile audioProfile;

	public bool useAudioController = true;

	public string mouseClickAudioItem;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string nonInteractableMouseDownAudioItem;

	public AudioClip mouseDownAudio;

	public AudioClip mouseUpAudio;

	public AudioClip mouseEnterAudio;

	public AudioClip mouseExitAudio;

	private CanvasGroup canvasGroup;

	private bool isFaded;

	private bool isHighlighted;

	private bool isMoved;

	private bool wasHighlightedOnDisable;

	private SelectionState prevState;

	private bool prevInteractable;

	public Action<bool> InteractableChanged;

	public Action<bool> ActiveChanged;

	public Action OnAnimationCancelled;

	public bool navigateOnlyInteractable;

	private INavigationCalculator navigationCalculator;

	private const string DebugPointerUp = "OnPointerUpButton";

	private const string DebugPointerDown = "OnPointerDownButton";

	private const string DebugHighlight = "ToggleHighlight";

	private Vector2 hoverStartPosition;

	public string TextLanguageKey
	{
		set
		{
			textLanguageKey = value;
			if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
			{
				buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
			}
		}
	}

	public bool IsSelected => base.currentSelectionState == SelectionState.Selected;

	public bool IsFaded => isFaded;

	public RectTransform TargetRect
	{
		get
		{
			if (targetRect == null)
			{
				targetRect = GetComponent<RectTransform>();
			}
			return targetRect;
		}
	}

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (canvasGroup == null)
			{
				canvasGroup = GetComponent<CanvasGroup>();
			}
			return canvasGroup;
		}
	}

	public new bool IsInteractable => base.interactable;

	protected override void Awake()
	{
		prevInteractable = base.interactable;
		base.Awake();
	}

	protected override void OnDestroy()
	{
		onMouseEnter.RemoveAllListeners();
		onMouseExit.RemoveAllListeners();
		onSelected.RemoveAllListeners();
		onDeselected.RemoveAllListeners();
		base.onClick.RemoveAllListeners();
		InteractableChanged = null;
		ActiveChanged = null;
		base.OnDestroy();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (base.interactable && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonClick(base.gameObject);
		}
		base.OnPointerClick(eventData);
		if (base.interactable)
		{
			PlaySound(null, (mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseClickAudioItem : mouseClickAudioItem);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnSubmit(eventData);
		if (base.interactable)
		{
			PlaySound(null, (mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseClickAudioItem : mouseClickAudioItem);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedButton(this))
		{
			Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerDown(eventData);
		if (isHighlighted && base.interactable)
		{
			RectTransform rectTransform = ((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale);
			if (LeanTween.isTweening(rectTransform))
			{
				LeanTween.cancel(rectTransform, "OnPointerDownButton");
			}
			rectTransform.localScale = new Vector3((highlightScaleFactor + 1f) / 2f, (highlightScaleFactor + 1f) / 2f, 1f);
			PlaySound(mouseDownAudio, (mouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseDownAudioItem);
		}
		else if (!base.interactable)
		{
			PlaySound(null, (nonInteractableMouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.nonInteractableMouseDownAudioItem : nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (isHighlighted && base.interactable)
		{
			if (LeanTween.isTweening(base.gameObject))
			{
				LeanTween.cancel(base.gameObject, "OnPointerUpButton");
			}
			((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale).localScale = new Vector3(highlightScaleFactor, highlightScaleFactor, 1f);
			PlaySound(mouseUpAudio, (mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	private void PlaySound(AudioClip mouseAudio, string audioItem)
	{
		if (useAudioController)
		{
			if (!string.IsNullOrEmpty(audioItem) && AudioController.GetAudioItem(audioItem) != null)
			{
				AudioControllerUtils.PlaySound(audioItem);
			}
			else if (mouseAudio != null)
			{
				AudioControllerUtils.PlaySound(mouseAudio.name);
			}
		}
		else if ((bool)UIManager.Instance)
		{
			UIManager.Instance.PlayUISound(mouseAudio);
		}
	}

	protected void Update()
	{
		try
		{
			if ((!(SceneController.Instance != null) || !(SceneController.Instance.GlobalErrorMessage != null) || !SceneController.Instance.GlobalErrorMessage.ShowingMessage) && alternativeInputKey != Key.None && base.onClick != null && InputManager.GetKeyDown(alternativeInputKey))
			{
				base.onClick.Invoke();
			}
		}
		catch
		{
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalize;
			I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalize;
		}
		if (restoreHighlightWhenReenable && wasHighlightedOnDisable && UIManager.GameObjectUnderPointer() == base.gameObject)
		{
			OnPointerEnter(new PointerEventData(EventSystem.current));
		}
		else if (base.currentSelectionState == SelectionState.Selected)
		{
			OnHighlight();
		}
		wasHighlightedOnDisable = false;
		ActiveChanged?.Invoke(obj: true);
	}

	private void OnLocalize()
	{
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			buttonText.text = GLOOM.LocalizationManager.GetTranslation(textLanguageKey);
		}
	}

	protected override void OnDisable()
	{
		ActiveChanged?.Invoke(obj: false);
		bool flag = base.currentSelectionState == SelectionState.Selected;
		base.OnDisable();
		wasHighlightedOnDisable = isHighlighted;
		if (isHighlighted)
		{
			ToggleHighlight(active: false, playAnimation: false);
		}
		if (buttonText != null && !string.IsNullOrEmpty(textLanguageKey))
		{
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLocalize;
		}
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
			onDeselected.Invoke();
		}
		else if (flag)
		{
			onDeselected.Invoke();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonHover(base.gameObject);
		}
		base.OnPointerEnter(eventData);
		OnHighlight();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		OnUnhighlight();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (InputManager.GamePadInUse)
		{
			OnHighlight();
			onSelected?.Invoke();
		}
	}

	public override Selectable FindSelectableOnUp()
	{
		if (base.navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		if (base.navigation.mode == Navigation.Mode.Explicit && navigationCalculator != null)
		{
			return navigationCalculator.FindUp();
		}
		return base.FindSelectableOnUp();
	}

	public override Selectable FindSelectableOnLeft()
	{
		if (base.navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		if (base.navigation.mode == Navigation.Mode.Explicit && navigationCalculator != null)
		{
			return navigationCalculator.FindLeft();
		}
		return base.FindSelectableOnLeft();
	}

	public override Selectable FindSelectableOnRight()
	{
		if (base.navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		if (base.navigation.mode == Navigation.Mode.Explicit && navigationCalculator != null)
		{
			return navigationCalculator.FindRight();
		}
		return base.FindSelectableOnRight();
	}

	public override Selectable FindSelectableOnDown()
	{
		if (base.navigation.mode == Navigation.Mode.Explicit && !IsInteractable() && navigateOnlyInteractable)
		{
			return null;
		}
		if (base.navigation.mode == Navigation.Mode.Explicit && navigationCalculator != null)
		{
			return navigationCalculator.FindDown();
		}
		return base.FindSelectableOnDown();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnUnhighlight();
		onDeselected?.Invoke();
	}

	private void OnHighlight()
	{
		if (scaleNonInteractable || IsInteractable())
		{
			if (onMouseEnter != null)
			{
				onMouseEnter.Invoke();
			}
			PlaySound(mouseEnterAudio, (mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
			ToggleHighlight(active: true, animateScaling);
		}
	}

	private void OnUnhighlight()
	{
		wasHighlightedOnDisable = false;
		if (onMouseExit != null)
		{
			onMouseExit.Invoke();
		}
		PlaySound(mouseExitAudio, (mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		ToggleHighlight(active: false, animateScaling);
	}

	private void ToggleHighlight(bool active, bool playAnimation = true)
	{
		isHighlighted = active;
		if (active)
		{
			HighlightFinished(active: true);
		}
		if (!active && tooltip != null && autoDisplayTooltip)
		{
			tooltip.ToggleEnable(active: false);
		}
		RectTransform rectTransform = ((overridedTargetRectScale == null) ? TargetRect : overridedTargetRectScale);
		if (playAnimation && LeanTween.isTweening(rectTransform))
		{
			LeanTween.cancel(rectTransform, "ToggleHighlight");
			OnAnimationCancelled?.Invoke();
		}
		if (playAnimation)
		{
			LTDescr lTDescr = LeanTween.scale(rectTransform, active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one, animationDuration).setEase(LeanTweenType.easeOutExpo).setIgnoreTimeScale(useUnScaledTime: true);
			if (!active)
			{
				lTDescr.setOnComplete((Action)delegate
				{
					HighlightFinished(active: false);
				});
			}
		}
		else
		{
			rectTransform.localScale = (active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one);
			if (!active)
			{
				HighlightFinished(active: false);
			}
		}
		if (hoverMovement != Vector3.zero && active != isMoved)
		{
			isMoved = active;
			if (active)
			{
				hoverStartPosition = TargetRect.anchoredPosition;
			}
			TargetRect.anchoredPosition = (active ? (hoverStartPosition + new Vector2(hoverMovement.x, hoverMovement.y)) : hoverStartPosition);
		}
	}

	private void HighlightFinished(bool active)
	{
		if (tooltip != null && autoDisplayTooltip)
		{
			tooltip.transform.localScale = (active ? new Vector3(1f / highlightScaleFactor, 1f / highlightScaleFactor, 1f) : Vector3.one);
			tooltip.transform.SetParent(active ? base.transform.parent : base.transform, worldPositionStays: true);
			tooltip.ToggleEnable(active);
		}
		if (ignoreScalingObjects == null || ignoreScalingObjects.Length == 0)
		{
			return;
		}
		Transform[] array = ignoreScalingObjects;
		foreach (Transform transform in array)
		{
			if (active)
			{
				UIManager.Instance.HighlightElement(transform.gameObject);
			}
			else
			{
				UIManager.Instance.UnhighlightElement(transform.gameObject);
			}
		}
	}

	public void ToggleFade(bool active, float fade = 0.3f)
	{
		isFaded = active;
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = (active ? fade : 1f);
		}
	}

	public void ClearSelectedState()
	{
		InstantClearState();
	}

	public void addToEnabledList(GameObject g)
	{
		g.SetActive(value: true);
	}

	public void SetNavigation(INavigationCalculator navigationCalculator)
	{
		if (navigationCalculator == null)
		{
			DisableNavigation();
		}
		else
		{
			SetNavigation(Navigation.Mode.Explicit);
		}
		this.navigationCalculator = navigationCalculator;
	}

	public void SetNavigation(Navigation.Mode mode, bool wrapAround = false)
	{
		navigationCalculator = null;
		SelectableExtensions.SetNavigation(this, mode, wrapAround);
	}

	public void DisableNavigation()
	{
		navigationCalculator = null;
		SelectableExtensions.DisableNavigation(this);
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (state != prevState)
		{
			if (prevInteractable != base.interactable)
			{
				prevInteractable = base.interactable;
				InteractableChanged?.Invoke(base.interactable);
			}
			prevState = state;
		}
	}
}
