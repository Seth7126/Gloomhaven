using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

[Serializable]
public class StatsDataStorage : ISerializable
{
	public bool m_IsStorageNew = true;

	public double m_TimeLTD;

	public int m_RoguelikeRunCount;

	public int m_RoguelikeRunVictoryCount;

	public double m_RoguelikeTimeLTD;

	public int m_RoguelikeGoldEarningsLTD;

	public int m_CampaignRunCount;

	public int m_CampaignRunVictoryCount;

	public double m_CampaignTimeLTD;

	public double m_CampaignTimeCurrentRun;

	public int m_CampaignGoldEarningsLTD;

	public double m_NewAdventureModeLTD;

	public string m_RunSessionID;

	public string m_MatchSessionID;

	public double m_CurrentEventTime;

	public double m_TutorialTimeLTD;

	public double m_TutorialTimeCurrentRun;

	public double m_CurrentTutorialStepTime;

	public bool m_CompletedTutorial;

	public int m_TotalKills;

	public Dictionary<string, int> m_DefeatedAdventureBosses;

	public EResult m_LastResult;

	public StatsState m_ScenarioStats;

	public int m_TotalEnemiesLastScenario;

	public Tuple<int, List<ECharacter>> HighestItemsUsedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestGoldLootedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestXPEarnedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestChestLootedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsStorageNew", m_IsStorageNew);
		info.AddValue("TimeLTD", m_TimeLTD);
		info.AddValue("RoguelikeRunCount", m_RoguelikeRunCount);
		info.AddValue("RoguelikeRunVictoryCount", m_RoguelikeRunVictoryCount);
		info.AddValue("RoguelikeTimeLTD", m_RoguelikeTimeLTD);
		info.AddValue("RoguelikeGoldEarningsLTD", m_RoguelikeGoldEarningsLTD);
		info.AddValue("CampaignRunCount", m_CampaignRunCount);
		info.AddValue("CampaignRunVictoryCount", m_CampaignRunVictoryCount);
		info.AddValue("CampaignTimeLTD", m_CampaignTimeLTD);
		info.AddValue("CampaignTimeCurrentRun", m_CampaignTimeCurrentRun);
		info.AddValue("CampaignGoldEarningsLTD", m_CampaignGoldEarningsLTD);
		info.AddValue("m_NewAdventureModeLTD", m_NewAdventureModeLTD);
		info.AddValue("CurrentEventTime", m_CurrentEventTime);
		info.AddValue("TutorialTimeLTD", m_TutorialTimeLTD);
		info.AddValue("TutorialTimeCurrentRun", m_TutorialTimeCurrentRun);
		info.AddValue("CurrentTutorialStepTime", m_CurrentTutorialStepTime);
		info.AddValue("CompletedTutorial", m_CompletedTutorial);
		info.AddValue("TotalKills", m_TotalKills);
		info.AddValue("DefeatedAdventureBosses", m_DefeatedAdventureBosses);
	}

	private StatsDataStorage(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsStorageNew":
					m_IsStorageNew = info.GetBoolean("IsStorageNew");
					break;
				case "TimeLTD":
					m_TimeLTD = info.GetDouble("TimeLTD");
					break;
				case "RoguelikeRunCount":
					m_RoguelikeRunCount = info.GetInt32("RoguelikeRunCount");
					break;
				case "RoguelikeRunVictoryCount":
					m_RoguelikeRunVictoryCount = info.GetInt32("RoguelikeRunVictoryCount");
					break;
				case "RoguelikeTimeLTD":
					m_RoguelikeTimeLTD = info.GetDouble("RoguelikeTimeLTD");
					break;
				case "RoguelikeGoldEarningsLTD":
					m_RoguelikeGoldEarningsLTD = info.GetInt32("RoguelikeGoldEarningsLTD");
					break;
				case "CampaignRunCount":
					m_CampaignRunCount = info.GetInt32("CampaignRunCount");
					break;
				case "CampaignRunVictoryCount":
					m_CampaignRunVictoryCount = info.GetInt32("CampaignRunVictoryCount");
					break;
				case "CampaignTimeLTD":
					m_CampaignTimeLTD = info.GetDouble("CampaignTimeLTD");
					break;
				case "CampaignTimeCurrentRun":
					m_CampaignTimeCurrentRun = info.GetDouble("CampaignTimeCurrentRun");
					break;
				case "CampaignGoldEarningsLTD":
					m_CampaignGoldEarningsLTD = info.GetInt32("CampaignGoldEarningsLTD");
					break;
				case "m_NewAdventureModeLTD":
					m_NewAdventureModeLTD = info.GetDouble("m_NewAdventureModeLTD");
					break;
				case "CurrentEventTime":
					m_CurrentEventTime = info.GetInt32("CurrentEventTime");
					break;
				case "TutorialTimeLTD":
					m_TutorialTimeLTD = info.GetDouble("TutorialTimeLTD");
					break;
				case "TutorialTimeCurrentRun":
					m_TutorialTimeCurrentRun = info.GetDouble("TutorialTimeCurrentRun");
					break;
				case "CurrentTutorialStepTime":
					m_CurrentTutorialStepTime = info.GetInt32("CurrentTutorialStepTime");
					break;
				case "CompletedTutorial":
					m_CompletedTutorial = info.GetBoolean("CompletedTutorial");
					break;
				case "TotalKills":
					m_TotalKills = info.GetInt32("TotalKills");
					break;
				case "DefeatedAdventureBosses":
					m_DefeatedAdventureBosses = (Dictionary<string, int>)info.GetValue("DefeatedAdventureBosses", typeof(Dictionary<string, int>));
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to deserialize StatsDataStorage entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (m_DefeatedAdventureBosses == null)
		{
			m_DefeatedAdventureBosses = new Dictionary<string, int>();
		}
	}

	public StatsDataStorage()
	{
		m_DefeatedAdventureBosses = new Dictionary<string, int>();
		m_CompletedTutorial = false;
	}

	public void OldScrapeEventLog()
	{
		m_TotalKills += SEventLog.FindAllActorEventsOfSubTypeAndActorType(ESESubTypeActor.ActorOnDeath, CActor.EType.Enemy).Count;
		SEventLog.ClearEventLog();
	}

	public bool GetPlayer(Dictionary<string, CPlayerStatsScenario> playerStats, string classID, string playerTurn, out CPlayerStatsScenario currentPlayerStat)
	{
		bool flag = false;
		currentPlayerStat = null;
		flag = classID != null && playerStats.TryGetValue(classID, out currentPlayerStat);
		if (!flag)
		{
			flag = playerTurn != null && playerStats.TryGetValue(playerTurn, out currentPlayerStat);
		}
		return flag;
	}

	public void ScrapeEventLog(EResult result, bool endScenario, int round = 0)
	{
		try
		{
			m_LastResult = result;
			string scenarioResult = result.ToString();
			ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			string questType = ((SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.InProgressQuestState != null) ? SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.InProgressQuestState.Quest.Type.ToString() : "Story");
			if ((round == 1 && !endScenario) || Choreographer.s_Choreographer.m_CurrentState.Stats == null)
			{
				Choreographer.s_Choreographer.m_CurrentState.Stats = new StatsState(currentScenarioState.Players);
			}
			m_ScenarioStats = Choreographer.s_Choreographer.m_CurrentState.Stats;
			try
			{
				List<SEventActor> list = SEventLog.FindAllEventsOfType<SEventActor>(checkQueue: true);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is SEventActorUsedItem)
					{
						continue;
					}
					CActor.EType actorType = list[i].ActorType;
					ConvertSummon(list[i].ActorType, list[i].ActorEnemySummon, list[i].ActedOnType, list[i].ActedOnEnemySummon, out var ActorType, out var ActedOnType);
					switch (actorType)
					{
					case CActor.EType.Enemy:
					case CActor.EType.Enemy2:
						m_ScenarioStats.m_MonsterStats.Actor.Add(new CPlayerStatsDamage(CActor.ECauseOfDamage.None, 0, 0, 0, 0, isMelee: true, 0, 0, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list[i].Round, list[i].ActedOnByClassID, list[i].ActorClassID, ActorType, ActedOnType, list[i].Elements, list[i].PositiveConditions, list[i].NegativeConditions, list[i].ActedOnPositiveConditions, list[i].ActedOnNegativeConditions, list[i].ActedOnByGUID, list[i].ActorGuid, list[i].Health, list[i].MaxHealth, list[i].CardID, list[i].CardType, list[i].AbilityType, list[i].ActingAbilityName, list[i].AbilityStrength));
						break;
					case CActor.EType.Player:
					case CActor.EType.HeroSummon:
					{
						if (actorType == CActor.EType.Player && list[i].ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(list[i].ActorClassID, out var value) && list[i].ActorSubType != ESESubTypeActor.ActorGainXP)
						{
							value.Actor.Add(new CPlayerStatsDamage(CActor.ECauseOfDamage.None, 0, 0, 0, 0, isMelee: true, 0, 0, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list[i].Round, list[i].ActedOnByClassID, list[i].ActorClassID, ActorType, ActedOnType, list[i].Elements, list[i].PositiveConditions, list[i].NegativeConditions, list[i].ActedOnPositiveConditions, list[i].ActedOnNegativeConditions, list[i].ActedOnByGUID, list[i].ActorGuid, list[i].Health, list[i].MaxHealth, list[i].CardID, list[i].CardType, list[i].AbilityType, list[i].ActingAbilityName, list[i].AbilityStrength));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActorType))
						{
							m_ScenarioStats.m_HeroSummonStats.Actor.Add(new CPlayerStatsDamage(CActor.ECauseOfDamage.None, 0, 0, 0, 0, isMelee: true, 0, 0, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list[i].Round, list[i].ActedOnByClassID, list[i].ActorClassID, ActorType, ActedOnType, list[i].Elements, list[i].PositiveConditions, list[i].NegativeConditions, list[i].ActedOnPositiveConditions, list[i].ActedOnNegativeConditions, list[i].ActedOnByGUID, list[i].ActorGuid, list[i].Health, list[i].MaxHealth, list[i].CardID, list[i].CardType, list[i].AbilityType, list[i].ActingAbilityName, list[i].AbilityStrength));
						}
						break;
					}
					}
				}
				foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat3 in m_ScenarioStats.m_PlayerStats)
				{
					foreach (CPlayerStatsDamage item in playerStat3.Value.Actor)
					{
						item.ScenarioResult = scenarioResult;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not process Scraping Actor Event Log.cs.\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
			try
			{
				List<SEventActorDamaged> list2 = SEventLog.FindAllEventsOfType<SEventActorDamaged>();
				for (int j = 0; j < list2.Count; j++)
				{
					int damageTaken = list2[j].DamageTaken;
					CActor.EType actorType2 = list2[j].ActorType;
					ConvertSummon(list2[j].ActorType, list2[j].ActorEnemySummon, list2[j].ActedOnType, list2[j].ActedOnEnemySummon, out var ActorType2, out var ActedOnType2);
					switch (actorType2)
					{
					case CActor.EType.Enemy:
					case CActor.EType.Enemy2:
					{
						m_ScenarioStats.m_TotalDamageDoneLastScenario += damageTaken;
						if (GetPlayer(m_ScenarioStats.m_PlayerStats, list2[j].ActedOnByClassID, list2[j].CurrentPhaseActorName, out var currentPlayerStat2))
						{
							currentPlayerStat2.DamageDealt.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, list2[j].HasFavorite));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActedOnType2))
						{
							bool rolledIntoSummoner2 = false;
							if (!string.IsNullOrEmpty(list2[j].ActedOnSummonerID) && GetPlayer(m_ScenarioStats.m_PlayerStats, list2[j].ActedOnSummonerID, "", out var currentPlayerStat3))
							{
								ActedOnType2 = CActor.EType.Player.ToString();
								currentPlayerStat3.DamageDealt.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnSummonerID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: true, rolledIntoSummoner: false, list2[j].HasFavorite));
								rolledIntoSummoner2 = true;
							}
							m_ScenarioStats.m_HeroSummonStats.DamageDealt.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner2, list2[j].HasFavorite));
						}
						m_ScenarioStats.m_MonsterStats.DamageReceived.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, list2[j].HasFavorite));
						break;
					}
					case CActor.EType.Player:
					case CActor.EType.HeroSummon:
					{
						m_ScenarioStats.m_TotalDamageTakenLastScenario += damageTaken;
						if (actorType2 == CActor.EType.Player && list2[j].ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(list2[j].ActorClassID, out var value2))
						{
							value2.DamageReceived.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, list2[j].HasFavorite));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActorType2))
						{
							bool rolledIntoSummoner = false;
							if (!string.IsNullOrEmpty(list2[j].ActorSummonerID) && GetPlayer(m_ScenarioStats.m_PlayerStats, list2[j].ActorSummonerID, "", out var currentPlayerStat))
							{
								ActorType2 = CActor.EType.Player.ToString();
								currentPlayerStat.DamageReceived.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorSummonerID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: true, rolledIntoSummoner: false, list2[j].HasFavorite));
								rolledIntoSummoner = true;
							}
							m_ScenarioStats.m_HeroSummonStats.DamageReceived.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActedOnByGUID, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner, list2[j].HasFavorite));
						}
						m_ScenarioStats.m_MonsterStats.DamageDealt.Add(new CPlayerStatsDamage(list2[j].CauseOfDamage, list2[j].DamageTaken, list2[j].Avoided, list2[j].Shielded, list2[j].ItemShielded, list2[j].IsMelee, list2[j].PoisonDamage, list2[j].Shield, scenarioResult, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list2[j].Round, list2[j].ActedOnByClassID, list2[j].ActorClassID, ActorType2, ActedOnType2, list2[j].Elements, list2[j].PositiveConditions, list2[j].NegativeConditions, list2[j].ActedOnPositiveConditions, list2[j].ActedOnNegativeConditions, list2[j].ActorGuid, list2[j].ActorGuid, list2[j].Health, list2[j].MaxHealth, list2[j].CardID, list2[j].CardType, list2[j].AbilityType, list2[j].ActingAbilityName, list2[j].AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, list2[j].HasFavorite));
						break;
					}
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("Could not process Scraping Damage Event Log.cs.\n" + ex2.Message + "\n" + ex2.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
			}
			try
			{
				m_TotalEnemiesLastScenario = ScenarioManager.CurrentScenarioState.Monsters.Count;
				foreach (SEventActor item2 in SEventLog.FindAllActorEventsOfSubType(ESESubTypeActor.ActorOnDeath))
				{
					ConvertSummon(item2.ActorType, item2.ActorEnemySummon, item2.ActedOnType, item2.ActedOnEnemySummon, out var ActorType3, out var ActedOnType3);
					if (item2.ActorType == CActor.EType.Enemy || item2.ActorType == CActor.EType.Enemy2)
					{
						m_TotalKills++;
						m_ScenarioStats.m_EnemiesKilledLastScenario++;
						if (GetPlayer(m_ScenarioStats.m_PlayerStats, item2.ActedOnByClassID, item2.CurrentPhaseActorName, out var currentPlayerStat4))
						{
							ActedOnType3 = CActor.EType.Player.ToString();
							currentPlayerStat4.Kills.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, item2.Items, item2.Doom, item2.MonsterType, item2.AttackerAtDisadvantage, item2.TargetAdjacent, item2.AllyAdjacent, item2.EnemyAdjacent, item2.ObstacleAdjacent, item2.WallAdjacent, item2.HasFavorite));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActedOnType3))
						{
							bool rolledIntoSummoner3 = false;
							if (!string.IsNullOrEmpty(item2.ActedOnSummonerID) && GetPlayer(m_ScenarioStats.m_PlayerStats, item2.ActedOnSummonerID, "", out var currentPlayerStat5))
							{
								ActedOnType3 = CActor.EType.Player.ToString();
								currentPlayerStat5.Kills.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnSummonerID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: true, rolledIntoSummoner: false, item2.Items, item2.Doom, item2.MonsterType, item2.AttackerAtDisadvantage, targetAdjacent: false, item2.AllyAdjacent, item2.EnemyAdjacent, item2.ObstacleAdjacent, item2.WallAdjacent, item2.HasFavorite));
								rolledIntoSummoner3 = true;
							}
							m_ScenarioStats.m_HeroSummonStats.Kills.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner3, null, item2.Doom, item2.MonsterType, item2.AttackerAtDisadvantage, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, item2.HasFavorite));
						}
						m_ScenarioStats.m_MonsterStats.Deaths.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, null, doom: false, "", disadvantaged: false, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, item2.HasFavorite));
					}
					if (item2.ActorType == CActor.EType.Player || item2.ActorType == CActor.EType.HeroSummon)
					{
						if (item2.ActorType == CActor.EType.Player && GetPlayer(m_ScenarioStats.m_PlayerStats, item2.ActorClassID, item2.CurrentPhaseActorName, out var currentPlayerStat6))
						{
							ActorType3 = CActor.EType.Player.ToString();
							currentPlayerStat6.Deaths.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, null, doom: false, "", disadvantaged: false, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, item2.HasFavorite));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActorType3))
						{
							bool rolledIntoSummoner4 = false;
							m_ScenarioStats.m_HeroSummonStats.Deaths.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner4, null, doom: false, "", disadvantaged: false, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, item2.HasFavorite));
						}
						if (item2.ActedOnType != CActor.EType.Player)
						{
							m_ScenarioStats.m_MonsterStats.Kills.Add(new CPlayerStatsKill(item2.CauseOfDeath, item2.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item2.Round, item2.ActedOnByClassID, item2.ActorClassID, ActorType3, ActedOnType3, item2.Elements, item2.PositiveConditions, item2.NegativeConditions, item2.ActedOnPositiveConditions, item2.ActedOnNegativeConditions, item2.ActedOnByGUID, item2.ActorGuid, item2.MaxHealth, item2.LastDamageAmount, item2.PreDamageHealth, item2.CardID, item2.CardType, item2.AbilityType, item2.ActingAbilityName, item2.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, null, doom: false, "", disadvantaged: false, targetAdjacent: false, 0, 0, 0, wallAdjacent: false, item2.HasFavorite));
						}
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.LogError("Could not process Scraping Kill Event Log.cs.\n" + ex3.Message + "\n" + ex3.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex3.Message);
			}
			List<SEventActorHealed> list3 = SEventLog.FindAllEventsOfType<SEventActorHealed>();
			for (int k = 0; k < list3.Count; k++)
			{
				CActor.EType? actedOnType = list3[k].ActedOnType;
				ConvertSummon(list3[k].ActorType, list3[k].ActorEnemySummon, list3[k].ActedOnType, list3[k].ActedOnEnemySummon, out var ActorType4, out var ActedOnType4);
				if (list3[k].ActedOnByClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(list3[k].ActedOnByClassID, out var value3))
				{
					value3.Heals.Add(new CPlayerStatsHeal(list3[k].HealAmount, list3[k].PoisonRemoved, list3[k].WoundRemoved, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list3[k].Round, list3[k].ActedOnByClassID, list3[k].ActorClassID, ActorType4, ActedOnType4, list3[k].Elements, list3[k].PositiveConditions, list3[k].NegativeConditions, list3[k].ActedOnPositiveConditions, list3[k].ActedOnNegativeConditions, list3[k].ActedOnByGUID, list3[k].ActorGuid, list3[k].CardID, list3[k].CardType, list3[k].AbilityType, list3[k].ActingAbilityName, list3[k].AbilityStrength));
				}
				if (list3[k].ActedOnType == CActor.EType.HeroSummon)
				{
					bool rolledIntoSummoner5 = false;
					if (!string.IsNullOrEmpty(list3[k].ActedOnSummonerID) && GetPlayer(m_ScenarioStats.m_PlayerStats, list3[k].ActedOnSummonerID, "", out var currentPlayerStat7))
					{
						ActedOnType4 = CActor.EType.Player.ToString();
						currentPlayerStat7.Heals.Add(new CPlayerStatsHeal(list3[k].HealAmount, list3[k].PoisonRemoved, list3[k].WoundRemoved, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list3[k].Round, list3[k].ActedOnSummonerID, list3[k].ActorClassID, ActorType4, ActedOnType4, list3[k].Elements, list3[k].PositiveConditions, list3[k].NegativeConditions, list3[k].ActedOnPositiveConditions, list3[k].ActedOnNegativeConditions, list3[k].ActedOnByGUID, list3[k].ActorGuid, list3[k].CardID, list3[k].CardType, list3[k].AbilityType, list3[k].ActingAbilityName, list3[k].AbilityStrength, performedBySummons: true));
						rolledIntoSummoner5 = true;
					}
					m_ScenarioStats.m_HeroSummonStats.Heals.Add(new CPlayerStatsHeal(list3[k].HealAmount, list3[k].PoisonRemoved, list3[k].WoundRemoved, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list3[k].Round, list3[k].ActedOnByClassID, list3[k].ActorClassID, ActorType4, ActedOnType4, list3[k].Elements, list3[k].PositiveConditions, list3[k].NegativeConditions, list3[k].ActedOnPositiveConditions, list3[k].ActedOnNegativeConditions, list3[k].ActedOnByGUID, list3[k].ActorGuid, list3[k].CardID, list3[k].CardType, list3[k].AbilityType, list3[k].ActingAbilityName, list3[k].AbilityStrength, performedBySummons: false, rolledIntoSummoner5));
				}
				if (list3[k].ActedOnType == CActor.EType.Enemy || list3[k].ActedOnType == CActor.EType.Enemy2)
				{
					m_ScenarioStats.m_MonsterStats.Heals.Add(new CPlayerStatsHeal(list3[k].HealAmount, list3[k].PoisonRemoved, list3[k].WoundRemoved, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, list3[k].Round, list3[k].ActedOnByClassID, list3[k].ActorClassID, ActorType4, ActedOnType4, list3[k].Elements, list3[k].PositiveConditions, list3[k].NegativeConditions, list3[k].ActedOnPositiveConditions, list3[k].ActedOnNegativeConditions, list3[k].ActedOnByGUID, list3[k].ActorGuid, list3[k].CardID, list3[k].CardType, list3[k].AbilityType, list3[k].ActingAbilityName, list3[k].AbilityStrength));
				}
				if (actedOnType == CActor.EType.Player || actedOnType == CActor.EType.HeroSummon)
				{
					m_ScenarioStats.m_TotalHealingDoneLastScenario += list3[k].HealAmount;
				}
			}
			foreach (SEventAbility item3 in SEventLog.FindAllAbilityEventsOfTypeAndSubType(CAbility.EAbilityType.DestroyObstacle, ESESubTypeAbility.AbilityEnded))
			{
				if (item3 is SEventAbilityDestroyObstacle { ActorClassID: not null } sEventAbilityDestroyObstacle && m_ScenarioStats.m_PlayerStats.TryGetValue(sEventAbilityDestroyObstacle.ActorClassID, out var value4))
				{
					value4.DestroyedObstacles.Add(new CPlayerStatsDestroyObstacle(sEventAbilityDestroyObstacle.DestroyedPropsDictionary, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, sEventAbilityDestroyObstacle.Round, sEventAbilityDestroyObstacle.ActorClassID, "", sEventAbilityDestroyObstacle.ActorType.ToString(), "", sEventAbilityDestroyObstacle.Elements, null, null, null, null, "", "", sEventAbilityDestroyObstacle.CardID, sEventAbilityDestroyObstacle.CardType, CAbility.EAbilityType.DestroyObstacle, sEventAbilityDestroyObstacle.Name, sEventAbilityDestroyObstacle.Strength));
				}
			}
			foreach (SEventEnhancement item4 in SEventLog.FindAllEventsOfType<SEventEnhancement>())
			{
				if (item4.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item4.CharacterID, out var value5))
				{
					value5.Enhancements.Add(new CPlayerStatsEnhancements(item4.Amount, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item4.Round, item4.CharacterID, "", "", "", item4.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventLoseCard item5 in SEventLog.FindAllEventsOfType<SEventLoseCard>())
			{
				if (item5.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item5.CharacterID, out var value6))
				{
					value6.LoseCards.Add(new CPlayerStatsLoseCard(item5.Amount, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item5.Round, item5.CharacterID, "", "", "", item5.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventDiscardCard item6 in SEventLog.FindAllEventsOfType<SEventDiscardCard>())
			{
				if (item6.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item6.CharacterID, out var value7))
				{
					value7.DiscardCard.Add(new CPlayerStatsDiscardCard(item6.CardID, item6.Pile, item6.TimeStamp, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item6.Round, item6.CharacterID, "", "", "", item6.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventDonate item7 in SEventLog.FindAllEventsOfType<SEventDonate>())
			{
				if (item7.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item7.CharacterID, out var value8))
				{
					value8.Donations.Add(new CPlayerStatsDonations(item7.Amount, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item7.Round, item7.CharacterID, "", "", "", item7.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventPersonalQuest item8 in SEventLog.FindAllEventsOfType<SEventPersonalQuest>())
			{
				if (item8.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item8.CharacterID, out var value9))
				{
					value9.PersonalQuests.Add(new CPlayerStatsPersonalQuests(item8.Amount, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item8.Round, item8.CharacterID, "", "", "", item8.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventPerk item9 in SEventLog.FindAllEventsOfType<SEventPerk>())
			{
				if (item9.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item9.CharacterID, out var value10))
				{
					value10.BattleGoalPerks.Add(new CPlayerStatsBattlePerks(item9.PerkPoints, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item9.Round, item9.CharacterID, "", "", "", item9.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventHand item10 in SEventLog.FindAllEventsOfType<SEventHand>())
			{
				if (item10.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item10.CharacterID, out var value11))
				{
					value11.Hand.Add(new CPlayerStatsHand(item10.HandSize, item10.DiscardSize, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item10.Round, item10.CharacterID, "", "", "", item10.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventLostAdjacency item11 in SEventLog.FindAllEventsOfType<SEventLostAdjacency>())
			{
				if (item11.CharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item11.CharacterID, out var value12))
				{
					value12.LostAdjacency.Add(new CPlayerStatsLostAdjacency(item11.CharacterID, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item11.Round, item11.CharacterID, "", "", "", item11.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventObjectProp item12 in SEventLog.FindAllEventsOfType<SEventObjectProp>())
			{
				if (item12.ObjectAction == "Activate" && item12.ObjectType == ScenarioManager.ObjectImportType.Door && item12.ObjectCharacterID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item12.ObjectCharacterID, out var value13))
				{
					value13.Door.Add(new CPlayerStatsDoor(1, item12.ObjectMaps, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item12.Round, item12.ObjectCharacterID, "", "", "", item12.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventObjectPropTrap item13 in SEventLog.FindAllEventsOfType<SEventObjectPropTrap>())
			{
				if (item13.ObjectType == ScenarioManager.ObjectImportType.Trap && item13.ActionActor != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item13.ActionActor, out var value14))
				{
					value14.Trap.Add(new CPlayerStatsTrap(item13.Damage, item13.ObjectAction, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item13.Round, item13.ActionActor, "", "", "", item13.Elements, null, null, null, null, "", ""));
				}
			}
			foreach (SEventRound item14 in SEventLog.FindAllEventsOfType<SEventRound>())
			{
				foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat4 in m_ScenarioStats.m_PlayerStats)
				{
					playerStat4.Value.Monsters.Add(new CPlayerStatsMonsters(item14.MonsterCount, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item14.Round, playerStat4.Key, "", "", "", item14.Elements, null, null, null, null, "", "", 0, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0));
				}
			}
			List<CObjectProp> activatedProps = ScenarioManager.CurrentScenarioState.ActivatedProps;
			for (int l = 0; l < activatedProps.Count; l++)
			{
				if (activatedProps[l].ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
				{
					m_ScenarioStats.m_ScenarioGoldLastScenario++;
				}
				else if (activatedProps[l].ObjectType == ScenarioManager.ObjectImportType.Chest || activatedProps[l].ObjectType == ScenarioManager.ObjectImportType.GoalChest)
				{
					m_ScenarioStats.m_ScenarioChestsLastScenario++;
				}
			}
			List<SEventActorLooted> list4 = SEventLog.FindAllEventsOfType<SEventActorLooted>();
			for (int m = 0; m < list4.Count; m++)
			{
				ScenarioManager.ObjectImportType lootedType = list4[m].LootedType;
				SEventActorLooted sEventActorLooted = list4[m];
				if (list4[m].ActorType != CActor.EType.Player)
				{
					continue;
				}
				ConvertSummon(sEventActorLooted.ActorType, sEventActorLooted.ActorEnemySummon, sEventActorLooted.ActedOnType, sEventActorLooted.ActedOnEnemySummon, out var ActorType5, out var ActedOnType5);
				if (sEventActorLooted.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(sEventActorLooted.ActorClassID, out var value15))
				{
					value15.Loot.Add(new CPlayerStatsLoot(sEventActorLooted.LootedType.ToString(), SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, sEventActorLooted.Round, sEventActorLooted.ActorClassID, sEventActorLooted.ActedOnByClassID, ActorType5, ActedOnType5, sEventActorLooted.Elements, sEventActorLooted.PositiveConditions, sEventActorLooted.NegativeConditions, sEventActorLooted.ActedOnPositiveConditions, sEventActorLooted.ActedOnNegativeConditions, sEventActorLooted.ActorGuid, sEventActorLooted.ActorDroppingLoot, sEventActorLooted.CardID, sEventActorLooted.CardType, sEventActorLooted.AbilityType, sEventActorLooted.ActingAbilityName, sEventActorLooted.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, sEventActorLooted.AllyAdjacent, sEventActorLooted.EnemyAdjacent, sEventActorLooted.ObstacleAdjacent, sEventActorLooted.WallAdjacent));
				}
				switch (lootedType)
				{
				case ScenarioManager.ObjectImportType.MoneyToken:
					m_ScenarioStats.m_GoldPilesLootedLastScenario++;
					if (m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter.ContainsKey(list4[m].ActorClassID))
					{
						m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter[list4[m].ActorClassID]++;
					}
					else if (list4[m].ActorClassID != null)
					{
						m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter.Add(list4[m].ActorClassID, 1);
					}
					break;
				case ScenarioManager.ObjectImportType.Chest:
				case ScenarioManager.ObjectImportType.GoalChest:
					m_ScenarioStats.m_ChestsLootedLastScenario++;
					if (m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter.ContainsKey(list4[m].ActorClassID))
					{
						m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter[list4[m].ActorClassID]++;
					}
					else if (list4[m].ActorClassID != null)
					{
						m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter.Add(list4[m].ActorClassID, 1);
					}
					break;
				}
			}
			List<SEventActorEarnedAbilityXP> list5 = SEventLog.FindAllEventsOfType<SEventActorEarnedAbilityXP>();
			for (int n = 0; n < list5.Count; n++)
			{
				if (list5[n].ActorType == CActor.EType.Player)
				{
					SEventActorEarnedAbilityXP sEventActorEarnedAbilityXP = list5[n];
					ConvertSummon(sEventActorEarnedAbilityXP.ActorType, sEventActorEarnedAbilityXP.ActorEnemySummon, sEventActorEarnedAbilityXP.ActedOnType, sEventActorEarnedAbilityXP.ActedOnEnemySummon, out var ActorType6, out var ActedOnType6);
					if (sEventActorEarnedAbilityXP.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(sEventActorEarnedAbilityXP.ActorClassID, out var value16))
					{
						value16.XP.Add(new CPlayerStatsXP(sEventActorEarnedAbilityXP.XPEarned, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, sEventActorEarnedAbilityXP.Round, sEventActorEarnedAbilityXP.ActorClassID, sEventActorEarnedAbilityXP.ActedOnByClassID, ActorType6, ActedOnType6, sEventActorEarnedAbilityXP.Elements, sEventActorEarnedAbilityXP.PositiveConditions, sEventActorEarnedAbilityXP.NegativeConditions, sEventActorEarnedAbilityXP.ActedOnPositiveConditions, sEventActorEarnedAbilityXP.ActedOnNegativeConditions, sEventActorEarnedAbilityXP.ActorGuid, "", sEventActorEarnedAbilityXP.CardID, sEventActorEarnedAbilityXP.CardType, sEventActorEarnedAbilityXP.AbilityType, sEventActorEarnedAbilityXP.ActingAbilityName, sEventActorEarnedAbilityXP.AbilityStrength));
					}
					m_ScenarioStats.m_TotalXPEarnedLastScenario += list5[n].XPEarned;
					if (m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter.ContainsKey(list5[n].ActorClassID))
					{
						m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter[list5[n].ActorClassID] += list5[n].XPEarned;
					}
					else if (list5[n].ActorClassID != null)
					{
						m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter.Add(list5[n].ActorClassID, list5[n].XPEarned);
					}
				}
			}
			List<SEventActorUsedItem> list6 = SEventLog.FindAllEventsOfType<SEventActorUsedItem>();
			for (int num = 0; num < list6.Count; num++)
			{
				SEventActorUsedItem sEventActorUsedItem = list6[num];
				m_ScenarioStats.m_ItemsUsedLastScenario++;
				if (m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter.ContainsKey(sEventActorUsedItem.ActorClassID))
				{
					m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter[sEventActorUsedItem.ActorClassID]++;
				}
				else if (sEventActorUsedItem.ActorClassID != null)
				{
					m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter.Add(sEventActorUsedItem.ActorClassID, 1);
				}
				ConvertSummon(sEventActorUsedItem.ActorType, sEventActorUsedItem.ActorEnemySummon, sEventActorUsedItem.ActedOnType, sEventActorUsedItem.ActedOnEnemySummon, out var ActorType7, out var ActedOnType7);
				if (sEventActorUsedItem.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(sEventActorUsedItem.ActorClassID, out var value17))
				{
					value17.Items.Add(new CPlayerStatsItem(sEventActorUsedItem.ItemName, sEventActorUsedItem.Slot, sEventActorUsedItem.FirstTimeUse, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, sEventActorUsedItem.Round, sEventActorUsedItem.ActorClassID, sEventActorUsedItem.ActedOnByClassID, ActorType7, ActedOnType7, sEventActorUsedItem.Elements, sEventActorUsedItem.PositiveConditions, sEventActorUsedItem.NegativeConditions, sEventActorUsedItem.ActedOnPositiveConditions, sEventActorUsedItem.ActedOnNegativeConditions, sEventActorUsedItem.ActorGuid, "", sEventActorUsedItem.CardID, sEventActorUsedItem.CardType, sEventActorUsedItem.AbilityType, sEventActorUsedItem.ActingAbilityName, sEventActorUsedItem.AbilityStrength));
				}
			}
			List<SEventActorEndTurn> list7 = SEventLog.FindAllEventsOfType<SEventActorEndTurn>();
			for (int num2 = 0; num2 < list7.Count; num2++)
			{
				SEventActorEndTurn sEventActorEndTurn = list7[num2];
				ConvertSummon(sEventActorEndTurn.ActorType, sEventActorEndTurn.ActorEnemySummon, sEventActorEndTurn.ActedOnType, sEventActorEndTurn.ActedOnEnemySummon, out var ActorType8, out var ActedOnType8);
				if (sEventActorEndTurn.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(sEventActorEndTurn.ActorClassID, out var value18))
				{
					value18.EndTurn.Add(new CPlayerStatsEndTurn(sEventActorEndTurn.TileX, sEventActorEndTurn.TileY, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, sEventActorEndTurn.Round, sEventActorEndTurn.ActorClassID, sEventActorEndTurn.ActedOnByClassID, ActorType8, ActedOnType8, sEventActorEndTurn.Elements, sEventActorEndTurn.PositiveConditions, sEventActorEndTurn.NegativeConditions, sEventActorEndTurn.ActedOnPositiveConditions, sEventActorEndTurn.ActedOnNegativeConditions, sEventActorEndTurn.ActorGuid, "", sEventActorEndTurn.CardID, sEventActorEndTurn.CardType, sEventActorEndTurn.AbilityType, sEventActorEndTurn.ActingAbilityName, sEventActorEndTurn.AbilityStrength, performedBySummons: false, rolledIntoSummoner: false, sEventActorEndTurn.AllyAdjacent, sEventActorEndTurn.EnemyAdjacent, sEventActorEndTurn.ObstacleAdjacent, sEventActorEndTurn.WallAdjacent));
				}
			}
			try
			{
				foreach (SEventElement item15 in SEventLog.FindAllEventsOfType<SEventElement>())
				{
					ConvertSummon(item15.ActorType, item15.ActorEnemySummon, null, actedOnSummon: false, out var ActorType9, out var _);
					if (item15.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item15.ActorClassID, out var value19))
					{
						if (item15.ElementSubType == ESESubTypeElement.Consumed)
						{
							value19.Consumed.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Consumed, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
						else if (item15.ElementSubType == ESESubTypeElement.Infused)
						{
							value19.Infusions.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Infused, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
					}
					if (CActor.EType.HeroSummon.ToString().Contains(ActorType9))
					{
						if (item15.ElementSubType == ESESubTypeElement.Consumed)
						{
							m_ScenarioStats.m_HeroSummonStats.Consumed.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Consumed, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
						else if (item15.ElementSubType == ESESubTypeElement.Infused)
						{
							m_ScenarioStats.m_HeroSummonStats.Infusions.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Infused, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
					}
					if (item15.ActorType == CActor.EType.Enemy || item15.ActorType == CActor.EType.Enemy2)
					{
						if (item15.ElementSubType == ESESubTypeElement.Consumed)
						{
							m_ScenarioStats.m_MonsterStats.Consumed.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Consumed, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
						else if (item15.ElementSubType == ESESubTypeElement.Infused)
						{
							m_ScenarioStats.m_MonsterStats.Infusions.Add(new CPlayerStatsElement(SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item15.Round, item15.ActorClassID, CPlayerStatsElement.EStatsElementType.Infused, item15.Element, ActorType9, item15.Elements, item15.PositiveConditions, item15.NegativeConditions));
						}
					}
				}
			}
			catch (Exception ex4)
			{
				Debug.LogError("Could not process Scraping Element Event Log.cs.\n" + ex4.Message + "\n" + ex4.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex4.Message);
			}
			try
			{
				List<SEventAbility> list8 = SEventLog.FindAllAbilityEventsOfSubType(ESESubTypeAbility.AbilityEnded);
				list8.AddRange(SEventLog.FindAllAbilityEventsOfSubType(ESESubTypeAbility.ApplyToActor));
				List<SEventAbility> list9 = SEventLog.FindAllAbilityEventsOfSubType(ESESubTypeAbility.AbilityStart);
				if (list9 != null && list9.Count > 0)
				{
					for (int num3 = list9.Count - 1; num3 >= 0; num3--)
					{
						if (list9[num3].AbilityType != CAbility.EAbilityType.Damage || (list9[num3].AbilityType == CAbility.EAbilityType.Damage && list9[num3].ActorClassID != "BerserkerID"))
						{
							list9.RemoveAt(num3);
						}
					}
					list8.AddRange(list9);
				}
				foreach (SEventAbility item16 in list8)
				{
					bool? areaEffect = null;
					int targets = 0;
					bool specialAbility = false;
					int num4 = 1;
					if (item16.AbilityType == CAbility.EAbilityType.Move)
					{
						num4 = ((SEventAbilityMove)item16).TilesMoved;
						specialAbility = ((SEventAbilityMove)item16).Jumped;
					}
					if (item16.AbilityType == CAbility.EAbilityType.Create)
					{
						num4 = ((SEventAbilityCreate)item16).PropsSpawned;
					}
					if (item16.AbilityType == CAbility.EAbilityType.RecoverLostCards)
					{
						num4 = ((SEventAbilityTargeting)item16).Amount;
					}
					if (item16.AbilityType == CAbility.EAbilityType.GiveSupplyCard)
					{
						num4 = ((SEventAbilityTargeting)item16).Amount;
					}
					if (item16.AbilityType == CAbility.EAbilityType.Attack)
					{
						areaEffect = ((SEventAbilityAttack)item16).AreaEffect;
						targets = ((SEventAbilityAttack)item16).Targets;
					}
					if (item16.AbilityType == CAbility.EAbilityType.DisarmTrap)
					{
						num4 = 0;
						foreach (KeyValuePair<string, int> item17 in ((SEventAbilityDisarmTrap)item16).TrapsDisarmedDictionary)
						{
							num4 += item17.Value;
						}
					}
					if ((item16.AbilitySubType != ESESubTypeAbility.ApplyToActor || (item16.AbilityType != CAbility.EAbilityType.RecoverDiscardedCards && item16.AbilityType != CAbility.EAbilityType.PlaySong && item16.AbilityType != CAbility.EAbilityType.RecoverLostCards && item16.AbilityType != CAbility.EAbilityType.RefreshItemCards && item16.AbilityType != CAbility.EAbilityType.LoseCards && item16.AbilityType != CAbility.EAbilityType.IncreaseCardLimit)) && (item16.AbilitySubType != ESESubTypeAbility.AbilityEnded || (item16.AbilityType != CAbility.EAbilityType.Kill && item16.AbilityType != CAbility.EAbilityType.ControlActor && item16.AbilityType != CAbility.EAbilityType.Advantage && item16.AbilityType != CAbility.EAbilityType.Bless && item16.AbilityType != CAbility.EAbilityType.Curse && item16.AbilityType != CAbility.EAbilityType.Disarm && item16.AbilityType != CAbility.EAbilityType.Immobilize && item16.AbilityType != CAbility.EAbilityType.Invisible && item16.AbilityType != CAbility.EAbilityType.Muddle && item16.AbilityType != CAbility.EAbilityType.Poison && item16.AbilityType != CAbility.EAbilityType.Strengthen && item16.AbilityType != CAbility.EAbilityType.Retaliate && item16.AbilityType != CAbility.EAbilityType.Stun && item16.AbilityType != CAbility.EAbilityType.Wound && item16.AbilityType != CAbility.EAbilityType.Shield && item16.AbilityType != CAbility.EAbilityType.AddDoom && item16.AbilityType != CAbility.EAbilityType.StopFlying && item16.AbilityType != CAbility.EAbilityType.Sleep && item16.AbilityType != CAbility.EAbilityType.LoseGoalChestReward)))
					{
						ConvertSummon(item16.ActorType, item16.ActorEnemySummon, item16.ActedOnType, item16.ActedOnEnemySummon, out var ActorType10, out var ActedOnType10);
						if (item16.ActorClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(item16.ActorClassID, out var value20))
						{
							value20.Abilities.Add(new CPlayerStatsAbilities(num4, specialAbility, areaEffect, targets, item16.DefaultAction, item16.HasHappened, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item16.Round, item16.ActorClassID, item16.ActedOnByClassID, ActorType10, ActedOnType10, item16.Elements, item16.PositiveConditions, item16.NegativeConditions, item16.ActedOnPositiveConditions, item16.ActedOnNegativeConditions, "", "", item16.CardID, item16.CardType, item16.AbilityType, item16.Name, item16.Strength));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(ActorType10))
						{
							m_ScenarioStats.m_HeroSummonStats.Abilities.Add(new CPlayerStatsAbilities(num4, specialAbility, areaEffect, targets, item16.DefaultAction, item16.HasHappened, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item16.Round, item16.ActorClassID, item16.ActedOnByClassID, ActorType10, ActedOnType10, item16.Elements, item16.PositiveConditions, item16.NegativeConditions, item16.ActedOnPositiveConditions, item16.ActedOnNegativeConditions, "", "", item16.CardID, item16.CardType, item16.AbilityType, item16.Name, item16.Strength));
						}
						if (item16.ActorType == CActor.EType.Enemy || item16.ActorType == CActor.EType.Enemy2)
						{
							m_ScenarioStats.m_MonsterStats.Abilities.Add(new CPlayerStatsAbilities(num4, specialAbility, areaEffect, targets, item16.DefaultAction, item16.HasHappened, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, item16.Round, item16.ActorClassID, item16.ActedOnByClassID, ActorType10, ActedOnType10, item16.Elements, item16.PositiveConditions, item16.NegativeConditions, item16.ActedOnPositiveConditions, item16.ActedOnNegativeConditions, "", "", item16.CardID, item16.CardType, item16.AbilityType, item16.Name, item16.Strength));
						}
					}
				}
			}
			catch (Exception ex5)
			{
				Debug.LogError("Could not process Scraping Ability Event Log.cs.\n" + ex5.Message + "\n" + ex5.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex5.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex5.Message);
			}
			try
			{
				foreach (SEventAttackModifier modifierEvent in SEventLog.FindAllEventsOfType<SEventAttackModifier>())
				{
					ConvertSummon(modifierEvent.ActorType, modifierEvent.ActorEnemySummon, modifierEvent.ActedOnType, modifierEvent.ActedOnEnemySummon, out var ActorType11, out var ActedOnType11);
					PlayerState playerState = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ActorGuid == modifierEvent.CurrentPhaseActorGuid);
					if (playerState != null && playerState.ClassID != null && m_ScenarioStats.m_PlayerStats.TryGetValue(playerState.ClassID, out var value21))
					{
						value21.Modifiers.Add(new CPlayerStatsModifiers(modifierEvent.UsedAttackModifierStrings, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, modifierEvent.Round, modifierEvent.ActorClass, modifierEvent.ActedOnByClass, ActorType11, ActedOnType11, modifierEvent.Elements, modifierEvent.PositiveConditions, modifierEvent.NegativeConditions, modifierEvent.ActedOnPositiveConditions, modifierEvent.ActedOnNegativeConditions, "", "", 0, CBaseCard.ECardType.None, CAbility.EAbilityType.None, null, 0));
					}
					if (CActor.EType.HeroSummon.ToString().Contains(ActorType11))
					{
						m_ScenarioStats.m_HeroSummonStats.Modifiers.Add(new CPlayerStatsModifiers(modifierEvent.UsedAttackModifierStrings, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, modifierEvent.Round, modifierEvent.ActorClass, modifierEvent.ActedOnByClass, ActorType11, ActedOnType11, modifierEvent.Elements, modifierEvent.PositiveConditions, modifierEvent.NegativeConditions, modifierEvent.ActedOnPositiveConditions, modifierEvent.ActedOnNegativeConditions, "", "", 0, CBaseCard.ECardType.None, CAbility.EAbilityType.None, null, 0));
					}
					if (modifierEvent.ActorType == CActor.EType.Enemy || modifierEvent.ActorType == CActor.EType.Enemy2)
					{
						m_ScenarioStats.m_MonsterStats.Modifiers.Add(new CPlayerStatsModifiers(modifierEvent.UsedAttackModifierStrings, SaveData.Instance.Global.CurrentAdventureData.PartyName, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID, questType, modifierEvent.Round, modifierEvent.ActorClass, modifierEvent.ActedOnByClass, ActorType11, ActedOnType11, modifierEvent.Elements, modifierEvent.PositiveConditions, modifierEvent.NegativeConditions, modifierEvent.ActedOnPositiveConditions, modifierEvent.ActedOnNegativeConditions, "", "", 0, CBaseCard.ECardType.None, CAbility.EAbilityType.None, null, 0));
					}
				}
			}
			catch (Exception ex6)
			{
				Debug.LogError("Could not process Scraping Modifier Event Log.cs.\n" + ex6.Message + "\n" + ex6.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex6.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex6.Message);
			}
			if (endScenario)
			{
				AdventureState.MapState.MapParty.CurrentScenarioStats.Clear();
				AdventureState.MapState.MapParty.LastScenarioStats.Clear();
				AdventureState.MapState.MapParty.LastScenarioHeroSummon.Clear();
				AdventureState.MapState.MapParty.LastScenarioMonster.Clear();
				foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat in m_ScenarioStats.m_PlayerStats)
				{
					PlayerState playerState2 = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ClassID == playerStat.Key);
					playerStat.Value.PlayerSurvivedScenario = !playerState2.IsDead && playerState2.Player.CauseOfDeath == CActor.ECauseOfDeath.StillAlive;
					playerStat.Value.RoundsPlayed = currentScenarioState.RoundNumber;
					AdventureState.MapState.MapParty.LastScenarioStats.Add(playerStat.Value);
					AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == playerStat.Key)?.UpdatePlayerStats(playerStat.Value);
				}
				try
				{
					if (currentScenarioState.HeroSummons != null)
					{
						m_ScenarioStats.m_HeroSummonStats.RoundsPlayed = currentScenarioState.RoundNumber;
						AdventureState.MapState.MapParty.LastScenarioHeroSummon.Add(m_ScenarioStats.m_HeroSummonStats);
						AdventureState.MapState.MapParty.HeroSummonsStats.UpdatePlayerStats(m_ScenarioStats.m_HeroSummonStats);
					}
				}
				catch (Exception ex7)
				{
					Debug.LogError("Could not process Scraping HeroSummon updatestats.\n" + ex7.Message + "\n" + ex7.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex7.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex7.Message);
				}
				try
				{
					if (currentScenarioState.Monsters != null)
					{
						m_ScenarioStats.m_MonsterStats.RoundsPlayed = currentScenarioState.RoundNumber;
						AdventureState.MapState.MapParty.LastScenarioMonster.Add(m_ScenarioStats.m_MonsterStats);
						AdventureState.MapState.MapParty.MonstersStats.UpdatePlayerStats(m_ScenarioStats.m_MonsterStats);
					}
					return;
				}
				catch (Exception ex8)
				{
					Debug.LogError("Could not process Scraping Monster updatestats.\n" + ex8.Message + "\n" + ex8.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex8.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex8.Message);
					return;
				}
			}
			AdventureState.MapState.MapParty.CurrentScenarioStats.Clear();
			foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat2 in m_ScenarioStats.m_PlayerStats)
			{
				PlayerState playerState3 = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ClassID == playerStat2.Key);
				playerStat2.Value.PlayerSurvivedScenario = !playerState3.IsDead;
				playerStat2.Value.RoundsPlayed = currentScenarioState.RoundNumber;
				AdventureState.MapState.MapParty.CurrentScenarioStats.Add(playerStat2.Value);
			}
		}
		catch (Exception ex9)
		{
			Debug.LogError("Could not process Scraping Event Log.cs.\n" + ex9.Message + "\n" + ex9.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex9.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex9.Message);
		}
		finally
		{
			SEventLog.ClearEventLog();
		}
	}

	public void ConvertSummon(CActor.EType? actorType, bool isSummon, CActor.EType? actedOnType, bool actedOnSummon, out string ActorType, out string ActedOnType)
	{
		if ((actorType == CActor.EType.Enemy || actorType == CActor.EType.Enemy2) && isSummon)
		{
			ActorType = "EnemySummon";
		}
		else
		{
			ActorType = actorType.ToString();
		}
		if ((actedOnType == CActor.EType.Enemy || actedOnType == CActor.EType.Enemy2) && actedOnSummon)
		{
			ActedOnType = "EnemySummon";
		}
		else
		{
			ActedOnType = actedOnType.ToString();
		}
	}

	public void UpdateDefeatedBosses(string bossName)
	{
		if (!m_DefeatedAdventureBosses.ContainsKey(bossName))
		{
			m_DefeatedAdventureBosses.Add(bossName, 1);
		}
		else
		{
			m_DefeatedAdventureBosses[bossName]++;
		}
	}
}
