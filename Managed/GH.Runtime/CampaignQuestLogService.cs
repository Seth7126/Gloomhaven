using System;
using System.Collections.Generic;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;

public class CampaignQuestLogService : IQuestLogService
{
	public enum ECampaignLogGroup
	{
		City,
		World,
		Completed
	}

	public List<QuestLogGroup> GetGroups()
	{
		return new List<QuestLogGroup>
		{
			new QuestLogGroup(ECampaignLogGroup.City, isExpanded: true, isNewNotificationEnabled: true, HighlightCity),
			new QuestLogGroup(ECampaignLogGroup.World, isExpanded: true, isNewNotificationEnabled: true),
			new QuestLogGroup(ECampaignLogGroup.Completed, isExpanded: false, isNewNotificationEnabled: false, null, OnExpandedCompletedLog)
		};
	}

	public Enum GetGroup(CQuestState quest)
	{
		return (quest.QuestState >= CQuestState.EQuestState.Completed) ? ECampaignLogGroup.Completed : ((quest.Quest.Type != EQuestType.City) ? ECampaignLogGroup.World : ECampaignLogGroup.City);
	}

	private void HighlightCity(bool highlight)
	{
		Singleton<MapChoreographer>.Instance.HeadquartersLocation.ForceHighlight(highlight);
	}

	private void OnExpandedCompletedLog(bool expanded)
	{
		Singleton<QuestManager>.Instance.HideCompletedQuests(!expanded);
	}
}
