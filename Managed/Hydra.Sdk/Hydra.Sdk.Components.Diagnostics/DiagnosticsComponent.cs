using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Hydra.Api.Diagnostics;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Diagnostics;

public sealed class DiagnosticsComponent : IHydraSdkComponent
{
	private DiagnosticsApi.DiagnosticsApiClient _api;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ClientInfo> _clientInfo;

	public int GetDisposePriority()
	{
		return 0;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_clientInfo = stateResolver.CreateLinkedObserver<ClientInfo>();
		_api = connectionManager.GetConnection<DiagnosticsApi.DiagnosticsApiClient>();
		return Task.CompletedTask;
	}

	public async Task<WriteCrashDumpUserResponse> WriteCrashDumpUser(string provider, DiagnosticsDataType dataType, byte[] data, string dumpHash, IEnumerable<DiagnosticsProperty> properties)
	{
		return await _api.WriteCrashDumpUserAsync(new WriteCrashDumpUserRequest
		{
			UserContext = _userContext.State.Context,
			SdkVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
			ClientVersion = _clientInfo.State.ClientVersion,
			Provider = provider,
			DataType = dataType,
			Data = ByteString.CopyFrom(data),
			DumpHash = dumpHash,
			Properties = { properties }
		});
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}
}
