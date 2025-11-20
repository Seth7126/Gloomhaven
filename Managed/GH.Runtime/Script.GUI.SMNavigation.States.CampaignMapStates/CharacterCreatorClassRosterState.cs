using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class CharacterCreatorClassRosterState : CampaignMapState
{
	private bool _shieldInputLocked;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.CharacterCreatorClassRoster;

	protected override bool SelectedFirst => false;

	protected override string RootName => "CharacterCreatorClassRoster";

	public CharacterCreatorClassRosterState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_shieldInputLocked = Singleton<UIGuildmasterHUD>.Instance.IsNavigationLocked;
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(isNavigationLocked: true);
		if (Singleton<MapFTUEManager>.Instance.HasToShowFTUE)
		{
			FirstOrCurrentNavigation();
		}
		else
		{
			FirstNavigation();
		}
		InputManager.RequestDisableInput(this, KeyActionConstants.GamepadTriggersActions);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(_shieldInputLocked);
		base.Exit(stateProvider, payload);
		_navigationManager.DeselectAll();
		InputManager.RequestEnableInput(this, KeyActionConstants.GamepadTriggersActions);
	}
}
