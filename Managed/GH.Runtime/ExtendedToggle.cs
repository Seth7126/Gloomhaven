#define ENABLE_LOGS
using System;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class ExtendedToggle : Toggle, IInteractable
{
	public RectTransform targetRect;

	public float highlightScaleFactor = 1.1f;

	public bool scaleWhenSelected;

	public Transform[] ignoreScalingObjects;

	public bool animateScaling;

	public float animationDuration = 0.3f;

	public TooltipUI tooltip;

	public UnityEvent onMouseEnter;

	public UnityEvent onMouseExit;

	public UnityEvent onSelected;

	public UnityEvent onDeselected;

	public bool triggerNonInteractableHover;

	public bool useAudioController = true;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string nonInteractableMouseDownAudioItem;

	public AudioClip mouseDownAudio;

	public AudioClip mouseUpAudio;

	public AudioClip mouseEnterAudio;

	public AudioClip mouseExitAudio;

	public AudioButtonProfile audioProfile;

	private const string DebugPointerUp = "OnPointerUpToggle";

	private const string DebugPointerDown = "OnPointerDownToggle";

	private bool isHighlighted;

	private bool isMouseOver;

	private const string DebugHighlight = "ToggleHighlight";

	private LTDescr hoverAnimationTween;

	public new bool IsInteractable => base.interactable;

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

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Debug.Log("Toggle press for toggle " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (base.interactable && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonClick(base.gameObject);
		}
		base.OnPointerClick(eventData);
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Debug.Log("Toggle press for toggle " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnSubmit(eventData);
		if (base.interactable)
		{
			PlaySound(mouseDownAudio, (mouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseDownAudioItem);
		}
		else
		{
			PlaySound(null, (nonInteractableMouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.nonInteractableMouseDownAudioItem : nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForExtendedToggle(this))
		{
			Debug.Log("Toggle press for toggle " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerDown(eventData);
		if (isHighlighted && base.interactable)
		{
			if (LeanTween.isTweening(base.gameObject))
			{
				LeanTween.cancel(base.gameObject, "OnPointerDownToggle");
			}
			TargetRect.localScale = new Vector3((highlightScaleFactor + 1f) / 2f, (highlightScaleFactor + 1f) / 2f, 1f);
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
				LeanTween.cancel(base.gameObject, "OnPointerUpToggle");
			}
			TargetRect.localScale = new Vector3(highlightScaleFactor, highlightScaleFactor, 1f);
			PlaySound(mouseUpAudio, (mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		onValueChanged.AddListener(OnValueChanged);
	}

	protected override void OnDestroy()
	{
		onValueChanged.RemoveAllListeners();
		onMouseEnter.RemoveAllListeners();
		onMouseExit.RemoveAllListeners();
		onSelected.RemoveAllListeners();
		onDeselected.RemoveAllListeners();
		onValueChanged.RemoveAllListeners();
		if (hoverAnimationTween != null)
		{
			LeanTween.cancel(hoverAnimationTween.id);
			hoverAnimationTween = null;
		}
		base.OnDestroy();
	}

	private void OnValueChanged(bool active)
	{
		if (scaleWhenSelected && InputManager.GamePadInUse)
		{
			ToggleHighlight(active, animateScaling);
		}
		if (!active && scaleWhenSelected && !isMouseOver)
		{
			ToggleHighlight(active: false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ToggleHighlight(active: false, playAnimation: false);
	}

	protected override void OnDisable()
	{
		bool num = base.currentSelectionState == SelectionState.Selected;
		base.OnDisable();
		ToggleHighlight(active: false, playAnimation: false);
		if (num)
		{
			onDeselected.Invoke();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		isMouseOver = true;
		OnHighlight();
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonHover(base.gameObject);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		isMouseOver = false;
		OnUnhighlight();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (InputManager.GamePadInUse)
		{
			OnHighlight();
			onSelected.Invoke();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnUnhighlight();
		onDeselected.Invoke();
	}

	public void OnHighlight()
	{
		if (base.interactable || triggerNonInteractableHover)
		{
			onMouseEnter?.Invoke();
		}
		PlaySound(mouseEnterAudio, (mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		if (!base.isOn || !scaleWhenSelected)
		{
			ToggleHighlight(active: true, animateScaling);
		}
	}

	public void OnUnhighlight()
	{
		if (base.interactable || triggerNonInteractableHover)
		{
			onMouseExit?.Invoke();
		}
		PlaySound(mouseExitAudio, (mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		if (!base.isOn || !scaleWhenSelected)
		{
			ToggleHighlight(active: false, animateScaling);
		}
	}

	private void ToggleHighlight(bool active, bool playAnimation = true)
	{
		isHighlighted = active;
		if (active)
		{
			HighlightFinished(active: true);
		}
		if (!active && tooltip != null)
		{
			tooltip.ToggleEnable(active: false);
		}
		if (playAnimation)
		{
			if (LeanTween.isTweening(TargetRect))
			{
				LeanTween.cancel(TargetRect, "ToggleHighlight");
			}
			hoverAnimationTween = LeanTween.scale(TargetRect, active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one, animationDuration).setEase(LeanTweenType.easeOutExpo).setIgnoreTimeScale(useUnScaledTime: true);
			if (!active)
			{
				hoverAnimationTween.setOnComplete((Action)delegate
				{
					HighlightFinished(active: false);
				});
			}
		}
		else
		{
			TargetRect.localScale = (active ? new Vector3(highlightScaleFactor, highlightScaleFactor, 1f) : Vector3.one);
			if (!active)
			{
				HighlightFinished(active: false);
			}
		}
	}

	private void HighlightFinished(bool active)
	{
		if (tooltip != null)
		{
			tooltip.transform.localScale = (active ? new Vector3(1f / highlightScaleFactor, 1f / highlightScaleFactor, 1f) : Vector3.one);
			tooltip.transform.SetParent(active ? base.transform.parent : base.transform, worldPositionStays: true);
			tooltip.ToggleEnable(active);
		}
		if (ignoreScalingObjects != null && ignoreScalingObjects.Length != 0)
		{
			Transform[] array = ignoreScalingObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetParent(active ? base.transform.parent : base.transform, worldPositionStays: true);
			}
		}
	}
}
