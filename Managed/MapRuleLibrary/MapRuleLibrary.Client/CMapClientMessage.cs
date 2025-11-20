namespace MapRuleLibrary.Client;

public class CMapClientMessage
{
	public EMapClientMessageType m_MessageType;

	public CMapClientMessage(EMapClientMessageType eMessageType)
	{
		m_MessageType = eMessageType;
	}
}
