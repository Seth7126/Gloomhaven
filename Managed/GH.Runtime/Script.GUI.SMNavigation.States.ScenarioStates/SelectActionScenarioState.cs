using Code.State;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectActionScenarioState : ScenarioState
{
	private IHotkeySession _hotkeySession;

	private CardActionSelectHotkeys _cardActionSelectHotkeys = new CardActionSelectHotkeys();

	public override ScenarioStateTag StateTag => ScenarioStateTag.SelectAction;

	public SelectActionScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		InputManager.SkipNextSubmitAction();
		_hotkeySession = Hotkeys.Instance.GetSession();
		_cardActionSelectHotkeys.Enter(_hotkeySession);
		_hotkeySession.AddOrReplaceHotkeys(("Back", null), ("Select", null), ("Tips", ToggleTooltip), ("Highlight", null), ("AllCards", null));
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: true);
		if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.Complete && _stateMachine.PreviousState is SelectItemState)
		{
			Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDTURN, string.Format(LocalizationManager.GetTranslation(GameState.TurnActor.IsTakingExtraTurn ? "GUI_END_EXTRA_TURN" : "GUI_END_TURN"), LocalizationManager.GetTranslation(GameState.TurnActor.ActorLocKey())), hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: true, hideSelectionOnEndTurn: false);
			Choreographer.s_Choreographer.readyButton.SetDisableVisualState(value: false);
		}
		if (CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaFocused();
		}
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.Hide(this);
		if (ControllerInputAreaManager.Instance.m_FocusArea != "Tutorial Message Box fIXED")
		{
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
		}
		InitiativeTrack.Instance.DisplayControllerTips();
		InitiativeTrack.Instance.ReorderControllerTips(isEnemyActionState: true);
		InputManager.RegisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, OnReleased);
		InputManager.RegisterToOnPressed(KeyAction.UI_NEXT_TAB, OnReleased);
		InputManager.RegisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.RegisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, ToHexMovementState);
		Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutLeft.OnPressed += UIMoveLeft;
		Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutRight.OnPressed += UIMoveRight;
		Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutUp.OnPressed += UIMoveUp;
		Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutDown.OnPressed += UIMoveDown;
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		_cardActionSelectHotkeys.Exit();
		_hotkeySession.Dispose();
		_hotkeySession = null;
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
		if (CardsHandManager.Instance != null && CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		InputManager.UnregisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, OnReleased);
		InputManager.UnregisterToOnPressed(KeyAction.UI_NEXT_TAB, OnReleased);
		InputManager.UnregisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.UnregisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, ToHexMovementState);
		Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutLeft.OnPressed -= UIMoveLeft;
		Singleton<InputManager>.Instance.PlayerControl.HorizontalShortcutRight.OnPressed -= UIMoveRight;
		Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutUp.OnPressed -= UIMoveUp;
		Singleton<InputManager>.Instance.PlayerControl.VerticalShortcutDown.OnPressed -= UIMoveDown;
		TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
	}

	private void UIMoveLeft()
	{
		UIMove(UINavigationDirection.Left);
	}

	private void UIMoveRight()
	{
		UIMove(UINavigationDirection.Right);
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
		if (Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot?.Name != "CardsHighlight")
		{
			Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("CardsHighlight");
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

	private void OnReleased()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckOutRoundCards);
	}

	private void OnItemReleased()
	{
		Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Focus();
	}

	private void ToHexMovementState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.HexMovement);
	}

	public void ToggleTooltip()
	{
		TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
	}
}
