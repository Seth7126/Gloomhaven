using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Shared;

public class CUnlockCondition
{
	public static readonly EUnlockConditionType[] UnlockConditionTypes = (EUnlockConditionType[])Enum.GetValues(typeof(EUnlockConditionType));

	private List<CPlayerStatsScenario> m_PlayerStatsScenarioToUse;

	public CChapter Chapter { get; private set; }

	public int? Prosperity { get; private set; }

	public int? Reputation { get; private set; }

	public int? Gold { get; private set; }

	public int? PersonalQuestStep { get; private set; }

	public List<Tuple<string, int>> RequiredHeroes { get; private set; }

	public List<Tuple<EUnlockConditionType, string>> RequiredConditions { get; private set; }

	public List<Tuple<EUnlockConditionType, int>> RequiredConditionsCount { get; private set; }

	public List<Tuple<EUnlockConditionType, int>> RequiredConditionsTotal { get; private set; }

	public List<int> RequiredConditionsTotalMet { get; private set; }

	public List<CUnlockChoiceContainer> RequiredChoiceContainer { get; private set; }

	public List<CUnlockConditionTarget> Targets { get; private set; }

	public List<EAdventureDifficulty> Difficulty { get; private set; }

	public int TargetsRequired { get; private set; }

	public bool Ordered { get; private set; }

	public bool SingleScenario { get; private set; }

