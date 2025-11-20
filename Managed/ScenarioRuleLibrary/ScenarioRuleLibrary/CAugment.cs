using System;
using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAugment
{
	public enum EAugmentType
	{
		None,
		Ability,
		Override,
		Bonus
	}

	public static EAugmentType[] AugmentTypes = (EAugmentType[])Enum.GetValues(typeof(EAugmentType));

	public Guid ID;

	public EAugmentType AugmentType;

	public CAbility.EAttackType AttackType;

	public List<CAbility> Abilities;

	public List<CAbilityOverride> AbilityOverrides;

	public string ActiveBonusLayout;

	public CBaseCard RegisteredBaseCard { get; set; }

	public CAugment(List<CAbility> abilities, CAbility.EAttackType type, bool augBonus = false)
	{
		ID = Guid.NewGuid();
		AttackType = type;
		if (augBonus)
		{
			AugmentType = EAugmentType.Bonus;
		}
		else
		{
			AugmentType = EAugmentType.Ability;
		}
		Abilities = abilities;
	}

	public CAugment(List<CAbilityOverride> abilityOverrides, CAbility.EAttackType type, bool augBonus = false)
	{
		ID = Guid.NewGuid();
		AugmentType = EAugmentType.Override;
		AbilityOverrides = abilityOverrides;
		AttackType = type;
	}

	public CAugment(CAbility.EAttackType type)
	{
		ID = Guid.NewGuid();
		AugmentType = EAugmentType.Bonus;
		AttackType = type;
	}

	public CAugment()
	{
	}

	public CAugment(CAugment state, ReferenceDictionary references)
	{
		ID = state.ID;
		AugmentType = state.AugmentType;
		AttackType = state.AttackType;
		Abilities = references.Get(state.Abilities);
		if (Abilities == null && state.Abilities != null)
		{
			Abilities = new List<CAbility>();
			for (int i = 0; i < state.Abilities.Count; i++)
			{
				CAbility cAbility = state.Abilities[i];
				CAbility cAbility2 = references.Get(cAbility);
				if (cAbility2 == null && cAbility != null)
				{
					CAbility cAbility3 = ((cAbility is CAbilityBlockHealing state2) ? new CAbilityBlockHealing(state2, references) : ((cAbility is CAbilityNeutralizeShield state3) ? new CAbilityNeutralizeShield(state3, references) : ((cAbility is CAbilityAdvantage state4) ? new CAbilityAdvantage(state4, references) : ((cAbility is CAbilityBless state5) ? new CAbilityBless(state5, references) : ((cAbility is CAbilityCurse state6) ? new CAbilityCurse(state6, references) : ((cAbility is CAbilityDisarm state7) ? new CAbilityDisarm(state7, references) : ((cAbility is CAbilityImmobilize state8) ? new CAbilityImmobilize(state8, references) : ((cAbility is CAbilityImmovable state9) ? new CAbilityImmovable(state9, references) : ((cAbility is CAbilityInvisible state10) ? new CAbilityInvisible(state10, references) : ((cAbility is CAbilityMuddle state11) ? new CAbilityMuddle(state11, references) : ((cAbility is CAbilityOverheal state12) ? new CAbilityOverheal(state12, references) : ((cAbility is CAbilityPoison state13) ? new CAbilityPoison(state13, references) : ((cAbility is CAbilitySleep state14) ? new CAbilitySleep(state14, references) : ((cAbility is CAbilityStopFlying state15) ? new CAbilityStopFlying(state15, references) : ((cAbility is CAbilityStrengthen state16) ? new CAbilityStrengthen(state16, references) : ((cAbility is CAbilityStun state17) ? new CAbilityStun(state17, references) : ((cAbility is CAbilityWound state18) ? new CAbilityWound(state18, references) : ((cAbility is CAbilityChooseAbility state19) ? new CAbilityChooseAbility(state19, references) : ((cAbility is CAbilityAddActiveBonus state20) ? new CAbilityAddActiveBonus(state20, references) : ((cAbility is CAbilityAddAugment state21) ? new CAbilityAddAugment(state21, references) : ((cAbility is CAbilityAddCondition state22) ? new CAbilityAddCondition(state22, references) : ((cAbility is CAbilityAddDoom state23) ? new CAbilityAddDoom(state23, references) : ((cAbility is CAbilityAddDoomSlots state24) ? new CAbilityAddDoomSlots(state24, references) : ((cAbility is CAbilityAddHeal state25) ? new CAbilityAddHeal(state25, references) : ((cAbility is CAbilityAddRange state26) ? new CAbilityAddRange(state26, references) : ((cAbility is CAbilityAddSong state27) ? new CAbilityAddSong(state27, references) : ((cAbility is CAbilityAddTarget state28) ? new CAbilityAddTarget(state28, references) : ((cAbility is CAbilityAdjustInitiative state29) ? new CAbilityAdjustInitiative(state29, references) : ((cAbility is CAbilityAttackersGainDisadvantage state30) ? new CAbilityAttackersGainDisadvantage(state30, references) : ((cAbility is CAbilityChangeAllegiance state31) ? new CAbilityChangeAllegiance(state31, references) : ((cAbility is CAbilityChangeCharacterModel state32) ? new CAbilityChangeCharacterModel(state32, references) : ((cAbility is CAbilityChoose state33) ? new CAbilityChoose(state33, references) : ((cAbility is CAbilityConsume state34) ? new CAbilityConsume(state34, references) : ((cAbility is CAbilityConsumeItemCards state35) ? new CAbilityConsumeItemCards(state35, references) : ((cAbility is CAbilityControlActor state36) ? new CAbilityControlActor(state36, references) : ((cAbility is CAbilityDisableCardAction state37) ? new CAbilityDisableCardAction(state37, references) : ((cAbility is CAbilityDiscardCards state38) ? new CAbilityDiscardCards(state38, references) : ((cAbility is CAbilityExtraTurn state39) ? new CAbilityExtraTurn(state39, references) : ((cAbility is CAbilityForgoActionsForCompanion state40) ? new CAbilityForgoActionsForCompanion(state40, references) : ((cAbility is CAbilityGiveSupplyCard state41) ? new CAbilityGiveSupplyCard(state41, references) : ((cAbility is CAbilityHeal state42) ? new CAbilityHeal(state42, references) : ((cAbility is CAbilityHealthReduction state43) ? new CAbilityHealthReduction(state43, references) : ((cAbility is CAbilityImmunityTo state44) ? new CAbilityImmunityTo(state44, references) : ((cAbility is CAbilityImprovedShortRest state45) ? new CAbilityImprovedShortRest(state45, references) : ((cAbility is CAbilityIncreaseCardLimit state46) ? new CAbilityIncreaseCardLimit(state46, references) : ((cAbility is CAbilityInvulnerability state47) ? new CAbilityInvulnerability(state47, references) : ((cAbility is CAbilityItemLock state48) ? new CAbilityItemLock(state48, references) : ((cAbility is CAbilityKill state49) ? new CAbilityKill(state49, references) : ((cAbility is CAbilityLoseCards state50) ? new CAbilityLoseCards(state50, references) : ((cAbility is CAbilityLoseGoalChestReward state51) ? new CAbilityLoseGoalChestReward(state51, references) : ((cAbility is CAbilityNullTargeting state52) ? new CAbilityNullTargeting(state52, references) : ((cAbility is CAbilityOverrideAugmentAttackType state53) ? new CAbilityOverrideAugmentAttackType(state53, references) : ((cAbility is CAbilityPierceInvulnerability state54) ? new CAbilityPierceInvulnerability(state54, references) : ((cAbility is CAbilityPlaySong state55) ? new CAbilityPlaySong(state55, references) : ((cAbility is CAbilityPreventDamage state56) ? new CAbilityPreventDamage(state56, references) : ((cAbility is CAbilityRecoverDiscardedCards state57) ? new CAbilityRecoverDiscardedCards(state57, references) : ((cAbility is CAbilityRecoverLostCards state58) ? new CAbilityRecoverLostCards(state58, references) : ((cAbility is CAbilityRedirect state59) ? new CAbilityRedirect(state59, references) : ((cAbility is CAbilityRefreshItemCards state60) ? new CAbilityRefreshItemCards(state60, references) : ((cAbility is CAbilityRemoveActorFromMap state61) ? new CAbilityRemoveActorFromMap(state61, references) : ((cAbility is CAbilityRemoveConditions state62) ? new CAbilityRemoveConditions(state62, references) : ((cAbility is CAbilityRetaliate state63) ? new CAbilityRetaliate(state63, references) : ((cAbility is CAbilityShield state64) ? new CAbilityShield(state64, references) : ((cAbility is CAbilityShuffleModifierDeck state65) ? new CAbilityShuffleModifierDeck(state65, references) : ((cAbility is CAbilityTransferDooms state66) ? new CAbilityTransferDooms(state66, references) : ((cAbility is CAbilityUntargetable state67) ? new CAbilityUntargetable(state67, references) : ((cAbility is CAbilityCondition state68) ? new CAbilityCondition(state68, references) : ((cAbility is CAbilityMergedCreateAttack state69) ? new CAbilityMergedCreateAttack(state69, references) : ((cAbility is CAbilityMergedDestroyAttack state70) ? new CAbilityMergedDestroyAttack(state70, references) : ((cAbility is CAbilityMergedDisarmTrapDestroyObstacle state71) ? new CAbilityMergedDisarmTrapDestroyObstacle(state71, references) : ((cAbility is CAbilityMergedKillCreate state72) ? new CAbilityMergedKillCreate(state72, references) : ((cAbility is CAbilityMergedMoveAttack state73) ? new CAbilityMergedMoveAttack(state73, references) : ((cAbility is CAbilityMergedMoveObstacleAttack state74) ? new CAbilityMergedMoveObstacleAttack(state74, references) : ((cAbility is CAbilityActivateSpawner state75) ? new CAbilityActivateSpawner(state75, references) : ((cAbility is CAbilityAddModifierToMonsterDeck state76) ? new CAbilityAddModifierToMonsterDeck(state76, references) : ((cAbility is CAbilityAttack state77) ? new CAbilityAttack(state77, references) : ((cAbility is CAbilityChangeCondition state78) ? new CAbilityChangeCondition(state78, references) : ((cAbility is CAbilityChangeModifier state79) ? new CAbilityChangeModifier(state79, references) : ((cAbility is CAbilityConsumeElement state80) ? new CAbilityConsumeElement(state80, references) : ((cAbility is CAbilityCreate state81) ? new CAbilityCreate(state81, references) : ((cAbility is CAbilityDamage state82) ? new CAbilityDamage(state82, references) : ((cAbility is CAbilityDeactivateSpawner state83) ? new CAbilityDeactivateSpawner(state83, references) : ((cAbility is CAbilityDestroyObstacle state84) ? new CAbilityDestroyObstacle(state84, references) : ((cAbility is CAbilityDisarmTrap state85) ? new CAbilityDisarmTrap(state85, references) : ((cAbility is CAbilityFear state86) ? new CAbilityFear(state86, references) : ((cAbility is CAbilityInfuse state87) ? new CAbilityInfuse(state87, references) : ((cAbility is CAbilityLoot state88) ? new CAbilityLoot(state88, references) : ((cAbility is CAbilityMove state89) ? new CAbilityMove(state89, references) : ((cAbility is CAbilityMoveObstacle state90) ? new CAbilityMoveObstacle(state90, references) : ((cAbility is CAbilityMoveTrap state91) ? new CAbilityMoveTrap(state91, references) : ((cAbility is CAbilityNull state92) ? new CAbilityNull(state92, references) : ((cAbility is CAbilityNullHex state93) ? new CAbilityNullHex(state93, references) : ((cAbility is CAbilityPull state94) ? new CAbilityPull(state94, references) : ((cAbility is CAbilityPush state95) ? new CAbilityPush(state95, references) : ((cAbility is CAbilityRecoverResources state96) ? new CAbilityRecoverResources(state96, references) : ((cAbility is CAbilityRedistributeDamage state97) ? new CAbilityRedistributeDamage(state97, references) : ((cAbility is CAbilityRevive state98) ? new CAbilityRevive(state98, references) : ((cAbility is CAbilitySummon state99) ? new CAbilitySummon(state99, references) : ((cAbility is CAbilitySwap state100) ? new CAbilitySwap(state100, references) : ((cAbility is CAbilityTargeting state101) ? new CAbilityTargeting(state101, references) : ((cAbility is CAbilityTeleport state102) ? new CAbilityTeleport(state102, references) : ((cAbility is CAbilityTrap state103) ? new CAbilityTrap(state103, references) : ((cAbility is CAbilityXP state104) ? new CAbilityXP(state104, references) : ((!(cAbility is CAbilityMerged state105)) ? new CAbility(cAbility, references) : new CAbilityMerged(state105, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility2 = cAbility3;
					references.Add(cAbility, cAbility2);
				}
				Abilities.Add(cAbility2);
			}
			references.Add(state.Abilities, Abilities);
		}
		AbilityOverrides = references.Get(state.AbilityOverrides);
		if (AbilityOverrides == null && state.AbilityOverrides != null)
		{
			AbilityOverrides = new List<CAbilityOverride>();
			for (int j = 0; j < state.AbilityOverrides.Count; j++)
			{
				CAbilityOverride cAbilityOverride = state.AbilityOverrides[j];
				CAbilityOverride cAbilityOverride2 = references.Get(cAbilityOverride);
				if (cAbilityOverride2 == null && cAbilityOverride != null)
				{
					cAbilityOverride2 = new CAbilityOverride(cAbilityOverride, references);
					references.Add(cAbilityOverride, cAbilityOverride2);
				}
				AbilityOverrides.Add(cAbilityOverride2);
			}
			references.Add(state.AbilityOverrides, AbilityOverrides);
		}
		ActiveBonusLayout = state.ActiveBonusLayout;
	}
}
