using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRevive : CAbility
{
	public enum EReviveState
	{
		SelectGraves,
		Revive,
		ReviveDone
	}

	private EReviveState m_State;

	private bool m_Summoned;

	public CActor SummonedActor { get; set; }

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

	public MonsterYMLData SelectedMonsterYMLData => ScenarioRuleClient.SRLYML.GetMonsterData(SelectedSummonID);

	public string SelectedLocKey => SelectedMonsterYMLData.LocKey;

	public CAbilityRevive(List<string> summons)
	{
		if (summons.Count != 4)
		{
			throw new Exception("Invalid size for summons list.  Must be exactly 4.  Current size: " + summons.Count);
		}
		SummonIDs = summons;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EReviveState.SelectGraves;
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_Summoned = false;
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
		switch (m_State)
		{
		case EReviveState.SelectGraves:
			if (base.AreaEffect == null)
			{
				base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true);
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
				if (!base.TilesInRange.Contains(adjacentTile))
				{
					base.TilesInRange.Add(adjacentTile);
				}
				List<CTile> list3 = new List<CTile>();
				foreach (CTile item in base.TilesInRange)
				{
					using List<CObjectProp>.Enumerator enumerator3 = item.FindProps(ScenarioManager.ObjectImportType.MonsterGrave).GetEnumerator();
					if (enumerator3.MoveNext())
					{
						_ = enumerator3.Current;
						list3.Add(item);
					}
				}
				base.TilesInRange = list3.ToList();
			}
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				if (base.TilesInRange.Count > 0)
				{
					if (base.TargetingActor.AIMoveFocusTiles.Count != 0)
					{
						List<CAbilitySummon.CSummonActorPath> list4 = new List<CAbilitySummon.CSummonActorPath>();
						foreach (Point aIMoveFocusTile in base.TargetingActor.AIMoveFocusTiles)
						{
							CAbilitySummon.CSummonActorPath cSummonActorPath2 = new CAbilitySummon.CSummonActorPath();
							CTile cTile2 = ScenarioManager.Tiles[aIMoveFocusTile.X, aIMoveFocusTile.Y];
							if ((CObjectMonsterGrave)cTile2.FindProp(ScenarioManager.ObjectImportType.MonsterGrave) != null)
							{
								cSummonActorPath2.m_Tile = cTile2;
								cSummonActorPath2.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(base.TargetingActor.ArrayIndex, cTile2, out cSummonActorPath2.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out var foundPath2, shouldPathThroughDoors: false);
								if (foundPath2)
								{
									list4.Add(cSummonActorPath2);
								}
							}
						}
						if (list4.Count == 0)
						{
							foreach (CTile item2 in base.TilesInRange)
							{
								CAbilitySummon.CSummonActorPath cSummonActorPath3 = new CAbilitySummon.CSummonActorPath();
								cSummonActorPath3.m_Tile = item2;
								CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
								bool foundPath3;
								if (adjacentTile2 == item2)
								{
									foundPath3 = true;
									cSummonActorPath3.m_ArrayIndices = new List<Point> { adjacentTile2.m_ArrayIndex };
								}
								else
								{
									cSummonActorPath3.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(item2.m_ArrayIndex, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y], out cSummonActorPath3.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out foundPath3, shouldPathThroughDoors: false);
								}
								if (foundPath3)
								{
									list4.Add(cSummonActorPath3);
								}
							}
						}
						list4.Sort((CAbilitySummon.CSummonActorPath x, CAbilitySummon.CSummonActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
						if (m_NumberTargetsRemaining == -1)
						{
							m_NumberTargetsRemaining = base.TilesInRange.Count;
						}
						if (base.AllTargets)
						{
							m_TilesSelected.AddRange(list4.Select((CAbilitySummon.CSummonActorPath x) => x.m_Tile));
						}
						else
						{
							for (int num2 = 0; num2 < m_NumberTargetsRemaining; num2++)
							{
								if (list4.Count > 0 && list4.Count > num2)
								{
									m_TilesSelected.Add(list4[num2].m_Tile);
								}
								else if (base.TilesInRange.Count > num2)
								{
									m_TilesSelected.Add(base.TilesInRange[num2]);
									base.TilesInRange.RemoveAt(num2);
								}
							}
						}
					}
					else if (base.AllTargets)
					{
						m_TilesSelected.AddRange(base.TilesInRange);
					}
					else
					{
						for (int num3 = 0; num3 < m_NumberTargetsRemaining; num3++)
						{
							if (base.TilesInRange.Count > num3)
							{
								m_TilesSelected.Add(base.TilesInRange[num3]);
							}
						}
					}
					PhaseManager.StepComplete();
				}
				else
				{
					PhaseManager.NextStep();
				}
			}
			else
			{
				DLLDebug.LogError("Revive ability not supported for players");
			}
			break;
		case EReviveState.Revive:
		{
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				base.AbilityHasHappened = true;
				CRevive_MessageData message = new CRevive_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_ActorReviving = base.TargetingActor,
					m_ReviveAbility = this,
					m_ReviveTiles = m_TilesSelected.ToList()
				};
				ScenarioRuleClient.MessageHandler(message);
				m_Summoned = true;
			}
			else
			{
				m_CancelAbility = true;
				PhaseManager.StepComplete();
			}
			CMonsterClass cMonsterClass = null;
			foreach (CTile item3 in m_TilesSelected)
			{
				CObjectMonsterGrave obj = (CObjectMonsterGrave)item3.FindProp(ScenarioManager.ObjectImportType.MonsterGrave);
				cMonsterClass = MonsterClassManager.Find(SelectedMonsterYMLData.ID);
				if (!obj.IsEliteGrave && cMonsterClass != null)
				{
					cMonsterClass = cMonsterClass.NonEliteVariant;
				}
				if (cMonsterClass == null)
				{
					continue;
				}
				CTile cTile = null;
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.EmptyHex))
				{
					cTile = item3;
				}
				else
				{
					for (int i = 0; i < 5; i++)
					{
						List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(item3, i + 1);
						allUnblockedTilesFromOrigin.Remove(item3);
						List<CTile> list = new List<CTile>();
						foreach (CTile item4 in allUnblockedTilesFromOrigin)
						{
							if (CAbilityFilter.IsValidTile(item4, CAbilityFilter.EFilterTile.EmptyHex))
							{
								list.Add(item4);
							}
						}
						if (list.Count <= 0)
						{
							continue;
						}
						if (base.TargetingActor.AIMoveFocusActors.Count > 0)
						{
							List<CAbilitySummon.CSummonActorPath> list2 = new List<CAbilitySummon.CSummonActorPath>();
							foreach (CTile item5 in list)
							{
								CAbilitySummon.CSummonActorPath cSummonActorPath = new CAbilitySummon.CSummonActorPath();
								cSummonActorPath.m_Tile = item5;
								bool foundPath;
								if (base.TargetingActor.AIMoveFocusActors[0] != null)
								{
									cSummonActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(item5.m_ArrayIndex, ScenarioManager.Tiles[base.TargetingActor.AIMoveFocusActors[0].ArrayIndex.X, base.TargetingActor.AIMoveFocusActors[0].ArrayIndex.Y], out cSummonActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out foundPath, shouldPathThroughDoors: false);
								}
								else
								{
									cSummonActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(item5.m_ArrayIndex, ScenarioManager.Tiles[base.TargetingActor.AIMoveFocusTiles[0].X, base.TargetingActor.AIMoveFocusTiles[0].Y], out cSummonActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out foundPath, shouldPathThroughDoors: false);
								}
								if (foundPath)
								{
									list2.Add(cSummonActorPath);
								}
							}
							list2.Sort((CAbilitySummon.CSummonActorPath x, CAbilitySummon.CSummonActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
							cTile = list2[0].m_Tile;
						}
						else
						{
							cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
						}
						break;
					}
				}
				if (cTile == null)
				{
					continue;
				}
				int num = cMonsterClass.Health();
				int maxHealth = num;
				if (base.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == EAbilityStatType.Strength))
				{
					int strength = m_Strength;
					SetStatBasedOnX(base.TargetingActor, base.StatIsBasedOnXEntries, base.AbilityFilter);
					num = Math.Min(m_Strength, num);
					m_Strength = strength;
				}
				CEnemyActor cEnemyActor = null;
				if (cMonsterClass is CObjectClass)
				{
					int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
					ObjectState objectState = new ObjectState(cMonsterClass.ID, chosenModelIndex, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), num, maxHealth, base.TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, base.TargetingActor.ActorGuid, 1, base.TargetingActor.OriginalType);
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
					EnemyState enemyState = new EnemyState(cMonsterClass.ID, chosenModelIndex2, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), num, maxHealth, base.TargetingActor.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: true, base.TargetingActor.ActorGuid, 1, base.TargetingActor.OriginalType);
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
					SummonedActor = cEnemyActor;
					cEnemyActor.PlayedThisRound = true;
					GameState.SortIntoInitiativeAndIDOrder();
					ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				}
				using List<CObjectProp>.Enumerator enumerator3 = item3.FindProps(ScenarioManager.ObjectImportType.MonsterGrave).GetEnumerator();
				if (enumerator3.MoveNext())
				{
					CObjectProp current4 = enumerator3.Current;
					current4.Activate(base.TargetingActor, base.TargetingActor);
					item3.m_Props.Remove(current4);
				}
			}
			break;
		}
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool CanClearTargets()
	{
		return m_State == EReviveState.SelectGraves;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EReviveState.SelectGraves;
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
		return m_State >= EReviveState.ReviveDone;
	}

	public override string GetDescription()
	{
		return "Revive";
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
			if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == summonID) == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(base.AbilityBaseCard.Name, "Unable to find Revive " + summonID + ".  AbilityBaseCard Name: " + base.AbilityBaseCard.Name);
				return false;
			}
		}
		return true;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid)?.IsSummon;
		}
		_ = m_State;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityRevive()
	{
	}

	public CAbilityRevive(CAbilityRevive state, ReferenceDictionary references)
		: base(state, references)
	{
		SummonIDs = references.Get(state.SummonIDs);
		if (SummonIDs == null && state.SummonIDs != null)
		{
			SummonIDs = new List<string>();
			for (int i = 0; i < state.SummonIDs.Count; i++)
			{
				string item = state.SummonIDs[i];
				SummonIDs.Add(item);
			}
			references.Add(state.SummonIDs, SummonIDs);
		}
		m_State = state.m_State;
		m_Summoned = state.m_Summoned;
	}
}
