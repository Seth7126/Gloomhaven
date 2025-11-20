using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.UserReports;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.UserReports;

public sealed class UserReportsComponent : IHydraSdkComponent
{
	private IHydraSdkLogger _logger;

	private StateObserver<UserContextWrapper> _userContext;

	private UserReportsApi.UserReportsApiClient _api;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_logger = logger;
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_api = connectionManager.GetConnection<UserReportsApi.UserReportsApiClient>();
		return Task.CompletedTask;
	}

	public async Task<SendUserReportResponse> SendUserReport(string reportReasonId, string toUserId, string userMessage, IEnumerable<UserReportsProperty> userReportsProperties)
	{
		return await _api.SendUserReportAsync(new SendUserReportRequest
		{
			UserContext = _userContext.State.Context,
			ReportReasonId = reportReasonId,
			ToUserId = toUserId,
			UserMessage = userMessage,
			UserReportsProperties = { userReportsProperties }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
