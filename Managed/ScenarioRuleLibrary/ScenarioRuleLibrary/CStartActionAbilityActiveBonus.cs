using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CStartActionAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CStartActionAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseAction)
		{
			if (!CheckAddAbilityValidity(AddAbility))
			{
				return;
			}
			RestrictActiveBonus(base.Actor);
			if (base.HasTracker && RequirementsMet(base.Actor))
			{
				UpdateXPTracker();
				if (base.Remaining <= 0)
				{
					Finish();
				}
			}
		}
		else
		{
			DLLDebug.LogError("Invalid phase for StartAction Ability");
		}
	}

	public CStartActionAbilityActiveBonus()
	{
	}

	public CStartActionAbilityActiveBonus(CStartActionAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
