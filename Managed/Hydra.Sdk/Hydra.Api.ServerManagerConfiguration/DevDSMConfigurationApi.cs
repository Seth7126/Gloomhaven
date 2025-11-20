using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.ServerManagerConfiguration;

public static class DevDSMConfigurationApi
{
	public class DevDSMConfigurationApiClient : ClientBase<DevDSMConfigurationApiClient>
	{
		private readonly ICaller _caller;

		public DevDSMConfigurationApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
		{
			return _caller.Execute<RegisterResponse, RegisterRequest>(Descriptor, "Register", request);
		}

		public Task<UnregisterResponse> UnregisterAsync(UnregisterRequest request)
		{
			return _caller.Execute<UnregisterResponse, UnregisterRequest>(Descriptor, "Unregister", request);
		}

		public Task<GetPendingSessionsResponse> GetPendingSessionsAsync(GetPendingSessionsRequest request)
		{
			return _caller.Execute<GetPendingSessionsResponse, GetPendingSessionsRequest>(Descriptor, "GetPendingSessions", request);
		}

		public Task<RejectPendingSessionResponse> RejectPendingSessionAsync(RejectPendingSessionRequest request)
		{
			return _caller.Execute<RejectPendingSessionResponse, RejectPendingSessionRequest>(Descriptor, "RejectPendingSession", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("DevDSMConfigurationApi", "Hydra.Api.ServerManagerConfiguration.DevDSMConfigurationApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
