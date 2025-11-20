using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.Party;

[Serializable]
public class CCompletedSoloQuestData
{
	public string QuestID { get; private set; }

	public int QuestCompletionLevel { get; private set; }

	public int QuestGainedXP { get; private set; }

	public int QuestGainedGold { get; private set; }

	public CCompletedSoloQuestData()
	{
	}

	public CCompletedSoloQuestData(CCompletedSoloQuestData state, ReferenceDictionary references)
	{
		QuestID = state.QuestID;
		QuestCompletionLevel = state.QuestCompletionLevel;
		QuestGainedXP = state.QuestGainedXP;
		QuestGainedGold = state.QuestGainedGold;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("QuestID", QuestID);
		info.AddValue("QuestCompletionLevel", QuestCompletionLevel);
		info.AddValue("QuestGainedXP", QuestGainedXP);
		info.AddValue("QuestGainedGold", QuestGainedGold);
	}

	public CCompletedSoloQuestData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "QuestID":
					QuestID = info.GetString("QuestID");
					break;
				case "QuestCompletionLevel":
					QuestCompletionLevel = info.GetInt32("QuestCompletionLevel");
					break;
				case "QuestGainedXP":
					QuestGainedXP = info.GetInt32("QuestGainedXP");
					break;
				case "QuestGainedGold":
					QuestGainedGold = info.GetInt32("QuestGainedGold");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CCompletedSoloQuestData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CCompletedSoloQuestData(string questID, int questCompletionLevel, int questGainedXP, int questGainedGold)
	{
		QuestID = questID;
		QuestCompletionLevel = questCompletionLevel;
		QuestGainedXP = questGainedXP;
		QuestGainedGold = questGainedGold;
	}
}
