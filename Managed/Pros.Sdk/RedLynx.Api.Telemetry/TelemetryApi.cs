using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.Telemetry;

public static class TelemetryApi
{
	public class TelemetryApiClient : ClientBase<TelemetryApiClient>
	{
		private readonly ICaller _caller;

		public TelemetryApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<SendTelemetryPackResponse> SendTelemetryPackAsync(SendTelemetryPackRequest request)
		{
			return _caller.Execute<SendTelemetryPackResponse, SendTelemetryPackRequest>(Descriptor, "SendTelemetryPack", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("TelemetryApi", "RedLynx.Api.Telemetry.TelemetryApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 1,
		Minor = 0
	};
}
