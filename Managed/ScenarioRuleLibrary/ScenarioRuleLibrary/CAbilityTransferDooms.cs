using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityTransferDooms : CAbilityTargeting
{
	public CAbilityTransferDooms()
		: base(EAbilityType.TransferDooms)
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
			if (m_Strength == int.MaxValue)
			{
				base.TargetingActor.TransferAllDooms(actor);
			}
			else
			{
				CTransferDoomChoice_MessageData message = new CTransferDoomChoice_MessageData(base.TargetingActor)
				{
					m_NewDoomTargetActor = actor,
					m_TransferDoomAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
			}
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

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
		base.TargetingActor.ClearCharacterSpecialMechanicsCache(clearAugments: false, clearSongs: false, clearDooms: true);
	}

	public CAbilityTransferDooms(CAbilityTransferDooms state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
