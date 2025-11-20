using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.BuildServers;

public static class BuildServersManagementApi
{
	public class BuildServersManagementApiClient : ClientBase<BuildServersManagementApiClient>
	{
		private readonly ICaller _caller;

		public BuildServersManagementApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetUploadCredentialsToolResponse> GetUploadCredentialsToolAsync(GetUploadCredentialsToolRequest request)
		{
			return _caller.Execute<GetUploadCredentialsToolResponse, GetUploadCredentialsToolRequest>(Descriptor, "GetUploadCredentialsTool", request);
		}

		public Task<UploadBuildPackConfigToolResponse> UploadBuildPackConfigToolAsync(UploadBuildPackConfigToolRequest request)
		{
			return _caller.Execute<UploadBuildPackConfigToolResponse, UploadBuildPackConfigToolRequest>(Descriptor, "UploadBuildPackConfigTool", request);
		}

		public Task<AnalyzeBuildPackConfigToolResponse> AnalyzeBuildPackConfigToolAsync(AnalyzeBuildPackConfigToolRequest request)
		{
			return _caller.Execute<AnalyzeBuildPackConfigToolResponse, AnalyzeBuildPackConfigToolRequest>(Descriptor, "AnalyzeBuildPackConfigTool", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("BuildServersManagementApi", "Hydra.Api.BuildServers.BuildServersManagementApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 3
	};
}
