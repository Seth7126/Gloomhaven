using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityPush : SEventAbility
{
	public CAbilityPush.EPushState PushState { get; private set; }

	public List<CAbilityPush.PushedActorStats> ActorsDistanceMoved { get; private set; }

	public SEventAbilityPush()
	{
	}

	public SEventAbilityPush(SEventAbilityPush state, ReferenceDictionary references)
		: base(state, references)
	{
		PushState = state.PushState;
		ActorsDistanceMoved = references.Get(state.ActorsDistanceMoved);
		if (ActorsDistanceMoved != null || state.ActorsDistanceMoved == null)
		{
			return;
		}
		ActorsDistanceMoved = new List<CAbilityPush.PushedActorStats>();
		for (int i = 0; i < state.ActorsDistanceMoved.Count; i++)
		{
			CAbilityPush.PushedActorStats pushedActorStats = state.ActorsDistanceMoved[i];
			CAbilityPush.PushedActorStats pushedActorStats2 = references.Get(pushedActorStats);
			if (pushedActorStats2 == null && pushedActorStats != null)
			{
				pushedActorStats2 = new CAbilityPush.PushedActorStats(pushedActorStats, references);
				references.Add(pushedActorStats, pushedActorStats2);
			}
			ActorsDistanceMoved.Add(pushedActorStats2);
		}
		references.Add(state.ActorsDistanceMoved, ActorsDistanceMoved);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PushState", PushState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityPush(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "PushState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityPush.PushedActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityPush.PushedActorStats>));
					}
				}
				else
				{
					PushState = (CAbilityPush.EPushState)info.GetValue("PushState", typeof(CAbilityPush.EPushState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityPush entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityPush(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityPush.EPushState pushState, List<CAbilityPush.PushedActorStats> actorsDistanceMoved, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Push, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		PushState = pushState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}
