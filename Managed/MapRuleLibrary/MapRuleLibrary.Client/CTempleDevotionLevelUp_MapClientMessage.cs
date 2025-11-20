using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace MapRuleLibrary.Client;

public class CTempleDevotionLevelUp_MapClientMessage : CMapClientMessage
{
	public int m_NewLevel;

	public List<RewardGroup> m_Rewards;

	public string m_StoryLocText;

	public string m_StoryAudioId;

	public CTempleDevotionLevelUp_MapClientMessage(int level, List<RewardGroup> rewards, string storyLocText = null, string storyAudioId = null)
		: base(EMapClientMessageType.TempleDevotionLevelUp)
	{
		m_NewLevel = level;
		m_Rewards = rewards;
		m_StoryLocText = storyLocText;
		m_StoryAudioId = storyAudioId;
	}
}
