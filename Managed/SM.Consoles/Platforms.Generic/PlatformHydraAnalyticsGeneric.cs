#define ENABLE_LOGS
using System.Threading.Tasks;
using Hydra.Api.Auth;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Generic;

public class PlatformHydraAnalyticsGeneric : HydraAnalyticsBase
{
	private IProsHydraSettingsProviderGeneric _genericProvider;

	public PlatformHydraAnalyticsGeneric(IUpdater updater, IAppFlowInformer appFlowInformer, IProsHydraSettingsProviderGeneric genericProvider, IHydraSettingsProvider hydraSettingsProvider, DebugFlags debugPFlags = DebugFlags.Error)
		: base(updater, appFlowInformer, hydraSettingsProvider, debugPFlags)
	{
		_genericProvider = genericProvider;
	}

	protected override async Task<HydraProsSignInResponse> SignInForPlatform()
	{
		SendDebugP.Log("[PlatformHydraAnalyticsGeneric] Signing in for Generic...");
		SignInHydraResponse signInHydraResponse = await base.Authorization.SignInHydra(_genericProvider.Login);
		SendDebugP.Log("[PlatformHydraAnalyticsGeneric] Signed in");
		return new HydraProsSignInResponse
		{
			Response = new SignInResponseHydra(signInHydraResponse.Data),
			Failed = false,
			SuppressReconnect = false
		};
	}
}
