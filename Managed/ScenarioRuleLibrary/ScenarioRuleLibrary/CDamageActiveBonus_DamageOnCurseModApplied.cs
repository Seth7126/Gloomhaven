using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDamageActiveBonus_DamageOnCurseModApplied : CBespokeBehaviour
{
	public CDamageActiveBonus_DamageOnCurseModApplied(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public void TriggerAbility(CActor actorToDamage)
	{
		if (!m_Ability.ActiveBonusData.Filter.IsValidTarget(actorToDamage, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false))
		{
			return;
		}
		actorToDamage.ApplyImmediateDamage(m_Ability.Strength);
		GameState.ActorHealthCheck(m_Actor, actorToDamage);
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

	public CDamageActiveBonus_DamageOnCurseModApplied()
	{
	}

	public CDamageActiveBonus_DamageOnCurseModApplied(CDamageActiveBonus_DamageOnCurseModApplied state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
