using System;
using System.Linq;
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

public abstract class APushListener
{
	public delegate void DisconnectDelegate(MessageReason reason);

	protected delegate void OnTokenUpdateDelegate(PushToken pushToken);

	protected delegate Task OnPushDisconnectDelegate(MessageReason reason);

	public DisconnectDelegate OnDisconnect;

	protected OnTokenUpdateDelegate OnTokenUpdate;

	protected OnPushDisconnectDelegate OnPushDisconnect;

	protected ComponentMessager _componentMessager;

	protected IHydraSdkLogger _logger;

	protected StateObserver<ClientInfo> _clientInfo;

	protected StateObserver<PushTokenWrapper> _pushToken;

	protected StateObserver<WebSocketWrapper> _webSocket;

	protected StateObserver<UserContextWrapper> _userContext;

	protected StateObserver<ServerContextWrapper> _serverContext;

	public bool IsConnected { get; private set; }

	public int GetDisposePriority()
	{
		return 199;
	}

	protected Task InitializePush(ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_componentMessager = componentMessager;
		_logger = logger;
		_clientInfo = stateResolver.CreateLinkedObserver<ClientInfo>();
		_pushToken = stateResolver.CreateLinkedObserver<PushTokenWrapper>();
		_webSocket = stateResolver.CreateLinkedObserver<WebSocketWrapper>();
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		ComponentMessager componentMessager2 = _componentMessager;
		componentMessager2.OnMessageReceived = (ComponentMessager.MessageReceived)Delegate.Combine(componentMessager2.OnMessageReceived, new ComponentMessager.MessageReceived(ComponentMessageReceived));
		StateObserver<PushTokenWrapper> pushToken = _pushToken;
		pushToken.OnStateUpdate = (StateObserver<PushTokenWrapper>.StateUpdate)Delegate.Combine(pushToken.OnStateUpdate, new StateObserver<PushTokenWrapper>.StateUpdate(PushTokenUpdated));
		return Task.CompletedTask;
	}

	protected virtual void PushMessageReceived(WebSocketMessage msg)
	{
		throw new NotImplementedException();
	}

	protected async Task<T> ConnectInternal<T>(Func<PushToken, Task<T>> func)
	{
		if (_pushToken.State?.Token == null)
		{
			if (_clientInfo.State.ManualComponentsHandling)
			{
				throw new HydraSdkException(ErrorCode.SdkNotInitialized, "'Push' component connection is required");
			}
			ComponentMessager componentMessager = _componentMessager;
			ComponentMessage componentMessage = new ComponentMessage
			{
				Message = MessageType.RegisterComponent
			};
			object[] args = new Type[1] { typeof(PushComponent) };
			componentMessage.Args = args;
			await componentMessager.BroadcastMessage(componentMessage);
		}
		int pushTimeoutInSeconds = 15;
		DateTime deadline = DateTime.UtcNow.AddSeconds(pushTimeoutInSeconds);
		while (_pushToken.State?.Token == null)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new HydraSdkException(ErrorCode.SdkTimeout, $"Failed to register 'Push' component within {pushTimeoutInSeconds}s timeout.");
			}
			await Task.Delay(100);
		}
		T result = default(T);
		if (func != null)
		{
			result = await func(_pushToken.State?.Token);
		}
		IsConnected = true;
		WebSocketClient client = _webSocket.State.Client;
		client.OnMessageReceived = (WebSocketClient.MessageReceivedDelegate)Delegate.Combine(client.OnMessageReceived, new WebSocketClient.MessageReceivedDelegate(PushMessageReceived));
		return result;
	}

	protected async Task<T> DisconnectInternal<T>(Func<Task<T>> func, MessageReason reason)
	{
		if (_webSocket.State?.Client != null)
		{
			WebSocketClient client = _webSocket.State.Client;
			client.OnMessageReceived = (WebSocketClient.MessageReceivedDelegate)Delegate.Remove(client.OnMessageReceived, new WebSocketClient.MessageReceivedDelegate(PushMessageReceived));
		}
		if (reason == MessageReason.Error)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Received fatal error, disconnecting.");
		}
		IsConnected = false;
		try
		{
			if (reason != MessageReason.User)
			{
				OnDisconnect?.Invoke(reason);
			}
		}
		catch (Exception ex)
		{
			_logger?.LogException(ErrorCode.SdkInternalError, this.GetLogCatErr(), ex.GetErrorMessage(), ex);
		}
		T result = default(T);
		if (func != null)
		{
			result = await func();
		}
		return result;
	}

	protected string GetClientId()
	{
		if (_userContext.State?.Context != null)
		{
			return _userContext.State?.Context.Data.UserIdentity;
		}
		if (_serverContext.State?.Context != null)
		{
			return _serverContext.State.Context.Data.KernelSessionId;
		}
		throw new HydraSdkException(ErrorCode.SdkNotInitialized, "The client context is null, perform authorization first.");
	}

	private void PushTokenUpdated(PushTokenWrapper oldState, PushTokenWrapper newState)
	{
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Received 'Push' token: {0}", newState?.Token);
		if (oldState?.Token?.UserKey != newState?.Token?.UserKey && IsConnected)
		{
			OnTokenUpdate?.Invoke(newState.Token);
		}
	}

	private async Task ComponentMessageReceived(ComponentMessage msg)
	{
		if (msg.Message == MessageType.UnregisterComponent)
		{
			Type type = (Type)msg.Args.FirstOrDefault();
			if (type == typeof(PushComponent) && OnPushDisconnect != null)
			{
				await OnPushDisconnect(msg.Reason);
			}
		}
	}
}
