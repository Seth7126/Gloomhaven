using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Auth;

public sealed class SignInHydraAuthTicketRequest : IMessage<SignInHydraAuthTicketRequest>, IMessage, IEquatable<SignInHydraAuthTicketRequest>, IDeepCloneable<SignInHydraAuthTicketRequest>, IBufferMessage
{
	private static readonly MessageParser<SignInHydraAuthTicketRequest> _parser = new MessageParser<SignInHydraAuthTicketRequest>(() => new SignInHydraAuthTicketRequest());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private SignInData data_;

	public const int HydraAuthTicketFieldNumber = 2;

	private string hydraAuthTicket_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SignInHydraAuthTicketRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AuthorizationContractsReflection.Descriptor.MessageTypes[31];

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
	public string HydraAuthTicket
	{
		get
		{
			return hydraAuthTicket_;
		}
		set
		{
			hydraAuthTicket_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SignInHydraAuthTicketRequest()
	{
	}

	[DebuggerNonUserCode]
	public SignInHydraAuthTicketRequest(SignInHydraAuthTicketRequest other)
		: this()
	{
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		hydraAuthTicket_ = other.hydraAuthTicket_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SignInHydraAuthTicketRequest Clone()
	{
		return new SignInHydraAuthTicketRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SignInHydraAuthTicketRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SignInHydraAuthTicketRequest other)
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
		if (HydraAuthTicket != other.HydraAuthTicket)
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
		if (HydraAuthTicket.Length != 0)
		{
			num ^= HydraAuthTicket.GetHashCode();
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
		if (HydraAuthTicket.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(HydraAuthTicket);
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
		if (HydraAuthTicket.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(HydraAuthTicket);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SignInHydraAuthTicketRequest other)
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
		if (other.HydraAuthTicket.Length != 0)
		{
			HydraAuthTicket = other.HydraAuthTicket;
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
				HydraAuthTicket = input.ReadString();
				break;
			}
		}
	}
}
