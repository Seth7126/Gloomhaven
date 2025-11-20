using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace RedLynx.Api.CrossSave;

public static class CrossSaveApi
{
	public class CrossSaveApiClient : ClientBase<CrossSaveApiClient>
	{
		private readonly ICaller _caller;

		public CrossSaveApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<SubmitSnapshotResponse> SubmitSnapshotAsync(SubmitSnapshotRequest request)
		{
			return _caller.Execute<SubmitSnapshotResponse, SubmitSnapshotRequest>(Descriptor, "SubmitSnapshot", request);
		}

		public Task<GetTransferInfoResponse> GetTransferInfoAsync(GetTransferInfoRequest request)
		{
			return _caller.Execute<GetTransferInfoResponse, GetTransferInfoRequest>(Descriptor, "GetTransferInfo", request);
		}

		public Task<CompleteTransferResponse> CompleteTransferAsync(CompleteTransferRequest request)
		{
			return _caller.Execute<CompleteTransferResponse, CompleteTransferRequest>(Descriptor, "CompleteTransfer", request);
		}

		public Task<SkipTransferResponse> SkipTransferAsync(SkipTransferRequest request)
		{
			return _caller.Execute<SkipTransferResponse, SkipTransferRequest>(Descriptor, "SkipTransfer", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("CrossSaveApi", "RedLynx.Api.CrossSave.CrossSaveApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 2,
		Minor = 0
	};
}
