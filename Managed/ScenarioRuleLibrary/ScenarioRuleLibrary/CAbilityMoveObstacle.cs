using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityMoveObstacle : CAbility
{
	public enum MoveObstacleState
	{
		SelectObstacleToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingObstacles,
		RemoveObstacle,
		PlaceObstacle,
		MovedObstacle,
		MoveObstacleDone
	}

	private MoveObstacleState m_State;

	private List<CTile> m_SelectedObstacleTiles;

	private int m_MovingObstacleIndex;

	private string m_MovingPropName;

	private string m_MovingPropGUID;

	private Dictionary<string, int> m_MovedObstaclesDictionary;

	public int MoveObstacleRange { get; set; }

	public string MoveObstacleAnimOverload { get; set; }

	public CAbilityMoveObstacle(int moveObstacleRange, string moveObstacleAnimOverload)
	{
		MoveObstacleRange = moveObstacleRange;
		MoveObstacleAnimOverload = moveObstacleAnimOverload;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MoveObstacleState.SelectObstacleToMove;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		m_SelectedObstacleTiles = new List<CTile>();
		m_MovedObstaclesDictionary = new Dictionary<string, int>();
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
		List<CTile> list;
		AbilityData.MiscAbilityData miscAbilityData2;
		switch (m_State)
		{
		case MoveObstacleState.SelectObstacleToMove:
			if (base.AllTargetsOnMovePath)
			{
				for (int num = 0; num < CAbilityMove.AllArrayIndexOnPathIncludingStart.Count; num++)
				{
					if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(num))
					{
						Point point = CAbilityMove.AllArrayIndexOnPathIncludingStart[num];
						CTile cTile2 = ScenarioManager.Tiles[point.X, point.Y];
						if (cTile2 != null)
						{
							base.TilesInRange.Add(cTile2);
						}
					}
				}
			}
			else if (base.UseSubAbilityTargeting)
			{
				AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
				if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentMovePath.Value && base.ParentAbility is CAbilityMove)
					{
						List<CTile> list3 = new List<CTile>();
						for (int num2 = 0; num2 < CAbilityMove.AllArrayIndexOnPathIncludingStart.Count; num2++)
						{
							if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(num2))
							{
								Point point2 = CAbilityMove.AllArrayIndexOnPathIncludingStart[num2];
								CTile cTile3 = ScenarioManager.Tiles[point2.X, point2.Y];
								list3.Add(cTile3);
								if (cTile3 != null)
								{
									base.TilesInRange.AddRange(GameState.GetTilesInRange(cTile3.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true));
								}
							}
						}
						base.TilesInRange = base.TilesInRange.Distinct().ToList();
						goto IL_04de;
					}
				}
				if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
					if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
						if (miscAbilityData6 != null && miscAbilityData6.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
							{
								base.TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
							}
							base.TilesInRange = base.TilesInRange.Distinct().ToList();
							goto IL_04de;
						}
					}
					base.TilesInRange = base.InlineSubAbilityTiles.ToList();
				}
				else if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
					if (miscAbilityData7 != null && miscAbilityData7.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData8 = base.MiscAbilityData;
						if (miscAbilityData8 != null && miscAbilityData8.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile item in base.ParentAbility.TilesSelected)
							{
								base.TilesInRange.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
							}
							base.TilesInRange = base.TilesInRange.Distinct().ToList();
							goto IL_04de;
						}
					}
					base.TilesInRange = base.ParentAbility.TilesSelected.ToList();
				}
			}
			else if (base.AreaEffect == null)
			{
				base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			goto IL_04de;
		case MoveObstacleState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = MoveObstacleState.SelectMoveToTile;
			Perform();
			break;
		case MoveObstacleState.SelectMoveToTile:
		{
			m_CanUndo = false;
			m_SelectedObstacleTiles.AddRange(m_TilesSelected);
			m_TilesSelected.Clear();
			m_NumberTargets = m_SelectedObstacleTiles.Count;
			m_NumberTargetsRemaining = m_NumberTargets;
			m_Range = MoveObstacleRange;
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: true, ignoreBlocked: true);
			List<CTile> list2 = new List<CTile>();
			foreach (CTile item2 in base.TilesInRange)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles);
				if (CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex) && !MF.IsBlockingSpawnPosition(item2, base.TilesSelected, base.TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles))
				{
					list2.Add(item2);
				}
			}
			base.TilesInRange = list2.ToList();
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.None,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			break;
		}
		case MoveObstacleState.PreMovingObstacles:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			base.AbilityHasHappened = true;
			m_MovingObstacleIndex = 0;
			foreach (CTile selectedObstacleTile in m_SelectedObstacleTiles)
			{
				CTile propTile = selectedObstacleTile;
				CObjectObstacle cObjectObstacle = selectedObstacleTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedObstacleTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(cObjectObstacle);
				}
			}
			if (base.IsMergedAbility)
			{
				PhaseManager.StepComplete();
				break;
			}
			m_State = MoveObstacleState.RemoveObstacle;
			Perform();
			break;
		case MoveObstacleState.RemoveObstacle:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingObstacleIndex < m_SelectedObstacleTiles.Count)
				{
					CTile cTile = m_SelectedObstacleTiles[m_MovingObstacleIndex];
					CObjectObstacle cObjectObstacle3 = cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					m_MovingPropName = cObjectObstacle3.PrefabName;
					m_MovingPropGUID = cObjectObstacle3.PropGuid;
					cObjectObstacle3.DestroyProp(0f, sendMessageToClient: false);
					CDestroyObstacle_MessageData message4 = new CDestroyObstacle_MessageData(base.AnimOverload, base.TargetingActor)
					{
						m_ActorDestroyingObstacle = base.TargetingActor,
						m_Tiles = new List<CTile> { cTile },
						m_DestroyDelay = base.SpawnDelay,
						m_DestroyedProps = new List<CObjectProp> { cObjectObstacle3 },
						m_DestroyObstacleAbility = this,
						m_OverrideSetLastSelectedTile = true
					};
					ScenarioRuleClient.MessageHandler(message4);
					m_MovedObstaclesDictionary.TryGetValue(cObjectObstacle3.PrefabName, out var value);
					m_MovedObstaclesDictionary[cObjectObstacle3.PrefabName] = value + 1;
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case MoveObstacleState.PlaceObstacle:
		{
			CPlacingSpawn_MessageData message2 = new CPlacingSpawn_MessageData(MoveObstacleAnimOverload, base.TargetingActor)
			{
				m_Ability = this,
				m_ActorPlacingSpawn = base.TargetingActor
			};
			ScenarioRuleClient.MessageHandler(message2);
			List<TileIndex> pathingBlockers = new List<TileIndex>
			{
				new TileIndex(base.TilesSelected[m_MovingObstacleIndex].m_ArrayIndex)
			};
			CObjectObstacle cObjectObstacle2 = new CObjectObstacle(m_MovingPropName, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(base.TilesSelected[m_MovingObstacleIndex].m_ArrayIndex), null, null, pathingBlockers, base.TargetingActor, base.TilesSelected[m_MovingObstacleIndex].m_HexMap.MapGuid, ignoresFlyAndJump: false);
			if (!string.IsNullOrEmpty(m_MovingPropGUID))
			{
				cObjectObstacle2.PropGUIDToCopy = m_MovingPropGUID;
			}
			base.TilesSelected[m_MovingObstacleIndex].SpawnProp(cObjectObstacle2, notifyClient: true, base.SpawnDelay);
			break;
		}
		case MoveObstacleState.MovedObstacle:
			{
				m_MovingObstacleIndex++;
				if (m_MovingObstacleIndex <= m_TilesSelected.Count - 1)
				{
					m_State = MoveObstacleState.RemoveObstacle;
					Perform();
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			}
			IL_04de:
			list = new List<CTile>();
			foreach (CTile item3 in base.TilesInRange)
			{
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.Obstacle))
				{
					list.Add(item3);
				}
			}
			base.TilesInRange = list.ToList();
			base.TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
			miscAbilityData2 = base.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.ObstacleTypes?.Count > 0)
			{
				base.TilesInRange.RemoveAll((CTile t) => t != null && !t.m_Props.Any((CObjectProp p) => base.MiscAbilityData.ObstacleTypes.Contains(p.PropType)));
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.None,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.SingleHexObstacle },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message5 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
			else
			{
				CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
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
		if (!base.TilesSelected.Contains(selectedTile))
		{
			if (m_State == MoveObstacleState.SelectObstacleToMove && m_NumberTargetsRemaining > 0)
			{
				if (base.TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle cObjectObstacle && cObjectObstacle.PathingBlockers.Count <= 1)
				{
					CTile cTile = ScenarioManager.Tiles[cObjectObstacle.ArrayIndex.X, cObjectObstacle.ArrayIndex.Y];
					if (cTile != null)
					{
						m_TilesSelected.Add(cTile);
						m_NumberTargetsRemaining--;
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
			else if (m_State == MoveObstacleState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles);
				if (base.TilesInRange.Contains(selectedTile) && !MF.IsBlockingSpawnPosition(selectedTile, m_TilesSelected, base.TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles))
				{
					m_TilesSelected.Add(selectedTile);
					m_NumberTargetsRemaining--;
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message2 = new CPlayerSelectedTile_MessageData(base.TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
				else
				{
					ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(base.TargetingActor));
				}
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
			m_NumberTargetsRemaining++;
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
		if (m_State == MoveObstacleState.SelectObstacleToMove || m_State == MoveObstacleState.SelectMoveToTile)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MoveObstacleState.SelectObstacleToMove)
		{
			return m_State == MoveObstacleState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != MoveObstacleState.SelectObstacleToMove)
			{
				return m_State == MoveObstacleState.SelectMoveToTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == MoveObstacleState.MoveObstacleDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = MoveObstacleState.SelectObstacleToMove;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			base.TilesInRange = m_ValidTilesInAreaEffect;
			base.TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_SelectedObstacleTiles = new List<CTile>();
		m_MovedObstaclesDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityMoveObstacle(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_MovedObstaclesDictionary, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override string GetDescription()
	{
		return "MoveObstacle";
	}

	public bool HasPassedState(MoveObstacleState moveObstacleState)
	{
		return m_State > moveObstacleState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityMoveObstacle()
	{
	}

	public CAbilityMoveObstacle(CAbilityMoveObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		MoveObstacleRange = state.MoveObstacleRange;
		MoveObstacleAnimOverload = state.MoveObstacleAnimOverload;
		m_State = state.m_State;
		m_SelectedObstacleTiles = references.Get(state.m_SelectedObstacleTiles);
		if (m_SelectedObstacleTiles == null && state.m_SelectedObstacleTiles != null)
		{
			m_SelectedObstacleTiles = new List<CTile>();
			for (int i = 0; i < state.m_SelectedObstacleTiles.Count; i++)
			{
				CTile cTile = state.m_SelectedObstacleTiles[i];
				CTile cTile2 = references.Get(cTile);
				if (cTile2 == null && cTile != null)
				{
					cTile2 = new CTile(cTile, references);
					references.Add(cTile, cTile2);
				}
				m_SelectedObstacleTiles.Add(cTile2);
			}
			references.Add(state.m_SelectedObstacleTiles, m_SelectedObstacleTiles);
		}
		m_MovingObstacleIndex = state.m_MovingObstacleIndex;
		m_MovingPropName = state.m_MovingPropName;
		m_MovedObstaclesDictionary = references.Get(state.m_MovedObstaclesDictionary);
		if (m_MovedObstaclesDictionary != null || state.m_MovedObstaclesDictionary == null)
		{
			return;
		}
		m_MovedObstaclesDictionary = new Dictionary<string, int>(state.m_MovedObstaclesDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.m_MovedObstaclesDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			m_MovedObstaclesDictionary.Add(key, value);
		}
		references.Add(state.m_MovedObstaclesDictionary, m_MovedObstaclesDictionary);
	}
}
