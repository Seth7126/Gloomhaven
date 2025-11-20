using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CEndActionAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CEndActionAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseEndTurn || PhaseManager.CurrentPhase is CPhaseAction)
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
			DLLDebug.LogError("Invalid phase for EndAction Ability");
		}
	}

	public CEndActionAbilityActiveBonus()
	{
	}

	public CEndActionAbilityActiveBonus(CEndActionAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
