using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class VectorStringValue : IMessage<VectorStringValue>, IMessage, IEquatable<VectorStringValue>, IDeepCloneable<VectorStringValue>, IBufferMessage
{
	private static readonly MessageParser<VectorStringValue> _parser = new MessageParser<VectorStringValue>(() => new VectorStringValue());

	private UnknownFieldSet _unknownFields;

	public const int ValueFieldNumber = 1;

	private static readonly FieldCodec<string> _repeated_value_codec = FieldCodec.ForString(10u);

	private readonly RepeatedField<string> value_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<VectorStringValue> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[23];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<string> Value => value_;

	[DebuggerNonUserCode]
	public VectorStringValue()
	{
	}

	[DebuggerNonUserCode]
	public VectorStringValue(VectorStringValue other)
		: this()
	{
		value_ = other.value_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public VectorStringValue Clone()
	{
		return new VectorStringValue(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as VectorStringValue);
	}

	[DebuggerNonUserCode]
	public bool Equals(VectorStringValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!value_.Equals(other.value_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= value_.GetHashCode();
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
		value_.WriteTo(ref output, _repeated_value_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += value_.CalculateSize(_repeated_value_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(VectorStringValue other)
	{
		if (other != null)
		{
			value_.Add(other.value_);
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
				value_.AddEntriesFrom(ref input, _repeated_value_codec);
			}
		}
	}
}
