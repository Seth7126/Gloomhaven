using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf;
using Hydra.Api.Facts;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Components.Facts.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Helpers;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Facts;

public sealed class FactsComponent : IHydraSdkComponent
{
	private IConnectionManager _connectionManager;

	private StateObserver<SdkState> _sdkState;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	private StateObserver<ToolContextWrapper> _toolContext;

	private StateObserver<ClientInfo> _clientInfo;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private FactWriter _factWriter;

	private FactsApi.FactsApiClient _api;

	private ConcurrentQueue<FactPack> _packsQueue;

	private bool _isBusy;

	private IHydraSdkLogger _logger;

	public const int FACTS_BUFFER_SIZE_BYTES = 10000000;

	public const int FACTS_BUFFER_TIMEOUT_SECONDS = 60;

	public const bool FACTS_COMPRESSION_ENABLED = true;

	public bool CanSendFactPacks => _api != null && _sdkState.State.State == OnlineState.Online && !_sdkState.State.Suspended && !_isBusy;

	public bool HasFactPacks => !_packsQueue.IsEmpty;

	public int GetDisposePriority()
	{
		return 250;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_logger = logger;
		_connectionManager = connectionManager;
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_toolContext = stateResolver.CreateLinkedObserver<ToolContextWrapper>();
		StateObserver<UserContextWrapper> userContext = _userContext;
		userContext.OnStateUpdate = (StateObserver<UserContextWrapper>.StateUpdate)Delegate.Combine(userContext.OnStateUpdate, new StateObserver<UserContextWrapper>.StateUpdate(UserContextUpdate));
		StateObserver<ServerContextWrapper> serverContext = _serverContext;
		serverContext.OnStateUpdate = (StateObserver<ServerContextWrapper>.StateUpdate)Delegate.Combine(serverContext.OnStateUpdate, new StateObserver<ServerContextWrapper>.StateUpdate(ServerContextUpdate));
		StateObserver<ToolContextWrapper> toolContext = _toolContext;
		toolContext.OnStateUpdate = (StateObserver<ToolContextWrapper>.StateUpdate)Delegate.Combine(toolContext.OnStateUpdate, new StateObserver<ToolContextWrapper>.StateUpdate(ToolContextUpdate));
		_clientInfo = stateResolver.CreateLinkedObserver<ClientInfo>();
		_sessionInfo = stateResolver.CreateLinkedObserver<SdkSessionInfo>();
		_packsQueue = new ConcurrentQueue<FactPack>();
		_factWriter = new FactWriter(10000000, TimeSpan.FromSeconds(60.0), _sessionInfo, _sdkState, FlushTask, _logger);
		_factWriter.UpdateGlobalContext("SDK_VERSION", ServiceHelper.SdkVersion);
		_factWriter.UpdateGlobalContext("CLIENT_VERSION", _clientInfo.State.ClientVersion);
		_factWriter.UpdateGlobalContext("TITLE_ID", _clientInfo.State.TitleId);
		_factWriter.UpdateGlobalContext("ENVIRONMENT_ID", _clientInfo.State.HydraEndpoint);
		_factWriter.UpdateGlobalContext("HTTP_VERSION", "1.1");
		if (_logger is InternalLogger internalLogger)
		{
			internalLogger.OnLog = (InternalLogger.LogMessageDelegate)Delegate.Combine(internalLogger.OnLog, new InternalLogger.LogMessageDelegate(Log));
			internalLogger.OnLogContext = (InternalLogger.LogContextDelegate)Delegate.Combine(internalLogger.OnLogContext, new InternalLogger.LogContextDelegate(SetContext));
		}
		else
		{
			_logger.Log(HydraLogType.Warning, "FactsComponent", "Initialization failed.");
		}
		CheckApi();
		return Task.CompletedTask;
	}

	private void ToolContextUpdate(ToolContextWrapper oldState, ToolContextWrapper newState)
	{
		CheckApi(ignoreState: true);
		if (oldState?.Context?.Data?.KernelSessionId != newState.Context.Data?.KernelSessionId)
		{
			string text = newState.Context.Data?.KernelSessionId;
			_factWriter.UpdateGlobalContext("KERNEL_SESSION_ID", text);
			_factWriter.UpdateGlobalContext("KSIVA", text.Truncate(5));
			_factWriter.UpdateGlobalContext("ROLE", "TOOL");
		}
	}

