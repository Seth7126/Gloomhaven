using System.Collections.Generic;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;
using StateCodeGenerator;

namespace MapRuleLibrary.YML.BattleGoals;

public class BattleGoalYMLData
{
	public string ID { get; set; }

	public string LocalisedName { get; set; }

	public string LocalisedDescription { get; set; }

	public CUnlockCondition BattleGoalCondition { get; set; }

	public CUnlockCondition BattleGoalFailCondition { get; set; }

	public int? PerkPoints { get; set; }

	public List<string> TreasureTables { get; set; }

	public bool CheckAtEndOfScenario { get; set; }

	public bool CampaignOnly { get; set; }

	public string FileName { get; private set; }

	public BattleGoalYMLData()
	{
	}

	public BattleGoalYMLData(BattleGoalYMLData state, ReferenceDictionary references)
	{
		ID = state.ID;
		LocalisedName = state.LocalisedName;
		LocalisedDescription = state.LocalisedDescription;
		PerkPoints = state.PerkPoints;
		TreasureTables = references.Get(state.TreasureTables);
		if (TreasureTables == null && state.TreasureTables != null)
		{
			TreasureTables = new List<string>();
			for (int i = 0; i < state.TreasureTables.Count; i++)
			{
				string item = state.TreasureTables[i];
				TreasureTables.Add(item);
			}
			references.Add(state.TreasureTables, TreasureTables);
		}
		CheckAtEndOfScenario = state.CheckAtEndOfScenario;
		CampaignOnly = state.CampaignOnly;
		FileName = state.FileName;
	}

	public BattleGoalYMLData(string filename)
	{
		FileName = filename;
		ID = null;
		LocalisedName = null;
		LocalisedDescription = null;
		PerkPoints = null;
		TreasureTables = null;
		CheckAtEndOfScenario = false;
		CampaignOnly = false;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid BattleGoal ID specified for BattleGoal in file " + FileName);
			return false;
		}
		if (LocalisedName == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid BattleGoal LocalisedName specified for BattleGoal in file " + FileName);
			return false;
		}
		if (LocalisedDescription == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid BattleGoal LocalisedDescription specified for BattleGoal in file " + FileName);
			return false;
		}
		if (!PerkPoints.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid BattleGoal PerkPoints specified for BattleGoal in file " + FileName);
			return false;
		}
		if (TreasureTables == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid BattleGoal TreasureTables specified for BattleGoal in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(BattleGoalYMLData newData)
	{
		if (newData.LocalisedName != null)
		{
			LocalisedName = newData.LocalisedName;
		}
		if (newData.LocalisedDescription != null)
		{
			LocalisedDescription = newData.LocalisedDescription;
		}
		if (newData.PerkPoints.HasValue)
		{
			PerkPoints = newData.PerkPoints;
		}
		if (newData.TreasureTables != null)
		{
			TreasureTables = newData.TreasureTables;
		}
		if (newData.CheckAtEndOfScenario != CheckAtEndOfScenario)
		{
			CheckAtEndOfScenario = newData.CheckAtEndOfScenario;
		}
		if (newData.CampaignOnly != CampaignOnly)
		{
			CampaignOnly = newData.CampaignOnly;
		}
	}
}
