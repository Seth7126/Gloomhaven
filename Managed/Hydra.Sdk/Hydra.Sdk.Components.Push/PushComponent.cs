using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Api.Push;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Communication.WebSockets;
using Hydra.Sdk.Communication.WebSockets.States;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Push;

public sealed class PushComponent : IHydraSdkComponent
{
	public delegate void MessageReceivedDelegate(WebSocketMessage msg);

	public delegate void ErrorReceivedDelegate(Exception ex);

	public MessageReceivedDelegate OnPushMessageReceived;

	public ErrorReceivedDelegate OnPushError;

	private IHydraSdkLogger _logger;

	private StateObserver<SdkState> _sdkState;

	private StateObserver<PushTokenWrapper> _pushToken;

	private StateObserver<WebSocketWrapper> _webSocket;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	private Task _taskReconnect;

	private CancellationTokenSource _token;

	private bool _connecting;

	private bool _reconnecting;

	private int _generation;

	public int GetDisposePriority()
	{
		return 200;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_pushToken = stateResolver.CreateLinkedObserver<PushTokenWrapper>();
		_webSocket = stateResolver.CreateLinkedObserver<WebSocketWrapper>();
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		StateObserver<SdkState> sdkState = _sdkState;
		sdkState.OnStateUpdate = (StateObserver<SdkState>.StateUpdate)Delegate.Combine(sdkState.OnStateUpdate, new StateObserver<SdkState>.StateUpdate(SdkStateChange));
		_logger = logger;
		IHydraSdkChannel channel = connectionManager.GetChannel(PushServiceStub.Descriptor.FullName);
		string text = (channel.GetInfo().IsSecure ? "wss://" : "ws://");
		WebSocketClient client = new WebSocketClient(new Uri(text + channel.GetInfo().Address), logger);
		_webSocket.Update(new WebSocketWrapper(client));
		return Connect();
	}

	public async Task Connect()
	{
		_token = new CancellationTokenSource();
		if (_connecting)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "Connection already initiated");
		}
		_connecting = true;
		if (_webSocket.State?.Client == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "WebSocket client hasn't been initialized");
		}
		if (_webSocket.State.Client.IsConnected)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "Already connected");
		}
		WebSocketClient client = _webSocket.State.Client;
		client.OnMessageReceived = (WebSocketClient.MessageReceivedDelegate)Delegate.Combine(client.OnMessageReceived, new WebSocketClient.MessageReceivedDelegate(OnPushMessage));
		WebSocketClient client2 = _webSocket.State.Client;
		client2.OnError = (WebSocketClient.OnErrorDelegate)Delegate.Combine(client2.OnError, new WebSocketClient.OnErrorDelegate(OnError));
		_userContext.UpdateFromSource();
		_serverContext.UpdateFromSource();
		PushAuthorizationData data = GetAuthorizationData();
		try
		{
			await _webSocket.State.Client.Start(data);
		}
		catch (Exception)
		{
			_connecting = false;
			throw;
		}
		_connecting = false;
		_logger?.Log(HydraLogType.Message, this.GetLogCatMsg(), "WebSocket connected with generation {0}, Versions: {1}", data.Generation, data.Versions);
		_pushToken.Update(new PushTokenWrapper(_webSocket.State.Client.Token));
	}

	public async Task Disconnect()
	{
		await _webSocket.State.Client.Stop();
		WebSocketClient client = _webSocket.State.Client;
		client.OnMessageReceived = (WebSocketClient.MessageReceivedDelegate)Delegate.Remove(client.OnMessageReceived, new WebSocketClient.MessageReceivedDelegate(OnPushMessage));
		WebSocketClient client2 = _webSocket.State.Client;
		client2.OnError = (WebSocketClient.OnErrorDelegate)Delegate.Remove(client2.OnError, new WebSocketClient.OnErrorDelegate(OnError));
	}

	public Task Unregister()
	{
		try
		{
			if (_taskReconnect != null && !_taskReconnect.IsCompleted)
			{
				_token.Cancel();
			}
			return Disconnect();
		}
		catch (Exception ex)
		{
			_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Shutdown exception: {0}" + ex.Message);
		}
		return Task.CompletedTask;
	}

	private void SdkStateChange(SdkState oldState, SdkState newState)
	{
		if (newState.State == OnlineState.Online)
		{
			if (_webSocket.State.Client.State != WebSocketState.Open)
			{
				_logger?.Log(HydraLogType.Information, this.GetLogCatInf(), $"Detected state: {newState.State}, connecting...");
				Connect().Wait();
			}
		}
		else if (_webSocket.State.Client != null)
		{
			_logger?.Log(HydraLogType.Information, this.GetLogCatInf(), $"Detected state: {newState.State}, disconnecting...");
			Disconnect().Wait();
		}
	}

	private void OnError(HydraSdkException ex)
	{
		if (!_reconnecting)
		{
			_reconnecting = true;
			_taskReconnect = Task.Run((Func<Task>)Reconnect, _token.Token);
		}
		OnPushError?.Invoke(ex);
	}

	private async Task Reconnect()
	{
		int limit = 60;
		double initialValue = 0.5;
		int attempt = 0;
		int attemptsLimit = 30;
		while (_reconnecting)
		{
			try
			{
				if (_webSocket.State.Client.IsConnected)
				{
					if (attempt < attemptsLimit)
					{
						attempt++;
					}
					_logger.Log(HydraLogType.Warning, this.GetLogCatWrn(), "[{0}] Reconnecting...", attempt);
					await Disconnect();
				}
				double delay = initialValue + (double)limit / Math.Pow(2.0, attemptsLimit - attempt);
				await Task.Delay(TimeSpan.FromSeconds(delay));
				if (!_webSocket.State.Client.IsConnected)
				{
					await Connect();
					_reconnecting = false;
					attempt = 0;
				}
			}
			catch (Exception err)
			{
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "[{0}] Reconnecting failed: {1}", attempt, err.GetErrorMessage());
			}
		}
	}

	private void OnPushMessage(WebSocketMessage msg)
	{
		OnPushMessageReceived?.Invoke(msg);
	}

	private PushAuthorizationData GetAuthorizationData()
	{
		PushAuthorizationData pushAuthorizationData = new PushAuthorizationData
		{
			Generation = ++_generation,
			Versions = { _webSocket.State.Client.GetVersions() }
		};
		if (_userContext.State?.Context != null)
		{
			pushAuthorizationData.UserContext = _userContext.State.Context;
		}
		else
		{
			if (_serverContext.State?.Context == null)
			{
				throw new HydraSdkException(ErrorCode.SdkNotInitialized, "The client context is null, perform authorization first.");
			}
			pushAuthorizationData.ServerContext = _serverContext.State.Context;
		}
		if (_pushToken.State?.Token != null)
		{
			pushAuthorizationData.Token = _pushToken.State.Token;
		}
		return pushAuthorizationData;
	}
}
