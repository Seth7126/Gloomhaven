using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectElementItemState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.SelectElementItem;

	public SelectElementItemState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectElementItemState", new Dictionary<string, Action> { { "Back", null } });
		UseActionScenarioState.EnableMouse();
		Choreographer.s_Choreographer.SetDisableVisualState(value: true);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.Hide(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.RemoveHotkeysForObject("SelectElementItemState");
		UseActionScenarioState.DisableMouse();
		Choreographer.s_Choreographer.SetDisableVisualState(value: false);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
	}
}
