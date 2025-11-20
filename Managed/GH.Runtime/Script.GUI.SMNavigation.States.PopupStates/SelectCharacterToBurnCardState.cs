using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class SelectCharacterToBurnCardState : PopupState
{
	private Dictionary<string, Action> _selectHotkeys;

	private Dictionary<string, Action> _unselectHotkeys;

	public override PopupStateTag StateTag => PopupStateTag.SelectCharacterToBurnCard;

	protected override string RootName => "SelectCharacterToBurnCard";

	public SelectCharacterToBurnCardState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		_selectHotkeys = new Dictionary<string, Action> { { "Select", null } };
		_unselectHotkeys = new Dictionary<string, Action> { { "Unselect", null } };
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectCharacterToBurnCardState", _selectHotkeys);
		Singleton<UIScenarioDistributePointsManager>.Instance.DistributePointsChanged += OnHoverChanged;
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		Hotkeys.Instance.RemoveHotkeysForObject("SelectCharacterToBurnCardState");
		Singleton<UIScenarioDistributePointsManager>.Instance.DistributePointsChanged -= OnHoverChanged;
	}

	private void OnHoverChanged(bool hover)
	{
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("SelectCharacterToBurnCardState", hover ? _unselectHotkeys : _selectHotkeys);
	}
}
