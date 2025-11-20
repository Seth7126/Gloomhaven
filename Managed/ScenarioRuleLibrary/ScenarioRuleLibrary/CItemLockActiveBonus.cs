using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CItemLockActiveBonus : CActiveBonus
{
	public CItemLockActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public override void Finish()
	{
		base.Finish();
		base.Actor?.ActivatePassiveItems(firstLoad: true);
	}

	public CItemLockActiveBonus()
	{
	}

	public CItemLockActiveBonus(CItemLockActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
