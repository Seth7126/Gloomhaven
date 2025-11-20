using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest : CBespokeBehaviour
{
	public CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public void TriggerAbility()
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: true);
		cAbility.SetCanUndo(canUndo: false);
		PhaseManager.StartAbilities(new List<CAbility> { cAbility }, m_ActiveBonus.BaseCard, fullCopy: true);
		m_ActiveBonus.RestrictActiveBonus(cActor);
		OnBehaviourTriggered();
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest(CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
