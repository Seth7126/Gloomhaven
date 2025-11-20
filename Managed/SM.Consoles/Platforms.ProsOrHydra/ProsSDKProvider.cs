using Platforms.Utils;

namespace Platforms.ProsOrHydra;

public class ProsSDKProvider : IHydraProsSDKProvider
{
	private readonly DebugFlags _debugFlags;

	private readonly IProsSettingsProvider _prosSettingsProvider;

	public ProsSDKProvider(IProsSettingsProvider settingsProvider, DebugFlags debugFlags)
	{
		_debugFlags = debugFlags;
		_prosSettingsProvider = settingsProvider;
	}

	public IHydraProsSDK CreateSDK()
	{
		return new ProsSDKAdapter(_prosSettingsProvider, _debugFlags);
	}
}
