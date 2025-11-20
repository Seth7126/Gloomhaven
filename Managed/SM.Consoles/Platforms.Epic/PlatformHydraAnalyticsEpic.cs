using System.Threading.Tasks;
using Hydra.Api.Auth;
using Platforms.ProsOrHydra;
using Platforms.Utils;

namespace Platforms.Epic;

public class PlatformHydraAnalyticsEpic : HydraAnalyticsBase
{
	private IHydraSettingsProviderEpic _epicProvider;

	public PlatformHydraAnalyticsEpic(IUpdater updater, IAppFlowInformer appFlowInformer, IHydraSettingsProviderEpic epicProvider, IHydraSettingsProvider prosHydraSettingsProvider, DebugFlags debugPFlags = DebugFlags.Error)
		: base(updater, appFlowInformer, prosHydraSettingsProvider, debugPFlags)
	{
		_epicProvider = epicProvider;
	}

	protected override async Task<HydraProsSignInResponse> SignInForPlatform()
	{
		string text = await _epicProvider.GetToken();
		if (string.IsNullOrEmpty(text))
		{
			return new HydraProsSignInResponse
			{
				Response = null,
				Failed = true,
				SuppressReconnect = true
			};
		}
		SignInEpicOnlineServicesResponse signInEpicOnlineServicesResponse = await base.Authorization.SignInEpicOnlineServices(text);
		return new HydraProsSignInResponse
		{
			Response = new SignInResponseHydra(signInEpicOnlineServicesResponse.Data),
			Failed = false,
			SuppressReconnect = false
		};
	}
}
