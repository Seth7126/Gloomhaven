using System;

namespace MapRuleLibrary.YML.Shared;

[Serializable]
public enum EUnlockConditionTargetSubFilter
{
	None,
	Round,
	Scenario,
	Total,
	RoundNoReset,
	SingleScenario
}
