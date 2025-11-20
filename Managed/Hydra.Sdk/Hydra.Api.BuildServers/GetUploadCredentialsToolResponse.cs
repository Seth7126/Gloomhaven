using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.BuildServers;

public sealed class GetUploadCredentialsToolResponse : IMessage<GetUploadCredentialsToolResponse>, IMessage, IEquatable<GetUploadCredentialsToolResponse>, IDeepCloneable<GetUploadCredentialsToolResponse>, IBufferMessage
{
	private static readonly MessageParser<GetUploadCredentialsToolResponse> _parser = new MessageParser<GetUploadCredentialsToolResponse>(() => new GetUploadCredentialsToolResponse());

	private UnknownFieldSet _unknownFields;

	public const int PackIdFieldNumber = 1;

	private string packId_ = "";

	public const int TokenFieldNumber = 2;

	private string token_ = "";

	public const int TokenTypeFieldNumber = 3;

	private CredentialsTokenType tokenType_ = CredentialsTokenType.Unknown;

	public const int ExpiresOnFieldNumber = 4;

	private Timestamp expiresOn_;

	[DebuggerNonUserCode]
	public static MessageParser<GetUploadCredentialsToolResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => BuildServersContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PackId
	{
		get
		{
			return packId_;
		}
		set
		{
			packId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Token
	{
		get
		{
			return token_;
		}
		set
		{
			token_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public CredentialsTokenType TokenType
	{
		get
		{
			return tokenType_;
		}
		set
		{
			tokenType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Timestamp ExpiresOn
	{
		get
		{
			return expiresOn_;
		}
		set
		{
			expiresOn_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolResponse(GetUploadCredentialsToolResponse other)
		: this()
	{
		packId_ = other.packId_;
		token_ = other.token_;
		tokenType_ = other.tokenType_;
		expiresOn_ = ((other.expiresOn_ != null) ? other.expiresOn_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetUploadCredentialsToolResponse Clone()
	{
		return new GetUploadCredentialsToolResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetUploadCredentialsToolResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetUploadCredentialsToolResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PackId != other.PackId)
		{
			return false;
		}
		if (Token != other.Token)
		{
			return false;
		}
		if (TokenType != other.TokenType)
		{
			return false;
		}
		if (!object.Equals(ExpiresOn, other.ExpiresOn))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PackId.Length != 0)
		{
			num ^= PackId.GetHashCode();
		}
		if (Token.Length != 0)
		{
			num ^= Token.GetHashCode();
		}
		if (TokenType != CredentialsTokenType.Unknown)
		{
			num ^= TokenType.GetHashCode();
		}
		if (expiresOn_ != null)
		{
			num ^= ExpiresOn.GetHashCode();
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
		if (PackId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PackId);
		}
		if (Token.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Token);
		}
		if (TokenType != CredentialsTokenType.Unknown)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)TokenType);
		}
		if (expiresOn_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(ExpiresOn);
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
		if (PackId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PackId);
		}
		if (Token.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Token);
		}
		if (TokenType != CredentialsTokenType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TokenType);
		}
		if (expiresOn_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ExpiresOn);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetUploadCredentialsToolResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.PackId.Length != 0)
		{
			PackId = other.PackId;
		}
		if (other.Token.Length != 0)
		{
			Token = other.Token;
		}
		if (other.TokenType != CredentialsTokenType.Unknown)
		{
			TokenType = other.TokenType;
		}
		if (other.expiresOn_ != null)
		{
			if (expiresOn_ == null)
			{
				ExpiresOn = new Timestamp();
			}
			ExpiresOn.MergeFrom(other.ExpiresOn);
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
				PackId = input.ReadString();
				break;
			case 18u:
				Token = input.ReadString();
				break;
			case 24u:
				TokenType = (CredentialsTokenType)input.ReadEnum();
				break;
			case 34u:
				if (expiresOn_ == null)
				{
					ExpiresOn = new Timestamp();
				}
				input.ReadMessage(ExpiresOn);
				break;
			}
		}
	}
}
