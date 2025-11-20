using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CStartTurnAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CStartTurnAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
	}

	public bool CanTriggerAbility()
	{
		bool flag = RequirementsMet();
		if (base.Ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns && !flag)
		{
			TriggerAbility();
		}
		return flag;
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
			DLLDebug.LogError("Invalid phase for StartTurn Ability");
		}
	}

	public CStartTurnAbilityActiveBonus()
	{
	}

	public CStartTurnAbilityActiveBonus(CStartTurnAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
