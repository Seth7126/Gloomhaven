using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESESubTypePhase
{
	None,
	PhaseStart,
	PhaseNextStep,
	PhaseStepComplete,
	PhaseEnd,
	PhaseTileSelected,
	PhaseTileDeselected,
	PhaseApplySingleTarget
}
