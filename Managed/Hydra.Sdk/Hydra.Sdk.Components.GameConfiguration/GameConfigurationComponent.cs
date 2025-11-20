using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Api.GameConfiguration;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Components.GameConfiguration.Core;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.GameConfiguration;

public sealed class GameConfigurationComponent : IHydraSdkComponent
{
	public const int GAME_CONFIGURATION_BUFFER_SIZE_BYTES = 10000000;

	private IHydraSdkLogger _logger;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<GcContextWrapper> _gcContext;

	private GameConfigurationApi.GameConfigurationApiClient _api;

	private IGameConfigurationCache _cache;

	private List<ConfigurationComponent> _components;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_logger = logger;
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_gcContext = stateResolver.CreateLinkedObserver<GcContextWrapper>();
		_api = connectionManager.GetConnection<GameConfigurationApi.GameConfigurationApiClient>();
		_cache = new GameConfigurationCache(10000000, _logger);
		return Task.CompletedTask;
	}

	public async Task<GetConfigurationContextResponse> GetConfigurationContext(IEnumerable<ConfigurationComponent> components = null)
	{
		GetConfigurationContextResponse response = await _api.GetConfigurationContextAsync(new GetConfigurationContextRequest
		{
			Context = _userContext.State.Context,
			Components = { components ?? GetAvailableComponents() }
		});
		_components = response.Snapshots.Select((ConfigurationComponentSnapshot s) => s.Component).ToList();
		_gcContext.Update(new GcContextWrapper(response.Context));
		return response;
	}

	public async Task<GetConfigurationComponentDataResponse> GetConfigurationComponentData(IEnumerable<ConfigurationComponent> components)
	{
		if (_gcContext.State?.Context == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidState, "GameConfigurationContext must be obtained first.");
		}
		await _cache.AwaitCache();
		_cache.Lock();
		GetConfigurationComponentDataResponse response = new GetConfigurationComponentDataResponse();
		try
		{
			List<ConfigurationComponent> componentsToRequest = new List<ConfigurationComponent>();
			IEnumerable<ConfigurationComponent> enumerable;
			if (components != null && components.Any())
			{
				enumerable = components;
			}
			else
			{
				IEnumerable<ConfigurationComponent> availableComponents = GetAvailableComponents();
				enumerable = availableComponents;
			}
			IEnumerable<ConfigurationComponent> requestedComponents = enumerable;
			foreach (ConfigurationComponent component in requestedComponents)
			{
				if (!_cache.IsCached(GetCacheKey(component.Name)))
				{
					componentsToRequest.Add(component);
				}
			}
			if (componentsToRequest.Any())
			{
				foreach (ComponentDataResult result in (await _api.GetConfigurationComponentDataAsync(new GetConfigurationComponentDataRequest
				{
					Context = _gcContext.State.Context,
					Components = { (IEnumerable<ConfigurationComponent>)componentsToRequest }
				})).Results)
				{
					string key = GetCacheKey(result.ComponentSnapshot.Component.Name);
					if (!_cache.IsCached(key))
					{
						_cache.TryAdd(key, result);
					}
				}
			}
			foreach (ConfigurationComponent component2 in requestedComponents)
			{
				if (_cache.TryGetValue(GetCacheKey(component2.Name), out var cachedData))
				{
					response.Results.Add(cachedData);
				}
				else
				{
					_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Results cache failed");
				}
				cachedData = null;
			}
			return response;
		}
		finally
		{
			_cache.Release();
		}
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}

	private string GetCacheKey(string componentName)
	{
		return _gcContext.State.Context.Data.ConfigurationHash + "/" + componentName;
	}

	private List<ConfigurationComponent> GetAvailableComponents()
	{
		return _components ?? new List<ConfigurationComponent>();
	}
}
