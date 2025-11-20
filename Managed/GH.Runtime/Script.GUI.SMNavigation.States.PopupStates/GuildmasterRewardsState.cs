using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class GuildmasterRewardsState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.GuildmasterRewards;

	protected override string RootName => "GuildmasterRewards";

	public GuildmasterRewardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, KeyActionConstants.GamepadTriggersActions);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, KeyActionConstants.GamepadTriggersActions);
	}
}
