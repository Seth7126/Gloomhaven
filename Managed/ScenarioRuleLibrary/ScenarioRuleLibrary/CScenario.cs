using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class CScenario
{
	public bool SetEnemyHealthToMaxOnPlay;

	public bool EnemyMaxHealthBasedOnPartyLevel;

	public SEventActorFinishedScenario.EScenarioResult CurrentScenarioResult;

	private string m_ID;

	private string m_Description;

	private int m_Level;

	private List<CPlayerActor> m_InitialPlayers = new List<CPlayerActor>();

	private List<CEnemyActor> m_InitialEnemies = new List<CEnemyActor>();

	private List<CHeroSummonActor> m_InitialHeroSummons = new List<CHeroSummonActor>();

	private List<CEnemyActor> m_InitialAllyMonsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_InitialEnemy2Monsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_InitialNeutralMonsters = new List<CEnemyActor>();

	private List<CObjectActor> m_InitialObjects = new List<CObjectActor>();

	private List<CObjectActor> m_Objects = new List<CObjectActor>();

	private List<CPlayerActor> m_Players = new List<CPlayerActor>();

	private List<CEnemyActor> m_Enemies = new List<CEnemyActor>();

	private List<CHeroSummonActor> m_HeroSummons = new List<CHeroSummonActor>();

	private List<CEnemyActor> m_AllyMonsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_Enemy2Monsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_NeutralMonsters = new List<CEnemyActor>();

	private List<CMap> m_Maps = new List<CMap>();

	private CVectorInt3 m_PositiveSpaceOffset;

	public string ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			m_ID = value;
		}
	}

	public string Description => m_Description;

	public int Level => m_Level;

	public List<CPlayerActor> PlayerActors => m_Players;

	public List<CPlayerActor> ExhaustedPlayers { get; private set; }

	public List<CEnemyActor> Enemies => m_Enemies;

	public List<CEnemyActor> DeadEnemies { get; private set; }

	public List<CHeroSummonActor> HeroSummons => m_HeroSummons;

	public List<CHeroSummonActor> DeadHeroSummons { get; private set; }

	public List<CEnemyActor> AllyMonsters => m_AllyMonsters;

	public List<CEnemyActor> DeadAllyMonsters { get; private set; }

	public List<CEnemyActor> Enemy2Monsters => m_Enemy2Monsters;

	public List<CEnemyActor> DeadEnemy2Monsters { get; private set; }

	public List<CEnemyActor> NeutralMonsters => m_NeutralMonsters;

	public List<CEnemyActor> DeadNeutralMonsters { get; private set; }

	public List<CMap> Maps => m_Maps;

	public CVectorInt3 PositiveSpaceOffset => m_PositiveSpaceOffset;

	public List<CPlayerActor> InitialPlayers => m_InitialPlayers;

	public List<CEnemyActor> InitialEnemies => m_InitialEnemies;

	public List<CHeroSummonActor> InitialHeroSummons => m_InitialHeroSummons;

	public List<CEnemyActor> InitialAllyMonsters => m_InitialAllyMonsters;

	public List<CEnemyActor> InitialEnemy2Monsters => m_InitialEnemy2Monsters;

	public List<CEnemyActor> InitialNeutralMonsters => m_InitialNeutralMonsters;

	public List<CObjectActor> InitialObjects => m_InitialObjects;

	public List<CObjectActor> Objects => m_Objects;

	public List<CObjectActor> DeadObjects { get; private set; }

	public ScenarioLevelTableEntry SLTE { get; private set; }

	public List<CPlayerActor> AllPlayers => PlayerActors.Concat(ExhaustedPlayers).ToList();

	public int PartyGold => AllPlayers.Sum((CPlayerActor s) => s.Gold) + HeroSummons.Where((CHeroSummonActor it) => it.IsCompanionSummon).Sum((CHeroSummonActor it) => it.Gold);

	public List<CEnemyActor> AllEnemies => Enemies.Concat(DeadEnemies).ToList();

	public List<CHeroSummonActor> AllHeroSummons => HeroSummons.Concat(DeadHeroSummons).ToList();

	public List<CEnemyActor> AllAllyMonsters => AllyMonsters.Concat(DeadAllyMonsters).ToList();

	public List<CEnemyActor> AllEnemy2Monsters => Enemy2Monsters.Concat(DeadEnemy2Monsters).ToList();

	public List<CEnemyActor> AllNeutralMonsters => NeutralMonsters.Concat(DeadNeutralMonsters).ToList();

	public List<CEnemyActor> AllAliveMonsters => Enemies.Concat(AllyMonsters).Concat(Enemy2Monsters).Concat(NeutralMonsters)
		.ToList();

	public List<CEnemyActor> AllMonsters => AllEnemies.Concat(AllAllyMonsters).Concat(AllEnemy2Monsters).Concat(AllNeutralMonsters)
		.ToList();

	public List<CObjectActor> AllObjects => Objects.Concat(DeadObjects).ToList();

	public List<CEnemyActor> AllEnemyMonsters => Enemies.Concat(Enemy2Monsters).ToList();

	public List<CEnemyActor> AllEnemyMonstersAndObjects => Enemies.Concat(Enemy2Monsters).Concat(Objects).ToList();

	public List<CActor> AllActors
	{
		get
		{
			try
			{
				List<CActor> list = new List<CActor>();
				list.AddRange(AllPlayers);
				list.AddRange(AllEnemies);
				list.AddRange(AllHeroSummons);
				list.AddRange(AllAllyMonsters);
				list.AddRange(AllEnemy2Monsters);
				list.AddRange(AllNeutralMonsters);
				list.AddRange(AllObjects);
				return list;
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Error accessing all actors list.\n" + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
	}

	public List<CActor> AllAliveActors
	{
		get
		{
			List<CActor> list = new List<CActor>();
			list.AddRange(PlayerActors);
			list.AddRange(Enemies);
			list.AddRange(HeroSummons);
			list.AddRange(AllyMonsters);
			list.AddRange(Enemy2Monsters);
			list.AddRange(NeutralMonsters);
			list.AddRange(Objects);
			return list;
		}
	}

	public List<CActor> AllDeadActors
	{
		get
		{
			List<CActor> list = new List<CActor>();
			list.AddRange(ExhaustedPlayers);
			list.AddRange(DeadEnemies);
			list.AddRange(DeadHeroSummons);
			list.AddRange(DeadAllyMonsters);
			list.AddRange(DeadEnemy2Monsters);
			list.AddRange(DeadNeutralMonsters);
			list.AddRange(DeadObjects);
			return list;
		}
	}

	public Dictionary<string, int> GetAllEnemyCountByType
	{
		get
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (EnemyState item in m_Maps.SelectMany((CMap m) => m.Monsters).ToList())
			{
				if (!dictionary.ContainsKey(item.ClassID))
				{
					dictionary[item.ClassID] = 0;
				}
				dictionary[item.ClassID]++;
			}
			return dictionary;
		}
	}

	public CScenario(string id, string name, int level, CVectorInt3 positiveSpaceOffset, ScenarioLevelTableEntry slte)
	{
		m_ID = id;
		m_Description = name;
		m_Level = level;
		m_PositiveSpaceOffset = positiveSpaceOffset;
		SLTE = slte;
		CurrentScenarioResult = SEventActorFinishedScenario.EScenarioResult.None;
		ExhaustedPlayers = new List<CPlayerActor>();
		DeadEnemies = new List<CEnemyActor>();
		DeadHeroSummons = new List<CHeroSummonActor>();
		DeadAllyMonsters = new List<CEnemyActor>();
		DeadEnemy2Monsters = new List<CEnemyActor>();
		DeadNeutralMonsters = new List<CEnemyActor>();
		DeadObjects = new List<CObjectActor>();
	}

	public void AddMap(CMap map)
	{
		m_Maps.Add(map);
	}

	public CPlayerActor AddPlayer(PlayerState playerState, bool initial)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Find(playerState.ClassID);
		if (cCharacterClass != null)
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (Tuple<int, int> selectedAbilityCardIDAndInstanceID in playerState.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs)
			{
				list.Add(CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard x) => x.ID == selectedAbilityCardIDAndInstanceID.Item1 && x.CardInstanceID == selectedAbilityCardIDAndInstanceID.Item2));
			}
			cCharacterClass.SetHand(list);
			CPlayerActor cPlayerActor = new CPlayerActor(new Point(playerState.Location), cCharacterClass, playerState.Health, playerState.MaxHealth, playerState.Level, playerState.ActorGuid, playerState.ChosenModelIndex, playerState.CharacterName);
			if (playerState.IsDead)
			{
				ScenarioManager.Scenario.ExhaustedPlayers.Add(cPlayerActor);
				cPlayerActor.CauseOfDeath = playerState.CauseOfDeath;
			}
			else if (initial)
			{
				m_InitialPlayers.Add(cPlayerActor);
			}
			else
			{
				m_Players.Add(cPlayerActor);
			}
			if (playerState.ActorGuid == null)
			{
				playerState.ActorGuid = cPlayerActor.ActorGuid;
				playerState.IsRevealed = true;
				playerState.Save(initial);
			}
			if (!initial)
			{
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				CActiveBonus.RefreshAllSongActiveBonuses(cPlayerActor);
				CActiveBonus.RefreshAllAuraActiveBonuses();
				CActiveBonus.RefreshOverhealActiveBonuses();
			}
			return cPlayerActor;
		}
		DLLDebug.LogError("Cannot find Hero: " + playerState.ClassID);
		return null;
	}

	public CEnemyActor AddEnemy(EnemyState enemyState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(enemyState.ClassID);
		if (cMonsterClass == null)
		{
			DLLDebug.LogError("Cannot find Monster: " + enemyState.ClassID);
			return null;
		}
		if (!enemyState.IsRevealed && resetHealth)
		{
			if (enemyState.ActorGuid == null)
			{
				if (enemyState.Health == 0)
				{
					enemyState.Health = cMonsterClass.Health();
				}
				if (enemyState.MaxHealth == 0)
				{
					enemyState.MaxHealth = cMonsterClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					enemyState.MaxHealth = cMonsterClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					enemyState.Health = enemyState.MaxHealth;
				}
				enemyState.Health = Math.Min(enemyState.Health, enemyState.MaxHealth);
			}
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || enemyState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = enemyState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(enemyState.Location), cMonsterClass, CActor.EType.Enemy, enemyState.IsSummon, enemyState.SummonerGuid, enemyState.ActorGuid, enemyState.Health, enemyState.MaxHealth, enemyState.Level, num, initial, enemyState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Enemy Monster: " + enemyState.ClassID);
			return null;
		}
		if (enemyState.IsDead)
		{
			ScenarioManager.Scenario.DeadEnemies.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialEnemies.Add(cEnemyActor);
		}
		else
		{
			m_Enemies.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				SimpleLog.AddToSimpleLog(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		enemyState.IsRevealed = true;
		if (enemyState.ActorGuid == null)
		{
			enemyState.ActorGuid = cEnemyActor.ActorGuid;
			enemyState.Save(initial);
		}
		if (!initial)
		{
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddAllyMonster(EnemyState allyMonsterState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(allyMonsterState.ClassID);
		if (cMonsterClass == null)
		{
			DLLDebug.LogError("Cannot find Monster: " + allyMonsterState.ClassID);
			return null;
		}
		if (!allyMonsterState.IsRevealed && resetHealth)
		{
			if (allyMonsterState.ActorGuid == null)
			{
				if (allyMonsterState.Health == 0)
				{
					allyMonsterState.Health = cMonsterClass.Health();
				}
				if (allyMonsterState.MaxHealth == 0)
				{
					allyMonsterState.MaxHealth = cMonsterClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					allyMonsterState.MaxHealth = cMonsterClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					allyMonsterState.Health = allyMonsterState.MaxHealth;
				}
				allyMonsterState.Health = Math.Min(allyMonsterState.Health, allyMonsterState.MaxHealth);
			}
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || allyMonsterState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = allyMonsterState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(allyMonsterState.Location), cMonsterClass, CActor.EType.Ally, allyMonsterState.IsSummon, allyMonsterState.SummonerGuid, allyMonsterState.ActorGuid, allyMonsterState.Health, allyMonsterState.MaxHealth, allyMonsterState.Level, num, initial, allyMonsterState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Ally Monster: " + allyMonsterState.ClassID);
			return null;
		}
		if (allyMonsterState.IsDead)
		{
			ScenarioManager.Scenario.DeadAllyMonsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialAllyMonsters.Add(cEnemyActor);
		}
		else
		{
			m_AllyMonsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				SimpleLog.AddToSimpleLog(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		allyMonsterState.IsRevealed = true;
		if (allyMonsterState.ActorGuid == null)
		{
			allyMonsterState.ActorGuid = cEnemyActor.ActorGuid;
			allyMonsterState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddEnemy2Monster(EnemyState enemy2MonsterState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(enemy2MonsterState.ClassID);
		if (cMonsterClass == null)
		{
			DLLDebug.LogError("Cannot find Monster: " + enemy2MonsterState.ClassID);
			return null;
		}
		if (!enemy2MonsterState.IsRevealed && resetHealth)
		{
			if (enemy2MonsterState.ActorGuid == null)
			{
				if (enemy2MonsterState.Health == 0)
				{
					enemy2MonsterState.Health = cMonsterClass.Health();
				}
				if (enemy2MonsterState.MaxHealth == 0)
				{
					enemy2MonsterState.MaxHealth = cMonsterClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					enemy2MonsterState.MaxHealth = cMonsterClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					enemy2MonsterState.Health = enemy2MonsterState.MaxHealth;
				}
				enemy2MonsterState.Health = Math.Min(enemy2MonsterState.Health, enemy2MonsterState.MaxHealth);
			}
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || enemy2MonsterState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = enemy2MonsterState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(enemy2MonsterState.Location), cMonsterClass, CActor.EType.Enemy2, enemy2MonsterState.IsSummon, enemy2MonsterState.SummonerGuid, enemy2MonsterState.ActorGuid, enemy2MonsterState.Health, enemy2MonsterState.MaxHealth, enemy2MonsterState.Level, num, initial, enemy2MonsterState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Enemy 2 Monster: " + enemy2MonsterState.ClassID);
			return null;
		}
		if (enemy2MonsterState.IsDead)
		{
			ScenarioManager.Scenario.DeadEnemy2Monsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialEnemy2Monsters.Add(cEnemyActor);
		}
		else
		{
			m_Enemy2Monsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				SimpleLog.AddToSimpleLog(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		enemy2MonsterState.IsRevealed = true;
		if (enemy2MonsterState.ActorGuid == null)
		{
			enemy2MonsterState.ActorGuid = cEnemyActor.ActorGuid;
			enemy2MonsterState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddNeutralMonster(EnemyState neutralMonsterState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(neutralMonsterState.ClassID);
		if (cMonsterClass == null)
		{
			DLLDebug.LogError("Cannot find Monster: " + neutralMonsterState.ClassID);
			return null;
		}
		if (!neutralMonsterState.IsRevealed && resetHealth)
		{
			if (neutralMonsterState.ActorGuid == null)
			{
				if (neutralMonsterState.Health == 0)
				{
					neutralMonsterState.Health = cMonsterClass.Health();
				}
				if (neutralMonsterState.MaxHealth == 0)
				{
					neutralMonsterState.MaxHealth = cMonsterClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					neutralMonsterState.MaxHealth = cMonsterClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					neutralMonsterState.Health = neutralMonsterState.MaxHealth;
				}
				neutralMonsterState.Health = Math.Min(neutralMonsterState.Health, neutralMonsterState.MaxHealth);
			}
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || neutralMonsterState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = neutralMonsterState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(neutralMonsterState.Location), cMonsterClass, CActor.EType.Neutral, neutralMonsterState.IsSummon, neutralMonsterState.SummonerGuid, neutralMonsterState.ActorGuid, neutralMonsterState.Health, neutralMonsterState.MaxHealth, neutralMonsterState.Level, num, initial, neutralMonsterState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Neutral Monster: " + neutralMonsterState.ClassID);
			return null;
		}
		if (neutralMonsterState.IsDead)
		{
			ScenarioManager.Scenario.DeadNeutralMonsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialNeutralMonsters.Add(cEnemyActor);
		}
		else
		{
			m_NeutralMonsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				SimpleLog.AddToSimpleLog(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		neutralMonsterState.IsRevealed = true;
		if (neutralMonsterState.ActorGuid == null)
		{
			neutralMonsterState.ActorGuid = cEnemyActor.ActorGuid;
			neutralMonsterState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CHeroSummonActor AddHeroSummon(HeroSummonState heroSummonState, bool initial, bool noIDRegen = false)
	{
		if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == heroSummonState.ClassID) == null)
		{
			DLLDebug.LogError("Cannot find YML for Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		CHeroSummonClass cHeroSummonClass = CharacterClassManager.FindHeroSummonClass(heroSummonState.ClassID);
		if (cHeroSummonClass == null)
		{
			DLLDebug.LogError("Cannot find Class for Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		CHeroSummonActor cHeroSummonActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || heroSummonState.ID == 0)
			{
				num = cHeroSummonClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of hero summon of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = heroSummonState.ID;
			}
			cHeroSummonActor = new CHeroSummonActor(new Point(heroSummonState.Location), heroSummonState.Summoner, heroSummonState.Level, num, cHeroSummonClass, heroSummonState.HeroSummonData, heroSummonState.ActorGuid, heroSummonState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		if (heroSummonState.IsDead)
		{
			DeadHeroSummons.Add(cHeroSummonActor);
		}
		else if (initial)
		{
			m_InitialHeroSummons.Add(cHeroSummonActor);
		}
		else
		{
			m_HeroSummons.Add(cHeroSummonActor);
		}
		if (heroSummonState.ActorGuid == null)
		{
			heroSummonState.ActorGuid = cHeroSummonActor.ActorGuid;
			heroSummonState.IsRevealed = true;
			heroSummonState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cHeroSummonActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cHeroSummonActor;
	}

	public CObjectActor AddObject(ObjectState objectState, bool initial, bool noIDRegen = false, bool dummyObjectForProp = false)
	{
		CObjectClass cObjectClass = MonsterClassManager.FindObjectClass(objectState.ClassID);
		if (cObjectClass == null)
		{
			DLLDebug.LogError("Cannot find Monster: " + objectState.ClassID);
			return null;
		}
		if (!objectState.IsRevealed)
		{
			if (objectState.ActorGuid == null || dummyObjectForProp)
			{
				if (objectState.Health == 0)
				{
					objectState.Health = cObjectClass.Health();
				}
				if (objectState.MaxHealth == 0)
				{
					objectState.MaxHealth = cObjectClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					objectState.MaxHealth = cObjectClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					objectState.Health = objectState.MaxHealth;
				}
				objectState.Health = Math.Min(objectState.Health, objectState.MaxHealth);
			}
		}
		CObjectActor cObjectActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || objectState.ID == 0)
			{
				num = cObjectClass.GetNextID();
				if (num == -1)
				{
					DLLDebug.Log("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = objectState.ID;
			}
			cObjectActor = new CObjectActor(new Point(objectState.Location), cObjectClass, objectState.Type, objectState.ActorGuid, objectState.Health, objectState.MaxHealth, objectState.Level, num, initial, objectState.ChosenModelIndex);
		}
		catch
		{
			DLLDebug.LogError("Could not create Enemy Monster: " + objectState.ClassID);
			return null;
		}
		if (objectState.IsDead)
		{
			ScenarioManager.Scenario.DeadObjects.Add(cObjectActor);
		}
		else if (initial)
		{
			m_InitialObjects.Add(cObjectActor);
		}
		else
		{
			m_Objects.Add(cObjectActor);
		}
		if (!initial)
		{
			cObjectClass.SelectRoundAbilityCard();
		}
		objectState.IsRevealed = true;
		if (objectState.ActorGuid == null)
		{
			objectState.ActorGuid = cObjectActor.ActorGuid;
			objectState.Save(initial);
		}
		if (objectState.IsAttachedToProp)
		{
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == objectState.PropGuidAttachedTo);
			if (cObjectProp != null)
			{
				cObjectActor.SetAttachedToProp(cObjectProp);
				cObjectProp.SetActorAttachedAtRuntime(cObjectActor);
			}
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
		return cObjectActor;
	}

	public bool AnyMapsHiddenAndStillHaveEnemies()
	{
		foreach (CMap map in m_Maps)
		{
			if (!map.Revealed && map.Monsters.Count() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<CActor> FindAllActorsWithActiveBonusClass(CClass classToFind)
	{
		List<CPlayerActor> list = m_Players.FindAll((CPlayerActor x) => x.ActiveBonusClass() == classToFind);
		List<CHeroSummonActor> list2 = m_HeroSummons.FindAll((CHeroSummonActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list3 = m_Enemies.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list4 = m_AllyMonsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list5 = m_Enemy2Monsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list6 = m_NeutralMonsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CObjectActor> list7 = m_Objects.FindAll((CObjectActor x) => x.ActiveBonusClass() == classToFind);
		List<CActor> list8 = new List<CActor>();
		foreach (CPlayerActor item in list)
		{
			list8.Add(item);
		}
		foreach (CHeroSummonActor item2 in list2)
		{
			list8.Add(item2);
		}
		foreach (CEnemyActor item3 in list3)
		{
			list8.Add(item3);
		}
		foreach (CEnemyActor item4 in list4)
		{
			list8.Add(item4);
		}
		foreach (CEnemyActor item5 in list5)
		{
			list8.Add(item5);
		}
		foreach (CEnemyActor item6 in list6)
		{
			list8.Add(item6);
		}
		foreach (CObjectActor item7 in list7)
		{
			list8.Add(item7);
		}
		return list8;
	}

	public CActor FindActorAt(Point position, CActor actorToIgnore = null, bool includeInitial = false, bool isRetry = false)
	{
		try
		{
			CActor cActor = m_Players.Find((CPlayerActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			if (cActor == null)
			{
				cActor = m_Enemies.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (cActor == null)
			{
				cActor = m_HeroSummons.Find((CHeroSummonActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (cActor == null)
			{
				cActor = m_AllyMonsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (cActor == null)
			{
				cActor = m_Enemy2Monsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (cActor == null)
			{
				cActor = m_NeutralMonsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (cActor == null)
			{
				cActor = m_Objects.Find((CObjectActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
			}
			if (includeInitial)
			{
				if (cActor == null)
				{
					cActor = m_InitialPlayers.Find((CPlayerActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialEnemies.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialHeroSummons.Find((CHeroSummonActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialAllyMonsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialEnemy2Monsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialNeutralMonsters.Find((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
				if (cActor == null)
				{
					cActor = m_InitialObjects.Find((CObjectActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore);
				}
			}
			return cActor;
		}
		catch (Exception ex)
		{
			if (!isRetry)
			{
				DLLDebug.Log("Caught exception at CScenario.FindActorAt - Sleeping Thread" + Environment.StackTrace);
				Thread.Sleep(50);
				return FindActorAt(position, actorToIgnore, includeInitial, isRetry: true);
			}
			string text = "Error occurred trying to FindActorAt position: " + position.ToString() + ".\n";
			if (ex is InvalidOperationException)
			{
				text += "Invalid Operation Exception, a threading issue.\n";
			}
			if (m_Players == null)
			{
				text += "m_Players list is null.\n";
			}
			if (m_Players.Any((CPlayerActor x) => x == null))
			{
				text += "m_Players contains a null actor.\n";
			}
			if (m_Players.Any(delegate(CPlayerActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Players contains an actor with a null array index.\n";
			}
			if (m_Enemies == null)
			{
				text += "m_Enemies list is null.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemies contains a null actor.\n";
			}
			if (m_Enemies.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemies contains an actor with a null array index.\n";
			}
			if (m_HeroSummons == null)
			{
				text += "m_HeroSummons list is null.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) => x == null))
			{
				text += "m_HeroSummons contains a null actor.\n";
			}
			if (m_HeroSummons.Any(delegate(CHeroSummonActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_HeroSummons contains an actor with a null array index.\n";
			}
			if (m_AllyMonsters == null)
			{
				text += "m_AllyMonsters list is null.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_AllyMonsters contains a null actor.\n";
			}
			if (m_AllyMonsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_AllyMonsters contains an actor with a null array index.\n";
			}
			if (m_Enemy2Monsters == null)
			{
				text += "m_Enemy2Monsters list is null.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemy2Monsters contains a null actor.\n";
			}
			if (m_Enemy2Monsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemy2Monsters contains an actor with a null array index.\n";
			}
			if (m_NeutralMonsters == null)
			{
				text += "m_NeutralMonsters list is null.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_NeutralMonsters contains a null actor.\n";
			}
			if (m_NeutralMonsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_NeutralMonsters contains an actor with a null array index.\n";
			}
			if (m_Objects == null)
			{
				text += "m_Objects list is null.\n";
			}
			if (m_Objects.Any((CObjectActor x) => x == null))
			{
				text += "m_Objects contains a null actor.\n";
			}
			if (m_Objects.Any(delegate(CObjectActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Objects contains an actor with a null array index.\n";
			}
			if (includeInitial)
			{
				if (m_InitialPlayers == null)
				{
					text += "m_InitialPlayers list is null.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) => x == null))
				{
					text += "m_InitialPlayers contains a null actor.\n";
				}
				if (m_InitialPlayers.Any(delegate(CPlayerActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialPlayers contains an actor with a null array index.\n";
				}
				if (m_InitialEnemies == null)
				{
					text += "m_InitialEnemies list is null.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemies contains a null actor.\n";
				}
				if (m_InitialEnemies.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemies contains an actor with a null array index.\n";
				}
				if (m_InitialHeroSummons == null)
				{
					text += "m_InitialHeroSummons list is null.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) => x == null))
				{
					text += "m_InitialHeroSummons contains a null actor.\n";
				}
				if (m_InitialHeroSummons.Any(delegate(CHeroSummonActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialHeroSummons contains an actor with a null array index.\n";
				}
				if (m_InitialAllyMonsters == null)
				{
					text += "m_InitialAllyMonsters list is null.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialAllyMonsters contains a null actor.\n";
				}
				if (m_InitialAllyMonsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialAllyMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialEnemy2Monsters == null)
				{
					text += "m_InitialEnemy2Monsters list is null.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemy2Monsters contains a null actor.\n";
				}
				if (m_InitialEnemy2Monsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemy2Monsters contains an actor with a null array index.\n";
				}
				if (m_InitialNeutralMonsters == null)
				{
					text += "m_InitialNeutralMonsters list is null.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialNeutralMonsters contains a null actor.\n";
				}
				if (m_InitialNeutralMonsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialNeutralMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialObjects == null)
				{
					text += "m_InitialObjects list is null.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) => x == null))
				{
					text += "m_InitialObjects contains a null actor.\n";
				}
				if (m_InitialObjects.Any(delegate(CObjectActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialObjects contains an actor with a null array index.\n";
				}
			}
			DLLDebug.LogError(text + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	public List<CActor> FindActorsAt(Point position, CActor actorToIgnore = null, bool includeInitial = false, bool isRetry = false)
	{
		try
		{
			List<CActor> list = new List<CActor>();
			list.AddRange(m_Players.FindAll((CPlayerActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_Enemies.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_HeroSummons.FindAll((CHeroSummonActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_AllyMonsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_Enemy2Monsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_NeutralMonsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			list.AddRange(m_Objects.FindAll((CObjectActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			if (includeInitial)
			{
				list.AddRange(m_InitialPlayers.FindAll((CPlayerActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialEnemies.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialHeroSummons.FindAll((CHeroSummonActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialAllyMonsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialEnemy2Monsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialNeutralMonsters.FindAll((CEnemyActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
				list.AddRange(m_InitialObjects.FindAll((CObjectActor x) => x.ArrayIndex == position && !x.PhasedOut && x != actorToIgnore));
			}
			return list;
		}
		catch (Exception ex)
		{
			if (!isRetry)
			{
				DLLDebug.Log("Caught exception at CScenario.FindActorsAt - Sleeping Thread" + Environment.StackTrace);
				Thread.Sleep(50);
				return FindActorsAt(position, actorToIgnore, includeInitial, isRetry: true);
			}
			string text = "Error occurred trying to FindActorsAt position: " + position.ToString() + ".\n";
			if (ex is InvalidOperationException)
			{
				text += "Invalid Operation Exception, a threading issue.\n";
			}
			if (m_Players == null)
			{
				text += "m_Players list is null.\n";
			}
			if (m_Players.Any((CPlayerActor x) => x == null))
			{
				text += "m_Players contains a null actor.\n";
			}
			if (m_Players.Any(delegate(CPlayerActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Players contains an actor with a null array index.\n";
			}
			if (m_Enemies == null)
			{
				text += "m_Enemies list is null.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemies contains a null actor.\n";
			}
			if (m_Enemies.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemies contains an actor with a null array index.\n";
			}
			if (m_HeroSummons == null)
			{
				text += "m_HeroSummons list is null.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) => x == null))
			{
				text += "m_HeroSummons contains a null actor.\n";
			}
			if (m_HeroSummons.Any(delegate(CHeroSummonActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_HeroSummons contains an actor with a null array index.\n";
			}
			if (m_AllyMonsters == null)
			{
				text += "m_AllyMonsters list is null.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_AllyMonsters contains a null actor.\n";
			}
			if (m_AllyMonsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_AllyMonsters contains an actor with a null array index.\n";
			}
			if (m_Enemy2Monsters == null)
			{
				text += "m_Enemy2Monsters list is null.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemy2Monsters contains a null actor.\n";
			}
			if (m_Enemy2Monsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemy2Monsters contains an actor with a null array index.\n";
			}
			if (m_NeutralMonsters == null)
			{
				text += "m_NeutralMonsters list is null.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_NeutralMonsters contains a null actor.\n";
			}
			if (m_NeutralMonsters.Any(delegate(CEnemyActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_NeutralMonsters contains an actor with a null array index.\n";
			}
			if (m_Objects == null)
			{
				text += "m_Objects list is null.\n";
			}
			if (m_Objects.Any((CObjectActor x) => x == null))
			{
				text += "m_Objects contains a null actor.\n";
			}
			if (m_Objects.Any(delegate(CObjectActor x)
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Objects contains an actor with a null array index.\n";
			}
			if (includeInitial)
			{
				if (m_InitialPlayers == null)
				{
					text += "m_InitialPlayers list is null.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) => x == null))
				{
					text += "m_InitialPlayers contains a null actor.\n";
				}
				if (m_InitialPlayers.Any(delegate(CPlayerActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialPlayers contains an actor with a null array index.\n";
				}
				if (m_InitialEnemies == null)
				{
					text += "m_InitialEnemies list is null.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemies contains a null actor.\n";
				}
				if (m_InitialEnemies.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemies contains an actor with a null array index.\n";
				}
				if (m_InitialHeroSummons == null)
				{
					text += "m_InitialHeroSummons list is null.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) => x == null))
				{
					text += "m_InitialHeroSummons contains a null actor.\n";
				}
				if (m_InitialHeroSummons.Any(delegate(CHeroSummonActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialHeroSummons contains an actor with a null array index.\n";
				}
				if (m_InitialAllyMonsters == null)
				{
					text += "m_InitialAllyMonsters list is null.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialAllyMonsters contains a null actor.\n";
				}
				if (m_InitialAllyMonsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialAllyMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialEnemy2Monsters == null)
				{
					text += "m_InitialEnemy2Monsters list is null.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemy2Monsters contains a null actor.\n";
				}
				if (m_InitialEnemy2Monsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemy2Monsters contains an actor with a null array index.\n";
				}
				if (m_InitialNeutralMonsters == null)
				{
					text += "m_InitialNeutralMonsters list is null.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialNeutralMonsters contains a null actor.\n";
				}
				if (m_InitialNeutralMonsters.Any(delegate(CEnemyActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialNeutralMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialObjects == null)
				{
					text += "m_InitialObjects list is null.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) => x == null))
				{
					text += "m_InitialObjects contains a null actor.\n";
				}
				if (m_InitialObjects.Any(delegate(CObjectActor x)
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialObjects contains an actor with a null array index.\n";
				}
			}
			DLLDebug.LogError(text + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	public CPlayerActor FindPlayerAt(Point position)
	{
		return m_Players.Find((CPlayerActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindEnemyAt(Point position)
	{
		return m_Enemies.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CHeroSummonActor FindHeroSummonAt(Point position)
	{
		return m_HeroSummons.Find((CHeroSummonActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindAllyMonsterAt(Point position)
	{
		return m_AllyMonsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindEnemy2MonsterAt(Point position)
	{
		return m_Enemy2Monsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindNeutralMonsterAt(Point position)
	{
		return m_NeutralMonsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CObjectActor FindObjectActorAt(Point position)
	{
		return m_Objects.Find((CObjectActor x) => x.ArrayIndex == position);
	}

	public void RemoveMonster(CEnemyActor monster)
	{
		if (m_Enemies.Contains(monster))
		{
			RemoveEnemy(monster);
		}
		else if (m_AllyMonsters.Contains(monster))
		{
			RemoveAllyMonsters(monster);
		}
		else if (m_Enemy2Monsters.Contains(monster))
		{
			RemoveEnemy2Monster(monster);
		}
		else if (m_NeutralMonsters.Contains(monster))
		{
			RemoveNeutralMonster(monster);
		}
		else if (m_Objects.Contains(monster))
		{
			RemoveObjectActor((CObjectActor)monster);
		}
	}

	public void RemoveEnemy(CEnemyActor enemyActor)
	{
		m_Enemies.Remove(enemyActor);
		if (!DeadEnemies.Exists((CEnemyActor x) => x.ActorGuid == enemyActor.ActorGuid))
		{
			DeadEnemies.Add(enemyActor);
		}
	}

	public void RemovePlayer(CPlayerActor playerActor)
	{
		m_Players.Remove(playerActor);
		if (!ExhaustedPlayers.Exists((CPlayerActor x) => x.ActorGuid == playerActor.ActorGuid))
		{
			ExhaustedPlayers.Add(playerActor);
		}
	}

	public void RemoveHeroSummon(CHeroSummonActor heroSummonActor)
	{
		m_HeroSummons.Remove(heroSummonActor);
		if (!DeadHeroSummons.Exists((CHeroSummonActor x) => x.ActorGuid == heroSummonActor.ActorGuid))
		{
			DeadHeroSummons.Add(heroSummonActor);
		}
	}

	public void RemoveAllyMonsters(CEnemyActor allyMonster)
	{
		m_AllyMonsters.Remove(allyMonster);
		if (!DeadAllyMonsters.Exists((CEnemyActor x) => x.ActorGuid == allyMonster.ActorGuid))
		{
			DeadAllyMonsters.Add(allyMonster);
		}
	}

	public void RemoveEnemy2Monster(CEnemyActor enemy2monster)
	{
		m_Enemy2Monsters.Remove(enemy2monster);
		if (!DeadEnemy2Monsters.Exists((CEnemyActor x) => x.ActorGuid == enemy2monster.ActorGuid))
		{
			DeadEnemy2Monsters.Add(enemy2monster);
		}
	}

	public void RemoveNeutralMonster(CEnemyActor neutralMonster)
	{
		m_NeutralMonsters.Remove(neutralMonster);
		if (!DeadNeutralMonsters.Exists((CEnemyActor x) => x.ActorGuid == neutralMonster.ActorGuid))
		{
			DeadNeutralMonsters.Add(neutralMonster);
		}
	}

	public void RemoveObjectActor(CObjectActor objectActor)
	{
		m_Objects.Remove(objectActor);
		if (!DeadObjects.Exists((CObjectActor x) => x.ActorGuid == objectActor.ActorGuid))
		{
			DeadObjects.Add(objectActor);
		}
	}

	public bool HasActor(CActor actor)
	{
		if (actor.Class is CCharacterClass)
		{
			return m_Players.Contains((CPlayerActor)actor);
		}
		if (actor.Class is CMonsterClass)
		{
			if (actor.Class is CObjectClass)
			{
				return m_Objects.Contains((CObjectActor)actor);
			}
			return AllAliveMonsters.Contains((CEnemyActor)actor);
		}
		return m_HeroSummons.Contains((CHeroSummonActor)actor);
	}

	public CActor FindActor(string actorGuid)
	{
		object obj = m_Players.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid) ?? ExhaustedPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid);
		if (obj == null)
		{
			obj = m_InitialPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialHeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Enemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_AllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadAllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Enemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_NeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Objects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		return (CActor)obj;
	}
}
