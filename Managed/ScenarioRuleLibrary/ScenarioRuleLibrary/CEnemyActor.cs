using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CEnemyActor : CActor
{
	private string m_SummonerGuid;

	public bool OnDeathAbilityActorDeadHandled;

	public int StandeeID { get; set; }

	public CMonsterClass MonsterClass => (CMonsterClass)m_Class;

	public override int Level => MonsterClass.StatLevel;

	public bool IsSummon { get; private set; }

	public int MonsterGroupType { get; set; }

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
			return MonsterClass.Flying;
		}
	}

	public override bool Invulnerable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count <= 0)
			{
				return MonsterClass.Invulnerable;
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
				return MonsterClass.PierceInvulnerability;
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
				return MonsterClass.Untargetable;
			}
			return true;
		}
	}

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
			foreach (CAbility.EAbilityType immunity in MonsterClass.Immunities)
			{
				if (!list.Contains(immunity))
				{
					list.Add(immunity);
				}
			}
			return list;
		}
	}

	public CEnemyActor Summoner => ScenarioManager.Scenario.AllEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == m_SummonerGuid);

	public int AttackStrengthForUI { get; private set; }

	public CActor TargetingActor { get; private set; }

	public CAttackSummary.TargetSummary AttackSummary { get; private set; }

	public CEnemyActor()
	{
	}

	public CEnemyActor(CEnemyActor state, ReferenceDictionary references)
		: base(state, references)
	{
		StandeeID = state.StandeeID;
		IsSummon = state.IsSummon;
		MonsterGroupType = state.MonsterGroupType;
		m_SummonerGuid = state.m_SummonerGuid;
		AttackStrengthForUI = state.AttackStrengthForUI;
		OnDeathAbilityActorDeadHandled = state.OnDeathAbilityActorDeadHandled;
	}

	public CEnemyActor(Point startPosition, CMonsterClass monsterClass, EType type, bool isSummon, string summonerGuid, string actorGuid, int currentHealth, int maxHealth, int level, int standeeID, bool initialMapLoad, int chosenModelIndex)
		: base(type, monsterClass, startPosition, currentHealth, maxHealth, level, actorGuid, chosenModelIndex)
	{
		IsSummon = isSummon;
		m_SummonerGuid = summonerGuid;
		StandeeID = standeeID;
		if (monsterClass.Advantage)
		{
			base.Tokens.AddPositiveToken(CCondition.EPositiveCondition.Advantage, int.MaxValue, EConditionDecTrigger.Never, this);
		}
		if ((monsterClass.NonEliteVariant != null) ? monsterClass.NonEliteVariant.InitialState : monsterClass.InitialState)
		{
			monsterClass.InitialState = false;
			monsterClass.ResetAbilityCards();
		}
		CalculateAttackStrengthForUI();
		DLLDebug.Log("Created a new CEnemyActor with ActorID: " + base.ID);
	}

	public override CClass ActiveBonusClass()
	{
		if (MonsterClass.NonEliteVariant == null)
		{
			return MonsterClass;
		}
		return MonsterClass.NonEliteVariant;
	}

	public override int BaseShield()
	{
		MonsterClass.SetBaseStatIsBasedOnXValueUsingCurrent(onlyRefreshRequiredTypes: true);
		return MonsterClass.Shield;
	}

	public override int BaseRetaliate(int distanceToEnemy)
	{
		MonsterClass.SetBaseStatIsBasedOnXValueUsingCurrent(onlyRefreshRequiredTypes: true);
		if (distanceToEnemy > MonsterClass.RetaliateRange)
		{
			return 0;
		}
		return MonsterClass.Retaliate;
	}

	public override string ActorLocKey()
	{
		return MonsterClass.LocKey;
	}

	public new CEnemyActor Clone()
	{
		CEnemyActor cEnemyActor = (CEnemyActor)MemberwiseClone();
		cEnemyActor.Inventory.InventoryActor = cEnemyActor;
		return cEnemyActor;
	}

	public override EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage)
	{
		if (MonsterClass.Advantage)
		{
			addAdvantage = true;
		}
		return base.Tokens.GetAdvantageStatus(addAdvantage, addDisadvantage);
	}

	public override int Initiative()
	{
		int result = 0;
		bool flag = false;
		if (MonsterClass.MonsterClassIDToActImmediatelyBefore != null)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonsters.FirstOrDefault((CEnemyActor x) => x.MonsterClass.ID == MonsterClass.MonsterClassIDToActImmediatelyBefore);
			if (cEnemyActor?.MonsterClass?.RoundAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				result = cEnemyActor.MonsterClass.RoundAbilityCard.Initiative;
				flag = true;
			}
		}
		if (!flag && MonsterClass.RoundAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			result = MonsterClass.RoundAbilityCard.Initiative;
			flag = true;
		}
		return result;
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		MonsterClass.RoundAbilityCard.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(MonsterClass.RoundAbilityCard.Action.Infusions, MonsterClass.RoundAbilityCard.Action.Augmentations, 0), MonsterClass.RoundAbilityCard);
		foreach (CAbility ability in MonsterClass.RoundAbilityCard.Action.Abilities)
		{
			CAbility item = CloneAbilityAndApplyBaseStats(ability);
			GameState.CurrentAction.Action.Abilities.Add(item);
		}
		CAbility cAbility = GameState.CurrentAction.Action.Abilities.Find((CAbility x) => x.AbilityType == CAbility.EAbilityType.Move);
		CAbility cAbility2 = GameState.CurrentAction.Action.Abilities.Find((CAbility x) => x.AbilityType == CAbility.EAbilityType.Attack);
		if (cAbility != null && cAbility2 != null)
		{
			cAbility.Range = cAbility2.Range;
		}
	}

	public void AbilitySelectionAddConsume(CActionAugmentation consume)
	{
		foreach (CActionAugmentationOp consumeOp in consume.AugmentationOps)
		{
			if (consumeOp.ParentAbilityName == string.Empty)
			{
				if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					CAbility item = CloneAbilityAndApplyBaseStats(consumeOp.Ability);
					GameState.CurrentAction.Action.Abilities.Add(item);
				}
				else
				{
					DLLDebug.LogError("No parent specified for ability override");
				}
			}
			else if (MonsterClass.RoundAbilityCard.Action.Abilities.SingleOrDefault((CAbility x) => x.Name == consumeOp.ParentAbilityName) != null)
			{
				if (consumeOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
				{
					CCondition.ENegativeCondition num = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
					CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == consumeOp.Ability.AbilityType.ToString());
					if (num == CCondition.ENegativeCondition.NA && ePositiveCondition == CCondition.EPositiveCondition.NA)
					{
						CAbility item2 = CloneAbilityAndApplyBaseStats(consumeOp.Ability);
						GameState.CurrentAction.Action.Abilities.Add(item2);
					}
				}
			}
			else
			{
				DLLDebug.LogError("Unable to find matching parent ability for consume.");
			}
		}
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, base.Type == EType.HeroSummon || base.Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		base.CauseOfDeath = causeOfDeath;
		AttackSummary = attackSummary;
		m_CauseOfDeathAbility = causeOfDeathAbility;
		if (!m_OnDeathAbilityUsed && base.CauseOfDeath != ECauseOfDeath.ActorRemovedFromMap && PhaseManager.Phase is CPhaseAction cPhaseAction && cPhaseAction != null && MonsterClass.RoundAbilityCard != null)
		{
			List<CAbility> list = new List<CAbility>();
			bool flag = false;
			if (base.ActorActionHasHappened)
			{
				foreach (CAbility item in MonsterClass.RoundAbilityCard.Action.Abilities.Where((CAbility x) => x.OnDeath))
				{
					CAbility cAbility = CloneAbilityAndApplyBaseStats(item);
					cAbility.ProcessIfDead = true;
					list.Add(cAbility);
					flag = true;
				}
			}
			if (MonsterClass.CurrentMonsterStat.OnDeathAbilities != null && MonsterClass.CurrentMonsterStat.OnDeathAbilities.Count > 0)
			{
				foreach (CAbility onDeathAbility in MonsterClass.CurrentMonsterStat.OnDeathAbilities)
				{
					CAbility cAbility2 = CAbility.CopyAbility(onDeathAbility, generateNewID: false);
					cAbility2.ProcessIfDead = true;
					cAbility2.OnDeath = true;
					list.Add(cAbility2);
				}
			}
			if (list.Count > 0)
			{
				if (!flag)
				{
					CActorDead_MessageData message = new CActorDead_MessageData(this)
					{
						m_Actor = this,
						m_OnDeathAbility = true
					};
					ScenarioRuleClient.MessageHandler(message);
					OnDeathAbilityActorDeadHandled = true;
				}
				TargetingActor = targetingActor;
				m_OnDeathAbilityUsed = true;
				GameState.OverrideCurrentActorForOneAction(this, null, killActorAfterAction: true);
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				startedOnDeathAbility = true;
				return false;
			}
		}
		base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility, AttackSummary);
		if (startedOnDeathAbility)
		{
			TargetingActor = targetingActor;
			return false;
		}
		(base.Class as CMonsterClass).RecycleID(base.ID);
		return true;
	}

	public void OnDeathAbilityComplete()
	{
		if (base.CauseOfDeath == ECauseOfDeath.None)
		{
			DLLDebug.LogError("Cause of death was not set before calling OnDeathAbilityComplete()");
			base.CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		base.OnDeath(TargetingActor, base.CauseOfDeath, out var _, fromDeathAbilityComplete: true, m_CauseOfDeathAbility, AttackSummary);
		(base.Class as CMonsterClass).RecycleID(base.ID);
		GameState.KillActorInternal(TargetingActor, this, m_OnDeathAbilityUsed);
		m_OnDeathAbilityUsed = false;
		m_CauseOfDeathAbility = null;
	}

	public static List<Point> FindEnemyPath(EType actorType, Point startLocation, Point endLocation, bool jump, bool ignoreMoveCost, out bool foundPath, bool logFailure = false)
	{
		if (!jump)
		{
			List<Point> list = new List<Point>();
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				if (allAliveActor.BlocksPathing && !CActor.AreActorsAllied(actorType, allAliveActor.Type))
				{
					list.Add(allAliveActor.ArrayIndex);
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
		return ScenarioManager.PathFinder.FindPath(startLocation, endLocation, jump, ignoreMoveCost, out foundPath, ignoreBridges: false, ignoreDifficultTerrain: false, logFailure);
	}

	public CAbility CloneAbilityAndApplyBaseStats(CAbility ability)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
		AbilityData.ActiveBonusData activeBonusData = ability.ActiveBonusData;
		if (activeBonusData != null)
		{
			_ = activeBonusData.Duration;
			if (true && ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				return cAbility;
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		int num = 0;
		num = ((!ability.AddAttackBaseStat) ? ((ability.AbilityType == CAbility.EAbilityType.Move) ? MonsterClass.Move : ((ability.AbilityType == CAbility.EAbilityType.Attack) ? MonsterClass.Attack : 0)) : MonsterClass.Attack);
		if (cAbility is CAbilityAttack && MonsterClass.AttackStatIsBasedOnXEntries != null)
		{
			cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			cAbility.SetStatBasedOnX(this, MonsterClass.AttackStatIsBasedOnXEntries, cAbility.AbilityFilter);
			flag |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
			flag2 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.AddTo);
			flag3 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Range);
			flag4 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Range && x.AddTo);
			flag5 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets);
			flag6 |= MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets && x.AddTo);
		}
		else if (cAbility is CAbilityMove && MonsterClass.MoveStatIsBasedOnXEntries != null)
		{
			cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			cAbility.SetStatBasedOnX(this, MonsterClass.MoveStatIsBasedOnXEntries, cAbility.AbilityFilter);
			flag |= MonsterClass.MoveStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
		}
		if (ability.StrengthIsBase || !flag || flag2)
		{
			cAbility.Strength = (ability.StrengthIsBase ? ability.Strength : (num + ability.Strength));
		}
		if (ability.RangeIsBase || !flag3 || flag4)
		{
			cAbility.Range = ((!ability.RangeIsBase) ? Math.Max(0, MonsterClass.Range + ability.Range) : ((ability.AreaEffect != null && ability.AreaEffect.Melee) ? 1 : ability.Range));
		}
		if (ability.TargetIsBase || !flag5 || flag6)
		{
			cAbility.NumberTargets = (ability.TargetIsBase ? ability.NumberTargets : MonsterClass.Target);
		}
		if (!ability.RangeIsBase || ability.Range != 1)
		{
			AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.TreatAsMelee != true)
			{
				goto IL_03a8;
			}
		}
		if (cAbility.MiscAbilityData == null)
		{
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
		}
		cAbility.MiscAbilityData.TreatAsMelee = true;
		goto IL_03a8;
		IL_03a8:
		if (cAbility is CAbilityAttack cAbilityAttack)
		{
			cAbilityAttack.Pierce += MonsterClass.Pierce;
		}
		if (ability.AbilityType == CAbility.EAbilityType.Move)
		{
			((CAbilityMove)cAbility).Fly = MonsterClass.Flying;
		}
		if (ability.AbilityType == CAbility.EAbilityType.Attack)
		{
			foreach (CCondition.ENegativeCondition condition in MonsterClass.Conditions)
			{
				if (condition == CCondition.ENegativeCondition.NA)
				{
					continue;
				}
				if (condition == CCondition.ENegativeCondition.Curse && cAbility.NegativeConditions.ContainsKey(condition))
				{
					if (cAbility.NegativeConditions[condition].Strength == 0)
					{
						cAbility.NegativeConditions[condition].Strength++;
					}
					cAbility.NegativeConditions[condition].Strength++;
					continue;
				}
				CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == condition.ToString());
				if (eAbilityType != CAbility.EAbilityType.None && !cAbility.NegativeConditions.ContainsKey(condition))
				{
					cAbility.NegativeConditions.Add(condition, CAbility.CreateAbility(eAbilityType, cAbility.AbilityFilter, isMonster: true, cAbility.IsTargetedAbility));
				}
			}
		}
		return cAbility;
	}

	public override void CalculateAttackStrengthForUI()
	{
		if (MonsterClass.AttackStatIsBasedOnXEntries != null && MonsterClass.AttackStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && (!x.AddTo || !CAbility.StatIsXTypesToIgnoreIfAddTo.Contains(x.BasedOn))))
		{
			CAbility cAbility = CAbilityAttack.CreateDefaultAttack(MonsterClass.Attack, 1, 1, isMonster: false);
			cAbility.Start(this, this);
			cAbility.AbilityFilter.IsValidTarget(this, this, cAbility.IsTargetedAbility, useTargetOriginalType: false, false);
			AttackStrengthForUI = cAbility.Strength;
		}
		else
		{
			AttackStrengthForUI = MonsterClass.Attack;
		}
	}

	public void LoadEnemy(EnemyState enemyState)
	{
		if (enemyState.ID != 0)
		{
			m_ID = enemyState.ID;
		}
		m_ArrayIndex = new Point(enemyState.Location.X, enemyState.Location.Y);
		m_Health = enemyState.Health;
		base.MaxHealth = enemyState.MaxHealth;
		m_Level = enemyState.Level;
		m_Tokens = new CTokens(this, enemyState.PositiveConditions, enemyState.NegativeConditions);
		m_PlayedThisRound = enemyState.PlayedThisRound;
		base.CauseOfDeath = enemyState.CauseOfDeath;
		base.KilledByActorGuid = enemyState.KilledByActorGuid;
		base.Type = enemyState.Type;
		base.PhasedOut = enemyState.PhasedOut;
		base.Deactivated = enemyState.Deactivated;
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
		IsSummon = enemyState.IsSummon;
		m_SummonerGuid = enemyState.SummonerGuid;
		m_CharacterResources = new List<CCharacterResource>();
		foreach (KeyValuePair<string, int> characterResource in enemyState.CharacterResources)
		{
			m_CharacterResources.Add(new CCharacterResource(characterResource.Key, characterResource.Value));
		}
	}
}
