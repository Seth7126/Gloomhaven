using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public sealed class YesNoCharacterActionDialogState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.YesNoCharacterActionDialog;

	protected override bool SelectedFirst => false;

	protected override string RootName => null;

	public YesNoCharacterActionDialogState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_navigationManager.DeselectAll();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
