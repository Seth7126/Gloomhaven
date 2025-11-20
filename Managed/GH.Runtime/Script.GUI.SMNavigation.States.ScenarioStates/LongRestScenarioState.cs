using System;
using System.Collections.Generic;
using Code.State;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class LongRestScenarioState : ScenarioState
{
	private IHotkeySession _hotkeySession;

	private CardSelectHotkeys _cardSelectHotkeys = new CardSelectHotkeys(checkSelectable: false, updateOnHover: false, checkSelectInScenario: false, checkHovered: false, hideOnFullCardFocus: false);

	public override ScenarioStateTag StateTag => ScenarioStateTag.LongRest;

	private static AbilityCardUI LongRestCard => CardsHandManager.Instance.LongRestConfirmationButton.LongRestCard;

	private static bool ImprovedShortRest => PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;

	public LongRestScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("LongRestScenarioState", new Dictionary<string, Action>
		{
			{ "Highlight", null },
			{ "AllCards", null },
			{ "Back", null }
		});
		_hotkeySession = Hotkeys.Instance.GetSession();
		_cardSelectHotkeys.Enter(_hotkeySession);
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
			_cardSelectHotkeys.SetSelectHotkeys(LongRestCard);
		}
		else
		{
			Debug.LogError("CardsHandManager.Instance.CurrentHand != null, can't get long rest card");
		}
		if (ControllerInputAreaManager.Instance.m_FocusArea != "Tutorial Message Box fIXED" && !ImprovedShortRest)
		{
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
		}
		if (!ImprovedShortRest)
		{
			InitiativeTrack.Instance.DisplayControllerTips();
			InitiativeTrack.Instance.ReorderControllerTips(isEnemyActionState: true);
		}
		InputManager.SkipNextSubmitAction();
		InputManager.RegisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.RegisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, ToHexMovementState);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		Choreographer.s_Choreographer.readyButton.SetSendDefaultSubmitEvent(enable: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Choreographer.s_Choreographer.readyButton.SetSendDefaultSubmitEvent(enable: false);
		_hotkeySession.Dispose();
		_cardSelectHotkeys.Exit();
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		Singleton<UINavigation>.Instance.NavigationManager.DeselectAll();
		Hotkeys.Instance.RemoveHotkeysForObject("LongRestScenarioState");
		if (CardsHandManager.Instance != null && CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		InputManager.UnregisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.UnregisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, ToHexMovementState);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}

	private void OnItemReleased()
	{
		Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Focus();
	}

	private void ToHexMovementState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.HexMovement);
	}
}
