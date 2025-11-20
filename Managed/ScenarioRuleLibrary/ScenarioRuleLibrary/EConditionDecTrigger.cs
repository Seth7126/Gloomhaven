using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EConditionDecTrigger
{
	None,
	Abilities,
	Actions,
	Turns,
	Rounds,
	Never,
	ConditionalCondition
}
