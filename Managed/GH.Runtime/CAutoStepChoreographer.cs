using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;
using ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("MessageType: {ChoreographerMessageType}")]
public class CAutoStepChoreographer : CAuto, ISerializable
{
	public CMessageData.MessageType ChoreographerMessageType;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ChoreographerMessageType", ChoreographerMessageType);
	}

	protected CAutoStepChoreographer(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ChoreographerMessageType")
				{
					ChoreographerMessageType = (CMessageData.MessageType)info.GetValue("ChoreographerMessageType", typeof(CMessageData.MessageType));
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize CAutoStepChoreographer entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAutoStepChoreographer(int id, CMessageData.MessageType messageType)
		: base(EAutoType.ChoreographerStep, id)
	{
		ChoreographerMessageType = messageType;
	}
}
