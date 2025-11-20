using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CChangeConditionActiveBonus : CActiveBonus
{
	public CChangeConditionActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool ChangeCondition(CTokens token, CCondition.EPositiveCondition condition, int duration, EConditionDecTrigger decTrigger, CActor actor)
	{
		if (base.Ability.MiscAbilityData.ReplacePositiveConditions != null && base.Ability.MiscAbilityData.ReplacePositiveConditions.SingleOrDefault((CCondition.EPositiveCondition s) => s == condition) != CCondition.EPositiveCondition.NA)
		{
			if (base.Ability.MiscAbilityData.ReplaceWithPositiveConditions != null)
			{
				token.AddPositiveToken(base.Ability.MiscAbilityData.ReplaceWithPositiveConditions[0], duration, decTrigger, actor, recall: true);
				token.ModifiedPositiveCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
			if (base.Ability.MiscAbilityData.ReplaceWithNegativeConditions != null)
			{
				token.AddNegativeToken(base.Ability.MiscAbilityData.ReplaceWithNegativeConditions[0], duration, decTrigger, actor, recall: true);
				token.ModifiedPositiveCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
		}
		return true;
	}

	public bool ChangeCondition(CTokens token, CCondition.ENegativeCondition condition, int duration, EConditionDecTrigger decTrigger, CActor actor)
	{
		if (base.Ability.MiscAbilityData.ReplaceNegativeConditions != null && base.Ability.MiscAbilityData.ReplaceNegativeConditions.SingleOrDefault((CCondition.ENegativeCondition s) => s == condition) != CCondition.ENegativeCondition.NA)
		{
			if (base.Ability.MiscAbilityData.ReplaceWithPositiveConditions != null)
			{
				token.AddPositiveToken(base.Ability.MiscAbilityData.ReplaceWithPositiveConditions[0], duration, decTrigger, actor, recall: true);
				token.ModifiedNegativeCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
			if (base.Ability.MiscAbilityData.ReplaceWithNegativeConditions != null)
			{
				token.AddNegativeToken(base.Ability.MiscAbilityData.ReplaceWithNegativeConditions[0], duration, decTrigger, actor, recall: true);
				token.ModifiedNegativeCondition = condition;
				RestrictActiveBonus(actor);
				return false;
			}
		}
		return true;
	}

	public CChangeConditionActiveBonus()
	{
	}

	public CChangeConditionActiveBonus(CChangeConditionActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
