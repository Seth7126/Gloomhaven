#define ENABLE_LOGS
using System.Threading.Tasks;
using Hydra.Api.Auth;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Generic;

public class PlatformProsGeneric : ProsBase
{
	private readonly IProsHydraSettingsProviderGeneric _providerGeneric;

	public PlatformProsGeneric(IUpdater updater, IAppFlowInformer appFlowInformer, IProsSettingsProvider prosSettingsProvider, IProsHydraSettingsProviderGeneric providerGeneric, DebugFlags debugPFlags = DebugFlags.Error)
		: base(updater, appFlowInformer, prosSettingsProvider, debugPFlags)
	{
		_providerGeneric = providerGeneric;
	}

	protected override async Task<HydraProsSignInResponse> SignInForPlatform()
	{
		SendDebugP.Log("[PlatformHydraAnalyticsGeneric] Signing in for Generic...");
		SignInHydraResponse signInHydraResponse = await base.Authorization.SignInHydra(_providerGeneric.Login);
		SendDebugP.Log("[PlatformHydraAnalyticsGeneric] Signed in");
		return new HydraProsSignInResponse
		{
			Response = new SignInResponseHydra(signInHydraResponse.Data),
			Failed = false,
			SuppressReconnect = false
		};
	}
}
