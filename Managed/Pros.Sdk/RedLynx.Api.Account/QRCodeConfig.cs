using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public sealed class QRCodeConfig : IMessage<QRCodeConfig>, IMessage, IEquatable<QRCodeConfig>, IDeepCloneable<QRCodeConfig>, IBufferMessage
{
	private static readonly MessageParser<QRCodeConfig> _parser = new MessageParser<QRCodeConfig>(() => new QRCodeConfig());

	private UnknownFieldSet _unknownFields;

	public const int SystemModeFieldNumber = 1;

	private QRCodeSystemMode systemMode_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<QRCodeConfig> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => AccountContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRCodeSystemMode SystemMode
	{
		get
		{
			return systemMode_;
		}
		set
		{
			systemMode_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRCodeConfig()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRCodeConfig(QRCodeConfig other)
		: this()
	{
		systemMode_ = other.systemMode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRCodeConfig Clone()
	{
		return new QRCodeConfig(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as QRCodeConfig);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(QRCodeConfig other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SystemMode != other.SystemMode)
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
		if (SystemMode != QRCodeSystemMode.Normal)
		{
			num ^= SystemMode.GetHashCode();
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
		if (SystemMode != QRCodeSystemMode.Normal)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)SystemMode);
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
		if (SystemMode != QRCodeSystemMode.Normal)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SystemMode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(QRCodeConfig other)
	{
		if (other != null)
		{
			if (other.SystemMode != QRCodeSystemMode.Normal)
			{
				SystemMode = other.SystemMode;
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
			if (num != 8)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				SystemMode = (QRCodeSystemMode)input.ReadEnum();
			}
		}
	}
}
