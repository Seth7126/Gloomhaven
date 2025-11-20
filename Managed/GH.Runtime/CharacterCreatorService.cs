using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;

public class CharacterCreatorService : ICharacterCreatorService
{
	public CMapCharacter Create(string classId, string characterName)
	{
		return CreateCharacter(classId, characterName);
	}

	public void AssignPersonalQuest(CMapCharacter character, CPersonalQuestState personalQuest)
	{
		character.AssignPersonalQuest(personalQuest, removeCard: false);
	}

	public static CMapCharacter CreateCharacter(string classId, string characterName)
	{
		CMapCharacter cMapCharacter = new CMapCharacter(classId, characterName, AdventureState.MapState.HeadquartersState.CurrentStartingPerksAmount);
		cMapCharacter.DrawPossiblePersonalQuests();
		cMapCharacter.GiveCreateCharacterGoldAndItems();
		AdventureState.MapState.MapParty.UnlockNewProsperityItems();
		AdventureState.MapState.DebugNextPersonalQuests.Clear();
		return cMapCharacter;
	}

	public static void AssignPersonalQuestToCharacter(string characterID, string characterName, string personalQuestID)
	{
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.CheckCharacters.Single((CMapCharacter it) => it.CharacterID == characterID && it.CharacterName == characterName);
		cMapCharacter.AssignPersonalQuest(cMapCharacter.PossiblePersonalQuests.Single((CPersonalQuestState it) => it.ID == personalQuestID));
	}
}
