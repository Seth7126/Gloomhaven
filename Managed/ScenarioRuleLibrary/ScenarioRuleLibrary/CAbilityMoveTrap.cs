using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityMoveTrap : CAbility
{
	public enum EMoveTrapState
	{
		SelectTrapToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingTrap,
		RemoveTrap,
		PlaceTrap,
		MovedTrap,
		MoveTrapDone
	}

	private EMoveTrapState m_State;

	private List<CTile> m_SelectedTrapTiles;

	private int m_MovingTrapIndex;

	private string m_MovingPropName;

	private Dictionary<string, int> m_MovedTrapDictionary;

	private AbilityData.TrapData m_MovingTrapData;

	public int MoveTrapRange { get; set; }

	public string MoveTrapAnimOverload { get; set; }

	public CAbilityMoveTrap(int moveTrapRange, string moveTrapAnimOverload)
	{
		MoveTrapRange = moveTrapRange;
		MoveTrapAnimOverload = moveTrapAnimOverload;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EMoveTrapState.SelectTrapToMove;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.AreaEffect == null)
		{
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			List<CTile> list = new List<CTile>();
			foreach (CTile item in base.TilesInRange)
			{
				if (CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.Trap))
				{
					list.Add(item);
				}
			}
			base.TilesInRange = list.ToList();
		}
		m_SelectedTrapTiles = new List<CTile>();
		m_MovedTrapDictionary = new Dictionary<string, int>();
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
		case EMoveTrapState.SelectTrapToMove:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message4 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.None,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Trap },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message4);
			}
			break;
		case EMoveTrapState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = EMoveTrapState.SelectMoveToTile;
			Perform();
			break;
		case EMoveTrapState.SelectMoveToTile:
		{
			m_CanUndo = false;
			m_SelectedTrapTiles.AddRange(m_TilesSelected);
			m_TilesSelected.Clear();
			m_NumberTargets = m_SelectedTrapTiles.Count;
			m_NumberTargetsRemaining = m_NumberTargets;
			m_Range = MoveTrapRange;
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: true, ignoreBlocked: true);
			List<CTile> list = new List<CTile>();
			foreach (CTile item2 in base.TilesInRange)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles, m_SelectedTrapTiles);
				if (CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex) && !MF.IsBlockingSpawnPosition(item2, base.TilesSelected, base.TargetingActor, ref extraUnblockedTiles, m_SelectedTrapTiles))
				{
					list.Add(item2);
				}
			}
			base.TilesInRange = list.ToList();
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
				{
					m_SpawnType = ScenarioManager.ObjectImportType.None,
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		}
		case EMoveTrapState.PreMovingTrap:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			base.AbilityHasHappened = true;
			m_MovingTrapIndex = 0;
			foreach (CTile selectedTrapTile in m_SelectedTrapTiles)
			{
				if (selectedTrapTile.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap item)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(item);
				}
			}
			if (base.IsMergedAbility)
			{
				PhaseManager.StepComplete();
				break;
			}
			m_State = EMoveTrapState.RemoveTrap;
			Perform();
			break;
		case EMoveTrapState.RemoveTrap:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingTrapIndex < m_SelectedTrapTiles.Count)
				{
					CObjectTrap cObjectTrap2 = m_SelectedTrapTiles[m_MovingTrapIndex].FindProp(ScenarioManager.ObjectImportType.Trap) as CObjectTrap;
					m_MovingTrapData = new AbilityData.TrapData
					{
						Damage = cObjectTrap2.DamageValue,
						Conditions = cObjectTrap2.Conditions.ToList(),
						AdjacentRange = cObjectTrap2.AdjacentRange,
						AdjacentDamage = cObjectTrap2.AdjacentDamageValue,
						AdjacentConditions = cObjectTrap2.AdjacentConditions.ToList(),
						AdjacentFilter = cObjectTrap2.AdjacentFilter?.Copy(),
						TriggeredXP = cObjectTrap2.TriggeredXP
					};
					m_MovingPropName = cObjectTrap2.PrefabName;
					cObjectTrap2.DestroyProp(0f, sendMessageToClient: false);
					CDisarmTrap_MessageData message3 = new CDisarmTrap_MessageData(base.AnimOverload, base.TargetingActor)
					{
						m_ActorDisarmingTrap = base.TargetingActor,
						m_Tiles = m_SelectedTrapTiles,
						m_DisarmedTraps = new List<CObjectTrap> { cObjectTrap2 },
						m_DisarmTrapAbility = this
					};
					ScenarioRuleClient.MessageHandler(message3);
					m_MovedTrapDictionary.TryGetValue(cObjectTrap2.PrefabName, out var value);
					m_MovedTrapDictionary[cObjectTrap2.PrefabName] = value + 1;
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EMoveTrapState.PlaceTrap:
		{
			CObjectTrap cObjectTrap = new CObjectTrap(m_MovingPropName, ScenarioManager.ObjectImportType.Trap, new TileIndex(m_TilesSelected[0].m_ArrayIndex), null, null, base.TargetingActor, m_TilesSelected[0].m_HexMap.MapGuid, m_MovingTrapData);
			m_TilesSelected[0].SpawnProp(cObjectTrap, notifyClient: true, base.SpawnDelay);
			CPlacingTrap_MessageData message2 = new CPlacingTrap_MessageData(MoveTrapAnimOverload, base.TargetingActor)
			{
				m_ActorPlacingTrap = base.TargetingActor,
				m_Tile = m_TilesSelected[0],
				m_TrapAbility = this,
				m_TrapObject = cObjectTrap
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case EMoveTrapState.MovedTrap:
			m_MovingTrapIndex++;
			if (m_MovingTrapIndex <= m_TilesSelected.Count - 1)
			{
				m_State = EMoveTrapState.RemoveTrap;
				Perform();
			}
			else
			{
				PhaseManager.StepComplete();
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
			if (m_State == EMoveTrapState.SelectTrapToMove && m_NumberTargetsRemaining > 0)
			{
				if (base.TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap cObjectTrap)
				{
					CTile cTile = ScenarioManager.Tiles[cObjectTrap.ArrayIndex.X, cObjectTrap.ArrayIndex.Y];
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
			else if (m_State == EMoveTrapState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles, m_SelectedTrapTiles);
				if (base.TilesInRange.Contains(selectedTile) && !MF.IsBlockingSpawnPosition(selectedTile, m_TilesSelected, base.TargetingActor, ref extraUnblockedTiles, m_SelectedTrapTiles))
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
		if (m_State == EMoveTrapState.SelectMoveToTile)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != EMoveTrapState.SelectTrapToMove)
		{
			return m_State == EMoveTrapState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveTrapState.SelectTrapToMove)
			{
				return m_State == EMoveTrapState.SelectMoveToTile;
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
		return m_State == EMoveTrapState.MoveTrapDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = EMoveTrapState.SelectTrapToMove;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TilesSelected.Clear();
		if (base.AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			base.TilesInRange = m_ValidTilesInAreaEffect;
			base.TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_SelectedTrapTiles = new List<CTile>();
		m_MovedTrapDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid)?.IsSummon;
		}
	}

	public override string GetDescription()
	{
		return "MoveTrap";
	}

	public bool HasPassedState(EMoveTrapState moveTrapState)
	{
		return m_State > moveTrapState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityMoveTrap()
	{
	}

	public CAbilityMoveTrap(CAbilityMoveTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		MoveTrapRange = state.MoveTrapRange;
		MoveTrapAnimOverload = state.MoveTrapAnimOverload;
		m_State = state.m_State;
		m_SelectedTrapTiles = references.Get(state.m_SelectedTrapTiles);
		if (m_SelectedTrapTiles == null && state.m_SelectedTrapTiles != null)
		{
			m_SelectedTrapTiles = new List<CTile>();
			for (int i = 0; i < state.m_SelectedTrapTiles.Count; i++)
			{
				CTile cTile = state.m_SelectedTrapTiles[i];
				CTile cTile2 = references.Get(cTile);
				if (cTile2 == null && cTile != null)
				{
					cTile2 = new CTile(cTile, references);
					references.Add(cTile, cTile2);
				}
				m_SelectedTrapTiles.Add(cTile2);
			}
			references.Add(state.m_SelectedTrapTiles, m_SelectedTrapTiles);
		}
		m_MovingTrapIndex = state.m_MovingTrapIndex;
		m_MovingPropName = state.m_MovingPropName;
		m_MovedTrapDictionary = references.Get(state.m_MovedTrapDictionary);
		if (m_MovedTrapDictionary != null || state.m_MovedTrapDictionary == null)
		{
			return;
		}
		m_MovedTrapDictionary = new Dictionary<string, int>(state.m_MovedTrapDictionary.Comparer);
		foreach (KeyValuePair<string, int> item in state.m_MovedTrapDictionary)
		{
			string key = item.Key;
			int value = item.Value;
			m_MovedTrapDictionary.Add(key, value);
		}
		references.Add(state.m_MovedTrapDictionary, m_MovedTrapDictionary);
	}
}
