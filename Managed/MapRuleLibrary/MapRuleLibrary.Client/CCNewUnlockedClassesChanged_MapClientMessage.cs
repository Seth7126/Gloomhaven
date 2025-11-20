using System.Collections.Generic;

namespace MapRuleLibrary.Client;

public class CCNewUnlockedClassesChanged_MapClientMessage : CMapClientMessage
{
	public List<string> m_NewUnlockedClasses;

	public CCNewUnlockedClassesChanged_MapClientMessage(List<string> newUnlockedCharactersId)
		: base(EMapClientMessageType.NewUnlockedClassesChanged)
	{
		m_NewUnlockedClasses = newUnlockedCharactersId;
	}
}
