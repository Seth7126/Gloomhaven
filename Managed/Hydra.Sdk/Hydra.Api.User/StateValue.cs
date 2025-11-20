using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.User;

public sealed class StateValue : IMessage<StateValue>, IMessage, IEquatable<StateValue>, IDeepCloneable<StateValue>, IBufferMessage
{
	public enum ValueOneofCase
	{
		None = 0,
		Int64Value = 3,
		StringValue = 4,
		VectorStringValue = 5
	}

	private static readonly MessageParser<StateValue> _parser = new MessageParser<StateValue>(() => new StateValue());

	private UnknownFieldSet _unknownFields;

	public const int StateOwnTypeFieldNumber = 1;

	private UserStateOwnType stateOwnType_ = UserStateOwnType.Unknown;

	public const int StateDataTypeFieldNumber = 2;

	private UserStateDataType stateDataType_ = UserStateDataType.Unknown;

	public const int Int64ValueFieldNumber = 3;

	public const int StringValueFieldNumber = 4;

	public const int VectorStringValueFieldNumber = 5;

	private object value_;

	private ValueOneofCase valueCase_ = ValueOneofCase.None;

	[DebuggerNonUserCode]
	public static MessageParser<StateValue> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => EconomyContractsReflection.Descriptor.MessageTypes[18];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserStateOwnType StateOwnType
	{
		get
		{
			return stateOwnType_;
		}
		set
		{
			stateOwnType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public UserStateDataType StateDataType
	{
		get
		{
			return stateDataType_;
		}
		set
		{
			stateDataType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public Int64Value Int64Value
	{
		get
		{
			return (valueCase_ == ValueOneofCase.Int64Value) ? ((Int64Value)value_) : null;
		}
		set
		{
			value_ = value;
			valueCase_ = ((value != null) ? ValueOneofCase.Int64Value : ValueOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public StringValue StringValue
	{
		get
		{
			return (valueCase_ == ValueOneofCase.StringValue) ? ((StringValue)value_) : null;
		}
		set
		{
			value_ = value;
			valueCase_ = ((value != null) ? ValueOneofCase.StringValue : ValueOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public VectorStringValue VectorStringValue
	{
		get
		{
			return (valueCase_ == ValueOneofCase.VectorStringValue) ? ((VectorStringValue)value_) : null;
		}
		set
		{
			value_ = value;
			valueCase_ = ((value != null) ? ValueOneofCase.VectorStringValue : ValueOneofCase.None);
		}
	}

	[DebuggerNonUserCode]
	public ValueOneofCase ValueCase => valueCase_;

	[DebuggerNonUserCode]
	public StateValue()
	{
	}

	[DebuggerNonUserCode]
	public StateValue(StateValue other)
		: this()
	{
		stateOwnType_ = other.stateOwnType_;
		stateDataType_ = other.stateDataType_;
		switch (other.ValueCase)
		{
		case ValueOneofCase.Int64Value:
			Int64Value = other.Int64Value.Clone();
			break;
		case ValueOneofCase.StringValue:
			StringValue = other.StringValue.Clone();
			break;
		case ValueOneofCase.VectorStringValue:
			VectorStringValue = other.VectorStringValue.Clone();
			break;
		}
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public StateValue Clone()
	{
		return new StateValue(this);
	}

	[DebuggerNonUserCode]
	public void ClearValue()
	{
		valueCase_ = ValueOneofCase.None;
		value_ = null;
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as StateValue);
	}

	[DebuggerNonUserCode]
	public bool Equals(StateValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (StateOwnType != other.StateOwnType)
		{
			return false;
		}
		if (StateDataType != other.StateDataType)
		{
			return false;
		}
		if (!object.Equals(Int64Value, other.Int64Value))
		{
			return false;
		}
		if (!object.Equals(StringValue, other.StringValue))
		{
			return false;
		}
		if (!object.Equals(VectorStringValue, other.VectorStringValue))
		{
			return false;
		}
		if (ValueCase != other.ValueCase)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (StateOwnType != UserStateOwnType.Unknown)
		{
			num ^= StateOwnType.GetHashCode();
		}
		if (StateDataType != UserStateDataType.Unknown)
		{
			num ^= StateDataType.GetHashCode();
		}
		if (valueCase_ == ValueOneofCase.Int64Value)
		{
			num ^= Int64Value.GetHashCode();
		}
		if (valueCase_ == ValueOneofCase.StringValue)
		{
			num ^= StringValue.GetHashCode();
		}
		if (valueCase_ == ValueOneofCase.VectorStringValue)
		{
			num ^= VectorStringValue.GetHashCode();
		}
		num ^= (int)valueCase_;
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
		if (StateOwnType != UserStateOwnType.Unknown)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)StateOwnType);
		}
		if (StateDataType != UserStateDataType.Unknown)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)StateDataType);
		}
		if (valueCase_ == ValueOneofCase.Int64Value)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Int64Value);
		}
		if (valueCase_ == ValueOneofCase.StringValue)
		{
			output.WriteRawTag(34);
			output.WriteMessage(StringValue);
		}
		if (valueCase_ == ValueOneofCase.VectorStringValue)
		{
			output.WriteRawTag(42);
			output.WriteMessage(VectorStringValue);
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
		if (StateOwnType != UserStateOwnType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)StateOwnType);
		}
		if (StateDataType != UserStateDataType.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)StateDataType);
		}
		if (valueCase_ == ValueOneofCase.Int64Value)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Int64Value);
		}
		if (valueCase_ == ValueOneofCase.StringValue)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(StringValue);
		}
		if (valueCase_ == ValueOneofCase.VectorStringValue)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(VectorStringValue);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(StateValue other)
	{
		if (other == null)
		{
			return;
		}
		if (other.StateOwnType != UserStateOwnType.Unknown)
		{
			StateOwnType = other.StateOwnType;
		}
		if (other.StateDataType != UserStateDataType.Unknown)
		{
			StateDataType = other.StateDataType;
		}
		switch (other.ValueCase)
		{
		case ValueOneofCase.Int64Value:
			if (Int64Value == null)
			{
				Int64Value = new Int64Value();
			}
			Int64Value.MergeFrom(other.Int64Value);
			break;
		case ValueOneofCase.StringValue:
			if (StringValue == null)
			{
				StringValue = new StringValue();
			}
			StringValue.MergeFrom(other.StringValue);
			break;
		case ValueOneofCase.VectorStringValue:
			if (VectorStringValue == null)
			{
				VectorStringValue = new VectorStringValue();
			}
			VectorStringValue.MergeFrom(other.VectorStringValue);
			break;
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
				StateOwnType = (UserStateOwnType)input.ReadEnum();
				break;
			case 16u:
				StateDataType = (UserStateDataType)input.ReadEnum();
				break;
			case 26u:
			{
				Int64Value int64Value = new Int64Value();
				if (valueCase_ == ValueOneofCase.Int64Value)
				{
					int64Value.MergeFrom(Int64Value);
				}
				input.ReadMessage(int64Value);
				Int64Value = int64Value;
				break;
			}
			case 34u:
			{
				StringValue stringValue = new StringValue();
				if (valueCase_ == ValueOneofCase.StringValue)
				{
					stringValue.MergeFrom(StringValue);
				}
				input.ReadMessage(stringValue);
				StringValue = stringValue;
				break;
			}
			case 42u:
			{
				VectorStringValue vectorStringValue = new VectorStringValue();
				if (valueCase_ == ValueOneofCase.VectorStringValue)
				{
					vectorStringValue.MergeFrom(VectorStringValue);
				}
				input.ReadMessage(vectorStringValue);
				VectorStringValue = vectorStringValue;
				break;
			}
			}
		}
	}
}
