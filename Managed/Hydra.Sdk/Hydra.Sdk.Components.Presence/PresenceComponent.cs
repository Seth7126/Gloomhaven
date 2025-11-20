using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Api.Presence;
using Hydra.Api.Push;
using Hydra.Api.Push.Presence;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Communication.WebSockets;
using Hydra.Sdk.Components.Presence.Core;
using Hydra.Sdk.Components.Push;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Presence;

public sealed class PresenceComponent : APushListener, IHydraSdkComponent
{
	public delegate Task ErrorDelegate(Exception ex);

	public delegate void PartyStatusUpdatedDelegate(PresencePartyUpdate partyUpdate);

	public delegate void SessionUpdatedDelegate(PresenceSessionUpdate sessionUpdate);

	public delegate void LongOperationDelegate(string correlationId, ErrorCode resultCode);

	public delegate void InviteReceivedDelegate(InviteData invite);

	public delegate void InviteRevokeReceivedDelegate(InviteData invite);

	public delegate void InviteResponseReceivedDelegate(InviteResponse result, InviteData invite);

	private StateObserver<SdkState> _sdkState;

	private PresenceApi.PresenceApiClient _api;

	public ErrorDelegate OnError;

	public PartyStatusUpdatedDelegate OnPartyUpdate;

	public SessionUpdatedDelegate OnSessionUpdate;

	public LongOperationDelegate OnLongOperationResult;

	public InviteReceivedDelegate OnInviteReceived;

	public InviteRevokeReceivedDelegate OnInviteRevoked;

	public InviteResponseReceivedDelegate OnInviteResponse;

	public string StaticProperty { get; private set; }

	public ConcurrentBag<Exception> Errors { get; } = new ConcurrentBag<Exception>();

	public async Task Register(IConnectionManager connectionManager, ComponentMessager messageHandler, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		await InitializePush(messageHandler, stateResolver, logger);
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		_api = connectionManager.GetConnection<PresenceApi.PresenceApiClient>();
		OnTokenUpdate = (OnTokenUpdateDelegate)Delegate.Combine(OnTokenUpdate, new OnTokenUpdateDelegate(PushTokenUpdated));
		OnPushDisconnect = (OnPushDisconnectDelegate)Delegate.Combine(OnPushDisconnect, new OnPushDisconnectDelegate(PushDisconnected));
	}

	public Task Unregister()
	{
		if (base.IsConnected && _sdkState.State.State == OnlineState.Online)
		{
			return DisconnectInternal(() => HandleDisconnect(), MessageReason.Normal);
		}
		return Task.CompletedTask;
	}

	public Task<ConnectResponse> Connect(string staticProperty = null)
	{
		StaticProperty = staticProperty ?? string.Empty;
		return ConnectInternal((PushToken token) => HandleConnect(token));
	}

	public Task<DisconnectResponse> Disconnect()
	{
		return DisconnectInternal(() => HandleDisconnect(), MessageReason.User);
	}

	public async Task<GetUsersPresenceResponse> GetUsersPresence(IEnumerable<string> userIds)
	{
		return await _api.GetUsersPresenceAsync(new GetUsersPresenceRequest
		{
			Context = _userContext.State.Context,
			Users = { userIds }
		});
	}

	public async Task<MatchmakeStartResponse> MatchmakeStart(string playlistId, IEnumerable<DCPing> dataCenterPings, IEnumerable<QueueVariants> variants)
	{
		return await _api.MatchmakeStartAsync(new MatchmakeStartRequest
		{
			Context = _userContext.State.Context,
			Options = new MatchmakeSearchOptions
			{
				Options = new MatchmakeSessionOptions
				{
					PlaylistId = playlistId,
					Pings = { dataCenterPings }
				},
				Variants = { variants }
			}
		});
	}

	public async Task<MatchmakeStopResponse> MatchmakeStop()
	{
		return await _api.MatchmakeStopAsync(new MatchmakeStopRequest
		{
			Context = _userContext.State.Context
		});
	}

	[Obsolete]
	public async Task<GetDataCenterPingEndpointsResponse> GetDataCenterPingEndpoints()
	{
		return await _api.GetDataCenterPingEndpointsAsync(new GetDataCenterPingEndpointsRequest());
	}

	private async Task<ConnectResponse> HandleConnect(PushToken token)
	{
		return await _api.ConnectAsync(new ConnectRequest
		{
			Context = _userContext.State?.Context,
			PushToken = token,
			ClientVersion = _clientInfo.State.ClientVersion,
			StaticProperty = StaticProperty
		});
	}

	private async Task<DisconnectResponse> HandleDisconnect()
	{
		return await _api.DisconnectAsync(new DisconnectRequest
		{
			Context = _userContext.State.Context
		});
	}

	private void PushTokenUpdated(PushToken pushToken)
	{
		_logger.Log(HydraLogType.Information, this.GetLogCatInf(), "'Push' token changed, reconnecting...");
		HandleConnect(pushToken).Wait();
	}

	private Task PushDisconnected(MessageReason reason)
	{
		return DisconnectInternal(() => HandleDisconnect(), reason);
	}

