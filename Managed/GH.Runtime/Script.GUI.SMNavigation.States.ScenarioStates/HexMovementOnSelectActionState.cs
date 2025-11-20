using Code.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using UnityEngine;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class HexMovementOnSelectActionState : RoundStartScenarioState
{
	private Coroutine _detectSubmitClickCoroutine;

	private bool CardSelectionEnabled
	{
		get
		{
			if (GameState.CurrentActionSelectionSequence != GameState.ActionSelectionSequenceType.Complete)
			{
				return Choreographer.s_Choreographer.ThisPlayerHasTurnControl;
			}
			return false;
		}
	}

	protected override bool CanToggleCards => false;

	private static bool SelectAbilityCardsOrLongRest => PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;

	public override ScenarioStateTag StateTag => ScenarioStateTag.HexMovement;

	public HexMovementOnSelectActionState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		if (!SelectAbilityCardsOrLongRest)
		{
			InitiativeTrack.Instance.DisplayControllerTips();
		}
		if (CardSelectionEnabled)
		{
			CardsHandManager.Instance.ReturnToActionSelectHotkeys.Show();
		}
		Choreographer.s_Choreographer.readyButton.SendSubmitPlayerActionOnShort(enable: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Choreographer.s_Choreographer.readyButton.SendSubmitPlayerActionOnShort(enable: false);
		base.Exit(stateProvider, payload);
		CardsHandManager.Instance.ReturnToActionSelectHotkeys.Hide();
	}

	protected override void SubscribeInput()
	{
		InputManager.RegisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_LEFT, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_RIGHT, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.VERTICAL_SHORTCUT_UP, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.VERTICAL_SHORTCUT_DOWN, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, GoToActionSelectionScenarioState);
		InputManager.RegisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, GoToCheckOutRoundCardsState);
		InputManager.RegisterToOnPressed(KeyAction.UI_NEXT_TAB, GoToCheckOutRoundCardsState);
	}

	protected override void UnsubscribeInput()
	{
		InputManager.UnregisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_LEFT, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.HORIZONTAL_SHORTCUT_RIGHT, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.VERTICAL_SHORTCUT_UP, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.VERTICAL_SHORTCUT_DOWN, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, GoToActionSelectionScenarioState);
		InputManager.UnregisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, GoToCheckOutRoundCardsState);
		InputManager.UnregisterToOnPressed(KeyAction.UI_NEXT_TAB, GoToCheckOutRoundCardsState);
	}

	private void GoToActionSelectionScenarioState()
	{
		if (CardSelectionEnabled)
		{
			ScenarioStateTag scenarioStateTag = Singleton<UINavigation>.Instance.StateMachine.GetLatestState<ScenarioStateTag>(ScenarioStateTag.SelectAction, ScenarioStateTag.LongRest);
			if (scenarioStateTag == ScenarioStateTag.Empty)
			{
				scenarioStateTag = ScenarioStateTag.SelectAction;
			}
			_stateMachine.Enter(scenarioStateTag);
		}
	}

	private void GoToCheckOutRoundCardsState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckOutRoundCards);
	}
}
