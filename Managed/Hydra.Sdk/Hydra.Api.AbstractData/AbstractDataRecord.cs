using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.AbstractData;

public sealed class AbstractDataRecord : IMessage<AbstractDataRecord>, IMessage, IEquatable<AbstractDataRecord>, IDeepCloneable<AbstractDataRecord>, IBufferMessage
{
	private static readonly MessageParser<AbstractDataRecord> _parser = new MessageParser<AbstractDataRecord>(() => new AbstractDataRecord());

	private UnknownFieldSet _unknownFields;

	public const int LayoutFieldNumber = 1;

	private long layout_;

	public const int VersionFieldNumber = 2;

	private long version_;

	public const int DataFieldNumber = 3;

	private ByteString data_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<AbstractDataRecord> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Layout
	{
		get
		{
			return layout_;
		}
		set
		{
			layout_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public AbstractDataRecord()
	{
	}

	[DebuggerNonUserCode]
	public AbstractDataRecord(AbstractDataRecord other)
		: this()
	{
		layout_ = other.layout_;
		version_ = other.version_;
		data_ = other.data_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public AbstractDataRecord Clone()
	{
		return new AbstractDataRecord(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as AbstractDataRecord);
	}

	[DebuggerNonUserCode]
	public bool Equals(AbstractDataRecord other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Layout != other.Layout)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Layout != 0)
		{
			num ^= Layout.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
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
		if (Layout != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Layout);
		}
		if (Version != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt64(Version);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteBytes(Data);
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
		if (Layout != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Layout);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Version);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(AbstractDataRecord other)
	{
		if (other != null)
		{
			if (other.Layout != 0)
			{
				Layout = other.Layout;
			}
			if (other.Version != 0)
			{
				Version = other.Version;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
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
				Layout = input.ReadInt64();
				break;
			case 16u:
				Version = input.ReadInt64();
				break;
			case 26u:
				Data = input.ReadBytes();
				break;
			}
		}
	}
}
