using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDamage : CPlayerStatsAction, ISerializable
{
	public int Avoided;

	public int Shielded;

	public int ItemShielded;

	public bool IsMelee;

	public int PoisonDamage;

	public int Shield;

	public string ScenarioResult;

	public bool HasFavorite;

	public CActor.ECauseOfDamage CauseOfDamage { get; private set; }

	public int FinalDamageAmount { get; private set; }

	public CPlayerStatsDamage()
	{
	}

	public CPlayerStatsDamage(CPlayerStatsDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		CauseOfDamage = state.CauseOfDamage;
		FinalDamageAmount = state.FinalDamageAmount;
		Avoided = state.Avoided;
		Shielded = state.Shielded;
		ItemShielded = state.ItemShielded;
		IsMelee = state.IsMelee;
		PoisonDamage = state.PoisonDamage;
		Shield = state.Shield;
		ScenarioResult = state.ScenarioResult;
		HasFavorite = state.HasFavorite;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CauseOfDamage", CauseOfDamage);
		info.AddValue("FinalDamageAmount", FinalDamageAmount);
		info.AddValue("Avoided", Avoided);
		info.AddValue("Shielded", Shielded);
		info.AddValue("ItemShielded", Shielded);
		info.AddValue("IsMelee", IsMelee);
		info.AddValue("PoisonDamage", PoisonDamage);
		info.AddValue("Shield", Shield);
		info.AddValue("ScenarioResult", ScenarioResult);
		info.AddValue("HasFavorite", HasFavorite);
	}

	public CPlayerStatsDamage(SerializationInfo info, StreamingContext context)
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
				case "CauseOfDamage":
					CauseOfDamage = (CActor.ECauseOfDamage)info.GetValue("CauseOfDamage", typeof(CActor.ECauseOfDamage));
					break;
				case "FinalDamageAmount":
					FinalDamageAmount = info.GetInt32("FinalDamageAmount");
					break;
				case "Avoided":
					Avoided = info.GetInt32("Avoided");
					break;
				case "Shielded":
					Shielded = info.GetInt32("Shielded");
					break;
				case "ItemShielded":
					ItemShielded = info.GetInt32("ItemShielded");
					break;
				case "IsMelee":
					IsMelee = info.GetBoolean("IsMelee");
					break;
				case "PoisonDamage":
					PoisonDamage = info.GetInt32("PoisonDamage");
					break;
				case "Shield":
					Shield = info.GetInt32("Shield");
					break;
				case "ScenarioResult":
					ScenarioResult = info.GetString("ScenarioResult");
					break;
				case "HasFavorite":
					HasFavorite = info.GetBoolean("HasFavorite");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsDamage entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDamage(CActor.ECauseOfDamage causeOfDamage, int finalDamage, int avoided, int shielded, int itemShielded, bool isMelee, int poison, int shield, string scenarioResult, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int health, int maxHealth, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false, bool hasFavorite = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner, health, maxHealth)
	{
		CauseOfDamage = causeOfDamage;
		FinalDamageAmount = finalDamage;
		Avoided = avoided;
		Shielded = shielded;
		ItemShielded = itemShielded;
		IsMelee = isMelee;
		PoisonDamage = poison;
		Shield = shield;
		ScenarioResult = scenarioResult;
		HasFavorite = hasFavorite;
	}
}
