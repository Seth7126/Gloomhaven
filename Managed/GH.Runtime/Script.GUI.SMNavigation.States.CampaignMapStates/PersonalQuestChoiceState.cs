using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class PersonalQuestChoiceState : CampaignMapState
{
	private bool _shieldInputLocked;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.PersonalQuestChoice;

	protected override bool SelectedFirst => true;

	protected override string RootName => "PersonalQuestChoice";

	public PersonalQuestChoiceState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_shieldInputLocked = Singleton<UIGuildmasterHUD>.Instance.IsNavigationLocked;
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(isNavigationLocked: true);
		DefaultNavigation();
		InputManager.RequestDisableInput(this, KeyActionConstants.GamepadTriggersActions);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(_shieldInputLocked);
		InputManager.RequestEnableInput(this, KeyActionConstants.GamepadTriggersActions);
	}
}
