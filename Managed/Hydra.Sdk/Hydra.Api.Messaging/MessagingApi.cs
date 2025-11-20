using System.Threading.Tasks;
using Hydra.Api.Infrastructure;
using Hydra.Sdk.Generated;

namespace Hydra.Api.Messaging;

public static class MessagingApi
{
	public class MessagingApiClient : ClientBase<MessagingApiClient>
	{
		private readonly ICaller _caller;

		public MessagingApiClient(ICaller caller)
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

		public Task<CreateChannelResponse> CreateChannelAsync(CreateChannelRequest request)
		{
			return _caller.Execute<CreateChannelResponse, CreateChannelRequest>(Descriptor, "CreateChannel", request);
		}

		public Task<UpdateChannelResponse> UpdateChannelAsync(UpdateChannelRequest request)
		{
			return _caller.Execute<UpdateChannelResponse, UpdateChannelRequest>(Descriptor, "UpdateChannel", request);
		}

		public Task<UpdateMembersResponse> UpdateMembersAsync(UpdateMembersRequest request)
		{
			return _caller.Execute<UpdateMembersResponse, UpdateMembersRequest>(Descriptor, "UpdateMembers", request);
		}

		public Task<JoinChannelsResponse> JoinChannelsAsync(JoinChannelsRequest request)
		{
			return _caller.Execute<JoinChannelsResponse, JoinChannelsRequest>(Descriptor, "JoinChannels", request);
		}

		public Task<LeaveChannelsResponse> LeaveChannelsAsync(LeaveChannelsRequest request)
		{
			return _caller.Execute<LeaveChannelsResponse, LeaveChannelsRequest>(Descriptor, "LeaveChannels", request);
		}

		public Task<SendChannelMessageResponse> SendChannelMessageAsync(SendChannelMessageRequest request)
		{
			return _caller.Execute<SendChannelMessageResponse, SendChannelMessageRequest>(Descriptor, "SendChannelMessage", request);
		}

		public Task<SendPrivateMessageResponse> SendPrivateMessageAsync(SendPrivateMessageRequest request)
		{
			return _caller.Execute<SendPrivateMessageResponse, SendPrivateMessageRequest>(Descriptor, "SendPrivateMessage", request);
		}

		public Task<ReadChannelsHistoryResponse> ReadChannelsHistoryAsync(ReadChannelsHistoryRequest request)
		{
			return _caller.Execute<ReadChannelsHistoryResponse, ReadChannelsHistoryRequest>(Descriptor, "ReadChannelsHistory", request);
		}

		public Task<GetChannelsHistoryResponse> GetChannelsHistoryAsync(GetChannelsHistoryRequest request)
		{
			return _caller.Execute<GetChannelsHistoryResponse, GetChannelsHistoryRequest>(Descriptor, "GetChannelsHistory", request);
		}
	}

	public static GenDescriptor Descriptor { get; } = new GenDescriptor("MessagingApi", "Hydra.Api.Messaging.MessagingApi");

	public static ServiceVersion Version { get; } = new ServiceVersion
	{
		Major = 5,
		Minor = 0
	};
}
