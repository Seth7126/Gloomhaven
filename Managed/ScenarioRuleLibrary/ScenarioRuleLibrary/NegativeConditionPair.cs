using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{NegativeCondition}")]
public class NegativeConditionPair : ISerializable
{
	public CCondition.ENegativeCondition NegativeCondition;

	public RewardCondition.EConditionMapDuration MapDuration;

	public EConditionDecTrigger ConditionDecTrigger;

	public int RoundDuration;

	public NegativeConditionPair()
	{
	}

	public NegativeConditionPair(NegativeConditionPair state, ReferenceDictionary references)
	{
		NegativeCondition = state.NegativeCondition;
		MapDuration = state.MapDuration;
		ConditionDecTrigger = state.ConditionDecTrigger;
		RoundDuration = state.RoundDuration;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("NegativeCondition", NegativeCondition);
		info.AddValue("MapDuration", MapDuration);
		info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		info.AddValue("RoundDuration", RoundDuration);
	}

	private NegativeConditionPair(SerializationInfo info, StreamingContext context)
	{
		try
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.Name)
				{
				case "NegativeCondition":
					NegativeCondition = (CCondition.ENegativeCondition)info.GetValue("NegativeCondition", typeof(CCondition.ENegativeCondition));
					break;
				case "MapDuration":
					MapDuration = (RewardCondition.EConditionMapDuration)info.GetValue("MapDuration", typeof(int));
					break;
				case "ConditionDecrementTrigger":
					ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
					break;
				case "RoundDuration":
					RoundDuration = info.GetInt32("RoundDuration");
					break;
				case "ConditionDecTrigger":
					ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger));
					if (ConditionDecTrigger == EConditionDecTrigger.Rounds)
					{
						ConditionDecTrigger = EConditionDecTrigger.Turns;
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Unable to deserialize NegativeConditionPair.cs.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public NegativeConditionPair(CCondition.ENegativeCondition negCondition, RewardCondition.EConditionMapDuration mapDuration, int roundDuration, EConditionDecTrigger decTrigger)
	{
		NegativeCondition = negCondition;
		MapDuration = mapDuration;
		RoundDuration = roundDuration;
		ConditionDecTrigger = decTrigger;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is NegativeConditionPair negativeConditionPair)
		{
			if (NegativeCondition == negativeConditionPair.NegativeCondition && ConditionDecTrigger == negativeConditionPair.ConditionDecTrigger && MapDuration == negativeConditionPair.MapDuration)
			{
				return RoundDuration == negativeConditionPair.RoundDuration;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "NegativeCondition: " + NegativeCondition.ToString() + ", ConditionDecTrigger: " + ConditionDecTrigger.ToString() + ", MapDuration: " + MapDuration.ToString() + ", RoundDuration: " + RoundDuration;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
