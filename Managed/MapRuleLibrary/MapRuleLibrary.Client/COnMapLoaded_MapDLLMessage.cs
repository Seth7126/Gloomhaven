namespace MapRuleLibrary.Client;

public class COnMapLoaded_MapDLLMessage : CMapDLLMessage
{
	public bool m_IsMPClientJoining;

	public COnMapLoaded_MapDLLMessage(bool isMPClientJoining)
		: base(EMapDLLMessageType.OnMapLoaded)
	{
		m_IsMPClientJoining = isMPClientJoining;
	}
}
