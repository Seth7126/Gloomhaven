using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelTrigger : ISerializable
{
	public enum ELevelMessagePredefinedDisplayTrigger
	{
		ScenarioStart,
		ScenarioEnd,
		RoomRevealed
	}

	public bool IsTriggeredByDismiss { get; set; }

	public bool IsUIEventTypeTrigger { get; set; }

	public int EventTriggerTypeInt { get; set; }

	public int EventTriggerSubTypeInt { get; set; }

	public string EventTriggerActorName { get; set; }

	public string EventTriggerActorGuid { get; set; }

	public int EventTriggerContextTypeInt { get; set; }

	public int EventTriggerContextSubTypeInt { get; set; }

	public int EventTriggerRound { get; set; }

	public string EventTriggerContextId { get; set; }

	public int EventTriggerContextIndex { get; set; }

	public string EventTriggerPlayedMessageReq { get; set; }

	public string EventTriggerNotPlayedMessageReq { get; set; }

	public bool EventTriggerPlayedMessageReqOR { get; set; }

	public bool EventTriggerNotPlayedMessageReqOR { get; set; }

	public TileIndex EventTriggerTile { get; set; }

	public CObjectiveFilter EventFilter { get; set; }

	public string EventAliveActorGUID { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsTriggeredByDismiss", IsTriggeredByDismiss);
		info.AddValue("IsUIEventTypeTrigger", IsUIEventTypeTrigger);
		info.AddValue("EventTriggerTypeInt", EventTriggerTypeInt);
		info.AddValue("EventTriggerSubTypeInt", EventTriggerSubTypeInt);
		info.AddValue("EventTriggerActorName", EventTriggerActorName);
		info.AddValue("EventTriggerActorGuid", EventTriggerActorGuid);
		info.AddValue("EventTriggerContextTypeInt", EventTriggerContextTypeInt);
		info.AddValue("EventTriggerContextSubTypeInt", EventTriggerContextSubTypeInt);
		info.AddValue("EventTriggerRound", EventTriggerRound);
		info.AddValue("EventTriggerContextId", EventTriggerContextId);
		info.AddValue("EventTriggerContextIndex", EventTriggerContextIndex);
		info.AddValue("EventTriggerPlayedMessageReq", EventTriggerPlayedMessageReq);
		info.AddValue("EventTriggerNotPlayedMessageReq", EventTriggerNotPlayedMessageReq);
		info.AddValue("EventTriggerPlayedMessageReqOR", EventTriggerPlayedMessageReqOR);
		info.AddValue("EventTriggerNotPlayedMessageReqOR", EventTriggerNotPlayedMessageReqOR);
		info.AddValue("EventTriggerTile", EventTriggerTile);
		info.AddValue("EventFilter", EventFilter);
		info.AddValue("EventAliveActorGUID", EventAliveActorGUID);
	}

	public CLevelTrigger(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsTriggeredByDismiss":
					IsTriggeredByDismiss = info.GetBoolean("IsTriggeredByDismiss");
					break;
				case "IsUIEventTypeTrigger":
					IsUIEventTypeTrigger = info.GetBoolean("IsUIEventTypeTrigger");
					break;
				case "EventTriggerType":
					EventTriggerTypeInt = (int)info.GetValue("EventTriggerType", typeof(ESEType));
					break;
				case "EventTriggerTypeInt":
					EventTriggerTypeInt = info.GetInt32("EventTriggerTypeInt");
					break;
				case "EventTriggerSubTypeInt":
					EventTriggerSubTypeInt = info.GetInt32("EventTriggerSubTypeInt");
					break;
				case "EventTriggerActorName":
					EventTriggerActorName = info.GetString("EventTriggerActorName");
					break;
				case "EventTriggerActorGuid":
					EventTriggerActorGuid = info.GetString("EventTriggerActorGuid");
					break;
				case "EventTriggerContextTypeInt":
					EventTriggerContextTypeInt = info.GetInt32("EventTriggerContextTypeInt");
					break;
				case "EventTriggerContextSubTypeInt":
					EventTriggerContextSubTypeInt = info.GetInt32("EventTriggerContextSubTypeInt");
					break;
				case "EventTriggerRound":
					EventTriggerRound = info.GetInt32("EventTriggerRound");
					break;
				case "EventTriggerContextId":
					EventTriggerContextId = info.GetString("EventTriggerContextId");
					break;
				case "EventTriggerContextIndex":
					EventTriggerContextIndex = info.GetInt32("EventTriggerContextIndex");
					break;
				case "EventTriggerPlayedMessageReq":
					EventTriggerPlayedMessageReq = info.GetString("EventTriggerPlayedMessageReq");
					break;
				case "EventTriggerNotPlayedMessageReq":
					EventTriggerNotPlayedMessageReq = info.GetString("EventTriggerNotPlayedMessageReq");
					break;
				case "EventTriggerPlayedMessageReqOR":
					EventTriggerPlayedMessageReqOR = info.GetBoolean("EventTriggerPlayedMessageReqOR");
					break;
				case "EventTriggerNotPlayedMessageReqOR":
					EventTriggerNotPlayedMessageReqOR = info.GetBoolean("EventTriggerNotPlayedMessageReqOR");
					break;
				case "EventTriggerTile":
					EventTriggerTile = (TileIndex)info.GetValue("EventTriggerTile", typeof(TileIndex));
					break;
				case "EventFilter":
					EventFilter = (CObjectiveFilter)info.GetValue("EventFilter", typeof(CObjectiveFilter));
					break;
				case "EventAliveActorGUID":
					EventAliveActorGUID = info.GetString("EventAliveActorGUID");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CLevelTrigger entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelTrigger()
	{
		IsTriggeredByDismiss = false;
		IsUIEventTypeTrigger = false;
		EventTriggerTypeInt = 0;
		EventTriggerSubTypeInt = 0;
		EventTriggerActorName = string.Empty;
		EventTriggerActorGuid = string.Empty;
		EventTriggerContextTypeInt = 0;
		EventTriggerContextSubTypeInt = 0;
		EventTriggerRound = 0;
		EventTriggerContextId = string.Empty;
		EventTriggerContextIndex = -1;
		EventTriggerPlayedMessageReq = string.Empty;
		EventTriggerNotPlayedMessageReq = string.Empty;
		EventTriggerPlayedMessageReqOR = false;
		EventTriggerNotPlayedMessageReqOR = false;
		EventTriggerTile = null;
		EventFilter = new CObjectiveFilter();
		EventAliveActorGUID = string.Empty;
	}

	public CLevelTrigger(ELevelMessagePredefinedDisplayTrigger predefinedDisplayTriggerType, string mapGuid = null)
	{
		IsTriggeredByDismiss = false;
		IsUIEventTypeTrigger = false;
		EventTriggerTypeInt = 0;
		EventTriggerSubTypeInt = 0;
		EventTriggerActorName = string.Empty;
		EventTriggerActorGuid = string.Empty;
		EventTriggerContextTypeInt = 0;
		EventTriggerContextSubTypeInt = 0;
		EventTriggerRound = 0;
		EventTriggerContextId = string.Empty;
		EventTriggerContextIndex = -1;
		EventTriggerPlayedMessageReq = string.Empty;
		EventTriggerNotPlayedMessageReq = string.Empty;
		EventTriggerPlayedMessageReqOR = false;
		EventTriggerNotPlayedMessageReqOR = false;
		EventTriggerTile = null;
		EventFilter = null;
		EventAliveActorGUID = string.Empty;
		switch (predefinedDisplayTriggerType)
		{
		case ELevelMessagePredefinedDisplayTrigger.ScenarioStart:
			EventTriggerTypeInt = 2;
			EventTriggerSubTypeInt = 1;
			EventTriggerContextTypeInt = 1;
			EventTriggerRound = 1;
			break;
		case ELevelMessagePredefinedDisplayTrigger.ScenarioEnd:
			IsUIEventTypeTrigger = true;
			EventTriggerTypeInt = 30;
			EventTriggerContextTypeInt = 9;
			break;
		case ELevelMessagePredefinedDisplayTrigger.RoomRevealed:
			IsUIEventTypeTrigger = true;
			EventTriggerTypeInt = 29;
			EventTriggerContextTypeInt = 9;
			EventTriggerContextId = (string.IsNullOrEmpty(mapGuid) ? string.Empty : mapGuid);
			break;
		}
	}

	public CLevelTrigger(CLevelTrigger state, ReferenceDictionary references)
	{
		IsTriggeredByDismiss = state.IsTriggeredByDismiss;
		IsUIEventTypeTrigger = state.IsUIEventTypeTrigger;
		EventTriggerTypeInt = state.EventTriggerTypeInt;
		EventTriggerSubTypeInt = state.EventTriggerSubTypeInt;
		EventTriggerActorName = state.EventTriggerActorName;
		EventTriggerActorGuid = state.EventTriggerActorGuid;
		EventTriggerContextTypeInt = state.EventTriggerContextTypeInt;
		EventTriggerContextSubTypeInt = state.EventTriggerContextSubTypeInt;
		EventTriggerRound = state.EventTriggerRound;
		EventTriggerContextId = state.EventTriggerContextId;
		EventTriggerContextIndex = state.EventTriggerContextIndex;
		EventTriggerPlayedMessageReq = state.EventTriggerPlayedMessageReq;
		EventTriggerNotPlayedMessageReq = state.EventTriggerNotPlayedMessageReq;
		EventTriggerPlayedMessageReqOR = state.EventTriggerPlayedMessageReqOR;
		EventTriggerNotPlayedMessageReqOR = state.EventTriggerNotPlayedMessageReqOR;
		EventTriggerTile = references.Get(state.EventTriggerTile);
		if (EventTriggerTile == null && state.EventTriggerTile != null)
		{
			EventTriggerTile = new TileIndex(state.EventTriggerTile, references);
			references.Add(state.EventTriggerTile, EventTriggerTile);
		}
		EventFilter = references.Get(state.EventFilter);
		if (EventFilter == null && state.EventFilter != null)
		{
			EventFilter = new CObjectiveFilter(state.EventFilter, references);
			references.Add(state.EventFilter, EventFilter);
		}
		EventAliveActorGUID = state.EventAliveActorGUID;
	}
}
