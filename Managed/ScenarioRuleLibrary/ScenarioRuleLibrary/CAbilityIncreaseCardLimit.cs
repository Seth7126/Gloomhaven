using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityIncreaseCardLimit : CAbilityTargeting
{
	public CAbilityIncreaseCardLimit()
		: base(EAbilityType.IncreaseCardLimit)
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			base.TargetingActor.FindCardWithAbility(this);
			_ = (CPlayerActor)actor;
			CSelectIncreasedCardLimit_MessageData cSelectIncreasedCardLimit_MessageData = new CSelectIncreasedCardLimit_MessageData(base.AnimOverload, base.TargetingActor);
			cSelectIncreasedCardLimit_MessageData.m_ActorIncreasingCardLimit = actor;
			cSelectIncreasedCardLimit_MessageData.m_Ability = this;
			ScenarioRuleClient.MessageHandler(cSelectIncreasedCardLimit_MessageData);
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityIncreaseCardLimit(CAbilityIncreaseCardLimit state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
