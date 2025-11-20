using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CStartRoundAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CStartRoundAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool CanTriggerAbility()
	{
		return RequirementsMet();
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseStartTurn || PhaseManager.CurrentPhase is CPhaseAction)
		{
			if (!CheckAddAbilityValidity(AddAbility))
			{
				return;
			}
			RestrictActiveBonus(base.Actor);
			if (base.HasTracker)
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
			DLLDebug.LogError("Invalid phase for StartRound Ability");
		}
	}

	public CStartRoundAbilityActiveBonus()
	{
	}

	public CStartRoundAbilityActiveBonus(CStartRoundAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
