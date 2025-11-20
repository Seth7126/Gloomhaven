using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.Entitlement;

public static class EntitlementApi
{
	public class EntitlementApiClient : ClientBase<EntitlementApiClient>
	{
		private readonly ICaller _caller;

		public EntitlementApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetEntitlementsResponse> GetEntitlementsAsync(GetEntitlementsRequest request)
		{
			return _caller.Execute<GetEntitlementsResponse, GetEntitlementsRequest>(Descriptor, "GetEntitlements", request);
		}

		public Task<ConsumeEntitlementsResponse> ConsumeEntitlementsAsync(ConsumeEntitlementsRequest request)
		{
			return _caller.Execute<ConsumeEntitlementsResponse, ConsumeEntitlementsRequest>(Descriptor, "ConsumeEntitlements", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("EntitlementApi", "RedLynx.Api.Entitlement.EntitlementApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
