using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Client;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhaseInHQ : CMapPhase, ISerializable
{
	public CMapPhaseInHQ(CMapPhaseInHQ state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CMapPhaseInHQ(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_ = enumerator.Current;
		}
	}

	public CMapPhaseInHQ()
		: base(EMapPhaseType.InHQ)
	{
	}

	protected override void OnNextStep()
	{
		CInHQ_MapClientMessage message = new CInHQ_MapClientMessage();
		MapRuleLibraryClient.Instance.MessageHandler(message);
	}
}
