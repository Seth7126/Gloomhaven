using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class UiMultiplayerConfirmationBoxState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.MultiplayerConfirmationBox;

	protected override bool SelectedFirst => false;

	protected override string RootName => null;

	public UiMultiplayerConfirmationBoxState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
