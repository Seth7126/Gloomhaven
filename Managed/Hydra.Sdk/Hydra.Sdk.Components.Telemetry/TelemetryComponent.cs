using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf;
using Hydra.Api.Telemetry;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Components.Telemetry.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Telemetry;

public sealed class TelemetryComponent : IHydraSdkComponent
{
	private TelemetryApi.TelemetryApiClient _api;

	private IHydraSdkLogger _logger;

	private StateObserver<SdkState> _sdkState;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	private StateObserver<ToolContextWrapper> _toolContext;

	private IConnectionManager _connectionManager;

	private TelemetryEventWriter _eventWriter;

	private ConcurrentQueue<TelemetryPack> _packsQueue;

	private bool _isBusy;

	public const int TELEMETRY_EVENTS_BUFFER_SIZE_BYTES = 2000000;

	public const int TELEMETRY_EVENTS_BUFFER_TIMEOUT_SECONDS = 15;

	public const bool TELEMETRY_COMPRESSION_ENABLED = true;

	public bool CanSendTelemetryPacks => _api != null && _sdkState.State.State == OnlineState.Online && !_sdkState.State.Suspended && !_isBusy;

	public bool HasTelemetryPacks => !_packsQueue.IsEmpty;

	public int GetDisposePriority()
	{
		return 240;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_logger = logger;
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		_sessionInfo = stateResolver.CreateLinkedObserver<SdkSessionInfo>();
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_toolContext = stateResolver.CreateLinkedObserver<ToolContextWrapper>();
		StateObserver<UserContextWrapper> userContext = _userContext;
		userContext.OnStateUpdate = (StateObserver<UserContextWrapper>.StateUpdate)Delegate.Combine(userContext.OnStateUpdate, new StateObserver<UserContextWrapper>.StateUpdate(UserContextUpdate));
		StateObserver<ServerContextWrapper> serverContext = _serverContext;
		serverContext.OnStateUpdate = (StateObserver<ServerContextWrapper>.StateUpdate)Delegate.Combine(serverContext.OnStateUpdate, new StateObserver<ServerContextWrapper>.StateUpdate(ServerContextUpdate));
		StateObserver<ToolContextWrapper> toolContext = _toolContext;
		toolContext.OnStateUpdate = (StateObserver<ToolContextWrapper>.StateUpdate)Delegate.Combine(toolContext.OnStateUpdate, new StateObserver<ToolContextWrapper>.StateUpdate(ToolContextUpdate));
		_connectionManager = connectionManager;
		_packsQueue = new ConcurrentQueue<TelemetryPack>();
		_eventWriter = new TelemetryEventWriter(_sdkState, _sessionInfo, 2000000, TimeSpan.FromSeconds(15.0), FlushTelemetryEvents, logger);
		CheckApi();
		return Task.CompletedTask;
	}

	private void CheckApi(bool ignoreState = false)
	{
		if (_api != null)
		{
			return;
		}
		if (!ignoreState)
		{
			SdkState state = _sdkState.State;
			if (state == null || state.State != OnlineState.Online)
			{
				return;
			}
		}
		_api = _connectionManager.GetConnection<TelemetryApi.TelemetryApiClient>();
	}

	private void ToolContextUpdate(ToolContextWrapper oldState, ToolContextWrapper newState)
	{
		CheckApi(ignoreState: true);
	}

	private void ServerContextUpdate(ServerContextWrapper oldState, ServerContextWrapper newState)
	{
		CheckApi(ignoreState: true);
	}

	private void UserContextUpdate(UserContextWrapper oldState, UserContextWrapper newState)
	{
		CheckApi(ignoreState: true);
	}

	private Task<bool> FlushTelemetryEvents(TelemetryPack pack)
	{
		_packsQueue.Enqueue(pack);
		return Task.FromResult(result: true);
	}

	public async Task<bool> TrySendTelemetryPack(bool forceFlush = false)
	{
		if (!CanSendTelemetryPacks || _isBusy)
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
					SendTelemetryPackRequest request = new SendTelemetryPackRequest
					{
						UserContext = _userContext.State.Context,
						Header = pack.Header,
						Data = ByteString.CopyFrom(pack.Data),
						Entries = ByteString.CopyFrom(pack.Entries),
						EventGeneration = 2
					};
					await _api.SendTelemetryPackAsync(request);
				}
				isSuccess = true;
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.Log(HydraLogType.Error, this.GetLogCatErr() + "/SendTelemetryPack", "{0}", ex2.GetErrorMessage());
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

	public bool SubmitEvent(string eventType, int eventTypeVersion, string jsonParams)
	{
		try
		{
			TelemetryEventBaseEntry rawEventData = new TelemetryEventBaseEntry
			{
				EventType = eventType,
				EventUid = Guid.NewGuid().ToString(),
				Version = eventTypeVersion,
				JsonParams = jsonParams
			};
			_eventWriter.WriteEvent(rawEventData);
			return true;
		}
		catch (Exception err)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Failed to send {0} event: {1}", eventType, err.GetErrorMessage());
		}
		return false;
	}

	public Task Unregister()
	{
		return _eventWriter.Shutdown();
	}
}
