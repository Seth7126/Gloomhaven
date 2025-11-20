using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTargeting : SEventAbility
{
	public CAbilityTargeting.TargetingState TargetingState { get; private set; }

	public int Amount { get; private set; }

	public SEventAbilityTargeting()
	{
	}

	public SEventAbilityTargeting(SEventAbilityTargeting state, ReferenceDictionary references)
		: base(state, references)
	{
		TargetingState = state.TargetingState;
		Amount = state.Amount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetingState", TargetingState);
		info.AddValue("Amount", Amount);
	}

	public SEventAbilityTargeting(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "TargetingState"))
				{
					if (name == "Amount")
					{
						Amount = info.GetInt32("Amount");
					}
				}
				else
				{
					TargetingState = (CAbilityTargeting.TargetingState)info.GetValue("TargetingState", typeof(CAbilityTargeting.TargetingState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityTargeting entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTargeting(CAbility.EAbilityType abilityType, ESESubTypeAbility abilitySubType, CAbilityTargeting.TargetingState targetingState, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "", int amount = 0)
		: base(abilityType, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		TargetingState = targetingState;
		Amount = amount;
	}
}
