#define ENABLE_LOGS
using System;
using System.Globalization;
using System.Threading.Tasks;
using Hydra.Sdk.Extensions;
using Platforms.Utils;
using UnityEngine;

namespace Platforms.ProsOrHydra;

public abstract class HydraProsBase : IKsivaProvider, IAsyncDisposable
{
	protected DebugFlags SendDebugP;

	private IAppFlowInformer _appFlowInformer;

	private IHydraProsSDKProvider _sdkProvider;

	private bool _disposed;

	private string _ksiva;

	private string _originalSession = string.Empty;

	private bool _registered;

	private bool _registering;

	private IUpdater _updater;

	private float _lastRegisterAttempt;

	private bool _isPreviousStateOnline;

	private bool _needsReconnect;

	private string _tag;

	private volatile bool _isOnline;

	public string Ksiva => _ksiva;

	public DebugFlags SendDebug
	{
		get
		{
			return SendDebugP;
		}
		set
		{
			SendDebugP = value;
		}
	}

	protected IHydraProsSDK SDK { get; private set; }

	public bool IsOnline
	{
		get
		{
			return _isOnline;
		}
		private set
		{
			if (_isOnline != value)
			{
				_isOnline = value;
				_updater.ExecuteInMainThread(delegate
				{
					this.EventConnectionStateChanged?.Invoke(_isOnline);
				});
			}
		}
	}

	public event Action<string> EventKsivaReceived;

	public event Action<bool> EventConnectionStateChanged;

	protected abstract Task<HydraProsSignInResponse> SignInForPlatform();

	protected virtual Task<bool> IsConnectionAvailable()
	{
		return Task.FromResult(result: true);
	}

	protected HydraProsBase(string tag, IUpdater updater, IAppFlowInformer appFlowInformer, IHydraProsSDKProvider sdkProvider, DebugFlags debugPFlags = DebugFlags.Error)
	{
		_updater = updater;
		_appFlowInformer = appFlowInformer;
		SendDebugP = debugPFlags;
		_tag = tag;
		_sdkProvider = sdkProvider;
		_appFlowInformer.EventAppStarting += OnApplicationStart;
		_appFlowInformer.EventAppSuspended += OnApplicationSuspended;
		_appFlowInformer.EventAppUnsuspended += OnApplicationUnsuspended;
		_appFlowInformer.EventAppQuiting += OnApplicationQuit;
	}

	public void Initialize()
	{
		if (_appFlowInformer.AppStarted)
		{
			OnApplicationStart();
		}
		if (_appFlowInformer.AppSuspended)
		{
			OnApplicationSuspended();
		}
	}

	public virtual async ValueTask DisposeAsync()
	{
		await InternalDispose();
	}

	private async Task InitializeHydraPros()
	{
		await DisposeHydraProsAsync();
		_disposed = false;
		SDK = _sdkProvider.CreateSDK();
		SendDebugP.Log("[" + _tag + "] SDK created");
		_updater.SubscribeForUpdate(CheckReconnect);
		await Register();
	}

