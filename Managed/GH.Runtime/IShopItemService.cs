using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public interface IShopItemService
{
	bool IsMerchantIntroShown { get; }

	void Buy(CItem item, CMapCharacter character = null, bool equip = false);

	void Sell(CItem item, CMapCharacter character = null);

	bool IsAffordable(CItem item, CMapCharacter character = null);

	int DiscountedCost(CItem item);

	List<CItem> GetItemsToSell(CMapCharacter character = null);

	List<CItem> GetItemsToBuy(CMapCharacter character = null);

	void RegisterOnRemovedNewItemFlag(PartyItemEventHandler callback);

	void UnregisterOnRemovedNewItemFlag(PartyItemEventHandler callback);

	void UnmarkNewItem(List<CItem> items);

	void UnmarkNewItem(CItem item);

	Dictionary<CItem, Tuple<CMapCharacter, bool>> GetBoundsItems(CMapCharacter character = null);

	int GetBuyDiscount();

	void SetMerchantIntroShown();
}
