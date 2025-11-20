using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelCredential : IMessage<ChannelCredential>, IMessage, IEquatable<ChannelCredential>, IDeepCloneable<ChannelCredential>, IBufferMessage
{
	public enum TypeOneofCase
	{
		None = 0,
		CredentialPassword = 2
	}

	private static readonly MessageParser<ChannelCredential> _parser = new MessageParser<ChannelCredential>(() => new ChannelCredential());

	private UnknownFieldSet _unknownFields;

	public const int CredentialTypeFieldNumber = 1;

	private ChannelCredentialType credentialType_ = ChannelCredentialType.Unknown;

	public const int CredentialPasswordFieldNumber = 2;

	private object type_;

	private TypeOneofCase typeCase_ = TypeOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<ChannelCredential> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[31];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ChannelCredentialType CredentialType
	{
		get
		{
			return credentialType_;
		}
		set
		{
			credentialType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelCredentialPassword CredentialPassword
	{
		get
		{
			return (typeCase_ == TypeOneofCase.CredentialPassword) ? ((ChannelCredentialPassword)type_) : null;
		}
		set
		{
			type_ = value;
			typeCase_ = ((value != null) ? TypeOneofCase.CredentialPassword : TypeOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public TypeOneofCase TypeCase => typeCase_;

	[DebuggerNonUserCode]
	public ChannelCredential()
	{
	}

	[DebuggerNonUserCode]
	public ChannelCredential(ChannelCredential other)
		: this()
	{
		credentialType_ = other.credentialType_;
		TypeOneofCase typeCase = other.TypeCase;
		TypeOneofCase typeOneofCase = typeCase;
		if (typeOneofCase == TypeOneofCase.CredentialPassword)
		{
			CredentialPassword = other.CredentialPassword.Clone();
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelCredential Clone()
	{
		return new ChannelCredential(this);
	}

	[DebuggerNonUserCode]
	public void ClearType()
	{
		typeCase_ = TypeOneofCase.None;
		type_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelCredential);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelCredential other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CredentialType != other.CredentialType)
		{
			return false;
		}
		if (!object.Equals(CredentialPassword, other.CredentialPassword))
		{
			return false;
		}
		if (TypeCase != other.TypeCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CredentialType != ChannelCredentialType.Unknown)
		{
			num ^= CredentialType.GetHashCode();
		}
		if (typeCase_ == TypeOneofCase.CredentialPassword)
		{
			num ^= CredentialPassword.GetHashCode();
		}
		num ^= (int)typeCase_;
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
		if (CredentialType != ChannelCredentialType.Unknown)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)CredentialType);
		}
		if (typeCase_ == TypeOneofCase.CredentialPassword)
		{
			output.WriteRawTag(18);
			output.WriteMessage(CredentialPassword);
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
		if (CredentialType != ChannelCredentialType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CredentialType);
		}
		if (typeCase_ == TypeOneofCase.CredentialPassword)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CredentialPassword);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelCredential other)
	{
		if (other == null)
		{
			return;
		}
		if (other.CredentialType != ChannelCredentialType.Unknown)
		{
			CredentialType = other.CredentialType;
		}
		TypeOneofCase typeCase = other.TypeCase;
		TypeOneofCase typeOneofCase = typeCase;
		if (typeOneofCase == TypeOneofCase.CredentialPassword)
		{
			if (CredentialPassword == null)
			{
				CredentialPassword = new ChannelCredentialPassword();
			}
			CredentialPassword.MergeFrom(other.CredentialPassword);
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
				CredentialType = (ChannelCredentialType)input.ReadEnum();
				break;
			case 18u:
			{
				ChannelCredentialPassword channelCredentialPassword = new ChannelCredentialPassword();
				if (typeCase_ == TypeOneofCase.CredentialPassword)
				{
					channelCredentialPassword.MergeFrom(CredentialPassword);
				}
				input.ReadMessage(channelCredentialPassword);
				CredentialPassword = channelCredentialPassword;
				break;
			}
			}
		}
	}
}
