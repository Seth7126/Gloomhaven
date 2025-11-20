using MapRuleLibrary.MapState;

namespace MapRuleLibrary.Client;

public class CStartMoving_MapClientMessage : CMapClientMessage
{
	public CLocationState m_StartLocation;

	public CLocationState m_EndLocation;

	public CStartMoving_MapClientMessage(CLocationState startLocation, CLocationState endLocation)
		: base(EMapClientMessageType.StartMoving)
	{
		m_StartLocation = startLocation;
		m_EndLocation = endLocation;
	}
}
