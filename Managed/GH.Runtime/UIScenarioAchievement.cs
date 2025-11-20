public class UIScenarioAchievement : UIAchievement
{
	private ScenarioAchievementProgress achievementProgress;

	public void SetAchievement(ScenarioAchievementProgress achievementProgress)
	{
		this.achievementProgress = achievementProgress;
		base.SetAchievement(achievementProgress.achievement);
		if (base.gameObject.activeInHierarchy)
		{
			progressBar.PlayFromProgress(achievementProgress.achievement.AchievementConditionState.CurrentProgress);
		}
	}

	protected override void UpdateProgress()
	{
		progressBar.SetAmount(achievementProgress.achievement.AchievementConditionState.TotalConditionsAndTargets, achievementProgress.achievement.AchievementConditionState.CurrentProgress);
	}

	private void OnEnable()
	{
		if (achievementProgress != null)
		{
			progressBar.PlayFromProgress(achievementProgress.achievement.AchievementConditionState.PreviousProgress);
		}
	}
}
