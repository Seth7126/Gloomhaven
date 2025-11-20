using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class CreateNameStepState : CampaignMapState
{
	private bool _shieldInputLocked;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.CreateNameStep;

	protected override bool SelectedFirst => true;

	protected override string RootName => "CreateNameStep";

	public CreateNameStepState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_shieldInputLocked = Singleton<UIGuildmasterHUD>.Instance.IsNavigationLocked;
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(isNavigationLocked: true);
		_navigationManager.DeselectCurrentSelectable();
		DefaultNavigation();
		InputManager.RequestDisableInput(this, KeyActionConstants.GamepadTriggersActions);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(_shieldInputLocked);
		InputManager.RequestEnableInput(this, KeyActionConstants.GamepadTriggersActions);
	}
}
