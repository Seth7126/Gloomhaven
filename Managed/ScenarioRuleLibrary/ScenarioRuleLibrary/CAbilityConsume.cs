using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityConsume : CAbilityTargeting
{
	public CAbilityConsume()
		: base(EAbilityType.Consume)
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (CActor.IsSameType(actor, base.TargetingActor))
			{
				if (m_PositiveConditions.Count > 0)
				{
					ProcessPositiveStatusEffects(actor);
				}
			}
			else if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return m_PositiveConditions.Count > 0;
	}

	public CAbilityConsume(CAbilityConsume state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
