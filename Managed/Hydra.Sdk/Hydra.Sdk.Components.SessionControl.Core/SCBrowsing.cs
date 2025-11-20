using System;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Components.SessionControl.Core.ServerBrowsing;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.SessionControl.Core;

public sealed class SCBrowsing
{
	private SessionControlServerBrowsingApi.SessionControlServerBrowsingApiClient _api;

	private IHydraSdkLogger _logger;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	private StateObserver<StandaloneServerWrapper> _standaloneInfo;

	private StateObserver<ClientInfo> _clientInfo;

	private bool _isRunning;

	private ServerParameters _serverParametersCache;

	private Task _heartbeatTask;

	private CancellationTokenSource _tokenSource;

	public ServerParameters ServerParameters { get; }

	public SCBrowsing(SessionControlServerBrowsingApi.SessionControlServerBrowsingApiClient browsingApi, IHydraSdkLogger logger, StateObserver<UserContextWrapper> userContext, StateObserver<ServerContextWrapper> serverContext, StateObserver<StandaloneServerWrapper> standaloneInfo, StateObserver<ClientInfo> clientInfo)
	{
		_tokenSource = new CancellationTokenSource();
		_api = browsingApi;
		_logger = logger;
		_userContext = userContext;
		_serverContext = serverContext;
		_standaloneInfo = standaloneInfo;
		_clientInfo = clientInfo;
		ServerParameters = new ServerParameters();
	}

	public async Task<BrowseServersResponse> BrowseServers(string clientVersion)
	{
		return await _api.BrowseServersAsync(new BrowseServersRequest
		{
			ClientVersion = clientVersion,
			UserContext = _userContext.State.Context
		});
	}

	public async Task<CreateServerResponse> CreateServer(string connectionInfo, string serverProperty)
	{
		ServerBrowsingSessionData sbSessionData = ServerParameters.ToServerBrowsingSessionData();
		sbSessionData.Validate();
		CreateServerResponse response = await _api.CreateServerAsync(new CreateServerRequest
		{
			ServerToken = _standaloneInfo.State.ServerToken,
			ClientVersion = _clientInfo.State.ClientVersion,
			CreateData = sbSessionData,
			ServerInfo = new ServerInfo
			{
				ConnectionInfo = connectionInfo,
				ServerProperty = serverProperty
			}
		});
		_serverParametersCache = ServerParameters.Copy();
		_serverContext.Update(new ServerContextWrapper(response.ServerContext));
		_isRunning = true;
		_heartbeatTask = Task.Run((Func<Task>)HeartbeatServerLoop, _tokenSource.Token);
		return response;
	}

	private async Task HeartbeatServerLoop()
	{
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Heartbeat started");
		int refreshAfterSeconds = 5;
		DateTime nextCall = DateTime.UtcNow.AddSeconds(refreshAfterSeconds);
		while (_isRunning && !_tokenSource.IsCancellationRequested)
		{
			if (DateTime.UtcNow >= nextCall)
			{
				try
				{
					HeartbeatServerData update = new HeartbeatServerData();
					ServerParameters updateData = ServerParameters.GetDifference(_serverParametersCache);
					if (updateData != null)
					{
						update.Data = updateData.ToServerBrowsingSessionData();
					}
					refreshAfterSeconds = (await _api.HeartbeatServerAsync(new HeartbeatServerRequest
					{
						ServerContext = _serverContext.State.Context,
						Update = update
					})).RefreshAfterSeconds;
					_serverParametersCache = ServerParameters.Copy();
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					_logger.Log(HydraLogType.Error, this.GetLogCatErr(), ex2.GetErrorMessage());
				}
				finally
				{
					nextCall = DateTime.UtcNow.AddSeconds(refreshAfterSeconds);
				}
			}
			await Task.Delay(100);
		}
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Heartbeat stopped");
	}

	public async Task<DestroyServerResponse> DestroyServer()
	{
		try
		{
			_isRunning = false;
			await _heartbeatTask;
			DateTime deadline = DateTime.UtcNow.AddSeconds(5.0);
			while (!_heartbeatTask.IsCompleted)
			{
				if (DateTime.UtcNow > deadline)
				{
					_tokenSource.Cancel();
				}
				await Task.Delay(100);
			}
		}
		catch (OperationCanceledException)
		{
			_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Heartbeat stop enforced");
		}
		return await _api.DestroyServerAsync(new DestroyServerRequest
		{
			ServerContext = _serverContext.State.Context
		});
	}
}
