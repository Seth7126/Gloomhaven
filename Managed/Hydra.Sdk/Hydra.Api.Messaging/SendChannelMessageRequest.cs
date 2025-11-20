using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Messaging;

public sealed class SendChannelMessageRequest : IMessage<SendChannelMessageRequest>, IMessage, IEquatable<SendChannelMessageRequest>, IDeepCloneable<SendChannelMessageRequest>, IBufferMessage
{
	private static readonly MessageParser<SendChannelMessageRequest> _parser = new MessageParser<SendChannelMessageRequest>(() => new SendChannelMessageRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ChannelFieldNumber = 2;

	private Channel channel_;

	public const int ChannelMessageFieldNumber = 3;

	private ChannelMessage channelMessage_;

	[DebuggerNonUserCode]
	public static MessageParser<SendChannelMessageRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[14];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Channel Channel
	{
		get
		{
			return channel_;
		}
		set
		{
			channel_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelMessage ChannelMessage
	{
		get
		{
			return channelMessage_;
		}
		set
		{
			channelMessage_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SendChannelMessageRequest()
	{
	}

	[DebuggerNonUserCode]
	public SendChannelMessageRequest(SendChannelMessageRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		channelMessage_ = ((other.channelMessage_ != null) ? other.channelMessage_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SendChannelMessageRequest Clone()
	{
		return new SendChannelMessageRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SendChannelMessageRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SendChannelMessageRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!object.Equals(Channel, other.Channel))
		{
			return false;
		}
		if (!object.Equals(ChannelMessage, other.ChannelMessage))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		if (channel_ != null)
		{
			num ^= Channel.GetHashCode();
		}
		if (channelMessage_ != null)
		{
			num ^= ChannelMessage.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		if (channel_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Channel);
		}
		if (channelMessage_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(ChannelMessage);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (channel_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Channel);
		}
		if (channelMessage_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelMessage);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SendChannelMessageRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		if (other.channel_ != null)
		{
			if (channel_ == null)
			{
				Channel = new Channel();
			}
			Channel.MergeFrom(other.Channel);
		}
		if (other.channelMessage_ != null)
		{
			if (channelMessage_ == null)
			{
				ChannelMessage = new ChannelMessage();
			}
			ChannelMessage.MergeFrom(other.ChannelMessage);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				if (channel_ == null)
				{
					Channel = new Channel();
				}
				input.ReadMessage(Channel);
				break;
			case 26u:
				if (channelMessage_ == null)
				{
					ChannelMessage = new ChannelMessage();
				}
				input.ReadMessage(ChannelMessage);
				break;
			}
		}
	}
}
