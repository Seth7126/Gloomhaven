using System;
using System.Collections;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectTargetState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.SelectTarget;

	public SelectTargetState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (IsAoeAbility())
		{
			Choreographer.s_Choreographer.OnAoeTileSelected += UpdateHotkeysActivityBasedOnTileSelection;
			Choreographer.s_Choreographer.m_UndoButton.OnClearTargets += OnCancelTargets;
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectTargetState", new Dictionary<string, Action>
			{
				{ "Highlight", null },
				{ "RotateTarget", null }
			});
		}
		else
		{
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectTargetState", new Dictionary<string, Action> { { "Highlight", null } });
		}
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
		UseActionScenarioState.EnableMouse();
		InputManager.RegisterToOnReleased(KeyAction.UI_PREVIOUS_TAB, OnReleased);
		InputManager.RegisterToOnReleased(KeyAction.UI_NEXT_TAB, OnReleased);
		InputManager.RegisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.RegisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
		UseActionScenarioState.DisableMouse();
		Hotkeys.Instance.RemoveHotkeysForObject("SelectTargetState");
		Choreographer.s_Choreographer.OnAoeTileSelected -= UpdateHotkeysActivityBasedOnTileSelection;
		Choreographer.s_Choreographer.m_UndoButton.OnClearTargets -= OnCancelTargets;
		InputManager.UnregisterToOnReleased(KeyAction.UI_PREVIOUS_TAB, OnReleased);
		InputManager.UnregisterToOnReleased(KeyAction.UI_NEXT_TAB, OnReleased);
		InputManager.UnregisterToOnReleased(KeyAction.PREVIOUS_ITEM, OnItemReleased);
		InputManager.UnregisterToOnReleased(KeyAction.NEXT_ITEM, OnItemReleased);
	}

	private void OnReleased()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.CheckOutRoundCards);
	}

	private void OnItemReleased()
	{
		Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Focus();
	}

	private bool IsAoeAbility()
	{
		WorldspaceStarHexDisplay.EAbilityDisplayType currentAbilityDisplayType = WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType;
		if (currentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect || currentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.EnemyAreaOfEffect || currentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.SelectObjectPositionAreaOfEffect)
		{
			return WorldspaceStarHexDisplay.Instance.AbilityRange > 1;
		}
		return false;
	}

	private void OnCancelTargets()
	{
		UpdateHotkeysActivityBasedOnTileSelection(isTileSelected: false);
	}

	private void UpdateHotkeysActivityBasedOnTileSelection(bool isTileSelected)
	{
		CoroutineHelper.RunCoroutine(UpdateHotkeysActivityBasedOnTileSelectionWithAFrameSkip(isTileSelected));
	}

	private IEnumerator UpdateHotkeysActivityBasedOnTileSelectionWithAFrameSkip(bool isTileSelected)
	{
		yield return null;
		if (IsAoeAbility() && (!isTileSelected || !WorldspaceStarHexDisplay.Instance.MaxTargetsSelected))
		{
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectTargetState", new Dictionary<string, Action>
			{
				{ "Highlight", null },
				{ "RotateTarget", null }
			});
		}
		else
		{
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectTargetState", new Dictionary<string, Action> { { "Highlight", null } });
		}
	}
}
