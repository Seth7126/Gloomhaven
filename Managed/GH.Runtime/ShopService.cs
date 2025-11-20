using System;
using System.Collections.Generic;
using System.Linq;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

public class ShopService : IShopItemService
{
	private CMapParty party;

	private Action<CItem> onUpdateNewItemFlag;

	public bool IsMerchantIntroShown => party.HasIntroduced(EIntroductionConcept.Merchant.ToString());

	public ShopService(CMapParty party, Action<CItem> onUpdateNewItemFlag)
	{
		this.party = party;
		this.onUpdateNewItemFlag = onUpdateNewItemFlag;
	}

	public bool IsAffordable(CItem item, CMapCharacter character)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			return DiscountedCost(item) <= party.PartyGold;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (character == null)
			{
				return false;
			}
			return DiscountedCost(item) <= character.CharacterGold;
		}
		return false;
	}

	public int DiscountedCost(CItem item)
	{
		return item.YMLData.Cost + AdventureState.MapState.MapParty.ShopDiscount;
	}

	public void Buy(CItem item, CMapCharacter mapCharacter = null, bool equip = false)
	{
		if (FFSNetwork.IsClient)
		{
			return;
		}
		item.IsNew = true;
		party.AddItem(item);
		AdventureState.MapState.HeadquartersState.RemoveItemFromMerchantStock(item);
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			party.ModifyPartyGold(-DiscountedCost(item), useGoldModifier: true);
		}
		if (mapCharacter != null)
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				mapCharacter.ModifyGold(-DiscountedCost(item), useGoldModifier: true);
			}
			party.BindItem(mapCharacter.CharacterID, mapCharacter.CharacterName, item, equip);
		}
		SaveData.Instance.SaveCurrentAdventureData();
		MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckLockedContent), processImmediately: false);
	}

	public void Sell(CItem item, CMapCharacter character)
	{
		if (FFSNetwork.IsClient)
		{
			return;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			party.ModifyPartyGold(item.SellPrice, useGoldModifier: true);
		}
		else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (character == null)
			{
				throw new Exception("Null character sent to Sell function.  Item: " + item.Name);
			}
			character.ModifyGold(item.SellPrice, useGoldModifier: true);
		}
		party.RemoveItem(item, character);
		AdventureState.MapState.HeadquartersState.AddItemToMerchantStock(item);
		if (character != null)
		{
			character.MultiplayerSoldItems.Add(item);
		}
		else
		{
			AdventureState.MapState.MapParty.MultiplayerSoldItems.Add(item);
		}
		SaveData.Instance.SaveCurrentAdventureData();
		MapRuleLibraryClient.Instance.AddQueueMessage(new CMapDLLMessage(EMapDLLMessageType.CheckLockedContent), processImmediately: false);
	}

	public List<CItem> GetItemsToBuy(CMapCharacter character = null)
	{
		return AdventureState.MapState.HeadquartersState.CheckMerchantStock.FindAll((CItem it) => it.YMLData.Slot != CItem.EItemSlot.QuestItem && (character == null || it.CanEquipItem(character.CharacterID)));
	}

	public void RegisterOnRemovedNewItemFlag(PartyItemEventHandler callback)
	{
		NewPartyDisplayUI partyDisplay = NewPartyDisplayUI.PartyDisplay;
		partyDisplay.OnUpdatedNewItems = (PartyItemEventHandler)Delegate.Combine(partyDisplay.OnUpdatedNewItems, callback);
	}

	public void UnregisterOnRemovedNewItemFlag(PartyItemEventHandler callback)
	{
		if (NewPartyDisplayUI.PartyDisplay.OnUpdatedNewItems != null)
		{
			NewPartyDisplayUI partyDisplay = NewPartyDisplayUI.PartyDisplay;
			partyDisplay.OnUpdatedNewItems = (PartyItemEventHandler)Delegate.Remove(partyDisplay.OnUpdatedNewItems, callback);
		}
	}

	public List<CItem> GetItemsToSell(CMapCharacter character = null)
	{
		if (character == null)
		{
			return (from it in party.CheckUnboundItems.Concat(party.CheckCharacters.SelectMany((CMapCharacter it) => it.GetBoundAndEquippedItems()))
				where it.YMLData.Slot != CItem.EItemSlot.QuestItem
				select it).ToList();
		}
		return character.GetBoundAndEquippedItems().FindAll((CItem it) => it.YMLData.Slot != CItem.EItemSlot.QuestItem);
	}

	public void UnmarkNewItem(CItem item)
	{
		if (item.IsNew)
		{
			item.IsNew = false;
			onUpdateNewItemFlag?.Invoke(item);
		}
	}

	public void UnmarkNewItem(List<CItem> items)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].IsNew)
			{
				items[i].IsNew = false;
				onUpdateNewItemFlag?.Invoke(items[i]);
			}
		}
	}

	public Dictionary<CItem, Tuple<CMapCharacter, bool>> GetBoundsItems(CMapCharacter mapCharacer = null)
	{
		Dictionary<CItem, Tuple<CMapCharacter, bool>> dictionary = new Dictionary<CItem, Tuple<CMapCharacter, bool>>();
		if (mapCharacer != null)
		{
			AddBoundsItems(dictionary, mapCharacer);
			return dictionary;
		}
		foreach (CMapCharacter checkCharacter in party.CheckCharacters)
		{
			AddBoundsItems(dictionary, checkCharacter);
		}
		return dictionary;
	}

	private void AddBoundsItems(Dictionary<CItem, Tuple<CMapCharacter, bool>> boundItems, CMapCharacter character)
	{
		foreach (CItem item in character.CheckBoundItems.Where((CItem it) => it.YMLData.Slot != CItem.EItemSlot.QuestItem))
		{
			boundItems[item] = new Tuple<CMapCharacter, bool>(character, item2: false);
		}
		foreach (CItem item2 in character.CheckEquippedItems.Where((CItem it) => it.YMLData.Slot != CItem.EItemSlot.QuestItem))
		{
			boundItems[item2] = new Tuple<CMapCharacter, bool>(character, item2: true);
		}
	}

	public int GetBuyDiscount()
	{
		return party.ShopDiscount;
	}

	public void SetMerchantIntroShown()
	{
		if (party.MarkIntroDone(EIntroductionConcept.Merchant.ToString()))
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}
}
