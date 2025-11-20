using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus_Retaliate : CBespokeBehaviour
{
	public CRetaliateActiveBonus_Retaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack attackAbility, int modifiedStrength)
	{
		if (IsValidTarget(attackAbility, attackAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			if (m_ActiveBonus.HasTracker)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
			}
		}
	}

	public override void OnRetaliate()
	{
		if (m_ActiveBonus.HasTracker && m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public CRetaliateActiveBonus_Retaliate()
	{
	}

	public CRetaliateActiveBonus_Retaliate(CRetaliateActiveBonus_Retaliate state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
