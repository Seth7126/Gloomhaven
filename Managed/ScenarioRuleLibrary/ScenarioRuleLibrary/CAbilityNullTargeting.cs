using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityNullTargeting : CAbilityTargeting
{
	public CAbilityNullTargeting()
		: base(EAbilityType.NullTargeting)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(base.AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityNullTargeting(CAbilityNullTargeting state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
