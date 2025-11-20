using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDisarmTrap : SEventAbility
{
	public CAbilityDisarmTrap.DisarmTrapState DisarmTrapState { get; private set; }

	public Dictionary<string, int> TrapsDisarmedDictionary { get; private set; }

	public SEventAbilityDisarmTrap()
	{
	}

	public SEventAbilityDisarmTrap(SEventAbilityDisarmTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		DisarmTrapState = state.DisarmTrapState;
		TrapsDisarmedDictionary = references.Get(state.TrapsDisarmedDictionary);
		if (TrapsDisarmedDictionary != null || state.TrapsDisarmedDictionary == null)
		{
			return;
		}
		TrapsDisarmedDictionary = new Dictionary<string, int>(state.TrapsDisarmedDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.TrapsDisarmedDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			TrapsDisarmedDictionary.Add(key, value);
		}
		references.Add(state.TrapsDisarmedDictionary, TrapsDisarmedDictionary);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DisarmTrapState", DisarmTrapState);
		info.AddValue("TrapsDisarmedDictionary", TrapsDisarmedDictionary);
	}

	public SEventAbilityDisarmTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "DisarmTrapState"))
				{
					if (name == "TrapsDisarmedDictionary")
					{
						TrapsDisarmedDictionary = (Dictionary<string, int>)info.GetValue("TrapsDisarmedDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					DisarmTrapState = (CAbilityDisarmTrap.DisarmTrapState)info.GetValue("DisarmTrapState", typeof(CAbilityDisarmTrap.DisarmTrapState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityDisarmTrap entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDisarmTrap(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityDisarmTrap.DisarmTrapState disarmTrapState, Dictionary<string, int> trapsDisarmedDictionary, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.DisarmTrap, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		DisarmTrapState = disarmTrapState;
		TrapsDisarmedDictionary = trapsDisarmedDictionary;
	}
}
