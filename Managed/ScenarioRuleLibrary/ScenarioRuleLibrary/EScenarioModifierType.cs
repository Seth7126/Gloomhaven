using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EScenarioModifierType
{
	None,
	SetElements,
	TriggerAbility,
	AddConditionsToAbilities,
	ApplyConditionToActor,
	ApplyActiveBonusToActor,
	AddModifierCards,
	PhaseOut,
	PhaseInAndTeleport,
	Teleport,
	ActivateClosestAI,
	ToggleActorDeactivated,
	MoveActorsInDirection,
	ForceSpawnerToSpawnIfActorsNotInRooms,
	MovePropsInSequence,
	MovePropsToNearestPlayer,
	DestroyRoom,
	ActorsCreateGraves,
	OverrideCompanionSummonTiles
}
