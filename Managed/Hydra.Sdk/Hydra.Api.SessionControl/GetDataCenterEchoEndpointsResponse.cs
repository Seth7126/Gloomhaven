using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class GetDataCenterEchoEndpointsResponse : IMessage<GetDataCenterEchoEndpointsResponse>, IMessage, IEquatable<GetDataCenterEchoEndpointsResponse>, IDeepCloneable<GetDataCenterEchoEndpointsResponse>, IBufferMessage
{
	private static readonly MessageParser<GetDataCenterEchoEndpointsResponse> _parser = new MessageParser<GetDataCenterEchoEndpointsResponse>(() => new GetDataCenterEchoEndpointsResponse());

	private UnknownFieldSet _unknownFields;

	public const int EndpointsFieldNumber = 1;

	private static readonly FieldCodec<DataCenterEchoEndpoints> _repeated_endpoints_codec = FieldCodec.ForMessage(10u, DataCenterEchoEndpoints.Parser);

	private readonly RepeatedField<DataCenterEchoEndpoints> endpoints_ = new RepeatedField<DataCenterEchoEndpoints>();

	[DebuggerNonUserCode]
	public static MessageParser<GetDataCenterEchoEndpointsResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[18];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<DataCenterEchoEndpoints> Endpoints => endpoints_;

	[DebuggerNonUserCode]
	public GetDataCenterEchoEndpointsResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetDataCenterEchoEndpointsResponse(GetDataCenterEchoEndpointsResponse other)
		: this()
	{
		endpoints_ = other.endpoints_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataCenterEchoEndpointsResponse Clone()
	{
		return new GetDataCenterEchoEndpointsResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataCenterEchoEndpointsResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataCenterEchoEndpointsResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		num += endpoints_.CalculateSize(_repeated_endpoints_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetDataCenterEchoEndpointsResponse other)
	{
		if (other != null)
		{
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				endpoints_.AddEntriesFrom(ref input, _repeated_endpoints_codec);
			}
		}
	}
}
