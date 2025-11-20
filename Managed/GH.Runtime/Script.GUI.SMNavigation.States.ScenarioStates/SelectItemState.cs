using System;
using System.Collections.Generic;
using Code.State;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectItemState : ScenarioState
{
	private UiNavigationBlocker _navBlocker = new UiNavigationBlocker("SelectItemStateBlocker");

	private TakeDamagePanel.ActionButtonsState _savedState;

	private bool _isItemBackActionHandling;

	public override ScenarioStateTag StateTag => ScenarioStateTag.SelectItem;

	public SelectItemState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		UIManager.Instance.ToggleBurnCardBlock(isBlocked: true);
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectItemState", new Dictionary<string, Action> { { "Back", null } });
		UseActionScenarioState.EnableMouse();
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.Hide(this);
		if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.Complete && _stateMachine.PreviousState is ItemActionsScenarioState)
		{
			Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDTURN, string.Format(LocalizationManager.GetTranslation(GameState.TurnActor.IsTakingExtraTurn ? "GUI_END_EXTRA_TURN" : "GUI_END_TURN"), LocalizationManager.GetTranslation(GameState.TurnActor.ActorLocKey())), hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: true, hideSelectionOnEndTurn: false);
			Choreographer.s_Choreographer.readyButton.SetDisableVisualState(value: true);
			_isItemBackActionHandling = true;
		}
		else
		{
			_isItemBackActionHandling = false;
			Singleton<TakeDamagePanel>.Instance.DisplayButtons();
			_savedState = Singleton<TakeDamagePanel>.Instance.SetDisableVisualState();
			Choreographer.s_Choreographer.SetDisableVisualState(value: true);
		}
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navBlocker);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		UIManager.Instance.ToggleBurnCardBlock(isBlocked: false);
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navBlocker);
		if (!_isItemBackActionHandling)
		{
			Singleton<TakeDamagePanel>.Instance.SetActionButtonsState(_savedState);
			Choreographer.s_Choreographer.SetDisableVisualState(value: false);
			Singleton<TakeDamagePanel>.Instance.DisplayButtons(visibility: false);
		}
		UseActionScenarioState.DisableMouse();
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
		Hotkeys.Instance.RemoveHotkeysForObject("SelectItemState");
	}
}
