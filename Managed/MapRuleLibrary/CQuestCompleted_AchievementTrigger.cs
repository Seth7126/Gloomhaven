public class CQuestCompleted_AchievementTrigger : CAchievementTrigger
{
	public readonly string QuestId;

	public CQuestCompleted_AchievementTrigger(string questId)
		: base(EAchievementTriggerType.QuestCompleted)
	{
		QuestId = questId;
	}
}
