using System.Threading.Tasks;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;
using RedLynx.Api.Account;

namespace Pros.Sdk.Components.Account;

public class AccountComponent : IHydraSdkComponent
{
	private AccountApi.AccountApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		IHydraSdkChannel channel = connectionManager.GetChannel(AccountApi.Descriptor.FullName);
		_api = new AccountApi.AccountApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<GetRegistrationQRCodeResponse> GetRegistrationQRCode()
	{
		return await _api.GetRegistrationQRCodeAsync(new GetRegistrationQRCodeRequest
		{
			UserContext = _userContext.State.Context
		});
	}

	public async Task<GetRegistrationStatusResponse> GetRegistrationStatus(string code)
	{
		return await _api.GetRegistrationStatusAsync(new GetRegistrationStatusRequest
		{
			UserContext = _userContext.State.Context,
			Code = code
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
