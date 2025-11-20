using MapRuleLibrary.YML.Shared;

namespace MapRuleLibrary.Client;

public class CContentUnlocked_MapClientMessage : CMapClientMessage
{
	public string m_ContentID;

	public string m_ContentType;

	public CUnlockCondition m_ContentUnlockCondition;

	public CContentUnlocked_MapClientMessage(string contentID, string contentType, CUnlockCondition unlockCondition)
		: base(EMapClientMessageType.ContentUnlocked)
	{
		m_ContentID = contentID;
		m_ContentType = contentType;
		m_ContentUnlockCondition = unlockCondition;
	}
}
