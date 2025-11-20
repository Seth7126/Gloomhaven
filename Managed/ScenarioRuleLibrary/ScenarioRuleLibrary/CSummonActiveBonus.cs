using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CSummonActiveBonus : CActiveBonus
{
	private CActor m_SummonActor;

	public CSummonActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD)
	{
		m_SummonActor = actor;
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.CastAbilityFromSummon)
		{
			if (!(ability is CAbilitySummon ability2))
			{
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Summon active bonus");
			}
			m_BespokeBehaviour = new CSummonActiveBonus_CastAbilityFromSummon(actor, ability2, this);
		}
	}

	public override bool Finished()
	{
		return !ScenarioManager.Scenario.HasActor(m_SummonActor);
	}

	public CSummonActiveBonus()
	{
	}

	public CSummonActiveBonus(CSummonActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
