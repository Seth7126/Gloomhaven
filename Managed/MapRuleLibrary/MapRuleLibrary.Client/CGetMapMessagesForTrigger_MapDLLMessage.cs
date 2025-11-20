using MapRuleLibrary.YML.Message;

namespace MapRuleLibrary.Client;

public class CGetMapMessagesForTrigger_MapDLLMessage : CMapDLLMessage
{
	public EMapMessageTrigger m_MapMessageTrigger;

	public CGetMapMessagesForTrigger_MapDLLMessage(EMapMessageTrigger mapMessageTrigger)
		: base(EMapDLLMessageType.GetMapMessagesForTrigger)
	{
		m_MapMessageTrigger = mapMessageTrigger;
	}
}
