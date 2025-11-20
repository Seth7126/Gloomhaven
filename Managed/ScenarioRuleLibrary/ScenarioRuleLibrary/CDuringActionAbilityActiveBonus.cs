using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus : CActiveBonus
{
	public CAbility AddAbility;

	public CDuringActionAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.DuringActionAbility:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbility(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAttacked:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnKill:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnKill(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnMoved:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnMoved(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnMovedORCarried:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedORCarried(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnCreated:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnCreated(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnFinishedMovement:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnFinishedMovement(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnMovedIntoCasterHex:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedIntoCasterHex(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAttackAbilityFinished:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackAbilityFinished(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAttackFinished:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAttackStart:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackStart(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnLongRest:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnDamaged:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnDamaged(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnDeath:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAttacking:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnPreventedDamage:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnAbilityEnded:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnHealed:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnHealed(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.DuringActionAbilityOnHealedToFull:
			m_BespokeBehaviour = new CDuringActionAbilityActiveBonus_TriggerAbilityOnHealedToFull(actor, ability, this);
			break;
		}
	}

	public void TriggerAbility()
	{
		if (PhaseManager.CurrentPhase is CPhaseAction)
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
		else
		{
			DLLDebug.LogError("Invalid phase for DuringAction Ability");
		}
	}

	public CDuringActionAbilityActiveBonus()
	{
	}

	public CDuringActionAbilityActiveBonus(CDuringActionAbilityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
