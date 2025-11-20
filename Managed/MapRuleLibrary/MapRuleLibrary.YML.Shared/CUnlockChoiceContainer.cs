using System;
using System.Collections.Generic;

namespace MapRuleLibrary.YML.Shared;

[Serializable]
public class CUnlockChoiceContainer
{
	public List<Tuple<EUnlockConditionType, string>> RequiredConditions { get; private set; }

	public List<Tuple<EUnlockConditionType, int>> RequiredConditionsTotal { get; private set; }

	public List<int> RequiredConditionsTotalMet { get; private set; }

	public bool OROperator { get; private set; }

	public string Description { get; private set; }

	public CUnlockChoiceContainer(List<Tuple<EUnlockConditionType, string>> requiredConditions, List<Tuple<EUnlockConditionType, int>> requiredConditionsTotal, bool orOperator, string description)
	{
		RequiredConditions = requiredConditions;
		RequiredConditionsTotal = ((requiredConditionsTotal != null) ? requiredConditionsTotal : new List<Tuple<EUnlockConditionType, int>>());
		RequiredConditionsTotalMet = new List<int>();
		RequiredConditionsTotalMet.Add(0);
		foreach (Tuple<EUnlockConditionType, int> item in RequiredConditionsTotal)
		{
			_ = item;
			RequiredConditionsTotalMet.Add(0);
		}
		OROperator = orOperator;
		Description = description;
	}
}
