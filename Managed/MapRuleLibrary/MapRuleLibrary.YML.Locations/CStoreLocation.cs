using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class CStoreLocation : CLocation
{
	public EHQStores StoreType { get; private set; }

	public CStoreLocation(string id, string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, CUnlockCondition unlockCondition, string fileName, EHQStores storeType)
		: base(id, localisedName, localisedDescription, mesh, mapLocation, unlockCondition, fileName)
	{
		StoreType = storeType;
	}

	public new bool Validate()
	{
		bool result = base.Validate();
		if (StoreType == EHQStores.None || StoreType == EHQStores.Trainer)
		{
			SharedClient.ValidationRecord.RecordParseFailure(base.FileName, "Invalid StoreType specified for Store Location in File: " + base.FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, CUnlockCondition unlockCondition, EHQStores storeType)
	{
		UpdateData(localisedName, localisedDescription, mesh, mapLocation, unlockCondition);
		StoreType = storeType;
	}
}
