namespace MapRuleLibrary.Client;

public class CMapDLLMessage
{
	public EMapDLLMessageType m_MessageType;

	public CMapDLLMessage(EMapDLLMessageType eMessageType)
	{
		m_MessageType = eMessageType;
	}
}
