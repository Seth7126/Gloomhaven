using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;
using RedLynx.Api.Entitlement;

namespace Pros.Sdk.Components.Entitlements;

public class EntitlementComponent : IHydraSdkComponent
{
	private StateObserver<UserContextWrapper> _userContext;

	private EntitlementApi.EntitlementApiClient _api;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		IHydraSdkChannel channel = connectionManager.GetChannel(EntitlementApi.Descriptor.FullName);
		_api = new EntitlementApi.EntitlementApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<GetEntitlementsResponse> GetEntitlements()
	{
		return await _api.GetEntitlementsAsync(new GetEntitlementsRequest
		{
			UserContext = _userContext.State.Context
		});
	}

	public async Task<ConsumeEntitlementsResponse> ConsumeEntitlements(IEnumerable<string> entitlementIds)
	{
		return await _api.ConsumeEntitlementsAsync(new ConsumeEntitlementsRequest
		{
			UserContext = _userContext.State.Context,
			EntitlementIds = { entitlementIds }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
