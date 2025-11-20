#define ENABLE_LOGS
using System;
using System.Threading.Tasks;
using Hydra.Sdk;
using Hydra.Sdk.Components.Authorization;
using Hydra.Sdk.Components.Facts;
using Hydra.Sdk.Components.Telemetry;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Errors;
using Platforms.Utils;

namespace Platforms.ProsOrHydra;

public class HydraSDKAdapter : IHydraProsSDK
{
	private readonly DebugFlags _debugFlags;

	private readonly HydraSdk _hydraSdk;

	public bool IsOnlineState => _hydraSdk.State == OnlineState.Online;

	public double ConnectionCheckCooldown { get; }

	public AuthorizationComponent AuthorizationComponent => _hydraSdk.Call<AuthorizationComponent>();

	public TelemetryComponent Telemetry => _hydraSdk.Call<TelemetryComponent>();

	public FactsComponent Facts => _hydraSdk.Call<FactsComponent>();

	public HydraSDKAdapter(IHydraSettingsProvider settingsProvider, DebugFlags debugFlags)
	{
		_debugFlags = debugFlags;
		ConnectionCheckCooldown = settingsProvider.ConnectionCheckCooldown;
		_hydraSdk = new HydraSdk(new HydraSdkSettings
		{
			HydraEndpoint = settingsProvider.HydraEndpoint,
			TitleId = settingsProvider.HydraTitleId,
			SecretKey = settingsProvider.HydraSecretKey,
			ClientVersion = settingsProvider.ClientVersion
		}, new DefaultHydraProsLogger());
	}

	public async Task RegisterComponents()
	{
		await _hydraSdk.RegisterComponent<AuthorizationComponent>();
	}

	public async Task<IGetEnvironmentInfoResponse> GetEnvironmentInfo()
	{
		return new HydraGetEnvironmentInfoResponse(await AuthorizationComponent.GetEnvironmentInfo());
	}

	public void DisableNetworkInBackground(bool value)
	{
		_hydraSdk.DisableNetworkInBackground(value);
	}

	public Task DisposeAsync()
	{
		return _hydraSdk.DisposeAsync();
	}

	public void LogException(Exception exception)
	{
		LogExceptionStatic(exception, _debugFlags, LogHydraSdkException);
	}

	private static void LogHydraSdkException(HydraSdkException hydraSdkException, DebugFlags debugFlags)
	{
		debugFlags.LogWarning($"[Hydra] SDK exception: {hydraSdkException.ErrorCode}; Help link: {hydraSdkException.HelpLink}; Exception: {hydraSdkException};");
	}

	public static void LogExceptionStatic(Exception exception, DebugFlags debugFlags, Action<HydraSdkException, DebugFlags> sdkExceptionLogger)
	{
		if (!(exception is HydraSdkException arg))
		{
			if (exception is AggregateException ex)
			{
				debugFlags.Log("[Hydra AggregateException");
				debugFlags.LogException(ex);
				{
					foreach (Exception innerException in ex.InnerExceptions)
					{
						if (innerException is HydraSdkException arg2)
						{
							sdkExceptionLogger?.Invoke(arg2, debugFlags);
						}
						else
						{
							debugFlags.LogException(innerException);
						}
					}
					return;
				}
			}
			debugFlags.LogException(exception);
		}
		else
		{
			sdkExceptionLogger?.Invoke(arg, debugFlags);
		}
	}

	public async Task Tick()
	{
		if (_hydraSdk.IsComponentRegistered<AuthorizationComponent>() && AuthorizationComponent.IsRefreshRequired)
		{
			await AuthorizationComponent.TryRefreshAuthSession();
		}
		if (_hydraSdk.IsComponentRegistered<FactsComponent>() && Facts.CanSendFactPacks && Facts.HasFactPacks)
		{
			await Facts.TrySendFactPack();
		}
		if (_hydraSdk.IsComponentRegistered<TelemetryComponent>() && Telemetry.CanSendTelemetryPacks && Telemetry.HasTelemetryPacks)
		{
			await Telemetry.TrySendTelemetryPack();
		}
	}
}
