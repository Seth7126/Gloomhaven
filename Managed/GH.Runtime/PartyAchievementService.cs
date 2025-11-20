using System.Collections.Generic;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;

public class PartyAchievementService : ITrainerService
{
	public bool HasShownTrainerTooltip
	{
		get
		{
			return AdventureState.MapState.MapParty.IntroductionDoneIds.Contains(EIntroductionConcept.Trainer.ToString());
		}
		set
		{
			if (value)
			{
				AdventureState.MapState.MapParty.IntroductionDoneIds.Add(EIntroductionConcept.Trainer.ToString());
			}
			else
			{
				AdventureState.MapState.MapParty.IntroductionDoneIds.Remove(EIntroductionConcept.Trainer.ToString());
			}
			SaveData.Instance.SaveCurrentAdventureData();
		}
	}

	public List<CPartyAchievement> GetAchievements()
	{
		return AdventureState.MapState.MapParty.Achievements.FindAll((CPartyAchievement it) => it.State != EAchievementState.Locked && it.Achievement.AchievementType != EAchievementType.Trophy);
	}

	public void ClaimReward(CPartyAchievement achievement)
	{
		achievement.ClaimRewards();
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void UncheckNewAchievement(CPartyAchievement achievement)
	{
		if (achievement.IsNew)
		{
			achievement.IsNew = false;
		}
	}
}
