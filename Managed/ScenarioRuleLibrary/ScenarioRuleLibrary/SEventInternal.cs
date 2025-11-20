using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventInternal : SEvent
{
	public ESESubTypeInternal SubType { get; private set; }

	public SEventInternal()
	{
	}

	public SEventInternal(SEventInternal state, ReferenceDictionary references)
		: base(state, references)
	{
		SubType = state.SubType;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubType", SubType);
	}

	public SEventInternal(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "SubType")
				{
					SubType = (ESESubTypeInternal)info.GetValue("SubType", typeof(ESESubTypeInternal));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventInternal entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventInternal(ESESubTypeInternal subType, string text = "")
		: base(ESEType.Internal, text)
	{
		SubType = subType;
	}
}
