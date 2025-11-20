using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilitySwap : SEventAbility
{
	public CAbilitySwap.ESwapState SwapState { get; private set; }

	public string FirstTargetName { get; private set; }

	public string SecondTargetName { get; private set; }

	public TileIndex FirstTargetStartLocation { get; private set; }

	public TileIndex SecondTargetStartLocation { get; private set; }

	public SEventAbilitySwap()
	{
	}

	public SEventAbilitySwap(SEventAbilitySwap state, ReferenceDictionary references)
		: base(state, references)
	{
		SwapState = state.SwapState;
		FirstTargetName = state.FirstTargetName;
		SecondTargetName = state.SecondTargetName;
		FirstTargetStartLocation = references.Get(state.FirstTargetStartLocation);
		if (FirstTargetStartLocation == null && state.FirstTargetStartLocation != null)
		{
			FirstTargetStartLocation = new TileIndex(state.FirstTargetStartLocation, references);
			references.Add(state.FirstTargetStartLocation, FirstTargetStartLocation);
		}
		SecondTargetStartLocation = references.Get(state.SecondTargetStartLocation);
		if (SecondTargetStartLocation == null && state.SecondTargetStartLocation != null)
		{
			SecondTargetStartLocation = new TileIndex(state.SecondTargetStartLocation, references);
			references.Add(state.SecondTargetStartLocation, SecondTargetStartLocation);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SwapState", SwapState);
		info.AddValue("FirstTargetName", FirstTargetName);
		info.AddValue("SecondTargetName", SecondTargetName);
		info.AddValue("FirstTargetStartLocation", FirstTargetStartLocation);
		info.AddValue("SecondTargetStartLocation", SecondTargetStartLocation);
	}

	public SEventAbilitySwap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Name)
			{
			case "SwapState":
				SwapState = (CAbilitySwap.ESwapState)info.GetValue("SwapState", typeof(CAbilitySwap.ESwapState));
				break;
			case "FirstTargetName":
				FirstTargetName = info.GetString("FirstTargetName");
				break;
			case "SecondTargetName":
				SecondTargetName = info.GetString("SecondTargetName");
				break;
			case "FirstTargetStartLocation":
				FirstTargetStartLocation = (TileIndex)info.GetValue("FirstTargetStartLocation", typeof(TileIndex));
				break;
			case "SecondTargetStartLocation":
				SecondTargetStartLocation = (TileIndex)info.GetValue("SecondTargetStartLocation", typeof(TileIndex));
				break;
			}
		}
	}

	public SEventAbilitySwap(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilitySwap.ESwapState swapState, string firstTargetName, string secondTargetName, TileIndex firstTargetStartLocation, TileIndex secondTargetStartLocation, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Swap, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		SwapState = swapState;
		FirstTargetName = firstTargetName;
		SecondTargetName = secondTargetName;
		FirstTargetStartLocation = firstTargetStartLocation;
		SecondTargetStartLocation = secondTargetStartLocation;
	}
}
