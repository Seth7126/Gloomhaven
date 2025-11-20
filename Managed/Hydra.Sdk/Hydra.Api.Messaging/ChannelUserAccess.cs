using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelUserAccess : IMessage<ChannelUserAccess>, IMessage, IEquatable<ChannelUserAccess>, IDeepCloneable<ChannelUserAccess>, IBufferMessage
{
	private static readonly MessageParser<ChannelUserAccess> _parser = new MessageParser<ChannelUserAccess>(() => new ChannelUserAccess());

	private UnknownFieldSet _unknownFields;

	public const int ChannelUserFieldNumber = 1;

	private ChannelUser channelUser_;

	public const int ChannelAccessPolicyFieldNumber = 2;

	private ChannelAccessPolicy channelAccessPolicy_ = ChannelAccessPolicy.Unknown;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelUserAccess> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[27];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ChannelUser ChannelUser
	{
		get
		{
			return channelUser_;
		}
		set
		{
			channelUser_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelAccessPolicy ChannelAccessPolicy
	{
		get
		{
			return channelAccessPolicy_;
		}
		set
		{
			channelAccessPolicy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelUserAccess()
	{
	}

	[DebuggerNonUserCode]
	public ChannelUserAccess(ChannelUserAccess other)
		: this()
	{
		channelUser_ = ((other.channelUser_ != null) ? other.channelUser_.Clone() : null);
		channelAccessPolicy_ = other.channelAccessPolicy_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelUserAccess Clone()
	{
		return new ChannelUserAccess(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelUserAccess);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelUserAccess other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ChannelUser, other.ChannelUser))
		{
			return false;
		}
		if (ChannelAccessPolicy != other.ChannelAccessPolicy)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (channelUser_ != null)
		{
			num ^= ChannelUser.GetHashCode();
		}
		if (ChannelAccessPolicy != ChannelAccessPolicy.Unknown)
		{
			num ^= ChannelAccessPolicy.GetHashCode();
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
		if (channelUser_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ChannelUser);
		}
		if (ChannelAccessPolicy != ChannelAccessPolicy.Unknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ChannelAccessPolicy);
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
		if (channelUser_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelUser);
		}
		if (ChannelAccessPolicy != ChannelAccessPolicy.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelAccessPolicy);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelUserAccess other)
	{
		if (other == null)
		{
			return;
		}
		if (other.channelUser_ != null)
		{
			if (channelUser_ == null)
			{
				ChannelUser = new ChannelUser();
			}
			ChannelUser.MergeFrom(other.ChannelUser);
		}
		if (other.ChannelAccessPolicy != ChannelAccessPolicy.Unknown)
		{
			ChannelAccessPolicy = other.ChannelAccessPolicy;
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
				if (channelUser_ == null)
				{
					ChannelUser = new ChannelUser();
				}
				input.ReadMessage(ChannelUser);
				break;
			case 16u:
				ChannelAccessPolicy = (ChannelAccessPolicy)input.ReadEnum();
				break;
			}
		}
	}
}
