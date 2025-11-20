using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityCondition : SEventAbility
{
	public EConditionDecTrigger ConditionDecrementTrigger { get; private set; }

	public int Duration { get; set; }

	public SEventAbilityCondition()
	{
	}

	public SEventAbilityCondition(SEventAbilityCondition state, ReferenceDictionary references)
		: base(state, references)
	{
		ConditionDecrementTrigger = state.ConditionDecrementTrigger;
		Duration = state.Duration;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ConditionDecrementTrigger", ConditionDecrementTrigger);
		info.AddValue("Duration", Duration);
	}

	public SEventAbilityCondition(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "ConditionDecrementTrigger"))
				{
					if (name == "Duration")
					{
						Duration = info.GetInt32("Duration");
					}
				}
				else
				{
					ConditionDecrementTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityCondition entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityCondition(CAbility.EAbilityType conditionType, ESESubTypeAbility abilitySubType, EConditionDecTrigger conditionDecrementTrigger, int duration, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(conditionType, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		ConditionDecrementTrigger = conditionDecrementTrigger;
		Duration = duration;
	}
}
