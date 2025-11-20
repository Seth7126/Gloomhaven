using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Telemetry;

public sealed class TelemetryContext : IMessage<TelemetryContext>, IMessage, IEquatable<TelemetryContext>, IDeepCloneable<TelemetryContext>, IBufferMessage
{
	private static readonly MessageParser<TelemetryContext> _parser = new MessageParser<TelemetryContext>(() => new TelemetryContext());

	private UnknownFieldSet _unknownFields;

	public const int PropertyNameFieldNumber = 1;

	private string propertyName_ = "";

	public const int PropertyValueFieldNumber = 2;

	private string propertyValue_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TelemetryContext> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
	public TelemetryContext()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryContext(TelemetryContext other)
		: this()
	{
		propertyName_ = other.propertyName_;
		propertyValue_ = other.propertyValue_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryContext Clone()
	{
		return new TelemetryContext(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TelemetryContext);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TelemetryContext other)
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
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
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
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TelemetryContext other)
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
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
