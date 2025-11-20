using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class RewardCondition
{
	[Serializable]
	public enum EConditionMapDuration
	{
		None,
		NextScenario,
		NextVillage,
		Now,
		Infinite
	}

	[Serializable]
	public enum EConditionType
	{
		None,
		Negative,
		Positive
	}

	public static EConditionMapDuration[] ConditionDurations = (EConditionMapDuration[])Enum.GetValues(typeof(EConditionMapDuration));

	public static EConditionType[] ConditionTypes = (EConditionType[])Enum.GetValues(typeof(EConditionType));

	public Guid ID;

	public EConditionMapDuration MapDuration;

	public int RoundDuration;

	public EConditionType Type;

	public CCondition.ENegativeCondition NegativeCondition;

	public CCondition.EPositiveCondition PositiveCondition;

	public RewardCondition(EConditionMapDuration mapDuration, CCondition.ENegativeCondition negCondition, int roundDuration)
	{
		ID = Guid.NewGuid();
		MapDuration = mapDuration;
		RoundDuration = roundDuration;
		NegativeCondition = negCondition;
		PositiveCondition = CCondition.EPositiveCondition.NA;
		Type = EConditionType.Negative;
	}

	public RewardCondition(EConditionMapDuration mapDuration, CCondition.EPositiveCondition posCondition, int roundDuration)
	{
		ID = Guid.NewGuid();
		MapDuration = mapDuration;
		RoundDuration = roundDuration;
		NegativeCondition = CCondition.ENegativeCondition.NA;
		PositiveCondition = posCondition;
		Type = EConditionType.Positive;
	}

	public override string ToString()
	{
		if (Type == EConditionType.Negative)
		{
			return NegativeCondition.ToString();
		}
		if (Type == EConditionType.Positive)
		{
			return PositiveCondition.ToString();
		}
		return ID.ToString();
	}

	public RewardCondition()
	{
	}

	public RewardCondition(RewardCondition state, ReferenceDictionary references)
	{
		ID = state.ID;
		MapDuration = state.MapDuration;
		RoundDuration = state.RoundDuration;
		Type = state.Type;
		NegativeCondition = state.NegativeCondition;
		PositiveCondition = state.PositiveCondition;
	}
}
