using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public sealed class GetRegistrationStatusResponse : IMessage<GetRegistrationStatusResponse>, IMessage, IEquatable<GetRegistrationStatusResponse>, IDeepCloneable<GetRegistrationStatusResponse>, IBufferMessage
{
	private static readonly MessageParser<GetRegistrationStatusResponse> _parser = new MessageParser<GetRegistrationStatusResponse>(() => new GetRegistrationStatusResponse());

	private UnknownFieldSet _unknownFields;

	public const int LinkStatusFieldNumber = 1;

	private LinkStatus linkStatus_;

	public const int AccountInfoFieldNumber = 2;

	private AccountInfo accountInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GetRegistrationStatusResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => AccountContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LinkStatus LinkStatus
	{
		get
		{
			return linkStatus_;
		}
		set
		{
			linkStatus_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountInfo AccountInfo
	{
		get
		{
			return accountInfo_;
		}
		set
		{
			accountInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationStatusResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationStatusResponse(GetRegistrationStatusResponse other)
		: this()
	{
		linkStatus_ = other.linkStatus_;
		accountInfo_ = ((other.accountInfo_ != null) ? other.accountInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GetRegistrationStatusResponse Clone()
	{
		return new GetRegistrationStatusResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GetRegistrationStatusResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GetRegistrationStatusResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LinkStatus != other.LinkStatus)
		{
			return false;
		}
		if (!object.Equals(AccountInfo, other.AccountInfo))
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
		if (LinkStatus != LinkStatus.Unknown)
		{
			num ^= LinkStatus.GetHashCode();
		}
		if (accountInfo_ != null)
		{
			num ^= AccountInfo.GetHashCode();
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
		if (LinkStatus != LinkStatus.Unknown)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)LinkStatus);
		}
		if (accountInfo_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(AccountInfo);
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
		if (LinkStatus != LinkStatus.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LinkStatus);
		}
		if (accountInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(AccountInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GetRegistrationStatusResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.LinkStatus != LinkStatus.Unknown)
		{
			LinkStatus = other.LinkStatus;
		}
		if (other.accountInfo_ != null)
		{
			if (accountInfo_ == null)
			{
				AccountInfo = new AccountInfo();
			}
			AccountInfo.MergeFrom(other.AccountInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			case 8u:
				LinkStatus = (LinkStatus)input.ReadEnum();
				break;
			case 18u:
				if (accountInfo_ == null)
				{
					AccountInfo = new AccountInfo();
				}
				input.ReadMessage(AccountInfo);
				break;
			}
		}
	}
}
