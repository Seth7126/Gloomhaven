#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Text;
using FFSNet;
using Photon.Bolt;
using ScenarioRuleLibrary;

public class UseItemService
{
	private CActor inventoryOwner;

	public UseItemService(CActor inventoryOwner)
	{
		this.inventoryOwner = inventoryOwner;
	}

	public void UseItem(CItem Item, bool networkActionIfOnline = true, List<ElementInfusionBoardManager.EElement> infusions = null, bool saveState = true)
	{
		if (FFSNetwork.IsOnline && saveState)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
		}
		if (Item == null)
		{
			Debug.Log("Invalid item");
			return;
		}
		if (Item.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect)
		{
			Debug.Log("Passive items can't be selected.");
			return;
		}
		if (Item.SlotState != CItem.EItemSlotState.Useable && Item.SlotState != CItem.EItemSlotState.Selected)
		{
			Debug.Log("Item is not in a usable state.");
			return;
		}
		FFSNet.Console.Log("Item.NetworkID: " + Item.NetworkID);
		FFSNet.Console.Log("Item.ItemGuid: " + Item.ItemGuid);
		FFSNet.Console.Log("Item.SlotType: " + Item.YMLData.Slot);
		if (FFSNetwork.IsOnline && inventoryOwner.IsUnderMyControl && networkActionIfOnline)
		{
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			int iD = inventoryOwner.ID;
			IProtocolToken supplementaryDataToken = new ItemToken(Item.NetworkID, (int)Item.YMLData.Slot, int.MaxValue, Item.ChosenElement, infusions);
			byte[] bytes = Encoding.ASCII.GetBytes("Item: " + Item.Name);
			Synchronizer.SendGameAction(GameActionType.UseItem, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken, null, null, null, bytes, binaryDataIncludesLoggingDetails: true);
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
		ScenarioRuleClient.ToggleItem(Item, inventoryOwner);
	}
}
