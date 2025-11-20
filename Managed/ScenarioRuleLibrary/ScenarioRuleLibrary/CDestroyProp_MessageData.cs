namespace ScenarioRuleLibrary;

public class CDestroyProp_MessageData : CMessageData
{
	public CObjectProp m_Prop;

	public float m_DestroyDelay;

	public CDestroyProp_MessageData()
		: base(MessageType.DestroyProp, null)
	{
	}
}