	private void ServerContextUpdate(ServerContextWrapper oldState, ServerContextWrapper newState)
	{
		CheckApi(ignoreState: true);
		if (oldState?.Context?.Data?.KernelSessionId != newState.Context.Data?.KernelSessionId)
		{
			string text = newState.Context.Data?.KernelSessionId;
			_factWriter.UpdateGlobalContext("KERNEL_SESSION_ID", text);
			_factWriter.UpdateGlobalContext("KSIVA", text.Truncate(5));
			_factWriter.UpdateGlobalContext("ROLE", "GAME_SERVER");
		}
	}

	private void UserContextUpdate(UserContextWrapper oldState, UserContextWrapper newState)
	{
		CheckApi(ignoreState: true);
		if (oldState?.Context?.Data?.KernelSessionId != newState.Context.Data?.KernelSessionId)
		{
			string text = newState.Context.Data?.KernelSessionId;
			_factWriter.UpdateGlobalContext("KERNEL_SESSION_ID", text);
			_factWriter.UpdateGlobalContext("KSIVA", text.Truncate(5));
			_factWriter.UpdateGlobalContext("ROLE", "GAME_CLIENT");
		}
		if (oldState?.Context?.Data?.UserIdentity != newState.Context.Data?.UserIdentity)
		{
			_factWriter.UpdateGlobalContext("USER_ID", newState.Context.Data?.UserIdentity);
		}
	}

	private void CheckApi(bool ignoreState = false)
	{
		if (_api == null && (ignoreState || _sdkState.State.State == OnlineState.Online))
		{
			_api = _connectionManager.GetConnection<FactsApi.FactsApiClient>();
		}
	}

	private Task<bool> FlushTask(FactPack pack)
	{
		_packsQueue.Enqueue(pack);
		return Task.FromResult(result: true);
	}

	public async Task<bool> TrySendFactPack(bool forceFlush = false)
	{
		if (!CanSendFactPacks || _isBusy)
		{
			return false;
		}
		_isBusy = true;
		bool isSuccess = false;
		try
		{
			if (_packsQueue.TryPeek(out var pack))
			{
				if (_userContext.State?.Context != null)
				{
					await _api.WriteBinaryPackUserAsync(new WriteBinaryPackUserRequest
					{
						UserContext = _userContext.State.Context,
						Data = ByteString.CopyFrom(pack.Data),
						Entries = ByteString.CopyFrom(pack.Entries),
						Header = pack.Header
					});
				}
				else if (_serverContext.State?.Context != null)
				{
					await _api.WriteBinaryPackServerAsync(new WriteBinaryPackServerRequest
					{
						ServerContext = _serverContext.State.Context,
						Data = ByteString.CopyFrom(pack.Data),
						Entries = ByteString.CopyFrom(pack.Entries),
						Header = pack.Header
					});
				}
				else if (_toolContext.State?.Context != null)
				{
					await _api.WriteBinaryPackToolAsync(new WriteBinaryPackToolRequest
					{
						ToolContext = _toolContext.State.Context,
						Data = ByteString.CopyFrom(pack.Data),
						Entries = ByteString.CopyFrom(pack.Entries),
						Header = pack.Header
					});
				}
				isSuccess = true;
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.Log(HydraLogType.Error, this.GetLogCatErr() + "/WriteBinaryPack", "{0}", ex2.GetErrorMessage());
		}
		finally
		{
			if (isSuccess || forceFlush)
			{
				_packsQueue.TryDequeue(out var _);
			}
			_isBusy = false;
		}
		return isSuccess;
	}

	public void Log(HydraLogType type, string category, string message, params object[] args)
	{
		if (!string.IsNullOrWhiteSpace(category))
		{
			switch (type)
			{
			case HydraLogType.Message:
				_factWriter.LogMessage(category, message, args);
				break;
			case HydraLogType.Information:
				_factWriter.LogInformation(category, message, args);
				break;
			case HydraLogType.Warning:
				_factWriter.LogWarning(category, message, args);
				break;
			case HydraLogType.Error:
				_factWriter.LogError(category, message, args);
				break;
			}
		}
	}

	public void SetContext(string category, string name, string value)
	{
		if (!string.IsNullOrWhiteSpace(category) && !string.IsNullOrWhiteSpace(name))
		{
			_factWriter.SetContext(category, name, value);
		}
	}

	public void RemoveContext(string category)
	{
		_factWriter.RemoveContext(category);
	}

	public void RemoveAllContext()
	{
		_factWriter.RemoveAllContext();
	}

	public Task Unregister()
	{
		return _factWriter.Shutdown();
	}
}
