using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class MercenariesState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Mercenaries;

	protected override bool SelectedFirst => false;

	protected override string RootName => "Mercenaries";

	public MercenariesState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
