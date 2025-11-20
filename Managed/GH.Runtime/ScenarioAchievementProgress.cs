using MapRuleLibrary.Party;

public class ScenarioAchievementProgress
{
	public CPartyAchievement achievement;

	public ScenarioAchievementProgress(CPartyAchievement achievement)
	{
		this.achievement = achievement;
	}

	public bool HasProgressed()
	{
		return achievement.AchievementConditionState.HasProgressed;
	}
}
