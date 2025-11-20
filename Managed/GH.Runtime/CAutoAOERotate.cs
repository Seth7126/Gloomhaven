using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

[Serializable]
public class CAutoAOERotate : CAuto, ISerializable
{
	public bool IsClockWise;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsClockWise", IsClockWise);
	}

	protected CAutoAOERotate(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "IsClockWise")
				{
					IsClockWise = info.GetBoolean("IsClockWise");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to deserialize CAutoAOERotate entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAutoAOERotate(int id, bool clockwise)
		: base(EAutoType.AOERotate, id)
	{
		IsClockWise = clockwise;
	}
}
