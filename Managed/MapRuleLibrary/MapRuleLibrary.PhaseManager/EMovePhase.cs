using System;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public enum EMovePhase
{
	None,
	BeforeRoadEvent,
	AtRoadEvent,
	AfterRoadEvent
}
