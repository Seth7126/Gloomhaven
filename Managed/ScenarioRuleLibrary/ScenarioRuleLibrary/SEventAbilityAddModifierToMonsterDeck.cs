using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityAddModifierToMonsterDeck : SEventAbility
{
	public CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState AddModifierToMonsterDeckState { get; private set; }

	public List<string> AddModifierStrings { get; private set; }

	public SEventAbilityAddModifierToMonsterDeck()
	{
	}

	public SEventAbilityAddModifierToMonsterDeck(SEventAbilityAddModifierToMonsterDeck state, ReferenceDictionary references)
		: base(state, references)
	{
		AddModifierToMonsterDeckState = state.AddModifierToMonsterDeckState;
		AddModifierStrings = references.Get(state.AddModifierStrings);
		if (AddModifierStrings == null && state.AddModifierStrings != null)
		{
			AddModifierStrings = new List<string>();
			for (int i = 0; i < state.AddModifierStrings.Count; i++)
			{
				string item = state.AddModifierStrings[i];
				AddModifierStrings.Add(item);
			}
			references.Add(state.AddModifierStrings, AddModifierStrings);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AddModifierToMonsterDeckState", AddModifierToMonsterDeckState);
		info.AddValue("AddModifierStrings", AddModifierStrings);
	}

	public SEventAbilityAddModifierToMonsterDeck(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Current.Name;
			if (!(name == "AddModifierToMonsterDeckState"))
			{
				if (name == "AddModifierStrings")
				{
					AddModifierStrings = (List<string>)info.GetValue("AddModifierStrings", typeof(List<string>));
				}
			}
			else
			{
				AddModifierToMonsterDeckState = (CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState)info.GetValue("AddModifierToMonsterDeckState", typeof(CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState));
			}
		}
	}

	public SEventAbilityAddModifierToMonsterDeck(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState addModifierToMonsterDeckState, List<string> addModifierStrings, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.AddModifierToMonsterDeck, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		AddModifierToMonsterDeckState = addModifierToMonsterDeckState;
		AddModifierStrings = addModifierStrings;
	}
}
