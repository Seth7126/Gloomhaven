using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.PersonalQuests;
using ScenarioRuleLibrary.YML;

public class PersonalQuestDTO
{
	public int Step { get; }

	public List<Reward> Rewards { get; }

	public int CurrentProgress { get; }

	public int TotalProgress { get; }

	public string LocalizationProgressStory { get; }

	public string AudioIdProgressStory { get; }

	public PersonalQuestYMLData Data { get; }

	public PersonalQuestDTO(CPersonalQuestState personalQuest)
	{
		Step = personalQuest.CurrentPersonalQuestStep;
		Rewards = personalQuest.CurrentRewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
		CurrentProgress = personalQuest.PersonalQuestConditionState.CurrentProgress;
		TotalProgress = personalQuest.PersonalQuestConditionState.TotalConditionsAndTargets;
		LocalizationProgressStory = personalQuest.LocalisedCompletedProgressStepStory;
		Data = personalQuest.CurrentPersonalQuestStepData;
		AudioIdProgressStory = personalQuest.AudioIdCompletedProgressStepStory;
	}
}
