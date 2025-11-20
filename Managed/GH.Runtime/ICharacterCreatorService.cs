using MapRuleLibrary.Party;

public interface ICharacterCreatorService
{
	CMapCharacter Create(string classId, string characterName);

	void AssignPersonalQuest(CMapCharacter character, CPersonalQuestState personalQuest);
}
