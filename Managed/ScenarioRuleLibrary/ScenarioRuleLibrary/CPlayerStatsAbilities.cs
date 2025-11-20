using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsAbilities : CPlayerStatsAction, ISerializable
{
	public int Amount { get; private set; }

	public bool SpecialAbility { get; private set; }

	public bool? AreaEffect { get; private set; }

	public int Targets { get; private set; }

	public bool DefaultAction { get; private set; }

	public bool HasHappened { get; private set; }

	public CPlayerStatsAbilities()
	{
	}

	public CPlayerStatsAbilities(CPlayerStatsAbilities state, ReferenceDictionary references)
		: base(state, references)
	{
		Amount = state.Amount;
		SpecialAbility = state.SpecialAbility;
		AreaEffect = state.AreaEffect;
		Targets = state.Targets;
		DefaultAction = state.DefaultAction;
		HasHappened = state.HasHappened;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Amount", Amount);
		info.AddValue("SpecialAbility", SpecialAbility);
		info.AddValue("AreaEffect", AreaEffect);
		info.AddValue("Targets", Targets);
		info.AddValue("DefaultAction", DefaultAction);
		info.AddValue("HasHappened", HasHappened);
	}

	public CPlayerStatsAbilities(SerializationInfo info, StreamingContext context)
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
				case "Amount":
					Amount = info.GetInt32("Amount");
					break;
				case "SpecialAbility":
					SpecialAbility = info.GetBoolean("SpecialAbility");
					break;
				case "AreaEffect":
					AreaEffect = (bool?)info.GetValue("AreaEffect", typeof(bool?));
					break;
				case "Targets":
					Targets = info.GetInt32("Targets");
					break;
				case "DefaultAction":
					DefaultAction = info.GetBoolean("DefaultAction");
					break;
				case "HasHappened":
					HasHappened = info.GetBoolean("HasHappened");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsAbilities entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsAbilities(int amount, bool specialAbility, bool? areaEffect, int targets, bool defaultAction, bool hasHappened, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength)
	{
		Amount = amount;
		SpecialAbility = specialAbility;
		AreaEffect = areaEffect;
		Targets = targets;
		DefaultAction = defaultAction;
		HasHappened = hasHappened;
	}
}
