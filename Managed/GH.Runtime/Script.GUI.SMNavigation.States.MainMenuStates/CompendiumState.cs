using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class CompendiumState : MainMenuState
{
	private SelectHotkeys _selectHotkeys = new SelectHotkeys();

	private readonly KeyAction[] _actionsToDisable = new KeyAction[5]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.CONTROL_COMBAT_LOG
	};

	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.Compendium;

	protected override string RootName => "Compendium";

	public CompendiumState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		CompendiumWindow instance = Singleton<CompendiumWindow>.Instance;
		instance.OnSectionSelected = (Action<bool>)Delegate.Combine(instance.OnSectionSelected, new Action<bool>(OnSectionSelected));
		_selectHotkeys.Enter(Hotkeys.Instance.GetSession());
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("CompendiumState", new Dictionary<string, Action> { { "Back", null } });
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.HowToPlay);
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, _actionsToDisable);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		CompendiumWindow instance = Singleton<CompendiumWindow>.Instance;
		instance.OnSectionSelected = (Action<bool>)Delegate.Remove(instance.OnSectionSelected, new Action<bool>(OnSectionSelected));
		_selectHotkeys.Hide();
		Hotkeys.Instance.RemoveHotkeysForObject("CompendiumState");
		Hotkeys.Instance.SetPreviousState();
		InputManager.RequestEnableInput(this, _actionsToDisable);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}

	private void OnSectionSelected(bool value)
	{
		_selectHotkeys.SetShown(value, canUnselect: false);
	}
}
