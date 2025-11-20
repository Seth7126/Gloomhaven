using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;

public static class PersonalQuestObjectiveUtils
{
	private const string OBJECTIVE_PROGRESS_FORMAT = "{0} {1}/{2}";

	public static List<string> CalculateObjectives(CUnlockConditionState conditionState)
	{
		CUnlockCondition unlockCondition = conditionState.UnlockCondition;
		List<string> result = null;
		if (unlockCondition.Targets.Count > 0)
		{
			result = CalculateTargetsRequired(conditionState);
		}
		else if (unlockCondition.RequiredConditions.Exists((Tuple<EUnlockConditionType, string> it) => it.Item1 == EUnlockConditionType.CompletedQuest))
		{
			result = CalculateCompletedQuests(conditionState);
		}
		else if (unlockCondition.RequiredChoiceContainer != null && unlockCondition.RequiredChoiceContainer.Exists((CUnlockChoiceContainer it) => it.RequiredConditions.Exists((Tuple<EUnlockConditionType, string> tuple) => tuple.Item1 == EUnlockConditionType.CompletedQuest)))
		{
			result = CalculateProgressAreas(conditionState);
		}
		return result;
	}

	private static List<string> CalculateTargetsRequired(CUnlockConditionState condition)
	{
		List<CUnlockConditionState.CUnlockConditionTargetState> list = condition.UnlockConditionTargetStates.FindAll((CUnlockConditionState.CUnlockConditionTargetState it) => it.UnlockConditionTarget.Filter == EUnlockConditionTargetFilter.Kill && !it.UnlockConditionTarget.TargetEnemyClassIDs.IsNullOrEmpty());
		if (list.Count > 1)
		{
			List<string> list2 = new List<string>();
			if (condition.UnlockCondition.TargetsRequired > 0)
			{
				foreach (CUnlockConditionState.CUnlockConditionTargetState item in list.Where((CUnlockConditionState.CUnlockConditionTargetState it) => it.CompletedValue >= it.UnlockConditionTarget.Value))
				{
					CMonsterClass cMonsterClass = MonsterClassManager.Find(item.UnlockConditionTarget.TargetEnemyClassIDs[0]);
					list2.Add(LocalizationManager.GetTranslation(cMonsterClass.LocKey));
				}
			}
			else
			{
				foreach (CUnlockConditionState.CUnlockConditionTargetState item2 in list)
				{
					CMonsterClass cMonsterClass2 = MonsterClassManager.Find(item2.UnlockConditionTarget.TargetEnemyClassIDs[0]);
					list2.Add(FormatObjectiveProgress(LocalizationManager.GetTranslation(cMonsterClass2.LocKey), item2.CompletedValue, item2.UnlockConditionTarget.Value));
				}
			}
			return list2;
		}
		List<CUnlockConditionState.CUnlockConditionTargetState> list3 = condition.UnlockConditionTargetStates.FindAll((CUnlockConditionState.CUnlockConditionTargetState it) => it.UnlockConditionTarget.Filter == EUnlockConditionTargetFilter.OwnItem);
		if (list3.Count > 0)
		{
			List<string> list4 = new List<string>();
			{
				foreach (CUnlockConditionState.CUnlockConditionTargetState item3 in list3)
				{
					string arg = ((item3.CompletedValue >= item3.UnlockConditionTarget.Value) ? UIInfoTools.Instance.mainColor.ToHex() : UIInfoTools.Instance.greyedOutTextColor.ToHex());
					list4.Add(FormatObjectiveProgress(string.Format("<sprite name=\"{1}\" color=#{2}> {0}", LocalizationManager.GetTranslation("GUI_ITEM_SLOT_" + item3.UnlockConditionTarget.Slot[0]), "Inv" + item3.UnlockConditionTarget.Slot[0], arg), item3.CompletedValue, item3.UnlockConditionTarget.Value));
				}
				return list4;
			}
		}
		return null;
	}

	private static List<string> CalculateCompletedQuests(CUnlockConditionState condition)
	{
		List<string> list = (from it in condition.UnlockCondition.RequiredConditions
			where it.Item1 == EUnlockConditionType.CompletedQuest
			select it.Item2).ToList();
		if (list.Count <= 1)
		{
			return null;
		}
		List<string> list2 = new List<string>();
		Tuple<EUnlockConditionType, int> tuple = condition.UnlockCondition.RequiredConditionsTotal.FirstOrDefault((Tuple<EUnlockConditionType, int> it) => it.Item1 == EUnlockConditionType.CompletedQuest);
		foreach (string questId in list)
		{
			CQuestState cQuestState = AdventureState.MapState.AllQuests.First((CQuestState it) => it.ID == questId);
			if ((cQuestState.QuestState == CQuestState.EQuestState.Completed || cQuestState.QuestState == CQuestState.EQuestState.InProgressCasual) && cQuestState.CompleteCharacterNames.Contains(condition.OverrideCharacterName))
			{
				list2.Add(LocalizationManager.GetTranslation(cQuestState.Quest.LocalisedNameKey));
				if (tuple != null && list2.Count == tuple.Item2)
				{
					return list2;
				}
			}
		}
		return list2;
	}

	private static List<string> CalculateProgressAreas(CUnlockConditionState condition)
	{
		if (condition.UnlockCondition.RequiredChoiceContainer.IsNullOrEmpty())
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (CUnlockChoiceContainer item in condition.UnlockCondition.RequiredChoiceContainer)
		{
			int num = item.RequiredConditionsTotal.FindIndex((Tuple<EUnlockConditionType, int> it) => it.Item1 == EUnlockConditionType.CompletedQuest);
			if (num >= 0)
			{
				int current2 = Math.Min(item.RequiredConditionsTotal[num].Item2, item.RequiredConditionsTotalMet[num + 1]);
				list.Add(FormatObjectiveProgress(LocalizationManager.GetTranslation(item.Description.Substring(item.Description.LastIndexOf("_") + 1)), current2, item.RequiredConditionsTotal[num].Item2));
			}
		}
		return list;
	}

	private static string FormatObjectiveProgress(string objective, int current, int total)
	{
		bool flag = current >= total;
		string text = (flag ? UIInfoTools.Instance.mainColor.ToHex() : UIInfoTools.Instance.greyedOutTextColor.ToHex());
		string text2 = string.Format("{0} {1}/{2}", objective, current, total, text);
		if (!flag)
		{
			text2 = string.Format("<color=#{1}>{0}", text2, text);
		}
		return text2;
	}
}
