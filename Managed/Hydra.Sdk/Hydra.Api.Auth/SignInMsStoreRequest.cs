using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInMsStoreRequest : IMessage<SignInMsStoreRequest>, IMessage, IEquatable<SignInMsStoreRequest>, IDeepCloneable<SignInMsStoreRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInMsStoreRequest> _parser = new MessageParser<SignInMsStoreRequest>(() => new SignInMsStoreRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int XstsClientTokenFieldNumber = 2;

	private string xstsClientToken_ = "";

	public const int XstoreClientTokenFieldNumber = 3;

	private string xstoreClientToken_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInMsStoreRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[5];

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
	public string XstsClientToken
	{
		get
		{
			return xstsClientToken_;
		}
		set
		{
			xstsClientToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string XstoreClientToken
	{
		get
		{
			return xstoreClientToken_;
		}
		set
		{
			xstoreClientToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInMsStoreRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInMsStoreRequest(SignInMsStoreRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		xstsClientToken_ = other.xstsClientToken_;
		xstoreClientToken_ = other.xstoreClientToken_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInMsStoreRequest Clone()
	{
		return new SignInMsStoreRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInMsStoreRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInMsStoreRequest other)
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
		if (XstsClientToken != other.XstsClientToken)
		{
			return false;
		}
		if (XstoreClientToken != other.XstoreClientToken)
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
		if (XstsClientToken.Length != 0)
		{
			num ^= XstsClientToken.GetHashCode();
		}
		if (XstoreClientToken.Length != 0)
		{
			num ^= XstoreClientToken.GetHashCode();
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
		if (XstsClientToken.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(XstsClientToken);
		}
		if (XstoreClientToken.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(XstoreClientToken);
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
		if (XstsClientToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(XstsClientToken);
		}
		if (XstoreClientToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(XstoreClientToken);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInMsStoreRequest other)
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
		if (other.XstsClientToken.Length != 0)
		{
			XstsClientToken = other.XstsClientToken;
		}
		if (other.XstoreClientToken.Length != 0)
		{
			XstoreClientToken = other.XstoreClientToken;
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
				XstsClientToken = input.ReadString();
				break;
			case 26u:
				XstoreClientToken = input.ReadString();
				break;
			}
		}
	}
}
