using System;
using System.Collections.Generic;
using MapRuleLibrary.MapState;

public interface IQuestLogService
{
	List<QuestLogGroup> GetGroups();

	Enum GetGroup(CQuestState quest);
}
