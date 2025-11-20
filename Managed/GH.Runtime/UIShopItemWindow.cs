using System;
using System.Collections.Generic;
using Code.State;
using FFSNet;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIShopItemWindow : Singleton<UIShopItemWindow>
{
	[SerializeField]
	private Button exitShopButton;

	[SerializeField]
	private UIShopItemInventory itemInventory;

	[SerializeField]
	private UIIntroduce introduction;

	[SerializeField]
	private UIMapFTUEStep ftueStep;

	private Action onExit;

	private UIWindow shopWindow;

	private ShopService service;

	public BasicEventHandler OnNewMerchantItemShown
	{
		get
		{
			return ItemInventory.OnNewItemBuyShown;
		}
		set
		{
			ItemInventory.OnNewItemBuyShown = value;
		}
	}

	public PartyItemEventHandler OnUpdateNewPartyItemNotification { get; set; }

	public UIShopItemInventory ItemInventory => itemInventory;

	protected override void Awake()
	{
		base.Awake();
		shopWindow = GetComponent<UIWindow>();
		exitShopButton.onClick.AddListener(Exit);
		shopWindow.onHidden.AddListener(OnHidden);
	}

	protected override void OnDestroy()
	{
		exitShopButton.onClick.RemoveAllListeners();
		Singleton<UIShopItemWindow>.Instance.OnUpdateNewPartyItemNotification = null;
		base.OnDestroy();
	}

	public void EnterShop(CHeadquartersState location, Action onExit = null)
	{
		ClearEvents();
		this.onExit = onExit;
		service = new ShopService(AdventureState.MapState.MapParty, delegate(CItem item)
		{
			OnUpdateNewPartyItemNotification?.Invoke(item);
		});
		CMapCharacter selectedCharacter = null;
		if (AdventureState.MapState.GoldMode != EGoldMode.None)
		{
			NewPartyDisplayUI.PartyDisplay.EnableSelectionMode(delegate(NewPartyCharacterUI slot)
			{
				selectedCharacter = slot.Data;
				if (shopWindow.IsOpen)
				{
					ItemInventory.RefreshView(slot.Data);
				}
			}, disableButtons: false);
		}
		shopWindow.Show();
		ItemInventory.Init(service, selectedCharacter);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemBound(OnItemBound);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnGoldChanged(OnGoldUpdate);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemEquipped(OnItemEquipped);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemUnbound(OnItemUnbound);
		Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterItemUnequipped(OnItemUnequipped);
		service.RegisterOnRemovedNewItemFlag(ItemInventory.RefreshSellNewItemNotification);
		if (!service.IsMerchantIntroShown)
		{
			introduction.Show(FinishEnterShop);
			service.SetMerchantIntroShown();
		}
		else
		{
			FinishEnterShop();
		}
	}

	private void FinishEnterShop()
	{
		if (InputManager.GamePadInUse)
		{
			NewPartyDisplayUI.PartyDisplay.SelectCurrentCharacter();
			TryFocus();
		}
		Singleton<MapFTUEManager>.Instance?.StartStep(ftueStep);
	}

	public void TryFocus()
	{
		if (!IsTutorial())
		{
			NewPartyDisplayUI.PartyDisplay.TryToggleEquipmentPanelForSelectedCharacter(isEnabled: true);
			if (!IsTutorial())
			{
				Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus();
			}
		}
	}

	private bool IsTutorial()
	{
		IState currentState = Singleton<UINavigation>.Instance.StateMachine.CurrentState;
		if (currentState is LevelMessageState || currentState is LevelUpState)
		{
			return true;
		}
		return false;
	}

	public void SetEditMode(bool canEdit)
	{
		ItemInventory.SetInteractable(canEdit);
	}

	public void RefreshView()
	{
		if (shopWindow.IsVisible)
		{
			ItemInventory.RefreshView();
		}
	}

	public void Exit()
	{
		introduction.Hide();
		shopWindow.Hide();
	}

	private void OnGoldUpdate(int gold)
	{
		ItemInventory.UpdateListingsAffordability();
	}

	private void OnHidden()
	{
		NewPartyDisplayUI.PartyDisplay.DisableSelectionMode();
		ClearEvents();
		onExit?.Invoke();
	}

	private void ClearEvents()
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnGoldChanged(OnGoldUpdate);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemEquipped(OnItemEquipped);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemUnbound(OnItemUnbound);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemUnequipped(OnItemUnequipped);
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterItemBound(OnItemBound);
		}
		if (service != null)
		{
			service.UnregisterOnRemovedNewItemFlag(ItemInventory.RefreshSellNewItemNotification);
		}
	}

	private void OnDisable()
	{
		ClearEvents();
	}

	private void OnItemUnequipped(List<CItem> items, CMapCharacter character)
	{
		for (int i = 0; i < items.Count; i++)
		{
			ItemInventory.UpdateItemBound(items[i], character);
		}
	}

	private void OnItemUnbound(CItem item, CMapCharacter mapCharacter)
	{
		ItemInventory.UpdateItemUnbound(item);
	}

	private void OnItemEquipped(CItem item, CMapCharacter mapCharacter)
	{
		ItemInventory.UpdateItemEquipped(item);
	}

	private void OnItemBound(CItem newitem, CMapCharacter character)
	{
		ItemInventory.UpdateItemBound(newitem, character);
	}

	public void MPBuyItem(GameAction action, ref bool actionValid)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			actionValid = false;
		}
		else
		{
			ItemInventory.MPBuyItem(action, ref actionValid, shopWindow.IsVisible);
		}
	}

	public void MPSellItem(GameAction action, ref bool actionValid)
	{
		if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(ItemInventory);
			actionValid = false;
		}
		else
		{
			ItemInventory.MPSellItem(action, ref actionValid, shopWindow.IsVisible);
		}
	}
}
