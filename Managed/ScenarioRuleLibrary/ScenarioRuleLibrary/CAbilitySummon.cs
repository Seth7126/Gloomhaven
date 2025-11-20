using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilitySummon : CAbility
{
	public enum SummonState
	{
		SelectSummonPosition,
		Summon,
		NoTilesAvailable,
		SummonDone
	}

	public class CSummonActorPath
	{
		public List<Point> m_ArrayIndices;

		public List<CTile> m_Waypoints;

		public CTile m_Tile;
	}

	private SummonState m_State;

	private bool m_Summoned;

	public List<CActor> SummonedActors { get; set; }

	public List<string> SummonIDs { get; set; }

	public string SelectedSummonID
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState == null)
			{
				return SummonIDs[0];
			}
			return SummonIDs[ScenarioManager.CurrentScenarioState.Players.Count - 1];
		}
	}

	public HeroSummonYMLData SelectedSummonYMLData => ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == SelectedSummonID);

	public MonsterYMLData SelectedMonsterYMLData => ScenarioRuleClient.SRLYML.GetMonsterData(SelectedSummonID);

	public bool IsMonsterSummon => SelectedMonsterYMLData != null;

	public string SelectedLocKey
	{
		get
		{
			if (!IsMonsterSummon)
			{
				return SelectedSummonYMLData.LocKey;
			}
			return SelectedMonsterYMLData.LocKey;
		}
	}

	public CAbilitySummon(List<string> summons)
	{
		if (summons.Count != 4)
		{
			throw new Exception("Invalid size for summons list.  Must be exactly 4.  Current size: " + summons.Count);
		}
		SummonIDs = summons;
		SummonedActors = new List<CActor>();
		m_Range = 1;
	}

	public static CAbility CreateCompanionSummon(string companionSummonID)
	{
		return CAbility.CreateAbility(EAbilityType.Summon, 1, useSpecialBaseStat: false, 1, 1, new CAbilityFilterContainer(), "Summon", null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "DefaultAttack", new List<CEnhancement>(), null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, new List<string> { companionSummonID, companionSummonID, companionSummonID, companionSummonID }, null, EAbilityTargeting.Range, null, isMonster: false, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, new List<EAbilityType>(), new List<EAttackType>(), null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: true);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (!IsMonsterSummon && SelectedSummonYMLData != null)
		{
			SelectedSummonYMLData.ResetEnhancementBonuses();
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_State = SummonState.SelectSummonPosition;
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon)
		{
			m_CanUndo = false;
			m_CanSkip = false;
		}
		m_Summoned = false;
		if (base.UseSubAbilityTargeting && base.IsInlineSubAbility)
		{
			foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
			{
				if (ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex) == null)
				{
					m_TilesSelected.Add(inlineSubAbilityTile);
				}
			}
			if (m_TilesSelected.Count > 0)
			{
				m_State = SummonState.Summon;
			}
			else
			{
				m_CancelAbility = true;
			}
		}
		if (SelectedSummonID == "Empty")
		{
			m_CancelAbility = true;
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0075;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0075;
				}
			}
		}
		if (m_CancelAbility)
		{
			PhaseManager.NextStep();
			return true;
		}
		List<CTile> list2;
		List<string> list3;
		List<CTile> list4;
		switch (m_State)
		{
		case SummonState.SelectSummonPosition:
		{
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (base.AreaEffect == null)
				{
					base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: true, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: true);
					List<CTile> list = new List<CTile>();
					foreach (CTile item3 in base.TilesInRange)
					{
						if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.EmptyHex) && item3.m_HexMap.Revealed)
						{
							list.Add(item3);
						}
					}
					base.TilesInRange = list.ToList();
				}
				for (int num = base.TilesInRange.Count - 1; num >= 0; num--)
				{
					CTile cTile = base.TilesInRange[num];
					if (ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex) != null)
					{
						base.TilesInRange.Remove(cTile);
					}
				}
				CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.HeroSummons,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message2);
				break;
			}
			list2 = new List<CTile>();
			list3 = new List<string>();
			list4 = new List<CTile>();
			AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.UseParentTiles.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
				if (miscAbilityData3 != null && miscAbilityData3.UseParentTiles.Value && base.ParentAbility != null && base.ParentAbility.ActorsTargeted != null && base.ParentAbility.ActorsTargeted.Count > 0 && base.ParentAbility.AreaEffect == null)
				{
					CTile propTile = null;
					foreach (CActor item4 in base.ParentAbility.ActorsTargeted)
					{
						CTile cTile2 = ScenarioManager.Tiles[item4.ArrayIndex.X, item4.ArrayIndex.Y];
						if (!list4.Contains(cTile2) && ScenarioManager.PathFinder.Nodes[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(cTile2, CAbilityFilter.EFilterTile.EmptyHex) && CObjectProp.FindPropWithPathingBlocker(cTile2.m_ArrayIndex, ref propTile) == null)
						{
							list4.Add(cTile2);
							continue;
						}
						list2.Add(cTile2);
						list3.Add("Invalid parent tile");
					}
					if (list4.Count == 0 && list2.Count == 0)
					{
						list2.Add(ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
						list3.Add("Actor has no target");
					}
					goto IL_0733;
				}
			}
			if (base.OnDeath)
			{
				CTile item2 = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
				list4.Add(item2);
			}
			else
			{
				CTile cTile3 = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
				AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
					if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value && base.ParentAbility.AreaEffect == null)
					{
						CActor cActor = base.ParentAbility.ActorsTargeted[0];
						cTile3 = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
					}
				}
				bool flag = cTile3.FindProp(ScenarioManager.ObjectImportType.Door) != null;
				for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.ELeft; eAdjacentPosition <= ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition++)
				{
					CTile adjacentTile = ScenarioManager.GetAdjacentTile(cTile3.m_ArrayIndex.X, cTile3.m_ArrayIndex.Y, eAdjacentPosition);
					if (adjacentTile != null)
					{
						CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
						if ((adjacentTile.m_Props.Count == 0 || (adjacentTile.m_Props.Count == 1 && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) != null && cNode.IsBridgeOpen)) && adjacentTile.m_HexMap.Revealed && ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && cNode.Walkable && !cNode.Blocked && (flag || adjacentTile.m_HexMap == cTile3.m_HexMap || adjacentTile.m_Hex2Map == cTile3.m_HexMap))
						{
							list4.Add(adjacentTile);
							continue;
						}
						list2.Add(adjacentTile);
						if (adjacentTile.m_Props.Count > 0 && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) != null && !cNode.IsBridgeOpen)
						{
							list3.Add("Closed Door");
						}
						else if (adjacentTile.m_Props.Count > 0 && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) == null)
						{
							list3.Add("Tile contains a Prop");
						}
						else if (!adjacentTile.m_HexMap.Revealed)
						{
							list3.Add("Tile not revealed");
						}
						else if (ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) != null)
						{
							list3.Add("Actor on tile");
						}
						else if (!cNode.Walkable)
						{
							list3.Add("Tile is a wall");
						}
						else if (cNode.Blocked)
						{
							list3.Add("Tile blocked by obstacle");
						}
						else
						{
							list3.Add("Tile seems OK");
						}
					}
					else
					{
						list2.Add(cTile3);
						list3.Add("No adjacent tile");
					}
				}
				if (list4.Count == 0 && list2.Count == 0)
				{
					list2.Add(ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
					list3.Add("Actor target has no adjacent tiles");
				}
			}
			goto IL_0733;
		}
		case SummonState.Summon:
		{
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			CMonsterClass cMonsterClass = null;
			CHeroSummonClass cHeroSummonClass = null;
			if (IsMonsterSummon)
			{
				cMonsterClass = MonsterClassManager.Find(SelectedMonsterYMLData.ID);
			}
			else
			{
				cHeroSummonClass = CharacterClassManager.FindHeroSummonClass(SelectedSummonYMLData.ID);
			}
			if (cMonsterClass == null && (cHeroSummonClass == null || !cHeroSummonClass.IsValidClassName()))
			{
				DLLDebug.LogError("Unable to findpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class for summon " + SelectedSummonYMLData.ID);
				return false;
			}
			foreach (CTile item5 in m_TilesSelected)
			{
				if (cMonsterClass == null && cHeroSummonClass == null)
				{
					continue;
				}
				if (cMonsterClass != null)
				{
					int num5 = cMonsterClass.Health();
					int maxHealth = num5;
					if (base.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == EAbilityStatType.Strength))
					{
						int strength = m_Strength;
						SetStatBasedOnX(base.TargetingActor, base.StatIsBasedOnXEntries, base.AbilityFilter);
						num5 = Math.Min(m_Strength, num5);
						m_Strength = strength;
					}
					CEnemyActor cEnemyActor = null;
					if (cMonsterClass is CObjectClass)
					{
						int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
						ObjectState objectState = new ObjectState(cMonsterClass.ID, chosenModelIndex, null, item5.m_HexMap.MapGuid, new TileIndex(item5.m_ArrayIndex), num5, maxHealth, base.TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, base.TargetingActor.ActorGuid, 1, base.TargetingActor.OriginalType);
						cEnemyActor = ScenarioManager.Scenario.AddObject(objectState, initial: false);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
						}
						else
						{
							m_Summoned = false;
						}
					}
					else
					{
						int chosenModelIndex2 = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
						EnemyState enemyState = new EnemyState(cMonsterClass.ID, chosenModelIndex2, null, item5.m_HexMap.MapGuid, new TileIndex(item5.m_ArrayIndex), num5, maxHealth, base.TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, base.TargetingActor.ActorGuid, 1, base.TargetingActor.OriginalType);
						switch (base.TargetingActor.OriginalType)
						{
						case CActor.EType.Enemy:
							cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
							if (cEnemyActor != null)
							{
								ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
							}
							else
							{
								m_Summoned = false;
							}
							break;
						case CActor.EType.Ally:
							cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
							if (cEnemyActor != null)
							{
								ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
							}
							else
							{
								m_Summoned = false;
							}
							break;
						case CActor.EType.Enemy2:
							cEnemyActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
							if (cEnemyActor != null)
							{
								ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
							}
							else
							{
								m_Summoned = false;
							}
							break;
						case CActor.EType.Neutral:
							cEnemyActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
							if (cEnemyActor != null)
							{
								ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
							}
							else
							{
								m_Summoned = false;
							}
							break;
						default:
							DLLDebug.LogError("Non monster tried to summon a monster actor");
							return false;
						}
					}
					if (cEnemyActor != null)
					{
						SummonedActors.Add(cEnemyActor);
						cEnemyActor.PlayedThisRound = true;
						GameState.SortIntoInitiativeAndIDOrder();
						ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
					}
				}
				else
				{
					CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
					int chosenModelIndex3 = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cHeroSummonClass.Models.Count);
					HeroSummonState heroSummonState = new HeroSummonState(cHeroSummonClass.ID, chosenModelIndex3, null, item5.m_HexMap.MapGuid, new TileIndex(item5.m_ArrayIndex), SelectedSummonYMLData.GetHealth(base.TargetingActor.Level), SelectedSummonYMLData.GetHealth(base.TargetingActor.Level), base.TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, 1, base.TargetingActor.ActorGuid);
					CHeroSummonActor cHeroSummonActor = ScenarioManager.Scenario.AddHeroSummon(heroSummonState, initial: false);
					if (cHeroSummonActor != null)
					{
						cHeroSummonActor.SummonedOrderIndex = ScenarioManager.Scenario.HeroSummons.Count;
						if (GameState.CurrentActionInitiator == GameState.EActionInitiator.CompanionSummon && base.TargetingActor is CPlayerActor cPlayerActor)
						{
							cPlayerActor.SetCompanionGuid(cHeroSummonActor.ActorGuid);
						}
						cBaseCard?.AddActiveBonus(this, cHeroSummonActor, base.TargetingActor);
						cHeroSummonActor.PlayedThisRound = true;
						SummonedActors.Add(cHeroSummonActor);
						GameState.SortIntoInitiativeAndIDOrder();
						cHeroSummonActor.OnSummoned();
						ScenarioManager.CurrentScenarioState.RefreshHeroSummons();
					}
					else
					{
						m_Summoned = false;
					}
				}
				m_State = SummonState.SummonDone;
			}
			if (m_TilesSelected.Count > 0)
			{
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				base.AbilityHasHappened = true;
				CSummon_MessageData message3 = new CSummon_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_ActorSummoning = base.TargetingActor,
					m_SummonAbility = this,
					m_SummonedActors = SummonedActors.ToList(),
					m_SummonTiles = m_TilesSelected
				};
				ScenarioRuleClient.MessageHandler(message3);
				m_Summoned = true;
			}
			else
			{
				m_CancelAbility = true;
				PhaseManager.StepComplete();
			}
			break;
		}
		case SummonState.NoTilesAvailable:
			{
				if (!m_Summoned)
				{
					base.AbilityHasHappened = true;
					CTile item = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
					m_TilesSelected.Add(item);
					CSummon_MessageData message = new CSummon_MessageData(base.AnimOverload, base.TargetingActor)
					{
						m_ActorSummoning = base.TargetingActor,
						m_SummonAbility = this,
						m_SummonTiles = m_TilesSelected
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			}
			IL_0733:
			if (list4.Count > 0)
			{
				if (base.TargetingActor.AIMoveFocusActors.Count == 0)
				{
					base.TargetingActor.Move(1, jump: false, fly: false, m_Range, allowMove: false, ignoreDifficultTerrain: false, null, firstMove: true);
				}
				if (base.TargetingActor.AIMoveFocusActors.Count != 0)
				{
					List<CSummonActorPath> list5 = new List<CSummonActorPath>();
					foreach (CTile item6 in list4)
					{
						CSummonActorPath cSummonActorPath = new CSummonActorPath();
						cSummonActorPath.m_Tile = item6;
						cSummonActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(item6.m_ArrayIndex, ScenarioManager.Tiles[base.TargetingActor.AIMoveFocusActors[0].ArrayIndex.X, base.TargetingActor.AIMoveFocusActors[0].ArrayIndex.Y], out cSummonActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out var foundPath, shouldPathThroughDoors: false);
						if (foundPath)
						{
							list5.Add(cSummonActorPath);
						}
					}
					list5.Sort((CSummonActorPath x, CSummonActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
					if (base.AllTargets)
					{
						m_TilesSelected.AddRange(list5.Select((CSummonActorPath x) => x.m_Tile));
					}
					else
					{
						for (int num2 = 0; num2 < m_NumberTargetsRemaining; num2++)
						{
							if (list5.Count > 0 && list5.Count > num2)
							{
								m_TilesSelected.Add(list5[num2].m_Tile);
							}
							else if (list4.Count > num2)
							{
								m_TilesSelected.Add(list4[num2]);
								list4.RemoveAt(num2);
							}
						}
					}
				}
				else if (base.AllTargets)
				{
					m_TilesSelected.AddRange(list4);
				}
				else
				{
					for (int num3 = 0; num3 < m_NumberTargetsRemaining; num3++)
					{
						if (list4.Count > num3)
						{
							m_TilesSelected.Add(list4[num3]);
						}
					}
				}
				m_State = SummonState.Summon;
				Perform();
				return true;
			}
			SimpleLog.AddToSimpleLog(base.TargetingActor?.Class?.ID + " Failed to summon adjacent to " + base.ParentAbility?.ActorsTargeted[0]?.Class?.ID);
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				SimpleLog.AddToSimpleLog("Tile at x:" + list2[num4].m_ArrayIndex.X + " y:" + list2[num4].m_ArrayIndex.Y + ", fail reason=" + list3[num4]);
			}
			m_State = SummonState.NoTilesAvailable;
			Perform();
			return true;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.TilesSelected.Contains(selectedTile) && m_State == SummonState.SelectSummonPosition)
		{
			if (base.TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < m_NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
					}
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(base.TargetingActor));
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == SummonState.SelectSummonPosition)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == SummonState.SelectSummonPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == SummonState.SelectSummonPosition;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State >= SummonState.SummonDone;
	}

	public override string GetDescription()
	{
		return "Summon";
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool Validate()
	{
		foreach (string summonID in SummonIDs)
		{
			if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonID) == null && ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonID) == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(base.AbilityBaseCard.Name, "Unable to find Summon " + summonID + ".  AbilityBaseCard Name: " + base.AbilityBaseCard.Name);
				return false;
			}
		}
		return true;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		bool isSummon = false;
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SummonState summonState = m_State;
		if (m_State == SummonState.SummonDone && !m_Summoned)
		{
			summonState = SummonState.NoTilesAvailable;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilitySummon(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, summonState, SummonIDs[ScenarioManager.CurrentScenarioState.Players.Count - 1], base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilitySummon()
	{
	}

	public CAbilitySummon(CAbilitySummon state, ReferenceDictionary references)
		: base(state, references)
	{
		SummonedActors = references.Get(state.SummonedActors);
		if (SummonedActors == null && state.SummonedActors != null)
		{
			SummonedActors = new List<CActor>();
			for (int i = 0; i < state.SummonedActors.Count; i++)
			{
				CActor cActor = state.SummonedActors[i];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				SummonedActors.Add(cActor2);
			}
			references.Add(state.SummonedActors, SummonedActors);
		}
		SummonIDs = references.Get(state.SummonIDs);
		if (SummonIDs == null && state.SummonIDs != null)
		{
			SummonIDs = new List<string>();
			for (int j = 0; j < state.SummonIDs.Count; j++)
			{
				string item = state.SummonIDs[j];
				SummonIDs.Add(item);
			}
			references.Add(state.SummonIDs, SummonIDs);
		}
		m_State = state.m_State;
		m_Summoned = state.m_Summoned;
	}
}
