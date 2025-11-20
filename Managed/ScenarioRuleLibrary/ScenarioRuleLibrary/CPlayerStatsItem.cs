using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsItem : CPlayerStatsAction, ISerializable
{
	public string ItemName { get; private set; }

	public string Slot { get; private set; }

	public bool FirstTimeUse { get; private set; }

	public CPlayerStatsItem()
	{
	}

	public CPlayerStatsItem(CPlayerStatsItem state, ReferenceDictionary references)
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

	public CPlayerStatsItem(SerializationInfo info, StreamingContext context)
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
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsItem entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsItem(string itemName, string slot, bool firstTimeUse, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner)
	{
		ItemName = itemName;
		Slot = slot;
		FirstTimeUse = firstTimeUse;
	}
}
