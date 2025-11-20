using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Challenges;

public static class ChallengesApi
{
	public class ChallengesApiClient : ClientBase<ChallengesApiClient>
	{
		private readonly ICaller _caller;

		public ChallengesApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ConnectResponse> ConnectAsync(ConnectRequest request)
		{
			return _caller.Execute<ConnectResponse, ConnectRequest>(Descriptor, "Connect", request);
		}

		public Task<GetChallengesResponse> GetChallengesAsync(GetChallengesRequest request)
		{
			return _caller.Execute<GetChallengesResponse, GetChallengesRequest>(Descriptor, "GetChallenges", request);
		}

		public Task<GetChallengesIncrementalUpdateResponse> GetChallengesIncrementalUpdateAsync(GetChallengesIncrementalUpdateRequest request)
		{
			return _caller.Execute<GetChallengesIncrementalUpdateResponse, GetChallengesIncrementalUpdateRequest>(Descriptor, "GetChallengesIncrementalUpdate", request);
		}

		public Task<ServerSubmitChallengeCountersResponse> ServerSubmitChallengeCountersAsync(ServerSubmitChallengeCountersRequest request)
		{
			return _caller.Execute<ServerSubmitChallengeCountersResponse, ServerSubmitChallengeCountersRequest>(Descriptor, "ServerSubmitChallengeCounters", request);
		}

		public Task<ServerGetChallengesResponse> ServerGetChallengesAsync(ServerGetChallengesRequest request)
		{
			return _caller.Execute<ServerGetChallengesResponse, ServerGetChallengesRequest>(Descriptor, "ServerGetChallenges", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("ChallengesApi", "Hydra.Api.Challenges.ChallengesApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
