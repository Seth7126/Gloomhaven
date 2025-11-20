using System.Threading.Tasks;
using Hydra.Api.Auth;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Steam;

public class PlatformHydraAnalyticsSteam : HydraAnalyticsBase
{
	private IHydraSettingsProviderSteam _steamProvider;

	public PlatformHydraAnalyticsSteam(IUpdater updater, IAppFlowInformer appFlowInformer, IHydraSettingsProviderSteam steamProvider, IHydraSettingsProvider prosHydraSettingsProvider, DebugFlags debugPFlags = DebugFlags.Error)
		: base(updater, appFlowInformer, prosHydraSettingsProvider, debugPFlags)
	{
		_steamProvider = steamProvider;
	}

	protected override async Task<HydraProsSignInResponse> SignInForPlatform()
	{
		byte[] array = await _steamProvider.GetAuthTicket();
		if (array == null)
		{
			return new HydraProsSignInResponse
			{
				Response = null,
				Failed = true,
				SuppressReconnect = true
			};
		}
		SignInSteamResponse signInSteamResponse = await base.Authorization.SignInSteam(array, array.Length);
		_steamProvider.DisposeTicket();
		return new HydraProsSignInResponse
		{
			Response = new SignInResponseHydra(signInSteamResponse.Data),
			Failed = false,
			SuppressReconnect = false
		};
	}
}
