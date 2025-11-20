using System.Threading.Tasks;
using Hydra.Api.ServerManagerConfiguration;
using Hydra.Api.SessionControl;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.ServerManagerConfiguration;

public sealed class DevDSMComponent : IHydraSdkComponent
{
	private StateObserver<ToolContextWrapper> _context;

	private DevDSMConfigurationApi.DevDSMConfigurationApiClient _api;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager channelManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_context = stateResolver.CreateLinkedObserver<ToolContextWrapper>();
		IHydraSdkChannel channel = channelManager.GetChannel(DevDSMConfigurationApi.Descriptor.FullName);
		_api = new DevDSMConfigurationApi.DevDSMConfigurationApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<GetPendingSessionsResponse> GetPendingSessions(string serverManagerId)
	{
		return await _api.GetPendingSessionsAsync(new GetPendingSessionsRequest
		{
			Context = _context.State.Context,
			ServerManagerId = serverManagerId
		});
	}

	public async Task<RegisterResponse> Register(string version)
	{
		return await _api.RegisterAsync(new RegisterRequest
		{
			Context = _context.State.Context,
			Version = version
		});
	}

	public async Task<RejectPendingSessionResponse> RejectPendingSession(PendingSession session, string serverManagerId)
	{
		return await _api.RejectPendingSessionAsync(new RejectPendingSessionRequest
		{
			Context = _context.State.Context,
			Session = session,
			ServerManagerId = serverManagerId
		});
	}

	public async Task<UnregisterResponse> Unregister(string id)
	{
		return await _api.UnregisterAsync(new UnregisterRequest
		{
			Context = _context.State.Context,
			Id = id
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
