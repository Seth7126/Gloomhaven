using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDestroyObstacle : CPlayerStatsAction, ISerializable
{
	public Dictionary<string, int> DestroyedObstaclesDictionary { get; private set; }

	public CPlayerStatsDestroyObstacle()
	{
	}

	public CPlayerStatsDestroyObstacle(CPlayerStatsDestroyObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		DestroyedObstaclesDictionary = references.Get(state.DestroyedObstaclesDictionary);
		if (DestroyedObstaclesDictionary != null || state.DestroyedObstaclesDictionary == null)
		{
			return;
		}
		DestroyedObstaclesDictionary = new Dictionary<string, int>(state.DestroyedObstaclesDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.DestroyedObstaclesDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			DestroyedObstaclesDictionary.Add(key, value);
		}
		references.Add(state.DestroyedObstaclesDictionary, DestroyedObstaclesDictionary);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DestroyedObstaclesDictionary", DestroyedObstaclesDictionary);
	}

	public CPlayerStatsDestroyObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "DestroyedObstaclesDictionary")
				{
					DestroyedObstaclesDictionary = (Dictionary<string, int>)info.GetValue("DestroyedObstaclesDictionary", typeof(Dictionary<string, int>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsDestroyObstacle entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDestroyObstacle(Dictionary<string, int> destroyedObstacles, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength)
	{
		DestroyedObstaclesDictionary = destroyedObstacles;
	}
}
