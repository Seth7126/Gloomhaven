using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierMovePropsToNearestPlayer : CScenarioModifier
{
	public CScenarioModifierMovePropsToNearestPlayer()
	{
	}

	public CScenarioModifierMovePropsToNearestPlayer(CScenarioModifierMovePropsToNearestPlayer state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CScenarioModifierMovePropsToNearestPlayer(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_ = enumerator.Current;
		}
	}

	public CScenarioModifierMovePropsToNearestPlayer(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.MovePropsToNearestPlayer, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.ToList())
		{
			PerformScenarioModifier(roundCount, item, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CObjectProp currentProp = null, int partySize = 2, bool forceActivate = false)
	{
		if (!currentProp.StartingMap.Revealed || !IsActiveInRound(roundCount, forceActivate) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce) || base.AppliedToPropGUIDs.Contains(currentProp.PropGuid))
		{
			return;
		}
		bool flag = true;
		if (base.ScenarioModifierFilter != null)
		{
			flag = base.ScenarioModifierFilter.IsValidProp(currentProp);
		}
		if (!flag)
		{
			return;
		}
		if (base.ApplyToEachActorOnce)
		{
			base.AppliedToPropGUIDs.Add(currentProp.PropGuid);
		}
		base.HasBeenAppliedOnce = true;
		CTile cTile = ScenarioManager.Tiles[currentProp.ArrayIndex.X, currentProp.ArrayIndex.Y];
		List<Point> list = null;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			bool foundPath = false;
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(cTile.m_ArrayIndex, playerActor.ArrayIndex, ignoreBlocked: false, ignoreMoveCost: true, out foundPath);
			if (foundPath && (list == null || list2.Count < list.Count))
			{
				list = list2.ToList();
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		Point point = list[0];
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(point.X, point.Y, ScenarioManager.EAdjacentPosition.ECenter);
		CTile propTile = null;
		if (adjacentTile == null || !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable || CObjectProp.FindPropWithPathingBlocker(adjacentTile.m_ArrayIndex, ref propTile) != null)
		{
			return;
		}
		cTile.m_Props.Remove(currentProp);
		currentProp.SetLocation(new TileIndex(point.X, point.Y), null, currentProp.Rotation);
		currentProp.StartingMapGuid = adjacentTile.m_HexMap.MapGuid;
		adjacentTile.m_Props.Add(currentProp);
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = currentProp,
			m_MoveToTile = adjacentTile,
			m_MoveSpeed = 2f
		};
		ScenarioRuleClient.MessageHandler(message);
		foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
		{
			currentProp.Activate(item);
			currentProp.DestroyProp();
		}
	}
}
