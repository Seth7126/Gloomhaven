using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class CardsHandPreviewWindow : Singleton<CardsHandPreviewWindow>
{
	[SerializeField]
	private CardsHandPreviewUI previewPrefab;

	[SerializeField]
	private RectTransform previewsContainer;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button closeBackgroundButton;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private UiNavigationRoot previewNavigationRoot;

	private List<CardsHandPreviewUI> previews = new List<CardsHandPreviewUI>();

	private CardsHandPreviewUI currentPreviw;

	private Action onCancel;

	private int currentHandFocused;

	private UIWindow _window;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private bool _isFirstTimeFocus = true;

	public UIWindow Window => _window ?? GetComponent<UIWindow>();

	public bool IsOpenByKey { get; private set; }

	public bool IsOpen => Window.IsOpen;

	protected override void Awake()
	{
		base.Awake();
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Cancel);
		}
		closeBackgroundButton.onClick.AddListener(Cancel);
		Window.onHidden.AddListener(OnHidden);
		SetInstance(this);
		controllerArea.OnFocused.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocused.AddListener(OnControllerAreaUnfocused);
		InitGamepadInput();
	}

	protected override void OnDestroy()
	{
		DeinitGamepadInput();
		if (closeButton != null)
		{
			closeButton.onClick.RemoveListener(Cancel);
		}
		closeBackgroundButton.onClick.RemoveListener(Cancel);
		Window.onHidden.RemoveListener(OnHidden);
		controllerArea.OnFocused.RemoveListener(OnControllerAreaFocused);
		controllerArea.OnUnfocused.RemoveListener(OnControllerAreaUnfocused);
		base.OnDestroy();
	}

	public void InitGamepadInput()
	{
		if (InputManager.GamePadInUse)
		{
			_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
			_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, GoToPreviousStateWithWindowHide).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_ALL_CARDS, GoToPreviousStateWithWindowHide).AddBlocker(_simpleKeyActionHandlerBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker));
		}
	}

	public void DeinitGamepadInput()
	{
		if (InputManager.GamePadInUse)
		{
			_simpleKeyActionHandlerBlocker = null;
			_skipFrameKeyActionHandlerBlocker = null;
			if (Singleton<KeyActionHandlerController>.Instance != null)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, GoToPreviousStateWithWindowHide);
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_ALL_CARDS, GoToPreviousStateWithWindowHide);
			}
		}
	}

	public void EnableInput()
	{
		_simpleKeyActionHandlerBlocker.SetBlock(value: false);
		_skipFrameKeyActionHandlerBlocker.Run();
	}

	public void DisableInput()
	{
		_simpleKeyActionHandlerBlocker.SetBlock(value: true);
	}

	private void GoToPreviousStateWithWindowHide()
	{
		Window.Hide();
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	public void CreatePreview(CardsHandUI hand)
	{
		if (!hand.PlayerActor.IsDead)
		{
			CardsHandPreviewUI cardsHandPreviewUI = UnityEngine.Object.Instantiate(previewPrefab, previewsContainer);
			cardsHandPreviewUI.transform.SetAsFirstSibling();
			cardsHandPreviewUI.Init(hand, GoNext, GoPrevious);
			previews.Add(cardsHandPreviewUI);
		}
	}

	public IEnumerator CreatePreviewCoroutine(CardsHandUI hand)
	{
		if (!hand.PlayerActor.IsDead)
		{
			CardsHandPreviewUI preview = UnityEngine.Object.Instantiate(previewPrefab, previewsContainer);
			preview.transform.SetAsFirstSibling();
			yield return preview.InitCoroutine(hand, GoNext, GoPrevious);
			previews.Add(preview);
		}
	}

	public void UpdateCharText()
	{
		foreach (CardsHandPreviewUI preview in previews)
		{
			preview.UpdateCharText();
		}
	}

	public void Show(bool cardsSelectable, Action onCancel = null, bool openByKey = false)
	{
		IsOpenByKey = openByKey;
		if (closeButton != null)
		{
			closeButton.gameObject.SetActive(!openByKey);
		}
		closeBackgroundButton.interactable = !openByKey;
		int num = previews.Count - 1;
		this.onCancel = onCancel;
		for (int i = 0; i < previews.Count; i++)
		{
			if (previews[i].PlayerActor.IsDead)
			{
				previews[i].Hide();
				continue;
			}
			if (i < num && previews[i].PlayerActor.HasAbilityCards())
			{
				num = i;
			}
			previews[i].Show(cardsSelectable);
		}
		SetCurrentPreview(num);
		base.gameObject.SetActive(value: true);
		Window.Show();
		controllerArea.enabled = true;
		controllerArea.Focus();
		InitiativeTrack.Instance.ToggleSortingOrder(value: false);
		InteractabilityHighlightCanvas.s_Instance?.ToggleHighlights(isActive: false);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: true);
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: true);
	}

	public void ReleasePreviews()
	{
		foreach (CardsHandPreviewUI preview in previews)
		{
			preview.Release();
		}
		previews.Clear();
	}

	public void SetCardsSelectable(bool selectable)
	{
		if (!Window.IsOpen)
		{
			return;
		}
		for (int i = 0; i < previews.Count; i++)
		{
			if (!previews[i].PlayerActor.IsDead)
			{
				previews[i].SetCardsSelectable(selectable && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			}
		}
	}

	public void Cancel()
	{
		Window.Hide();
	}

	public void ClearLastDeckState()
	{
		Singleton<UINavigation>.Instance.StateMachine.RemoveLast(Singleton<UINavigation>.Instance.StateMachine.GetState(ScenarioStateTag.Deck));
	}

	public void Hide()
	{
		onCancel = null;
		Window.Hide();
	}

	private void OnHidden()
	{
		InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: false);
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: false);
		IsOpenByKey = false;
		for (int i = 0; i < previews.Count; i++)
		{
			previews[i].ResetView();
		}
		controllerArea.enabled = false;
		onCancel?.Invoke();
		InteractabilityHighlightCanvas.s_Instance?.ToggleHighlights(isActive: true);
	}

	private void SetNavigationRoot()
	{
		if (_isFirstTimeFocus)
		{
			previewNavigationRoot.enabled = false;
			CoroutineHelper.RunNextFrame(delegate
			{
				previewNavigationRoot.enabled = true;
				Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(previewNavigationRoot);
			});
		}
		else
		{
			Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(previewNavigationRoot);
		}
	}

	private void OnControllerAreaFocused()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<InputManager>.Instance.DisableAllMouses();
			SetNavigationRoot();
			if (currentPreviw == null)
			{
				currentPreviw = previews[0];
			}
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < previews.Count; i++)
			{
				if (!previews[i].PlayerActor.IsDead)
				{
					previews[i].EnableNavigation();
					if (!flag)
					{
						previews[i].Select();
						flag = true;
						currentHandFocused = i;
					}
				}
			}
		}
		if (!InputManager.GamePadInUse)
		{
			Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_MERCENARY_DECK"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.CONTROL_DECK)), LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_MERCENARY_DECK_TITLE"));
		}
		InputManager.RegisterToOnPressed(KeyAction.UI_NEXT_TAB, GoNext);
		InputManager.RegisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, GoPrevious);
		InputManager.RequestDisableInput(this, EKeyActionTag.AreaShortcuts);
		InputManager.RequestEnableInput(this, KeyAction.CONTROL_DECK);
	}

	private void OnControllerAreaUnfocused()
	{
		Singleton<HelpBox>.Instance.ClearOverrideController();
		if (InputManager.GamePadInUse)
		{
			Singleton<InputManager>.Instance.EnableAllMouses();
		}
		else
		{
			currentHandFocused = -1;
			for (int i = 0; i < previews.Count; i++)
			{
				previews[i].DisableNavigation();
			}
		}
		InputManager.UnregisterToOnPressed(KeyAction.UI_NEXT_TAB, GoNext);
		InputManager.UnregisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, GoPrevious);
		InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
	}

	private void SetCurrentPreview(int index)
	{
		if (currentPreviw != null)
		{
			currentPreviw.ResetView();
		}
		currentHandFocused = index;
		currentPreviw = previews[currentHandFocused];
		currentPreviw.Select();
	}

	private void GoNext()
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		for (int i = 1; i < previews.Count; i++)
		{
			int num = (currentHandFocused + i) % previews.Count;
			if (!previews[num].PlayerActor.IsDeadOrHasNotAbilityCards())
			{
				SetCurrentPreview(num);
				break;
			}
		}
	}

	private void GoPrevious()
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		for (int num = currentHandFocused - 1; num >= 0; num--)
		{
			if (!previews[num].PlayerActor.IsDeadOrHasNotAbilityCards())
			{
				SetCurrentPreview(num);
				return;
			}
		}
		for (int num2 = previews.Count - 1; num2 > currentHandFocused; num2--)
		{
			if (!previews[num2].PlayerActor.IsDeadOrHasNotAbilityCards())
			{
				SetCurrentPreview(num2);
				break;
			}
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			InputManager.UnregisterToOnPressed(KeyAction.UI_NEXT_TAB, GoNext);
			InputManager.UnregisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, GoPrevious);
			InputManager.UnregisterToOnPressed(KeyAction.CONTROL_DECK, Cancel);
			InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
		}
	}
}
