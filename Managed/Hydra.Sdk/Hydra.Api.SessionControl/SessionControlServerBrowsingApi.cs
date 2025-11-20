using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.SessionControl;

public static class SessionControlServerBrowsingApi
{
	public class SessionControlServerBrowsingApiClient : ClientBase<SessionControlServerBrowsingApiClient>
	{
		private readonly ICaller _caller;

		public SessionControlServerBrowsingApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<BrowseServersResponse> BrowseServersAsync(BrowseServersRequest request)
		{
			return _caller.Execute<BrowseServersResponse, BrowseServersRequest>(Descriptor, "BrowseServers", request);
		}

		public Task<CreateServerResponse> CreateServerAsync(CreateServerRequest request)
		{
			return _caller.Execute<CreateServerResponse, CreateServerRequest>(Descriptor, "CreateServer", request);
		}

		public Task<HeartbeatServerResponse> HeartbeatServerAsync(HeartbeatServerRequest request)
		{
			return _caller.Execute<HeartbeatServerResponse, HeartbeatServerRequest>(Descriptor, "HeartbeatServer", request);
		}

		public Task<DestroyServerResponse> DestroyServerAsync(DestroyServerRequest request)
		{
			return _caller.Execute<DestroyServerResponse, DestroyServerRequest>(Descriptor, "DestroyServer", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("SessionControlServerBrowsingApi", "Hydra.Api.SessionControl.SessionControlServerBrowsingApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
