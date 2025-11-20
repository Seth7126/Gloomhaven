namespace MapRuleLibrary.Client;

public class CProsperityChanged_MapClientMessage : CMapClientMessage
{
	public int m_NewProsperityXP;

	public int m_PreviousProsperityXP;

	public int m_NewProsperityLevel;

	public int m_PreviousProsperityLevel;

	public CProsperityChanged_MapClientMessage(int newProsperityXp, int previousProsperityXp, int newProsperityLevel, int previousProsperityLevel)
		: base(EMapClientMessageType.ProsperityChanged)
	{
		m_NewProsperityXP = newProsperityXp;
		m_PreviousProsperityXP = previousProsperityXp;
		m_NewProsperityLevel = newProsperityLevel;
		m_PreviousProsperityLevel = previousProsperityLevel;
	}
}
