using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class InventoryState : CampaignMapState
{
	private CampaignMapStateTag _previousState;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Inventory;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Inventory";

	public InventoryState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_previousState = Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PreviousCampaignState;
		InitState(_previousState, isActive: true);
		if (payload != null)
		{
			if (payload is SelectionStateData data)
			{
				NavigationWithSelectionStateData(data);
			}
		}
		else
		{
			DefaultNavigation();
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		NewPartyDisplayUI.PartyDisplay?.CloseTooltip();
		InitState(_previousState, isActive: false);
		base.Exit(stateProvider, payload);
	}

	private void InitState(CampaignMapStateTag previousStateTag, bool isActive)
	{
		switch (previousStateTag)
		{
		case CampaignMapStateTag.Merchant:
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleFocusBackground(isActive);
			break;
		default:
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive);
			break;
		case CampaignMapStateTag.Temple:
			break;
		}
	}
}
