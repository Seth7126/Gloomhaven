using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelHistory : IMessage<ChannelHistory>, IMessage, IEquatable<ChannelHistory>, IDeepCloneable<ChannelHistory>, IBufferMessage
{
	private static readonly MessageParser<ChannelHistory> _parser = new MessageParser<ChannelHistory>(() => new ChannelHistory());

	private UnknownFieldSet _unknownFields;

	public const int ChannelIndexFieldNumber = 1;

	private ChannelIndex channelIndex_;

	public const int ReadMessageIndexFieldNumber = 2;

	private long readMessageIndex_;

	public const int MessagesInfoFieldNumber = 3;

	private static readonly FieldCodec<ChannelMessageInfo> _repeated_messagesInfo_codec = FieldCodec.ForMessage(26u, ChannelMessageInfo.Parser);

	private readonly RepeatedField<ChannelMessageInfo> messagesInfo_ = new RepeatedField<ChannelMessageInfo>();

	[DebuggerNonUserCode]
	public static MessageParser<ChannelHistory> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[23];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ChannelIndex ChannelIndex
	{
		get
		{
			return channelIndex_;
		}
		set
		{
			channelIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long ReadMessageIndex
	{
		get
		{
			return readMessageIndex_;
		}
		set
		{
			readMessageIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChannelMessageInfo> MessagesInfo => messagesInfo_;

	[DebuggerNonUserCode]
	public ChannelHistory()
	{
	}

	[DebuggerNonUserCode]
	public ChannelHistory(ChannelHistory other)
		: this()
	{
		channelIndex_ = ((other.channelIndex_ != null) ? other.channelIndex_.Clone() : null);
		readMessageIndex_ = other.readMessageIndex_;
		messagesInfo_ = other.messagesInfo_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelHistory Clone()
	{
		return new ChannelHistory(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelHistory);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelHistory other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ChannelIndex, other.ChannelIndex))
		{
			return false;
		}
		if (ReadMessageIndex != other.ReadMessageIndex)
		{
			return false;
		}
		if (!messagesInfo_.Equals(other.messagesInfo_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (channelIndex_ != null)
		{
			num ^= ChannelIndex.GetHashCode();
		}
		if (ReadMessageIndex != 0)
		{
			num ^= ReadMessageIndex.GetHashCode();
		}
		num ^= messagesInfo_.GetHashCode();
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
		if (channelIndex_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ChannelIndex);
		}
		if (ReadMessageIndex != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(ReadMessageIndex);
		}
		messagesInfo_.WriteTo(ref output, _repeated_messagesInfo_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (channelIndex_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelIndex);
		}
		if (ReadMessageIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ReadMessageIndex);
		}
		num += messagesInfo_.CalculateSize(_repeated_messagesInfo_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelHistory other)
	{
		if (other == null)
		{
			return;
		}
		if (other.channelIndex_ != null)
		{
			if (channelIndex_ == null)
			{
				ChannelIndex = new ChannelIndex();
			}
			ChannelIndex.MergeFrom(other.ChannelIndex);
		}
		if (other.ReadMessageIndex != 0)
		{
			ReadMessageIndex = other.ReadMessageIndex;
		}
		messagesInfo_.Add(other.messagesInfo_);
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
				if (channelIndex_ == null)
				{
					ChannelIndex = new ChannelIndex();
				}
				input.ReadMessage(ChannelIndex);
				break;
			case 16u:
				ReadMessageIndex = input.ReadInt64();
				break;
			case 26u:
				messagesInfo_.AddEntriesFrom(ref input, _repeated_messagesInfo_codec);
				break;
			}
		}
	}
}
