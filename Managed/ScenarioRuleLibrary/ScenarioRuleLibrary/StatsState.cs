using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class StatsState : ISerializable
{
	public int m_ChestsLootedLastScenario;

	public int m_GoldPilesLootedLastScenario;

	public int m_TotalDamageDoneLastScenario;

	public int m_TotalDamageTakenLastScenario;

	public int m_TotalHealingDoneLastScenario;

	public int m_TotalXPEarnedLastScenario;

	public int m_ItemsUsedLastScenario;

	public int m_ScenarioGoldLastScenario;

	public int m_ScenarioChestsLastScenario;

	public int m_EnemiesKilledLastScenario;

	public Dictionary<string, int> m_ChestsLootedLastScenarioByCharacter;

	public Dictionary<string, int> m_GoldPilesLootedLastScenarioByCharacter;

	public Dictionary<string, int> m_TotalXPEarnedLastScenarioByCharacter;

	public Dictionary<string, int> m_ItemsUsedLastScenarioByCharacter;

	public Dictionary<string, CPlayerStatsScenario> m_PlayerStats;

	public CPlayerStatsScenario m_HeroSummonStats;

	public CPlayerStatsScenario m_MonsterStats;

	public StatsState()
	{
	}

	public StatsState(StatsState state, ReferenceDictionary references)
	{
		m_ChestsLootedLastScenario = state.m_ChestsLootedLastScenario;
		m_GoldPilesLootedLastScenario = state.m_GoldPilesLootedLastScenario;
		m_TotalDamageDoneLastScenario = state.m_TotalDamageDoneLastScenario;
		m_TotalDamageTakenLastScenario = state.m_TotalDamageTakenLastScenario;
		m_TotalHealingDoneLastScenario = state.m_TotalHealingDoneLastScenario;
		m_TotalXPEarnedLastScenario = state.m_TotalXPEarnedLastScenario;
		m_ItemsUsedLastScenario = state.m_ItemsUsedLastScenario;
		m_ScenarioGoldLastScenario = state.m_ScenarioGoldLastScenario;
		m_ScenarioChestsLastScenario = state.m_ScenarioChestsLastScenario;
		m_EnemiesKilledLastScenario = state.m_EnemiesKilledLastScenario;
		m_ChestsLootedLastScenarioByCharacter = references.Get(state.m_ChestsLootedLastScenarioByCharacter);
		if (m_ChestsLootedLastScenarioByCharacter == null && state.m_ChestsLootedLastScenarioByCharacter != null)
		{
			m_ChestsLootedLastScenarioByCharacter = new Dictionary<string, int>(state.m_ChestsLootedLastScenarioByCharacter.Comparer);
			foreach (KeyValuePair<string, int> item in state.m_ChestsLootedLastScenarioByCharacter)
			{
				string key = item.Key;
				int value = item.Value;
				m_ChestsLootedLastScenarioByCharacter.Add(key, value);
			}
			references.Add(state.m_ChestsLootedLastScenarioByCharacter, m_ChestsLootedLastScenarioByCharacter);
		}
		m_GoldPilesLootedLastScenarioByCharacter = references.Get(state.m_GoldPilesLootedLastScenarioByCharacter);
		if (m_GoldPilesLootedLastScenarioByCharacter == null && state.m_GoldPilesLootedLastScenarioByCharacter != null)
		{
			m_GoldPilesLootedLastScenarioByCharacter = new Dictionary<string, int>(state.m_GoldPilesLootedLastScenarioByCharacter.Comparer);
			foreach (KeyValuePair<string, int> item2 in state.m_GoldPilesLootedLastScenarioByCharacter)
			{
				string key2 = item2.Key;
				int value2 = item2.Value;
				m_GoldPilesLootedLastScenarioByCharacter.Add(key2, value2);
			}
			references.Add(state.m_GoldPilesLootedLastScenarioByCharacter, m_GoldPilesLootedLastScenarioByCharacter);
		}
		m_TotalXPEarnedLastScenarioByCharacter = references.Get(state.m_TotalXPEarnedLastScenarioByCharacter);
		if (m_TotalXPEarnedLastScenarioByCharacter == null && state.m_TotalXPEarnedLastScenarioByCharacter != null)
		{
			m_TotalXPEarnedLastScenarioByCharacter = new Dictionary<string, int>(state.m_TotalXPEarnedLastScenarioByCharacter.Comparer);
			foreach (KeyValuePair<string, int> item3 in state.m_TotalXPEarnedLastScenarioByCharacter)
			{
				string key3 = item3.Key;
				int value3 = item3.Value;
				m_TotalXPEarnedLastScenarioByCharacter.Add(key3, value3);
			}
			references.Add(state.m_TotalXPEarnedLastScenarioByCharacter, m_TotalXPEarnedLastScenarioByCharacter);
		}
		m_ItemsUsedLastScenarioByCharacter = references.Get(state.m_ItemsUsedLastScenarioByCharacter);
		if (m_ItemsUsedLastScenarioByCharacter == null && state.m_ItemsUsedLastScenarioByCharacter != null)
		{
			m_ItemsUsedLastScenarioByCharacter = new Dictionary<string, int>(state.m_ItemsUsedLastScenarioByCharacter.Comparer);
			foreach (KeyValuePair<string, int> item4 in state.m_ItemsUsedLastScenarioByCharacter)
			{
				string key4 = item4.Key;
				int value4 = item4.Value;
				m_ItemsUsedLastScenarioByCharacter.Add(key4, value4);
			}
			references.Add(state.m_ItemsUsedLastScenarioByCharacter, m_ItemsUsedLastScenarioByCharacter);
		}
		m_PlayerStats = references.Get(state.m_PlayerStats);
		if (m_PlayerStats == null && state.m_PlayerStats != null)
		{
			m_PlayerStats = new Dictionary<string, CPlayerStatsScenario>(state.m_PlayerStats.Comparer);
			foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat in state.m_PlayerStats)
			{
				string key5 = playerStat.Key;
				CPlayerStatsScenario cPlayerStatsScenario = references.Get(playerStat.Value);
				if (cPlayerStatsScenario == null && playerStat.Value != null)
				{
					cPlayerStatsScenario = new CPlayerStatsScenario(playerStat.Value, references);
					references.Add(playerStat.Value, cPlayerStatsScenario);
				}
				m_PlayerStats.Add(key5, cPlayerStatsScenario);
			}
			references.Add(state.m_PlayerStats, m_PlayerStats);
		}
		m_HeroSummonStats = references.Get(state.m_HeroSummonStats);
		if (m_HeroSummonStats == null && state.m_HeroSummonStats != null)
		{
			m_HeroSummonStats = new CPlayerStatsScenario(state.m_HeroSummonStats, references);
			references.Add(state.m_HeroSummonStats, m_HeroSummonStats);
		}
		m_MonsterStats = references.Get(state.m_MonsterStats);
		if (m_MonsterStats == null && state.m_MonsterStats != null)
		{
			m_MonsterStats = new CPlayerStatsScenario(state.m_MonsterStats, references);
			references.Add(state.m_MonsterStats, m_MonsterStats);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ChestsLootedLastScenario", m_ChestsLootedLastScenario);
		info.AddValue("GoldPilesLootedLastScenario", m_GoldPilesLootedLastScenario);
		info.AddValue("TotalDamageDoneLastScenario", m_TotalDamageDoneLastScenario);
		info.AddValue("TotalDamageTakenLastScenario", m_TotalDamageTakenLastScenario);
		info.AddValue("TotalHealingDoneLastScenario", m_TotalHealingDoneLastScenario);
		info.AddValue("TotalXPEarnedLastScenario", m_TotalXPEarnedLastScenario);
		info.AddValue("ItemsUsedLastScenario", m_ItemsUsedLastScenario);
		info.AddValue("ScenarioGoldLastScenario", m_ScenarioGoldLastScenario);
		info.AddValue("ScenarioChestsLastScenario", m_ScenarioChestsLastScenario);
		info.AddValue("EnemiesKilledLastScenario", m_EnemiesKilledLastScenario);
		info.AddValue("ChestsLootedLastScenarioByCharacter", m_ChestsLootedLastScenarioByCharacter);
		info.AddValue("GoldPilesLootedLastScenarioByCharacter", m_GoldPilesLootedLastScenarioByCharacter);
		info.AddValue("TotalXPEarnedLastScenarioByCharacter", m_TotalXPEarnedLastScenarioByCharacter);
		info.AddValue("ItemsUsedLastScenarioByCharacter", m_ItemsUsedLastScenarioByCharacter);
		info.AddValue("PlayerStats", m_PlayerStats);
		info.AddValue("HeroSummonStats", m_HeroSummonStats);
		info.AddValue("MonsterStats", m_MonsterStats);
	}

	public StatsState(List<PlayerState> players)
	{
		m_PlayerStats = new Dictionary<string, CPlayerStatsScenario>();
		foreach (PlayerState player in players)
		{
			if (CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == player.ClassID) != null)
			{
				m_PlayerStats.Add(player.ClassID, new CPlayerStatsScenario(player.ClassID));
			}
			else
			{
				DLLDebug.LogError("Unable to find character withpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class ID " + player.ClassID);
			}
		}
		m_HeroSummonStats = new CPlayerStatsScenario(null);
		m_MonsterStats = new CPlayerStatsScenario(null);
		m_ChestsLootedLastScenarioByCharacter = new Dictionary<string, int>();
		m_GoldPilesLootedLastScenarioByCharacter = new Dictionary<string, int>();
		m_TotalXPEarnedLastScenarioByCharacter = new Dictionary<string, int>();
		m_ItemsUsedLastScenarioByCharacter = new Dictionary<string, int>();
	}

	public StatsState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ChestsLootedLastScenario":
					m_ChestsLootedLastScenario = info.GetInt32("ChestsLootedLastScenario");
					break;
				case "GoldPilesLootedLastScenario":
					m_GoldPilesLootedLastScenario = info.GetInt32("GoldPilesLootedLastScenario");
					break;
				case "TotalDamageDoneLastScenario":
					m_TotalDamageDoneLastScenario = info.GetInt32("TotalDamageDoneLastScenario");
					break;
				case "TotalDamageTakenLastScenario":
					m_TotalDamageTakenLastScenario = info.GetInt32("TotalDamageTakenLastScenario");
					break;
				case "TotalHealingDoneLastScenario":
					m_TotalHealingDoneLastScenario = info.GetInt32("TotalHealingDoneLastScenario");
					break;
				case "TotalXPEarnedLastScenario":
					m_TotalXPEarnedLastScenario = info.GetInt32("TotalXPEarnedLastScenario");
					break;
				case "ItemsUsedLastScenario":
					m_ItemsUsedLastScenario = info.GetInt32("ItemsUsedLastScenario");
					break;
				case "ScenarioGoldLastScenario":
					m_ScenarioGoldLastScenario = info.GetInt32("ScenarioGoldLastScenario");
					break;
				case "ScenarioChestsLastScenario":
					m_ScenarioChestsLastScenario = info.GetInt32("ScenarioChestsLastScenario");
					break;
				case "EnemiesKilledLastScenario":
					m_EnemiesKilledLastScenario = info.GetInt32("EnemiesKilledLastScenario");
					break;
				case "ChestsLootedLastScenarioByCharacter":
					m_ChestsLootedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("ChestsLootedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "GoldPilesLootedLastScenarioByCharacter":
					m_GoldPilesLootedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("GoldPilesLootedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "TotalXPEarnedLastScenarioByCharacter":
					m_TotalXPEarnedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("TotalXPEarnedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "ItemsUsedLastScenarioByCharacter":
					m_ItemsUsedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("ItemsUsedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "PlayerStats":
					m_PlayerStats = (Dictionary<string, CPlayerStatsScenario>)info.GetValue("PlayerStats", typeof(Dictionary<string, CPlayerStatsScenario>));
					break;
				case "HeroSummonStats":
					m_HeroSummonStats = (CPlayerStatsScenario)info.GetValue("HeroSummonStats", typeof(CPlayerStatsScenario));
					break;
				case "MonsterStats":
					m_MonsterStats = (CPlayerStatsScenario)info.GetValue("MonsterStats", typeof(CPlayerStatsScenario));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize StatsDataStorage entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}
}
