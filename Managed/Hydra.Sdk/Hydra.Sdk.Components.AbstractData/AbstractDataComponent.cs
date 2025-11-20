using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.AbstractData;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.AbstractData;

public sealed class AbstractDataComponent : IHydraSdkComponent
{
	private AbstractDataApi.AbstractDataApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_api = connectionManager.GetConnection<AbstractDataApi.AbstractDataApiClient>();
		return Task.CompletedTask;
	}

	public async Task<GetDataResponse> GetData(IEnumerable<AbstractDataKeyContainerNames> keyContainerNames)
	{
		return await _api.GetDataAsync(new GetDataRequest
		{
			Context = _userContext.State.Context,
			KeyContainerNames = { keyContainerNames }
		});
	}

	public Task<GetDataResponse> GetOwnData(IEnumerable<string> containerNames)
	{
		return GetData(new AbstractDataKeyContainerNames[1]
		{
			new AbstractDataKeyContainerNames
			{
				Key = _userContext.State.Context.Data.UserIdentity,
				ContainerNames = { containerNames }
			}
		});
	}

	public async Task<SetDataResponse> SetData(IEnumerable<AbstractDataKeyContainers> keyContainers)
	{
		return await _api.SetDataAsync(new SetDataRequest
		{
			Context = _userContext.State.Context,
			Data = { keyContainers }
		});
	}

	public Task<SetDataResponse> SetOwnData(IEnumerable<AbstractDataContainerData> containersData)
	{
		return SetData(new AbstractDataKeyContainers[1]
		{
			new AbstractDataKeyContainers
			{
				Key = _userContext.State.Context.Data.UserIdentity,
				Containers = { containersData }
			}
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
