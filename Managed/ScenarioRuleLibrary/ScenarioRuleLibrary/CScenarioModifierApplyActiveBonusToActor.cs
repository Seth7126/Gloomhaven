using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierApplyActiveBonusToActor : CScenarioModifier
{
	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	public CScenarioModifierApplyActiveBonusToActor()
	{
	}

	public CScenarioModifierApplyActiveBonusToActor(CScenarioModifierApplyActiveBonusToActor state, ReferenceDictionary references)
		: base(state, references)
	{
		TileSpecificAbilityIds = references.Get(state.TileSpecificAbilityIds);
		if (TileSpecificAbilityIds != null || state.TileSpecificAbilityIds == null)
		{
			return;
		}
		TileSpecificAbilityIds = new Dictionary<TileIndex, List<string>>(state.TileSpecificAbilityIds.Comparer);
		foreach (KeyValuePair<TileIndex, List<string>> tileSpecificAbilityId in state.TileSpecificAbilityIds)
		{
			TileIndex tileIndex = references.Get(tileSpecificAbilityId.Key);
			if (tileIndex == null && tileSpecificAbilityId.Key != null)
			{
				tileIndex = new TileIndex(tileSpecificAbilityId.Key, references);
				references.Add(tileSpecificAbilityId.Key, tileIndex);
			}
			List<string> list = references.Get(tileSpecificAbilityId.Value);
			if (list == null && tileSpecificAbilityId.Value != null)
			{
				list = new List<string>();
				for (int i = 0; i < tileSpecificAbilityId.Value.Count; i++)
				{
					string item = tileSpecificAbilityId.Value[i];
					list.Add(item);
				}
				references.Add(tileSpecificAbilityId.Value, list);
			}
			TileSpecificAbilityIds.Add(tileIndex, list);
		}
		references.Add(state.TileSpecificAbilityIds, TileSpecificAbilityIds);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierApplyActiveBonusToActor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "TileSpecificAbilityIds")
				{
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierApplyActiveBonusToActor entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierApplyActiveBonusToActor(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, Dictionary<TileIndex, List<string>> abilityIdsByLocation, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ApplyActiveBonusToActor, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TileSpecificAbilityIds = abilityIdsByLocation?.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value) ?? null;
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
		List<CAbility> list = TriggerAbilities(currentActor, ScenarioManager.CurrentScenarioState.Players.Count);
		if (list == null)
		{
			return;
		}
		foreach (CAbility item in list)
		{
			CAbility cAbility = CAbility.CopyAbility(item, generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(currentActor, currentActor);
			cAbility.ApplyToActor(currentActor);
		}
		base.HasBeenAppliedOnce = true;
		if (base.ApplyToEachActorOnce)
		{
			base.AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
	}

	public override CAbility TriggerAbility(CActor actorPerformingAbility = null, int partySize = 2)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				return ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities[0];
			}
		}
		return base.TriggerAbility(actorPerformingAbility, partySize);
	}

	public override List<CAbility> TriggerAbilities(CActor actorPerformingAbility = null, int partySize = 2)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				return ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities.ToList();
			}
		}
		return base.TriggerAbilities(actorPerformingAbility, partySize);
	}

	public override List<CAbility> AllListedTriggerAbilities()
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(base.ScenarioAbilityID))
		{
			list.Add(base.ScenarioAbilityID);
		}
		if (TileSpecificAbilityIds != null)
		{
			foreach (TileIndex key in TileSpecificAbilityIds.Keys)
			{
				foreach (string item in TileSpecificAbilityIds[key])
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		List<CAbility> list2 = new List<CAbility>();
		foreach (string abilityID in list)
		{
			ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityID);
			if (scenarioAbilitiesYMLData == null)
			{
				continue;
			}
			foreach (CAbility scenarioAbility in scenarioAbilitiesYMLData.ScenarioAbilities)
			{
				list2.Add(scenarioAbility);
			}
		}
		return list2 ?? new List<CAbility>();
	}
}