	public async Task<PartyCreateResponse> PartyCreate(PartyCreateParams partyCreateParams)
	{
		return await _api.PartyCreateAsync(new PartyCreateRequest
		{
			Context = _userContext.State.Context,
			Params = partyCreateParams
		});
	}

	public async Task<PartyDisbandResponse> PartyDisband()
	{
		return await _api.PartyDisbandAsync(new PartyDisbandRequest
		{
			Context = _userContext.State.Context
		});
	}

	public async Task<PartyInviteAcceptResponse> PartyInviteAccept(InviteData inviteData)
	{
		return await _api.PartyInviteAcceptAsync(new PartyInviteAcceptRequest
		{
			Context = _userContext.State.Context,
			InviteData = inviteData
		});
	}

	public async Task<PartyInviteRejectResponse> PartyInviteReject(InviteData inviteData)
	{
		return await _api.PartyInviteRejectAsync(new PartyInviteRejectRequest
		{
			Context = _userContext.State.Context,
			InviteData = inviteData
		});
	}

	public async Task<PartyInviteRevokeResponse> PartyInviteRevoke(string userId)
	{
		return await _api.PartyInviteRevokeAsync(new PartyInviteRevokeRequest
		{
			Context = _userContext.State.Context,
			UserIdTo = userId
		});
	}

	public async Task<PartyInviteSendResponse> PartyInviteSend(string userId)
	{
		return await _api.PartyInviteSendAsync(new PartyInviteSendRequest
		{
			Context = _userContext.State.Context,
			UserIdTo = userId
		});
	}

	public async Task<PartyJoinResponse> PartyJoin(string userId)
	{
		return await _api.PartyJoinAsync(new PartyJoinRequest
		{
			Context = _userContext.State.Context,
			UserIdTo = userId
		});
	}

	public async Task<PartyLeaveResponse> PartyLeave()
	{
		return await _api.PartyLeaveAsync(new PartyLeaveRequest
		{
			Context = _userContext.State.Context
		});
	}

	public async Task<PartyRemoveMembersResponse> PartyRemoveMember(string userId)
	{
		return await _api.PartyRemoveMemberAsync(new PartyRemoveMembersRequest
		{
			Context = _userContext.State.Context,
			UserId = userId
		});
	}

	public async Task<PartySetDataResponse> PartySetData(string partyData)
	{
		return await _api.PartySetDataAsync(new PartySetDataRequest
		{
			Context = _userContext.State.Context,
			Data = partyData
		});
	}

	public async Task<PartySetOwnerResponse> PartySetOwner(string userId)
	{
		return await _api.PartySetOwnerAsync(new PartySetOwnerRequest
		{
			Context = _userContext.State.Context,
			UserId = userId
		});
	}

	public async Task<PartySetMemberDataResponse> PartySetMemberData(string memberData)
	{
		return await _api.PartySetMemberDataAsync(new PartySetMemberDataRequest
		{
			Context = _userContext.State.Context,
			MemberData = memberData
		});
	}

	public async Task<PartySetSettingsResponse> PartySetSettings(PartySettings partySettings)
	{
		return await _api.PartySetSettingsAsync(new PartySetSettingsRequest
		{
			Context = _userContext.State.Context,
			Settings = partySettings
		});
	}

	public async Task<PartyGenerateJoinCodeResponse> PartyGenerateJoinCode()
	{
		return await _api.PartyGenerateJoinCodeAsync(new PartyGenerateJoinCodeRequest
		{
			Context = _userContext.State.Context
		});
	}

	public async Task<PartyUseJoinCodeResponse> PartyUseJoinCode(string joinCode)
	{
		return await _api.PartyUseJoinCodeAsync(new PartyUseJoinCodeRequest
		{
			Context = _userContext.State.Context,
			JoinCode = joinCode
		});
	}

	public async Task<PartyClearJoinCodeResponse> PartyClearJoinCode()
	{
		return await _api.PartyClearJoinCodeAsync(new PartyClearJoinCodeRequest
		{
			Context = _userContext.State.Context
		});
	}

