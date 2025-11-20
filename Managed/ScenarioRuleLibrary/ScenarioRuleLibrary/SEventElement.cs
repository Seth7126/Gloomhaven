using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventElement : SEvent
{
	public ESESubTypeElement ElementSubType { get; private set; }

	public ElementInfusionBoardManager.EElement Element { get; private set; }

	public string ActorClassID { get; private set; }

	public CActor.EType ActorType { get; private set; }

	public bool ActorEnemySummon { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public SEventElement()
	{
	}

	public SEventElement(SEventElement state, ReferenceDictionary references)
		: base(state, references)
	{
		ElementSubType = state.ElementSubType;
		Element = state.Element;
		ActorClassID = state.ActorClassID;
		ActorType = state.ActorType;
		ActorEnemySummon = state.ActorEnemySummon;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int i = 0; i < state.PositiveConditions.Count; i++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[i];
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
		for (int j = 0; j < state.NegativeConditions.Count; j++)
		{
			NegativeConditionPair negativeConditionPair = state.NegativeConditions[j];
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
		info.AddValue("ElementSubType", ElementSubType);
		info.AddValue("Element", Element);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("ActorType", ActorType);
		info.AddValue("ActorEnemySummon", ActorEnemySummon);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public SEventElement(SerializationInfo info, StreamingContext context)
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
				case "ElementSubType":
					ElementSubType = (ESESubTypeElement)info.GetValue("ElementSubType", typeof(ESESubTypeElement));
					break;
				case "Element":
					Element = (ElementInfusionBoardManager.EElement)info.GetValue("Element", typeof(ElementInfusionBoardManager.EElement));
					break;
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "ActorType":
					ActorType = (CActor.EType)info.GetValue("ActorType", typeof(CActor.EType));
					break;
				case "ActorEnemySummon":
					ActorEnemySummon = info.GetBoolean("ActorEnemySummon");
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
				DLLDebug.LogError("Exception while trying to deserialize SEventElement entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventElement(ESESubTypeElement elementSubType, ElementInfusionBoardManager.EElement element, string actorClassID, CActor.EType actorType, bool IsSummon, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, string text = "")
		: base(ESEType.Element, text)
	{
		ElementSubType = elementSubType;
		Element = element;
		ActorClassID = actorClassID;
		ActorType = actorType;
		ActorEnemySummon = IsSummon;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}
}
