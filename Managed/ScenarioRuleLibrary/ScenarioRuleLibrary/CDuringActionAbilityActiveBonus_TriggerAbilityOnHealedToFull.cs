using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAfterBeingHealed()
	{
		base.OnAfterBeingHealed();
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (cActor.Health < cActor.MaxHealth || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonusData.Filter.IsValidTarget(cActor, m_ActiveBonus.Caster, isTargetedAbility: false, useTargetOriginalType: false, false))
		{
			return;
		}
		CAbility item = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		List<CAbility> list = new List<CAbility> { item };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(list, cActor);
		}
		else
		{
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard);
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
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

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull(CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
