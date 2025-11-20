using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.Api.GameConfiguration;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.GameConfiguration;

public sealed class GameConfigurationManagementComponent : IHydraSdkComponent
{
	private GameConfigurationManagementApi.GameConfigurationManagementApiClient _api;

	private StateObserver<ToolContextWrapper> _toolContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_api = connectionManager.GetConnection<GameConfigurationManagementApi.GameConfigurationManagementApiClient>();
		_toolContext = stateResolver.CreateLinkedObserver<ToolContextWrapper>();
		return Task.CompletedTask;
	}

	public async Task<UploadConfigurationComponentPacksToolResponse> UploadConfigurationComponentDataPacksTool(IEnumerable<ConfigurationComponentPack> packs)
	{
		return await _api.UploadConfigurationComponentPacksToolAsync(new UploadConfigurationComponentPacksToolRequest
		{
			ToolContext = _toolContext.State.Context,
			Packs = { packs }
		});
	}

	public async Task<DownloadConfigurationComponentPacksToolResponse> DownloadConfigurationComponentDataPacksTool()
	{
		return await _api.DownloadConfigurationComponentPacksToolAsync(new DownloadConfigurationComponentPacksToolRequest
		{
			ToolContext = _toolContext.State.Context
		});
	}

	public async Task<DownloadDefaultConfigurationComponentPacksToolResponse> DownloadDefaultConfigurationComponentDataPacksTool()
	{
		return await _api.DownloadDefaultConfigurationComponentPacksToolAsync(new DownloadDefaultConfigurationComponentPacksToolRequest
		{
			ToolContext = _toolContext.State.Context
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
