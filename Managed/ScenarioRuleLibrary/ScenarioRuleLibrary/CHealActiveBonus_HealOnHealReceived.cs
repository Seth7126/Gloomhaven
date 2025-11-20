using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CHealActiveBonus_HealOnHealReceived : CBespokeBehaviour
{
	public CHealActiveBonus_HealOnHealReceived(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingHealed(CAbilityHeal healAbility)
	{
		base.OnBeingHealed(healAbility);
		bool flag = false;
		for (int i = 0; i < ScenarioManager.Scenario.AllActors.Count; i++)
		{
			CActor cActor = ScenarioManager.Scenario.AllActors[i];
			if (cActor != m_Actor && m_Ability.ActiveBonusData.Filter.IsValidTarget(cActor, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false))
			{
				int health = cActor.Health;
				cActor.Healed(healAbility.Strength);
				flag = true;
				CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(cActor, cActor, healAbility.Strength, health, poisonTokenRemoved: false, woundTokenRemoved: false);
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

	public CHealActiveBonus_HealOnHealReceived()
	{
	}

	public CHealActiveBonus_HealOnHealReceived(CHealActiveBonus_HealOnHealReceived state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
