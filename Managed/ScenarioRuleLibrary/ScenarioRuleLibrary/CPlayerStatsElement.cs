using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsElement : CPlayerStatsBase, ISerializable
{
	[Serializable]
	public enum EStatsElementType
	{
		None,
		Infused,
		Consumed
	}

	public string ActorClassID { get; private set; }

	public EStatsElementType StatsElementType { get; private set; }

	public ElementInfusionBoardManager.EElement Element { get; private set; }

	public string ActingType { get; private set; }

	public List<ElementInfusionBoardManager.EElement> Infused { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public CPlayerStatsElement()
	{
	}

	public CPlayerStatsElement(CPlayerStatsElement state, ReferenceDictionary references)
		: base(state, references)
	{
		ActorClassID = state.ActorClassID;
		StatsElementType = state.StatsElementType;
		Element = state.Element;
		ActingType = state.ActingType;
		Infused = references.Get(state.Infused);
		if (Infused == null && state.Infused != null)
		{
			Infused = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.Infused.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.Infused[i];
				Infused.Add(item);
			}
			references.Add(state.Infused, Infused);
		}
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int j = 0; j < state.PositiveConditions.Count; j++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[j];
				PositiveConditionPair positiveConditionPair2 = references.Get(positiveConditionPair);
				if (positiveConditionPair2 == null && positiveConditionPair != null)
				{
					positiveConditionPair2 = new PositiveConditionPair(positiveConditionPair, references);
					references.Add(positiveConditionPair, positiveConditionPair2);
				}
				PositiveConditions.Add(positiveConditionPair2);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions != null || state.NegativeConditions == null)
		{
			return;
		}
		NegativeConditions = new List<NegativeConditionPair>();
		for (int k = 0; k < state.NegativeConditions.Count; k++)
		{
			NegativeConditionPair negativeConditionPair = state.NegativeConditions[k];
			NegativeConditionPair negativeConditionPair2 = references.Get(negativeConditionPair);
			if (negativeConditionPair2 == null && negativeConditionPair != null)
			{
				negativeConditionPair2 = new NegativeConditionPair(negativeConditionPair, references);
				references.Add(negativeConditionPair, negativeConditionPair2);
			}
			NegativeConditions.Add(negativeConditionPair2);
		}
		references.Add(state.NegativeConditions, NegativeConditions);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("StatsElementType", StatsElementType);
		info.AddValue("Element", Element);
		info.AddValue("ActingType", ActingType);
		info.AddValue("Infused", Infused);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public CPlayerStatsElement(SerializationInfo info, StreamingContext context)
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
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "StatsElementType":
					StatsElementType = (EStatsElementType)info.GetValue("StatsElementType", typeof(EStatsElementType));
					break;
				case "Element":
					Element = (ElementInfusionBoardManager.EElement)info.GetValue("Element", typeof(ElementInfusionBoardManager.EElement));
					break;
				case "ActingType":
					ActingType = info.GetString("ActingType");
					break;
				case "Infused":
					Infused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infused", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActorClass":
				{
					string text = info.GetString("ActorClass").Replace(" ", string.Empty);
					ActorClassID = text + "ID";
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsElement entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsElement(string advGuid, string sceGuid, string questType, int round, string actorClassID, EStatsElementType statsElementType, ElementInfusionBoardManager.EElement element, string actingType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions)
		: base(advGuid, sceGuid, questType, round)
	{
		ActorClassID = actorClassID;
		StatsElementType = statsElementType;
		Element = element;
		ActingType = actingType;
		Infused = infused;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}
}
