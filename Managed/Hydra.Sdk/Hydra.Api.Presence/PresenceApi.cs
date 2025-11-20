using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Presence;

public static class PresenceApi
{
	public class PresenceApiClient : ClientBase<PresenceApiClient>
	{
		private readonly ICaller _caller;

		public PresenceApiClient(ICaller caller)
		{
			_caller = caller;
		}

		public Task<ConnectResponse> ConnectAsync(ConnectRequest request)
		{
			return _caller.Execute<ConnectResponse, ConnectRequest>(Descriptor, "Connect", request);
		}

		public Task<DisconnectResponse> DisconnectAsync(DisconnectRequest request)
		{
			return _caller.Execute<DisconnectResponse, DisconnectRequest>(Descriptor, "Disconnect", request);
		}

		public Task<GetUsersPresenceResponse> GetUsersPresenceAsync(GetUsersPresenceRequest request)
		{
			return _caller.Execute<GetUsersPresenceResponse, GetUsersPresenceRequest>(Descriptor, "GetUsersPresence", request);
		}

		public Task<PartyCreateResponse> PartyCreateAsync(PartyCreateRequest request)
		{
			return _caller.Execute<PartyCreateResponse, PartyCreateRequest>(Descriptor, "PartyCreate", request);
		}

		public Task<PartySetSettingsResponse> PartySetSettingsAsync(PartySetSettingsRequest request)
		{
			return _caller.Execute<PartySetSettingsResponse, PartySetSettingsRequest>(Descriptor, "PartySetSettings", request);
		}

		public Task<PartySetDataResponse> PartySetDataAsync(PartySetDataRequest request)
		{
			return _caller.Execute<PartySetDataResponse, PartySetDataRequest>(Descriptor, "PartySetData", request);
		}

		public Task<PartySetMemberDataResponse> PartySetMemberDataAsync(PartySetMemberDataRequest request)
		{
			return _caller.Execute<PartySetMemberDataResponse, PartySetMemberDataRequest>(Descriptor, "PartySetMemberData", request);
		}

		public Task<PartyInviteSendResponse> PartyInviteSendAsync(PartyInviteSendRequest request)
		{
			return _caller.Execute<PartyInviteSendResponse, PartyInviteSendRequest>(Descriptor, "PartyInviteSend", request);
		}

		public Task<PartyInviteRevokeResponse> PartyInviteRevokeAsync(PartyInviteRevokeRequest request)
		{
			return _caller.Execute<PartyInviteRevokeResponse, PartyInviteRevokeRequest>(Descriptor, "PartyInviteRevoke", request);
		}

		public Task<PartyInviteAcceptResponse> PartyInviteAcceptAsync(PartyInviteAcceptRequest request)
		{
			return _caller.Execute<PartyInviteAcceptResponse, PartyInviteAcceptRequest>(Descriptor, "PartyInviteAccept", request);
		}

		public Task<PartyInviteRejectResponse> PartyInviteRejectAsync(PartyInviteRejectRequest request)
		{
			return _caller.Execute<PartyInviteRejectResponse, PartyInviteRejectRequest>(Descriptor, "PartyInviteReject", request);
		}

		public Task<PartyJoinResponse> PartyJoinAsync(PartyJoinRequest request)
		{
			return _caller.Execute<PartyJoinResponse, PartyJoinRequest>(Descriptor, "PartyJoin", request);
		}

		public Task<PartyLeaveResponse> PartyLeaveAsync(PartyLeaveRequest request)
		{
			return _caller.Execute<PartyLeaveResponse, PartyLeaveRequest>(Descriptor, "PartyLeave", request);
		}

		public Task<PartyRemoveMembersResponse> PartyRemoveMemberAsync(PartyRemoveMembersRequest request)
		{
			return _caller.Execute<PartyRemoveMembersResponse, PartyRemoveMembersRequest>(Descriptor, "PartyRemoveMember", request);
		}

		public Task<PartySetOwnerResponse> PartySetOwnerAsync(PartySetOwnerRequest request)
		{
			return _caller.Execute<PartySetOwnerResponse, PartySetOwnerRequest>(Descriptor, "PartySetOwner", request);
		}

		public Task<PartyDisbandResponse> PartyDisbandAsync(PartyDisbandRequest request)
		{
			return _caller.Execute<PartyDisbandResponse, PartyDisbandRequest>(Descriptor, "PartyDisband", request);
		}

		public Task<PartyGenerateJoinCodeResponse> PartyGenerateJoinCodeAsync(PartyGenerateJoinCodeRequest request)
		{
			return _caller.Execute<PartyGenerateJoinCodeResponse, PartyGenerateJoinCodeRequest>(Descriptor, "PartyGenerateJoinCode", request);
		}

