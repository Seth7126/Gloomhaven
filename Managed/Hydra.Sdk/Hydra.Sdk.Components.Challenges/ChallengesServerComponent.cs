using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.Challenges;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Challenges;

public sealed class ChallengesServerComponent : IHydraSdkComponent
{
	private StateObserver<ServerContextWrapper> _serverContext;

	private ChallengesApi.ChallengesApiClient _api;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_api = connectionManager.GetConnection<ChallengesApi.ChallengesApiClient>();
		return Task.CompletedTask;
	}

	public async Task<ServerSubmitChallengeCountersResponse> ServerSubmitChallengeCounters(string referenceId, IEnumerable<ChallengeOperationList> userOperations, bool isLastUpdate)
	{
		return await _api.ServerSubmitChallengeCountersAsync(new ServerSubmitChallengeCountersRequest
		{
			Context = _serverContext.State.Context,
			ReferenceId = referenceId,
			UserOperations = { userOperations },
			IsLastUpdate = isLastUpdate
		});
	}

	public async Task<ServerGetChallengesResponse> ServerGetChallenges(IEnumerable<string> userIds)
	{
		return await _api.ServerGetChallengesAsync(new ServerGetChallengesRequest
		{
			Context = _serverContext.State.Context,
			UserIds = { userIds }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
