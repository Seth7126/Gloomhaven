using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class CMapCharacterService : ICharacterService, ICharacterEnhancementService, ICharacter
{
	private CMapCharacter characterData;

	public CMapCharacter Data => characterData;

	public string CharacterID => characterData.CharacterID;

	public string CharacterName => characterData.CharacterName;

	public ECharacter CharacterModel => characterData.CharacterYMLData.Model;

	public CharacterYMLData Class => characterData.CharacterYMLData;

	public int PerkPoints => characterData.PerkPoints;

	public List<CharacterPerk> Perks => characterData.Perks;

	public int PerkChecks => characterData.PerkChecks;

	public int PerkChecksToComplete
	{
		get
		{
			if (!AdventureState.MapState.IsCampaign)
			{
				return 0;
			}
			return 3;
		}
	}

	public List<AttackModifierYMLData> AttackModifierDeck
	{
		get
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == CharacterID);
			if (cCharacterClass == null)
			{
				Debug.LogError("Unable to find player class" + CharacterID);
				return new List<AttackModifierYMLData>();
			}
			List<AttackModifierYMLData> applyToThisList = cCharacterClass.AttackModifierCardsPool.ToList();
			CCharacterClass.ApplyPerksToList((from s in Perks
				where s.IsActive
				select s.Perk).ToList(), ref applyToThisList);
			return applyToThisList;
		}
	}

	public List<CItem> EquippedItems => characterData.CheckEquippedItems;

	public int Gold
	{
		get
		{
			if (AdventureState.MapState.MapParty != null)
			{
				if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
				{
					return AdventureState.MapState.MapParty.PartyGold;
				}
				if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
				{
					return characterData.CharacterGold;
				}
			}
			return 0;
		}
	}

	public int Level => characterData.Level;

	public List<int> HandAbilityCardIDs => Data.HandAbilityCardIDs;

	public List<int> OwnedAbilityCardIDs => Data.OwnedAbilityCardIDs;

	public List<CEnhancement> AllEnhancements => characterData.Enhancements;

	public bool IsUnderMyControl => Data.IsUnderMyControl;

	private event BasicEventHandler onLevelupAvailable;

	private event BasicEventHandler onPerkPointsChanged;

	private event StatEventHandler onCharacterGoldChanged;

	public CMapCharacterService(CMapCharacter characterData)
	{
		this.characterData = characterData;
	}

	public void AddCallbackOnLevelUpAvailable(BasicEventHandler callback)
	{
		onLevelupAvailable -= callback;
		onLevelupAvailable += callback;
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterLevelupUnlocked(OnLevelUpAvailable);
			if (AdventureState.MapState.IsCampaign)
			{
				Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnProsperityChanged(OnProsperityChanged);
			}
		}
	}

	public void RemoveCallbackOnLevelUpAvailable(BasicEventHandler callback)
	{
		onLevelupAvailable -= callback;
		if ((this.onLevelupAvailable == null || this.onLevelupAvailable.GetInvocationList().Length == 0) && Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance?.EventBuss.UnregisterToOnCharacterLevelupUnlocked(OnLevelUpAvailable);
			if (AdventureState.MapState.IsCampaign)
			{
				Singleton<MapChoreographer>.Instance?.EventBuss.UnregisterToOnProsperityChange(OnProsperityChanged);
			}
		}
	}

	private void OnProsperityChanged(int newprosperityxp, int oldprosperityxp, int newprosperitylevel, int oldprosperitlevel)
	{
		if (newprosperitylevel != oldprosperitlevel)
		{
			this.onLevelupAvailable?.Invoke();
		}
	}

	private void OnLevelUpAvailable(string characterId, string characterName)
	{
		if (characterId == characterData.CharacterID && characterName == characterData.CharacterName)
		{
			this.onLevelupAvailable?.Invoke();
		}
	}

	public void UpdatePerkPoints(int points)
	{
		characterData.UpdatePerkPoints(points);
	}

	public void AddCallbackOnPerkPointsChanged(BasicEventHandler callback)
	{
		onPerkPointsChanged -= callback;
		onPerkPointsChanged += callback;
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterPerkPointsChanged(OnPerkPointChanged);
		}
	}

	public void RemoveCallbackOnPerkPointsChanged(BasicEventHandler callback)
	{
		onPerkPointsChanged -= callback;
		if ((this.onPerkPointsChanged == null || this.onPerkPointsChanged.GetInvocationList().Length == 0) && Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterPerkPointsChanged(OnPerkPointChanged);
		}
	}

	private void OnPerkPointChanged(string characterId, string characterName, int perkPoints)
	{
		if (characterId == characterData.CharacterID && characterName == characterData.CharacterName)
		{
			this.onPerkPointsChanged?.Invoke();
		}
	}

	public void ModifyGold(int gold, bool useGoldModifier = false)
	{
		if (AdventureState.MapState.MapParty != null)
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				AdventureState.MapState.MapParty.ModifyPartyGold(gold, useGoldModifier);
			}
			else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				characterData.ModifyGold(gold, useGoldModifier);
			}
		}
	}

	public void AddCallbackOnGoldChanged(IGoldEventListener callback)
	{
		if (Singleton<MapChoreographer>.Instance != null)
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
			{
				Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnPartyGoldChanged(callback.OnUpdatedGold);
				return;
			}
			onCharacterGoldChanged -= callback.OnUpdatedGold;
			onCharacterGoldChanged += callback.OnUpdatedGold;
			Singleton<MapChoreographer>.Instance.EventBuss.RegisterToOnCharacterGoldChanged(OnCharacterGoldChanged);
		}
	}

	public void RemoveCallbackOnGoldChanged(IGoldEventListener callback)
	{
		if (!(Singleton<MapChoreographer>.Instance != null))
		{
			return;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnPartyGoldChanged(callback.OnUpdatedGold);
			return;
		}
		onCharacterGoldChanged -= callback.OnUpdatedGold;
		if (this.onCharacterGoldChanged == null || this.onCharacterGoldChanged.GetInvocationList().Length == 0)
		{
			Singleton<MapChoreographer>.Instance.EventBuss.UnregisterToOnCharacterGoldChanged(OnCharacterGoldChanged);
		}
	}

	private void OnCharacterGoldChanged(int newgoldvalue, string characterid, string charactername)
	{
		if (characterid == characterData.CharacterID && charactername == characterData.CharacterName)
		{
			this.onCharacterGoldChanged?.Invoke(newgoldvalue);
		}
	}

	public bool CanLevelup()
	{
		if (!HasXpToLevelUp())
		{
			return CanLevelupToProsperityLevel();
		}
		return true;
	}

	private bool CanLevelupToProsperityLevel()
	{
		if (AdventureState.MapState.IsCampaign)
		{
			return Level < AdventureState.MapState.MapParty.ProsperityLevel;
		}
		return false;
	}

	public bool HasXpToLevelUp()
	{
		return characterData.EXP >= characterData.GetNextXPThreshold();
	}

	public int GetLevelsToLevelUp()
	{
		return characterData.GetLevelsToLevelUp();
	}

	public int CalculatePriceResetLevel()
	{
		return characterData.CalculatePriceResetLevel();
	}

	public void ResetLevels()
	{
		List<CEnhancement> list = characterData.Enhancements.Select((CEnhancement s) => s.Copy()).ToList();
		foreach (CEnhancement item in list)
		{
			item.Enhancement = EEnhancement.NoEnhancement;
			item.PaidPrice = 0;
		}
		characterData.ResetLevels();
		SaveDataShared.ApplyEnhancementIcons(list, characterData.CharacterID);
	}

	public bool HasFreeLevelReset()
	{
		if (characterData.LastFreeLevelResetTicket != null)
		{
			return !characterData.LastFreeLevelResetTicket.IsUsed;
		}
		return false;
	}

	public bool AddNewFreeResetLevel(string notificationId, int notificationOrder)
	{
		if (characterData.LastFreeLevelResetTicket != null && characterData.LastFreeLevelResetTicket.Order >= notificationOrder)
		{
			return false;
		}
		characterData.LastFreeLevelResetTicket = new CFreeLevelResetTicket(notificationId, notificationOrder);
		SaveData.Instance.SaveCurrentAdventureData();
		return true;
	}

	public void UseFreeResetLevel()
	{
		characterData.LastFreeLevelResetTicket.IsUsed = true;
	}

	public bool IsNewPerk(AttackModifierYMLData perk)
	{
		return characterData.NewEquippedItemsWithModifiers.Contains(perk.Name);
	}

	public void GainCard(CAbilityCard card)
	{
		characterData.GainCard(card);
	}

	public List<CAbilityCard> GetUnownedAbilityCards(int minCardLevel, int maxCardLevel)
	{
		return characterData.UnownedAbilityCards.Where((CAbilityCard it) => it.Level >= minCardLevel && it.Level <= maxCardLevel).ToList();
	}

	public int CalculateSellPriceEnhancement(EnhancementButtonBase enhancement)
	{
		return Mathf.RoundToInt((float)GetPaidPriceEnhancement(enhancement) * GetEnhancementSellPricePercentage());
	}

	public float GetEnhancementSellPricePercentage()
	{
		return ScenarioRuleClient.SRLYML.Enhancements.SellPricePercentage(AdventureState.MapState.EnhancementMode == EEnhancementMode.ClassPersistent);
	}

	public int GetPaidPriceEnhancement(EnhancementButtonBase button)
	{
		CEnhancement buttonEnhancement = button.Enhancement;
		return characterData.Enhancements.SingleOrDefault((CEnhancement s) => s.Enhancement == buttonEnhancement.Enhancement && s.EnhancementLine == buttonEnhancement.EnhancementLine && s.EnhancementSlot == buttonEnhancement.EnhancementSlot && buttonEnhancement.AbilityCardID == s.AbilityCardID && buttonEnhancement.Ability.ID == s.Ability.ID)?.PaidPrice ?? 0;
	}

	public bool HasFreeEnhancementSlots()
	{
		return GetFreeEnhancementSlots() > 0;
	}

	public int GetFreeEnhancementSlots()
	{
		if (AdventureState.MapState.IsCampaign)
		{
			List<int> list = (from it in characterData.Enhancements
				where it.Enhancement != EEnhancement.NoEnhancement
				select it.AbilityCardID).Distinct().ToList();
			return AdventureState.MapState.HeadquartersState.EnhancementSlots - list.Count;
		}
		return AdventureState.MapState.HeadquartersState.EnhancementSlots - characterData.Enhancements.Count((CEnhancement it) => it.Enhancement != EEnhancement.NoEnhancement);
	}

	public List<CAbilityCard> GetOwnedAbilityCards()
	{
		return characterData.GetOwnedAbilityCards();
	}

	public bool IsCardEnhanced(int abilityCardID)
	{
		return characterData.Enhancements.Any((CEnhancement x) => x.AbilityCardID == abilityCardID && x.Enhancement != EEnhancement.NoEnhancement);
	}

	public int CountEnhancements(int abilityCardID)
	{
		return characterData.Enhancements.Count((CEnhancement x) => x.AbilityCardID == abilityCardID && x.Enhancement != EEnhancement.NoEnhancement);
	}

	public bool LevelUp()
	{
		if (!CanLevelup())
		{
			return false;
		}
		bool adjustPerkPointsToo = !FFSNetwork.IsOnline || IsUnderMyControl;
		int nextXPThreshold = characterData.GetNextXPThreshold();
		if (CanLevelupToProsperityLevel() && characterData.EXP < nextXPThreshold)
		{
			characterData.GainEXP(nextXPThreshold - characterData.EXP, 1f);
		}
		characterData.LevelUp(adjustPerkPointsToo);
		SaveData.Instance.SaveCurrentAdventureData();
		return true;
	}
}
