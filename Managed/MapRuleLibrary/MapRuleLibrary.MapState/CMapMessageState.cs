using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Message;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CMapMessageState : ISerializable
{
	[Serializable]
	public enum EMapMessageState
	{
		None,
		Locked,
		Unlocked,
		Shown
	}

	private CMapMessage m_CachedQuest;

	public bool IsInitialised { get; private set; }

	public string ID { get; private set; }

	public EMapMessageState MapMessageState { get; private set; }

	public CUnlockConditionState UnlockConditionState { get; private set; }

	public CMapMessage MapMessage
	{
		get
		{
			if (m_CachedQuest == null)
			{
				m_CachedQuest = MapRuleLibraryClient.MRLYML.MapMessages.SingleOrDefault((CMapMessage s) => s.MessageID == ID);
			}
			return m_CachedQuest;
		}
	}

	public CMapMessageState()
	{
	}

	public CMapMessageState(CMapMessageState state, ReferenceDictionary references)
	{
		IsInitialised = state.IsInitialised;
		ID = state.ID;
		MapMessageState = state.MapMessageState;
		UnlockConditionState = references.Get(state.UnlockConditionState);
		if (UnlockConditionState == null && state.UnlockConditionState != null)
		{
			UnlockConditionState = new CUnlockConditionState(state.UnlockConditionState, references);
			references.Add(state.UnlockConditionState, UnlockConditionState);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("ID", ID);
		info.AddValue("MapMessageState", MapMessageState);
		info.AddValue("UnlockConditionState", UnlockConditionState);
	}

	public CMapMessageState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "ID":
					ID = info.GetString("ID");
					break;
				case "MapMessageState":
					MapMessageState = (EMapMessageState)info.GetValue("MapMessageState", typeof(EMapMessageState));
					break;
				case "UnlockConditionState":
					UnlockConditionState = (CUnlockConditionState)info.GetValue("UnlockConditionState", typeof(CUnlockConditionState));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapMessageState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
	}

	public CMapMessageState(CMapMessage mapMessage)
	{
		ID = mapMessage.MessageID;
		MapMessageState = EMapMessageState.Locked;
		UnlockConditionState = new CUnlockConditionState(MapMessage.UnlockCondition);
		IsInitialised = false;
	}

	public void Init()
	{
		if (!IsInitialised)
		{
			IsInitialised = true;
		}
	}

	public void OnMapStateAdventureStarted()
	{
		if (MapMessage != null)
		{
			if (UnlockConditionState == null)
			{
				UnlockConditionState = new CUnlockConditionState(MapMessage.UnlockCondition);
			}
			UnlockConditionState.CacheUnlockCondition(MapMessage.UnlockCondition);
		}
	}

	public void UnlockMapMessage()
	{
		MapMessageState = EMapMessageState.Unlocked;
	}

	public void MapMessageShown()
	{
		MapMessageState = EMapMessageState.Shown;
	}
}
