using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbility : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbility(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, bool toggledOn)
	{
		if (IsValidAbilityType(currentAbility) && toggledOn)
		{
			CAbility item = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
			List<CAbility> inlineSubAbilities = new List<CAbility> { item };
			CPhaseAction obj = PhaseManager.CurrentPhase as CPhaseAction;
			CActiveBonus activeBonus = m_ActiveBonus;
			obj.StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true, activeBonus);
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(AbilityAddActiveBonus.AddAbility, m_ActiveBonus);
		}
	}

	public CDuringActionAbilityActiveBonus_TriggerAbility()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbility(CDuringActionAbilityActiveBonus_TriggerAbility state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
