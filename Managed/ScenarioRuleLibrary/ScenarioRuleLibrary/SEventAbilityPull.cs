using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityPull : SEventAbility
{
	public CAbilityPull.EPullState PullState { get; private set; }

	public List<CAbilityPull.PulledActorStats> ActorsDistanceMoved { get; private set; }

	public SEventAbilityPull()
	{
	}

	public SEventAbilityPull(SEventAbilityPull state, ReferenceDictionary references)
		: base(state, references)
	{
		PullState = state.PullState;
		ActorsDistanceMoved = references.Get(state.ActorsDistanceMoved);
		if (ActorsDistanceMoved != null || state.ActorsDistanceMoved == null)
		{
			return;
		}
		ActorsDistanceMoved = new List<CAbilityPull.PulledActorStats>();
		for (int i = 0; i < state.ActorsDistanceMoved.Count; i++)
		{
			CAbilityPull.PulledActorStats pulledActorStats = state.ActorsDistanceMoved[i];
			CAbilityPull.PulledActorStats pulledActorStats2 = references.Get(pulledActorStats);
			if (pulledActorStats2 == null && pulledActorStats != null)
			{
				pulledActorStats2 = new CAbilityPull.PulledActorStats(pulledActorStats, references);
				references.Add(pulledActorStats, pulledActorStats2);
			}
			ActorsDistanceMoved.Add(pulledActorStats2);
		}
		references.Add(state.ActorsDistanceMoved, ActorsDistanceMoved);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PullState", PullState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityPull(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "PullState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityPull.PulledActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityPull.PulledActorStats>));
					}
				}
				else
				{
					PullState = (CAbilityPull.EPullState)info.GetValue("PullState", typeof(CAbilityPull.EPullState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityPull entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityPull(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityPull.EPullState pullState, List<CAbilityPull.PulledActorStats> actorsDistanceMoved, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Pull, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		PullState = pullState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}
