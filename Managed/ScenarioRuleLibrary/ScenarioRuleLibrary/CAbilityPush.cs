using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityPush : CAbility
{
	public enum EAdditionalPushEffect
	{
		None,
		IntoObstacles,
		TrackBlocked
	}

	public enum EPushState
	{
		None,
		SelectPushTarget,
		ActorIsSelectingPushTile,
		ActorIsPushing,
		ActorHasPushed,
		ActorFinalizePush,
		CheckForNextTarget,
		PushDone
	}

	[Serializable]
	public class PushedActorStats
	{
		public string Name;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public PushedActorStats(string name, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}

		public PushedActorStats()
		{
		}

		public PushedActorStats(PushedActorStats state, ReferenceDictionary references)
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

	public static EAdditionalPushEffect[] AdditionalPushEffects = (EAdditionalPushEffect[])Enum.GetValues(typeof(EAdditionalPushEffect));

	public static EPushState[] PushStates = (EPushState[])Enum.GetValues(typeof(EPushState));

	private EPushState m_State;

	private CTile m_PlayerDestination;

	private List<CTile> m_PlayerWaypoints;

	private List<Point> m_AbilityPath;

	private CActor m_CurrentTarget;

	private bool m_TileSelected;

	private Dictionary<CActor, PushedActorStats> m_ActorDistanceMovedDictionary;

	private int m_ObstaclesDestroyed;

	private int m_PushesBlocked;

	private CAttackSummary m_AdditionalPushDamageSummary;

	private bool m_TargetSet;

	private bool m_DoThisOnce = true;

	private Point m_StartingPoint;

	public EAdditionalPushEffect AdditionalPushEffect { get; set; }

	public int AdditionalPushEffectDamage { get; set; }

	public int AdditionalPushEffectXP { get; set; }

	public CAttackSummary AdditionalPushDamageSummary => m_AdditionalPushDamageSummary;

	public int RemainingPushes { get; set; }

	public CActor CurrentTarget => m_CurrentTarget;

	public Point PushFromPoint { get; private set; }

	public CAbilityPush(EAdditionalPushEffect additionalPushEffect, int additionalPushEffectDamage, int additionalPushEffectXP)
	{
		AdditionalPushEffect = additionalPushEffect;
		AdditionalPushEffectDamage = additionalPushEffectDamage;
		AdditionalPushEffectXP = additionalPushEffectXP;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Ability Started");
		base.Start(targetingActor, filterActor, controllingActor);
		RemainingPushes = m_Strength;
		m_State = EPushState.SelectPushTarget;
		if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || base.TargetingActor.Type != CActor.EType.Player)
		{
			m_CanUndo = false;
		}
		SetTargets(targetingActor);
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (m_CurrentTarget == null || base.TargetingActor.Type != CActor.EType.Player)
		{
			if (base.Strength <= 0)
			{
				m_State = EPushState.ActorHasPushed;
			}
			else
			{
				m_State = EPushState.SelectPushTarget;
			}
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorsToTarget.Clear();
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>();
		m_ObstaclesDestroyed = 0;
		m_PushesBlocked = 0;
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public void SetTargets(CActor targetingActor)
	{
		PushFromPoint = targetingActor.ArrayIndex;
		if (base.AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(targetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		}
		if (base.TargetThisActorAutomatically != null)
		{
			if (!CAbility.ImmuneToAbility(base.TargetThisActorAutomatically, this))
			{
				base.ValidActorsInRange = new List<CActor> { base.TargetThisActorAutomatically };
				m_CurrentTarget = base.TargetThisActorAutomatically;
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					m_State = EPushState.ActorIsSelectingPushTile;
				}
			}
			else
			{
				m_CancelAbility = true;
			}
		}
		else if (base.UseSubAbilityTargeting)
		{
			if (base.ParentAbility != null && AdditionalPushEffect == EAdditionalPushEffect.None && base.ParentAbility.SubAbilities != null)
			{
				foreach (CAbility subAbility in base.ParentAbility.SubAbilities)
				{
					if (subAbility is CAbilityPush { AdditionalPushEffect: not EAdditionalPushEffect.None } cAbilityPush)
					{
						AdditionalPushEffect = cAbilityPush.AdditionalPushEffect;
						AdditionalPushEffectDamage = cAbilityPush.AdditionalPushEffectDamage;
						AdditionalPushEffectXP = cAbilityPush.AdditionalPushEffectXP;
					}
				}
			}
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
				if (AdditionalPushEffect != EAdditionalPushEffect.TrackBlocked && GetPushTiles(cActor2, cActor2.Type, cActor2.ArrayIndex, PushFromPoint, RemainingPushes, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles)).Count == 0)
				{
					flag = true;
				}
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
					m_State = EPushState.ActorIsSelectingPushTile;
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
		m_TargetSet = true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Performing state: " + m_State);
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
		case EPushState.SelectPushTarget:
			if (m_DoThisOnce)
			{
				SetTargets(base.TargetingActor);
				m_DoThisOnce = false;
			}
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				if (base.AreaEffect == null)
				{
					int num6 = base.ValidActorsInRange.Count - 1;
					while (num6 >= 0 && base.ValidActorsInRange.Count > m_NumberTargetsRemaining)
					{
						base.ValidActorsInRange.RemoveAt(num6);
						num6--;
					}
				}
				if (base.ValidActorsInRange.Count > 0)
				{
					m_CurrentTarget = GetNextTarget();
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPushState.ActorIsPushing;
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
		case EPushState.ActorIsSelectingPushTile:
			if (m_CurrentTarget == null || !ScenarioManager.Scenario.HasActor(m_CurrentTarget) || (GetPushTiles(m_CurrentTarget, m_CurrentTarget.Type, m_CurrentTarget.ArrayIndex, PushFromPoint, RemainingPushes, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles)).Count == 0 && AdditionalPushEffect != EAdditionalPushEffect.TrackBlocked))
			{
				CheckNextTarget();
			}
			else if (base.TargetingActor.Type == CActor.EType.Player && m_PlayerDestination != null)
			{
				bool flag2 = false;
				if (m_CurrentTarget != null)
				{
					int strength = 1;
					CAbilityMove.GetMoveBonuses(m_CurrentTarget, out var _, out var fly, out var _, out var _, ref strength);
					flag2 = fly;
				}
				bool flag3 = AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles);
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Player location: " + m_CurrentTarget.ArrayIndex.ToString());
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Player destination: " + m_PlayerDestination.m_ArrayIndex.ToString());
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Flying: " + flag2);
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - IntoBlocked: " + flag3);
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Generating path using passed in waypoints");
				string text = "[PUSH ABILITY] - In waypoints: ";
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					text = text + "\n" + playerWaypoint.m_ArrayIndex.ToString();
				}
				SimpleLog.AddToSimpleLog(text);
				CAbilityMove.GetNextPathingPositionUsingExistingWaypoints(m_CurrentTarget, m_PlayerDestination, jump: false, flag2 || flag3, m_PlayerWaypoints, out m_PlayerWaypoints, out m_AbilityPath, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, ignoreMoveCost: true, logFailure: true);
				string text2 = "[PUSH ABILITY] - Out waypoints: ";
				foreach (CTile playerWaypoint2 in m_PlayerWaypoints)
				{
					text2 = text2 + "\n" + playerWaypoint2.m_ArrayIndex.ToString();
				}
				SimpleLog.AddToSimpleLog(text2);
				string text3 = "[PUSH ABILITY] - Out ability path: ";
				foreach (Point item2 in m_AbilityPath)
				{
					text3 = text3 + "\n" + item2.ToString();
				}
				SimpleLog.AddToSimpleLog(text3);
				for (int num3 = 0; num3 < m_AbilityPath.Count; num3++)
				{
					Point point2 = m_AbilityPath[num3];
					CTile cTile2 = ScenarioManager.Tiles[point2.X, point2.Y];
					bool flag4 = false;
					for (int num4 = cTile2.m_Props.Count - 1; num4 >= 0; num4--)
					{
						if (cTile2.m_Props[num4].ObjectType != ScenarioManager.ObjectImportType.Door && cTile2.m_Props[num4].WillActivationKillActor(m_CurrentTarget))
						{
							flag4 = true;
							break;
						}
					}
					if (flag4)
					{
						SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Truncating push path as prop activation will kill target along the way");
						m_PlayerDestination = cTile2;
						for (int num5 = num3 + 1; num5 < m_AbilityPath.Count; num5++)
						{
							CTile item = ScenarioManager.Tiles[m_AbilityPath[num5].X, m_AbilityPath[num5].Y];
							m_PlayerWaypoints.Remove(item);
						}
						m_AbilityPath.RemoveRange(num3 + 1, m_AbilityPath.Count - (num3 + 1));
						break;
					}
				}
				if (m_ActorDistanceMovedDictionary.TryGetValue(m_CurrentTarget, out var value))
				{
					value.DistanceMoved = m_AbilityPath.Count;
				}
				else
				{
					value = new PushedActorStats(m_CurrentTarget.GetPrefabName() + ((m_CurrentTarget is CEnemyActor cEnemyActor) ? (" " + cEnemyActor.ID) : ""), m_AbilityPath.Count, new TileIndex(m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y), new TileIndex(m_PlayerDestination.m_ArrayIndex.X, m_PlayerDestination.m_ArrayIndex.Y));
				}
				m_ActorDistanceMovedDictionary[m_CurrentTarget] = value;
				SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Updating Current Push Target (" + m_CurrentTarget.GetPrefabName() + " " + m_CurrentTarget.ID + ") array index to: " + m_PlayerDestination.m_ArrayIndex.ToString());
				m_CurrentTarget.ArrayIndex = m_PlayerDestination.m_ArrayIndex;
				m_State = EPushState.ActorIsPushing;
				if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked) && m_AbilityPath.Count <= 0)
				{
					m_State = EPushState.ActorFinalizePush;
				}
				Perform();
			}
			else
			{
				m_TileSelected = false;
				CActorIsSelectingPushTile_MessageData message = new CActorIsSelectingPushTile_MessageData(base.TargetingActor)
				{
					m_PushAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		case EPushState.ActorIsPushing:
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
				m_CurrentTarget.Push(base.TargetingActor.ArrayIndex, RemainingPushes, out m_AbilityPath, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles));
				if (m_CurrentTarget.ArrayIndex != point)
				{
					CActorIsPushing_MessageData cActorIsPushing_MessageData = new CActorIsPushing_MessageData(base.TargetingActor);
					cActorIsPushing_MessageData.m_PushAbility = this;
					cActorIsPushing_MessageData.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
					cActorIsPushing_MessageData.AnimOverload = base.AnimOverload;
					cActorIsPushing_MessageData.m_SkipPushAnim = base.ActorsTargeted.Count > 0;
					ScenarioRuleClient.MessageHandler(cActorIsPushing_MessageData);
				}
				else
				{
					ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
				}
			}
			else
			{
				CActorIsPushing_MessageData cActorIsPushing_MessageData2 = new CActorIsPushing_MessageData(base.TargetingActor);
				cActorIsPushing_MessageData2.m_PushAbility = this;
				cActorIsPushing_MessageData2.m_Waypoints = m_PlayerWaypoints;
				cActorIsPushing_MessageData2.AnimOverload = base.AnimOverload;
				ScenarioRuleClient.MessageHandler(cActorIsPushing_MessageData2);
			}
			break;
		case EPushState.ActorHasPushed:
		{
			CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(base.TargetingActor);
			cActorHasPushed_MessageData.m_PushAbility = this;
			cActorHasPushed_MessageData.m_ActorBeingPushed = m_CurrentTarget;
			ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
			break;
		}
		case EPushState.ActorFinalizePush:
		{
			base.AbilityHasHappened = true;
			for (int i = 0; i < m_AbilityPath.Count; i++)
			{
				Point firstPoint = ((i == 0) ? m_StartingPoint : m_AbilityPath[i - 1]);
				GameState.LostAdjacency(m_CurrentTarget, firstPoint, m_AbilityPath[i]);
			}
			foreach (Point item3 in m_AbilityPath)
			{
				CTile propTile = ScenarioManager.Tiles[item3.X, item3.Y];
				for (int num = propTile.m_Props.Count - 1; num >= 0; num--)
				{
					if (propTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.Door)
					{
						bool flag = false;
						if (propTile.m_Props[num].WillActivationKillActor(m_CurrentTarget))
						{
							flag = true;
							m_CurrentTarget.ArrayIndex = item3;
						}
						if (propTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.PressurePlate && propTile.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.Portal)
						{
							propTile.m_Props[num].AutomaticActivate(m_CurrentTarget);
						}
						else if (propTile.m_Props[num] is CObjectPressurePlate cObjectPressurePlate && m_CurrentTarget.Class is CCharacterClass && (cObjectPressurePlate.PressurePlateType == CObjectPressurePlate.EPressurePlateType.ActivateOnce || (m_CurrentTarget.ArrayIndex.X == cObjectPressurePlate.ArrayIndex.X && m_CurrentTarget.ArrayIndex.Y == cObjectPressurePlate.ArrayIndex.Y && !flag)))
						{
							propTile.m_Props[num].AutomaticActivate(m_CurrentTarget);
						}
					}
				}
				if (AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles))
				{
					CObjectObstacle cObjectObstacle = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					if (cObjectObstacle == null)
					{
						cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
					}
					if (cObjectObstacle != null && !cObjectObstacle.PropActorHasBeenAssigned)
					{
						cObjectObstacle.DestroyProp(base.SpawnDelay);
						m_ObstaclesDestroyed++;
						if (AdditionalPushEffectDamage > 0)
						{
							int health = m_CurrentTarget.Health;
							bool actorWasAsleep = m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
							GameState.ActorBeenDamaged(m_CurrentTarget, AdditionalPushEffectDamage, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, this, base.AbilityType);
							if (GameState.ActorHealthCheck(base.TargetingActor, m_CurrentTarget, isTrap: false, isTerrain: false, actorWasAsleep))
							{
								CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(base.TargetingActor);
								cActorBeenDamaged_MessageData.m_ActorBeingDamaged = m_CurrentTarget;
								cActorBeenDamaged_MessageData.m_DamageAbility = null;
								cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
								cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
								ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
								m_ActorsToIgnore.Add(m_CurrentTarget);
							}
						}
					}
				}
				m_CurrentTarget.ArrayIndex = item3;
				CActiveBonus.RefreshAllAuraActiveBonuses();
				m_CurrentTarget.CalculateAttackStrengthForUI();
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, new List<CActor>(), newActorCarried: false, 1, finalMovement: false, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1);
				if (m_CurrentTarget.Health <= 0)
				{
					break;
				}
			}
			if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked))
			{
				if (m_AbilityPath.Count > 0)
				{
					CTile cTile = ScenarioManager.Tiles[m_AbilityPath.Last().X, m_AbilityPath.Last().Y];
					int num2 = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, PushFromPoint.X, PushFromPoint.Y) - 1;
					m_PushesBlocked = Math.Max(0, m_Strength - num2);
				}
				else
				{
					m_PushesBlocked = m_Strength;
				}
				if (m_PushesBlocked > 0 && AdditionalPushEffectDamage > 0)
				{
					int health2 = m_CurrentTarget.Health;
					bool actorWasAsleep2 = m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					GameState.ActorBeenDamaged(m_CurrentTarget, AdditionalPushEffectDamage * m_PushesBlocked, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, this, base.AbilityType);
					if (GameState.ActorHealthCheck(base.TargetingActor, m_CurrentTarget, isTrap: false, isTerrain: false, actorWasAsleep2))
					{
						CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData2 = new CActorBeenDamaged_MessageData(base.TargetingActor);
						cActorBeenDamaged_MessageData2.m_ActorBeingDamaged = m_CurrentTarget;
						cActorBeenDamaged_MessageData2.m_DamageAbility = null;
						cActorBeenDamaged_MessageData2.m_ActorOriginalHealth = health2;
						cActorBeenDamaged_MessageData2.m_ActorWasAsleep = actorWasAsleep2;
						ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData2);
						m_ActorsToIgnore.Add(m_CurrentTarget);
					}
				}
			}
			if (base.ParentAbility != null && base.ParentAbility is CAbilityAttack cAbilityAttack)
			{
				CActor cActor = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
				if (!cActor.IsDead && !m_CurrentTarget.IsDead)
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
		case EPushState.CheckForNextTarget:
			CheckNextTarget();
			return true;
		}
		return false;
		IL_0096:
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

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Tile selected");
		bool flag = false;
		if (m_State == EPushState.SelectPushTarget)
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
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(base.TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
				if (base.TargetingActor.Type != CActor.EType.Player)
				{
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPushState.ActorIsPushing;
					flag = true;
				}
				else if (base.TargetingActor.Type == CActor.EType.Player && base.UseSubAbilityTargeting)
				{
					m_State = EPushState.ActorIsSelectingPushTile;
					flag = true;
				}
			}
			else if (base.UseSubAbilityTargeting)
			{
				if (base.ValidActorsInRange.Count > 0)
				{
					base.ValidActorsInRange.RemoveAt(0);
				}
				if (base.ValidActorsInRange.Count > 0)
				{
					flag = true;
				}
				else
				{
					PhaseManager.NextStep();
				}
			}
		}
		else if (m_State == EPushState.ActorIsSelectingPushTile)
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
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Tile deselected");
		bool flag = false;
		if (m_State == EPushState.SelectPushTarget && m_TilesSelected.Contains(selectedTile))
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
		if (m_State == EPushState.SelectPushTarget)
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
		if (m_State != EPushState.SelectPushTarget)
		{
			return m_State == EPushState.ActorIsSelectingPushTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EPushState.SelectPushTarget)
			{
				return m_State == EPushState.ActorIsSelectingPushTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State != EPushState.SelectPushTarget)
		{
			return true;
		}
		return m_ActorsToTarget.Count > 0;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == EPushState.ActorIsSelectingPushTile;
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
		if (m_State == EPushState.ActorIsPushing && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
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
		if (!dontMoveState)
		{
			SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		else
		{
			SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Ability complete check not moving state");
		}
		return m_State == EPushState.PushDone;
	}

	public override void AbilityPassStep()
	{
		if (m_State == EPushState.ActorIsSelectingPushTile && base.ParentAbility != null && base.ParentAbility is CAbilityAttack { MultiPassAttack: not false } cAbilityAttack)
		{
			CActor cActor = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
			if (ScenarioManager.Scenario.HasActor(cActor) && ScenarioManager.Scenario.HasActor(m_CurrentTarget))
			{
				cActor.ApplyRetaliateToAttack(m_CurrentTarget, cAbilityAttack);
			}
		}
	}

	public override string GetDescription()
	{
		return "Pull(" + base.Strength + ")";
	}

	public static List<CTile> GetPushTiles(CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false)
	{
		List<CTile> tiles = new List<CTile>();
		bool foundPath;
		List<Point> list = CActor.FindCharacterPath(pushingActor, pushTarget, pushSource, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (!foundPath || list.Count == 0 || remainingPushes == 0)
		{
			return tiles;
		}
		GetPushTilesRecursive(pushingActor, ref tiles, ScenarioManager.Tiles[pushTarget.X, pushTarget.Y], list.Count, targetType, pushSource, remainingPushes, 0, intoBlocked);
		return tiles;
	}

	private static void GetPushTilesRecursive(CActor pushingActor, ref List<CTile> tiles, CTile currentTile, int distance, CActor.EType targetType, Point pushSource, int remainingPushes, int count = 0, bool intoBlocked = false)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			bool foundPath;
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(currentTile.m_ArrayIndex, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
			if (!foundPath || list2.Count != 1)
			{
				continue;
			}
			bool foundPath2;
			List<Point> list3 = CActor.FindCharacterPath(pushingActor, pushSource, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2);
			bool num = IsClosedDoor(allAdjacentTile);
			bool flag = false;
			if (pushingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pushingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag = fly;
			}
			if (!(!num && foundPath2) || list3.Count <= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || flag || intoBlocked) || !(allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump || intoBlocked))
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
					if (!CActor.AreActorsAllied(item.Type, targetType))
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
		if (count >= remainingPushes)
		{
			return;
		}
		foreach (CTile item2 in list)
		{
			GetPushTilesRecursive(pushingActor, ref tiles, item2, distance + 1, targetType, pushSource, remainingPushes, count, intoBlocked);
		}
	}

	public static List<CTile> GetPushTilesIncludingWallTiles(CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false)
	{
		List<CTile> tiles = new List<CTile>();
		bool foundPath;
		List<Point> list = CActor.FindCharacterPath(pushingActor, pushTarget, pushSource, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (!foundPath || list.Count == 0 || remainingPushes == 0)
		{
			return tiles;
		}
		GetPushTilesIncludingWallsRecursive(pushingActor, ref tiles, ScenarioManager.Tiles[pushTarget.X, pushTarget.Y], list.Count, targetType, pushSource, remainingPushes, 0, intoBlocked);
		return tiles;
	}

	private static void GetPushTilesIncludingWallsRecursive(CActor pushingActor, ref List<CTile> tiles, CTile currentTile, int distance, CActor.EType targetType, Point pushSource, int remainingPushes, int count = 0, bool intoBlocked = false)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (ScenarioManager.GetTileDistance(currentTile.m_ArrayIndex.X, currentTile.m_ArrayIndex.Y, allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y) != 1)
			{
				continue;
			}
			int tileDistance = ScenarioManager.GetTileDistance(pushSource.X, pushSource.Y, allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y);
			bool flag = IsClosedDoor(allAdjacentTile);
			bool flag2 = false;
			CNode cNode = ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y];
			if (cNode != null && !cNode.IsBridge && (allAdjacentTile.m_HexMap != currentTile.m_HexMap || (allAdjacentTile.m_Hex2Map != null && allAdjacentTile.m_Hex2Map != currentTile.m_HexMap)))
			{
				flag2 = true;
			}
			bool flag3 = false;
			if (pushingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pushingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag3 = fly;
			}
			if (tileDistance <= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked || flag3 || intoBlocked) || !(allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump || intoBlocked))
			{
				continue;
			}
			if (flag || flag2)
			{
				tiles.Add(allAdjacentTile);
				continue;
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
			if (cActor == null)
			{
				list.Add(allAdjacentTile);
				tiles.Add(allAdjacentTile);
			}
			else if (CActor.AreActorsAllied(cActor.Type, targetType))
			{
				list.Add(allAdjacentTile);
			}
		}
		if (count >= remainingPushes)
		{
			return;
		}
		foreach (CTile item in list)
		{
			GetPushTilesRecursive(pushingActor, ref tiles, item, distance + 1, targetType, pushSource, remainingPushes, count, intoBlocked);
		}
	}

	public static bool IsPushTileAdjacentToBlockedPushTile(CTile checkTile, CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false)
	{
		if (checkTile.m_ArrayIndex.X == 1 || checkTile.m_ArrayIndex.Y == 1 || checkTile.m_ArrayIndex.X == ScenarioManager.Width || checkTile.m_ArrayIndex.Y == ScenarioManager.Height)
		{
			return true;
		}
		List<CTile> pushTilesIncludingWallTiles = GetPushTilesIncludingWallTiles(pushingActor, targetType, pushTarget, pushSource, remainingPushes, intoBlocked);
		CNode cNode = ScenarioManager.PathFinder.Nodes[checkTile.m_ArrayIndex.X, checkTile.m_ArrayIndex.Y];
		foreach (CTile item in pushTilesIncludingWallTiles)
		{
			if (ScenarioManager.IsTileAdjacent(checkTile.m_ArrayIndex.X, checkTile.m_ArrayIndex.Y, item.m_ArrayIndex.X, item.m_ArrayIndex.Y, ignoreWalls: true))
			{
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (IsClosedDoor(item) || ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Blocked || !ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable || (checkTile.m_HexMap != item.m_HexMap && !cNode.IsBridge && !cNode2.IsBridge))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CheckNextTarget()
	{
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Checking for next target");
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
			m_State = EPushState.SelectPushTarget;
			m_ValidActorsInRange.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			m_PlayerDestination = null;
			RemainingPushes = m_Strength;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == EAbilityType.Push)
		{
			PhaseManager.NextStep();
		}
	}

	public override void AbilityEnded()
	{
		SimpleLog.AddToSimpleLog("[PUSH ABILITY] - Ability Ended");
		if (AdditionalPushEffectXP > 0)
		{
			if (AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles))
			{
				base.TargetingActor.GainXP(AdditionalPushEffectXP * m_ObstaclesDestroyed);
			}
			else if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked))
			{
				base.TargetingActor.GainXP(AdditionalPushEffectXP * m_PushesBlocked);
			}
			else
			{
				base.TargetingActor.GainXP(AdditionalPushEffectXP);
			}
		}
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void Restart()
	{
		base.Restart();
		if (m_ActorsToTarget != null)
		{
			m_ActorsToTarget.Clear();
		}
		m_CurrentTarget = null;
		RemainingPushes = m_Strength;
		PushFromPoint = base.TargetingActor.ArrayIndex;
		m_State = EPushState.SelectPushTarget;
		m_CancelAbility = false;
		if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || base.TargetingActor.Type != CActor.EType.Player || m_NumberTargets == -1 || base.AreaEffect != null)
		{
			m_CanUndo = false;
		}
		SetTargets(base.TargetingActor);
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (base.Strength <= 0)
		{
			m_State = EPushState.ActorHasPushed;
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>();
		m_ObstaclesDestroyed = 0;
		m_PushesBlocked = 0;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<PushedActorStats> actorsDistanceMoved = m_ActorDistanceMovedDictionary.Values.ToList();
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityPush(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, actorsDistanceMoved, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	private void InitAdditionalPushDamageSummary()
	{
		if (!base.ValidActorsInRange.Contains(CurrentTarget))
		{
			base.ValidActorsInRange.Add(CurrentTarget);
		}
		m_AdditionalPushDamageSummary = new CAttackSummary(this);
		m_NumberTargetsRemaining = m_NumberTargets + m_AdditionalPushDamageSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_Range += m_AdditionalPushDamageSummary.ActiveBonusAddRangeBuff;
	}

	public void UpdateAdditionalPushDamageSummary(bool adjacentToBlockedTile, CTile blockedTile)
	{
		if (adjacentToBlockedTile)
		{
			if (m_AdditionalPushDamageSummary == null)
			{
				InitAdditionalPushDamageSummary();
			}
			if (m_AdditionalPushDamageSummary != null)
			{
				int num = ScenarioManager.GetTileDistance(blockedTile.m_ArrayIndex.X, blockedTile.m_ArrayIndex.Y, PushFromPoint.X, PushFromPoint.Y) - 1;
				int overrideStrength = Math.Max(0, m_Strength - num) * AdditionalPushEffectDamage;
				m_AdditionalPushDamageSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
				m_AdditionalPushDamageSummary.OverrideModifiedStrength(overrideStrength);
			}
		}
		else
		{
			m_AdditionalPushDamageSummary = null;
		}
		CUpdateAdditionalPushDamagePreview_MessageData cUpdateAdditionalPushDamagePreview_MessageData = new CUpdateAdditionalPushDamagePreview_MessageData(base.TargetingActor);
		cUpdateAdditionalPushDamagePreview_MessageData.m_AdditionalPushDamageSummary = m_AdditionalPushDamageSummary;
		cUpdateAdditionalPushDamagePreview_MessageData.m_PushAbility = this;
		ScenarioRuleClient.MessageHandler(cUpdateAdditionalPushDamagePreview_MessageData);
	}

	public override bool IsPositive()
	{
		return false;
	}

	public bool IsAlreadyPushing()
	{
		EPushState state = m_State;
		return state == EPushState.ActorIsPushing || state == EPushState.ActorHasPushed;
	}

	public CAbilityPush()
	{
	}

	public CAbilityPush(CAbilityPush state, ReferenceDictionary references)
		: base(state, references)
	{
		AdditionalPushEffect = state.AdditionalPushEffect;
		AdditionalPushEffectDamage = state.AdditionalPushEffectDamage;
		AdditionalPushEffectXP = state.AdditionalPushEffectXP;
		RemainingPushes = state.RemainingPushes;
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
		if (m_ActorDistanceMovedDictionary == null && state.m_ActorDistanceMovedDictionary != null)
		{
			m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>(state.m_ActorDistanceMovedDictionary.Comparer);
			foreach (KeyValuePair<CActor, PushedActorStats> item2 in state.m_ActorDistanceMovedDictionary)
			{
				CActor cActor = references.Get(item2.Key);
				if (cActor == null && item2.Key != null)
				{
					cActor = new CActor(item2.Key, references);
					references.Add(item2.Key, cActor);
				}
				PushedActorStats pushedActorStats = references.Get(item2.Value);
				if (pushedActorStats == null && item2.Value != null)
				{
					pushedActorStats = new PushedActorStats(item2.Value, references);
					references.Add(item2.Value, pushedActorStats);
				}
				m_ActorDistanceMovedDictionary.Add(cActor, pushedActorStats);
			}
			references.Add(state.m_ActorDistanceMovedDictionary, m_ActorDistanceMovedDictionary);
		}
		m_ObstaclesDestroyed = state.m_ObstaclesDestroyed;
		m_PushesBlocked = state.m_PushesBlocked;
		m_TargetSet = state.m_TargetSet;
		m_DoThisOnce = state.m_DoThisOnce;
	}
}
