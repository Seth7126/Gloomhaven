using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.Rating;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Ratings;

public sealed class RatingsComponent : IHydraSdkComponent
{
	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ServerContextWrapper> _serverContext;

	private RatingApi.RatingApiClient _api;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_serverContext = stateResolver.CreateLinkedObserver<ServerContextWrapper>();
		_api = connectionManager.GetConnection<RatingApi.RatingApiClient>();
		return Task.CompletedTask;
	}

	public async Task<GetRatingsUserResponse> GetRatingsUser(string ratingId, bool includeHistory, IEnumerable<string> userIds)
	{
		return await _api.GetRatingsUserAsync(new GetRatingsUserRequest
		{
			Context = _userContext.State.Context,
			RatingId = ratingId,
			IncludeHistory = includeHistory,
			UserIds = { userIds }
		});
	}

	public async Task<UpdateRatingsUserResponse> UpdateRatingsUser(string ratingId, string sessionId, IEnumerable<UserTeamResult> userTeamResults)
	{
		return await _api.UpdateRatingsUserAsync(new UpdateRatingsUserRequest
		{
			Context = _userContext.State.Context,
			RatingId = ratingId,
			SessionId = sessionId,
			Update = new RatingUpdate
			{
				TeamResults = new SessionTeamResult
				{
					Results = { userTeamResults }
				}
			}
		});
	}

	public async Task<UpdateRatingsUserResponse> UpdateRatingsUser(string ratingId, string sessionId, IEnumerable<UserIndividualResult> userIndividualResults)
	{
		return await _api.UpdateRatingsUserAsync(new UpdateRatingsUserRequest
		{
			Context = _userContext.State.Context,
			RatingId = ratingId,
			SessionId = sessionId,
			Update = new RatingUpdate
			{
				IndividualResults = new SessionIndividualResult
				{
					Results = { userIndividualResults }
				}
			}
		});
	}

	public async Task<GetRatingsServerResponse> GetRatingsServer(string ratingId, bool includeHistory, IEnumerable<string> userIds)
	{
		return await _api.GetRatingsServerAsync(new GetRatingsServerRequest
		{
			Context = _serverContext.State.Context,
			RatingId = ratingId,
			IncludeHistory = includeHistory,
			UserIds = { userIds }
		});
	}

	public async Task<UpdateRatingsServerResponse> UpdateRatingsServer(string ratingId, IEnumerable<UserTeamResult> userTeamResults)
	{
		return await _api.UpdateRatingsServerAsync(new UpdateRatingsServerRequest
		{
			Context = _serverContext.State.Context,
			RatingId = ratingId,
			Update = new RatingUpdate
			{
				TeamResults = new SessionTeamResult
				{
					Results = { userTeamResults }
				}
			}
		});
	}

	public async Task<UpdateRatingsServerResponse> UpdateRatingsServer(string ratingId, IEnumerable<UserIndividualResult> userIndividualResults)
	{
		return await _api.UpdateRatingsServerAsync(new UpdateRatingsServerRequest
		{
			Context = _serverContext.State.Context,
			RatingId = ratingId,
			Update = new RatingUpdate
			{
				IndividualResults = new SessionIndividualResult
				{
					Results = { userIndividualResults }
				}
			}
		});
	}

	public async Task<UpdateCustomRatingsUserResponse> UpdateCustomRatingsUser(string ratingId, string sessionId, IEnumerable<CustomRatingUpdate> customRatings)
	{
		return await _api.UpdateCustomRatingsUserAsync(new UpdateCustomRatingsUserRequest
		{
			Context = _userContext.State.Context,
			RatingId = ratingId,
			SessionId = sessionId,
			Update = { customRatings }
		});
	}

	public async Task<UpdateCustomRatingsServerResponse> UpdateCustomRatingsServer(string ratingId, IEnumerable<CustomRatingUpdate> customRatings)
	{
		return await _api.UpdateCustomRatingsServerAsync(new UpdateCustomRatingsServerRequest
		{
			Context = _serverContext.State.Context,
			RatingId = ratingId,
			Update = { customRatings }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
