using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventPhase : SEvent
{
	public CPhase.PhaseType SubType { get; private set; }

	public ESESubTypePhase SubTypePhase { get; private set; }

	public SEventPhase()
	{
	}

	public SEventPhase(SEventPhase state, ReferenceDictionary references)
		: base(state, references)
	{
		SubType = state.SubType;
		SubTypePhase = state.SubTypePhase;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubType", SubType);
		info.AddValue("SubTypePhase", SubTypePhase);
	}

	public SEventPhase(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "SubType"))
				{
					if (name == "SubTypePhase")
					{
						SubTypePhase = (ESESubTypePhase)info.GetValue("SubTypePhase", typeof(ESESubTypePhase));
					}
				}
				else
				{
					SubType = (CPhase.PhaseType)info.GetValue("SubType", typeof(CPhase.PhaseType));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventPhase entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventPhase(CPhase.PhaseType subType, ESESubTypePhase subTypePhase, string text = "", bool doNotSerialize = false)
		: base(ESEType.Phase, text, doNotSerialize)
	{
		SubType = subType;
		SubTypePhase = subTypePhase;
	}
}
