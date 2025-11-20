using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringTurnAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CDuringTurnAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.DuringTurnAbility)
		{
			m_BespokeBehaviour = new CDuringTurnAbilityActiveBonus_TriggerAbility(actor, ability, this);
		}
	}

	public void TriggerAbility()
	{
		RestrictActiveBonus(base.Actor);
		if (m_BespokeBehaviour != null)
		{
			m_BespokeBehaviour.OnBehaviourTriggered();
		}
		if (base.HasTracker)
		{
			UpdateXPTracker();
			if (base.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public CDuringTurnAbilityActiveBonus()
	{
	}

	public CDuringTurnAbilityActiveBonus(CDuringTurnAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
