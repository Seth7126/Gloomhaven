using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus : CActiveBonus
{
	public CAddConditionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability is CAbilityAddCondition ability2)
		{
			switch (ability.ActiveBonusData.Behaviour)
			{
			case EActiveBonusBehaviourType.MultiplyAddedConditions:
				m_BespokeBehaviour = new CAddConditionActiveBonus_MultiplyAddedConditions(actor, ability2, this);
				break;
			case EActiveBonusBehaviourType.ApplyConditionalConditions:
				m_BespokeBehaviour = new CAddConditionActiveBonus_ApplyConditionalConditions(actor, ability2, this);
				break;
			case EActiveBonusBehaviourType.ApplyConditionOnLoseCondition:
				m_BespokeBehaviour = new CAddConditionActiveBonus_ApplyConditionOnLoseCondition(actor, ability2, this);
				break;
			case EActiveBonusBehaviourType.AddConditionUntilDamaged:
				m_BespokeBehaviour = new CAddConditionActiveBonus_AddConditionUntilDamaged(actor, ability2, this);
				break;
			}
			return;
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Condition active bonus");
	}

	public CAddConditionActiveBonus()
	{
	}

	public CAddConditionActiveBonus(CAddConditionActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
