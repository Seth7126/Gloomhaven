using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus : CActiveBonus
{
	public CAdjustInitiativeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.AdjustInitiative:
			m_BespokeBehaviour = new CAdjustInitiativeActiveBonus_AdjustInitiative(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.FocusInitiative:
			m_BespokeBehaviour = new CAdjustInitiativeActiveBonus_FocusInitiative(actor, ability, this);
			break;
		}
	}

	public void IsInitiativeAdjusted()
	{
		if (!IsActiveBonusToggledAndNotRestricted(base.Actor))
		{
			return;
		}
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

	public CAdjustInitiativeActiveBonus()
	{
	}

	public CAdjustInitiativeActiveBonus(CAdjustInitiativeActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
