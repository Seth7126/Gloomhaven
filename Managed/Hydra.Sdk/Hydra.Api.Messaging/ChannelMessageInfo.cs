using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Messaging;

public sealed class ChannelMessageInfo : IMessage<ChannelMessageInfo>, IMessage, IEquatable<ChannelMessageInfo>, IDeepCloneable<ChannelMessageInfo>, IBufferMessage
{
	private static readonly MessageParser<ChannelMessageInfo> _parser = new MessageParser<ChannelMessageInfo>(() => new ChannelMessageInfo());

	private UnknownFieldSet _unknownFields;

	public const int MessageIndexFieldNumber = 1;

	private long messageIndex_;

	public const int ChannelMessageFieldNumber = 2;

	private ChannelMessage channelMessage_;

	public const int ChannelMessageTypeFieldNumber = 3;

	private ChannelMessageType channelMessageType_ = ChannelMessageType.None;

	public const int FromUserFieldNumber = 4;

	private ChannelUser fromUser_;

	public const int CreatedAtFieldNumber = 5;

	private Timestamp createdAt_;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelMessageInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[28];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public ChannelMessageType ChannelMessageType
	{
		get
		{
			return channelMessageType_;
		}
		set
		{
			channelMessageType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelUser FromUser
	{
		get
		{
			return fromUser_;
		}
		set
		{
			fromUser_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Timestamp CreatedAt
	{
		get
		{
			return createdAt_;
		}
		set
		{
			createdAt_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelMessageInfo()
	{
	}

	[DebuggerNonUserCode]
	public ChannelMessageInfo(ChannelMessageInfo other)
		: this()
	{
		messageIndex_ = other.messageIndex_;
		channelMessage_ = ((other.channelMessage_ != null) ? other.channelMessage_.Clone() : null);
		channelMessageType_ = other.channelMessageType_;
		fromUser_ = ((other.fromUser_ != null) ? other.fromUser_.Clone() : null);
		createdAt_ = ((other.createdAt_ != null) ? other.createdAt_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelMessageInfo Clone()
	{
		return new ChannelMessageInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelMessageInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelMessageInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MessageIndex != other.MessageIndex)
		{
			return false;
		}
		if (!object.Equals(ChannelMessage, other.ChannelMessage))
		{
			return false;
		}
		if (ChannelMessageType != other.ChannelMessageType)
		{
			return false;
		}
		if (!object.Equals(FromUser, other.FromUser))
		{
			return false;
		}
		if (!object.Equals(CreatedAt, other.CreatedAt))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (MessageIndex != 0)
		{
			num ^= MessageIndex.GetHashCode();
		}
		if (channelMessage_ != null)
		{
			num ^= ChannelMessage.GetHashCode();
		}
		if (ChannelMessageType != ChannelMessageType.None)
		{
			num ^= ChannelMessageType.GetHashCode();
		}
		if (fromUser_ != null)
		{
			num ^= FromUser.GetHashCode();
		}
		if (createdAt_ != null)
		{
			num ^= CreatedAt.GetHashCode();
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
		if (MessageIndex != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(MessageIndex);
		}
		if (channelMessage_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ChannelMessage);
		}
		if (ChannelMessageType != ChannelMessageType.None)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)ChannelMessageType);
		}
		if (fromUser_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(FromUser);
		}
		if (createdAt_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(CreatedAt);
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
		if (MessageIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(MessageIndex);
		}
		if (channelMessage_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelMessage);
		}
		if (ChannelMessageType != ChannelMessageType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelMessageType);
		}
		if (fromUser_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(FromUser);
		}
		if (createdAt_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreatedAt);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelMessageInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.MessageIndex != 0)
		{
			MessageIndex = other.MessageIndex;
		}
		if (other.channelMessage_ != null)
		{
			if (channelMessage_ == null)
			{
				ChannelMessage = new ChannelMessage();
			}
			ChannelMessage.MergeFrom(other.ChannelMessage);
		}
		if (other.ChannelMessageType != ChannelMessageType.None)
		{
			ChannelMessageType = other.ChannelMessageType;
		}
		if (other.fromUser_ != null)
		{
			if (fromUser_ == null)
			{
				FromUser = new ChannelUser();
			}
			FromUser.MergeFrom(other.FromUser);
		}
		if (other.createdAt_ != null)
		{
			if (createdAt_ == null)
			{
				CreatedAt = new Timestamp();
			}
			CreatedAt.MergeFrom(other.CreatedAt);
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
			case 8u:
				MessageIndex = input.ReadInt64();
				break;
			case 18u:
				if (channelMessage_ == null)
				{
					ChannelMessage = new ChannelMessage();
				}
				input.ReadMessage(ChannelMessage);
				break;
			case 24u:
				ChannelMessageType = (ChannelMessageType)input.ReadEnum();
				break;
			case 34u:
				if (fromUser_ == null)
				{
					FromUser = new ChannelUser();
				}
				input.ReadMessage(FromUser);
				break;
			case 42u:
				if (createdAt_ == null)
				{
					CreatedAt = new Timestamp();
				}
				input.ReadMessage(CreatedAt);
				break;
			}
		}
	}
}
