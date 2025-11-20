using MapRuleLibrary.MapState;

namespace MapRuleLibrary.Client;

public class CMoveComplete_MapDLLMessage : CMapDLLMessage
{
	public CLocationState m_LocationState;

	public CMoveComplete_MapDLLMessage(CLocationState locationState)
		: base(EMapDLLMessageType.MoveComplete)
	{
		m_LocationState = locationState;
	}
}
