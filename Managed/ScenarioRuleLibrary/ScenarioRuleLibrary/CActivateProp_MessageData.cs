namespace ScenarioRuleLibrary;

public class CActivateProp_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public bool m_InitialLoad;

	public bool m_CarriedByActor;

	public CActor m_CreditActor;

	public CActivateProp_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActivateProp, actorSpawningMessage)
	{
	}
}
