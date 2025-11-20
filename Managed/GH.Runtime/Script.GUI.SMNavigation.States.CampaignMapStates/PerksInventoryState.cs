using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class PerksInventoryState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.PerksInventory;

	protected override bool SelectedFirst => true;

	protected override string RootName => "PerksInventory";

	public PerksInventoryState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
	}
}
