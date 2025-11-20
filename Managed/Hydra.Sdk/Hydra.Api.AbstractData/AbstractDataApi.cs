using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.AbstractData;

public static class AbstractDataApi
{
	public class AbstractDataApiClient : ClientBase<AbstractDataApiClient>
	{
		private readonly ICaller _caller;

		public AbstractDataApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<GetDataResponse> GetDataAsync(GetDataRequest request)
		{
			return _caller.Execute<GetDataResponse, GetDataRequest>(Descriptor, "GetData", request);
		}

		public Task<SetDataResponse> SetDataAsync(SetDataRequest request)
		{
			return _caller.Execute<SetDataResponse, SetDataRequest>(Descriptor, "SetData", request);
		}

		public Task<GetDataServerResponse> GetDataServerAsync(GetDataServerRequest request)
		{
			return _caller.Execute<GetDataServerResponse, GetDataServerRequest>(Descriptor, "GetDataServer", request);
		}

		public Task<SetDataServerResponse> SetDataServerAsync(SetDataServerRequest request)
		{
			return _caller.Execute<SetDataServerResponse, SetDataServerRequest>(Descriptor, "SetDataServer", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("AbstractDataApi", "Hydra.Api.AbstractData.AbstractDataApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 6,
		Minor = 0
	};
}
