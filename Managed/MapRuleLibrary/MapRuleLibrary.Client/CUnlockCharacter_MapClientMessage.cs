using MapRuleLibrary.Party;

namespace MapRuleLibrary.Client;

public class CUnlockCharacter_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_UnlockedCharacter;

	public CUnlockCharacter_MapClientMessage(CMapCharacter unlockedCharacter)
		: base(EMapClientMessageType.UnlockCharacter)
	{
		m_UnlockedCharacter = unlockedCharacter;
	}
}
