using System.Collections.Generic;
using ScenarioRuleLibrary;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Events;

public class CRoadEvent
{
	public string ID { get; private set; }

	public string LocKey { get; private set; }

	public List<CRoadEventScreen> Screens { get; private set; }

	public string EventType { get; private set; }

	public string FileName { get; private set; }

	public string NarrativeImageId { get; private set; }

	public string Expansion { get; private set; }

	public List<string> RequiredClass { get; private set; }

	public CRoadEvent(string id, string locKey, List<CRoadEventScreen> screens, string eventType, string filename, string narrativeImageId, List<string> requiredClass, string expansion)
	{
		ID = id;
		LocKey = locKey;
		Screens = screens;
		EventType = eventType;
		FileName = filename;
		NarrativeImageId = narrativeImageId;
		RequiredClass = requiredClass;
		Expansion = expansion;
	}

	public List<CItem> GetEventItemRewards()
	{
		List<CItem> list = new List<CItem>();
		foreach (CRoadEventScreen screen in Screens)
		{
			list.AddRange(screen.GetScreenItems());
		}
		return list;
	}

	public bool Validate()
	{
		bool result = true;
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid ID specified for road event in file " + FileName);
			result = false;
		}
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid LocKey specified for road event in file " + FileName);
			result = false;
		}
		if (Screens == null || Screens.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Screens specified for road event in file " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string locKey, List<CRoadEventScreen> screens, string eventType, string narrativeImageId, List<string> requiredClass, string expansion)
	{
		if (locKey != null)
		{
			LocKey = locKey;
		}
		if (screens != null)
		{
			Screens = screens;
		}
		if (eventType != null)
		{
			EventType = eventType;
		}
		if (narrativeImageId != null)
		{
			NarrativeImageId = narrativeImageId;
		}
		if (requiredClass != null)
		{
			RequiredClass = requiredClass;
		}
		if (expansion != "")
		{
			Expansion = expansion;
		}
	}
}
