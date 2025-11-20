using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityInfuse : SEventAbility
{
	public CAbilityInfuse.EInfuseState InfuseState { get; private set; }

	public List<ElementInfusionBoardManager.EElement> ElementsInfused { get; set; }

	public SEventAbilityInfuse()
	{
	}

	public SEventAbilityInfuse(SEventAbilityInfuse state, ReferenceDictionary references)
		: base(state, references)
	{
		InfuseState = state.InfuseState;
		ElementsInfused = references.Get(state.ElementsInfused);
		if (ElementsInfused == null && state.ElementsInfused != null)
		{
			ElementsInfused = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.ElementsInfused.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.ElementsInfused[i];
				ElementsInfused.Add(item);
			}
			references.Add(state.ElementsInfused, ElementsInfused);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("InfuseState", InfuseState);
		info.AddValue("ElementsInfused", ElementsInfused);
	}

	public SEventAbilityInfuse(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "InfuseState"))
				{
					if (name == "ElementsInfused")
					{
						ElementsInfused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsInfused", typeof(List<ElementInfusionBoardManager.EElement>));
					}
				}
				else
				{
					InfuseState = (CAbilityInfuse.EInfuseState)info.GetValue("InfuseState", typeof(CAbilityInfuse.EInfuseState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityInfuse entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityInfuse(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityInfuse.EInfuseState infuseState, List<ElementInfusionBoardManager.EElement> elementsInfused, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Infuse, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		InfuseState = infuseState;
		ElementsInfused = elementsInfused;
	}
}
