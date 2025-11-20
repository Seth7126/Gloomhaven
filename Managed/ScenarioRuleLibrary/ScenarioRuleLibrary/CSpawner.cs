using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CSpawner : ISerializable
{
	public string SpawnerGuid { get; private set; }

	public SpawnerData SpawnerData { get; private set; }

	public TileIndex ArrayIndex { get; private set; }

	public bool IsActive { get; private set; }

	public int SpawnRoundCounter { get; private set; }

	public int CurrentSpawnRoundEntryIndex { get; private set; }

	public string StartingMapGuid { get; private set; }

	public CMap StartingMap => ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.MapGuid == StartingMapGuid);

	public CObjectProp Prop { get; protected set; }

	public CActor.EType ActorTypeToSpawn { get; protected set; }

	public SpawnerData.ESpawnerEntryDifficulty SpawnerEntryDifficulty { get; private set; }

	public CAIFocusOverrideDetails AIFocusOverride { get; private set; }

	public Dictionary<int, ScenarioManager.EPerPartySizeConfig> ConfigPerPartySize { get; private set; }

	public bool HasSpawned { get; private set; }

	public List<SpawnRoundEntry> SpawnRoundEntries
	{
		get
		{
			if (SpawnerData.SpawnRoundEntryDictionary.TryGetValue(SpawnerEntryDifficulty.ToString(), out var value))
			{
				return value;
			}
			return SpawnerData.SpawnRoundEntryDictionary["Default"];
		}
	}

	public List<TileIndex> PathingBlockers
	{
		get
		{
			if (Prop != null && Prop is CObjectObstacle cObjectObstacle)
			{
				return cObjectObstacle.PathingBlockers;
			}
			return new List<TileIndex>();
		}
	}

	public CSpawner()
	{
	}

	public CSpawner(CSpawner state, ReferenceDictionary references)
	{
		SpawnerGuid = state.SpawnerGuid;
		SpawnerData = references.Get(state.SpawnerData);
		if (SpawnerData == null && state.SpawnerData != null)
		{
			SpawnerData = new SpawnerData(state.SpawnerData, references);
			references.Add(state.SpawnerData, SpawnerData);
		}
		ArrayIndex = references.Get(state.ArrayIndex);
		if (ArrayIndex == null && state.ArrayIndex != null)
		{
			ArrayIndex = new TileIndex(state.ArrayIndex, references);
			references.Add(state.ArrayIndex, ArrayIndex);
		}
		IsActive = state.IsActive;
		SpawnRoundCounter = state.SpawnRoundCounter;
		CurrentSpawnRoundEntryIndex = state.CurrentSpawnRoundEntryIndex;
		StartingMapGuid = state.StartingMapGuid;
		Prop = references.Get(state.Prop);
		if (Prop == null && state.Prop != null)
		{
			Prop = new CObjectProp(state.Prop, references);
			references.Add(state.Prop, Prop);
		}
		ActorTypeToSpawn = state.ActorTypeToSpawn;
		SpawnerEntryDifficulty = state.SpawnerEntryDifficulty;
		AIFocusOverride = references.Get(state.AIFocusOverride);
		if (AIFocusOverride == null && state.AIFocusOverride != null)
		{
			AIFocusOverride = new CAIFocusOverrideDetails(state.AIFocusOverride, references);
			references.Add(state.AIFocusOverride, AIFocusOverride);
		}
		ConfigPerPartySize = references.Get(state.ConfigPerPartySize);
		if (ConfigPerPartySize == null && state.ConfigPerPartySize != null)
		{
			ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>(state.ConfigPerPartySize.Comparer);
			foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in state.ConfigPerPartySize)
			{
				int key = item.Key;
				ScenarioManager.EPerPartySizeConfig value = item.Value;
				ConfigPerPartySize.Add(key, value);
			}
			references.Add(state.ConfigPerPartySize, ConfigPerPartySize);
		}
		HasSpawned = state.HasSpawned;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpawnerGuid", SpawnerGuid);
		info.AddValue("SpawnerData", SpawnerData);
		info.AddValue("ArrayIndex", ArrayIndex);
		info.AddValue("IsActive", IsActive);
		info.AddValue("SpawnRoundCounter", SpawnRoundCounter);
		info.AddValue("CurrentSpawnRoundEntryIndex", CurrentSpawnRoundEntryIndex);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("Prop", Prop);
		info.AddValue("AIFocusOverride", AIFocusOverride);
		info.AddValue("ConfigPerPartySize", ConfigPerPartySize);
		info.AddValue("ActorTypeToSpawn", ActorTypeToSpawn);
		info.AddValue("HasSpawned", HasSpawned);
	}

	public CSpawner(SerializationInfo info, StreamingContext context)
	{
		bool flag = false;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "SpawnerGuid":
					SpawnerGuid = info.GetString("SpawnerGuid");
					break;
				case "SpawnerData":
					SpawnerData = (SpawnerData)info.GetValue("SpawnerData", typeof(SpawnerData));
					break;
				case "ArrayIndex":
					ArrayIndex = (TileIndex)info.GetValue("ArrayIndex", typeof(TileIndex));
					break;
				case "IsActive":
					IsActive = info.GetBoolean("IsActive");
					break;
				case "SpawnRoundCounter":
					SpawnRoundCounter = info.GetInt32("SpawnRoundCounter");
					break;
				case "CurrentSpawnRoundEntryIndex":
					CurrentSpawnRoundEntryIndex = info.GetInt32("CurrentSpawnRoundEntryIndex");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "Prop":
					Prop = (CObjectProp)info.GetValue("Prop", typeof(CObjectProp));
					break;
				case "AIFocusOverride":
					AIFocusOverride = (CAIFocusOverrideDetails)info.GetValue("AIFocusOverride", typeof(CAIFocusOverrideDetails));
					break;
				case "ConfigPerPartySize":
					ConfigPerPartySize = (Dictionary<int, ScenarioManager.EPerPartySizeConfig>)info.GetValue("ConfigPerPartySize", typeof(Dictionary<int, ScenarioManager.EPerPartySizeConfig>));
					break;
				case "ActorTypeToSpawn":
					flag = true;
					ActorTypeToSpawn = (CActor.EType)info.GetValue("ActorTypeToSpawn", typeof(CActor.EType));
					break;
				case "HasSpawned":
					HasSpawned = info.GetBoolean("HasSpawned");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CSpawner entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (!flag)
		{
			ActorTypeToSpawn = CActor.EType.Enemy;
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (ConfigPerPartySize == null)
		{
			ConfigPerPartySize = GetDefaultConfigPerPartySize();
		}
	}

	public CSpawner(SpawnerData spawnerData, TileIndex arrayIndex, string startingMapGuid, string spawnerGuid = null, SpawnerData.ESpawnerEntryDifficulty spawnerEntryDifficulty = SpawnerData.ESpawnerEntryDifficulty.Default, CActor.EType typeToSpawn = CActor.EType.Enemy)
	{
		SpawnerGuid = spawnerGuid ?? Guid.NewGuid().ToString();
		StartingMapGuid = startingMapGuid;
		SpawnerData = spawnerData;
		ArrayIndex = arrayIndex;
		SpawnRoundCounter = 0;
		CurrentSpawnRoundEntryIndex = 0;
		IsActive = SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ScenarioStart;
		SpawnerEntryDifficulty = spawnerEntryDifficulty;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
		ActorTypeToSpawn = typeToSpawn;
		HasSpawned = false;
	}

	public void SetLocation(TileIndex arrayIndex)
	{
		ArrayIndex = arrayIndex;
	}

	public void SetAIFocusOverride(CAIFocusOverrideDetails aiFocusOverride)
	{
		AIFocusOverride = aiFocusOverride;
	}

	public void SetSpawnType(CActor.EType actorTypeToSpawn)
	{
		ActorTypeToSpawn = actorTypeToSpawn;
	}

	public virtual void SetActive(bool active, bool forceDontSpawn = false)
	{
		bool isActive = IsActive;
		IsActive = active;
		if (!IsActive && Prop != null)
		{
			HideSpawnerProp();
		}
		else if (IsActive && !isActive && !forceDontSpawn && SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ActivatedExternally)
		{
			TrySpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count);
		}
	}

	public virtual void CreateSpawnerProp()
	{
		if (Prop == null)
		{
			Prop = new CObjectProp(EPropType.Spawner.ToString(), ScenarioManager.ObjectImportType.Spawner, ArrayIndex, null, null, null, StartingMapGuid);
			CSpawn_MessageData message = new CSpawn_MessageData(null)
			{
				m_SpawnDelay = 0f,
				m_Prop = Prop
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public virtual void HideSpawnerProp()
	{
		if (Prop != null)
		{
			CActivateProp_MessageData message = new CActivateProp_MessageData(null)
			{
				m_Prop = Prop,
				m_InitialLoad = true
			};
			ScenarioRuleClient.MessageHandler(message);
			Prop = null;
		}
	}

	public void OnScenarioStart(bool firstLoad = false)
	{
		if (firstLoad)
		{
			IsActive = SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ScenarioStart;
		}
	}

	public void OnStartRound(int partySize, int roundNumber)
	{
		if (IsActive && roundNumber >= SpawnerData.SpawnStartRound)
		{
			SpawnRoundCounter++;
			if (SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartRound || SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.Both)
			{
				bool treatAsSummon = true;
				if (roundNumber <= 1 || (SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.RoomOpen && SpawnRoundCounter <= 1))
				{
					treatAsSummon = false;
				}
				TrySpawnUnit(partySize, treatAsSummon, initial: false, startRound: true);
			}
		}
		WillNewEnemyExistNextRound(partySize, roundNumber);
	}

	public void OnEndRound(int partySize, int roundNumber)
	{
		if (IsActive && roundNumber >= SpawnerData.SpawnStartRound && (SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.EndRound || SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.Both))
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnStartTurn(int partySize, int roundNumber)
	{
		if (IsActive && roundNumber >= SpawnerData.SpawnStartRound && SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartTurn)
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnEndTurn(int partySize, int roundNumber)
	{
		if (IsActive && roundNumber >= SpawnerData.SpawnStartRound && SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.EndTurn)
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnRoomOpened(int partySize, bool initial = false)
	{
		if (!initial && IsActive)
		{
			bool treatAsSummon = false;
			TrySpawnUnit(partySize, treatAsSummon, initial);
		}
	}

	public void OnPressurePlateTriggered(int partySize)
	{
		if (IsActive)
		{
			TrySpawnUnit(partySize);
		}
	}

	private void TrySpawnUnit(int partySize, bool treatAsSummon = false, bool initial = false, bool startRound = false)
	{
		int num = SpawnerData.SpawnRoundInterval[partySize - 1];
		if (num != 0 && SpawnRoundCounter % num == 0)
		{
			SpawnUnit(partySize, treatAsSummon, initial, startRound);
		}
	}

	public void SpawnUnit(int partySize, bool treatAsSummon = false, bool initial = false, bool startRound = false, bool forceUseAnyEntry = false)
	{
		SpawnRoundEntry spawnRoundEntry = null;
		if (CurrentSpawnRoundEntryIndex < SpawnRoundEntries.Count)
		{
			spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
		}
		CurrentSpawnRoundEntryIndex++;
		if (SpawnerData.LoopSpawnPattern && CurrentSpawnRoundEntryIndex >= SpawnRoundEntries.Count)
		{
			CurrentSpawnRoundEntryIndex = 0;
		}
		if (spawnRoundEntry == null && forceUseAnyEntry)
		{
			if (SpawnRoundEntries.Count > CurrentSpawnRoundEntryIndex)
			{
				spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
				CurrentSpawnRoundEntryIndex++;
			}
			else if (SpawnRoundEntries.Count > 0)
			{
				CurrentSpawnRoundEntryIndex = 0;
				spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
				CurrentSpawnRoundEntryIndex++;
			}
			if (spawnRoundEntry == null)
			{
				DLLDebug.LogWarning("Unable to find any entry from which to spawn on the spawner, nothing getting spawned");
			}
		}
		if (spawnRoundEntry == null)
		{
			return;
		}
		string text = spawnRoundEntry.SpawnClass[partySize - 1];
		bool flag = text == "PROP_BearTrap";
		CMapTile cMapTile = FindSpawnTile(initial, !flag);
		if (cMapTile != null)
		{
			CTile cTile = ScenarioManager.Tiles[cMapTile.ArrayIndex.X, cMapTile.ArrayIndex.Y];
			if (flag)
			{
				CObjectTrap cObjectTrap = new CObjectTrap(ESpecificPropType.BearTrap.ToString(), ScenarioManager.ObjectImportType.Trap, new TileIndex(cTile.m_ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, 1, 0, 0, new List<CCondition.ENegativeCondition>(), null, 0);
				cTile.SpawnProp(cObjectTrap);
				SimpleLog.AddToSimpleLog("(SpawnerGUID: " + SpawnerGuid + " spawned Trap): " + ESpecificPropType.BearTrap.ToString() + " at Array Index: " + cTile.m_ArrayIndex.ToString());
				CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
				if (cActor != null)
				{
					cObjectTrap.Activate(cActor);
				}
				HasSpawned = true;
				return;
			}
			CClass cClass = MonsterClassManager.Find(text);
			if (cClass != null)
			{
				if (ActorTypeToSpawn == CActor.EType.Player || ActorTypeToSpawn == CActor.EType.HeroSummon || ActorTypeToSpawn == CActor.EType.Unknown)
				{
					DLLDebug.LogWarning("Unable to spawn Unit with ID " + text + " from Spawner SpawnerGUID: " + SpawnerGuid + " StartingMapGuid: " + StartingMapGuid + " because the \"" + ActorTypeToSpawn.ToString() + "\" Type is unsupported for Spawners");
					return;
				}
				int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cClass.Models.Count);
				EnemyState enemyState = new EnemyState(cClass.ID, chosenModelIndex, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), cClass.Health(), cClass.Health(), partySize, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, treatAsSummon, null, 1, ActorTypeToSpawn);
				if (AIFocusOverride != null)
				{
					enemyState.SetAIFocusOverride(AIFocusOverride);
				}
				CEnemyActor cEnemyActor = null;
				switch (ActorTypeToSpawn)
				{
				case CActor.EType.Enemy:
					cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial, noIDRegen: false, startRound);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
					}
					break;
				case CActor.EType.Ally:
					cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial, noIDRegen: false, startRound);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
					}
					break;
				case CActor.EType.Enemy2:
					cEnemyActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial, noIDRegen: false, startRound);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
					}
					break;
				case CActor.EType.Neutral:
					cEnemyActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial, noIDRegen: false, startRound);
					if (cEnemyActor != null)
					{
						ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
					}
					break;
				}
				if (cEnemyActor == null)
				{
					return;
				}
				HasSpawned = true;
				if (!initial)
				{
					ScenarioManager.CurrentScenarioState.Update();
					CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
					{
						m_SpawnTile = cTile,
						m_SpawnEnemy = cEnemyActor
					};
					ScenarioRuleClient.MessageHandler(message);
					ScenarioManager.CurrentScenarioState.Update();
					foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawned))
					{
						item.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, cEnemyActor, partySize);
					}
					ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				}
				SimpleLog.AddToSimpleLog("(SpawnerGUID: " + SpawnerGuid + " spawned Actor): " + cEnemyActor.Class.ID + cEnemyActor.ID + " at Array Index: " + cEnemyActor.ArrayIndex.ToString());
			}
			else
			{
				DLLDebug.LogWarning("Unable to find unitClass with ID " + text + " from Spawner SpawnerGUID: " + SpawnerGuid + " StartingMapGuid: " + StartingMapGuid);
			}
		}
		else
		{
			DLLDebug.LogWarning("Unable to find spawn tile to spawn unit from Spawner SpawnerGUID: " + SpawnerGuid + " StartingMapGuid: " + StartingMapGuid);
		}
	}

	private CMapTile FindSpawnTile(bool initial, bool needEmptyTile = true)
	{
		if (needEmptyTile)
		{
			CMapTile cMapTile = null;
			List<CMapTile> mapTiles = FindClosestEmptyTileList(initial);
			cMapTile = FindEmptyTileInList(mapTiles, initial);
			if (cMapTile == null && ScenarioManager.Scenario.FindActorAt(new Point(ArrayIndex)) == null)
			{
				cMapTile = StartingMap.MapTiles.FirstOrDefault((CMapTile t) => t.ArrayIndex.X == ArrayIndex.X && t.ArrayIndex.Y == ArrayIndex.Y);
			}
			return cMapTile;
		}
		return StartingMap.MapTiles.FirstOrDefault((CMapTile t) => t.ArrayIndex.X == ArrayIndex.X && t.ArrayIndex.Y == ArrayIndex.Y);
	}

	private CMapTile FindEmptyTileInList(List<CMapTile> mapTiles, bool initial)
	{
		CMapTile cMapTile = null;
		while (mapTiles.Count > 0 && cMapTile == null)
		{
			CMapTile cMapTile2 = mapTiles[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(mapTiles.Count)];
			CTile cTile = ScenarioManager.Tiles[cMapTile2.ArrayIndex.X, cMapTile2.ArrayIndex.Y];
			CTile propTile = null;
			if (CAbilityFilter.IsValidTile(cTile, CAbilityFilter.EFilterTile.EmptyHex, initial) && CObjectProp.FindPropWithPathingBlocker(cTile.m_ArrayIndex, ref propTile) == null)
			{
				cMapTile = cMapTile2;
			}
			else
			{
				mapTiles.Remove(cMapTile2);
			}
		}
		return cMapTile;
	}

	private List<CMapTile> FindClosestEmptyTileList(bool initial)
	{
		CTile spawnerTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
		List<CMapTile> list = new List<CMapTile>();
		CTile propTile = null;
		if (!(this is CInteractableSpawner) && ScenarioManager.PathFinder.Nodes[spawnerTile.m_ArrayIndex.X, spawnerTile.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(spawnerTile, CAbilityFilter.EFilterTile.EmptyHex, initial) && CObjectProp.FindPropWithPathingBlocker(spawnerTile.m_ArrayIndex, ref propTile) == null && (ScenarioManager.CurrentScenarioState.RoundNumber > 1 || !ScenarioManager.StartingTiles.Contains(spawnerTile)))
		{
			CMapTile item = StartingMap.MapTiles.SingleOrDefault((CMapTile x) => x.ArrayIndex.X == spawnerTile.m_ArrayIndex.X && x.ArrayIndex.Y == spawnerTile.m_ArrayIndex.Y);
			list.Add(item);
		}
		else
		{
			string text = "(SpawnerGUID: " + SpawnerGuid + " Spawner Tile Blocked):\n";
			string text2 = "";
			CActor cActor = ScenarioManager.Scenario.FindActorAt(spawnerTile.m_ArrayIndex);
			if (cActor != null)
			{
				text2 = cActor.Class.ID + ((cActor is CPlayerActor) ? "" : cActor.ID.ToString());
			}
			else
			{
				for (int num = 0; num < spawnerTile.m_Props.Count; num++)
				{
					CObjectProp cObjectProp = spawnerTile.m_Props[num];
					text2 = text2 + cObjectProp.InstanceName + ((num < spawnerTile.m_Props.Count - 1) ? ", " : "");
				}
				if (ScenarioManager.CurrentScenarioState.RoundNumber <= 1 && ScenarioManager.StartingTiles.Contains(spawnerTile))
				{
					text2 += (string.IsNullOrEmpty(text2) ? "" : ", Starting Tile on Round 1");
				}
			}
			text = text + "Tile " + spawnerTile.m_ArrayIndex.ToString() + " Blocked By: " + text2;
			SimpleLog.AddToSimpleLog(text);
			for (int num2 = 0; num2 < 10; num2++)
			{
				List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(spawnerTile, num2 + 1);
				allUnblockedTilesFromOrigin.Remove(spawnerTile);
				List<CTile> list2 = new List<CTile>();
				List<CTile> list3 = new List<CTile>();
				foreach (CTile item2 in allUnblockedTilesFromOrigin)
				{
					if (ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex, initial) && CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile) == null && (ScenarioManager.CurrentScenarioState.RoundNumber > 1 || !ScenarioManager.StartingTiles.Contains(item2)))
					{
						list2.Add(item2);
					}
					else
					{
						list3.Add(item2);
					}
				}
				string text3 = "(SpawnerGUID: " + SpawnerGuid + " Check Spawner Ring " + (num2 + 1) + "):\nEmpty Tiles Found:";
				for (int num3 = 0; num3 < list2.Count; num3++)
				{
					CTile cTile = list2[num3];
					text3 = text3 + "\nTile " + cTile.m_ArrayIndex.ToString();
				}
				text3 += "\nBlocked Tiles Found:";
				for (int num4 = 0; num4 < list3.Count; num4++)
				{
					CTile cTile2 = list3[num4];
					text2 = "";
					CActor cActor2 = ScenarioManager.Scenario.FindActorAt(cTile2.m_ArrayIndex);
					if (cActor2 != null)
					{
						text2 = cActor2.Class.ID + ((cActor2 is CPlayerActor) ? "" : cActor2.ID.ToString());
					}
					else
					{
						for (int num5 = 0; num5 < cTile2.m_Props.Count; num5++)
						{
							CObjectProp cObjectProp2 = cTile2.m_Props[num5];
							text2 = text2 + cObjectProp2.InstanceName + ((num5 < cTile2.m_Props.Count - 1) ? ", " : "");
						}
						if (ScenarioManager.CurrentScenarioState.RoundNumber <= 1 && ScenarioManager.StartingTiles.Contains(cTile2))
						{
							text2 += (string.IsNullOrEmpty(text2) ? "" : ", Starting Tile on Round 1");
						}
					}
					text3 = text3 + "\nTile " + cTile2.m_ArrayIndex.ToString() + " Blocked By: " + text2;
				}
				SimpleLog.AddToSimpleLog(text3);
				if (list2.Count <= 0)
				{
					continue;
				}
				foreach (CTile emptyRingTile in list2)
				{
					CMapTile cMapTile = StartingMap.MapTiles.SingleOrDefault((CMapTile x) => x.ArrayIndex.X == emptyRingTile.m_ArrayIndex.X && x.ArrayIndex.Y == emptyRingTile.m_ArrayIndex.Y);
					if (cMapTile != null)
					{
						list.Add(cMapTile);
						continue;
					}
					SimpleLog.AddToSimpleLog("Empty tile: " + emptyRingTile.m_ArrayIndex.ToString() + " is not on the same room map as (SpawnerGUID: " + SpawnerGuid + ")");
				}
				break;
			}
		}
		return list;
	}

	public bool WillNewEnemyExistNextRound(int partySize, int currentRound)
	{
		if (IsActive && currentRound >= SpawnerData.SpawnStartRound && (!HasSpawned || SpawnerData.LoopSpawnPattern))
		{
			int num = SpawnerData.SpawnRoundInterval[partySize - 1];
			if (num != 0)
			{
				bool flag = false;
				switch (SpawnerData.SpawnerTriggerType)
				{
				case SpawnerData.ESpawnerTriggerType.EndRound:
					flag = SpawnRoundCounter % num == 0;
					break;
				case SpawnerData.ESpawnerTriggerType.StartRound:
					flag = (SpawnRoundCounter + 1) % num == 0;
					break;
				case SpawnerData.ESpawnerTriggerType.Both:
					flag = SpawnRoundCounter % num == 0 || (SpawnRoundCounter + 1) % num == 0;
					break;
				}
				if (flag)
				{
					CClass cClass = null;
					if (CurrentSpawnRoundEntryIndex < SpawnRoundEntries.Count)
					{
						cClass = MonsterClassManager.Find(SpawnRoundEntries[CurrentSpawnRoundEntryIndex].SpawnClass[partySize - 1]);
					}
					if (cClass != null)
					{
						if (Prop == null)
						{
							CreateSpawnerProp();
						}
						return true;
					}
				}
			}
		}
		HideSpawnerProp();
		return false;
	}

	public CMonsterClass GetNextMonsterClassToSpawn(int partySize, int currentRound)
	{
		CMonsterClass cMonsterClass = null;
		if (WillNewEnemyExistNextRound(partySize, currentRound))
		{
			for (int i = CurrentSpawnRoundEntryIndex; i < SpawnRoundEntries.Count; i++)
			{
				cMonsterClass = MonsterClassManager.Find(SpawnRoundEntries[i].SpawnClass[partySize - 1]);
				if (cMonsterClass != null)
				{
					break;
				}
			}
			if (cMonsterClass == null && SpawnerData.LoopSpawnPattern)
			{
				for (int j = 0; j < SpawnRoundEntries.Count; j++)
				{
					cMonsterClass = MonsterClassManager.Find(SpawnRoundEntries[j].SpawnClass[partySize - 1]);
					if (cMonsterClass != null)
					{
						break;
					}
				}
			}
		}
		return cMonsterClass;
	}

	public int TotalMonstersLeftToSpawn()
	{
		if (!SpawnerData.LoopSpawnPattern)
		{
			return Math.Max(0, SpawnRoundEntries.Count - CurrentSpawnRoundEntryIndex);
		}
		return int.MaxValue;
	}

	public void SetConfigForPartySize(int partySize, ScenarioManager.EPerPartySizeConfig configToSet)
	{
		ConfigPerPartySize[partySize] = configToSet;
	}

	public ScenarioManager.EPerPartySizeConfig GetConfigForPartySize(int partySize)
	{
		if (ConfigPerPartySize == null || !ConfigPerPartySize.ContainsKey(partySize))
		{
			return ScenarioManager.EPerPartySizeConfig.Normal;
		}
		return ConfigPerPartySize[partySize];
	}

	private static Dictionary<int, ScenarioManager.EPerPartySizeConfig> GetDefaultConfigPerPartySize()
	{
		return new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
	}

	public static List<Tuple<int, string>> Compare(CSpawner state1, CSpawner state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.SpawnerGuid != state2.SpawnerGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2301, "CSpawner SpawnerGuid does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					}
				});
			}
			if (!TileIndex.Compare(state1.ArrayIndex, state2.ArrayIndex))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2302, "CSpawner ArrayIndex does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					}
				});
			}
			if (state1.IsActive != state2.IsActive)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2303, "CSpawner IsActive does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"IsActive",
						state1.IsActive.ToString(),
						state2.IsActive.ToString()
					}
				});
			}
			if (state1.SpawnRoundCounter != state2.SpawnRoundCounter)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2304, "CSpawner SpawnRoundCounter does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnRoundCounter",
						state1.SpawnRoundCounter.ToString(),
						state2.SpawnRoundCounter.ToString()
					}
				});
			}
			if (state1.CurrentSpawnRoundEntryIndex != state2.CurrentSpawnRoundEntryIndex)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2305, "CSpawner CurrentSpawnRoundEntryIndex does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"CurrentSpawnRoundEntryIndex",
						state1.CurrentSpawnRoundEntryIndex.ToString(),
						state2.CurrentSpawnRoundEntryIndex.ToString()
					}
				});
			}
			if (state1.StartingMapGuid != state2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2306, "CSpawner StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3] { "StartingMapGuid", state1.StartingMapGuid, state2.StartingMapGuid }
				});
			}
			list.AddRange(SpawnerData.Compare(state1.SpawnerData, state2.SpawnerData, state1.SpawnerGuid, state1.ArrayIndex, isMPCompare));
			if (StateShared.CheckNullsMatch(state1.AIFocusOverride, state2.AIFocusOverride) == StateShared.ENullStatus.Mismatch)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2310, "CSpawner AIFocusOverride null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"AIFocusOverride",
						(state1.AIFocusOverride == null) ? "is null" : "is not null",
						(state2.AIFocusOverride == null) ? "is null" : "is not null"
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.ConfigPerPartySize, state2.ConfigPerPartySize))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2311, "CSpawner ConfigPerPartySize null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"ConfigPerPartySize",
						(state1.ConfigPerPartySize == null) ? "is null" : "is not null",
						(state2.ConfigPerPartySize == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.ConfigPerPartySize.Count != state2.ConfigPerPartySize.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2312, "CSpawner total ConfigPerPartySize Count does not match.", new List<string[]>
					{
						new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
						new string[3]
						{
							"ConfigPerPartySize Count",
							state1.ConfigPerPartySize.Count.ToString(),
							state2.ConfigPerPartySize.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in state1.ConfigPerPartySize)
				{
					if (!state2.ConfigPerPartySize.ContainsKey(item.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2313, "CSpawner ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item.Key.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item2 in state2.ConfigPerPartySize)
				{
					if (!state1.ConfigPerPartySize.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2314, "CSpawner ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								"Missing",
								item2.Key.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item3 in state1.ConfigPerPartySize)
				{
					if (item3.Value != state2.ConfigPerPartySize[item3.Key])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2315, "CSpawner ConfigPerPartySize has key with differing values.", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item3.Key.ToString(),
								item3.Key.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Value",
								item3.Value.ToString(),
								state2.ConfigPerPartySize[item3.Key].ToString()
							}
						});
						flag = true;
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2399, "Exception during CSpawner compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
