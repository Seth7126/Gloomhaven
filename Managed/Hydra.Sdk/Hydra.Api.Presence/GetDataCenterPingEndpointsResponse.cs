using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public sealed class GetDataCenterPingEndpointsResponse : IMessage<GetDataCenterPingEndpointsResponse>, IMessage, IEquatable<GetDataCenterPingEndpointsResponse>, IDeepCloneable<GetDataCenterPingEndpointsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetDataCenterPingEndpointsResponse> _parser = new MessageParser<GetDataCenterPingEndpointsResponse>(() => new GetDataCenterPingEndpointsResponse());

	private UnknownFieldSet _unknownFields;

	public const int DataCenterPingEndpointsFieldNumber = 1;

	private static readonly FieldCodec<DataCenterPingEndpoints> _repeated_dataCenterPingEndpoints_codec = FieldCodec.ForMessage(10u, Hydra.Api.Presence.DataCenterPingEndpoints.Parser);

	private readonly RepeatedField<DataCenterPingEndpoints> dataCenterPingEndpoints_ = new RepeatedField<DataCenterPingEndpoints>();

	[DebuggerNonUserCode]
	public static MessageParser<GetDataCenterPingEndpointsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceServiceContractsReflection.Descriptor.MessageTypes[62];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<DataCenterPingEndpoints> DataCenterPingEndpoints => dataCenterPingEndpoints_;

	[DebuggerNonUserCode]
	public GetDataCenterPingEndpointsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetDataCenterPingEndpointsResponse(GetDataCenterPingEndpointsResponse other)
		: this()
	{
		dataCenterPingEndpoints_ = other.dataCenterPingEndpoints_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataCenterPingEndpointsResponse Clone()
	{
		return new GetDataCenterPingEndpointsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataCenterPingEndpointsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataCenterPingEndpointsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!dataCenterPingEndpoints_.Equals(other.dataCenterPingEndpoints_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= dataCenterPingEndpoints_.GetHashCode();
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
		dataCenterPingEndpoints_.WriteTo(ref output, _repeated_dataCenterPingEndpoints_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += dataCenterPingEndpoints_.CalculateSize(_repeated_dataCenterPingEndpoints_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetDataCenterPingEndpointsResponse other)
	{
		if (other != null)
		{
			dataCenterPingEndpoints_.Add(other.dataCenterPingEndpoints_);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				dataCenterPingEndpoints_.AddEntriesFrom(ref input, _repeated_dataCenterPingEndpoints_codec);
			}
		}
	}
}
