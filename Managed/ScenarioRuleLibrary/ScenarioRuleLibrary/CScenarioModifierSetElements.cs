using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierSetElements : CScenarioModifier
{
	public List<ElementInfusionBoardManager.EElement> StrongElements { get; private set; }

	public List<ElementInfusionBoardManager.EElement> WaningElements { get; private set; }

	public List<ElementInfusionBoardManager.EElement> InertElements { get; private set; }

	public int CheckRoundInterval { get; private set; }

	public int CheckRoundIntervalOffset { get; private set; }

	public CScenarioModifierSetElements()
	{
	}

	public CScenarioModifierSetElements(CScenarioModifierSetElements state, ReferenceDictionary references)
		: base(state, references)
	{
		StrongElements = references.Get(state.StrongElements);
		if (StrongElements == null && state.StrongElements != null)
		{
			StrongElements = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.StrongElements.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.StrongElements[i];
				StrongElements.Add(item);
			}
			references.Add(state.StrongElements, StrongElements);
		}
		WaningElements = references.Get(state.WaningElements);
		if (WaningElements == null && state.WaningElements != null)
		{
			WaningElements = new List<ElementInfusionBoardManager.EElement>();
			for (int j = 0; j < state.WaningElements.Count; j++)
			{
				ElementInfusionBoardManager.EElement item2 = state.WaningElements[j];
				WaningElements.Add(item2);
			}
			references.Add(state.WaningElements, WaningElements);
		}
		InertElements = references.Get(state.InertElements);
		if (InertElements == null && state.InertElements != null)
		{
			InertElements = new List<ElementInfusionBoardManager.EElement>();
			for (int k = 0; k < state.InertElements.Count; k++)
			{
				ElementInfusionBoardManager.EElement item3 = state.InertElements[k];
				InertElements.Add(item3);
			}
			references.Add(state.InertElements, InertElements);
		}
		CheckRoundInterval = state.CheckRoundInterval;
		CheckRoundIntervalOffset = state.CheckRoundIntervalOffset;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("StrongElements", StrongElements);
		info.AddValue("WaningElements", WaningElements);
		info.AddValue("InertElements", InertElements);
		info.AddValue("CheckRoundInterval", CheckRoundInterval);
		info.AddValue("CheckRoundIntervalOffset", CheckRoundIntervalOffset);
	}

	public CScenarioModifierSetElements(SerializationInfo info, StreamingContext context)
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
				case "StrongElements":
					StrongElements = (List<ElementInfusionBoardManager.EElement>)info.GetValue("StrongElements", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "WaningElements":
					WaningElements = (List<ElementInfusionBoardManager.EElement>)info.GetValue("WaningElements", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "InertElements":
					InertElements = (List<ElementInfusionBoardManager.EElement>)info.GetValue("InertElements", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "CheckRoundInterval":
					CheckRoundInterval = info.GetInt32("CheckRoundInterval");
					break;
				case "CheckRoundIntervalOffset":
					CheckRoundIntervalOffset = info.GetInt32("CheckRoundIntervalOffset");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CScenarioModifierSetElements entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierSetElements(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<ElementInfusionBoardManager.EElement> strongElements, List<ElementInfusionBoardManager.EElement> waningElements, List<ElementInfusionBoardManager.EElement> inertElements, int checkRoundInterval, int checkRoundIntervalOffset, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.SetElements, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		StrongElements = strongElements;
		WaningElements = waningElements;
		InertElements = inertElements;
		CheckRoundInterval = checkRoundInterval;
		CheckRoundIntervalOffset = checkRoundIntervalOffset;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (IsActiveInRound(roundCount, forceActivate) && (CheckRoundInterval == 0 || (ScenarioManager.CurrentScenarioState.RoundNumber + CheckRoundIntervalOffset) % CheckRoundInterval == 0))
		{
			PerformScenarioModifier(roundCount, (CActor)null, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || (base.ApplyOnceTotal && base.HasBeenAppliedOnce))
		{
			return;
		}
		base.HasBeenAppliedOnce = true;
		foreach (ElementInfusionBoardManager.EElement strongElement in StrongElements)
		{
			ElementInfusionBoardManager.SetElementInstantly(strongElement, ElementInfusionBoardManager.EColumn.Strong);
		}
		foreach (ElementInfusionBoardManager.EElement waningElement in WaningElements)
		{
			ElementInfusionBoardManager.SetElementInstantly(waningElement, ElementInfusionBoardManager.EColumn.Waning);
		}
		foreach (ElementInfusionBoardManager.EElement inertElement in InertElements)
		{
			ElementInfusionBoardManager.SetElementInstantly(inertElement, ElementInfusionBoardManager.EColumn.Inert);
		}
	}
}
