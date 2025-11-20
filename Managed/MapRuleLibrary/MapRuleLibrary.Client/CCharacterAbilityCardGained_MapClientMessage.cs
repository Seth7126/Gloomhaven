using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterAbilityCardGained_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public CAbilityCard m_AbilityCard;

	public CCharacterAbilityCardGained_MapClientMessage(string characterId, string characterName, CAbilityCard abilityCard)
		: base(EMapClientMessageType.CharacterAbilityCardGained)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
		m_AbilityCard = abilityCard;
	}
}
