using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierToggleActorDeactivated : CScenarioModifier
{
	public CScenarioModifierToggleActorDeactivated()
	{
	}

	public CScenarioModifierToggleActorDeactivated(CScenarioModifierToggleActorDeactivated state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CScenarioModifierToggleActorDeactivated(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CScenarioModifierToggleActorDeactivated(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ToggleActorDeactivated, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			PerformScenarioModifier(roundCount, allActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if ((!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || currentActor.IsDead || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (base.ScenarioModifierFilter != null && currentActor != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && !currentActor.PhasedOut && base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		if (base.ApplyToEachActorOnce)
		{
			base.AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
		base.HasBeenAppliedOnce = true;
		if (!ScenarioManager.Scenario.HasActor(currentActor))
		{
			return;
		}
		bool deactivated = false;
		if (roundCount % 2 == 0)
		{
			if (base.ActivationRound != 0 && base.ActivationRound % 2 == 0)
			{
				deactivated = true;
			}
		}
		else if (base.ActivationRound == 0 || base.ActivationRound % 2 != 0)
		{
			deactivated = true;
		}
		currentActor.Deactivated = deactivated;
	}
}
