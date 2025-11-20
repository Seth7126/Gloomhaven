using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CLooting_MessageData : CMessageData
{
	public CAbilityLoot m_LootAbility;

	public CActor m_ActorLooting;

	public List<CObjectProp> m_PropsLooted;

	public int m_ExtraGoldFromAdditionalEffects;

	public CLooting_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.Looting, actorSpawningMessage, animOverload)
	{
	}
}
