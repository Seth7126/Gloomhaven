using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLostAdjacency : SEvent
{
	public string CharacterID { get; private set; }

	public SEventLostAdjacency()
	{
	}

	public SEventLostAdjacency(SEventLostAdjacency state, ReferenceDictionary references)
		: base(state, references)
	{
		CharacterID = state.CharacterID;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
	}

	public SEventLostAdjacency(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "CharacterID")
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventLostAdjacency entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventLostAdjacency(string characterID, string text = "")
		: base(ESEType.Donate, text)
	{
		CharacterID = characterID;
	}
}
