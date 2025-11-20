using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Message;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhaseShowMapMessages : CMapPhase, ISerializable
{
	private EMapMessageTrigger MapMessageTrigger { get; set; }

	public CMapPhaseShowMapMessages()
	{
	}

	public CMapPhaseShowMapMessages(CMapPhaseShowMapMessages state, ReferenceDictionary references)
		: base(state, references)
	{
		MapMessageTrigger = state.MapMessageTrigger;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MapMessageTrigger", MapMessageTrigger);
	}

	public CMapPhaseShowMapMessages(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "MapMessageTrigger")
				{
					MapMessageTrigger = (EMapMessageTrigger)info.GetValue("MapMessageTrigger", typeof(EMapMessageTrigger));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapPhaseShowMapMessages entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapPhaseShowMapMessages(EMapMessageTrigger mapMessageTrigger)
		: base(EMapPhaseType.ShowMapMessages)
	{
		MapMessageTrigger = mapMessageTrigger;
	}

	protected override void OnNextStep()
	{
		List<CMapMessageState> list = new List<CMapMessageState>();
		foreach (CMapMessageState mapMessageState in AdventureState.MapState.MapMessageStates)
		{
			bool flag = false;
			if (mapMessageState.MapMessageState == CMapMessageState.EMapMessageState.Unlocked && mapMessageState.MapMessage.MapMessageTrigger == MapMessageTrigger)
			{
				flag = true;
			}
			if (flag)
			{
				list.Add(mapMessageState);
			}
		}
		CShowMapMessages_MapClientMessage message = new CShowMapMessages_MapClientMessage(MapMessageTrigger, list);
		MapRuleLibraryClient.Instance.MessageHandler(message);
	}
}
