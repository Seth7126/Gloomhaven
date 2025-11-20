using Platforms.Generic;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Epic;

public class PlatformEpic : PlatformGeneric
{
	public PlatformEpic(IGameProvider gameProvider, bool initHydra, bool initEntitlements, bool initPros, bool isDevicePairingIncluded)
		: base(gameProvider, initHydra, initEntitlements, initPros, isDevicePairingIncluded)
	{
	}

	protected override HydraAnalyticsBase CreateHydraAnalytics(IGameProvider gameProvider)
	{
		return new PlatformHydraAnalyticsEpic(base.PlatformUpdater, gameProvider.AppFlowInformer, gameProvider.HydraSettingsProviderEpic, gameProvider.HydraSettingsProvider, DebugFlags.Warning | DebugFlags.Error);
	}
}
