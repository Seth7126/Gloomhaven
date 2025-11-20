using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Messaging;
using Hydra.Api.Push;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Communication.WebSockets;
using Hydra.Sdk.Components.Push;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Messaging;

public sealed class MessagingComponent : APushListener, IHydraSdkComponent
{
	public delegate void OnMessagesReceivedDelegate(IReadOnlyCollection<ChannelUserMessage> messages);

	public OnMessagesReceivedDelegate OnMessagesReceived;

	private MessagingApi.MessagingApiClient _api;

	private StateObserver<SdkState> _sdkState;

	public async Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		await InitializePush(componentMessager, stateResolver, logger);
		_api = connectionManager.GetConnection<MessagingApi.MessagingApiClient>();
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
	}

	public Task Unregister()
	{
		if (base.IsConnected && _sdkState.State.State == OnlineState.Online)
		{
			return DisconnectInternal(() => HandleDisconnect(), MessageReason.Normal);
		}
		return Task.CompletedTask;
	}

	public Task<ConnectResponse> Connect(bool enablePrivateMessages)
	{
		return ConnectInternal((PushToken token) => HandleConnect(token, enablePrivateMessages));
	}

	public Task<DisconnectResponse> Disconnect()
	{
		return DisconnectInternal(() => HandleDisconnect(), MessageReason.User);
	}

	public async Task<CreateChannelResponse> CreateChannel(string channelName, ChannelConfiguration channelConfiguration, ChannelCredential channelCredential)
	{
		return await _api.CreateChannelAsync(new CreateChannelRequest
		{
			UserContext = _userContext.State?.Context,
			Channel = new Channel
			{
				ChannelName = channelName,
				ChannelType = ChannelType.User
			},
			ChannelConfiguration = channelConfiguration,
			ChannelCredential = channelCredential
		});
	}

	public async Task<UpdateChannelResponse> UpdateChannel(string userChannelName, ChannelConfiguration channelConfiguration)
	{
		return await _api.UpdateChannelAsync(new UpdateChannelRequest
		{
			UserContext = _userContext.State?.Context,
			Channel = new Channel
			{
				ChannelName = userChannelName,
				ChannelType = ChannelType.User
			},
			ChannelConfiguration = channelConfiguration
		});
	}

	public async Task<UpdateMembersResponse> UpdateMembers(Channel channel, IEnumerable<ChannelUserAccess> channelUsersAccess)
	{
		return await _api.UpdateMembersAsync(new UpdateMembersRequest
		{
			UserContext = _userContext.State?.Context,
			Channel = channel,
			ChannelUsersAccess = { channelUsersAccess }
		});
	}

	public async Task<JoinChannelsResponse> JoinChannels(IEnumerable<ChannelInfo> channelsInfo)
	{
		return await _api.JoinChannelsAsync(new JoinChannelsRequest
		{
			UserContext = _userContext.State?.Context,
			ChannelsInfo = { channelsInfo }
		});
	}

	public async Task<LeaveChannelsResponse> LeaveChannels(IEnumerable<Channel> channels)
	{
		return await _api.LeaveChannelsAsync(new LeaveChannelsRequest
		{
			UserContext = _userContext.State?.Context,
			Channels = { channels }
		});
	}

	public async Task<SendChannelMessageResponse> SendChannelMessage(string channelName, ChannelType channelType, string text)
	{
		return await _api.SendChannelMessageAsync(new SendChannelMessageRequest
		{
			UserContext = _userContext.State?.Context,
			Channel = new Channel
			{
				ChannelName = channelName,
				ChannelType = channelType
			},
			ChannelMessage = new ChannelMessage
			{
				Text = text
			}
		});
	}

	public async Task<SendPrivateMessageResponse> SendPrivateMessage(string userId, string messageText)
	{
		return await _api.SendPrivateMessageAsync(new SendPrivateMessageRequest
		{
			UserContext = _userContext.State?.Context,
			ForUser = new ChannelUser
			{
				User = userId
			},
			ChannelMessage = new ChannelMessage
			{
				Text = messageText
			}
		});
	}

	public async Task<ReadChannelsHistoryResponse> ReadChannelsHistory(IEnumerable<ChannelIndex> channelIndexes)
	{
		return await _api.ReadChannelsHistoryAsync(new ReadChannelsHistoryRequest
		{
			UserContext = _userContext.State?.Context,
			ChannelIndexes = { channelIndexes }
		});
	}

	public async Task<GetChannelsHistoryResponse> GetChannelsHistory(IEnumerable<ChannelIndex> channelIndexes)
	{
		return await _api.GetChannelsHistoryAsync(new GetChannelsHistoryRequest
		{
			UserContext = _userContext.State?.Context,
			ChannelIndexes = { channelIndexes }
		});
	}

	private async Task<ConnectResponse> HandleConnect(PushToken token, bool enablePrivateMessages)
	{
		ConnectResponse response = await _api.ConnectAsync(new ConnectRequest
		{
			UserContext = _userContext.State?.Context,
			PushToken = token
		});
		bool isPrivateMessagesEnabled = response.ChannelHistories.Any((ChannelHistory a) => a.ChannelIndex.Channel.ChannelType == ChannelType.Private && string.IsNullOrEmpty(a.ChannelIndex?.Channel?.ChannelName));
		if (!isPrivateMessagesEnabled && enablePrivateMessages)
		{
			await JoinChannels(new ChannelInfo[1]
			{
				new ChannelInfo
				{
					Channel = new Channel
					{
						ChannelType = ChannelType.Private
					}
				}
			});
		}
		else if (isPrivateMessagesEnabled && !enablePrivateMessages)
		{
			await LeaveChannels(new Channel[1]
			{
				new Channel
				{
					ChannelType = ChannelType.Private
				}
			});
		}
		return response;
	}

	private async Task<DisconnectResponse> HandleDisconnect()
	{
		return await _api.DisconnectAsync(new DisconnectRequest
		{
			UserContext = _userContext.State?.Context
		});
	}

	protected override void PushMessageReceived(WebSocketMessage msg)
	{
		object obj = null;
		PushMessageType type = msg.Type;
		PushMessageType pushMessageType = type;
		if (pushMessageType == PushMessageType.MessagingUserUpdate)
		{
			MessagingUserUpdateVersion messagingUserUpdateVersion = MessagingUserUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = messagingUserUpdateVersion;
			_logger?.Log(HydraLogType.Message, string.Format("{0}/{1}/{2}", "SDK/Hydra", PushServiceStub.Descriptor.FullName, msg.Type), "{0}", messagingUserUpdateVersion);
			HandleMessage(messagingUserUpdateVersion);
		}
	}

	private void HandleMessage(MessagingUserUpdateVersion update)
	{
		if (update.Update != null && update.Update.Messages.Any())
		{
			OnMessagesReceived?.Invoke(update.Update.Messages);
		}
	}
}
