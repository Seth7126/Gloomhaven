using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventPerk : SEvent
{
	public string CharacterID { get; private set; }

	public int PerkPoints { get; private set; }

	public SEventPerk()
	{
	}

	public SEventPerk(SEventPerk state, ReferenceDictionary references)
		: base(state, references)
	{
		CharacterID = state.CharacterID;
		PerkPoints = state.PerkPoints;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("PerkPoints", PerkPoints);
	}

	public SEventPerk(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "CharacterID"))
				{
					if (name == "PerkPoints")
					{
						PerkPoints = info.GetInt32("PerkPoints");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventPerk entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventPerk(string characterID, int perkPoints, string text = "")
		: base(ESEType.Perk, text)
	{
		CharacterID = characterID;
		PerkPoints = perkPoints;
	}
}
