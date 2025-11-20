using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Telemetry;

public static class TelemetryApi
{
	public class TelemetryApiClient : ClientBase<TelemetryApiClient>
	{
		private readonly ICaller _caller;

		public TelemetryApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<SendTelemetryEventsResponse> SendTelemetryEventsAsync(SendTelemetryEventsRequest request)
		{
			return _caller.Execute<SendTelemetryEventsResponse, SendTelemetryEventsRequest>(Descriptor, "SendTelemetryEvents", request);
		}

		public Task<SendTelemetryServerEventsResponse> SendTelemetryServerEventsAsync(SendTelemetryServerEventsRequest request)
		{
			return _caller.Execute<SendTelemetryServerEventsResponse, SendTelemetryServerEventsRequest>(Descriptor, "SendTelemetryServerEvents", request);
		}

		public Task<SendTelemetryPackResponse> SendTelemetryPackAsync(SendTelemetryPackRequest request)
		{
			return _caller.Execute<SendTelemetryPackResponse, SendTelemetryPackRequest>(Descriptor, "SendTelemetryPack", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("TelemetryApi", "Hydra.Api.Telemetry.TelemetryApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 1
	};
}
