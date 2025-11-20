#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using SM.Utils;

namespace Platforms.Generic;

public class PlatformEntitlementGeneric : IPlatformEntitlement
{
	private readonly HashSet<string> _testEntitlements = new HashSet<string>();

	private bool _initialized;

	public HashSet<string> AvailableEntitlements => _testEntitlements;

	public HashSet<string> ActiveEntitlements => _testEntitlements;

	public HashSet<string> OwnedEntitlements => _testEntitlements;

	public event Action EntitlementsChanged;

	public bool IsOwnedAndInstalled(string entitlementID)
	{
		if (!_initialized)
		{
			throw new Exception("Check called without RefreshEntitlements");
		}
		return ActiveEntitlements.Contains(entitlementID);
	}

	public bool IsOwned(string entitlementID)
	{
		if (!_initialized)
		{
			throw new Exception("Check called without RefreshEntitlements");
		}
		return OwnedEntitlements.Contains(entitlementID);
	}

	public void OpenEntitlementStorePage(string entitlementLabel)
	{
		LogUtils.LogWarning("PlatformEntitlementGeneric.OpenEntitlementStorePage is not implemented");
	}

	public void RefreshEntitlements(Action callback)
	{
		_initialized = true;
		callback?.Invoke();
	}
}
