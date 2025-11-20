using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorUsedItem : SEventActor
{
	public string ItemName;

	public string Slot;

	public bool FirstTimeUse;

	public SEventActorUsedItem()
	{
	}

	public SEventActorUsedItem(SEventActorUsedItem state, ReferenceDictionary references)
		: base(state, references)
	{
		ItemName = state.ItemName;
		Slot = state.Slot;
		FirstTimeUse = state.FirstTimeUse;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ItemName", ItemName);
		info.AddValue("Slot", Slot);
		info.AddValue("FirstTimeUse", FirstTimeUse);
	}

	public SEventActorUsedItem(SerializationInfo info, StreamingContext context)
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
				case "ItemName":
					ItemName = info.GetString("ItemName");
					break;
				case "Slot":
					Slot = info.GetString("Slot");
					break;
				case "FirstTimeUse":
					FirstTimeUse = info.GetBoolean("FirstTimeUse");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventActorUsedItem entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorUsedItem(string itemName, string slot, bool firstTimeUse, CActor.EType actorType, string actorGuid, string actorClass, int health, int gold, int xp, int level, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, bool playedThisRound, bool isDead, CActor.ECauseOfDeath causeOfDeath, bool IsSummon, string actedOnByGUID = "", string actedOnByClass = "", CActor.EType? actedOnType = null, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = 0, bool actedOnSummon = false, List<PositiveConditionPair> actedOnPositiveConditions = null, List<NegativeConditionPair> actedOnNegativeConditions = null, string text = "", int maxHealth = 0)
		: base(ESESubTypeActor.ActorUsedItem, actorType, actorGuid, actorClass, health, gold, xp, level, positiveConditions, negativeConditions, playedThisRound, isDead, causeOfDeath, IsSummon, actedOnByGUID, actedOnByClass, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnSummon, actedOnPositiveConditions, actedOnNegativeConditions, text, doNotSerialize: false, maxHealth)
	{
		ItemName = itemName;
		Slot = slot;
		FirstTimeUse = firstTimeUse;
	}
}
