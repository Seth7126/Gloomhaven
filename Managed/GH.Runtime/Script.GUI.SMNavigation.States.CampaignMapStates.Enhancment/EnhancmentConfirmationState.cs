using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;

public class EnhancmentConfirmationState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.EnhancmentConfirmation;

	protected override bool SelectedFirst => false;

	protected override string RootName => "EnhancmentConfirmation";

	public EnhancmentConfirmationState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: false);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: true);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
	}
}
