using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityDestroyObstacle : CAbility
{
	public enum DestroyObstacleState
	{
		SelectObstaclesPosition,
		PreDestroyObstacleProcessing,
		DestroyObstacles,
		DestroyObstaclesDone
	}

	public class DestroyObstacleData
	{
		public bool CanTargetEmptyHexes;

		public bool CanDestroyObstaclesWithHP;

		public DestroyObstacleData Copy()
		{
			return new DestroyObstacleData
			{
				CanTargetEmptyHexes = CanTargetEmptyHexes,
				CanDestroyObstaclesWithHP = CanDestroyObstaclesWithHP
			};
		}

		public static DestroyObstacleData DefaultDestroyObstacleData()
		{
			return new DestroyObstacleData
			{
				CanTargetEmptyHexes = false,
				CanDestroyObstaclesWithHP = false
			};
		}
	}

	private DestroyObstacleState m_State;

	private Dictionary<string, int> m_destroyedObstaclesDictionary;

	private int m_NumberOfTilesBlockedDestroyed;

	public DestroyObstacleState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public DestroyObstacleData AbilityDestroyObstacleData { get; set; }

	public CAbilityDestroyObstacle(DestroyObstacleData destroyObstacleData)
	{
		AbilityDestroyObstacleData = destroyObstacleData;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = DestroyObstacleState.SelectObstaclesPosition;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.CanSkip == false)
		{
			m_CanSkip = false;
		}
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
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (base.UseSubAbilityTargeting)
		{
			if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
			{
				m_ValidTilesInAreaEffect.AddRange(base.ParentAbility.InlineSubAbilityTiles);
			}
			else if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
			{
				if (base.ParentAbility.AreaEffect != null)
				{
					m_ValidTilesInAreaEffect.AddRange(base.ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
				}
				else
				{
					m_ValidTilesInAreaEffect.AddRange(base.ParentAbility.TilesSelected);
				}
			}
		}
		else if (base.AreaEffect == null)
		{
			if (m_Range == 0)
			{
				m_ValidTilesInAreaEffect = new List<CTile>();
				m_ValidTilesInAreaEffect.Add(ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y]);
			}
			else
			{
				m_ValidTilesInAreaEffect = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			base.TilesInRange = m_ValidTilesInAreaEffect.ToList();
		}
		else
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(targetActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		}
		m_ValidTilesInAreaEffect.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (!AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				CTile propTile = null;
				CObjectObstacle cObjectObstacle = item.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null && !cObjectObstacle.Activated)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						CTile cTile2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
						if (cTile2 != null && !m_TilesSelected.Contains(cTile2))
						{
							m_TilesSelected.Add(cTile2);
						}
					}
				}
				if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && GameState.GetActorsOnTile(item, base.FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), base.ActorsToIgnore, isTargetedAbility: false, false).Count > 0 && !m_TilesSelected.Contains(item))
				{
					m_TilesSelected.Add(item);
				}
				if (AbilityDestroyObstacleData.CanTargetEmptyHexes && CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.EmptyHex) && !m_TilesSelected.Contains(item))
				{
					m_TilesSelected.Add(item);
				}
			}
		}
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberOfTilesBlockedDestroyed = 0;
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
		case DestroyObstacleState.SelectObstaclesPosition:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
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
					else if (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value)
					{
						PhaseManager.StepComplete();
						break;
					}
				}
				if (!m_CancelAbility)
				{
					List<CAbilityFilter.EFilterTile> list2 = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Obstacle };
					if (AbilityDestroyObstacleData.CanTargetEmptyHexes)
					{
						list2.Add(CAbilityFilter.EFilterTile.EmptyHex);
					}
					if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
					{
						list2.Add(CAbilityFilter.EFilterTile.ObjectActor);
					}
					CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
					{
						m_SpawnType = ScenarioManager.ObjectImportType.None,
						m_TileFilter = list2,
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				break;
			}
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle)
				{
					if (!m_TilesSelected.Contains(item))
					{
						m_TilesSelected.Add(item);
					}
					m_State = DestroyObstacleState.DestroyObstacles;
					Perform();
					break;
				}
			}
			m_State = DestroyObstacleState.DestroyObstaclesDone;
			Perform();
			break;
		case DestroyObstacleState.PreDestroyObstacleProcessing:
			foreach (CTile item2 in m_TilesSelected)
			{
				CTile propTile2 = item2;
				CObjectObstacle cObjectObstacle2 = item2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle2 == null)
				{
					cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile2);
				}
				if (cObjectObstacle2 != null)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(cObjectObstacle2);
				}
			}
			PhaseManager.StepComplete();
			break;
		case DestroyObstacleState.DestroyObstacles:
		{
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			bool faceObstacle = !base.IsMergedAbility;
			List<CObjectProp> list = new List<CObjectProp>();
			foreach (CTile item3 in m_TilesSelected)
			{
				base.AbilityHasHappened = true;
				CTile propTile = item3;
				bool flag = false;
				CObjectObstacle cObjectObstacle = item3.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item3.m_ArrayIndex, ref propTile);
					flag = true;
				}
				if (cObjectObstacle != null)
				{
					if (cObjectObstacle.PropHealthDetails != null && cObjectObstacle.PropHealthDetails.HasHealth && AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
					{
						GameState.KillActor(base.TargetingActor, cObjectObstacle.RuntimeAttachedActor, CActor.ECauseOfDeath.KillAbility, out var _, this);
					}
					if (base.ParentAbility is CAbilityMerged cAbilityMerged && cAbilityMerged.GetMergedWithAbility(this).AbilityHasBeenCancelled)
					{
						faceObstacle = true;
					}
					if (!flag)
					{
						m_destroyedObstaclesDictionary.TryGetValue(cObjectObstacle.PrefabName, out var value);
						m_destroyedObstaclesDictionary[cObjectObstacle.PrefabName] = value + 1;
						m_NumberOfTilesBlockedDestroyed += cObjectObstacle.PathingBlockers.Count;
					}
					cObjectObstacle.DestroyProp(0f, sendMessageToClient: false);
					list.Add(cObjectObstacle);
				}
				else
				{
					if (!AbilityDestroyObstacleData.CanDestroyObstaclesWithHP)
					{
						continue;
					}
					foreach (CActor item4 in GameState.GetActorsOnTile(item3, base.FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), base.ActorsToIgnore, isTargetedAbility: false, false))
					{
						GameState.KillActor(base.TargetingActor, item4, CActor.ECauseOfDeath.KillAbility, out var _, this);
					}
				}
			}
			CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(base.AnimOverload, base.TargetingActor, faceObstacle)
			{
				m_ActorDestroyingObstacle = base.TargetingActor,
				m_Tiles = m_TilesSelected.ToList(),
				m_DestroyedProps = list.ToList(),
				m_DestroyDelay = base.SpawnDelay,
				m_DestroyObstacleAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			return true;
		}
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
		if (!base.AllTargets && !base.TilesSelected.Contains(selectedTile) && m_State == DestroyObstacleState.SelectObstaclesPosition && m_NumberTargetsRemaining > 0 && m_ValidTilesInAreaEffect.Contains(selectedTile))
		{
			CObjectObstacle cObjectObstacle = selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				CTile propTile = null;
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedTile.m_ArrayIndex, ref propTile);
			}
			bool flag2 = false;
			if (cObjectObstacle != null && !cObjectObstacle.Activated)
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (cTile != null && !m_TilesSelected.Contains(cTile))
					{
						m_TilesSelected.Add(cTile);
						flag2 = true;
					}
				}
			}
			if (AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && GameState.GetActorsOnTile(selectedTile, base.FilterActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.Object), base.ActorsToIgnore, isTargetedAbility: false, false).Count > 0 && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag2 = true;
			}
			if (AbilityDestroyObstacleData.CanTargetEmptyHexes && CAbilityFilter.IsValidTile(selectedTile, CAbilityFilter.EFilterTile.EmptyHex) && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag2 = true;
			}
			if (flag2)
			{
				m_NumberTargetsRemaining--;
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
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
		if (!base.AllTargets)
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				CObjectObstacle cObjectObstacle = selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					CTile propTile = null;
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null)
				{
					foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
					{
						CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
						if (cTile != null)
						{
							m_TilesSelected.Remove(cTile);
						}
					}
					m_NumberTargetsRemaining++;
				}
				else
				{
					m_TilesSelected.Remove(selectedTile);
					m_NumberTargetsRemaining++;
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
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == DestroyObstacleState.SelectObstaclesPosition)
		{
			Restart();
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == DestroyObstacleState.SelectObstaclesPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == DestroyObstacleState.SelectObstaclesPosition;
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
		return m_State == DestroyObstacleState.DestroyObstaclesDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = DestroyObstacleState.SelectObstaclesPosition;
		m_TilesSelected.Clear();
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
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (base.AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			base.TilesInRange = m_ValidTilesInAreaEffect;
		}
		m_ValidTilesInAreaEffect.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (!AbilityDestroyObstacleData.CanDestroyObstaclesWithHP && p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		if (m_NumberTargets == base.OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				CTile propTile = null;
				CObjectObstacle cObjectObstacle = item.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle == null || cObjectObstacle.Activated)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					CTile cTile2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (cTile2 != null && !m_TilesSelected.Contains(cTile2))
					{
						m_TilesSelected.Add(cTile2);
					}
				}
			}
		}
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_NumberOfTilesBlockedDestroyed = 0;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		GameState.UpdateObstaclesDestroyedThisTurn(m_NumberOfTilesBlockedDestroyed);
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyObstacle(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_destroyedObstaclesDictionary, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override string GetDescription()
	{
		return "DestroyObstacle";
	}

	public bool HasPassedState(DestroyObstacleState destroyState)
	{
		return m_State > destroyState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityDestroyObstacle()
	{
	}

	public CAbilityDestroyObstacle(CAbilityDestroyObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_destroyedObstaclesDictionary = references.Get(state.m_destroyedObstaclesDictionary);
		if (m_destroyedObstaclesDictionary == null && state.m_destroyedObstaclesDictionary != null)
		{
			m_destroyedObstaclesDictionary = new Dictionary<string, int>(state.m_destroyedObstaclesDictionary.Comparer);
			foreach (KeyValuePair<string, int> item in state.m_destroyedObstaclesDictionary)
			{
				string key = item.Key;
				int value = item.Value;
				m_destroyedObstaclesDictionary.Add(key, value);
			}
			references.Add(state.m_destroyedObstaclesDictionary, m_destroyedObstaclesDictionary);
		}
		m_NumberOfTilesBlockedDestroyed = state.m_NumberOfTilesBlockedDestroyed;
	}
}
