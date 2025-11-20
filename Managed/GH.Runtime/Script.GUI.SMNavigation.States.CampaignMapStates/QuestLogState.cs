using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class QuestLogState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.QuestLog;

	protected override bool SelectedFirst => true;

	protected override string RootName => "QuestLog";

	public QuestLogState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (stateProvider.GetLatestState<CampaignMapStateTag>(CampaignMapStateTag.WorldMap, CampaignMapStateTag.TravelQuestState) == CampaignMapStateTag.WorldMap)
		{
			DefaultNavigation();
		}
		else
		{
			FirstOrCurrentNavigation();
		}
	}
}
