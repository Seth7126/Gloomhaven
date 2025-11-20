using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;
using RedLynx.Api.Banner;

namespace Pros.Sdk.Components.Banner;

public class BannerComponent : IHydraSdkComponent
{
	private BannerApi.BannerApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		IHydraSdkChannel channel = connectionManager.GetChannel(BannerApi.Descriptor.FullName);
		_api = new BannerApi.BannerApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<GetBannersResponse> GetBanners(string referenceId = null, IEnumerable<string> entitlements = null)
	{
		return await _api.GetBannersAsync(new GetBannersRequest
		{
			UserContext = _userContext.State.Context,
			ReferenceId = (referenceId ?? string.Empty),
			Entitlements = { entitlements ?? new List<string>() }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
