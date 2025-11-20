using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityCreate : SEventAbility
{
	public CAbilityCreate.CreateState CreateState { get; private set; }

	public string PropName { get; private set; }

	public int PropsSpawned { get; private set; }

	public SEventAbilityCreate()
	{
	}

	public SEventAbilityCreate(SEventAbilityCreate state, ReferenceDictionary references)
		: base(state, references)
	{
		CreateState = state.CreateState;
		PropName = state.PropName;
		PropsSpawned = state.PropsSpawned;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CreateState", CreateState);
		info.AddValue("PropName", PropName);
		info.AddValue("PropsSpawned", PropsSpawned);
	}

	public SEventAbilityCreate(SerializationInfo info, StreamingContext context)
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
				case "CreateState":
					CreateState = (CAbilityCreate.CreateState)info.GetValue("CreateState", typeof(CAbilityCreate.CreateState));
					break;
				case "PropName":
					PropName = info.GetString("PropName");
					break;
				case "PropsSpawned":
					PropsSpawned = info.GetInt32("PropsSpawned");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityCreate entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityCreate(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CAbilityCreate.CreateState createState, string propName, int propsSpawned, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Create, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		CreateState = createState;
		PropName = propName;
		PropsSpawned = propsSpawned;
	}
}
