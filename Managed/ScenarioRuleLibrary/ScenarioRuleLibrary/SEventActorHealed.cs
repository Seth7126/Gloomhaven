using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorHealed : SEventActor
{
	public int HealAmount { get; private set; }

	public bool PoisonRemoved { get; private set; }

	public bool WoundRemoved { get; private set; }

	public SEventActorHealed()
	{
	}

	public SEventActorHealed(SEventActorHealed state, ReferenceDictionary references)
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

	public SEventActorHealed(SerializationInfo info, StreamingContext context)
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
				DLLDebug.LogError("Exception while trying to deserialize SEventActorHealed entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorHealed(bool poisonRemoved, bool woundRemoved, int healAmount, CActor.EType actorType, string actorGuid, string actorClass, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actedOnByGUID = "", string actedOnByClass = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnSummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", int maxHealth = 0)
		: base(ESESubTypeActor.ActorHealed, actorType, actorGuid, actorClass, health, gold, xp, level, positiveConditions, negativeConditions, playedThisRound, isDead, causeOfDeath, IsSummon, actedOnByGUID, actedOnByClass, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnSummon, actedOnPositiveConditions, actedOnNegativeConditions, text, doNotSerialize: false, maxHealth)
	{
		PoisonRemoved = poisonRemoved;
		WoundRemoved = woundRemoved;
		HealAmount = healAmount;
	}
}
