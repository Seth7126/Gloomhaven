using System;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public enum EMapPhaseType
{
	None,
	Default,
	InHQ,
	Moving,
	RoadEvent,
	AtScenario,
	InScenario,
	ScenarioComplete,
	QuestComplete,
	ShowMapMessages,
	InScenarioDebugMode,
	AtLinkedScenario
}
