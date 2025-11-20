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
public class CScenarioModifierTriggerAbility : CScenarioModifier
{
	public enum ETriggerAbilityStackingType
	{
		None,
		StackToPhaseAbilities,
		StackNextCurrentPhaseAbility
	}

	public static ETriggerAbilityStackingType[] TriggerAbilityStackingTypes = (ETriggerAbilityStackingType[])Enum.GetValues(typeof(ETriggerAbilityStackingType));

	public ETriggerAbilityStackingType TriggerAbilityStackingType { get; private set; }

	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	public List<TileIndex> InlineSubAbilityTileIndexes { get; private set; }

	public int? OverrideTriggerAbilityStrength { get; private set; }

	public CScenarioModifierTriggerAbility()
	{
	}

	public CScenarioModifierTriggerAbility(CScenarioModifierTriggerAbility state, ReferenceDictionary references)
		: base(state, references)
	{
		TriggerAbilityStackingType = state.TriggerAbilityStackingType;
		TileSpecificAbilityIds = references.Get(state.TileSpecificAbilityIds);
		if (TileSpecificAbilityIds == null && state.TileSpecificAbilityIds != null)
		{
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
		InlineSubAbilityTileIndexes = references.Get(state.InlineSubAbilityTileIndexes);
		if (InlineSubAbilityTileIndexes == null && state.InlineSubAbilityTileIndexes != null)
		{
			InlineSubAbilityTileIndexes = new List<TileIndex>();
			for (int j = 0; j < state.InlineSubAbilityTileIndexes.Count; j++)
			{
				TileIndex tileIndex2 = state.InlineSubAbilityTileIndexes[j];
				TileIndex tileIndex3 = references.Get(tileIndex2);
				if (tileIndex3 == null && tileIndex2 != null)
				{
					tileIndex3 = new TileIndex(tileIndex2, references);
					references.Add(tileIndex2, tileIndex3);
				}
				InlineSubAbilityTileIndexes.Add(tileIndex3);
			}
			references.Add(state.InlineSubAbilityTileIndexes, InlineSubAbilityTileIndexes);
		}
		OverrideTriggerAbilityStrength = state.OverrideTriggerAbilityStrength;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TriggerAbilityStackingType", TriggerAbilityStackingType);
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		info.AddValue("InlineSubAbilityTileIndexes", InlineSubAbilityTileIndexes);
		info.AddValue("OverrideTriggerAbilityStrength", OverrideTriggerAbilityStrength);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierTriggerAbility(SerializationInfo info, StreamingContext context)
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
				case "TriggerAbilityStackingType":
					TriggerAbilityStackingType = (ETriggerAbilityStackingType)info.GetValue("TriggerAbilityStackingType", typeof(ETriggerAbilityStackingType));
					break;
				case "TileSpecificAbilityIds":
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
					break;
				case "InlineSubAbilityTileIndexes":
					InlineSubAbilityTileIndexes = (List<TileIndex>)info.GetValue("InlineSubAbilityTileIndexes", typeof(List<TileIndex>));
					break;
				case "OverrideTriggerAbilityStrength":
					OverrideTriggerAbilityStrength = (int?)info.GetValue("OverrideTriggerAbilityStrength", typeof(int?));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierTriggerAbility entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierTriggerAbility(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, ETriggerAbilityStackingType triggerAbilityType, Dictionary<TileIndex, List<string>> abilityIdsByLocation, List<TileIndex> inlineSubAbilityTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, int? overrideTriggerAbilityStrength = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.TriggerAbility, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TriggerAbilityStackingType = triggerAbilityType;
		TileSpecificAbilityIds = abilityIdsByLocation?.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value) ?? null;
		InlineSubAbilityTileIndexes = inlineSubAbilityTiles.ToList();
		OverrideTriggerAbilityStrength = overrideTriggerAbilityStrength;
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
		if (!IsActiveInRound(roundCount, forceActivate) || currentActor == null || (!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (base.ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.ToList().SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
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
		if (currentActor is CEnemyActor cEnemyActor)
		{
			for (int num = 0; num < list.Count; num++)
			{
				CAbility ability = list[num];
				CAbility value = cEnemyActor.CloneAbilityAndApplyBaseStats(ability);
				list[num] = value;
			}
		}
		if (OverrideTriggerAbilityStrength.HasValue)
		{
			foreach (CAbility item in list)
			{
				item.Strength = OverrideTriggerAbilityStrength.Value;
			}
		}
		foreach (CAbility item2 in list)
		{
			if (!item2.IsInlineSubAbility)
			{
				continue;
			}
			foreach (TileIndex inlineSubAbilityTileIndex in InlineSubAbilityTileIndexes)
			{
				item2.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[inlineSubAbilityTileIndex.X, inlineSubAbilityTileIndex.Y]);
			}
		}
		if (PhaseManager.CurrentPhase is CPhaseAction)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list.ToList(), currentActor, killAfter: false, TriggerAbilityStackingType == ETriggerAbilityStackingType.StackNextCurrentPhaseAbility);
		}
		else
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(list[num2], currentActor));
			}
		}
		if (base.ApplyToEachActorOnce)
		{
			base.AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
		base.HasBeenAppliedOnce = true;
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
