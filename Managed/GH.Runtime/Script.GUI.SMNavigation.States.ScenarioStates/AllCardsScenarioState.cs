using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class AllCardsScenarioState : ScenarioState
{
	private IStateFilter _previousStateFilter = new StateFilterByType(typeof(SelectInputDeviceBoxState)).InverseFilter();

	public override ScenarioStateTag StateTag => ScenarioStateTag.AllCards;

	public AllCardsScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("AllCardsScenarioState", new Dictionary<string, Action>
		{
			{ "Back", null },
			{
				"Tips",
				delegate
				{
					TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
				}
			}
		});
		if (CardsHandManager.Instance.CurrentHand != null)
		{
			CardsHandManager.Instance.OnControllerAreaUnfocused();
		}
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot("FullCardsView");
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelPressed));
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: true);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: true);
		if (!FFSNetwork.IsOnline)
		{
			InputManager.RequestDisableInput(this, KeyAction.CONFIRM_ACTION_BUTTON);
			Singleton<UIReadyToggle>.Instance.CanBeToggled = false;
		}
		else
		{
			Singleton<UIReadyToggle>.Instance.CanBeToggled = true;
		}
		InputManager.RequestDisableInput(this, EKeyActionTag.Scenario);
		Singleton<FullCardHandViewer>.Instance.ToggleControllerInputScroll(isEnabled: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.RemoveHotkeysForObject("AllCardsScenarioState");
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelPressed);
		Singleton<ActorStatPanel>.Instance.HideTemporary(hide: false);
		Singleton<UITextInfoPanel>.Instance.HideTemporary(hide: false);
		TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
		if (!FFSNetwork.IsOnline)
		{
			InputManager.RequestEnableInput(this, KeyAction.CONFIRM_ACTION_BUTTON);
			Singleton<UIReadyToggle>.Instance.CanBeToggled = true;
		}
		InputManager.RequestEnableInput(this, EKeyActionTag.Scenario);
		Singleton<FullCardHandViewer>.Instance.ToggleControllerInputScroll(isEnabled: false);
	}

	private void OnCancelPressed()
	{
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_previousStateFilter);
	}
}
