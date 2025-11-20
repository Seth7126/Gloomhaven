using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CActor : IEquatable<CActor>
{
	[Serializable]
	public enum EType
	{
		Player,
		Enemy,
		HeroSummon,
		Ally,
		Enemy2,
		Neutral,
		Unknown
	}

	[Serializable]
	public enum EOverrideLookupProperty
	{
		NA,
		Shield
	}

	[Serializable]
	public enum ECauseOfDeath
	{
		None,
		StillAlive,
		Undetermined,
		DebugMenu,
		Damage,
		NoMoreCards,
		KillAbility,
		SummonerDied,
		SummonActiveBonusCancelled,
		Trap,
		Suicide,
		HazardousTerrain,
		ActorRemovedFromMap
	}

	[Serializable]
	public enum ECauseOfDamage
	{
		None,
		Trap,
		EnemyAttack,
		EnemyDamage,
		Wound,
		Self,
		Ally,
		HazardousTerrain,
		ScenarioElements
	}

	public delegate void MoveStartListener(CAbilityMove moveAbility);

	public delegate void AttackStartListener(CAbilityAttack attackAbility);

	public delegate void PreAttackingListener(CAbilityAttack attackAbility);

	public delegate void HealApplyToActorListener(CAbilityHeal healAbility);

	public delegate void HealApplyToActionListener(CAbilityHeal healAbility);

	public delegate void BeingHealedListener(CAbilityHeal healAbility);

	public delegate void AfterBeingHealedListener();

	public delegate void ConditionApplyToActorListener(CAbility conditionAbility, CActor target);

	public delegate void MoveListener(CAbilityMove moveAbility);

	public delegate void LootListener(CAbilityLoot lootAbility);

	public delegate void AttackListener(CAbilityAttack attackAbility, CActor target);

	public delegate void BeingAttackedPreDamageListener(CAbilityAttack attackAbility);

	public delegate void BeingAttackedPostDamageListener(CAbilityAttack attackAbility);

	public delegate void BeingAttackedListener(CAbilityAttack ability, int modifiedStrength);

	public delegate void RetaliateListener();

	public delegate void AttackFinishedListener(CAbilityAttack ability, CActor target, int damageDealt);

	public delegate void AttackAbilityFinishedListener(CAbilityAttack ability);

	public delegate void TakeDamageListener(int damageTaken, CAbility damagingAbility, int DamageReducedByShields, int actualDamage);

	public delegate void KillListener(CActor target, CActor actor, ECauseOfDeath causeOfDeath, CAbility causeOfDeathAbility, bool onActorTurn);

	public delegate void MovedListener(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes);

	public delegate void CarriedListener(CAbility moveAbility, CActor movedActor, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes);

	public delegate void DrawModifierListener(ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target);

	public delegate void AbilityStartedListener(CAbility startedAbility);

	public delegate void AbilityEndedListener(CAbility endedAbility);

	public delegate void DamagedListener(CActor actor);

	public delegate void DeathListener(CActor actor, CAbility causeOfDeathAbility);

	public delegate void AbilityTargetingStartListener(CAbility ability);

	public delegate void PositiveConditionEndedOnActorListener(CCondition.EPositiveCondition positiveConditionType, CActor target);

	public delegate void NegativeConditionEndedOnActorListener(CCondition.ENegativeCondition negativeConditionType, CActor target);

	public delegate void PreventDamageListener(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility);

	public delegate void CreatedListener(CAbility createdAbility);

	public delegate void LOSCallbackType(Vector sourcePosition, Vector targetPosition, bool rayBlocked, string primitive = "line");

	public static EType[] Types = (EType[])Enum.GetValues(typeof(EType));

	public static EOverrideLookupProperty[] OverrideLookupProperties = (EOverrideLookupProperty[])Enum.GetValues(typeof(EOverrideLookupProperty));

	public AbilityTargetingStartListener m_OnAbilityTargetingStartListeners;

	public AbilityStartedListener m_OnAbilityStartedListeners;

	public AbilityEndedListener m_OnAbilityEndedListeners;

	public AttackStartListener m_OnAttackStartListeners;

	public PreAttackingListener m_OnPreActorIsAttackingListeners;

	public AttackListener m_OnAttackingListeners;

	public BeingAttackedPreDamageListener m_OnBeingAttackedPreDamageListeners;

	public BeingAttackedPostDamageListener m_OnBeingAttackedPostDamageListeners;

	public BeingAttackedListener m_OnBeingAttackedListeners;

	public AttackFinishedListener m_OnAttackFinishedListeners;

	public AttackAbilityFinishedListener m_OnAttackAbilityFinishedListeners;

	public HealApplyToActorListener m_OnHealApplyToActorListeners;

	public HealApplyToActionListener m_OnHealApplyToActionListeners;

	public BeingHealedListener m_OnBeingHealedListeners;

	public AfterBeingHealedListener m_OnAfterBeingHealedListeners;

	public ConditionApplyToActorListener m_OnConditionApplyToActorListeners;

	public PositiveConditionEndedOnActorListener m_OnPositiveConditionEndedOnActorListeners;

	public NegativeConditionEndedOnActorListener m_OnNegativeConditionEndedOnActorListeners;

	public MoveStartListener m_OnMoveStartListeners;

	public MoveListener m_OnMovingListeners;

	public MovedListener m_OnMovedListeners;

	public CarriedListener m_OnCarriedListeners;

	public LootListener m_OnLootListeners;

	public RetaliateListener m_OnRetaliateListeners;

	public DamagedListener m_OnDamagedListeners;

	public TakeDamageListener m_OnTakenDamageListeners;

	public PreventDamageListener m_OnPreventDamageListeners;

	public KillListener m_OnKillListeners;

	public DeathListener m_OnDeathListeners;

	public DrawModifierListener m_OnDrawModifierListeners;

	public CreatedListener m_OnCreatedListeners;

	public bool m_PlayedThisRound;

	public CActor WoundedBy;

	private EType m_Type;

	private EType m_OriginalType;

	private Point m_StartArrayIndex;

	protected int m_ID;

	protected Point m_ArrayIndex;

	protected int m_Health;

	protected int m_MaxHealth;

	protected int m_Gold;

	protected int m_XP;

	protected int m_Level;

	protected CTokens m_Tokens;

	protected CInventory m_Inventory;

	protected int m_BaseAugmentSlots;

	protected int? m_OverridedBaseAugmentSlots;

	protected int m_BaseSongSlots;

	protected int m_BaseDoomSlots;

	protected bool m_OnDeathAbilityUsed;

	protected List<CObjectProp> m_CarriedQuestProps = new List<CObjectProp>();

	protected List<CCharacterResource> m_CharacterResources = new List<CCharacterResource>();

	protected List<CActor> m_ActorsAttackedThisRound = new List<CActor>();

	protected List<CAbility.EAbilityType> m_AbilityTypesPerformedThisTurn = new List<CAbility.EAbilityType>();

	protected List<CAbility.EAbilityType> m_AbilityTypesPerformedThisAction = new List<CAbility.EAbilityType>();

	protected CClass m_Class;

	protected List<CActor> m_AIMoveFocusActors = new List<CActor>();

	protected List<Point> m_AIMoveFocusPath;

	protected List<Point> m_AIMoveFocusTiles = new List<Point>();

	protected bool m_MovementPathSelected;

	protected List<CTile> m_AIMoveFocusWaypoints;

	protected int m_AIMoveRange;

	protected CAbility m_LastAbilityDamagedBy;

	protected CAbility m_CauseOfDeathAbility;

	protected int m_LastDamageAmount;

	protected int m_PreDamageHealth;

	public static readonly List<CAbility.EAbilityType> ImmunityConditionAbilityTypes = new List<CAbility.EAbilityType>
	{
		CAbility.EAbilityType.Bless,
		CAbility.EAbilityType.Curse,
		CAbility.EAbilityType.Disarm,
		CAbility.EAbilityType.Immobilize,
		CAbility.EAbilityType.Invisible,
		CAbility.EAbilityType.Muddle,
		CAbility.EAbilityType.Poison,
		CAbility.EAbilityType.Strengthen,
		CAbility.EAbilityType.Stun,
		CAbility.EAbilityType.Wound,
		CAbility.EAbilityType.StopFlying,
		CAbility.EAbilityType.Sleep
	};

	private static int s_TargetPathID;

	public static Vector s_LOSTileScalar;

	public static ScenarioManager.EAdjacentPosition[] s_EdgeToAdjacentMapping = new ScenarioManager.EAdjacentPosition[6]
	{
		ScenarioManager.EAdjacentPosition.ETopRight,
		ScenarioManager.EAdjacentPosition.ERight,
		ScenarioManager.EAdjacentPosition.EBottomRight,
		ScenarioManager.EAdjacentPosition.EBottomLeft,
		ScenarioManager.EAdjacentPosition.ELeft,
		ScenarioManager.EAdjacentPosition.ETopLeft
	};

	private const float c_CoPlanarLimit = 0.1f;

	private const float c_MinEdgeNormalDot = 0.001f;

	public EType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public EType OriginalType
	{
		get
		{
			return m_OriginalType;
		}
		set
		{
			m_OriginalType = value;
		}
	}

	public Point StartArrayIndex => m_StartArrayIndex;

	public int Health
	{
		get
		{
			return m_Health;
		}
		set
		{
			m_Health = value;
			OnHealthChanged();
		}
	}

	public int OriginalMaxHealth
	{
		get
		{
			return m_MaxHealth;
		}
		set
		{
			m_MaxHealth = value;
		}
	}

	public int MaxHealth
	{
		get
		{
			if (!HealthReduction.HasValue)
			{
				return Math.Max(m_MaxHealth, Overheal);
			}
			return Math.Min(HealthReduction.Value, Math.Max(m_MaxHealth, Overheal));
		}
		set
		{
			m_MaxHealth = value;
		}
	}

	public int Overheal => Math.Max((AccumulativeOverheal != 0) ? (OriginalMaxHealth + AccumulativeOverheal) : 0, MinimumOverheal);

	public int AccumulativeOverheal { get; set; }

	public int MinimumOverheal { get; set; }

	public int? HealthReduction { get; set; }

	public bool IgnoreDifficultTerrain { get; set; }

	public bool IgnoreHazardousTerrain { get; set; }

	public virtual bool Flying => false;

	public int Gold => m_Gold;

	public int XP => m_XP;

	public virtual int Level => m_Level;

	public CClass Class => m_Class;

	public CTokens Tokens => m_Tokens;

	public CInventory Inventory => m_Inventory;

	public bool PlayedThisRound
	{
		get
		{
			return m_PlayedThisRound;
		}
		set
		{
			m_PlayedThisRound = value;
		}
	}

	public List<CActor> AIMoveFocusActors
	{
		get
		{
			return m_AIMoveFocusActors;
		}
		set
		{
			m_AIMoveFocusActors = value;
		}
	}

	public List<CTile> AIMoveFocusWaypoints
	{
		get
		{
			return m_AIMoveFocusWaypoints;
		}
		set
		{
			m_AIMoveFocusWaypoints = value;
		}
	}

	public List<Point> AIMoveFocusPath
	{
		get
		{
			return m_AIMoveFocusPath;
		}
		set
		{
			m_AIMoveFocusPath = value;
		}
	}

	public List<Point> AIMoveFocusTiles
	{
		get
		{
			return m_AIMoveFocusTiles;
		}
		set
		{
			m_AIMoveFocusTiles = value;
		}
	}

	public bool MovementPathSelected
	{
		get
		{
			return m_MovementPathSelected;
		}
		set
		{
			m_MovementPathSelected = value;
		}
	}

	public int IncomingAttackDamage { get; set; }

	public string ActorGuid { get; private set; }

	public List<CAugment> Augments { get; private set; }

	public int AugmentSlots => m_BaseAugmentSlots + (from w in FindApplicableActiveBonuses(CAbility.EAbilityType.AddAugment)
		where w.Ability != null
		select w).Sum((CActiveBonus s) => s.Ability.Strength);

	public CAugment CachedRemovedAugment { get; private set; }

	public CActiveBonus CachedRemovedAugmentActiveBonus { get; private set; }

	public List<CSong> Songs { get; private set; }

	public int SongSlots => (m_OverridedBaseAugmentSlots.HasValue ? m_OverridedBaseAugmentSlots.Value : m_BaseSongSlots) + (from w in FindApplicableActiveBonuses(CAbility.EAbilityType.AddSong)
		where w.Ability != null
		select w).Sum((CActiveBonus s) => s.Ability.Strength);

	public List<CDoom> Dooms { get; private set; }

	public List<CDoom> CachedRemovedOnDeathDooms { get; private set; }

	public CActor DoomTarget { get; private set; }

	public int DoomSlots { get; private set; }

	public CSong CachedRemovedSong { get; private set; }

	public CActiveBonus CachedRemovedSongActiveBonus { get; private set; }

	public ECauseOfDeath CauseOfDeath { get; set; }

	public string KilledByActorGuid { get; set; }

	public CAbilityControlActor.EControlDurationType MindControlDuration { get; set; }

	public bool IsDead => CauseOfDeath != ECauseOfDeath.StillAlive;

	public bool IsDeadForObjectives
	{
		get
		{
			if (CauseOfDeath != ECauseOfDeath.StillAlive)
			{
				return CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap;
			}
			return false;
		}
	}

	public bool IsUsingOnDeathAbility => m_OnDeathAbilityUsed;

	public CAbility LastAbilityPerformed { get; set; }

	public CAbility LastAbilityDamagedBy => m_LastAbilityDamagedBy;

	public int PreDamageHealth => m_PreDamageHealth;

	public int LastDamageAmount => m_LastDamageAmount;

	public bool ActorActionHasHappened { get; set; }

	public bool HasTakenWoundDamageThisTurn { get; set; }

	public bool ExhaustAfterAction { get; set; }

	public CAbilityExtraTurn.EExtraTurnType PendingExtraTurnOfType
	{
		get
		{
			if (PendingExtraTurnOfTypeStack.Count <= 0)
			{
				return CAbilityExtraTurn.EExtraTurnType.None;
			}
			return PendingExtraTurnOfTypeStack.Peek();
		}
	}

	public Stack<CAbilityExtraTurn.EExtraTurnType> PendingExtraTurnOfTypeStack { get; set; }

	public CAbilityExtraTurn.EExtraTurnType TakingExtraTurnOfType
	{
		get
		{
			if (TakingExtraTurnOfTypeStack.Count <= 0)
			{
				return CAbilityExtraTurn.EExtraTurnType.None;
			}
			return TakingExtraTurnOfTypeStack.Peek();
		}
	}

	public Stack<CAbilityExtraTurn.EExtraTurnType> TakingExtraTurnOfTypeStack { get; set; }

	public bool IsTakingExtraTurn => TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None;

	public bool HasPendingExtraTurn => PendingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None;

	public bool IsDoomed { get; set; }

	public bool NoGoldDrop { get; set; }

	public bool PhasedOut { get; set; }

	public bool Deactivated { get; set; }

	public int ChosenModelIndex { get; set; }

	public float CrowFlyDistance { get; set; }

	public List<string> TeleportedToPropGuids { get; set; }

	public virtual bool Invulnerable => false;

	public virtual bool PierceInvulnerability => false;

	public virtual List<CAbility.EAbilityType> Immunities => null;

	public virtual bool Untargetable => false;

	public virtual bool DoesNotBlock => false;

	public virtual bool IgnoreActorCollision => false;

	public List<CObjectProp> CarriedQuestItems => m_CarriedQuestProps;

	public List<CCharacterResource> CharacterResources => m_CharacterResources;

	public List<CActor> ActorsAttackedThisRound => m_ActorsAttackedThisRound;

	public List<CAbility.EAbilityType> AbilityTypesPerformedThisTurn => m_AbilityTypesPerformedThisTurn;

	public List<CAbility.EAbilityType> AbilityTypesPerformedThisAction => m_AbilityTypesPerformedThisAction;

	public int CachedShieldValue { get; private set; }

	public bool CachedHasRetaliate { get; private set; }

	public bool CachedShieldNeutralized { get; private set; }

	public bool CachedHealingBlocked { get; private set; }

	public bool CachedFlying { get; private set; }

	public List<CActiveBonus> CachedDoomActiveBonuses { get; set; }

	public List<CActiveBonus> CachedAddDoomSlotActiveBonuses { get; set; }

	public List<CActiveBonus> CachedActiveItemEffectBonuses { get; set; }

	public List<CDisableCardActionActiveBonus> CachedDisableCardActionActiveBonuses { get; set; }

	public virtual bool IsDeadPlayer => false;

	public bool ItemLocked => CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ItemLock).Count > 0;

	public bool BlocksPathing
	{
		get
		{
			if (!PhasedOut && !Deactivated)
			{
				return !IgnoreActorCollision;
			}
			return false;
		}
	}

	public Point ArrayIndex
	{
		get
		{
			return m_ArrayIndex;
		}
		set
		{
			if (m_ArrayIndex != value)
			{
				m_ArrayIndex = value;
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
		}
	}

	public CActor KilledByActor
	{
		get
		{
			if (!string.IsNullOrEmpty(KilledByActorGuid))
			{
				return ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.ActorGuid == KilledByActorGuid);
			}
			return null;
		}
	}

	public int ID
	{
		get
		{
			if (this is CPlayerActor cPlayerActor)
			{
				return cPlayerActor.CharacterClass.ModelInstanceID;
			}
			if (this is CEnemyActor cEnemyActor)
			{
				return cEnemyActor.StandeeID;
			}
			if (this is CHeroSummonActor cHeroSummonActor)
			{
				return cHeroSummonActor.StandeeID;
			}
			throw new Exception("Invalid Actor type.  Unable to get ID for actor " + Class.ID);
		}
	}

	public List<CPreventDamageActiveBonus> PreventDamageActiveAllSources => (from s in CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.PreventDamage, this)
		select s as CPreventDamageActiveBonus into w
		where (!w.HasTracker) ? (!w.PreventDamageAttackSourcesOnly) : (w.Remaining > 0)
		select w).ToList();

	public List<CPreventDamageActiveBonus> PreventDamageActiveAttackSourcesOnly => (from s in CharacterClassManager.FindAllActiveBonuses(CAbility.EAbilityType.PreventDamage, this)
		select s as CPreventDamageActiveBonus into w
		where (!w.HasTracker) ? w.PreventDamageAttackSourcesOnly : (w.Remaining > 0)
		select w).ToList();

	public List<CAbility.EAttackType> AttackTypeImmunities
	{
		get
		{
			List<CAbility.EAttackType> list = new List<CAbility.EAttackType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item.Ability is CAbilityImmunityTo cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAttackType immuneToAttackType in cAbilityImmunityTo.ImmuneToAttackTypes)
				{
					if (!list.Contains(immuneToAttackType))
					{
						list.Add(immuneToAttackType);
					}
				}
			}
			return list;
		}
	}

	public bool IsMonsterType
	{
		get
		{
			if (m_Type != EType.Enemy && m_Type != EType.Ally && m_Type != EType.Enemy2)
			{
				return m_Type == EType.Neutral;
			}
			return true;
		}
	}

	public bool IsEnemyMonsterType
	{
		get
		{
			if (m_Type != EType.Enemy)
			{
				return m_Type == EType.Enemy2;
			}
			return true;
		}
	}

	public bool IsOriginalMonsterType
	{
		get
		{
			if (m_OriginalType != EType.Enemy && m_OriginalType != EType.Ally && m_Type != EType.Enemy2)
			{
				return m_OriginalType == EType.Neutral;
			}
			return true;
		}
	}

	public bool IsUnderMyControl { get; set; }

	public virtual CClass ActiveBonusClass()
	{
		return m_Class;
	}

	public CActor(EType type, CClass actorClass, Point startArrayIndex, int currentHealth, int maxHealth, int level, string actorGuid, int chosenModelIndex)
	{
		if (actorGuid == null)
		{
			if (ScenarioManager.CurrentScenarioState != null)
			{
				bool flag = false;
				while (!flag)
				{
					ActorGuid = ScenarioManager.CurrentScenarioState.GetGUIDBasedOnGuidRNGState().ToString();
					if (ScenarioManager.CurrentScenarioState.ActorStates.Count <= 0 || ScenarioManager.CurrentScenarioState.ActorStates.Find((ActorState s) => s.ActorGuid == ActorGuid) == null)
					{
						flag = true;
					}
				}
			}
			else
			{
				ActorGuid = Guid.NewGuid().ToString();
			}
			m_PlayedThisRound = false;
			m_Gold = 0;
			m_XP = 0;
		}
		else
		{
			ActorGuid = actorGuid;
		}
		m_Type = type;
		m_OriginalType = type;
		m_Class = actorClass;
		ChosenModelIndex = chosenModelIndex;
		m_StartArrayIndex = startArrayIndex;
		m_ArrayIndex = startArrayIndex;
		m_Health = currentHealth;
		MaxHealth = maxHealth;
		m_Level = level;
		m_Inventory = new CInventory(m_Level, this);
		m_Tokens = new CTokens(this);
		Augments = new List<CAugment>();
		m_BaseAugmentSlots = 1;
		Songs = new List<CSong>();
		m_BaseSongSlots = 1;
		Dooms = new List<CDoom>();
		CachedRemovedOnDeathDooms = new List<CDoom>();
		m_BaseDoomSlots = 1;
		DoomSlots = m_BaseDoomSlots;
		CachedDoomActiveBonuses = new List<CActiveBonus>();
		CachedAddDoomSlotActiveBonuses = new List<CActiveBonus>();
		CachedActiveItemEffectBonuses = new List<CActiveBonus>();
		CachedDisableCardActionActiveBonuses = new List<CDisableCardActionActiveBonus>();
		TeleportedToPropGuids = new List<string>();
		CauseOfDeath = ECauseOfDeath.StillAlive;
		ExhaustAfterAction = false;
		PhasedOut = false;
		TakingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
		PendingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
		bool flag2 = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				flag2 = cEnemyActor.IsSummon;
			}
		}
		EType type2 = Type;
		string actorGuid2 = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level2 = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		bool isSummon = flag2;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorInitialized, type2, actorGuid2, iD, health, gold, xP, level2, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, isSummon, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnEnemySummon: false, null, null, "", doNotSerialize: true, originalMaxHealth));
	}

	public void AddAugmentOrSong(CAbility ability, CActor caster, int? id = null, int? activeBonusStartRound = null)
	{
		int? activeBonusStartRound2;
		if (ability.Augment != null)
		{
			CBaseCard cBaseCard = null;
			if (!Augments.Contains(ability.Augment))
			{
				if (Augments.Count >= AugmentSlots)
				{
					if (AugmentSlots != 1)
					{
						CActiveBonusAugmentSlotChoice_MessageData message = new CActiveBonusAugmentSlotChoice_MessageData(this)
						{
							m_Actor = this,
							m_Ability = ability
						};
						ScenarioRuleClient.MessageHandler(message);
						GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
						return;
					}
					CachedRemovedAugment = Augments[0];
					CachedRemovedAugmentActiveBonus = CachedRemovedAugment.RegisteredBaseCard.GetAugmentActiveBonus(CachedRemovedAugment);
					RemoveAugment(Augments[0]);
					Augments.Add(ability.Augment);
				}
				else
				{
					Augments.Add(ability.Augment);
				}
				cBaseCard = FindCardWithAbility(ability);
				if (cBaseCard != null)
				{
					ability.Augment.RegisteredBaseCard = cBaseCard;
					CBaseCard cBaseCard2 = cBaseCard;
					activeBonusStartRound2 = activeBonusStartRound;
					cBaseCard2.AddActiveBonus(ability, this, caster, id, null, isAugment: true, isSong: false, loadingItemBonus: false, isDoom: false, null, activeBonusStartRound2);
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(CAbility.EAbilityType.Augment, ESESubTypeAbility.AbilityEnded, cBaseCard?.Name, cBaseCard?.ID ?? int.MaxValue, cBaseCard?.CardType ?? CBaseCard.ECardType.None, Class.ID, 1, new List<CAbility>(), new List<CAbility>(), this?.Type, IsSummon: false, Tokens.CheckPositiveTokens, Tokens.CheckNegativeTokens, Class.ID, Type, ActedOnIsSummon: false, null, null));
		}
		if (ability.Song == null)
		{
			return;
		}
		if (Songs.Count >= SongSlots)
		{
			if (SongSlots != 1)
			{
				CActiveBonusSongSlotChoice_MessageData message2 = new CActiveBonusSongSlotChoice_MessageData(this)
				{
					m_Actor = this,
					m_Ability = ability
				};
				ScenarioRuleClient.MessageHandler(message2);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
				return;
			}
			CachedRemovedSong = Songs[0];
			CachedRemovedSongActiveBonus = CachedRemovedSong.RegisteredBaseCard.GetSongActiveBonus(CachedRemovedSong);
			RemoveSong(Songs[0]);
			Songs.Add(ability.Song);
		}
		else
		{
			Songs.Add(ability.Song);
		}
		CBaseCard cBaseCard3 = FindCardWithAbility(ability);
		ability.Song.RegisteredBaseCard = cBaseCard3;
		activeBonusStartRound2 = activeBonusStartRound;
		cBaseCard3.AddActiveBonus(ability, this, caster, id, null, isAugment: false, isSong: true, loadingItemBonus: false, isDoom: false, null, activeBonusStartRound2);
	}

	public void AddDoom(CDoom doom, CActor doomTarget, bool addDoomActiveBonuses = true, bool bonusCheck = true, bool removeAllDomsIfTargetChanged = true)
	{
		if (Dooms.Contains(doom))
		{
			return;
		}
		if (removeAllDomsIfTargetChanged && DoomTarget != null && DoomTarget != doomTarget)
		{
			RemoveAllDooms();
		}
		DoomTarget = doomTarget;
		doomTarget.IsDoomed = true;
		if (Dooms.Count >= DoomSlots)
		{
			if (DoomSlots != 1)
			{
				CActiveBonusDoomSlotChoice_MessageData message = new CActiveBonusDoomSlotChoice_MessageData(this)
				{
					m_DoomTargetActor = doomTarget,
					m_NewDoom = doom
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
				return;
			}
			RemoveDoom(Dooms[0]);
			Dooms.Add(doom);
			doomTarget.IsDoomed = true;
		}
		else
		{
			Dooms.Add(doom);
		}
		if (!addDoomActiveBonuses)
		{
			return;
		}
		foreach (CAbility doomAbility in doom.DoomAbilities)
		{
			CBaseCard cBaseCard = FindCardWithAbility(doomAbility);
			if (cBaseCard == null && this is CHeroSummonActor cHeroSummonActor)
			{
				cBaseCard = cHeroSummonActor.BaseCard;
			}
			if (cBaseCard != null)
			{
				CBaseCard cBaseCard2 = cBaseCard;
				bool bonusCheck2 = bonusCheck;
				cBaseCard2.AddActiveBonus(doomAbility, doomTarget, this, null, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: true, null, null, bonusCheck2);
			}
		}
	}

	public void ReplaceAugment(CAbility newAugmentAbility, CAugment augmentToBeReplaced, CActor caster)
	{
		if (Augments.Contains(augmentToBeReplaced))
		{
			CachedRemovedAugment = augmentToBeReplaced;
			CachedRemovedAugmentActiveBonus = CachedRemovedAugment.RegisteredBaseCard.GetAugmentActiveBonus(CachedRemovedAugment);
			RemoveAugment(augmentToBeReplaced);
			AddAugmentOrSong(newAugmentAbility, caster);
		}
	}

	public void ReplaceSong(CAbility newSongAbility, CSong songToBeReplaced, CActor caster)
	{
		if (Songs.Contains(songToBeReplaced))
		{
			CachedRemovedSong = songToBeReplaced;
			CachedRemovedSongActiveBonus = CachedRemovedSong.RegisteredBaseCard.GetSongActiveBonus(CachedRemovedSong);
			RemoveSong(songToBeReplaced);
			AddAugmentOrSong(newSongAbility, caster);
		}
	}

	public void ReplaceDoom(CDoom newDoom, CDoom doomToBeReplaced, CActor doomTarget)
	{
		if (Dooms.Contains(doomToBeReplaced))
		{
			if (CachedRemovedOnDeathDooms.Contains(newDoom))
			{
				CachedRemovedOnDeathDooms.Remove(newDoom);
			}
			RemoveDoom(doomToBeReplaced);
			AddDoom(newDoom, doomTarget);
		}
	}

	public void TransferAllDooms(CActor doomTarget)
	{
		for (int num = Dooms.Count - 1; num >= 0; num--)
		{
			TransferDoom(Dooms[num], doomTarget);
		}
	}

	public void TransferDoom(CDoom doomToTransfer, CActor doomTarget)
	{
		if (!Dooms.Contains(doomToTransfer))
		{
			return;
		}
		if (CachedRemovedOnDeathDooms.Contains(doomToTransfer))
		{
			CachedRemovedOnDeathDooms.Remove(doomToTransfer);
		}
		Dooms.Remove(doomToTransfer);
		DoomTarget.IsDoomed = false;
		foreach (CActiveBonus item in CharacterClassManager.FindAllDoomActiveBonuses(null, checkDeadActor: true))
		{
			if (doomToTransfer.DoomAbilities.Contains(item.Ability))
			{
				item.Finish();
			}
		}
		AddDoom(doomToTransfer, doomTarget, addDoomActiveBonuses: true, bonusCheck: true, removeAllDomsIfTargetChanged: false);
		(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public void UndoAugment(CAugment previousAugment)
	{
		if (!Augments.Contains(previousAugment))
		{
			return;
		}
		RemoveAugment(previousAugment, undo: true);
		if (CachedRemovedAugment != null)
		{
			Augments.Add(CachedRemovedAugment);
			CachedRemovedAugmentActiveBonus.UndoFinish();
			CachedRemovedAugment.RegisteredBaseCard.AddActiveBonus(CachedRemovedAugmentActiveBonus);
			if (Class is CCharacterClass cCharacterClass && CachedRemovedAugment.RegisteredBaseCard is CAbilityCard roundAbilityCard)
			{
				cCharacterClass.RestoreCachedAugmentOrSongAbilityCard(roundAbilityCard);
			}
			ClearCharacterSpecialMechanicsCache(clearAugments: true);
		}
	}

	public void UndoSong(CSong previousSong)
	{
		if (!Songs.Contains(previousSong))
		{
			return;
		}
		RemoveSong(previousSong);
		if (CachedRemovedSong != null)
		{
			Songs.Add(CachedRemovedSong);
			CachedRemovedSongActiveBonus.UndoFinish();
			CachedRemovedSong.RegisteredBaseCard.AddActiveBonus(CachedRemovedSongActiveBonus);
			if (Class is CCharacterClass cCharacterClass && CachedRemovedSong.RegisteredBaseCard is CAbilityCard roundAbilityCard)
			{
				cCharacterClass.RestoreCachedAugmentOrSongAbilityCard(roundAbilityCard);
			}
			ClearCharacterSpecialMechanicsCache(clearAugments: true);
		}
	}

	public void ClearCharacterSpecialMechanicsCache(bool clearAugments = false, bool clearSongs = false, bool clearDooms = false)
	{
		if (clearAugments)
		{
			CachedRemovedAugment = null;
			CachedRemovedAugmentActiveBonus = null;
		}
		if (clearSongs)
		{
			CachedRemovedSong = null;
			CachedRemovedSongActiveBonus = null;
		}
		if (!clearDooms)
		{
			return;
		}
		foreach (CDoom cachedRemovedOnDeathDoom in CachedRemovedOnDeathDooms)
		{
			RemoveDoom(cachedRemovedOnDeathDoom);
		}
		CachedRemovedOnDeathDooms.Clear();
	}

	public void RemoveAugment(CAugment augment, bool undo = false)
	{
		CAugment activeAugment = augment;
		CAugment cAugment = null;
		foreach (CAugment augment2 in Augments)
		{
			if (augment2 == augment)
			{
				cAugment = augment2;
			}
			else if (augment.AugmentType == CAugment.EAugmentType.Bonus && augment2.Abilities != null && augment2.Abilities[0].Augment == augment)
			{
				cAugment = augment2;
			}
		}
		if (cAugment == null)
		{
			return;
		}
		Augments.Remove(cAugment);
		if (augment.AugmentType == CAugment.EAugmentType.Bonus)
		{
			List<CAbility> abilities = augment.Abilities;
			if (abilities != null && abilities.Count > 0)
			{
				activeAugment = augment.Abilities[0].Augment;
			}
		}
		CActiveBonus cActiveBonus = (Class as CCharacterClass).FindActiveBonuses(this).SingleOrDefault((CActiveBonus s) => s.Ability.Augment == activeAugment);
		if (cActiveBonus != null)
		{
			if (undo)
			{
				augment.RegisteredBaseCard.ActiveBonuses.Remove(cActiveBonus);
				if (Class is CCharacterClass cCharacterClass && cCharacterClass.ActivatedCards.Contains(cActiveBonus.BaseCard))
				{
					cCharacterClass.ActivatedCards.Remove(cActiveBonus.BaseCard);
				}
			}
			else
			{
				cActiveBonus.Finish();
				(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
				}
			}
		}
		cAugment.RegisteredBaseCard?.RemoveAugmentActiveBonus(activeAugment);
	}

	public void RemoveSong(CSong song)
	{
		if (!Songs.Contains(song))
		{
			return;
		}
		Songs.Remove(song);
		CActiveBonus cActiveBonus = (Class as CCharacterClass).FindActiveBonuses(this).SingleOrDefault((CActiveBonus s) => s.Ability.Song == song);
		if (cActiveBonus != null)
		{
			cActiveBonus.Finish();
			(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
		song.RegisteredBaseCard.RemoveSongActiveBonus(song);
	}

	public void RemoveDoom(CDoom doom)
	{
		if (!Dooms.Contains(doom))
		{
			return;
		}
		Dooms.Remove(doom);
		DoomTarget.IsDoomed = false;
		foreach (CActiveBonus item in CharacterClassManager.FindAllDoomActiveBonuses(null, checkDeadActor: true))
		{
			if (!doom.DoomAbilities.Contains(item.Ability))
			{
				continue;
			}
			item.Finish();
			(Class as CCharacterClass).CheckForFinishedActiveBonuses(this);
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
	}

	public void RemoveAllDooms()
	{
		for (int num = Dooms.Count - 1; num >= 0; num--)
		{
			RemoveDoom(Dooms[num]);
		}
	}

	public void RefreshCharacterSpecialMechanicSlots()
	{
		while (Augments.Count > AugmentSlots)
		{
			RemoveAugment(Augments[0]);
		}
		while (Songs.Count > SongSlots)
		{
			RemoveSong(Songs[0]);
		}
		while (Dooms.Count > DoomSlots)
		{
			RemoveDoom(Dooms[0]);
		}
	}

	public void OverrideAugmentSlots(int? overrideValue)
	{
		if (overrideValue.HasValue)
		{
			m_OverridedBaseAugmentSlots = overrideValue.Value;
		}
		else
		{
			m_OverridedBaseAugmentSlots = null;
		}
	}

	public virtual bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		startedOnDeathAbility = false;
		CauseOfDeath = causeOfDeath;
		if (CauseOfDeath == ECauseOfDeath.None || CauseOfDeath == ECauseOfDeath.StillAlive)
		{
			DLLDebug.LogError("Invalid cause of death when actor OnDeath entered");
			CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		if (causeOfDeath != ECauseOfDeath.ActorRemovedFromMap && !fromDeathAbilityComplete)
		{
			if (GameState.InternalCurrentActor != null && (targetingActor == null || targetingActor == GameState.InternalCurrentActor))
			{
				GameState.InternalCurrentActor.m_OnKillListeners?.Invoke(this, targetingActor, causeOfDeath, causeOfDeathAbility, onActorTurn: true);
			}
			else
			{
				targetingActor?.m_OnKillListeners?.Invoke(this, targetingActor, causeOfDeath, causeOfDeathAbility, onActorTurn: false);
			}
			bool flag = false;
			int num = 0;
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				flag = true;
				num = ((cPhaseAction.CurrentPhaseAbilities != null) ? cPhaseAction.CurrentPhaseAbilities.Count : 0);
			}
			m_OnDeathListeners?.Invoke(this, causeOfDeathAbility);
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbilities: not null } cPhaseAction2 && (!flag || num != cPhaseAction2.CurrentPhaseAbilities?.Count) && cPhaseAction2.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.TargetingActor == this))
			{
				startedOnDeathAbility = true;
			}
		}
		if (IsOriginalMonsterType)
		{
			CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
			foreach (CCharacterResource characterResource in CharacterResources)
			{
				if (characterResource.ResourceData.DropOnDeath && characterResource.Amount > 0)
				{
					CObjectResource prop = new CObjectResource(characterResource.ResourceData, characterResource.ResourceData.ResourceModel, ScenarioManager.ObjectImportType.Resource, new TileIndex(ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid);
					cTile.SpawnProp(prop);
				}
			}
		}
		List<CScenarioModifier> list = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FindAll((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.ActorsCreateGraves).ToList();
		if (list.Count > 0)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == ActorGuid);
			if (actorState != null && list.Any((CScenarioModifier x) => x.ScenarioModifierFilter.IsValidTarget(actorState)))
			{
				bool isEliteGrave = false;
				if (this is CEnemyActor cEnemyActor)
				{
					isEliteGrave = cEnemyActor.MonsterClass.NonEliteVariant != null;
				}
				CTile cTile2 = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
				CObjectProp prop2 = new CObjectMonsterGrave(isEliteGrave, EPropType.MonsterGrave.ToString(), ScenarioManager.ObjectImportType.MonsterGrave, new TileIndex(ArrayIndex), null, null, null, cTile2.m_HexMap.MapGuid);
				cTile2.SpawnProp(prop2);
			}
		}
		if (causeOfDeath == ECauseOfDeath.Trap)
		{
			targetingActor = GameState.InternalCurrentActor;
		}
		CBaseCard cBaseCard = ((causeOfDeathAbility != null) ? targetingActor.FindCardWithAbility(causeOfDeathAbility) : null);
		bool flag2 = false;
		string monsterType = "";
		if (OriginalType == EType.Enemy && this is CEnemyActor cEnemyActor2)
		{
			flag2 = cEnemyActor2.IsSummon;
			monsterType = cEnemyActor2.MonsterClass.MonsterType.ToString();
		}
		bool flag3 = false;
		if (targetingActor != null && targetingActor.OriginalType == EType.Enemy && targetingActor is CEnemyActor cEnemyActor3)
		{
			flag3 = cEnemyActor3.IsSummon;
		}
		List<string> list2 = new List<string>();
		if (targetingActor != null && targetingActor.OriginalType == EType.Player)
		{
			foreach (CItem allItem in targetingActor.Inventory.AllItems)
			{
				if (allItem.SlotState == CItem.EItemSlotState.Locked)
				{
					list2.Add(allItem.YMLData.StringID);
				}
			}
		}
		string text = targetingActor?.Class.ID;
		EType? eType = targetingActor?.OriginalType;
		KilledByActorGuid = targetingActor?.ActorGuid;
		GameState.UpdateActorsKilledThisRoundAndTurn(this);
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction3 && cPhaseAction3.CurrentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && cAbilityCard != null && cAbilityCard.ActiveBonuses?.Count > 0 && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
		{
			text = cAbilityCard.ClassID;
			eType = EType.Player;
		}
		bool attackerDisadvantage = attackSummary != null && attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage;
		bool targetAdjacent = targetingActor != null && ScenarioManager.IsTileAdjacent(ArrayIndex.X, ArrayIndex.Y, targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y);
		GameState.GetAdjacency(targetingActor, out var wall, out var obstacles, out var allies, out var enemies);
		EType originalType = OriginalType;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath2 = CauseOfDeath;
		bool isSummon = flag2;
		string actedOnByGUID = targetingActor?.ActorGuid;
		string actedOnByClassID = text;
		EType? actedOnType = eType;
		int cardID = cBaseCard?.ID ?? int.MaxValue;
		CBaseCard.ECardType cardType = cBaseCard?.CardType ?? CBaseCard.ECardType.None;
		CAbility.EAbilityType abilityType = causeOfDeathAbility?.AbilityType ?? CAbility.EAbilityType.None;
		string actingAbilityName = ((cBaseCard != null) ? cBaseCard.Name : "");
		int abilityStrength = causeOfDeathAbility?.Strength ?? 0;
		bool actedOnEnemySummon = flag3;
		List<PositiveConditionPair> actedOnPositiveConditions = targetingActor?.Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> actedOnNegativeConditions = targetingActor?.Tokens.CheckNegativeTokens;
		int lastDamageAmount = LastDamageAmount;
		int preDamageHealth = PreDamageHealth;
		List<string> items = list2;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorOnDeath, originalType, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath2, isSummon, actedOnByGUID, actedOnByClassID, actedOnType, cardID, cardType, abilityType, actingAbilityName, abilityStrength, actedOnEnemySummon, actedOnPositiveConditions, actedOnNegativeConditions, "", doNotSerialize: false, OriginalMaxHealth, lastDamageAmount, preDamageHealth, items, IsDoomed, monsterType, attackerDisadvantage, targetAdjacent, allies, enemies, obstacles, wall, CharacterHasResourceAny("Favorite", 1)));
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		return !startedOnDeathAbility;
	}

	protected virtual void OnHealthChanged()
	{
	}

	public virtual string GetPrefabName()
	{
		return m_Class.Models[ChosenModelIndex];
	}

	public virtual int Initiative()
	{
		return 0;
	}

	public virtual int SubInitiative()
	{
		return 0;
	}

	public virtual void CalculateAttackStrengthForUI()
	{
	}

	public virtual string ActorLocKey()
	{
		return Class.LocKey;
	}

	public void Damaged(int strength, bool fromAttackAbility, CActor damageSource, CAbility damagingAbility)
	{
		if (strength > 0)
		{
			m_LastAbilityDamagedBy = damagingAbility;
			m_PreDamageHealth = Health;
			m_LastDamageAmount = strength;
			Health -= strength;
			CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(damageSource);
			cActorBeenDamaged_MessageData.m_ActorBeingDamaged = this;
			cActorBeenDamaged_MessageData.m_DamageAbility = damagingAbility as CAbilityDamage;
			cActorBeenDamaged_MessageData.m_ActualDamage = strength;
			cActorBeenDamaged_MessageData.m_ActorOriginalHealth = m_PreDamageHealth;
			cActorBeenDamaged_MessageData.m_ActorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
			if (Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep);
				CActorAwakened_MessageData message = new CActorAwakened_MessageData(this);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public CBaseCard FindCardWithAbility(CAbility ability)
	{
		CClass cClass = m_Class;
		if (ability.IsControlAbility && ability.ControllingActor != null)
		{
			cClass = ability.ControllingActor.m_Class;
		}
		return cClass.FindCardWithAbility(ability, this);
	}

	public CBaseCard FindCard(int id, string name)
	{
		return m_Class.FindCard(id, name);
	}

	public List<CActiveBonus> FindApplicableActiveBonuses(CAbility.EAbilityType type, CActiveBonus.EActiveBonusBehaviourType behaviourType = CActiveBonus.EActiveBonusBehaviourType.None)
	{
		return CActiveBonus.FindApplicableActiveBonuses(this, type, behaviourType);
	}

	public int CalculateShield(CAbility abilityToShieldFrom = null, bool ignoreNeutralizeShield = false, bool includeItemShieldValues = false)
	{
		if (!ignoreNeutralizeShield && FindApplicableActiveBonuses(CAbility.EAbilityType.NeutralizeShield).Count > 0)
		{
			return 0;
		}
		int num = BaseShield();
		List<CActiveBonus> applicableActiveShieldBonuses = (from it in FindApplicableActiveBonuses(CAbility.EAbilityType.Shield)
			where !it.IsSong && !it.Ability.ActiveBonusData.IsToggleBonus
			select it).ToList();
		applicableActiveShieldBonuses.AddRange((from it in CharacterClassManager.FindAllSongActiveBonuses(this)
			where it.Ability.AbilityType == CAbility.EAbilityType.Shield && it.Ability.ActiveBonusData.Filter.IsValidTarget(this, it.Actor, isTargetedAbility: false, MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false) && !applicableActiveShieldBonuses.Contains(it)
			select it).ToList());
		if (applicableActiveShieldBonuses != null)
		{
			num += applicableActiveShieldBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(abilityToShieldFrom, this));
		}
		if (includeItemShieldValues)
		{
			num += Inventory.GetItemShieldValue();
		}
		CachedShieldValue = num;
		CachedFlying = Flying;
		return num;
	}

	public bool WillActorDie(int incomingDamage, int pierce, CAbility ability = null)
	{
		int num = Math.Max(0, incomingDamage - Math.Max(0, CalculateShield(ability) - pierce));
		if (OriginalType == EType.Player)
		{
			return false;
		}
		return Health - num <= 0;
	}

	public int CalculateRetaliate(CAbility ability, int distanceToEnemy)
	{
		List<CActiveBonus> usedActiveBonuses;
		return CalculateRetaliate(ability, distanceToEnemy, out usedActiveBonuses);
	}

	public int CalculateRetaliate(CAbility ability, int distanceToEnemy, out List<CActiveBonus> usedActiveBonuses)
	{
		usedActiveBonuses = new List<CActiveBonus>();
		List<CActiveBonus> applicableActiveRetaliateBonuses = (from it in FindApplicableActiveBonuses(CAbility.EAbilityType.Retaliate)
			where !it.IsSong
			select it).ToList();
		applicableActiveRetaliateBonuses.AddRange((from it in CharacterClassManager.FindAllSongActiveBonuses(this)
			where it.Ability.AbilityType == CAbility.EAbilityType.Retaliate && it.Ability.ActiveBonusData.Filter.IsValidTarget(this, it.Actor, isTargetedAbility: false, MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false) && !applicableActiveRetaliateBonuses.Contains(it)
			select it).ToList());
		int num = BaseRetaliate(distanceToEnemy);
		if (applicableActiveRetaliateBonuses != null)
		{
			foreach (CActiveBonus item in applicableActiveRetaliateBonuses.Where((CActiveBonus x) => !(x.BespokeBehaviour is CRetaliateActiveBonus_BuffRetaliate)))
			{
				if (!(item.Ability is CAbilityRetaliate cAbilityRetaliate) || distanceToEnemy > cAbilityRetaliate.RetaliateRange)
				{
					continue;
				}
				int num2 = 0;
				usedActiveBonuses.Add(item);
				foreach (CActiveBonus item2 in applicableActiveRetaliateBonuses.Where((CActiveBonus x) => x.BespokeBehaviour is CRetaliateActiveBonus_BuffRetaliate))
				{
					if (item2.Ability is CAbilityRetaliate cAbilityRetaliate2 && distanceToEnemy <= cAbilityRetaliate2.RetaliateRange)
					{
						num2 += item2.ReferenceStrength(ability, ability.TargetingActor);
					}
				}
				num += item.ReferenceStrength(ability, ability.TargetingActor) + num2;
			}
		}
		CachedHasRetaliate = applicableActiveRetaliateBonuses.Count > 0 || BaseRetaliate(int.MaxValue) > 0;
		return num;
	}

	public void CheckForCachedValuesAfterActiveBonusesUpdate()
	{
		CalculateShield();
		List<CActiveBonus> applicableActiveRetaliateBonuses = (from it in FindApplicableActiveBonuses(CAbility.EAbilityType.Retaliate)
			where !it.IsSong
			select it).ToList();
		applicableActiveRetaliateBonuses.AddRange((from it in CharacterClassManager.FindAllSongActiveBonuses(this)
			where it.Ability.AbilityType == CAbility.EAbilityType.Retaliate && it.Ability.ActiveBonusData.Filter.IsValidTarget(this, it.Actor, isTargetedAbility: false, MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction, false) && !applicableActiveRetaliateBonuses.Contains(it)
			select it).ToList());
		CachedHasRetaliate = applicableActiveRetaliateBonuses.Count > 0 || BaseRetaliate(int.MaxValue) > 0;
		CachedHealingBlocked = FindApplicableActiveBonuses(CAbility.EAbilityType.BlockHealing).Count > 0;
		CachedShieldNeutralized = FindApplicableActiveBonuses(CAbility.EAbilityType.NeutralizeShield).Count > 0;
		CachedDoomActiveBonuses = CharacterClassManager.FindAllDoomActiveBonuses(this);
		foreach (CActiveBonus item2 in FindApplicableActiveBonuses(CAbility.EAbilityType.DisableCardAction))
		{
			if (item2 is CDisableCardActionActiveBonus item)
			{
				CachedDisableCardActionActiveBonuses.Add(item);
			}
		}
		if (IsOriginalMonsterType)
		{
			CachedActiveItemEffectBonuses = (from x in CActiveBonus.FindAllActiveBonuses()
				where x.BaseCard != null && x.Actor == this && x.Caster != x.Actor && x.BaseCard.CardType == CBaseCard.ECardType.Item
				select x).Concat(CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttacked)).Distinct().ToList();
		}
		CachedAddDoomSlotActiveBonuses = FindApplicableActiveBonuses(CAbility.EAbilityType.AddDoomSlots).ToList();
		DoomSlots = m_BaseDoomSlots + CachedAddDoomSlotActiveBonuses.Where((CActiveBonus w) => w.Ability != null).Sum((CActiveBonus s) => s.Ability.Strength);
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(this);
		ScenarioRuleClient.MessageHandler(message);
	}

	public CActor ApplyRedirector(CAbility ability, CActor originalTargetActor)
	{
		List<CActiveBonus> list = FindApplicableActiveBonuses(CAbility.EAbilityType.Redirect);
		CActor result = originalTargetActor;
		if (list != null && list.Count > 0)
		{
			result = ((CRedirectActiveBonus)list[0]).ApplyRedirect(ability, originalTargetActor);
		}
		bool flag = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				flag = cEnemyActor.IsSummon;
			}
		}
		EType type = Type;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		bool isSummon = flag;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorApplyRedirector, type, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, isSummon, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnEnemySummon: false, null, null, "", doNotSerialize: false, originalMaxHealth));
		return result;
	}

	public void ApplyRetaliateToAttack(CActor actorBeingAttacked, CAbility ability, int? retaliateBuffOverride = null)
	{
		if (IsUsingOnDeathAbility)
		{
			return;
		}
		bool foundPath;
		List<Point> list = ScenarioManager.PathFinder.FindPath(ArrayIndex, actorBeingAttacked.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (foundPath)
		{
			List<CActiveBonus> usedActiveBonuses = new List<CActiveBonus>();
			int num = (retaliateBuffOverride.HasValue ? retaliateBuffOverride.Value : actorBeingAttacked.CalculateRetaliate(ability, list.Count, out usedActiveBonuses));
			if (num > 0)
			{
				actorBeingAttacked.m_OnRetaliateListeners?.Invoke();
				CTargetRetaliate_MessageData message = new CTargetRetaliate_MessageData(null)
				{
					m_ActorBeingAttacked = actorBeingAttacked,
					m_ActorAttacking = this,
					m_retaliateBuff = num
				};
				ScenarioRuleClient.MessageHandler(message);
				int health = Health;
				bool actorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(this, num, checkIfPlayerCanAvoidDamage: true, actorBeingAttacked, null, CAbility.EAbilityType.Retaliate);
				if (GameState.ActorHealthCheck(actorBeingAttacked, this, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(null)
					{
						m_ActorBeingDamaged = this,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				foreach (CActiveBonus item in usedActiveBonuses)
				{
					actorBeingAttacked.GainXP(item.ReferenceXP(ability, this));
				}
			}
		}
		bool flag = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				flag = cEnemyActor.IsSummon;
			}
		}
		EType type = Type;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health2 = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		bool isSummon = flag;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorApplyRetaliate, type, actorGuid, iD, health2, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, isSummon, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnEnemySummon: false, null, null, "", doNotSerialize: false, originalMaxHealth));
	}

	public virtual int BaseShield()
	{
		return 0;
	}

	public virtual int BaseRetaliate(int distanceToEnemy)
	{
		return 0;
	}

	public void Healed(int strength, bool ignoreTokens = false, bool report = true, bool actualHeal = false)
	{
		if (actualHeal)
		{
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.AddHeal))
			{
				CAbility ability = item.Ability;
				if (ability != null && ability.ActiveBonusData?.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal)
				{
					strength += item.ReferenceStrength(new CAbility(), this);
					m_OnBeingHealedListeners?.Invoke(new CAbilityHeal(CAbilityHeal.HealAbilityData.DefaultHealData()));
				}
			}
		}
		Healed(strength, this, out var _, out var _, out var _, ignoreTokens, report);
	}

	public void Healed(int strength, CActor actorHealing, out bool woundRemoved, out bool poisonRemoved, out int healedAmount, bool ignoreTokens = false, bool report = true)
	{
		woundRemoved = false;
		poisonRemoved = false;
		healedAmount = 0;
		if (!ignoreTokens && Tokens.HasKey(CCondition.ENegativeCondition.Wound))
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Wound);
			woundRemoved = true;
		}
		if (!ignoreTokens && Tokens.HasKey(CCondition.ENegativeCondition.Poison))
		{
			RemoveNegativeConditionToken(CCondition.ENegativeCondition.Poison);
			poisonRemoved = true;
		}
		else
		{
			if (MaxHealth - Health < strength)
			{
				healedAmount = MaxHealth - Health;
			}
			else
			{
				healedAmount = strength;
			}
			Health = Math.Min(Health + strength, MaxHealth);
		}
		m_OnAfterBeingHealedListeners?.Invoke();
		bool isSummon = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		if (report)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorHealed(poisonRemoved, woundRemoved, (!poisonRemoved) ? healedAmount : 0, OriginalType, ActorGuid, m_Class.ID, Health, Gold, XP, Level, Tokens.CheckPositiveTokens, Tokens.CheckNegativeTokens, PlayedThisRound, IsDead, CauseOfDeath, isSummon, actorHealing.ActorGuid, actorHealing.Class.ID, actorHealing.OriginalType, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", OriginalMaxHealth));
		}
	}

	public bool RemoveNegativeConditionToken(CCondition.ENegativeCondition negativeCondition, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.None)
	{
		bool num = Tokens.RemoveNegativeToken(negativeCondition, conditionDecrementTrigger);
		if (num)
		{
			NegativeConditionEndedOnActorListener onNegativeConditionEndedOnActorListeners = m_OnNegativeConditionEndedOnActorListeners;
			if (onNegativeConditionEndedOnActorListeners == null)
			{
				return num;
			}
			onNegativeConditionEndedOnActorListeners(negativeCondition, this);
		}
		return num;
	}

	public bool RemovePositiveConditionToken(CCondition.EPositiveCondition positiveCondition, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.None)
	{
		bool num = Tokens.RemovePositiveToken(positiveCondition, conditionDecrementTrigger);
		if (num)
		{
			PositiveConditionEndedOnActorListener onPositiveConditionEndedOnActorListeners = m_OnPositiveConditionEndedOnActorListeners;
			if (onPositiveConditionEndedOnActorListeners == null)
			{
				return num;
			}
			onPositiveConditionEndedOnActorListeners(positiveCondition, this);
		}
		return num;
	}

	public void ProcessConditionTokens(EConditionDecTrigger conditionDecrementTrigger)
	{
		Tokens.ProcessTokens(conditionDecrementTrigger, out var positiveConditionsRemoved, out var negativeConditionsRemoved);
		foreach (CCondition.EPositiveCondition item in positiveConditionsRemoved)
		{
			m_OnPositiveConditionEndedOnActorListeners?.Invoke(item, this);
		}
		foreach (CCondition.ENegativeCondition item2 in negativeConditionsRemoved)
		{
			m_OnNegativeConditionEndedOnActorListeners?.Invoke(item2, this);
		}
	}

	public virtual void ActionSelection()
	{
		m_AIMoveFocusPath = null;
		m_MovementPathSelected = false;
		m_AIMoveFocusActors.Clear();
		m_AIMoveFocusTiles.Clear();
		m_AIMoveRange = 0;
	}

	public virtual void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove = true, bool ignoreDifficultTerrain = false, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
	}

	public void Pull(Point pullToTarget, CActor pullToActor, int remainingPulls, CAbilityPull.EPullType pullType, out List<Point> points)
	{
		List<CTile> pullTiles = CAbilityPull.GetPullTiles(this, Type, ArrayIndex, pullToTarget, pullType, remainingPulls);
		if (pullTiles.Count > 0)
		{
			int index = pullTiles.Count - 1;
			points = FindCharacterPath(this, ArrayIndex, pullTiles[index].m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out var _);
			m_AIMoveFocusWaypoints = points.Select((Point x) => ScenarioManager.Tiles[x.X, x.Y]).ToList();
			ArrayIndex = pullTiles[index].m_ArrayIndex;
		}
		else
		{
			points = new List<Point>();
		}
	}

	public void Push(Point pushFromPoint, int remainingPushes, out List<Point> points, bool intoBlocked = false)
	{
		List<CTile> pushTiles = CAbilityPush.GetPushTiles(this, Type, ArrayIndex, pushFromPoint, remainingPushes, intoBlocked);
		if (pushTiles.Count > 0)
		{
			int index = pushTiles.Count - 1;
			points = FindCharacterPath(this, ArrayIndex, pushTiles[index].m_ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out var _);
			m_AIMoveFocusWaypoints = points.Select((Point x) => ScenarioManager.Tiles[x.X, x.Y]).ToList();
			ArrayIndex = pushTiles[index].m_ArrayIndex;
		}
		else
		{
			points = new List<Point>();
		}
	}

	public CActor Clone()
	{
		CActor cActor = (CActor)MemberwiseClone();
		cActor.Inventory.InventoryActor = cActor;
		return cActor;
	}

	public void AddGold(int amount)
	{
		m_Gold += amount;
		DLLDebug.LogInfo(Class.ID + " picked up " + amount + " gold.");
	}

	public void CarryQuestItem(CObjectProp questItemProp)
	{
		if (questItemProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
		{
			if (!m_CarriedQuestProps.Contains(questItemProp))
			{
				m_CarriedQuestProps.Add(questItemProp);
			}
		}
		else
		{
			DLLDebug.LogError("Attempted to carry a non-quest item prop");
		}
	}

	public void DropQuestItem(CObjectProp questItemProp)
	{
		if (m_CarriedQuestProps.Contains(questItemProp))
		{
			float y = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
			CVector3 rotation = new CVector3(0f, y, 0f);
			CTile cTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
			CMap hexMap = cTile.m_HexMap;
			questItemProp.SetNewStartingMapGuid(hexMap.MapGuid);
			ScenarioManager.CurrentScenarioState.Props.Add(questItemProp);
			ScenarioManager.CurrentScenarioState.ActivatedProps.Remove(questItemProp);
			questItemProp.Deactivate();
			questItemProp.SetLocation(new TileIndex(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y), null, rotation);
			cTile.SpawnProp(questItemProp, notifyClient: true, 0.5f);
			m_CarriedQuestProps.Remove(questItemProp);
		}
		else
		{
			DLLDebug.LogError("Attempted to drop a quest item not in the carried quest item list");
		}
	}

	public void AddCharacterResource(string resourceID, int amountToAdd)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			cCharacterResource.Amount += amountToAdd;
		}
		else
		{
			CharacterResources.Add(new CCharacterResource(resourceID, amountToAdd));
		}
	}

	public void RemoveCharacterResource(string resourceID, int amountToRemove)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			cCharacterResource.Amount -= amountToRemove;
		}
		else
		{
			DLLDebug.LogError("Can't remove a character resource that the player has none of");
		}
	}

	public bool CharacterHasResource(string resourceID, int amountRequired)
	{
		CCharacterResource cCharacterResource = CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
		if (cCharacterResource != null)
		{
			return cCharacterResource.Amount >= amountRequired;
		}
		return false;
	}

	public bool CharacterHasResourceAny(string resourceID, int amountRequired)
	{
		List<CCharacterResource> list = CharacterResources.FindAll((CCharacterResource x) => x.ID.Contains(resourceID));
		if (list.Count > 0)
		{
			return list.Any((CCharacterResource x) => x.Amount >= amountRequired);
		}
		return false;
	}

	public bool LootTile(CTile tile, bool asPartOfAbility = true, ScenarioManager.ObjectImportType objectType = ScenarioManager.ObjectImportType.None)
	{
		bool result = false;
		CActor cActor = null;
		if (tile != null)
		{
			foreach (CObjectProp item in tile.m_Props.Where((CObjectProp p) => p.IsLootable && p.CanActorLoot(this) && (asPartOfAbility || !p.IgnoreEndOfTurnLooting)).ToList())
			{
				cActor = this;
				if (this is CHeroSummonActor cHeroSummonActor)
				{
					cActor = cHeroSummonActor.Summoner;
				}
				if (objectType != ScenarioManager.ObjectImportType.None && objectType != item.ObjectType)
				{
					continue;
				}
				switch (item.ObjectType)
				{
				case ScenarioManager.ObjectImportType.Chest:
				case ScenarioManager.ObjectImportType.GoalChest:
					if (OriginalType == EType.Player || (this is CHeroSummonActor && !(this is CHeroSummonActor { IsCompanionSummon: false })))
					{
						result = true;
						item.Activate(this, cActor);
						tile.m_Props.Remove(item);
					}
					break;
				case ScenarioManager.ObjectImportType.MoneyToken:
				{
					int amount = ((ScenarioManager.Scenario.SLTE == null) ? 1 : ScenarioManager.Scenario.SLTE.GoldConversion);
					cActor.AddGold(amount);
					result = true;
					item.Activate(this);
					tile.m_Props.Remove(item);
					break;
				}
				case ScenarioManager.ObjectImportType.CarryableQuestItem:
					if (OriginalType == EType.Player || (this is CHeroSummonActor && !(this is CHeroSummonActor { IsCompanionSummon: false })))
					{
						cActor.CarryQuestItem(item);
						item.Activate(this);
						tile.m_Props.Remove(item);
					}
					break;
				case ScenarioManager.ObjectImportType.Resource:
					if (OriginalType == EType.Player || (this is CHeroSummonActor && !(this is CHeroSummonActor { IsCompanionSummon: false })))
					{
						CObjectResource cObjectResource = (CObjectResource)item;
						cActor.AddCharacterResource(cObjectResource.ResourceData.ID, 1);
						item.Activate(this);
						tile.m_Props.Remove(item);
					}
					break;
				}
				GameState.GetAdjacency(cActor, out var wall, out var obstacles, out var allies, out var enemies);
				ScenarioManager.ObjectImportType objectType2 = item.ObjectType;
				EType actorType = cActor?.Type ?? OriginalType;
				string actorGuid = ActorGuid;
				string actorClass = ((cActor != null) ? cActor.Class.ID : m_Class.ID);
				int health = Health;
				int gold = Gold;
				int xP = XP;
				int level = Level;
				List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
				List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
				bool playedThisRound = PlayedThisRound;
				bool isDead = IsDead;
				ECauseOfDeath causeOfDeath = CauseOfDeath;
				string ownerGUID = item.OwnerGUID;
				int originalMaxHealth = OriginalMaxHealth;
				int allyAdjacent = allies;
				int enemyAdjacent = enemies;
				int obstacleAdjacent = obstacles;
				bool wallAdjacent = wall;
				SEventLogMessageHandler.AddEventLogMessage(new SEventActorLooted(objectType2, actorType, actorGuid, actorClass, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, ownerGUID, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", originalMaxHealth, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent));
			}
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		return result;
	}

	public void UsedItem(CItem item, bool firstTimeUse)
	{
		string stringID = item.YMLData.StringID;
		string slot = item.YMLData.Slot.ToString();
		EType originalType = OriginalType;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorUsedItem(stringID, slot, firstTimeUse, originalType, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", originalMaxHealth));
	}

	public virtual EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage)
	{
		return Tokens.GetAdvantageStatus(addAdvantage, addDisadvantage);
	}

	public void GainXP(int xpAmount)
	{
		if (xpAmount > 0)
		{
			m_XP += xpAmount;
			DLLDebug.LogInfo(m_Class.ID + " has gained " + xpAmount + "xp.  " + m_Class.ID + " Total XP: " + m_XP);
			int xpEarned = xpAmount;
			EType originalType = OriginalType;
			string actorGuid = ActorGuid;
			string iD = m_Class.ID;
			int health = Health;
			int gold = Gold;
			int xP = XP;
			int level = Level;
			List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
			List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
			bool playedThisRound = PlayedThisRound;
			bool isDead = IsDead;
			ECauseOfDeath causeOfDeath = CauseOfDeath;
			int originalMaxHealth = OriginalMaxHealth;
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorEarnedAbilityXP(xpEarned, originalType, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, IsSummon: false, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnSummon: false, null, null, "", originalMaxHealth));
			CActorEarnedXP_MessageData message = new CActorEarnedXP_MessageData(this)
			{
				m_xpAmount = xpAmount,
				m_scenarioXP = m_XP
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void ApplyImmediateDamage(int damage, bool cannotPrevent = false, bool pierceInvulnerable = false)
	{
		int health = Health;
		bool actorWasAsleep = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		GameState.ActorBeenDamaged(this, damage, checkIfPlayerCanAvoidDamage: false, null, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: false, cannotPrevent, pierceInvulnerable);
		CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(this);
		cActorBeenDamaged_MessageData.m_ActorBeingDamaged = this;
		cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
		cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
		ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
	}

	public void ApplyCondition(CActor actorApplying, CCondition.ENegativeCondition condition, int duration = 0, EConditionDecTrigger decTrigger = EConditionDecTrigger.Never, string animOverload = "", bool isMapCondition = false, bool canGoOverCurseLimit = false)
	{
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
		{
			if (item.Ability.HasCondition(condition))
			{
				CImmunity_MessageData cImmunity_MessageData = new CImmunity_MessageData(this);
				cImmunity_MessageData.m_ImmunityAbility = item;
				cImmunity_MessageData.negativeCondition = condition;
				ScenarioRuleClient.MessageHandler(cImmunity_MessageData);
				return;
			}
		}
		switch (condition)
		{
		case CCondition.ENegativeCondition.Curse:
		{
			bool conditionAlreadyApplied7 = Tokens.HasKey(CCondition.ENegativeCondition.Curse);
			if (OriginalType == EType.HeroSummon)
			{
				if (this is CHeroSummonActor cHeroSummonActor)
				{
					cHeroSummonActor.Summoner.Class.AddCurseCard(cHeroSummonActor.Summoner, canGoOverCurseLimit);
				}
			}
			else
			{
				Class.AddCurseCard(this, canGoOverCurseLimit);
			}
			CCurse_MessageData message7 = new CCurse_MessageData(animOverload, null)
			{
				m_ActorToCurse = this,
				m_DifficultyModCurse = (duration == int.MaxValue),
				m_ConditionAlreadyApplied = conditionAlreadyApplied7
			};
			ScenarioRuleClient.MessageHandler(message7);
			break;
		}
		case CCondition.ENegativeCondition.Disarm:
		{
			bool conditionAlreadyApplied5 = Tokens.HasKey(CCondition.ENegativeCondition.Disarm);
			if (!GameState.OverridingCurrentActor && this == GameState.InternalCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disarm, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disarm, duration, decTrigger, this);
			}
			CDisarm_MessageData message5 = new CDisarm_MessageData(animOverload, null)
			{
				m_ActorToDisarm = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied5
			};
			ScenarioRuleClient.MessageHandler(message5);
			break;
		}
		case CCondition.ENegativeCondition.Immobilize:
		{
			bool conditionAlreadyApplied6 = Tokens.HasKey(CCondition.ENegativeCondition.Immobilize);
			if (!GameState.OverridingCurrentActor && this == GameState.InternalCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Immobilize, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Immobilize, duration, decTrigger, this);
			}
			CImmobilize_MessageData message6 = new CImmobilize_MessageData(animOverload, null)
			{
				m_ActorToImmobilize = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied6
			};
			ScenarioRuleClient.MessageHandler(message6);
			break;
		}
		case CCondition.ENegativeCondition.Muddle:
		{
			bool conditionAlreadyApplied4 = Tokens.HasKey(CCondition.ENegativeCondition.Muddle);
			if (!GameState.OverridingCurrentActor && this == GameState.InternalCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Muddle, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Muddle, duration, decTrigger, this);
			}
			CMuddle_MessageData message4 = new CMuddle_MessageData(animOverload, null)
			{
				m_ActorToMuddle = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied4
			};
			ScenarioRuleClient.MessageHandler(message4);
			break;
		}
		case CCondition.ENegativeCondition.Poison:
		{
			bool conditionAlreadyApplied8 = Tokens.HasKey(CCondition.ENegativeCondition.Poison);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Poison, int.MaxValue, EConditionDecTrigger.Never, this);
			CPoison_MessageData message8 = new CPoison_MessageData(animOverload, null)
			{
				m_PoisonedActor = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied8
			};
			ScenarioRuleClient.MessageHandler(message8);
			break;
		}
		case CCondition.ENegativeCondition.Stun:
		{
			bool conditionAlreadyApplied3 = Tokens.HasKey(CCondition.ENegativeCondition.Stun);
			if (!GameState.OverridingCurrentActor && this == GameState.InternalCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Stun, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddNegativeToken(CCondition.ENegativeCondition.Stun, duration, decTrigger, this);
			}
			CStun_MessageData message3 = new CStun_MessageData(animOverload, null)
			{
				m_ActorToStun = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied3
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case CCondition.ENegativeCondition.Wound:
		{
			bool conditionAlreadyApplied2 = Tokens.HasKey(CCondition.ENegativeCondition.Wound);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Wound, int.MaxValue, EConditionDecTrigger.Never, this);
			CWound_MessageData message2 = new CWound_MessageData(animOverload, null)
			{
				m_ActorToWound = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied2
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CCondition.ENegativeCondition.StopFlying:
			Tokens.HasKey(CCondition.ENegativeCondition.StopFlying);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.StopFlying, int.MaxValue, EConditionDecTrigger.Never, this);
			break;
		case CCondition.ENegativeCondition.Sleep:
		{
			bool conditionAlreadyApplied = Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			Tokens.AddNegativeToken(CCondition.ENegativeCondition.Sleep, int.MaxValue, EConditionDecTrigger.Never, this);
			CSleep_MessageData message = new CSleep_MessageData(animOverload, null)
			{
				m_ActorToSleep = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		default:
			DLLDebug.LogError("Unable to find condition " + condition);
			break;
		}
		bool flag = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				flag = cEnemyActor.IsSummon;
			}
		}
		EType type = Type;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		bool isSummon = flag;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorApplyNegativeCondition, type, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, isSummon, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnEnemySummon: false, null, null, "", doNotSerialize: false, originalMaxHealth));
	}

	public void ActivatePassiveItems(bool firstLoad, PlayerState playerState = null, CItem singleItem = null)
	{
		foreach (CItem item in (singleItem == null) ? Inventory.AllItems : new List<CItem> { singleItem })
		{
			if (!firstLoad || ItemLocked || item.YMLData.Trigger != CItem.EItemTrigger.PassiveEffect || item.SlotState != CItem.EItemSlotState.Equipped)
			{
				continue;
			}
			item.ActiveBonuses.Clear();
			if (item.YMLData.Data.Abilities != null)
			{
				foreach (CAbility ability in item.YMLData.Data.Abilities)
				{
					item.AddActiveBonus(ability, this, this);
				}
			}
			item.SlotState = CItem.EItemSlotState.Selected;
			Inventory.HandleActiveItemTriggered(item);
			if (item.YMLData.Usage == CItem.EUsageType.Unrestricted && item.YMLData.UsedWhenEquipped == true)
			{
				UsedItem(item, firstTimeUse: true);
			}
		}
		playerState?.Save(firstLoad, forceSave: false, (CPlayerActor)this);
	}

	public void ApplyCondition(CActor actorApplying, CCondition.EPositiveCondition condition, int duration = 0, EConditionDecTrigger decTrigger = EConditionDecTrigger.Never, string animOverload = "", bool isMapCondition = false, bool canGoOverBlessLimit = false)
	{
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
		{
			if (item.Ability.HasCondition(condition))
			{
				CImmunity_MessageData cImmunity_MessageData = new CImmunity_MessageData(this);
				cImmunity_MessageData.m_ImmunityAbility = item;
				cImmunity_MessageData.positiveCondition = condition;
				ScenarioRuleClient.MessageHandler(cImmunity_MessageData);
				return;
			}
		}
		switch (condition)
		{
		case CCondition.EPositiveCondition.Bless:
		{
			bool conditionAlreadyApplied3 = Tokens.HasKey(CCondition.EPositiveCondition.Bless);
			if (OriginalType == EType.HeroSummon)
			{
				if (this is CHeroSummonActor cHeroSummonActor)
				{
					cHeroSummonActor.Summoner.Class.AddBlessCard(cHeroSummonActor.Summoner, canGoOverBlessLimit);
				}
			}
			else
			{
				Class.AddBlessCard(this, canGoOverBlessLimit);
			}
			CBless_MessageData message3 = new CBless_MessageData(animOverload, actorApplying)
			{
				m_ActorToBless = this,
				m_DifficultyModBless = (duration == int.MaxValue),
				m_ConditionAlreadyApplied = conditionAlreadyApplied3
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case CCondition.EPositiveCondition.Invisible:
		{
			bool conditionAlreadyApplied = Tokens.HasKey(CCondition.EPositiveCondition.Invisible);
			if (this == GameState.InternalCurrentActor && !GameState.OverridingCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Invisible, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Invisible, duration, decTrigger, this);
			}
			CInvisible_MessageData message = new CInvisible_MessageData(animOverload, actorApplying)
			{
				m_ActorToMakeInvisible = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		case CCondition.EPositiveCondition.Strengthen:
		{
			bool conditionAlreadyApplied2 = Tokens.HasKey(CCondition.EPositiveCondition.Strengthen);
			if (this == GameState.InternalCurrentActor && !GameState.OverridingCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Strengthen, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Strengthen, duration, decTrigger, this);
			}
			CStrengthen_MessageData message2 = new CStrengthen_MessageData(animOverload, actorApplying)
			{
				m_ActorToStrengthen = this,
				m_ConditionAlreadyApplied = conditionAlreadyApplied2
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CCondition.EPositiveCondition.Immovable:
			Tokens.HasKey(CCondition.EPositiveCondition.Immovable);
			if (this == GameState.InternalCurrentActor && !GameState.OverridingCurrentActor && !isMapCondition && decTrigger == EConditionDecTrigger.Turns)
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Immovable, duration + 1, decTrigger, this);
			}
			else
			{
				Tokens.AddPositiveToken(CCondition.EPositiveCondition.Immovable, duration, decTrigger, this);
			}
			break;
		default:
			DLLDebug.LogError("Unable to find condition " + condition);
			break;
		}
		bool flag = false;
		if (Type == EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == ActorGuid);
			if (cEnemyActor != null)
			{
				flag = cEnemyActor.IsSummon;
			}
		}
		EType type = Type;
		string actorGuid = ActorGuid;
		string iD = m_Class.ID;
		int health = Health;
		int gold = Gold;
		int xP = XP;
		int level = Level;
		List<PositiveConditionPair> checkPositiveTokens = Tokens.CheckPositiveTokens;
		List<NegativeConditionPair> checkNegativeTokens = Tokens.CheckNegativeTokens;
		bool playedThisRound = PlayedThisRound;
		bool isDead = IsDead;
		ECauseOfDeath causeOfDeath = CauseOfDeath;
		bool isSummon = flag;
		int originalMaxHealth = OriginalMaxHealth;
		SEventLogMessageHandler.AddEventLogMessage(new SEventActor(ESESubTypeActor.ActorApplyPositiveCondition, type, actorGuid, iD, health, gold, xP, level, checkPositiveTokens, checkNegativeTokens, playedThisRound, isDead, causeOfDeath, isSummon, "", "", null, int.MaxValue, CBaseCard.ECardType.None, CAbility.EAbilityType.None, "", 0, actedOnEnemySummon: false, null, null, "", doNotSerialize: false, originalMaxHealth));
	}

	public bool Equals(CActor other)
	{
		if (other == null)
		{
			return false;
		}
		return ActorGuid.Equals(other.ActorGuid);
	}

	public override int GetHashCode()
	{
		return ActorGuid.GetHashCode();
	}

	public static bool IsSameType(CActor first, CActor second)
	{
		return IsSameType(first.Type, second.Type);
	}

	public static bool IsSameType(EType first, EType second)
	{
		return first == second;
	}

	public static List<Point> FindCharacterPath(CActor actor, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, out bool foundPath, bool avoidTraps = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool carryOtherActors = false, EType carryType = EType.Unknown, bool logFailure = false)
	{
		EType eType = actor.Type;
		if (carryType != EType.Unknown)
		{
			eType = carryType;
		}
		if (actor.Type == EType.Player && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Peek().OriginalType == EType.Enemy)
		{
			eType = EType.Enemy;
		}
		switch (eType)
		{
		case EType.Enemy:
		case EType.Enemy2:
		case EType.Neutral:
			return CEnemyActor.FindEnemyPath(eType, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, out foundPath, logFailure);
		case EType.Player:
		case EType.HeroSummon:
		case EType.Ally:
			return CPlayerActor.FindPlayerPath(actor, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, out foundPath, avoidTraps, ignoreDifficultTerrain, ignoreHazardousTerrain, carryOtherActors, carryType, logFailure);
		default:
			foundPath = false;
			return new List<Point>();
		}
	}

	public static List<Point> FindStraightPath(Point startLocation, Point endLocation, bool ignoreBlocked, out bool foundPath)
	{
		if (!ignoreBlocked)
		{
			List<Point> list = new List<Point>();
			foreach (CEnemyActor allEnemyMonstersAndObject in ScenarioManager.Scenario.AllEnemyMonstersAndObjects)
			{
				if (allEnemyMonstersAndObject.BlocksPathing)
				{
					list.Add(allEnemyMonstersAndObject.ArrayIndex);
				}
			}
			ScenarioManager.PathFinder.QueuedTransientBlockedLists.Add(list);
		}
		List<CObjectObstacle> list2;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list2 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		List<Point> list3 = new List<Point>();
		foreach (CObjectObstacle item in list2)
		{
			foreach (TileIndex pathingBlocker in item.PathingBlockers)
			{
				if (item.IgnoresFlyAndJump)
				{
					list3.Add(new Point(pathingBlocker));
				}
			}
		}
		ScenarioManager.PathFinder.QueuedTransientSuperBlockedLists.Add(list3);
		return ScenarioManager.PathFinder.FindStraightPath(startLocation, endLocation, ignoreBlocked, out foundPath);
	}

	public static bool AreActorsAllied(EType actor1Type, EType actor2Type)
	{
		if (actor1Type == actor2Type)
		{
			return true;
		}
		switch (actor1Type)
		{
		case EType.Player:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Enemy:
			switch (actor2Type)
			{
			case EType.Enemy:
				return true;
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Enemy2:
			case EType.Neutral:
				return false;
			}
			break;
		case EType.HeroSummon:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Ally:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		case EType.Enemy2:
			switch (actor2Type)
			{
			case EType.Enemy2:
				return true;
			case EType.Player:
			case EType.Enemy:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return false;
			}
			break;
		case EType.Neutral:
			switch (actor2Type)
			{
			case EType.Player:
			case EType.HeroSummon:
			case EType.Ally:
			case EType.Neutral:
				return true;
			case EType.Enemy:
			case EType.Enemy2:
				return false;
			}
			break;
		}
		return false;
	}

	public static bool ImmuneToAllConditionsCheck(List<CAbility.EAbilityType> immunities)
	{
		bool result = false;
		if (!ImmunityConditionAbilityTypes.Except(immunities).Any())
		{
			result = true;
		}
		return result;
	}

	public static List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, out bool shuffle, out List<AttackModifierYMLData> notUsed)
	{
		notUsed = new List<AttackModifierYMLData>();
		if (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenRollingAttackModifiers))
		{
			List<AttackModifierYMLData> list = new List<AttackModifierYMLData>();
			switch (advStatus)
			{
			case EAdvantageStatuses.Advantage:
			{
				List<AttackModifierYMLData> list2 = new List<AttackModifierYMLData>();
				AttackModifierYMLData attackModifierYMLData2 = DrawAttackModifierCard(actor, current, discarded);
				list2.Add(attackModifierYMLData2);
				AttackModifierYMLData attackModifierYMLData3 = DrawAttackModifierCard(actor, current, discarded, list2);
				list2.Add(attackModifierYMLData3);
				if (attackModifierYMLData2.Rolling)
				{
					AttackModifierYMLData attackModifierYMLData4 = null;
					bool flag = !attackModifierYMLData3.Rolling;
					while (!flag && list2.Count < 200)
					{
						attackModifierYMLData4 = DrawAttackModifierCard(actor, current, discarded, list2);
						if (!attackModifierYMLData4.Rolling)
						{
							flag = true;
							list2.Add(attackModifierYMLData4);
						}
						else
						{
							list2.Add(attackModifierYMLData4);
						}
					}
					attackModifierYMLData4 = DrawAttackModifierCard(actor, current, discarded, list2);
					list2.Add(attackModifierYMLData4);
					for (int num = 0; num < list2.Count - 2; num++)
					{
						AttackModifierYMLData attackModifierYMLData5 = list2[num];
						if (attackModifierYMLData5.Rolling)
						{
							list.Add(attackModifierYMLData5);
						}
					}
					AttackModifierYMLData item = ChooseCardBasedOnAdvantage(attackStrength, list2[list2.Count - 2], list2[list2.Count - 1], advStatus);
					list.Add(item);
				}
				else
				{
					AttackModifierYMLData item2 = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData2, attackModifierYMLData3, advStatus);
					list.Add(item2);
				}
				shuffle = list2.Any((AttackModifierYMLData a) => a.Shuffle);
				foreach (AttackModifierYMLData item5 in list)
				{
					list2.Remove(item5);
				}
				notUsed = list2.ToList();
				return list;
			}
			case EAdvantageStatuses.Disadvantage:
			{
				List<AttackModifierYMLData> list3 = new List<AttackModifierYMLData>();
				AttackModifierYMLData attackModifierYMLData6 = DrawAttackModifierCard(actor, current, discarded);
				list3.Add(attackModifierYMLData6);
				AttackModifierYMLData attackModifierYMLData7 = DrawAttackModifierCard(actor, current, discarded, list3);
				list3.Add(attackModifierYMLData7);
				if (attackModifierYMLData6.Rolling)
				{
					AttackModifierYMLData attackModifierYMLData8 = null;
					bool flag2 = !attackModifierYMLData7.Rolling;
					while (!flag2 && list3.Count < 200)
					{
						attackModifierYMLData8 = DrawAttackModifierCard(actor, current, discarded, list3);
						if (!attackModifierYMLData8.Rolling)
						{
							flag2 = true;
							list3.Add(attackModifierYMLData8);
						}
						else
						{
							list3.Add(attackModifierYMLData8);
						}
					}
					attackModifierYMLData8 = DrawAttackModifierCard(actor, current, discarded, list3);
					list3.Add(attackModifierYMLData8);
					AttackModifierYMLData item3 = ChooseCardBasedOnAdvantage(attackStrength, list3[list3.Count - 2], list3[list3.Count - 1], advStatus);
					list.Add(item3);
				}
				else
				{
					AttackModifierYMLData item4 = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData6, attackModifierYMLData7, advStatus);
					list.Add(item4);
				}
				shuffle = list3.Any((AttackModifierYMLData a) => a.Shuffle);
				foreach (AttackModifierYMLData item6 in list)
				{
					list3.Remove(item6);
				}
				notUsed = list3.ToList();
				return list;
			}
			default:
			{
				AttackModifierYMLData attackModifierYMLData = DrawAttackModifierCard(actor, current, discarded);
				list.Add(attackModifierYMLData);
				while (attackModifierYMLData.Rolling && list.Count < 200)
				{
					attackModifierYMLData = DrawAttackModifierCard(actor, current, discarded, list);
					list.Add(attackModifierYMLData);
				}
				shuffle = list.Any((AttackModifierYMLData a) => a.Shuffle);
				return list;
			}
			}
		}
		switch (advStatus)
		{
		case EAdvantageStatuses.Advantage:
		{
			List<AttackModifierYMLData> list5 = new List<AttackModifierYMLData>();
			AttackModifierYMLData attackModifierYMLData10 = DrawAttackModifierCard(actor, current, discarded);
			AttackModifierYMLData attackModifierYMLData11 = DrawAttackModifierCard(actor, current, discarded, new List<AttackModifierYMLData> { attackModifierYMLData10 });
			if (attackModifierYMLData10.Rolling && attackModifierYMLData11.Rolling)
			{
				list5.Add(attackModifierYMLData10);
				list5.Add(attackModifierYMLData11);
				AttackModifierYMLData attackModifierYMLData12 = DrawAttackModifierCard(actor, current, discarded, list5);
				list5.Add(attackModifierYMLData12);
				while (attackModifierYMLData12.Rolling && list5.Count < 200)
				{
					attackModifierYMLData12 = DrawAttackModifierCard(actor, current, discarded, list5);
					list5.Add(attackModifierYMLData12);
				}
				shuffle = list5.Any((AttackModifierYMLData a) => a.Shuffle);
				return list5;
			}
			if (attackModifierYMLData10.Rolling || attackModifierYMLData11.Rolling)
			{
				list5.Add(attackModifierYMLData10);
				list5.Add(attackModifierYMLData11);
				shuffle = list5.Any((AttackModifierYMLData a) => a.Shuffle);
				return list5;
			}
			shuffle = attackModifierYMLData10.Shuffle || attackModifierYMLData11.Shuffle;
			AttackModifierYMLData attackModifierYMLData13 = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData10, attackModifierYMLData11, advStatus);
			if (attackModifierYMLData13 == attackModifierYMLData10)
			{
				notUsed.Add(attackModifierYMLData11);
			}
			else
			{
				notUsed.Add(attackModifierYMLData10);
			}
			return new List<AttackModifierYMLData> { attackModifierYMLData13 };
		}
		case EAdvantageStatuses.Disadvantage:
		{
			AttackModifierYMLData attackModifierYMLData14 = DrawAttackModifierCard(actor, current, discarded);
			AttackModifierYMLData attackModifierYMLData15 = DrawAttackModifierCard(actor, current, discarded, new List<AttackModifierYMLData> { attackModifierYMLData14 });
			if (attackModifierYMLData14.Rolling && attackModifierYMLData15.Rolling)
			{
				notUsed.Add(attackModifierYMLData14);
				notUsed.Add(attackModifierYMLData15);
				AttackModifierYMLData attackModifierYMLData16 = DrawAttackModifierCard(actor, current, discarded, notUsed);
				while (attackModifierYMLData16.Rolling && notUsed.Count < 200)
				{
					notUsed.Add(attackModifierYMLData16);
					attackModifierYMLData16 = DrawAttackModifierCard(actor, current, discarded, notUsed);
				}
				shuffle = attackModifierYMLData16.Shuffle || notUsed.Any((AttackModifierYMLData a) => a.Shuffle);
				return new List<AttackModifierYMLData> { attackModifierYMLData16 };
			}
			if (attackModifierYMLData14.Rolling)
			{
				notUsed.Add(attackModifierYMLData14);
				shuffle = attackModifierYMLData14.Shuffle || attackModifierYMLData15.Shuffle;
				return new List<AttackModifierYMLData> { attackModifierYMLData15 };
			}
			if (attackModifierYMLData15.Rolling)
			{
				notUsed.Add(attackModifierYMLData15);
				shuffle = attackModifierYMLData14.Shuffle || attackModifierYMLData15.Shuffle;
				return new List<AttackModifierYMLData> { attackModifierYMLData14 };
			}
			shuffle = attackModifierYMLData14.Shuffle || attackModifierYMLData15.Shuffle;
			AttackModifierYMLData attackModifierYMLData17 = ChooseCardBasedOnAdvantage(attackStrength, attackModifierYMLData14, attackModifierYMLData15, advStatus);
			if (attackModifierYMLData17 == attackModifierYMLData14)
			{
				notUsed.Add(attackModifierYMLData15);
			}
			else
			{
				notUsed.Add(attackModifierYMLData14);
			}
			return new List<AttackModifierYMLData> { attackModifierYMLData17 };
		}
		default:
		{
			List<AttackModifierYMLData> list4 = new List<AttackModifierYMLData>();
			AttackModifierYMLData attackModifierYMLData9 = DrawAttackModifierCard(actor, current, discarded);
			list4.Add(attackModifierYMLData9);
			while (attackModifierYMLData9.Rolling && list4.Count < 200)
			{
				attackModifierYMLData9 = DrawAttackModifierCard(actor, current, discarded, list4);
				list4.Add(attackModifierYMLData9);
			}
			shuffle = list4.Any((AttackModifierYMLData a) => a.Shuffle);
			return list4;
		}
		}
	}

	private static AttackModifierYMLData DrawAttackModifierCard(CActor actor, List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, List<AttackModifierYMLData> drawnMods = null)
	{
		if (current.Count == 0 && ((actor is CPlayerActor && GameState.ShuffleAttackModsEnabledForPlayers) || (actor is CEnemyActor && GameState.ShuffleAttackModsEnabledForMonsters) || (actor is CHeroSummonActor && GameState.ShuffleAttackModsEnabledForPlayers)))
		{
			ShuffleAttackModifierCards(current, discarded, drawnMods);
		}
		AttackModifierYMLData attackModifierYMLData = current[0];
		if (attackModifierYMLData.IsCurse)
		{
			DLLDebug.LogInfo("A Curse card has been drawn by " + actor.Class.ID + " and removed from their attack modifier deck");
		}
		else if (attackModifierYMLData.IsBless)
		{
			DLLDebug.LogInfo("A Bless card has been drawn by " + actor.Class.ID + " and removed from their attack modifier deck");
		}
		else
		{
			discarded.Add(attackModifierYMLData);
		}
		current.Remove(attackModifierYMLData);
		return attackModifierYMLData;
	}

	public static void ShuffleAttackModifierCards(List<AttackModifierYMLData> current, List<AttackModifierYMLData> discarded, List<AttackModifierYMLData> inUse = null)
	{
		bool flag = false;
		if (inUse != null && discarded.Count > inUse.Count)
		{
			discarded.RemoveAll((AttackModifierYMLData card) => inUse.Contains(card));
			flag = true;
		}
		while (discarded.Count > 0)
		{
			AttackModifierYMLData item = discarded[0];
			discarded.Remove(item);
			current.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, current.Count), item);
		}
		current.Shuffle();
		if (flag)
		{
			discarded.AddRange(inUse);
		}
	}

	private static AttackModifierYMLData ChooseCardBasedOnAdvantage(int attackStrength, AttackModifierYMLData first, AttackModifierYMLData second, EAdvantageStatuses advantage)
	{
		int num = Convert.ToInt32(GameState.s_Parser.Parse("100" + first.MathModifier, isRadians: false));
		int num2 = Convert.ToInt32(GameState.s_Parser.Parse(attackStrength + first.MathModifier, isRadians: false));
		int num3 = Convert.ToInt32(GameState.s_Parser.Parse("100" + second.MathModifier, isRadians: false));
		int num4 = Convert.ToInt32(GameState.s_Parser.Parse(attackStrength + second.MathModifier, isRadians: false));
		switch (advantage)
		{
		case EAdvantageStatuses.Advantage:
			if (num2 == num4)
			{
				if (first.Card.HasAdditionalEffect && second.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (first.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (second.Card.HasAdditionalEffect)
				{
					return second;
				}
				if (num == num3)
				{
					return first;
				}
				if (num > num3)
				{
					return first;
				}
				return second;
			}
			if (num2 > num4)
			{
				return first;
			}
			if (first.Card.HasAdditionalEffect && second.Card.HasAdditionalEffect)
			{
				return first;
			}
			if (first.Card.HasAdditionalEffect)
			{
				return first;
			}
			_ = second.Card.HasAdditionalEffect;
			return second;
		case EAdvantageStatuses.Disadvantage:
			if ((num2 < num4 && num3 != 0) || num == 0)
			{
				return first;
			}
			if (num2 == num4)
			{
				if (first.Card.HasAdditionalEffect && second.Card.HasAdditionalEffect)
				{
					return first;
				}
				if (first.Card.HasAdditionalEffect)
				{
					return second;
				}
				_ = second.Card.HasAdditionalEffect;
				return first;
			}
			return second;
		default:
			DLLDebug.LogError("Invalid advantage status sent to ChooseCardBasedOnAdvantage");
			return first;
		}
	}

	public static CActorStatic.CTargetPath CalculateTargetActorPath(CActor actor, Point actorHexArrayIndex, List<CActor> targetActors, List<CActor> blockingActors, CActor targetActor, Point targetHexArrayIndex, Point sampleHexArrayIndex, bool trapsBlock, List<Point> trapsToBlock, bool alliesBlock, List<Point> alliesToBlock, bool isPlayer, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, bool excludeDestinationInPath, bool targetActorShouldBlock, int maxMoveCount, int range, ref bool foundPath, bool obstaclesBlock, bool hazardTerrainBlock, bool shouldPathThroughDoors, bool openDoorwaysBlock)
	{
		List<Point> list = new List<Point>();
		List<Point> list2 = new List<Point>();
		if (blockingActors != null)
		{
			foreach (CActor blockingActor in blockingActors)
			{
				if (blockingActor != null && (blockingActor != targetActor || targetActorShouldBlock))
				{
					list.Add(blockingActor.ArrayIndex);
				}
			}
		}
		if (trapsBlock)
		{
			foreach (CObjectTrap item2 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
			{
				if ((!item2.Activated && item2.ArrayIndex.X != sampleHexArrayIndex.X) || item2.ArrayIndex.Y != sampleHexArrayIndex.Y)
				{
					list.Add(new Point(item2.ArrayIndex));
				}
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (heroSummon.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					list.Add(heroSummon.m_ArrayIndex);
				}
			}
		}
		else if (trapsToBlock != null)
		{
			foreach (Point item3 in trapsToBlock)
			{
				if (item3.X != sampleHexArrayIndex.X || item3.Y != sampleHexArrayIndex.Y)
				{
					list.Add(item3);
				}
			}
		}
		if (hazardTerrainBlock)
		{
			foreach (CObjectHazardousTerrain item4 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>())
			{
				if (item4.ArrayIndex.X != sampleHexArrayIndex.X || item4.ArrayIndex.Y != sampleHexArrayIndex.Y)
				{
					list.Add(new Point(item4.ArrayIndex));
				}
			}
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == actor.ActorGuid);
			foreach (CObjectDifficultTerrain item5 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>())
			{
				if (item5.TreatAsTrap && item5.TreatAsTrapFilter.IsValidTarget(actorState) && (item5.ArrayIndex.X != sampleHexArrayIndex.X || item5.ArrayIndex.Y != sampleHexArrayIndex.Y))
				{
					list.Add(new Point(item5.ArrayIndex));
				}
			}
		}
		if (alliesBlock)
		{
			if (isPlayer)
			{
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					list.Add(playerActor.ArrayIndex);
				}
				foreach (CHeroSummonActor heroSummon2 in ScenarioManager.Scenario.HeroSummons)
				{
					list.Add(heroSummon2.ArrayIndex);
				}
				foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
				{
					list.Add(allyMonster.ArrayIndex);
				}
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					list.Add(neutralMonster.ArrayIndex);
				}
			}
			else if (actor.Type == EType.Enemy)
			{
				foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
				{
					list.Add(enemy.ArrayIndex);
				}
			}
			else if (actor.Type == EType.Enemy2)
			{
				foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
				{
					list.Add(enemy2Monster.ArrayIndex);
				}
			}
		}
		else if (alliesToBlock != null)
		{
			foreach (Point item6 in alliesToBlock)
			{
				if (item6.X != actor.ArrayIndex.X || item6.Y != actor.ArrayIndex.Y)
				{
					list.Add(item6);
				}
			}
		}
		if (!obstaclesBlock)
		{
			for (int num = 0; num < ScenarioManager.Height; num++)
			{
				for (int num2 = 0; num2 < ScenarioManager.Width; num2++)
				{
					if (ScenarioManager.Tiles[num2, num] == null)
					{
						continue;
					}
					List<CObjectObstacle> list3;
					lock (ScenarioManager.CurrentScenarioState.Props)
					{
						list3 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
					}
					foreach (CObjectObstacle item7 in list3)
					{
						foreach (TileIndex point in item7.PathingBlockers)
						{
							list.RemoveAll((Point it) => it.Equals(new Point(point)));
						}
					}
				}
			}
		}
		else
		{
			List<CObjectObstacle> list4;
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				list4 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
			}
			foreach (CObjectObstacle item8 in list4)
			{
				foreach (TileIndex pathingBlocker in item8.PathingBlockers)
				{
					list.Add(new Point(pathingBlocker));
					if (item8.IgnoresFlyAndJump)
					{
						list2.Add(new Point(pathingBlocker));
					}
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				list.Add(@object.ArrayIndex);
			}
		}
		List<CObjectObstacle> list5;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list5 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		foreach (CObjectObstacle item9 in list5)
		{
			foreach (TileIndex pathingBlocker2 in item9.PathingBlockers)
			{
				if (item9.IgnoresFlyAndJump)
				{
					list2.Add(new Point(pathingBlocker2));
				}
			}
		}
		if (shouldPathThroughDoors || openDoorwaysBlock)
		{
			for (int num3 = 0; num3 < ScenarioManager.Height; num3++)
			{
				for (int num4 = 0; num4 < ScenarioManager.Width; num4++)
				{
					if (ScenarioManager.Tiles[num4, num3] == null)
					{
						continue;
					}
					List<CObjectDoor> list6;
					lock (ScenarioManager.CurrentScenarioState.Props)
					{
						list6 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>().ToList();
					}
					if (openDoorwaysBlock)
					{
						foreach (CObjectDoor item10 in list6)
						{
							Point item = new Point(item10.ArrayIndex);
							if (!list.Contains(item))
							{
								list.Add(item);
							}
						}
					}
					else
					{
						if (!shouldPathThroughDoors)
						{
							continue;
						}
						foreach (CObjectDoor door in list6)
						{
							if (!door.DoorIsOpen)
							{
								list.RemoveAll((Point it) => it.Equals(new Point(door.ArrayIndex)));
							}
						}
					}
				}
			}
		}
		ScenarioManager.PathFinder.QueuedTransientBlockedLists.Add(list);
		ScenarioManager.PathFinder.QueuedTransientSuperBlockedLists.Add(list2);
		foundPath = false;
		CActorStatic.CTargetPath cTargetPath = new CActorStatic.CTargetPath();
		cTargetPath.m_Target = targetActor;
		cTargetPath.m_TargetTile = targetHexArrayIndex;
		if (actorHexArrayIndex.X != sampleHexArrayIndex.X || actorHexArrayIndex.Y != sampleHexArrayIndex.Y)
		{
			cTargetPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(actorHexArrayIndex, ScenarioManager.Tiles[sampleHexArrayIndex.X, sampleHexArrayIndex.Y], out cTargetPath.m_Waypoints, maxMoveCount, jump, fly, ignoreDifficultTerrain, excludeDestinationInPath, ignoreMoveCost: false, out foundPath, shouldPathThroughDoors);
		}
		else
		{
			cTargetPath.m_ArrayIndices = new List<Point>();
			cTargetPath.m_Waypoints = new List<CTile>();
			foundPath = true;
		}
		cTargetPath.m_ArrayIndicesBeforeCull = new List<Point>(cTargetPath.m_ArrayIndices);
		cTargetPath.m_TargetPathID = s_TargetPathID++;
		cTargetPath.m_PathEndTile = sampleHexArrayIndex;
		cTargetPath.m_TrapsInPath = new List<Point>();
		if (!trapsBlock)
		{
			List<CObjectTrap> list7 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>().ToList();
			foreach (Point point2 in cTargetPath.m_ArrayIndices)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(point2);
				bool flag = false;
				if (cActor is CHeroSummonActor cHeroSummonActor && !AreActorsAllied(actor.Type, cActor.Type) && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
				{
					flag = true;
				}
				if ((actorHexArrayIndex.X != point2.X || actorHexArrayIndex.Y != point2.Y) && (list7.Find((CObjectTrap x) => x.ArrayIndex.X == point2.X && x.ArrayIndex.Y == point2.Y) != null || flag) && ((!fly && !jump) || (!fly && jump && point2.X == sampleHexArrayIndex.X && point2.Y == sampleHexArrayIndex.Y)))
				{
					cTargetPath.m_TrapsInPath.Add(point2);
				}
			}
		}
		if (!hazardTerrainBlock)
		{
			List<CObjectHazardousTerrain> list8 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>().ToList();
			ActorState actorState2 = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			List<CObjectDifficultTerrain> list9 = (from x in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>()
				where x.TreatAsTrap && (x.TreatAsTrapFilter == null || x.TreatAsTrapFilter.IsValidTarget(actorState2))
				select x).ToList();
			foreach (Point point3 in cTargetPath.m_ArrayIndices)
			{
				if ((actorHexArrayIndex.X != point3.X || actorHexArrayIndex.Y != point3.Y) && (list8.Find((CObjectHazardousTerrain x) => x.ArrayIndex.X == point3.X && x.ArrayIndex.Y == point3.Y) != null || list9.Find((CObjectDifficultTerrain x) => x.ArrayIndex.X == point3.X && x.ArrayIndex.Y == point3.Y) != null) && ((!ignoreHazardousTerrain && !fly && !jump) || (!ignoreHazardousTerrain && !fly && jump && point3.X == sampleHexArrayIndex.X && point3.Y == sampleHexArrayIndex.Y)))
				{
					cTargetPath.m_TrapsInPath.Add(point3);
				}
			}
		}
		cTargetPath.m_AlliesInPath = new List<Point>();
		if (!alliesBlock)
		{
			List<CActor> list10 = new List<CActor>();
			if (isPlayer)
			{
				list10.AddRange(ScenarioManager.Scenario.PlayerActors);
				list10.AddRange(ScenarioManager.Scenario.HeroSummons);
			}
			else
			{
				list10.AddRange(ScenarioManager.Scenario.Enemies);
			}
			list10.Remove(actor);
			foreach (Point point4 in cTargetPath.m_ArrayIndices)
			{
				if (list10.Any((CActor x) => x.ArrayIndex.X == point4.X && x.ArrayIndex.Y == point4.Y))
				{
					cTargetPath.m_AlliesInPath.Add(point4);
				}
			}
		}
		if (foundPath)
		{
			CActorStatic.PostProcessTargetPath(cTargetPath, actor, isPlayer, maxMoveCount, fly, jump, ignoreDifficultTerrain, range);
		}
		return cTargetPath;
	}

	public static void Reset()
	{
		s_TargetPathID = 0;
	}

	public static void SetLOSTileScalar(float x, float y)
	{
		s_LOSTileScalar = new Vector(x, y);
	}

	public static bool LineSegmentsIntersect(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
	{
		intersection = default(Vector);
		Vector vector = p2 - p;
		Vector vector2 = q2 - q;
		float num = vector.Cross(vector2);
		float num2 = (q - p).Cross(vector);
		if (MF.IsZero(num) && MF.IsZero(num2))
		{
			if (considerCollinearOverlapAsIntersect && ((0f <= (q - p) * vector && (q - p) * vector <= vector * vector) || (0f <= (p - q) * vector2 && (p - q) * vector2 <= vector2 * vector2)))
			{
				return true;
			}
			return false;
		}
		if (MF.IsZero(num) && !MF.IsZero(num2))
		{
			return false;
		}
		float num3 = (q - p).Cross(vector2) / num;
		float num4 = (q - p).Cross(vector) / num;
		float num5 = 0.001f;
		if (!MF.IsZero(num) && 0f - num5 <= num3 && num3 <= 1f + num5 && 0f - num5 <= num4 && num4 <= 1f + num5)
		{
			intersection = p + num3 * vector;
			return true;
		}
		return false;
	}

	private static bool LOSRayBlocked(CTile sourceClientTile, CTile targetClientTile, Vector sourcePosition, Vector targetPosition, Point passThruTileArrayIndex, out bool rayIntersects, List<CMap> visableMaps, LOSCallbackType callback)
	{
		CTile cTile = ScenarioManager.Tiles[passThruTileArrayIndex.X, passThruTileArrayIndex.Y];
		CNode cNode = ScenarioManager.PathFinder.Nodes[passThruTileArrayIndex.X, passThruTileArrayIndex.Y];
		Vector vector = default(Vector);
		MF.ArrayIndexToCartesianCoord(passThruTileArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector.X, out vector.Y);
		Vector vector2 = vector;
		vector2.Y += s_LOSTileScalar.Y / 0.75f * 0.5f;
		int num = 0;
		rayIntersects = false;
		for (float num2 = 60f; num2 <= 360f; num2 += 60f)
		{
			Vector vector3 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f);
			float num3 = (float)Math.Cos((double)(0f - num2) * (Math.PI / 180.0));
			float num4 = (float)Math.Sin((double)(0f - num2) * (Math.PI / 180.0));
			float x = vector3.X * num3 - vector3.Y * num4;
			float y = vector3.X * num4 + vector3.Y * num3;
			vector3 = new Vector(x, y);
			Vector vector4 = vector + vector3;
			Vector vector5 = (vector2 + vector4) * 0.5f;
			Vector vector6 = (vector5 - vector).Normalise();
			float num5 = Math.Abs((vector5 - sourcePosition) * vector6);
			float num6 = vector6 * (targetPosition - sourcePosition);
			bool flag = ScenarioManager.IsTileAdjacent(sourceClientTile.m_ArrayIndex.X, sourceClientTile.m_ArrayIndex.Y, passThruTileArrayIndex.X, passThruTileArrayIndex.Y, ignoreWalls: true);
			bool flag2 = num5 <= 0.1f;
			bool flag3 = false;
			if ((!flag) ? (num6 < 0.001f && LineSegmentsIntersect(vector2, vector4, sourcePosition, targetPosition, out var intersection)) : (num6 < 0f && LineSegmentsIntersect(vector2, vector4, sourcePosition, targetPosition, out intersection)))
			{
				int num7 = num;
				if (flag2 && !flag)
				{
					if ((vector2 - sourcePosition).Magnitude() < (vector4 - sourcePosition).Magnitude())
					{
						num7--;
						if (num7 < 0)
						{
							num7 = s_EdgeToAdjacentMapping.Length - 1;
						}
					}
					else
					{
						num7++;
						if (num7 == s_EdgeToAdjacentMapping.Length)
						{
							num7 = 0;
						}
					}
				}
				rayIntersects = true;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(passThruTileArrayIndex.X, passThruTileArrayIndex.Y, s_EdgeToAdjacentMapping[num7]);
				bool flag4 = adjacentTile?.m_Props.Any((CObjectProp p) => p.PropHealthDetails != null && p.PropHealthDetails.HasHealth) ?? false;
				bool flag5 = cTile?.m_Props.Any((CObjectProp p) => p.PropHealthDetails != null && p.PropHealthDetails.HasHealth) ?? false;
				bool flag6 = adjacentTile != null && ((targetClientTile != null && adjacentTile.m_ArrayIndex == targetClientTile.m_ArrayIndex) || (sourceClientTile != null && adjacentTile.m_ArrayIndex == sourceClientTile.m_ArrayIndex));
				bool flag7 = cTile != null && ((targetClientTile != null && cTile.m_ArrayIndex == targetClientTile.m_ArrayIndex) || (sourceClientTile != null && cTile.m_ArrayIndex == sourceClientTile.m_ArrayIndex));
				CNode cNode2 = ((adjacentTile != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
				if (cTile == null || adjacentTile == null || !cNode.Walkable || (cNode.IsBridge && !cNode.IsBridgeOpen && !(flag5 && flag7)) || (cNode2.IsBridge && !cNode2.IsBridgeOpen && !(flag4 && flag6)) || (!cNode2.Walkable && !cNode2.IsBridge) || (sourceClientTile != null && cTile.m_HexMap != sourceClientTile.m_HexMap && !visableMaps.Contains(cTile.m_HexMap) && (cTile.m_Hex2Map == null || !visableMaps.Contains(cTile.m_Hex2Map))) || (adjacentTile.m_HexMap != cTile.m_HexMap && !cNode2.IsBridge && !cNode.IsBridge))
				{
					callback?.Invoke(vector2, vector4, rayBlocked: true);
					callback?.Invoke(vector5, vector5 + vector6, rayBlocked: true);
					return true;
				}
			}
			vector2 = vector4;
			num++;
		}
		return false;
	}

	private static bool IsEdgeOnWall(Point arrayIndex, float edgeAngle)
	{
		int num = 6;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		CNode cNode = ((cTile != null) ? ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y] : null);
		int num2 = (int)edgeAngle / 60 % num;
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, s_EdgeToAdjacentMapping[num2]);
		CNode cNode2 = ((adjacentTile != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y] : null);
		num2 = ((num2 == 0) ? (num - 1) : (num2 - 1));
		CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(arrayIndex.X, arrayIndex.Y, s_EdgeToAdjacentMapping[num2]);
		CNode cNode3 = ((adjacentTile2 != null) ? ScenarioManager.PathFinder.Nodes[adjacentTile2.m_ArrayIndex.X, adjacentTile2.m_ArrayIndex.Y] : null);
		if (adjacentTile2 != null && adjacentTile != null && cNode3.Walkable && cNode2.Walkable && (!cNode3.IsBridge || cNode3.IsBridgeOpen) && (!cNode2.IsBridge || cNode2.IsBridgeOpen) && (cTile.m_HexMap == adjacentTile2.m_HexMap || cNode.IsBridge || cNode3.IsBridge))
		{
			if (cTile.m_HexMap != adjacentTile.m_HexMap && !cNode.IsBridge)
			{
				return !cNode2.IsBridge;
			}
			return false;
		}
		return true;
	}

	private static bool InternalHaveLOS(CTile sourceTile, CTile targetTile, LOSCallbackType callback = null)
	{
		Vector vector = default(Vector);
		MF.ArrayIndexToCartesianCoord(sourceTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector.X, out vector.Y);
		Vector vector2 = default(Vector);
		MF.ArrayIndexToCartesianCoord(targetTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector2.X, out vector2.Y);
		Vector vector3 = vector2 - vector;
		Vector vector4 = vector3.Normalise();
		float num = vector3.Magnitude() + s_LOSTileScalar.X * 0.5f;
		Vector vector5 = new Vector(vector4.Y, 0f - vector4.X);
		List<Point> list = new List<Point>();
		List<CMap> list2 = new List<CMap>();
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				Point point = new Point(j, i);
				Vector vector6 = default(Vector);
				MF.ArrayIndexToCartesianCoord(point, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector6.X, out vector6.Y);
				Vector vector7 = vector6 - vector;
				if (vector7.Magnitude() < num && Math.Abs(vector7 * vector5) < s_LOSTileScalar.X && vector7 * vector4 > 0f - s_LOSTileScalar.X)
				{
					callback?.Invoke(vector6, vector6 + new Vector(0.25f, 0f), rayBlocked: false);
					list.Add(point);
				}
			}
		}
		for (float num2 = 0f; num2 < 420f; num2 += 60f)
		{
			float num3 = num2;
			float num4 = num2;
			if (num2 >= 360f)
			{
				if (!ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenLOS))
				{
					continue;
				}
				num3 = 90f;
				num4 = 0f;
			}
			Vector vector8 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f);
			float num5 = (float)Math.Cos((double)(0f - num3) * (Math.PI / 180.0));
			float num6 = (float)Math.Sin((double)(0f - num4) * (Math.PI / 180.0));
			float x = vector8.X * num5 - vector8.Y * num6;
			float y = vector8.X * num6 + vector8.Y * num5;
			vector8 = new Vector(x, y);
			Vector vector9 = default(Vector);
			MF.ArrayIndexToCartesianCoord(sourceTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector9.X, out vector9.Y);
			vector9 += vector8;
			if (!ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenLOS) && num2 < 360f && IsEdgeOnWall(sourceTile.m_ArrayIndex, num2))
			{
				callback?.Invoke(vector9, vector9, rayBlocked: true, "square");
				continue;
			}
			for (float num7 = 0f; num7 < 420f; num7 += 60f)
			{
				float num8 = num7;
				float num9 = num7;
				if (num7 >= 360f)
				{
					if (!ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenLOS))
					{
						continue;
					}
					num8 = 90f;
					num9 = 0f;
				}
				Vector vector10 = new Vector(0f, s_LOSTileScalar.Y / 0.75f * 0.5f);
				num5 = (float)Math.Cos((double)(0f - num8) * (Math.PI / 180.0));
				num6 = (float)Math.Sin((double)(0f - num9) * (Math.PI / 180.0));
				x = vector10.X * num5 - vector10.Y * num6;
				y = vector10.X * num6 + vector10.Y * num5;
				vector10 = new Vector(x, y);
				Vector vector11 = default(Vector);
				MF.ArrayIndexToCartesianCoord(targetTile.m_ArrayIndex, s_LOSTileScalar.X, s_LOSTileScalar.Y, out vector11.X, out vector11.Y);
				vector11 += vector10;
				if (!ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenLOS) && num7 < 360f && IsEdgeOnWall(targetTile.m_ArrayIndex, num7))
				{
					callback?.Invoke(vector11, vector11, rayBlocked: true, "square");
					continue;
				}
				foreach (Point item in list)
				{
					CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
					CNode cNode = ScenarioManager.PathFinder.Nodes[item.X, item.Y];
					if (cTile == null || cNode == null || !cNode.IsBridge)
					{
						continue;
					}
					LOSRayBlocked(sourceTile, targetTile, vector9, vector11, item, out var rayIntersects, new List<CMap>(), callback);
					if (rayIntersects || (sourceTile.m_ArrayIndex.X == cTile.m_ArrayIndex.X && sourceTile.m_ArrayIndex.Y == cTile.m_ArrayIndex.Y))
					{
						if (cTile.m_HexMap != null && cTile.m_HexMap.Revealed && !list2.Contains(cTile.m_HexMap))
						{
							list2.Add(cTile.m_HexMap);
						}
						if (cTile.m_Hex2Map != null && cTile.m_Hex2Map.Revealed && !list2.Contains(cTile.m_Hex2Map))
						{
							list2.Add(cTile.m_Hex2Map);
						}
					}
				}
				bool flag = false;
				foreach (Point item2 in list)
				{
					if (item2.X != sourceTile.m_ArrayIndex.X || item2.Y != sourceTile.m_ArrayIndex.Y)
					{
						flag = LOSRayBlocked(sourceTile, targetTile, vector9, vector11, item2, out var _, list2, callback);
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					if (callback == null)
					{
						return true;
					}
					callback(vector9, vector11, flag);
				}
			}
		}
		return false;
	}

	public static bool HaveLOS(CTile sourceTile, CTile targetTile, LOSCallbackType callback = null)
	{
		if (InternalHaveLOS(sourceTile, targetTile, callback))
		{
			return InternalHaveLOS(targetTile, sourceTile, callback);
		}
		return false;
	}

	public CActor()
	{
	}

	public CActor(CActor state, ReferenceDictionary references)
	{
		AccumulativeOverheal = state.AccumulativeOverheal;
		MinimumOverheal = state.MinimumOverheal;
		HealthReduction = state.HealthReduction;
		IgnoreDifficultTerrain = state.IgnoreDifficultTerrain;
		IgnoreHazardousTerrain = state.IgnoreHazardousTerrain;
		IncomingAttackDamage = state.IncomingAttackDamage;
		ActorGuid = state.ActorGuid;
		Augments = references.Get(state.Augments);
		if (Augments == null && state.Augments != null)
		{
			Augments = new List<CAugment>();
			for (int i = 0; i < state.Augments.Count; i++)
			{
				CAugment cAugment = state.Augments[i];
				CAugment cAugment2 = references.Get(cAugment);
				if (cAugment2 == null && cAugment != null)
				{
					cAugment2 = new CAugment(cAugment, references);
					references.Add(cAugment, cAugment2);
				}
				Augments.Add(cAugment2);
			}
			references.Add(state.Augments, Augments);
		}
		Songs = references.Get(state.Songs);
		if (Songs == null && state.Songs != null)
		{
			Songs = new List<CSong>();
			for (int j = 0; j < state.Songs.Count; j++)
			{
				CSong cSong = state.Songs[j];
				CSong cSong2 = references.Get(cSong);
				if (cSong2 == null && cSong != null)
				{
					cSong2 = new CSong(cSong, references);
					references.Add(cSong, cSong2);
				}
				Songs.Add(cSong2);
			}
			references.Add(state.Songs, Songs);
		}
		Dooms = references.Get(state.Dooms);
		if (Dooms == null && state.Dooms != null)
		{
			Dooms = new List<CDoom>();
			for (int k = 0; k < state.Dooms.Count; k++)
			{
				CDoom cDoom = state.Dooms[k];
				CDoom cDoom2 = references.Get(cDoom);
				if (cDoom2 == null && cDoom != null)
				{
					cDoom2 = new CDoom(cDoom, references);
					references.Add(cDoom, cDoom2);
				}
				Dooms.Add(cDoom2);
			}
			references.Add(state.Dooms, Dooms);
		}
		CachedRemovedOnDeathDooms = references.Get(state.CachedRemovedOnDeathDooms);
		if (CachedRemovedOnDeathDooms == null && state.CachedRemovedOnDeathDooms != null)
		{
			CachedRemovedOnDeathDooms = new List<CDoom>();
			for (int l = 0; l < state.CachedRemovedOnDeathDooms.Count; l++)
			{
				CDoom cDoom3 = state.CachedRemovedOnDeathDooms[l];
				CDoom cDoom4 = references.Get(cDoom3);
				if (cDoom4 == null && cDoom3 != null)
				{
					cDoom4 = new CDoom(cDoom3, references);
					references.Add(cDoom3, cDoom4);
				}
				CachedRemovedOnDeathDooms.Add(cDoom4);
			}
			references.Add(state.CachedRemovedOnDeathDooms, CachedRemovedOnDeathDooms);
		}
		DoomSlots = state.DoomSlots;
		CauseOfDeath = state.CauseOfDeath;
		KilledByActorGuid = state.KilledByActorGuid;
		MindControlDuration = state.MindControlDuration;
		ActorActionHasHappened = state.ActorActionHasHappened;
		HasTakenWoundDamageThisTurn = state.HasTakenWoundDamageThisTurn;
		ExhaustAfterAction = state.ExhaustAfterAction;
		PendingExtraTurnOfTypeStack = references.Get(state.PendingExtraTurnOfTypeStack);
		if (PendingExtraTurnOfTypeStack == null && state.PendingExtraTurnOfTypeStack != null)
		{
			PendingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
			CAbilityExtraTurn.EExtraTurnType[] array = state.PendingExtraTurnOfTypeStack.ToArray();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				CAbilityExtraTurn.EExtraTurnType item = array[num];
				PendingExtraTurnOfTypeStack.Push(item);
			}
			references.Add(state.PendingExtraTurnOfTypeStack, PendingExtraTurnOfTypeStack);
		}
		TakingExtraTurnOfTypeStack = references.Get(state.TakingExtraTurnOfTypeStack);
		if (TakingExtraTurnOfTypeStack == null && state.TakingExtraTurnOfTypeStack != null)
		{
			TakingExtraTurnOfTypeStack = new Stack<CAbilityExtraTurn.EExtraTurnType>();
			CAbilityExtraTurn.EExtraTurnType[] array2 = state.TakingExtraTurnOfTypeStack.ToArray();
			for (int num2 = array2.Length - 1; num2 >= 0; num2--)
			{
				CAbilityExtraTurn.EExtraTurnType item2 = array2[num2];
				TakingExtraTurnOfTypeStack.Push(item2);
			}
			references.Add(state.TakingExtraTurnOfTypeStack, TakingExtraTurnOfTypeStack);
		}
		IsDoomed = state.IsDoomed;
		NoGoldDrop = state.NoGoldDrop;
		PhasedOut = state.PhasedOut;
		Deactivated = state.Deactivated;
		ChosenModelIndex = state.ChosenModelIndex;
		CrowFlyDistance = state.CrowFlyDistance;
		TeleportedToPropGuids = references.Get(state.TeleportedToPropGuids);
		if (TeleportedToPropGuids == null && state.TeleportedToPropGuids != null)
		{
			TeleportedToPropGuids = new List<string>();
			for (int m = 0; m < state.TeleportedToPropGuids.Count; m++)
			{
				string item3 = state.TeleportedToPropGuids[m];
				TeleportedToPropGuids.Add(item3);
			}
			references.Add(state.TeleportedToPropGuids, TeleportedToPropGuids);
		}
		CachedShieldValue = state.CachedShieldValue;
		CachedHasRetaliate = state.CachedHasRetaliate;
		CachedShieldNeutralized = state.CachedShieldNeutralized;
		CachedHealingBlocked = state.CachedHealingBlocked;
		CachedFlying = state.CachedFlying;
		CachedDoomActiveBonuses = references.Get(state.CachedDoomActiveBonuses);
		if (CachedDoomActiveBonuses == null && state.CachedDoomActiveBonuses != null)
		{
			CachedDoomActiveBonuses = new List<CActiveBonus>();
			for (int n = 0; n < state.CachedDoomActiveBonuses.Count; n++)
			{
				CActiveBonus cActiveBonus = state.CachedDoomActiveBonuses[n];
				CActiveBonus cActiveBonus2 = references.Get(cActiveBonus);
				if (cActiveBonus2 == null && cActiveBonus != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus is CAddConditionActiveBonus state2) ? new CAddConditionActiveBonus(state2, references) : ((cActiveBonus is CAddHealActiveBonus state3) ? new CAddHealActiveBonus(state3, references) : ((cActiveBonus is CAddRangeActiveBonus state4) ? new CAddRangeActiveBonus(state4, references) : ((cActiveBonus is CAddTargetActiveBonus state5) ? new CAddTargetActiveBonus(state5, references) : ((cActiveBonus is CAdjustInitiativeActiveBonus state6) ? new CAdjustInitiativeActiveBonus(state6, references) : ((cActiveBonus is CAdvantageActiveBonus state7) ? new CAdvantageActiveBonus(state7, references) : ((cActiveBonus is CAttackActiveBonus state8) ? new CAttackActiveBonus(state8, references) : ((cActiveBonus is CAttackersGainDisadvantageActiveBonus state9) ? new CAttackersGainDisadvantageActiveBonus(state9, references) : ((cActiveBonus is CChangeCharacterModelActiveBonus state10) ? new CChangeCharacterModelActiveBonus(state10, references) : ((cActiveBonus is CChangeConditionActiveBonus state11) ? new CChangeConditionActiveBonus(state11, references) : ((cActiveBonus is CChangeModifierActiveBonus state12) ? new CChangeModifierActiveBonus(state12, references) : ((cActiveBonus is CChooseAbilityActiveBonus state13) ? new CChooseAbilityActiveBonus(state13, references) : ((cActiveBonus is CDamageActiveBonus state14) ? new CDamageActiveBonus(state14, references) : ((cActiveBonus is CDisableCardActionActiveBonus state15) ? new CDisableCardActionActiveBonus(state15, references) : ((cActiveBonus is CDuringActionAbilityActiveBonus state16) ? new CDuringActionAbilityActiveBonus(state16, references) : ((cActiveBonus is CDuringTurnAbilityActiveBonus state17) ? new CDuringTurnAbilityActiveBonus(state17, references) : ((cActiveBonus is CEndActionAbilityActiveBonus state18) ? new CEndActionAbilityActiveBonus(state18, references) : ((cActiveBonus is CEndRoundAbilityActiveBonus state19) ? new CEndRoundAbilityActiveBonus(state19, references) : ((cActiveBonus is CEndTurnAbilityActiveBonus state20) ? new CEndTurnAbilityActiveBonus(state20, references) : ((cActiveBonus is CForgoActionsForCompanionActiveBonus state21) ? new CForgoActionsForCompanionActiveBonus(state21, references) : ((cActiveBonus is CHealthReductionActiveBonus state22) ? new CHealthReductionActiveBonus(state22, references) : ((cActiveBonus is CImmunityActiveBonus state23) ? new CImmunityActiveBonus(state23, references) : ((cActiveBonus is CInfuseActiveBonus state24) ? new CInfuseActiveBonus(state24, references) : ((cActiveBonus is CInvulnerabilityActiveBonus state25) ? new CInvulnerabilityActiveBonus(state25, references) : ((cActiveBonus is CItemLockActiveBonus state26) ? new CItemLockActiveBonus(state26, references) : ((cActiveBonus is CLootActiveBonus state27) ? new CLootActiveBonus(state27, references) : ((cActiveBonus is CMoveActiveBonus state28) ? new CMoveActiveBonus(state28, references) : ((cActiveBonus is COverhealActiveBonus state29) ? new COverhealActiveBonus(state29, references) : ((cActiveBonus is COverrideAbilityTypeActiveBonus state30) ? new COverrideAbilityTypeActiveBonus(state30, references) : ((cActiveBonus is CPierceInvulnerabilityActiveBonus state31) ? new CPierceInvulnerabilityActiveBonus(state31, references) : ((cActiveBonus is CPreventDamageActiveBonus state32) ? new CPreventDamageActiveBonus(state32, references) : ((cActiveBonus is CRedirectActiveBonus state33) ? new CRedirectActiveBonus(state33, references) : ((cActiveBonus is CRetaliateActiveBonus state34) ? new CRetaliateActiveBonus(state34, references) : ((cActiveBonus is CShieldActiveBonus state35) ? new CShieldActiveBonus(state35, references) : ((cActiveBonus is CStartActionAbilityActiveBonus state36) ? new CStartActionAbilityActiveBonus(state36, references) : ((cActiveBonus is CStartRoundAbilityActiveBonus state37) ? new CStartRoundAbilityActiveBonus(state37, references) : ((cActiveBonus is CStartTurnAbilityActiveBonus state38) ? new CStartTurnAbilityActiveBonus(state38, references) : ((cActiveBonus is CSummonActiveBonus state39) ? new CSummonActiveBonus(state39, references) : ((!(cActiveBonus is CUntargetableActiveBonus state40)) ? new CActiveBonus(cActiveBonus, references) : new CUntargetableActiveBonus(state40, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus2 = cActiveBonus3;
					references.Add(cActiveBonus, cActiveBonus2);
				}
				CachedDoomActiveBonuses.Add(cActiveBonus2);
			}
			references.Add(state.CachedDoomActiveBonuses, CachedDoomActiveBonuses);
		}
		CachedAddDoomSlotActiveBonuses = references.Get(state.CachedAddDoomSlotActiveBonuses);
		if (CachedAddDoomSlotActiveBonuses == null && state.CachedAddDoomSlotActiveBonuses != null)
		{
			CachedAddDoomSlotActiveBonuses = new List<CActiveBonus>();
			for (int num3 = 0; num3 < state.CachedAddDoomSlotActiveBonuses.Count; num3++)
			{
				CActiveBonus cActiveBonus4 = state.CachedAddDoomSlotActiveBonuses[num3];
				CActiveBonus cActiveBonus5 = references.Get(cActiveBonus4);
				if (cActiveBonus5 == null && cActiveBonus4 != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus4 is CAddConditionActiveBonus state41) ? new CAddConditionActiveBonus(state41, references) : ((cActiveBonus4 is CAddHealActiveBonus state42) ? new CAddHealActiveBonus(state42, references) : ((cActiveBonus4 is CAddRangeActiveBonus state43) ? new CAddRangeActiveBonus(state43, references) : ((cActiveBonus4 is CAddTargetActiveBonus state44) ? new CAddTargetActiveBonus(state44, references) : ((cActiveBonus4 is CAdjustInitiativeActiveBonus state45) ? new CAdjustInitiativeActiveBonus(state45, references) : ((cActiveBonus4 is CAdvantageActiveBonus state46) ? new CAdvantageActiveBonus(state46, references) : ((cActiveBonus4 is CAttackActiveBonus state47) ? new CAttackActiveBonus(state47, references) : ((cActiveBonus4 is CAttackersGainDisadvantageActiveBonus state48) ? new CAttackersGainDisadvantageActiveBonus(state48, references) : ((cActiveBonus4 is CChangeCharacterModelActiveBonus state49) ? new CChangeCharacterModelActiveBonus(state49, references) : ((cActiveBonus4 is CChangeConditionActiveBonus state50) ? new CChangeConditionActiveBonus(state50, references) : ((cActiveBonus4 is CChangeModifierActiveBonus state51) ? new CChangeModifierActiveBonus(state51, references) : ((cActiveBonus4 is CChooseAbilityActiveBonus state52) ? new CChooseAbilityActiveBonus(state52, references) : ((cActiveBonus4 is CDamageActiveBonus state53) ? new CDamageActiveBonus(state53, references) : ((cActiveBonus4 is CDisableCardActionActiveBonus state54) ? new CDisableCardActionActiveBonus(state54, references) : ((cActiveBonus4 is CDuringActionAbilityActiveBonus state55) ? new CDuringActionAbilityActiveBonus(state55, references) : ((cActiveBonus4 is CDuringTurnAbilityActiveBonus state56) ? new CDuringTurnAbilityActiveBonus(state56, references) : ((cActiveBonus4 is CEndActionAbilityActiveBonus state57) ? new CEndActionAbilityActiveBonus(state57, references) : ((cActiveBonus4 is CEndRoundAbilityActiveBonus state58) ? new CEndRoundAbilityActiveBonus(state58, references) : ((cActiveBonus4 is CEndTurnAbilityActiveBonus state59) ? new CEndTurnAbilityActiveBonus(state59, references) : ((cActiveBonus4 is CForgoActionsForCompanionActiveBonus state60) ? new CForgoActionsForCompanionActiveBonus(state60, references) : ((cActiveBonus4 is CHealthReductionActiveBonus state61) ? new CHealthReductionActiveBonus(state61, references) : ((cActiveBonus4 is CImmunityActiveBonus state62) ? new CImmunityActiveBonus(state62, references) : ((cActiveBonus4 is CInfuseActiveBonus state63) ? new CInfuseActiveBonus(state63, references) : ((cActiveBonus4 is CInvulnerabilityActiveBonus state64) ? new CInvulnerabilityActiveBonus(state64, references) : ((cActiveBonus4 is CItemLockActiveBonus state65) ? new CItemLockActiveBonus(state65, references) : ((cActiveBonus4 is CLootActiveBonus state66) ? new CLootActiveBonus(state66, references) : ((cActiveBonus4 is CMoveActiveBonus state67) ? new CMoveActiveBonus(state67, references) : ((cActiveBonus4 is COverhealActiveBonus state68) ? new COverhealActiveBonus(state68, references) : ((cActiveBonus4 is COverrideAbilityTypeActiveBonus state69) ? new COverrideAbilityTypeActiveBonus(state69, references) : ((cActiveBonus4 is CPierceInvulnerabilityActiveBonus state70) ? new CPierceInvulnerabilityActiveBonus(state70, references) : ((cActiveBonus4 is CPreventDamageActiveBonus state71) ? new CPreventDamageActiveBonus(state71, references) : ((cActiveBonus4 is CRedirectActiveBonus state72) ? new CRedirectActiveBonus(state72, references) : ((cActiveBonus4 is CRetaliateActiveBonus state73) ? new CRetaliateActiveBonus(state73, references) : ((cActiveBonus4 is CShieldActiveBonus state74) ? new CShieldActiveBonus(state74, references) : ((cActiveBonus4 is CStartActionAbilityActiveBonus state75) ? new CStartActionAbilityActiveBonus(state75, references) : ((cActiveBonus4 is CStartRoundAbilityActiveBonus state76) ? new CStartRoundAbilityActiveBonus(state76, references) : ((cActiveBonus4 is CStartTurnAbilityActiveBonus state77) ? new CStartTurnAbilityActiveBonus(state77, references) : ((cActiveBonus4 is CSummonActiveBonus state78) ? new CSummonActiveBonus(state78, references) : ((!(cActiveBonus4 is CUntargetableActiveBonus state79)) ? new CActiveBonus(cActiveBonus4, references) : new CUntargetableActiveBonus(state79, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus5 = cActiveBonus3;
					references.Add(cActiveBonus4, cActiveBonus5);
				}
				CachedAddDoomSlotActiveBonuses.Add(cActiveBonus5);
			}
			references.Add(state.CachedAddDoomSlotActiveBonuses, CachedAddDoomSlotActiveBonuses);
		}
		CachedActiveItemEffectBonuses = references.Get(state.CachedActiveItemEffectBonuses);
		if (CachedActiveItemEffectBonuses == null && state.CachedActiveItemEffectBonuses != null)
		{
			CachedActiveItemEffectBonuses = new List<CActiveBonus>();
			for (int num4 = 0; num4 < state.CachedActiveItemEffectBonuses.Count; num4++)
			{
				CActiveBonus cActiveBonus6 = state.CachedActiveItemEffectBonuses[num4];
				CActiveBonus cActiveBonus7 = references.Get(cActiveBonus6);
				if (cActiveBonus7 == null && cActiveBonus6 != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus6 is CAddConditionActiveBonus state80) ? new CAddConditionActiveBonus(state80, references) : ((cActiveBonus6 is CAddHealActiveBonus state81) ? new CAddHealActiveBonus(state81, references) : ((cActiveBonus6 is CAddRangeActiveBonus state82) ? new CAddRangeActiveBonus(state82, references) : ((cActiveBonus6 is CAddTargetActiveBonus state83) ? new CAddTargetActiveBonus(state83, references) : ((cActiveBonus6 is CAdjustInitiativeActiveBonus state84) ? new CAdjustInitiativeActiveBonus(state84, references) : ((cActiveBonus6 is CAdvantageActiveBonus state85) ? new CAdvantageActiveBonus(state85, references) : ((cActiveBonus6 is CAttackActiveBonus state86) ? new CAttackActiveBonus(state86, references) : ((cActiveBonus6 is CAttackersGainDisadvantageActiveBonus state87) ? new CAttackersGainDisadvantageActiveBonus(state87, references) : ((cActiveBonus6 is CChangeCharacterModelActiveBonus state88) ? new CChangeCharacterModelActiveBonus(state88, references) : ((cActiveBonus6 is CChangeConditionActiveBonus state89) ? new CChangeConditionActiveBonus(state89, references) : ((cActiveBonus6 is CChangeModifierActiveBonus state90) ? new CChangeModifierActiveBonus(state90, references) : ((cActiveBonus6 is CChooseAbilityActiveBonus state91) ? new CChooseAbilityActiveBonus(state91, references) : ((cActiveBonus6 is CDamageActiveBonus state92) ? new CDamageActiveBonus(state92, references) : ((cActiveBonus6 is CDisableCardActionActiveBonus state93) ? new CDisableCardActionActiveBonus(state93, references) : ((cActiveBonus6 is CDuringActionAbilityActiveBonus state94) ? new CDuringActionAbilityActiveBonus(state94, references) : ((cActiveBonus6 is CDuringTurnAbilityActiveBonus state95) ? new CDuringTurnAbilityActiveBonus(state95, references) : ((cActiveBonus6 is CEndActionAbilityActiveBonus state96) ? new CEndActionAbilityActiveBonus(state96, references) : ((cActiveBonus6 is CEndRoundAbilityActiveBonus state97) ? new CEndRoundAbilityActiveBonus(state97, references) : ((cActiveBonus6 is CEndTurnAbilityActiveBonus state98) ? new CEndTurnAbilityActiveBonus(state98, references) : ((cActiveBonus6 is CForgoActionsForCompanionActiveBonus state99) ? new CForgoActionsForCompanionActiveBonus(state99, references) : ((cActiveBonus6 is CHealthReductionActiveBonus state100) ? new CHealthReductionActiveBonus(state100, references) : ((cActiveBonus6 is CImmunityActiveBonus state101) ? new CImmunityActiveBonus(state101, references) : ((cActiveBonus6 is CInfuseActiveBonus state102) ? new CInfuseActiveBonus(state102, references) : ((cActiveBonus6 is CInvulnerabilityActiveBonus state103) ? new CInvulnerabilityActiveBonus(state103, references) : ((cActiveBonus6 is CItemLockActiveBonus state104) ? new CItemLockActiveBonus(state104, references) : ((cActiveBonus6 is CLootActiveBonus state105) ? new CLootActiveBonus(state105, references) : ((cActiveBonus6 is CMoveActiveBonus state106) ? new CMoveActiveBonus(state106, references) : ((cActiveBonus6 is COverhealActiveBonus state107) ? new COverhealActiveBonus(state107, references) : ((cActiveBonus6 is COverrideAbilityTypeActiveBonus state108) ? new COverrideAbilityTypeActiveBonus(state108, references) : ((cActiveBonus6 is CPierceInvulnerabilityActiveBonus state109) ? new CPierceInvulnerabilityActiveBonus(state109, references) : ((cActiveBonus6 is CPreventDamageActiveBonus state110) ? new CPreventDamageActiveBonus(state110, references) : ((cActiveBonus6 is CRedirectActiveBonus state111) ? new CRedirectActiveBonus(state111, references) : ((cActiveBonus6 is CRetaliateActiveBonus state112) ? new CRetaliateActiveBonus(state112, references) : ((cActiveBonus6 is CShieldActiveBonus state113) ? new CShieldActiveBonus(state113, references) : ((cActiveBonus6 is CStartActionAbilityActiveBonus state114) ? new CStartActionAbilityActiveBonus(state114, references) : ((cActiveBonus6 is CStartRoundAbilityActiveBonus state115) ? new CStartRoundAbilityActiveBonus(state115, references) : ((cActiveBonus6 is CStartTurnAbilityActiveBonus state116) ? new CStartTurnAbilityActiveBonus(state116, references) : ((cActiveBonus6 is CSummonActiveBonus state117) ? new CSummonActiveBonus(state117, references) : ((!(cActiveBonus6 is CUntargetableActiveBonus state118)) ? new CActiveBonus(cActiveBonus6, references) : new CUntargetableActiveBonus(state118, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus7 = cActiveBonus3;
					references.Add(cActiveBonus6, cActiveBonus7);
				}
				CachedActiveItemEffectBonuses.Add(cActiveBonus7);
			}
			references.Add(state.CachedActiveItemEffectBonuses, CachedActiveItemEffectBonuses);
		}
		CachedDisableCardActionActiveBonuses = references.Get(state.CachedDisableCardActionActiveBonuses);
		if (CachedDisableCardActionActiveBonuses == null && state.CachedDisableCardActionActiveBonuses != null)
		{
			CachedDisableCardActionActiveBonuses = new List<CDisableCardActionActiveBonus>();
			for (int num5 = 0; num5 < state.CachedDisableCardActionActiveBonuses.Count; num5++)
			{
				CDisableCardActionActiveBonus cDisableCardActionActiveBonus = state.CachedDisableCardActionActiveBonuses[num5];
				CDisableCardActionActiveBonus cDisableCardActionActiveBonus2 = references.Get(cDisableCardActionActiveBonus);
				if (cDisableCardActionActiveBonus2 == null && cDisableCardActionActiveBonus != null)
				{
					cDisableCardActionActiveBonus2 = new CDisableCardActionActiveBonus(cDisableCardActionActiveBonus, references);
					references.Add(cDisableCardActionActiveBonus, cDisableCardActionActiveBonus2);
				}
				CachedDisableCardActionActiveBonuses.Add(cDisableCardActionActiveBonus2);
			}
			references.Add(state.CachedDisableCardActionActiveBonuses, CachedDisableCardActionActiveBonuses);
		}
		m_PlayedThisRound = state.m_PlayedThisRound;
		IsUnderMyControl = state.IsUnderMyControl;
		m_Type = state.m_Type;
		m_OriginalType = state.m_OriginalType;
		m_ID = state.m_ID;
		m_Health = state.m_Health;
		m_MaxHealth = state.m_MaxHealth;
		m_Gold = state.m_Gold;
		m_XP = state.m_XP;
		m_Level = state.m_Level;
		m_BaseAugmentSlots = state.m_BaseAugmentSlots;
		m_OverridedBaseAugmentSlots = state.m_OverridedBaseAugmentSlots;
		m_BaseSongSlots = state.m_BaseSongSlots;
		m_BaseDoomSlots = state.m_BaseDoomSlots;
		m_OnDeathAbilityUsed = state.m_OnDeathAbilityUsed;
		m_CarriedQuestProps = references.Get(state.m_CarriedQuestProps);
		if (m_CarriedQuestProps == null && state.m_CarriedQuestProps != null)
		{
			m_CarriedQuestProps = new List<CObjectProp>();
			for (int num6 = 0; num6 < state.m_CarriedQuestProps.Count; num6++)
			{
				CObjectProp cObjectProp = state.m_CarriedQuestProps[num6];
				CObjectProp cObjectProp2 = references.Get(cObjectProp);
				if (cObjectProp2 == null && cObjectProp != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp is CObjectChest state119) ? new CObjectChest(state119, references) : ((cObjectProp is CObjectDifficultTerrain state120) ? new CObjectDifficultTerrain(state120, references) : ((cObjectProp is CObjectDoor state121) ? new CObjectDoor(state121, references) : ((cObjectProp is CObjectGoldPile state122) ? new CObjectGoldPile(state122, references) : ((cObjectProp is CObjectHazardousTerrain state123) ? new CObjectHazardousTerrain(state123, references) : ((cObjectProp is CObjectMonsterGrave state124) ? new CObjectMonsterGrave(state124, references) : ((cObjectProp is CObjectObstacle state125) ? new CObjectObstacle(state125, references) : ((cObjectProp is CObjectPortal state126) ? new CObjectPortal(state126, references) : ((cObjectProp is CObjectPressurePlate state127) ? new CObjectPressurePlate(state127, references) : ((cObjectProp is CObjectQuestItem state128) ? new CObjectQuestItem(state128, references) : ((cObjectProp is CObjectResource state129) ? new CObjectResource(state129, references) : ((cObjectProp is CObjectTerrainVisual state130) ? new CObjectTerrainVisual(state130, references) : ((!(cObjectProp is CObjectTrap state131)) ? new CObjectProp(cObjectProp, references) : new CObjectTrap(state131, references))))))))))))));
					cObjectProp2 = cObjectProp3;
					references.Add(cObjectProp, cObjectProp2);
				}
				m_CarriedQuestProps.Add(cObjectProp2);
			}
			references.Add(state.m_CarriedQuestProps, m_CarriedQuestProps);
		}
		m_CharacterResources = references.Get(state.m_CharacterResources);
		if (m_CharacterResources == null && state.m_CharacterResources != null)
		{
			m_CharacterResources = new List<CCharacterResource>();
			for (int num7 = 0; num7 < state.m_CharacterResources.Count; num7++)
			{
				CCharacterResource cCharacterResource = state.m_CharacterResources[num7];
				CCharacterResource cCharacterResource2 = references.Get(cCharacterResource);
				if (cCharacterResource2 == null && cCharacterResource != null)
				{
					cCharacterResource2 = new CCharacterResource(cCharacterResource, references);
					references.Add(cCharacterResource, cCharacterResource2);
				}
				m_CharacterResources.Add(cCharacterResource2);
			}
			references.Add(state.m_CharacterResources, m_CharacterResources);
		}
		m_ActorsAttackedThisRound = references.Get(state.m_ActorsAttackedThisRound);
		if (m_ActorsAttackedThisRound == null && state.m_ActorsAttackedThisRound != null)
		{
			m_ActorsAttackedThisRound = new List<CActor>();
			for (int num8 = 0; num8 < state.m_ActorsAttackedThisRound.Count; num8++)
			{
				CActor cActor = state.m_ActorsAttackedThisRound[num8];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state132) ? new CObjectActor(state132, references) : ((cActor is CEnemyActor state133) ? new CEnemyActor(state133, references) : ((cActor is CHeroSummonActor state134) ? new CHeroSummonActor(state134, references) : ((!(cActor is CPlayerActor state135)) ? new CActor(cActor, references) : new CPlayerActor(state135, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_ActorsAttackedThisRound.Add(cActor2);
			}
			references.Add(state.m_ActorsAttackedThisRound, m_ActorsAttackedThisRound);
		}
		m_AbilityTypesPerformedThisTurn = references.Get(state.m_AbilityTypesPerformedThisTurn);
		if (m_AbilityTypesPerformedThisTurn == null && state.m_AbilityTypesPerformedThisTurn != null)
		{
			m_AbilityTypesPerformedThisTurn = new List<CAbility.EAbilityType>();
			for (int num9 = 0; num9 < state.m_AbilityTypesPerformedThisTurn.Count; num9++)
			{
				CAbility.EAbilityType item4 = state.m_AbilityTypesPerformedThisTurn[num9];
				m_AbilityTypesPerformedThisTurn.Add(item4);
			}
			references.Add(state.m_AbilityTypesPerformedThisTurn, m_AbilityTypesPerformedThisTurn);
		}
		m_AbilityTypesPerformedThisAction = references.Get(state.m_AbilityTypesPerformedThisAction);
		if (m_AbilityTypesPerformedThisAction == null && state.m_AbilityTypesPerformedThisAction != null)
		{
			m_AbilityTypesPerformedThisAction = new List<CAbility.EAbilityType>();
			for (int num10 = 0; num10 < state.m_AbilityTypesPerformedThisAction.Count; num10++)
			{
				CAbility.EAbilityType item5 = state.m_AbilityTypesPerformedThisAction[num10];
				m_AbilityTypesPerformedThisAction.Add(item5);
			}
			references.Add(state.m_AbilityTypesPerformedThisAction, m_AbilityTypesPerformedThisAction);
		}
		m_AIMoveFocusActors = references.Get(state.m_AIMoveFocusActors);
		if (m_AIMoveFocusActors == null && state.m_AIMoveFocusActors != null)
		{
			m_AIMoveFocusActors = new List<CActor>();
			for (int num11 = 0; num11 < state.m_AIMoveFocusActors.Count; num11++)
			{
				CActor cActor4 = state.m_AIMoveFocusActors[num11];
				CActor cActor5 = references.Get(cActor4);
				if (cActor5 == null && cActor4 != null)
				{
					CActor cActor3 = ((cActor4 is CObjectActor state136) ? new CObjectActor(state136, references) : ((cActor4 is CEnemyActor state137) ? new CEnemyActor(state137, references) : ((cActor4 is CHeroSummonActor state138) ? new CHeroSummonActor(state138, references) : ((!(cActor4 is CPlayerActor state139)) ? new CActor(cActor4, references) : new CPlayerActor(state139, references)))));
					cActor5 = cActor3;
					references.Add(cActor4, cActor5);
				}
				m_AIMoveFocusActors.Add(cActor5);
			}
			references.Add(state.m_AIMoveFocusActors, m_AIMoveFocusActors);
		}
		m_AIMoveFocusPath = references.Get(state.m_AIMoveFocusPath);
		if (m_AIMoveFocusPath == null && state.m_AIMoveFocusPath != null)
		{
			m_AIMoveFocusPath = new List<Point>();
			for (int num12 = 0; num12 < state.m_AIMoveFocusPath.Count; num12++)
			{
				Point item6 = state.m_AIMoveFocusPath[num12];
				m_AIMoveFocusPath.Add(item6);
			}
			references.Add(state.m_AIMoveFocusPath, m_AIMoveFocusPath);
		}
		m_AIMoveFocusTiles = references.Get(state.m_AIMoveFocusTiles);
		if (m_AIMoveFocusTiles == null && state.m_AIMoveFocusTiles != null)
		{
			m_AIMoveFocusTiles = new List<Point>();
			for (int num13 = 0; num13 < state.m_AIMoveFocusTiles.Count; num13++)
			{
				Point item7 = state.m_AIMoveFocusTiles[num13];
				m_AIMoveFocusTiles.Add(item7);
			}
			references.Add(state.m_AIMoveFocusTiles, m_AIMoveFocusTiles);
		}
		m_MovementPathSelected = state.m_MovementPathSelected;
		m_AIMoveFocusWaypoints = references.Get(state.m_AIMoveFocusWaypoints);
		if (m_AIMoveFocusWaypoints == null && state.m_AIMoveFocusWaypoints != null)
		{
			m_AIMoveFocusWaypoints = new List<CTile>();
			for (int num14 = 0; num14 < state.m_AIMoveFocusWaypoints.Count; num14++)
			{
				CTile cTile = state.m_AIMoveFocusWaypoints[num14];
				CTile cTile2 = references.Get(cTile);
				if (cTile2 == null && cTile != null)
				{
					cTile2 = new CTile(cTile, references);
					references.Add(cTile, cTile2);
				}
				m_AIMoveFocusWaypoints.Add(cTile2);
			}
			references.Add(state.m_AIMoveFocusWaypoints, m_AIMoveFocusWaypoints);
		}
		m_AIMoveRange = state.m_AIMoveRange;
		m_LastDamageAmount = state.m_LastDamageAmount;
		m_PreDamageHealth = state.m_PreDamageHealth;
	}
}
