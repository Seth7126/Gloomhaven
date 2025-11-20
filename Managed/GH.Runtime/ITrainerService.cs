using System.Collections.Generic;
using MapRuleLibrary.Party;

public interface ITrainerService
{
	bool HasShownTrainerTooltip { get; set; }

	List<CPartyAchievement> GetAchievements();

	void ClaimReward(CPartyAchievement achievement);

	void UncheckNewAchievement(CPartyAchievement achievement);
}
