using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInOculusRequest : IMessage<SignInOculusRequest>, IMessage, IEquatable<SignInOculusRequest>, IDeepCloneable<SignInOculusRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInOculusRequest> _parser = new MessageParser<SignInOculusRequest>(() => new SignInOculusRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int NonceFieldNumber = 2;

	private string nonce_ = "";

	public const int UserIdFieldNumber = 3;

	private long userId_;

	[DebuggerNonUserCode]
	public static MessageParser<SignInOculusRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[12];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public SignInData Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Nonce
	{
		get
		{
			return nonce_;
		}
		set
		{
			nonce_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public long UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SignInOculusRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInOculusRequest(SignInOculusRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		nonce_ = other.nonce_;
		userId_ = other.userId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInOculusRequest Clone()
	{
		return new SignInOculusRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInOculusRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInOculusRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		if (Nonce != other.Nonce)
		{
			return false;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
		}
		if (Nonce.Length != 0)
		{
			num ^= Nonce.GetHashCode();
		}
		if (UserId != 0)
		{
			num ^= UserId.GetHashCode();
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
		if (data_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Data);
		}
		if (Nonce.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Nonce);
		}
		if (UserId != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(UserId);
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
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (Nonce.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Nonce);
		}
		if (UserId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(UserId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInOculusRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new SignInData();
			}
			Data.MergeFrom(other.Data);
		}
		if (other.Nonce.Length != 0)
		{
			Nonce = other.Nonce;
		}
		if (other.UserId != 0)
		{
			UserId = other.UserId;
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
				if (data_ == null)
				{
					Data = new SignInData();
				}
				input.ReadMessage(Data);
				break;
			case 18u:
				Nonce = input.ReadString();
				break;
			case 24u:
				UserId = input.ReadInt64();
				break;
			}
		}
	}
}
