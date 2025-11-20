using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class StartingAbilityCardsState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.StartingAbilityCards;

	protected override bool SelectedFirst => true;

	protected override string RootName => "StartingAbilityCards";

	public StartingAbilityCardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		DefaultNavigation();
	}
}
