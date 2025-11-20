using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierAddConditionsToAbilities : CScenarioModifier
{
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	public CScenarioModifierAddConditionsToAbilities()
	{
	}

	public CScenarioModifierAddConditionsToAbilities(CScenarioModifierAddConditionsToAbilities state, ReferenceDictionary references)
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
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public CScenarioModifierAddConditionsToAbilities(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "PositiveConditions"))
				{
					if (name == "NegativeConditions")
					{
						NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					}
				}
				else
				{
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierAddConditionsToAbilities entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierAddConditionsToAbilities(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.AddConditionsToAbilities, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}

	public bool ShouldAddConditions(CActor currentActor, CAbility.EAbilityType currentAbilityType)
	{
		if (base.AfterAbilityTypes != null && base.AfterAbilityTypes.Count > 0 && !base.AfterAbilityTypes.Contains(currentAbilityType))
		{
			return false;
		}
		if (base.ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null)
			{
				if (base.ScenarioModifierFilter.IsValidTarget(actorState))
				{
					return base.ScenarioModifierFilter.IsValidAbilityType(currentAbilityType);
				}
				return false;
			}
			return false;
		}
		return true;
	}
}
