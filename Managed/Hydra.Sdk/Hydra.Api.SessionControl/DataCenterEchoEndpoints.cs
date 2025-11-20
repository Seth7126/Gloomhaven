using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class DataCenterEchoEndpoints : IMessage<DataCenterEchoEndpoints>, IMessage, IEquatable<DataCenterEchoEndpoints>, IDeepCloneable<DataCenterEchoEndpoints>, IBufferMessage
{
	private static readonly MessageParser<DataCenterEchoEndpoints> _parser = new MessageParser<DataCenterEchoEndpoints>(() => new DataCenterEchoEndpoints());

	private UnknownFieldSet _unknownFields;

	public const int DataCenterIdFieldNumber = 1;

	private string dataCenterId_ = "";

	public const int EndpointsFieldNumber = 2;

	private static readonly FieldCodec<EchoEndpoint> _repeated_endpoints_codec = FieldCodec.ForMessage(18u, EchoEndpoint.Parser);

	private readonly RepeatedField<EchoEndpoint> endpoints_ = new RepeatedField<EchoEndpoint>();

	[DebuggerNonUserCode]
	public static MessageParser<DataCenterEchoEndpoints> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[17];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string DataCenterId
	{
		get
		{
			return dataCenterId_;
		}
		set
		{
			dataCenterId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<EchoEndpoint> Endpoints => endpoints_;

	[DebuggerNonUserCode]
	public DataCenterEchoEndpoints()
	{
	}

	[DebuggerNonUserCode]
	public DataCenterEchoEndpoints(DataCenterEchoEndpoints other)
		: this()
	{
		dataCenterId_ = other.dataCenterId_;
		endpoints_ = other.endpoints_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DataCenterEchoEndpoints Clone()
	{
		return new DataCenterEchoEndpoints(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DataCenterEchoEndpoints);
	}

	[DebuggerNonUserCode]
	public bool Equals(DataCenterEchoEndpoints other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DataCenterId != other.DataCenterId)
		{
			return false;
		}
		if (!endpoints_.Equals(other.endpoints_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (DataCenterId.Length != 0)
		{
			num ^= DataCenterId.GetHashCode();
		}
		num ^= endpoints_.GetHashCode();
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
		if (DataCenterId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(DataCenterId);
		}
		endpoints_.WriteTo(ref output, _repeated_endpoints_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (DataCenterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DataCenterId);
		}
		num += endpoints_.CalculateSize(_repeated_endpoints_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(DataCenterEchoEndpoints other)
	{
		if (other != null)
		{
			if (other.DataCenterId.Length != 0)
			{
				DataCenterId = other.DataCenterId;
			}
			endpoints_.Add(other.endpoints_);
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				DataCenterId = input.ReadString();
				break;
			case 18u:
				endpoints_.AddEntriesFrom(ref input, _repeated_endpoints_codec);
				break;
			}
		}
	}
}
