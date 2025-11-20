using System;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.SessionControl.Core;

public sealed class SCClient
{
	public delegate Task ServerInfoDelegate(AcceptClientResult acceptResult);

	public delegate void ServerInfoTimeoutDelegate();

	public delegate void ServerInfoErrorDelegate(Exception ex);

	private readonly SessionControlClientApi.SessionControlClientApiClient _api;

	private readonly IHydraSdkLogger _logger;

	private readonly StateObserver<UserContextWrapper> _userContext;

	private readonly StateObserver<GameSessionWrapper> _gameSession;

	private readonly StateObserver<ClientInfo> _clientInfo;

	private Task _pollingTask;

	private readonly CancellationTokenSource _tokenSource;

	private bool _isSessionActive;

	public ServerInfoDelegate OnServerInfoReceived;

	public ServerInfoTimeoutDelegate OnServerInfoTimeout;

	public ServerInfoErrorDelegate OnServerInfoError;

	public TimeSpan GetServerInfoTimeout { get; set; } = TimeSpan.FromMinutes(5.0);

	public SCClient(SessionControlClientApi.SessionControlClientApiClient api, IHydraSdkLogger logger, StateObserver<UserContextWrapper> userContext, StateObserver<GameSessionWrapper> gameSession, StateObserver<ClientInfo> clientInfo)
	{
		_api = api;
		_logger = logger;
		_tokenSource = new CancellationTokenSource();
		_userContext = userContext;
		_gameSession = gameSession;
		_clientInfo = clientInfo;
		StateObserver<GameSessionWrapper> gameSession2 = _gameSession;
		gameSession2.OnStateUpdate = (StateObserver<GameSessionWrapper>.StateUpdate)Delegate.Combine(gameSession2.OnStateUpdate, new StateObserver<GameSessionWrapper>.StateUpdate(GameSessionUpdate));
	}

	private void GameSessionUpdate(GameSessionWrapper oldState, GameSessionWrapper newState)
	{
		if (_userContext.State?.Context == null)
		{
			return;
		}
		if (_isSessionActive)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Game session is already running");
		}
		if (!(newState?.GameSessionId != oldState?.GameSessionId) || !string.IsNullOrEmpty(newState.GameSessionId))
		{
			return;
		}
		_isSessionActive = false;
		if (_pollingTask.IsCompleted)
		{
			return;
		}
		try
		{
			_tokenSource.Cancel();
		}
		catch (OperationCanceledException)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Received empty game session before getting session information");
		}
	}

	public void StartUnmanagedServerInfoPending(string gameSessionId)
	{
		if (_isSessionActive)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "Game session is already running");
		}
		_pollingTask = Task.Run(async delegate
		{
			await GetServerInformation(async () => await _api.GetServerInfoAsync(new GetServerInfoRequest
			{
				UserContext = _userContext.State.Context,
				GameSessionId = gameSessionId
			}), (GetServerInfoResponse response) => response?.AcceptClientResult != null && response.AcceptClientResult.Status != AcceptStatus.Pending, (GetServerInfoResponse response) => response.AcceptClientResult);
		}, _tokenSource.Token);
	}

	public void StartManagedServerInfoPending(string gameSessionId, string sessionControlManagedContext)
	{
		if (_isSessionActive)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "Game session is already running");
		}
		_pollingTask = Task.Run(async delegate
		{
			await GetServerInformation(async () => await _api.ManagedGetServerInfoAsync(new ManagedGetServerInfoRequest
			{
				UserContext = _userContext.State.Context,
				GameSessionId = gameSessionId,
				SCContext = sessionControlManagedContext
			}), (ManagedGetServerInfoResponse response) => response.AcceptClientResult != null && response.AcceptClientResult.Status != AcceptStatus.Pending, (ManagedGetServerInfoResponse response) => response.AcceptClientResult);
		}, _tokenSource.Token);
	}

	public void StopServerInfoPending()
	{
		_tokenSource.Cancel();
	}

	public async Task<CreateSessionResponse> CreateSession(string dataCenterId, string serverData)
	{
		CreateSessionResponse response = await _api.CreateSessionAsync(new CreateSessionRequest
		{
			ClientVersion = _clientInfo.State.ClientVersion,
			UserContext = _userContext.State.Context,
			ServerData = serverData,
			DataCenterId = dataCenterId
		});
		_gameSession.Update(new GameSessionWrapper(response.GameSessionId));
		return response;
	}

	private async Task GetServerInformation<T>(Func<Task<T>> funcGet, Func<T, bool> funcValidate, Func<T, AcceptClientResult> funcReturn)
	{
		TimeSpan waitTime = TimeSpan.FromSeconds(6.0);
		DateTime endTime = DateTime.UtcNow.Add(GetServerInfoTimeout);
		_isSessionActive = true;
		try
		{
			T response;
			do
			{
				if (DateTime.UtcNow > endTime)
				{
					OnServerInfoTimeout?.Invoke();
					return;
				}
				response = await funcGet();
				await Task.Delay(waitTime);
			}
			while (!funcValidate(response));
			if (OnServerInfoReceived != null)
			{
				await OnServerInfoReceived(funcReturn(response));
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogException(ErrorCode.SdkInternalError, this.GetLogCatErr(), "Error retrieving server information, session canceled: " + ex2.GetErrorMessage());
			OnServerInfoError?.Invoke(ex2);
		}
	}

	public async Task<GetDataCenterEchoEndpointsResponse> GetDataCenterEchoEndpoints(string clientVersion = null)
	{
		return await _api.GetDataCenterEchoEndpointsV2Async(new GetDataCenterEchoEndpointsV2Request
		{
			ClientVersion = (clientVersion ?? string.Empty),
			UserContext = _userContext.State.Context
		});
	}

	public Task Shutdown()
	{
		try
		{
			if (_pollingTask != null && !_pollingTask.IsCompleted)
			{
				_tokenSource.Cancel();
			}
		}
		catch
		{
		}
		return Task.CompletedTask;
	}
}
