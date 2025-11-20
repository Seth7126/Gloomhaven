using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierMovePropsInSequence : CScenarioModifier
{
	public List<TileIndex> SequenceTiles { get; set; }

	public int NextTileIndex { get; set; }

	public CScenarioModifierMovePropsInSequence()
	{
	}

	public CScenarioModifierMovePropsInSequence(CScenarioModifierMovePropsInSequence state, ReferenceDictionary references)
		: base(state, references)
	{
		SequenceTiles = references.Get(state.SequenceTiles);
		if (SequenceTiles == null && state.SequenceTiles != null)
		{
			SequenceTiles = new List<TileIndex>();
			for (int i = 0; i < state.SequenceTiles.Count; i++)
			{
				TileIndex tileIndex = state.SequenceTiles[i];
				TileIndex tileIndex2 = references.Get(tileIndex);
				if (tileIndex2 == null && tileIndex != null)
				{
					tileIndex2 = new TileIndex(tileIndex, references);
					references.Add(tileIndex, tileIndex2);
				}
				SequenceTiles.Add(tileIndex2);
			}
			references.Add(state.SequenceTiles, SequenceTiles);
		}
		NextTileIndex = state.NextTileIndex;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SequenceTiles", SequenceTiles);
		info.AddValue("NextTileIndex", NextTileIndex);
	}

	public CScenarioModifierMovePropsInSequence(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "SequenceTiles"))
				{
					if (name == "NextTileIndex")
					{
						NextTileIndex = info.GetInt32("NextTileIndex");
					}
				}
				else
				{
					SequenceTiles = (List<TileIndex>)info.GetValue("SequenceTiles", typeof(List<TileIndex>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierMoveTrapsInSequence entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierMovePropsInSequence(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<TileIndex> sequenceTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.MovePropsInSequence, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		SequenceTiles = sequenceTiles;
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
		if (base.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound || base.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			NextTileIndex++;
			if (NextTileIndex > SequenceTiles.Count - 1)
			{
				NextTileIndex = 0;
			}
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
		TileIndex tileIndex = SequenceTiles[NextTileIndex];
		if (base.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && base.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			NextTileIndex++;
			if (NextTileIndex > SequenceTiles.Count - 1)
			{
				NextTileIndex = 0;
			}
		}
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(tileIndex.X, tileIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
		CTile propTile = null;
		if (adjacentTile == null || !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable || CObjectProp.FindPropWithPathingBlocker(adjacentTile.m_ArrayIndex, ref propTile) != null)
		{
			return;
		}
		ScenarioManager.Tiles[currentProp.ArrayIndex.X, currentProp.ArrayIndex.Y].m_Props.Remove(currentProp);
		currentProp.SetLocation(tileIndex, null, currentProp.Rotation);
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
