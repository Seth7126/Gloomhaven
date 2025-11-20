using StateCodeGenerator;

namespace ScenarioRuleLibrary;

internal class COverrideAbilityTypeActiveBonus : CActiveBonus
{
	public COverrideAbilityTypeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		EActiveBonusBehaviourType behaviour = ability.ActiveBonusData.Behaviour;
		if (behaviour == EActiveBonusBehaviourType.OverrideAbility || behaviour == EActiveBonusBehaviourType.OverrideMoveAbility)
		{
			m_BespokeBehaviour = new COverrideAbilityTypeActiveBonus_OverrideAbility(actor, ability, this);
		}
	}

	public COverrideAbilityTypeActiveBonus()
	{
	}

	public COverrideAbilityTypeActiveBonus(COverrideAbilityTypeActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
