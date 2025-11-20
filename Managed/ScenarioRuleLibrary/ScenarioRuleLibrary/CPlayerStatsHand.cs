using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsHand : CPlayerStatsAction, ISerializable
{
	public int HandSize { get; private set; }

	public int DiscardSize { get; private set; }

	public CPlayerStatsHand()
	{
	}

	public CPlayerStatsHand(CPlayerStatsHand state, ReferenceDictionary references)
		: base(state, references)
	{
		HandSize = state.HandSize;
		DiscardSize = state.DiscardSize;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HandSize", HandSize);
		info.AddValue("DiscardSize", DiscardSize);
	}

	public CPlayerStatsHand(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "HandSize"))
				{
					if (name == "DiscardSize")
					{
						DiscardSize = info.GetInt32("DiscardSize");
					}
				}
				else
				{
					HandSize = info.GetInt32("HandSize");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsHand entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsHand(int handSize, int discardSize, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner)
	{
		HandSize = handSize;
		DiscardSize = discardSize;
	}
}
