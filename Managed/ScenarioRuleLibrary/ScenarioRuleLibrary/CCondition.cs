using System;

namespace ScenarioRuleLibrary;

public class CCondition
{
	[Serializable]
	public enum ENegativeCondition
	{
		NA = 0,
		Poison = 1,
		Wound = 2,
		Immobilize = 4,
		Disarm = 8,
		Stun = 0x10,
		Muddle = 0x20,
		Curse = 0x40,
		Disadvantage = 0x80,
		StopFlying = 0x100,
		Sleep = 0x200
	}

	[Serializable]
	public enum EPositiveCondition
	{
		NA = 0,
		Invisible = 1,
		Strengthen = 2,
		Bless = 4,
		Advantage = 8,
		Immovable = 0x10
	}

	public static readonly ENegativeCondition[] NegativeConditions = (ENegativeCondition[])Enum.GetValues(typeof(ENegativeCondition));

	public static readonly EPositiveCondition[] PositiveConditions = (EPositiveCondition[])Enum.GetValues(typeof(EPositiveCondition));
}
