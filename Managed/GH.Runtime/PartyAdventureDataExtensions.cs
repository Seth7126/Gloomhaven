using ScenarioRuleLibrary;

public static class PartyAdventureDataExtensions
{
	public static bool HasInvalidDLCs(this PartyAdventureData partyAdventureData)
	{
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != DLCRegistry.EDLCKey.None && partyAdventureData.DLCEnabled.HasFlag(eDLCKey) && !PlatformLayer.DLC.UserInstalledDLC(eDLCKey))
			{
				return true;
			}
		}
		return false;
	}
}
