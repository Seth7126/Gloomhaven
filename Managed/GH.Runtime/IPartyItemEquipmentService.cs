using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public interface IPartyItemEquipmentService
{
	bool HasShownEquipmentIntro { get; }

	List<CItem> GetUnboundItemsForSlot(CItem.EItemSlot slotType, CMapCharacter character);

	bool CanUnbound(CMapCharacter characterID, CItem item);

	void UnbindItem(CMapCharacter characterID, CItem item);

	void BindItem(CMapCharacter characterID, CItem item);

	Dictionary<CItem, Tuple<CMapCharacter, bool>> GetBoundAndEquippedItemsForSlot(CItem.EItemSlot slotType, CMapCharacter character = null);

	int CountNewItems(CItem.EItemSlot itemSlot, CMapCharacter character = null);

	List<CItem.EItemSlot> GetAvailableSlotItems(CMapCharacter character = null);

	int CountNewItems(CMapCharacter character = null);

	void UnmarkNewItem(CItem item);

	void RegisterOnAddedItemParty(PartyItemEventHandler callback);

	void UnregisterOnAddedItemParty(PartyItemEventHandler callback);

	void RegisterOnRemovedItemParty(PartyItemSlotEventHandler callback);

	void UnregisterOnRemovedItemParty(PartyItemSlotEventHandler callback);

	void RegisterOnRemovedNewItemFlag(PartyItemEventHandler callback);

	void UnregisterOnRemovedNewItemFlag(PartyItemEventHandler callback);

	void RegisterOnBindedItemToCharacter(CharacterItemEventHandler callback);

	void UnregisterOnBindedItemToCharacter(CharacterItemEventHandler callback);

	void RegisterOnEquippedItem(CharacterItemEventHandler callback);

	void UnregisterOnEquippedItem(CharacterItemEventHandler callback);

	void RegisterOnUnequippedItem(CharacterItemsEventHandler callback);

	void UnregisterOnUnequippedItem(CharacterItemsEventHandler callback);

	void SetEquipmentIntroShown();

	bool CanEquip(CItem item, CMapCharacter character);

	bool CanUnequip(CItem item, CMapCharacter character);
}
