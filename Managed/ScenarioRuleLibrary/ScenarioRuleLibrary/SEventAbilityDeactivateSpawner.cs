using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDeactivateSpawner : SEventAbility
{
	public CAbilityDeactivateSpawner.EDeactivateSpawnerState DeactivateSpawnerState { get; private set; }

	public SEventAbilityDeactivateSpawner()
	{
	}

	public SEventAbilityDeactivateSpawner(SEventAbilityDeactivateSpawner state, ReferenceDictionary references)
		: base(state, references)
	{
		DeactivateSpawnerState = state.DeactivateSpawnerState;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DeactivateSpawnerState", DeactivateSpawnerState);
	}

	public SEventAbilityDeactivateSpawner(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "DeactivateSpawnerState")
				{
					DeactivateSpawnerState = (CAbilityDeactivateSpawner.EDeactivateSpawnerState)info.GetValue("DeactivateSpawnerState", typeof(CAbilityDeactivateSpawner.EDeactivateSpawnerState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityDeactivateSpawner entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDeactivateSpawner(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityDeactivateSpawner.EDeactivateSpawnerState deactivateSpawnerState, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.DeactivateSpawner, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		DeactivateSpawnerState = deactivateSpawnerState;
	}
}
