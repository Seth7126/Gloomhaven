using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public sealed class FactsContext : IMessage<FactsContext>, IMessage, IEquatable<FactsContext>, IDeepCloneable<FactsContext>, IBufferMessage
{
	private static readonly MessageParser<FactsContext> _parser = new MessageParser<FactsContext>(() => new FactsContext());

	private UnknownFieldSet _unknownFields;

	public const int PropertyNameFieldNumber = 1;

	private string propertyName_ = "";

	public const int PropertyValueFieldNumber = 2;

	private string propertyValue_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<FactsContext> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => FactsContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string PropertyName
	{
		get
		{
			return propertyName_;
		}
		set
		{
			propertyName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string PropertyValue
	{
		get
		{
			return propertyValue_;
		}
		set
		{
			propertyValue_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public FactsContext()
	{
	}

	[DebuggerNonUserCode]
	public FactsContext(FactsContext other)
		: this()
	{
		propertyName_ = other.propertyName_;
		propertyValue_ = other.propertyValue_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public FactsContext Clone()
	{
		return new FactsContext(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as FactsContext);
	}

	[DebuggerNonUserCode]
	public bool Equals(FactsContext other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PropertyName != other.PropertyName)
		{
			return false;
		}
		if (PropertyValue != other.PropertyValue)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PropertyName.Length != 0)
		{
			num ^= PropertyName.GetHashCode();
		}
		if (PropertyValue.Length != 0)
		{
			num ^= PropertyValue.GetHashCode();
		}
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
		if (PropertyName.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PropertyName);
		}
		if (PropertyValue.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(PropertyValue);
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
		if (PropertyName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PropertyName);
		}
		if (PropertyValue.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PropertyValue);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(FactsContext other)
	{
		if (other != null)
		{
			if (other.PropertyName.Length != 0)
			{
				PropertyName = other.PropertyName;
			}
			if (other.PropertyValue.Length != 0)
			{
				PropertyValue = other.PropertyValue;
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
			case 10u:
				PropertyName = input.ReadString();
				break;
			case 18u:
				PropertyValue = input.ReadString();
				break;
			}
		}
	}
}
