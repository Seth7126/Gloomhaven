using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class UseActionScenarioState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.UseAction;

	public UseActionScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (Choreographer.s_Choreographer.IsPlayerTurn)
		{
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("UseActionScenarioState", new Dictionary<string, Action> { { "Highlight", null } });
			CardsHandManager.Instance.EnableAllCardsCombo(value: true);
			CameraController.s_CameraController.DisableCameraInput(disableInput: false);
			EnableMouse();
			Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnReleased += OnReleased;
			Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnReleased += OnReleased;
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		DisableMouse();
		Hotkeys.Instance.RemoveHotkeysForObject("UseActionScenarioState");
		Singleton<InputManager>.Instance.PlayerControl.UIPreviousTab.OnReleased -= OnReleased;
		Singleton<InputManager>.Instance.PlayerControl.UINextTab.OnReleased -= OnReleased;
	}

	private void OnReleased()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckOutRoundCards);
	}

	public static void EnableMouse()
	{
		InputManager.UpdateMouseInputEnabled(value: true);
	}

	public static void DisableMouse()
	{
		InputManager.UpdateMouseInputEnabled(value: false);
	}
}
