using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.Source.Shared;

[Serializable]
public class CNextScenarioEffects : ISerializable
{
	public List<ElementInfusionBoardManager.EElement> Infusions;

	public int Damage;

	public int Discard;

	public List<Tuple<string, string>> ConsumeItems;

	public List<Tuple<string, Dictionary<string, int>>> AttackModifiers;

	public List<NegativeConditionPair> EnemyNegativeConditions { get; set; }

	public List<PositiveConditionPair> EnemyPositiveConditions { get; set; }

	public CNextScenarioEffects(CNextScenarioEffects state, ReferenceDictionary references)
	{
		Infusions = references.Get(state.Infusions);
		if (Infusions == null && state.Infusions != null)
		{
			Infusions = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.Infusions.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.Infusions[i];
				Infusions.Add(item);
			}
			references.Add(state.Infusions, Infusions);
		}
		Damage = state.Damage;
		Discard = state.Discard;
		ConsumeItems = references.Get(state.ConsumeItems);
		if (ConsumeItems == null && state.ConsumeItems != null)
		{
			ConsumeItems = new List<Tuple<string, string>>();
			for (int j = 0; j < state.ConsumeItems.Count; j++)
			{
				Tuple<string, string> tuple = state.ConsumeItems[j];
				string item2 = tuple.Item1;
				string item3 = tuple.Item2;
				Tuple<string, string> item4 = new Tuple<string, string>(item2, item3);
				ConsumeItems.Add(item4);
			}
			references.Add(state.ConsumeItems, ConsumeItems);
		}
		AttackModifiers = references.Get(state.AttackModifiers);
		if (AttackModifiers == null && state.AttackModifiers != null)
		{
			AttackModifiers = new List<Tuple<string, Dictionary<string, int>>>();
			for (int k = 0; k < state.AttackModifiers.Count; k++)
			{
				Tuple<string, Dictionary<string, int>> tuple2 = state.AttackModifiers[k];
				string item5 = tuple2.Item1;
				Dictionary<string, int> dictionary = references.Get(tuple2.Item2);
				if (dictionary == null && tuple2.Item2 != null)
				{
					dictionary = new Dictionary<string, int>(tuple2.Item2.Comparer);
					foreach (KeyValuePair<string, int> item7 in tuple2.Item2)
					{
						string key = item7.Key;
						int value = item7.Value;
						dictionary.Add(key, value);
					}
					references.Add(tuple2.Item2, dictionary);
				}
				Tuple<string, Dictionary<string, int>> item6 = new Tuple<string, Dictionary<string, int>>(item5, dictionary);
				AttackModifiers.Add(item6);
			}
			references.Add(state.AttackModifiers, AttackModifiers);
		}
		EnemyNegativeConditions = references.Get(state.EnemyNegativeConditions);
		if (EnemyNegativeConditions == null && state.EnemyNegativeConditions != null)
		{
			EnemyNegativeConditions = new List<NegativeConditionPair>();
			for (int l = 0; l < state.EnemyNegativeConditions.Count; l++)
			{
				NegativeConditionPair negativeConditionPair = state.EnemyNegativeConditions[l];
				NegativeConditionPair negativeConditionPair2 = references.Get(negativeConditionPair);
				if (negativeConditionPair2 == null && negativeConditionPair != null)
				{
					negativeConditionPair2 = new NegativeConditionPair(negativeConditionPair, references);
					references.Add(negativeConditionPair, negativeConditionPair2);
				}
				EnemyNegativeConditions.Add(negativeConditionPair2);
			}
			references.Add(state.EnemyNegativeConditions, EnemyNegativeConditions);
		}
		EnemyPositiveConditions = references.Get(state.EnemyPositiveConditions);
		if (EnemyPositiveConditions != null || state.EnemyPositiveConditions == null)
		{
			return;
		}
		EnemyPositiveConditions = new List<PositiveConditionPair>();
		for (int m = 0; m < state.EnemyPositiveConditions.Count; m++)
		{
			PositiveConditionPair positiveConditionPair = state.EnemyPositiveConditions[m];
			PositiveConditionPair positiveConditionPair2 = references.Get(positiveConditionPair);
			if (positiveConditionPair2 == null && positiveConditionPair != null)
			{
				positiveConditionPair2 = new PositiveConditionPair(positiveConditionPair, references);
				references.Add(positiveConditionPair, positiveConditionPair2);
			}
			EnemyPositiveConditions.Add(positiveConditionPair2);
		}
		references.Add(state.EnemyPositiveConditions, EnemyPositiveConditions);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Infusions", Infusions);
		info.AddValue("Damage", Damage);
		info.AddValue("Discard", Discard);
		info.AddValue("ConsumeItemSlots", ConsumeItems);
		info.AddValue("AttackModifiers", AttackModifiers);
		info.AddValue("EnemyNegativeConditions", EnemyNegativeConditions);
		info.AddValue("EnemyPositiveConditions", EnemyPositiveConditions);
	}

	private CNextScenarioEffects(SerializationInfo info, StreamingContext context)
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
				case "Discard":
					Discard = info.GetInt32(current.Name);
					break;
				case "ConsumeItemSlots":
					ConsumeItems = (List<Tuple<string, string>>)info.GetValue(current.Name, typeof(List<Tuple<string, string>>));
					break;
				case "AttackModifiers":
					AttackModifiers = (List<Tuple<string, Dictionary<string, int>>>)info.GetValue(current.Name, typeof(List<Tuple<string, Dictionary<string, int>>>));
					break;
				case "EnemyNegativeConditions":
					EnemyNegativeConditions = (List<NegativeConditionPair>)info.GetValue(current.Name, typeof(List<NegativeConditionPair>));
					break;
				case "EnemyPositiveConditions":
					EnemyPositiveConditions = (List<PositiveConditionPair>)info.GetValue(current.Name, typeof(List<PositiveConditionPair>));
					break;
				case "Modifiers":
					AttackModifiers = new List<Tuple<string, Dictionary<string, int>>>();
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CNextScenarioEffects entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CNextScenarioEffects()
	{
		Infusions = new List<ElementInfusionBoardManager.EElement>();
		EnemyNegativeConditions = new List<NegativeConditionPair>();
		EnemyPositiveConditions = new List<PositiveConditionPair>();
		ConsumeItems = new List<Tuple<string, string>>();
		AttackModifiers = new List<Tuple<string, Dictionary<string, int>>>();
	}
}
