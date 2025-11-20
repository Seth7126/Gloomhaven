using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Locations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public class TempleShopService : ITempleShopService, IBlessingShopService
{
	private CTempleState templeState;

	private CMapParty party;

	public int DevotionLevel => templeState.DevotionLevel + 1;

	private bool IsMaxLevel => templeState.Temple.DonationTable.Count <= DevotionLevel;

	public int DevotionCurrentProgress
	{
		get
		{
			if (IsMaxLevel)
			{
				return templeState.Temple.DonationTable.LastOrDefault()?.Item1 ?? 0;
			}
			if (templeState.DevotionLevel >= 1)
			{
				return templeState.GoldDonated - CalculateDevotionTotalProgress(templeState.DevotionLevel);
			}
			return templeState.GoldDonated;
		}
	}

	public int NextDevotionLevelAmount
	{
		get
		{
			if (IsMaxLevel)
			{
				return templeState.Temple.DonationTable.LastOrDefault()?.Item1 ?? 0;
			}
			return CalculateDevotionTotalProgress(DevotionLevel) - CalculateDevotionTotalProgress(templeState.DevotionLevel);
		}
	}

	public bool IsTempleIntroductionShown => party.HasIntroduced(EIntroductionConcept.Temple.ToString());

	public TempleShopService()
	{
		templeState = AdventureState.MapState.TempleState;
		party = AdventureState.MapState.MapParty;
	}

	public bool CanAfford(string characterID, TempleYML.TempleBlessingDefinition blessing)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			return blessing.GoldCost <= party.PartyGold;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			if (characterID.IsNullOrEmpty())
			{
				return false;
			}
			CMapCharacter cMapCharacter = party.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == characterID);
			if (cMapCharacter == null)
			{
				Debug.LogErrorFormat("SelectedCharacters doesn't contain a character with ID {0} (Selected characters: {1})", characterID, string.Join(", ", party.SelectedCharacters.Select((CMapCharacter it) => it.CharacterID)));
				return false;
			}
			return cMapCharacter.CharacterGold >= blessing.GoldCost;
		}
		return false;
	}

	public bool IsAvailable(string characterID, TempleYML.TempleBlessingDefinition blessing)
	{
		if (characterID.IsNullOrEmpty())
		{
			return false;
		}
		CMapCharacter cMapCharacter = party.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == characterID);
		if (cMapCharacter == null)
		{
			Debug.LogErrorFormat("SelectedCharacters doesn't contain a character with ID {0} (Selected characters: {1})", characterID, string.Join(", ", party.SelectedCharacters.Select((CMapCharacter it) => it.CharacterID)));
			return false;
		}
		if (blessing.TempleBlessingCondition.Type == RewardCondition.EConditionType.Negative)
		{
			return !cMapCharacter.NegativeConditions.Exists((NegativeConditionPair it) => IsEqual(it, blessing.TempleBlessingCondition));
		}
		if (blessing.TempleBlessingCondition.Type == RewardCondition.EConditionType.Positive)
		{
			return !cMapCharacter.PositiveConditions.Exists((PositiveConditionPair it) => IsEqual(it, blessing.TempleBlessingCondition));
		}
		return true;
	}

	private bool IsEqual(NegativeConditionPair condition, RewardCondition blessing)
	{
		if (condition.MapDuration == blessing.MapDuration && condition.NegativeCondition == blessing.NegativeCondition && condition.RoundDuration == blessing.RoundDuration)
		{
			return condition.ConditionDecTrigger == EConditionDecTrigger.Turns;
		}
		return false;
	}

	private bool IsEqual(PositiveConditionPair condition, RewardCondition blessing)
	{
		if (condition.MapDuration == blessing.MapDuration && condition.PositiveCondition == blessing.PositiveCondition && condition.RoundDuration == blessing.RoundDuration)
		{
			return condition.ConditionDecTrigger == EConditionDecTrigger.Turns;
		}
		return false;
	}

	public void Buy(string characterID, TempleYML.TempleBlessingDefinition blessing)
	{
		CMapCharacter cMapCharacter = party.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == characterID);
		if (cMapCharacter == null)
		{
			Debug.LogErrorFormat("SelectedCharacters doesn't contain a character with ID {0} (Selected characters: {1})", characterID, string.Join(", ", party.SelectedCharacters.Select((CMapCharacter it) => it.CharacterID)));
		}
		else
		{
			templeState.ApplyTempleBlessing(cMapCharacter, blessing);
			AdventureState.MapState.CheckPersonalQuests();
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	public List<TempleYML.TempleBlessingDefinition> GetAvailableBlessings()
	{
		return templeState.Temple.AvailableBlessings;
	}

	public bool CanBuy(string characterID, TempleYML.TempleBlessingDefinition blessing)
	{
		if (!IsAvailable(characterID, blessing))
		{
			NewPartyDisplayUI.PartyDisplay.ShowPerksWarning(characterID, show: true);
			return false;
		}
		if (!CanAfford(characterID, blessing))
		{
			NewPartyDisplayUI.PartyDisplay.ShowWarningGold(show: true, (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? characterID : null);
			return false;
		}
		if (FFSNetwork.IsOnline)
		{
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterID);
			int controllableID = (AdventureState.MapState.IsCampaign ? cMapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter.CharacterID));
			if ((FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
			{
				return false;
			}
			if (!PlayerRegistry.MyPlayer.HasControlOver(controllableID))
			{
				return false;
			}
		}
		return true;
	}

	public int CalculateTotalGoldDonated()
	{
		return templeState.GoldDonated;
	}

	public int CalculateDevotionTotalProgress(int level)
	{
		if (level != 0)
		{
			return templeState.Temple.DonationTable[level - 1].Item1;
		}
		return 0;
	}

	public void SetTempleIntroductionShown()
	{
		if (party.MarkIntroDone(EIntroductionConcept.Temple.ToString()))
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}
}
