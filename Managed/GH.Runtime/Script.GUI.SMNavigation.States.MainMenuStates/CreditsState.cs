using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class CreditsState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.Credits;

	protected override bool SelectedFirst => false;

	protected override string RootName => "Credits";

	public CreditsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		_navigationManager.DeselectAll();
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("CreditsState", new Dictionary<string, Action> { { "Back", null } });
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		Hotkeys.Instance.RemoveHotkeysForObject("CreditsState");
	}
}
