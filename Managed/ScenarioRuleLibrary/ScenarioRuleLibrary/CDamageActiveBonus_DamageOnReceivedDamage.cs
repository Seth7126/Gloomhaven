using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDamageActiveBonus_DamageOnReceivedDamage : CBespokeBehaviour
{
	public CDamageActiveBonus_DamageOnReceivedDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage)
	{
		base.OnTakenDamage(damageTaken, damagingAbility, damageReducedByShields, actualDamage);
		bool flag = false;
		for (int i = 0; i < ScenarioManager.Scenario.AllActors.Count; i++)
		{
			if (ScenarioManager.Scenario.AllActors[i] != m_Actor && m_Ability.ActiveBonusData.Filter.IsValidTarget(ScenarioManager.Scenario.AllActors[i], m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false))
			{
				ScenarioManager.Scenario.AllActors[i].ApplyImmediateDamage(damageTaken, cannotPrevent: false, pierceInvulnerable: true);
				GameState.ActorHealthCheck(m_Actor, ScenarioManager.Scenario.AllActors[i]);
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
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

	public CDamageActiveBonus_DamageOnReceivedDamage()
	{
	}

	public CDamageActiveBonus_DamageOnReceivedDamage(CDamageActiveBonus_DamageOnReceivedDamage state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
