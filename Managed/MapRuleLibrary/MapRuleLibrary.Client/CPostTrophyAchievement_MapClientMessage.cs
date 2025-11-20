using System.Collections.Generic;
using System.Linq;

namespace MapRuleLibrary.Client;

public class CPostTrophyAchievement_MapClientMessage : CMapClientMessage
{
	public List<string> m_Achievements;

	public CPostTrophyAchievement_MapClientMessage(List<string> achievements)
		: base(EMapClientMessageType.PostTrophyAchievement)
	{
		m_Achievements = achievements.ToList();
	}
}
