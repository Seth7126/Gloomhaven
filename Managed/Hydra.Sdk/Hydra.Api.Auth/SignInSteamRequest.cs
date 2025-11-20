using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInSteamRequest : IMessage<SignInSteamRequest>, IMessage, IEquatable<SignInSteamRequest>, IDeepCloneable<SignInSteamRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInSteamRequest> _parser = new MessageParser<SignInSteamRequest>(() => new SignInSteamRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int AuthSessionTicketFieldNumber = 2;

	private string authSessionTicket_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInSteamRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[10];

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
	public string AuthSessionTicket
	{
		get
		{
			return authSessionTicket_;
		}
		set
		{
			authSessionTicket_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInSteamRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInSteamRequest(SignInSteamRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		authSessionTicket_ = other.authSessionTicket_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInSteamRequest Clone()
	{
		return new SignInSteamRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInSteamRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInSteamRequest other)
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
		if (AuthSessionTicket != other.AuthSessionTicket)
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
		if (AuthSessionTicket.Length != 0)
		{
			num ^= AuthSessionTicket.GetHashCode();
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
		if (AuthSessionTicket.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(AuthSessionTicket);
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
		if (AuthSessionTicket.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AuthSessionTicket);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInSteamRequest other)
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
		if (other.AuthSessionTicket.Length != 0)
		{
			AuthSessionTicket = other.AuthSessionTicket;
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
				AuthSessionTicket = input.ReadString();
				break;
			}
		}
	}
}
