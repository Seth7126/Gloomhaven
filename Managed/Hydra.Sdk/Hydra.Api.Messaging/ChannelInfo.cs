using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelInfo : IMessage<ChannelInfo>, IMessage, IEquatable<ChannelInfo>, IDeepCloneable<ChannelInfo>, IBufferMessage
{
	private static readonly MessageParser<ChannelInfo> _parser = new MessageParser<ChannelInfo>(() => new ChannelInfo());

	private UnknownFieldSet _unknownFields;

	public const int ChannelFieldNumber = 1;

	private Channel channel_;

	public const int ChannelCredentialFieldNumber = 2;

	private ChannelCredential channelCredential_;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[32];

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
	public ChannelCredential ChannelCredential
	{
		get
		{
			return channelCredential_;
		}
		set
		{
			channelCredential_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelInfo()
	{
	}

	[DebuggerNonUserCode]
	public ChannelInfo(ChannelInfo other)
		: this()
	{
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		channelCredential_ = ((other.channelCredential_ != null) ? other.channelCredential_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelInfo Clone()
	{
		return new ChannelInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelInfo other)
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
		if (!object.Equals(ChannelCredential, other.ChannelCredential))
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
		if (channelCredential_ != null)
		{
			num ^= ChannelCredential.GetHashCode();
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
		if (channelCredential_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(ChannelCredential);
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
		if (channelCredential_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelCredential);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelInfo other)
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
		if (other.channelCredential_ != null)
		{
			if (channelCredential_ == null)
			{
				ChannelCredential = new ChannelCredential();
			}
			ChannelCredential.MergeFrom(other.ChannelCredential);
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
				if (channelCredential_ == null)
				{
					ChannelCredential = new ChannelCredential();
				}
				input.ReadMessage(ChannelCredential);
				break;
			}
		}
	}
}
