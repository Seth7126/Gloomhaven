using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDestroyObstacle : SEventAbility
{
	public CAbilityDestroyObstacle.DestroyObstacleState DestroyObstacleState { get; private set; }

	public Dictionary<string, int> DestroyedPropsDictionary { get; private set; }

	public SEventAbilityDestroyObstacle()
	{
	}

	public SEventAbilityDestroyObstacle(SEventAbilityDestroyObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		DestroyObstacleState = state.DestroyObstacleState;
		DestroyedPropsDictionary = references.Get(state.DestroyedPropsDictionary);
		if (DestroyedPropsDictionary != null || state.DestroyedPropsDictionary == null)
		{
			return;
		}
		DestroyedPropsDictionary = new Dictionary<string, int>(state.DestroyedPropsDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.DestroyedPropsDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			DestroyedPropsDictionary.Add(key, value);
		}
		references.Add(state.DestroyedPropsDictionary, DestroyedPropsDictionary);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DestroyObstacleState", DestroyObstacleState);
		info.AddValue("DestroyedPropNames", DestroyedPropsDictionary);
	}

	public SEventAbilityDestroyObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "DestroyObstacleState"))
				{
					if (name == "DestroyedPropsDictionary")
					{
						DestroyedPropsDictionary = (Dictionary<string, int>)info.GetValue("DestroyedPropsDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					DestroyObstacleState = (CAbilityDestroyObstacle.DestroyObstacleState)info.GetValue("DestroyObstacleState", typeof(CAbilityDestroyObstacle.DestroyObstacleState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityDestroyObstacle entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDestroyObstacle(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityDestroyObstacle.DestroyObstacleState destroyObstacleState, Dictionary<string, int> destroyedPropsDictionary, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.DestroyObstacle, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		DestroyObstacleState = destroyObstacleState;
		DestroyedPropsDictionary = destroyedPropsDictionary;
	}
}
