using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class CLocation
{
	public string ID { get; private set; }

	public string LocalisedName { get; set; }

	public string LocalisedDescription { get; set; }

	public string Mesh { get; set; }

	public CVector3 MapLocation { get; set; }

	public CUnlockCondition UnlockCondition { get; set; }

	public string FileName { get; private set; }

	public CLocation(string id, string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, CUnlockCondition unlockCondition, string fileName)
	{
		ID = id;
		LocalisedName = localisedName;
		LocalisedDescription = localisedDescription;
		Mesh = mesh;
		if (mapLocation != null)
		{
			MapLocation = MapYMLShared.GetScreenPointFromMap(mapLocation.X, mapLocation.Y);
		}
		UnlockCondition = unlockCondition;
		FileName = fileName;
	}

	protected bool Validate()
	{
		bool result = true;
		if (ID == string.Empty)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified in Location file: " + FileName);
			result = false;
		}
		if (string.IsNullOrEmpty(LocalisedName))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No LocalisedName specified in Location file: " + FileName);
			result = false;
		}
		if (string.IsNullOrEmpty(LocalisedDescription))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No LocalisedDescription specified in Location file: " + FileName);
			result = false;
		}
		if (string.IsNullOrEmpty(Mesh))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Mesh specified in Location file: " + FileName);
			result = false;
		}
		if (MapLocation == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MapLocation specified in Headquarters file: " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, CUnlockCondition unlockCondition)
	{
		if (!string.IsNullOrEmpty(localisedName))
		{
			LocalisedName = localisedName;
		}
		if (!string.IsNullOrEmpty(localisedDescription))
		{
			LocalisedDescription = localisedDescription;
		}
		if (!string.IsNullOrEmpty(mesh))
		{
			Mesh = mesh;
		}
		if (mapLocation != null)
		{
			MapLocation = MapYMLShared.GetScreenPointFromMap(mapLocation.X, mapLocation.Y);
		}
		if (unlockCondition != null)
		{
			UnlockCondition = unlockCondition;
		}
	}
}
