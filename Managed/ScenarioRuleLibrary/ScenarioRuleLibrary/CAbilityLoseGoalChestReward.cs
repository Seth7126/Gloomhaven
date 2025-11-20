using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityLoseGoalChestReward : CAbilityTargeting
{
	public static int STRENGTH_ALL = -1;

	public CAbilityLoseGoalChestReward()
		: base(EAbilityType.LoseGoalChestReward)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (base.Strength != CAbilityRefreshItemCards.STRENGTH_ALL)
		{
			m_OneTargetAtATime = true;
		}
		base.Start(targetActor, filterActor, controllingActor);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsSelectingItemCards_MessageData message = new CActorIsSelectingItemCards_MessageData(base.AnimOverload, actorApplying)
		{
			m_ActorsRefreshed = actorsAppliedTo,
			m_ItemSelectionAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			bool num = ScenarioManager.CurrentScenarioState.GoalChestRewards.Count > 0;
			if (num)
			{
				if (base.Strength == STRENGTH_ALL)
				{
					ScenarioManager.CurrentScenarioState.GoalChestRewards.Clear();
				}
				else
				{
					CLoseGoalChestRewardChoice_MessageData message = new CLoseGoalChestRewardChoice_MessageData
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
			return num;
		}
		return false;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		STRENGTH_ALL = -1;
	}

	public CAbilityLoseGoalChestReward(CAbilityLoseGoalChestReward state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
