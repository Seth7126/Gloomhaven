#define ENABLE_LOGS
using System;
using System.Threading.Tasks;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Errors;
using Platforms.Utils;
using Pros.Sdk;
using Pros.Sdk.Components.Account;
using Pros.Sdk.Components.Authorization;
using Pros.Sdk.Components.CrossSave;
using Pros.Sdk.Components.Telemetry;
using RedLynx.Api.Errors;

namespace Platforms.ProsOrHydra;

public class ProsSDKAdapter : IHydraProsSDK
{
	private readonly DebugFlags _debugFlags;

	public bool IsOnlineState => ProsSdk.State == OnlineState.Online;

	public double ConnectionCheckCooldown { get; }

	public AccountComponent AccountComponent => ProsSdk.Call<AccountComponent>();

	public AuthorizationComponent AuthorizationComponent => ProsSdk.Call<AuthorizationComponent>();

	public CrossSaveComponent CrossSaveComponent => ProsSdk.Call<CrossSaveComponent>();

	public TelemetryComponent Telemetry => ProsSdk.Call<TelemetryComponent>();

	public ProsSdk ProsSdk { get; }

	public ProsSDKAdapter(IProsSettingsProvider prosSettingsProvider, DebugFlags debugFlags)
	{
		_debugFlags = debugFlags;
		ConnectionCheckCooldown = prosSettingsProvider.ConnectionCheckCooldown;
		ProsSdk = new ProsSdk(new ProsSdkSettings
		{
			Environment = prosSettingsProvider.Environment,
			TitleId = prosSettingsProvider.ProsTitleId,
			SecretKey = prosSettingsProvider.ProsSecretKey,
			Version = prosSettingsProvider.ClientVersion
		}, new DefaultHydraProsLogger());
	}

	public Task RegisterComponents()
	{
		return Task.CompletedTask;
	}

	public async Task<IGetEnvironmentInfoResponse> GetEnvironmentInfo()
	{
		return new HydraGetEnvironmentInfoResponse(await AuthorizationComponent.GetEnvironmentInfo());
	}

	public void DisableNetworkInBackground(bool value)
	{
		ProsSdk.DisableNetworkInBackground(value);
	}

	public Task DisposeAsync()
	{
		return ProsSdk.DisposeAsync();
	}

	public void LogException(Exception exception)
	{
		HydraSDKAdapter.LogExceptionStatic(exception, _debugFlags, LogHydraSdkException);
	}

	private static void LogHydraSdkException(HydraSdkException hydraSdkException, DebugFlags debugFlags)
	{
		ErrorCode errorCode = (ErrorCode)hydraSdkException.ErrorCode;
		debugFlags.LogWarning($"[Pros] SDK exception: {errorCode}; Help link: {hydraSdkException.HelpLink}; Exception: {hydraSdkException};");
	}

	public async Task Tick()
	{
		if (ProsSdk.IsComponentRegistered<AuthorizationComponent>() && AuthorizationComponent.IsRefreshRequired)
		{
			await AuthorizationComponent.TryRefreshAuthSession();
		}
		if (ProsSdk.IsComponentRegistered<TelemetryComponent>() && Telemetry.CanSendTelemetryPacks && Telemetry.HasTelemetryPacks)
		{
			await Telemetry.TrySendTelemetryPack();
		}
	}
}
