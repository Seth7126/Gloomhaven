using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.EndpointDispatcher;

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

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("EndpointDispatcherApi", "Hydra.Api.EndpointDispatcher.EndpointDispatcherApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
