using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityCreate : CAbility
{
	public enum CreateState
	{
		SelectObstaclePositions,
		PreCreateObstacles,
		CreateObstacles,
		CreateDone
	}

	private CreateState m_State;

	private int m_PropsSpawned;

	public string PropName { get; set; }

	public CAbilityCreate(string propName)
	{
		if (base.TileFilter == CAbilityFilter.EFilterTile.None)
		{
			base.TileFilter = CAbilityFilter.EFilterTile.EmptyHex;
		}
		PropName = propName;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = CreateState.SelectObstaclePositions;
		if (base.IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		m_PropsSpawned = 0;
		if (base.AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
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
		CPlayerSelectingObjectPosition_MessageData message3;
		switch (m_State)
		{
		case CreateState.SelectObstaclePositions:
		{
			if (base.UseSubAbilityTargeting)
			{
				if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
						if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
							{
								base.TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: true));
							}
							base.TilesInRange = base.TilesInRange.Distinct().ToList();
							foreach (CTile inlineSubAbilityTile2 in base.InlineSubAbilityTiles)
							{
								base.TilesInRange.Remove(inlineSubAbilityTile2);
							}
							goto IL_037d;
						}
					}
					base.TilesInRange = base.InlineSubAbilityTiles.ToList();
				}
				else if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
						if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile item in base.ParentAbility.TilesSelected)
							{
								base.TilesInRange.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: true));
							}
							base.TilesInRange = base.TilesInRange.Distinct().ToList();
							foreach (CTile item2 in base.ParentAbility.TilesSelected)
							{
								base.TilesInRange.Remove(item2);
							}
							goto IL_037d;
						}
					}
					base.TilesInRange = base.ParentAbility.TilesSelected.ToList();
				}
				goto IL_037d;
			}
			if (base.IsMergedAbility)
			{
				AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
				if (miscAbilityData6 == null || !miscAbilityData6.IgnoreMergedAbilityTargetSelection.HasValue)
				{
					if (base.ParentAbility != null && base.ParentAbility is CAbilityMerged cAbilityMerged)
					{
						CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
						if (mergedWithAbility != null && mergedWithAbility.TilesSelected != null && mergedWithAbility.TilesSelected.Count > 0)
						{
							base.TilesInRange.Clear();
							foreach (CTile item3 in mergedWithAbility.TilesSelected)
							{
								AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
								if (miscAbilityData7 != null && miscAbilityData7.UseMergedWithAbilityTiles.HasValue)
								{
									AbilityData.MiscAbilityData miscAbilityData8 = base.MiscAbilityData;
									if (miscAbilityData8 != null && miscAbilityData8.UseMergedWithAbilityTiles.Value)
									{
										base.TilesInRange.Add(item3);
										continue;
									}
								}
								CActor cActor = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
								if (cActor != null)
								{
									m_ActorsToIgnore.Add(cActor);
								}
								base.TilesInRange.AddRange(GameState.GetTilesInRange(item3.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
							}
						}
						if (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value)
						{
							foreach (CTile item4 in base.TilesInRange)
							{
								SelectParentAbilityTiles(item4);
							}
							m_State = CreateState.PreCreateObstacles;
							Perform();
							break;
						}
					}
					goto IL_06f4;
				}
			}
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: true, ignoreBlocked: true);
			List<CTile> list3 = new List<CTile>();
			foreach (CTile item5 in base.TilesInRange)
			{
				List<CTile> extraUnblockedTiles2 = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles2);
				if (CAbilityFilter.IsValidTile(item5, base.TileFilter) && !MF.IsBlockingSpawnPosition(item5, base.TilesSelected, base.TargetingActor, ref extraUnblockedTiles2))
				{
					list3.Add(item5);
				}
			}
			base.TilesInRange = list3.ToList();
			goto IL_06f4;
		}
		case CreateState.PreCreateObstacles:
			m_CanUndo = false;
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = CreateState.CreateObstacles;
			Perform();
			break;
		case CreateState.CreateObstacles:
			{
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				if (m_TilesSelected.Count != 0 && m_PropsSpawned == 0)
				{
					if (base.AreaEffect != null)
					{
						base.AbilityHasHappened = true;
						CPlacingSpawn_MessageData message = new CPlacingSpawn_MessageData(base.AnimOverload, base.TargetingActor)
						{
							m_Ability = this,
							m_ActorPlacingSpawn = base.TargetingActor
						};
						ScenarioRuleClient.MessageHandler(message);
						List<TileIndex> list = new List<TileIndex>();
						foreach (CTile item6 in m_TilesSelected)
						{
							list.Add(new TileIndex(item6.m_ArrayIndex));
						}
						CVector3 cVector = null;
						if (base.AreaEffect != null)
						{
							float y = 180f - base.AreaEffectAngle;
							cVector = new CVector3(0f, y, 0f);
						}
						else
						{
							float y2 = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
							cVector = new CVector3(0f, y2, 0f);
						}
						CObjectObstacle prop = new CObjectObstacle(PropName, ScenarioManager.ObjectImportType.Obstacle, list[0], null, cVector, list, base.TargetingActor, m_TilesSelected[0].m_HexMap.MapGuid, ignoresFlyAndJump: false, setPositionToCenterOfAllPathBlockers: true);
						m_TilesSelected[0].SpawnProp(prop, notifyClient: true, base.SpawnDelay);
						m_PropsSpawned++;
					}
					else
					{
						foreach (CTile item7 in m_TilesSelected)
						{
							base.AbilityHasHappened = true;
							CPlacingSpawn_MessageData message2 = new CPlacingSpawn_MessageData(base.AnimOverload, base.TargetingActor)
							{
								m_Ability = this,
								m_ActorPlacingSpawn = base.TargetingActor
							};
							ScenarioRuleClient.MessageHandler(message2);
							List<TileIndex> pathingBlockers = new List<TileIndex>
							{
								new TileIndex(item7.m_ArrayIndex)
							};
							float y3 = 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360);
							CObjectObstacle prop2 = new CObjectObstacle(PropName, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(item7.m_ArrayIndex), null, new CVector3(0f, y3, 0f), pathingBlockers, base.TargetingActor, item7.m_HexMap.MapGuid, ignoresFlyAndJump: false);
							item7.SpawnProp(prop2, notifyClient: true, base.SpawnDelay);
							m_PropsSpawned++;
						}
					}
					base.TargetingActor.m_OnCreatedListeners?.Invoke(this);
				}
				else if (!base.IsMergedAbility)
				{
					PhaseManager.NextStep();
				}
				return true;
			}
			IL_037d:
			if (base.TilesInRange.Count > 0)
			{
				List<CTile> list2 = new List<CTile>();
				foreach (CTile item8 in base.TilesInRange)
				{
					List<CTile> extraUnblockedTiles = new List<CTile>();
					MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles);
					if (CAbilityFilter.IsValidTile(item8, base.TileFilter) && !MF.IsBlockingSpawnPosition(item8, base.TilesSelected, base.TargetingActor, ref extraUnblockedTiles))
					{
						list2.Add(item8);
					}
				}
				base.TilesInRange = list2.ToList();
				if (base.AllTargets)
				{
					foreach (CTile item9 in base.TilesInRange)
					{
						SelectParentAbilityTiles(item9);
					}
					PhaseManager.StepComplete();
					break;
				}
				goto IL_06f4;
			}
			PhaseManager.NextStep();
			return true;
			IL_06f4:
			message3 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
			{
				m_SpawnType = ScenarioManager.ObjectImportType.Obstacle,
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message3);
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
		if (!base.TilesSelected.Contains(selectedTile) && m_State == CreateState.SelectObstaclePositions)
		{
			if (base.AreaEffect != null)
			{
				if (!m_AreaEffectLocked && m_ValidTilesInAreaEffect.Count == base.ValidTilesInAreaEffectedIncludingBlocked.Count && m_ValidTilesInAreaEffect.Count == base.AreaEffect.AllHexes.Count)
				{
					foreach (CTile item in m_ValidTilesInAreaEffect)
					{
						if (!CAbilityFilter.IsValidTile(item, base.TileFilter))
						{
							if (flag)
							{
								Perform();
							}
							base.TileSelected(selectedTile, optionalTileList);
							LogEvent(ESESubTypeAbility.AbilityTileSelected);
							return;
						}
					}
					if (m_ValidTilesInAreaEffect.All((CTile x) => base.TilesInRange.Contains(x)))
					{
						m_TilesSelected.AddRange(m_ValidTilesInAreaEffect);
						m_AreaEffectLocked = true;
						if (base.TargetingActor.Type == CActor.EType.Player)
						{
							CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
							cPlayerSelectedTile_MessageData.m_Ability = this;
							ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
						}
					}
				}
			}
			else if (base.TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < m_NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
						flag = true;
					}
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(base.TargetingActor);
						cPlayerSelectedTile_MessageData2.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(base.TargetingActor));
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
		bool flag2 = false;
		if (base.AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
			m_TilesSelected.Clear();
			flag2 = true;
		}
		else
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Remove(selectedTile);
			}
			flag2 = true;
		}
		if (flag2)
		{
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
			flag = true;
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
		if (m_State == CreateState.SelectObstaclePositions)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == CreateState.SelectObstaclePositions;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == CreateState.SelectObstaclePositions;
		}
		return false;
	}

	public bool SelectParentAbilityTiles(CTile selectedTile)
	{
		if (base.TilesSelected.Contains(selectedTile))
		{
			return m_CanUndo;
		}
		if (m_State == CreateState.SelectObstaclePositions)
		{
			List<CTile> extraUnblockedTiles = new List<CTile>();
			MF.IsBlockingSpawnPosition(null, null, base.TargetingActor, ref extraUnblockedTiles);
			if (!MF.IsBlockingSpawnPosition(selectedTile, m_TilesSelected, base.TargetingActor, ref extraUnblockedTiles))
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				m_CanUndo = false;
				LogEvent(ESESubTypeAbility.AbilityTileSelected);
			}
			else
			{
				LogEvent(ESESubTypeAbility.AbilityTileSelected);
				PhaseManager.StepComplete();
			}
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
		return m_State == CreateState.CreateDone;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreate(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), m_State, PropName, m_PropsSpawned, base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override string GetDescription()
	{
		return "Create Obstacle R: " + m_Range + " N: " + m_NumberTargets;
	}

	public bool HasPassedState(CreateState createState)
	{
		return m_State > createState;
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

	public CAbilityCreate()
	{
	}

	public CAbilityCreate(CAbilityCreate state, ReferenceDictionary references)
		: base(state, references)
	{
		PropName = state.PropName;
		m_State = state.m_State;
		m_PropsSpawned = state.m_PropsSpawned;
	}
}
