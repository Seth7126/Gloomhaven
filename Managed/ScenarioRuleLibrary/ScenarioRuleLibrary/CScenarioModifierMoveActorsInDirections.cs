using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierMoveActorsInDirections : CScenarioModifier
{
	private List<CActor> m_ActorsMovedInRound = new List<CActor>();

	public List<ScenarioManager.EAdjacentPosition> MoveDirections { get; set; }

	public int NextMoveDirectionIndex { get; set; }

	public bool ShouldTakeDamageIfMovementBlocked { get; set; }

	public int DamageToTake { get; set; }

	public bool PreventMovementIfBehindObstacle { get; set; }

	public string MapGuid { get; set; }

	public CMap MapToMoveIn
	{
		get
		{
			if (!string.IsNullOrEmpty(MapGuid))
			{
				return ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.MapGuid == MapGuid);
			}
			return null;
		}
	}

	public CScenarioModifierMoveActorsInDirections()
	{
	}

	public CScenarioModifierMoveActorsInDirections(CScenarioModifierMoveActorsInDirections state, ReferenceDictionary references)
		: base(state, references)
	{
		MoveDirections = references.Get(state.MoveDirections);
		if (MoveDirections == null && state.MoveDirections != null)
		{
			MoveDirections = new List<ScenarioManager.EAdjacentPosition>();
			for (int i = 0; i < state.MoveDirections.Count; i++)
			{
				ScenarioManager.EAdjacentPosition item = state.MoveDirections[i];
				MoveDirections.Add(item);
			}
			references.Add(state.MoveDirections, MoveDirections);
		}
		NextMoveDirectionIndex = state.NextMoveDirectionIndex;
		ShouldTakeDamageIfMovementBlocked = state.ShouldTakeDamageIfMovementBlocked;
		DamageToTake = state.DamageToTake;
		PreventMovementIfBehindObstacle = state.PreventMovementIfBehindObstacle;
		MapGuid = state.MapGuid;
		m_ActorsMovedInRound = references.Get(state.m_ActorsMovedInRound);
		if (m_ActorsMovedInRound != null || state.m_ActorsMovedInRound == null)
		{
			return;
		}
		m_ActorsMovedInRound = new List<CActor>();
		for (int j = 0; j < state.m_ActorsMovedInRound.Count; j++)
		{
			CActor cActor = state.m_ActorsMovedInRound[j];
			CActor cActor2 = references.Get(cActor);
			if (cActor2 == null && cActor != null)
			{
				CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
				cActor2 = cActor3;
				references.Add(cActor, cActor2);
			}
			m_ActorsMovedInRound.Add(cActor2);
		}
		references.Add(state.m_ActorsMovedInRound, m_ActorsMovedInRound);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MoveDirections", MoveDirections);
		info.AddValue("NextMoveDirectionIndex", NextMoveDirectionIndex);
		info.AddValue("ShouldTakeDamageIfMovementBlocked", ShouldTakeDamageIfMovementBlocked);
		info.AddValue("DamageToTake", DamageToTake);
		info.AddValue("PreventMovementIfBehindObstacle", PreventMovementIfBehindObstacle);
		info.AddValue("MapGuid", MapGuid);
	}

	public CScenarioModifierMoveActorsInDirections(SerializationInfo info, StreamingContext context)
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
				case "MoveDirections":
					MoveDirections = (List<ScenarioManager.EAdjacentPosition>)info.GetValue("MoveDirections", typeof(List<ScenarioManager.EAdjacentPosition>));
					break;
				case "NextMoveDirectionIndex":
					NextMoveDirectionIndex = info.GetInt32("NextMoveDirectionIndex");
					break;
				case "ShouldTakeDamageIfMovementBlocked":
					ShouldTakeDamageIfMovementBlocked = info.GetBoolean("ShouldTakeDamageIfMovementBlocked");
					break;
				case "DamageToTake":
					DamageToTake = info.GetInt32("DamageToTake");
					break;
				case "PreventMovementIfBehindObstacle":
					PreventMovementIfBehindObstacle = info.GetBoolean("PreventMovementIfBehindObstacle");
					break;
				case "MapGuid":
					MapGuid = info.GetString("MapGuid");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierMoveActorsInDirections entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierMoveActorsInDirections(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<ScenarioManager.EAdjacentPosition> moveDirections, bool shouldTakeDamageIfMovementBlocked, int damageToTake, bool preventMovementIfBehindObstacle, string mapGUID, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.MoveActorsInDirection, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		NextMoveDirectionIndex = 0;
		MoveDirections = moveDirections;
		ShouldTakeDamageIfMovementBlocked = shouldTakeDamageIfMovementBlocked;
		DamageToTake = damageToTake;
		PreventMovementIfBehindObstacle = preventMovementIfBehindObstacle;
		MapGuid = mapGUID;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		m_ActorsMovedInRound.Clear();
		int num = 0;
		do
		{
			num = m_ActorsMovedInRound.Count;
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				if (!m_ActorsMovedInRound.Contains(allAliveActor))
				{
					PerformScenarioModifier(roundCount, allAliveActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
				}
			}
		}
		while (num < m_ActorsMovedInRound.Count);
		if (base.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound || base.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			NextMoveDirectionIndex++;
			if (NextMoveDirectionIndex > MoveDirections.Count - 1)
			{
				NextMoveDirectionIndex = 0;
			}
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || currentActor == null || (!base.IsPositive && CScenarioModifier.IgnoreNegativeScenarioEffects(currentActor)) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || currentActor.Tokens.HasKey(CCondition.EPositiveCondition.Immovable) || base.AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = true;
		if (base.ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && !base.ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = false;
			}
			if (MapToMoveIn != null)
			{
				CTile cTile = ScenarioManager.Tiles[currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y];
				if (cTile.m_HexMap.MapGuid != MapToMoveIn.MapGuid && (cTile.m_Hex2Map == null || cTile.m_Hex2Map.MapGuid != MapToMoveIn.MapGuid))
				{
					flag = false;
				}
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
		ScenarioManager.EAdjacentPosition eAdjacentPosition = MoveDirections[NextMoveDirectionIndex];
		if (base.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && base.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			NextMoveDirectionIndex++;
			if (NextMoveDirectionIndex > MoveDirections.Count - 1)
			{
				NextMoveDirectionIndex = 0;
			}
		}
		CTile propTile = null;
		if (PreventMovementIfBehindObstacle)
		{
			ScenarioManager.EAdjacentPosition oppositeDirection = ScenarioManager.GetOppositeDirection(eAdjacentPosition);
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y, oppositeDirection);
			if (adjacentTile != null && (!ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable || CObjectProp.FindPropWithPathingBlocker(adjacentTile.m_ArrayIndex, ref propTile) != null))
			{
				return;
			}
		}
		CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y, eAdjacentPosition);
		CObjectDoor cObjectDoor = null;
		if (adjacentTile2 != null)
		{
			cObjectDoor = (CObjectDoor)adjacentTile2.FindProp(ScenarioManager.ObjectImportType.Door);
		}
		if (adjacentTile2 != null && ScenarioManager.PathFinder.Nodes[adjacentTile2.m_ArrayIndex.X, adjacentTile2.m_ArrayIndex.Y].Walkable && CObjectProp.FindPropWithPathingBlocker(adjacentTile2.m_ArrayIndex, ref propTile) == null && (cObjectDoor == null || !cObjectDoor.DoorIsLocked) && ScenarioManager.Scenario.FindActorAt(adjacentTile2.m_ArrayIndex) == null)
		{
			GameState.LostAdjacency(currentActor, currentActor.ArrayIndex, adjacentTile2.m_ArrayIndex);
			currentActor.ArrayIndex = adjacentTile2.m_ArrayIndex;
			CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(currentActor);
			cActorHasPushed_MessageData.m_PushAbility = null;
			cActorHasPushed_MessageData.m_ActorBeingPushed = currentActor;
			ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
			CTile cTile2 = adjacentTile2;
			for (int num = cTile2.m_Props.Count - 1; num >= 0; num--)
			{
				if (cTile2.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.Door && (cTile2.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.PressurePlate || currentActor.Class is CCharacterClass))
				{
					cTile2.m_Props[num].AutomaticActivate(currentActor);
				}
			}
			m_ActorsMovedInRound.Add(currentActor);
			{
				foreach (CActor item in GameState.GetActorsInRange(currentActor, currentActor, 1, new List<CActor> { currentActor }, CAbilityFilterContainer.CreateDefaultFilter(), null, null, isTargetedAbility: false, null, true))
				{
					if (item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) && !CActor.AreActorsAllied(currentActor.Type, item.Type))
					{
						item.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep);
						CActorAwakened_MessageData message = new CActorAwakened_MessageData(item);
						ScenarioRuleClient.MessageHandler(message);
					}
				}
				return;
			}
		}
		if (ShouldTakeDamageIfMovementBlocked)
		{
			int health = currentActor.Health;
			bool actorWasAsleep = currentActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			GameState.ActorBeenDamaged(currentActor, DamageToTake, checkIfPlayerCanAvoidDamage: true);
			if (ScenarioManager.Scenario.HasActor(currentActor) && GameState.ActorHealthCheck(null, currentActor, out var _, isTrap: false, isTerrain: false, actorWasAsleep))
			{
				CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(null);
				cActorBeenDamaged_MessageData.m_ActorBeingDamaged = currentActor;
				cActorBeenDamaged_MessageData.m_DamageAbility = null;
				cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
				cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
				ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
			}
		}
	}
}
