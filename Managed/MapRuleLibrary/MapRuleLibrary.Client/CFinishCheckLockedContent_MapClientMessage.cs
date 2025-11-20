namespace MapRuleLibrary.Client;

public class CFinishCheckLockedContent_MapClientMessage : CMapClientMessage
{
	public CFinishCheckLockedContent_MapClientMessage()
		: base(EMapClientMessageType.FinishCheckLockedContent)
	{
	}
}
