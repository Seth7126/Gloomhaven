using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESESubTypeAction
{
	None,
	ActionInitialized,
	ActionPerform,
	ActionUndoItemAbility,
	ActionNextStep,
	ActionEndAbilitySynchronise,
	ActionOnTileSelected,
	ActionOnStepComplete,
	ActionInjectAbility,
	ActionStackItemAbilities,
	ActionStackInlineSubAbilities,
	ActionStackNextAbilities,
	ActionUnstackItemAbilities,
	ActionAddConsumeAbilities,
	ActionOnFirstAbilityStarted,
	ActionUpdateItemsUsedInPhase,
	ActionRemoveConsumeAbilities,
	ActionOnTileDeselected,
	ActionOnApplySingleTarget
}
