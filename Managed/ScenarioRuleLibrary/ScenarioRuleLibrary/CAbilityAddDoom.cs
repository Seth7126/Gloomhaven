using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAddDoom : CAbilityTargeting
{
	public CDoom Doom { get; set; }

	public CAbilityAddDoom(CDoom doom)
		: base(EAbilityType.AddDoom)
	{
		Doom = doom;
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
			if (Doom != null)
			{
				base.TargetingActor.AddDoom(Doom, actor);
			}
			else
			{
				DLLDebug.LogError("Unable to find Song for Song Ability " + base.Name);
			}
			if (!GameState.WaitingForMercenarySpecialMechanicSlotChoice)
			{
				CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(base.TargetingActor)
				{
					WaitActor = base.TargetingActor,
					WaitTickFrame = 10000,
					ClearEvents = base.SkipAnim
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			CActorBeenDoomed_MessageData message2 = new CActorBeenDoomed_MessageData(base.TargetingActor)
			{
				m_ActorBeingDoomed = actor,
				m_DoomAbility = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityAddDoom()
	{
	}

	public CAbilityAddDoom(CAbilityAddDoom state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
