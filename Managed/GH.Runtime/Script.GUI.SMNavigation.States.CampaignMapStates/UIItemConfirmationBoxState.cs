using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class UIItemConfirmationBoxState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.ItemConfirmationBox;

	protected override bool SelectedFirst => false;

	protected override string RootName => null;

	public UIItemConfirmationBoxState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_navigationManager.DeselectAll();
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: false);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: true);
	}
}
