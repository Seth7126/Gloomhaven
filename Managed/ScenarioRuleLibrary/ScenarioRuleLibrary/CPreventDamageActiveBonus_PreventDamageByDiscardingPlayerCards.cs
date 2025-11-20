using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards : CBespokeBehaviour
{
	public CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility)
	{
		OnBehaviourTriggered();
	}

	public CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards()
	{
	}

	public CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards(CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
