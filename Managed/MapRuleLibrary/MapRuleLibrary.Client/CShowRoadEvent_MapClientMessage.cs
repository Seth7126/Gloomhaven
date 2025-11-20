namespace MapRuleLibrary.Client;

public class CShowRoadEvent_MapClientMessage : CMapClientMessage
{
	public CShowRoadEvent_MapClientMessage()
		: base(EMapClientMessageType.ShowRoadEvent)
	{
	}
}
