using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CMoveActiveBonus : CActiveBonus
{
	public CMoveActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.BuffMove)
		{
			if (!(ability is CAbilityMove ability2))
			{
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus MoveBuffStrength");
			}
			m_BespokeBehaviour = new CMoveActiveBonus_BuffMove(actor, ability2, this);
		}
	}

	public CMoveActiveBonus()
	{
	}

	public CMoveActiveBonus(CMoveActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
