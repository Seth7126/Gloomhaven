using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class CharacterImportData : ICharacterImportData
{
	private CQuestState questState;

	private CMapCharacter character;

	public QuestCompletionToken.QuestCompletionData QuestCompletionData { get; private set; }

	public string CharacterID { get; private set; }

	public string CharacterName { get; private set; }

	public Sprite CharacterIcon => UIInfoTools.Instance.GetCharacterMarker(CharacterID);

	public int Gold => QuestCompletionData.QuestGainedGold;

	public int XP => QuestCompletionData.QuestGainedXP;

	public Sprite ItemIcon { get; private set; }

	public string ItemLocKey { get; private set; }

	public string Id => CharacterID + CharacterName + QuestCompletionData.QuestID;

	public CharacterImportData(string characterID, string characterName, QuestCompletionToken.QuestCompletionData data)
	{
		CharacterID = characterID;
		CharacterName = characterName;
		QuestCompletionData = data;
		character = AdventureState.MapState.MapParty.CheckCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == characterID && x.CharacterName == characterName);
		questState = AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState x) => x.ID == data.QuestID);
		CItem cItem = questState?.QuestCompletionRewardGroup.Rewards.FirstOrDefault((Reward it) => it.Item != null && it.Item.YMLData.ValidEquipCharacterClassIDs.Contains(characterID))?.Item;
		if (cItem != null)
		{
			ItemIcon = UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
			ItemLocKey = cItem.Name;
		}
	}

	public bool CanImport()
	{
		return character.Level >= 5;
	}

	public void Import()
	{
		if (character != null)
		{
			if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
			{
				character.ModifyGold(Gold, useGoldModifier: true);
			}
			else
			{
				AdventureState.MapState.MapParty.ModifyPartyGold(Gold, useGoldModifier: true);
			}
			character.GainEXP(XP, AdventureState.MapState.Difficulty.HasXPModifier ? AdventureState.MapState.Difficulty.XPModifier : 1f);
		}
		questState?.SoloScenarioImportCompletion(character, QuestCompletionData.QuestCompletedScenarioLevel);
	}
}
