using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Facts;

public static class FactsApi
{
	public class FactsApiClient : ClientBase<FactsApiClient>
	{
		private readonly ICaller _caller;

		public FactsApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<WriteBinaryPackUserResponse> WriteBinaryPackUserAsync(WriteBinaryPackUserRequest request)
		{
			return _caller.Execute<WriteBinaryPackUserResponse, WriteBinaryPackUserRequest>(Descriptor, "WriteBinaryPackUser", request);
		}

		public Task<WriteBinaryPackToolResponse> WriteBinaryPackToolAsync(WriteBinaryPackToolRequest request)
		{
			return _caller.Execute<WriteBinaryPackToolResponse, WriteBinaryPackToolRequest>(Descriptor, "WriteBinaryPackTool", request);
		}

		public Task<WriteBinaryPackServerResponse> WriteBinaryPackServerAsync(WriteBinaryPackServerRequest request)
		{
			return _caller.Execute<WriteBinaryPackServerResponse, WriteBinaryPackServerRequest>(Descriptor, "WriteBinaryPackServer", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("FactsApi", "Hydra.Api.Facts.FactsApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 1
	};
}
