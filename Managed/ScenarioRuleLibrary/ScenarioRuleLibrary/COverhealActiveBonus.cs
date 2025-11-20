using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class COverhealActiveBonus : CActiveBonus
{
	public COverhealActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
		case EActiveBonusBehaviourType.AddAccumulativeOverheal:
			m_BespokeBehaviour = new COverhealActiveBonus_AddAccumulativeOverheal(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.SetMinimumOverheal:
			m_BespokeBehaviour = new COverhealActiveBonus_SetMinimumOverheal(actor, ability, this);
			break;
		}
	}

	public void CheckForOverhealStrengthUpdate()
	{
		switch (base.Ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
		case EActiveBonusBehaviourType.AddAccumulativeOverheal:
			(m_BespokeBehaviour as COverhealActiveBonus_AddAccumulativeOverheal)?.UpdateCurrentOverhealStrength();
			break;
		case EActiveBonusBehaviourType.SetMinimumOverheal:
			(m_BespokeBehaviour as COverhealActiveBonus_SetMinimumOverheal)?.UpdateCurrentOverhealStrength();
			break;
		}
	}

	public override void Finish()
	{
		base.Finish();
		switch (base.Ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax:
		case EActiveBonusBehaviourType.AddAccumulativeOverheal:
			(m_BespokeBehaviour as COverhealActiveBonus_AddAccumulativeOverheal)?.GetAccumulativeOverhealAndUpdate(excludeSelf: true);
			break;
		case EActiveBonusBehaviourType.SetMinimumOverheal:
			(m_BespokeBehaviour as COverhealActiveBonus_SetMinimumOverheal)?.CheckForAllMinimumOverhealsAndUpdate(excludeSelf: true);
			break;
		}
	}

	public COverhealActiveBonus()
	{
	}

	public COverhealActiveBonus(COverhealActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
