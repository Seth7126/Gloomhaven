using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityLoot : SEventAbility
{
	public CAbilityLoot.ELootState LootState { get; private set; }

	public List<string> PropsLooted { get; private set; }

	public SEventAbilityLoot()
	{
	}

	public SEventAbilityLoot(SEventAbilityLoot state, ReferenceDictionary references)
		: base(state, references)
	{
		LootState = state.LootState;
		PropsLooted = references.Get(state.PropsLooted);
		if (PropsLooted == null && state.PropsLooted != null)
		{
			PropsLooted = new List<string>();
			for (int i = 0; i < state.PropsLooted.Count; i++)
			{
				string item = state.PropsLooted[i];
				PropsLooted.Add(item);
			}
			references.Add(state.PropsLooted, PropsLooted);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootState", LootState);
		info.AddValue("PropsLooted", PropsLooted);
	}

	public SEventAbilityLoot(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "LootState"))
				{
					if (name == "PropsLooted")
					{
						PropsLooted = (List<string>)info.GetValue("PropsLooted", typeof(List<string>));
					}
				}
				else
				{
					LootState = (CAbilityLoot.ELootState)info.GetValue("LootState", typeof(CAbilityLoot.ELootState));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityLoot entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityLoot(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityLoot.ELootState lootState, List<string> propsLooted, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Loot, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		LootState = lootState;
		PropsLooted = propsLooted;
	}
}
