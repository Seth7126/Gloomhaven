namespace MapRuleLibrary.Client;

public class CMove_MapDLLMessage : CMapDLLMessage
{
	public string m_StartLocation;

	public string m_EndLocation;

	public CMove_MapDLLMessage(string startLocation, string endLocation)
		: base(EMapDLLMessageType.Move)
	{
		m_StartLocation = startLocation;
		m_EndLocation = endLocation;
	}
}
