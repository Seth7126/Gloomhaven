using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilitySummon : SEventAbility
{
	public CAbilitySummon.SummonState SummonState { get; private set; }

	public string SummonName { get; private set; }

	public SEventAbilitySummon()
	{
	}

	public SEventAbilitySummon(SEventAbilitySummon state, ReferenceDictionary references)
		: base(state, references)
	{
		SummonState = state.SummonState;
		SummonName = state.SummonName;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SummonState", SummonState);
		info.AddValue("SummonName", SummonName);
	}

	public SEventAbilitySummon(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "SummonState"))
				{
					if (name == "SummonName")
					{
						SummonName = info.GetString("SummonName");
					}
				}
				else
				{
					SummonState = (CAbilitySummon.SummonState)info.GetValue("SummonState", typeof(CAbilitySummon.SummonState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilitySummon entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilitySummon(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilitySummon.SummonState summonState, string summonName, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Summon, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		SummonState = summonState;
		SummonName = summonName;
	}
}
