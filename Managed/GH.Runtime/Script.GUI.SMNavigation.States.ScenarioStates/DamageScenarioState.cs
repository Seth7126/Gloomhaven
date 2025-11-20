using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class DamageScenarioState : ScenarioState
{
	private CameraController.ECameraInput _cameraInputDisabledState;

	public override ScenarioStateTag StateTag => ScenarioStateTag.Damage;

	public DamageScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_cameraInputDisabledState = CameraController.s_CameraController.CameraInputDisabled;
		CameraController.s_CameraController.DisableCameraInput(disableInput: false);
		UIManager.Instance.RunBaseButtonsBlocker();
		InitiativeTrack.Instance.DisplayControllerTips(doShow: false);
		Singleton<EnemyCurrentTurnStatPanel>.Instance.Hide(this);
		Singleton<TakeDamagePanel>.Instance.DisplayButtons();
		Singleton<TakeDamagePanel>.Instance.ClearPreviouslyPreviewed();
		Singleton<TakeDamagePanel>.Instance.InputArea.Focus();
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("DamageScenarioState", new Dictionary<string, Action>
		{
			{ "Highlight", null },
			{ "AllCards", null }
		});
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CameraController.s_CameraController.CameraInputDisabled = _cameraInputDisabledState;
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		CardsHandManager.Instance.EnableAllDeckSelection(isEnabled: false);
		InitiativeTrack.Instance.DisplayControllerTips();
		Singleton<EnemyCurrentTurnStatPanel>.Instance.CancelHide(this);
		Singleton<TakeDamagePanel>.Instance.DisplayButtons(visibility: false);
		Singleton<TakeDamagePanel>.Instance.InputArea.Unfocus();
		Hotkeys.Instance.RemoveHotkeysForObject("DamageScenarioState");
	}
}
