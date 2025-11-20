using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelUserMessage : IMessage<ChannelUserMessage>, IMessage, IEquatable<ChannelUserMessage>, IDeepCloneable<ChannelUserMessage>, IBufferMessage
{
	private static readonly MessageParser<ChannelUserMessage> _parser = new MessageParser<ChannelUserMessage>(() => new ChannelUserMessage());

	private UnknownFieldSet _unknownFields;

	public const int ChannelFieldNumber = 1;

	private Channel channel_;

	public const int ChannelMessageInfoFieldNumber = 2;

	private ChannelMessageInfo channelMessageInfo_;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelUserMessage> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[35];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public ChannelMessageInfo ChannelMessageInfo
	{
		get
		{
			return channelMessageInfo_;
		}
		set
		{
			channelMessageInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelUserMessage()
	{
	}

	[DebuggerNonUserCode]
	public ChannelUserMessage(ChannelUserMessage other)
		: this()
	{
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		channelMessageInfo_ = ((other.channelMessageInfo_ != null) ? other.channelMessageInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelUserMessage Clone()
	{
		return new ChannelUserMessage(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelUserMessage);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelUserMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Channel, other.Channel))
		{
			return false;
		}
		if (!object.Equals(ChannelMessageInfo, other.ChannelMessageInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (channel_ != null)
		{
			num ^= Channel.GetHashCode();
		}
		if (channelMessageInfo_ != null)
		{
			num ^= ChannelMessageInfo.GetHashCode();
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
		if (channel_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Channel);
		}
		if (channelMessageInfo_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ChannelMessageInfo);
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
		if (channel_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Channel);
		}
		if (channelMessageInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelMessageInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelUserMessage other)
	{
		if (other == null)
		{
			return;
		}
		if (other.channel_ != null)
		{
			if (channel_ == null)
			{
				Channel = new Channel();
			}
			Channel.MergeFrom(other.Channel);
		}
		if (other.channelMessageInfo_ != null)
		{
			if (channelMessageInfo_ == null)
			{
				ChannelMessageInfo = new ChannelMessageInfo();
			}
			ChannelMessageInfo.MergeFrom(other.ChannelMessageInfo);
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
				if (channel_ == null)
				{
					Channel = new Channel();
				}
				input.ReadMessage(Channel);
				break;
			case 18u:
				if (channelMessageInfo_ == null)
				{
					ChannelMessageInfo = new ChannelMessageInfo();
				}
				input.ReadMessage(ChannelMessageInfo);
				break;
			}
		}
	}
}
