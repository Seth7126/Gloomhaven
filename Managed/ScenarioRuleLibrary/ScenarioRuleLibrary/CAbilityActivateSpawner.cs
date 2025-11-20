using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityActivateSpawner : CAbility
{
	public enum EActivateSpawnerState
	{
		SelectSpawnerPosition,
		ActivateSpawner,
		ActivateSpawnerDone
	}

	private EActivateSpawnerState m_State;

	private List<CInteractableSpawner> m_SpawnersToActivate;

	public CAbilityActivateSpawner()
	{
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		m_State = EActivateSpawnerState.SelectSpawnerPosition;
		m_SpawnersToActivate = new List<CInteractableSpawner>();
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
			{
				if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						base.TilesInRange.Add(cTile);
					}
				}
			}
		}
		else if (base.AreaEffect == null)
		{
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
		}
		if (m_NumberTargets == -1 || base.Targeting == EAbilityTargeting.All || base.Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
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
		case EActivateSpawnerState.SelectSpawnerPosition:
			if (base.UseSubAbilityTargeting)
			{
				if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
					{
						TileSelected(inlineSubAbilityTile, null);
					}
					break;
				}
				if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					foreach (CTile item5 in base.ParentAbility.TilesSelected)
					{
						TileSelected(item5, null);
					}
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
					m_TilesSelected.Clear();
					foreach (CTile item6 in base.TilesInRange)
					{
						foreach (CSpawner spawner in item6.m_Spawners)
						{
							if (!(spawner is CInteractableSpawner { IsActive: false } cInteractableSpawner))
							{
								continue;
							}
							if (!m_SpawnersToActivate.Contains(cInteractableSpawner))
							{
								m_SpawnersToActivate.Add(cInteractableSpawner);
							}
							if (!m_TilesSelected.Contains(item6))
							{
								m_TilesSelected.Add(item6);
							}
							if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
							{
								continue;
							}
							foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
							{
								CTile item = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
								if (!m_TilesSelected.Contains(item))
								{
									m_TilesSelected.Add(item);
								}
							}
						}
						CTile spawnerTile = null;
						CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(item6.m_ArrayIndex, ref spawnerTile);
						if (cInteractableSpawner2 == null || cInteractableSpawner2.IsActive)
						{
							continue;
						}
						if (!m_SpawnersToActivate.Contains(cInteractableSpawner2))
						{
							m_SpawnersToActivate.Add(cInteractableSpawner2);
						}
						if (!m_TilesSelected.Contains(item6))
						{
							m_TilesSelected.Add(item6);
						}
						if (cInteractableSpawner2.PathingBlockers == null || cInteractableSpawner2.PathingBlockers.Count <= 0)
						{
							continue;
						}
						foreach (TileIndex pathingBlocker2 in cInteractableSpawner2.PathingBlockers)
						{
							CTile item2 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
							if (!m_TilesSelected.Contains(item2))
							{
								m_TilesSelected.Add(item2);
							}
						}
					}
					if (m_TilesSelected.Count <= 0)
					{
						m_CancelAbility = true;
						if (base.IsMergedAbility)
						{
							PhaseManager.StepComplete();
						}
						else
						{
							PhaseManager.NextStep();
						}
					}
				}
				if (!m_CancelAbility)
				{
					CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
					{
						m_SpawnType = ScenarioManager.ObjectImportType.Spawner,
						m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.DeactiveSpawner },
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				break;
			}
			m_TilesSelected.Clear();
			foreach (CTile item7 in base.TilesInRange)
			{
				foreach (CSpawner spawner2 in item7.m_Spawners)
				{
					if (!(spawner2 is CInteractableSpawner { IsActive: false } cInteractableSpawner3))
					{
						continue;
					}
					if (!m_SpawnersToActivate.Contains(cInteractableSpawner3))
					{
						m_SpawnersToActivate.Add(cInteractableSpawner3);
					}
					if (!m_TilesSelected.Contains(item7))
					{
						m_TilesSelected.Add(item7);
					}
					if (spawner2.PathingBlockers == null || spawner2.PathingBlockers.Count <= 0)
					{
						continue;
					}
					foreach (TileIndex pathingBlocker3 in spawner2.PathingBlockers)
					{
						CTile item3 = ScenarioManager.Tiles[pathingBlocker3.X, pathingBlocker3.Y];
						if (!m_TilesSelected.Contains(item3))
						{
							m_TilesSelected.Add(item3);
						}
					}
				}
				CTile spawnerTile2 = null;
				CInteractableSpawner cInteractableSpawner4 = CObjectProp.FindSpawnerWithPathingBlocker(item7.m_ArrayIndex, ref spawnerTile2);
				if (cInteractableSpawner4 != null && !cInteractableSpawner4.IsActive)
				{
					if (!m_SpawnersToActivate.Contains(cInteractableSpawner4))
					{
						m_SpawnersToActivate.Add(cInteractableSpawner4);
					}
					if (!m_TilesSelected.Contains(item7))
					{
						m_TilesSelected.Add(item7);
					}
					if (cInteractableSpawner4.PathingBlockers != null && cInteractableSpawner4.PathingBlockers.Count > 0)
					{
						foreach (TileIndex pathingBlocker4 in cInteractableSpawner4.PathingBlockers)
						{
							CTile item4 = ScenarioManager.Tiles[pathingBlocker4.X, pathingBlocker4.Y];
							if (!m_TilesSelected.Contains(item4))
							{
								m_TilesSelected.Add(item4);
							}
						}
					}
				}
				if (m_SpawnersToActivate.Count >= m_NumberTargetsRemaining)
				{
					break;
				}
			}
			m_State = EActivateSpawnerState.ActivateSpawner;
			Perform();
			break;
		case EActivateSpawnerState.ActivateSpawner:
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.AbilityHasHappened = true;
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				CActivateOrDeactivateSpawner_MessageData message = new CActivateOrDeactivateSpawner_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_ActorDeactivatingSpawner = base.TargetingActor,
					m_Tiles = m_TilesSelected,
					m_ActivateOrDeactivateSpawnerAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			foreach (CInteractableSpawner item8 in m_SpawnersToActivate)
			{
				item8.SetActive(active: true);
			}
			break;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
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
		if (!base.AllTargets && !base.TilesSelected.Contains(selectedTile) && base.TilesSelected.Count < m_NumberTargets && m_State == EActivateSpawnerState.SelectSpawnerPosition && base.TilesInRange.Contains(selectedTile))
		{
			foreach (CSpawner spawner in selectedTile.m_Spawners)
			{
				if (!(spawner is CInteractableSpawner { IsActive: false } cInteractableSpawner))
				{
					continue;
				}
				if (!m_SpawnersToActivate.Contains(cInteractableSpawner))
				{
					m_SpawnersToActivate.Add(cInteractableSpawner);
				}
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
				{
					CTile item = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (!m_TilesSelected.Contains(item))
					{
						m_TilesSelected.Add(item);
					}
				}
			}
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(selectedTile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner2 != null && !cInteractableSpawner2.IsActive)
			{
				if (!m_SpawnersToActivate.Contains(cInteractableSpawner2))
				{
					m_SpawnersToActivate.Add(cInteractableSpawner2);
				}
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				if (cInteractableSpawner2.PathingBlockers != null && cInteractableSpawner2.PathingBlockers.Count > 0)
				{
					foreach (TileIndex pathingBlocker2 in cInteractableSpawner2.PathingBlockers)
					{
						CTile item2 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
						if (!m_TilesSelected.Contains(item2))
						{
							m_TilesSelected.Add(item2);
						}
					}
				}
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
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
		bool flag = false;
		if (!base.AllTargets && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			foreach (CSpawner spawner in selectedTile.m_Spawners)
			{
				if (!(spawner is CInteractableSpawner item))
				{
					continue;
				}
				m_SpawnersToActivate.Remove(item);
				if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
				{
					CTile item2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					m_TilesSelected.Remove(item2);
				}
			}
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner = CObjectProp.FindSpawnerWithPathingBlocker(selectedTile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner != null)
			{
				m_SpawnersToActivate.Remove(cInteractableSpawner);
				if (cInteractableSpawner.PathingBlockers != null && cInteractableSpawner.PathingBlockers.Count > 0)
				{
					foreach (TileIndex pathingBlocker2 in cInteractableSpawner.PathingBlockers)
					{
						CTile item3 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
						m_TilesSelected.Remove(item3);
					}
				}
			}
			if (m_State == EActivateSpawnerState.SelectSpawnerPosition && base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EActivateSpawnerState.SelectSpawnerPosition)
		{
			Perform();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EActivateSpawnerState.ActivateSpawnerDone;
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
		return "ActivateSpawner";
	}

	public override bool CanClearTargets()
	{
		return m_State == EActivateSpawnerState.SelectSpawnerPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EActivateSpawnerState.SelectSpawnerPosition;
		}
		return false;
	}

	public bool HasPassedState(EActivateSpawnerState deactivateState)
	{
		return m_State > deactivateState;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityActivateSpawner(CAbilityActivateSpawner state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_SpawnersToActivate = references.Get(state.m_SpawnersToActivate);
		if (m_SpawnersToActivate != null || state.m_SpawnersToActivate == null)
		{
			return;
		}
		m_SpawnersToActivate = new List<CInteractableSpawner>();
		for (int i = 0; i < state.m_SpawnersToActivate.Count; i++)
		{
			CInteractableSpawner cInteractableSpawner = state.m_SpawnersToActivate[i];
			CInteractableSpawner cInteractableSpawner2 = references.Get(cInteractableSpawner);
			if (cInteractableSpawner2 == null && cInteractableSpawner != null)
			{
				cInteractableSpawner2 = new CInteractableSpawner(cInteractableSpawner, references);
				references.Add(cInteractableSpawner, cInteractableSpawner2);
			}
			m_SpawnersToActivate.Add(cInteractableSpawner2);
		}
		references.Add(state.m_SpawnersToActivate, m_SpawnersToActivate);
	}
}
