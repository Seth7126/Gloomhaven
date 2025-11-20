using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESESubTypeAbility
{
	None,
	AbilityStart,
	AbilityPerform,
	AbilityPerformImmediate,
	AbilityTileSelected,
	AbilityComplete,
	AbilityEnded,
	AbilityReset,
	AbilityRestart,
	AbilityApplySingleTargetItem,
	AbilityOverrideAbilityValues,
	AbilityUndoOverride,
	ApplyToActor,
	ActorIsApplying,
	AbilityApplySingleTargetActiveBonus,
	AbilityTileDeselected
}
