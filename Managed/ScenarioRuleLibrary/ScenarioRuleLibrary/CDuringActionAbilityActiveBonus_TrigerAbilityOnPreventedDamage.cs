using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		base.OnPreventDamageTriggered(damagePrevented, damageSource, actorDamaged, damagingAbility);
		if (!IsValidTarget(m_Ability, actorDamaged) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			return;
		}
		CAbility item = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		List<CAbility> nextAbilities = new List<CAbility> { item };
		(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, actorDamaged, killAfter: false, stackToNextCurrent: true);
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		m_ActiveBonus.RestrictActiveBonus(actorDamaged);
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

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage()
	{
	}

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage(CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
