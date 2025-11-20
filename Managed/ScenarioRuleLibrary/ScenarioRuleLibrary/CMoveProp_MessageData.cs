namespace ScenarioRuleLibrary;

public class CMoveProp_MessageData : CMessageData
{
	public CObjectProp m_MoveProp;

	public CTile m_MoveToTile;

	public float m_MoveSpeed;

	public CMoveProp_MessageData()
		: base(MessageType.MoveProp, null)
	{
	}
}
