using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CAllowedAbilitiesPerCharacter : ISerializable
{
	public string CharacterID { get; private set; }

	public List<string> Abilities { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Abilities", Abilities);
	}

	public CAllowedAbilitiesPerCharacter(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "Abilities":
					Abilities = (List<string>)info.GetValue("Abilities", typeof(List<string>));
					break;
				case "Character":
					CharacterID = info.GetString("Character").Replace(" ", string.Empty) + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CAllowedAbilitiesPerCharacter entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAllowedAbilitiesPerCharacter(string characterID, List<string> abilities)
	{
		CharacterID = characterID;
		Abilities = abilities;
	}

	public CAllowedAbilitiesPerCharacter()
	{
	}

	public CAllowedAbilitiesPerCharacter(CAllowedAbilitiesPerCharacter state, ReferenceDictionary references)
	{
		CharacterID = state.CharacterID;
		Abilities = references.Get(state.Abilities);
		if (Abilities == null && state.Abilities != null)
		{
			Abilities = new List<string>();
			for (int i = 0; i < state.Abilities.Count; i++)
			{
				string item = state.Abilities[i];
				Abilities.Add(item);
			}
			references.Add(state.Abilities, Abilities);
		}
	}
}
