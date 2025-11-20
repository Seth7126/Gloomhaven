using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.AbstractData;

public sealed class AbstractDataContainerData : IMessage<AbstractDataContainerData>, IMessage, IEquatable<AbstractDataContainerData>, IDeepCloneable<AbstractDataContainerData>, IBufferMessage
{
	private static readonly MessageParser<AbstractDataContainerData> _parser = new MessageParser<AbstractDataContainerData>(() => new AbstractDataContainerData());

	private UnknownFieldSet _unknownFields;

	public const int ContainerNameFieldNumber = 1;

	private string containerName_ = "";

	public const int RecordFieldNumber = 2;

	private AbstractDataRecord record_;

	[DebuggerNonUserCode]
	public static MessageParser<AbstractDataContainerData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ContainerName
	{
		get
		{
			return containerName_;
		}
		set
		{
			containerName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public AbstractDataRecord Record
	{
		get
		{
			return record_;
		}
		set
		{
			record_ = value;
		}
	}

	[DebuggerNonUserCode]
	public AbstractDataContainerData()
	{
	}

	[DebuggerNonUserCode]
	public AbstractDataContainerData(AbstractDataContainerData other)
		: this()
	{
		containerName_ = other.containerName_;
		record_ = ((other.record_ != null) ? other.record_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public AbstractDataContainerData Clone()
	{
		return new AbstractDataContainerData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as AbstractDataContainerData);
	}

	[DebuggerNonUserCode]
	public bool Equals(AbstractDataContainerData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ContainerName != other.ContainerName)
		{
			return false;
		}
		if (!object.Equals(Record, other.Record))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ContainerName.Length != 0)
		{
			num ^= ContainerName.GetHashCode();
		}
		if (record_ != null)
		{
			num ^= Record.GetHashCode();
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
		if (ContainerName.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ContainerName);
		}
		if (record_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Record);
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
		if (ContainerName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ContainerName);
		}
		if (record_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Record);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(AbstractDataContainerData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ContainerName.Length != 0)
		{
			ContainerName = other.ContainerName;
		}
		if (other.record_ != null)
		{
			if (record_ == null)
			{
				Record = new AbstractDataRecord();
			}
			Record.MergeFrom(other.Record);
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
			case 10u:
				ContainerName = input.ReadString();
				break;
			case 18u:
				if (record_ == null)
				{
					Record = new AbstractDataRecord();
				}
				input.ReadMessage(Record);
				break;
			}
		}
	}
}
