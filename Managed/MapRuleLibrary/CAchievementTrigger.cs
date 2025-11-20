public abstract class CAchievementTrigger
{
	public enum EAchievementTriggerType
	{
		None,
		QuestCompleted,
		CharacterRetired,
		EnhancementAdded,
		LevelUp,
		UnlockClass,
		LootChest,
		ScenarioEnded
	}

	public EAchievementTriggerType Type { get; }

	protected CAchievementTrigger(EAchievementTriggerType type)
	{
		Type = type;
	}
}
