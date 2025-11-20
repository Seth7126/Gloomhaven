using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class ChannelCredentialPassword : IMessage<ChannelCredentialPassword>, IMessage, IEquatable<ChannelCredentialPassword>, IDeepCloneable<ChannelCredentialPassword>, IBufferMessage
{
	private static readonly MessageParser<ChannelCredentialPassword> _parser = new MessageParser<ChannelCredentialPassword>(() => new ChannelCredentialPassword());

	private UnknownFieldSet _unknownFields;

	public const int PasswordFieldNumber = 1;

	private string password_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ChannelCredentialPassword> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[30];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Password
	{
		get
		{
			return password_;
		}
		set
		{
			password_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChannelCredentialPassword()
	{
	}

	[DebuggerNonUserCode]
	public ChannelCredentialPassword(ChannelCredentialPassword other)
		: this()
	{
		password_ = other.password_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChannelCredentialPassword Clone()
	{
		return new ChannelCredentialPassword(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChannelCredentialPassword);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChannelCredentialPassword other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Password != other.Password)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Password.Length != 0)
		{
			num ^= Password.GetHashCode();
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
		if (Password.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Password);
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
		if (Password.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Password);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChannelCredentialPassword other)
	{
		if (other != null)
		{
			if (other.Password.Length != 0)
			{
				Password = other.Password;
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				Password = input.ReadString();
			}
		}
	}
}
