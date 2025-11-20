using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorIsHealing_MessageData : CMessageData
{
	public Dictionary<CActor, int> m_ActorsHealedAndHealStrength;

	public CAbilityHeal m_HealAbility;

	public CActorIsHealing_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsHealing, actorSpawningMessage, animOverload)
	{
	}
}
