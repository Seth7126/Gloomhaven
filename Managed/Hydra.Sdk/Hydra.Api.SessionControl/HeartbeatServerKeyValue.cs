using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public sealed class HeartbeatServerKeyValue : IMessage<HeartbeatServerKeyValue>, IMessage, IEquatable<HeartbeatServerKeyValue>, IDeepCloneable<HeartbeatServerKeyValue>, IBufferMessage
{
	private static readonly MessageParser<HeartbeatServerKeyValue> _parser = new MessageParser<HeartbeatServerKeyValue>(() => new HeartbeatServerKeyValue());

	private UnknownFieldSet _unknownFields;

	public const int ListFieldNumber = 1;

	private static readonly FieldCodec<SessionKeyValue> _repeated_list_codec = FieldCodec.ForMessage(10u, SessionKeyValue.Parser);

	private readonly RepeatedField<SessionKeyValue> list_ = new RepeatedField<SessionKeyValue>();

	[DebuggerNonUserCode]
	public static MessageParser<HeartbeatServerKeyValue> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SessionControlContractsReflection.Descriptor.MessageTypes[21];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<SessionKeyValue> List => list_;

	[DebuggerNonUserCode]
	public HeartbeatServerKeyValue()
	{
	}

	[DebuggerNonUserCode]
	public HeartbeatServerKeyValue(HeartbeatServerKeyValue other)
		: this()
	{
		list_ = other.list_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public HeartbeatServerKeyValue Clone()
	{
		return new HeartbeatServerKeyValue(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as HeartbeatServerKeyValue);
	}

	[DebuggerNonUserCode]
	public bool Equals(HeartbeatServerKeyValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!list_.Equals(other.list_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= list_.GetHashCode();
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
		list_.WriteTo(ref output, _repeated_list_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += list_.CalculateSize(_repeated_list_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(HeartbeatServerKeyValue other)
	{
		if (other != null)
		{
			list_.Add(other.list_);
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
				list_.AddEntriesFrom(ref input, _repeated_list_codec);
			}
		}
	}
}
