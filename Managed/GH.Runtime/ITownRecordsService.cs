using System.Collections.Generic;
using MapRuleLibrary.Party;

public interface ITownRecordsService
{
	List<ITownRecordCharacter> GetCharacters();

	List<DialogLineDTO> GetStory();

	List<CPartyAchievement> GetAchievementsToClaim();

	void ClaimAchievement(CPartyAchievement achievement);
}
