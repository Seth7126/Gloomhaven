using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.SessionControl;

public static class SessionControlClientApi
{
	public class SessionControlClientApiClient : ClientBase<SessionControlClientApiClient>
	{
		private readonly ICaller _caller;

		public SessionControlClientApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request)
		{
			return _caller.Execute<CreateSessionResponse, CreateSessionRequest>(Descriptor, "CreateSession", request);
		}

		public Task<GetServerInfoResponse> GetServerInfoAsync(GetServerInfoRequest request)
		{
			return _caller.Execute<GetServerInfoResponse, GetServerInfoRequest>(Descriptor, "GetServerInfo", request);
		}

		public Task<ManagedGetServerInfoResponse> ManagedGetServerInfoAsync(ManagedGetServerInfoRequest request)
		{
			return _caller.Execute<ManagedGetServerInfoResponse, ManagedGetServerInfoRequest>(Descriptor, "ManagedGetServerInfo", request);
		}

		public Task<GetDataCenterEchoEndpointsResponse> GetDataCenterEchoEndpointsV2Async(GetDataCenterEchoEndpointsV2Request request)
		{
			return _caller.Execute<GetDataCenterEchoEndpointsResponse, GetDataCenterEchoEndpointsV2Request>(Descriptor, "GetDataCenterEchoEndpointsV2", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("SessionControlClientApi", "Hydra.Api.SessionControl.SessionControlClientApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
