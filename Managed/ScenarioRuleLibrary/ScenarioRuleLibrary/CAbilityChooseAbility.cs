using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityChooseAbility : CAbilityTargeting
{
	public CAbility chosenAbility;

	public List<CAbility> ChooseAbilities { get; private set; }

	public List<CAbility> ApplicableAbilities { get; private set; }

	public CBaseCard ParentBaseCard { get; set; }

	public bool HasChosenAbility { get; set; }

	public CAbilityChooseAbility(List<CAbility> chooseAbilities, CBaseCard parentBaseCard = null)
		: base(EAbilityType.Choose)
	{
		ChooseAbilities = chooseAbilities;
		ParentBaseCard = parentBaseCard;
		HasChosenAbility = false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(targetActor);
			m_State = TargetingState.PreProcessingBeforeActorApplies;
		}
	}

	public override bool PreProcessBeforeActorApplies()
	{
		FilterConditions(m_ActorsToTarget[0]);
		CShowSelectAbility_MessageData message = new CShowSelectAbility_MessageData(base.TargetingActor)
		{
			m_ChooseAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		base.PreProcessBeforeActorApplies();
		return false;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		if (base.MiscAbilityData.HasCondition == true)
		{
			CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(base.AnimOverload, actorApplying)
			{
				m_Ability = this,
				m_ActorsAppliedTo = actorsAppliedTo
			};
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (chosenAbility != null)
		{
			base.AbilityHasHappened = true;
			DoAbility(actor, toggledOn: true);
			ResetChosenAbility();
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		return true;
	}

	protected void FilterConditions(CActor actor)
	{
		ApplicableAbilities = new List<CAbility>();
		foreach (CAbility chooseAbility in ChooseAbilities)
		{
			bool flag = false;
			if (base.MiscAbilityData.HasCondition != true)
			{
				flag = true;
			}
			else
			{
				foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in chooseAbility.NegativeConditions)
				{
					if (actor.Tokens.HasKey(negativeCondition.Key))
					{
						flag = true;
					}
					if (negativeCondition.Key == CCondition.ENegativeCondition.Curse && actor.Class.GetCurseCards().Count > 0)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				ApplicableAbilities.Add(chooseAbility);
			}
		}
	}

	public void DoAbility(CActor currentActor, bool toggledOn)
	{
		if (toggledOn)
		{
			if (base.MiscAbilityData.HasCondition == true)
			{
				CAbilityRemoveConditions.RemoveConditions(base.TargetingActor, currentActor, chosenAbility.NegativeConditions.Keys.ToList(), chosenAbility.PositiveConditions.Keys.ToList(), this, base.AnimOverload);
				return;
			}
			List<CAbility> inlineSubAbilities = new List<CAbility> { CAbility.CopyAbility(chosenAbility, generateNewID: false) };
			(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, currentActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: true, null, ignorePerformNow: true);
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(chosenAbility);
		}
	}

	public void AbilityChosen(int abilityIndex)
	{
		if (abilityIndex < ApplicableAbilities.Count)
		{
			chosenAbility = ApplicableAbilities[abilityIndex];
		}
	}

	public void ResetChosenAbility()
	{
		chosenAbility = null;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(EAbilityType.Choose, subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityChooseAbility()
	{
	}

	public CAbilityChooseAbility(CAbilityChooseAbility state, ReferenceDictionary references)
		: base(state, references)
	{
		ChooseAbilities = references.Get(state.ChooseAbilities);
		if (ChooseAbilities == null && state.ChooseAbilities != null)
		{
			ChooseAbilities = new List<CAbility>();
			for (int i = 0; i < state.ChooseAbilities.Count; i++)
			{
				CAbility cAbility = state.ChooseAbilities[i];
				CAbility cAbility2 = references.Get(cAbility);
				if (cAbility2 == null && cAbility != null)
				{
					CAbility cAbility3 = ((cAbility is CAbilityBlockHealing state2) ? new CAbilityBlockHealing(state2, references) : ((cAbility is CAbilityNeutralizeShield state3) ? new CAbilityNeutralizeShield(state3, references) : ((cAbility is CAbilityAdvantage state4) ? new CAbilityAdvantage(state4, references) : ((cAbility is CAbilityBless state5) ? new CAbilityBless(state5, references) : ((cAbility is CAbilityCurse state6) ? new CAbilityCurse(state6, references) : ((cAbility is CAbilityDisarm state7) ? new CAbilityDisarm(state7, references) : ((cAbility is CAbilityImmobilize state8) ? new CAbilityImmobilize(state8, references) : ((cAbility is CAbilityImmovable state9) ? new CAbilityImmovable(state9, references) : ((cAbility is CAbilityInvisible state10) ? new CAbilityInvisible(state10, references) : ((cAbility is CAbilityMuddle state11) ? new CAbilityMuddle(state11, references) : ((cAbility is CAbilityOverheal state12) ? new CAbilityOverheal(state12, references) : ((cAbility is CAbilityPoison state13) ? new CAbilityPoison(state13, references) : ((cAbility is CAbilitySleep state14) ? new CAbilitySleep(state14, references) : ((cAbility is CAbilityStopFlying state15) ? new CAbilityStopFlying(state15, references) : ((cAbility is CAbilityStrengthen state16) ? new CAbilityStrengthen(state16, references) : ((cAbility is CAbilityStun state17) ? new CAbilityStun(state17, references) : ((cAbility is CAbilityWound state18) ? new CAbilityWound(state18, references) : ((cAbility is CAbilityChooseAbility state19) ? new CAbilityChooseAbility(state19, references) : ((cAbility is CAbilityAddActiveBonus state20) ? new CAbilityAddActiveBonus(state20, references) : ((cAbility is CAbilityAddAugment state21) ? new CAbilityAddAugment(state21, references) : ((cAbility is CAbilityAddCondition state22) ? new CAbilityAddCondition(state22, references) : ((cAbility is CAbilityAddDoom state23) ? new CAbilityAddDoom(state23, references) : ((cAbility is CAbilityAddDoomSlots state24) ? new CAbilityAddDoomSlots(state24, references) : ((cAbility is CAbilityAddHeal state25) ? new CAbilityAddHeal(state25, references) : ((cAbility is CAbilityAddRange state26) ? new CAbilityAddRange(state26, references) : ((cAbility is CAbilityAddSong state27) ? new CAbilityAddSong(state27, references) : ((cAbility is CAbilityAddTarget state28) ? new CAbilityAddTarget(state28, references) : ((cAbility is CAbilityAdjustInitiative state29) ? new CAbilityAdjustInitiative(state29, references) : ((cAbility is CAbilityAttackersGainDisadvantage state30) ? new CAbilityAttackersGainDisadvantage(state30, references) : ((cAbility is CAbilityChangeAllegiance state31) ? new CAbilityChangeAllegiance(state31, references) : ((cAbility is CAbilityChangeCharacterModel state32) ? new CAbilityChangeCharacterModel(state32, references) : ((cAbility is CAbilityChoose state33) ? new CAbilityChoose(state33, references) : ((cAbility is CAbilityConsume state34) ? new CAbilityConsume(state34, references) : ((cAbility is CAbilityConsumeItemCards state35) ? new CAbilityConsumeItemCards(state35, references) : ((cAbility is CAbilityControlActor state36) ? new CAbilityControlActor(state36, references) : ((cAbility is CAbilityDisableCardAction state37) ? new CAbilityDisableCardAction(state37, references) : ((cAbility is CAbilityDiscardCards state38) ? new CAbilityDiscardCards(state38, references) : ((cAbility is CAbilityExtraTurn state39) ? new CAbilityExtraTurn(state39, references) : ((cAbility is CAbilityForgoActionsForCompanion state40) ? new CAbilityForgoActionsForCompanion(state40, references) : ((cAbility is CAbilityGiveSupplyCard state41) ? new CAbilityGiveSupplyCard(state41, references) : ((cAbility is CAbilityHeal state42) ? new CAbilityHeal(state42, references) : ((cAbility is CAbilityHealthReduction state43) ? new CAbilityHealthReduction(state43, references) : ((cAbility is CAbilityImmunityTo state44) ? new CAbilityImmunityTo(state44, references) : ((cAbility is CAbilityImprovedShortRest state45) ? new CAbilityImprovedShortRest(state45, references) : ((cAbility is CAbilityIncreaseCardLimit state46) ? new CAbilityIncreaseCardLimit(state46, references) : ((cAbility is CAbilityInvulnerability state47) ? new CAbilityInvulnerability(state47, references) : ((cAbility is CAbilityItemLock state48) ? new CAbilityItemLock(state48, references) : ((cAbility is CAbilityKill state49) ? new CAbilityKill(state49, references) : ((cAbility is CAbilityLoseCards state50) ? new CAbilityLoseCards(state50, references) : ((cAbility is CAbilityLoseGoalChestReward state51) ? new CAbilityLoseGoalChestReward(state51, references) : ((cAbility is CAbilityNullTargeting state52) ? new CAbilityNullTargeting(state52, references) : ((cAbility is CAbilityOverrideAugmentAttackType state53) ? new CAbilityOverrideAugmentAttackType(state53, references) : ((cAbility is CAbilityPierceInvulnerability state54) ? new CAbilityPierceInvulnerability(state54, references) : ((cAbility is CAbilityPlaySong state55) ? new CAbilityPlaySong(state55, references) : ((cAbility is CAbilityPreventDamage state56) ? new CAbilityPreventDamage(state56, references) : ((cAbility is CAbilityRecoverDiscardedCards state57) ? new CAbilityRecoverDiscardedCards(state57, references) : ((cAbility is CAbilityRecoverLostCards state58) ? new CAbilityRecoverLostCards(state58, references) : ((cAbility is CAbilityRedirect state59) ? new CAbilityRedirect(state59, references) : ((cAbility is CAbilityRefreshItemCards state60) ? new CAbilityRefreshItemCards(state60, references) : ((cAbility is CAbilityRemoveActorFromMap state61) ? new CAbilityRemoveActorFromMap(state61, references) : ((cAbility is CAbilityRemoveConditions state62) ? new CAbilityRemoveConditions(state62, references) : ((cAbility is CAbilityRetaliate state63) ? new CAbilityRetaliate(state63, references) : ((cAbility is CAbilityShield state64) ? new CAbilityShield(state64, references) : ((cAbility is CAbilityShuffleModifierDeck state65) ? new CAbilityShuffleModifierDeck(state65, references) : ((cAbility is CAbilityTransferDooms state66) ? new CAbilityTransferDooms(state66, references) : ((cAbility is CAbilityUntargetable state67) ? new CAbilityUntargetable(state67, references) : ((cAbility is CAbilityCondition state68) ? new CAbilityCondition(state68, references) : ((cAbility is CAbilityMergedCreateAttack state69) ? new CAbilityMergedCreateAttack(state69, references) : ((cAbility is CAbilityMergedDestroyAttack state70) ? new CAbilityMergedDestroyAttack(state70, references) : ((cAbility is CAbilityMergedDisarmTrapDestroyObstacle state71) ? new CAbilityMergedDisarmTrapDestroyObstacle(state71, references) : ((cAbility is CAbilityMergedKillCreate state72) ? new CAbilityMergedKillCreate(state72, references) : ((cAbility is CAbilityMergedMoveAttack state73) ? new CAbilityMergedMoveAttack(state73, references) : ((cAbility is CAbilityMergedMoveObstacleAttack state74) ? new CAbilityMergedMoveObstacleAttack(state74, references) : ((cAbility is CAbilityActivateSpawner state75) ? new CAbilityActivateSpawner(state75, references) : ((cAbility is CAbilityAddModifierToMonsterDeck state76) ? new CAbilityAddModifierToMonsterDeck(state76, references) : ((cAbility is CAbilityAttack state77) ? new CAbilityAttack(state77, references) : ((cAbility is CAbilityChangeCondition state78) ? new CAbilityChangeCondition(state78, references) : ((cAbility is CAbilityChangeModifier state79) ? new CAbilityChangeModifier(state79, references) : ((cAbility is CAbilityConsumeElement state80) ? new CAbilityConsumeElement(state80, references) : ((cAbility is CAbilityCreate state81) ? new CAbilityCreate(state81, references) : ((cAbility is CAbilityDamage state82) ? new CAbilityDamage(state82, references) : ((cAbility is CAbilityDeactivateSpawner state83) ? new CAbilityDeactivateSpawner(state83, references) : ((cAbility is CAbilityDestroyObstacle state84) ? new CAbilityDestroyObstacle(state84, references) : ((cAbility is CAbilityDisarmTrap state85) ? new CAbilityDisarmTrap(state85, references) : ((cAbility is CAbilityFear state86) ? new CAbilityFear(state86, references) : ((cAbility is CAbilityInfuse state87) ? new CAbilityInfuse(state87, references) : ((cAbility is CAbilityLoot state88) ? new CAbilityLoot(state88, references) : ((cAbility is CAbilityMove state89) ? new CAbilityMove(state89, references) : ((cAbility is CAbilityMoveObstacle state90) ? new CAbilityMoveObstacle(state90, references) : ((cAbility is CAbilityMoveTrap state91) ? new CAbilityMoveTrap(state91, references) : ((cAbility is CAbilityNull state92) ? new CAbilityNull(state92, references) : ((cAbility is CAbilityNullHex state93) ? new CAbilityNullHex(state93, references) : ((cAbility is CAbilityPull state94) ? new CAbilityPull(state94, references) : ((cAbility is CAbilityPush state95) ? new CAbilityPush(state95, references) : ((cAbility is CAbilityRecoverResources state96) ? new CAbilityRecoverResources(state96, references) : ((cAbility is CAbilityRedistributeDamage state97) ? new CAbilityRedistributeDamage(state97, references) : ((cAbility is CAbilityRevive state98) ? new CAbilityRevive(state98, references) : ((cAbility is CAbilitySummon state99) ? new CAbilitySummon(state99, references) : ((cAbility is CAbilitySwap state100) ? new CAbilitySwap(state100, references) : ((cAbility is CAbilityTargeting state101) ? new CAbilityTargeting(state101, references) : ((cAbility is CAbilityTeleport state102) ? new CAbilityTeleport(state102, references) : ((cAbility is CAbilityTrap state103) ? new CAbilityTrap(state103, references) : ((cAbility is CAbilityXP state104) ? new CAbilityXP(state104, references) : ((!(cAbility is CAbilityMerged state105)) ? new CAbility(cAbility, references) : new CAbilityMerged(state105, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility2 = cAbility3;
					references.Add(cAbility, cAbility2);
				}
				ChooseAbilities.Add(cAbility2);
			}
			references.Add(state.ChooseAbilities, ChooseAbilities);
		}
		ApplicableAbilities = references.Get(state.ApplicableAbilities);
		if (ApplicableAbilities == null && state.ApplicableAbilities != null)
		{
			ApplicableAbilities = new List<CAbility>();
			for (int j = 0; j < state.ApplicableAbilities.Count; j++)
			{
				CAbility cAbility4 = state.ApplicableAbilities[j];
				CAbility cAbility5 = references.Get(cAbility4);
				if (cAbility5 == null && cAbility4 != null)
				{
					CAbility cAbility3 = ((cAbility4 is CAbilityBlockHealing state106) ? new CAbilityBlockHealing(state106, references) : ((cAbility4 is CAbilityNeutralizeShield state107) ? new CAbilityNeutralizeShield(state107, references) : ((cAbility4 is CAbilityAdvantage state108) ? new CAbilityAdvantage(state108, references) : ((cAbility4 is CAbilityBless state109) ? new CAbilityBless(state109, references) : ((cAbility4 is CAbilityCurse state110) ? new CAbilityCurse(state110, references) : ((cAbility4 is CAbilityDisarm state111) ? new CAbilityDisarm(state111, references) : ((cAbility4 is CAbilityImmobilize state112) ? new CAbilityImmobilize(state112, references) : ((cAbility4 is CAbilityImmovable state113) ? new CAbilityImmovable(state113, references) : ((cAbility4 is CAbilityInvisible state114) ? new CAbilityInvisible(state114, references) : ((cAbility4 is CAbilityMuddle state115) ? new CAbilityMuddle(state115, references) : ((cAbility4 is CAbilityOverheal state116) ? new CAbilityOverheal(state116, references) : ((cAbility4 is CAbilityPoison state117) ? new CAbilityPoison(state117, references) : ((cAbility4 is CAbilitySleep state118) ? new CAbilitySleep(state118, references) : ((cAbility4 is CAbilityStopFlying state119) ? new CAbilityStopFlying(state119, references) : ((cAbility4 is CAbilityStrengthen state120) ? new CAbilityStrengthen(state120, references) : ((cAbility4 is CAbilityStun state121) ? new CAbilityStun(state121, references) : ((cAbility4 is CAbilityWound state122) ? new CAbilityWound(state122, references) : ((cAbility4 is CAbilityChooseAbility state123) ? new CAbilityChooseAbility(state123, references) : ((cAbility4 is CAbilityAddActiveBonus state124) ? new CAbilityAddActiveBonus(state124, references) : ((cAbility4 is CAbilityAddAugment state125) ? new CAbilityAddAugment(state125, references) : ((cAbility4 is CAbilityAddCondition state126) ? new CAbilityAddCondition(state126, references) : ((cAbility4 is CAbilityAddDoom state127) ? new CAbilityAddDoom(state127, references) : ((cAbility4 is CAbilityAddDoomSlots state128) ? new CAbilityAddDoomSlots(state128, references) : ((cAbility4 is CAbilityAddHeal state129) ? new CAbilityAddHeal(state129, references) : ((cAbility4 is CAbilityAddRange state130) ? new CAbilityAddRange(state130, references) : ((cAbility4 is CAbilityAddSong state131) ? new CAbilityAddSong(state131, references) : ((cAbility4 is CAbilityAddTarget state132) ? new CAbilityAddTarget(state132, references) : ((cAbility4 is CAbilityAdjustInitiative state133) ? new CAbilityAdjustInitiative(state133, references) : ((cAbility4 is CAbilityAttackersGainDisadvantage state134) ? new CAbilityAttackersGainDisadvantage(state134, references) : ((cAbility4 is CAbilityChangeAllegiance state135) ? new CAbilityChangeAllegiance(state135, references) : ((cAbility4 is CAbilityChangeCharacterModel state136) ? new CAbilityChangeCharacterModel(state136, references) : ((cAbility4 is CAbilityChoose state137) ? new CAbilityChoose(state137, references) : ((cAbility4 is CAbilityConsume state138) ? new CAbilityConsume(state138, references) : ((cAbility4 is CAbilityConsumeItemCards state139) ? new CAbilityConsumeItemCards(state139, references) : ((cAbility4 is CAbilityControlActor state140) ? new CAbilityControlActor(state140, references) : ((cAbility4 is CAbilityDisableCardAction state141) ? new CAbilityDisableCardAction(state141, references) : ((cAbility4 is CAbilityDiscardCards state142) ? new CAbilityDiscardCards(state142, references) : ((cAbility4 is CAbilityExtraTurn state143) ? new CAbilityExtraTurn(state143, references) : ((cAbility4 is CAbilityForgoActionsForCompanion state144) ? new CAbilityForgoActionsForCompanion(state144, references) : ((cAbility4 is CAbilityGiveSupplyCard state145) ? new CAbilityGiveSupplyCard(state145, references) : ((cAbility4 is CAbilityHeal state146) ? new CAbilityHeal(state146, references) : ((cAbility4 is CAbilityHealthReduction state147) ? new CAbilityHealthReduction(state147, references) : ((cAbility4 is CAbilityImmunityTo state148) ? new CAbilityImmunityTo(state148, references) : ((cAbility4 is CAbilityImprovedShortRest state149) ? new CAbilityImprovedShortRest(state149, references) : ((cAbility4 is CAbilityIncreaseCardLimit state150) ? new CAbilityIncreaseCardLimit(state150, references) : ((cAbility4 is CAbilityInvulnerability state151) ? new CAbilityInvulnerability(state151, references) : ((cAbility4 is CAbilityItemLock state152) ? new CAbilityItemLock(state152, references) : ((cAbility4 is CAbilityKill state153) ? new CAbilityKill(state153, references) : ((cAbility4 is CAbilityLoseCards state154) ? new CAbilityLoseCards(state154, references) : ((cAbility4 is CAbilityLoseGoalChestReward state155) ? new CAbilityLoseGoalChestReward(state155, references) : ((cAbility4 is CAbilityNullTargeting state156) ? new CAbilityNullTargeting(state156, references) : ((cAbility4 is CAbilityOverrideAugmentAttackType state157) ? new CAbilityOverrideAugmentAttackType(state157, references) : ((cAbility4 is CAbilityPierceInvulnerability state158) ? new CAbilityPierceInvulnerability(state158, references) : ((cAbility4 is CAbilityPlaySong state159) ? new CAbilityPlaySong(state159, references) : ((cAbility4 is CAbilityPreventDamage state160) ? new CAbilityPreventDamage(state160, references) : ((cAbility4 is CAbilityRecoverDiscardedCards state161) ? new CAbilityRecoverDiscardedCards(state161, references) : ((cAbility4 is CAbilityRecoverLostCards state162) ? new CAbilityRecoverLostCards(state162, references) : ((cAbility4 is CAbilityRedirect state163) ? new CAbilityRedirect(state163, references) : ((cAbility4 is CAbilityRefreshItemCards state164) ? new CAbilityRefreshItemCards(state164, references) : ((cAbility4 is CAbilityRemoveActorFromMap state165) ? new CAbilityRemoveActorFromMap(state165, references) : ((cAbility4 is CAbilityRemoveConditions state166) ? new CAbilityRemoveConditions(state166, references) : ((cAbility4 is CAbilityRetaliate state167) ? new CAbilityRetaliate(state167, references) : ((cAbility4 is CAbilityShield state168) ? new CAbilityShield(state168, references) : ((cAbility4 is CAbilityShuffleModifierDeck state169) ? new CAbilityShuffleModifierDeck(state169, references) : ((cAbility4 is CAbilityTransferDooms state170) ? new CAbilityTransferDooms(state170, references) : ((cAbility4 is CAbilityUntargetable state171) ? new CAbilityUntargetable(state171, references) : ((cAbility4 is CAbilityCondition state172) ? new CAbilityCondition(state172, references) : ((cAbility4 is CAbilityMergedCreateAttack state173) ? new CAbilityMergedCreateAttack(state173, references) : ((cAbility4 is CAbilityMergedDestroyAttack state174) ? new CAbilityMergedDestroyAttack(state174, references) : ((cAbility4 is CAbilityMergedDisarmTrapDestroyObstacle state175) ? new CAbilityMergedDisarmTrapDestroyObstacle(state175, references) : ((cAbility4 is CAbilityMergedKillCreate state176) ? new CAbilityMergedKillCreate(state176, references) : ((cAbility4 is CAbilityMergedMoveAttack state177) ? new CAbilityMergedMoveAttack(state177, references) : ((cAbility4 is CAbilityMergedMoveObstacleAttack state178) ? new CAbilityMergedMoveObstacleAttack(state178, references) : ((cAbility4 is CAbilityActivateSpawner state179) ? new CAbilityActivateSpawner(state179, references) : ((cAbility4 is CAbilityAddModifierToMonsterDeck state180) ? new CAbilityAddModifierToMonsterDeck(state180, references) : ((cAbility4 is CAbilityAttack state181) ? new CAbilityAttack(state181, references) : ((cAbility4 is CAbilityChangeCondition state182) ? new CAbilityChangeCondition(state182, references) : ((cAbility4 is CAbilityChangeModifier state183) ? new CAbilityChangeModifier(state183, references) : ((cAbility4 is CAbilityConsumeElement state184) ? new CAbilityConsumeElement(state184, references) : ((cAbility4 is CAbilityCreate state185) ? new CAbilityCreate(state185, references) : ((cAbility4 is CAbilityDamage state186) ? new CAbilityDamage(state186, references) : ((cAbility4 is CAbilityDeactivateSpawner state187) ? new CAbilityDeactivateSpawner(state187, references) : ((cAbility4 is CAbilityDestroyObstacle state188) ? new CAbilityDestroyObstacle(state188, references) : ((cAbility4 is CAbilityDisarmTrap state189) ? new CAbilityDisarmTrap(state189, references) : ((cAbility4 is CAbilityFear state190) ? new CAbilityFear(state190, references) : ((cAbility4 is CAbilityInfuse state191) ? new CAbilityInfuse(state191, references) : ((cAbility4 is CAbilityLoot state192) ? new CAbilityLoot(state192, references) : ((cAbility4 is CAbilityMove state193) ? new CAbilityMove(state193, references) : ((cAbility4 is CAbilityMoveObstacle state194) ? new CAbilityMoveObstacle(state194, references) : ((cAbility4 is CAbilityMoveTrap state195) ? new CAbilityMoveTrap(state195, references) : ((cAbility4 is CAbilityNull state196) ? new CAbilityNull(state196, references) : ((cAbility4 is CAbilityNullHex state197) ? new CAbilityNullHex(state197, references) : ((cAbility4 is CAbilityPull state198) ? new CAbilityPull(state198, references) : ((cAbility4 is CAbilityPush state199) ? new CAbilityPush(state199, references) : ((cAbility4 is CAbilityRecoverResources state200) ? new CAbilityRecoverResources(state200, references) : ((cAbility4 is CAbilityRedistributeDamage state201) ? new CAbilityRedistributeDamage(state201, references) : ((cAbility4 is CAbilityRevive state202) ? new CAbilityRevive(state202, references) : ((cAbility4 is CAbilitySummon state203) ? new CAbilitySummon(state203, references) : ((cAbility4 is CAbilitySwap state204) ? new CAbilitySwap(state204, references) : ((cAbility4 is CAbilityTargeting state205) ? new CAbilityTargeting(state205, references) : ((cAbility4 is CAbilityTeleport state206) ? new CAbilityTeleport(state206, references) : ((cAbility4 is CAbilityTrap state207) ? new CAbilityTrap(state207, references) : ((cAbility4 is CAbilityXP state208) ? new CAbilityXP(state208, references) : ((!(cAbility4 is CAbilityMerged state209)) ? new CAbility(cAbility4, references) : new CAbilityMerged(state209, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility5 = cAbility3;
					references.Add(cAbility4, cAbility5);
				}
				ApplicableAbilities.Add(cAbility5);
			}
			references.Add(state.ApplicableAbilities, ApplicableAbilities);
		}
		HasChosenAbility = state.HasChosenAbility;
	}
}
