using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus : CActiveBonus
{
	public bool PreventDamageAttackSourcesOnly { get; set; }

	public bool PreventOnlyIfLethal { get; set; }

	public CPreventDamageActiveBonus(CBaseCard baseCard, CAbilityPreventDamage ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		PreventDamageAttackSourcesOnly = ability.AttackSourcesOnly;
		AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
		PreventOnlyIfLethal = miscAbilityData != null && miscAbilityData.PreventOnlyIfLethal == true;
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.PreventAndRetaliate:
			m_BespokeBehaviour = new CPreventDamageActiveBonus_PreventAndRetaliate(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.PreventAndApplyToActiveBonusCaster:
			m_BespokeBehaviour = new CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards:
			m_BespokeBehaviour = new CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.PreventAndFullyHeal:
			m_BespokeBehaviour = new CPreventDamageActiveBonus_PreventDamageAndFullyHeal(actor, ability, this);
			break;
		}
	}

	public override void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		if (!IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			return;
		}
		CPreventDamageTriggered_MessageData message = new CPreventDamageTriggered_MessageData("", base.Actor)
		{
			m_PreventDamageBaseCard = base.Ability.AbilityBaseCard
		};
		ScenarioRuleClient.MessageHandler(message);
		RestrictActiveBonus(base.Actor);
		foreach (ElementInfusionBoardManager.EElement item in base.Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, base.Actor);
		}
		actorDamaged.m_OnPreventDamageListeners?.Invoke(damagePrevented, damageSource, actorDamaged, damagingAbility);
		if (m_BespokeBehaviour == null && base.Ability.ActiveBonusData.IsToggleBonus)
		{
			base.ToggledBonus = false;
			base.ToggleLocked = false;
			base.ToggledElement = null;
		}
		if (!base.HasTracker)
		{
			return;
		}
		UpdateXPTracker();
		if (base.Remaining <= 0)
		{
			Finish();
			if (base.BespokeBehaviour != null)
			{
				base.BespokeBehaviour.Finish();
			}
		}
	}

	public CPreventDamageActiveBonus()
	{
	}

	public CPreventDamageActiveBonus(CPreventDamageActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
		PreventDamageAttackSourcesOnly = state.PreventDamageAttackSourcesOnly;
		PreventOnlyIfLethal = state.PreventOnlyIfLethal;
	}
}
