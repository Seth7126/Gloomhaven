using System.Collections.Generic;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class EquipmentState : CampaignMapState
{
	private CampaignMapStateTag _previousState;

	private IHotkeySession _templeHotkeySession;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Equipment;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Equipment";

	protected CampaignMapStateTag[] TagsFilter { get; }

	public EquipmentState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		TagsFilter = new CampaignMapStateTag[3]
		{
			CampaignMapStateTag.Merchant,
			CampaignMapStateTag.Temple,
			CampaignMapStateTag.WorldMap
		};
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		NewPartyDisplayUI.PartyDisplay.TabInput.Register();
		_previousState = GetPreviousStateTag(stateProvider, TagsFilter);
		InitHotkeys(_previousState);
		InitState(_previousState, isActive: true);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PreviousCampaignState = _previousState;
		if (Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.Loadout) && Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.TravelQuestState) && Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.Temple))
		{
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		}
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.DisableDarkenPanel();
		if (_previousState == CampaignMapStateTag.Temple)
		{
			_templeHotkeySession = Singleton<UIGuildmasterHUD>.Instance.TempleWindow.HotkeyContainer?.GetSessionOrEmpty();
			_templeHotkeySession?.AddOrReplaceHotkey("Switch_Right", null);
		}
		if (payload != null)
		{
			if (payload is SelectionStateData data)
			{
				NavigationWithSelectionStateData(data);
			}
		}
		else
		{
			FirstOrCurrentNavigation();
		}
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus, null, null, isPersistent: false, KeyActionHandler.RegisterType.Click));
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_templeHotkeySession?.Dispose();
		_templeHotkeySession = null;
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_RIGHT, Singleton<UIGuildmasterHUD>.Instance.TempleInputArea.Focus);
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetActiveHotkey("Switch_Right", value: false);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: false);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.EnableDarkenPanel();
		NewPartyDisplayUI.PartyDisplay?.CloseTooltip();
		InitState(_previousState, isActive: false);
		base.Exit(stateProvider, payload);
	}

	private void InitState(CampaignMapStateTag previousStateTag, bool isActive)
	{
		switch (previousStateTag)
		{
		case CampaignMapStateTag.Temple:
			Singleton<UIGuildmasterHUD>.Instance.TempleWindow.Shop.ToggleFocusBackground(isActive);
			break;
		case CampaignMapStateTag.Merchant:
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleFocusBackground(isActive);
			break;
		default:
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive);
			break;
		}
	}

	private void InitHotkeys(CampaignMapStateTag previousStateTag)
	{
		List<HotkeyAction> hotKeys = GetHotKeys(previousStateTag);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetHotkeysAction(hotKeys);
		InitActiveHotkeys(previousStateTag);
	}

	private CampaignMapStateTag GetPreviousStateTag(IStateProvider stateProvider, CampaignMapStateTag[] tagsFilter)
	{
		return stateProvider.GetLatestState(tagsFilter);
	}

	private List<HotkeyAction> GetHotKeys(CampaignMapStateTag previousStateTag)
	{
		List<HotkeyAction> list = new List<HotkeyAction>();
		switch (previousStateTag)
		{
		case CampaignMapStateTag.Temple:
			NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
			InputManager.RegisterToOnReleased(KeyAction.UI_R_RIGHT, Singleton<UIGuildmasterHUD>.Instance.TempleInputArea.Focus);
			break;
		case CampaignMapStateTag.Merchant:
			NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
			list.Add(new HotkeyAction("Switch_Right", Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus));
			break;
		default:
			return null;
		}
		return list;
	}

	private void InitActiveHotkeys(CampaignMapStateTag previousStateTag)
	{
		bool value = previousStateTag == CampaignMapStateTag.Merchant;
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetActiveHotkey("Switch_Right", value);
	}
}
