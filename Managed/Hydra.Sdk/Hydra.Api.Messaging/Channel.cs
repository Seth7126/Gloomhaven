using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class Channel : IMessage<Channel>, IMessage, IEquatable<Channel>, IDeepCloneable<Channel>, IBufferMessage
{
	private static readonly MessageParser<Channel> _parser = new MessageParser<Channel>(() => new Channel());

	private UnknownFieldSet _unknownFields;

	public const int ChannelNameFieldNumber = 1;

	private string channelName_ = "";

	public const int ChannelTypeFieldNumber = 2;

	private ChannelType channelType_ = ChannelType.None;

	[DebuggerNonUserCode]
	public static MessageParser<Channel> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[25];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ChannelName
	{
		get
		{
			return channelName_;
		}
		set
		{
			channelName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChannelType ChannelType
	{
		get
		{
			return channelType_;
		}
		set
		{
			channelType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Channel()
	{
	}

	[DebuggerNonUserCode]
	public Channel(Channel other)
		: this()
	{
		channelName_ = other.channelName_;
		channelType_ = other.channelType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public Channel Clone()
	{
		return new Channel(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as Channel);
	}

	[DebuggerNonUserCode]
	public bool Equals(Channel other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChannelName != other.ChannelName)
		{
			return false;
		}
		if (ChannelType != other.ChannelType)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ChannelName.Length != 0)
		{
			num ^= ChannelName.GetHashCode();
		}
		if (ChannelType != ChannelType.None)
		{
			num ^= ChannelType.GetHashCode();
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
		if (ChannelName.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ChannelName);
		}
		if (ChannelType != ChannelType.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ChannelType);
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
		if (ChannelName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ChannelName);
		}
		if (ChannelType != ChannelType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(Channel other)
	{
		if (other != null)
		{
			if (other.ChannelName.Length != 0)
			{
				ChannelName = other.ChannelName;
			}
			if (other.ChannelType != ChannelType.None)
			{
				ChannelType = other.ChannelType;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				ChannelName = input.ReadString();
				break;
			case 16u:
				ChannelType = (ChannelType)input.ReadEnum();
				break;
			}
		}
	}
}
