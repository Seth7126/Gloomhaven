using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.User;

public static class EconomyApi
{
	public class EconomyApiClient : ClientBase<EconomyApiClient>
	{
		private readonly ICaller _caller;

		public EconomyApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ConnectResponse> ConnectAsync(ConnectRequest request)
		{
			return _caller.Execute<ConnectResponse, ConnectRequest>(Descriptor, "Connect", request);
		}

		public Task<DisconnectResponse> DisconnectAsync(DisconnectRequest request)
		{
			return _caller.Execute<DisconnectResponse, DisconnectRequest>(Descriptor, "Disconnect", request);
		}

		public Task<ProcessPlatformEntitlementsResponse> ProcessPlatformEntitlementsAsync(ProcessPlatformEntitlementsRequest request)
		{
			return _caller.Execute<ProcessPlatformEntitlementsResponse, ProcessPlatformEntitlementsRequest>(Descriptor, "ProcessPlatformEntitlements", request);
		}

		public Task<GetUserStatesResponse> GetUserStatesAsync(GetUserStatesRequest request)
		{
			return _caller.Execute<GetUserStatesResponse, GetUserStatesRequest>(Descriptor, "GetUserStates", request);
		}

		public Task<GetTransactionsResponse> GetTransactionsAsync(GetTransactionsRequest request)
		{
			return _caller.Execute<GetTransactionsResponse, GetTransactionsRequest>(Descriptor, "GetTransactions", request);
		}

		public Task<GetTransactionsReverseResponse> GetTransactionsReverseAsync(GetTransactionsReverseRequest request)
		{
			return _caller.Execute<GetTransactionsReverseResponse, GetTransactionsReverseRequest>(Descriptor, "GetTransactionsReverse", request);
		}

		public Task<ApplyOffersResponse> ApplyOffersAsync(ApplyOffersRequest request)
		{
			return _caller.Execute<ApplyOffersResponse, ApplyOffersRequest>(Descriptor, "ApplyOffers", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("EconomyApi", "Hydra.Api.User.EconomyApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 3
	};
}
