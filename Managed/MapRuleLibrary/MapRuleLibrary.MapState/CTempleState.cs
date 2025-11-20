using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CTempleState : ISerializable
{
	public int GoldDonated { get; set; }

	public int DevotionLevel => CalculateDevotionLevel(GoldDonated);

	public CTemple Temple => MapRuleLibraryClient.MRLYML.Temples[0];

	public CTempleState(CTempleState state, ReferenceDictionary references)
	{
		GoldDonated = state.GoldDonated;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("GoldDonated", GoldDonated);
	}

	public CTempleState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "GoldDonated")
				{
					GoldDonated = info.GetInt32("GoldDonated");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CTempleState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CTempleState()
	{
		GoldDonated = 0;
	}

	public int CalculateDevotionLevel(int goldDonated)
	{
		int num = 0;
		if (Temple.DonationTable != null)
		{
			foreach (Tuple<int, string> item in Temple.DonationTable)
			{
				if (goldDonated >= item.Item1)
				{
					num++;
					continue;
				}
				return num;
			}
		}
		return num;
	}

	public void ApplyTempleBlessing(CMapCharacter character, TempleYML.TempleBlessingDefinition blessingDefinition)
	{
		for (int i = 0; i < blessingDefinition.Quantity; i++)
		{
			switch (blessingDefinition.TempleBlessingCondition.MapDuration)
			{
			case RewardCondition.EConditionMapDuration.NextScenario:
			case RewardCondition.EConditionMapDuration.NextVillage:
				if (blessingDefinition.TempleBlessingCondition.Type == RewardCondition.EConditionType.Positive)
				{
					character.PositiveConditions.Add(new PositiveConditionPair(blessingDefinition.TempleBlessingCondition.PositiveCondition, blessingDefinition.TempleBlessingCondition.MapDuration, blessingDefinition.TempleBlessingCondition.RoundDuration, EConditionDecTrigger.Turns));
				}
				break;
			case RewardCondition.EConditionMapDuration.Now:
				DLLDebug.LogError("Unable to apply condition Now. This can only be done within a scenario.");
				break;
			default:
				DLLDebug.LogError("Unable to process condition with duration " + blessingDefinition.TempleBlessingCondition.MapDuration);
				break;
			}
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			AdventureState.MapState.MapParty.ModifyPartyGold(-blessingDefinition.GoldCost, useGoldModifier: true);
		}
		else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			character.ModifyGold(-blessingDefinition.GoldCost, useGoldModifier: true);
			character.Donations += blessingDefinition.GoldCost;
		}
		UpdateDonatedGold(blessingDefinition.GoldCost);
	}

	public void UpdateDonatedGold(int goldDonated)
	{
		int goldDonated2 = GoldDonated;
		GoldDonated += goldDonated;
		for (int i = 0; i < Temple.DonationTable.Count; i++)
		{
			Tuple<int, string> donationTableEntry = Temple.DonationTable[i];
			if (donationTableEntry.Item1 > goldDonated2 && donationTableEntry.Item1 <= GoldDonated)
			{
				TreasureTable item = ScenarioRuleClient.SRLYML.TreasureTables.Find((TreasureTable x) => x.Name == donationTableEntry.Item2);
				List<RewardGroup> list = TreasureTableProcessing.RollTreasureTables(AdventureState.MapState.MapRNG, MapYMLShared.ValidateTreasureTableRewards(new List<TreasureTable> { item }, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter)), AdventureState.MapState.MapParty.ScenarioLevel, AdventureState.MapState.HighestUnlockedChapter, AdventureState.MapState.GetHighestUnlockedSubChapter(AdventureState.MapState.HighestUnlockedChapter));
				AdventureState.MapState.ApplyRewards(list);
				CTempleDevotionLevelUp_MapClientMessage message = new CTempleDevotionLevelUp_MapClientMessage(i + 1, list, "GUI_TEMPLE_DEVOTION_LEVEL");
				MapRuleLibraryClient.Instance.MessageHandler(message);
				SimpleLog.AddToSimpleLog("MapRNG (donated gold): " + AdventureState.MapState.PeekMapRNG);
			}
		}
	}
}
