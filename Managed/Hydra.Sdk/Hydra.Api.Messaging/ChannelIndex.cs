using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelIndex : IMessage<ChannelIndex>, IMessage, IEquatable<ChannelIndex>, IDeepCloneable<ChannelIndex>, IBufferMessage
{
	private static readonly MessageParser<ChannelIndex> _parser = new MessageParser<ChannelIndex>(() => new ChannelIndex());

	private UnknownFieldSet _unknownFields;

	public const int ChannelFieldNumber = 1;

	private Channel channel_;

	public const int MessageIndexFieldNumber = 2;

	private long messageIndex_;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelIndex> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[22];

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
	public long MessageIndex
	{
		get
		{
			return messageIndex_;
		}
		set
		{
			messageIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelIndex()
	{
	}

	[DebuggerNonUserCode]
	public ChannelIndex(ChannelIndex other)
		: this()
	{
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		messageIndex_ = other.messageIndex_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelIndex Clone()
	{
		return new ChannelIndex(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelIndex);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelIndex other)
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
		if (MessageIndex != other.MessageIndex)
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
		if (MessageIndex != 0)
		{
			num ^= MessageIndex.GetHashCode();
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
		if (MessageIndex != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(MessageIndex);
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
		if (MessageIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(MessageIndex);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelIndex other)
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
		if (other.MessageIndex != 0)
		{
			MessageIndex = other.MessageIndex;
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
			case 16u:
				MessageIndex = input.ReadInt64();
				break;
			}
		}
	}
}
