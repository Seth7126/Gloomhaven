using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Leaderboards;

public static class LeaderboardsApi
{
	public class LeaderboardsApiClient : ClientBase<LeaderboardsApiClient>
	{
		private readonly ICaller _caller;

		public LeaderboardsApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetLeaderboardsResponse> GetLeaderboardsAsync(GetLeaderboardsRequest request)
		{
			return _caller.Execute<GetLeaderboardsResponse, GetLeaderboardsRequest>(Descriptor, "GetLeaderboards", request);
		}

		public Task<GetLeaderboardFilteredResponse> GetLeaderboardFilteredAsync(GetLeaderboardFilteredRequest request)
		{
			return _caller.Execute<GetLeaderboardFilteredResponse, GetLeaderboardFilteredRequest>(Descriptor, "GetLeaderboardFiltered", request);
		}

		public Task<UpdateLeaderboardServerResponse> UpdateLeaderboardServerAsync(UpdateLeaderboardServerRequest request)
		{
			return _caller.Execute<UpdateLeaderboardServerResponse, UpdateLeaderboardServerRequest>(Descriptor, "UpdateLeaderboardServer", request);
		}

		public Task<UpdateLeaderboardUserResponse> UpdateLeaderboardUserAsync(UpdateLeaderboardUserRequest request)
		{
			return _caller.Execute<UpdateLeaderboardUserResponse, UpdateLeaderboardUserRequest>(Descriptor, "UpdateLeaderboardUser", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("LeaderboardsApi", "Hydra.Api.Leaderboards.LeaderboardsApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 2
	};
}
