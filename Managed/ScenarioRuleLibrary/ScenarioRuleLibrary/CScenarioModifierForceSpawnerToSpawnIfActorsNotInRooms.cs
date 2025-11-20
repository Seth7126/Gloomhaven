using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms : CScenarioModifier
{
	public string SpawnerGUID { get; private set; }

	public List<string> CheckMapGUIDs { get; private set; }

	public int CheckRoundInterval { get; private set; }

	public int CheckRoundIntervalOffset { get; private set; }

	public CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms()
	{
	}

	public CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms(CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms state, ReferenceDictionary references)
		: base(state, references)
	{
		SpawnerGUID = state.SpawnerGUID;
		CheckMapGUIDs = references.Get(state.CheckMapGUIDs);
		if (CheckMapGUIDs == null && state.CheckMapGUIDs != null)
		{
			CheckMapGUIDs = new List<string>();
			for (int i = 0; i < state.CheckMapGUIDs.Count; i++)
			{
				string item = state.CheckMapGUIDs[i];
				CheckMapGUIDs.Add(item);
			}
			references.Add(state.CheckMapGUIDs, CheckMapGUIDs);
		}
		CheckRoundInterval = state.CheckRoundInterval;
		CheckRoundIntervalOffset = state.CheckRoundIntervalOffset;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpawnerGUID", SpawnerGUID);
		info.AddValue("CheckMapGUIDs", CheckMapGUIDs);
		info.AddValue("CheckRoundInterval", CheckRoundInterval);
		info.AddValue("CheckRoundIntervalOffset", CheckRoundIntervalOffset);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms(SerializationInfo info, StreamingContext context)
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
				case "SpawnerGUID":
					SpawnerGUID = info.GetString("SpawnerGUID");
					break;
				case "CheckMapGUIDs":
					CheckMapGUIDs = (List<string>)info.GetValue("CheckMapGUIDs", typeof(List<string>));
					break;
				case "CheckRoundInterval":
					CheckRoundInterval = info.GetInt32("CheckRoundInterval");
					break;
				case "CheckRoundIntervalOffset":
					CheckRoundIntervalOffset = info.GetInt32("CheckRoundIntervalOffset");
					break;
				case "RoomMapGUIDs":
					CheckMapGUIDs = (List<string>)info.GetValue("RoomMapGUIDs", typeof(List<string>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string spawnerGUID, int checkRoundInterval, int checkRoundIntervalOffset, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ForceSpawnerToSpawnIfActorsNotInRooms, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		SpawnerGUID = spawnerGUID;
		CheckMapGUIDs = roomMapGuids;
		CheckRoundInterval = checkRoundInterval;
		CheckRoundIntervalOffset = checkRoundIntervalOffset;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || (CheckRoundInterval != 0 && (ScenarioManager.CurrentScenarioState.RoundNumber + CheckRoundIntervalOffset) % CheckRoundInterval != 0))
		{
			return;
		}
		List<CMap> list = new List<CMap>();
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			if (!base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				continue;
			}
			CActor cActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (cActor != null)
			{
				CTile cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				if (cTile.m_HexMap != null && CheckMapGUIDs.Contains(cTile.m_HexMap.MapGuid) && !list.Contains(cTile.m_HexMap))
				{
					list.Add(cTile.m_HexMap);
				}
			}
		}
		if (CheckMapGUIDs.Count != list.Count)
		{
			ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner x) => x.SpawnerGuid == SpawnerGUID)?.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: true);
		}
	}
}
