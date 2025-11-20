using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.GameConfiguration;

public static class GameConfigurationManagementApi
{
	public class GameConfigurationManagementApiClient : ClientBase<GameConfigurationManagementApiClient>
	{
		private readonly ICaller _caller;

		public GameConfigurationManagementApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<UploadConfigurationComponentPacksToolResponse> UploadConfigurationComponentPacksToolAsync(UploadConfigurationComponentPacksToolRequest request)
		{
			return _caller.Execute<UploadConfigurationComponentPacksToolResponse, UploadConfigurationComponentPacksToolRequest>(Descriptor, "UploadConfigurationComponentPacksTool", request);
		}

		public Task<DownloadConfigurationComponentPacksToolResponse> DownloadConfigurationComponentPacksToolAsync(DownloadConfigurationComponentPacksToolRequest request)
		{
			return _caller.Execute<DownloadConfigurationComponentPacksToolResponse, DownloadConfigurationComponentPacksToolRequest>(Descriptor, "DownloadConfigurationComponentPacksTool", request);
		}

		public Task<DownloadDefaultConfigurationComponentPacksToolResponse> DownloadDefaultConfigurationComponentPacksToolAsync(DownloadDefaultConfigurationComponentPacksToolRequest request)
		{
			return _caller.Execute<DownloadDefaultConfigurationComponentPacksToolResponse, DownloadDefaultConfigurationComponentPacksToolRequest>(Descriptor, "DownloadDefaultConfigurationComponentPacksTool", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("GameConfigurationManagementApi", "Hydra.Api.GameConfiguration.GameConfigurationManagementApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 1
	};
}
