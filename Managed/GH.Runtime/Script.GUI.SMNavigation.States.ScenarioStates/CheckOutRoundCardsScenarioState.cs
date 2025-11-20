using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class CheckOutRoundCardsScenarioState : ScenarioState
{
	private IStateFilter _stateFilter = new StateFilterByType(typeof(LevelMessageState)).InverseFilter();

	private bool _isLongConfirmActivated;

	private bool _previousSkipButtonState;

	private bool _previousUndoButtonState;

	public override ScenarioStateTag StateTag => ScenarioStateTag.CheckOutRoundCards;

	public CheckOutRoundCardsScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_isLongConfirmActivated = Choreographer.s_Choreographer.readyButton.IsInteractable;
		_previousSkipButtonState = Choreographer.s_Choreographer.m_SkipButton.IsInteractable;
		_previousUndoButtonState = Choreographer.s_Choreographer.m_UndoButton.IsInteractable;
		Choreographer.s_Choreographer.ToggleBottomButtons(isActive: false, _isLongConfirmActivated);
		InitiativeTrack.Instance.SelectFirst();
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("CheckOutRoundCardsScenarioState", new Dictionary<string, Action> { { "Back", null } });
		Singleton<InputManager>.Instance.PlayerControl.UICancel.OnReleased += GoToPreviousState;
		Singleton<InputManager>.Instance.PlayerControl.UISubmit.OnReleased += OnSubmit;
		Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnPressed += InitiativeTrack.Instance.GoToPrevious;
		Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnPressed += InitiativeTrack.Instance.GoToNext;
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is AllCardsScenarioState)
		{
			Choreographer.s_Choreographer.ToggleBottomButtons(isActive: true, _isLongConfirmActivated);
			Choreographer.s_Choreographer.m_SkipButton.SetInteractable(_previousSkipButtonState);
			Choreographer.s_Choreographer.m_UndoButton.SetInteractable(_previousUndoButtonState);
		}
		InitiativeTrack.Instance.Clear();
		Hotkeys.Instance.RemoveHotkeysForObject("CheckOutRoundCardsScenarioState");
		Singleton<InputManager>.Instance.PlayerControl.UICancel.OnReleased -= GoToPreviousState;
		Singleton<InputManager>.Instance.PlayerControl.UISubmit.OnReleased -= OnSubmit;
		Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnPressed -= InitiativeTrack.Instance.GoToPrevious;
		Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnPressed -= InitiativeTrack.Instance.GoToNext;
	}

	private void OnSubmit()
	{
		if (!(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot != null) || !Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot.GameObject.name.Equals("UILevelMessageBoxFixed"))
		{
			GoToPreviousState();
		}
	}

	private void GoToPreviousState()
	{
		Choreographer.s_Choreographer.ToggleBottomButtons(isActive: true, _isLongConfirmActivated);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(_previousSkipButtonState);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(_previousUndoButtonState);
		Singleton<UINavigation>.Instance.StateMachine.ToPreviousLatestState<ScenarioStateTag>(ScenarioStateTag.HexMovement, ScenarioStateTag.RoundStart, ScenarioStateTag.SelectTarget, ScenarioStateTag.UseAction, ScenarioStateTag.SelectAction);
	}
}
