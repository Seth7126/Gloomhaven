using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLog : ISerializable
{
	public List<SEvent> Events { get; private set; }

	public SEventLog(SEventLog state, ReferenceDictionary references)
	{
		Events = references.Get(state.Events);
		if (Events != null || state.Events == null)
		{
			return;
		}
		Events = new List<SEvent>();
		for (int i = 0; i < state.Events.Count; i++)
		{
			SEvent sEvent = state.Events[i];
			SEvent sEvent2 = references.Get(sEvent);
			if (sEvent2 == null && sEvent != null)
			{
				SEvent sEvent3 = ((sEvent is SEventAbilityAddModifierToMonsterDeck state2) ? new SEventAbilityAddModifierToMonsterDeck(state2, references) : ((sEvent is SEventAbilityAttack state3) ? new SEventAbilityAttack(state3, references) : ((sEvent is SEventAbilityCondition state4) ? new SEventAbilityCondition(state4, references) : ((sEvent is SEventAbilityCreate state5) ? new SEventAbilityCreate(state5, references) : ((sEvent is SEventAbilityDamage state6) ? new SEventAbilityDamage(state6, references) : ((sEvent is SEventAbilityDeactivateSpawner state7) ? new SEventAbilityDeactivateSpawner(state7, references) : ((sEvent is SEventAbilityDestroyObstacle state8) ? new SEventAbilityDestroyObstacle(state8, references) : ((sEvent is SEventAbilityDisarmTrap state9) ? new SEventAbilityDisarmTrap(state9, references) : ((sEvent is SEventAbilityFear state10) ? new SEventAbilityFear(state10, references) : ((sEvent is SEventAbilityInfuse state11) ? new SEventAbilityInfuse(state11, references) : ((sEvent is SEventAbilityLoot state12) ? new SEventAbilityLoot(state12, references) : ((sEvent is SEventAbilityMove state13) ? new SEventAbilityMove(state13, references) : ((sEvent is SEventAbilityMoveObstacle state14) ? new SEventAbilityMoveObstacle(state14, references) : ((sEvent is SEventAbilityNull state15) ? new SEventAbilityNull(state15, references) : ((sEvent is SEventAbilityPull state16) ? new SEventAbilityPull(state16, references) : ((sEvent is SEventAbilityPush state17) ? new SEventAbilityPush(state17, references) : ((sEvent is SEventAbilityRedistributeDamage state18) ? new SEventAbilityRedistributeDamage(state18, references) : ((sEvent is SEventAbilitySummon state19) ? new SEventAbilitySummon(state19, references) : ((sEvent is SEventAbilitySwap state20) ? new SEventAbilitySwap(state20, references) : ((sEvent is SEventAbilityTargeting state21) ? new SEventAbilityTargeting(state21, references) : ((sEvent is SEventAbilityTeleport state22) ? new SEventAbilityTeleport(state22, references) : ((sEvent is SEventAbilityTrap state23) ? new SEventAbilityTrap(state23, references) : ((sEvent is SEventActorDamaged state24) ? new SEventActorDamaged(state24, references) : ((sEvent is SEventActorEarnedAbilityXP state25) ? new SEventActorEarnedAbilityXP(state25, references) : ((sEvent is SEventActorEndTurn state26) ? new SEventActorEndTurn(state26, references) : ((sEvent is SEventActorFinishedScenario state27) ? new SEventActorFinishedScenario(state27, references) : ((sEvent is SEventActorHealed state28) ? new SEventActorHealed(state28, references) : ((sEvent is SEventActorLooted state29) ? new SEventActorLooted(state29, references) : ((sEvent is SEventActorUsedItem state30) ? new SEventActorUsedItem(state30, references) : ((sEvent is SEventObjectPropChest state31) ? new SEventObjectPropChest(state31, references) : ((sEvent is SEventObjectPropTrap state32) ? new SEventObjectPropTrap(state32, references) : ((sEvent is SEventAbility state33) ? new SEventAbility(state33, references) : ((sEvent is SEventAction state34) ? new SEventAction(state34, references) : ((sEvent is SEventActor state35) ? new SEventActor(state35, references) : ((sEvent is SEventAttackModifier state36) ? new SEventAttackModifier(state36, references) : ((sEvent is SEventDiscardCard state37) ? new SEventDiscardCard(state37, references) : ((sEvent is SEventDonate state38) ? new SEventDonate(state38, references) : ((sEvent is SEventElement state39) ? new SEventElement(state39, references) : ((sEvent is SEventEnhancement state40) ? new SEventEnhancement(state40, references) : ((sEvent is SEventHand state41) ? new SEventHand(state41, references) : ((sEvent is SEventInternal state42) ? new SEventInternal(state42, references) : ((sEvent is SEventItem state43) ? new SEventItem(state43, references) : ((sEvent is SEventLoseCard state44) ? new SEventLoseCard(state44, references) : ((sEvent is SEventLostAdjacency state45) ? new SEventLostAdjacency(state45, references) : ((sEvent is SEventObjectProp state46) ? new SEventObjectProp(state46, references) : ((sEvent is SEventPerk state47) ? new SEventPerk(state47, references) : ((sEvent is SEventPersonalQuest state48) ? new SEventPersonalQuest(state48, references) : ((sEvent is SEventPhase state49) ? new SEventPhase(state49, references) : ((!(sEvent is SEventRound state50)) ? new SEvent(sEvent, references) : new SEventRound(state50, references))))))))))))))))))))))))))))))))))))))))))))))))));
				sEvent2 = sEvent3;
				references.Add(sEvent, sEvent2);
			}
			Events.Add(sEvent2);
		}
		references.Add(state.Events, Events);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Events", (Events != null) ? Events.Where((SEvent w) => w != null && !w.m_DoNotSerialize).ToList() : null);
	}

	public SEventLog(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "Events")
				{
					Events = (List<SEvent>)info.GetValue("Events", typeof(List<SEvent>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventLog entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Events == null)
		{
			Events = new List<SEvent>
			{
				new SEventInternal(ESESubTypeInternal.Start)
			};
		}
	}

	public SEventLog()
	{
		Events = new List<SEvent>
		{
			new SEventInternal(ESESubTypeInternal.Start)
		};
	}

	public void AddEvent(SEvent eventToAdd)
	{
		lock (Events)
		{
			Events.Add(eventToAdd);
		}
	}

	public void WriteToFile(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			using StreamWriter streamWriter = new StreamWriter(path);
			foreach (SEvent @event in Events)
			{
				if (!(@event is SEventInternal))
				{
					if (@event is SEventPhase)
					{
					}
				}
				else
				{
					streamWriter.WriteLine();
				}
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("An exception ocurred while attempting to write the scenario event log to file " + path + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static List<SEvent> FindLoggedEvents(ESEType eventType)
	{
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEvent> result;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			result = (from x in ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList()
				where x.Type.Equals(eventType)
				select x).ToList();
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEvent FindLastEventOfType(ESEType eventType)
	{
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		SEvent result;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			result = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().LastOrDefault((SEvent x) => x.Type.Equals(eventType));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAbility FindLastAbilityEventOfAbilityType(CAbility.EAbilityType abilityType, ESESubTypeAbility subType = ESESubTypeAbility.None, bool checkQueue = false)
	{
		SEventAbility result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAbility> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAbility>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAbility> queuedAbilities = SEventLogMessageHandler.GetQueuedAbilities();
			list.AddRange(queuedAbilities);
		}
		if (list.Count > 0)
		{
			result = ((subType == ESESubTypeAbility.None) ? list.LastOrDefault((SEventAbility x) => x.AbilityType.Equals(abilityType)) : list.LastOrDefault((SEventAbility x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType)));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAbilityCondition FindLastConditionAbilityEventOfAbilityType(CAbility.EAbilityType abilityType, ESESubTypeAbility subType = ESESubTypeAbility.None, bool checkQueue = false)
	{
		SEventAbilityCondition result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAbilityCondition> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAbilityCondition>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAbilityCondition> queuedConditions = SEventLogMessageHandler.GetQueuedConditions();
			list.AddRange(queuedConditions);
		}
		if (list.Count > 0)
		{
			result = ((subType == ESESubTypeAbility.None) ? list.LastOrDefault((SEventAbilityCondition x) => x.AbilityType.Equals(abilityType)) : list.LastOrDefault((SEventAbilityCondition x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType)));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAction FindLastActionEventOfSubType(ESESubTypeAction subTypeAction)
	{
		SEventAction result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAction> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAction>().ToList();
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventAction x) => x.SubTypeAction.Equals(subTypeAction));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventObjectProp FindLastObjectEventOfSubTypeAndObjectType(ESESubTypeObjectProp subTypeObjectProp, ScenarioManager.ObjectImportType objectType)
	{
		SEventObjectProp result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventObjectProp> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventObjectProp>().ToList();
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventObjectProp x) => x.ObjectPropSubType.Equals(subTypeObjectProp) && x.ObjectType.Equals(objectType));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static SEventAttackModifier FindLastAttackModifierEventWithID(int actorId, string actorClass, bool checkQueue = false, int attackIndex = 0)
	{
		SEventAttackModifier result = null;
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<SEventAttackModifier> list;
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAttackModifier>().ToList();
		}
		if (checkQueue)
		{
			List<SEventAttackModifier> queuedModifiers = SEventLogMessageHandler.GetQueuedModifiers();
			list.AddRange(queuedModifiers);
		}
		if (list.Count > 0)
		{
			result = list.LastOrDefault((SEventAttackModifier x) => x.TargetActorID.Equals(actorId) && x.ActedOnByClass.Equals(actorClass) && x.AttackIndex.Equals(attackIndex));
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return result;
	}

	public static List<SEventActor> FindAllActorEventsOfSubTypeAndActorType(ESESubTypeActor subType, CActor.EType actorType)
	{
		List<SEventActor> list = new List<SEventActor>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventActor>().ToList();
		}
		list = list.FindAll((SEventActor x) => x.ActorSubType.Equals(subType) && x.ActorType.Equals(actorType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventActor> FindAllActorEventsOfSubType(ESESubTypeActor subType)
	{
		List<SEventActor> list = new List<SEventActor>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventActor>().ToList();
		}
		list = list.FindAll((SEventActor x) => x.ActorSubType.Equals(subType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfType(CAbility.EAbilityType abilityType)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAbility>().ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilityType.Equals(abilityType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfTypeAndSubType(CAbility.EAbilityType abilityType, ESESubTypeAbility subType)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAbility>().ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilityType.Equals(abilityType) && x.AbilitySubType.Equals(subType));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<SEventAbility> FindAllAbilityEventsOfSubType(ESESubTypeAbility subType, bool excludeTargeting = false)
	{
		List<SEventAbility> list = new List<SEventAbility>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		List<string> excluder = new List<string>
		{
			"Advantage", "Bless", "Curse", "Disarm", "Immobilize", "Invisible", "Muddle", "Poison", "Strengthen", "Stun",
			"Wound"
		};
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = (from x in ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<SEventAbility>()
				where x.GetType() != typeof(SEventAbilityCondition)
				select x).ToList();
		}
		list = list.FindAll((SEventAbility x) => x.AbilitySubType.Equals(subType) && (!excludeTargeting || !excluder.Contains(x.AbilityType.ToString())));
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static List<T> FindAllEventsOfType<T>(bool checkQueue = false) where T : SEvent
	{
		List<T> list = new List<T>();
		SEventLogMessageHandler.ToggleEventLogProcessing(process: false);
		lock (ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events)
		{
			list = ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.ToList().OfType<T>().ToList();
		}
		if (checkQueue)
		{
			List<T> queuedEvents = SEventLogMessageHandler.GetQueuedEvents<T>();
			list.AddRange(queuedEvents);
		}
		SEventLogMessageHandler.ToggleEventLogProcessing(process: true);
		return list;
	}

	public static void ClearEventLog()
	{
		if (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.Events != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Clear();
		}
	}
}
