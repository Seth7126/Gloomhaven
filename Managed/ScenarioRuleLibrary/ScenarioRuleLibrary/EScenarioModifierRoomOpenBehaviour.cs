using System;

namespace ScenarioRuleLibrary;

[Serializable]
[Flags]
public enum EScenarioModifierRoomOpenBehaviour
{
	None = 0,
	TriggerOnOpen = 1,
	ActivateOnOpen = 2
}
