using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityFear : SEventAbility
{
	public CAbilityFear.EFearState FearState { get; private set; }

	public List<CAbilityFear.FearedActorStats> ActorsDistanceMoved { get; private set; }

	public SEventAbilityFear()
	{
	}

	public SEventAbilityFear(SEventAbilityFear state, ReferenceDictionary references)
		: base(state, references)
	{
		FearState = state.FearState;
		ActorsDistanceMoved = references.Get(state.ActorsDistanceMoved);
		if (ActorsDistanceMoved != null || state.ActorsDistanceMoved == null)
		{
			return;
		}
		ActorsDistanceMoved = new List<CAbilityFear.FearedActorStats>();
		for (int i = 0; i < state.ActorsDistanceMoved.Count; i++)
		{
			CAbilityFear.FearedActorStats fearedActorStats = state.ActorsDistanceMoved[i];
			CAbilityFear.FearedActorStats fearedActorStats2 = references.Get(fearedActorStats);
			if (fearedActorStats2 == null && fearedActorStats != null)
			{
				fearedActorStats2 = new CAbilityFear.FearedActorStats(fearedActorStats, references);
				references.Add(fearedActorStats, fearedActorStats2);
			}
			ActorsDistanceMoved.Add(fearedActorStats2);
		}
		references.Add(state.ActorsDistanceMoved, ActorsDistanceMoved);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("FearState", FearState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityFear(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "FearState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityFear.FearedActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityFear.FearedActorStats>));
					}
				}
				else
				{
					FearState = (CAbilityFear.EFearState)info.GetValue("FearState", typeof(CAbilityFear.EFearState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityPush entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityFear(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityFear.EFearState fearState, List<CAbilityFear.FearedActorStats> actorsDistanceMoved, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Fear, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		FearState = fearState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}
