using Code.State;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using SM.Gamepad;
using ScenarioRuleLibrary;

namespace Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;

public class EnhancmentSelectOptionUpgradeState : CampaignMapState
{
	public class Data
	{
		public bool SelectedFirst;

		public UiNavigationRoot PreviousRoot;
	}

	private readonly StateMachine _stateMachine;

	private Data _data = new Data();

	private IHotkeySession _hotkeySession;

	private SessionHotkey _sellHotkey;

	private SessionHotkey _buyHotkey;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.EnhancmentSelectOptionUpgrade;

	protected override bool SelectedFirst => true;

	protected override string RootName => "EnhancmentSelectOptionUpgrade";

	private UINewEnhancementWindow EnhancementWindow => Singleton<UIGuildmasterHUD>.Instance.EnhancementWindow;

	private static UINewEnhancementShopInventory EnhancementShop => Singleton<UINewEnhancementShopInventory>.Instance;

	public EnhancmentSelectOptionUpgradeState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		_stateMachine = stateMachine;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		if (!(payload is Data data))
		{
			if (payload == null)
			{
				if (_data == null)
				{
					SelectRootDefault();
				}
				else
				{
					SwitchStateWithData(_data);
				}
			}
			else
			{
				SelectRootDefault();
			}
		}
		else
		{
			SwitchStateWithData(data);
		}
		EnhancementShop.InputArea.Focus();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, SwitchToCardState);
		_hotkeySession = EnhancementShop.SelectEnhancementHotkeyContainer.GetSession().SetHotkeyAdded("Back", added: true);
		_buyHotkey = _hotkeySession.GetHotkey("Buy");
		_sellHotkey = _hotkeySession.GetHotkey("Sell");
		EnhancementShop.OnHoveredSlot.AddListener(OnSlotSelect);
		OnSlotSelect(EnhancementShop.CurrentSlot);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		EnhancementWindow.SetFullyEnhanced(fullyEnhanced: false);
		EnhancementShop.OnHoveredSlot.RemoveListener(OnSlotSelect);
		_hotkeySession.Dispose();
		_hotkeySession = null;
		Singleton<UINewEnhancementShopInventory>.Instance.InputArea.Unfocus();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, SwitchToCardState);
	}

	private void SelectRootDefault()
	{
		_navigationManager.SetCurrentRoot(RootName, SelectedFirst);
	}

	private void OnSlotSelect(EnhancementSlot slot)
	{
		_buyHotkey.SetShown(slot.AvailableToBuy);
		if (ScenarioRuleClient.SRLYML.Enhancements.AllowSell(AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent))
		{
			_sellHotkey.SetShown(slot.AvailableToSell);
		}
		EnhancementWindow.SetFullyEnhanced(!slot.Available);
	}

	private void SwitchStateWithData(Data data)
	{
		_data = data;
		_navigationManager.SetCurrentRoot(RootName, data.SelectedFirst);
	}

	private void SwitchToCardState()
	{
		CoroutineHelper.RunNextFrame(delegate
		{
			_stateMachine.Enter(CampaignMapStateTag.EnhancmentSelectCardOption, _data.PreviousRoot);
		});
	}
}
