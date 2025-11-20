using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Events;

public class CRoadEventTreasureCondition
{
	public static ETreasureCondition[] TreasureConditions = (ETreasureCondition[])Enum.GetValues(typeof(ETreasureCondition));

	public CRoadEventScreen Screen { get; private set; }

	public RewardGroup MatchConditions { get; private set; }

	public ETreasureCondition TreasureCondition { get; private set; }

	public List<string> InParty { get; private set; }

	public CEqualityFilter Reputation { get; private set; }

	public string MinimumGold { get; private set; }

	public List<string> Items { get; private set; }

	public int Player { get; private set; }

	public int MinValue { get; private set; }

	public int MaxValue { get; private set; }

	public CRoadEventTreasureCondition(CRoadEventScreen screen, RewardGroup matchConditions, ETreasureCondition treasureCondition, int player, int minValue, int maxValue, List<string> inParty = null, CEqualityFilter reputation = null, string minimumGold = null, List<string> items = null)
	{
		Screen = screen;
		MatchConditions = matchConditions;
		TreasureCondition = treasureCondition;
		InParty = inParty;
		Reputation = reputation;
		MinimumGold = minimumGold;
		Items = items;
		Player = player;
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public static bool DoesRulesMatch(CRoadEventTreasureCondition matchConditions, int roll = 0)
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		bool flag5 = true;
		if (matchConditions.TreasureCondition == ETreasureCondition.InParty)
		{
			flag = false;
			foreach (string classID in matchConditions.InParty)
			{
				if (AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter x) => x.CharacterID == classID))
				{
					flag = true;
				}
			}
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.Reputation)
		{
			flag2 = matchConditions.Reputation.Compare(AdventureState.MapState.MapParty.Reputation);
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.RollResult)
		{
			flag5 = AdventureState.MapState.MapParty.SelectedCharacters.Count() >= matchConditions.Player && roll >= matchConditions.MinValue && roll <= matchConditions.MaxValue;
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.MinGold)
		{
			flag3 = false;
			if (int.TryParse(matchConditions.MinimumGold.Replace("Each", ""), out var goldValue))
			{
				if (matchConditions.MinimumGold.Contains("Each"))
				{
					if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold && AdventureState.MapState.MapParty.PartyGold >= goldValue * AdventureState.MapState.MapParty.SelectedCharacters.Count())
					{
						flag3 = true;
					}
					else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && AdventureState.MapState.MapParty.SelectedCharacters.All((CMapCharacter it) => it.CharacterGold >= goldValue))
					{
						flag3 = true;
					}
				}
				else if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold && AdventureState.MapState.MapParty.PartyGold >= goldValue)
				{
					flag3 = true;
				}
				else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && AdventureState.MapState.MapParty.SelectedCharacters.Sum((CMapCharacter it) => it.CharacterGold) >= goldValue)
				{
					flag3 = true;
				}
			}
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.HaveItem)
		{
			flag4 = false;
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				foreach (string item in matchConditions.Items)
				{
					if (selectedCharacter.CheckEquippedItems.Any((CItem it) => it.YMLData.StringID == item))
					{
						flag4 = true;
					}
					if (selectedCharacter.CheckBoundItems.Any((CItem it) => it.YMLData.StringID == item))
					{
						flag4 = true;
					}
				}
			}
		}
		return flag && flag2 && flag3 && flag4 && flag5;
	}

	public static bool DoesGroupMatch(RewardGroup reward, CRoadEventTreasureCondition matchConditions)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		flag4 = reward.GainedDamage == matchConditions.MatchConditions.GainedDamage;
		flag5 = matchConditions.MatchConditions.Conditions.Count == 0 || reward.Conditions.Any((RewardCondition x) => matchConditions.MatchConditions.Conditions.Any((RewardCondition y) => x.ToString() == y.ToString()));
		flag6 = matchConditions.MatchConditions.EnemyConditions.Count == 0 || reward.EnemyConditions.Any((RewardCondition x) => matchConditions.MatchConditions.EnemyConditions.Any((RewardCondition y) => x.ToString() == y.ToString()));
		flag7 = matchConditions.MatchConditions.Infusions.Count == 0 || reward.Infusions.Any((ElementInfusionBoardManager.EElement x) => matchConditions.MatchConditions.Infusions.Any((ElementInfusionBoardManager.EElement y) => x.ToString() == y.ToString()));
		if (matchConditions.TreasureCondition == ETreasureCondition.Gained)
		{
			flag = reward.GainedGold == matchConditions.MatchConditions.GainedGold;
			flag2 = reward.GainedXP == matchConditions.MatchConditions.GainedXP;
			flag8 = reward.GainedProsperity == matchConditions.MatchConditions.GainedProsperity;
			flag9 = reward.GainedReputation == matchConditions.MatchConditions.GainedReputation;
			if (matchConditions.MatchConditions.Items.Count > 0)
			{
				foreach (string itemName in matchConditions.MatchConditions.Items)
				{
					List<Reward> list = reward.Rewards.Where((Reward x) => x.Item != null && x.Item.Name == itemName).ToList();
					if (list.Count > 0 && list.Sum((Reward x) => x.Amount) > 0)
					{
						flag3 = true;
						break;
					}
				}
			}
			else
			{
				flag3 = true;
			}
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.Lost)
		{
			flag = reward.LostGold == matchConditions.MatchConditions.LostGold;
			flag2 = reward.LostXP == matchConditions.MatchConditions.LostXP;
			flag8 = reward.LostProsperity == matchConditions.MatchConditions.LostProsperity;
			flag9 = reward.LostReputation == matchConditions.MatchConditions.LostReputation;
			if (matchConditions.MatchConditions.Items.Count > 0)
			{
				foreach (string itemName2 in matchConditions.MatchConditions.Items)
				{
					List<Reward> list2 = reward.Rewards.Where((Reward x) => x.Item != null && x.Item.Name == itemName2).ToList();
					if (list2.Count > 0 && list2.Sum((Reward x) => x.Amount) < 0)
					{
						flag3 = true;
						break;
					}
				}
			}
			else
			{
				flag3 = true;
			}
		}
		else if (matchConditions.TreasureCondition == ETreasureCondition.RollResult)
		{
			flag10 = true;
		}
		return (flag && flag2 && flag4 && flag3 && flag5 && flag6 && flag7 && flag8 && flag9) || flag10;
	}

	public static bool DoGroupsMatch(List<RewardGroup> rewards, CRoadEventTreasureCondition matchConditions, int rollResult)
	{
		foreach (RewardGroup reward in rewards)
		{
			if ((matchConditions.MatchConditions.FromTreasureTables == null || matchConditions.MatchConditions.FromTreasureTables.Count <= 0 || matchConditions.MatchConditions.FromTreasureTables.Contains(reward.TreasureTable)) && DoesGroupMatch(reward, matchConditions) && DoesRulesMatch(matchConditions, rollResult))
			{
				return true;
			}
		}
		if (rewards.Count < 1 && DoesRulesMatch(matchConditions, rollResult))
		{
			return true;
		}
		return false;
	}

	public static bool ProcessEventTreasureCondition(MappingEntry mapEntry, string fileName, out CRoadEventTreasureCondition roadEventTreasureCondition)
	{
		roadEventTreasureCondition = null;
		bool flag = true;
		CRoadEventScreen cRoadEventScreen = null;
		RewardGroup rewardGroup = new RewardGroup();
		ETreasureCondition eTreasureCondition = TreasureConditions.SingleOrDefault((ETreasureCondition x) => x.ToString() == mapEntry.Key.ToString());
		List<string> list = null;
		CEqualityFilter cEqualityFilter = null;
		string text = null;
		List<string> list2 = null;
		int num = 0;
		int minValue = 0;
		int maxValue = 0;
		if (eTreasureCondition == ETreasureCondition.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid treasure condition.  Unable to process entry.  File:" + fileName);
			return false;
		}
		if (YMLShared.GetMapping(mapEntry, fileName, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "FromTreasureTable":
				case "FromTreasureTables":
				{
					if (YMLShared.GetStringList(entry.Value, "FromTreasureTables", fileName, out var values4))
					{
						rewardGroup.FromTreasureTables = values4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Item":
				case "Items":
				{
					if (YMLShared.GetStringList(entry.Value, "Items", fileName, out var values2))
					{
						if (eTreasureCondition == ETreasureCondition.HaveItem)
						{
							list2 = values2;
						}
						else
						{
							rewardGroup.Items = values2;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "XP":
					switch (eTreasureCondition)
					{
					case ETreasureCondition.Gained:
						rewardGroup.GainedXP = true;
						break;
					case ETreasureCondition.Lost:
						rewardGroup.LostXP = true;
						break;
					default:
						flag = false;
						break;
					}
					break;
				case "Gold":
					switch (eTreasureCondition)
					{
					case ETreasureCondition.Gained:
						rewardGroup.GainedGold = true;
						break;
					case ETreasureCondition.Lost:
						rewardGroup.LostGold = true;
						break;
					default:
						flag = false;
						break;
					}
					break;
				case "Damage":
					if (eTreasureCondition == ETreasureCondition.Gained)
					{
						rewardGroup.GainedDamage = true;
					}
					else
					{
						flag = false;
					}
					break;
				case "Condition":
				case "Conditions":
				{
					if (YMLShared.GetStringList(entry.Value, "Conditions", fileName, out var values6))
					{
						rewardGroup.AddConditions(values6, forEnemy: false);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "EnemyCondition":
				case "EnemyConditions":
				{
					if (YMLShared.GetStringList(entry.Value, "Conditions", fileName, out var values5))
					{
						rewardGroup.AddConditions(values5, forEnemy: true);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Infuse":
				case "Infusion":
				case "Infusions":
				{
					if (YMLShared.GetStringList(entry.Value, "Infusions", fileName, out var values))
					{
						rewardGroup.AddInfusions(values);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Screen":
				{
					if (CRoadEventScreen.ProcessScreensEntry(mapEntry.Key.ToString(), entry, fileName, out var roadEventScreen))
					{
						cRoadEventScreen = roadEventScreen;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ClassIDs":
				{
					if (YMLShared.GetStringList(entry.Value, "ClassIDs", fileName, out var values3))
					{
						foreach (string character in values3)
						{
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == character) == null)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Class:" + character + " specified for Event condition.  File: " + fileName);
								flag = false;
							}
						}
						if (flag)
						{
							list = values3;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Amount":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Amount", fileName, out var value))
					{
						if (eTreasureCondition == ETreasureCondition.Reputation)
						{
							if (CardProcessingShared.GetEqualityFilter(value, "Amount", fileName, out var equalityFilter))
							{
								cEqualityFilter = equalityFilter;
							}
							else
							{
								flag = false;
							}
						}
						if (eTreasureCondition != ETreasureCondition.MinGold)
						{
							break;
						}
						if (YMLShared.GetStringPropertyValue(entry.Value, "Gold", fileName, out var value2))
						{
							int result;
							if (value2.Contains("Each"))
							{
								text = value2;
							}
							else if (int.TryParse(value2.Replace("Each", ""), out result))
							{
								text = value2;
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Player":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Player", fileName, out var value5))
					{
						num = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MinValue":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "MinValue", fileName, out var value4))
					{
						minValue = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MaxValue":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "MaxValue", fileName, out var value3))
					{
						maxValue = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Key " + entry.Key?.ToString() + " under " + mapEntry.Key?.ToString() + ".  File: " + fileName);
					flag = false;
					break;
				}
			}
			if (cRoadEventScreen == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No screen found for Treasure Condition.  File: " + fileName);
				flag = false;
			}
			if (!rewardGroup.HasValues() && list == null && cEqualityFilter == null && text == null && list2 == null && num == 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No match conditions found for Treasure Condition.  File: " + fileName);
				flag = false;
			}
			if (flag)
			{
				roadEventTreasureCondition = new CRoadEventTreasureCondition(cRoadEventScreen, rewardGroup, eTreasureCondition, num, minValue, maxValue, list, cEqualityFilter, text, list2);
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
