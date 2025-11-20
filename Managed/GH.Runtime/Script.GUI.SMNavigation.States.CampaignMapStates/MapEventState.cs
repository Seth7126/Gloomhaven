using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class MapEventState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.MapEvent;

	protected override bool SelectedFirst => true;

	protected override string RootName => "MapEvent";

	public MapEventState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		DefaultNavigation();
	}
}
