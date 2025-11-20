using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.Channels;
using Hydra.Sdk.Communication.Channels.Unity;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Helpers;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk;

public class HydraSdk
{
	public delegate void SdkStateDelegate(SdkState state);

	public SdkStateDelegate OnStateChanged;

	private bool _isDisposed;

	private HydraSdkSettings _settings;

	private ConcurrentDictionary<Type, IHydraSdkComponent> _components;

	private IChannelManager _channelManager;

	private IConnectionManager _connectionManager;

	private ComponentMessager _messageHandler;

	private StateResolver _stateResolver;

	private InternalLogger _logger;

	private StateObserver<SdkState> _sdkState;

	public OnlineState State => _sdkState.State.State;

	public bool IsNetworkSuspended => _sdkState.State.Suspended;

	public HydraSdk(HydraSdkSettings settings, IHydraSdkLogger debugLogger = null)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		_settings = settings;
		_logger = new InternalLogger();
		if (debugLogger != null)
		{
			InternalLogger logger = _logger;
			logger.OnLog = (InternalLogger.LogMessageDelegate)Delegate.Combine(logger.OnLog, new InternalLogger.LogMessageDelegate(debugLogger.Log));
		}
		_channelManager = new ChannelManager((ChannelCreationData data) => new HydraUnityChannel(data), _logger);
		_connectionManager = new ConnectionManager(_channelManager);
		_components = new ConcurrentDictionary<Type, IHydraSdkComponent>();
		_messageHandler = new ComponentMessager();
		ComponentMessager messageHandler = _messageHandler;
		messageHandler.OnMessageReceived = (ComponentMessager.MessageReceived)Delegate.Combine(messageHandler.OnMessageReceived, new ComponentMessager.MessageReceived(HandleComponentMessage));
		_stateResolver = new StateResolver();
		_stateResolver.Register(new ClientInfo(_settings));
		_stateResolver.Register(new SdkSessionInfo());
		_stateResolver.Register(new SdkState());
		_sdkState = _stateResolver.CreateLinkedObserver<SdkState>();
		StateObserver<SdkState> sdkState = _sdkState;
		sdkState.OnStateUpdate = (StateObserver<SdkState>.StateUpdate)Delegate.Combine(sdkState.OnStateUpdate, new StateObserver<SdkState>.StateUpdate(SdkStateUpdated));
		ServiceHelper.RegisterServices();
		_isDisposed = false;
		_logger?.Log(HydraLogType.Information, this.GetLogCatInf(), "{0} C# SDK {1} initialized", "Hydra", ServiceHelper.SdkVersion);
	}

	public void DisableNetworkInBackground(bool value)
	{
		DisposeCheck();
		_sdkState.Update(new SdkState(_sdkState.State.State, value));
	}

	public void AddEndpoint(string name, Uri uri)
	{
		DisposeCheck();
		_channelManager.AddEndpoint(name, uri);
	}

	public T Call<T>() where T : IHydraSdkComponent, new()
	{
		DisposeCheck();
		if (_settings == null)
		{
			throw new HydraSdkException(ErrorCode.SdkNotInitialized, "Error: SDK isn't initialized");
		}
		if (!_components.TryGetValue(typeof(T), out var value))
		{
			if (_settings.ManualComponentsHandling)
			{
				throw new HydraSdkException(ErrorCode.SdkInvalidState, "Component '" + typeof(T).Name + "' isn't registered. Register it manually or enable auto registration in SDK settings.");
			}
			value = RegisterComponent<T>().Result;
		}
		return (T)value;
	}

	public bool IsComponentRegistered<T>() where T : IHydraSdkComponent
	{
		DisposeCheck();
		if (!_components.ContainsKey(typeof(T)))
		{
			return false;
		}
		return true;
	}

	public async Task<IHydraSdkComponent> RegisterComponent<T>() where T : IHydraSdkComponent, new()
	{
		DisposeCheck();
		Type type = typeof(T);
		if (IsComponentRegistered<T>())
		{
			if (_settings.ManualComponentsHandling)
			{
				throw new HydraSdkException(ErrorCode.SdkInvalidState, "Component '" + type.Name + "' is already registered");
			}
			return _components[type];
		}
		return await AddAndRegister(typeof(T), new T());
	}

	public async Task UnregisterComponent<T>() where T : IHydraSdkComponent
	{
		DisposeCheck();
		Type type = typeof(T);
		if (_components.TryGetValue(type, out var component))
		{
			await Unregister(component);
		}
		else if (_settings.ManualComponentsHandling)
		{
			throw new HydraSdkException(ErrorCode.SdkInternalError, "Component '" + type.Name + "' isn't registered");
		}
	}

	public async Task DisposeAsync()
	{
		try
		{
			if (_isDisposed)
			{
				return;
			}
			_logger?.Log(HydraLogType.Message, this.GetLogCatMsg(), "Disposing SDK client...");
			IOrderedEnumerable<KeyValuePair<Type, IHydraSdkComponent>> componentsByDisposePriority = _components.OrderBy((KeyValuePair<Type, IHydraSdkComponent> o) => o.Value.GetDisposePriority());
			foreach (KeyValuePair<Type, IHydraSdkComponent> item in componentsByDisposePriority)
			{
				await Unregister(item.Value);
			}
			await _channelManager.ShutdownChannels();
			_connectionManager = null;
			_logger?.Log(HydraLogType.Message, this.GetLogCatMsg(), "SDK client disposed");
		}
		finally
		{
			_isDisposed = true;
		}
	}

	private void DisposeCheck()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("SDK");
		}
	}

	private void SdkStateUpdated(SdkState oldState, SdkState newState)
	{
		_logger.Log(HydraLogType.Information, this.GetLogCatInf(), "State change: {0} -> {1}", oldState?.State, newState?.State);
		if (newState.State == OnlineState.Offline)
		{
			_channelManager.UpdateToken(string.Empty);
		}
		OnStateChanged?.Invoke(newState);
	}

	private async Task Unregister(IHydraSdkComponent component)
	{
		Type type = component.GetType();
		_logger?.Log(HydraLogType.Message, this.GetLogCatMsg(), "Unregistering component: {0}", type.Name);
		try
		{
			await component.Unregister();
		}
		catch (Exception err)
		{
			_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Failed to unregister component: {0}, {1}", type.Name, err.GetErrorMessage());
		}
		_components.TryRemove(type, out var _);
	}

	private async Task<IHydraSdkComponent> AddAndRegister(Type type, IHydraSdkComponent component)
	{
		try
		{
			await component.Register(_connectionManager, _messageHandler, _stateResolver, _logger);
			_components.TryAdd(type, component);
			_logger?.Log(HydraLogType.Message, this.GetLogCatMsg(), "Registered component: {0}", type.Name);
			return component;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			throw new HydraSdkException(ErrorCode.SdkInternalError, "Failed to register '" + type.Name + "' component, check inner exception for more info", ex2);
		}
	}

	private async Task HandleComponentMessage(ComponentMessage msg)
	{
		if (msg.Message == MessageType.RegisterComponent)
		{
			if (msg.Args == null)
			{
				return;
			}
			object[] args = msg.Args;
			foreach (object arg in args)
			{
				try
				{
					Type type = (Type)arg;
					await AddAndRegister(type, (IHydraSdkComponent)Activator.CreateInstance(type));
				}
				catch (Exception)
				{
				}
			}
		}
		else
		{
			if (msg.Message != MessageType.UnregisterComponent || msg.Args == null)
			{
				return;
			}
			object[] args2 = msg.Args;
			foreach (object arg2 in args2)
			{
				Type type2 = (Type)arg2;
				if (_components.ContainsKey(type2))
				{
					try
					{
						await _components[type2].Unregister();
						_components.TryRemove(type2, out var _);
						_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Unregistered by requested: {0}", type2.Name);
					}
					catch (Exception)
					{
					}
				}
			}
		}
	}
}
