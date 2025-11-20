using System;
using AsmodeeNet.Foundation;
using Script.GUI.Cards_Hand;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

public class FullCardHandViewer : Singleton<FullCardHandViewer>, IEscapable
{
	[SerializeField]
	private ExtendedScrollRect cardScroll;

	[SerializeField]
	private CanvasGroup cardsInteraction;

	[SerializeField]
	private GameObject viewer;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button closeBackgroundButton;

	[SerializeField]
	private FullCardNavigationTranslator cardNavigationTranslator;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[SerializeField]
	private ControllerInputScroll _controllerInputScroll;

	private Action onClose;

	private bool openByKey;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public RectTransform CardContainer => cardScroll.content;

	public FullCardNavigationTranslator CardNavigationTranslator => cardNavigationTranslator;

	public bool IsActive => viewer.activeSelf;

	public bool IsOpenByKey
	{
		get
		{
			if (viewer.gameObject.activeSelf)
			{
				return openByKey;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Hide);
		}
		closeBackgroundButton.onClick.AddListener(Hide);
	}

	protected override void OnDestroy()
	{
		if (closeButton != null)
		{
			closeButton.onClick.RemoveListener(Hide);
		}
		closeBackgroundButton.onClick.RemoveListener(Hide);
		base.OnDestroy();
	}

	public void ToggleControllerInputScroll(bool isEnabled)
	{
		if (_controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = isEnabled;
		}
	}

	public void Hide()
	{
		InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		Singleton<UINavigation>.Instance.NavigationManager.DeselectCurrentSelectable();
		openByKey = false;
		if (viewer.activeSelf)
		{
			viewer.SetActive(value: false);
			UIWindowManager.UnregisterEscapable(this);
			ControllerInputAreaManager.Instance.UnfocusArea(EControllerInputAreaType.FullAbilityCards);
			onClose?.Invoke();
		}
		Singleton<ActorStatPanel>.Instance.DoShow = true;
		Singleton<UITextInfoPanel>.Instance.DoShow = true;
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			UIWindowManager.UnregisterEscapable(this);
			ControllerInputAreaManager.Instance.UnfocusArea(EControllerInputAreaType.FullAbilityCards);
		}
	}

	public void Show(Action onClose = null, bool openByKey = false)
	{
		InitiativeTrack.Instance.ToggleSortingOrder(value: false);
		this.onClose = onClose;
		SetOpenByKey(openByKey);
		cardScroll.ScrollToTop();
		viewer.SetActive(value: true);
		UIWindowManager.RegisterEscapable(this);
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.FullAbilityCards);
		Singleton<ActorStatPanel>.Instance.DoShow = false;
		Singleton<UITextInfoPanel>.Instance.DoShow = false;
	}

	public void SetOpenByKey(bool openByKey)
	{
		this.openByKey = openByKey;
		if (closeButton != null)
		{
			closeButton.gameObject.SetActive(!openByKey);
		}
		closeBackgroundButton.interactable = !openByKey;
	}

	public void SetCardsInteractable(bool interactable)
	{
		cardsInteraction.interactable = interactable;
	}

	public bool Escape()
	{
		UIWindowManager.UnregisterEscapable(this);
		if (!viewer.gameObject.activeSelf || IsOpenByKey)
		{
			return false;
		}
		Hide();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
