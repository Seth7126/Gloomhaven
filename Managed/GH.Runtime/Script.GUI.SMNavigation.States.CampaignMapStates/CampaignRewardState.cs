using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class CampaignRewardState : CampaignMapState
{
	private bool _shieldInputLocked;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.CampaignReward;

	protected override bool SelectedFirst => true;

	protected override string RootName => "CampaignReward";

	public CampaignRewardState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		DefaultNavigation();
		if (Singleton<UIGuildmasterHUD>.Instance != null)
		{
			_shieldInputLocked = Singleton<UIGuildmasterHUD>.Instance.IsNavigationLocked;
			Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(isNavigationLocked: true);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (Singleton<UIGuildmasterHUD>.Instance != null)
		{
			Singleton<UIGuildmasterHUD>.Instance.ToggleNavigationLock(_shieldInputLocked);
		}
	}
}
