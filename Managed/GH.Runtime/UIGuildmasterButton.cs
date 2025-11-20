using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(GuildmasterModeSelectable), typeof(LayoutElement))]
public class UIGuildmasterButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IMoveHandler
{
	[Serializable]
	public class GuildmasterButtonEvent : UnityEvent<EGuildmasterMode>
	{
	}

	[Serializable]
	public class GuildmasterHoverEvent : UnityEvent<EGuildmasterMode, bool>
	{
	}

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image hoverMask;

	[SerializeField]
	private EGuildmasterMode mode;

	[SerializeField]
	private UINewNotificationTip newNotification;

	[SerializeField]
	private LoopAnimator highlightAnimator;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private UITextTooltipTarget tooltipTarget;

	public GuildmasterButtonEvent OnSelected;

	public GuildmasterHoverEvent OnHovered;

	private Action navigatePrevious;

	private Action navigateNext;

	private bool highlight;

	private bool isHovered;

	private LayoutElement layoutElement;

	private HashSet<Component> hideRequests;

	private Func<bool> canToggle;

	public EGuildmasterMode GuildmasterMode => mode;

	public bool IsActive => base.gameObject.activeInHierarchy;

	public GuildmasterModeSelectable GuildmasterModeSelectable { get; private set; }

	private LayoutElement LayoutElement
	{
		get
		{
			if (layoutElement == null)
			{
				layoutElement = GetComponent<LayoutElement>();
			}
			return layoutElement;
		}
	}

	public bool IsShown
	{
		get
		{
			if (hideRequests.IsNullOrEmpty())
			{
				return base.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	private void Awake()
	{
		GuildmasterModeSelectable = GetComponent<GuildmasterModeSelectable>();
		toggle.SetIsOnWithoutNotify(value: false);
		toggle.onValueChanged.AddListener(OnToggled);
		RefreshSelected();
		RefreshHighlight();
		Initialize();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		toggle.onValueChanged.RemoveAllListeners();
		highlightAnimator?.StopLoop();
	}

	private void Initialize()
	{
		icon.sprite = UIInfoTools.Instance.GetGuildmasterModeSprite(mode);
	}

	public void SetMode(EGuildmasterMode mode, bool newNotification = false, Func<bool> canToggle = null)
	{
		this.mode = mode;
		this.canToggle = canToggle;
		Initialize();
		ShowNewNotification(newNotification);
	}

	public void ShowNewNotification(bool show)
	{
		if (show)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
	}

	public void Highlight(bool enable)
	{
		highlight = enable;
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (!(highlightAnimator == null))
		{
			if (highlight && !isHovered)
			{
				highlightAnimator.gameObject.SetActive(value: true);
				highlightAnimator.StartLoop(resetToInitial: true);
			}
			else
			{
				highlightAnimator.StopLoop();
				highlightAnimator.gameObject.SetActive(value: false);
			}
		}
	}

	public void ShowHovered(bool hovered)
	{
		SetHovered(hovered);
		if (hovered)
		{
			toggle.OnPointerEnter(null);
		}
		else
		{
			toggle.OnPointerExit(null);
		}
	}

	private void Update()
	{
		if (InputManager.GamePadInUse)
		{
			hoverMask.enabled = toggle.isOn;
		}
	}

	private void SetHovered(bool hovered)
	{
		isHovered = hovered;
		hoverMask.enabled = (isHovered && InputManager.GamePadInUse) || toggle.interactable;
		RefreshHighlight();
	}

	private void OnToggled(bool isToggled)
	{
		if (!isToggled || canToggle == null || canToggle())
		{
			OnValueChanged(isToggled);
		}
	}

	private void OnValueChanged(bool selected)
	{
		RefreshSelected();
		if (selected)
		{
			OnSelected.Invoke(mode);
		}
	}

	private void RefreshSelected()
	{
		bool num = EventSystem.current.currentSelectedGameObject == toggle.gameObject;
		toggle.interactable = !toggle.isOn;
		if (num && EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(toggle.gameObject);
		}
		if (!InputManager.GamePadInUse)
		{
			icon.material = (toggle.isOn ? UIInfoTools.Instance.disabledGrayscaleMaterial : null);
			hoverMask.enabled = isHovered && toggle.interactable;
		}
	}

	public void Select()
	{
		if (!toggle.isOn)
		{
			toggle.SetValue(value: true);
			OnValueChanged(selected: true);
		}
	}

	public void Deselect()
	{
		if (toggle.isOn)
		{
			toggle.SetValue(value: false);
			OnValueChanged(selected: false);
		}
	}

	public void ToggleGreyOut(bool greyedOut = true, string tooltip = null)
	{
		bool num = EventSystem.current.currentSelectedGameObject == toggle.gameObject;
		toggle.interactable = !greyedOut;
		if (num && EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(toggle.gameObject);
		}
		icon.material = (greyedOut ? UIInfoTools.Instance.disabledGrayscaleMaterial : null);
		hoverMask.enabled = isHovered && toggle.interactable;
		Highlight(!greyedOut);
		if (!toggle.interactable)
		{
			toggle.isOn = false;
		}
		if (tooltip.IsNullOrEmpty())
		{
			tooltipTarget.gameObject.SetActive(value: false);
			return;
		}
		tooltipTarget.SetText(tooltip, refreshTooltip: true);
		tooltipTarget.gameObject.SetActive(value: true);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetHovered(hovered: true);
		OnHovered.Invoke(mode, arg1: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetHovered(hovered: false);
		OnHovered.Invoke(mode, arg1: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		SetHovered(hovered: true);
		OnHovered.Invoke(mode, arg1: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		SetHovered(hovered: false);
		OnHovered.Invoke(mode, arg1: false);
	}

	public void Show(Component request = null)
	{
		hideRequests?.Remove((request == null) ? this : request);
		if (hideRequests == null || !hideRequests.Any())
		{
			LayoutElement.ignoreLayout = false;
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = true;
		}
	}

	public void Hide(Component request = null)
	{
		if (hideRequests == null)
		{
			hideRequests = new HashSet<Component>();
		}
		hideRequests.Add((request == null) ? this : request);
		LayoutElement.ignoreLayout = true;
		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			SetHovered(hovered: false);
		}
	}

	private void OnEnable()
	{
		RefreshHighlight();
	}

	public void EnableNavigation(Action navigatePrevious, Action navigateNext)
	{
		this.navigateNext = navigateNext;
		this.navigatePrevious = navigatePrevious;
	}

	public void DisableNavigation()
	{
		navigatePrevious = null;
		navigateNext = null;
		toggle.DisableNavigation();
	}

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
			navigatePrevious?.Invoke();
			break;
		case MoveDirection.Right:
			navigateNext?.Invoke();
			break;
		}
	}
}
