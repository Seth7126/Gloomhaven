using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Nullable;

public sealed class NullableBool : IMessage<NullableBool>, IMessage, IEquatable<NullableBool>, IDeepCloneable<NullableBool>, IBufferMessage
{
	public enum KindOneofCase
	{
		None,
		Null,
		Data
	}

	private static readonly MessageParser<NullableBool> _parser = new MessageParser<NullableBool>(() => new NullableBool());

	private UnknownFieldSet _unknownFields;

	public const int NullFieldNumber = 1;

	public const int DataFieldNumber = 2;

	private object kind_;

	private KindOneofCase kindCase_ = KindOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<NullableBool> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => NullableReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public NullValue Null
	{
		get
		{
			return (kindCase_ == KindOneofCase.Null) ? ((NullValue)kind_) : NullValue.NullValue;
		}
		set
		{
			kind_ = value;
			kindCase_ = KindOneofCase.Null;
		}
	}

	[DebuggerNonUserCode]
	public bool Data
	{
		get
		{
			return kindCase_ == KindOneofCase.Data && (bool)kind_;
		}
		set
		{
			kind_ = value;
			kindCase_ = KindOneofCase.Data;
		}
	}

	[DebuggerNonUserCode]
	public KindOneofCase KindCase => kindCase_;

	[DebuggerNonUserCode]
	public NullableBool()
	{
	}

	[DebuggerNonUserCode]
	public NullableBool(NullableBool other)
		: this()
	{
		switch (other.KindCase)
		{
		case KindOneofCase.Null:
			Null = other.Null;
			break;
		case KindOneofCase.Data:
			Data = other.Data;
			break;
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public NullableBool Clone()
	{
		return new NullableBool(this);
	}

	[DebuggerNonUserCode]
	public void ClearKind()
	{
		kindCase_ = KindOneofCase.None;
		kind_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as NullableBool);
	}

	[DebuggerNonUserCode]
	public bool Equals(NullableBool other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Null != other.Null)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (KindCase != other.KindCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (kindCase_ == KindOneofCase.Null)
		{
			num ^= Null.GetHashCode();
		}
		if (kindCase_ == KindOneofCase.Data)
		{
			num ^= Data.GetHashCode();
		}
		num ^= (int)kindCase_;
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
		if (kindCase_ == KindOneofCase.Null)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)Null);
		}
		if (kindCase_ == KindOneofCase.Data)
		{
			output.WriteRawTag(16);
			output.WriteBool(Data);
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
		if (kindCase_ == KindOneofCase.Null)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Null);
		}
		if (kindCase_ == KindOneofCase.Data)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(NullableBool other)
	{
		if (other != null)
		{
			switch (other.KindCase)
			{
			case KindOneofCase.Null:
				Null = other.Null;
				break;
			case KindOneofCase.Data:
				Data = other.Data;
				break;
			}
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
			case 8u:
				kind_ = input.ReadEnum();
				kindCase_ = KindOneofCase.Null;
				break;
			case 16u:
				Data = input.ReadBool();
				break;
			}
		}
	}
}
