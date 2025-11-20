using Code.State;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class RoundStartScenarioState : ScenarioState
{
	private KeyActionHandler _guiActionHandlerPress;

	private KeyActionHandler _guiActionHandlerRelease;

	private SimpleKeyActionHandlerBlocker _multiplayerReadyBlocker = new SimpleKeyActionHandlerBlocker();

	private IHotkeySession _hotkeySession;

	private SessionHotkey _hideCardsHotkey;

	private SessionHotkey _showCardsHotkey;

	private bool _toggleState;

	private bool _canToggleCards;

	public override ScenarioStateTag StateTag => ScenarioStateTag.RoundStart;

	protected virtual bool CanToggleCards => true;

	public RoundStartScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (TransitionManager.s_Instance.TransitionDone)
		{
			InitializeInput();
		}
		else
		{
			TransitionManager.s_Instance.OnTransitionFinished += InitializeInput;
		}
		Singleton<UINavigation>.Instance.NavigationManager.SetRootsOnEnable = false;
		InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
		_navigationManager.DeselectAll();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UINavigation>.Instance.NavigationManager.SetRootsOnEnable = true;
		TransitionManager.s_Instance.OnTransitionFinished -= InitializeInput;
		ReleaseInput();
	}

	private void InitializeInput()
	{
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.Scenario);
		if (AdventureState.MapState != null && !AdventureState.MapState.IsPlayingTutorial)
		{
			_canToggleCards = CanToggleCards;
		}
		else
		{
			_canToggleCards = false;
		}
		_hotkeySession = Hotkeys.Instance.GetSession();
		_hotkeySession.AddOrReplaceHotkeys("Highlight", "AllCards");
		_hideCardsHotkey = _hotkeySession.GetHotkey("HideCards");
		_showCardsHotkey = _hotkeySession.GetHotkey("ShowCards");
		_hideCardsHotkey.SetShown(_canToggleCards);
		CardsHandManager.Instance.EnableAllCardsCombo(PhaseManager.Phase.Type != CPhase.PhaseType.EndTurn);
		CardsHandManager.Instance.EnableAllDeckSelection(PhaseManager.Phase.Type != CPhase.PhaseType.EndTurn);
		UseActionScenarioState.EnableMouse();
		SubscribeInput();
	}

	private void ReleaseInput()
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		UseActionScenarioState.DisableMouse();
		ToggleCards(forceOff: true);
		UnsubscribeInput();
		_hotkeySession?.Dispose();
		_hotkeySession = null;
	}

	protected virtual void SubscribeInput()
	{
		InputManager.RegisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_LEFT, SwitchToPreviousMercenary);
		InputManager.RegisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_RIGHT, SwitchToNextMercenary);
		if (_canToggleCards)
		{
			_guiActionHandlerPress = new KeyActionHandler(KeyAction.UI_INFO, delegate
			{
				Choreographer.s_Choreographer.HideGameGUI();
				Hotkeys.Instance.SetActiveState(state: false);
				InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.UI_INFO, KeyAction.MOVE_CAMERA_UP, KeyAction.MOVE_CAMERA_DOWN, KeyAction.MOVE_CAMERA_LEFT, KeyAction.MOVE_CAMERA_RIGHT, KeyAction.ROTATE_CAMERA_RIGHT, KeyAction.ROTATE_CAMERA_LEFT);
			});
			_guiActionHandlerRelease = new KeyActionHandler(KeyAction.UI_INFO, delegate
			{
				Choreographer.s_Choreographer.ShowGameGUI();
				Hotkeys.Instance.SetActiveState(state: true);
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
			}, null, null, isPersistent: false, KeyActionHandler.RegisterType.Release);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(_guiActionHandlerPress);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(_guiActionHandlerRelease);
		}
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.VERTICAL_SHORTCUT_UP, SwitchToCards).AddBlocker(_multiplayerReadyBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.VERTICAL_SHORTCUT_DOWN, SwitchToCards).AddBlocker(_multiplayerReadyBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_PREVIOUS_TAB, SwitchToCards).AddBlocker(_multiplayerReadyBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_NEXT_TAB, SwitchToCards).AddBlocker(_multiplayerReadyBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, ToggleUI).AddBlocker(_multiplayerReadyBlocker));
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.OnCardSelectionReadyChanged += OnMultiplayerCardSelectionReadyChanged;
		}
		SetMultiplayerReadyBlockedState(FFSNetwork.IsOnline && Singleton<UIScenarioMultiplayerController>.Instance.IsReady);
	}

	protected virtual void UnsubscribeInput()
	{
		_guiActionHandlerRelease?.Action();
		InputManager.UnregisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_LEFT, SwitchToPreviousMercenary);
		InputManager.UnregisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_RIGHT, SwitchToNextMercenary);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_guiActionHandlerPress);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_guiActionHandlerRelease);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.VERTICAL_SHORTCUT_UP, SwitchToCards);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.VERTICAL_SHORTCUT_DOWN, SwitchToCards);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_PREVIOUS_TAB, SwitchToCards);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_NEXT_TAB, SwitchToCards);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, ToggleUI);
		Singleton<UIScenarioMultiplayerController>.Instance.OnCardSelectionReadyChanged -= OnMultiplayerCardSelectionReadyChanged;
	}

	protected virtual void SwitchToNextMercenary()
	{
		ToggleCards(forceOff: true);
		CardsHandManager.Instance.SwitchToNextMercenary();
	}

	protected virtual void SwitchToPreviousMercenary()
	{
		ToggleCards(forceOff: true);
		CardsHandManager.Instance.SwitchToPreviousMercenary();
	}

	private void SwitchToCards()
	{
		ToggleCards(forceOff: true);
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CardSelection);
		CardsHandManager.Instance.ControllerInputArea.Focus();
	}

	private void OnMultiplayerCardSelectionReadyChanged(bool ready)
	{
		SetMultiplayerReadyBlockedState(ready);
		if (!ready)
		{
			SwitchToCards();
		}
	}

	private void SetMultiplayerReadyBlockedState(bool block)
	{
		_multiplayerReadyBlocker.SetBlock(block);
		if (block)
		{
			_hideCardsHotkey.SetShown(shown: false);
			_showCardsHotkey.SetShown(shown: false);
		}
	}

	private void ToggleUI()
	{
		ToggleCards();
	}

	private void ToggleCards(bool forceOff = false)
	{
		if (!_canToggleCards)
		{
			return;
		}
		CardsHandUI activeHand = CardsHandManager.Instance.GetActiveHand();
		if (activeHand == null)
		{
			Debug.LogError("Active hand is missing!");
		}
		else if (forceOff)
		{
			if (_hotkeySession != null)
			{
				_hideCardsHotkey.SetShown(shown: false);
				_showCardsHotkey.SetShown(shown: false);
			}
			CardsHandManager.Instance.cardHandsUI.ForEach(delegate(CardsHandUI hand)
			{
				hand?.Toggle(active: true);
				hand?.ItemsUI.Toggle(active: true);
			});
			_toggleState = false;
		}
		else
		{
			if (_hotkeySession != null)
			{
				_hideCardsHotkey.SetShown(_toggleState);
				_showCardsHotkey.SetShown(!_toggleState);
			}
			activeHand.Toggle(_toggleState);
			activeHand.ItemsUI.Toggle(_toggleState);
			_toggleState = !_toggleState;
		}
	}
}
