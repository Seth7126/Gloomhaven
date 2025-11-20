namespace ScenarioRuleLibrary;

public class CActorIsSelectingDamageFocusTargets : CMessageData
{
	public CAbilityDamage m_DamageAbility;

	public bool m_DamageSelf;

	public CActorIsSelectingDamageFocusTargets(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingDamageFocus, actorSpawningMessage, animOverload)
	{
	}
}
