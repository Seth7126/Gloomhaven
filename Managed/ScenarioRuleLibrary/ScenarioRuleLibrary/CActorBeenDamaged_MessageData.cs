namespace ScenarioRuleLibrary;

public class CActorBeenDamaged_MessageData : CMessageData
{
	public CActor m_ActorBeingDamaged;

	public CAbilityDamage m_DamageAbility;

	public int m_ActorOriginalHealth;

	public bool m_ActorWasAsleep;

	public int? m_ActualDamage;

	public CActorBeenDamaged_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorBeenDamaged, actorSpawningMessage)
	{
	}
}
