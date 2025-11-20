using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("Name:{Name} ID:{ID}")]
public class ScenarioState : ISerializable
{
	public static readonly EScenarioType[] MapScenarioTypes = (EScenarioType[])Enum.GetValues(typeof(EScenarioType));

	private SharedLibrary.Random m_ScenarioRNG;

	private SharedLibrary.Random m_EnemyIDRNG;

	private SharedLibrary.Random m_EnemyAbilityCardRNG;

	private SharedLibrary.Random m_GuidRNG;

	public string Name { get; protected set; }

	public string Description { get; private set; }

	public string ID { get; protected set; }

	public int Level { get; protected set; }

	public int Width { get; protected set; }

	public int Height { get; protected set; }

	public CVectorInt3 PositiveSpaceOffset { get; protected set; }

	public bool IsInitialised { get; protected set; }

	public int Seed { get; set; }

	public int? SeedFromMap { get; set; }

	public bool IsFirstLoad { get; protected set; }

	public EScenarioType ScenarioType { get; set; }

	public string ScenarioFileName { get; private set; }

	public ApparanceStyle Style { get; set; }

	public List<string> ChestTreasureTables { get; private set; }

	public List<string> RewardsTreasureTables { get; private set; }

	public int ActiveBonusIDCounter { get; private set; }

	public StatsState Stats { get; set; }

	public int RoundNumber { get; set; }

	public List<CMap> Maps { get; set; }

	public List<CMap> MapsFailedToLoad { get; set; }

	public List<PlayerState> Players { get; private set; }

	public List<EnemyState> Monsters { get; private set; }

	public List<EnemyState> AllyMonsters { get; private set; }

	public List<EnemyState> Enemy2Monsters { get; private set; }

	public List<EnemyState> NeutralMonsters { get; private set; }

	public List<HeroSummonState> HeroSummons { get; private set; }

	public List<ObjectState> Objects { get; private set; }

	public EnemyClassManagerState EnemyClassManager { get; protected set; }

	public HeroSummonClassManagerState HeroSummonClassManager { get; protected set; }

	public List<CObjectProp> Props { get; private set; }

	public List<CObjectProp> ChestProps => Props.Where((CObjectProp d) => d.ObjectType == ScenarioManager.ObjectImportType.Chest || d.ObjectType == ScenarioManager.ObjectImportType.GoalChest).ToList();

	public List<CObjectProp> DoorProps => Props.Where((CObjectProp d) => d.ObjectType == ScenarioManager.ObjectImportType.Door).ToList();

	public List<CObjectProp> PortalProps => Props.Where((CObjectProp d) => d.ObjectType == ScenarioManager.ObjectImportType.Portal).ToList();

	public List<CObjectProp> ResourceProps => Props.Where((CObjectProp d) => d.ObjectType == ScenarioManager.ObjectImportType.Resource).ToList();

	public List<CObjectProp> ActivatedProps { get; private set; }

	public List<CObjectObstacle> TransparentProps { get; private set; }

	public List<CObjectProp> DestroyedProps { get; private set; }

	public List<CSpawner> Spawners { get; private set; }

	public ElementInfusionBoardManager.EColumn[] ElementColumn { get; protected set; }

	public List<CObjective> WinObjectives { get; private set; }

	public List<CObjective> LoseObjectives { get; private set; }

	public List<CObjective> AllObjectives => LoseObjectives.Concat(WinObjectives).ToList();

	public List<CScenarioModifier> ScenarioModifiers { get; private set; }

	public List<Tuple<string, RewardGroup>> RoundChestRewards { get; private set; }

	public List<Tuple<string, RewardGroup>> GoalChestRewards { get; private set; }

	public Extensions.RandomState ScenarioRNGState { get; set; }

	public SharedLibrary.Random ScenarioRNG
	{
		get
		{
			if (LoadingScenarioState)
			{
				SimpleLog.AddToSimpleLog("[ScenarioRNG] - " + PeekScenarioRNG);
			}
			return m_ScenarioRNG;
		}
	}

	public int PeekScenarioRNG => m_ScenarioRNG.Save().Restore().Next();

	public bool ScenarioRNGNotNull => m_ScenarioRNG != null;

	public Extensions.RandomState EnemyIDRNGState { get; set; }

	public SharedLibrary.Random EnemyIDRNG
	{
		get
		{
			SimpleLog.AddToSimpleLog("[EnemyIDRNG] - " + PeekEnemyIDRNG);
			return m_EnemyIDRNG;
		}
	}

	public int PeekEnemyIDRNG => m_EnemyIDRNG.Save().Restore().Next();

	public Extensions.RandomState EnemyAbilityCardRNGState { get; set; }

	public SharedLibrary.Random EnemyAbilityCardRNG => m_EnemyAbilityCardRNG;

	public int PeekEnemyAbilityCardRNG => m_EnemyAbilityCardRNG.Save().Restore().Next();

	public Extensions.RandomState GuidRNGState { get; set; }

	public SharedLibrary.Random GuidRNG => m_GuidRNG;

	public int PeekGuidRNG => m_GuidRNG.Save().Restore().Next();

	public SEventLog ScenarioEventLog { get; private set; }

	public bool StateNeedsUpdatesSaved { get; set; }

	public bool LoadingScenarioState { get; set; }

	public List<ActorState> ActorStates
	{
		get
		{
			List<ActorState> list = new List<ActorState>();
			list.AddRange(Players);
			list.AddRange(Monsters);
			list.AddRange(HeroSummons);
			list.AddRange(AllyMonsters);
			list.AddRange(NeutralMonsters);
			list.AddRange(Enemy2Monsters);
			list.AddRange(Objects);
			return list;
		}
	}

	public List<EnemyState> AllEnemyStates => Monsters.Concat(AllyMonsters).Concat(Enemy2Monsters).Concat(NeutralMonsters)
		.Concat(Objects)
		.ToList();

