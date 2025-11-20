using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		base.OnAbilityEnded(endedAbility);
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!IsValidAbilityType(endedAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent)
		{
			cAbility.ParentAbility = endedAbility;
		}
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles = AbilityAddActiveBonus.AddAbility.InlineSubAbilityTiles;
		}
		List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
		(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true);
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

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded(CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
