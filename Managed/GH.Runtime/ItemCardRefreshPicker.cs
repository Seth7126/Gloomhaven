using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using Photon.Bolt;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;

public class ItemCardRefreshPicker : Singleton<ItemCardRefreshPicker>
{
	[SerializeField]
	private ItemCardPicker picker;

	private CActor actor;

	private bool refreshingItems;

	public bool CanSelect => true;

	public void Show(CActor actor, bool refreshingItems, List<CItem.EItemSlotState> slotStatesToChooseFrom, List<CItem.EItemSlot> slotsToChooseFrom, int itemsToSelect = 1)
	{
		this.actor = actor;
		this.refreshingItems = refreshingItems;
		List<CItem> list = actor.Inventory.AllItems.FindAll((CItem it) => it.YMLData.PermanentlyConsumed != true && (slotStatesToChooseFrom == null || slotStatesToChooseFrom.Count == 0 || slotStatesToChooseFrom.Contains(it.SlotState)) && (slotsToChooseFrom == null || slotsToChooseFrom.Count == 0 || slotsToChooseFrom.Contains(it.YMLData.Slot)));
		if (itemsToSelect == 0 || list.Count == 0)
		{
			Finish();
			return;
		}
		var (hintTitle, hintMessage) = GetHintMessage(refreshingItems, slotsToChooseFrom);
		picker.Show(PopupStateTag.PickItemToRefreshOrConsume, list, ConfirmSelectedCards, CanSelect, itemsToSelect, hintTitle, hintMessage);
	}

	private (string, string) GetHintMessage(bool refreshingItems, List<CItem.EItemSlot> slotsToChooseFrom)
	{
		if (refreshingItems)
		{
			return (LocalizationManager.GetTranslation("GUI_REFRESH_ITEMS_TITLE"), LocalizationManager.GetTranslation("GUI_REFRESH_ITEMS_TIP"));
		}
		string arg = (slotsToChooseFrom.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation($"GUI_ITEM_SLOT_{slotsToChooseFrom[0]}"));
		return (string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ITEMS_TITLE"), arg), string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ITEMS_TIP"), arg));
	}

	private void ConfirmSelectedCards()
	{
		foreach (CItem currentSelectedItem in picker.GetCurrentSelectedItems())
		{
			if (refreshingItems)
			{
				actor.Inventory.ReactivateItem(currentSelectedItem, actor);
			}
			else
			{
				actor.Inventory.UseItem(currentSelectedItem);
			}
		}
		if (FFSNetwork.IsOnline)
		{
			if (refreshingItems)
			{
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ItemsToken(picker.GetCurrentSelectedItems());
				Synchronizer.SendGameAction(GameActionType.RefreshItem, ActionPhaseType.ItemRefreshing, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			else
			{
				int iD2 = actor.ID;
				IProtocolToken supplementaryDataToken = new ItemsToken(picker.GetCurrentSelectedItems());
				Synchronizer.SendGameAction(GameActionType.ConsumeItem, ActionPhaseType.ItemConsuming, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD2, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}
		picker.Hide();
		Finish();
	}

	private void Finish()
	{
		Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
		ScenarioRuleClient.StepComplete();
	}

	public void ProxyRefreshItems(GameAction action)
	{
		CActor cActor = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		if (cActor != null)
		{
			FFSNet.Console.LogInfo("Refreshing item cards for a proxy playerActor.");
			uint[] itemNetworkIDs = ((ItemsToken)action.SupplementaryDataToken).ItemNetworkIDs;
			foreach (uint itemNetworkID in itemNetworkIDs)
			{
				CItem cItem = cActor.Inventory.AllItems.SingleOrDefault((CItem x) => x.NetworkID == itemNetworkID);
				if (cItem != null)
				{
					cActor.Inventory.ReactivateItem(cItem, cActor);
				}
			}
			if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForItemRefresh)
			{
				Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.StepComplete();
			}
			return;
		}
		throw new Exception("Error refreshing items for proxy playerActor. Target actor not found (ActorID: " + cActor.ID + ").");
	}

	public void ProxyConsumeItems(GameAction action)
	{
		CActor cActor = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		if (cActor != null)
		{
			FFSNet.Console.LogInfo("Consuming item cards for a proxy playerActor.");
			uint[] itemNetworkIDs = ((ItemsToken)action.SupplementaryDataToken).ItemNetworkIDs;
			foreach (uint itemNetworkID in itemNetworkIDs)
			{
				CItem cItem = cActor.Inventory.AllItems.SingleOrDefault((CItem x) => x.NetworkID == itemNetworkID);
				if (cItem != null)
				{
					cActor.Inventory.UseItem(cItem);
				}
			}
			if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForItemRefresh)
			{
				Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.StepComplete();
			}
			return;
		}
		throw new Exception("Error consuming items for proxy playerActor. Target actor not found (ActorID: " + cActor.ID + ").");
	}
}
