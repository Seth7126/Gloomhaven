using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster : CBespokeBehaviour
{
	public CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility)
	{
		int num = Math.Max(0, damagePrevented - m_Ability.Strength);
		if (num > 0)
		{
			GameState.RedirectedDamageToActor = new Tuple<CActor, int>(m_ActiveBonus.Caster, num);
		}
		OnBehaviourTriggered();
	}

	public CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster()
	{
	}

	public CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster(CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
