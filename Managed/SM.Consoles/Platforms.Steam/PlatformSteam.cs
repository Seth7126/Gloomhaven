using Platforms.Generic;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Steam;

public class PlatformSteam : PlatformGeneric
{
	public PlatformSteam(IGameProvider gameProvider, bool initHydra, bool initEntitlements, bool initPros, bool isDevicePairingIncluded)
		: base(gameProvider, initHydra, initEntitlements, initPros, isDevicePairingIncluded)
	{
	}

	protected override HydraAnalyticsBase CreateHydraAnalytics(IGameProvider gameProvider)
	{
		return new PlatformHydraAnalyticsSteam(base.PlatformUpdater, gameProvider.AppFlowInformer, gameProvider.HydraSettingsProviderSteam, gameProvider.HydraSettingsProvider, DebugFlags.Warning | DebugFlags.Error);
	}
}
