using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventDamageAndFullyHeal : CBespokeBehaviour
{
	public CPreventDamageActiveBonus_PreventDamageAndFullyHeal(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		base.OnPreventDamageTriggered(preventedDamage, damageSource, actorDamaged, damagingAbility);
		actorDamaged.Healed(actorDamaged.MaxHealth);
		OnBehaviourTriggered();
	}

	public CPreventDamageActiveBonus_PreventDamageAndFullyHeal()
	{
	}

	public CPreventDamageActiveBonus_PreventDamageAndFullyHeal(CPreventDamageActiveBonus_PreventDamageAndFullyHeal state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
