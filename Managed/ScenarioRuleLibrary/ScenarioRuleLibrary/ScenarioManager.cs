using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class ScenarioManager
{
	[Serializable]
	public enum EDLLMode
	{
		None,
		Campaign,
		Guildmaster,
		CustomScenario,
		Mod
	}

	[Serializable]
	public enum ObjectImportType
	{
		Hero,
		Monster,
		Tile,
		EdgeTile,
		Door,
		GenericProp,
		Chest,
		GoalChest,
		MoneyToken,
		Trap,
		Obstacle,
		Coverage,
		HeroSummons,
		Spawner,
		PressurePlate,
		TerrainHotCoals,
		TerrainWater,
		TerrainRubble,
		TerrainThorns,
		Portal,
		CarryableQuestItem,
		TerrainVisualEffect,
		Resource,
		MonsterGrave,
		None
	}

	public enum EAdjacentPosition
	{
		ECenter,
		ELeft,
		ETopLeft,
		ETopRight,
		ERight,
		EBottomLeft,
		EBottomRight
	}

	[Serializable]
	public enum EPerPartySizeConfig
	{
		Normal,
		Hidden,
		ToElite
	}

	public static readonly EDLLMode[] DLLModes = (EDLLMode[])Enum.GetValues(typeof(EDLLMode));

	public static readonly ObjectImportType[] ObjectImportTypes = (ObjectImportType[])Enum.GetValues(typeof(ObjectImportType));

	public static readonly EAdjacentPosition[] AdjacentPositionTypes = (EAdjacentPosition[])Enum.GetValues(typeof(EAdjacentPosition));

	private static int s_Width;

	private static int s_Height;

	private static CPathFinder s_PathFinder;

	private static CTile[,] s_TileArray;

	private static CScenario s_Scenario;

	private static bool s_IsFirstLoad;

	private static List<CTile> s_StartingTiles;

	public static CTile[,] Tiles => s_TileArray;

	public static List<CTile> StartingTiles => s_StartingTiles;

	public static int Width => s_Width;

	public static int Height => s_Height;

	public static CPathFinder PathFinder => s_PathFinder;

	public static CScenario Scenario => s_Scenario;

	public static bool IsFirstLoad => s_IsFirstLoad;

	public static EDLLMode Mode { get; set; }

	public static StateShared.EHouseRulesFlag HouseRulesSettings { get; set; }

	public static ScenarioState CurrentScenarioState { get; set; }

	public static void Load(ScenarioState scenarioState, ScenarioLevelTable scenarioLevelTable, bool firstLoad)
	{
		s_IsFirstLoad = firstLoad;
		CurrentScenarioState = scenarioState;
		foreach (CMap map in scenarioState.Maps)
		{
			map.Reset();
		}
		s_Scenario = new CScenario(scenarioState.ID, scenarioState.Name, scenarioState.Level, scenarioState.PositiveSpaceOffset, scenarioLevelTable.Entries[scenarioState.Level]);
		SimpleLog.AddToSimpleLog("[Scenario Level]: (Loading Scenario State) Current Scenario level is " + scenarioState.Level);
		s_Width = scenarioState.Width + 2;
		s_Height = scenarioState.Height + 2;
		s_TileArray = new CTile[s_Width, s_Height];
		s_PathFinder = new CPathFinder(s_Width, s_Height, oddEvenCheck: true);
		foreach (CMap map2 in scenarioState.Maps)
		{
			s_Scenario.AddMap(map2);
			foreach (CMapTile mapTile in map2.MapTiles)
			{
				if (s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y] == null)
				{
					CTile cTile = new CTile
					{
						m_HexMap = map2,
						m_Hex = mapTile,
						m_ArrayIndex = new Point(mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y)
					};
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y] = cTile;
				}
				else
				{
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].m_Hex2Map = map2;
					s_TileArray[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].m_Hex2 = mapTile;
				}
				s_PathFinder.Nodes[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y].IslandID = map2.IslandID;
			}
		}
		for (int i = 0; i < s_Height; i++)
		{
			for (int j = 0; j < s_Width; j++)
			{
				CTile cTile2 = s_TileArray[j, i];
				if (cTile2 != null && !cTile2.m_Hex.FlagsSet(EFlags.Blocked | EFlags.Edge))
				{
					s_PathFinder.Nodes[j, i].Walkable = true;
					s_PathFinder.Nodes[j, i].Blocked = false;
				}
			}
		}
	}

	public static void ResetCustomIDs(ScenarioState currentScenarioState)
	{
		foreach (CMap map in currentScenarioState.Maps)
		{
			foreach (EnemyState monster in map.Monsters)
			{
				CMonsterClass cMonsterClass = MonsterClassManager.Find(monster.ClassID);
				if (cMonsterClass != null)
				{
					cMonsterClass.RecycleID(monster.ID);
					monster.ID = 0;
				}
			}
			foreach (EnemyState allyMonster in map.AllyMonsters)
			{
				CMonsterClass cMonsterClass2 = MonsterClassManager.Find(allyMonster.ClassID);
				if (cMonsterClass2 != null)
				{
					cMonsterClass2.RecycleID(allyMonster.ID);
					allyMonster.ID = 0;
				}
			}
			foreach (EnemyState enemy2Monster in map.Enemy2Monsters)
			{
				CMonsterClass cMonsterClass3 = MonsterClassManager.Find(enemy2Monster.ClassID);
				if (cMonsterClass3 != null)
				{
					cMonsterClass3.RecycleID(enemy2Monster.ID);
					enemy2Monster.ID = 0;
				}
			}
			foreach (EnemyState neutralMonster in map.NeutralMonsters)
			{
				CMonsterClass cMonsterClass4 = MonsterClassManager.Find(neutralMonster.ClassID);
				if (cMonsterClass4 != null)
				{
					cMonsterClass4.RecycleID(neutralMonster.ID);
					neutralMonster.ID = 0;
				}
			}
		}
		if (currentScenarioState.EnemyClassManager == null)
		{
			return;
		}
		foreach (EnemyClassState enemyClass in currentScenarioState.EnemyClassManager.EnemyClasses)
		{
			enemyClass.ResetIDs();
		}
	}

	public static void SetHouseRules(StateShared.EHouseRulesFlag houseRulesSettings)
	{
		HouseRulesSettings = houseRulesSettings;
		SimpleLog.AddToSimpleLog("[HOUSE RULES] - changed house rules settings in ScenarioManager to " + houseRulesSettings);
	}

	public static void Reset()
	{
		ElementInfusionBoardManager.Reset();
		CharacterClassManager.Reset();
		MonsterClassManager.Reset();
	}

	public static void InitScenario(bool levelEditing)
	{
		ScenarioRuleClient.Reset();
		if (!levelEditing && CurrentScenarioState.IsFirstLoad)
		{
			if (GameState.RandomiseOnLoad)
			{
				if (CurrentScenarioState != null)
				{
					CurrentScenarioState.RandomiseRNGOnLoad();
				}
				ResetCustomIDs(CurrentScenarioState);
			}
			foreach (EnemyState allEnemyState in CurrentScenarioState.AllEnemyStates)
			{
				allEnemyState.IsRevealed = false;
			}
		}
		foreach (CObjectProp prop in CurrentScenarioState.Props)
		{
			if ((levelEditing || prop.GetConfigForPartySize(CurrentScenarioState.Players.Count) != EPerPartySizeConfig.Hidden) && s_TileArray[prop.ArrayIndex.X, prop.ArrayIndex.Y] != null)
			{
				s_TileArray[prop.ArrayIndex.X, prop.ArrayIndex.Y].SpawnProp(prop, notifyClient: false);
			}
		}
		foreach (CSpawner spawner in CurrentScenarioState.Spawners)
		{
			if (levelEditing || spawner.GetConfigForPartySize(CurrentScenarioState.Players.Count) != EPerPartySizeConfig.Hidden)
			{
				s_TileArray[spawner.ArrayIndex.X, spawner.ArrayIndex.Y].SpawnSpawner(spawner);
				spawner.OnScenarioStart(CurrentScenarioState.IsFirstLoad);
				if (spawner.Prop != null && spawner.StartingMap.Revealed)
				{
					CSpawn_MessageData message = new CSpawn_MessageData(null)
					{
						m_SpawnDelay = 0f,
						m_Prop = spawner.Prop
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		CurrentScenarioState.ApplyClassStates();
		if (levelEditing)
		{
			foreach (CMap map in CurrentScenarioState.Maps)
			{
				bool revealed = map.Revealed;
				map.Reveal(initial: true, noIDRegen: true, forLevelEditor: true);
				map.Revealed = revealed;
			}
		}
		else
		{
			foreach (CMap item in CurrentScenarioState.Maps.Where((CMap w) => w.Revealed || w.IsAdditionalDungeonEntranceRoom || w.Players.Any((PlayerState p) => p.IsRevealed && !p.HiddenAtStart) || w.Monsters.Any((EnemyState m) => m.IsRevealed) || w.HeroSummons.Any((HeroSummonState h) => h.IsRevealed)))
			{
				item.Reveal(initial: true, noIDRegen: true);
				if (item.Destroyed)
				{
					CDestroyRoom_MessageData cDestroyRoom_MessageData = new CDestroyRoom_MessageData();
					cDestroyRoom_MessageData.m_MapToDestroy = item;
					ScenarioRuleClient.MessageHandler(cDestroyRoom_MessageData);
				}
			}
		}
		foreach (CObjectDoor item2 in CurrentScenarioState.Props.OfType<CObjectDoor>())
		{
			item2.InitDoor();
			if (!s_IsFirstLoad && item2.Activated)
			{
				if (PathFinder != null && PathFinder.Nodes != null)
				{
					PathFinder.Nodes[item2.ArrayIndex.X, item2.ArrayIndex.Y].IsBridgeOpen = true;
				}
				CActivateProp_MessageData message2 = new CActivateProp_MessageData(null)
				{
					m_Prop = item2,
					m_InitialLoad = true
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
		}
		foreach (CObjectDifficultTerrain item3 in CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
		{
			item3.InitDifficultTerrain();
		}
		Scenario.PlayerActors.Clear();
		foreach (CPlayerActor initialPlayer in Scenario.InitialPlayers)
		{
			Scenario.PlayerActors.Add(initialPlayer.Clone());
		}
		Scenario.HeroSummons.Clear();
		foreach (CHeroSummonActor initialHeroSummon in Scenario.InitialHeroSummons)
		{
			Scenario.HeroSummons.Add(initialHeroSummon.Clone());
		}
		Scenario.Enemies.Clear();
		foreach (CEnemyActor initialEnemy in Scenario.InitialEnemies)
		{
			Scenario.Enemies.Add(initialEnemy.Clone());
		}
		Scenario.AllyMonsters.Clear();
		foreach (CEnemyActor initialAllyMonster in Scenario.InitialAllyMonsters)
		{
			Scenario.AllyMonsters.Add(initialAllyMonster.Clone());
		}
		Scenario.Enemy2Monsters.Clear();
		foreach (CEnemyActor initialEnemy2Monster in Scenario.InitialEnemy2Monsters)
		{
			Scenario.Enemy2Monsters.Add(initialEnemy2Monster.Clone());
		}
		Scenario.NeutralMonsters.Clear();
		foreach (CEnemyActor initialNeutralMonster in Scenario.InitialNeutralMonsters)
		{
			Scenario.NeutralMonsters.Add(initialNeutralMonster.Clone());
		}
		Scenario.Objects.Clear();
		foreach (CObjectActor initialObject in Scenario.InitialObjects)
		{
			CObjectActor cObjectActor = initialObject.Clone();
			Scenario.Objects.Add(cObjectActor);
			if (cObjectActor.AttachedProp != null)
			{
				cObjectActor.AttachedProp.SetActorAttachedAtRuntime(cObjectActor);
			}
		}
		CurrentScenarioState.Apply();
		if (GameState.RandomiseOnLoad && CurrentScenarioState.IsFirstLoad && CurrentScenarioState != null)
		{
			CurrentScenarioState.RandomiseDecksOnLoad();
		}
		CurrentScenarioState.CheckObjectivesComplete();
	}

	public static void SetStartingTiles(List<CTile> startingTiles)
	{
		s_StartingTiles = startingTiles;
	}

	public static EAdjacentPosition GetOppositeDirection(EAdjacentPosition currentDirection)
	{
		return currentDirection switch
		{
			EAdjacentPosition.ELeft => EAdjacentPosition.ERight, 
			EAdjacentPosition.ERight => EAdjacentPosition.ELeft, 
			EAdjacentPosition.ETopLeft => EAdjacentPosition.EBottomRight, 
			EAdjacentPosition.ETopRight => EAdjacentPosition.EBottomLeft, 
			EAdjacentPosition.EBottomLeft => EAdjacentPosition.ETopRight, 
			EAdjacentPosition.EBottomRight => EAdjacentPosition.ETopLeft, 
			_ => EAdjacentPosition.ECenter, 
		};
	}

	public static CTile GetAdjacentTile(int x, int y, EAdjacentPosition eposition)
	{
		int num = x;
		int num2 = y;
		switch (eposition)
		{
		case EAdjacentPosition.ELeft:
			num--;
			break;
		case EAdjacentPosition.ERight:
			num++;
			break;
		case EAdjacentPosition.ETopLeft:
			num2++;
			break;
		case EAdjacentPosition.ETopRight:
			num++;
			num2++;
			break;
		case EAdjacentPosition.EBottomLeft:
			num2--;
			break;
		case EAdjacentPosition.EBottomRight:
			num++;
			num2--;
			break;
		}
		int num3 = (((num2 & 1) != 0) ? (Width - 2) : (Width - 1));
		if (num2 != y)
		{
			num = (((num2 & 1) != 0) ? (num - 1) : num);
		}
		if (num >= 0 && num2 >= 0 && num <= num3 && num2 <= Height - 1)
		{
			return Tiles[num, num2];
		}
		return null;
	}

	public static List<CTile> GetAllAdjacentTiles(CTile tile)
	{
		List<CTile> list = new List<CTile>();
		CTile adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.EBottomLeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.EBottomRight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.ELeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.ERight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.ETopLeft);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		adjacentTile = GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, EAdjacentPosition.ETopRight);
		if (adjacentTile != null)
		{
			list.Add(adjacentTile);
		}
		return list;
	}

	public static List<CTile> GetAllUnblockedTilesFromOrigin(CTile originTile, int maxDepth = 100, List<CTile> tempTiles = null)
	{
		List<CTile> unblockedTiles = new List<CTile>();
		unblockedTiles.Add(originTile);
		return GetAllUnblockedTilesRecursive(ref unblockedTiles, ref originTile, 1, maxDepth, tempTiles);
	}

	public static List<CTile> GetAllUnblockedTilesRecursive(ref List<CTile> unblockedTiles, ref CTile currentTile, int currentDepth, int maxDepth, List<CTile> tempTiles)
	{
		foreach (CTile allAdjacentTile in GetAllAdjacentTiles(currentTile))
		{
			CTile currentTile2 = allAdjacentTile;
			CObjectDoor cObjectDoor = allAdjacentTile.FindProp(ObjectImportType.Door) as CObjectDoor;
			if (unblockedTiles.Contains(allAdjacentTile) || !allAdjacentTile.IsMapShared(currentTile))
			{
				continue;
			}
			if (cObjectDoor != null && !cObjectDoor.DoorIsOpen)
			{
				unblockedTiles.Add(allAdjacentTile);
			}
			else if ((!PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || (tempTiles != null && tempTiles.Contains(allAdjacentTile))) && !PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].TransientBlocked && PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Walkable)
			{
				unblockedTiles.Add(allAdjacentTile);
				if (currentDepth < maxDepth)
				{
					unblockedTiles = GetAllUnblockedTilesRecursive(ref unblockedTiles, ref currentTile2, currentDepth + 1, maxDepth, tempTiles);
				}
				else
				{
					DLLDebug.LogWarning("GetAllUnblockedTilesRecursive Max depth hit");
				}
			}
		}
		return unblockedTiles;
	}

	public static List<CActor> GetAllAdjacentActors(CActor actor)
	{
		List<CActor> list = new List<CActor>();
		CActor adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.EBottomLeft);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.EBottomRight);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.ELeft);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.ERight);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.ETopLeft);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		adjacentActor = GetAdjacentActor(actor, EAdjacentPosition.ETopRight);
		if (adjacentActor != null)
		{
			list.Add(adjacentActor);
		}
		return list;
	}

	private static CActor GetAdjacentActor(CActor actor, EAdjacentPosition position)
	{
		CTile adjacentTile = GetAdjacentTile(actor.ArrayIndex.X, actor.ArrayIndex.Y, position);
		CTile cTile = Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
		CNode cNode = ((cTile != null) ? PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y] : null);
		CNode cNode2 = ((adjacentTile != null) ? PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
		if (adjacentTile != null && (cTile.m_HexMap == adjacentTile.m_HexMap || (cTile.m_HexMap != adjacentTile.m_HexMap && (cNode.IsBridge || cNode2.IsBridge))))
		{
			return Scenario.FindActorAt(adjacentTile.m_ArrayIndex);
		}
		return null;
	}

	public static EAdjacentPosition GetAdjacentPosition(int x, int y, int newx, int newy)
	{
		if (newy != y)
		{
			newx = (((newy & 1) != 0) ? (newx + 1) : newx);
		}
		if (newx + 1 == x && newy == y)
		{
			return EAdjacentPosition.ELeft;
		}
		if (newx - 1 == x && newy == y)
		{
			return EAdjacentPosition.ERight;
		}
		if (newx == x && newy - 1 == y)
		{
			return EAdjacentPosition.ETopLeft;
		}
		if (newx - 1 == x && newy - 1 == y)
		{
			return EAdjacentPosition.ETopRight;
		}
		if (newx == x && newy + 1 == y)
		{
			return EAdjacentPosition.EBottomLeft;
		}
		if (newx - 1 == x && newy + 1 == y)
		{
			return EAdjacentPosition.EBottomRight;
		}
		return EAdjacentPosition.ECenter;
	}

	public static bool IsTileAdjacent(int sourceX, int sourceY, int targetX, int targetY, bool ignoreWalls = false)
	{
		for (EAdjacentPosition eAdjacentPosition = EAdjacentPosition.ELeft; eAdjacentPosition <= EAdjacentPosition.EBottomRight; eAdjacentPosition++)
		{
			CTile adjacentTile = GetAdjacentTile(sourceX, sourceY, eAdjacentPosition);
			if (adjacentTile != null && targetX == adjacentTile.m_ArrayIndex.X && targetY == adjacentTile.m_ArrayIndex.Y)
			{
				CTile cTile = Tiles[sourceX, sourceY];
				CNode cNode = PathFinder.Nodes[sourceX, sourceY];
				CNode cNode2 = PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if (!ignoreWalls && cTile.m_HexMap != adjacentTile.m_HexMap && !cNode.IsBridge && !cNode2.IsBridge)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public static int GetTileDistance(int x1, int y1, int x2, int y2)
	{
		int num = x2 - x1;
		int value = y2 - y1;
		int num2 = Math.Abs(num);
		int num3 = Math.Abs(value);
		num2 = ((!((num < 0) ^ ((y1 & 1) == 1))) ? Math.Max(0, num2 - num3 / 2) : Math.Max(0, num2 - (num3 + 1) / 2));
		return num2 + num3;
	}

	public static bool RoomsBetweenTilesRevealed(CTile originTile, CTile destinationTile)
	{
		if (originTile.IsMapShared(destinationTile))
		{
			return true;
		}
		List<CMap> checkedMaps = new List<CMap> { originTile.m_HexMap };
		return RoomsBetweenTilesRevealedRecursive(foundRoomPath: false, originTile.m_HexMap, destinationTile, ref checkedMaps);
	}

	private static bool RoomsBetweenTilesRevealedRecursive(bool foundRoomPath, CMap currentMap, CTile destinationTile, ref List<CMap> checkedMaps)
	{
		foreach (CObjectDoor doorProp in CurrentScenarioState.DoorProps)
		{
			if ((doorProp.HexMap != currentMap && doorProp.Hex2Map != currentMap) || !doorProp.DoorIsOpen)
			{
				continue;
			}
			CTile cTile = Tiles[doorProp.ArrayIndex.X, doorProp.ArrayIndex.Y];
			CMap cMap = ((cTile.m_HexMap == currentMap) ? cTile.m_Hex2Map : cTile.m_HexMap);
			if (cMap == null)
			{
				continue;
			}
			if (cMap == destinationTile.m_HexMap)
			{
				foundRoomPath = true;
				break;
			}
			if (!checkedMaps.Contains(cMap))
			{
				checkedMaps.Add(cMap);
				foundRoomPath = RoomsBetweenTilesRevealedRecursive(foundRoomPath, cMap, destinationTile, ref checkedMaps);
				if (foundRoomPath)
				{
					break;
				}
			}
		}
		return foundRoomPath;
	}

	public static void RevealAllMaps()
	{
		foreach (CObjectDoor item in from w in CurrentScenarioState.Props.OfType<CObjectDoor>()
			where !w.Activated && !w.IsDungeonEntrance
			select w)
		{
			item.ForceActivate(null);
		}
	}

	public static CActor FindActor(string actorGuid)
	{
		if (Scenario != null)
		{
			return Scenario.AllActors.SingleOrDefault((CActor s) => s.ActorGuid == actorGuid);
		}
		DLLDebug.LogError("Scenario is null, unable to find actor by actor guid.");
		return null;
	}

	public static CActor FindActorWithAbilityCard(CAbilityCard card)
	{
		return Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.CharacterClass.AbilityCardsPool.Exists((CAbilityCard e) => e.ID == card.ID));
	}

	public static CActor FindActorWithActivatedItemFromID(int itemID, CCharacterClass characterClass)
	{
		List<CPlayerActor> list = Scenario.AllPlayers.Where((CPlayerActor w) => w.Inventory.AllItems.Exists((CItem e) => e.ID == itemID && e.SlotState == CItem.EItemSlotState.Active)).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		CPlayerActor cPlayerActor = list.SingleOrDefault((CPlayerActor s) => s.CharacterClass.ID == characterClass.ID);
		if (cPlayerActor != null)
		{
			return cPlayerActor;
		}
		return null;
	}

	public static List<CTile> GetTilesInLine(int x, int y, int range, EAdjacentPosition eposition)
	{
		List<CTile> list = new List<CTile>();
		int x2 = x;
		int y2 = y;
		for (int i = 0; i < range; i++)
		{
			if (i != 0)
			{
				x2 = list.Last().m_ArrayIndex.X;
				y2 = list.Last().m_ArrayIndex.Y;
			}
			CTile adjacentTile = GetAdjacentTile(x2, y2, eposition);
			if (adjacentTile == null)
			{
				break;
			}
			list.Add(adjacentTile);
		}
		return list;
	}
}
