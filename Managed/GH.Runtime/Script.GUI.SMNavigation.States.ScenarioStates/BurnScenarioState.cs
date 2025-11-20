using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class BurnScenarioState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.Burn;

	public BurnScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: true);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action> { { "Tips", null } };
		if (CardsHandManager.Instance.IsFullDeckPreviewAllowed)
		{
			dictionary.Add("AllCards", null);
		}
		InitiativeTrack.Instance.ToggleSortingOrder(value: false);
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("BurnScenarioState", dictionary);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		Hotkeys.Instance.RemoveHotkeysForObject("BurnScenarioState");
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
	}
}
