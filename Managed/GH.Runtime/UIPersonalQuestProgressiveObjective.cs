using MapRuleLibrary.Party;
using UnityEngine;

public class UIPersonalQuestProgressiveObjective : UIPersonalQuestStateInfo
{
	[SerializeField]
	protected TextLocalizedListener title;

	[SerializeField]
	protected string format = "{CURRENT}/{MAX} {0}";

	public override void SetPersonalQuest(CPersonalQuestState personalQuest)
	{
		SetPersonalQuestStep(personalQuest, personalQuest.CurrentPersonalQuestStep);
	}

	public void SetPersonalQuestStep(CPersonalQuestState personalQuest, int step)
	{
		if (personalQuest.PersonalQuestSteps == 0)
		{
			title.SetFormat("{0}");
		}
		else
		{
			title.SetFormat(format.ReplaceLastOccurrence("{CURRENT}", (step + 1).ToString()).ReplaceLastOccurrence("{MAX}", personalQuest.PersonalQuestSteps.ToString()));
		}
	}
}
