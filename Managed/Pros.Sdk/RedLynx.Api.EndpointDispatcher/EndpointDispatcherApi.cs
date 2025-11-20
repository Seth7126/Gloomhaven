using System.Threading.Tasks;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.EndpointDispatcher;

public static class EndpointDispatcherApi
{
	public class EndpointDispatcherApiClient : ClientBase<EndpointDispatcherApiClient>
	{
		private readonly ICaller _caller;

		public EndpointDispatcherApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetEnvironmentInfoResponse> GetEnvironmentInfoAsync(GetEnvironmentInfoRequest request)
		{
			return _caller.Execute<GetEnvironmentInfoResponse, GetEnvironmentInfoRequest>(Descriptor, "GetEnvironmentInfo", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("EndpointDispatcherApi", "RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
