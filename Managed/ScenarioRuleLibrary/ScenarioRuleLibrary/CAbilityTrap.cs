using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityTrap : CAbility
{
	public enum TrapState
	{
		CheckForDelayedDrops,
		SelectTrapPosition,
		SpawnTrap,
		TrapDone,
		NoTilesSelected
	}

	private class CActorPath
	{
		public CActor m_Actor;

		public List<Point> m_ArrayIndices;

		public List<CTile> m_Waypoints;
	}

	private TrapState m_State;

	private bool m_TrapPlaced;

	public AbilityData.TrapData TrapData { get; set; }

	public CAbilityTrap(AbilityData.TrapData trapData)
	{
		TrapData = trapData;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = TrapState.CheckForDelayedDrops;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TrapPlaced = false;
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
		case TrapState.CheckForDelayedDrops:
		{
			CWaitForDelayedDrops_MessageData message2 = new CWaitForDelayedDrops_MessageData();
			ScenarioRuleClient.MessageHandler(message2);
			return true;
		}
		case TrapState.SelectTrapPosition:
		{
			if (base.UseSubAbilityTargeting)
			{
				if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					m_TilesSelected.AddRange(base.InlineSubAbilityTiles);
				}
				else
				{
					if (base.ParentAbility == null || base.ParentAbility.TilesSelected == null || base.ParentAbility.TilesSelected.Count <= 0)
					{
						PhaseManager.NextStep();
						return true;
					}
					foreach (CTile item in base.ParentAbility.TilesSelected)
					{
						if (!m_TilesSelected.Contains(item))
						{
							m_TilesSelected.Add(item);
						}
					}
				}
				m_State = TrapState.SpawnTrap;
				Perform();
				break;
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (base.AreaEffect == null)
				{
					base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
					List<CTile> list = new List<CTile>();
					foreach (CTile item2 in base.TilesInRange)
					{
						if (CAbilityFilter.IsValidTile(item2, TrapData.PlacementTileFilter) && item2.FindProp(ScenarioManager.ObjectImportType.Trap) == null)
						{
							list.Add(item2);
						}
					}
					base.TilesInRange = list.ToList();
				}
				CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.Trap,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { TrapData.PlacementTileFilter },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message3);
				break;
			}
			CTile cTile = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
			List<CTile> list2 = new List<CTile>();
			for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.ELeft; eAdjacentPosition <= ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition++)
			{
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, eAdjacentPosition);
				if (adjacentTile != null)
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
					if ((adjacentTile.m_Props.Count == 0 || (adjacentTile.m_Props.Count == 1 && adjacentTile.FindProp(ScenarioManager.ObjectImportType.TerrainVisualEffect) != null) || (adjacentTile.FindProp(ScenarioManager.ObjectImportType.Trap) == null && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) != null && cNode.IsBridgeOpen)) && adjacentTile.m_HexMap.Revealed && ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && cNode.Walkable && !cNode.Blocked && (cTile.FindProp(ScenarioManager.ObjectImportType.Door) != null || (cTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && (adjacentTile.m_HexMap == cTile.m_HexMap || adjacentTile.m_Hex2Map == cTile.m_HexMap))))
					{
						list2.Add(adjacentTile);
					}
				}
			}
			List<CActor> list3 = new List<CActor>();
			if (base.TargetingActor.Type == CActor.EType.Player || base.TargetingActor.Type == CActor.EType.Ally || base.TargetingActor.Type == CActor.EType.HeroSummon)
			{
				foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
				{
					list3.Add(enemy);
				}
				foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
				{
					list3.Add(enemy2Monster);
				}
			}
			else if (base.TargetingActor.Type == CActor.EType.Enemy2)
			{
				foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
				{
					list3.Add(allyMonster);
				}
				foreach (CEnemyActor enemy2 in ScenarioManager.Scenario.Enemies)
				{
					list3.Add(enemy2);
				}
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					list3.Add(neutralMonster);
				}
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					list3.Add(playerActor);
				}
				foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
				{
					list3.Add(heroSummon);
				}
			}
			else if (base.TargetingActor.Type == CActor.EType.Neutral)
			{
				foreach (CEnemyActor enemy3 in ScenarioManager.Scenario.Enemies)
				{
					list3.Add(enemy3);
				}
				foreach (CEnemyActor enemy2Monster2 in ScenarioManager.Scenario.Enemy2Monsters)
				{
					list3.Add(enemy2Monster2);
				}
			}
			else
			{
				foreach (CEnemyActor allyMonster2 in ScenarioManager.Scenario.AllyMonsters)
				{
					list3.Add(allyMonster2);
				}
				foreach (CEnemyActor enemy2Monster3 in ScenarioManager.Scenario.Enemy2Monsters)
				{
					list3.Add(enemy2Monster3);
				}
				foreach (CEnemyActor neutralMonster2 in ScenarioManager.Scenario.NeutralMonsters)
				{
					list3.Add(neutralMonster2);
				}
				foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
				{
					list3.Add(playerActor2);
				}
				foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
				{
					list3.Add(heroSummon2);
				}
			}
			List<CActorPath> list4 = new List<CActorPath>();
			foreach (CActor item3 in list3)
			{
				foreach (CTile item4 in list2)
				{
					CActorPath cActorPath = new CActorPath();
					cActorPath.m_Actor = item3;
					cActorPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(item4.m_ArrayIndex, ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y], out cActorPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, excludeDestination: true, ignoreMoveCost: true, out var foundPath, shouldPathThroughDoors: false);
					if (foundPath)
					{
						cActorPath.m_ArrayIndices.Insert(0, item4.m_ArrayIndex);
					}
					list4.Add(cActorPath);
				}
			}
			list4.Sort((CActorPath x, CActorPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
			foreach (CActorPath item5 in list4)
			{
				if (item5.m_ArrayIndices.Count <= 0)
				{
					continue;
				}
				CTile cTile2 = ScenarioManager.Tiles[item5.m_ArrayIndices[0].X, item5.m_ArrayIndices[0].Y];
				if (cTile2.m_Props.Count == 0 && ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndices[0]) == null && cTile2.m_HexMap.Revealed)
				{
					if (!m_TilesSelected.Contains(cTile2))
					{
						m_TilesSelected.Add(cTile2);
					}
					break;
				}
			}
			if (m_TilesSelected.Count == 0 && list4.Count > 0)
			{
				int num = int.MaxValue;
				CTile cTile3 = null;
				foreach (CTile item6 in list2)
				{
					foreach (CActorPath item7 in list4)
					{
						bool foundPath2 = false;
						List<Point> list5 = ScenarioManager.PathFinder.FindPath(item6.m_ArrayIndex, item7.m_Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2);
						if (foundPath2 && list5.Count < num)
						{
							num = list5.Count;
							cTile3 = item6;
						}
					}
				}
				if (cTile3 != null && !m_TilesSelected.Contains(cTile3))
				{
					m_TilesSelected.Add(cTile3);
				}
			}
			if (m_TilesSelected.Count > 0)
			{
				m_State = TrapState.SpawnTrap;
				Perform();
				break;
			}
			PhaseManager.NextStep();
			return true;
		}
		case TrapState.SpawnTrap:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				base.AbilityHasHappened = true;
				m_TrapPlaced = true;
				CObjectTrap cObjectTrap = new CObjectTrap(TrapData.TrapName, ScenarioManager.ObjectImportType.Trap, new TileIndex(m_TilesSelected[0].m_ArrayIndex), null, null, base.TargetingActor, m_TilesSelected[0].m_HexMap.MapGuid, TrapData);
				m_TilesSelected[0].SpawnProp(cObjectTrap, notifyClient: true, base.SpawnDelay);
				CPlacingTrap_MessageData message = new CPlacingTrap_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_ActorPlacingTrap = base.TargetingActor,
					m_Tile = m_TilesSelected[0],
					m_TrapAbility = this,
					m_TrapObject = cObjectTrap
				};
				ScenarioRuleClient.MessageHandler(message);
				m_TilesSelected.RemoveAt(0);
				if (m_TilesSelected.Count > 0)
				{
					Perform();
				}
			}
			break;
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
		bool flag = false;
		if (!base.TilesSelected.Contains(selectedTile) && m_State == TrapState.SelectTrapPosition)
		{
			if (base.TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < m_NumberTargets)
				{
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					}
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(base.TargetingActor));
				m_State = TrapState.SelectTrapPosition;
				flag = true;
			}
		}
		if (flag)
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

	public override bool CanClearTargets()
	{
		return m_State == TrapState.SelectTrapPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == TrapState.SelectTrapPosition;
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
		return m_State == TrapState.TrapDone;
	}

	public override string GetDescription()
	{
		return "Trap";
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
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
		TrapState trapState = m_State;
		if (trapState == TrapState.TrapDone && !m_TrapPlaced)
		{
			trapState = TrapState.NoTilesSelected;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTrap(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, trapState, TrapData.TrapName, base.Strength, TrapData.TriggeredXP, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityTrap()
	{
	}

	public CAbilityTrap(CAbilityTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_TrapPlaced = state.m_TrapPlaced;
	}
}
