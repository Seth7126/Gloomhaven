using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Components.SessionControl.Core;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.SessionControl;

public sealed class SessionControlComponent : IHydraSdkComponent
{
	private InternalLogger _logger;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<GameSessionWrapper> _gameSession;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private StateObserver<ClientInfo> _clientInfo;

	private StateObserver<ServerContextWrapper> _serverContext;

	private StateObserver<StandaloneServerWrapper> _standaloneInfo;

	public SCClient Client { get; private set; }

	public SCServer Server { get; private set; }

	public SCBrowsing ServerBrowsing { get; private set; }

	public string GameSessionId => _gameSession?.State?.GameSessionId;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_logger = logger as InternalLogger;
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_gameSession = stateResolver.CreateLinkedObserver<GameSessionWrapper>();
		_sessionInfo = stateResolver.CreateLinkedObserver<SdkSessionInfo>();
		_standaloneInfo = stateResolver.CreateLinkedObserver<StandaloneServerWrapper>();
		_clientInfo = stateResolver.CreateLinkedObserver<ClientInfo>();
		SessionControlClientApi.SessionControlClientApiClient connection = connectionManager.GetConnection<SessionControlClientApi.SessionControlClientApiClient>();
		Client = new SCClient(connection, _logger, _userContext, _gameSession, _clientInfo);
		SessionControlServerApi.SessionControlServerApiClient connection2 = connectionManager.GetConnection<SessionControlServerApi.SessionControlServerApiClient>();
		Server = new SCServer(connection2, _logger, _serverContext, _sessionInfo, _gameSession);
		SessionControlServerBrowsingApi.SessionControlServerBrowsingApiClient connection3 = connectionManager.GetConnection<SessionControlServerBrowsingApi.SessionControlServerBrowsingApiClient>();
		ServerBrowsing = new SCBrowsing(connection3, _logger, _userContext, _serverContext, _standaloneInfo, _clientInfo);
		return Task.CompletedTask;
	}

	public Task Unregister()
	{
		List<Task> list = new List<Task>();
		list.Add(Client.Shutdown());
		list.Add(Server.Shutdown());
		return list.Any() ? Task.WhenAll(list) : Task.CompletedTask;
	}
}
