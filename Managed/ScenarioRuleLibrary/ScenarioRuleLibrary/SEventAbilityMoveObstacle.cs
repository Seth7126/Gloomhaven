using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityMoveObstacle : SEventAbility
{
	public CAbilityMoveObstacle.MoveObstacleState MovedObstacleState { get; private set; }

	public Dictionary<string, int> MovedPropsDictionary { get; private set; }

	public SEventAbilityMoveObstacle()
	{
	}

	public SEventAbilityMoveObstacle(SEventAbilityMoveObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		MovedObstacleState = state.MovedObstacleState;
		MovedPropsDictionary = references.Get(state.MovedPropsDictionary);
		if (MovedPropsDictionary != null || state.MovedPropsDictionary == null)
		{
			return;
		}
		MovedPropsDictionary = new Dictionary<string, int>(state.MovedPropsDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.MovedPropsDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			MovedPropsDictionary.Add(key, value);
		}
		references.Add(state.MovedPropsDictionary, MovedPropsDictionary);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MovedObstacleState", MovedObstacleState);
		info.AddValue("MovedPropsDictionary", MovedPropsDictionary);
	}

	public SEventAbilityMoveObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "MovedObstacleState"))
				{
					if (name == "MovedPropsDictionary")
					{
						MovedPropsDictionary = (Dictionary<string, int>)info.GetValue("MovedPropsDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					MovedObstacleState = (CAbilityMoveObstacle.MoveObstacleState)info.GetValue("MovedObstacleState", typeof(CAbilityMoveObstacle.MoveObstacleState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityMoveObstacle entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityMoveObstacle(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityMoveObstacle.MoveObstacleState moveObstacleState, Dictionary<string, int> movedPropsDictionary, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.MoveObstacle, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		MovedObstacleState = moveObstacleState;
		MovedPropsDictionary = movedPropsDictionary;
	}
}
