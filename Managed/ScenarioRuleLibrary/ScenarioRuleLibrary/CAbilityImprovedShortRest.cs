using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityImprovedShortRest : CAbilityTargeting
{
	public CAbilityImprovedShortRest()
		: base(EAbilityType.ImprovedShortRest)
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

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (actor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.CharacterClass.ImprovedShortRest = true;
			}
			else
			{
				DLLDebug.LogError("Trying to apply Improved Short Rest to a non player actor");
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityImprovedShortRest(CAbilityImprovedShortRest state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
