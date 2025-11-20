using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventRound : SEvent
{
	public int MonsterCount { get; private set; }

	public SEventRound()
	{
	}

	public SEventRound(SEventRound state, ReferenceDictionary references)
		: base(state, references)
	{
		MonsterCount = state.MonsterCount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MonsterCount", MonsterCount);
	}

	public SEventRound(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "MonsterCount")
				{
					MonsterCount = info.GetInt32("MonsterCount");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventRound entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventRound(int monsterCount, string text = "")
		: base(ESEType.Round, text)
	{
		MonsterCount = monsterCount;
	}
}
