using System.Collections.Generic;
using Platforms;
using Platforms.PS4;
using Platforms.PS5;
using ScenarioRuleLibrary;

namespace Script.PlatformLayer;

public class GHEntitlementsProvider : IEntitlementsProviderPS4, IEntitlementPack, IEntitlementsProviderPS5
{
	private HashSet<string> _availableEntitlements;

	public HashSet<string> AvailableEntitlements
	{
		get
		{
			FillEntitlements();
			return _availableEntitlements;
		}
	}

	public string FromSceLabel(string sceID)
	{
		return sceID;
	}

	private void FillEntitlements()
	{
		if (_availableEntitlements == null)
		{
			_availableEntitlements = new HashSet<string>();
			string[] dLCNames = DLCRegistry.DLCNames;
			foreach (string item in dLCNames)
			{
				_availableEntitlements.Add(item);
			}
		}
	}
}
