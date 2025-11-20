using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsHeal : CPlayerStatsAction, ISerializable
{
	public int HealAmount { get; private set; }

	public bool PoisonRemoved { get; private set; }

	public bool WoundRemoved { get; private set; }

	public CPlayerStatsHeal()
	{
	}

	public CPlayerStatsHeal(CPlayerStatsHeal state, ReferenceDictionary references)
		: base(state, references)
	{
		HealAmount = state.HealAmount;
		PoisonRemoved = state.PoisonRemoved;
		WoundRemoved = state.WoundRemoved;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HealAmount", HealAmount);
		info.AddValue("PoisonRemoved", PoisonRemoved);
		info.AddValue("WoundRemoved", WoundRemoved);
	}

	public CPlayerStatsHeal(SerializationInfo info, StreamingContext context)
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
				case "HealAmount":
					HealAmount = info.GetInt32("HealAmount");
					break;
				case "PoisonRemoved":
					PoisonRemoved = info.GetBoolean("PoisonRemoved");
					break;
				case "WoundRemoved":
					WoundRemoved = info.GetBoolean("WoundRemoved");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsHeal entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsHeal(int healAmount, bool poisonRemoved, bool woundRemoved, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner)
	{
		HealAmount = healAmount;
		PoisonRemoved = poisonRemoved;
		WoundRemoved = woundRemoved;
	}
}