		public Task<PartyClearJoinCodeResponse> PartyClearJoinCodeAsync(PartyClearJoinCodeRequest request)
		{
			return _caller.Execute<PartyClearJoinCodeResponse, PartyClearJoinCodeRequest>(Descriptor, "PartyClearJoinCode", request);
		}

		public Task<PartyUseJoinCodeResponse> PartyUseJoinCodeAsync(PartyUseJoinCodeRequest request)
		{
			return _caller.Execute<PartyUseJoinCodeResponse, PartyUseJoinCodeRequest>(Descriptor, "PartyUseJoinCode", request);
		}

		public Task<MatchmakeStartResponse> MatchmakeStartAsync(MatchmakeStartRequest request)
		{
			return _caller.Execute<MatchmakeStartResponse, MatchmakeStartRequest>(Descriptor, "MatchmakeStart", request);
		}

		public Task<MatchmakeStopResponse> MatchmakeStopAsync(MatchmakeStopRequest request)
		{
			return _caller.Execute<MatchmakeStopResponse, MatchmakeStopRequest>(Descriptor, "MatchmakeStop", request);
		}

		public Task<MatchmakeSessionCreateResponse> MatchmakeSessionCreateAsync(MatchmakeSessionCreateRequest request)
		{
			return _caller.Execute<MatchmakeSessionCreateResponse, MatchmakeSessionCreateRequest>(Descriptor, "MatchmakeSessionCreate", request);
		}

		public Task<MatchmakeSessionSetSettingsResponse> MatchmakeSessionSetSettingsAsync(MatchmakeSessionSetSettingsRequest request)
		{
			return _caller.Execute<MatchmakeSessionSetSettingsResponse, MatchmakeSessionSetSettingsRequest>(Descriptor, "MatchmakeSessionSetSettings", request);
		}

		public Task<MatchmakeSessionSetVariantsResponse> MatchmakeSessionSetVariantsAsync(MatchmakeSessionSetVariantsRequest request)
		{
			return _caller.Execute<MatchmakeSessionSetVariantsResponse, MatchmakeSessionSetVariantsRequest>(Descriptor, "MatchmakeSessionSetVariants", request);
		}

		public Task<MatchmakeSessionSetDataResponse> MatchmakeSessionSetDataAsync(MatchmakeSessionSetDataRequest request)
		{
			return _caller.Execute<MatchmakeSessionSetDataResponse, MatchmakeSessionSetDataRequest>(Descriptor, "MatchmakeSessionSetData", request);
		}

		public Task<MatchmakeSessionSetMemberDataResponse> MatchmakeSessionSetMemberDataAsync(MatchmakeSessionSetMemberDataRequest request)
		{
			return _caller.Execute<MatchmakeSessionSetMemberDataResponse, MatchmakeSessionSetMemberDataRequest>(Descriptor, "MatchmakeSessionSetMemberData", request);
		}

		public Task<MatchmakeSessionJoinResponse> MatchmakeSessionJoinAsync(MatchmakeSessionJoinRequest request)
		{
			return _caller.Execute<MatchmakeSessionJoinResponse, MatchmakeSessionJoinRequest>(Descriptor, "MatchmakeSessionJoin", request);
		}

		public Task<MatchmakeSessionLeaveResponse> MatchmakeSessionLeaveAsync(MatchmakeSessionLeaveRequest request)
		{
			return _caller.Execute<MatchmakeSessionLeaveResponse, MatchmakeSessionLeaveRequest>(Descriptor, "MatchmakeSessionLeave", request);
		}

		public Task<MatchmakeSessionRemoveMembersResponse> MatchmakeSessionRemoveMembersAsync(MatchmakeSessionRemoveMembersRequest request)
		{
			return _caller.Execute<MatchmakeSessionRemoveMembersResponse, MatchmakeSessionRemoveMembersRequest>(Descriptor, "MatchmakeSessionRemoveMembers", request);
		}

		public Task<MatchmakeSessionSetOwnerResponse> MatchmakeSessionSetOwnerAsync(MatchmakeSessionSetOwnerRequest request)
		{
			return _caller.Execute<MatchmakeSessionSetOwnerResponse, MatchmakeSessionSetOwnerRequest>(Descriptor, "MatchmakeSessionSetOwner", request);
		}

		public Task<GetDataCenterPingEndpointsResponse> GetDataCenterPingEndpointsAsync(GetDataCenterPingEndpointsRequest request)
		{
			return _caller.Execute<GetDataCenterPingEndpointsResponse, GetDataCenterPingEndpointsRequest>(Descriptor, "GetDataCenterPingEndpoints", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("PresenceApi", "Hydra.Api.Presence.PresenceApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
