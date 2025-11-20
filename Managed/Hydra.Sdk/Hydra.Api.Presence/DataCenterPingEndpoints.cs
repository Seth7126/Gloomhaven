using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.EndpointDispatcher;

namespace Hydra.Api.Presence;

public sealed class DataCenterPingEndpoints : IMessage<DataCenterPingEndpoints>, IMessage, IEquatable<DataCenterPingEndpoints>, IDeepCloneable<DataCenterPingEndpoints>, IBufferMessage
{
	private static readonly MessageParser<DataCenterPingEndpoints> _parser = new MessageParser<DataCenterPingEndpoints>(() => new DataCenterPingEndpoints());

	private UnknownFieldSet _unknownFields;

	public const int DataCenterIdFieldNumber = 1;

	private string dataCenterId_ = "";

	public const int EndpointsFieldNumber = 2;

	private static readonly FieldCodec<EndpointInfo> _repeated_endpoints_codec = FieldCodec.ForMessage(18u, EndpointInfo.Parser);

	private readonly RepeatedField<EndpointInfo> endpoints_ = new RepeatedField<EndpointInfo>();

	[DebuggerNonUserCode]
	public static MessageParser<DataCenterPingEndpoints> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[60];

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
	public RepeatedField<EndpointInfo> Endpoints => endpoints_;

	[DebuggerNonUserCode]
	public DataCenterPingEndpoints()
	{
	}

	[DebuggerNonUserCode]
	public DataCenterPingEndpoints(DataCenterPingEndpoints other)
		: this()
	{
		dataCenterId_ = other.dataCenterId_;
		endpoints_ = other.endpoints_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DataCenterPingEndpoints Clone()
	{
		return new DataCenterPingEndpoints(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DataCenterPingEndpoints);
	}

	[DebuggerNonUserCode]
	public bool Equals(DataCenterPingEndpoints other)
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
	public void MergeFrom(DataCenterPingEndpoints other)
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
