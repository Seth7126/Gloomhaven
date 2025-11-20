using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityMerged : CAbility
{
	protected List<CAbility> m_MergedAbilities;

	protected List<CAbility> m_CopiedMergedAbilities;

	protected CAbility m_ActiveAbility;

	protected CActor m_StartTargetActor;

	protected CActor m_StartFilterActor;

	public List<CAbility> MergedAbilities => m_MergedAbilities;

	public List<CAbility> CopiedMergedAbilities => m_CopiedMergedAbilities;

	public CAbility ActiveAbility => m_ActiveAbility;

	public CAbilityMerged(CAbility abilityA, CAbility abilityB)
	{
		m_MergedAbilities = new List<CAbility>();
		m_CopiedMergedAbilities = new List<CAbility>();
		m_MergedAbilities.Add(abilityA);
		m_MergedAbilities.Add(abilityB);
		abilityA.ParentAbility = this;
		abilityB.ParentAbility = this;
	}

	public virtual void CopyMergedAbilities()
	{
		m_CopiedMergedAbilities.Clear();
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_StartTargetActor = targetActor;
		m_StartFilterActor = filterActor;
	}

	public virtual CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedAbilities[0].Equals(ability))
		{
			return m_MergedAbilities[0];
		}
		return m_MergedAbilities[1];
	}

	public override void OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null)
	{
		if (item == null)
		{
			CAbility cAbility = m_CopiedMergedAbilities.SingleOrDefault((CAbility x) => x.Name == abilityOverride.ParentName);
			if (cAbility != null)
			{
				cAbility.OverrideAbilityValues(abilityOverride, perform, item, filter);
			}
			else
			{
				DLLDebug.LogError("Error trying to override Merged ability");
			}
		}
		else
		{
			m_ActiveAbility.OverrideAbilityValues(abilityOverride, perform, item, filter);
		}
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		if (item == null)
		{
			CAbility cAbility = m_CopiedMergedAbilities.SingleOrDefault((CAbility x) => x.Name == abilityOverride.ParentName);
			if (cAbility != null)
			{
				cAbility.UndoOverride(abilityOverride, perform, item);
			}
			else
			{
				DLLDebug.LogError("Error trying to undo override Merged ability");
			}
		}
		else
		{
			m_ActiveAbility.UndoOverride(abilityOverride, perform, item);
		}
	}

	public override void ClearTargets()
	{
		m_ActiveAbility.ClearTargets();
	}

	public override void ToggleSingleTargetItem(CItem item)
	{
		m_ActiveAbility.ToggleSingleTargetItem(item);
	}

	public override void Restart()
	{
		m_ActiveAbility.Restart();
	}

	public override bool CanReceiveTileSelection()
	{
		return ActiveAbility?.CanReceiveTileSelection() ?? false;
	}

	public override bool RequiresWaypointSelection()
	{
		return ActiveAbility?.RequiresWaypointSelection() ?? false;
	}

	public override bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		return ActiveAbility?.ShouldRestartAbilityWhenApplyingOverride(abilityOverride) ?? false;
	}

	public CAbilityMerged()
	{
	}

	public CAbilityMerged(CAbilityMerged state, ReferenceDictionary references)
		: base(state, references)
	{
		m_MergedAbilities = references.Get(state.m_MergedAbilities);
		if (m_MergedAbilities == null && state.m_MergedAbilities != null)
		{
			m_MergedAbilities = new List<CAbility>();
			for (int i = 0; i < state.m_MergedAbilities.Count; i++)
			{
				CAbility cAbility = state.m_MergedAbilities[i];
				CAbility cAbility2 = references.Get(cAbility);
				if (cAbility2 == null && cAbility != null)
				{
					CAbility cAbility3 = ((cAbility is CAbilityBlockHealing state2) ? new CAbilityBlockHealing(state2, references) : ((cAbility is CAbilityNeutralizeShield state3) ? new CAbilityNeutralizeShield(state3, references) : ((cAbility is CAbilityAdvantage state4) ? new CAbilityAdvantage(state4, references) : ((cAbility is CAbilityBless state5) ? new CAbilityBless(state5, references) : ((cAbility is CAbilityCurse state6) ? new CAbilityCurse(state6, references) : ((cAbility is CAbilityDisarm state7) ? new CAbilityDisarm(state7, references) : ((cAbility is CAbilityImmobilize state8) ? new CAbilityImmobilize(state8, references) : ((cAbility is CAbilityImmovable state9) ? new CAbilityImmovable(state9, references) : ((cAbility is CAbilityInvisible state10) ? new CAbilityInvisible(state10, references) : ((cAbility is CAbilityMuddle state11) ? new CAbilityMuddle(state11, references) : ((cAbility is CAbilityOverheal state12) ? new CAbilityOverheal(state12, references) : ((cAbility is CAbilityPoison state13) ? new CAbilityPoison(state13, references) : ((cAbility is CAbilitySleep state14) ? new CAbilitySleep(state14, references) : ((cAbility is CAbilityStopFlying state15) ? new CAbilityStopFlying(state15, references) : ((cAbility is CAbilityStrengthen state16) ? new CAbilityStrengthen(state16, references) : ((cAbility is CAbilityStun state17) ? new CAbilityStun(state17, references) : ((cAbility is CAbilityWound state18) ? new CAbilityWound(state18, references) : ((cAbility is CAbilityChooseAbility state19) ? new CAbilityChooseAbility(state19, references) : ((cAbility is CAbilityAddActiveBonus state20) ? new CAbilityAddActiveBonus(state20, references) : ((cAbility is CAbilityAddAugment state21) ? new CAbilityAddAugment(state21, references) : ((cAbility is CAbilityAddCondition state22) ? new CAbilityAddCondition(state22, references) : ((cAbility is CAbilityAddDoom state23) ? new CAbilityAddDoom(state23, references) : ((cAbility is CAbilityAddDoomSlots state24) ? new CAbilityAddDoomSlots(state24, references) : ((cAbility is CAbilityAddHeal state25) ? new CAbilityAddHeal(state25, references) : ((cAbility is CAbilityAddRange state26) ? new CAbilityAddRange(state26, references) : ((cAbility is CAbilityAddSong state27) ? new CAbilityAddSong(state27, references) : ((cAbility is CAbilityAddTarget state28) ? new CAbilityAddTarget(state28, references) : ((cAbility is CAbilityAdjustInitiative state29) ? new CAbilityAdjustInitiative(state29, references) : ((cAbility is CAbilityAttackersGainDisadvantage state30) ? new CAbilityAttackersGainDisadvantage(state30, references) : ((cAbility is CAbilityChangeAllegiance state31) ? new CAbilityChangeAllegiance(state31, references) : ((cAbility is CAbilityChangeCharacterModel state32) ? new CAbilityChangeCharacterModel(state32, references) : ((cAbility is CAbilityChoose state33) ? new CAbilityChoose(state33, references) : ((cAbility is CAbilityConsume state34) ? new CAbilityConsume(state34, references) : ((cAbility is CAbilityConsumeItemCards state35) ? new CAbilityConsumeItemCards(state35, references) : ((cAbility is CAbilityControlActor state36) ? new CAbilityControlActor(state36, references) : ((cAbility is CAbilityDisableCardAction state37) ? new CAbilityDisableCardAction(state37, references) : ((cAbility is CAbilityDiscardCards state38) ? new CAbilityDiscardCards(state38, references) : ((cAbility is CAbilityExtraTurn state39) ? new CAbilityExtraTurn(state39, references) : ((cAbility is CAbilityForgoActionsForCompanion state40) ? new CAbilityForgoActionsForCompanion(state40, references) : ((cAbility is CAbilityGiveSupplyCard state41) ? new CAbilityGiveSupplyCard(state41, references) : ((cAbility is CAbilityHeal state42) ? new CAbilityHeal(state42, references) : ((cAbility is CAbilityHealthReduction state43) ? new CAbilityHealthReduction(state43, references) : ((cAbility is CAbilityImmunityTo state44) ? new CAbilityImmunityTo(state44, references) : ((cAbility is CAbilityImprovedShortRest state45) ? new CAbilityImprovedShortRest(state45, references) : ((cAbility is CAbilityIncreaseCardLimit state46) ? new CAbilityIncreaseCardLimit(state46, references) : ((cAbility is CAbilityInvulnerability state47) ? new CAbilityInvulnerability(state47, references) : ((cAbility is CAbilityItemLock state48) ? new CAbilityItemLock(state48, references) : ((cAbility is CAbilityKill state49) ? new CAbilityKill(state49, references) : ((cAbility is CAbilityLoseCards state50) ? new CAbilityLoseCards(state50, references) : ((cAbility is CAbilityLoseGoalChestReward state51) ? new CAbilityLoseGoalChestReward(state51, references) : ((cAbility is CAbilityNullTargeting state52) ? new CAbilityNullTargeting(state52, references) : ((cAbility is CAbilityOverrideAugmentAttackType state53) ? new CAbilityOverrideAugmentAttackType(state53, references) : ((cAbility is CAbilityPierceInvulnerability state54) ? new CAbilityPierceInvulnerability(state54, references) : ((cAbility is CAbilityPlaySong state55) ? new CAbilityPlaySong(state55, references) : ((cAbility is CAbilityPreventDamage state56) ? new CAbilityPreventDamage(state56, references) : ((cAbility is CAbilityRecoverDiscardedCards state57) ? new CAbilityRecoverDiscardedCards(state57, references) : ((cAbility is CAbilityRecoverLostCards state58) ? new CAbilityRecoverLostCards(state58, references) : ((cAbility is CAbilityRedirect state59) ? new CAbilityRedirect(state59, references) : ((cAbility is CAbilityRefreshItemCards state60) ? new CAbilityRefreshItemCards(state60, references) : ((cAbility is CAbilityRemoveActorFromMap state61) ? new CAbilityRemoveActorFromMap(state61, references) : ((cAbility is CAbilityRemoveConditions state62) ? new CAbilityRemoveConditions(state62, references) : ((cAbility is CAbilityRetaliate state63) ? new CAbilityRetaliate(state63, references) : ((cAbility is CAbilityShield state64) ? new CAbilityShield(state64, references) : ((cAbility is CAbilityShuffleModifierDeck state65) ? new CAbilityShuffleModifierDeck(state65, references) : ((cAbility is CAbilityTransferDooms state66) ? new CAbilityTransferDooms(state66, references) : ((cAbility is CAbilityUntargetable state67) ? new CAbilityUntargetable(state67, references) : ((cAbility is CAbilityCondition state68) ? new CAbilityCondition(state68, references) : ((cAbility is CAbilityMergedCreateAttack state69) ? new CAbilityMergedCreateAttack(state69, references) : ((cAbility is CAbilityMergedDestroyAttack state70) ? new CAbilityMergedDestroyAttack(state70, references) : ((cAbility is CAbilityMergedDisarmTrapDestroyObstacle state71) ? new CAbilityMergedDisarmTrapDestroyObstacle(state71, references) : ((cAbility is CAbilityMergedKillCreate state72) ? new CAbilityMergedKillCreate(state72, references) : ((cAbility is CAbilityMergedMoveAttack state73) ? new CAbilityMergedMoveAttack(state73, references) : ((cAbility is CAbilityMergedMoveObstacleAttack state74) ? new CAbilityMergedMoveObstacleAttack(state74, references) : ((cAbility is CAbilityActivateSpawner state75) ? new CAbilityActivateSpawner(state75, references) : ((cAbility is CAbilityAddModifierToMonsterDeck state76) ? new CAbilityAddModifierToMonsterDeck(state76, references) : ((cAbility is CAbilityAttack state77) ? new CAbilityAttack(state77, references) : ((cAbility is CAbilityChangeCondition state78) ? new CAbilityChangeCondition(state78, references) : ((cAbility is CAbilityChangeModifier state79) ? new CAbilityChangeModifier(state79, references) : ((cAbility is CAbilityConsumeElement state80) ? new CAbilityConsumeElement(state80, references) : ((cAbility is CAbilityCreate state81) ? new CAbilityCreate(state81, references) : ((cAbility is CAbilityDamage state82) ? new CAbilityDamage(state82, references) : ((cAbility is CAbilityDeactivateSpawner state83) ? new CAbilityDeactivateSpawner(state83, references) : ((cAbility is CAbilityDestroyObstacle state84) ? new CAbilityDestroyObstacle(state84, references) : ((cAbility is CAbilityDisarmTrap state85) ? new CAbilityDisarmTrap(state85, references) : ((cAbility is CAbilityFear state86) ? new CAbilityFear(state86, references) : ((cAbility is CAbilityInfuse state87) ? new CAbilityInfuse(state87, references) : ((cAbility is CAbilityLoot state88) ? new CAbilityLoot(state88, references) : ((cAbility is CAbilityMove state89) ? new CAbilityMove(state89, references) : ((cAbility is CAbilityMoveObstacle state90) ? new CAbilityMoveObstacle(state90, references) : ((cAbility is CAbilityMoveTrap state91) ? new CAbilityMoveTrap(state91, references) : ((cAbility is CAbilityNull state92) ? new CAbilityNull(state92, references) : ((cAbility is CAbilityNullHex state93) ? new CAbilityNullHex(state93, references) : ((cAbility is CAbilityPull state94) ? new CAbilityPull(state94, references) : ((cAbility is CAbilityPush state95) ? new CAbilityPush(state95, references) : ((cAbility is CAbilityRecoverResources state96) ? new CAbilityRecoverResources(state96, references) : ((cAbility is CAbilityRedistributeDamage state97) ? new CAbilityRedistributeDamage(state97, references) : ((cAbility is CAbilityRevive state98) ? new CAbilityRevive(state98, references) : ((cAbility is CAbilitySummon state99) ? new CAbilitySummon(state99, references) : ((cAbility is CAbilitySwap state100) ? new CAbilitySwap(state100, references) : ((cAbility is CAbilityTargeting state101) ? new CAbilityTargeting(state101, references) : ((cAbility is CAbilityTeleport state102) ? new CAbilityTeleport(state102, references) : ((cAbility is CAbilityTrap state103) ? new CAbilityTrap(state103, references) : ((cAbility is CAbilityXP state104) ? new CAbilityXP(state104, references) : ((!(cAbility is CAbilityMerged state105)) ? new CAbility(cAbility, references) : new CAbilityMerged(state105, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility2 = cAbility3;
					references.Add(cAbility, cAbility2);
				}
				m_MergedAbilities.Add(cAbility2);
			}
			references.Add(state.m_MergedAbilities, m_MergedAbilities);
		}
		m_CopiedMergedAbilities = references.Get(state.m_CopiedMergedAbilities);
		if (m_CopiedMergedAbilities != null || state.m_CopiedMergedAbilities == null)
		{
			return;
		}
		m_CopiedMergedAbilities = new List<CAbility>();
		for (int j = 0; j < state.m_CopiedMergedAbilities.Count; j++)
		{
			CAbility cAbility4 = state.m_CopiedMergedAbilities[j];
			CAbility cAbility5 = references.Get(cAbility4);
			if (cAbility5 == null && cAbility4 != null)
			{
				CAbility cAbility3 = ((cAbility4 is CAbilityBlockHealing state106) ? new CAbilityBlockHealing(state106, references) : ((cAbility4 is CAbilityNeutralizeShield state107) ? new CAbilityNeutralizeShield(state107, references) : ((cAbility4 is CAbilityAdvantage state108) ? new CAbilityAdvantage(state108, references) : ((cAbility4 is CAbilityBless state109) ? new CAbilityBless(state109, references) : ((cAbility4 is CAbilityCurse state110) ? new CAbilityCurse(state110, references) : ((cAbility4 is CAbilityDisarm state111) ? new CAbilityDisarm(state111, references) : ((cAbility4 is CAbilityImmobilize state112) ? new CAbilityImmobilize(state112, references) : ((cAbility4 is CAbilityImmovable state113) ? new CAbilityImmovable(state113, references) : ((cAbility4 is CAbilityInvisible state114) ? new CAbilityInvisible(state114, references) : ((cAbility4 is CAbilityMuddle state115) ? new CAbilityMuddle(state115, references) : ((cAbility4 is CAbilityOverheal state116) ? new CAbilityOverheal(state116, references) : ((cAbility4 is CAbilityPoison state117) ? new CAbilityPoison(state117, references) : ((cAbility4 is CAbilitySleep state118) ? new CAbilitySleep(state118, references) : ((cAbility4 is CAbilityStopFlying state119) ? new CAbilityStopFlying(state119, references) : ((cAbility4 is CAbilityStrengthen state120) ? new CAbilityStrengthen(state120, references) : ((cAbility4 is CAbilityStun state121) ? new CAbilityStun(state121, references) : ((cAbility4 is CAbilityWound state122) ? new CAbilityWound(state122, references) : ((cAbility4 is CAbilityChooseAbility state123) ? new CAbilityChooseAbility(state123, references) : ((cAbility4 is CAbilityAddActiveBonus state124) ? new CAbilityAddActiveBonus(state124, references) : ((cAbility4 is CAbilityAddAugment state125) ? new CAbilityAddAugment(state125, references) : ((cAbility4 is CAbilityAddCondition state126) ? new CAbilityAddCondition(state126, references) : ((cAbility4 is CAbilityAddDoom state127) ? new CAbilityAddDoom(state127, references) : ((cAbility4 is CAbilityAddDoomSlots state128) ? new CAbilityAddDoomSlots(state128, references) : ((cAbility4 is CAbilityAddHeal state129) ? new CAbilityAddHeal(state129, references) : ((cAbility4 is CAbilityAddRange state130) ? new CAbilityAddRange(state130, references) : ((cAbility4 is CAbilityAddSong state131) ? new CAbilityAddSong(state131, references) : ((cAbility4 is CAbilityAddTarget state132) ? new CAbilityAddTarget(state132, references) : ((cAbility4 is CAbilityAdjustInitiative state133) ? new CAbilityAdjustInitiative(state133, references) : ((cAbility4 is CAbilityAttackersGainDisadvantage state134) ? new CAbilityAttackersGainDisadvantage(state134, references) : ((cAbility4 is CAbilityChangeAllegiance state135) ? new CAbilityChangeAllegiance(state135, references) : ((cAbility4 is CAbilityChangeCharacterModel state136) ? new CAbilityChangeCharacterModel(state136, references) : ((cAbility4 is CAbilityChoose state137) ? new CAbilityChoose(state137, references) : ((cAbility4 is CAbilityConsume state138) ? new CAbilityConsume(state138, references) : ((cAbility4 is CAbilityConsumeItemCards state139) ? new CAbilityConsumeItemCards(state139, references) : ((cAbility4 is CAbilityControlActor state140) ? new CAbilityControlActor(state140, references) : ((cAbility4 is CAbilityDisableCardAction state141) ? new CAbilityDisableCardAction(state141, references) : ((cAbility4 is CAbilityDiscardCards state142) ? new CAbilityDiscardCards(state142, references) : ((cAbility4 is CAbilityExtraTurn state143) ? new CAbilityExtraTurn(state143, references) : ((cAbility4 is CAbilityForgoActionsForCompanion state144) ? new CAbilityForgoActionsForCompanion(state144, references) : ((cAbility4 is CAbilityGiveSupplyCard state145) ? new CAbilityGiveSupplyCard(state145, references) : ((cAbility4 is CAbilityHeal state146) ? new CAbilityHeal(state146, references) : ((cAbility4 is CAbilityHealthReduction state147) ? new CAbilityHealthReduction(state147, references) : ((cAbility4 is CAbilityImmunityTo state148) ? new CAbilityImmunityTo(state148, references) : ((cAbility4 is CAbilityImprovedShortRest state149) ? new CAbilityImprovedShortRest(state149, references) : ((cAbility4 is CAbilityIncreaseCardLimit state150) ? new CAbilityIncreaseCardLimit(state150, references) : ((cAbility4 is CAbilityInvulnerability state151) ? new CAbilityInvulnerability(state151, references) : ((cAbility4 is CAbilityItemLock state152) ? new CAbilityItemLock(state152, references) : ((cAbility4 is CAbilityKill state153) ? new CAbilityKill(state153, references) : ((cAbility4 is CAbilityLoseCards state154) ? new CAbilityLoseCards(state154, references) : ((cAbility4 is CAbilityLoseGoalChestReward state155) ? new CAbilityLoseGoalChestReward(state155, references) : ((cAbility4 is CAbilityNullTargeting state156) ? new CAbilityNullTargeting(state156, references) : ((cAbility4 is CAbilityOverrideAugmentAttackType state157) ? new CAbilityOverrideAugmentAttackType(state157, references) : ((cAbility4 is CAbilityPierceInvulnerability state158) ? new CAbilityPierceInvulnerability(state158, references) : ((cAbility4 is CAbilityPlaySong state159) ? new CAbilityPlaySong(state159, references) : ((cAbility4 is CAbilityPreventDamage state160) ? new CAbilityPreventDamage(state160, references) : ((cAbility4 is CAbilityRecoverDiscardedCards state161) ? new CAbilityRecoverDiscardedCards(state161, references) : ((cAbility4 is CAbilityRecoverLostCards state162) ? new CAbilityRecoverLostCards(state162, references) : ((cAbility4 is CAbilityRedirect state163) ? new CAbilityRedirect(state163, references) : ((cAbility4 is CAbilityRefreshItemCards state164) ? new CAbilityRefreshItemCards(state164, references) : ((cAbility4 is CAbilityRemoveActorFromMap state165) ? new CAbilityRemoveActorFromMap(state165, references) : ((cAbility4 is CAbilityRemoveConditions state166) ? new CAbilityRemoveConditions(state166, references) : ((cAbility4 is CAbilityRetaliate state167) ? new CAbilityRetaliate(state167, references) : ((cAbility4 is CAbilityShield state168) ? new CAbilityShield(state168, references) : ((cAbility4 is CAbilityShuffleModifierDeck state169) ? new CAbilityShuffleModifierDeck(state169, references) : ((cAbility4 is CAbilityTransferDooms state170) ? new CAbilityTransferDooms(state170, references) : ((cAbility4 is CAbilityUntargetable state171) ? new CAbilityUntargetable(state171, references) : ((cAbility4 is CAbilityCondition state172) ? new CAbilityCondition(state172, references) : ((cAbility4 is CAbilityMergedCreateAttack state173) ? new CAbilityMergedCreateAttack(state173, references) : ((cAbility4 is CAbilityMergedDestroyAttack state174) ? new CAbilityMergedDestroyAttack(state174, references) : ((cAbility4 is CAbilityMergedDisarmTrapDestroyObstacle state175) ? new CAbilityMergedDisarmTrapDestroyObstacle(state175, references) : ((cAbility4 is CAbilityMergedKillCreate state176) ? new CAbilityMergedKillCreate(state176, references) : ((cAbility4 is CAbilityMergedMoveAttack state177) ? new CAbilityMergedMoveAttack(state177, references) : ((cAbility4 is CAbilityMergedMoveObstacleAttack state178) ? new CAbilityMergedMoveObstacleAttack(state178, references) : ((cAbility4 is CAbilityActivateSpawner state179) ? new CAbilityActivateSpawner(state179, references) : ((cAbility4 is CAbilityAddModifierToMonsterDeck state180) ? new CAbilityAddModifierToMonsterDeck(state180, references) : ((cAbility4 is CAbilityAttack state181) ? new CAbilityAttack(state181, references) : ((cAbility4 is CAbilityChangeCondition state182) ? new CAbilityChangeCondition(state182, references) : ((cAbility4 is CAbilityChangeModifier state183) ? new CAbilityChangeModifier(state183, references) : ((cAbility4 is CAbilityConsumeElement state184) ? new CAbilityConsumeElement(state184, references) : ((cAbility4 is CAbilityCreate state185) ? new CAbilityCreate(state185, references) : ((cAbility4 is CAbilityDamage state186) ? new CAbilityDamage(state186, references) : ((cAbility4 is CAbilityDeactivateSpawner state187) ? new CAbilityDeactivateSpawner(state187, references) : ((cAbility4 is CAbilityDestroyObstacle state188) ? new CAbilityDestroyObstacle(state188, references) : ((cAbility4 is CAbilityDisarmTrap state189) ? new CAbilityDisarmTrap(state189, references) : ((cAbility4 is CAbilityFear state190) ? new CAbilityFear(state190, references) : ((cAbility4 is CAbilityInfuse state191) ? new CAbilityInfuse(state191, references) : ((cAbility4 is CAbilityLoot state192) ? new CAbilityLoot(state192, references) : ((cAbility4 is CAbilityMove state193) ? new CAbilityMove(state193, references) : ((cAbility4 is CAbilityMoveObstacle state194) ? new CAbilityMoveObstacle(state194, references) : ((cAbility4 is CAbilityMoveTrap state195) ? new CAbilityMoveTrap(state195, references) : ((cAbility4 is CAbilityNull state196) ? new CAbilityNull(state196, references) : ((cAbility4 is CAbilityNullHex state197) ? new CAbilityNullHex(state197, references) : ((cAbility4 is CAbilityPull state198) ? new CAbilityPull(state198, references) : ((cAbility4 is CAbilityPush state199) ? new CAbilityPush(state199, references) : ((cAbility4 is CAbilityRecoverResources state200) ? new CAbilityRecoverResources(state200, references) : ((cAbility4 is CAbilityRedistributeDamage state201) ? new CAbilityRedistributeDamage(state201, references) : ((cAbility4 is CAbilityRevive state202) ? new CAbilityRevive(state202, references) : ((cAbility4 is CAbilitySummon state203) ? new CAbilitySummon(state203, references) : ((cAbility4 is CAbilitySwap state204) ? new CAbilitySwap(state204, references) : ((cAbility4 is CAbilityTargeting state205) ? new CAbilityTargeting(state205, references) : ((cAbility4 is CAbilityTeleport state206) ? new CAbilityTeleport(state206, references) : ((cAbility4 is CAbilityTrap state207) ? new CAbilityTrap(state207, references) : ((cAbility4 is CAbilityXP state208) ? new CAbilityXP(state208, references) : ((!(cAbility4 is CAbilityMerged state209)) ? new CAbility(cAbility4, references) : new CAbilityMerged(state209, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
				cAbility5 = cAbility3;
				references.Add(cAbility4, cAbility5);
			}
			m_CopiedMergedAbilities.Add(cAbility5);
		}
		references.Add(state.m_CopiedMergedAbilities, m_CopiedMergedAbilities);
	}
}
