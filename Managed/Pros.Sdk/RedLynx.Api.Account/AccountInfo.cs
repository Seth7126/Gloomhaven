using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public sealed class AccountInfo : IMessage<AccountInfo>, IMessage, IEquatable<AccountInfo>, IDeepCloneable<AccountInfo>, IBufferMessage
{
	private static readonly MessageParser<AccountInfo> _parser = new MessageParser<AccountInfo>(() => new AccountInfo());

	private UnknownFieldSet _unknownFields;

	public const int UsernameFieldNumber = 1;

	private string username_ = "";

	public const int UsernamePostfixFieldNumber = 2;

	private string usernamePostfix_ = "";

	public const int AvatarLinkFieldNumber = 3;

	private string avatarLink_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AccountInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => AccountContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Username
	{
		get
		{
			return username_;
		}
		set
		{
			username_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UsernamePostfix
	{
		get
		{
			return usernamePostfix_;
		}
		set
		{
			usernamePostfix_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string AvatarLink
	{
		get
		{
			return avatarLink_;
		}
		set
		{
			avatarLink_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountInfo(AccountInfo other)
		: this()
	{
		username_ = other.username_;
		usernamePostfix_ = other.usernamePostfix_;
		avatarLink_ = other.avatarLink_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountInfo Clone()
	{
		return new AccountInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AccountInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AccountInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Username != other.Username)
		{
			return false;
		}
		if (UsernamePostfix != other.UsernamePostfix)
		{
			return false;
		}
		if (AvatarLink != other.AvatarLink)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (Username.Length != 0)
		{
			num ^= Username.GetHashCode();
		}
		if (UsernamePostfix.Length != 0)
		{
			num ^= UsernamePostfix.GetHashCode();
		}
		if (AvatarLink.Length != 0)
		{
			num ^= AvatarLink.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (Username.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Username);
		}
		if (UsernamePostfix.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(UsernamePostfix);
		}
		if (AvatarLink.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(AvatarLink);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (Username.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Username);
		}
		if (UsernamePostfix.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UsernamePostfix);
		}
		if (AvatarLink.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AvatarLink);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AccountInfo other)
	{
		if (other != null)
		{
			if (other.Username.Length != 0)
			{
				Username = other.Username;
			}
			if (other.UsernamePostfix.Length != 0)
			{
				UsernamePostfix = other.UsernamePostfix;
			}
			if (other.AvatarLink.Length != 0)
			{
				AvatarLink = other.AvatarLink;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
				Username = input.ReadString();
				break;
			case 18u:
				UsernamePostfix = input.ReadString();
				break;
			case 26u:
				AvatarLink = input.ReadString();
				break;
			}
		}
	}
}
