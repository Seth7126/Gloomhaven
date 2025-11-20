using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class TownRecordsState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.TownRecords;

	protected override bool SelectedFirst => true;

	protected override string RootName => "TownRecords";

	public TownRecordsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		SetActiveReturnToMap(value: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		SetActiveReturnToMap(value: false);
	}
}
