using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInHydraRequest : IMessage<SignInHydraRequest>, IMessage, IEquatable<SignInHydraRequest>, IDeepCloneable<SignInHydraRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInHydraRequest> _parser = new MessageParser<SignInHydraRequest>(() => new SignInHydraRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int LoginFieldNumber = 2;

	private string login_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInHydraRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[9];

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
	public string Login
	{
		get
		{
			return login_;
		}
		set
		{
			login_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInHydraRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInHydraRequest(SignInHydraRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		login_ = other.login_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInHydraRequest Clone()
	{
		return new SignInHydraRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInHydraRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInHydraRequest other)
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
		if (Login != other.Login)
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
		if (Login.Length != 0)
		{
			num ^= Login.GetHashCode();
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
		if (Login.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Login);
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
		if (Login.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Login);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInHydraRequest other)
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
		if (other.Login.Length != 0)
		{
			Login = other.Login;
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
				Login = input.ReadString();
				break;
			}
		}
	}
}
