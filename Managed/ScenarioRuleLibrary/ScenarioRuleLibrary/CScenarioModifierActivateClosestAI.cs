using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierActivateClosestAI : CScenarioModifier
{
	public int HighestRoundProcessed;

	public int HighestRoundActivated;

	public string CurrentChosenActorGUID;

	public TileIndex CurrentChosenTargetTile;

	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	public CScenarioModifierActivateClosestAI()
	{
	}

	public CScenarioModifierActivateClosestAI(CScenarioModifierActivateClosestAI state, ReferenceDictionary references)
		: base(state, references)
	{
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
		HighestRoundProcessed = state.HighestRoundProcessed;
		HighestRoundActivated = state.HighestRoundActivated;
		CurrentChosenActorGUID = state.CurrentChosenActorGUID;
		CurrentChosenTargetTile = references.Get(state.CurrentChosenTargetTile);
		if (CurrentChosenTargetTile == null && state.CurrentChosenTargetTile != null)
		{
			CurrentChosenTargetTile = new TileIndex(state.CurrentChosenTargetTile, references);
			references.Add(state.CurrentChosenTargetTile, CurrentChosenTargetTile);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		info.AddValue("HighestRoundProcessed", HighestRoundProcessed);
		info.AddValue("CurrentChosenActorGUID", CurrentChosenActorGUID);
		info.AddValue("CurrentChosenTargetTile", CurrentChosenTargetTile);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierActivateClosestAI(SerializationInfo info, StreamingContext context)
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
				case "TileSpecificAbilityIds":
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
					break;
				case "HighestRoundProcessed":
					HighestRoundProcessed = info.GetInt32("HighestRoundProcessed");
					break;
				case "CurrentChosenActorGUID":
					CurrentChosenActorGUID = info.GetString("CurrentChosenActorGUID");
					break;
				case "CurrentChosenTargetTile":
					CurrentChosenTargetTile = (TileIndex)info.GetValue("CurrentChosenTargetTile", typeof(TileIndex));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierActivateClosestAI entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierActivateClosestAI(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, Dictionary<TileIndex, List<string>> abilityIdsByLocation, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ActivateClosestAI, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
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
		if (!IsActiveInRound(roundCount, forceActivate) || currentActor == null || (base.ApplyOnceTotal && base.HasBeenAppliedOnce))
		{
			return;
		}
		if (HighestRoundProcessed < roundCount)
		{
			HighestRoundProcessed = roundCount;
			CurrentChosenActorGUID = string.Empty;
			CurrentChosenTargetTile = null;
		}
		if (HighestRoundActivated >= roundCount || (!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (base.ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = true;
		if (!string.IsNullOrEmpty(CurrentChosenActorGUID))
		{
			flag2 = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor a) => a.ActorGuid == CurrentChosenActorGUID)?.IsDead ?? true;
		}
		if (flag2)
		{
			List<CActor> list = new List<CActor>();
			List<ActorState> list2 = new List<ActorState>();
			foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (!actorState2.IsDead && actorState2.AIFocusOverride != null && actorState2.AIFocusOverride.OverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.None && base.ScenarioModifierFilter.IsValidTarget(actorState2))
				{
					CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor a) => a.ActorGuid == actorState2.ActorGuid);
					if (cActor != null && !cActor.IsDead)
					{
						list.Add(cActor);
						list2.Add(actorState2);
					}
				}
			}
			if (list.Count > 0)
			{
				List<Tuple<GameState.CRangeSortedActor, TileIndex>> rangeIndexedActors = new List<Tuple<GameState.CRangeSortedActor, TileIndex>>();
				foreach (CActor actor in list)
				{
					foreach (TileIndex key in TileSpecificAbilityIds.Keys)
					{
						bool foundPath;
						List<Point> list3 = ScenarioManager.PathFinder.FindPath(new Point(key), actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
						if (!foundPath)
						{
							continue;
						}
						GameState.CRangeSortedActor existingActorOfSame = rangeIndexedActors.FirstOrDefault((Tuple<GameState.CRangeSortedActor, TileIndex> r) => r.Item1.m_Actor.ActorGuid == actor.ActorGuid)?.Item1;
						if (existingActorOfSame != null)
						{
							if (existingActorOfSame.m_Range < list3.Count)
							{
								rangeIndexedActors.RemoveAll((Tuple<GameState.CRangeSortedActor, TileIndex> r) => r.Item1 == existingActorOfSame);
								rangeIndexedActors.Add(new Tuple<GameState.CRangeSortedActor, TileIndex>(new GameState.CRangeSortedActor(actor, list3.Count), key));
							}
						}
						else
						{
							rangeIndexedActors.Add(new Tuple<GameState.CRangeSortedActor, TileIndex>(new GameState.CRangeSortedActor(actor, list3.Count), key));
						}
					}
				}
				rangeIndexedActors.Sort((Tuple<GameState.CRangeSortedActor, TileIndex> x, Tuple<GameState.CRangeSortedActor, TileIndex> y) => GameState.s_ActorInitiativeComparer.Compare(x.Item1.m_Actor, y.Item1.m_Actor));
				rangeIndexedActors.Sort((Tuple<GameState.CRangeSortedActor, TileIndex> x, Tuple<GameState.CRangeSortedActor, TileIndex> y) => x.Item1.m_Range.CompareTo(y.Item1.m_Range));
				if (rangeIndexedActors.Count > 0)
				{
					CurrentChosenActorGUID = rangeIndexedActors[0].Item1.m_Actor.ActorGuid;
					CurrentChosenTargetTile = rangeIndexedActors[0].Item2;
				}
				int i;
				for (i = 0; i < rangeIndexedActors.Count; i++)
				{
					ActorState actorState3 = list2.FirstOrDefault((ActorState a) => a.ActorGuid == rangeIndexedActors[i].Item1.m_Actor.ActorGuid);
					if (i == 0)
					{
						actorState3.AIFocusOverride.IsDisabled = false;
						List<CAbility> list4 = TriggerAbilities(currentActor, ScenarioManager.CurrentScenarioState.Players.Count);
						if (list4 != null)
						{
							list[i].MindControlDuration = CAbilityControlActor.EControlDurationType.ControlForOneTurn;
							GameState.QueueOverrideActorForOneTurn(list[i], list[i].Type, list4.ToList(), this);
						}
					}
					else
					{
						actorState3.AIFocusOverride.IsDisabled = true;
					}
				}
			}
		}
		ActorState actorState4 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == currentActor.ActorGuid);
		if (CurrentChosenActorGUID == currentActor.ActorGuid)
		{
			actorState4.AIFocusOverride.IsDisabled = false;
			actorState4.AIFocusOverride.ForcedUseTileIndex = CurrentChosenTargetTile;
			HighestRoundActivated = roundCount;
			if (!TileSpecificAbilityIds.TryGetValue(CurrentChosenTargetTile, out var abilityIds) || abilityIds.Count < partySize)
			{
				return;
			}
			List<CAbility> list5 = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities.ToList();
			if (list5 != null)
			{
				currentActor.MindControlDuration = CAbilityControlActor.EControlDurationType.ControlForOneTurn;
				GameState.QueueOverrideActorForOneTurn(currentActor, currentActor.Type, list5.ToList(), this);
				if (!base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
				{
					base.AppliedToActorGUIDs.Add(currentActor.ActorGuid);
				}
				base.HasBeenAppliedOnce = true;
			}
		}
		else
		{
			actorState4.AIFocusOverride.IsDisabled = true;
			actorState4.AIFocusOverride.ForcedUseTileIndex = null;
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
