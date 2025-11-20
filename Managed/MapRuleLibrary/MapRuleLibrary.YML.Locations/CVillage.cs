using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class CVillage : CLocation
{
	public List<string> QuestNames { get; set; }

	public List<CVector3> JobMapLocations { get; set; }

	public List<CQuest> Quests => MapRuleLibraryClient.MRLYML.Quests.Where((CQuest w) => QuestNames.Any((string a) => w.ID == a)).ToList();

	public List<CQuest> JobQuests => MapRuleLibraryClient.MRLYML.Quests.Where((CQuest w) => QuestNames.Any((string a) => w.ID == a && w.Type == EQuestType.Job)).ToList();

	public CVillage(string id, string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, List<TileIndex> jobMapLocations, CUnlockCondition unlockCondition, List<string> questNames, string fileName)
		: base(id, localisedName, localisedDescription, mesh, mapLocation, unlockCondition, fileName)
	{
		QuestNames = questNames;
		JobMapLocations = new List<CVector3>();
		foreach (TileIndex jobMapLocation in jobMapLocations)
		{
			JobMapLocations.Add(MapYMLShared.GetScreenPointFromMap(jobMapLocation.X, jobMapLocation.Y));
		}
	}

	public new bool Validate()
	{
		bool result = base.Validate();
		if (JobMapLocations == null || JobMapLocations.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "No valid Job Map Locations specified for Village in file " + base.FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, List<TileIndex> jobMapLocations, List<TileIndex> removeJobMapLocations, CUnlockCondition unlockCondition, List<string> questNames, List<string> removeQuestNames)
	{
		UpdateData(localisedName, localisedDescription, mesh, mapLocation, unlockCondition);
		if (questNames.Count > 0)
		{
			foreach (string questName in questNames)
			{
				if (!QuestNames.Contains(questName))
				{
					QuestNames.Add(questName);
				}
			}
		}
		if (removeQuestNames.Count > 0)
		{
			foreach (string removeQuestName in removeQuestNames)
			{
				if (QuestNames.Contains(removeQuestName))
				{
					QuestNames.Remove(removeQuestName);
				}
			}
		}
		if (jobMapLocations.Count > 0)
		{
			foreach (TileIndex jobMapLocation in jobMapLocations)
			{
				CVector3 location = MapYMLShared.GetScreenPointFromMap(jobMapLocation.X, jobMapLocation.Y);
				if (!JobMapLocations.Exists((CVector3 e) => CVector3.Compare(e, location)))
				{
					JobMapLocations.Add(location);
				}
			}
		}
		if (removeJobMapLocations.Count <= 0)
		{
			return;
		}
		foreach (TileIndex removeJobMapLocation in removeJobMapLocations)
		{
			CVector3 location2 = MapYMLShared.GetScreenPointFromMap(removeJobMapLocation.X, removeJobMapLocation.Y);
			CVector3 cVector = JobMapLocations.SingleOrDefault((CVector3 s) => CVector3.Compare(s, location2));
			if (cVector != null)
			{
				JobMapLocations.Remove(cVector);
			}
		}
	}
}
