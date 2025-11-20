using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityPull : CAbility
{
	public enum EPullState
	{
		None,
		SelectTileToPullTowards,
		PostSelectTileToPullTowards,
		SelectPullTarget,
		ActorIsSelectingPullTile,
		ActorIsPulling,
		ActorHasPulled,
		ActorFinalizePull,
		CheckForNextTarget,
		PullDone
	}

	public enum EPullType
	{
		None,
		PullTowardsActor,
		PullTowardsTile
	}

	[Serializable]
	public class PulledActorStats
	{
		public string Name;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public PulledActorStats(string name, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}

		public PulledActorStats()
		{
		}

		public PulledActorStats(PulledActorStats state, ReferenceDictionary references)
		{
			Name = state.Name;
			DistanceMoved = state.DistanceMoved;
			MovedFromPoint = references.Get(state.MovedFromPoint);
			if (MovedFromPoint == null && state.MovedFromPoint != null)
			{
				MovedFromPoint = new TileIndex(state.MovedFromPoint, references);
				references.Add(state.MovedFromPoint, MovedFromPoint);
			}
			MovedToPoint = references.Get(state.MovedToPoint);
			if (MovedToPoint == null && state.MovedToPoint != null)
			{
				MovedToPoint = new TileIndex(state.MovedToPoint, references);
				references.Add(state.MovedToPoint, MovedToPoint);
			}
		}
	}

	public static EPullType[] PullTypes = (EPullType[])Enum.GetValues(typeof(EPullType));

	public static EPullState[] PullStates = (EPullState[])Enum.GetValues(typeof(EPullState));

	private EPullState m_State;

	private CTile m_PlayerDestination;

	private List<CTile> m_PlayerWaypoints;

	private List<Point> m_AbilityPath;

	private CActor m_CurrentTarget;

	private bool m_TileSelected;

	private Dictionary<CActor, PulledActorStats> m_ActorDistanceMovedDictionary;

	private Point m_StartingPoint;

	public int RemainingPulls { get; set; }

	public CActor CurrentTarget => m_CurrentTarget;

	public Point PullToPoint { get; private set; }

	public EPullType PullType { get; set; }

	public CAbilityPull(EPullType pullType)
	{
		PullType = pullType;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Ability Started");
		base.Start(targetingActor, filterActor, controllingActor);
		RemainingPulls = m_Strength;
		PullToPoint = targetingActor.ArrayIndex;
		if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || base.TargetingActor.Type != CActor.EType.Player || base.IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		if (base.TargetThisActorAutomatically != null)
		{
			if (!CAbility.ImmuneToAbility(base.TargetThisActorAutomatically, this))
			{
				base.ValidActorsInRange = new List<CActor> { base.TargetThisActorAutomatically };
				m_CurrentTarget = base.TargetThisActorAutomatically;
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
				}
			}
			else
			{
				m_CancelAbility = true;
			}
		}
		else if (base.UseSubAbilityTargeting)
		{
			if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
			{
				base.TilesInRange = base.InlineSubAbilityTiles.ToList();
				foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
					if (cActor != null && !m_ValidActorsInRange.Contains(cActor) && base.AbilityFilter.IsValidTarget(cActor, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false))
					{
						m_ValidActorsInRange.Add(cActor);
					}
				}
			}
			else if (base.ParentAbility.AreaEffect != null || base.ParentAbility.AreaEffectBackup != null)
			{
				m_ValidTilesInAreaEffect = base.ParentAbility.ValidTilesInAreaAffected.ToList();
				base.ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffect);
				m_ActorsToTarget = base.ValidActorsInRange.ToList();
			}
			else if (base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter) && base.ParentAbility?.ActorsTargeted != null)
			{
				base.ValidActorsInRange = base.ParentAbility.ActorsTargeted.ToList();
			}
			else
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			m_NumberTargets = base.ValidActorsInRange.Count;
		}
		else
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		for (int num = base.ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor2 = base.ValidActorsInRange[num];
			if (cActor2 != null)
			{
				bool flag = false;
				if (cActor2.IsDead)
				{
					flag = true;
				}
				if (CAbility.ImmuneToAbility(cActor2, this))
				{
					flag = true;
				}
				if (cActor2.Tokens.HasKey(CCondition.EPositiveCondition.Immovable))
				{
					flag = true;
				}
				if (flag)
				{
					base.ValidActorsInRange.Remove(cActor2);
				}
			}
		}
		foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(base.TargetingActor))
		{
			if (base.ValidActorsInRange.Contains(allAdjacentActor))
			{
				base.ValidActorsInRange.Remove(allAdjacentActor);
			}
		}
		if (base.UseSubAbilityTargeting)
		{
			if (base.ValidActorsInRange.Count <= 0)
			{
				m_CancelAbility = true;
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && base.ParentAbility != null && base.ParentAbility is CAbilityAttack cAbilityAttack && !cPhaseAction.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability != this && (x.m_Ability.AbilityType == EAbilityType.Push || x.m_Ability.AbilityType == EAbilityType.Pull)))
				{
					CActor cActor3 = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
					if (base.TargetThisActorAutomatically != null)
					{
						if (ScenarioManager.Scenario.HasActor(cActor3) && ScenarioManager.Scenario.HasActor(base.TargetThisActorAutomatically) && !base.ActorsTargeted.Contains(base.TargetThisActorAutomatically))
						{
							cActor3.ApplyRetaliateToAttack(base.TargetThisActorAutomatically, cAbilityAttack);
						}
					}
					else
					{
						foreach (CActor item in base.ParentAbility?.ActorsTargeted)
						{
							if (ScenarioManager.Scenario.HasActor(cActor3) && ScenarioManager.Scenario.HasActor(item) && !base.ActorsTargeted.Contains(item))
							{
								cActor3.ApplyRetaliateToAttack(item, cAbilityAttack);
							}
						}
					}
				}
			}
			else if (m_ValidActorsInRange.Count == 1)
			{
				m_CurrentTarget = m_ValidActorsInRange[0];
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
				}
			}
		}
		if (base.ValidActorsInRange.Count <= 0 && base.TargetingActor.Type != CActor.EType.Player)
		{
			m_CancelAbility = true;
		}
		if (base.UseSubAbilityTargeting || base.IsModifierAbility)
		{
			m_NumberTargets = base.ValidActorsInRange.Count;
		}
		if (m_NumberTargets == -1)
		{
			m_NumberTargetsRemaining = base.ValidActorsInRange.Count;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else
		{
			m_NumberTargetsRemaining = m_NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (m_CurrentTarget == null || base.TargetingActor.Type != CActor.EType.Player)
		{
			if (PullType.Equals(EPullType.PullTowardsTile))
			{
				m_State = EPullState.SelectTileToPullTowards;
			}
			else if (base.Strength <= 0)
			{
				m_State = EPullState.ActorHasPulled;
			}
			else
			{
				m_State = EPullState.SelectPullTarget;
			}
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorsToTarget.Clear();
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PulledActorStats>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Performing state: " + m_State);
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0096;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0096;
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
		case EPullState.SelectTileToPullTowards:
		{
			CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
			{
				m_SpawnType = ScenarioManager.ObjectImportType.None,
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		case EPullState.PostSelectTileToPullTowards:
		{
			m_CanUndo = false;
			base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, base.Range, base.ActorsToIgnore, base.AbilityFilter, null, null, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
			CActor cActor2 = ScenarioManager.Scenario.FindActorAt(PullToPoint);
			if (cActor2 != null && base.AbilityFilter.IsValidTarget(cActor2, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
			{
				base.ValidActorsInRange.Add(cActor2);
			}
			if (m_AllTargets)
			{
				m_NumberTargets = base.ValidActorsInRange.Count;
				m_NumberTargetsRemaining = m_NumberTargets;
				m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
				m_AllTargets = false;
			}
			CActorSelectedTileToPullTowards message3 = new CActorSelectedTileToPullTowards(base.AnimOverload, base.TargetingActor)
			{
				m_PullAbility = this,
				m_TileToPullTowards = ScenarioManager.Tiles[PullToPoint.X, PullToPoint.Y]
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case EPullState.SelectPullTarget:
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				if (base.AreaEffect == null)
				{
					int num5 = base.ValidActorsInRange.Count - 1;
					while (num5 >= 0 && base.ValidActorsInRange.Count > m_NumberTargetsRemaining)
					{
						base.ValidActorsInRange.RemoveAt(num5);
						num5--;
					}
				}
				if (base.ValidActorsInRange.Count > 0)
				{
					m_CurrentTarget = GetNextTarget();
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPullState.ActorIsPulling;
					Perform();
				}
				else
				{
					m_CancelAbility = true;
					PhaseManager.NextStep();
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor)
				{
					m_IsPositive = false,
					m_TargetingAbility = this
				});
			}
			break;
		case EPullState.ActorIsSelectingPullTile:
			if (m_CurrentTarget == null || !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
			{
				CheckNextTarget();
			}
			else if (base.TargetingActor.Type == CActor.EType.Player && m_PlayerDestination != null)
			{
				SimpleLog.AddToSimpleLog("[PULL ABILITY] - Player location: " + m_CurrentTarget.ArrayIndex.ToString());
				SimpleLog.AddToSimpleLog("[PULL ABILITY] - Player destination: " + m_PlayerDestination.m_ArrayIndex.ToString());
				SimpleLog.AddToSimpleLog("[PULL ABILITY] - Generating path using passed in waypoints");
				string text = "[PULL ABILITY] - In waypoints: ";
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					text = text + "\n" + playerWaypoint.m_ArrayIndex.ToString();
				}
				SimpleLog.AddToSimpleLog(text);
				CAbilityMove.GetNextPathingPositionUsingExistingWaypoints(m_CurrentTarget, m_PlayerDestination, jump: false, fly: false, m_PlayerWaypoints, out m_PlayerWaypoints, out m_AbilityPath, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, ignoreMoveCost: true, logFailure: true);
				string text2 = "[PULL ABILITY] - Out waypoints: ";
				foreach (CTile playerWaypoint2 in m_PlayerWaypoints)
				{
					text2 = text2 + "\n" + playerWaypoint2.m_ArrayIndex.ToString();
				}
				SimpleLog.AddToSimpleLog(text2);
				string text3 = "[PULL ABILITY] - Out ability path: ";
				foreach (Point item2 in m_AbilityPath)
				{
					text3 = text3 + "\n" + item2.ToString();
				}
				SimpleLog.AddToSimpleLog(text3);
				for (int num2 = 0; num2 < m_AbilityPath.Count; num2++)
				{
					Point point2 = m_AbilityPath[num2];
					CTile cTile2 = ScenarioManager.Tiles[point2.X, point2.Y];
					bool flag2 = false;
					for (int num3 = cTile2.m_Props.Count - 1; num3 >= 0; num3--)
					{
						if (cTile2.m_Props[num3].ObjectType != ScenarioManager.ObjectImportType.Door && cTile2.m_Props[num3].WillActivationKillActor(m_CurrentTarget))
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						SimpleLog.AddToSimpleLog("[PULL ABILITY] - Truncating pull path as prop activation will kill target along the way");
						m_PlayerDestination = cTile2;
						for (int num4 = num2 + 1; num4 < m_AbilityPath.Count; num4++)
						{
							CTile item = ScenarioManager.Tiles[m_AbilityPath[num4].X, m_AbilityPath[num4].Y];
							m_PlayerWaypoints.Remove(item);
						}
						m_AbilityPath.RemoveRange(num2 + 1, m_AbilityPath.Count - (num2 + 1));
						break;
					}
				}
				if (m_ActorDistanceMovedDictionary.TryGetValue(m_CurrentTarget, out var value))
				{
					value.DistanceMoved = m_AbilityPath.Count;
				}
				else
				{
					value = new PulledActorStats(m_CurrentTarget.GetPrefabName() + ((m_CurrentTarget is CEnemyActor cEnemyActor) ? (" " + cEnemyActor.ID) : ""), m_AbilityPath.Count, new TileIndex(m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y), new TileIndex(m_PlayerDestination.m_ArrayIndex.X, m_PlayerDestination.m_ArrayIndex.Y));
				}
				m_ActorDistanceMovedDictionary[m_CurrentTarget] = value;
				SimpleLog.AddToSimpleLog("[PULL ABILITY] - Updating Current Pull Target (" + m_CurrentTarget.GetPrefabName() + " " + m_CurrentTarget.ID + ") array index to: " + m_PlayerDestination.m_ArrayIndex.ToString());
				m_CurrentTarget.ArrayIndex = m_PlayerDestination.m_ArrayIndex;
				m_State = EPullState.ActorIsPulling;
				Perform();
			}
			else
			{
				m_TileSelected = false;
				CActorIsSelectingPullTile_MessageData message2 = new CActorIsSelectingPullTile_MessageData(base.TargetingActor)
				{
					m_PullAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			break;
		case EPullState.ActorIsPulling:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (base.TargetingActor.Type == CActor.EType.Player && !m_TileSelected)
			{
				CheckNextTarget();
			}
			else if (base.TargetingActor.Type != CActor.EType.Player)
			{
				Point point = (m_StartingPoint = m_CurrentTarget.ArrayIndex);
				m_CurrentTarget.Pull(base.TargetingActor.ArrayIndex, base.TargetingActor, RemainingPulls, PullType, out m_AbilityPath);
				if (m_CurrentTarget.ArrayIndex != point)
				{
					CActorIsPulling_MessageData cActorIsPulling_MessageData = new CActorIsPulling_MessageData(base.TargetingActor);
					cActorIsPulling_MessageData.m_PullAbility = this;
					cActorIsPulling_MessageData.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
					ScenarioRuleClient.MessageHandler(cActorIsPulling_MessageData);
				}
				else
				{
					ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
				}
			}
			else
			{
				CActorIsPulling_MessageData cActorIsPulling_MessageData2 = new CActorIsPulling_MessageData(base.TargetingActor);
				cActorIsPulling_MessageData2.m_PullAbility = this;
				cActorIsPulling_MessageData2.m_Waypoints = m_PlayerWaypoints;
				ScenarioRuleClient.MessageHandler(cActorIsPulling_MessageData2);
			}
			break;
		case EPullState.ActorHasPulled:
		{
			CActorHasPulled_MessageData cActorHasPulled_MessageData = new CActorHasPulled_MessageData(base.TargetingActor);
			cActorHasPulled_MessageData.m_PullAbility = this;
			cActorHasPulled_MessageData.m_Waypoints = m_PlayerWaypoints;
			ScenarioRuleClient.MessageHandler(cActorHasPulled_MessageData);
			break;
		}
		case EPullState.ActorFinalizePull:
		{
			base.AbilityHasHappened = true;
			for (int i = 0; i < m_AbilityPath.Count; i++)
			{
				Point firstPoint = ((i == 0) ? m_StartingPoint : m_AbilityPath[i - 1]);
				GameState.LostAdjacency(m_CurrentTarget, firstPoint, m_AbilityPath[i]);
			}
			foreach (Point item3 in m_AbilityPath)
			{
				CTile cTile = ScenarioManager.Tiles[item3.X, item3.Y];
				for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
				{
					bool flag = false;
					if (cTile.m_Props[num].WillActivationKillActor(m_CurrentTarget))
					{
						flag = true;
						m_CurrentTarget.ArrayIndex = item3;
					}
					if (cTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.PressurePlate && cTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.Portal)
					{
						cTile.m_Props[num].AutomaticActivate(m_CurrentTarget);
					}
					else if (cTile.m_Props[num] is CObjectPressurePlate cObjectPressurePlate && m_CurrentTarget.Class is CCharacterClass && (cObjectPressurePlate.PressurePlateType == CObjectPressurePlate.EPressurePlateType.ActivateOnce || (m_CurrentTarget.ArrayIndex.X == cObjectPressurePlate.ArrayIndex.X && m_CurrentTarget.ArrayIndex.Y == cObjectPressurePlate.ArrayIndex.Y && !flag)))
					{
						cTile.m_Props[num].AutomaticActivate(m_CurrentTarget);
					}
				}
				m_CurrentTarget.ArrayIndex = item3;
				CActiveBonus.RefreshAllAuraActiveBonuses();
				m_CurrentTarget.CalculateAttackStrengthForUI();
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, new List<CActor>(), newActorCarried: false, 1, finalMovement: false, cTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), cTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1);
				if (m_CurrentTarget.Health <= 0)
				{
					m_CurrentTarget.ArrayIndex = item3;
					break;
				}
			}
			if (base.ParentAbility != null && base.ParentAbility is CAbilityAttack cAbilityAttack)
			{
				CActor cActor = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
				if (ScenarioManager.Scenario.HasActor(cActor) && ScenarioManager.Scenario.HasActor(m_CurrentTarget))
				{
					cActor.ApplyRetaliateToAttack(m_CurrentTarget, cAbilityAttack);
				}
			}
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			PhaseManager.StepComplete();
			return true;
		}
		case EPullState.CheckForNextTarget:
			CheckNextTarget();
			return true;
		}
		return false;
		IL_0096:
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
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Tile selected");
		bool flag = false;
		if (m_State == EPullState.SelectTileToPullTowards)
		{
			if (PullToPoint != selectedTile.m_ArrayIndex)
			{
				PullToPoint = selectedTile.m_ArrayIndex;
				m_TilesSelected.Clear();
				m_TilesSelected.Add(selectedTile);
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
			}
		}
		else if (m_State == EPullState.SelectPullTarget)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex);
			if (cActor != null && cActor != m_CurrentTarget && !base.ActorsTargeted.Contains(cActor) && base.ValidActorsInRange.Contains(cActor))
			{
				if (m_CurrentTarget != null)
				{
					CTile item = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
					m_ActorsToTarget.Remove(m_CurrentTarget);
					m_TilesSelected.Remove(item);
				}
				m_CurrentTarget = cActor;
				m_ActorsToTarget.Add(cActor);
				m_TilesSelected.Add(selectedTile);
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(base.TargetingActor);
					cPlayerSelectedTile_MessageData2.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
				}
				if (base.TargetingActor.Type != CActor.EType.Player)
				{
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPullState.ActorIsPulling;
					flag = true;
				}
				else if (base.TargetingActor.Type == CActor.EType.Player && base.UseSubAbilityTargeting)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
					flag = true;
				}
			}
		}
		else if (m_State == EPullState.ActorIsSelectingPullTile)
		{
			m_TileSelected = true;
			m_PlayerDestination = selectedTile;
			m_PlayerWaypoints = optionalTileList;
			m_CanUndo = false;
			flag = true;
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
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Tile deselected");
		bool flag = false;
		if (m_State == EPullState.SelectTileToPullTowards)
		{
			if (PullToPoint == selectedTile.m_ArrayIndex)
			{
				m_TilesSelected.Remove(selectedTile);
				flag = true;
			}
		}
		else if (m_State == EPullState.SelectPullTarget && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			base.ValidActorsInRange.Add(m_CurrentTarget);
			m_ActorsToTarget.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			flag = true;
		}
		if (flag && base.TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
			{
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message);
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
		m_CurrentTarget = null;
		if (m_State == EPullState.SelectTileToPullTowards || m_State == EPullState.SelectPullTarget)
		{
			Perform();
		}
	}

	public static bool IsClosedDoor(CTile tile)
	{
		bool result = false;
		if (((tile.m_HexMap != null && tile.m_Hex2Map != null) || tile.FindProp(ScenarioManager.ObjectImportType.Door) != null) && ((tile.m_HexMap != null && !tile.m_HexMap.Revealed) || (tile.m_Hex2Map != null && !tile.m_Hex2Map.Revealed) || !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			result = true;
		}
		return result;
	}

	public override bool CanClearTargets()
	{
		if (m_State != EPullState.SelectTileToPullTowards && m_State != EPullState.SelectPullTarget)
		{
			return m_State == EPullState.ActorIsSelectingPullTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EPullState.SelectTileToPullTowards && m_State != EPullState.SelectPullTarget)
			{
				return m_State == EPullState.ActorIsSelectingPullTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State != EPullState.SelectPullTarget)
		{
			return true;
		}
		return m_ActorsToTarget.Count > 0;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == EPullState.ActorIsSelectingPullTile;
	}

	public CActor GetNextTarget()
	{
		if (m_ValidActorsInRange.Count > 0)
		{
			CActor result = m_ValidActorsInRange[0];
			m_ValidActorsInRange.RemoveAt(0);
			return result;
		}
		return null;
	}

	public override void Update()
	{
		if (m_State == EPullState.ActorIsPulling && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
		{
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			CheckNextTarget();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State != EPullState.PullDone)
		{
			SimpleLog.AddToSimpleLog("[PULL ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		else
		{
			SimpleLog.AddToSimpleLog("[PULL ABILITY] - Ability complete check not moving state");
		}
		return m_State == EPullState.PullDone;
	}

	public override string GetDescription()
	{
		return "Pull(" + base.Strength + ")";
	}

	public static List<CTile> GetPullTiles(CActor pullingActor, CActor.EType targetType, Point pullTarget, Point pullToPoint, EPullType pullType, int remainingPulls = int.MaxValue)
	{
		List<CTile> tiles = new List<CTile>();
		bool foundPath;
		List<Point> list = CActor.FindCharacterPath(pullingActor, pullTarget, pullToPoint, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (!foundPath || list.Count == 0)
		{
			return tiles;
		}
		GetPullTilesRecursive(pullingActor, ref tiles, ScenarioManager.Tiles[pullTarget.X, pullTarget.Y], list.Count, targetType, pullToPoint, remainingPulls);
		if (pullType.Equals(EPullType.PullTowardsTile))
		{
			bool flag = false;
			if (pullingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pullingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag = fly;
			}
			if (tiles.Count > 0 || list.Count == 1)
			{
				CTile cTile = ScenarioManager.Tiles[pullToPoint.X, pullToPoint.Y];
				CNode cNode = ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y];
				CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
				if (cNode.Walkable && (!cNode.Blocked || flag) && (cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump) && cActor == null)
				{
					tiles.Add(ScenarioManager.Tiles[pullToPoint.X, pullToPoint.Y]);
				}
			}
		}
		return tiles;
	}

	private static void GetPullTilesRecursive(CActor pullingActor, ref List<CTile> tiles, CTile currentTile, int distance, CActor.EType targetType, Point pullToPoint, int remainingPulls)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			bool foundPath;
			List<Point> list2 = CActor.FindCharacterPath(pullingActor, currentTile.m_ArrayIndex, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
			if (!foundPath || list2.Count != 1)
			{
				continue;
			}
			bool foundPath2;
			List<Point> list3 = CActor.FindCharacterPath(pullingActor, pullToPoint, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2);
			bool num = IsClosedDoor(allAdjacentTile);
			bool flag = false;
			if (pullingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pullingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag = fly;
			}
			if (!(!num && foundPath2) || list3.Count >= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || flag) || (allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) != null && ((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump))
			{
				continue;
			}
			List<CActor> list4 = ScenarioManager.Scenario.FindActorsAt(allAdjacentTile.m_ArrayIndex);
			bool flag2 = false;
			bool flag3 = false;
			foreach (CActor item in list4)
			{
				if (item != null && !(item is CHeroSummonActor { DoesNotBlock: not false }))
				{
					if (!CActor.AreActorsAllied(item.Type, targetType) || remainingPulls <= 1)
					{
						flag2 = true;
					}
					else
					{
						flag3 = true;
					}
				}
			}
			if (!flag2)
			{
				list.Add(allAdjacentTile);
				if (!flag3)
				{
					tiles.Add(allAdjacentTile);
				}
			}
		}
		if (remainingPulls - 1 <= 0)
		{
			return;
		}
		foreach (CTile item2 in list)
		{
			GetPullTilesRecursive(pullingActor, ref tiles, item2, distance - 1, targetType, pullToPoint, remainingPulls - 1);
		}
	}

	private void CheckNextTarget()
	{
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Checking for next target");
		if (m_CurrentTarget != null)
		{
			GameState.ActorHealthCheck(base.TargetingActor, m_CurrentTarget);
			base.ActorsTargeted.Add(m_CurrentTarget);
			m_ActorsToTarget.Remove(m_CurrentTarget);
			if (base.ResourcesToTakeFromTargets != null && base.ResourcesToTakeFromTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in base.ResourcesToTakeFromTargets)
				{
					if (m_CurrentTarget.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
					{
						m_CurrentTarget.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
						base.TargetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					}
				}
			}
			if (base.ResourcesToGiveToTargets != null && base.ResourcesToGiveToTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToGiveToTarget in base.ResourcesToGiveToTargets)
				{
					if (base.TargetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
					{
						base.TargetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
						m_CurrentTarget.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					}
				}
			}
		}
		m_NumberTargetsRemaining--;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (m_NumberTargetsRemaining > 0)
		{
			m_State = EPullState.SelectPullTarget;
			base.ValidActorsInRange.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			m_PlayerDestination = null;
			RemainingPulls = m_Strength;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == EAbilityType.Pull)
		{
			PhaseManager.NextStep();
		}
	}

	public override void AbilityEnded()
	{
		SimpleLog.AddToSimpleLog("[PULL ABILITY] - Ability Ended");
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<PulledActorStats> actorsDistanceMoved = m_ActorDistanceMovedDictionary.Values.ToList();
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityPull(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, actorsDistanceMoved, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public bool HasPassedState(EPullState pullState)
	{
		return m_State > pullState;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityPull()
	{
	}

	public CAbilityPull(CAbilityPull state, ReferenceDictionary references)
		: base(state, references)
	{
		RemainingPulls = state.RemainingPulls;
		PullType = state.PullType;
		m_State = state.m_State;
		m_PlayerWaypoints = references.Get(state.m_PlayerWaypoints);
		if (m_PlayerWaypoints == null && state.m_PlayerWaypoints != null)
		{
			m_PlayerWaypoints = new List<CTile>();
			for (int i = 0; i < state.m_PlayerWaypoints.Count; i++)
			{
				CTile cTile = state.m_PlayerWaypoints[i];
				CTile cTile2 = references.Get(cTile);
				if (cTile2 == null && cTile != null)
				{
					cTile2 = new CTile(cTile, references);
					references.Add(cTile, cTile2);
				}
				m_PlayerWaypoints.Add(cTile2);
			}
			references.Add(state.m_PlayerWaypoints, m_PlayerWaypoints);
		}
		m_AbilityPath = references.Get(state.m_AbilityPath);
		if (m_AbilityPath == null && state.m_AbilityPath != null)
		{
			m_AbilityPath = new List<Point>();
			for (int j = 0; j < state.m_AbilityPath.Count; j++)
			{
				Point item = state.m_AbilityPath[j];
				m_AbilityPath.Add(item);
			}
			references.Add(state.m_AbilityPath, m_AbilityPath);
		}
		m_TileSelected = state.m_TileSelected;
		m_ActorDistanceMovedDictionary = references.Get(state.m_ActorDistanceMovedDictionary);
		if (m_ActorDistanceMovedDictionary != null || state.m_ActorDistanceMovedDictionary == null)
		{
			return;
		}
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PulledActorStats>(state.m_ActorDistanceMovedDictionary.Comparer);
		foreach (KeyValuePair<CActor, PulledActorStats> item2 in state.m_ActorDistanceMovedDictionary)
		{
			CActor cActor = references.Get(item2.Key);
			if (cActor == null && item2.Key != null)
			{
				cActor = new CActor(item2.Key, references);
				references.Add(item2.Key, cActor);
			}
			PulledActorStats pulledActorStats = references.Get(item2.Value);
			if (pulledActorStats == null && item2.Value != null)
			{
				pulledActorStats = new PulledActorStats(item2.Value, references);
				references.Add(item2.Value, pulledActorStats);
			}
			m_ActorDistanceMovedDictionary.Add(cActor, pulledActorStats);
		}
		references.Add(state.m_ActorDistanceMovedDictionary, m_ActorDistanceMovedDictionary);
	}
}
