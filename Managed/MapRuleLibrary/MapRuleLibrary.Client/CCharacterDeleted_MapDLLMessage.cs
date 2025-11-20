using MapRuleLibrary.Party;

namespace MapRuleLibrary.Client;

public class CCharacterDeleted_MapDLLMessage : CMapDLLMessage
{
	public CMapCharacter CharacterToDelete;

	public CCharacterDeleted_MapDLLMessage(CMapCharacter characterToDelete)
		: base(EMapDLLMessageType.DeleteCharacter)
	{
		CharacterToDelete = characterToDelete;
	}
}
