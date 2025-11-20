using Platforms.Utils;

namespace Platforms.ProsOrHydra;

public class HydraSDKProvider : IHydraProsSDKProvider
{
	private readonly IHydraSettingsProvider _hydraSettingsProvider;

	private readonly DebugFlags _debugFlags;

	public HydraSDKProvider(IHydraSettingsProvider settingsProvider, DebugFlags debugFlags)
	{
		_hydraSettingsProvider = settingsProvider;
		_debugFlags = debugFlags;
	}

	public IHydraProsSDK CreateSDK()
	{
		return new HydraSDKAdapter(_hydraSettingsProvider, _debugFlags);
	}
}
