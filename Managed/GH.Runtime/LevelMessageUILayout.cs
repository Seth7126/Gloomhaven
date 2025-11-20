#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Code.State;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary.CustomLevels;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelMessageUILayout : LocalizedListener
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Transform pageContainer;

	[SerializeField]
	private List<LevelMessagePageUI> pagesUI;

	[SerializeField]
	public PaginationHandler pagination;

	[SerializeField]
	public ExtendedButton closeButton;

	[SerializeField]
	private string audioItemShowMessage;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private Hotkey hotkey;

	[Header("Navigation")]
	[SerializeField]
	private UiNavigationRoot root;

	private CLevelMessage _message;

	private Action _onCloseButtonPressed;

	private bool _triggeredByDismiss;

	private int _focusedFrame;

	private Coroutine _setRootRoutine;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(QuestLogState), typeof(AnimationScenarioState)).InverseFilter();

	protected void Awake()
	{
		if (controllerArea != null)
		{
			controllerArea.OnFocusedArea.AddListener(delegate
			{
				_focusedFrame = Time.frameCount;
			});
		}
		if (closeButton != null)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, CloseButtonPressed, ShowHotkey, HideHotkey).AddBlocker(new ExtendedButtonActiveKeyActionHandlerBlocker(closeButton)));
			if (controllerArea != null)
			{
				Singleton<KeyActionHandlerController>.Instance.AddBlockerForHandler(KeyAction.UI_SUBMIT, CloseButtonPressed, new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea));
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (title != null)
		{
			StartCoroutine(WaitRegenerateSubmeshes());
		}
	}

	protected override void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			Clear();
			base.OnDisable();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, CloseButtonPressed);
		}
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChangedEvent));
		HideHotkey();
	}

	public void Init(CLevelMessage message, Action onClose = null, bool playOpenSound = false)
	{
		if (closeButton != null)
		{
			closeButton.gameObject.SetActive(value: false);
		}
		_triggeredByDismiss = message.DismissTrigger.IsTriggeredByDismiss;
		if (_triggeredByDismiss && closeButton != null)
		{
			closeButton.gameObject.SetActive(value: true);
			closeButton.buttonText.text = LocalizationManager.GetTranslation("GUI_CONTINUE");
			if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.FixedLowerRight)
			{
				title.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>().padding.bottom = (InputManager.GamePadInUse ? 60 : 40);
			}
		}
		else if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.FixedLowerRight)
		{
			title.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
		}
		Image component = GetComponent<Image>();
		if (component != null)
		{
			component.enabled = message.ShowScreenBG;
		}
		_onCloseButtonPressed = onClose;
		_message = message;
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChangedEvent));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChangedEvent));
		if (!message.TitleKey.IsNullOrEmpty())
		{
			if (message.TitleKeyController.IsNullOrEmpty())
			{
				title.text = LocalizationManager.GetTranslation(message.TitleKey);
			}
			else
			{
				title.text = (InputManager.GamePadInUse ? Singleton<InputManager>.Instance.LocalizeControls(LocalizationManager.GetTranslation(message.TitleKeyController)) : LocalizationManager.GetTranslation(message.TitleKey));
			}
			title.gameObject.SetActive(value: true);
		}
		else
		{
			title.gameObject.SetActive(value: false);
		}
		if (message.Pages.Count > 0 && pagination != null)
		{
			PaginationHandler paginationHandler = pagination;
			paginationHandler.OnPageChanged = (Action<int, int>)Delegate.Combine(paginationHandler.OnPageChanged, new Action<int, int>(OnPageChanged));
			CreatePages(message.Pages);
		}
		if (playOpenSound)
		{
			AudioControllerUtils.PlaySound(audioItemShowMessage);
		}
		base.gameObject.SetActive(value: true);
		if (closeButton != null && closeButton.gameObject.activeSelf)
		{
			if (Singleton<ESCMenu>.Instance == null || !Singleton<ESCMenu>.Instance.IsOpen)
			{
				controllerArea?.Enable();
			}
			Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.LevelMessage, new MainStateData(root));
		}
	}

	public void EnableArea()
	{
		controllerArea?.Enable();
	}

	public void OnPageChanged(int pageChangedTo, int maxPages)
	{
	}

	public void CloseButtonPressed()
	{
		_ = Time.frameCount;
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (controllerArea != null && controllerArea.IsFocused && Time.frameCount - _focusedFrame < 2)
		{
			Debug.LogWarningController("Skip close button");
			return;
		}
		if (pagination.currentPage < pagination.pages.Count)
		{
			pagination.GoNextPage();
			return;
		}
		if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<LevelMessageState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_stateFilter);
			InputManager.SkipNextSubmitAction();
		}
		controllerArea?.Destroy();
		_onCloseButtonPressed?.Invoke();
	}

	public void Hide()
	{
		Clear();
		base.gameObject.SetActive(value: false);
	}

	public void Clear()
	{
		controllerArea?.Destroy();
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChangedEvent));
	}

	protected override void OnLanguageChanged()
	{
		if (title != null && _message != null && title.gameObject.activeSelf)
		{
			if (InputManager.GamePadInUse && _message.TitleKeyController.IsNOTNullOrEmpty())
			{
				title.text = Singleton<InputManager>.Instance.LocalizeControls(LocalizationManager.GetTranslation(_message.TitleKeyController));
			}
			else
			{
				title.text = LocalizationManager.GetTranslation(_message.TitleKey);
			}
		}
	}

	private void OnControllerTypeChangedEvent(ControllerType obj)
	{
		OnLanguageChanged();
		if (pagination != null)
		{
			for (int i = 0; i < _message.Pages.Count && pagesUI[i].gameObject.activeSelf; i++)
			{
				pagesUI[i].RefreshText();
			}
		}
	}

	private void CreatePages(List<CLevelMessagePage> pages)
	{
		HelperTools.NormalizePool(ref pagesUI, pagesUI[0].gameObject, pageContainer, pages.Count);
		for (int i = 0; i < pages.Count; i++)
		{
			pagesUI[i].transform.SetSiblingIndex(1);
			pagesUI[i].Init(pages[i]);
		}
		pagination.Init((from it in pagesUI
			where it.gameObject.activeSelf
			select it.gameObject).ToList());
	}

	private void ShowHotkey()
	{
		hotkey?.Initialize(Singleton<UINavigation>.Instance.Input);
	}

	private void HideHotkey()
	{
		hotkey?.DisplayHotkey(active: false);
		hotkey?.Deinitialize();
	}

	private IEnumerator WaitRegenerateSubmeshes()
	{
		yield return null;
		RefreshSubmeshes();
	}

	private void RefreshSubmeshes()
	{
		TMP_SubMeshUI[] componentsInChildren = title.GetComponentsInChildren<TMP_SubMeshUI>();
		foreach (TMP_SubMeshUI tMP_SubMeshUI in componentsInChildren)
		{
			if (!(tMP_SubMeshUI.spriteAsset == null))
			{
				tMP_SubMeshUI.transform.SetParent(title.transform.parent);
				tMP_SubMeshUI.transform.SetSiblingIndex(title.transform.GetSiblingIndex());
			}
		}
	}
}