	private void HandleInviteEvents(PresenceUserUpdateVersion update)
	{
		try
		{
			if (update.Update.InviteEvents == null || !update.Update.InviteEvents.Any())
			{
				return;
			}
			foreach (InviteEvent inviteEvent in update.Update.InviteEvents)
			{
				switch (inviteEvent.EventType)
				{
				case InviteEventType.InviteReceived:
					OnInviteReceived?.Invoke(inviteEvent.Data);
					break;
				case InviteEventType.RevokeReceived:
					OnInviteRevoked?.Invoke(inviteEvent.Data);
					break;
				case InviteEventType.RejectReceived:
					OnInviteResponse?.Invoke(InviteResponse.Rejected, inviteEvent.Data);
					break;
				case InviteEventType.AcceptSuccessReceived:
					OnInviteResponse?.Invoke(InviteResponse.Accepted, inviteEvent.Data);
					break;
				case InviteEventType.AcceptFailReceived:
					OnInviteResponse?.Invoke(InviteResponse.Failed, inviteEvent.Data);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Invite handle exception: {0}", ex);
			Errors.Add(ex);
		}
	}

	private void HandleAction(Action a)
	{
		try
		{
			a();
		}
		catch (Exception ex)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Handle exception: {0}", ex);
			Errors.Add(ex);
		}
	}

	protected override void PushMessageReceived(WebSocketMessage msg)
	{
		object obj = null;
		switch (msg.Type)
		{
		case PushMessageType.PresenceUserUpdate:
		{
			PresenceUserUpdateVersion update2 = PresenceUserUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = update2;
			_logger?.Log(HydraLogType.Message, string.Format("{0}/{1}/{2}", "SDK/Hydra", PushServiceStub.Descriptor.FullName, msg.Type), "{0}", update2);
			HandleInviteEvents(update2);
			if (update2.Update?.LongOperationResult != null)
			{
				HandleAction(delegate
				{
					OnLongOperationResult?.Invoke(update2.Update.LongOperationResult.CorrelationId, update2.Update.LongOperationResult.ResultCode);
				});
			}
			break;
		}
		case PushMessageType.PresencePartyUpdate:
		{
			PresencePartyUpdateVersion update3 = PresencePartyUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = update3;
			_logger?.Log(HydraLogType.Message, string.Format("{0}/{1}/{2}", "SDK/Hydra", PushServiceStub.Descriptor.FullName, msg.Type), "{0}", update3);
			HandleAction(delegate
			{
				OnPartyUpdate?.Invoke(update3.Update);
			});
			break;
		}
		case PushMessageType.PresenceSessionUpdate:
		{
			PresenceSessionUpdateVersion update = PresenceSessionUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = update;
			_logger?.Log(HydraLogType.Message, string.Format("{0}/{1}/{2}", "SDK/Hydra", PushServiceStub.Descriptor.FullName, msg.Type), "{0}", update);
			HandleAction(delegate
			{
				OnSessionUpdate?.Invoke(update.Update);
			});
			break;
		}
		}
	}

	public async Task<MatchmakeSessionCreateResponse> MatchmakeSessionCreate(MatchmakeSessionOptions options, MatchmakeSessionSettings settings, IEnumerable<GameVariant> variants = null)
	{
		return await _api.MatchmakeSessionCreateAsync(new MatchmakeSessionCreateRequest
		{
			Context = _userContext.State.Context,
			Options = new MatchmakeCreateOptions
			{
				Settings = settings,
				Options = options,
				Variants = { variants ?? new List<GameVariant>() }
			}
		});
	}

	public async Task<MatchmakeSessionJoinResponse> MatchmakeSessionJoin(string sessionId)
	{
		return await _api.MatchmakeSessionJoinAsync(new MatchmakeSessionJoinRequest
		{
			Context = _userContext.State.Context,
			SessionId = sessionId
		});
	}

	public async Task<MatchmakeSessionLeaveResponse> MatchmakeSessionLeave()
	{
		return await _api.MatchmakeSessionLeaveAsync(new MatchmakeSessionLeaveRequest
		{
			Context = _userContext.State.Context
		});
	}

	public async Task<MatchmakeSessionRemoveMembersResponse> MatchmakeSessionRemoveMembers(IEnumerable<string> userIds)
	{
		return await _api.MatchmakeSessionRemoveMembersAsync(new MatchmakeSessionRemoveMembersRequest
		{
			Context = _userContext.State.Context,
			UserId = { userIds }
		});
	}

	public async Task<MatchmakeSessionSetDataResponse> MatchmakeSessionSetData(string data)
	{
		return await _api.MatchmakeSessionSetDataAsync(new MatchmakeSessionSetDataRequest
		{
			Context = _userContext.State.Context,
			Data = data
		});
	}

	public async Task<MatchmakeSessionSetOwnerResponse> MatchmakeSessionSetOwner(string userId)
	{
		return await _api.MatchmakeSessionSetOwnerAsync(new MatchmakeSessionSetOwnerRequest
		{
			Context = _userContext.State.Context,
			UserId = userId
		});
	}

	public async Task<MatchmakeSessionSetMemberDataResponse> MatchmakeSessionSetMemberData(string memberData)
	{
		return await _api.MatchmakeSessionSetMemberDataAsync(new MatchmakeSessionSetMemberDataRequest
		{
			Context = _userContext.State.Context,
			MemberData = memberData
		});
	}

	public async Task<MatchmakeSessionSetSettingsResponse> MatchmakeSessionSetSettings(MatchmakeSessionSettings settings)
	{
		return await _api.MatchmakeSessionSetSettingsAsync(new MatchmakeSessionSetSettingsRequest
		{
			Context = _userContext.State.Context,
			Settings = settings
		});
	}

	public async Task<MatchmakeSessionSetVariantsResponse> MatchmakeSessionSetVariants(IEnumerable<GameVariant> gameVariants)
	{
		return await _api.MatchmakeSessionSetVariantsAsync(new MatchmakeSessionSetVariantsRequest
		{
			Context = _userContext.State.Context,
			Variants = { gameVariants ?? Enumerable.Empty<GameVariant>() }
		});
	}
}
