namespace ScenarioRuleLibrary;

public class CPlacingTrap_MessageData : CMessageData
{
	public CActor m_ActorPlacingTrap;

	public CTile m_Tile;

	public CAbility m_TrapAbility;

	public CObjectTrap m_TrapObject;

	public CPlacingTrap_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PlacingTrap, actorSpawningMessage, animOverload)
	{
	}
}
