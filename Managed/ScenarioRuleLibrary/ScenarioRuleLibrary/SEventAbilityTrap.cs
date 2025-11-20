using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTrap : SEventAbility
{
	public CAbilityTrap.TrapState TrapState { get; private set; }

	public string TrapName { get; private set; }

	public int TrapTriggeredXP { get; private set; }

	public SEventAbilityTrap()
	{
	}

	public SEventAbilityTrap(SEventAbilityTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		TrapState = state.TrapState;
		TrapName = state.TrapName;
		TrapTriggeredXP = state.TrapTriggeredXP;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TrapState", TrapState);
		info.AddValue("TrapName", TrapName);
		info.AddValue("TrapTriggeredXP", TrapTriggeredXP);
	}

	public SEventAbilityTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "TrapState":
					TrapState = (CAbilityTrap.TrapState)info.GetValue("TrapState", typeof(CAbilityTrap.TrapState));
					break;
				case "TrapName":
					TrapName = info.GetString("TrapName");
					break;
				case "TrapTriggeredXP":
					TrapTriggeredXP = info.GetInt32("TrapTriggeredXP");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityTrap entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTrap(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityTrap.TrapState trapState, string trapName, int strength, int trapTriggeredXP, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Trap, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		TrapState = trapState;
		TrapName = trapName;
		TrapTriggeredXP = trapTriggeredXP;
	}
}
