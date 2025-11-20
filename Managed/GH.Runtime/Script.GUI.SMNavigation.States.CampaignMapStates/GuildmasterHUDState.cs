using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class GuildmasterHUDState : CampaignMapState
{
	private bool _oldHotkeysMercenariesState;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.GuildmasterHUD;

	protected override bool SelectedFirst => false;

	protected override string RootName => "GuildmasterHUD";

	public GuildmasterHUDState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false);
		_oldHotkeysMercenariesState = NewPartyDisplayUI.PartyDisplay.MercenariesHotkeysActiveState;
		CampaignMapStateTag latestState = stateProvider.GetLatestState<CampaignMapStateTag>(CampaignMapStateTag.Merchant, CampaignMapStateTag.Temple, CampaignMapStateTag.WorldMap);
		if (latestState == CampaignMapStateTag.Merchant || latestState == CampaignMapStateTag.Temple)
		{
			NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(_oldHotkeysMercenariesState);
	}
}
