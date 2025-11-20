using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Diagnostics;

public static class DiagnosticsApi
{
	public class DiagnosticsApiClient : ClientBase<DiagnosticsApiClient>
	{
		private readonly ICaller _caller;

		public DiagnosticsApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<WriteCrashDumpUserResponse> WriteCrashDumpUserAsync(WriteCrashDumpUserRequest request)
		{
			return _caller.Execute<WriteCrashDumpUserResponse, WriteCrashDumpUserRequest>(Descriptor, "WriteCrashDumpUser", request);
		}

		public Task<WriteCrashDumpServerResponse> WriteCrashDumpServerAsync(WriteCrashDumpServerRequest request)
		{
			return _caller.Execute<WriteCrashDumpServerResponse, WriteCrashDumpServerRequest>(Descriptor, "WriteCrashDumpServer", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("DiagnosticsApi", "Hydra.Api.Diagnostics.DiagnosticsApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
