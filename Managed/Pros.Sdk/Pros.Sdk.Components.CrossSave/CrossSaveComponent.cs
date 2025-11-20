using System.Threading.Tasks;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;
using RedLynx.Api.CrossSave;

namespace Pros.Sdk.Components.CrossSave;

public class CrossSaveComponent : IHydraSdkComponent
{
	private CrossSaveApi.CrossSaveApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessage, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		IHydraSdkChannel channel = connectionManager.GetChannel(CrossSaveApi.Descriptor.FullName);
		_api = new CrossSaveApi.CrossSaveApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<SubmitSnapshotResponse> SubmitSnapshot(SaveSnapshot snapshot)
	{
		return await _api.SubmitSnapshotAsync(new SubmitSnapshotRequest
		{
			UserContext = _userContext.State.Context,
			Snapshot = snapshot
		});
	}

	public async Task<GetTransferInfoResponse> GetTransferInfo()
	{
		return await _api.GetTransferInfoAsync(new GetTransferInfoRequest
		{
			UserContext = _userContext.State.Context
		});
	}

	public async Task<CompleteTransferResponse> CompleteTransfer(string transferId, SaveSnapshot backupSnapshot)
	{
		return await _api.CompleteTransferAsync(new CompleteTransferRequest
		{
			UserContext = _userContext.State.Context,
			TransferId = transferId,
			BackupSnapshot = backupSnapshot
		});
	}

	public async Task<SkipTransferResponse> SkipTransfer(string transferId)
	{
		return await _api.SkipTransferAsync(new SkipTransferRequest
		{
			UserContext = _userContext.State.Context,
			TransferId = transferId
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
