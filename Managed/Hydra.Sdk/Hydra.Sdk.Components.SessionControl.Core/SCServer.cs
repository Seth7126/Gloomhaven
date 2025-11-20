using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Hydra.Api.Errors;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.SessionControl.Core;

public sealed class SCServer
{
	private readonly SessionControlServerApi.SessionControlServerApiClient _api;

	private readonly IHydraSdkLogger _logger;

	private readonly StateObserver<ServerContextWrapper> _serverContext;

	private readonly StateObserver<SdkSessionInfo> _sessionInfo;

	private readonly StateObserver<GameSessionWrapper> _gameSession;

	private bool _isSessionActive;

	private Task _pollingTask;

	private readonly CancellationTokenSource _tokenSource;

	public ConcurrentDictionary<string, ServerUserContext> KnownKeys { get; private set; }

	public Func<SessionMemberEvent, AcceptStatus> UserValidation { get; set; }

	public SCServer(SessionControlServerApi.SessionControlServerApiClient api, IHydraSdkLogger logger, StateObserver<ServerContextWrapper> serverContext, StateObserver<SdkSessionInfo> sessionInfo, StateObserver<GameSessionWrapper> gameSession)
	{
		_api = api;
		_logger = logger;
		_tokenSource = new CancellationTokenSource();
		_serverContext = serverContext;
		_sessionInfo = sessionInfo;
		_gameSession = gameSession;
		StateObserver<GameSessionWrapper> gameSession2 = _gameSession;
		gameSession2.OnStateUpdate = (StateObserver<GameSessionWrapper>.StateUpdate)Delegate.Combine(gameSession2.OnStateUpdate, new StateObserver<GameSessionWrapper>.StateUpdate(GameSessionUpdate));
	}

	private void StopSession()
	{
		_isSessionActive = false;
		if (!_pollingTask.IsCompleted)
		{
			try
			{
				_tokenSource.Cancel();
			}
			catch (OperationCanceledException)
			{
			}
		}
	}

	private void GameSessionUpdate(GameSessionWrapper oldState, GameSessionWrapper newState)
	{
		if (_serverContext.State?.Context == null)
		{
			return;
		}
		if (_isSessionActive)
		{
			throw new HydraSdkException(ErrorCode.SdkInternalError, "Game session is already running");
		}
		if (!(newState?.GameSessionId != oldState?.GameSessionId))
		{
			return;
		}
		if (newState?.GameSessionId != null)
		{
			_pollingTask = Task.Run(async delegate
			{
				await GetSessionMemberEvents();
			}, _tokenSource.Token);
		}
		else if (string.IsNullOrEmpty(newState?.GameSessionId))
		{
			StopSession();
		}
	}

	public async Task<ActivateSessionResponse> ActivateSession(string connectionInfo, string token, string serverProperty = "")
	{
		if (UserValidation == null)
		{
			throw new HydraSdkException(ErrorCode.SdkNotInitialized, "Error: 'ServerUserValidation' hasn't been defined.");
		}
		if (_serverContext.State?.Context != null)
		{
			throw new HydraSdkException(ErrorCode.SdkNotInitialized, "Error: session is already activated.");
		}
		KnownKeys = new ConcurrentDictionary<string, ServerUserContext>();
		ActivateSessionResponse response = await _api.ActivateSessionAsync(new ActivateSessionRequest
		{
			ServerInfo = new ServerInfo
			{
				ConnectionInfo = connectionInfo,
				ServerProperty = serverProperty
			},
			ServerToken = token
		});
		_serverContext.Update(new ServerContextWrapper(response.ServerContext));
		_gameSession.Update(new GameSessionWrapper(response.ServerContext.Data.KernelSessionId));
		return response;
	}

	public async Task FinishSession()
	{
		try
		{
			await _api.FinishSessionAsync(new FinishSessionRequest
			{
				ServerContext = _serverContext.State.Context
			});
		}
		finally
		{
			_isSessionActive = false;
		}
	}

	private async Task GetSessionMemberEvents()
	{
		TimeSpan delay = TimeSpan.FromSeconds(5.0);
		_isSessionActive = true;
		long lastEventId = 0L;
		do
		{
			GetSessionMemberEventsResponse response = await _api.GetSessionMemberEventsAsync(new GetSessionMemberEventsRequest
			{
				ServerContext = _serverContext.State.Context,
				LastEventId = lastEventId
			});
			lastEventId = response.LastEventId;
			RepeatedField<SessionMemberEvent> events = response.Events;
			await ProcessSessionMemberEvents(events);
			await Task.Delay(delay);
		}
		while (_isSessionActive);
	}

	private async Task ProcessSessionMemberEvents(IEnumerable<SessionMemberEvent> events)
	{
		IEnumerable<SessionMemberEventResult> preparedEvents = events.Select((SessionMemberEvent sessionMemberEvent) => new SessionMemberEventResult
		{
			Key = sessionMemberEvent.Key,
			Data = new SessionMemberEventResultData
			{
				Status = UserValidation(sessionMemberEvent)
			}
		});
		foreach (SessionMemberEvent e in events)
		{
			KnownKeys.TryAdd(e.Key, e.Data.ServerUserContext);
		}
		await _api.ProcessSessionMemberEventsAsync(new ProcessSessionMemberEventsRequest
		{
			ServerContext = _serverContext.State.Context,
			List = { preparedEvents }
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
