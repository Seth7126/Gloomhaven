namespace MapRuleLibrary.Client;

public class CAchievementCompleted_MapClientMessage : CMapClientMessage
{
	public string m_AchievementID;

	public CAchievementCompleted_MapClientMessage(string achievementID)
		: base(EMapClientMessageType.AchievementCompleted)
	{
		m_AchievementID = achievementID;
	}
}
