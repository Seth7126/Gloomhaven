using Hydra.Sdk.Components.Authorization;
using Platforms.Utils;

namespace Platforms.ProsOrHydra;

public abstract class HydraAnalyticsBase : HydraProsBase
{
	protected AuthorizationComponent Authorization => ((HydraSDKAdapter)base.SDK).AuthorizationComponent;

	protected HydraAnalyticsBase(IUpdater updater, IAppFlowInformer appFlowInformer, IHydraSettingsProvider prosHydraSettingsProvider, DebugFlags debugPFlags = DebugFlags.Error)
		: base("Hydra", updater, appFlowInformer, new HydraSDKProvider(prosHydraSettingsProvider, debugPFlags), debugPFlags)
	{
	}
}
