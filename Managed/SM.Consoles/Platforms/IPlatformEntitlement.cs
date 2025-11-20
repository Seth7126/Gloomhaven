using System;
using System.Collections.Generic;

namespace Platforms;

public interface IPlatformEntitlement
{
	HashSet<string> AvailableEntitlements { get; }

	HashSet<string> ActiveEntitlements { get; }

	HashSet<string> OwnedEntitlements { get; }

	event Action EntitlementsChanged;

	bool IsOwnedAndInstalled(string entitlementID);

	bool IsOwned(string entitlementID);

	void OpenEntitlementStorePage(string entitlementLabel);

	void RefreshEntitlements(Action callback);
}
