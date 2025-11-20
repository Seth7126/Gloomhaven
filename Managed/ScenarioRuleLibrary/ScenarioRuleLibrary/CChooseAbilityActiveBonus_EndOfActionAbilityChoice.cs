using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CChooseAbilityActiveBonus_EndOfActionAbilityChoice : CBespokeBehaviour
{
	private CChooseAbilityActiveBonus ChooseAbilityActiveBonus;

	public CChooseAbilityActiveBonus_EndOfActionAbilityChoice(CActor actor, CAbility ability, CChooseAbilityActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		ChooseAbilityActiveBonus = activeBonus;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(endedAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(endedAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnActiveBonusToggled(CActor currentActor, bool toggledOn)
	{
		if (toggledOn)
		{
			List<CAbility> inlineSubAbilities = new List<CAbility> { CAbility.CopyAbility(ChooseAbilityActiveBonus.chosenAbility, generateNewID: false) };
			(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, currentActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: true, m_ActiveBonus);
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(ChooseAbilityActiveBonus.chosenAbility, m_ActiveBonus);
		}
	}

	public CChooseAbilityActiveBonus_EndOfActionAbilityChoice()
	{
	}

	public CChooseAbilityActiveBonus_EndOfActionAbilityChoice(CChooseAbilityActiveBonus_EndOfActionAbilityChoice state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
