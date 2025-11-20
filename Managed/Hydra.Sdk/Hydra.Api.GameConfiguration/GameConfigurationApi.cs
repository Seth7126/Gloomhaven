using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.GameConfiguration;

public static class GameConfigurationApi
{
	public class GameConfigurationApiClient : ClientBase<GameConfigurationApiClient>
	{
		private readonly ICaller _caller;

		public GameConfigurationApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetConfigurationContextResponse> GetConfigurationContextAsync(GetConfigurationContextRequest request)
		{
			return _caller.Execute<GetConfigurationContextResponse, GetConfigurationContextRequest>(Descriptor, "GetConfigurationContext", request);
		}

		public Task<GetConfigurationComponentDataResponse> GetConfigurationComponentDataAsync(GetConfigurationComponentDataRequest request)
		{
			return _caller.Execute<GetConfigurationComponentDataResponse, GetConfigurationComponentDataRequest>(Descriptor, "GetConfigurationComponentData", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("GameConfigurationApi", "Hydra.Api.GameConfiguration.GameConfigurationApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 1
	};
}