	public bool CheckConditions(string overrideCharacterID, out int totalConditions, out int conditionsMet)
	{
		totalConditions = 0;
		conditionsMet = 0;
		try
		{
			bool isUnlocked = true;
			if (Chapter != null)
			{
				totalConditions++;
				if (!AdventureState.MapState.UnlockedChapters.Any((CChapter c) => c.Chapter == Chapter.Chapter && c.SubChapter == Chapter.SubChapter))
				{
					isUnlocked = false;
				}
				else
				{
					conditionsMet++;
				}
			}
			if (Prosperity.HasValue)
			{
				totalConditions++;
				if (AdventureState.MapState.MapParty.ProsperityLevel < Prosperity)
				{
					isUnlocked = false;
				}
				else
				{
					conditionsMet++;
				}
			}
			if (Reputation.HasValue)
			{
				totalConditions++;
				if (Reputation >= 0 && AdventureState.MapState.MapParty.Reputation < Reputation)
				{
					isUnlocked = false;
				}
				else if (Reputation < 0 && AdventureState.MapState.MapParty.Reputation > Reputation)
				{
					isUnlocked = false;
				}
				else
				{
					conditionsMet++;
				}
			}
			if (Gold.HasValue)
			{
				totalConditions++;
				if (AdventureState.MapState.MapParty.PartyGold < Gold)
				{
					isUnlocked = false;
				}
				else
				{
					conditionsMet++;
				}
			}
			if (Difficulty != null)
			{
				totalConditions++;
				if (!Difficulty.Contains(AdventureState.MapState.DifficultySetting))
				{
					isUnlocked = false;
				}
				else
				{
					conditionsMet++;
				}
			}
			if (RequiredHeroes != null && RequiredHeroes.Count > 0)
			{
				totalConditions++;
				conditionsMet++;
				List<CMapCharacter> checkCharacters = AdventureState.MapState.MapParty.CheckCharacters;
				foreach (Tuple<string, int> req in RequiredHeroes)
				{
					bool flag = false;
					foreach (CMapCharacter item in checkCharacters.FindAll((CMapCharacter x) => x.CharacterID == req.Item1))
					{
						if (item.Level >= req.Item2)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						isUnlocked = false;
						conditionsMet--;
						break;
					}
				}
			}
			if (RequiredConditionsTotal != null)
			{
				for (int num = 0; num < RequiredConditionsTotal.Count; num++)
				{
					totalConditions += RequiredConditionsTotal[num].Item2;
					RequiredConditionsTotalMet[num + 1] = 0;
				}
			}
			TestRequiredConditions(RequiredConditions, RequiredConditionsTotal, RequiredConditionsTotalMet, overrideCharacterID, ref totalConditions, ref conditionsMet, ref isUnlocked);
			if (RequiredConditionsCount != null)
			{
				foreach (Tuple<EUnlockConditionType, int> item2 in RequiredConditionsCount)
				{
					totalConditions += item2.Item2;
					conditionsMet += item2.Item2;
					switch (item2.Item1)
					{
					case EUnlockConditionType.CompletedAchievement:
					{
						int num4 = AdventureState.MapState.MapParty.Achievements.Where((CPartyAchievement w) => w.State == EAchievementState.RewardsClaimed).Count();
						if (item2.Item2 > num4)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num4;
						}
						break;
					}
					case EUnlockConditionType.UnlockedAchievement:
					{
						int num7 = AdventureState.MapState.MapParty.Achievements.Where((CPartyAchievement w) => w.State == EAchievementState.Unlocked || w.State == EAchievementState.Completed).Count();
						if (item2.Item2 > num7)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num7;
						}
						break;
					}
					case EUnlockConditionType.UnlockedVillage:
					{
						int num3 = AdventureState.MapState.AllVillages.Where((CLocationState w) => w.LocationState == ELocationState.Unlocked || w.LocationState == ELocationState.Completed).Count();
						if (item2.Item2 > num3)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num3;
						}
						break;
					}
					case EUnlockConditionType.CompletedQuest:
					{
						int num5 = AdventureState.MapState.AllQuests.Where((CQuestState w) => w.QuestState == CQuestState.EQuestState.Completed).Count();
						if (item2.Item2 > num5)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num5;
						}
						break;
					}
					case EUnlockConditionType.Retirement:
					{
						int count = AdventureState.MapState.MapParty.RetiredCharacterRecords.Count;
						if (item2.Item2 > count)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - count;
						}
						break;
					}
					case EUnlockConditionType.EnhancedMercenaries:
					{
						int num6 = 0;
						foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
						{
							if (selectedCharacter.Enhancements.Count((CEnhancement x) => x.Enhancement != EEnhancement.NoEnhancement) > 0)
							{
								num6++;
							}
						}
						if (item2.Item2 > num6)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num6;
						}
						break;
					}
					case EUnlockConditionType.UnlockClass:
					{
						int count2 = AdventureState.MapState.MapParty.UnlockedCharacterIDs.Count;
						if (item2.Item2 > count2)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - count2;
						}
						break;
					}
					case EUnlockConditionType.UnlockedQuest:
					{
						int num2 = AdventureState.MapState.AllQuests.Where((CQuestState w) => w.QuestState == CQuestState.EQuestState.Unlocked || w.QuestState == CQuestState.EQuestState.Completed).Count();
						if (item2.Item2 > num2)
						{
							isUnlocked = false;
							conditionsMet -= item2.Item2 - num2;
						}
						break;
					}
					default:
						DLLDebug.LogError("Unsupported UnlockConditionType '" + item2.Item1.ToString() + "'");
						break;
					}
				}
			}
			if (RequiredChoiceContainer != null && RequiredChoiceContainer.Count > 0)
			{
				bool flag2 = false;
				bool flag3 = true;
				int num8 = 0;
				int num9 = 0;
				bool flag4 = true;
				foreach (CUnlockChoiceContainer item3 in RequiredChoiceContainer)
				{
					flag4 = item3.OROperator;
					int totalConditions2 = 0;
					int conditionsMet2 = 0;
					bool isUnlocked2 = true;
					if (item3.RequiredConditionsTotal != null)
					{
						for (int num10 = 0; num10 < item3.RequiredConditionsTotal.Count; num10++)
						{
							totalConditions2 += item3.RequiredConditionsTotal[num10].Item2;
							item3.RequiredConditionsTotalMet[num10 + 1] = 0;
						}
					}
					TestRequiredConditions(item3.RequiredConditions, item3.RequiredConditionsTotal, item3.RequiredConditionsTotalMet, overrideCharacterID, ref totalConditions2, ref conditionsMet2, ref isUnlocked2);
					if (isUnlocked2 && flag3)
					{
						flag2 = true;
						if (item3.OROperator)
						{
							num8 = totalConditions2;
							num9 = conditionsMet2;
						}
						else
						{
							num8 += totalConditions2;
							num9 += conditionsMet2;
						}
					}
					if (!isUnlocked2 && !item3.OROperator)
					{
						flag2 = false;
						num8 += totalConditions2;
					}
					if (!flag2)
					{
						num8 = Math.Max(num8, totalConditions2);
						num9 = Math.Max(num9, conditionsMet2);
					}
				}
				if (!flag4 && num9 < num8)
				{
					isUnlocked = false;
				}
				isUnlocked = flag2 && isUnlocked;
				totalConditions += num8;
				conditionsMet += num9;
			}
			return isUnlocked;
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception during CheckConditions comparison\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public void TestRequiredConditions(List<Tuple<EUnlockConditionType, string>> requiredConditions, List<Tuple<EUnlockConditionType, int>> requiredConditionsTotal, List<int> requiredConditionsTotalMet, string overrideCharacterID, ref int totalConditions, ref int conditionsMet, ref bool isUnlocked)
	{
		if (requiredConditions == null)
		{
			return;
		}
		foreach (Tuple<EUnlockConditionType, string> req in requiredConditions)
		{
			int num = -1;
			for (int i = 0; i < requiredConditionsTotal.Count; i++)
			{
				if (requiredConditionsTotal[i].Item1 == req.Item1)
				{
					num = i;
				}
			}
			if (num < 0)
			{
				totalConditions++;
				conditionsMet++;
			}
			num++;
			requiredConditionsTotalMet[num]++;
			switch (req.Item1)
			{
			case EUnlockConditionType.CompletedAchievement:
			{
				CPartyAchievement cPartyAchievement3 = AdventureState.MapState.MapParty.Achievements.SingleOrDefault((CPartyAchievement s) => s.ID == req.Item2);
				if (cPartyAchievement3 == null || cPartyAchievement3.State < EAchievementState.RewardsClaimed)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.NotCompletedAchievement:
			{
				CPartyAchievement cPartyAchievement2 = AdventureState.MapState.MapParty.Achievements.SingleOrDefault((CPartyAchievement s) => s.ID == req.Item2);
				if (cPartyAchievement2 != null && cPartyAchievement2.State >= EAchievementState.RewardsClaimed)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.Retirement:
			{
				CPlayerRecords cPlayerRecords = null;
				if (req.Item2 != "ANY")
				{
					cPlayerRecords = AdventureState.MapState.MapParty.RetiredCharacterRecords.FirstOrDefault((CPlayerRecords s) => s.CharacterID == req.Item2);
				}
				if ((req.Item2 != "ANY" && cPlayerRecords == null) || (req.Item2 == "ANY" && (AdventureState.MapState.MapParty.RetiredCharacterRecords == null || AdventureState.MapState.MapParty.RetiredCharacterRecords.Count == 0)))
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.UnlockClass:
				if (!AdventureState.MapState.MapParty.UnlockedCharacterIDs.Contains(req.Item2))
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			case EUnlockConditionType.UnlockedAchievement:
			{
				CPartyAchievement cPartyAchievement = AdventureState.MapState.MapParty.Achievements.SingleOrDefault((CPartyAchievement s) => s.ID == req.Item2);
				if (cPartyAchievement == null || cPartyAchievement.State < EAchievementState.Unlocked)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.UnlockedVillage:
			{
				CLocationState cLocationState = AdventureState.MapState.AllLocations.SingleOrDefault((CLocationState s) => s.ID == req.Item2);
				if (cLocationState == null || cLocationState.LocationState < ELocationState.Unlocked)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.CompletedQuest:
			{
				CQuestState cQuestState3 = AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState s) => s.ID == req.Item2);
				if (cQuestState3 == null || cQuestState3.QuestState < CQuestState.EQuestState.Completed || cQuestState3.QuestState == CQuestState.EQuestState.Blocked)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				else
				{
					if (overrideCharacterID == null)
					{
						break;
					}
					CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == overrideCharacterID);
					if (cQuestState3.CompleteCharacterNames == null || !cQuestState3.CompleteCharacterNames.Contains(cMapCharacter?.CharacterName))
					{
						if (num < 1)
						{
							isUnlocked = false;
							conditionsMet--;
						}
						requiredConditionsTotalMet[num]--;
					}
				}
				break;
			}
			case EUnlockConditionType.NotCompletedQuest:
			{
				CQuestState cQuestState = AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState s) => s.ID == req.Item2);
				if (cQuestState != null && cQuestState.QuestState >= CQuestState.EQuestState.Completed)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.UnlockedQuest:
			{
				CQuestState cQuestState2 = AdventureState.MapState.AllQuests.SingleOrDefault((CQuestState s) => s.ID == req.Item2);
				if (cQuestState2 == null || cQuestState2.QuestState < CQuestState.EQuestState.Unlocked || cQuestState2.QuestState == CQuestState.EQuestState.Blocked)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.MessageSeen:
			{
				CMapMessageState cMapMessageState = AdventureState.MapState.MapMessageStates.SingleOrDefault((CMapMessageState s) => s.ID == req.Item2);
				if (cMapMessageState == null || cMapMessageState.MapMessageState < CMapMessageState.EMapMessageState.Shown)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			case EUnlockConditionType.CompletedPersonalQuestStep:
			{
				bool flag = true;
				if (PersonalQuestStep.HasValue)
				{
					if (!(from c in AdventureState.MapState.MapParty.CheckCharacters.FindAll((CMapCharacter x) => x.PersonalQuest != null)
						select c.PersonalQuest into p
						where p.ID == req.Item2
						select p).ToList().Any((CPersonalQuestState x) => x.CurrentPersonalQuestStep > PersonalQuestStep.Value))
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag)
				{
					if (num < 1)
					{
						isUnlocked = false;
						conditionsMet--;
					}
					requiredConditionsTotalMet[num]--;
				}
				break;
			}
			default:
				DLLDebug.LogError("Unsupported UnlockConditionType '" + req.Item1.ToString() + "'");
				break;
			}
		}
		for (int num2 = 0; num2 < requiredConditionsTotal.Count; num2++)
		{
			if (requiredConditionsTotalMet[num2 + 1] < requiredConditionsTotal[num2].Item2)
			{
				isUnlocked = false;
			}
			if (requiredConditionsTotalMet[num2 + 1] > 0)
			{
				conditionsMet += Math.Min(requiredConditionsTotalMet[num2 + 1], requiredConditionsTotal[num2].Item2);
			}
		}
	}

	public bool CheckTarget(CUnlockConditionTarget target, int currentTargetsMet, out int targetsMet, string overrideCharacterId = null, string overrideCharacterName = null, bool useCurrentScenarioStats = false, bool checkingFailCondition = false)
	{
		m_PlayerStatsScenarioToUse = (useCurrentScenarioStats ? AdventureState.MapState.MapParty.CurrentScenarioStats.ToList() : AdventureState.MapState.MapParty.LastScenarioStats.ToList());
		List<CPlayerStatsScenario> list = AdventureState.MapState.MapParty.LastScenarioHeroSummon.ToList();
		List<CPlayerStatsScenario> list2 = AdventureState.MapState.MapParty.LastScenarioMonster.ToList();
		targetsMet = currentTargetsMet;
		bool flag = true;
		if (target != null)
		{
			List<CPlayerStats> list3 = new List<CPlayerStats>();
			List<CPlayerStats> list4 = new List<CPlayerStats>();
			switch (target.SubFilter)
			{
			case EUnlockConditionTargetSubFilter.Total:
			{
				bool flag4 = false;
				foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
				{
					if ((target.CharacterIDs == null && overrideCharacterId == null) || (target.CharacterIDs != null && target.CharacterIDs.Contains(checkCharacter.CharacterID)) || overrideCharacterId == checkCharacter.CharacterID || target.Filter == EUnlockConditionTargetFilter.FirstKill || target.Filter == EUnlockConditionTargetFilter.LastKill || target.Filter == EUnlockConditionTargetFilter.ExhaustFirst || (target.AllyType != null && target.AllyType.Contains("NotSelf")))
					{
						list3.Add(checkCharacter.PlayerStats);
						if (target.CharacterIDs != null || overrideCharacterId != null)
						{
							flag4 = true;
						}
					}
					list4.Add(checkCharacter.PlayerStats);
				}
				list3.Add(AdventureState.MapState.MapParty.MonstersStats);
				if (!flag4)
				{
					list3.Add(AdventureState.MapState.MapParty.HeroSummonsStats);
				}
				flag = CheckUnlockConditionTarget(target, list3, list4, targetsMet, overrideCharacterId, out var _, out targetsMet, null, checkingFailCondition);
				break;
			}
			case EUnlockConditionTargetSubFilter.Scenario:
			case EUnlockConditionTargetSubFilter.SingleScenario:
			{
				bool flag3 = false;
				if (m_PlayerStatsScenarioToUse != null && m_PlayerStatsScenarioToUse.Count > 0)
				{
					foreach (CPlayerStatsScenario item in m_PlayerStatsScenarioToUse)
					{
						if ((target.CharacterIDs == null && overrideCharacterId == null) || (target.CharacterIDs != null && target.CharacterIDs.Contains(item.CharacterID)) || overrideCharacterId == item.CharacterID || target.Filter == EUnlockConditionTargetFilter.FirstKill || target.Filter == EUnlockConditionTargetFilter.LastKill || target.Filter == EUnlockConditionTargetFilter.ExhaustFirst || (target.AllyType != null && target.AllyType.Contains("NotSelf")))
						{
							list3.Add(item);
							if (target.CharacterIDs != null || overrideCharacterId != null)
							{
								flag3 = true;
							}
						}
						list4.Add(item);
					}
					if (!flag3)
					{
						foreach (CPlayerStatsScenario item2 in list)
						{
							list3.Add(item2);
						}
					}
					foreach (CPlayerStatsScenario item3 in list2)
					{
						list3.Add(item3);
					}
					int conditionsMet3 = targetsMet;
					if (target.SubFilter == EUnlockConditionTargetSubFilter.SingleScenario)
					{
						conditionsMet3 = 0;
					}
					flag = CheckUnlockConditionTarget(target, list3, list4, conditionsMet3, overrideCharacterId, out var _, out conditionsMet3, null, checkingFailCondition);
					targetsMet = conditionsMet3;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case EUnlockConditionTargetSubFilter.Round:
			case EUnlockConditionTargetSubFilter.RoundNoReset:
				try
				{
					int num = 0;
					int val = 0;
					int num2 = 0;
					if (m_PlayerStatsScenarioToUse != null && m_PlayerStatsScenarioToUse.Count > 0)
					{
						num = m_PlayerStatsScenarioToUse[0].RoundsPlayed;
						for (int i = 0; i <= num; i++)
						{
							int requiredRound = i + 1;
							bool flag2 = false;
							list3.Clear();
							list4.Clear();
							foreach (CPlayerStatsScenario item4 in m_PlayerStatsScenarioToUse)
							{
								if ((target.CharacterIDs == null && overrideCharacterId == null) || (target.CharacterIDs != null && target.CharacterIDs.Contains(item4.CharacterID)) || overrideCharacterId == item4.CharacterID || target.Filter == EUnlockConditionTargetFilter.FirstKill || target.Filter == EUnlockConditionTargetFilter.LastKill || target.Filter == EUnlockConditionTargetFilter.ExhaustFirst || (target.AllyType != null && target.AllyType.Contains("NotSelf")))
								{
									list3.Add(item4.GetPlayerStatsForRound(requiredRound));
									if (target.CharacterIDs != null || overrideCharacterId != null)
									{
										flag2 = true;
									}
								}
								list4.Add(item4.GetPlayerStatsForRound(requiredRound));
							}
							if (!flag2)
							{
								foreach (CPlayerStatsScenario item5 in list)
								{
									list3.Add(item5.GetPlayerStatsForRound(requiredRound));
								}
							}
							foreach (CPlayerStatsScenario item6 in list2)
							{
								list3.Add(item6.GetPlayerStatsForRound(requiredRound));
							}
							int conditionsMet = 0;
							if (target.SubFilter == EUnlockConditionTargetSubFilter.RoundNoReset)
							{
								conditionsMet = targetsMet;
							}
							flag = CheckUnlockConditionTarget(target, list3, list4, conditionsMet, overrideCharacterId, out var totalConditions, out conditionsMet, null, checkingFailCondition);
							val = Math.Max(val, totalConditions);
							num2 = Math.Max(num2, conditionsMet);
							if (flag)
							{
								break;
							}
						}
						targetsMet = num2;
					}
					else if (target.SubFilter == EUnlockConditionTargetSubFilter.Round)
					{
						targetsMet = 0;
						flag = false;
					}
					else if (target.SubFilter == EUnlockConditionTargetSubFilter.RoundNoReset)
					{
						list3.Clear();
						list4.Clear();
						int conditionsMet2 = targetsMet;
						flag = CheckUnlockConditionTarget(target, list3, list4, conditionsMet2, overrideCharacterId, out var totalConditions2, out conditionsMet2, null, checkingFailCondition);
						val = Math.Max(val, totalConditions2);
						num2 = Math.Max(num2, conditionsMet2);
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception checking Condition, Target, Round for Filter: " + target.Filter.ToString() + "\n" + ex.Message);
				}
				break;
			}
		}
		return flag;
	}

	private bool CheckUnlockConditionTarget(CUnlockConditionTarget unlockConditionTarget, List<CPlayerStats> comparePlayerStats, List<CPlayerStats> allPlayerStats, int currentConditionsMet, string overrideCharacterId, out int totalConditions, out int conditionsMet, CPartyAchievement achievement = null, bool checkingFailCondition = false)
	{
		totalConditions = unlockConditionTarget.Value;
		conditionsMet = currentConditionsMet;
		bool result = true;
		int tally = 0;
		switch (unlockConditionTarget.Filter)
		{
		case EUnlockConditionTargetFilter.FirstKill:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> source2 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Kills).ToList();
			List<CPlayerStatsKill> killStats = source2.OrderBy((CPlayerStatsKill x) => x.TimeStamp).ToList();
			int num38 = 0;
			if (killStats.Count > 0 && unlockConditionTarget.Targets > 0)
			{
				killStats.RemoveAll((CPlayerStatsKill x) => x.TimeStamp != killStats[0].TimeStamp);
				if (killStats.Find((CPlayerStatsKill x) => x.ActingClassID == overrideCharacterId) != null)
				{
					num38 = 1;
				}
				else if (AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == killStats[0].ActingClassID) != null)
				{
					SimpleLog.AddToSimpleLog("Opener Battle Goal failed, " + killStats[0].ActingClassID + " has first kill, not " + overrideCharacterId);
					num38 = 2;
				}
			}
			if (conditionsMet == 0)
			{
				conditionsMet = num38;
			}
			if (unlockConditionTarget.TargetFilter != null && !unlockConditionTarget.TargetFilter.Compare(conditionsMet))
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.LastKill:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> source3 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Kills).ToList();
			List<CPlayerStatsKill> killStats2 = source3.OrderByDescending((CPlayerStatsKill x) => x.TimeStamp).ToList();
			List<CPlayerStatsDamage> actors15 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			int num57 = 0;
			if (CheckScenarioResult(unlockConditionTarget, actors15) && killStats2.Count > 0 && unlockConditionTarget.Targets > 0)
			{
				killStats2.RemoveAll((CPlayerStatsKill x) => x.TimeStamp != killStats2.First().TimeStamp);
				if (killStats2.Find((CPlayerStatsKill x) => x.ActingClassID == overrideCharacterId) != null)
				{
					num57 = 1;
				}
				else if (AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == killStats2[0].ActingClassID) != null)
				{
					SimpleLog.AddToSimpleLog("Closer Battle Goal failed, " + killStats2.Last().ActingClassID + " has last kill, not " + overrideCharacterId);
					num57 = 2;
				}
			}
			if (conditionsMet == 0)
			{
				conditionsMet = num57;
			}
			if (unlockConditionTarget.TargetFilter != null && !unlockConditionTarget.TargetFilter.Compare(conditionsMet))
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Kill:
		case EUnlockConditionTargetFilter.KillDamaged:
		case EUnlockConditionTargetFilter.KillDisadvantaged:
		case EUnlockConditionTargetFilter.KillAdjacent:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> Stats9 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Kills).ToList();
			FilterKillStats(ref Stats9, unlockConditionTarget, "Kill Achievement", killed: true);
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.KillDisadvantaged)
			{
				Stats9.RemoveAll((CPlayerStatsKill x) => !x.Disadvantaged);
			}
			int num25 = Stats9.Count;
			List<CPlayerStatsDamage> actors6 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			bool flag8 = false;
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.KillDamaged)
			{
				num25 = 0;
				List<CPlayerStatsDamage> Stats10 = allPlayerStats.SelectMany((CPlayerStats x) => x.DamageDealt).ToList();
				FilterDamageStats(ref Stats10, unlockConditionTarget, "Deal Damage Achievement", dealt: true);
				foreach (CPlayerStatsKill kill in Stats9)
				{
					if (Stats10.Find((CPlayerStatsDamage x) => x.ActedOnClassID == kill.ActedOnClassID && x.ActedOnGUID == kill.ActedOnGUID && x.ActingClassID != kill.ActingClassID && x.Round == kill.Round) != null)
					{
						num25++;
					}
				}
			}
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.KillAdjacent)
			{
				num25 = 0;
				foreach (CPlayerStatsKill item3 in Stats9)
				{
					int num26 = 1;
					bool flag9 = false;
					if (!item3.TargetAdjacent)
					{
						if (unlockConditionTarget.Adjacency.Contains("Wall") && item3.WallAdjacent)
						{
							flag9 = true;
						}
						if (unlockConditionTarget.Adjacency.Contains("Obstacle") && item3.ObstacleAdjacent > 0)
						{
							flag9 = true;
							num26 = item3.ObstacleAdjacent;
						}
						if (unlockConditionTarget.Adjacency.Contains("Ally") && item3.AllyAdjacent > 0)
						{
							flag9 = true;
							num26 = item3.AllyAdjacent;
						}
						if (unlockConditionTarget.Adjacency.Contains("Enemy") && item3.EnemyAdjacent > 0)
						{
							flag9 = true;
							num26 = item3.EnemyAdjacent;
						}
						if (flag9 && num26 >= unlockConditionTarget.Targets)
						{
							num25 = 1;
							break;
						}
					}
				}
			}
			if (CheckScenarioResult(unlockConditionTarget, actors6) && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(Stats9.Count))
			{
				flag8 = true;
			}
			conditionsMet += num25;
			progress(achievement?.ID, num25, unlockConditionTarget.Value);
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag8)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.NotKill:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> Stats2 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Kills).ToList();
			FilterKillStats(ref Stats2, unlockConditionTarget, "Kill Achievement", killed: true);
			int count = Stats2.Count;
			List<CPlayerStatsDamage> actors3 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			bool flag5 = false;
			if (CheckScenarioResult(unlockConditionTarget, actors3) && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(Stats2.Count))
			{
				flag5 = true;
			}
			conditionsMet += count;
			progress(achievement?.ID, count, unlockConditionTarget.Value);
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag5)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.TurnsWithoutAdjacency:
		case EUnlockConditionTargetFilter.TurnsWithAdjacency:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsEndTurn> list5 = comparePlayerStats.SelectMany((CPlayerStats x) => x.EndTurn).ToList();
			int num28 = 0;
			int num29 = ((unlockConditionTarget.Filter == EUnlockConditionTargetFilter.TurnsWithAdjacency) ? 1 : 0);
			int num30 = ((unlockConditionTarget.Filter != EUnlockConditionTargetFilter.TurnsWithAdjacency) ? 1 : 0);
			_ = unlockConditionTarget.Filter;
			_ = 46;
			foreach (CPlayerStatsEndTurn item4 in list5)
			{
				bool flag10 = false;
				if (unlockConditionTarget.Adjacency.Contains("Wall") && item4.WallAdjacent)
				{
					flag10 = true;
				}
				if (unlockConditionTarget.Adjacency.Contains("Obstacle") && item4.ObstacleAdjacent > 0)
				{
					flag10 = true;
				}
				if (unlockConditionTarget.Adjacency.Contains("Ally") && item4.AllyAdjacent > 0)
				{
					flag10 = true;
				}
				if (unlockConditionTarget.Adjacency.Contains("Enemy") && item4.EnemyAdjacent > 0)
				{
					flag10 = true;
				}
				num28 = ((!flag10) ? (num28 + num30) : (num28 + num29));
			}
			List<CPlayerStatsDamage> actors7 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			bool flag11 = false;
			if (CheckScenarioResult(unlockConditionTarget, actors7) && unlockConditionTarget.TargetFilter != null)
			{
				if (unlockConditionTarget.TargetFilter.Compare(num28))
				{
					flag11 = true;
					num28 = 1;
				}
				else
				{
					num28 = 0;
				}
			}
			conditionsMet = num28;
			progress(achievement?.ID, num28, unlockConditionTarget.Value);
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag11)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.LostAdjacency:
		{
			test(" K ", 1, ref tally, achievement?.ID);
			int num56 = comparePlayerStats.SelectMany((CPlayerStats x) => x.LostAdjacency).ToList().Count;
			List<CPlayerStatsDamage> actors14 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			bool flag14 = false;
			if (CheckScenarioResult(unlockConditionTarget, actors14) && unlockConditionTarget.TargetFilter != null)
			{
				if (unlockConditionTarget.TargetFilter.Compare(num56))
				{
					flag14 = true;
					num56 = 1;
				}
				else
				{
					num56 = 0;
				}
			}
			conditionsMet = num56;
			progress(achievement?.ID, num56, unlockConditionTarget.Value);
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag14)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Die:
		{
			test(" D ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> Stats13 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Deaths).ToList();
			FilterKillStats(ref Stats13, unlockConditionTarget, "Die Achievement", killed: false);
			int count5 = Stats13.Count;
			conditionsMet += count5;
			progress(achievement?.ID, count5, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Exhaust:
		case EUnlockConditionTargetFilter.ExhaustFirst:
		{
			test(" X ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsKill> deathStats = comparePlayerStats.SelectMany((CPlayerStats x) => x.Deaths).ToList();
			List<CPlayerStatsDamage> list11 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (overrideCharacterId != null && unlockConditionTarget.AllyType != null && unlockConditionTarget.AllyType.Contains("NotSelf"))
			{
				unlockConditionTarget.ExcludeCharacterIDs = new List<string> { overrideCharacterId };
			}
			FilterKillStats(ref deathStats, unlockConditionTarget, "Exhaust Achievement", killed: false);
			int num58 = 0;
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.ExhaustFirst)
			{
				if (deathStats.Count > 0 && deathStats.Find((CPlayerStatsKill x) => x.ActedOnClassID != null && overrideCharacterId != null && x.ActedOnClassID.Contains(overrideCharacterId)) != null)
				{
					deathStats = deathStats.OrderBy((CPlayerStatsKill x) => x.TimeStamp).ToList();
					deathStats.RemoveAll((CPlayerStatsKill x) => x.TimeStamp != deathStats.First().TimeStamp);
					if (deathStats.Find((CPlayerStatsKill x) => x.ActedOnClassID == overrideCharacterId) != null)
					{
						num58 = 1;
					}
					else if (AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == deathStats[0].ActedOnClassID) != null)
					{
						SimpleLog.AddToSimpleLog("Weakling Battle Goal failed, " + deathStats.First().ActedOnClassID + " exhausted first, not " + overrideCharacterId);
						num58 = 2;
					}
				}
				if (conditionsMet == 0)
				{
					conditionsMet = num58;
				}
				if (unlockConditionTarget.TargetFilter != null && !unlockConditionTarget.TargetFilter.Compare(conditionsMet))
				{
					result = false;
				}
				break;
			}
			if (CheckScenarioResult(unlockConditionTarget, list11) && (unlockConditionTarget.ExcludeCharacterIDs == null || (unlockConditionTarget.ExcludeCharacterIDs != null && list11.Any((CPlayerStatsDamage x) => unlockConditionTarget.ExcludeCharacterIDs.Contains(x.ActedOnClassID)))))
			{
				if (unlockConditionTarget.TargetFilter != null)
				{
					if (unlockConditionTarget.TargetFilter.Compare(deathStats.Count))
					{
						num58 = 1;
					}
				}
				else
				{
					num58 = deathStats.Count;
				}
			}
			conditionsMet += num58;
			progress(achievement?.ID, num58, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.ReceiveDamage:
		case EUnlockConditionTargetFilter.ReceiveDamageWhileResting:
		case EUnlockConditionTargetFilter.UniqueDamage:
		{
			test(" RD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats4 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageReceived).ToList();
			FilterDamageStats(ref Stats4, unlockConditionTarget, "Receive Damage Achievement", dealt: false);
			if (unlockConditionTarget.TargetFilter != null)
			{
				for (int num8 = Stats4.Count - 1; num8 >= 0; num8--)
				{
					if (!unlockConditionTarget.TargetFilter.Compare(Stats4[num8].FinalDamageAmount))
					{
						Stats4.RemoveAt(num8);
					}
				}
			}
			if (unlockConditionTarget.SingleTarget)
			{
				foreach (CMapCharacter character2 in AdventureState.MapState.MapParty.CheckCharacters)
				{
					List<CPlayerStatsDamage> list2 = Stats4.FindAll((CPlayerStatsDamage x) => x.ActedOnClassID != null && x.ActedOnClassID == character2.CharacterID);
					if (list2 != null)
					{
						int num9 = 0;
						num9 = ((!unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? list2.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : list2.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
						if (num9 >= unlockConditionTarget.Value)
						{
							Stats4.Clear();
							Stats4.AddRange(list2);
							break;
						}
					}
				}
			}
			int num10 = 0;
			num10 = ((unlockConditionTarget.DamageSource == null || !unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? Stats4.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : Stats4.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.ReceiveDamageWhileResting)
			{
				num10 = 0;
				List<CPlayerStatsAbilities> list3 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Abilities).ToList();
				list3.RemoveAll((CPlayerStatsAbilities x) => x.AbilityType != CAbility.EAbilityType.LongRest);
				foreach (CPlayerStatsDamage dmg in Stats4)
				{
					if (list3.Find((CPlayerStatsAbilities x) => x.Round == dmg.Round) != null)
					{
						num10 += dmg.FinalDamageAmount;
					}
				}
			}
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.UniqueDamage)
			{
				List<CPlayerStatsDamage> list4 = (from x in Stats4
					group x by new { x.Round, x.ActingGUID } into g
					select g.FirstOrDefault()).ToList();
				int num11 = 0;
				int num12 = 0;
				num10 = 0;
				foreach (CPlayerStatsDamage item5 in list4)
				{
					if (item5.Round == num11)
					{
						num12++;
						continue;
					}
					if (num12 >= unlockConditionTarget.Value)
					{
						num10 = unlockConditionTarget.Value;
					}
					num11 = item5.Round;
					num12 = 1;
				}
				if (num12 >= unlockConditionTarget.Value)
				{
					num10 = unlockConditionTarget.Value;
				}
			}
			int num13 = num10;
			int num14 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num13 = Stats4.Count;
				num14 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num13 = (conditionsMet += num13);
			progress(achievement?.ID, num13, num14);
			if (num13 < num14)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Health:
		{
			test(" HH ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats20 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageReceived).ToList();
			List<CPlayerStatsDamage> list16 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			Stats20.AddRange(list16);
			FilterDamageStats(ref Stats20, unlockConditionTarget, "Receive Damage Achievement", dealt: false);
			Stats20.RemoveAll((CPlayerStatsDamage x) => x.PerformedBySummons);
			int num69 = 0;
			if (CheckScenarioResult(unlockConditionTarget, list16))
			{
				if (unlockConditionTarget.TargetPercentMaxHP != null || unlockConditionTarget.TargetFilter != null)
				{
					switch (unlockConditionTarget.ConditionalType)
					{
					case EUnlockConditionalType.ScenarioMinimum:
					{
						if (Stats20.Count <= 0)
						{
							break;
						}
						for (int num70 = ((Stats20.Count >= 2) ? ((Stats20.Last().Health == 0) ? (Stats20.Count - 2) : (Stats20.Count - 1)) : 0); num70 >= 0; num70--)
						{
							if (unlockConditionTarget.TargetPercentMaxHP != null && ((!checkingFailCondition && unlockConditionTarget.TargetPercentMaxHP.Compare(Stats20[num70].Health, Stats20[num70].MaximumHealth)) || (checkingFailCondition && !unlockConditionTarget.TargetPercentMaxHP.Compare(Stats20[num70].Health, Stats20[num70].MaximumHealth))))
							{
								Stats20.RemoveAt(num70);
							}
							if (unlockConditionTarget.TargetFilter != null && ((!checkingFailCondition && unlockConditionTarget.TargetFilter.Compare(Stats20[num70].Health)) || (checkingFailCondition && !unlockConditionTarget.TargetFilter.Compare(Stats20[num70].Health))))
							{
								Stats20.RemoveAt(num70);
							}
						}
						if (checkingFailCondition ? (Stats20.Count > 0) : (Stats20.Count <= 0))
						{
							num69 = 1;
						}
						break;
					}
					case EUnlockConditionalType.EndScenario:
						if (Stats20.Count > 0)
						{
							int index = ((Stats20.Count >= 2) ? ((Stats20.Last().Health == 0) ? (Stats20.Count - 2) : (Stats20.Count - 1)) : 0);
							if (unlockConditionTarget.TargetPercentMaxHP != null && unlockConditionTarget.TargetPercentMaxHP.Compare(Stats20[index].Health, Stats20[index].MaximumHealth))
							{
								num69 = 1;
							}
							if (unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(Stats20[index].Health))
							{
								num69 = 1;
							}
						}
						break;
					}
				}
				else
				{
					if (unlockConditionTarget.SingleTarget)
					{
						foreach (CMapCharacter character3 in AdventureState.MapState.MapParty.CheckCharacters)
						{
							List<CPlayerStatsDamage> list17 = Stats20.FindAll((CPlayerStatsDamage x) => x.ActedOnClassID != null && x.ActedOnClassID == character3.CharacterID);
							if (list17 != null)
							{
								int num71 = 0;
								num71 = ((!unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? list17.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : list17.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
								if (num71 >= unlockConditionTarget.Value)
								{
									Stats20.Clear();
									Stats20.AddRange(list17);
									break;
								}
							}
						}
					}
					num69 = ((unlockConditionTarget.DamageSource == null || !unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? Stats20.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : Stats20.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
				}
			}
			int num72 = num69;
			int num73 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num72 = Stats20.Count;
				num73 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num72 = (conditionsMet += num72);
			progress(achievement?.ID, num72, num73);
			if (num72 < num73)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.DealDamage:
		{
			test(" DD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageDealt).ToList();
			FilterDamageStats(ref Stats, unlockConditionTarget, "Deal Damage Achievement", dealt: true);
			List<CPlayerStatsDamage> actors2 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			bool flag2 = false;
			if (unlockConditionTarget.TargetFilter != null)
			{
				for (int num = Stats.Count - 1; num >= 0; num--)
				{
					if (!unlockConditionTarget.TargetFilter.Compare(Stats[num].FinalDamageAmount))
					{
						Stats.RemoveAt(num);
					}
				}
			}
			if (unlockConditionTarget.SingleActor)
			{
				bool flag3 = false;
				foreach (CMapCharacter character in AdventureState.MapState.MapParty.SelectedCharacters)
				{
					List<CPlayerStatsDamage> list = Stats.FindAll((CPlayerStatsDamage x) => x.ActingClassID != null && x.ActingClassID == character.CharacterID && (x.ActedOnClassID == null || x.ActedOnClassID != character.CharacterID));
					if (list != null)
					{
						int num2 = 0;
						num2 = ((unlockConditionTarget.DamageSource == null || !unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? list.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : list.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
						if (num2 >= unlockConditionTarget.Value)
						{
							flag3 = true;
							Stats.Clear();
							Stats.AddRange(list);
							break;
						}
					}
				}
				if (!flag3)
				{
					Stats.Clear();
				}
			}
			if (unlockConditionTarget.SingleTarget)
			{
				int num3 = 0;
				CPlayerStatsDamage item = null;
				foreach (CPlayerStatsDamage item6 in Stats)
				{
					if (item6.ActingClassID != item6.ActedOnClassID)
					{
						int num4 = ((unlockConditionTarget.DamageSource == null || !unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? item6.FinalDamageAmount : item6.PoisonDamage);
						if (num4 > num3)
						{
							item = item6;
							num3 = num4;
						}
					}
				}
				Stats.Clear();
				if (num3 > 0)
				{
					Stats.Add(item);
				}
			}
			bool? flag4 = null;
			if (unlockConditionTarget.SameTarget)
			{
				flag4 = false;
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (CPlayerStatsDamage item7 in Stats)
				{
					if (dictionary.ContainsKey(item7.ActedOnGUID))
					{
						dictionary[item7.ActedOnGUID]++;
						if (dictionary[item7.ActedOnGUID] >= unlockConditionTarget.Times)
						{
							flag4 = true;
						}
					}
					else
					{
						dictionary.Add(item7.ActedOnGUID, 1);
					}
				}
			}
			if (CheckScenarioResult(unlockConditionTarget, actors2))
			{
				flag2 = true;
			}
			int num5 = 0;
			num5 = ((unlockConditionTarget.DamageSource == null || !unlockConditionTarget.DamageSource.Contains(CCondition.ENegativeCondition.Poison.ToString())) ? Stats.Sum((CPlayerStatsDamage x) => x.FinalDamageAmount) : Stats.Sum((CPlayerStatsDamage x) => x.PoisonDamage));
			int num6 = num5;
			int num7 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num6 = Stats.Count;
				num7 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			if (flag4.HasValue)
			{
				num6 = ((flag4 == true) ? 1 : 0);
				num7 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num6 = (conditionsMet += num6);
			progress(achievement?.ID, num6, num7);
			if (unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.EqualityString.Contains("<"))
			{
				if (num6 > num7)
				{
					result = false;
				}
			}
			else if (num6 < num7)
			{
				result = false;
			}
			if (unlockConditionTarget.ScenarioResult != null && !flag2)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.UseAbility:
		{
			test(" UA ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsAbilities> Stats17 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Abilities).ToList();
			List<CPlayerStatsDamage> actors10 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			FilterAbilityStats(ref Stats17, unlockConditionTarget, "Ability Achievement", ability: true);
			int num42 = Stats17.Count;
			if (unlockConditionTarget.AbilityTypes != null && unlockConditionTarget.AbilityTypes.Contains(CAbility.EAbilityType.Move) && !unlockConditionTarget.DefaultAction)
			{
				if (unlockConditionTarget.TargetFilter != null)
				{
					for (int num43 = Stats17.Count - 1; num43 >= 0; num43--)
					{
						if (!unlockConditionTarget.TargetFilter.Compare(Stats17[num43].Amount))
						{
							Stats17.RemoveAt(num43);
						}
					}
					num42 = Stats17.Count;
				}
			}
			else if (CheckScenarioResult(unlockConditionTarget, actors10) && unlockConditionTarget.TargetFilter != null)
			{
				num42 = 0;
				if (unlockConditionTarget.TargetFilter.Compare(Stats17.Count))
				{
					num42 = 1;
				}
			}
			List<CPlayerStatsHeal> Stats18 = null;
			if (unlockConditionTarget.AbilityTypes != null && unlockConditionTarget.AbilityTypes.Contains(CAbility.EAbilityType.Heal))
			{
				Stats18 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Heals).ToList();
				FilterHealStats(ref Stats18, unlockConditionTarget, "Heal Achievement", ability: true);
				if (unlockConditionTarget.TargetFilter != null)
				{
					for (int num44 = Stats18.Count - 1; num44 >= 0; num44--)
					{
						if (!unlockConditionTarget.TargetFilter.Compare(Stats18[num44].HealAmount))
						{
							Stats18.RemoveAt(num44);
						}
					}
				}
			}
			if (Stats18 != null)
			{
				num42 += Stats18.Count;
			}
			int num45 = 0;
			int num46 = num42;
			int num47 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				if (Stats18 != null)
				{
					foreach (CPlayerStatsHeal item8 in Stats18)
					{
						num45 += item8.HealAmount;
					}
				}
				foreach (CPlayerStatsAbilities item9 in Stats17)
				{
					num45 += item9.Amount;
				}
				num46 = num45;
				num47 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num46 = (conditionsMet += num46);
			progress(achievement?.ID, num46, num47);
			if (num46 < num47)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.UseAbilityStrength:
		{
			List<CPlayerStatsAbilities> Stats15 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Abilities).ToList();
			FilterAbilityStats(ref Stats15, unlockConditionTarget, "Ability Strength Achievement", ability: true);
			break;
		}
		case EUnlockConditionTargetFilter.DestroyObstacle:
		{
			test(" DO ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDestroyObstacle> Stats8 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DestroyedObstacles).ToList();
			FilterDestroyStats(ref Stats8, unlockConditionTarget, "Destroy Obstacle Achievement", consume: true);
			int num19 = Stats8.Sum((CPlayerStatsDestroyObstacle x) => x.DestroyedObstaclesDictionary.Sum((KeyValuePair<string, int> keyValuePair) => keyValuePair.Value));
			conditionsMet += num19;
			progress(achievement?.ID, num19, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Consume:
		{
			test(" CO ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsElement> Stats14 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Consumed).ToList();
			FilterElementStats(ref Stats14, unlockConditionTarget, "Consume Achievement", consume: true);
			int count6 = Stats14.Count;
			conditionsMet += count6;
			progress(achievement?.ID, count6, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.RoundMonster:
		{
			test(" RM ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsMonsters> list6 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Monsters).ToList();
			List<CPlayerStatsDamage> actors8 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (CheckScenarioResult(unlockConditionTarget, actors8))
			{
				if (unlockConditionTarget.Targets > 0)
				{
					list6.RemoveAll((CPlayerStatsMonsters x) => x.Monsters >= unlockConditionTarget.Targets);
					if (list6.Count == 0)
					{
						conditionsMet = 1;
					}
				}
				else
				{
					list6.RemoveAll((CPlayerStatsMonsters x) => x.Monsters != unlockConditionTarget.Targets);
					if (list6.Count > 0)
					{
						conditionsMet = 1;
					}
				}
			}
			progress(achievement?.ID, conditionsMet, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.OpenDoor:
		case EUnlockConditionTargetFilter.NextRoom:
		{
			test(" OD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDoor> list12 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Door).ToList();
			List<CPlayerStatsEndTurn> source4 = comparePlayerStats.SelectMany((CPlayerStats x) => x.EndTurn).ToList();
			List<CPlayerStatsDamage> actors16 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (CheckScenarioResult(unlockConditionTarget, actors16))
			{
				if (list12.Count >= unlockConditionTarget.Targets)
				{
					conditionsMet = 1;
				}
				if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.NextRoom)
				{
					conditionsMet = 0;
					foreach (CPlayerStatsDoor door in list12)
					{
						List<CPlayerStatsEndTurn> list13 = source4.Where((CPlayerStatsEndTurn x) => x.Round == door.Round).ToList();
						foreach (CMap revealedMap in door.RevealedMaps)
						{
							foreach (CPlayerStatsEndTurn item10 in list13)
							{
								if (ScenarioManager.Tiles[item10.TileX, item10.TileY].m_HexMap == revealedMap)
								{
									conditionsMet = 1;
								}
							}
						}
					}
				}
			}
			progress(achievement?.ID, conditionsMet, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.ActivateTrap:
			try
			{
				test(" OD ", 1, ref tally, achievement?.ID);
				List<CPlayerStatsTrap> list9 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Trap).ToList();
				List<CPlayerStatsDamage> actors12 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
				if (CheckScenarioResult(unlockConditionTarget, actors12) && list9.Count >= unlockConditionTarget.Targets)
				{
					conditionsMet = list9.Count;
				}
				progress(achievement?.ID, conditionsMet, unlockConditionTarget.Value);
				if (conditionsMet < unlockConditionTarget.Value)
				{
					result = false;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception checking Condition: ActivateTrap\n" + ex.Message);
			}
			break;
		case EUnlockConditionTargetFilter.XPGain:
		{
			test(" OD ", 1, ref tally, achievement?.ID);
			int num20 = comparePlayerStats.SelectMany((CPlayerStats x) => x.XP).ToList().Sum((CPlayerStatsXP x) => x.XPEarned);
			bool flag7 = false;
			List<CPlayerStatsDamage> actors5 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (CheckScenarioResult(unlockConditionTarget, actors5) && unlockConditionTarget.TargetFilter != null)
			{
				if (unlockConditionTarget.ConditionalType == EUnlockConditionalType.EndScenario)
				{
					if (unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(num20))
					{
						flag7 = true;
					}
				}
				else if (unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(num20))
				{
					flag7 = true;
				}
			}
			conditionsMet = num20;
			progress(achievement?.ID, conditionsMet, unlockConditionTarget.Value);
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag7)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.CardsRemaining:
		{
			test(" OD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsHand> list10 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Hand).ToList();
			int compareValue = 0;
			if (list10.Count > 0)
			{
				compareValue = list10.Last().HandSize + list10.Last().DiscardSize;
			}
			List<CPlayerStatsDamage> actors13 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (CheckScenarioResult(unlockConditionTarget, actors13) && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.ConditionalType == EUnlockConditionalType.EndScenario && list10.Count > 0 && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(compareValue))
			{
				conditionsMet = 1;
			}
			progress(achievement?.ID, conditionsMet, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.LostActions:
		case EUnlockConditionTargetFilter.LostActionsBeforeRest:
		{
			test(" OD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDiscardCard> list14 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DiscardCard).ToList();
			list14.RemoveAll((CPlayerStatsDiscardCard x) => x.Pile != CBaseCard.ECardPile.Lost && x.Pile != CBaseCard.ECardPile.PermanentlyLost);
			list14.OrderBy((CPlayerStatsDiscardCard x) => x.Round);
			int num59 = list14.Count;
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LostActions)
			{
				int num60 = 0;
				int num61 = 0;
				int num62 = 0;
				foreach (CPlayerStatsDiscardCard item11 in list14)
				{
					if (item11.Round == num60)
					{
						num61++;
						continue;
					}
					if (num61 > num62)
					{
						num62 = num60;
					}
					num60 = item11.Round;
					num61 = 1;
				}
				if (num61 > num62)
				{
					num62 = num61;
				}
				num59 = ((num62 > 1) ? 1 : 0);
			}
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LostActionsBeforeRest)
			{
				num59 = list14.Count;
				List<CPlayerStatsAbilities> list15 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Abilities).ToList();
				list15.RemoveAll((CPlayerStatsAbilities x) => x.AbilityType != CAbility.EAbilityType.LongRest && x.AbilityType != CAbility.EAbilityType.LongRest);
				if (list15.Count > 0)
				{
					int restRound = list15.OrderBy((CPlayerStatsAbilities x) => x.Round).First().Round;
					list14.RemoveAll((CPlayerStatsDiscardCard x) => x.Round > restRound);
					num59 = list14.Count;
					if (num59 < unlockConditionTarget.Value)
					{
						num59 = -1;
					}
				}
			}
			int num63 = 0;
			List<CPlayerStatsDamage> actors17 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			if (CheckScenarioResult(unlockConditionTarget, actors17) && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Compare(num59) && unlockConditionTarget.Amount > 0)
			{
				num63 = 1;
			}
			int num64 = num59;
			int num65 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num64 = num63;
				num65 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num64;
			progress(achievement?.ID, num64, num65);
			if (num64 < num65)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.LoseCardToDamage:
		{
			test(" OD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsLoseCard> list7 = comparePlayerStats.SelectMany((CPlayerStats x) => x.LoseCards).ToList();
			List<CPlayerStatsDamage> actors11 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			int count8 = list7.Count;
			int num48 = 0;
			int num49 = count8;
			int num50 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				if (list7.Count > 0)
				{
					num48 = list7.OrderBy((CPlayerStatsLoseCard x) => x.DamageAvoided).Last().DamageAvoided;
				}
				num49 = num48;
				num50 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			if (CheckScenarioResult(unlockConditionTarget, actors11) && unlockConditionTarget.TargetFilter != null)
			{
				num49 = (unlockConditionTarget.TargetFilter.Compare(num49) ? 1 : 0);
			}
			conditionsMet = num49;
			progress(achievement?.ID, num49, num50);
			if (num49 < num50)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Infuse:
		{
			test(" IN ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsElement> Stats16 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Infusions).ToList();
			FilterElementStats(ref Stats16, unlockConditionTarget, "Infuse Achievement", consume: false);
			int count7 = Stats16.Count;
			conditionsMet += count7;
			progress(achievement?.ID, count7, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.ModifiersUsed:
		{
			test(" MU ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsModifiers> Stats3 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Modifiers).ToList();
			FilterModifierStats(ref Stats3, unlockConditionTarget, "Modifier Achievement", modifier: true);
			int count2 = Stats3.Count;
			conditionsMet += count2;
			progress(achievement?.ID, count2, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.UseItem:
		case EUnlockConditionTargetFilter.ItemUse:
		{
			test(" MU ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsItem> Stats12 = (from y in comparePlayerStats.SelectMany((CPlayerStats x) => x.Items).ToList()
				where y.FirstTimeUse
				select y).ToList();
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.ItemUse)
			{
				Stats12 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Items).ToList();
			}
			FilterItemStats(ref Stats12, unlockConditionTarget, "UseItem Achievement", modifier: true);
			bool flag12 = false;
			int num34 = 0;
			if (unlockConditionTarget.TargetFilter != null && overrideCharacterId != null)
			{
				CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == overrideCharacterId);
				if (cMapCharacter != null)
				{
					num34 = cMapCharacter.Level;
				}
			}
			int count4 = Stats12.Count;
			List<CPlayerStatsDamage> actors9 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			conditionsMet += count4;
			if (CheckScenarioResult(unlockConditionTarget, actors9) && unlockConditionTarget.TargetFilter != null)
			{
				if (unlockConditionTarget.TargetFilter.Compare(Stats12.Count, -1, num34))
				{
					flag12 = true;
					if (unlockConditionTarget.Value == 1)
					{
						conditionsMet = 1;
					}
				}
				else if (unlockConditionTarget.Value == 1)
				{
					conditionsMet = 0;
				}
			}
			progress(achievement?.ID, count4, unlockConditionTarget.Value);
			if (overrideCharacterId != null && unlockConditionTarget.TargetFilter != null && unlockConditionTarget.TargetFilter.Level)
			{
				unlockConditionTarget.Value = unlockConditionTarget.TargetFilter.Value + (unlockConditionTarget.TargetFilter.Level ? num34 : 0);
			}
			if (unlockConditionTarget.TargetFilter == null && conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			if (unlockConditionTarget.TargetFilter != null && !flag12)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.OwnItem:
		{
			test(" MU ", 1, ref tally, achievement?.ID);
			List<CItem> Stats22 = new List<CItem>();
			FilterOwnItemStats(ref Stats22, unlockConditionTarget, "OwnItem Achievement", modifier: true, overrideCharacterId);
			progress(total: conditionsMet = Stats22.Count, name: achievement?.ID, target: unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Loot:
		case EUnlockConditionTargetFilter.LootLeast:
		case EUnlockConditionTargetFilter.LootMost:
		case EUnlockConditionTargetFilter.LootTheKill:
		case EUnlockConditionTargetFilter.LootAdjacent:
		{
			test(" MU ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsLoot> Stats5 = allPlayerStats.SelectMany((CPlayerStats x) => x.Loot).ToList();
			List<CPlayerStatsLoot> Stats6 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Loot).ToList();
			FilterLootStats(ref Stats6, unlockConditionTarget, "Loot Achievement", modifier: true);
			FilterLootStats(ref Stats5, unlockConditionTarget, "Loot Achievement", modifier: true);
			List<CPlayerStatsDamage> actors4 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			int num15 = Stats6.Count;
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootMost || unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootLeast)
			{
				Stats5.RemoveAll((CPlayerStatsLoot x) => x.ActingClassID == overrideCharacterId);
				var source = from x in Stats5
					group x by x.ActingClassID into g
					let dcount = g.Count()
					orderby dcount descending
					select new
					{
						Value = g.Key,
						Count = dcount
					};
				int num16 = ((source.Count() > 0) ? source.First().Count : 0);
				int num17 = ((source.Count() > 0) ? source.Last().Count : 0);
				if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootMost)
				{
					num15 = ((num15 > num16) ? 1 : 0);
				}
				if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootLeast)
				{
					num15 = ((num15 < num17) ? 1 : 0);
				}
			}
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootTheKill)
			{
				List<CPlayerStatsKill> Stats7 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Kills).ToList();
				FilterKillStats(ref Stats7, unlockConditionTarget, "Kill Achievement", killed: true);
				num15 = 0;
				foreach (CPlayerStatsLoot loot in Stats6)
				{
					if (Stats7.Find((CPlayerStatsKill x) => x.ActedOnGUID == loot.ActedOnGUID && x.Round == loot.Round) != null)
					{
						num15++;
					}
				}
			}
			if (unlockConditionTarget.Filter == EUnlockConditionTargetFilter.LootAdjacent)
			{
				num15 = 0;
				foreach (CPlayerStatsLoot item12 in Stats6)
				{
					int num18 = 1;
					bool flag6 = false;
					if (unlockConditionTarget.Adjacency.Contains("Wall") && item12.WallAdjacent)
					{
						flag6 = true;
					}
					if (unlockConditionTarget.Adjacency.Contains("Obstacle") && item12.ObstacleAdjacent > 0)
					{
						flag6 = true;
						num18 = item12.ObstacleAdjacent;
					}
					if (unlockConditionTarget.Adjacency.Contains("Ally") && item12.AllyAdjacent > 0)
					{
						flag6 = true;
						num18 = item12.AllyAdjacent;
					}
					if (unlockConditionTarget.Adjacency.Contains("Enemy") && item12.EnemyAdjacent > 0)
					{
						flag6 = true;
						num18 = item12.EnemyAdjacent;
					}
					if (flag6 && num18 >= unlockConditionTarget.Targets)
					{
						num15 = 1;
						break;
					}
				}
			}
			if (CheckScenarioResult(unlockConditionTarget, actors4) && unlockConditionTarget.TargetFilter != null)
			{
				num15 = 0;
				if (unlockConditionTarget.TargetFilter.Compare(Stats6.Count))
				{
					num15 = 1;
				}
			}
			conditionsMet += num15;
			progress(achievement?.ID, num15, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Difficulty:
		{
			test(" DF ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> list8 = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			int num51 = 0;
			bool flag13 = true;
			if (unlockConditionTarget.QuestTypes != null)
			{
				flag13 = false;
				for (int num52 = list8.Count - 1; num52 >= 0; num52--)
				{
					string findqt = list8[num52].QuestType;
					EQuestType item2 = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
					if (unlockConditionTarget.QuestTypes.Contains(item2))
					{
						flag13 = true;
					}
				}
			}
			if (CheckScenarioResult(unlockConditionTarget, list8) && flag13 && unlockConditionTarget.Difficulty.Contains(AdventureState.MapState.DifficultySetting))
			{
				num51 = 1;
			}
			conditionsMet += num51;
			progress(achievement?.ID, num51, unlockConditionTarget.Value);
			if (conditionsMet < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.PierceDamage:
		{
			test(" PD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats21 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageDealt).ToList();
			FilterDamageStats(ref Stats21, unlockConditionTarget, "Pierce Damage Achievement", dealt: true);
			Stats21.RemoveAll((CPlayerStatsDamage x) => x.Shield == x.Shielded);
			if (unlockConditionTarget.TargetFilter != null)
			{
				Stats21.RemoveAll((CPlayerStatsDamage x) => !unlockConditionTarget.TargetFilter.Compare(Math.Max(0, x.Shield - x.Shielded)));
			}
			test(" PD ", Stats21.Count, ref tally, achievement?.ID);
			int count10 = Stats21.Count;
			int num77 = 0;
			int num78 = count10;
			int num79 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				foreach (CPlayerStatsDamage item13 in Stats21)
				{
					num77 += Math.Max(0, item13.Shield - item13.Shielded);
				}
				num78 = num77;
				num79 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num78 = (conditionsMet += num78);
			progress(achievement?.ID, num78, num79);
			if (num78 < num79)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.ShieldDamage:
		{
			test(" SD ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats11 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageReceived).ToList();
			FilterDamageStats(ref Stats11, unlockConditionTarget, "Shield Damage Achievement", dealt: false);
			Stats11.RemoveAll((CPlayerStatsDamage x) => x.Shielded == 0);
			if (unlockConditionTarget.TargetFilter != null)
			{
				Stats11.RemoveAll((CPlayerStatsDamage x) => !unlockConditionTarget.TargetFilter.Compare(x.Shielded));
			}
			int count3 = Stats11.Count;
			int num31 = 0;
			int num32 = count3;
			int num33 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				foreach (CPlayerStatsDamage item14 in Stats11)
				{
					num31 += item14.Shielded;
				}
				num32 = num31;
				num33 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num32 = (conditionsMet += num32);
			progress(achievement?.ID, num32, num33);
			if (num32 < num33)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.ItemShielded:
		{
			test(" IS ", 1, ref tally, achievement?.ID);
			List<CPlayerStatsDamage> Stats19 = comparePlayerStats.SelectMany((CPlayerStats x) => x.DamageReceived).ToList();
			FilterDamageStats(ref Stats19, unlockConditionTarget, "Item Shield Achievement", dealt: false);
			Stats19.RemoveAll((CPlayerStatsDamage x) => x.ItemShielded == 0);
			if (unlockConditionTarget.TargetFilter != null)
			{
				Stats19.RemoveAll((CPlayerStatsDamage x) => !unlockConditionTarget.TargetFilter.Compare(x.ItemShielded));
			}
			int count9 = Stats19.Count;
			int num66 = 0;
			int num67 = count9;
			int num68 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				foreach (CPlayerStatsDamage item15 in Stats19)
				{
					num66 += item15.ItemShielded;
				}
				num67 = num66;
				num68 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			num67 = (conditionsMet += num67);
			progress(achievement?.ID, num67, num68);
			if (num67 < num68)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.BattleGoalPerkPoints:
		{
			test(" IS ", 1, ref tally, achievement?.ID);
			int num39 = FilterBattleGoalStats(unlockConditionTarget, "Battle Goal Perk Achievement", ability: false, overrideCharacterId);
			int num40 = num39;
			int num41 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num40 = num39;
				num41 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num40;
			progress(achievement?.ID, num40, num41);
			if (num40 < num41)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Gold:
		{
			test(" GD ", 1, ref tally, achievement?.ID);
			int num22 = FilterGoldStats(unlockConditionTarget, "Gold Achievement", modifier: false, overrideCharacterId);
			int num23 = num22;
			int num24 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num23 = num22;
				num24 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num23;
			progress(achievement?.ID, num23, num24);
			if (num23 < num24)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Enhancement:
		{
			test(" IS ", 1, ref tally, achievement?.ID);
			int num74 = FilterEnhancementStats(unlockConditionTarget, "Enhancements Achievement", ability: false, overrideCharacterId);
			int num75 = num74;
			int num76 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num75 = num74;
				num76 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num75;
			progress(achievement?.ID, num75, num76);
			if (num75 < num76)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Donate:
		{
			test(" IS ", 1, ref tally, achievement?.ID);
			int num53 = FilterDonationStats(unlockConditionTarget, "Donate Achievement", ability: false, overrideCharacterId);
			int num54 = num53;
			int num55 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num54 = num53;
				num55 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num54;
			progress(achievement?.ID, num54, num55);
			if (num54 < num55)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.PersonalQuest:
		{
			test(" IS ", 1, ref tally, achievement?.ID);
			int num35 = FilterPersonalQuestStats(unlockConditionTarget, "Personal Quest Achievement", ability: false, overrideCharacterId);
			int num36 = num35;
			int num37 = unlockConditionTarget.Value;
			if (unlockConditionTarget.Amount > 0)
			{
				num36 = num35;
				num37 = unlockConditionTarget.Amount;
				totalConditions = unlockConditionTarget.Amount;
			}
			conditionsMet = num36;
			progress(achievement?.ID, num36, num37);
			if (num36 < num37)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.SpendGold:
		{
			int num27 = 0;
			conditionsMet += num27;
			progress(achievement?.ID, num27, unlockConditionTarget.Value);
			if (num27 < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.Chests:
		{
			int num21 = 0;
			foreach (string chest in unlockConditionTarget.Chests)
			{
				if (AdventureState.MapState.AlreadyRewardedChestTreasureTableIDs.Contains(chest))
				{
					num21++;
				}
			}
			conditionsMet = num21;
			if (num21 < unlockConditionTarget.Value)
			{
				result = false;
			}
			break;
		}
		case EUnlockConditionTargetFilter.PartyMercenaries:
		{
			bool flag = true;
			List<CPlayerStatsDamage> actors = comparePlayerStats.SelectMany((CPlayerStats x) => x.Actor).ToList();
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				if (!unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID))
				{
					flag = false;
				}
			}
			if (!CheckScenarioResult(unlockConditionTarget, actors))
			{
				result = false;
			}
			else if (flag)
			{
				conditionsMet = 1;
			}
			else
			{
				result = false;
			}
			break;
		}
		default:
			result = false;
			break;
		}
		if (unlockConditionTarget.TargetFilter == null || !unlockConditionTarget.TargetFilter.EqualityString.Contains("<") || unlockConditionTarget.Value <= 1)
		{
			conditionsMet = ((conditionsMet > totalConditions) ? totalConditions : conditionsMet);
		}
		return result;
	}

	public bool CheckScenarioResult(CUnlockConditionTarget unlockConditionTarget, List<CPlayerStatsDamage> actors)
	{
		if (actors.Count > 0)
		{
			if (unlockConditionTarget.ScenarioResult == null)
			{
				return true;
			}
			foreach (CPlayerStatsDamage actor in actors)
			{
				if (unlockConditionTarget.ScenarioResult == actor.ScenarioResult)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void test(string why, int count, ref int tally, string who = " ")
	{
		tally = count;
	}

	private void progress(string name, int total, int target)
	{
		_ = 0;
	}

	private void FilterKillStats(ref List<CPlayerStatsKill> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool killed)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally, "Time");
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally, "QuestType");
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActedOnClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		test(tag, Stats.Count, ref tally, "EnemyClass");
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.AbilityTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsKill x) => !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally, "Ability");
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsKill x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally, "Survive");
		bool flag = false;
		if (unlockConditionTarget.CharacterIDs != null)
		{
			foreach (string characterID in unlockConditionTarget.CharacterIDs)
			{
				bool flag2 = false;
				foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
				{
					if (checkCharacter.CharacterID.Contains(characterID))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if ((unlockConditionTarget.AllyType == null || !unlockConditionTarget.AllyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.EnemyType == null || !unlockConditionTarget.EnemyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.CharacterIDs == null || (unlockConditionTarget.CharacterIDs != null && !flag)))
		{
			Stats.RemoveAll((CPlayerStatsKill x) => x.RolledIntoSummoner);
		}
		foreach (CPlayerStatsKill Stat in Stats)
		{
			if (Stat.ActedOnType.Contains(CActor.EType.Player.ToString()))
			{
				bool flag3 = false;
				foreach (CMapCharacter checkCharacter2 in AdventureState.MapState.MapParty.CheckCharacters)
				{
					if (checkCharacter2.CharacterID.Contains(Stat.ActingClassID))
					{
						flag3 = true;
					}
				}
				if (!flag3)
				{
					Stat.ActedOnType = CActor.EType.HeroSummon.ToString();
				}
			}
			if (!Stat.ActingType.Contains(CActor.EType.Player.ToString()))
			{
				continue;
			}
			bool flag4 = false;
			foreach (CMapCharacter checkCharacter3 in AdventureState.MapState.MapParty.CheckCharacters)
			{
				if (checkCharacter3.CharacterID.Contains(Stat.ActedOnClassID))
				{
					flag4 = true;
				}
			}
			if (!flag4)
			{
				Stat.ActingType = CActor.EType.HeroSummon.ToString();
			}
		}
		if (unlockConditionTarget.MonsterType != null)
		{
			List<CPlayerStatsKill> list = new List<CPlayerStatsKill>();
			foreach (string type in unlockConditionTarget.MonsterType)
			{
				list.AddRange(Stats.Where((CPlayerStatsKill x) => (x.MonsterType != null && x.MonsterType.Contains(type)) || (x.ActedOnClassID != null && x.ActedOnClassID.Contains(type))));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsKill> list2 = new List<CPlayerStatsKill>();
			foreach (string type2 in unlockConditionTarget.EnemyType)
			{
				list2.AddRange(Stats.Where((CPlayerStatsKill x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		test(tag, Stats.Count, ref tally, "Target");
		if (unlockConditionTarget.SingleDamage)
		{
			Stats.RemoveAll((CPlayerStatsKill x) => x.PreDamageHealth < x.MaxHealth || x.LastDamageAmount < x.MaxHealth);
		}
		if (unlockConditionTarget.Overkill.HasValue)
		{
			Stats.RemoveAll((CPlayerStatsKill x) => x.LastDamageAmount - x.PreDamageHealth < unlockConditionTarget.Overkill);
		}
		if (unlockConditionTarget.HasFavorite)
		{
			Stats.RemoveAll((CPlayerStatsKill x) => !x.HasFavorite);
		}
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsKill> list3 = new List<CPlayerStatsKill>();
			foreach (string type3 in unlockConditionTarget.AllyType)
			{
				if (killed)
				{
					list3.AddRange(Stats.Where((CPlayerStatsKill x) => x.ActedOnType != null && x.ActedOnType.Contains(type3)));
				}
				else
				{
					list3.AddRange(Stats.Where((CPlayerStatsKill x) => x.ActingType != null && x.ActingType.Contains(type3)));
				}
			}
			Stats.Clear();
			Stats.AddRange(list3);
		}
		else
		{
			List<CPlayerStatsKill> list4 = new List<CPlayerStatsKill>();
			if (killed)
			{
				list4.AddRange(Stats.Where((CPlayerStatsKill x) => (x.ActedOnType != null && x.ActedOnType.Contains(CActor.EType.Player.ToString())) || (x.ActedOnType != null && x.ActedOnType.Contains(CActor.EType.HeroSummon.ToString()))));
			}
			else
			{
				list4.AddRange(Stats.Where((CPlayerStatsKill x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		test(tag, Stats.Count, ref tally, "Actor");
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsKill> list5 = new List<CPlayerStatsKill>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				if (killed)
				{
					list5.AddRange(Stats.Where((CPlayerStatsKill x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
				}
				else
				{
					list5.AddRange(Stats.Where((CPlayerStatsKill x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID));
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				if (killed)
				{
					Stats.RemoveAll((CPlayerStatsKill x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
				}
				else
				{
					Stats.RemoveAll((CPlayerStatsKill x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID2);
				}
			}
		}
		if (unlockConditionTarget.Infused != null)
		{
			List<CPlayerStatsKill> list6 = new List<CPlayerStatsKill>();
			foreach (ElementInfusionBoardManager.EElement element in unlockConditionTarget.Infused)
			{
				list6.AddRange(Stats.Where((CPlayerStatsKill x) => x.Infused.Contains(element)));
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally, "Infused");
		if (unlockConditionTarget.Items != null)
		{
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag5 = false;
				foreach (string item2 in unlockConditionTarget.Items)
				{
					if (!Stats[num3].Items.Contains(item2))
					{
						flag5 = true;
					}
				}
				if (flag5)
				{
					Stats.RemoveAt(num3);
				}
			}
		}
		if (unlockConditionTarget.CauseOfDeath != null)
		{
			List<CPlayerStatsKill> list7 = new List<CPlayerStatsKill>();
			foreach (string cause in unlockConditionTarget.CauseOfDeath)
			{
				list7.AddRange(Stats.Where((CPlayerStatsKill x) => x.CauseOfDeath.ToString().Contains(cause)));
			}
			Stats.Clear();
			Stats.AddRange(list7);
		}
		test(tag, Stats.Count, ref tally, "Cause");
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsKill> list8 = new List<CPlayerStatsKill>();
			List<string> list9 = new List<string>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag6 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num4].ActedOnPositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag6 = true;
							list9.Add(allyCondition);
						}
					}
				}
				if (!flag6)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num4].ActedOnNegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag6 = true;
								list9.Add(allyCondition2);
							}
						}
					}
				}
				if (flag6)
				{
					list8.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			list9 = list9.Distinct().ToList();
			if (list9.Count >= unlockConditionTarget.AllyMinConditions)
			{
				Stats.AddRange(list8);
			}
		}
		test(tag, Stats.Count, ref tally, "ActorCondition");
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsKill> list10 = new List<CPlayerStatsKill>();
			List<string> list11 = new List<string>();
			for (int num5 = Stats.Count - 1; num5 >= 0; num5--)
			{
				bool flag7 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num5].PositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag7 = true;
							list11.Add(enemyCondition);
						}
					}
				}
				if (!flag7)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num5].NegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag7 = true;
								list11.Add(enemyCondition2);
							}
						}
					}
					if (Stats[num5].Doom && unlockConditionTarget.EnemyConditions.Contains("Doom"))
					{
						flag7 = true;
						list11.Add("Doom");
					}
				}
				if (flag7)
				{
					list10.Add(Stats[num5]);
					Stats.RemoveAt(num5);
				}
			}
			Stats.Clear();
			list11 = list11.Distinct().ToList();
			if (list11.Count >= unlockConditionTarget.EnemyMinConditions)
			{
				Stats.AddRange(list10);
			}
		}
		test(tag, Stats.Count, ref tally, "TargetCondition");
	}

	private void FilterDamageStats(ref List<CPlayerStatsDamage> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool dealt)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AbilityTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsDamage x) => !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsDamage x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		bool flag = false;
		if (unlockConditionTarget.CharacterIDs != null)
		{
			foreach (string characterID in unlockConditionTarget.CharacterIDs)
			{
				bool flag2 = false;
				foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
				{
					if (checkCharacter.CharacterID.Contains(characterID))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if ((unlockConditionTarget.AllyType == null || !unlockConditionTarget.AllyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.EnemyType == null || !unlockConditionTarget.EnemyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.CharacterIDs == null || (unlockConditionTarget.CharacterIDs != null && !flag)))
		{
			Stats.RemoveAll((CPlayerStatsDamage x) => x.RolledIntoSummoner);
		}
		if (unlockConditionTarget.AttackTypes != null)
		{
			List<CPlayerStatsDamage> list = new List<CPlayerStatsDamage>();
			foreach (string attackType in unlockConditionTarget.AttackTypes)
			{
				if (attackType.Contains(CAbility.EAttackType.Melee.ToString()))
				{
					list.AddRange(Stats.Where((CPlayerStatsDamage x) => x.IsMelee));
				}
				else
				{
					list.AddRange(Stats.Where((CPlayerStatsDamage x) => !x.IsMelee));
				}
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.HasFavorite)
		{
			Stats.RemoveAll((CPlayerStatsDamage x) => !x.HasFavorite);
		}
		if (unlockConditionTarget.DamageSource != null)
		{
			List<CPlayerStatsDamage> list2 = new List<CPlayerStatsDamage>();
			foreach (string damage in unlockConditionTarget.DamageSource)
			{
				if (damage.Contains(CCondition.ENegativeCondition.Poison.ToString()))
				{
					list2.AddRange(Stats.Where((CPlayerStatsDamage x) => x.PoisonDamage > 0));
				}
				else
				{
					list2.AddRange(Stats.Where((CPlayerStatsDamage x) => x.CauseOfDamage.ToString().Contains(damage)));
				}
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Infused != null)
		{
			List<CPlayerStatsDamage> list3 = new List<CPlayerStatsDamage>();
			foreach (ElementInfusionBoardManager.EElement element in unlockConditionTarget.Infused)
			{
				list3.AddRange(Stats.Where((CPlayerStatsDamage x) => x.Infused.Contains(element)));
			}
			Stats.Clear();
			Stats.AddRange(list3);
		}
		test(tag, Stats.Count, ref tally);
		if (dealt)
		{
			FilterDamageDealtStats(ref Stats, unlockConditionTarget, tag, ref tally);
		}
		else
		{
			FilterDamageReceiveStats(ref Stats, unlockConditionTarget, tag, ref tally);
		}
	}

	private void FilterDamageReceiveStats(ref List<CPlayerStatsDamage> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, ref int eventcount)
	{
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num].ActedOnClassID))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref eventcount);
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsDamage> list = new List<CPlayerStatsDamage>();
			foreach (string type in unlockConditionTarget.AllyType)
			{
				list.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsDamage> list2 = new List<CPlayerStatsDamage>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list2.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsDamage x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID2);
			}
		}
		if (Stats.Count > 0)
		{
			if (unlockConditionTarget.EnemyType != null)
			{
				List<CPlayerStatsDamage> list3 = new List<CPlayerStatsDamage>();
				foreach (string type2 in unlockConditionTarget.EnemyType)
				{
					list3.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActingType != null && x.ActingType.Contains(type2)));
				}
				Stats.Clear();
				Stats.AddRange(list3);
			}
			else
			{
				List<CPlayerStatsDamage> list4 = new List<CPlayerStatsDamage>();
				list4.AddRange(Stats.Where((CPlayerStatsDamage x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
				Stats.Clear();
				Stats.AddRange(list4);
			}
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsDamage> list5 = new List<CPlayerStatsDamage>();
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				bool flag = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num2].ActedOnPositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num2].ActedOnNegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag = true;
							}
						}
					}
				}
				if (flag)
				{
					list5.Add(Stats[num2]);
					Stats.RemoveAt(num2);
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsDamage> list6 = new List<CPlayerStatsDamage>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num3].PositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num3].NegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list6.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref eventcount);
	}

	private void FilterDamageDealtStats(ref List<CPlayerStatsDamage> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, ref int eventcount)
	{
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num].ActingClassID))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref eventcount);
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsDamage> list = new List<CPlayerStatsDamage>();
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				list.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActingType != null && x.ActingType.Contains(type)));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsDamage> list2 = new List<CPlayerStatsDamage>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list2.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActedOnType != null && x.ActedOnType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		else
		{
			List<CPlayerStatsDamage> list3 = new List<CPlayerStatsDamage>();
			list3.AddRange(Stats.Where((CPlayerStatsDamage x) => (x.ActedOnType != null && x.ActedOnType.Contains(CActor.EType.Player.ToString())) || (x.ActedOnType != null && x.ActedOnType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list3);
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsDamage> list4 = new List<CPlayerStatsDamage>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list4.AddRange(Stats.Where((CPlayerStatsDamage x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsDamage x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsDamage> list5 = new List<CPlayerStatsDamage>();
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				bool flag = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num2].PositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num2].NegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag = true;
							}
						}
					}
				}
				if (flag)
				{
					list5.Add(Stats[num2]);
					Stats.RemoveAt(num2);
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref eventcount);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsDamage> list6 = new List<CPlayerStatsDamage>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list6.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref eventcount);
	}

	private void FilterElementStats(ref List<CPlayerStatsElement> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool consume)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Elements != null)
		{
			Stats.RemoveAll((CPlayerStatsElement x) => !unlockConditionTarget.Elements.Contains(x.Element));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsElement> list = new List<CPlayerStatsElement>();
			foreach (string type in unlockConditionTarget.AllyType)
			{
				list.AddRange(Stats.Where((CPlayerStatsElement x) => x.ActingType != null && x.ActingType.Contains(type)));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		else
		{
			List<CPlayerStatsElement> list2 = new List<CPlayerStatsElement>();
			list2.AddRange(Stats.Where((CPlayerStatsElement x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list2);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsElement> list3 = new List<CPlayerStatsElement>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list3.AddRange(Stats.Where((CPlayerStatsElement x) => x.ActorClassID != null && x.ActorClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list3);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsElement x) => x.ActorClassID != null && x.ActorClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsElement x) => x.ActorClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterDestroyStats(ref List<CPlayerStatsDestroyObstacle> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool consume)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsDestroyObstacle> list = new List<CPlayerStatsDestroyObstacle>();
			foreach (string type in unlockConditionTarget.AllyType)
			{
				list.AddRange(Stats.Where((CPlayerStatsDestroyObstacle x) => x.ActingType != null && x.ActingType.Contains(type)));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		else
		{
			List<CPlayerStatsDestroyObstacle> list2 = new List<CPlayerStatsDestroyObstacle>();
			list2.AddRange(Stats.Where((CPlayerStatsDestroyObstacle x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list2);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsDestroyObstacle> list3 = new List<CPlayerStatsDestroyObstacle>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list3.AddRange(Stats.Where((CPlayerStatsDestroyObstacle x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list3);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsDestroyObstacle x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsDestroyObstacle x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterModifierStats(ref List<CPlayerStatsModifiers> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool modifier)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActingClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.AbilityTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsModifiers x) => !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsModifiers x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsModifiers> list = new List<CPlayerStatsModifiers>();
			bool flag = true;
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				if (!type.Contains("NotSelf") && !type.Contains("OnlySelf"))
				{
					flag = false;
				}
				list.AddRange(Stats.Where((CPlayerStatsModifiers x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			if (!flag)
			{
				Stats.Clear();
				Stats.AddRange(list);
			}
			if (unlockConditionTarget.EnemyType.Contains("NotSelf"))
			{
				Stats.RemoveAll((CPlayerStatsModifiers x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && x.ActedOnClassID == x.ActingClassID && x.ActingType.Contains("Player"));
			}
			if (unlockConditionTarget.EnemyType.Contains("OnlySelf"))
			{
				Stats.RemoveAll((CPlayerStatsModifiers x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && !(x.ActedOnClassID == x.ActingClassID) && x.ActingType.Contains("Player"));
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsModifiers> list2 = new List<CPlayerStatsModifiers>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list2.AddRange(Stats.Where((CPlayerStatsModifiers x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		else
		{
			List<CPlayerStatsModifiers> list3 = new List<CPlayerStatsModifiers>();
			list3.AddRange(Stats.Where((CPlayerStatsModifiers x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list3);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsModifiers> list4 = new List<CPlayerStatsModifiers>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list4.AddRange(Stats.Where((CPlayerStatsModifiers x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsModifiers x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsModifiers> list5 = new List<CPlayerStatsModifiers>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list5.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsModifiers> list6 = new List<CPlayerStatsModifiers>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag3 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num4].PositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag3 = true;
						}
					}
				}
				if (!flag3)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num4].NegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag3 = true;
							}
						}
					}
				}
				if (flag3)
				{
					list6.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Modifiers != null)
		{
			List<CPlayerStatsModifiers> list7 = new List<CPlayerStatsModifiers>();
			bool flag4 = false;
			foreach (CPlayerStatsModifiers Stat in Stats)
			{
				flag4 = false;
				foreach (string modifier2 in unlockConditionTarget.Modifiers)
				{
					string value;
					string text = (value = modifier2);
					if (text.Contains("0"))
					{
						value = "+0";
					}
					if (text.Contains("*0"))
					{
						value = "x0";
					}
					if (text.Contains("*2"))
					{
						value = "x2";
					}
					foreach (string item2 in Stat.Modifier)
					{
						if (item2.Contains(value))
						{
							flag4 = true;
						}
					}
				}
				if (flag4)
				{
					list7.Add(Stat);
				}
			}
			Stats.Clear();
			Stats.AddRange(list7);
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterItemStats(ref List<CPlayerStatsItem> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool modifier)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActingClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.AbilityTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsItem x) => !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsItem x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsItem> list = new List<CPlayerStatsItem>();
			bool flag = true;
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				if (!type.Contains("NotSelf") && !type.Contains("OnlySelf"))
				{
					flag = false;
				}
				list.AddRange(Stats.Where((CPlayerStatsItem x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			if (!flag)
			{
				Stats.Clear();
				Stats.AddRange(list);
			}
			if (unlockConditionTarget.EnemyType.Contains("NotSelf"))
			{
				Stats.RemoveAll((CPlayerStatsItem x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && x.ActedOnClassID == x.ActingClassID && x.ActingType.Contains("Player"));
			}
			if (unlockConditionTarget.EnemyType.Contains("OnlySelf"))
			{
				Stats.RemoveAll((CPlayerStatsItem x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && !(x.ActedOnClassID == x.ActingClassID) && x.ActingType.Contains("Player"));
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsItem> list2 = new List<CPlayerStatsItem>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list2.AddRange(Stats.Where((CPlayerStatsItem x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		else
		{
			List<CPlayerStatsItem> list3 = new List<CPlayerStatsItem>();
			list3.AddRange(Stats.Where((CPlayerStatsItem x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list3);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsItem> list4 = new List<CPlayerStatsItem>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list4.AddRange(Stats.Where((CPlayerStatsItem x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsItem x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsItem> list5 = new List<CPlayerStatsItem>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list5.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsItem> list6 = new List<CPlayerStatsItem>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag3 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num4].PositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag3 = true;
						}
					}
				}
				if (!flag3)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num4].NegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag3 = true;
							}
						}
					}
				}
				if (flag3)
				{
					list6.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Items != null)
		{
			Stats.RemoveAll((CPlayerStatsItem x) => !unlockConditionTarget.Items.Contains(x.ItemName));
		}
		if (unlockConditionTarget.Slot != null)
		{
			Stats.RemoveAll((CPlayerStatsItem x) => !unlockConditionTarget.Slot.Contains(x.Slot));
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterOwnItemStats(ref List<CItem> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool modifier, string overrideCharacterId)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count(), ref tally);
		test(tag, Stats.Count(), ref tally);
		foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(checkCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == checkCharacter.CharacterID))
			{
				if (checkCharacter.CheckBoundItems == null)
				{
					Stats.AddRange(checkCharacter.CheckEquippedItems);
				}
				else
				{
					Stats.AddRange(checkCharacter.CheckEquippedItems.Concat(checkCharacter.CheckBoundItems).ToList());
				}
			}
		}
		if (unlockConditionTarget.CharacterIDs == null)
		{
			Stats.AddRange(AdventureState.MapState.MapParty.CheckUnboundItems);
		}
		if (unlockConditionTarget.Items != null)
		{
			Stats.RemoveAll((CItem x) => !unlockConditionTarget.Items.Contains(x.Name));
		}
		if (unlockConditionTarget.Slot != null)
		{
			Stats.RemoveAll((CItem x) => !unlockConditionTarget.Slot.Contains(x.YMLData.Slot.ToString()));
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterLootStats(ref List<CPlayerStatsLoot> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool modifier)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActingClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		_ = unlockConditionTarget.TargetEnemyFamilies;
		if (unlockConditionTarget.AbilityTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsLoot x) => !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsLoot x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsLoot> list = new List<CPlayerStatsLoot>();
			bool flag = true;
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				if (!type.Contains("NotSelf") && !type.Contains("OnlySelf"))
				{
					flag = false;
				}
				list.AddRange(Stats.Where((CPlayerStatsLoot x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			if (!flag)
			{
				Stats.Clear();
				Stats.AddRange(list);
			}
			if (unlockConditionTarget.EnemyType.Contains("NotSelf"))
			{
				Stats.RemoveAll((CPlayerStatsLoot x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && x.ActedOnClassID == x.ActingClassID && x.ActingType.Contains("Player"));
			}
			if (unlockConditionTarget.EnemyType.Contains("OnlySelf"))
			{
				Stats.RemoveAll((CPlayerStatsLoot x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && !(x.ActedOnClassID == x.ActingClassID) && x.ActingType.Contains("Player"));
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsLoot> list2 = new List<CPlayerStatsLoot>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list2.AddRange(Stats.Where((CPlayerStatsLoot x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		else
		{
			List<CPlayerStatsLoot> list3 = new List<CPlayerStatsLoot>();
			list3.AddRange(Stats.Where((CPlayerStatsLoot x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list3);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsLoot> list4 = new List<CPlayerStatsLoot>();
			foreach (string targetClassID in unlockConditionTarget.CharacterIDs)
			{
				list4.AddRange(Stats.Where((CPlayerStatsLoot x) => x.ActingClassID != null && x.ActingClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID2 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsLoot x) => x.ActingClassID != null && x.ActingClassID == targetClassID2);
			}
		}
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsLoot> list5 = new List<CPlayerStatsLoot>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list5.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsLoot> list6 = new List<CPlayerStatsLoot>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag3 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num4].PositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag3 = true;
						}
					}
				}
				if (!flag3)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num4].NegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag3 = true;
							}
						}
					}
				}
				if (flag3)
				{
					list6.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.LootTypes != null)
		{
			Stats.RemoveAll((CPlayerStatsLoot x) => !unlockConditionTarget.LootTypes.Contains(x.LootType));
		}
		test(tag, Stats.Count, ref tally);
	}

	private void FilterAbilityStats(ref List<CPlayerStatsAbilities> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool ability)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.SpecialAbility)
		{
			Stats.RemoveAll((CPlayerStatsAbilities x) => !x.SpecialAbility);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.DefaultAction)
		{
			Stats.RemoveAll((CPlayerStatsAbilities x) => !x.DefaultAction || !x.HasHappened);
		}
		if (unlockConditionTarget.Targets > 0)
		{
			Stats.RemoveAll((CPlayerStatsAbilities x) => x.Targets < unlockConditionTarget.Targets);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AreaEffect.HasValue)
		{
			Stats.RemoveAll((CPlayerStatsAbilities x) => x.AreaEffect != unlockConditionTarget.AreaEffect);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActedOnClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		_ = unlockConditionTarget.TargetEnemyFamilies;
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AbilityTypes != null && unlockConditionTarget.AbilityTypes.Count > 0)
		{
			Stats.RemoveAll((CPlayerStatsAbilities x) => x.AbilityType == CAbility.EAbilityType.Heal || !unlockConditionTarget.AbilityTypes.Contains(x.AbilityType));
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsAbilities x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsAbilities> list = new List<CPlayerStatsAbilities>();
			bool flag = true;
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				if (!type.Contains("NotSelf") && !type.Contains("OnlySelf"))
				{
					flag = false;
				}
				list.AddRange(Stats.Where((CPlayerStatsAbilities x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			if (!flag)
			{
				Stats.Clear();
				Stats.AddRange(list);
			}
			if (unlockConditionTarget.EnemyType.Contains("NotSelf"))
			{
				Stats.RemoveAll((CPlayerStatsAbilities x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && x.ActedOnClassID == x.ActingClassID && x.ActingType.Contains("Player"));
			}
			if (unlockConditionTarget.EnemyType.Contains("OnlySelf"))
			{
				foreach (CPlayerStatsAbilities Stat in Stats)
				{
					if (Stat.AbilityType == CAbility.EAbilityType.Damage && string.IsNullOrEmpty(Stat.ActedOnClassID) && Stat.ActedOnType == "Unknown")
					{
						Stat.ActedOnClassID = Stat.ActingClassID;
					}
				}
				Stats.RemoveAll((CPlayerStatsAbilities x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && !(x.ActedOnClassID == x.ActingClassID) && x.ActingType.Contains("Player"));
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetCharacterIDs != null)
		{
			List<CPlayerStatsAbilities> list2 = new List<CPlayerStatsAbilities>();
			foreach (string targetClassID in unlockConditionTarget.TargetCharacterIDs)
			{
				list2.AddRange(Stats.Where((CPlayerStatsAbilities x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsAbilities> list3 = new List<CPlayerStatsAbilities>();
			foreach (string targetClassID2 in unlockConditionTarget.CharacterIDs)
			{
				list3.AddRange(Stats.Where((CPlayerStatsAbilities x) => x.ActingClassID != null && x.ActingClassID == targetClassID2));
			}
			Stats.Clear();
			Stats.AddRange(list3);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID3 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsAbilities x) => x.ActingClassID != null && x.ActingClassID == targetClassID3);
			}
		}
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsAbilities> list4 = new List<CPlayerStatsAbilities>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list4.AddRange(Stats.Where((CPlayerStatsAbilities x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		else
		{
			List<CPlayerStatsAbilities> list5 = new List<CPlayerStatsAbilities>();
			list5.AddRange(Stats.Where((CPlayerStatsAbilities x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Infused != null)
		{
			List<CPlayerStatsAbilities> list6 = new List<CPlayerStatsAbilities>();
			foreach (ElementInfusionBoardManager.EElement element in unlockConditionTarget.Infused)
			{
				list6.AddRange(Stats.Where((CPlayerStatsAbilities x) => x.Infused.Contains(element)));
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsAbilities> list7 = new List<CPlayerStatsAbilities>();
			for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
			{
				bool flag2 = false;
				foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
				{
					foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
					{
						if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
					{
						foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
						{
							if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					list7.Add(Stats[num3]);
					Stats.RemoveAt(num3);
				}
			}
			Stats.Clear();
			Stats.AddRange(list7);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsAbilities> list8 = new List<CPlayerStatsAbilities>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag3 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num4].PositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag3 = true;
						}
					}
				}
				if (!flag3)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num4].NegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag3 = true;
							}
						}
					}
				}
				if (flag3)
				{
					list8.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			Stats.AddRange(list8);
		}
		test(tag, Stats.Count, ref tally);
	}

	private int FilterEnhancementStats(CUnlockConditionTarget unlockConditionTarget, string tag, bool ability, string overrideCharacterId)
	{
		int num = 0;
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == selectedCharacter.CharacterID))
			{
				num += selectedCharacter.Enhancements.Count((CEnhancement x) => x.Enhancement != EEnhancement.NoEnhancement);
			}
		}
		return num;
	}

	private int FilterGoldStats(CUnlockConditionTarget unlockConditionTarget, string tag, bool modifier, string overrideCharacterId)
	{
		int num = 0;
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == selectedCharacter.CharacterID))
			{
				num += selectedCharacter.CharacterGold;
			}
		}
		return num;
	}

	private int FilterDonationStats(CUnlockConditionTarget unlockConditionTarget, string tag, bool ability, string overrideCharacterId)
	{
		int num = 0;
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == selectedCharacter.CharacterID))
			{
				num += selectedCharacter.Donations;
			}
		}
		return num;
	}

	private int FilterPersonalQuestStats(CUnlockConditionTarget unlockConditionTarget, string tag, bool ability, string overrideCharacterId)
	{
		int num = 0;
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == selectedCharacter.CharacterID))
			{
				num += selectedCharacter.ExperiencePersonalGoal;
			}
		}
		return num;
	}

	private int FilterBattleGoalStats(CUnlockConditionTarget unlockConditionTarget, string tag, bool ability, string overrideCharacterId)
	{
		int num = 0;
		foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if ((unlockConditionTarget.CharacterIDs == null && overrideCharacterId == null) || (unlockConditionTarget.CharacterIDs != null && unlockConditionTarget.CharacterIDs.Contains(selectedCharacter.CharacterID)) || (overrideCharacterId != null && overrideCharacterId == selectedCharacter.CharacterID))
			{
				num += selectedCharacter.BattleGoalPerks;
			}
		}
		return num;
	}

	private void FilterHealStats(ref List<CPlayerStatsHeal> Stats, CUnlockConditionTarget unlockConditionTarget, string tag, bool ability)
	{
		int tally = 0;
		tag += " ";
		test(tag, Stats.Count, ref tally);
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.QuestTypes != null)
		{
			for (int num = Stats.Count - 1; num >= 0; num--)
			{
				string findqt = Stats[num].QuestType;
				EQuestType item = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == findqt);
				if (!unlockConditionTarget.QuestTypes.Contains(item))
				{
					Stats.RemoveAt(num);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.TargetEnemyClassIDs != null)
		{
			for (int num2 = Stats.Count - 1; num2 >= 0; num2--)
			{
				if (!unlockConditionTarget.TargetEnemyClassIDs.Contains(Stats[num2].ActedOnClassID))
				{
					Stats.RemoveAt(num2);
				}
			}
		}
		_ = unlockConditionTarget.TargetEnemyFamilies;
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Survived != null)
		{
			foreach (CPlayerStatsScenario scenarioStats in m_PlayerStatsScenarioToUse)
			{
				if (!scenarioStats.PlayerSurvivedScenario)
				{
					if (unlockConditionTarget.Survived.Contains("All"))
					{
						Stats.Clear();
						break;
					}
					Stats.RemoveAll((CPlayerStatsHeal x) => x.ActingClassID == scenarioStats.CharacterID);
				}
			}
		}
		test(tag, Stats.Count, ref tally);
		bool flag = false;
		if (unlockConditionTarget.CharacterIDs != null)
		{
			foreach (string characterID in unlockConditionTarget.CharacterIDs)
			{
				bool flag2 = false;
				foreach (CMapCharacter checkCharacter in AdventureState.MapState.MapParty.CheckCharacters)
				{
					if (checkCharacter.CharacterID.Contains(characterID))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if ((unlockConditionTarget.AllyType == null || !unlockConditionTarget.AllyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.EnemyType == null || !unlockConditionTarget.EnemyType.Contains(CActor.EType.HeroSummon.ToString())) && (unlockConditionTarget.CharacterIDs == null || (unlockConditionTarget.CharacterIDs != null && !flag)))
		{
			Stats.RemoveAll((CPlayerStatsHeal x) => x.RolledIntoSummoner);
		}
		if (unlockConditionTarget.TargetCharacterIDs != null)
		{
			List<CPlayerStatsHeal> list = new List<CPlayerStatsHeal>();
			foreach (string targetClassID in unlockConditionTarget.TargetCharacterIDs)
			{
				list.AddRange(Stats.Where((CPlayerStatsHeal x) => x.ActedOnClassID != null && x.ActedOnClassID == targetClassID));
			}
			Stats.Clear();
			Stats.AddRange(list);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.CharacterIDs != null)
		{
			List<CPlayerStatsHeal> list2 = new List<CPlayerStatsHeal>();
			foreach (string targetClassID2 in unlockConditionTarget.CharacterIDs)
			{
				list2.AddRange(Stats.Where((CPlayerStatsHeal x) => x.ActingClassID != null && x.ActingClassID == targetClassID2));
			}
			Stats.Clear();
			Stats.AddRange(list2);
		}
		if (unlockConditionTarget.ExcludeCharacterIDs != null)
		{
			foreach (string targetClassID3 in unlockConditionTarget.ExcludeCharacterIDs)
			{
				Stats.RemoveAll((CPlayerStatsHeal x) => x.ActingClassID != null && x.ActingClassID == targetClassID3);
			}
		}
		if (unlockConditionTarget.EnemyType != null)
		{
			List<CPlayerStatsHeal> list3 = new List<CPlayerStatsHeal>();
			bool flag3 = true;
			foreach (string type in unlockConditionTarget.EnemyType)
			{
				if (!type.Contains("NotSelf") && !type.Contains("OnlySelf"))
				{
					flag3 = false;
				}
				list3.AddRange(Stats.Where((CPlayerStatsHeal x) => x.ActedOnType != null && x.ActedOnType.Contains(type)));
			}
			if (!flag3)
			{
				Stats.Clear();
				Stats.AddRange(list3);
			}
			if (unlockConditionTarget.EnemyType.Contains("NotSelf"))
			{
				Stats.RemoveAll((CPlayerStatsHeal x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && x.ActedOnClassID == x.ActingClassID && x.ActingType.Contains("Player"));
			}
			if (unlockConditionTarget.EnemyType.Contains("OnlySelf"))
			{
				Stats.RemoveAll((CPlayerStatsHeal x) => x.ActingType != null && x.ActingClassID != null && x.ActedOnClassID != null && !(x.ActedOnClassID == x.ActingClassID) && x.ActingType.Contains("Player"));
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyType != null)
		{
			List<CPlayerStatsHeal> list4 = new List<CPlayerStatsHeal>();
			foreach (string type2 in unlockConditionTarget.AllyType)
			{
				list4.AddRange(Stats.Where((CPlayerStatsHeal x) => x.ActingType != null && x.ActingType.Contains(type2)));
			}
			Stats.Clear();
			Stats.AddRange(list4);
		}
		else
		{
			List<CPlayerStatsHeal> list5 = new List<CPlayerStatsHeal>();
			list5.AddRange(Stats.Where((CPlayerStatsHeal x) => (x.ActingType != null && x.ActingType.Contains(CActor.EType.Player.ToString())) || (x.ActingType != null && x.ActingType.Contains(CActor.EType.HeroSummon.ToString()))));
			Stats.Clear();
			Stats.AddRange(list5);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.Infused != null)
		{
			List<CPlayerStatsHeal> list6 = new List<CPlayerStatsHeal>();
			foreach (ElementInfusionBoardManager.EElement element in unlockConditionTarget.Infused)
			{
				list6.AddRange(Stats.Where((CPlayerStatsHeal x) => x.Infused.Contains(element)));
			}
			Stats.Clear();
			Stats.AddRange(list6);
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.EnemyConditions != null)
		{
			List<CPlayerStatsHeal> list7 = new List<CPlayerStatsHeal>();
			if (unlockConditionTarget.EnemyConditions.Contains(CCondition.ENegativeCondition.Poison.ToString()))
			{
				list7.AddRange(Stats.Where((CPlayerStatsHeal x) => x.PoisonRemoved));
				Stats.Clear();
				Stats.AddRange(list7);
			}
			if (unlockConditionTarget.EnemyConditions.Contains(CCondition.ENegativeCondition.Wound.ToString()))
			{
				list7.AddRange(Stats.Where((CPlayerStatsHeal x) => x.WoundRemoved));
				Stats.Clear();
				Stats.AddRange(list7);
			}
			if (!unlockConditionTarget.EnemyConditions.Contains(CCondition.ENegativeCondition.Poison.ToString()) && !unlockConditionTarget.EnemyConditions.Contains(CCondition.ENegativeCondition.Wound.ToString()))
			{
				for (int num3 = Stats.Count - 1; num3 >= 0; num3--)
				{
					bool flag4 = false;
					foreach (PositiveConditionPair actedOnPositiveCondition in Stats[num3].ActedOnPositiveConditions)
					{
						foreach (string enemyCondition in unlockConditionTarget.EnemyConditions)
						{
							if (actedOnPositiveCondition.PositiveCondition.ToString().Contains(enemyCondition))
							{
								flag4 = true;
							}
						}
					}
					if (!flag4)
					{
						foreach (NegativeConditionPair actedOnNegativeCondition in Stats[num3].ActedOnNegativeConditions)
						{
							foreach (string enemyCondition2 in unlockConditionTarget.EnemyConditions)
							{
								if (actedOnNegativeCondition.NegativeCondition.ToString().Contains(enemyCondition2))
								{
									flag4 = true;
								}
							}
						}
					}
					if (flag4)
					{
						list7.Add(Stats[num3]);
						Stats.RemoveAt(num3);
					}
				}
				Stats.Clear();
				Stats.AddRange(list7);
			}
		}
		test(tag, Stats.Count, ref tally);
		if (unlockConditionTarget.AllyConditions != null)
		{
			List<CPlayerStatsHeal> list8 = new List<CPlayerStatsHeal>();
			for (int num4 = Stats.Count - 1; num4 >= 0; num4--)
			{
				bool flag5 = false;
				foreach (PositiveConditionPair positiveCondition in Stats[num4].PositiveConditions)
				{
					foreach (string allyCondition in unlockConditionTarget.AllyConditions)
					{
						if (positiveCondition.PositiveCondition.ToString().Contains(allyCondition))
						{
							flag5 = true;
						}
					}
				}
				if (!flag5)
				{
					foreach (NegativeConditionPair negativeCondition in Stats[num4].NegativeConditions)
					{
						foreach (string allyCondition2 in unlockConditionTarget.AllyConditions)
						{
							if (negativeCondition.NegativeCondition.ToString().Contains(allyCondition2))
							{
								flag5 = true;
							}
						}
					}
				}
				if (flag5)
				{
					list8.Add(Stats[num4]);
					Stats.RemoveAt(num4);
				}
			}
			Stats.Clear();
			Stats.AddRange(list8);
		}
		test(tag, Stats.Count, ref tally);
	}

	public CUnlockCondition(CChapter chapter = null, int? prosperity = null, int? reputation = null, int? gold = null, int? personalQuestStep = null, List<Tuple<EUnlockConditionType, string>> requiredConditions = null, List<Tuple<EUnlockConditionType, int>> requiredConditionsCount = null, List<Tuple<EUnlockConditionType, int>> requiredConditionsTotal = null, List<CUnlockChoiceContainer> requiredChoiceContainer = null, List<Tuple<string, int>> requiredHeroes = null, List<CUnlockConditionTarget> targets = null, bool ordered = false, int targetsRequired = 0, bool singleScenario = false, List<EAdventureDifficulty> difficulty = null)
	{
		Chapter = chapter;
		Prosperity = prosperity;
		Reputation = reputation;
		Gold = gold;
		PersonalQuestStep = personalQuestStep;
		RequiredConditions = requiredConditions;
		RequiredConditionsCount = requiredConditionsCount;
		RequiredConditionsTotal = ((requiredConditionsTotal != null) ? requiredConditionsTotal : new List<Tuple<EUnlockConditionType, int>>());
		RequiredConditionsTotalMet = new List<int>();
		RequiredConditionsTotalMet.Add(0);
		foreach (Tuple<EUnlockConditionType, int> item in RequiredConditionsTotal)
		{
			_ = item;
			RequiredConditionsTotalMet.Add(0);
		}
		RequiredChoiceContainer = requiredChoiceContainer;
		RequiredHeroes = requiredHeroes;
		Targets = targets;
		Ordered = ordered;
		SingleScenario = singleScenario;
		TargetsRequired = targetsRequired;
		Difficulty = difficulty;
	}

	public static bool Parse(MappingEntry entry, string fileName, out CUnlockCondition unlockCondition)
	{
		bool flag = true;
		unlockCondition = null;
		if (!YMLShared.GetSequence(entry, fileName, out var sequence))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockCondition entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
			return false;
		}
		CChapter chapter = null;
		int? prosperity = null;
		int? reputation = null;
		bool singleScenario = false;
		int? gold = null;
		int? personalQuestStep = null;
		List<Tuple<EUnlockConditionType, string>> list = new List<Tuple<EUnlockConditionType, string>>();
		List<Tuple<EUnlockConditionType, int>> list2 = new List<Tuple<EUnlockConditionType, int>>();
		List<Tuple<EUnlockConditionType, int>> list3 = new List<Tuple<EUnlockConditionType, int>>();
		List<CUnlockChoiceContainer> requiredChoiceContainer = new List<CUnlockChoiceContainer>();
		List<Tuple<string, int>> list4 = new List<Tuple<string, int>>();
		List<CUnlockConditionTarget> list5 = new List<CUnlockConditionTarget>();
		List<EAdventureDifficulty> list6 = null;
		bool ordered = false;
		int targetsRequired = 0;
		foreach (DataItem entry2 in sequence.Entries)
		{
			if (entry2 is Mapping mapping && mapping.Entries.Count == 1)
			{
				try
				{
					MappingEntry ucEntry = mapping.Entries[0];
					EUnlockConditionType eUnlockConditionType = UnlockConditionTypes.SingleOrDefault((EUnlockConditionType x) => x.ToString() == ucEntry.Key.ToString());
					if (eUnlockConditionType != EUnlockConditionType.None)
					{
						string text = ucEntry.Value.ToString();
						if (int.TryParse(text, out var result))
						{
							list2.Add(new Tuple<EUnlockConditionType, int>(eUnlockConditionType, result));
						}
						else
						{
							list.Add(new Tuple<EUnlockConditionType, string>(eUnlockConditionType, text));
						}
						continue;
					}
					switch (ucEntry.Key.ToString())
					{
					case "Chapter":
					{
						if (YMLShared.GetIntList(ucEntry.Value, "Chapter", fileName, out var values2, allowScalar: false))
						{
							chapter = new CChapter(values2[0], values2[1]);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Prosperity":
					{
						if (YMLShared.ParseIntValue(ucEntry.Value.ToString(), "Prosperity", fileName, out var value5))
						{
							prosperity = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Reputation":
					{
						if (YMLShared.ParseIntValue(ucEntry.Value.ToString(), "Reputation", fileName, out var value2))
						{
							reputation = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Gold":
					{
						if (YMLShared.ParseIntValue(ucEntry.Value.ToString(), "Gold", fileName, out var value3))
						{
							gold = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PersonalQuestStep":
					{
						if (YMLShared.ParseIntValue(ucEntry.Value.ToString(), "PersonalQuestStep", fileName, out var value6))
						{
							personalQuestStep = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RequiredHeroes":
					{
						if (YMLShared.GetMapping(ucEntry, fileName, out var mapping2))
						{
							foreach (MappingEntry entry3 in mapping2.Entries)
							{
								if (int.TryParse(entry3.Value.ToString(), out var result2))
								{
									list4.Add(new Tuple<string, int>(entry3.Key.ToString(), result2));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RequiredHeroes entry, must be \"CharacterName: MinCharLevel\" File: " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Required":
					{
						if (YMLShared.GetMapping(ucEntry, fileName, out var mapping3))
						{
							foreach (MappingEntry reqConditionEntry in mapping3.Entries)
							{
								EUnlockConditionType eUnlockConditionType2 = UnlockConditionTypes.SingleOrDefault((EUnlockConditionType x) => x.ToString() == reqConditionEntry.Key.ToString());
								if (eUnlockConditionType2 != EUnlockConditionType.None)
								{
									if (int.TryParse(reqConditionEntry.Value.ToString(), out var result3))
									{
										list3.Add(new Tuple<EUnlockConditionType, int>(eUnlockConditionType2, result3));
										continue;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Required entry, must be \"UnlockCondition: Count\" File: " + fileName);
									flag = false;
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Entry " + reqConditionEntry.Key.ToString() + " in Required. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Targets":
					{
						if (CUnlockConditionTarget.Parse(ucEntry, fileName, out var unlockConditionTarget))
						{
							list5.Add(unlockConditionTarget);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TargetsRequired":
					{
						if (YMLShared.ParseIntValue(ucEntry.Value.ToString(), "TargetsRequired", fileName, out var value))
						{
							targetsRequired = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UnlockCombined":
					{
						if (GetUnlockContainer(ucEntry, fileName, orOperator: false, out var unlockContainer2))
						{
							requiredChoiceContainer = unlockContainer2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UnlockChoice":
					{
						if (GetUnlockContainer(ucEntry, fileName, orOperator: true, out var unlockContainer))
						{
							requiredChoiceContainer = unlockContainer;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SingleScenario":
					{
						if (YMLShared.GetBoolPropertyValue(ucEntry.Value, "SingleScenario", fileName, out var value4))
						{
							singleScenario = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Difficulty":
					{
						if (list6 == null)
						{
							list6 = new List<EAdventureDifficulty>();
						}
						if (YMLShared.GetStringList(ucEntry.Value, "Difficulty", fileName, out var values))
						{
							foreach (string difficultyString in values)
							{
								EAdventureDifficulty eAdventureDifficulty = CAdventureDifficulty.AdventureDifficulties.SingleOrDefault((EAdventureDifficulty x) => x.ToString() == difficultyString);
								if (eAdventureDifficulty == EAdventureDifficulty.None)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Difficulty entry " + difficultyString + " in file: " + fileName);
									flag = false;
								}
								else
								{
									list6.Add(eAdventureDifficulty);
								}
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockCondition entry " + ucEntry.Key.ToString() + " in file: " + fileName);
						flag = false;
						break;
					}
				}
				catch (Exception ex)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Exception during Unlock Condition processing in file: " + fileName + "\n" + ex.Message + "\n" + ex.StackTrace);
					flag = false;
				}
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockCondition list entry, must be sequence (list) of \"- Key: Value\" mappings. File: " + fileName);
				flag = false;
			}
		}
		if (flag)
		{
			unlockCondition = new CUnlockCondition(chapter, prosperity, reputation, gold, personalQuestStep, list, list2, list3, requiredChoiceContainer, list4, list5, ordered, targetsRequired, singleScenario, list6);
		}
		return flag;
	}

	public static bool GetUnlockContainer(MappingEntry keyMappingEntry, string fileName, bool orOperator, out List<CUnlockChoiceContainer> unlockContainer)
	{
		unlockContainer = new List<CUnlockChoiceContainer>();
		bool result = true;
		try
		{
			if (YMLShared.GetMapping(keyMappingEntry, fileName, out var mapping))
			{
				foreach (MappingEntry entry in mapping.Entries)
				{
					if (GetUnlocks(entry, fileName, "Unlock Choice", out var unlocks, out var required, out var requiredDescription))
					{
						unlockContainer.Add(new CUnlockChoiceContainer(unlocks, required, orOperator, requiredDescription));
					}
					else
					{
						result = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "An exception occurred while parsing an Unlock Choice Container.\nException:" + ex.Message + "\n" + ex.StackTrace + "\nFile: " + fileName);
			result = false;
		}
		return result;
	}

	public static bool GetUnlocks(MappingEntry keyMapping, string fileName, string callerName, out List<Tuple<EUnlockConditionType, string>> unlocks, out List<Tuple<EUnlockConditionType, int>> required, out string requiredDescription)
	{
		unlocks = new List<Tuple<EUnlockConditionType, string>>();
		required = new List<Tuple<EUnlockConditionType, int>>();
		requiredDescription = null;
		bool result = true;
		if (YMLShared.GetMapping(keyMapping, fileName, out var mapping))
		{
			foreach (MappingEntry ConditionEntry in mapping.Entries)
			{
				EUnlockConditionType eUnlockConditionType = UnlockConditionTypes.SingleOrDefault((EUnlockConditionType x) => x.ToString() == ConditionEntry.Key.ToString());
				if (eUnlockConditionType != EUnlockConditionType.None)
				{
					unlocks.Add(new Tuple<EUnlockConditionType, string>(eUnlockConditionType, ConditionEntry.Value.ToString()));
				}
				else if (ConditionEntry.Key.ToString() == "Required")
				{
					if (YMLShared.GetMapping(ConditionEntry, fileName, out var mapping2))
					{
						foreach (MappingEntry reqConditionEntry in mapping2.Entries)
						{
							EUnlockConditionType eUnlockConditionType2 = UnlockConditionTypes.SingleOrDefault((EUnlockConditionType x) => x.ToString() == reqConditionEntry.Key.ToString());
							if (eUnlockConditionType2 != EUnlockConditionType.None)
							{
								if (int.TryParse(reqConditionEntry.Value.ToString(), out var result2))
								{
									required.Add(new Tuple<EUnlockConditionType, int>(eUnlockConditionType2, result2));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Required entry, must be \"UnlockCondition: Count\" File: " + fileName);
								result = false;
							}
							else if (reqConditionEntry.Key.ToString() == "Description")
							{
								if (YMLShared.GetStringPropertyValue(reqConditionEntry.Value, "Description", fileName, out var value))
								{
									requiredDescription = value;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Entry " + reqConditionEntry.Key.ToString() + " in Required. File: " + fileName);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
				}
				else
				{
					result = false;
				}
			}
		}
		return result;
	}
}
