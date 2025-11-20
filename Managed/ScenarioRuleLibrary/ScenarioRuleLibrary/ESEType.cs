using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESEType
{
	None,
	Internal,
	Phase,
	Actor,
	Item,
	Action,
	Ability,
	AttackModifier,
	ObjectProp,
	Element,
	Donate,
	Perk,
	Enhancement,
	PersonalQuest,
	Round,
	LoseCard,
	DiscardCard
}
