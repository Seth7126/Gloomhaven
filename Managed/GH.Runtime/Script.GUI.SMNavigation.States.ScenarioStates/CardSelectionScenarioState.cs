using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class CardSelectionScenarioState : ScenarioState
{
	private bool _recoverMode;

	private IHotkeySession _hotkeySession;

	private CardSelectHotkeys _cardSelectHotkeys = new CardSelectHotkeys();

	private SessionHotkey _tipHotkey;

	public override ScenarioStateTag StateTag => ScenarioStateTag.CardSelection;

	public CardSelectionScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UITextInfoPanel>.Instance.DoShow = false;
		_hotkeySession = Hotkeys.Instance.GetSession();
		_cardSelectHotkeys.Enter(_hotkeySession);
		_hotkeySession.AddOrReplaceHotkeys("Highlight");
		_tipHotkey = _hotkeySession.GetHotkey("Tips", delegate
		{
			TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
		});
		CardHandMode currentMode = CardsHandManager.Instance.CurrentHand.currentMode;
		_recoverMode = currentMode == CardHandMode.RecoverDiscardedCard || currentMode == CardHandMode.RecoverLostCard || currentMode == CardHandMode.IncreaseCardLimit;
		if (!_recoverMode)
		{
			Singleton<InputManager>.Instance.PlayerControl.UICancel.OnPressed += OnCancel;
			Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnPressed += OnPreviousTab;
			Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnPressed += OnNextTab;
			Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutLeft.OnPressed += SwitchToPreviousMercenary;
			Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutRight.OnPressed += SwitchToNextMercenary;
			Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutUp.OnPressed += UIMoveUp;
			Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutDown.OnPressed += UIMoveDown;
			_hotkeySession.AddOrReplaceHotkeys("Back", "AllCards");
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIScenarioMultiplayerController>.Instance.OnCardSelectionReadyChanged += OnMultiplayerCardSelectionReadyChanged;
			}
			CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: true);
		}
		InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
		FullAbilityCard.FullCardHoveringStateChanged += AnyFullCardHighlightChanged;
		AnyFullCardHighlightChanged(isHighlight: false);
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.Hide(this);
		if (CardsHandManager.Instance.CurrentCharacterSymbolTab != null && !CardsHandManager.Instance.CurrentCharacterSymbolTab.PlayerActor.IsDead)
		{
			InitiativeTrack.Instance.Select(CardsHandManager.Instance.ActivePlayer);
			CardsHandManager.Instance.CurrentCharacterSymbolTab.GrayOutTab(isGrayedOut: false);
			CardsHandManager.Instance.RefreshCardHandTabs();
		}
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands", selectFirst: false);
		Choreographer.s_Choreographer.readyButton.SetSendDefaultSubmitEvent(enable: true);
		Singleton<UIReadyToggle>.Instance.SetSendDefaultSubmitEvent(enable: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Choreographer.s_Choreographer.readyButton.SetSendDefaultSubmitEvent(enable: false);
		Singleton<UIReadyToggle>.Instance.SetSendDefaultSubmitEvent(enable: false);
		Singleton<UITextInfoPanel>.Instance.DoShow = true;
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		if (!_recoverMode)
		{
			Singleton<InputManager>.Instance.PlayerControl.UICancel.OnPressed -= OnCancel;
			Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnPressed -= OnPreviousTab;
			Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnPressed -= OnNextTab;
			Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutLeft.OnPressed -= SwitchToPreviousMercenary;
			Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutRight.OnPressed -= SwitchToNextMercenary;
			Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutUp.OnPressed -= UIMoveUp;
			Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutDown.OnPressed -= UIMoveDown;
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIScenarioMultiplayerController>.Instance.OnCardSelectionReadyChanged -= OnMultiplayerCardSelectionReadyChanged;
			}
		}
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		FullAbilityCard.FullCardHoveringStateChanged -= AnyFullCardHighlightChanged;
		CardsHandManager.Instance.CurrentCharacterSymbolTab.GrayOutTab(isGrayedOut: true);
		_navigationManager.DeselectAll();
		if (CardsHandManager.Instance != null && CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		_cardSelectHotkeys.Exit();
		_hotkeySession.Dispose();
		TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
	}

	private void OnMultiplayerCardSelectionReadyChanged(bool ready)
	{
		if (ready)
		{
			SwitchToRoundStartState();
		}
	}

	private void OnCancel()
	{
		SwitchToRoundStartState();
	}

	public void SwitchToRoundStartState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.RoundStart);
	}

	private void OnNextTab()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		CardsHandManager.Instance.GoToNextHand();
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands");
		Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
	}

	private void OnPreviousTab()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		CardsHandManager.Instance.GoToPreviousHand();
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands");
		Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
	}

	private void SwitchToNextMercenary()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		CardsHandManager.Instance.GoToNextHand();
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands");
	}

	private void SwitchToPreviousMercenary()
	{
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		CardsHandManager.Instance.GoToPreviousHand();
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands");
	}

	private void UIMoveUp()
	{
		UIMove(UINavigationDirection.Up);
	}

	private void UIMoveDown()
	{
		UIMove(UINavigationDirection.Down);
	}

	private void UIMove(UINavigationDirection direction)
	{
		if (Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot?.Name != "CardHands")
		{
			Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardHands");
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
		}
		else
		{
			InputManager.MoveSelectionEvent?.Invoke(new UIActionBaseEventData
			{
				UINavigationDirection = direction,
				UINavigationSourceType = UINavigationSourceType.Stick
			});
		}
	}

	private void AnyFullCardHighlightChanged(bool isHighlight)
	{
		_tipHotkey.SetShown(isHighlight);
	}
}
