using System.Collections.Generic;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Message;

namespace MapRuleLibrary.Client;

public class CShowMapMessages_MapClientMessage : CMapClientMessage
{
	public EMapMessageTrigger m_messageTrigger;

	public List<CMapMessageState> m_MapMessages;

	public CShowMapMessages_MapClientMessage(EMapMessageTrigger messageTrigger, List<CMapMessageState> mapMessages)
		: base(EMapClientMessageType.ShowMapMessages)
	{
		m_messageTrigger = messageTrigger;
		m_MapMessages = mapMessages;
	}
}
