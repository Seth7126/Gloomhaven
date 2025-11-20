using Hydra.Api.Errors;
using Hydra.Sdk;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Interfaces;
using Pros.Sdk.Helpers;

namespace Pros.Sdk;

public class ProsSdk : HydraSdk
{
	public ProsSdk(ProsSdkSettings settings, IHydraSdkLogger debugLogger = null)
		: base(CreateSettings(settings), debugLogger)
	{
		ServiceHelper.RegisterServices();
	}

	private static HydraSdkSettings CreateSettings(ProsSdkSettings settings)
	{
		HydraSdkSettings hydraSdkSettings = new HydraSdkSettings
		{
			ClientVersion = (settings.Version ?? throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Property 'Version' cannot not be null")),
			TitleId = (settings.TitleId ?? throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Property 'TitleId' cannot not be null")),
			SecretKey = settings.SecretKey,
			ManualComponentsHandling = settings.ManualComponentsHandling,
			Platform = settings.Platform
		};
		switch (settings.Environment)
		{
		case SdkEnvironment.PRODUCTION:
			hydraSdkSettings.HydraEndpoint = "https://eds.prismray.io:11701";
			break;
		case SdkEnvironment.STAGING:
			hydraSdkSettings.HydraEndpoint = "https://stgpros.hydrapi.net:11701";
			break;
		case SdkEnvironment.DEVELOPMENT:
			hydraSdkSettings.HydraEndpoint = "https://rlxd1a1l.hydrapi.net:11701";
			break;
		}
		return hydraSdkSettings;
	}
}
