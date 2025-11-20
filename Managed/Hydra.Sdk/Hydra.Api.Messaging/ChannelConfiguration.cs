using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelConfiguration : IMessage<ChannelConfiguration>, IMessage, IEquatable<ChannelConfiguration>, IDeepCloneable<ChannelConfiguration>, IBufferMessage
{
	private static readonly MessageParser<ChannelConfiguration> _parser = new MessageParser<ChannelConfiguration>(() => new ChannelConfiguration());

	private UnknownFieldSet _unknownFields;

	public const int ChannelMaxMembersFieldNumber = 1;

	private int channelMaxMembers_;

	public const int ChannelCredentialPolicyFieldNumber = 2;

	private ChannelCredentialPolicy channelCredentialPolicy_ = ChannelCredentialPolicy.Unknown;

	public const int ChannelOwnerMigrationPolicyFieldNumber = 3;

	private ChannelOwnerMigrationPolicy channelOwnerMigrationPolicy_ = ChannelOwnerMigrationPolicy.Unknown;

	public const int ChannelWritingPolicyFieldNumber = 4;

	private ChannelWritingPolicy channelWritingPolicy_ = ChannelWritingPolicy.Unknown;

	public const int ChannelMembershipPolicyFieldNumber = 5;

	private ChannelMembershipPolicy channelMembershipPolicy_ = ChannelMembershipPolicy.Unknown;

	public const int ChannelTimeoutMsFieldNumber = 6;

	private long channelTimeoutMs_;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelConfiguration> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[24];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int ChannelMaxMembers
	{
		get
		{
			return channelMaxMembers_;
		}
		set
		{
			channelMaxMembers_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelCredentialPolicy ChannelCredentialPolicy
	{
		get
		{
			return channelCredentialPolicy_;
		}
		set
		{
			channelCredentialPolicy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelOwnerMigrationPolicy ChannelOwnerMigrationPolicy
	{
		get
		{
			return channelOwnerMigrationPolicy_;
		}
		set
		{
			channelOwnerMigrationPolicy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelWritingPolicy ChannelWritingPolicy
	{
		get
		{
			return channelWritingPolicy_;
		}
		set
		{
			channelWritingPolicy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelMembershipPolicy ChannelMembershipPolicy
	{
		get
		{
			return channelMembershipPolicy_;
		}
		set
		{
			channelMembershipPolicy_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long ChannelTimeoutMs
	{
		get
		{
			return channelTimeoutMs_;
		}
		set
		{
			channelTimeoutMs_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelConfiguration()
	{
	}

	[DebuggerNonUserCode]
	public ChannelConfiguration(ChannelConfiguration other)
		: this()
	{
		channelMaxMembers_ = other.channelMaxMembers_;
		channelCredentialPolicy_ = other.channelCredentialPolicy_;
		channelOwnerMigrationPolicy_ = other.channelOwnerMigrationPolicy_;
		channelWritingPolicy_ = other.channelWritingPolicy_;
		channelMembershipPolicy_ = other.channelMembershipPolicy_;
		channelTimeoutMs_ = other.channelTimeoutMs_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelConfiguration Clone()
	{
		return new ChannelConfiguration(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelConfiguration);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelConfiguration other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChannelMaxMembers != other.ChannelMaxMembers)
		{
			return false;
		}
		if (ChannelCredentialPolicy != other.ChannelCredentialPolicy)
		{
			return false;
		}
		if (ChannelOwnerMigrationPolicy != other.ChannelOwnerMigrationPolicy)
		{
			return false;
		}
		if (ChannelWritingPolicy != other.ChannelWritingPolicy)
		{
			return false;
		}
		if (ChannelMembershipPolicy != other.ChannelMembershipPolicy)
		{
			return false;
		}
		if (ChannelTimeoutMs != other.ChannelTimeoutMs)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ChannelMaxMembers != 0)
		{
			num ^= ChannelMaxMembers.GetHashCode();
		}
		if (ChannelCredentialPolicy != ChannelCredentialPolicy.Unknown)
		{
			num ^= ChannelCredentialPolicy.GetHashCode();
		}
		if (ChannelOwnerMigrationPolicy != ChannelOwnerMigrationPolicy.Unknown)
		{
			num ^= ChannelOwnerMigrationPolicy.GetHashCode();
		}
		if (ChannelWritingPolicy != ChannelWritingPolicy.Unknown)
		{
			num ^= ChannelWritingPolicy.GetHashCode();
		}
		if (ChannelMembershipPolicy != ChannelMembershipPolicy.Unknown)
		{
			num ^= ChannelMembershipPolicy.GetHashCode();
		}
		if (ChannelTimeoutMs != 0)
		{
			num ^= ChannelTimeoutMs.GetHashCode();
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
		if (ChannelMaxMembers != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(ChannelMaxMembers);
		}
		if (ChannelCredentialPolicy != ChannelCredentialPolicy.Unknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ChannelCredentialPolicy);
		}
		if (ChannelOwnerMigrationPolicy != ChannelOwnerMigrationPolicy.Unknown)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)ChannelOwnerMigrationPolicy);
		}
		if (ChannelWritingPolicy != ChannelWritingPolicy.Unknown)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)ChannelWritingPolicy);
		}
		if (ChannelMembershipPolicy != ChannelMembershipPolicy.Unknown)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)ChannelMembershipPolicy);
		}
		if (ChannelTimeoutMs != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt64(ChannelTimeoutMs);
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
		if (ChannelMaxMembers != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ChannelMaxMembers);
		}
		if (ChannelCredentialPolicy != ChannelCredentialPolicy.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelCredentialPolicy);
		}
		if (ChannelOwnerMigrationPolicy != ChannelOwnerMigrationPolicy.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelOwnerMigrationPolicy);
		}
		if (ChannelWritingPolicy != ChannelWritingPolicy.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelWritingPolicy);
		}
		if (ChannelMembershipPolicy != ChannelMembershipPolicy.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChannelMembershipPolicy);
		}
		if (ChannelTimeoutMs != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ChannelTimeoutMs);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelConfiguration other)
	{
		if (other != null)
		{
			if (other.ChannelMaxMembers != 0)
			{
				ChannelMaxMembers = other.ChannelMaxMembers;
			}
			if (other.ChannelCredentialPolicy != ChannelCredentialPolicy.Unknown)
			{
				ChannelCredentialPolicy = other.ChannelCredentialPolicy;
			}
			if (other.ChannelOwnerMigrationPolicy != ChannelOwnerMigrationPolicy.Unknown)
			{
				ChannelOwnerMigrationPolicy = other.ChannelOwnerMigrationPolicy;
			}
			if (other.ChannelWritingPolicy != ChannelWritingPolicy.Unknown)
			{
				ChannelWritingPolicy = other.ChannelWritingPolicy;
			}
			if (other.ChannelMembershipPolicy != ChannelMembershipPolicy.Unknown)
			{
				ChannelMembershipPolicy = other.ChannelMembershipPolicy;
			}
			if (other.ChannelTimeoutMs != 0)
			{
				ChannelTimeoutMs = other.ChannelTimeoutMs;
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
			case 8u:
				ChannelMaxMembers = input.ReadInt32();
				break;
			case 16u:
				ChannelCredentialPolicy = (ChannelCredentialPolicy)input.ReadEnum();
				break;
			case 24u:
				ChannelOwnerMigrationPolicy = (ChannelOwnerMigrationPolicy)input.ReadEnum();
				break;
			case 32u:
				ChannelWritingPolicy = (ChannelWritingPolicy)input.ReadEnum();
				break;
			case 40u:
				ChannelMembershipPolicy = (ChannelMembershipPolicy)input.ReadEnum();
				break;
			case 48u:
				ChannelTimeoutMs = input.ReadInt64();
				break;
			}
		}
	}
}
