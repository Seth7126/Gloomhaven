using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierOverrideCompanionSummonTiles : CScenarioModifier
{
	public List<TileIndex> SummonTileIndexes { get; set; }

	public CScenarioModifierOverrideCompanionSummonTiles()
	{
	}

	public CScenarioModifierOverrideCompanionSummonTiles(CScenarioModifierOverrideCompanionSummonTiles state, ReferenceDictionary references)
		: base(state, references)
	{
		SummonTileIndexes = references.Get(state.SummonTileIndexes);
		if (SummonTileIndexes != null || state.SummonTileIndexes == null)
		{
			return;
		}
		SummonTileIndexes = new List<TileIndex>();
		for (int i = 0; i < state.SummonTileIndexes.Count; i++)
		{
			TileIndex tileIndex = state.SummonTileIndexes[i];
			TileIndex tileIndex2 = references.Get(tileIndex);
			if (tileIndex2 == null && tileIndex != null)
			{
				tileIndex2 = new TileIndex(tileIndex, references);
				references.Add(tileIndex, tileIndex2);
			}
			SummonTileIndexes.Add(tileIndex2);
		}
		references.Add(state.SummonTileIndexes, SummonTileIndexes);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SummonTileIndexes", SummonTileIndexes);
	}

	public CScenarioModifierOverrideCompanionSummonTiles(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "SummonTileIndexes")
				{
					SummonTileIndexes = (List<TileIndex>)info.GetValue("SummonTileIndexes", typeof(List<TileIndex>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierOverrideCompanionSummonTiles entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierOverrideCompanionSummonTiles(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<TileIndex> summonTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.OverrideCompanionSummonTiles, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		SummonTileIndexes = summonTiles;
	}
}
