using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhaseRoadEvent : CMapPhase, ISerializable
{
	public CLocationState EndLocation { get; set; }

	public CMapPhaseRoadEvent(CMapPhaseRoadEvent state, ReferenceDictionary references)
		: base(state, references)
	{
		EndLocation = references.Get(state.EndLocation);
		if (EndLocation == null && state.EndLocation != null)
		{
			EndLocation = new CLocationState(state.EndLocation, references);
			references.Add(state.EndLocation, EndLocation);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CMapPhaseRoadEvent(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_ = enumerator.Current;
		}
	}

	public CMapPhaseRoadEvent(CLocationState endLocation = null)
		: base(EMapPhaseType.RoadEvent)
	{
		EndLocation = endLocation;
	}

	protected override void OnNextStep()
	{
		CShowRoadEvent_MapClientMessage message = new CShowRoadEvent_MapClientMessage();
		MapRuleLibraryClient.Instance.MessageHandler(message);
	}
}
