using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityFear : CAbility
{
	public enum EFearState
	{
		None,
		SelectFearTarget,
		ActorIsSelectingFearToTile,
		ActorIsFearing,
		ActorIsMoving,
		ActorHasMoved,
		ActorFinalizeFear,
		CheckForNextTarget,
		FearDone
	}

	[Serializable]
	public class FearedActorStats
	{
		public string Name;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public FearedActorStats(string name, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}

		public FearedActorStats()
		{
		}

		public FearedActorStats(FearedActorStats state, ReferenceDictionary references)
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

	public static EFearState[] FearStates = (EFearState[])Enum.GetValues(typeof(EFearState));

	private EFearState m_State;

	private CTile m_FearDestination;

	private List<Point> m_FearPath;

	private CActor m_CurrentTarget;

	private Point m_CurrentTargetStartArrayIndex;

	private int m_CurrentTargetDistanceMoved;

	private bool m_CancelFearOnCurrentTarget;

	private Point m_StartingPoint;

	private Dictionary<CActor, FearedActorStats> m_ActorDistanceMovedDictionary;

	private bool m_Jump;

	private bool m_Fly;

	private bool m_IgnoreDifficultTerrain;

	private bool m_IgnoreHazardousTerrain;

	private bool m_CurrentTargetJump;

	private bool m_CurrentTargetFly;

	private bool m_CurrentTargetIgnoreDifficult;

	private bool m_CurrentTargetIgnoreHazardous;

	private CTile m_LastTileChecked;

	public int RemainingFears { get; private set; }

	public CActor CurrentTarget => m_CurrentTarget;

	public CAbilityFear(bool jump, bool fly, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false)
	{
		m_Jump = jump;
		m_Fly = fly;
		m_IgnoreDifficultTerrain = ignoreDifficultTerrain;
		m_IgnoreHazardousTerrain = ignoreHazardousTerrain;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		RemainingFears = m_Strength;
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, FearedActorStats>();
		m_State = EFearState.SelectFearTarget;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
			m_NumberTargetsRemaining = base.ValidActorsInRange.Count;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else
		{
			m_NumberTargetsRemaining = m_NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (base.ValidActorsInRange.Count <= 0)
		{
			m_CancelAbility = true;
		}
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
		case EFearState.SelectFearTarget:
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				base.ValidActorsInRange.Sort((CActor x, CActor y) => x.Initiative().CompareTo(y.Initiative()));
				if (base.AreaEffect == null && !base.AllTargets)
				{
					int num3 = base.ValidActorsInRange.Count - 1;
					while (num3 >= 0 && base.ValidActorsInRange.Count > m_NumberTargets)
					{
						base.ValidActorsInRange.RemoveAt(num3);
						num3--;
					}
				}
				m_CurrentTarget = GetNextTarget();
				m_CurrentTargetStartArrayIndex = m_CurrentTarget.ArrayIndex;
				if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
				}
				m_State = EFearState.ActorIsSelectingFearToTile;
				Perform();
			}
			else
			{
				DLLDebug.LogError("Fear ability not implemented for players");
			}
			break;
		case EFearState.ActorIsSelectingFearToTile:
			m_FearPath = GetFearToTilePath();
			m_CurrentTargetDistanceMoved = m_FearPath.Count;
			m_State = EFearState.ActorIsFearing;
			Perform();
			break;
		case EFearState.ActorIsFearing:
		{
			CActorIsApplyingConditionActiveBonus_MessageData message3 = new CActorIsApplyingConditionActiveBonus_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_Ability = this,
				m_ActorsAppliedTo = new List<CActor> { m_CurrentTarget }
			};
			ScenarioRuleClient.MessageHandler(message3);
			m_StartingPoint = m_CurrentTarget.ArrayIndex;
			break;
		}
		case EFearState.ActorIsMoving:
			if (ScenarioManager.Scenario.HasActor(m_CurrentTarget))
			{
				Point arrayIndex = CurrentTarget.ArrayIndex;
				CurrentTarget.ArrayIndex = m_FearPath.First();
				m_FearPath.Remove(m_FearPath.First());
				if (m_CurrentTarget.ArrayIndex != arrayIndex)
				{
					CTile propTile2 = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
					List<CObjectObstacle> list2 = new List<CObjectObstacle>();
					if (m_CurrentTargetJump || m_CurrentTargetFly)
					{
						CObjectObstacle cObjectObstacle2 = propTile2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
						if (cObjectObstacle2 == null)
						{
							cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(propTile2.m_ArrayIndex, ref propTile2);
						}
						if (cObjectObstacle2 != null)
						{
							list2.Add(cObjectObstacle2);
						}
					}
					CUpdatePropTransparency_MessageData message2 = new CUpdatePropTransparency_MessageData
					{
						m_PropList = list2,
						m_ActorSpawningMessage = m_CurrentTarget
					};
					ScenarioRuleClient.MessageHandler(message2, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
					propTile2.FindProp(ScenarioManager.ObjectImportType.Door)?.AutomaticActivate(base.TargetingActor);
					CActorHasMoved_MessageData cActorHasMoved_MessageData = new CActorHasMoved_MessageData(m_CurrentTarget);
					cActorHasMoved_MessageData.m_Ability = this;
					cActorHasMoved_MessageData.m_Waypoints = new List<CTile> { m_FearDestination };
					cActorHasMoved_MessageData.m_MovingActor = m_CurrentTarget;
					cActorHasMoved_MessageData.m_Jump = m_CurrentTargetJump;
					ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
				}
				else
				{
					ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
				}
			}
			else
			{
				CheckNextTarget();
			}
			break;
		case EFearState.ActorHasMoved:
		{
			base.AbilityHasHappened = true;
			CTile propTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			CTile cTile2 = ScenarioManager.Tiles[m_StartingPoint.X, m_StartingPoint.Y];
			if (propTile != m_LastTileChecked)
			{
				GameState.LostAdjacency(m_CurrentTarget, (m_LastTileChecked == null) ? cTile2 : m_LastTileChecked, propTile);
				for (int num2 = propTile.m_Props.Count - 1; num2 >= 0; num2--)
				{
					CObjectProp cObjectProp2 = propTile.m_Props[num2];
					if (cObjectProp2.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && cObjectProp2.ObjectType != ScenarioManager.ObjectImportType.Portal && ((!m_CurrentTargetJump && !m_CurrentTargetFly) || (cObjectProp2 != null && cObjectProp2.ObjectType == ScenarioManager.ObjectImportType.Door)))
					{
						if ((m_CurrentTarget.OriginalType == CActor.EType.Player || m_CurrentTarget is CHeroSummonActor { IsCompanionSummon: not false }) && !cObjectProp2.Activated && cObjectProp2.WillActivationDamageActor(m_CurrentTarget))
						{
							bool flag = true;
							if (m_CurrentTargetIgnoreHazardous && (cObjectProp2.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || cObjectProp2.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns))
							{
								flag = false;
							}
							if (flag)
							{
								CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(m_CurrentTarget);
								cPauseLoco_MessageData.m_Pause = true;
								ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
							}
							bool flag2 = false;
							flag2 = cObjectProp2.WillActivationKillActor(m_CurrentTarget);
							if (cObjectProp2 is CObjectTrap cObjectTrap && cObjectTrap.Conditions.Count > 0 && (cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Stun) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Immobilize) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Sleep)))
							{
								flag2 = true;
							}
							if (flag2)
							{
								CStopLoco_MessageData cStopLoco_MessageData = new CStopLoco_MessageData(m_CurrentTarget);
								cStopLoco_MessageData.m_Pause = true;
								ScenarioRuleClient.MessageHandler(cStopLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
								m_CancelFearOnCurrentTarget = true;
							}
						}
						cObjectProp2.AutomaticActivate(m_CurrentTarget);
					}
				}
			}
			m_LastTileChecked = propTile;
			List<CObjectObstacle> list = new List<CObjectObstacle>();
			if (m_CurrentTargetJump || m_CurrentTargetFly)
			{
				CObjectObstacle cObjectObstacle = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null)
				{
					list.Add(cObjectObstacle);
				}
			}
			CUpdatePropTransparency_MessageData message = new CUpdatePropTransparency_MessageData
			{
				m_PropList = list,
				m_ActorSpawningMessage = m_CurrentTarget
			};
			ScenarioRuleClient.MessageHandler(message, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			if (!ScenarioManager.Scenario.HasActor(m_CurrentTarget))
			{
				PhaseManager.NextStep();
				return false;
			}
			CActiveBonus.RefreshAllAuraActiveBonuses();
			m_CurrentTarget.CalculateAttackStrengthForUI();
			if (propTile.m_Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) && !m_CurrentTargetJump && !m_CurrentTargetFly)
			{
				_ = m_CurrentTargetIgnoreDifficult;
			}
			m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, new List<CActor>(), newActorCarried: false, 1, finalMovement: false, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1);
			if ((m_FearDestination != null && m_CurrentTarget.ArrayIndex == m_FearDestination.m_ArrayIndex) || m_CancelFearOnCurrentTarget)
			{
				if (m_ActorDistanceMovedDictionary.TryGetValue(m_CurrentTarget, out var value))
				{
					value.DistanceMoved = m_FearPath.Count;
				}
				else
				{
					value = new FearedActorStats(m_CurrentTarget.GetPrefabName() + ((m_CurrentTarget is CEnemyActor cEnemyActor) ? (" " + cEnemyActor.ID) : ""), m_CurrentTargetDistanceMoved, new TileIndex(m_CurrentTargetStartArrayIndex.X, m_CurrentTargetStartArrayIndex.Y), new TileIndex(m_FearDestination.m_ArrayIndex.X, m_FearDestination.m_ArrayIndex.Y));
				}
				m_ActorDistanceMovedDictionary[m_CurrentTarget] = value;
				m_FearDestination = null;
				m_FearPath = null;
				m_CancelFearOnCurrentTarget = false;
				m_State = EFearState.ActorFinalizeFear;
			}
			else
			{
				m_State = EFearState.ActorIsMoving;
			}
			Perform();
			break;
		}
		case EFearState.ActorFinalizeFear:
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			if (m_FearPath != null && m_FearPath.Count > 0)
			{
				CTile cTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
				for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
				{
					CObjectProp cObjectProp = cTile.m_Props[num];
					switch (cObjectProp.ObjectType)
					{
					case ScenarioManager.ObjectImportType.Portal:
						cObjectProp.AutomaticActivate(m_CurrentTarget);
						break;
					default:
						if (m_CurrentTargetJump && !m_CurrentTargetFly)
						{
							cObjectProp.AutomaticActivate(m_CurrentTarget);
						}
						break;
					case ScenarioManager.ObjectImportType.PressurePlate:
						break;
					}
				}
			}
			PhaseManager.StepComplete();
			break;
		case EFearState.CheckForNextTarget:
			CheckNextTarget();
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

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EFearState.FearDone;
	}

	public override string GetDescription()
	{
		return "Fear(" + base.Strength + ")";
	}

	public override void AbilityEnded()
	{
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<FearedActorStats> actorsDistanceMoved = m_ActorDistanceMovedDictionary.Values.ToList();
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityFear(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, actorsDistanceMoved, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override void Restart()
	{
		base.Restart();
	}

	private void CheckNextTarget()
	{
		if (m_CurrentTarget != null)
		{
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
			m_State = EFearState.SelectFearTarget;
			base.ValidActorsInRange.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			RemainingFears = m_Strength;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else
		{
			PhaseManager.NextStep();
		}
	}

	private CActor GetNextTarget()
	{
		if (m_ValidActorsInRange.Count > 0)
		{
			CActor result = m_ValidActorsInRange[0];
			m_ValidActorsInRange.RemoveAt(0);
			return result;
		}
		return null;
	}

	private List<Point> GetFearToTilePath()
	{
		m_CurrentTargetJump = m_Jump;
		m_CurrentTargetFly = m_Fly || CurrentTarget.Flying;
		m_CurrentTargetIgnoreDifficult = m_IgnoreDifficultTerrain || CurrentTarget.IgnoreDifficultTerrain;
		m_CurrentTargetIgnoreHazardous = m_IgnoreHazardousTerrain || CurrentTarget.IgnoreHazardousTerrain;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(CurrentTarget, EAbilityType.Move);
		if (list != null)
		{
			foreach (CActiveBonus item in list)
			{
				if (item is CMoveActiveBonus { BespokeBehaviour: not null } cMoveActiveBonus)
				{
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(this, CurrentTarget).HasValue)
					{
						m_CurrentTargetJump |= ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(this, CurrentTarget).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(this, CurrentTarget).HasValue)
					{
						m_CurrentTargetFly |= ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(this, CurrentTarget).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(this, CurrentTarget).HasValue)
					{
						m_CurrentTargetIgnoreDifficult |= ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(this, CurrentTarget).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(this, CurrentTarget).HasValue)
					{
						m_CurrentTargetIgnoreHazardous |= ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(this, CurrentTarget).Value;
					}
				}
			}
		}
		List<CTile> tilesInRange = GameState.GetTilesInRange(CurrentTarget.ArrayIndex, base.Strength, EAbilityTargeting.Range, emptyTilesOnly: false, m_CurrentTargetFly || m_CurrentTargetJump, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, m_CurrentTargetIgnoreDifficult);
		tilesInRange.Add(ScenarioManager.Tiles[CurrentTarget.ArrayIndex.X, CurrentTarget.ArrayIndex.Y]);
		List<CTile> list2 = new List<CTile>();
		int num = 0;
		foreach (CTile item2 in tilesInRange)
		{
			if (CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex) || item2.m_ArrayIndex.Equals(CurrentTarget.ArrayIndex))
			{
				int tileDistance = ScenarioManager.GetTileDistance(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y);
				if (tileDistance > num)
				{
					num = tileDistance;
					list2.Clear();
				}
				if (tileDistance == num)
				{
					list2.Add(item2);
				}
			}
		}
		m_FearDestination = list2[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list2.Count)];
		bool foundPath;
		List<Point> result = ScenarioManager.PathFinder.FindPath(CurrentTarget.ArrayIndex, m_FearDestination.m_ArrayIndex, m_CurrentTargetJump || m_CurrentTargetFly, ignoreMoveCost: false, out foundPath, ignoreBridges: false, m_CurrentTargetIgnoreDifficult);
		if (foundPath)
		{
			return result;
		}
		if (m_FearDestination.m_ArrayIndex.Equals(CurrentTarget.ArrayIndex))
		{
			return new List<Point> { m_FearDestination.m_ArrayIndex };
		}
		return null;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityFear()
	{
	}

	public CAbilityFear(CAbilityFear state, ReferenceDictionary references)
		: base(state, references)
	{
		RemainingFears = state.RemainingFears;
		m_State = state.m_State;
		m_FearPath = references.Get(state.m_FearPath);
		if (m_FearPath == null && state.m_FearPath != null)
		{
			m_FearPath = new List<Point>();
			for (int i = 0; i < state.m_FearPath.Count; i++)
			{
				Point item = state.m_FearPath[i];
				m_FearPath.Add(item);
			}
			references.Add(state.m_FearPath, m_FearPath);
		}
		m_CurrentTargetDistanceMoved = state.m_CurrentTargetDistanceMoved;
		m_CancelFearOnCurrentTarget = state.m_CancelFearOnCurrentTarget;
		m_ActorDistanceMovedDictionary = references.Get(state.m_ActorDistanceMovedDictionary);
		if (m_ActorDistanceMovedDictionary == null && state.m_ActorDistanceMovedDictionary != null)
		{
			m_ActorDistanceMovedDictionary = new Dictionary<CActor, FearedActorStats>(state.m_ActorDistanceMovedDictionary.Comparer);
			foreach (KeyValuePair<CActor, FearedActorStats> item2 in state.m_ActorDistanceMovedDictionary)
			{
				CActor cActor = references.Get(item2.Key);
				if (cActor == null && item2.Key != null)
				{
					cActor = new CActor(item2.Key, references);
					references.Add(item2.Key, cActor);
				}
				FearedActorStats fearedActorStats = references.Get(item2.Value);
				if (fearedActorStats == null && item2.Value != null)
				{
					fearedActorStats = new FearedActorStats(item2.Value, references);
					references.Add(item2.Value, fearedActorStats);
				}
				m_ActorDistanceMovedDictionary.Add(cActor, fearedActorStats);
			}
			references.Add(state.m_ActorDistanceMovedDictionary, m_ActorDistanceMovedDictionary);
		}
		m_Jump = state.m_Jump;
		m_Fly = state.m_Fly;
		m_IgnoreDifficultTerrain = state.m_IgnoreDifficultTerrain;
		m_IgnoreHazardousTerrain = state.m_IgnoreHazardousTerrain;
		m_CurrentTargetJump = state.m_CurrentTargetJump;
		m_CurrentTargetFly = state.m_CurrentTargetFly;
		m_CurrentTargetIgnoreDifficult = state.m_CurrentTargetIgnoreDifficult;
		m_CurrentTargetIgnoreHazardous = state.m_CurrentTargetIgnoreHazardous;
	}
}
