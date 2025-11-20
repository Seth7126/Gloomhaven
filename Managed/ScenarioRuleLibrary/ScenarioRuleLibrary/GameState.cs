using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using MathParserTK;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using UnityEngine;

namespace ScenarioRuleLibrary;

public class GameState
{
	public enum ActionSelectionSequenceType
	{
		FirstAction,
		SecondAction,
		Complete
	}

	[Flags]
	public enum EActionSelectionFlag
	{
		None = 0,
		TopActionPlayed = 1,
		BottomActionPlayed = 2
	}

	public enum EActionInitiator
	{
		None,
		AbilityCard,
		ItemCard,
		ActiveBonus,
		ScenarioModifier,
		CompanionSummon,
		ActionsTriggeredOutsideActionPhase,
		OverrideTurnAction
	}

	public enum EAvoidDamageOption
	{
		None,
		Lose1HandCard,
		Lose2DiscardCards
	}

	public class ActorInitiativeComparer : IComparer<CActor>
	{
		public bool CheckForAdjustment { get; set; }

		public ActorInitiativeComparer(bool checkforadjustment = false)
		{
			CheckForAdjustment = checkforadjustment;
		}

		public int Compare(CActor x, CActor y)
		{
			if (x == null || y == null)
			{
				return 0;
			}
			int num = x.Initiative();
			int num2 = y.Initiative();
			if (CheckForAdjustment)
			{
				List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(x, CAbility.EAbilityType.AdjustInitiative);
				if (list.Count > 0)
				{
					foreach (CActiveBonus item in list)
					{
						if (item.BespokeBehaviour is CAdjustInitiativeActiveBonus_FocusInitiative cAdjustInitiativeActiveBonus_FocusInitiative)
						{
							num = cAdjustInitiativeActiveBonus_FocusInitiative.SetFocusInitiative();
						}
					}
				}
				list = CActiveBonus.FindApplicableActiveBonuses(y, CAbility.EAbilityType.AdjustInitiative);
				if (list.Count > 0)
				{
					foreach (CActiveBonus item2 in list)
					{
						if (item2.BespokeBehaviour is CAdjustInitiativeActiveBonus_FocusInitiative cAdjustInitiativeActiveBonus_FocusInitiative2)
						{
							num2 = cAdjustInitiativeActiveBonus_FocusInitiative2.SetFocusInitiative();
						}
					}
				}
			}
			if (num != num2)
			{
				return num.CompareTo(num2);
			}
			if (x.Type == CActor.EType.Player && y.Type == CActor.EType.Player && x.SubInitiative() != y.SubInitiative())
			{
				return x.SubInitiative().CompareTo(y.SubInitiative());
			}
			if (x is CHeroSummonActor cHeroSummonActor && y is CPlayerActor cPlayerActor)
			{
				if (cPlayerActor == cHeroSummonActor.Summoner)
				{
					return -1;
				}
				return cHeroSummonActor.Summoner.SubInitiative().CompareTo(cPlayerActor.SubInitiative());
			}
			if (y is CHeroSummonActor cHeroSummonActor2 && x is CPlayerActor cPlayerActor2)
			{
				if (cPlayerActor2 == cHeroSummonActor2.Summoner)
				{
					return 1;
				}
				return cPlayerActor2.SubInitiative().CompareTo(cHeroSummonActor2.Summoner.SubInitiative());
			}
			if (x.Type == CActor.EType.Player && y.IsMonsterType)
			{
				return -1;
			}
			if (x.IsMonsterType && y.Type == CActor.EType.Player)
			{
				return 1;
			}
			if (x.Class is CMonsterClass cMonsterClass && y.Class is CMonsterClass cMonsterClass2)
			{
				if (cMonsterClass.MonsterClassIDToActImmediatelyBefore == cMonsterClass2.ID)
				{
					return -1;
				}
				if (cMonsterClass.ID == cMonsterClass2.MonsterClassIDToActImmediatelyBefore)
				{
					return 1;
				}
				int monsterGroupType = ((CEnemyActor)x).MonsterGroupType;
				int num3 = ((CEnemyActor)y).MonsterGroupType.CompareTo(monsterGroupType);
				if (num3 != 0)
				{
					return num3;
				}
				num3 = string.Compare(x.Class.ID, y.Class.ID);
				if (num3 != 0)
				{
					return num3;
				}
				bool flag = cMonsterClass.NonEliteVariant == null;
				bool flag2 = cMonsterClass2.NonEliteVariant == null;
				if (flag != flag2)
				{
					if (flag && !flag2)
					{
						return 1;
					}
					return -1;
				}
				num3 = string.Compare(x.Type.ToString(), y.Type.ToString());
				if (num3 != 0)
				{
					return num3;
				}
			}
			int iD = x.ID;
			if (x is CHeroSummonActor cHeroSummonActor3)
			{
				iD = cHeroSummonActor3.Summoner.ID;
			}
			int iD2 = y.ID;
			if (y is CHeroSummonActor cHeroSummonActor4)
			{
				iD2 = cHeroSummonActor4.Summoner.ID;
			}
			return iD.CompareTo(iD2);
		}
	}

	public class CRangeSortedActor
	{
		public CActor m_Actor;

		public int m_Range;

		public bool m_Self;

		public CRangeSortedActor(CActor actor, int range, bool self = false)
		{
			m_Actor = actor;
			m_Range = range;
			m_Self = self;
		}
	}

	public class CurrentActorAction
	{
		public CAction Action { get; private set; }

		public CBaseCard BaseCard { get; private set; }

		public CurrentActorAction(CAction action, CBaseCard baseCard)
		{
			Action = action;
			BaseCard = baseCard;
		}
	}

	public class OverrideActorForActionStackData
	{
		public CActor PreviousActor;

		public bool OverrideSwitchedType;

		public CActor.EType OriginalType;

		public bool OriginallyUnderMyControl;

		public bool OverrideKillActor;

		public int PreOverrideAugmentSlots;

		public List<CAugment> AddedAugments;

		public CActor ControllingActor;

		public CActor UnderMyControlActor;

		public bool UseControllingActorModifierDeck;

		public OverrideActorForActionStackData(CActor previousActor, bool overrideSwitchedType, CActor.EType originalType, bool originallyUnderMyControl, bool overrideKillActor, int preOverrideAugmentSlots, List<CAugment> addedAugments = null, CActor controllingActor = null, CActor underMyControlActor = null, bool useControllingActorModifierDeck = false)
		{
			PreviousActor = previousActor;
			OverrideSwitchedType = overrideSwitchedType;
			OriginalType = originalType;
			OriginallyUnderMyControl = originallyUnderMyControl;
			OverrideKillActor = overrideKillActor;
			PreOverrideAugmentSlots = preOverrideAugmentSlots;
			AddedAugments = addedAugments;
			ControllingActor = controllingActor;
			UnderMyControlActor = underMyControlActor;
			UseControllingActorModifierDeck = useControllingActorModifierDeck;
		}
	}

	public class QueueOverrideActorForTurnData
	{
		public CActor.EType OverrideActorType;

		public List<CAbility> OverrideActionAbilities;

		public CBaseCard OverrideActionBaseCard;

		public CActor ControllingActor;

		public bool UseControllingActorPlayerControl;

		public QueueOverrideActorForTurnData(CActor.EType overrideActorType, List<CAbility> overrideActionAbilities, CBaseCard overrideActionBaseCard, CActor controllingActor = null, bool useControllingActorPlayerControl = false)
		{
			OverrideActorType = overrideActorType;
			OverrideActionAbilities = overrideActionAbilities;
			OverrideActionBaseCard = overrideActionBaseCard;
			ControllingActor = controllingActor;
			UseControllingActorPlayerControl = useControllingActorPlayerControl;
		}
	}

	public class OverrideActorForTurnData
	{
		public CActor OverridenActor;

		public bool OverrideSwitchedType;

		public CActor.EType OriginalType;

		public bool OriginallyUnderMyControl;

		public CActor ControllingActor;

		public OverrideActorForTurnData(CActor overridenActor, bool overrideSwitchedType, CActor.EType originalType, bool originallyUnderMyControl, CActor controllingActor)
		{
			OverridenActor = overridenActor;
			OverrideSwitchedType = overrideSwitchedType;
			OriginalType = originalType;
			OriginallyUnderMyControl = originallyUnderMyControl;
			ControllingActor = controllingActor;
		}
	}

	public class DamageData
	{
		public CActor ActorDamaged;

		public int DamageSourceStrength;

		public int Shield;

		public int PreDamageHealth;

		public CActor DamageSourceActor;

		public CAbility DamageSourceAbility;

		public CAbility.EAbilityType DamageSourceAbilityType;

		public CActor.ECauseOfDamage CauseOfDamage;

		public bool IsTrapDamage;

		public bool IsTerrainDamage;

		public bool CheckIfPlayerCanAvoidDamageByBurningCards;

		public bool CannotPreventDamageWithActiveBonus;

		public int DamageAvoided;

		public int DamageShielded;

		public int DamageShieldedByItems;

		public bool IsDirectDamage
		{
			get
			{
				if (!IsTrapDamage && !IsTerrainDamage)
				{
					return DamageSourceAbilityType != CAbility.EAbilityType.Attack;
				}
				return true;
			}
		}

		public DamageData(CActor actorDamaged, int damageSourceStrength, int shield, int preDamageHealth, CActor damageSourceActor, CAbility damageSourceAbility, CAbility.EAbilityType damageSourceAbilityType, CActor.ECauseOfDamage causeOfDamage, bool isTrapDamage, bool isTerrainDamage, bool checkIfPlayerCanAvoidDamageByBurningCards, bool cannotPreventDamageWithActiveBonus)
		{
			ActorDamaged = actorDamaged;
			DamageSourceStrength = damageSourceStrength;
			Shield = shield;
			PreDamageHealth = preDamageHealth;
			DamageSourceActor = damageSourceActor;
			DamageSourceAbility = damageSourceAbility;
			DamageSourceAbilityType = damageSourceAbilityType;
			CauseOfDamage = causeOfDamage;
			IsTrapDamage = isTrapDamage;
			IsTerrainDamage = isTerrainDamage;
			CheckIfPlayerCanAvoidDamageByBurningCards = checkIfPlayerCanAvoidDamageByBurningCards;
			CannotPreventDamageWithActiveBonus = cannotPreventDamageWithActiveBonus;
		}
	}

	public static volatile Thread ThreadCheckingForActiveBonuses = null;

	public static MathParser s_Parser = new MathParser();

	public static ActorInitiativeComparer s_ActorInitiativeComparer = new ActorInitiativeComparer();

	public static ActorInitiativeComparer s_ActorAdjustedInitiativeComparer = new ActorInitiativeComparer(checkforadjustment: true);

	public static volatile bool s_ThreadAboutToAbort;

	private static List<CActor> s_InitiativeSortedActors = new List<CActor>();

	private static CActor s_LastActor;

	private static CActor s_CurrentActor;

	private static CActor s_TurnActor;

	private static EActionSelectionFlag s_CurrentActionSelectionFlag;

	private static EActionSelectionFlag s_CachedActionSelectionFlag;

	private static List<CActionAugmentation> s_CurrentActionValidAugmentations = new List<CActionAugmentation>();

	private static List<CActionAugmentation> s_CurrentActionSelectedAugmentations = new List<CActionAugmentation>();

	private static int s_RoundCount;

	private static List<KeyValuePair<CEnemyActor, CActor>> s_EnemyKillTracker;

	private static int s_DamageInflictedThisTurn;

	private static List<CActor> s_ActorsMovedThisTurn;

	private static List<CActor> s_ActorsKilledThisTurn;

	private static List<CActor> s_ActorsKilledThisRound;

	private static int s_TargetsDamagedInPrevAttackThisTurn;

	private static Guid s_TargetsDamagedInPrevAttackThisTurnAbilityID;

	private static int s_TargetsActuallyDamagedInPrevAttackThisTurn;

	private static Guid s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID;

	private static int s_TargetsDamagedInPrevDamageAbilityThisTurn;

	private static Guid s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID;

	private static List<TileIndex> s_HexesMovedThisTurn = new List<TileIndex>();

	private static int s_ObstaclesDestroyedThisTurn;

	private static int s_HazardousTerrainTilesMovedOverThisTurn;

	private static int s_DifficultTerrainTilesMovedOverThisTurn;

	private static volatile EAvoidDamageOption s_PlayerSelectedToAvoidDamage;

	private static volatile List<CAbilityCard> s_CardsBurnedToAvoidDamage;

	private static volatile bool s_RecievedPlayerActorToAvoidDamageResponse;

	private static volatile CPlayerActor s_PlayerActorToAvoidDamage;

	private static volatile bool s_RecievedSelectedToAvoidDamageResponse;

	private static volatile List<CActiveBonus> s_ToggledShieldBonuses;

	private static int s_RecievedDamagedReducedByShieldItems;

	private static int s_RecievedDamagePreventedByActiveBonuses;

	private static Dictionary<CActor, QueueOverrideActorForTurnData> s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();

	private static Stack<OverrideActorForActionStackData> s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();

	private static Stack<OverrideActorForActionStackData> s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();

	private static int s_CheckAdjustInitiativeIndex = 0;

	private const int c_InitiativeBitShift = 24;

	private const int c_PlayerEnemyBitShift = 16;

	private const int c_EliteBitShift = 8;

	private const string TAG_FORMAT = "<color=#f1dc81>[GameState]</color>";

	public static Dictionary<string, int> EnemyKillStatsByClass
	{
		get
		{
			if (s_EnemyKillTracker != null)
			{
				return s_EnemyKillTracker?.GroupBy((KeyValuePair<CEnemyActor, CActor> item) => item.Key.MonsterClass.ID).ToDictionary((IGrouping<string, KeyValuePair<CEnemyActor, CActor>> g) => g.Key, (IGrouping<string, KeyValuePair<CEnemyActor, CActor>> g) => g.Count());
			}
			return new Dictionary<string, int>();
		}
	}

