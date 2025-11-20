using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class GuildmasterTrainerState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.GuildmasterTrainer;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Trainer";

	public GuildmasterTrainerState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		DefaultNavigation();
		SetActiveReturnToMap(value: true);
		Singleton<UIGuildmasterHUD>.Instance.UIAchievementInventory.EnableNavigation();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		SetActiveReturnToMap(value: false);
	}
}
