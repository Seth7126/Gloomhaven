using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EScenarioModifierTriggerPhase
{
	None,
	StartRound,
	EndRound,
	StartTurn,
	EndTurn,
	StartScenario,
	AfterAbility,
	OnActorSpawned
}
