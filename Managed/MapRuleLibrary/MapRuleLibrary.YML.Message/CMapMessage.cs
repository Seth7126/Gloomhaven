using System;
using System.Collections.Generic;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Message;

public class CMapMessage
{
	public static readonly EMapMessageTrigger[] MessageTriggers = (EMapMessageTrigger[])Enum.GetValues(typeof(EMapMessageTrigger));

	public string MessageID { get; private set; }

	public EMapMessageTrigger MapMessageTrigger { get; set; }

	public List<MapDialogueLine> DialogueLines { get; set; }

	public CUnlockCondition UnlockCondition { get; set; }

	public string FileName { get; private set; }

	public CMapMessage(string messageID, List<MapDialogueLine> dialogueLines, EMapMessageTrigger mapMessageTrigger, CUnlockCondition unlockCondition, string fileName)
	{
		MessageID = messageID;
		MapMessageTrigger = mapMessageTrigger;
		DialogueLines = dialogueLines;
		UnlockCondition = unlockCondition;
		FileName = fileName;
	}

	public bool Validate()
	{
		if (MessageID == string.Empty)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Name specified for MapMessage in file " + FileName);
			return false;
		}
		if (MapMessageTrigger == EMapMessageTrigger.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid MapMessageTrigger specified for MapMessage in file " + FileName);
			return false;
		}
		if (UnlockCondition == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid UnlockCondition specified for MapMessage in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(List<MapDialogueLine> dialogueLines, EMapMessageTrigger mapMessageTrigger, CUnlockCondition unlockCondition)
	{
		if (mapMessageTrigger != EMapMessageTrigger.None)
		{
			MapMessageTrigger = mapMessageTrigger;
		}
		if (dialogueLines != null && dialogueLines.Count > 0)
		{
			DialogueLines = dialogueLines;
		}
		if (unlockCondition != null)
		{
			UnlockCondition = unlockCondition;
		}
	}
}
