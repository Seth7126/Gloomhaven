using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;

public class GuildmasterQuestLogService : IQuestLogService
{
	public enum EGuildmasterLogGroup
	{
		Travel,
		Job,
		Story,
		Relic,
		Completed
	}

	public List<QuestLogGroup> GetGroups()
	{
		return (from EGuildmasterLogGroup it in Enum.GetValues(typeof(EGuildmasterLogGroup))
			select new QuestLogGroup(it, IsGroupExpanded(it), IsNewNotificationEnabled(it), null, (it == EGuildmasterLogGroup.Completed) ? new Action<bool>(OnExpandedCompletedLog) : null)).ToList();
	}

	public Enum GetGroup(CQuestState quest)
	{
		return (quest.QuestState >= CQuestState.EQuestState.Completed) ? EGuildmasterLogGroup.Completed : QuestTypeToLogGroup(quest.Quest.Type);
	}

	private bool IsGroupExpanded(Enum group)
	{
		return !group.Equals(EGuildmasterLogGroup.Completed);
	}

	private bool IsNewNotificationEnabled(Enum group)
	{
		return !group.Equals(EGuildmasterLogGroup.Completed);
	}

	public EGuildmasterLogGroup QuestTypeToLogGroup(EQuestType questType)
	{
		return questType switch
		{
			EQuestType.Job => EGuildmasterLogGroup.Job, 
			EQuestType.Relic => EGuildmasterLogGroup.Relic, 
			EQuestType.Story => EGuildmasterLogGroup.Story, 
			EQuestType.Travel => EGuildmasterLogGroup.Travel, 
			_ => EGuildmasterLogGroup.Completed, 
		};
	}

	private void OnExpandedCompletedLog(bool expanded)
	{
		Singleton<QuestManager>.Instance.HideCompletedQuests(!expanded);
	}
}
