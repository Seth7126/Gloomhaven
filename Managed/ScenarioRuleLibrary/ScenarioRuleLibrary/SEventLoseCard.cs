using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLoseCard : SEvent
{
	public string CharacterID { get; private set; }

	public int Amount { get; private set; }

	public SEventLoseCard()
	{
	}

	public SEventLoseCard(SEventLoseCard state, ReferenceDictionary references)
		: base(state, references)
	{
		CharacterID = state.CharacterID;
		Amount = state.Amount;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Amount", Amount);
	}

	public SEventLoseCard(SerializationInfo info, StreamingContext context)
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
					if (name == "Amount")
					{
						Amount = info.GetInt32("Amount");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventEnhancement entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventLoseCard(string characterID, int amount, string text = "")
		: base(ESEType.LoseCard, text)
	{
		CharacterID = characterID;
		Amount = amount;
	}
}
