using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.UserReports;

public static class UserReportsApi
{
	public class UserReportsApiClient : ClientBase<UserReportsApiClient>
	{
		private readonly ICaller _caller;

		public UserReportsApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<SendUserReportResponse> SendUserReportAsync(SendUserReportRequest request)
		{
			return _caller.Execute<SendUserReportResponse, SendUserReportRequest>(Descriptor, "SendUserReport", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("UserReportsApi", "Hydra.Api.UserReports.UserReportsApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
