using StateCodeGenerator;

namespace ScenarioRuleLibrary;

internal class CAttackersGainDisadvantageActiveBonus : CActiveBonus
{
	public CAttackersGainDisadvantageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public void GainDisadvantageTriggered(bool processTracker = true)
	{
		RestrictActiveBonus(base.Actor);
		if (base.HasTracker)
		{
			UpdateXPTracker(processTracker);
			if (base.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public CAttackersGainDisadvantageActiveBonus()
	{
	}

	public CAttackersGainDisadvantageActiveBonus(CAttackersGainDisadvantageActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
