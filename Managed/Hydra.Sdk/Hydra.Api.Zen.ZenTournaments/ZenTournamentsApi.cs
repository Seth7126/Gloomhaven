using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Zen.ZenTournaments;

public static class ZenTournamentsApi
{
	public class ZenTournamentsApiClient : ClientBase<ZenTournamentsApiClient>
	{
		private readonly ICaller _caller;

		public ZenTournamentsApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ClientGetTournamentsResponse> ClientGetTournamentsAsync(ClientGetTournamentsRequest request)
		{
			return _caller.Execute<ClientGetTournamentsResponse, ClientGetTournamentsRequest>(Descriptor, "ClientGetTournaments", request);
		}

		public Task<ClientGetTournamentsSortedResponse> ClientGetTournamentsSortedAsync(ClientGetTournamentsSortedRequest request)
		{
			return _caller.Execute<ClientGetTournamentsSortedResponse, ClientGetTournamentsSortedRequest>(Descriptor, "ClientGetTournamentsSorted", request);
		}

		public Task<ClientGetTournamentsByHashResponse> ClientGetTournamentsByHashAsync(ClientGetTournamentsByHashRequest request)
		{
			return _caller.Execute<ClientGetTournamentsByHashResponse, ClientGetTournamentsByHashRequest>(Descriptor, "ClientGetTournamentsByHash", request);
		}

		public Task<ClientGetCreatedTournamentsResponse> ClientGetCreatedTournamentsAsync(ClientGetCreatedTournamentsRequest request)
		{
			return _caller.Execute<ClientGetCreatedTournamentsResponse, ClientGetCreatedTournamentsRequest>(Descriptor, "ClientGetCreatedTournaments", request);
		}

		public Task<ClientCreateTournamentResponse> ClientCreateTournamentAsync(ClientCreateTournamentRequest request)
		{
			return _caller.Execute<ClientCreateTournamentResponse, ClientCreateTournamentRequest>(Descriptor, "ClientCreateTournament", request);
		}

		public Task<ClientPostTournamentResultResponse> ClientPostTournamentResultAsync(ClientPostTournamentResultRequest request)
		{
			return _caller.Execute<ClientPostTournamentResultResponse, ClientPostTournamentResultRequest>(Descriptor, "ClientPostTournamentResult", request);
		}

		public Task<ClientPostTournamentAttemptIncrementResponse> ClientPostTournamentAttemptIncrementAsync(ClientPostTournamentAttemptIncrementRequest request)
		{
			return _caller.Execute<ClientPostTournamentAttemptIncrementResponse, ClientPostTournamentAttemptIncrementRequest>(Descriptor, "ClientPostTournamentAttemptIncrement", request);
		}

		public Task<ClientPostTournamentAttemptResultResponse> ClientPostTournamentAttemptResultAsync(ClientPostTournamentAttemptResultRequest request)
		{
			return _caller.Execute<ClientPostTournamentAttemptResultResponse, ClientPostTournamentAttemptResultRequest>(Descriptor, "ClientPostTournamentAttemptResult", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("ZenTournamentsApi", "Hydra.Api.Zen.ZenTournaments.ZenTournamentsApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
