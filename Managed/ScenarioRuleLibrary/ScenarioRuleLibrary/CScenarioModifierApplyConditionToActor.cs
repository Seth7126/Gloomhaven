using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierApplyConditionToActor : CScenarioModifier
{
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	public EConditionDecTrigger ConditionDecrementTrigger { get; private set; }

	public CScenarioModifierApplyConditionToActor()
	{
	}

	public CScenarioModifierApplyConditionToActor(CScenarioModifierApplyConditionToActor state, ReferenceDictionary references)
		: base(state, references)
	{
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<CCondition.EPositiveCondition>();
			for (int i = 0; i < state.PositiveConditions.Count; i++)
			{
				CCondition.EPositiveCondition item = state.PositiveConditions[i];
				PositiveConditions.Add(item);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<CCondition.ENegativeCondition>();
			for (int j = 0; j < state.NegativeConditions.Count; j++)
			{
				CCondition.ENegativeCondition item2 = state.NegativeConditions[j];
				NegativeConditions.Add(item2);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		ConditionDecrementTrigger = state.ConditionDecrementTrigger;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("ConditionDecrementTrigger", ConditionDecrementTrigger);
	}

	public CScenarioModifierApplyConditionToActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PositiveConditions":
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "ConditionDecrementTrigger":
					ConditionDecrementTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecrementTrigger", typeof(EConditionDecTrigger));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierApplyConditionToActor entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierApplyConditionToActor(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.Turns, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ApplyConditionToActor, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		ConditionDecrementTrigger = conditionDecrementTrigger;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			PerformScenarioModifier(roundCount, allActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || (!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = true;
		if (base.ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null)
			{
				if (!base.ScenarioModifierFilter.IsValidTarget(actorState))
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
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
		foreach (CCondition.EPositiveCondition posCon in PositiveConditions)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCon.ToString()), new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 0, 1, 1, 1, ConditionDecrementTrigger);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(currentActor, currentActor);
			if (cAbility is CAbilityBless cAbilityBless)
			{
				cAbilityBless.ApplyToActor(currentActor, canGoOverBlessLimit: true);
			}
			else
			{
				((CAbilityTargeting)cAbility).ApplyToActor(currentActor);
			}
		}
		foreach (CCondition.ENegativeCondition negCon in NegativeConditions)
		{
			CAbility cAbility2 = CAbility.CreateAbility(CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCon.ToString()), new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: true, 0, 1, 1, 1, ConditionDecrementTrigger);
			cAbility2.IsScenarioModifierAbility = true;
			cAbility2.Start(currentActor, currentActor);
			if (cAbility2 is CAbilityCurse cAbilityCurse)
			{
				cAbilityCurse.ApplyToActor(currentActor, canGoOverCurseLimit: true);
			}
			else
			{
				((CAbilityTargeting)cAbility2).ApplyToActor(currentActor);
			}
		}
	}
}
