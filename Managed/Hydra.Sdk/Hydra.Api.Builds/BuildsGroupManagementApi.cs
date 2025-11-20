using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Builds;

public static class BuildsGroupManagementApi
{
	public class BuildsGroupManagementApiClient : ClientBase<BuildsGroupManagementApiClient>
	{
		private readonly ICaller _caller;

		public BuildsGroupManagementApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<RegisterBuildVersionsToolResponse> RegisterBuildVersionsToolAsync(RegisterBuildVersionsToolRequest request)
		{
			return _caller.Execute<RegisterBuildVersionsToolResponse, RegisterBuildVersionsToolRequest>(Descriptor, "RegisterBuildVersionsTool", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("BuildsGroupManagementApi", "Hydra.Api.Builds.BuildsGroupManagementApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 2
	};
}
