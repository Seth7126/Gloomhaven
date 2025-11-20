using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus : CActiveBonus
{
	public CRetaliateActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.Retaliate:
			if (ability is CAbilityRetaliate ability3)
			{
				m_BespokeBehaviour = new CRetaliateActiveBonus_Retaliate(actor, ability3, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Retaliate active bonus");
		case EActiveBonusBehaviourType.BuffRetaliate:
			if (ability is CAbilityRetaliate ability2)
			{
				m_BespokeBehaviour = new CRetaliateActiveBonus_BuffRetaliate(actor, ability2, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Retaliate active bonus");
		}
	}

	public CRetaliateActiveBonus()
	{
	}

	public CRetaliateActiveBonus(CRetaliateActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