	private async Task Register()
	{
		_ = 3;
		try
		{
			_registering = true;
			SendDebugP.Log("[" + _tag + "] Registering...");
			await SDK.RegisterComponents();
			SendDebugP.Log("[" + _tag + "] Registered");
			SendDebugP.Log("[" + _tag + "] Getting EnvironmentInfo...");
			if (!(await IsConnectionAvailable()))
			{
				_needsReconnect = true;
				return;
			}
			IGetEnvironmentInfoResponse getEnvironmentInfoResponse = await SDK.GetEnvironmentInfo();
			SendDebugP.Log("[" + _tag + "] Got EnvironmentInfo");
			if (!getEnvironmentInfoResponse.IsReadyStatus)
			{
				throw new Exception($"[{_tag}] Invalid environment status {getEnvironmentInfoResponse.IsReadyStatus}");
			}
			HydraProsSignInResponse hydraProsSignInResponse = await SignInForPlatform();
			if (hydraProsSignInResponse.Failed)
			{
				SendDebugP.LogWarning("[" + _tag + "] User can't sign in to platform => needs reconnect");
				_needsReconnect = !hydraProsSignInResponse.SuppressReconnect;
				return;
			}
			ISignInResponse response = hydraProsSignInResponse.Response;
			SendDebugP.Log("[" + _tag + "] signed in");
			string text = response.Date.FromUnixMilliseconds().ToString(CultureInfo.InvariantCulture);
			SendDebugP.Log("[" + _tag + "] sign in result: " + text + " " + response.ExternalIdentityToken + ".");
			if (string.IsNullOrEmpty(_originalSession))
			{
				_originalSession = response.KernelSessionId;
			}
			SendDebugP.Log("[" + _tag + "] Initializing Ksiva...");
			InitializeKsiva(response);
			SendDebugP.Log("[" + _tag + "] Ksiva initialized");
			_registered = true;
			_needsReconnect = false;
			SendDebugP.Log("[" + _tag + "] Registered successfully");
		}
		catch (Exception exception)
		{
			SendDebugP.LogWarning("[" + _tag + "] SDK connection error => needs reconnect");
			_needsReconnect = true;
			SDK.LogException(exception);
		}
		finally
		{
			_registering = false;
		}
	}

	private void OnApplicationStart()
	{
		_lastRegisterAttempt = Time.time;
		InitializeHydraPros();
	}

	private void OnApplicationQuit()
	{
		InternalDispose();
	}

	private void OnApplicationSuspended()
	{
		SDK?.DisableNetworkInBackground(value: true);
	}

	private void OnApplicationUnsuspended()
	{
		SDK?.DisableNetworkInBackground(value: false);
	}

	private async ValueTask InternalDispose()
	{
		_appFlowInformer.EventAppStarting -= OnApplicationStart;
		_appFlowInformer.EventAppSuspended -= OnApplicationSuspended;
		_appFlowInformer.EventAppUnsuspended -= OnApplicationUnsuspended;
		_appFlowInformer.EventAppQuiting -= OnApplicationQuit;
		_updater.UnsubscribeFromUpdate(CheckReconnect);
		await DisposeHydraProsAsync();
	}

	private async Task DisposeHydraProsAsync()
	{
		if (_disposed || SDK == null)
		{
			return;
		}
		_disposed = true;
		_registered = false;
		_registering = false;
		SendDebugP.Log("[" + _tag + "] Disposing");
		try
		{
			await SDK.DisposeAsync();
			SendDebugP.Log("[" + _tag + "] successfully disposed");
		}
		catch (Exception exception)
		{
			SDK.LogException(exception);
		}
	}

	private void InitializeKsiva(ISignInResponse signInResponse)
	{
		if (signInResponse != null)
		{
			string kernelSessionId = signInResponse.KernelSessionId;
			_ksiva = ((kernelSessionId != null) ? kernelSessionId.Substring(0, Math.Min(5, kernelSessionId.Length)) : string.Empty);
			IsOnline = true;
			_updater.ExecuteInMainThread(delegate
			{
				this.EventKsivaReceived?.Invoke(_ksiva);
			});
		}
		else
		{
			SendDebugP.Log("[" + _tag + "] sign in Data is empty.");
		}
	}

	private async void CheckReconnect()
	{
		if (SDK != null)
		{
			await SDK.Tick();
			if (!SDK.IsOnlineState && _isPreviousStateOnline)
			{
				SendDebugP.LogWarning("[" + _tag + "] Connection to SDK lost => needs reconnect");
				_lastRegisterAttempt = Time.time;
				_needsReconnect = true;
				IsOnline = false;
			}
			_isPreviousStateOnline = SDK.IsOnlineState;
			if (_needsReconnect && !_registering && !_appFlowInformer.AppSuspended && (double)(Time.time - _lastRegisterAttempt) >= SDK.ConnectionCheckCooldown)
			{
				SendDebugP.Log("[" + _tag + "] Trying to reconnect SDK");
				_lastRegisterAttempt = Time.time;
				Register();
			}
		}
	}
}
