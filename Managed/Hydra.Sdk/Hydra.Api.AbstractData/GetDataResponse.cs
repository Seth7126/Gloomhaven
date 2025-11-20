using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.AbstractData;

public sealed class GetDataResponse : IMessage<GetDataResponse>, IMessage, IEquatable<GetDataResponse>, IDeepCloneable<GetDataResponse>, IBufferMessage
{
	private static readonly MessageParser<GetDataResponse> _parser = new MessageParser<GetDataResponse>(() => new GetDataResponse());

	private UnknownFieldSet _unknownFields;

	public const int DataFieldNumber = 1;

	private static readonly FieldCodec<AbstractDataKeyContainers> _repeated_data_codec = FieldCodec.ForMessage(10u, AbstractDataKeyContainers.Parser);

	private readonly RepeatedField<AbstractDataKeyContainers> data_ = new RepeatedField<AbstractDataKeyContainers>();

	[DebuggerNonUserCode]
	public static MessageParser<GetDataResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<AbstractDataKeyContainers> Data => data_;

	[DebuggerNonUserCode]
	public GetDataResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetDataResponse(GetDataResponse other)
		: this()
	{
		data_ = other.data_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetDataResponse Clone()
	{
		return new GetDataResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetDataResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetDataResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!data_.Equals(other.data_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= data_.GetHashCode();
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
		data_.WriteTo(ref output, _repeated_data_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += data_.CalculateSize(_repeated_data_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetDataResponse other)
	{
		if (other != null)
		{
			data_.Add(other.data_);
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
				data_.AddEntriesFrom(ref input, _repeated_data_codec);
			}
		}
	}
}
