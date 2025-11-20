using System;
using System.Threading.Tasks;
using Hydra.Api.Challenges;
using Hydra.Api.Push;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Communication.WebSockets;
using Hydra.Sdk.Components.Push;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Challenges;

public sealed class ChallengesComponent : APushListener, IHydraSdkComponent
{
	public delegate void OnChallengesUpdateReceivedDelegate(UserChallengesUpdateVersion update);

	private ChallengesApi.ChallengesApiClient _api;

	private StateObserver<GcContextWrapper> _gcContext;

	public OnChallengesUpdateReceivedDelegate OnChallengesUpdateReceived;

	public async Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		await InitializePush(componentMessager, stateResolver, logger);
		_gcContext = stateResolver.CreateLinkedObserver<GcContextWrapper>();
		_api = connectionManager.GetConnection<ChallengesApi.ChallengesApiClient>();
		OnTokenUpdate = (OnTokenUpdateDelegate)Delegate.Combine(OnTokenUpdate, new OnTokenUpdateDelegate(PushTokenUpdated));
		OnPushDisconnect = (OnPushDisconnectDelegate)Delegate.Combine(OnPushDisconnect, new OnPushDisconnectDelegate(PushDisconnected));
	}

	public async Task<GetChallengesResponse> GetChallenges()
	{
		return await _api.GetChallengesAsync(new GetChallengesRequest
		{
			Context = _userContext.State.Context,
			ConfigurationContext = _gcContext.State.Context
		});
	}

	public async Task<GetChallengesIncrementalUpdateResponse> GetChallengesIncrementalUpdate(long fromVersion)
	{
		return await _api.GetChallengesIncrementalUpdateAsync(new GetChallengesIncrementalUpdateRequest
		{
			Context = _userContext.State.Context,
			ConfigurationContext = _gcContext.State.Context,
			FromVersion = fromVersion
		});
	}

	public Task<ConnectResponse> Connect()
	{
		return ConnectInternal((PushToken token) => HandleConnect(token));
	}

	public Task<int> Disconnect()
	{
		return DisconnectInternal(() => HandleDisconnect(), MessageReason.User);
	}

	public Task Unregister()
	{
		return Task.CompletedTask;
	}

	protected override void PushMessageReceived(WebSocketMessage msg)
	{
		object obj = null;
		PushMessageType type = msg.Type;
		PushMessageType pushMessageType = type;
		if (pushMessageType == PushMessageType.ChallengesIncrementalUpdate)
		{
			UserChallengesUpdateVersion userChallengesUpdateVersion = UserChallengesUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = userChallengesUpdateVersion;
			HandleChallengesUpdate(userChallengesUpdateVersion);
		}
	}

	private void HandleChallengesUpdate(UserChallengesUpdateVersion update)
	{
		if (OnChallengesUpdateReceived != null)
		{
			OnChallengesUpdateReceived(update);
		}
	}

	private async Task<ConnectResponse> HandleConnect(PushToken token)
	{
		return await _api.ConnectAsync(new ConnectRequest
		{
			Context = _userContext.State?.Context,
			PushToken = token,
			ConfigurationContext = _gcContext.State?.Context
		});
	}

	private Task<int> HandleDisconnect()
	{
		return Task.FromResult(0);
	}

	private void PushTokenUpdated(PushToken pushToken)
	{
		_logger.Log(HydraLogType.Information, this.GetLogCatInf(), "Challenges: 'Push' token changed, reconnecting...");
		HandleConnect(pushToken).Wait();
	}

	private async Task PushDisconnected(MessageReason reason)
	{
		_logger.Log(HydraLogType.Information, this.GetLogCatInf(), "Challenges: 'Push' disconnected...");
		await DisconnectInternal(() => HandleDisconnect(), reason);
	}
}