	public static Dictionary<string, int> EnemyEncounteredByClass
	{
		get
		{
			Dictionary<string, int> enemyKillStatsByClass = EnemyKillStatsByClass;
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				string iD = enemy.MonsterClass.ID;
				if (enemyKillStatsByClass.ContainsKey(iD))
				{
					enemyKillStatsByClass[iD]++;
				}
				else
				{
					enemyKillStatsByClass.Add(iD, 1);
				}
			}
			return enemyKillStatsByClass;
		}
	}

	public static CActor InternalCurrentActor => s_CurrentActor;

	public static CActor TurnActor => s_TurnActor;

	public static List<CActor> InitiativeSortedActors => s_InitiativeSortedActors;

	public static CurrentActorAction CurrentAction { get; set; }

	public static List<CActiveBonus> PendingOnLongRestBonuses { get; set; }

	public static List<CActiveBonus> PendingActiveBonuses { get; set; }

	public static List<CActiveBonus> PendingStartEndTurnAbilityBonusTriggers { get; set; }

	public static List<Tuple<CAbility, CActor>> PendingScenarioModifierAbilities { get; set; }

	public static List<CPlayerActor> PendingCompanionSummonActors { get; set; }

	public static bool SkipNextPhase { get; set; }

	public static ActionSelectionSequenceType CurrentActionSelectionSequence
	{
		get
		{
			if (ExtraTurnActionSelectionFlagStack.Count <= 0)
			{
				return GetCurrentActionSelectionSequenceTypeFromFlags(s_CurrentActionSelectionFlag);
			}
			return GetCurrentActionSelectionSequenceTypeFromFlags(ExtraTurnActionSelectionFlagStack.Peek());
		}
	}

	public static Stack<EActionSelectionFlag> ExtraTurnActionSelectionFlagStack { get; set; }

	public static bool HasPlayedTopAction
	{
		get
		{
			if (ExtraTurnActionSelectionFlagStack.Count <= 0)
			{
				return HasPlayedTopActionFromFlag(s_CurrentActionSelectionFlag);
			}
			return HasPlayedTopActionFromFlag(ExtraTurnActionSelectionFlagStack.Peek());
		}
	}

	public static bool HasPlayedBottomAction
	{
		get
		{
			if (ExtraTurnActionSelectionFlagStack.Count <= 0)
			{
				return HasPlayedBottomActionFromFlag(s_CurrentActionSelectionFlag);
			}
			return HasPlayedBottomActionFromFlag(ExtraTurnActionSelectionFlagStack.Peek());
		}
	}

	public static EActionInitiator CurrentActionInitiator { get; set; }

	public static List<CActionAugmentation> CurrentActionValidAugmentations => s_CurrentActionValidAugmentations;

	public static List<CActionAugmentation> CurrentActionSelectedAugmentations => s_CurrentActionSelectedAugmentations;

	public static bool OutputAdvantageInfo { get; set; }

	public static int RoundCount => s_RoundCount;

	public static Stack<DamageData> DamageDataStack { get; private set; }

	public static DamageData CurrentDamageData
	{
		get
		{
			if (DamageDataStack == null || DamageDataStack.Count <= 0)
			{
				return null;
			}
			return DamageDataStack.Peek();
		}
	}

	public static CAttackSummary.TargetSummary LastTargetSummary { get; private set; }

	public static CAbilityCard RoundAbilityCardselected { get; private set; }

	public static CBaseCard.ActionType RoundAbilityCardActionType { get; private set; }

	public static EAvoidDamageOption PlayerSelectedToAvoidDamage => s_PlayerSelectedToAvoidDamage;

	public static List<CAbilityCard> CardsBurnedToAvoidDamage => s_CardsBurnedToAvoidDamage;

	public static int DamageInflictedThisTurn => s_DamageInflictedThisTurn;

	public static List<CActor> ActorsKilledThisTurn => s_ActorsKilledThisTurn;

	public static List<CActor> ActorsKilledThisRound => s_ActorsKilledThisRound;

	public static int TargetsDamagedInPrevAttackThisTurn => s_TargetsDamagedInPrevAttackThisTurn;

	public static int TargetsActuallyDamagedInPrevAttackThisTurn => s_TargetsActuallyDamagedInPrevAttackThisTurn;

	public static int TargetsDamagedInPrevDamageAbilityThisTurn => s_TargetsDamagedInPrevDamageAbilityThisTurn;

	public static List<CActor> ActorsMovedThisTurn => s_ActorsMovedThisTurn;

	public static List<TileIndex> HexesMovedThisTurn => s_HexesMovedThisTurn;

	public static int ObstaclesDestroyedThisTurn => s_ObstaclesDestroyedThisTurn;

	public static bool WaitingForMercenarySpecialMechanicSlotChoice { get; set; }

	public static bool OverridingCurrentActor { get; set; }

	public static Stack<OverrideActorForActionStackData> OverridenActionActorStack => s_OverridenActionActorStack;

	public static Dictionary<CActor, QueueOverrideActorForTurnData> ActorsToOverrideTurns => s_ActorsToOverrideTurns;

	public static OverrideActorForTurnData CurrentOverridenActorForTurnData { get; private set; }

	public static Tuple<CActor, int> RedirectedDamageToActor { get; set; }

	public static Stack<CAbilityCard> CachedExtraTurnInitiatorCardStack { get; private set; }

	public static CObjectObstacle LastDestroyedObstacle { get; set; }

	public static List<CPlayerActor> PreInitiativeAdjustedPlayerActors { get; set; }

	public static int RecievedDamagedReducedByShieldItems => s_RecievedDamagedReducedByShieldItems;

	public static bool ThreadIsSleeping { get; set; }

	public static bool ShuffleAttackModsEnabledForPlayers { get; set; }

	public static bool ShuffleAbilityDecksEnabledForMonsters { get; set; }

	public static bool ShuffleAttackModsEnabledForMonsters { get; set; }

	public static bool RandomiseOnLoad { get; set; }

	public static bool WaitingForPlayerActorToAvoidDamageResponse => !s_RecievedPlayerActorToAvoidDamageResponse;

	public static bool WaitingForPlayerToSelectDamageResponse => !s_RecievedSelectedToAvoidDamageResponse;

	public static EActionSelectionFlag SetFlag(EActionSelectionFlag a, EActionSelectionFlag b)
	{
		return a | b;
	}

	public static EActionSelectionFlag UnsetFlag(EActionSelectionFlag a, EActionSelectionFlag b)
	{
		return a & ~b;
	}

	public static bool HasFlag(EActionSelectionFlag a, EActionSelectionFlag b)
	{
		return (a & b) == b;
	}

	public static EActionSelectionFlag ToggleFlag(EActionSelectionFlag a, EActionSelectionFlag b)
	{
		return a ^ b;
	}

	public static ActionSelectionSequenceType GetCurrentActionSelectionSequenceTypeFromFlags(EActionSelectionFlag checkSelectionFlag)
	{
		if (HasFlag(checkSelectionFlag, EActionSelectionFlag.TopActionPlayed) && HasFlag(checkSelectionFlag, EActionSelectionFlag.BottomActionPlayed))
		{
			return ActionSelectionSequenceType.Complete;
		}
		if (HasFlag(checkSelectionFlag, EActionSelectionFlag.TopActionPlayed) || HasFlag(checkSelectionFlag, EActionSelectionFlag.BottomActionPlayed))
		{
			return ActionSelectionSequenceType.SecondAction;
		}
		return ActionSelectionSequenceType.FirstAction;
	}

	public static bool HasPlayedTopActionFromFlag(EActionSelectionFlag checkSelectionFlag)
	{
		return HasFlag(checkSelectionFlag, EActionSelectionFlag.TopActionPlayed);
	}

	public static bool HasPlayedBottomActionFromFlag(EActionSelectionFlag checkSelectionFlag)
	{
		return HasFlag(checkSelectionFlag, EActionSelectionFlag.BottomActionPlayed);
	}

	public static int UpdatedInitiative(CActor actor, int initiative)
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.AdjustInitiative);
		if (list.Count > 0)
		{
			foreach (CActiveBonus item in list)
			{
				if (item.BespokeBehaviour is CAdjustInitiativeActiveBonus_FocusInitiative cAdjustInitiativeActiveBonus_FocusInitiative)
				{
					initiative = cAdjustInitiativeActiveBonus_FocusInitiative.SetFocusInitiative();
				}
			}
		}
		return initiative;
	}

	public static int UpdatedInitiativeWithSummons(CActor actor, int initiative)
	{
		CActor actor2 = actor;
		if (actor is CHeroSummonActor { Summoner: not null } cHeroSummonActor)
		{
			actor2 = cHeroSummonActor.Summoner;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor2, CAbility.EAbilityType.AdjustInitiative);
		if (list.Count > 0)
		{
			foreach (CActiveBonus item in list)
			{
				if (item.BespokeBehaviour is CAdjustInitiativeActiveBonus_FocusInitiative cAdjustInitiativeActiveBonus_FocusInitiative)
				{
					initiative = cAdjustInitiativeActiveBonus_FocusInitiative.SetFocusInitiative();
				}
			}
		}
		return initiative;
	}

	public static void Reset()
	{
		ScenarioManager.Reset();
		CActorStatic.Reset();
		CActor.Reset();
		CurrentAction = null;
		PendingOnLongRestBonuses = null;
		PendingActiveBonuses = null;
		PendingStartEndTurnAbilityBonusTriggers = null;
		PendingScenarioModifierAbilities = null;
		PendingCompanionSummonActors = null;
		SkipNextPhase = false;
		CurrentActionInitiator = EActionInitiator.None;
		OutputAdvantageInfo = false;
		DamageDataStack = null;
		LastTargetSummary = null;
		RoundAbilityCardselected = null;
		RoundAbilityCardActionType = CBaseCard.ActionType.TopAction;
		WaitingForMercenarySpecialMechanicSlotChoice = false;
		OverridingCurrentActor = false;
		RedirectedDamageToActor = null;
		s_Parser = new MathParser();
		s_ActorInitiativeComparer = new ActorInitiativeComparer();
		s_ActorAdjustedInitiativeComparer = new ActorInitiativeComparer(checkforadjustment: true);
		s_InitiativeSortedActors = new List<CActor>();
		s_LastActor = null;
		s_CurrentActor = null;
		s_CurrentActionSelectionFlag = EActionSelectionFlag.None;
		ExtraTurnActionSelectionFlagStack = new Stack<EActionSelectionFlag>();
		CachedExtraTurnInitiatorCardStack = new Stack<CAbilityCard>();
		s_CurrentActionValidAugmentations = new List<CActionAugmentation>();
		s_CurrentActionSelectedAugmentations = new List<CActionAugmentation>();
		s_RoundCount = 0;
		s_EnemyKillTracker = null;
		s_DamageInflictedThisTurn = 0;
		s_ActorsMovedThisTurn = new List<CActor>();
		s_ActorsKilledThisTurn = new List<CActor>();
		s_ActorsKilledThisRound = new List<CActor>();
		s_TargetsDamagedInPrevAttackThisTurn = 0;
		s_TargetsDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID = Guid.Empty;
		s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID = Guid.Empty;
		s_HexesMovedThisTurn = new List<TileIndex>();
		s_ObstaclesDestroyedThisTurn = 0;
		s_HazardousTerrainTilesMovedOverThisTurn = 0;
		s_DifficultTerrainTilesMovedOverThisTurn = 0;
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_CardsBurnedToAvoidDamage = new List<CAbilityCard>();
		s_ToggledShieldBonuses = new List<CActiveBonus>();
		s_RecievedPlayerActorToAvoidDamageResponse = true;
		s_PlayerActorToAvoidDamage = null;
		s_RecievedSelectedToAvoidDamageResponse = true;
		s_ThreadAboutToAbort = false;
		s_RecievedDamagedReducedByShieldItems = 0;
		s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();
		s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();
		s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();
		s_CheckAdjustInitiativeIndex = 0;
		DLLDebug.Log("GAME STATE RESET!");
	}

	public static void Start()
	{
		s_RoundCount = 0;
		s_EnemyKillTracker = new List<KeyValuePair<CEnemyActor, CActor>>();
		s_OverridenActionActorStack = new Stack<OverrideActorForActionStackData>();
		s_ExtraTurnActorStack = new Stack<OverrideActorForActionStackData>();
		s_ActorsToOverrideTurns = new Dictionary<CActor, QueueOverrideActorForTurnData>();
		PendingOnLongRestBonuses = new List<CActiveBonus>();
		PendingActiveBonuses = new List<CActiveBonus>();
		PendingScenarioModifierAbilities = new List<Tuple<CAbility, CActor>>();
		PendingStartEndTurnAbilityBonusTriggers = new List<CActiveBonus>();
		PendingCompanionSummonActors = new List<CPlayerActor>();
		DamageDataStack = new Stack<DamageData>();
		s_RoundCount++;
		CheckObjectives();
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			allActor.HasTakenWoundDamageThisTurn = false;
			allActor.ActorActionHasHappened = false;
			allActor.ProcessConditionTokens(EConditionDecTrigger.Rounds);
		}
		CNextRound_MessageData message = new CNextRound_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
		if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.StartScenarioEffects);
		}
		else
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.SelectAbilityCardsOrLongRest);
		}
	}

	public static void Stop()
	{
		PhaseManager.Stop();
	}

	public static void PlayerSelectedAbilityCardAction(CAbilityCard roundAbilityCard, CBaseCard.ActionType actionType)
	{
		_ = (CPlayerActor)s_CurrentActor;
		CurrentActionInitiator = EActionInitiator.AbilityCard;
		CAction cAction = roundAbilityCard.GetActionForType(actionType).Copy();
		roundAbilityCard.ActionHasHappened = false;
		CurrentAction = new CurrentActorAction(cAction, roundAbilityCard);
		roundAbilityCard.SetSelectedAction(cAction);
		RoundAbilityCardselected = roundAbilityCard;
		RoundAbilityCardActionType = actionType;
		for (int num = s_CurrentActionSelectedAugmentations.Count - 1; num >= 0; num--)
		{
			CActionAugmentation cActionAugmentation = s_CurrentActionSelectedAugmentations[num];
			if (cActionAugmentation.ActionID != CurrentAction.Action.ID)
			{
				foreach (ElementInfusionBoardManager.EElement element in cActionAugmentation.Elements)
				{
					SelectActionAugmentation(cActionAugmentation, element, remove: true);
				}
			}
		}
	}

	public static int DrawAndApplyAttackModifierCards(CAbilityAttack attackAbility, CActor actor, CActor currentTarget, int attackStrength, EAdvantageStatuses advantageStatus, out List<AttackModifierYMLData> attackModifierCards, out List<AttackModifierYMLData> notUsedAttackModifierCards, out int pierce, out SEventAttackModifier attackModifierEvent, out int addTargetsDrawn, int attackIndex)
	{
		attackModifierCards = new List<AttackModifierYMLData>();
		pierce = 0;
		addTargetsDrawn = 0;
		string text = "(" + attackStrength;
		CActor controllingActor = s_CurrentActor;
		if (OverridingCurrentActor)
		{
			OverrideActorForActionStackData overrideActorForActionStackData = OverridenActionActorStack.Peek();
			if (overrideActorForActionStackData != null && overrideActorForActionStackData.ControllingActor != null && overrideActorForActionStackData.UseControllingActorModifierDeck)
			{
				controllingActor = overrideActorForActionStackData.ControllingActor;
			}
		}
		if (CAbilityAttack.s_AttackModifierCardOverride == null)
		{
			List<AttackModifierYMLData> list;
			if (controllingActor.Class is CCharacterClass || controllingActor.Class is CHeroSummonClass)
			{
				CHeroSummonActor cHeroSummonActor = ((controllingActor is CHeroSummonActor) ? (controllingActor as CHeroSummonActor) : null);
				CCharacterClass characterClass = ((cHeroSummonActor != null) ? cHeroSummonActor.Summoner : ((CPlayerActor)controllingActor)).CharacterClass;
				if (characterClass.AttackModifierCards.Count == 0)
				{
					characterClass.CheckAttackModifierCardShuffle(force: true);
				}
				list = characterClass.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out notUsedAttackModifierCards);
				if (OutputAdvantageInfo)
				{
					DLLDebug.LogInfo(((cHeroSummonActor != null) ? cHeroSummonActor.Class.ID : characterClass.ID) + " has advantage status: " + advantageStatus.ToString() + ".  Drew " + characterClass.LastDrawnAttackModifierCards.Count + " card" + ((characterClass.LastDrawnAttackModifierCards.Count > 1) ? "s: " : ": ") + string.Join(", ", characterClass.LastDrawnAttackModifierCards.Select((AttackModifierYMLData x) => x.MathModifier).ToArray()) + ".  Used: " + string.Join(", ", list.Select((AttackModifierYMLData y) => y.MathModifier).ToArray()));
				}
			}
			else
			{
				if (!(controllingActor.Class is CMonsterClass))
				{
					DLLDebug.LogError("Invalid Actor Type");
					notUsedAttackModifierCards = new List<AttackModifierYMLData>();
					attackModifierEvent = new SEventAttackModifier();
					return 0;
				}
				CEnemyActor cEnemyActor = (CEnemyActor)controllingActor;
				if (MonsterClassManager.IsDeckEmpty(cEnemyActor))
				{
					MonsterClassManager.CheckAttackModifierCardShuffle(cEnemyActor);
				}
				list = MonsterClassManager.DrawAttackModifierCards(actor, attackStrength, advantageStatus, out notUsedAttackModifierCards);
			}
			foreach (AttackModifierYMLData item in list)
			{
				attackModifierCards.Add(item.Copy());
			}
			currentTarget.m_OnDrawModifierListeners?.Invoke(ref attackModifierCards, s_CurrentActor, currentTarget);
			List<string> list2 = new List<string>();
			foreach (AttackModifierYMLData attackModifierCard in attackModifierCards)
			{
				if (attackModifierCard.MathModifier.Contains("*") || attackModifierCard.MathModifier.Contains("/"))
				{
					list2.Add(attackModifierCard.MathModifier);
				}
				else
				{
					text += attackModifierCard.MathModifier;
				}
			}
			text += ")";
			foreach (string item2 in list2)
			{
				text += item2;
			}
		}
		else
		{
			attackModifierCards = new List<AttackModifierYMLData>
			{
				new AttackModifierYMLData("DebugOverride", CAbilityAttack.s_AttackModifierCardOverride)
			};
			notUsedAttackModifierCards = new List<AttackModifierYMLData>();
			text += CAbilityAttack.s_AttackModifierCardOverride;
			CAbilityAttack.s_AttackModifierCardOverride = null;
		}
		int num = 0;
		try
		{
			num = Convert.ToInt32(s_Parser.Parse(text, isRadians: false));
		}
		catch
		{
		}
		DLLDebug.Log("Attack modifier Expression String: " + text + "\tResult: " + num);
		CAbility ability = (PhaseManager.CurrentPhase as CPhaseAction).CurrentPhaseAbility.m_Ability;
		int num2 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Card.HealAmount);
		int num3 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Card.DamageAmount);
		int num4 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Abilities.Where((CAbility w) => w is CAbilityPull).Sum((CAbility cAbility3) => cAbility3.Strength));
		int num5 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Abilities.Where((CAbility w) => w is CAbilityPush).Sum((CAbility cAbility3) => cAbility3.Strength));
		int num6 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Abilities.Where((CAbility w) => w is CAbilityShield).Sum((CAbility cAbility3) => cAbility3.Strength));
		int num7 = attackModifierCards.Sum((AttackModifierYMLData s) => s.Abilities.Where((CAbility w) => w is CAbilityRefreshItemCards).Sum((CAbility cAbility3) => cAbility3.Strength));
		addTargetsDrawn = attackModifierCards.Count((AttackModifierYMLData s) => s.AddTarget);
		int num8 = attackModifierCards.Count((AttackModifierYMLData s) => s.Card.HealAlly);
		List<CItem.EItemSlotState> list3 = new List<CItem.EItemSlotState>();
		foreach (AttackModifierYMLData attackModifierCard2 in attackModifierCards)
		{
			foreach (CAbility ability2 in attackModifierCard2.Abilities)
			{
				if (ability2 is CAbilityRefreshItemCards cAbilityRefreshItemCards)
				{
					list3.AddRange(cAbilityRefreshItemCards.SlotStatesToRefresh);
				}
			}
		}
		list3 = list3.Distinct().ToList();
		pierce = attackModifierCards.Sum((AttackModifierYMLData s) => s.Overrides.Where((CAbilityOverride w) => w.Pierce.HasValue).Sum((CAbilityOverride cAbilityOverride) => cAbilityOverride.Pierce.Value));
		bool flag = currentTarget.WillActorDie(num, pierce, attackAbility);
		List<CCondition.ENegativeCondition> list4 = new List<CCondition.ENegativeCondition>();
		foreach (CCondition.ENegativeCondition negCon in (from w in attackModifierCards.SelectMany((AttackModifierYMLData sm) => sm.Overrides)
			where w.NegativeConditions != null
			select w).SelectMany((CAbilityOverride sm2) => sm2.NegativeConditions))
		{
			if (!attackAbility.NegativeConditions.ContainsKey(negCon))
			{
				CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCon.ToString());
				attackAbility.NegativeConditions.Add(negCon, CAbility.CreateAbility(abilityType, attackAbility.AbilityFilter, attackAbility.IsMonsterAbility, attackAbility.IsTargetedAbility, 0, 1, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true));
				list4.Add(negCon);
			}
			else if (negCon == CCondition.ENegativeCondition.Curse)
			{
				if (attackAbility.NegativeConditions[negCon].Strength == 0)
				{
					attackAbility.NegativeConditions[negCon].Strength++;
				}
				attackAbility.NegativeConditions[negCon].Strength++;
			}
		}
		List<CCondition.EPositiveCondition> list5 = new List<CCondition.EPositiveCondition>();
		foreach (CCondition.EPositiveCondition posCon in (from w in attackModifierCards.SelectMany((AttackModifierYMLData sm) => sm.Overrides)
			where w.PositiveConditions != null
			select w).SelectMany((CAbilityOverride sm2) => sm2.PositiveConditions))
		{
			if (!attackAbility.PositiveConditions.ContainsKey(posCon))
			{
				CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCon.ToString());
				attackAbility.PositiveConditions.Add(posCon, CAbility.CreateAbility(abilityType2, attackAbility.AbilityFilter, attackAbility.IsMonsterAbility, attackAbility.IsTargetedAbility, 0, 1, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true));
				list5.Add(posCon);
			}
			else if (posCon == CCondition.EPositiveCondition.Bless)
			{
				if (attackAbility.PositiveConditions[posCon].Strength == 0)
				{
					attackAbility.PositiveConditions[posCon].Strength++;
				}
				attackAbility.PositiveConditions[posCon].Strength++;
			}
		}
		List<ElementInfusionBoardManager.EElement> list6 = new List<ElementInfusionBoardManager.EElement>();
		foreach (ElementInfusionBoardManager.EElement item3 in (from w in attackModifierCards.SelectMany((AttackModifierYMLData sm) => sm.Abilities)
			where w is CAbilityInfuse
			select w).SelectMany((CAbility sm2) => (sm2 as CAbilityInfuse).ElementsToInfuse))
		{
			if (!list6.Contains(item3))
			{
				list6.Add(item3);
			}
		}
		if (num3 > 0)
		{
			CAbilityDamage cAbilityDamage = CAbility.CreateAbility(CAbility.EAbilityType.Damage, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), isMonster: false, isTargetedAbility: true, num3, 1, -1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true, null, null, isSub: true, isInline: true) as CAbilityDamage;
			cAbilityDamage.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityDamage).ID);
			cAbilityDamage.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbilityDamage.MiscAbilityData.AutotriggerAbility = true;
			cAbilityDamage.MiscAbilityData.UseParentTiles = false;
			cAbilityDamage.MiscAbilityData.AllTargetsAdjacentToParentTargets = true;
			attackAbility.ModifierAbilities.Add(cAbilityDamage);
		}
		if (num2 > 0)
		{
			if (num8 > 0)
			{
				CAbilityHeal cAbilityHeal = CAbility.CreateAbility(CAbility.EAbilityType.Heal, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Ally), isMonster: false, isTargetedAbility: true, num2, 1, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true) as CAbilityHeal;
				cAbilityHeal.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityHeal).ID);
				cAbilityHeal.Targeting = CAbility.EAbilityTargeting.All;
				cAbilityHeal.MiscAbilityData = new AbilityData.MiscAbilityData();
				cAbilityHeal.MiscAbilityData.AutotriggerAbility = false;
				cAbilityHeal.AnimOverload = "Heal";
				attackAbility.ModifierAbilities.Add(cAbilityHeal);
			}
			else
			{
				CAbilityHeal cAbilityHeal2 = CAbility.CreateAbility(CAbility.EAbilityType.Heal, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, num2, 1, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true) as CAbilityHeal;
				cAbilityHeal2.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityHeal).ID);
				cAbilityHeal2.MiscAbilityData = new AbilityData.MiscAbilityData();
				cAbilityHeal2.MiscAbilityData.AutotriggerAbility = true;
				attackAbility.ModifierAbilities.Add(cAbilityHeal2);
			}
		}
		if (list6.Count > 0)
		{
			CAbilityInfuse cAbilityInfuse = CAbility.CreateAbility(CAbility.EAbilityType.Infuse, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 0, 1, 1, 1, EConditionDecTrigger.Turns, list6, showElementPicker: false, null, isModifierAbility: true) as CAbilityInfuse;
			cAbilityInfuse.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityInfuse).ID);
			cAbilityInfuse.Start(actor, actor);
			attackAbility.ModifierAbilities.Add(cAbilityInfuse);
		}
		if (num4 > 0 && !flag)
		{
			bool flag2 = false;
			for (int num9 = 0; num9 < (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities.Count; num9++)
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities[num9];
				if (cPhaseAbility.m_Ability.AbilityType == CAbility.EAbilityType.Pull && cPhaseAbility.m_Ability.IsSubAbility)
				{
					CAbilityOverride abilityOverride = CAbilityOverride.CreateAbilityOverride(cPhaseAbility.m_Ability, num4, null, null);
					cPhaseAbility.m_Ability.OverrideAbilityValues(abilityOverride, perform: false);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Pull, ability.AbilityFilter, isMonster: false, isTargetedAbility: true, num4, ability.Range, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true);
				cAbility.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityPull).ID);
				cAbility.TargetThisActorAutomatically = currentTarget;
				attackAbility.ModifierAbilities.Add(cAbility);
			}
			DLLDebug.LogInfo("You have applied a Pull of " + num4 + " from attack modifier cards");
		}
		if (num5 > 0 && !flag)
		{
			bool flag3 = false;
			for (int num10 = 0; num10 < (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities.Count; num10++)
			{
				CPhaseAction.CPhaseAbility cPhaseAbility2 = (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities[num10];
				if (cPhaseAbility2.m_Ability.AbilityType == CAbility.EAbilityType.Push && cPhaseAbility2.m_Ability.IsSubAbility)
				{
					CAbilityOverride abilityOverride2 = CAbilityOverride.CreateAbilityOverride(cPhaseAbility2.m_Ability, num5, null, null);
					cPhaseAbility2.m_Ability.OverrideAbilityValues(abilityOverride2, perform: false);
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.Push, ability.AbilityFilter, isMonster: false, isTargetedAbility: true, num5, ability.Range, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true);
				cAbility2.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityPush).ID);
				cAbility2.TargetThisActorAutomatically = currentTarget;
				attackAbility.ModifierAbilities.Add(cAbility2);
			}
			DLLDebug.LogInfo("You have applied a Push of " + num5 + " from attack modifier cards");
		}
		if (num6 > 0)
		{
			CAbilityShield cAbilityShield = CAbility.CreateAbility(CAbility.EAbilityType.Shield, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, num6, 1, 1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true) as CAbilityShield;
			cAbilityShield.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityShield).ID);
			cAbilityShield.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
			cAbilityShield.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbilityShield.MiscAbilityData.AutotriggerAbility = true;
			attackAbility.ModifierAbilities.Add(cAbilityShield);
		}
		if (num7 > 0)
		{
			CAbilityRefreshItemCards cAbilityRefreshItemCards2 = CAbility.CreateAbility(CAbility.EAbilityType.RefreshItemCards, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, num7, 1, -1, 1, EConditionDecTrigger.Turns, null, showElementPicker: false, null, isModifierAbility: true, null, list3) as CAbilityRefreshItemCards;
			cAbilityRefreshItemCards2.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbilityRefreshItemCards2.MiscAbilityData.AutotriggerAbility = true;
			cAbilityRefreshItemCards2.SetID(attackModifierCards.SelectMany((AttackModifierYMLData s) => s.Abilities).First((CAbility f) => f is CAbilityRefreshItemCards).ID);
			cAbilityRefreshItemCards2.Start(actor, actor);
			attackAbility.ModifierAbilities.Add(cAbilityRefreshItemCards2);
		}
		List<string> list7 = new List<string>();
		List<bool> list8 = new List<bool>();
		List<bool> list9 = new List<bool>();
		foreach (AttackModifierYMLData attackModifierCard3 in attackModifierCards)
		{
			list7.Add(attackModifierCard3.Card.AttackModifierLogString());
			list8.Add(attackModifierCard3.IsBless);
			list9.Add(attackModifierCard3.IsCurse);
		}
		List<string> list10 = new List<string>();
		foreach (AttackModifierYMLData attackModifierCard4 in attackModifierCards)
		{
			list10.Add((attackModifierCard4.NewCard != null) ? attackModifierCard4.NewCard.AttackModifierLogString() : "");
		}
		List<string> list11 = new List<string>();
		foreach (AttackModifierYMLData notUsedAttackModifierCard in notUsedAttackModifierCards)
		{
			list11.Add(notUsedAttackModifierCard.Card.AttackModifierLogString());
		}
		bool isSummon = false;
		if (actor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
			if (cEnemyActor2 != null)
			{
				isSummon = cEnemyActor2.IsSummon;
			}
		}
		bool actedOnIsSummon = false;
		if (currentTarget.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == currentTarget.ActorGuid);
			if (cEnemyActor3 != null)
			{
				actedOnIsSummon = cEnemyActor3.IsSummon;
			}
		}
		attackModifierEvent = new SEventAttackModifier(attackStrength, advantageStatus, list7, list10, list11, currentTarget.ID, num, num2, num6, pierce, list4, list5, list6, actor.Class.ID, actor.Type, isSummon, actor.Tokens.CheckPositiveTokens.ToList(), actor.Tokens.CheckNegativeTokens.ToList(), currentTarget.Class.ID, currentTarget.Type, actedOnIsSummon, currentTarget.Tokens.CheckPositiveTokens.ToList(), currentTarget.Tokens.CheckNegativeTokens.ToList(), "", attackIndex);
		return num;
	}

	public static void ActorBeenDamaged(CActor actor, int strength, bool checkIfPlayerCanAvoidDamage = false, CActor damageSource = null, CAbility damageAbility = null, CAbility.EAbilityType damageAbilityType = CAbility.EAbilityType.None, int shield = 0, bool isTrapDamage = false, bool isTerrainDamage = false, bool cannotPreventDamageWithActiveBonus = false, bool pierceInvulnerable = false)
	{
		if (strength < 0)
		{
			DLLDebug.LogWarning("Attempted to deal less than 0 damage");
		}
		else
		{
			if (!pierceInvulnerable && (damageSource == null || !damageSource.PierceInvulnerability) && actor.Invulnerable)
			{
				return;
			}
			CActor.ECauseOfDamage eCauseOfDamage = CActor.ECauseOfDamage.None;
			eCauseOfDamage = (isTrapDamage ? CActor.ECauseOfDamage.Trap : (isTerrainDamage ? CActor.ECauseOfDamage.HazardousTerrain : ((damageAbilityType == CAbility.EAbilityType.Attack) ? ((actor == damageSource) ? CActor.ECauseOfDamage.Self : ((actor.Type != damageSource.Type) ? CActor.ECauseOfDamage.EnemyAttack : CActor.ECauseOfDamage.Ally)) : ((damageAbilityType == CAbility.EAbilityType.Wound) ? CActor.ECauseOfDamage.Wound : ((actor == damageSource) ? CActor.ECauseOfDamage.Self : ((actor.Type != damageSource?.Type) ? CActor.ECauseOfDamage.EnemyDamage : CActor.ECauseOfDamage.Ally))))));
			if (damageAbility is CAbilityAttack cAbilityAttack)
			{
				LastTargetSummary = cAbilityAttack.CurrentAttackingTargetSummary;
				UpdateTargetsDamagedInPrevAttackThisTurn(cAbilityAttack);
			}
			else
			{
				LastTargetSummary = null;
				if (damageAbility is CAbilityDamage abilityDamage)
				{
					UpdateTargetsDamagedInPrevDamageAbilityThisTurn(abilityDamage);
				}
			}
			UpdateDamageInflictedThisTurn(strength);
			if (damageAbility != null)
			{
				damageAbility.DamageInflictedByAbility += strength;
				damageAbility.DamageInflictedByAbilityOnLastTarget = strength;
			}
			int preDamageHealth = actor.Health;
			actor.Damaged(strength, damageAbilityType == CAbility.EAbilityType.Attack, damageSource, damageAbility);
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
			if (actorState != null)
			{
				foreach (CObjective item in ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective o) => o.ObjectiveType == EObjectiveType.DealXDamage))
				{
					(item as CObjective_DealXDamage).SetActorDamaged(actorState, strength);
				}
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			DamageDataStack.Push(new DamageData(actor, strength, shield, preDamageHealth, damageSource, damageAbility, damageAbilityType, eCauseOfDamage, isTrapDamage, isTerrainDamage, checkIfPlayerCanAvoidDamage, cannotPreventDamageWithActiveBonus));
			List<CActiveBonus> list = new List<CActiveBonus>();
			List<CActiveBonus> list2 = new List<CActiveBonus>();
			CActiveBonus.RefreshAllAuraActiveBonuses();
			if (!cannotPreventDamageWithActiveBonus)
			{
				list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.PreventDamage);
			}
			list2.AddRange(list.FindAll((CActiveBonus it) => it is CPreventDamageActiveBonus cPreventDamageActiveBonus && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(actor) && (!cPreventDamageActiveBonus.PreventOnlyIfLethal || (cPreventDamageActiveBonus.PreventOnlyIfLethal && strength >= preDamageHealth)) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(damageAbilityType)) && it.Ability.ActiveBonusData.Consuming.Count <= 0));
			bool flag = list.Any((CActiveBonus x) => x.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards);
			bool flag2 = ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor it) => it.CharacterClass.HandAbilityCards.Count > 0 || it.CharacterClass.DiscardedAbilityCards.Count > 1);
			s_PlayerActorToAvoidDamage = null;
			if (actor is CPlayerActor cPlayerActor)
			{
				s_PlayerActorToAvoidDamage = cPlayerActor;
			}
			else
			{
				if (!(actor is CPlayerActor) && flag && flag2 && strength > 0)
				{
					s_RecievedPlayerActorToAvoidDamageResponse = false;
					CChoosePlayerActorToBurnCardToPreventDamage_MessageData message = new CChoosePlayerActorToBurnCardToPreventDamage_MessageData();
					ScenarioRuleClient.MessageHandler(message);
					bool flag3 = false;
					while (!s_RecievedPlayerActorToAvoidDamageResponse && !s_ThreadAboutToAbort)
					{
						if (!flag3)
						{
							flag3 = true;
							if (Thread.CurrentThread == ScenarioRuleClient.s_WorkThread)
							{
								DLLDebug.Log("Sleeping SRL Thread" + Environment.StackTrace);
							}
						}
						ThreadIsSleeping = true;
						Thread.Sleep(100);
						ThreadIsSleeping = false;
					}
					if (s_ThreadAboutToAbort)
					{
						s_RecievedPlayerActorToAvoidDamageResponse = true;
						s_PlayerActorToAvoidDamage = null;
						s_ThreadAboutToAbort = false;
						return;
					}
				}
				if (s_PlayerActorToAvoidDamage == null && actor is CHeroSummonActor { IsCompanionSummon: not false } cHeroSummonActor)
				{
					s_PlayerActorToAvoidDamage = cHeroSummonActor.Summoner;
				}
			}
			ContinueActorDamagedAfterSelectingPlayerActorToBurnCards(s_PlayerActorToAvoidDamage);
		}
	}

	public static void ContinueActorDamagedAfterSelectingPlayerActorToBurnCards(CPlayerActor playerActorToBurnCards = null)
	{
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_CardsBurnedToAvoidDamage.Clear();
		s_ToggledShieldBonuses.Clear();
		bool flag = playerActorToBurnCards != null && playerActorToBurnCards.IsDead;
		bool num = playerActorToBurnCards != null && playerActorToBurnCards.CharacterClass.HandAbilityCards.Count > 0;
		bool flag2 = playerActorToBurnCards != null && playerActorToBurnCards.CharacterClass.DiscardedAbilityCards.Count > 1;
		bool flag3 = CurrentDamageData.DamageSourceStrength > 0;
		bool flag4 = CurrentDamageData.ActorDamaged.Health < CurrentDamageData.PreDamageHealth;
		bool flag5 = CActor.AreActorsAllied(CurrentDamageData.ActorDamaged.OriginalType, CActor.EType.Player);
		bool flag6 = false;
		if (playerActorToBurnCards != null)
		{
			flag6 = playerActorToBurnCards.Inventory.AllItems.Any((CItem item) => item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttacked) && !playerActorToBurnCards.Inventory.IsItemUsedOrActive(item) && item.YMLData.Data.CompareAbility != null && CurrentDamageData.DamageSourceAbility != null && item.YMLData.Data.CompareAbility.CompareAbility(CurrentDamageData.DamageSourceAbility, CurrentDamageData.ActorDamaged));
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(CurrentDamageData.ActorDamaged, CAbility.EAbilityType.PreventDamage);
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(list.FindAll((CActiveBonus it) => it is CPreventDamageActiveBonus cPreventDamageActiveBonus && it.Ability.ActiveBonusData.IsToggleBonus && !it.IsRestricted(CurrentDamageData.ActorDamaged) && (!cPreventDamageActiveBonus.PreventOnlyIfLethal || (cPreventDamageActiveBonus.PreventOnlyIfLethal && CurrentDamageData.DamageSourceStrength >= CurrentDamageData.PreDamageHealth)) && (it.Ability.ActiveBonusData.ValidAbilityTypes.Count <= 0 || it.Ability.ActiveBonusData.ValidAbilityTypes.Contains(CurrentDamageData.DamageSourceAbilityType)) && it.Ability.ActiveBonusData.Consuming.Count <= 0));
		bool flag7 = list2.Count > 0 && !CurrentDamageData.CannotPreventDamageWithActiveBonus;
		if ((num || flag2 || flag6 || flag7) && flag3 && flag4 && !flag && flag5 && CurrentDamageData.CheckIfPlayerCanAvoidDamageByBurningCards)
		{
			CurrentDamageData.ActorDamaged.Inventory.HighlightUsableItems(null, CurrentDamageData.ActorDamaged, CItem.EItemTrigger.OnAttacked);
			s_RecievedSelectedToAvoidDamageResponse = false;
			CPlayerSelectingToAvoidDamageOrNot_MessageData message = new CPlayerSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged)
			{
				m_ActorBeingAttacked = CurrentDamageData.ActorDamaged,
				m_ActorToShowCardsFor = playerActorToBurnCards,
				m_ModifiedStrength = CurrentDamageData.DamageSourceStrength,
				m_IsDirectDamage = CurrentDamageData.IsDirectDamage,
				m_TargetSummary = LastTargetSummary,
				m_DamagingAbility = CurrentDamageData.DamageSourceAbility
			};
			ScenarioRuleClient.MessageHandler(message);
			bool flag8 = false;
			while (!s_RecievedSelectedToAvoidDamageResponse && !s_ThreadAboutToAbort)
			{
				if (!flag8)
				{
					flag8 = true;
					if (Thread.CurrentThread == ScenarioRuleClient.s_WorkThread)
					{
						DLLDebug.Log("Sleeping SRL Thread" + Environment.StackTrace);
					}
				}
				ThreadIsSleeping = true;
				Thread.Sleep(100);
				ThreadIsSleeping = false;
			}
			if (s_ThreadAboutToAbort)
			{
				s_RecievedSelectedToAvoidDamageResponse = true;
				s_ThreadAboutToAbort = false;
				return;
			}
			if (PhaseManager.CurrentPhase == null)
			{
				return;
			}
			if (s_PlayerSelectedToAvoidDamage != EAvoidDamageOption.None)
			{
				if (CurrentDamageData.ActorDamaged != playerActorToBurnCards)
				{
					foreach (CActiveBonus item in list2.FindAll((CActiveBonus x) => x.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards))
					{
						item.ToggleActiveBonus(null, CurrentDamageData.ActorDamaged);
						item.TriggerPreventDamage(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
					}
				}
				SimpleLog.AddToSimpleLog(CurrentDamageData.ActorDamaged.Class.ID + " chose to avoid " + CurrentDamageData.DamageSourceStrength + " damage");
				CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false);
				CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
				SEventLogMessageHandler.AddEventLogMessage(new SEventLoseCard(CurrentDamageData.ActorDamaged.Class.ID, CurrentDamageData.DamageSourceStrength));
				if (s_PlayerSelectedToAvoidDamage == EAvoidDamageOption.Lose2DiscardCards)
				{
					SEventLogMessageHandler.AddEventLogMessage(new SEventLoseCard(CurrentDamageData.ActorDamaged.Class.ID, CurrentDamageData.DamageSourceStrength));
				}
				CPlayerSelectedToAvoidDamage_MessageData cPlayerSelectedToAvoidDamage_MessageData = new CPlayerSelectedToAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerSelectedToAvoidDamage_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				cPlayerSelectedToAvoidDamage_MessageData.m_AvoidDamageOption = s_PlayerSelectedToAvoidDamage;
				cPlayerSelectedToAvoidDamage_MessageData.m_CardsBurnedToAvoidDamage = s_CardsBurnedToAvoidDamage.ToList();
				ScenarioRuleClient.MessageHandler(cPlayerSelectedToAvoidDamage_MessageData);
			}
			else
			{
				foreach (CActiveBonus item2 in s_ToggledShieldBonuses.ToList())
				{
					item2.TriggerPreventDamage(s_RecievedDamagePreventedByActiveBonuses, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
				}
				if (LastTargetSummary != null)
				{
					CurrentDamageData.DamageShielded = LastTargetSummary.ShieldMinusPierce;
				}
				if (s_RecievedDamagedReducedByShieldItems > 0)
				{
					SimpleLog.AddToSimpleLog(CurrentDamageData.ActorDamaged.Class.ID + " shielded " + s_RecievedDamagedReducedByShieldItems + " damage");
					CurrentDamageData.ActorDamaged.Healed(s_RecievedDamagedReducedByShieldItems, ignoreTokens: true, report: false);
					CurrentDamageData.DamageShielded += s_RecievedDamagedReducedByShieldItems;
					CurrentDamageData.DamageShieldedByItems = s_RecievedDamagedReducedByShieldItems;
				}
				if (ActorHealthCheck(CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged))
				{
					CPlayerSelectedToNotAvoidDamage_MessageData cPlayerSelectedToNotAvoidDamage_MessageData = new CPlayerSelectedToNotAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
					cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
					cPlayerSelectedToNotAvoidDamage_MessageData.m_ActorOriginalHealth = CurrentDamageData.PreDamageHealth;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedToNotAvoidDamage_MessageData);
				}
			}
			if (RedirectedDamageToActor == null)
			{
				CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData = new CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				ScenarioRuleClient.MessageHandler(cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData);
			}
		}
		else if (CurrentDamageData.ActorDamaged is CPlayerActor && CurrentDamageData.CheckIfPlayerCanAvoidDamageByBurningCards)
		{
			if (ActorHealthCheck(CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged))
			{
				CPlayerSelectedToNotAvoidDamage_MessageData cPlayerSelectedToNotAvoidDamage_MessageData2 = new CPlayerSelectedToNotAvoidDamage_MessageData(CurrentDamageData.ActorDamaged);
				cPlayerSelectedToNotAvoidDamage_MessageData2.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
				cPlayerSelectedToNotAvoidDamage_MessageData2.m_ActorOriginalHealth = CurrentDamageData.PreDamageHealth;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedToNotAvoidDamage_MessageData2);
			}
			CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2 = new CPlayerFinishedSelectingToAvoidDamageOrNot_MessageData(CurrentDamageData.ActorDamaged);
			cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2.m_ActorBeingAttacked = CurrentDamageData.ActorDamaged;
			ScenarioRuleClient.MessageHandler(cPlayerFinishedSelectingToAvoidDamageOrNot_MessageData2);
		}
		else
		{
			if (flag7 && CurrentDamageData.DamageSourceStrength > 0)
			{
				list2[0].ToggleActiveBonus(null, CurrentDamageData.ActorDamaged);
				list2[0].TriggerPreventDamage(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceActor, CurrentDamageData.ActorDamaged, CurrentDamageData.DamageSourceAbility);
				SimpleLog.AddToSimpleLog(CurrentDamageData.ActorDamaged.Class.ID + " prevented " + CurrentDamageData.DamageSourceStrength + " damage");
				CurrentDamageData.ActorDamaged.Healed(CurrentDamageData.DamageSourceStrength, ignoreTokens: true, report: false);
				CurrentDamageData.DamageAvoided = CurrentDamageData.DamageSourceStrength;
			}
			if (CurrentDamageData.ActorDamaged.m_OnTakenDamageListeners != null && list2.Count <= 0)
			{
				CurrentDamageData.ActorDamaged.m_OnTakenDamageListeners?.Invoke(CurrentDamageData.DamageSourceStrength, CurrentDamageData.DamageSourceAbility, 0, CurrentDamageData.PreDamageHealth - CurrentDamageData.ActorDamaged.Health);
			}
		}
		if (s_ThreadAboutToAbort)
		{
			s_ThreadAboutToAbort = false;
		}
		else
		{
			FinishActorDamaged();
		}
	}

	public static void FinishActorDamaged()
	{
		CurrentDamageData.ActorDamaged.m_OnDamagedListeners?.Invoke(CurrentDamageData.ActorDamaged);
		int num = CurrentDamageData.PreDamageHealth - CurrentDamageData.ActorDamaged.Health;
		SimpleLog.AddToSimpleLog(CurrentDamageData.ActorDamaged.Class.ID + " takes " + num + " damage and is now at " + CurrentDamageData.ActorDamaged.Health + " health");
		CBaseCard cBaseCard = ((CurrentDamageData.DamageSourceAbility != null) ? CurrentDamageData.DamageSourceActor.FindCardWithAbility(CurrentDamageData.DamageSourceAbility) : null);
		bool isMelee = false;
		if (CurrentDamageData.DamageSourceAbility is CAbilityAttack cAbilityAttack && cAbilityAttack != null && cAbilityAttack.IsMeleeAttack)
		{
			isMelee = true;
		}
		bool isSummon = false;
		if (CurrentDamageData.ActorDamaged.OriginalType == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == CurrentDamageData.ActorDamaged.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
			if (LastTargetSummary != null)
			{
				CurrentDamageData.DamageShielded = LastTargetSummary.ShieldMinusPierce;
			}
		}
		bool actedOnSummon = false;
		CActor damageSourceActor = CurrentDamageData.DamageSourceActor;
		if (damageSourceActor != null && damageSourceActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == CurrentDamageData.DamageSourceActor.ActorGuid);
			if (cEnemyActor2 != null)
			{
				actedOnSummon = cEnemyActor2.IsSummon;
			}
		}
		int num2 = 0;
		if (LastTargetSummary != null && LastTargetSummary.Poison)
		{
			num2 = 1;
			foreach (AttackModifierYMLData usedAttackMod in LastTargetSummary.UsedAttackMods)
			{
				if (usedAttackMod.MathModifier.Contains("*2"))
				{
					num2 *= 2;
				}
				if (LastTargetSummary.UsedAttackMods[0].MathModifier.Contains("*0"))
				{
					num2 = 0;
				}
			}
		}
		if (CurrentDamageData.DamageSourceAbility is CAbilityAttack abilityAttack && num > 0)
		{
			UpdateTargetsActuallyDamagedInPrevAttackThisTurn(abilityAttack);
		}
		if (CurrentDamageData.DamageSourceAbility != null)
		{
			CurrentDamageData.DamageSourceAbility.DamageActuallyTakenByAbility = num;
			if (CurrentDamageData.ActorDamaged.Health < 0)
			{
				CurrentDamageData.DamageSourceAbility.ExcessDamageInflictedOnLastTargetKilled = Math.Abs(CurrentDamageData.ActorDamaged.Health);
			}
			else
			{
				CurrentDamageData.DamageSourceAbility.ExcessDamageInflictedOnLastTargetKilled = 0;
			}
		}
		DamageData damageData = DamageDataStack.Pop();
		string actedOnByClass = damageData.DamageSourceActor?.Class.ID;
		CActor.EType? actedOnType = damageData.DamageSourceActor?.OriginalType;
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && cAbilityCard != null && cAbilityCard.ActiveBonuses?.Count > 0 && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
		{
			actedOnByClass = cAbilityCard.ClassID;
			actedOnType = CActor.EType.Player;
		}
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
		{
			List<CPhaseAction.CPhaseAbility> previousPhaseAbilities = cPhaseAction2.PreviousPhaseAbilities;
			if (previousPhaseAbilities == null || previousPhaseAbilities.Count <= 0 || !cPhaseAction2.PreviousPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.TargetingActor?.Class?.ID == "SandDevilID"))
			{
				List<CPhaseAction.CPhaseAbility> currentPhaseAbilities = cPhaseAction2.CurrentPhaseAbilities;
				if (currentPhaseAbilities == null || currentPhaseAbilities.Count <= 0 || !cPhaseAction2.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.TargetingActor?.Class?.ID == "SandDevilID"))
				{
					goto IL_044c;
				}
			}
			actedOnByClass = "SandDevilID";
			actedOnType = CActor.EType.HeroSummon;
		}
		goto IL_044c;
		IL_044c:
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorDamaged(num, damageData.CauseOfDamage, damageData.ActorDamaged.OriginalType, damageData.ActorDamaged.ActorGuid, damageData.ActorDamaged.Class.ID, damageData.ActorDamaged.Health, damageData.ActorDamaged.Gold, damageData.ActorDamaged.XP, damageData.ActorDamaged.Level, damageData.ActorDamaged.Tokens.CheckPositiveTokens, damageData.ActorDamaged.Tokens.CheckNegativeTokens, damageData.ActorDamaged.PlayedThisRound, damageData.ActorDamaged.IsDead, damageData.ActorDamaged.CauseOfDeath, isSummon, damageData.DamageSourceActor?.ActorGuid, actedOnByClass, actedOnType, cBaseCard?.ID ?? int.MaxValue, cBaseCard?.CardType ?? CBaseCard.ECardType.None, damageData.DamageSourceAbilityType, (cBaseCard != null) ? cBaseCard.Name : "", (damageData.DamageSourceAbility != null) ? damageData.DamageSourceAbility.Strength : 0, damageData.DamageAvoided, damageData.DamageShielded, damageData.DamageShieldedByItems, isMelee, num2, damageData.Shield, actedOnSummon, damageData.DamageSourceActor?.Tokens.CheckPositiveTokens, damageData.DamageSourceActor?.Tokens.CheckNegativeTokens, "", damageData.ActorDamaged.OriginalMaxHealth, damageData.ActorDamaged.CharacterHasResourceAny("Favorite", 1)));
		if (RedirectedDamageToActor != null)
		{
			CActor item = RedirectedDamageToActor.Item1;
			int item2 = RedirectedDamageToActor.Item2;
			RedirectedDamageToActor = null;
			ActorBeenDamaged(item, item2, damageData.CheckIfPlayerCanAvoidDamageByBurningCards, damageData.DamageSourceActor, damageData.DamageSourceAbility, CAbility.EAbilityType.Damage, damageData.Shield, damageData.IsTrapDamage, damageData.IsTerrainDamage, damageData.CannotPreventDamageWithActiveBonus);
		}
	}

	public static CActionAugmentation SelectActionAugmentation(CActionAugmentation augmentation, ElementInfusionBoardManager.EElement element, bool remove, int elementIndex = 0, bool noElement = false)
	{
		if (augmentation == null || (!noElement && (elementIndex < 0 || elementIndex >= augmentation.Elements.Count)))
		{
			return null;
		}
		if (remove)
		{
			if (s_CurrentActionSelectedAugmentations.Contains(augmentation))
			{
				s_CurrentActionSelectedAugmentations.Remove(augmentation);
			}
		}
		else if (!noElement && element != augmentation.Elements[elementIndex] && augmentation.Elements[elementIndex] == ElementInfusionBoardManager.EElement.Any)
		{
			CActionAugmentation cActionAugmentation = augmentation.Copy();
			cActionAugmentation.Elements[elementIndex] = element;
			s_CurrentActionSelectedAugmentations.Remove(augmentation);
			s_CurrentActionSelectedAugmentations.Add(cActionAugmentation);
			augmentation = cActionAugmentation;
		}
		else if (!s_CurrentActionSelectedAugmentations.Contains(augmentation))
		{
			s_CurrentActionSelectedAugmentations.Add(augmentation);
		}
		return augmentation;
	}

	public static void Lose1HandCardToAvoidAttack(CPlayerActor playerActor, CAbilityCard abilityCard)
	{
		s_CardsBurnedToAvoidDamage.Add(abilityCard);
		playerActor.CharacterClass.MoveAbilityCard(abilityCard, playerActor.CharacterClass.HandAbilityCards, playerActor.CharacterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
	}

	public static void Lose2DiscardCardsToAvoidAttack(CPlayerActor playerActor, CAbilityCard abilityCard1, CAbilityCard abilityCard2)
	{
		s_CardsBurnedToAvoidDamage.Add(abilityCard1);
		s_CardsBurnedToAvoidDamage.Add(abilityCard2);
		playerActor.CharacterClass.MoveAbilityCard(abilityCard1, playerActor.CharacterClass.DiscardedAbilityCards, playerActor.CharacterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
		playerActor.CharacterClass.MoveAbilityCard(abilityCard2, playerActor.CharacterClass.DiscardedAbilityCards, playerActor.CharacterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
	}

	public static void PlayerAvoidingDamage(EAvoidDamageOption avoidDamageOption)
	{
		s_PlayerSelectedToAvoidDamage = avoidDamageOption;
		s_RecievedSelectedToAvoidDamageResponse = true;
	}

	public static void PlayerNotAvoidingDamage(int damageReducedByShieldItems, List<CActiveBonus> toggledShieldBonuses, int damagePreventedByActiveBonuses)
	{
		s_PlayerSelectedToAvoidDamage = EAvoidDamageOption.None;
		s_RecievedDamagedReducedByShieldItems = damageReducedByShieldItems;
		s_ToggledShieldBonuses = toggledShieldBonuses.ToList();
		s_RecievedDamagePreventedByActiveBonuses = damagePreventedByActiveBonuses;
		s_RecievedSelectedToAvoidDamageResponse = true;
	}

	public static void SelectedPlayerToAvoidDamage(CPlayerActor playerActor)
	{
		s_RecievedPlayerActorToAvoidDamageResponse = true;
		s_PlayerActorToAvoidDamage = playerActor;
	}

	public static bool KillActor(CActor targetingActor, CActor actor, CActor.ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, CAbility causeOfDeathAbility = null, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null)
	{
		if (actor is CEnemyActor cEnemyActor)
		{
			cEnemyActor.Type = cEnemyActor.OriginalType;
		}
		if (actor.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete: false, causeOfDeathAbility, attackSummary))
		{
			KillActorInternal(targetingActor, actor, onDeathAbility: false, actorWasAsleep);
			return true;
		}
		return false;
	}

	public static bool KillSummonOnActorDeath(CActor targetingActor, CHeroSummonActor heroSummonActor)
	{
		if (heroSummonActor.OnDeathFromSummonerDeath(targetingActor))
		{
			KillActorInternal(targetingActor, heroSummonActor);
			return true;
		}
		return false;
	}

	public static void KillActorInternal(CActor targetingActor, CActor actor, bool onDeathAbility = false, bool actorWasAsleep = false)
	{
		if (actor.Class is CMonsterClass)
		{
			s_EnemyKillTracker.Add(new KeyValuePair<CEnemyActor, CActor>((CEnemyActor)actor, targetingActor));
			ScenarioManager.Scenario.RemoveMonster((CEnemyActor)actor);
		}
		else if (actor.Class is CCharacterClass)
		{
			ScenarioManager.Scenario.RemovePlayer((CPlayerActor)actor);
		}
		else if (actor.Class is CHeroSummonClass)
		{
			ScenarioManager.Scenario.RemoveHeroSummon((CHeroSummonActor)actor);
		}
		if (actor.CarriedQuestItems.Count > 0)
		{
			for (int num = actor.CarriedQuestItems.Count - 1; num >= 0; num--)
			{
				CObjectProp questItemProp = actor.CarriedQuestItems[num];
				actor.DropQuestItem(questItemProp);
			}
			actor.CarriedQuestItems.Clear();
		}
		if (!(actor is CEnemyActor) || actor is CEnemyActor { OnDeathAbilityActorDeadHandled: false })
		{
			CActorDead_MessageData message = new CActorDead_MessageData(actor)
			{
				m_Actor = actor,
				m_OnDeathAbility = onDeathAbility,
				m_ActorWasAsleep = actorWasAsleep
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		for (int num2 = ScenarioManager.Scenario.HeroSummons.Count - 1; num2 >= 0; num2--)
		{
			CHeroSummonActor cHeroSummonActor = ScenarioManager.Scenario.HeroSummons[num2];
			if (cHeroSummonActor.Summoner == actor)
			{
				KillSummonOnActorDeath(targetingActor, cHeroSummonActor);
			}
		}
		CClass.CancelAllActiveBonusesOnDeath(actor);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	public static bool ActorHealthCheck(CActor targetingActor, CActor actor, bool isTrap = false, bool isTerrain = false, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null)
	{
		bool onDeathAbility;
		return ActorHealthCheck(targetingActor, actor, out onDeathAbility, isTrap, isTerrain, actorWasAsleep, attackSummary);
	}

	public static bool ActorHealthCheck(CActor targetingActor, CActor actor, out bool onDeathAbility, bool isTrap = false, bool isTerrain = false, bool actorWasAsleep = false, CAttackSummary.TargetSummary attackSummary = null)
	{
		onDeathAbility = false;
		if (actor.Health <= 0 && !actor.IsDead)
		{
			CActor.ECauseOfDeath causeOfDeath = (isTrap ? CActor.ECauseOfDeath.Trap : (isTerrain ? CActor.ECauseOfDeath.HazardousTerrain : CActor.ECauseOfDeath.Damage));
			onDeathAbility = !KillActor(targetingActor, actor, causeOfDeath, out var _, actor.LastAbilityDamagedBy, actorWasAsleep, attackSummary);
			return false;
		}
		return !actor.IsDead;
	}

	public static void Undo()
	{
		s_CurrentActionValidAugmentations.Clear();
		s_CurrentActionSelectedAugmentations.Clear();
		s_CurrentActor.Inventory.ClearSelectedItems();
	}

	public static void SortIntoInitiativeAndIDOrder()
	{
		if (ScenarioManager.Scenario.AllAliveMonsters != null && ScenarioManager.Scenario.AllAliveMonsters.Count > 0)
		{
			ScenarioManager.Scenario.AllAliveMonsters.Sort((CEnemyActor x, CEnemyActor y) => x.Class.ID.CompareTo(y.Class.ID));
			string classid = ScenarioManager.Scenario.AllAliveMonsters[0].Class.ID;
			int num = 0;
			for (int num2 = 0; num2 < ScenarioManager.Scenario.AllAliveMonsters.Count; num2++)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllAliveMonsters[num2];
				if (!string.Equals(cEnemyActor.Class.ID, classid))
				{
					foreach (CEnemyActor item in ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor m) => m.Class.ID == classid))
					{
						item.MonsterGroupType = num;
					}
					num = 0;
					classid = cEnemyActor.Class.ID;
				}
				if (cEnemyActor.Type == CActor.EType.Ally)
				{
					num |= 8;
				}
				if (cEnemyActor.Type == CActor.EType.Enemy)
				{
					num |= 4;
				}
				if (cEnemyActor.Type == CActor.EType.Enemy2)
				{
					num |= 2;
				}
				if (cEnemyActor.Type == CActor.EType.Neutral)
				{
					num |= 1;
				}
			}
			foreach (CEnemyActor item2 in ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor m) => m.Class.ID == classid))
			{
				item2.MonsterGroupType = num;
			}
		}
		ScenarioManager.Scenario.PlayerActors.Sort((CPlayerActor x, CPlayerActor y) => x.Initiative().CompareTo(y.Initiative()));
		ScenarioManager.Scenario.HeroSummons.Sort((CHeroSummonActor x, CHeroSummonActor y) => x.SummonedOrderIndex.CompareTo(y.SummonedOrderIndex));
		ScenarioManager.Scenario.Enemies.Sort((CEnemyActor x, CEnemyActor y) => x.Initiative().CompareTo(y.Initiative()));
		ScenarioManager.Scenario.AllyMonsters.Sort((CEnemyActor x, CEnemyActor y) => x.Initiative().CompareTo(y.Initiative()));
		ScenarioManager.Scenario.Enemy2Monsters.Sort((CEnemyActor x, CEnemyActor y) => x.Initiative().CompareTo(y.Initiative()));
		ScenarioManager.Scenario.NeutralMonsters.Sort((CEnemyActor x, CEnemyActor y) => x.Initiative().CompareTo(y.Initiative()));
		ScenarioManager.Scenario.Objects.Sort((CObjectActor x, CObjectActor y) => x.Initiative().CompareTo(y.Initiative()));
		s_InitiativeSortedActors.Clear();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			s_InitiativeSortedActors.Add(playerActor);
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (allAliveMonster.Class is CMonsterClass { RoundAbilityCard: not null })
			{
				s_InitiativeSortedActors.Add(allAliveMonster);
			}
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (@object.Class is CMonsterClass { RoundAbilityCard: not null })
			{
				s_InitiativeSortedActors.Add(@object);
			}
		}
		s_InitiativeSortedActors.Sort((CActor x, CActor y) => s_ActorInitiativeComparer.Compare(x, y));
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			int index = s_InitiativeSortedActors.IndexOf(heroSummon.Summoner);
			s_InitiativeSortedActors.Insert(index, heroSummon);
		}
	}

	public static void AddActorsIntoInitiativeOrderAfterCurrentActor(List<CActor> insertActors)
	{
		int num = s_InitiativeSortedActors.IndexOf(s_CurrentActor) + 1;
		foreach (CActor insertActor in insertActors)
		{
			s_InitiativeSortedActors.Insert(num, insertActor);
			num++;
		}
	}

	public static bool EndTurnCheckNextActorAndMoveToNextPhase()
	{
		DLLDebug.Log("GameState EndTurnCheckNextActorAndMoveToNextPhase");
		bool flag = false;
		if (s_CurrentActor.IsTakingExtraTurn)
		{
			flag = true;
			ExtraTurnActionSelectionFlagStack.Pop();
		}
		else
		{
			s_CurrentActionSelectionFlag = EActionSelectionFlag.None;
		}
		CurrentActionInitiator = EActionInitiator.None;
		s_LastActor = s_CurrentActor;
		if (s_LastActor.Class is CCharacterClass cCharacterClass && !flag)
		{
			cCharacterClass.HasLongRested = false;
			cCharacterClass.HasShortRested = false;
			cCharacterClass.HasImprovedShortRested = false;
		}
		CheckActiveBonuses();
		if (s_CurrentActor != s_LastActor && OverridingCurrentActor)
		{
			return false;
		}
		if (!s_CurrentActor.IsTakingExtraTurn)
		{
			InternalCurrentActor.ProcessConditionTokens(EConditionDecTrigger.Turns);
		}
		ElementInfusionBoardManager.EndTurn(s_CurrentActor.Type);
		if (s_CurrentActor.Type == CActor.EType.Player)
		{
			s_CurrentActor.Inventory.HighlightUsableItems(null, default(CItem.EItemTrigger));
		}
		if (!s_CurrentActor.IsTakingExtraTurn)
		{
			s_CurrentActor.PlayedThisRound = true;
		}
		EndOverrideActorForOneTurn();
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurnLoot);
		return true;
	}

	public static void MoveToNextActor()
	{
		int num = -1;
		for (int i = 0; i < s_InitiativeSortedActors.Count; i++)
		{
			if (ScenarioManager.Scenario.HasActor(s_InitiativeSortedActors[i]) && !s_InitiativeSortedActors[i].Deactivated && (!s_InitiativeSortedActors[i].PlayedThisRound || s_InitiativeSortedActors[i].HasPendingExtraTurn))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.EndRound);
			return;
		}
		s_CurrentActor = s_InitiativeSortedActors[num];
		s_TurnActor = s_CurrentActor;
		if (s_CurrentActor.HasPendingExtraTurn)
		{
			s_CurrentActor.TakingExtraTurnOfTypeStack.Push(s_CurrentActor.PendingExtraTurnOfTypeStack.Pop());
			EActionSelectionFlag item = EActionSelectionFlag.None;
			ExtraTurnActionSelectionFlagStack.Push(item);
			if (s_CurrentActor is CPlayerActor cPlayerActor)
			{
				if (s_CurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
				{
					CAbilityExtraTurn.PendingExtraTurnData pendingExtraTurnData = cPlayerActor.CharacterClass.PendingExtraTurnCardsData[0];
					cPlayerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(pendingExtraTurnData.PendingExtraTurnCards.First((CAbilityCard x) => x.Initiative == pendingExtraTurnData.LeadingInitiative));
					List<CAbilityCard> pendingExtraTurnCards = pendingExtraTurnData.PendingExtraTurnCards;
					for (int num2 = pendingExtraTurnCards.Count - 1; num2 >= 0; num2--)
					{
						CAbilityCard cAbilityCard = pendingExtraTurnCards[num2];
						if (pendingExtraTurnCards.Contains(cAbilityCard))
						{
							cPlayerActor.CharacterClass.MoveAbilityCard(cAbilityCard, pendingExtraTurnCards, cPlayerActor.CharacterClass.ExtraTurnCards, "PendingExtraTurnCards", "ExtraTurnCards");
						}
						else
						{
							DLLDebug.LogError("Card to use for Extra Turn could not be found in PendingExtraTurnCards List");
						}
					}
					cPlayerActor.CharacterClass.PendingExtraTurnCardsData.RemoveAt(0);
				}
				cPlayerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Push(cPlayerActor.CharacterClass.ExtraTurnCards.ToList());
			}
		}
		if (s_CurrentActor is CPlayerActor cPlayerActor2)
		{
			if (cPlayerActor2.SkipTopCardAction)
			{
				cPlayerActor2.SkipTopCardAction = false;
				s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, EActionSelectionFlag.TopActionPlayed);
			}
			if (cPlayerActor2.SkipBottomCardAction)
			{
				cPlayerActor2.SkipBottomCardAction = false;
				s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, EActionSelectionFlag.BottomActionPlayed);
			}
		}
		if (s_CurrentActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.PlayerControlled)
		{
			QueueOverrideActorForOneTurn(cHeroSummonActor, CActor.EType.Player, null, null, cHeroSummonActor.Summoner);
		}
		foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
		{
			s_InitiativeSortedActor.AbilityTypesPerformedThisTurn.Clear();
			s_InitiativeSortedActor.AbilityTypesPerformedThisAction.Clear();
		}
		s_DamageInflictedThisTurn = 0;
		s_ActorsMovedThisTurn.Clear();
		s_ActorsKilledThisTurn.Clear();
		s_TargetsDamagedInPrevAttackThisTurn = 0;
		s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		s_HexesMovedThisTurn.Clear();
		s_ObstaclesDestroyedThisTurn = 0;
		s_HazardousTerrainTilesMovedOverThisTurn = 0;
		s_DifficultTerrainTilesMovedOverThisTurn = 0;
		PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForForgoActionActiveBonuses);
	}

	private static void ProgressActionSelection(CAbilityCard roundAbilityCard)
	{
		bool flag = roundAbilityCard.SelectedAction.ID == roundAbilityCard.TopAction.ID || roundAbilityCard.SelectedAction.ID == roundAbilityCard.DefaultAttackAction.ID;
		if (CurrentActionSelectionSequence >= ActionSelectionSequenceType.Complete)
		{
			return;
		}
		if (s_CurrentActor.IsTakingExtraTurn)
		{
			EActionSelectionFlag eActionSelectionFlag = EActionSelectionFlag.None;
			if (ExtraTurnActionSelectionFlagStack.Count > 0)
			{
				eActionSelectionFlag = ExtraTurnActionSelectionFlagStack.Pop();
				eActionSelectionFlag = SetFlag(eActionSelectionFlag, flag ? EActionSelectionFlag.TopActionPlayed : EActionSelectionFlag.BottomActionPlayed);
				ExtraTurnActionSelectionFlagStack.Push(eActionSelectionFlag);
			}
		}
		else
		{
			s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, flag ? EActionSelectionFlag.TopActionPlayed : EActionSelectionFlag.BottomActionPlayed);
		}
	}

	public static void SetActionFlag(EActionSelectionFlag actionSelectionFlag)
	{
		if (s_CurrentActor.IsTakingExtraTurn)
		{
			EActionSelectionFlag a = ExtraTurnActionSelectionFlagStack.Pop();
			a = SetFlag(a, actionSelectionFlag);
			ExtraTurnActionSelectionFlagStack.Push(a);
		}
		else
		{
			s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, actionSelectionFlag);
		}
	}

	public static void UpdateCacheActionSelectionSequence(EActionSelectionFlag actionSelectionFlag)
	{
		s_CachedActionSelectionFlag = s_CurrentActionSelectionFlag;
		s_CachedActionSelectionFlag = SetFlag(s_CachedActionSelectionFlag, actionSelectionFlag);
	}

	public static void RestoreCacheActionSelectionSequence()
	{
		s_CurrentActionSelectionFlag = s_CachedActionSelectionFlag;
	}

	public static void NextPhase()
	{
		SimpleLog.AddToSimpleLog("GameState Next Phase called, current phase: " + PhaseManager.PhaseType);
		switch (PhaseManager.PhaseType)
		{
		case CPhase.PhaseType.StartScenarioEffects:
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.StartRoundEffects);
			}
			break;
		case CPhase.PhaseType.StartRoundEffects:
			foreach (CActiveBonus item in CActiveBonus.FindAllActiveBonuses())
			{
				item.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerRound);
			}
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner.OnStartRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventRound(ScenarioManager.Scenario.AllEnemyMonsters.Count + ScenarioManager.Scenario.AllEnemy2Monsters.Count));
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor.CharacterClass.ResetInitiativeBonus();
			}
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.PlayerExhausted);
			}
			break;
		case CPhase.PhaseType.PlayerExhausted:
			if (PendingScenarioModifierAbilities.Count > 0 || PendingActiveBonuses.Count > 0)
			{
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.Autosave);
			}
			break;
		case CPhase.PhaseType.Autosave:
			PhaseManager.SetNextPhase(CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			break;
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			MonsterClassManager.SelectRoundAbilityCards();
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor2.CharacterClass.ImprovedShortRest = false;
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.MonsterClassesSelectAbilityCards);
			break;
		case CPhase.PhaseType.MonsterClassesSelectAbilityCards:
			s_CurrentActionSelectionFlag = EActionSelectionFlag.None;
			RoundAbilityCardselected = null;
			SortIntoInitiativeAndIDOrder();
			foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
			{
				s_InitiativeSortedActor.PlayedThisRound = false;
				s_InitiativeSortedActor.ActorsAttackedThisRound.Clear();
				s_InitiativeSortedActor.AbilityTypesPerformedThisTurn.Clear();
				s_InitiativeSortedActor.AbilityTypesPerformedThisAction.Clear();
			}
			s_DamageInflictedThisTurn = 0;
			s_ActorsMovedThisTurn.Clear();
			s_ActorsKilledThisTurn.Clear();
			s_ActorsKilledThisRound.Clear();
			s_TargetsDamagedInPrevAttackThisTurn = 0;
			s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
			s_HexesMovedThisTurn.Clear();
			s_ObstaclesDestroyedThisTurn = 0;
			s_HazardousTerrainTilesMovedOverThisTurn = 0;
			s_DifficultTerrainTilesMovedOverThisTurn = 0;
			s_CheckAdjustInitiativeIndex = 0;
			PreInitiativeAdjustedPlayerActors = ScenarioManager.Scenario.PlayerActors.ToList();
			s_CurrentActor = ScenarioManager.Scenario.PlayerActors[s_CheckAdjustInitiativeIndex++];
			PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForInitiativeAdjustments);
			break;
		case CPhase.PhaseType.CheckForInitiativeAdjustments:
		{
			CEndInitiativeAdjustments_MessageData message = new CEndInitiativeAdjustments_MessageData(InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message);
			if (s_CheckAdjustInitiativeIndex < PreInitiativeAdjustedPlayerActors.Count)
			{
				s_CurrentActor = PreInitiativeAdjustedPlayerActors[s_CheckAdjustInitiativeIndex++];
				PhaseManager.SetNextPhase(CPhase.PhaseType.CheckForInitiativeAdjustments);
				break;
			}
			SortIntoInitiativeAndIDOrder();
			foreach (CPlayerActor playerActor3 in ScenarioManager.Scenario.PlayerActors)
			{
				foreach (CAdjustInitiativeActiveBonus item2 in playerActor3.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative))
				{
					item2.IsInitiativeAdjusted();
				}
			}
			MoveToNextActor();
			break;
		}
		case CPhase.PhaseType.CheckForForgoActionActiveBonuses:
			if (s_CurrentActor is CHeroSummonActor { IsCompanionSummon: not false, Summoner: var summoner })
			{
				List<CActiveBonus> list3 = summoner.FindApplicableActiveBonuses(CAbility.EAbilityType.ForgoActionsForCompanion);
				if (list3.Count > 0 && !summoner.CharacterClass.LongRest)
				{
					foreach (CForgoActionsForCompanionActiveBonus item3 in list3)
					{
						item3.ResetToggles();
					}
					CShowForgoActiveBonusBar_MessageData cShowForgoActiveBonusBar_MessageData = new CShowForgoActiveBonusBar_MessageData(summoner);
					cShowForgoActiveBonusBar_MessageData.m_Ability = list3[0].Ability;
					ScenarioRuleClient.MessageHandler(cShowForgoActiveBonusBar_MessageData);
				}
				else
				{
					PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
				}
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
			}
			break;
		case CPhase.PhaseType.StartTurn:
		{
			OverrideActorForOneTurn(s_CurrentActor);
			CActor cActor = s_CurrentActor;
			bool actorWasAsleep = cActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			if (cActor.Tokens.HasKey(CCondition.ENegativeCondition.Wound) && !cActor.IsDead && !cActor.IsTakingExtraTurn && !cActor.HasTakenWoundDamageThisTurn)
			{
				int health = cActor.Health;
				ActorBeenDamaged(cActor, 1, checkIfPlayerCanAvoidDamage: true, cActor.WoundedBy, null, CAbility.EAbilityType.Wound);
				cActor.HasTakenWoundDamageThisTurn = true;
				CWoundTriggered_MessageData message2 = new CWoundTriggered_MessageData(cActor)
				{
					m_WoundedActor = cActor,
					m_ActorOriginalHealth = health,
					m_ActorWasAsleep = actorWasAsleep
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			foreach (CSpawner spawner2 in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner2.OnStartTurn(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			if (!ActorHealthCheck(cActor, cActor, isTrap: false, isTerrain: false, actorWasAsleep) && s_CurrentActor == cActor)
			{
				if (ScenarioManager.Scenario.PlayerActors.Count > 0 && s_CurrentActor == cActor && PhaseManager.PhaseType == CPhase.PhaseType.StartTurn)
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
			}
			else if (PhaseManager.PhaseType == CPhase.PhaseType.StartTurn)
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			}
			break;
		}
		case CPhase.PhaseType.ActionSelection:
			if (CurrentAction == null)
			{
				if (!(s_CurrentActor is CPlayerActor cPlayerActor) || !cPlayerActor.CharacterClass.LongRest || cPlayerActor.CharacterClass.HasLongRested)
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
				break;
			}
			if ((CurrentAction.Action.Augmentations != null && CurrentAction.Action.Augmentations.Count > 0 && !s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun)) || s_CurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
				List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
				if (s_CurrentActor.IsOriginalMonsterType)
				{
					list = ElementInfusionBoardManager.GetEnemyConsumedElements(s_CurrentActor.ActorGuid);
				}
				foreach (CActionAugmentation augmentation in CurrentAction.Action.Augmentations)
				{
					bool flag = false;
					if (s_CurrentActor.IsOriginalMonsterType && list != null && list.Count > 0)
					{
						foreach (ElementInfusionBoardManager.EElement item4 in augmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag = list.Contains(item4);
							if (!flag)
							{
								break;
							}
						}
						if (augmentation.Elements.Contains(ElementInfusionBoardManager.EElement.Any))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						List<ElementInfusionBoardManager.EElement> list2 = availableElements;
						bool flag2 = true;
						foreach (ElementInfusionBoardManager.EElement item5 in augmentation.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
						{
							flag2 = list2.Remove(item5);
							if (!flag2)
							{
								break;
							}
						}
						if (augmentation.Elements.Count((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any) > list2.Count)
						{
							flag2 = false;
						}
						if (flag2)
						{
							s_CurrentActionValidAugmentations.Add(augmentation);
							if (s_CurrentActor.IsOriginalMonsterType)
							{
								(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation);
							}
						}
					}
					else
					{
						s_CurrentActionValidAugmentations.Add(augmentation);
						if (s_CurrentActor.IsOriginalMonsterType)
						{
							(s_CurrentActor as CEnemyActor).AbilitySelectionAddConsume(augmentation);
						}
					}
				}
			}
			if (s_CurrentActionValidAugmentations.Count > 0 && s_CurrentActor.IsOriginalMonsterType)
			{
				s_CurrentActionSelectedAugmentations.AddRange(s_CurrentActionValidAugmentations);
			}
			if (s_CurrentActor.Class is CCharacterClass cCharacterClass2 && cCharacterClass2.RoundAbilityCards.Count == 2)
			{
				cCharacterClass2.RoundCardsSelectedInCardSelection = cCharacterClass2.RoundAbilityCards.ToList();
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.Action);
			break;
		case CPhase.PhaseType.Action:
			if (s_CurrentActor.Class is CCharacterClass)
			{
				CCharacterClass cCharacterClass = s_CurrentActor.Class as CCharacterClass;
				if (cCharacterClass.RoundAbilityCards.Count == 2)
				{
					cCharacterClass.RoundCardsSelectedInCardSelection = cCharacterClass.RoundAbilityCards.ToList();
				}
			}
			s_CurrentActionValidAugmentations.Clear();
			s_CurrentActionSelectedAugmentations.Clear();
			s_RecievedDamagedReducedByShieldItems = 0;
			if (OverridingCurrentActor)
			{
				EndOverrideCurrentActorForOneAction();
			}
			if (s_CurrentActor.Class is CMonsterClass || s_CurrentActor.Class is CHeroSummonClass || !ScenarioManager.Scenario.HasActor(s_CurrentActor))
			{
				if (s_CurrentActor.Class is CCharacterClass)
				{
					if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null)
					{
						ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
					}
					else
					{
						EndTurnCheckNextActorAndMoveToNextPhase();
					}
				}
				else
				{
					EndTurnCheckNextActorAndMoveToNextPhase();
				}
				break;
			}
			if (CurrentActionInitiator == EActionInitiator.AbilityCard)
			{
				ProgressActionSelection(RoundAbilityCardselected);
			}
			if (s_CurrentActor != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
			{
				if (s_CurrentActor.IsTakingExtraTurn)
				{
					((CPlayerActor)s_CurrentActor).CharacterClass.DiscardExtraTurnAbilityCard(RoundAbilityCardselected);
				}
				else
				{
					((CPlayerActor)s_CurrentActor).CharacterClass.DiscardRoundAbilityCard(RoundAbilityCardselected);
				}
				CClass.MarkAbilityActiveBonusesFinished(s_CurrentActor);
			}
			PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			break;
		case CPhase.PhaseType.EndTurnLoot:
			PhaseManager.SetNextPhase(CPhase.PhaseType.EndTurn);
			break;
		case CPhase.PhaseType.EndTurn:
			foreach (CPlayerActor playerActor4 in ScenarioManager.Scenario.PlayerActors)
			{
				CClass.MarkTurnActiveBonusesFinished(playerActor4);
			}
			foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
			{
				CClass.MarkTurnActiveBonusesFinished(exhaustedPlayer);
			}
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				CClass.MarkTurnActiveBonusesFinished(enemy);
			}
			foreach (CEnemyActor deadEnemy in ScenarioManager.Scenario.DeadEnemies)
			{
				CClass.MarkTurnActiveBonusesFinished(deadEnemy);
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				CClass.MarkTurnActiveBonusesFinished(heroSummon);
			}
			foreach (CHeroSummonActor deadHeroSummon in ScenarioManager.Scenario.DeadHeroSummons)
			{
				CClass.MarkTurnActiveBonusesFinished(deadHeroSummon);
			}
			foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
			{
				CClass.MarkTurnActiveBonusesFinished(enemy2Monster);
			}
			foreach (CEnemyActor deadEnemy2Monster in ScenarioManager.Scenario.DeadEnemy2Monsters)
			{
				CClass.MarkTurnActiveBonusesFinished(deadEnemy2Monster);
			}
			foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
			{
				CClass.MarkTurnActiveBonusesFinished(allyMonster);
			}
			foreach (CEnemyActor deadAllyMonster in ScenarioManager.Scenario.DeadAllyMonsters)
			{
				CClass.MarkTurnActiveBonusesFinished(deadAllyMonster);
			}
			foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
			{
				CClass.MarkTurnActiveBonusesFinished(neutralMonster);
			}
			foreach (CEnemyActor deadNeutralMonster in ScenarioManager.Scenario.DeadNeutralMonsters)
			{
				CClass.MarkTurnActiveBonusesFinished(deadNeutralMonster);
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				CClass.MarkTurnActiveBonusesFinished(@object);
			}
			foreach (CObjectActor deadObject in ScenarioManager.Scenario.DeadObjects)
			{
				CClass.MarkTurnActiveBonusesFinished(deadObject);
			}
			foreach (CPlayerActor playerActor5 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor5.ClearCharacterSpecialMechanicsCache(clearAugments: false, clearSongs: false, clearDooms: true);
			}
			if (s_CurrentActor is CPlayerActor player)
			{
				ReportAdjacency(player);
			}
			if (s_CurrentActor.IsTakingExtraTurn)
			{
				if (s_CurrentActor is CPlayerActor cPlayerActor2)
				{
					CAbilityExtraTurn.EExtraTurnType takingExtraTurnOfType = s_CurrentActor.TakingExtraTurnOfType;
					EndExtraTurn();
					cPlayerActor2.CharacterClass.DiscardRoundAbilityCards(extraTurn: true);
					if (takingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
					{
						MoveToNextActor();
					}
				}
				else
				{
					s_CurrentActor.TakingExtraTurnOfTypeStack.Pop();
					MoveToNextActor();
				}
				break;
			}
			if (s_CurrentActor is CPlayerActor cPlayerActor3)
			{
				cPlayerActor3.CharacterClass.DiscardRoundAbilityCards();
			}
			foreach (CSpawner spawner3 in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner3.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner3.OnEndTurn(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			MoveToNextActor();
			break;
		case CPhase.PhaseType.EndRound:
		{
			CActor.EType lastActorType = ((s_CurrentActor != null) ? s_CurrentActor.Type : ((s_LastActor != null) ? s_LastActor.Type : CActor.EType.Player));
			s_LastActor = null;
			s_CurrentActor = null;
			s_TurnActor = null;
			foreach (CActor key in s_ActorsToOverrideTurns.Keys)
			{
				key.MindControlDuration = CAbilityControlActor.EControlDurationType.None;
			}
			s_ActorsToOverrideTurns.Clear();
			foreach (CPlayerActor playerActor6 in ScenarioManager.Scenario.PlayerActors)
			{
				CClass.MarkRoundActiveBonusesFinished(playerActor6);
			}
			foreach (CPlayerActor exhaustedPlayer2 in ScenarioManager.Scenario.ExhaustedPlayers)
			{
				CClass.MarkRoundActiveBonusesFinished(exhaustedPlayer2);
			}
			foreach (CEnemyActor enemy2 in ScenarioManager.Scenario.Enemies)
			{
				CClass.MarkRoundActiveBonusesFinished(enemy2);
			}
			foreach (CEnemyActor deadEnemy2 in ScenarioManager.Scenario.DeadEnemies)
			{
				CClass.MarkRoundActiveBonusesFinished(deadEnemy2);
			}
			foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
			{
				CClass.MarkRoundActiveBonusesFinished(heroSummon2);
			}
			foreach (CHeroSummonActor deadHeroSummon2 in ScenarioManager.Scenario.DeadHeroSummons)
			{
				CClass.MarkRoundActiveBonusesFinished(deadHeroSummon2);
			}
			foreach (CEnemyActor enemy2Monster2 in ScenarioManager.Scenario.Enemy2Monsters)
			{
				CClass.MarkRoundActiveBonusesFinished(enemy2Monster2);
			}
			foreach (CEnemyActor deadEnemy2Monster2 in ScenarioManager.Scenario.DeadEnemy2Monsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadEnemy2Monster2);
			}
			foreach (CEnemyActor allyMonster2 in ScenarioManager.Scenario.AllyMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(allyMonster2);
			}
			foreach (CEnemyActor deadAllyMonster2 in ScenarioManager.Scenario.DeadAllyMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadAllyMonster2);
			}
			foreach (CEnemyActor neutralMonster2 in ScenarioManager.Scenario.NeutralMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(neutralMonster2);
			}
			foreach (CEnemyActor deadNeutralMonster2 in ScenarioManager.Scenario.DeadNeutralMonsters)
			{
				CClass.MarkRoundActiveBonusesFinished(deadNeutralMonster2);
			}
			foreach (CObjectActor object2 in ScenarioManager.Scenario.Objects)
			{
				CClass.MarkRoundActiveBonusesFinished(object2);
			}
			foreach (CObjectActor deadObject2 in ScenarioManager.Scenario.DeadObjects)
			{
				CClass.MarkRoundActiveBonusesFinished(deadObject2);
			}
			foreach (CPlayerActor playerActor7 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor7.CharacterClass.CheckForFinishedActiveBonuses(playerActor7);
			}
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
			foreach (CPlayerActor playerActor8 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor8.CharacterClass.CheckAttackModifierCardShuffle();
			}
			foreach (CPlayerActor playerActor9 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor9.CharacterClass.RoundCardsSelectedInCardSelection.Clear();
				playerActor9.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Clear();
			}
			MonsterClassManager.DiscardRoundAbilityCards();
			MonsterClassManager.CheckAllAttackModifierDecksForShuffle();
			foreach (CSpawner spawner4 in ScenarioManager.CurrentScenarioState.Spawners)
			{
				if (spawner4.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					spawner4.OnEndRound(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
				}
			}
			ElementInfusionBoardManager.EndRound(lastActorType);
			ScenarioManager.CurrentScenarioState.RoundNumber++;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			NextRound();
			break;
		}
		case CPhase.PhaseType.Count:
		case CPhase.PhaseType.None:
			break;
		}
	}

	public static void ReportAdjacency(CPlayerActor player)
	{
		if (player != null)
		{
			_ = ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y];
		}
		GetAdjacency(player, out var wall, out var obstacles, out var allies, out var enemies);
		int x = player.ArrayIndex.X;
		int y = player.ArrayIndex.Y;
		CActor.EType originalType = player.OriginalType;
		string actorGuid = player.ActorGuid;
		string iD = player.Class.ID;
		int health = player.Health;
		int gold = player.Gold;
		int xP = player.XP;
		int level = player.Level;
		List<PositiveConditionPair> checkPositiveTokens = player.Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = player.Tokens.CheckNegativeTokens;
		bool playedThisRound = player.PlayedThisRound;
		bool isDead = player.IsDead;
		CActor.ECauseOfDeath causeOfDeath = player.CauseOfDeath;
		int originalMaxHealth = player.OriginalMaxHealth;
		int allyAdjacent = allies;
		int enemyAdjacent = enemies;
		int obstacleAdjacent = obstacles;
		bool wallAdjacent = wall;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorEndTurn(x, y, originalType, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", originalMaxHealth, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent));
	}

	public static void GetAdjacency(CActor player, out bool wall, out int obstacles, out int allies, out int enemies)
	{
		wall = false;
		obstacles = (allies = (enemies = 0));
		if (player != null)
		{
			wall = CAbilityFilter.WallIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y]);
			obstacles = CAbilityFilter.ObstacleIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y]);
			allies = CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y], player, CActor.EType.Ally);
			enemies = CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[player.ArrayIndex.X, player.ArrayIndex.Y], player, CActor.EType.Enemy);
		}
	}

	public static List<CActor> GetAdjacenctEnemies(CActor player, Point arrayIndex)
	{
		if (player != null)
		{
			CAbilityFilter.ActorIsAdjacent(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], player, CActor.EType.Enemy, out var adjacent);
			return adjacent;
		}
		return new List<CActor>();
	}

	public static bool LostAdjacency(CActor actor, Point firstPoint, Point secondPoint)
	{
		if (actor != null && actor is CPlayerActor && firstPoint != secondPoint)
		{
			bool num = GetAdjacenctEnemies(actor, firstPoint).Count > 0;
			if (num)
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventLostAdjacency(actor.Class.ID));
			}
			return num;
		}
		return false;
	}

	public static bool LostAdjacency(CActor actor, CTile firstTile, CTile secondTile)
	{
		if (actor != null && firstTile != null && secondTile != null)
		{
			return LostAdjacency(actor, new Point(firstTile), new Point(secondTile));
		}
		return false;
	}

	public static void CheckActiveBonuses()
	{
		try
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor.CharacterClass.CheckForFinishedActiveBonuses(playerActor);
			}
			foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
			{
				exhaustedPlayer.CharacterClass.CheckForFinishedActiveBonuses(exhaustedPlayer);
			}
		}
		catch (InvalidOperationException)
		{
			DLLDebug.Log("Caught InvalidOperationException at GameState.CheckActiveBonuses - Sleeping Thread" + Environment.StackTrace);
			Thread.Sleep(10);
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor2.CharacterClass.CheckForFinishedActiveBonuses(playerActor2);
			}
			foreach (CPlayerActor exhaustedPlayer2 in ScenarioManager.Scenario.ExhaustedPlayers)
			{
				exhaustedPlayer2.CharacterClass.CheckForFinishedActiveBonuses(exhaustedPlayer2);
			}
		}
	}

	public static void CheckAllPressurePlates()
	{
		List<Tuple<CObjectPressurePlate, CActor>> list = new List<Tuple<CObjectPressurePlate, CActor>>();
		List<CObjectPressurePlate> list2 = new List<CObjectPressurePlate>();
		foreach (CObjectPressurePlate item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectPressurePlate>().ToList())
		{
			if (item.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				CTile cTile = ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y];
				CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
				if (cActor != null && cActor.Class is CCharacterClass)
				{
					list.Add(new Tuple<CObjectPressurePlate, CActor>(item, cActor));
				}
				else
				{
					list2.Add(item);
				}
			}
		}
		foreach (Tuple<CObjectPressurePlate, CActor> item2 in list)
		{
			item2.Item1.Activate(item2.Item2);
		}
		foreach (CObjectPressurePlate item3 in list2)
		{
			item3.Deactivate();
		}
	}

	public static void PlayerLongRested(CAbilityCard abilityCard, CPlayerActor playerActor = null, bool improvedShortRestUsed = false, bool fromStateUpdate = false)
	{
		if (playerActor == null)
		{
			playerActor = (CPlayerActor)s_CurrentActor;
		}
		CCharacterClass characterClass = playerActor.CharacterClass;
		if (!improvedShortRestUsed)
		{
			s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, EActionSelectionFlag.None);
			characterClass.HasLongRested = true;
		}
		else
		{
			characterClass.HasShortRested = true;
			characterClass.HasImprovedShortRested = true;
		}
		int health = playerActor.Health;
		if (abilityCard != null && !fromStateUpdate)
		{
			playerActor.CharacterClass.MoveAbilityCard(abilityCard, characterClass.DiscardedAbilityCards, characterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
			while (characterClass.DiscardedAbilityCards.Count > 0)
			{
				playerActor.CharacterClass.MoveAbilityCard(characterClass.DiscardedAbilityCards[0], characterClass.DiscardedAbilityCards, characterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
			}
		}
		playerActor.Healed(2, ignoreTokens: false, report: true, actualHeal: true);
		playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Spent);
		playerActor.Inventory.RefreshItems(CItem.EItemSlotState.Locked, CItem.EItemSlot.None, CItem.EUsageType.Spent);
		bool improvedShortRest = playerActor.CharacterClass.ImprovedShortRest;
		if (!improvedShortRest)
		{
			playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
		}
		playerActor.CharacterClass.LongRest = false;
		playerActor.CharacterClass.ImprovedShortRest = false;
		if (improvedShortRest)
		{
			playerActor.CharacterClass.ShortRestCardBurned = abilityCard;
			playerActor.Inventory.HighlightUsableItems(null, default(CItem.EItemTrigger));
			ScenarioRuleClient.MessageHandler(new CPlayerImprovedShortRested_MessageData(health, abilityCard, playerActor));
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, null, null, playerActor?.Type, IsSummon: false, playerActor?.Tokens.CheckPositiveTokens, playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
		}
		else
		{
			playerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.AtEndOfTurn, CItem.EItemTrigger.DuringOwnTurn);
			ScenarioRuleClient.MessageHandler(new CPlayerLongRested_MessageData(health, abilityCard, playerActor));
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(CAbility.EAbilityType.LongRest, ESESubTypeAbility.AbilityEnded, null, int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, null, null, playerActor?.Type, IsSummon: false, playerActor?.Tokens.CheckPositiveTokens, playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
		}
		PendingOnLongRestBonuses.AddRange(playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnLongRest));
	}

	public static void TriggerAnyOnLongRestAddActiveBonuses()
	{
		if (PendingOnLongRestBonuses.Count <= 0)
		{
			return;
		}
		foreach (CActiveBonus pendingOnLongRestBonuse in PendingOnLongRestBonuses)
		{
			if (pendingOnLongRestBonuse.BespokeBehaviour is CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest cDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest)
			{
				cDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest.TriggerAbility();
			}
		}
	}

	public static void PlayerShortRested(CPlayerActor playerActor, CAbilityCard discardedAbilityCard, bool loseHealth, bool updateScenarioRNG, bool fromStateUpdate = false)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		if (characterClass.HasShortRested && fromStateUpdate)
		{
			SimpleLog.AddToSimpleLog("Returning early from short rest triggered by a state update as " + playerActor.ActorLocKey() + " has already short rested");
			return;
		}
		characterClass.HasShortRested = true;
		characterClass.ShortRestCardBurned = discardedAbilityCard;
		characterClass.ShortRestCardRedrawn = loseHealth;
		if (!fromStateUpdate)
		{
			playerActor.CharacterClass.MoveAbilityCard(discardedAbilityCard, characterClass.DiscardedAbilityCards, characterClass.LostAbilityCards, "DiscardedAbilityCards", "LostAbilityCards");
			for (int num = characterClass.DiscardedAbilityCards.Count - 1; num >= 0; num--)
			{
				playerActor.CharacterClass.MoveAbilityCard(characterClass.DiscardedAbilityCards[num], characterClass.DiscardedAbilityCards, characterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
			}
		}
		if (updateScenarioRNG)
		{
			ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
			if (loseHealth)
			{
				ScenarioManager.CurrentScenarioState.ScenarioRNG.Next();
			}
		}
		if (loseHealth)
		{
			int health = playerActor.Health;
			playerActor.Damaged(1, fromAttackAbility: false, null, null);
			playerActor.m_OnDamagedListeners?.Invoke(playerActor);
			playerActor.m_OnTakenDamageListeners?.Invoke(1, null, 0, 1);
			int damageTaken = health - playerActor.Health;
			string actorGuid = playerActor.ActorGuid;
			string iD = playerActor.Class.ID;
			int health2 = playerActor.Health;
			int gold = playerActor.Gold;
			int xP = playerActor.XP;
			int level = playerActor.Level;
			List<PositiveConditionPair> checkPositiveTokens = playerActor.Tokens.CheckPositiveTokens;
			List<NegativeConditionPair> checkNegativeTokens = playerActor.Tokens.CheckNegativeTokens;
			bool playedThisRound = playerActor.PlayedThisRound;
			bool isDead = playerActor.IsDead;
			CActor.ECauseOfDeath causeOfDeath = playerActor.CauseOfDeath;
			int originalMaxHealth = playerActor.OriginalMaxHealth;
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorDamaged(damageTaken, CActor.ECauseOfDamage.Self, CActor.EType.Player, actorGuid, iD, health2, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, 0, 0, 0, isMelee: false, 0, 0, actedOnSummon: false, null, null, "", originalMaxHealth));
		}
		CPlayerShortRested_MessageData message = new CPlayerShortRested_MessageData(playerActor, discardedAbilityCard, playerActor);
		ScenarioRuleClient.MessageHandler(message);
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(CAbility.EAbilityType.ShortRest, ESESubTypeAbility.AbilityEnded, null, int.MaxValue, CBaseCard.ECardType.None, playerActor.Class.ID, 0, null, null, playerActor?.Type, IsSummon: false, playerActor?.Tokens.CheckPositiveTokens, playerActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
		if (playerActor.CharacterClass.HandAbilityCards.Count < 2 && playerActor.CharacterClass.DiscardedAbilityCards.Count < 2)
		{
			bool actorWasAsleep = playerActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			CClass.CancelAllActiveBonusesOnDeath(playerActor);
			if (!fromStateUpdate)
			{
				CReplicateStartRoundCardState_MessageData message2 = new CReplicateStartRoundCardState_MessageData(playerActor, 51, playerActor.CharacterClass.GetCurrentCardState());
				ScenarioRuleClient.MessageHandler(message2);
			}
			KillActor(playerActor, playerActor, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
			CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(playerActor);
			cPlayersExhausted_MessageData.m_Players = new List<CPlayerActor> { playerActor };
			ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
		}
		else if (!fromStateUpdate)
		{
			CReplicateStartRoundCardState_MessageData message3 = new CReplicateStartRoundCardState_MessageData(playerActor, 51, playerActor.CharacterClass.GetCurrentCardState());
			ScenarioRuleClient.MessageHandler(message3);
		}
	}

	public static void PlayerSelectedCardsToRecover(CPlayerActor playerActor, List<CAbilityCard> recoveredAbilityCards, CBaseCard.ECardPile recoveredCardPile)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard recoveredAbilityCard in recoveredAbilityCards)
		{
			if (characterClass.DiscardedAbilityCards.Contains(recoveredAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(recoveredAbilityCard, characterClass.DiscardedAbilityCards, characterClass.HandAbilityCards, "DiscardedAbilityCards", "HandAbilityCards");
			}
			else if (characterClass.LostAbilityCards.Contains(recoveredAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(recoveredAbilityCard, characterClass.LostAbilityCards, characterClass.HandAbilityCards, "LostAbilityCards", "HandAbilityCards");
			}
			else
			{
				DLLDebug.LogError("Card to recover could not be found in DiscardedAbilityCards or LostAbilityCards Lists");
			}
		}
		if (recoveredCardPile != CBaseCard.ECardPile.None)
		{
			CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
			cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
			cMoveSelectedCards.m_MoveFromPile = recoveredCardPile;
			cMoveSelectedCards.m_quantity = recoveredAbilityCards.Count;
			cMoveSelectedCards.m_Cards = recoveredAbilityCards;
			ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
		}
	}

	public static void PlayerSelectedCardsToLose(CPlayerActor playerActor, List<CAbilityCard> losingAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard losingAbilityCard in losingAbilityCards)
		{
			if (characterClass.HandAbilityCards.Contains(losingAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(losingAbilityCard, characterClass.HandAbilityCards, characterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
			}
			else
			{
				DLLDebug.LogError("Card to lose could not be found in HandAbilityCards Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.Hand;
		cMoveSelectedCards.m_MoveToPile = CBaseCard.ECardPile.Lost;
		cMoveSelectedCards.m_quantity = losingAbilityCards.Count;
		cMoveSelectedCards.m_Cards = losingAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedCardsToDiscard(CPlayerActor playerActor, List<CAbilityCard> losingAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard losingAbilityCard in losingAbilityCards)
		{
			if (characterClass.HandAbilityCards.Contains(losingAbilityCard))
			{
				playerActor.CharacterClass.MoveAbilityCard(losingAbilityCard, characterClass.HandAbilityCards, characterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
			}
			else
			{
				DLLDebug.LogError("Card to lose could not be found in HandAbilityCards Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.Hand;
		cMoveSelectedCards.m_MoveToPile = CBaseCard.ECardPile.Discarded;
		cMoveSelectedCards.m_quantity = losingAbilityCards.Count;
		cMoveSelectedCards.m_Cards = losingAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedCardsToIncreaseLimit(CPlayerActor playerActor, List<CAbilityCard> increasingLimitAbilityCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		foreach (CAbilityCard increasingLimitAbilityCard in increasingLimitAbilityCards)
		{
			if (characterClass.AbilityCardsPool.Contains(increasingLimitAbilityCard))
			{
				playerActor.CharacterClass.SelectedAbilityCards.Add(increasingLimitAbilityCard);
				playerActor.CharacterClass.HandAbilityCards.Add(increasingLimitAbilityCard);
			}
			else
			{
				DLLDebug.LogError("Card to increase limit by could not be found in AbilityCardsPool Lists");
			}
		}
		CMoveSelectedCards cMoveSelectedCards = new CMoveSelectedCards(playerActor);
		cMoveSelectedCards.m_ActorRecoveringCards = playerActor;
		cMoveSelectedCards.m_MoveFromPile = CBaseCard.ECardPile.None;
		cMoveSelectedCards.m_quantity = increasingLimitAbilityCards.Count;
		cMoveSelectedCards.m_Cards = increasingLimitAbilityCards;
		ScenarioRuleClient.MessageHandler(cMoveSelectedCards);
	}

	public static void PlayerSelectedExtraTurnCards(CPlayerActor playerActor, List<CAbilityCard> extraTurnCards)
	{
		CCharacterClass characterClass = playerActor.CharacterClass;
		List<CAbilityCard> list = null;
		string text = null;
		if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			list = new List<CAbilityCard>();
			text = "PendingExtraTurnCards";
			characterClass.PendingExtraTurnCardsData.Add(new CAbilityExtraTurn.PendingExtraTurnData(characterClass.ExtraTurnInitiativeAbilityCard.Initiative, list));
			characterClass.PendingExtraTurnCardsData = characterClass.PendingExtraTurnCardsData.OrderBy((CAbilityExtraTurn.PendingExtraTurnData x) => x.LeadingInitiative).ToList();
		}
		else
		{
			list = characterClass.ExtraTurnCards;
			text = "ExtraTurnCards";
		}
		list.Clear();
		foreach (CAbilityCard extraTurnCard in extraTurnCards)
		{
			if (characterClass.AbilityCardsPool.Contains(extraTurnCard) || characterClass.GivenCards.Contains(extraTurnCard))
			{
				SimpleLog.AddToSimpleLog(playerActor.Class.ID + " chose " + extraTurnCard.Name + " (Intiative " + extraTurnCard.Initiative + ") for extra turn");
				playerActor.CharacterClass.MoveAbilityCard(extraTurnCard, characterClass.HandAbilityCards, list, "HandAbilityCards", text);
			}
			else
			{
				DLLDebug.LogError("Card to use for Extra Turn could not be found in AbilityCardsPool Lists");
			}
		}
		if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (!playerActor.HasPendingExtraTurn && !playerActor.IsTakingExtraTurn)
			{
				s_InitiativeSortedActors.Remove(playerActor);
				bool flag = false;
				foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
				{
					if (s_ActorInitiativeComparer.Compare(playerActor, s_InitiativeSortedActor) < 0)
					{
						int index = s_InitiativeSortedActors.IndexOf(s_InitiativeSortedActor);
						s_InitiativeSortedActors.Insert(index, playerActor);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					s_InitiativeSortedActors.Add(playerActor);
				}
			}
			playerActor.PendingExtraTurnOfTypeStack.Push(playerActor.SelectingCardsForExtraTurnOfType);
			playerActor.SelectingCardsForExtraTurnOfType = CAbilityExtraTurn.EExtraTurnType.None;
			PlayerFinishedMovingCards(playerActor);
		}
		else
		{
			StartActorExtraTurnImmediately(playerActor);
			CExtraTurnCardsSelected message = new CExtraTurnCardsSelected(playerActor);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void PlayerFinishedMovingCards(CPlayerActor playerActor)
	{
		CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(null)
		{
			WaitActor = playerActor,
			WaitTickFrame = 10000,
			ClearEvents = false
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void NextRound()
	{
		s_RoundCount++;
		CheckObjectives();
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			allActor.ActorActionHasHappened = false;
			allActor.HasTakenWoundDamageThisTurn = false;
			allActor.ProcessConditionTokens(EConditionDecTrigger.Rounds);
		}
		CNextRound_MessageData message = new CNextRound_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
		PhaseManager.SetNextPhase(CPhase.PhaseType.StartRoundEffects);
	}

	public static List<CActor> ActorListOnly(List<CRangeSortedActor> rangedSortedActors, bool sortForAttack = false, bool shortestOnly = false)
	{
		rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Actor.SubInitiative().CompareTo(y.m_Actor.SubInitiative()));
		if (sortForAttack)
		{
			rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => UpdatedInitiativeWithSummons(x.m_Actor, x.m_Actor.Initiative()).CompareTo(UpdatedInitiativeWithSummons(y.m_Actor, y.m_Actor.Initiative())));
		}
		else
		{
			rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Actor.Initiative().CompareTo(y.m_Actor.Initiative()));
		}
		rangedSortedActors.Sort((CRangeSortedActor x, CRangeSortedActor y) => x.m_Range.CompareTo(y.m_Range));
		if (shortestOnly)
		{
			rangedSortedActors.RemoveAll((CRangeSortedActor x) => x.m_Range > rangedSortedActors[0].m_Range);
		}
		rangedSortedActors.RemoveAll((CRangeSortedActor x) => x.m_Actor.Untargetable && !x.m_Self);
		return rangedSortedActors.Select((CRangeSortedActor s) => s.m_Actor).ToList();
	}

	public static List<CActor> GetActorsInRange(CActor targetingActor, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, CTile lastSelectedTile = null, bool? canTargetInvisible = false, bool originalTargetType = false)
	{
		return GetActorsInRange(targetingActor.ArrayIndex, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, areaEffectTiles, isTargetedAbility, lastSelectedTile, canTargetInvisible, originalTargetType);
	}

	public static List<CActor> GetActorsInRange(Point arrayIndex, CActor filterActor, int range, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, CAreaEffect areaEffect, List<CTile> areaEffectTiles, bool isTargetedAbility, CTile lastSelectedTile = null, bool? canTargetInvisible = false, bool originalTargetType = false)
	{
		List<CRangeSortedActor> list = new List<CRangeSortedActor>();
		if (abilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !abilityFilter.HasNonTargetTypeFilters())
		{
			if (actorsToIgnore == null || !actorsToIgnore.Contains(filterActor))
			{
				list.Add(new CRangeSortedActor(filterActor, 0, self: true));
			}
			return ActorListOnly(list);
		}
		if (areaEffect != null)
		{
			if (areaEffectTiles == null)
			{
				DLLDebug.LogError("AreaEffect sent without tile set to GetActorsInRange.");
				return ActorListOnly(list);
			}
			foreach (CTile areaEffectTile in areaEffectTiles)
			{
				CActor actorOnTile = GetActorOnTile(areaEffectTile, filterActor, abilityFilter, actorsToIgnore, isTargetedAbility, canTargetInvisible);
				if (actorOnTile != null && !actorOnTile.IsDead)
				{
					CTile targetTile = ScenarioManager.Tiles[actorOnTile.ArrayIndex.X, actorOnTile.ArrayIndex.Y];
					bool foundPath;
					List<Point> list2 = ScenarioManager.PathFinder.FindPath(arrayIndex, actorOnTile.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
					if (foundPath && CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], targetTile))
					{
						list.Add(new CRangeSortedActor(actorOnTile, list2.Count));
					}
				}
			}
			return ActorListOnly(list);
		}
		Point startLocation = lastSelectedTile?.m_ArrayIndex ?? arrayIndex;
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(startLocation.X, startLocation.Y, ScenarioManager.EAdjacentPosition.ECenter);
		List<CTile> list3 = new List<CTile>();
		for (int i = startLocation.Y - range; i <= startLocation.Y + range; i++)
		{
			for (int j = startLocation.X - range; j <= startLocation.X + range; j++)
			{
				CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile2 != null)
				{
					CObjectDoor obj = (CObjectDoor)adjacentTile2.FindProp(ScenarioManager.ObjectImportType.Door);
					if (obj != null && obj.Activated)
					{
						list3.Add(adjacentTile2);
					}
				}
			}
		}
		for (int k = startLocation.Y - range; k <= startLocation.Y + range; k++)
		{
			for (int l = startLocation.X - range; l <= startLocation.X + range; l++)
			{
				CTile adjacentTile3 = ScenarioManager.GetAdjacentTile(l, k, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile3 == null)
				{
					continue;
				}
				List<CActor> actorsOnTile = GetActorsOnTile(adjacentTile3, filterActor, abilityFilter, actorsToIgnore, isTargetedAbility, canTargetInvisible, originalTargetType);
				if (actorsOnTile == null || actorsOnTile.Count <= 0)
				{
					continue;
				}
				foreach (CActor item in actorsOnTile)
				{
					if (item == null || item.IsDead)
					{
						continue;
					}
					CTile targetTile2 = ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y];
					CTile cTile = null;
					if (adjacentTile3.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile3.m_HexMap != adjacentTile.m_HexMap)
					{
						bool flag = false;
						foreach (CTile item2 in list3)
						{
							if ((adjacentTile3.m_HexMap == item2.m_HexMap && adjacentTile.m_HexMap == item2.m_Hex2Map) || (adjacentTile3.m_HexMap == item2.m_Hex2Map && adjacentTile.m_HexMap == item2.m_HexMap))
							{
								if (cTile == null)
								{
									cTile = item2;
								}
								else
								{
									flag = true;
								}
							}
						}
						if (flag)
						{
							cTile = null;
						}
					}
					List<Point> list4 = null;
					bool foundPath2;
					if (cTile == null)
					{
						list4 = ScenarioManager.PathFinder.FindPath(startLocation, item.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2);
					}
					else
					{
						list4 = ScenarioManager.PathFinder.FindPath(startLocation, cTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2);
						if (foundPath2)
						{
							list4.AddRange(ScenarioManager.PathFinder.FindPath(cTile.m_ArrayIndex, item.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath2));
						}
					}
					if (startLocation.Equals(item.ArrayIndex))
					{
						foundPath2 = true;
					}
					if ((item == filterActor || (foundPath2 && list4.Count <= range)) && CActor.HaveLOS(ScenarioManager.Tiles[startLocation.X, startLocation.Y], targetTile2))
					{
						list.Add(new CRangeSortedActor(item, list4.Count));
					}
				}
			}
		}
		return ActorListOnly(list);
	}

	public static bool? CanTargetInvisible(CActor actor, bool? canTargetInvisible)
	{
		bool? flag = canTargetInvisible;
		if (flag != true)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.Attack).Where(delegate(CActiveBonus w)
			{
				AbilityData.MiscAbilityData miscAbilityData = w.Ability.MiscAbilityData;
				return miscAbilityData != null && miscAbilityData.CanTargetInvisible == true;
			}).ToList();
			if (list.Count > 0)
			{
				CAbility ability = list[0].Ability;
				if (ability == null || ability.MiscAbilityData?.NotApplyEnemy != true)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public static CActor GetActorOnTile(CTile tile, CActor filterActor, CAbilityFilterContainer abilityFilter, List<CActor> actorsToIgnore, bool isTargetedAbility, bool? canTargetInvisible = false, bool originalTargetType = false)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex);
		if (cActor == null)
		{
			return null;
		}
		if (abilityFilter.IsValidTarget(cActor, filterActor, isTargetedAbility, originalTargetType, canTargetInvisible))
		{
			if (actorsToIgnore != null && actorsToIgnore.Contains(cActor))
			{
				return null;
			}
			return cActor;
		}
		return null;
	}

	public static List<CActor> GetActorsOnTile(CTile tile, CActor filterActor, CAbilityFilterContainer abilityFilter, List<CActor> actorsToIgnore, bool isTargetedAbility, bool? canTargetInvisible = false, bool originalTargetType = false)
	{
		List<CActor> list = ScenarioManager.Scenario.FindActorsAt(tile.m_ArrayIndex);
		if (list == null)
		{
			return null;
		}
		if (list.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!abilityFilter.IsValidTarget(list[num], filterActor, isTargetedAbility, originalTargetType, canTargetInvisible) || (actorsToIgnore != null && actorsToIgnore.Contains(list[num])))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static List<CTile> GetTilesInRange(CActor targetActor, int range, CAbility.EAbilityTargeting targetingType, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool allowClosedDoorTiles = false, bool includeTargetPosition = false)
	{
		return GetTilesInRange(targetActor.ArrayIndex, range, targetingType, emptyTilesOnly, ignoreBlocked, innerRange, ignorePathLength, ignoreBlockedWithActor, ignoreLOS, emptyOpenDoorTiles, ignoreMoveCost, ignoreDifficultTerrain, allowClosedDoorTiles, includeTargetPosition);
	}

	public static List<CTile> GetTilesInRange(Point arrayIndex, int range, CAbility.EAbilityTargeting targetingType, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool allowClosedDoorTiles = false, bool includeTargetPosition = false, bool noActorsAllowed = false, bool skipPathingCheck = false)
	{
		List<CTile> list = new List<CTile>();
		List<CTile> list2 = new List<CTile>();
		List<CTile> list3 = new List<CTile>();
		switch (targetingType)
		{
		case CAbility.EAbilityTargeting.Range:
		{
			int num = Math.Max(1, range);
			int num2 = ((arrayIndex.Y - num >= 0) ? (arrayIndex.Y - num) : 0);
			int num3 = ((arrayIndex.Y + num < ScenarioManager.Height) ? (arrayIndex.Y + num) : ScenarioManager.Height);
			int num4 = ((arrayIndex.X - num >= 0) ? (arrayIndex.X - num) : 0);
			int num5 = ((arrayIndex.X + num < ScenarioManager.Width) ? (arrayIndex.X + num) : ScenarioManager.Width);
			for (int k = num2; k <= num3; k++)
			{
				for (int l = num4; l <= num5; l++)
				{
					if (k < 0 || l < 0 || k >= ScenarioManager.Height || l >= ScenarioManager.Width)
					{
						continue;
					}
					CTile adjacentTile = ScenarioManager.GetAdjacentTile(l, k, ScenarioManager.EAdjacentPosition.ECenter);
					if (adjacentTile != null && (innerRange == null || !innerRange.Contains(adjacentTile)))
					{
						CObjectDoor obj = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
						if (obj != null && obj.Activated)
						{
							list3.Add(adjacentTile);
						}
						list2.Add(adjacentTile);
					}
				}
			}
			break;
		}
		case CAbility.EAbilityTargeting.Room:
		{
			CTile cTile2 = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
			if (cTile2.m_HexMap != null && cTile2.m_Hex2Map != null)
			{
				break;
			}
			foreach (CMapTile mapTile in cTile2.m_HexMap.MapTiles)
			{
				CTile item = ScenarioManager.Tiles[mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y];
				list2.Add(item);
			}
			break;
		}
		case CAbility.EAbilityTargeting.All:
		{
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile3 in tiles)
			{
				if (cTile3 != null)
				{
					list2.Add(cTile3);
				}
			}
			break;
		}
		case CAbility.EAbilityTargeting.AllConnectedRooms:
		{
			CTile originTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile != null && ScenarioManager.RoomsBetweenTilesRevealed(originTile, cTile))
				{
					list2.Add(cTile);
				}
			}
			break;
		}
		}
		foreach (CTile item2 in list2)
		{
			if (item2 == null)
			{
				continue;
			}
			bool foundPath = false;
			List<Point> list4 = null;
			CTile cTile4 = null;
			int num6 = 0;
			CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
			if (!skipPathingCheck)
			{
				if (item2.FindProp(ScenarioManager.ObjectImportType.Door) == null && adjacentTile2.FindProp(ScenarioManager.ObjectImportType.Door) == null && item2.m_HexMap != adjacentTile2.m_HexMap)
				{
					bool flag = false;
					foreach (CTile item3 in list3)
					{
						if ((item2.m_HexMap == item3.m_HexMap && adjacentTile2.m_HexMap == item3.m_Hex2Map) || (item2.m_HexMap == item3.m_Hex2Map && adjacentTile2.m_HexMap == item3.m_HexMap))
						{
							if (cTile4 == null)
							{
								cTile4 = item3;
							}
							else
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						cTile4 = null;
					}
					if (cTile4 != null)
					{
						list4 = ScenarioManager.PathFinder.FindPath(arrayIndex, cTile4.m_ArrayIndex, ignoreBlocked || ignoreBlockedWithActor, ignoreMoveCost: false, out foundPath, allowClosedDoorTiles);
						if (foundPath)
						{
							list4.AddRange(ScenarioManager.PathFinder.FindPath(cTile4.m_ArrayIndex, item2.m_ArrayIndex, ignoreBlocked || ignoreBlockedWithActor, ignoreMoveCost: false, out foundPath, allowClosedDoorTiles));
						}
					}
				}
				if (cTile4 == null)
				{
					list4 = ScenarioManager.PathFinder.FindPath(arrayIndex, item2.m_ArrayIndex, ignoreBlocked || ignoreBlockedWithActor, ignoreMoveCost: false, out foundPath, allowClosedDoorTiles);
				}
				if (includeTargetPosition && !foundPath && item2.m_ArrayIndex == arrayIndex)
				{
					foundPath = true;
					list4.Add(arrayIndex);
				}
				num6 = CAbilityMove.CalculateMoveCost(list4, !ignoreBlocked, !ignoreBlocked, ignoreMoveCost);
			}
			else
			{
				foundPath = true;
			}
			if (emptyTilesOnly)
			{
				bool flag2 = false;
				bool flag3 = true;
				bool flag4 = item2.m_Props.FindAll((CObjectProp x) => x.ObjectType != ScenarioManager.ObjectImportType.Trap || (x.ObjectType == ScenarioManager.ObjectImportType.Trap && !x.Activated)).ToList().Count == 0;
				bool flag5 = item2.m_Props.Count > 0 && ignoreDifficultTerrain && (item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater);
				bool flag6 = item2.m_Props.Count > 0 && item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect;
				bool flag7 = item2.m_Props.Count > 0 && item2.m_Props[0].IsLootable;
				bool num7 = item2.FindProp(ScenarioManager.ObjectImportType.Door) != null;
				bool isBridgeOpen = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].IsBridgeOpen;
				bool flag8 = num7 && (isBridgeOpen || allowClosedDoorTiles);
				bool walkable = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Walkable;
				bool blocked = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked;
				bool flag9 = num6 <= range || ignorePathLength;
				foreach (CActor item4 in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex))
				{
					if (!(item4 is CHeroSummonActor cHeroSummonActor) || !cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
					{
						flag3 = false;
					}
				}
				if (emptyOpenDoorTiles)
				{
					if (flag3 && (flag4 || flag5 || flag6 || flag7 || flag8) && walkable && !blocked && flag9 && foundPath)
					{
						flag2 = true;
					}
				}
				else if (flag3 && (flag4 || (ignoreDifficultTerrain && item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.MoneyToken || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Chest || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.GoalChest || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Resource || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.PressurePlate || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Portal || item2.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect) && walkable && !blocked && foundPath && flag9)
				{
					flag2 = true;
				}
				if (flag2 && ((allowClosedDoorTiles && ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
				{
					list.Add(item2);
				}
			}
			else if (ignoreBlockedWithActor)
			{
				if (foundPath && (num6 <= range || ignorePathLength) && (ignoreBlocked || !ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked || (ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Blocked && ScenarioManager.Scenario.FindActorAt(item2.m_ArrayIndex) != null)) && ((allowClosedDoorTiles && ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
				{
					list.Add(item2);
				}
			}
			else if (!emptyTilesOnly && !allowClosedDoorTiles)
			{
				if (!((item2.FindProp(ScenarioManager.ObjectImportType.Door) == null || ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].IsBridgeOpen) && foundPath) || !(num6 <= range || ignorePathLength))
				{
					continue;
				}
				if (noActorsAllowed)
				{
					bool flag10 = true;
					foreach (CActor item5 in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex))
					{
						if (!(item5 is CHeroSummonActor cHeroSummonActor2) || !cHeroSummonActor2.HeroSummonClass.SummonYML.TreatAsTrap)
						{
							flag10 = false;
						}
					}
					if (flag10)
					{
						list.Add(item2);
					}
				}
				else
				{
					list.Add(item2);
				}
			}
			else if (foundPath && (num6 <= range || ignorePathLength) && (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], item2)))
			{
				list.Add(item2);
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetTilesAtRange(Point arrayIndex, int atRange, int range, bool emptyTilesOnly, bool ignoreBlocked = false, List<CTile> innerRange = null, bool ignorePathLength = false, bool ignoreBlockedWithActor = false, bool ignoreLOS = false, bool emptyOpenDoorTiles = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false, bool allowClosedDoorTiles = false, bool includeTargetPosition = false)
	{
		List<CTile> list = new List<CTile>();
		int num = Math.Max(1, atRange);
		for (int i = arrayIndex.Y - num; i <= arrayIndex.Y + num; i += num)
		{
			for (int j = arrayIndex.X - num; j <= arrayIndex.X + num; j += num)
			{
				if (i < 0 || j < 0 || i >= ScenarioManager.Height || j >= ScenarioManager.Width)
				{
					continue;
				}
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile == null || (innerRange != null && innerRange.Contains(adjacentTile)))
				{
					continue;
				}
				bool foundPath;
				List<Point> list2 = ScenarioManager.PathFinder.FindPath(arrayIndex, adjacentTile.m_ArrayIndex, ignoreBlocked || ignoreBlockedWithActor, ignoreMoveCost: true, out foundPath, allowClosedDoorTiles);
				if (includeTargetPosition && !foundPath && adjacentTile.m_ArrayIndex == arrayIndex)
				{
					foundPath = true;
					list2.Add(arrayIndex);
				}
				int num2 = CAbilityMove.CalculateMoveCost(list2, !ignoreBlocked, !ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain);
				if (emptyTilesOnly)
				{
					bool flag = false;
					if (emptyOpenDoorTiles)
					{
						if (ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && (adjacentTile.m_Props.Count == 0 || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || adjacentTile.m_Props[0].IsLootable || (adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door) != null && (ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].IsBridgeOpen || allowClosedDoorTiles))) && ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked && foundPath && (num2 <= range || ignorePathLength))
						{
							flag = true;
						}
					}
					else if (ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) == null && (adjacentTile.m_Props.Count == 0 || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainRubble) || (ignoreDifficultTerrain && adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainWater) || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.MoneyToken || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Chest || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.GoalChest || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Resource || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.PressurePlate || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.Portal || adjacentTile.m_Props[0].ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect) && ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked && foundPath && (num2 <= range || ignorePathLength))
					{
						flag = true;
					}
					if (flag && ((allowClosedDoorTiles && ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
					{
						list.Add(adjacentTile);
					}
				}
				else if (ignoreBlockedWithActor)
				{
					if (foundPath && (num2 <= range || ignorePathLength) && (ignoreBlocked || !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked || (ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Blocked && ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex) != null)) && ((allowClosedDoorTiles && ignoreLOS) || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
					{
						list.Add(adjacentTile);
					}
				}
				else if (foundPath && (num2 <= range || ignorePathLength) && (ignoreLOS || CActor.HaveLOS(ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y], adjacentTile)))
				{
					list.Add(adjacentTile);
				}
			}
		}
		return list.Distinct().ToList();
	}

	public static void UpdateDamageInflictedThisTurn(int damageInflictedThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_DamageInflictedThisTurn += damageInflictedThisTurn;
		}
		CPhaseAction.UpdateDamageInflictedThisAction(damageInflictedThisTurn);
	}

	public static void UpdateActorsKilledThisRoundAndTurn(CActor actorKilled)
	{
		s_ActorsKilledThisRound.Add(actorKilled);
		s_ActorsKilledThisTurn.Add(actorKilled);
		CPhaseAction.UpdateActorsKilledThisAction(actorKilled);
	}

	public static void UpdateTargetsDamagedInPrevAttackThisTurn(CAbilityAttack abilityAttack)
	{
		if (s_TargetsDamagedInPrevAttackThisTurnAbilityID != abilityAttack.ID)
		{
			s_TargetsDamagedInPrevAttackThisTurnAbilityID = abilityAttack.ID;
			s_TargetsDamagedInPrevAttackThisTurn = 0;
		}
		s_TargetsDamagedInPrevAttackThisTurn++;
		CPhaseAction.UpdateTargetsDamagedInPrevAttackThisAction(abilityAttack);
	}

	public static void UpdateTargetsActuallyDamagedInPrevAttackThisTurn(CAbilityAttack abilityAttack)
	{
		if (s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID != abilityAttack.ID)
		{
			s_TargetsActuallyDamagedInPrevAttackThisTurnAbilityID = abilityAttack.ID;
			s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
		}
		s_TargetsActuallyDamagedInPrevAttackThisTurn++;
		CPhaseAction.UpdateTargetsActuallyDamagedInPrevAttackThisAction(abilityAttack);
	}

	public static void UpdateTargetsDamagedInPrevDamageAbilityThisTurn(CAbilityDamage abilityDamage)
	{
		if (s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID != abilityDamage.ID)
		{
			s_TargetsDamagedInPrevDamageAbilityThisTurnAbilityID = abilityDamage.ID;
			s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
		}
		s_TargetsDamagedInPrevDamageAbilityThisTurn++;
		CPhaseAction.UpdateTargetsDamagedInPrevDamageAbilityThisAction(abilityDamage);
	}

	public static void UpdateHexesMovedThisTurn(CActor actorMoved, List<TileIndex> hexesMoved)
	{
		if (hexesMoved.Count > 0)
		{
			s_ActorsMovedThisTurn.Add(actorMoved);
		}
		if (!OverridingCurrentActor)
		{
			s_HexesMovedThisTurn.AddRange(hexesMoved);
		}
		CPhaseAction.UpdateHexesMovedThisAction(hexesMoved.Count);
	}

	public static void UpdateObstaclesDestroyedThisTurn(int obstaclesDestroyedThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_ObstaclesDestroyedThisTurn += obstaclesDestroyedThisTurn;
		}
		CPhaseAction.UpdateObstaclesDestroyedThisAction(obstaclesDestroyedThisTurn);
	}

	public static void UpdateHazardousTerrainTilesMovedOverThisTurn(int hazardousTerrainTilesMovedOverThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_HazardousTerrainTilesMovedOverThisTurn += hazardousTerrainTilesMovedOverThisTurn;
		}
	}

	public static void UpdateDifficultTerrainTilesMovedOverThisTurn(int difficultTerrainTilesMovedOverThisTurn)
	{
		if (!OverridingCurrentActor)
		{
			s_DifficultTerrainTilesMovedOverThisTurn += difficultTerrainTilesMovedOverThisTurn;
		}
	}

	public static void OverrideCurrentActorForOneAction(CActor actor, CActor.EType? switchType = null, bool killActorAfterAction = false, List<CAugment> addedAugments = null, CActor controllingActor = null, bool useControllingActorModifierDeck = false, bool useControllingActorPlayerControl = false)
	{
		if (controllingActor is CPlayerActor cPlayerActor)
		{
			Debug.Log(string.Format("{0} {1} get control over {2} {3}", "<color=#f1dc81>[GameState]</color>", cPlayerActor.CharacterClass.CharacterID, actor.OriginalType, actor.ActorGuid));
		}
		CActor.EType type = actor.Type;
		bool isUnderMyControl = actor.IsUnderMyControl;
		if (switchType.HasValue)
		{
			actor.Type = switchType.Value;
			if (switchType.Value != CActor.EType.Player)
			{
				if (type != CActor.EType.Player)
				{
					actor.IsUnderMyControl = false;
				}
			}
			else if (type == CActor.EType.Player)
			{
				if (controllingActor != null && useControllingActorPlayerControl)
				{
					actor.IsUnderMyControl = controllingActor.IsUnderMyControl;
				}
			}
			else if (controllingActor != null)
			{
				actor.IsUnderMyControl = controllingActor.IsUnderMyControl;
			}
			else
			{
				actor.IsUnderMyControl = true;
			}
		}
		s_OverridenActionActorStack.Push(new OverrideActorForActionStackData(s_CurrentActor, switchType.HasValue, type, isUnderMyControl, killActorAfterAction, actor.AugmentSlots, addedAugments, controllingActor, actor, useControllingActorModifierDeck));
		if (addedAugments != null && addedAugments.Count > 0)
		{
			actor.OverrideAugmentSlots(actor.Augments.Count + addedAugments.Count);
			foreach (CAugment addedAugment in addedAugments)
			{
				actor.Class.AddAugmentBaseCardToTemporaryCards(addedAugment.RegisteredBaseCard as CAbilityCard);
				actor.Augments.Add(addedAugment);
			}
		}
		s_CurrentActor = actor;
		OverridingCurrentActor = true;
		ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
	}

	public static void EndOverrideCurrentActorForOneAction()
	{
		Debug.Log(string.Format("{0} {1} {2} freed from control...", "<color=#f1dc81>[GameState]</color>", s_CurrentActor.OriginalType, s_CurrentActor.ActorGuid));
		InternalCurrentActor.Inventory.HandleUsedItems();
		CActor cActor = s_CurrentActor;
		OverrideActorForActionStackData overrideActorForActionStackData = s_OverridenActionActorStack.Pop();
		cActor.MindControlDuration = CAbilityControlActor.EControlDurationType.None;
		if (overrideActorForActionStackData.OverrideSwitchedType)
		{
			cActor.Type = overrideActorForActionStackData.OriginalType;
			cActor.IsUnderMyControl = overrideActorForActionStackData.OriginallyUnderMyControl;
		}
		if (overrideActorForActionStackData.AddedAugments != null && overrideActorForActionStackData.AddedAugments.Count > 0)
		{
			cActor.OverrideAugmentSlots(null);
			for (int num = overrideActorForActionStackData.AddedAugments.Count - 1; num >= 0; num--)
			{
				CAugment item = overrideActorForActionStackData.AddedAugments[num];
				cActor.Augments.Remove(item);
			}
			cActor.Class.ClearTemporaryCards();
		}
		if (overrideActorForActionStackData.PreviousActor != null)
		{
			s_CurrentActor = overrideActorForActionStackData.PreviousActor;
		}
		OverridingCurrentActor = s_OverridenActionActorStack.Count > 0;
		ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
		CEnemyActor cEnemyActor = cActor as CEnemyActor;
		if (overrideActorForActionStackData.OverrideKillActor)
		{
			if (cEnemyActor != null)
			{
				cEnemyActor.OnDeathAbilityComplete();
			}
			else if (cActor is CHeroSummonActor cHeroSummonActor)
			{
				cHeroSummonActor.OnDeathAbilityComplete();
			}
		}
	}

	public static void QueueOverrideActorForOneTurn(CActor actor, CActor.EType switchType, List<CAbility> controlAbilities = null, CBaseCard baseCard = null, CActor controllingActor = null, bool useControllingActorPlayerControl = false)
	{
		s_ActorsToOverrideTurns[actor] = new QueueOverrideActorForTurnData(switchType, controlAbilities, baseCard, controllingActor, useControllingActorPlayerControl);
	}

	public static void OverrideActorForOneTurn(CActor currentActor)
	{
		if (!s_ActorsToOverrideTurns.TryGetValue(currentActor, out var value))
		{
			return;
		}
		CActor.EType type = currentActor.Type;
		currentActor.Type = value.OverrideActorType;
		bool isUnderMyControl = currentActor.IsUnderMyControl;
		if (value.OverrideActorType != CActor.EType.Player)
		{
			if (type != CActor.EType.Player)
			{
				currentActor.IsUnderMyControl = false;
			}
		}
		else if (type == CActor.EType.Player)
		{
			if (value.ControllingActor != null && value.UseControllingActorPlayerControl)
			{
				currentActor.IsUnderMyControl = value.ControllingActor.IsUnderMyControl;
			}
		}
		else if (value.ControllingActor != null)
		{
			currentActor.IsUnderMyControl = value.ControllingActor.IsUnderMyControl;
		}
		else
		{
			currentActor.IsUnderMyControl = true;
		}
		if (value.OverrideActionAbilities != null && value.OverrideActionAbilities.Count > 0 && value.OverrideActionBaseCard != null)
		{
			s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, EActionSelectionFlag.TopActionPlayed);
			s_CurrentActionSelectionFlag = SetFlag(s_CurrentActionSelectionFlag, EActionSelectionFlag.BottomActionPlayed);
		}
		CurrentOverridenActorForTurnData = new OverrideActorForTurnData(currentActor, currentActor.Type == type, type, isUnderMyControl, null);
		CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(currentActor)
		{
			m_ControlActorAbility = null,
			m_ControlledActor = currentActor
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void OverrideCurrentActor(CActor currentActor)
	{
		s_CurrentActor = currentActor;
	}

	public static void EndOverrideActorForOneTurn()
	{
		if (CurrentOverridenActorForTurnData != null)
		{
			CurrentOverridenActorForTurnData.OverridenActor.MindControlDuration = CAbilityControlActor.EControlDurationType.None;
			CurrentOverridenActorForTurnData.OverridenActor.Type = CurrentOverridenActorForTurnData.OriginalType;
			CurrentOverridenActorForTurnData.OverridenActor.IsUnderMyControl = CurrentOverridenActorForTurnData.OriginallyUnderMyControl;
			s_ActorsToOverrideTurns.Remove(CurrentOverridenActorForTurnData.OverridenActor);
			CurrentOverridenActorForTurnData = null;
		}
	}

	public static void CacheExtraTurnInitiatorCard(CAbilityCard extraTurnInitiatorCard)
	{
		if (s_CurrentActor != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			CachedExtraTurnInitiatorCardStack.Push(extraTurnInitiatorCard);
		}
	}

	public static void StartActorExtraTurnImmediately(CPlayerActor playerActor)
	{
		if (playerActor.SelectingCardsForExtraTurnOfType != playerActor.TakingExtraTurnOfType)
		{
			playerActor.TakingExtraTurnOfTypeStack.Push(playerActor.SelectingCardsForExtraTurnOfType);
		}
		playerActor.SelectingCardsForExtraTurnOfType = CAbilityExtraTurn.EExtraTurnType.None;
		playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Push(playerActor.CharacterClass.ExtraTurnCards.ToList());
		if (CurrentActionInitiator == EActionInitiator.AbilityCard)
		{
			ProgressActionSelection(RoundAbilityCardselected);
		}
		EActionSelectionFlag eActionSelectionFlag = EActionSelectionFlag.None;
		if (playerActor.SkipTopCardAction)
		{
			playerActor.SkipTopCardAction = false;
			eActionSelectionFlag = SetFlag(eActionSelectionFlag, EActionSelectionFlag.TopActionPlayed);
		}
		if (playerActor.SkipBottomCardAction)
		{
			playerActor.SkipBottomCardAction = false;
			eActionSelectionFlag = SetFlag(eActionSelectionFlag, EActionSelectionFlag.BottomActionPlayed);
		}
		ExtraTurnActionSelectionFlagStack.Push(eActionSelectionFlag);
		s_ExtraTurnActorStack.Push(new OverrideActorForActionStackData(s_CurrentActor, overrideSwitchedType: false, s_CurrentActor.Type, s_CurrentActor.IsUnderMyControl, overrideKillActor: false, s_CurrentActor.AugmentSlots, null, null, playerActor));
		s_CurrentActor = playerActor;
		s_TurnActor = playerActor;
		if (playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActions || playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			playerActor.AbilityTypesPerformedThisTurn.Clear();
			playerActor.AbilityTypesPerformedThisAction.Clear();
			s_DamageInflictedThisTurn = 0;
			s_ActorsKilledThisTurn.Clear();
			s_ActorsMovedThisTurn.Clear();
			s_TargetsDamagedInPrevAttackThisTurn = 0;
			s_TargetsActuallyDamagedInPrevAttackThisTurn = 0;
			s_TargetsDamagedInPrevDamageAbilityThisTurn = 0;
			s_HexesMovedThisTurn.Clear();
			s_ObstaclesDestroyedThisTurn = 0;
			s_HazardousTerrainTilesMovedOverThisTurn = 0;
			s_DifficultTerrainTilesMovedOverThisTurn = 0;
		}
		PhaseManager.SetNextPhase(CPhase.PhaseType.StartTurn);
	}

	public static void EndExtraTurn()
	{
		CActor cActor = s_CurrentActor;
		CCharacterClass cCharacterClass = null;
		if (s_CurrentActor.Class is CCharacterClass cCharacterClass2)
		{
			cCharacterClass = cCharacterClass2;
			cCharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Pop();
		}
		CAbilityExtraTurn.EExtraTurnType eExtraTurnType = cActor.TakingExtraTurnOfTypeStack.Pop();
		if (cActor.PendingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (cCharacterClass != null)
			{
				CAbilityExtraTurn.PendingExtraTurnData pendingExtraTurnData = cCharacterClass.PendingExtraTurnCardsData[0];
				cCharacterClass.SetExtraTurnInitiativeAbilityCard(pendingExtraTurnData.PendingExtraTurnCards.First((CAbilityCard x) => x.Initiative == pendingExtraTurnData.LeadingInitiative));
			}
			s_InitiativeSortedActors.Remove(cActor);
			bool flag = false;
			foreach (CActor s_InitiativeSortedActor in s_InitiativeSortedActors)
			{
				if (s_ActorInitiativeComparer.Compare(cActor, s_InitiativeSortedActor) < 0)
				{
					int index = s_InitiativeSortedActors.IndexOf(s_InitiativeSortedActor);
					s_InitiativeSortedActors.Insert(index, cActor);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				s_InitiativeSortedActors.Add(cActor);
			}
		}
		else
		{
			cCharacterClass?.SetExtraTurnInitiativeAbilityCard(null);
		}
		if (s_ExtraTurnActorStack.Count > 0)
		{
			s_CurrentActor = s_ExtraTurnActorStack.Pop().PreviousActor;
		}
		if (s_CurrentActor != null && CachedExtraTurnInitiatorCardStack.Peek() != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			CachedExtraTurnInitiatorCardStack.Peek().ActionHasHappened = true;
		}
		if (eExtraTurnType == CAbilityExtraTurn.EExtraTurnType.CurrentAction)
		{
			RestoreCacheActionSelectionSequence();
		}
		if (eExtraTurnType != CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			CEndedExtraTurn cEndedExtraTurn = new CEndedExtraTurn(s_CurrentActor);
			cEndedExtraTurn.m_ActorTurnEnded = cActor;
			ScenarioRuleClient.MessageHandler(cEndedExtraTurn);
			PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
		}
	}

	public static void FinishEndExtraTurn()
	{
		CAbilityCard cAbilityCard = CachedExtraTurnInitiatorCardStack.Pop();
		if (s_CurrentActor != null && cAbilityCard != null && ScenarioManager.Scenario.HasActor(s_CurrentActor) && s_CurrentActor.Class is CCharacterClass)
		{
			if (s_CurrentActor.IsTakingExtraTurn)
			{
				((CPlayerActor)s_CurrentActor).CharacterClass.DiscardExtraTurnAbilityCard(cAbilityCard);
			}
			else
			{
				((CPlayerActor)s_CurrentActor).CharacterClass.DiscardRoundAbilityCard(cAbilityCard);
			}
			CClass.MarkAbilityActiveBonusesFinished(s_CurrentActor);
		}
	}

	public static void SetCurrentActorForCompanionSummon(CPlayerActor playerActor)
	{
		s_CurrentActor = playerActor;
		ScenarioRuleClient.MessageHandler(new CUpdateCurrentActor_MessageData(s_CurrentActor));
	}

	public static bool IsParentUnderMyControl(CActor underMyControlActor)
	{
		OverrideActorForActionStackData[] array = s_OverridenActionActorStack.ToArray();
		foreach (OverrideActorForActionStackData overrideActorForActionStackData in array)
		{
			if (overrideActorForActionStackData.UnderMyControlActor.Equals(underMyControlActor))
			{
				return overrideActorForActionStackData.PreviousActor.IsUnderMyControl;
			}
		}
		if (s_ActorsToOverrideTurns.ContainsKey(underMyControlActor))
		{
			return s_ActorsToOverrideTurns[underMyControlActor].ControllingActor.IsUnderMyControl;
		}
		return false;
	}

	private static void CheckObjectives()
	{
		foreach (CObjective item in ScenarioManager.CurrentScenarioState.WinObjectives.Where((CObjective o) => !o.IsActive))
		{
			item.CheckActivationRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		foreach (CObjective item2 in ScenarioManager.CurrentScenarioState.LoseObjectives.Where((CObjective o) => !o.IsActive))
		{
			item2.CheckActivationRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		CCheckCompleteObjectives_MessageData message = new CCheckCompleteObjectives_MessageData();
		ScenarioRuleClient.MessageHandler(message);
	}
}
