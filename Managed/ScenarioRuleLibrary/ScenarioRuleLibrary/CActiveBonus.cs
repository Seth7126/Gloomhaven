using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CActiveBonus
{
	public enum EActiveBonusDurationType
	{
		NA,
		Ability,
		Round,
		Persistent,
		Summon,
		Turn
	}

	public enum EActiveBonusBehaviourType
	{
		None,
		BuffAttack,
		BuffHealPerActor,
		BuffHealPerAction,
		BuffRange,
		BuffTarget,
		ShieldIncomingAttacks,
		Retaliate,
		PreventAndRetaliate,
		MultiplyAddedConditions,
		EndTurnAbility,
		StartTurnAbility,
		OverrideAbility,
		ShieldTargetedAttacks,
		ShieldAndRetaliate,
		CastAbilityFromSummon,
		DuringActionAbility,
		BuffMove,
		ShieldAndDisadvantage,
		DefaultToggleBehaviour,
		BuffIncomingAttacks,
		BuffRetaliate,
		PreventAndApplyToActiveBonusCaster,
		DuringActionAbilityOnAttacked,
		BuffIncomingHeal,
		DuringActionAbilityOnKill,
		DuringActionAbilityOnMoved,
		AdjustInitiative,
		DuringActionAbilityOnAttackAbilityFinished,
		DuringActionAbilityOnAttackStart,
		AdjustWhenAttacked,
		FocusInitiative,
		ApplyConditionalConditions,
		StartActionAbility,
		EndActionAbility,
		DuringActionAbilityOnLongRest,
		DuringActionAbilityOnDeath,
		StartRoundAbility,
		EndRoundAbility,
		DamageOnCurseModApplied,
		StartTurnAbilityAfterXCasterTurns,
		DuringActionAbilityOnDamaged,
		BuffLoot,
		DuringTurnAbility,
		DamageOnReceivedDamage,
		AddAccumulativeOverhealAndHealIfMax,
		SetMinimumOverheal,
		PreventDamageByDiscardingPlayerCards,
		LimitLoot,
		HealOnHealingReceived,
		ApplyConditionOnLoseCondition,
		DuringActionAbilityOnAttacking,
		AddConditionUntilDamaged,
		AddAccumulativeOverheal,
		DuringActionAbilityOnMovedIntoCasterHex,
		PreventAndFullyHeal,
		DuringActionAbilityOnPreventedDamage,
		DuringActionAbilityOnFinishedMovement,
		HealOnDamageReceived,
		DuringActionAbilityOnAbilityEnded,
		DuringActionAbilityOnAttackFinished,
		DuringActionAbilityOnHealed,
		DuringActionAbilityOnHealedToFull,
		DuringActionAbilityOnCreated,
		OverrideMoveAbility,
		DuringActionAbilityOnMovedORCarried
	}

	public enum EActiveBonusRestrictionType
	{
		None,
		OncePerAction,
		OncePerTurn,
		OncePerRound
	}

	public enum EAuraTriggerAbilityAnimType
	{
		None,
		AnimateAuraOriginator,
		AnimateAuraReceiver,
		AnimateBoth
	}

	public static EActiveBonusDurationType[] ActiveBonusDurationTypes = (EActiveBonusDurationType[])Enum.GetValues(typeof(EActiveBonusDurationType));

	public static EActiveBonusBehaviourType[] ActiveBonusTypes = (EActiveBonusBehaviourType[])Enum.GetValues(typeof(EActiveBonusBehaviourType));

	public static EActiveBonusRestrictionType[] ActiveBonusRestrictionTypes = (EActiveBonusRestrictionType[])Enum.GetValues(typeof(EActiveBonusRestrictionType));

	public static EAuraTriggerAbilityAnimType[] AuraTriggerAbilityAnimTypes = (EAuraTriggerAbilityAnimType[])Enum.GetValues(typeof(EAuraTriggerAbilityAnimType));

	private CBaseCard m_BaseCard;

	private CAbility m_Ability;

	private bool m_Finishing;

	private bool m_Finished;

	private int m_TrackerIndex;

	private List<CActor> m_ValidActorsInRangeOfAura;

	private bool m_HasTracker;

	private List<CActor> m_RestrictedActors = new List<CActor>();

	private int m_ActiveBonusStartRound;

	protected CBespokeBehaviour m_BespokeBehaviour;

	public int ID { get; set; }

	public CBaseCard BaseCard => m_BaseCard;

	public CAbility Ability => m_Ability;

	public EActiveBonusDurationType Duration { get; set; }

	public ActiveBonusLayout Layout
	{
		get
		{
			return m_Ability.ActiveBonusYML;
		}
		set
		{
			m_Ability.ActiveBonusYML = value;
		}
	}

	public int TrackerIndex => m_TrackerIndex;

	public int ActiveBonusStartRound => m_ActiveBonusStartRound;

	public CBespokeBehaviour BespokeBehaviour => m_BespokeBehaviour;

	public int Remaining
	{
		get
		{
			if (!m_HasTracker)
			{
				return 0;
			}
			ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
			if (activeBonusYML == null || !(activeBonusYML.TrackerPattern?.Count > 0))
			{
				AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
				if (activeBonusData == null || !(activeBonusData.Tracker?.Count > 0))
				{
					return 0;
				}
				return m_Ability.ActiveBonusData.Tracker.Count - m_TrackerIndex;
			}
			return m_Ability.ActiveBonusYML.TrackerPattern.Count - m_TrackerIndex;
		}
	}

	public int InternalRoundNumber => ScenarioManager.CurrentScenarioState.RoundNumber - ActiveBonusStartRound;

	public CActor Actor { get; set; }

	public CActor Caster { get; private set; }

	public bool IsAura
	{
		get
		{
			if (m_Ability == null || m_Ability.ActiveBonusData == null)
			{
				return false;
			}
			return m_Ability.ActiveBonusData.IsAura;
		}
	}

	public CAbilityFilterContainer AuraFilter => m_Ability.ActiveBonusData.AuraFilter;

	public CAbility.EAbilityTargeting AuraTargeting => m_Ability.ActiveBonusData.AuraTargeting;

	public bool HasTracker => m_HasTracker;

	public bool IsAugment { get; set; }

	public bool IsSong { get; set; }

	public bool IsDoom { get; set; }

	public bool ToggledBonus { get; set; }

	public bool ToggleLocked { get; set; }

	public bool ApplyToAction { get; set; }

	public CActor SingleTarget { get; set; }

	public ElementInfusionBoardManager.EElement? ToggledElement { get; set; }

	public List<CCharacterResource> ToggledResourcesToConsume { get; set; }

	public bool FullCopy => m_Ability.ActiveBonusData.FullCopy;

	public bool NeedsDamage => m_Ability.ActiveBonusData.NeedsDamage;

	public List<CActor> ValidActorsInRangeOfAura => m_ValidActorsInRangeOfAura;

	public static int IDCounter { get; set; }

	public virtual bool Finished()
	{
		return m_Finished;
	}

	public bool Finishing()
	{
		return m_Finishing;
	}

	private static CActiveBonus GetBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int? iD, int? remaining, int activeBonusStartRound)
	{
		switch (ability.AbilityType)
		{
		case CAbility.EAbilityType.Attack:
			return new CAttackActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Move:
			return new CMoveActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Loot:
			return new CLootActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Shield:
			return new CShieldActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Retaliate:
			return new CRetaliateActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Damage:
			return new CDamageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Summon:
			return new CSummonActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD);
		case CAbility.EAbilityType.Advantage:
			return new CAdvantageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.PreventDamage:
			return new CPreventDamageActiveBonus(baseCard, ability as CAbilityPreventDamage, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ImmunityTo:
			return new CImmunityActiveBonus(baseCard, ability as CAbilityImmunityTo, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Redirect:
			return new CRedirectActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddTarget:
			return new CAddTargetActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Overheal:
			return new COverhealActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddHeal:
			return new CAddHealActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddRange:
			return new CAddRangeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AttackersGainDisadvantage:
			return new CAttackersGainDisadvantageActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AddCondition:
			return new CAddConditionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Infuse:
			return new CInfuseActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.AdjustInitiative:
			return new CAdjustInitiativeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ForgoActionsForCompanion:
			return new CForgoActionsForCompanionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeModifier:
			return new CChangeModifierActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeCondition:
			return new CChangeConditionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Immovable:
			((CAbilityImmovable)ability).ApplyThisCondition(actor);
			return new CActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChooseAbility:
			return new CChooseAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Invulnerability:
			return new CInvulnerabilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.PierceInvulnerability:
			return new CPierceInvulnerabilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.Untargetable:
			return new CUntargetableActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ItemLock:
			return new CItemLockActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.ChangeCharacterModel:
			return new CChangeCharacterModelActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.HealthReduction:
			return new CHealthReductionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		case CAbility.EAbilityType.DisableCardAction:
			return new CDisableCardActionActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		default:
			return new CActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
		}
	}

	public static CActiveBonus CreateActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null, bool isAugment = false, bool isSong = false, bool loadingItemBonus = false, bool isDoom = false)
	{
		if (ability.ActiveBonusData.OverrideAsSong)
		{
			isSong = true;
		}
		CActiveBonus cActiveBonus;
		if (ability.AbilityType == CAbility.EAbilityType.AddActiveBonus || ability.AbilityType == CAbility.EAbilityType.PlaySong)
		{
			switch (ability.ActiveBonusData.Behaviour)
			{
			case EActiveBonusBehaviourType.StartTurnAbility:
			case EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns:
				cActiveBonus = new CStartTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndTurnAbility:
				cActiveBonus = new CEndTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.StartRoundAbility:
				cActiveBonus = new CStartRoundAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndRoundAbility:
				cActiveBonus = new CEndRoundAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.StartActionAbility:
				cActiveBonus = new CStartActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.EndActionAbility:
				cActiveBonus = new CEndActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.DuringActionAbility:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacked:
			case EActiveBonusBehaviourType.DuringActionAbilityOnKill:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMoved:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackAbilityFinished:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackStart:
			case EActiveBonusBehaviourType.DuringActionAbilityOnLongRest:
			case EActiveBonusBehaviourType.DuringActionAbilityOnDeath:
			case EActiveBonusBehaviourType.DuringActionAbilityOnDamaged:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttacking:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedIntoCasterHex:
			case EActiveBonusBehaviourType.DuringActionAbilityOnPreventedDamage:
			case EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAbilityEnded:
			case EActiveBonusBehaviourType.DuringActionAbilityOnAttackFinished:
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealed:
			case EActiveBonusBehaviourType.DuringActionAbilityOnHealedToFull:
			case EActiveBonusBehaviourType.DuringActionAbilityOnCreated:
			case EActiveBonusBehaviourType.DuringActionAbilityOnMovedORCarried:
				cActiveBonus = new CDuringActionAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.DuringTurnAbility:
				cActiveBonus = new CDuringTurnAbilityActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			case EActiveBonusBehaviourType.OverrideAbility:
			case EActiveBonusBehaviourType.OverrideMoveAbility:
				cActiveBonus = new COverrideAbilityTypeActiveBonus(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining);
				break;
			default:
				cActiveBonus = GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound);
				break;
			}
		}
		else
		{
			cActiveBonus = GetBonus(baseCard, ability, actor, caster, iD, remaining, activeBonusStartRound);
		}
		if (ability.ActiveBonusData.SetFilterToCaster)
		{
			CAbilityFilter cAbilityFilter = ability.ActiveBonusData.Filter.AbilityFilters[0];
			if (cAbilityFilter != null)
			{
				if (cAbilityFilter.FilterPlayerClasses == null)
				{
					cAbilityFilter.FilterPlayerClasses = new List<string>();
				}
				cAbilityFilter.FilterPlayerClasses.Add(caster.Class.ID);
			}
		}
		if (baseCard.CardType != CBaseCard.ECardType.ScenarioModifier && ((baseCard.CardType == CBaseCard.ECardType.Item && !loadingItemBonus) || baseCard.CardType == CBaseCard.ECardType.AttackModifier || (cActiveBonus.IsAura && caster is CEnemyActor)))
		{
			if (caster.Class is CCharacterClass cCharacterClass)
			{
				if (baseCard.CardType != CBaseCard.ECardType.Item || baseCard.ActiveBonuses.Count <= 0)
				{
					cCharacterClass.ActivateCard(baseCard);
				}
			}
			else if (caster.Class is CHeroSummonClass)
			{
				((actor as CHeroSummonActor).Summoner.Class as CCharacterClass).ActivateCard(baseCard);
			}
			else if (caster.Class is CMonsterClass cMonsterClass)
			{
				cMonsterClass.ActivatedCards.Add(baseCard);
			}
		}
		if (isAugment)
		{
			cActiveBonus.IsAugment = isAugment;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (isSong)
		{
			cActiveBonus.IsSong = isSong;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (isDoom)
		{
			cActiveBonus.IsDoom = isDoom;
			cActiveBonus.Duration = EActiveBonusDurationType.Persistent;
		}
		if (cActiveBonus.IsAura)
		{
			cActiveBonus.FindValidActorsInRangeOfAura();
		}
		if (ability.ActiveBonusData.CancelActiveBonusesOnBaseCardIDs != null)
		{
			if (caster.Class is CCharacterClass cCharacterClass2)
			{
				foreach (CBaseCard activatedCard in cCharacterClass2.ActivatedCards)
				{
					if (!ability.ActiveBonusData.CancelActiveBonusesOnBaseCardIDs.Contains(activatedCard.ID))
					{
						continue;
					}
					foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
					{
						activeBonuse.Finish();
					}
				}
			}
			else
			{
				DLLDebug.LogError("ActiveBonusData.CancelActiveBonusesOnBaseCardIDs not currently supported for non player actors");
			}
		}
		return cActiveBonus;
	}

	public int BespokeBehaviourStrength()
	{
		if (m_BespokeBehaviour != null)
		{
			return m_BespokeBehaviour.Strength;
		}
		return 0;
	}

	public int ReferenceStrength(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				if (m_Ability == null)
				{
					return 0;
				}
				return m_Ability.Strength;
			}
			return m_BespokeBehaviour.ReferenceStrength(ability, target);
		}
		return 0;
	}

	public int ReferenceXP(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				if (m_Ability == null)
				{
					return 0;
				}
				return m_Ability.ActiveBonusData.ProcXP;
			}
			return m_BespokeBehaviour.ReferenceXP(ability, target);
		}
		return 0;
	}

	public int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return 1;
			}
			return m_BespokeBehaviour.ReferenceStrengthScalar(ability, target);
		}
		return 1;
	}

	public int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return 1;
			}
			return m_BespokeBehaviour.ReferenceAbilityStrengthScalar(ability, target);
		}
		return 1;
	}

	public string ReferenceAnimOverload(CAbility ability, CActor target)
	{
		if (IsActiveBonusToggledAndNotRestricted((ability != null) ? ability.TargetingActor : target))
		{
			if (m_BespokeBehaviour == null)
			{
				return string.Empty;
			}
			return m_BespokeBehaviour.ReferenceAnimOverload(ability, target);
		}
		return string.Empty;
	}

	public bool ActiveBonusIsActivatedByTile(CTile tile)
	{
		if (IsActiveBonusToggledAndNotRestricted(Actor))
		{
			if (m_BespokeBehaviour == null)
			{
				return false;
			}
			return m_BespokeBehaviour.ActiveBonusIsActivatedByTile(tile);
		}
		return false;
	}

	public bool StackActiveBonusInlineAbility(CActor target, CAbility ability)
	{
		if (m_BespokeBehaviour != null)
		{
			return m_BespokeBehaviour.StackActiveBonusInlineAbility(target, ability);
		}
		return false;
	}

	public void UpdateActiveBonusInlineAbilityTarget(CActor target)
	{
		if (m_BespokeBehaviour != null)
		{
			m_BespokeBehaviour.UpdateActiveBonusInlineAbilityTarget(target);
		}
	}

	public void ToggleActiveBonus(ElementInfusionBoardManager.EElement? toggleElement, CActor actorToggling, object extraOption = null)
	{
		if (!m_Ability.ActiveBonusData.IsToggleBonus || ToggleLocked)
		{
			return;
		}
		if (m_Ability.ActiveBonusData.Consuming.Count > 0)
		{
			if (!ToggledBonus && toggleElement.HasValue && m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => x == toggleElement.Value || x == ElementInfusionBoardManager.EElement.Any))
			{
				ToggledElement = toggleElement.Value;
			}
			else
			{
				ToggledElement = null;
			}
		}
		if (m_Ability.ActiveBonusData.RequiredResources.Count > 0 && m_Ability.ActiveBonusData.ConsumeResources)
		{
			if (!ToggledBonus)
			{
				foreach (KeyValuePair<string, int> requiredResource in m_Ability.ActiveBonusData.RequiredResources)
				{
					ToggledResourcesToConsume.Add(new CCharacterResource(requiredResource.Key, requiredResource.Value));
				}
			}
			else
			{
				ToggledResourcesToConsume.Clear();
			}
		}
		ToggledBonus = !ToggledBonus;
		if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction)
		{
			CAbility ability = cPhaseAction.CurrentPhaseAbility.m_Ability;
			if (m_Ability.ActiveBonusData.IsSingleTargetBonus)
			{
				ability.ToggleSingleTargetActiveBonus(this);
			}
			else
			{
				if (m_Ability.Song != null)
				{
					foreach (CSong.SongEffect songEffect in m_Ability.Song.SongEffects)
					{
						songEffect.OnSongEffectToggled(ability, ToggledBonus);
					}
				}
				ability.ActiveBonusToggled(actorToggling, this);
				if (m_BespokeBehaviour != null)
				{
					m_BespokeBehaviour.OnActiveBonusToggled(ability, ToggledBonus);
				}
			}
			cPhaseAction.HandleActiveBonusCostAbilityToggling(this, m_Ability.ActiveBonusData.CostAbility, ToggledBonus);
		}
		else if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			if (m_BespokeBehaviour is CDuringTurnAbilityActiveBonus_TriggerAbility)
			{
				m_BespokeBehaviour.OnActiveBonusToggled((CAbility)null, ToggledBonus);
			}
		}
		else if (m_Ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.StartActionAbility || m_Ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.EndActionAbility)
		{
			if (m_BespokeBehaviour != null)
			{
				m_BespokeBehaviour.OnActiveBonusToggled(actorToggling, ToggledBonus);
			}
		}
		else if (m_Ability.AbilityType == CAbility.EAbilityType.AdjustInitiative && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.CheckForInitiativeAdjustments && m_BespokeBehaviour != null && m_BespokeBehaviour is CAdjustInitiativeActiveBonus_AdjustInitiative cAdjustInitiativeActiveBonus_AdjustInitiative)
		{
			if (ToggledBonus && extraOption != null)
			{
				cAdjustInitiativeActiveBonus_AdjustInitiative.OnToggleInitiativeBonus((bool)extraOption);
			}
			else
			{
				cAdjustInitiativeActiveBonus_AdjustInitiative.OnUntoggleInitiativeBonus();
			}
		}
	}

	public void ResetRestriction(EActiveBonusRestrictionType restrictionType)
	{
		if (m_Ability.ActiveBonusData.Restriction == restrictionType)
		{
			m_RestrictedActors.Clear();
			if (m_BespokeBehaviour != null)
			{
				m_BespokeBehaviour.OnRestrictionReset();
			}
		}
	}

	public void OnActionEnded(CActor actorEndingAction)
	{
		if (m_BespokeBehaviour != null)
		{
			m_BespokeBehaviour.OnActionEnded(actorEndingAction);
			m_BespokeBehaviour.AppliedThisAction = false;
		}
	}

	public void RestrictActiveBonus(CActor restrictForActor)
	{
		if (m_Ability.ActiveBonusData.Restriction != EActiveBonusRestrictionType.None && !m_RestrictedActors.Contains(restrictForActor))
		{
			m_RestrictedActors.Add(restrictForActor);
		}
		ActiveBonusUsed(restrictForActor);
	}

	public void ActiveBonusUsed(CActor actor)
	{
		if (BaseCard.CardType != CBaseCard.ECardType.Item || m_HasTracker || !(Actor.Class is CCharacterClass))
		{
			return;
		}
		CItem cItem = Actor.Inventory.AllItems.Find((CItem s) => s.ID == BaseCard.ID);
		if (cItem.YMLData.Usage == CItem.EUsageType.Unrestricted && cItem.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect && cItem.YMLData.UsedWhenEquipped != true)
		{
			if (m_TrackerIndex < 1)
			{
				m_TrackerIndex++;
			}
			Actor.UsedItem(cItem, m_TrackerIndex < 1);
		}
	}

	public bool IsRestricted(CActor checkRestrictActor)
	{
		if (m_Ability.ActiveBonusData.Restriction != EActiveBonusRestrictionType.None)
		{
			if (m_Ability.ActiveBonusData.RestrictPerActor)
			{
				return m_RestrictedActors.Contains(checkRestrictActor);
			}
			return m_RestrictedActors.Count > 0;
		}
		return false;
	}

	public bool RequirementsMet(CActor actor = null)
	{
		if (m_Ability.ActiveBonusData.Requirements != null)
		{
			CAbility ability = m_Ability;
			if (m_Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				ability = cAbilityAddActiveBonus.AddAbility;
			}
			if (m_Ability.ActiveBonusData.Requirements.MeetsAbilityRequirements(actor, ability))
			{
				return m_Ability.ActiveBonusData.Requirements.MeetsActiveBonusRequirements(this);
			}
			return false;
		}
		return true;
	}

	public void PayRequirementCost()
	{
		if (m_Ability.ActiveBonusData.Requirements != null)
		{
			m_Ability.ActiveBonusData.Requirements.PayRequirementCost();
		}
	}

	public bool IsActiveBonusToggledAndNotRestricted(CActor checkActor)
	{
		if (m_Ability.ActiveBonusData.Consuming.Count > 0)
		{
			if (!m_Ability.ActiveBonusData.IsToggleBonus)
			{
				return m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.GetAvailableElements().Any((ElementInfusionBoardManager.EElement y) => x == y));
			}
			if (ToggledBonus)
			{
				return !IsRestricted(checkActor);
			}
			return false;
		}
		if (m_Ability.ActiveBonusData.IsToggleBonus)
		{
			if (ToggledBonus)
			{
				return !IsRestricted(checkActor);
			}
			return false;
		}
		return !IsRestricted(checkActor);
	}

	public void ActiveBonusConsumeElements()
	{
		if (m_Ability.ActiveBonusData.Consuming.Count <= 0)
		{
			return;
		}
		if (m_Ability.ActiveBonusData.IsToggleBonus)
		{
			if (ToggledBonus)
			{
				CActiveBonusConsumedElementsCombatLog_MessageData message = new CActiveBonusConsumedElementsCombatLog_MessageData(Actor)
				{
					m_ActiveBonus = this,
					m_ElementsConsumed = new List<ElementInfusionBoardManager.EElement> { ToggledElement.Value }
				};
				ScenarioRuleClient.MessageHandler(message);
				ElementInfusionBoardManager.Consume(ToggledElement.Value, Actor);
			}
			return;
		}
		CActiveBonusConsumedElementsCombatLog_MessageData message2 = new CActiveBonusConsumedElementsCombatLog_MessageData(Actor)
		{
			m_ActiveBonus = this,
			m_ElementsConsumed = m_Ability.ActiveBonusData.Consuming
		};
		ScenarioRuleClient.MessageHandler(message2);
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, Actor);
		}
	}

	public bool CanToggleActiveBonus(CActor actor)
	{
		if (m_Ability.ActiveBonusData.IsToggleBonus)
		{
			if (m_Ability.ActiveBonusData.Consuming.Count > 0 && !m_Ability.ActiveBonusData.Consuming.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.GetAvailableElements().Any((ElementInfusionBoardManager.EElement y) => x == y)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
	}

	public CAbility.EAbilityType Type()
	{
		if (m_Ability == null)
		{
			return CAbility.EAbilityType.None;
		}
		return m_Ability.AbilityType;
	}

	protected CActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
	{
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.DefaultToggleBehaviour)
		{
			m_BespokeBehaviour = new CActiveBonus_DefaultToggleBehaviour(actor, ability, this);
		}
		ID = (iD.HasValue ? iD.Value : (++IDCounter));
		m_BaseCard = baseCard;
		m_Ability = ability;
		Actor = actor;
		Caster = caster;
		m_TrackerIndex = 0;
		ApplyToAction = false;
		m_ValidActorsInRangeOfAura = new List<CActor>();
		ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
		int hasTracker;
		if (activeBonusYML == null || !(activeBonusYML.TrackerPattern?.Count > 0))
		{
			AbilityData.ActiveBonusData activeBonusData = m_Ability.ActiveBonusData;
			hasTracker = ((activeBonusData != null && activeBonusData.Tracker?.Count > 0) ? 1 : 0);
		}
		else
		{
			hasTracker = 1;
		}
		m_HasTracker = (byte)hasTracker != 0;
		Duration = m_Ability.ActiveBonusData.Duration;
		m_ActiveBonusStartRound = activeBonusStartRound;
		if (m_HasTracker)
		{
			if (remaining.HasValue)
			{
				ActiveBonusLayout activeBonusYML2 = m_Ability.ActiveBonusYML;
				if (activeBonusYML2 != null && activeBonusYML2.TrackerPattern?.Count > 0)
				{
					m_TrackerIndex = m_Ability.ActiveBonusYML.TrackerPattern.Count - remaining.Value;
				}
				else
				{
					m_TrackerIndex = m_Ability.ActiveBonusData.Tracker.Count - remaining.Value;
				}
			}
			UpdateXPTracker(processTracker: false);
			if (Remaining <= 0)
			{
				Finish();
			}
		}
		else if (m_Ability.Augment != null)
		{
			CActiveBonusAugmentAdded_MessageData message = new CActiveBonusAugmentAdded_MessageData(Actor)
			{
				m_Actor = Actor,
				m_ActiveBonus = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void UpdateXPTracker(bool processTracker = true)
	{
		if ((m_Ability.ActiveBonusYML == null || m_Ability.ActiveBonusYML.TrackerPattern == null || m_TrackerIndex >= m_Ability.ActiveBonusYML.TrackerPattern.Count) && (m_Ability.ActiveBonusData.Tracker == null || m_TrackerIndex >= m_Ability.ActiveBonusData.Tracker.Count))
		{
			return;
		}
		if (processTracker)
		{
			ActiveBonusLayout activeBonusYML = m_Ability.ActiveBonusYML;
			if (activeBonusYML != null && activeBonusYML.TrackerPattern?.Count > 0)
			{
				ActiveBonusLayout activeBonusYML2 = m_Ability.ActiveBonusYML;
				if (activeBonusYML2 != null && activeBonusYML2.TrackerPattern?[m_TrackerIndex] > 0)
				{
					Actor.GainXP(m_Ability.ActiveBonusYML.TrackerPattern[m_TrackerIndex]);
				}
			}
			m_TrackerIndex++;
			if (BaseCard.CardType == CBaseCard.ECardType.Item && Actor.Class is CCharacterClass)
			{
				CItem item = Actor.Inventory.AllItems.Find((CItem s) => s.ID == BaseCard.ID);
				Actor.UsedItem(item, m_TrackerIndex < 2);
			}
		}
		CActiveBonusTrackerIncremented_MessageData message = new CActiveBonusTrackerIncremented_MessageData(Actor)
		{
			m_Actor = Actor,
			m_ActiveBonus = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static EActiveBonusDurationType GetActiveBonusDurationType(string duration)
	{
		string text = duration.ToLower();
		if (!(text == "roundbonus"))
		{
			if (text == "persistentbonus")
			{
				return EActiveBonusDurationType.Persistent;
			}
			return EActiveBonusDurationType.NA;
		}
		return EActiveBonusDurationType.Round;
	}

	public virtual void Finish()
	{
		try
		{
			m_Finishing = true;
			if (m_BespokeBehaviour != null)
			{
				m_BespokeBehaviour.OnFinished();
				m_BespokeBehaviour.RemoveListeners();
			}
			m_ValidActorsInRangeOfAura.Clear();
			if (IsAura)
			{
				CFinishAura_MessageData message = new CFinishAura_MessageData(Actor)
				{
					m_AuraAbilityID = BaseCard.ID
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (m_Ability.ActiveBonusData.EndAllActiveBonusesOnBaseCardSimultaneously)
			{
				foreach (CActiveBonus item4 in BaseCard.ActiveBonuses.ToList())
				{
					if (item4 != this && !item4.Finished() && !item4.Finishing())
					{
						item4.Finish();
					}
				}
			}
			if (m_Ability.ActiveBonusData.RemoveAllInstancesOfResourcesOnFinish != null)
			{
				foreach (string resourceID in m_Ability.ActiveBonusData.RemoveAllInstancesOfResourcesOnFinish)
				{
					foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
					{
						CCharacterResource cCharacterResource = allActor.CharacterResources.SingleOrDefault((CCharacterResource x) => x.ID == resourceID);
						if (cCharacterResource != null)
						{
							allActor.RemoveCharacterResource(resourceID, cCharacterResource.Amount);
						}
					}
					foreach (CObjectProp resourceProp in ScenarioManager.CurrentScenarioState.ResourceProps)
					{
						if (resourceProp is CObjectResource cObjectResource && cObjectResource.ResourceID == resourceID)
						{
							resourceProp.Activate(null);
							ScenarioManager.Tiles[cObjectResource.ArrayIndex.X, cObjectResource.ArrayIndex.Y].m_Props.Remove(resourceProp);
						}
					}
				}
			}
			switch (BaseCard.CardType)
			{
			case CBaseCard.ECardType.Item:
			{
				bool flag = true;
				foreach (CActiveBonus item5 in BaseCard.ActiveBonuses.ToList())
				{
					if (!item5.Finishing() && !item5.Finished())
					{
						flag = false;
					}
				}
				if (!flag)
				{
					break;
				}
				if (Actor.Class is CCharacterClass)
				{
					CItem item = Actor.Inventory.AllItems.Find((CItem s) => s.ID == BaseCard.ID);
					Actor.Inventory.HandleUsedItem(item, this);
				}
				else if (Actor.Class is CHeroSummonClass)
				{
					CItem item2 = (Actor as CHeroSummonActor).Summoner.Inventory.AllItems.Find((CItem s) => s.ID == BaseCard.ID);
					(Actor as CHeroSummonActor).Summoner.Inventory.HandleUsedItem(item2);
				}
				else if (Actor.Class is CMonsterClass cMonsterClass2)
				{
					CItem item3 = Caster.Inventory.AllItems.Find((CItem s) => s.ID == BaseCard.ID);
					Caster.Inventory.HandleUsedItem(item3, this);
					cMonsterClass2.ActivatedCards.Remove(BaseCard);
				}
				else
				{
					DLLDebug.LogError("Trying to deactivate an item card active bonus on an unexpectedpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartialpartial class type");
				}
				break;
			}
			case CBaseCard.ECardType.AttackModifier:
				if (Actor.Class is CCharacterClass cCharacterClass && cCharacterClass.ActivatedCards.Contains(BaseCard))
				{
					cCharacterClass.ActivatedCards.Remove(BaseCard);
				}
				else if (Caster.Class is CCharacterClass cCharacterClass2 && cCharacterClass2.ActivatedCards.Contains(BaseCard))
				{
					cCharacterClass2.ActivatedCards.Remove(BaseCard);
				}
				else if (Actor.Class is CHeroSummonClass)
				{
					CCharacterClass cCharacterClass3 = (Actor as CHeroSummonActor).Summoner.Class as CCharacterClass;
					if (cCharacterClass3.ActivatedCards.Contains(BaseCard))
					{
						cCharacterClass3.ActivatedCards.Remove(BaseCard);
					}
				}
				else if (Actor.Class is CMonsterClass cMonsterClass && cMonsterClass.ActivatedCards.Contains(BaseCard))
				{
					cMonsterClass.ActivatedCards.Remove(BaseCard);
				}
				break;
			case CBaseCard.ECardType.ScenarioModifier:
				BaseCard.ActiveBonuses.Remove(this);
				break;
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception while trying to finish Active Bonus. Ability Name: " + m_Ability.Name + " BaseCard Name: " + m_BaseCard.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void UndoFinish()
	{
		m_Finishing = false;
		if (m_BespokeBehaviour != null)
		{
			m_BespokeBehaviour.AddListeners();
		}
	}

	public static List<CActiveBonus> FindAllActiveBonuses()
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		list.AddRange(MonsterClassManager.FindAllActiveBonuses());
		list.AddRange(CharacterClassManager.FindAllActiveBonuses());
		list.AddRange(ScenarioManager.Scenario.HeroSummons.SelectMany((CHeroSummonActor x) => x.BaseCard.ActiveBonuses));
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			list.AddRange(scenarioModifier.ActiveBonuses);
		}
		list.Distinct();
		return list;
	}

	public static List<CActiveBonus> FindApplicableActiveBonuses(CActor actor, CAbility.EAbilityType type, EActiveBonusBehaviourType behaviourType = EActiveBonusBehaviourType.None)
	{
		List<CActiveBonus> activeBonuses = null;
		if (actor.IsMonsterType)
		{
			activeBonuses = MonsterClassManager.FindAllActiveBonuses(type, actor);
			activeBonuses.AddRange(from b in CharacterClassManager.FindAllActiveBonuses(type, actor)
				where !activeBonuses.Contains(b)
				select b);
		}
		else if (!actor.IsMonsterType)
		{
			activeBonuses = CharacterClassManager.FindAllActiveBonuses(type, actor);
		}
		else
		{
			activeBonuses = new List<CActiveBonus>();
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CActiveBonus activeBonuse in scenarioModifier.ActiveBonuses)
			{
				if (!activeBonuses.Contains(activeBonuse) && activeBonuse.Ability.AbilityType == type && (activeBonuse.IsAura ? activeBonuse.ValidActorsInRangeOfAura.Contains(actor) : (activeBonuse.Actor == actor)))
				{
					activeBonuses.Add(activeBonuse);
				}
			}
		}
		activeBonuses.Distinct();
		for (int num = activeBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = activeBonuses[num];
			if (cActiveBonus.m_Finishing)
			{
				cActiveBonus.m_Finished = true;
				activeBonuses.Remove(cActiveBonus);
			}
			if (behaviourType != EActiveBonusBehaviourType.None && cActiveBonus.Ability.ActiveBonusData.Behaviour != behaviourType)
			{
				activeBonuses.Remove(cActiveBonus);
			}
		}
		return activeBonuses;
	}

	public static List<CActiveBonus> FindApplicableActiveBonuses(CActor actor)
	{
		List<CActiveBonus> activeBonuses = null;
		if (actor.IsEnemyMonsterType || actor.Type == CActor.EType.Neutral)
		{
			activeBonuses = MonsterClassManager.FindAllActiveBonuses(actor);
			activeBonuses.AddRange(from b in CharacterClassManager.FindAllActiveBonuses(actor)
				where !activeBonuses.Contains(b)
				select b);
		}
		else if (actor.Type == CActor.EType.Ally)
		{
			activeBonuses = MonsterClassManager.FindAllActiveBonuses(actor);
			activeBonuses.AddRange(from b in CharacterClassManager.FindAllActiveBonuses(actor)
				where !activeBonuses.Contains(b)
				select b);
		}
		else if (!actor.IsMonsterType)
		{
			activeBonuses = CharacterClassManager.FindAllActiveBonuses(actor);
		}
		else
		{
			DLLDebug.LogError("Invalid actor type " + actor.Type.ToString() + ".  Unable to find active bonuses");
			activeBonuses = new List<CActiveBonus>();
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			activeBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus a) => !activeBonuses.Contains(a) && (a.Actor == actor || a.ValidActorsInRangeOfAura.Contains(actor))));
		}
		foreach (CActiveBonus item in activeBonuses.ToList())
		{
			if (item.m_Finishing)
			{
				item.m_Finished = true;
				activeBonuses.Remove(item);
			}
		}
		return activeBonuses;
	}

	public static List<CActiveBonus> FindAllApplicableSongActiveBonuses(CActor actor)
	{
		return CharacterClassManager.FindAllSongActiveBonuses(actor);
	}

	public static void RefreshAllSongActiveBonuses(CActor actor)
	{
		foreach (CActiveBonus item in CharacterClassManager.FindAllSongActiveBonuses(actor))
		{
			if (item.BespokeBehaviour != null)
			{
				item.BespokeBehaviour.RefreshListeners(actor);
			}
		}
	}

	public static void RefreshAllAuraActiveBonuses()
	{
		List<CActiveBonus> auraActiveBonuses = new List<CActiveBonus>();
		auraActiveBonuses.AddRange(from a in MonsterClassManager.FindAllActiveBonuses()
			where a.IsAura
			select a);
		auraActiveBonuses.AddRange(from a in CharacterClassManager.FindAllActiveBonuses()
			where a.IsAura
			select a);
		auraActiveBonuses.AddRange(ScenarioManager.Scenario.HeroSummons.SelectMany((CHeroSummonActor x) => x.BaseCard.ActiveBonuses.Where((CActiveBonus a) => a.IsAura && !auraActiveBonuses.Contains(a))));
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			auraActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus a) => a.IsAura));
		}
		foreach (CActiveBonus item in auraActiveBonuses)
		{
			item.FindValidActorsInRangeOfAura();
		}
	}

	public static void RefreshOverhealActiveBonuses()
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		list.AddRange(from a in MonsterClassManager.FindAllActiveBonuses()
			where a.Ability.AbilityType == CAbility.EAbilityType.Overheal
			select a);
		list.AddRange(from a in CharacterClassManager.FindAllActiveBonuses()
			where a.Ability.AbilityType == CAbility.EAbilityType.Overheal
			select a);
		list.AddRange(ScenarioManager.Scenario.HeroSummons.SelectMany((CHeroSummonActor x) => x.BaseCard.ActiveBonuses.Where((CActiveBonus a) => a.Ability.AbilityType == CAbility.EAbilityType.Overheal)));
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			list.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus a) => a.Ability.AbilityType == CAbility.EAbilityType.Overheal));
		}
		foreach (CActiveBonus item in list)
		{
			(item as COverhealActiveBonus)?.CheckForOverhealStrengthUpdate();
		}
	}

	public virtual void FindValidActorsInRangeOfAura()
	{
		if (m_Finishing || m_Finished)
		{
			return;
		}
		m_ValidActorsInRangeOfAura.Clear();
		if (BespokeBehaviour != null)
		{
			BespokeBehaviour.RemoveListeners();
		}
		switch (AuraTargeting)
		{
		case CAbility.EAbilityTargeting.Range:
			m_ValidActorsInRangeOfAura = GameState.GetActorsInRange((Ability.TargetingActor != null) ? Ability.TargetingActor : Actor, (Ability.FilterActor != null) ? Ability.FilterActor : Actor, Ability.Range, null, Ability.ActiveBonusData.AuraFilter, null, null, Ability.IsTargetedAbility, null, Ability.MiscAbilityData?.CanTargetInvisible, Ability.ActiveBonusData.OriginalTargetType);
			if (BespokeBehaviour == null)
			{
				break;
			}
			{
				foreach (CActor item in m_ValidActorsInRangeOfAura)
				{
					BespokeBehaviour.RefreshListeners(item, Ability.ActiveBonusData.OriginalTargetType);
				}
				break;
			}
		case CAbility.EAbilityTargeting.Room:
		case CAbility.EAbilityTargeting.All:
		case CAbility.EAbilityTargeting.AllConnectedRooms:
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (!AuraFilter.IsValidTarget(playerActor, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag = false;
				if (AuraTargeting == CAbility.EAbilityTargeting.All)
				{
					flag = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(playerActor, Actor))
				{
					flag = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(playerActor.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath);
					if (foundPath)
					{
						flag = true;
					}
				}
				if (flag)
				{
					m_ValidActorsInRangeOfAura.Add(playerActor);
					if (BespokeBehaviour != null)
					{
						BespokeBehaviour.RefreshListeners(playerActor);
					}
				}
			}
			foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
			{
				if (!AuraFilter.IsValidTarget(heroSummon, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag2 = false;
				if (AuraTargeting == CAbility.EAbilityTargeting.All)
				{
					flag2 = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(heroSummon, Actor))
				{
					flag2 = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(heroSummon.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath2);
					if (foundPath2)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					m_ValidActorsInRangeOfAura.Add(heroSummon);
					if (BespokeBehaviour != null)
					{
						BespokeBehaviour.RefreshListeners(heroSummon);
					}
				}
			}
			foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
			{
				if (!AuraFilter.IsValidTarget(allAliveMonster, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
				{
					continue;
				}
				bool flag3 = false;
				if (AuraTargeting == CAbility.EAbilityTargeting.All)
				{
					flag3 = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(allAliveMonster, Actor))
				{
					flag3 = true;
				}
				else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
				{
					ScenarioManager.PathFinder.FindPath(allAliveMonster.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath3);
					if (foundPath3)
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					m_ValidActorsInRangeOfAura.Add(allAliveMonster);
					if (BespokeBehaviour != null)
					{
						BespokeBehaviour.RefreshListeners(allAliveMonster);
					}
				}
			}
			{
				foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
				{
					if (!AuraFilter.IsValidTarget(@object, Actor, isTargetedAbility: false, useTargetOriginalType: false, Ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag4 = false;
					if (AuraTargeting == CAbility.EAbilityTargeting.All)
					{
						flag4 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.Room && SharedAbilityTargeting.IsActorInSameRoom(@object, Actor))
					{
						flag4 = true;
					}
					else if (AuraTargeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(@object.ArrayIndex, Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath4);
						if (foundPath4)
						{
							flag4 = true;
						}
					}
					if (flag4)
					{
						m_ValidActorsInRangeOfAura.Add(@object);
						if (BespokeBehaviour != null)
						{
							BespokeBehaviour.RefreshListeners(@object);
						}
					}
				}
				break;
			}
		}
	}

	public static bool AreTheSame(CActiveBonus bonus1, CActiveBonus bonus2)
	{
		if (bonus1.ID == bonus2.ID && bonus1.BaseCard.ID == bonus2.BaseCard.ID && bonus1.Ability.Name == bonus2.Ability.Name && bonus1.Actor.ActorGuid == bonus2.Actor.ActorGuid && bonus1.Caster.ActorGuid == bonus2.Caster.ActorGuid)
		{
			return bonus1.Remaining == bonus2.Remaining;
		}
		return false;
	}

	public bool CheckAddAbilityValidity(CAbility addAbility)
	{
		bool result = true;
		if (addAbility is CAbilityInfuse cAbilityInfuse && cAbilityInfuse.ElementAlreadyStrongAndInfuseIfNotStrongSet())
		{
			result = false;
		}
		return result;
	}

	public bool IsValidAttackType(CAbilityAttack attackAbility)
	{
		bool result = false;
		switch (Ability.ActiveBonusData.AttackType)
		{
		case CAbility.EAttackType.Melee:
			result = attackAbility.IsMeleeAttack;
			break;
		case CAbility.EAttackType.Ranged:
			result = !attackAbility.IsMeleeAttack;
			break;
		case CAbility.EAttackType.Default:
			result = attackAbility.IsDefaultAttack;
			break;
		case CAbility.EAttackType.Control:
			result = attackAbility.IsControlAbility;
			break;
		case CAbility.EAttackType.ControlledByCaster:
			result = (attackAbility.IsControlAbility || (attackAbility.ParentAbility != null && attackAbility.ParentAbility.IsControlAbility)) && GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().ControllingActor == Caster;
			break;
		case CAbility.EAttackType.Augment:
			result = attackAbility.Augment != null || attackAbility.AugmentsAdded;
			break;
		case CAbility.EAttackType.None:
		case CAbility.EAttackType.Attack:
			result = true;
			break;
		}
		return result;
	}

	public CActiveBonus()
	{
	}

	public CActiveBonus(CActiveBonus state, ReferenceDictionary references)
	{
		ID = state.ID;
		Duration = state.Duration;
		IsAugment = state.IsAugment;
		IsSong = state.IsSong;
		IsDoom = state.IsDoom;
		ToggledBonus = state.ToggledBonus;
		ToggleLocked = state.ToggleLocked;
		ApplyToAction = state.ApplyToAction;
		ToggledElement = state.ToggledElement;
		ToggledResourcesToConsume = references.Get(state.ToggledResourcesToConsume);
		if (ToggledResourcesToConsume == null && state.ToggledResourcesToConsume != null)
		{
			ToggledResourcesToConsume = new List<CCharacterResource>();
			for (int i = 0; i < state.ToggledResourcesToConsume.Count; i++)
			{
				CCharacterResource cCharacterResource = state.ToggledResourcesToConsume[i];
				CCharacterResource cCharacterResource2 = references.Get(cCharacterResource);
				if (cCharacterResource2 == null && cCharacterResource != null)
				{
					cCharacterResource2 = new CCharacterResource(cCharacterResource, references);
					references.Add(cCharacterResource, cCharacterResource2);
				}
				ToggledResourcesToConsume.Add(cCharacterResource2);
			}
			references.Add(state.ToggledResourcesToConsume, ToggledResourcesToConsume);
		}
		m_Finishing = state.m_Finishing;
		m_Finished = state.m_Finished;
		m_TrackerIndex = state.m_TrackerIndex;
		m_ValidActorsInRangeOfAura = references.Get(state.m_ValidActorsInRangeOfAura);
		if (m_ValidActorsInRangeOfAura == null && state.m_ValidActorsInRangeOfAura != null)
		{
			m_ValidActorsInRangeOfAura = new List<CActor>();
			for (int j = 0; j < state.m_ValidActorsInRangeOfAura.Count; j++)
			{
				CActor cActor = state.m_ValidActorsInRangeOfAura[j];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_ValidActorsInRangeOfAura.Add(cActor2);
			}
			references.Add(state.m_ValidActorsInRangeOfAura, m_ValidActorsInRangeOfAura);
		}
		m_HasTracker = state.m_HasTracker;
		m_RestrictedActors = references.Get(state.m_RestrictedActors);
		if (m_RestrictedActors == null && state.m_RestrictedActors != null)
		{
			m_RestrictedActors = new List<CActor>();
			for (int k = 0; k < state.m_RestrictedActors.Count; k++)
			{
				CActor cActor4 = state.m_RestrictedActors[k];
				CActor cActor5 = references.Get(cActor4);
				if (cActor5 == null && cActor4 != null)
				{
					CActor cActor3 = ((cActor4 is CObjectActor state6) ? new CObjectActor(state6, references) : ((cActor4 is CEnemyActor state7) ? new CEnemyActor(state7, references) : ((cActor4 is CHeroSummonActor state8) ? new CHeroSummonActor(state8, references) : ((!(cActor4 is CPlayerActor state9)) ? new CActor(cActor4, references) : new CPlayerActor(state9, references)))));
					cActor5 = cActor3;
					references.Add(cActor4, cActor5);
				}
				m_RestrictedActors.Add(cActor5);
			}
			references.Add(state.m_RestrictedActors, m_RestrictedActors);
		}
		m_ActiveBonusStartRound = state.m_ActiveBonusStartRound;
	}
}
