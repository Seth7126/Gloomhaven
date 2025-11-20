using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityRedistributeDamage : SEventAbility
{
	public CAbilityRedistributeDamage.ERedistributeState RedistributeState { get; private set; }

	public SEventAbilityRedistributeDamage()
	{
	}

	public SEventAbilityRedistributeDamage(SEventAbilityRedistributeDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		RedistributeState = state.RedistributeState;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SwapState", RedistributeState);
	}

	public SEventAbilityRedistributeDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Name == "RedistributeState")
			{
				RedistributeState = (CAbilityRedistributeDamage.ERedistributeState)info.GetValue("RedistributeState", typeof(CAbilityRedistributeDamage.ERedistributeState));
			}
		}
	}

	public SEventAbilityRedistributeDamage(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityRedistributeDamage.ERedistributeState redistributeState, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.RedistributeDamage, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		RedistributeState = redistributeState;
	}
}
