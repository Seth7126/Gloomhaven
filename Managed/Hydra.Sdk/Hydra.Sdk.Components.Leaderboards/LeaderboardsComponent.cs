using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.Leaderboards;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Leaderboards;

public sealed class LeaderboardsComponent : IHydraSdkComponent
{
	private LeaderboardsApi.LeaderboardsApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_api = connectionManager.GetConnection<LeaderboardsApi.LeaderboardsApiClient>();
		return Task.CompletedTask;
	}

	public async Task<GetLeaderboardsResponse> GetLeaderboards(IEnumerable<LeaderboardRequest> requests)
	{
		return await _api.GetLeaderboardsAsync(new GetLeaderboardsRequest
		{
			Context = _userContext.State.Context,
			Requests = { requests }
		});
	}

	public async Task<GetLeaderboardFilteredResponse> GetLeaderboardFiltered(string leaderBoardId, IEnumerable<string> userIds)
	{
		return await _api.GetLeaderboardFilteredAsync(new GetLeaderboardFilteredRequest
		{
			Context = _userContext.State.Context,
			LeaderboardId = leaderBoardId,
			UserIds = { userIds }
		});
	}

	public async Task<UpdateLeaderboardUserResponse> UpdateLeaderboardUser(string leaderBoardId, IEnumerable<UpdateEntry> entries)
	{
		return await _api.UpdateLeaderboardUserAsync(new UpdateLeaderboardUserRequest
		{
			Context = _userContext.State.Context,
			LeaderboardId = leaderBoardId,
			Entries = { entries }
		});
	}

	public async Task<UpdateLeaderboardServerResponse> UpdateLeaderboardServer(string leaderBoardId, IEnumerable<UpdateEntry> entries)
	{
		return await _api.UpdateLeaderboardServerAsync(new UpdateLeaderboardServerRequest
		{
			Context = _serverContext.State.Context,
			LeaderboardId = leaderBoardId,
			Entries = { entries }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
