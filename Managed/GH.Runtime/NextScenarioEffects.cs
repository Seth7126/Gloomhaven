using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;
using ScenarioRuleLibrary;

[Serializable]
public class NextScenarioEffects : ISerializable
{
	public List<ElementInfusionBoardManager.EElement> Infusions;

	public int Damage;

	public List<NegativeConditionPair> EnemyNegativeConditions { get; private set; }

	public List<PositiveConditionPair> EnemyPositiveConditions { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Infusions", Infusions);
		info.AddValue("Damage", Damage);
		info.AddValue("EnemyNegativeConditions", EnemyNegativeConditions);
		info.AddValue("EnemyPositiveConditions", EnemyPositiveConditions);
	}

	private NextScenarioEffects(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Infusions":
					Infusions = (List<ElementInfusionBoardManager.EElement>)info.GetValue(current.Name, typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "Damage":
					Damage = info.GetInt32(current.Name);
					break;
				case "EnemyNegativeConditions":
					EnemyNegativeConditions = (List<NegativeConditionPair>)info.GetValue(current.Name, typeof(List<NegativeConditionPair>));
					break;
				case "EnemyPositiveConditions":
					EnemyPositiveConditions = (List<PositiveConditionPair>)info.GetValue(current.Name, typeof(List<PositiveConditionPair>));
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize NextScenarioEffects entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public NextScenarioEffects()
	{
		Infusions = new List<ElementInfusionBoardManager.EElement>();
		EnemyNegativeConditions = new List<NegativeConditionPair>();
		EnemyPositiveConditions = new List<PositiveConditionPair>();
	}
}
