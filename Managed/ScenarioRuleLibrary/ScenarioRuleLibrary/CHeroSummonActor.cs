using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CHeroSummonActor : CActor
{
	private string m_SummonerGuid;

	private CPlayerActor m_SummonerCached;

	public int StandeeID { get; set; }

	public CHeroSummonClass HeroSummonClass => (CHeroSummonClass)m_Class;

	public HeroSummonYMLData SummonData { get; private set; }

	public CBaseAbilityCard BaseCard { get; private set; }

	public CActor TargetingActor { get; private set; }

	public int SummonedOrderIndex { get; set; }

	public bool? ReturnToSummoner { get; set; }

	public override bool Flying
	{
		get
		{
			if (base.Tokens.HasKey(CCondition.ENegativeCondition.StopFlying))
			{
				return false;
			}
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Move))
			{
				if (item.BespokeBehaviour != null && item.BespokeBehaviour is CMoveActiveBonus_BuffMove cMoveActiveBonus_BuffMove)
				{
					cMoveActiveBonus_BuffMove.CalculateBuffs(out var _, out var _, out var _, out var fly, out var _, out var _);
					if (fly.HasValue && fly.Value)
					{
						return true;
					}
				}
			}
			return SummonData.Flying;
		}
	}

	public override bool Invulnerable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count <= 0)
			{
				return SummonData.Invulnerable;
			}
			return true;
		}
	}

	public override bool PierceInvulnerability
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.PierceInvulnerability).Count <= 0)
			{
				return SummonData.PierceInvulnerability;
			}
			return true;
		}
	}

	public override bool Untargetable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Untargetable).Count <= 0)
			{
				return SummonData.Untargetable;
			}
			return true;
		}
	}

	public override bool DoesNotBlock => SummonData.DoesNotBlock;

	public override bool IgnoreActorCollision => SummonData.IgnoreActorCollision;

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (!(item.Ability is CAbilityImmunityTo cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (!list.Contains(immuneToAbilityType))
					{
						list.Add(immuneToAbilityType);
					}
				}
			}
			return list;
		}
	}

	public CPlayerActor Summoner
	{
		get
		{
			if (m_SummonerCached != null)
			{
				return m_SummonerCached;
			}
			m_SummonerCached = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == m_SummonerGuid);
			return m_SummonerCached;
		}
	}

	public bool IsCompanionSummon
	{
		get
		{
			if (Summoner.CharacterClass.CompanionSummonData == null)
			{
				return false;
			}
			return SummonData == Summoner.CharacterClass.CompanionSummonData;
		}
	}

	public int AttackStrength
	{
		get
		{
			if (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength))
			{
				CAbility cAbility = CAbilityAttack.CreateDefaultAttack(0, 1, 1, isMonster: false);
				cAbility.Start(this, this);
				cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
				cAbility.SetStatBasedOnX(this, SummonData.AttackStatIsBasedOnXEntries, cAbility.AbilityFilter);
				return cAbility.Strength;
			}
			return SummonData.Attack;
		}
	}

	public CHeroSummonActor()
	{
	}

	public CHeroSummonActor(CHeroSummonActor state, ReferenceDictionary references)
		: base(state, references)
	{
		StandeeID = state.StandeeID;
		SummonedOrderIndex = state.SummonedOrderIndex;
		ReturnToSummoner = state.ReturnToSummoner;
		m_SummonerGuid = state.m_SummonerGuid;
	}

	public CHeroSummonActor(Point startPosition, string summonerGuid, int summonerLevel, int standeeID, CHeroSummonClass heroSummonClass, HeroSummonYMLData summonData, string actorGuid, int chosenModelIndex)
		: base(EType.HeroSummon, heroSummonClass, startPosition, summonData.GetHealth(summonerLevel), summonData.GetHealth(summonerLevel), summonerLevel, actorGuid, chosenModelIndex)
	{
		StandeeID = standeeID;
		m_SummonerGuid = summonerGuid;
		SummonData = summonData;
		BaseCard = new CBaseAbilityCard(0, summonData.ID.GetHashCode(), heroSummonClass.ID, CBaseCard.ECardType.HeroSummonAbility, summonData.ID);
		DLLDebug.Log("Created a new CHeroSummonActor with BaseCardID: " + BaseCard.ID);
	}

	public override string GetPrefabName()
	{
		return SummonData.Model.ToString();
	}

	public override int Initiative()
	{
		if (Summoner == null)
		{
			return 99;
		}
		return Summoner.Initiative();
	}

	public override string ActorLocKey()
	{
		return HeroSummonClass.LocKey;
	}

	public override int BaseShield()
	{
		return SummonData.Shield;
	}

	public override int BaseRetaliate(int distanceToEnemy)
	{
		if (distanceToEnemy > SummonData.RetaliateRange)
		{
			return 0;
		}
		return SummonData.Retaliate;
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		BaseCard.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(null, null, 0), BaseCard);
		if (SummonData.Move > 0 || SummonData.MoveOverride != null || (SummonData.MoveStatIsBasedOnXEntries != null && SummonData.MoveStatIsBasedOnXEntries.Count > 0))
		{
			CAbility cAbility = CAbilityMove.CreateDefaultMove(SummonData.Move, isMonster: false, SummonData.Range, jump: false, SummonData.Flying);
			if (SummonData.MoveOverride != null)
			{
				cAbility.OverrideAbilityValues(SummonData.MoveOverride, perform: false);
			}
			GameState.CurrentAction.Action.AddAbility(cAbility);
		}
		if (SummonData.Attack > 0 || SummonData.AttackOverride != null || (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Count > 0))
		{
			CAbility ability = CAbilityAttack.CreateDefaultAttack(0, 0, 1, isMonster: false, SummonData.AttackAnimOverload);
			ability = ApplyBaseStats(ability);
			if (SummonData.AttackOverride != null)
			{
				ability.OverrideAbilityValues(SummonData.AttackOverride, perform: false);
			}
			GameState.CurrentAction.Action.AddAbility(ability);
		}
		if (SummonData.SpecialAbilities == null)
		{
			return;
		}
		foreach (CAbility specialAbility in SummonData.SpecialAbilities)
		{
			GameState.CurrentAction.Action.AddAbility(CAbility.CopyAbility(specialAbility, generateNewID: false));
		}
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, base.Type == EType.HeroSummon || base.Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public CAbility ApplyBaseStats(CAbility ability, bool copyCurrentOverrides = false, bool overrideAttackZero = false)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false, fullCopy: false, copyCurrentOverrides);
		if (!ability.StrengthIsBase)
		{
			if (cAbility is CAbilityAttack cAbilityAttack)
			{
				if (SummonData.AttackStatIsBasedOnXEntries != null && SummonData.AttackStatIsBasedOnXEntries.Count > 0)
				{
					cAbilityAttack.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
					cAbilityAttack.SetStatBasedOnX(this, SummonData.AttackStatIsBasedOnXEntries, cAbility.AbilityFilter);
				}
				else
				{
					if (SummonData.Attack == 0 && !overrideAttackZero)
					{
						return null;
					}
					cAbilityAttack.Strength += SummonData.Attack;
				}
			}
			else if (cAbility is CAbilityMove cAbilityMove)
			{
				cAbilityMove.Strength += SummonData.Move;
			}
		}
		if (cAbility is CAbilityAttack cAbilityAttack2)
		{
			cAbilityAttack2.Pierce += SummonData.Pierce;
		}
		if (ability.AbilityType == CAbility.EAbilityType.Move)
		{
			((CAbilityMove)cAbility).Fly = SummonData.Flying;
		}
		if (!ability.RangeIsBase)
		{
			cAbility.Range += SummonData.Range;
		}
		return cAbility;
	}

	public void OnSummoned()
	{
		if (SummonData.OnSummonAbilities != null && SummonData.OnSummonAbilities.Count > 0 && PhaseManager.Phase is CPhaseAction cPhaseAction)
		{
			cPhaseAction?.StackNextAbilities(SummonData.OnSummonAbilities, this);
		}
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		base.CauseOfDeath = causeOfDeath;
		m_CauseOfDeathAbility = causeOfDeathAbility;
		if (!m_OnDeathAbilityUsed && base.CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap && PhaseManager.Phase is CPhaseAction cPhaseAction && cPhaseAction != null)
		{
			List<CAbility> list = new List<CAbility>();
			if (SummonData.AdjacentAttackOnDeath > 0)
			{
				CAbility cAbility = CAbilityAttack.CreateDefaultAttack(SummonData.AdjacentAttackOnDeath, 1, -1, isMonster: false, SummonData.AttackOnDeathAnimOverload, isMultipass: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.None, null, null, null, null, null, null, null, null, null, null, null, new CEqualityFilter(">", 0, valueIsPercentage: false)));
				cAbility.AbilityFilter.IsValidTarget(this, this, isTargetedAbility: true, useTargetOriginalType: false, false);
				if (cAbility.AbilityFilter.LastCheckedCasterAdjacentEnemies.Count > 0)
				{
					cAbility.ProcessIfDead = true;
					list.Add(cAbility);
				}
			}
			if (SummonData.OnDeathAbilities != null && SummonData.OnDeathAbilities.Count > 0)
			{
				foreach (CAbility onDeathAbility in SummonData.OnDeathAbilities)
				{
					CAbility cAbility2 = CAbility.CopyAbility(onDeathAbility, generateNewID: false);
					cAbility2.ProcessIfDead = true;
					list.Add(cAbility2);
				}
			}
			if (list.Count > 0)
			{
				TargetingActor = targetingActor;
				m_OnDeathAbilityUsed = true;
				GameState.OverrideCurrentActorForOneAction(this, base.OriginalType, killActorAfterAction: true);
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				startedOnDeathAbility = true;
				return false;
			}
		}
		base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility);
		if (startedOnDeathAbility)
		{
			TargetingActor = targetingActor;
			return false;
		}
		HeroSummonClass.RecycleID(base.ID);
		return true;
	}

	public bool OnDeathFromSummonerDeath(CActor targetingActor)
	{
		base.OnDeath(targetingActor, ECauseOfDeath.SummonerDied, out var _);
		HeroSummonClass.RecycleID(base.ID);
		return true;
	}

	public void OnDeathAbilityComplete()
	{
		if (base.CauseOfDeath == ECauseOfDeath.None)
		{
			DLLDebug.LogError("Cause of death was not set before calling OnDeathAbilityComplete()");
			base.CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		base.OnDeath(TargetingActor, base.CauseOfDeath, out var _, fromDeathAbilityComplete: true, m_CauseOfDeathAbility);
		HeroSummonClass.RecycleID(base.ID);
		GameState.KillActorInternal(TargetingActor, this, m_OnDeathAbilityUsed);
		m_OnDeathAbilityUsed = false;
		m_CauseOfDeathAbility = null;
	}

	public new CHeroSummonActor Clone()
	{
		CHeroSummonActor cHeroSummonActor = (CHeroSummonActor)MemberwiseClone();
		cHeroSummonActor.Inventory.InventoryActor = cHeroSummonActor;
		return cHeroSummonActor;
	}

	public void LoadHeroSummon(HeroSummonState heroSummonState)
	{
		if (heroSummonState.ID != 0)
		{
			m_ID = heroSummonState.ID;
		}
		m_ArrayIndex = new Point(heroSummonState.Location.X, heroSummonState.Location.Y);
		m_Health = heroSummonState.Health;
		m_MaxHealth = heroSummonState.HeroSummonData.GetHealth(heroSummonState.Level);
		m_Level = heroSummonState.Level;
		m_Tokens = new CTokens(this, heroSummonState.PositiveConditions, heroSummonState.NegativeConditions);
		m_PlayedThisRound = heroSummonState.PlayedThisRound;
		base.CauseOfDeath = heroSummonState.CauseOfDeath;
		base.KilledByActorGuid = heroSummonState.KilledByActorGuid;
		base.PhasedOut = heroSummonState.PhasedOut;
		base.Deactivated = heroSummonState.Deactivated;
		if (base.PhasedOut)
		{
			CActorIsTeleporting_MessageData message = new CActorIsTeleporting_MessageData(this)
			{
				m_ActorTeleporting = this,
				m_TeleportAbility = null
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (m_Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			CSleep_MessageData message2 = new CSleep_MessageData("", null)
			{
				m_ActorToSleep = this
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		m_SummonerGuid = heroSummonState.Summoner;
		m_SummonerCached = null;
		SummonData = heroSummonState.HeroSummonData;
		SummonedOrderIndex = heroSummonState.SummonedOrderIndex;
		if (heroSummonState.OnSummonActiveBonuses != null)
		{
			foreach (ActiveBonusState activeBonusState in heroSummonState.OnSummonActiveBonuses)
			{
				CActor cActor = ScenarioManager.FindActor(activeBonusState.ActorGuid);
				CActor caster = ScenarioManager.FindActor(activeBonusState.CasterGuid);
				if (cActor == null)
				{
					DLLDebug.LogError("Unable to find actor attached to active bonus");
					continue;
				}
				CAbility cAbility = SummonData.OnSummonAbilities?.SingleOrDefault((CAbility x) => x.Name == activeBonusState.AbilityName);
				if (cAbility == null)
				{
					DLLDebug.LogError("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
				}
				else
				{
					BaseCard.AddActiveBonus(CAbility.CopyAbility(cAbility, generateNewID: false), cActor, caster, activeBonusState.ID, activeBonusState.Remaining, isAugment: false, isSong: false, loadingItemBonus: false, activeBonusState.IsDoom, activeBonusState.BespokeBehaviourStrength, activeBonusState.ActiveBonusStartRound);
				}
			}
		}
		m_CharacterResources = new List<CCharacterResource>();
		foreach (KeyValuePair<string, int> characterResource in heroSummonState.CharacterResources)
		{
			m_CharacterResources.Add(new CCharacterResource(characterResource.Key, characterResource.Value));
		}
	}
}
