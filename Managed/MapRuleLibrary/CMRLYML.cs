using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Source.YML.Debug;
using MapRuleLibrary.Source.YML.VisibilitySpheres;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using SharedLibrary.Client;

public class CMRLYML
{
	private ScenarioManager.EDLLMode _mapMode;

	public CMRLYMLModeData GlobalData { get; private set; }

	public CMRLYMLModeData RulesetData { get; private set; }

	public CMRLYMLModeData ModdedData { get; private set; }

	public ScenarioManager.EDLLMode MapMode
	{
		get
		{
			return _mapMode;
		}
		set
		{
			ScenarioRuleClient.SRLYML.MapMode = value;
			_mapMode = value;
		}
	}

	public CSRLYML.EYMLMode YMLMode { get; set; }

	public List<DifficultyYMLData> Difficulty => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.Difficulty.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.Difficulty.LoadedYML.Concat(RulesetData.Difficulty.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.Difficulty.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<AchievementYMLData> Achievements => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.Achievements.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.Achievements.LoadedYML.Concat(RulesetData.Achievements.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.Achievements.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<BattleGoalYMLData> BattleGoals => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.BattleGoals.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.BattleGoals.LoadedYML.Concat(RulesetData.BattleGoals.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.BattleGoals.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<PersonalQuestYMLData> PersonalQuests => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.PersonalQuests.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.PersonalQuests.LoadedYML.Concat(RulesetData.PersonalQuests.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.PersonalQuests.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CRoadEvent> RoadEvents => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.RoadEvents.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.RoadEvents.LoadedYML.Concat(RulesetData.RoadEvents.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.RoadEvents.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CRoadEvent> CityEvents => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.CityEvents.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.CityEvents.LoadedYML.Concat(RulesetData.CityEvents.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.CityEvents.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public CInitialEventsYML InitialEvents
	{
		get
		{
			switch (YMLMode)
			{
			case CSRLYML.EYMLMode.Global:
				return GlobalData.InitialEvents;
			case CSRLYML.EYMLMode.StandardRuleset:
				if (!RulesetData.InitialEvents.IsLoaded)
				{
					return GlobalData.InitialEvents;
				}
				return RulesetData.InitialEvents;
			case CSRLYML.EYMLMode.ModdedRuleset:
				return ModdedData.InitialEvents;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public List<CVisibilitySphere> VisibilitySpheres => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.VisibilitySpheres.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.VisibilitySpheres.LoadedYML.Concat(RulesetData.VisibilitySpheres.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.VisibilitySpheres.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CTemple> Temples => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.Temples.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.Temples.LoadedYML.Concat(RulesetData.Temples.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.Temples.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CVillage> Villages => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.Villages.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.Villages.LoadedYML.Concat(RulesetData.Villages.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.Villages.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CStoreLocation> StoreLocations => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.StoreLocations.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.StoreLocations.LoadedYML.Concat(RulesetData.StoreLocations.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.StoreLocations.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CQuest> Quests => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.Quests.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.Quests.LoadedYML.Concat(RulesetData.Quests.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.Quests.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public List<CMapMessage> MapMessages => YMLMode switch
	{
		CSRLYML.EYMLMode.Global => GlobalData.MapMessages.LoadedYML, 
		CSRLYML.EYMLMode.StandardRuleset => GlobalData.MapMessages.LoadedYML.Concat(RulesetData.MapMessages.LoadedYML).ToList(), 
		CSRLYML.EYMLMode.ModdedRuleset => ModdedData.MapMessages.LoadedYML, 
		_ => throw new Exception("Attempting to read YML data while no mode is set!"), 
	};

	public CHeadquarters Headquarters
	{
		get
		{
			switch (YMLMode)
			{
			case CSRLYML.EYMLMode.Global:
				return GlobalData.Headquarters.Headquarters;
			case CSRLYML.EYMLMode.StandardRuleset:
				if (RulesetData.Headquarters == null)
				{
					return GlobalData.Headquarters.Headquarters;
				}
				return RulesetData.Headquarters.Headquarters;
			case CSRLYML.EYMLMode.ModdedRuleset:
				return ModdedData.Headquarters.Headquarters;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public AutoCompleteYML AutoComplete
	{
		get
		{
			switch (YMLMode)
			{
			case CSRLYML.EYMLMode.Global:
				return GlobalData.AutoCompletes;
			case CSRLYML.EYMLMode.StandardRuleset:
				if (!RulesetData.AutoCompletes.IsLoaded)
				{
					return GlobalData.AutoCompletes;
				}
				return RulesetData.AutoCompletes;
			case CSRLYML.EYMLMode.ModdedRuleset:
				return ModdedData.AutoCompletes;
			default:
				throw new Exception("Attempting to read YML data while no mode is set!");
			}
		}
	}

	public CMRLYML()
	{
		GlobalData = new CMRLYMLModeData();
		RulesetData = new CMRLYMLModeData();
	}

	public void UnloadRuleset()
	{
		RulesetData = new CMRLYMLModeData();
		ModdedData = new CMRLYMLModeData();
	}

	public void ResetModdedData()
	{
		ModdedData = new CMRLYMLModeData();
	}

	public bool Validate()
	{
		bool result = true;
		foreach (DifficultyYMLData item in Difficulty)
		{
			if (!item.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("DifficultyYML", "Failed to validate DifficultyYML YML " + item.FileName);
				result = false;
			}
		}
		foreach (AchievementYMLData achievement in Achievements)
		{
			if (!achievement.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("AchievementYML", "Failed to validate Achievement YML " + achievement.FileName);
				result = false;
			}
		}
		foreach (CRoadEvent roadEvent in RoadEvents)
		{
			if (!roadEvent.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("RoadEventYML", "Failed to validate Road Event YML " + roadEvent.FileName);
				result = false;
			}
		}
		foreach (CRoadEvent cityEvent in CityEvents)
		{
			if (!cityEvent.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("CityEventYML", "Failed to validate Road Event YML " + cityEvent.FileName);
				result = false;
			}
		}
		foreach (CVisibilitySphere visibilitySphere in VisibilitySpheres)
		{
			if (!visibilitySphere.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("VisibilitySphereYML", "Failed to validate Visibility Sphere YML " + visibilitySphere.FileName);
				result = false;
			}
		}
		foreach (CTemple temple in Temples)
		{
			if (!temple.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("TempleYML", "Failed to validate Temple YML " + temple.FileName);
				result = false;
			}
		}
		foreach (CVillage village in Villages)
		{
			if (!village.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("VillageYML", "Failed to validate village YML " + village.FileName);
				result = false;
			}
		}
		foreach (CQuest quest in Quests)
		{
			if (!quest.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("QuestYML", "Failed to validate quest YML " + quest.FileName);
				result = false;
			}
		}
		foreach (CMapMessage mapMessage in MapMessages)
		{
			if (!mapMessage.Validate())
			{
				SharedClient.ValidationRecord.RecordParseFailure("MapMessagesYML", "Failed to validate mapmessage YML " + mapMessage.FileName);
				result = false;
			}
		}
		if (!Headquarters.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("HeadquartersYML", "Failed to validate Headquarters YML " + Headquarters.FileName);
			result = false;
		}
		if (!AutoComplete.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure("AutoCompleteYML", "Failed to validate AutoComplete YML " + AutoComplete.FileName);
			result = false;
		}
		return result;
	}
}
