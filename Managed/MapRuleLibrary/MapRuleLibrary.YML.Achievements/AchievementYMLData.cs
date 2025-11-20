using System;
using System.Collections.Generic;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Achievements;

public class AchievementYMLData
{
	public static readonly EAchievementType[] AchievementTypes = (EAchievementType[])Enum.GetValues(typeof(EAchievementType));

	public string ID { get; set; }

	public string LocalisedName { get; set; }

	public string LocalisedDescription { get; set; }

	public CUnlockCondition UnlockCondition { get; set; }

	public CUnlockCondition AchievementCondition { get; set; }

	public List<string> TreasureTables { get; set; }

	public EAchievementType AchievementType { get; set; }

	public int AchievementLevel { get; set; }

	public List<MapDialogueLine> CompleteDialogueLines { get; set; }

	public string AchievementOrderId { get; set; }

	public string FileName { get; private set; }

	public string LocalizedRewardsTitle => ID + "_REWARDS_TITLE";

	public string LocalizedRewardsStory => ID + "_REWARDS_STORY";

	public AchievementYMLData(string filename)
	{
		FileName = filename;
		ID = null;
		LocalisedName = null;
		LocalisedDescription = null;
		UnlockCondition = null;
		AchievementCondition = null;
		TreasureTables = null;
		AchievementType = EAchievementType.None;
		AchievementLevel = int.MaxValue;
		CompleteDialogueLines = new List<MapDialogueLine>();
		AchievementOrderId = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement ID specified for achievement in file " + FileName);
			return false;
		}
		if (LocalisedName == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement LocalisedName specified for achievement in file " + FileName);
			return false;
		}
		if (LocalisedDescription == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement LocalisedDescription specified for achievement in file " + FileName);
			return false;
		}
		if (UnlockCondition == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement UnlockCondition specified for achievement in file " + FileName);
			return false;
		}
		if (AchievementCondition == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement AchievementCondition specified for achievement in file " + FileName);
			return false;
		}
		if (TreasureTables == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement TreasureTables specified for achievement in file " + FileName);
			return false;
		}
		if (AchievementType == EAchievementType.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement AchievementType specified for achievement in file " + FileName);
			return false;
		}
		if (AchievementLevel == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement AchievementLevel specified for achievement in file " + FileName);
			return false;
		}
		if (CompleteDialogueLines == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement CompleteDialogueLines specified for achievement in file " + FileName);
			return false;
		}
		if (AchievementOrderId == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Achievement AchievementOrderId specified for achievement in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(AchievementYMLData newData)
	{
		if (newData.LocalisedName != null)
		{
			LocalisedName = newData.LocalisedName;
		}
		if (newData.LocalisedDescription != null)
		{
			LocalisedDescription = newData.LocalisedDescription;
		}
		if (newData.UnlockCondition != null)
		{
			UnlockCondition = newData.UnlockCondition;
		}
		if (newData.AchievementCondition != null)
		{
			AchievementCondition = newData.AchievementCondition;
		}
		if (newData.TreasureTables != null)
		{
			TreasureTables = newData.TreasureTables;
		}
		if (newData.AchievementType != EAchievementType.None)
		{
			AchievementType = newData.AchievementType;
		}
		if (newData.AchievementLevel != int.MaxValue)
		{
			AchievementLevel = newData.AchievementLevel;
		}
		if (newData.CompleteDialogueLines != null)
		{
			CompleteDialogueLines = newData.CompleteDialogueLines;
		}
		if (newData.AchievementOrderId != null)
		{
			AchievementOrderId = newData.AchievementOrderId;
		}
	}
}
