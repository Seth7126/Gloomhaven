using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringTurnAbilityActiveBonus_TriggerAbility : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringTurnAbilityActiveBonus_TriggerAbility(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, bool toggledOn)
	{
		if (toggledOn)
		{
			CAbility item = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
			List<CAbility> list = new List<CAbility> { item };
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				CActiveBonus activeBonus = m_ActiveBonus;
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true, activeBonus);
				return;
			}
			if (GameState.InternalCurrentActor != m_ActiveBonus.Caster)
			{
				GameState.OverrideCurrentActorForOneAction(m_ActiveBonus.Caster);
			}
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true, m_ActiveBonus);
		}
		else if (!toggledOn && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
		{
			cPhaseAction2.UnstackAbility(AbilityAddActiveBonus.AddAbility, m_ActiveBonus);
		}
	}

	public CDuringTurnAbilityActiveBonus_TriggerAbility()
	{
	}

	public CDuringTurnAbilityActiveBonus_TriggerAbility(CDuringTurnAbilityActiveBonus_TriggerAbility state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
