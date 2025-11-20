using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Hydra.Api.Nullable;

public sealed class NullableString : IMessage<NullableString>, IMessage, IEquatable<NullableString>, IDeepCloneable<NullableString>, IBufferMessage
{
	public enum KindOneofCase
	{
		None,
		Null,
		Data
	}

	private static readonly MessageParser<NullableString> _parser = new MessageParser<NullableString>(() => new NullableString());

	private UnknownFieldSet _unknownFields;

	public const int NullFieldNumber = 1;

	public const int DataFieldNumber = 2;

	private object kind_;

	private KindOneofCase kindCase_ = KindOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<NullableString> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => NullableReflection.Descriptor.MessageTypes[0];

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
	public string Data
	{
		get
		{
			return (kindCase_ == KindOneofCase.Data) ? ((string)kind_) : "";
		}
		set
		{
			kind_ = ProtoPreconditions.CheckNotNull(value, "value");
			kindCase_ = KindOneofCase.Data;
		}
	}

	[DebuggerNonUserCode]
	public KindOneofCase KindCase => kindCase_;

	[DebuggerNonUserCode]
	public NullableString()
	{
	}

	[DebuggerNonUserCode]
	public NullableString(NullableString other)
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
	public NullableString Clone()
	{
		return new NullableString(this);
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
		return Equals(other as NullableString);
	}

	[DebuggerNonUserCode]
	public bool Equals(NullableString other)
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
			output.WriteRawTag(18);
			output.WriteString(Data);
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
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(NullableString other)
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
			case 18u:
				Data = input.ReadString();
				break;
			}
		}
	}
}
