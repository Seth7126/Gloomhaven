using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CHealActiveBonus_HealOnDamageReceived : CBespokeBehaviour
{
	public CHealActiveBonus_HealOnDamageReceived(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage)
	{
		base.OnTakenDamage(damageTaken, damagingAbility, damageReducedByShields, actualDamage);
		if (actualDamage <= 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ScenarioManager.Scenario.AllActors.Count; i++)
		{
			CActor cActor = ScenarioManager.Scenario.AllActors[i];
			if ((m_ActiveBonusData.IsAura || cActor != m_Actor) && m_Ability.ActiveBonusData.Filter.IsValidTarget(cActor, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false))
			{
				int health = cActor.Health;
				cActor.Healed(damageTaken);
				flag = true;
				CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(cActor, cActor, damageTaken, health, poisonTokenRemoved: false, woundTokenRemoved: false);
				ScenarioRuleClient.MessageHandler(message);
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

	public CHealActiveBonus_HealOnDamageReceived()
	{
	}

	public CHealActiveBonus_HealOnDamageReceived(CHealActiveBonus_HealOnDamageReceived state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
