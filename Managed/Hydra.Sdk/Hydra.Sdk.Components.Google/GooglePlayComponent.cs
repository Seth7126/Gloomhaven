using System.Threading.Tasks;
using Hydra.Api.GooglePlay;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Google;

public sealed class GooglePlayComponent : IHydraSdkComponent
{
	private GooglePlayApi.GooglePlayApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_api = connectionManager.GetConnection<GooglePlayApi.GooglePlayApiClient>();
		return Task.CompletedTask;
	}

	public async Task<ValidateGooglePurchaseResponse> ValidateGooglePurchase(GooglePlayPurchase purchase)
	{
		return await _api.ValidateGooglePurchaseAsync(new ValidateGooglePurchaseRequest
		{
			UserContext = _userContext.State.Context,
			Purchase = purchase
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
