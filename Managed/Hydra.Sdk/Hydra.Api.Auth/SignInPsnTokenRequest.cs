using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInPsnTokenRequest : IMessage<SignInPsnTokenRequest>, IMessage, IEquatable<SignInPsnTokenRequest>, IDeepCloneable<SignInPsnTokenRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInPsnTokenRequest> _parser = new MessageParser<SignInPsnTokenRequest>(() => new SignInPsnTokenRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int IdTokenFieldNumber = 2;

	private string idToken_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInPsnTokenRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[8];

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
	public string IdToken
	{
		get
		{
			return idToken_;
		}
		set
		{
			idToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInPsnTokenRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInPsnTokenRequest(SignInPsnTokenRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		idToken_ = other.idToken_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInPsnTokenRequest Clone()
	{
		return new SignInPsnTokenRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInPsnTokenRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInPsnTokenRequest other)
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
		if (IdToken != other.IdToken)
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
		if (IdToken.Length != 0)
		{
			num ^= IdToken.GetHashCode();
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
		if (IdToken.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(IdToken);
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
		if (IdToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(IdToken);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInPsnTokenRequest other)
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
		if (other.IdToken.Length != 0)
		{
			IdToken = other.IdToken;
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
				IdToken = input.ReadString();
				break;
			}
		}
	}
}
