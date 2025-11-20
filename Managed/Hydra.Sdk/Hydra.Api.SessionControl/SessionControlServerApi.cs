using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.SessionControl;

public static class SessionControlServerApi
{
	public class SessionControlServerApiClient : ClientBase<SessionControlServerApiClient>
	{
		private readonly ICaller _caller;

		public SessionControlServerApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ActivateSessionResponse> ActivateSessionAsync(ActivateSessionRequest request)
		{
			return _caller.Execute<ActivateSessionResponse, ActivateSessionRequest>(Descriptor, "ActivateSession", request);
		}

		public Task<GetSessionMemberEventsResponse> GetSessionMemberEventsAsync(GetSessionMemberEventsRequest request)
		{
			return _caller.Execute<GetSessionMemberEventsResponse, GetSessionMemberEventsRequest>(Descriptor, "GetSessionMemberEvents", request);
		}

		public Task<ProcessSessionMemberEventsResponse> ProcessSessionMemberEventsAsync(ProcessSessionMemberEventsRequest request)
		{
			return _caller.Execute<ProcessSessionMemberEventsResponse, ProcessSessionMemberEventsRequest>(Descriptor, "ProcessSessionMemberEvents", request);
		}

		public Task<FinishSessionResponse> FinishSessionAsync(FinishSessionRequest request)
		{
			return _caller.Execute<FinishSessionResponse, FinishSessionRequest>(Descriptor, "FinishSession", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("SessionControlServerApi", "Hydra.Api.SessionControl.SessionControlServerApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
