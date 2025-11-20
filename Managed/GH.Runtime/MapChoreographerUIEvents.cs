using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public class MapChoreographerUIEvents
{
	private event PartyItemEventHandler onNewItemAdded;

	private event PartyItemSlotEventHandler onItemRemoved;

	private event StatEventHandler onGoldChanged;

	private event StatEventHandler onPartyGoldChanged;

	private event ProsperityEventHandler onProsperityChanged;

	private event StatEventHandler onReputationChanged;

	private event CharacterGoldEventHandler onCharacterGoldChanged;

	private event CharacterEventHandler onCharacterXpChanged;

	private event CharacterEventHandler onCharacterLevelupUnlocked;

	private event CharacterEventHandler onCharacterConditionGained;

	private event MapCharacterEventHandler onCharacterCreated;

	private event CharacterPerkPointsEventHandler onCharacterPerkPointsChanged;

	private event CharacterItemsEventHandler onCharacterItemUnequipped;

	private event CharacterItemEventHandler onCharacterItemEquipped;

	private event CharacterItemEventHandler onCharacterItemBound;

	private event CharacterItemEventHandler onCharacterItemUnbound;

	public void ClearEvents()
	{
		this.onGoldChanged = null;
		this.onPartyGoldChanged = null;
		this.onCharacterGoldChanged = null;
		this.onCharacterXpChanged = null;
		this.onCharacterLevelupUnlocked = null;
		this.onCharacterPerkPointsChanged = null;
		this.onCharacterConditionGained = null;
		this.onCharacterCreated = null;
		this.onProsperityChanged = null;
		this.onReputationChanged = null;
		this.onCharacterItemEquipped = null;
		this.onCharacterItemUnequipped = null;
		this.onCharacterItemUnbound = null;
		this.onCharacterItemBound = null;
		this.onItemRemoved = null;
		this.onNewItemAdded = null;
	}

	public void OnCharacterItemEquipped(CMapCharacter character, CItem item)
	{
		this.onCharacterItemEquipped?.Invoke(item, character);
	}

	public void RegisterToOnCharacterItemEquipped(CharacterItemEventHandler callback)
	{
		onCharacterItemEquipped -= callback;
		onCharacterItemEquipped += callback;
	}

	public void UnregisterToOnCharacterItemEquipped(CharacterItemEventHandler callback)
	{
		onCharacterItemEquipped -= callback;
	}

	public void OnCharacterItemUnbound(CMapCharacter character, CItem item)
	{
		this.onCharacterItemUnbound?.Invoke(item, character);
	}

	public void RegisterToOnCharacterItemUnbound(CharacterItemEventHandler callback)
	{
		onCharacterItemUnbound -= callback;
		onCharacterItemUnbound += callback;
	}

	public void UnregisterToOnCharacterItemUnbound(CharacterItemEventHandler callback)
	{
		onCharacterItemUnbound -= callback;
	}

	public void OnCharacterItemUnequipped(CMapCharacter character, List<CItem> items)
	{
		this.onCharacterItemUnequipped?.Invoke(items, character);
	}

	public void RegisterToOnCharacterItemUnequipped(CharacterItemsEventHandler callback)
	{
		onCharacterItemUnequipped -= callback;
		onCharacterItemUnequipped += callback;
	}

	public void UnregisterToOnCharacterItemUnequipped(CharacterItemsEventHandler callback)
	{
		onCharacterItemUnequipped -= callback;
	}

	public void OnCharacterItemBound(CMapCharacter character, CItem item)
	{
		this.onCharacterItemBound?.Invoke(item, character);
	}

	public void RegisterToOnCharacterItemBound(CharacterItemEventHandler callback)
	{
		onCharacterItemBound -= callback;
		onCharacterItemBound += callback;
	}

	public void UnregisterToOnCharacterItemBound(CharacterItemEventHandler callback)
	{
		onCharacterItemBound -= callback;
	}

	public void OnItemRemoved(CItem item, int slotIndex)
	{
		this.onItemRemoved?.Invoke(item, slotIndex);
	}

	public void RegisterToOnItemRemoved(PartyItemSlotEventHandler callback)
	{
		onItemRemoved -= callback;
		onItemRemoved += callback;
	}

	public void UnregisterToOnItemRemoved(PartyItemSlotEventHandler callback)
	{
		onItemRemoved -= callback;
	}

	public void OnItemAdded(CItem item)
	{
		this.onNewItemAdded?.Invoke(item);
	}

	public void RegisterToOnItemAdded(PartyItemEventHandler callback)
	{
		onNewItemAdded -= callback;
		onNewItemAdded += callback;
	}

	public void UnregisterToOnItemAdded(PartyItemEventHandler callback)
	{
		onNewItemAdded -= callback;
	}

	public void OnCharacterCreated(CMapCharacter character)
	{
		this.onCharacterCreated?.Invoke(character);
	}

	public void RegisterToOnCharacterCreated(MapCharacterEventHandler callback)
	{
		onCharacterCreated -= callback;
		onCharacterCreated += callback;
	}

	public void UnregisterToOnCharacterCreated(MapCharacterEventHandler callback)
	{
		onCharacterCreated -= callback;
	}

	public void OnCharacterPerkPointsChanged(string characterId, string characterName, int perkPoints)
	{
		this.onCharacterPerkPointsChanged?.Invoke(characterId, characterName, perkPoints);
	}

	public void RegisterToOnCharacterPerkPointsChanged(CharacterPerkPointsEventHandler callback)
	{
		onCharacterPerkPointsChanged -= callback;
		onCharacterPerkPointsChanged += callback;
	}

	public void UnregisterToOnCharacterPerkPointsChanged(CharacterPerkPointsEventHandler callback)
	{
		onCharacterPerkPointsChanged -= callback;
	}

	public void OnCharacterConditionGained(string characterId, string characterName)
	{
		this.onCharacterConditionGained?.Invoke(characterId, characterName);
	}

	public void RegisterToOnCharacterConditionGained(CharacterEventHandler callback)
	{
		onCharacterConditionGained -= callback;
		onCharacterConditionGained += callback;
	}

	public void UnregisterToOnCharacterConditionGained(CharacterEventHandler callback)
	{
		onCharacterConditionGained -= callback;
	}

	public void OnCharacterLevelupUnlocked(string characterId, string characterName)
	{
		this.onCharacterLevelupUnlocked?.Invoke(characterId, characterName);
	}

	public void RegisterToOnCharacterLevelupUnlocked(CharacterEventHandler callback)
	{
		onCharacterLevelupUnlocked -= callback;
		onCharacterLevelupUnlocked += callback;
	}

	public void UnregisterToOnCharacterLevelupUnlocked(CharacterEventHandler callback)
	{
		onCharacterLevelupUnlocked -= callback;
	}

	public void OnCharacterXpChanged(string characterId, string characterName)
	{
		this.onCharacterXpChanged?.Invoke(characterId, characterName);
	}

	public void RegisterToOnCharacterXpChanged(CharacterEventHandler callback)
	{
		onCharacterXpChanged -= callback;
		onCharacterXpChanged += callback;
	}

	public void UnregisterToOnCharacterXpChanged(CharacterEventHandler callback)
	{
		onCharacterXpChanged -= callback;
	}

	public void OnProsperityChanged(int newProsperityXp, int previousProsperityXp, int newProsperityLevel, int previousProsperityLevel)
	{
		this.onProsperityChanged?.Invoke(newProsperityXp, previousProsperityXp, newProsperityLevel, previousProsperityLevel);
	}

	public void RegisterToOnProsperityChanged(ProsperityEventHandler callback)
	{
		onProsperityChanged -= callback;
		onProsperityChanged += callback;
	}

	public void UnregisterToOnProsperityChange(ProsperityEventHandler callback)
	{
		onProsperityChanged -= callback;
	}

	public void OnReputationChanged(int reputation)
	{
		this.onReputationChanged?.Invoke(reputation);
	}

	public void RegisterToOnReputationChanged(StatEventHandler callback)
	{
		onReputationChanged -= callback;
		onReputationChanged += callback;
	}

	public void UnregisterToOnReputationChanged(StatEventHandler callback)
	{
		onReputationChanged -= callback;
	}

	public virtual void OnPartyGoldChanged(int gold)
	{
		this.onPartyGoldChanged?.Invoke(gold);
		this.onGoldChanged?.Invoke(gold);
	}

	public virtual void RegisterToOnPartyGoldChanged(StatEventHandler callback)
	{
		onPartyGoldChanged += callback;
	}

	public virtual void UnregisterToOnPartyGoldChanged(StatEventHandler callback)
	{
		onPartyGoldChanged -= callback;
	}

	public virtual void OnCharacterGoldChanged(int gold, string characterId, string characterName)
	{
		this.onCharacterGoldChanged?.Invoke(gold, characterId, characterName);
		this.onGoldChanged?.Invoke(gold);
	}

	public void RegisterToOnCharacterGoldChanged(CharacterGoldEventHandler callback)
	{
		onCharacterGoldChanged += callback;
	}

	public void UnregisterToOnCharacterGoldChanged(CharacterGoldEventHandler callback)
	{
		onCharacterGoldChanged -= callback;
	}

	public void RegisterToOnGoldChanged(StatEventHandler callback)
	{
		onGoldChanged += callback;
	}

	public void UnregisterToOnGoldChanged(StatEventHandler callback)
	{
		onGoldChanged -= callback;
	}
}