	public ScenarioState(ScenarioState state, ReferenceDictionary references)
	{
		Name = state.Name;
		Description = state.Description;
		ID = state.ID;
		Level = state.Level;
		Width = state.Width;
		Height = state.Height;
		PositiveSpaceOffset = references.Get(state.PositiveSpaceOffset);
		if (PositiveSpaceOffset == null && state.PositiveSpaceOffset != null)
		{
			PositiveSpaceOffset = new CVectorInt3(state.PositiveSpaceOffset, references);
			references.Add(state.PositiveSpaceOffset, PositiveSpaceOffset);
		}
		IsInitialised = state.IsInitialised;
		Seed = state.Seed;
		SeedFromMap = state.SeedFromMap;
		IsFirstLoad = state.IsFirstLoad;
		ScenarioType = state.ScenarioType;
		ScenarioFileName = state.ScenarioFileName;
		Style = references.Get(state.Style);
		if (Style == null && state.Style != null)
		{
			Style = new ApparanceStyle(state.Style, references);
			references.Add(state.Style, Style);
		}
		ChestTreasureTables = references.Get(state.ChestTreasureTables);
		if (ChestTreasureTables == null && state.ChestTreasureTables != null)
		{
			ChestTreasureTables = new List<string>();
			for (int i = 0; i < state.ChestTreasureTables.Count; i++)
			{
				string item = state.ChestTreasureTables[i];
				ChestTreasureTables.Add(item);
			}
			references.Add(state.ChestTreasureTables, ChestTreasureTables);
		}
		RewardsTreasureTables = references.Get(state.RewardsTreasureTables);
		if (RewardsTreasureTables == null && state.RewardsTreasureTables != null)
		{
			RewardsTreasureTables = new List<string>();
			for (int j = 0; j < state.RewardsTreasureTables.Count; j++)
			{
				string item2 = state.RewardsTreasureTables[j];
				RewardsTreasureTables.Add(item2);
			}
			references.Add(state.RewardsTreasureTables, RewardsTreasureTables);
		}
		ActiveBonusIDCounter = state.ActiveBonusIDCounter;
		Stats = references.Get(state.Stats);
		if (Stats == null && state.Stats != null)
		{
			Stats = new StatsState(state.Stats, references);
			references.Add(state.Stats, Stats);
		}
		RoundNumber = state.RoundNumber;
		Maps = references.Get(state.Maps);
		if (Maps == null && state.Maps != null)
		{
			Maps = new List<CMap>();
			for (int k = 0; k < state.Maps.Count; k++)
			{
				CMap cMap = state.Maps[k];
				CMap cMap2 = references.Get(cMap);
				if (cMap2 == null && cMap != null)
				{
					cMap2 = new CMap(cMap, references);
					references.Add(cMap, cMap2);
				}
				Maps.Add(cMap2);
			}
			references.Add(state.Maps, Maps);
		}
		MapsFailedToLoad = references.Get(state.MapsFailedToLoad);
		if (MapsFailedToLoad == null && state.MapsFailedToLoad != null)
		{
			MapsFailedToLoad = new List<CMap>();
			for (int l = 0; l < state.MapsFailedToLoad.Count; l++)
			{
				CMap cMap3 = state.MapsFailedToLoad[l];
				CMap cMap4 = references.Get(cMap3);
				if (cMap4 == null && cMap3 != null)
				{
					cMap4 = new CMap(cMap3, references);
					references.Add(cMap3, cMap4);
				}
				MapsFailedToLoad.Add(cMap4);
			}
			references.Add(state.MapsFailedToLoad, MapsFailedToLoad);
		}
		Players = references.Get(state.Players);
		if (Players == null && state.Players != null)
		{
			Players = new List<PlayerState>();
			for (int m = 0; m < state.Players.Count; m++)
			{
				PlayerState playerState = state.Players[m];
				PlayerState playerState2 = references.Get(playerState);
				if (playerState2 == null && playerState != null)
				{
					playerState2 = new PlayerState(playerState, references);
					references.Add(playerState, playerState2);
				}
				Players.Add(playerState2);
			}
			references.Add(state.Players, Players);
		}
		Monsters = references.Get(state.Monsters);
		if (Monsters == null && state.Monsters != null)
		{
			Monsters = new List<EnemyState>();
			for (int n = 0; n < state.Monsters.Count; n++)
			{
				EnemyState enemyState = state.Monsters[n];
				EnemyState enemyState2 = references.Get(enemyState);
				if (enemyState2 == null && enemyState != null)
				{
					EnemyState enemyState3 = ((!(enemyState is ObjectState state2)) ? new EnemyState(enemyState, references) : new ObjectState(state2, references));
					enemyState2 = enemyState3;
					references.Add(enemyState, enemyState2);
				}
				Monsters.Add(enemyState2);
			}
			references.Add(state.Monsters, Monsters);
		}
		AllyMonsters = references.Get(state.AllyMonsters);
		if (AllyMonsters == null && state.AllyMonsters != null)
		{
			AllyMonsters = new List<EnemyState>();
			for (int num = 0; num < state.AllyMonsters.Count; num++)
			{
				EnemyState enemyState4 = state.AllyMonsters[num];
				EnemyState enemyState5 = references.Get(enemyState4);
				if (enemyState5 == null && enemyState4 != null)
				{
					EnemyState enemyState3 = ((!(enemyState4 is ObjectState state3)) ? new EnemyState(enemyState4, references) : new ObjectState(state3, references));
					enemyState5 = enemyState3;
					references.Add(enemyState4, enemyState5);
				}
				AllyMonsters.Add(enemyState5);
			}
			references.Add(state.AllyMonsters, AllyMonsters);
		}
		Enemy2Monsters = references.Get(state.Enemy2Monsters);
		if (Enemy2Monsters == null && state.Enemy2Monsters != null)
		{
			Enemy2Monsters = new List<EnemyState>();
			for (int num2 = 0; num2 < state.Enemy2Monsters.Count; num2++)
			{
				EnemyState enemyState6 = state.Enemy2Monsters[num2];
				EnemyState enemyState7 = references.Get(enemyState6);
				if (enemyState7 == null && enemyState6 != null)
				{
					EnemyState enemyState3 = ((!(enemyState6 is ObjectState state4)) ? new EnemyState(enemyState6, references) : new ObjectState(state4, references));
					enemyState7 = enemyState3;
					references.Add(enemyState6, enemyState7);
				}
				Enemy2Monsters.Add(enemyState7);
			}
			references.Add(state.Enemy2Monsters, Enemy2Monsters);
		}
		NeutralMonsters = references.Get(state.NeutralMonsters);
		if (NeutralMonsters == null && state.NeutralMonsters != null)
		{
			NeutralMonsters = new List<EnemyState>();
			for (int num3 = 0; num3 < state.NeutralMonsters.Count; num3++)
			{
				EnemyState enemyState8 = state.NeutralMonsters[num3];
				EnemyState enemyState9 = references.Get(enemyState8);
				if (enemyState9 == null && enemyState8 != null)
				{
					EnemyState enemyState3 = ((!(enemyState8 is ObjectState state5)) ? new EnemyState(enemyState8, references) : new ObjectState(state5, references));
					enemyState9 = enemyState3;
					references.Add(enemyState8, enemyState9);
				}
				NeutralMonsters.Add(enemyState9);
			}
			references.Add(state.NeutralMonsters, NeutralMonsters);
		}
		HeroSummons = references.Get(state.HeroSummons);
		if (HeroSummons == null && state.HeroSummons != null)
		{
			HeroSummons = new List<HeroSummonState>();
			for (int num4 = 0; num4 < state.HeroSummons.Count; num4++)
			{
				HeroSummonState heroSummonState = state.HeroSummons[num4];
				HeroSummonState heroSummonState2 = references.Get(heroSummonState);
				if (heroSummonState2 == null && heroSummonState != null)
				{
					heroSummonState2 = new HeroSummonState(heroSummonState, references);
					references.Add(heroSummonState, heroSummonState2);
				}
				HeroSummons.Add(heroSummonState2);
			}
			references.Add(state.HeroSummons, HeroSummons);
		}
		Objects = references.Get(state.Objects);
		if (Objects == null && state.Objects != null)
		{
			Objects = new List<ObjectState>();
			for (int num5 = 0; num5 < state.Objects.Count; num5++)
			{
				ObjectState objectState = state.Objects[num5];
				ObjectState objectState2 = references.Get(objectState);
				if (objectState2 == null && objectState != null)
				{
					objectState2 = new ObjectState(objectState, references);
					references.Add(objectState, objectState2);
				}
				Objects.Add(objectState2);
			}
			references.Add(state.Objects, Objects);
		}
		EnemyClassManager = references.Get(state.EnemyClassManager);
		if (EnemyClassManager == null && state.EnemyClassManager != null)
		{
			EnemyClassManager = new EnemyClassManagerState(state.EnemyClassManager, references);
			references.Add(state.EnemyClassManager, EnemyClassManager);
		}
		HeroSummonClassManager = references.Get(state.HeroSummonClassManager);
		if (HeroSummonClassManager == null && state.HeroSummonClassManager != null)
		{
			HeroSummonClassManager = new HeroSummonClassManagerState(state.HeroSummonClassManager, references);
			references.Add(state.HeroSummonClassManager, HeroSummonClassManager);
		}
		Props = references.Get(state.Props);
		if (Props == null && state.Props != null)
		{
			Props = new List<CObjectProp>();
			for (int num6 = 0; num6 < state.Props.Count; num6++)
			{
				CObjectProp cObjectProp = state.Props[num6];
				CObjectProp cObjectProp2 = references.Get(cObjectProp);
				if (cObjectProp2 == null && cObjectProp != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp is CObjectChest state6) ? new CObjectChest(state6, references) : ((cObjectProp is CObjectDifficultTerrain state7) ? new CObjectDifficultTerrain(state7, references) : ((cObjectProp is CObjectDoor state8) ? new CObjectDoor(state8, references) : ((cObjectProp is CObjectGoldPile state9) ? new CObjectGoldPile(state9, references) : ((cObjectProp is CObjectHazardousTerrain state10) ? new CObjectHazardousTerrain(state10, references) : ((cObjectProp is CObjectMonsterGrave state11) ? new CObjectMonsterGrave(state11, references) : ((cObjectProp is CObjectObstacle state12) ? new CObjectObstacle(state12, references) : ((cObjectProp is CObjectPortal state13) ? new CObjectPortal(state13, references) : ((cObjectProp is CObjectPressurePlate state14) ? new CObjectPressurePlate(state14, references) : ((cObjectProp is CObjectQuestItem state15) ? new CObjectQuestItem(state15, references) : ((cObjectProp is CObjectResource state16) ? new CObjectResource(state16, references) : ((cObjectProp is CObjectTerrainVisual state17) ? new CObjectTerrainVisual(state17, references) : ((!(cObjectProp is CObjectTrap state18)) ? new CObjectProp(cObjectProp, references) : new CObjectTrap(state18, references))))))))))))));
					cObjectProp2 = cObjectProp3;
					references.Add(cObjectProp, cObjectProp2);
				}
				Props.Add(cObjectProp2);
			}
			references.Add(state.Props, Props);
		}
		ActivatedProps = references.Get(state.ActivatedProps);
		if (ActivatedProps == null && state.ActivatedProps != null)
		{
			ActivatedProps = new List<CObjectProp>();
			for (int num7 = 0; num7 < state.ActivatedProps.Count; num7++)
			{
				CObjectProp cObjectProp4 = state.ActivatedProps[num7];
				CObjectProp cObjectProp5 = references.Get(cObjectProp4);
				if (cObjectProp5 == null && cObjectProp4 != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp4 is CObjectChest state19) ? new CObjectChest(state19, references) : ((cObjectProp4 is CObjectDifficultTerrain state20) ? new CObjectDifficultTerrain(state20, references) : ((cObjectProp4 is CObjectDoor state21) ? new CObjectDoor(state21, references) : ((cObjectProp4 is CObjectGoldPile state22) ? new CObjectGoldPile(state22, references) : ((cObjectProp4 is CObjectHazardousTerrain state23) ? new CObjectHazardousTerrain(state23, references) : ((cObjectProp4 is CObjectMonsterGrave state24) ? new CObjectMonsterGrave(state24, references) : ((cObjectProp4 is CObjectObstacle state25) ? new CObjectObstacle(state25, references) : ((cObjectProp4 is CObjectPortal state26) ? new CObjectPortal(state26, references) : ((cObjectProp4 is CObjectPressurePlate state27) ? new CObjectPressurePlate(state27, references) : ((cObjectProp4 is CObjectQuestItem state28) ? new CObjectQuestItem(state28, references) : ((cObjectProp4 is CObjectResource state29) ? new CObjectResource(state29, references) : ((cObjectProp4 is CObjectTerrainVisual state30) ? new CObjectTerrainVisual(state30, references) : ((!(cObjectProp4 is CObjectTrap state31)) ? new CObjectProp(cObjectProp4, references) : new CObjectTrap(state31, references))))))))))))));
					cObjectProp5 = cObjectProp3;
					references.Add(cObjectProp4, cObjectProp5);
				}
				ActivatedProps.Add(cObjectProp5);
			}
			references.Add(state.ActivatedProps, ActivatedProps);
		}
		TransparentProps = references.Get(state.TransparentProps);
		if (TransparentProps == null && state.TransparentProps != null)
		{
			TransparentProps = new List<CObjectObstacle>();
			for (int num8 = 0; num8 < state.TransparentProps.Count; num8++)
			{
				CObjectObstacle cObjectObstacle = state.TransparentProps[num8];
				CObjectObstacle cObjectObstacle2 = references.Get(cObjectObstacle);
				if (cObjectObstacle2 == null && cObjectObstacle != null)
				{
					cObjectObstacle2 = new CObjectObstacle(cObjectObstacle, references);
					references.Add(cObjectObstacle, cObjectObstacle2);
				}
				TransparentProps.Add(cObjectObstacle2);
			}
			references.Add(state.TransparentProps, TransparentProps);
		}
		DestroyedProps = references.Get(state.DestroyedProps);
		if (DestroyedProps == null && state.DestroyedProps != null)
		{
			DestroyedProps = new List<CObjectProp>();
			for (int num9 = 0; num9 < state.DestroyedProps.Count; num9++)
			{
				CObjectProp cObjectProp6 = state.DestroyedProps[num9];
				CObjectProp cObjectProp7 = references.Get(cObjectProp6);
				if (cObjectProp7 == null && cObjectProp6 != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp6 is CObjectChest state32) ? new CObjectChest(state32, references) : ((cObjectProp6 is CObjectDifficultTerrain state33) ? new CObjectDifficultTerrain(state33, references) : ((cObjectProp6 is CObjectDoor state34) ? new CObjectDoor(state34, references) : ((cObjectProp6 is CObjectGoldPile state35) ? new CObjectGoldPile(state35, references) : ((cObjectProp6 is CObjectHazardousTerrain state36) ? new CObjectHazardousTerrain(state36, references) : ((cObjectProp6 is CObjectMonsterGrave state37) ? new CObjectMonsterGrave(state37, references) : ((cObjectProp6 is CObjectObstacle state38) ? new CObjectObstacle(state38, references) : ((cObjectProp6 is CObjectPortal state39) ? new CObjectPortal(state39, references) : ((cObjectProp6 is CObjectPressurePlate state40) ? new CObjectPressurePlate(state40, references) : ((cObjectProp6 is CObjectQuestItem state41) ? new CObjectQuestItem(state41, references) : ((cObjectProp6 is CObjectResource state42) ? new CObjectResource(state42, references) : ((cObjectProp6 is CObjectTerrainVisual state43) ? new CObjectTerrainVisual(state43, references) : ((!(cObjectProp6 is CObjectTrap state44)) ? new CObjectProp(cObjectProp6, references) : new CObjectTrap(state44, references))))))))))))));
					cObjectProp7 = cObjectProp3;
					references.Add(cObjectProp6, cObjectProp7);
				}
				DestroyedProps.Add(cObjectProp7);
			}
			references.Add(state.DestroyedProps, DestroyedProps);
		}
		Spawners = references.Get(state.Spawners);
		if (Spawners == null && state.Spawners != null)
		{
			Spawners = new List<CSpawner>();
			for (int num10 = 0; num10 < state.Spawners.Count; num10++)
			{
				CSpawner cSpawner = state.Spawners[num10];
				CSpawner cSpawner2 = references.Get(cSpawner);
				if (cSpawner2 == null && cSpawner != null)
				{
					CSpawner cSpawner3 = ((!(cSpawner is CInteractableSpawner state45)) ? new CSpawner(cSpawner, references) : new CInteractableSpawner(state45, references));
					cSpawner2 = cSpawner3;
					references.Add(cSpawner, cSpawner2);
				}
				Spawners.Add(cSpawner2);
			}
			references.Add(state.Spawners, Spawners);
		}
		ElementColumn = references.Get(state.ElementColumn);
		if (ElementColumn == null && state.ElementColumn != null)
		{
			ElementColumn = new ElementInfusionBoardManager.EColumn[state.ElementColumn.Length];
			for (int num11 = 0; num11 < state.ElementColumn.Length; num11++)
			{
				ElementColumn[num11] = state.ElementColumn[num11];
			}
		}
		WinObjectives = references.Get(state.WinObjectives);
		if (WinObjectives == null && state.WinObjectives != null)
		{
			WinObjectives = new List<CObjective>();
			for (int num12 = 0; num12 < state.WinObjectives.Count; num12++)
			{
				CObjective cObjective = state.WinObjectives[num12];
				CObjective cObjective2 = references.Get(cObjective);
				if (cObjective2 == null && cObjective != null)
				{
					CObjective cObjective3 = ((cObjective is CObjective_KillAllEnemies state46) ? new CObjective_KillAllEnemies(state46, references) : ((cObjective is CObjective_KillAllBosses state47) ? new CObjective_KillAllBosses(state47, references) : ((cObjective is CObjective_ReachRound state48) ? new CObjective_ReachRound(state48, references) : ((cObjective is CObjective_ActorReachPosition state49) ? new CObjective_ActorReachPosition(state49, references) : ((cObjective is CObjective_XCharactersDie state50) ? new CObjective_XCharactersDie(state50, references) : ((cObjective is CObjective_LootX state51) ? new CObjective_LootX(state51, references) : ((cObjective is CObjective_ActivatePressurePlateX state52) ? new CObjective_ActivatePressurePlateX(state52, references) : ((cObjective is CObjective_DestroyXObjects state53) ? new CObjective_DestroyXObjects(state53, references) : ((cObjective is CObjective_CustomTrigger state54) ? new CObjective_CustomTrigger(state54, references) : ((cObjective is CObjective_RevealAllRooms state55) ? new CObjective_RevealAllRooms(state55, references) : ((cObjective is CObjective_AllCharactersMustLoot state56) ? new CObjective_AllCharactersMustLoot(state56, references) : ((cObjective is CObjective_AnyActorReachPosition state57) ? new CObjective_AnyActorReachPosition(state57, references) : ((cObjective is CObjective_DeactivateXSpawners state58) ? new CObjective_DeactivateXSpawners(state58, references) : ((cObjective is CObjective_ActivateXSpawners state59) ? new CObjective_ActivateXSpawners(state59, references) : ((cObjective is CObjective_ActorsEscaped state60) ? new CObjective_ActorsEscaped(state60, references) : ((cObjective is CObjective_DealXDamage state61) ? new CObjective_DealXDamage(state61, references) : ((cObjective is CObjective_ActorsNotInAllRooms state62) ? new CObjective_ActorsNotInAllRooms(state62, references) : ((!(cObjective is CObjective_XActorsHealToMax state63)) ? new CObjective(cObjective, references) : new CObjective_XActorsHealToMax(state63, references)))))))))))))))))));
					cObjective2 = cObjective3;
					references.Add(cObjective, cObjective2);
				}
				WinObjectives.Add(cObjective2);
			}
			references.Add(state.WinObjectives, WinObjectives);
		}
		LoseObjectives = references.Get(state.LoseObjectives);
		if (LoseObjectives == null && state.LoseObjectives != null)
		{
			LoseObjectives = new List<CObjective>();
			for (int num13 = 0; num13 < state.LoseObjectives.Count; num13++)
			{
				CObjective cObjective4 = state.LoseObjectives[num13];
				CObjective cObjective5 = references.Get(cObjective4);
				if (cObjective5 == null && cObjective4 != null)
				{
					CObjective cObjective3 = ((cObjective4 is CObjective_KillAllEnemies state64) ? new CObjective_KillAllEnemies(state64, references) : ((cObjective4 is CObjective_KillAllBosses state65) ? new CObjective_KillAllBosses(state65, references) : ((cObjective4 is CObjective_ReachRound state66) ? new CObjective_ReachRound(state66, references) : ((cObjective4 is CObjective_ActorReachPosition state67) ? new CObjective_ActorReachPosition(state67, references) : ((cObjective4 is CObjective_XCharactersDie state68) ? new CObjective_XCharactersDie(state68, references) : ((cObjective4 is CObjective_LootX state69) ? new CObjective_LootX(state69, references) : ((cObjective4 is CObjective_ActivatePressurePlateX state70) ? new CObjective_ActivatePressurePlateX(state70, references) : ((cObjective4 is CObjective_DestroyXObjects state71) ? new CObjective_DestroyXObjects(state71, references) : ((cObjective4 is CObjective_CustomTrigger state72) ? new CObjective_CustomTrigger(state72, references) : ((cObjective4 is CObjective_RevealAllRooms state73) ? new CObjective_RevealAllRooms(state73, references) : ((cObjective4 is CObjective_AllCharactersMustLoot state74) ? new CObjective_AllCharactersMustLoot(state74, references) : ((cObjective4 is CObjective_AnyActorReachPosition state75) ? new CObjective_AnyActorReachPosition(state75, references) : ((cObjective4 is CObjective_DeactivateXSpawners state76) ? new CObjective_DeactivateXSpawners(state76, references) : ((cObjective4 is CObjective_ActivateXSpawners state77) ? new CObjective_ActivateXSpawners(state77, references) : ((cObjective4 is CObjective_ActorsEscaped state78) ? new CObjective_ActorsEscaped(state78, references) : ((cObjective4 is CObjective_DealXDamage state79) ? new CObjective_DealXDamage(state79, references) : ((cObjective4 is CObjective_ActorsNotInAllRooms state80) ? new CObjective_ActorsNotInAllRooms(state80, references) : ((!(cObjective4 is CObjective_XActorsHealToMax state81)) ? new CObjective(cObjective4, references) : new CObjective_XActorsHealToMax(state81, references)))))))))))))))))));
					cObjective5 = cObjective3;
					references.Add(cObjective4, cObjective5);
				}
				LoseObjectives.Add(cObjective5);
			}
			references.Add(state.LoseObjectives, LoseObjectives);
		}
		ScenarioModifiers = references.Get(state.ScenarioModifiers);
		if (ScenarioModifiers == null && state.ScenarioModifiers != null)
		{
			ScenarioModifiers = new List<CScenarioModifier>();
			for (int num14 = 0; num14 < state.ScenarioModifiers.Count; num14++)
			{
				CScenarioModifier cScenarioModifier = state.ScenarioModifiers[num14];
				CScenarioModifier cScenarioModifier2 = references.Get(cScenarioModifier);
				if (cScenarioModifier2 == null && cScenarioModifier != null)
				{
					CScenarioModifier cScenarioModifier3 = ((cScenarioModifier is CScenarioModifierActivateClosestAI state82) ? new CScenarioModifierActivateClosestAI(state82, references) : ((cScenarioModifier is CScenarioModifierActorsCreateGraves state83) ? new CScenarioModifierActorsCreateGraves(state83, references) : ((cScenarioModifier is CScenarioModifierAddConditionsToAbilities state84) ? new CScenarioModifierAddConditionsToAbilities(state84, references) : ((cScenarioModifier is CScenarioModifierAddModifierCards state85) ? new CScenarioModifierAddModifierCards(state85, references) : ((cScenarioModifier is CScenarioModifierApplyActiveBonusToActor state86) ? new CScenarioModifierApplyActiveBonusToActor(state86, references) : ((cScenarioModifier is CScenarioModifierApplyConditionToActor state87) ? new CScenarioModifierApplyConditionToActor(state87, references) : ((cScenarioModifier is CScenarioModifierDestroyRoom state88) ? new CScenarioModifierDestroyRoom(state88, references) : ((cScenarioModifier is CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms state89) ? new CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms(state89, references) : ((cScenarioModifier is CScenarioModifierMoveActorsInDirections state90) ? new CScenarioModifierMoveActorsInDirections(state90, references) : ((cScenarioModifier is CScenarioModifierMovePropsInSequence state91) ? new CScenarioModifierMovePropsInSequence(state91, references) : ((cScenarioModifier is CScenarioModifierMovePropsToNearestPlayer state92) ? new CScenarioModifierMovePropsToNearestPlayer(state92, references) : ((cScenarioModifier is CScenarioModifierOverrideCompanionSummonTiles state93) ? new CScenarioModifierOverrideCompanionSummonTiles(state93, references) : ((cScenarioModifier is CScenarioModifierPhaseInAndTeleport state94) ? new CScenarioModifierPhaseInAndTeleport(state94, references) : ((cScenarioModifier is CScenarioModifierPhaseOut state95) ? new CScenarioModifierPhaseOut(state95, references) : ((cScenarioModifier is CScenarioModifierSetElements state96) ? new CScenarioModifierSetElements(state96, references) : ((cScenarioModifier is CScenarioModifierTeleport state97) ? new CScenarioModifierTeleport(state97, references) : ((cScenarioModifier is CScenarioModifierToggleActorDeactivated state98) ? new CScenarioModifierToggleActorDeactivated(state98, references) : ((!(cScenarioModifier is CScenarioModifierTriggerAbility state99)) ? new CScenarioModifier(cScenarioModifier, references) : new CScenarioModifierTriggerAbility(state99, references)))))))))))))))))));
					cScenarioModifier2 = cScenarioModifier3;
					references.Add(cScenarioModifier, cScenarioModifier2);
				}
				ScenarioModifiers.Add(cScenarioModifier2);
			}
			references.Add(state.ScenarioModifiers, ScenarioModifiers);
		}
		RoundChestRewards = references.Get(state.RoundChestRewards);
		if (RoundChestRewards == null && state.RoundChestRewards != null)
		{
			RoundChestRewards = new List<Tuple<string, RewardGroup>>();
			for (int num15 = 0; num15 < state.RoundChestRewards.Count; num15++)
			{
				Tuple<string, RewardGroup> tuple = state.RoundChestRewards[num15];
				string item3 = tuple.Item1;
				RewardGroup rewardGroup = references.Get(tuple.Item2);
				if (rewardGroup == null && tuple.Item2 != null)
				{
					rewardGroup = new RewardGroup(tuple.Item2, references);
					references.Add(tuple.Item2, rewardGroup);
				}
				Tuple<string, RewardGroup> item4 = new Tuple<string, RewardGroup>(item3, rewardGroup);
				RoundChestRewards.Add(item4);
			}
			references.Add(state.RoundChestRewards, RoundChestRewards);
		}
		GoalChestRewards = references.Get(state.GoalChestRewards);
		if (GoalChestRewards == null && state.GoalChestRewards != null)
		{
			GoalChestRewards = new List<Tuple<string, RewardGroup>>();
			for (int num16 = 0; num16 < state.GoalChestRewards.Count; num16++)
			{
				Tuple<string, RewardGroup> tuple2 = state.GoalChestRewards[num16];
				string item5 = tuple2.Item1;
				RewardGroup rewardGroup2 = references.Get(tuple2.Item2);
				if (rewardGroup2 == null && tuple2.Item2 != null)
				{
					rewardGroup2 = new RewardGroup(tuple2.Item2, references);
					references.Add(tuple2.Item2, rewardGroup2);
				}
				Tuple<string, RewardGroup> item6 = new Tuple<string, RewardGroup>(item5, rewardGroup2);
				GoalChestRewards.Add(item6);
			}
			references.Add(state.GoalChestRewards, GoalChestRewards);
		}
		ScenarioRNGState = references.Get(state.ScenarioRNGState);
		if (ScenarioRNGState == null && state.ScenarioRNGState != null)
		{
			ScenarioRNGState = new Extensions.RandomState(state.ScenarioRNGState, references);
			references.Add(state.ScenarioRNGState, ScenarioRNGState);
		}
		m_ScenarioRNG = references.Get(state.m_ScenarioRNG);
		if (m_ScenarioRNG == null && state.m_ScenarioRNG != null)
		{
			m_ScenarioRNG = new SharedLibrary.Random(state.m_ScenarioRNG, references);
			references.Add(state.m_ScenarioRNG, m_ScenarioRNG);
		}
		EnemyIDRNGState = references.Get(state.EnemyIDRNGState);
		if (EnemyIDRNGState == null && state.EnemyIDRNGState != null)
		{
			EnemyIDRNGState = new Extensions.RandomState(state.EnemyIDRNGState, references);
			references.Add(state.EnemyIDRNGState, EnemyIDRNGState);
		}
		m_EnemyIDRNG = references.Get(state.m_EnemyIDRNG);
		if (m_EnemyIDRNG == null && state.m_EnemyIDRNG != null)
		{
			m_EnemyIDRNG = new SharedLibrary.Random(state.m_EnemyIDRNG, references);
			references.Add(state.m_EnemyIDRNG, m_EnemyIDRNG);
		}
		EnemyAbilityCardRNGState = references.Get(state.EnemyAbilityCardRNGState);
		if (EnemyAbilityCardRNGState == null && state.EnemyAbilityCardRNGState != null)
		{
			EnemyAbilityCardRNGState = new Extensions.RandomState(state.EnemyAbilityCardRNGState, references);
			references.Add(state.EnemyAbilityCardRNGState, EnemyAbilityCardRNGState);
		}
		m_EnemyAbilityCardRNG = references.Get(state.m_EnemyAbilityCardRNG);
		if (m_EnemyAbilityCardRNG == null && state.m_EnemyAbilityCardRNG != null)
		{
			m_EnemyAbilityCardRNG = new SharedLibrary.Random(state.m_EnemyAbilityCardRNG, references);
			references.Add(state.m_EnemyAbilityCardRNG, m_EnemyAbilityCardRNG);
		}
		GuidRNGState = references.Get(state.GuidRNGState);
		if (GuidRNGState == null && state.GuidRNGState != null)
		{
			GuidRNGState = new Extensions.RandomState(state.GuidRNGState, references);
			references.Add(state.GuidRNGState, GuidRNGState);
		}
		m_GuidRNG = references.Get(state.m_GuidRNG);
		if (m_GuidRNG == null && state.m_GuidRNG != null)
		{
			m_GuidRNG = new SharedLibrary.Random(state.m_GuidRNG, references);
			references.Add(state.m_GuidRNG, m_GuidRNG);
		}
		ScenarioEventLog = references.Get(state.ScenarioEventLog);
		if (ScenarioEventLog == null && state.ScenarioEventLog != null)
		{
			ScenarioEventLog = new SEventLog(state.ScenarioEventLog, references);
			references.Add(state.ScenarioEventLog, ScenarioEventLog);
		}
		StateNeedsUpdatesSaved = state.StateNeedsUpdatesSaved;
		LoadingScenarioState = state.LoadingScenarioState;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("Description", Description);
		info.AddValue("ID", ID);
		info.AddValue("Level", Level);
		info.AddValue("Width", Width);
		info.AddValue("Height", Height);
		info.AddValue("PositiveSpaceOffset", PositiveSpaceOffset);
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("Seed", Seed);
		info.AddValue("SeedFromMap", SeedFromMap);
		info.AddValue("IsFirstLoad", IsFirstLoad);
		info.AddValue("ScenarioType", ScenarioType);
		info.AddValue("ScenarioFileName", ScenarioFileName);
		info.AddValue("Style", Style);
		info.AddValue("ChestTreasureTables", ChestTreasureTables);
		info.AddValue("RewardsTreasureTables", RewardsTreasureTables);
		info.AddValue("ActiveBonusIDCounter", ActiveBonusIDCounter);
		info.AddValue("Stats", Stats);
		info.AddValue("RoundNumber", RoundNumber);
		info.AddValue("Maps", Maps);
		info.AddValue("MapsFailedToLoad", MapsFailedToLoad);
		info.AddValue("Players", Players);
		info.AddValue("Monsters", Monsters);
		info.AddValue("AllyMonsters", AllyMonsters);
		info.AddValue("Enemy2Monsters", Enemy2Monsters);
		info.AddValue("NeutralMonsterStates", NeutralMonsters);
		info.AddValue("HeroSummons", HeroSummons);
		info.AddValue("Objects", Objects);
		info.AddValue("EnemyClassManager", EnemyClassManager);
		info.AddValue("HeroSummonClassManager", HeroSummonClassManager);
		info.AddValue("Props", Props);
		info.AddValue("ActivatedProps", ActivatedProps);
		info.AddValue("TransparentProps", TransparentProps);
		info.AddValue("DestroyedProps", DestroyedProps);
		info.AddValue("Spawners", Spawners);
		info.AddValue("ElementColumn", ElementColumn);
		info.AddValue("WinObjectives", WinObjectives);
		info.AddValue("LoseObjectives", LoseObjectives);
		info.AddValue("ScenarioModifiers", ScenarioModifiers);
		info.AddValue("RoundChestRewards", RoundChestRewards);
		info.AddValue("GoalChestRewards", GoalChestRewards);
		info.AddValue("ScenarioRNGState", ScenarioRNGState);
		info.AddValue("EnemyIDRNGState", EnemyIDRNGState);
		info.AddValue("EnemyAbilityCardRNGState", EnemyAbilityCardRNGState);
		info.AddValue("GuidRNGState", GuidRNGState);
		info.AddValue("ScenarioEventLog", ScenarioEventLog);
	}

	public ScenarioState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "Description":
					Description = info.GetString("Description");
					break;
				case "ID":
					try
					{
						ID = info.GetString("ID");
					}
					catch
					{
						ID = info.GetInt32("ID").ToString();
					}
					break;
				case "Level":
					Level = info.GetInt32("Level");
					break;
				case "Width":
					Width = info.GetInt32("Width");
					break;
				case "Height":
					Height = info.GetInt32("Height");
					break;
				case "PositiveSpaceOffset":
					PositiveSpaceOffset = (CVectorInt3)info.GetValue("PositiveSpaceOffset", typeof(CVectorInt3));
					break;
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "Seed":
					Seed = info.GetInt32("Seed");
					break;
				case "SeedFromMap":
					SeedFromMap = (int?)info.GetValue("SeedFromMap", typeof(int?));
					break;
				case "IsFirstLoad":
					IsFirstLoad = info.GetBoolean("IsFirstLoad");
					break;
				case "ScenarioType":
					ScenarioType = (EScenarioType)info.GetValue("ScenarioType", typeof(EScenarioType));
					break;
				case "ScenarioFileName":
					ScenarioFileName = info.GetString("ScenarioFileName");
					break;
				case "Style":
					Style = (ApparanceStyle)info.GetValue("Style", typeof(ApparanceStyle));
					break;
				case "ChestTreasureTables":
					ChestTreasureTables = (List<string>)info.GetValue("ChestTreasureTables", typeof(List<string>));
					break;
				case "RewardsTreasureTables":
					RewardsTreasureTables = (List<string>)info.GetValue("RewardsTreasureTables", typeof(List<string>));
					break;
				case "ActiveBonusIDCounter":
					ActiveBonusIDCounter = info.GetInt32("ActiveBonusIDCounter");
					break;
				case "Stats":
					Stats = (StatsState)info.GetValue("Stats", typeof(StatsState));
					break;
				case "RoundNumber":
					RoundNumber = info.GetInt32("RoundNumber");
					if (RoundNumber < 1)
					{
						RoundNumber = 1;
					}
					break;
				case "Maps":
					Maps = (List<CMap>)info.GetValue("Maps", typeof(List<CMap>));
					break;
				case "MapsFailedToLoad":
					MapsFailedToLoad = (List<CMap>)info.GetValue("MapsFailedToLoad", typeof(List<CMap>));
					break;
				case "Players":
					Players = (List<PlayerState>)info.GetValue("Players", typeof(List<PlayerState>));
					break;
				case "Monsters":
					Monsters = (List<EnemyState>)info.GetValue("Monsters", typeof(List<EnemyState>));
					break;
				case "AllyMonsters":
					AllyMonsters = (List<EnemyState>)info.GetValue("AllyMonsters", typeof(List<EnemyState>));
					break;
				case "Enemy2Monsters":
					Enemy2Monsters = (List<EnemyState>)info.GetValue("Enemy2Monsters", typeof(List<EnemyState>));
					break;
				case "NeutralMonsterStates":
					NeutralMonsters = (List<EnemyState>)info.GetValue("NeutralMonsterStates", typeof(List<EnemyState>));
					break;
				case "NeutralMonsters":
					Enemy2Monsters = (List<EnemyState>)info.GetValue("NeutralMonsters", typeof(List<EnemyState>));
					break;
				case "HeroSummons":
					HeroSummons = (List<HeroSummonState>)info.GetValue("HeroSummons", typeof(List<HeroSummonState>));
					break;
				case "Objects":
					Objects = (List<ObjectState>)info.GetValue("Objects", typeof(List<ObjectState>));
					break;
				case "EnemyClassManager":
					EnemyClassManager = (EnemyClassManagerState)info.GetValue("EnemyClassManager", typeof(EnemyClassManagerState));
					break;
				case "HeroSummonClassManager":
					HeroSummonClassManager = (HeroSummonClassManagerState)info.GetValue("HeroSummonClassManager", typeof(HeroSummonClassManagerState));
					break;
				case "Props":
					Props = (List<CObjectProp>)info.GetValue("Props", typeof(List<CObjectProp>));
					break;
				case "ActivatedProps":
					ActivatedProps = (List<CObjectProp>)info.GetValue("ActivatedProps", typeof(List<CObjectProp>));
					break;
				case "TransparentProps":
					TransparentProps = (List<CObjectObstacle>)info.GetValue("TransparentProps", typeof(List<CObjectObstacle>));
					break;
				case "DestroyedProps":
					DestroyedProps = (List<CObjectProp>)info.GetValue("DestroyedProps", typeof(List<CObjectProp>));
					break;
				case "Spawners":
					Spawners = (List<CSpawner>)info.GetValue("Spawners", typeof(List<CSpawner>));
					break;
				case "ElementColumn":
					ElementColumn = (ElementInfusionBoardManager.EColumn[])info.GetValue("ElementColumn", typeof(ElementInfusionBoardManager.EColumn[]));
					break;
				case "Objectives":
				{
					List<CObjective> list = (List<CObjective>)info.GetValue("Objectives", typeof(List<CObjective>));
					if (list != null)
					{
						WinObjectives = list.Where((CObjective o) => o != null && o.Result == EObjectiveResult.Win).ToList();
						LoseObjectives = list.Where((CObjective o) => o != null && o.Result == EObjectiveResult.Lose).ToList();
					}
					break;
				}
				case "WinObjectives":
					WinObjectives = (List<CObjective>)info.GetValue("WinObjectives", typeof(List<CObjective>));
					break;
				case "LoseObjectives":
					LoseObjectives = (List<CObjective>)info.GetValue("LoseObjectives", typeof(List<CObjective>));
					break;
				case "ScenarioModifiers":
					try
					{
						ScenarioModifiers = (List<CScenarioModifier>)info.GetValue("ScenarioModifiers", typeof(List<CScenarioModifier>));
					}
					catch
					{
						DLLDebug.LogError(Name + ": Could not properly deserialize Scenario State: Scenario Modififers");
						ScenarioModifiers = new List<CScenarioModifier>();
					}
					break;
				case "RoundChestRewards":
					RoundChestRewards = (List<Tuple<string, RewardGroup>>)info.GetValue("RoundChestRewards", typeof(List<Tuple<string, RewardGroup>>));
					break;
				case "GoalChestRewards":
					GoalChestRewards = (List<Tuple<string, RewardGroup>>)info.GetValue("GoalChestRewards", typeof(List<Tuple<string, RewardGroup>>));
					break;
				case "ScenarioRNGState":
					try
					{
						ScenarioRNGState = (Extensions.RandomState)info.GetValue("ScenarioRNGState", typeof(Extensions.RandomState));
						m_ScenarioRNG = ScenarioRNGState.Restore();
					}
					catch (Exception ex)
					{
						DLLDebug.LogError(Name + ": Unable to restore RNG state.\n" + ex.Message + "\nResetting RNG.");
						m_ScenarioRNG = new SharedLibrary.Random(Seed);
						ScenarioRNGState = m_ScenarioRNG.Save();
						StateNeedsUpdatesSaved = true;
					}
					break;
				case "EnemyIDRNGState":
					try
					{
						EnemyIDRNGState = (Extensions.RandomState)info.GetValue("EnemyIDRNGState", typeof(Extensions.RandomState));
						m_EnemyIDRNG = EnemyIDRNGState.Restore();
						if (m_EnemyIDRNG == null || EnemyIDRNGState == null)
						{
							throw new NullReferenceException();
						}
					}
					catch
					{
						DLLDebug.LogWarning(Name + ": Unable to restore EnemyIDRNGState state (probably old save data). Resetting RNG.");
						m_EnemyIDRNG = new SharedLibrary.Random(Seed);
						EnemyIDRNGState = m_EnemyIDRNG.Save();
						StateNeedsUpdatesSaved = true;
					}
					break;
				case "EnemyAbilityCardRNGState":
					try
					{
						EnemyAbilityCardRNGState = (Extensions.RandomState)info.GetValue("EnemyAbilityCardRNGState", typeof(Extensions.RandomState));
						m_EnemyAbilityCardRNG = EnemyAbilityCardRNGState.Restore();
						if (m_EnemyAbilityCardRNG == null || EnemyAbilityCardRNGState == null)
						{
							throw new NullReferenceException();
						}
					}
					catch
					{
						DLLDebug.LogWarning(Name + ": Unable to restore EnemyAbilityCardRNGState state (probably old save data). Resetting RNG.");
						m_EnemyAbilityCardRNG = new SharedLibrary.Random(Seed);
						EnemyAbilityCardRNGState = m_EnemyAbilityCardRNG.Save();
						StateNeedsUpdatesSaved = true;
					}
					break;
				case "GuidRNGState":
					try
					{
						GuidRNGState = (Extensions.RandomState)info.GetValue("GuidRNGState", typeof(Extensions.RandomState));
						m_GuidRNG = GuidRNGState.Restore();
					}
					catch
					{
						DLLDebug.LogError(Name + ": Unable to restore Guid RNG state.  Resetting Guid RNG.");
						m_GuidRNG = new SharedLibrary.Random(Seed);
						GuidRNGState = m_GuidRNG.Save();
						StateNeedsUpdatesSaved = true;
					}
					break;
				case "ScenarioEventLog":
					try
					{
						ScenarioEventLog = (SEventLog)info.GetValue("ScenarioEventLog", typeof(SEventLog));
					}
					catch
					{
						DLLDebug.LogError(Name + ": Unable to find Scenario Event Log. Creating new Scenario Event Log");
						ScenarioEventLog = new SEventLog();
						StateNeedsUpdatesSaved = true;
					}
					break;
				}
			}
			catch (Exception ex2)
			{
				DLLDebug.LogError("Exception while trying to deserialize ScenarioState entry " + current.Name + "\n" + ex2.Message + "\n" + ex2.StackTrace);
				throw ex2;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (TransparentProps == null)
		{
			DLLDebug.Log(Name + ": Unable to find Transparent Props List. Creating new Transparent Props List");
			TransparentProps = new List<CObjectObstacle>();
			StateNeedsUpdatesSaved = true;
		}
		if (Spawners == null)
		{
			DLLDebug.Log(Name + ": Unable to find Spawners List. Creating new Spawners List");
			Spawners = new List<CSpawner>();
			StateNeedsUpdatesSaved = true;
		}
		if (ScenarioModifiers == null)
		{
			DLLDebug.Log(Name + ": Unable to find ScenarioModifiers List. Creating new Spawners List");
			ScenarioModifiers = new List<CScenarioModifier>();
			StateNeedsUpdatesSaved = true;
		}
		if (WinObjectives == null)
		{
			DLLDebug.Log(Name + ": Unable to find WinObjectives List. Creating new WinObjectives List");
			WinObjectives = new List<CObjective>();
		}
		if (LoseObjectives == null)
		{
			DLLDebug.Log(Name + ": Unable to find LoseObjectives List. Creating new LoseObjectives List");
			LoseObjectives = new List<CObjective>();
			StateNeedsUpdatesSaved = true;
		}
		if (m_GuidRNG == null)
		{
			m_GuidRNG = new SharedLibrary.Random(Seed);
			GuidRNGState = m_GuidRNG.Save();
			StateNeedsUpdatesSaved = true;
		}
		if (ScenarioEventLog == null)
		{
			DLLDebug.Log(Name + ": Unable to find Scenario Event Log. Creating new Scenario Event Log");
			ScenarioEventLog = new SEventLog();
			StateNeedsUpdatesSaved = true;
		}
		if (ScenarioType == EScenarioType.None)
		{
			DLLDebug.Log(Name + ": Unable to find ScenarioType. Setting to YML");
			ScenarioType = EScenarioType.YML;
			StateNeedsUpdatesSaved = true;
		}
		if (AllyMonsters == null)
		{
			DLLDebug.Log(Name + ": Unable to find AllyMonsters List. Creating new AllyMonsters List");
			AllyMonsters = new List<EnemyState>();
			StateNeedsUpdatesSaved = true;
		}
		if (Enemy2Monsters == null)
		{
			DLLDebug.Log(Name + ": Unable to find Enemy2Monsters List. Creating new Enemy2Monsters List");
			Enemy2Monsters = new List<EnemyState>();
			StateNeedsUpdatesSaved = true;
		}
		if (NeutralMonsters == null)
		{
			DLLDebug.Log(Name + ": Unable to find NeutralMonsters List. Creating new NeutralMonsters List");
			NeutralMonsters = new List<EnemyState>();
			StateNeedsUpdatesSaved = true;
		}
		if (RoundChestRewards == null)
		{
			DLLDebug.Log(Name + ": Unable to find RoundChestRewards List. Creating new RoundChestRewards List");
			RoundChestRewards = new List<Tuple<string, RewardGroup>>();
			StateNeedsUpdatesSaved = true;
		}
		if (GoalChestRewards == null)
		{
			DLLDebug.Log(Name + ": Unable to find GoalChestRewards List. Creating new GoalChestRewards List");
			GoalChestRewards = new List<Tuple<string, RewardGroup>>();
			StateNeedsUpdatesSaved = true;
		}
		if (Objects == null)
		{
			DLLDebug.Log(Name + ": Unable to find Objects List. Creating new Objects List");
			Objects = new List<ObjectState>();
			StateNeedsUpdatesSaved = true;
		}
		if (DestroyedProps == null)
		{
			DLLDebug.Log(Name + ": Unable to find Destroyed Props List. Creating new Destroyed Props List");
			DestroyedProps = new List<CObjectProp>();
			StateNeedsUpdatesSaved = true;
		}
		UpdateEnemyStateTypes(Monsters, CActor.EType.Enemy);
		UpdateEnemyStateTypes(AllyMonsters, CActor.EType.Ally);
		UpdateEnemyStateTypes(NeutralMonsters, CActor.EType.Neutral);
		UpdateEnemyStateTypes(Enemy2Monsters, CActor.EType.Enemy2);
		foreach (ObjectState @object in Objects)
		{
			if (@object.Type == CActor.EType.Player)
			{
				@object.Type = CActor.EType.Enemy;
				StateNeedsUpdatesSaved = true;
			}
		}
	}

	private void UpdateEnemyStateTypes(List<EnemyState> enemyStates, CActor.EType type)
	{
		if (enemyStates == null)
		{
			return;
		}
		foreach (EnemyState enemyState in enemyStates)
		{
			if (enemyState.Type != type)
			{
				enemyState.Type = type;
				StateNeedsUpdatesSaved = true;
			}
		}
	}

	public ScenarioState(string name, string description, string id, int seed, int level, EScenarioType scenarioType, string scenarioFileName, List<CObjective> winObjectives, List<CObjective> loseObjectives, List<CScenarioModifier> scenarioModifiers, ApparanceStyle style, List<string> chestTreasureTables, List<string> rewardsTreasureTables)
	{
		Name = name;
		Description = description;
		ID = id;
		Level = level;
		ScenarioType = scenarioType;
		ScenarioFileName = scenarioFileName;
		Seed = seed;
		WinObjectives = winObjectives;
		LoseObjectives = loseObjectives;
		ScenarioModifiers = scenarioModifiers;
		Style = style;
		ChestTreasureTables = chestTreasureTables;
		RewardsTreasureTables = rewardsTreasureTables;
		ActiveBonusIDCounter = 0;
		Stats = null;
		IsFirstLoad = true;
		RoundNumber = 1;
		ElementColumn = new ElementInfusionBoardManager.EColumn[Enum.GetNames(typeof(ElementInfusionBoardManager.EElement)).Length];
		IsInitialised = false;
		Maps = new List<CMap>();
		MapsFailedToLoad = new List<CMap>();
		Players = new List<PlayerState>();
		HeroSummons = new List<HeroSummonState>();
		Monsters = new List<EnemyState>();
		AllyMonsters = new List<EnemyState>();
		Enemy2Monsters = new List<EnemyState>();
		NeutralMonsters = new List<EnemyState>();
		Objects = new List<ObjectState>();
		Props = new List<CObjectProp>();
		ActivatedProps = new List<CObjectProp>();
		TransparentProps = new List<CObjectObstacle>();
		DestroyedProps = new List<CObjectProp>();
		Spawners = new List<CSpawner>();
		RoundChestRewards = new List<Tuple<string, RewardGroup>>();
		GoalChestRewards = new List<Tuple<string, RewardGroup>>();
		m_ScenarioRNG = new SharedLibrary.Random(seed);
		ScenarioRNGState = m_ScenarioRNG.Save();
		m_EnemyIDRNG = new SharedLibrary.Random(seed);
		EnemyIDRNGState = m_EnemyIDRNG.Save();
		m_EnemyAbilityCardRNG = new SharedLibrary.Random(seed);
		EnemyAbilityCardRNGState = m_EnemyAbilityCardRNG.Save();
		m_GuidRNG = new SharedLibrary.Random(seed);
		GuidRNGState = m_GuidRNG.Save();
		ScenarioEventLog = new SEventLog();
	}

	public ScenarioState()
	{
	}

	public ScenarioState Copy()
	{
		return new ScenarioState
		{
			Name = Name,
			Description = Description,
			ID = ID,
			Level = Level,
			ScenarioType = ScenarioType,
			ScenarioFileName = ScenarioFileName,
			Seed = Seed,
			WinObjectives = ((WinObjectives != null) ? WinObjectives.ToList() : new List<CObjective>()),
			LoseObjectives = ((LoseObjectives != null) ? LoseObjectives.ToList() : new List<CObjective>()),
			ScenarioModifiers = ((ScenarioModifiers != null) ? ScenarioModifiers.ToList() : new List<CScenarioModifier>()),
			Style = Style,
			ChestTreasureTables = ((ChestTreasureTables != null) ? ChestTreasureTables.ToList() : new List<string>()),
			RewardsTreasureTables = ((RewardsTreasureTables != null) ? RewardsTreasureTables.ToList() : new List<string>()),
			ActiveBonusIDCounter = 0,
			Stats = Stats,
			IsFirstLoad = IsFirstLoad,
			RoundNumber = RoundNumber,
			ElementColumn = ElementColumn.ToArray(),
			Width = Width,
			Height = Height,
			PositiveSpaceOffset = PositiveSpaceOffset,
			IsInitialised = IsInitialised,
			Maps = ((Maps != null) ? Maps.ToList() : new List<CMap>()),
			MapsFailedToLoad = ((MapsFailedToLoad != null) ? MapsFailedToLoad.ToList() : new List<CMap>()),
			Players = ((Players != null) ? Players.ToList() : new List<PlayerState>()),
			HeroSummons = ((HeroSummons != null) ? HeroSummons.ToList() : new List<HeroSummonState>()),
			Monsters = ((Monsters != null) ? Monsters.ToList() : new List<EnemyState>()),
			AllyMonsters = ((AllyMonsters != null) ? AllyMonsters.ToList() : new List<EnemyState>()),
			Enemy2Monsters = ((Enemy2Monsters != null) ? Enemy2Monsters.ToList() : new List<EnemyState>()),
			NeutralMonsters = ((NeutralMonsters != null) ? NeutralMonsters.ToList() : new List<EnemyState>()),
			Objects = ((Objects != null) ? Objects.ToList() : new List<ObjectState>()),
			Props = ((Props != null) ? Props.ToList() : new List<CObjectProp>()),
			ActivatedProps = ((ActivatedProps != null) ? ActivatedProps.ToList() : new List<CObjectProp>()),
			TransparentProps = ((TransparentProps != null) ? TransparentProps.ToList() : new List<CObjectObstacle>()),
			DestroyedProps = ((DestroyedProps != null) ? DestroyedProps.ToList() : new List<CObjectProp>()),
			Spawners = ((Spawners != null) ? Spawners.ToList() : new List<CSpawner>()),
			RoundChestRewards = ((RoundChestRewards != null) ? RoundChestRewards.ToList() : new List<Tuple<string, RewardGroup>>()),
			GoalChestRewards = ((GoalChestRewards != null) ? GoalChestRewards.ToList() : new List<Tuple<string, RewardGroup>>()),
			SeedFromMap = SeedFromMap,
			m_ScenarioRNG = ScenarioRNGState.Restore(),
			ScenarioRNGState = m_ScenarioRNG.Save(),
			m_EnemyIDRNG = EnemyIDRNGState.Restore(),
			EnemyIDRNGState = m_EnemyIDRNG.Save(),
			m_EnemyAbilityCardRNG = EnemyAbilityCardRNGState.Restore(),
			EnemyAbilityCardRNGState = m_EnemyAbilityCardRNG.Save(),
			m_GuidRNG = GuidRNGState.Restore(),
			GuidRNGState = m_GuidRNG.Save(),
			ScenarioEventLog = ScenarioEventLog,
			EnemyClassManager = ((EnemyClassManager != null) ? EnemyClassManager.Copy() : null),
			HeroSummonClassManager = ((HeroSummonClassManager != null) ? HeroSummonClassManager.Copy() : null)
		};
	}

	public void InitScenarioState(int width, int height, CVectorInt3 positiveSpaceOffset)
	{
		Width = width;
		Height = height;
		PositiveSpaceOffset = positiveSpaceOffset;
		IsInitialised = true;
	}

	public void RefreshHeroSummons()
	{
		foreach (CHeroSummonActor heroSummonActor in ScenarioManager.Scenario.HeroSummons)
		{
			if (HeroSummons.Any((HeroSummonState a) => a.ActorGuid == heroSummonActor.ActorGuid))
			{
				continue;
			}
			foreach (CMap map in Maps)
			{
				if (ScenarioManager.Tiles[heroSummonActor.ArrayIndex.X, heroSummonActor.ArrayIndex.Y].m_HexMap == map || ScenarioManager.Tiles[heroSummonActor.ArrayIndex.X, heroSummonActor.ArrayIndex.Y].m_Hex2Map == map)
				{
					HeroSummons.Add(new HeroSummonState(heroSummonActor, map.MapGuid));
					break;
				}
			}
		}
	}

	public void Update(bool saveHiddenUnits = false)
	{
		if (!saveHiddenUnits)
		{
			IsFirstLoad = false;
		}
		else
		{
			IsFirstLoad = true;
		}
		ScenarioRNGState = m_ScenarioRNG.Save();
		EnemyIDRNGState = m_EnemyIDRNG.Save();
		EnemyAbilityCardRNGState = m_EnemyAbilityCardRNG.Save();
		GuidRNGState = m_GuidRNG.Save();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.AllPlayers)
		{
			if (Players.Any((PlayerState a) => a.ActorGuid == playerActor.ActorGuid))
			{
				continue;
			}
			foreach (CMap map in Maps)
			{
				if (ScenarioManager.Tiles[playerActor.ArrayIndex.X, playerActor.ArrayIndex.Y].m_HexMap == map || ScenarioManager.Tiles[playerActor.ArrayIndex.X, playerActor.ArrayIndex.Y].m_Hex2Map == map)
				{
					Players.Add(new PlayerState(playerActor, map.MapGuid));
					break;
				}
			}
		}
		RefreshHeroSummons();
		foreach (CEnemyActor enemyActor in ScenarioManager.Scenario.Enemies)
		{
			if (Monsters.Any((EnemyState a) => a.ActorGuid == enemyActor.ActorGuid))
			{
				continue;
			}
			foreach (CMap map2 in Maps)
			{
				if (ScenarioManager.Tiles[enemyActor.ArrayIndex.X, enemyActor.ArrayIndex.Y].m_HexMap == map2 || ScenarioManager.Tiles[enemyActor.ArrayIndex.X, enemyActor.ArrayIndex.Y].m_Hex2Map == map2)
				{
					Monsters.Add(new EnemyState(enemyActor, map2.MapGuid));
					break;
				}
			}
		}
		foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
		{
			if (AllyMonsters.Any((EnemyState a) => a.ActorGuid == allyMonster.ActorGuid))
			{
				continue;
			}
			foreach (CMap map3 in Maps)
			{
				if (ScenarioManager.Tiles[allyMonster.ArrayIndex.X, allyMonster.ArrayIndex.Y].m_HexMap == map3 || ScenarioManager.Tiles[allyMonster.ArrayIndex.X, allyMonster.ArrayIndex.Y].m_Hex2Map == map3)
				{
					AllyMonsters.Add(new EnemyState(allyMonster, map3.MapGuid));
					break;
				}
			}
		}
		foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
		{
			if (Enemy2Monsters.Any((EnemyState a) => a.ActorGuid == enemy2Monster.ActorGuid))
			{
				continue;
			}
			foreach (CMap map4 in Maps)
			{
				if (ScenarioManager.Tiles[enemy2Monster.ArrayIndex.X, enemy2Monster.ArrayIndex.Y].m_HexMap == map4 || ScenarioManager.Tiles[enemy2Monster.ArrayIndex.X, enemy2Monster.ArrayIndex.Y].m_Hex2Map == map4)
				{
					Enemy2Monsters.Add(new EnemyState(enemy2Monster, map4.MapGuid));
					break;
				}
			}
		}
		foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
		{
			if (NeutralMonsters.Any((EnemyState a) => a.ActorGuid == neutralMonster.ActorGuid))
			{
				continue;
			}
			foreach (CMap map5 in Maps)
			{
				if (ScenarioManager.Tiles[neutralMonster.ArrayIndex.X, neutralMonster.ArrayIndex.Y].m_HexMap == map5 || ScenarioManager.Tiles[neutralMonster.ArrayIndex.X, neutralMonster.ArrayIndex.Y].m_Hex2Map == map5)
				{
					NeutralMonsters.Add(new EnemyState(neutralMonster, map5.MapGuid));
					break;
				}
			}
		}
		foreach (CObjectActor objectActor in ScenarioManager.Scenario.Objects)
		{
			if (Objects.Any((ObjectState a) => a.ActorGuid == objectActor.ActorGuid))
			{
				continue;
			}
			foreach (CMap map6 in Maps)
			{
				if (ScenarioManager.Tiles[objectActor.ArrayIndex.X, objectActor.ArrayIndex.Y].m_HexMap == map6 || ScenarioManager.Tiles[objectActor.ArrayIndex.X, objectActor.ArrayIndex.Y].m_Hex2Map == map6)
				{
					Objects.Add(new ObjectState(objectActor, map6.MapGuid));
					break;
				}
			}
		}
		foreach (CMap map7 in Maps)
		{
			map7.Save(saveHiddenUnits);
		}
		if (EnemyClassManager == null)
		{
			EnemyClassManager = new EnemyClassManagerState();
		}
		EnemyClassManager.Save();
		if (HeroSummonClassManager == null)
		{
			HeroSummonClassManager = new HeroSummonClassManagerState();
		}
		else
		{
			HeroSummonClassManager.Save();
		}
		ElementColumn = ElementInfusionBoardManager.GetElementColumn.ToArray();
		foreach (CScenarioModifier scenarioModifier in ScenarioModifiers)
		{
			scenarioModifier.SaveScenarioModifier();
		}
		ActiveBonusIDCounter = CActiveBonus.IDCounter;
	}

	public void ApplyClassStates()
	{
		if (EnemyClassManager != null)
		{
			EnemyClassManager.Load();
		}
		if (HeroSummonClassManager != null)
		{
			HeroSummonClassManager.Load();
		}
	}

	public void Apply()
	{
		foreach (CMap map in Maps)
		{
			map.Load();
		}
		ElementInfusionBoardManager.SetElementColumn(ElementColumn);
		foreach (CScenarioModifier scenarioModifier in ScenarioModifiers)
		{
			scenarioModifier.LoadScenarioModifier();
		}
		CActiveBonus.IDCounter = ActiveBonusIDCounter;
		CActiveBonus.RefreshAllAuraActiveBonuses();
		CActiveBonus.RefreshOverhealActiveBonuses();
	}

	public void SetScenarioLevel(int level)
	{
		Level = level;
	}

	public void UpdateMonsterClassesFromState(int playercount = 0, List<CStatBasedOnXOverrideDetails> statBasedOnXOverrides = null)
	{
		foreach (EnemyState monster in Monsters)
		{
			CMonsterClass monsterClass = MonsterClassManager.Find(monster.ClassID);
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass.ID))
			{
				additionalStatBasedOnXData = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass.ID
					select o.OverrideData).ToList();
			}
			monsterClass.SetMonsterStatLevel(monster.Level, playercount, additionalStatBasedOnXData);
			if (monsterClass.NonEliteVariant == null)
			{
				MonsterClassManager.FindEliteVariantOfClass(monsterClass.ID)?.SetMonsterStatLevel(monster.Level, playercount, additionalStatBasedOnXData);
			}
		}
		foreach (EnemyState allyMonster in AllyMonsters)
		{
			CMonsterClass monsterClass2 = MonsterClassManager.Find(allyMonster.ClassID);
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData2 = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass2.ID))
			{
				additionalStatBasedOnXData2 = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass2.ID
					select o.OverrideData).ToList();
			}
			monsterClass2.SetMonsterStatLevel(allyMonster.Level, playercount, additionalStatBasedOnXData2);
			if (monsterClass2.NonEliteVariant == null)
			{
				MonsterClassManager.FindEliteVariantOfClass(monsterClass2.ID)?.SetMonsterStatLevel(allyMonster.Level, playercount, additionalStatBasedOnXData2);
			}
		}
		foreach (EnemyState enemy2Monster in Enemy2Monsters)
		{
			CMonsterClass monsterClass3 = MonsterClassManager.Find(enemy2Monster.ClassID);
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData3 = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass3.ID))
			{
				additionalStatBasedOnXData3 = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass3.ID
					select o.OverrideData).ToList();
			}
			monsterClass3.SetMonsterStatLevel(enemy2Monster.Level, playercount, additionalStatBasedOnXData3);
			if (monsterClass3.NonEliteVariant == null)
			{
				MonsterClassManager.FindEliteVariantOfClass(monsterClass3.ID)?.SetMonsterStatLevel(enemy2Monster.Level, playercount, additionalStatBasedOnXData3);
			}
		}
		foreach (EnemyState neutralMonster in NeutralMonsters)
		{
			CMonsterClass monsterClass4 = MonsterClassManager.Find(neutralMonster.ClassID);
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData4 = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass4.ID))
			{
				additionalStatBasedOnXData4 = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass4.ID
					select o.OverrideData).ToList();
			}
			monsterClass4.SetMonsterStatLevel(neutralMonster.Level, playercount, additionalStatBasedOnXData4);
			if (monsterClass4.NonEliteVariant == null)
			{
				MonsterClassManager.FindEliteVariantOfClass(monsterClass4.ID)?.SetMonsterStatLevel(neutralMonster.Level, playercount, additionalStatBasedOnXData4);
			}
		}
		foreach (ObjectState @object in Objects)
		{
			CObjectClass objectClass = MonsterClassManager.FindObjectClass(@object.ClassID);
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData5 = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == objectClass.ID))
			{
				additionalStatBasedOnXData5 = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == objectClass.ID
					select o.OverrideData).ToList();
			}
			objectClass.SetMonsterStatLevel(@object.Level, playercount, additionalStatBasedOnXData5);
		}
	}

	public void OverridePlayersInState(List<PlayerState> playersToUse)
	{
		Players = playersToUse;
	}

	public void RestoreRNGStates()
	{
		SimpleLog.AddToSimpleLog("Restoring RNG states");
		m_ScenarioRNG = new SharedLibrary.Random(Seed);
		m_EnemyIDRNG = new SharedLibrary.Random(Seed);
		m_EnemyAbilityCardRNG = new SharedLibrary.Random(Seed);
		m_GuidRNG = new SharedLibrary.Random(Seed);
	}

	public void RandomiseRNGOnLoad()
	{
		SimpleLog.AddToSimpleLog("Randomising RNGs on load");
		Seed = (SeedFromMap.HasValue ? SeedFromMap.Value : SharedClient.GlobalRNG.Next());
		SimpleLog.AddToSimpleLog("SeedFromMap value set: " + SeedFromMap.HasValue);
		SimpleLog.AddToSimpleLog("Randomising Seed = " + Seed);
		m_ScenarioRNG = new SharedLibrary.Random(Seed);
		ScenarioRNGState = m_ScenarioRNG.Save();
		m_EnemyAbilityCardRNG = new SharedLibrary.Random(Seed);
		EnemyAbilityCardRNGState = m_EnemyAbilityCardRNG.Save();
		m_EnemyIDRNG = new SharedLibrary.Random(Seed);
		EnemyIDRNGState = m_EnemyIDRNG.Save();
		SimpleLog.AddToSimpleLog("RNG STATES (After Randomise RNG On Load): \nScenarioRNG:" + PeekScenarioRNG + "\nEnemyIDRNG:" + PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + PeekEnemyAbilityCardRNG + "\nGuidRNG:" + PeekGuidRNG);
	}

	public void RandomiseDecksOnLoad()
	{
		foreach (PlayerState player in Players)
		{
			player.Player.CharacterClass.ResetAttackModifierDeck();
		}
		EnemyClassManager = new EnemyClassManagerState();
		MonsterClassManager.ResetAttackModifiers();
		foreach (string item in AllEnemyStates.Select((EnemyState x) => x.ClassID).ToList().Distinct()
			.ToList())
		{
			MonsterClassManager.Find(item).ResetAbilityCards();
		}
	}

	public void ResetEnemyClassManager()
	{
		EnemyClassManager = new EnemyClassManagerState();
	}

	public void MapFailedToLoad(CMap map)
	{
		map.FailedToLoad = true;
		Maps.Remove(map);
		MapsFailedToLoad.Add(map);
		foreach (CMap map2 in Maps)
		{
			if (map2.Children.Contains(map.MapGuid))
			{
				map2.Children.Remove(map.MapGuid);
			}
		}
		foreach (CObjectProp item in Props.Where((CObjectProp w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			Props.Remove(item);
		}
		foreach (EnemyState item2 in Monsters.Where((EnemyState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			Monsters.Remove(item2);
		}
		foreach (PlayerState item3 in Players.Where((PlayerState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			Players.Remove(item3);
		}
		foreach (HeroSummonState item4 in HeroSummons.Where((HeroSummonState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			HeroSummons.Remove(item4);
		}
		foreach (EnemyState item5 in AllyMonsters.Where((EnemyState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			AllyMonsters.Remove(item5);
		}
		foreach (EnemyState item6 in Enemy2Monsters.Where((EnemyState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			Enemy2Monsters.Remove(item6);
		}
		foreach (EnemyState item7 in NeutralMonsters.Where((EnemyState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			NeutralMonsters.Remove(item7);
		}
		foreach (ObjectState item8 in Objects.Where((ObjectState w) => w.StartingMapGuid == map.MapGuid).ToList())
		{
			Objects.Remove(item8);
		}
	}

	public EObjectiveResult CheckObjectivesComplete(bool isEndOfRound = false, EObjectiveType specificObjectiveType = EObjectiveType.None)
	{
		List<string> list = new List<string>();
		IEnumerable<CObjective> enumerable;
		if (specificObjectiveType == EObjectiveType.None)
		{
			IEnumerable<CObjective> winObjectives = WinObjectives;
			enumerable = winObjectives;
		}
		else
		{
			enumerable = WinObjectives.Where((CObjective x) => x.ObjectiveType == specificObjectiveType);
		}
		foreach (CObjective item in enumerable)
		{
			if (item.IsActive)
			{
				bool isComplete = item.IsComplete;
				item.CheckObjectiveComplete(Players.Count, isEndOfRound);
				if (item.IsComplete && item.IsComplete != isComplete && !string.IsNullOrEmpty(item.EventIdentifier))
				{
					list.Add(item.EventIdentifier);
				}
			}
		}
		IEnumerable<CObjective> enumerable2;
		if (specificObjectiveType == EObjectiveType.None)
		{
			IEnumerable<CObjective> winObjectives = LoseObjectives;
			enumerable2 = winObjectives;
		}
		else
		{
			enumerable2 = LoseObjectives.Where((CObjective x) => x.ObjectiveType == specificObjectiveType);
		}
		foreach (CObjective item2 in enumerable2)
		{
			if (item2.IsActive)
			{
				bool isComplete2 = item2.IsComplete;
				item2.CheckObjectiveComplete(Players.Count, isEndOfRound);
				if (item2.IsComplete && item2.IsComplete != isComplete2 && !string.IsNullOrEmpty(item2.EventIdentifier))
				{
					list.Add(item2.EventIdentifier);
				}
			}
		}
		CUpdateObjectiveProgress_MessageData message = new CUpdateObjectiveProgress_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		if (list != null && list.Count > 0)
		{
			CTriggeredObjectivesEventIdList_MessageData message2 = new CTriggeredObjectivesEventIdList_MessageData
			{
				m_TriggeredObjectiveEventIdList = list.ToList()
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		if (LoseObjectives.Count > 0 && LoseObjectives.Any((CObjective a) => a.ObjectiveType != EObjectiveType.ReachRound && a.IsComplete && a.IsActive))
		{
			return EObjectiveResult.Lose;
		}
		if (WinObjectives.Count > 0 && WinObjectives.Any((CObjective a) => a.EnoughToWinAlone && a.IsComplete))
		{
			return EObjectiveResult.Win;
		}
		if (WinObjectives.Count > 0 && WinObjectives.Any((CObjective a) => !a.IsOptional && a.UnableToComplete))
		{
			return EObjectiveResult.Lose;
		}
		if (WinObjectives.Count > 0 && WinObjectives.All((CObjective a) => a.IsOptional || (a.IsComplete && !a.IsActive) || (a.IsComplete && a.IsActive)))
		{
			return EObjectiveResult.Win;
		}
		if (LoseObjectives.Count > 0 && LoseObjectives.Any((CObjective a) => a.ObjectiveType == EObjectiveType.ReachRound && a.IsComplete && a.IsActive))
		{
			return EObjectiveResult.Lose;
		}
		return EObjectiveResult.None;
	}

	public List<string> GetAllObjectivesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (CObjective winObjective in WinObjectives)
		{
			if (winObjective.GetObjectiveCompletionValue(Players.Count) > 0)
			{
				list.Add("Win_" + winObjective.ObjectiveType.ToString() + "_" + winObjective.GetObjectiveCompletionValue(Players.Count));
			}
			else
			{
				list.Add("Win_" + winObjective.ObjectiveType);
			}
		}
		foreach (CObjective loseObjective in LoseObjectives)
		{
			if (loseObjective.GetObjectiveCompletionValue(Players.Count) > 0)
			{
				list.Add("Lose_" + loseObjective.ObjectiveType.ToString() + "_" + loseObjective.GetObjectiveCompletionValue(Players.Count));
			}
			else
			{
				list.Add("Lose_" + loseObjective.ObjectiveType);
			}
		}
		return list;
	}

	public List<string> GetAllCompleteObjectivesForAnalytics()
	{
		List<string> list = new List<string>();
		foreach (CObjective winObjective in WinObjectives)
		{
			if (winObjective.IsComplete)
			{
				if (winObjective.GetObjectiveCompletionValue(Players.Count) > 0)
				{
					list.Add(winObjective.ObjectiveType.ToString() + "_" + winObjective.GetObjectiveCompletionValue(Players.Count));
				}
				else
				{
					list.Add(winObjective.ObjectiveType.ToString());
				}
			}
		}
		foreach (CObjective loseObjective in LoseObjectives)
		{
			if (loseObjective.IsComplete)
			{
				if (loseObjective.GetObjectiveCompletionValue(Players.Count) > 0)
				{
					list.Add(loseObjective.ObjectiveType.ToString() + "_" + loseObjective.GetObjectiveCompletionValue(Players.Count));
				}
				else
				{
					list.Add(loseObjective.ObjectiveType.ToString());
				}
			}
		}
		return list;
	}

	public List<string> GetCharacterStatuses()
	{
		List<string> list = new List<string>();
		foreach (PlayerState character in Players)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor p) => p.CharacterClass.ID == character.ClassID);
			if (cPlayerActor == null)
			{
				list.Add(character.ClassID + "_dead");
			}
			else if (cPlayerActor.CharacterClass.HandAbilityCards.Count < 2)
			{
				list.Add(character.ClassID + "_exhausted");
			}
			else
			{
				list.Add(character.ClassID + "_alive");
			}
		}
		return list;
	}

	public bool AreAllCharactersExhausted()
	{
		return ScenarioManager.Scenario.PlayerActors.Select((CPlayerActor p) => p != null && p.CharacterClass.HandAbilityCards.Count < 2).Count() == 0;
	}

	public List<List<string>> GetAllPlayersSelectedAbilityCards()
	{
		List<List<string>> list = new List<List<string>>();
		foreach (PlayerState player in Players)
		{
			List<string> list2 = new List<string>();
			if (player.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs != null)
			{
				foreach (Tuple<int, int> selectedCard in player.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs)
				{
					CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.SingleOrDefault((CAbilityCard s) => s.ID == selectedCard.Item1);
					if (cAbilityCard != null)
					{
						list2.Add(cAbilityCard.Name);
					}
				}
			}
			list.Add(list2);
		}
		return list;
	}

	public List<List<string>> GetAllEquippedItemsForSelectedCharacters()
	{
		List<List<string>> list = new List<List<string>>();
		foreach (PlayerState player in Players)
		{
			List<string> list2 = new List<string>();
			foreach (CItem item in player.Items)
			{
				list2.Add(item.Name);
			}
			list.Add(list2);
		}
		return list;
	}

	public Guid GetGUIDBasedOnGuidRNGState()
	{
		byte[] array = new byte[16];
		GuidRNG.NextBytes(array);
		return new Guid(array);
	}

	public CActor.EType GetActorType(ActorState stateToCheck)
	{
		if (stateToCheck == null)
		{
			return CActor.EType.Unknown;
		}
		if (Players.Any((PlayerState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.Player;
		}
		if (Monsters.Any((EnemyState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.Enemy;
		}
		if (AllyMonsters.Any((EnemyState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.Ally;
		}
		if (Enemy2Monsters.Any((EnemyState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.Enemy2;
		}
		if (NeutralMonsters.Any((EnemyState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.Neutral;
		}
		if (HeroSummons.Any((HeroSummonState a) => a.ActorGuid == stateToCheck.ActorGuid))
		{
			return CActor.EType.HeroSummon;
		}
		return CActor.EType.Unknown;
	}

	public static List<Tuple<int, string>> CompareStates(ScenarioState state1, ScenarioState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.Name != state2.Name)
			{
				LogMismatch(list, isMPCompare, 101, "State Name does not match.", new List<string[]> { new string[3] { "Name", state1.Name, state2.Name } });
			}
			if (state1.Description != state2.Description)
			{
				LogMismatch(list, isMPCompare, 102, "State Description does not match.", new List<string[]> { new string[3] { "Description", state1.Description, state2.Description } });
			}
			if (state1.ID != state2.ID)
			{
				LogMismatch(list, isMPCompare, 103, "State ID does not match.", new List<string[]> { new string[3] { "ID", state1.ID, state2.ID } });
			}
			if (state1.Level != state2.Level)
			{
				LogMismatch(list, isMPCompare, 104, "State Level does not match.", new List<string[]> { new string[3]
				{
					"Level",
					state1.Level.ToString(),
					state2.Level.ToString()
				} });
			}
			if (state1.Width != state2.Width)
			{
				LogMismatch(list, isMPCompare, 105, "State Width does not match.", new List<string[]> { new string[3]
				{
					"Width",
					state1.Width.ToString(),
					state2.Width.ToString()
				} });
			}
			if (state1.Height != state2.Height)
			{
				LogMismatch(list, isMPCompare, 106, "State Height does not match.", new List<string[]> { new string[3]
				{
					"Height",
					state1.Height.ToString(),
					state2.Height.ToString()
				} });
			}
			if (!CVectorInt3.Compare(state1.PositiveSpaceOffset, state2.PositiveSpaceOffset))
			{
				LogMismatch(list, isMPCompare, 107, "State PositiveSpaceOffset does not match.", new List<string[]> { new string[3]
				{
					"PositiveSpaceOffset",
					state1.PositiveSpaceOffset.ToString(),
					state2.PositiveSpaceOffset.ToString()
				} });
			}
			if (state1.Seed != state2.Seed)
			{
				LogMismatch(list, isMPCompare, 108, "State Seed does not match.", new List<string[]> { new string[3]
				{
					"Seed",
					state1.Seed.ToString(),
					state2.Seed.ToString()
				} });
			}
			if (state1.ScenarioType != state2.ScenarioType)
			{
				LogMismatch(list, isMPCompare, 109, "State ScenarioType does not match.", new List<string[]> { new string[3]
				{
					"ScenarioType",
					state1.ScenarioType.ToString(),
					state2.ScenarioType.ToString()
				} });
			}
			string fileName = Path.GetFileName(state1.ScenarioFileName);
			string fileName2 = Path.GetFileName(state2.ScenarioFileName);
			if (state1.ScenarioFileName != state2.ScenarioFileName && fileName != fileName2)
			{
				LogMismatch(list, isMPCompare, 110, "State ScenarioFileName does not match.", new List<string[]>
				{
					new string[3] { "ScenarioFileName", state1.ScenarioFileName, state2.ScenarioFileName },
					new string[3] { "ScenarioFileNameShort", fileName, fileName2 }
				});
			}
			switch (StateShared.CheckNullsMatch(state1.ChestTreasureTables, state2.ChestTreasureTables))
			{
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 111, "ChestTreasureTables List null states do not match.", new List<string[]> { new string[3]
				{
					"ChestTreasureTables",
					(state1.ChestTreasureTables == null) ? "is null" : "is not null",
					(state2.ChestTreasureTables == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.ChestTreasureTables.Count != state2.ChestTreasureTables.Count)
				{
					LogMismatch(list, isMPCompare, 111, "State ChestTreasureTables Count does not match.", new List<string[]> { new string[3]
					{
						"ChestTreasureTables Count",
						state1.ChestTreasureTables.Count.ToString(),
						state2.ChestTreasureTables.Count.ToString()
					} });
					break;
				}
				foreach (string tt in state1.ChestTreasureTables.Concat(state2.ChestTreasureTables).Distinct())
				{
					if (state1.ChestTreasureTables.Where((string w) => w == tt).Count() != state2.ChestTreasureTables.Where((string w) => w == tt).Count())
					{
						LogMismatch(list, isMPCompare, 112, "State ChestTreasureTables Contents do not match.", new List<string[]>
						{
							new string[3] { "ChestTreasureTable", tt, tt },
							new string[3]
							{
								"Count",
								state1.ChestTreasureTables.Where((string w) => w == tt).Count().ToString(),
								state2.ChestTreasureTables.Where((string w) => w == tt).Count().ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.RewardsTreasureTables, state2.RewardsTreasureTables))
			{
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 113, "RewardsTreasureTables List is null states do not match.", new List<string[]> { new string[3]
				{
					"RewardsTreasureTables",
					(state1.RewardsTreasureTables == null) ? "is null" : "is not null",
					(state2.RewardsTreasureTables == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.RewardsTreasureTables.Count != state2.RewardsTreasureTables.Count)
				{
					LogMismatch(list, isMPCompare, 113, "State RewardsTreasureTables Count does not match.", new List<string[]> { new string[3]
					{
						"RewardsTreasureTables Count",
						state1.RewardsTreasureTables.Count.ToString(),
						state2.RewardsTreasureTables.Count.ToString()
					} });
					break;
				}
				foreach (string tt2 in state1.RewardsTreasureTables.Concat(state2.RewardsTreasureTables).Distinct())
				{
					if (state1.RewardsTreasureTables.Where((string w) => w == tt2).Count() != state2.RewardsTreasureTables.Where((string w) => w == tt2).Count())
					{
						LogMismatch(list, isMPCompare, 114, "State RewardsTreasureTables Contents do not match.", new List<string[]>
						{
							new string[3] { "RewardsTreasureTables", tt2, tt2 },
							new string[3]
							{
								"Count",
								state1.RewardsTreasureTables.Where((string w) => w == tt2).Count().ToString(),
								state2.RewardsTreasureTables.Where((string w) => w == tt2).Count().ToString()
							}
						});
					}
				}
				break;
			}
			if (state1.ActiveBonusIDCounter != state2.ActiveBonusIDCounter)
			{
				LogMismatch(list, isMPCompare, 115, "State ActiveBonusIDCounter does not match.", new List<string[]> { new string[3]
				{
					"ActiveBonusIDCounter",
					state1.ActiveBonusIDCounter.ToString(),
					state2.ActiveBonusIDCounter.ToString()
				} });
			}
			if (state1.RoundNumber != state2.RoundNumber)
			{
				LogMismatch(list, isMPCompare, 116, "Round Number does not match.", new List<string[]> { new string[3]
				{
					"Round Number",
					state1.RoundNumber.ToString(),
					state2.RoundNumber.ToString()
				} });
			}
			if (state1.ElementColumn.Length != state2.ElementColumn.Length)
			{
				LogMismatch(list, isMPCompare, 117, "Element Column Count does not match.", new List<string[]>
				{
					new string[3]
					{
						"Element Column Count",
						state1.ElementColumn.Length.ToString(),
						state2.ElementColumn.Length.ToString()
					},
					new string[3]
					{
						"Element Column Values",
						string.Join(", ", state1.ElementColumn),
						string.Join(", ", state2.ElementColumn)
					}
				});
			}
			else
			{
				for (int num = 0; num < state1.ElementColumn.Length; num++)
				{
					if (state1.ElementColumn[num] != state2.ElementColumn[num])
					{
						LogMismatch(list, isMPCompare, 118, "Element Column value mismatch.", new List<string[]>
						{
							new string[3]
							{
								"Element Column Index",
								num.ToString(),
								num.ToString()
							},
							new string[3]
							{
								"Element Column Value",
								state1.ElementColumn[num].ToString(),
								state2.ElementColumn[num].ToString()
							}
						});
					}
				}
			}
			if (state1.ScenarioRNGState.Restore().Next() != state2.ScenarioRNGState.Restore().Next())
			{
				LogMismatch(list, isMPCompare, 119, "ScenarioRNGState does not match.", new List<string[]> { new string[3]
				{
					"ScenarioRNGState",
					state1.ScenarioRNGState.Restore().Next().ToString(),
					state2.ScenarioRNGState.Restore().Next().ToString()
				} });
			}
			if (state1.EnemyIDRNGState.Restore().Next() != state2.EnemyIDRNGState.Restore().Next())
			{
				LogMismatch(list, isMPCompare, 120, "EnemyIDRNGState does not match.", new List<string[]> { new string[3]
				{
					"EnemyIDRNGState",
					state1.EnemyIDRNGState.Restore().Next().ToString(),
					state2.EnemyIDRNGState.Restore().Next().ToString()
				} });
			}
			if (state1.EnemyAbilityCardRNGState.Restore().Next() != state2.EnemyAbilityCardRNGState.Restore().Next())
			{
				LogMismatch(list, isMPCompare, 121, "EnemyAbilityCardRNGState does not match.", new List<string[]> { new string[3]
				{
					"EnemyAbilityCardRNGState",
					state1.EnemyAbilityCardRNGState.Restore().Next().ToString(),
					state2.EnemyAbilityCardRNGState.Restore().Next().ToString()
				} });
			}
			if (state1.GuidRNGState.Restore().Next() != state2.GuidRNGState.Restore().Next())
			{
				LogMismatch(list, isMPCompare, 122, "GuidRNGState does not match.", new List<string[]> { new string[3]
				{
					"GuidRNGState",
					state1.GuidRNGState.Restore().Next().ToString(),
					state2.GuidRNGState.Restore().Next().ToString()
				} });
			}
			list.AddRange(ApparanceStyle.Compare(state1.Style, state2.Style, isMPCompare));
			switch (StateShared.CheckNullsMatch(state1.Maps, state2.Maps))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 123, "Maps List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Maps",
					(state1.Maps == null) ? "is null" : "is not null",
					(state2.Maps == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Maps.Any((CMap a) => a == null) || state2.Maps.Any((CMap a) => a == null))
				{
					LogMismatch(list, isMPCompare, 124, "Scenario State contains null maps.", new List<string[]> { new string[3]
					{
						"Maps",
						string.Join(", ", state1.Maps.Select((CMap s) => (s == null) ? "null" : s.MapType.ToString())),
						string.Join(", ", state2.Maps.Select((CMap s) => (s == null) ? "null" : s.MapType.ToString()))
					} });
					break;
				}
				bool flag = false;
				foreach (CMap map1 in state1.Maps)
				{
					if (state1.Maps.Where((CMap w) => w.MapGuid == map1.MapGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 125, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state Maps list.", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, "NA" },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								"NA"
							},
							new string[3] { "RoomName", map1.RoomName, "NA" },
							new string[3]
							{
								"Duplicate Count",
								state1.Maps.Where((CMap w) => w.MapGuid == map1.MapGuid).Count().ToString(),
								"NA"
							}
						});
						flag = true;
					}
				}
				foreach (CMap map2 in state2.Maps)
				{
					if (state2.Maps.Where((CMap w) => w.MapGuid == map2.MapGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 125, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state Maps list.", new List<string[]>
						{
							new string[3] { "Map GUID", "NA", map2.MapGuid },
							new string[3]
							{
								"Map Type",
								"NA",
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", "NA", map2.RoomName },
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.Maps.Where((CMap w) => w.MapGuid == map2.MapGuid).Count().ToString()
							}
						});
						flag = true;
					}
				}
				if (state1.Maps.Count != state2.Maps.Count)
				{
					LogMismatch(list, isMPCompare, 126, "Number of maps does not match.", new List<string[]>
					{
						new string[3]
						{
							"Maps Count",
							state1.Maps.Count.ToString(),
							state2.Maps.Count.ToString()
						},
						new string[3]
						{
							"Maps",
							string.Join(", ", state1.Maps.Select((CMap s) => s.MapType.ToString() + ", " + s.RoomName)),
							string.Join(", ", state2.Maps.Select((CMap s) => s.MapType.ToString() + ", " + s.RoomName))
						}
					});
				}
				else
				{
					if (flag)
					{
						break;
					}
					bool flag2 = false;
					foreach (CMap map3 in state1.Maps)
					{
						if (!state2.Maps.Exists((CMap e) => e.MapGuid == map3.MapGuid))
						{
							LogMismatch(list, isMPCompare, 127, "Map in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Map GUID", map3.MapGuid, "NA" },
								new string[3]
								{
									"Map Type",
									map3.MapType.ToString(),
									"NA"
								},
								new string[3] { "RoomName", map3.RoomName, "NA" }
							});
							flag2 = true;
						}
					}
					foreach (CMap map4 in state2.Maps)
					{
						if (!state1.Maps.Exists((CMap e) => e.MapGuid == map4.MapGuid))
						{
							LogMismatch(list, isMPCompare, 127, "Map in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Map GUID", "NA", map4.MapGuid },
								new string[3]
								{
									"Map Type",
									"NA",
									map4.MapType.ToString()
								},
								new string[3] { "RoomName", "NA", map4.RoomName }
							});
							flag2 = true;
						}
					}
					if (flag2)
					{
						break;
					}
					foreach (CMap map5 in state1.Maps)
					{
						try
						{
							CMap map6 = state2.Maps.Single((CMap s) => s.MapGuid == map5.MapGuid);
							list.AddRange(CMap.Compare(map5, map6, isMPCompare));
						}
						catch (Exception ex)
						{
							list.Add(new Tuple<int, string>(128, "Exception during map compare.\n" + ex.Message + "\n" + ex.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.Players, state2.Players))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 129, "Players List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Players",
					(state1.Players == null) ? "is null" : "is not null",
					(state2.Players == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Players.Any((PlayerState a) => a == null) || state2.Players.Any((PlayerState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 130, "Scenario State contains null Players.", new List<string[]> { new string[3]
					{
						"Players",
						string.Join(", ", state1.Players.Select((PlayerState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.Players.Select((PlayerState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag3 = false;
				foreach (PlayerState player1 in state1.Players.Where((PlayerState w) => w.ActorGuid != null))
				{
					if (state1.Players.Where((PlayerState w) => w.ActorGuid == player1.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 131, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state Players list.", new List<string[]>
						{
							new string[3] { "Player GUID", player1.ActorGuid, "NA" },
							new string[3]
							{
								"Player ID",
								player1.Player?.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", player1.ClassID, "NA" },
							new string[3]
							{
								"Location",
								player1.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.Players.Where((PlayerState w) => w.ActorGuid == player1.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag3 = true;
					}
				}
				foreach (PlayerState player2 in state2.Players.Where((PlayerState w) => w.ActorGuid != null))
				{
					if (state2.Players.Where((PlayerState w) => w.ActorGuid == player2.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 131, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state Players list.", new List<string[]>
						{
							new string[3] { "Player GUID", "NA", player2.ActorGuid },
							new string[3]
							{
								"Player ID",
								"NA",
								player2.Player?.ID.ToString()
							},
							new string[3] { "Class ID", "NA", player2.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								player2.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.Players.Where((PlayerState w) => w.ActorGuid == player2.ActorGuid).Count().ToString()
							}
						});
						flag3 = true;
					}
				}
				if (state1.Players.Count != state2.Players.Count)
				{
					LogMismatch(list, isMPCompare, 132, "Number of Players does not match.", new List<string[]>
					{
						new string[3]
						{
							"Players Count",
							state1.Players.Count.ToString(),
							state2.Players.Count.ToString()
						},
						new string[3]
						{
							"Players",
							string.Join(", ", state1.Players.Select((PlayerState s) => s.ClassID)),
							string.Join(", ", state2.Players.Select((PlayerState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag3)
					{
						break;
					}
					bool flag4 = false;
					foreach (PlayerState player3 in state1.Players)
					{
						if (!state2.Players.Exists((PlayerState e) => e.ActorGuid == player3.ActorGuid && TileIndex.Compare(e.Location, player3.Location)))
						{
							LogMismatch(list, isMPCompare, 133, "Player in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Player GUID", player3.ActorGuid, "Missing" },
								new string[3]
								{
									"Player ID",
									player3.Player?.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", player3.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									player3.Location.ToString(),
									"Missing"
								}
							});
							flag4 = true;
						}
					}
					foreach (PlayerState player4 in state2.Players)
					{
						if (!state1.Players.Exists((PlayerState e) => e.ActorGuid == player4.ActorGuid && TileIndex.Compare(e.Location, player4.Location)))
						{
							LogMismatch(list, isMPCompare, 133, "Player in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Player GUID", "Missing", player4.ActorGuid },
								new string[3]
								{
									"Player ID",
									"Missing",
									player4.Player?.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", player4.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									player4.Location.ToString()
								}
							});
							flag4 = true;
						}
					}
					if (flag4)
					{
						break;
					}
					foreach (PlayerState player5 in state1.Players)
					{
						try
						{
							PlayerState state3 = state2.Players.Single((PlayerState s) => s.ActorGuid == player5.ActorGuid && TileIndex.Compare(s.Location, player5.Location));
							list.AddRange(PlayerState.Compare(player5, state3, isMPCompare));
						}
						catch (Exception ex2)
						{
							list.Add(new Tuple<int, string>(134, "Exception during player compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.Monsters, state2.Monsters))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 135, "Monsters List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Monsters",
					(state1.Monsters == null) ? "is null" : "is not null",
					(state2.Monsters == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Monsters.Any((EnemyState a) => a == null) || state2.Monsters.Any((EnemyState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 136, "Scenario State contains null Monsters.", new List<string[]> { new string[3]
					{
						"Monsters",
						string.Join(", ", state1.Monsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.Monsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag5 = false;
				foreach (EnemyState monster1 in state1.Monsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state1.Monsters.Where((EnemyState w) => w.ActorGuid == monster1.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 137, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state Monsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", monster1.ActorGuid, "NA" },
							new string[3]
							{
								"Standee ID",
								monster1.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", monster1.ClassID, "NA" },
							new string[3]
							{
								"Location",
								monster1.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.Monsters.Where((EnemyState w) => w.ActorGuid == monster1.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag5 = true;
					}
				}
				foreach (EnemyState monster2 in state2.Monsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state2.Monsters.Where((EnemyState w) => w.ActorGuid == monster2.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 137, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state Monsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", "NA", monster2.ActorGuid },
							new string[3]
							{
								"Standee ID",
								"NA",
								monster2.ID.ToString()
							},
							new string[3] { "Class ID", "NA", monster2.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								monster2.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.Monsters.Where((EnemyState w) => w.ActorGuid == monster2.ActorGuid).Count().ToString()
							}
						});
						flag5 = true;
					}
				}
				if (state1.Monsters.Count != state2.Monsters.Count)
				{
					LogMismatch(list, isMPCompare, 138, "Number of Monsters does not match.", new List<string[]>
					{
						new string[3]
						{
							"Monsters Count",
							state1.Monsters.Count.ToString(),
							state2.Monsters.Count.ToString()
						},
						new string[3]
						{
							"Monsters",
							string.Join(", ", state1.Monsters.Select((EnemyState s) => s.ClassID)),
							string.Join(", ", state2.Monsters.Select((EnemyState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag5)
					{
						break;
					}
					bool flag6 = false;
					foreach (EnemyState monster3 in state1.Monsters)
					{
						if (!state2.Monsters.Exists((EnemyState e) => e.ActorGuid == monster3.ActorGuid && TileIndex.Compare(e.Location, monster3.Location)))
						{
							LogMismatch(list, isMPCompare, 139, "Enemy in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Enemy GUID", monster3.ActorGuid, "Missing" },
								new string[3]
								{
									"Standee ID",
									monster3.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", monster3.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									monster3.Location.ToString(),
									"Missing"
								}
							});
							flag6 = true;
						}
					}
					foreach (EnemyState monster4 in state2.Monsters)
					{
						if (!state1.Monsters.Exists((EnemyState e) => e.ActorGuid == monster4.ActorGuid && TileIndex.Compare(e.Location, monster4.Location)))
						{
							LogMismatch(list, isMPCompare, 139, "Enemy in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Enemy GUID", "Missing", monster4.ActorGuid },
								new string[3]
								{
									"Standee ID",
									"Missing",
									monster4.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", monster4.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									monster4.Location.ToString()
								}
							});
							flag6 = true;
						}
					}
					if (flag6)
					{
						break;
					}
					foreach (EnemyState monster5 in state1.Monsters)
					{
						try
						{
							EnemyState state4 = state2.Monsters.Single((EnemyState s) => s.ActorGuid == monster5.ActorGuid && TileIndex.Compare(s.Location, monster5.Location));
							list.AddRange(EnemyState.Compare(monster5, state4, isMPCompare));
						}
						catch (Exception ex3)
						{
							list.Add(new Tuple<int, string>(140, "Exception during Enemy compare.\n" + ex3.Message + "\n" + ex3.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.HeroSummons, state2.HeroSummons))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 141, "HeroSummons List is null on one or both states.", new List<string[]> { new string[3]
				{
					"HeroSummons",
					(state1.HeroSummons == null) ? "is null" : "is not null",
					(state2.HeroSummons == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.HeroSummons.Any((HeroSummonState a) => a == null) || state2.HeroSummons.Any((HeroSummonState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 142, "Scenario State contains null HeroSummons.", new List<string[]> { new string[3]
					{
						"HeroSummons",
						string.Join(", ", state1.HeroSummons.Select((HeroSummonState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.HeroSummons.Select((HeroSummonState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag7 = false;
				foreach (HeroSummonState summon1 in state1.HeroSummons.Where((HeroSummonState w) => w.ActorGuid != null))
				{
					if (state1.HeroSummons.Where((HeroSummonState w) => w.ActorGuid == summon1.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 143, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state HeroSummons list.", new List<string[]>
						{
							new string[3] { "HeroSummon GUID", summon1.ActorGuid, "NA" },
							new string[3]
							{
								"HeroSummon ID",
								summon1.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", summon1.ClassID, "NA" },
							new string[3]
							{
								"Location",
								summon1.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.HeroSummons.Where((HeroSummonState w) => w.ActorGuid == summon1.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag7 = true;
					}
				}
				foreach (HeroSummonState summon2 in state2.HeroSummons.Where((HeroSummonState w) => w.ActorGuid != null))
				{
					if (state2.HeroSummons.Where((HeroSummonState w) => w.ActorGuid == summon2.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 143, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state HeroSummons list.", new List<string[]>
						{
							new string[3] { "HeroSummon GUID", "NA", summon2.ActorGuid },
							new string[3]
							{
								"HeroSummon ID",
								"NA",
								summon2.ID.ToString()
							},
							new string[3] { "Class ID", "NA", summon2.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								summon2.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.HeroSummons.Where((HeroSummonState w) => w.ActorGuid == summon2.ActorGuid).Count().ToString()
							}
						});
						flag7 = true;
					}
				}
				if (state1.HeroSummons.Count != state2.HeroSummons.Count)
				{
					LogMismatch(list, isMPCompare, 144, "Number of HeroSummons does not match.", new List<string[]>
					{
						new string[3]
						{
							"HeroSummons Count",
							state1.HeroSummons.Count.ToString(),
							state2.HeroSummons.Count.ToString()
						},
						new string[3]
						{
							"HeroSummons",
							string.Join(", ", state1.HeroSummons.Select((HeroSummonState s) => s.ClassID)),
							string.Join(", ", state2.HeroSummons.Select((HeroSummonState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag7)
					{
						break;
					}
					bool flag8 = false;
					foreach (HeroSummonState summon3 in state1.HeroSummons)
					{
						if (!state2.HeroSummons.Exists((HeroSummonState e) => e.ActorGuid == summon3.ActorGuid && TileIndex.Compare(e.Location, summon3.Location)))
						{
							LogMismatch(list, isMPCompare, 145, "HeroSummon in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "HeroSummon GUID", summon3.ActorGuid, "Missing" },
								new string[2]
								{
									"HeroSummon ID",
									summon3.ID.ToString()
								},
								new string[3] { "Class ID", summon3.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									summon3.Location.ToString(),
									"Missing"
								}
							});
							flag8 = true;
						}
					}
					foreach (HeroSummonState summon4 in state2.HeroSummons)
					{
						if (!state1.HeroSummons.Exists((HeroSummonState e) => e.ActorGuid == summon4.ActorGuid && TileIndex.Compare(e.Location, summon4.Location)))
						{
							LogMismatch(list, isMPCompare, 145, "HeroSummon in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "HeroSummon GUID", "Missing", summon4.ActorGuid },
								new string[3]
								{
									"HeroSummon ID",
									"Missing",
									summon4.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", summon4.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									summon4.Location.ToString()
								}
							});
							flag8 = true;
						}
					}
					if (flag8)
					{
						break;
					}
					foreach (HeroSummonState summon5 in state1.HeroSummons)
					{
						try
						{
							HeroSummonState state5 = state2.HeroSummons.Single((HeroSummonState s) => s.ActorGuid == summon5.ActorGuid && TileIndex.Compare(s.Location, summon5.Location));
							list.AddRange(HeroSummonState.Compare(summon5, state5, isMPCompare));
						}
						catch (Exception ex4)
						{
							list.Add(new Tuple<int, string>(146, "Exception during HeroSummon compare.\n" + ex4.Message + "\n" + ex4.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.AllyMonsters, state2.AllyMonsters))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 147, "AllyMonsters List is null on one or both states.", new List<string[]> { new string[3]
				{
					"AllyMonsters",
					(state1.AllyMonsters == null) ? "is null" : "is not null",
					(state2.AllyMonsters == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.AllyMonsters.Any((EnemyState a) => a == null) || state2.AllyMonsters.Any((EnemyState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 148, "Scenario State contains null AllyMonsters.", new List<string[]> { new string[3]
					{
						"AllyMonsters",
						string.Join(", ", state1.AllyMonsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.AllyMonsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag9 = false;
				foreach (EnemyState monster6 in state1.AllyMonsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state1.AllyMonsters.Where((EnemyState w) => w.ActorGuid == monster6.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 149, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state AllyMonsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", monster6.ActorGuid, "NA" },
							new string[3]
							{
								"Standee ID",
								monster6.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", monster6.ClassID, "NA" },
							new string[3]
							{
								"Location",
								monster6.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.AllyMonsters.Where((EnemyState w) => w.ActorGuid == monster6.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag9 = true;
					}
				}
				foreach (EnemyState monster7 in state2.AllyMonsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state2.AllyMonsters.Where((EnemyState w) => w.ActorGuid == monster7.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 149, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state AllyMonsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", "NA", monster7.ActorGuid },
							new string[3]
							{
								"Standee ID",
								"NA",
								monster7.ID.ToString()
							},
							new string[3] { "Class ID", "NA", monster7.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								monster7.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.AllyMonsters.Where((EnemyState w) => w.ActorGuid == monster7.ActorGuid).Count().ToString()
							}
						});
						flag9 = true;
					}
				}
				if (state1.AllyMonsters.Count != state2.AllyMonsters.Count)
				{
					LogMismatch(list, isMPCompare, 150, "Number of AllyMonsters does not match.", new List<string[]>
					{
						new string[3]
						{
							"AllyMonsters Count",
							state1.AllyMonsters.Count.ToString(),
							state2.AllyMonsters.Count.ToString()
						},
						new string[3]
						{
							"AllyMonsters",
							string.Join(", ", state1.AllyMonsters.Select((EnemyState s) => s.ClassID)),
							string.Join(", ", state2.AllyMonsters.Select((EnemyState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag9)
					{
						break;
					}
					bool flag10 = false;
					foreach (EnemyState monster8 in state1.AllyMonsters)
					{
						if (!state2.AllyMonsters.Exists((EnemyState e) => e.ActorGuid == monster8.ActorGuid && TileIndex.Compare(e.Location, monster8.Location)))
						{
							LogMismatch(list, isMPCompare, 151, "AllyMonster in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "AllyMonster GUID", monster8.ActorGuid, "Missing" },
								new string[3]
								{
									"Standee ID",
									monster8.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", monster8.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									monster8.Location.ToString(),
									"Missing"
								}
							});
							flag10 = true;
						}
					}
					foreach (EnemyState monster9 in state2.AllyMonsters)
					{
						if (!state1.AllyMonsters.Exists((EnemyState e) => e.ActorGuid == monster9.ActorGuid && TileIndex.Compare(e.Location, monster9.Location)))
						{
							LogMismatch(list, isMPCompare, 151, "AllyMonster in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "AllyMonster GUID", "Missing", monster9.ActorGuid },
								new string[3]
								{
									"Standee ID",
									"Missing",
									monster9.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", monster9.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									monster9.Location.ToString()
								}
							});
							flag10 = true;
						}
					}
					if (flag10)
					{
						break;
					}
					foreach (EnemyState monster10 in state1.AllyMonsters)
					{
						try
						{
							EnemyState state6 = state2.AllyMonsters.Single((EnemyState s) => s.ActorGuid == monster10.ActorGuid && TileIndex.Compare(s.Location, monster10.Location));
							list.AddRange(EnemyState.Compare(monster10, state6, isMPCompare));
						}
						catch (Exception ex5)
						{
							list.Add(new Tuple<int, string>(152, "Exception during AllyMonster compare.\n" + ex5.Message + "\n" + ex5.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.NeutralMonsters, state2.NeutralMonsters))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 153, "NeutralMonsters List is null on one or both states.", new List<string[]> { new string[3]
				{
					"NeutralMonsters",
					(state1.NeutralMonsters == null) ? "is null" : "is not null",
					(state2.NeutralMonsters == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.NeutralMonsters.Any((EnemyState a) => a == null) || state2.NeutralMonsters.Any((EnemyState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 154, "Scenario State contains null NeutralMonsters.", new List<string[]> { new string[3]
					{
						"NeutralMonsters",
						string.Join(", ", state1.NeutralMonsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.NeutralMonsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag11 = false;
				foreach (EnemyState monster11 in state1.NeutralMonsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state1.NeutralMonsters.Where((EnemyState w) => w.ActorGuid == monster11.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 155, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state NeutralMonsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", monster11.ActorGuid, "NA" },
							new string[3]
							{
								"Standee ID",
								monster11.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", monster11.ClassID, "NA" },
							new string[3]
							{
								"Location",
								monster11.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.NeutralMonsters.Where((EnemyState w) => w.ActorGuid == monster11.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag11 = true;
					}
				}
				foreach (EnemyState monster12 in state2.NeutralMonsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state2.NeutralMonsters.Where((EnemyState w) => w.ActorGuid == monster12.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 155, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state NeutralMonsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", "NA", monster12.ActorGuid },
							new string[3]
							{
								"Standee ID",
								"NA",
								monster12.ID.ToString()
							},
							new string[3] { "Class ID", "NA", monster12.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								monster12.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.NeutralMonsters.Where((EnemyState w) => w.ActorGuid == monster12.ActorGuid).Count().ToString()
							}
						});
						flag11 = true;
					}
				}
				if (state1.NeutralMonsters.Count != state2.NeutralMonsters.Count)
				{
					LogMismatch(list, isMPCompare, 156, "Number of NeutralMonsters does not match.", new List<string[]>
					{
						new string[3]
						{
							"NeutralMonsters Count",
							state1.NeutralMonsters.Count.ToString(),
							state2.NeutralMonsters.Count.ToString()
						},
						new string[3]
						{
							"NeutralMonsters",
							string.Join(", ", state1.NeutralMonsters.Select((EnemyState s) => s.ClassID)),
							string.Join(", ", state2.NeutralMonsters.Select((EnemyState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag11)
					{
						break;
					}
					bool flag12 = false;
					foreach (EnemyState monster13 in state1.NeutralMonsters)
					{
						if (!state2.NeutralMonsters.Exists((EnemyState e) => e.ActorGuid == monster13.ActorGuid && TileIndex.Compare(e.Location, monster13.Location)))
						{
							LogMismatch(list, isMPCompare, 157, "NeutralMonster in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "NeutralMonster GUID", monster13.ActorGuid, "Missing" },
								new string[3]
								{
									"Standee ID",
									monster13.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", monster13.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									monster13.Location.ToString(),
									"Missing"
								}
							});
							flag12 = true;
						}
					}
					foreach (EnemyState monster14 in state2.NeutralMonsters)
					{
						if (!state1.NeutralMonsters.Exists((EnemyState e) => e.ActorGuid == monster14.ActorGuid && TileIndex.Compare(e.Location, monster14.Location)))
						{
							LogMismatch(list, isMPCompare, 157, "NeutralMonster in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "NeutralMonster GUID", "Missing", monster14.ActorGuid },
								new string[3]
								{
									"Standee ID",
									"Missing",
									monster14.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", monster14.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									monster14.Location.ToString()
								}
							});
							flag12 = true;
						}
					}
					if (flag12)
					{
						break;
					}
					foreach (EnemyState monster15 in state1.NeutralMonsters)
					{
						try
						{
							EnemyState state7 = state2.NeutralMonsters.Single((EnemyState s) => s.ActorGuid == monster15.ActorGuid && TileIndex.Compare(s.Location, monster15.Location));
							list.AddRange(EnemyState.Compare(monster15, state7, isMPCompare));
						}
						catch (Exception ex6)
						{
							list.Add(new Tuple<int, string>(158, "Exception during NeutralMonster compare.\n" + ex6.Message + "\n" + ex6.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.Objects, state2.Objects))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 159, "Objects List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Objects",
					(state1.Objects == null) ? "is null" : "is not null",
					(state2.Objects == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Objects.Any((ObjectState a) => a == null) || state2.Objects.Any((ObjectState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 160, "Scenario State contains null Objects.", new List<string[]> { new string[3]
					{
						"Objects",
						string.Join(", ", state1.Objects.Select((ObjectState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.Objects.Select((ObjectState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag13 = false;
				foreach (ObjectState object1 in state1.Objects.Where((ObjectState w) => w.ActorGuid != null))
				{
					if (state1.Objects.Where((ObjectState w) => w.ActorGuid == object1.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 161, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state Objects list.", new List<string[]>
						{
							new string[3] { "Object GUID", object1.ActorGuid, "NA" },
							new string[3]
							{
								"Standee ID",
								object1.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", object1.ClassID, "NA" },
							new string[3]
							{
								"Duplicate Count",
								state1.Objects.Where((ObjectState w) => w.ActorGuid == object1.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag13 = true;
					}
				}
				foreach (ObjectState object2 in state2.Objects.Where((ObjectState w) => w.ActorGuid != null))
				{
					if (state2.Objects.Where((ObjectState w) => w.ActorGuid == object2.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 161, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state Objects list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", "NA", object2.ActorGuid },
							new string[3]
							{
								"Standee ID",
								"NA",
								object2.ID.ToString()
							},
							new string[3] { "Class ID", "NA", object2.ClassID },
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.Objects.Where((ObjectState w) => w.ActorGuid == object2.ActorGuid).Count().ToString()
							}
						});
						flag13 = true;
					}
				}
				if (state1.Objects.Count != state2.Objects.Count)
				{
					LogMismatch(list, isMPCompare, 162, "Number of Objects does not match.", new List<string[]>
					{
						new string[3]
						{
							"Objects Count",
							state1.Objects.Count.ToString(),
							state2.Objects.Count.ToString()
						},
						new string[3]
						{
							"Objects",
							string.Join(", ", state1.Objects.Select((ObjectState s) => s.ClassID)),
							string.Join(", ", state2.Objects.Select((ObjectState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag13)
					{
						break;
					}
					bool flag14 = false;
					foreach (ObjectState object3 in state1.Objects.Where((ObjectState w) => w.ActorGuid != null))
					{
						if (!state2.Objects.Exists((ObjectState e) => e.ActorGuid == object3.ActorGuid))
						{
							LogMismatch(list, isMPCompare, 162, "Object in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Object GUID", object3.ActorGuid, "Missing" },
								new string[3]
								{
									"Standee ID",
									object3.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", object3.ClassID, "Missing" }
							});
							flag14 = true;
						}
					}
					foreach (ObjectState object4 in state2.Objects.Where((ObjectState w) => w.ActorGuid != null))
					{
						if (!state1.Objects.Exists((ObjectState e) => e.ActorGuid == object4.ActorGuid))
						{
							LogMismatch(list, isMPCompare, 162, "Object in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Object GUID", "Missing", object4.ActorGuid },
								new string[3]
								{
									"Standee ID",
									"Missing",
									object4.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", object4.ClassID }
							});
							flag14 = true;
						}
					}
					if (flag14)
					{
						break;
					}
					foreach (ObjectState object5 in state1.Objects.Where((ObjectState w) => w.ActorGuid != null))
					{
						try
						{
							ObjectState state8 = state2.Objects.Single((ObjectState s) => s.ActorGuid == object5.ActorGuid);
							list.AddRange(ObjectState.Compare(object5, state8, isMPCompare));
						}
						catch (Exception ex7)
						{
							list.Add(new Tuple<int, string>(163, "Exception during Object compare.\n" + ex7.Message + "\n" + ex7.StackTrace));
						}
					}
				}
				break;
			}
			}
			List<ActorState> list2 = ((IEnumerable<PlayerState>)state1.Players).Select((Func<PlayerState, ActorState>)((PlayerState s) => s)).Concat(((IEnumerable<EnemyState>)state1.Monsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<HeroSummonState>)state1.HeroSummons).Select((Func<HeroSummonState, ActorState>)((HeroSummonState s) => s)).Concat(((IEnumerable<EnemyState>)state1.AllyMonsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<EnemyState>)state1.NeutralMonsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<ObjectState>)state1.Objects).Select((Func<ObjectState, ActorState>)((ObjectState s) => s))))))).ToList();
			List<ActorState> list3 = ((IEnumerable<PlayerState>)state2.Players).Select((Func<PlayerState, ActorState>)((PlayerState s) => s)).Concat(((IEnumerable<EnemyState>)state2.Monsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<HeroSummonState>)state2.HeroSummons).Select((Func<HeroSummonState, ActorState>)((HeroSummonState s) => s)).Concat(((IEnumerable<EnemyState>)state2.AllyMonsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<EnemyState>)state2.NeutralMonsters).Select((Func<EnemyState, ActorState>)((EnemyState s) => s)).Concat(((IEnumerable<ObjectState>)state2.Objects).Select((Func<ObjectState, ActorState>)((ObjectState s) => s))))))).ToList();
			list2.RemoveAll((ActorState x) => x is EnemyState { IsHiddenForCurrentPartySize: not false } || x.PhasedOut || (x.Actor != null && x.Actor.IgnoreActorCollision));
			list3.RemoveAll((ActorState x) => x is EnemyState { IsHiddenForCurrentPartySize: not false } || x.PhasedOut || (x.Actor != null && x.Actor.IgnoreActorCollision));
			foreach (ActorState actor1 in list2.Where((ActorState w) => !w.IsDead))
			{
				List<ActorState> list4 = list2.Where((ActorState w) => !w.IsDead && TileIndex.Compare(w.Location, actor1.Location)).ToList();
				if (list4.Count > 1)
				{
					List<string[]> list5 = new List<string[]>();
					list5.Add(new string[3]
					{
						"Duplicate Location Count",
						list2.Where((ActorState w) => !w.IsDead && TileIndex.Compare(w.Location, actor1.Location)).Count().ToString(),
						"NA"
					});
					List<string[]> list6 = list5;
					for (int num2 = 0; num2 < list4.Count; num2++)
					{
						list6.AddRange(new List<string[]>
						{
							new string[3]
							{
								"Actor GUID",
								list4[num2].ActorGuid,
								"NA"
							},
							new string[3]
							{
								"Actor ID",
								list4[num2].ActorID.ToString(),
								"NA"
							},
							new string[3]
							{
								"Class ID",
								list4[num2].ClassID,
								"NA"
							},
							new string[3]
							{
								"Location",
								list4[num2].Location.ToString(),
								"NA"
							}
						});
					}
					LogMismatch(list, isMPCompare, 155, "Duplicate Location set in " + GetStateName(isState1: true, isMPCompare) + " state list.", list6);
				}
			}
			foreach (ActorState actor2 in list3.Where((ActorState w) => !w.IsDead))
			{
				List<ActorState> list7 = list3.Where((ActorState w) => !w.IsDead && TileIndex.Compare(w.Location, actor2.Location)).ToList();
				if (list7.Count > 1)
				{
					List<string[]> list5 = new List<string[]>();
					list5.Add(new string[3]
					{
						"Duplicate Location Count",
						list3.Where((ActorState w) => !w.IsDead && TileIndex.Compare(w.Location, actor2.Location)).Count().ToString(),
						"NA"
					});
					List<string[]> list8 = list5;
					for (int num3 = 0; num3 < list7.Count; num3++)
					{
						list8.AddRange(new List<string[]>
						{
							new string[3]
							{
								"Actor GUID",
								"NA",
								list7[num3].ActorGuid
							},
							new string[3]
							{
								"Actor ID",
								"NA",
								list7[num3].ActorID.ToString()
							},
							new string[3]
							{
								"Class ID",
								"NA",
								list7[num3].ClassID
							},
							new string[3]
							{
								"Location",
								"NA",
								list7[num3].Location.ToString()
							}
						});
					}
					LogMismatch(list, isMPCompare, 155, "Duplicate Location set in " + GetStateName(isState1: false, isMPCompare) + " state list.", list8);
				}
			}
			list.AddRange(EnemyClassManagerState.Compare(state1.EnemyClassManager, state2.EnemyClassManager, isMPCompare));
			if (StateShared.CheckNullsMatch(state1.HeroSummonClassManager, state2.HeroSummonClassManager) == StateShared.ENullStatus.Mismatch)
			{
				LogMismatch(list, isMPCompare, 3100, "HeroSummonClassManager null state does not match.", new List<string[]> { new string[3]
				{
					"HeroSummonClassManager",
					(state1.HeroSummonClassManager == null) ? "is null" : "is not null",
					(state2.HeroSummonClassManager == null) ? "is null" : "is not null"
				} });
			}
			else
			{
				list.AddRange(HeroSummonClassManagerState.Compare(state1.HeroSummonClassManager, state2.HeroSummonClassManager, isMPCompare));
			}
			switch (StateShared.CheckNullsMatch(state1.Props, state2.Props))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 164, "Props List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Props",
					(state1.Props == null) ? "is null" : "is not null",
					(state2.Props == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Props.Any((CObjectProp a) => a == null) || state2.Props.Any((CObjectProp a) => a == null))
				{
					LogMismatch(list, isMPCompare, 165, "Scenario State contains null Props.", new List<string[]> { new string[3]
					{
						"Props",
						string.Join(", ", state1.Props.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString())),
						string.Join(", ", state2.Props.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString()))
					} });
					break;
				}
				bool flag15 = false;
				foreach (CObjectProp object6 in state1.Props)
				{
					if (state1.Props.Where((CObjectProp w) => w.PropGuid == object6.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 166, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state Props list.", new List<string[]>
						{
							new string[3] { "Prop GUID", object6.PropGuid, "NA" },
							new string[3]
							{
								"Prop Type",
								object6.PropType.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate Count",
								state1.Props.Where((CObjectProp w) => w.PropGuid == object6.PropGuid).Count().ToString(),
								"NA"
							}
						});
						flag15 = true;
					}
				}
				foreach (CObjectProp object7 in state2.Props)
				{
					if (state2.Props.Where((CObjectProp w) => w.PropGuid == object7.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 166, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state Props list.", new List<string[]>
						{
							new string[3] { "Prop GUID", "NA", object7.PropGuid },
							new string[3]
							{
								"Prop Type",
								"NA",
								object7.PropType.ToString()
							},
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.Props.Where((CObjectProp w) => w.PropGuid == object7.PropGuid).Count().ToString()
							}
						});
						flag15 = true;
					}
				}
				if (state1.Props.Count != state2.Props.Count)
				{
					LogMismatch(list, isMPCompare, 167, "Number of Props does not match.", new List<string[]>
					{
						new string[3]
						{
							"Props Count",
							state1.Props.Count.ToString(),
							state2.Props.Count.ToString()
						},
						new string[3]
						{
							"Props",
							string.Join(", ", state1.Props.Select((CObjectProp s) => s.PropType.ToString())),
							string.Join(", ", state2.Props.Select((CObjectProp s) => s.PropType.ToString()))
						}
					});
				}
				else
				{
					if (flag15)
					{
						break;
					}
					bool flag16 = false;
					foreach (CObjectProp object8 in state1.Props)
					{
						if (!state2.Props.Exists((CObjectProp e) => e.PropGuid == object8.PropGuid))
						{
							LogMismatch(list, isMPCompare, 168, "Prop in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", object8.PropGuid, "Missing" },
								new string[3]
								{
									"Prop Type",
									object8.PropType.ToString(),
									"Missing"
								}
							});
							flag16 = true;
						}
					}
					foreach (CObjectProp object9 in state2.Props)
					{
						if (!state1.Props.Exists((CObjectProp e) => e.PropGuid == object9.PropGuid))
						{
							LogMismatch(list, isMPCompare, 168, "Prop in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", "Missing", object9.PropGuid },
								new string[3]
								{
									"Prop Type",
									"Missing",
									object9.PropType.ToString()
								}
							});
							flag16 = true;
						}
					}
					if (flag16)
					{
						break;
					}
					foreach (CObjectProp object10 in state1.Props)
					{
						try
						{
							CObjectProp cObjectProp = state2.Props.Single((CObjectProp s) => s.PropGuid == object10.PropGuid);
							if (object10 is CObjectChest)
							{
								list.AddRange(CObjectChest.Compare(object10 as CObjectChest, cObjectProp as CObjectChest, isMPCompare));
							}
							else if (object10 is CObjectDoor)
							{
								list.AddRange(CObjectDoor.Compare(object10 as CObjectDoor, cObjectProp as CObjectDoor, isMPCompare));
							}
							else if (object10 is CObjectGoldPile)
							{
								list.AddRange(CObjectGoldPile.Compare(object10 as CObjectGoldPile, cObjectProp as CObjectGoldPile, isMPCompare));
							}
							else if (object10 is CObjectObstacle)
							{
								list.AddRange(CObjectObstacle.Compare(object10 as CObjectObstacle, cObjectProp as CObjectObstacle, isMPCompare));
							}
							else if (object10 is CObjectTrap)
							{
								list.AddRange(CObjectTrap.Compare(object10 as CObjectTrap, cObjectProp as CObjectTrap, isMPCompare));
							}
							else if (object10 is CObjectPortal)
							{
								list.AddRange(CObjectPortal.Compare(object10 as CObjectPortal, cObjectProp as CObjectPortal, isMPCompare));
							}
							else
							{
								list.AddRange(CObjectProp.Compare(object10, cObjectProp, isMPCompare));
							}
						}
						catch (Exception ex8)
						{
							list.Add(new Tuple<int, string>(169, "Exception during Props compare.\n" + ex8.Message + "\n" + ex8.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.ActivatedProps, state2.ActivatedProps))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 170, "ActivatedProps List is null on one or both states.", new List<string[]> { new string[3]
				{
					"ActivatedProps",
					(state1.ActivatedProps == null) ? "is null" : "is not null",
					(state2.ActivatedProps == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.ActivatedProps.Any((CObjectProp a) => a == null) || state2.ActivatedProps.Any((CObjectProp a) => a == null))
				{
					LogMismatch(list, isMPCompare, 171, "Scenario State contains null ActivatedProps.", new List<string[]> { new string[3]
					{
						"ActivatedProps",
						string.Join(", ", state1.ActivatedProps.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString())),
						string.Join(", ", state2.ActivatedProps.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString()))
					} });
					break;
				}
				bool flag17 = false;
				foreach (CObjectProp object11 in state1.ActivatedProps)
				{
					if (state1.ActivatedProps.Where((CObjectProp w) => w.PropGuid == object11.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 172, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state ActivatedProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", object11.PropGuid, "NA" },
							new string[3]
							{
								"Prop Type",
								object11.PropType.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate Count",
								state1.ActivatedProps.Where((CObjectProp w) => w.PropGuid == object11.PropGuid).Count().ToString(),
								"NA"
							}
						});
						flag17 = true;
					}
				}
				foreach (CObjectProp object12 in state2.ActivatedProps)
				{
					if (state2.ActivatedProps.Where((CObjectProp w) => w.PropGuid == object12.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 172, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state ActivatedProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", "NA", object12.PropGuid },
							new string[3]
							{
								"Prop Type",
								"NA",
								object12.PropType.ToString()
							},
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.ActivatedProps.Where((CObjectProp w) => w.PropGuid == object12.PropGuid).Count().ToString()
							}
						});
						flag17 = true;
					}
				}
				if (state1.ActivatedProps.Count != state2.ActivatedProps.Count)
				{
					LogMismatch(list, isMPCompare, 173, "Number of ActivatedProps does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActivatedProps Count",
							state1.ActivatedProps.Count.ToString(),
							state2.ActivatedProps.Count.ToString()
						},
						new string[3]
						{
							"ActivatedProps",
							string.Join(", ", state1.ActivatedProps.Select((CObjectProp s) => s.PropType.ToString())),
							string.Join(", ", state2.ActivatedProps.Select((CObjectProp s) => s.PropType.ToString()))
						}
					});
				}
				else
				{
					if (flag17)
					{
						break;
					}
					bool flag18 = false;
					foreach (CObjectProp object13 in state1.ActivatedProps)
					{
						if (!state2.ActivatedProps.Exists((CObjectProp e) => e.PropGuid == object13.PropGuid))
						{
							LogMismatch(list, isMPCompare, 174, "ActivatedProp in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", object13.PropGuid, "Missing" },
								new string[3]
								{
									"Prop Type",
									object13.PropType.ToString(),
									"Missing"
								}
							});
							flag18 = true;
						}
					}
					foreach (CObjectProp object14 in state2.ActivatedProps)
					{
						if (!state1.ActivatedProps.Exists((CObjectProp e) => e.PropGuid == object14.PropGuid))
						{
							LogMismatch(list, isMPCompare, 174, "ActivatedProp in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", "Missing", object14.PropGuid },
								new string[3]
								{
									"Prop Type",
									"Missing",
									object14.PropType.ToString()
								}
							});
							flag18 = true;
						}
					}
					if (flag18)
					{
						break;
					}
					foreach (CObjectProp object15 in state1.ActivatedProps)
					{
						try
						{
							CObjectProp cObjectProp2 = state2.ActivatedProps.Single((CObjectProp s) => s.PropGuid == object15.PropGuid);
							if (object15 is CObjectChest)
							{
								list.AddRange(CObjectChest.Compare(object15 as CObjectChest, cObjectProp2 as CObjectChest, isMPCompare));
							}
							else if (object15 is CObjectDoor)
							{
								list.AddRange(CObjectDoor.Compare(object15 as CObjectDoor, cObjectProp2 as CObjectDoor, isMPCompare));
							}
							else if (object15 is CObjectGoldPile)
							{
								list.AddRange(CObjectGoldPile.Compare(object15 as CObjectGoldPile, cObjectProp2 as CObjectGoldPile, isMPCompare));
							}
							else if (object15 is CObjectObstacle)
							{
								list.AddRange(CObjectObstacle.Compare(object15 as CObjectObstacle, cObjectProp2 as CObjectObstacle, isMPCompare));
							}
							else if (object15 is CObjectTrap)
							{
								list.AddRange(CObjectTrap.Compare(object15 as CObjectTrap, cObjectProp2 as CObjectTrap, isMPCompare));
							}
							else
							{
								list.AddRange(CObjectProp.Compare(object15, cObjectProp2, isMPCompare));
							}
						}
						catch (Exception ex9)
						{
							list.Add(new Tuple<int, string>(175, "Exception during ActivatedProps compare.\n" + ex9.Message + "\n" + ex9.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.TransparentProps, state2.TransparentProps))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 176, "TransparentProps List is null on one or both states.", new List<string[]> { new string[3]
				{
					"TransparentProps",
					(state1.TransparentProps == null) ? "is null" : "is not null",
					(state2.TransparentProps == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.TransparentProps.Any((CObjectObstacle a) => a == null) || state2.TransparentProps.Any((CObjectObstacle a) => a == null))
				{
					LogMismatch(list, isMPCompare, 177, "Scenario State contains null TransparentProps.", new List<string[]> { new string[3]
					{
						"TransparentProps",
						string.Join(", ", state1.TransparentProps.Select((CObjectObstacle s) => (s == null) ? "null" : s.PropType.ToString())),
						string.Join(", ", state2.TransparentProps.Select((CObjectObstacle s) => (s == null) ? "null" : s.PropType.ToString()))
					} });
					break;
				}
				bool flag19 = false;
				foreach (CObjectObstacle object16 in state1.TransparentProps)
				{
					if (state1.TransparentProps.Where((CObjectObstacle w) => w.PropGuid == object16.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 178, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state TransparentProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", object16.PropGuid, "NA" },
							new string[3]
							{
								"Prop Type",
								object16.PropType.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate Count",
								state1.TransparentProps.Where((CObjectObstacle w) => w.PropGuid == object16.PropGuid).Count().ToString(),
								"NA"
							}
						});
						flag19 = true;
					}
				}
				foreach (CObjectObstacle object17 in state2.TransparentProps)
				{
					if (state2.TransparentProps.Where((CObjectObstacle w) => w.PropGuid == object17.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 178, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state TransparentProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", "NA", object17.PropGuid },
							new string[3]
							{
								"Prop Type",
								"NA",
								object17.PropType.ToString()
							},
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.TransparentProps.Where((CObjectObstacle w) => w.PropGuid == object17.PropGuid).Count().ToString()
							}
						});
						flag19 = true;
					}
				}
				if (state1.TransparentProps.Count != state2.TransparentProps.Count)
				{
					LogMismatch(list, isMPCompare, 179, "Number of TransparentProps does not match.", new List<string[]>
					{
						new string[3]
						{
							"TransparentProps Count",
							state1.TransparentProps.Count.ToString(),
							state2.TransparentProps.Count.ToString()
						},
						new string[3]
						{
							"TransparentProps",
							string.Join(", ", state1.TransparentProps.Select((CObjectObstacle s) => s.PropType)),
							string.Join(", ", state2.TransparentProps.Select((CObjectObstacle s) => s.PropType))
						}
					});
				}
				else
				{
					if (flag19)
					{
						break;
					}
					bool flag20 = false;
					foreach (CObjectObstacle object18 in state1.TransparentProps)
					{
						if (!state2.TransparentProps.Exists((CObjectObstacle e) => e.PropGuid == object18.PropGuid))
						{
							LogMismatch(list, isMPCompare, 180, "TransparentProp in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", object18.PropGuid, "Missing" },
								new string[3]
								{
									"Prop Type",
									object18.PropType.ToString(),
									"Missing"
								}
							});
							flag20 = true;
						}
					}
					foreach (CObjectObstacle object19 in state2.TransparentProps)
					{
						if (!state1.TransparentProps.Exists((CObjectObstacle e) => e.PropGuid == object19.PropGuid))
						{
							LogMismatch(list, isMPCompare, 180, "TransparentProp in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", "Missing", object19.PropGuid },
								new string[3]
								{
									"Prop Type",
									"Missing",
									object19.PropType.ToString()
								}
							});
							flag20 = true;
						}
					}
					if (flag20)
					{
						break;
					}
					foreach (CObjectObstacle object20 in state1.TransparentProps)
					{
						try
						{
							CObjectObstacle obs = state2.TransparentProps.Single((CObjectObstacle s) => s.PropGuid == object20.PropGuid);
							list.AddRange(CObjectObstacle.Compare(object20, obs, isMPCompare));
						}
						catch (Exception ex10)
						{
							list.Add(new Tuple<int, string>(181, "Exception during TransparentProps compare.\n" + ex10.Message + "\n" + ex10.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.WinObjectives, state2.WinObjectives))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 182, "WinObjectives List is null on one or both states.", new List<string[]> { new string[3]
				{
					"WinObjectives",
					(state1.WinObjectives == null) ? "is null" : "is not null",
					(state2.WinObjectives == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.WinObjectives.Any((CObjective a) => a == null) || state2.WinObjectives.Any((CObjective a) => a == null))
				{
					LogMismatch(list, isMPCompare, 183, "Scenario State contains null WinObjectives.", new List<string[]> { new string[3]
					{
						"WinObjectives",
						string.Join(", ", state1.WinObjectives.Select((CObjective s) => (s == null) ? "null" : s.ObjectiveType.ToString())),
						string.Join(", ", state2.WinObjectives.Select((CObjective s) => (s == null) ? "null" : s.ObjectiveType.ToString()))
					} });
					break;
				}
				if (state1.WinObjectives.Count != state2.WinObjectives.Count)
				{
					LogMismatch(list, isMPCompare, 184, "Number of WinObjectives does not match.", new List<string[]>
					{
						new string[3]
						{
							"WinObjectives Count",
							state1.WinObjectives.Count.ToString(),
							state2.WinObjectives.Count.ToString()
						},
						new string[3]
						{
							"WinObjectives",
							string.Join(", ", state1.WinObjectives.Select((CObjective s) => s.ObjectiveType.ToString())),
							string.Join(", ", state2.WinObjectives.Select((CObjective s) => s.ObjectiveType.ToString()))
						}
					});
					break;
				}
				for (int num4 = 0; num4 < state1.WinObjectives.Count; num4++)
				{
					try
					{
						list.AddRange(CObjective.Compare(state1.WinObjectives[num4], state2.WinObjectives[num4], isMPCompare));
					}
					catch (Exception ex11)
					{
						list.Add(new Tuple<int, string>(185, "Exception during WinObjectives compare.\n" + ex11.Message + "\n" + ex11.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.LoseObjectives, state2.LoseObjectives))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 186, "LoseObjectives List is null on one or both states.", new List<string[]> { new string[3]
				{
					"LoseObjectives",
					(state1.LoseObjectives == null) ? "is null" : "is not null",
					(state2.LoseObjectives == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.LoseObjectives.Any((CObjective a) => a == null) || state2.LoseObjectives.Any((CObjective a) => a == null))
				{
					LogMismatch(list, isMPCompare, 187, "Scenario State contains null LoseObjectives.", new List<string[]> { new string[3]
					{
						"LoseObjectives",
						string.Join(", ", state1.LoseObjectives.Select((CObjective s) => (s == null) ? "null" : s.ObjectiveType.ToString())),
						string.Join(", ", state2.LoseObjectives.Select((CObjective s) => (s == null) ? "null" : s.ObjectiveType.ToString()))
					} });
					break;
				}
				if (state1.LoseObjectives.Count != state2.LoseObjectives.Count)
				{
					LogMismatch(list, isMPCompare, 188, "Number of LoseObjectives does not match.", new List<string[]>
					{
						new string[3]
						{
							"LoseObjectives Count",
							state1.LoseObjectives.Count.ToString(),
							state2.LoseObjectives.Count.ToString()
						},
						new string[3]
						{
							"LoseObjectives",
							string.Join(", ", state1.LoseObjectives.Select((CObjective s) => s.ObjectiveType.ToString())),
							string.Join(", ", state2.LoseObjectives.Select((CObjective s) => s.ObjectiveType.ToString()))
						}
					});
					break;
				}
				for (int num5 = 0; num5 < state1.LoseObjectives.Count; num5++)
				{
					try
					{
						list.AddRange(CObjective.Compare(state1.LoseObjectives[num5], state2.LoseObjectives[num5], isMPCompare));
					}
					catch (Exception ex12)
					{
						list.Add(new Tuple<int, string>(189, "Exception during LoseObjectives compare.\n" + ex12.Message + "\n" + ex12.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.Spawners, state2.Spawners))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 190, "Spawners List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Spawners",
					(state1.Spawners == null) ? "is null" : "is not null",
					(state2.Spawners == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Spawners.Any((CSpawner a) => a == null) || state2.Spawners.Any((CSpawner a) => a == null))
				{
					LogMismatch(list, isMPCompare, 191, "Scenario State contains null Spawners.", new List<string[]> { new string[3]
					{
						"Spawners",
						string.Join(", ", state1.Spawners.Select((CSpawner s) => (s == null) ? "null" : s.SpawnerGuid)),
						string.Join(", ", state2.Spawners.Select((CSpawner s) => (s == null) ? "null" : s.SpawnerGuid))
					} });
					break;
				}
				bool flag21 = false;
				foreach (CSpawner spawner1 in state1.Spawners)
				{
					if (state1.Spawners.Where((CSpawner w) => w.SpawnerGuid == spawner1.SpawnerGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 192, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state Spawners list.", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawner1.SpawnerGuid, "NA" },
							new string[3]
							{
								"Duplicate Count",
								state1.Spawners.Where((CSpawner w) => w.SpawnerGuid == spawner1.SpawnerGuid).Count().ToString(),
								"NA"
							}
						});
						flag21 = true;
					}
				}
				foreach (CSpawner spawner2 in state2.Spawners)
				{
					if (state2.Spawners.Where((CSpawner w) => w.SpawnerGuid == spawner2.SpawnerGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 192, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state Spawners list.", new List<string[]>
						{
							new string[3] { "Spawner GUID", "NA", spawner2.SpawnerGuid },
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.Spawners.Where((CSpawner w) => w.SpawnerGuid == spawner2.SpawnerGuid).Count().ToString()
							}
						});
						flag21 = true;
					}
				}
				if (state1.Spawners.Count != state2.Spawners.Count)
				{
					LogMismatch(list, isMPCompare, 193, "Number of Spawners does not match.", new List<string[]>
					{
						new string[3]
						{
							"Spawner Count",
							state1.Spawners.Count.ToString(),
							state2.Spawners.Count.ToString()
						},
						new string[3]
						{
							"Spawners",
							string.Join(", ", state1.Spawners.Select((CSpawner s) => s.SpawnerGuid)),
							string.Join(", ", state2.Spawners.Select((CSpawner s) => s.SpawnerGuid))
						}
					});
				}
				else
				{
					if (flag21)
					{
						break;
					}
					bool flag22 = false;
					foreach (CSpawner spawner3 in state1.Spawners)
					{
						if (!state2.Spawners.Exists((CSpawner e) => e.SpawnerGuid == spawner3.SpawnerGuid))
						{
							LogMismatch(list, isMPCompare, 194, "Spawner in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]> { new string[3] { "Spawner GUID", spawner3.SpawnerGuid, "Missing" } });
							flag22 = true;
						}
					}
					foreach (CSpawner spawner4 in state2.Spawners)
					{
						if (!state1.Spawners.Exists((CSpawner e) => e.SpawnerGuid == spawner4.SpawnerGuid))
						{
							LogMismatch(list, isMPCompare, 194, "Spawner in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]> { new string[3] { "Spawner GUID", "Missing", spawner4.SpawnerGuid } });
							flag22 = true;
						}
					}
					if (flag22)
					{
						break;
					}
					foreach (CSpawner spawner5 in state1.Spawners)
					{
						try
						{
							CSpawner state9 = state2.Spawners.Single((CSpawner s) => s.SpawnerGuid == spawner5.SpawnerGuid);
							list.AddRange(CSpawner.Compare(spawner5, state9, isMPCompare));
						}
						catch (Exception ex13)
						{
							list.Add(new Tuple<int, string>(195, "Exception during Spawners compare.\n" + ex13.Message + "\n" + ex13.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.ScenarioModifiers, state2.ScenarioModifiers))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 196, "ScenarioModifiers List is null on one or both states.", new List<string[]> { new string[3]
				{
					"ScenarioModifiers",
					(state1.ScenarioModifiers == null) ? "is null" : "is not null",
					(state2.ScenarioModifiers == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.ScenarioModifiers.Any((CScenarioModifier a) => a == null) || state2.ScenarioModifiers.Any((CScenarioModifier a) => a == null))
				{
					LogMismatch(list, isMPCompare, 197, "Scenario State contains null ScenarioModifiers.", new List<string[]> { new string[3]
					{
						"ScenarioModifiers",
						string.Join(", ", state1.ScenarioModifiers.Select((CScenarioModifier s) => (s == null) ? "null" : s.ScenarioModifierType.ToString())),
						string.Join(", ", state2.ScenarioModifiers.Select((CScenarioModifier s) => (s == null) ? "null" : s.ScenarioModifierType.ToString()))
					} });
					break;
				}
				if (state1.ScenarioModifiers.Count != state2.ScenarioModifiers.Count)
				{
					LogMismatch(list, isMPCompare, 198, "Number of ScenarioModifiers does not match.", new List<string[]>
					{
						new string[3]
						{
							"ScenarioModifiers Count",
							state1.ScenarioModifiers.Count.ToString(),
							state2.ScenarioModifiers.Count.ToString()
						},
						new string[3]
						{
							"ScenarioModifiers",
							string.Join(", ", state1.ScenarioModifiers.Select((CScenarioModifier s) => s.ScenarioModifierType.ToString())),
							string.Join(", ", state2.ScenarioModifiers.Select((CScenarioModifier s) => s.ScenarioModifierType.ToString()))
						}
					});
					break;
				}
				for (int num6 = 0; num6 < state1.ScenarioModifiers.Count; num6++)
				{
					try
					{
						list.AddRange(CScenarioModifier.Compare(state1.ScenarioModifiers[num6], state2.ScenarioModifiers[num6], isMPCompare));
					}
					catch (Exception ex14)
					{
						list.Add(new Tuple<int, string>(199, "Exception during ScenarioModifiers compare.\n" + ex14.Message + "\n" + ex14.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.RoundChestRewards, state2.RoundChestRewards))
			{
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 200, "State RoundChestRewards null state does not match.", new List<string[]> { new string[3]
				{
					"RoundChestRewards",
					(state1.RoundChestRewards == null) ? "is null" : "is not null",
					(state2.RoundChestRewards == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.RoundChestRewards.Count != state2.RoundChestRewards.Count)
				{
					LogMismatch(list, isMPCompare, 201, "State total RoundChestRewards Count does not match.", new List<string[]> { new string[3]
					{
						"RoundChestRewards Count",
						state1.RoundChestRewards.Count.ToString(),
						state2.RoundChestRewards.Count.ToString()
					} });
					break;
				}
				bool flag23 = false;
				foreach (Tuple<string, RewardGroup> pair in state1.RoundChestRewards)
				{
					if (!state2.RoundChestRewards.Exists((Tuple<string, RewardGroup> e) => e.Item1 == pair.Item1))
					{
						LogMismatch(list, isMPCompare, 202, "State RoundChestRewards in " + GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + GetStateName(isState1: true, isMPCompare) + ".", new List<string[]> { new string[3] { "RoundChestRewards Key", pair.Item1, "Missing" } });
						flag23 = true;
					}
				}
				foreach (Tuple<string, RewardGroup> pair2 in state2.RoundChestRewards)
				{
					if (!state1.RoundChestRewards.Exists((Tuple<string, RewardGroup> e) => e.Item1 == pair2.Item1))
					{
						LogMismatch(list, isMPCompare, 202, "State RoundChestRewards in " + GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + GetStateName(isState1: false, isMPCompare) + ".", new List<string[]> { new string[3] { "RoundChestRewards Key", "Missing", pair2.Item1 } });
						flag23 = true;
					}
				}
				if (flag23)
				{
					break;
				}
				foreach (Tuple<string, RewardGroup> pair3 in state1.RoundChestRewards)
				{
					try
					{
						if (!pair3.Item2.Equals(state2.RoundChestRewards.Single((Tuple<string, RewardGroup> s) => s.Item1 == pair3.Item1).Item2))
						{
							LogMismatch(list, isMPCompare, 203, "State RoundChestRewards has key with differing Reward values.", new List<string[]> { new string[3] { "RoundChestRewards Key", pair3.Item1, pair3.Item1 } });
							flag23 = true;
						}
					}
					catch (Exception ex15)
					{
						list.Add(new Tuple<int, string>(204, "Exception during RoundChestRewards compare.\n" + ex15.Message + "\n" + ex15.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.GoalChestRewards, state2.GoalChestRewards))
			{
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 205, "State GoalChestRewards null state does not match.", new List<string[]> { new string[3]
				{
					"GoalChestRewards",
					(state1.GoalChestRewards == null) ? "is null" : "is not null",
					(state2.GoalChestRewards == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.GoalChestRewards.Count != state2.GoalChestRewards.Count)
				{
					LogMismatch(list, isMPCompare, 206, "State total GoalChestRewards Count does not match.", new List<string[]> { new string[3]
					{
						"GoalChestRewards Count",
						state1.GoalChestRewards.Count.ToString(),
						state2.GoalChestRewards.Count.ToString()
					} });
					break;
				}
				try
				{
					for (int num7 = 0; num7 < state1.GoalChestRewards.Count; num7++)
					{
						Tuple<string, RewardGroup> tuple = state1.GoalChestRewards[num7];
						Tuple<string, RewardGroup> tuple2 = state2.GoalChestRewards[num7];
						if (tuple.Item1 != tuple2.Item1)
						{
							LogMismatch(list, isMPCompare, 207, "State GoalChestReward in " + GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + GetStateName(isState1: true, isMPCompare) + ".", new List<string[]> { new string[3] { "GoalChestReward Key", tuple.Item1, tuple2.Item1 } });
						}
						else if (!tuple.Item2.Equals(tuple2.Item2))
						{
							LogMismatch(list, isMPCompare, 208, "State GoalChestReward has key with differing Reward values.", new List<string[]> { new string[3] { "GoalChestReward Key", tuple.Item1, tuple2.Item1 } });
						}
					}
				}
				catch (Exception ex16)
				{
					list.Add(new Tuple<int, string>(210, "Exception during GoalChestRewards compare.\n" + ex16.Message + "\n" + ex16.StackTrace));
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.Enemy2Monsters, state2.Enemy2Monsters))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 211, "Enemy2Monsters List is null on one or both states.", new List<string[]> { new string[3]
				{
					"Enemy2Monsters",
					(state1.Enemy2Monsters == null) ? "is null" : "is not null",
					(state2.Enemy2Monsters == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Enemy2Monsters.Any((EnemyState a) => a == null) || state2.Enemy2Monsters.Any((EnemyState a) => a == null))
				{
					LogMismatch(list, isMPCompare, 212, "Scenario State contains null Enemy2Monsters.", new List<string[]> { new string[3]
					{
						"Enemy2Monsters",
						string.Join(", ", state1.Enemy2Monsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString())),
						string.Join(", ", state2.Enemy2Monsters.Select((EnemyState s) => (s == null) ? "null" : s.ClassID.ToString()))
					} });
					break;
				}
				bool flag24 = false;
				foreach (EnemyState monster16 in state1.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state1.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid == monster16.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 213, "Duplicate Actor GUID in " + GetStateName(isState1: true, isMPCompare) + " state Enemy2Monsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", monster16.ActorGuid, "NA" },
							new string[3]
							{
								"Standee ID",
								monster16.ID.ToString(),
								"NA"
							},
							new string[3] { "Class ID", monster16.ClassID, "NA" },
							new string[3]
							{
								"Location",
								monster16.Location.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate GUID Count",
								state1.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid == monster16.ActorGuid).Count().ToString(),
								"NA"
							}
						});
						flag24 = true;
					}
				}
				foreach (EnemyState monster17 in state2.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid != null))
				{
					if (state2.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid == monster17.ActorGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 214, "Duplicate Actor GUID in " + GetStateName(isState1: false, isMPCompare) + " state Enemy2Monsters list.", new List<string[]>
						{
							new string[3] { "Enemy GUID", "NA", monster17.ActorGuid },
							new string[3]
							{
								"Standee ID",
								"NA",
								monster17.ID.ToString()
							},
							new string[3] { "Class ID", "NA", monster17.ClassID },
							new string[3]
							{
								"Location",
								"NA",
								monster17.Location.ToString()
							},
							new string[3]
							{
								"Duplicate GUID Count",
								"NA",
								state2.Enemy2Monsters.Where((EnemyState w) => w.ActorGuid == monster17.ActorGuid).Count().ToString()
							}
						});
						flag24 = true;
					}
				}
				if (state1.Enemy2Monsters.Count != state2.Enemy2Monsters.Count)
				{
					LogMismatch(list, isMPCompare, 215, "Number of Enemy2Monsters does not match.", new List<string[]>
					{
						new string[3]
						{
							"Enemy2Monsters Count",
							state1.Enemy2Monsters.Count.ToString(),
							state2.Enemy2Monsters.Count.ToString()
						},
						new string[3]
						{
							"Enemy2Monsters",
							string.Join(", ", state1.Enemy2Monsters.Select((EnemyState s) => s.ClassID)),
							string.Join(", ", state2.Enemy2Monsters.Select((EnemyState s) => s.ClassID))
						}
					});
				}
				else
				{
					if (flag24)
					{
						break;
					}
					bool flag25 = false;
					foreach (EnemyState monster18 in state1.Enemy2Monsters)
					{
						if (!state2.Enemy2Monsters.Exists((EnemyState e) => e.ActorGuid == monster18.ActorGuid && TileIndex.Compare(e.Location, monster18.Location)))
						{
							LogMismatch(list, isMPCompare, 216, "Enemy2Monsters in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Enemy2Monsters GUID", monster18.ActorGuid, "Missing" },
								new string[3]
								{
									"Standee ID",
									monster18.ID.ToString(),
									"Missing"
								},
								new string[3] { "Class ID", monster18.ClassID, "Missing" },
								new string[3]
								{
									"Location",
									monster18.Location.ToString(),
									"Missing"
								}
							});
							flag25 = true;
						}
					}
					foreach (EnemyState monster19 in state2.Enemy2Monsters)
					{
						if (!state1.Enemy2Monsters.Exists((EnemyState e) => e.ActorGuid == monster19.ActorGuid && TileIndex.Compare(e.Location, monster19.Location)))
						{
							LogMismatch(list, isMPCompare, 217, "Enemy2Monsters in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Enemy2Monsters GUID", "Missing", monster19.ActorGuid },
								new string[3]
								{
									"Standee ID",
									"Missing",
									monster19.ID.ToString()
								},
								new string[3] { "Class ID", "Missing", monster19.ClassID },
								new string[3]
								{
									"Location",
									"Missing",
									monster19.Location.ToString()
								}
							});
							flag25 = true;
						}
					}
					if (flag25)
					{
						break;
					}
					foreach (EnemyState monster20 in state1.Enemy2Monsters)
					{
						try
						{
							EnemyState state10 = state2.Enemy2Monsters.Single((EnemyState s) => s.ActorGuid == monster20.ActorGuid && TileIndex.Compare(s.Location, monster20.Location));
							list.AddRange(EnemyState.Compare(monster20, state10, isMPCompare));
						}
						catch (Exception ex17)
						{
							list.Add(new Tuple<int, string>(218, "Exception during Enemy2Monsters compare.\n" + ex17.Message + "\n" + ex17.StackTrace));
						}
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(state1.DestroyedProps, state2.DestroyedProps))
			{
			case StateShared.ENullStatus.BothNull:
			case StateShared.ENullStatus.Mismatch:
				LogMismatch(list, isMPCompare, 219, "DestroyedProps List is null on one or both states.", new List<string[]> { new string[3]
				{
					"DestroyedProps",
					(state1.DestroyedProps == null) ? "is null" : "is not null",
					(state2.DestroyedProps == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.DestroyedProps.Any((CObjectProp a) => a == null) || state2.DestroyedProps.Any((CObjectProp a) => a == null))
				{
					LogMismatch(list, isMPCompare, 220, "Scenario State contains null DestroyedProps.", new List<string[]> { new string[3]
					{
						"DestroyedProps",
						string.Join(", ", state1.DestroyedProps.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString())),
						string.Join(", ", state2.DestroyedProps.Select((CObjectProp s) => (s == null) ? "null" : s.PropType.ToString()))
					} });
					break;
				}
				bool flag26 = false;
				foreach (CObjectProp object21 in state1.DestroyedProps)
				{
					if (state1.DestroyedProps.Where((CObjectProp w) => w.PropGuid == object21.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 221, "Duplicate entry in " + GetStateName(isState1: true, isMPCompare) + " state DestroyedProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", object21.PropGuid, "NA" },
							new string[3]
							{
								"Prop Type",
								object21.PropType.ToString(),
								"NA"
							},
							new string[3]
							{
								"Duplicate Count",
								state1.DestroyedProps.Where((CObjectProp w) => w.PropGuid == object21.PropGuid).Count().ToString(),
								"NA"
							}
						});
						flag26 = true;
					}
				}
				foreach (CObjectProp object22 in state2.DestroyedProps)
				{
					if (state2.DestroyedProps.Where((CObjectProp w) => w.PropGuid == object22.PropGuid).Count() > 1)
					{
						LogMismatch(list, isMPCompare, 222, "Duplicate entry in " + GetStateName(isState1: false, isMPCompare) + " state DestroyedProps list.", new List<string[]>
						{
							new string[3] { "Prop GUID", "NA", object22.PropGuid },
							new string[3]
							{
								"Prop Type",
								"NA",
								object22.PropType.ToString()
							},
							new string[3]
							{
								"Duplicate Count",
								"NA",
								state2.DestroyedProps.Where((CObjectProp w) => w.PropGuid == object22.PropGuid).Count().ToString()
							}
						});
						flag26 = true;
					}
				}
				if (state1.DestroyedProps.Count != state2.DestroyedProps.Count)
				{
					LogMismatch(list, isMPCompare, 223, "Number of DestroyedProps does not match.", new List<string[]>
					{
						new string[3]
						{
							"DestroyedProps Count",
							state1.DestroyedProps.Count.ToString(),
							state2.DestroyedProps.Count.ToString()
						},
						new string[3]
						{
							"DestroyedProps",
							string.Join(", ", state1.DestroyedProps.Select((CObjectProp s) => s.PropType.ToString())),
							string.Join(", ", state2.DestroyedProps.Select((CObjectProp s) => s.PropType.ToString()))
						}
					});
				}
				else
				{
					if (flag26)
					{
						break;
					}
					bool flag27 = false;
					foreach (CObjectProp object23 in state1.DestroyedProps)
					{
						if (!state2.DestroyedProps.Exists((CObjectProp e) => e.PropGuid == object23.PropGuid))
						{
							LogMismatch(list, isMPCompare, 224, "ActivatedProp in " + GetStateName(isState1: true, isMPCompare) + " state could not be found in " + GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", object23.PropGuid, "Missing" },
								new string[3]
								{
									"Prop Type",
									object23.PropType.ToString(),
									"Missing"
								}
							});
							flag27 = true;
						}
					}
					foreach (CObjectProp object24 in state2.DestroyedProps)
					{
						if (!state1.DestroyedProps.Exists((CObjectProp e) => e.PropGuid == object24.PropGuid))
						{
							LogMismatch(list, isMPCompare, 225, "ActivatedProp in " + GetStateName(isState1: false, isMPCompare) + " state could not be found in " + GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
							{
								new string[3] { "Prop GUID", "Missing", object24.PropGuid },
								new string[3]
								{
									"Prop Type",
									"Missing",
									object24.PropType.ToString()
								}
							});
							flag27 = true;
						}
					}
					if (flag27)
					{
						break;
					}
					foreach (CObjectProp object25 in state1.DestroyedProps)
					{
						try
						{
							CObjectProp cObjectProp3 = state2.DestroyedProps.Single((CObjectProp s) => s.PropGuid == object25.PropGuid);
							if (object25 is CObjectChest)
							{
								list.AddRange(CObjectChest.Compare(object25 as CObjectChest, cObjectProp3 as CObjectChest, isMPCompare));
							}
							else if (object25 is CObjectDoor)
							{
								list.AddRange(CObjectDoor.Compare(object25 as CObjectDoor, cObjectProp3 as CObjectDoor, isMPCompare));
							}
							else if (object25 is CObjectGoldPile)
							{
								list.AddRange(CObjectGoldPile.Compare(object25 as CObjectGoldPile, cObjectProp3 as CObjectGoldPile, isMPCompare));
							}
							else if (object25 is CObjectObstacle)
							{
								list.AddRange(CObjectObstacle.Compare(object25 as CObjectObstacle, cObjectProp3 as CObjectObstacle, isMPCompare));
							}
							else if (object25 is CObjectTrap)
							{
								list.AddRange(CObjectTrap.Compare(object25 as CObjectTrap, cObjectProp3 as CObjectTrap, isMPCompare));
							}
							else
							{
								list.AddRange(CObjectProp.Compare(object25, cObjectProp3, isMPCompare));
							}
						}
						catch (Exception ex18)
						{
							list.Add(new Tuple<int, string>(226, "Exception during DestroyedProps compare.\n" + ex18.Message + "\n" + ex18.StackTrace));
						}
					}
				}
				break;
			}
			}
		}
		catch (Exception ex19)
		{
			list.Add(new Tuple<int, string>(299, "Exception during State compare.\n" + ex19.Message + "\n" + ex19.StackTrace));
		}
		return list;
	}

	public static void LogMismatch(List<Tuple<int, string>> mismatches, bool isMPCompare, int errorCode, string message, List<string[]> props)
	{
		try
		{
			message = message + "\n" + GetStateName(isState1: true, isMPCompare) + " Properties:";
			foreach (string[] prop in props)
			{
				message = message + "\n" + prop[0] + ": " + prop[1];
			}
			message = message + "\n" + GetStateName(isState1: false, isMPCompare) + " Properties:";
			foreach (string[] prop2 in props)
			{
				message = message + "\n" + prop2[0] + ": " + prop2[2];
			}
			mismatches.Add(new Tuple<int, string>(errorCode, message));
		}
		catch
		{
			mismatches.Add(new Tuple<int, string>(errorCode, "Failed to format mismatch output"));
		}
	}

	public static string GetStateName(bool isState1, bool isMPCompare)
	{
		if (isMPCompare)
		{
			if (isState1)
			{
				return "Client";
			}
			return "Host";
		}
		if (isState1)
		{
			return "Current State";
		}
		return "Saved State";
	}
}
