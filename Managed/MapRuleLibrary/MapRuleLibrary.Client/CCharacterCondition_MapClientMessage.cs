using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterCondition_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public CCondition.ENegativeCondition m_NegativeCondition;

	public CCondition.EPositiveCondition m_PositiveCondition;

	public CCharacterCondition_MapClientMessage(string characterId, string characterName, CCondition.ENegativeCondition negativeCondition)
		: base(EMapClientMessageType.CharacterConditionGained)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
		m_NegativeCondition = negativeCondition;
		m_PositiveCondition = CCondition.EPositiveCondition.NA;
	}

	public CCharacterCondition_MapClientMessage(string characterId, string characterName, CCondition.EPositiveCondition positiveCondition)
		: base(EMapClientMessageType.CharacterConditionGained)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
		m_NegativeCondition = CCondition.ENegativeCondition.NA;
		m_PositiveCondition = positiveCondition;
	}
}
