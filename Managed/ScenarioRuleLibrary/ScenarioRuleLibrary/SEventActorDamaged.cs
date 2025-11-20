using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorDamaged : SEventActor
{
	public int Avoided;

	public int Shielded;

	public int ItemShielded;

	public bool IsMelee;

	public int Shield;

	public int PoisonDamage;

	public int DamageTaken { get; private set; }

	public CActor.ECauseOfDamage CauseOfDamage { get; private set; }

	public SEventActorDamaged()
	{
	}

	public SEventActorDamaged(SEventActorDamaged state, ReferenceDictionary references)
		: base(state, references)
	{
		DamageTaken = state.DamageTaken;
		CauseOfDamage = state.CauseOfDamage;
		Avoided = state.Avoided;
		Shielded = state.Shielded;
		ItemShielded = state.ItemShielded;
		IsMelee = state.IsMelee;
		Shield = state.Shield;
		PoisonDamage = state.PoisonDamage;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DamageTaken", DamageTaken);
		info.AddValue("CauseOfDamage", CauseOfDamage);
		info.AddValue("Avoided", Avoided);
		info.AddValue("Shielded", Shielded);
		info.AddValue("ItemShielded", Shielded);
		info.AddValue("IsMelee", IsMelee);
		info.AddValue("Shield", Shield);
		info.AddValue("PoisonDamage", PoisonDamage);
	}

	public SEventActorDamaged(SerializationInfo info, StreamingContext context)
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
				case "DamageTaken":
					DamageTaken = info.GetInt32("DamageTaken");
					break;
				case "CauseOfDamage":
					CauseOfDamage = (CActor.ECauseOfDamage)info.GetValue("CauseOfDamage", typeof(CActor.ECauseOfDamage));
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
				case "Shield":
					Shield = info.GetInt32("Shield");
					break;
				case "PoisonDamage":
					PoisonDamage = info.GetInt32("PoisonDamage");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventActorDamaged entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorDamaged(int damageTaken, CActor.ECauseOfDamage causeOfDamage, CActor.EType actorType, string actorGuid, string actorClass, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actedOnByGUID = "", string actedOnByClass = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, int avoided = 0, int shielded = 0, int itemShielded = 0, bool isMelee = false, int poisonAmt = 0, int shield = 0, bool actedOnSummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", int maxHealth = 0, bool hasFavorite = false)
		: base(ESESubTypeActor.ActorDamaged, actorType, actorGuid, actorClass, health, gold, xp, level, positiveConditions, negativeConditions, playedThisRound, isDead, causeOfDeath, IsSummon, actedOnByGUID, actedOnByClass, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnSummon, actedOnPositiveConditions, actedOnNegativeConditions, text, doNotSerialize: false, maxHealth, 0, 0, null, doom: false, "", attackerDisadvantage: false, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, hasFavorite)
	{
		DamageTaken = damageTaken;
		CauseOfDamage = causeOfDamage;
		Avoided = avoided;
		Shielded = shielded;
		ItemShielded = itemShielded;
		IsMelee = isMelee;
		PoisonDamage = poisonAmt;
		Shield = shield;
	}
}
