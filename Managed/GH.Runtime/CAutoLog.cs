using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;

[Serializable]
public class CAutoLog : ISerializable
{
	public const int c_Version = 1;

	public int Version;

	public List<CAuto> Events;

	public bool ContainsTypedChoreographerSteps;

	private int _NextEventID;

	public int NextEventID
	{
		get
		{
			_NextEventID++;
			return _NextEventID;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Version", Version);
		info.AddValue("Events", Events);
		info.AddValue("_NextEventID", _NextEventID);
		info.AddValue("ContainsTypedChoreographerSteps", ContainsTypedChoreographerSteps);
	}

	public CAutoLog(SerializationInfo info, StreamingContext context)
	{
		ContainsTypedChoreographerSteps = false;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Version":
					Version = info.GetInt32("Version");
					break;
				case "Events":
					Events = (List<CAuto>)info.GetValue("Events", typeof(List<CAuto>));
					break;
				case "_NextEventID":
					_NextEventID = info.GetInt32("_NextEventID");
					break;
				case "ContainsTypedChoreographerSteps":
					ContainsTypedChoreographerSteps = info.GetBoolean("ContainsTypedChoreographerSteps");
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize CAutoLog entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAutoLog()
	{
		Version = 1;
		_NextEventID = 0;
		Events = new List<CAuto>();
		ContainsTypedChoreographerSteps = true;
	}
}
