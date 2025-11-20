using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESESubTypeActor
{
	None,
	ActorInitialized,
	ActorOnDeath,
	ActorDamaged,
	ActorCalculateShield,
	ActorApplyRedirector,
	ActorApplyRetaliate,
	ActorHealed,
	ActorGainXP,
	ActorApplyImmediateDamage,
	ActorApplyNegativeCondition,
	ActorApplyPositiveCondition,
	ActorLooted,
	ActorUsedItem,
	ActorFinishedScenario,
	ActorEndTurn
}
