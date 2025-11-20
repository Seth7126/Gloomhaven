using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;
using StateCodeGenerator;

namespace MapRuleLibrary.YML.PersonalQuests;

public class PersonalQuestYMLData
{
	public string ID { get; set; }

	public string LocalisedName { get; set; }

	public string LocalisedDescription { get; set; }

	public string LocalisedObjectiveDescription { get; set; }

	public string LocalisedCompletedStory => ID + "_COMPLETED";

	public string LocalisedObjectiveTitle { get; set; }

	public string LocalisedObjectiveNotProgressed
	{
		get
		{
			if (!string.IsNullOrEmpty(LocalisedObjectiveTitle))
			{
				return LocalisedObjectiveTitle + "_EMPTY";
			}
			return null;
		}
	}

	public CUnlockCondition PersonalQuestCondition { get; set; }

	public List<PersonalQuestYMLData> PersonalQuestSteps { get; set; }

	public bool IsPersonalQuestStep { get; set; }

	public bool? Discard { get; set; }

	public List<string> TreasureTables { get; set; }

	public string AudioIdCompletedStory { get; set; }

	public string AudioIdProgressFirstStepStory { get; set; }

	public string FileName { get; private set; }

	public List<string> FinalStepTreasureTable
	{
		get
		{
			if (TreasureTables != null)
			{
				return TreasureTables;
			}
			if (PersonalQuestSteps.Count > 0)
			{
				return PersonalQuestSteps.Last().TreasureTables;
			}
			return null;
		}
	}

	public PersonalQuestYMLData()
	{
	}

	public PersonalQuestYMLData(PersonalQuestYMLData state, ReferenceDictionary references)
	{
		ID = state.ID;
		LocalisedName = state.LocalisedName;
		LocalisedDescription = state.LocalisedDescription;
		LocalisedObjectiveDescription = state.LocalisedObjectiveDescription;
		LocalisedObjectiveTitle = state.LocalisedObjectiveTitle;
		PersonalQuestSteps = references.Get(state.PersonalQuestSteps);
		if (PersonalQuestSteps == null && state.PersonalQuestSteps != null)
		{
			PersonalQuestSteps = new List<PersonalQuestYMLData>();
			for (int i = 0; i < state.PersonalQuestSteps.Count; i++)
			{
				PersonalQuestYMLData personalQuestYMLData = state.PersonalQuestSteps[i];
				PersonalQuestYMLData personalQuestYMLData2 = references.Get(personalQuestYMLData);
				if (personalQuestYMLData2 == null && personalQuestYMLData != null)
				{
					personalQuestYMLData2 = new PersonalQuestYMLData(personalQuestYMLData, references);
					references.Add(personalQuestYMLData, personalQuestYMLData2);
				}
				PersonalQuestSteps.Add(personalQuestYMLData2);
			}
			references.Add(state.PersonalQuestSteps, PersonalQuestSteps);
		}
		IsPersonalQuestStep = state.IsPersonalQuestStep;
		Discard = state.Discard;
		TreasureTables = references.Get(state.TreasureTables);
		if (TreasureTables == null && state.TreasureTables != null)
		{
			TreasureTables = new List<string>();
			for (int j = 0; j < state.TreasureTables.Count; j++)
			{
				string item = state.TreasureTables[j];
				TreasureTables.Add(item);
			}
			references.Add(state.TreasureTables, TreasureTables);
		}
		AudioIdCompletedStory = state.AudioIdCompletedStory;
		AudioIdProgressFirstStepStory = state.AudioIdProgressFirstStepStory;
		FileName = state.FileName;
	}

	public PersonalQuestYMLData(string filename)
	{
		FileName = filename;
		ID = null;
		LocalisedName = null;
		LocalisedDescription = null;
		LocalisedObjectiveDescription = null;
		TreasureTables = null;
		AudioIdCompletedStory = null;
		AudioIdProgressFirstStepStory = null;
		Discard = null;
		LocalisedObjectiveTitle = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid PersonalQuest ID specified for PersonalQuest in file " + FileName);
			return false;
		}
		if (LocalisedName == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid PersonalQuest LocalisedName specified for PersonalQuest in file " + FileName);
			return false;
		}
		if (LocalisedDescription == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid PersonalQuest LocalisedDescription specified for PersonalQuest in file " + FileName);
			return false;
		}
		if (LocalisedObjectiveDescription == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid PersonalQuest LocalisedObjectiveDescription specified for PersonalQuest in file " + FileName);
			return false;
		}
		if (TreasureTables == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid PersonalQuest TreasureTables specified for PersonalQuest in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(PersonalQuestYMLData newData)
	{
		if (newData.LocalisedName != null)
		{
			LocalisedName = newData.LocalisedName;
		}
		if (newData.LocalisedDescription != null)
		{
			LocalisedDescription = newData.LocalisedDescription;
		}
		if (newData.LocalisedObjectiveDescription != null)
		{
			LocalisedObjectiveDescription = newData.LocalisedObjectiveDescription;
		}
		if (newData.TreasureTables != null)
		{
			TreasureTables = newData.TreasureTables;
		}
		if (newData.AudioIdCompletedStory != null)
		{
			AudioIdCompletedStory = newData.AudioIdCompletedStory;
		}
		if (newData.AudioIdProgressFirstStepStory != null)
		{
			AudioIdProgressFirstStepStory = newData.AudioIdProgressFirstStepStory;
		}
		if (newData.Discard.HasValue)
		{
			Discard = newData.Discard;
		}
		if (newData.LocalisedObjectiveTitle != null)
		{
			LocalisedObjectiveTitle = newData.LocalisedObjectiveTitle;
		}
	}
}
