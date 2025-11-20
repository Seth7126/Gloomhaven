namespace MapRuleLibrary.Client;

public class CReputationChanged_MapClientMessage : CMapClientMessage
{
	public int m_Reputation;

	public CReputationChanged_MapClientMessage(int reputation)
		: base(EMapClientMessageType.ReputationChanged)
	{
		m_Reputation = reputation;
	}
}
