using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityMove : CAbility
{
	public enum EMoveState
	{
		None,
		PreMoveBuffTargeting,
		ApplyMoveBuff,
		CheckForAIToggleBonuses,
		ActorIsSelectingMoveTile,
		ActorIsMoving,
		ActorIsMovingB,
		ActorHasMoved,
		UnableToMove,
		MoveDone
	}

	public enum EMoveRestrictionType
	{
		None,
		StraightLineOnly,
		MustEndNextToObstacle
	}

	public static EMoveState[] MoveStates = (EMoveState[])Enum.GetValues(typeof(EMoveState));

	public static EMoveRestrictionType[] MoveRestrictionTypes = (EMoveRestrictionType[])Enum.GetValues(typeof(EMoveRestrictionType));

	public const string DEFAULT_MOVE_NAME = "DefaultMove";

	private static List<CActor> s_AllTargetActorsOnPath;

	private static List<Point> s_AllArrayIndexOnPath;

	private static List<Point> s_AllArrayIndexOnPathIncludingRepeats;

	private static List<Point> s_AllArrayIndexOnPathIncludingStart;

	private int m_InitialMoveCount;

	private int m_MoveCount;

	private EMoveState m_State;

	private CTile m_PlayerDestination;

	private List<CTile> m_PlayerWaypoints;

	private List<CTile> m_AllSelectedPlayerWaypoints;

	private bool m_Jump;

	private bool m_Fly;

	private bool m_IgnoreDifficultTerrain;

	private bool m_IgnoreHazardousTerrain;

	private bool m_IgnoreBlockedTileMoveCost;

	private bool m_CarryOtherActorsOnHex;

	private int m_AlreadyMoved;

	private int m_TilesMoved;

	private List<Point> m_PlayerArrayIndexPath = new List<Point>();

	private bool m_IsDefaultMove;

	private bool m_AIFinishedMove;

	private CActor m_CurrentTarget;

	private Point m_StartingPoint;

	private bool m_MergedAbilityMoved;

	private CTile m_LastTileChecked;

	private List<CMap> m_RoomRevealsPendingChecks;

	private List<CActor> m_ActorsToCarry = new List<CActor>();

	private bool m_hasMoved;

	private bool m_NewActorCarried;

	private bool m_MoveStartListenersInvoked;

	public int MoveCount
	{
		get
		{
			return m_InitialMoveCount;
		}
		set
		{
			m_InitialMoveCount = value;
		}
	}

	public int RemainingMoves => m_MoveCount;

	public bool Jump
	{
		get
		{
			return m_Jump;
		}
		set
		{
			m_Jump = value;
		}
	}

	public bool Fly
	{
		get
		{
			return m_Fly;
		}
		set
		{
			m_Fly = value;
		}
	}

	public bool IgnoreDifficultTerrain
	{
		get
		{
			return m_IgnoreDifficultTerrain;
		}
		set
		{
			m_IgnoreDifficultTerrain = value;
		}
	}

	public bool IgnoreHazardousTerrain
	{
		get
		{
			return m_IgnoreHazardousTerrain;
		}
		set
		{
			m_IgnoreHazardousTerrain = value;
		}
	}

	public bool IgnoreBlockedTileMoveCost
	{
		get
		{
			return m_IgnoreBlockedTileMoveCost;
		}
		set
		{
			m_IgnoreBlockedTileMoveCost = value;
		}
	}

	public bool CarryOtherActorsOnHex
	{
		get
		{
			return m_CarryOtherActorsOnHex;
		}
		set
		{
			m_CarryOtherActorsOnHex = value;
		}
	}

	public List<CActor> ActorsToCarry
	{
		get
		{
			return m_ActorsToCarry;
		}
		set
		{
			m_ActorsToCarry = value;
		}
	}

	public CAIFocusOverrideDetails AIFocusOverride { get; private set; }

	public bool IsDefaultMove => m_IsDefaultMove;

	public bool HasMoved => m_TilesMoved > 0;

	public CActor CurrentMovingActor => m_CurrentTarget;

	public int? RestrictRange => base.MiscAbilityData?.RestrictMoveRange;

	public Point? RestrictPoint => base.ControllingActor?.ArrayIndex;

	public Point StartingPoint => m_StartingPoint;

	public EMoveRestrictionType MoveRestrictionType { get; set; }

	public List<Point> AllArrayIndexOnPathIncludingRepeatsCopy { get; set; }

	public int DoorsOpened { get; set; }

	public static List<CActor> AllTargetActorsOnPath => s_AllTargetActorsOnPath;

	public static List<Point> AllArrayIndexOnPath => s_AllArrayIndexOnPath;

	public static List<Point> AllArrayIndexOnPathIncludingRepeats => s_AllArrayIndexOnPathIncludingRepeats;

	public static List<Point> AllArrayIndexOnPathIncludingStart => s_AllArrayIndexOnPathIncludingStart;

	public bool MoveStartListenersInvoked
	{
		get
		{
			return m_MoveStartListenersInvoked;
		}
		set
		{
			m_MoveStartListenersInvoked = value;
		}
	}

	public EMoveState State => m_State;

	public CAbilityMove(bool jump, bool fly, bool isDefaultMove, EMoveRestrictionType moveRestrictionType, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActorsOnHex = false, CAIFocusOverrideDetails aiFocusOverride = null)
	{
		MoveRestrictionType = moveRestrictionType;
		m_Jump = jump;
		m_Fly = fly;
		m_IgnoreDifficultTerrain = ignoreDifficultTerrain;
		m_IgnoreHazardousTerrain = ignoreHazardousTerrain;
		m_IgnoreBlockedTileMoveCost = ignoreBlockedTileMoveCost;
		m_CarryOtherActorsOnHex = carryOtherActorsOnHex;
		AIFocusOverride = aiFocusOverride;
		m_hasMoved = false;
		m_NewActorCarried = false;
		m_IsDefaultMove = isDefaultMove;
	}

	public static CAbility CreateDefaultMove(int moveAmount, bool isMonster, int range = 0, bool jump = false, bool fly = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActorsOnHex = false)
	{
		return CAbility.CreateAbility(EAbilityType.Move, moveAmount, useSpecialBaseStat: false, range, 0, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), string.Empty, null, null, attackSourcesOnly: false, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, ignoreBlockedTileMoveCost, carryOtherActorsOnHex, null, EMoveRestrictionType.None, null, null, "", "", "DefaultMove", new List<CEnhancement>(), null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, null, null, EAbilityTargeting.Range, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, new List<EAbilityType>(), new List<EAttackType>(), null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: true);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		s_AllTargetActorsOnPath = new List<CActor>();
		s_AllArrayIndexOnPath = new List<Point>();
		s_AllArrayIndexOnPathIncludingStart = new List<Point>();
		s_AllArrayIndexOnPathIncludingStart.Add(targetActor.ArrayIndex);
		s_AllArrayIndexOnPathIncludingRepeats = new List<Point>();
		m_RoomRevealsPendingChecks = new List<CMap>();
		targetActor.AIMoveFocusActors?.Clear();
		targetActor.AIMoveFocusWaypoints?.Clear();
		targetActor.AIMoveFocusPath = new List<Point>();
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetActor.ActorGuid);
		if (AIFocusOverride != null)
		{
			actorState?.SetAIFocusOverride(AIFocusOverride);
		}
		else if (actorState != null && actorState.AIFocusOverride?.IsTemporary == true)
		{
			actorState.ResetAIFocusOverride();
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_StartingPoint = targetActor.ArrayIndex;
		m_State = ((base.TargetingActor.Type != CActor.EType.Player) ? EMoveState.CheckForAIToggleBonuses : EMoveState.ActorIsSelectingMoveTile);
		m_CurrentTarget = base.TargetingActor;
		m_IgnoreDifficultTerrain |= base.TargetingActor.IgnoreDifficultTerrain;
		m_IgnoreHazardousTerrain |= base.TargetingActor.IgnoreHazardousTerrain;
		GetBonuses();
		m_InitialMoveCount = m_ModifiedStrength;
		m_MoveCount = m_InitialMoveCount;
		m_AlreadyMoved = 0;
		m_TilesMoved = 0;
		m_PlayerDestination = null;
		if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = EMoveState.PreMoveBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(base.TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(base.TargetingActor);
		}
		else if (MoveCount <= 0 && (base.UseSubAbilityTargeting || base.TargetingActor.Type != CActor.EType.Player))
		{
			m_State = EMoveState.ActorHasMoved;
		}
		m_PlayerWaypoints = null;
		if (!MoveStartListenersInvoked)
		{
			MoveStartListenersInvoked = true;
			if (targetActor.m_OnMoveStartListeners != null)
			{
				targetActor.m_OnMoveStartListeners?.Invoke(this);
			}
		}
		if (CAbility.ImmuneToAbility(base.TargetingActor, this))
		{
			m_CancelAbility = true;
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public override void Restart()
	{
		base.Restart();
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		m_ModifiedStrength = base.Strength;
		GetBonuses();
		m_InitialMoveCount = m_ModifiedStrength;
		m_MoveCount = m_InitialMoveCount - m_AlreadyMoved;
		if (MoveCount <= 0 && m_AlreadyMoved > 0)
		{
			m_State = EMoveState.ActorHasMoved;
		}
		else if (base.TargetingActor.m_OnMoveStartListeners != null)
		{
			base.TargetingActor.m_OnMoveStartListeners(this);
		}
	}

	public void GetBonuses(CActor actor = null)
	{
		if (actor == null)
		{
			actor = base.TargetingActor;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.Move);
		if (list != null)
		{
			foreach (CActiveBonus item in list)
			{
				m_ModifiedStrength += item.ReferenceStrength(this, actor);
				if (item is CMoveActiveBonus { BespokeBehaviour: not null } cMoveActiveBonus)
				{
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(this, actor).HasValue)
					{
						m_Jump = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(this, actor).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(this, actor).HasValue)
					{
						m_Fly = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(this, actor).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(this, actor).HasValue)
					{
						m_IgnoreDifficultTerrain = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(this, actor).Value;
					}
					if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(this, actor).HasValue)
					{
						m_IgnoreHazardousTerrain = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(this, actor).Value;
					}
				}
			}
			foreach (CActiveBonus item2 in list)
			{
				m_ModifiedStrength *= item2.ReferenceStrengthScalar(this, actor);
			}
		}
		if (actor.Type == CActor.EType.HeroSummon && IsDefaultMove)
		{
			CAbility cAbility = CAbilityAttack.CreateDefaultAttack(1, m_Range, 1, isMonster: false);
			((CAbilityAttack)cAbility).ProcessSongOverridesAndAbilities(actor, CSong.ESongActivationType.AbilityStart, temporaryOverrides: true);
			m_Range = ((CAbilityAttack)cAbility).Range;
		}
		else if (actor is CPlayerActor && actor.IsMonsterType)
		{
			foreach (CPhaseAction.CPhaseAbility remainingPhaseAbility in (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities)
			{
				if (remainingPhaseAbility.m_Ability.AbilityType == EAbilityType.Attack)
				{
					m_Range = remainingPhaseAbility.m_Ability.Range;
					break;
				}
			}
		}
		if (actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
		{
			m_Fly = true;
		}
		if (actor.Tokens.HasKey(CCondition.ENegativeCondition.StopFlying))
		{
			m_Fly = false;
		}
	}

	public override void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
		Restart();
	}

	public static void GetMoveBonuses(CActor actor, out bool jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, ref int strength)
	{
		CAbilityMove ability = (CAbilityMove)CreateDefaultMove(1, isMonster: false);
		jump = false;
		fly = actor.Flying;
		ignoreDifficultTerrain = false;
		ignoreHazardousTerrain = false;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.Move);
		if (list == null)
		{
			return;
		}
		foreach (CActiveBonus item in list)
		{
			strength += item.ReferenceStrength(ability, actor);
			if (item is CMoveActiveBonus { BespokeBehaviour: not null } cMoveActiveBonus)
			{
				if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(ability, actor).HasValue)
				{
					jump = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceJump(ability, actor).Value;
				}
				if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(ability, actor).HasValue)
				{
					fly = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceFly(ability, actor).Value;
				}
				if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(ability, actor).HasValue)
				{
					ignoreDifficultTerrain = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreDifficultTerrain(ability, actor).Value;
				}
				if (((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(ability, actor).HasValue)
				{
					ignoreHazardousTerrain = ((CMoveActiveBonus_BuffMove)cMoveActiveBonus.BespokeBehaviour).ReferenceIgnoreHazardousTerrain(ability, actor).Value;
				}
			}
		}
		foreach (CActiveBonus item2 in list)
		{
			strength *= item2.ReferenceStrengthScalar(ability, actor);
		}
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (m_CurrentTarget != null)
		{
			if (!m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize) || base.ActiveBonusData == null || base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
				{
					AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
					{
						goto IL_009d;
					}
				}
				if (!m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
				{
					goto IL_023d;
				}
			}
			goto IL_009d;
		}
		goto IL_023d;
		IL_023d:
		if (m_CancelAbility)
		{
			PhaseManager.NextStep(passing: true);
			return true;
		}
		Point arrayIndex;
		switch (m_State)
		{
		case EMoveState.PreMoveBuffTargeting:
		{
			if (base.TargetingActor.Type != CActor.EType.Player || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value))
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EMoveState.ApplyMoveBuff:
		{
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			CMoveBuff_MessageData message3 = new CMoveBuff_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_MoveAbility = this
			};
			ScenarioRuleClient.MessageHandler(message3);
			PhaseManager.NextStep();
			return true;
		}
		case EMoveState.CheckForAIToggleBonuses:
		{
			bool flag = false;
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.Move);
			if (list != null)
			{
				foreach (CActiveBonus item2 in list)
				{
					flag = item2.Ability.ActiveBonusData.IsToggleBonus && item2.CanToggleActiveBonus(base.TargetingActor);
				}
			}
			if (flag)
			{
				ShowToggleBonusesForAI();
				break;
			}
			m_State = EMoveState.ActorIsSelectingMoveTile;
			Perform();
			break;
		}
		case EMoveState.ActorIsSelectingMoveTile:
			if (base.TargetingActor.Type == CActor.EType.Player && m_PlayerDestination != null)
			{
				if (m_PlayerArrayIndexPath.Count == 0)
				{
					if (MoveRestrictionType == EMoveRestrictionType.StraightLineOnly)
					{
						for (int num3 = 1; num3 < 7; num3++)
						{
							List<CTile> tilesInLine = ScenarioManager.GetTilesInLine(m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y, RemainingMoves, (ScenarioManager.EAdjacentPosition)num3);
							if (!tilesInLine.Contains(m_PlayerWaypoints.Last()))
							{
								continue;
							}
							m_PlayerArrayIndexPath.Clear();
							for (int num4 = 0; num4 < tilesInLine.Count; num4++)
							{
								m_PlayerArrayIndexPath.Add(tilesInLine[num4].m_ArrayIndex);
								if (tilesInLine[num4] == m_PlayerWaypoints.Last())
								{
									m_PlayerDestination = tilesInLine[num4];
									break;
								}
							}
							break;
						}
					}
					else
					{
						bool jump = m_Jump;
						bool fly = m_Fly;
						if (CarryOtherActorsOnHex && m_ActorsToCarry.Count == 0)
						{
							CTile tile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
							m_ActorsToCarry = GameState.GetActorsOnTile(tile, base.FilterActor, CAbilityFilterContainer.CreateDefaultFilter(), base.ActorsToIgnore, isTargetedAbility: false, false);
							m_ActorsToCarry.Remove(m_CurrentTarget);
							if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex && m_AlreadyMoved < 1)
							{
								m_NewActorCarried = true;
							}
						}
						CActor.EType carryType = CActor.EType.Unknown;
						if (ActorsToCarry.Count > 0)
						{
							int strength = 0;
							carryType = ActorsToCarry[0].Type;
							GetMoveBonuses(ActorsToCarry[0], out jump, out fly, out var _, out var _, ref strength);
						}
						m_CurrentTarget.ArrayIndex = GetNextPathingPosition(m_CurrentTarget, m_PlayerDestination, jump, fly, RemainingMoves, out m_PlayerWaypoints, out m_PlayerArrayIndexPath, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, ignoreBlockedTileMoveCost: false, CarryOtherActorsOnHex, carryType);
						foreach (CActor item3 in m_ActorsToCarry)
						{
							item3.ArrayIndex = m_CurrentTarget.ArrayIndex;
						}
						ActorsToCarry = ActorsToCarry.Distinct().ToList();
					}
					foreach (Point item4 in m_PlayerArrayIndexPath)
					{
						if (!s_AllArrayIndexOnPath.Contains(item4))
						{
							s_AllArrayIndexOnPath.Add(item4);
							s_AllArrayIndexOnPathIncludingStart.Add(item4);
						}
						s_AllArrayIndexOnPathIncludingRepeats.Add(item4);
					}
				}
				if (m_PlayerArrayIndexPath.Count > 0)
				{
					m_CurrentTarget.ArrayIndex = m_PlayerArrayIndexPath[0];
					m_PlayerArrayIndexPath.RemoveAt(0);
					CObjectProp cObjectProp3 = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y].FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectProp3 != null && cObjectProp3 is CObjectDoor cObjectDoor)
					{
						if (!cObjectDoor.Activated)
						{
							DoorsOpened++;
						}
						cObjectDoor.SetDoorOpenedByMovingActor(base.TargetingActor);
						if (cObjectDoor.RoomsRevealedInLastOpening != null)
						{
							m_RoomRevealsPendingChecks.AddRange(cObjectDoor.RoomsRevealedInLastOpening);
						}
					}
				}
				m_State = EMoveState.ActorIsMoving;
				Perform();
			}
			else if (m_IsMergedAbility && m_AlreadyMoved > 0 && m_AllSelectedPlayerWaypoints.Count <= 0 && m_MergedAbilityMoved)
			{
				m_State = EMoveState.ActorHasMoved;
				m_MergedAbilityMoved = false;
				PhaseManager.StepComplete();
			}
			else
			{
				CActorIsSelectingMoveTile_MessageData cActorIsSelectingMoveTile_MessageData = new CActorIsSelectingMoveTile_MessageData(base.TargetingActor);
				cActorIsSelectingMoveTile_MessageData.m_MoveAbility = this;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingMoveTile_MessageData);
				DLLDebug.LogInfo("About to highlight items usable during the movement phase");
				base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				if (base.TargetingActor.Type != CActor.EType.Player)
				{
					m_State = EMoveState.ActorIsMoving;
					Perform();
				}
			}
			break;
		case EMoveState.ActorIsMoving:
			if (m_CurrentTarget.Equals(base.TargetingActor) && m_CurrentTarget.Type != CActor.EType.Player && m_CurrentTarget is CHeroSummonActor cHeroSummonActor && !m_CurrentTarget.IsDead && ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenSummonFocus))
			{
				m_CurrentTarget.Move(0, m_Jump, m_Fly, m_Range, allowMove: true, m_IgnoreDifficultTerrain, null, firstMove: false, moveTest: true);
				if (cHeroSummonActor.AIMoveFocusActors.Count == 0)
				{
					cHeroSummonActor.ReturnToSummoner = null;
					CReturnToSummoner_MessageData message = new CReturnToSummoner_MessageData(cHeroSummonActor, cHeroSummonActor.Summoner);
					ScenarioRuleClient.MessageHandler(message, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
					return false;
				}
			}
			m_State = EMoveState.ActorIsMovingB;
			Perform();
			break;
		case EMoveState.ActorIsMovingB:
		{
			if (base.TargetingActor.m_OnMovingListeners != null)
			{
				base.TargetingActor.m_OnMovingListeners?.Invoke(this);
			}
			if (m_CurrentTarget.Equals(base.TargetingActor) && m_CurrentTarget.Type != CActor.EType.Player)
			{
				arrayIndex = m_CurrentTarget.ArrayIndex;
				m_CurrentTarget.Move(m_MoveCount, m_Jump, m_Fly, m_Range, allowMove: true, m_IgnoreDifficultTerrain, null, firstMove: false, moveTest: false, CarryOtherActorsOnHex);
				CActor currentTarget = m_CurrentTarget;
				CHeroSummonActor summons = currentTarget as CHeroSummonActor;
				if (summons != null && ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenSummonFocus) && summons.AIMoveFocusActors.Count == 0 && summons.ReturnToSummoner == true)
				{
					ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == summons.ActorGuid);
					if (actorState != null)
					{
						if (actorState.AIFocusOverride != null)
						{
							CAIFocusOverrideDetails aIFocusOverride = actorState.AIFocusOverride;
							if (aIFocusOverride == null || aIFocusOverride.OverrideType != CAIFocusOverrideDetails.EOverrideType.None)
							{
								goto IL_0b30;
							}
						}
						CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
						cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
						cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
						cAIFocusOverrideDetails.TargetGUID = summons.Summoner.ActorGuid;
						cAIFocusOverrideDetails.IsTemporary = true;
						cAIFocusOverrideDetails.IsSummonsReturn = true;
						cAIFocusOverrideDetails.FocusBenign = true;
						actorState.SetAIFocusOverride(cAIFocusOverrideDetails);
						m_CurrentTarget.Move(m_MoveCount, m_Jump, m_Fly, m_Range, allowMove: true, m_IgnoreDifficultTerrain);
					}
				}
				goto IL_0b30;
			}
			if (m_CurrentTarget.Type == CActor.EType.Player)
			{
				SetCanSkip(canSkip: false);
				foreach (CActiveBonus item5 in CharacterClassManager.FindAllActiveBonusAuras(m_CurrentTarget))
				{
					CPauseAura_MessageData message4 = new CPauseAura_MessageData(m_CurrentTarget)
					{
						m_AuraAbilityID = item5.BaseCard.ID
					};
					ScenarioRuleClient.MessageHandler(message4);
				}
			}
			if (m_CurrentTarget.Type == CActor.EType.Player)
			{
				m_CurrentTarget.Inventory.LockInSelectedItemsAndResetUnselected();
				if (m_AlreadyMoved <= 0)
				{
					ScenarioRuleClient.FirstAbilityStarted();
				}
			}
			CActorHasMoved_MessageData cActorHasMoved_MessageData = new CActorHasMoved_MessageData(m_CurrentTarget);
			cActorHasMoved_MessageData.m_Ability = this;
			cActorHasMoved_MessageData.m_Waypoints = m_PlayerWaypoints;
			cActorHasMoved_MessageData.m_MovingActor = m_CurrentTarget;
			cActorHasMoved_MessageData.m_ActorsToCarry = m_ActorsToCarry.ToList();
			cActorHasMoved_MessageData.m_Jump = m_Jump;
			ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			break;
		}
		case EMoveState.ActorHasMoved:
		{
			base.AbilityHasHappened = true;
			m_hasMoved = true;
			if (m_CurrentTarget is CHeroSummonActor cHeroSummonActor2)
			{
				cHeroSummonActor2.ReturnToSummoner = null;
				ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == m_CurrentTarget.ActorGuid)?.ResetAIFocusOverride();
			}
			CTile propTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			CTile cTile = ScenarioManager.Tiles[m_StartingPoint.X, m_StartingPoint.Y];
			int thisMoveHexes = 0;
			if (propTile != m_LastTileChecked && (HasMoved || m_StartingPoint != m_CurrentTarget.ArrayIndex))
			{
				thisMoveHexes = 1;
				foreach (CActor item6 in m_ActorsToCarry)
				{
					item6.ArrayIndex = m_CurrentTarget.ArrayIndex;
					GameState.LostAdjacency(item6, (m_LastTileChecked == null) ? cTile : m_LastTileChecked, propTile);
				}
				GameState.LostAdjacency(m_CurrentTarget, (m_LastTileChecked == null) ? cTile : m_LastTileChecked, propTile);
				for (int num = propTile.m_Props.Count - 1; num >= 0; num--)
				{
					CObjectProp cObjectProp = propTile.m_Props[num];
					foreach (CActor item7 in m_ActorsToCarry)
					{
						CheckProps(item7, cObjectProp);
					}
					if (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && cObjectProp.ObjectType != ScenarioManager.ObjectImportType.Portal && ((!m_Jump && !m_Fly) || (cObjectProp != null && cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Door)))
					{
						if ((m_CurrentTarget.OriginalType == CActor.EType.Player || m_CurrentTarget is CHeroSummonActor { IsCompanionSummon: not false }) && !cObjectProp.Activated && cObjectProp.WillActivationDamageActor(m_CurrentTarget))
						{
							bool flag2 = true;
							if (m_IgnoreHazardousTerrain && (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || cObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns))
							{
								flag2 = false;
							}
							if (flag2)
							{
								CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(m_CurrentTarget);
								cPauseLoco_MessageData.m_Pause = true;
								ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
							}
							bool flag3 = false;
							flag3 = cObjectProp.WillActivationKillActor(m_CurrentTarget);
							if (cObjectProp is CObjectTrap cObjectTrap && cObjectTrap.Conditions.Count > 0 && (cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Stun) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Immobilize) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Sleep)))
							{
								flag3 = true;
							}
							if (flag3)
							{
								CStopLoco_MessageData cStopLoco_MessageData = new CStopLoco_MessageData(base.TargetingActor);
								cStopLoco_MessageData.m_Pause = true;
								ScenarioRuleClient.MessageHandler(cStopLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
								m_CancelAbility = true;
							}
						}
						if (!cObjectProp.Activated && cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Door)
						{
							DoorsOpened++;
						}
						cObjectProp.AutomaticActivate(m_CurrentTarget);
					}
				}
			}
			List<CObjectObstacle> list2 = new List<CObjectObstacle>();
			if (m_Jump || m_Fly)
			{
				CObjectObstacle cObjectObstacle = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle == null)
				{
					cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle != null)
				{
					list2.Add(cObjectObstacle);
				}
			}
			CUpdatePropTransparency_MessageData message2 = new CUpdatePropTransparency_MessageData
			{
				m_PropList = list2,
				m_ActorSpawningMessage = m_CurrentTarget
			};
			ScenarioRuleClient.MessageHandler(message2, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			if (!ScenarioManager.Scenario.HasActor(m_CurrentTarget))
			{
				PhaseManager.NextStep();
				return false;
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(m_CurrentTarget.ArrayIndex, m_CurrentTarget);
			if (cActor != null && !s_AllTargetActorsOnPath.Contains(cActor))
			{
				s_AllTargetActorsOnPath.Add(cActor);
			}
			CActiveBonus.RefreshAllAuraActiveBonuses();
			base.TargetingActor.CalculateAttackStrengthForUI();
			int num2 = CalculateMoveUsed(propTile.m_ArrayIndex, !m_Fly, !m_Jump, ignoreMoveCost: false, m_IgnoreDifficultTerrain, m_IgnoreBlockedTileMoveCost);
			m_MoveCount -= num2;
			if (m_CurrentTarget.Type == CActor.EType.Player || !m_AIFinishedMove)
			{
				if (m_ModifiedStrength > 0)
				{
					m_AlreadyMoved += ((!propTile.m_Props.Any((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || m_Jump || m_Fly || m_IgnoreDifficultTerrain) ? 1 : 2);
					m_TilesMoved++;
				}
				if (base.IsMergedAbility)
				{
					m_MergedAbilityMoved = true;
				}
			}
			if (HasMoved)
			{
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, m_ActorsToCarry.ToList(), m_NewActorCarried, 1, finalMovement: false, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes);
				List<CActor> actorsToCarry = m_ActorsToCarry;
				if (actorsToCarry != null && actorsToCarry.Count > 0)
				{
					m_ActorsToCarry[0].m_OnCarriedListeners?.Invoke(this, m_ActorsToCarry[0], 1, finalMovement: false, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes);
				}
			}
			m_NewActorCarried = false;
			m_LastTileChecked = propTile;
			bool canSkip = true;
			if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndNextToObstacle) && m_MoveCount > 1)
			{
				canSkip = false;
				foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
				{
					CTile propTile2 = allAdjacentTile;
					CObjectProp cObjectProp2 = allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
					if (cObjectProp2 == null)
					{
						cObjectProp2 = CObjectProp.FindPropWithPathingBlocker(allAdjacentTile.m_ArrayIndex, ref propTile2);
					}
					if (cObjectProp2 != null)
					{
						canSkip = true;
						break;
					}
				}
			}
			SetCanSkip(canSkip);
			if (m_MoveCount <= 0 || m_AIFinishedMove)
			{
				if (HasMoved)
				{
					m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, m_ActorsToCarry.ToList(), newActorCarried: false, 0, finalMovement: true, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes);
					List<CActor> actorsToCarry2 = m_ActorsToCarry;
					if (actorsToCarry2 != null && actorsToCarry2.Count > 0)
					{
						m_ActorsToCarry[0].m_OnCarriedListeners?.Invoke(this, m_ActorsToCarry[0], 0, finalMovement: true, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes);
					}
				}
				CAbility cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
				if (cAbility == this || (cAbility is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this))
				{
					if (m_IsMergedAbility)
					{
						PhaseManager.StepComplete();
					}
					else
					{
						PhaseManager.NextStep();
					}
				}
				else
				{
					m_State = EMoveState.MoveDone;
				}
				CheckPendingMapRevealsAndCurrentPhaseAbility();
				return true;
			}
			if (m_PlayerDestination != null && m_CurrentTarget.ArrayIndex == m_PlayerDestination.m_ArrayIndex)
			{
				m_AllSelectedPlayerWaypoints.Remove(m_PlayerDestination);
				m_PlayerDestination = null;
			}
			m_State = EMoveState.ActorIsSelectingMoveTile;
			if (CheckPendingMapRevealsAndCurrentPhaseAbility())
			{
				Perform();
			}
			break;
		}
		case EMoveState.UnableToMove:
			if (base.IsMergedAbility && m_MoveCount > 0)
			{
				m_State = EMoveState.ActorIsSelectingMoveTile;
			}
			break;
		case EMoveState.MoveDone:
			{
				PhaseManager.NextStep();
				return true;
			}
			IL_0b30:
			if (m_CurrentTarget.ArrayIndex != arrayIndex)
			{
				CTile propTile3 = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
				List<CObjectObstacle> list3 = new List<CObjectObstacle>();
				if (m_Jump || m_Fly)
				{
					CObjectObstacle cObjectObstacle2 = propTile3.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					if (cObjectObstacle2 == null)
					{
						cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(propTile3.m_ArrayIndex, ref propTile3);
					}
					if (cObjectObstacle2 != null)
					{
						list3.Add(cObjectObstacle2);
					}
				}
				CUpdatePropTransparency_MessageData message5 = new CUpdatePropTransparency_MessageData
				{
					m_PropList = list3,
					m_ActorSpawningMessage = m_CurrentTarget
				};
				ScenarioRuleClient.MessageHandler(message5, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
				CObjectProp cObjectProp4 = propTile3.FindProp(ScenarioManager.ObjectImportType.Door);
				if (cObjectProp4 != null && cObjectProp4 is CObjectDoor cObjectDoor2)
				{
					if (!cObjectDoor2.Activated)
					{
						DoorsOpened++;
					}
					cObjectDoor2.SetDoorOpenedByMovingActor(base.TargetingActor);
					if (cObjectDoor2.RoomsRevealedInLastOpening != null)
					{
						m_RoomRevealsPendingChecks.AddRange(cObjectDoor2.RoomsRevealedInLastOpening);
					}
				}
				CActorHasMoved_MessageData cActorHasMoved_MessageData2 = new CActorHasMoved_MessageData(m_CurrentTarget);
				cActorHasMoved_MessageData2.m_Ability = this;
				cActorHasMoved_MessageData2.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
				cActorHasMoved_MessageData2.m_MovingActor = m_CurrentTarget;
				cActorHasMoved_MessageData2.m_ActorsToCarry = m_ActorsToCarry.ToList();
				cActorHasMoved_MessageData2.m_Jump = m_Jump;
				ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData2, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			}
			else
			{
				m_AIFinishedMove = true;
				ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
			}
			break;
		}
		return false;
		IL_009d:
		if (m_CurrentTarget.Type == CActor.EType.Player)
		{
			if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				if (m_AlreadyMoved > 0)
				{
					CStopLoco_MessageData cStopLoco_MessageData2 = new CStopLoco_MessageData(base.TargetingActor);
					cStopLoco_MessageData2.m_Pause = true;
					ScenarioRuleClient.MessageHandler(cStopLoco_MessageData2, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
					PhaseManager.NextStep();
				}
				else
				{
					CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(m_CurrentTarget);
					ScenarioRuleClient.MessageHandler(message6);
				}
			}
			else if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				if (m_AlreadyMoved > 0)
				{
					CStopLoco_MessageData cStopLoco_MessageData3 = new CStopLoco_MessageData(base.TargetingActor);
					cStopLoco_MessageData3.m_Pause = true;
					ScenarioRuleClient.MessageHandler(cStopLoco_MessageData3, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
					PhaseManager.NextStep();
				}
				else
				{
					CPlayerIsSleeping_MessageData message7 = new CPlayerIsSleeping_MessageData(m_CurrentTarget);
					ScenarioRuleClient.MessageHandler(message7);
				}
			}
			else if (m_AlreadyMoved > 0)
			{
				CStopLoco_MessageData cStopLoco_MessageData4 = new CStopLoco_MessageData(base.TargetingActor);
				cStopLoco_MessageData4.m_Pause = true;
				ScenarioRuleClient.MessageHandler(cStopLoco_MessageData4, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
				PhaseManager.NextStep();
			}
			else
			{
				CPlayerIsImmobilized_MessageData message8 = new CPlayerIsImmobilized_MessageData(m_CurrentTarget);
				ScenarioRuleClient.MessageHandler(message8);
			}
		}
		else
		{
			CStopLoco_MessageData cStopLoco_MessageData5 = new CStopLoco_MessageData(base.TargetingActor);
			cStopLoco_MessageData5.m_Pause = true;
			ScenarioRuleClient.MessageHandler(cStopLoco_MessageData5, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			PhaseManager.NextStep();
		}
		m_State = EMoveState.UnableToMove;
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		DLLDebug.Log("[MOVE ABILITY] - Tile Selected");
		bool flag = false;
		if (!base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize) && !base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) && m_State == EMoveState.ActorIsSelectingMoveTile)
		{
			m_PlayerDestination = selectedTile;
			if (!m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
			}
			m_PlayerWaypoints = optionalTileList;
			m_AllSelectedPlayerWaypoints = optionalTileList;
			if (m_IsMergedAbility)
			{
				List<Point> list = new List<Point>();
				int num = RemainingMoves;
				Point arrayIndex = m_CurrentTarget.ArrayIndex;
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					List<Point> list2 = CActor.FindCharacterPath(m_CurrentTarget, arrayIndex, playerWaypoint.m_ArrayIndex, m_Jump || m_Fly, ignoreMoveCost: false, out var foundPath, avoidTraps: true, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, CarryOtherActorsOnHex);
					if (foundPath && list2.Count <= num)
					{
						list.AddRange(list2);
						num -= list2.Count;
					}
					else
					{
						list2 = CActor.FindCharacterPath(m_CurrentTarget, arrayIndex, playerWaypoint.m_ArrayIndex, m_Jump || m_Fly, ignoreMoveCost: false, out foundPath, avoidTraps: false, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, CarryOtherActorsOnHex);
						if (!foundPath)
						{
							break;
						}
						list.AddRange(list2);
						num -= list2.Count;
					}
					arrayIndex = playerWaypoint.m_ArrayIndex;
				}
				foreach (Point item in list)
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(item, m_CurrentTarget);
					if (cActor != null && !s_AllTargetActorsOnPath.Contains(cActor))
					{
						s_AllTargetActorsOnPath.Add(cActor);
					}
				}
			}
			m_PlayerArrayIndexPath.Clear();
			m_CanUndo = false;
			if (!m_IsMergedAbility)
			{
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

	public override bool CanClearTargets()
	{
		if (m_State != EMoveState.PreMoveBuffTargeting)
		{
			return m_State == EMoveState.ActorIsSelectingMoveTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveState.PreMoveBuffTargeting)
			{
				return m_State == EMoveState.ActorIsSelectingMoveTile;
			}
			return true;
		}
		return false;
	}

	public override bool RequiresWaypointSelection()
	{
		if (m_State == EMoveState.ActorIsSelectingMoveTile)
		{
			return base.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
		}
		return false;
	}

	public override bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		if (!base.ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && !abilityOverride.Strength.HasValue && !abilityOverride.IgnoreDifficultTerrain.HasValue && !abilityOverride.IgnoreHazardousTerrain.HasValue && !abilityOverride.CarryOtherActorsOnHex.HasValue && !abilityOverride.Jump.HasValue)
		{
			return abilityOverride.Fly.HasValue;
		}
		return true;
	}

	public void CheckProps(CActor actor, CObjectProp prop)
	{
		int strength = 0;
		GetMoveBonuses(actor, out var jump, out var fly, out var _, out var _, ref strength);
		if (prop.ObjectType == ScenarioManager.ObjectImportType.PressurePlate || prop.ObjectType == ScenarioManager.ObjectImportType.Portal || ((jump || fly) && (prop == null || prop.ObjectType != ScenarioManager.ObjectImportType.Door)))
		{
			return;
		}
		if ((actor.OriginalType == CActor.EType.Player || actor is CHeroSummonActor { IsCompanionSummon: not false }) && !prop.Activated && prop.WillActivationDamageActor(actor))
		{
			bool flag = true;
			if (m_IgnoreHazardousTerrain && (prop.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || prop.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns))
			{
				flag = false;
			}
			if (flag)
			{
				CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(actor);
				cPauseLoco_MessageData.m_Pause = true;
				ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
			}
			bool flag2 = false;
			flag2 = prop.WillActivationKillActor(actor);
			if (prop is CObjectTrap cObjectTrap && cObjectTrap.Conditions.Count > 0 && (cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Stun) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Immobilize) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Sleep)))
			{
				flag2 = true;
			}
			if (flag2)
			{
				CStopLoco_MessageData cStopLoco_MessageData = new CStopLoco_MessageData(base.TargetingActor);
				cStopLoco_MessageData.m_Pause = true;
				ScenarioRuleClient.MessageHandler(cStopLoco_MessageData, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
				m_CancelAbility = true;
			}
		}
		if (!prop.Activated && prop.ObjectType == ScenarioManager.ObjectImportType.Door)
		{
			DoorsOpened++;
		}
		prop.AutomaticActivate(actor);
	}

	public bool CheckPendingMapRevealsAndCurrentPhaseAbility()
	{
		if (m_RoomRevealsPendingChecks.Count > 0)
		{
			foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => (m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound || m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario) && m_RoomRevealsPendingChecks.Any((CMap x) => m.ShouldTriggerWhenOpeningRoom(x.MapGuid))))
			{
				item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			m_RoomRevealsPendingChecks.Clear();
		}
		CAbility cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
		if (cAbility != this)
		{
			if (cAbility is CAbilityMerged cAbilityMerged)
			{
				return cAbilityMerged.ActiveAbility == this;
			}
			return false;
		}
		return true;
	}

	public static Point GetNextPathingPosition(CActor actor, CTile destination, bool jump, bool fly, int remainingMoves, out List<CTile> waypoints, out List<Point> arrayIndexPath, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActors = false, CActor.EType carryType = CActor.EType.Unknown)
	{
		arrayIndexPath = CActor.FindCharacterPath(actor, actor.ArrayIndex, destination.m_ArrayIndex, jump || fly, ignoreMoveCost: false, out var foundPath, avoidTraps: true, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType);
		if (!foundPath || CalculateMoveCost(arrayIndexPath, !fly, !jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost) > remainingMoves)
		{
			arrayIndexPath.Clear();
			arrayIndexPath = CActor.FindCharacterPath(actor, actor.ArrayIndex, destination.m_ArrayIndex, jump || fly, ignoreMoveCost: false, out foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType);
		}
		waypoints = new List<CTile>();
		int num = 0;
		if (arrayIndexPath.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, arrayIndexPath[0].X, arrayIndexPath[0].Y);
			for (int i = 1; i < arrayIndexPath.Count; i++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(arrayIndexPath[i - 1].X, arrayIndexPath[i - 1].Y, arrayIndexPath[i].X, arrayIndexPath[i].Y);
				num += CalculateMoveUsed(arrayIndexPath[i - 1], !fly, !jump, ignoreMoveCost: false, ignoreDifficultTerrain);
				if (num > remainingMoves)
				{
					break;
				}
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[arrayIndexPath[i - 1].X, arrayIndexPath[i - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		if (num < remainingMoves)
		{
			waypoints.Add(destination);
		}
		if (arrayIndexPath.Count > 0)
		{
			return arrayIndexPath[0];
		}
		return destination.m_ArrayIndex;
	}

	public static Point GetNextPathingPositionUsingExistingWaypoints(CActor actor, CTile destination, bool jump, bool fly, List<CTile> inWaypoints, out List<CTile> waypoints, out List<Point> arrayIndexPath, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreMoveCost = false, bool logFailure = false)
	{
		arrayIndexPath = new List<Point>();
		waypoints = new List<CTile>();
		if (inWaypoints == null)
		{
			return destination.m_ArrayIndex;
		}
		arrayIndexPath.AddRange(CActor.FindCharacterPath(actor, actor.ArrayIndex, inWaypoints[0].m_ArrayIndex, jump || fly, ignoreMoveCost, out var foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors: false, CActor.EType.Unknown, logFailure));
		if (!foundPath)
		{
			SimpleLog.AddToSimpleLog("[GetNextPathingPosition] - Failed to find path to waypoint, from:" + actor.ArrayIndex.ToString() + " to:" + inWaypoints[0].m_ArrayIndex.ToString());
		}
		else
		{
			SimpleLog.AddToSimpleLog("[GetNextPathingPosition] - Found path to waypoint, from:" + actor.ArrayIndex.ToString() + " to:" + inWaypoints[0].m_ArrayIndex.ToString());
		}
		for (int i = 0; i < inWaypoints.Count - 1; i++)
		{
			arrayIndexPath.AddRange(CActor.FindCharacterPath(actor, inWaypoints[i].m_ArrayIndex, inWaypoints[i + 1].m_ArrayIndex, jump || fly, ignoreMoveCost, out foundPath, avoidTraps: false, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors: false, CActor.EType.Unknown, logFailure));
		}
		if (arrayIndexPath.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, arrayIndexPath[0].X, arrayIndexPath[0].Y);
			for (int j = 1; j < arrayIndexPath.Count; j++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(arrayIndexPath[j - 1].X, arrayIndexPath[j - 1].Y, arrayIndexPath[j].X, arrayIndexPath[j].Y);
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[arrayIndexPath[j - 1].X, arrayIndexPath[j - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		waypoints.Add(destination);
		if (arrayIndexPath.Count > 0)
		{
			return arrayIndexPath[0];
		}
		return destination.m_ArrayIndex;
	}

	public static int CalculateMoveCost(List<Point> path, bool nofly, bool nojump, bool ignoreMoveCost = false, bool ignoreDifficultTerrain = false, bool ignoreBlockedTileMoveCost = false, bool noMoreMovement = true)
	{
		int num = 0;
		foreach (Point item in path)
		{
			if (item == path.Last() && noMoreMovement)
			{
				nojump = true;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[item.X, item.Y];
			num = ((ignoreBlockedTileMoveCost && cNode.Blocked) ? num : ((!(!ignoreMoveCost && cNode.IsDifficultTerrain && nofly && nojump) || ignoreDifficultTerrain) ? (num + 1) : (num + 2)));
		}
		return num;
	}

	public static int CalculateMoveUsed(Point point, bool nofly, bool nojump, bool ignoreMoveCost = false, bool ignoreDifficultTerrain = false, bool ignoreBlockedTileMoveCost = false)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[point.X, point.Y];
		if (ignoreBlockedTileMoveCost && cNode.Blocked)
		{
			return 0;
		}
		if (!ignoreMoveCost && cNode.IsDifficultTerrain && nofly && nojump && !ignoreDifficultTerrain)
		{
			return 2;
		}
		return 1;
	}

	public static List<Point> FindPathAndWaypoints(Point origin, CTile destination, out List<CTile> waypoints, int moveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool excludeDestination, bool ignoreMoveCost, out bool foundPath, bool shouldPathThroughDoors)
	{
		CPathFinder pathFinder = ScenarioManager.PathFinder;
		Point startLocation = origin;
		Point arrayIndex = destination.m_ArrayIndex;
		bool ignoreBlocked = jump || fly;
		bool ignoreDifficultTerrain2 = ignoreDifficultTerrain;
		List<Point> list = pathFinder.FindPath(startLocation, arrayIndex, ignoreBlocked, ignoreMoveCost: false, out foundPath, shouldPathThroughDoors, ignoreDifficultTerrain2);
		if (excludeDestination && list.Count > 1)
		{
			destination = ScenarioManager.Tiles[list[list.Count - 2].X, list[list.Count - 2].Y];
		}
		waypoints = new List<CTile>();
		if (list.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(origin.X, origin.Y, list[0].X, list[0].Y);
			int num = CalculateMoveUsed(list[0], !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain);
			for (int i = 1; i < list.Count; i++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(list[i - 1].X, list[i - 1].Y, list[i].X, list[i].Y);
				num += CalculateMoveUsed(list[i], !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain);
				if (num > moveCount)
				{
					break;
				}
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[list[i - 1].X, list[i - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		waypoints.Add(destination);
		if (excludeDestination && list.Count > 0)
		{
			list.Remove(list[list.Count - 1]);
		}
		int num2 = CalculateMoveCost(list, !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain);
		if (moveCount < num2)
		{
			int num3 = CalculateMoveUsed(list[0], !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain);
			for (int j = 1; j < list.Count; j++)
			{
				num3 += CalculateMoveUsed(list[j], !fly, !jump, ignoreMoveCost, ignoreDifficultTerrain);
				if (num3 > moveCount)
				{
					waypoints[waypoints.Count - 1] = ScenarioManager.Tiles[list[j - 1].X, list[j - 1].Y];
					break;
				}
			}
		}
		return list;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Unable to find base ability card for ability " + base.Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			DLLDebug.Log("[MOVE ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		return m_State == EMoveState.MoveDone;
	}

	public override void Update()
	{
		if (m_State == EMoveState.ActorIsMoving && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
		{
			PhaseManager.NextStep();
		}
	}

	public static List<CTile> GetMustEndNextToObstacleTiles(CActor movingActor, int remainingMoves, Point startPoint, bool ignoreBlocked, bool ignoreDifficultTerrain)
	{
		List<CTile> tiles = new List<CTile>();
		List<CTile> list = new List<CTile>();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (!map.Revealed)
			{
				continue;
			}
			foreach (CObjectObstacle item in map.Props.OfType<CObjectObstacle>())
			{
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y]))
					{
						if (!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked && ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex) == null)
						{
							list.Add(allAdjacentTile);
						}
					}
				}
			}
		}
		list = list.Distinct().ToList();
		List<CTile> list2 = new List<CTile>();
		foreach (CTile item2 in list)
		{
			bool foundPath;
			List<Point> list3 = CActor.FindCharacterPath(movingActor, startPoint, item2.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: true, out foundPath, avoidTraps: false, ignoreDifficultTerrain);
			if (foundPath && list3.Count != 0 && CalculateMoveCost(list3, !ignoreBlocked, !ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain) <= remainingMoves)
			{
				tiles.Clear();
				GetMustEndNextToObstacleTilesRecursive(movingActor, ref tiles, ScenarioManager.Tiles[startPoint.X, startPoint.Y], item2, remainingMoves, ignoreBlocked, ignoreDifficultTerrain, startPoint);
				list2.AddRange(tiles);
			}
		}
		return list2.Distinct().ToList();
	}

	private static void GetMustEndNextToObstacleTilesRecursive(CActor movingActor, ref List<CTile> tiles, CTile currentTile, CTile destinationTile, int remainingMoves, bool ignoreBlocked, bool ignoreDifficultTerrain, Point startPoint, int count = 0)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (tiles.Contains(allAdjacentTile) || (allAdjacentTile.m_HexMap != null && allAdjacentTile.m_Hex2Map != null && (!allAdjacentTile.m_HexMap.Revealed || !allAdjacentTile.m_Hex2Map.Revealed)))
			{
				continue;
			}
			if (!ignoreBlocked)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
				if ((cActor != null && cActor != movingActor && !CActor.AreActorsAllied(cActor.Type, movingActor.Type)) || allAdjacentTile.FindProps(ScenarioManager.ObjectImportType.Obstacle).Count > 0)
				{
					continue;
				}
			}
			if (CalculateMoveCost(ScenarioManager.PathFinder.FindPath(allAdjacentTile.m_ArrayIndex, destinationTile.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: false, out var _, ignoreBridges: false, ignoreDifficultTerrain), !ignoreBlocked, !ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain) <= remainingMoves - 1)
			{
				list.Add(allAdjacentTile);
				tiles.Add(allAdjacentTile);
			}
		}
		if (remainingMoves <= 0)
		{
			return;
		}
		foreach (CTile item in list)
		{
			GetMustEndNextToObstacleTilesRecursive(movingActor, ref tiles, item, destinationTile, remainingMoves - count, ignoreBlocked, ignoreDifficultTerrain, startPoint, count);
		}
	}

	public override void AbilityEnded()
	{
		DLLDebug.Log("[MOVE ABILITY] - Ability Ended");
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		if (HasMoved || m_StartingPoint != m_CurrentTarget.ArrayIndex)
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
					if (m_Jump && !m_Fly)
					{
						cObjectProp.AutomaticActivate(m_CurrentTarget);
					}
					break;
				case ScenarioManager.ObjectImportType.PressurePlate:
					break;
				}
			}
		}
		if (base.AllTargetsOnMovePath || (base.AllTargetsOnMovePathSameStartAndEnd && m_StartingPoint == m_CurrentTarget.ArrayIndex))
		{
			for (int num2 = s_AllTargetActorsOnPath.Count - 1; num2 >= 0; num2--)
			{
				CActor actorOnPath = s_AllTargetActorsOnPath[num2];
				if (!ProcessTargetOnMovePath(actorOnPath))
				{
					s_AllTargetActorsOnPath.RemoveAt(num2);
				}
			}
		}
		if (m_CurrentTarget != null && ScenarioManager.Scenario.HasActor(m_CurrentTarget) && m_CurrentTarget.Type == CActor.EType.Player && HasMoved)
		{
			foreach (CActiveBonus item in CharacterClassManager.FindAllActiveBonusAuras(m_CurrentTarget))
			{
				CUnpauseAura_MessageData message = new CUnpauseAura_MessageData(m_CurrentTarget)
				{
					m_AuraBaseCardID = item.BaseCard.ID,
					m_AuraBaseCardName = item.BaseCard.Name
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		List<TileIndex> list = new List<TileIndex>();
		foreach (Point allArrayIndexOnPathIncludingRepeat in AllArrayIndexOnPathIncludingRepeats)
		{
			list.Add(new TileIndex(allArrayIndexOnPathIncludingRepeat));
		}
		if (m_CurrentTarget.Type == CActor.EType.Player)
		{
			GameState.UpdateHexesMovedThisTurn(m_CurrentTarget, list);
		}
		else
		{
			CPhaseAction.UpdateHexesMovedThisAction(m_AlreadyMoved);
		}
		AllArrayIndexOnPathIncludingRepeatsCopy = new List<Point>();
		AllArrayIndexOnPathIncludingRepeatsCopy.AddRange(AllArrayIndexOnPathIncludingRepeats);
	}

	public void UpdateCarryActors(List<CTile> optionalTileList, bool removeWaypoint)
	{
		if ((!removeWaypoint && (removeWaypoint || ActorsToCarry.Count != 0)) || !TargetsOnPath(this, optionalTileList, out var targetActors))
		{
			return;
		}
		ActorsToCarry.Clear();
		m_NewActorCarried = false;
		foreach (CActor item in targetActors)
		{
			if (!CAbility.ImmuneToAbility(item, this))
			{
				ActorsToCarry.Add(item);
			}
		}
		ActorsToCarry.Remove(m_CurrentTarget);
		ActorsToCarry = ActorsToCarry.Distinct().ToList();
		if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex)
		{
			m_NewActorCarried = true;
		}
	}

	public void CarryOtherActorsOnHexUpdated()
	{
		if (CarryOtherActorsOnHex)
		{
			CTile tile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			m_ActorsToCarry = GameState.GetActorsOnTile(tile, base.FilterActor, CAbilityFilterContainer.CreateDefaultFilter(), base.ActorsToIgnore, isTargetedAbility: false, false);
			m_ActorsToCarry.Remove(m_CurrentTarget);
			if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex && m_AlreadyMoved < 1)
			{
				m_NewActorCarried = true;
			}
		}
		else
		{
			m_ActorsToCarry.Clear();
		}
		ActorsToCarry = ActorsToCarry.Distinct().ToList();
		CAbilityRestarted_MessageData message = new CAbilityRestarted_MessageData(base.TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static bool TargetsOnPath(CAbility attackAbility, List<CTile> optionalTileList, out List<CActor> targetActors)
	{
		targetActors = new List<CActor>();
		if (optionalTileList != null)
		{
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				bool foundPath = false;
				List<Point> list = new List<Point>();
				if (i == 0)
				{
					list = ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor.ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
					list.Insert(0, attackAbility.TargetingActor.ArrayIndex);
				}
				else
				{
					list = ScenarioManager.PathFinder.FindPath(optionalTileList[i - 1].m_ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
					list.Insert(0, optionalTileList[i - 1].m_ArrayIndex);
				}
				if (foundPath)
				{
					foreach (Point item in list)
					{
						List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[item.X, item.Y], attackAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), attackAbility.ActorsToIgnore, isTargetedAbility: false, false);
						targetActors.AddRange(actorsOnTile);
					}
					continue;
				}
				return false;
			}
			if (optionalTileList.Count == 0)
			{
				List<CActor> actorsOnTile2 = GameState.GetActorsOnTile(ScenarioManager.Tiles[attackAbility.TargetingActor.ArrayIndex.X, attackAbility.TargetingActor.ArrayIndex.Y], attackAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), attackAbility.ActorsToIgnore, isTargetedAbility: false, false);
				targetActors.AddRange(actorsOnTile2);
			}
			targetActors = targetActors.Distinct().ToList();
			return true;
		}
		return false;
	}

	private bool ProcessTargetOnMovePath(CActor actorOnPath)
	{
		bool result = true;
		if (m_PositiveConditions.Count > 0)
		{
			if (actorOnPath != null)
			{
				actorOnPath = (CActor.AreActorsAllied(m_CurrentTarget.Type, actorOnPath.Type) ? actorOnPath : null);
			}
			if (actorOnPath != null)
			{
				ProcessPositiveStatusEffects(actorOnPath);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		if (m_NegativeConditions.Count > 0)
		{
			if (actorOnPath != null)
			{
				actorOnPath = ((!CActor.AreActorsAllied(m_CurrentTarget.Type, actorOnPath.Type)) ? actorOnPath : null);
			}
			if (actorOnPath != null)
			{
				ProcessNegativeStatusEffects(actorOnPath);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		if (subTypeAbility == ESESubTypeAbility.AbilityPerform)
		{
			DLLDebug.Log("[MOVE ABILITY] - Performing state: " + m_State);
		}
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityMove(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_AlreadyMoved, m_TilesMoved, new TileIndex(m_StartingPoint), new TileIndex(base.TargetingActor.ArrayIndex), base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, m_Jump, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null, "", IsDefaultMove, base.AbilityHasHappened || m_hasMoved));
	}

	public override string GetDescription()
	{
		return "Move(" + m_InitialMoveCount + ")";
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

	public bool HasPassedState(EMoveState moveState)
	{
		return m_State > moveState;
	}

	public override void Reset()
	{
		base.Reset();
		s_AllTargetActorsOnPath?.Clear();
		s_AllArrayIndexOnPath?.Clear();
		s_AllArrayIndexOnPathIncludingRepeats?.Clear();
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State == EMoveState.ActorIsSelectingMoveTile)
		{
			return m_PlayerDestination != null;
		}
		return true;
	}

	public CAbilityMove()
	{
	}

	public CAbilityMove(CAbilityMove state, ReferenceDictionary references)
		: base(state, references)
	{
		AIFocusOverride = references.Get(state.AIFocusOverride);
		if (AIFocusOverride == null && state.AIFocusOverride != null)
		{
			AIFocusOverride = new CAIFocusOverrideDetails(state.AIFocusOverride, references);
			references.Add(state.AIFocusOverride, AIFocusOverride);
		}
		MoveRestrictionType = state.MoveRestrictionType;
		AllArrayIndexOnPathIncludingRepeatsCopy = references.Get(state.AllArrayIndexOnPathIncludingRepeatsCopy);
		if (AllArrayIndexOnPathIncludingRepeatsCopy == null && state.AllArrayIndexOnPathIncludingRepeatsCopy != null)
		{
			AllArrayIndexOnPathIncludingRepeatsCopy = new List<Point>();
			for (int i = 0; i < state.AllArrayIndexOnPathIncludingRepeatsCopy.Count; i++)
			{
				Point item = state.AllArrayIndexOnPathIncludingRepeatsCopy[i];
				AllArrayIndexOnPathIncludingRepeatsCopy.Add(item);
			}
			references.Add(state.AllArrayIndexOnPathIncludingRepeatsCopy, AllArrayIndexOnPathIncludingRepeatsCopy);
		}
		DoorsOpened = state.DoorsOpened;
		m_InitialMoveCount = state.m_InitialMoveCount;
		m_MoveCount = state.m_MoveCount;
		m_State = state.m_State;
		m_PlayerWaypoints = references.Get(state.m_PlayerWaypoints);
		if (m_PlayerWaypoints == null && state.m_PlayerWaypoints != null)
		{
			m_PlayerWaypoints = new List<CTile>();
			for (int j = 0; j < state.m_PlayerWaypoints.Count; j++)
			{
				CTile cTile = state.m_PlayerWaypoints[j];
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
		m_AllSelectedPlayerWaypoints = references.Get(state.m_AllSelectedPlayerWaypoints);
		if (m_AllSelectedPlayerWaypoints == null && state.m_AllSelectedPlayerWaypoints != null)
		{
			m_AllSelectedPlayerWaypoints = new List<CTile>();
			for (int k = 0; k < state.m_AllSelectedPlayerWaypoints.Count; k++)
			{
				CTile cTile3 = state.m_AllSelectedPlayerWaypoints[k];
				CTile cTile4 = references.Get(cTile3);
				if (cTile4 == null && cTile3 != null)
				{
					cTile4 = new CTile(cTile3, references);
					references.Add(cTile3, cTile4);
				}
				m_AllSelectedPlayerWaypoints.Add(cTile4);
			}
			references.Add(state.m_AllSelectedPlayerWaypoints, m_AllSelectedPlayerWaypoints);
		}
		m_Jump = state.m_Jump;
		m_Fly = state.m_Fly;
		m_IgnoreDifficultTerrain = state.m_IgnoreDifficultTerrain;
		m_IgnoreHazardousTerrain = state.m_IgnoreHazardousTerrain;
		m_IgnoreBlockedTileMoveCost = state.m_IgnoreBlockedTileMoveCost;
		m_CarryOtherActorsOnHex = state.m_CarryOtherActorsOnHex;
		m_AlreadyMoved = state.m_AlreadyMoved;
		m_TilesMoved = state.m_TilesMoved;
		m_PlayerArrayIndexPath = references.Get(state.m_PlayerArrayIndexPath);
		if (m_PlayerArrayIndexPath == null && state.m_PlayerArrayIndexPath != null)
		{
			m_PlayerArrayIndexPath = new List<Point>();
			for (int l = 0; l < state.m_PlayerArrayIndexPath.Count; l++)
			{
				Point item2 = state.m_PlayerArrayIndexPath[l];
				m_PlayerArrayIndexPath.Add(item2);
			}
			references.Add(state.m_PlayerArrayIndexPath, m_PlayerArrayIndexPath);
		}
		m_IsDefaultMove = state.m_IsDefaultMove;
		m_AIFinishedMove = state.m_AIFinishedMove;
		m_MergedAbilityMoved = state.m_MergedAbilityMoved;
		m_RoomRevealsPendingChecks = references.Get(state.m_RoomRevealsPendingChecks);
		if (m_RoomRevealsPendingChecks == null && state.m_RoomRevealsPendingChecks != null)
		{
			m_RoomRevealsPendingChecks = new List<CMap>();
			for (int m = 0; m < state.m_RoomRevealsPendingChecks.Count; m++)
			{
				CMap cMap = state.m_RoomRevealsPendingChecks[m];
				CMap cMap2 = references.Get(cMap);
				if (cMap2 == null && cMap != null)
				{
					cMap2 = new CMap(cMap, references);
					references.Add(cMap, cMap2);
				}
				m_RoomRevealsPendingChecks.Add(cMap2);
			}
			references.Add(state.m_RoomRevealsPendingChecks, m_RoomRevealsPendingChecks);
		}
		m_ActorsToCarry = references.Get(state.m_ActorsToCarry);
		if (m_ActorsToCarry == null && state.m_ActorsToCarry != null)
		{
			m_ActorsToCarry = new List<CActor>();
			for (int n = 0; n < state.m_ActorsToCarry.Count; n++)
			{
				CActor cActor = state.m_ActorsToCarry[n];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_ActorsToCarry.Add(cActor2);
			}
			references.Add(state.m_ActorsToCarry, m_ActorsToCarry);
		}
		m_hasMoved = state.m_hasMoved;
		m_NewActorCarried = state.m_NewActorCarried;
		m_MoveStartListenersInvoked = state.m_MoveStartListenersInvoked;
	}
}
