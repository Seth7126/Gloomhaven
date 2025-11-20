using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.EndpointDispatcher;

public sealed class GetEnvironmentInfoResponse : IMessage<GetEnvironmentInfoResponse>, IMessage, IEquatable<GetEnvironmentInfoResponse>, IDeepCloneable<GetEnvironmentInfoResponse>, IBufferMessage
{
	private static readonly MessageParser<GetEnvironmentInfoResponse> _parser = new MessageParser<GetEnvironmentInfoResponse>(() => new GetEnvironmentInfoResponse());

	private UnknownFieldSet _unknownFields;

	public const int DateFieldNumber = 1;

	private long date_;

	public const int EnvironmentFieldNumber = 2;

	private EnvironmentInfo environment_;

	[DebuggerNonUserCode]
	public static MessageParser<GetEnvironmentInfoResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EndpointDispatcherContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Date
	{
		get
		{
			return date_;
		}
		set
		{
			date_ = value;
		}
	}

	[DebuggerNonUserCode]
	public EnvironmentInfo Environment
	{
		get
		{
			return environment_;
		}
		set
		{
			environment_ = value;
		}
	}

	[DebuggerNonUserCode]
	public GetEnvironmentInfoResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetEnvironmentInfoResponse(GetEnvironmentInfoResponse other)
		: this()
	{
		date_ = other.date_;
		environment_ = ((other.environment_ != null) ? other.environment_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetEnvironmentInfoResponse Clone()
	{
		return new GetEnvironmentInfoResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetEnvironmentInfoResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetEnvironmentInfoResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Date != other.Date)
		{
			return false;
		}
		if (!object.Equals(Environment, other.Environment))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Date != 0)
		{
			num ^= Date.GetHashCode();
		}
		if (environment_ != null)
		{
			num ^= Environment.GetHashCode();
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
		if (Date != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Date);
		}
		if (environment_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Environment);
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
		if (Date != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Date);
		}
		if (environment_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Environment);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetEnvironmentInfoResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Date != 0)
		{
			Date = other.Date;
		}
		if (other.environment_ != null)
		{
			if (environment_ == null)
			{
				Environment = new EnvironmentInfo();
			}
			Environment.MergeFrom(other.Environment);
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
			case 8u:
				Date = input.ReadInt64();
				break;
			case 18u:
				if (environment_ == null)
				{
					Environment = new EnvironmentInfo();
				}
				input.ReadMessage(Environment);
				break;
			}
		}
	}
}
