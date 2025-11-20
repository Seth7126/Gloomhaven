using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDiscardCard : CPlayerStatsAction, ISerializable
{
	public int DiscardedCardID { get; private set; }

	public CBaseCard.ECardPile Pile { get; private set; }

	public DateTime TimeStamp { get; private set; }

	public CPlayerStatsDiscardCard()
	{
	}

	public CPlayerStatsDiscardCard(CPlayerStatsDiscardCard state, ReferenceDictionary references)
		: base(state, references)
	{
		DiscardedCardID = state.DiscardedCardID;
		Pile = state.Pile;
		TimeStamp = state.TimeStamp;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DiscardedCardID", DiscardedCardID);
		info.AddValue("Pile", Pile);
		info.AddValue("TimeStamp", TimeStamp);
	}

	public CPlayerStatsDiscardCard(SerializationInfo info, StreamingContext context)
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
				case "DiscardedCardID":
					DiscardedCardID = info.GetInt32("DiscardedCardID");
					break;
				case "Pile":
					Pile = (CBaseCard.ECardPile)info.GetValue("Pile", typeof(CBaseCard.ECardPile));
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsDiscardCard entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDiscardCard(int card, CBaseCard.ECardPile pile, DateTime timeStamp, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner)
	{
		DiscardedCardID = card;
		Pile = pile;
		TimeStamp = timeStamp;
	}
}
